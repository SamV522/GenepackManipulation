using System.Collections.Generic;
using Verse;

namespace GenepackManipulation.Defs
{
    public class ManipulationDef : Def
    {
        public readonly string name;
        public readonly string gizmoDescription;
        public readonly string verb;
        public readonly string gerund;
        public readonly string iconPath;
        public readonly string className; // Fully qualified class name
        public readonly List<ResearchProjectDef> requiredResearch;
    }
}