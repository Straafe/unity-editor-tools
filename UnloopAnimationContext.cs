using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public static class UnloopAnimationContext
{
    [MenuItem("Assets/Unloop Animation", false, 15)]
    private static void UnloopClips()
    {
        List<AnimationClip> clips = GetSelectedClips();
        if (clips != null && clips.Count > 0)
        {
            foreach (AnimationClip clip in clips)
            {
                UnLoopClip(clip);
            }
        }
    }
    public static List<AnimationClip> GetSelectedClips()
    {
        var clips = Selection.GetFiltered(typeof(AnimationClip), SelectionMode.Assets);
        List<AnimationClip> animClips = new List<AnimationClip>();
        if (clips.Length > 0)
        {
            foreach (var clip in clips)
            {
                animClips.Add(clip as AnimationClip);
            }
            return animClips;
        }
        return null;
    }
    private static void UnLoopClip(AnimationClip clip)
    {
        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = false;
        AnimationUtility.SetAnimationClipSettings(clip, settings);
    }
    [MenuItem("Assets/Unloop Animation", true)]
    static bool UnLoopValidation()
    {
        return Selection.activeObject && Selection.activeObject.GetType() == typeof(AnimationClip);
    }

}
