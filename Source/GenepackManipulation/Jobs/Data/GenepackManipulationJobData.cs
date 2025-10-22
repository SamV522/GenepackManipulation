using GenepackManipulation.Manipulations;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace GenepackManipulation.Jobs.Data
{
    public class GenepackManipulationJobData
    {
        public Genepack Genepack;
        public Manipulations.GenepackManipulation Manipulation;
        public int TicksElapsed = 0;
        public int TicksRequired = 0;
        public List<ThingDefCountClass> RequiredIngredients;
    }
}
