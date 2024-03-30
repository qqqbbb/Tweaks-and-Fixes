using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx.Configuration;
using Nautilus.Json;
using UnityEngine;

namespace Tweaks_Fixes
{
    public static class ConfigToEdit
    {
        public static ConfigEntry<bool> heatBladeCooks;
        public static ConfigEntry<bool> dontSpawnKnownFragments;
        public static ConfigEntry<bool> cantScanExosuitClawArm;
        public static ConfigEntry<bool> mapRoomFreeCameras;
        public static ConfigEntry<bool> decoyRequiresSub;
        public static ConfigEntry<bool> noKillParticles;
        public static ConfigEntry<bool> cyclopsSunlight;
        public static ConfigEntry<bool> alwaysShowHealthFoodNunbers;
        public static ConfigEntry<bool> pdaClock;
        public static ConfigEntry<Button> transferAllItemsButton;
        public static ConfigEntry<Button> transferSameItemsButton;
        public static ConfigEntry<Button> quickslotButton;
        public static ConfigEntry<Button> lightButton;
        public static ConfigEntry<string> gameStartWarningText;
        public static ConfigEntry<string> newGameLoot;
        public static ConfigEntry<string> crushDepthEquipment;
        public static ConfigEntry<string> crushDamageEquipment;
        public static ConfigEntry<string> itemMass;
        public static ConfigEntry<string> unmovableItems;
        public static ConfigEntry<Vector3> bloodColor;
        public static ConfigEntry<string> gravTrappable;
        public static ConfigEntry<float> medKitHPperSecond;
        public static ConfigEntry<string> silentCreatures;
        public static ConfigEntry<string> stalkerPlayThings;
        public static ConfigEntry<string> eatableFoodValue;
        public static ConfigEntry<string> eatableWaterValue;
        public static ConfigEntry<bool> fixMelons;
        public static ConfigEntry<bool> randomPlantRotation;
        public static ConfigEntry<bool> silentReactor;
        public static ConfigEntry<bool> removeFragmentCrate;
        public static ConfigEntry<bool> creepvineLights;
        public static ConfigEntry<bool> newUIstrings;
        public static ConfigEntry<bool> tweaksAffectingGPU;
        public static ConfigEntry<bool> disableUseText;
        public static ConfigEntry<bool> craftWithoutBattery;
        public static ConfigEntry<bool> disableCyclopsProximitySensor;
        public static ConfigEntry<bool> builderPlacingWhenFinishedBuilding;
        public static ConfigEntry<bool> crushDamageScreenEffect;
        public static ConfigEntry<bool> removeCookedFishOnReload;
        public static ConfigEntry<bool> fishRespawn;
        public static ConfigEntry<bool> fishRespawnIfKilledByPlayer;
        public static ConfigEntry<bool> creaturesRespawn;
        public static ConfigEntry<bool> creaturesRespawnIfKilledByPlayer;
        public static ConfigEntry<bool> leviathansRespawn;
        public static ConfigEntry<bool> leviathansRespawnIfKilledByPlayer;
        public static ConfigEntry<bool> disableGravityForExosuit;
        public static ConfigEntry<bool> replaceDealDamageOnImpactScript;
        public static ConfigEntry<float> cyclopsDealDamageMinSpeed;
        public static ConfigEntry<float> cyclopsTakeDamageMinSpeed;
        public static ConfigEntry<float> cyclopsTakeDamageMinMass;
        public static ConfigEntry<float> exosuitDealDamageMinSpeed;
        public static ConfigEntry<float> exosuitTakeDamageMinSpeed;
        public static ConfigEntry<float> exosuitTakeDamageMinMass;
        public static ConfigEntry<float> seamothDealDamageMinSpeed;
        public static ConfigEntry<float> seamothTakeDamageMinSpeed;
        public static ConfigEntry<float> seamothTakeDamageMinMass;
        public static ConfigEntry<float> lavaGeyserEruptionForce;
        public static ConfigEntry<float> lavaGeyserEruptionInterval;
        public static ConfigEntry<bool> removeLavaGeyserRockParticles;
        public static ConfigEntry<float> solarPanelMaxDepth;
        public static ConfigEntry<bool> stalkerLooseToothSound;
        public static ConfigEntry<bool> canReplantMelon;
        public static ConfigEntry<bool> removeCookedFishFromBlueprints;
        public static ConfigEntry<bool> removeWaterFromBlueprints;
        public static ConfigEntry<bool> swapControllerTriggers;
        public static ConfigEntry<bool> disableFootstepClickSound;
        public static ConfigEntry<bool> newStorageUI;

        

