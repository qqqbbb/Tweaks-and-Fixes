using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    [HarmonyPatch(typeof(Constructor))]
    class Constructor_
    {
        public static bool fixed_;

        public IEnumerator FixConstructor()
        {
            CoroutineTask<GameObject> request = CraftData.GetPrefabForTechTypeAsync(TechType.Constructor);
            yield return request;
            GameObject prefab = request.GetResult();
            ImmuneToPropulsioncannon itpc = prefab.GetComponent<ImmuneToPropulsioncannon>();
            UnityEngine.Object.Destroy(itpc);
            Util.AttachPing(prefab, PingType.Signal, "Constructor");
            Transform packUpTr = prefab.transform.Find("unequipped/deployed/PickupableTrigger");
            UnityEngine.Object.Destroy(packUpTr.gameObject);
            UnderWaterTracker underWaterTracker = prefab.GetComponent<UnderWaterTracker>();
            UnityEngine.Object.Destroy(underWaterTracker);
            fixed_ = true;
        }

        //[HarmonyPostfix]
        //[HarmonyPatch("Deploy")]
        static void DeployPostfix(Constructor __instance, bool value)
        {
            AddDebug("Constructor Deploy " + value);
            if (value)
            {
                WorldForces wf = __instance.GetComponent<WorldForces>();
                if (wf)
                    wf.underwaterGravity = -3f;
            }
        }

        //[HarmonyPostfix, HarmonyPatch("Update")]
        static void UpdatePostfix(Constructor __instance)
        {
            if (Player.main.transform.position.y > 1f && __instance.climbTrigger.activeSelf)
                __instance.climbTrigger.SetActive(false);
        }

        [HarmonyPrefix, HarmonyPatch("Update")]
        private static bool Prefix(Constructor __instance)
        {
            if (__instance.deployed == false)
                return false;

            bool playerClose = __instance.playerDistanceTracker.distanceToPlayer < 3f;
            bool onSurface = __instance.transform.position.y >= __instance.worldForces.waterDepth - 0.5f;
            bool deployed = __instance.timeDeployed + 3f < Time.time;
            bool canUseBots = (playerClose && onSurface && deployed) || __instance.buildTarget != null;

            for (int i = 0; i < __instance.buildBots.Count; i++)
            {
                __instance.buildBots[i].GetComponent<ConstructorBuildBot>().launch = canUseBots || __instance.buildBots[i].transform.localPosition != Vector3.zero;
            }
            if (__instance.building && __instance.buildTarget == null)
            {
                __instance.RecallBuildBots();
            }
            if (!__instance.climbTrigger.activeSelf && __instance.deployed && !__instance.IsDeployAnimationInProgress && onSurface && Player.main.transform.position.y < 1)
                __instance.climbTrigger.SetActive(true);
            else if (__instance.climbTrigger.activeSelf && Player.main.transform.position.y > 1)
                __instance.climbTrigger.SetActive(false);

            return false;
        }
    }

    [HarmonyPatch(typeof(CinematicModeTrigger))]
    class CinematicModeTrigger_Patch
    {
        [HarmonyPostfix, HarmonyPatch("OnHandHover")]
        static void OnHandHoverPostfix(CinematicModeTrigger __instance, GUIHand hand)
        {
            Transform parent = __instance.transform.parent;
            if (parent == null || parent.parent == null || parent.parent.parent == null)
                return;

            if (parent.parent.parent.name == "Constructor(Clone)")
            {
                Constructor constructor = parent.parent.parent.GetComponent<Constructor>();
                //AddDebug(" building " + constructor.building);
                HandReticle.main.SetText(HandReticle.TextType.HandSubscript, string.Empty, false);
                HandReticle.main.SetIcon(HandReticle.IconType.Hand);
                if (constructor.building)
                    return;

                HandReticle.main.SetText(HandReticle.TextType.Hand, UI_Patches.constructorString, false);
                if (GameInput.GetButtonDown(GameInput.Button.RightHand))
                {
                    if (constructor.pickupable)
                        constructor.pickupable.OnHandClick(hand);
                }
            }
        }
    }

    [HarmonyPatch(typeof(ConstructorInput))]
    class ConstructorInput_Patch
    {
        [HarmonyPostfix, HarmonyPatch("ReturnValidCraftingPosition")]
        static void ReturnValidCraftingPositionPostfix(ConstructorInput __instance, ref bool __result)
        {
            if (ConfigToEdit.checkWaterDepthWhenConstructingCyclops.Value == false)
                __result = true;
        }
    }


}
