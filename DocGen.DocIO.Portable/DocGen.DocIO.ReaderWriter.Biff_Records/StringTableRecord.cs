using System;
using System.IO;

namespace DocGen.DocIO.ReaderWriter.Biff_Records;

[CLSCompliant(false)]
internal class StringTableRecord : BaseWordRecord
{
	internal StringTableRecord()
	{
	}

	internal StringTableRecord(byte[] data)
		: base(data)
	{
	}

	internal StringTableRecord(byte[] arrData, int iOffset)
		: base(arrData, iOffset)
	{
	}

	internal StringTableRecord(byte[] arrData, int iOffset, int iCount)
		: base(arrData, iOffset, iCount)
	{
	}

	internal StringTableRecord(Stream stream, int iCount)
		: base(stream, iCount)
	{
	}

	internal override void Parse(byte[] arrData, int iOffset, int iCount)
	{
		base.Parse(arrData, iOffset, iCount);
	}
}
