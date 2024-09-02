using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records.Charts;

[Biff(TBIFFRecord.ChartAttachedLabelLayout)]
[CLSCompliant(false)]
internal class ChartAttachedLabelLayoutRecord : BiffRecordRaw
{
	private const int DEF_RECORD_SIZE = 60;

	private byte[] DEF_HEADER = new byte[12]
	{
		157, 8, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0
	};

	private byte[] m_frtHeader;

	private int m_dwCheckSum;

	private int m_info;

	private bool m_unUsed;

	private int m_autoLayoutType;

	private byte[] m_reserved1;

	private LayoutModes m_wXMode;

	private LayoutModes m_wYMode;

	private LayoutModes m_wWidthMode;

	private LayoutModes m_wHeightMode;

	private double m_x;

	private double m_y;

	private double m_dx;

	private double m_dy;

	private byte[] m_reserved2 = new byte[2];

	private byte[] FrtHeader => m_frtHeader;

	private int dwCheckSum
	{
		get
		{
			return m_dwCheckSum;
		}
		set
		{
			m_dwCheckSum = value;
		}
	}

	public LayoutModes WXMode
	{
		get
		{
			return m_wXMode;
		}
		set
		{
			m_wXMode = value;
		}
	}

	public LayoutModes WYMode
	{
		get
		{
			return m_wYMode;
		}
		set
		{
			m_wYMode = value;
		}
	}

	public LayoutModes WWidthMode
	{
		get
		{
			return m_wWidthMode;
		}
		set
		{
			m_wWidthMode = value;
		}
	}

	public LayoutModes WHeightMode
	{
		get
		{
			return m_wHeightMode;
		}
		set
		{
			m_wHeightMode = value;
		}
	}

	public double X
	{
		get
		{
			return m_x;
		}
		set
		{
			m_x = value;
		}
	}

	public double Y
	{
		get
		{
			return m_y;
		}
		set
		{
			m_y = value;
		}
	}

	public double Dx
	{
		get
		{
			return m_dx;
		}
		set
		{
			m_dx = value;
		}
	}

	public double Dy
	{
		get
		{
			return m_dy;
		}
		set
		{
			m_dy = value;
		}
	}

	public ChartAttachedLabelLayoutRecord()
	{
		m_frtHeader = DEF_HEADER;
	}

	public ChartAttachedLabelLayoutRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
		m_frtHeader = DEF_HEADER;
	}

	public ChartAttachedLabelLayoutRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		provider.ReadArray(iOffset, m_frtHeader);
		m_dwCheckSum = provider.ReadInt32(iOffset + 12);
		m_info = provider.ReadInt32(iOffset + 16);
		m_wXMode = (LayoutModes)Enum.Parse(typeof(LayoutModes), provider.ReadInt16(iOffset + 18).ToString(), ignoreCase: true);
		m_wYMode = (LayoutModes)Enum.Parse(typeof(LayoutModes), provider.ReadInt16(iOffset + 20).ToString(), ignoreCase: true);
		m_wWidthMode = (LayoutModes)Enum.Parse(typeof(LayoutModes), provider.ReadInt16(iOffset + 22).ToString(), ignoreCase: true);
		m_wHeightMode = (LayoutModes)Enum.Parse(typeof(LayoutModes), provider.ReadInt16(iOffset + 24).ToString(), ignoreCase: true);
		m_x = provider.ReadDouble(iOffset + 26);
		m_y = provider.ReadDouble(iOffset + 34);
		m_dx = provider.ReadDouble(iOffset + 42);
		m_dy = provider.ReadDouble(iOffset + 50);
		provider.ReadArray(iOffset + 58, m_reserved2);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_iLength = GetStoreSize(version);
		provider.WriteBytes(iOffset, m_frtHeader);
		provider.WriteInt32(iOffset + 12, m_dwCheckSum);
		provider.WriteInt32(iOffset + 16, m_info);
		provider.WriteInt32(iOffset + 18, (int)Enum.Parse(typeof(LayoutModes), m_wXMode.ToString(), ignoreCase: true));
		provider.WriteInt32(iOffset + 20, (int)Enum.Parse(typeof(LayoutModes), m_wYMode.ToString(), ignoreCase: true));
		provider.WriteInt32(iOffset + 22, (int)Enum.Parse(typeof(LayoutModes), m_wWidthMode.ToString(), ignoreCase: true));
		provider.WriteInt32(iOffset + 24, (int)Enum.Parse(typeof(LayoutModes), m_wHeightMode.ToString(), ignoreCase: true));
		provider.WriteDouble(iOffset + 26, m_x);
		provider.WriteDouble(iOffset + 34, m_y);
		provider.WriteDouble(iOffset + 42, m_dx);
		provider.WriteDouble(iOffset + 50, m_dy);
		provider.WriteBytes(iOffset + 58, m_reserved2);
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 60;
	}

	private int CheckSum()
	{
		return 0;
	}
}
