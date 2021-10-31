using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using HarmonyLib;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    public class Spawnables
    {
        //4594f9c0-1b4d-4b10-871d-53950de686fb
        //static string floatingStone = "a1f3da68-d810-44ff-a0a2-6cf3c6a3eff5";

        public class Stone : Spawnable
        {
            //public static TechType TechTypeID { get; protected set; }
            public Stone()
            : base("TF_Stone", "", "")
            {
                //OnFinishedPatching += () =>
                //{ };
            }
            public override IEnumerator GetGameObjectAsync(IOut<GameObject> gameObject)
            {
                UWE.IPrefabRequest request = UWE.PrefabDatabase.GetPrefabAsync("3e581d94-c873-4ad7-a2f4-a35ec6ac3ecb"); // Rock_Small01
                yield return request;
                GameObject prefab;
                request.TryGetPrefab(out prefab);
                gameObject.Set(GameObject.Instantiate(prefab));
                yield break;
            }
            public override List<SpawnLocation> CoordinatedSpawns => new List<SpawnLocation>()
            {
                new SpawnLocation(new Vector3(18.71f, -26.35f, -155.85f), new Vector3(20f, 350f, 11f)),
                new SpawnLocation(new Vector3(348.3f, -25.3f, -205.1f), new Vector3(0f, 266f, 325f)),
                new SpawnLocation(new Vector3(-637f, -110.5f, -49.2f), new Vector3(355f, 0f, 90f)),
                new SpawnLocation(new Vector3(-185f, -42f, 138.5f), new Vector3(0f, 270f, 138f)),
                new SpawnLocation(new Vector3(-63.85f, -16f, -223f), new Vector3(270f, 321f, 0f)),// scale 1.1
            };
        }

    }
}