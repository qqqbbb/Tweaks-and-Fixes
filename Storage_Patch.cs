using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
using System.Text;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    public class Storage_Patch
    {
        static FMODAsset openSound;
        static FMODAsset closeSound;

        public class LockerDoorOpener: MonoBehaviour
        {
            public float startRotation;
            public float endRotation;
            public float t;
            public float duration = 1f;
            public float openAngle = 135f;
            public float doubleDoorOpenAngle = 90f;

            public IEnumerator Rotate(Transform door, bool playCloseSound = false)
            {
                while (t < duration)
                {
                    t += Time.deltaTime;
                    float f = t / duration;
                    float rotation = Mathf.Lerp(startRotation, endRotation, f);
                    //Main.Log("rotation " + rotation );
                    //AddDebug(" rotation " + rotation);
                    door.localEulerAngles = new Vector3(door.localEulerAngles.x, door.localEulerAngles.y,                    rotation);

                    if (endRotation == 0f)
                    {
                        if (playCloseSound && f > .62f && closeSound != null)
                        {
                            playCloseSound = false;
                            Utils.PlayFMODAsset(closeSound, door.transform);
                        }
                        else if (f > 1f)
                        {
                            ColoredLabel cl = door.GetComponentInChildren<ColoredLabel>();
                            Transform parent = door.transform.parent.parent.parent;
                            if (cl && parent)
                                cl.transform.SetParent(parent);
                        }
                    }
                    yield return null;
                }
            }

            public IEnumerator Rotate(Transform doorLeft, Transform doorRight, bool playCloseSound = false)
            {
                while (t < duration)
                {
                    t += Time.deltaTime;
                    float f = t / duration;
                    float rotation = Mathf.Lerp(startRotation, endRotation, f);
                    doorLeft.localEulerAngles = new Vector3(doorLeft.localEulerAngles.x, doorLeft.localEulerAngles.y, -rotation);
                    doorRight.localEulerAngles = new Vector3(doorRight.localEulerAngles.x, doorRight.localEulerAngles.y, rotation);
                    if (f > .62f && playCloseSound && closeSound != null)
                    {
                        playCloseSound = false;
                        Utils.PlayFMODAsset(closeSound, doorLeft.transform.parent);
                    }
                    yield return null;
                }
            }
        }

        static string GetKey(Transform door)
        {
            PrefabIdentifier pi = door.GetComponentInParent<PrefabIdentifier>();
            Vector3 pos = pi.transform.position;
            StringBuilder sb = new StringBuilder(Mathf.RoundToInt(pos.x).ToString());
            sb.Append("_");
            sb.Append(Mathf.RoundToInt(pos.y));
            sb.Append("_");
            sb.Append(Mathf.RoundToInt(pos.z));
            return sb.ToString();
        }

        static string GetKeyCyclops(Transform door)
        {
            //SubControl sc = door.GetComponentInParent<SubControl>();
            PingInstance pi = door.GetComponentInParent<PingInstance>();
            StorageContainer st = door.GetComponentInParent<StorageContainer>();
            Vector3 pos = st.transform.localPosition;
            if (st.GetComponent<CyclopsLocker>())
                pos = st.transform.parent.parent.localPosition;
            StringBuilder sb = new StringBuilder(pi._label);
            sb.Append("_");
            //sb.Append(door.transform.parent.parent.name); 
            sb.Append(Mathf.RoundToInt(pos.x));
            sb.Append("_");
            sb.Append(Mathf.RoundToInt(pos.y));
            sb.Append("_");
            sb.Append(Mathf.RoundToInt(pos.z));
            return sb.ToString();
        }

        static IEnumerator AddLabel(Transform door, TechType techType = TechType.None)
        {
            if (door.parent == null)
                yield return null;

            bool cyclops = door.GetComponentInParent<SubControl>();
            //AddDebug("AddLabel " + cyclops + " " + techType);
            GameObject prefabForTechType = CraftData.GetPrefabForTechType(TechType.Sign);
            if (prefabForTechType == null)
                yield break;

            bool cyclopsLocker = cyclops && techType == TechType.None;
            GameObject go = Utils.CreatePrefab(prefabForTechType);
            go.transform.position = door.transform.position;
            go.transform.SetParent(door);
            go.transform.localPosition = cyclopsLocker ? new Vector3(-.03f, -.37f, .45f) : new Vector3(.32f, -.58f, .26f);
            go.transform.localEulerAngles = cyclopsLocker ? new Vector3(0f, 80f, 90f) : new Vector3(0f, 90f, 90f);
            Transform tr = go.transform.Find("SignMesh");
            UnityEngine.Object.Destroy(tr.gameObject);
            tr = go.transform.Find("Trigger");
            UnityEngine.Object.Destroy(tr.gameObject);
            tr = go.transform.Find("UI/Base/Up");
            UnityEngine.Object.Destroy(tr.gameObject);
            tr = go.transform.Find("UI/Base/Down");
            UnityEngine.Object.Destroy(tr.gameObject);
            tr = go.transform.Find("UI/Base/Left");
            UnityEngine.Object.Destroy(tr.gameObject);
            tr = go.transform.Find("UI/Base/Right");
            UnityEngine.Object.Destroy(tr.gameObject);
            if (techType == TechType.Locker)
            {
                //tr = go.transform.Find("UI/Base/BackgroundToggle");
                tr = go.transform.Find("UI/Base/Minus");
                tr.localPosition = new Vector3(tr.localPosition.x - 130f, tr.localPosition.y - 320f, tr.localPosition.z);
                tr = go.transform.Find("UI/Base/Plus");
                tr.localPosition = new Vector3(tr.localPosition.x + 130f, tr.localPosition.y - 320f, tr.localPosition.z);
            }
            Constructable c = go.GetComponent<Constructable>();
            UnityEngine.Object.Destroy(c);
            TechTag tt = go.GetComponent<TechTag>();
            UnityEngine.Object.Destroy(tt);
            ConstructableBounds cb = go.GetComponent<ConstructableBounds>();
            UnityEngine.Object.Destroy(cb);
            PrefabIdentifier pi = go.GetComponent<PrefabIdentifier>();
            UnityEngine.Object.Destroy(pi); 
            uGUI_SignInput si = go.GetComponentInChildren<uGUI_SignInput>();
            si.stringDefaultLabel = "SmallLockerDefaultLabel";
            si.inputField.text = Language.main.Get(si.stringDefaultLabel);
            si.inputField.characterLimit = cyclopsLocker ? 44 : 58;
            //if (cyclopsLocker) 
            //    si.scaleIndex = -2; // range -3 3 
            //AddDebug("cyclopsLocker " + cyclopsLocker);
            string slot = SaveLoadManager.main.currentSlot;
            if (Main.config.lockerNames.ContainsKey(slot))
            {
                string key = cyclops ? GetKeyCyclops(door) : GetKey(door);
                if (Main.config.lockerNames[slot].ContainsKey(key))
                {
                    SavedLabel sl = Main.config.lockerNames[slot][key];
                    si.inputField.text = sl.text;
                    si.colorIndex = sl.color;
                    si.SetBackground(sl.background);
                    si.scaleIndex = sl.scale;
                }
            }
        }

        [HarmonyPatch(typeof(CyclopsLocker))]
        public class CyclopsLocker_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("Start")]
            static void StartPostfix(CyclopsLocker __instance)
            { // cyclops prefab always loads
                //AddDebug("CyclopsLocker Start");
                openSound = __instance.openSound;
                closeSound = __instance.closeSound;
            }
        }

        //[HarmonyPatch(typeof(ColoredLabel))]
        class ColoredLabel_Patch
        {
            //[HarmonyPrefix]
            //[HarmonyPatch("OnProtoSerialize")]
            static bool OnProtoSerializePrefix(ColoredLabel __instance)
            {
                //AddDebug("locker ColoredLabel OnProtoSerialize");
                if (__instance.gameObject.name == "submarine_Storage_locker_big_01_hinges_R")
                {
                    return false;
                }
                return true;
            }
            //[HarmonyPrefix]
            //[HarmonyPatch("OnProtoDeserialize")]
            static bool OnProtoDeserializePrefix(ColoredLabel __instance)
            {
                //AddDebug("locker ColoredLabel OnProtoDeserialize");
                if (__instance.gameObject.name == "submarine_Storage_locker_big_01_hinges_R")
                {

                    return false;
                }
                return true;
            }
            //[HarmonyPostfix]
            //[HarmonyPatch("OnProtoDeserialize")]
            static void OnProtoDeserializePostfix(ColoredLabel __instance)
            {
                //AddDebug("locker ColoredLabel OnProtoDeserialize");
                TechTag techTag = __instance.transform.parent.GetComponent<TechTag>();
                if (techTag == null)
                    return;
                //AddDebug("ColoredLabel OnProtoDeserialize " + techTag.type);
                if (techTag.type == TechType.SmallLocker)
                {
                    //Transform door = __instance.transform.Find("model/submarine_locker_02/submarine_locker_02_door");
                    //if (door)
                    //    __instance.transform.SetParent(door.transform);
                }
            }
            //[HarmonyPostfix]
            //[HarmonyPatch("OnEnable")]
            static void OnEnablePostfix(ColoredLabel __instance)
            { 
              //AddDebug("locker ColoredLabel OnProtoDeserialize");
                Transform parent = __instance.transform.parent;
                TechTag techTag = parent.GetComponent<TechTag>();
                if (techTag == null)
                    return;
                //AddDebug("ColoredLabel OnEnable " + techTag.type);
                if (techTag.type == TechType.SmallLocker)
                {
                    Transform door = parent.Find("model/submarine_locker_02/submarine_locker_02_door");
                    if (door)
                        __instance.transform.SetParent(door.transform);
                }
            }
        }

        [HarmonyPatch(typeof(DeployableStorage))]
        public class DeployableStorage_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("Awake")]
            static void AwakePostfix(DeployableStorage __instance)
            {
                PickupableStorage ps = __instance.GetComponentInChildren<PickupableStorage>(true);
                if (ps)
                {
                    Collider collider = ps.GetComponent<Collider>();
                    if (collider)
                        UnityEngine.Object.Destroy(collider);
                }
                ColoredLabel cl = __instance.GetComponentInChildren<ColoredLabel>(true);
                if (cl)
                {
                    Collider collider = cl.GetComponent<Collider>();
                    if (collider)
                        UnityEngine.Object.Destroy(collider);
                }
                Pickupable p = __instance.GetComponent<Pickupable>();
                if (p && p.inventoryItem == null)
                { // fix: when game loads 1st_person_model used instead of 3rd_person_model
                    // should use FPModel.SetState
                    Transform tpm = __instance.transform.Find("3rd_person_model");
                    Transform fpm = __instance.transform.Find("1st_person_model");
                    if (tpm && fpm)
                    {
                        fpm.gameObject.SetActive(false);
                        tpm.gameObject.SetActive(true);
                    }
                }
                Transform label = __instance.transform.Find("LidLabel");
                if (label)
                {
                    FollowTransform ft = label.GetComponent<FollowTransform>();
                    if (ft)
                    {
                        //ft.enabled = false;
                        UnityEngine.Object.Destroy(ft);
                    }
                    label.localPosition = new Vector3(0f, .031f, 0f);
                }
            }

            [HarmonyPostfix]
            [HarmonyPatch("OnPickedUp")]
            static void OnPickedUpPostfix(DeployableStorage __instance)
            { // does not run during loading if in inventory
                //AddDebug("DeployableStorage OnPickedUp");
                Transform label = __instance.transform.Find("LidLabel");
                if (label)
                {
                    label.localPosition = new Vector3(0.02f, .031f, -0.04f);
                }
            }

            [HarmonyPostfix]
            [HarmonyPatch("OnDropped")]
            static void OnDroppedPostfix(DeployableStorage __instance)
            {
                //AddDebug("DeployableStorage OnDropped");
                Transform label = __instance.transform.Find("LidLabel");
                if (label)
                {
                    label.localPosition = new Vector3(0f, .031f, 0f);
                }
            }

        }

        [HarmonyPatch(typeof(StorageContainer))]
        class StorageContainer_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("Awake")]
            static void AwakePostfix(StorageContainer __instance)
            {
                TechTag techTag = __instance.GetComponent<TechTag>();
                if (techTag)
                {
                    //AddDebug("StorageContainer Awake " + techTag.type);
                    if (techTag.type == TechType.SmallLocker)
                    {
                        ColoredLabel cl = __instance.GetComponentInChildren<ColoredLabel>(true);
                        if (cl)
                        {
                            Collider collider = cl.GetComponent<Collider>();
                            if (collider)
                                UnityEngine.Object.Destroy(collider);
                        }
                    }
                    else if (techTag.type == TechType.Locker)
                    {
                        Transform doorRight = __instance.transform.Find("model/submarine_Storage_locker_big_01/submarine_Storage_locker_big_01_hinges_R");
                        if (doorRight)
                        { // parent is null
                            UWE.CoroutineHost.StartCoroutine(AddLabel(doorRight, TechType.Locker));
                            //AddLabel(doorRight, inSub, TechType.Locker);
                        }
                    }
                }
                else if (__instance.GetComponent<CyclopsLocker>())
                {
                    PrefabIdentifier pi = __instance.GetComponentInParent<PrefabIdentifier>();
                    //AddDebug("CyclopsLocker " + pi.name);
                    //dont add to prefab
                    if (pi.name == "Cyclops-MainPrefab(Clone)")
                        //AddLabel(__instance.transform);
                        UWE.CoroutineHost.StartCoroutine(AddLabel(__instance.transform));
                }
              
            }

            [HarmonyPostfix]
            [HarmonyPatch("Open", new Type[] { typeof(Transform) })]
            static void OpenPostfix(StorageContainer __instance, Transform useTransform)
            {
                TechTag techTag = __instance.GetComponent<TechTag>();
                if (techTag == null)
                    return;
                if (techTag.type == TechType.SmallLocker)
                {
                    Transform door = __instance.transform.Find("model/submarine_locker_02/submarine_locker_02_door");
                    if (door)
                    {
                        //AddDebug("SmallLocker Open ");
                        ColoredLabel cl = __instance.GetComponentInChildren<ColoredLabel>(true);
                        if (cl)
                             cl.transform.SetParent(door.transform);
                        LockerDoorOpener rotater = __instance.gameObject.EnsureComponent<LockerDoorOpener>();
                        rotater.startRotation = door.transform.localEulerAngles.z;
                        rotater.endRotation = rotater.startRotation + rotater.openAngle;
                        rotater.t = 0f;
                        rotater.StartCoroutine(rotater.Rotate(door));
                        if (openSound != null)
                            Utils.PlayFMODAsset(openSound, __instance.transform);
                    }
                }
                else if (techTag.type == TechType.Locker)
                {
                    Transform doorLeft = __instance.transform.Find("model/submarine_Storage_locker_big_01/submarine_Storage_locker_big_01_hinges_L");
                    Transform doorRight = __instance.transform.Find("model/submarine_Storage_locker_big_01/submarine_Storage_locker_big_01_hinges_R");
                    if (doorLeft && doorRight)
                    {
                        LockerDoorOpener rotater = __instance.gameObject.EnsureComponent<LockerDoorOpener>();
                        rotater.startRotation = doorLeft.transform.localEulerAngles.z;
                        rotater.endRotation = rotater.startRotation + rotater.doubleDoorOpenAngle;
                        rotater.t = 0f;
                        rotater.StartCoroutine(rotater.Rotate(doorLeft, doorRight));
                        if (openSound != null)
                            Utils.PlayFMODAsset(openSound, __instance.transform);
                    }
                }
            }

            [HarmonyPostfix]
            [HarmonyPatch("OnClose")]
            static void OnClosePostfix(StorageContainer __instance)
            {
                TechTag techTag = __instance.GetComponent<TechTag>();
                if (techTag == null)
                    return;

                if (techTag.type == TechType.SmallLocker)
                {
                    Transform door = __instance.transform.Find("model/submarine_locker_02/submarine_locker_02_door");
                    if (door)
                    {
                        //AddDebug("SmallLocker OnClose ");
                        LockerDoorOpener rotater = __instance.gameObject.EnsureComponent<LockerDoorOpener>();
                        rotater.startRotation = door.transform.localEulerAngles.z;
                        rotater.endRotation = 0f;
                        rotater.t = 0f;
                        rotater.StartCoroutine(rotater.Rotate(door, true));
                    }
                }
                else if (techTag.type == TechType.Locker)
                {
                    Transform doorLeft = __instance.transform.Find("model/submarine_Storage_locker_big_01/submarine_Storage_locker_big_01_hinges_L");
                    Transform doorRight = __instance.transform.Find("model/submarine_Storage_locker_big_01/submarine_Storage_locker_big_01_hinges_R");
                    if (doorLeft && doorRight)
                    {
                        LockerDoorOpener rotater = __instance.gameObject.EnsureComponent<LockerDoorOpener>();
                        rotater.startRotation = doorRight.transform.localEulerAngles.z;
                        rotater.endRotation = 0f;
                        rotater.t = 0f;
                        rotater.StartCoroutine(rotater.Rotate(doorLeft, doorRight, true));
                    }
                }
            }

            [HarmonyPrefix]
            [HarmonyPatch( "OnHandHover")]
            static bool OnHandHoverPrefix(StorageContainer __instance, GUIHand hand)
            {
                if (!__instance.enabled)
                    return false;
                Constructable c = __instance.gameObject.GetComponent<Constructable>();
                if (c && !c.constructed)
                    return false;

                //Vector3 pos = __instance.transform.localPosition;
                //AddDebug("pos " + (int)pos.x + " " + (int)pos.y + " " + (int)pos.z);
                //TechType techType = __instance.GetTechType();
                StringBuilder stringBuilder = new StringBuilder(__instance.hoverText);
                stringBuilder.Append(" (");
                stringBuilder.Append(TooltipFactory.stringLeftHand);
                stringBuilder.Append(")\n");
                //string text = __instance.hoverText + " (" + TooltipFactory.stringLeftHand + ")\n";
                //string textEmpty = __instance.IsEmpty() ? "Empty" : string.Empty;
                string textEmpty = string.Empty;
                bool empty = __instance.IsEmpty();
                ColoredLabel cl = null;
                PickupableStorage ps = null;
                Sign sign = __instance.GetComponentInChildren<Sign>();
                if (__instance.GetComponent<SmallStorage>())
                {
                    cl = __instance.transform.parent.GetComponentInChildren<ColoredLabel>();
                    ps = __instance.transform.parent.GetComponentInChildren<PickupableStorage>();
                }
                else
                    cl = __instance.GetComponentInChildren<ColoredLabel>();

                if (cl && cl.enabled)
                {
                    //AddDebug("ColoredLabel " + cl.stringEditLabel);
                    stringBuilder.Append(Language.main.Get(cl.stringEditLabel));
                    stringBuilder.Append(" (");
                    stringBuilder.Append(TooltipFactory.stringRightHand);
                    stringBuilder.Append(")");
                    //text  += Language.main.Get(cl.stringEditLabel) + " (" + TooltipFactory.stringRightHand+ ")";
                    if (GameInput.GetButtonDown(GameInput.Button.RightHand))
                        cl.signInput.Select(true);
                }
                else if (sign && sign.enabled)
                {
                    //AddDebug("sign");
                    stringBuilder.Append(Language.main.Get("SmallLockerEditLabel"));
                    stringBuilder.Append(" (");
                    stringBuilder.Append(TooltipFactory.stringRightHand);
                    stringBuilder.Append(")");
                    //text  += Language.main.Get(cl.stringEditLabel) + " (" + TooltipFactory.stringRightHand+ ")";
                    if (GameInput.GetButtonDown(GameInput.Button.RightHand))
                        sign.signInput.Select(true);
                }
                if (ps)
                {
                    //AddDebug("PickupableStorage cantPickupHoverText " + ps.cantPickupHoverText);
                    if (empty)
                    {
                        //stprageString = text;
                        stringBuilder.Append(UI_Patches.smallStorageString);
                        //text += smallStorageString;
                    }
                    else
                        textEmpty = Language.main.Get(ps.cantPickupClickText);

                    if (empty && GameInput.GetButtonDown(GameInput.Button.AltTool))
                        ps.pickupable.OnHandClick(hand);
                }
                HandReticle.main.SetInteractTextRaw(stringBuilder.ToString(), textEmpty);
                //HandReticle.main.SetInteractText(text, __instance.IsEmpty() ? "Empty" : string.Empty);
                HandReticle.main.SetIcon(HandReticle.IconType.Hand);
                return false;
            }
        }

        [HarmonyPatch(typeof(Sign))]
        class Sign_Patch
        {
            [HarmonyPrefix]
            [HarmonyPatch("UpdateCollider")]
            static bool UpdateColliderPrefix(Sign __instance)
            {
                if (__instance.boxCollider == null)
                {
                    //AddDebug("Sign boxCollider == null"); 
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(uGUI_SignInput))]
        class SignInput_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("OnDeselect")]
            static void OnDeselectPostfix(uGUI_SignInput __instance)
            {
                //AddDebug("uGUI_SignInput OnDeselect " + __instance.stringDefaultLabel);
                if (__instance.stringDefaultLabel == "SmallLockerDefaultLabel")
                {
                    if (__instance.inputField.characterLimit == 44 || __instance.inputField.characterLimit == 58)
                    {
                        //AddDebug("uGUI_SignInput OnDeselect locker " + __instance.inputField.characterLimit);
                        //bool cyclopsLocker = __instance.transform.parent.parent.GetComponent<CyclopsLocker>();
                        bool cyclops = __instance.transform.GetComponentInParent<SubControl>();
                        string key = cyclops ? GetKeyCyclops(__instance.transform.parent.parent) : GetKey(__instance.transform.parent.parent);
                        //AddDebug("key " + key);
                        string slot = SaveLoadManager.main.currentSlot;
                        if (!Main.config.lockerNames.ContainsKey(slot))
                            Main.config.lockerNames[slot] = new Dictionary<string, SavedLabel>();

                        Main.config.lockerNames[slot][key] = new SavedLabel(__instance.text, __instance.backgroundToggle.isOn, __instance.colorIndex, __instance.scaleIndex);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Constructable))]
        class Constructable_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("Deconstruct")]
            static void DeconstructPostfix(Constructable __instance)
            {
                if (__instance.techType == TechType.Locker && __instance.constructedAmount == 0f)
                {
                    //AddDebug("Deconstruct " + __instance.constructedAmount);

                    string slot = SaveLoadManager.main.currentSlot;
                    if (Main.config.lockerNames.ContainsKey(slot))
                    {
                        string key = GetKey(__instance.transform);
                        if (Main.config.lockerNames[slot].ContainsKey(key))
                        {
                            //AddDebug("Deconstruct saved locker ");
                            Main.config.lockerNames[slot].Remove(key);
                        }
                    }
                }
            }
        }

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


    }
}
