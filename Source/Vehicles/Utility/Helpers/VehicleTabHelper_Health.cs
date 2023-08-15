﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;
using RimWorld;
using SmashTools;

namespace Vehicles
{
	public static class VehicleTabHelper_Health
	{
		public const float ComponentRowHeight = 20f;
		public const float ComponentIndicatorIconSize = 20f;

		private static readonly Color MouseOverColor = new Color(0.85f, 0.85f, 0.85f, 0.1f);
		private static readonly Color AlternatingColor = new Color(0.75f, 0.75f, 0.75f, 0.1f);

		private static float componentListHeight;
		private static VehiclePawn inspectingVehicle;
		private static Vector2 size;

		private static ITab_Vehicle_Health.VehicleHealthTab onTab;
		private static Vector2 componentTabScrollPos;
		private static VehicleComponent selectedComponent;

		private static readonly List<JobDef> jobLimitJobDefs = new List<JobDef>();

		public static void Init()
		{
			componentTabScrollPos = Vector2.zero;
			selectedComponent = null;
			//jobLimitJobDefs.Clear();
			//foreach (JobDef jobDef in DefDatabase<JobDef>.AllDefsListForReading)
			//{
			//	if (jobDef.driverClass.IsSubclassOf(typeof(VehicleJobDriver)))
			//	{
			//		jobLimitJobDefs.Add(jobDef);
			//	}
			//}
		}

		public static void Start(Vector2 size, VehiclePawn vehicle)
		{
			if (vehicle != inspectingVehicle)
			{
				//Not captured by OnOpen when switching between vehicles with ITab already open
				inspectingVehicle = vehicle;
				VehicleTabHelper_Health.size = size;
				RecacheComponentListHeight();
			}
		}

		public static void End()
		{
		}

		public static void DrawHealthPanel(VehiclePawn vehicle)
		{
			GUIState.Push();
			{
				Rect rect = new Rect(0, 20, size.x, size.y - 20);

				Rect infoPanelRect = new Rect(rect.x, rect.y, ITab_Vehicle_Health.InfoPanelWidth, rect.height).Rounded();
				Rect componentPanelRect = new Rect(infoPanelRect.xMax, rect.y, size.x - ITab_Vehicle_Health.InfoPanelWidth, rect.height);
				
				infoPanelRect.yMin += 11f; //Extra space for tab, excluded from componentPanelRect for top options

				DrawHealthInfo(infoPanelRect, vehicle);
				GUIState.Reset();
				DrawComponentsInfo(componentPanelRect, vehicle);
			}
			GUIState.Pop();
		}

		private static void DrawHealthInfo(Rect rect, VehiclePawn vehicle)
		{
			Widgets.DrawMenuSection(rect);
			List<TabRecord> list = new List<TabRecord>();
			list.Add(new TabRecord("HealthOverview".Translate(), delegate ()
			{
				onTab = ITab_Vehicle_Health.VehicleHealthTab.Overview;
			}, onTab == ITab_Vehicle_Health.VehicleHealthTab.Overview));
			//list.Add(new TabRecord("VF_JobSettings".Translate(), delegate ()
			//{
			//	onTab = ITab_Vehicle_Health.VehicleHealthTab.JobSettings;
			//}, onTab == ITab_Vehicle_Health.VehicleHealthTab.JobSettings));
			TabDrawer.DrawTabs(rect, list);

			rect = rect.ContractedBy(9f);
			Widgets.BeginGroup(rect);
			{
				GUIState.Push();
				{
					Text.Font = GameFont.Small;
					GUI.color = Color.white;
					Text.Anchor = TextAnchor.UpperLeft;

					switch (onTab)
					{
						case ITab_Vehicle_Health.VehicleHealthTab.Overview:
							DrawVehicleInformation(rect, vehicle);
							break;
						case ITab_Vehicle_Health.VehicleHealthTab.JobSettings:
							DrawJobSettings(rect, vehicle);
							break;
					}
				}
				GUIState.Pop();
			}
			Widgets.EndGroup();
		}

