using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UWE;
using static ErrorMessage;


namespace Tweaks_Fixes
{
    [HarmonyPatch(typeof(ExosuitDrillArm))]
    internal class Exosuit_Drill
    {
        static bool wasAlive;
        static LiveMixin targetLiveMixin;
        static float targetHealthGiveResourceThreshold = float.MaxValue;
        static Drillable drillable;

        [HarmonyPrefix, HarmonyPatch("OnHit")]
        public static bool OnHitPrefix(ExosuitDrillArm __instance)
        {
            if (!__instance.exosuit.CanPilot() || !__instance.exosuit.GetPilotingMode())
                return false;

            Vector3 position = Vector3.zero;
            GameObject closestObj = null;
            UWE.Utils.TraceFPSTargetPosition(__instance.exosuit.gameObject, ExosuitDrillArm.attackDist, ref closestObj, ref position);
            GameObject root = null;

            if (closestObj == null)
            {
                InteractionVolumeUser component = Player.main.gameObject.GetComponent<InteractionVolumeUser>();
                if (component != null && component.GetMostRecent() != null)
                {
                    closestObj = component.GetMostRecent().gameObject;
                }
            }
            else
                root = Util.GetEntityRoot(closestObj);

            if (closestObj && __instance.drilling)
            {
                //AddDebug($"OnHit closestObj {closestObj.name}");
                __instance.loopHit.Play();
                if (root == null)
                    drillable = closestObj.GetComponent<Drillable>();
                else
                {
                    drillable = root.GetComponent<Drillable>();
                    targetLiveMixin = root.GetComponent<LiveMixin>();
                    if (targetLiveMixin)
                    {
                        wasAlive = targetLiveMixin.IsAlive();
                        if (targetHealthGiveResourceThreshold == float.MaxValue)
                        {
                            targetHealthGiveResourceThreshold = targetLiveMixin.health - Knife_Patch.GetKnifeDamage();
                            //AddDebug("OnHit targetHealthGiveResourceThreshold " + (int)targetHealthGiveResourceThreshold);
                        }
                        if (targetLiveMixin.IsAlive())
                            targetLiveMixin.TakeDamage(ExosuitDrillArm.damage * ConfigMenu.drillDamageMult.Value, position, DamageType.Drill);
                    }
                }
                if (drillable)
                {
                    drillable.OnDrill(__instance.fxSpawnPoint.position, __instance.exosuit, out var hitObject);
                    // fix bug: particles did not play when started to drill drillable
                    if (__instance.fxControl.emitters[0].fxPS != null && (!__instance.fxControl.emitters[0].fxPS.IsAlive() || !__instance.fxControl.emitters[0].fxPS.emission.enabled))
                    {
                        __instance.fxControl.Play(0);
                    }
                    return false;
                }
                VFXSurface surface = closestObj.GetComponent<VFXSurface>();
                if (surface == null && root != null)
                    surface = root.GetComponent<VFXSurface>();

                //if (surface == null)
                //    AddDebug($"OnHit surface == null ");
                //else
                //    AddDebug($"OnHit {surface.surfaceType}");
                if (__instance.drillFXinstance == null)
                {
                    //AddDebug($"OnHit VFXSurfaceTypeManager Play");
                    __instance.drillFXinstance = VFXSurfaceTypeManager.main.Play(surface, __instance.vfxEventType, __instance.fxSpawnPoint.position, __instance.fxSpawnPoint.rotation, __instance.fxSpawnPoint);
                }
                else if (surface != null && __instance.prevSurfaceType != surface.surfaceType)
                {
                    __instance.drillFXinstance.GetComponent<VFXLateTimeParticles>().Stop();
                    UnityEngine.Object.Destroy(__instance.drillFXinstance.gameObject, 0.6f);
                    __instance.drillFXinstance = VFXSurfaceTypeManager.main.Play(surface, __instance.vfxEventType, __instance.fxSpawnPoint.position, __instance.fxSpawnPoint.rotation, __instance.fxSpawnPoint);
                    __instance.prevSurfaceType = surface.surfaceType;
                }
                if (root)
                    root.SendMessage("BashHit", __instance, SendMessageOptions.DontRequireReceiver);
                else
                    closestObj.SendMessage("BashHit", __instance, SendMessageOptions.DontRequireReceiver);
            }
            else
            {
                __instance.StopEffects();
            }
            return false;
        }
        [HarmonyPostfix, HarmonyPatch("OnHit")]
        static void OnHitPostfix(ExosuitDrillArm __instance)
        {
            if (targetLiveMixin == null || drillable)
                return;

            //if (Input.GetKey(KeyCode.LeftShift))
            //    AddDebug("OnHit postfix target.health " + (int)targetLiveMixin.health);

            if (targetHealthGiveResourceThreshold <= 0 || targetLiveMixin.health < targetHealthGiveResourceThreshold)
            {
                HarvestResource(targetLiveMixin.gameObject, targetLiveMixin.IsAlive(), wasAlive, __instance.fxSpawnPoint.transform.position);
                targetHealthGiveResourceThreshold = float.MaxValue;
            }
            targetLiveMixin = null;
            drillable = null;
        }

