using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RBB
{
    public class WorkGiver_UnloadCrabPot : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(ResourceBank.ThingDefOf.RBB_ShellfishTrap);

        public override PathEndMode PathEndMode
        {
            get { return PathEndMode.Touch; }
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if ((t is Building_ShellfishTrap) == false) { return false; }
            Building_ShellfishTrap crabPot = t as Building_ShellfishTrap;
            if (crabPot.IsBurning() || crabPot.IsForbidden(pawn)) { return false; }
            if (pawn.Dead || pawn.Downed || pawn.IsBurning()) { return false; }
            if (pawn.CanReserveAndReach(crabPot, this.PathEndMode, Danger.Some) == false) { return false; }
            if (crabPot.GetDirectlyHeldThings().Count == 0)
            {
                return false;
            }

            if (forced)
            {
                return true;
            }
            return Find.TickManager.TicksGame > (crabPot.LastOpenTick + 2500 * crabPot.OpenFrequency);
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Job job = new Job();
            Building_ShellfishTrap crabPot = t as Building_ShellfishTrap;
            job = new Job(ResourceBank.JobDefOf.JobDef_EmptyCrabPot);
            job.SetTarget(JobDriver_UnloadCrabPot.CrabPotIndex, crabPot);
            return job;
        }
    }
}
