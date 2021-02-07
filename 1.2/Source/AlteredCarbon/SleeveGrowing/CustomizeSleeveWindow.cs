﻿using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;
using System.Text.RegularExpressions;
using System.Linq.Expressions;

namespace AlteredCarbon
{

    public class CustomizeSleeveWindow : Window
    {
        private List<ThingDef> orderedValidAlienRaces;
        private List<HairDef> permittedHair;

        //Variables
        public Pawn newSleeve;
        public int finalExtraPrintingTimeCost = 0;
        public bool refreshAndroidPortrait = false;
        public Vector2 upgradesScrollPosition = new Vector2();
        public Vector2 traitsScrollPosition = new Vector2();
        List<Trait> allTraits = new List<Trait>();

        //Customization
        public PawnKindDef currentPawnKindDef;
        public Backstory newChildhoodBackstory;
        public Backstory newAdulthoodBackstory;
        public Trait replacedTrait;
        public Trait newTrait;

        public List<Trait> originalTraits = new List<Trait>();
        public Building_SleeveGrower sleeveGrower;

        //Static Values
        public override Vector2 InitialSize
        {
            get
            {
                if (!ModCompatibility.AlienRacesIsActive)
                {
                    return new Vector2(728f, 538f);
                }
                else
                {
                    return new Vector2(728f, 638f);
                }
            }
        }
            
            
        public static readonly float upgradesOffset = 640f;
        private static readonly Vector2 PawnPortraitSize = new Vector2(200f, 280f);


        //RectValues
        public Rect lblName;
        public Rect lblGender;
        public Rect lblSkinColour;
        public Rect lblRaceChange;
        public Rect lblHeadShape;
        public Rect lblBodyShape;
        public Rect lblHairColour;
        public Rect lblHairType;
        public Rect lblTimeToGrow;
        public Rect lblRequireBiomass;

        public Rect qualitySlider;
        public Rect beautySlider;

        public bool allowMales = true;
        public bool allowFemales = true;
        public Rect btnGenderMale;
        public Rect btnGenderFemale;

        public Rect btnSkinColourOutlinePremade;
        public List<Tuple<Rect, Color>> skinColorButtons;
        static readonly List<Color> humanSkinColors = new List<Color>(new Color[] { rgbConvert(242, 237, 224), rgbConvert(255, 239, 213), rgbConvert(255, 239, 189), rgbConvert(228, 158, 90), rgbConvert(130, 91, 48) });
        public Rect skinColorPicker;
        public Rect skinSaturationSlider;
        public Rect skinValueSlider;
        public float skinHue = 1.0f;
        public float skinSaturation = 0.0f;
        public float skinValue = 1.0f;
        public float selectedSkinHue = 1.0f;
        public float selectedSkinSaturation = 0.0f;
        public float selectedSkinValue = 1.0f;

        public Rect btnRaceChangeOutline;
        public Rect btnRaceChangeArrowLeft;
        public Rect btnRaceChangeArrowRight;
        public Rect btnRaceChangeSelection;

        public Rect btnHeadShapeOutline;
        public Rect btnHeadShapeArrowLeft;
        public Rect btnHeadShapeArrowRight;
        public Rect btnHeadShapeSelection;

        public Rect btnBodyShapeOutline;
        public Rect btnBodyShapeArrowLeft;
        public Rect btnBodyShapeArrowRight;
        public Rect btnBodyShapeSelection;

        public Rect btnHairColourOutlinePremade;
        public List<Tuple<Rect, Color>> hairColorButtons;
        static readonly List<Color> humanHairColors = new List<Color>(new Color[] { rgbConvert(51, 51, 51), rgbConvert(79, 71, 66), rgbConvert(64, 51, 38), rgbConvert(77, 51, 26), rgbConvert(90, 58, 32), rgbConvert(132, 83, 47), rgbConvert(193, 146, 85), rgbConvert(237, 202, 156) });
        public Rect hairColorPicker;
        public Rect hairSaturationSlider;
        public Rect hairValueSlider;
        float hairHue = 1.0f;
        float hairSaturation = 0.0f;
        float hairValue = 1.0f;
        public float selectedHairHue = 1.0f;
        public float selectedHairSaturation = 0.0f;
        public float selectedHairValue = 1.0f;

        public Rect btnHairTypeOutline;
        public Rect btnHairTypeArrowLeft;
        public Rect btnHairTypeArrowRight;
        public Rect btnHairTypeSelection;

        public Rect pawnBox;
        public Rect pawnBoxPawn;
        public Rect healthBoxLabel;
        public Rect healthBox;
        public Rect heDiffPrintout;

        public Rect btnAccept;
        public Rect btnCancel;


        public float leftOffset = 20;
        public float topOffset = 50;
        public float optionOffset = 15;

        public float labelWidth = 120;
        public float buttonWidth = 120;
        public float buttonHeight = 30;
        public float buttonOffsetFromText = 10;
        public float buttonOffsetFromButton = 15;

        public float smallButtonOptionWidth = 44;  //for 5 elements   5 on each side + (buttonoptionwidth + offset) * number of buttons
        public float smallButtonOptionWidthHair = 25;  //for 8 elements  (255[OUTLINEWIDTH]-10)/NUM - 5     
        public float pawnBoxSide = 200;
        public float pawnSpacingFromEdge = 5;

        public Texture2D texColor;

        public int hairTypeIndex = 0;
        public int raceTypeIndex = 0;
        public int maleHeadTypeIndex = 0;
        public int femaleHeadTypeIndex = 0;
        public int alienHeadtypeIndex = 0;
        public int maleBodyTypeIndex = 0;
        public int femaleBodyTypeIndex = 0;
        public string headLabel;

        public int ticksToGrow = 900000;
        public int meatCost = 250;
        public int beautyLevel = 0;
        public int qualityLevel = 0;

        //button text subtle copied from Rimworld basecode but with minor changes to fit this UI
        public static bool ButtonTextSubtleCentered(Rect rect, string label, Vector2 functionalSizeOffset = default(Vector2))
        {
            Rect rect2 = rect;
            rect2.width += functionalSizeOffset.x;
            rect2.height += functionalSizeOffset.y;
            bool flag = false;
            if (Mouse.IsOver(rect2))
            {
                flag = true;
                GUI.color = GenUI.MouseoverColor;
            }
            Widgets.DrawAtlas(rect, Widgets.ButtonSubtleAtlas);
            GUI.color = Color.white;
            Rect rect3 = new Rect(rect);
            if (flag)
            {
                rect3.x += 2f;
                rect3.y -= 2f;
            }
            Text.Anchor = TextAnchor.MiddleCenter;
            Text.WordWrap = false;
            Text.Font = GameFont.Small;
            Widgets.Label(rect3, label);
            Text.Anchor = TextAnchor.MiddleLeft;
            Text.WordWrap = true;
            return Widgets.ButtonInvisible(rect2, false);
        }

