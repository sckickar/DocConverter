using System;
using System.Collections.Generic;
using System.IO;
using DocGen.Drawing;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.RangeProtection)]
[CLSCompliant(false)]
internal class RangeProtectionRecord : BiffRecordRaw
{
	private const int DEF_LENGTH_OFFSET = 19;

	private readonly byte[] DEF_FIRST_UNKNOWN_BYTES = new byte[19]
	{
		104, 8, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 3, 0, 0, 0, 0, 0, 0
	};

	private const int DEF_DATA_OFFSET = 27;

	private readonly byte[] DEF_SECOND_UNKNOWN_BYTES = new byte[6] { 4, 0, 0, 0, 0, 0 };

	private const int DEF_SUBRECORD_SIZE = 8;

	private const int DEF_FINISH_OFFSET = 4;

	public const int DEF_MAX_SUBRECORDS_SIZE = 1024;

	private ExcelIgnoreError m_ignoreOpt;

	internal MemoryStream m_preservedData;

	internal List<UnknownRecord> m_continueRecords;

	public ExcelIgnoreError IgnoreOptions
	{
		get
		{
			return m_ignoreOpt;
		}
		set
		{
			m_ignoreOpt = value;
		}
	}

	public override int MinimumRecordSize => 39;

	public RangeProtectionRecord()
	{
	}

	public RangeProtectionRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public RangeProtectionRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		int num = provider.ReadUInt16(iOffset + 19);
		int num2 = 27 + num * 8;
		if (num2 > iLength)
		{
			m_preservedData = new MemoryStream(8224);
			byte[] array = new byte[iLength];
			provider.ReadArray(iOffset, array);
			m_preservedData.Write(array, 0, array.Length);
		}
		else
		{
			m_ignoreOpt = (ExcelIgnoreError)provider.ReadUInt16(iOffset + num2);
			iOffset += 27;
			_ = m_ignoreOpt;
		}
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		if (m_preservedData != null)
		{
			m_preservedData.Position = 0L;
			provider.WriteBytes(iOffset, m_preservedData.ToArray());
		}
		else
		{
			m_iLength = GetStoreSize(version);
			provider.WriteBytes(iOffset, DEF_FIRST_UNKNOWN_BYTES, 0, DEF_FIRST_UNKNOWN_BYTES.Length);
			iOffset += 19;
		}
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		if (m_preservedData != null)
		{
			return (int)m_preservedData.Length;
		}
		OptimizeStorage();
		return 39;
	}

	private void OptimizeStorage()
	{
	}

	private IList<Rectangle> CombineSameRowRectangles(IList<Rectangle> lstRects)
	{
		if (lstRects == null || lstRects.Count == 0)
		{
			return lstRects;
		}
		List<Rectangle> list = new List<Rectangle>();
		list.Add(lstRects[0]);
		int i = 1;
		for (int count = lstRects.Count; i < count; i++)
		{
			int index = list.Count - 1;
			Rectangle rectangle = list[index];
			Rectangle item = lstRects[i];
			if (rectangle.Top == item.Top && rectangle.Bottom == item.Bottom && rectangle.Right + 1 == item.Left)
			{
				rectangle = Rectangle.FromLTRB(rectangle.Left, rectangle.Top, item.Right, rectangle.Bottom);
				list[index] = rectangle;
			}
			else
			{
				list.Add(item);
			}
		}
		return list;
	}
}
