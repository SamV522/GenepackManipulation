using Verse;

namespace GenepackRefinement
{
    [StaticConstructorOnStartup]
    public static class ModInit
    {
        static ModInit()
        {
            var harmony = new HarmonyLib.Harmony("com.SamV522.genepackrefinement");
            harmony.PatchAll();
            Log.Message("[GenepackRefinement] Harmony patches applied.");

        }
    }
}