using HarmonyLib;
using MelonLoader;

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

        //[HarmonyPatch(typeof(WenklyStudio.ElvenAssassin.DragonAttackControler), "KillPlayer", new Type[] { typeof(WenklyStudio.ElvenAssassin.PlayerController) })]
        //public class DragonKillPlayer
        //{
        //    [HarmonyPostfix]
        //    public static void Postfix(WenklyStudio.ElvenAssassin.PlayerController playerToBeKilled)
        //    {
        //        if (!owoSkin.suitEnabled) return;

        //        if (playerToBeKilled != PlayersManager.Instance.LocalPlayer) return;
        //        owoSkin.Feel("Flame Thrower", 3);
        //    }
        //}

    }
}