        public float returnYfromPrevious(Rect rect)
        {
            float y;
            y = rect.y;
            y += rect.height;
            y += optionOffset;

            return y;
        }

        public float returnButtonOffset(Rect rect)
        {
            float y;
            y = rect.width + buttonOffsetFromText;
            return y;
        }

        public List<ThingDef> InitializeExclusionsCache(string field)
        {
            List<ThingDef> excludedRaces = new List<ThingDef>();
            foreach (ThingDef def in DefDatabase<ThingDef>.AllDefs.Where(def => def.category == ThingCategory.Pawn))
            {
                if (def.GetModExtension<ExcludeRacesModExtension>() is ExcludeRacesModExtension props)
                {
                    if (!(bool)typeof(ExcludeRacesModExtension).GetField(field).GetValue(props))
                    {
                        excludedRaces.Add(def);
                    }
                }
            }
            return excludedRaces;
        }

        public CustomizeSleeveWindow(Building_SleeveGrower sleeveGrower)
        {
            if (ModCompatibility.AlienRacesIsActive)
            {
                List<ThingDef> excludedRaces = new List<ThingDef>();
                foreach (ThingDef def in DefDatabase<ThingDef>.AllDefs.Where(def => def.category == ThingCategory.Pawn))
                {
                    if (def.GetModExtension<ExcludeRacesModExtension>() is ExcludeRacesModExtension props)
                    {
                        if (!props.canBeGrown)
                        {
                            excludedRaces.Add(def);
                        }
                    }
                }
                orderedValidAlienRaces = ModCompatibility.GetGrowableRaces(excludedRaces).OrderBy(entry => entry.LabelCap.RawText).ToList();
            }
            this.sleeveGrower = sleeveGrower;
            currentPawnKindDef = PawnKindDefOf.Colonist;
            var gender = Gender.Male;
            if (Rand.Chance(0.5f)) gender = Gender.Female;
            newSleeve = GetNewPawn(gender);
            UpdateGrowingCost();
            UpdateHediffs();
            ApplyBeauty();

            //860x570
            //init Rects
            //gender
            lblGender = new Rect(leftOffset, topOffset, labelWidth, buttonHeight);
            btnGenderMale = lblGender;
            btnGenderMale.width = buttonWidth;
            btnGenderMale.x += returnButtonOffset(lblGender);
            btnGenderFemale = btnGenderMale;
            btnGenderFemale.x += btnGenderMale.width + buttonOffsetFromButton;

            //skin colour
            float lastOffset;
            lblSkinColour = new Rect(leftOffset, returnYfromPrevious(lblGender), labelWidth, buttonHeight);
            btnSkinColourOutlinePremade = lblSkinColour;
            btnSkinColourOutlinePremade.x += returnButtonOffset(lblSkinColour);
            btnSkinColourOutlinePremade.width = buttonWidth * 2 + buttonOffsetFromButton;
            if (ModCompatibility.AlienRacesIsActive)
            {
                skinColorPicker = btnSkinColourOutlinePremade;
                skinColorPicker.y = returnYfromPrevious(btnSkinColourOutlinePremade);
                skinSaturationSlider = skinColorPicker;
                skinSaturationSlider.height = buttonHeight / 2.0f;
                skinSaturationSlider.y = returnYfromPrevious(skinColorPicker);
                skinValueSlider = skinColorPicker;
                skinValueSlider.height = buttonHeight / 2.0f;
                skinValueSlider.y = returnYfromPrevious(skinSaturationSlider);
                lastOffset = returnYfromPrevious(skinValueSlider);
            }
            else
            {
                lastOffset = returnYfromPrevious(btnSkinColourOutlinePremade);
            }
            UpdateSkinColorButtons();

            if (!ModCompatibility.AlienRacesIsActive)
            {
                //head shape
                lblHeadShape = new Rect(leftOffset, lastOffset, labelWidth, buttonHeight);
                btnHeadShapeOutline = lblHeadShape;
                btnHeadShapeOutline.x += returnButtonOffset(lblHeadShape);
                btnHeadShapeOutline.width = buttonWidth * 2 + buttonOffsetFromButton;
                btnHeadShapeArrowLeft = new Rect(btnHeadShapeOutline.x + 2, btnHeadShapeOutline.y, btnHeadShapeOutline.height, btnHeadShapeOutline.height);
                btnHeadShapeArrowRight = new Rect(btnHeadShapeOutline.x + btnHeadShapeOutline.width - btnHeadShapeOutline.height - 2, btnHeadShapeOutline.y, btnHeadShapeOutline.height, btnHeadShapeOutline.height);
                btnHeadShapeSelection = new Rect(btnHeadShapeOutline.x + 5 + btnHeadShapeArrowLeft.width, btnHeadShapeOutline.y, btnHeadShapeOutline.width - 2 * (btnHeadShapeArrowLeft.width) - 10, btnHeadShapeOutline.height);
            }
            else
            {
                //alien race
                lblRaceChange = new Rect(leftOffset, lastOffset, labelWidth, buttonHeight);
                btnRaceChangeOutline = lblRaceChange;
                btnRaceChangeOutline.x += returnButtonOffset(lblRaceChange);
                btnRaceChangeOutline.width = buttonWidth * 2 + buttonOffsetFromButton;
                btnRaceChangeArrowLeft = new Rect(btnRaceChangeOutline.x + 2, btnRaceChangeOutline.y, btnRaceChangeOutline.height, btnRaceChangeOutline.height);
                btnRaceChangeArrowRight = new Rect(btnRaceChangeOutline.x + btnRaceChangeOutline.width - btnRaceChangeOutline.height - 2, btnRaceChangeOutline.y, btnRaceChangeOutline.height, btnRaceChangeOutline.height);
                btnRaceChangeSelection = new Rect(btnRaceChangeOutline.x + 5 + btnRaceChangeArrowLeft.width, btnRaceChangeOutline.y, btnRaceChangeOutline.width - 2 * (btnRaceChangeArrowLeft.width) - 10, btnRaceChangeOutline.height);

                //head shape
                lblHeadShape = new Rect(leftOffset, returnYfromPrevious(lblRaceChange), labelWidth, buttonHeight);
                btnHeadShapeOutline = lblHeadShape;
                btnHeadShapeOutline.x += returnButtonOffset(lblHeadShape);
                btnHeadShapeOutline.width = buttonWidth * 2 + buttonOffsetFromButton;
                btnHeadShapeArrowLeft = new Rect(btnHeadShapeOutline.x + 2, btnHeadShapeOutline.y, btnHeadShapeOutline.height, btnHeadShapeOutline.height);
                btnHeadShapeArrowRight = new Rect(btnHeadShapeOutline.x + btnHeadShapeOutline.width - btnHeadShapeOutline.height - 2, btnHeadShapeOutline.y, btnHeadShapeOutline.height, btnHeadShapeOutline.height);
                btnHeadShapeSelection = new Rect(btnHeadShapeOutline.x + 5 + btnHeadShapeArrowLeft.width, btnHeadShapeOutline.y, btnHeadShapeOutline.width - 2 * (btnHeadShapeArrowLeft.width) - 10, btnHeadShapeOutline.height);
            }


            //body shape
            lblBodyShape = new Rect(leftOffset, returnYfromPrevious(lblHeadShape), labelWidth, buttonHeight);
            btnBodyShapeOutline = lblBodyShape;
            btnBodyShapeOutline.x += returnButtonOffset(lblBodyShape);
            btnBodyShapeOutline.width = buttonWidth * 2 + buttonOffsetFromButton;
            btnBodyShapeArrowLeft = new Rect(btnBodyShapeOutline.x + 2, btnBodyShapeOutline.y, btnBodyShapeOutline.height, btnBodyShapeOutline.height);
            btnBodyShapeArrowRight = new Rect(btnBodyShapeOutline.x + btnBodyShapeOutline.width - btnBodyShapeOutline.height - 2, btnBodyShapeOutline.y, btnBodyShapeOutline.height, btnBodyShapeOutline.height);
            btnBodyShapeSelection = new Rect(btnBodyShapeOutline.x + 5 + btnBodyShapeArrowLeft.width, btnBodyShapeOutline.y, btnBodyShapeOutline.width - 2 * (btnBodyShapeArrowLeft.width) - 10, btnBodyShapeOutline.height);

            //hair colour
            lblHairColour = new Rect(leftOffset, returnYfromPrevious(lblBodyShape), labelWidth, buttonHeight);
            btnHairColourOutlinePremade = lblHairColour;
            btnHairColourOutlinePremade.x += returnButtonOffset(lblHairColour);
            btnHairColourOutlinePremade.width = buttonWidth * 2 + buttonOffsetFromButton;
            hairColorPicker = btnHairColourOutlinePremade;
            hairColorPicker.y = returnYfromPrevious(btnHairColourOutlinePremade);
            hairSaturationSlider = hairColorPicker;
            hairSaturationSlider.height = buttonHeight / 2.0f;
            hairSaturationSlider.y = returnYfromPrevious(hairColorPicker);
            hairValueSlider = hairColorPicker;
            hairValueSlider.height = buttonHeight / 2.0f;
            hairValueSlider.y = returnYfromPrevious(hairSaturationSlider);
            UpdateHairColorButtons();

            //hairtype
            lblHairType = new Rect(leftOffset, returnYfromPrevious(hairValueSlider), labelWidth, buttonHeight);
            btnHairTypeOutline = lblHairType;
            btnHairTypeOutline.x += returnButtonOffset(lblHairType);
            btnHairTypeOutline.width = buttonWidth * 2 + buttonOffsetFromButton;
            btnHairTypeArrowLeft = new Rect(btnHairTypeOutline.x + 2, btnHairTypeOutline.y, btnHairTypeOutline.height, btnHairTypeOutline.height);
            btnHairTypeArrowRight = new Rect(btnHairTypeOutline.x + btnHairTypeOutline.width - btnHairTypeOutline.height - 2, btnHairTypeOutline.y, btnHairTypeOutline.height, btnHairTypeOutline.height);
            btnHairTypeSelection = new Rect(btnHairTypeOutline.x + 5 + btnHairTypeArrowLeft.width, btnHairTypeOutline.y, btnHairTypeOutline.width - 2 * (btnHairTypeArrowLeft.width) - 10, btnHairTypeOutline.height);

            //timetogrow
            lblTimeToGrow = new Rect(leftOffset, returnYfromPrevious(lblHairType), labelWidth * 3, buttonHeight);

            //biomass
            lblRequireBiomass = new Rect(leftOffset, lblTimeToGrow.y + lblTimeToGrow.height, labelWidth * 3, buttonHeight);

            //Pawn
            pawnBox = new Rect(labelWidth + buttonOffsetFromText + buttonWidth * 2 + buttonOffsetFromButton + 60 + leftOffset, topOffset, pawnBoxSide, pawnBoxSide);
            pawnBoxPawn = new Rect(pawnBox.x + pawnSpacingFromEdge, pawnBox.y + pawnSpacingFromEdge, pawnBox.width - pawnSpacingFromEdge * 2, pawnBox.height - pawnSpacingFromEdge * 2);

            //Quality preview
            healthBox = new Rect(pawnBox.x - 15, pawnBox.y + pawnBox.height + 40f, pawnBox.width + 30, 90f);
            healthBoxLabel = new Rect(healthBox.x, healthBox.y - buttonHeight, healthBox.width, buttonHeight);
            heDiffPrintout = healthBox.BottomPart(0.95f).LeftPart(0.95f).RightPart(0.95f);

            //Levels of Beauty
            beautySlider = new Rect(pawnBox.x, healthBox.y + healthBox.height + optionOffset, pawnBox.width, buttonHeight);

            //Levels of Quality
            qualitySlider = new Rect(beautySlider.x, beautySlider.y + buttonHeight, pawnBox.width, buttonHeight);

            //Accept/Cancel buttons
            btnAccept = new Rect(InitialSize.x * .5f - buttonWidth / 2 - buttonOffsetFromButton / 2 - buttonWidth / 2, InitialSize.y - buttonHeight - 38, buttonWidth, buttonHeight);
            btnCancel = new Rect(InitialSize.x * .5f + buttonWidth / 2 + buttonOffsetFromButton / 2 - buttonWidth / 2, InitialSize.y - buttonHeight - 38, buttonWidth, buttonHeight);

            //Create textures
            InitColorPicker();
        }

