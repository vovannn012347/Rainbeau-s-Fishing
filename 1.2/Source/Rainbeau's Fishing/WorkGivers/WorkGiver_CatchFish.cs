using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RBB
{
    public class WorkGiver_CatchFish : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(ResourceBank.ThingDefOf.RBB_FishingSpot);

        public override PathEndMode PathEndMode => PathEndMode.InteractionCell;
        
        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if ((t is Building_FishingSpot) == false) { return false; }
            Building_FishingSpot fishingSpot = t as Building_FishingSpot;
            if (fishingSpot.IsBurning() || fishingSpot.IsForbidden(pawn)) { return false; }
            if (pawn.Dead || pawn.Downed || pawn.IsBurning()) { return false; }
            if (pawn.CanReserveAndReach(fishingSpot, this.PathEndMode, Danger.Some) == false) { return false; }
            if (fishingSpot.FishStock < 1) { return false; }
            if ((fishingSpot as IBillGiver)?.BillStack?.AnyShouldDoNow == false)
            {
                return false;
            }

            if (forced)
            {
                return true;
            }

            if(Find.TickManager.TicksGame < (fishingSpot.lastFishingStartTick + 2500 * fishingSpot.FishingDuration) ||
                Find.TickManager.TicksGame > fishingSpot.lastFishingStartTick + 60000 * fishingSpot.FishingFrequency)
            {
                return true;
            }


            return false;
        }
        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Job job = new Job();
            Building_FishingSpot fishingSpot = t as Building_FishingSpot;
            job = new Job(ResourceBank.JobDefOf.JobDef_CatchFish);
            job.SetTarget(JobDriver_CatchFish.FishingSpotIndex, fishingSpot);
            job.bill = fishingSpot.BillStack.FirstShouldDoNow;

            return job;
        }
    }
}
