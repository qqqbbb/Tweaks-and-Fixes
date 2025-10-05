using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static ErrorMessage;
using static Tweaks_Fixes.Locker_Door_Anim;

namespace Tweaks_Fixes
{
    public class Storage_Patch
    {
        public static HashSet<Sign> savedSigns = new HashSet<Sign>();
        public static HashSet<StorageContainer> labelledLockers = new HashSet<StorageContainer>();
        enum DoorType { Locker, CyclopsLocker, DecoLocker }

        public struct SavedLabel
        {
            public string text;
            public bool background;
            public int color;
            public int scale;

            public SavedLabel(string text_, bool background_, int color_, int scale_)
            {
                text = text_;
                color = color_;
                scale = scale_;
                background = background_;
            }
        }

        static string GetLabelKey(Transform door)
        {
            Constructable c = door.GetComponentInParent<Constructable>();
            PrefabIdentifier pi = c.GetComponent<PrefabIdentifier>();
            //AddDebug("GetKey " + pi.name);
            Vector3 pos = pi.transform.position;
            StringBuilder sb = new StringBuilder(Mathf.RoundToInt(pos.x).ToString());
            sb.Append("_");
            sb.Append(Mathf.RoundToInt(pos.y));
            sb.Append("_");
            sb.Append(Mathf.RoundToInt(pos.z));
            //Main.logger.LogMessage("GetLabelKey " + door.name + " " + sb.ToString());
            return sb.ToString();
        }

        static string GetLabelKeyCyclops(Transform door)
        {
            //AddDebug("GetKeyCyclops " + door.name);
            PingInstance pi = door.GetComponentInParent<PingInstance>();
            StorageContainer st = door.GetComponentInParent<StorageContainer>();
            Vector3 pos = Vector3.zero;
            if (st)
                pos = st.transform.localPosition;
            else
            { // deco locker 
                //AddDebug("StorageContainer == null " + door.name);
                PrefabIdentifier pi_ = door.GetComponentInParent<PrefabIdentifier>();
                pos = pi_.transform.position;
            }
            if (st && st.GetComponent<CyclopsLocker>())
                pos = st.transform.parent.parent.localPosition;

            StringBuilder sb = new StringBuilder(pi._label);
            sb.Append("_");
            sb.Append(Mathf.RoundToInt(pos.x));
            sb.Append("_");
            sb.Append(Mathf.RoundToInt(pos.y));
            sb.Append("_");
            sb.Append(Mathf.RoundToInt(pos.z));
            //AddDebug("GetKeyCyclops " + door.name + " " + sb.ToString());
            return sb.ToString();
        }

        static void CleanDecoLocker(Transform locker)
        {
            Constructable c = locker.GetComponent<Constructable>();
            UnityEngine.Object.Destroy(c);
            TechTag tt = locker.GetComponent<TechTag>();
            UnityEngine.Object.Destroy(tt);
            //PrefabIdentifier pi = locker.GetComponent<PrefabIdentifier>();
            //UnityEngine.Object.Destroy(pi); need this to save stored items
            LiveMixin lm = locker.GetComponent<LiveMixin>();
            UnityEngine.Object.Destroy(lm);
            SkyApplier sa = locker.GetComponent<SkyApplier>();
            UnityEngine.Object.Destroy(sa);
            Transform tr = locker.Find("model");
            UnityEngine.Object.Destroy(tr.gameObject);
            tr = locker.Find("Builder Trigger");
            UnityEngine.Object.Destroy(tr.gameObject);
            tr = locker.Find("Collider");
            UnityEngine.Object.Destroy(tr.gameObject);
            //PrefabIdentifier pi = locker.gameObject.GetComponent<PrefabIdentifier>();
            //Main.Log("CleanDecoLocker PrefabIdentifier id " + pi.id);
            //string id = pi.id;
            //string classId = pi.classId;
            //UnityEngine.Object.Destroy(pi);
            //if (!Main.IsDestroyed(pi))
            //{
            //    AddDebug("CleanDecoLocker Destroying PrefabIdentifier ");
            //    yield return null;
            //}
            //ChildObjectIdentifier coi = locker.gameObject.AddComponent<ChildObjectIdentifier>();
            //coi.id = id;
            //coi.classId = classId;
            //AddDebug("CleanDecoLocker done");
        }

