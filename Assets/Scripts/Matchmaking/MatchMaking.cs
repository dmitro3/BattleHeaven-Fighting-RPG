using System.Collections.Generic;
using System.Collections.Specialized;
using System;
using UnityEngine;

public static class MatchMaking
{
    private static int baseEloGain = 15;
    public static CupFighter bot; // use for cup mode

    public static void GenerateBotData(Fighter player, Fighter bot)
    {
        string botName = FetchBotRandomName();
        int botLevel = GenerateBotLevel(player.level);
        Combat.botElo = GenerateBotElo(User.Instance.elo);

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

        //FIXME v2: We should remove the skin concept from the fighters and use the species name for the skin.

        Debug.Log("hp: " + botStats["hp"] + " damage: " + botStats["damage"] + " speed: " + botStats["speed"]);
    }

    public static void GenerateCupBotData(Fighter player, Fighter bot)
    {
        CupFighter cupBot = GetCupBotData();

        string botName = cupBot.fighterName; 
        int botLevel = GenerateBotLevel(player.level);
        Combat.botElo = GenerateBotElo(User.Instance.elo);

        List<Skill> botSkills = new List<Skill>();

        //ADD ALL SKILLS
        foreach (OrderedDictionary skill in SkillCollection.skills)
        {
            Skill skillInstance = new Skill(skill["name"].ToString(), skill["description"].ToString(),
                skill["skillRarity"].ToString(), skill["category"].ToString(), skill["icon"].ToString());

            botSkills.Add(skillInstance);
        }

        SpeciesNames randomSpecies = (SpeciesNames)Enum.Parse(typeof(SpeciesNames), cupBot.species);

        Dictionary<string, float> botStats = GenerateBotRandomStats(randomSpecies);

        bot.FighterConstructor(botName, botStats["hp"], botStats["damage"], botStats["speed"],
            randomSpecies.ToString(), randomSpecies.ToString(), botLevel, 0, botSkills);
    }

    private static CupFighter GetCupBotData()
    {
        string cupBotId = "";
        int counter = 0;

        // player enemies will be on seed2, seed10, seed14
        if (Cup.Instance.round == CupDB.CupRounds.QUARTERS.ToString())
            cupBotId = Cup.Instance.cupInfo[CupDB.CupRounds.QUARTERS.ToString()]["1"]["2"];
        if (Cup.Instance.round == CupDB.CupRounds.SEMIS.ToString())
            cupBotId = Cup.Instance.cupInfo[CupDB.CupRounds.SEMIS.ToString()]["5"]["10"];
        if (Cup.Instance.round == CupDB.CupRounds.FINALS.ToString())
            cupBotId = Cup.Instance.cupInfo[CupDB.CupRounds.FINALS.ToString()]["7"]["14"];

        for (int i = 0; i < Cup.Instance.participants.Count; i++)
        {
            if (cupBotId == Cup.Instance.participants[counter].id)
                return Cup.Instance.participants[counter];
            counter++;
        }

        Debug.Log("Couldn't get fighter!");
        return new CupFighter("", "", "");
    }

    private static Dictionary<string, float> GenerateBotRandomStats(SpeciesNames randomSpecies)
    {
        float hp = Species.defaultStats[randomSpecies]["hp"] + (Species.statsPerLevel[randomSpecies]["hp"] * (Combat.player.level - 1));
        float damage = Species.defaultStats[randomSpecies]["damage"] + (Species.statsPerLevel[randomSpecies]["damage"] * (Combat.player.level - 1));
        float speed = Species.defaultStats[randomSpecies]["speed"] + (Species.statsPerLevel[randomSpecies]["speed"] * (Combat.player.level - 1));

        return new Dictionary<string, float>
        {
            {"hp", hp},
            {"damage", damage},
            {"speed", speed},
        };
    }
    private static SpeciesNames GetRandomSpecies()
    {
        System.Random random = new System.Random();
        Array species = Enum.GetValues(typeof(SpeciesNames));
        return (SpeciesNames)species.GetValue(random.Next(species.Length));
    }
    private static string FetchBotRandomName()
    {
        return RandomNameGenerator.GenerateRandomName();
    }

    private static int GenerateBotElo(int playerElo)
    {
        int botElo = UnityEngine.Random.Range(playerElo - 50, playerElo + 50);
        return botElo >= 0 ? botElo : 0;
    }
    private static int GenerateBotLevel(int playerLevel)
    {
        return playerLevel > 1 ? UnityEngine.Random.Range(playerLevel - 1, playerLevel + 2) : playerLevel;
    }

    public static int CalculateEloChange(int playerElo, int botElo, bool isPlayerWinner)
    {
        int eloDifference = botElo - playerElo;
        int eloPonderance = eloDifference / 10;
        int absoluteEloChange = baseEloGain + eloPonderance;
        int modifierToRankUpOverTime = 2;
        int eloChange = isPlayerWinner ? absoluteEloChange : -absoluteEloChange + modifierToRankUpOverTime;
        if (playerElo + eloChange < 0) return -playerElo;
        return eloChange;
    }
}