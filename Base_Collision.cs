using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    internal class Base_Collision
    {
        public static void FixWaterParkHatch(GameObject go)
        {
            //AddDebug(" FixWaterParkHatch " + go.name);
            Transform t = go.transform.Find("collisions");
            if (t == null)
                return;

            Collider[] colliders = t.GetComponents<Collider>();
            //AddDebug(" BaseWaterParkHatch colliders " + colliders.Length);
            for (int i = colliders.Length - 1; i >= 0; i--)
            {
                //AddDebug(" destroy BaseWaterParkHatch " + colliders[i].name);
                UnityEngine.Object.Destroy(colliders[i]);
            }
            BoxCollider[] boxColliders = t.GetComponentsInChildren<BoxCollider>();
            foreach (var c in boxColliders)
            {
                if (c.center.x < 0)
                { // fix outside collider so player not get stuck when using ladder
                    c.center = new Vector3(-0.02f, c.center.y, c.center.z);
                }
                else
                {// fix inside collider so player not prompted to exit upon entering 
                    c.center = new Vector3(0.15f, c.center.y, c.center.z);
                }
            }
        }

        internal static void RemoveHatchRailingCollision(GameObject go)
        {
            //AddDebug("FixBaseRoomHatch " + go.name);
            Transform t = go.transform.Find("collision/stairhandle");
            if (t == null)
                return;

            UnityEngine.Object.Destroy(t.gameObject);
        }

        public static void AddRampToHatch(GameObject go)
        {
            //AddDebug("AddRampToHatch");
            GameObject ramp = new GameObject("Ramp");
            Transform t = go.transform.GetChild(0);
            ramp.transform.SetParent(t);
            ramp.transform.localPosition = new Vector3(0.325f, -4.025f, 0);
            ramp.transform.localEulerAngles = new Vector3(0, 0, 325f);
            BoxCollider collider = ramp.AddComponent<BoxCollider>();
            collider.center = new Vector3(-6.38f, -1.1f, 0);
            collider.size = new Vector3(0.6f, 0.08f, 2.5f);
            //Testing.CreateDebugCollider(collider);
        }

        [HarmonyPatch(typeof(BaseDeconstructable), "Init")]
        class BaseDeconstructable_Init_Patch
        {
            static void Postfix(BaseDeconstructable __instance)
            {
                //AddDebug("BaseDeconstructable Init " + __instance.name);
                if (__instance.recipe == TechType.BaseLadder && __instance.name.EndsWith("LadderTop(Clone)"))
                {
                    FixTopLadderCollision(__instance.gameObject);
                    return;
                }
                else if (__instance.recipe == TechType.BaseHatch)
                {
                    if (__instance.name == "BaseLargeRoomHatch(Clone)")
                    {
                        AddRampToHatch(__instance.gameObject);
                    }
                    else if (__instance.name == "BaseWaterParkHatch(Clone)")
                    {
                        FixWaterParkHatch(__instance.gameObject);
                    }
                    else if (__instance.name == "BaseRoomHatch(Clone)")
                    {
                        RemoveHatchRailingCollision(__instance.gameObject);
                    }
                    else if (__instance.name.StartsWith("BaseLargeRoomWaterParkHatch"))
                    {
                        FixLargeWaterParkHatch(__instance.gameObject);
                    }
                }
                else if (__instance.recipe == TechType.None)
                {
                    if (__instance.name == "BaseRoomCorridorConnector(Clone)")
                        RemoveHatchRailingCollision(__instance.gameObject);
                }
            }

            private static void FixLargeWaterParkHatch(GameObject go)
            {
                Transform t = go.transform.Find("collisions");
                t.gameObject.SetActive(false);
                Transform collisions = go.transform.Find("BaseCorridorHatch/models/collisions");
                t = collisions.transform.Find("Sphere (1)");
                t.gameObject.SetActive(false);
                t = collisions.transform.Find("Sphere (2)");
                t.gameObject.SetActive(false);
                t = collisions.transform.Find("Cube (1)");
                BoxCollider collider = t.GetComponent<BoxCollider>();
                collider.size = new Vector3(collider.size.x, .5f, collider.size.z);
            }

            private static void FixTopLadderCollision(GameObject go)
            {
                Transform logic = go.transform.GetChild(0);
                BoxCollider collider = logic.GetComponent<BoxCollider>();
                if (collider)
                {
                    collider.center = new Vector3(collider.center.x, 0.36f, collider.center.z);
                    collider.size = new Vector3(collider.size.x, 0.1f, collider.size.z);
                }
            }
        }


        [HarmonyPatch(typeof(BaseLadder), "GetExitPoint", new Type[] { typeof(Vector3), typeof(Base.Direction) }, new[] { ArgumentType.Out, ArgumentType.Out })]
        class BaseLadder_GetExitPoint_Patch
        {
            static void Postfix(BaseLadder __instance, ref Vector3 position, ref Base.Direction direction)
            {
                //AddDebug("BaseLadder GetExitPoint " + position);
                position = new Vector3(position.x + .4f, position.y, position.z);
            }
        }


    }


}
