using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tweaks_Fixes;
using UnityEngine;

namespace Tweaks_and_Fixes
{
    [HarmonyPatch(typeof(Planter))]
    internal class Planter_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch("AddItem", new Type[1] { typeof(InventoryItem) })]
        static void AddItemPrefix(Planter __instance, InventoryItem item)
        {
            if (!Main.config.fixMelons)
                return;

            Plantable p = item.item.GetComponent<Plantable>();
            if (p && p.plantTechType == TechType.MelonPlant)
            {
                //Planter.PlantSlot slotById = __instance.GetSlotByID(slotID);
                //if (slotById == null || slotById.isOccupied)
                //    return;
                //AddDebug("Planter AddItem InventoryItem " + p.plantTechType);
                p.size = Plantable.PlantSize.Large;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch("RemoveItem", new Type[1] { typeof(int) })]
        static bool RemoveItemPrefix(Planter __instance, int slotID)
        {
            Planter.PlantSlot slotById = __instance.GetSlotByID(slotID);
            if (slotById == null)
                return false;

            Plantable plantable = slotById.plantable;
            plantable.currentPlanter = null;
            if (plantable.eatable)
            {
                TechType tt = CraftData.GetTechType(plantable.gameObject);
                if (tt == TechType.JellyPlant)
                {
                    plantable.eatable.SetDecomposes(false);
                    //plantable.eatable.kDecayRate = .01f;
                }
                else
                    plantable.eatable.SetDecomposes(true);
            }
            if (plantable.linkedGrownPlant)
                UnityEngine.Object.Destroy(plantable.linkedGrownPlant.gameObject);

            GameObject plantModel = slotById.plantModel;
            slotById.Clear();
            ResourceTracker component = plantModel.GetComponent<ResourceTracker>();
            if (component)
                component.OnPickedUp(null);

            UnityEngine.Object.Destroy(plantModel);
            __instance.SetSlotOccupiedState(slotID, false);
            return false;
        }




    }
}
