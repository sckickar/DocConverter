namespace DocGen.Compression.Zip;

public enum CompressionMethod
{
	Stored = 0,
	Shrunk = 1,
	ReducedFactor1 = 2,
	ReducedFactor2 = 3,
	ReducedFactor3 = 4,
	ReducedFactor4 = 5,
	Imploded = 6,
	Tokenizing = 7,
	Deflated = 8,
	Defalte64 = 9,
	PRWARE = 10,
	BZIP2 = 12,
	LZMA = 14,
	IBMTerse = 18,
	LZ77 = 19,
	PPMd = 98
}
