﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using RimWorld;
using RimWorld.Planet;
using Verse;
using SmashTools;

namespace Vehicles
{
	[StaticConstructorOnStartup]
	public class VehicleCaravan : Caravan, IVehicleWorldObject
	{
		private static readonly Texture2D SplitCommand = ContentFinder<Texture2D>.Get("UI/Commands/SplitCaravan", true);

		private static MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
		private static Dictionary<ThingDef, Material> materials = new Dictionary<ThingDef, Material>();

		public VehicleCaravan_PathFollower vPather;
		public VehicleCaravan_Tweener vTweener;

		private VehiclePawn leadVehicle;

		public VehicleCaravan() : base()
		{
			vPather = new VehicleCaravan_PathFollower(this);
			vTweener = new VehicleCaravan_Tweener(this);
		}

		public override Vector3 DrawPos => vTweener.TweenedPos;

		public bool CanDismount => true;

		public IEnumerable<VehiclePawn> Vehicles
		{
			get
			{
				foreach (Pawn pawn in PawnsListForReading)
				{
					if (pawn is VehiclePawn vehicle)
					{
						yield return vehicle;
					}
				}
			}
		}

		public IEnumerable<Pawn> DismountedPawns
		{
			get
			{
				foreach (Pawn pawn in PawnsListForReading)
				{
					if (!(pawn is VehiclePawn) && !pawn.IsInVehicle())
					{
						yield return pawn;
					}
				}
			}
		}

		public VehiclePawn LeadVehicle
		{
			get
			{
				if (leadVehicle is null)
				{
					leadVehicle = PawnsListForReading.First(v => v is VehiclePawn) as VehiclePawn;
				}
				return leadVehicle;
			}
		}

		public override Material Material
		{
			get
			{
				VehicleDef leadVehicleDef = (PawnsListForReading.First(v => v is VehiclePawn) as VehiclePawn).VehicleDef;
				if (!materials.ContainsKey(leadVehicleDef))
				{
					var texture = VehicleTex.CachedTextureIcons[leadVehicleDef];
					var material = MaterialPool.MatFrom(texture, ShaderDatabase.WorldOverlayTransparentLit, Color.white, WorldMaterials.WorldObjectRenderQueue);
					materials.Add(leadVehicleDef, material);
				}
				return materials[leadVehicleDef];
			}
		}

		public bool OutOfFuel
		{
			get
			{
				foreach (VehiclePawn vehicle in Vehicles)
				{
					if (vehicle.CompFueledTravel != null && vehicle.CompFueledTravel.Fuel <= 0)
					{
						return true;
					}
				}
				return false;
			}
		}

		public new int TicksPerMove
		{
			get
			{
				return VehicleCaravanTicksPerMoveUtility.GetTicksPerMove(this, null);
			}
		}

		public new string TicksPerMoveExplanation
		{
			get
			{
				StringBuilder stringBuilder = new StringBuilder();
				VehicleCaravanTicksPerMoveUtility.GetTicksPerMove(this, stringBuilder);
				return stringBuilder.ToString();
			}
		}

		public override void Draw()
		{
			float averageTileSize = Find.WorldGrid.averageTileSize;
			float transitionPct = ExpandableWorldObjectsUtility.TransitionPct;
			if (def.expandingIcon && transitionPct > 0f)
			{
				Color color = Material.color;
				float num = 1f - transitionPct;
				propertyBlock.SetColor(ShaderPropertyIDs.Color, new Color(color.r, color.g, color.b, color.a * num));
				DrawQuadTangentialToPlanet(DrawPos, 0.7f * averageTileSize, 0.015f, Material, false, false, propertyBlock);
				return;
			}
			DrawQuadTangentialToPlanet(DrawPos, 0.7f * averageTileSize, 0.015f, Material, false, false, null);
		}

