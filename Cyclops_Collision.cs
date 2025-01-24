using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    class Cyclops_Collision
    {
        static bool fixCyclopsCollision;
        // Storage_Patch 510
        //[HarmonyPatch(typeof(SubRoot))]
        class SubRoot_Patch
        {
            //[HarmonyPostfix]
            //[HarmonyPatch("Start")]
            public static void StartPostfix(SubRoot __instance)
            {
                if (__instance.isCyclops && fixCyclopsCollision)
                {
                    //    FixCollision(__instance);
                }

            }

            private static void FixCollision(SubRoot __instance)
            {// Start runs for prefab too
                AddDebug("FixCollision " + __instance.name);
                if (__instance.isCyclops && __instance.name == "Cyclops-MainPrefab(Clone)")
                {

                    Transform outerCol = __instance.transform.Find("CyclopsCollision/zOuterGroup");
                    if (outerCol)
                    {
                        AddDebug("FixCollision outerCol ");
                        foreach (Transform child in outerCol)
                        { // ignore cyclops outer colliders when building in cyclops
                            //AddDebug("outerCol child " + child.name);
                            child.gameObject.layer = LayerID.NotUseable;
                            //child.gameObject.SetActive(false);
                        }
                    }
                    Transform rightLowerWall = __instance.transform.Find("CyclopsCollision/keelFrontGroup/right_wall");
                    if (rightLowerWall)
                    {
                        rightLowerWall.localPosition = new Vector3(-.25f, 0f, 0f);
                        Vector3 rot = rightLowerWall.eulerAngles;
                        rightLowerWall.eulerAngles = new Vector3(80f, rot.y, rot.z);
                    }
                    Transform leftLowerWall = __instance.transform.Find("CyclopsCollision/keelFrontGroup/left_wall");
                    if (leftLowerWall)
                    {
                        leftLowerWall.localPosition = new Vector3(-.15f, 0f, 0f);
                        Vector3 rot = leftLowerWall.eulerAngles;
                        leftLowerWall.eulerAngles = new Vector3(80f, rot.y, rot.z);
                    }
                    Transform launchBayright_wall = __instance.transform.Find("CyclopsCollision/launchBayright_wall");
                    if (launchBayright_wall)
                    {
                        launchBayright_wall.localPosition = new Vector3(-.125f, 0f, 0f);
                        Vector3 rot = launchBayright_wall.eulerAngles;
                        launchBayright_wall.eulerAngles = new Vector3(80f, rot.y, rot.z);
                    }
                    Transform launchBayleft_wall = __instance.transform.Find("CyclopsCollision/launchBayleft_wall");
                    if (launchBayleft_wall)
                    {
                        launchBayleft_wall.localPosition = new Vector3(-.04f, 0f, 0f);
                        Vector3 rot = launchBayleft_wall.eulerAngles;
                        launchBayleft_wall.eulerAngles = new Vector3(80f, rot.y, rot.z);
                    }
                    Transform secondRoomGroup = __instance.transform.Find("CyclopsCollision/secondRoomGroup");
                    Transform secondRoomRight_wall = __instance.transform.Find("CyclopsCollision/secondRoomGroup/right_wall");
                    BoxCollider[] colliders = secondRoomRight_wall.GetComponents<BoxCollider>();
                    //AddDebug("secondRoomRight_wall size " + colliders[0].size);
                    GameObject leftWall = new GameObject("leftWall");
                    leftWall.transform.SetParent(secondRoomGroup);
                    Vector3 leftWallPos = secondRoomRight_wall.transform.position;
                    leftWall.transform.position = new Vector3(leftWallPos.x + .15f, leftWallPos.y, leftWallPos.z);
                    Vector3 leftWallRot = secondRoomRight_wall.transform.eulerAngles;
                    Vector3 leftWallCenter = colliders[0].center;
                    leftWall.transform.eulerAngles = new Vector3(leftWallRot.x - 3.5f, leftWallRot.y + 1f, leftWallRot.z);
                    BoxCollider leftWallСol = leftWall.AddComponent<BoxCollider>();
                    leftWallСol.size = colliders[0].size;
                    leftWallСol.center = new Vector3(leftWallCenter.x + .4f, leftWallCenter.y, leftWallCenter.z);
                    UnityEngine.Object.Destroy(colliders[0]);
                    Vector3 rightWallPos = colliders[1].transform.position;
                    colliders[1].transform.position = new Vector3(rightWallPos.x - 1.05f, rightWallPos.y, rightWallPos.z);
                    Vector3 rightWallRot = colliders[1].transform.eulerAngles;
                    colliders[1].transform.eulerAngles = new Vector3(rightWallRot.x + 3.5f, rightWallRot.y + .9f, rightWallRot.z);

                    Transform controlRoomGroup = __instance.transform.Find("CyclopsCollision/controlRoomGroup");
                    Transform controlRoomRightWall = __instance.transform.Find("CyclopsCollision/controlRoomGroup/right_wall");
                    BoxCollider[] controlRoomСolliders = controlRoomRightWall.GetComponents<BoxCollider>();
                    GameObject controlRoomLeftWall = new GameObject("leftWall");
                    controlRoomLeftWall.transform.eulerAngles = controlRoomRightWall.transform.eulerAngles;
                    controlRoomLeftWall.transform.SetParent(controlRoomGroup);
                    Vector3 controlRoomLeftWallPos = controlRoomRightWall.transform.position;
                    controlRoomLeftWall.transform.position = new Vector3(controlRoomLeftWallPos.x + .35f, controlRoomLeftWallPos.y, controlRoomLeftWallPos.z);
                    BoxCollider controlRoomLeftWallСol = controlRoomLeftWall.AddComponent<BoxCollider>();
                    controlRoomLeftWallСol.size = controlRoomСolliders[0].size;
                    controlRoomLeftWallСol.center = controlRoomСolliders[0].center;
                    UnityEngine.Object.Destroy(controlRoomСolliders[0]);
                    Vector3 controlRoomRightWallPos = controlRoomСolliders[1].transform.position;
                    controlRoomСolliders[1].transform.position = new Vector3(controlRoomRightWallPos.x - .5f, controlRoomRightWallPos.y, controlRoomRightWallPos.z);

                    Transform engineRoomLeftWall = __instance.transform.Find("CyclopsCollision/engineRoomGroup/right_wall");
                    engineRoomLeftWall.name = "leftWall";
                    Transform engineRoomRightWall = __instance.transform.Find("CyclopsCollision/engineRoomGroup/right_wall");
                    Vector3 engineRoomLeftWallPos = engineRoomLeftWall.transform.position;
                    Vector3 engineRoomLeftWallRot = engineRoomLeftWall.transform.eulerAngles;
                    engineRoomLeftWall.transform.eulerAngles = new Vector3(engineRoomLeftWallRot.x, engineRoomLeftWallRot.y - 1f, engineRoomLeftWallRot.z);
                    engineRoomLeftWall.transform.position = new Vector3(engineRoomLeftWallPos.x + 1f, engineRoomLeftWallPos.y, engineRoomLeftWallPos.z);
                    Vector3 engineRoomRightWallPos = engineRoomRightWall.transform.position;

                    engineRoomRightWall.transform.position = new Vector3(engineRoomRightWallPos.x - 1.0f, engineRoomRightWallPos.y, engineRoomRightWallPos.z);
                    Vector3 engineRoomRightWallRot = engineRoomRightWall.transform.eulerAngles;
                    engineRoomRightWall.transform.eulerAngles = new Vector3(engineRoomRightWallRot.x, engineRoomRightWallRot.y + .75f, engineRoomRightWallRot.z);
                }
            }

        }

        //[HarmonyPatch(typeof(Builder), "InitializeAsync")]
        class Builder_Initialize_Patch
        {
            public static void Postfix()
            { // ignore cyclops outer colliders when building in cyclops
              //Builder.placeLayerMask = (LayerMask)~(1 << LayerMask.NameToLayer("Player") | 1 << LayerMask.NameToLayer("Trigger") | 1 << LayerMask.NameToLayer("NotUseable"));
                if (fixCyclopsCollision)
                    Builder.placeLayerMask = -6815745;
                //AddDebug("Builder Initialize ");
                // Main.Log("Builder Initialize " + Builder.placeLayerMask.value);
            }
        }

        //[HarmonyPatch(typeof(Targeting), "GetTarget", new Type[] { typeof(float), typeof(GameObject), typeof(float)}, new[] { ArgumentType.Normal, ArgumentType.Out, ArgumentType.Out })]
        class Targeting_GetTarget_PrefixPatch
        {
            public static bool Prefix(ref GameObject result, ref bool __result, float maxDistance, out float distance)
            {
                if (!fixCyclopsCollision)
                {
                    distance = 0f;
                    return true;
                }
                Transform transform = MainCamera.camera.transform;
                Vector3 position = transform.position;
                Vector3 forward = transform.forward;
                Ray ray = new Ray(position, forward);
                int layerMask = ~(1 << LayerID.Trigger | 1 << LayerID.OnlyVehicle);
                layerMask = Builder.placeLayerMask; // my
                QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Collide;
                int numHits1 = UWE.Utils.RaycastIntoSharedBuffer(ray, maxDistance, layerMask, queryTriggerInteraction);
                DebugTargetConsoleCommand.radius = -1f;
                RaycastHit resultHit;
                if (Targeting.Filter(UWE.Utils.sharedHitBuffer, numHits1, out resultHit))
                    __result = true;

                if (!__result)
                {
                    foreach (float radius in GameInput.IsPrimaryDeviceGamepad() ? Targeting.gamepadRadiuses : Targeting.standardRadiuses)
                    {
                        DebugTargetConsoleCommand.radius = radius;
                        ray.origin = position + forward * radius;
                        int numHits2 = UWE.Utils.SpherecastIntoSharedBuffer(ray, radius, maxDistance, layerMask, queryTriggerInteraction);
                        if (Targeting.Filter(UWE.Utils.sharedHitBuffer, numHits2, out resultHit))
                        {
                            __result = true;
                            break;
                        }
                    }
                }
                Targeting.Reset();
                DebugTargetConsoleCommand.Stop();
                result = resultHit.collider != null ? resultHit.collider.gameObject : null;
                distance = resultHit.distance;
                return false;
            }
            public static bool PrefixOld(ref GameObject result, ref bool __result, float maxDistance, Targeting.FilterRaycast filter, out float distance)
            {
                //AddDebug(" Targeting GetTarget  " + result.name);
                if (!fixCyclopsCollision || !Player.main.currentSub || !Player.main.currentSub.isCyclops)
                {
                    distance = 0f;
                    return true;
                }
                bool flag = false;
                Transform transform = MainCamera.camera.transform;
                Vector3 position = transform.position;
                Vector3 forward = transform.forward;
                Ray ray = new Ray(position, forward);
                //int layerMask = -2097153;
                int layerMask = Builder.placeLayerMask;
                QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Collide;
                int numHits1 = UWE.Utils.RaycastIntoSharedBuffer(ray, maxDistance, Builder.placeLayerMask, queryTriggerInteraction);
                RaycastHit resultHit;
                //if (Targeting.Filter(UWE.Utils.sharedHitBuffer, numHits1, filter, out resultHit))
                //    flag = true;
                if (!flag)
                {
                    //for (int index = 0; index < Targeting.radiuses.Length; ++index)
                    //{
                    //    float radiuse = Targeting.radiuses[index];
                    //    ray.origin = position + forward * radiuse;
                    //    int numHits2 = UWE.Utils.SpherecastIntoSharedBuffer(ray, radiuse, maxDistance, layerMask, queryTriggerInteraction);
                    //    if (Targeting.Filter(UWE.Utils.sharedHitBuffer, numHits2, filter, out resultHit))
                    //    {
                    //        flag = true;
                    //        break;
                    //    }
                    //}
                }
                Targeting.Reset();
                //result = resultHit.collider != null ? resultHit.collider.gameObject : null;
                //distance = resultHit.distance;
                distance = 1;
                __result = flag;
                return false;
            }
        }

        //[HarmonyPatch(typeof(Fabricator), "Start")]
        class Fabricator_Start_Patch
        {
            public static void Postfix(Fabricator __instance)
            {
                if (fixCyclopsCollision && Main.gameLoaded && __instance.transform.parent && __instance.transform.parent.name == "Cyclops-MainPrefab(Clone)")
                { // collision does not match mesh. Can see it after fixing cyclops collision. move it so cant see it when outside
                  //AddDebug("Fabricator Start parent " + __instance.transform.parent.name);
                    __instance.transform.position += __instance.transform.forward * .11f;
                }
            }
        }

        //[HarmonyPatch(typeof(BuilderTool), "HandleInput")]
        class BuilderTool_HandleInput_Patch
        { // ignore cyclops outer colliders when building in cyclops
          //static readonly Targeting.FilterRaycast filter = hit => hit.collider != null && hit.collider.gameObject.layer == LayerID.NotUseable;
            public static bool Prefix(BuilderTool __instance)
            {
                if (!fixCyclopsCollision)
                    return true;

                if (__instance.handleInputFrame == Time.frameCount)
                    return false;

                __instance.handleInputFrame = Time.frameCount;
                if (!__instance.isDrawn || Builder.isPlacing || (!AvatarInputHandler.main.IsEnabled() || __instance.TryDisplayNoPowerTooltip()))
                    return false;

                Targeting.AddToIgnoreList(Player.main.gameObject);
                GameObject result;
                float distance;
                Targeting.GetTarget(30f, out result, out distance);
                if (result == null)
                    return false;

                bool buttonHeld1 = GameInput.GetButtonHeld(GameInput.Button.LeftHand);
                bool buttonDown = GameInput.GetButtonDown(GameInput.Button.Deconstruct);
                bool buttonHeld2 = GameInput.GetButtonHeld(GameInput.Button.Deconstruct);
                Constructable constructable = result.GetComponentInParent<Constructable>();
                if (constructable != null && distance > constructable.placeMaxDistance)
                    constructable = null;
                if (constructable != null)
                {
                    __instance.OnHover(constructable);
                    if (buttonHeld1)
                    {
                        __instance.Construct(constructable, true);
                    }
                    else
                    {
                        string reason;
                        if (constructable.DeconstructionAllowed(out reason))
                        {
                            if (!buttonHeld2)
                                return false;
                            if (constructable.constructed)
                            {
                                Builder.ResetLast();
                                constructable.SetState(false, false);
                            }
                            else
                                __instance.Construct(constructable, false, buttonDown);
                        }
                        else
                        {
                            if (!buttonDown || string.IsNullOrEmpty(reason))
                                return false;
                            AddMessage(reason);
                        }
                    }
                }
                else
                {
                    BaseDeconstructable deconstructable = result.GetComponentInParent<BaseDeconstructable>();
                    if (deconstructable == null)
                    {
                        BaseExplicitFace componentInParent = result.GetComponentInParent<BaseExplicitFace>();
                        if (componentInParent != null)
                            deconstructable = componentInParent.parent;
                    }
                    if (!(deconstructable != null) || distance > 11.0)
                        return false;

                    string reason;
                    if (deconstructable.DeconstructionAllowed(out reason))
                    {
                        __instance.OnHover(deconstructable);
                        if (!buttonDown)
                            return false;
                        Builder.ResetLast();
                        deconstructable.Deconstruct();
                    }
                    else
                    {
                        if (!buttonDown || string.IsNullOrEmpty(reason))
                            return false;
                        AddMessage(reason);
                    }
                }
                return false;
            }

            public static bool PrefixOld(BuilderTool __instance)
            {
                if (!fixCyclopsCollision)
                    return true;

                if (__instance.handleInputFrame == Time.frameCount)
                    return false;

                //AddDebug("BuilderTool HandleInput ");
                __instance.handleInputFrame = Time.frameCount;
                if (!__instance.isDrawn || Builder.isPlacing || (!AvatarInputHandler.main.IsEnabled() || __instance.TryDisplayNoPowerTooltip()))
                    return false;
                //AddDebug("BuilderTool HandleInput placeLayerMask " + Builder.placeLayerMask.value);
                RaycastHit hitInfo;
                if (!Physics.Raycast(MainCamera.camera.transform.position, MainCamera.camera.transform.forward, out hitInfo, 30f, Builder.placeLayerMask.value, QueryTriggerInteraction.Collide))
                    return false;
                //AddDebug("BuilderTool HandleInput Target " + hitInfo.collider.name + " parent " + hitInfo.collider.transform.parent.name);
                bool buttonHeld1 = GameInput.GetButtonHeld(GameInput.Button.LeftHand);
                bool buttonDown = GameInput.GetButtonDown(GameInput.Button.Deconstruct);
                bool buttonHeld2 = GameInput.GetButtonHeld(GameInput.Button.Deconstruct);
                Constructable constructable = hitInfo.collider.GetComponentInParent<Constructable>();
                if (constructable != null && hitInfo.distance > constructable.placeMaxDistance)
                    constructable = null;
                if (constructable != null)
                {
                    __instance.OnHover(constructable);
                    if (buttonHeld1)
                    {
                        __instance.Construct(constructable, true);
                    }
                    else
                    {
                        string reason;
                        if (constructable.DeconstructionAllowed(out reason))
                        {
                            if (!buttonHeld2)
                                return false;
                            if (constructable.constructed)
                                constructable.SetState(false, false);
                            else
                                __instance.Construct(constructable, false);
                        }
                        else
                        {
                            if (!buttonDown || string.IsNullOrEmpty(reason))
                                return false;
                            AddMessage(reason);
                        }
                    }
                }
                else
                {
                    BaseDeconstructable deconstructable = hitInfo.collider.GetComponentInParent<BaseDeconstructable>();
                    //BaseDeconstructable deconstructable = result.GetComponentInParent<BaseDeconstructable>();
                    if (deconstructable == null)
                    {
                        BaseExplicitFace componentInParent = hitInfo.collider.GetComponentInParent<BaseExplicitFace>();
                        //BaseExplicitFace componentInParent = result.GetComponentInParent<BaseExplicitFace>();
                        if (componentInParent != null)
                            deconstructable = componentInParent.parent;
                    }
                    if (deconstructable == null)
                        return false;

                    string reason;
                    if (deconstructable.DeconstructionAllowed(out reason))
                    {
                        __instance.OnHover(deconstructable);
                        if (!buttonDown)
                            return false;
                        deconstructable.Deconstruct();
                    }
                    else
                    {
                        if (!buttonDown || string.IsNullOrEmpty(reason))
                            return false;
                        AddMessage(reason);
                    }
                }
                return false;
            }
        }
        /*
        //[HarmonyPatch(typeof(BuilderTool), "OnHolster")]
        class BuilderTool_OnHolster_Patch
        {
            public static void Prefix(BuilderTool __instance)
            {
                SubRoot subRoot = Player.main.currentSub;
                if (subRoot && subRoot.isCyclops)
                {
                    //AddDebug("BuilderTool OnHolster ");
                    Transform outerCol = subRoot.transform.Find("CyclopsCollision/zOuterGroup");
                    if (outerCol)
                    {
                        foreach (Transform child in outerCol)
                        {
                            //AddDebug("outerCol child " + child.name);
                            //child.gameObject.layer = LayerID.Default;
                            //child.gameObject.SetActive(false);
                        }
                    }
                }
            }
        }

        //[HarmonyPatch(typeof(PlayerTool), "OnDraw")]
        class PlayerTool_OnDraw_Patch
        {
            public static void Prefix(PlayerTool __instance)
            {
                SubRoot subRoot = Player.main.currentSub;
                if (subRoot && subRoot.isCyclops && __instance is BuilderTool)
                {
                    //AddDebug("PlayerTool OnDraw ");
                    Transform outerCol = subRoot.transform.Find("CyclopsCollision/zOuterGroup");
                    if (outerCol)
                    {
                        foreach (Transform child in outerCol)
                        {
                            //AddDebug("outerCol child " + child.name);
                            child.gameObject.layer = LayerID.Player;
                            //child.gameObject.SetActive(false);
                        }
                    }
                }
            }
        }
        */
    }
}
