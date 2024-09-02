using System;
using System.Collections.Generic;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records.MsoDrawing;

[MsoDrawing(MsoRecords.msofbtDg)]
[CLSCompliant(false)]
internal class MsofbtDg : MsoBase
{
	private const int DEF_INSTANCE = 1;

	private const int DEF_RECORD_SIZE = 8;

	[BiffRecordPos(0, 4)]
	private uint m_uiShapesNumber;

	[BiffRecordPos(4, 4)]
	private int m_iLastId;

	public uint ShapesNumber
	{
		get
		{
			return m_uiShapesNumber;
		}
		set
		{
			m_uiShapesNumber = value;
		}
	}

	public int LastId
	{
		get
		{
			return m_iLastId;
		}
		set
		{
			m_iLastId = value;
		}
	}

	public MsofbtDg(MsoBase parent)
		: base(parent)
	{
		base.Instance = 1;
	}

	public MsofbtDg(MsoBase parent, byte[] data, int iOffset)
		: base(parent, data, iOffset)
	{
	}

	public override void InfillInternalData(Stream stream, int iOffset, List<int> arrBreaks, List<List<BiffRecordRaw>> arrRecords)
	{
		m_iLength = 8;
		MsoBase.WriteUInt32(stream, m_uiShapesNumber);
		MsoBase.WriteInt32(stream, m_iLastId);
	}

	public override void ParseStructure(Stream stream)
	{
		m_uiShapesNumber = MsoBase.ReadUInt32(stream);
		m_iLastId = MsoBase.ReadInt32(stream);
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 8;
	}
}
