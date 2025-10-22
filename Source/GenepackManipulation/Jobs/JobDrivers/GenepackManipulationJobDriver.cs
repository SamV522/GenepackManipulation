using GenepackManipulation.Components.Things;
using GenepackManipulation.Jobs.Data;
using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace GenepackManipulation.Jobs
{
    internal class GenepackManipulationJobDriver : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            if (TargetA == null || TargetA.Thing == null)
            {
                Log.Error("[GenepackManipulation] TargetA or TargetA.Thing is null during reservation.");
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
                Log.Error("[GenepackManipulation] No job assigned to GenepackManipulatorComponent!");
                yield break;
            }

            Toil workToil = new Toil();

            workToil.tickIntervalAction = delta =>
            {
                jobData.TicksElapsed += delta;
                pawn.skills?.Learn(SkillDefOf.Intellectual, 0.1f * (float)delta);
                pawn.GainComfortFromCellIfPossible(delta, chairsOnly: true);

                if (workToil.defaultDuration <= jobData.TicksElapsed)
                {
                    comp.ExecuteManipulation();
                    pawn.jobs.EndCurrentJob(JobCondition.Succeeded);
                }
            };
            workToil.FailOn(() => comp.GetJob() != jobData);
            workToil.FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell);
            workToil.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            workToil.WithEffect(EffecterDefOf.GeneAssembler_Working, TargetIndex.A);
            workToil.WithProgressBar(TargetIndex.A, () => (float) jobData.TicksElapsed / jobData.TicksRequired);
            workToil.defaultCompleteMode = ToilCompleteMode.Never;
            workToil.defaultDuration = jobData.TicksRequired;
            workToil.activeSkill = () => SkillDefOf.Intellectual;

            yield return workToil;
        }
    }
}