        public static AcceptableValueRange<float> medKitHPperSecondRange = new AcceptableValueRange<float>(0.001f, 100f);

        public static void Bind()
        {
            heatBladeCooks = Main.configB.Bind("", "Thermoblade cooks fish on kill", true);
            dontSpawnKnownFragments = Main.configB.Bind("", "Do not spawn fragments for unlocked blueprints", false);
            cantScanExosuitClawArm = Main.configB.Bind("", "Unlock prawn suit only by scanning prawn suit", true, "In vanilla game prawn suit can be unlocked by scanning 20 prawn suit arms fragments. Game has to be reloaded after changing this.");
            mapRoomFreeCameras = Main.configB.Bind("", "Free camera drones for scanner room", false, "Scanner room will be built without camera drones if this is false.");
            decoyRequiresSub = Main.configB.Bind("", "Creature decoy does not work when dropped from inventory", false);
            noKillParticles = Main.configB.Bind("", "No particles when creature dies", false, "No yellow cloud particles will spawn when a creature dies. Game has to be reloaded after changing this. ");
            cyclopsSunlight = Main.configB.Bind("", "Sunlight affects lighting in cyclops", false);
            alwaysShowHealthFoodNunbers = Main.configB.Bind("", "Always show health and food values in UI", false);
            pdaClock = Main.configB.Bind("", "PDA clock", true);
            transferAllItemsButton = Main.configB.Bind("", "Move all items button", Button.LookUp, "Press this button to move all items from one container to another. This works only with controller.");
            transferSameItemsButton = Main.configB.Bind("", "Move same items button", Button.LookDown, "Press this button to move all items of the same type from one container to another. This works only with controller.");
            quickslotButton = Main.configB.Bind("", "Quickslot cycle button", Button.Jump, "Press 'Cycle next' or 'Cycle previous' button while holding down this button to cycle tools in your current quickslot. This works only with controller.");
            lightButton = Main.configB.Bind("", "Light intensity button", Button.LeftHand, "When holding a tool in your hand or driving a vehicle press 'Cycle next' or 'Cycle previous' button while holding down this button to change the tool's or vehicle's light intensity. This works only with controller.");
            gameStartWarningText = Main.configB.Bind("", "Game start warning text", "", "Text shown when the game starts. If this field is empty the warning will be skipped.");
            newGameLoot = Main.configB.Bind("", "Life pod items", "FilteredWater 2, NutrientBlock 2, Flare 2", "Items you find in your life pod when you start a new game. The format is item ID, space, number of items. Every entry is separated by comma.");
            crushDepthEquipment = Main.configB.Bind("", "Crush depth equipment", "ReinforcedDiveSuit 0", "Allows you to make your equipment increase your crush depth. The format is: item ID, space, number of meters that will be added to your crush depth. Every entry is separated by comma.");
            crushDamageEquipment = Main.configB.Bind("", "Crush damage equipment", "ReinforcedDiveSuit 0", "Allows you to make your equipment reduce your crush damage. The format is: item ID, space, crush damage percent that will be blocked. Every entry is separated by comma.");
            itemMass = Main.configB.Bind("", "Item mass", "PrecursorKey_Blue 5.0, PrecursorKey_Orange 5.0, PrecursorKey_Purple 5.0, PrecursorKey_Red 5.0, PrecursorKey_White 5.0", "Allows you to change mass of pickupable items. The format is: item ID, space, item mass in kg. Mass is a decimal point number. Every entry is separated by comma.");
            unmovableItems = Main.configB.Bind("", "Unmovable items", "", "Contains pickupable items that can not be moved by bumping into them. You will always find them where you dropped them.  Every entry is separated by comma.");
            bloodColor = Main.configB.Bind("", "Blood color", new Vector3(0.784f, 1f, 0.157f), "Lets you change the color of creatures' blood. Each value is a decimal point number from 0 to 1. First number is red. Second number is green. Third number is blue.");
            gravTrappable = Main.configB.Bind("", "Gravtrappable items", "seaglide, airbladder, flare, flashlight, builder, lasercutter, ledlight, divereel, propulsioncannon, welder, repulsioncannon, scanner, stasisrifle, knife, heatblade, precursorkey_blue, precursorkey_orange, precursorkey_purple, compass, fins, fireextinguisher, firstaidkit, doubletank, plasteeltank, radiationsuit, radiationhelmet, radiationgloves, rebreather, reinforceddivesuit, maproomhudchip, tank, stillsuit, swimchargefins, ultraglidefins, highcapacitytank,", "List of items affected by grav trap.");
            medKitHPperSecond = Main.configB.Bind("", "Amount of HP restored by first aid kit every second", 100f, new ConfigDescription("Set this to a low number to slowly restore HP after using first aid kit.", medKitHPperSecondRange));
            silentCreatures = Main.configB.Bind("", "Silent creatures", "", "List of creature IDs separated by comma. Creatures in this list will be silent.");
            stalkerPlayThings = Main.configB.Bind("", "Items stalkers can grab", "ScrapMetal, MapRoomCamera, Beacon, Seaglide, CyclopsDecoy, Gravsphere, SmallStorage, FireExtinguisher, DoubleTank, PlasteelTank, PrecursorKey_Blue, PrecursorKey_Orange, PrecursorKey_Purple, PrecursorKey_Red, PrecursorKey_White, Rebreather, Tank, HighCapacityTank, Flare, Flashlight, Builder, LaserCutter, LEDLight, DiveReel, PropulsionCannon, Knife, HeatBlade, Scanner, Welder, RepulsionCannon, StasisRifle", "List of item IDs separated by comma. Only items in this list can be grabbed by stalkers.");
            eatableFoodValue = Main.configB.Bind("", "Eatable component food value", "CreepvineSeedCluster 5", "Items from this list will be made eatable. The format is: item ID, space, food value. Every entry is separated by comma.");
            eatableWaterValue = Main.configB.Bind("", "Eatable component water value", "CreepvineSeedCluster 10", "Items from this list will be made eatable. The format is: item ID, space, water value. Every entry is separated by comma.");
            fixMelons = Main.configB.Bind("", "Fix melons", false, "If true you will be able to plant only 1 melon in a pot and only 4 in a planter.");
            randomPlantRotation = Main.configB.Bind("", "Random plant rotation", true, "If true plants in planters to have random rotation.");
            silentReactor = Main.configB.Bind("", "Silent nuclear reactor", false, "If true nuclear reactor will be silent.");
            removeFragmentCrate = Main.configB.Bind("", "Remove fragment crate", false, "If true a fragment's crate will be removed when you scan a blueprint fragment.");
            creepvineLights = Main.configB.Bind("", "Real creepvine lights", true, "If true creepvine seed cluster light intensity will depend on number of seed clusters on the vine.");
            newUIstrings = Main.configB.Bind("", "New UI text", true, "If false new UI elements added by the mod wil be disabled.");
            newStorageUI = Main.configB.Bind("", "New storage UI", true);
            tweaksAffectingGPU = Main.configB.Bind("", "Tweaks affecting GPU", true, "If false changes that may affect GPU performance (LOD edits, edits that increase popping in distance for objects) will be disabled.");
            disableUseText = Main.configB.Bind("", "Disable quickslots text", false, "If true text above your quickslots will be disabled.");
            craftWithoutBattery = Main.configB.Bind("", "Craft without battery", false, "If true your newly crafted tools and vehicles will not have batteries in them.");
            disableCyclopsProximitySensor = Main.configB.Bind("", "Disable cyclops proximity sensor", false);
            builderPlacingWhenFinishedBuilding = Main.configB.Bind("", "Builder tool placing mode when finished building", true, "If false your builder tool will exit placing mode when you finish building.");
            crushDamageScreenEffect = Main.configB.Bind("", "Crush damage screen effect", true, "If false there will be no screen effects when player takes crush damage.");
            removeCookedFishOnReload = Main.configB.Bind("", "Remove cooked fish on reload", false, "If true cooked fish will be removed from the world (not from containers) when the game loads.");
            fishRespawn = Main.configB.Bind("", "Fish respawn", true, "If false dead fish will never respawn.");
            fishRespawnIfKilledByPlayer = Main.configB.Bind("", "Fish respawn if killed by player", true, "If false fish killed or caught by player will never respawn.");
            creaturesRespawn = Main.configB.Bind("", "Creatures respawn", true, "If false dead creatures will never respawn.");
            creaturesRespawnIfKilledByPlayer = Main.configB.Bind("", "Creatures respawn if killed by player", true, "If false creatures killed by player will never respawn.");
            leviathansRespawn = Main.configB.Bind("", "Leviathans respawn", true, "If false dead leviathans will never respawn.");
            leviathansRespawnIfKilledByPlayer = Main.configB.Bind("", "Leviathans respawn if killed by player", true, "If false leviathans killed by player will never respawn.");
            disableGravityForExosuit = Main.configB.Bind("", "Disable gravity for prawn suit", false, "If true prawn suit will ignore gravity when you are not piloting it. Use this if your prawn suit falls through the ground.");
            replaceDealDamageOnImpactScript = Main.configB.Bind("", "Replace DealDamageOnImpact script", true, "If false the game will use vanilla script when vehicles collide with objects.");
            cyclopsDealDamageMinSpeed = Main.configB.Bind("", "Cyclops min speed to deal damage", 2f, "Min speed in meters per second at which cyclops deals damage when colliding with objects. Works only if 'replaceDealDamageOnImpactScript' setting is true.");
            cyclopsTakeDamageMinSpeed = Main.configB.Bind("", "Cyclops min speed to take damage", 2f, "Min speed in meters per second at which cyclops takes damage when colliding with objects. Works only if 'replaceDealDamageOnImpactScript' setting is true.");
            cyclopsTakeDamageMinMass = Main.configB.Bind("", "Min mass that can damage cyclops", 200f, "Min mass in kg for objects that can damage cyclops when colliding with it. Works only if 'replaceDealDamageOnImpactScript' setting is true.");
            exosuitDealDamageMinSpeed = Main.configB.Bind("", "Prawn suit min speed to deal damage", 7f, "Min speed in meters per second at which prawn suit deals damage when colliding with objects. Works only if 'replaceDealDamageOnImpactScript' setting is true.");
            exosuitTakeDamageMinSpeed = Main.configB.Bind("", "Prawn suit min speed to take damage", 7f, "Min speed in meters per second at which prawn suit takes damage when colliding with objects. Works only if 'replaceDealDamageOnImpactScript' setting is true.");
            exosuitTakeDamageMinMass = Main.configB.Bind("", "Min mass that can damage prawn suit", 5f, "Min mass in kg for objects that can damage prawn suit when colliding with it. Works only if 'replaceDealDamageOnImpactScript' setting is true.");
            seamothDealDamageMinSpeed = Main.configB.Bind("", "Seamoth min speed to deal damage", 7f, "Min speed in meters per second at which seamoth deals damage when colliding with objects. Works only if 'replaceDealDamageOnImpactScript' setting is true.");
            seamothTakeDamageMinSpeed = Main.configB.Bind("", "Seamoth min speed to take damage", 7f, "Min speed in meters per second at which seamoth takes damage when colliding with objects. Works only if 'replaceDealDamageOnImpactScript' setting is true.");
            seamothTakeDamageMinMass = Main.configB.Bind("", "Min mass that can damage seamoth", 5f, "Min mass in kg for objects that can damage seamoth when colliding with it. Works only if 'replaceDealDamageOnImpactScript' setting is true.");
            lavaGeyserEruptionForce = Main.configB.Bind("", "Lava geyser eruption force", 20f, "Force  applied to objects by lava geysers.");
            lavaGeyserEruptionInterval = Main.configB.Bind("", "Lava geyser eruption interval", 12f, "Time in seconds between lava geyser eruptions including 5.5 seconds of eruption.");
            removeLavaGeyserRockParticles = Main.configB.Bind("", "Remove rock particles from lava geysers", false, "If true rock particles will be removed from lava geyser eruptions.");
            solarPanelMaxDepth = Main.configB.Bind("", "Solar panel max depth", 250f, "Depth in meters below which solar panel does not produce power.");
            stalkerLooseToothSound = Main.configB.Bind("", "Stalker loosing tooth sound", true, "If false there will be no sound effect when a stalker looses its tooth.");
            canReplantMelon = Main.configB.Bind("", "Can replant melon", true, "If false gel sack and melon can't be replanted.");
            removeCookedFishFromBlueprints = Main.configB.Bind("", "Remove cooked fish recipes from blueprints PDA tab.", true);
            removeWaterFromBlueprints = Main.configB.Bind("", "Remove water recipes from blueprints PDA tab.", false);
            swapControllerTriggers = Main.configB.Bind("", "Swap controller triggers", false, "If true controller's left and right trigger will be swapped.");
            disableFootstepClickSound = Main.configB.Bind("", "Disable clicking sound when walking on metal surface", true);

        }

