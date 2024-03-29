using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using UnityEngine;
using UnityEngine.EventSystems;
using static ErrorMessage;
using static GameInput;

namespace Tweaks_Fixes
{
    internal class GameInput_Patch
    {
        [HarmonyPatch(typeof(GameInput), "SetBindingInternal", new Type[] { typeof(Device), typeof(Button), typeof(BindingSet), typeof(string) })]
        class GameInput_SetBindingInternal_Patch
        {
            static bool Prefix(GameInput __instance, Device device, Button button, BindingSet bindingSet, ref string input)
            {
                if (string.IsNullOrEmpty(input))
                    return false;

                if (!ConfigToEdit.swapControllerTriggers.Value || device != Device.Controller)
                    return true;

                if (input == "ControllerRightTrigger")
                    input = "ControllerLeftTrigger";
                else if (input == "ControllerLeftTrigger")
                    input = "ControllerRightTrigger";
                //int inputIndex = GameInput.GetInputIndex(input);
                //if (inputIndex == -1)
                //    Debug.LogErrorFormat("GameInput: Input {0} not found", input);

                //Main.logger.LogMessage(" SetBindingInternal string device " + device + " button " + button + " input " + inputIndex);
                //GameInput.SetBindingInternal(device, button, bindingSet, inputIndex);
                return true;
            }
        }

        [HarmonyPatch(typeof(uGUI), "GetDisplayTextForBinding")]
        class uGUI_GetDisplayTextForBinding_Patch
        {
            static void Prefix(uGUI __instance, ref string bindingName)
            {
                //Main.logger.LogMessage("uGUI GetDisplayTextForBinding " + bindingName);
                if (!ConfigToEdit.swapControllerTriggers.Value)
                    return;

                if (bindingName == "ControllerRightTrigger")
                    bindingName = "ControllerLeftTrigger";
                else if (bindingName == "ControllerLeftTrigger")
                    bindingName = "ControllerRightTrigger";
            }
        }

        [HarmonyPatch(typeof(uGUI_Binding), "RefreshValue")]
        class uGUI_Binding_UpdateState_Patch
        {
            static bool Prefix(uGUI_Binding __instance)
            {
                if (!ConfigToEdit.swapControllerTriggers.Value)
                    return true;

                __instance.value = GetBinding(__instance.device, __instance.action, __instance.bindingSet);
                if (__instance.active || __instance.value == null)
                    __instance.currentText.text = string.Empty;
                else
                {
                    //AddDebug("RefreshValue value " + __instance.value);
                    if (__instance.value == "ControllerRightTrigger")
                        __instance.value = "ControllerLeftTrigger";
                    else if (__instance.value == "ControllerLeftTrigger")
                        __instance.value = "ControllerRightTrigger";

                    __instance.currentText.text = uGUI.GetDisplayTextForBinding(GetInputName(__instance.value));
                    //AddDebug("RefreshValue currentText " + __instance.currentText.text);
                }
                __instance.UpdateState();
                return false;
            }
        }


    }
}
