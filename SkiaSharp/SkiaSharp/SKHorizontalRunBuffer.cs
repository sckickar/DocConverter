using System;
using System.ComponentModel;

namespace SkiaSharp;

public sealed class SKHorizontalRunBuffer : SKRunBuffer
{
	internal SKHorizontalRunBuffer(SKRunBufferInternal buffer, int count)
		: base(buffer, count)
	{
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete]
	internal SKHorizontalRunBuffer(SKRunBufferInternal buffer, int count, int textSize)
		: base(buffer, count, textSize)
	{
	}

	public unsafe Span<float> GetPositionSpan()
	{
		return new Span<float>(internalBuffer.pos, (internalBuffer.pos != null) ? base.Size : 0);
	}

	public void SetPositions(ReadOnlySpan<float> positions)
	{
		positions.CopyTo(GetPositionSpan());
	}
}
