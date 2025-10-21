using GenepackRefinement.Components.Things;
using GenepackRefinement.Jobs.Data;
using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace GenepackRefinement.Jobs
{
    internal class GenepackManipulationJobDriver : JobDriver
    {
        private Building_GeneAssembler Assembler => (Building_GeneAssembler) job.targetB.Thing;
        private GenepackManipulatorComponent Comp => Assembler.TryGetComp<GenepackManipulatorComponent>();
        private Thing Genepack => job.targetA.Thing;
        private GenepackManipulationJobData JobData => Comp?.GetJob();

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            bool reserved = pawn.Reserve(Assembler, job, 1, -1, null, errorOnFailed)
                          && pawn.Reserve(Genepack, job, 1, -1, null, errorOnFailed);

            if (JobData?.RequiredIngredients != null)
            {
                foreach (var req in JobData.RequiredIngredients)
                {
                    Thing ingredient = FindClosestIngredient(req.thingDef, req.count);
                    if (ingredient == null || !pawn.Reserve(ingredient, job, 1, -1, null, errorOnFailed))
                        return false;
                }
            }

            if (pawn.skills?.GetSkill(SkillDefOf.Intellectual)?.Level < 10)
                return false;

            return reserved;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);

            var workToil = new Toil
            {
                activeSkill = () => SkillDefOf.Intellectual,
                initAction = () => ticksLeftThisToil = JobData.ticksRequired,
                tickAction = () =>
                {
                    JobData.ticksWorked++;
                    pawn.skills?.Learn(SkillDefOf.Intellectual, 0.1f);
                },
                finishActions = { () =>
                    {
                        if (JobData.isPrune)
                            Comp.ExecutePrune();
                        else
                            Comp.ExecuteSplit();
                    }
                },
                defaultCompleteMode = ToilCompleteMode.Delay
            };

            workToil.WithProgressBar(TargetIndex.A, () => (float)JobData.ticksWorked / JobData.ticksRequired)
                    .WithEffect(EffecterDefOf.GeneAssembler_Working, TargetIndex.A);
            
            yield return workToil;
        }

        private Thing FindClosestIngredient(ThingDef def, int minCount)
        {
            return GenClosest.ClosestThingReachable(
                pawn.Position,
                pawn.Map,
                ThingRequest.ForDef(def),
                PathEndMode.Touch,
                TraverseParms.For(pawn),
                9999f,
                t => t.stackCount >= minCount && !t.IsForbidden(pawn)
            );
        }
    }
}
