using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    internal class Cyclops_Constructable_Collision
    {
        static Dictionary<SubControl, Collider[]> subColliders = new Dictionary<SubControl, Collider[]>();

        public static void CleanUp()
        {
            subColliders.Clear();
        }

        public static void AddCyclopsCollisionExclusion(GameObject go, SubControl subControl = null)
        {
            if (subControl == null)
            {
                if (Player.main.currentSub == null || Player.main.currentSub.isCyclops == false)
                    return;

                subControl = Player.main.currentSub.GetComponent<SubControl>();
            }
            Collider[] myCols = go.GetAllComponentsInChildren<Collider>();
            if (myCols == null || myCols.Length == 0)
                return;

            if (subColliders.ContainsKey(subControl) == false)
            {
                Transform t = subControl.transform.Find("CyclopsCollision");
                if (t == null)
                    return;

                subColliders[subControl] = t.GetComponentsInChildren<Collider>();
            }
            //AddDebug($"AddCyclopsCollisionExclusion {go.name} {myCols.Length}");
            foreach (Collider c in subColliders[subControl])
            {
                if (c == null)
                    continue;
                foreach (Collider myCol in myCols)
                    Physics.IgnoreCollision(myCol, c);
            }
        }

        [HarmonyPatch(typeof(Constructable), "Start")]
        class Constructable_Start_Patch
        {
            public static void Postfix(Constructable __instance)
            {
                SubControl subControl = __instance.GetComponentInParent<SubControl>();
                if (subControl == null || subControl.name == "__LIGHTMAPPED_PREFAB__")
                    return;

                AddCyclopsCollisionExclusion(__instance.gameObject, subControl);
            }
        }

        [HarmonyPatch(typeof(Plantable), "Spawn")]
        public class Plantable_Spawn_Patch
        {
            public static void Postfix(Plantable __instance, Transform parent, bool isIndoor, GameObject __result)
            {
                SubControl subControl = __instance.GetComponentInParent<SubControl>();
                if (subControl == null || subControl.name == "__LIGHTMAPPED_PREFAB__")
                    return;

                AddCyclopsCollisionExclusion(__instance.gameObject, subControl);
            }
        }

        [HarmonyPatch(typeof(GrownPlant), "Awake")]
        public class GrownPlant_Awake_Patch
        {
            public static void Postfix(GrownPlant __instance)
            {
                SubControl subControl = __instance.GetComponentInParent<SubControl>();
                if (subControl == null || subControl.name == "__LIGHTMAPPED_PREFAB__")
                    return;

                AddCyclopsCollisionExclusion(__instance.gameObject, subControl);
            }
        }

        //[HarmonyPatch(typeof(PlaceTool))]
        public class PlaceTool_Patch_
        {
            //[HarmonyPostfix, HarmonyPatch("CreateGhostModel")]
            static void CreateGhostModelPostfix(PlaceTool __instance)
            {
                //AddDebug("CreateGhostModel " + __instance.name);
                //AddCyclopsCollisionExclusion(__instance.gameObject);

            }
            //[HarmonyPostfix, HarmonyPatch("OnPlace")]
            static void DestroyGhostModelPostfix(PlaceTool __instance)
            {
                //AddDebug("PlaceTool OnPlace " + __instance.name);
                //pickupable.droppedEvent.AddHandler(base.gameObject, OnDropped);
            }
        }



    }
}
