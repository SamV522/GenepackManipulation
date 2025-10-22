using GenepackRefinement.Components.Things;
using GenepackRefinement.DefOfs;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace GenepackRefinement.Jobs
{
    public class GenepackManipulationWorkGiver : WorkGiver_Scanner
    {
        public override PathEndMode PathEndMode => PathEndMode.Touch;

        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(ThingDef.Named("GeneAssembler"));

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            var assembler = t as Building_GeneAssembler;
            if (assembler == null) return false;

            var comp = assembler.TryGetComp<GenepackManipulatorComponent>();
            if (!comp.CanManipulateNow() || !comp.HasJob()) return false;

            var jobData = comp.GetJob();
            if (jobData == null) return false;

            if (!pawn.CanReserve(t, 1, -1, null, forced)) return false;

            // try to reserve the genepack as well
            if (!pawn.CanReserve(jobData.Genepack, 1, -1, null, true))
            {
                Log.Warning($"[GenepackRefinement] Pawn {pawn} cannot reserve genepack {jobData.Genepack}. Cannot assign GenepackManipulation job.");
                return false;
            }

            if (t.IsForbidden(pawn)) return false;
            if (t.IsBurning()) return false;

            return true;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!(t is Building_GeneAssembler assembler))
                return null;

            var comp = assembler.TryGetComp<GenepackManipulatorComponent>();
            if (comp == null) return null;

            var compJob = comp.GetJob();
            if (compJob == null)
            {
                Log.Error($"[GenepackRefinement] No current job found for assembler {assembler}. Cannot create GenepackManipulation job.");
                return null;
            }

            
            if (!assembler.innerContainer.Contains(compJob.Genepack))
            {
                if (compJob.Genepack.ParentHolder is CompGenepackContainer genebank)
                {
                    genebank.innerContainer.TryDrop(compJob.Genepack, genebank.parent.InteractionCell, genebank.parent.Map, ThingPlaceMode.Near, out _);
                    Job job = JobMaker.MakeJob(JobDefOf.HaulToContainer, compJob.Genepack, (LocalTargetInfo) t);
                    job.count = 1;
                    return job;
                }
            }

            // Take the required ingredients to the assembler
            if (comp.NeedsIngredients())
            {
                foreach (var req in comp.RequiredIngredients())
                {
                    Thing found = FindIngredient(pawn, req.thingDef, req.count);
                    if (found != null)
                    {
                        Job job = JobMaker.MakeJob(JobDefOf.HaulToContainer, found, (LocalTargetInfo) t);
                        job.count = Mathf.Min(req.count, found.stackCount);
                        return job;
                    }
                }
            }

            if (JobDefOfLocal.GenepackManipulation == null)
            {
                Log.Error("JobDefOfLocal.GenepackManipulation is null!");
                return null;
            }

            // Finally, perform the genepack manipulation
            Job manipulationJob = JobMaker.MakeJob(JobDefOfLocal.GenepackManipulation, (LocalTargetInfo)t);

            if (manipulationJob == null)
            {
                Log.Error("Failed to create GenepackManipulation job!");
                return null;
            }
            else
                Log.Message("Created GenepackManipulation job successfully.");

            return manipulationJob;
        }

        private Thing FindIngredient(Pawn pawn, ThingDef def, int minCount = 1) =>
            GenClosest.ClosestThingReachable(
                pawn.Position,
                pawn.Map,
                ThingRequest.ForDef(def),
                PathEndMode.ClosestTouch,
                TraverseParms.For(pawn),
                validator: x => !x.IsForbidden(pawn) && pawn.CanReserve(x) && x.stackCount >= minCount
            );

    }
}
