using HarmonyLib;
using RimWorld;

namespace Stockpile_Ranking
{
    [HarmonyPatch(typeof(StorageSettings), "TryNotifyChanged")]
    internal class TryNotifyChanged
    {
        //private void TryNotifyChanged()
        public static void Prefix(StorageSettings __instance)
        {
            RankComp.Get()?.CascadeDown(__instance);
        }
    }
}