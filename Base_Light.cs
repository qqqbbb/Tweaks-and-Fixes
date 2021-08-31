using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using static ErrorMessage;

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
                    //AddDebug(" subLightsOn " + subRoot.subLightsOn);
                }
            }

            static void Postfix(SubRoot __instance)
            {
                Light[] lights = __instance.GetAllComponentsInChildren<Light>();
                if (__instance.isBase)
                {
                    __instance.subLightsOn = !Main.config.baseLightOff;
                    bases.Add(__instance);
                    //AddDebug(" SubRoot awake base lights " + lights.Length);
                }
                //else
                //    AddDebug(" SubRoot awake sub lights " + lights.Length);
            }
        }


    }
}
