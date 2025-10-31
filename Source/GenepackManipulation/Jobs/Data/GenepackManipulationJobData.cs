using RimWorld;
using System.Collections.Generic;
using Verse;

namespace GenepackManipulation.Jobs.Data
{
    public class GenepackManipulationJobData : IExposable
    {
        public GenepackManipulationJobData() { }

        public List<Genepack> Genepacks;
        public Manipulations.GenepackManipulation Manipulation;
        internal int TicksElapsed = 0;
        public int TicksRequired = 0;
        public List<ThingDefCountClass> RequiredIngredients;

        public void ExposeData()
        {
            Scribe_Deep.Look(ref Genepacks, "Genepack");
            Scribe_Deep.Look(ref Manipulation, "Manipulation");
            Scribe_Values.Look(ref TicksElapsed, "TicksElapsed");
            Scribe_Values.Look(ref TicksRequired, "TicksRequired");
            Scribe_Collections.Look(ref RequiredIngredients, "RequiredIngredients", LookMode.Deep);
        }
    }
}
