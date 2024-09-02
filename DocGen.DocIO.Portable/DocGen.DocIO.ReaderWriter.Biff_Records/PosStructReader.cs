using System.IO;

namespace DocGen.DocIO.ReaderWriter.Biff_Records;

internal class PosStructReader
{
	internal PosStructReader()
	{
	}

	internal static void Read(BinaryReader binaryReader, int structCount, PosStructReaderDelegate deleg)
	{
		int num = structCount + 1;
		int[] array = new int[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = binaryReader.ReadInt32();
		}
		for (int j = 0; j < structCount; j++)
		{
			deleg(binaryReader, array[j], array[j + 1]);
		}
	}

	internal static void Read(BinaryReader reader, int dataLength, int structLength, PosStructReaderDelegate deleg)
	{
		if (dataLength != 0)
		{
			int structCount = (dataLength - 4) / (4 + structLength);
			Read(reader, structCount, deleg);
		}
	}
}
