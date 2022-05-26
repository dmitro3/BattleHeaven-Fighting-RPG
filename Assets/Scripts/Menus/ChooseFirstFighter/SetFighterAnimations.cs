using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SetFighterAnimations : MonoBehaviour
{
    AnimationClip idleAnimation;
    AnimationClip runAnimation;
    public Animator fighterAnimator;

    private void Start()
    {
        string skinName = GetSkinNameDependingOnScene();
        fighterAnimator = GetComponent<Animator>();

        //TODO: This script only sets the idle animation. Change it to be more flexible
        idleAnimation = Resources.Load<AnimationClip>("Animations/Characters/" + skinName + "/01_idle");
        runAnimation = Resources.Load<AnimationClip>("Animations/Characters/" + skinName + "/02_run");
        SetAnimationClipToAnimator(fighterAnimator, idleAnimation);
    }

    private string GetSkinNameDependingOnScene()
    {
        string currrentScene = SceneManager.GetActiveScene().name;

        if (currrentScene == SceneNames.ChooseFirstFighter.ToString()) return GetComponent<FighterSkinData>().skinName;
        if (currrentScene == SceneNames.MainMenu.ToString()) return PlayerUtils.FindInactiveFighter().skin;
        if (currrentScene == SceneNames.Credits.ToString()) return PlayerUtils.FindInactiveFighter().skin;
        //Combat
        return tag == "LoadingScreenBot" ? Combat.bot.skin : Combat.player.skin;
    }

    private static void SetAnimationClipToAnimator(Animator animator, AnimationClip idleAnimation)
    {
        AnimatorOverrideController aoc = new AnimatorOverrideController(animator.runtimeAnimatorController);
        AnimationClip defaultIdleClip = aoc.animationClips[0];

        var anims = new List<KeyValuePair<AnimationClip, AnimationClip>>();
        anims.Add(new KeyValuePair<AnimationClip, AnimationClip>(defaultIdleClip, idleAnimation));
        aoc.ApplyOverrides(anims);
        animator.runtimeAnimatorController = aoc;
    }

    public void PlayIdleAnimation()
    {
        SetAnimationClipToAnimator(fighterAnimator, idleAnimation);
    }

    public void PlayRunAnimation()
    {
        SetAnimationClipToAnimator(fighterAnimator, runAnimation);
    }
}
