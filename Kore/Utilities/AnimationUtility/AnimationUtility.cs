using System;
using UnityEngine;
using System.Collections;
public static class AnimationUtility
{
    private static Animator animator;

    public static void Initialize(Animator anim)
    {
        // get the Animator component
        animator = anim;
    }

    public static void PlayAnimation(string clipName, bool playDirectly = true, Action onStart = null, Action onEnd = null)
    {
        // get the animation clip by name
        AnimationClip clip = Array.Find(animator.runtimeAnimatorController.animationClips, c => c.name == clipName);

        // play the animation directly or wait for any other animation that is being played
        if (playDirectly)
        {
            animator.Play(clipName);
        }
        else
        {
            animator.CrossFade(clipName, 0.1f);
        }

        // invoke the onStart callback if it exists
        if (onStart != null)
        {
            onStart.Invoke();
        }

        // invoke the onEnd callback after the animation has finished playing
        MonoBehaviourHelper.Instance.StartCoroutine(DelayedCallback(clip.length, onEnd));
    }

    public static float GetAnimationLength(string clipName)
    {
        // get the animation clip by name and return its length
        AnimationClip clip = Array.Find(animator.runtimeAnimatorController.animationClips, c => c.name == clipName);
        return clip.length;
    }

    public static void ResetAnimation(string clipName)
    {
        // set the animation clip to its starting state
        animator.Play(clipName, 0, 0);
        animator.Update(0);
    }

    public static void PauseAnimation(string clipName, Action onPause = null)
    {
        // pause the animation
        animator.speed = 0;

        // invoke the onPause callback if it exists
        if (onPause != null)
        {
            onPause.Invoke();
        }
    }

    public static void ResumeAnimation(string clipName, Action onResume = null)
    {
        // resume the animation
        animator.speed = 1;

        // invoke the onResume callback if it exists
        if (onResume != null)
        {
            onResume.Invoke();
        }
    }

    private static IEnumerator DelayedCallback(float delay, Action callback)
    {
        // wait for the specified delay and then invoke the callback if it exists
        yield return new WaitForSeconds(delay);
        if (callback != null)
        {
            callback.Invoke();
        }
    }

    private class MonoBehaviourHelper : MonoBehaviour
    {
        public static readonly MonoBehaviourHelper Instance = new MonoBehaviourHelper();

        private MonoBehaviourHelper()
        {
        }
    }
}
