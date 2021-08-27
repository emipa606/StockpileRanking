using HarmonyLib;
using RimWorld;

namespace Stockpile_Ranking
{
    [HarmonyPatch(typeof(ITab_Storage), "TopAreaHeight", MethodType.Getter)]
    internal static class TopAreaHeight
    {
        public const float rankHeight = 24f;

        //private float TopAreaHeight
        public static void Postfix(ref float __result)
        {
            __result += rankHeight;
        }
    }

    //[HarmonyPatch(typeof(InspectTabBase), "OnOpen")]
    //static class ResetCurRank
    //{
    //	//public virtual void OnOpen()
    //	public static void Postfix(InspectTabBase __instance)
    //	{
    //		if (__instance is ITab_Storage)
    //			FillTab.curRank = 0;
    //	}
    //}
}