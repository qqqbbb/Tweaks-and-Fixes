
using HarmonyLib;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    static class PDA_Clock_
    {
        public static GameObject PDA_ClockGO { get; set; }
        public static PDA_Clock PDA_Clock__;

        public class PDA_Clock : MonoBehaviour
        {
            private TextMeshProUGUI textComponent;
            //private const float oneHour = 0.0416666679084301f;

            private void Awake()
            {
                textComponent = GetComponent<TextMeshProUGUI>();
                textComponent.fontSize = 50;
                textComponent.color = Color.white;
            }

            private void Start()
            {
                //AddDebug("PDA clock start");
                //transform.GetChild(0).gameObject.SetActive(false);
                Destroy(transform.GetChild(0).gameObject);
                PDA_Clock__ = this;
                //InvokeRepeating("ApplyTimeToText", 0f, 1f);
                //Player.main.StartCoroutine(ApplyTimeToText());
            }

            public IEnumerator ApplyTimeToText()
            {
                while (Player.main.pda.isInUse)
                {
                    DateTime dateTime = DayNightCycle.ToGameDateTime(DayNightCycle.main.timePassedAsFloat);
                    textComponent.text = dateTime.Hour.ToString("00") + " : " + dateTime.Minute.ToString("00");
                    yield return new WaitForSeconds(1);
                    //AddDebug($"ApplyTimeToText isOpen {Player.main.pda.isOpen} isInUse {Player.main.pda.isInUse}");
                    //AddDebug("ApplyTimeToText " + gameObject.activeSelf + " " + gameObject.activeInHierarchy);
                    //float dayScalar = DayNightCycle.main.GetDayScalar();
                    //int minutes = Mathf.FloorToInt(dayScalar % oneHour / oneHour * 60f);
                    //int hours = Mathf.FloorToInt(dayScalar * 24f);
                }
            }
        }

        [HarmonyPatch(typeof(uGUI_InventoryTab))]
        static class uGUI_InventoryTab_Patch
        {
            [HarmonyPrefix, HarmonyPatch("OnOpenPDA")]
            private static void OnOpenPDAPrefix()
            {
                //AddDebug($"OnOpenPDA isOpen {Player.main.pda.isOpen} isInUse {Player.main.pda.isInUse}");
                if (ConfigToEdit.pdaClock.Value)
                    Player.main.StartCoroutine(PDA_Clock__.ApplyTimeToText());
            }
            [HarmonyPostfix, HarmonyPatch("Awake")]
            private static void AwakePostfix(uGUI_InventoryTab __instance)
            {
                //GameObject label = CreateGameObject(__instance, "TimeLabel", -280f);
                //PDA_Clock.TimeLabelObject = label;
                //label.GetComponent<Text>().text = "TIME";
                if (!ConfigToEdit.pdaClock.Value)
                    return;

                PDA_ClockGO = UnityEngine.Object.Instantiate(__instance.storageLabel.gameObject, __instance.gameObject.transform);
                PDA_ClockGO.name = "TweaksFixesTimeDisplay";
                Vector3 localPosition = PDA_ClockGO.transform.localPosition;
                PDA_ClockGO.transform.localPosition = new Vector3(localPosition.x, -350f, localPosition.z);
                PDA_ClockGO.AddComponent<PDA_Clock>();
            }
        }

        [HarmonyPatch(typeof(uGUI_Equipment), "Init")]
        static class uGUI_Equipment_Init_Patch
        {
            private static void Postfix(uGUI_Equipment __instance, Equipment equipment)
            {
                if (!ConfigToEdit.pdaClock.Value)
                    return;

                //AddDebug("uGUI_Equipment Init");
                //uGUI_PDAScreen
                if (equipment.GetCompatibleSlot(EquipmentType.Body, out string str))
                    PDA_ClockGO.SetActive(true);
                else
                    PDA_ClockGO.SetActive(false);
            }
        }
    }



}