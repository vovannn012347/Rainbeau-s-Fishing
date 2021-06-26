using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RBB
{

    public class Building_ShellfishTrap : Building, IThingHolder
    {
        System.Random rnd = new System.Random();
        protected const string DeepWaterTag = "DeepWater";
        protected const string WaterTag = "Water";
        protected const int updateTicks = 15000;
        
        private TerrainDef terrainDef;
        private bool OceanTerrain = false;
        private bool MarshTerrain = false;
        private int HealthToEmpty = 0;

        protected ThingOwner innerContainer;
        private int NextTickToCatch = 0;
        private int NextUpdateTick = 0;
        
        public int LastOpenTick = 0;
        public float OpenFrequency = 1;
        public BillStoreModeDef StoreMode = BillStoreModeDefOf.DropOnFloor;

        private List<Tuple<float, FishDef>> fishByWeight = new List<Tuple<float, FishDef>>();
        private FishStuffTrackerComponent mapFishTracker;
        private CompFishingStatAffector fishStatAffector;

        public Building_ShellfishTrap()
        {
            innerContainer = new ThingOwner<Thing>(this, oneStackOnly: false);
        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            if (this.holdingOwner == null)
            {
                this.innerContainer.TryDropAll(this.Position, this.Map, ThingPlaceMode.Near);
            }
            base.DeSpawn(mode);
        }
        
        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            if(mode != DestroyMode.Vanish)
            {
                if (this.holdingOwner == null)
                {
                    this.innerContainer.TryDropAll(this.Position, this.Map, ThingPlaceMode.Near);
                }
            }
            base.DeSpawn(mode);
        }


        public ThingOwner GetDirectlyHeldThings()
        {
            return innerContainer;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref this.NextTickToCatch, "NextTickToCatch", 0);
            Scribe_Values.Look(ref this.NextUpdateTick, "NextUpdateTick", 0);
            
            Scribe_Values.Look(ref this.OpenFrequency, "OpenDelay", 1);
            Scribe_Values.Look(ref this.LastOpenTick, "LastOpenTick", 0);
            Scribe_Defs.Look(ref this.StoreMode, "StoreMode");
            Scribe_Deep.Look(ref innerContainer, "innerContainer", this);
        }

        public override void SpawnSetup(Map currentGame, bool respawningAfterLoad)
        {
            base.SpawnSetup(currentGame, respawningAfterLoad);

            mapFishTracker = this.Map.GetComponent<FishStuffTrackerComponent>();
            fishStatAffector = this.TryGetComp<CompFishingStatAffector>();

            HealthToEmpty = (int)(this.MaxHitPoints * 0.2f);
            terrainDef = this.Map.terrainGrid.TerrainAt(this.Position);

            if(!respawningAfterLoad)
            {
                RefreshCatchTimer(Find.TickManager.TicksGame);
                LastOpenTick = Find.TickManager.TicksGame;
            }
        }

        public override void PostMapInit()
        {
            base.PostMapInit();
            UpdateFish();
        }

        private void UpdateFish()
        {
            OceanTerrain = terrainDef.defName.Contains("Ocean");
            MarshTerrain = terrainDef.generatedFilth != null;
            bool waterTerrain = terrainDef.affordances.Contains(ResourceBank.TerrainAffordanceDefOf.Fishable);
            
            if (waterTerrain)
            {
                if (OceanTerrain)
                {
                    RefreshFishStuff(mapFishTracker.CrabPotOceanFishLocal);
                }
                else
                {
                    RefreshFishStuff(mapFishTracker.CrabPotNonOceanFishLocal);
                }
            }
            else
            {
                fishByWeight.Clear();
            }

            //foreach (var fish in fishByWeight)
            //{
            //    Log.Message(fish.Item2.defName + " " + fish.Item1);
            //}

            NextUpdateTick = Find.TickManager.TicksGame + updateTicks;
        }

        public override string GetInspectString()
        {
            return base.GetInspectString();
        }

        private void RefreshFishStuff(List<FishDef> fishList)
        {
            fishByWeight.Clear();
            fishByWeight.AddRange(mapFishTracker.GetUpdatedFishList(fishList, OceanTerrain ? TerrainType.SaltWater : TerrainType.FreshWater, MarshTerrain));
        }

        private void RefreshCatchTimer(int CurrentTicks)
        {
            //from 8 to 20 hrs, modified by difficulty and other stuff
            NextTickToCatch = CurrentTicks + (int)(8000 * Rand.Range(4, 8) *  mapFishTracker.DifficultyMultiplier / (ModController.Settings.TrapEfficiency * (fishStatAffector?.GetOffset(this) ?? 1)));
        }

        public override void Tick()
        {
            base.Tick();

            TickTime(1);
        }

        public override void TickRare()
        {
            base.TickRare();

            TickTime(250);
        }


        public override void TickLong()
        {
            base.TickLong();

            TickTime(1000);
        }

        private void TickTime(int time)
        {
            terrainDef = base.Map.terrainGrid.TerrainAt(this.Position);
            if (NextUpdateTick < Find.TickManager.TicksGame)
            {
                UpdateFish();
            }

            if (terrainDef.affordances.Contains(ResourceBank.TerrainAffordanceDefOf.Fishable))
            {
                if (this.NextTickToCatch <= Find.TickManager.TicksGame) {
                    PlaceProduct();
                    RefreshCatchTimer(Find.TickManager.TicksGame);
                }
            }
            else
            {
                RefreshCatchTimer(Find.TickManager.TicksGame);
            }

            //deteriorate caught stuff
            for (int i = 0; i < innerContainer.Count; ++i)
            {
                if (TryDoDeteriorate(innerContainer[i], terrainDef, time))
                {
                    innerContainer.RemoveAt(i);
                    continue;
                }
                ++i;
            }
        }

        private bool TryDoDeteriorate(Thing t, TerrainDef terrain, int ticks)
        {
            float num = 1000 * SteadyEnvironmentEffects. FinalDeteriorationRate(t, false, false, false, terrain);
            if (!(num < 0.001f) && Rand.Chance(num / 36f))
            {
                IntVec3 position = this.Position;
                Map map = this.Map;

                t.TakeDamage(new DamageInfo(DamageDefOf.Deterioration, 1f));
                if (t.Destroyed && t.def.messageOnDeteriorateInStorage)
                {
                    return true;
                }
            }
            return false;
        }

        private void PlaceProduct()
        {
            if (this.HitPoints <= this.HealthToEmpty)
            {
                for (int i = 0; i < innerContainer.Count; ++i)
                {
                    innerContainer[i].stackCount = innerContainer[i].stackCount / 2 - 1;

                    if (innerContainer[i].stackCount <= 0)
                    {
                        innerContainer.RemoveAt(i);
                        continue;
                    }
                    ++i;
                }

                return;
            }

            if (Rand.Value <= 0.25)
            {
                int damage = Rand.RangeInclusive(0, 4);
                if (Rand.Value < 0.5)
                {
                    damage += Rand.RangeInclusive(0, 6);
                }

                this.TakeDamage(new DamageInfo(DamageDefOf.Blunt, damage, 0f, -1f, null, null, null, DamageInfo.SourceCategory.ThingOrUnknown));
            }

            string eventText = "";
            if (this.HitPoints <= 0)
            {
                this.Destroy(DestroyMode.Vanish);
                eventText = ResourceBank.Strings.TrapDestroyedTest.Translate();
            }
            else if(fishByWeight.Count > 0)
            {                
                var fishDefToCatch = fishByWeight.RandomElementByWeight(item => item.Item1).Item2;

                bool roeFish = false;
                if (fishDefToCatch.fishProperties.HasRoe && fishDefToCatch.fishProperties.RoeDef != null && fishDefToCatch.fishProperties.SpawningYearRange != null)
                {
                    int day = GenDate.DayOfYear(Find.TickManager.TicksAbs, Find.WorldGrid.LongLatOf(this.Map.Tile).x);
                    if (fishDefToCatch.fishProperties.SpawningYearRange.min <= day && day <= fishDefToCatch.fishProperties.SpawningYearRange.max)
                        if (Rand.Value < ModController.Settings.roeChance)
                        {
                            roeFish = true;
                        }
                }

                Thing trapCatchItem;
                if (roeFish)
                {
                    trapCatchItem = ThingMaker.MakeThing(fishDefToCatch.fishProperties.RoeDef, null);
                }
                else
                {
                    trapCatchItem = ThingMaker.MakeThing(fishDefToCatch, null);
                }

                trapCatchItem.stackCount = 1;

                if (!innerContainer.TryAdd(trapCatchItem))
                {
                    trapCatchItem.Destroy();
                }

                eventText = string.Format(ResourceBank.Strings.TrapCaughtStuffLabel.Translate(), fishDefToCatch.label);
            }

            if (ModController.Settings.SuccessLevel < 1) { }
            else if (ModController.Settings.SuccessLevel < 2)
            {
                Messages.Message(eventText, new TargetInfo(base.Position, base.Map, false), MessageTypeDefOf.SilentInput);
            }
            else if (ModController.Settings.SuccessLevel < 3)
            {
                Messages.Message(eventText, new TargetInfo(base.Position, base.Map, false), MessageTypeDefOf.PositiveEvent);
            }
            else
            {
                Find.LetterStack.ReceiveLetter(ResourceBank.Strings.FishingSuccessTitle.Translate(), eventText, LetterDefOf.PositiveEvent);
            }
        }
    }

}
