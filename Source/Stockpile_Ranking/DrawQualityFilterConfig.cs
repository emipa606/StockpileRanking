using System;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Stockpile_Ranking
{
    [HarmonyPatch(typeof(ThingFilterUI), "DrawQualityFilterConfig")]
    internal static class DrawQualityFilterConfig
    {
        //private static void DrawQualityFilterConfig(ref float y, float width, ThingFilter filter)
        public static void Prefix(ThingFilter filter, ref QualityRange __state)
        {
            __state = filter.AllowedQualityLevels;
        }

        public static void Postfix(ThingFilter filter, QualityRange __state)
        {
            if (__state != filter.AllowedQualityLevels)
            {
                if (RankComp.settingsChangedCallbackInfo.GetValue(filter) is Action a)
                {
                    a();
                }
            }
        }
    }
}