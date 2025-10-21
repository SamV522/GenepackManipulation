using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace GenepackRefinement.Jobs.Data
{
    public class GenepackManipulationJobData
    {
        public Genepack genepack;
        public int ticksWorked;
        public int ticksRequired;
        public bool isPrune;
        public string uniqueID = Guid.NewGuid().ToString();
        public List<ThingDefCountClass> RequiredIngredients;
    }
}
