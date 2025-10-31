using GenepackManipulation.Defs;
using GenepackManipulation.Types;
using RimWorld;
using System;
using Verse;

namespace GenepackManipulation.Manipulations
{
    internal static class ManipulationFactory
    {
        internal static GenepackManipulation Create(Building_GeneAssembler assembler, ManipulationDef def)
        {
            Type manipulationType = ManipulationTypeCache.Get(def.className);

            if (manipulationType == null || !typeof(GenepackManipulation).IsAssignableFrom(manipulationType))
            {
                Log.Error($"[GenepackManipulation] Could not find manipulation class of correct type '{def.className}' does it inherit type 'GenepackManipulation'?");
                return null;
            }
            var manipulationInstance = Activator.CreateInstance(manipulationType, assembler, def) as GenepackManipulation;

            return manipulationInstance;
        }
    }
}
