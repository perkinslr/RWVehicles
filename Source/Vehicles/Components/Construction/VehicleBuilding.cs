﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmashTools;
using UnityEngine;
using Verse;

namespace Vehicles
{
	public class VehicleBuilding : Building, IInspectable
	{
		public VehiclePawn vehicle;

		public VehicleDef VehicleDef
		{
			get
			{
				VehicleBuildDef buildDef = def as VehicleBuildDef;
				return buildDef.thingToSpawn;
			}
		}

		public override void DrawAt(Vector3 drawLoc, bool flip = false)
		{
			if (vehicle != null)
			{
				vehicle.DrawAt(drawLoc, flip);
				if (vehicle.CompVehicleTurrets != null)
				{
					vehicle.CompVehicleTurrets.PostDraw();
				}
			}
			else
			{
				Log.ErrorOnce($"VehicleReference for building {LabelShort} is null. This should not happen unless spawning VehicleBuildings in DevMode.", GetHashCode());
				base.DrawAt(drawLoc, flip);
			}
		}

		public virtual void InspectOpen()
		{
			VehicleInfoCard.Init(VehicleDef);
		}

		public virtual void InspectClose()
		{
			VehicleInfoCard.Clear();
		}

		public virtual void DrawInspectDialog(Rect rect)
		{
			VehicleInfoCard.Draw(rect);
		}

		public virtual float DoInspectPaneButtons(float x)
		{
			Rect rect = new Rect(x, 0f, Extra.IconBarDim, Extra.IconBarDim);
			float usedWidth = 0;

			if (Prefs.DevMode)
			{
				rect.x -= rect.width;
				usedWidth += rect.width;
				{
					//TODO - add devmode options related to constructions
				}
			}
			return usedWidth;
		}

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			vehicle.CompVehicleTurrets?.RevalidateTurrets();
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look(ref vehicle, "vehicle", true);
		}
	}
}
