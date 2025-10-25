using GenepackManipulation.Dialogs;
using GenepackManipulation.Jobs.Data;
using RimWorld;
using System.Linq;
using UnityEngine;
using Verse;

namespace GenepackManipulation.Components.Things.Gizmos
{
    public static class GenepackManipulationGizmos
    {

        private static Command_Action DisableIfUnavailable(this Command_Action action, Building_GeneAssembler assembler)
        {
            // The original gene assembler includes a check for research, but I cannot see where that would be necessary?

            // Disable the Gizmo if there is no power
            if (!assembler.PowerOn)
            {
                action.Disable("CannotUseNoPower".Translate());
            }

            // Disable the Gizmo if there is no genepacks.
            if (!assembler.GetGenepacks(true, true).Any())
            {
                action.Disable("CannotUseReason".Translate("NoGenepacksAvailable".Translate().CapitalizeFirst()));
            }

            // Safely get the manipulator component and disable if it's not present
            var comp = assembler.TryGetComp<GenepackManipulatorComponent>();
            if (comp != null && comp.HasJob())
            {
                action.Disable("CannotUseReason".Translate("Manipulation job already in progress"));
            }

            return action;
        }

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
            }.DisableIfUnavailable(assembler);
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
            }.DisableIfUnavailable(assembler);
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
