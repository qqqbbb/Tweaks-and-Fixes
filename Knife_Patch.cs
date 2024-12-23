using HarmonyLib;
using Nautilus.Handlers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UWE;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    internal class Knife_Patch
    {
        public static bool giveResourceOnDamage;
        public static Vector3 knifeTargetPos;

        [HarmonyPatch(typeof(PlayerTool))]
        public class PlayerTool_Patch
        {
            static float knifeRangeDefault = 0f;
            static float knifeDamageDefault = 0f;

            [HarmonyPostfix]
            [HarmonyPatch("OnDraw")]
            public static void OnDrawPostfix(PlayerTool __instance)
            {
                //TechType tt = CraftData.GetTechType(__instance.gameObject);
                Knife knife = __instance as Knife;
                if (knife)
                {
                    if (knifeRangeDefault == 0f)
                        knifeRangeDefault = knife.attackDist;
                    if (knifeDamageDefault == 0f)
                        knifeDamageDefault = knife.damage;

                    knife.attackDist = knifeRangeDefault * ConfigMenu.knifeRangeMult.Value;
                    knife.damage = knifeDamageDefault * ConfigMenu.knifeDamageMult.Value;
                    //AddDebug(" attackDist  " + knife.attackDist);
                    //AddDebug(" damage  " + knife.damage);
                }
            }

        }

        [HarmonyPatch(typeof(Knife))]
        class Knife_Patch_
        {
            [HarmonyPrefix]
            [HarmonyPatch("OnToolUseAnim")]
            public static bool OnToolUseAnimPrefix(Knife __instance, GUIHand hand)
            {
                Vector3 position = new Vector3();
                GameObject closestObj = null;
                UWE.Utils.TraceFPSTargetPosition(Player.main.gameObject, __instance.attackDist, ref closestObj, ref position);

                //if (closestObj)
                //{
                //AddDebug("OnToolUseAnim closestObj " + closestObj.name);
                //AddDebug("OnToolUseAnim closestObj parent " + closestObj.transform.parent.name);
                //AddDebug("OnToolUseAnim closestObj parent parent " + closestObj.transform.parent.parent.name);
                //}
                //else
                //    AddDebug("OnToolUseAnim closestObj null");

                if (closestObj == null)
                {
                    InteractionVolumeUser ivu = Player.main.gameObject.GetComponent<InteractionVolumeUser>();
                    if (ivu != null && ivu.GetMostRecent() != null)
                    {
                        closestObj = ivu.GetMostRecent().gameObject;
                        //AddDebug("OnToolUseAnim GetMostRecent " + closestObj.name);
                    }
                }
                if (closestObj)
                {
                    Utils.PlayFMODAsset(__instance.attackSound, __instance.transform);
                    VFXSurface vfxSurface = closestObj.GetComponentInParent<VFXSurface>();
                    //if (vfxSurface)
                    //    AddDebug("OnToolUseAnim vfxSurface " + vfxSurface.surfaceType);
                    //else
                    //    AddDebug("OnToolUseAnim no vfxSurface ");
                    Vector3 euler = MainCameraControl.main.transform.eulerAngles + new Vector3(300f, 90f, 0f);
                    ParticleSystem particleSystem = VFXSurfaceTypeManager.main.Play(vfxSurface, __instance.vfxEventType, position, Quaternion.Euler(euler), Player.main.transform);

                    LiveMixin liveMixin = closestObj.GetComponentInParent<LiveMixin>();
                    bool validTarget = liveMixin == null || Knife.IsValidTarget(liveMixin);
                    //AddDebug("OnToolUseAnim IsValidTarget " + validTarget);
                    if (validTarget)
                    {
                        if (liveMixin)
                        {
                            bool wasAlive = liveMixin.IsAlive();
                            liveMixin.TakeDamage(__instance.damage, position, __instance.damageType, Player.main.gameObject);
                            __instance.GiveResourceOnDamage(closestObj, liveMixin.IsAlive(), wasAlive);
                        }
                    }
                    else
                        closestObj = null;
                }
                if (closestObj || hand.GetActiveTarget())
                    return false;

                if (Player.main.IsUnderwater())
                    Utils.PlayFMODAsset(__instance.underwaterMissSound, __instance.transform);
                else
                    Utils.PlayFMODAsset(__instance.surfaceMissSound, __instance.transform);

                return false;
            }

            [HarmonyPostfix]
            [HarmonyPatch("OnToolUseAnim")]
            public static void OnToolUseAnimPostfix(Knife __instance)
            {
                if (!Player.main.guiHand.activeTarget)
                    return;

                BreakableResource breakableResource = Player.main.guiHand.activeTarget.GetComponent<BreakableResource>();
                if (breakableResource)
                {
                    breakableResource.BreakIntoResources();
                    //AddDebug("BreakableResource");
                }
                Pickupable pickupable = Player.main.guiHand.activeTarget.GetComponent<Pickupable>();
                if (pickupable)
                {
                    TechType techType = pickupable.GetTechType();
                    if (PickupablePatch.notPickupableResources.Contains(techType))
                    {
                        Rigidbody rb = pickupable.GetComponent<Rigidbody>();
                        if (rb && rb.isKinematic)  // attached to wall
                            pickupable.OnHandClick(Player.main.guiHand);
                    }
                }
            }

            [HarmonyPrefix]
            [HarmonyPatch("GiveResourceOnDamage")]
            public static void GiveResourceOnDamagePrefix(Knife __instance, GameObject target, bool isAlive, bool wasAlive)
            {
                //AddDebug("GiveResourceOnDamage ");
                giveResourceOnDamage = true;
                knifeTargetPos = target.transform.position;

            }

            //[HarmonyPostfix]
            //[HarmonyPatch("GiveResourceOnDamage")]
            public static void GiveResourceOnDamageMy(Knife __instance, GameObject target, bool isAlive, bool wasAlive)
            {
                if (isAlive || wasAlive)
                    return;

                //TechType techType = CraftData.GetTechType(target);
                //string name = techType.AsString();
                //if (Main.config.deadCreatureLoot.ContainsKey(name))
                //{
                //    Creature creature = target.GetComponent<Creature>();
                //    if (creature == null)
                //        return;

                //    if (deadCreatureLoot.ContainsKey(creature))
                //    {
                //        foreach (var pair in Main.config.deadCreatureLoot[name])
                //        {
                //            TechType loot = pair.Key;
                //            int max = pair.Value;
                //            if (deadCreatureLoot[creature].ContainsKey(loot) && deadCreatureLoot[creature][loot] < max)
                //            {
                //                CraftData.AddToInventory(loot);
                //                deadCreatureLoot[creature][loot]++;
                //            }
                //        }
                //    }
                //    else
                //    {
                //        foreach (var pair in Main.config.deadCreatureLoot[name])
                //        {
                //            CraftData.AddToInventory(pair.Key);
                //            deadCreatureLoot.Add(creature, new Dictionary<TechType, int> { { pair.Key, 1 } });
                //        }
                //    }
                //}
            }
        }

        public static void AddToInventoryOrSpawn(TechType techType, int num)
        {
            for (int i = 0; i < num; ++i)
            {
                if (Inventory.main.HasRoomFor(techType))
                    CraftData.AddToInventory(techType);
                else
                { // spawn position from AddToInventory can be behind object
                    AddError(Language.main.Get("InventoryFull"));
                    Vector3 pos = default;
                    if (knifeTargetPos != default)
                    {
                        Transform camTr = MainCamera.camera.transform;
                        float x = Mathf.Lerp(knifeTargetPos.x, camTr.position.x, .5f);
                        float y = camTr.position.y + camTr.forward.y * 3f; // fix for creepvine 
                        float z = Mathf.Lerp(knifeTargetPos.z, camTr.position.z, .5f);
                        pos = new Vector3(x, y, z);
                        //AddDebug("spawn Pos " + pos);
                    }
                    CoroutineHost.StartCoroutine(Util.Spawn(techType, pos));
                }
            }
        }

        [HarmonyPatch(typeof(CraftData), "AddToInventory")]
        class CraftData_AddToInventory_Patch
        {
            static bool Prefix(CraftData __instance, TechType techType, int num, bool noMessage, bool spawnIfCantAdd)
            {
                //AddDebug("AddToInventory Prefix giveResourceOnDamage " + techType + " " + num);
                if (giveResourceOnDamage && !spawnIfCantAdd)
                {
                    AddToInventoryOrSpawn(techType, num);
                    giveResourceOnDamage = false;
                    return false;
                }
                return true;
            }
        }

        static ParticleSystem[] heatBladeParticles;

        [HarmonyPatch(typeof(VFXLateTimeParticles))]
        public class VFXLateTimeParticles_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("Play")]
            public static void PlayPostfix(VFXLateTimeParticles __instance)
            {
                if (__instance.name != "xHeatBlade_Bubbles(Clone)")
                    return;

                heatBladeParticles = __instance.psChildren;
                FixHeatBlade();
            }
        }

        public static void FixHeatBlade()
        { //  fix heatblade underwater particles 
            if (heatBladeParticles == null || heatBladeParticles.Length != 3 || heatBladeParticles[0] == null || heatBladeParticles[0].gameObject == null || !heatBladeParticles[0].gameObject.activeInHierarchy)
                return;

            //AddDebug("FixHeatBlade");
            bool underwater = Player.main.isUnderwater.value;
            heatBladeParticles[1].EnableEmission(!underwater); // xSmk
            heatBladeParticles[0].EnableEmission(underwater); // xHeatBlade_Bubbles(Clone)
            heatBladeParticles[2].EnableEmission(underwater); // xRefract
        }

        public static void OnPlayerUnderwaterChanged(Utils.MonitoredValue<bool> isUnderwaterForSwimming)
        {
            //AddDebug(" OnPlayerUnderwaterChanged " + Player.main.IsUnderwater());
            Knife_Patch.FixHeatBlade();
        }
    }
}
