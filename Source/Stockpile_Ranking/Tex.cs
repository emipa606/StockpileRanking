using UnityEngine;
using Verse;

namespace Stockpile_Ranking
{
    [StaticConstructorOnStartup]
    internal static class Tex
    {
        public static readonly Texture2D Plus = ContentFinder<Texture2D>.Get("UI/Buttons/Plus");
        public static readonly Texture2D DeleteX = ContentFinder<Texture2D>.Get("UI/Buttons/Delete");
        public static readonly Texture2D ArrowLeft = ContentFinder<Texture2D>.Get("ArrowLeftColor");
        public static readonly Texture2D ArrowRight = ContentFinder<Texture2D>.Get("ArrowRightColor");
    }
}