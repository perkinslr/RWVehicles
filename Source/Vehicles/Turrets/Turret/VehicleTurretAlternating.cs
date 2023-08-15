﻿using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using SmashTools;

namespace Vehicles
{
	public class VehicleTurretAlternating : VehicleTurret
	{
		public VehicleTurretAlternating(VehiclePawn vehicle) : base(vehicle)
		{
		}

		public VehicleTurretAlternating(VehiclePawn vehicle, VehicleTurretAlternating reference) : base(vehicle, reference)
		{
		}

		public override CompVehicleTurrets.TurretData GenerateTurretData()
		{
			return new CompVehicleTurrets.TurretData()
			{
				shots = CurrentFireMode.shotsPerBurst,
				ticksTillShot = (CurrentFireMode.ticksBetweenShots / GroupTurrets.Count) * GroupTurrets.FindIndex(t => t == this),
				turret = this
			};
		}

		public override IEnumerable<string> ConfigErrors(VehicleDef vehicleDef)
		{
			foreach (string error in base.ConfigErrors(vehicleDef))
			{
				yield return error;
			}
			if (groupKey.NullOrEmpty())
			{
				yield return "<field>groupKey</field> must be populated for a turret of type <type>VehicleTurretAlternating</type>".ConvertRichText();
			}
		}
	}
}
