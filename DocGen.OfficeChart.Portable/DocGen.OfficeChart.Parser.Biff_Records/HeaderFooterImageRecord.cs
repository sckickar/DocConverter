using System;
using System.Collections.Generic;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.HeaderFooterImage)]
[CLSCompliant(false)]
internal class HeaderFooterImageRecord : MSODrawingGroupRecord, ILengthSetter
{
	internal static readonly byte[] DEF_RECORD_START = new byte[14]
	{
		102, 8, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 2, 0
	};

	internal static readonly byte[] DEF_WORKSHEET_RECORD_START = new byte[14]
	{
		102, 8, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 1, 0
	};

	internal static readonly byte[] DEF_CONTINUE_START = new byte[14]
	{
		102, 8, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 6, 0
	};

	internal static readonly int DEF_DATA_OFFSET = DEF_RECORD_START.Length;

	protected override int StructuresOffset => DEF_DATA_OFFSET;

	public HeaderFooterImageRecord()
	{
	}

	public HeaderFooterImageRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public HeaderFooterImageRecord(int iReserve)
		: base(iReserve)
	{
	}

	protected override Stream CreateDataList(out int iStartIndex)
	{
		_ = m_arrStructures.Count;
		iStartIndex = 1;
		MemoryStream memoryStream = new MemoryStream();
		memoryStream.Write(DEF_RECORD_START, 0, DEF_RECORD_START.Length);
		return memoryStream;
	}

	protected override int AddRecordData(List<byte[]> arrRecords, BiffRecordRaw record)
	{
		if (arrRecords == null)
		{
			throw new ArgumentNullException("arrRecords");
		}
		if (record == null)
		{
			throw new ArgumentNullException("record");
		}
		byte[] array = record.Data;
		int num = array.Length;
		int num2 = num - DEF_DATA_OFFSET;
		if (num2 > 0)
		{
			byte[] array2 = new byte[num2];
			num = num2;
			Buffer.BlockCopy(array, DEF_DATA_OFFSET, array2, 0, num2);
			array = array2;
		}
		arrRecords.Add(array);
		return num;
	}

	protected override ContinueRecordBuilder CreateBuilder()
	{
		HeaderContinueRecordBuilder headerContinueRecordBuilder = new HeaderContinueRecordBuilder(this);
		headerContinueRecordBuilder.OnFirstContinue += builder_OnFirstContinue;
		return headerContinueRecordBuilder;
	}

	public void SetLength(int iLength)
	{
		m_iLength = iLength;
	}
}
