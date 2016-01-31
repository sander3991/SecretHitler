using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretHitler.Logic
{
    static class ShuffleCollectionExtension
    {
        public static void Shuffle<T>(this IList<T> list, int amountOfShuffles = 200)
        {
            int length = list.Count;
            Random rand = new Random(Environment.TickCount * 13 / 7);
            for (var i = 0; i < amountOfShuffles; i++)
                SwapIndexes(list, rand.Next(length), rand.Next(length));
        }
        private static void SwapIndexes<T>(IList<T> list, int indexA, int indexB)
        {
            if (indexA == indexB) return;
            T temp = list[indexA];
            list[indexA] = list[indexB];
            list[indexB] = temp;
        }
    }
}
