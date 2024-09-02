using System;
using System.ComponentModel;

namespace SkiaSharp;

public class SKRunBuffer
{
	internal readonly SKRunBufferInternal internalBuffer;

	public int Size { get; }

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete]
	public int TextSize { get; }

	internal SKRunBuffer(SKRunBufferInternal buffer, int size)
	{
		internalBuffer = buffer;
		Size = size;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete]
	internal SKRunBuffer(SKRunBufferInternal buffer, int size, int textSize)
	{
		internalBuffer = buffer;
		Size = size;
		TextSize = textSize;
	}

	public unsafe Span<ushort> GetGlyphSpan()
	{
		return new Span<ushort>(internalBuffer.glyphs, (internalBuffer.glyphs != null) ? Size : 0);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete]
	public unsafe Span<byte> GetTextSpan()
	{
		return new Span<byte>(internalBuffer.utf8text, (internalBuffer.utf8text != null) ? TextSize : 0);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete]
	public unsafe Span<uint> GetClusterSpan()
	{
		return new Span<uint>(internalBuffer.clusters, (internalBuffer.clusters != null) ? Size : 0);
	}

	public void SetGlyphs(ReadOnlySpan<ushort> glyphs)
	{
		glyphs.CopyTo(GetGlyphSpan());
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete]
	public void SetText(ReadOnlySpan<byte> text)
	{
		text.CopyTo(GetTextSpan());
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete]
	public void SetClusters(ReadOnlySpan<uint> clusters)
	{
		clusters.CopyTo(GetClusterSpan());
	}
}
