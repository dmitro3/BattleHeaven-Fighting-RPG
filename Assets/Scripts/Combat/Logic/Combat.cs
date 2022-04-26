using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Specialized;
using System;

public class Combat : MonoBehaviour
{
    // Data Objects
    public static Fighter player;
    public static Fighter bot;
    public int botElo;

    // GameObjects
    public static GameObject playerGameObject;
    public GameObject playerWrapper;
    public static GameObject botGameObject;
    public Canvas results;
    public SpriteRenderer arena;
    public GameObject combatUI;
    public GameObject combatLoadingScreenUI;
    public GameObject combatLoadingScreenSprites;

    // Script references
    public static Movement movementScript;
    public static FightersUIData fightersUIDataScript;
    SkillsLogicInCombat skillsLogicScript;
    LoadingScreen loadingScreen;

    // Positions data
    static Vector3 playerStartingPosition = new Vector3(-6, -0.7f, 0);
    static Vector3 botStartingPosition = new Vector3(6, -0.7f, 0);
    public const float DefaultDistanceFromEachotherOnAttack = 2.3f;

    // Game status data
    public static bool isGameOver;
    List<Fighter> fightersOrderOfAttack = new List<Fighter> { };
    public static float playerMaxHp;
    public static float botMaxHp;

    private void Awake()
    {
        isGameOver = false;

        FindGameObjects();
        GetComponentReferences();
        GenerateBotData();

        // LoadingScreen
        loadingScreen.SetPlayerLoadingScreenData(player);
        loadingScreen.DisplayLoaderForEnemy();
        ToggleLoadingScreenVisibility(true);

        //Load everything needed for the combat
        GenerateSkillsFixturesForPlayer();
        BoostStatsBasedOnPassiveSkills();
        SetVisibilityOfGameObjects();
        SetFighterPositions();
        SetOrderOfAttacks();
        GetRandomArena();
        FighterSkin.SetFightersSkin(player, bot);
        FighterAnimations.ResetToDefaultAnimation(player);
        fightersUIDataScript.SetFightersUIInfo(player, bot, botElo);
        SetMaxHpValues();
    }

    IEnumerator Start()
    {
        yield return new WaitForSeconds(0);
        loadingScreen.SetBotLoadingScreenData(bot);
        yield return new WaitForSeconds(0);
        ToggleLoadingScreenVisibility(false);
        StartCoroutine(InitiateCombat());
    }

    private void BoostStatsBasedOnPassiveSkills()
    {
        ModifyStatsAffectedByPassiveSkills(player);
        ModifyStatsAffectedByPassiveSkills(bot);
    }

    private void ModifyStatsAffectedByPassiveSkills(Fighter fighter)
    {
        if (fighter.HasSkill(SkillNames.DangerousStrength)) fighter.damage = GetModifiedStat(fighter.damage);
        if (fighter.HasSkill(SkillNames.Heavyweight)) fighter.hp = GetModifiedStat(fighter.hp);
        if (fighter.HasSkill(SkillNames.Lightning)) fighter.speed = GetModifiedStat(fighter.speed);
        if (fighter.HasSkill(SkillNames.Persistant)) fighter.repeatAttackChance = GetModifiedStat(fighter.repeatAttackChance);
        if (fighter.HasSkill(SkillNames.FelineAgility)) fighter.dodgeChance = GetModifiedStat(fighter.dodgeChance);
        if (fighter.HasSkill(SkillNames.CriticalBleeding)) fighter.criticalChance = GetModifiedStat(fighter.criticalChance);
        if (fighter.HasSkill(SkillNames.Reversal)) fighter.reversalChance = GetModifiedStat(fighter.reversalChance);
        if (fighter.HasSkill(SkillNames.CounterAttack)) fighter.counterAttackChance = GetModifiedStat(fighter.counterAttackChance);
    }

