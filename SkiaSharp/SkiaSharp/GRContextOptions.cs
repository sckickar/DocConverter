using System;

namespace SkiaSharp;

public class GRContextOptions
{
	public bool AvoidStencilBuffers { get; set; }

	public int RuntimeProgramCacheSize { get; set; } = 256;


	public int GlyphCacheTextureMaximumBytes { get; set; } = 8388608;


	public bool AllowPathMaskCaching { get; set; } = true;


	public bool DoManualMipmapping { get; set; }

	public int BufferMapThreshold { get; set; } = -1;


	internal GRContextOptionsNative ToNative()
	{
		GRContextOptionsNative result = default(GRContextOptionsNative);
		result.fAllowPathMaskCaching = (AllowPathMaskCaching ? ((byte)1) : ((byte)0));
		result.fAvoidStencilBuffers = (AvoidStencilBuffers ? ((byte)1) : ((byte)0));
		result.fBufferMapThreshold = BufferMapThreshold;
		result.fDoManualMipmapping = (DoManualMipmapping ? ((byte)1) : ((byte)0));
		result.fGlyphCacheTextureMaximumBytes = (IntPtr)GlyphCacheTextureMaximumBytes;
		result.fRuntimeProgramCacheSize = RuntimeProgramCacheSize;
		return result;
	}
}
