using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records.Charts;

[Biff(TBIFFRecord.Chart3D)]
[CLSCompliant(false)]
internal class Chart3DRecord : BiffRecordRaw
{
	private const int DEF_RECORD_SIZE = 14;

	[BiffRecordPos(0, 2)]
	private ushort m_usRotationAngle = 20;

	[BiffRecordPos(2, 2, true)]
	private short m_sElevationAngle = 15;

	[BiffRecordPos(4, 2)]
	private ushort m_usDistance = 15;

	[BiffRecordPos(6, 2)]
	private ushort m_usHeight = 100;

	[BiffRecordPos(8, 2)]
	private ushort m_usDepth = 100;

	[BiffRecordPos(10, 2)]
	private ushort m_usGap = 150;

	[BiffRecordPos(12, 2)]
	private ushort m_usOptions;

	[BiffRecordPos(12, 0, TFieldType.Bit)]
	private bool m_bPerspective;

	[BiffRecordPos(12, 1, TFieldType.Bit)]
	private bool m_bClustered;

	[BiffRecordPos(12, 2, TFieldType.Bit)]
	private bool m_bAutoScaling = true;

	[BiffRecordPos(12, 4, TFieldType.Bit)]
	private bool m_bReserved = true;

	[BiffRecordPos(12, 5, TFieldType.Bit)]
	private bool m_b2DWalls;

	private bool m_bDefaultElevation = true;

	private bool m_bDefaultRotation = true;

	public ushort RotationAngle
	{
		get
		{
			return m_usRotationAngle;
		}
		set
		{
			if (value < 0 || value > 360)
			{
				throw new ArgumentOutOfRangeException("value", "Value cannot be less than 0 or greater than 360.");
			}
			m_usRotationAngle = value;
			m_bDefaultRotation = false;
		}
	}

	public short ElevationAngle
	{
		get
		{
			return m_sElevationAngle;
		}
		set
		{
			if (value < -90 || value > 90)
			{
				throw new ArgumentOutOfRangeException("value", "Value cannot be less than -90 or greater than 90.");
			}
			m_sElevationAngle = value;
			m_bDefaultElevation = false;
		}
	}

	public bool IsDefaultRotation
	{
		get
		{
			return m_bDefaultRotation;
		}
		set
		{
			m_bDefaultRotation = value;
		}
	}

	public bool IsDefaultElevation
	{
		get
		{
			return m_bDefaultElevation;
		}
		set
		{
			m_bDefaultElevation = value;
		}
	}

	public ushort DistanceFromEye
	{
		get
		{
			return m_usDistance;
		}
		set
		{
			if (value < 0 || value > 100)
			{
				throw new ArgumentOutOfRangeException("value", "Value cannot be less than 0 or greater than 100.");
			}
			m_usDistance = value;
		}
	}

	public ushort Height
	{
		get
		{
			return m_usHeight;
		}
		set
		{
			m_usHeight = value;
		}
	}

	public ushort Depth
	{
		get
		{
			return m_usDepth;
		}
		set
		{
			if (value != m_usDepth)
			{
				m_usDepth = value;
			}
		}
	}

	public ushort SeriesSpace
	{
		get
		{
			return m_usGap;
		}
		set
		{
			if (value != m_usGap)
			{
				m_usGap = value;
			}
		}
	}

	public ushort Options => m_usOptions;

	public bool IsPerspective
	{
		get
		{
			return m_bPerspective;
		}
		set
		{
			if (value != m_bPerspective)
			{
				m_bPerspective = value;
			}
		}
	}

	public bool IsClustered
	{
		get
		{
			return m_bClustered;
		}
		set
		{
			if (value != m_bClustered)
			{
				m_bClustered = value;
			}
		}
	}

	public bool IsAutoScaled
	{
		get
		{
			return m_bAutoScaling;
		}
		set
		{
			if (value != m_bAutoScaling)
			{
				m_bAutoScaling = value;
			}
		}
	}

	public bool Is2DWalls
	{
		get
		{
			return m_b2DWalls;
		}
		set
		{
			if (value != m_b2DWalls)
			{
				m_b2DWalls = value;
			}
		}
	}

	public Chart3DRecord()
	{
	}

	public Chart3DRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public Chart3DRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 14;
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_usRotationAngle = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_sElevationAngle = provider.ReadInt16(iOffset);
		iOffset += 2;
		m_usDistance = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_usHeight = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_usDepth = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_usGap = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_usOptions = provider.ReadUInt16(iOffset);
		m_bPerspective = provider.ReadBit(iOffset, 0);
		m_bClustered = provider.ReadBit(iOffset, 1);
		m_bAutoScaling = provider.ReadBit(iOffset, 2);
		m_bReserved = provider.ReadBit(iOffset, 4);
		m_b2DWalls = provider.ReadBit(iOffset, 5);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_usOptions &= 55;
		m_iLength = GetStoreSize(version);
		provider.WriteUInt16(iOffset, m_usRotationAngle);
		iOffset += 2;
		provider.WriteInt16(iOffset, m_sElevationAngle);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_usDistance);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_usHeight);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_usDepth);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_usGap);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_usOptions);
		provider.WriteBit(iOffset, m_bPerspective, 0);
		provider.WriteBit(iOffset, m_bClustered, 1);
		provider.WriteBit(iOffset, m_bAutoScaling, 2);
		provider.WriteBit(iOffset, m_bReserved, 4);
		provider.WriteBit(iOffset, m_b2DWalls, 5);
	}

	public static bool operator ==(Chart3DRecord chart3D, Chart3DRecord chart3D2)
	{
		if (object.Equals(chart3D, null) && object.Equals(chart3D2, null))
		{
			return true;
		}
		if (object.Equals(chart3D, null) || object.Equals(chart3D2, null))
		{
			return false;
		}
		if (chart3D2.m_usRotationAngle == chart3D.m_usRotationAngle && chart3D2.m_sElevationAngle == chart3D.m_sElevationAngle && chart3D2.m_usDistance == chart3D.m_usDistance && chart3D2.m_usHeight == chart3D.m_usHeight && chart3D2.m_usDepth == chart3D.m_usDepth && chart3D2.m_usGap == chart3D.m_usGap && chart3D2.m_bPerspective == chart3D.m_bPerspective && chart3D2.m_bClustered == chart3D.m_bClustered && chart3D2.m_bAutoScaling == chart3D.m_bAutoScaling)
		{
			return chart3D2.m_b2DWalls == chart3D.m_b2DWalls;
		}
		return false;
	}

	public static bool operator !=(Chart3DRecord chart3D, Chart3DRecord chart3D2)
	{
		return !(chart3D == chart3D2);
	}
}
