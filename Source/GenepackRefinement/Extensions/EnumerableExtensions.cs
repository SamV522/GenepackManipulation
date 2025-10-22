using System.Collections.Generic;
using System.Linq;
using Verse;

namespace GenepackRefinement.Extensions
{
    public static class EnumerableExtensions
    {
        public static (List<T> remainder, List<T> selected) Split<T>(this IEnumerable<T> source, int count, bool shuffle = false)
        {
            var workingList = shuffle ? source.OrderBy(x => Rand.Value).ToList() : source.ToList();

            var selected = workingList.Take(count).ToList();
            var remainder = workingList.Skip(count).ToList();
            return (remainder, selected);
        }
    }
}
