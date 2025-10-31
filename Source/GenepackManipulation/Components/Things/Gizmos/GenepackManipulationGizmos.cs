using GenepackManipulation.Defs;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace GenepackManipulation.Components.Things.Gizmos
{
    internal static class GenepackManipulationGizmos
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
                action.Disable("CannotUseReason".Translate("GenepackManipulationInProgress".Translate()));
            }

            return action;
        }

        internal static Command_Action MakeMultiManipulationGizmo(Building_GeneAssembler assembler)
        {
            return new Command_Action
            {
                defaultLabel = "GenepackManipulationMuliGizmoLabel".Translate(),
                defaultDesc = "GenepackManipulationMuliGizmoDesc".Translate(),
                icon = ContentFinder<Texture2D>.Get("UI/Gizmos/SamV522.genepackmanipulation.manipulate"),
                action = () =>
                {
                    List<FloatMenuOption> options = new List<FloatMenuOption>();

                    foreach (var def in DefDatabase<ManipulationDef>.AllDefs)
                    {
                        if (def.requiredResearch.All(research => research.IsFinished))
                        {
                            options.Add(new FloatMenuOption(def.name, () =>
                            {
                                var manipulation = Manipulations.ManipulationFactory.Create(assembler, def);
                                if (manipulation != null)
                                {
                                    Find.WindowStack.Add(manipulation.GetDialog());
                                }
                            }));
                        }
                        else
                            continue;
                    }

                    if (options.Count == 0)
                    {
                        Log.Warning("[GenepackManipulation] No available manipulations to choose from, either def is incorrect or the research is not complete?");
                        return;
                    }

                    Find.WindowStack.Add(new FloatMenu(options));
                }
            }.DisableIfUnavailable(assembler);
        }

        internal static Command_Action MakeManipulationGizmo(Building_GeneAssembler assembler, ManipulationDef manipulationDef)
        {
            return new Command_Action
            {
                defaultLabel = manipulationDef.name,
                defaultDesc = manipulationDef.gizmoDescription,
                icon = ContentFinder<Texture2D>.Get(manipulationDef.iconPath),
                action = () =>
                {
                    var manipulation = Manipulations.ManipulationFactory.Create(assembler, manipulationDef);
                    if (manipulation != null)
                    {
                        Find.WindowStack.Add(manipulation.GetDialog());
                    }
                }
            }.DisableIfUnavailable(assembler);
        }

        internal static Command_Action MakeCancelGizmo(Building_GeneAssembler assembler)
        {
            return new Command_Action
            {
                defaultLabel = "GenepackManipulationCancelGizmoLabel".Translate(),
                defaultDesc = "GenepackManipulationCancelGizmoDesc".Translate(),
                icon = ContentFinder<Texture2D>.Get("UI/Designators/Cancel"),
                hotKey = KeyBindingDefOf.Designator_Cancel,
                action = () =>
                {
                    var comp = assembler.TryGetComp<GenepackManipulatorComponent>();
                    if (comp != null && comp.HasJob())
                    {
                        comp.ClearJob();
                        Messages.Message("GenepackManipulationCancelGizmoMessage".Translate(), MessageTypeDefOf.NeutralEvent);
                    }
                }
            };
        }
    }
}
