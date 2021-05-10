using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FMOD;
using FMOD.Studio;
using FMODUnity;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    class Footsteps_Patch
    {
        [HarmonyPatch(typeof(FootstepSounds), "OnStep")]
        class FootstepSounds_OnStep_Patch
        {
            static bool Prefix(FootstepSounds __instance, Transform xform)
            {
                if (!Main.config.fixFootstepSound)
                    return true;

                if (!__instance.ShouldPlayStepSounds())
                    return false;

                //FMODAsset test = new FMODAsset() { path = "event:/player/footstep_rocket" };
                FMODAsset asset;
                if (__instance.groundMoveable.GetGroundSurfaceType() == VFXSurfaceTypes.metal || Player.main.IsInside() || Player.main.GetBiomeString() == FootstepSounds.crashedShip)
                    asset = __instance.metalSound;
                else if (Player.main.precursorOutOfWater)
                    asset = __instance.precursorInteriorSound;
                else
                    asset = __instance.landSound;

                //asset = test;
                EventInstance evt = FMODUWE.GetEvent(asset);

                if (!evt.isValid())
                    return false;
                if (__instance.fmodIndexSpeed < 0)
                    __instance.fmodIndexSpeed = FMODUWE.GetEventInstanceParameterIndex(evt, "speed");
                //AddDebug("Velocity.magnitude " + __instance.groundMoveable.GetVelocity().magnitude);
                ATTRIBUTES_3D attributes = xform.To3DAttributes();
                evt.set3DAttributes(attributes);
                float velMag = __instance.groundMoveable.GetVelocity().magnitude;

                evt.setParameterValueByIndex(__instance.fmodIndexSpeed, velMag);
                evt.setVolume(1f);
                if (asset != __instance.landSound)
                {
                    //AddDebug("FIX");
                    evt.setParameterValueByIndex(__instance.fmodIndexSpeed, 7f);
                    if (velMag < 6f)
                        evt.setVolume(.3f);
                }
                evt.start();
                evt.release();
                return false;
            }
        }



    }
}
