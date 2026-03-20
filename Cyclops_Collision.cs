using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    class Cyclops_Collision
    {
        static string[] hatches = { "submarine_hatch_02", "submarine_hatch_02 1", "submarine_hatch_02 3", "submarine_hatch_02 4", "submarine_hatch_02 7" };

        [HarmonyPatch(typeof(SubRoot))]
        class SubRoot_Patch
        {
            [HarmonyPostfix, HarmonyPatch("Start")]
            public static void StartPostfix(SubRoot __instance)
            {// Start runs for prefab too
                if (__instance.isCyclops && ConfigToEdit.fixCyclopsCollision.Value)
                {
                    //AddDebug("SubRoot Start " + __instance.name);
                    //Main.logger.LogMessage("SubRoot Start " + __instance.name);
                    FixCollision(__instance);
                    //fixCyclopsCollision = false;
                    //ShowColliders(__instance);
                }
            }

            //[HarmonyPostfix, HarmonyPatch(nameof(SubRoot.Update))]
            public static void UpdatePostfix(SubRoot __instance)
            {
                if (Keyboard.current.xKey.wasPressedThisFrame)
                {
                    ShowColliders(__instance);
                }
                else if (Keyboard.current.zKey.wasPressedThisFrame)
                {
                    RemoveColliders(__instance);
                }
            }

            private static void RemoveColliders(SubRoot subRoot)
            {
                if (subRoot.isCyclops == false || subRoot.name != "Cyclops-MainPrefab(Clone)")
                    return;

                AddDebug("RemoveColliders " + subRoot.name);
                Transform collisionTr = subRoot.transform.Find("CyclopsCollision");
                if (collisionTr)
                {
                    Collider[] colliders = collisionTr.GetComponentsInChildren<Collider>();
                    foreach (Collider collider in colliders)
                    {
                        foreach (Transform child in collider.transform)
                        {
                            if (child.name == "Debug collider")
                                UnityEngine.Object.Destroy(child.gameObject);
                        }
                    }
                }
            }

            private static void ShowColliders(SubRoot subRoot)
            {
                if (subRoot.isCyclops == false || subRoot.name != "Cyclops-MainPrefab(Clone)")
                    return;

                AddDebug("ShowColliders " + subRoot.name);
                Transform collisionTr = subRoot.transform.Find("CyclopsCollision");
                if (collisionTr)
                {
                    Collider[] colliders = collisionTr.GetComponentsInChildren<Collider>();
                    foreach (Collider collider in colliders)
                        CreateDebugCollider(collider);
                }
            }

            public static void CreateDebugCollider(Collider collider)
            {
                PrimitiveType pt = PrimitiveType.Cube;
                if (collider is CapsuleCollider)
                    pt = PrimitiveType.Capsule;
                else if (collider is SphereCollider)
                    pt = PrimitiveType.Sphere;

                GameObject debugCollider = GameObject.CreatePrimitive(pt);
                debugCollider.name = "Debug collider";
                UnityEngine.Object.DestroyImmediate(debugCollider.GetComponent<Collider>());
                //debugCollider.GetComponent<MeshRenderer>().material.color = Color.white;
                //debugCollider.GetComponent<MeshRenderer>().material.color = new Color(1f, 0f, 0f);
                Material unlitMaterial = new Material(Shader.Find("Unlit/Color"));
                unlitMaterial.color = new Color(0f, 0f, 1f);
                debugCollider.GetComponent<MeshRenderer>().material = unlitMaterial;
                debugCollider.SetActive(true);
                debugCollider.transform.SetParent(collider.transform, false);
                debugCollider.transform.localEulerAngles = Vector3.zero;
                Testing.MatchColliderSize(debugCollider, collider);
                //AddDebug("ShowDebugCollider " + collider.name);
            }

            private static void FixCollision(SubRoot subRoot)
            {
                //if (subRoot.isCyclops == false || subRoot.name == "Cyclops-MainPrefab(Clone)")
                if (subRoot.isCyclops == false || subRoot.name != "__LIGHTMAPPED_PREFAB__")
                    return;

                //AddDebug("FixCollision " + subRoot.name);
                //Rigidbody rb = subRoot.GetComponent<Rigidbody>();
                //rb.freezeRotation = true;
                //Util.FreezeObject(subRoot.gameObject, true);

                Transform animMeshes = subRoot.transform.Find("CyclopsMeshAnimated");
                foreach (string hatchName in hatches)
                {
                    Transform hatch = animMeshes.Find(hatchName);
                    FixHatch(hatch);
                }
                Transform roundHatch = animMeshes.Find("submarine_hatch_01");
                if (roundHatch)
                {
                    BoxCollider[] colliders = roundHatch.GetComponentsInChildren<BoxCollider>();
                    for (int i = 0; i < colliders.Length; i++)
                    {
                        BoxCollider collider = colliders[i];
                        if (i == 1)
                        {
                            collider.size = new Vector3(collider.size.x, 1, 1);
                            collider.center = new Vector3(collider.center.x, -1.2f, collider.center.z);
                        }
                        else if (i == 2)
                            UnityEngine.Object.Destroy(collider);
                    }
                }
                Transform decoyLoadingTube = subRoot.transform.Find("DecoyLoadingTube");
                if (decoyLoadingTube)
                {
                    BoxCollider collider = decoyLoadingTube.GetComponent<BoxCollider>();
                    collider.size = new Vector3(0.7f, 0.7f, 0.2f);
                    collider.center = new Vector3(-0.05f, 0, 0);
                }
                Transform cyclopsFabricator = subRoot.transform.Find("CyclopsFabricator(Clone)");
                if (cyclopsFabricator)
                {
                    BoxCollider collider = cyclopsFabricator.GetComponent<BoxCollider>();
                    UnityEngine.Object.Destroy(collider);
                    Transform mesh = cyclopsFabricator.transform.Find("submarine_fabricator_03");
                    collider = mesh.GetComponent<BoxCollider>();
                    collider.center = new Vector3(collider.center.x, collider.center.y, 0);
                    collider.size = new Vector3(collider.size.x, collider.size.y, .20f);
                }
                Transform cyclopsCollision = subRoot.transform.Find("CyclopsCollision");
                Transform generator = subRoot.transform.Find("cyclopspower/generator");
                if (generator != null)
                {
                    SphereCollider[] sphereColliders = generator.GetComponentsInChildren<SphereCollider>();
                    foreach (var c in sphereColliders)
                        c.isTrigger = true;
                }
                Transform hatchWallPlayerBlocker = subRoot.transform.Find("HatchWallPlayerBlocker");
                if (hatchWallPlayerBlocker)
                    UnityEngine.Object.Destroy(hatchWallPlayerBlocker.gameObject);

                Transform outer = cyclopsCollision.Find("zOuterGroup");
                if (outer)
                {
                    Transform t = outer.GetChild(3); // left_wing
                    t.localEulerAngles = new Vector3(90, 90, 0);
                    BoxCollider collider = t.GetComponent<BoxCollider>();
                    collider.center = new Vector3(collider.center.x, 17.55f, collider.center.z);
                    t = outer.GetChild(4); // right_wing
                    t.localEulerAngles = new Vector3(90, 90, 0);
                    t.position = new Vector3(t.position.x, t.position.y, 54.3f);
                    t = outer.GetChild(9); // left_wing
                    t.localEulerAngles = new Vector3(90, 90, 0);
                    BoxCollider[] colliders = t.GetComponents<BoxCollider>();
                    collider = colliders[0];
                    collider.center = new Vector3(collider.center.x, 17.1f, collider.center.z);
                    collider = colliders[1];
                    collider.center = new Vector3(collider.center.x, 9.55f, collider.center.z);
                    t = outer.GetChild(10); // right_side
                    collider = t.GetComponent<BoxCollider>();
                    collider.center = new Vector3(collider.center.x, 9.3f, collider.center.z);
                    t = outer.GetChild(11); // left_side
                    collider = t.GetComponent<BoxCollider>();
                    collider.center = new Vector3(collider.center.x, 17.70f, collider.center.z);
                }
                Transform controlRoomWall = cyclopsCollision.Find("controlRoomGroup/right_wall");
                if (controlRoomWall)
                {
                    controlRoomWall.localEulerAngles = new Vector3(90, 90, 0);
                    controlRoomWall.localPosition = new Vector3(-0.32f, 0, 0);
                    BoxCollider[] colliders = controlRoomWall.GetComponents<BoxCollider>();
                    foreach (BoxCollider collider in colliders)
                        collider.size = new Vector3(collider.size.x, .01f, collider.size.z);
                }
                Transform secondRoom = cyclopsCollision.Find("secondRoomGroup");
                Transform bottomDeck = secondRoom.Find("bottom_deck");
                if (bottomDeck)
                {
                    BoxCollider[] colliders = bottomDeck.GetComponents<BoxCollider>();
                    BoxCollider collider = colliders[1];
                    collider.size = new Vector3(collider.size.x, collider.size.y, .1f);
                    collider.center = new Vector3(collider.center.x, 6.8f, 22.15f);
                    collider = colliders[2];
                    collider.size = new Vector3(collider.size.x, collider.size.y, .1f);
                    collider.center = new Vector3(collider.center.x, 6.8f, 22.15f);
                    collider = colliders[3];
                    collider.size = new Vector3(collider.size.x, 1.1f, .1f);
                    collider.center = new Vector3(collider.center.x, 7.87f, 22.15f);
                    collider = colliders[4];
                    collider.size = new Vector3(collider.size.x, collider.size.y, .2f);
                    collider.center = new Vector3(collider.center.x, 6.8f, collider.center.z);
                    collider = colliders[5];
                    collider.size = new Vector3(collider.size.x, 1.1f, .2f);
                    collider.center = new Vector3(collider.center.x, 7.8f, collider.center.z);
                    collider = colliders[6];
                    collider.size = new Vector3(collider.size.x, collider.size.y, .2f);
                    collider.center = new Vector3(collider.center.x, 6.8f, collider.center.z);
                }
                Transform secondRoomRightWall = secondRoom.Find("right_wall");
                if (secondRoomRightWall)
                {
                    secondRoomRightWall.localEulerAngles = new Vector3(90, 90, 0); // this makes cyclops drift
                    BoxCollider[] colliders = secondRoomRightWall.GetComponents<BoxCollider>();
                    foreach (BoxCollider collider in colliders)
                        collider.size = new Vector3(collider.size.x, .01f, collider.size.z);

                    GameObject leftWall = GameObject.Instantiate(secondRoomRightWall.gameObject);
                    GameObject rightWall = GameObject.Instantiate(secondRoomRightWall.gameObject);
                    leftWall.name = "left_Wall";
                    rightWall.name = "right_Wall_";
                    leftWall.transform.SetParent(secondRoom);
                    rightWall.transform.SetParent(secondRoom);
                    rightWall.transform.localPosition = new Vector3(-0.88f, -0.5f, 0);
                    rightWall.transform.localEulerAngles = new Vector3(86, 270, 180);
                    leftWall.transform.localPosition = new Vector3(0.1f, -0.5f, 0);
                    leftWall.transform.localEulerAngles = new Vector3(86, 90, 0);
                    colliders = leftWall.GetComponents<BoxCollider>();
                    UnityEngine.Object.Destroy(colliders[1]);
                    colliders = rightWall.GetComponents<BoxCollider>();
                    UnityEngine.Object.Destroy(colliders[0]);
                    UnityEngine.Object.Destroy(secondRoomRightWall.gameObject);
                }
                Transform engineRoom = cyclopsCollision.Find("engineRoomGroup");
                if (engineRoom)
                {
                    Transform leftWall = engineRoom.GetChild(1);
                    leftWall.localEulerAngles = new Vector3(leftWall.localEulerAngles.x, 86.1f, leftWall.localEulerAngles.z);
                    BoxCollider collider = leftWall.GetComponent<BoxCollider>();
                    collider.size = new Vector3(collider.size.x, .01f, collider.size.z);
                    collider.center = new Vector3(collider.center.x, 10.9f, collider.center.z);
                    Transform rightWall = engineRoom.GetChild(2);
                    rightWall.localEulerAngles = new Vector3(rightWall.localEulerAngles.x, 94.05f, rightWall.localEulerAngles.z);
                    collider = rightWall.GetComponent<BoxCollider>();
                    collider.size = new Vector3(collider.size.x, .01f, collider.size.z);
                    collider.center = new Vector3(collider.center.x, -0.52f, collider.center.z);

                    Transform bottomDeck_ = engineRoom.GetChild(0);
                    BoxCollider[] colliders = bottomDeck_.GetComponents<BoxCollider>();
                    collider = colliders[0];
                    collider.center = new Vector3(3.1f, 7.8f, 39.63f);
                    collider.size = new Vector3(collider.size.x, 1.1f, 0.22f);
                    collider = colliders[1];
                    collider.center = new Vector3(collider.center.x, 6.8f, 39.63f);
                    collider.size = new Vector3(collider.size.x, 3.2f, 0.22f);
                    collider = colliders[2];
                    collider.center = new Vector3(collider.center.x, 7.8f, 39.63f);
                    collider.size = new Vector3(collider.size.x, 1.1f, 0.22f);
                }
                Transform keelFrontGroup = cyclopsCollision.Find("keelFrontGroup");
                Transform wall = keelFrontGroup.Find("proppeler");
                if (wall)
                {
                    BoxCollider[] colliders = wall.GetComponents<BoxCollider>();
                    BoxCollider collider = colliders[0];
                    collider.size = new Vector3(collider.size.x, collider.size.y, 0.2f);
                    collider = colliders[1];
                    collider.size = new Vector3(collider.size.x, collider.size.y, 0.2f);
                    collider = colliders[2];
                    collider.size = new Vector3(collider.size.x, collider.size.y, 0.2f);
                    collider.center = new Vector3(collider.center.x, 3, collider.center.z);
                }
                Transform rightLowerWall = keelFrontGroup.Find("right_wall");
                if (rightLowerWall)
                {
                    rightLowerWall.localPosition = new Vector3(-.25f, 0f, 0f);
                    rightLowerWall.eulerAngles = new Vector3(80f, rightLowerWall.eulerAngles.y, rightLowerWall.eulerAngles.z);
                }
                Transform leftLowerWall = keelFrontGroup.Find("left_wall");
                {
                    leftLowerWall.localPosition = new Vector3(-0.035f, 0, 1.4f);
                    leftLowerWall.eulerAngles = new Vector3(81, leftLowerWall.eulerAngles.y, leftLowerWall.eulerAngles.z);
                    BoxCollider collider = leftLowerWall.GetComponent<BoxCollider>();
                    collider.size = new Vector3(9.8f, 0.4f, collider.size.z);
                    GameObject wall_ = new GameObject("wall_");
                    wall_.transform.SetParent(leftLowerWall);
                    wall_.transform.localEulerAngles = Vector3.zero;
                    wall_.transform.localPosition = new Vector3(-15.3f, 6.57f, -2.8f);
                    BoxCollider newCollider = wall_.AddComponent<BoxCollider>();
                    newCollider.size = new Vector3(3.8f, 0.1f, 4f);
                    //Testing.ShowDebugCollider(newCollider);
                }
                Transform keel = keelFrontGroup.Find("keel");
                GameObject ramp = null;
                if (keel)
                {
                    BoxCollider[] colliders = keel.GetComponents<BoxCollider>();
                    BoxCollider collider = colliders[0];
                    collider.center = new Vector3(5.25f, collider.center.y, 26.6f);
                    collider.size = new Vector3(2.4f, collider.size.y, 2.3f);
                    collider = colliders[1];
                    collider.center = new Vector3(collider.center.x, collider.center.y, 21.5f);
                    collider = colliders[2];
                    collider.center = new Vector3(collider.center.x, collider.center.y, 14.64f);
                    collider.size = new Vector3(collider.size.x, collider.size.y, 0.56f);
                    //ramp = GameObject.Instantiate(keel.gameObject);
                    ramp = new GameObject("ramp");
                    ramp.name = "ramp";
                    ramp.transform.SetParent(keelFrontGroup);
                    ramp.transform.localEulerAngles = new Vector3(30, 0, 0);
                    ramp.transform.localPosition = new Vector3(5.1f, -0.29f, 25.71f);
                    collider = ramp.AddComponent<BoxCollider>();
                    collider.size = new Vector3(2, 0, 0.5f);
                    //Testing.ShowDebugCollider(collider);
                }
                Transform keelBackGroup = cyclopsCollision.Find("keelBackGroup");
                if (keelBackGroup && ramp)
                {
                    GameObject ramp_ = GameObject.Instantiate(ramp);
                    ramp_.name = "ramp";
                    ramp_.transform.SetParent(keelBackGroup);
                    ramp_.transform.localEulerAngles = new Vector3(-30, 0, 0);
                    ramp_.transform.localPosition = new Vector3(4.5f, 0.456f, -7.155f);
                    BoxCollider collider = ramp_.GetComponent<BoxCollider>();
                    collider.size = new Vector3(2, 0, 0.5f);
                    Transform backWall = keelBackGroup.GetChild(0);
                    BoxCollider[] colliders = backWall.GetComponents<BoxCollider>();
                    collider = colliders[3];
                    collider.center = new Vector3(collider.center.x, collider.center.y, 8);
                    collider.size = new Vector3(collider.size.x, collider.size.y, 1.3f);

                    Transform leftWall = keelBackGroup.GetChild(2);
                    leftWall.localEulerAngles = new Vector3(81.5f, 271, 182);
                    leftWall.localPosition = new Vector3(0.1f, 0, 0);
                    collider = leftWall.GetComponent<BoxCollider>();
                    collider.size = new Vector3(collider.size.x, .3f, collider.size.z);

                    Transform rightWall = keelBackGroup.GetChild(3);
                    rightWall.localEulerAngles = new Vector3(81, 91, 180);
                    rightWall.localPosition = new Vector3(0.01f, 0, 0);
                    collider = rightWall.GetComponent<BoxCollider>();
                    collider.size = new Vector3(collider.size.x, .3f, collider.size.z);

                    leftWall = keelBackGroup.GetChild(4);
                    leftWall.localEulerAngles = new Vector3(75, 47, 134f);
                    collider = leftWall.GetComponent<BoxCollider>();
                    collider.size = new Vector3(collider.size.x, .3f, collider.size.z);

                    leftWall = keelBackGroup.GetChild(5);
                    leftWall.localEulerAngles = new Vector3(72, 325.8f, 58);
                    leftWall.localPosition = new Vector3(-0.11f, 0, 0);
                    collider = leftWall.GetComponent<BoxCollider>();
                    collider.size = new Vector3(collider.size.x, .3f, collider.size.z);

                    leftWall = keelBackGroup.GetChild(6);
                    leftWall.localEulerAngles = new Vector3(81, 81.27f, 168);
                    collider = leftWall.GetComponent<BoxCollider>();
                    collider.size = new Vector3(collider.size.x, .3f, collider.size.z);

                    leftWall = keelBackGroup.GetChild(7);
                    leftWall.localEulerAngles = new Vector3(81, 274.5f, 8f);
                    leftWall.localPosition = new Vector3(-0.03f, 0, 0);
                }
                Transform launchBayGroup = cyclopsCollision.Find("launchBayGroup");
                Transform dividerWall = launchBayGroup.Find("proppeler");
                if (dividerWall)
                {
                    BoxCollider[] colliders = dividerWall.GetComponents<BoxCollider>();
                    for (int i = 0; i < colliders.Length; i++)
                    {
                        BoxCollider collider = colliders[i];
                        if (i == 2 || i == 3 || i == 7)
                        { // copies
                            UnityEngine.Object.Destroy(collider);
                            continue;
                        }
                        collider.size = new Vector3(collider.size.x, collider.size.y, 0.04f);
                    }
                }
                Transform launchBayFloor = launchBayGroup.Find("keel");
                if (launchBayFloor)
                {
                    launchBayFloor.localPosition = new Vector3(0, .08f, 0);
                }
                Transform launchBayright_wall = cyclopsCollision.Find("launchBayright_wall");
                if (launchBayright_wall)
                {
                    launchBayright_wall.localPosition = new Vector3(.22f, 0f, 0f);
                    launchBayright_wall.eulerAngles = new Vector3(80f, launchBayright_wall.eulerAngles.y, launchBayright_wall.eulerAngles.z);
                    BoxCollider collider = launchBayright_wall.GetComponent<BoxCollider>();
                    collider.size = new Vector3(collider.size.x, .2f, collider.size.z);
                }
                Transform launchBayleft_wall = cyclopsCollision.Find("launchBayleft_wall");
                if (launchBayleft_wall)
                {
                    launchBayleft_wall.localPosition = new Vector3(-0.23f, 0, 0);
                    launchBayleft_wall.eulerAngles = new Vector3(80f, launchBayleft_wall.eulerAngles.y, launchBayleft_wall.eulerAngles.z);
                    BoxCollider collider = launchBayleft_wall.GetComponent<BoxCollider>();
                    collider.size = new Vector3(collider.size.x, .2f, collider.size.z);
                }

            }

            private static void FixHatch(Transform hatch)
            {
                BoxCollider[] colliders = hatch.GetComponentsInChildren<BoxCollider>();
                for (int i = 0; i < colliders.Length; i++)
                {
                    BoxCollider collider = colliders[i];
                    if (i == 0)
                    {
                        collider.center = new Vector3(-0.59f, -0.06f, 0);
                        collider.size = new Vector3(0.98f, 0.2f, 1.72f);
                    }
                    else
                        UnityEngine.Object.Destroy(collider);
                }
            }


        }


    }
}
