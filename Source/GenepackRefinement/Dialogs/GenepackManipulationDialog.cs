using GenepackRefinement.Components.Things;
using GenepackRefinement.Jobs.Data;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace GenepackRefinement.Dialogs
{
    public class GenepackManipulationDialog : Window
    {
        private bool isPrune;
        private Building_GeneAssembler assembler;
        private GenepackManipulatorComponent genepackManipulatorComponent;

        private Genepack selectedGenepack;
        private Vector2 scrollPos;
        private float scrollHeight;

        public override Vector2 InitialSize => new Vector2(1016f, (float)UI.screenHeight / 2);

        public GenepackManipulationDialog(Building_GeneAssembler assembler, bool isPrune)
        {
            this.assembler = assembler;
            this.genepackManipulatorComponent = assembler.GetComp<GenepackManipulatorComponent>();
            this.isPrune = isPrune;
            this.forcePause = true;
            this.absorbInputAroundWindow = true;
            this.closeOnClickedOutside = true;
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(inRect.x, inRect.y, inRect.width, 40f), "Select a Genepack to " + (isPrune ? "Prune" : "Split"));
            Text.Font = GameFont.Small;

            Rect scrollRect = new Rect(inRect.x, inRect.y + 50f, inRect.width, inRect.height - 100f);
            Rect viewRect = new Rect(0, 0f, scrollRect.width - 64f, scrollHeight);

            Widgets.BeginScrollView(scrollRect, ref scrollPos, viewRect);

            float curX = 10f;
            float curY = 10f;
            float packWidth = 34;
            float packHeight = 100f;
            float verticalSpacing = 18f;

            foreach (Genepack genepack in assembler.GetGenepacks(true, true).Where(genepack => genepack.GeneSet.GenesListForReading.Count > 1))
            {
                packWidth = (float)(34.0 + (double)GeneCreationDialogBase.GeneSize.x * (double)genepack.GeneSet.GenesListForReading.Count + 4.0 * (double)(genepack.GeneSet.GenesListForReading.Count + 2));
                Rect packRect = new Rect(curX, curY, packWidth, packHeight);
                if (DrawGenepack(genepack,ref curX, curY, packRect))
                {
                    selectedGenepack = genepack;
                    SoundDefOf.Tick_High.PlayOneShotOnCamera();
                }

                curX += 4;
                if (curX + packWidth > viewRect.width)
                {
                    curX = 10f;
                    curY += packHeight + verticalSpacing;
                }
            }

            if (Event.current.type == EventType.Layout)
                scrollHeight = curY + packHeight + verticalSpacing;

            Widgets.EndScrollView();

            // Confirm button button
            if (Widgets.ButtonText(new Rect(inRect.xMax - 158f, inRect.yMax - 40f, 150f, 30f), "Start " + (isPrune ? "Pruning" : "Splitting")))
            {
                if (selectedGenepack != null)
                {
                    List<ThingDefCountClass> ingredients = new List<ThingDefCountClass>
                    {
                        new ThingDefCountClass(ThingDefOf.MedicineUltratech, 1) // Glitterworld medicine
                    };

                    if (selectedGenepack.GeneSet.ArchitesTotal > 0)
                        ingredients.Add(new ThingDefCountClass(ThingDefOf.ArchiteCapsule, selectedGenepack.GeneSet.ArchitesTotal));

                    GenepackManipulationJobData jobData = new GenepackManipulationJobData()
                    {
                        genepack = selectedGenepack,
                        isPrune = isPrune,
                        ticksWorked = 0,
                        ticksRequired = selectedGenepack.GeneSet.ComplexityTotal * 2500,
                        uniqueID = Guid.NewGuid().ToString(),
                        RequiredIngredients = ingredients
                    };

                    genepackManipulatorComponent.SetJob(jobData);

                    Close();
                }
                else
                {
                    Messages.Message("Please select a genepack first.", MessageTypeDefOf.RejectInput, false);
                }
            }

            if (Widgets.ButtonText(new Rect(8, inRect.yMax - 40f, 150f, 30f), "Close"))
            {
                Close();
            }
        }

        private bool DrawGenepack(Genepack genepack,
              ref float curX,
              float curY,
              Rect rect)
        {
            bool clicked = false;

            Widgets.DrawBoxSolid(rect, genepack == selectedGenepack ? new Color(0.3f, 0.6f, 0.3f, 0.3f) : new Color(0.2f, 0.2f, 0.2f, 0.2f));
            Widgets.DrawBox(rect);
            Widgets.DrawHighlight(rect);
            Widgets.DrawBox(rect);
            GUI.color = Color.white;
            GeneUIUtility.DrawBiostats(genepack.GeneSet.ComplexityTotal, genepack.GeneSet.MetabolismTotal, genepack.GeneSet.ArchitesTotal, ref curX, curY, 4f);
            List<GeneDef> genesListForReading = genepack.GeneSet.GenesListForReading;
            for (int index = 0; index < genesListForReading.Count; ++index)
            {
                GeneDef gene = genesListForReading[index];
                Rect geneRect = new Rect(curX, curY + 4f, GeneCreationDialogBase.GeneSize.x, GeneCreationDialogBase.GeneSize.y);
                string extraTooltip = (string)null;
                GeneUIUtility.DrawGeneDef(genesListForReading[index], geneRect, GeneType.Xenogene, (Func<string>)(() => extraTooltip), false, false);
                curX += GeneCreationDialogBase.GeneSize.x + 4f;
            }
            Widgets.InfoCardButton(rect.xMax - 24f, rect.y + 2f, (Thing)genepack);
            if (Mouse.IsOver(rect))
                Widgets.DrawHighlight(rect);

            if (Widgets.ButtonInvisible(rect))
                clicked = true;

            curX = Mathf.Max(curX, rect.xMax + 14f);

            return clicked;
        }
    }
}
