using RimWorld;
using System;
using System.Linq;
using UnityEngine;
using Verse;

namespace RBB
{
    public class ModController : Mod
    {
        public static Settings Settings;
        public override string SettingsCategory() => ResourceBank.Strings.FishingTitle.Translate();
        public override void DoSettingsWindowContents(Rect canvas)
        {
            Settings.DoWindowContents(canvas);
        }

        public ModController(ModContentPack content) : base(content)
        {
            Settings = GetSettings<Settings>();
            //GenerateImpliedFishDefs(content);
        }

        //static void GenerateImpliedFishDefs(ModContentPack content)
        //{
        //    var fishDefs = content.AllDefs.OfType<FishDef>();

        //    FishDef newDef = null;
        //    ThingDef roeDef = null;
        //    int roeCount = 0;

        //    Log.Message("fishcount:" + fishDefs.Count());
        //    foreach (var def in fishDefs)
        //    {
        //        Log.Message("def:" + def.defName);
        //    }

        //    foreach (var def in fishDefs)
        //    {
        //        if (def.fishProperties.HasRoe)
        //        {
        //            if(def.fishProperties.RoeDef != null)
        //            {
        //                newDef = def.DeepClone();
        //                newDef.generated = true;
        //                roeDef = def.fishProperties.RoeDef;
        //                roeCount = Math.Min(1, def.fishProperties.RoeCount);
                        
        //                newDef.butcherProducts.Add(new ThingDefCountClass(roeDef, roeCount));
                        
        //                var marketPriceStat = newDef.statBases.FirstOrDefault(s => s.stat == StatDefOf.MarketValue);

        //                if(marketPriceStat != null)
        //                {
        //                    marketPriceStat.value += roeDef.BaseMarketValue * roeCount * 0.6f;
        //                }
        //                else
        //                {
        //                    var newmarketPrice = new StatModifier()
        //                    {
        //                        stat = StatDefOf.MarketValue,
        //                        value = roeDef.BaseMarketValue * roeCount * 0.6f
        //                    };
        //                    newDef.statBases.Add(newmarketPrice);
        //                }

        //                var massStat = newDef.statBases.FirstOrDefault(s => s.stat == StatDefOf.Mass);

        //                if (massStat != null)
        //                {
        //                    massStat.value += roeDef.BaseMass * roeCount;
        //                }
        //                else
        //                {
        //                    var newmassStat = new StatModifier()
        //                    {
        //                        stat = StatDefOf.MarketValue,
        //                        value = roeDef.BaseMarketValue * roeCount * 0.6f
        //                    };
        //                    newDef.statBases.Add(newmassStat);
        //                }

        //                DefGenerator.AddImpliedDef(newDef);
        //            }
        //            else
        //            {
        //                Log.Error("fish has roe but no roedef");
        //            }
        //        }
        //    }
        //}
    }
}