        string GetQualityLabel()
        {
            if (qualityLevel == -1)
            {
                return QualityCategory.Poor.GetLabel();                
            }
            else if (qualityLevel == 1)
            {
                return QualityCategory.Good.GetLabel();                
            }
            else
            {
                return QualityCategory.Normal.GetLabel();
            }
        }
        string GetBeautyLabel()
        {
            if (beautyLevel != 0)
            {
                Trait beauty = new Trait(TraitDefOf.Beauty, beautyLevel);
                return beauty.LabelCap;
            }
            else
            {
                return QualityCategory.Normal.GetLabel().CapitalizeFirst();
            }
        }
        void UpdateGrowingCost()
        {
            ticksToGrow = (int)((900000 + beautyLevel * 105000 + qualityLevel * 210000) * AlteredCarbonMod.settings.growingTimeModifier);
            meatCost = 250 + beautyLevel * 25 + qualityLevel * 50;
        }
        void UpdateHediffs()
        {
            HediffDef qualityDiff;
            if (qualityLevel == -1)
            {
                qualityDiff = AlteredCarbonDefOf.AC_Sleeve_Quality_Low;
            }
            else if (qualityLevel == 1)
            {
                qualityDiff = AlteredCarbonDefOf.AC_Sleeve_Quality_High;
            }
            else
            {
                qualityDiff = AlteredCarbonDefOf.AC_Sleeve_Quality_Standard;
            }

            RemoveAllHediffs(newSleeve);
            newSleeve.health.AddHediff(qualityDiff, null);
            var comp = sleeveGrower.ActiveBrainTemplate.TryGetComp<CompBrainTemplate>();
            comp.SaveBodyData(newSleeve);
        }

