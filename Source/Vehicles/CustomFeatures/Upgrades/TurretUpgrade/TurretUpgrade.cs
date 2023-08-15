﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Vehicles
{
	public class TurretUpgrade : UpgradeNode
	{
		public Dictionary<VehicleTurret, VehicleRole> turretUpgrades = new Dictionary<VehicleTurret, VehicleRole>();

		public Dictionary<VehicleTurret, VehicleHandler> turretsUnlocked = new Dictionary<VehicleTurret, VehicleHandler>();

		public TurretUpgrade()
		{
		}

		public TurretUpgrade(VehiclePawn parent) : base(parent)
		{
		}

		public TurretUpgrade(TurretUpgrade reference, VehiclePawn parent) : base(reference, parent)
		{
			turretUpgrades = reference.turretUpgrades;
			foreach(KeyValuePair<VehicleTurret, VehicleRole> cu in turretUpgrades)
			{
				VehicleTurret newTurret = CompVehicleTurrets.CreateTurret(parent, cu.Key);
				VehicleHandler handler = new VehicleHandler(parent, cu.Value);
				turretsUnlocked.Add(newTurret, handler);
			}
		}

		public override string UpgradeIdName => "TurretUpgrade";

		public override void OnInit()
		{
			base.OnInit();
			if (turretsUnlocked is null)
			{
				turretsUnlocked = new Dictionary<VehicleTurret, VehicleHandler>();
			}

			foreach(KeyValuePair<VehicleTurret,VehicleHandler> cannon in turretsUnlocked)
			{
				if (cannon.Key.uniqueID < 0)
				{
					cannon.Key.uniqueID = VehicleIdManager.Instance.GetNextCannonId();
				}
				if (cannon.Value.uniqueID < 0)
				{
					cannon.Value.uniqueID = VehicleIdManager.Instance.GetNextHandlerId();
				}
			}
		}

		public override void Upgrade()
		{
			vehicle.CompVehicleTurrets.AddTurrets(turretsUnlocked.Keys.ToList());
			vehicle.AddHandlers(turretsUnlocked.Values.ToList());
		}

		public override void Refund()
		{
			vehicle.CompVehicleTurrets.RemoveTurrets(turretsUnlocked.Keys.ToList());
			vehicle.RemoveHandlers(turretsUnlocked.Values.ToList());
		}

		public override void DrawExtraOnGUI(Rect rect)
		{
			//vehicle.DrawCannonTextures(rect, turretsUnlocked.Keys.OrderBy(c => c.drawLayer), vehicle.Pattern, true, vehicle.DrawColor, vehicle.DrawColorTwo, vehicle.DrawColorThree);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Collections.Look(ref turretsUnlocked, "turretsUnlocked", LookMode.Deep, LookMode.Deep);
		}
	}
}
