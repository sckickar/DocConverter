using System;
using System.Collections.Generic;
using System.IO;
using DocGen.Drawing;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.CondFMT)]
[CLSCompliant(false)]
internal class CondFMTRecord : BiffRecordRaw, ICloneable
{
	private const ushort DEF_MINIMUM_RECORD_SIZE = 14;

	private const int DEF_FIXED_SIZE = 14;

	private const int DEF_SUB_ITEM_SIZE = 8;

	private const ushort DEF_REDRAW_ON = 1;

	private const ushort DEF_REDRAW_OFF = 0;

	[BiffRecordPos(0, 2)]
	private ushort m_usCFNumber;

	[BiffRecordPos(2, 0, TFieldType.Bit)]
	private bool m_usNeedRecalc;

	private ushort m_index;

	private TAddr m_addrEncloseRange;

	private ushort m_usCellsCount;

	private List<Rectangle> m_arrCells = new List<Rectangle>();

	private bool m_isparsed;

	public ushort CFNumber
	{
		get
		{
			return m_usCFNumber;
		}
		set
		{
			m_usCFNumber = value;
		}
	}

	public bool NeedRecalc
	{
		get
		{
			return !m_usNeedRecalc;
		}
		set
		{
			m_usNeedRecalc = value;
		}
	}

	public ushort Index
	{
		get
		{
			return m_index;
		}
		set
		{
			m_index = value;
		}
	}

	public TAddr EncloseRange
	{
		get
		{
			return m_addrEncloseRange;
		}
		set
		{
			m_addrEncloseRange = value;
		}
	}

	public ushort CellsCount
	{
		get
		{
			return m_usCellsCount;
		}
		set
		{
			m_usCellsCount = (ushort)m_arrCells.Count;
		}
	}

	public List<Rectangle> CellList
	{
		get
		{
			return m_arrCells;
		}
		internal set
		{
			m_arrCells = value;
		}
	}

	public override int MinimumRecordSize => 14;

	public bool IsParsed
	{
		get
		{
			return m_isparsed;
		}
		set
		{
			m_isparsed = value;
		}
	}

	public CondFMTRecord()
	{
	}

	public CondFMTRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public CondFMTRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_isparsed = true;
		m_usCFNumber = provider.ReadUInt16(iOffset);
		iOffset += 2;
		ushort num = provider.ReadUInt16(iOffset);
		m_usNeedRecalc = (num & 1) == 1;
		m_index = (ushort)(num >> 1);
		iOffset += 2;
		m_addrEncloseRange = provider.ReadAddr(iOffset);
		iOffset += 8;
		ExtractCellsList(provider, ref iOffset);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_usCellsCount = (ushort)m_arrCells.Count;
		m_iLength = GetStoreSize(version);
		provider.WriteUInt16(iOffset, m_usCFNumber);
		iOffset += 2;
		int num = (m_usNeedRecalc ? ((m_index << 1) | 1) : ((m_index << 1) | 0));
		provider.WriteUInt16(iOffset, (ushort)num);
		iOffset += 2;
		provider.WriteAddr(iOffset, m_addrEncloseRange);
		iOffset += 8;
		provider.WriteUInt16(iOffset, m_usCellsCount);
		iOffset += 2;
		int num2 = 0;
		while (num2 < m_usCellsCount)
		{
			Rectangle addr = m_arrCells[num2];
			provider.WriteAddr(iOffset, addr);
			num2++;
			iOffset += 8;
		}
	}

	private void ExtractCellsList(DataProvider provider, ref int offset)
	{
		m_usCellsCount = provider.ReadUInt16(offset);
		offset += 2;
		int num = 0;
		while (num < m_usCellsCount)
		{
			Rectangle item = provider.ReadAddrAsRectangle(offset);
			m_arrCells.Add(item);
			num++;
			offset += 8;
		}
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 14 + m_arrCells.Count * 8;
	}

	public void AddCell(Rectangle addr)
	{
		m_arrCells.Add(addr);
		m_usCellsCount++;
	}

	public override object Clone()
	{
		CondFMTRecord condFMTRecord = (CondFMTRecord)base.Clone();
		condFMTRecord.m_arrCells = new List<Rectangle>(m_arrCells.Count);
		int i = 0;
		for (int count = m_arrCells.Count; i < count; i++)
		{
			condFMTRecord.AddCell(m_arrCells[i]);
		}
		return condFMTRecord;
	}
}
