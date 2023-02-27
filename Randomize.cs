
using HarmonyLib;
using System.Reflection;
using System;
using SMLHelper.V2.Handlers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Text;
using static ErrorMessage;

namespace Tweaks_Fixes
{ 
    class Randomize
    {
        //[HarmonyPatch(typeof(KeypadDoorConsole), "NumberButtonPress")]
        class KeypadDoorConsole_NumberButtonPress_Patch
        {
            public static void Prefix(KeypadDoorConsole __instance, int index)
            {
                AddDebug(" " + index);
                AddDebug("accessCode " + __instance.accessCode);
                //return false;
            }
        }

        //[HarmonyPatch(typeof(Story.StoryGoal), "Execute")]
        class StoryGoal_Execute_Patch
        {
            public static void Postfix(Story.StoryGoal __instance, string key, Story.GoalType goalType)
            {
                AddDebug("StoryGoal " + key);
                AddDebug("goalType " + goalType);
                //return false;
            }
        }

        //[HarmonyPatch(typeof(PDAEncyclopedia), "AddAndPlaySound")]
        class PDAEncyclopedia_AddAndPlaySound_Patch
        {
            public static void Postfix(string key, PDAEncyclopedia.EntryData __result)
            {
                AddDebug("AddAndPlaySound " + key);
                AddDebug("EntryData " + __result.key);
                //return false;
            }
        }


    }
}