    //Boosts stats based on passives at the start of the combat. Resets stats at the end of the combat.
    public static Func<float, float> GetModifiedStat = stat =>
        isGameOver ? stat /= SkillsLogicInCombat.PassiveSkillsModifier : stat *= SkillsLogicInCombat.PassiveSkillsModifier;

    private void GetComponentReferences()
    {
        // From the current gameobject (this) access the movement component which is a script.
        movementScript = this.GetComponent<Movement>();
        fightersUIDataScript = this.GetComponent<FightersUIData>();
        skillsLogicScript = this.GetComponent<SkillsLogicInCombat>();
        loadingScreen = this.GetComponent<LoadingScreen>();
        player = playerGameObject.GetComponent<Fighter>();
        bot = botGameObject.GetComponent<Fighter>();
    }

    private void ToggleLoadingScreenVisibility(bool displayLoadingScreen)
    {
        combatLoadingScreenUI.SetActive(displayLoadingScreen);
        combatLoadingScreenSprites.SetActive(displayLoadingScreen);
    }

    private void SetMaxHpValues()
    {
        playerMaxHp = player.hp;
        botMaxHp = bot.hp;
    }
    private void GetRandomArena()
    {
        Sprite[] arenas = Resources.LoadAll<Sprite>("Arenas/");
        int chosenArena = UnityEngine.Random.Range(0, arenas.Length);
        arena.sprite = arenas[chosenArena];
    }

    private void SetVisibilityOfGameObjects()
    {
        playerGameObject.SetActive(true);
    }
    private void FindGameObjects()
    {
        playerWrapper = GameObject.Find("FighterWrapper");
        playerGameObject = playerWrapper.transform.Find("Fighter").gameObject;
        botGameObject = GameObject.FindGameObjectWithTag("FighterBot");
        results = GameObject.FindGameObjectWithTag("Results").GetComponent<Canvas>();
        arena = GameObject.FindGameObjectWithTag("Arena").GetComponent<SpriteRenderer>();
        combatUI = GameObject.FindGameObjectWithTag("CombatUI");
        combatLoadingScreenUI = GameObject.FindGameObjectWithTag("CombatLoadingScreenUI");
        combatLoadingScreenSprites = GameObject.FindGameObjectWithTag("CombatLoadingScreenSprites");
    }

    private void SetFighterPositions()
    {
        //Set GameObjects
        playerGameObject.transform.position = playerStartingPosition;
        botGameObject.transform.position = botStartingPosition;

        //Set Objects
        player.initialPosition = playerStartingPosition;
        bot.initialPosition = botStartingPosition;

        SetFightersDestinationPositions(DefaultDistanceFromEachotherOnAttack);
    }

    public void ResetFightersDestinationPosition()
    {
        SetFightersDestinationPositions(DefaultDistanceFromEachotherOnAttack);
    }

    public void SetFightersDestinationPositions(float distanceAwayFromEachOtherOnAttack)
    {
        Vector3 playerDestinationPosition = botStartingPosition;
        Vector3 botDestinationPosition = playerStartingPosition;

        playerDestinationPosition.x -= distanceAwayFromEachOtherOnAttack;
        player.destinationPosition = playerDestinationPosition;

        botDestinationPosition.x += distanceAwayFromEachOtherOnAttack;
        bot.destinationPosition = botDestinationPosition;
    }

    IEnumerator InitiateCombat()
    {
        Fighter firstAttacker = fightersOrderOfAttack[0];
        Fighter secondAttacker = fightersOrderOfAttack[1];

        //1 loop = 1 turn (both players attacking)
        while (!isGameOver)
        {
            // The StartTurn method should handle all the actions of a player for that turn. E.G. Move, Attack, Throw skill....
            yield return StartCoroutine(StartTurn(firstAttacker, secondAttacker));
            if (isGameOver) break;
            yield return StartCoroutine(StartTurn(secondAttacker, firstAttacker));
        }
        StartPostGameActions();
    }

