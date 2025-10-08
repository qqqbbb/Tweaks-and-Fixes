using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static ErrorMessage;


namespace Tweaks_Fixes
{
    [HarmonyPatch(typeof(Subtitles))]
    internal class Subtitles_
    {
        [HarmonyPrefix, HarmonyPatch("AddRawLong")]
        public static void Prefix(Subtitles __instance, StringBuilder text, ref float delay, float durationText, float durationSound)
        {
            delay += ConfigMenu.subtitlesDelay.Value;
            //AddDebug($"Subtitles AddRawLong delay {delay} ::: {text.ToString()}");
        }

        //[HarmonyPrefix, HarmonyPatch("AddInternal")]
        public static void AddInternalPrefix(Subtitles __instance, string key)
        {
            //AddDebug($"Subtitles AddInternal  {key} ");
        }


    }

}
