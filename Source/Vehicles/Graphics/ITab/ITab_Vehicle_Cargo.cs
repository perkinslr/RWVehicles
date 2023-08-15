﻿using System;
using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Vehicles
{
	public class ITab_Vehicle_Cargo : ITab
	{
		private const float TopPadding = 20f;
		private const float ThingIconSize = 28f;
		private const float ThingRowHeight = 28f;
		private const float ThingLeftX = 36f;
		private const float StandardLineHeight = 22f;

		private Vector2 scrollPosition = Vector2.zero;
		private float scrollViewHeight;

		public static readonly Color ThingLabelColor = new Color(0.9f, 0.9f, 0.9f, 1f);
		public static readonly Color HighlightColor = new Color(0.5f, 0.5f, 0.5f, 1f);
		public static readonly Color MissingItemColor = new Color(0.8f, 0, 0, 0.5f);

		private static List<Thing> workingInvList = new List<Thing>();

		public ITab_Vehicle_Cargo()
		{
			size = new Vector2(460f, 450f);
			labelKey = "VF_TabCargo";
		}

		public override bool IsVisible => !Vehicle.beached;

		private VehiclePawn Vehicle
		{
			get
			{
				if(SelPawn is VehiclePawn vehicle)
				{
					return vehicle;
				}
				throw new InvalidOperationException("Cargo tab on non-pawn ship " + SelThing);
			}
		}

		protected override void FillTab()
		{
			Text.Font = GameFont.Small;
			Rect rect = new Rect(0f, TopPadding, size.x, size.y - TopPadding);
			Rect rect2 = rect.ContractedBy(10f);
			Rect position = new Rect(rect2.x, rect2.y, rect2.width, rect2.height);
			Widgets.BeginGroup(position);
			Text.Font = GameFont.Small;
			GUI.color = Color.white;
			Rect outRect = new Rect(0f, 0f, position.width, position.height);
			Rect viewRect = new Rect(0f, 0f, position.width - 16f, scrollViewHeight);
			Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect, true);
			float num = 0f;
			TryDrawMassInfo(ref num, viewRect.width);
			
			if(IsVisible)
			{
				Widgets.ListSeparator(ref num, viewRect.width, "VF_Cargo".Translate());
				workingInvList.Clear();
				workingInvList.AddRange(Vehicle.inventory.innerContainer);
				foreach(Thing t in workingInvList)
				{
					DrawThingRow(ref num, viewRect.width, t, null, true);
				}
				workingInvList.Clear();
			}
			if(IsVisible && !Vehicle.cargoToLoad.NullOrEmpty())
			{
				foreach (TransferableOneWay transferable in Vehicle.cargoToLoad)
				{
					if (transferable.AnyThing != null && transferable.CountToTransfer > 0 && !Vehicle.inventory.innerContainer.Contains(transferable.AnyThing))
					{
						DrawThingRow(ref num, viewRect.width, transferable.AnyThing, transferable.CountToTransfer, false, true);
					}
				}
			}

			if(Event.current.type is EventType.Layout)
			{
				scrollViewHeight = num + 30f;
			}
			Widgets.EndScrollView();
			Widgets.EndGroup();
			GUI.color = Color.white;
			Text.Anchor = TextAnchor.UpperLeft;
		}

		private void DrawThingRow(ref float y, float width, Thing thing, int? transferStackCount = null, bool inventory = false, bool missingFromInventory = false)
		{
			var color = GUI.color;


			if (missingFromInventory)
				GUI.color = MissingItemColor;

			Rect rect = new Rect(0f, y, width, ThingIconSize);
			Widgets.InfoCardButton(rect.width - 24f, y, thing);
			rect.width -= 24f;

			if(inventory && Vehicle.Spawned)
			{
				Rect rectDrop = new Rect(rect.width - 24f, y, 24f, 24f);
				TooltipHandler.TipRegion(rectDrop, "DropThing".Translate());
				if(Widgets.ButtonImage(rectDrop, VehicleTex.Drop))
				{
					SoundDefOf.Tick_High.PlayOneShotOnCamera(null);
					InterfaceDrop(thing);
				}
				rect.width -= 24f;
			}

			Rect rect2 = rect;
			rect2.xMin = rect2.xMax - 60f;
			CaravanThingsTabUtility.DrawMass(thing, rect2);
			rect.width -= 60f;
			GUI.color = color;
			if(Mouse.IsOver(rect))
			{
				GUI.color = HighlightColor;
				GUI.DrawTexture(rect, TexUI.HighlightTex);
			}
			if(!(thing.def.DrawMatSingle is null) && !(thing.def.DrawMatSingle.mainTexture is null))
			{
				Widgets.ThingIcon(new Rect(4f, y, ThingIconSize, ThingRowHeight), thing, 1f);
			}

			Text.Anchor = TextAnchor.MiddleLeft;
			if (!missingFromInventory)
				GUI.color = ThingLabelColor;
			else
				GUI.color = MissingItemColor;
			Rect rect3 = new Rect(ThingLeftX, y, rect.width - ThingLeftX, rect.height);
			string text = string.Empty;
			if (transferStackCount != null)
			{
				text = thing.LabelCapNoCount + " x" + transferStackCount.Value.ToStringCached();
			}
			else
			{
				text = thing.LabelCap;
			}
			Text.WordWrap = false;
			Widgets.Label(rect3, text.Truncate(rect3.width, null));
			Text.WordWrap = true;
			string text2 = thing.DescriptionDetailed;
			if(thing.def.useHitPoints)
			{
				string text3 = text2;
				text2 = string.Concat(new object[]
				{
					text3, "\n", thing.HitPoints, " / ", thing.MaxHitPoints
				});
			}
			TooltipHandler.TipRegion(rect, text2);
			y += ThingRowHeight;

			GUI.color = color;
		}
		
		private void TryDrawMassInfo(ref float curY, float width)
		{
			Rect rect = new Rect(0f, curY, width, StandardLineHeight);
			float cannonsNum = 0f;
			if (Vehicle.TryGetComp<CompVehicleTurrets>() != null)
			{
				foreach (VehicleTurret turret in Vehicle.CompVehicleTurrets.turrets)
				{
					cannonsNum += turret.loadedAmmo is null ? 0f : turret.loadedAmmo.BaseMass * turret.shellCount;
				}
			}
			float mass = MassUtility.GearAndInventoryMass(Vehicle) + cannonsNum;
			float capacity = MassUtility.Capacity(Vehicle, null);
			Widgets.Label(rect, "MassCarried".Translate(mass.ToString("0.##"), capacity.ToString("0.##")));
			curY += StandardLineHeight;
		}

		private void InterfaceDrop(Thing thing)
		{
			if (Vehicle.inventory.innerContainer.TryDrop(thing, Vehicle.Position, Vehicle.Map, ThingPlaceMode.Near, out Thing _))
			{
				Vehicle.EventRegistry[VehicleEventDefOf.CargoRemoved].ExecuteEvents();
			}
		}
	}
}
