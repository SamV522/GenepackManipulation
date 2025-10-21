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
            Messages.Message("Goto job", MessageTypeDefOf.NeutralEvent);
            if(TargetA.Thing == null)
            {
                Log.Error("TargetA is null!");
                yield break;
            }
            Log.Message("Creating goto toil");
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
                Log.Error("No job assigned to GenepackManipulatorComponent!");
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

            Messages.Message("Starting work toil", MessageTypeDefOf.NeutralEvent);

            Log.Message("Creating work toil");
            Log.Message("Job will require "+ jobData.ticksRequired +" ticks");
            var workToil = new Toil
            {
                initAction = () =>
                    {
                        Log.Message("Init action");
                    },
                defaultCompleteMode = ToilCompleteMode.Delay,
                defaultDuration = jobData.ticksRequired
            };

            workToil.tickAction = () =>
            {
                pawn.skills.Learn(SkillDefOf.Intellectual, 0.1f);
                jobData.ticksWorked++;
                if (jobData.ticksWorked >= jobData.ticksRequired)
                {
                    Log.Message("Work toil completed");
                    workToil.actor.jobs.curDriver.ReadyForNextToil();
                }
            };

            workToil.AddFinishAction(() =>
            {
               if (jobData.isPrune)
               {
                   comp.ExecutePrune();
               }
               else
               {
                   comp.ExecuteSplit();
                }
            });

            Log.Message("Created work toil");

            Log.Message("using progress bar");

            workToil.WithProgressBar(TargetIndex.A, () => (float)jobData.ticksWorked / jobData.ticksRequired)
                    .WithEffect(EffecterDefOf.GeneAssembler_Working, TargetIndex.A)
                    .FailOnDespawnedNullOrForbidden(TargetIndex.A);
            
            Log.Message("Yielding work toil");
            yield return workToil;
        }
    }
}
