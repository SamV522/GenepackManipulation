using RimWorld;
using System.Collections.Generic;
using System.Linq;
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

        public bool TryValidate(out IReadOnlyList<string> validationErrors)
        {
            List<string> errorList = new List<string>();

            if (Genepacks == null)
            {
                errorList.Add("Genepacks is null");
            }
            else
            {
                if (Genepacks.Count == 0) errorList.Add("Genepacks is empty");
                if (Genepacks.Any(genepack => genepack == null)) errorList.Add("Genepacks contains null entries");
            }

            if (Manipulation == null) errorList.Add("Manipulation is null");
            if (TicksRequired <= 0) errorList.Add($"TicksRequired is {TicksRequired}");
            if (RequiredIngredients == null) errorList.Add("RequiredIngredients is null");

            validationErrors = errorList;
            return errorList.Count == 0;
        }

        public void ExposeData()
        {
            Scribe_Collections.Look(ref Genepacks, "Genepack", LookMode.Reference);
            Scribe_Deep.Look(ref Manipulation, "Manipulation");
            Scribe_Values.Look(ref TicksElapsed, "TicksElapsed");
            Scribe_Values.Look(ref TicksRequired, "TicksRequired");
            Scribe_Collections.Look(ref RequiredIngredients, "RequiredIngredients", LookMode.Deep);
        }
    }
}
