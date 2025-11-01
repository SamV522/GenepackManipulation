using GenepackManipulation.Components.Things;
using GenepackManipulation.Components.World;
using GenepackManipulation.Jobs.Data;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace GenepackManipulation.Dialogs
{
    public class GenepackManipulationDialog : Window
    {
        protected readonly Manipulations.GenepackManipulation _genepackManipulation;
        protected readonly Building_GeneAssembler _assembler;
        protected readonly GenepackManipulatorComponent _genepackManipulatorComponent;

        private Genepack _selectedGenepack;
        protected Vector2 _scrollPos;
        protected float _scrollHeight;

        protected static readonly float GeneGap = 4f;
        protected static readonly float GenepackGap = 14f;
        protected static readonly float GenepackBiostatLabelWidth = 36f;
        protected static readonly Vector2 GeneSize = new Vector2(87f, 68f);

        public override Vector2 InitialSize => new Vector2(UI.screenWidth * 0.525f, UI.screenHeight * 0.75f);

        public GenepackManipulationDialog(Building_GeneAssembler assembler, Manipulations.GenepackManipulation genepackManipulation)
        {
            this._assembler = assembler;
            this._genepackManipulatorComponent = assembler.GetComp<GenepackManipulatorComponent>();
            this._genepackManipulation = genepackManipulation;
            this.forcePause = true;
            this.absorbInputAroundWindow = true;
            this.closeOnClickedOutside = true;
        }

        /// <summary>
        /// Invoked when the genes of the entity have changed.
        /// </summary>
        /// <remarks>
        /// <para>This method is called to handle any logic that should occur when the genes are modified.</para>
        /// <para>Subclasses can override this method to implement custom behavior in response to gene changes.</para>
        /// </list>
        /// </remarks>
        protected virtual void OnGenesChanged()
        {
            return;
        }

        /// <summary>
        /// Invoked once the user clicks a Genepack button in the dialog.
        /// </summary>
        /// <remarks>Subclasses can override this method to implement custom behaviour.</remarks>
        /// <param name="genepack">The genepack to be selected. Cannot be null.</param>
        protected virtual void TrySelectGenepack(Genepack genepack)
        {
            _selectedGenepack = genepack;
            SoundDefOf.Tick_High.PlayOneShotOnCamera();
            OnGenesChanged();

            return;
        }

        /// <summary>
        /// Invoked when the user clicks the confirm button in the dialog.
        /// </summary>
        /// <remarks>Subclasses can override this method to implement custom behaviour.</remarks>
        protected virtual void OnConfirmManipulation()
        {

                var targetGenepacks = new List<Genepack> { _selectedGenepack };
                if (cooldowns.IsOnCooldown(targetGenepacks))
                    {
                        if (!cooldowns.IsOnCooldown(genepack))
                        {
                            int remainingTicks = cooldowns.GetRemainingTicks(genepack);
                            float hours = (float)remainingTicks / GenDate.TicksPerHour;
                            Messages.Message("GenepackManipulationOnCooldown".Translate(hours.ToString("0.0")), MessageTypeDefOf.RejectInput, false);
                            return;
                        }
                    }
                }
                else
                {
                    GenepackManipulationJobData jobData = new GenepackManipulationJobData()
                    {
                        Genepacks = targetGenepacks,
                        Manipulation = _genepackManipulation,
                        TicksRequired = targetGenepacks.Sum(g => g.GeneSet.ComplexityTotal) * 2500,
                        RequiredIngredients = _genepackManipulation.CalculateRequiredIngredients(targetGenepacks)
                    };

                    _genepackManipulatorComponent.SetJob(jobData);

                    Close();
                }
            }
            else
            {
                Messages.Message("GenepackManipulationPleaseSelect".Translate(), MessageTypeDefOf.RejectInput, false);
            }
        }

        /// <summary>
        /// Draws a labeled section on the user interface and returns the rectangle representing the content area.
        /// </summary>
        /// <remarks>Subclasses can override this method to implement custom drawing logic for sections.</remarks>
        /// <param name="rect">The rectangle defining the position and size of the section.</param>
        /// <param name="label">The text label to display at the top of the section.</param>
        /// <returns>A <see cref="Rect"/> representing the content area of the section, excluding the label area.</returns>
        protected Rect DrawSection(Rect rect, string label)
        {
            Text.Anchor = TextAnchor.UpperLeft;
            Widgets.Label(rect, label);
            Rect contentRect = new Rect(rect.x, rect.y + Text.LineHeight, rect.width, rect.height - Text.LineHeight);
            Widgets.DrawBoxSolid(contentRect, Widgets.MenuSectionBGFillColor);
            return contentRect;
        }

        public override void DoWindowContents(Rect inRect)
        {

            Text.Font = GameFont.Medium;
            Rect headerRect = new Rect(inRect.x, inRect.y, inRect.width, Text.LineHeight);
            Widgets.Label(headerRect, "GenepackManipulationSelect".Translate(_genepackManipulation.Verb));

            Rect sectionRect = new Rect(inRect.x + 10, headerRect.y + headerRect.height + GeneGap, inRect.width - 30, inRect.height - 100f);
            Text.Font = GameFont.Small;
            Rect contentRect = DrawSection(sectionRect, "GenepackLibrary".Translate());         
            
            Rect scrollRect = new Rect(contentRect.x +4f, contentRect.y, contentRect.width - 4f, contentRect.height);
            Rect viewRect = new Rect(0, 0f, scrollRect.width - 64f, _scrollHeight);

            Widgets.BeginScrollView(scrollRect, ref _scrollPos, viewRect);

            float curX = 4f;
            float curY = 8f;

            IEnumerable<Genepack> genepacks = _genepackManipulation.FilterGenepacks(_assembler.GetGenepacks(true, true))
                                                        .OrderByDescending(genepack => genepack.GeneSet.ArchitesTotal)
                                                        .ThenByDescending(genepack => genepack.GeneSet.ComplexityTotal)
                                                        .ThenBy(genepack => genepack.GeneSet.MetabolismTotal);

            foreach (Genepack genepack in genepacks)
            {
                var thisPackWidth = GenepackBiostatLabelWidth +
                                        (GeneSize.x * genepack.GeneSet.GenesListForReading.Count) +
                                        (GeneGap * (genepack.GeneSet.GenesListForReading.Count + 1));

                Rect packRect = new Rect(curX, curY, thisPackWidth, GeneSize.y + (GeneGap * 2));

                if (DrawGenepackButton(genepack,ref curX, curY, packRect))
                {
                    TrySelectGenepack(genepack);
                }

                curX += GeneGap;
                if (curX + thisPackWidth > viewRect.width)
                {
                    curX = GeneGap;
                    curY += packRect.height + GenepackGap;
                }
            }

            if (Event.current.type == EventType.Layout)
                _scrollHeight = curY + GeneSize.y + (GeneGap * 2) + GenepackGap;

            Widgets.EndScrollView();

            // Confirm button button
            if (Widgets.ButtonText(new Rect(inRect.xMax - 158f, inRect.yMax - 40f, 150f, 30f), "GenepackManipulationStart".Translate(_genepackManipulation.Gerund.CapitalizeFirst())))
            {
                OnConfirmManipulation();
            }

            // Close button
            if (Widgets.ButtonText(new Rect(0, inRect.yMax - 40f, 150f, 30f), "Close".Translate()))
            {
                Close();
            }
        }

        /// <summary>
        /// Determines whether the specified genepack is currently selected.
        /// </summary>
        /// <remarks>Subclasses can override this method to implement custom selection validation logic.</remarks>
        /// <param name="genepack">The genepack to check for selection.</param>
        /// <returns><see langword="true"/> if the specified genepack is selected; otherwise, <see langword="false"/>.</returns>
        protected virtual bool IsGenepackSelected(Genepack genepack)
        {
            return _selectedGenepack == genepack;
        }

        /// <summary>
        /// Draws a button representing a genepack and handles user interaction with it.
        /// </summary>
        /// <remarks>
        /// <para>The button visually represents the properties of <paramref name="genepack"/> and allows the user to
        /// interact with it. The method updates the <paramref name="curX"/> position to account for the drawn button
        /// and it's contents.</para>
        /// <para>Subclasses can override this method to implement custom drawing logic for genepack buttons.</para>
        /// </remarks>
        /// <param name="genepack">The genepack to be represented by the button.</param>
        /// <param name="curX">The current X position, which will be updated after drawing.</param>
        /// <param name="curY">The Y position where the button should be drawn.</param>
        /// <param name="rect">The rectangle defining the button's position and size.</param>
        /// <returns><see langword="true"/> if the button was clicked; otherwise, <see langword="false"/>.</returns>
        protected virtual bool DrawGenepackButton(Genepack genepack,
              ref float curX,
              float curY,
              Rect rect)
        {
            bool clicked = false;

            Widgets.DrawBoxSolidWithOutline(rect, IsGenepackSelected(genepack) ? new Color(0.3f, 0.6f, 0.3f, 0.3f) : new Color(1f, 1f, 1f, 0.1f), new Color(0.5f, 0.5f, 0.5f, 0.5f));
            GUI.color = Color.white;
            curX += GeneGap;
            GeneUIUtility.DrawBiostats(genepack.GeneSet.ComplexityTotal, genepack.GeneSet.MetabolismTotal, genepack.GeneSet.ArchitesTotal, ref curX, curY, 4f);
            List<GeneDef> genesListForReading = genepack.GeneSet.GenesListForReading;
            for (int index = 0; index < genesListForReading.Count; ++index)
            {
                GeneDef gene = genesListForReading[index];
                Rect geneRect = new Rect(curX, curY + GeneGap, GeneSize.x, GeneSize.y);
                GeneUIUtility.DrawGeneDef(genesListForReading[index], geneRect, GeneType.Xenogene, null, false, false);
                curX += GeneSize.x + GeneGap;
            }

            Widgets.InfoCardButton(rect.xMax - 24f, rect.y + 2f, genepack);

            if (Mouse.IsOver(rect))
                Widgets.DrawHighlight(rect);

            if (Widgets.ButtonInvisible(rect))
                clicked = true;

            curX = Mathf.Max(curX, rect.xMax + GenepackGap);

            return clicked;
        }
    }
}
