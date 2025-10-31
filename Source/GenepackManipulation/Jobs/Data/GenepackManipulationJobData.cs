using RimWorld;
using System.Collections.Generic;
using Verse;

namespace GenepackManipulation.Jobs.Data
{
    public class GenepackManipulationJobData : IExposable
    {
        public GenepackManipulationJobData() { }

        internal Genepack Genepack;
        internal Manipulations.GenepackManipulation Manipulation;
        internal int TicksElapsed = 0;
        internal int TicksRequired = 0;
        internal List<ThingDefCountClass> RequiredIngredients;

        public void ExposeData()
        {
            Scribe_References.Look(ref Genepack, "Genepack");
            Scribe_Deep.Look(ref Manipulation, "Manipulation");
            Scribe_Values.Look(ref TicksElapsed, "TicksElapsed");
            Scribe_Values.Look(ref TicksRequired, "TicksRequired");
            Scribe_Collections.Look(ref RequiredIngredients, "RequiredIngredients", LookMode.Deep);
        }
    }
}
