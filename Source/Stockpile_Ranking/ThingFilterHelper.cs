using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace Stockpile_Ranking
{
    internal static class ThingFilterHelper
    {
        public static FieldInfo specials = AccessTools.Field(typeof(ThingFilter), "disallowedSpecialFilters");

        public static void Add(this ThingFilter filter, ThingFilter other)
        {
            foreach (var def in other.AllowedThingDefs)
            {
                filter.SetAllow(def, true);
            }

            var disallowedSpecialFilters = (List<SpecialThingFilterDef>)specials.GetValue(other);
            foreach (var specDef in disallowedSpecialFilters)
            {
                if (other.Allows(specDef))
                {
                    filter.SetAllow(specDef, true);
                }
            }

            var q = filter.AllowedQualityLevels;
            var qO = other.AllowedQualityLevels;
            q.max = q.max > qO.max ? q.max : qO.max;
            q.min = q.min < qO.min ? q.min : qO.min;
            filter.AllowedQualityLevels = q;

            var hp = filter.AllowedHitPointsPercents;
            var hpO = other.AllowedHitPointsPercents;
            hp.max = Math.Max(hp.max, hpO.max);
            hp.min = Math.Min(hp.min, hpO.min);
            filter.AllowedHitPointsPercents = hp;
        }
    }
}