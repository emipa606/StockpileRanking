using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Stockpile_Ranking
{
    [HarmonyPatch(typeof(ListerHaulables), "Check")]
    internal class DirtyHaulables
    {
        //private void Check(Thing t)
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var AddInfo = AccessTools.Method(typeof(List<Thing>), "Add");
            var RemoveInfo = AccessTools.Method(typeof(List<Thing>), "Remove");

            foreach (var i in instructions)
            {
                yield return i;

                if (!i.Calls(AddInfo) && !i.Calls(RemoveInfo))
                {
                    continue;
                }

                yield return new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(RankComp), nameof(RankComp.Get))); //RankComp.Get()
                yield return new CodeInstruction(OpCodes.Ldc_I4_1); //true
                yield return new CodeInstruction(OpCodes.Stfld,
                    AccessTools.Field(typeof(RankComp), nameof(RankComp.dirty))); //RankComp.Get().dirty = true;
            }
        }
    }
}