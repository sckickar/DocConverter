using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records.Charts;

[Biff(TBIFFRecord.ChartAxcext)]
[CLSCompliant(false)]
internal class ChartAxcextRecord : BiffRecordRaw
{
	[Flags]
	private enum OptionFlags
	{
		None = 0,
		DefaultMinimum = 1,
		DefaultMaximum = 2,
		DefaultMajorUnits = 4,
		DefaultMinorUnits = 8,
		DateAxis = 0x10,
		DefaultBaseUnits = 0x20,
		DefaultCrossPoint = 0x40,
		DefaultDateSettings = 0x80
	}

	private const int DEF_RECORD_SIZE = 18;

	[BiffRecordPos(0, 2)]
	private ushort m_usMinCategoryAxis;

	[BiffRecordPos(2, 2)]
	private ushort m_usMaxCategoryAxis;

	[BiffRecordPos(4, 2)]
	private ushort m_usMajor = 1;

	[BiffRecordPos(6, 2)]
	private ushort m_usMajorUnits;

	[BiffRecordPos(8, 2)]
	private ushort m_usMinor = 1;

	[BiffRecordPos(10, 2)]
	private ushort m_usMinorUnits;

	[BiffRecordPos(12, 2)]
	private ushort m_usBaseUnits;

	[BiffRecordPos(14, 2)]
	private ushort m_usCrossingPoint;

	[BiffRecordPos(16, 2)]
	private OptionFlags m_options = OptionFlags.DefaultMinimum | OptionFlags.DefaultMaximum | OptionFlags.DefaultMajorUnits | OptionFlags.DefaultMinorUnits | OptionFlags.DateAxis | OptionFlags.DefaultBaseUnits | OptionFlags.DefaultCrossPoint;

	public ushort MinCategoryOnAxis
	{
		get
		{
			return m_usMinCategoryAxis;
		}
		set
		{
			if (value != m_usMinCategoryAxis)
			{
				m_usMinCategoryAxis = value;
			}
		}
	}

	public ushort MaxCategoryOnAxis
	{
		get
		{
			return m_usMaxCategoryAxis;
		}
		set
		{
			if (value != m_usMaxCategoryAxis)
			{
				m_usMaxCategoryAxis = value;
			}
		}
	}

	public ushort Major
	{
		get
		{
			return m_usMajor;
		}
		set
		{
			if (value != m_usMajor)
			{
				m_usMajor = value;
			}
		}
	}

	public OfficeChartBaseUnit MajorUnits
	{
		get
		{
			return (OfficeChartBaseUnit)m_usMajorUnits;
		}
		set
		{
			m_usMajorUnits = (ushort)value;
		}
	}

	public ushort Minor
	{
		get
		{
			return m_usMinor;
		}
		set
		{
			if (value != m_usMinor)
			{
				m_usMinor = value;
			}
		}
	}

	public OfficeChartBaseUnit MinorUnits
	{
		get
		{
			return (OfficeChartBaseUnit)m_usMinorUnits;
		}
		set
		{
			m_usMinorUnits = (ushort)value;
		}
	}

	public OfficeChartBaseUnit BaseUnits
	{
		get
		{
			return (OfficeChartBaseUnit)m_usBaseUnits;
		}
		set
		{
			m_usBaseUnits = (ushort)value;
		}
	}

	public ushort CrossingPoint
	{
		get
		{
			return m_usCrossingPoint;
		}
		set
		{
			if (value != m_usCrossingPoint)
			{
				m_usCrossingPoint = value;
			}
		}
	}

	public ushort Options => (ushort)m_options;

	public bool UseDefaultMinimum
	{
		get
		{
			return (m_options & OptionFlags.DefaultMinimum) != 0;
		}
		set
		{
			if (value)
			{
				m_options |= OptionFlags.DefaultMinimum;
			}
			else
			{
				m_options &= ~OptionFlags.DefaultMinimum;
			}
		}
	}

	public bool UseDefaultMaximum
	{
		get
		{
			return (m_options & OptionFlags.DefaultMaximum) != 0;
		}
		set
		{
			if (value)
			{
				m_options |= OptionFlags.DefaultMaximum;
			}
			else
			{
				m_options &= ~OptionFlags.DefaultMaximum;
			}
		}
	}

	public bool UseDefaultMajorUnits
	{
		get
		{
			return (m_options & OptionFlags.DefaultMajorUnits) != 0;
		}
		set
		{
			if (value)
			{
				m_options |= OptionFlags.DefaultMajorUnits;
			}
			else
			{
				m_options &= ~OptionFlags.DefaultMajorUnits;
			}
		}
	}

	public bool UseDefaultMinorUnits
	{
		get
		{
			return (m_options & OptionFlags.DefaultMinorUnits) != 0;
		}
		set
		{
			if (value)
			{
				m_options |= OptionFlags.DefaultMinorUnits;
			}
			else
			{
				m_options &= ~OptionFlags.DefaultMinorUnits;
			}
		}
	}

	public bool IsDateAxis
	{
		get
		{
			return (m_options & OptionFlags.DateAxis) != 0;
		}
		set
		{
			if (value)
			{
				m_options |= OptionFlags.DateAxis;
			}
			else
			{
				m_options &= ~OptionFlags.DateAxis;
			}
		}
	}

	public bool UseDefaultBaseUnits
	{
		get
		{
			return (m_options & OptionFlags.DefaultBaseUnits) != 0;
		}
		set
		{
			if (value)
			{
				m_options |= OptionFlags.DefaultBaseUnits;
			}
			else
			{
				m_options &= ~OptionFlags.DefaultBaseUnits;
			}
		}
	}

	public bool UseDefaultCrossPoint
	{
		get
		{
			return (m_options & OptionFlags.DefaultCrossPoint) != 0;
		}
		set
		{
			if (value)
			{
				m_options |= OptionFlags.DefaultCrossPoint;
			}
			else
			{
				m_options &= ~OptionFlags.DefaultCrossPoint;
			}
		}
	}

	public bool UseDefaultDateSettings
	{
		get
		{
			return (m_options & OptionFlags.DefaultDateSettings) != 0;
		}
		set
		{
			if (value)
			{
				m_options |= OptionFlags.DefaultDateSettings;
			}
			else
			{
				m_options &= ~OptionFlags.DefaultDateSettings;
			}
		}
	}

	public ChartAxcextRecord()
	{
	}

	public ChartAxcextRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public ChartAxcextRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_usMinCategoryAxis = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_usMaxCategoryAxis = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_usMajor = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_usMajorUnits = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_usMinor = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_usMinorUnits = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_usBaseUnits = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_usCrossingPoint = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_options = (OptionFlags)provider.ReadUInt16(iOffset);
		if (m_usMajor > 1)
		{
			m_options &= ~OptionFlags.DefaultMajorUnits;
		}
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_iLength = GetStoreSize(version);
		provider.WriteUInt16(iOffset, m_usMinCategoryAxis);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_usMaxCategoryAxis);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_usMajor);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_usMajorUnits);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_usMinor);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_usMinorUnits);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_usBaseUnits);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_usCrossingPoint);
		iOffset += 2;
		provider.WriteUInt16(iOffset, (ushort)m_options);
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 18;
	}
}