    private void GenerateBotData()
    {
        string botName = MatchMaking.FetchBotRandomName();
        int botLevel = MatchMaking.GenerateBotLevel(player.level);
        botElo = MatchMaking.GenerateBotElo(User.Instance.elo);

        List<Skill> botSkills = new List<Skill>();

        //ADD ALL SKILLS
        foreach (OrderedDictionary skill in SkillCollection.skills)
        {
            Skill skillInstance = new Skill(skill["name"].ToString(), skill["description"].ToString(),
                skill["skillRarity"].ToString(), skill["category"].ToString(), skill["icon"].ToString());

            botSkills.Add(skillInstance);
        }

        SpeciesNames randomSpecies = GetRandomSpecies();

        Dictionary<string, float> botStats = GenerateBotRandomStats(randomSpecies);

        bot.FighterConstructor(botName, botStats["hp"], botStats["damage"], botStats["speed"],
            randomSpecies.ToString(), randomSpecies.ToString(), botLevel, 0, botSkills);

        //FIXME: We should remove the skin concept from the fighters and use the species name for the skin.
    }

    //TODO: Remove this on production
    private void GenerateSkillsFixturesForPlayer()
    {
        //GIVE ALL SKILLS TO THE PLAYER
        foreach (OrderedDictionary skill in SkillCollection.skills)
        {
            Skill skillInstance = new Skill(skill["name"].ToString(), skill["description"].ToString(),
                skill["SkillRarity"].ToString(), skill["category"].ToString(), skill["icon"].ToString());

            player.skills.Add(skillInstance);
        }
    }

    private Dictionary<string, float> GenerateBotRandomStats(SpeciesNames randomSpecies)
    {
        float hp = Species.defaultStats[randomSpecies]["hp"] + (Species.statsPerLevel[randomSpecies]["hp"] * player.level);
        float damage = Species.defaultStats[randomSpecies]["damage"] + (Species.statsPerLevel[randomSpecies]["damage"] * player.level);
        float speed = Species.defaultStats[randomSpecies]["speed"] + (Species.statsPerLevel[randomSpecies]["speed"] * player.level);

        return new Dictionary<string, float>
        {
            {"hp", hp},
            {"damage", damage},
            {"speed", speed},
        };
    }
    private SpeciesNames GetRandomSpecies()
    {
        System.Random random = new System.Random();
        Array species = Enum.GetValues(typeof(SpeciesNames));
        return (SpeciesNames)species.GetValue(random.Next(species.Length));
    }

    IEnumerator StartTurn(Fighter attacker, Fighter defender)
    {
        if (WillUseSkillThisTurn())
        {
            yield return StartCoroutine(UseRandomSkill(attacker, defender));
            yield break;
        }
        yield return skillsLogicScript.AttackWithoutSkills(attacker, defender);
    }

    IEnumerator UseRandomSkill(Fighter attacker, Fighter defender)
    {
        //TODO FUTURE REFACTOR: Each skill should have each own class with its own skill implementation. (methods, attributes, etc...)
        // Then we can instantiate a random class here to use a random SUPER skill this turn
        int numberOfSkills = 4;

        int randomNumber = UnityEngine.Random.Range(0, numberOfSkills) + 1;
        switch (randomNumber)
        {
            case 1:
                yield return skillsLogicScript.JumpStrike(attacker, defender);
                attacker.removeUsedSkill(SkillNames.JumpStrike);
                break;
            case 2:
                yield return skillsLogicScript.CosmicKicks(attacker, defender);
                attacker.removeUsedSkill(SkillNames.CosmicKicks);
                break;
            case 3:
                yield return skillsLogicScript.ShurikenFury(attacker, defender);
                attacker.removeUsedSkill(SkillNames.ShurikenFury);
                break;
            case 4:
                yield return skillsLogicScript.LowBlow(attacker, defender);
                attacker.removeUsedSkill(SkillNames.LowBlow);
                break;
        }
    }

