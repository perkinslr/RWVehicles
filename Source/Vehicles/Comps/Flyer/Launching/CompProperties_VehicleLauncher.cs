﻿using System.Collections.Generic;
using Verse;
using SmashTools;

namespace Vehicles
{
	[HeaderTitle(Label = "VehicleLauncherProperties", Translate = true)]
	public class CompProperties_VehicleLauncher : VehicleCompProperties
	{
		[PostToSettings(Label = "VehicleFuelConsumptionWorld", Translate = true, Tooltip = "VehicleFuelEfficiencyWorldTooltip", UISettingsType = UISettingsType.SliderPercent, VehicleType = VehicleType.Air)]
		[SliderValues(MinValue = 0.05f, MaxValue = 1f, Increment = 0.05f, RoundDecimalPlaces = 2, EndSymbol = "%")]
		public float fuelConsumptionWorldMultiplier = 0.1f;
		[PostToSettings(Label = "VehicleFlySpeed", Translate = true, Tooltip = "VehicleFlySpeedTooltip", UISettingsType = UISettingsType.SliderFloat, VehicleType = VehicleType.Air)]
		[SliderValues(MinValue = 0.5f, MaxValue = 20.5f, EndValue = 999999f, RoundDecimalPlaces = 1, Increment = 0.5f, MaxValueDisplay = "VehicleTeleportation")]
		public float flySpeed = 1.5f;

		//[PostToSettings(Label = "VehicleRateOfClimb", Translate = true, Tooltip = "VehicleRateOfClimbTooltip", UISettingsType = UISettingsType.SliderFloat, VehicleType = VehicleType.Air)]
		[SliderValues(MinValue = 1, MaxValue = 200, EndValue = 999999f, RoundDecimalPlaces = 0, Increment = 1, MaxValueDisplay = "VehicleInstant")]
		public float rateOfClimb = 10f;
		//[PostToSettings(Label = "VehicleMaxFallRate", Translate = true, Tooltip = "VehicleMaxFallRateTooltip", UISettingsType = UISettingsType.SliderFloat, VehicleType = VehicleType.Air)]
		[SliderValues(MinValue = 1, MaxValue = 101, EndValue = 100000, RoundDecimalPlaces = 1, Increment = 1, MaxValueDisplay = "VehicleInstant")]
		public float maxFallRate = 20;

		//[PostToSettings(Label = "VehicleMaxAltitude", Translate = true, Tooltip = "VehicleMaxAltitudeTooltip", UISettingsType = UISettingsType.IntegerBox, VehicleType = VehicleType.Air)]
		[NumericBoxValues(MinValue = AltitudeMeter.MinimumAltitude, MaxValue = AltitudeMeter.MaximumAltitude)]
		public int maxAltitude = 10000;
		//[PostToSettings(Label = "VehicleLandingAltitude", Translate = true, Tooltip = "VehicleLandingAltitudeTooltip", UISettingsType = UISettingsType.IntegerBox, VehicleType = VehicleType.Air)]
		[NumericBoxValues(MinValue = AltitudeMeter.MinimumAltitude, MaxValue = AltitudeMeter.MaximumAltitude)]
		public int landingAltitude = 1000;

		[PostToSettings(Label = "VehicleReconDistance", Translate = true, Tooltip = "VehicleReconDistanceTooltip", UISettingsType = UISettingsType.SliderInt, VehicleType = VehicleType.Air)]
		[SliderValues(MinValue = 1, MaxValue = 8)]
		public int reconDistance = 1;
		[PostToSettings(Label = "VehicleLaunchFixedMaxDistance", Translate = true, Tooltip = "VehicleLaunchFixedMaxDistanceTooltip", UISettingsType = UISettingsType.SliderInt, VehicleType = VehicleType.Air)]
		[SliderValues(MinValue = 0, MaxValue = 100, MinValueDisplay = "VehicleLaunchFixedMaxDistanceDisabled")]
		public int fixedLaunchDistanceMax = 0;

		[PostToSettings(Label = "VehicleControlInFlight", Translate = true, Tooltip = "VehicleControlInFlightTooltip", UISettingsType = UISettingsType.Checkbox, VehicleType = VehicleType.Air)]
		public bool controlInFlight = true;
		[PostToSettings(Label = "VehicleSpaceFlight", Translate = true, Tooltip = "VehicleSpaceFlightTooltip", UISettingsType = UISettingsType.Checkbox, VehicleType = VehicleType.Air)]
		public bool spaceFlight = false;

		public bool faceDirectionOfTravel = true;
		public bool circleToLand = true;

		public BomberProperties bombing;
		public StrafingProperties strafing;

		public LaunchProtocol launchProtocol;

		public ThingDef skyfallerLeaving;
		public ThingDef skyfallerIncoming;
		public ThingDef skyfallerCrashing;
		public ThingDef skyfallerStrafing;
		public ThingDef skyfallerBombing;

		public CompProperties_VehicleLauncher()
		{
			compClass = typeof(CompVehicleLauncher);
		}

		public override IEnumerable<VehicleStatDef> StatCategoryDefs()
		{
			yield return VehicleStatDefOf.FlightSpeed;
			yield return VehicleStatDefOf.FlightControl;
		}

		public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
		{
			foreach (string error in base.ConfigErrors(parentDef))
			{
				yield return error;
			}
		}
	}
}
