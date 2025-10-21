using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace GenepackRefinement.Jobs.Data
{
    public class GenepackManipulationJobData
    {
        public Genepack genepack;
        public int ticksWorked = 0;
        public int ticksRequired = 0;
        public bool isPrune = false;
        public string uniqueID = Guid.NewGuid().ToString();
        public List<ThingDefCountClass> RequiredIngredients;
    }
}
