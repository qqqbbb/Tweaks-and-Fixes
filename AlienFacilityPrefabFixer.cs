using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UWE;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    internal class AlienFacilityPrefabFixer
    {
        internal static bool alienFacilityPrefabsFixed;
        List<string> prefabsWithoutShadows = new List<string> {
            "6a01a336-fb46-469a-9f7d-1659e07d11d7", // Precursor_Lab_surgical_machine
            "78009225-a9fa-4d21-9580-8719a3368373", // precursor_deco_props_01
            "1673ee4a-6c28-4651-8d5e-929de26dc25f",// Precursor_Prison_EggLab
            "ef375125-885f-4289-8577-c7a4a5f218b3",// Precursor_Prison_DissectionRoom
            "df9aed66-c131-4570-9dcd-1e3d2109dcaa",// Precursor_Lab_table_LabCache
        };

        readonly Dictionary<string, RendererData> prefabsWithoutShadows_ = new Dictionary<string, RendererData> {
            {"68254d33-2d67-48a8-b485-9929f23a8ba8", new RendererData(null, new List<string>{"Pipes", "Eggs", "___New Group" }) },//Precursor_Prison_EggLab_Extras
            {"a3476419-0a2f-40e7-b325-0a592f0ebea3", new RendererData("Precursor_lab_container_02", new List<string>{ "Precursor_lab_container_02_bottom", "Precursor_lab_container_02_top" }) },//Precursor_lab_container_02_LabCache
            {"2213b907-3231-4c7a-aeaf-03e7c7d349d8", new RendererData(null, new List<string>{ "precursor_block_maze_04_04_04_v4/precursor_block_maze_04_04_04_v4", "precursor_block_deco_06_02_06" }) },//IonCrystalPedestal_Cache
        };

        IEnumerator EnableShadowCasting(string classID)
        {
            //Main.logger.LogDebug("EnableShadowCasting " + classID);
            IPrefabRequest request = PrefabDatabase.GetPrefabAsync(classID);
            yield return request;
            GameObject prefab;
            if (request.TryGetPrefab(out prefab) == false)
            {
                Main.logger.LogError("EnableShadowCasting no prefab for " + classID);
                yield break;
            }
            prefab.transform.EnableShadowCastingInChildren();
        }

        IEnumerator EnableShadowCasting(string classID, RendererData data)
        {
            //Main.logger.LogDebug("EnableShadowCasting RendererData " + classID);
            IPrefabRequest request = PrefabDatabase.GetPrefabAsync(classID);
            yield return request;
            GameObject prefab;
            if (request.TryGetPrefab(out prefab) == false)
            {
                Main.logger.LogError("EnableShadowCasting no prefab for " + classID);
                yield break;
            }
            prefab.transform.EnableShadowCasting(data);
        }


        internal void FixAlienFacilityPrefabs()
        {
            UWE.CoroutineHost.StartCoroutine(FixGunBase());
            UWE.CoroutineHost.StartCoroutine(FixPrison());
            UWE.CoroutineHost.StartCoroutine(FixLostRiverBase());
            UWE.CoroutineHost.StartCoroutine(FixLavaCastleBase());
            foreach (string classID in prefabsWithoutShadows)
            {
                UWE.CoroutineHost.StartCoroutine(EnableShadowCasting(classID));
            }
            foreach (var kv in prefabsWithoutShadows_)
            {
                UWE.CoroutineHost.StartCoroutine(EnableShadowCasting(kv.Key, kv.Value));
            }
            alienFacilityPrefabsFixed = true;
        }

        private IEnumerator FixLavaCastleBase()
        {
            IPrefabRequest request = PrefabDatabase.GetPrefabAsync("b823a2df-f873-411f-8752-991384872e41");
            yield return request;// Precursor_LavaCastleBase
            GameObject prefab;
            request.TryGetPrefab(out prefab);
            Transform meshes = prefab.transform.GetChild(0);
            foreach (Transform child in meshes.transform)
            {
                Util.IncreaseLODdistane(child.gameObject);
            }
            request = PrefabDatabase.GetPrefabAsync("a743777f-1c01-47e9-8436-d107032a0c87");
            yield return request;// Precursor_LavaBase_Entry01
            request.TryGetPrefab(out prefab);
            meshes = prefab.transform.Find("Precursor_LavaBase_Entry01_instances/Precursor_LavaBase_Entry01");
            foreach (Transform child in meshes.transform)
            {
                if (child.name.Contains("ceiling") == false)
                    Util.IncreaseLODdistane(child.gameObject);
            }
            request = PrefabDatabase.GetPrefabAsync("e968ac17-2a1a-400e-864e-991d97c60634");
            yield return request;// Precursor_LavaBase_Entry02
            request.TryGetPrefab(out prefab);
            meshes = prefab.transform.Find("Precursor_LavaBase_Entry02_instances/Precursor_LavaBase_Entry02");
            foreach (Transform child in meshes.transform)
            {
                if (child.name.Contains("ceiling") == false)
                    Util.IncreaseLODdistane(child.gameObject);
            }
            request = PrefabDatabase.GetPrefabAsync("a84f22af-9802-49c2-92ff-5c58335593a1");
            yield return request;// Precursor_LavaBase_Hallway
            request.TryGetPrefab(out prefab);
            meshes = prefab.transform.Find("Precursor_LavaBase_Hallway_instances/Precursor_LavaBase_Hallway");
            foreach (Transform child in meshes.transform)
            {
                if (child.name.Contains("ceiling") == false)
                    Util.IncreaseLODdistane(child.gameObject);
            }
            Transform t = prefab.transform.Find("CullVolumeManager/CullVolume (1)");
            BoxCollider boxCollider = t.GetComponent<BoxCollider>();
            Vector3 oldSize = boxCollider.size;
            //fix: VolumeCullManager disables meshes when player at -75.9 -1216.8 127.9 
            boxCollider.size = new Vector3(oldSize.x, oldSize.y, 20);

            request = PrefabDatabase.GetPrefabAsync("710a7f6c-a409-4966-af68-ff46827a2bcc");
            yield return request;// Precursor_LavaBase_TeleporterRoom
            request.TryGetPrefab(out prefab);
            meshes = prefab.transform.Find("Precursor_LavaBase_TeleporterRoom_instances/Precursor_LavaBase_TeleporterRoom");
            foreach (Transform child in meshes.transform)
            {
                if (child.name.Contains("ceiling") == false)
                    Util.IncreaseLODdistane(child.gameObject);
            }
            request = PrefabDatabase.GetPrefabAsync("a19a9a9c-25db-4e90-aed4-643d62aa0a5b");
            yield return request;// Precursor_LavaBase_HallwayRamps
            request.TryGetPrefab(out prefab);
            meshes = prefab.transform.Find("Precursor_LavaBase_Hallway_instances/Precursor_LavaBase_Hallway_Ramps_Instances");
            foreach (Transform child in meshes.transform)
            {
                if (child.name.Contains("wall"))
                    Util.IncreaseLODdistane(child.gameObject);
            }
            request = PrefabDatabase.GetPrefabAsync("105f340b-65ac-43c7-b2f0-612d2cf3e400");
            yield return request;// Precursor_LavaBase_ThermalRoomEntry
            request.TryGetPrefab(out prefab);
            meshes = prefab.transform.Find("Precursor_LavaBase_Hallway_instances/Precursor_LavaBase_ThermalRoomEntry_Instances");
            foreach (Transform child in meshes.transform)
            {
                if (child.name.Contains("floor"))
                    Util.IncreaseLODdistane(child.gameObject);
            }
            request = PrefabDatabase.GetPrefabAsync("8df14188-4856-4e42-b8ae-bbc27bfb5e4c");
            yield return request;// Precursor_LavaBase_ThermalRoom
            request.TryGetPrefab(out prefab);
            meshes = prefab.transform.Find("Precursor_LavaBase_ThermalRoom_instances/Precursor_LavaBase_ThermalRoom");
            foreach (Transform child in meshes.transform)
            {
                if (child.name.Contains("ceiling") == false)
                    Util.IncreaseLODdistane(child.gameObject);
            }
        }


        private IEnumerator FixLostRiverBase()
        {
            IPrefabRequest request = PrefabDatabase.GetPrefabAsync("80f6c46a-ecfe-4a19-b05f-0466eafde411");
            yield return request;// Precursor_LostRiverBase_Balcony
            GameObject prefab;
            request.TryGetPrefab(out prefab);
            Transform meshes = prefab.transform.Find("Precursor_LostRiverBase_Balcony_instances/Precursor_LostRiverBase_Balcony");
            foreach (Transform child in meshes.transform)
            {
                Util.IncreaseLODdistane(child.gameObject);
            }
            request = PrefabDatabase.GetPrefabAsync("11e3ab1a-3848-4992-a40c-078eb43431fd");
            yield return request;// Precursor_LostRiverBase_Lab_01
            request.TryGetPrefab(out prefab);
            meshes = prefab.transform.Find("Precursor_LostRiverBase_Lab_01_instances/Precursor_LostRiverBase_Lab_01");
            foreach (Transform child in meshes.transform)
            {
                Util.IncreaseLODdistane(child.gameObject);
            }
            request = PrefabDatabase.GetPrefabAsync("1607277c-65f8-4c82-b739-2c6fd937e0ee");
            yield return request;// Precursor_LostRiverBase_ObservationRoom
            request.TryGetPrefab(out prefab);
            meshes = prefab.transform.Find("Precursor_LostRiverBase_ObservationRoom_instances/Precursor_LostRiverBase_ObservationRoom");
            foreach (Transform child in meshes.transform)
            {
                if (child.name.Contains("floor"))
                    Util.IncreaseLODdistane(child.gameObject);
            }
            request = PrefabDatabase.GetPrefabAsync("7879c3ab-3165-4f2f-bc52-dcf6a0f393c5");
            yield return request;// Precursor_LostRiverBase_Aquarium
            request.TryGetPrefab(out prefab);
            meshes = prefab.transform.Find("Precursor_LostRiverBase_Aquarium_instances/Precursor_LostRiverBase_Aquarium");
            foreach (Transform child in meshes.transform)
            {
                if (child.name.Contains("wall"))
                    Util.IncreaseLODdistane(child.gameObject);
            }
            request = PrefabDatabase.GetPrefabAsync("258d971e-48b1-4cef-955c-a0222586d0c5");
            yield return request;// Precursor_LostRiverBase_WarperLab
            request.TryGetPrefab(out prefab);
            meshes = prefab.transform.Find("Precursor_LostRiverBase_Warperlab");
            foreach (Transform child in meshes.transform)
            {
                //if (child.name.Contains("wall"))
                //if (child.name.Contains("ceiling") || child.name.Contains("floor"))
                Util.IncreaseLODdistane(child.gameObject);
            }
        }

        private IEnumerator FixPrison()
        {
            UWE.CoroutineHost.StartCoroutine(FixPrisonOutpost("c5512e00-9959-4f57-98ae-9a9962976eaa"));// Precursor_Prison_Outpost1
            UWE.CoroutineHost.StartCoroutine(FixPrisonOutpost("542aaa41-26df-4dba-b2bc-3fa3aa84b777"));// Precursor_Prison_Outpost2
            UWE.CoroutineHost.StartCoroutine(FixPrisonOutpost("5bcaefae-2236-4082-9a44-716b0598d6ed"));// Precursor_Prison_Outpost3
            UWE.CoroutineHost.StartCoroutine(FixPrisonOutpost("20ad299d-ca52-48ef-ac29-c5ec5479e070"));// Precursor_Prison_Outpost4
            UWE.CoroutineHost.StartCoroutine(FixPrisonOutpost("430b36ae-94f3-4289-91ac-25475ad3bf74"));// Precursor_Prison_Outpost5

            UWE.CoroutineHost.StartCoroutine(FixPrecursorLabContainerShadow());
            UWE.CoroutineHost.StartCoroutine(FixDissectionRoomEmperorTankShadow());
            UWE.CoroutineHost.StartCoroutine(FixPrisonTankGlass());
            UWE.CoroutineHost.StartCoroutine(FixPrisonTeleporterRoom03Shadows());

            IPrefabRequest request = PrefabDatabase.GetPrefabAsync("e8143977-448e-4202-b780-83485fa5f31a");
            yield return request;// Precursor_Prison_Interior_Antechamber
            GameObject prefab;
            request.TryGetPrefab(out prefab);
            Transform meshes = prefab.transform.Find("Precursor_Prison_Interior_Antechamber/mesh");
            foreach (Transform child in meshes.transform)
            {
                if (child.name.Contains("ceiling") == false)
                    Util.IncreaseLODdistane(child.gameObject);
            }
            request = PrefabDatabase.GetPrefabAsync("d964f99c-abc1-463c-9e0b-dc5a719eb94a");
            yield return request;// Precursor_Prison_TeleporterHallway03
            request.TryGetPrefab(out prefab);
            meshes = prefab.transform.Find("Precursor_Prison_Interior_Antechamber_Teleporter_Hallway_02_instances/mesh");
            foreach (Transform child in meshes.transform)
            {
                if (child.name.Contains("ceiling") == false)
                    Util.IncreaseLODdistane(child.gameObject);
            }
            request = PrefabDatabase.GetPrefabAsync("3cd6459f-c245-44bc-8c44-8a4c5a94330c");
            yield return request;// Precursor_Prison_PipeRoom
            request.TryGetPrefab(out prefab);
            meshes = prefab.transform.Find("Precursor_Prison_Antechamber_Lab_02_instances/Precursor_Prison_Antechamber_PipeLab");
            foreach (Transform child in meshes.transform)
            {
                if (child.name.Contains("ceiling") == false)
                    Util.IncreaseLODdistane(child.gameObject);
            }
            request = PrefabDatabase.GetPrefabAsync("4ac691ba-767d-4f5d-a1a5-9a5b4f0aaa0b");
            yield return request;// Precursor_Prison_TeleporterHallway01
            request.TryGetPrefab(out prefab);
            meshes = prefab.transform.Find("Precursor_Prison_Interior_Antechamber_Teleporter_Hallway_02_instances/mesh");
            foreach (Transform child in meshes.transform)
            {
                if (child.name.Contains("floor"))
                    Util.IncreaseLODdistane(child.gameObject);
            }
            request = PrefabDatabase.GetPrefabAsync("f3350851-47f2-46fe-92d5-2ab8a1a98fcb");
            yield return request;// Precursor_Prison_TeleporterHallway02
            request.TryGetPrefab(out prefab);
            meshes = prefab.transform.Find("Precursor_Prison_Interior_Antechamber_Teleporter_Hallway_01_instances/mesh");
            foreach (Transform child in meshes.transform)
            {
                if (child.name.Contains("floor"))
                    Util.IncreaseLODdistane(child.gameObject);
            }
            request = PrefabDatabase.GetPrefabAsync("1673ee4a-6c28-4651-8d5e-929de26dc25f");
            yield return request;// Precursor_Prison_EggLab
            request.TryGetPrefab(out prefab);
            meshes = prefab.transform.Find("Precursor_Prison_EggLab");
            foreach (Transform child in meshes.transform)
            {
                if (child.name.Contains("ceiling") == false)
                    Util.IncreaseLODdistane(child.gameObject);
            }
            request = PrefabDatabase.GetPrefabAsync("597ca303-3b24-45dc-b3d1-1e450ba5cf32");
            yield return request;// Precursor_Prison_Interior_Moon_Pool
            request.TryGetPrefab(out prefab);
            meshes = prefab.transform.Find("Precursor_Prison_Interior_Moon_Pool/mesh");
            foreach (Transform child in meshes.transform)
            {
                if (child.name.Contains("floor"))
                    Util.IncreaseLODdistane(child.gameObject);
            }
            //request = PrefabDatabase.GetPrefabAsync("c27611ce-de24-46fb-9d4b-9cb16b7c94a0");
            //yield return request;// Precursor_Prison_MoonPoolSurface
            //request.TryGetPrefab(out prefab);

            //Main.logger.LogDebug(" FixPrisonFacility !!!");
        }

        private IEnumerator FixPrisonOutpost(string classID)
        {
            IPrefabRequest request = PrefabDatabase.GetPrefabAsync(classID);
            yield return request;
            GameObject prefab;
            request.TryGetPrefab(out prefab);
            Util.IncreaseLODdistane(prefab);
        }

        IEnumerator FixPrecursorLabContainerShadow()
        { //  183 -1440 -423
            IPrefabRequest request = PrefabDatabase.GetPrefabAsync("d0fea4da-39f2-47b4-aece-bb12fe7f9410"); // Precursor_lab_container_01
            yield return request;
            GameObject prefab;
            if (request.TryGetPrefab(out prefab) == false)
            {
                Main.logger.LogError("no prefab for Precursor_lab_container_01");
                yield break;
            }
            Renderer[] renderers = prefab.transform.GetComponentsInChildren<Renderer>();
            renderers[0].receiveShadows = true;
            renderers[0].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            renderers[2].receiveShadows = true;
            renderers[2].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        }

        IEnumerator FixPrisonTankGlass()
        {
            IPrefabRequest request = PrefabDatabase.GetPrefabAsync("ac6dd6fe-5835-41b9-96e8-2ec4120699ff");
            yield return request; // Precursor_Prison_TankGlassSmall  188 -1440 -420
            GameObject prefab;
            request.TryGetPrefab(out prefab);
            prefab.AddComponent<PrisonTankGlassFixer>();
        }

        IEnumerator FixPrisonTeleporterRoom03Shadows()
        { //  334 -1430 -278
            IPrefabRequest request = PrefabDatabase.GetPrefabAsync("8c3cc489-cb05-42a0-830d-b9dc73a841c0"); // Precursor_Prison_TeleporterRoom_03
            yield return request;
            GameObject prefab;
            if (request.TryGetPrefab(out prefab) == false)
            {
                Main.logger.LogError("no prefab for Precursor_Prison_TeleporterRoom_03");
                yield break;
            }
            Transform meshes = prefab.transform.GetChild(1);
            foreach (int i in new int[] { 19, 20, 40, 41 })
            {
                Transform t = meshes.GetChild(i);
                Renderer renderer = t.GetComponentInChildren<Renderer>();
                //Main.logger.LogDebug("FixPrisonTeleporterRoom03Shadows Renderer " + renderer.name);
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            }
        }

        IEnumerator FixDissectionRoomEmperorTankShadow()
        { //  225 -1428 -282
            IPrefabRequest request = PrefabDatabase.GetPrefabAsync("44974fcd-c47a-41aa-a279-43eaf234bfa6"); // Precursor_Prison_DissectionRoom_EmperorTank
            yield return request;
            GameObject prefab;
            request.TryGetPrefab(out prefab);
            for (int i = 2; i < 5; i++)
            {
                Transform child = prefab.transform.GetChild(i);
                child.EnableShadowCastingInChildren();
            }
        }

        private IEnumerator FixGunBase()
        {
            IPrefabRequest request = PrefabDatabase.GetPrefabAsync("5ca9d34d-3b1e-46b7-9660-ab3156db7d28");
            yield return request;// Precursor_Gun_Entry
            GameObject prefab;
            request.TryGetPrefab(out prefab);
            Transform meshes = prefab.transform.GetChild(1);
            for (int i = 0; i < meshes.childCount; i++)
            {
                if (i == 8 || i == 9 || i == 10)
                    continue;

                Transform child = meshes.GetChild(i);
                Util.IncreaseLODdistane(child.gameObject);
            }
            UWE.CoroutineHost.StartCoroutine(FuxGunRoom("4a5670a3-1459-45b9-81b4-44ecc7af5996"));// Precursor_Gun_TerminalHallway_01
            UWE.CoroutineHost.StartCoroutine(FuxGunRoom("1160ef75-8bf6-4bf4-a7d7-6718956e22f1"));// Precursor_Gun_Terminal_Room_01
            UWE.CoroutineHost.StartCoroutine(FuxGunRoom("10ff8700-c95a-44d9-9be6-d16f29332c83"));// Precursor_Gun_TerminalHallway_02
            UWE.CoroutineHost.StartCoroutine(FuxGunRoom("14cd4cc9-93ef-4104-ae95-f8cee52a5698"));// Gun_LargeHallway

            request = PrefabDatabase.GetPrefabAsync("74ee7292-f385-4063-91e6-d9448e95de6e");
            yield return request;// Precursor_Gun_TerminalRoom_02
            request.TryGetPrefab(out prefab);
            meshes = prefab.transform.GetChild(1);
            for (int i = 0; i < 27; i++)
            {
                Transform child = meshes.GetChild(i);
                Util.IncreaseLODdistane(child.gameObject);
            }
            Transform child_ = meshes.GetChild(34);
            Util.IncreaseLODdistane(child_.gameObject);
            child_ = meshes.GetChild(35);
            Util.IncreaseLODdistane(child_.gameObject);

            request = PrefabDatabase.GetPrefabAsync("51e58608-a80b-4135-9143-add4ce77a42f");
            yield return request;// Precursor_Gun_Elevator
            request.TryGetPrefab(out prefab);
            meshes = prefab.transform.GetChild(1);
            //Main.logger.LogDebug("Precursor_Gun_Elevator ");
            for (int i = 18; i < 22; i++)
            {
                Transform child = meshes.GetChild(i);
                Util.IncreaseLODdistane(child.gameObject);
            }
            request = PrefabDatabase.GetPrefabAsync("2f90460f-b348-4f49-98b7-6e0611d9239c");
            yield return request;// Precursor_Gun_MoonPool
            request.TryGetPrefab(out prefab);
            meshes = prefab.transform.GetChild(2);
            for (int i = 4; i < meshes.childCount; i++)
            {
                Transform child = meshes.GetChild(i);
                Util.IncreaseLODdistane(child.gameObject);
            }
            request = PrefabDatabase.GetPrefabAsync("963fa3a3-9192-4912-8c8d-d0d98f22ed13");
            yield return request;// Precursor_Gun_ControlRoom
            request.TryGetPrefab(out prefab);
            meshes = prefab.transform.GetChild(1);
            for (int i = 0; i < meshes.childCount; i++)
            {
                Transform child = meshes.GetChild(i);
                if (child.name.Contains("column"))
                    Util.IncreaseLODdistane(child.gameObject);
                else if (child.name.Contains("ramp"))
                    Util.ForceLOD(child.gameObject);
            }
        }

        private IEnumerator FuxGunRoom(string classID)
        {
            IPrefabRequest request = PrefabDatabase.GetPrefabAsync(classID);
            yield return request;
            GameObject prefab;
            request.TryGetPrefab(out prefab);
            Transform meshes = prefab.transform.GetChild(1);
            foreach (Transform child in meshes)
                Util.IncreaseLODdistane(child.gameObject);
        }

    }

    public class PrisonTankGlassFixer : MonoBehaviour
    {
        public void Start()
        {
            //Main.logger.LogDebug("Prison Tank glass Start " + transform.parent.name);
            if (transform.parent.name == "CellRoot(Clone)")
            {
                //AddDebug("Destroy Prison Tank glass");
                Destroy(transform.gameObject);
            }
            else
                Destroy(this);
        }
    }

}
