using System;

namespace SkiaSharp;

[Flags]
public enum GRGlBackendState : uint
{
	None = 0u,
	RenderTarget = 1u,
	TextureBinding = 2u,
	View = 4u,
	Blend = 8u,
	MSAAEnable = 0x10u,
	Vertex = 0x20u,
	Stencil = 0x40u,
	PixelStore = 0x80u,
	Program = 0x100u,
	FixedFunction = 0x200u,
	Misc = 0x400u,
	PathRendering = 0x800u,
	All = 0xFFFFu
}
