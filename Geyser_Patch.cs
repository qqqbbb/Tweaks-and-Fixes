using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tweaks_Fixes;
using UnityEngine;

namespace Tweaks_Fixes
{

        [HarmonyPatch(typeof(Geyser))]
        class Geyser_Patch
        {
            public static GameObject fishToCook;
            public static Dictionary<Geyser, Vector3> eruptionForce = new Dictionary<Geyser, Vector3>();
            public static Dictionary<Geyser, Vector3> rotationForce = new Dictionary<Geyser, Vector3>();

            [HarmonyPrefix]
            [HarmonyPatch("Start")]
            static bool StartPrefix(Geyser __instance)
            {
                foreach (Transform childTransform in __instance.transform)
                {
                    if (childTransform.name != "Model")
                        UnityEngine.Object.Destroy(childTransform.gameObject);
                }
                GameObject go = Utils.SpawnZeroedAt(__instance.warningSmokeParticles, __instance.transform);
                __instance.warningSmokeEmitter = go.GetComponent<ParticleSystem>();
                go = Utils.SpawnZeroedAt(__instance.eruptionParticles, __instance.transform);
                __instance.eruptionEmitter = go.GetComponent<ParticleSystem>();
                __instance.warningSmokeEmitter.Play();
                __instance.eruptionEmitter.Stop();

                float eruptionIntervalvariance = ConfigToEdit.lavaGeyserEruptionInterval.Value * .5f;
                float nextEruptTime = ConfigToEdit.lavaGeyserEruptionInterval.Value + UnityEngine.Random.value * eruptionIntervalvariance;
                __instance.Invoke("Erupt", UnityEngine.Random.value * nextEruptTime);
                //Main.logger.LogDebug("Geyser Start nextEruptTime " + nextEruptTime);
                if (Geyser.consoleCmdRegged)
                    return false;

                DevConsole.RegisterConsoleCommand(__instance, "erupt");
                Geyser.consoleCmdRegged = true;
                return false;
            }
            [HarmonyPostfix]
            [HarmonyPatch("Start")]
            static void StartPostfix(Geyser __instance)
            {
                int x = (int)__instance.transform.position.x;
                int y = (int)__instance.transform.position.y;
                int z = (int)__instance.transform.position.z;
                //Main.logger.LogDebug("Geyser Start  " + x + " " + z);
                bool fix = x == 961 && z == 470 || x == 965 && z == 625 || x == -175 && z == 1024 || x == -153 && z == 956 || x == -70 && z == 1024 || x == -67 && z == 1066 || x == -80 && z == 968 || x == -78 && z == 930 || x == -32 && z == 973;
                if (fix)
                { // fix safe spot at bottom of geyser    
                    CapsuleCollider cc = __instance.GetComponent<CapsuleCollider>();
                    if (cc)  // 24
                        cc.height = 30;
                }
                if (ConfigToEdit.removeLavaGeyserRockParticles.Value)
                    __instance.StartCoroutine(RemoveRockParticles(__instance));
            }
            [HarmonyPrefix]
            [HarmonyPatch("Erupt")]
            static bool EruptPrefix(Geyser __instance)
            {
                if (__instance.erupting || !__instance.gameObject.activeInHierarchy)
                    return false;

                Vector3 force = CalculateGeyserForce();
                eruptionForce[__instance] = force;
                float rot = UnityEngine.Random.Range(-force.y, force.y);
                rotationForce[__instance] = new Vector3(rot * UnityEngine.Random.value, 0, rot * UnityEngine.Random.value);
                //AddDebug("Erupt " + eruptionForce[__instance]);
                __instance.erupting = true;
                __instance.eruptionEmitter.Play();
                __instance.warningSmokeEmitter.Stop();
                Utils.PlayEnvSound("event:/env/geyser_erupt", __instance.transform.position);
                __instance.Invoke("EndErupt", __instance.eruptionLength);
                return false;
            }
            [HarmonyPostfix]
            [HarmonyPatch("Erupt")]
            static void EruptPostfix(Geyser __instance)
            {
                float eruptionIntervalvariance = ConfigToEdit.lavaGeyserEruptionInterval.Value * .5f;
                float nextEruptTime = ConfigToEdit.lavaGeyserEruptionInterval.Value + UnityEngine.Random.value * eruptionIntervalvariance;
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

            [HarmonyPostfix]
            [HarmonyPatch("OnTriggerStay")]
            static void OnTriggerStayPostfix(Geyser __instance, Collider other)
            {
                if (!__instance.erupting)
                    return;

                GameObject go = Util.GetEntityRoot(other.gameObject);
                if (!go)
                    go = other.gameObject;
                //AddDebug("Geyser OnTriggerStay " + go.name);
                if (Util.IsEatableFish(go) && Util.IsDead(go))
                {
                    if (fishToCook == go)
                    { // wait 1 frame so we dont get 2 cooked fishes when geyser kills fish
                        fishToCook = null;
                        //Main.logger.LogDebug("Geyser OnTriggerStay " + go.name);
                        Player.main.StartCoroutine(Util.Cook(go));
                    }
                    else
                        fishToCook = go;
                }
                //if (other.gameObject.tag == "Player")
                //    AddDebug("Geyser OnTriggerStay " + other.gameObject.tag);

                //Rigidbody rb = other.GetComponentInChildren<Rigidbody>();
                if (ConfigToEdit.lavaGeyserEruptionForce.Value == 0)
                    return;
            
                Rigidbody rb = go.GetComponentInChildren<Rigidbody>();
                if (rb != null)
                {
                    if (eruptionForce.ContainsKey(__instance))
                    {
                        //AddDebug("Geyser AddForce " + rb.name + " " + rb.mass);
                        rb.AddForce(eruptionForce[__instance]);
                    }
                    if (rotationForce.ContainsKey(__instance))
                        rb.AddTorque(rotationForce[__instance]);
                }
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
