using System;
using System.ComponentModel;

namespace SkiaSharp;

public sealed class SKRotationScaleRunBuffer : SKRunBuffer
{
	internal SKRotationScaleRunBuffer(SKRunBufferInternal buffer, int count)
		: base(buffer, count)
	{
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete]
	internal SKRotationScaleRunBuffer(SKRunBufferInternal buffer, int count, int textSize)
		: base(buffer, count, textSize)
	{
	}

	public unsafe Span<SKRotationScaleMatrix> GetRotationScaleSpan()
	{
		return new Span<SKRotationScaleMatrix>(internalBuffer.pos, base.Size);
	}

	public void SetRotationScale(ReadOnlySpan<SKRotationScaleMatrix> positions)
	{
		positions.CopyTo(GetRotationScaleSpan());
	}
}
