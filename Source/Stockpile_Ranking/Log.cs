using System.Diagnostics;

namespace Stockpile_Ranking
{
    internal static class Log
    {
        [Conditional("DEBUG")]
        public static void Message(string x)
        {
            Verse.Log.Message(x);
        }
    }
}