using BepInEx.Configuration;
using Nautilus.Json;
using System;
using System.Collections;
using System.Collections.Generic;
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
        public static ConfigEntry<string> bloodColor;
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
        public static ConfigEntry<int> seaTreaderOutcropMult;
        public static ConfigEntry<int> seaTreaderAttackOutcropMult;
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
        public static ConfigEntry<int> targetFrameRate;
        public static ConfigEntry<bool> removeCreditsButton;
        public static ConfigEntry<bool> removeRedeemButton;
        public static ConfigEntry<bool> removeTroubleshootButton;
        public static ConfigEntry<bool> removeUnstuckButton;
        public static ConfigEntry<bool> removeFeedbackButton;
        public static ConfigEntry<bool> enableDevButton;
        public static ConfigEntry<bool> propCannonGrabsAnyPlant;
        public static ConfigEntry<bool> seaglideWorksOnlyForward;
        public static ConfigEntry<int> oneHandToolSwimSpeedMod;
        public static ConfigEntry<int> oneHandToolWalkSpeedMod;
        public static ConfigEntry<int> twoHandToolSwimSpeedMod;
        public static ConfigEntry<int> twoHandToolWalkSpeedMod;
        public static ConfigEntry<int> playerSidewardSpeedMod;
        public static ConfigEntry<int> playerBackwardSpeedMod;
        public static ConfigEntry<int> playerVerticalSpeedMod;
        public static ConfigEntry<bool> sprintOnlyForward;
        public static ConfigEntry<string> groundSpeedEquipment;
        public static ConfigEntry<string> waterSpeedEquipment;
        public static ConfigEntry<int> seamothSidewardSpeedMod;
        public static ConfigEntry<int> seamothBackwardSpeedMod;
        public static ConfigEntry<int> seamothVerticalSpeedMod;
        public static ConfigEntry<bool> disableExosuitSidestep;
        public static ConfigEntry<bool> exosuitThrusterWithoutLimit;
        public static ConfigEntry<bool> fixSeamothMove;
        public static ConfigEntry<bool> fixCyclopsMove;
        public static ConfigEntry<int> cyclopsVerticalSpeedMod;
        public static ConfigEntry<int> cyclopsBackwardSpeedMod;




        public static ConfigEntry<bool> disableIonCubeFabricator;



        public static AcceptableValueRange<float> medKitHPperSecondRange = new AcceptableValueRange<float>(0.001f, 100f);
        public static AcceptableValueRange<int> percentRange = new AcceptableValueRange<int>(0, 100);
        public static AcceptableValueRange<int> freeTorpedosRange = new AcceptableValueRange<int>(0, 6);

        public static void Bind()
        {  // “ ” ‛

            seaglideWorksOnlyForward = Main.configToEdit.Bind("PLAYER MOVEMENT", "Seaglide works only when moving forward", false, "");
            targetFrameRate = Main.configToEdit.Bind("MISC", "Frame rate limiter", 0, "Number of frames the game renders every second will be limited to this. Numbers smaller than 10 are ignored.");
            heatBladeCooks = Main.configToEdit.Bind("TOOLS", "Thermoblade cooks fish on kill", true);
            dontSpawnKnownFragments = Main.configToEdit.Bind("FRAGMENTS", "Do not spawn fragments for unlocked blueprints", false);
            cantScanExosuitClawArm = Main.configToEdit.Bind("FRAGMENTS", "Unlock prawn suit only by scanning prawn suit", true, "In vanilla game prawn suit can be unlocked by scanning 20 prawn suit arm fragments.");
            mapRoomFreeCameras = Main.configToEdit.Bind("BASE", "Free camera drones for scanner room", true, "Scanner room will be built without camera drones if this is false.");
            decoyRequiresSub = Main.configToEdit.Bind("ITEMS", "Creature decoy does not work when dropped from inventory", false);
            noKillParticles = Main.configToEdit.Bind("CREATURES", "No yellow cloud particle effect when creature dies", false);
            cyclopsSunlight = Main.configToEdit.Bind("CYCLOPS", "Sunlight affects lighting in cyclops", false);
            alwaysShowHealthFoodNunbers = Main.configToEdit.Bind("UI", "Always show player‛s health and hunger as numbers in UI", false);
            pdaClock = Main.configToEdit.Bind("PDA", "PDA clock", true);

            gameStartWarningText = Main.configToEdit.Bind("MISC", "Game start warning text", "", "Text shown when the game starts. If this field is empty the warning will be skipped.");
            newGameLoot = Main.configToEdit.Bind("LIFE POD", "Life pod items", "FilteredWater 2, NutrientBlock 2, Flare 2", "Items you find in your life pod when you start a new game. The format is item ID, space, number of items. Every entry is separated by comma.");


            sprintOnlyForward = Main.configToEdit.Bind("PLAYER MOVEMENT", "Player can sprint only when moving forward", false);
            playerSidewardSpeedMod = Main.configToEdit.Bind("PLAYER MOVEMENT", "Player sideward speed modifier", 0, "Player's speed will be reduced by this percent when moving sideward.");
            playerBackwardSpeedMod = Main.configToEdit.Bind("PLAYER MOVEMENT", "Player backward speed modifier", 0, "Player's speed will be reduced by this percent when moving backward.");
            playerVerticalSpeedMod = Main.configToEdit.Bind("PLAYER MOVEMENT", "Player vertical speed modifier", 0, "Player's speed will be reduced by this percent when swimming up or down.");
            oneHandToolWalkSpeedMod = Main.configToEdit.Bind("PLAYER MOVEMENT", "One handed tool walking speed modifier", 0, "Your walking speed will be reduced by this percent when holding tool or PDA with one hand.");
            twoHandToolWalkSpeedMod = Main.configToEdit.Bind("PLAYER MOVEMENT", "Two handed tool walking speed modifier", 0, "Your walking speed will be reduced by this percent when holding tool with both hands.");
            oneHandToolSwimSpeedMod = Main.configToEdit.Bind("PLAYER MOVEMENT", "One handed tool swimming speed modifier", 0, "Your swimming speed will be reduced by this percent when holding tool or PDA with one hand.");
            twoHandToolSwimSpeedMod = Main.configToEdit.Bind("PLAYER MOVEMENT", "Two handed tool swimming speed modifier", 0, "Your swimming speed will be reduced by this percent when holding tool with both hands.");
            groundSpeedEquipment = Main.configToEdit.Bind("EQUIPMENT", "Ground speed equipment", "", "Equipment in this list affects your movement speed on ground. The format is: item ID, space, percent that will be added to your movement speed. Negative numbers will reduce your movement speed. Every entry is separated by comma.");
            waterSpeedEquipment = Main.configToEdit.Bind("EQUIPMENT", "Water speed equipment", "", "Equipment in this list affects your movement speed in water. The format is: item ID, space, percent that will be added to your movement speed. Negative numbers will reduce your movement speed. Every entry is separated by comma. If this list is not empty, vanilla script that changes your movement speed will not run. ");

            crushDepthEquipment = Main.configToEdit.Bind("EQUIPMENT", "Crush depth equipment", "ReinforcedDiveSuit 0", "Equipment in this list increases your safe diving depth. The format is: item ID, space, number of meters that will be added to your safe diving depth. Every entry is separated by comma.");
            crushDamageEquipment = Main.configToEdit.Bind("EQUIPMENT", "Crush damage equipment", "ReinforcedDiveSuit 0", "Equipment in this list reduces your crush damage. The format is: item ID, space, crush damage percent that will be blocked. Every entry is separated by comma.");
            itemMass = Main.configToEdit.Bind("ITEMS", "Item mass", "PrecursorKey_Blue 5, PrecursorKey_Orange 5, PrecursorKey_Purple 5, PrecursorKey_Red 5, PrecursorKey_White 5", "Allows you to change mass of pickupable items. The format is: item ID, space, item mass in kg as a decimal point number. Every entry is separated by comma.");
            unmovableItems = Main.configToEdit.Bind("ITEMS", "Unmovable items", "", "Contains pickupable items that can not be moved by bumping into them. You will always find them where you dropped them.  Every entry is separated by comma.");
            bloodColor = Main.configToEdit.Bind("CREATURES", "Blood color", "0.784 1.0 0.157", "Creatures‛ blood color will be set to this. Each value is a decimal point number from 0 to 1. First number is red. Second number is green. Third number is blue.");
            gravTrappable = Main.configToEdit.Bind("TOOLS", "Gravtrappable items", "seaglide, airbladder, flare, flashlight, builder, lasercutter, ledlight, divereel, propulsioncannon, welder, repulsioncannon, scanner, stasisrifle, knife, heatblade, precursorkey_blue, precursorkey_orange, precursorkey_purple, precursorkey_red, precursorkey_white, compass, fins, fireextinguisher, firstaidkit, doubletank, plasteeltank, radiationsuit, radiationhelmet, radiationgloves, rebreather, reinforceddivesuit, maproomhudchip, tank, stillsuit, swimchargefins, ultraglidefins, highcapacitytank,", "List of items affected by grav trap. This list does not replace vanilla list. Items from this list will be added to vanilla one.");
            medKitHPperSecond = Main.configToEdit.Bind("ITEMS", "Amount of HP restored by first aid kit every second", 100f, new ConfigDescription("Set this to a small number to slowly restore HP after using first aid kit.", medKitHPperSecondRange));
            silentCreatures = Main.configToEdit.Bind("CREATURES", "Silent creatures", "", "List of creature IDs separated by comma. Creatures in this list will be silent.");

            eatableFoodValue = Main.configToEdit.Bind("ITEMS", "Eatable component food value", "CreepvineSeedCluster 5", "Items from this list will be made eatable. The format is: item ID, space, food value. Every entry is separated by comma.");
            eatableWaterValue = Main.configToEdit.Bind("ITEMS", "Eatable component water value", "CreepvineSeedCluster 10", "Items from this list will be made eatable. The format is: item ID, space, water value. Every entry is separated by comma.");
            fixMelons = Main.configToEdit.Bind("PLANTS", "Fix melons", false, "You will be able to plant only 1 melon in a pot and only 4 in a planter if this is true.");
            randomPlantRotation = Main.configToEdit.Bind("PLANTS", "Random plant rotation", true, "Plants in planters will have random rotation if this is true.");
            silentReactor = Main.configToEdit.Bind("BASE", "Silent nuclear reactor", false);
            removeFragmentCrate = Main.configToEdit.Bind("FRAGMENTS", "Remove fragment crate", false, "When you scan a blueprint fragment, the crate holding the fragment will be removed if this is true.");
            creepvineLights = Main.configToEdit.Bind("PLANTS", "Real creepvine lights", true, "Creepvine seed cluster light intensity will depend on number of seed clusters on the vine if this is true.");
            newUIstrings = Main.configToEdit.Bind("UI", "New UI text", true, "New UI elements added by the mod wil be disabled if this is false.");
            newStorageUI = Main.configToEdit.Bind("UI", "New storage UI", true, "New UI for storage containers");

            disableUseText = Main.configToEdit.Bind("UI", "Disable quickslots text", false, "Text above your quickslots will be disabled if this is true.");

            notRechargableBatteries = Main.configToEdit.Bind("ITEMS", "Not rechargable batteries", "", "Comma separated list of battery IDs. Batteries from this list can not be recharged");
            craftWithoutBattery = Main.configToEdit.Bind("TOOLS", "Craft without battery", false, "Your newly crafted tools and vehicles will not have batteries in them if this is true.");
            disableCyclopsProximitySensor = Main.configToEdit.Bind("CYCLOPS", "Disable cyclops proximity sensor", false);
            builderPlacingWhenFinishedBuilding = Main.configToEdit.Bind("TOOLS", "Builder tool placing mode when finished building", true, "Your builder tool will exit placing mode when you finish building if this is false .");
            crushDamageScreenEffect = Main.configToEdit.Bind("PLAYER", "Crush damage screen effect", true, "There will be no screen effects when player takes crush damage if this is false.");
            removeCookedFishOnReload = Main.configToEdit.Bind("CREATURES", "Remove cooked fish when loading saved game", false, "Cooked fish will be removed from the world (not from containers) when loading saved game if this is true.");
            disableGravityForExosuit = Main.configToEdit.Bind("VEHICLES", "Disable gravity for prawn suit", false, "Prawn suit will ignore gravity when you are not piloting it if this is true. Use this if your prawn suit falls through the ground.");
            replaceDealDamageOnImpactScript = Main.configToEdit.Bind("VEHICLES", "Replace DealDamageOnImpact script", false, "The game will use vanilla script when vehicles collide with objects if this is false.");
            cyclopsDealDamageMinSpeed = Main.configToEdit.Bind("CYCLOPS", "Cyclops min speed to deal damage", 2f, "Min speed in meters per second at which cyclops deals damage when colliding with objects. Works only if ‛Replace DealDamageOnImpact script‛ setting is true.");
            cyclopsTakeDamageMinSpeed = Main.configToEdit.Bind("CYCLOPS", "Cyclops min speed to take damage", 2f, "Min speed in meters per second at which cyclops takes damage when colliding with objects. Works only if ‛Replace DealDamageOnImpact script‛ setting is true.");
            cyclopsTakeDamageMinMass = Main.configToEdit.Bind("CYCLOPS", "Min mass that can damage cyclops", 200f, "Min mass in kg for objects that can damage cyclops when colliding with it. Works only if ‛Replace DealDamageOnImpact script‛ setting is true.");
            exosuitDealDamageMinSpeed = Main.configToEdit.Bind("VEHICLES", "Prawn suit min speed to deal damage", 7f, "Min speed in meters per second at which prawn suit deals damage when colliding with objects. Works only if ‛Replace DealDamageOnImpact script‛ setting is true.");
            exosuitTakeDamageMinSpeed = Main.configToEdit.Bind("VEHICLES", "Prawn suit min speed to take damage", 7f, "Min speed in meters per second at which prawn suit takes damage when colliding with objects. Works only if ‛Replace DealDamageOnImpact script‛ setting is true.");
            exosuitTakeDamageMinMass = Main.configToEdit.Bind("VEHICLES", "Min mass that can damage prawn suit", 5f, "Min mass in kg for objects that can damage prawn suit when colliding with it. Works only if ‛Replace DealDamageOnImpact script‛ setting is true.");
            seamothDealDamageMinSpeed = Main.configToEdit.Bind("VEHICLES", "Seamoth min speed to deal damage", 7f, "Min speed in meters per second at which seamoth deals damage when colliding with objects. Works only if ‛Replace DealDamageOnImpact script‛ setting is true.");
            seamothTakeDamageMinSpeed = Main.configToEdit.Bind("VEHICLES", "Seamoth min speed to take damage", 7f, "Min speed in meters per second at which seamoth takes damage when colliding with objects. Works only if ‛Replace DealDamageOnImpact script‛ setting is true.");
            seamothTakeDamageMinMass = Main.configToEdit.Bind("VEHICLES", "Min mass that can damage seamoth", 5f, "Min mass in kg for objects that can damage seamoth when colliding with it. Works only if ‛Replace DealDamageOnImpact script‛ setting is true.");
            vehiclesHurtCreatures = Main.configToEdit.Bind("VEHICLES", "Vehicles hurt creatures", true, "Vehicles will not hurt creatures when colliding with them if this is false.");
            fixSeamothMove = Main.configToEdit.Bind("VEHICLES", "Fix seamoth diagonal movement", false, "Seamoth will not exceed its max speed and will not consume more power when moving diagonally.");
            seamothSidewardSpeedMod = Main.configToEdit.Bind("VEHICLES", "Seamoth sideward speed modifier", 0, "Seamoth speed will be reduced by this percent when moving sideward.");
            seamothBackwardSpeedMod = Main.configToEdit.Bind("VEHICLES", "Seamoth backward speed modifier", 0, "Seamoth speed will be reduced by this percent when moving backward.");
            seamothVerticalSpeedMod = Main.configToEdit.Bind("VEHICLES", "Seamoth vertical speed modifier", 0, "Seamoth speed will be reduced by this percent when moving up or down.");
            disableExosuitSidestep = Main.configToEdit.Bind("VEHICLES", "Disable prawn suit sidestep", false, "");
            exosuitThrusterWithoutLimit = Main.configToEdit.Bind("VEHICLES", "Prawn suit thrusters never overheat", false, "No time limit when using thrusters, but they consume twice more power than walking");

            cyclopsVerticalSpeedMod = Main.configToEdit.Bind("CYCLOPS", "Cyclops vertical speed modifier", 0, "Cyclops speed will be reduced by this percent when moving up or down.");
            cyclopsBackwardSpeedMod = Main.configToEdit.Bind("CYCLOPS", "Cyclops backward speed modifier", 0, "Cyclops speed will be reduced by this percent when moving backward.");
            fixCyclopsMove = Main.configToEdit.Bind("CYCLOPS", "Fix cyclops diagonal movement", false, "Cyclops will not exceed its max speed and will not consume more power when moving diagonally.");



            lavaGeyserEruptionForce = Main.configToEdit.Bind("MISC", "Lava geyser eruption force", 20f, "Force applied to objects by lava geysers.");
            lavaGeyserEruptionInterval = Main.configToEdit.Bind("MISC", "Lava geyser eruption interval", 12f, "Time in seconds between lava geyser eruptions including 5.5 seconds of eruption.");
            removeLavaGeyserRockParticles = Main.configToEdit.Bind("MISC", "Remove rock particles from lava geysers", false, "Rock particles will be removed from lava geyser eruptions if this is true.");
            solarPanelMaxDepth = Main.configToEdit.Bind("BASE", "Solar panel max depth", 250f, "Depth in meters below which solar panel does not produce power.");
            stalkerLooseToothSound = Main.configToEdit.Bind("CREATURES", "Stalker losing tooth sound", true, "There will be no sound effect when a stalker loses its tooth if this false.");
            canReplantMelon = Main.configToEdit.Bind("PLANTS", "Can replant melon", true, "Gel sack and melon can not be replanted if this is false.");
            removeCookedFishFromBlueprints = Main.configToEdit.Bind("PDA", "Remove fish recipes from blueprints PDA tab", false);
            removeWaterFromBlueprints = Main.configToEdit.Bind("PDA", "Remove water recipes from blueprints PDA tab", false);
            disableFootstepClickSound = Main.configToEdit.Bind("PLAYER", "Disable clicking sound when walking on metal surface", true);
            notRespawningCreatures = Main.configToEdit.Bind("CREATURES", "Not respawning creatures", "Warper, GhostLeviathan, GhostLeviathanJuvenile, ReaperLeviathan, Reefback, SeaTreader", "Comma separated list of creature IDs that will not respawn when killed.");
            notRespawningCreaturesIfKilledByPlayer = Main.configToEdit.Bind("CREATURES", "Not respawning creatures if killed by player", "Biter, Bleeder, Blighter, Gasopod, Stalker, Shocker, BoneShark, Crabsnake, CrabSquid, LavaLizard, Mesmer, Sandshark, SpineEel, Rockgrub, Shuttlebug, CaveCrawler, LavaLarva, SeaEmperorBaby, SeaEmperorJuvenile", "Comma separated list of creature IDs that will respawn only if killed by another creature.");
            respawnTime = Main.configToEdit.Bind("CREATURES", "Creature respawn time", "", "Number of days it takes a creature to respawn. The format is: creature ID, space, number of days it takes to respawn. By default fish and big creatures respawn in 12 hours, leviathans respawn after 1 day.");
            //spawnChance = Main.configB.Bind("", "Spawn chance", "", "Chance for a object ID to spawn. The format is: object ID, space, chance to spawn percent. Can be used only to reduce chances to spawn. Every entry is separated by comma.");
            propulsionCannonGrabFX = Main.configToEdit.Bind("TOOLS", "Propulsion cannon sphere effect", true, "Blue sphere visual effect you see when holding an object with propulsion cannon will be disabled if this is false.");
            fixCuteFish = Main.configToEdit.Bind("CREATURES", "Fix cuddlefish", false, "You will be able to interact with cuddlefish only when swimming if this is true.");
            seaTreaderOutcropMult = Main.configToEdit.Bind("CREATURES", "Outcrop from seatreader step", 100, "Chance percent to unearth outcrop when seatreader steps");
            seaTreaderAttackOutcropMult = Main.configToEdit.Bind("CREATURES", "Outcrop from seatreader attack", 100, "Chance percent to unearth outcrop when seatreader attacks player");
            shroomDamageChance = Main.configToEdit.Bind("PLANTS", "Mushroom damage chance percent", 0, new ConfigDescription("Chance of a mushroom dealing damage to player when picked up and dealing area damage when destroyed. The script to do it was always in the game but was disabled.", percentRange));
            escapePodPowerTweak = Main.configToEdit.Bind("LIFE POD", "Life pod power tweaks", false, "When your life pod is damaged its max power is reduced to 50%. When you crashland your life pod power cells are not charged.");
            stalkerPlayThings = Main.configToEdit.Bind("CREATURES", "Items stalkers can grab", "ScrapMetal, MapRoomCamera, Beacon, Seaglide, CyclopsDecoy, Gravsphere, SmallStorage, FireExtinguisher, DoubleTank, PlasteelTank, PrecursorKey_Blue, PrecursorKey_Orange, PrecursorKey_Purple, PrecursorKey_Red, PrecursorKey_White, Rebreather, Tank, HighCapacityTank, Flare, Flashlight, Builder, LaserCutter, LEDLight, DiveReel, PropulsionCannon, Knife, HeatBlade, Scanner, Welder, RepulsionCannon, StasisRifle", "List of item IDs separated by comma. Only items in this list can be grabbed by stalkers.");
            stalkersGrabShinyTool = Main.configToEdit.Bind("CREATURES", "Stalkers grab tools from player hands when playing", false, "Stalkers can grab only things that are in the ‛Items stalkers can grab‛ list.");
            dropHeldTool = Main.configToEdit.Bind("PLAYER", "Drop tool in your hands when taking damage", false, "Chance percent to drop your tool is equal to amount of damage taken.");
            newPoisonSystem = Main.configToEdit.Bind("PLAYER", "New poison damage system", false, "Every 2 seconds poison will deal 1 point of permanent damage and decrease your food and water values by 1. Using first aid kit will remove poison from your system.");
            freeTorpedos = Main.configToEdit.Bind("VEHICLES", "Free torpedos", 2, new ConfigDescription("Number of torpedos you get when installing Torpedo System or Prawn Suit Torpedo Arm. After changing this you have to craft a new Torpedo System.", freeTorpedosRange));
            lowOxygenWarning = Main.configToEdit.Bind("PLAYER", "Low oxygen onscreen warning", true);
            lowOxygenAudioWarning = Main.configToEdit.Bind("PLAYER", "Low oxygen audio warning", true);
            builderToolBuildsInsideWithoutPower = Main.configToEdit.Bind("TOOLS", "Builder tool does not need power when building inside", true);
            cameraBobbing = Main.configToEdit.Bind("PLAYER", "Screen bobbing when swimming", true);
            disableHints = Main.configToEdit.Bind("MISC", "Disable tutorial messages", true, "This disables messages that tell you to 'eat something', 'break limestone', etc.");
            cyclopsHUDalwaysOn = Main.configToEdit.Bind("CYCLOPS", "Cyclops HUD always on", false);
            cameraShake = Main.configToEdit.Bind("CYCLOPS", "Screen shakes when cyclops takes damage", true);
            removeDeadCreaturesOnLoad = Main.configToEdit.Bind("CREATURES", "Remove dead creatures when loading saved game", true, "");
            sunlightAffectsEscapePodLighting = Main.configToEdit.Bind("LIFE POD", "Sunlight affects lighting in your life pod", false, "");
            scannerFX = Main.configToEdit.Bind("TOOLS", "Wierd visual effect on objects being scanned", true, "");
            escapePodMedkitCabinetWorks = Main.configToEdit.Bind("LIFE POD", "Medical kit fabricator in your life pod produces first aid kit", EscapePodMedkitCabinetWorks.Always, "");
            dropItemsAnywhere = Main.configToEdit.Bind("PLAYER", "Player can drop inventory items anywhere", false, "This allows you to place placable items anywhere in the world, drop items anywhere except cyclops and grab items in your base with propulsion cannon.");
            //disableIonCubeFabricator = Main.configToEdit.Bind("", "Disable ion cube fabricator at the Primary containment facility", false);
            showTempFahrenhiet = Main.configToEdit.Bind("UI", "Show temperature in Fahrenhiet instead of Celcius", false, "");


            fixScreenResolution = Main.configToEdit.Bind("MISC", "Fix screen resolution", false, "The game sometimes resets screen resolution to desktop one. Set this to true to fix it.");
            removeCreditsButton = Main.configToEdit.Bind("MENU BUTTONS", "Remove credits button from main menu", false);
            removeRedeemButton = Main.configToEdit.Bind("MENU BUTTONS", "Remove redeem key button from options menu", false);
            removeTroubleshootButton = Main.configToEdit.Bind("MENU BUTTONS", "Remove troubleshooting button from options menu", false);
            removeUnstuckButton = Main.configToEdit.Bind("MENU BUTTONS", "Remove unstuck button from pause menu", false);
            removeFeedbackButton = Main.configToEdit.Bind("MENU BUTTONS", "Remove feedback button from pause menu", false);
            enableDevButton = Main.configToEdit.Bind("MENU BUTTONS", "Enable developer button in pause menu", false);
            propCannonGrabsAnyPlant = Main.configToEdit.Bind("TOOLS", "Propulsion cannon grabs any plant", true, "Propulsion cannon will grab only plants you can pick up if this is false");




            transferAllItemsButton = Main.configToEdit.Bind("BUTTON BIND", "Move all items button", Button.None, "Press this button to move all items from one container to another. This works only with controller. Use this if you can not bind a controller button in the mod menu.");
            transferSameItemsButton = Main.configToEdit.Bind("BUTTON BIND", "Move same items button", Button.None, "Press this button to move all items of the same type from one container to another. This works only with controller. Use this if you can not bind a controller button in the mod menu.");
            quickslotButton = Main.configToEdit.Bind("BUTTON BIND", "Quickslot cycle button", Button.None, "Press 'Cycle next' or 'Cycle previous' button while holding down this button to cycle tools in your current quickslot. This works only with controller. Use this if you can not bind a controller button in the mod menu.");
            lightButton = Main.configToEdit.Bind("BUTTON BIND", "Light intensity button", Button.None, "When holding a tool in your hand or driving a vehicle press 'Cycle next' or 'Cycle previous' button while holding down this button to change the tool's or vehicle's light intensity. This works only with controller. Use this if you can not bind a controller button in the mod menu.");


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
                    Main.logger.LogWarning("Could not parse: " + input);
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
                    Main.logger.LogWarning("Could not parse: " + input);
                    continue;
                }
                if (fl < 1)
                    continue;

                dic.Add(tt, fl);
            }
            return dic;
        }

        private static Dictionary<TechType, float> Parse01floatDicFromString(string input)
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
                    Main.logger.LogWarning("Could not parse: " + input);
                    continue;
                }
                if (fl == 0)
                    continue;

                fl = Mathf.Clamp(fl, -100f, 100f);
                dic.Add(tt, fl * .01f);
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

        private static Vector3 ParseBloodColor(string input)
        {
            float f = float.MaxValue;
            float f1 = float.MaxValue;
            float f2 = float.MaxValue;
            string[] entries = input.Split(' ');
            for (int i = 0; i < entries.Length; i++)
            {
                try
                {
                    if (f == float.MaxValue)
                        f = float.Parse(entries[i]);
                    else if (f1 == float.MaxValue)
                        f1 = float.Parse(entries[i]);
                    else if (f2 == float.MaxValue)
                        f2 = float.Parse(entries[i]);
                }
                catch (Exception)
                {
                    break;
                }
            }
            if (f == float.MaxValue || f1 == float.MaxValue || f2 == float.MaxValue)
            {
                Main.logger.LogWarning("Could not parse blood color: " + input);
                return new Vector3(0.784f, 1f, 0.157f);
            }
            return new Vector3(Mathf.Clamp01(f), Mathf.Clamp01(f1), Mathf.Clamp01(f2));
        }

        public static void ParseConfig()
        {
            Player_Movement.waterSpeedEquipment = Parse01floatDicFromString(waterSpeedEquipment.Value);
            Player_Movement.groundSpeedEquipment = Parse01floatDicFromString(groundSpeedEquipment.Value);
            Crush_Damage_.crushDepthEquipment = ParseIntDicFromString(crushDepthEquipment.Value);
            Crush_Damage_.crushDamageEquipment = ParseIntDicFromString(crushDamageEquipment.Value);
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

            Creature_Tweaks.bloodColor = ParseBloodColor(bloodColor.Value);
            Enum.TryParse(transferAllItemsButton.Value.ToString(), out Inventory_Patch.transferAllItemsButton);
            Enum.TryParse(transferSameItemsButton.Value.ToString(), out Inventory_Patch.transferSameItemsButton);
            Enum.TryParse(quickslotButton.Value.ToString(), out QuickSlots_Patch.quickslotButton);
            Enum.TryParse(lightButton.Value.ToString(), out Light_Control.lightButton);
            //Main.logger.LogDebug("transferAllItemsButton " + Inventory_Patch.transferAllItemsButton);
            //Main.logger.LogDebug("transferSameItemsButton " + Inventory_Patch.transferSameItemsButton);

            Player_Movement.CacheSettings();
        }

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