        static IEnumerator AddLabel(Transform door, DoorType type, Transform locker)
        {
            if (door.GetComponentInChildren<Sign>())
                yield break;

            yield return new WaitUntil(() => Main.gameLoaded == true);
            //Main.logger.LogMessage("AddLabel 1 " + door.name);
            //AddDebug("AddLabel  " + locker.name + " " + type);
            TaskResult<GameObject> result = new TaskResult<GameObject>();
            yield return CraftData.InstantiateFromPrefabAsync(TechType.Sign, result);
            GameObject sign = result.Get();
            //AddDebug("AddLabel sign " + sign.name);
            sign.transform.position = door.transform.position;
            sign.transform.SetParent(door);
            //Transform tr = sign.transform.Find("SignMesh");
            //if (tr == null)
            //    AddDebug("AddLabel SignMesh null");
            //UnityEngine.Object.Destroy(tr.gameObject);
            Transform tr = sign.transform.Find("Trigger");
            //if (tr == null)
            //    AddDebug("AddLabel Trigger null");
            UnityEngine.Object.Destroy(tr.gameObject);
            tr = sign.transform.Find("UI/Base/Up");
            //if (tr == null)
            //    AddDebug("AddLabel UI/Base/Up null");
            UnityEngine.Object.Destroy(tr.gameObject);
            tr = sign.transform.Find("UI/Base/Down");
            UnityEngine.Object.Destroy(tr.gameObject);
            tr = sign.transform.Find("UI/Base/Left");
            UnityEngine.Object.Destroy(tr.gameObject);
            tr = sign.transform.Find("UI/Base/Right");
            UnityEngine.Object.Destroy(tr.gameObject);
            if (type == DoorType.Locker)
            {
                LiveMixin lm = locker.GetComponent<LiveMixin>();
                UnityEngine.Object.Destroy(lm);
                sign.transform.localPosition = new Vector3(.32f, -.58f, .26f);
                sign.transform.localEulerAngles = new Vector3(0f, 90f, 90f);
                tr = sign.transform.Find("UI/Base/Minus");
                tr.localPosition = new Vector3(tr.localPosition.x - 130f, tr.localPosition.y - 320f, tr.localPosition.z);
                tr = sign.transform.Find("UI/Base/Plus");
                tr.localPosition = new Vector3(tr.localPosition.x + 130f, tr.localPosition.y - 320f, tr.localPosition.z);
            }
            else if (type == DoorType.CyclopsLocker)
            {
                sign.transform.localPosition = new Vector3(-.03f, -.37f, .45f);
                sign.transform.localEulerAngles = new Vector3(0f, 80f, 90f);
            }
            else if (type == DoorType.DecoLocker)
            {
                sign.transform.localPosition = new Vector3(-.31f, -.02f, .45f);
                sign.transform.localEulerAngles = new Vector3(90f, 0f, 0f);
                locker.gameObject.AddComponent<DecoLockerMyController>();
            }
            Constructable c = sign.GetComponent<Constructable>();
            //if (c == null)
            //    AddDebug("AddLabel Constructable null");
            UnityEngine.Object.Destroy(c);
            TechTag tt = sign.GetComponent<TechTag>();
            UnityEngine.Object.Destroy(tt);
            ConstructableBounds cb = sign.GetComponent<ConstructableBounds>();
            //if (cb == null)
            //    AddDebug("AddLabel ConstructableBounds null");
            UnityEngine.Object.Destroy(cb);
            PrefabIdentifier pi = sign.GetComponent<PrefabIdentifier>();
            //if (pi == null)
            //    AddDebug("AddLabel PrefabIdentifier null");
            UnityEngine.Object.Destroy(pi);
            uGUI_SignInput signInput = sign.GetComponentInChildren<uGUI_SignInput>();
            //AddDebug("AddLabel 11 " + locker.name + " " + type);
            while (signInput == null)
            {
                //AddDebug("AddLabel signInput == null " + type);
                signInput = sign.GetComponentInChildren<uGUI_SignInput>();
                yield return null;
            }
            //AddDebug("AddLabel 22 " + locker.name + " " + type);
            signInput.stringDefaultLabel = "SmallLockerDefaultLabel";
            signInput.inputField.text = Language.main.Get(signInput.stringDefaultLabel);

            if (type == DoorType.Locker)
                signInput.inputField.characterLimit = 58;
            else if (type == DoorType.CyclopsLocker || type == DoorType.DecoLocker)
                signInput.inputField.characterLimit = 44;
            //    si.scaleIndex = -2; // range -3 3 
            string slot = SaveLoadManager.main.currentSlot;
            if (Main.configMain.lockerNames.ContainsKey(slot))
            {
                //Main.logger.LogMessage("AddLabel lockerNames.ContainsKey" + door.name);
                if (locker.parent && locker.parent.GetComponent<SubControl>())
                {
                    //AddDebug("AddLabel  parent is cyclops " + type);
                    type = DoorType.CyclopsLocker;
                }
                string key = string.Empty;
                if (type == DoorType.Locker || type == DoorType.DecoLocker)
                    key = GetLabelKey(door);
                else if (type == DoorType.CyclopsLocker)
                    key = GetLabelKeyCyclops(door);

                if (Main.configMain.lockerNames[slot].ContainsKey(key))
                {
                    //Main.logger.LogMessage("AddLabel lockerNames.ContainsKey " + door.name);
                    SavedLabel sl = Main.configMain.lockerNames[slot][key];
                    signInput.inputField.text = sl.text;
                    signInput.colorIndex = sl.color;
                    signInput.SetBackground(sl.background);
                    signInput.scaleIndex = sl.scale;
                    //Main.logger.LogMessage("AddLabel lockerNames.ContainsKey " + signInput.inputField.text);
                    //AddDebug("saved text " + sl.text);
                }
            }
            //AddDebug("AddLabel done " + type);
        }

