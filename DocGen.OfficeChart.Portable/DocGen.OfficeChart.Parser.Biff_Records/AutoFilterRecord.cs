using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.AutoFilter)]
[CLSCompliant(false)]
internal class AutoFilterRecord : BiffRecordRaw, ICloneable
{
	public class DOPER : ICloneable
	{
		public enum DOPERDataType
		{
			FilterNotUsed = 0,
			RKNumber = 2,
			Number = 4,
			String = 6,
			BoolOrError = 8,
			MatchBlanks = 12,
			MatchNonBlanks = 14
		}

		public enum DOPERComparisonSign
		{
			Less = 1,
			Equal,
			LessOrEqual,
			Greater,
			NotEqual,
			GreaterOrEqual
		}

		private const int DEF_SIZE = 10;

		private const int DEF_DATATYPE_OFFSET = 0;

		private const int DEF_SIGN_OFFSET = 1;

		private const int DEF_VALUE_OFFSET = 2;

		private const int DEF_STRING_LENGTH_OFFSET = 6;

		private byte[] m_data = new byte[10];

		private string m_strValue = string.Empty;

		public DOPERDataType DataType
		{
			get
			{
				return (DOPERDataType)m_data[0];
			}
			set
			{
				m_data[0] = (byte)value;
			}
		}

		public DOPERComparisonSign ComparisonSign
		{
			get
			{
				return (DOPERComparisonSign)m_data[1];
			}
			set
			{
				m_data[1] = (byte)value;
			}
		}

		public int RKNumber
		{
			get
			{
				return BitConverter.ToInt32(m_data, 2);
			}
			set
			{
				DataType = DOPERDataType.RKNumber;
				BitConverter.GetBytes(value).CopyTo(m_data, 2);
			}
		}

		public double Number
		{
			get
			{
				return BitConverter.ToDouble(m_data, 2);
			}
			set
			{
				DataType = DOPERDataType.Number;
				BitConverter.GetBytes(value).CopyTo(m_data, 2);
			}
		}

		public bool IsBool
		{
			get
			{
				if (DataType == DOPERDataType.BoolOrError)
				{
					return m_data[2] == 1;
				}
				return false;
			}
		}

		public bool Boolean
		{
			get
			{
				return m_data[3] != 0;
			}
			set
			{
				DataType = DOPERDataType.BoolOrError;
				m_data[2] = 1;
				m_data[3] = (value ? ((byte)1) : ((byte)0));
			}
		}

		public byte ErrorCode
		{
			get
			{
				return m_data[3];
			}
			set
			{
				DataType = DOPERDataType.BoolOrError;
				m_data[2] = 0;
				m_data[3] = value;
			}
		}

		public bool HasAdditionalData => DataType == DOPERDataType.String;

		public byte StringLength
		{
			get
			{
				return m_data[6];
			}
			set
			{
				DataType = DOPERDataType.String;
				m_data[6] = value;
			}
		}

		public string StringValue
		{
			get
			{
				return m_strValue;
			}
			set
			{
				m_data[7] = 1;
				m_strValue = value;
				StringLength = (byte)value.Length;
			}
		}

		public int Length => 10 + ((StringLength > 0) ? (StringLength * 2 + 1) : 0);

		public int Parse(DataProvider provider, int iOffset)
		{
			provider.ReadArray(iOffset, m_data);
			return 10;
		}

		public int ParseAdditionalData(DataProvider provider, int iOffset)
		{
			if (!HasAdditionalData)
			{
				return 0;
			}
			if (DataType == DOPERDataType.String && StringLength > 0)
			{
				m_strValue = provider.ReadString(iOffset, StringLength, out var iBytesInString, isByteCounted: false);
				return iBytesInString;
			}
			return 0;
		}

		public int Serialize(DataProvider provider, int iOffset)
		{
			if (provider == null)
			{
				throw new ArgumentNullException("provider");
			}
			provider.WriteBytes(iOffset, m_data, 0, 10);
			return 10;
		}

		public int SerializeAdditionalData(DataProvider provider, int iOffset)
		{
			if (m_strValue == null || m_strValue.Length == 0)
			{
				return 0;
			}
			if (provider == null)
			{
				throw new ArgumentNullException("provider");
			}
			if (iOffset < 0)
			{
				throw new ArgumentOutOfRangeException("iOffset");
			}
			int num = iOffset;
			provider.WriteStringNoLenUpdateOffset(ref iOffset, m_strValue, bUnicode: true);
			return iOffset - num;
		}

		public object Clone()
		{
			DOPER dOPER = (DOPER)MemberwiseClone();
			dOPER.m_data = new byte[10];
			Buffer.BlockCopy(m_data, 0, dOPER.m_data, 0, 10);
			return dOPER;
		}
	}

	private const int DEF_RECORD_MIN_SIZE = 24;

	private const int DEF_TOP10_BITMASK = 65408;

	private const int DEF_TOP10_FIRSTBIT = 7;

	private const int DEF_FIRST_CONDITION_OFFSET = 4;

	private const int DEF_SECOND_CONDITION_OFFSET = 14;

	private const int DEF_ADDITIONAL_OFFSET = 24;

	[BiffRecordPos(0, 2)]
	private ushort m_usIndex;

	[BiffRecordPos(2, 2)]
	private ushort m_usOptions;

	[BiffRecordPos(2, 0, TFieldType.Bit)]
	private bool m_bOr;

