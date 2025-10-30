using RimWorld;
using Verse;

namespace GenepackManipulation.Defs
{
    [DefOf]
    public class ResearchDefOfLocal
    {
        public static ResearchProjectDef GenePruning;
        public static ResearchProjectDef GeneSplitting;

        static ResearchDefOfLocal()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ResearchDefOfLocal));
        }
    }
}
