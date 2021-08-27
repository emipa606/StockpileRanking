using UnityEngine;
using Verse;

namespace Stockpile_Ranking
{
    internal class Settings : ModSettings
    {
        public bool returnLower;

        public static Settings Get()
        {
            return LoadedModManager.GetMod<Mod>().GetSettings<Settings>();
        }

        public void DoWindowContents(Rect wrect)
        {
            var options = new Listing_Standard();
            options.Begin(wrect);

            var old = returnLower;
            options.CheckboxLabeled("TD.SettingReturn".Translate(), ref returnLower);
            if (old != returnLower)
            {
                if (RankComp.Get() is { } comp)
                {
                    comp.dirty = true;
                }
            }

            options.Label("TD.SettingDescCPU".Translate());
            options.Label("TD.SettingDescLowerStorage".Translate());
            options.Label("TD.SettingDescDefault".Translate());
            options.Gap();
            options.Label("TD.SettingDesc".Translate());
            options.Gap();

            options.End();
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref returnLower, "returnLower", true);
        }
    }
}