	[BiffRecordPos(2, 2, TFieldType.Bit)]
	private bool m_bSimple1;

	[BiffRecordPos(2, 3, TFieldType.Bit)]
	private bool m_bSimple2;

	[BiffRecordPos(2, 4, TFieldType.Bit)]
	private bool m_bTop10;

	[BiffRecordPos(2, 5, TFieldType.Bit)]
	private bool m_bTop;

	[BiffRecordPos(2, 6, TFieldType.Bit)]
	private bool m_bPercent;

	private DOPER m_firstCondition = new DOPER();

	private DOPER m_secondCondition = new DOPER();

	public override bool NeedDataArray => true;

	public override int MinimumRecordSize => 24;

	public ushort Index
	{
		get
		{
			return m_usIndex;
		}
		set
		{
			m_usIndex = value;
		}
	}

	public ushort Options => m_usOptions;

	public bool IsSimple1
	{
		get
		{
			return m_bSimple1;
		}
		set
		{
			m_bSimple1 = value;
		}
	}

	public bool IsSimple2
	{
		get
		{
			return m_bSimple2;
		}
		set
		{
			m_bSimple2 = value;
		}
	}

	public bool IsTop10
	{
		get
		{
			return m_bTop10;
		}
		set
		{
			m_bTop10 = value;
		}
	}

	public bool IsTop
	{
		get
		{
			return m_bTop;
		}
		set
		{
			m_bTop = value;
		}
	}

	public bool IsPercent
	{
		get
		{
			return m_bPercent;
		}
		set
		{
			m_bPercent = value;
		}
	}

	public bool IsAnd
	{
		get
		{
			return !m_bOr;
		}
		set
		{
			m_bOr = !value;
		}
	}

	public int Top10Number
	{
		get
		{
			return BiffRecordRaw.GetUInt16BitsByMask(m_usOptions, 65408) >> 7;
		}
		set
		{
			if (value < 0 || value > 500)
			{
				throw new ArgumentOutOfRangeException("Top10Number");
			}
			BiffRecordRaw.SetUInt16BitsByMask(ref m_usOptions, 65408, (ushort)(value << 7));
		}
	}

	public DOPER FirstCondition => m_firstCondition;

	public DOPER SecondCondition => m_secondCondition;

	public bool IsBlank
	{
		get
		{
			if (FirstCondition.DataType == DOPER.DOPERDataType.MatchBlanks)
			{
				return SecondCondition.DataType == DOPER.DOPERDataType.FilterNotUsed;
			}
			return false;
		}
	}

	public bool IsNonBlank
	{
		get
		{
			if (FirstCondition.DataType == DOPER.DOPERDataType.MatchNonBlanks)
			{
				return SecondCondition.DataType == DOPER.DOPERDataType.FilterNotUsed;
			}
			return false;
		}
	}

	public AutoFilterRecord()
	{
	}

	public AutoFilterRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public AutoFilterRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		int num = iOffset;
		m_usIndex = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_usOptions = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_bOr = BiffRecordRaw.GetBitFromVar(m_usOptions, 0);
		m_bSimple1 = BiffRecordRaw.GetBitFromVar(m_usOptions, 2);
		m_bSimple2 = BiffRecordRaw.GetBitFromVar(m_usOptions, 3);
		m_bTop10 = BiffRecordRaw.GetBitFromVar(m_usOptions, 4);
		m_bTop = BiffRecordRaw.GetBitFromVar(m_usOptions, 5);
		m_bPercent = BiffRecordRaw.GetBitFromVar(m_usOptions, 6);
		m_firstCondition.Parse(provider, num + 4);
		m_secondCondition.Parse(provider, num + 14);
		iOffset = num + 24;
		iOffset += m_firstCondition.ParseAdditionalData(provider, iOffset);
		m_secondCondition.ParseAdditionalData(provider, iOffset);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_iLength = GetStoreSize(OfficeVersion.Excel97to2003);
		provider.WriteUInt16(iOffset, m_usIndex);
		iOffset += 2;
		SetBitInVar(ref m_usOptions, m_bOr, 0);
		SetBitInVar(ref m_usOptions, m_bSimple1, 2);
		SetBitInVar(ref m_usOptions, m_bSimple2, 3);
		SetBitInVar(ref m_usOptions, m_bTop10, 4);
		SetBitInVar(ref m_usOptions, m_bTop, 5);
		SetBitInVar(ref m_usOptions, m_bPercent, 6);
		provider.WriteUInt16(iOffset, m_usOptions);
		iOffset += 2;
		iOffset += m_firstCondition.Serialize(provider, iOffset);
		iOffset += m_secondCondition.Serialize(provider, iOffset);
		iOffset += m_firstCondition.SerializeAdditionalData(provider, iOffset);
		iOffset += m_secondCondition.SerializeAdditionalData(provider, iOffset);
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 4 + m_firstCondition.Length + m_secondCondition.Length;
	}

	public new object Clone()
	{
		AutoFilterRecord autoFilterRecord = (AutoFilterRecord)base.Clone();
		if (m_firstCondition != null)
		{
			autoFilterRecord.m_firstCondition = (DOPER)m_firstCondition.Clone();
		}
		if (m_secondCondition != null)
		{
			autoFilterRecord.m_secondCondition = (DOPER)m_secondCondition.Clone();
		}
		return autoFilterRecord;
	}
}
