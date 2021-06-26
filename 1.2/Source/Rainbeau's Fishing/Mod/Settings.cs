using UnityEngine;
using Verse;

namespace RBB
{
    public class Settings : ModSettings
    {
        public float FishingEfficiency => fishingEfficiency / 100;
        public float TrapEfficiency => trapEfficiency / 100;

        public float fishingEfficiency = 100.0f;
        public float trapEfficiency = 100.0f;
        
        public float failureLevel = 1.5f;
        public float junkLevel = 1.5f;
        public float SuccessLevel = 1.5f;
        public float treasureLevel = 2.5f;

        public float MaxTreasureMass = 7f;
        public float MaxJunkMass = 2f;
        public float treasureChanceStandart = 0.0006f;
        public float treasureMaxValue = 5500f;

        public float junkChance = 0.25f;
        public float roeChance = 0.5f;

        public void DoWindowContents(Rect canvas)
        {
            Listing_Standard list = new Listing_Standard();
            list.ColumnWidth = canvas.width;
            list.Begin(canvas);

            list.Gap();
            list.Label(ResourceBank.Strings.FishingEfficiency.Translate() + "  " + (int)fishingEfficiency + "%");
            fishingEfficiency = list.Slider(fishingEfficiency, 50f, 150.99f);
            Text.Font = GameFont.Tiny;
            list.Label("          " + ResourceBank.Strings.FishingEfficiencyTip.Translate());
            Text.Font = GameFont.Small;

            list.Gap();
            list.Label(ResourceBank.Strings.SettingTrapEfficiency.Translate() + "  " + (int)trapEfficiency + "%");
            trapEfficiency = list.Slider(trapEfficiency, 50f, 150.99f);
            Text.Font = GameFont.Tiny;
            list.Label("          " + ResourceBank.Strings.SettingTrapEfficiencyTip.Translate());
            Text.Font = GameFont.Small;
            list.Gap();

            list.Label(ResourceBank.Strings.FailureLevel.Translate(failureLevel));
            failureLevel = list.Slider(failureLevel, 0, 4);
            Text.Font = GameFont.Tiny;
            if (failureLevel < 1) { list.Label("          " + ResourceBank.Strings.SettingNoticeNone.Translate()); }
            else if (failureLevel < 2)
            {
                GUI.contentColor = Color.yellow;
                list.Label("          " + ResourceBank.Strings.SettingNoticeSilent.Translate());
                GUI.contentColor = Color.white;
            }
            else if (failureLevel < 3) { list.Label("          " + ResourceBank.Strings.SettingNoticeDing.Translate()); }
            else { list.Label("          " + ResourceBank.Strings.SettingNoticeBox.Translate()); }
            Text.Font = GameFont.Small;
            list.Gap();
            list.Label(ResourceBank.Strings.JunkLevel.Translate(junkLevel));
            junkLevel = list.Slider(junkLevel, 0, 4);
            Text.Font = GameFont.Tiny;
            if (junkLevel < 1) { list.Label("          " + ResourceBank.Strings.SettingNoticeNone.Translate()); }
            else if (junkLevel < 2)
            {
                GUI.contentColor = Color.yellow;
                list.Label("          " + ResourceBank.Strings.SettingNoticeSilent.Translate());
                GUI.contentColor = Color.white;
            }
            else if (junkLevel < 3) { list.Label("          " + ResourceBank.Strings.SettingNoticeDing.Translate()); }
            else { list.Label("          " + ResourceBank.Strings.SettingNoticeBox.Translate()); }
            Text.Font = GameFont.Small;
            list.Gap();
            list.Label(ResourceBank.Strings.SuccessLevel.Translate(SuccessLevel));
            SuccessLevel = list.Slider(SuccessLevel, 0, 4);
            Text.Font = GameFont.Tiny;
            if (SuccessLevel < 1) { list.Label("          " + ResourceBank.Strings.SettingNoticeNone.Translate()); }
            else if (SuccessLevel < 2)
            {
                GUI.contentColor = Color.yellow;
                list.Label("          " + ResourceBank.Strings.SettingNoticeSilent.Translate());
                GUI.contentColor = Color.white;
            }
            else if (SuccessLevel < 3) { list.Label("          " + ResourceBank.Strings.SettingNoticeDing.Translate()); }
            else { list.Label("          " + ResourceBank.Strings.SettingNoticeBox.Translate()); }
            Text.Font = GameFont.Small;
            list.Gap();
            list.Label(ResourceBank.Strings.TreasureLevel.Translate(treasureLevel));
            treasureLevel = list.Slider(treasureLevel, 0, 4);
            Text.Font = GameFont.Tiny;
            if (treasureLevel < 1) { list.Label("          " + ResourceBank.Strings.SettingNoticeNone.Translate()); }
            else if (treasureLevel < 2) { list.Label("          " + ResourceBank.Strings.SettingNoticeSilent.Translate()); }
            else if (treasureLevel < 3)
            {
                GUI.contentColor = Color.yellow;
                list.Label("          " + ResourceBank.Strings.SettingNoticeDing.Translate());
                GUI.contentColor = Color.white;
            }
            else { list.Label("          " + ResourceBank.Strings.SettingNoticeBox.Translate()); }
            Text.Font = GameFont.Small;
            
            list.Gap();
            list.Label(ResourceBank.Strings.MaxTreasureMass.Translate() + "  " + MaxTreasureMass.ToString("0.00") + " kg".Translate());
            MaxTreasureMass = list.Slider(MaxTreasureMass, 1f, 13f);
            Text.Font = GameFont.Tiny;
            list.Label("          " + ResourceBank.Strings.MaxTreasureMassTip.Translate());
            Text.Font = GameFont.Small;

            list.Gap();
            list.Label(ResourceBank.Strings.MaxJunkMass.Translate() + "  " + MaxJunkMass.ToString("0.00") + " kg".Translate());
            MaxJunkMass = list.Slider(MaxJunkMass, 0.1f, 3.9f);
            Text.Font = GameFont.Tiny;
            list.Label("          " + ResourceBank.Strings.MaxJunkMassTip.Translate());
            Text.Font = GameFont.Small;

            list.Gap();
            list.Label(ResourceBank.Strings.TreasureChance.Translate() + "  " + treasureChanceStandart.ToString("0.00") + "%");
            treasureChanceStandart = list.Slider(treasureChanceStandart, 0.00001f, 0.001f);
            Text.Font = GameFont.Tiny;
            list.Label("          " + ResourceBank.Strings.TreasureChanceTip.Translate());
            Text.Font = GameFont.Small;

            list.Gap();
            list.Label(ResourceBank.Strings.TreasureMaxValue.Translate() + "  " + treasureMaxValue.ToString("0.00") + "%");
            treasureMaxValue = list.Slider(treasureMaxValue, 100f, 11000f);
            Text.Font = GameFont.Tiny;
            list.Label("          " + ResourceBank.Strings.TreasureMaxValueTip.Translate());
            Text.Font = GameFont.Small;
            
            list.Gap();
            list.Label(ResourceBank.Strings.JunkChance.Translate() + "  " + junkChance.ToString("0.00") + "%");
            junkChance = list.Slider(junkChance, 0f, 1f);
            Text.Font = GameFont.Tiny;
            list.Label("          " + ResourceBank.Strings.JunkChanceTip.Translate());
            Text.Font = GameFont.Small;

            list.Gap();
            list.Label(ResourceBank.Strings.RoeChance.Translate() + "  " + roeChance.ToString("0.00") + "%");
            roeChance = list.Slider(roeChance, 0f, 1f);
            Text.Font = GameFont.Tiny;
            list.Label("          " + ResourceBank.Strings.RoeChanceTip.Translate());
            Text.Font = GameFont.Small;

            list.End();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref fishingEfficiency, "fishingEfficiency", 100.0f);
            Scribe_Values.Look(ref trapEfficiency, "trapEfficiency", 100.0f);
            Scribe_Values.Look(ref failureLevel, "failureLevel", 1.5f);
            Scribe_Values.Look(ref junkLevel, "junkLevel", 1.5f);
            Scribe_Values.Look(ref SuccessLevel, "successLevel", 1.5f);
            Scribe_Values.Look(ref treasureLevel, "treasureLevel", 2.5f);
        }
    }
}
