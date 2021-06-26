using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RBB
{
    public class JobDriver_CatchFish : JobDriver
    {
        const float treasureDeepAdditionalChance = 2f;

        public const float baseFishingDuration = 2000f;

        public const TargetIndex HaulableInd = TargetIndex.A;
        public const TargetIndex StoreCellInd = TargetIndex.B;
        public const TargetIndex FishingSpotIndex = TargetIndex.C;



        private List<ThingDef> caughtItems = new List<ThingDef>();

        protected void ModifyPlayToil(Toil toil)
        {
            //ThingDef moteDef = null;
            //if (fishingSpot.Rotation == Rot4.North) { moteDef = ThingDef.Named("Mote_FishingRodNorth"); }
            //else if (fishingSpot.Rotation == Rot4.East) { moteDef = ThingDef.Named("Mote_FishingRodEast"); }
            //else if (fishingSpot.Rotation == Rot4.South) { moteDef = ThingDef.Named("Mote_FishingRodSouth"); }
            //else { moteDef = ThingDef.Named("Mote_FishingRodWest"); }
            //this.fishingRodMote = (Mote)ThingMaker.MakeThing(moteDef, null);
            //this.fishingRodMote.exactPosition = fishingSpot.fishingSpotCell.ToVector3Shifted();
            //this.fishingRodMote.Scale = 1f;
            //GenSpawn.Spawn(this.fishingRodMote, fishingSpot.fishingSpotCell, this.Map);

            toil.WithEffect(ResourceBank.EffecterDefOf.Fishing_Effecter, FishingSpotIndex);
        }

        public override void ExposeData()
        {
            base.ExposeData();
        }
        

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return this.pawn.Reserve(this.job.GetTarget(FishingSpotIndex), this.job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            Building_FishingSpot fishingSpot = this.job.GetTarget(FishingSpotIndex).Thing as Building_FishingSpot;
            
            int fishingDuration = (int)(baseFishingDuration * Rand.Range(0.2f, 1.8f) * fishingSpot.MapFishTracker.DifficultyMultiplier / (ModController.Settings.FishingEfficiency));

            Thing thingCaught = null;

            yield return Toils_Goto.GotoThing(FishingSpotIndex, fishingSpot.InteractionCell)
                .FailOnDespawnedNullOrForbidden(FishingSpotIndex);
            Toil fishToil = new Toil()
            {
                initAction = () =>
                {
                    if(Find.TickManager.TicksGame > fishingSpot.lastFishingStartTick + 60000 * fishingSpot.FishingFrequency)
                    {
                        fishingSpot.lastFishingStartTick = Find.TickManager.TicksGame;
                    }
                },
                tickAction = () => {
                    this.pawn.skills.Learn(job.def.joySkill, job.def.joyXpPerTick);

                    if (pawn.CurJob.GetTarget(FishingSpotIndex).IsValid)
                        pawn.rotationTracker.FaceCell(pawn.CurJob.GetTarget(FishingSpotIndex).Cell);
                },
                handlingFacing = true,
                defaultDuration = fishingDuration,
                defaultCompleteMode = ToilCompleteMode.Delay
            };
            yield return fishToil.FailOnDespawnedNullOrForbidden(FishingSpotIndex);

            Toil catchFishToil = new Toil()
            {
                initAction = () =>
                {
                    string eventText;
                    if (Rand.Value < pawn.GetStatValue(ResourceBank.StatDefOf.FishingChance))
                    {
                        if (fishingSpot.FishStock > 0 && fishingSpot.FishByWeight.Count > 0)
                        {

                            FishDef fishDefToCatch = fishingSpot.FishByWeight.RandomElementByWeight(item => item.Item1).Item2;


                            if (fishDefToCatch.fishProperties.CanBite && Rand.Value < pawn.GetStatValue(ResourceBank.StatDefOf.FishBiteChance))
                            {
                                float damage = Rand.Range(fishDefToCatch.fishProperties.BiteDamageRange.min, fishDefToCatch.fishProperties.BiteDamageRange.max);

                                eventText = string.Format(ResourceBank.Strings.FishingTradegyText.Translate(), this.pawn.Name.ToStringShort.CapitalizeFirst(), fishDefToCatch.label);
                                Find.LetterStack.ReceiveLetter(ResourceBank.Strings.FishingTradegyTitle.Translate(), eventText, LetterDefOf.NegativeEvent, this.pawn);
                                this.pawn.TakeDamage(new DamageInfo(fishDefToCatch.fishProperties.BiteDamageDef, damage, 0f, -1f, this.pawn, null, null, DamageInfo.SourceCategory.ThingOrUnknown, this.pawn));
                                return;
                            }

                            fishingSpot.FishCaught();

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

                            if (roeFish)
                            {
                                thingCaught = ThingMaker.MakeThing(fishDefToCatch.fishProperties.RoeDef, null);
                            }
                            else
                            {
                                thingCaught = ThingMaker.MakeThing(fishDefToCatch, null);
                            }

                            thingCaught.stackCount = 1;
                            eventText = ResourceBank.Strings.FishingSuccess.Translate(this.pawn.Name.ToStringShort.CapitalizeFirst(), fishDefToCatch.label);

                            NotifyFishingSuccess(eventText);
                        }
                        else
                        if (Rand.Value < ModController.Settings.junkChance)
                        {
                            //catch non-fish
                            float treasureChance = ModController.Settings.treasureChanceStandart;
                            if (fishingSpot.DeepTerrain) { treasureChance *= treasureDeepAdditionalChance; }

                            ThingSetMakerParams parms = default(ThingSetMakerParams);
                            parms.countRange = new IntRange(1, 1);

                            if (Rand.Value <= treasureChance)
                            {
                                parms.maxTotalMass = ModController.Settings.MaxTreasureMass;
                                parms.totalMarketValueRange = new FloatRange(100.0f, ModController.Settings.treasureMaxValue);
                                parms.qualityGenerator = QualityGenerator.Reward;

                                List<Thing> list = ThingSetMakerDefOf.Reward_ItemsStandard.root.Generate(parms);
                                thingCaught = list.RandomElement();

                                eventText = ResourceBank.Strings.FishingCaughtTreasureText.Translate(this.pawn.Name.ToStringShort.CapitalizeFirst(), thingCaught.def.label);
                                NotifyCaughtTreasure(eventText);
                            }
                            else
                            {
                                parms.maxTotalMass = ModController.Settings.MaxJunkMass;
                                parms.qualityGenerator = QualityGenerator.BaseGen;

                                List<Thing> list = ResourceBank.ThingSetMakerDefOf.Fishing_ItemJunk.root.Generate(parms);
                                thingCaught = list.RandomElement();

                                eventText = ResourceBank.Strings.FishingCaughtJunkText.Translate(this.pawn.Name.ToStringShort.CapitalizeFirst(), thingCaught.def.label);
                                NotifyCaughtJunk(eventText);
                            }
                        }
                    }
                    else
                    {
                        eventText = ResourceBank.Strings.FishingCaughtNothingText.Translate(this.pawn.Name.ToStringShort.CapitalizeFirst());
                        MoteMaker.ThrowMetaIcon(this.pawn.Position, this.Map, ThingDefOf.Mote_IncapIcon);
                        NotifyCaughtNothing(eventText);
                        this.pawn.jobs.EndCurrentJob(JobCondition.Incompletable);
                        return;
                    }
                }
            };
            yield return catchFishToil;
            //yield return FinishAndStartHaul(fishingCatch);

            Toil toilFinishAndStartHaul = new Toil();
            toilFinishAndStartHaul.initAction = delegate
            {
                Pawn actor = toilFinishAndStartHaul.actor;
                Job curJob = actor.jobs.curJob;
                Bill bill = curJob.bill;

                Log.Message(bill.recipe.defName);
                Log.Message(thingCaught.def.defName);
                
                if (curJob.bill.GetStoreMode() == BillStoreModeDefOf.DropOnFloor)
                {
                    if (!GenPlace.TryPlaceThing(thingCaught, actor.Position, actor.Map, ThingPlaceMode.Near))
                    {
                        Log.Error(actor + " could not drop recipe product " + thingCaught + " near " + actor.Position);
                    }

                    actor.jobs.EndCurrentJob(JobCondition.Succeeded);
                }
                else
                {
                    IntVec3 foundCell = IntVec3.Invalid;
                    if (curJob.bill.GetStoreMode() == BillStoreModeDefOf.BestStockpile)
                    {
                        StoreUtility.TryFindBestBetterStoreCellFor(thingCaught, actor, actor.Map, StoragePriority.Unstored, actor.Faction, out foundCell);
                    }
                    else if (curJob.bill.GetStoreMode() == BillStoreModeDefOf.SpecificStockpile)
                    {
                        if (curJob.bill.GetStoreZone().Accepts(thingCaught))
                        {
                            StoreUtility.TryFindBestBetterStoreCellForIn(thingCaught, actor, actor.Map, StoragePriority.Unstored, actor.Faction, curJob.bill.GetStoreZone().slotGroup, out foundCell);
                        }
                        else
                        {
                            if (!GenPlace.TryPlaceThing(thingCaught, actor.Position, actor.Map, ThingPlaceMode.Near))
                            {
                                Log.Error(actor + " could not drop recipe product " + thingCaught + " near " + actor.Position);
                            }
                            actor.jobs.EndCurrentJob(JobCondition.Succeeded);
                        }
                    }
                    else
                    {
                        Log.ErrorOnce("Unknown store mode", 9158246);
                    }

                    if (foundCell.IsValid)
                    {
                        actor.carryTracker.TryStartCarry(thingCaught);
                        curJob.SetTarget(HaulableInd, thingCaught);
                        curJob.SetTarget(StoreCellInd, foundCell);
                        curJob.count = 99999;
                    }
                    else
                    {
                        if (!GenPlace.TryPlaceThing(thingCaught, actor.Position, actor.Map, ThingPlaceMode.Near))
                        {
                            Log.Error("Bill doer could not drop product " + thingCaught + " near " + actor.Position);
                        }
                        actor.jobs.EndCurrentJob(JobCondition.Succeeded);
                    }
                }
            };
            yield return toilFinishAndStartHaul;
            yield return Toils_Reserve.Reserve(StoreCellInd);
            Toil carryToCell = Toils_Haul.CarryHauledThingToCell(StoreCellInd);
            yield return carryToCell;
            yield return Toils_Haul.PlaceHauledThingInCell(StoreCellInd, carryToCell, storageMode: true, tryStoreInSameStorageIfSpotCantHoldWholeStack: true);
            Toil recount = new Toil();
            recount.initAction = delegate
            {
                Bill_Production bill_Production = recount.actor.jobs.curJob.bill as Bill_Production;
                if (bill_Production != null && bill_Production.repeatMode == BillRepeatModeDefOf.TargetCount)
                {
                    Map.resourceCounter.UpdateResourceCounts();
                }
            };
            yield return recount;
        }

        //private static Toil FinishAndStartHaul(Thing thingCaught)
        //{
            
        //}

        private void NotifyFishingSuccess(string eventText)
        {
            if (ModController.Settings.SuccessLevel < 1) { }
            else if (ModController.Settings.SuccessLevel < 2)
            {
                Messages.Message(eventText, new TargetInfo(this.pawn.Position, this.Map, false), MessageTypeDefOf.SilentInput);
            }
            else if (ModController.Settings.SuccessLevel < 3)
            {
                Messages.Message(eventText, new TargetInfo(this.pawn.Position, this.Map, false), MessageTypeDefOf.PositiveEvent);
            }
            else
            {
                Find.LetterStack.ReceiveLetter(ResourceBank.Strings.FishingSuccessTitle.Translate(), eventText, LetterDefOf.PositiveEvent, this.pawn);
            }
        }

        private void NotifyCaughtNothing(string eventText)
        {
            if (ModController.Settings.failureLevel < 1)
            { }
            else if (ModController.Settings.failureLevel < 2)
            {
                Messages.Message(eventText, new TargetInfo(this.pawn.Position, this.Map, false), MessageTypeDefOf.SilentInput);
            }
            else if (ModController.Settings.failureLevel < 3)
            {
                Messages.Message(eventText, new TargetInfo(this.pawn.Position, this.Map, false), MessageTypeDefOf.NegativeEvent);
            }
            else
            {
                Find.LetterStack.ReceiveLetter(ResourceBank.Strings.FishingCaughtNothingTitle.Translate(), eventText, LetterDefOf.NegativeEvent, this.pawn);
            }
        }

        private void NotifyCaughtTreasure(string eventText)
        {
            if (ModController.Settings.treasureLevel < 1) { }
            else if (ModController.Settings.treasureLevel < 2)
            {
                Messages.Message(eventText, new TargetInfo(this.pawn.Position, this.Map, false), MessageTypeDefOf.SilentInput);
            }
            else if (ModController.Settings.treasureLevel < 3)
            {
                Messages.Message(eventText, new TargetInfo(this.pawn.Position, this.Map, false), MessageTypeDefOf.PositiveEvent);
            }
            else
            {
                Find.LetterStack.ReceiveLetter(ResourceBank.Strings.FishingCaughtTreasureTitle.Translate(), eventText, LetterDefOf.PositiveEvent, this.pawn);
            }
        }

        private void NotifyCaughtJunk(string eventText)
        {
            if (ModController.Settings.junkLevel < 1) { }
            else if (ModController.Settings.junkLevel < 2)
            {
                Messages.Message(eventText, new TargetInfo(this.pawn.Position, this.Map, false), MessageTypeDefOf.SilentInput);
            }
            else if (ModController.Settings.junkLevel < 3)
            {
                Messages.Message(eventText, new TargetInfo(this.pawn.Position, this.Map, false), MessageTypeDefOf.NegativeEvent);
            }
            else
            {
                Find.LetterStack.ReceiveLetter(ResourceBank.Strings.FishingCaughtJunkTitle.Translate(), eventText, LetterDefOf.NegativeEvent, this.pawn);
            }
        }
    }
}
