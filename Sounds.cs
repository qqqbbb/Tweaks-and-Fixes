using HarmonyLib;
using System;
using System.Collections.Generic;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    internal class Sounds
    {

        [HarmonyPatch(typeof(FMOD_CustomEmitter), "Play")]
        class FMOD_CustomEmitter_Play_Patch
        {
            public static bool Prefix(FMOD_CustomEmitter __instance)
            {
                //AddDebug(" FMOD_CustomEmitter Play " + __instance.asset.name);
                return Main.gameLoaded;
            }
        }

        //[HarmonyPatch(typeof(FMOD_CustomLoopingEmitter), "Play")]
        class FMOD_CustomLoopingEmitter_Play_Patch
        {
            public static bool Prefix(FMOD_CustomLoopingEmitter __instance)
            {
                //AddDebug(" FMOD_CustomEmitter Play " + __instance.asset.name);
                return Main.gameLoaded;
            }
        }

        [HarmonyPatch(typeof(VoiceNotification), "Play", new Type[1] { typeof(object[]) })]
        class VoiceNotification_Play_Patch
        {
            public static bool Prefix(VoiceNotification __instance)
            {
                //AddDebug("VoiceNotification Play");
                return Main.gameLoaded;
            }
        }

        [HarmonyPatch(typeof(SoundQueue), "PlayQueued", new Type[2] { typeof(string), typeof(string) })]
        class SoundQueue_PlayQueued_Patch
        {
            public static bool Prefix(SoundQueue __instance, string sound)
            {
                //AddDebug(" PlayQueued  " + sound);
                return Main.gameLoaded;
            }
        }



    }
}
