using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RBB {

    public enum WeightingType
    {
        None,
        Static,
        StaticFactor
    }
    
    public enum TerrainType
    {
        FreshWater,
        SaltWater
    }

    public class UncertainRange
    {
        public float? min;
        public float? fav;
        public float? max;


        public bool CheckOutOfRange(float minVal, float maxVal)
        {
            if (max.HasValue && max < minVal)
            {
                return true;
            }

            if (min.HasValue && min > maxVal)
            {
                return true;
            }

            return false;
        }


        public static double GetWeightPercent(UncertainRange valueRange, double value)
        {
            if (valueRange != null)
            {
                if (valueRange.fav.HasValue)
                {
                    if (value > valueRange.fav)
                    {
                        if (valueRange.max.HasValue)
                        {
                            if (value < valueRange.max)
                            {
                                return (1 + Math.Cos(Math.PI * (value - valueRange.fav.Value) / (valueRange.max.Value - valueRange.fav.Value))) / 2;
                            }
                            else
                            {
                                return 0;
                            }
                        }
                        else
                        {
                            return (1 + Math.Cos(Math.PI * ( 1 - 1/(value + 1 - valueRange.fav.Value)))) / 2;
                        }
                    }
                    else
                    if (value < valueRange.fav)
                    {
                        if (valueRange.min.HasValue)
                        {
                            if (value > valueRange.min)
                            {
                                return (1 + Math.Cos(Math.PI * (value - valueRange.fav.Value) / (valueRange.min.Value - valueRange.fav.Value))) / 2;
                            }
                            else
                            {
                                return 0;
                            }
                        }
                        else
                        {
                            return (1 + Math.Cos(Math.PI * (1 + 1 / (value - valueRange.fav.Value - 1)))) / 2;
                            //return (1 + Math.Cos(Math.PI / (1 + Math.Abs(value - valueRange.fav.Value)))) / 2;
                        }
                    }
                    else
                    {
                        return 1;
                    }
                }
                else if (valueRange.min.HasValue && valueRange.max.HasValue)
                {
                    if (valueRange.min.Value < value && value < valueRange.max.Value)
                    {
                        return (1 + Math.Cos(Math.PI * (1 + 2 * (value - valueRange.min.Value) / (valueRange.max.Value - valueRange.min.Value)))) / 2;
                    }
                    return 0;
                }
                else
                if (valueRange.min.HasValue && valueRange.min.Value < value)
                {
                    return 1;
                }
                else
                if (valueRange.max.HasValue && valueRange.max.Value > value)
                {
                    return 1;
                }

                return 0;
            }

            return 1;
        }
    }

    public class FishProperties
    {
        public WeightingType WeightType;
        public float Weight;
        public float FreshWaterFactor = 1;
        public float SaltWaterFactor = 1;
        public float MarshFactor = 1;

        public bool HasRoe = false;
        public ThingDef RoeDef;
        public IntRange SpawningYearRange;

        public bool CanBite = false;
        public DamageDef BiteDamageDef;
        public FloatRange BiteDamageRange;

        public UncertainRange TemperatureRange;
        //usually from 0 (desserts) to 3000 (rainforest)
        public UncertainRange HumidityRange;
        //in-game: 0 to 1, actual is from 0 to 2 - marsh tiles add 0.8 marshiness to fishing spot
        public UncertainRange SwampinessRange;
    }

}
