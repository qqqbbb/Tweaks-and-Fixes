using BepInEx.Configuration;
using Nautilus.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static ErrorMessage;


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
        //public static ConfigEntry<Button> transferAllItemsButton;
        //public static ConfigEntry<Button> transferSameItemsButton;
        //public static ConfigEntry<Button> quickslotButton;
        //public static ConfigEntry<Button> lightButton;
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
        public static ConfigEntry<bool> vehicleUItweaks;
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
        public static ConfigEntry<EscapePodMedicalCabinetWorks> escapePodMedkitCabinetWorks;
        public static ConfigEntry<bool> dropItemsAnywhere;
        public static ConfigEntry<bool> showTempFahrenhiet;
        public static ConfigEntry<bool> fixScreenResolution;
        public static ConfigEntry<bool> vehiclesHurtCreatures;
        public static ConfigEntry<string> notRechargableBatteries;
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
        public static ConfigEntry<int> cyclopsVerticalSpeedMod;
        public static ConfigEntry<int> cyclopsBackwardSpeedMod;
        public static ConfigEntry<bool> alwaysSpawnWhenKnifeHarvesting;
        public static ConfigEntry<bool> cyclopsSonar;
        public static ConfigEntry<bool> cyclopsFireMusic;
        public static ConfigEntry<bool> playerBreathBubbles;
        public static ConfigEntry<bool> playerBreathBubblesSoundFX;
        public static ConfigEntry<bool> medkitFabAlertSound;
        public static ConfigEntry<bool> consistentHungerUpdateTime;
        public static ConfigEntry<bool> removeBigParticlesWhenKnifing;
        public static ConfigEntry<int> permPoisonDamage;
        public static ConfigEntry<int> poisonFoodDamage;
        public static ConfigEntry<bool> propulsionCannonTweaks;
        public static ConfigEntry<bool> beaconTweaks;
        public static ConfigEntry<bool> flareTweaks;
        public static ConfigEntry<bool> stasisRifleTweaks;
        public static ConfigEntry<bool> disableWeirdPlantAnimation;
        public static ConfigEntry<bool> coralShellPlateGivesTableCoral;
        public static ConfigEntry<bool> disableTimeCapsule;
        public static ConfigEntry<bool> spawnResourcesWhenDrilling;
        public static ConfigEntry<bool> canPickUpContainerWithItems;

        public static ConfigEntry<float> spotlightLightIntensityMult;
        public static ConfigEntry<float> exosuitLightIntensityMult;
        public static ConfigEntry<float> seamothLightIntensityMult;
        public static ConfigEntry<float> cyclopsLightIntensityMult;
        public static ConfigEntry<float> seaglideLightIntensityMult;
        public static ConfigEntry<float> flareLightIntensityMult;
        public static ConfigEntry<float> laserCutterLightIntensityMult;
        public static ConfigEntry<float> cameraLightIntensityMult;
        public static ConfigEntry<float> flashlightLightIntensityMult;
        private static ConfigEntry<string> cameraLightColor;
        private static ConfigEntry<string> seaglideLightColor;
        private static ConfigEntry<string> seamothLightColor;
        private static ConfigEntry<string> exosuitLightColor;
        private static ConfigEntry<string> cyclopsLightColor;
        private static ConfigEntry<string> flashlightLightColor;
        private static ConfigEntry<string> flareLightColor;
        private static ConfigEntry<string> spotlightLightColor;
        public static ConfigEntry<string> damageModifiers;
        public static ConfigEntry<bool> craftedPowercellInheritsBatteryCharge;
        public static ConfigEntry<float> filtrationMachineWaterTimeMult;
        public static ConfigEntry<float> filtrationMachineSaltTimeMult;
        public static ConfigEntry<float> batteryChargeSpeedMult;
        public static ConfigEntry<float> fruitGrowTime;


        public static ConfigEntry<bool> disableIonCubeFabricator;


        public static AcceptableValueRange<float> medKitHPperSecondRange = new AcceptableValueRange<float>(0.001f, 100f);
        public static AcceptableValueRange<float> lightIntensityRange = new AcceptableValueRange<float>(0.1f, 1f);
        public static AcceptableValueRange<int> percentRange = new AcceptableValueRange<int>(0, 100);
        public static AcceptableValueRange<int> freeTorpedosRange = new AcceptableValueRange<int>(0, 6);

        public static void Bind()
        {  // “ ” ‛


            seaglideWorksOnlyForward = Main.configToEdit.Bind("PLAYER MOVEMENT", "Seaglide works only when moving forward", false, "");
            heatBladeCooks = Main.configToEdit.Bind("TOOLS", "Thermoblade cooks fish on kill", true);
            alwaysSpawnWhenKnifeHarvesting = Main.configToEdit.Bind("TOOLS", "Always spawn things you harvest with knife instead of adding them to inventory", false);

            dontSpawnKnownFragments = Main.configToEdit.Bind("FRAGMENTS", "Do not spawn fragments for unlocked blueprints", false);
            cantScanExosuitClawArm = Main.configToEdit.Bind("FRAGMENTS", "Unlock prawn suit only by scanning prawn suit", false, "In vanilla game prawn suit can be unlocked by scanning 20 prawn suit arm fragments.");
            mapRoomFreeCameras = Main.configToEdit.Bind("BASE", "Free camera drones for scanner room", true, "Scanner room will be built without camera drones if this is false.");
            decoyRequiresSub = Main.configToEdit.Bind("ITEMS", "Creature decoy does not work when dropped from inventory", false);
            noKillParticles = Main.configToEdit.Bind("CREATURES", "No yellow cloud particle effect when creature dies", false);
            cyclopsSunlight = Main.configToEdit.Bind("CYCLOPS", "Sunlight affects lighting in cyclops", false);
            alwaysShowHealthFoodNunbers = Main.configToEdit.Bind("UI", "Always show numbers for health, food and temperature meters in UI", false);
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
            waterSpeedEquipment = Main.configToEdit.Bind("EQUIPMENT", "Water speed equipment", "", "Equipment in this list affects your movement speed in water. The format is: item ID, space, percent that will be added to your movement speed. Negative numbers will reduce your movement speed. Every entry is separated by comma.");

            crushDepthEquipment = Main.configToEdit.Bind("EQUIPMENT", "Crush depth equipment", "ReinforcedDiveSuit 0", "Equipment in this list increases your safe diving depth. The format is: item ID, space, number of meters that will be added to your safe diving depth. Every entry is separated by comma.");
            damageModifiers = Main.configToEdit.Bind("CREATURES", "Damage modifiers", "", "Use this to modify damage taken by things. Negative numbers reduce damage. Positive numbers increase damage. The format is: ID, space, damage percent that will be added or subtracted. Every entry is separated by comma.");
            crushDamageEquipment = Main.configToEdit.Bind("EQUIPMENT", "Crush damage equipment", "ReinforcedDiveSuit 0", "Equipment in this list reduces your crush damage. The format is: item ID, space, crush damage percent that will be blocked. Every entry is separated by comma.");
            itemMass = Main.configToEdit.Bind("ITEMS", "Item mass", "PrecursorKey_Blue 5, PrecursorKey_Orange 5, PrecursorKey_Purple 5, PrecursorKey_Red 5, PrecursorKey_White 5", "This allows you to change mass of pickupable items. The format is: item ID, space, item mass in kg as a decimal point number. Every entry is separated by comma.");
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
            vehicleUItweaks = Main.configToEdit.Bind("UI", "Vehicle UI tweaks", true, "UI Prompts for toggling lights in seamoth and prawn suit. UI Prompts for installed seamoth upgrade modules. UI Prompts for installed prawn suit arms. Ability to change current torpedo for seamoth and prawn suit.");
            newStorageUI = Main.configToEdit.Bind("UI", "New storage UI", true, "New UI for storage containers");

            disableUseText = Main.configToEdit.Bind("UI", "Disable quickslots text", false, "Text above your quickslots will be disabled if this is true.");

            notRechargableBatteries = Main.configToEdit.Bind("ITEMS", "Not rechargable batteries", "", "Comma separated list of battery IDs. Batteries from this list can not be recharged");
            craftWithoutBattery = Main.configToEdit.Bind("TOOLS", "Craft without battery", false, "Your newly crafted tools and vehicles will not have batteries in them if this is true.");
            disableCyclopsProximitySensor = Main.configToEdit.Bind("CYCLOPS", "Disable cyclops proximity sensor", false);
            builderPlacingWhenFinishedBuilding = Main.configToEdit.Bind("TOOLS", "Builder tool placing mode when finished building", true, "Your builder tool will exit placing mode when you finish building if this is false .");
            crushDamageScreenEffect = Main.configToEdit.Bind("PLAYER", "Crush damage screen effect", true, "There will be no screen effects when player takes crush damage if this is false.");
            removeCookedFishOnReload = Main.configToEdit.Bind("CREATURES", "Remove cooked fish when loading saved game", false, "Cooked fish will be removed from the world (not from containers) when loading saved game if this is true.");
            disableGravityForExosuit = Main.configToEdit.Bind("VEHICLES", "Disable gravity for prawn suit", false, "Prawn suit will ignore gravity when you are not piloting it if this is true. Use this if your prawn suit falls through the ground.");
            //replaceDealDamageOnImpactScript = Main.configToEdit.Bind("VEHICLES", "Replace DealDamageOnImpact script", false, "The game will use vanilla script when vehicles collide with objects if this is false.");
            //cyclopsDealDamageMinSpeed = Main.configToEdit.Bind("CYCLOPS", "Cyclops min speed to deal damage", 2f, "Min speed in meters per second at which cyclops deals damage when colliding with objects. Works only if ‛Replace DealDamageOnImpact script‛ setting is true.");
            //cyclopsTakeDamageMinSpeed = Main.configToEdit.Bind("CYCLOPS", "Cyclops min speed to take damage", 2f, "Min speed in meters per second at which cyclops takes damage when colliding with objects. Works only if ‛Replace DealDamageOnImpact script‛ setting is true.");
            //cyclopsTakeDamageMinMass = Main.configToEdit.Bind("CYCLOPS", "Min mass that can damage cyclops", 200f, "Min mass in kg for objects that can damage cyclops when colliding with it. Works only if ‛Replace DealDamageOnImpact script‛ setting is true.");
            //exosuitDealDamageMinSpeed = Main.configToEdit.Bind("VEHICLES", "Prawn suit min speed to deal damage", 7f, "Min speed in meters per second at which prawn suit deals damage when colliding with objects. Works only if ‛Replace DealDamageOnImpact script‛ setting is true.");
            //exosuitTakeDamageMinSpeed = Main.configToEdit.Bind("VEHICLES", "Prawn suit min speed to take damage", 7f, "Min speed in meters per second at which prawn suit takes damage when colliding with objects. Works only if ‛Replace DealDamageOnImpact script‛ setting is true.");
            //exosuitTakeDamageMinMass = Main.configToEdit.Bind("VEHICLES", "Min mass that can damage prawn suit", 5f, "Min mass in kg for objects that can damage prawn suit when colliding with it. Works only if ‛Replace DealDamageOnImpact script‛ setting is true.");
            //seamothDealDamageMinSpeed = Main.configToEdit.Bind("VEHICLES", "Seamoth min speed to deal damage", 7f, "Min speed in meters per second at which seamoth deals damage when colliding with objects. Works only if ‛Replace DealDamageOnImpact script‛ setting is true.");
            //seamothTakeDamageMinSpeed = Main.configToEdit.Bind("VEHICLES", "Seamoth min speed to take damage", 7f, "Min speed in meters per second at which seamoth takes damage when colliding with objects. Works only if ‛Replace DealDamageOnImpact script‛ setting is true.");
            //seamothTakeDamageMinMass = Main.configToEdit.Bind("VEHICLES", "Min mass that can damage seamoth", 5f, "Min mass in kg for objects that can damage seamoth when colliding with it. Works only if ‛Replace DealDamageOnImpact script‛ setting is true.");
            vehiclesHurtCreatures = Main.configToEdit.Bind("VEHICLES", "Vehicles hurt creatures", true, "Vehicles will not hurt creatures when colliding with them if this is false.");
            fixSeamothMove = Main.configToEdit.Bind("VEHICLES", "Fix seamoth movement", true, "Seamoth will not exceed its max speed when moving diagonally. It will use analog values from controller stick.");
            seamothSidewardSpeedMod = Main.configToEdit.Bind("VEHICLES", "Seamoth sideward speed modifier", 0, "Seamoth speed will be reduced by this percent when moving sideward.");
            seamothBackwardSpeedMod = Main.configToEdit.Bind("VEHICLES", "Seamoth backward speed modifier", 0, "Seamoth speed will be reduced by this percent when moving backward.");
            seamothVerticalSpeedMod = Main.configToEdit.Bind("VEHICLES", "Seamoth vertical speed modifier", 0, "Seamoth speed will be reduced by this percent when moving up or down.");
            disableExosuitSidestep = Main.configToEdit.Bind("VEHICLES", "Disable prawn suit sidestep", false, "");
            exosuitThrusterWithoutLimit = Main.configToEdit.Bind("VEHICLES", "Prawn suit thrusters never overheat", false, "No time limit when using thrusters, but they consume twice more power than walking");

            cyclopsVerticalSpeedMod = Main.configToEdit.Bind("CYCLOPS", "Cyclops vertical speed modifier", 0, "Cyclops speed will be reduced by this percent when moving up or down.");
            cyclopsBackwardSpeedMod = Main.configToEdit.Bind("CYCLOPS", "Cyclops backward speed modifier", 0, "Cyclops speed will be reduced by this percent when moving backward.");
            //fixCyclopsMove = Main.configToEdit.Bind("CYCLOPS", "Fix cyclops diagonal movement", true, "Cyclops will not exceed its max speed and will not consume more power when moving diagonally.");
            cyclopsFireMusic = Main.configToEdit.Bind("CYCLOPS", "Play dubsteppy music when cyclops engine is on fire", true);



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
            //newPoisonSystem = Main.configToEdit.Bind("PLAYER", "New poison damage system", false, "Every 2 seconds poison will deal 1 point of permanent damage and decrease your food and water values by 1. Using first aid kit will remove poison from your system.");
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
            escapePodMedkitCabinetWorks = Main.configToEdit.Bind("LIFE POD", "Medical kit fabricator in your life pod produces first aid kit", EscapePodMedicalCabinetWorks.Always, "");
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
            cyclopsSonar = Main.configToEdit.Bind("CYCLOPS", "Cyclops sonar", true, "Cyclops sonar that detects aggresive creatures will be off if this is false");
            playerBreathBubbles = Main.configToEdit.Bind("PLAYER", "Player breath bubbles particle effect", true);
            playerBreathBubblesSoundFX = Main.configToEdit.Bind("PLAYER", "Player breath bubbles sound effect", true);
            medkitFabAlertSound = Main.configToEdit.Bind("BASE", "Medical kit fabricator alert sound when first aid kit is ready", true);
            consistentHungerUpdateTime = Main.configToEdit.Bind("PLAYER", "Consistent hunger update time", false, "In vanilla game your hunger updates every 10 real time seconds. If this is true, hunger update interval will be divided by 'time flow speed multiplier' from the mod options.");
            //consistantHungerUpdateTime.SettingChanged += ConsistantHungerUpdateTimeChanged;
            removeBigParticlesWhenKnifing = Main.configToEdit.Bind("CREATURES", "Remove big particles when slashing creatures with knife", false, "You will see less blood particles when slashing creatures with knife if this is true.");
            permPoisonDamage = Main.configToEdit.Bind("PLAYER", "Permanent poison damage percent", 0, new ConfigDescription("If this is more than 0 you will take not temporary but permanent health damage when poisoned. For example if this is 90, you will lose 0.9 health permanantly for every point of poison damage.", percentRange));
            poisonFoodDamage = Main.configToEdit.Bind("PLAYER", "Poison food damage percent", 0, new ConfigDescription("If this is more than 0 you will lose food or water instead of taking temporary health damage when poisoned. For example if this is 90, you will lose 0.9 food or water for every point of poison damage.", percentRange));
            propulsionCannonTweaks = Main.configToEdit.Bind("TOOLS", "Propulsion cannon tweaks", true, "Improvements to propulsion cannon UI prompts. Ability ot eat fish you are holding with propulsion cannon. When grabbing and holding table coral with propulsion cannon, you can put it in inventory. Your propulsion cannon will break outcrop when you try to grab it. Propulsion cannon can grab fruits on plants ");
            beaconTweaks = Main.configToEdit.Bind("TOOLS", "Beacon tweaks", true, "You do not have to aim for certain part of a beacon to rename it. You can rename a beacon while holding it in your hands.");
            flareTweaks = Main.configToEdit.Bind("TOOLS", "Flare tweaks", true, "Tooltip for flare will tell you if it is burnt out. When you look at a dropped flare, you see if it is burnt out. You can light flare and not throw it. This setting will be disabled if 'Flare repair' mod is installed.");
            stasisRifleTweaks = Main.configToEdit.Bind("TOOLS", "Stasis rifle tweaks", true, "UI prompt when stasis rifle is equipped. Gasopods do not drop gas pods when in stasis field. Gas pods do not explode when in stasis field.");
            coralShellPlateGivesTableCoral = Main.configToEdit.Bind("PLANTS", "Coral Shell Plate gives Table Coral Sample", false, "When you destroy Coral Shell Plate you will get Table Coral Sample instead of Coral Tube Sample");
            disableWeirdPlantAnimation = Main.configToEdit.Bind("PLANTS", "Disable weird plant animation", false, "Disable animation for grub basket, bulbo tree, speckled rattler, pink cap, ming plant");
            disableTimeCapsule = Main.configToEdit.Bind("MISC", "Disable time capsules", false, "");
            spawnResourcesWhenDrilling = Main.configToEdit.Bind("VEHICLES", "Spawn resources instead of adding them to prawn suit container when drilling", false, "");
            canPickUpContainerWithItems = Main.configToEdit.Bind("MISC", "Can pick up containers with items", false, "");

            exosuitLightIntensityMult = Main.configToEdit.Bind("VEHICLES", "Prawn suit light intensity multiplier", 1f, new ConfigDescription("", lightIntensityRange));
            seamothLightIntensityMult = Main.configToEdit.Bind("VEHICLES", "Seamoth light intensity multiplier", 1f, new ConfigDescription("", lightIntensityRange));
            cyclopsLightIntensityMult = Main.configToEdit.Bind("CYCLOPS", "Cyclops light intensity multiplier", 1f, new ConfigDescription("", lightIntensityRange));
            seaglideLightIntensityMult = Main.configToEdit.Bind("TOOLS", "Seaglide light intensity multiplier", 1f, new ConfigDescription("", lightIntensityRange));
            flareLightIntensityMult = Main.configToEdit.Bind("TOOLS", "Flare light intensity multiplier", 1f, new ConfigDescription("", lightIntensityRange));
            laserCutterLightIntensityMult = Main.configToEdit.Bind("TOOLS", "Laser cutter light intensity multiplier", 1f, new ConfigDescription("", lightIntensityRange));
            cameraLightIntensityMult = Main.configToEdit.Bind("TOOLS", "Camera drone light intensity multiplier", 1f, new ConfigDescription("", lightIntensityRange));
            flashlightLightIntensityMult = Main.configToEdit.Bind("TOOLS", "Flashlight light intensity multiplier", 1f, new ConfigDescription("", lightIntensityRange));
            spotlightLightIntensityMult = Main.configToEdit.Bind("BASE", "Spotlight light intensity multiplier", 1f, new ConfigDescription("", lightIntensityRange));


            cameraLightColor = Main.configToEdit.Bind("TOOLS", "Camera drone light color", "0.463 0.902 0.902", "Camera drone light color will be set to this. Each value is a decimal point number from 0 to 1. First number is red. Second number is green. Third number is blue.");
            seaglideLightColor = Main.configToEdit.Bind("TOOLS", "Seaglide light color", "0.016 1 1", "Seaglide light color will be set to this. Each value is a decimal point number from 0 to 1. First number is red. Second number is green. Third number is blue.");
            seamothLightColor = Main.configToEdit.Bind("VEHICLES", "SeaMoth light color", "0.463 0.902 0.902", "SeaMoth light color will be set to this. Each value is a decimal point number from 0 to 1. First number is red. Second number is green. Third number is blue.");
            exosuitLightColor = Main.configToEdit.Bind("VEHICLES", "Prawn suit light color", "0.463 0.902 0.902", "Prawn suit light color will be set to this. Each value is a decimal point number from 0 to 1. First number is red. Second number is green. Third number is blue.");
            cyclopsLightColor = Main.configToEdit.Bind("CYCLOPS", "Cyclops light color", "1 1 1", "Cyclops light color will be set to this. Each value is a decimal point number from 0 to 1. First number is red. Second number is green. Third number is blue.");
            flashlightLightColor = Main.configToEdit.Bind("TOOLS", "Flashlight light color", "1 1 1", "Flashlight light color will be set to this. Each value is a decimal point number from 0 to 1. First number is red. Second number is green. Third number is blue.");
            flareLightColor = Main.configToEdit.Bind("TOOLS", "Flare light color", "0.706 0.448 0.431", "Flare light color will be set to this. Each value is a decimal point number from 0 to 1. First number is red. Second number is green. Third number is blue.");
            spotlightLightColor = Main.configToEdit.Bind("BASE", "Spotlight light color", "0.779 0.890 1", "Spotlight light color will be set to this. Each value is a decimal point number from 0 to 1. First number is red. Second number is green. Third number is blue.");
            craftedPowercellInheritsBatteryCharge = Main.configToEdit.Bind("ITEMS", "Crafted powercell inherits charge from batteries", false, "");
            filtrationMachineWaterTimeMult = Main.configToEdit.Bind("BASE", "Filtration machine water time multiplier", 1f, "Time it takes filtration machine to produce water will be multiplied by this");
            filtrationMachineSaltTimeMult = Main.configToEdit.Bind("BASE", "Filtration machine salt time multiplier", 1f, "Time it takes filtration machine to produce salt will be multiplied by this");
            batteryChargeSpeedMult = Main.configToEdit.Bind("BASE", "Battery charging rate multiplier", 1f, "Charging rate of battery charger and power cell charger will be multiplied by this. The faster they charge batteries the more power they drain.");
            fruitGrowTime = Main.configToEdit.Bind("PLANTS", "Fruit growth time", 0f, "Time in days it takes a lantern tree fruit, creepvine seeds, blood oil to grow. Default values will be used if this is 0: fruits from wild plants will never grow back after ypu pick them, fruits from your plants will grow in less than a day.");
        }


        private static Dictionary<TechType, int> ParseIntDicFromString(string input)
        {
            if (string.IsNullOrEmpty(input))
                return null;

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

        private static Dictionary<TechType, float> ParseFloatDicFromPercentString(string input)
        {
            if (string.IsNullOrEmpty(input))
                return null;

            //Main.logger.LogMessage("ParseFloatDicFromPercentString " + input);
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

                //Main.logger.LogMessage("ParseFloatDicFromPercentString TechType " + tt);
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
                //Main.logger.LogMessage("ParseFloatDicFromPercentString amount " + a);
                if (a == 0)
                    continue;
                else if (a == -100)
                {
                    dic.Add(tt, 0);
                    continue;
                }
                float f = a * .01f + 1f;
                if (f < 0 || Mathf.Approximately(f, 0f))
                    f = 0;

                //Main.logger.LogMessage("ParseFloatDicFromPercentString amount ! " + f);
                dic.Add(tt, f);
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

                set.Add(tt);
                //Main.logger.LogDebug("ParseSetFromString " + tt );
            }
            return set;
        }

        private static Color ParseColor(string input)
        {
            float r = float.MaxValue;
            float g = float.MaxValue;
            float b = float.MaxValue;
            string[] entries = input.Split(' ');
            for (int i = 0; i < entries.Length; i++)
            {
                try
                {
                    if (r == float.MaxValue)
                        r = float.Parse(entries[i]);
                    else if (g == float.MaxValue)
                        g = float.Parse(entries[i]);
                    else if (b == float.MaxValue)
                        b = float.Parse(entries[i]);
                }
                catch (Exception)
                {
                    break;
                }
            }
            if (r == float.MaxValue || g == float.MaxValue || b == float.MaxValue)
            {
                Main.logger.LogWarning("Could not parse color: " + input);
                return default;
            }
            return new Color(Mathf.Clamp01(r), Mathf.Clamp01(g), Mathf.Clamp01(b));
        }

        public static void ParseConfig()
        {
            Player_Movement.waterSpeedEquipment = Parse01floatDicFromString(waterSpeedEquipment.Value);
            Player_Movement.groundSpeedEquipment = Parse01floatDicFromString(groundSpeedEquipment.Value);
            Player_Movement.CacheSettings();
            Crush_Damage_.crushDepthEquipment = ParseIntDicFromString(crushDepthEquipment.Value);
            Crush_Damage_.crushDamageEquipment = ParseIntDicFromString(crushDamageEquipment.Value);
            Pickupable_.itemMass = ParseFloatDicFromString(itemMass.Value);
            Pickupable_.unmovableItems = ParseSetFromString(unmovableItems.Value);
            Gravsphere_Patch.gravTrappable = ParseSetFromString(gravTrappable.Value);
            Silent_Creatures.silentCreatures = ParseSetFromString(silentCreatures.Value);
            Pickupable_.shinies = ParseSetFromString(stalkerPlayThings.Value);
            LargeWorldEntity_Patch.eatableFoodValue = ParseIntDicFromString(eatableFoodValue.Value);
            LargeWorldEntity_Patch.eatableWaterValue = ParseIntDicFromString(eatableWaterValue.Value);
            Escape_Pod_Patch.newGameLoot = ParseIntDicFromString(newGameLoot.Value);
            CreatureDeath_.notRespawningCreatures = ParseSetFromString(notRespawningCreatures.Value);
            CreatureDeath_.notRespawningCreaturesIfKilledByPlayer = ParseSetFromString(notRespawningCreaturesIfKilledByPlayer.Value);
            CreatureDeath_.respawnTime = ParseIntDicFromString(respawnTime.Value);
            //LargeWorldEntity_Patch.techTypesToDespawn = ParseIntDicFromString(spawnChance.Value);
            Charger_.notRechargableBatteries = ParseSetFromString(notRechargableBatteries.Value);
            Creatures.bloodColor = ParseColor(bloodColor.Value);
            MapRoomCamera_.lightColor = ParseColor(cameraLightColor.Value);
            Seaglide_.lightColor = ParseColor(seaglideLightColor.Value);
            VehicleLightFix.seamothLightColor = ParseColor(seamothLightColor.Value);
            VehicleLightFix.exosuitLightColor = ParseColor(exosuitLightColor.Value);
            Cyclops_.cyclopsLightColor = ParseColor(cyclopsLightColor.Value);
            Tools.flashLightLightColor = ParseColor(flashlightLightColor.Value);
            Flare_.flareLightColor = ParseColor(flareLightColor.Value);
            BaseSpotLight_.lightColor = ParseColor(spotlightLightColor.Value);
            Damage_Patch.damageModifiers = ParseFloatDicFromPercentString(damageModifiers.Value);
            //Main.logger.LogMessage("ParseConfig done ");
        }

        public enum EscapePodMedicalCabinetWorks { Always, After_repairing_life_pod, Never }

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
