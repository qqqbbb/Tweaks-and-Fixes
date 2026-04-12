using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Profiling;
using UWE;
using static GameInput;

namespace Tweaks_Fixes
{
    internal class Input_
    {
        [HarmonyPatch(typeof(MainGameController), "Update")]
        class MainGameController_Update_Patch
        {
            static bool Prefix(MainGameController __instance)
            { // pressing PDA + reload does not open debug window
                if (uGUI.main == null || GameApplication.isQuitting)
                {
                    return false;
                }
                if (GC.CollectionCount(0) != __instance.lastFrameGCCount)
                    __instance.NotifyGarbageCollected();

                __instance.UpdateAutoGarbageCollection();
                AddressablesUtility.Update();
                __instance.lastFrameGCCount = GC.CollectionCount(0);
                if (UnityEngine.Debug.isDebugBuild && Input.GetKeyDown(KeyCode.F5) && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
                {
                    if (!Profiler.enabled)
                    {
                        Profiler.logFile = "profiling-" + Time.frameCount + ".log";
                        Profiler.enableBinaryLog = true;
                        Profiler.enabled = true;
                        UnityEngine.Debug.Log("Started profiling, writing to " + Profiler.logFile);
                    }
                    else
                    {
                        Profiler.enabled = false;
                        Profiler.enableBinaryLog = false;
                        Profiler.logFile = null;
                        UnityEngine.Debug.Log("Stopped profiling");
                    }
                }
                if (Input.GetKeyDown(KeyCode.F1))
                {
                    TerrainDebugGUI[] array = UnityEngine.Object.FindObjectsOfType<TerrainDebugGUI>();
                    foreach (TerrainDebugGUI obj in array)
                    {
                        obj.enabled = !obj.enabled;
                    }
                }
                if (Input.GetKeyDown(KeyCode.F3))
                {
                    GraphicsDebugGUI[] array2 = UnityEngine.Object.FindObjectsOfType<GraphicsDebugGUI>();
                    foreach (GraphicsDebugGUI graphicsDebugGUI in array2)
                    {
                        if (graphicsDebugGUI != null)
                        {
                            graphicsDebugGUI.enabled = !graphicsDebugGUI.enabled;
                        }
                    }
                }
                if (!Cursor.visible && Cursor.lockState == CursorLockMode.None)
                {
                    Cursor.visible = true;
                }
                MiscSettings.Update();
                return false;
            }
        }

        [HarmonyPatch(typeof(GameInput))]
        internal class GameInput_
        {
            [HarmonyPatch("UpdateMove"), HarmonyPrefix]
            static bool UpdateMovePrefix()
            { // dont stop automove when moving sideways
                Vector2 moveVector2 = GetVector2(Button.Move);
                float y = 0f;
                y += (GetButtonHeld(Button.MoveUp) ? 1f : 0f);
                y -= (GetButtonHeld(Button.MoveDown) ? 1f : 0f);
                if (autoMove && moveVector2.y != 0)
                {
                    autoMove = false;
                }
                if (autoMove)
                    moveDirection.Set(moveVector2.x, y, 1f);
                else
                    moveDirection.Set(moveVector2.x, y, moveVector2.y);

                if (IsPrimaryDeviceGamepad())
                {
                    if (autoMove)
                        isRunningMoveThreshold = false;
                    else
                    {
                        isRunningMoveThreshold = moveDirection.sqrMagnitude > 0.8f;
                        if (!isRunningMoveThreshold)
                            moveDirection /= 0.9f;
                    }
                }
                if (runMode == RunModeOption.PressToToggle && GetButtonDown(Button.Sprint))
                    isRunning = !isRunning;

                return false;
            }
            //[HarmonyPatch("UpdateMove"), HarmonyTranspiler]
            static IEnumerable<CodeInstruction> UpdateMoveTranspiler(IEnumerable<CodeInstruction> instructions)
            {
                var codeMatcher = new CodeMatcher(instructions);

                // Try to find stsfld for autoMove regardless of what's being stored
                codeMatcher.MatchForward(false,
                    new CodeMatch(OpCodes.Stsfld, AccessTools.Field(typeof(GameInput), "autoMove"))
                )
                .ThrowIfInvalid("Could not find autoMove field store in UpdateMove");
                // Go back one instruction to find what's being stored
                codeMatcher.Advance(-1);
                // Replace the loading instruction (ldc.i4.0 or whatever) with your delegate
                codeMatcher.SetInstructionAndAdvance(Transpilers.EmitDelegate<Func<bool>>(GetAutoMove));

                return codeMatcher.InstructionEnumeration();
            }

            static bool GetAutoMove()
            {
                //AddDebug("GetAutoMove");
                return true;
            }
        }
    }
}
