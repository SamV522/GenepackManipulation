using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
