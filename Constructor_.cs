using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    [HarmonyPatch(typeof(Constructor))]
    class Constructor_
    {
        [HarmonyPostfix]
        [HarmonyPatch("OnEnable")]
        static void OnEnablePostfix(Constructor __instance)
        {
            //AddDebug("Constructor OnEnable");
            ImmuneToPropulsioncannon itpc = __instance.GetComponent<ImmuneToPropulsioncannon>();
            if (itpc)
                UnityEngine.Object.Destroy(itpc);

            if (!__instance.deployed)
            { // vanilla underwaterGravity 4
                //AddDebug("Constructor OnEnable");
                WorldForces wf = __instance.GetComponent<WorldForces>();
                if (wf)
                    wf.underwaterGravity = 1f;
            }
            PingInstance pi = __instance.gameObject.EnsureComponent<PingInstance>();
            pi.pingType = PingType.Signal;
            pi.origin = __instance.transform;
            pi.SetLabel(Language.main.Get("Constructor"));
            Transform packUpTr = __instance.transform.Find("unequipped/deployed/PickupableTrigger");
            if (packUpTr)
                UnityEngine.Object.Destroy(packUpTr.gameObject);
        }

        [HarmonyPostfix]
        [HarmonyPatch("Deploy")]
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

        [HarmonyPostfix]
        [HarmonyPatch("Update")]
        static void UpdatePostfix(Constructor __instance)
        {
            if (Player.main.transform.position.y > 1f)
                __instance.climbTrigger.SetActive(false);
        }


    }

    [HarmonyPatch(typeof(CinematicModeTrigger))]
    class CinematicModeTrigger_Patch
    {
        public static HashSet<CinematicModeTrigger> cmtSet = new HashSet<CinematicModeTrigger>();

        [HarmonyPostfix]
        [HarmonyPatch("OnHandHover")]
        static void OnHandHoverPostfix(CinematicModeTrigger __instance, GUIHand hand)
        {
            Transform parent = __instance.transform.parent;
            if (parent == null || parent.parent == null || parent.parent.parent == null)
                return;

            if (cmtSet.Contains(__instance))
            {
                //AddDebug("CinematicModeTrigger OnHandHover");
                HandReticle.main.SetText(HandReticle.TextType.HandSubscript, string.Empty, false);
                HandReticle.main.SetIcon(HandReticle.IconType.Hand);
                HandReticle.main.SetText(HandReticle.TextType.Hand, UI_Patches.constructorString, false);
                if (GameInput.GetButtonDown(GameInput.Button.RightHand))
                {
                    Constructor constructor = parent.parent.parent.GetComponent<Constructor>();
                    if (constructor && constructor.pickupable)
                        constructor.pickupable.OnHandClick(hand);
                }
            }
            else if (parent.parent.parent.name == "Constructor(Clone)")
            {
                cmtSet.Add(__instance);
            }
        }
    }


}
