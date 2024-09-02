using System;

namespace DocGen.Compression.Zip;

[Flags]
public enum GeneralPurposeBitFlags : short
{
	SizeAfterData = 8,
	Unicode = 0x800
}
