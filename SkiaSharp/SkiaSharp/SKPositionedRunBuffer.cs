using System;
using System.ComponentModel;

namespace SkiaSharp;

public sealed class SKPositionedRunBuffer : SKRunBuffer
{
	internal SKPositionedRunBuffer(SKRunBufferInternal buffer, int count)
		: base(buffer, count)
	{
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete]
	internal SKPositionedRunBuffer(SKRunBufferInternal buffer, int count, int textSize)
		: base(buffer, count, textSize)
	{
	}

	public unsafe Span<SKPoint> GetPositionSpan()
	{
		return new Span<SKPoint>(internalBuffer.pos, (internalBuffer.pos != null) ? base.Size : 0);
	}

	public void SetPositions(ReadOnlySpan<SKPoint> positions)
	{
		positions.CopyTo(GetPositionSpan());
	}
}
