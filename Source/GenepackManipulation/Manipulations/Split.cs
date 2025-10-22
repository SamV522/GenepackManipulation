using GenepackManipulation.Extensions;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace GenepackManipulation.Manipulations
{
    public class Split : GenepackManipulation, IExposable
    {

        private static readonly SimpleCurve GeneCountChanceCurve = new SimpleCurve()
        {
          {
            new CurvePoint(2f, 0.7f),
            true
          },
          {
            new CurvePoint(3f, 0.25f),
            true
          },
          {
            new CurvePoint(1f, 0.05f),
            true
          }
        };

        [Obsolete("For Scribe use only")]
        public Split() : base() { } // For Scribe

        public Split(Building_GeneAssembler assembler) : base(assembler)
        {
            Name = "Split";
            Verb = "split";
            Gerund = "splitting";
        }

        public override void Execute(Genepack genepack)
        {
            if (genepack.GeneSet.GenesListForReading.Count <= 1)
            {
                Log.Warning($"[GenepackManipulation] Tried to split a genepack with 1 gene. Skipping.");
                Assembler.innerContainer.TryDrop(genepack, Assembler.InteractionCell, Assembler.Map, ThingPlaceMode.Near, out _);
                return;
            }

            // Determine number of genes to split to a new pack
            int num = Mathf.Min((int)GeneCountChanceCurve.RandomElementByWeight(p => p.y).x, genepack.GeneSet.GenesListForReading.Count-1);

            List<GeneDef> originalGenes = genepack.GeneSet.GenesListForReading.ToList();

            (List<GeneDef> genesA, List<GeneDef> genesB) = originalGenes.Split(num, shuffle: true);

            SpawnGenepack(genesA);
            SpawnGenepack(genesB);
        }
    }
}
