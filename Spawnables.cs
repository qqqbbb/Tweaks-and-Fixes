using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nautilus.Assets;
using Nautilus.Handlers;
using HarmonyLib;
using static ErrorMessage;
using Nautilus.Assets.Gadgets;
using System;
using static Tweaks_Fixes.Spawnables;

namespace Tweaks_Fixes
{
    public class Spawnables
    {
        //4594f9c0-1b4d-4b10-871d-53950de686fb
        //static string floatingStone = "a1f3da68-d810-44ff-a0a2-6cf3c6a3eff5";
        //Stone stone = new CustomPrefab(
        //    "TF_Stone",
        //"TF_Stone",
        //"");

        public class Stone : ICustomPrefab
        {
            public static TechType TechTypeID { get; protected set; }

            PrefabInfo Info { get; }

            //public override IEnumerator GetGameObjectAsync(IOut<GameObject> gameObject)
            //{
            //    UWE.IPrefabRequest request = UWE.PrefabDatabase.GetPrefabAsync("3e581d94-c873-4ad7-a2f4-a35ec6ac3ecb"); // Rock_Small01
            //    yield return request;
            //    GameObject prefab;
            //    request.TryGetPrefab(out prefab);
            //    gameObject.Set(GameObject.Instantiate(prefab));
            //    yield break;
            //}

            //public override List<SpawnLocation> CoordinatedSpawns => new List<SpawnLocation>()
            //{
            //    new SpawnLocation(new Vector3(0.67f, -14.11f, -323.3f), new Vector3(0f, 310f, 329f)),
            //    new SpawnLocation(new Vector3(348.3f, -25.3f, -205.1f), new Vector3(0f, 266f, 325f)),
            //    new SpawnLocation(new Vector3(-637f, -110.5f, -49.2f), new Vector3(355f, 0f, 90f)),
            //    new SpawnLocation(new Vector3(-185f, -42f, 138.5f), new Vector3(0f, 270f, 138f)),
            //    new SpawnLocation(new Vector3(-63.85f, -16f, -223f), new Vector3(270f, 321f, 0f)), // scale 1.1
            //};

            public TGadget AddGadget<TGadget>(TGadget gadget) where TGadget : Gadget
            {
                throw new NotImplementedException();
            }

            public Gadget GetGadget(Type gadgetType)
            {
                throw new NotImplementedException();
            }

            public TGadget GetGadget<TGadget>() where TGadget : Gadget
            {
                throw new NotImplementedException();
            }

            public bool TryGetGadget<TGadget>(out TGadget gadget) where TGadget : Gadget
            {
                throw new NotImplementedException();
            }

            public bool TryAddGadget<TGadget>(TGadget gadget) where TGadget : Gadget
            {
                throw new NotImplementedException();
            }

            public bool RemoveGadget(Type gadget)
            {
                throw new NotImplementedException();
            }

            public bool RemoveGadget<TGadget>() where TGadget : Gadget
            {
                throw new NotImplementedException();
            }

            public void AddOnRegister(Action onRegisterCallback)
            {
                throw new NotImplementedException();
            }

            public void AddOnUnregister(Action onUnregisterCallback)
            {
                throw new NotImplementedException();
            }

            public PrefabFactoryAsync Prefab => throw new NotImplementedException();

            public static PrefabFactoryAsync GetPrefab()
            {
                throw new NotImplementedException();
            }

            public PrefabPostProcessorAsync OnPrefabPostProcess => throw new NotImplementedException();

            PrefabInfo ICustomPrefab.Info => throw new NotImplementedException();

            public static implicit operator Stone(CustomPrefab v)
            {
                throw new NotImplementedException();
            }
        }

    }
}