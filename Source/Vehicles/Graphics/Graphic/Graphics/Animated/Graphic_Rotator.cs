﻿using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Vehicles
{
	public abstract class Graphic_Rotator : Graphic_Single
	{
		public abstract string RegistryKey { get; }
	}
}
