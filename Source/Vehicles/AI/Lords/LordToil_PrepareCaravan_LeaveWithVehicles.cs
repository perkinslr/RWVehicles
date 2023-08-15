﻿using System.Linq;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using RimWorld;
using SmashTools;

namespace Vehicles
{
	public class LordToil_PrepareCaravan_LeaveWithVehicles : LordToil, IDebugLordMeetingPoint
	{
		private IntVec3 exitSpot;

		public LordToil_PrepareCaravan_LeaveWithVehicles(IntVec3 exitSpot)
		{
			this.exitSpot = exitSpot;
		}

		public IntVec3 MeetingPoint => exitSpot;

		public override bool AllowSatisfyLongNeeds => false;

		public override float? CustomWakeThreshold => new float?(0.5f);

		public override bool AllowRestingInBed => false;

		public override bool AllowSelfTend => false;

		public override void Init()
		{
			base.Init();
			foreach (Pawn pawn in lord.ownedPawns)
			{
				if (pawn.roping != null)
				{
					pawn.roping.BreakAllRopes();
				}
			}
		}

		public override void UpdateAllDuties()
		{
			RotatingList<VehiclePawn> vehicles = lord.ownedPawns.Where(p => p is VehiclePawn).Cast<VehiclePawn>().ToRotatingList();
			foreach (Pawn pawn in lord.ownedPawns)
			{
				if (pawn is VehiclePawn vehicle)
				{
					vehicle.ignition.Drafted = true;
					pawn.mindState.duty = new PawnDuty(DutyDefOf_Vehicles.TravelOrWaitVehicle, exitSpot)
					{
						locomotion = LocomotionUrgency.Jog
					};
					pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
				}
				else
				{
					VehiclePawn nextVehicle = vehicles.Next;
					pawn.mindState.duty = new PawnDuty(DutyDefOf_Vehicles.FollowVehicle, nextVehicle, nextVehicle.VehicleDef.Size.z * 1.5f)
					{
						locomotion = LocomotionUrgency.Jog
					};
				}
			}
		}

		public override void LordToilTick()
		{
			if (Find.TickManager.TicksGame % 100 == 0)
			{
				ExitMapUtility.CheckArrived(lord, lord.ownedPawns, exitSpot, MemoTrigger.ExitMap, (_) => true, (Pawn pawn) => !(pawn is VehiclePawn vehicle) || vehicle.CanMoveFinal);
			}
		}
	}
}
