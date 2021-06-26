using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RBB
{

    public class PlaceWorker_IceFishingSpot : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
        {
            TerrainDef terrainDef = map.terrainGrid.TerrainAt(loc);
            if (!terrainDef.defName.Contains("Ice"))
            {
                return new AcceptanceReport(ResourceBank.Strings.AcceptanceFishingSpot1.Translate());
            }
            ThingDef thingDef = checkingDef as ThingDef;
            IntVec3 intVec3 = ThingUtility.InteractionCellWhenAt(thingDef, loc, rot, map);
            if (!intVec3.InBounds(map))
            {
                return new AcceptanceReport("InteractionSpotOutOfBounds".Translate());
            }
            
            List<Thing> things = map.thingGrid.ThingsListAtFast(intVec3);
            for (int j = 0; j < things.Count; j++)
            {
                if (things[j] != thingToIgnore)
                {
                    if (things[j].def.passability == Traversability.Impassable)
                    {
                        return new AcceptanceReport(TranslatorFormattedStringExtensions.Translate("InteractionSpotBlocked", things[j].LabelNoCount).CapitalizeFirst());
                    }
                    Blueprint blueprint = things[j] as Blueprint;
                    if (blueprint != null && blueprint.def.entityDefToBuild.passability == Traversability.Impassable)
                    {
                        return new AcceptanceReport(TranslatorFormattedStringExtensions.Translate("InteractionSpotWillBeBlocked", blueprint.LabelNoCount).CapitalizeFirst());
                    }
                }
            }
            TerrainDef landCheck = map.terrainGrid.TerrainAt(intVec3);
            if (!landCheck.defName.Contains("Water") && landCheck != TerrainDef.Named("Marsh"))
            {
                return true;
            }
            if (landCheck.defName.Contains("Bridge"))
            {
                return true;
            }
            return new AcceptanceReport(ResourceBank.Strings.AcceptanceFishingSpot2.Translate());
        }
    }


}
