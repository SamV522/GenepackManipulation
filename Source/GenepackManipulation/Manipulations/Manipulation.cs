using GenepackManipulation.Components.World;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace GenepackManipulation.Manipulations
{
    public abstract class GenepackManipulation : IExposable
    {
        protected Building_GeneAssembler Assembler;
        protected Genepack Genepack;

        [Obsolete("For Scribe use only")]
        public GenepackManipulation() { } // For Scribe

        public GenepackManipulation(Building_GeneAssembler assembler)
        {
            Assembler = assembler;
        }

        protected string _name;
        protected string _verb;
        protected string _gerund;

        public string Name { get => _name; protected set => _name = value; } // e.g., "Prune", "Split"
        public string Verb { get => _verb; protected set => _verb = value; } // e.g., "prune", "split"
        public string Gerund { get => _gerund; protected set => _gerund = value; } // e.g., "Pruning", "Splitting"

        public abstract void Execute(Genepack genepack);

        public void ExposeData()
        {
            Scribe_Values.Look(ref _name, "Name");
            Scribe_Values.Look(ref _verb, "Verb");
            Scribe_Values.Look(ref _gerund, "Gerund");
            Scribe_References.Look(ref Assembler, "Assembler");
            Scribe_References.Look(ref Genepack, "Genepack");
        }

        protected virtual void ApplyCooldown(Genepack genepack)
        {
            ApplyCooldowns(new List<Genepack> { genepack });
        }

        protected void ApplyCooldowns(IEnumerable<Genepack> genepacks)
        {
            var cooldowns = Find.World.GetComponent<GenepackCooldownWorldComponent>();

            foreach (Genepack genepack in genepacks)
            {
                cooldowns.ApplyCooldown(genepack, genepack.GeneSet.ComplexityTotal * 2500);
            }
        }

        protected Genepack SpawnGenepack(List<GeneDef> genes)
        {
            Genepack newGenepack = (Genepack) ThingMaker.MakeThing(ThingDefOf.Genepack);

            genes = genes
               .OrderByDescending(g => g.biostatArc) // Archite genes first
               .ThenByDescending(g => g.biostatCpx) // Then complex genes
               .ThenByDescending(g => g.biostatMet) // Then high metabolism genes
               .ToList();

            newGenepack.Initialize(genes);

            GenPlace.TryPlaceThing(newGenepack, Assembler.InteractionCell, Assembler.Map, ThingPlaceMode.Near);

            ApplyCooldown(newGenepack);

            return newGenepack;
        }
    }
}
