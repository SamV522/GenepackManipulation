using GenepackManipulation.Components.World;
using GenepackManipulation.Defs;
using GenepackManipulation.Dialogs;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace GenepackManipulation.Manipulations
{
    public abstract class GenepackManipulation : IExposable
    {
        protected Building_GeneAssembler Assembler;
        protected Genepack Genepack;

        /// <summary>
        /// This constructor is intended for use by the Scribe system only.
        /// </summary>
        public GenepackManipulation() { } // For Scribe

        public GenepackManipulation(Building_GeneAssembler assembler, ManipulationDef def)
        {
            Assembler = assembler;
            Name = def.name;
            Verb = def.verb;
            Gerund = def.gerund;
        }

        protected string _name;
        protected string _verb;
        protected string _gerund;

        /// <summary>
        /// Gets or sets the name of the manipulation
        /// </summary>
        internal string Name { get => _name; set => _name = value; } // e.g., "Prune", "Split"

        /// <summary>
        /// Gets or sets the action verb associated with the operation, such as "prune" or "split".
        /// </summary>
        internal string Verb { get => _verb; set => _verb = value; } // e.g., "prune", "split"

        /// <summary>
        /// Gets the current gerund form of the action being performed, such as "Pruning" or "Splitting".
        /// </summary>
        internal string Gerund { get => _gerund; set => _gerund = value; } // e.g., "Pruning", "Splitting"

        /// <summary>
        /// Filters the provided list of genepacks based on specific criteria.
        /// </summary>
        /// <remarks>This method can be overridden in derived classes to implement custom filtering logic.</remarks>
        /// <param name="genepacks">The list of genepacks to be filtered.</param>
        /// <returns>A list of genepacks that meet the filtering criteria. By default, returns the original list without any
        /// filtering.</returns>
        public virtual List<Genepack> FilterGenepacks(List<Genepack> genepacks)
        {
            // By default, only return genepacks with more than one gene
            return genepacks.Where(genepack => genepack.GeneSet.GenesListForReading.Count > 1).ToList();
        }

        /// <summary>
        /// Calculates the required ingredients for the given list of genepacks.
        /// </summary>
        /// <remarks>This method computes the necessary ingredients based on the genepacks provided.
        /// This method can be overridden in derived classes to implement custom ingredient calculation logic.
        /// </remarks>
        /// <param name="genepacks">A list of genepacks for which the ingredients are to be calculated.</param>
        /// <returns>A list of <see cref="ThingDefCountClass"/> representing the required ingredients to complete the manipulation.</returns>
        public virtual List<ThingDefCountClass> CalculateRequiredIngredients(List<Genepack> genepacks)
        {
            List<ThingDefCountClass> ingredients = new List<ThingDefCountClass>
            {
                // Glitterworld medicine
                new ThingDefCountClass(ThingDefOf.MedicineUltratech, 1)
            };

            int totalArchitesRequired = genepacks.Sum(g => g.GeneSet.ArchitesTotal);

            if (totalArchitesRequired > 0)
            // Archite Capsules for Archite genes
            ingredients.Add(new ThingDefCountClass(ThingDefOf.ArchiteCapsule, genepacks.Sum(g => g.GeneSet.ArchitesTotal)));

            return ingredients;
        }

        /// <summary>
        /// Executes the specified genepack operation.
        /// </summary>
        /// <remarks>This method performs the core operation on the provided genepack. Ensure that the
        /// genepack is properly initialized before calling this method.</remarks>
        /// <param name="genepack">The genepack to be processed. Cannot be null.</param>
        public abstract void Execute(Genepack genepack);

        /// <summary>
        /// Get the dialog window to be displayed for this manipulation when the gizmo is clicked
        /// </summary>
        /// <remarks>This method can be overridden to provide a custom dialog for the manipulation.</remarks>
        /// <returns></returns>
        public virtual Window GetDialog()
        {
            return new GenepackManipulationDialog(Assembler, this);
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref _name, "Name");
            Scribe_Values.Look(ref _verb, "Verb");
            Scribe_Values.Look(ref _gerund, "Gerund");
            Scribe_References.Look(ref Assembler, "Assembler");
            Scribe_References.Look(ref Genepack, "Genepack");
        }

        /// <summary>
        /// Applies a cooldown period to the specified genepack.
        /// </summary>
        /// <remarks>This method invokes the cooldown process for a single genepack by internally calling
        /// <see cref="ApplyCooldowns"/> with a list containing the specified genepack.</remarks>
        /// <param name="genepack">The genepack to which the cooldown will be applied. Cannot be null.</param>
        protected virtual void ApplyCooldown(Genepack genepack)
        {
            ApplyCooldowns(new List<Genepack> { genepack });
        }

        /// <summary>
        /// Applies cooldowns to a collection of genepacks based on their gene set complexity.
        /// </summary>
        /// <remarks>
        /// Each genepack's cooldown is calculated by multiplying its gene set complexity total by 2500.
        /// This method can be overridden in derived classes to implement custom cooldown logic.
        /// </remarks>
        /// <param name="genepacks">The collection of genepacks to which cooldowns will be applied.</param>
        public void ApplyCooldowns(IEnumerable<Genepack> genepacks)
        {
            var cooldowns = Find.World.GetComponent<GenepackCooldownWorldComponent>();

            foreach (Genepack genepack in genepacks)
            {
                cooldowns.ApplyCooldown(genepack, genepack.GeneSet.ComplexityTotal * 2500);
            }
        }

        /// <summary>
        /// Creates and places a new genepack containing the specified genes.
        /// </summary>
        /// <remarks>The genes are sorted by their biostatistics, prioritizing archite genes, followed by
        /// complex and high metabolism genes. The genepack is placed near the assembler's interaction cell on the map,
        /// and a cooldown is applied after placement.</remarks>
        /// <param name="genes">A list of <see cref="GeneDef"/> objects to include in the genepack. The list is sorted by gene priority
        /// before initialization.</param>
        /// <returns>A <see cref="Genepack"/> object initialized with the specified genes, placed near the assembler.</returns>
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
