using Verse;

namespace GenepackManipulation
{
    [StaticConstructorOnStartup]
    public static class ModInit
    {
        static ModInit()
        {
            var harmony = new HarmonyLib.Harmony("com.SamV522.genepackManipulation");
            harmony.PatchAll();
            Log.Message("[GenepackManipulation] Harmony patches applied.");

        }
    }
}