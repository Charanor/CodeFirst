﻿using JXS.Ecs.Core;

namespace JXS.Ecs.Examples._03_GeneratingCode;

internal struct HelloWorldComponent : IComponent
{
	public int WorldCount { get; set; }
}