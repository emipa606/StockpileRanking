using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;

namespace Stockpile_Ranking
{
    [HarmonyPatch(typeof(StorageSettings), "CopyFrom")]
    internal class CopyFrom
    {
        //public void CopyFrom(StorageSettings other)

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var TryNotifyChangedInfo = AccessTools.Method(typeof(StorageSettings), "TryNotifyChanged");

            foreach (var i in instructions)
            {
                if (i.Calls(TryNotifyChangedInfo))
                {
                    //RankComp.CopyFrom(__instance, other);
                    yield return new CodeInstruction(OpCodes.Ldarg_0); //this
                    yield return new CodeInstruction(OpCodes.Ldarg_1); //other
                    yield return new CodeInstruction(OpCodes.Call,
                        AccessTools.Method(typeof(RankComp), nameof(RankComp.CopyFrom)));
                }

                yield return i;
            }
        }
    }
}