using GenepackManipulation.Components.Things.Gizmos;
using GenepackManipulation.Defs;
using GenepackManipulation.Jobs.Data;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace GenepackManipulation.Components.Things
{
    public class GenepackManipulatorComponent : ThingComp
    {
        public CompProperties_GenepackManipulator Props => (CompProperties_GenepackManipulator) this.props;
        private Building_GeneAssembler assembler;
        private GenepackManipulationJobData activeJob;

        public bool HasJob() => activeJob != null;

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

        internal void ClearJob()
        {
            activeJob = null;
            assembler.innerContainer.TryDropAll(assembler.Position, assembler.Map, ThingPlaceMode.Near);
        }

		public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (var gizmo in base.CompGetGizmosExtra())
                yield return gizmo;

            IEnumerable<ManipulationDef> availableManipulations = DefDatabase<ManipulationDef>.AllDefs
                .Where(def => def.requiredResearch.All(research => research.IsFinished));
            
            // If there are more than 2 available manipulations, show the slightly less glamorous multi-manipulation gizmo
            if (availableManipulations.Count() > 2)
            {
                yield return GenepackManipulationGizmos.MakeMultiManipulationGizmo(assembler);
            }
            else
            {
                foreach (ManipulationDef genepackManipulationDef in availableManipulations)
                {
                    yield return GenepackManipulationGizmos.MakeManipulationGizmo(assembler, genepackManipulationDef);
                }
            }

            if (HasJob())
                yield return GenepackManipulationGizmos.MakeCancelGizmo(assembler);
        }

        internal bool CanManipulateNow()
        {
            return parent.Faction == Faction.OfPlayer && assembler.PowerOn == true && !assembler.Working;
        }

        internal void ExecuteManipulation()
        {
            activeJob.Manipulation.Execute(activeJob.Genepacks);
            assembler.innerContainer.ClearAndDestroyContents();
            Messages.Message("GenepackManipulationSuccessful".Translate(activeJob.Manipulation.Verb), MessageTypeDefOf.PositiveEvent);
            ClearJob();
        }

        internal List<ThingDefCountClass> RequiredIngredients()
        {
            if (!HasJob())
                return new List<ThingDefCountClass>();

            return GetJob().RequiredIngredients
                .Where(req => assembler.innerContainer.TotalStackCountOfDef(req.thingDef) < req.count)
                .ToList();
        }


        internal bool NeedsIngredients()
        {
            if (!HasJob())
                return false;

            return RequiredIngredients().Any();
        }

		public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_References.Look(ref assembler, "assembler");
            Scribe_Deep.Look(ref activeJob, "activeJob");
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