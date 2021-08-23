using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using UnityEditor.Animations;

public static class ReverseAnimationContext
{
    [MenuItem("Assets/Create Reversed Clip", false, 14)]
    private static void ReverseClips()
    {
        List<AnimatorController> animConts = new List<AnimatorController>();
        var animators = Object.FindObjectsOfType<Animator>();
        AssetDatabase.FindAssets("t:AnimatorController");
        List<AnimationClip> clips = GetSelectedClips();
        if (clips != null && clips.Count > 0)
        {
            foreach (AnimationClip clip in clips)
            {
                ReverseClip(clip, animators);
            }
            Debug.Log("All selected clips reversed");
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
    private static void ReverseClip(AnimationClip clip, Animator[] animators)
    {
        AnimationClip originalClip = clip;
        string directoryPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(clip));
        string fileName = Path.GetFileName(AssetDatabase.GetAssetPath(clip));
        string fileExtension = Path.GetExtension(AssetDatabase.GetAssetPath(clip));
        fileName = fileName.Split('.')[0];
        string copiedFilePath = directoryPath + Path.DirectorySeparatorChar + fileName + "_Reversed" + fileExtension;

        AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(clip), copiedFilePath);

        clip = (AnimationClip)AssetDatabase.LoadAssetAtPath(copiedFilePath, typeof(AnimationClip));

        if (clip == null)
            return;

        float clipLength = clip.length;
        var curves = AnimationUtility.GetCurveBindings(clip);

        foreach (EditorCurveBinding binding in curves)
        {
            var animCurve = AnimationUtility.GetEditorCurve(clip, binding);
            var keys = animCurve.keys;
            int keyCount = keys.Length;

            for (int i = 0; i < keyCount; i++)
            {
                Keyframe K = keys[i];
                K.time = clipLength - K.time;
                var tmp = -K.inTangent;
                K.inTangent = -K.outTangent;
                K.outTangent = tmp;
                keys[i] = K;
            }

            animCurve.keys = keys;
            clip.SetCurve(binding.path, binding.type, binding.propertyName, animCurve);
        }

        var events = AnimationUtility.GetAnimationEvents(clip);
        if (events.Length > 0)
        {
            for (int i = 0; i < events.Length; i++)
            {
                events[i].time = clipLength - events[i].time;
            }
            AnimationUtility.SetAnimationEvents(clip, events);
        }

        var objectReferenceCurves = AnimationUtility.GetObjectReferenceCurveBindings(clip);
        foreach (EditorCurveBinding binding in objectReferenceCurves)
        {
            ObjectReferenceKeyframe[] objectReferenceKeyframes = AnimationUtility.GetObjectReferenceCurve(clip, binding);
            for (int i = 0; i < objectReferenceKeyframes.Length; i++)
            {
                ObjectReferenceKeyframe K = objectReferenceKeyframes[i];
                //K.time = clipLength - K.time - (1 / clip.frameRate); //Reversed sprite clips may be offset by 1 frame time
                K.time = clipLength - K.time;
                objectReferenceKeyframes[i] = K;
            }
            AnimationUtility.SetObjectReferenceCurve(clip, binding, objectReferenceKeyframes);
        }

        foreach (Animator anim in animators)
        {
            AnimationClip[] clips = AnimationUtility.GetAnimationClips(anim.gameObject);
            bool foundClip = false;
            foreach (AnimationClip c in clips)
            {
                if (c == originalClip)
                {
                    foundClip = true;
                    break;
                }
            }
            if (foundClip)
            {
                Debug.Log("Found the animator containing the original clip that was reversed, adding new clip to its state machine...");
                AnimatorController controller = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.AnimatorController>(UnityEditor.AssetDatabase.GetAssetPath(anim.runtimeAnimatorController));
                AnimatorStateMachine asm = controller.layers[0].stateMachine;
                AnimatorState animState = asm.AddState(clip.name);
                animState.motion = clip;
                break;
            }
        }
    }

    [MenuItem("Assets/Create Reversed Clip", true)]
    static bool ReverseClipValidation()
    {
        return Selection.activeObject && Selection.activeObject.GetType() == typeof(AnimationClip);
    }

    public static AnimationClip GetSelectedClip()
    {
        var clips = Selection.GetFiltered(typeof(AnimationClip), SelectionMode.Assets);
        if (clips.Length > 0)
        {
            return clips[0] as AnimationClip;
        }
        return null;
    }

}