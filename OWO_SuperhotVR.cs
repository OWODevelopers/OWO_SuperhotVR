using HarmonyLib;
using MelonLoader;

using System;

using UnityEngine;

[assembly: MelonInfo(typeof(OWO_SuperhotVR.OWO_SuperhotVR), "OWO_SuperHotVR", "1.0.0", "OWOGame")]
[assembly: MelonGame("SUPERHOT_Team", "SUPERHOT_VR")]


namespace OWO_SuperhotVR
{
    public class OWO_SuperhotVR : MelonMod
    {
        public static OWOSkin owoSkin;

        public override void OnInitializeMelon()
        {
            owoSkin = new OWOSkin();
        }

        [HarmonyPatch(typeof(PlayerActionsVR), "Kill", new Type[] { typeof(Vector3), typeof(bool), typeof(bool), typeof(bool) })]
        public class Patch_KillPlayer
        {
            [HarmonyPostfix]
            public static void Postfix(Vector3 killerObjectPosition, bool switchToBlack = false, bool hardKill = false, bool forced = false)
            {
                if (!owoSkin.suitEnabled) return;

                owoSkin.Feel("Death", Priority: 4);
            }
        }

        [HarmonyPatch(typeof(VrPickingSystem), "PickupItem", new Type[]
        {typeof(VrHandController),typeof(PickupProxy),typeof(GrabTypes)})]
        public class Patch_PickUpItem
        {
            [HarmonyPostfix]
            public static void PostFix(VrPickingSystem __instance, VrHandController handController, PickupProxy pickup, GrabTypes grabType = GrabTypes.Grip)
            {
                string sensationName = pickup.GameObject.name == "TeleportPyramid" ? "Grab Pyramid" : "Grab Object";

                HandType hand = GetHandFromControllerString(handController.CurrentController.ToString());
                switch (hand)
                {
                    case HandType.Empty_LeftHand:
                    case HandType.LeftHand:
                        owoSkin.FeelWithHand(sensationName, isRightHand: false, Priority: 2);
                        break;
                    case HandType.Empty_RightHand:
                    case HandType.RightHand:
                        owoSkin.FeelWithHand(sensationName, Priority: 2);
                        break;
                }
            }
        }

        [HarmonyPatch(typeof(VrHapticSystem), "SetVibration", new Type[]
        {typeof(Controller),typeof(string),typeof(float)})]
        public class Patch_SetVibration
        {
            [HarmonyPostfix]
            public static void PostFix(Controller controller, string preset, float multiplier)
            {
                if (!owoSkin.suitEnabled) return;

                HandType hand = GetHandFromControllerString(controller.ToString());
                switch (hand)
                {
                    case HandType.Empty_LeftHand:
                    case HandType.LeftHand:
                        if (preset == "Punch")
                            owoSkin.FeelWithHand("Punch Hit", false, Priority: 2);
                        break;
                    case HandType.Empty_RightHand:
                    case HandType.RightHand:
                        if (preset == "Punch")
                            owoSkin.FeelWithHand("Punch Hit", Priority: 2);
                        break;
                }
            }
        }

        [HarmonyPatch(typeof(Gun), "Fire", new System.Type[] { typeof(Ray), typeof(LayerMask), typeof(Gun) })]
        public class Patch_FireGun
        {
            [HarmonyPostfix]
            public static void PostFix(Ray ray, LayerMask mask, Gun weapon = null)
            {
                HandType hand = GetHandFromControllerString(weapon.gameObject.transform.parent.parent.parent.ToString());
                PickableItems guntype = GunType(weapon);
                owoSkin.FeelGunfire(guntype, hand);
            }
        }

        [HarmonyPatch(typeof(ShotGun), "Fire", new System.Type[] { typeof(Ray), typeof(LayerMask), typeof(ShotGun) })]
        public class Patch_FireShotgun
        {
            [HarmonyPostfix]
            public static void PostFix(Ray ray, LayerMask mask, Gun weapon = null)
            {
                HandType hand = GetHandFromControllerString(weapon.gameObject.transform.parent.parent.parent.ToString());
                PickableItems guntype = GunType(weapon);
                owoSkin.FeelGunfire(guntype, hand);
            }
        }

