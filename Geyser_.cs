using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{

    [HarmonyPatch(typeof(Geyser))]
    class Geyser_
    {
        static Dictionary<Geyser, Vector3> eruptionForce = new Dictionary<Geyser, Vector3>();
        static Dictionary<Geyser, Vector3> rotationForce = new Dictionary<Geyser, Vector3>();

        public static void CleanUp()
        {
            eruptionForce.Clear();
            rotationForce.Clear();
        }

        [HarmonyPrefix, HarmonyPatch("Start")]
        static void StartPrefix(Geyser __instance)
        {
            DestroyUnusedGOs(__instance);
        }

        private static void DestroyUnusedGOs(Geyser geyser)
        {
            foreach (Transform childTransform in geyser.transform)
            {
                if (childTransform.name != "Model")
                    UnityEngine.Object.Destroy(childTransform.gameObject);
            }
        }

        [HarmonyPostfix, HarmonyPatch("Start")]
        static void StartPostfix(Geyser __instance)
        {
            __instance.CancelInvoke();
            float eruptionIntervalVariance = ConfigToEdit.lavaGeyserEruptionInterval.Value * .5f;
            float nextEruptTime = ConfigToEdit.lavaGeyserEruptionInterval.Value + UnityEngine.Random.value * eruptionIntervalVariance;
            __instance.Invoke("Erupt", UnityEngine.Random.value * nextEruptTime);
            int x = (int)__instance.transform.position.x;
            int z = (int)__instance.transform.position.z;
            //Main.logger.LogDebug("Geyser Start  " + x + " " + z);
            bool fix = x == 961 && z == 470 || x == 965 && z == 625 || x == -175 && z == 1024 || x == -153 && z == 956 || x == -70 && z == 1024 || x == -67 && z == 1066 || x == -80 && z == 968 || x == -78 && z == 930 || x == -32 && z == 973;
            if (fix)
            { // remove safe spot at bottom of geyser    
                CapsuleCollider cc = __instance.GetComponent<CapsuleCollider>();
                if (cc)  // 24
                    cc.height = 30;
            }
            if (ConfigToEdit.removeLavaGeyserRockParticles.Value)
                __instance.StartCoroutine(RemoveRockParticles(__instance));
        }
        [HarmonyPrefix, HarmonyPatch("Erupt")]
        static void EruptPrefix(Geyser __instance)
        {
            if (__instance.erupting || !__instance.gameObject.activeInHierarchy)
                return;

            Vector3 force = CalculateGeyserForce();
            eruptionForce[__instance] = force;
            float rot = UnityEngine.Random.Range(-force.y, force.y);
            rotationForce[__instance] = new Vector3(rot * UnityEngine.Random.value, 0, rot * UnityEngine.Random.value);
        }
        [HarmonyPostfix, HarmonyPatch("Erupt")]
        static void EruptPostfix(Geyser __instance)
        {
            float eruptionIntervalVariance = ConfigToEdit.lavaGeyserEruptionInterval.Value * .5f;
            float nextEruptTime = ConfigToEdit.lavaGeyserEruptionInterval.Value + UnityEngine.Random.value * eruptionIntervalVariance;
            __instance.Invoke("Erupt", nextEruptTime);
        }

        private static IEnumerator RemoveRockParticles(Geyser geyser)
        {
            yield return new WaitForSeconds(1);
            Transform t = geyser.transform.Find("xGeyser_Warning(Clone)/xMeshFrag");
            if (t)
            {
                UnityEngine.Object.Destroy(t.gameObject);
                //t.gameObject.SetActive(false);
                //Main.logger.LogDebug("Geyser xGeyser_Warning activeInHierarchy " + t.gameObject.activeInHierarchy);
            }
            t = geyser.transform.Find("xGeyser_Warning(Clone)/xAshes");
            if (t)
                UnityEngine.Object.Destroy(t.gameObject);

            t = geyser.transform.Find("xGeyserShort_Eruption(Clone)/xMeshFrag");
            if (t)
                UnityEngine.Object.Destroy(t.gameObject);

            t = geyser.transform.Find("xGeyser_Eruption(Clone)/xMeshFrag");
            if (t)
                UnityEngine.Object.Destroy(t.gameObject);

            t = geyser.transform.Find("xGeyser_Eruption(Clone)/xAshes");
            if (t)
                UnityEngine.Object.Destroy(t.gameObject);

            t = geyser.transform.Find("xGeyserShort_Eruption(Clone)/xAshes");
            if (t)
                UnityEngine.Object.Destroy(t.gameObject);
        }

        [HarmonyPostfix, HarmonyPatch("OnTriggerStay")]
        static void OnTriggerStayPostfix(Geyser __instance, Collider other)
        {
            if (!__instance.erupting || ConfigToEdit.lavaGeyserEruptionForce.Value == 0)
                return;

            GameObject go = Util.GetEntityRoot(other.gameObject);
            if (!go)
                go = other.gameObject;
            //AddDebug("Geyser OnTriggerStay " + go.name);

            //if (other.gameObject.tag == "Player")
            //    AddDebug("Geyser OnTriggerStay " + other.gameObject.tag);

            Rigidbody rb = go.GetComponentInChildren<Rigidbody>();
            if (rb == null)
                return;

            if (eruptionForce.ContainsKey(__instance))
            {
                //AddDebug("Geyser AddForce " + rb.name + " " + rb.mass);
                rb.AddForce(eruptionForce[__instance]);
            }
            if (rotationForce.ContainsKey(__instance))
                rb.AddTorque(rotationForce[__instance]);
        }

        private static Vector3 CalculateGeyserForce()
        {
            float force = ConfigToEdit.lavaGeyserEruptionForce.Value;
            float xForce = UnityEngine.Random.Range(-force, force) * UnityEngine.Random.value;
            float yForce = force + force * UnityEngine.Random.value;
            float zForce = UnityEngine.Random.Range(-force, force) * UnityEngine.Random.value;
            return new Vector3(xForce, yForce, zForce);
        }
    }




}
