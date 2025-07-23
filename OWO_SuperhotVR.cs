using HarmonyLib;
using MelonLoader;
using Photon.Pun;
using System;
using UnityEngine;
using WenklyStudio.ElvenAssassin;

[assembly: MelonInfo(typeof(OWO_ElvenAssassin.OWO_SuperhotVR), "OWO_SuperHotVR", "1.0.0", "OWOGame")]
[assembly: MelonGame("SUPERHOT Team", "SUPERHOT VR")]


namespace OWO_ElvenAssassin
{
    public class OWO_SuperhotVR : MelonMod
    {
        public static OWOSkin owoSkin;
        public static bool isRightHanded = true;
        public static string itemInHand = "";

        public override void OnInitializeMelon()
        {
            owoSkin = new OWOSkin();
        }

        #region Shoot bow
        [HarmonyPatch(typeof(HandsDominanceSwitcher), "InitializeWithPlayer", new Type[] { typeof(bool) })]
        public class HandsDominance
        {
            [HarmonyPostfix]
            public static void Postfix(HandsDominanceSwitcher __instance, bool isLocalPlayer)
            {
                if (!isLocalPlayer) return;
                if (__instance.HandsDominance == HandsDominanceSwitcher.HandsDominanceType.Left) isRightHanded = false;
            }
        }

        [HarmonyPatch(typeof(WenklyStudio.BowController), "UpdateBowTensionValue", new Type[] { })]
        public class UpdateBowTensionValue
        {
            [HarmonyPostfix]
            public static void Postfix(WenklyStudio.BowController __instance)
            {
                if (!owoSkin.suitEnabled || __instance.photonView.Owner != PhotonNetwork.LocalPlayer) return;

                if (__instance.BowAnimationNormalizedTime >= 0.3 )
                {
                    owoSkin.stringBowIntensity = Mathf.FloorToInt(Mathf.Clamp(__instance.BowAnimationNormalizedTime * 100.0f ,40, 100));
                    owoSkin.StartStringBow(isRightHanded);
                }
                else if (owoSkin.stringBowIsActive)
                {
                    owoSkin.StopStringBow();
                }

            }
        }

        [HarmonyPatch(typeof(WenklyStudio.BowController), "Shoot", new Type[] { })]
        public class ShootBow
        {
            [HarmonyPostfix]
            public static void Postfix(WenklyStudio.BowController __instance)
            {
                //if (!__instance.IsHandAttached) return;
                if (!owoSkin.suitEnabled) return;

                PlayerController playerController = Traverse.Create(__instance).Field("playerController").GetValue<PlayerController>();
                if (playerController.photonView.Owner == PhotonNetwork.LocalPlayer)
                {
                    owoSkin.StopStringBow();
                    owoSkin.FeelWithHand("Bow Release", 2, isRightHanded);
                    itemInHand = "";
                }
            }
        }
        #endregion

        #region Get hit
        [HarmonyPatch(typeof(WenklyStudio.ElvenAssassin.DragonAttackControler), "KillPlayer", new Type[] { typeof(WenklyStudio.ElvenAssassin.PlayerController) })]
        public class DragonKillPlayer
        {
            [HarmonyPostfix]
            public static void Postfix(WenklyStudio.ElvenAssassin.PlayerController playerToBeKilled)
            {
                if (!owoSkin.suitEnabled) return;

                if (playerToBeKilled != PlayersManager.Instance.LocalPlayer) return;
                owoSkin.Feel("Flame Thrower", 3);
            }
        }

        [HarmonyPatch(typeof(WenklyStudio.ElvenAssassin.DeathMatchKillsController), "KillPlayer", new Type[] { typeof(PlayerControllerCore), typeof(PlayerControllerCore) })]
        public class PlayerKillPlayer
        {
            [HarmonyPostfix]
            public static void Postfix(PlayerControllerCore victim)
            {
                if (!owoSkin.suitEnabled) return;

                if (victim != PlayersManager.Instance.LocalPlayer) return;

                owoSkin.Feel("Impact", 3);
            }
        }

