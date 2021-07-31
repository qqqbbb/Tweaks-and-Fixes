using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    class Torpedo_Patch
    {
        [HarmonyPatch(typeof(SeamothStorageContainer), "OnCraftEnd")]
        class SeamothStorageContainer_OnCraftEnd_Patch
        {
            public static bool Prefix(SeamothStorageContainer __instance, TechType techType)
            {
                __instance.Init();
                if (techType == TechType.SeamothTorpedoModule || techType == TechType.ExosuitTorpedoArmModule)
                {
                    for (int index = 0; index < Main.config.freeTorpedos; ++index)
                    {
                        GameObject gameObject = CraftData.InstantiateFromPrefab(TechType.WhirlpoolTorpedo);
                        if (gameObject != null)
                        {
                            Pickupable component = gameObject.GetComponent<Pickupable>();
                            if (component != null)
                            {
                                Pickupable pickupable = component.Pickup(false);
                                if (__instance.container.AddItem(pickupable) == null)
                                    UnityEngine.Object.Destroy(pickupable.gameObject);
                            }
                        }
                    }
                }
                return false;
            }
        }



    }
}
