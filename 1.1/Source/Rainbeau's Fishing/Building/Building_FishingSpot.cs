using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RBB
{
    public class Building_FishingSpot : Building_WorkTable
    {
        protected const string DeepWaterTag = "DeepWater";
        protected const int updateTicks = 20000; //8hrs

        public IntVec3 fishingSpotCell = new IntVec3(0, 0, 0);
        public List<Tuple<float, FishDef>> FishByWeight => fishByWeight;
        public FishStuffTrackerComponent MapFishTracker => mapFishTracker;

        private FishStuffTrackerComponent mapFishTracker;
        private CompFishingStatAffector fishStatAffector;
        private int fishStockRespawnInterval = 24000;
        private List<Tuple<float, FishDef>> fishByWeight = new List<Tuple<float, FishDef>>();

        public int FishStock => fishStock;
        public bool OceanTerrain => oceanTerrain;
        public bool MarshTerrain => marshTerrain;
        public bool DeepTerrain => deepTerrain;
        public bool MovingTerrain => movingTerrain;
        
        private int maxFishStock = 3;
        private int fishStock = 2;
        private int fishStockRespawnTick = 0;
        private int lastUpdateTick = 0;

        public float FishingDuration = 12;
        public float FishingFrequency = 1;
        public int lastFishingStartTick = 0;

        private bool oceanTerrain = false;
        private bool marshTerrain = false;
        private bool deepTerrain = false;
        private bool movingTerrain = false;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref fishStock, "fishStock");
            Scribe_Values.Look(ref maxFishStock, "maxFishStock");
            Scribe_Values.Look(ref fishStockRespawnTick, "fishStockRespawnTick");

            Scribe_Values.Look(ref FishingDuration, "fishingDuration");
            Scribe_Values.Look(ref FishingFrequency, "fishingFrequency");
            Scribe_Values.Look(ref lastFishingStartTick, "lastFishingStartTick");
            Scribe_Values.Look(ref lastUpdateTick, "lastUpdateTick");
        }
        
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            
            fishingSpotCell = this.Position;
            mapFishTracker = this.Map.GetComponent<FishStuffTrackerComponent>();
            fishStatAffector = this.TryGetComp<CompFishingStatAffector>();

            fishStockRespawnInterval = mapFishTracker.FishSpawnInterval;
            maxFishStock = mapFishTracker.MaxFishStock;

            if (!respawningAfterLoad)
            {
                fishStock = 0;
                lastFishingStartTick = Find.TickManager.TicksGame;
            }

            UpdateFish();
        }

        public void FishCaught()
        {
            fishStock--;
        }

        private void UpdateFish()
        {
            TerrainDef terrainDef = this.Map.terrainGrid.TerrainAt(this.fishingSpotCell);
            oceanTerrain = terrainDef.defName.Contains("Ocean");
            marshTerrain = terrainDef.generatedFilth != null;
            deepTerrain = terrainDef.tags.Contains(DeepWaterTag);
            movingTerrain = terrainDef.affordances.Contains(TerrainAffordanceDefOf.MovingFluid);

            bool waterTerrain = terrainDef.affordances.Contains(ResourceBank.TerrainAffordanceDefOf.Fishable);

            maxFishStock = mapFishTracker.MaxFishStock;
            if (oceanTerrain || movingTerrain)
            {
                maxFishStock += 1;
            }

            if (deepTerrain)
            {
                maxFishStock += 1;
            }

            if (waterTerrain)
            {
                if (oceanTerrain)
                {
                    RefreshFishStuff(mapFishTracker.FishingSpotOceanFishLocal);
                }
                else
                {
                    RefreshFishStuff(mapFishTracker.FishingSpotNonOceanFishLocal);
                }
            }
            else
            {
                fishByWeight.Clear();
            }

            //foreach(var fish in fishByWeight)
            //{
            //    Log.Message(fish.Item2.defName + " " + fish.Item1);
            //}

            lastUpdateTick = Find.TickManager.TicksGame + updateTicks;
        }


        private void RefreshFishStuff(List<FishDef> fishList)
        {
            fishByWeight.Clear();
            fishByWeight.AddRange(mapFishTracker.GetUpdatedFishList(fishList, OceanTerrain ? TerrainType.SaltWater : TerrainType.FreshWater, MarshTerrain));
        }

        public override void Tick()
        {
            base.Tick();
            TickUpdate();
        }

        public override void TickRare()
        {
            base.TickRare();
            TickUpdate();
        }

        public override void TickLong()
        {
            base.TickLong();
            TickUpdate();
        }

        public void TickUpdate()
        {

            TerrainDef terrainDef = base.Map.terrainGrid.TerrainAt(this.fishingSpotCell);

            if (!terrainDef.affordances.Contains(this.def.terrainAffordanceNeeded))
            {
                this.Destroy();
                return;
            }


            if (Find.TickManager.TicksGame > (lastUpdateTick + updateTicks))
            {
                UpdateFish();
            }

            if (Find.TickManager.TicksGame >= fishStockRespawnTick)
            {
                if (fishStock < maxFishStock)
                {
                    fishStock++;
                }

                fishStockRespawnTick = Find.TickManager.TicksGame + (int)((float)fishStockRespawnInterval * Rand.Range(0.8f, 1.2f) /(ModController.Settings.FishingEfficiency * (fishStatAffector?.GetOffset(this) ?? 1)));
            }
        }

        public override void Destroy(DestroyMode mode = 0)
        {
            //mote despawn not needed
            //List<Thing> list = base.Map.thingGrid.ThingsListAt(fishingSpotCell);
            //for (int i = 0; i < list.Count; i++)
            //{
            //    if (list[i].def.defName.Contains("Mote_Fishing"))
            //    {
            //        list[i].DeSpawn();
            //    }
            //}
            base.Destroy(mode);
        }
    }

}
