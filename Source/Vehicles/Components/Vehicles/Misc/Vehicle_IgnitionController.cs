﻿using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using RimWorld;
using RimWorld.Planet;
using SmashTools;

namespace Vehicles
{
	public class Vehicle_IgnitionController : IExposable
	{
		private bool drafted;
		private VehiclePawn vehicle;

		public Vehicle_IgnitionController(VehiclePawn vehicle)
		{
			this.vehicle = vehicle;
		}

		public bool Drafted
		{
			get
			{
				return drafted;
			}
			set
			{
				//Don't trigger events if already set to this draft status
				if (value == Drafted)
				{
					return;
				}

				if (value)
				{
					if (!VehicleMod.settings.debug.debugDraftAnyVehicle && !vehicle.CanDraft(out string reason))
					{
						Messages.Message(reason, MessageTypeDefOf.RejectInput);
						return;
					}
					
					if (vehicle.Spawned)
					{
						vehicle.Map.GetCachedMapComponent<VehicleReservationManager>().ClearReservedFor(vehicle);
					}
				}

				if (!value && vehicle.vPather.curPath != null)
				{
					vehicle.vPather.PatherFailed();
				}

				if (!value)
				{
					vehicle.jobs.ClearQueuedJobs(true);
					if (vehicle.jobs.curJob != null && vehicle.jobs.IsCurrentJobPlayerInterruptible())
					{
						vehicle.jobs.EndCurrentJob(JobCondition.InterruptForced);
					}
					vehicle.vPather.PatherFailed(); //Unecessary, but for exceptions thrown during pathfinding on dedicated thread it serves as a fail-safe
				}
				if (!VehicleMod.settings.main.fishingPersists)
				{
					vehicle.currentlyFishing = false;
				}

				if (vehicle.jobs != null && (value || vehicle.IsFormingCaravan()))
				{
					vehicle.jobs.SetFormingCaravanTick(true);
				}

				drafted = value;

				if (value)
				{
					CompCanBeDormant compCanBeDormant = vehicle.GetCachedComp<CompCanBeDormant>();
					if (compCanBeDormant != null)
					{
						compCanBeDormant.WakeUp();
					}

					vehicle.EventRegistry[VehicleEventDefOf.IgnitionOn].ExecuteEvents();
				}
				else
				{
					vehicle.EventRegistry[VehicleEventDefOf.IgnitionOff].ExecuteEvents();
				}
			}
		}

		private string DraftGizmoLabel
		{
			get
			{
				if (Drafted)
				{
					if (vehicle.vPather.Moving)
					{
						return "VF_StopVehicle".Translate();
					}
					return "VF_UndraftVehicle".Translate();
				}
				return vehicle.VehicleDef.draftLabel;
			}
		}

		private string DraftGizmoDescription
		{
			get
			{
				if (Drafted)
				{
					if (vehicle.vPather.Moving)
					{
						return "VF_StopVehicleDesc".Translate();
					}
					return "VF_UndraftVehicleDesc".Translate();
				}
				return "VF_DraftVehicleDesc".Translate();
			}
		}

		/// <summary>
		/// Draft gizmos for VehiclePawn
		/// </summary>
		public IEnumerable<Gizmo> GetGizmos()
		{
			Command draftCommand = new Command_Toggle()
			{
				hotKey = KeyBindingDefOf.Command_ColonistDraft,
				isActive = () => Drafted,
				toggleAction = delegate ()
				{
					if (Drafted && vehicle.vPather.Moving)
					{
						vehicle.vPather.EngageBrakes();
					}
					else
					{
						Drafted = !Drafted;
					}
				},
				defaultLabel = DraftGizmoLabel,
				defaultDesc = DraftGizmoDescription,
				icon = (Drafted && vehicle.vPather.Moving) ? VehicleTex.HaltVehicle : VehicleTex.DraftVehicle
			};
			if (!Drafted)
			{
				draftCommand.defaultLabel = vehicle.VehicleDef.draftLabel;
			}
			if (!vehicle.CanMove)
			{
				draftCommand.Disable("VF_VehicleUnableToMove".Translate(vehicle));
				Drafted = false;
			}
			if (!Drafted)
			{
				draftCommand.tutorTag = "Draft";
			}
			else
			{
				draftCommand.tutorTag = "Undraft";
			}
			yield return draftCommand;
		}

		public void ExposeData()
		{
			Scribe_Values.Look(ref drafted, nameof(drafted));
			Scribe_References.Look(ref vehicle, nameof(vehicle));
		}
	}
}
