using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;

namespace Tweaks_Fixes
{
    public static class Base_Light
    {
        [HarmonyPatch(typeof(SubRoot), "Awake")]
        public static class SubRoot_Awake_Patch
        {
            public static HashSet<SubRoot> bases = new HashSet<SubRoot>();

            public static void UpdateBaseLight()
            {
                foreach (SubRoot subRoot in bases)
                {
                    subRoot.subLightsOn = !Main.config.baseLightOff;
                    //ErrorMessage.AddDebug(" subLightsOn " + subRoot.subLightsOn);
                }
            }

            static void Postfix(SubRoot __instance)
            {
                if (__instance.isBase)
                {
                    __instance.subLightsOn = !Main.config.baseLightOff;
                    bases.Add(__instance);
                    //ErrorMessage.AddDebug(" SubRoot awake isBase " + __instance.isBase);
                }

            }
        }


    }
}
