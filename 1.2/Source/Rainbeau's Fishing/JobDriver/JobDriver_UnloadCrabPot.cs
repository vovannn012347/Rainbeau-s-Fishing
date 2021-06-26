using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RBB
{
    public class JobDriver_UnloadCrabPot : JobDriver
    {
        public const int BaseUnloadDuration = 500;
        public const TargetIndex HaulableInd = TargetIndex.A;
        public const TargetIndex StoreCellInd = TargetIndex.B;
        public const TargetIndex CrabPotIndex = TargetIndex.C;

        private BillStoreModeDef storemode;

        protected List<Thing> LoadedFish = new List<Thing>();

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            bool flag = this.pawn.Reserve(this.job.GetTarget(CrabPotIndex), this.job, 1, -1, null, errorOnFailed);
            return flag;
        }

        protected void DropStuff()
        {
            Pawn actor = this.pawn;
            Thing resultingDroppedThing = null;
            foreach(var fish in LoadedFish)
            {
                actor.inventory.innerContainer.TryDrop(fish, ThingPlaceMode.Near, out resultingDroppedThing);
            }
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            Building_ShellfishTrap crabPot = this.job.GetTarget(CrabPotIndex).Thing as Building_ShellfishTrap;
            Pawn actor = this.pawn;

            Toil unloadProcess = new Toil()
            {
                initAction = () =>
                {
                    storemode = crabPot.StoreMode;
                },
                defaultDuration = BaseUnloadDuration,
                defaultCompleteMode = ToilCompleteMode.Delay,
                activeSkill = (() => SkillDefOf.Animals)
            }
                .FailOnDespawnedNullOrForbidden(CrabPotIndex)
                .FailOnBurningImmobile(CrabPotIndex)
                .FailOnThingHavingDesignation(CrabPotIndex, DesignationDefOf.Uninstall)
                .FailOnCannotTouch(CrabPotIndex, PathEndMode.Touch)
                .WithProgressBarToilDelay(CrabPotIndex);

            Toil unloadPot = new Toil()
            {
                initAction = () =>
                {
                    //Log.Message("unloadPot");
                    AddFinishAction(DropStuff);
                    
                    crabPot.LastOpenTick = Find.TickManager.TicksGame;

                    var items = crabPot.GetDirectlyHeldThings();

                    //Log.Message("items");
                    //foreach(var item in items)
                    //{
                    //    Log.Message(item.def.defName + " " + item.stackCount);
                    //}

                    //Log.Message("unloading");
                    while (!MassUtility.IsOverEncumbered(pawn) && items.Count > 0)
                    {
                        int count = Math.Min(MassUtility.CountToPickUpUntilOverEncumbered(actor, items[0]), items[0].stackCount);

                        //Log.Message(count + " " + items[0].stackCount + MassUtility.CountToPickUpUntilOverEncumbered(actor, items[0]));

                        if (items.TryTransferToContainer(items[0], pawn.inventory.innerContainer, count, out Thing resultingItem, false) > 0)
                        {
                            if (!resultingItem.Destroyed && resultingItem.stackCount > 0)
                            {
                                LoadedFish.Add(resultingItem);
                            }
                        }
                    }

                    items.TryDropAll(pawn.Position, pawn.Map, ThingPlaceMode.Near);

                    //actor.jobs.EndCurrentJob(JobCondition.Succeeded);
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };

            this.FailOnDespawnedNullOrForbidden(CrabPotIndex);
            this.FailOnBurningImmobile(CrabPotIndex);
            this.FailOnThingHavingDesignation(CrabPotIndex, DesignationDefOf.Uninstall);
            yield return Toils_Goto.GotoThing(CrabPotIndex, PathEndMode.Touch)
                .FailOnDespawnedNullOrForbidden(CrabPotIndex)
                .FailOnBurningImmobile(CrabPotIndex)
                .FailOnThingHavingDesignation(CrabPotIndex, DesignationDefOf.Uninstall);

            yield return unloadProcess;
            yield return unloadPot;

            //todo: repeatetive haul
            Toil finishAndStartHaul = FinishAndStartHaul();
            yield return finishAndStartHaul;
            yield return Toils_Misc.TakeItemFromInventoryToCarrier(pawn, HaulableInd);
            yield return Toils_Reserve.Reserve(StoreCellInd);
            Toil carryToCell = Toils_Haul.CarryHauledThingToCell(StoreCellInd);
            yield return carryToCell;
            yield return Toils_Haul.PlaceHauledThingInCell(StoreCellInd, carryToCell, storageMode: true, tryStoreInSameStorageIfSpotCantHoldWholeStack: true);
            yield return Toils_Jump.JumpIf(finishAndStartHaul, () => LoadedFish.Count > 0);

        }

        //get a store cell
        //remove targeted thing from list of loadedfish
        //assign carry thing to a target index
        private Toil FinishAndStartHaul()
        {
            Toil toil = new Toil();
            toil.initAction = delegate
            {
                Pawn actor = toil.actor;
                Job curJob = actor.jobs.curJob;

                if (LoadedFish.Count == 0)
                {
                    actor.jobs.EndCurrentJob(JobCondition.Succeeded);
                }
                else if (storemode == BillStoreModeDefOf.DropOnFloor)
                {
                    while (LoadedFish.Count > 0)
                    {
                        if (!GenPlace.TryPlaceThing(LoadedFish[0], actor.Position, actor.Map, ThingPlaceMode.Near))
                        {
                            Log.Error(actor + " could not drop fish product " + LoadedFish[0] + " near " + actor.Position);
                        }
                        LoadedFish.RemoveAt(0);
                    }
                    actor.jobs.EndCurrentJob(JobCondition.Succeeded);
                }
                else //BillStoreModeDefOf.BestStockpile
                {
                    //currently it is 
                    IntVec3 foundCell = IntVec3.Invalid;
                    while (LoadedFish.Count > 0)
                    {
                        if (StoreUtility.TryFindBestBetterStoreCellFor(LoadedFish[0], actor, actor.Map, StoragePriority.Unstored, actor.Faction, out foundCell)){
                            if (foundCell.IsValid)
                            {
                                curJob.SetTarget(StoreCellInd, foundCell);
                                curJob.SetTarget(HaulableInd, LoadedFish[0]);
                                curJob.count = 99999;
                                LoadedFish.RemoveAt(0);
                                break;
                            }
                            else
                            {
                                if (!GenPlace.TryPlaceThing(LoadedFish[0], actor.Position, actor.Map, ThingPlaceMode.Near))
                                {
                                    Log.Error("Could not drop fishing product " + LoadedFish[0] + " near " + actor.Position);
                                }
                                LoadedFish.RemoveAt(0);
                            }
                        }
                        else
                        {
                            if (!GenPlace.TryPlaceThing(LoadedFish[0], actor.Position, actor.Map, ThingPlaceMode.Near))
                            {
                                Log.Error("Could not drop fishing product " + LoadedFish[0] + " near " + actor.Position);
                            }
                            LoadedFish.RemoveAt(0);
                        };
                    }

                    if(LoadedFish.Count == 0 && !foundCell.IsValid)
                    {
                        actor.jobs.EndCurrentJob(JobCondition.Succeeded);
                    }
                }
            };
            return toil;
        }
    }
}
