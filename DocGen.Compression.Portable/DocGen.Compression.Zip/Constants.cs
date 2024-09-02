using System;

namespace DocGen.Compression.Zip;

[CLSCompliant(false)]
public sealed class Constants
{
	public const int HeaderSignature = 67324752;

	public const int HeaderSignatureBytes = 4;

	public const int BufferSize = 4096;

	public const short VersionNeededToExtract = 20;

	public const short VersionMadeBy = 45;

	public const int ShortSize = 2;

	public const int IntSize = 4;

	public const int CentralHeaderSignature = 33639248;

	public const int CentralDirectoryEndSignature = 101010256;

	public const uint StartCrc = uint.MaxValue;

	public const int CentralDirSizeOffset = 12;

	public const int HeaderSignatureStartByteValue = 80;

	private Constants()
	{
	}
}
