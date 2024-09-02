using System;
using System.IO;

namespace DocGen.DocIO.ReaderWriter.Biff_Records;

[CLSCompliant(false)]
internal class SinglePropertyModifierRecord : BaseWordRecord
{
	private const int DEF_MASK_UNIQUE_ID = 511;

	private const int DEF_START_UNIQUE_ID = 0;

	private const int DEF_BIT_SPECIAL_HANDLE = 9;

	private const int DEF_MASK_SPRM_TYPE = 7168;

	private const int DEF_START_SPRM_TYPE = 10;

	private const int DEF_MASK_OPERAND_SIZE = 57344;

	private const int DEF_START_OPERAND_SIZE = 13;

	private const int DEF_MASK_WORD = 65535;

	private ushort m_usOptions;

	private int m_iOperandLength;

	private byte[] m_arrOperand;

	private short m_length = short.MaxValue;

	internal int UniqueID
	{
		get
		{
			return BaseWordRecord.GetBitsByMask(m_usOptions, 511, 0);
		}
		set
		{
			m_usOptions = (byte)BaseWordRecord.SetBitsByMask(m_usOptions, 511, value << 31);
		}
	}

	internal bool IsSpecialHandling
	{
		get
		{
			return BaseWordRecord.GetBit(m_usOptions, 9);
		}
		set
		{
			m_usOptions = (byte)BaseWordRecord.SetBit(m_usOptions, 9, value);
		}
	}

	internal WordSprmType SprmType
	{
		get
		{
			return (WordSprmType)BaseWordRecord.GetBitsByMask(m_usOptions, 7168, 10);
		}
		set
		{
			m_usOptions = (ushort)((uint)BaseWordRecord.SetBitsByMask(m_usOptions, 7168, (int)value << 10) & 0xFFFFu);
		}
	}

	internal WordSprmOperandSize OperandSize
	{
		get
		{
			return (WordSprmOperandSize)BaseWordRecord.GetBitsByMask(m_usOptions, 57344, 13);
		}
		set
		{
			m_usOptions = (ushort)((uint)BaseWordRecord.SetBitsByMask(m_usOptions, 57344, (int)value << 13) & 0xFFFFu);
		}
	}

	internal int OperandLength
	{
		get
		{
			if (m_arrOperand == null)
			{
				return 0;
			}
			return m_arrOperand.Length;
		}
	}

	internal byte[] Operand
	{
		get
		{
			if (m_iOperandLength > 0 && m_arrOperand == null)
			{
				m_arrOperand = new byte[m_iOperandLength];
			}
			return m_arrOperand;
		}
		set
		{
			m_arrOperand = value;
		}
	}

	internal bool BoolValue
	{
		get
		{
			if (OperandLength > 0)
			{
				return m_arrOperand[0] != 0;
			}
			return false;
		}
		set
		{
			m_arrOperand = new byte[ConvertToInt(OperandSize)];
			m_arrOperand[0] = (value ? ((byte)1) : ((byte)0));
		}
	}

	internal byte ByteValue
	{
		get
		{
			if (OperandLength > 0)
			{
				return Operand[0];
			}
			return 0;
		}
		set
		{
			if (ByteValue != value)
			{
				Operand = new byte[1] { value };
			}
		}
	}

	internal ushort UshortValue
	{
		get
		{
			if (OperandLength == 2)
			{
				return BitConverter.ToUInt16(Operand, 0);
			}
			return 0;
		}
		set
		{
			if (UshortValue != value)
			{
				Operand = BitConverter.GetBytes(value);
			}
		}
	}

	internal short ShortValue
	{
		get
		{
			if (OperandLength == 2)
			{
				return BitConverter.ToInt16(Operand, 0);
			}
			return 0;
		}
		set
		{
			if (ShortValue != value)
			{
				Operand = BitConverter.GetBytes(value);
			}
		}
	}

	internal int IntValue
	{
		get
		{
			if (OperandLength == 4)
			{
				return BitConverter.ToInt32(Operand, 0);
			}
			return 0;
		}
		set
		{
			if (IntValue != value)
			{
				Operand = BitConverter.GetBytes(value);
			}
		}
	}

	internal uint UIntValue
	{
		get
		{
			if (OperandLength == 4)
			{
				return BitConverter.ToUInt32(Operand, 0);
			}
			return 0u;
		}
		set
		{
			if (UIntValue != value)
			{
				Operand = BitConverter.GetBytes(value);
			}
		}
	}

	internal byte[] ByteArray
	{
		get
		{
			return Operand;
		}
		set
		{
			Operand = value;
		}
	}

	internal int TypedOptions
	{
		get
		{
			return m_usOptions;
		}
		set
		{
			m_usOptions = (ushort)value;
		}
	}

	internal ushort Options
	{
		get
		{
			return m_usOptions;
		}
		set
		{
			m_usOptions = value;
		}
	}

	internal override int Length
	{
		get
		{
			if (m_length == short.MaxValue)
			{
				m_length = GetSprmLength();
			}
			return m_length;
		}
	}

	internal WordSprmOptionType OptionType => (WordSprmOptionType)m_usOptions;

	internal SinglePropertyModifierRecord()
	{
	}

	internal SinglePropertyModifierRecord(int options)
	{
		m_usOptions = (ushort)options;
		m_iOperandLength = ConvertToInt(OperandSize);
	}

	internal SinglePropertyModifierRecord(Stream stream)
	{
		Parse(stream);
	}

