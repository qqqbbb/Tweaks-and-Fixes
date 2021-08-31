using System;
using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;
using static ErrorMessage;

namespace Tweaks_Fixes
{
	class Sealed_Door_Patch
	{
		[HarmonyPatch(typeof(StarshipDoor), "OnHandHover")]
		class StarshipDoor_OnHandHover_Patch
		{
			private static bool Prefix(StarshipDoor __instance)
			{
                //AddDebug("doorOpenMethod " + __instance.doorOpenMethod);
				LaserCutObject laserCutObject = __instance.GetComponent<LaserCutObject>();
                if (laserCutObject != null && laserCutObject.isCutOpen)
				{
					//if (Input.GetKey(KeyCode.Z))
					//{ 
					//	laserCutObject.cutObject.SetActive(true);
					//	AddDebug("cutObject.SetActive ");
					//}
					return false;
				}
				return true;
			}
		}


	}
}
