using System;
using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;

namespace Tweaks_Fixes
{
	class Sealed_Door_Patch
	{
		[HarmonyPatch(typeof(StarshipDoor), "OnHandHover")]
		class StarshipDoor_OnHandHover_Patch
		{
			private static bool Prefix(StarshipDoor __instance)
			{
                //ErrorMessage.AddDebug("doorOpenMethod " + __instance.doorOpenMethod);
				LaserCutObject laserCutObject = __instance.GetComponent<LaserCutObject>();
                if (laserCutObject != null && laserCutObject.isCutOpen)
				{
					//if (Input.GetKey(KeyCode.Z))
					//{ 
					//	laserCutObject.cutObject.SetActive(true);
					//	ErrorMessage.AddDebug("cutObject.SetActive ");
					//}
					return false;
				}
				else
					return true;
			}
		}


	}
}
