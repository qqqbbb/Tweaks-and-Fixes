using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
using System.Text;
using static ErrorMessage;
using static UWE.CubeFace;

namespace Tweaks_Fixes
{
    public class Storage_Patch
    {
        static FMODAsset openSound;
        static FMODAsset closeSound;
        public static List<Sign> savedSigns = new List<Sign>();
        public static List<StorageContainer> labelledLockers = new List<StorageContainer>();

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
        
        public class LockerDoorOpener: MonoBehaviour
        {
            public float startRotation;
            public float endRotation;
            public float t;
            public float duration = 1f;
            public float openAngle = 135f;
            public float doubleDoorOpenAngle = 90f;

            public IEnumerator Rotate(Transform door, bool playCloseSound = false, bool deco = false)
            {
                while (t < duration)
                {
                    t += Time.deltaTime;
                    float f = t / duration;
                    float rotation = Mathf.Lerp(startRotation, endRotation, f);
                    //Main.Log("rotation " + rotation );
                    //AddDebug(" rotation " + rotation);
                    if (deco)
                        door.localEulerAngles = new Vector3(door.localEulerAngles.x, rotation, door.localEulerAngles.z);
                    //else if (test)
                    //    door.localEulerAngles = new Vector3(door.localEulerAngles.x, rotation, door.localEulerAngles.z);
                    else
                        door.localEulerAngles = new Vector3(door.localEulerAngles.x, door.localEulerAngles.y, rotation);

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

        public class DecoLockerMyController : HandTarget, IHandTarget
        {
            private StorageContainer _storageContainer = null;

            public override void Awake()
            {
                //base.Awake();
                //if (gameObject.transform.parent == null)
                //    AddDebug("DecoLockerMyController Awake parent null");
                //else
                HandTarget[] handTargets = transform.GetComponents<HandTarget>();
                HandTarget oldController = null;
                foreach (HandTarget handTarget in handTargets)
                {
                    if (handTarget != this)
                        oldController = handTarget;
                }
                if (oldController)
                    Destroy(oldController);
                _storageContainer = gameObject.transform.GetComponentInChildren<StorageContainer>();
            }

            public void OnHandClick(GUIHand hand)
            {
                if (!base.enabled)
                    return;

                if (_storageContainer)
                    _storageContainer.OnHandClick(hand);
            }

            public void OnHandHover(GUIHand hand)
            {
                if (!base.enabled)
                    return;

                //AddDebug("DecoLockerMyController OnHandHover " + transform.name);
                //AddDebug("DecoLockerMyController OnHandHover parent " + transform.parent.name);
                if (_storageContainer == null)
                {
                    //AddDebug("DecoLockerMyController OnHandHover _storageContainer == null " + transform.name);
                    _storageContainer = gameObject.transform.GetComponentInChildren<StorageContainer>();
                }
                else
                    _storageContainer.OnHandHover(hand);
            }
        }

        static string GetKey(Transform door)
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
            return sb.ToString();
        }

        static string GetKeyCyclops(Transform door)
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

        static IEnumerator AddLabel(Transform door, int type, Transform locker)
        {
            if (door.GetComponentInChildren<Sign>())
                yield break;

            yield return new WaitUntil(() => Main.gameLoaded == true);

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
            if (type == 0) // locker
            {
                LiveMixin lm = locker.GetComponent<LiveMixin>();
                //if (lm == null)
                //    AddDebug("AddLabel LiveMixin null");

                UnityEngine.Object.Destroy(lm);
                sign.transform.localPosition = new Vector3(.32f, -.58f, .26f);
                sign.transform.localEulerAngles = new Vector3(0f, 90f, 90f);
                tr = sign.transform.Find("UI/Base/Minus");
                tr.localPosition = new Vector3(tr.localPosition.x - 130f, tr.localPosition.y - 320f, tr.localPosition.z);
                tr = sign.transform.Find("UI/Base/Plus");
                tr.localPosition = new Vector3(tr.localPosition.x + 130f, tr.localPosition.y - 320f, tr.localPosition.z);
            }
            else if (type == 1) // cyclops locker
            {
                sign.transform.localPosition = new Vector3(-.03f, -.37f, .45f);
                sign.transform.localEulerAngles = new Vector3(0f, 80f, 90f);
            }
            else if (type == 2) // decorations locker
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

            if (type == 0)
                signInput.inputField.characterLimit = 58;
            else if (type == 1 || type == 2)
                signInput.inputField.characterLimit = 44;
            //    si.scaleIndex = -2; // range -3 3 
            string slot = SaveLoadManager.main.currentSlot;
            if (Main.config.lockerNames.ContainsKey(slot))
            {
                if (locker.parent && locker.parent.GetComponent<SubControl>())
                {
                    //AddDebug("AddLabel  parent is cyclops " + type);
                    type = 1;
                }
                string key = string.Empty;
                if (type == 0 || type == 2)
                    key = GetKey(door);
                else if (type == 1)
                    key = GetKeyCyclops(door);

                if (Main.config.lockerNames[slot].ContainsKey(key))
                {
                    SavedLabel sl = Main.config.lockerNames[slot][key];
                    signInput.inputField.text = sl.text;
                    signInput.colorIndex = sl.color;
                    signInput.SetBackground(sl.background);
                    signInput.scaleIndex = sl.scale;
                    //AddDebug("saved text " + sl.text);
                }
            }
            //AddDebug("AddLabel done " + type);
        }
        
        [HarmonyPatch(typeof(DeployableStorage))]
        public class DeployableStorage_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("Awake")]
            static void AwakePostfix(DeployableStorage __instance)
            {
                if (!Main.config.newStorageUI)
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
                Pickupable p = __instance.GetComponent<Pickupable>();
                if (p && p.inventoryItem == null)
                { // fix: when game loads 1st_person_model used instead of 3rd_person_model
                    FPModel fPModel = __instance.GetComponent<FPModel>();
                    if (fPModel)
                        fPModel.SetState(false);
                    
                    //Transform tpm = __instance.transform.Find("3rd_person_model");
                    //Transform fpm = __instance.transform.Find("1st_person_model");
                    //if (tpm && fpm)
                    //{
                        //fpm.gameObject.SetActive(false);
                        //tpm.gameObject.SetActive(true);
                    //}
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

            //[HarmonyPostfix]
            //[HarmonyPatch("OnPickedUp")]
            static void OnPickedUpPostfix(DeployableStorage __instance)
            { // does not run during loading if in inventory
   
                Transform label = __instance.transform.Find("LidLabel");
                if (label)
                {
                    //AddDebug("DeployableStorage OnPickedUp");
                    //label.localPosition = new Vector3(0.02f, .031f, -0.04f);

                }
            }
            //[HarmonyPostfix]
            //[HarmonyPatch("OnDropped")]
            static void OnDroppedPostfix(DeployableStorage __instance)
            {

                Transform label = __instance.transform.Find("LidLabel");
                if (label)
                {
                    //AddDebug("DeployableStorage OnDropped");
                    //label.localPosition = new Vector3(0f, .031f, 0f);
                    //Transform lid = __instance.transform.Find("3rd_person_model/floating_storage_cube_tp/Floating_storage_lid_geo");
                    //if (lid)
                    //{
                    //    label.SetParent(lid);
                    //}
                }
            }

        }

        [HarmonyPatch(typeof(StorageContainer))]
        class StorageContainer_Patch
        {
            //[HarmonyPostfix]
            //[HarmonyPatch("OnConstructedChanged")]
            static void OnConstructedChangedPostfix(StorageContainer __instance, bool constructed)
            {
                if (!constructed)
                    return;

                TechTag techTag = __instance.GetComponent<TechTag>();
                //if (techTag)
                //    AddDebug(__instance.name + "  OnConstructedChanged  " + techTag.type);
                //else
                //    AddDebug(__instance.name + "  OnConstructedChanged  ");
                //if (__instance.transform.parent)
                //    AddDebug(__instance.name + "  OnConstructedChanged parent  " + __instance.transform.parent.name);
                if (techTag)
                {
                    if (techTag.type == TechType.SmallLocker)
                    {
                        //AddDebug("StorageContainer OnConstructedChanged " + __instance.prefabRoot.name);
                        if (Main.gameLoaded && __instance.transform.parent && __instance.transform.parent.name == "Cyclops-MainPrefab(Clone)")
                        { // collision does not match mesh. Can see it after fixing cyclops collision. move it so cant see it when outside
                            //AddDebug("StorageContainer OnConstructedChanged parent " + __instance.transform.parent.name);
                            __instance.transform.position += __instance.transform.forward * .05f;
                        }
                        Transform label = __instance.transform.Find("Label");
                        if (label)
                        {
                            Collider collider = label.GetComponent<Collider>();
                            if (collider)
                                UnityEngine.Object.Destroy(collider);
                        }
                    }
                    else if (techTag.type == TechType.Locker && !Main.visibleLockerInteriorLoaded)
                    {
                        Transform parent = __instance.transform.parent;
                        if (parent && parent.name == "DecorativeLockerClosed(Clone)")
                            return;

                        Transform doorRight = __instance.transform.Find("model/submarine_Storage_locker_big_01/submarine_Storage_locker_big_01_hinges_R");
                        if (doorRight)// parent is null when built
                        {
                            UWE.CoroutineHost.StartCoroutine(AddLabel(doorRight, 0, __instance.transform));
                        }
                    }

                }

            }
          
            [HarmonyPostfix]
            [HarmonyPatch("CreateContainer")]
            static void CreateContainerPostfix(StorageContainer __instance)
            { // runs twice on game load
                if (!Main.config.newStorageUI)
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
                        UWE.CoroutineHost.StartCoroutine(AddLabel(decoDoor, 2, __instance.transform.parent));
                    }
                }
                TechTag techTag = __instance.GetComponent<TechTag>();
                //if (techTag)
                //    AddDebug(__instance.name + "  OnConstructedChanged  " + techTag.type);
                //else
                //    AddDebug(__instance.name + "  OnConstructedChanged  ");
                //if (__instance.transform.parent)
                //    AddDebug(__instance.name + "  OnConstructedChanged parent  " + __instance.transform.parent.name);
                if (techTag)
                {
                    if (techTag.type == TechType.SmallLocker)
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
                    else if (techTag.type == TechType.Locker && !Main.visibleLockerInteriorLoaded)
                    {
                        //Transform parent = __instance.transform.parent;
                        if (parent && parent.name == "DecorativeLockerClosed(Clone)")
                            return;

                        //AddDebug("StorageContainer CreateContainer " + __instance.prefabRoot.name);
                        Transform doorRight = __instance.transform.Find("model/submarine_Storage_locker_big_01/submarine_Storage_locker_big_01_hinges_R");
                        if (!labelledLockers.Contains(__instance) && doorRight)// parent is null when built
                        {
                            labelledLockers.Add(__instance);
                            UWE.CoroutineHost.StartCoroutine(AddLabel(doorRight, 0, __instance.transform));
                        }
                    }
                }
            }
         
