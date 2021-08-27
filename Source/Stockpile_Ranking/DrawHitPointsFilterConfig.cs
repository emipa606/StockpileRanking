using System;
using HarmonyLib;
using Verse;

namespace Stockpile_Ranking
{
    //Fleshing out vanilla functions: changing Quality and HP filter calls notify method
    [HarmonyPatch(typeof(ThingFilterUI), "DrawHitPointsFilterConfig")]
    internal static class DrawHitPointsFilterConfig
    {
        //private static void DrawHitPointsFilterConfig(ref float y, float width, ThingFilter filter)
        public static void Prefix(ThingFilter filter, ref FloatRange __state)
        {
            __state = filter.AllowedHitPointsPercents;
        }

        public static void Postfix(ThingFilter filter, FloatRange __state)
        {
            if (__state != filter.AllowedHitPointsPercents)
            {
                if (RankComp.settingsChangedCallbackInfo.GetValue(filter) is Action a)
                {
                    a();
                }
            }
        }
    }
}