        [HarmonyPatch(typeof(StorageContainer))]
        class StorageContainer_Patch
        {
            [HarmonyPostfix, HarmonyPatch("Awake")]
            static void AwakePostfix(StorageContainer __instance)
            {
                if (!ConfigToEdit.newStorageUI.Value)
                    return;
                //AddDebug("StorageContainer Awake " + __instance.prefabRoot.name);
                //if (__instance.transform.parent)
                //    AddDebug("StorageContainer Awake parent " + __instance.transform.parent.name);
                if (__instance.name == "submarine_locker_01_door")
                {
                    PrefabIdentifier pi = __instance.GetComponentInParent<PrefabIdentifier>();
                    //AddDebug("CyclopsLocker  " + pi.name);
                    if (pi.name == "Cyclops-MainPrefab(Clone)") // dont touch prefab
                        UWE.CoroutineHost.StartCoroutine(AddLabel(__instance.transform, DoorType.CyclopsLocker, __instance.transform));
                }
            }

            [HarmonyPostfix, HarmonyPatch("CreateContainer")]
            static void CreateContainerPostfix(StorageContainer __instance)
            { // runs twice on game load
                if (!ConfigToEdit.newStorageUI.Value)
                    return;
                //TechTag techTag = __instance.GetComponent<TechTag>();
                //if (techTag)
                //    AddDebug(__instance.name + "  CreateContainer  " + techTag.type);
                //else
                //    AddDebug(__instance.name + "  CreateContainer  ");
                //if (__instance.transform.parent)
                //    AddDebug(__instance.name + "  CreateContainer parent  " + __instance.transform.parent.name);
                Transform parent = __instance.transform.parent;
                if (!labelledLockers.Contains(__instance) && parent && parent.name == "DecorativeLockerClosed(Clone)")
                {
                    Transform decoDoor = parent.Find("submarine_locker_03_door_01/locker_04_door");
                    if (decoDoor)
                    {
                        //AddDebug("decoDoor ");
                        labelledLockers.Add(__instance);
                        CleanDecoLocker(__instance.transform);
                        UWE.CoroutineHost.StartCoroutine(AddLabel(decoDoor, DoorType.DecoLocker, parent));
                    }
                }
                TechType techType = CraftData.GetTechType(__instance.gameObject);
                if (techType == TechType.None)
                    return;

                //AddDebug(__instance.name + "  CreateContainer  " + techType);
                if (techType == TechType.LuggageBag)
                //if (parent && parent.name == "docking_luggage_01_bag4(Clone)")
                {
                    //AddDebug("CreateContainer LuggageBag");
                    Pickupable p = parent.GetComponent<Pickupable>();
                    if (p == null)
                        return;

                    Transform t = parent.Find("model/Cube");
                    if (t)
                    {
                        BoxCollider colliderToDelete = t.GetComponent<BoxCollider>();
                        p.disabledColliders?.Remove(colliderToDelete);
                        UnityEngine.Object.Destroy(colliderToDelete);
                    }
                    t = parent.Find("storage");
                    if (t)
                    {
                        BoxCollider boxCollider = t.GetComponent<BoxCollider>();
                        p.disabledColliders?.Add(boxCollider);
                        //boxCollider.isTrigger = false; this gives problems when placing bag in cyclops
                        boxCollider.center = new Vector3(0, .23f, 0);
                        boxCollider.size = new Vector3(1, .45f, .5f);
                    }
                }
                else if (techType == TechType.SmallLocker)
                {
                    //AddDebug("StorageContainer CreateContainer " + __instance.prefabRoot.name);
                    //if (Main.loadingDone && __instance.transform.parent && __instance.transform.parent.name == "Cyclops-MainPrefab(Clone)")
                    { // collision does not match mesh. Can see it after fixing cyclops collision. move it so cant see it when outside
                      //AddDebug("StorageContainer OnConstructedChanged parent " + __instance.transform.parent.name);
                      //__instance.transform.position += __instance.transform.forward * .05f;
                    }
                    Transform label = __instance.transform.Find("Label");
                    if (label)
                    {
                        Collider collider = label.GetComponent<Collider>();
                        if (collider)
                            UnityEngine.Object.Destroy(collider);
                    }
                }
                else if (techType == TechType.Locker && !Main.visibleLockerInteriorLoaded)
                {
                    //Transform parent = __instance.transform.parent;
                    if (parent && parent.name == "DecorativeLockerClosed(Clone)")
                        return;

                    //Main.logger.LogMessage("StorageContainer CreateContainer " + __instance.prefabRoot.name);
                    Transform doorRight = __instance.transform.Find("model/submarine_Storage_locker_big_01/submarine_Storage_locker_big_01_hinges_R");
                    if (!labelledLockers.Contains(__instance) && doorRight)// parent is null when built
                    {
                        //Main.logger.LogMessage("StorageContainer CreateContainer found door " + __instance.prefabRoot.name);
                        labelledLockers.Add(__instance);
                        UWE.CoroutineHost.StartCoroutine(AddLabel(doorRight, 0, __instance.transform));
                    }
                }
                //AddDebug("CreateContainer !!!");

            }

