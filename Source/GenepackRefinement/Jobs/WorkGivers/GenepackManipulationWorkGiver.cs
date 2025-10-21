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
            if (comp.CanManipulateNow()) return false;

            if (!pawn.CanReserve(t, 1, -1, null, forced)) return false;
            if (t.IsForbidden(pawn)) return false;
            if (t.IsBurning()) return false;

            return true;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!(t is Building_GeneAssembler buildingGeneAssembler))
                return (Job)null;

            var comp = t.TryGetComp<GenepackManipulatorComponent>();
            if (comp == null) return null;

            Genepack genepack = comp.GetJob().genepack;
            Building_GeneAssembler assembler = t as Building_GeneAssembler;

            // Take the required ingredients to the assembler
            if (comp.NeedsIngredients())
            {
                foreach (var req in comp.RequiredIngredients())
                {
                    Thing found = FindIngredient(pawn, req.thingDef, req.count);
                    if (found != null)
                    {
                        Job job = JobMaker.MakeJob(JobDefOf.HaulToContainer, found, t);
                        job.count = Mathf.Min(req.count, found.stackCount);
                        return job;
                    }
                }
            }

            // Finally, perform the genepack manipulation
            return JobMaker.MakeJob(JobDefOfLocal.GenepackManipulation, t);
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
