using System;

namespace DocGen.DocIO.ReaderWriter.Biff_Records.Structures;

[CLSCompliant(false)]
internal class BreakDescriptorStructure : IDataStructure
{
	private const int DEF_RECORD_SIZE = 6;

	internal short ipgd;

	internal short itxbxs;

	internal short dcpDepend;

	internal byte iCol;

	internal byte Options;

	public int Length => 6;

	public void Parse(byte[] arrData, int iOffset)
	{
		itxbxs = ByteConverter.ReadInt16(arrData, ref iOffset);
		dcpDepend = ByteConverter.ReadInt16(arrData, ref iOffset);
		iCol = arrData[iOffset];
		iOffset++;
		Options = arrData[iOffset];
		iOffset++;
	}

	public int Save(byte[] arr, int iOffset)
	{
		ByteConverter.WriteInt16(arr, ref iOffset, itxbxs);
		ByteConverter.WriteInt16(arr, ref iOffset, dcpDepend);
		arr[iOffset] = iCol;
		iOffset++;
		arr[iOffset] = Options;
		iOffset++;
		return 6;
	}
}
