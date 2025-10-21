using GenepackRefinement.Dialogs;
using RimWorld;
using UnityEngine;
using Verse;

namespace GenepackRefinement.Components.Things.Gizmos
{
    internal class GenepackManipulationGizmos
    {
        public static Command_Action MakePruneGizmo(Building_GeneAssembler assembler)
        {
            return new Command_Action
            {
                defaultLabel = "Prune",
                defaultDesc = "Prune randomly selected gene(s) from a genepack.",
                icon = ContentFinder<Texture2D>.Get("UI/Gizmos/samv522.genepackrefinment.prune"),
                action = () =>
                {
                    Find.WindowStack.Add(new GenepackManipulationDialog(assembler, isPrune: true));
                }
            };
        }

        public static Command_Action MakeSplitGizmo(Building_GeneAssembler assembler)
        {
            return new Command_Action
            {
                defaultLabel = "Split",
                defaultDesc = "Split a genepack into two genepacks with randomly selected gene(s).",
                icon = ContentFinder<Texture2D>.Get("UI/Gizmos/samv522.genepackrefinment.split"),
                action = () =>
                {
                    Find.WindowStack.Add(new GenepackManipulationDialog(assembler, isPrune: false));
                }
            };
        }
    }
}
