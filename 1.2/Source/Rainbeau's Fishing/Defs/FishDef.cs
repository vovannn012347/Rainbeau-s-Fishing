using RimWorld;
using System;
using System.Linq;
using Verse;

namespace RBB
{

    public class FishDef : ThingDef
    {
        const float MinTemperature = -8192;
        const float MaxTemperature = 8192;
        const float MinSwampiness = 0;
        const float MaxSwampiness = 2;
        const float MinHumidity = 0;
        const float MaxHumidity = 8192;

        public FishProperties fishProperties;

        public override void ResolveReferences()
        {
            base.ResolveReferences();
        }

        public override void PostLoad()
        {
            if(fishProperties.TemperatureRange != null)
            {
                if (!fishProperties.TemperatureRange.min.HasValue) fishProperties.TemperatureRange.min = MinTemperature;
                if (!fishProperties.TemperatureRange.max.HasValue) fishProperties.TemperatureRange.max = MaxTemperature;
            }
            else
            {
                fishProperties.TemperatureRange = new UncertainRange
                {
                    min = MinTemperature,
                    max = MaxTemperature
                };
            }

            if (fishProperties.HumidityRange != null)
            {
                if (!fishProperties.HumidityRange.min.HasValue) fishProperties.HumidityRange.min = MinHumidity;
                if (!fishProperties.HumidityRange.max.HasValue) fishProperties.HumidityRange.max = MaxHumidity;
            }
            else
            {
                fishProperties.HumidityRange = new UncertainRange
                {
                    min = MinHumidity,
                    max = MaxHumidity
                };
            }

            if (fishProperties.SwampinessRange != null)
            {
                if (!fishProperties.SwampinessRange.min.HasValue) fishProperties.SwampinessRange.min = MinSwampiness;
                if (!fishProperties.SwampinessRange.max.HasValue) fishProperties.SwampinessRange.max = MaxSwampiness;
            }
            else
            {
                fishProperties.SwampinessRange = new UncertainRange
                {
                    min = MinSwampiness,
                    max = MaxSwampiness
                };
            }
        }

        public FishDef ShallowClone()
        {
            return (FishDef)this.MemberwiseClone();
        }

        public FishDef DeepClone()
        {
            return this.DeepClone();
        }
    }
}
