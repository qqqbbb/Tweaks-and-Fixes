using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
//using AddressableAssets;

namespace Tweaks_Fixes
{
    class Oxygen_Patch
    {
        static GameObject bubble;


        static void SpawnBubble(Vector3 position)
        {
            //UWE.PrefabDatabase.TryGetPrefabForFilename("WorldEntities/VFX/x_BubblesPlane_01", out bubble);
            //bubble = Resources.Load<GameObject>("PrefabInstance/Bubble");
            if (bubble)
            {
                //ErrorMessage.AddDebug("bubble ");
                GameObject ent = UWE.Utils.InstantiateWrap(bubble, position, Quaternion.identity);
            }
        }

        [HarmonyPatch(typeof(LiveMixin), "TakeDamage")]
        class LiveMixin_TakeDamage_Patch
        {
            public static void Postfix(LiveMixin __instance, float originalDamage, Vector3 position, DamageType type, GameObject dealer)
            {
                if (originalDamage > 1f && __instance.GetComponent<BrainCoral>())
                {
                    //ErrorMessage.AddDebug("BrainCoral " );
                    IntermittentInstantiate ii = __instance.GetComponent<IntermittentInstantiate>();
                    GameObject ent = UWE.Utils.InstantiateWrap(ii.prefab, position, Quaternion.identity);
                    //if (ii.registerToWorldStreamer && LargeWorldStreamer.main)
                    //    LargeWorldStreamer.main.cellManager.RegisterEntity(ent);
                    //if (ii.onCreateSound)
                    //    Utils.PlayEnvSound(ii.onCreateSound, ent.transform.position, 1f);
                }
            }
        }

        [HarmonyPatch(typeof(Player), "Start")]
        class Player_Start_Patch
        {// cant use Resources.Load , bubble prefab is not in resources folder
            public static void Postfix()
            {
                GameObject brainCoral = CraftData.GetPrefabForTechType(TechType.PurpleBrainCoral);
                brainCoral = Utils.CreatePrefab(brainCoral);
                IntermittentInstantiate ii = brainCoral.GetComponent<IntermittentInstantiate>();
                bubble = ii.prefab;
                UnityEngine.Object.Destroy(brainCoral);
                //LargeWorldEntity.Register(prefab);

            }
        }

        [HarmonyPatch(typeof(LiveMixin), "Kill")]
        class LiveMixin_Kill_Patch
        {
            public static void Postfix(LiveMixin __instance)
            {
                if (__instance.GetComponent<Bladderfish>() || __instance.GetComponent<Floater>())
                {
                    //ErrorMessage.AddDebug("Kill ");
                    SpawnBubble(__instance.gameObject.transform.position);
                }

            }
        }
    }
}
