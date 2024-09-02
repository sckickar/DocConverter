using System;
using System.Globalization;
using DocGen.OfficeChart.Implementation;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.RK)]
[CLSCompliant(false)]
internal class RKRecord : CellPositionBase, IDoubleValue, IValueHolder
{
	internal const int DEF_RECORD_SIZE = 10;

	internal const int DEF_RECORD_SIZE_WITH_HEADER = 14;

	internal const int DEF_NUMBER_OFFSET = 6;

	internal const int DEF_HEADER_NUMBER_OFFSET = 10;

	public const uint DEF_RK_MASK = 4294967292u;

	private const int MaxRkNumber = 536870912;

	private const int MinRkNumber = -536870912;

	[BiffRecordPos(6, 4, true)]
	private int m_iNumber;

	[BiffRecordPos(6, 0, TFieldType.Bit)]
	private bool m_bValueNotChanged;

	[BiffRecordPos(6, 1, TFieldType.Bit)]
	private bool m_bIEEEFloat;

	public int RKNumberInt
	{
		get
		{
			return m_iNumber;
		}
		set
		{
			m_iNumber = value;
			m_bValueNotChanged = (value & 1) != 0;
			m_bIEEEFloat = (value & 2) != 0;
		}
	}

	public double RKNumber
	{
		get
		{
			long num = m_iNumber >> 2;
			if (IsNotFloat)
			{
				double num2 = num;
				if (!IsValueChanged)
				{
					return num2;
				}
				return num2 / 100.0;
			}
			double num3 = BitConverterGeneral.Int64BitsToDouble(num << 34);
			if (!IsValueChanged)
			{
				return num3;
			}
			return num3 / 100.0;
		}
		set
		{
			SetRKNumber(value);
		}
	}

	public override int MinimumRecordSize => 10;

	public override int MaximumRecordSize => 10;

	public override int MaximumMemorySize => 10;

	public bool IsNotFloat
	{
		get
		{
			return m_bIEEEFloat;
		}
		set
		{
			m_bIEEEFloat = value;
		}
	}

	public bool IsValueChanged
	{
		get
		{
			return m_bValueNotChanged;
		}
		set
		{
			m_bValueNotChanged = value;
		}
	}

	public double DoubleValue => RKNumber;

	public object Value
	{
		get
		{
			return RKNumber;
		}
		set
		{
			RKNumber = (double)value;
		}
	}

	protected override void ParseCellData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_iNumber = provider.ReadInt32(iOffset);
		m_bIEEEFloat = provider.ReadBit(iOffset, 1);
		m_bValueNotChanged = provider.ReadBit(iOffset, 0);
	}

	protected override void InfillCellData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		provider.WriteInt32(iOffset, m_iNumber);
		provider.WriteBit(iOffset, m_bIEEEFloat, 1);
		provider.WriteBit(iOffset, m_bValueNotChanged, 0);
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		int num = 10;
		if (version != 0)
		{
			num += 4;
		}
		return num;
	}

	public void SetRKNumber(string value)
	{
		if (double.TryParse(value, NumberStyles.Any, null, out var result))
		{
			SetRKNumber(result);
		}
	}

	public void SetRKNumber(double value)
	{
		m_iNumber = ConvertToRKNumber(value);
		m_bValueNotChanged = (m_iNumber & 1) != 0;
		m_bIEEEFloat = (m_iNumber & 2) != 0;
	}

	public void SetConvertedNumber(int rkNumber)
	{
		m_iNumber = rkNumber;
		m_bValueNotChanged = (m_iNumber & 1) != 0;
		m_bIEEEFloat = (m_iNumber & 2) != 0;
	}

	public void SetRKRecord(MulRKRecord.RkRec rc)
	{
		m_usExtendedFormat = rc.ExtFormatIndex;
		m_iNumber = rc.Rk;
		m_bIEEEFloat = (m_iNumber & 2) == 2;
		m_bValueNotChanged = (m_iNumber & 1) == 1;
	}

	public MulRKRecord.RkRec GetAsRkRec()
	{
		if (m_bValueNotChanged)
		{
			m_iNumber |= 1;
		}
		if (m_bIEEEFloat)
		{
			m_iNumber |= 2;
		}
		return new MulRKRecord.RkRec(m_usExtendedFormat, m_iNumber);
	}

	public static int ConvertToRKNumber(string value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (double.TryParse(value, NumberStyles.Any, null, out var result))
		{
			return ConvertToRKNumber(result);
		}
		return int.MaxValue;
	}

	public static int ConvertToRKNumber(double value)
	{
		if (value > 536870912.0 || value < -536870912.0)
		{
			return int.MaxValue;
		}
		long num = BitConverterGeneral.DoubleToInt64Bits(value);
		int result = 0;
		bool flag = true;
		if ((num & 0x3FFFFFFFFL) == 0L)
		{
			result = ConvertDouble(num, bValueNotChanged: false);
			flag = false;
		}
		if (flag)
		{
			int num2 = (int)Math.Round(value, 0);
			if (value - (double)num2 == 0.0 && num2 > 0 && num2 <= 1073741823)
			{
				result = num2 << 2;
				result |= 2;
				flag = false;
			}
		}
		if (flag)
		{
			value *= 100.0;
			num = BitConverterGeneral.DoubleToInt64Bits(value);
			if ((num & 0x3FFFFFFFFL) == 0L)
			{
				result = ConvertDouble(num, bValueNotChanged: true);
				flag = false;
			}
		}
		if (flag)
		{
			int num3 = (int)Math.Round(value, 0);
			if (value - (double)num3 == 0.0 && num3 > 0 && num3 <= 1073741823)
			{
				result = num3 << 2;
				result |= 3;
			}
		}
		if (!flag)
		{
			return result;
		}
		return int.MaxValue;
	}

	public static double ConvertToDouble(int rkNumber)
	{
		bool flag = (rkNumber & 1) != 0;
		bool num = (rkNumber & 2) != 0;
		long num2 = rkNumber >> 2;
		if (num)
		{
			double num3 = num2;
			if (!flag)
			{
				return num3;
			}
			return num3 / 100.0;
		}
		double num4 = BitConverterGeneral.Int64BitsToDouble(num2 << 34);
		if (!flag)
		{
			return num4;
		}
		return num4 / 100.0;
	}

	private static int ConvertDouble(long value, bool bValueNotChanged)
	{
		int num = (int)(value >> 32);
		if (bValueNotChanged)
		{
			num |= 1;
		}
		return num;
	}

	public static double EncodeRK(int value)
	{
		double num = (((value & 2) <= 0) ? SafeGetDouble(value) : ((double)(value >> 2)));
		if ((value & 1) > 0)
		{
			num /= 100.0;
		}
		return num;
	}

	private static double SafeGetDouble(int value)
	{
		byte[] array = new byte[8];
		Buffer.BlockCopy(BitConverter.GetBytes(value & 0xFFFFFFFCu), 0, array, 4, 4);
		return BitConverter.ToDouble(array, 0);
	}

	public static int ReadValue(DataProvider provider, int recordStart, OfficeVersion version)
	{
		recordStart += 10;
		if (version != 0)
		{
			recordStart += 4;
		}
		return provider.ReadInt32(recordStart);
	}
}
