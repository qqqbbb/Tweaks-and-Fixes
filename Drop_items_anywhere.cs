using HarmonyLib;
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
    internal class Drop_items_anywhere
    {
        public static Dictionary<GameObject, SubRoot> droppedInBase = new Dictionary<GameObject, SubRoot>();
        public static Dictionary<GameObject, EscapePod> droppedInEscapePod = new Dictionary<GameObject, EscapePod>();

        private static void DropInSub(Pickupable pickupable)
        {
            SubRoot subRoot = Player.main.currentSub;

            if (subRoot == null)
                return;

            //AddDebug(pickupable.name + " DropInSub ");
            LargeWorldEntity lwe = pickupable.GetComponent<LargeWorldEntity>();
            if (lwe && lwe.isActiveAndEnabled)
                lwe.enabled = false;

            if (Main.gameLoaded && !subRoot.IsLeaking())
            {
                HandleFish(pickupable.gameObject, false);
            }
            pickupable.transform.SetParent(subRoot.GetModulesRoot());
            pickupable._isInSub = true;
            //droppedInBase.Add(pickupable.gameObject, subRoot);
            SkyEnvironmentChanged.Send(pickupable.gameObject, subRoot);
            //Rigidbody component1 = this.GetComponent<Rigidbody>();
            //if (component1)
            //    UWE.Utils.SetIsKinematicAndUpdateInterpolation(component1, true);
        }

        private static void DropInSub(Pickupable pickupable, SubRoot subRoot)
        {
            if (subRoot == null)
                return;

            //AddDebug(pickupable.name + " DropInSub ");
            LargeWorldEntity lwe = pickupable.GetComponent<LargeWorldEntity>();
            if (lwe && lwe.isActiveAndEnabled)
                lwe.enabled = false;

            //pickupable.transform.SetParent(subRoot.GetModulesRoot());
            //pickupable._isInSub = true;
            //droppedInBase.Add(pickupable.gameObject, subRoot);
            SkyEnvironmentChanged.Send(pickupable.gameObject, subRoot);
        }

        private static void DropInEscapePod(Pickupable pickupable)
        {
            EscapePod escapePod = Player.main.currentEscapePod;
            if (escapePod == null)
                return;

            //AddDebug(pickupable.name + " Drop In escapePod ");
            LargeWorldEntity lwe = pickupable.GetComponent<LargeWorldEntity>();
            if (lwe && lwe.isActiveAndEnabled)
                lwe.enabled = false;

            HandleFish(pickupable.gameObject, false);
            pickupable.transform.SetParent(escapePod.transform);
            pickupable._isInSub = true;
            SkyEnvironmentChanged.Send(pickupable.gameObject, escapePod);
        }

        private static void DropInEscapePod(Pickupable pickupable, EscapePod escapePod)
        {
            if (escapePod == null)
                return;

            //AddDebug(pickupable.name + " Drop In escapePod ");
            LargeWorldEntity lwe = pickupable.GetComponent<LargeWorldEntity>();
            if (lwe)
                lwe.enabled = false;

            HandleFish(pickupable.gameObject, false);
            SkyEnvironmentChanged.Send(pickupable.gameObject, escapePod);
            //pickupable.transform.SetParent(escapePod.transform);
            //pickupable._isInSub = true;
            //droppedInEscapePod.Add(pickupable.gameObject, escapePod);
        }

        static void HandleFish(GameObject go, bool enableMovement)
        {
            //AddDebug(go.name + " HandleFish " + enableMovement);
            if (go.GetComponent<Creature>() == null || go.GetComponent<CuteFish>())
                return;

            StayAtLeashPosition salp = go.GetComponent<StayAtLeashPosition>();
            if (salp)
                salp.enabled = enableMovement;

            SwimRandom sr = go.GetComponent<SwimRandom>();
            if (sr)
                sr.enabled = enableMovement;

            SwimToTarget stt = go.GetComponent<SwimToTarget>();
            if (stt)
                stt.enabled = enableMovement;

            MoveTowardsTarget mtt = go.GetComponent<MoveTowardsTarget>();
            if (mtt)
                mtt.enabled = enableMovement;

            SwimBehaviour sb = go.GetComponent<SwimBehaviour>();
            if (sb)
                sb.enabled = enableMovement;

            AvoidObstacles ao = go.GetComponent<AvoidObstacles>();
            if (ao)
                ao.enabled = enableMovement;

            Locomotion l = go.GetComponent<Locomotion>();
            if (l)
                l.enabled = enableMovement;

            Animator[] animators = go.GetComponentsInChildren<Animator>();
            foreach (var a in animators)
                a.enabled = enableMovement;

        }

        [HarmonyPatch(typeof(WorldForces))]
        class WorldForces_Patch
        {
            [HarmonyPrefix]
            [HarmonyPatch("DoFixedUpdate")]
            static bool DoFixedUpdatePrefix(WorldForces __instance)
            {
                if (!ConfigToEdit.dropItemsAnywhere.Value)
                    return true;

                __instance.UpdateInterpolation();
                if (__instance.useRigidbody == null || __instance.useRigidbody.isKinematic)
                    return false;

                Vector3 position = __instance.transform.position;
                bool aboveWater = __instance.IsAboveWater();
                float height = position.y;
                if (droppedInBase.ContainsKey(__instance.gameObject))
                {
                    SubRoot subRoot = droppedInBase[__instance.gameObject];
                    if (subRoot && !subRoot.IsLeaking())
                    {
                        height = 1;
                        aboveWater = true;
                    }
                }
                else if (droppedInEscapePod.ContainsKey(__instance.gameObject))
                {
                    height = 1;
                    aboveWater = true;
                }
                //if (droppedInBase.ContainsKey(__instance.gameObject))
                //    AddDebug(__instance.name + " DoFixedUpdate aboveWater " + droppedInBase[__instance.gameObject]);

                if (__instance.handleGravity && __instance.useRigidbody)
                {
                    __instance.useRigidbody.AddForce(__instance.GetGravityAtHeight(height), ForceMode.Acceleration);
                }
                if (__instance.handleDrag && __instance.useRigidbody != null)
                {
                    if (__instance.was_above_water && !aboveWater)
                        __instance.useRigidbody.drag = __instance.underwaterDrag;
                    else if (!__instance.was_above_water & aboveWater)
                        __instance.useRigidbody.drag = __instance.aboveWaterDrag;
                    __instance.was_above_water = aboveWater;
                }
                for (int index = 0; index < WorldForces.explosionList.Count; ++index)
                {
                    WorldForces.Explosion explosion = WorldForces.explosionList[index];
                    if (DayNightCycle.main.timePassed > explosion.endTime)
                    {
                        WorldForces.explosionList[index] = WorldForces.explosionList[WorldForces.explosionList.Count - 1];
                        WorldForces.explosionList.RemoveAt(WorldForces.explosionList.Count - 1);
                        --index;
                    }
                    else
                    {
                        double startTime = explosion.startTime;
                        float magnitude = (explosion.position - position).magnitude;
                        double num1 = magnitude / 500.0;
                        double num2 = startTime + num1;
                        if (DayNightCycle.main.timePassed >= num2 && DayNightCycle.main.timePassed <= num2 + 0.03 && __instance.useRigidbody != null)
                        {
                            Vector3 vector3 = position - explosion.position;
                            vector3.Normalize();
                            float num3 = Mathf.Max(explosion.magnitude - magnitude / 500f, 1f);
                            Vector3 force = vector3 * (num3 * (0.5f + UnityEngine.Random.value * 0.5f));
                            __instance.useRigidbody.AddForce(force, ForceMode.Impulse);
                            Debug.DrawLine(position, position + force, Color.yellow, 0.1f);
                        }
                    }
                }
                Vector3 vector3_1 = Vector3.zero;
                float num4 = 0f;
                for (int index = 0; index < WorldForces.currentsList.Count; ++index)
                {
                    WorldForces.Current currents = WorldForces.currentsList[index];
                    if (currents == null || DayNightCycle.main.timePassed > currents.endTime)
                    {
                        WorldForces.currentsList[index] = WorldForces.currentsList[WorldForces.currentsList.Count - 1];
                        WorldForces.currentsList.RemoveAt(WorldForces.currentsList.Count - 1);
                        --index;
                    }
                    else if ((position - currents.position).sqrMagnitude < currents.radius * currents.radius)
                    {
                        float num1 = currents.startSpeed;
                        if (!double.IsInfinity(currents.endTime))
                        {
                            float t = Mathf.InverseLerp(0f, (float)(currents.endTime - currents.startTime), (float)(DayNightCycle.main.timePassed - currents.startTime));
                            num1 = Mathf.Lerp(currents.startSpeed, 0f, t);
                        }
                        if (num1 > num4)
                        {
                            num4 = num1;
                            vector3_1 = currents.direction;
                        }
                    }
                }
                if (num4 <= 0 || __instance.useRigidbody == null)
                    return false;

                __instance.useRigidbody.AddForce(num4 * vector3_1, ForceMode.Impulse);
                return false;
            }
        }

        public static void OnGameLoadingFinished()
        {
            foreach (var kv in droppedInBase)
            {   // IsLeaking always returns false on Pickupable awake
                SubRoot subRoot = kv.Value;
                if (subRoot && !subRoot.IsLeaking())
                {
                    HandleFish(kv.Key, false);
                }
            }
        }

        [HarmonyPatch(typeof(Pickupable))]
        class Pickupable_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("Awake")]
            static void AwakePostfix(Pickupable __instance)
            {
                if (!ConfigToEdit.dropItemsAnywhere.Value || Main.gameLoaded)
                    return;

                if (__instance.inventoryItem != null)
                    return;

                if (droppedInBase.ContainsKey(__instance.gameObject) || droppedInEscapePod.ContainsKey(__instance.gameObject))
                    return;

                SubRoot subRoot = __instance.GetComponentInParent<SubRoot>();
                PlaceTool placeTool = __instance.GetComponent<PlaceTool>();
                if (subRoot)
                {
                    if (subRoot.isBase && placeTool == null)
                    {
                        //AddDebug(__instance.name + " Pickupable Awake in base ");
                        droppedInBase.Add(__instance.gameObject, subRoot);
                        DropInSub(__instance, subRoot);
                    }
                    else if (subRoot.isCyclops)
                    {
                        //AddDebug(__instance.name + " Pickupable Awake in Cyclops ");
                        if (placeTool)
                            __instance.Place();
                    }
                    return;
                }
                EscapePod escapePod = __instance.GetComponentInParent<EscapePod>();
                if (escapePod)
                {
                    //AddDebug(__instance.name + " Pickupable Awake in escapePod ");
                    droppedInEscapePod.Add(__instance.gameObject, escapePod);
                    DropInEscapePod(__instance, escapePod);
                }
            }

            [HarmonyPrefix]
            [HarmonyPatch("Unplace")]
            static bool UnplacePostfix(Pickupable __instance)
            {
                if (!ConfigToEdit.dropItemsAnywhere.Value)
                    return true;

                if (__instance.GetComponent<PlaceTool>())
                {
                    //AddDebug(__instance.name + " Pickupable Unplace  ");
                    return true;
                }
                return false;
            }

            [HarmonyPrefix]
            [HarmonyPatch("Place")]
            static bool PlacePostfix(Pickupable __instance)
            {
                if (!ConfigToEdit.dropItemsAnywhere.Value)
                    return true;
                //AddDebug(__instance.name + " Pickupable Place ");
                if (__instance.GetComponent<PlaceTool>())
                {
                    //AddDebug(__instance.name + " Pickupable Place ");
                    return true;
                }
                return false;
            }

            private static void ReplaceCollider(GameObject go)
            {
                SphereCollider sphereCollider = go.GetComponent<SphereCollider>();
                if (sphereCollider)
                {
                    //AddDebug("ReplaceCollider " + go.name);
                    BoxCollider boxCollider = go.AddComponent<BoxCollider>();
                    boxCollider.size = new Vector3(sphereCollider.radius, sphereCollider.radius, sphereCollider.radius);
                    UnityEngine.Object.Destroy(sphereCollider);

                }
            }

            static bool droppedFromInventory = false;

            [HarmonyPrefix]
            [HarmonyPatch("Drop", new Type[] { typeof(Vector3), typeof(Vector3), typeof(bool) })]
            static bool DropPrefix(Pickupable __instance, Vector3 dropPosition, Vector3 pushVelocity = default, bool checkPosition = true)
            {
                LargeWorldEntity_Patch.droppedObject = __instance.gameObject;
                if (!ConfigToEdit.dropItemsAnywhere.Value)
                    return true;

                PlaceTool placeTool = __instance.GetComponent<PlaceTool>();
                if (placeTool && Player.main.currentEscapePod == null)
                    return true;

                if (__instance.inventoryItem != null)
                {
                    IItemsContainer container = __instance.inventoryItem.container;
                    if (container != null)
                    {
                        container.RemoveItem(__instance.inventoryItem, true, false);
                        if (container == Inventory.main.container || container == Inventory.main.equipment)
                        {
                            droppedFromInventory = true;
                            //AddDebug(__instance.name + " droppedFromInventory");
                            __instance.PlayDropSound();
                        }
                    }
                    __instance.inventoryItem = null;
                }
                Player player = Player.main;
                if (droppedFromInventory)
                {
                    if (player.currentSub && player.currentSub.isBase)
                        droppedInBase.Add(__instance.gameObject, player.currentSub);
                    else if (player.currentEscapePod)
                        droppedInEscapePod.Add(__instance.gameObject, player.currentEscapePod);
                }
                WaterPark currentWaterPark = player.currentWaterPark;
                bool waterPark = currentWaterPark != null;
                __instance.SetVisible(false);
                __instance.Reparent(null);
                if (checkPosition)
                    dropPosition = Pickupable.FindDropPosition(MainCamera.camera.transform.position, dropPosition);

                __instance.transform.position = dropPosition;
                __instance.Activate(!waterPark);
                if (waterPark)
                    currentWaterPark.AddItem(__instance);

                __instance.timeDropped = 0f;
                __instance.droppedEvent.Trigger(__instance);
                __instance.gameObject.SendMessage("OnDrop", SendMessageOptions.DontRequireReceiver);
                Rigidbody rb = __instance.GetComponent<Rigidbody>();
                __instance.attached = false;
                if (!waterPark)
                {
                    if (!player.pda.isInUse)
                    {
                        rb.AddForce(pushVelocity * .5f, ForceMode.VelocityChange);
                        //AddDebug("AddForce " + pushVelocity);
                    }
                    Smell smell = __instance.gameObject.GetComponent<Smell>();
                    if (smell == null)
                        smell = __instance.gameObject.AddComponent<Smell>();

                    smell.owner = player.gameObject;
                    smell.strength = 1f;
                    smell.falloff = 0.05f;
                }
                UWE.Utils.SetIsKinematicAndUpdateInterpolation(rb, !__instance.activateRigidbodyWhenDropped);
                if (!Player.main.pda.isOpen)
                {
                    if (placeTool)
                        __instance.Place();
                }
                return false;
            }

            [HarmonyPostfix]
            [HarmonyPatch("Drop", new Type[] { typeof(Vector3), typeof(Vector3), typeof(bool) })]
            static void DropPostfix(Pickupable __instance, Vector3 dropPosition, Vector3 pushVelocity, bool checkPosition)
            {
                //AddDebug("Drop " + __instance.gameObject.name);
                if (droppedFromInventory)
                {
                    if (Player.main.currentEscapePod)
                    {
                        DropInEscapePod(__instance);
                    }
                    else if (Player.main.currentSub)
                    {
                        DropInSub(__instance);
                    }
                    else if (Player.main.precursorOutOfWater)
                    {
                        HandleFish(__instance.gameObject, false);
                    }
                    droppedFromInventory = false;
                }
            }

            [HarmonyPostfix]
            [HarmonyPatch("Pickup")]
            static void PickupPostfix(Pickupable __instance)
            {
                if (droppedInBase.ContainsKey(__instance.gameObject))
                {
                    //AddDebug("Pickupable Pickup droppedInBase");
                    HandleFish(__instance.gameObject, true);
                    droppedInBase.Remove(__instance.gameObject);
                }
                else if (droppedInEscapePod.ContainsKey(__instance.gameObject))
                {
                    //AddDebug("Pickupable Pickup droppedInBase");
                    HandleFish(__instance.gameObject, true);
                    droppedInEscapePod.Remove(__instance.gameObject);
                }
            }

        }

        [HarmonyPatch(typeof(PlaceTool), "LateUpdate")]
        class PlaceTool_LateUpdate_Prefix_Patch
        {
            static bool Prefix(PlaceTool __instance)
            { // allow to place on any surface
                if (__instance.usingPlayer == null)
                    return false;

                if (!ConfigToEdit.dropItemsAnywhere.Value)
                    return true;

                Transform aimTransform = Builder.GetAimTransform();
                RaycastHit raycastHit1 = new RaycastHit();
                bool foundSurface = false;
                int raycastHits = UWE.Utils.RaycastIntoSharedBuffer(aimTransform.position, aimTransform.forward, 5f);
                float distance = float.PositiveInfinity;
                for (int index = 0; index < raycastHits; ++index)
                {
                    RaycastHit raycastHit2 = UWE.Utils.sharedHitBuffer[index];
                    if (!raycastHit2.collider.isTrigger && !UWE.Utils.SharingHierarchy(__instance.gameObject, raycastHit2.collider.gameObject) && distance > raycastHit2.distance)
                    {
                        foundSurface = true;
                        raycastHit1 = raycastHit2;
                        distance = raycastHit2.distance;
                    }
                }
                Vector3 position;
                Vector3 forward2;
                Vector3 up2;
                if (foundSurface)
                {
                    PlaceTool.SurfaceType surfaceType = PlaceTool.SurfaceType.Floor;
                    if (Mathf.Abs(raycastHit1.normal.y) < 0.3)
                        surfaceType = PlaceTool.SurfaceType.Wall;
                    else if (raycastHit1.normal.y < 0)
                        surfaceType = PlaceTool.SurfaceType.Ceiling;

                    position = raycastHit1.point;
                    if (__instance.alignWithSurface || surfaceType == PlaceTool.SurfaceType.Wall)
                    {
                        forward2 = raycastHit1.normal;
                        up2 = Vector3.up;
                    }
                    else
                    {
                        forward2 = new Vector3(-aimTransform.forward.x, 0f, -aimTransform.forward.z).normalized;
                        up2 = Vector3.up;
                    }
                    switch (surfaceType)
                    {
                        case PlaceTool.SurfaceType.Floor:
                            __instance.validPosition = __instance.allowedOnGround;
                            break;
                        case PlaceTool.SurfaceType.Wall:
                            __instance.validPosition = __instance.allowedOnWalls;
                            break;
                        case PlaceTool.SurfaceType.Ceiling:
                            __instance.validPosition = __instance.allowedOnCeiling;
                            break;
                    }
                }
                else
                {
                    position = aimTransform.position + aimTransform.forward * 1.5f;
                    forward2 = -aimTransform.forward;
                    up2 = Vector3.up;
                    __instance.validPosition = false;
                }
                __instance.additiveRotation = Builder.CalculateAdditiveRotationFromInput(__instance.additiveRotation);
                Quaternion rotation = Quaternion.LookRotation(forward2, up2);
                if (__instance.rotationEnabled)
                    rotation *= Quaternion.AngleAxis(__instance.additiveRotation, up2);

                __instance.ghostModel.transform.position = position;
                __instance.ghostModel.transform.rotation = rotation;
                if (foundSurface)
                {
                    Rigidbody componentInParent = raycastHit1.collider.gameObject.GetComponentInParent<Rigidbody>();
                    __instance.validPosition = __instance.validPosition && (componentInParent == null || componentInParent.isKinematic || __instance.allowedOnRigidBody);
                }
                SubRoot currentSub = Player.main.GetCurrentSub();
                bool flag2 = false;
                if (foundSurface)
                    flag2 = raycastHit1.collider.gameObject.GetComponentInParent<SubRoot>() != null;

                if (foundSurface && raycastHit1.collider.gameObject.CompareTag("DenyBuilding"))
                    __instance.validPosition = false;
                if (!__instance.allowedUnderwater && raycastHit1.point.y < 0)
                    __instance.validPosition = false;
                if (currentSub == null)
                    __instance.validPosition = __instance.validPosition && (__instance.allowedOnBase || !flag2);

                if (((!__instance.allowedInBase || !currentSub ? (!__instance.allowedOutside ? 0 : (!currentSub ? 1 : 0)) : 1) & (foundSurface ? 1 : 0)) != 0)
                {
                    GameObject hitObject = UWE.Utils.GetEntityRoot(raycastHit1.collider.gameObject);
                    if (!hitObject)
                    {
                        SceneObjectIdentifier componentInParent = raycastHit1.collider.GetComponentInParent<SceneObjectIdentifier>();
                        hitObject = !componentInParent ? raycastHit1.collider.gameObject : componentInParent.gameObject;
                    }
                    if (currentSub == null)
                        __instance.validPosition = __instance.validPosition && Builder.ValidateOutdoor(hitObject);

                    if (!__instance.allowedOnConstructable)
                        __instance.validPosition = __instance.validPosition && hitObject.GetComponentInParent<Constructable>() == null;

                    __instance.validPosition &= Builder.CheckSpace(position, rotation, PlaceTool.localBounds, (int)PlaceTool.placeLayerMask, raycastHit1.collider);
                }
                else
                    __instance.validPosition = false;

                if (foundSurface)
                    __instance.validPosition = true;

                MaterialExtensions.SetColor(__instance.modelRenderers, ShaderPropertyID._Tint, __instance.validPosition ? PlaceTool.placeColorAllow : PlaceTool.placeColorDeny);
                if (__instance.hideInvalidGhostModel)
                    __instance.ghostModel.SetActive(__instance.validPosition);

                return false;
            }
        }


        [HarmonyPatch(typeof(SubRoot), "OnPlayerEntered")]
        class SubRoot_OnPlayerEntered_Postfix_Patch
        {
            static void Postfix(SubRoot __instance)
            {
                CheckFishInBase(__instance);
            }

            private static void CheckFishInBase(SubRoot subRoot)
            {
                if (subRoot.isCyclops)
                    return;

                foreach (var kv in droppedInBase)
                {
                    if (kv.Value == subRoot)
                        HandleFish(kv.Key, subRoot.IsLeaking());
                }
            }
        }

        [HarmonyPatch(typeof(Inventory), "CanDropItemHere")]
        class Inventory_CanDropItemHere_Patch
        {
            static bool Prefix(Inventory __instance, Pickupable item, ref bool __result)
            {
                if (!ConfigToEdit.dropItemsAnywhere.Value)
                    return true;

                if (item.GetComponent<PlaceTool>())
                {
                    __result = false;
                    return false;
                }
                if (Player.main.currentSub && Player.main.currentSub.isBase)
                {
                    __result = true;
                    return false;
                }
                if (Player.main.currentEscapePod || Player.main.precursorOutOfWater)
                {
                    __result = true;
                    return false;
                }
                return true;
            }
        }

        //[HarmonyPatch(typeof(Pickupable), "OnHandHover")]
        class Pickupable_OnHandHover_Postfix_Patch
        {
            static void Postfix(Pickupable __instance)
            {
                WorldForces wf = __instance.GetComponent<WorldForces>();
                LargeWorldEntity lwe = __instance.GetComponent<LargeWorldEntity>();
                bool lwe_ = lwe && lwe.isActiveAndEnabled;
                string parent = __instance.transform.parent == null ? "null" : __instance.transform.parent.name;
                AddDebug(__instance.name + " _isInSub " + __instance._isInSub + " LargeWorldEntity " + lwe_ + " parent " + parent + " droppedInBase " + droppedInBase.ContainsKey(__instance.gameObject));
            }
        }


    }

}