		public void DrawQuadTangentialToPlanet(Vector3 pos, float size, float altOffset, Material material, bool counterClockwise = false, bool useSkyboxLayer = false, MaterialPropertyBlock propertyBlock = null)
		{
			if (material == null)
			{
				Log.Warning("Tried to draw quad with null material.");
				return;
			}
			Vector3 normalized = pos.normalized;
			Vector3 vector;

			Vector2 drawSize = new Vector2(LeadVehicle.VehicleDef.graphicData.drawSize.x, LeadVehicle.VehicleDef.graphicData.drawSize.y);

			if (counterClockwise)
			{
				vector = -normalized;
			}
			else
			{
				vector = normalized;
			}
			int smallerSide = drawSize.x < drawSize.y ? -1 : 1;
			float vehicleSizeX;
			float vehicleSizeY;

			float ratio;

			if (smallerSide == 1)
			{
				ratio = drawSize.x / size;

				vehicleSizeX = size;
				vehicleSizeY = drawSize.y / ratio;
			}
			else
			{
				ratio = drawSize.y / size;

				vehicleSizeX = drawSize.x / ratio;
				vehicleSizeY = size;
			}

			Quaternion q = Quaternion.LookRotation(Vector3.Cross(vector, Vector3.up), vector) * Quaternion.Euler(0, -90f, 0);
			//Swapped X and Y due to using Rot4.West
			//Vector3 s = new Vector3(vehicleSizeY, 1f, vehicleSizeX); 
			Vector3 s = new Vector3(size, 1f, size);
			Matrix4x4 matrix = default;
			matrix.SetTRS(pos + normalized * altOffset, q, s);
			int layer = useSkyboxLayer ? WorldCameraManager.WorldSkyboxLayer : WorldCameraManager.WorldLayer;
			if (propertyBlock != null)
			{
				Graphics.DrawMesh(MeshPool.plane10, matrix, material, layer, null, 0, propertyBlock);
				//Graphics.DrawMesh(MeshPool.plane10, matrix, LeadVehicle.VehicleGraphic.MatAt(Rot4.West, LeadVehicle), layer, null, 0, propertyBlock);
				return;
			}
			Graphics.DrawMesh(MeshPool.plane10, matrix, material, layer);
			//Graphics.DrawMesh(MeshPool.plane10, matrix, LeadVehicle.VehicleGraphic.MatAt(Rot4.West, LeadVehicle), layer);
			//if (LeadVehicle.CompCannons != null)
			//{
			//    Vector3 cPos = pos;

			//    foreach (VehicleTurret cannon in LeadVehicle.CompCannons.Cannons)
			//    {
			//        cPos.y += 0.1f;
			//        Vector3 s2 = new Vector3(cannon.turretDef.graphicData.drawSize.x / ratio, 1f, cannon.turretDef.graphicData.drawSize.y / ratio);
			//        Quaternion q2 = Quaternion.LookRotation(Vector3.Cross(vector, Vector3.up), vector) * Quaternion.Euler(0, cannon.defaultAngleRotated, 0);
			//        matrix.SetTRS(cPos + normalized * altOffset, q2, s2);
			//        Graphics.DrawMesh(MeshPool.plane10, matrix, cannon.CannonMaterial, layer);
			//    }
			//}
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo gizmo in base.GetGizmos())
			{
				//Only pull non-devmode gizmos
				if (!DebugSettings.ShowDevGizmos || !(gizmo is Command command) || command.icon)
				{
					yield return gizmo;
				}
			}

			foreach (VehiclePawn vehicle in Vehicles)
			{
				foreach (VehicleComp vehicleComp in vehicle.AllComps.Where(comp => comp is VehicleComp))
				{
					foreach (Gizmo gizmo in vehicleComp.CompCaravanGizmos())
					{
						yield return gizmo;
					}
				}
			}

			if (IsPlayerControlled)
			{
				if (vPather.Moving)
				{
					yield return new Command_Toggle
					{
						hotKey = KeyBindingDefOf.Misc1,
						isActive = (() => vPather.Paused),
						toggleAction = delegate()
						{
							if (!vPather.Moving)
							{
								return;
							}
							vPather.Paused = !vPather.Paused;
						},
						defaultDesc = "CommandToggleCaravanPauseDesc".Translate(2f.ToString("0.#"), 0.3f.ToStringPercent()),
						icon = TexCommand.PauseCaravan,
						defaultLabel = "CommandPauseCaravan".Translate()
					};
				}
				if (CaravanMergeUtility.ShouldShowMergeCommand)
				{
					yield return CaravanMergeUtility.MergeCommand(this);
				}
				foreach (Gizmo gizmo2 in forage.GetGizmos())
				{
					yield return gizmo2;
				}

				foreach (WorldObject worldObject in Find.WorldObjects.ObjectsAt(base.Tile))
				{
					foreach (Gizmo gizmo3 in worldObject.GetCaravanGizmos(this))
					{
						yield return gizmo3;
					}
				}
			}
			if (Prefs.DevMode && DebugSettings.godMode)
			{
				yield return new Command_Action
				{
					defaultLabel = "Vehicle Dev: Teleport to destination",
					action = delegate()
					{
						Tile = vPather.Destination;
						vPather.StopDead();
					}
				};
			}
			if (this.HasBoat() && (Find.World.CoastDirectionAt(Tile).IsValid || WorldHelper.RiverIsValid(Tile, PawnsListForReading.Where(p => p.IsBoat()).ToList())))
			{
				if(!vPather.Moving && !PawnsListForReading.NotNullAndAny(p => !p.IsBoat()))
				{
					Command_Action dock = new Command_Action();
					dock.icon = VehicleTex.Anchor;
					dock.defaultLabel = Find.WorldObjects.AnySettlementBaseAt(Tile) ? "VF_CommandDockShip".Translate() : "VF_CommandDockShipDisembark".Translate();
					dock.defaultDesc = Find.WorldObjects.AnySettlementBaseAt(Tile) ? "VF_CommandDockShipDesc".Translate(Find.WorldObjects.SettlementBaseAt(Tile)) : "VF_CommandDockShipObjectDesc".Translate();
					dock.action = delegate ()
					{
						List<WorldObject> objects = Find.WorldObjects.ObjectsAt(Tile).ToList();
						if (!objects.All(x => x is Caravan))
						{
							CaravanHelper.ToggleDocking(this, true);
						}
						else
						{
							CaravanHelper.SpawnDockedBoatObject(this);
						}
					};

					yield return dock;
				}
				else if (!vPather.Moving && PawnsListForReading.NotNullAndAny(p => !p.IsBoat()))
				{
					Command_Action undock = new Command_Action
					{
						icon = VehicleTex.UnloadAll,
						defaultLabel = "VF_CommandUndockShip".Translate(),
						defaultDesc = "VF_CommandUndockShipDesc".Translate(Label),
						action = delegate ()
						{
							CaravanHelper.ToggleDocking(this, false);
						}
					};

					yield return undock;
				}
			}
		}

