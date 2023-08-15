﻿using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Vehicles
{
	public class LordToil_PrepareCaravan_GatherDownedPawnsVehicle : LordToil, IDebugLordMeetingPoint
	{
		private IntVec3 meetingPoint;

		public LordToil_PrepareCaravan_GatherDownedPawnsVehicle(IntVec3 meetingPoint)
		{
			this.meetingPoint = meetingPoint;
		}

		public IntVec3 MeetingPoint => meetingPoint;

		public override float? CustomWakeThreshold
		{
			get
			{
				return new float?(0.5f);
			}
		}

		public override bool AllowRestingInBed
		{
			get
			{
				return false;
			}
		}

		public override void UpdateAllDuties()
		{
			foreach (Pawn pawn in lord.ownedPawns)
			{
				if (pawn.IsColonist)
				{
					pawn.mindState.duty = new PawnDuty(DutyDefOf_Vehicles.PrepareVehicleCaravan_GatherDownedPawns);
				}
				else if (pawn is VehiclePawn)
				{
					pawn.mindState.duty = new PawnDuty(DutyDefOf_Vehicles.PrepareVehicleCaravan_WaitVehicle);
				}
				else
				{
					pawn.mindState.duty = new PawnDuty(DutyDefOf.PrepareCaravan_Wait, meetingPoint);
				}
			}
		}

		public override void LordToilTick()
		{
			if (Find.TickManager.TicksGame % 100 == 0)
			{
				List<Pawn> downedPawns = ((LordJob_FormAndSendVehicles)lord.LordJob).downedPawns;
				if (CheckMemo(downedPawns))
				{
					return;
				}
				List<VehiclePawn> vehicles = ((LordJob_FormAndSendVehicles)lord.LordJob).vehicles;
				foreach (VehiclePawn vehicle in vehicles)
				{
					downedPawns.RemoveAll(pawn => vehicle.AllPawnsAboard.Contains(pawn));
				}
				CheckMemo(downedPawns);
			}
		}

		private bool CheckMemo(List<Pawn> pawns)
		{
			if (pawns.NullOrEmpty())
			{
				lord.ReceiveMemo(MemoTrigger.DownedPawnsGathered);
				return true;
			}
			return false;
		}
	}
}
