﻿using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;
using SmashTools;

namespace Vehicles
{
	public class JobGiver_GotoTravelDestinationVehicle : JobGiver_GotoTravelDestination
	{
		// Amble = Raiders + Vehicle Formations
		// Walk = 
		// Jog = Normal Speed
		// Sprint = Speed Away (escaping raiders?)

		protected override Job TryGiveJob(Pawn pawn)
		{
			if (pawn is VehiclePawn vehicle)
			{
				IntVec3 cell = pawn.mindState.duty.focus.Cell;
				if (vehicle.Position == cell)
				{
					return null;
				}
				if (!vehicle.CanReachVehicle(cell, PathEndMode.Touch, PawnUtility.ResolveMaxDanger(pawn, maxDanger), TraverseMode.ByPawn))
				{
					return null;
				}
				Job job = new Job(JobDefOf.Goto, cell)
				{
					locomotionUrgency = LocomotionUrgency.Jog,
					expiryInterval = jobMaxDuration
				};
				if (vehicle.InhabitedCellsProjected(cell, Rot8.Invalid).Any(cell => pawn.Map.exitMapGrid.IsExitCell(cell)))
				{
					job.exitMapOnArrival = true;
				}
				return job;
			}
			return null;
		}
	}
}