using HarmonyLib;
using UnityEngine;


namespace Tweaks_Fixes
{
    class Crush_Damage
    {
        public static float crushPeriod = 3f;

        public static void CrushDamage()
        {
            if (!Player.main.gameObject.activeInHierarchy)
                return;

            float depth = Ocean.main.GetDepthOf(Player.main.gameObject);
            float crushDepth = Main.config.crushDepth;
            if (depth < crushDepth || !Player.main.IsSwimming())
                return;

            float damage = (depth - crushDepth) * Main.config.crushDamageMult;
            if (!Player.main.liveMixin)
                return;
            //ErrorMessage.AddDebug(" CrushDamageUpdate " + damage);
            Player.main.liveMixin.TakeDamage(damage, Utils.GetRandomPosInView(), DamageType.Pressure);
        }

        [HarmonyPatch(typeof(Player), "Update")]
        class Player_Update_Patch
        {
            private static float crushTime = 0f;
            static void Postfix(Player __instance)
            {
                //Main.Message("Depth Class " + __instance.GetDepthClass());
                if (Main.config.crushDamageMult > 0f && Time.time - crushTime > crushPeriod)
                {
                    crushTime = Time.time;
                    CrushDamage();
                }
            }
        }

        [HarmonyPatch(typeof(CrushDamage), "CrushDamageUpdate")]
        class DamageSystem_CalculateDamage_Patch
        { // player does not have this
            public static bool Prefix(CrushDamage __instance)
            {
                if (Main.config.vehicleCrushDamageMult == 0f)
                    return true;

                if (!__instance.gameObject.activeInHierarchy || !__instance.enabled || !__instance.GetCanTakeCrushDamage())
                    return false;

                float depth = __instance.depthCache.Get();
                if (depth < __instance.crushDepth)
                    return false;

                float damage = (depth - __instance.crushDepth) * Main.config.vehicleCrushDamageMult;
                //ErrorMessage.AddDebug("damage " + damage);
                __instance.liveMixin.TakeDamage(damage, __instance.transform.position, DamageType.Pressure);
                if (__instance.soundOnDamage)
                    __instance.soundOnDamage.Play();

                return false;
            }
        }



    }
}
