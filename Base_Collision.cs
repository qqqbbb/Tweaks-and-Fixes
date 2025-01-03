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
                    c.center = new Vector3(0.2f, c.center.y, c.center.z);
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


        [HarmonyPatch(typeof(BaseDeconstructable), "Init")]
        class BaseDeconstructable_Init_Patch
        {
            static void Postfix(BaseDeconstructable __instance)
            {
                //AddDebug("BaseDeconstructable Init " + __instance.name);
                if (__instance.name == "BaseRoomHatch(Clone)" || __instance.name == "BaseRoomCorridorConnector(Clone)")
                {
                    RemoveHatchRailingCollision(__instance.gameObject);
                }
                else if (__instance.name == "BaseWaterParkHatch(Clone)")
                {
                    FixWaterParkHatch(__instance.gameObject);
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
