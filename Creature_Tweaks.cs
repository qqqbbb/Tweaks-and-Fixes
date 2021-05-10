using HarmonyLib;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    class Creature_Tweaks
    {
        [HarmonyPatch(typeof(CreatureEgg), "Awake")]
        class CreatureEgg_Awake_Patch
        {
            public static void Postfix(CreatureEgg __instance)
            {
                __instance.explodeOnHatch = false;
            }
        }

        [HarmonyPatch(typeof(FleeOnDamage), "OnTakeDamage")]
        class FleeOnDamage_OnTakeDamage_Postfix_Patch
        {
            public static void Postfix(FleeOnDamage __instance, DamageInfo damageInfo)
            { // game code makes them always flee to 0 0 0 
                __instance.moveTo = __instance.swimBehaviour.originalTargetPosition * damageInfo.damage;
                //__instance.timeToFlee = Time.time;
                //if (damageInfo.type == DamageType.Heat)
                //{
                //TechType techType = CraftData.GetTechType(__instance.gameObject);
                //string name = Language.main.Get(techType);
                //float magnitude = (__instance.transform.position - Player.main.transform.position).magnitude;
                //if (damageInfo.damage == 0 && magnitude < 5)
                //{
                //    LiveMixin liveMixin = __instance.creature.liveMixin;
                //AddDebug(name + " maxHealth " + liveMixin.maxHealth + " Health " + liveMixin.health);
                //}

                //Main.Message(name + " originalTargetPosition " + __instance.swimBehaviour.originalTargetPosition);
                //Main.Message(name + " moveTo " + __instance.moveTo);

                //}
                CollectShiny collectShiny = __instance.GetComponent<CollectShiny>();
                if (collectShiny)
                {
                    //Main.Message("Stalker DropShinyTarget");
                    collectShiny.DropShinyTarget();
                }
            }
        }

        [HarmonyPatch(typeof(FleeOnDamage), nameof(FleeOnDamage.OnTakeDamage))]
        internal class FleeOnDamage_OnTakeDamage_Prefix_Patch
        {
            static System.Random rndm = new System.Random();

            private static bool Prefix(FleeOnDamage __instance, DamageInfo damageInfo)
            {
                //int health = (int)__instance.creature.liveMixin?.health; 
                //Main.Message("creaturesDontFlee " + Main.config.creaturesDontFlee);
                if (Main.config.predatorsDontFlee)
                {
                    LiveMixin liveMixin = __instance.creature.liveMixin;
                    AggressiveWhenSeeTarget agr = __instance.GetComponent<AggressiveWhenSeeTarget>();
                    if (liveMixin && agr)
                    { //  && damageInfo.dealer == Player.main
                      //if (damageInfo.dealer)
                      //  Main.Message("damage dealer " + damageInfo.dealer.name);
                      //int maxHealth = (int)liveMixin.maxHealth;
                        int halfMaxHealth = Mathf.RoundToInt(liveMixin.maxHealth * .5f);
                        int rnd = rndm.Next(1, halfMaxHealth);

                        if (liveMixin.health > halfMaxHealth || rnd < liveMixin.health)
                        {
                            damageInfo.damage = 0f;
                            //_Message
                            //Main.Message("Dont flee");
                            //Main.Message("health " + liveMixin.health + " rnd100 " + rnd100);
                        }
                        return false;
                    }
                }
                return true;
            }
        }

        //[HarmonyPatch(typeof(FMOD_CustomEmitter), nameof(FMOD_CustomEmitter.Play))]
        public static class ReaperLeviathan_FMOD_CustomEmitter_Patch
        {
            public static bool Prefix(FMOD_CustomEmitter __instance)
            {
                if (Main.config.disableReaperRoar && __instance.gameObject.GetComponent<ReaperLeviathan>())
                {
                    //AddDebug("FMOD_CustomEmitter Play ");
                    //if (__instance.asset)
                    //    AddDebug("asset " + __instance.asset.id);
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(Creature), nameof(Creature.Start))]
        public static class Creature_Start_Patch
        {
            public static void Postfix(Creature __instance)
            {
                if (__instance is Spadefish)
                {
                    //AddDebug("Spadefish");
                    __instance.GetComponent<Rigidbody>().mass = 4f;
                }
                else if (Main.config.disableReaperRoar && __instance is ReaperLeviathan)
                {
                    FMOD_CustomEmitter[] ces = __instance.GetAllComponentsInChildren<FMOD_CustomEmitter>();
                    foreach (FMOD_CustomEmitter ce in ces)
                    {
                        ce.enabled = false;
                    }
                }
            }
        }


        [HarmonyPatch(typeof(CreatureDeath), nameof(CreatureDeath.OnKill))]
        class CreatureDeath_OnKill_Prefix_Patch
        {
            public static void Prefix(CreatureDeath __instance)
            {
                if (Main.config.creaturesRespawn)
                {
                    __instance.respawnOnlyIfKilledByCreature = false;
                }
            }
        }

        [HarmonyPatch(typeof(CreatureDeath), nameof(CreatureDeath.OnKill))]
        class CreatureDeath_OnKill_Patch
        {
            public static void Postfix(CreatureDeath __instance)
            {
                //AddDebug("respawnOnlyIfKilledByCreature " + __instance.respawnOnlyIfKilledByCreature);
                Stalker stalker = __instance.GetComponent<Stalker>();
                //ReaperLeviathan reaper = __instance.GetComponent<ReaperLeviathan>();
                //SandShark sandShark = __instance.GetComponent<SandShark>();
                //if (sandShark)
                //{
                    //Animator animator = __instance.GetComponentInChildren<Animator>();
                    //animator.GetCurrentAnimatorStateInfo(animator.layerCount -1);
                    //if (animator != null)
                    //    animator.enabled = false;
                //}
                //if (reaper != null)
                //{
                    //Animator animator = __instance.GetComponentInChildren<Animator>();
                    //if (animator != null)
                    //    animator.enabled = false;
                //}
                if (stalker != null)
                {
                    //Main.Log("Stalker kill");
                    Animator animator = __instance.GetComponentInChildren<Animator>();
                    //AnimateByVelocity animByVelocity = __instance.GetComponentInChildren<AnimateByVelocity>();
                    if (animator != null)
                    {
                        animator.enabled = false;
                        //animator.enabled = true;
                        //animator.SetFloat(AnimateByVelocity.animSpeed, 0.0f);
                        //animator.SetFloat(AnimateByVelocity.animPitch, 0.0f);
                        //animator.SetFloat(AnimateByVelocity.animTilt, 0.0f);
                        //SafeAnimator.SetBool(animator, "dead", true);
                    } 
                    CollectShiny collectShiny = __instance.GetComponent<CollectShiny>();
                    collectShiny?.DropShinyTarget();
                }
            }
        }

        [HarmonyPatch(typeof(SeaTreaderSounds), nameof(SeaTreaderSounds.OnStep))]
        class SeaTreaderSounds_OnStep_patch
        { // seatreader spawns chunks only when stomping
            public static bool Prefix(SeaTreaderSounds __instance, Transform legTr, AnimationEvent animationEvent)
            {
                if (!Main.config.seaTreaderChunks)
                    return true;

                if (animationEvent.animatorClipInfo.clip == __instance.walkinAnimClip && !__instance.treader.IsWalking())
                    return false;
                if (__instance.stepEffect != null)
                    Utils.SpawnPrefabAt(__instance.stepEffect, null, legTr.position);
                if (__instance.stepSound != null)
                    Utils.PlayEnvSound(__instance.stepSound, legTr.position);
                return false;
            }
        }

        //[HarmonyPatch(typeof(FleeOnDamage), "Evaluate")]
        class FleeOnDamage_Evaluate_Prefix_Patch
        {
            public static bool Prefix(FleeOnDamage __instance, Creature creature)
            {
                //__instance.GetEvaluatePriority();
                __instance.StartPerform(creature);
                //Main.Message(" FleeOnDamage_Evaluate_Prefix_Patch ");
                return false;

            }
        }

        //[HarmonyPatch(typeof(FleeOnDamage), "Evaluate")]
        class FleeOnDamage_Evaluate_Patch
        {
            public static void Postfix(FleeOnDamage __instance, Creature creature)
            {
                TechType techType = CraftData.GetTechType(__instance.gameObject);
                string name = Language.main.Get(techType);
                //Creature_Loot_Drop.Creature_Loot Crloot = __instance.GetComponent<Creature_Loot_Drop.Creature_Loot>();
                //if (Crloot)
                //{
                //Main.Message("Stalker DropShinyTarget");
                //if (Time.time < __instance.timeToFlee)
                //{
                //Main.Message(name + " FleeOnDamage GetEvaluatePriority " + __instance.GetEvaluatePriority());
                //}
                //else
                //    Main.Message(name + " FleeOnDamage Evaluate " + 0);
                //}
            }
        }

        //[HarmonyPatch(typeof(FleeOnDamage), "StopPerform")]
        class FleeOnDamage_StopPerform_Patch
        {
            public static void Postfix(FleeOnDamage __instance, Creature creature)
            {

                TechType techType = CraftData.GetTechType(__instance.gameObject);
                string name = Language.main.Get(techType);
                //Main.Message(name + " Stop Perform ");
            }
        }
    }
}
