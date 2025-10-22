using GenepackRefinement.Components.Things;
using GenepackRefinement.Jobs.Data;
using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace GenepackRefinement.Jobs
{
    internal class GenepackManipulationJobDriver : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            if (TargetA == null || TargetA.Thing == null)
            {
                Log.Error("[GenepackRefinement] TargetA or TargetA.Thing is null during reservation.");
                return false;
            }

            bool reserved = pawn.Reserve(TargetA, job, 1, -1, null, errorOnFailed);
            
            if (pawn.skills?.GetSkill(SkillDefOf.Intellectual)?.Level < 10)
                return false;

            return reserved;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            if(TargetA.Thing == null)
            {
                Log.Error("TargetA is null!");
                yield break;
            }

            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);

            GenepackManipulatorComponent comp = TargetA.Thing.TryGetComp<GenepackManipulatorComponent>();
            if (comp == null)
            {
                Log.Error("GenepackManipulatorComponent not found!");
                yield break;
            }

            GenepackManipulationJobData jobData = comp.GetJob();
            if (jobData == null)
            {
                Log.Error("[GenepackRefinement] No job assigned to GenepackManipulatorComponent!");
                yield break;
            }

            if (pawn == null)
            {
                Log.Error("Pawn is null!");
                yield break;
            }

            if (pawn.skills == null)
            {
                Log.Error("Pawn has no skills!");
                yield break;
            }

            var workToil = new Toil
            {
                defaultCompleteMode = ToilCompleteMode.Delay,
                defaultDuration = jobData.TicksRequired
            };

            workToil.AddFinishAction(() =>
            {
                comp.ExecuteManipulation();
            });

            workToil.WithProgressBarToilDelay(TargetIndex.A)
                    .WithEffect(EffecterDefOf.GeneAssembler_Working, TargetIndex.A)
                    .FailOnDespawnedNullOrForbidden(TargetIndex.A);
            
            yield return workToil;
        }
    }
}
