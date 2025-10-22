using GenepackManipulation.Dialogs;
using GenepackManipulation.Jobs.Data;
using RimWorld;
using UnityEngine;
using Verse;

namespace GenepackManipulation.Components.Things.Gizmos
{
    internal class GenepackManipulationGizmos
    {
        public static Command_Action MakePruneGizmo(Building_GeneAssembler assembler)
        {
            return new Command_Action
            {
                defaultLabel = "Prune",
                defaultDesc = "Remove randomly selected gene(s) from a genepack.",
                icon = ContentFinder<Texture2D>.Get("UI/Gizmos/samv522.genepackrefinment.prune"),
                action = () =>
                {
                    Find.WindowStack.Add(new GenepackManipulationDialog(assembler, new Manipulations.Prune(assembler)));
                }
            };
        }

        public static Command_Action MakeSplitGizmo(Building_GeneAssembler assembler)
        {
            return new Command_Action
            {
                defaultLabel = "Split",
                defaultDesc = "Split a genepack into two genepacks.",
                icon = ContentFinder<Texture2D>.Get("UI/Gizmos/samv522.genepackrefinment.split"),
                action = () =>
                {
                    Find.WindowStack.Add(new GenepackManipulationDialog(assembler, new Manipulations.Split(assembler)));
                }
            };
        }

        public static Command_Action MakeCancelGizmo(Building_GeneAssembler assembler, GenepackManipulationJobData jobData)
        {
            return new Command_Action
            {
                defaultLabel = "Cancel "+ jobData.Manipulation.Gerund.CapitalizeFirst(),
                defaultDesc = "Cancel the current genepack manipulation job.",
                icon = ContentFinder<Texture2D>.Get("UI/Designators/Cancel"),
                hotKey = KeyBindingDefOf.Designator_Cancel,
                action = () =>
                {
                    var comp = assembler.TryGetComp<GenepackManipulatorComponent>();
                    if (comp != null && comp.HasJob())
                    {
                        comp.ClearJob();
                        Messages.Message("Genepack manipulation job cancelled.", MessageTypeDefOf.NeutralEvent);
                    }
                }
            };
        }
    }
}
