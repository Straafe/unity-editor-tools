using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Animations;

public static class ReverseAnimationContext
{
    [MenuItem("Assets/Create Reversed Clip", false, 14)]
    private static void ReverseClips()
    {
        var animators = Object.FindObjectsOfType<Animator>();
        AssetDatabase.FindAssets("t:AnimatorController");
        List<AnimationClip> clips = GetSelectedClips();

        if (clips is not { Count: > 0 })
            return;

        foreach (AnimationClip clip in clips)
        {
            ReverseClip(clip, animators);
        }

        Debug.Log("All selected clips reversed");
    }

    private static List<AnimationClip> GetSelectedClips()
    {
        var clips = Selection.GetFiltered(typeof(AnimationClip), SelectionMode.Assets);

        if (clips.Length <= 0)
            return null;

        return clips.Select(clip => clip as AnimationClip).ToList();
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
                Keyframe kf = keys[i];
                kf.time = clipLength - kf.time;
                var tmp = -kf.inTangent;
                kf.inTangent = -kf.outTangent;
                kf.outTangent = tmp;
                keys[i] = kf;
            }

            animCurve.keys = keys;
            clip.SetCurve(binding.path, binding.type, binding.propertyName, animCurve);
        }

        var events = AnimationUtility.GetAnimationEvents(clip);
        if (events.Length > 0)
        {
            foreach (var e in events)
            {
                e.time = clipLength - e.time;
            }

            AnimationUtility.SetAnimationEvents(clip, events);
        }

        var objectReferenceCurves = AnimationUtility.GetObjectReferenceCurveBindings(clip);
        foreach (EditorCurveBinding binding in objectReferenceCurves)
        {
            ObjectReferenceKeyframe[] objectReferenceKeyframes =
                AnimationUtility.GetObjectReferenceCurve(clip, binding);
            for (int i = 0; i < objectReferenceKeyframes.Length; i++)
            {
                ObjectReferenceKeyframe kf = objectReferenceKeyframes[i];
                //K.time = clipLength - K.time - (1 / clip.frameRate); //Reversed sprite clips may be offset by 1 frame time
                kf.time = clipLength - kf.time;
                objectReferenceKeyframes[i] = kf;
            }

            AnimationUtility.SetObjectReferenceCurve(clip, binding, objectReferenceKeyframes);
        }

        foreach (Animator anim in animators)
        {
            AnimationClip[] clips = AnimationUtility.GetAnimationClips(anim.gameObject);

            if (clips.All(c => c != originalClip))
                continue;

            Debug.Log("Found the animator containing the original clip that was reversed, adding new clip to its state machine...");
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(AssetDatabase.GetAssetPath(anim.runtimeAnimatorController));
            AnimatorStateMachine asm = controller.layers[0].stateMachine;
            AnimatorState animState = asm.AddState(clip.name);
            animState.motion = clip;
            break;
        }
    }

    [MenuItem("Assets/Create Reversed Clip", true)]
    private static bool ReverseClipValidation()
    {
        return Selection.activeObject && Selection.activeObject is AnimationClip;
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
