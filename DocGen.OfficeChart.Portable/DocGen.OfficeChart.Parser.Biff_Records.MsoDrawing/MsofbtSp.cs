using System;
using System.Collections.Generic;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records.MsoDrawing;

[MsoDrawing(MsoRecords.msofbtSp)]
[CLSCompliant(false)]
internal class MsofbtSp : MsoBase
{
	private const int DEF_VERSION = 2;

	private const int DEF_RECORD_SIZE = 8;

	[BiffRecordPos(0, 4, true)]
	private int m_iShapeId;

	[BiffRecordPos(4, 4)]
	private uint m_uiOptions;

	[BiffRecordPos(4, 0, TFieldType.Bit)]
	private bool m_bGroup;

	[BiffRecordPos(4, 1, TFieldType.Bit)]
	private bool m_bChild;

	[BiffRecordPos(4, 2, TFieldType.Bit)]
	private bool m_bPatriarch;

	[BiffRecordPos(4, 3, TFieldType.Bit)]
	private bool m_bDeleted;

	[BiffRecordPos(4, 4, TFieldType.Bit)]
	private bool m_bOleShape;

	[BiffRecordPos(4, 5, TFieldType.Bit)]
	private bool m_bHaveMaster;

	[BiffRecordPos(4, 6, TFieldType.Bit)]
	private bool m_bFlipH;

	[BiffRecordPos(4, 7, TFieldType.Bit)]
	private bool m_bFlipV;

	[BiffRecordPos(5, 0, TFieldType.Bit)]
	private bool m_bConnector;

	[BiffRecordPos(5, 1, TFieldType.Bit)]
	private bool m_bHaveAnchor;

	[BiffRecordPos(5, 2, TFieldType.Bit)]
	private bool m_bBackground;

	[BiffRecordPos(5, 3, TFieldType.Bit)]
	private bool m_bHaveSpt;

	public int ShapeId
	{
		get
		{
			return m_iShapeId;
		}
		set
		{
			m_iShapeId = value;
		}
	}

	public uint Options => m_uiOptions;

	public bool IsGroup
	{
		get
		{
			return m_bGroup;
		}
		set
		{
			m_bGroup = value;
		}
	}

	public bool IsChild
	{
		get
		{
			return m_bChild;
		}
		set
		{
			m_bChild = value;
		}
	}

	public bool IsPatriarch
	{
		get
		{
			return m_bPatriarch;
		}
		set
		{
			m_bPatriarch = value;
		}
	}

	public bool IsDeleted
	{
		get
		{
			return m_bDeleted;
		}
		set
		{
			m_bDeleted = value;
		}
	}

	public bool IsOleShape
	{
		get
		{
			return m_bOleShape;
		}
		set
		{
			m_bOleShape = value;
		}
	}

	public bool IsHaveMaster
	{
		get
		{
			return m_bHaveMaster;
		}
		set
		{
			m_bHaveMaster = value;
		}
	}

	public bool IsFlipH
	{
		get
		{
			return m_bFlipH;
		}
		set
		{
			m_bFlipH = value;
		}
	}

	public bool IsFlipV
	{
		get
		{
			return m_bFlipV;
		}
		set
		{
			m_bFlipV = value;
		}
	}

	public bool IsConnector
	{
		get
		{
			return m_bConnector;
		}
		set
		{
			m_bConnector = value;
		}
	}

	public bool IsHaveAnchor
	{
		get
		{
			return m_bHaveAnchor;
		}
		set
		{
			m_bHaveAnchor = value;
		}
	}

	public bool IsBackground
	{
		get
		{
			return m_bBackground;
		}
		set
		{
			m_bBackground = value;
		}
	}

	public bool IsHaveSpt
	{
		get
		{
			return m_bHaveSpt;
		}
		set
		{
			m_bHaveSpt = value;
		}
	}

	public MsofbtSp(MsoBase parent)
		: base(parent)
	{
		base.Version = 2;
	}

	public MsofbtSp(MsoBase parent, byte[] data, int iOffset)
		: base(parent, data, iOffset)
	{
	}

	public override void InfillInternalData(Stream stream, int iOffset, List<int> arrBreaks, List<List<BiffRecordRaw>> arrRecords)
	{
		MsoBase.WriteInt32(stream, m_iShapeId);
		SetBitInVar(ref m_uiOptions, m_bGroup, 0);
		SetBitInVar(ref m_uiOptions, m_bChild, 1);
		SetBitInVar(ref m_uiOptions, m_bPatriarch, 2);
		SetBitInVar(ref m_uiOptions, m_bDeleted, 3);
		SetBitInVar(ref m_uiOptions, m_bOleShape, 4);
		SetBitInVar(ref m_uiOptions, m_bHaveMaster, 5);
		SetBitInVar(ref m_uiOptions, m_bFlipH, 6);
		SetBitInVar(ref m_uiOptions, m_bFlipV, 7);
		SetBitInVar(ref m_uiOptions, m_bConnector, 8);
		SetBitInVar(ref m_uiOptions, m_bHaveAnchor, 9);
		SetBitInVar(ref m_uiOptions, m_bBackground, 10);
		SetBitInVar(ref m_uiOptions, m_bHaveSpt, 11);
		MsoBase.WriteUInt32(stream, m_uiOptions);
		m_iLength = 8;
	}

	public override void ParseStructure(Stream stream)
	{
		m_iShapeId = MsoBase.ReadInt32(stream);
		m_uiOptions = MsoBase.ReadUInt32(stream);
		m_bGroup = BiffRecordRaw.GetBitFromVar(m_uiOptions, 0);
		m_bChild = BiffRecordRaw.GetBitFromVar(m_uiOptions, 1);
		m_bPatriarch = BiffRecordRaw.GetBitFromVar(m_uiOptions, 2);
		m_bDeleted = BiffRecordRaw.GetBitFromVar(m_uiOptions, 3);
		m_bOleShape = BiffRecordRaw.GetBitFromVar(m_uiOptions, 4);
		m_bHaveMaster = BiffRecordRaw.GetBitFromVar(m_uiOptions, 5);
		m_bFlipH = BiffRecordRaw.GetBitFromVar(m_uiOptions, 6);
		m_bFlipV = BiffRecordRaw.GetBitFromVar(m_uiOptions, 7);
		m_bConnector = BiffRecordRaw.GetBitFromVar(m_uiOptions, 8);
		m_bHaveAnchor = BiffRecordRaw.GetBitFromVar(m_uiOptions, 9);
		m_bBackground = BiffRecordRaw.GetBitFromVar(m_uiOptions, 10);
		m_bHaveSpt = BiffRecordRaw.GetBitFromVar(m_uiOptions, 11);
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 8;
	}
}