    private bool WillUseSkillThisTurn()
    {
        int probabilityOfUsingSkillEachTurn = 50;
        return Probabilities.IsHappening(probabilityOfUsingSkillEachTurn);
    }

    public IEnumerator MoveForwardHandler(Fighter attacker)
    {
        FighterAnimations.ChangeAnimation(attacker, FighterAnimations.AnimationNames.RUN);
        yield return StartCoroutine(movementScript.MoveForward(attacker, attacker.destinationPosition));
    }

    public IEnumerator MoveBackHandler(Fighter attacker)
    {
        FighterAnimations.ChangeAnimation(attacker, FighterAnimations.AnimationNames.RUN);
        FighterSkin.SwitchFighterOrientation(attacker.GetComponent<SpriteRenderer>());
        yield return StartCoroutine(movementScript.MoveBack(attacker, attacker.initialPosition));
        FighterSkin.SwitchFighterOrientation(attacker.GetComponent<SpriteRenderer>());
        FighterAnimations.ChangeAnimation(attacker, FighterAnimations.AnimationNames.IDLE);
    }

    //The attack order is determined by the Initiator skill. If no players have it it is determined by the speed.
    private void SetOrderOfAttacks()
    {
        if (player.HasSkill(SkillNames.Initiator))
        {
            fightersOrderOfAttack.Add(player);
            fightersOrderOfAttack.Add(bot);
            return;
        }

        // Creates a dictionary with the Fighter class objects sorted by their speeds to get the order of attack.
        // Higher speeds will get sorted first
        OrderedDictionary fighterDictWithSpeed = new OrderedDictionary
        {
            {player, player.speed},
            {bot, bot.speed},
        };

        var fighterDictSortedBySpeed = fighterDictWithSpeed.Cast<DictionaryEntry>()
                       .OrderByDescending(r => r.Value)
                       .ToDictionary(c => c.Key, d => d.Value);

        foreach (var fighter in fighterDictSortedBySpeed)
        {
            fightersOrderOfAttack.Add((Fighter)fighter.Key);
        }
    }

    private void StartPostGameActions()
    {
        //Reset stats boosted by Passive skills
        ModifyStatsAffectedByPassiveSkills(player);

        bool isPlayerWinner = PostGameActions.HasPlayerWon(player);
        int eloChange = MatchMaking.CalculateEloChange(User.Instance.elo, botElo, isPlayerWinner);
        int playerUpdatedExperience = player.experiencePoints + Levels.GetXpGain(isPlayerWinner);
        bool isLevelUp = Levels.IsLevelUp(player.level, playerUpdatedExperience);
        int goldReward = PostGameActions.GoldReward(isPlayerWinner);
        int gemsReward = PostGameActions.GemsReward();

        //PlayerData
        PostGameActions.SetElo(eloChange);
        PostGameActions.SetWinLoseCounter(isPlayerWinner);
        PostGameActions.SetExperience(player, isPlayerWinner);
        if (isLevelUp) PostGameActions.SetLevelUpSideEffects(player);
        EnergyManager.SubtractOneEnergyPoint();

        //Rewards
        PostGameActions.SetCurrencies(goldReward, gemsReward);

        //UI
        fightersUIDataScript.SetResultsBanner(isPlayerWinner);
        fightersUIDataScript.SetResultsEloChange(eloChange);
        fightersUIDataScript.SetResultsLevel(player.level, player.experiencePoints);
        fightersUIDataScript.SetResultsExpGainText(isPlayerWinner);
        fightersUIDataScript.ShowLevelUpIcon(isLevelUp);
        fightersUIDataScript.ShowRewards(goldReward, gemsReward, isLevelUp);
        fightersUIDataScript.EnableResults(results);

        //Save
        PostGameActions.ResetPlayerHp(playerMaxHp);
        PostGameActions.Save(player);
    }
}
