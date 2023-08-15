﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using RimWorld;
using SmashTools;

namespace Vehicles
{
	/// <summary>
	/// Grid related method helpers
	/// </summary>
	public static class GenGridVehicles
	{
		/// <summary>
		/// <paramref name="cell"/> is able to be traversed by <paramref name="vehicleDef"/>
		/// </summary>
		/// <param name="cell"></param>
		/// <param name="vehicleDef"></param>
		/// <param name="map"></param>
		public static bool Walkable(this IntVec3 cell, VehicleDef vehicleDef, Map map)
		{
			return map.GetCachedMapComponent<VehicleMapping>()[vehicleDef].VehiclePathGrid.Walkable(cell);
		}

		/// <summary>
		/// Check if <paramref name="cell"/> is standable on <paramref name="map"/> for <paramref name="pawn"/> unknown to be a <see cref="VehiclePawn"/> or not
		/// </summary>
		/// <param name="cell"></param>
		/// <param name="pawn"></param>
		/// <param name="map"></param>
		/// <returns></returns>
		public static bool StandableUnknown(this IntVec3 cell, Pawn pawn, Map map)
		{
			if (pawn is VehiclePawn vehicle)
			{
				return Standable(cell, vehicle, map);
			}
			return GenGrid.Standable(cell, map);
		}

		/// <summary>
		/// <paramref name="cell"/> is able to be stood on for <paramref name="vehicle"/>
		/// </summary>
		/// <param name="cell"></param>
		/// <param name="vehicle"></param>
		/// <param name="map"></param>
		public static bool Standable(this IntVec3 cell, VehiclePawn vehicle, Map map)
		{
			if (!map.GetCachedMapComponent<VehicleMapping>()[vehicle.VehicleDef].VehiclePathGrid.Walkable(cell))
			{
				return false;
			}
			List<Thing> list = map.thingGrid.ThingsListAt(cell);
			foreach (Thing thing in list)
			{
				if (thing != vehicle && thing.def.passability != Traversability.Standable)
				{
					return false;
				}
			}
			return true;
		}

		/// <summary>
		/// <paramref name="cell"/> is able to be stood on for <paramref name="vehicleDef"/>
		/// </summary>
		/// <param name="cell"></param>
		/// <param name="vehicleDef"></param>
		/// <param name="map"></param>
		public static bool Standable(this IntVec3 cell, VehicleDef vehicleDef, Map map)
		{
			if (!map.GetCachedMapComponent<VehicleMapping>()[vehicleDef].VehiclePathGrid.Walkable(cell))
			{
				return false;
			}
			List<Thing> list = map.thingGrid.ThingsListAt(cell);
			foreach (Thing t in list)
			{
				if (t.def.passability != Traversability.Standable)
				{
					return false;
				}
			}
			return true;
		}

		/// <summary>
		/// <paramref name="cell"/> is impassable for <paramref name="vehicleDef"/>
		/// </summary>
		/// <param name="cell"></param>
		/// <param name="map"></param>
		public static bool Impassable(IntVec3 cell, Map map, VehicleDef vehicleDef, Predicate<Thing> extraValidator = null)
		{
			List<Thing> thingList = map.thingGrid.ThingsListAt(cell);
			foreach (Thing thing in thingList)
			{
				if (vehicleDef.properties.customThingCosts.TryGetValue(thing.def, out int value) && value >= VehiclePathGrid.ImpassableCost)
				{
					return true;
				}
				else if (thing.ImpassableForVehicles())
				{
					return true;
				}
				else if (extraValidator != null && extraValidator(thing))
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Impassability check which also handles temporary or additional vehicle mechanics that ignore vanilla fields.
		/// </summary>
		/// <param name="thing"></param>
		public static bool ImpassableForVehicles(this Thing thing)
		{
			return thing.def.passability == Traversability.Impassable || thing.def.IsFence || thing is Building_Door;
		}
	}
}
