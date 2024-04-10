using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_and_Fixes
{
    [HarmonyPatch(typeof(Constructor))]
    class Constructor_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch("OnEnable")]
        static void OnEnablePostfix(Constructor __instance)
        {
            //AddDebug("Constructor OnEnable");
            ImmuneToPropulsioncannon itpc = __instance.GetComponent<ImmuneToPropulsioncannon>();
            //itpc.enabled = false;
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
        }

        [HarmonyPostfix]
        [HarmonyPatch("Deploy")]
        static void Deployostfix(Constructor __instance, bool value)
        {
            //AddDebug("Constructor Deploy " + value);
            if (value)
            {
                WorldForces wf = __instance.GetComponent<WorldForces>();
                if (wf)
                    wf.underwaterGravity = -3f;
            }
        }
    }


}
