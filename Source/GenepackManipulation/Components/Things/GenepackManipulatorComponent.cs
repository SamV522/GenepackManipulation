using GenepackManipulation.Components.Things.Gizmos;
using GenepackManipulation.Defs;
using GenepackManipulation.Jobs.Data;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace GenepackManipulation.Components.Things
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
                Log.Warning($"[GenepackManipulation] Tried to set job on Gene Assembler, but Gene Assembler already has an active job");
                return;
            }

            activeJob = jobData;
        }

        public void ClearJob()
        {
            activeJob = null;
            assembler.innerContainer.TryDropAll(assembler.Position, assembler.Map, ThingPlaceMode.Near);
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (var gizmo in base.CompGetGizmosExtra())
                yield return gizmo;

            if (CanManipulateNow() && !HasJob())
            {
                if (ResearchDefOfLocal.GenePruning.IsFinished)
                    yield return GenepackManipulationGizmos.MakePruneGizmo(assembler);

                if (ResearchDefOfLocal.GeneSplitting.IsFinished)
                    yield return GenepackManipulationGizmos.MakeSplitGizmo(assembler);
            }

            if (HasJob())
            {
                yield return GenepackManipulationGizmos.MakeCancelGizmo(assembler, activeJob);
            }
        }

        public bool CanManipulateNow()
        {
            return parent.Faction == Faction.OfPlayer && assembler.PowerOn == true && !assembler.Working;
        }

        public void ExecuteManipulation()
        {
            activeJob.Manipulation.Execute(activeJob.Genepack);
            assembler.innerContainer.ClearAndDestroyContents();
            Messages.Message($"Genepack {activeJob.Manipulation.Verb} successfully.", MessageTypeDefOf.PositiveEvent);
            ClearJob();
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