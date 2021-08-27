using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Stockpile_Ranking
{
    [HarmonyPatch(typeof(StorageSettings), "AllowedToAccept", typeof(Thing))]
    internal class AllowedToAccept_Thing
    {
        //public bool AllowedToAccept(Thing t)
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var filterInfo = AccessTools.Field(typeof(StorageSettings), "filter");

            foreach (var i in instructions)
            {
                //instead of this.filter.Allows(t)
                //RankComp.UsedFilter(this).Allows(t)
                //so the ilcodes are this, filter, t, Allows
                // replace filter with UsedFilter
                if (i.LoadsField(filterInfo))
                {
                    yield return new CodeInstruction(OpCodes.Call,
                        AccessTools.Method(typeof(RankComp), nameof(RankComp.UsedFilter)));
                }
                else
                {
                    yield return i;
                }
            }
        }
    }
}