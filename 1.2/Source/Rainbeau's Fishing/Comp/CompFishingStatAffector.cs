using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using static RimWorld.ListerBuldingOfDefInProximity;

namespace RBB
{
    public class CompFishingStatAffector : CompAffectedByFacilities
    {
        private CompProperties_FishingStatAffector CompProps => (CompProperties_FishingStatAffector)this.props;

        public const int UpdateTicks = 2500;

        private int LastUpdateTick;
        private float cachedValueFinalized;

        public CompFishingStatAffector(): base()
        {
            cachedValueFinalized = 0;
            LastUpdateTick = 0;
        }

        public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
        {
            yield return new StatDrawEntry(StatCategoryDefOf.Building, ResourceBank.Strings.FishingEfficiency.Translate(), GetOffset(this.parent).ToStringByStyle(ResourceBank.StatDefOf.FishingEfficiency.toStringStyle, ToStringNumberSense.Factor), ResourceBank.Strings.FishingEfficiencyTip.Translate(), 99996);
        }

        public float GetOffset(Thing parent)
        {
            if (parent.Spawned)
            {
                if(Find.TickManager.TicksGame < LastUpdateTick + UpdateTicks)
                {
                    return cachedValueFinalized;
                }

                var valueUnfinalized = this.LinkedFacilitiesListForReading.Sum(thing => OffsetFor(thing.def));
                cachedValueFinalized = this.CompProps.curve.Evaluate(valueUnfinalized);
                LastUpdateTick = Find.TickManager.TicksGame;

                return cachedValueFinalized;
            }
            return 0f;
        }

        protected float OffsetFor(ThingDef def)
        {
            return CompProps.defs.FirstOrDefault(d => d.building == def)?.offset ?? 0f;
        }

        public override string CompInspectStringExtra()
        {
            return ResourceBank.Strings.FishingEfficiency.Translate() + GetOffset(this.parent).ToStringByStyle(ResourceBank.StatDefOf.FishingEfficiency.toStringStyle, ToStringNumberSense.Factor);
        }
    }
}