        private static Dictionary<T, T1> ParseDicFromString<T, T1>(string input)
        {
            IDictionary dic = new Dictionary<T, T1>();
            string[] entries = input.Split(',');
            for (int i = 0; i < entries.Length; i++)
            {
                string s = entries[i].Trim();
                string techType;
                string num;
                int index = s.IndexOf(' ');
                if (index == -1)
                    continue;

                techType = s.Substring(0, index);
                num = s.Substring(index);
                if (!TechTypeExtensions.FromString(techType, out TechType tt, true))
                    continue;
                // no simple way to check if techType is pickupable
                int num_ = 0;
                float numFl = 0;
                try
                {
                    if (num.Contains('.'))
                        numFl = float.Parse(num);
                    else
                        num_ = int.Parse(num);
                }
                catch (Exception)
                {
                    continue;
                }
                if (num_ > 0)
                {
                    dic.Add(tt, num_);
                }
                else if (numFl > 0)
                {
                    dic.Add(tt, numFl);
                }
                //Main.logger.LogDebug("ParseDicFromString " + tt + " " + a);
            }
            return (Dictionary <T, T1>)dic;
        }

        private static List<T> ParselistFromString<T>(string input)
        {
            IList list = new List<T>();
            string[] entries = input.Split(',');
            for (int i = 0; i < entries.Length; i++)
            {
                string techType = entries[i].Trim();

                if (!TechTypeExtensions.FromString(techType, out TechType tt, true))
                    continue;
                // no simple way to check if techType is pickupable
                list.Add(tt);
                //Main.logger.LogDebug("ParselistFromString " + tt );
            }
            return (List<T>)list;
        }

