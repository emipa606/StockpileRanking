using System.Collections.Generic;
using HarmonyLib;
using RimWorld;

namespace Stockpile_Ranking
{
    [HarmonyPatch(typeof(ListerHaulables), "TryRemove")]
    internal class DirtyHaulablesRemove
    {
        //private void TryRemove(Thing t)
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return DirtyHaulables.Transpiler(instructions);
        }
    }
}