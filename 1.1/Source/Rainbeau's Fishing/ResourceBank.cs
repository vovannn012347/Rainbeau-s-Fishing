using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RBB {

    [StaticConstructorOnStartup]
    public static class ResourceBank
    {
        const string CrabPotItemTag = "Fishing_CrabPotItem";
        const string FishSpotItemTag = "Fishing_FishItem";

        public static readonly Texture2D Paste = ContentFinder<Texture2D>.Get("UI/Buttons/Paste");
        public static readonly Texture2D Copy = ContentFinder<Texture2D>.Get("UI/Buttons/Copy");

        private static List<FishDef> crabPotFish = null;
        private static List<FishDef> fishingSpotFish = null;


        public static List<FishDef> CrabPotFish
        {
            get
            {
                if(crabPotFish == null)
                {
                    PopulateFish();
                }
                return crabPotFish;
            }
        }

        public static List<FishDef> FishingSpotFish
        {
            get
            {
                if (fishingSpotFish == null)
                {
                    PopulateFish();
                }
                return fishingSpotFish;
            }
        }

        private static void PopulateFish()
        {
            List<FishDef> defOfFish = DefDatabase<FishDef>.AllDefsListForReading;

            crabPotFish = defOfFish.Where(def => def.thingSetMakerTags.Contains(CrabPotItemTag)).ToList();
            fishingSpotFish = defOfFish.Where(def => def.thingSetMakerTags.Contains(FishSpotItemTag)).ToList();
        }

        [DefOf]
        public static class EffecterDefOf
        {
            public static EffecterDef Fishing_Effecter;
        }

        [DefOf]
        public static class WorkTypeDefOf
        {
            public static WorkTypeDef Fishing;
        }
        [DefOf]
        public static class StatDefOf
        {
            public static StatDef FishingChance;
            public static StatDef FishBiteChance;
            public static StatDef FishingEfficiency;
        }
        [DefOf]
        public static class ThingDefOf
        {            
            public static ThingDef RBB_FishingSpot;
            public static ThingDef RBB_ShellfishTrap;
        }

        [DefOf]
        public static class JobDefOf
        {
            public static JobDef JobDef_CatchFish;
            public static JobDef JobDef_EmptyCrabPot;
        }

        [DefOf]
        public static class TerrainAffordanceDefOf
        {
            public static TerrainAffordanceDef Fishable;
        }

        [DefOf]
        public static class ThingSetMakerDefOf
        {
            public static ThingSetMakerDef Fishing_ItemJunk;
        }

        public static ThingSetMakerDef VisitorGift;

        public static class Strings
        {
            public static string AcceptanceFishingSpot1 = "RBB.FishingSpot1";
            public static string AcceptanceFishingSpot2 = "RBB.FishingSpot2";
            public static string AcceptanceNotOnFS = "RBB.NotOnFS";
            public static string AcceptanceShellfishTrap = "RBB.ShellfishTrap";

            public static string FishingTitle = "RBB.Fishing";

            public static string FishingTradegyTitle = "RBB.TragedyTitle";
            public static string FishingTradegyText = "RBB.TragedyTextSpecific";

            public static string FishingCaughtNothingTitle = "RBB.CaughtNothingTitle";
            public static string FishingCaughtJunkTitle = "RBB.SnaggedJunkTitle";
            public static string FishingSuccessTitle = "RBB.FishingSuccessTitle";
            public static string FishingCaughtTreasureTitle = "RBB.SunkenTreasureTitle";

            public static string FishingCaughtNothingText = "RBB.CaughtNothing1";
            public static string FishingCaughtJunkText = "RBB.SnaggedJunk1";
            public static string FishingSuccess = "RBB.FishingCatch1";
            public static string FishingCaughtTreasureText = "RBB.SunkenTreasureText1";
            
            public static string TrapDestroyedTest = "RBB.TrapDestroyed";
            public static string TrapCaughtStuffLabel = "RBB.TrapCatch";

            public static string FishingBuildingEfficiencyTip = "RBB.FishingBuildingEfficiencyTip";

            public static string FishingEfficiency = "RBB.FishingEfficiency";
            public static string FishingEfficiencyTip = "RBB.FishingEfficiencyTip";
            public static string SettingTrapEfficiency = "RBB.TrapEfficiency";
            public static string SettingTrapEfficiencyTip = "RBB.TrapEfficiencyTip";
            public static string SettingNoticeNone = "RBB.NoticeNone";
            public static string SettingNoticeSilent = "RBB.NoticeSilent";
            public static string SettingNoticeDing = "RBB.NoticeDing";
            public static string SettingNoticeBox = "RBB.NoticeBox";

            public static string UnloadFrequency = "RBB.UnloadFrequency";
            public static string FishingDuration = "RBB.FishingDuration";
            public static string FishingFrequency = "RBB.FishingFrequency";


            public static string TabUnloadTimer = "RBB.TabUnloadTimer";
            public static string TabFishingTimer = "RBB.TabFishingTimer";

            public static string PotUnloadTimerTag = "RBB.PotUnloadTimerTag";
            public static string FishingTimerTag = "RBB.FishingTimerTag";

            public static string CopyTip = "RBB.CopyTip";
            public static string PasteTip = "RBB.PasteTip";

            public static string FailureLevel = "RBB.FailureLevel";
            public static string JunkLevel = "RBB.JunkLevel";
            public static string SuccessLevel = "RBB.SuccessLevel";
            public static string TreasureLevel = "RBB.TreasureLevel";

            public static string MaxTreasureMass = "RBB.MaxTreasureMass";
            public static string MaxTreasureMassTip = "RBB.MaxTreasureMassTip";

            public static string MaxJunkMass = "RBB.MaxJunkMass";
            public static string MaxJunkMassTip = "RBB.MaxJunkMassTip";

            public static string TreasureChance = "RBB.TreasureChance";
            public static string TreasureChanceTip = "RBB.TreasureChanceTip";

            public static string TreasureMaxValue = "RBB.TreasureMaxValue";
            public static string TreasureMaxValueTip = "RBB.TreasureMaxValueTip";

            public static string JunkChance = "RBB.JunkChance";
            public static string JunkChanceTip = "RBB.JunkChanceTip";

            public static string RoeChance = "RBB.RoeChance";
            public static string RoeChanceTip = "RBB.RoeChanceTip";
        }
    }
}
