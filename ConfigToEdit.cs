using BepInEx.Configuration;
using Nautilus.Json;
using Nautilus.Options;
using Nautilus.Options.Attributes;
using Oculus.Platform;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        //public static ConfigEntry<bool> tweaksAffectingGPU;
        public static ConfigEntry<bool> disableUseText;
        public static ConfigEntry<bool> craftWithoutBattery;
        public static ConfigEntry<bool> disableCyclopsProximitySensor;
        public static ConfigEntry<bool> builderPlacingWhenFinishedBuilding;
        public static ConfigEntry<bool> crushDamageScreenEffect;
        public static ConfigEntry<bool> removeCookedFishOnReload;
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
        public static ConfigEntry<bool> disableFootstepClickSound;
        public static ConfigEntry<bool> newStorageUI;
        public static ConfigEntry<string> notRespawningCreatures;
        public static ConfigEntry<string> notRespawningCreaturesIfKilledByPlayer;
        public static ConfigEntry<string> respawnTime;
        public static ConfigEntry<string> spawnChance;
        public static ConfigEntry<bool> propulsionCannonGrabFX;
        public static ConfigEntry<bool> fixCuteFish;
        public static ConfigEntry<SeaTreaderOutcrop> seaTreaderOutcrop;
        public static ConfigEntry<int> shroomDamageChance;
        public static ConfigEntry<bool> escapePodPowerTweak;
        public static ConfigEntry<bool> stalkersGrabShinyTool;
        public static ConfigEntry<bool> dropHeldTool;
        public static ConfigEntry<bool> newPoisonSystem;
        public static ConfigEntry<int> freeTorpedos;
        public static ConfigEntry<bool> lowOxygenWarning;
        public static ConfigEntry<bool> lowOxygenAudioWarning;
        public static ConfigEntry<bool> builderToolBuildsInsideWithoutPower;
        public static ConfigEntry<bool> cameraBobbing;
        public static ConfigEntry<bool> disableHints;
        public static ConfigEntry<bool> cyclopsHUDalwaysOn;
        public static ConfigEntry<bool> cameraShake;
        public static ConfigEntry<bool> removeDeadCreaturesOnLoad;
        public static ConfigEntry<bool> sunlightAffectsEscapePodLighting;
        public static ConfigEntry<bool> scannerFX;
        public static ConfigEntry<EscapePodMedkitCabinetWorks> escapePodMedkitCabinetWorks;
        public static ConfigEntry<bool> dropItemsAnywhere;
        public static ConfigEntry<bool> showTempFahrenhiet;
        public static ConfigEntry<bool> fixScreenResolution;
        //public static ConfigEntry<bool> flareTweaks;
        public static ConfigEntry<bool> vehiclesHurtCreatures;
        public static ConfigEntry<string> notRechargableBatteries;



        public static AcceptableValueRange<float> medKitHPperSecondRange = new AcceptableValueRange<float>(0.001f, 100f);
        public static AcceptableValueRange<int> percentRange = new AcceptableValueRange<int>(0, 100);
        public static AcceptableValueRange<int> freeTorpedosRange = new AcceptableValueRange<int>(0, 6);

        public static void Bind()
        {  // “ ” ‛
            heatBladeCooks = Main.configToEdit.Bind("", "Thermoblade cooks fish on kill", true);
            dontSpawnKnownFragments = Main.configToEdit.Bind("", "Do not spawn fragments for unlocked blueprints", false);
            cantScanExosuitClawArm = Main.configToEdit.Bind("", "Unlock prawn suit only by scanning prawn suit", true, "In vanilla game prawn suit can be unlocked by scanning 20 prawn suit arm fragments.");
            mapRoomFreeCameras = Main.configToEdit.Bind("", "Free camera drones for scanner room", true, "Scanner room will be built without camera drones if this is false.");
            decoyRequiresSub = Main.configToEdit.Bind("", "Creature decoy does not work when dropped from inventory", false);
            noKillParticles = Main.configToEdit.Bind("", "No yellow cloud particle effect when creature dies", false);
            cyclopsSunlight = Main.configToEdit.Bind("", "Sunlight affects lighting in cyclops", false);
            alwaysShowHealthFoodNunbers = Main.configToEdit.Bind("", "Always show player‛s health and hunger as numbers in UI", false);
            pdaClock = Main.configToEdit.Bind("", "PDA clock", true);

            gameStartWarningText = Main.configToEdit.Bind("", "Game start warning text", "", "Text shown when the game starts. If this field is empty the warning will be skipped.");
            newGameLoot = Main.configToEdit.Bind("", "Life pod items", "FilteredWater 2, NutrientBlock 2, Flare 2", "Items you find in your life pod when you start a new game. The format is item ID, space, number of items. Every entry is separated by comma.");
            crushDepthEquipment = Main.configToEdit.Bind("", "Crush depth equipment", "ReinforcedDiveSuit 0", "Equipment in this list increases your safe diving depth. The format is: item ID, space, number of meters that will be added to your safe diving depth. Every entry is separated by comma.");
            crushDamageEquipment = Main.configToEdit.Bind("", "Crush damage equipment", "ReinforcedDiveSuit 0", "Equipment in this list reduces your crush damage. The format is: item ID, space, crush damage percent that will be blocked. Every entry is separated by comma.");
            itemMass = Main.configToEdit.Bind("", "Item mass", "PrecursorKey_Blue 5, PrecursorKey_Orange 5, PrecursorKey_Purple 5, PrecursorKey_Red 5, PrecursorKey_White 5", "Allows you to change mass of pickupable items. The format is: item ID, space, item mass in kg as a decimal point number. Every entry is separated by comma.");
            unmovableItems = Main.configToEdit.Bind("", "Unmovable items", "", "Contains pickupable items that can not be moved by bumping into them. You will always find them where you dropped them.  Every entry is separated by comma.");
            bloodColor = Main.configToEdit.Bind("", "Blood color", new Vector3(0.784f, 1f, 0.157f), "Lets you change the color of creatures‛ blood. Each value is a decimal point number from 0 to 1. First number is red. Second number is green. Third number is blue.");
            gravTrappable = Main.configToEdit.Bind("", "Gravtrappable items", "seaglide, airbladder, flare, flashlight, builder, lasercutter, ledlight, divereel, propulsioncannon, welder, repulsioncannon, scanner, stasisrifle, knife, heatblade, precursorkey_blue, precursorkey_orange, precursorkey_purple, precursorkey_red, precursorkey_white, compass, fins, fireextinguisher, firstaidkit, doubletank, plasteeltank, radiationsuit, radiationhelmet, radiationgloves, rebreather, reinforceddivesuit, maproomhudchip, tank, stillsuit, swimchargefins, ultraglidefins, highcapacitytank,", "List of items affected by grav trap.");
            medKitHPperSecond = Main.configToEdit.Bind("", "Amount of HP restored by first aid kit every second", 100f, new ConfigDescription("Set this to a small number to slowly restore HP after using first aid kit.", medKitHPperSecondRange));
            silentCreatures = Main.configToEdit.Bind("", "Silent creatures", "", "List of creature IDs separated by comma. Creatures in this list will be silent.");

            eatableFoodValue = Main.configToEdit.Bind("", "Eatable component food value", "CreepvineSeedCluster 5", "Items from this list will be made eatable. The format is: item ID, space, food value. Every entry is separated by comma.");
            eatableWaterValue = Main.configToEdit.Bind("", "Eatable component water value", "CreepvineSeedCluster 10", "Items from this list will be made eatable. The format is: item ID, space, water value. Every entry is separated by comma.");
            fixMelons = Main.configToEdit.Bind("", "Fix melons", false, "If true you will be able to plant only 1 melon in a pot and only 4 in a planter.");
            randomPlantRotation = Main.configToEdit.Bind("", "Random plant rotation", true, "Plants in planters will have random rotation if this is true.");
            silentReactor = Main.configToEdit.Bind("", "Silent nuclear reactor", false, "Nuclear reactor will be silent if this is true.");
            removeFragmentCrate = Main.configToEdit.Bind("", "Remove fragment crate", false, "When you scan a blueprint fragment, the crate holding the fragment will be removed if this is true.");
            creepvineLights = Main.configToEdit.Bind("", "Real creepvine lights", true, "Creepvine seed cluster light intensity will depend on number of seed clusters on the vine if this is true.");
            newUIstrings = Main.configToEdit.Bind("", "New UI text", true, "New UI elements added by the mod wil be disabled if this is false.");
            newStorageUI = Main.configToEdit.Bind("", "New storage UI", true, "New UI for storage containers");
            //tweaksAffectingGPU = Main.configToEdit.Bind("", "Tweaks affecting GPU", true, "If false changes that may affect GPU performance (LOD edits, edits that increase popping in distance for objects) will be disabled.");
            disableUseText = Main.configToEdit.Bind("", "Disable quickslots text", false, "Text above your quickslots will be disabled if this is true.");

            notRechargableBatteries = Main.configToEdit.Bind("", "Not rechargable batteries", "", "Comma separated list of battery IDs. Batteries from this list can not be recharged");
            craftWithoutBattery = Main.configToEdit.Bind("", "Craft without battery", false, "Your newly crafted tools and vehicles will not have batteries in them if this is true.");
            disableCyclopsProximitySensor = Main.configToEdit.Bind("", "Disable cyclops proximity sensor", false);
            builderPlacingWhenFinishedBuilding = Main.configToEdit.Bind("", "Builder tool placing mode when finished building", true, "Your builder tool will exit placing mode when you finish building if this is false .");
            crushDamageScreenEffect = Main.configToEdit.Bind("", "Crush damage screen effect", true, "There will be no screen effects when player takes crush damage if this is false.");
            removeCookedFishOnReload = Main.configToEdit.Bind("", "Remove cooked fish when loading saved game", false, "Cooked fish will be removed from the world (not from containers) when loading saved game if this is true.");
            disableGravityForExosuit = Main.configToEdit.Bind("", "Disable gravity for prawn suit", false, "Prawn suit will ignore gravity when you are not piloting it if this is true. Use this if your prawn suit falls through the ground.");
            replaceDealDamageOnImpactScript = Main.configToEdit.Bind("", "Replace DealDamageOnImpact script", false, "The game will use vanilla script when vehicles collide with objects if this is false.");
            cyclopsDealDamageMinSpeed = Main.configToEdit.Bind("", "Cyclops min speed to deal damage", 2f, "Min speed in meters per second at which cyclops deals damage when colliding with objects. Works only if ‛Replace DealDamageOnImpact script‛ setting is true.");
            cyclopsTakeDamageMinSpeed = Main.configToEdit.Bind("", "Cyclops min speed to take damage", 2f, "Min speed in meters per second at which cyclops takes damage when colliding with objects. Works only if ‛Replace DealDamageOnImpact script‛ setting is true.");
            cyclopsTakeDamageMinMass = Main.configToEdit.Bind("", "Min mass that can damage cyclops", 200f, "Min mass in kg for objects that can damage cyclops when colliding with it. Works only if ‛Replace DealDamageOnImpact script‛ setting is true.");
            exosuitDealDamageMinSpeed = Main.configToEdit.Bind("", "Prawn suit min speed to deal damage", 7f, "Min speed in meters per second at which prawn suit deals damage when colliding with objects. Works only if ‛Replace DealDamageOnImpact script‛ setting is true.");
            exosuitTakeDamageMinSpeed = Main.configToEdit.Bind("", "Prawn suit min speed to take damage", 7f, "Min speed in meters per second at which prawn suit takes damage when colliding with objects. Works only if ‛Replace DealDamageOnImpact script‛ setting is true.");
            exosuitTakeDamageMinMass = Main.configToEdit.Bind("", "Min mass that can damage prawn suit", 5f, "Min mass in kg for objects that can damage prawn suit when colliding with it. Works only if ‛Replace DealDamageOnImpact script‛ setting is true.");
            seamothDealDamageMinSpeed = Main.configToEdit.Bind("", "Seamoth min speed to deal damage", 7f, "Min speed in meters per second at which seamoth deals damage when colliding with objects. Works only if ‛Replace DealDamageOnImpact script‛ setting is true.");
            seamothTakeDamageMinSpeed = Main.configToEdit.Bind("", "Seamoth min speed to take damage", 7f, "Min speed in meters per second at which seamoth takes damage when colliding with objects. Works only if ‛Replace DealDamageOnImpact script‛ setting is true.");
            seamothTakeDamageMinMass = Main.configToEdit.Bind("", "Min mass that can damage seamoth", 5f, "Min mass in kg for objects that can damage seamoth when colliding with it. Works only if ‛Replace DealDamageOnImpact script‛ setting is true.");
            vehiclesHurtCreatures = Main.configToEdit.Bind("", "Vehicles hurt creatures", true, "Vehicles will not hurt creatures when colliding with them if this is false. Works only if ‛Replace DealDamageOnImpact script‛ setting is true.");
            lavaGeyserEruptionForce = Main.configToEdit.Bind("", "Lava geyser eruption force", 20f, "Force  applied to objects by lava geysers.");
            lavaGeyserEruptionInterval = Main.configToEdit.Bind("", "Lava geyser eruption interval", 12f, "Time in seconds between lava geyser eruptions including 5.5 seconds of eruption.");
            removeLavaGeyserRockParticles = Main.configToEdit.Bind("", "Remove rock particles from lava geysers", false, "Rock particles will be removed from lava geyser eruptions if this is true.");
            solarPanelMaxDepth = Main.configToEdit.Bind("", "Solar panel max depth", 250f, "Depth in meters below which solar panel does not produce power.");
            stalkerLooseToothSound = Main.configToEdit.Bind("", "Stalker losing tooth sound", true, "There will be no sound effect when a stalker loses its tooth if this false.");
            canReplantMelon = Main.configToEdit.Bind("", "Can replant melon", true, "Gel sack and melon can not be replanted if this is false.");
            removeCookedFishFromBlueprints = Main.configToEdit.Bind("", "Remove fish recipes from blueprints PDA tab", false);
            removeWaterFromBlueprints = Main.configToEdit.Bind("", "Remove water recipes from blueprints PDA tab", false);
            disableFootstepClickSound = Main.configToEdit.Bind("", "Disable clicking sound when walking on metal surface", true);
            notRespawningCreatures = Main.configToEdit.Bind("", "Not respawning creatures", "Warper, GhostLeviathan, GhostLeviathanJuvenile, ReaperLeviathan, Reefback, SeaTreader", "Comma separated list of creature IDs that will not respawn when killed.");
            notRespawningCreaturesIfKilledByPlayer = Main.configToEdit.Bind("", "Not respawning creatures if killed by player", "Biter, Bleeder, Blighter, Gasopod, Stalker, Shocker, BoneShark, Crabsnake, CrabSquid, LavaLizard, Mesmer, Sandshark, SpineEel, Rockgrub, Shuttlebug, CaveCrawler, LavaLarva, SeaEmperorBaby, SeaEmperorJuvenile", "Comma separated list of creature IDs that will respawn only if killed by another creature.");
            respawnTime = Main.configToEdit.Bind("", "Creature respawn time", "", "Number of days it takes a creature to respawn. The format is: creature ID, space, number of days it takes to respawn. By default fish and big creatures respawn in 12 hours, leviathans respawn after 1 day.");
            //spawnChance = Main.configB.Bind("", "Spawn chance", "", "Chance for a object ID to spawn. The format is: object ID, space, chance to spawn percent. Can be used only to reduce chances to spawn. Every entry is separated by comma.");
            propulsionCannonGrabFX = Main.configToEdit.Bind("", "Propulsion cannon sphere effect", true, "Blue sphere visual effect you see when holding an object with propulsion cannon will be disabled if this is false.");
            fixCuteFish = Main.configToEdit.Bind("", "Fix cuddlefish", false, "You will be able to interact with cuddlefish only when swimming if this is true.");
            seaTreaderOutcrop = Main.configToEdit.Bind("", "Outcrop from seatreader", SeaTreaderOutcrop.Vanilla, "");
            shroomDamageChance = Main.configToEdit.Bind("", "Mushroom damage chance percent", 0, new ConfigDescription("Chance of a mushroom dealing damage to player when picked up and dealing area damage when destroyed. The script to do it was always in the game but was disabled.", percentRange));
            escapePodPowerTweak = Main.configToEdit.Bind("", "Life pod power tweaks", false, "When your life pod is damaged its max power is reduced to 50%. When you crashland your life pod power cells are not charged.");
            stalkerPlayThings = Main.configToEdit.Bind("", "Items stalkers can grab", "ScrapMetal, MapRoomCamera, Beacon, Seaglide, CyclopsDecoy, Gravsphere, SmallStorage, FireExtinguisher, DoubleTank, PlasteelTank, PrecursorKey_Blue, PrecursorKey_Orange, PrecursorKey_Purple, PrecursorKey_Red, PrecursorKey_White, Rebreather, Tank, HighCapacityTank, Flare, Flashlight, Builder, LaserCutter, LEDLight, DiveReel, PropulsionCannon, Knife, HeatBlade, Scanner, Welder, RepulsionCannon, StasisRifle", "List of item IDs separated by comma. Only items in this list can be grabbed by stalkers.");
            stalkersGrabShinyTool = Main.configToEdit.Bind("", "Stalkers grab tools from player hands when playing", false, "Stalkers can grab only things that are in the ‛Items stalkers can grab‛ list.");
            dropHeldTool = Main.configToEdit.Bind("", "Drop tool in your hands when taking damage", false, "Chance percent to drop your tool is equal to amount of damage taken.");
            newPoisonSystem = Main.configToEdit.Bind("", "New poison damage system", false, "Every 2 seconds poison will deal 1 point of permanent damage and decrease your food and water values by 1. Using first aid kit will remove poison from your system.");
            freeTorpedos = Main.configToEdit.Bind("", "Free torpedos", 2, new ConfigDescription("Number of torpedos you get when installing Torpedo System or Prawn Suit Torpedo Arm. After changing this you have to craft a new Torpedo System.", freeTorpedosRange));
            lowOxygenWarning = Main.configToEdit.Bind("", "Low oxygen onscreen warning", true);
            lowOxygenAudioWarning = Main.configToEdit.Bind("", "Low oxygen audio warning", true);
            builderToolBuildsInsideWithoutPower = Main.configToEdit.Bind("", "Builder tool does not need power when building inside", true);
            cameraBobbing = Main.configToEdit.Bind("", "Screen bobbing when swimming", true);
            disableHints = Main.configToEdit.Bind("", "Disable tutorial messages", true, "This disables messages that tell you to 'eat something', 'break limestone', etc.");
            cyclopsHUDalwaysOn = Main.configToEdit.Bind("", "Cyclops HUD always on", false);
            cameraShake = Main.configToEdit.Bind("", "Screen shakes when cyclops takes damage", true);
            removeDeadCreaturesOnLoad = Main.configToEdit.Bind("", "Remove dead creatures when loading saved game", true, "");
            sunlightAffectsEscapePodLighting = Main.configToEdit.Bind("", "Sunlight affects lighting in your life pod", false, "");
            scannerFX = Main.configToEdit.Bind("", "Wierd visual effect on objects being scanned", true, "");
            escapePodMedkitCabinetWorks = Main.configToEdit.Bind("", "Medical kit fabricator in your life pod produces first aid kit", EscapePodMedkitCabinetWorks.Always, "");
            dropItemsAnywhere = Main.configToEdit.Bind("", "Player can drop inventory items anywhere", false, "This allows you to place placable items anywhere in the world, drop items anywhere except cyclops and grab items in your base with propulsion cannon.");
            showTempFahrenhiet = Main.configToEdit.Bind("", "Show temperature in Fahrenhiet instead of Celcius", false, "");
            fixScreenResolution = Main.configToEdit.Bind("", "Fix screen resolution", false, "The game sometimes resets screen resolution to desktop one. Set this to true to fix it.");
            //flareTweaks = Main.configToEdit.Bind("", "Flare tweaks", true, "You will be able to light flares without throwing them. You will not be able to throw flares in locations that do not allow dropping items.");






            transferAllItemsButton = Main.configToEdit.Bind("", "Move all items button", Button.None, "Press this button to move all items from one container to another. This works only with controller. Use this if you can not bind a controller button in the mod menu.");
            transferSameItemsButton = Main.configToEdit.Bind("", "Move same items button", Button.None, "Press this button to move all items of the same type from one container to another. This works only with controller. Use this if you can not bind a controller button in the mod menu.");
            quickslotButton = Main.configToEdit.Bind("", "Quickslot cycle button", Button.None, "Press 'Cycle next' or 'Cycle previous' button while holding down this button to cycle tools in your current quickslot. This works only with controller. Use this if you can not bind a controller button in the mod menu.");
            lightButton = Main.configToEdit.Bind("", "Light intensity button", Button.None, "When holding a tool in your hand or driving a vehicle press 'Cycle next' or 'Cycle previous' button while holding down this button to change the tool's or vehicle's light intensity. This works only with controller. Use this if you can not bind a controller button in the mod menu.");


        }


        private static Dictionary<TechType, int> ParseIntDicFromString(string input)
        {
            Dictionary<TechType, int> dic = new Dictionary<TechType, int>();
            string[] entries = input.Split(',');
            for (int i = 0; i < entries.Length; i++)
            {
                string s = entries[i].Trim();
                string techType;
                string amount;
                int index = s.IndexOf(' ');
                if (index == -1)
                    continue;

                techType = s.Substring(0, index);
                amount = s.Substring(index);
                if (!TechTypeExtensions.FromString(techType, out TechType tt, true))
                    continue;
                // no simple way to check if techType is pickupable
                int a = 0;
                try
                {
                    a = int.Parse(amount);
                }
                catch (Exception)
                {
                    continue;
                }
                if (a < 1)
                    continue;

                dic.Add(tt, a);
            }
            return dic;
        }

        private static Dictionary<TechType, float> ParseFloatDicFromString(string input)
        {
            Dictionary<TechType, float> dic = new Dictionary<TechType, float>();
            string[] entries = input.Split(',');
            for (int i = 0; i < entries.Length; i++)
            {
                string s = entries[i].Trim();
                string techType;
                string amount;
                int index = s.IndexOf(' ');
                if (index == -1)
                    continue;

                techType = s.Substring(0, index);
                amount = s.Substring(index);
                if (!TechTypeExtensions.FromString(techType, out TechType tt, true))
                    continue;
                // no simple way to check if techType is pickupable
                float fl = 0;
                try
                {
                    fl = float.Parse(amount);
                }
                catch (Exception)
                {
                    continue;
                }
                if (fl < 1)
                    continue;

                dic.Add(tt, fl);
            }
            return dic;
        }

        private static HashSet<TechType> ParseSetFromString(string input)
        {
            HashSet<TechType> set = new HashSet<TechType>();
            string[] entries = input.Split(',');
            for (int i = 0; i < entries.Length; i++)
            {
                string techType = entries[i].Trim();

                if (!TechTypeExtensions.FromString(techType, out TechType tt, true))
                    continue;
                // no simple way to check if techType is pickupable
                set.Add(tt);
                //Main.logger.LogDebug("ParseSetFromString " + tt );
            }
            return set;
        }

        public static void ParseFromConfig()
        {
            Crush_Damage.crushDepthEquipment = ParseIntDicFromString(crushDepthEquipment.Value);
            Crush_Damage.crushDamageEquipment = ParseIntDicFromString(crushDamageEquipment.Value);
            Pickupable_Patch.itemMass = ParseFloatDicFromString(itemMass.Value);
            Pickupable_Patch.unmovableItems = ParseSetFromString(unmovableItems.Value);
            Gravsphere_Patch.gravTrappable = ParseSetFromString(gravTrappable.Value);
            Creature_Tweaks.silentCreatures = ParseSetFromString(silentCreatures.Value);
            Pickupable_Patch.shinies = ParseSetFromString(stalkerPlayThings.Value);
            LargeWorldEntity_Patch.eatableFoodValue = ParseIntDicFromString(eatableFoodValue.Value);
            LargeWorldEntity_Patch.eatableWaterValue = ParseIntDicFromString(eatableWaterValue.Value);
            Escape_Pod_Patch.newGameLoot = ParseIntDicFromString(newGameLoot.Value);
            CreatureDeath_Patch.notRespawningCreatures = ParseSetFromString(notRespawningCreatures.Value);
            CreatureDeath_Patch.notRespawningCreaturesIfKilledByPlayer = ParseSetFromString(notRespawningCreaturesIfKilledByPlayer.Value);
            CreatureDeath_Patch.respawnTime = ParseIntDicFromString(respawnTime.Value);
            //LargeWorldEntity_Patch.techTypesToDespawn = ParseIntDicFromString(spawnChance.Value);
            Battery_Patch.notRechargableBatteries = ParseSetFromString(notRechargableBatteries.Value);

            Enum.TryParse(transferAllItemsButton.Value.ToString(), out Inventory_Patch.transferAllItemsButton);
            Enum.TryParse(transferSameItemsButton.Value.ToString(), out Inventory_Patch.transferSameItemsButton);
            Enum.TryParse(quickslotButton.Value.ToString(), out QuickSlots_Patch.quickslotButton);
            Enum.TryParse(lightButton.Value.ToString(), out QuickSlots_Patch.lightButton);
            //Main.logger.LogDebug("transferAllItemsButton " + Inventory_Patch.transferAllItemsButton);
            //Main.logger.LogDebug("transferSameItemsButton " + Inventory_Patch.transferSameItemsButton);
        }

        public enum SeaTreaderOutcrop { Vanilla, Only_when_stomping_ground, Never }
        public enum EscapePodMedkitCabinetWorks { Always, After_repairing_life_pod, Never }

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