            [HarmonyPostfix]
            [HarmonyPatch("Awake")]
            static void AwakePostfix(StorageContainer __instance)
            {
                if (!Main.config.newStorageUI)
                    return;
                //TechTag techTag = __instance.GetComponent<TechTag>();
                //AddDebug("StorageContainer Awake " + __instance.prefabRoot.name);
                //if (__instance.transform.parent)
                //    AddDebug("StorageContainer Awake parent " + __instance.transform.parent.name);
                //if (techTag && techTag.type == TechType.SmallLocker)
                //{
                //}
                if (__instance.name == "submarine_locker_01_door")
                {
                    PrefabIdentifier pi = __instance.GetComponentInParent<PrefabIdentifier>();
                    //AddDebug("CyclopsLocker  " + pi.name);
                    if (pi.name == "Cyclops-MainPrefab(Clone)") // dont touch prefab
                        UWE.CoroutineHost.StartCoroutine(AddLabel(__instance.transform, 1, __instance.transform));
                }
                if (openSound == null)
                {
                    openSound = ScriptableObject.CreateInstance<FMODAsset>();
                    openSound.path = "event:/sub/cyclops/locker_open";
                    openSound.id = "{c97d1fdf-ea26-4b19-8358-7f6ea77c3763}";
                    closeSound = ScriptableObject.CreateInstance<FMODAsset>();
                    closeSound.path = "event:/sub/cyclops/locker_close";
                    closeSound.id = "{16eb5589-e341-41cb-9c88-02cb4e3da44a}";
                }
            }

