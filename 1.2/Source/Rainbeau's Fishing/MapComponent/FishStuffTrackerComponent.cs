using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RBB
{
    public class FishStuffTrackerComponent : MapComponent
    {
        const int updateTicks = 60000;
        const int tempUpdateTicks = 2500;
        const int fishStockStandartRespawn = 36000;
        const int fishStockStandartAmount = 2;

        public List<FishDef> CrabPotOceanFishLocal => crabPotOceanFishLocal;
        public List<FishDef> CrabPotNonOceanFishLocal => crabPotNonOceanFishLocal;
        public List<FishDef> FishingSpotOceanFishLocal => fishingSpotOceanFishLocal;
        public List<FishDef> FishingSpotNonOceanFishLocal => fishingSpotNonOceanFishLocal;

        private List<FishDef> crabPotOceanFishLocal = new List<FishDef>();
        private List<FishDef> crabPotNonOceanFishLocal = new List<FishDef>();
        private List<FishDef> fishingSpotOceanFishLocal = new List<FishDef>();
        private List<FishDef> fishingSpotNonOceanFishLocal = new List<FishDef>();
        
        int LastUpdateTick = 0;

        float fishMultStandart = 1;

        int fishStockRespawn = fishStockStandartRespawn;
        int fishStockMaxAmount = fishStockStandartAmount;

        float averageMapHumidity = 0;
        float averageSwampiness = 0;
        float averageTemperature = 0;

        float minTemperature = 0;
        float maxTemperature = 0;

        public float DifficultyMultiplier => fishMultStandart;
        public int FishSpawnInterval => fishStockRespawn;
        public int MaxFishStock => fishStockMaxAmount;

        //float temperatureOverride = 0;
        //float swampOverride = 0;

        bool needsInitialize = true;


        public FishStuffTrackerComponent(Map map) : base(map)
        {

        }

        public override void MapComponentTick()
        {
            if (Find.TickManager.TicksGame > (LastUpdateTick + updateTicks))
            {
                LastUpdateTick = Find.TickManager.TicksGame;

                UpdateInfo();
            }
        }

        public List<Tuple<float, FishDef>> GetUpdatedFishList(List<FishDef> fishList, TerrainType type, bool IsMarshTerrain = false)
        {
            if (needsInitialize)
            {
                needsInitialize = false;
                MapInitialize();
                UpdateInfo();
            }

            List<Tuple<float, FishDef>> fishByWeight = new List<Tuple<float, FishDef>>();

            var constWeightFish = fishList.Where(fish => fish.fishProperties.WeightType == WeightingType.Static);
            var relativeWeightFish = fishList.Where(fish => fish.fishProperties.WeightType == WeightingType.StaticFactor);

            float tempWeightMod = 0;
            float humWeightMod = 0;
            float swampWeightMod = 0;
            float currentFishWeight = 1;
            float totalWeight = 0;

            fishByWeight.Clear();
            //Log.Message("constweight");
            foreach (var fish in constWeightFish)
            {
                tempWeightMod = (float)UncertainRange.GetWeightPercent(fish.fishProperties.TemperatureRange, map.mapTemperature.OutdoorTemp);
                humWeightMod = (float)UncertainRange.GetWeightPercent(fish.fishProperties.HumidityRange, averageMapHumidity);
                swampWeightMod = fish.fishProperties.MarshFactor * (float)UncertainRange.GetWeightPercent(fish.fishProperties.SwampinessRange, averageSwampiness + (IsMarshTerrain ? 0.8f : 0));
                
                currentFishWeight = fish.fishProperties.Weight * tempWeightMod * humWeightMod *(1 + swampWeightMod);
                //Log.Message(fish.defName + " " + fish.fishProperties.Weight + " tempWeightMod:" + tempWeightMod + " humWeightMod:" + humWeightMod + " swampWeightMod:" + swampWeightMod + " weight:" + currentFishWeight + " OutdoorTemp:" + map.mapTemperature.OutdoorTemp);

                //if (fish.defName == "Sturgeon")
                //{

                //}

                totalWeight += Math.Abs(currentFishWeight);

                if (currentFishWeight > 0.001f) fishByWeight.Add(new Tuple<float, FishDef>(currentFishWeight, fish));
            }

            if (totalWeight <= 0.2)
            {
                totalWeight = 1;
            }
            //Log.Message("relativeWeight");
            
            foreach (var fish in relativeWeightFish)
            {
                tempWeightMod = (float)UncertainRange.GetWeightPercent(fish.fishProperties.TemperatureRange, map.mapTemperature.OutdoorTemp);
                humWeightMod = (float)UncertainRange.GetWeightPercent(fish.fishProperties.HumidityRange, map.TileInfo.rainfall);
                swampWeightMod = fish.fishProperties.MarshFactor * (float)UncertainRange.GetWeightPercent(fish.fishProperties.SwampinessRange, averageSwampiness + (IsMarshTerrain ? 0.8f : 0));
                
                currentFishWeight = totalWeight * fish.fishProperties.Weight * tempWeightMod * humWeightMod * (1 + swampWeightMod);

                //Log.Message(fish.defName + " " + fish.fishProperties.Weight + " tempWeightMod:" + tempWeightMod + " humWeightMod:" + humWeightMod + " swampWeightMod:" + swampWeightMod + " weight:" + currentFishWeight + " OutdoorTemp:" + map.mapTemperature.OutdoorTemp);

                totalWeight += Math.Abs(currentFishWeight);
                if(currentFishWeight > 0.001f) fishByWeight.Add(new Tuple<float, FishDef>(currentFishWeight, fish));
            }


            return fishByWeight;
        }

        public override void FinalizeInit()
        {
            if (needsInitialize)
            {
                needsInitialize = false;
                MapInitialize();
                UpdateInfo();
            }
        }

        private void MapInitialize()
        {
            averageMapHumidity = map.TileInfo.rainfall;
            averageSwampiness = map.TileInfo.swampiness;
            averageTemperature = map.TileInfo.temperature;
            //Log.Message("averageTemperature:" + averageTemperature);
            //Log.Message("averageMapHumidity:" + averageMapHumidity);
            //Log.Message("averageSwampiness:" + averageSwampiness);

            fishMultStandart = (1 + System.Math.Abs(averageTemperature - 5) / 40) / (1 + System.Math.Abs(averageMapHumidity - 800) / 1600);

            fishStockRespawn = (int)(fishStockStandartRespawn * fishMultStandart);
            fishStockMaxAmount = (int)(fishStockStandartAmount / fishMultStandart);
        }

        private void UpdateInfo()
        {
            int tick = Find.TickManager.TicksGame;
            int tile = map.Tile;
            float currentTemp, mint = 0, maxt = 0;

            //getting temp averages, computation optimization
            for (int i = 0; i < 24; ++i)
            {
                currentTemp = GenTemperature.OffsetFromSeasonCycle(tick + i * tempUpdateTicks, tile);
                if (currentTemp < mint) mint = currentTemp;
                if (currentTemp > maxt) maxt = currentTemp;
            }

            minTemperature = mint;
            maxTemperature = maxt;

            UpdateFIshInfo();

            LastUpdateTick = tick;
        }

        private void UpdateFIshInfo()
        {
            crabPotOceanFishLocal.Clear();
            crabPotNonOceanFishLocal.Clear();
            fishingSpotOceanFishLocal.Clear();
            fishingSpotNonOceanFishLocal.Clear();
            
            crabPotOceanFishLocal.AddRange(ResourceBank.CrabPotFish.Where(def => def.fishProperties?.SaltWaterFactor > 0));
            crabPotNonOceanFishLocal.AddRange(ResourceBank.CrabPotFish.Where(def => def.fishProperties?.FreshWaterFactor > 0));

            fishingSpotOceanFishLocal.AddRange(ResourceBank.FishingSpotFish.Where(def => def.fishProperties?.SaltWaterFactor > 0));
            fishingSpotNonOceanFishLocal.AddRange(ResourceBank.FishingSpotFish.Where(def => def.fishProperties?.FreshWaterFactor > 0));

            //ListStuff(crabPotOceanFishLocal, "crabPotOceanFishLocal");
            FilterNotUsefulFish(crabPotOceanFishLocal);
            //ListStuff(crabPotOceanFishLocal, "crabPotOceanFishLocal");

            //ListStuff(crabPotOceanFishLocal, "crabPotNonOceanFishLocal");
            FilterNotUsefulFish(crabPotNonOceanFishLocal);
            //ListStuff(crabPotOceanFishLocal, "crabPotNonOceanFishLocal");

            //ListStuff(crabPotOceanFishLocal, "fishingSpotOceanFishLocal");
            FilterNotUsefulFish(fishingSpotOceanFishLocal);
            //ListStuff(crabPotOceanFishLocal, "fishingSpotOceanFishLocal");

            //ListStuff(crabPotOceanFishLocal, "fishingSpotNonOceanFishLocal");
            FilterNotUsefulFish(fishingSpotNonOceanFishLocal);
            //ListStuff(crabPotOceanFishLocal, "fishingSpotNonOceanFishLocal");
        }

        //private void ListStuff(List<FishDef> fishList, string type)
        //{
        //    Log.Message("fish:"+ type);
        //    foreach (var fish in fishList)
        //    {
        //        Log.Message(fish.defName);
        //    }
        //}

        private void FilterNotUsefulFish(List<FishDef> currenFishList)
        {
            FishDef fish = null;

            for (int i = 0; i < currenFishList.Count;)
            {
                fish = currenFishList[i];

                if (CheckOutOfRange(fish.fishProperties.TemperatureRange, minTemperature, maxTemperature))
                {
                    currenFishList.RemoveAt(i);
                    continue;
                }

                if (CheckOutOfRange(fish.fishProperties.HumidityRange, averageMapHumidity, averageMapHumidity))
                {
                    currenFishList.RemoveAt(i);
                    continue;
                }

                if (CheckOutOfRange(fish.fishProperties.SwampinessRange, averageSwampiness, averageSwampiness))
                {
                    currenFishList.RemoveAt(i);
                    continue;
                }
                ++i;
            }


        }

        private bool CheckOutOfRange(UncertainRange range, float min, float max)
        {
            if (range != null)
            {
                return range.CheckOutOfRange(min, max);
            }

            return false;
        }
    }
}
