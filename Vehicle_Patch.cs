using BepInEx;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    [HarmonyPatch(typeof(Vehicle))]
    public class Vehicle_patch
    {
        public static GameObject decoyPrefab;
        public static Light[] currentLights = new Light[2];
        public static TechType currentVehicleTT;
        public static Dictionary<Vehicle, Vehicle.DockType> dockedVehicles = new Dictionary<Vehicle, Vehicle.DockType>();
        static FMODAsset fireSound = null;
        public static float changeTorpedoTimeLeft = 0;
        public static float changeTorpedoTimeRight = 0;
        public static float changeTorpedoInterval = .5f;
        public static HashSet<TechType> vehicleTechTypes = new HashSet<TechType> { TechType.Cyclops };
        public static string exosuitName;

        public static void UpdateLights()
        {
            //AddDebug("UpdateLights " + currentLights.Length);
            if (currentLights == null || currentLights.Length == 0 || currentLights[0] == null || currentLights[0].gameObject == null || !currentLights[0].gameObject.activeInHierarchy)
                return;

            if (!Input.GetKey(ConfigMenu.lightButton.Value))
                return;
            //Light[] lights = __instance.GetComponentsInChildren<Light>();
            //AddDebug("lights.Length  " + currentLights[0].gameObject.activeInHierarchy);
            if (!Light_Control.lightIntensityStep.ContainsKey(currentVehicleTT))
            {
                AddDebug("lightIntensityStep missing " + currentVehicleTT);
                return;
            }
            if (!Light_Control.lightOrigIntensity.ContainsKey(currentVehicleTT))
            {
                AddDebug("lightOrigIntensity missing " + currentVehicleTT);
                return;
            }
            float step = 0f;
            //AddDebug("UpdateLights currentVehicleTT " + currentVehicleTT);
            if (GameInput.GetButtonDown(GameInput.Button.CycleNext))
                step = Light_Control.lightIntensityStep[currentVehicleTT];
            else if (GameInput.GetButtonDown(GameInput.Button.CyclePrev))
                step = -Light_Control.lightIntensityStep[currentVehicleTT];

            if (step == 0f)
                return;

            foreach (Light l in currentLights)
            {
                if (step > 0 && l.intensity > Light_Control.lightOrigIntensity[currentVehicleTT])
                    return;

                l.intensity += step;
                //AddDebug("Light Intensity " + l.intensity);
                Light_Control.SaveLightIntensity(currentVehicleTT, l.intensity);
            }
        }

        public static void VehicleUpdate(Vehicle vehicle)
        {
            if (vehicle.CanPilot())
            {
                vehicle.steeringWheelYaw = Mathf.Lerp(vehicle.steeringWheelYaw, 0.0f, Time.deltaTime);
                vehicle.steeringWheelPitch = Mathf.Lerp(vehicle.steeringWheelPitch, 0.0f, Time.deltaTime);
                if (vehicle.mainAnimator)
                {
                    vehicle.mainAnimator.SetFloat("view_yaw", vehicle.steeringWheelYaw * 70f);
                    vehicle.mainAnimator.SetFloat("view_pitch", vehicle.steeringWheelPitch * 45f);
                }
            }
            if (vehicle.GetPilotingMode() && vehicle.CanPilot() && (vehicle.moveOnLand || vehicle.transform.position.y < Ocean.GetOceanLevel()))
            {
                Vector2 vector2 = AvatarInputHandler.main.IsEnabled() ? GameInput.GetLookDelta() : Vector2.zero;
                vehicle.steeringWheelYaw = Mathf.Clamp(vehicle.steeringWheelYaw + vector2.x * vehicle.steeringReponsiveness, -1f, 1f);
                vehicle.steeringWheelPitch = Mathf.Clamp(vehicle.steeringWheelPitch + vector2.y * vehicle.steeringReponsiveness, -1f, 1f);
                if (vehicle.controlSheme == Vehicle.ControlSheme.Submersible)
                {
                    float num = 3f;
                    vehicle.useRigidbody.AddTorque(vehicle.transform.up * vector2.x * vehicle.sidewaysTorque * 0.0015f * num, ForceMode.VelocityChange);
                    vehicle.useRigidbody.AddTorque(vehicle.transform.right * -vector2.y * vehicle.sidewaysTorque * 0.0015f * num, ForceMode.VelocityChange);
                    vehicle.useRigidbody.AddTorque(vehicle.transform.forward * -vector2.x * vehicle.sidewaysTorque * 0.0002f * num, ForceMode.VelocityChange);
                }
                else if ((vehicle.controlSheme == Vehicle.ControlSheme.Submarine || vehicle.controlSheme == Vehicle.ControlSheme.Mech) && vector2.x != 0f)
                    vehicle.useRigidbody.AddTorque(vehicle.transform.up * vector2.x * vehicle.sidewaysTorque, ForceMode.VelocityChange);
            }
            bool powered = vehicle.IsPowered();
            if (vehicle.wasPowered != powered)
            {
                vehicle.wasPowered = powered;
                vehicle.OnPoweredChanged(powered);
            }
            vehicle.ReplenishOxygen();
        }

        public static List<TorpedoType> GetTorpedos(Vehicle vehicle, ItemsContainer torpedoStorage)
        {
            if (torpedoStorage == null)
                return null;

            List<TorpedoType> torpedos = new List<TorpedoType>();
            //AddDebug("GetTorpedos torpedoStorage.count " + torpedoStorage.count);
            for (int index = 0; index < vehicle.torpedoTypes.Length; ++index)
            {
                TechType torpedoType = vehicle.torpedoTypes[index].techType;
                if (torpedoStorage.Contains(torpedoType))
                {
                    torpedos.Add(vehicle.torpedoTypes[index]);
                }
            }
            return torpedos;
        }

        [HarmonyPostfix]
        [HarmonyPatch("Awake")]
        public static void AwakePostfix(Vehicle __instance)
        {
            //Light l1 = __instance.transform.Find("lights_parent/light_left").gameObject.GetComponent<Light>();
            //Light l2 = __instance.transform.Find("lights_parent/light_right").gameObject.GetComponent<Light>();
            TechType tt = CraftData.GetTechType(__instance.gameObject);
            vehicleTechTypes.Add(tt);
            //if (l1)
            //AddDebug( " Vehicle.Awake " + tt);
            //if (l2)
            //    AddDebug(__instance.gameObject.name + " Awake light 2");
            Light[] lights = __instance.transform.Find("lights_parent").GetComponentsInChildren<Light>(true);
            //AddDebug(tt + " Awake lights " + lights.Length);
            Light_Control.lightOrigIntensity[tt] = lights[0].intensity;
            Light_Control.lightIntensityStep[tt] = lights[0].intensity * .1f;
            if (Light_Control.IsLightSaved(tt))
            {
                float intensity = Light_Control.GetLightIntensity(tt);
                foreach (Light l in lights)
                    l.intensity = intensity;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("Start")]
        public static void StartPostfix(Vehicle __instance)
        {
            if (decoyPrefab)
            {
                List<TorpedoType> torpedoTypes = new List<TorpedoType>(__instance.torpedoTypes);
                TorpedoType decoy = new TorpedoType
                {
                    techType = TechType.CyclopsDecoy,
                    prefab = decoyPrefab
                };
                torpedoTypes.Add(decoy);
                //__instance.torpedoTypes = new TorpedoType[3] {decoy, torpedoTypes[0], torpedoTypes[1]};
                //TorpedoType[] tTypes = new TorpedoType[__instance.torpedoTypes.Length + 1];
                __instance.torpedoTypes = torpedoTypes.ToArray();
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch("TorpedoShot")]
        public static bool TorpedoShotPrefix(Vehicle __instance, ItemsContainer container, ref TorpedoType torpedoType, Transform muzzle, ref bool __result)
        { // __instance is null !
          //if (__instance == null)
          //    AddDebug("TorpedoShotPrefix  Vehicle is null  ");
            __instance = Player.main.currentMountedVehicle;
            if (SeaMoth_patch.selectedTorpedo != null)
                torpedoType = SeaMoth_patch.selectedTorpedo;

            if (container == Exosuit_Patch.torpedoStorageLeft)
                torpedoType = Exosuit_Patch.selectedTorpedoLeft;
            else if (container == Exosuit_Patch.torpedoStorageRight)
                torpedoType = Exosuit_Patch.selectedTorpedoRight;

            //AddDebug("TorpedoShot " + torpedoType.techType);
            if (torpedoType.techType == TechType.CyclopsDecoy)
            {
                if (torpedoType == null || !container.DestroyItem(torpedoType.techType))
                {
                    __result = false;
                    return false;
                }
                //AddDebug("TorpedoShot CyclopsDecoy ");
                Transform aimingTransform = Player.main.camRoot.GetAimingTransform();
                GameObject decoy = UnityEngine.Object.Instantiate(torpedoType.prefab, muzzle.position + Player.main.camRoot.mainCamera.transform.forward, aimingTransform.rotation);
                decoy.transform.Rotate(90f, 0f, 0f, Space.Self);
                CyclopsDecoy cd = decoy.GetComponent<CyclopsDecoy>();
                cd.launch = true;
                if (fireSound == null)
                {
                    fireSound = ScriptableObject.CreateInstance<FMODAsset>();
                    fireSound.path = "event:/sub/seamoth/torpedo_fire";
                    fireSound.id = "{63375e84-44b4-4be9-a5bd-80f3e974790d}";
                }
                if (fireSound)
                    FMODUWE.PlayOneShot(fireSound, decoy.transform.position);

                __result = true;
                return false;
            }
            else
                return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch("TorpedoShot")]
        public static void TorpedoShotPostfix(Vehicle __instance, ItemsContainer container, ref TorpedoType torpedoType, Transform muzzle)
        { // __instance is null !
            //if (SeaMoth_patch.selectedTorpedo == null)
            //    AddDebug("TorpedoShot __instance == null");
            __instance = Player.main.currentMountedVehicle;
            if (__instance is SeaMoth)
            {
                if (SeaMoth_patch.selectedTorpedo != null)
                {
                    if (!SeaMoth_patch.torpedoStorage.Contains(SeaMoth_patch.selectedTorpedo.techType))
                        SeaMoth_patch.ChangeTorpedo(Player.main.currentMountedVehicle as SeaMoth);
                }
                else
                {
                    List<TorpedoType> torpedos = GetTorpedos(__instance, SeaMoth_patch.torpedoStorage);
                    if (torpedos.Count <= 1)
                        SeaMoth_patch.exitButton = UI_Patches.vehicleExitButton + " " + UI_Patches.seamothLightsButton;
                }
            }
            else if (__instance is Exosuit)
            {
                if (Exosuit_Patch.selectedTorpedoLeft != null && !Exosuit_Patch.torpedoStorageLeft.Contains(Exosuit_Patch.selectedTorpedoLeft.techType))
                    Exosuit_Patch.ChangeTorpedo(__instance as Exosuit, Exosuit_Patch.torpedoStorageLeft);
                else if (Exosuit_Patch.selectedTorpedoRight != null && !Exosuit_Patch.torpedoStorageRight.Contains(Exosuit_Patch.selectedTorpedoRight.techType))
                    Exosuit_Patch.ChangeTorpedo(__instance as Exosuit, Exosuit_Patch.torpedoStorageRight);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("EnterVehicle")]
        public static void EnterVehiclePostfix(Vehicle __instance)
        {
            //foreach (TorpedoType t in __instance.torpedoTypes)
            //    AddDebug("TorpedoType " + t.techType);
            Exosuit_Patch.selectedTorpedoLeft = null;
            Exosuit_Patch.selectedTorpedoRight = null;
            SeaMoth_patch.selectedTorpedo = null;
            currentVehicleTT = CraftData.GetTechType(__instance.gameObject);
            currentLights = __instance.transform.Find("lights_parent").GetComponentsInChildren<Light>(true);
            //AddDebug("EnterVehicle " + currentLights.Length);
        }

        [HarmonyPostfix]
        [HarmonyPatch("OnPilotModeEnd")]
        public static void OnPilotModeEndPostfix(Vehicle __instance)
        {
            currentLights[0] = null;
            SeaMoth_patch.selectedTorpedo = null;
            //AddDebug("Vehicle OnPilotModeEnd " + currentLights.Length);
        }

        [HarmonyPostfix]
        [HarmonyPatch("OnHandHover")]
        public static void OnHandHoverPostfix(Vehicle __instance)
        {
            if (!ConfigToEdit.newUIstrings.Value)
                return;
            //AddDebug("SeaMoth_patch.seamothName " + SeaMoth_patch.seamothName);
            //EcoTarget ecoTarget = __instance.GetComponent<EcoTarget>();
            if (__instance.onGround && !Inventory.main.GetHeld() && __instance is SeaMoth && !__instance.docked && !Player.main.IsSwimming())
            {
                HandReticle.main.SetText(HandReticle.TextType.Hand, UI_Patches.pushSeamothString, false, GameInput.Button.RightHand);
                if (GameInput.GetButtonDown(GameInput.Button.RightHand))
                {
                    Rigidbody rb = __instance.GetComponent<Rigidbody>();
                    Vector3 direction = new Vector3(MainCameraControl.main.transform.forward.x, .2f, MainCameraControl.main.transform.forward.z);
                    rb.AddForce(direction * 3333f, ForceMode.Impulse);
                }
            }
        }
        // fix seamoth move diagonally

        [HarmonyPostfix]
        [HarmonyPatch("OnUpgradeModuleChange")]
        public static void OnUpgradeModuleChangePostfix(Vehicle __instance, TechType techType)
        {
            //AddDebug("OnUpgradeModuleChange");
            if (!Main.gameLoaded || Language.isQuitting || techType != TechType.VehicleArmorPlating)
                return;

            int armorUpgrades = GetNumModules(__instance, TechType.VehicleArmorPlating);
            if (armorUpgrades == 0)
                return;

            string msg = Language.main.Get("TF_VehicleArmorPlating_install_message");
            if (msg.IsNullOrWhiteSpace())
                return;

            if (armorUpgrades == 2)
                msg = msg.Replace("70", "50");
            else if (armorUpgrades > 2)
                msg = msg.Replace("70", "40");

            AddDebug(msg);
        }

        private static int GetNumModules(Vehicle __instance, TechType ttToCount)
        {
            int count = 0;
            for (int i = 0; i < __instance.slotIDs.Length; ++i)
            {
                TechType tt = __instance.modules.GetTechTypeInSlot(__instance.slotIDs[i]);
                if (tt == ttToCount)
                    count++;
            }
            return count;
        }

        [HarmonyPrefix]
        [HarmonyPatch("OnKill")]
        public static void OnKillPrefix(Vehicle __instance)
        {
            StorageContainer sc = __instance.GetComponentInChildren<StorageContainer>();
            if (sc != null && sc.container != null)
                Util.DropItems(sc.container);
            else
            {
                SeamothStorageContainer ssc = __instance.GetComponentInChildren<SeamothStorageContainer>(true);
                if (ssc != null && ssc.container != null)
                    Util.DropItems(ssc.container);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("OnDockedChanged")]
        public static void OnDockedChangedPrefix(Vehicle __instance, bool docked, Vehicle.DockType dockType)
        {
            //AddDebug("OnDockedChanged docked " + docked);
            if (docked)
                dockedVehicles[__instance] = dockType;
            else
                dockedVehicles[__instance] = Vehicle.DockType.None;
        }

        //[HarmonyPrefix]
        //[HarmonyPatch("ToggleSlot", new Type[] { typeof(int), typeof(bool) })]
        public static bool ToggleSlotPrefix(int slotID, bool state, Vehicle __instance)
        {
            //AddDebug(" Vehicle ToggleSlot  " + slotID + " " + state);
            if (!Main.exosuitTorpedoDisplayLoaded && __instance is Exosuit && GameInput.GetButtonHeld(GameInput.Button.AltTool))
            {
                ItemsContainer container = null;
                int torpedoSlot = -1;
                float changeTorpedoTime = 0f;
                if (Exosuit_Patch.torpedoStorageLeft != null && GameInput.GetButtonDown(GameInput.Button.CycleNext))
                {
                    container = Exosuit_Patch.torpedoStorageLeft;
                    torpedoSlot = 0;
                    //AddDebug("ToggleSlot torpedoStorageLeft");
                    changeTorpedoTime = changeTorpedoTimeLeft;
                }
                if (Exosuit_Patch.torpedoStorageRight != null && GameInput.GetButtonDown(GameInput.Button.CyclePrev))
                {
                    container = Exosuit_Patch.torpedoStorageRight;
                    torpedoSlot = 1;
                    //AddDebug("ToggleSlot torpedoStorageRight");
                    changeTorpedoTime = changeTorpedoTimeRight;
                }
                if (container == null)
                {
                    //AddDebug("ToggleSlot return false");
                    return false;
                }
                //List<TorpedoType> torpedoTypes = GetTorpedos(__instance, container);
                //AddDebug("TorpedoShot torpedoTypes " + torpedoTypes.Count);
                //Main.Log("timePassedAsFloat " + Time.time);
                if (Time.time - changeTorpedoTime > changeTorpedoInterval)
                {
                    //Main.Log("changeTorpedoTime " + changeTorpedoTime);
                    Exosuit_Patch.ChangeTorpedo(__instance as Exosuit, container);
                    if (torpedoSlot == 0)
                        changeTorpedoTimeLeft = Time.time;
                    else if (torpedoSlot == 1)
                        changeTorpedoTimeRight = Time.time;
                    return false;
                }
            }
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch("OnProtoDeserialize")]
        static void OnProtoDeserializePostfix(Vehicle __instance)
        {
            //AddDebug("Vehicle OnProtoDeserialize ");
            if (ConfigToEdit.disableGravityForExosuit.Value && __instance is Exosuit && Player.main.currentMountedVehicle != __instance)
            {
                Util.FreezeObject(__instance.gameObject, true);
            }
        }

        //[HarmonyPostfix]
        //[HarmonyPatch("SlotKeyDown")]
        public static void SlotKeyDownPostfix(Vehicle __instance, int slotID)
        {
            AddDebug("SlotKeyDown " + slotID);
            TechType currentModule = __instance.modules.GetTechTypeInSlot(__instance.slotIDs[slotID]);
            Exosuit exosuit = __instance as Exosuit;
            if (exosuit)
            {
                AddDebug("SlotKeyDown leftArmType " + exosuit.leftArmType);
                AddDebug("SlotKeyDown rightArmType " + exosuit.rightArmType);
                AddDebug("SlotKeyDown HasMoreThan1TorpedoType " + Exosuit_Patch.HasMoreThan1TorpedoType(exosuit));
            }
            else
                AddDebug("SlotKeyDown " + currentModule);

        }

    }

    [HarmonyPatch(typeof(VFXSeamothDamages))]
    class VFXSeamothDamages_patch
    {
        [HarmonyPostfix]
        [HarmonyPatch("Start")]
        public static void StartPostfix(VFXSeamothDamages __instance)
        {
            //Transform t = __instance.transform.Find("xDrips");
            __instance.dripsParticles = null;
        }
        //[HarmonyPrefix]
        //[HarmonyPatch("UpdateParticles")]
        public static bool UpdateParticlesPostfix(VFXSeamothDamages __instance)
        {
            //AddDebug("UpdateParticles " + rate);
            return true;
        }
        //[HarmonyPostfix]
        //[HarmonyPatch("Update")]
        public static void UpdatePostfix(VFXSeamothDamages __instance)
        {
            AddDebug("VFXSeamothDamages Update");
        }
    }

    [HarmonyPatch(typeof(SeaMoth))]
    class SeaMoth_patch
    {
        public static string exitButton;
        static TechType currentModule;
        static string useButton;
        static bool torpedoModuleActive;
        public static TorpedoType selectedTorpedo = null;
        public static ItemsContainer torpedoStorage;
        public static float changeTorpedoInterval = .5f;
        public static float changeTorpedoTime = 0f;

        public static void ChangeTorpedo(SeaMoth seamoth)
        {
            if (torpedoStorage == null)
                return;

            List<TorpedoType> torpedos = Vehicle_patch.GetTorpedos(seamoth, torpedoStorage);
            //AddDebug("ChangeTorpedo torpedos " + torpedos.Count);
            if (torpedos.Count == 0)
            {
                selectedTorpedo = null;
                useButton = "";
                return;
            }
            //else if (torpedos.Count == 1)
            //    return;
            bool found1 = false;
            for (int index = 0; index < torpedos.Count; ++index)
            {
                if (selectedTorpedo == null)
                    selectedTorpedo = torpedos[index];

                if (torpedos[index].techType == selectedTorpedo.techType)
                {
                    if (index + 1 == torpedos.Count)
                    {
                        selectedTorpedo = torpedos[0];
                        //AddDebug("ChangeTorpedo last index " + selectedTorpedo.techType);
                        found1 = true;
                        break;
                    }
                    else if (torpedos.Count > 1)
                    {
                        //AddDebug("ChangeTorpedo " + selectedTorpedo.techType);
                        selectedTorpedo = torpedos[index + 1];
                        //AddDebug("ChangeTorpedo new " + selectedTorpedo.techType);
                        found1 = true;
                        break;
                    }
                    else
                    {
                        selectedTorpedo = torpedos[0];
                        //AddDebug("ChangeTorpedo 1 type " + selectedTorpedo.techType);
                    }
                }
            }
            if (!found1)
            {
                selectedTorpedo = torpedos[0];
                //AddDebug("ChangeTorpedo prev not found " + selectedTorpedo.techType);
            }
            useButton = Language.main.Get(selectedTorpedo.techType) + " " + torpedoStorage.GetCount(selectedTorpedo.techType) + " " + UI_Patches.leftHandButton;
            //AddDebug("ChangeTorpedo useButton new " + useButton);
            if (!Main.torpedoImprovementsLoaded)
            {
                if (torpedos.Count > 1)
                {
                    exitButton = UI_Patches.vehicleExitButton + " " + UI_Patches.seamothLightsButton + UI_Patches.changeTorpedoButton;
                }
                else if (torpedos.Count <= 1)
                {
                    //AddDebug("ChangeTorpedo Count <= 1 " + selectedTorpedo.techType);
                    exitButton = UI_Patches.vehicleExitButton + " " + UI_Patches.seamothLightsButton;
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("Start")]
        public static void StartPostfix(SeaMoth __instance)
        {
            LargeWorldEntity_Patch.AddVFXsurfaceComponent(__instance.gameObject, VFXSurfaceTypes.metal);
            if (Vehicle_patch.dockedVehicles.ContainsKey(__instance) && Vehicle_patch.dockedVehicles[__instance] == Vehicle.DockType.Cyclops)
                __instance.animator.Play("seamoth_cyclops_launchbay_dock");

            //if (ConfigMenu.fixSeamothDiagMovement.Value)
            //{
            //    __instance.sidewardForce = __instance.forwardForce * .5f;
            //    __instance.verticalForce = __instance.forwardForce * .5f;
            //    __instance.backwardForce = __instance.backwardForce * .5f;
            //}
        }

        [HarmonyPostfix]
        [HarmonyPatch("OnPilotModeBegin")]
        public static void OnPilotModeBeginPostfix(SeaMoth __instance)
        {
            //AddDebug("OnPilotModeBegin");
            //__instance.OnUpgradeModuleToggle();
            exitButton = UI_Patches.vehicleExitButton + " " + UI_Patches.seamothLightsButton;
            //seamothName = Language.main.Get(TechType.Seamoth);
        }

        [HarmonyPostfix]
        [HarmonyPatch("OnUpgradeModuleToggle")]
        public static void OnUpgradeModuleTogglePostfix(SeaMoth __instance, int slotID, bool active)
        {
            //AddDebug("OnUpgradeModuleToggle " + slotID + " " + active);
            if (!active)
            {
                selectedTorpedo = null;
                torpedoModuleActive = false;
                useButton = null;
                exitButton = UI_Patches.vehicleExitButton + " " + UI_Patches.seamothLightsButton;
                return;
            }
            currentModule = __instance.modules.GetTechTypeInSlot(__instance.slotIDs[slotID]);
            string currentModuleName = Language.main.Get(currentModule);
            currentModuleName = currentModuleName.Replace(UI_Patches.seamothName, "");
            currentModuleName = currentModuleName.TrimStart();
            currentModuleName = currentModuleName[0].ToString().ToUpper() + currentModuleName.Substring(1); // Uppercase first character
            if (currentModule == TechType.SeamothElectricalDefense)
                useButton = currentModuleName + Language.main.Get("TF_seamoth_defence_press") + UI_Patches.leftHandButton + Language.main.Get("TF_seamoth_defence_charge");
            else
                useButton = currentModuleName + " " + UI_Patches.leftHandButton;

            //AddDebug("OnUpgradeModuleToggle " + currentModule + " " + active);
            if (currentModule == TechType.SeamothTorpedoModule)
            {
                torpedoModuleActive = true;
                torpedoStorage = __instance.GetStorageInSlot(slotID, TechType.SeamothTorpedoModule);
                //AddDebug("OnUpgradeModuleToggle torpedoStorage count " + torpedoStorage.count);
                List<TorpedoType> torpedos = Vehicle_patch.GetTorpedos(__instance, torpedoStorage);
                //AddDebug("OnUpgradeModuleToggle torpedos.Count " + torpedos.Count);
                if (torpedos.Count == 0)
                    useButton = "";
                else if (torpedos.Count > 0)
                {
                    TechType torpedoType = torpedos[0].techType;
                    useButton = Language.main.Get(torpedoType) + " " + torpedoStorage.GetCount(torpedoType) + " " + UI_Patches.leftHandButton;
                    //AddDebug("OnUpgradeModuleToggle useButton " + useButton);
                }
                if (torpedos.Count > 1 && !Main.torpedoImprovementsLoaded)
                {
                    exitButton = UI_Patches.vehicleExitButton + " " + UI_Patches.seamothLightsButton + UI_Patches.changeTorpedoButton;
                }
            }
            else
            {
                torpedoModuleActive = false;
                exitButton = UI_Patches.vehicleExitButton + " " + UI_Patches.seamothLightsButton;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("OnUpgradeModuleUse")]
        public static void OnUpgradeModuleUsePostfix(SeaMoth __instance, int slotID, TechType techType)
        {
            if (techType == TechType.SeamothTorpedoModule)
            {
                //AddDebug("OnUpgradeModuleUse selectedTorpedo " + selectedTorpedo.techType);
                ItemsContainer storageInSlot = __instance.GetStorageInSlot(slotID, TechType.SeamothTorpedoModule);
                if (selectedTorpedo != null && storageInSlot.Contains(selectedTorpedo.techType))
                {
                    useButton = Language.main.Get(selectedTorpedo.techType) + " " + storageInSlot.GetCount(selectedTorpedo.techType) + " " + UI_Patches.leftHandButton;
                    return;
                }
                for (int index = 0; index < __instance.torpedoTypes.Length; ++index)
                {
                    TechType torpedoType = __instance.torpedoTypes[index].techType;
                    if (storageInSlot.Contains(__instance.torpedoTypes[index].techType))
                    {
                        useButton = Language.main.Get(torpedoType) + " " + storageInSlot.GetCount(torpedoType) + " " + UI_Patches.leftHandButton;
                        return;
                    }
                }
                useButton = Language.main.Get("VehicleTorpedoNoAmmo");
            }
        }

        //[HarmonyPrefix]
        //[HarmonyPatch("Update")]
        public static bool UpdatePrefix(SeaMoth __instance)
        {    // seamoth does not consume more energy when moving diagonally. Upgrade module UI
            if (!Main.gameLoaded)
                return false;
            //AddDebug("SeaMoth Update");
            Vehicle_patch.VehicleUpdate(__instance as Vehicle);
            __instance.UpdateSounds();

            if (__instance.GetPilotingMode() && !__instance.ignoreInput)
            {
                if (ConfigToEdit.newUIstrings.Value)
                {
                    HandReticle.main.SetTextRaw(HandReticle.TextType.Use, useButton);
                    HandReticle.main.SetTextRaw(HandReticle.TextType.UseSubscript, exitButton);
                }
                else
                {
                    HandReticle.main.SetTextRaw(HandReticle.TextType.Use, UI_Patches.vehicleExitButton);
                    HandReticle.main.SetTextRaw(HandReticle.TextType.UseSubscript, string.Empty);
                }
                Vector3 moveDirection = AvatarInputHandler.main.IsEnabled() ? GameInput.GetMoveDirection() : Vector3.zero;
                float magnitude = moveDirection.magnitude;
                AddDebug("Update moveDirection " + moveDirection);
                AddDebug("magnitude " + magnitude);
                //if (ConfigMenu.fixSeamothDiagMovement.Value)
                //    magnitude = Mathf.Clamp(moveDirection.magnitude, 0f, 1f);

                //if (magnitude > 0.1f)
                //    __instance.ConsumeEngineEnergy(Time.deltaTime * __instance.enginePowerConsumption * magnitude);

                __instance.toggleLights.CheckLightToggle();
            }
            __instance.UpdateScreenFX();
            __instance.UpdateDockedAnim();
            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch("Update")]
        public static void UpdatePostfix(SeaMoth __instance)
        {
            //AddDebug("UpdatePostfix");
            if (!Main.gameLoaded)
                return;

            if (!Main.torpedoImprovementsLoaded && torpedoModuleActive && GameInput.GetButtonDown(GameInput.Button.AltTool))
            {
                if (Time.time - changeTorpedoTime > changeTorpedoInterval)
                {
                    //AddDebug("changeTorpedoKey");
                    //if (GameInput.GetButtonDown(GameInput.Button.CycleNext) || GameInput.GetButtonDown(GameInput.Button.CyclePrev))
                    changeTorpedoTime = Time.time;
                    ChangeTorpedo(__instance);
                }
            }
            if (__instance.GetPilotingMode() && !__instance.ignoreInput)
            {
                if (ConfigToEdit.newUIstrings.Value)
                {
                    //AddDebug(" new vehicle UI");
                    HandReticle.main.SetTextRaw(HandReticle.TextType.Use, useButton);
                    HandReticle.main.SetTextRaw(HandReticle.TextType.UseSubscript, exitButton);
                }
                else
                {
                    //AddDebug(" vanilla vehicle UI");
                    HandReticle.main.SetTextRaw(HandReticle.TextType.Use, UI_Patches.vehicleExitButton);
                    HandReticle.main.SetTextRaw(HandReticle.TextType.UseSubscript, string.Empty);
                }
            }
        }


    }

    [HarmonyPatch(typeof(Exosuit))]
    class Exosuit_Patch
    {
        public static string exosuitName;
        //public static string exitButton;
        public static string leftArm;
        public static string rightArm;
        public static bool armNamesChanged = false;
        public static bool exosuitStarted = false;
        public static TorpedoType selectedTorpedoLeft = null;
        public static TorpedoType selectedTorpedoRight = null;
        public static ItemsContainer torpedoStorageLeft;
        public static ItemsContainer torpedoStorageRight;

        public static List<TorpedoType> GetTorpedos(Exosuit exosuit, ItemsContainer torpedoStorage)
        {
            if (torpedoStorage == null)
                return null;

            List<TorpedoType> torpedos = new List<TorpedoType>();

            for (int index = 0; index < exosuit.torpedoTypes.Length; ++index)
            {
                TechType torpedoType = exosuit.torpedoTypes[index].techType;
                if (torpedoStorage.Contains(torpedoType))
                    torpedos.Add(exosuit.torpedoTypes[index]);
            }
            return torpedos;
        }

        public static bool HasMoreThan1TorpedoType(Exosuit exosuit)
        {
            List<TorpedoType> torpedosLeft = GetTorpedos(exosuit, torpedoStorageLeft);
            List<TorpedoType> torpedosRight = GetTorpedos(exosuit, torpedoStorageRight);

            if (torpedosLeft != null && torpedosLeft.Count > 1)
                return true;
            if (torpedosRight != null && torpedosRight.Count > 1)
                return true;

            return false;
        }

        public static bool HasMoreThan1TorpedoType(Exosuit exosuit, ItemsContainer torpedoStorage)
        {
            List<TorpedoType> torpedos = GetTorpedos(exosuit, torpedoStorage);
            return torpedos != null && torpedos.Count > 1;
        }

        public static void ChangeTorpedo(Exosuit exosuit, ItemsContainer torpedoStorage)
        {
            if (torpedoStorageLeft == null && torpedoStorageRight == null)
                return;

            List<TorpedoType> torpedos = GetTorpedos(exosuit, torpedoStorage);
            TorpedoType selectedTorpedo = null;
            bool left = torpedoStorage == torpedoStorageLeft;
            //if (slot == 0)
            //    torpedos = GetTorpedos(exosuit, torpedoStorageLeft);
            //else
            //    torpedos = GetTorpedos(exosuit, torpedoStorageRight);

            //AddDebug("ChangeTorpedo torpedos.Count " + torpedos.Count);
            if (torpedos.Count == 0)
            {
                //selectedTorpedo = null;
                return;
            }
            bool found1 = false;
            for (int index = 0; index < torpedos.Count; ++index)
            {
                if (left)
                {
                    selectedTorpedo = selectedTorpedoLeft;
                    if (selectedTorpedoLeft == null)
                        selectedTorpedoLeft = torpedos[index];
                }
                else
                {
                    selectedTorpedo = selectedTorpedoRight;
                    if (selectedTorpedoRight == null)
                        selectedTorpedoRight = torpedos[index];
                }
                if (torpedos[index].techType == selectedTorpedo.techType)
                {
                    if (index + 1 == torpedos.Count)
                    {
                        selectedTorpedo = torpedos[0];
                        //AddDebug("ChangeTorpedo last index " + selectedTorpedo.techType);
                        found1 = true;
                        break;
                    }
                    else if (torpedos.Count > 1)
                    {
                        //AddDebug("ChangeTorpedo " + selectedTorpedo.techType);
                        selectedTorpedo = torpedos[index + 1];
                        //AddDebug("ChangeTorpedo new " + selectedTorpedo.techType);
                        found1 = true;
                        break;
                    }
                    else
                    {
                        selectedTorpedo = torpedos[0];
                        //AddDebug("ChangeTorpedo 1 type " + selectedTorpedo.techType);
                    }
                }
            }
            if (!found1)
            {
                selectedTorpedo = torpedos[0];
                //AddDebug("ChangeTorpedo prev not found " + selectedTorpedo.techType);
            }
            //useButton = Language.main.Get(selectedTorpedo.techType) + " " + torpedoStorage.GetCount(selectedTorpedo.techType) + " " + TooltipFactory.stringLeftHand;
            //if (torpedos.Count > 1)
            {
                //exitButton = LanguageCache.GetButtonFormat("PressToExit", GameInput.Button.Exit) + " " + Main.config.translatableStrings[17] + " " + TooltipFactory.stringRightHand + UI_Patches.changeTorpedoButton;
            }
            //else if (torpedos.Count <= 1)
            {
                //exitButton = LanguageCache.GetButtonFormat("PressToExit", GameInput.Button.Exit) + " " + Main.config.translatableStrings[17] + " " + TooltipFactory.stringRightHand;
            }
            if (left)
            {
                selectedTorpedoLeft = selectedTorpedo;
                leftArm = GetTorpedoName(exosuit, torpedoStorage, selectedTorpedoLeft, true);
                //AddDebug("ChangeTorpedo selectedTorpedoLeft " + selectedTorpedoLeft.techType);
                //AddDebug("ChangeTorpedo leftArm " + leftArm);
            }
            else
            {
                selectedTorpedoRight = selectedTorpedo;
                rightArm = GetTorpedoName(exosuit, torpedoStorage, selectedTorpedoRight, true);
            }
            armNamesChanged = true;
        }

        static string GetTorpedoName(Exosuit exosuit, ItemsContainer torpedoStorage, TorpedoType selectedTorpedo, bool next = false)
        { // runs before UI_Patches.GetStrings when game loads
          //if (selectedTorpedoLeft != null)
          //      AddDebug("GetTorpedoName selectedTorpedoLeft " + selectedTorpedoLeft.techType);
          //if (selectedTorpedoRight != null)
          //      AddDebug("GetTorpedoName selectedTorpedoRight " + selectedTorpedoRight.techType);
          //if (selectedTorpedo == null)
          //    AddDebug("GetTorpedoName "  + next);
          //else 
          //    AddDebug("GetTorpedoName " + selectedTorpedo.techType + " " + next);
            TorpedoType[] torpedoTypes = exosuit.torpedoTypes;
            List<TorpedoType> torpedos = Vehicle_patch.GetTorpedos(exosuit, torpedoStorage);

            if (selectedTorpedo != null)
            {
                //AddDebug("GetTorpedoName selectedTorpedo " + selectedTorpedo.techType);
                for (int index = 0; index < torpedoTypes.Length; ++index)
                {
                    TechType torpedoType = torpedoTypes[index].techType;
                    //AddDebug(torpedoType + " " + container.GetCount(torpedoType));
                    if (selectedTorpedo.techType == torpedoType && torpedoStorage.Contains(torpedoType))
                        return Language.main.Get(torpedoType) + " " + torpedoStorage.GetCount(torpedoType);
                }
            }
            if (!next)
            {
                for (int index = 0; index < torpedoTypes.Length; ++index)
                {
                    TechType torpedoType = torpedoTypes[index].techType;
                    //AddDebug(torpedoType + " " + container.GetCount(torpedoType));
                    if (torpedoStorage.Contains(torpedoType))
                    {
                        if (torpedoStorage == torpedoStorageLeft && selectedTorpedoLeft == null)
                            selectedTorpedoLeft = torpedoTypes[index];
                        else if (torpedoStorage == torpedoStorageRight && selectedTorpedoRight == null)
                            selectedTorpedoRight = torpedoTypes[index];
                        //AddDebug(torpedoType + " " + SeaMoth_patch.torpedoStorage.GetCount(torpedoType));
                        return Language.main.Get(torpedoType) + " " + torpedoStorage.GetCount(torpedoType);
                    }
                }
            }
            //exitButton = LanguageCache.GetButtonFormat("PressToExit", GameInput.Button.Exit) + " " + Main.config.translatableStrings[2] + " (" + uGUI.FormatButton(GameInput.Button.Deconstruct) + ")";
            if (torpedos != null || torpedos.Count == 0)
                return "";
            string name = Language.main.Get(TechType.ExosuitTorpedoArmModule);
            if (Language.main.GetCurrentLanguage() == "English")
                name = ShortenArmName(name);

            return name;
        }

        static public void GetArmNames(Exosuit exosuit)
        {
            if (exosuit.currentLeftArmType == TechType.ExosuitTorpedoArmModule)
            {
                torpedoStorageLeft = exosuit.GetStorageInSlot(0, TechType.ExosuitTorpedoArmModule);
                leftArm = GetTorpedoName(exosuit, torpedoStorageLeft, selectedTorpedoLeft);
            }
            else
            {
                //AddDebug("GetNames TooltipFactory.stringLeftHand " + uGUI.FormatButton(GameInput.Button.LeftHand));
                leftArm = Language.main.Get(exosuit.currentLeftArmType);
                if (Language.main.GetCurrentLanguage() == "English")
                    leftArm = ShortenArmName(leftArm);
            }
            if (exosuit.currentRightArmType == TechType.ExosuitTorpedoArmModule)
            {
                torpedoStorageRight = exosuit.GetStorageInSlot(1, TechType.ExosuitTorpedoArmModule);
                rightArm = GetTorpedoName(exosuit, torpedoStorageRight, selectedTorpedoRight);
            }
            else
            {
                rightArm = Language.main.Get(exosuit.currentRightArmType);
                if (Language.main.GetCurrentLanguage() == "English")
                    rightArm = ShortenArmName(rightArm);
            }
        }

        private static string ShortenArmName(string armName)
        {
            armName = armName.Replace(exosuitName, "");
            armName = armName.Trim();
            armName = armName[0].ToString().ToUpper() + armName.Substring(1);
            return armName;
        }

        [HarmonyPostfix]
        [HarmonyPatch("OnPilotModeBegin")]
        public static void OnPilotModeBeginPostfix(Exosuit __instance)
        {
            //AddDebug("OnPilotModeBegin");

            if (!Main.exosuitTorpedoDisplayLoaded)
            {
                torpedoStorageLeft = null;
                torpedoStorageRight = null;
                armNamesChanged = true;
                GetArmNames(__instance);
            }
            if (ConfigToEdit.disableGravityForExosuit.Value)
            {
                Util.FreezeObject(__instance.gameObject, false);
            }
            //__instance.OnUpgradeModuleToggle();
            //exitButton = LanguageCache.GetButtonFormat("PressToExit", GameInput.Button.Exit) + " " + Main.config.translatableStrings[17] + " " + TooltipFactory.stringRightHand;
            //seamothName = Language.main.Get(TechType.Seamoth);
            //seamothName = __instance.GetName();
        }

        [HarmonyPostfix]
        [HarmonyPatch("OnPilotModeEnd")]
        public static void OnPilotModeEndPostfix(Exosuit __instance)
        {
            //AddDebug("OnPilotModeEnd");
            if (ConfigToEdit.disableGravityForExosuit.Value)
            {
                Util.FreezeObject(__instance.gameObject, true);
            }
        }


        [HarmonyPostfix]
        [HarmonyPatch("Start")]
        static void StartPostfix(Exosuit __instance)
        {
            //__instance.StartCoroutine(PlayClip(__instance.mainAnimator, "exo_docked"));
            //AddDebug("Exosuit Start currentLeftArmType " + __instance.currentLeftArmType);
            //rightButton = uGUI.FormatButton(GameInput.Button.RightHand);
            //leftButton = uGUI.FormatButton(GameInput.Button.LeftHand);
            //GetArmNames(__instance);
            if (exosuitName == null)
            {
                //AddDebug("Start exosuitName == null");
                exosuitName = Language.main.Get("Exosuit");
            }
            if (Vehicle_patch.dockedVehicles.ContainsKey(__instance))
            {
                Vehicle.DockType dockType = Vehicle_patch.dockedVehicles[__instance];
                if (dockType == Vehicle.DockType.Cyclops)
                {
                    //AddDebug("Play exo_docked Cyclops");
                    __instance.mainAnimator.Play("exo_docked");
                }
                //else if (dockType == Vehicle.DockType.Base)
                //{
                //AddDebug("Exosuit Start DockType.Base)");
                //__instance.StartCoroutine(PlayClip(__instance.mainAnimator, "exo_docked", 11f));
                //AddDebug("Play exoMoonp_docked");
                //__instance.mainAnimator.Play("exo_docked");
                //__instance.mainAnimator.Play("exoMoonp_docked");
                //}
            }
            armNamesChanged = true;
            //AddDebug("Exosuit Start " + __instance.docked);
            //Main.Log("Exosuit start pos " + __instance.transform.position);
            //Main.Log("Exosuit start locpos " + __instance.transform.localPosition);
            CollisionSound collisionSound = __instance.gameObject.EnsureComponent<CollisionSound>();
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
            SetLights(__instance, Main.configMain.exosuitLights);
            exosuitStarted = true;
        }

        //[HarmonyPrefix]
        //[HarmonyPatch("Update")]
        public static bool UpdatePrefix(Exosuit __instance)
        {    // thrusters consumes 2x energy
             // no limit on thrusters
             //  strafing disabled in ApplyPhysicsMoveExosuit
            if (!Main.gameLoaded)
                return false;

            //if (!ConfigMenu.exosuitMoveTweaks.Value)
            //    return true;

            Vehicle_patch.VehicleUpdate(__instance);
            __instance.UpdateThermalReactorCharge();
            __instance.openedFraction = !__instance.storageContainer.GetOpen() ? Mathf.Clamp01(__instance.openedFraction - Time.deltaTime * 2f) : Mathf.Clamp01(__instance.openedFraction + Time.deltaTime * 2f);
            __instance.storageFlap.localEulerAngles = new Vector3(__instance.startFlapPitch + __instance.openedFraction * 80f, 0.0f, 0.0f);
            bool pilotingMode = __instance.GetPilotingMode();
            bool onGround = __instance.onGround || Time.time - __instance.timeOnGround <= 0.5f;
            __instance.mainAnimator.SetBool("sit", !pilotingMode & onGround && !__instance.IsUnderwater());
            bool inUse = pilotingMode && !__instance.docked;
            if (pilotingMode)
            {
                Player.main.transform.localPosition = Vector3.zero;
                Player.main.transform.localRotation = Quaternion.identity;
                Vector3 input;
                if (__instance.IsAutopilotEnabled)
                    input = __instance.CalculateAutopilotLocalWishDir();
                else
                    input = AvatarInputHandler.main.IsEnabled() ? GameInput.GetMoveDirection() : Vector3.zero;

                bool thrusterOn = input.y > 0f;
                bool hasPower = __instance.IsPowered() && __instance.liveMixin.IsAlive();
                __instance.GetEnergyValues(out float charge, out float capacity);
                __instance.thrustPower = Util.NormalizeTo01range(charge, 0f, capacity);
                //Main.Message("thrustPower " + __instance.thrustPower);
                if (thrusterOn & hasPower)
                {
                    //__instance.thrustPower = Mathf.Clamp01(__instance.thrustPower - Time.deltaTime * __instance.thrustConsumption);
                    if ((__instance.onGround || Time.time - __instance.timeOnGround <= 1f) && !__instance.jetDownLastFrame)
                        __instance.ApplyJumpForce();

                    __instance.jetsActive = true;
                }
                else
                {
                    __instance.jetsActive = false;
                    //float num = Time.deltaTime * __instance.thrustConsumption * 0f;
                    //if (__instance.onGround)
                    //    num = (Time.deltaTime * __instance.thrustConsumption * 4f);
                    //__instance.thrustPower = Mathf.Clamp01(__instance.thrustPower + num);
                }
                //AddDebug("jetsActive" + __instance.jetsActive);
                __instance.jetDownLastFrame = thrusterOn;
                if (__instance.timeJetsActiveChanged + 0.3f < Time.time)
                {
                    if (__instance.jetsActive && __instance.thrustPower > 0)
                    {
                        __instance.loopingJetSound.Play();
                        __instance.fxcontrol.Play(0);
                        __instance.areFXPlaying = true;
                    }
                    else if (__instance.areFXPlaying)
                    {
                        __instance.loopingJetSound.Stop();
                        __instance.fxcontrol.Stop(0);
                        __instance.areFXPlaying = false;
                    }
                }
                float energyCost = __instance.thrustConsumption * Time.deltaTime;
                if (thrusterOn)
                {
                    __instance.ConsumeEngineEnergy(energyCost * 2f);
                    //Main.Message("Consume Energy thrust" + energyCost * 2f);
                }
                else if (input.z != 0f)
                {
                    __instance.ConsumeEngineEnergy(energyCost);
                    //Main.Message("Consume Energy move" + energyCost);
                }
                if (__instance.jetsActive)
                    __instance.thrustIntensity += Time.deltaTime / __instance.timeForFullVirbation;
                else
                    __instance.thrustIntensity -= Time.deltaTime * 10f;

                __instance.thrustIntensity = Mathf.Clamp01(__instance.thrustIntensity);
                if (AvatarInputHandler.main.IsEnabled() && !__instance.ignoreInput)
                {
                    Vector3 eulerAngles = __instance.transform.eulerAngles;
                    eulerAngles.x = MainCamera.camera.transform.eulerAngles.x;
                    Quaternion aimDirection1 = Quaternion.Euler(eulerAngles.x, eulerAngles.y, eulerAngles.z);
                    Quaternion aimDirection2 = aimDirection1;
                    __instance.leftArm.Update(ref aimDirection1);
                    __instance.rightArm.Update(ref aimDirection2);
                    if (inUse)
                    {
                        Vector3 b1 = MainCamera.camera.transform.position + aimDirection1 * Vector3.forward * 100f;
                        Vector3 b2 = MainCamera.camera.transform.position + aimDirection2 * Vector3.forward * 100f;
                        __instance.aimTargetLeft.transform.position = Vector3.Lerp(__instance.aimTargetLeft.transform.position, b1, Time.deltaTime * 15f);
                        __instance.aimTargetRight.transform.position = Vector3.Lerp(__instance.aimTargetRight.transform.position, b2, Time.deltaTime * 15f);
                    }
                    __instance.UpdateUIText(__instance.rightArm is ExosuitPropulsionArm || __instance.leftArm is ExosuitPropulsionArm);
                    if (GameInput.GetButtonDown(GameInput.Button.AltTool) && !__instance.rightArm.OnAltDown())
                        __instance.leftArm.OnAltDown();
                }
                __instance.UpdateActiveTarget();
                __instance.UpdateSounds();
            }
            if (!inUse)
            {
                bool flag3 = false;
                bool flag4 = false;
                if (!Util.Approximately(__instance.aimTargetLeft.transform.localPosition.y, 0f))
                    __instance.aimTargetLeft.transform.localPosition = new Vector3(__instance.aimTargetLeft.transform.localPosition.x, Mathf.MoveTowards(__instance.aimTargetLeft.transform.localPosition.y, 0f, Time.deltaTime * 50f), __instance.aimTargetLeft.transform.localPosition.z);
                else
                    flag3 = true;

                if (!Util.Approximately(__instance.aimTargetRight.transform.localPosition.y, 0f))
                    __instance.aimTargetRight.transform.localPosition = new Vector3(__instance.aimTargetRight.transform.localPosition.x, Mathf.MoveTowards(__instance.aimTargetRight.transform.localPosition.y, 0f, Time.deltaTime * 50f), __instance.aimTargetRight.transform.localPosition.z);
                else
                    flag4 = true;

                if (flag3 & flag4)
                    __instance.SetIKEnabled(false);
            }
            __instance.UpdateAnimations();
            if (__instance.armsDirty)
                __instance.UpdateExosuitArms();

            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch("Update")]
        public static void UpdatePostfix(Exosuit __instance)
        {
            if (!Main.gameLoaded || Main.vehicleLightsImprovedLoaded)
                return;

            CheckExosuitButtons(__instance);
        }

        private static void CheckExosuitButtons(Exosuit exosuit)
        {
            if (!IngameMenu.main.isActiveAndEnabled && !Player.main.pda.isInUse && Player.main.currentMountedVehicle == exosuit)
            {
                if (GameInput.GetButtonDown(GameInput.Button.MoveDown))
                    ToggleLights(exosuit);

                if (GameInput.lastDevice == GameInput.Device.Controller)
                {
                    bool leftTorpedo = HasMoreThan1TorpedoType(exosuit, torpedoStorageLeft);
                    bool rightTorpedo = HasMoreThan1TorpedoType(exosuit, torpedoStorageRight);

                    if (leftTorpedo && GameInput.GetButtonDown(GameInput.Button.Deconstruct))
                    {
                        ChangeTorpedo(exosuit, torpedoStorageLeft);
                    }
                    else if (rightTorpedo && GameInput.GetButtonDown(GameInput.Button.Deconstruct))
                    {
                        ChangeTorpedo(exosuit, torpedoStorageRight);
                    }
                }
            }
        }

        private static void ToggleLights(Exosuit exosuit)
        {
            Transform lightsT = exosuit.transform.Find("lights_parent");
            if (lightsT)
            {
                if (!lightsT.gameObject.activeSelf && exosuit.energyInterface.hasCharge)
                {
                    lightsT.gameObject.SetActive(true);
                    Main.configMain.exosuitLights = true;
                }
                else if (lightsT.gameObject.activeSelf)
                {
                    lightsT.gameObject.SetActive(false);
                    Main.configMain.exosuitLights = false;
                }
                //AddDebug("lights " + lightsT.gameObject.activeSelf);
            }
        }

        private static void SetLights(Exosuit exosuit, bool active)
        {
            if (active && !exosuit.energyInterface.hasCharge)
                return;

            Transform lightsT = exosuit.transform.Find("lights_parent");
            if (lightsT)
            {
                lightsT.gameObject.SetActive(active);
                //AddDebug("SetLights " + active);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch("UpdateUIText")]
        public static bool UpdateUITextPrefix(Exosuit __instance, bool hasPropCannon)
        {
            //return false;
            if (Main.vehicleLightsImprovedLoaded || !ConfigToEdit.newUIstrings.Value)
                return true;

            if (armNamesChanged || !__instance.hasInitStrings || __instance.lastHasPropCannon != hasPropCannon)
            {
                //AddDebug("UpdateUIText " + TooltipFactory.stringLeftHand);
                StringBuilder primary = new StringBuilder();
                bool leftTorpedo = HasMoreThan1TorpedoType(__instance, torpedoStorageLeft);
                bool rightTorpedo = HasMoreThan1TorpedoType(__instance, torpedoStorageRight);

                if (!string.IsNullOrEmpty(leftArm))
                {
                    primary.Append(leftArm);
                    primary.Append(' ');
                    primary.Append(UI_Patches.leftHandButton);
                    primary.Append("  ");
                }
                if (!string.IsNullOrEmpty(rightArm))
                {
                    primary.Append(rightArm);
                    primary.Append(' ');
                    primary.Append(UI_Patches.rightHandButton);
                    primary.Append("  ");
                }
                __instance.sb = new StringBuilder(UI_Patches.exosuitExitLightsButton);
                //AddDebug("__instance.sb Length" + __instance.sb.Length);
                if (!Main.exosuitTorpedoDisplayLoaded)
                {
                    if (GameInput.lastDevice == GameInput.Device.Keyboard)
                    {
                        //AddDebug("UpdateUIText leftTorpedo HasMoreThan1TorpedoType " + leftTorpedo);
                        //AddDebug("UpdateUIText rightTorpedo HasMoreThan1TorpedoType " + rightTorpedo);
                        if (leftTorpedo && rightTorpedo)
                        {
                            __instance.sb.Append(UI_Patches.changeTorpedoString);
                            __instance.sb.Append(UI_Patches.slot1Plus2Button);
                            //__instance.sb.Append(UI_Patches.changeTorpedoExosuitButton);
                        }
                        else if (leftTorpedo)
                        {
                            __instance.sb.Append(UI_Patches.changeTorpedoString);
                            __instance.sb.Append(UI_Patches.slot1Button);
                            //__instance.sb.Append(UI_Patches.changeTorpedoExosuitButton);
                        }
                        else if (rightTorpedo)
                        {
                            __instance.sb.Append(UI_Patches.changeTorpedoString);
                            __instance.sb.Append(UI_Patches.slot2Button);
                            //__instance.sb.Append(UI_Patches.changeTorpedoExosuitButton);
                        }
                    }
                    else if (GameInput.lastDevice == GameInput.Device.Controller)
                    { // alt tool button used by prop arm
                        if (leftTorpedo || rightTorpedo)
                            __instance.sb.Append(UI_Patches.exosuitChangeRightTorpedoButton);
                    }
                }
                if (hasPropCannon)
                {
                    primary.Append("\n");
                    primary.Append(UI_Patches.propCannonString);
                }
                __instance.lastHasPropCannon = hasPropCannon;
                __instance.uiStringPrimary = primary.ToString();
                armNamesChanged = false;
            }
            HandReticle.main.SetTextRaw(HandReticle.TextType.Use, __instance.uiStringPrimary);
            HandReticle.main.SetTextRaw(HandReticle.TextType.UseSubscript, __instance.sb.ToString());
            __instance.hasInitStrings = true;
            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch("OnUpgradeModuleChange")]
        public static void OnUpgradeModuleChangePostfix(Exosuit __instance, int slotID, TechType techType, bool added)
        { // runs before Exosuit.Start
            //AddDebug("OnUpgradeModuleChange " + techType + " " + added + " " + slotID);
            if (!exosuitStarted)
                return;
            //AddDebug("OnUpgradeModuleChange " + techType + " " + added + " " + slotID);
            if (!added)
            {
                if (slotID == 0)
                {
                    leftArm = Language.main.Get(TechType.ExosuitClawArmModule);
                    if (Language.main.GetCurrentLanguage() == "English")
                        leftArm = ShortenArmName(leftArm);
                }
                else if (slotID == 1)
                {
                    rightArm = Language.main.Get(TechType.ExosuitClawArmModule);
                    if (Language.main.GetCurrentLanguage() == "English")
                        rightArm = ShortenArmName(rightArm);
                }
            }
            else if (added)
            {
                if (slotID == 0)
                {
                    if (techType == TechType.ExosuitTorpedoArmModule)
                    {
                        torpedoStorageLeft = __instance.GetStorageInSlot(0, TechType.ExosuitTorpedoArmModule);
                        leftArm = GetTorpedoName(__instance, torpedoStorageLeft, selectedTorpedoLeft);
                    }
                    else
                    {
                        leftArm = Language.main.Get(techType);
                        if (Language.main.GetCurrentLanguage() == "English")
                            leftArm = ShortenArmName(leftArm);
                    }
                }
                else if (slotID == 1)
                {
                    if (techType == TechType.ExosuitTorpedoArmModule)
                    {
                        torpedoStorageRight = __instance.GetStorageInSlot(1, TechType.ExosuitTorpedoArmModule);
                        rightArm = GetTorpedoName(__instance, torpedoStorageRight, selectedTorpedoRight);
                    }
                    else
                    {
                        rightArm = Language.main.Get(techType);
                        if (Language.main.GetCurrentLanguage() == "English")
                            rightArm = ShortenArmName(rightArm);
                    }
                }
            }
            armNamesChanged = true;
            //AddDebug("OnUpgradeModuleChange currentLeftArmType " + __instance.currentLeftArmType);
        }

        [HarmonyPrefix]
        [HarmonyPatch("ApplyJumpForce")]
        public static bool ApplyJumpForcePrefix(Exosuit __instance)
        {
            if (__instance.timeLastJumped + 1f > Time.time)
                return false;

            if (__instance.onGround)
            {
                Utils.PlayFMODAsset(__instance.jumpSound, __instance.transform);
                if (__instance.IsUnderwater())
                {
                    if (Physics.Raycast(new Ray(__instance.transform.position, Vector3.down), out RaycastHit hitInfo, 10f))
                    {
                        //AddDebug("Raycast hit " + hitInfo.collider.name);
                        TerrainChunkPieceCollider tcpc = hitInfo.collider.GetComponent<TerrainChunkPieceCollider>();
                        if (tcpc)
                        {
                            __instance.fxcontrol.Play(1);
                            //AddDebug("jump from terrain ");
                        }
                    }
                }
            }
            __instance.ConsumeEngineEnergy(1.2f);
            __instance.useRigidbody.AddForce(Vector3.up * (__instance.jumpJetsUpgraded ? 7f : 5f), ForceMode.VelocityChange);
            __instance.timeLastJumped = Time.time;
            __instance.timeOnGround = 0f;
            __instance.onGround = false;
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch("OnLand")]
        public static bool OnLandPrefix(Exosuit __instance)
        {
            if (__instance.timeLastJumped + 1f > Time.time)
                return false;

            Utils.PlayFMODAsset(__instance.landSound, __instance.bottomTransform);
            if (__instance.IsUnderwater())
            {
                if (Physics.Raycast(new Ray(__instance.transform.position, Vector3.down), out RaycastHit hitInfo, 10f))
                {
                    TerrainChunkPieceCollider tcpc = hitInfo.collider.GetComponent<TerrainChunkPieceCollider>();
                    if (tcpc)
                    {
                        __instance.fxcontrol.Play(2);
                        //AddDebug("land on terrain ");
                    }
                }
            }
            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch("SlotKeyDown")]
        public static void SlotKeyDownPostfix(Exosuit __instance, int slotID)
        {
            if (Main.exosuitTorpedoDisplayLoaded)
                return;
            //AddDebug("Exosuit SlotKeyDown " + slotID);
            //TechType currentModule = __instance.modules.GetTechTypeInSlot(__instance.slotIDs[slotID]);
            //AddDebug("Exosuit SlotKeyDown currentModule " + currentModule);
            if (slotID == 2 && HasMoreThan1TorpedoType(__instance, torpedoStorageLeft))
            {
                //AddDebug("Exosuit SlotKeyDown left " );
                ChangeTorpedo(__instance, torpedoStorageLeft);
            }
            else if (slotID == 3 && HasMoreThan1TorpedoType(__instance, torpedoStorageRight))
            {
                //AddDebug("Exosuit SlotKeyDown Right ");
                ChangeTorpedo(__instance, torpedoStorageRight);
            }
            //AddDebug("SlotKeyDown leftArmType " + __instance.leftArmType);
            //AddDebug("SlotKeyDown rightArmType " + __instance.rightArmType);
            //AddDebug("SlotKeyDown HasMoreThan1TorpedoType " + HasMoreThan1TorpedoType(__instance));
        }

    }

    [HarmonyPatch(typeof(ExosuitClawArm))]
    class ExosuitClawArm_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch("IExosuitArm.GetInteractableRoot")]
        static void GetInteractableRootPostfix(ExosuitClawArm __instance, GameObject target, ref GameObject __result)
        {
            //AddDebug("ExosuitClawArm GetInteractableRoot Postfix target " + target.name);
            if (__result == null && target.GetComponent<SupplyCrate>())
                __result = target.gameObject;
        }
        [HarmonyPrefix]
        [HarmonyPatch("TryUse", new Type[] { typeof(float) }, new[] { ArgumentType.Out })]
        static bool TryUsePrefix(ExosuitClawArm __instance, ref float cooldownDuration, ref bool __result)
        { // open supply crates
            if (Time.time - __instance.timeUsed >= __instance.cooldownTime)
            {
                Pickupable pickupable = null;
                PickPrefab pickPrefab = null;
                SupplyCrate supplyCrate = null;
                __result = false;
                bool playAnim = false;
                GameObject target = __instance.exosuit.GetActiveTarget();
                if (target)
                {
                    pickupable = target.GetComponent<Pickupable>();
                    pickPrefab = target.GetComponent<PickPrefab>();
                    supplyCrate = target.GetComponent<SupplyCrate>();
                }
                if (pickupable != null && pickupable.isPickupable)
                {
                    if (__instance.exosuit.storageContainer.container.HasRoomFor(pickupable))
                    {
                        __instance.animator.SetTrigger("use_tool");
                        __instance.cooldownTime = cooldownDuration = __instance.cooldownPickup;
                        __result = true;
                        return false;
                    }
                    else
                        ErrorMessage.AddMessage(Language.main.Get("ContainerCantFit"));
                }
                else if (pickPrefab)
                {
                    __instance.animator.SetTrigger("use_tool");
                    __instance.cooldownTime = cooldownDuration = __instance.cooldownPickup;
                    __result = true;
                    return false;
                }
                else if (supplyCrate)
                {
                    if (supplyCrate.sealedComp && supplyCrate.sealedComp.IsSealed())
                        return false;

                    if (!supplyCrate.open)
                    {
                        supplyCrate.ToggleOpenState();
                        playAnim = true;
                    }
                    else if (supplyCrate.open)
                    {
                        if (supplyCrate.itemInside)
                        {
                            if (__instance.exosuit.storageContainer.container.HasRoomFor(supplyCrate.itemInside))
                            {
                                ItemsContainer container = __instance.exosuit.storageContainer.container;
                                supplyCrate.itemInside.Initialize();
                                InventoryItem inventoryItem = new InventoryItem(supplyCrate.itemInside);
                                container.UnsafeAdd(inventoryItem);
                                Utils.PlayFMODAsset(__instance.pickupSound, __instance.front, 5f);
                                supplyCrate.itemInside = null;
                                playAnim = true;
                            }
                            else
                            {
                                ErrorMessage.AddMessage(Language.main.Get("ContainerCantFit"));
                                return false;
                            }
                        }
                    }
                    if (playAnim)
                    {
                        __instance.animator.SetTrigger("use_tool");
                        __instance.cooldownTime = cooldownDuration = __instance.cooldownPickup;
                        //supplyCrate.OnHandClick(null);
                        __result = true;
                    }
                    return false;
                }
                else
                {
                    __instance.animator.SetTrigger("bash");
                    __instance.cooldownTime = cooldownDuration = __instance.cooldownPunch;
                    __instance.fxControl.Play(0);
                    __result = true;
                    return false;
                }
            }
            cooldownDuration = 0f;
            __result = false;
            return false;
        }
    }

    [HarmonyPatch(typeof(ExosuitTorpedoArm))]
    class ExosuitTorpedoArm_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch("OnAddItem")]
        static void OnAddItemPostfix(ExosuitTorpedoArm __instance)
        {
            //AddDebug("ExosuitTorpedoArm OnAddItem ");
            Exosuit_Patch.GetArmNames(__instance.exosuit);
            Exosuit_Patch.armNamesChanged = true;
        }

        [HarmonyPostfix]
        [HarmonyPatch("OnRemoveItem")]
        static void OnRemoveItemPostfix(ExosuitTorpedoArm __instance)
        {
            //AddDebug("ExosuitTorpedoArm OnRemoveItem ");
            Exosuit_Patch.GetArmNames(__instance.exosuit);
            Exosuit_Patch.armNamesChanged = true;
        }

        [HarmonyPostfix]
        [HarmonyPatch("Shoot")]
        static void ShootPostfix(ExosuitTorpedoArm __instance, TorpedoType torpedoType, bool __result)
        {
            //AddDebug("ExosuitTorpedoArm Shoot " + torpedoType.techType + " " + __result);
            Exosuit_Patch.GetArmNames(__instance.exosuit);
            Exosuit_Patch.armNamesChanged = true;
        }
    }

    [HarmonyPatch(typeof(ExosuitDrillArm))]
    class ExosuitDrillArm_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch("OnHit")]
        public static bool Prefix(ExosuitDrillArm __instance)
        { // fix not showing particles when start drilling
          //AddDebug("OnHit");
            if (!__instance.exosuit.CanPilot() || !__instance.exosuit.GetPilotingMode())
                return false;

            Vector3 zero = Vector3.zero;
            GameObject closestObj = null;
            __instance.drillTarget = null;
            UWE.Utils.TraceFPSTargetPosition(__instance.exosuit.gameObject, 5f, ref closestObj, ref zero);
            if (closestObj == null)
            {
                InteractionVolumeUser component = Player.main.gameObject.GetComponent<InteractionVolumeUser>();
                if (component != null && component.GetMostRecent() != null)
                    closestObj = component.GetMostRecent().gameObject;
            }
            if (closestObj && __instance.drilling)
            {
                Drillable drillable = closestObj.FindAncestor<Drillable>();
                __instance.loopHit.Play();
                if (drillable)
                {
                    GameObject hitObject;
                    drillable.OnDrill(__instance.fxSpawnPoint.position, __instance.exosuit, out hitObject);
                    __instance.drillTarget = hitObject;
                    //if (__instance.fxControl.emitters[0].fxPS == null || __instance.fxControl.emitters[0].fxPS.emission.enabled) 
                    //AddDebug("emission.enabled " + __instance.fxControl.emitters[0].fxPS.emission.enabled);
                    //AddDebug("IsAlive " + __instance.fxControl.emitters[0].fxPS.IsAlive());
                    if (__instance.fxControl.emitters[0].fxPS != null && (!__instance.fxControl.emitters[0].fxPS.IsAlive() || !__instance.fxControl.emitters[0].fxPS.emission.enabled))
                    {
                        __instance.fxControl.Play(0);
                    }
                }
                else
                {
                    LiveMixin lm = closestObj.FindAncestor<LiveMixin>();
                    if (lm)
                    {
                        lm.IsAlive();
                        lm.TakeDamage(4f, zero, DamageType.Drill);
                        __instance.drillTarget = closestObj;
                    }
                    //AddDebug("target " + closestObj.name);
                    VFXSurface vfxSurface = closestObj.GetComponent<VFXSurface>();
                    if (__instance.drillFXinstance == null)
                        __instance.drillFXinstance = VFXSurfaceTypeManager.main.Play(vfxSurface, __instance.vfxEventType, __instance.fxSpawnPoint.position, __instance.fxSpawnPoint.rotation, __instance.fxSpawnPoint);
                    else if (vfxSurface != null && __instance.prevSurfaceType != vfxSurface.surfaceType)
                    {
                        __instance.drillFXinstance.GetComponent<VFXLateTimeParticles>().Stop();
                        UnityEngine.Object.Destroy(__instance.drillFXinstance.gameObject, 1.6f);
                        __instance.drillFXinstance = VFXSurfaceTypeManager.main.Play(vfxSurface, __instance.vfxEventType, __instance.fxSpawnPoint.position, __instance.fxSpawnPoint.rotation, __instance.fxSpawnPoint);
                        __instance.prevSurfaceType = vfxSurface.surfaceType;
                    }
                    closestObj.SendMessage("BashHit", __instance, SendMessageOptions.DontRequireReceiver);
                }
            }
            else
                __instance.StopEffects();

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch("StopEffects")]
        static bool StopEffectsPrefix(ExosuitDrillArm __instance)
        { // dont stop drilling sound when not hitting anything
          //AddDebug("StopEffects ");
            if (__instance.drillFXinstance != null)
            {
                __instance.drillFXinstance.GetComponent<VFXLateTimeParticles>().Stop();
                UnityEngine.Object.Destroy(__instance.drillFXinstance.gameObject, 1.6f);
                __instance.drillFXinstance = null;
            }
            if (__instance.fxControl.emitters[0].fxPS != null && __instance.fxControl.emitters[0].fxPS.emission.enabled)
                __instance.fxControl.Stop(0);
            //__instance.loop.Stop();
            __instance.loopHit.Stop();
            return false;
        }

        [HarmonyPatch("IExosuitArm.OnUseUp")]
        [HarmonyPostfix]
        static void OnUseUpPostfix(ExosuitDrillArm __instance)
        {
            //AddDebug("OnUseUp ");
            __instance.loop.Stop();
        }

    }

    [HarmonyPatch(typeof(CollisionSound), "OnCollisionEnter")]
    class CollisionSound_OnCollisionEnter_Patch
    { // fix fish splat sound when colliding with rocks
        static bool Prefix(CollisionSound __instance, Collision col)
        {
            //SeaMoth seaMoth = __instance.GetComponent<SeaMoth>();
            //if (seaMoth)
            //{
            //    Main.Log(" hitSoundSmall path " + __instance.hitSoundSmall.path);
            //    Main.Log(" hitSoundSmall id " + __instance.hitSoundSmall.id);
            //    Main.Log(" hitSoundFast path " + __instance.hitSoundFast.path);
            //    Main.Log(" hitSoundFast id " + __instance.hitSoundFast.id);
            //    Main.Log(" hitSoundMedium path " + __instance.hitSoundMedium.path);
            //    Main.Log(" hitSoundMedium id " + __instance.hitSoundMedium.id);
            //    Main.Log(" hitSoundSlow path " + __instance.hitSoundSlow.path);
            //    Main.Log(" hitSoundSlow id " + __instance.hitSoundSlow.id);
            //}
            Exosuit exosuit = __instance.GetComponent<Exosuit>();
            Rigidbody rb = UWE.Utils.GetRootRigidbody(col.gameObject);
            if (exosuit && !rb)
                return false;// no sounds when walking on ground

            float magnitude = col.relativeVelocity.magnitude;
            //FMODAsset asset = !rootRigidbody || rootRigidbody.mass >= 10.0 ? (magnitude <= 8.0 ? (magnitude <= 4.0 ? __instance.hitSoundSlow : __instance.hitSoundMedium) : __instance.hitSoundFast) : __instance.hitSoundSmall;
            FMODAsset asset = null;
            if (!rb || rb.mass >= 10.0f)
            {
                if (magnitude < 4f)
                    asset = __instance.hitSoundSlow;
                else if (magnitude < 8f)
                    asset = __instance.hitSoundMedium;
                else
                    asset = __instance.hitSoundFast;
            }
            else if (col.gameObject.GetComponent<Creature>())
                asset = __instance.hitSoundSmall;// fish splat sound
            else
                asset = __instance.hitSoundSlow;

            if (col.gameObject.tag == "Player")
                asset = __instance.hitSoundSlow;

            if (asset)
            {
                //AddDebug("col magnitude " + magnitude);
                float soundRadiusObsolete = Mathf.Clamp01(magnitude / 8f);
                Utils.PlayFMODAsset(asset, col.contacts[0].point, soundRadiusObsolete);
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(SeamothStorageContainer))]
    class SeamothStorageContainer_Patch
    {
        //[HarmonyPatch("Awake")]
        //[HarmonyPrefix]
        public static void AwakePrefix(SeamothStorageContainer __instance)
        {
            if (__instance.allowedTech.Length == 2)
            {
                __instance.allowedTech = new TechType[3] { TechType.WhirlpoolTorpedo, TechType.GasTorpedo, TechType.CyclopsDecoy };
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch("OnCraftEnd")]
        public static bool OnCraftEndPrefix(SeamothStorageContainer __instance, TechType techType)
        {
            if (ConfigToEdit.freeTorpedos.Value == 2)
                return true;

            __instance.Init();
            if (techType == TechType.SeamothTorpedoModule || techType == TechType.ExosuitTorpedoArmModule)
                __instance.StartCoroutine(OnCraftEndAsync(__instance));

            return false;
        }

        public static IEnumerator OnCraftEndAsync(SeamothStorageContainer __instance)
        {
            TaskResult<GameObject> taskResult = new TaskResult<GameObject>();
            for (int i = 0; i < ConfigToEdit.freeTorpedos.Value; ++i)
            {
                yield return CraftData.InstantiateFromPrefabAsync(TechType.WhirlpoolTorpedo, (IOut<GameObject>)taskResult);
                GameObject gameObject = taskResult.Get();
                if (gameObject != null)
                {
                    Pickupable pickupable = gameObject.GetComponent<Pickupable>();
                    if (pickupable != null)
                    {
                        pickupable.Pickup(false);
                        if (__instance.container.AddItem(pickupable) == null)
                            UnityEngine.Object.Destroy(pickupable.gameObject);
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(ItemsContainer))]
    class ItemsContainer_Patch
    {
        //[HarmonyPostfix]
        //[HarmonyPatch("IsTechTypeAllowed")]
        public static void IsTechTypeAllowedPostfix(ItemsContainer __instance, TechType techType, bool __result)
        {
            //AddDebug(__instance._label + " IsTechTypeAllowed " + techType + " " + __result);
        }
        [HarmonyPostfix]
        [HarmonyPatch("SetAllowedTechTypes")]
        public static void SetAllowedTechTypesPostfix(ItemsContainer __instance, ref TechType[] allowedTech)
        {
            if (__instance._label == "VehicleTorpedoStorageLabel")
            {
                //AddDebug(__instance._label + " type " + __instance.containerType);
                __instance.allowedTech.Add(TechType.CyclopsDecoy);
            }
        }
    }

    //[HarmonyPatch(typeof(SeamothTorpedo), "Awake")]
    class SeamothTorpedo_Awake_Patch
    {
        static void Postfix(SeamothTorpedo __instance)
        {
            //AddDebug("SeamothTorpedo Awake ");
            if (__instance.fireSound)
            {
                //AddDebug("SeamothTorpedo Awake fireSound");

            }
        }

    }



}


