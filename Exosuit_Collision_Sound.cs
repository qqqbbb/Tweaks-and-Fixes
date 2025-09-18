using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;
using static ErrorMessage;


namespace Tweaks_Fixes
{
    internal class Exosuit_Collision_Sound
    {
        private static void CreateCollisionSounds(Exosuit exosuit)
        {
            CollisionSound collisionSound = exosuit.gameObject.EnsureComponent<CollisionSound>();
            FMODAsset so = ScriptableObject.CreateInstance<FMODAsset>();
            so.path = "event:/sub/common/fishsplat";
            so.id = "{0e47f1c6-6178-41bd-93bf-40bfca179cb6}";
            collisionSound.hitSoundSmall = so;
            so = ScriptableObject.CreateInstance<FMODAsset>();
            so.path = "event:/sub/seamoth/impact_solid_hard";
            so.id = "{ed65a390-2e80-4005-b31b-56380500df33}";
            collisionSound.hitSoundFast = so;
            so = ScriptableObject.CreateInstance<FMODAsset>();
            so.path = "event:/sub/seamoth/impact_solid_medium";
            so.id = "{cb2927bf-3f8d-45d8-afe2-c82128f39062}";
            collisionSound.hitSoundMedium = so;
            so = ScriptableObject.CreateInstance<FMODAsset>();
            so.path = "event:/sub/seamoth/impact_solid_soft";
            so.id = "{15dc7344-7b0a-4ffd-9b5c-c40f923e4f4d}";
            collisionSound.hitSoundSlow = so;
        }

        [HarmonyPatch(typeof(Exosuit), "Start")]
        class Exosuit_Start_Patch
        {
            public static void Postfix(Exosuit __instance)
            {
                CreateCollisionSounds(__instance);
            }
        }

        [HarmonyPatch(typeof(CollisionSound), "OnCollisionEnter")]
        class CollisionSound_OnCollisionEnter_Patch
        {
            static bool Prefix(CollisionSound __instance, Collision col)
            {
                Exosuit exosuit = Player.main.currentMountedVehicle as Exosuit;
                //AddDebug("OnCollisionEnter " + col.gameObject.name);
                if (exosuit && col.gameObject.name == "ChunkCollider(Clone)")
                {// no collision sound when walking on ground
                    Exosuit exosuit_ = __instance.GetComponent<Exosuit>();
                    if (exosuit_ == exosuit)
                        return false;
                }
                return true;
            }
        }
    }
}
