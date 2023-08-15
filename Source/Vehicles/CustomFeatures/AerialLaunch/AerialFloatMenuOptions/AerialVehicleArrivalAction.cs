﻿using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using RimWorld.Planet;

namespace Vehicles
{
	public abstract class AerialVehicleArrivalAction : IExposable
	{
		protected VehiclePawn vehicle;

		/// <summary>
		/// XML Save/Load initialization
		/// </summary>
		public AerialVehicleArrivalAction()
		{
		}
		
		/// <summary>
		/// Use for programmatic instantiation
		/// </summary>
		/// <param name="vehicle"></param>
		public AerialVehicleArrivalAction(VehiclePawn vehicle)
		{
			this.vehicle = vehicle;
		}

		public virtual bool DestroyOnArrival => false;

		public virtual FloatMenuAcceptanceReport StillValid(int destinationTile) => true;

		public virtual bool ShouldUseLongEvent(int tile) => false;

		public abstract bool Arrived(int tile); //CompVehicleLauncher.inFlight = false

		public virtual void ExposeData()
		{
			Scribe_References.Look(ref vehicle, "vehicle", true);
		}
	}
}
