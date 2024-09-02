using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records.Charts;

[Biff(TBIFFRecord.ChartSerAuxTrend)]
[CLSCompliant(false)]
internal class ChartSerAuxTrendRecord : BiffRecordRaw
{
	public enum TRegression
	{
		Polynomial,
		Exponential,
		Logarithmic,
		Power,
		MovingAverage
	}

	public static readonly byte[] DEF_NAN_BYTE_ARRAY = new byte[8] { 255, 255, 255, 255, 0, 1, 255, 255 };

	public static readonly double DEF_NAN_VALUE = BitConverter.ToDouble(DEF_NAN_BYTE_ARRAY, 0);

	public const int DEF_RECORD_SIZE = 28;

	[BiffRecordPos(0, 1)]
	private byte m_RegType;

	[BiffRecordPos(1, 1)]
	private byte m_Order = 1;

	[BiffRecordPos(2, 8, TFieldType.Float)]
	private double m_numIntercept = DEF_NAN_VALUE;

	[BiffRecordPos(10, 1)]
	private byte m_bEquation;

	[BiffRecordPos(11, 1)]
	private byte m_bRSquared;

	[BiffRecordPos(12, 8, TFieldType.Float)]
	private double m_NumForecast;

	[BiffRecordPos(20, 8, TFieldType.Float)]
	private double m_NumBackcast;

	public TRegression RegressionType
	{
		get
		{
			return (TRegression)m_RegType;
		}
		set
		{
			m_RegType = (byte)value;
		}
	}

	public byte Order
	{
		get
		{
			return m_Order;
		}
		set
		{
			m_Order = value;
		}
	}

	public double NumIntercept
	{
		get
		{
			return m_numIntercept;
		}
		set
		{
			m_numIntercept = value;
		}
	}

	public bool IsEquation
	{
		get
		{
			return m_bEquation == 1;
		}
		set
		{
			m_bEquation = (value ? ((byte)1) : ((byte)0));
		}
	}

	public bool IsRSquared
	{
		get
		{
			return m_bRSquared == 1;
		}
		set
		{
			m_bRSquared = (value ? ((byte)1) : ((byte)0));
		}
	}

	public double NumForecast
	{
		get
		{
			return m_NumForecast;
		}
		set
		{
			m_NumForecast = value;
		}
	}

	public double NumBackcast
	{
		get
		{
			return m_NumBackcast;
		}
		set
		{
			m_NumBackcast = value;
		}
	}

	public override int MinimumRecordSize => 28;

	public override int MaximumRecordSize => 28;

	public void UpdateType(OfficeTrendLineType type)
	{
		if (type != OfficeTrendLineType.Linear)
		{
			m_RegType = (byte)type;
		}
		else
		{
			m_RegType = 0;
		}
	}

	public ChartSerAuxTrendRecord()
	{
	}

	public ChartSerAuxTrendRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public ChartSerAuxTrendRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_RegType = provider.ReadByte(iOffset);
		m_Order = provider.ReadByte(iOffset + 1);
		m_numIntercept = provider.ReadDouble(iOffset + 2);
		m_bEquation = provider.ReadByte(iOffset + 10);
		m_bRSquared = provider.ReadByte(iOffset + 11);
		m_NumForecast = provider.ReadDouble(iOffset + 12);
		m_NumBackcast = provider.ReadDouble(iOffset + 20);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		provider.WriteByte(iOffset, m_RegType);
		provider.WriteByte(iOffset + 1, m_Order);
		provider.WriteDouble(iOffset + 2, m_numIntercept);
		provider.WriteByte(iOffset + 10, m_bEquation);
		provider.WriteByte(iOffset + 11, m_bRSquared);
		provider.WriteDouble(iOffset + 12, m_NumForecast);
		provider.WriteDouble(iOffset + 20, m_NumBackcast);
		m_iLength = 28;
	}
}