            [HarmonyPrefix, HarmonyPatch("OnHandHover")]
            static bool OnHandHoverPrefix(StorageContainer __instance, GUIHand hand)
            {
                if (!ConfigToEdit.newStorageUI.Value)
                    return true;

                if (!__instance.enabled)
                    return false;

                Constructable c = __instance.gameObject.GetComponent<Constructable>();
                if (c && !c.constructed)
                    return false;

                bool decoLocker = false;
                //AddDebug("StorageContainer OnHandHover " + __instance.name);
                //AddDebug("StorageContainer IsEmpty " + __instance.IsEmpty());
                Transform parent = __instance.transform.parent;
                TechTag parentTT = parent.GetComponent<TechTag>();
                if (parentTT)
                {
                    //AddDebug("parent TechTag " + parentTT.type.ToString());
                    decoLocker = parentTT.type.ToString() == "DecorativeLockerClosed";
                }
                StringBuilder stringBuilder = new StringBuilder(Language.main.Get(__instance.hoverText));
                stringBuilder.Append(" (");
                stringBuilder.Append(UI_Patches.leftHandButton);
                stringBuilder.Append(")\n");
                //string text = __instance.hoverText + " (" + TooltipFactory.stringLeftHand + ")\n";
                //string textEmpty = __instance.IsEmpty() ? "Empty" : string.Empty;
                string textEmpty = string.Empty;
                bool empty = __instance.IsEmpty();
                ColoredLabel label = null;
                PickupableStorage ps = null;
                Sign sign = null;
                if (decoLocker)
                    sign = parent.GetComponentInChildren<Sign>();
                else
                    sign = __instance.GetComponentInChildren<Sign>();

                if (__instance.GetComponent<SmallStorage>())
                {
                    //AddDebug("OnHandHover SmallStorage");
                    Transform tr = parent.Find("LidLabel/Label");
                    //cl = __instance.transform.parent.GetComponentInChildren<ColoredLabel>();
                    label = tr.GetComponent<ColoredLabel>();
                    tr = parent.Find("collider_main");
                    //ps = __instance.transform.parent.GetComponentInChildren<PickupableStorage>();
                    ps = tr.GetComponent<PickupableStorage>();
                }
                else if (parent.name == "docking_luggage_01_bag4(Clone)")
                {
                    ps = parent.GetComponentInChildren<PickupableStorage>();
                    //AddDebug("OnHandHover luggage " + ps.name);
                }
                else
                    label = __instance.GetComponentInChildren<ColoredLabel>();

                if (label && label.enabled)
                {
                    //AddDebug("ColoredLabel " + cl.stringEditLabel);
                    stringBuilder.Append(Language.main.Get(label.stringEditLabel));
                    stringBuilder.Append(" (");
                    stringBuilder.Append(UI_Patches.rightHandButton);
                    stringBuilder.Append(")");
                    //text  += Language.main.Get(cl.stringEditLabel) + " (" + TooltipFactory.stringRightHand+ ")";
                    if (GameInput.GetButtonDown(GameInput.Button.RightHand))
                        label.signInput.Select(true);
                }
                else if (sign && sign.enabled)
                {
                    //AddDebug("sign");
                    stringBuilder.Append(Language.main.Get("SmallLockerEditLabel"));
                    stringBuilder.Append(" (");
                    stringBuilder.Append(UI_Patches.rightHandButton);
                    stringBuilder.Append(")");
                    //text  += Language.main.Get(cl.stringEditLabel) + " (" + TooltipFactory.stringRightHand+ ")";
                    if (GameInput.GetButtonDown(GameInput.Button.RightHand))
                        sign.signInput.Select(true);
                }
                if (ps)
                {
                    bool canPickUp = empty || ConfigToEdit.canPickUpContainerWithItems.Value || Main.pickupFullCarryallIsLoaded;
                    //AddDebug("PickupableStorage cantPickupHoverText " + ps.cantPickupHoverText);
                    if (canPickUp)
                    {
                        if (label) // floating locker
                            stringBuilder.Append(UI_Patches.smallStorageString);
                        else // bag
                            stringBuilder.Append(UI_Patches.bagString);
                    }
                    else
                        textEmpty = Language.main.Get(ps.cantPickupClickText);

                    if (canPickUp && GameInput.GetButtonDown(GameInput.Button.AltTool))
                        ps.pickupable.OnHandClick(hand);
                }
                //HandReticle.main.SetText(HandReticle.TextType.Hand, stringBuilder.ToString(), true, GameInput.Button.RightHand);
                HandReticle.main.SetText(HandReticle.TextType.Hand, stringBuilder.ToString(), true);
                //AddDebug(stringBuilder.ToString());
                HandReticle.main.SetText(HandReticle.TextType.HandSubscript, empty ? "Empty" : textEmpty, true);
                HandReticle.main.SetIcon(HandReticle.IconType.Hand);
                return false;
            }
        }

