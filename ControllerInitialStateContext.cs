using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

public static class ControllerInitialState
{
    [MenuItem("Assets/Set Initial State", false, 16)]
    private static void SetInitialStates()
    {
        List<AnimatorController> conts = GetSelectedControllers();
        if (conts != null && conts.Count > 0)
        {
            foreach (AnimatorController con in conts)
            {
                SetInitialState(con);
            }
        }
    }

    public static List<AnimatorController> GetSelectedControllers()
    {
        var conts = Selection.GetFiltered(typeof(AnimatorController), SelectionMode.Assets);
        List<AnimatorController> animConts = new List<AnimatorController>();
        if (conts.Length > 0)
        {
            foreach (var cont in conts)
            {
                animConts.Add(cont as AnimatorController);
            }
            return animConts;
        }
        return null;
    }

    private static void SetInitialState(AnimatorController cont)
    {
        AnimatorStateMachine asm = cont.layers[0].stateMachine;
        AnimatorState newState = asm.AddState("Default State");
        asm.defaultState = newState;
    }

    [MenuItem("Assets/Set Initial State", true)]
    static bool SetInitialStateValidation()
    {
        return Selection.activeObject && Selection.activeObject.GetType() == typeof(AnimatorController);
    }

}
