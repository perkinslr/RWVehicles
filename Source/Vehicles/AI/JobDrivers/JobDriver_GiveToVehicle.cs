﻿using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;
using Verse.AI;
using RimWorld;
using HarmonyLib;
using SmashTools;

namespace Vehicles
{
	public class JobDriver_GiveToVehicle : JobDriver
	{
		private static FieldInfo countToTransferFieldInfo = AccessTools.Field(typeof(TransferableOneWay), "countToTransfer");

		public virtual Thing Item
		{
			get
			{
				return job.GetTarget(TargetIndex.A).Thing;
			}
		}

		public virtual VehiclePawn Vehicle
		{
			get
			{
				return job.GetTarget(TargetIndex.B).Thing as VehiclePawn;
			}
		}

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			if (!pawn.Reserve(Item, job, 1, -1, null, errorOnFailed))
			{
				return false;
			}
			pawn.ReserveAsManyAsPossible(job.GetTargetQueue(TargetIndex.A), job);
			return true;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDestroyedOrNull(TargetIndex.A);
			this.FailOnDestroyedOrNull(TargetIndex.B);
			this.FailOn(delegate ()
			{
				return !Map.GetCachedMapComponent<VehicleReservationManager>().VehicleListed(Vehicle, ReservationType.LoadVehicle);
			});
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch);
			yield return Toils_Haul.StartCarryThing(TargetIndex.A, false, false, false);
			yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.Touch).FailOnDespawnedNullOrForbidden(TargetIndex.B);
			yield return Toils_General.Wait(25, TargetIndex.None).WithProgressBarToilDelay(TargetIndex.B);
			yield return GiveAsMuchToVehicleAsPossible();
		}

		protected virtual Toil FindNearestVehicle()
		{
			return new Toil
			{
				initAction = delegate ()
				{
					Pawn pawn = CaravanHelper.UsableVehicleWithTheMostFreeSpace(this.pawn);
					if (pawn is null)
					{
						this.pawn.jobs.EndCurrentJob(JobCondition.Incompletable, true);
					}
					else
					{
						job.SetTarget(TargetIndex.B, pawn);
					}
				}
			};
		}

		protected virtual Toil GiveAsMuchToVehicleAsPossible()
		{
			return new Toil
			{
				initAction = delegate ()
				{
					if (Item is null || Item.stackCount == 0)
					{
						pawn.jobs.EndCurrentJob(JobCondition.Incompletable, true);
					}
					else
					{
						int stackCount = Item.stackCount; //store before transfer for transferable recache

						int result = Vehicle.AddOrTransfer(Item, stackCount);
						TransferableOneWay transferable = Vehicle.cargoToLoad.FirstOrDefault(t => t.AnyThing is {def: ThingDef def} && def == Item.def);
                        if (transferable != null)
                        {
							countToTransferFieldInfo.SetValue(transferable, transferable.CountToTransfer - stackCount);
						}
					}
				}
			};
		}
	}
}
