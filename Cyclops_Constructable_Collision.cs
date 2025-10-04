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
        public static HashSet<Collider> collidersInSub = new HashSet<Collider>();

        public static void AddCyclopsCollisionExclusion(GameObject go)
        {
            Collider[] myCols = go.GetAllComponentsInChildren<Collider>();
            AddDebug($"AddCyclopsCollisionExclusion {go.name} {myCols.Length}");
            foreach (Collider c in collidersInSub)
            {
                if (c == null)
                    continue;
                foreach (Collider myCol in myCols)
                    Physics.IgnoreCollision(myCol, c);
            }
            collidersInSub.AddRange(myCols);
            //foreach (Collider c in myCols)
            //AddDebug("add collider to collidersInSub");
        }

        //[HarmonyPatch(typeof(Constructable), "Start")]
        class Constructable_Start_Patch
        {
            public static void Postfix(Constructable __instance)
            {
                if (__instance.GetComponentInParent<SubControl>())
                {
                    //AddDebug("Constructable Start");
                    AddCyclopsCollisionExclusion(__instance.gameObject);
                }
            }
        }

        //[HarmonyPatch(typeof(Plantable), "Spawn")]
        public class Plantable_Spawn_Patch
        {
            public static void Postfix(Plantable __instance, Transform parent, bool isIndoor, GameObject __result)
            {
                if (__result && __instance.GetComponentInParent<SubControl>())
                    AddCyclopsCollisionExclusion(__result);
            }
        }

        //[HarmonyPatch(typeof(GrownPlant), "Awake")]
        public class GrownPlant_Awake_Patch
        {
            public static void Postfix(GrownPlant __instance)
            {
                if (__instance.GetComponentInParent<SubControl>())
                    AddCyclopsCollisionExclusion(__instance.gameObject);
            }
        }

        //[HarmonyPatch(typeof(PlaceTool))]
        public class PlaceTool_Patch_
        {
            //[HarmonyPostfix, HarmonyPatch("CreateGhostModel")]
            static void CreateGhostModelPostfix(PlaceTool __instance)
            {
                AddDebug("CreateGhostModel " + __instance.name);
                //AddCyclopsCollisionExclusion(__instance.gameObject);

            }
            //[HarmonyPostfix, HarmonyPatch("OnPlace")]
            static void DestroyGhostModelPostfix(PlaceTool __instance)
            {
                AddDebug("PlaceTool OnPlace " + __instance.name);
                //pickupable.droppedEvent.AddHandler(base.gameObject, OnDropped);
            }
        }



    }
}
