using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace GenepackManipulation.Types
{
    internal static class ManipulationTypeCache
    {
        private static readonly Dictionary<string, Type> _types;

        static ManipulationTypeCache()
        {
            Log.Message("[GenepackManipulation] Caching manipulation types...");
                        
            _types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => typeof(Manipulations.GenepackManipulation).IsAssignableFrom(t) && !t.IsAbstract)
                .ToDictionary(t => t.FullName, t => t);

            Log.Message($"[GenepackManipulation] Cached {_types.Count} manipulation types from {_types.Values.Select(t=> t.Assembly).Distinct().Count()} assemblies");

        }

        internal static Type Get(string className)
        {
            return _types.Where(k => k.Key == className).Select(k => k.Value).FirstOrDefault();
        }
    }
}
