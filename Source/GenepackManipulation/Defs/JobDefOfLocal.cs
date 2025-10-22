using RimWorld;
using Verse;

namespace GenepackManipulation.Defs
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
