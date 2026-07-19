using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UWE;

using static ErrorMessage;

namespace Tweaks_Fixes
{
    internal class Command_Console
    {
        [HarmonyPatch(typeof(DevConsole))]
        class DevConsole_
        {
            [HarmonyPrefix, HarmonyPatch("ShouldEnableDevTools")]
            public static bool AwakePrefix(DevConsole __instance, ref bool __result)
            {
                if (ConfigToEdit.pressShiftToOpenConsole.Value)
                    return true;

                __result = true;
                return false;
            }
            [HarmonyPrefix, HarmonyPatch("OnConsoleCommand_commands")]
            public static bool OnConsoleCommand_commandsPrefix(DevConsole __instance)
            {
                AddDebug($"Logged {DevConsole.commands.Keys.Count} console commands");
                Main.logger.LogInfo("Console commands:");
                foreach (string command in DevConsole.commands.Keys)
                {
                    Main.logger.LogInfo(command);
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(FreecamController), "Update")]
        class FreecamController_Update_patch
        {
            public static void Prefix(FreecamController __instance)
            {
                if (__instance.GetActive() == false)
                    return;

                Vector2 scrollValue = Mouse.current.scroll.ReadValue();
                if (scrollValue.y != 0)
                {
                    if (scrollValue.y > 0)
                        __instance.speed *= 1.5f;
                    else
                        __instance.speed *= .375f;

                    if (__instance.speed < 1)
                        __instance.speed = 1;
                }
            }
        }

        [HarmonyPatch(typeof(SpawnConsoleCommand), "OnConsoleCommand_spawn")]
        class SpawnConsoleCommand_OnConsoleCommand_spawn_patch
        {
            public static void Postfix(SpawnConsoleCommand __instance, NotificationCenter.Notification n)
            {
                if (n != null && n.data != null && n.data.Count > 0)
                {
                    string s = (string)n.data[0];
                    if (TechTypeExtensions.FromString(s, out _, true) == false)
                        CoroutineHost.StartCoroutine(SpawnAsync(s));
                }
            }

            private static IEnumerator SpawnAsync(string classID)
            {
                //AddDebug("SpawnAsync " + classID);
                GameObject prefab;
                IPrefabRequest request = PrefabDatabase.GetPrefabAsync(classID);
                yield return request;
                if (request.TryGetPrefab(out prefab) == false)
                {
                    AddDebug("no prefab for classID " + classID);
                    yield break;
                }
                float maxDist = 10f;
                GameObject obj = Utils.CreatePrefab(prefab, maxDist, true);
                LargeWorldEntity.Register(obj);
                obj.SendMessage("StartConstruction", SendMessageOptions.DontRequireReceiver);
            }
        }

        [HarmonyPatch(typeof(Player), "OnConsoleCommand_fastswim")]
        class Player_OnConsoleCommand_fastswim_patch
        {
            public static void Postfix(Player __instance)
            {
                if (__instance.debugFastSwimAllowed)
                {
                    ErrorMessage.AddDebug($"Hold {Language.main.Get("OptionSprint")} button to swim faster");
                }
            }
        }


    }
}
