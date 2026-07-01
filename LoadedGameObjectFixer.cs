using System;
using System.Collections.Generic;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    internal class LoadedGameObjectFixer
    {
        //public bool loadedObjectsFixed;
        HashSet<string> bloodPrefabs = new HashSet<string> {
            "xKnifeHit_Organic", "GenericCreatureHit", "xExoDrill_Organic" };
        public static Color defaultBloodColor = new Color(0.784f, 1f, 0.157f, 1f);
        public static Color bloodColor = defaultBloodColor;

        public void IterateRootGameObjects()
        {
            foreach (GameObject go in Util.FindAllRootGameObjects())
            {
                //if (go.name == "CellRoot(Clone)" || go.name == "ChunkCollider(Clone)" || go.name == "ChunkGrass(Clone)" || go.name == "ChunkLayer(Clone)" || go.name == "Chunk(Clone)" || go.name.StartsWith("Batch"))
                //{
                //    continue;
                //}
                //UniqueIdentifier pi = go.GetComponentInChildren<UniqueIdentifier>();
                //if (pi != null)
                //    logger.LogDebug($"{go.name} {pi.classId}");
                //else
                //    logger.LogDebug($"{go.name} no UniqueIdentifier");
                //if (go.name.Contains("Fragment"))
                {
                    //AddDebug($"Fragment {go.name} ");
                    //Main.logger.LogDebug($"Fragment {go.name} ");
                    //ResourceTracker rt = go.GetComponentInChildren<ResourceTracker>();
                    //if (rt)
                    {
                        //Main.logger.LogDebug($"ResourceTracker {go.name} {rt.techType} {rt.overrideTechType} ");
                    }
                }

                if (ConfigToEdit.grassCastShadow.Value && go.name == "ChunkGrass(Clone)")
                {
                    Renderer renderer = go.GetComponent<Renderer>();
                    renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                }
                else if (bloodColor != defaultBloodColor && bloodPrefabs.Contains(go.name))
                {
                    SetBloodColor(go);
                }
            }
        }

        public void SetBloodColor(GameObject go)
        {
            ParticleSystem[] pss = go.GetAllComponentsInChildren<ParticleSystem>();
            //Main.logger.LogMessage("SetBloodColor " + go.name + " to " + Creature_Tweaks.bloodColor);
            foreach (ParticleSystem ps in pss)
            {
                ParticleSystem.MainModule psMain = ps.main;
                //Main.logger.LogMessage("startColor " + psMain.startColor.color);
                psMain.startColor = new ParticleSystem.MinMaxGradient(bloodColor);
            }
        }

    }
}
