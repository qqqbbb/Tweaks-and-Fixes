using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UWE;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    internal class Flashes
    {
        [HarmonyPatch(typeof(VFXParticlesFlashesSlowdown), "Awake")]
        class VFXParticlesFlashesSlowdown_Patch
        {
            static bool Prefix(VFXParticlesFlashesSlowdown __instance)
            {
                return !ConfigToEdit.disableSlowFlashes.Value;
            }
        }

        [HarmonyPatch(typeof(VFXShockerElec), "GetRingDurationMult")]
        class VFXShockerElec_Patch
        {
            static void Postfix(VFXShockerElec __instance, ref float __result)
            {
                if (MiscSettings.flashes == false && ConfigToEdit.disableSlowFlashes.Value)
                    __result = 1f;
            }
        }

        [HarmonyPatch(typeof(VFXShockerElecLine), "UpdateSpeed")]
        class VFXShockerElecLine_Patch
        {
            static void Postfix(VFXShockerElecLine __instance)
            {
                if (MiscSettings.flashes == false && ConfigToEdit.disableSlowFlashes.Value)
                {
                    __instance.textureSpeedToken.RestoreScale();
                    __instance.currentNoiseSpeed = __instance.noiseSpeed;
                }
            }
        }

        [HarmonyPatch(typeof(VFXPrecursorTeleporter), "UpdateSpeed")]
        class VFXPrecursorTeleporter_Patch
        {
            static void Postfix(VFXPrecursorTeleporter __instance)
            {
                if (MiscSettings.flashes == false && ConfigToEdit.disableSlowFlashes.Value)
                {
                    __instance.SetSpeedScale(1f);
                }
            }
        }

        [HarmonyPatch(typeof(TeleportScreenFX), "UpdateSpeed")]
        class TeleportScreenFX_Patch
        {
            static void Postfix(TeleportScreenFX __instance)
            {
                if (MiscSettings.flashes == false && ConfigToEdit.disableSlowFlashes.Value)
                {
                    __instance.textureSpeedToken.RestoreScale();
                }
            }
        }

        //[HarmonyPatch(typeof(WarpScreenFX), "UpdateSpeed")]
        class WarpScreenFX_Patch
        {
            static void Postfix(WarpScreenFX __instance)
            {
                if (MiscSettings.flashes == false && ConfigToEdit.disableSlowFlashes.Value)
                {
                    __instance.textureSpeedToken.RestoreScale();
                }
            }
        }

        //[HarmonyPatch(typeof(VFXWarpTube), "UpdateSpeed")]
        class VFXWarpTube_Patch
        {
            static void Postfix(VFXWarpTube __instance)
            {
                if (MiscSettings.flashes == false && ConfigToEdit.disableSlowFlashes.Value)
                {
                    __instance.textureSpeedTokens.RestoreScale();
                }
            }
        }


    }
}
