using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RBB_Code {

	[StaticConstructorOnStartup]
	internal static class RBB_Initializer {
		public static bool iceFishing = false;
		static RBB_Initializer() {

			if (ModsConfig.ActiveModsInLoadOrder.Any(mod => mod.Name.Contains("Permafrost")) || ModsConfig.ActiveModsInLoadOrder.Any(mod => mod.Name.Contains("Dynamic Terrain"))
			|| ModsConfig.ActiveModsInLoadOrder.Any(mod => mod.Name == "Ice") || ModsConfig.ActiveModsInLoadOrder.Any(mod => mod.Name == "Nature's Pretty Sweet")) {
				iceFishing = true;
			}
			bool hasBulkRecipes = false;
			if (ModsConfig.ActiveModsInLoadOrder.Any(mod => mod.Name.Contains("Feed The Colonists")) || ModsConfig.ActiveModsInLoadOrder.Any(mod => mod.Name.Contains("Simple Bulk Cooking"))) {
                hasBulkRecipes = true;
			}
			if (Controller.Settings.useRecipes.Equals(true)) {
				List<RecipeDef> RecipeDefs = DefDatabase<RecipeDef>.AllDefsListForReading;

                HashSet<string> singleRecipes = new HashSet<string> { "CookMealSushi", "CookMealBoiledShellfish", "CookMealEscargot", "CookMealShrimpScampi" };
                HashSet<string> bulkRecipes = new HashSet<string> { "Cook4MealSushi", "Cook4MealBoiledShellfish", "Cook4MealEscargot", "Cook4MealShrimpScampi" };

                ThingDef fueledStove = getRecipeSafeStove("FueledStove");
                ThingDef electricStove = getRecipeSafeStove("ElectricStove");

                getRecipes(singleRecipes).ForEach(recipe => { fueledStove.recipes.Add(recipe); electricStove.recipes.Add(recipe); });
                if (hasBulkRecipes)
                {
                    getRecipes(bulkRecipes).ForEach(recipe => { fueledStove.recipes.Add(recipe); electricStove.recipes.Add(recipe); });
                }
                flushRecipeCache(fueledStove);
                flushRecipeCache(electricStove);
            }
		}

        static ThingDef getRecipeSafeStove(string defName)
        {
            ThingDef stove = ThingDef.Named(defName);
            if (stove.recipes == null) stove.recipes = new List<RecipeDef>();
            return stove;
        }

        static void flushRecipeCache(ThingDef thing)
        {
            FieldInfo field = thing.GetType().GetField("allRecipesCached", BindingFlags.NonPublic | BindingFlags.Instance);
            object recipeList = field.GetValue(thing);
            if (recipeList != null)
            {
                field.SetValue(thing, null);
            }
        }

        static List<RecipeDef> getRecipes(HashSet<string> recipeNames)
        {
            return DefDatabase<RecipeDef>.AllDefsListForReading.FindAll(recipe => recipeNames.Contains(recipe.defName));
        }
	}

	[DefOf]
	public static class WorkTypeDefOf {
		public static WorkTypeDef Fishing;
	}

	public class Controller : Mod {
		public static Settings Settings;
		public override string SettingsCategory() { return "RBB.Fishing".Translate(); }
		public override void DoSettingsWindowContents(Rect canvas) { Settings.DoWindowContents(canvas); }
		public Controller(ModContentPack content) : base(content) {
			Settings = GetSettings<Settings>();
		}
	}

	public class Settings : ModSettings {
		public float fishingEfficiency = 100.0f;
		public float trapEfficiency = 100.0f;
		public float failureLevel = 1.5f;
		public float junkLevel = 1.5f;
		public float successLevel = 1.5f;
		public float treasureLevel = 2.5f;
		public bool fishersHaul = false;
		public bool useRecipes = true;
		public void DoWindowContents(Rect canvas) {
			Listing_Standard list = new Listing_Standard();
			list.ColumnWidth = canvas.width;
			list.Begin(canvas);
			list.Gap();
			list.Label("RBB.FishingEfficiency".Translate()+"  "+(int)fishingEfficiency+"%");
			fishingEfficiency = list.Slider(fishingEfficiency, 50f, 150.99f);
			Text.Font = GameFont.Tiny;
			list.Label("          "+"RBB.FishingEfficiencyTip".Translate());
			Text.Font = GameFont.Small;
			list.Gap();
			list.Label("RBB.TrapEfficiency".Translate()+"  "+(int)trapEfficiency+"%");
			trapEfficiency = list.Slider(trapEfficiency, 50f, 150.99f);
			Text.Font = GameFont.Tiny;
			list.Label("          "+"RBB.TrapEfficiencyTip".Translate());
			Text.Font = GameFont.Small;
			list.Gap();
			list.Label("RBB.FailureLevel".Translate(failureLevel));
			failureLevel = list.Slider(failureLevel, 0, 4);
			Text.Font = GameFont.Tiny;
			if (failureLevel < 1) { list.Label("          "+"RBB.NoticeNone".Translate()); }
			else if (failureLevel < 2) {
				GUI.contentColor = Color.yellow;
				list.Label("          "+"RBB.NoticeSilent".Translate()); 
				GUI.contentColor = Color.white;
			}
			else if (failureLevel < 3) { list.Label("          "+"RBB.NoticeDing".Translate()); }
			else { list.Label("          "+"RBB.NoticeBox".Translate()); }
			Text.Font = GameFont.Small;
			list.Gap();
			list.Label("RBB.JunkLevel".Translate(junkLevel));
			junkLevel = list.Slider(junkLevel, 0, 4);
			Text.Font = GameFont.Tiny;
			if (junkLevel < 1) { list.Label("          "+"RBB.NoticeNone".Translate()); }
			else if (junkLevel < 2) {
				GUI.contentColor = Color.yellow;
				list.Label("          "+"RBB.NoticeSilent".Translate()); 
				GUI.contentColor = Color.white;
			}
			else if (junkLevel < 3) { list.Label("          "+"RBB.NoticeDing".Translate()); }
			else { list.Label("          "+"RBB.NoticeBox".Translate()); }
			Text.Font = GameFont.Small;
			list.Gap();
			list.Label("RBB.SuccessLevel".Translate(successLevel));
			successLevel = list.Slider(successLevel, 0, 4);
			Text.Font = GameFont.Tiny;
			if (successLevel < 1) { list.Label("          "+"RBB.NoticeNone".Translate()); }
			else if (successLevel < 2) {
				GUI.contentColor = Color.yellow;
				list.Label("          "+"RBB.NoticeSilent".Translate()); 
				GUI.contentColor = Color.white;
			}
			else if (successLevel < 3) { list.Label("          "+"RBB.NoticeDing".Translate()); }
			else { list.Label("          "+"RBB.NoticeBox".Translate()); }
			Text.Font = GameFont.Small;
			list.Gap();
			list.Label("RBB.TreasureLevel".Translate(treasureLevel));
			treasureLevel = list.Slider(treasureLevel, 0, 4);
			Text.Font = GameFont.Tiny;
			if (treasureLevel < 1) { list.Label("          "+"RBB.NoticeNone".Translate()); }
			else if (treasureLevel < 2) { list.Label("          "+"RBB.NoticeSilent".Translate()); }
			else if (treasureLevel < 3) {
				GUI.contentColor = Color.yellow;
				list.Label("          "+"RBB.NoticeDing".Translate()); 
				GUI.contentColor = Color.white;
			}
			else { list.Label("          "+"RBB.NoticeBox".Translate()); }
			Text.Font = GameFont.Small;
			list.Gap();
			list.CheckboxLabeled( "RBB.FishersHaul".Translate(), ref fishersHaul, "RBB.FishersHaulTip".Translate() );
			list.Gap();
			list.CheckboxLabeled( "RBB.UseRecipes".Translate(), ref useRecipes, "RBB.UseRecipesTip".Translate() );
			list.End();
		}
		public override void ExposeData() {
			base.ExposeData();
			Scribe_Values.Look(ref fishingEfficiency, "fishingEfficiency", 100.0f);
			Scribe_Values.Look(ref trapEfficiency, "trapEfficiency", 100.0f);
			Scribe_Values.Look(ref failureLevel, "failureLevel", 1.5f);
			Scribe_Values.Look(ref junkLevel, "junkLevel", 1.5f);
			Scribe_Values.Look(ref successLevel, "successLevel", 1.5f);
			Scribe_Values.Look(ref treasureLevel, "treasureLevel", 2.5f);
			Scribe_Values.Look(ref fishersHaul, "fishersHaul", false);
			Scribe_Values.Look(ref useRecipes, "useRecipes", true);
		}
	}

	public class PlaceWorker_ShellfishTrap : PlaceWorker {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null)
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
                            return new AcceptanceReport("RBB.NotOnFS".Translate());
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
                if (nearLand.Equals(true))
                {
                    List<Thing> shellfishTraps = map.listerThings.ThingsOfDef(ThingDef.Named("ShellfishTrap"));
                    if (shellfishTraps != null && (
                      from building in shellfishTraps
                      where loc.InHorDistOf(building.Position, 6f)
                      select building).Count<Thing>() > 0)
                    {
                        return new AcceptanceReport("RBB.TrapTooClose".Translate());
                    }
                    List<Thing> shellfishTrapBPs = map.listerThings.ThingsOfDef(ThingDef.Named("Blueprint_ShellfishTrap"));
                    if (shellfishTrapBPs != null && (
                      from building in shellfishTrapBPs
                      where loc.InHorDistOf(building.Position, 6f)
                      select building).Count<Thing>() > 0)
                    {
                        return new AcceptanceReport("RBB.TrapTooClose".Translate());
                    }
                    List<Thing> shellfishTrapFrames = map.listerThings.ThingsOfDef(ThingDef.Named("Frame_ShellfishTrap"));
                    if (shellfishTrapFrames != null && (
                      from building in shellfishTrapFrames
                      where loc.InHorDistOf(building.Position, 6f)
                      select building).Count<Thing>() > 0)
                    {
                        return new AcceptanceReport("RBB.TrapTooClose".Translate());
                    }
                    return true;
                }
            }
            return new AcceptanceReport("RBB.ShellfishTrap".Translate());
        }	
	}
	
	public class PlaceWorker_FishingSpot : PlaceWorker {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null)
        {
            TerrainDef terrainDef = map.terrainGrid.TerrainAt(loc);
            if (!terrainDef.defName.Contains("Water") && !terrainDef.defName.Equals("Marsh"))
            {
                return new AcceptanceReport("RBB.FishingSpot1".Translate());
            }
            ThingDef thingDef = checkingDef as ThingDef;
            IntVec3 intVec3 = ThingUtility.InteractionCellWhenAt(thingDef, loc, rot, map);
            if (!intVec3.InBounds(map))
            {
                return new AcceptanceReport("InteractionSpotOutOfBounds".Translate());
            }
            List<Thing> fishingSpots = map.listerThings.ThingsOfDef(ThingDef.Named("RBB_FishingSpot"));
            if (fishingSpots != null && (
              from building in fishingSpots
              where loc.InHorDistOf(building.Position, 9f)
              select building).Count<Thing>() > 0)
            {
                return new AcceptanceReport("RBB.FSTooClose".Translate());
            }
            if (RBB_Initializer.iceFishing.Equals(true))
            {
                List<Thing> icefishingSpots = map.listerThings.ThingsOfDef(ThingDef.Named("RBB_IceFishingSpot"));
                if (icefishingSpots != null && (
                  from building in icefishingSpots
                  where loc.InHorDistOf(building.Position, 9f)
                  select building).Count<Thing>() > 0)
                {
                    return new AcceptanceReport("RBB.FSTooClose".Translate());
                }
                List<Thing> icefishingSpotsBPs = map.listerThings.ThingsOfDef(ThingDef.Named("Blueprint_RBB_IceFishingSpot"));
                if (icefishingSpotsBPs != null && (
                  from building in icefishingSpotsBPs
                  where loc.InHorDistOf(building.Position, 9f)
                  select building).Count<Thing>() > 0)
                {
                    return new AcceptanceReport("RBB.FSTooClose".Translate());
                }
                List<Thing> icefishingSpotsFrames = map.listerThings.ThingsOfDef(ThingDef.Named("Frame_RBB_IceFishingSpot"));
                if (icefishingSpotsFrames != null && (
                  from building in icefishingSpotsFrames
                  where loc.InHorDistOf(building.Position, 9f)
                  select building).Count<Thing>() > 0)
                {
                    return new AcceptanceReport("RBB.FSTooClose".Translate());
                }
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
            return new AcceptanceReport("RBB.FishingSpot2".Translate());
        }
	}

	public class PlaceWorker_IceFishingSpot : PlaceWorker {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null)
        {
            TerrainDef terrainDef = map.terrainGrid.TerrainAt(loc);
            if (!terrainDef.defName.Contains("Ice"))
            {
                return new AcceptanceReport("RBB.FishingSpot1".Translate());
            }
            ThingDef thingDef = checkingDef as ThingDef;
            IntVec3 intVec3 = ThingUtility.InteractionCellWhenAt(thingDef, loc, rot, map);
            if (!intVec3.InBounds(map))
            {
                return new AcceptanceReport("InteractionSpotOutOfBounds".Translate());
            }
            List<Thing> fishingSpots = map.listerThings.ThingsOfDef(ThingDef.Named("RBB_FishingSpot"));
            if (fishingSpots != null && (
              from building in fishingSpots
              where loc.InHorDistOf(building.Position, 9f)
              select building).Count<Thing>() > 0)
            {
                return new AcceptanceReport("RBB.FSTooClose".Translate());
            }
            List<Thing> icefishingSpots = map.listerThings.ThingsOfDef(ThingDef.Named("RBB_IceFishingSpot"));
            if (icefishingSpots != null && (
              from building in icefishingSpots
              where loc.InHorDistOf(building.Position, 9f)
              select building).Count<Thing>() > 0)
            {
                return new AcceptanceReport("RBB.FSTooClose".Translate());
            }
            List<Thing> icefishingSpotsBPs = map.listerThings.ThingsOfDef(ThingDef.Named("Blueprint_RBB_IceFishingSpot"));
            if (icefishingSpotsBPs != null && (
              from building in icefishingSpotsBPs
              where loc.InHorDistOf(building.Position, 9f)
              select building).Count<Thing>() > 0)
            {
                return new AcceptanceReport("RBB.FSTooClose".Translate());
            }
            List<Thing> icefishingSpotsFrames = map.listerThings.ThingsOfDef(ThingDef.Named("Frame_RBB_IceFishingSpot"));
            if (icefishingSpotsFrames != null && (
              from building in icefishingSpotsFrames
              where loc.InHorDistOf(building.Position, 9f)
              select building).Count<Thing>() > 0)
            {
                return new AcceptanceReport("RBB.FSTooClose".Translate());
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
            return new AcceptanceReport("RBB.FishingSpot2".Translate());
        }
	}
	
	public class Building_FishingSpot : Building_WorkTable {
		public IntVec3 fishingSpotCell = new IntVec3(0, 0, 0);
        private int maxFishStock = 3;
        public int fishStock = 2;
        private int fishStockRespawnInterval = 24000;
        private int fishStockRespawnTick = 0;
		public override void SpawnSetup(Map map, bool respawningAfterLoad) {
			base.SpawnSetup(map, respawningAfterLoad);
			fishingSpotCell = this.Position + new IntVec3(0, 0, 0).RotatedBy(this.Rotation);
			if ((this.Map.Biome == BiomeDef.Named("AridShrubland")) || (this.Map.Biome.defName.Contains("DesertArchi")) || (this.Map.Biome.defName.Contains("Boreal"))) {
				maxFishStock = 2;
				fishStock = 1;
				fishStockRespawnInterval = 30000;
			}
			else if (this.Map.Biome.defName.Contains("Oasis")) {
				maxFishStock = 2;
				fishStock = 1;
				fishStockRespawnInterval = 33000;
			}
			else if ((this.Map.Biome.defName.Contains("ColdBog")) || (this.Map.Biome == BiomeDef.Named("Desert")) || (this.Map.Biome.defName.Contains("Tundra"))) {
				maxFishStock = 2;
				fishStock = 1;
				fishStockRespawnInterval = 36000;
			}
			else if (this.Map.Biome.defName.Contains("Permafrost") || this.Map.Biome.defName == "RRP_TemperateDesert") {
				maxFishStock = 1;
				fishStock = 1;
				fishStockRespawnInterval = 42000;
			}
			else if ((this.Map.Biome == BiomeDef.Named("ExtremeDesert")) || (this.Map.Biome == BiomeDef.Named("IceSheet")) || (this.Map.Biome == BiomeDef.Named("SeaIce"))) {
				maxFishStock = 1;
				fishStock = 1;
				fishStockRespawnInterval = 48000;
			}
			TerrainDef terrainDef = map.terrainGrid.TerrainAt(this.fishingSpotCell);
			if (terrainDef.defName.Contains("Moving") || terrainDef.defName.Contains("Ocean")) {
				maxFishStock += 1;
		    }
			if (terrainDef.defName.Contains("Deep")) {
				maxFishStock += 1;
				fishStock += 1;
		    }
        }
		public override void TickRare() {
			base.TickRare();
			fishingSpotCell = this.Position + new IntVec3(0, 0, 0).RotatedBy(this.Rotation);
			TerrainDef terrainDef = base.Map.terrainGrid.TerrainAt(this.fishingSpotCell);
			if (this.def.defName == "RBB_FishingSpot" && !terrainDef.defName.Contains("Water") && !terrainDef.defName.Equals("Marsh")) {
				this.Destroy();
			}
			else if (this.def.defName == "RBB_IceFishingSpot" && !terrainDef.defName.Contains("Ice")) {
				this.Destroy();
			}
			else {
				TerrainDef terrainDefIS = base.Map.terrainGrid.TerrainAt(this.InteractionCell);
				if (terrainDefIS.defName.Contains("Water") || terrainDefIS.defName.Equals("Marsh")) {
					if (!terrainDefIS.defName.Contains("Bridge")) {
						this.Destroy();
					}
				}
			}
			if (fishStock < maxFishStock) {
				if (fishStockRespawnTick == 0) {
					fishStockRespawnTick = Find.TickManager.TicksGame + (int)((float)fishStockRespawnInterval * Rand.Range(0.8f, 1.2f));
					fishStockRespawnTick = (int)(fishStockRespawnTick/(Controller.Settings.fishingEfficiency/100));
				}
				if (Find.TickManager.TicksGame >= fishStockRespawnTick) {
					fishStock++;
					fishStockRespawnTick = 0;
				}
			}
		}
		public override void Destroy(DestroyMode mode = 0) {
        	List<Thing> list = base.Map.thingGrid.ThingsListAt(fishingSpotCell);
        	for (int i = 0; i < list.Count; i++) {
        		if (list[i].def.defName.Contains("Mote_Fishing")) {
					list[i].DeSpawn();
        		}
        	}
			base.Destroy(mode);
		}
	}

	public class Building_ShellfishTrap : Building {
		System.Random rnd = new System.Random();
		private int ticksToCatch;
		public override void ExposeData() {
			base.ExposeData();
			Scribe_Values.Look<int>(ref this.ticksToCatch, "ticksToCatch", 0, false);
		}
		public override void SpawnSetup(Map currentGame, bool respawningAfterLoad) {
			base.SpawnSetup(currentGame, respawningAfterLoad);
			if (this.ticksToCatch <= 0) {
				int Hours1 = rnd.Next(24);
				int Hours2 = rnd.Next(24);
				this.ticksToCatch = (6+Hours1+Hours2)*10;
				if (currentGame.Biome.defName == "AridShrubland" || currentGame.Biome.defName.Contains("DesertArchi") || currentGame.Biome.defName.Contains("Boreal")) {
					int Hours3 = rnd.Next(6);
					int Hours4 = rnd.Next(6);
					this.ticksToCatch += (6+Hours3+Hours4)*10;
				}
				else if (this.Map.Biome.defName.Contains("Oasis")) {
					int Hours3 = rnd.Next(9);
					int Hours4 = rnd.Next(9);
					this.ticksToCatch += (6+Hours3+Hours4)*10;
				}
				else if (currentGame.Biome.defName.Contains("ColdBog") || currentGame.Biome.defName == "Desert" || currentGame.Biome.defName.Contains("Tundra")) {
					int Hours3 = rnd.Next(12);
					int Hours4 = rnd.Next(12);
					this.ticksToCatch += (6+Hours3+Hours4)*10;
				}
				else if (this.Map.Biome.defName.Contains("Permafrost") || this.Map.Biome.defName == "RRP_TemperateDesert") {
					int Hours3 = rnd.Next(18);
					int Hours4 = rnd.Next(18);
					this.ticksToCatch += (6+Hours3+Hours4)*10;
				}
				else if (currentGame.Biome.defName == "IceSheet" || currentGame.Biome.defName == "SeaIce" || currentGame.Biome.defName == "ExtremeDesert") {
					int Hours3 = rnd.Next(24);
					int Hours4 = rnd.Next(24);
					this.ticksToCatch += (6+Hours3+Hours4)*10;
				}
				this.ticksToCatch = (int)(this.ticksToCatch/(Controller.Settings.trapEfficiency/100));
		    }
		}
		public override void TickRare() {
			base.TickRare();
			TerrainDef terrainDef = base.Map.terrainGrid.TerrainAt(this.Position);
			if (terrainDef.defName.Contains("Water") || terrainDef.defName.Equals("Marsh")) {
				if (this.ticksToCatch > 0) { this.ticksToCatch--; }
				if (this.ticksToCatch <= 0) { this.PlaceProduct(); }
			}
			else if (terrainDef.defName.Contains("Ice")) {
				if (Rand.Value > 0.5f) {
					if (this.ticksToCatch > 0) { this.ticksToCatch--; }
					if (this.ticksToCatch <= 0) { this.PlaceProduct(); }
				}
			}
		}
		private void PlaceProduct() {
			float catchType = Rand.Value;
			float emptyCatch = Rand.Value;
			emptyCatch = emptyCatch * (Controller.Settings.trapEfficiency/100);
			float emptyChance = 0;
			Thing trapCatch = null;
			string eventText = "";
			int damage = 0;
			if (Rand.Value <= 0.25) {
				damage = Rand.RangeInclusive(0,10);
				if (Rand.Value < 0.5) {
					damage += Rand.RangeInclusive(0,15);
				}
			}
			if (this.HitPoints <= damage) {
				this.Destroy(DestroyMode.Vanish);
				eventText = "RBB.TrapDestroyed".Translate();
			}
			else {
				this.TakeDamage(new DamageInfo(DamageDefOf.Blunt, damage, 0f, -1f, null, null, null, DamageInfo.SourceCategory.ThingOrUnknown));
				List<Thing> shellfishTraps = base.Map.listerThings.ThingsOfDef(ThingDef.Named("ShellfishTrap"));
				if (shellfishTraps != null) {
					int check21 = (from building in shellfishTraps
					  where base.Position.InHorDistOf(building.Position, 21f)
					  select building).Count<Thing>();
					int check15 = (from building in shellfishTraps
					  where base.Position.InHorDistOf(building.Position, 15f)
					  select building).Count<Thing>();
					int check9 = (from building in shellfishTraps
					  where base.Position.InHorDistOf(building.Position, 9f)
					  select building).Count<Thing>();
					emptyChance = (check21*0.025f)+(check15*0.05f)+(check9*0.1f);
					if (emptyChance > 0.75f) { emptyChance = 0.75f; }
				}
 				if (emptyCatch > emptyChance) {
					TerrainDef terrainDef = base.Map.terrainGrid.TerrainAt(base.Position);
					if (terrainDef.defName.Contains("Ocean")) {
						if (catchType < 0.25) {
							Thing trapCatchItem = ThingMaker.MakeThing(ThingDef.Named("Lobster"), null);
							trapCatchItem.stackCount = rnd.Next(3)+1;
							trapCatch = GenSpawn.Spawn(trapCatchItem, base.Position, base.Map);
							eventText = "RBB.TrapLobster".Translate();
						}
						else if (catchType < 0.5) {
							Thing trapCatchItem = ThingMaker.MakeThing(ThingDef.Named("Crab"), null);
							trapCatchItem.stackCount = rnd.Next(3)+1;
							trapCatch = GenSpawn.Spawn(trapCatchItem, base.Position, base.Map);
							eventText = "RBB.TrapCrab".Translate();
						}
						else if (catchType < 0.75) {
							Thing trapCatchItem = ThingMaker.MakeThing(ThingDef.Named("Shrimp"), null);
							trapCatchItem.stackCount = rnd.Next(8)+2;
							trapCatch = GenSpawn.Spawn(trapCatchItem, base.Position, base.Map);
							eventText = "RBB.TrapShrimp".Translate();
						}
						else {
							Thing trapCatchItem = ThingMaker.MakeThing(ThingDef.Named("RawFishTiny"), null);
							trapCatchItem.stackCount = rnd.Next(6)+2;
							trapCatch = GenSpawn.Spawn(trapCatchItem, base.Position, base.Map);
							eventText = "RBB.TrapFish".Translate();
						}
					}
					else {
						if (catchType < 0.50) {
							Thing trapCatchItem = ThingMaker.MakeThing(ThingDef.Named("Crayfish"), null);
							trapCatchItem.stackCount = rnd.Next(8)+2;
							trapCatch = GenSpawn.Spawn(trapCatchItem, base.Position, base.Map);
							eventText = "RBB.TrapCrayfish".Translate();
						}
						else if (catchType < 0.75) {
							Thing trapCatchItem = ThingMaker.MakeThing(ThingDef.Named("Snail"), null);
							trapCatchItem.stackCount = rnd.Next(8)+2;
							trapCatch = GenSpawn.Spawn(trapCatchItem, base.Position, base.Map);
							eventText = "RBB.TrapSnail".Translate();
						}
						else {
							Thing trapCatchItem = ThingMaker.MakeThing(ThingDef.Named("RawFishTiny"), null);
							trapCatchItem.stackCount = rnd.Next(6)+2;
							trapCatch = GenSpawn.Spawn(trapCatchItem, base.Position, base.Map);
							eventText = "RBB.TrapFish".Translate();
						}
					}
				}
				int Hours1 = rnd.Next(24);
				int Hours2 = rnd.Next(24);
				this.ticksToCatch = (6+Hours1+Hours2)*10;
				if (base.Map.Biome.defName == "AridShrubland" || base.Map.Biome.defName.Contains("DesertArchi") || base.Map.Biome.defName.Contains("Boreal")) {
					int Hours3 = rnd.Next(6);
					int Hours4 = rnd.Next(6);
					this.ticksToCatch += (6+Hours3+Hours4)*10;
				}
				else if (this.Map.Biome.defName.Contains("Oasis")) {
					int Hours3 = rnd.Next(9);
					int Hours4 = rnd.Next(9);
					this.ticksToCatch += (6+Hours3+Hours4)*10;
				}
				else if (base.Map.Biome.defName.Contains("ColdBog") || base.Map.Biome.defName == "Desert" || base.Map.Biome.defName.Contains("Tundra")) {
					int Hours3 = rnd.Next(12);
					int Hours4 = rnd.Next(12);
					this.ticksToCatch += (6+Hours3+Hours4)*10;
				}
				else if (this.Map.Biome.defName.Contains("Permafrost") || this.Map.Biome.defName == "RRP_TemperateDesert") {
					int Hours3 = rnd.Next(18);
					int Hours4 = rnd.Next(18);
					this.ticksToCatch += (6+Hours3+Hours4)*10;
				}
				else if (base.Map.Biome.defName == "IceSheet" || base.Map.Biome.defName == "SeaIce" || base.Map.Biome.defName == "ExtremeDesert") {
					int Hours3 = rnd.Next(24);
					int Hours4 = rnd.Next(24);
					this.ticksToCatch += (6+Hours3+Hours4)*10;
				}
				this.ticksToCatch = (int)(this.ticksToCatch/(Controller.Settings.trapEfficiency/100));
			}
			if (Controller.Settings.successLevel < 1) { }
			else if (Controller.Settings.successLevel < 2 ) {
				Messages.Message(eventText, new TargetInfo(base.Position, base.Map, false), MessageTypeDefOf.SilentInput);
			}
			else if (Controller.Settings.successLevel < 3 ) {
				Messages.Message(eventText, new TargetInfo(base.Position, base.Map, false), MessageTypeDefOf.PositiveEvent);
			}
			else {
				Find.LetterStack.ReceiveLetter("RBB.FishingSuccessTitle".Translate(), eventText, LetterDefOf.PositiveEvent);
			}
		}
	}	

	public class JobDriver_CatchFish : JobDriver {
		public TargetIndex fishingSpotIndex = TargetIndex.A;
		public Mote fishingRodMote = null;
		public override void ExposeData() {
			base.ExposeData();
		}
		public override bool TryMakePreToilReservations(bool errorOnFailed) {
			bool flag = this.pawn.Reserve(this.job.targetA, this.job, 1, -1, null, errorOnFailed);
			return flag;
		}
		protected override IEnumerable<Toil> MakeNewToils() {
			const float baseFishingDuration = 2000f;
			int fishingDuration = (int)baseFishingDuration;
			Building_FishingSpot fishingSpot = this.TargetThingA as Building_FishingSpot;
			Passion passion = Passion.None;
			const float skillGainPerTick = 0.01f;
			float skillGainFactor = 0f;
			this.AddEndCondition(() => {
				var targ = this.pawn.jobs.curJob.GetTarget(fishingSpotIndex).Thing;
				if (targ is Building && !targ.Spawned)
					return JobCondition.Incompletable;
				return JobCondition.Ongoing;
			});
			this.FailOnBurningImmobile(fishingSpotIndex);
			this.rotateToFace = TargetIndex.B;
			yield return Toils_Reserve.Reserve(fishingSpotIndex);
			float fishingSkillLevel = 0f;
			fishingSkillLevel = this.pawn.skills.AverageOfRelevantSkillsFor(WorkTypeDefOf.Fishing);
			float fishingSkillDurationFactor = fishingSkillLevel / 20f;
			fishingDuration = (int)(baseFishingDuration * (1.5f - fishingSkillDurationFactor));
			fishingDuration = (int)(fishingDuration/(Controller.Settings.fishingEfficiency/100));
			yield return Toils_Goto.GotoThing(fishingSpotIndex, fishingSpot.InteractionCell).FailOnDespawnedOrNull(fishingSpotIndex);
			Toil fishToil = new Toil() {
				initAction = () => {
					ThingDef moteDef = null;
					if (fishingSpot.Rotation == Rot4.North) { moteDef = ThingDef.Named("Mote_FishingRodNorth"); }
					else if (fishingSpot.Rotation == Rot4.East) { moteDef = ThingDef.Named("Mote_FishingRodEast"); }
					else if (fishingSpot.Rotation == Rot4.South) { moteDef = ThingDef.Named("Mote_FishingRodSouth"); }
					else { moteDef = ThingDef.Named("Mote_FishingRodWest"); }
					this.fishingRodMote = (Mote)ThingMaker.MakeThing(moteDef, null);
					this.fishingRodMote.exactPosition = fishingSpot.fishingSpotCell.ToVector3Shifted();
					this.fishingRodMote.Scale = 1f;
					GenSpawn.Spawn(this.fishingRodMote, fishingSpot.fishingSpotCell, this.Map);
					WorkTypeDef fishingWorkDef = WorkTypeDefOf.Fishing;
					passion = this.pawn.skills.MaxPassionOfRelevantSkillsFor(fishingWorkDef);
					if (passion == Passion.None) { skillGainFactor = 0.3f; }
					else if (passion == Passion.Minor) { skillGainFactor = 1f; }
					else { skillGainFactor = 1.5f; }
				},
				tickAction = () => {
					this.pawn.skills.Learn(SkillDefOf.Animals, skillGainPerTick * skillGainFactor);
					if (this.ticksLeftThisToil == 1) {
						if (this.fishingRodMote != null) {
							this.fishingRodMote.Destroy();
						}
			        	List<Thing> list = base.Map.thingGrid.ThingsListAt(fishingSpot.fishingSpotCell);
			        	for (int i = 0; i < list.Count; i++) {
			        		if (list[i].def.defName.Contains("Mote_Fishing")) {
								list[i].DeSpawn();
			        		}
			        	}
					}
				},
				defaultDuration = fishingDuration,
				defaultCompleteMode = ToilCompleteMode.Delay
			};
			yield return fishToil.WithProgressBarToilDelay(fishingSpotIndex);
			Toil catchFishToil = new Toil() {
				initAction = () => {
					Job curJob = this.pawn.jobs.curJob;
					Thing fishingCatch = null;
					float biteChance = Rand.Value;
					TerrainDef terrainDef = this.Map.terrainGrid.TerrainAt(fishingSpot.fishingSpotCell);
					if (terrainDef.defName.Equals("Marsh") || this.Map.Biome.defName.Contains("Swamp")) { biteChance -= .0025f; }
					if (biteChance < 0.0025) {
						MoteMaker.ThrowMetaIcon(this.pawn.Position, this.Map, ThingDefOf.Mote_IncapIcon);
						this.pawn.jobs.EndCurrentJob(JobCondition.Incompletable);
						string eventText = "";
						if ((this.Map.Biome.defName.Contains("Tropical")) && !terrainDef.defName.Contains("Ocean") && (Rand.Value < 0.33)) {
							eventText = this.pawn.Name.ToStringShort.CapitalizeFirst() + "RBB.TragedyPiranhaText".Translate();
							Find.LetterStack.ReceiveLetter("RBB.TragedyTitle".Translate(), eventText, LetterDefOf.NegativeEvent, this.pawn);
							int bites = Rand.Range(3,6);
							for (int i = 0; i < bites; i++) {
								this.pawn.TakeDamage(new DamageInfo(DamageDefOf.Bite, Rand.Range(1, 4), 0f, -1f, this.pawn, null, null, DamageInfo.SourceCategory.ThingOrUnknown));
							}
							return;
						}
						if (terrainDef.defName.Contains("Ocean") && (Rand.Value < 0.33)) {
							eventText = this.pawn.Name.ToStringShort.CapitalizeFirst() + "RBB.TragedyJellyfishText".Translate();
							Find.LetterStack.ReceiveLetter("RBB.TragedyTitle".Translate(), eventText, LetterDefOf.NegativeEvent, this.pawn);
							this.pawn.TakeDamage(new DamageInfo(DamageDefOf.Burn, Rand.Range(3, 8), 0f, -1f, this.pawn, null, null, DamageInfo.SourceCategory.ThingOrUnknown));
							return;
						}
						eventText = this.pawn.Name.ToStringShort.CapitalizeFirst() + "RBB.TragedyText".Translate();
						Find.LetterStack.ReceiveLetter("RBB.TragedyTitle".Translate(), eventText, LetterDefOf.NegativeEvent, this.pawn);
						this.pawn.TakeDamage(new DamageInfo(DamageDefOf.Bite, Rand.Range(3, 8), 0f, -1f, this.pawn, null, null, DamageInfo.SourceCategory.ThingOrUnknown));
						return;
					}
					float catchSomethingThreshold = .20f + (fishingSkillLevel / 40f);
					catchSomethingThreshold *= (Controller.Settings.fishingEfficiency/100);
					if ((this.Map.Biome == BiomeDef.Named("AridShrubland")) || (this.Map.Biome.defName.Contains("DesertArchi")) || (this.Map.Biome.defName.Contains("Boreal"))) {
						catchSomethingThreshold -= .05f;
					}
					else if (this.Map.Biome.defName.Contains("Oasis")) {
						catchSomethingThreshold -= .075f;
					}
					else if ((this.Map.Biome.defName.Contains("ColdBog")) || (this.Map.Biome == BiomeDef.Named("Desert")) || (this.Map.Biome.defName.Contains("Tundra"))) {
						catchSomethingThreshold -= .10f;
					}
					else if (this.Map.Biome.defName.Contains("Permafrost") || this.Map.Biome.defName == "RRP_TemperateDesert") {
						catchSomethingThreshold -= .125f;
					}
					else if ((this.Map.Biome == BiomeDef.Named("ExtremeDesert")) || (this.Map.Biome == BiomeDef.Named("IceSheet")) || (this.Map.Biome == BiomeDef.Named("SeaIce"))) {
						catchSomethingThreshold -= .15f;
					}
					bool catchIsSuccessful = (Rand.Value <= catchSomethingThreshold);
					if (catchIsSuccessful == false) {
						string eventText = this.pawn.Name.ToStringShort.CapitalizeFirst() + "RBB.CaughtNothing".Translate();
						if (Rand.Value < 0.75f) {
							MoteMaker.ThrowMetaIcon(this.pawn.Position, this.Map, ThingDefOf.Mote_IncapIcon);
							this.pawn.jobs.EndCurrentJob(JobCondition.Incompletable);
							if (Controller.Settings.failureLevel < 1) { }
							else if (Controller.Settings.failureLevel < 2 ) {
								Messages.Message(eventText, new TargetInfo(this.pawn.Position, this.Map, false), MessageTypeDefOf.SilentInput);
							}
							else if (Controller.Settings.failureLevel < 3 ) {
								Messages.Message(eventText, new TargetInfo(this.pawn.Position, this.Map, false), MessageTypeDefOf.NegativeEvent);
							}
							else {
								Find.LetterStack.ReceiveLetter("RBB.CaughtNothingTitle".Translate(), eventText, LetterDefOf.NegativeEvent, this.pawn);
							}
							return;
						}
						float junkType1 = Rand.Value;
						float junkType2 = Rand.Value;
						if (junkType1 < 0.75f) {
							if (junkType2 < 0.5f) {
								fishingCatch = GenSpawn.Spawn(ThingDefOf.WoodLog, this.pawn.Position, this.Map);
							}
							else {
								fishingCatch = GenSpawn.Spawn(ThingDefOf.Cloth, this.pawn.Position, this.Map);
							}
						}
						else {
							if (junkType2 < 0.4f) {
								fishingCatch = GenSpawn.Spawn(ThingDefOf.WoodLog, this.pawn.Position, this.Map);
							}
							else if (junkType2 < 0.7f) {
								fishingCatch = GenSpawn.Spawn(ThingDefOf.Cloth, this.pawn.Position, this.Map);
							}
							else if (junkType2 < 0.8f) {
								fishingCatch = GenSpawn.Spawn(ThingDefOf.Steel, this.pawn.Position, this.Map);
							}
							else if (junkType2 < 0.85f) {
								fishingCatch = GenSpawn.Spawn(ThingDef.Named("WoolMuffalo"), this.pawn.Position, this.Map);
							}
							else if (junkType2 < 0.9f) {
								fishingCatch = GenSpawn.Spawn(ThingDef.Named("WoolMegasloth"), this.pawn.Position, this.Map);
							}
							else if (junkType2 < 0.975f) {
								fishingCatch = GenSpawn.Spawn(ThingDef.Named("WoolCamel"), this.pawn.Position, this.Map);
							}
							else if (junkType2 < 0.97f) {
								fishingCatch = GenSpawn.Spawn(ThingDef.Named("WoolAlpaca"), this.pawn.Position, this.Map);
							}
							else {
								fishingCatch = GenSpawn.Spawn(ThingDef.Named("Synthread"), this.pawn.Position, this.Map);
							}
						}
						if (Rand.Value < 0.75f) { fishingSpot.fishStock--; }
						eventText = this.pawn.Name.ToStringShort.CapitalizeFirst() + "RBB.SnaggedJunk".Translate() + fishingCatch.def.label + ".";
						if (Controller.Settings.junkLevel < 1) { }
						else if (Controller.Settings.junkLevel < 2 ) {
							Messages.Message(eventText, new TargetInfo(this.pawn.Position, this.Map, false), MessageTypeDefOf.SilentInput);
						}
						else if (Controller.Settings.junkLevel < 3 ) {
							Messages.Message(eventText, new TargetInfo(this.pawn.Position, this.Map, false), MessageTypeDefOf.NegativeEvent);
						}
						else {
							Find.LetterStack.ReceiveLetter("RBB.SnaggedJunkTitle".Translate(), eventText, LetterDefOf.NegativeEvent, this.pawn);
						}
					}
					else {
						float treasureChance = fishingSkillLevel*.0002f;
						if (terrainDef.defName.Contains("Deep")) { treasureChance += .001f; }
						if (Rand.Value <= treasureChance) {
							float treasureType1 = Rand.Value;
							float treasureType2 = Rand.Value;
							if (treasureType1 < 0.75f) {
								fishingCatch = GenSpawn.Spawn(ThingDefOf.Silver, this.pawn.Position, this.Map);
								fishingCatch.stackCount = Rand.RangeInclusive(5, 50);
							}
							else {
								if (treasureType2 < 0.6f) {
									fishingCatch = GenSpawn.Spawn(ThingDefOf.Silver, this.pawn.Position, this.Map);
									fishingCatch.stackCount = Rand.RangeInclusive(10, 100);
								}
								else if (treasureType2 < 0.9f) {
									fishingCatch = GenSpawn.Spawn(ThingDefOf.Gold, this.pawn.Position, this.Map);
									fishingCatch.stackCount = Rand.RangeInclusive(10, 100);
								}
								else if (treasureType2 < 0.96f) {
									fishingCatch = GenSpawn.Spawn(ThingDef.Named("Gun_ChargeRifle"), this.pawn.Position, this.Map);
								}
								else if (treasureType2 < 0.98f) {
									fishingCatch = GenSpawn.Spawn(ThingDef.Named("SimpleProstheticArm"), this.pawn.Position, this.Map);
								}
								else {
									fishingCatch = GenSpawn.Spawn(ThingDef.Named("SimpleProstheticLeg"), this.pawn.Position, this.Map);
								}
							}
							string eventText = this.pawn.Name.ToStringShort.CapitalizeFirst() + "RBB.SunkenTreasureText".Translate() + fishingCatch.stackCount + " " + fishingCatch.def.label + ".";
							if (Controller.Settings.treasureLevel < 1) { }
							else if (Controller.Settings.treasureLevel < 2 ) {
								Messages.Message(eventText, new TargetInfo(this.pawn.Position, this.Map, false), MessageTypeDefOf.SilentInput);
							}
							else if (Controller.Settings.treasureLevel < 3 ) {
								Messages.Message(eventText, new TargetInfo(this.pawn.Position, this.Map, false), MessageTypeDefOf.PositiveEvent);
							}
							else {
								Find.LetterStack.ReceiveLetter("RBB.SunkenTreasureTitle".Translate(), eventText, LetterDefOf.PositiveEvent, this.pawn);
							}
						}
						else {
							float fishType = Rand.Value + catchSomethingThreshold;
							string eventText = this.pawn.Name.ToStringShort.CapitalizeFirst();
							if (fishType >= 0.4f && fishType < 0.9f) {
								fishingCatch = GenSpawn.Spawn(ThingDef.Named("RawFishTiny"), this.pawn.Position, this.Map);
								eventText += "RBB.TinyFish".Translate();
							}
							else if ((terrainDef.defName == "Marsh") && fishType >= 0.9f && fishType < 1.15f) {
								fishingCatch = GenSpawn.Spawn(ThingDef.Named("Eel"), this.pawn.Position, this.Map);
								eventText += "RBB.Eel".Translate();
							}
							else if (fishType >= 1.15f && fishType < 1.4f) {
								fishingCatch = GenSpawn.Spawn(ThingDef.Named("Eel"), this.pawn.Position, this.Map);
								eventText += "RBB.Eel".Translate();
							}
							else {
								float specificFish = Rand.Value;
								if (terrainDef.defName.Contains("Ocean")) {
									if (terrainDef.defName.Contains("Deep") && specificFish > 0.7) {
										if (specificFish > 0.85) {
											fishingCatch = GenSpawn.Spawn(ThingDef.Named("Squid"), this.pawn.Position, this.Map);
											eventText += "RBB.Squid".Translate();
										}
										else {
											fishingCatch = GenSpawn.Spawn(ThingDef.Named("SeaCucumber"), this.pawn.Position, this.Map);
											eventText += "RBB.SeaCucumber".Translate();
										}
									}
									else if (this.Map.Biome.defName == "IceSheet" || this.Map.Biome.defName == "SeaIce") {
										if (specificFish < 0.025) {
											fishingCatch = GenSpawn.Spawn(ThingDef.Named("Salmon"), this.pawn.Position, this.Map);
											eventText += "RBB.Salmon".Translate();
										}
										else if (specificFish < 0.125) {
											fishingCatch = GenSpawn.Spawn(ThingDef.Named("Jellyfish"), this.pawn.Position, this.Map);
											eventText += "RBB.Jellyfish".Translate();
										}
										else if (specificFish < 0.475) {
											fishingCatch = GenSpawn.Spawn(ThingDef.Named("RawFishTiny"), this.pawn.Position, this.Map);
											eventText += "RBB.TinyFish".Translate();
										}
										else if (specificFish > 0.975) {
											fishingCatch = GenSpawn.Spawn(ThingDef.Named("Squid"), this.pawn.Position, this.Map);
											eventText += "RBB.Squid".Translate();
										}
										else if (specificFish > 0.95) {
											fishingCatch = GenSpawn.Spawn(ThingDef.Named("SeaCucumber"), this.pawn.Position, this.Map);
											eventText += "RBB.SeaCucumber".Translate();
										}
										else {
											fishingCatch = GenSpawn.Spawn(ThingDef.Named("RawFish"), this.pawn.Position, this.Map);
											eventText += "RBB.Fish".Translate();
										}
									}
									else if (this.Map.Biome.defName.Contains("Tundra") || this.Map.Biome.defName.Contains("Permafrost")
									|| this.Map.Biome.defName.Contains("Boreal") || this.Map.Biome.defName.Contains("ColdBog")) {
										if (specificFish < 0.15) {
											fishingCatch = GenSpawn.Spawn(ThingDef.Named("Salmon"), this.pawn.Position, this.Map);
											eventText += "RBB.Salmon".Translate();
										}
										else if (specificFish < 0.25) {
											fishingCatch = GenSpawn.Spawn(ThingDef.Named("Jellyfish"), this.pawn.Position, this.Map);
											eventText += "RBB.Jellyfish".Translate();
										}
										else if (specificFish < 0.35) {
											fishingCatch = GenSpawn.Spawn(ThingDef.Named("RawFishTiny"), this.pawn.Position, this.Map);
											eventText += "RBB.TinyFish".Translate();
										}
										else if (specificFish < 0.4) {
											fishingCatch = GenSpawn.Spawn(ThingDef.Named("Bass"),this.pawn.Position, this.Map);
											eventText += "RBB.Bass".Translate();
										}
										else if (specificFish > 0.95) {
											fishingCatch = GenSpawn.Spawn(ThingDef.Named("Squid"), this.pawn.Position, this.Map);
											eventText += "RBB.Squid".Translate();
										}
										else if (specificFish > 0.9) {
											fishingCatch = GenSpawn.Spawn(ThingDef.Named("SeaCucumber"), this.pawn.Position, this.Map);
											eventText += "RBB.SeaCucumber".Translate();
										}
										else {
											fishingCatch = GenSpawn.Spawn(ThingDef.Named("RawFish"), this.pawn.Position, this.Map);
											eventText += "RBB.Fish".Translate();
										}
									}
									else if ((this.Map.Biome.defName.Contains("Temperate") && this.Map.Biome.defName != "RRP_TemperateDesert")
									|| this.Map.Biome.defName.Contains("Steppes") || this.Map.Biome.defName.Contains("Grassland") || this.Map.Biome.defName.Contains("Savanna")) {
										if (specificFish < 0.05) {
											fishingCatch = GenSpawn.Spawn(ThingDef.Named("Salmon"), this.pawn.Position, this.Map);
											eventText += "RBB.Salmon".Translate();
										}
										else if (specificFish < 0.15) {
											fishingCatch = GenSpawn.Spawn(ThingDef.Named("Jellyfish"), this.pawn.Position, this.Map);
											eventText += "RBB.Jellyfish".Translate();
										}
										else if (specificFish < 0.25) {
											fishingCatch = GenSpawn.Spawn(ThingDef.Named("Pufferfish"), this.pawn.Position, this.Map);
											eventText += "RBB.Pufferfish".Translate();
										}
										else if (specificFish < 0.45) {
											fishingCatch = GenSpawn.Spawn(ThingDef.Named("Bass"),this.pawn.Position, this.Map);
											eventText += "RBB.Bass".Translate();
										}
										else if (specificFish > 0.95) {
											fishingCatch = GenSpawn.Spawn(ThingDef.Named("Squid"), this.pawn.Position, this.Map);
											eventText += "RBB.Squid".Translate();
										}
										else if (specificFish > 0.9) {
											fishingCatch = GenSpawn.Spawn(ThingDef.Named("SeaCucumber"), this.pawn.Position, this.Map);
											eventText += "RBB.SeaCucumber".Translate();
										}
										else {
											fishingCatch = GenSpawn.Spawn(ThingDef.Named("RawFish"), this.pawn.Position, this.Map);
											eventText += "RBB.Fish".Translate();
										}
									}
									else if (this.Map.Biome.defName.Contains("Tropical") || this.Map.Biome.defName == "AridShrubland"
									|| this.Map.Biome.defName.Contains("Desert") || this.Map.Biome.defName.Contains("Oasis")) {
										if (specificFish < 0.1) {
											fishingCatch = GenSpawn.Spawn(ThingDef.Named("Jellyfish"), this.pawn.Position, this.Map);
											eventText += "RBB.Jellyfish".Translate();
										}
										else if (specificFish < 0.25) {
											fishingCatch = GenSpawn.Spawn(ThingDef.Named("Pufferfish"), this.pawn.Position, this.Map);
											eventText += "RBB.Pufferfish".Translate();
										}
										else if (specificFish < 0.45) {
											fishingCatch = GenSpawn.Spawn(ThingDef.Named("Bass"),this.pawn.Position, this.Map);
											eventText += "RBB.Bass".Translate();
										}
										else if (specificFish > 0.95) {
											fishingCatch = GenSpawn.Spawn(ThingDef.Named("Squid"), this.pawn.Position, this.Map);
											eventText += "RBB.Squid".Translate();
										}
										else if (specificFish > 0.9) {
											fishingCatch = GenSpawn.Spawn(ThingDef.Named("SeaCucumber"), this.pawn.Position, this.Map);
											eventText += "RBB.SeaCucumber".Translate();
										}
										else {
											fishingCatch = GenSpawn.Spawn(ThingDef.Named("RawFish"), this.pawn.Position, this.Map);
											eventText += "RBB.Fish".Translate();
										}
									}
									else {
										if (specificFish < 0.1) {
											fishingCatch = GenSpawn.Spawn(ThingDef.Named("Jellyfish"), this.pawn.Position, this.Map);
											eventText += "RBB.Jellyfish".Translate();
										}
										else if (specificFish < 0.2) {
											fishingCatch = GenSpawn.Spawn(ThingDef.Named("Pufferfish"), this.pawn.Position, this.Map);
											eventText += "RBB.Pufferfish".Translate();
										}
										else if (specificFish < 0.3) {
											fishingCatch = GenSpawn.Spawn(ThingDef.Named("RawFishTiny"), this.pawn.Position, this.Map);
											eventText += "RBB.TinyFish".Translate();
										}
										else if (specificFish < 0.4) {
											fishingCatch = GenSpawn.Spawn(ThingDef.Named("Bass"),this.pawn.Position, this.Map);
											eventText += "RBB.Bass".Translate();
										}
										else if (specificFish > 0.95) {
											fishingCatch = GenSpawn.Spawn(ThingDef.Named("Squid"), this.pawn.Position, this.Map);
											eventText += "RBB.Squid".Translate();
										}
										else if (specificFish > 0.9) {
											fishingCatch = GenSpawn.Spawn(ThingDef.Named("SeaCucumber"), this.pawn.Position, this.Map);
											eventText += "RBB.SeaCucumber".Translate();
										}
										else {
											fishingCatch = GenSpawn.Spawn(ThingDef.Named("RawFish"), this.pawn.Position, this.Map);
											eventText += "RBB.Fish".Translate();
										}
									}
								}
								else {
									if (this.Map.Biome.defName.Contains("Tundra") || this.Map.Biome.defName.Contains("Permafrost")) {
										if (specificFish < 0.5) {
											fishingCatch = GenSpawn.Spawn(ThingDef.Named("Salmon"), this.pawn.Position, this.Map);
											eventText += "RBB.Salmon".Translate();
										}
										else if (specificFish < 0.7) {
											fishingCatch = GenSpawn.Spawn(ThingDef.Named("Bass"),this.pawn.Position, this.Map);
											eventText += "RBB.Bass".Translate();
										}
										else if (specificFish < 0.75) {
											fishingCatch = GenSpawn.Spawn(ThingDef.Named("Catfish"),this.pawn.Position, this.Map);
											eventText += "RBB.Catfish".Translate();
										}
										else {
											fishingCatch = GenSpawn.Spawn(ThingDef.Named("RawFish"), this.pawn.Position, this.Map);
											eventText += "RBB.Fish".Translate();
										}
									}
									else if (this.Map.Biome.defName.Contains("Boreal") || this.Map.Biome.defName.Contains("ColdBog")) {
										if (specificFish < 0.02) {
											fishingCatch = GenSpawn.Spawn(ThingDef.Named("SturgeonCaviar"), this.pawn.Position, this.Map);
											eventText += "RBB.Sturgeon".Translate();
										}
										else if (specificFish < 0.1) {
											fishingCatch = GenSpawn.Spawn(ThingDef.Named("Sturgeon"), this.pawn.Position, this.Map);
											eventText += "RBB.Sturgeon".Translate();
										}
										else if (specificFish < 0.5) {
											fishingCatch = GenSpawn.Spawn(ThingDef.Named("Salmon"), this.pawn.Position, this.Map);
											eventText += "RBB.Salmon".Translate();
										}
										else if (specificFish < 0.7) {
											fishingCatch = GenSpawn.Spawn(ThingDef.Named("Bass"),this.pawn.Position, this.Map);
											eventText += "RBB.Bass".Translate();
										}
										else if (specificFish < 0.75) {
											fishingCatch = GenSpawn.Spawn(ThingDef.Named("Catfish"),this.pawn.Position, this.Map);
											eventText += "RBB.Catfish".Translate();
										}
										else {
											fishingCatch = GenSpawn.Spawn(ThingDef.Named("RawFish"), this.pawn.Position, this.Map);
											eventText += "RBB.Fish".Translate();
										}
									}
									else if ((this.Map.Biome.defName.Contains("Temperate") && this.Map.Biome.defName != "RRP_TemperateDesert")
									|| this.Map.Biome.defName.Contains("Steppes") || this.Map.Biome.defName.Contains("Grassland") || this.Map.Biome.defName.Contains("Savanna")) {
										if (this.Map.Biome.defName.Contains("Swamp")) {
											if (specificFish < 0.02) {
												fishingCatch = GenSpawn.Spawn(ThingDef.Named("SturgeonCaviar"), this.pawn.Position, this.Map);
												eventText += "RBB.Sturgeon".Translate();
											}
											else if (specificFish < 0.1) {
												fishingCatch = GenSpawn.Spawn(ThingDef.Named("Sturgeon"), this.pawn.Position, this.Map);
												eventText += "RBB.Sturgeon".Translate();
											}
											else if (specificFish < 0.2) {
												fishingCatch = GenSpawn.Spawn(ThingDef.Named("Salmon"), this.pawn.Position, this.Map);
												eventText += "RBB.Salmon".Translate();
											}
											else if (specificFish < 0.45) {
												fishingCatch = GenSpawn.Spawn(ThingDef.Named("Bass"),this.pawn.Position, this.Map);
												eventText += "RBB.Bass".Translate();
											}
											else if (specificFish > 0.75) {
												fishingCatch = GenSpawn.Spawn(ThingDef.Named("Catfish"),this.pawn.Position, this.Map);
												eventText += "RBB.Catfish".Translate();
											}
											else {
												fishingCatch = GenSpawn.Spawn(ThingDef.Named("RawFish"), this.pawn.Position, this.Map);
												eventText += "RBB.Fish".Translate();
											}
										}
										else {
											if (specificFish < 0.04) {
												fishingCatch = GenSpawn.Spawn(ThingDef.Named("SturgeonCaviar"), this.pawn.Position, this.Map);
												eventText += "RBB.Sturgeon".Translate();
											}
											else if (specificFish < 0.2) {
												fishingCatch = GenSpawn.Spawn(ThingDef.Named("Sturgeon"), this.pawn.Position, this.Map);
												eventText += "RBB.Sturgeon".Translate();
											}
											else if (specificFish < 0.3) {
												fishingCatch = GenSpawn.Spawn(ThingDef.Named("Salmon"), this.pawn.Position, this.Map);
												eventText += "RBB.Salmon".Translate();
											}
											else if (specificFish < 0.6) {
												fishingCatch = GenSpawn.Spawn(ThingDef.Named("Bass"),this.pawn.Position, this.Map);
												eventText += "RBB.Bass".Translate();
											}
											else if (specificFish > 0.75) {
												fishingCatch = GenSpawn.Spawn(ThingDef.Named("Catfish"),this.pawn.Position, this.Map);
												eventText += "RBB.Catfish".Translate();
											}
											else {
												fishingCatch = GenSpawn.Spawn(ThingDef.Named("RawFish"), this.pawn.Position, this.Map);
												eventText += "RBB.Fish".Translate();
											}
										}
									}
									else if (this.Map.Biome.defName.Contains("Tropical")) {
										if (specificFish < 0.01) {
											fishingCatch = GenSpawn.Spawn(ThingDef.Named("SturgeonCaviar"), this.pawn.Position, this.Map);
											eventText += "RBB.Sturgeon".Translate();
										}
										else if (specificFish < 0.05) {
											fishingCatch = GenSpawn.Spawn(ThingDef.Named("Sturgeon"), this.pawn.Position, this.Map);
											eventText += "RBB.Sturgeon".Translate();
										}
										else if (specificFish < 0.15) {
											fishingCatch = GenSpawn.Spawn(ThingDef.Named("Arapaima"), this.pawn.Position, this.Map);
											eventText += "RBB.Arapaima".Translate();
										}
										else if (specificFish < 0.45) {
											fishingCatch = GenSpawn.Spawn(ThingDef.Named("Piranha"), this.pawn.Position, this.Map);
											eventText += "RBB.Piranha".Translate();
										}
										else if (specificFish < 0.65) {
											fishingCatch = GenSpawn.Spawn(ThingDef.Named("Bass"),this.pawn.Position, this.Map);
											eventText += "RBB.Bass".Translate();
										}
										else if (specificFish > 0.75) {
											fishingCatch = GenSpawn.Spawn(ThingDef.Named("Catfish"),this.pawn.Position, this.Map);
											eventText += "RBB.Catfish".Translate();
										}
										else {
											fishingCatch = GenSpawn.Spawn(ThingDef.Named("RawFish"), this.pawn.Position, this.Map);
											eventText += "RBB.Fish".Translate();
										}
									}
									else if (this.Map.Biome.defName == "AridShrubland" || this.Map.Biome.defName.Contains("DesertArchi")) {
										if (specificFish < 0.02) {
											fishingCatch = GenSpawn.Spawn(ThingDef.Named("SturgeonCaviar"), this.pawn.Position, this.Map);
											eventText += "RBB.Sturgeon".Translate();
										}
										else if (specificFish < 0.1) {
											fishingCatch = GenSpawn.Spawn(ThingDef.Named("Sturgeon"), this.pawn.Position, this.Map);
											eventText += "RBB.Sturgeon".Translate();
										}
										else if (specificFish < 0.2) {
											fishingCatch = GenSpawn.Spawn(ThingDef.Named("RawFishTiny"), this.pawn.Position, this.Map);
											eventText += "RBB.TinyFish".Translate();
										}
										else if (specificFish < 0.7) {
											fishingCatch = GenSpawn.Spawn(ThingDef.Named("Bass"),this.pawn.Position, this.Map);
											eventText += "RBB.Bass".Translate();
										}
										else if (specificFish > 0.75) {
											fishingCatch = GenSpawn.Spawn(ThingDef.Named("Catfish"),this.pawn.Position, this.Map);
											eventText += "RBB.Catfish".Translate();
										}
										else {
											fishingCatch = GenSpawn.Spawn(ThingDef.Named("RawFish"), this.pawn.Position, this.Map);
											eventText += "RBB.Fish".Translate();
										}
									}
									else if (this.Map.Biome.defName == "Desert" || this.Map.Biome.defName.Contains("Oasis")) {
										if (specificFish < 0.01) {
											fishingCatch = GenSpawn.Spawn(ThingDef.Named("SturgeonCaviar"), this.pawn.Position, this.Map);
											eventText += "RBB.Sturgeon".Translate();
										}
										else if (specificFish < 0.05) {
											fishingCatch = GenSpawn.Spawn(ThingDef.Named("Sturgeon"), this.pawn.Position, this.Map);
											eventText += "RBB.Sturgeon".Translate();
										}
										else if (specificFish < 0.30) {
											fishingCatch = GenSpawn.Spawn(ThingDef.Named("RawFishTiny"), this.pawn.Position, this.Map);
											eventText += "RBB.TinyFish".Translate();
										}
										else if (specificFish < 0.75) {
											fishingCatch = GenSpawn.Spawn(ThingDef.Named("Bass"),this.pawn.Position, this.Map);
											eventText += "RBB.Bass".Translate();
										}
										else {
											fishingCatch = GenSpawn.Spawn(ThingDef.Named("RawFish"), this.pawn.Position, this.Map);
											eventText += "RBB.Fish".Translate();
										}
									}
									else if (this.Map.Biome.defName == "ExtremeDesert" || this.Map.Biome.defName == "RRP_TemperateDesert") {
										if (specificFish < 0.5) {
											fishingCatch = GenSpawn.Spawn(ThingDef.Named("RawFishTiny"), this.pawn.Position, this.Map);
											eventText += "RBB.TinyFish".Translate();
										}
										else if (specificFish < 0.75) {
											fishingCatch = GenSpawn.Spawn(ThingDef.Named("Bass"),this.pawn.Position, this.Map);
											eventText += "RBB.Bass".Translate();
										}
										else {
											fishingCatch = GenSpawn.Spawn(ThingDef.Named("RawFish"), this.pawn.Position, this.Map);
											eventText += "RBB.Fish".Translate();
										}
									}
									else if (this.Map.Biome.defName == "IceSheet" || this.Map.Biome.defName == "SeaIce") {
										if (specificFish < 0.15) {
											fishingCatch = GenSpawn.Spawn(ThingDef.Named("Salmon"), this.pawn.Position, this.Map);
											eventText += "RBB.Salmon".Translate();
										}
										else if (specificFish < 0.65) {
											fishingCatch = GenSpawn.Spawn(ThingDef.Named("RawFishTiny"), this.pawn.Position, this.Map);
											eventText += "RBB.TinyFish".Translate();
										}
										else if (specificFish < 0.75) {
											fishingCatch = GenSpawn.Spawn(ThingDef.Named("Bass"),this.pawn.Position, this.Map);
											eventText += "RBB.Bass".Translate();
										}
										else {
											fishingCatch = GenSpawn.Spawn(ThingDef.Named("RawFish"), this.pawn.Position, this.Map);
											eventText += "RBB.Fish".Translate();
										}
									}
									else {
										if (specificFish < 0.01) {
											fishingCatch = GenSpawn.Spawn(ThingDef.Named("SturgeonCaviar"), this.pawn.Position, this.Map);
											eventText += "RBB.Sturgeon".Translate();
										}
										else if (specificFish < 0.05) {
											fishingCatch = GenSpawn.Spawn(ThingDef.Named("Sturgeon"), this.pawn.Position, this.Map);
											eventText += "RBB.Sturgeon".Translate();
										}
										else if (specificFish < 0.2) {
											fishingCatch = GenSpawn.Spawn(ThingDef.Named("RawFishTiny"), this.pawn.Position, this.Map);
											eventText += "RBB.TinyFish".Translate();
										}
										else if (specificFish < 0.7) {
											fishingCatch = GenSpawn.Spawn(ThingDef.Named("Bass"),this.pawn.Position, this.Map);
											eventText += "RBB.Bass".Translate();
										}
										else if (specificFish > 0.75) {
											fishingCatch = GenSpawn.Spawn(ThingDef.Named("Catfish"),this.pawn.Position, this.Map);
											eventText += "RBB.Catfish".Translate();
										}
										else {
											fishingCatch = GenSpawn.Spawn(ThingDef.Named("RawFish"), this.pawn.Position, this.Map);
											eventText += "RBB.Fish".Translate();
										}
									}
								}
							}
							fishingSpot.fishStock--;
							if (Controller.Settings.successLevel < 1) { }
							else if (Controller.Settings.successLevel < 2 ) {
								Messages.Message(eventText, new TargetInfo(this.pawn.Position, this.Map, false), MessageTypeDefOf.SilentInput);
							}
							else if (Controller.Settings.successLevel < 3 ) {
								Messages.Message(eventText, new TargetInfo(this.pawn.Position, this.Map, false), MessageTypeDefOf.PositiveEvent);
							}
							else {
								Find.LetterStack.ReceiveLetter("RBB.FishingSuccessTitle".Translate(), eventText, LetterDefOf.PositiveEvent, this.pawn);
							}
						}
					}
					this.pawn.carryTracker.TryStartCarry(fishingCatch,fishingCatch.stackCount);
					this.pawn.carryTracker.TryDropCarriedThing(this.pawn.Position, ThingPlaceMode.Near, out fishingCatch);
					if (Controller.Settings.fishersHaul.Equals(true)) {
						IntVec3 storageCell;
						if (StoreUtility.TryFindBestBetterStoreCellFor(fishingCatch, this.pawn, this.Map, StoragePriority.Unstored, this.pawn.Faction, out storageCell, true)) {
							this.pawn.Reserve(fishingCatch, this.job, 1, -1, null);
							this.pawn.Reserve(storageCell, this.job, 1, -1, null);
							this.pawn.CurJob.SetTarget(TargetIndex.B, storageCell);
							this.pawn.CurJob.SetTarget(TargetIndex.A, fishingCatch);
							this.pawn.CurJob.count = 1;
							this.pawn.CurJob.haulMode = HaulMode.ToCellStorage;
						}
						else {
							this.pawn.jobs.EndCurrentJob(JobCondition.Succeeded);
						}
					}
					else {
						this.pawn.jobs.EndCurrentJob(JobCondition.Succeeded);
					}
				}
			};
			yield return catchFishToil;
			yield return Toils_Haul.StartCarryThing(TargetIndex.A);
			Toil carryToCell = Toils_Haul.CarryHauledThingToCell(TargetIndex.B);
			yield return carryToCell;
			yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.B, carryToCell, true);
			yield return Toils_Reserve.Release(fishingSpotIndex);
		}
	}

	public class WorkGiver_CatchFish : WorkGiver_Scanner {
		public override ThingRequest PotentialWorkThingRequest {
			get { return ThingRequest.ForDef(ThingDef.Named("RBB_FishingSpot")); }
		}
		public override PathEndMode PathEndMode {
			get { return PathEndMode.InteractionCell; }
		}
		public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false) {
			if ((t is Building_FishingSpot) == false) { return false; }
			Building_FishingSpot fishingSpot = t as Building_FishingSpot;
			if (fishingSpot.IsBurning()) { return false; }
			if (pawn.Dead || pawn.Downed || pawn.IsBurning()) { return false; }
			if (pawn.CanReserveAndReach(fishingSpot, this.PathEndMode, Danger.Some) == false) { return false; }
            if (fishingSpot.fishStock < 1) { return false; }
			return true;
		}
		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false) {
			Job job = new Job();
			Building_FishingSpot fishingSpot = t as Building_FishingSpot;
			job = new Job(DefDatabase<JobDef>.GetNamed("JobDef_CatchFish"), fishingSpot, fishingSpot.fishingSpotCell);
			return job;
		}
	}

	public class WorkGiver_IceFish : WorkGiver_Scanner {
		public override ThingRequest PotentialWorkThingRequest {
			get { return ThingRequest.ForDef(ThingDef.Named("RBB_IceFishingSpot")); }
		}
		public override PathEndMode PathEndMode {
			get { return PathEndMode.InteractionCell; }
		}
		public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false) {
			if ((t is Building_FishingSpot) == false) { return false; }
			Building_FishingSpot fishingSpot = t as Building_FishingSpot;
			if (fishingSpot.IsBurning()) { return false; }
			if (pawn.Dead || pawn.Downed || pawn.IsBurning()) { return false; }
			if (pawn.CanReserveAndReach(fishingSpot, this.PathEndMode, Danger.Some) == false) { return false; }
			if (fishingSpot.fishStock < 1) { return false; }
			return true;
		}
		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false) {
			Job job = new Job();
			Building_FishingSpot fishingSpot = t as Building_FishingSpot;
			job = new Job(DefDatabase<JobDef>.GetNamed("JobDef_CatchFish"), fishingSpot, fishingSpot.fishingSpotCell);
			return job;
		}
	}
	
}