		private static void DrawJobSettings(Rect leftRect, VehiclePawn vehicle)
		{
			throw new NotImplementedException("Job Settings");
			GUIState.Push();
			{
				float curY = 0;
				Rect rect = new Rect(0f, curY, leftRect.width, 34f);

				rect.SplitVertically(rect.width / 2, out Rect _, out Rect buttonRect);

				if (Widgets.ButtonText(buttonRect, "ResetButton".Translate()))
				{
					//vehicle.jobLimitations.Clear();
				}

				foreach (JobDef jobDef in jobLimitJobDefs)
				{
					int maxWorkers = 1;
					
					curY += 34;
				}
			}
			GUIState.Pop();
		}

		private static void DrawVehicleInformation(Rect leftRect, VehiclePawn vehicle)
		{
			GUIState.Push();
			{
				float curY = 0;
				Rect rect = new Rect(0f, curY, leftRect.width, 34f);

				Text.Anchor = TextAnchor.UpperCenter;
				Widgets.Label(rect, vehicle.LabelCap);
				if (Mouse.IsOver(rect))
				{
					string dateReadout = $"{Find.ActiveLanguageWorker.OrdinalNumber(vehicle.ageTracker.BirthDayOfSeasonZeroBased + 1, Gender.None)} {vehicle.ageTracker.BirthQuadrum.Label()}, {vehicle.ageTracker.BirthYear}";
					(GenTicks.TicksAbs - vehicle.ageTracker.BirthAbsTicks).TicksToPeriod(out int years, out int quadrums, out int days, out float hours);
					string chronologicalReadout = "AgeChronological".Translate(years, quadrums, days);
					
					TooltipHandler.TipRegion(rect, () => $"{"VF_VehicleAgeReadout".Translate(dateReadout)}\n{chronologicalReadout}", "HealthTab".GetHashCode());
					Widgets.DrawHighlight(rect);
				}

				GUIState.Reset();
				
				curY += 34;

				Rect statRect = new Rect(0, curY, leftRect.width, 34);
				foreach (VehicleStatDef statDef in vehicle.VehicleDef.StatCategoryDefs().Distinct())
				{
					curY = statDef.Worker.DrawVehicleStat(statRect, curY, vehicle);
					statRect.y = curY;
				}
			}
			GUIState.Pop();
		}

		/// <summary>
		/// Draw component list with health, efficiency, and armor values
		/// </summary>
		/// <param name="rect"></param>
		/// <param name="vehicle"></param>
		/// <param name="componentViewHeight">Cached height of full component list, taking into account extra space of longer labels</param>
		private static void DrawComponentsInfo(Rect rect, VehiclePawn vehicle)
		{
			Text.Font = GameFont.Small;
			float textHeight = Text.CalcSize("VF_ComponentHealth".Translate()).y;
			float columnWidth = 75 - (ComponentIndicatorIconSize / 3f);
			float labelWidth = rect.width - (columnWidth * 3) - ComponentIndicatorIconSize * 2;
			//Skip header for component name column
			Rect topLabelRect = new Rect(rect.x + labelWidth, rect.y, columnWidth, textHeight);

			Text.Anchor = TextAnchor.MiddleCenter;
			Widgets.Label(topLabelRect, "VF_ComponentHealth".Translate());
			topLabelRect.x += topLabelRect.width;
			Widgets.Label(topLabelRect, "VF_ComponentEfficiency".Translate());
			topLabelRect.x += topLabelRect.width;
			Widgets.Label(topLabelRect, "VF_ComponentArmor".Translate());
			topLabelRect.x += topLabelRect.width;

			GUI.color = TexData.MenuBGColor;
			Widgets.DrawLineHorizontal(rect.x, topLabelRect.y + textHeight / 1.25f, rect.width);
			GUI.color = Color.white;

			rect.y += textHeight / 1.25f + 1; //+1 for H. line
			rect.x += 2.5f;
			rect.width -= 5;

			Rect scrollView = new Rect(rect.x, rect.y + topLabelRect.height * 2, rect.width, componentListHeight);
			bool alternatingRow = false;
			Widgets.BeginScrollView(rect, ref componentTabScrollPos, scrollView);
			{
				float curY = scrollView.y;
				bool highlighted = false;
				foreach (VehicleComponent component in vehicle.statHandler.components)
				{
					Rect compRect = new Rect(rect.x, curY, rect.width - 16, ComponentRowHeight);
					float usedHeight = DrawCompRow(compRect, component, labelWidth, columnWidth, alternatingRow);
					//TooltipHandler.TipRegion(compRect, "VF_ComponentClickMoreInfoTooltip".Translate());
					Rect highlightingRect = new Rect(compRect)
					{
						height = usedHeight
					};
					if (Mouse.IsOver(highlightingRect))
					{
						Widgets.DrawBoxSolid(highlightingRect, MouseOverColor);
						//For debug drawing of component hitbox
						vehicle.HighlightedComponent = component;
						highlighted = true;
					}
					else if (selectedComponent == component)
					{
						Widgets.DrawBoxSolid(highlightingRect, MouseOverColor);
						highlighted = true;
					}
					if (Widgets.ButtonInvisible(compRect))
					{
						SoundDefOf.Click.PlayOneShotOnCamera(null);
						if (selectedComponent != component)
						{
							selectedComponent = component;
						}
						else
						{
							selectedComponent = null;
						}
					}
					curY += usedHeight;
					alternatingRow = !alternatingRow;
				}
				if (!highlighted)
				{
					vehicle.HighlightedComponent = null;
				}
			}
			Widgets.EndScrollView();
		}