	internal int Parse(byte[] arrBuffer, int iOffset)
	{
		int num = 0;
		if (iOffset + 2 > arrBuffer.Length)
		{
			throw new ArgumentOutOfRangeException("iOffset is too large.");
		}
		m_usOptions = BitConverter.ToUInt16(arrBuffer, iOffset);
		iOffset += 2;
		if (iOffset + 1 > arrBuffer.Length)
		{
			return iOffset + 1;
		}
		if (OperandSize == WordSprmOperandSize.Variable)
		{
			if (m_usOptions == 54792)
			{
				num = BitConverter.ToUInt16(arrBuffer, iOffset) - 1;
				if (num < 0)
				{
					num = 0;
				}
				iOffset += 2;
			}
			else
			{
				num = arrBuffer[iOffset];
				iOffset++;
			}
		}
		else
		{
			num = ConvertToInt(OperandSize);
		}
		m_arrOperand = new byte[num];
		try
		{
			Array.Copy(arrBuffer, iOffset, m_arrOperand, 0, m_arrOperand.Length);
		}
		catch (ArgumentNullException)
		{
		}
		catch (RankException)
		{
		}
		catch (ArrayTypeMismatchException)
		{
		}
		catch (InvalidCastException)
		{
		}
		catch (ArgumentException)
		{
		}
		iOffset += num;
		return iOffset;
	}

	internal void Parse(Stream stream)
	{
		int num = 0;
		byte[] array = new byte[2];
		stream.Read(array, 0, 2);
		m_usOptions = BitConverter.ToUInt16(array, 0);
		if (OperandSize == WordSprmOperandSize.Variable)
		{
			if (m_usOptions == 54792)
			{
				stream.Read(array, 0, 2);
				num = BitConverter.ToUInt16(array, 0) - 1;
			}
			num = stream.ReadByte();
		}
		else
		{
			num = ConvertToInt(OperandSize);
		}
		m_arrOperand = new byte[num];
		stream.Read(m_arrOperand, 0, num);
	}

	internal override int Save(byte[] arrData, int iOffset)
	{
		if (arrData == null)
		{
			throw new ArgumentNullException("arrData");
		}
		if (iOffset < 0 || iOffset + Length > arrData.Length)
		{
			throw new ArgumentOutOfRangeException("iOffset");
		}
		int num = iOffset;
		BitConverter.GetBytes(m_usOptions).CopyTo(arrData, iOffset);
		iOffset += 2;
		if (OperandSize == WordSprmOperandSize.Variable)
		{
			if (m_usOptions == 54792)
			{
				byte[] bytes = BitConverter.GetBytes((ushort)(m_arrOperand.Length + 1));
				arrData[iOffset++] = bytes[0];
				arrData[iOffset++] = bytes[1];
			}
			else
			{
				byte b = (byte)m_arrOperand.Length;
				arrData[iOffset++] = b;
			}
		}
		if (m_arrOperand != null)
		{
			m_arrOperand.CopyTo(arrData, iOffset);
			return iOffset - num + m_arrOperand.Length;
		}
		return iOffset - num;
	}

	internal int Save(BinaryWriter writer, Stream stream)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("stream");
		}
		int num = (int)stream.Position;
		writer.Write(m_usOptions);
		if (OperandSize == WordSprmOperandSize.Variable)
		{
			if (m_usOptions == 54792)
			{
				ushort value = (ushort)(m_arrOperand.Length + 1);
				writer.Write(value);
			}
			else
			{
				byte value2 = (byte)m_arrOperand.Length;
				writer.Write(value2);
			}
		}
		if (m_arrOperand == null)
		{
			m_arrOperand = new byte[ConvertToInt(OperandSize)];
		}
		writer.Write(m_arrOperand);
		return (int)(stream.Position - num);
	}

	internal SinglePropertyModifierRecord Clone()
	{
		if (Operand == null)
		{
			return null;
		}
		SinglePropertyModifierRecord singlePropertyModifierRecord = new SinglePropertyModifierRecord();
		singlePropertyModifierRecord.TypedOptions = TypedOptions;
		if (OperandLength > 0)
		{
			singlePropertyModifierRecord.Operand = new byte[OperandLength];
		}
		if (singlePropertyModifierRecord.Operand != null)
		{
			Operand.CopyTo(singlePropertyModifierRecord.Operand, 0);
		}
		return singlePropertyModifierRecord;
	}

	internal static int ConvertToInt(WordSprmOperandSize operandSize)
	{
		switch (operandSize)
		{
		case WordSprmOperandSize.Variable:
			return -1;
		case WordSprmOperandSize.TwoBytes:
		case WordSprmOperandSize.TwoBytes2:
		case WordSprmOperandSize.TwoBytes3:
			return 2;
		case WordSprmOperandSize.ThreeBytes:
			return 3;
		case WordSprmOperandSize.OneBit:
		case WordSprmOperandSize.OneByte:
			return 1;
		case WordSprmOperandSize.FourBytes:
			return 4;
		default:
			throw new ArgumentOutOfRangeException("operandSize");
		}
	}

	internal static void ParseOptions(int options, out WordSprmType type, out WordSprmOperandSize opSize)
	{
		type = (WordSprmType)BaseWordRecord.GetBitsByMask(options, 7168, 10);
		int bitsByMask = BaseWordRecord.GetBitsByMask(options, 57344, 13);
		opSize = (WordSprmOperandSize)bitsByMask;
	}

	private void DBG_TestParseOptions()
	{
	}

	private short GetSprmLength()
	{
		int num = 2;
		if (OperandSize == WordSprmOperandSize.Variable)
		{
			num++;
		}
		if (m_usOptions == 54792)
		{
			num++;
		}
		return (short)(num + ((Operand != null) ? m_arrOperand.Length : 0));
	}
}
