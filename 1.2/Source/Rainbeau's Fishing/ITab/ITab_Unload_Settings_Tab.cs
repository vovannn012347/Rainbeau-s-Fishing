using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RBB
{
    public class ITab_Unload_Settings_Tab : ITab
    {
        public static bool HasCopy = false;
        public static BillStoreModeDef StoreModeCopy = null;
        public static float OpenDelayCopy = 1;

        //[TweakValue("Interface", 0f, 400f)]
        private static int StoreModeSubdialogHeight = 30;
        
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

        private const string StockpileString = "Stockpile";

        const float xLeft = 20;
        const float width = 200;

        public ITab_Unload_Settings_Tab()
        {
            size = new Vector2(240f, 184f);
            labelKey = ResourceBank.Strings.TabUnloadTimer;
            tutorTag = ResourceBank.Strings.PotUnloadTimerTag;
        }

        protected override void FillTab()
        {
            if (this.SelThing is Building_ShellfishTrap trap)
            {
                float y = 20;
                Rect label1 = new Rect(xLeft, y, width, 32f).ContractedBy(4f);
                y += 32;
                Rect storage = new Rect(xLeft, y, width, 40f);
                y += 60;
                Rect label2 = new Rect(xLeft, y, width, 32f).ContractedBy(4f);
                y += 32;
                Rect slider = new Rect(xLeft, y, width, 32f).ContractedBy(4f);

                Rect rectPaste = new Rect(size.x - PasteX, PasteY, PasteSize, PasteSize);
                Rect rectCopy = new Rect(size.x - CopyX, CopyY, CopySize, CopySize);

                if (HasCopy)
                {
                    if (Widgets.ButtonImageFitted(rectPaste, ResourceBank.Paste, Color.white))
                    {
                        trap.StoreMode = StoreModeCopy;
                        trap.OpenFrequency = OpenDelayCopy;
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
                    StoreModeCopy = trap.StoreMode;
                    OpenDelayCopy = trap.OpenFrequency;
                    SoundDefOf.Tick_Low.PlayOneShotOnCamera();
                }
                TooltipHandler.TipRegionByKey(rectCopy, ResourceBank.Strings.CopyTip);


                Text.Font = GameFont.Small;
                //stockpile setting
                Widgets.Label(label1, StockpileString.Translate());

                Listing_Standard listing_Standard = new Listing_Standard();
                listing_Standard.Begin(storage);
                Listing_Standard listing_Standard2 = listing_Standard.BeginSection(StoreModeSubdialogHeight);
                string text = trap.StoreMode.LabelCap;
                if (listing_Standard2.ButtonText(text))
                {
                    Text.Font = GameFont.Small;

                    List<FloatMenuOption> list = new List<FloatMenuOption>();

                    list.Add(new FloatMenuOption(BillStoreModeDefOf.BestStockpile.LabelCap, delegate
                    {
                        trap.StoreMode = BillStoreModeDefOf.BestStockpile;
                    }));
                    list.Add(new FloatMenuOption(BillStoreModeDefOf.DropOnFloor.LabelCap, delegate
                    {
                        trap.StoreMode = BillStoreModeDefOf.DropOnFloor;
                    }));
                    Find.WindowStack.Add(new FloatMenu(list));
                }
                Text.Font = GameFont.Small;
                listing_Standard.EndSection(listing_Standard2);
                listing_Standard.End();
                
                Text.Font = GameFont.Small;
                //delay setting
                Widgets.Label(label2, ResourceBank.Strings.UnloadFrequency.Translate());

                Text.Font = GameFont.Small;
                trap.OpenFrequency = Widgets.FrequencyHorizontalSlider(slider, trap.OpenFrequency, 0.1f, 25f, roundToInt: true);
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