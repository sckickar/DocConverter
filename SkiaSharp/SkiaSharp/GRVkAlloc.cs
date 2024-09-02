using System;

namespace SkiaSharp;

public struct GRVkAlloc : IEquatable<GRVkAlloc>
{
	private ulong fMemory;

	private ulong fOffset;

	private ulong fSize;

	private uint fFlags;

	private IntPtr fBackendMemory;

	private byte fUsesSystemHeap;

	public ulong Memory
	{
		readonly get
		{
			return fMemory;
		}
		set
		{
			fMemory = value;
		}
	}

	public ulong Offset
	{
		readonly get
		{
			return fOffset;
		}
		set
		{
			fOffset = value;
		}
	}

	public ulong Size
	{
		readonly get
		{
			return fSize;
		}
		set
		{
			fSize = value;
		}
	}

	public uint Flags
	{
		readonly get
		{
			return fFlags;
		}
		set
		{
			fFlags = value;
		}
	}

	public IntPtr BackendMemory
	{
		readonly get
		{
			return fBackendMemory;
		}
		set
		{
			fBackendMemory = value;
		}
	}

	public readonly bool Equals(GRVkAlloc obj)
	{
		if (fMemory == obj.fMemory && fOffset == obj.fOffset && fSize == obj.fSize && fFlags == obj.fFlags && fBackendMemory == obj.fBackendMemory)
		{
			return fUsesSystemHeap == obj.fUsesSystemHeap;
		}
		return false;
	}

	public override readonly bool Equals(object obj)
	{
		if (obj is GRVkAlloc obj2)
		{
			return Equals(obj2);
		}
		return false;
	}

	public static bool operator ==(GRVkAlloc left, GRVkAlloc right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(GRVkAlloc left, GRVkAlloc right)
	{
		return !left.Equals(right);
	}

	public override readonly int GetHashCode()
	{
		HashCode hashCode = default(HashCode);
		hashCode.Add(fMemory);
		hashCode.Add(fOffset);
		hashCode.Add(fSize);
		hashCode.Add(fFlags);
		hashCode.Add(fBackendMemory);
		hashCode.Add(fUsesSystemHeap);
		return hashCode.ToHashCode();
	}
}