		private static float DrawCompRow(Rect rect, VehicleComponent component, float labelWidth, float columnWidth, bool highlighted)
		{
			float textHeight = Text.CalcHeight(component.props.label, labelWidth);
			float labelHeight = Mathf.Max(rect.height, textHeight);
			Rect labelRect = new Rect(rect.x, rect.y, labelWidth, labelHeight);

			if (highlighted)
			{
				//+16 for full coverage even if scrollbar is hidden
				Widgets.DrawBoxSolid(new Rect(rect.x, rect.y, rect.width + 16, labelHeight), AlternatingColor);
			}

			Text.Anchor = TextAnchor.MiddleLeft;
			Widgets.Label(labelRect, component.props.label);
			labelRect.x += labelRect.width;

			labelRect.width = columnWidth;
			Text.Anchor = TextAnchor.MiddleCenter;
			Widgets.Label(labelRect, component.HealthPercent.ToStringPercent().Colorize(component.ComponentEfficiencyColor()));
			labelRect.x += columnWidth;
			string efficiencyEntry = component.props.categories.NullOrEmpty() ? "-" : component.Efficiency.ToStringPercent().Colorize(component.ComponentEfficiencyColor());
			Widgets.Label(labelRect, efficiencyEntry);
			labelRect.x += columnWidth;
			Widgets.Label(labelRect, component.ArmorRating(null).ToStringPercent());
			labelRect.x += columnWidth;

			Rect iconRect = new Rect(labelRect.x, labelRect.y, ComponentIndicatorIconSize, ComponentIndicatorIconSize);
			component.DrawIcon(iconRect);

			return labelHeight;
		}

		private static void RecacheComponentListHeight(float lineHeight = ComponentRowHeight)
		{
			componentListHeight = 0;
			foreach (VehicleComponent component in inspectingVehicle.statHandler.components)
			{
				float textHeight = Text.CalcHeight(component.props.label, size.x - ITab_Vehicle_Health.InfoPanelWidth);
				componentListHeight += Mathf.Max(lineHeight, textHeight);
			}
		}

		public static Color ComponentEfficiencyColor(this VehicleComponent component)
		{
			float efficiency = component.Efficiency;

			if (efficiency <= 0)
			{
				return Color.gray;
			}
			else if (efficiency < 0.4f)
			{
				return HealthUtility.RedColor;
			}
			else if (efficiency < 0.7f)
			{
				return HealthUtility.ImpairedColor;
			}
			else if (efficiency < 0.999f)
			{
				return HealthUtility.SlightlyImpairedColor;
			}
			return HealthUtility.GoodConditionColor;
		}
	}
}
