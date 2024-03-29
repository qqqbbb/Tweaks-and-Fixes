
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    static class PDA_Clock_
    {
        public static GameObject PDA_ClockGO { get; set; }

        public class PDA_Clock : MonoBehaviour
        {
            private TextMeshProUGUI textComponent;
            private const float oneHour = 0.0416666679084301f;
            //public static GameObject TimeLabelObject { private get; set; }
            bool blink = false;

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
                InvokeRepeating("ApplyTimeToText", 0f, 1f);
            }

            private void ApplyTimeToText()
            {
                if (!gameObject.activeInHierarchy)
                    return;
                //AddDebug("ApplyTimeToText " + gameObject.activeSelf + " " + gameObject.activeInHierarchy);
                float dayScalar = DayNightCycle.main.GetDayScalar();
                int minutes = Mathf.FloorToInt(dayScalar % oneHour / oneHour * 60f);
                int hours = Mathf.FloorToInt(dayScalar * 24f);
                //string str1 = "";
                //string str2 = dayScalar < 0.75 ? (dayScalar < 0.5 ? (dayScalar < 0.25 ? "Midnight" : "Morning") : "Noon") : "Evening";
                //this.textComponent.text = num2.ToString("00") + ":" + num1.ToString("00") + " " + str1 + " (" + str2 + ")";
                textComponent.text = blink ? hours.ToString("00") + ":" + minutes.ToString("00") : hours.ToString("00") + " " + minutes.ToString("00");
                blink = !blink;
            }
        }

        [HarmonyPatch(typeof(uGUI_InventoryTab), "OnOpenPDA")]
        static class uGUI_InventoryTab_OnOpenPDA_Patch
        {
            private static void Prefix()
            {
                if (!ConfigToEdit.pdaClock.Value)
                    return;
                PDA_ClockGO.SetActive(false);
            }
        }

        [HarmonyPatch(typeof(uGUI_Equipment), "Init")]
        static class uGUI_Equipment_Init_Patch
        {
            private static void Postfix(uGUI_Equipment __instance, Equipment equipment)
            {
                if (!ConfigToEdit.pdaClock.Value)
                    return;

                if (equipment.GetCompatibleSlot(EquipmentType.Body, out string str))
                    PDA_ClockGO.SetActive(true);
                else
                    PDA_ClockGO.SetActive(false);
            }
        }

        [HarmonyPatch(typeof(uGUI_InventoryTab), "Awake")]
        static class uGUI_InventoryTab_Awake_Patch
        {
            private static void Postfix(uGUI_InventoryTab __instance)
            {
                //GameObject label = CreateGameObject(__instance, "TimeLabel", -280f);
                //PDA_Clock.TimeLabelObject = label;
                //label.GetComponent<Text>().text = "TIME";
                if (!ConfigToEdit.pdaClock.Value)
                    return;

                PDA_ClockGO = Object.Instantiate(__instance.storageLabel.gameObject, __instance.gameObject.transform);
                PDA_ClockGO.name = "TimeDisplayText";
                Vector3 localPosition = PDA_ClockGO.transform.localPosition;
                PDA_ClockGO.transform.localPosition = new Vector3(localPosition.x, -350f, localPosition.z);
                PDA_ClockGO.AddComponent<PDA_Clock>();
            }


        }
    }
}