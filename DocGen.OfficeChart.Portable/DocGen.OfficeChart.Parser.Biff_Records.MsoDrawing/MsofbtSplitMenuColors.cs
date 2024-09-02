using System;
using System.Collections.Generic;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records.MsoDrawing;

[MsoDrawing(MsoRecords.msofbtSplitMenuColors)]
[CLSCompliant(false)]
internal class MsofbtSplitMenuColors : MsoBase
{
	private const int RecordSize = 16;

	[BiffRecordPos(0, 4, true)]
	private int m_iFillColor;

	[BiffRecordPos(4, 4, true)]
	private int m_iLineColor;

	[BiffRecordPos(8, 4, true)]
	private int m_iShadowColor;

	[BiffRecordPos(12, 4, true)]
	private int m_i3DColor;

	public int FillColor
	{
		get
		{
			return m_iFillColor;
		}
		set
		{
			m_iFillColor = value;
		}
	}

	public int LineColor
	{
		get
		{
			return m_iLineColor;
		}
		set
		{
			m_iLineColor = value;
		}
	}

	public int ShadowColor
	{
		get
		{
			return m_iShadowColor;
		}
		set
		{
			m_iShadowColor = value;
		}
	}

	public int Color3D
	{
		get
		{
			return m_i3DColor;
		}
		set
		{
			m_i3DColor = value;
		}
	}

	public MsofbtSplitMenuColors(MsoBase parent)
		: base(parent)
	{
	}

	public MsofbtSplitMenuColors(MsoBase parent, byte[] data, int iOffset)
		: base(parent, data, iOffset)
	{
	}

	public override void InfillInternalData(Stream stream, int iOffset, List<int> arrBreaks, List<List<BiffRecordRaw>> arrRecords)
	{
		MsoBase.WriteInt32(stream, m_iFillColor);
		MsoBase.WriteInt32(stream, m_iLineColor);
		MsoBase.WriteInt32(stream, m_iShadowColor);
		MsoBase.WriteInt32(stream, m_i3DColor);
		m_iLength = 16;
	}

	public override void ParseStructure(Stream stream)
	{
		m_iFillColor = MsoBase.ReadInt32(stream);
		m_iLineColor = MsoBase.ReadInt32(stream);
		m_iShadowColor = MsoBase.ReadInt32(stream);
		m_i3DColor = MsoBase.ReadInt32(stream);
	}
}
