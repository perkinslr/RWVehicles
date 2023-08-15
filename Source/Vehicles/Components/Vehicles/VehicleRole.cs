﻿using System.Collections.Generic;
using Verse;
using SmashTools;

namespace Vehicles
{
	public class VehicleRole
	{
		public string key;
		public string label = "[MissingLabel]";

		//Operating
		public HandlingTypeFlags handlingTypes = HandlingTypeFlags.None;
		public int slots;
		public int slotsToOperate;
		public List<string> turretIds;
		//Damaging
		public ComponentHitbox hitbox = new ComponentHitbox();
		public bool exposed = false;
		public float chanceToHit = 0.3f;
		//Rendering
		public PawnOverlayRenderer pawnRenderer;

		public VehicleRole()
		{
		}

		public VehicleRole(VehicleHandler group) : this(group.role)
		{
		}

		public VehicleRole(VehicleRole reference)
		{
			if (string.IsNullOrEmpty(reference.key))
			{
				Log.Error($"Missing Key on VehicleRole {reference.label}");
			}
			key = reference.key;
			label = reference.label;
			handlingTypes = reference.handlingTypes;
			slots = reference.slots;
			slotsToOperate = reference.slotsToOperate;
			if (!reference.turretIds.NullOrEmpty())
			{
				turretIds = new List<string>(reference.turretIds);
			}
			hitbox = reference.hitbox;
			pawnRenderer = reference.pawnRenderer;
		}

		public bool RequiredForCaravan => slotsToOperate > 0 && handlingTypes.HasFlag(HandlingTypeFlags.Movement);
	}
}
