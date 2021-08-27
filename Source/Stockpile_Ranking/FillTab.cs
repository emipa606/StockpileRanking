using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Stockpile_Ranking
{
    [HarmonyPatch(typeof(ITab_Storage), "FillTab")]
    internal static class FillTab
    {
        //protected override void FillTab()
        private static readonly MethodInfo GetTopAreaHeight =
            AccessTools.Property(typeof(ITab_Storage), "TopAreaHeight").GetGetMethod(true);

        //-----------------------------------------------
        //Here's the meat
        //-----------------------------------------------
        public static int curRank;
        public static PropertyInfo SelStoreInfo = AccessTools.Property(typeof(ITab_Storage), "SelStoreSettingsParent");

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            //		public static void BeginGroup(Rect position);
            var BeginGroupInfo = AccessTools.Method(typeof(GUI), nameof(GUI.BeginGroup), new[] { typeof(Rect) });

            //class Verse.ThingFilter RimWorld.StorageSettings::'filter'
            var filterInfo = AccessTools.Field(typeof(StorageSettings), "filter");
            var DoThingFilterConfigWindowInfo = AccessTools.Method(typeof(ThingFilterUI), "DoThingFilterConfigWindow");

            var firstTopAreaHeight = true;
            var instList = instructions.ToList();
            for (var i = 0; i < instList.Count; i++)
            {
                var inst = instList[i];

                if (inst.LoadsField(filterInfo) &&
                    instList[i + 8].Calls(DoThingFilterConfigWindowInfo))
                {
                    //instead of settings.filter, do RankComp.GetFilter(settings, curRank)
                    yield return new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(FillTab), "curRank"));
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(RankComp), "GetFilter"));
                }
                else
                {
                    yield return inst;
                }

                if (firstTopAreaHeight &&
                    inst.Calls(GetTopAreaHeight))
                {
                    firstTopAreaHeight = false;
                    yield return new CodeInstruction(OpCodes.Ldc_R4, TopAreaHeight.rankHeight);
                    yield return new CodeInstruction(OpCodes.Sub);
                }

                if (!inst.Calls(BeginGroupInfo))
                {
                    continue;
                }

                yield return new CodeInstruction(OpCodes.Ldarg_0); //ITab_Storage this
                yield return new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(FillTab), nameof(DrawRanking)));
            }
        }

        public static void DrawRanking(ITab_Storage tab)
        {
            if (SelStoreInfo.GetValue(tab, null) is not IHaulDestination haulDestination)
            {
                return;
            }

            var settings = haulDestination.GetStoreSettings();
            if (settings == null)
            {
                return;
            }

            var comp = RankComp.Get();
            var count = comp.CountExtraFilters(settings);
            if (curRank > count)
            {
                curRank = count;
            }

            var buttonMargin = TopAreaHeight.rankHeight + 4;

            //ITab_Storage.WinSize = 300
            var rect = new Rect(0f,
                (float)GetTopAreaHeight.Invoke(tab, new object[] { }) - TopAreaHeight.rankHeight - 2, 280,
                TopAreaHeight.rankHeight);

            //Left Arrow
            var leftButtonRect = rect.LeftPartPixels(TopAreaHeight.rankHeight);
            if (curRank > 0)
            {
                if (Widgets.ButtonImage(leftButtonRect, Tex.ArrowLeft))
                {
                    SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
                    curRank--;
                }
            }

            //Right Arrow
            var rightButtonRect = rect.RightPartPixels(TopAreaHeight.rankHeight);
            if (curRank == count)
            {
                if (Widgets.ButtonImage(rightButtonRect, Tex.Plus))
                {
                    SoundDefOf.Click.PlayOneShotOnCamera();
                    comp.AddFilter(settings);
                    curRank++;
                }
            }
            else
            {
                if (Widgets.ButtonImage(rightButtonRect, Tex.ArrowRight))
                {
                    SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
                    curRank++;
                }
            }

            //Delete rank button
            rightButtonRect.x -= buttonMargin;
            if (curRank > 0)
            {
                if (Widgets.ButtonImage(rightButtonRect, Tex.DeleteX))
                {
                    SoundDefOf.Crunch.PlayOneShotOnCamera();
                    comp.RemoveFilter(settings, curRank--);
                }
            }

            //Label
            rect.x += buttonMargin;
            rect.width -= buttonMargin * 3;
            Text.Font = GameFont.Small;
            Widgets.Label(rect, count == 0 ? "TD.AddFilter".Translate() : "TD.RankNum".Translate(curRank + 1));
        }
    }
}