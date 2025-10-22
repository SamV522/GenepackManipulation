using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace GenepackRefinement.Manipulations
{
    public class Prune : GenepackManipulation
    {
        public override string Name => "Prune";
        public override string Verb => "prune";
        public override string Gerund => "pruning";

        // Chance curve for number of genes to prune, prune 1 gene most of the time, rarely prune more
        private static readonly SimpleCurve GeneCountChanceCurve = new SimpleCurve()
        {
          {
            new CurvePoint(1f, 0.7f),
            true
          },
          {
            new CurvePoint(2f, 0.2f),
            true
          },
          {
            new CurvePoint(3f, 0.08f),
            true
          },
          {
            new CurvePoint(4f, 0.02f),
            true
          }
        };

        public Prune(Building_GeneAssembler assembler) : base(assembler) { }

        public override void Execute(Genepack genepack)
        {
            // Determine number of genes to prune
            int num = Mathf.Min((int)GeneCountChanceCurve.RandomElementByWeight(p => p.y).x, genepack.GeneSet.GenesListForReading.Count);

            //Initialize list of genes to add and out result
            List<GeneDef> genesToPrune = new List<GeneDef>();
            GeneDef result;

            // Select genes to prune
            for (int index = 0; index < num && genepack.GeneSet.GenesListForReading.TryRandomElementByWeight(SelectionWeight, out result); ++index)
                genesToPrune.Add(result);

            if (genesToPrune.Any())
            {
                ApplyCooldown(SpawnGenepack(genesToPrune));
            }

            float SelectionWeight(GeneDef g)
            {
                if (genesToPrune.Contains(g))
                    return 0.0f;

                // Weight genes based on biostat complexity: complex genes are less likely to be pruned
                return g.biostatCpx > 0 ? 1f : 3f;
            }
        }
    }
}