            [HarmonyPostfix]
            [HarmonyPatch("Open", new Type[] { typeof(Transform) })]
            static void OpenPostfix(StorageContainer __instance, Transform useTransform)
            {
                if (!Main.config.newStorageUI)
                    return;

                TechTag techTag = __instance.GetComponent<TechTag>();
                if (techTag == null)
                    techTag = __instance.transform.parent.GetComponent<TechTag>();

                //AddDebug(" Open  " + __instance.name);
                //AddDebug(" Open  useTransform " + useTransform.name);
                //if (__instance.transform.parent.name == "upgrade_geoHldr")
                //{
                //    Transform door = __instance.transform.parent.Find("Exosuit_01_storage");
                //    if (door)
                //    {
                //        AddDebug("Exosuit_01_storage " + door.transform.localEulerAngles);
                //    }
                //}
                if (techTag == null)
                    return;

                string techType = techTag.type.ToString();
                //AddDebug(" Open " + techTag.type + " " + useTransform.name);
                if (techTag.type == TechType.SmallLocker || techType == "RadLocker")
                {
                    OpenWallLocker(__instance);
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
                //AddDebug("StorageContainer open " + name);
                else if(techType == "DecorativeLockerClosed")
                {
                    Transform door = __instance.transform.parent.Find("submarine_locker_03_door_01/locker_04_door");
                    if (door)
                    {
                        //AddDebug("DecorativeLockerClosed");
                        LockerDoorOpener rotater = __instance.gameObject.EnsureComponent<LockerDoorOpener>();
                        rotater.startRotation = door.transform.localEulerAngles.z;
                        rotater.endRotation = rotater.startRotation + rotater.openAngle;
                        rotater.t = 0f;
                        rotater.StartCoroutine(rotater.Rotate(door, false, true));
                        if (openSound != null)
                            Utils.PlayFMODAsset(openSound, __instance.transform);
                    }
                }
                return;
                //else if (name == "Autosorter")
                {
                    Transform door = __instance.transform.Find("model/submarine_locker_02/submarine_locker_02_door");
                    if (door)
                    {
                        Transform canvas = __instance.transform.Find("Canvas");
                        FollowTransform ft = canvas.gameObject.EnsureComponent<FollowTransform>();
                        ft.parent = door;
                        //ft.keepRotation = true;
                        //AddDebug("Autosorter Open ");
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
            }

            private static void OpenWallLocker(StorageContainer __instance)
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

            [HarmonyPostfix]
            [HarmonyPatch("OnClose")]
            static void OnClosePostfix(StorageContainer __instance)
            {
                if (!Main.config.newStorageUI)
                    return;

                TechTag techTag = __instance.GetComponent<TechTag>();
                if (techTag == null)
                    techTag = __instance.transform.parent.GetComponent<TechTag>();

                if (techTag == null)
                    return;

                string techType = techTag.type.ToString();
                if (techTag.type == TechType.SmallLocker || techType == "RadLocker")
                {
                    CloseWallLocker(__instance);
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
                else if (techType == "DecorativeLockerClosed")
                {
                    Transform door = __instance.transform.parent.Find("submarine_locker_03_door_01/locker_04_door");
                    if (door)
                    {
                        //AddDebug("DecorativeLockerClosed");
                        LockerDoorOpener rotater = __instance.gameObject.EnsureComponent<LockerDoorOpener>();
                        rotater.startRotation = door.transform.localEulerAngles.y;
                        rotater.endRotation = 0f;
                        rotater.t = 0f;
                        rotater.StartCoroutine(rotater.Rotate(door, true, true));
                    }
                }
            }

            private static void CloseWallLocker(StorageContainer __instance)
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

            [HarmonyPrefix]
            [HarmonyPatch( "OnHandHover")]
            static bool OnHandHoverPrefix(StorageContainer __instance, GUIHand hand)
            {
                if (!Main.config.newStorageUI)
                    return true;

                if (!__instance.enabled)
                    return false;

                Constructable c = __instance.gameObject.GetComponent<Constructable>();
                if (c && !c.constructed)
                    return false;

                bool decoLocker = false;
                //AddDebug("StorageContainer OnHandHover " + __instance.name);
                //AddDebug("StorageContainer IsEmpty " + __instance.IsEmpty());
                TechTag parentTT = __instance.transform.parent.GetComponent<TechTag>();
                if (parentTT)
                {
                    //AddDebug("parent TechTag " + parentTT.type.ToString());
                    decoLocker = parentTT.type.ToString() == "DecorativeLockerClosed";
                }
                StringBuilder stringBuilder = new StringBuilder(Language.main.Get(__instance.hoverText));
                stringBuilder.Append(" (");
                stringBuilder.Append(TooltipFactory.stringButton0);
                stringBuilder.Append(")\n");
                //string text = __instance.hoverText + " (" + TooltipFactory.stringLeftHand + ")\n";
                //string textEmpty = __instance.IsEmpty() ? "Empty" : string.Empty;
                string textEmpty = string.Empty;
                bool empty = __instance.IsEmpty();
                ColoredLabel cl = null;
                PickupableStorage ps = null;
                Sign sign = null;
                if (decoLocker)
                    sign = __instance.transform.parent.GetComponentInChildren<Sign>();
                else
                    sign = __instance.GetComponentInChildren<Sign>();

                if (__instance.GetComponent<SmallStorage>())
                {
                    Transform tr = __instance.transform.parent.Find("LidLabel/Label");
                    //cl = __instance.transform.parent.GetComponentInChildren<ColoredLabel>();
                    cl = tr.GetComponent<ColoredLabel>();
                    tr = __instance.transform.parent.Find("collider_main");
                    //ps = __instance.transform.parent.GetComponentInChildren<PickupableStorage>();
                    ps = tr.GetComponent<PickupableStorage>();
                }
                else
                    cl = __instance.GetComponentInChildren<ColoredLabel>();

                if (cl && cl.enabled)
                {
                    //AddDebug("ColoredLabel " + cl.stringEditLabel);
                    stringBuilder.Append(Language.main.Get(cl.stringEditLabel));
                    stringBuilder.Append(" (");
                    stringBuilder.Append(TooltipFactory.stringButton1);
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
                    stringBuilder.Append(TooltipFactory.stringButton1);
                    stringBuilder.Append(")");
                    //text  += Language.main.Get(cl.stringEditLabel) + " (" + TooltipFactory.stringRightHand+ ")";
                    if (GameInput.GetButtonDown(GameInput.Button.RightHand))
                        sign.signInput.Select(true);
                }
                if (ps)
                {
                    bool canPickUp = empty || Main.pickupFullCarryallsLoaded;
                    //AddDebug("PickupableStorage cantPickupHoverText " + ps.cantPickupHoverText);
                    if (canPickUp)
                    {
                        //stprageString = text;
                        stringBuilder.Append(UI_Patches.smallStorageString);
                        //text += smallStorageString;
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
                if (!Main.config.newStorageUI)
                    return;
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
                if (!Main.config.newStorageUI)
                    return;

                if ( __instance.constructedAmount == 0f)
                {
                    if (__instance.techType == TechType.Locker || __instance.techType.ToString() == "DecorativeLockerClosed")
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

        
    }
}
