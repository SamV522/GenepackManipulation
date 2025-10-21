using RimWorld;
using Verse;

namespace GenepackRefinement.DefOfs
{
    [DefOf]
    public static class JobDefOfLocal
    {
        public static JobDef GenepackManipulation;

        static JobDefOfLocal()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(JobDefOfLocal));
        }
    }
}
