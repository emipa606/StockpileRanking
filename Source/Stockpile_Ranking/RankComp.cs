using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Stockpile_Ranking
{
    public class RankComp : GameComponent
    {
        public static FieldInfo settingsChangedCallbackInfo =
            AccessTools.Field(typeof(ThingFilter), "settingsChangedCallback");

        public static MethodInfo TryNotifyChangedInfo = AccessTools.Method(typeof(StorageSettings), "TryNotifyChanged");
        public bool dirty;

        public Dictionary<StorageSettings, List<ThingFilter>> rankedSettings =
            new Dictionary<StorageSettings, List<ThingFilter>>();

        public Dictionary<StorageSettings, ThingFilter> usedFilter = new Dictionary<StorageSettings, ThingFilter>();

        public RankComp(Game game)
        {
        }

        public static RankComp Get()
        {
            return Current.Game?.GetComponent<RankComp>();
        }

        public override void GameComponentTick()
        {
            base.GameComponentTick();

            if (!dirty)
            {
                return;
            }

            DetermineUsedFilters();
            dirty = false;
        }

        public override void LoadedGame()
        {
            base.LoadedGame();

            //pawns tick before game component so this needs to be set up first... probably.
            DetermineUsedFilters();
        }

        public void DetermineUsedFilters()
        {
            usedFilter.Clear();

            foreach (var kvp in rankedSettings)
            {
                DetermineUsedFilter(kvp.Key, kvp.Value);
            }
        }

        public void DetermineUsedFilter(StorageSettings settings, List<ThingFilter> ranks)
        {
            usedFilter.Remove(settings);
            if (ranks == null)
            {
                return;
            }

            //Find map
            var map = (settings.owner as IHaulDestination)?.Map;

            if (map == null)
            {
                return;
            }

            Log.Message($"DetermineUsedFilter for {settings.owner}");

            //First filter is just the one in the settings
            var bestFilter = settings.filter;

            //Find haulables that are in lower priority storage
            //Don't check if they are valid for that storage, since that would call filter.Allows() but wouldn't check lower-ranked filters
            //listerHaulables isn't perfect since things in good storage aren't listed
            //listerHaulables doesn't know if a higher-rank filter would apply, because we use that list to determine if the higher-rank applies to begin with...
            //It's a circular dependency
            //listerHaulables will get refilled when an item is missing from the ranked storage, 
            //so then things are all haulable to higher priority and put in listerHaulables 
            //and then DetermineUsedFilter finds which is best and sets the filter 
            //so then listerHaulables removes things that fit the higher-rank filter

            List<Thing> haulables;
            if (Settings.Get().returnLower)
            {
                haulables = map.listerThings.ThingsInGroup(ThingRequestGroup.HaulableAlways).FindAll(t =>
                    !t.IsForbidden(Faction.OfPlayer) &&
                    (StoreUtility.CurrentHaulDestinationOf(t)?.GetStoreSettings().Priority ??
                     StoragePriority.Unstored) < settings.Priority &&
                    ranks.Last().Allows(t));
            }
            else
            {
                haulables = map.listerHaulables.ThingsPotentiallyNeedingHauling().FindAll(t =>
                    (StoreUtility.CurrentHaulDestinationOf(t)?.GetStoreSettings().Priority ??
                     StoragePriority.Unstored) < settings.Priority &&
                    ranks.Last().Allows(t));
            }

            Log.Message($"haulables are {haulables.ToStringSafeEnumerable()}");
            //Loop but don't include last filter
            for (var i = 0; i < ranks.Count; i++)
            {
                Log.Message($"does rank {i + 1} work?");
                //something matches this filter? Then that's the one to use
                var filter = bestFilter;
                if (haulables.Any(t => filter.Allows(t)))
                {
                    Log.Message("(yes)");
                    usedFilter[settings] = bestFilter;
                    return;
                }

                Log.Message($"trying rank {i + 2}");
                bestFilter = ranks[i];
            }

            //If all other filters had nothing available, the last filter is used:
            usedFilter[settings] = bestFilter;
        }

        //This one is called from ilcode where it'd be tricky to get RankComp in front of arg list
        public static ThingFilter UsedFilter(StorageSettings settings)
        {
            var comp = Get();
            if (comp != null && comp.usedFilter.TryGetValue(settings, out var used))
            {
                return used;
            }

            return settings.filter;
        }

        public List<ThingFilter> GetRanks(StorageSettings settings, bool create = true)
        {
            var dict = rankedSettings;
            if (dict.TryGetValue(settings, out var list))
            {
                return list;
            }

            if (!create)
            {
                return null;
            }

            var newList = new List<ThingFilter>();
            dict[settings] = newList;
            return newList;
        }

        public bool HasRanks(StorageSettings settings)
        {
            return rankedSettings.ContainsKey(settings);
        }

        public int CountExtraFilters(StorageSettings settings)
        {
            return GetRanks(settings, false)?.Count ?? 0;
        }

        public ThingFilter GetLowestFilter(StorageSettings settings)
        {
            return GetRanks(settings, false)?.Last() ?? settings.filter;
        }

        public void CascadeDown(StorageSettings settings)
        {
            Log.Message($"Cascade down {settings.owner}");
            var ranks = GetRanks(settings, false);
            if (ranks == null)
            {
                return;
            }

            var higher = settings.filter;
            foreach (var lower in ranks)
            {
                lower.Add(higher);
                higher = lower;
            }
        }

        public static Action SettingsChangedAction(StorageSettings settings)
        {
            return settingsChangedCallbackInfo.GetValue(settings.filter) as Action;
        }

        public void AddFilter(StorageSettings settings, ThingFilter filter = null)
        {
            if (filter == null)
            {
                filter = GetLowestFilter(settings);
            }

            var newFilter = new ThingFilter(SettingsChangedAction(settings));
            newFilter.CopyAllowancesFrom(filter);
            GetRanks(settings).Add(newFilter);
        }

        public void SetRanks(StorageSettings settings, List<ThingFilter> newRanks)
        {
            if (newRanks == null)
            {
                rankedSettings.Remove(settings);
            }
            else
            {
                rankedSettings[settings] = newRanks;
                foreach (var filter in newRanks)
                {
                    settingsChangedCallbackInfo.SetValue(filter, SettingsChangedAction(settings));
                }
            }
        }

        public static void CopyFrom(StorageSettings settings, StorageSettings other)
        {
            var comp = Get();
            if (comp == null) //fixed storage settings will copy do this copy on game load
            {
                return;
            }

            var otherRanks = comp.GetRanks(other, false);
            if (otherRanks == null)
            {
                comp.rankedSettings.Remove(settings);
            }
            else
            {
                comp.GetRanks(settings).Clear();
                foreach (var otherFilter in otherRanks)
                {
                    comp.AddFilter(settings, otherFilter);
                }
            }

            comp.DetermineUsedFilter(settings, comp.GetRanks(settings, false));
        }

        //This one is called from ilcode where it'd be tricky to get RankComp in front of arg list
        public static ThingFilter GetFilter(StorageSettings settings, int rank)
        {
            var comp = Get();
            return comp == null || rank == 0 ? settings.filter : comp.GetRanks(settings)[rank - 1];
        }

        public void RemoveFilter(StorageSettings settings, int rank)
        {
            if (rank == 0)
            {
                return; //sanity check
            }

            var ranks = GetRanks(settings);
            if (ranks.Count == 1)
            {
                rankedSettings.Remove(settings);
            }
            else
            {
                ranks.RemoveAt(rank - 1);
            }

            TryNotifyChangedInfo.Invoke(settings, null);
            DetermineUsedFilter(settings, GetRanks(settings, false));
        }
    }
}