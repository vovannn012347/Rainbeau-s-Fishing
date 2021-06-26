using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RBB
{
    public class ITab_Fishing_Timer_Tab : ITab
    {
        public static bool HasCopy = false;
        public static float FishingDurationCopy = 1;
        public static float FishingFrequencyCopy = 1;

        [TweakValue("Interface", 0f, 128f)]
        private static float PasteX = 48f;

        [TweakValue("Interface", 0f, 128f)]
        private static float PasteY = 3f;

        [TweakValue("Interface", 0f, 32f)]
        private static float PasteSize = 24f;


        [TweakValue("Interface", 0f, 128f)]
        private static float CopyX = 72f;

        [TweakValue("Interface", 0f, 128f)]
        private static float CopyY = 3f;

        [TweakValue("Interface", 0f, 32f)]
        private static float CopySize = 24f;

        const string Period1Hour = "Period1Hour";
        const string PeriodHours = "PeriodHours";

        const float xLeft = 20;
        const float width = 200;

        public ITab_Fishing_Timer_Tab()
        {
            size = new Vector2(240f, 184f);
            labelKey = ResourceBank.Strings.TabFishingTimer;
            tutorTag = ResourceBank.Strings.FishingTimerTag;
        }

        protected override void FillTab()
        {
            if (this.SelThing is Building_FishingSpot spot)
            {
                float y = 20;
                Rect label1 = new Rect(xLeft, y, width, 32f).ContractedBy(4f);
                y += 32;
                Rect slider1 = new Rect(xLeft, y, width, 32f).ContractedBy(4f);
                y += 60;
                Rect label2 = new Rect(xLeft, y, width, 32f).ContractedBy(4f);
                y += 32;
                Rect slider2 = new Rect(xLeft, y, width, 32f).ContractedBy(4f);

                Rect rectPaste = new Rect(size.x - PasteX, PasteY, PasteSize, PasteSize);
                Rect rectCopy = new Rect(size.x - CopyX, CopyY, CopySize, CopySize);

                if (HasCopy)
                {
                    if (Widgets.ButtonImageFitted(rectPaste, ResourceBank.Paste, Color.white))
                    {
                        spot.FishingDuration = FishingDurationCopy;
                        spot.FishingFrequency = FishingFrequencyCopy;
                        SoundDefOf.Tick_Low.PlayOneShotOnCamera();
                    }
                }
                else
                {
                    GUI.color = Color.gray;
                    Widgets.DrawTextureFitted(rectPaste, ResourceBank.Paste, 1f);
                    GUI.color = Color.white;
                }
                TooltipHandler.TipRegionByKey(rectPaste, ResourceBank.Strings.PasteTip);

                if (Widgets.ButtonImageFitted(rectCopy, ResourceBank.Copy, Color.white))
                {
                    HasCopy = true;
                    FishingDurationCopy = spot.FishingDuration;
                    FishingFrequencyCopy = spot.FishingFrequency;
                    SoundDefOf.Tick_Low.PlayOneShotOnCamera();
                }
                TooltipHandler.TipRegionByKey(rectCopy, ResourceBank.Strings.CopyTip);

                //duration setting
                Text.Font = GameFont.Small;
                Widgets.Label(label1, ResourceBank.Strings.FishingDuration.Translate());

                Text.Font = GameFont.Small;
                spot.FishingDuration = Widgets.HorizontalSlider(slider1, spot.FishingDuration, 1f, 24f, true, (int)spot.FishingDuration == 1 ? Period1Hour.Translate() : PeriodHours.Translate(spot.FishingDuration.ToString("0.0")));

                //delay setting
                Text.Font = GameFont.Small;
                Widgets.Label(label2, ResourceBank.Strings.FishingFrequency.Translate());

                Text.Font = GameFont.Small;
                spot.FishingFrequency = Widgets.FrequencyHorizontalSlider(slider2, spot.FishingFrequency, 0.1f, 25f, roundToInt: true);
            }
            else
            {
                DrawTabNothing();
            }
        }

        private void DrawTabNothing()
        {
            Text.Anchor = TextAnchor.MiddleCenter;
            GUI.color = Color.grey;
            Widgets.Label(new Rect(0f, 0f, size.x, size.y), "(" + "UnknownLower".Translate() + ")");
            Text.Anchor = TextAnchor.UpperLeft;
            GUI.color = Color.white;
        }
    }
}