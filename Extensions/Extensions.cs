using System;
using System.Collections.Generic;

namespace TresetaApp.Extensions
{
    public static class Extensions
    {
        private static Random rng = new Random();

        public static List<T> GetAndRemove<T>(this List<T> list, int start, int end)
        {
            lock (list)
            {
                List<T> values = list.GetRange(start, end);
                list.RemoveRange(start, end);
                return values;
            }
        }
        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }

}