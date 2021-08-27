using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Stockpile_Ranking
{
    [HarmonyPatch(typeof(StorageSettings), "ExposeData")]
    internal static class ExposeData
    {
        public static void Prefix(StorageSettings __instance, ref Action __state)
        {
            __state = RankComp.settingsChangedCallbackInfo.GetValue(__instance.filter) as Action;
        }

        public static void Postfix(StorageSettings __instance, Action __state)
        {
            //BUG FIX TIME
            //public void ExposeData() in StorageSettings would re-assign the filter, meaning the action passed to its ctor was lost
            //so workaround, save it before ExposeData and re-assign it
            RankComp.settingsChangedCallbackInfo.SetValue(__instance.filter, __state);

            //Save/load the ranked filters in a list
            var comp = RankComp.Get();
            switch (Scribe.mode)
            {
                case LoadSaveMode.Saving:
                {
                    var ranks = comp?.GetRanks(__instance, false);
                    if (ranks == null)
                    {
                        return;
                    }

                    Scribe_Collections.Look(ref ranks, "ranks", LookMode.Deep);
                    break;
                }
                case LoadSaveMode.LoadingVars:
                {
                    List<ThingFilter> loadRanks = null;
                    Scribe_Collections.Look(ref loadRanks, "ranks", LookMode.Deep);
                    comp?.SetRanks(__instance, loadRanks);
                    break;
                }
            }
        }
    }
}