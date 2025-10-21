using GenepackRefinement.Components.Things.Gizmos;
using GenepackRefinement.Jobs.Data;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace GenepackRefinement.Components.Things
{
    public class GenepackManipulatorComponent : ThingComp
    {
        public CompProperties_GenepackManipulator Props => (CompProperties_GenepackManipulator) this.props;
        private Building_GeneAssembler assembler;
        private GenepackManipulationJobData activeJob;
        private bool workingOnJob = false;

        public bool HasJob() => activeJob != null;

        public bool Working => workingOnJob || assembler.Working;

        public GenepackManipulationJobData GetJob() => activeJob;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            assembler = parent as Building_GeneAssembler;
        }

        public void SetJob(GenepackManipulationJobData jobData)
        {
            if (HasJob())
            {
                Log.Warning($"Gene Assembler already has an active job: {activeJob.uniqueID}");
                return;
            }

            activeJob = jobData;
            Messages.Message(ToString() + " has set a new job: " + (jobData.isPrune ? "Pruning" : "Splitting") + " genepack.", MessageTypeDefOf.NeutralEvent);
        }

        public void ClearJob()
        {
            activeJob = null;
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (var gizmo in base.CompGetGizmosExtra())
                yield return gizmo;

            if (CanManipulateNow() && !HasJob())
            {
                yield return GenepackManipulationGizmos.MakePruneGizmo(assembler);
                yield return GenepackManipulationGizmos.MakeSplitGizmo(assembler);
            }

            if (HasJob())
            {
                var cancelCommand = new Command_Action
                {
                    defaultLabel = "Cancel Genepack Manipulation",
                    defaultDesc = "Cancel the current genepack manipulation job.",
                    icon = ContentFinder<Texture2D>.Get("Cancel"),
                    action = () =>
                    {
                        ClearJob();
                        Messages.Message("Genepack manipulation job cancelled.", MessageTypeDefOf.NegativeEvent);
                    }
                };
                yield return cancelCommand;
            }
        }

        public bool CanManipulateNow()
        {
            return parent.Faction == Faction.OfPlayer && assembler.PowerOn == true && !assembler.Working;
        }

        public override void CompTickInterval(int delta)
        {
            base.CompTickInterval(delta);
            if (this.Working && HasJob())
            {
                this.GetJob().ticksWorked += delta;
            }
        }

        public void ExecutePrune()
        {
            activeJob = null;
            Messages.Message("Genepack pruned successfully.", MessageTypeDefOf.PositiveEvent);
        }

        public void ExecuteSplit()
        {
            activeJob = null;
            Messages.Message("Genepack split successfully.", MessageTypeDefOf.PositiveEvent);
        }

        public List<ThingDefCountClass> RequiredIngredients()
        {
            if (!HasJob())
                return new List<ThingDefCountClass>();

            return GetJob().RequiredIngredients
                .Where(req => assembler.innerContainer.TotalStackCountOfDef(req.thingDef) < req.count)
                .ToList();
        }


        public bool NeedsIngredients()
        {
            if (!HasJob())
                return false;

            return RequiredIngredients().Any();
        }


    }

    public class CompProperties_GenepackManipulator : CompProperties
    {
        public CompProperties_GenepackManipulator()
        {
            this.compClass = typeof(GenepackManipulatorComponent);
        }
    }
}