        [HarmonyPatch(typeof(DeployableStorage))]
        public class DeployableStorage_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("Awake")]
            static void AwakePostfix(DeployableStorage __instance)
            {
                Pickupable p = __instance.GetComponent<Pickupable>();
                if (p && p.inventoryItem == null)
                { // fix bug: when game loads 1st_person_model used for containers in world
                    FPModel fPModel = __instance.GetComponent<FPModel>();
                    if (fPModel)
                        fPModel.SetState(false);
                }
                if (!ConfigToEdit.newStorageUI.Value)
                    return;
                //AddDebug("DeployableStorage Awake");
                //PickupableStorage ps = __instance.GetComponentInChildren<PickupableStorage>(true);
                LiveMixin lm = __instance.GetComponent<LiveMixin>();
                UnityEngine.Object.Destroy(lm);
                Transform tr = __instance.transform.Find("collider_main");
                if (tr)
                {
                    //AddDebug("DeployableStorage PickupableStorage");
                    Collider collider = tr.GetComponent<Collider>();
                    if (collider)
                        UnityEngine.Object.Destroy(collider);
                }
                //ColoredLabel cl = __instance.GetComponentInChildren<ColoredLabel>(true);
                tr = __instance.transform.Find("LidLabel/Label");
                if (tr)
                {
                    Collider collider = tr.GetComponent<Collider>();
                    if (collider)
                        UnityEngine.Object.Destroy(collider);
                }

                //Transform label = __instance.transform.Find("LidLabel");
                //if (label)
                {
                    //FollowTransform ft = label.GetComponent<FollowTransform>();
                    //if (ft)
                    //    ft.offsetPosition = new Vector3(0.03f, .04f, -0.04f);
                    //label.localPosition = new Vector3(0.03f, .04f, -0.04f);
                    //label.localPosition = new Vector3(0f, .031f, 0f);
                }

            }
            //[HarmonyPostfix, HarmonyPatch("Throw")]
            static void OnPickedUpPostfix(DeployableStorage __instance)
            { // does not run during loading if in inventory
                AddDebug("DeployableStorage Throw");
            }
            //[HarmonyPostfix, HarmonyPatch("OnToolUseAnim")]
            static void OnDroppedPostfix(DeployableStorage __instance)
            {
                AddDebug("DeployableStorage OnToolUseAnim");
            }
            //[HarmonyPostfix, HarmonyPatch("OnRightHandDown")]
            static void OnRightHandDownPostfix(DeployableStorage __instance)
            {
                AddDebug("DeployableStorage OnRightHandDown");
            }
        }

        [HarmonyPatch(typeof(Sign))]
        class Sign_Patch
        {
            [HarmonyPrefix]
            [HarmonyPatch("UpdateCollider")]
            static bool UpdateColliderPrefix(Sign __instance)
            { // avoid NRE 
                if (__instance.boxCollider == null)
                {
                    //AddDebug("Sign boxCollider == null");
                    return false;
                }
                return true;
            }

            //[HarmonyPrefix]
            //[HarmonyPatch("OnProtoDeserialize")]
            static bool OnProtoDeserializePrefix(Sign __instance, ProtobufSerializer serializer)
            { // fix NRE that prevents loading text color 
                //AddDebug(" Sign OnProtoDeserialize " );
                if (serializer != null && !Main.gameLoaded)
                {
                    //AddDebug("Add Sign");
                    savedSigns.Add(__instance);
                    return false;
                }
                if (savedSigns.Contains(__instance))
                {
                    //AddDebug("Fix Sign");
                    return true;
                }
                // dont allow fix from decorations mod to run. It removes text from my locker labels
                return false;
            }
        }

        [HarmonyPatch(typeof(uGUI_SignInput))]
        class SignInput_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("OnDeselect")]
            static void OnDeselectPostfix(uGUI_SignInput __instance)
            {
                if (!ConfigToEdit.newStorageUI.Value)
                    return;
                //AddDebug("uGUI_SignInput OnDeselect " + __instance.stringDefaultLabel);
                if (__instance.stringDefaultLabel == "SmallLockerDefaultLabel")
                {
                    if (__instance.inputField.characterLimit == 44 || __instance.inputField.characterLimit == 58)
                    {
                        //AddDebug("uGUI_SignInput OnDeselect locker " + __instance.inputField.characterLimit);
                        //bool cyclopsLocker = __instance.transform.parent.parent.GetComponent<CyclopsLocker>();
                        bool cyclops = __instance.transform.GetComponentInParent<SubControl>();
                        string key = cyclops ? GetLabelKeyCyclops(__instance.transform.parent.parent) : GetLabelKey(__instance.transform.parent.parent);
                        //AddDebug("key " + key);
                        string slot = SaveLoadManager.main.currentSlot;
                        if (!Main.configMain.lockerNames.ContainsKey(slot))
                            Main.configMain.lockerNames[slot] = new Dictionary<string, SavedLabel>();

                        Main.configMain.lockerNames[slot][key] = new SavedLabel(__instance.text, __instance.backgroundToggle.isOn, __instance.colorIndex, __instance.scaleIndex);
                    }
                }
            }

            //[HarmonyPrefix]
            //[HarmonyPatch("UpdateScale")]
            static bool UpdateScalePostfix(uGUI_SignInput __instance)
            {
                if (__instance.rt == null)
                    return false;

                float num = __instance.baseScale + __instance.scaleStep * __instance.scaleIndex;
                __instance.rt.localScale = new Vector3(num, num, 1f);
                return false;
            }
        }

        [HarmonyPatch(typeof(Constructable))]
        class Constructable_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("DeconstructAsync")]
            static void DeconstructPostfix(Constructable __instance)
            {
                if (!ConfigToEdit.newStorageUI.Value)
                    return;

                if (__instance.constructedAmount == 0f)
                {
                    if (__instance.techType == TechType.Locker || __instance.techType.ToString() == "DecorativeLockerClosed")
                    {
                        //AddDebug("Deconstruct " + __instance.constructedAmount);
                        string slot = SaveLoadManager.main.currentSlot;
                        if (Main.configMain.lockerNames.ContainsKey(slot))
                        {
                            string key = GetLabelKey(__instance.transform);
                            if (Main.configMain.lockerNames[slot].ContainsKey(key))
                            {
                                //AddDebug("Deconstruct saved locker ");
                                Main.configMain.lockerNames[slot].Remove(key);
                            }
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(SeaMoth), "OnUpgradeModuleChange")]
        class SeaMoth_OnUpgradeModuleChange_Patch
        {
            static void Postfix(SeaMoth __instance, int slotID, TechType techType, bool added)
            {
                if (!added || techType != TechType.VehicleStorageModule)
                    return;

                if (slotID == 0)
                {
                    Transform storage = __instance.transform.Find("Model/Submersible_SeaMoth_extras/Submersible_seaMoth_geo/seaMoth_storage_01_L_geo");
                    if (storage)
                    {
                        //AddDebug("fux left storage");
                        storage.localPosition = new Vector3(0.01f, 0f, 0f);
                        storage.localEulerAngles = new Vector3(0f, 0f, -0.6f);
                    }
                }
                else if (slotID == 1)
                {
                    Transform storage = __instance.transform.Find("Model/Submersible_SeaMoth_extras/Submersible_seaMoth_geo/seaMoth_storage_01_R_geo");
                    if (storage)
                    {
                        //AddDebug("fux right storage");
                        storage.localPosition = new Vector3(-0.01f, 0f, 0f);
                        storage.localEulerAngles = new Vector3(0f, 0f, 0.6f);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(PickupableStorage))]
        class PickupableStorage_Patch
        {
            [HarmonyPrefix, HarmonyPatch("OnHandHover")]
            static bool OnHandHoverPrefix(PickupableStorage __instance, GUIHand hand)
            {
                if (ConfigToEdit.canPickUpContainerWithItems.Value == false)
                    return true;

                __instance.pickupable.OnHandHover(hand);
                return false;
            }
            [HarmonyPrefix, HarmonyPatch("OnHandClick")]
            static bool CreateContainerPrefix(PickupableStorage __instance, GUIHand hand)
            {
                if (ConfigToEdit.canPickUpContainerWithItems.Value == false)
                    return true;

                __instance.pickupable.OnHandClick(hand);
                return false;
            }
        }


    }
}