        void ApplyBeauty()
        {
            RemoveAllTraits(newSleeve);
            if (beautyLevel != 0)
            {
                newSleeve.story.traits.GainTrait(new Trait(TraitDefOf.Beauty, beautyLevel));
            }
        }

        static string GetHediffToolTip(IEnumerable<Hediff> diffs, Pawn pawn)
        {
            MethodInfo tooltipGetter = typeof(HealthCardUtility).GetMethod("GetTooltip", BindingFlags.NonPublic | BindingFlags.Static);
            return (string)tooltipGetter.Invoke(null, new object[] { diffs, pawn, null });
        }
        public static Color rgbConvert(float r, float g, float b)
        {
            return new Color(((1f / 255f) * r), ((1f / 255f) * g), ((1f / 255f) * b));
        }
        public void InitColorPicker()
        {
            //Create textures
            texColor = new Texture2D(Convert.ToInt32(buttonWidth * 2 + buttonOffsetFromButton), Convert.ToInt32(buttonHeight));
            for (int x = 0; x < texColor.width; x++)
            {
                for (int y = 0; y < texColor.height; y++)
                {
                    texColor.SetPixel(x,y,Color.HSVToRGB(((float)x)/texColor.width, 1.0f, 1.0f));
                }
            }
            texColor.Apply(false);
        }

        public void UpdateSkin()
        {
            if (selectedSkinHue != skinHue
                || selectedSkinSaturation != skinSaturation
                || selectedSkinValue != skinValue)
            {
                Color color = Color.HSVToRGB(skinHue, skinSaturation, skinValue);
                selectedSkinHue = skinHue;
                selectedSkinSaturation = skinSaturation;
                selectedSkinValue = skinValue;
                ModCompatibility.SetSkinColor(newSleeve, color);
                refreshAndroidPortrait = true;
            }
        }
        public void UpdateHair()
        {
            if (selectedHairHue != hairHue
                || selectedHairSaturation != hairSaturation
                || selectedHairValue != hairValue)
            {
                Color color = Color.HSVToRGB(hairHue, hairSaturation, hairValue);
                selectedHairHue = hairHue;
                selectedHairSaturation = hairSaturation;
                selectedHairValue = hairValue;
                newSleeve.story.hairColor = color;
                refreshAndroidPortrait = true;
            }
        }

        public void UpdateSleeveGraphic()
        {
            newSleeve.Drawer.renderer.graphics.ResolveAllGraphics();
            PortraitsCache.SetDirty(newSleeve);
            PortraitsCache.PortraitsCacheUpdate();
        }

        public void UpdateSkinColorButtons()
        {
            //fetch colors from HAR defs where possible, fall back on inbuilt colors otherwise
            //incompatible with pre-1.2 style color channel Defs
            List<Color> skinColors = null;
            if (ModCompatibility.AlienRacesIsActive)
            {
                skinColors = ModCompatibility.GetRacialColorPresets(newSleeve.kindDef.race, "skin");
            }
            if (skinColors.NullOrEmpty())
            {
                skinColors = humanSkinColors;
            }            
            skinColorButtons = new List<Tuple<Rect, Color>>();
            float xOffset = btnSkinColourOutlinePremade.x;
            float yOffset = btnSkinColourOutlinePremade.y + 5;
            float buttonWidth = Math.Min(btnSkinColourOutlinePremade.width / skinColors.Count(), smallButtonOptionWidth);
            for (int ii = 0; ii < skinColors.Count(); ++ii)
            {
                skinColorButtons.Add(new Tuple<Rect, Color>(new Rect(xOffset + buttonWidth * ii, yOffset, buttonWidth, btnSkinColourOutlinePremade.height - 10), skinColors[ii]));
            }
            for (int ii = 0; ii < skinColorButtons.Count(); ++ii)
            {
                GUI.DrawTexture(GenUI.ExpandedBy(skinColorButtons[ii].Item1, 2f), BaseContent.GreyTex);
                Widgets.DrawBoxSolid(skinColorButtons[ii].Item1, skinColorButtons[ii].Item2);
            }
        }

        public void UpdateHairColorButtons()
        {
            //fetch colors from HAR defs where possible, fall back on inbuilt colors otherwise
            //incompatible with pre-1.2 style color channel Defs
            List<Color> hairColors = null;
            if (ModCompatibility.AlienRacesIsActive)
            {
                hairColors = ModCompatibility.GetRacialColorPresets(newSleeve.kindDef.race, "hair");
            }
            if (hairColors.NullOrEmpty())
            {
                hairColors = humanHairColors;
            }
            hairColorButtons = new List<Tuple<Rect, Color>>();
            float xOffset = btnHairColourOutlinePremade.x;
            float yOffset = btnHairColourOutlinePremade.y + 5;
            float buttonWidth = Math.Min(btnHairColourOutlinePremade.width / hairColors.Count(), smallButtonOptionWidth);
            for (int ii = 0; ii < hairColors.Count(); ++ii)
            {
                hairColorButtons.Add(new Tuple<Rect, Color>(new Rect(xOffset + buttonWidth * ii, yOffset, buttonWidth, btnHairColourOutlinePremade.height - 10), hairColors[ii]));
            }
            for (int ii = 0; ii < hairColorButtons.Count(); ++ii)
            {
                GUI.DrawTexture(GenUI.ExpandedBy(hairColorButtons[ii].Item1, 2f), BaseContent.GreyTex);
                Widgets.DrawBoxSolid(hairColorButtons[ii].Item1, hairColorButtons[ii].Item2);
            }
        }