        [HarmonyPrefix, HarmonyPatch("StopEffects")]
        static bool StopEffectsPrefix(ExosuitDrillArm __instance)
        {
            //AddDebug("StopEffects ");
            if (__instance.drillFXinstance != null)
            {
                __instance.drillFXinstance.GetComponent<VFXLateTimeParticles>().Stop();
                UnityEngine.Object.Destroy(__instance.drillFXinstance.gameObject, 1.6f);
                __instance.drillFXinstance = null;
            }
            if (__instance.fxControl.emitters[0].fxPS != null && __instance.fxControl.emitters[0].fxPS.emission.enabled)
                __instance.fxControl.Stop(0);
            //__instance.loop.Stop();// dont stop drilling sound when not hitting anything
            __instance.loopHit.Stop();
            targetHealthGiveResourceThreshold = float.MaxValue;
            targetLiveMixin = null;
            drillable = null;
            return false;
        }

        [HarmonyPostfix, HarmonyPatch("IExosuitArm.OnUseUp")]
        static void OnUseUpPostfix(ExosuitDrillArm __instance)
        {
            //AddDebug("OnUseUp ");
            targetHealthGiveResourceThreshold = float.MaxValue;
            targetLiveMixin = null;
            drillable = null;
            __instance.loop.Stop();
        }
        [HarmonyPrefix, HarmonyPatch("IExosuitArm.Update")]
        static bool UpdatePrefix(ExosuitDrillArm __instance, ref Quaternion aimDirection)
        { // dont autoaim the arm
            return false;
        }

        static void HarvestResource(GameObject target, bool isAlive, bool wasAlive, Vector3 armPos)
        {
            TechType techType = CraftData.GetTechType(target);
            //AddDebug("AddHarvestResourceToExosuit target techType " + techType);
            if (techType == TechType.None)
                return;

            HarvestType harvestType = TechData.GetHarvestType(techType);
            //AddDebug("AddHarvestResourceToExosuit harvestType " + harvestType);
            TechType harvestOutput = TechData.GetHarvestOutput(techType);
            if (harvestOutput == TechType.None)
                return;
            //AddDebug($"AddHarvestResourceToExosuit {harvestOutput} {harvestType}");
            if ((harvestType == HarvestType.DamageAlive && wasAlive) || (harvestType == HarvestType.DamageDead && !isAlive))
            {
                int num = 1;
                if (harvestType == HarvestType.DamageAlive && !isAlive)
                    num += TechData.GetHarvestFinalCutBonus(harvestOutput);

                Exosuit exosuit = Player.main.currentMountedVehicle as Exosuit;
                if (exosuit == null)
                    return;

                if (ConfigToEdit.spawnResourcesWhenDrilling.Value == false && exosuit.storageContainer.container.HasRoomFor(harvestOutput))
                {
                    CoroutineHost.StartCoroutine(Util.AddToContainerAsync(harvestOutput, exosuit.storageContainer.container, true));
                    string name = Language.main.Get(harvestOutput);
                    ErrorMessage.AddMessage(Language.main.GetFormat("VehicleAddedToStorage", name));
                    uGUI_IconNotifier.main.Play(harvestOutput, uGUI_IconNotifier.AnimationType.From);
                }
                else
                {
                    CoroutineHost.StartCoroutine(Util.SpawnAsync(harvestOutput, armPos));
                    if (ConfigToEdit.spawnResourcesWhenDrilling.Value == false)
                        ErrorMessage.AddMessage(Language.main.Get("ContainerCantFit"));
                }
            }
        }


    }
}
