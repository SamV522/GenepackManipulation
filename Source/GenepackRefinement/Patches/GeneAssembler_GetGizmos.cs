using GenepackRefinement.Components.Things;
using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace GenepackRefinement.Patches
{
    [HarmonyPatch(typeof(Building_GeneAssembler), "GetGizmos")]
    public static class Patch_GeneAssembler_GetGizmos
    {
        public static void Postfix(Building_GeneAssembler __instance, ref IEnumerable<Gizmo> __result)
        {
            // If there is an active job, remove the vanilla "Recombine" gizmo
            var comp = __instance.TryGetComp<GenepackManipulatorComponent>();
            if (comp != null && comp.HasJob())
            {
                __result = __result.Where(g => !IsVanillaRecombineGizmo(g));
            }

            return;
        }

        private static bool IsVanillaRecombineGizmo(Gizmo g)
        {
            return g is Command_Action action && action.defaultLabel == "Recombine".Translate() + "...";
        }
    }
}
