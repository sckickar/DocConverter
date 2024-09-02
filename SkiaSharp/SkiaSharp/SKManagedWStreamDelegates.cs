using System;

namespace SkiaSharp;

internal struct SKManagedWStreamDelegates : IEquatable<SKManagedWStreamDelegates>
{
	public SKManagedWStreamWriteProxyDelegate fWrite;

	public SKManagedWStreamFlushProxyDelegate fFlush;

	public SKManagedWStreamBytesWrittenProxyDelegate fBytesWritten;

	public SKManagedWStreamDestroyProxyDelegate fDestroy;

	public readonly bool Equals(SKManagedWStreamDelegates obj)
	{
		if (fWrite == obj.fWrite && fFlush == obj.fFlush && fBytesWritten == obj.fBytesWritten)
		{
			return fDestroy == obj.fDestroy;
		}
		return false;
	}

	public override readonly bool Equals(object obj)
	{
		if (obj is SKManagedWStreamDelegates obj2)
		{
			return Equals(obj2);
		}
		return false;
	}

	public static bool operator ==(SKManagedWStreamDelegates left, SKManagedWStreamDelegates right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(SKManagedWStreamDelegates left, SKManagedWStreamDelegates right)
	{
		return !left.Equals(right);
	}

	public override readonly int GetHashCode()
	{
		HashCode hashCode = default(HashCode);
		hashCode.Add(fWrite);
		hashCode.Add(fFlush);
		hashCode.Add(fBytesWritten);
		hashCode.Add(fDestroy);
		return hashCode.ToHashCode();
	}
}
