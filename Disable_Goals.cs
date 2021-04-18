﻿using HarmonyLib;

namespace Tweaks_Fixes
{
    class Disable_Goals
    {
        [HarmonyPatch(typeof(PlayerWorldArrows), "CreateWorldArrows")]
        internal class PlayerWorldArrows_CreateWorldArrows_Patch
        {
            internal static bool Prefix(PlayerWorldArrows __instance)
            {
                //ErrorMessage.AddDebug("CreateWorldArrows");
                return !Main.config.disableGoals;
            }
        }
    }
}
