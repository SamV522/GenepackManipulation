using GenepackRefinement.Manipulations;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace GenepackRefinement.Jobs.Data
{
    public class GenepackManipulationJobData
    {
        public Genepack Genepack;
        public GenepackManipulation Manipulation;
        public int TicksRequired = 0;
        public List<ThingDefCountClass> RequiredIngredients;
    }
}
