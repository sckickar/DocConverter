using System;
using System.Collections.Generic;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records.MsoDrawing;

[MsoDrawing(MsoRecords.msofbtAnchor)]
[CLSCompliant(false)]
internal class MsofbtAnchor : MsoBase
{
	private const int DEF_RECORD_SIZE = 16;

	[BiffRecordPos(0, 4, true)]
	private int m_iLeft;

	[BiffRecordPos(4, 4, true)]
	private int m_iTop;

	[BiffRecordPos(8, 4, true)]
	private int m_iRight;

	[BiffRecordPos(12, 4, true)]
	private int m_iBottom;

	public int Left
	{
		get
		{
			return m_iLeft;
		}
		set
		{
			m_iLeft = value;
		}
	}

	public int Top
	{
		get
		{
			return m_iTop;
		}
		set
		{
			m_iTop = value;
		}
	}

	public int Right
	{
		get
		{
			return m_iRight;
		}
		set
		{
			m_iRight = value;
		}
	}

	public int Bottom
	{
		get
		{
			return m_iBottom;
		}
		set
		{
			m_iBottom = value;
		}
	}

	public MsofbtAnchor(MsoBase parent)
		: base(parent)
	{
	}

	public MsofbtAnchor(MsoBase parent, byte[] data, int iOffset)
		: base(parent, data, iOffset)
	{
	}

	public override void InfillInternalData(Stream stream, int iOffset, List<int> arrBreaks, List<List<BiffRecordRaw>> arrRecords)
	{
		m_iLength = 16;
		MsoBase.WriteInt32(stream, m_iLeft);
		MsoBase.WriteInt32(stream, m_iTop);
		MsoBase.WriteInt32(stream, m_iRight);
		MsoBase.WriteInt32(stream, m_iBottom);
	}

	public override void ParseStructure(Stream stream)
	{
		m_iLeft = MsoBase.ReadInt32(stream);
		m_iTop = MsoBase.ReadInt32(stream);
		m_iRight = MsoBase.ReadInt32(stream);
		m_iBottom = MsoBase.ReadInt32(stream);
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 16;
	}
}
