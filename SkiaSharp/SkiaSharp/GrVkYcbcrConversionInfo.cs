using System;

namespace SkiaSharp;

public struct GrVkYcbcrConversionInfo : IEquatable<GrVkYcbcrConversionInfo>
{
	private uint fFormat;

	private ulong fExternalFormat;

	private uint fYcbcrModel;

	private uint fYcbcrRange;

	private uint fXChromaOffset;

	private uint fYChromaOffset;

	private uint fChromaFilter;

	private uint fForceExplicitReconstruction;

	private uint fFormatFeatures;

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

	public ulong ExternalFormat
	{
		readonly get
		{
			return fExternalFormat;
		}
		set
		{
			fExternalFormat = value;
		}
	}

	public uint YcbcrModel
	{
		readonly get
		{
			return fYcbcrModel;
		}
		set
		{
			fYcbcrModel = value;
		}
	}

	public uint YcbcrRange
	{
		readonly get
		{
			return fYcbcrRange;
		}
		set
		{
			fYcbcrRange = value;
		}
	}

	public uint XChromaOffset
	{
		readonly get
		{
			return fXChromaOffset;
		}
		set
		{
			fXChromaOffset = value;
		}
	}

	public uint YChromaOffset
	{
		readonly get
		{
			return fYChromaOffset;
		}
		set
		{
			fYChromaOffset = value;
		}
	}

	public uint ChromaFilter
	{
		readonly get
		{
			return fChromaFilter;
		}
		set
		{
			fChromaFilter = value;
		}
	}

	public uint ForceExplicitReconstruction
	{
		readonly get
		{
			return fForceExplicitReconstruction;
		}
		set
		{
			fForceExplicitReconstruction = value;
		}
	}

	public uint FormatFeatures
	{
		readonly get
		{
			return fFormatFeatures;
		}
		set
		{
			fFormatFeatures = value;
		}
	}

	public readonly bool Equals(GrVkYcbcrConversionInfo obj)
	{
		if (fFormat == obj.fFormat && fExternalFormat == obj.fExternalFormat && fYcbcrModel == obj.fYcbcrModel && fYcbcrRange == obj.fYcbcrRange && fXChromaOffset == obj.fXChromaOffset && fYChromaOffset == obj.fYChromaOffset && fChromaFilter == obj.fChromaFilter && fForceExplicitReconstruction == obj.fForceExplicitReconstruction)
		{
			return fFormatFeatures == obj.fFormatFeatures;
		}
		return false;
	}

	public override readonly bool Equals(object obj)
	{
		if (obj is GrVkYcbcrConversionInfo obj2)
		{
			return Equals(obj2);
		}
		return false;
	}

	public static bool operator ==(GrVkYcbcrConversionInfo left, GrVkYcbcrConversionInfo right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(GrVkYcbcrConversionInfo left, GrVkYcbcrConversionInfo right)
	{
		return !left.Equals(right);
	}

	public override readonly int GetHashCode()
	{
		HashCode hashCode = default(HashCode);
		hashCode.Add(fFormat);
		hashCode.Add(fExternalFormat);
		hashCode.Add(fYcbcrModel);
		hashCode.Add(fYcbcrRange);
		hashCode.Add(fXChromaOffset);
		hashCode.Add(fYChromaOffset);
		hashCode.Add(fChromaFilter);
		hashCode.Add(fForceExplicitReconstruction);
		hashCode.Add(fFormatFeatures);
		return hashCode.ToHashCode();
	}
}
