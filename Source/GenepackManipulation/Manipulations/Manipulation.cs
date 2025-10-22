using GenepackManipulation.Components.World;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace GenepackManipulation.Manipulations
{
    public abstract class GenepackManipulation
    {
        protected Building_GeneAssembler Assembler;
        protected Genepack Genepack;

        public GenepackManipulation(Building_GeneAssembler assembler)
        {
            Assembler = assembler;
        }

        public abstract string Name { get; }
        public abstract string Verb { get; } // e.g., "prune", "split"
        public abstract string Gerund { get; } // e.g., "Pruning", "Splitting"

        public abstract void Execute(Genepack genepack);

        protected virtual void ApplyCooldown(Genepack genepack)
        {
            ApplyCooldowns(new List<Genepack> { genepack });
        }

        protected  void ApplyCooldowns(IEnumerable<Genepack> genepacks)
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
