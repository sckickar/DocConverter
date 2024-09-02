using System;

namespace DocGen.DocIO.ReaderWriter.Biff_Records.Structures;

[CLSCompliant(false)]
internal class ParagraphHeightStructure : IDataStructure
{
	private const int DEF_RECORD_SIZE = 12;

	internal uint Options;

	internal int Width;

	internal int Height;

	public int Length => 12;

	public void Parse(byte[] arrData, int iOffset)
	{
		Options = ByteConverter.ReadUInt32(arrData, ref iOffset);
		Width = ByteConverter.ReadInt32(arrData, ref iOffset);
		Height = ByteConverter.ReadInt32(arrData, ref iOffset);
	}

	public int Save(byte[] arrData, int iOffset)
	{
		ByteConverter.WriteUInt32(arrData, ref iOffset, Options);
		ByteConverter.WriteInt32(arrData, ref iOffset, Width);
		ByteConverter.WriteInt32(arrData, ref iOffset, Height);
		return 12;
	}
}
