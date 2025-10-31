using GenepackManipulation.Components.Things;
using GenepackManipulation.Defs;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace GenepackManipulation.Jobs
{
    public class GenepackManipulationWorkGiver : WorkGiver_Scanner
    {
        public override PathEndMode PathEndMode => PathEndMode.Touch;

        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(ThingDef.Named("GeneAssembler"));

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Building_GeneAssembler assembler = t as Building_GeneAssembler;
            if (assembler == null) return false;

            var comp = assembler.TryGetComp<GenepackManipulatorComponent>();
            if (!comp.CanManipulateNow() || !comp.HasJob()) return false;

            var jobData = comp.GetJob();
            if (jobData == null) return false;

            if (!pawn.CanReserve(t, 1, -1, null, forced)) return false;

            // if the genepack is in a genebank
            if (jobData.Genepack.ParentHolder is CompGenepackContainer genebank && genebank.parent != null)
            {
                // reserve the genebank instead - reserving the genepack itself can cause issues if it is in a genebank
                if (!pawn.CanReserve(genebank.parent, 1, -1, null, false))
                {
                    Log.Warning($"[GenepackManipulation] Pawn {pawn} cannot reserve genepack {jobData.Genepack}. Cannot assign GenepackManipulation job.");
                    return false;
                }
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
                Log.Error($"[GenepackManipulation] No current job found for assembler {assembler}. Cannot create GenepackManipulation job.");
                return null;
            }

            
            if (!assembler.innerContainer.Contains(compJob.Genepack))
            {
                if (compJob.Genepack == null)
                {
                    Log.Error("[GenepackManipulation] Genepack to manipulate is null!");
                    return null;
                }
                
                Job job = JobMaker.MakeJob(JobDefOf.HaulToContainer, compJob.Genepack, (LocalTargetInfo)t);
                job.count = 1;
                return job;
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
                Log.Error("[GenepackManipulation] JobDefOfLocal.GenepackManipulation is null!");
                return null;
            }

            // Finally, perform the genepack manipulation
            Job manipulationJob = JobMaker.MakeJob(JobDefOfLocal.GenepackManipulation, (LocalTargetInfo)t);

            if (manipulationJob == null)
            {
                Log.Error("[GenepackManipulation] Failed to create GenepackManipulation job!");
                return null;
            }
            else
                Log.Message("[GenepackManipulation] Created GenepackManipulation job successfully.");

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