        [HarmonyPatch(typeof(WenklyStudio.ElvenAssassin.TrollAttackController), "AnimationEventKillPlayer", new Type[] { })]
        public class TrollKillPlayer
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                if (!owoSkin.suitEnabled) return;

                owoSkin.Feel("Impact", 3);
            }
        }

        [HarmonyPatch(typeof(WenklyStudio.ElvenAssassin.AxeController), "RpcPlayPlayerFleshSound", new Type[] { })]
        public class AxeHitPlayer
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                if (!owoSkin.suitEnabled) return;

                owoSkin.LOG("AxeHitPlayer");
                owoSkin.Feel("Impact", 3);
            }
        }
        #endregion

        [HarmonyPatch(typeof(WenklyStudio.ElvenAssassin.TrollAttackController), "Shout")]
        public class TrollShout
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                if (!owoSkin.suitEnabled) return;

                owoSkin.Feel("Belly Rumble", 1);
            }
        }

        #region RPG MODE
        [HarmonyPatch(typeof(GateController), "DamageGate")]
        public class DamageGate
        {
            static bool gateDestroyed = false;

            [HarmonyPrefix]
            public static void Prefix(GateController __instance)
            {
                gateDestroyed = Traverse.Create(__instance).Field("gateAlreadyDestroyed").GetValue<bool>();
            }

            [HarmonyPostfix]
            public static void Postfix(GateController __instance)
            {
                if (!owoSkin.suitEnabled) return;


                if (__instance.EnemiesThatCanEnter == __instance.MaxEnemiesThatCanEnter)
                {
                    gateDestroyed = false;
                }


                if (!gateDestroyed)
                {
                    owoSkin.Feel("Gate Damage", 2);

                    //If remaining life less than 5 start heartbeat
                    if (__instance.EnemiesThatCanEnter <= __instance.MaxEnemiesThatCanEnter / 4)
                    {
                        owoSkin.StartHeartBeat();
                    }

                    //If dead stop heatbeat
                    if (__instance.EnemiesThatCanEnter == 0)
                    {
                        gateDestroyed = true;
                        owoSkin.DeathAction();
                    }
                }
            }

            
        }

        [HarmonyPatch(typeof(CartController), "CallOnCartDiedEvent")]
        public class CallOnCartDiedEvent
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                if (!owoSkin.suitEnabled) return;

                owoSkin.DeathAction();
            }
        }
        #endregion

        #region Interactable

        [HarmonyPatch(typeof(TeleportController), "TeleportLocalPlayer")]
        public class TeleportLocalPlayer
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                if (!owoSkin.suitEnabled) return;

                owoSkin.Feel("Teleport", 2);
            }
        }

        [HarmonyPatch(typeof(CannonController), "FireCannon")]
        public class FireCannon
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                if (!owoSkin.suitEnabled) return;

                if (itemInHand != "Torch Handle") return;
                owoSkin.Feel("Fire Cannon", 2);
            }
        }

        [HarmonyPatch(typeof(CatapultController), "ThrowRock")]
        public class ThrowRock
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                if (!owoSkin.suitEnabled) return;

                if (itemInHand != "HandleOutline") return;
                owoSkin.Feel("Fire Catapult", 2);
            }
        }

        [HarmonyPatch(typeof(BalistaShootController), "Shoot")]
        public class Shoot
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                if (!owoSkin.suitEnabled) return;

                if (itemInHand != "balista_bone_VerticalTurn") return;
                owoSkin.Feel("Fire Balista", 2);
            }
        }


        [HarmonyPatch(typeof(HandPickUpController), "PickUpItem")]
        public class PickUpItem
        {
            [HarmonyPostfix]
            public static void Postfix(HandPickUpController __instance)
            {
                if (!owoSkin.suitEnabled) return;

                string holded = Traverse.Create(__instance).Field("pickableInteracterGrabbed").GetValue<PickableInteracter>().name;
                itemInHand = holded;
            }
        }
        #endregion
    }
}