        private static readonly string[] HeadsFolderPaths = new string[2]
        {
            "Things/Pawn/Humanlike/Heads/Male",
            "Things/Pawn/Humanlike/Heads/Female"
        };

        public void RemoveAllTraits(Pawn pawn)
        {
            if (pawn.story != null)
            {
                pawn.story.traits = new TraitSet(pawn);
            }
        }

        public void RemoveAllHediffs(Pawn pawn)
        {
            pawn.health = new Pawn_HealthTracker(pawn);
        }

        public string ExtractHeadLabels(string path)
        {
            string str = Regex.Replace(path, ".*/[A-Z]+?_", "", RegexOptions.IgnoreCase);
            return str;
        }
        public override void DoWindowContents(Rect inRect)
        {
            //Detect changes
            if (refreshAndroidPortrait)
            {
                newSleeve.Drawer.renderer.graphics.ResolveAllGraphics();
                PortraitsCache.SetDirty(newSleeve);
                PortraitsCache.PortraitsCacheUpdate();
                refreshAndroidPortrait = false;
            }

            //Draw Pawn stuff.
            if (newSleeve != null)
            {


                Text.Font = GameFont.Medium;

                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(new Rect(0f, 0f, inRect.width, 32f), "AlteredCarbon.SleeveCustomization".Translate());

                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.MiddleCenter;

                //Saakra's Code

                //Gender
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(lblGender, "Gender".Translate() + ":");
                if (allowMales && Widgets.ButtonText(btnGenderMale, "Male".Translate().CapitalizeFirst()))
                {
                    newSleeve = GetNewPawn(Gender.Male);
                    UpdateHediffs();
                    ApplyBeauty();
                }
                if (allowFemales && Widgets.ButtonText(btnGenderFemale, "Female".Translate().CapitalizeFirst()))
                {
                    newSleeve = GetNewPawn(Gender.Female);
                    UpdateHediffs();
                    ApplyBeauty();
                }

                if (ModCompatibility.AlienRacesIsActive)
                {
                    //Alien races
                    Widgets.Label(lblRaceChange, "SelectRace".Translate().CapitalizeFirst() + ":");
                    Widgets.DrawHighlight(btnRaceChangeOutline);
                    if (ButtonTextSubtleCentered(btnRaceChangeArrowLeft, "<"))
                    {
                        if (raceTypeIndex == 0)
                        {
                            raceTypeIndex = orderedValidAlienRaces.Count() - 1;
                        }
                        else
                        {
                            raceTypeIndex--;
                        }
                        currentPawnKindDef.race = orderedValidAlienRaces.ElementAt(raceTypeIndex); ;
                        newSleeve = GetNewPawn(newSleeve.gender);
                        UpdateSleeveGraphic();
                        UpdateSkinColorButtons();
                        UpdateHairColorButtons();
                        UpdateHediffs();
                        ApplyBeauty();
                    }
                    if (ButtonTextSubtleCentered(btnRaceChangeSelection, currentPawnKindDef.race.LabelCap))
                    {
                        FloatMenuUtility.MakeMenu<ThingDef>(orderedValidAlienRaces, raceDef => raceDef.LabelCap, (ThingDef raceDef) => delegate
                        {
                            currentPawnKindDef.race = raceDef;
                            newSleeve = GetNewPawn(newSleeve.gender);
                            UpdateSleeveGraphic();
                            UpdateSkinColorButtons();
                            UpdateHairColorButtons();
                            UpdateHediffs();
                            ApplyBeauty();
                        });
                    }
                    if (ButtonTextSubtleCentered(btnRaceChangeArrowRight, ">"))
                    {
                        if (raceTypeIndex == orderedValidAlienRaces.Count() - 1)
                        {
                            raceTypeIndex = 0;
                        }
                        else
                        {
                            raceTypeIndex++;
                        }
                        currentPawnKindDef.race = orderedValidAlienRaces.ElementAt(raceTypeIndex); ;
                        newSleeve = GetNewPawn(newSleeve.gender);
                        UpdateSleeveGraphic();
                        UpdateSkinColorButtons();
                        UpdateHairColorButtons();
                        UpdateHediffs();
                        ApplyBeauty();
                    }
                }

                //Skin Colour
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(lblSkinColour, "SkinColour".Translate().CapitalizeFirst() + ":");

                if (ModCompatibility.AlienRacesIsActive)
                {
                    Widgets.DrawMenuSection(skinColorPicker);
                    Widgets.DrawTextureFitted(skinColorPicker, texColor, 1);
                    skinSaturation = Widgets.HorizontalSlider(skinSaturationSlider, skinSaturation, 0.0f, 1f, true, "saturation");
                    skinValue = Widgets.HorizontalSlider(skinValueSlider, skinValue, 0.0f, 1f, true, "value");

                    //if click in texColour box
                    if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && Mouse.IsOver(skinColorPicker))
                    {
                        Vector2 mPos = Event.current.mousePosition;
                        float x = mPos.x - skinColorPicker.x;
                        float y = mPos.y - skinColorPicker.y;
                        Color pick = texColor.GetPixel(Convert.ToInt32(x), Convert.ToInt32(skinColorPicker.height - y));
                        Color.RGBToHSV(pick, out skinHue, out _, out _);
                        Event.current.Use();
                    }
                }

                for (int ii = 0; ii < skinColorButtons.Count(); ++ii)
                {
                    GUI.DrawTexture(GenUI.ExpandedBy(skinColorButtons[ii].Item1, 2f), BaseContent.GreyTex);
                    if (Widgets.ButtonInvisible(skinColorButtons[ii].Item1))
                    {
                        if (ModCompatibility.AlienRacesIsActive)
                        {
                            Color.RGBToHSV(skinColorButtons[ii].Item2, out skinHue, out skinSaturation, out skinValue);
                        }
                        else
                        {
                            newSleeve.story.melanin = 0.1f + ii * 0.2f;
                        }
                        UpdateSleeveGraphic();
                    }
                    Widgets.DrawBoxSolid(skinColorButtons[ii].Item1, skinColorButtons[ii].Item2);
                }
                UpdateSkin();

                //Head Shape
                Widgets.Label(lblHeadShape, "HeadShape".Translate().CapitalizeFirst() + ":");

                if (ModCompatibility.AlienRacesIsActive)
                {
                    headLabel = ModCompatibility.GetAlienHead(newSleeve);
                }
                else
                {
                    headLabel = newSleeve.story.HeadGraphicPath.Split('/').Last();
                }
                Widgets.DrawHighlight(btnHeadShapeOutline);
                if (ButtonTextSubtleCentered(btnHeadShapeArrowLeft, "<"))
                {
                    if (ModCompatibility.AlienRacesIsActive)
                    {
                        var list = ModCompatibility.GetAlienHeadPaths(newSleeve);
                        if (alienHeadtypeIndex == 0)
                        {
                            alienHeadtypeIndex = list.Count - 1;
                        }
                        else
                        {
                            alienHeadtypeIndex--;
                        }
                        ModCompatibility.SetAlienHead(newSleeve, list[alienHeadtypeIndex]);
                    }
                    else
                    {
                        if (newSleeve.gender == Gender.Male)
                        {
                            if (maleHeadTypeIndex == 0)
                            {
                                maleHeadTypeIndex = GraphicDatabaseHeadRecords.maleHeads.Count - 1;
                            }
                            else
                            {
                                maleHeadTypeIndex--;
                            }
                            typeof(Pawn_StoryTracker).GetField("headGraphicPath", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(newSleeve.story,
                                GraphicDatabaseHeadRecords.maleHeads.ElementAt(maleHeadTypeIndex).graphicPath);
                        }
                        else if (newSleeve.gender == Gender.Female)
                        {
                            if (femaleHeadTypeIndex == 0)
                            {
                                femaleHeadTypeIndex = GraphicDatabaseHeadRecords.femaleHeads.Count - 1;
                            }
                            else
                            {
                                femaleHeadTypeIndex--;
                            }

                            typeof(Pawn_StoryTracker).GetField("headGraphicPath", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(newSleeve.story,
                                GraphicDatabaseHeadRecords.femaleHeads.ElementAt(femaleHeadTypeIndex).graphicPath);
                        }
                    }
                    UpdateSleeveGraphic();
                }
                if (ButtonTextSubtleCentered(btnHeadShapeSelection, headLabel))
                {
                    if (ModCompatibility.AlienRacesIsActive)
                    {
                        var list = ModCompatibility.GetAlienHeadPaths(newSleeve);
                        FloatMenuUtility.MakeMenu<string>(list, head => head,
                        (string head) => delegate
                        {
                            ModCompatibility.SetAlienHead(newSleeve, head);
                            UpdateSleeveGraphic();
                        });
                    }
                    else
                    {
                        if (newSleeve.gender == Gender.Male)
                        {
                            FloatMenuUtility.MakeMenu<GraphicDatabaseHeadRecords.HeadGraphicRecord>(GraphicDatabaseHeadRecords.maleHeads, head => ExtractHeadLabels(head.graphicPath),
                            (GraphicDatabaseHeadRecords.HeadGraphicRecord head) => delegate
                            {
                                typeof(Pawn_StoryTracker).GetField("headGraphicPath", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(newSleeve.story,
                                head.graphicPath);
                                UpdateSleeveGraphic();
                            });
                        }
                        else if (newSleeve.gender == Gender.Female)
                        {
                            FloatMenuUtility.MakeMenu<GraphicDatabaseHeadRecords.HeadGraphicRecord>(GraphicDatabaseHeadRecords.femaleHeads, head => ExtractHeadLabels(head.graphicPath),
                                (GraphicDatabaseHeadRecords.HeadGraphicRecord head) => delegate
                                {
                                    typeof(Pawn_StoryTracker).GetField("headGraphicPath", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(newSleeve.story,
                                head.graphicPath);
                                    UpdateSleeveGraphic();
                                });
                        }
                    }
                }
                if (ButtonTextSubtleCentered(btnHeadShapeArrowRight, ">"))
                {
                    if (ModCompatibility.AlienRacesIsActive)
                    {
                        var list = ModCompatibility.GetAlienHeadPaths(newSleeve);

                        if (alienHeadtypeIndex == list.Count - 1)
                        {
                            alienHeadtypeIndex = 0;
                        }
                        else
                        {
                            alienHeadtypeIndex++;
                        }
                        ModCompatibility.SetAlienHead(newSleeve, list[alienHeadtypeIndex]);
                    }
                    else
                    {
                        if (newSleeve.gender == Gender.Male)
                        {
                            if (maleHeadTypeIndex == GraphicDatabaseHeadRecords.maleHeads.Count - 1)
                            {
                                maleHeadTypeIndex = 0;
                            }
                            else
                            {
                                maleHeadTypeIndex++;
                            }
                            typeof(Pawn_StoryTracker).GetField("headGraphicPath", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(newSleeve.story,
                                GraphicDatabaseHeadRecords.maleHeads.ElementAt(maleHeadTypeIndex).graphicPath);
                        }
                        else if (newSleeve.gender == Gender.Female)
                        {
                            if (femaleHeadTypeIndex == GraphicDatabaseHeadRecords.femaleHeads.Count - 1)
                            {
                                femaleHeadTypeIndex = 0;
                            }
                            else
                            {
                                femaleHeadTypeIndex++;
                            }
                            typeof(Pawn_StoryTracker).GetField("headGraphicPath", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(newSleeve.story,
                                GraphicDatabaseHeadRecords.femaleHeads.ElementAt(femaleHeadTypeIndex).graphicPath);
                        }
                    }
                    UpdateSleeveGraphic();
                }


                //Body Shape
                Widgets.Label(lblBodyShape, "BodyShape".Translate().CapitalizeFirst() + ":");
                Widgets.DrawHighlight(btnBodyShapeOutline);
                if (ButtonTextSubtleCentered(btnBodyShapeArrowLeft, "<"))
                {
                    if (newSleeve.gender == Gender.Male)
                    {
                        if (maleBodyTypeIndex == 0)
                        {
                            maleBodyTypeIndex = DefDatabase<BodyTypeDef>.AllDefs.Where(x => x != BodyTypeDefOf.Female).Count() - 1;
                        }
                        else
                        {
                            maleBodyTypeIndex--;
                        }
                        newSleeve.story.bodyType = DefDatabase<BodyTypeDef>.AllDefs.Where(x => x != BodyTypeDefOf.Female).ElementAt(maleBodyTypeIndex);
                    }
                    else if (newSleeve.gender == Gender.Female)
                    {
                        if (femaleBodyTypeIndex == 0)
                        {
                            femaleBodyTypeIndex = DefDatabase<BodyTypeDef>.AllDefs.Where(x => x != BodyTypeDefOf.Male).Count() - 1;
                        }
                        else
                        {
                            femaleBodyTypeIndex--;
                        }
                        newSleeve.story.bodyType = DefDatabase<BodyTypeDef>.AllDefs.Where(x => x != BodyTypeDefOf.Male).ElementAt(femaleBodyTypeIndex);
                    }

                    UpdateSleeveGraphic();
                }
                if (ButtonTextSubtleCentered(btnBodyShapeSelection, newSleeve.story.bodyType.defName))
                {
                    if (newSleeve.gender == Gender.Male)
                    {
                        IEnumerable<BodyTypeDef> bodyTypes = from bodyType in DefDatabase<BodyTypeDef>
                            .AllDefs.Where(x => x != BodyTypeDefOf.Female) select bodyType;
                        FloatMenuUtility.MakeMenu<BodyTypeDef>(bodyTypes, bodyType => bodyType.defName, (BodyTypeDef bodyType) => delegate
                        {
                            newSleeve.story.bodyType = bodyType;
                            UpdateSleeveGraphic();
                        });
                    }
                    else if (newSleeve.gender == Gender.Female)
                    {
                        IEnumerable<BodyTypeDef> bodyTypes = from bodyType in DefDatabase<BodyTypeDef>
                            .AllDefs.Where(x => x != BodyTypeDefOf.Male)
                                                             select bodyType;
                        FloatMenuUtility.MakeMenu<BodyTypeDef>(bodyTypes, bodyType => bodyType.defName, (BodyTypeDef bodyType) => delegate
                        {
                            newSleeve.story.bodyType = bodyType;
                            UpdateSleeveGraphic();
                        });
                    }
                }
                if (ButtonTextSubtleCentered(btnBodyShapeArrowRight, ">"))
                {
                    if (newSleeve.gender == Gender.Male)
                    {
                        if (maleBodyTypeIndex == DefDatabase<BodyTypeDef>.AllDefs.Where(x => x != BodyTypeDefOf.Female).Count() - 1)
                        {
                            maleBodyTypeIndex = 0;
                        }
                        else
                        {
                            maleBodyTypeIndex++;
                        }
                        newSleeve.story.bodyType = DefDatabase<BodyTypeDef>.AllDefs.Where(x => x != BodyTypeDefOf.Female).ElementAt(maleBodyTypeIndex);
                    }
                    else if (newSleeve.gender == Gender.Female)
                    {
                        if (femaleBodyTypeIndex == DefDatabase<BodyTypeDef>.AllDefs.Where(x => x != BodyTypeDefOf.Male).Count() - 1)
                        {
                            femaleBodyTypeIndex = 0;
                        }
                        else
                        {
                            femaleBodyTypeIndex++;
                        }
                        newSleeve.story.bodyType = DefDatabase<BodyTypeDef>.AllDefs.Where(x => x != BodyTypeDefOf.Male).ElementAt(femaleBodyTypeIndex);
                    }
                    UpdateSleeveGraphic();
                }


                //Hair Colour
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(lblHairColour, "HairColour".Translate().CapitalizeFirst() + ":");
                Widgets.DrawMenuSection(hairColorPicker);
                Widgets.DrawTextureFitted(hairColorPicker, texColor, 1);
                hairSaturation = Widgets.HorizontalSlider(hairSaturationSlider, hairSaturation, 0.0f, 1f, true, "saturation");
                hairValue = Widgets.HorizontalSlider(hairValueSlider, hairValue, 0.0f, 1f, true, "value");

                //if click in texColour box
                if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && Mouse.IsOver(hairColorPicker))
                {
                    Vector2 mPos = Event.current.mousePosition;
                    float x = mPos.x - hairColorPicker.x;
                    float y = mPos.y - hairColorPicker.y;
                    Color pick = texColor.GetPixel(Convert.ToInt32(x), Convert.ToInt32(hairColorPicker.height - y));
                    Color.RGBToHSV(pick, out hairHue, out _, out _);
                    Event.current.Use();
                }
                for (int ii = 0; ii < hairColorButtons.Count(); ++ii)
                {
                    GUI.DrawTexture(GenUI.ExpandedBy(hairColorButtons[ii].Item1, 2f), BaseContent.GreyTex);
                    if (Widgets.ButtonInvisible(hairColorButtons[ii].Item1))
                    {
                        Color.RGBToHSV(hairColorButtons[ii].Item2, out hairHue, out hairSaturation, out hairValue);
                    }
                    Widgets.DrawBoxSolid(hairColorButtons[ii].Item1, hairColorButtons[ii].Item2);
                }
                UpdateHair();

                //Hair Type
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(lblHairType, "HairType".Translate().CapitalizeFirst() + ":");
                Widgets.DrawHighlight(btnHairTypeOutline);
                if (ButtonTextSubtleCentered(btnHairTypeArrowLeft, "<"))
                {
                    if (hairTypeIndex == 0)
                    {
                        hairTypeIndex = permittedHair.Count() - 1;
                    }
                    else
                    {
                        hairTypeIndex--;
                    }
                    newSleeve.story.hairDef = permittedHair.ElementAt(hairTypeIndex);
                    UpdateSleeveGraphic();
                }
                if (ButtonTextSubtleCentered(btnHairTypeSelection, newSleeve.story.hairDef.LabelCap))
                {
                    IEnumerable<HairDef> hairs =
                        from hairdef in permittedHair select hairdef;
                    FloatMenuUtility.MakeMenu<HairDef>(hairs, hairDef => hairDef.LabelCap, (HairDef hairDef) => delegate
                    {
                        newSleeve.story.hairDef = hairDef;
                        UpdateSleeveGraphic();
                    });
                }
                if (ButtonTextSubtleCentered(btnHairTypeArrowRight, ">"))
                {
                    if (hairTypeIndex == permittedHair.Count() - 1)
                    {
                        hairTypeIndex = 0;
                    }
                    else
                    {
                        hairTypeIndex++;
                    }
                    newSleeve.story.hairDef = permittedHair.ElementAt(hairTypeIndex);
                    UpdateSleeveGraphic();
                }
                
                //Time to Grow
                Widgets.Label(lblTimeToGrow, "TimeToGrow".Translate().CapitalizeFirst() + ": " + GenDate.ToStringTicksToDays(ticksToGrow));//PUT TIME TO GROW INFO HERE

                //Require Biomass
                Widgets.Label(lblRequireBiomass, "RequireBiomass".Translate().CapitalizeFirst() + ": " + (meatCost));//PUT REQUIRED BIOMASS HERE

                //Vertical Divider
                //Widgets.DrawLineVertical((pawnBox.x + (btnGenderFemale.x + btnGenderFemale.width)) / 2, pawnBox.y, InitialSize.y - pawnBox.y - (buttonHeight + 53));

                //Pawn Box
                Widgets.DrawMenuSection(pawnBox);
                Widgets.DrawShadowAround(pawnBox);
                GUI.DrawTexture(pawnBoxPawn, PortraitsCache.Get(newSleeve, pawnBoxPawn.size, default(Vector3), 1f));
                Widgets.InfoCardButton(pawnBox.x + pawnBox.width - Widgets.InfoCardButtonSize - 10f, pawnBox.y + pawnBox.height - Widgets.InfoCardButtonSize - 10f, newSleeve);

                //Levels of Beauty
                Text.Anchor = TextAnchor.MiddleCenter;
                int newBeautyLevel = (int)Widgets.HorizontalSlider(beautySlider, beautyLevel, -2f, 2f, true, StatDefOf.Beauty.LabelCap + " : " + GetBeautyLabel(), null, null, 1f);
                if(newBeautyLevel != beautyLevel)
                {
                    beautyLevel = newBeautyLevel;
                    ApplyBeauty();
                    UpdateGrowingCost();
                }

                //Levels of Quality
                int newQualityLevel = (int)Widgets.HorizontalSlider(qualitySlider, qualityLevel, -1f, 1f, true, "Quality".Translate() + " : " + GetQualityLabel(), null, null, 1f);
                if (newQualityLevel != qualityLevel)
                {
                    qualityLevel = newQualityLevel;
                    UpdateHediffs();
                    UpdateGrowingCost();
                }

                //Hediff printout
                IEnumerable<IGrouping<BodyPartRecord, Hediff>> heDiffListing;
                MethodInfo heDiffLister = typeof(HealthCardUtility).GetMethod("VisibleHediffGroupsInOrder", BindingFlags.NonPublic | BindingFlags.Static);
                heDiffListing = (IEnumerable<IGrouping<BodyPartRecord, Hediff>>)heDiffLister.Invoke(null, new object[] { newSleeve, false });
                List<Hediff> diffs = heDiffListing.SelectMany(group => group).ToList();
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(healthBoxLabel, "AlteredCarbon.sleeveHealthPreview".Translate().CapitalizeFirst());
                Widgets.DrawHighlight(healthBox);
                GUI.color = HealthUtility.GoodConditionColor;
                Listing_Standard diffListing = new Listing_Standard();
                diffListing.Begin(heDiffPrintout);
                Text.Anchor = TextAnchor.MiddleLeft;
                for (int ii = 0; ii < diffs.Count; ++ii)
                {
                    diffListing.Label(diffs[ii].LabelCap);
                }
                diffListing.End();
                GUI.color = Color.white;
                if (Mouse.IsOver(healthBox))
                {
                    Widgets.DrawHighlight(healthBox);
                    TooltipHandler.TipRegion(healthBox, new TipSignal((Func<string>)(() => GetHediffToolTip(diffs, newSleeve)), 1147682));
                }

                if (Widgets.ButtonText(btnAccept, "Accept".Translate().CapitalizeFirst()))
                {
                    sleeveGrower.StartGrowth(newSleeve, ticksToGrow, meatCost);
                    this.Close();
                }
                if (Widgets.ButtonText(btnCancel, "Cancel".Translate().CapitalizeFirst()))
                {
                    this.Close();
                }
            }
            Text.Anchor = TextAnchor.UpperLeft;
        }

        public Pawn GetNewPawn(Gender gender)
        {
            if (ModCompatibility.AlienRacesIsActive)
            {
                ModCompatibility.UpdateGenderRestrictions(currentPawnKindDef.race, out allowMales, out allowFemales);
                if (gender == Gender.Male && !allowMales)
                {
                    gender = Gender.Female;
                }
                if (gender == Gender.Female && !allowFemales)
                {
                    gender = Gender.Male;
                }
            }
            maleBodyTypeIndex = 0;
            femaleBodyTypeIndex = 0;
            hairTypeIndex = 0;
            femaleHeadTypeIndex = 0;
            maleHeadTypeIndex = 0;
            alienHeadtypeIndex = 0;
            Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(currentPawnKindDef, Faction.OfAncients, PawnGenerationContext.NonPlayer,
            -1, true, false, false, false, false, false, 0f, false, true, true, false, false, false, true, fixedGender: gender, fixedBiologicalAge: 20, 
            fixedChronologicalAge: 20));
            pawn.story.childhood = null;
            pawn.story.adulthood = null;
            pawn.Name = new NameSingle("AlteredCarbon.EmptySleeve".Translate());
            pawn?.equipment.DestroyAllEquipment();
            pawn?.inventory.DestroyAll();
            pawn.apparel.DestroyAll();
            RemoveAllTraits(pawn);
            if (pawn.playerSettings == null) pawn.playerSettings = new Pawn_PlayerSettings(pawn);
            pawn.playerSettings.medCare = MedicalCareCategory.Best;
            pawn.skills = new Pawn_SkillTracker(pawn);
            pawn.needs = new Pawn_NeedsTracker(pawn);
            pawn.workSettings = new Pawn_WorkSettings(pawn);
            pawn.needs.mood.thoughts = new ThoughtHandler(pawn);
            pawn.timetable = new Pawn_TimetableTracker(pawn);
            if (BackstoryDatabase.TryGetWithIdentifier("AC_VatGrown45", out Backstory bs))
            {
                pawn.story.childhood = bs;
            }

            if (pawn.needs?.mood?.thoughts?.memories?.Memories != null)
            {
                for (int num = pawn.needs.mood.thoughts.memories.Memories.Count - 1; num >= 0; num--)
                {
                    pawn.needs.mood.thoughts.memories.RemoveMemory(pawn.needs.mood.thoughts.memories.Memories[num]);
                }
            }
            RemoveAllHediffs(pawn);

            if (pawn.workSettings != null)
            {
                pawn.workSettings.EnableAndInitialize();
            }

            if (pawn.skills != null)
            {
                pawn.skills.Notify_SkillDisablesChanged();
            }
            if (!pawn.Dead && pawn.RaceProps.Humanlike)
            {
                pawn.needs.mood.thoughts.situational.Notify_SituationalThoughtsDirty();
            }
            //align sliders
            Color.RGBToHSV(pawn.story.hairColor, out hairHue, out hairSaturation, out hairValue);
            if (ModCompatibility.AlienRacesIsActive)
            {
                Color.RGBToHSV(ModCompatibility.GetSkinColor(pawn), out skinHue, out skinSaturation, out skinValue);
                permittedHair = ModCompatibility.GetPermittedHair(currentPawnKindDef.race);
            }
            else
            {
                permittedHair = DefDatabase<HairDef>.AllDefs.ToList();
            }
            
            return pawn;
        }
    }
}
