﻿using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using RimWorld.Planet;
using SmashTools;

namespace Vehicles
{
	[HeaderTitle(Label = "VehicleProperties", Translate = true)]
	public class VehicleProperties
	{
		[PostToSettings(Label = "VehicleVisibilityWorldMap", Translate = true, UISettingsType = UISettingsType.SliderFloat)]
		[SliderValues(MinValue = 0, MaxValue = 1, MinValueDisplay = "Invisible", MaxValueDisplay = "FullyVisible", RoundDecimalPlaces = 1)]
		public float visibility = 2.5f;

		[PostToSettings(Label = "VehicleFishingEnabled", Translate = true, UISettingsType = UISettingsType.Checkbox, VehicleType = VehicleType.Sea)]
		public bool fishing = false;
		public float wakeMultiplier = 1.6f;
		public float wakeSpeed = 1.6f;

		//[PostToSettings(Label = "VehicleDamageMultipliers", Tooltip = "VehicleTechLevelDamageMultipliersTooltip", Translate = true, ParentHolder = true)]
		//public VehicleDamageMultipliers techLevelDamageMultipliers = new VehicleDamageMultipliers();

		public List<VehicleJobLimitations> vehicleJobLimitations = new List<VehicleJobLimitations>()
		{
			new VehicleJobLimitations("RepairVehicle", 1),
			new VehicleJobLimitations("LoadUpgradeMaterials", 1),
			new VehicleJobLimitations("UpgradeVehicle", 1),
		};

		public Dictionary<VehicleEventDef, SoundDef> soundOneShotsOnEvent = new Dictionary<VehicleEventDef, SoundDef>();
		//<Start Sustainer, Stop Sustainer>
		public Dictionary<Pair<VehicleEventDef, VehicleEventDef>, SoundDef> soundSustainersOnEvent = new Dictionary<Pair<VehicleEventDef, VehicleEventDef>, SoundDef>();

		public bool diagonalRotation = true;
		[PostToSettings(Label = "ManhunterTargetsVehicle", Tooltip = "ManhunterTargetsVehicleTooltip", Translate = true, UISettingsType = UISettingsType.Checkbox)]
		public bool manhunterTargetsVehicle = false;

		public string healthLabel_Healthy = "MissingHealthyLabel";
		public string healthLabel_Injured = "MissingInjuredLabel";
		public string healthLabel_Immobile = "MissingImmobileLabel";
		public string healthLabel_Dead = "MissingDeadLabel";
		public string healthLabel_Beached = "MissingBeachedLabel";

		public string iconTexPath;
		public bool generateThingIcon = true;

		//---------------   Pathing   ---------------
		public bool defaultTerrainImpassable = false;
		public int pathTurnCost = 10;

		/// <summary>
		/// Do not use snow costs to try and set impassable terrain based on snow depth. It is not designed for that, and if it was it would lag
		/// </summary>
		public Dictionary<SnowCategory, int> customSnowCosts;
		/// <summary>
		/// Set to -1 or >= to 10000 for impassable terrain
		/// </summary>
		public Dictionary<TerrainDef, int> customTerrainCosts;
		/// <summary>
		/// Set to -1 or >= to 10000 for impassable thing
		/// </summary>
		public Dictionary<ThingDef, int> customThingCosts;

		public bool defaultBiomesImpassable = false;
		public Dictionary<RiverDef, float> customRiverCosts = new Dictionary<RiverDef, float>();
		public Dictionary<BiomeDef, float> customBiomeCosts = new Dictionary<BiomeDef, float>();
		public Dictionary<Hilliness, float> customHillinessCosts = new Dictionary<Hilliness, float>();
		public Dictionary<RoadDef, float> customRoadCosts = new Dictionary<RoadDef, float>();
		//-------------------------------------------

		[PostToSettings(Label = "VehicleWinterCostMultiplier", Translate = true, UISettingsType = UISettingsType.SliderFloat)]
		[SliderValues(MinValue = 0, MaxValue = 10, RoundDecimalPlaces = 1)]
		public float winterPathCostMultiplier = 2.5f;
		[PostToSettings(Label = "VehicleWorldSpeedMultiplier", Translate = true, UISettingsType = UISettingsType.SliderFloat)]
		[SliderValues(MinValue = 0, MaxValue = 10, RoundDecimalPlaces = 1)]
		public float worldSpeedMultiplier = 1;

		public List<FactionDef> restrictToFactions;

		public RiverDef riverTraversability;
		public List<VehicleRole> roles  = new List<VehicleRole>();

		public SoundDef soundWhileDrafted; //REDO
		public SoundDef soundWhileMoving; //REDO

		public IEnumerable<string> ConfigErrors()
		{
			if (vehicleJobLimitations.NullOrEmpty())
			{
				yield return "<field>vehicleJobLimitations</field> list must be populated".ConvertRichText();
			}
		}

		public void ResolveReferences(VehicleDef vehicleDef)
		{
			customBiomeCosts ??= new Dictionary<BiomeDef, float>();
			customHillinessCosts ??= new Dictionary<Hilliness, float>();
			customRoadCosts ??= new Dictionary<RoadDef, float>();
			customTerrainCosts ??= new Dictionary<TerrainDef, int>();
			customThingCosts ??= new Dictionary<ThingDef, int>();
			customSnowCosts ??= new Dictionary<SnowCategory, int>();

			//vehicleDamageMultipliers ??= new VehicleDamageMultipliers();

			roles.OrderBy(c => c.hitbox.side == VehicleComponentPosition.BodyNoOverlap).ForEach(c => c.hitbox.Initialize(vehicleDef));
		}
	}
}