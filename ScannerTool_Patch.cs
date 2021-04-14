using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tweaks_Fixes
{
    [HarmonyPatch(typeof(ScannerTool))]
    [HarmonyPatch("Update")]
    class ScannerTool_Update_Patch
	{// SHOW power when equipped
		private static bool Prefix(ScannerTool __instance)
		{
			//PlayerTool playerTool = 
			//bool isDrawn = (bool)PlayerTool_get_isDrawn.Invoke(__instance, new object[] { });
			if (__instance.isDrawn)
			{
				//float idleTimer = (float)ScannerTool_idleTimer.GetValue(__instance);
				//ErrorMessage.AddDebug("useText1 " + HandReticle.main.useText1);
				//ErrorMessage.AddDebug("useText2 " + HandReticle.main.useText2);
				if (__instance.idleTimer > 0f)
				{
					__instance.idleTimer = Mathf.Max(0f, __instance.idleTimer - Time.deltaTime);
					//string buttonFormat = LanguageCache.GetButtonFormat("ScannerSelfScanFormat", GameInput.Button.AltTool);
     //               HandReticle.main.SetUseTextRaw(buttonFormat, null);
				}
			}
			return false;
		}
	}
}