        [HarmonyPatch(typeof(UziGun), "ShootUziBullets")]
        public class Patch_FireUzi
        {
            [HarmonyPostfix]
            public static void PostFix(UziGun __instance)
            {
                HandType hand = GetHandFromControllerString(__instance.gameObject.transform.parent.parent.parent.ToString());
                owoSkin.FeelGunfire(PickableItems.Uzi, hand);
            }
        }

        [HarmonyPatch(typeof(Gun), "FireNoAmmo")]
        public class Patch_NoAmmo
        {
            [HarmonyPostfix]
            public static void PostFix(Gun __instance)
            {
                HandType hand = GetHandFromControllerString(__instance.gameObject.transform.parent.parent.parent.ToString());
                owoSkin.FeelGunfire(PickableItems.Gun_NoAmmo, hand);
            }
        }

        [HarmonyPatch(typeof(VrHandController), "ApplyHit", new System.Type[] { })]
        public class Patch_ParryBullet
        {
            [HarmonyPostfix]
            public static void PostFix(VrHandController __instance)
            {
                HandType hand = GetHandFromControllerString(__instance.CurrentController.ToString());
                owoSkin.FeelWithHand("Bullet Parry", hand == HandType.RightHand, Priority: 3);
            }
        }

        [HarmonyPatch(typeof(VrPickupDroppingSystem), "DropItem", new System.Type[] { typeof(VrHandController), typeof(float), typeof(float) })]
        public class Patch_DropItem
        {
            [HarmonyPostfix]
            public static void PostFix(
              VrPickupDroppingSystem __instance,
              VrHandController handController,
              float linearVelocity = 0.0f,
              float angularVelocity = 0.0f)
            {
                if ((double)linearVelocity >= 2.0)
                {
                    HandType hand = GetHandFromControllerString(handController.CurrentController.ToString());
                    owoSkin.FeelWithHand("Throw", hand == HandType.RightHand, Priority: 2);
                }
            }
        }

        [HarmonyPatch(typeof(MindDeathWaveSystem), "DualModeDeathWaveOnTarget", new System.Type[] { typeof(MindDeathWaveComponent), typeof(MindDeathWaveComponent) })]
        public class DualDeathWave
        {
            [HarmonyPrefix]
            public static void PreFix(
              MindDeathWaveComponent firstController,
              MindDeathWaveComponent secondController,
              MindDeathWaveSystem __instance)
            {
                if (__instance.dualDeathWaveActivated)
                    return;

                owoSkin.StartMindWaveCharge();
            }
        }

        [HarmonyPatch(typeof(MindDeathWaveSystem), "ResetDeathWave", new System.Type[] { typeof(MindDeathWaveComponent) })]
        public class ResetDualDeathWave
        {
            [HarmonyPrefix]
            public static void PreFix(
              MindDeathWaveComponent deathWaveController,
              MindDeathWaveSystem __instance)
            {
                if (!__instance.dualDeathWaveActivated)
                    return;

                owoSkin.StopMindWaveCharge();
            }
        }

        [HarmonyPatch(typeof(ScoreManager), "scorePoints", new System.Type[] { typeof(int), typeof(Vector3), typeof(ScoreManager.ScoreType), typeof(GunPickup), typeof(PejAiBody) })]
        public static class ExplodeHead
        {
            [HarmonyPrefix]
            public static void PreFix(
              int rewardedpoints,
              Vector3 particlePos,
              ScoreManager.ScoreType scoreType,
              GunPickup weapon,
              PejAiBody enemy)
            {
                if (scoreType != ScoreManager.ScoreType.deathwave)
                    return;
                
                owoSkin.MindWaveSkill();
            }
        }

        #region HELPERS
        private static HandType GetHandFromControllerString(string hand)
        {
            if (hand.Contains("Right"))
            {
                return HandType.RightHand;
            }

            if (hand.Contains("Left"))
            {
                return HandType.LeftHand;
            }

            owoSkin.LOG("HAND PARAMETER = " + hand + " DOESNT EXIST");
            return HandType.None;
        }

        public static PickableItems GunType(Gun gun)
        {
            if (gun.ammoCount <= 0)
                return PickableItems.Gun_NoAmmo;

            switch (gun)
            {
                case ShotGun _:
                    return PickableItems.Shotgun;
                case UziGun _:
                    return PickableItems.Uzi;
                case PistolGun _:
                    return PickableItems.Pistol;
                default:
                    return PickableItems.None;
            }
        }
        #endregion

    }
}