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
        public static string itemInHand = "";

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

    }
}