        private static HashSet<T> ParseSetFromString<T>(string input)
        {
            ISet<T> set = new HashSet<T>();
            string[] entries = input.Split(',');
            for (int i = 0; i < entries.Length; i++)
            {
                string techType = entries[i].Trim();

                if (!TechTypeExtensions.FromString(techType, out TechType tt, true))
                    continue;
                // no simple way to check if techType is pickupable
                //set.Add(tt);
                set.Add((T)Convert.ChangeType(tt, typeof(T)));
                //Main.logger.LogDebug("ParselistFromString " + tt );
            }
            return (HashSet<T>)set;
        }
    
        public static void ParseFromConfig()
        {
            Crush_Damage.crushDepthEquipment = ParseDicFromString<TechType, int>(crushDepthEquipment.Value);
            Crush_Damage.crushDamageEquipment = ParseDicFromString<TechType, int>(crushDamageEquipment.Value);
            Pickupable_Patch.itemMass = ParseDicFromString<TechType, float>(itemMass.Value);
            Pickupable_Patch.unmovableItems = ParseSetFromString<TechType>(unmovableItems.Value);
            Gravsphere_Patch.gravTrappable = ParseSetFromString<TechType>(gravTrappable.Value);
            Creature_Tweaks.silentCreatures = ParseSetFromString<TechType>(silentCreatures.Value);
            Pickupable_Patch.shinies = ParseSetFromString<TechType>(stalkerPlayThings.Value);
            LargeWorldEntity_Patch.eatableFoodValue = ParseDicFromString<TechType, int>(eatableFoodValue.Value);
            LargeWorldEntity_Patch.eatableWaterValue = ParseDicFromString<TechType, int>(eatableWaterValue.Value);
            Escape_Pod_Patch.newGameLoot = ParseDicFromString<TechType, int>(newGameLoot.Value);

            // Button enum checked for acceptable value during ConfigEntry binding
            Enum.TryParse(transferAllItemsButton.Value.ToString(), out Inventory_Patch.transferAllItemsButton);
            Enum.TryParse(transferSameItemsButton.Value.ToString(), out Inventory_Patch.transferSameItemsButton);
            Enum.TryParse(quickslotButton.Value.ToString(), out QuickSlots_Patch.quickslotButton);
            Enum.TryParse(lightButton.Value.ToString(), out QuickSlots_Patch.lightButton);
            //Main.logger.LogMessage("quickslotButton " + QuickSlots_Patch.quickslotButton);
            //Main.logger.LogMessage("lightButton " + QuickSlots_Patch.lightButton);

        }

        public enum Button
        {
            None,
            Jump,
            PDA,
            Deconstruct,
            Exit,
            LeftHand,
            RightHand,
            CycleNext,
            CyclePrev,
            AltTool,
            TakePicture,
            Reload,
            Sprint,
            LookUp,
            LookDown,
            LookLeft,
            LookRight
            
        }

    }
}
