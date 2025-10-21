using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace GenepackRefinement.Components.World
{
    internal class GenepackCooldownWorldComponent : WorldComponent
    {
        private Dictionary<string, int> genepackCooldowns = new Dictionary<string, int>();

        public GenepackCooldownWorldComponent(RimWorld.Planet.World world) : base(world) { }

        public void ApplyCooldown(Thing genepack, int cooldownTicks)
        {
            if (genepack == null || genepack.Destroyed) return;
            genepackCooldowns[genepack.ThingID] = Find.TickManager.TicksGame + cooldownTicks;
        }

        public bool IsOnCooldown(Thing genepack)
        {
            if (genepack == null || genepack.Destroyed) return false;
            if (genepackCooldowns.TryGetValue(genepack.ThingID, out int endTick))
            {
                return Find.TickManager.TicksGame < endTick;
            }
            return false;
        }

        public int GetRemainingTicks(Thing genepack)
        {
            if (genepackCooldowns.TryGetValue(genepack.ThingID, out int endTick))
            {
                return endTick - Find.TickManager.TicksGame;
            }
            return 0;
        }

        public void CleanupExpired()
        {
            int currentTick = Find.TickManager.TicksGame;
            foreach (var key in new List<string>(genepackCooldowns.Keys))
            {
                if (genepackCooldowns[key] <= currentTick)
                    genepackCooldowns.Remove(key);
            }
        }

        public override void WorldComponentTick()
        {
            if (Find.TickManager.TicksGame % 1000 == 0)
            {
                CleanupExpired();
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref genepackCooldowns, "genepackCooldowns", LookMode.Value, LookMode.Value);
        }
    }
}
