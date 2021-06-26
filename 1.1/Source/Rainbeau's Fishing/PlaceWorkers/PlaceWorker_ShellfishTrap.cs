using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RBB
{

    public class PlaceWorker_ShellfishTrap : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
        {
            TerrainDef terrainDef = map.terrainGrid.TerrainAt(loc);
            if (terrainDef.defName.Contains("Water") || terrainDef == TerrainDef.Named("Marsh"))
            {
                List<Thing> things = map.thingGrid.ThingsListAtFast(loc);
                for (int j = 0; j < things.Count; j++)
                {
                    if (things[j] != thingToIgnore)
                    {
                        if (things[j].def.defName == "RBB_FishingSpot")
                        {
                            return new AcceptanceReport(ResourceBank.Strings.AcceptanceNotOnFS.Translate());
                        }
                    }
                }
                bool nearLand = false;
                for (int i = -1; i < 2; i++)
                {
                    for (int j = -1; j < 2; j++)
                    {
                        int x = loc.x + i;
                        int z = loc.z + j;
                        IntVec3 newSpot = new IntVec3(x, 0, z);
                        TerrainDef landCheck = map.terrainGrid.TerrainAt(newSpot);
                        if (!landCheck.defName.Contains("Water") && !(landCheck == TerrainDef.Named("Marsh")))
                        {
                            nearLand = true;
                        }
                        if (landCheck.defName.Contains("Bridge"))
                        {
                            nearLand = true;
                        }
                    }
                }
                if (nearLand)
                {
                    return true;
                }
            }
            return new AcceptanceReport(ResourceBank.Strings.AcceptanceShellfishTrap.Translate());
        }
    }

}
