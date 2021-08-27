using System.Collections.Generic;
using HarmonyLib;
using RimWorld;

namespace Stockpile_Ranking
{
    [HarmonyPatch(typeof(ListerHaulables), "CheckAdd")]
    internal class DirtyHaulablesAdd
    {
        //private void CheckAdd(Thing t)
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return DirtyHaulables.Transpiler(instructions);
        }
    }
}