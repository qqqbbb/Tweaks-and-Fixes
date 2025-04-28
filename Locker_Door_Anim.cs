using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static ErrorMessage;
using UnityEngine;

namespace Tweaks_Fixes
{
    internal class Locker_Door_Anim
    {
        static FMODAsset openSound;
        static FMODAsset closeSound;

        public class LockerDoorOpener : MonoBehaviour
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

        [HarmonyPatch(typeof(StorageContainer))]
        class StorageContainer_Patch
        {
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

            [HarmonyPostfix]
            [HarmonyPatch("Awake")]
            static void AwakePostfix(StorageContainer __instance)
            {
                //TechTag techTag = __instance.GetComponent<TechTag>();
                //AddDebug("StorageContainer Awake " + __instance.prefabRoot.name);
                //if (__instance.transform.parent)
                //    AddDebug("StorageContainer Awake parent " + __instance.transform.parent.name);
                //if (techTag && techTag.type == TechType.SmallLocker)
                //{
                //}
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
                else if (techType == "DecorativeLockerClosed")
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

            [HarmonyPostfix]
            [HarmonyPatch("OnClose")]
            static void OnClosePostfix(StorageContainer __instance)
            {
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

        }
    }
}
