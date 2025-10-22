using GenepackManipulation.Components.Things;
using GenepackManipulation.Components.World;
using GenepackManipulation.Jobs.Data;
using GenepackManipulation.Manipulations;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace GenepackManipulation.Dialogs
{
    public class GenepackManipulationDialog : Window
    {
        private readonly Manipulations.GenepackManipulation _genepackManipulation;
        private readonly Building_GeneAssembler _assembler;
        private readonly GenepackManipulatorComponent _genepackManipulatorComponent;

        private Genepack _selectedGenepack;
        private Vector2 _scrollPos;
        private float _scrollHeight;

        public override Vector2 InitialSize => new Vector2(1016f, (float)UI.screenHeight / 2);

        public GenepackManipulationDialog(Building_GeneAssembler assembler, Manipulations.GenepackManipulation genepackManipulation)
        {
            this._assembler = assembler;
            this._genepackManipulatorComponent = assembler.GetComp<GenepackManipulatorComponent>();
            this._genepackManipulation = genepackManipulation;
            this.forcePause = true;
            this.absorbInputAroundWindow = true;
            this.closeOnClickedOutside = true;
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(inRect.x, inRect.y, inRect.width, 40f), "Select a Genepack to " + _genepackManipulation.Verb);
            Text.Font = GameFont.Small;

            Rect scrollRect = new Rect(inRect.x, inRect.y + 50f, inRect.width, inRect.height - 100f);
            Rect viewRect = new Rect(0, 0f, scrollRect.width - 64f, _scrollHeight);

            Widgets.BeginScrollView(scrollRect, ref _scrollPos, viewRect);

            float curX = 10f;
            float curY = 10f;
            float packWidth = 34;
            float packHeight = 100f;
            float verticalSpacing = 18f;

            foreach (Genepack genepack in _assembler.GetGenepacks(true, true).Where(genepack => genepack.GeneSet.GenesListForReading.Count > 1))
            {
                packWidth = (float)(34.0 + (double)GeneCreationDialogBase.GeneSize.x * (double)genepack.GeneSet.GenesListForReading.Count + 4.0 * (double)(genepack.GeneSet.GenesListForReading.Count + 2));
                Rect packRect = new Rect(curX, curY, packWidth, packHeight);
                if (DrawGenepack(genepack,ref curX, curY, packRect))
                {
                    _selectedGenepack = genepack;
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
                _scrollHeight = curY + packHeight + verticalSpacing;

            Widgets.EndScrollView();

            // Confirm button button
            if (Widgets.ButtonText(new Rect(inRect.xMax - 158f, inRect.yMax - 40f, 150f, 30f), "Start " + _genepackManipulation.Gerund.CapitalizeFirst()))
            {
                if (_selectedGenepack != null)
                {
                    var cooldowns = Find.World.GetComponent<GenepackCooldownWorldComponent>();
                    if (cooldowns.IsOnCooldown(_selectedGenepack))
                    {
                        int remainingTicks = cooldowns.GetRemainingTicks(_selectedGenepack);
                        float hours = remainingTicks / GenDate.TicksPerHour;
                        Messages.Message($"The selected genepack is still on cooldown for {hours:0} hours.", MessageTypeDefOf.RejectInput, false);
                    }else
                    {
                        List<ThingDefCountClass> ingredients = new List<ThingDefCountClass>
                        {
                            // Glitterworld medicine
                            new ThingDefCountClass(ThingDefOf.MedicineUltratech, 1) 
                        };

                        if (_selectedGenepack.GeneSet.ArchitesTotal > 0)
                            // Archite Capsules for Archite genes
                            ingredients.Add(new ThingDefCountClass(ThingDefOf.ArchiteCapsule, _selectedGenepack.GeneSet.ArchitesTotal)); 

                        GenepackManipulationJobData jobData = new GenepackManipulationJobData()
                        {
                            Genepack = _selectedGenepack,
                            Manipulation = _genepackManipulation,
                            TicksRequired = _selectedGenepack.GeneSet.ComplexityTotal * 2500,
                            RequiredIngredients = ingredients
                        };

                        _genepackManipulatorComponent.SetJob(jobData);

                        Close();
                    }                        
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

            Widgets.DrawBoxSolid(rect, genepack == _selectedGenepack ? new Color(0.3f, 0.6f, 0.3f, 0.3f) : new Color(0.2f, 0.2f, 0.2f, 0.2f));
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