		public void Notify_VehicleTeleported()
		{
			vTweener.ResetTweenedPosToRoot();
			vPather.Notify_Teleported_Int();
		}

		public override void Notify_MemberDied(Pawn member)
		{
			if (!Spawned)
			{
				Log.Error("Caravan member died in an unspawned caravan. Unspawned caravans shouldn't be kept for more than a single frame.");
			}
			if (!PawnsListForReading.NotNullAndAny(x => x is VehiclePawn vehicle && !vehicle.Dead && vehicle.AllPawnsAboard.NotNullAndAny((Pawn y) => y != member && IsOwner(y))))
			{
				RemovePawn(member);
				if (Faction == Faction.OfPlayer)
				{
					Find.LetterStack.ReceiveLetter("LetterLabelAllCaravanColonistsDied".Translate(), "LetterAllCaravanColonistsDied".Translate(Name).CapitalizeFirst(), LetterDefOf.NegativeEvent, new GlobalTargetInfo(Tile), null, null);
				}
				pawns.Clear();
				Find.WorldObjects.Remove(this);
			}
			else
			{
				member.Strip();
				RemovePawn(member);
			}
		}
		
		public override string GetInspectString()
		{
			StringBuilder stringBuilder = new StringBuilder();

			int colonists = 0;
			int animals = 0;
			int prisoners = 0;
			int downed = 0;
			int mentalState = 0;
			int vehicles = 0;

			vehicles++;
			foreach (Pawn pawn in PawnsListForReading)
			{
				if (pawn is VehiclePawn) vehicles++;
				if (pawn.IsColonist) colonists++;
				if (pawn.RaceProps.Animal) animals++;
				if (pawn.IsPrisoner) prisoners++;
				if (pawn.Downed) downed++;
				if (pawn.InMentalState) mentalState++;
			}

			if (vehicles >= 1)
			{
				Dictionary<VehicleDef, int> vehicleCounts = new Dictionary<VehicleDef, int>();
				foreach (VehiclePawn vehicle in Vehicles)
				{
					if (vehicleCounts.ContainsKey(vehicle.VehicleDef))
					{
						vehicleCounts[vehicle.VehicleDef]++;
					}
					else
					{
						vehicleCounts[vehicle.VehicleDef] = 1;
					}
				}

				foreach ((VehicleDef def, int count) in vehicleCounts)
				{
					stringBuilder.Append($"{count} {def.LabelCap}, ");
				}
			}
			stringBuilder.Append("CaravanColonistsCount".Translate(colonists, (colonists != 1) ? Faction.OfPlayer.def.pawnsPlural : Faction.OfPlayer.def.pawnSingular));
			if (animals == 1)
			{
				stringBuilder.Append(", " + "CaravanAnimal".Translate());
			}
			else if (animals > 1)
			{
				stringBuilder.Append(", " + "CaravanAnimalsCount".Translate(animals));
			}
			if (prisoners == 1)
			{
				stringBuilder.Append(", " + "CaravanPrisoner".Translate());
			}
			else if (prisoners > 1)
			{
				stringBuilder.Append(", " + "CaravanPrisonersCount".Translate(prisoners));
			}
			stringBuilder.AppendLine();
			if (mentalState > 0)
			{
				stringBuilder.Append("CaravanPawnsInMentalState".Translate(mentalState));
			}
			if (downed > 0)
			{
				if (mentalState > 0)
				{
					stringBuilder.Append(", ");
				}
				stringBuilder.Append("CaravanPawnsDowned".Translate(downed));
			}
			foreach (VehiclePawn vehicle in Vehicles)
			{
				foreach (VehicleComp vehicleComp in vehicle.AllComps.Where(comp => comp is VehicleComp))
				{
					vehicleComp.CompCaravanInspectString(stringBuilder);
				}
			}
			if (mentalState > 0 || downed > 0)
			{
				stringBuilder.AppendLine();
			}

			if (vPather.Moving)
			{
				if (vPather.ArrivalAction != null)
				{
					stringBuilder.Append(vPather.ArrivalAction.ReportString);
				}
				else if (this.HasBoat())
				{
					stringBuilder.Append("VF_Sailing".Translate());
				}
				else
				{
					stringBuilder.Append("CaravanTraveling".Translate());
				}
			}
			else
			{
				Settlement settlementBase = CaravanVisitUtility.SettlementVisitedNow(this);
				if (!(settlementBase is null))
				{
					stringBuilder.Append("CaravanVisiting".Translate(settlementBase.Label));
				}
				else
				{
					stringBuilder.Append("CaravanWaiting".Translate());
				}
			}
			if (vPather.Moving)
			{
				float estimatedDaysToArrive = VehicleCaravanPathingHelper.EstimatedTicksToArrive(this, true) / 60000f;
				stringBuilder.AppendLine();
				stringBuilder.Append("CaravanEstimatedTimeToDestination".Translate(estimatedDaysToArrive.ToString("0.#")));
			}
			if (AllOwnersDowned)
			{
				stringBuilder.AppendLine();
				stringBuilder.Append("AllCaravanMembersDowned".Translate());
			}
			else if (AllOwnersHaveMentalBreak)
			{
				stringBuilder.AppendLine();
				stringBuilder.Append("AllCaravanMembersMentalBreak".Translate());
			}
			else if (ImmobilizedByMass)
			{
				stringBuilder.AppendLine();
				stringBuilder.Append("CaravanImmobilizedByMass".Translate());
			}
			if (needs.AnyPawnOutOfFood(out string text))
			{
				stringBuilder.AppendLine();
				stringBuilder.Append("CaravanOutOfFood".Translate());
				if (!text.NullOrEmpty())
				{
					stringBuilder.Append(" ");
					stringBuilder.Append(text);
					stringBuilder.Append(".");
				}
			}
			if (!vPather.MovingNow)
			{
				int usedBedCount = beds.GetUsedBedCount();
				stringBuilder.AppendLine();
				stringBuilder.Append(CaravanBedUtility.AppendUsingBedsLabel("CaravanResting".Translate(), usedBedCount));
			}
			else
			{
				string inspectStringLine = carryTracker.GetInspectStringLine();
				if (!inspectStringLine.NullOrEmpty())
				{
					stringBuilder.AppendLine();
					stringBuilder.Append(inspectStringLine);
				}
				string inBedForMedicalReasonsInspectStringLine = beds.GetInBedForMedicalReasonsInspectStringLine();
				if (!inBedForMedicalReasonsInspectStringLine.NullOrEmpty())
				{
					stringBuilder.AppendLine();
					stringBuilder.Append(inBedForMedicalReasonsInspectStringLine);
				}
			}
			return stringBuilder.ToString();
		}

		public override void DrawExtraSelectionOverlays()
		{
			if (IsPlayerControlled && vPather.curPath != null)
			{
				vPather.curPath.DrawPath(this);
			}
			gotoMote.RenderMote();
		}

		public override void PostRemove()
		{
			base.PostRemove();
			vPather.StopDead();
		}

		public override void SpawnSetup()
		{
			base.SpawnSetup();
			vTweener.ResetTweenedPosToRoot();

			//Necessary check for post load, otherwise registry will be null until spawned on map
			foreach (VehiclePawn vehicle in Vehicles)
			{
				vehicle.RegisterEvents();
			}
		}

		public override void Tick()
		{
			base.Tick();
			vPather.PatherTick();
			vTweener.TweenerTick();
			if (vPather.MovingNow)
			{
				foreach (Pawn pawn in pawns)
				{
					if (pawn is VehiclePawn vehicle)
					{
						vehicle.CompFueledTravel?.ConsumeFuelWorld();
					}
				}
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Deep.Look(ref vPather, "vehiclePather", new object[] { this });
		}
	}
}
