using System;

namespace SkiaSharp;

public struct GRVkImageInfo : IEquatable<GRVkImageInfo>
{
	private ulong fImage;

	private GRVkAlloc fAlloc;

	private uint fImageTiling;

	private uint fImageLayout;

	private uint fFormat;

	private uint fImageUsageFlags;

	private uint fSampleCount;

	private uint fLevelCount;

	private uint fCurrentQueueFamily;

	private byte fProtected;

	private GrVkYcbcrConversionInfo fYcbcrConversionInfo;

	private uint fSharingMode;

	public ulong Image
	{
		readonly get
		{
			return fImage;
		}
		set
		{
			fImage = value;
		}
	}

	public GRVkAlloc Alloc
	{
		readonly get
		{
			return fAlloc;
		}
		set
		{
			fAlloc = value;
		}
	}

	public uint ImageTiling
	{
		readonly get
		{
			return fImageTiling;
		}
		set
		{
			fImageTiling = value;
		}
	}

	public uint ImageLayout
	{
		readonly get
		{
			return fImageLayout;
		}
		set
		{
			fImageLayout = value;
		}
	}

	public uint Format
	{
		readonly get
		{
			return fFormat;
		}
		set
		{
			fFormat = value;
		}
	}

	public uint ImageUsageFlags
	{
		readonly get
		{
			return fImageUsageFlags;
		}
		set
		{
			fImageUsageFlags = value;
		}
	}

	public uint SampleCount
	{
		readonly get
		{
			return fSampleCount;
		}
		set
		{
			fSampleCount = value;
		}
	}

	public uint LevelCount
	{
		readonly get
		{
			return fLevelCount;
		}
		set
		{
			fLevelCount = value;
		}
	}

	public uint CurrentQueueFamily
	{
		readonly get
		{
			return fCurrentQueueFamily;
		}
		set
		{
			fCurrentQueueFamily = value;
		}
	}

	public bool Protected
	{
		readonly get
		{
			return fProtected > 0;
		}
		set
		{
			fProtected = (value ? ((byte)1) : ((byte)0));
		}
	}

	public GrVkYcbcrConversionInfo YcbcrConversionInfo
	{
		readonly get
		{
			return fYcbcrConversionInfo;
		}
		set
		{
			fYcbcrConversionInfo = value;
		}
	}

	public uint SharingMode
	{
		readonly get
		{
			return fSharingMode;
		}
		set
		{
			fSharingMode = value;
		}
	}

	public readonly bool Equals(GRVkImageInfo obj)
	{
		if (fImage == obj.fImage && fAlloc == obj.fAlloc && fImageTiling == obj.fImageTiling && fImageLayout == obj.fImageLayout && fFormat == obj.fFormat && fImageUsageFlags == obj.fImageUsageFlags && fSampleCount == obj.fSampleCount && fLevelCount == obj.fLevelCount && fCurrentQueueFamily == obj.fCurrentQueueFamily && fProtected == obj.fProtected && fYcbcrConversionInfo == obj.fYcbcrConversionInfo)
		{
			return fSharingMode == obj.fSharingMode;
		}
		return false;
	}

	public override readonly bool Equals(object obj)
	{
		if (obj is GRVkImageInfo obj2)
		{
			return Equals(obj2);
		}
		return false;
	}

	public static bool operator ==(GRVkImageInfo left, GRVkImageInfo right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(GRVkImageInfo left, GRVkImageInfo right)
	{
		return !left.Equals(right);
	}

	public override readonly int GetHashCode()
	{
		HashCode hashCode = default(HashCode);
		hashCode.Add(fImage);
		hashCode.Add(fAlloc);
		hashCode.Add(fImageTiling);
		hashCode.Add(fImageLayout);
		hashCode.Add(fFormat);
		hashCode.Add(fImageUsageFlags);
		hashCode.Add(fSampleCount);
		hashCode.Add(fLevelCount);
		hashCode.Add(fCurrentQueueFamily);
		hashCode.Add(fProtected);
		hashCode.Add(fYcbcrConversionInfo);
		hashCode.Add(fSharingMode);
		return hashCode.ToHashCode();
	}
}
