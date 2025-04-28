using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace Tweaks_Fixes
{
    internal class Main_Menu
    {
        [HarmonyPatch(typeof(MainMenuRightSide), "Start")]
        public static class MainMenuRightSide_Start_Patch
        {
            public static void Postfix(MainMenuRightSide __instance)
            {
                if (MiscSettings.newsEnabled)
                    return;

                Transform t = __instance.transform.Find("Home/EmailBox");
                if (t)
                    UnityEngine.Object.Destroy(t.gameObject);

            }
        }

        [HarmonyPatch(typeof(ConsoleMainMenuNewsController), "DisableNews")]
        public static class MConsoleMainMenuNewsController_DisableNews_Patch
        {
            public static void Postfix(ConsoleMainMenuNewsController __instance)
            {
                if (MiscSettings.newsEnabled)
                    return;

                Transform t = __instance.transform.parent.parent.Find("EmailBox");
                if (t)
                    UnityEngine.Object.Destroy(t.gameObject);
            }
        }


    }
}
