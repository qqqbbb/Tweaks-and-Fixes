using HarmonyLib;
using System;
using System.Collections.Generic;
using static ErrorMessage;
using static KnownTech;

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

        [HarmonyPatch(typeof(uGUI_PopupNotification), "OnAnalyze")]
        class uGUI_PopupNotification_OnAnalyze_Patch
        {
            static bool Prefix(uGUI_PopupNotification __instance, AnalysisTech analysis, bool verbose)
            {
                //AddDebug("uGUI_PopupNotification OnAnalyze " + analysis.techType + " " + verbose);
                if (ConfigToEdit.silentBlueprintUnlockNotification.Value == false)
                    return true;

                if (verbose)
                {
                    uGUI_PopupNotification.Entry entry = new uGUI_PopupNotification.Entry
                    {
                        duration = __instance.defaultDuration,
                        skin = PopupNotificationSkin.Unlock,
                        title = Language.main.Get(analysis.unlockMessage),
                        text = Language.main.Get(analysis.techType.AsString()),
                        sprite = analysis.unlockPopup,
                        //sound = analysis.unlockSound
                    };
                    __instance.Enqueue(entry);
                }
                return false;
            }
        }


        //[HarmonyPatch(typeof(uGUI_PopupNotification), "Enqueue")]
        class uGUI_PopupNotification_Enqueue_Patch
        {
            static void Prefix(uGUI_PopupNotification __instance, ref uGUI_PopupNotification.Entry entry)
            {
                AddDebug("uGUI_PopupNotification Enqueue " + entry.id);
                if (ConfigToEdit.silentBlueprintUnlockNotification.Value)
                    entry.sound = null;
            }
        }

        //[HarmonyPatch(typeof(uGUI_PopupNotification), "OnResourceDiscovered")]
        class uGUI_PopupNotification_OnResourceDiscovered_Patch
        {
            static void Prefix(uGUI_PopupNotification __instance, TechType techType)
            {
                AddDebug("uGUI_PopupNotification OnResourceDiscovered " + techType);
            }
        }


    }
}
