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
        public class KillPlayer
        {
            [HarmonyPostfix]
            public static void Postfix(Vector3 killerObjectPosition, bool switchToBlack = false, bool hardKill = false, bool forced = false)
            {
                if (!owoSkin.suitEnabled) return;

                owoSkin.Feel("Death", 3);
            }
        }

        [HarmonyPatch(typeof(VrPickingSystem), "PickupItem", new Type[]
{
        typeof(VrHandController),
        typeof(PickupProxy),
        typeof(GrabTypes)
})]
        public static class PickUpItem
        {
            [HarmonyPostfix]
            public static void PostFix(VrPickingSystem __instance, VrHandController handController, PickupProxy pickup, GrabTypes grabType = GrabTypes.Grip)
            {
                HandType hand = GetHandFromControllerString(handController.CurrentController.ToString());

                switch (hand)
                {
                    case HandType.Empty_LeftHand:
                    case HandType.LeftHand:
                        owoSkin.FeelWithHand("Grab", false);
                        break;
                    case HandType.Empty_RightHand:
                    case HandType.RightHand:
                        owoSkin.FeelWithHand("Grab", true);
                        break;
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
            #endregion

        }
    }
}