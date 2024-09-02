using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using DocGen.OfficeChart.Implementation.Exceptions;

namespace DocGen.OfficeChart.Parser.Biff_Records;

internal abstract class BiffRecordRaw : ICloneable, IBiffStorage
{
	private const int DEF_RESERVE_SIZE = 100;

	public const int DEF_RECORD_MAX_SIZE = 8224;

	public const int DEF_RECORD_MAX_SIZE_WITH_HADER = 8228;

	public const int DEF_HEADER_SIZE = 4;

	public const int DEF_BITS_IN_BYTE = 8;

	private const int DEF_BITS_IN_SHORT = 16;

	private const int DEF_BITS_IN_INT = 32;

	protected static Dictionary<int, SortedList<BiffRecordPosAttribute, FieldInfo>> m_ReflectCache = new Dictionary<int, SortedList<BiffRecordPosAttribute, FieldInfo>>(100);

	private static readonly Encoding s_latin1 = Encoding.GetEncoding("latin1");

	protected int m_iCode = -1;

	protected int m_iLength = -1;

	private bool m_bNeedInfill = true;

	public TBIFFRecord TypeCode => (TBIFFRecord)m_iCode;

	public int RecordCode => m_iCode;

	public int Length
	{
		get
		{
			return m_iLength;
		}
		set
		{
			m_iLength = value;
		}
	}

	public virtual byte[] Data
	{
		get
		{
			m_iLength = GetStoreSize(OfficeVersion.Excel97to2003);
			byte[] array = new byte[m_iLength];
			ByteArrayDataProvider provider = new ByteArrayDataProvider(array);
			InfillInternalData(provider, 0, OfficeVersion.Excel97to2003);
			return array;
		}
		set
		{
			if (value != null)
			{
				int iLength = value.Length;
				ParseStructure(new ByteArrayDataProvider(value), 0, iLength, OfficeVersion.Excel97to2003);
			}
		}
	}

	public virtual bool AutoGrowData
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public virtual long StreamPos
	{
		get
		{
			return -1L;
		}
		set
		{
		}
	}

	public virtual int MinimumRecordSize => 0;

	public virtual int MaximumRecordSize => 8224;

	public virtual int MaximumMemorySize => int.MaxValue;

	public bool NeedInfill
	{
		get
		{
			return m_bNeedInfill;
		}
		set
		{
			m_bNeedInfill = value;
		}
	}

	public virtual bool NeedDataArray => false;

	public virtual bool IsAllowShortData => false;

	public virtual bool NeedDecoding => true;

	public virtual int StartDecodingOffset => 0;

	public static Encoding LatinEncoding => s_latin1;

	public static int SkipBeginEndBlock(IList<BiffRecordRaw> recordList, int iPos)
	{
		recordList[iPos].CheckTypeCode(TBIFFRecord.Begin);
		int num = 1;
		iPos++;
		while (num > 0)
		{
			switch (recordList[iPos].TypeCode)
			{
			case TBIFFRecord.Begin:
				num++;
				break;
			case TBIFFRecord.End:
				num--;
				break;
			}
			iPos++;
		}
		return iPos;
	}

	internal static ushort GetUInt16BitsByMask(ushort value, ushort BitMask)
	{
		return (ushort)(value & BitMask);
	}

	internal static void SetUInt16BitsByMask(ref ushort destination, ushort BitMask, ushort value)
	{
		destination &= (ushort)(~BitMask);
		destination += (ushort)(value & BitMask);
	}

	internal static uint GetUInt32BitsByMask(uint value, uint BitMask)
	{
		return value & BitMask;
	}

	internal static void SetUInt32BitsByMask(ref uint destination, uint BitMask, uint value)
	{
		destination &= ~BitMask;
		destination += value & BitMask;
	}

	protected BiffRecordRaw()
	{
		object[] customAttributes = GetType().GetCustomAttributes(typeof(BiffAttribute), inherit: true);
		if (customAttributes.Length != 0)
		{
			BiffAttribute biffAttribute = (BiffAttribute)customAttributes[0];
			m_iCode = (int)biffAttribute.Code;
		}
	}

	protected BiffRecordRaw(Stream stream, out int itemSize)
	{
		throw new NotImplementedException();
	}

	protected BiffRecordRaw(int iReserve)
	{
	}

	public virtual void UpdateOffsets(List<BiffRecordRaw> records)
	{
		throw new ApplicationException("Class marked as offset contains field but does not provide override of UpdateOffset method. Or you try to call parent class virtual method. Please check code.");
	}

	public virtual void ParseStructure(DataProvider arrData, int iOffset, int iLength, OfficeVersion version)
	{
		throw new NotImplementedException(TypeCode.ToString());
	}

	public virtual void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		throw new NotImplementedException(TypeCode.ToString());
	}

	public virtual int GetStoreSize(OfficeVersion version)
	{
		int minimumRecordSize = MinimumRecordSize;
		if (minimumRecordSize == MaximumRecordSize)
		{
			return minimumRecordSize;
		}
		throw new ApplicationException("StoreSize should be overloaded " + TypeCode);
	}

	public static void CheckOffsetAndLength(byte[] arrData, int offset, int length)
	{
		int num = arrData.Length;
		if (offset < 0 || offset > num)
		{
			throw new ArgumentOutOfRangeException("offset", "");
		}
		if (length < 0 || length > num)
		{
			throw new ArgumentOutOfRangeException("length", "");
		}
		if (length + offset > num)
		{
			throw new ArgumentException("Length or offset has wrong value.", "length & offset");
		}
	}

	public static byte[] GetBytes(byte[] arrData, int offset, int length)
	{
		CheckOffsetAndLength(arrData, offset, length);
		byte[] array = new byte[length];
		Buffer.BlockCopy(arrData, offset, array, 0, length);
		return array;
	}

	public static byte GetByte(byte[] arrData, int offset)
	{
		CheckOffsetAndLength(arrData, offset, 1);
		return arrData[offset];
	}

	[CLSCompliant(false)]
	public static ushort GetUInt16(byte[] arrData, int offset)
	{
		CheckOffsetAndLength(arrData, offset, 2);
		return BitConverter.ToUInt16(arrData, offset);
	}

	[CLSCompliant(false)]
	public static short GetInt16(byte[] arrData, int offset)
	{
		CheckOffsetAndLength(arrData, offset, 2);
		return BitConverter.ToInt16(arrData, offset);
	}

	public static int GetInt32(byte[] arrData, int offset)
	{
		CheckOffsetAndLength(arrData, offset, 4);
		return BitConverter.ToInt32(arrData, offset);
	}

	[CLSCompliant(false)]
	public static uint GetUInt32(byte[] arrData, int offset)
	{
		CheckOffsetAndLength(arrData, offset, 4);
		return BitConverter.ToUInt32(arrData, offset);
	}

	public static long GetInt64(byte[] arrData, int offset)
	{
		CheckOffsetAndLength(arrData, offset, 28);
		return BitConverter.ToInt64(arrData, offset);
	}

	[CLSCompliant(false)]
	public static ulong GetUInt64(byte[] arrData, int offset)
	{
		CheckOffsetAndLength(arrData, offset, 8);
		return BitConverter.ToUInt64(arrData, offset);
	}

	public static float GetFloat(byte[] arrData, int offset)
	{
		CheckOffsetAndLength(arrData, offset, 4);
		return BitConverter.ToSingle(arrData, offset);
	}

	public static double GetDouble(byte[] arrData, int offset)
	{
		CheckOffsetAndLength(arrData, offset, 8);
		return BitConverter.ToDouble(arrData, offset);
	}

	public static bool GetBit(byte[] arrData, int offset, int bitPos)
	{
		if (bitPos < 0 || bitPos > 7)
		{
			throw new ArgumentOutOfRangeException("bitPos", "Bit Position cannot be less than 0 or greater than 7.");
		}
		if (arrData.Length <= offset)
		{
			throw new ArgumentOutOfRangeException("offset");
		}
		return (arrData[offset] & (1 << bitPos)) == 1 << bitPos;
	}

	public static string GetString16BitUpdateOffset(byte[] arrData, ref int offset)
	{
		int uInt = GetUInt16(arrData, offset);
		offset += 2;
		if (uInt > 0)
		{
			int iReadBytes;
			string @string = GetString(arrData, offset, uInt, out iReadBytes);
			offset += iReadBytes;
			return @string;
		}
		return string.Empty;
	}

	public static string GetStringUpdateOffset(byte[] arrData, ref int offset, int iStrLen)
	{
		if (iStrLen > 0)
		{
			int iReadBytes;
			string @string = GetString(arrData, offset, iStrLen, out iReadBytes);
			offset += iReadBytes;
			return @string;
		}
		return string.Empty;
	}

	public static string GetStringByteLen(byte[] arrData, int offset)
	{
		int @byte = GetByte(arrData, offset);
		return GetString(arrData, offset + 1, @byte);
	}

	public static string GetString(byte[] arrData, int offset, int iStrLen)
	{
		int iReadBytes;
		return GetString(arrData, offset, iStrLen, out iReadBytes);
	}

	public static string GetString(byte[] arrData, int offset, int iStrLen, out int iBytesInString, bool isByteCounted)
	{
		byte @byte = GetByte(arrData, offset);
		if (((@byte != 0 && !isByteCounted) ? (2 * iStrLen) : iStrLen) + (offset + 1) > arrData.Length)
		{
			throw new WrongBiffRecordDataException(string.Format("String and arrData array do not fit each other {0}."));
		}
		if (@byte == 0)
		{
			iBytesInString = iStrLen;
			return LatinEncoding.GetString(GetBytes(arrData, offset + 1, iStrLen), 0, iStrLen);
		}
		iBytesInString = (isByteCounted ? iStrLen : (iStrLen * 2));
		return Encoding.Unicode.GetString(GetBytes(arrData, offset + 1, iBytesInString), 0, iBytesInString);
	}

	public static string GetUnkTypeString(byte[] arrData, int offset, int[] continuePos, out int length, out byte[] rich, out byte[] extended)
	{
		string text = string.Empty;
		int num = 3;
		rich = null;
		extended = null;
		ushort uInt = GetUInt16(arrData, offset);
		byte @byte = GetByte(arrData, offset + 2);
		bool flag = (@byte & 1) == 1;
		bool flag2 = @byte == 8 || @byte == 9;
		bool flag3 = @byte == 4 || @byte == 5;
		bool flag4 = @byte == 12 || @byte == 13;
		int num2 = 3;
		short num3 = 0;
		if (flag2)
		{
			num3 = GetInt16(arrData, offset + 3);
			num2 = 5;
			extended = null;
			num += 2;
		}
		else if (flag3)
		{
			int @int = GetInt32(arrData, offset + 3);
			num2 = 7;
			rich = null;
			num += 4;
			extended = GetBytes(arrData, num, @int);
			num += @int;
		}
		else if (flag4)
		{
			num3 = GetInt16(arrData, offset + 3);
			int int2 = GetInt32(arrData, offset + 5);
			num2 = 9;
			num += 6;
			rich = GetBytes(arrData, num, num3 * 4);
			num += num3 * 4;
			extended = GetBytes(arrData, num, int2);
			num += int2;
		}
		int num4 = offset + num2;
		int num5 = 0;
		int iStartIndex = 0;
		while (num5 < uInt)
		{
			int num6 = (flag ? ((uInt - num5) * 2) : (uInt - num5));
			int num7 = FindNextBreak(continuePos, continuePos.Length, num4, ref iStartIndex) - num4;
			if (num6 <= num7)
			{
				text += (flag ? Encoding.Unicode.GetString(GetBytes(arrData, num4, num6), 0, num6) : LatinEncoding.GetString(GetBytes(arrData, num4, num6), 0, num6));
				num += num6;
				break;
			}
			if (num7 > 0)
			{
				text += (flag ? Encoding.Unicode.GetString(GetBytes(arrData, num4, num7), 0, num7) : LatinEncoding.GetString(GetBytes(arrData, num4, num7), 0, num7));
				num5 += (flag ? (num7 / 2) : num7);
			}
			if (arrData[num4 + num7] == 0 || arrData[num4 + num7] == 1)
			{
				flag = arrData[num4 + num7] == 1;
				num4++;
				num++;
			}
			num4 += num7;
			num += num7;
		}
		if (flag2)
		{
			rich = GetBytes(arrData, offset + num, num3 * 4);
			num += num3 * 4;
		}
		length = num;
		return text;
	}

	[CLSCompliant(false)]
	public static TAddr GetAddr(byte[] arrData, int offset)
	{
		TAddr result = default(TAddr);
		result.FirstRow = GetUInt16(arrData, offset);
		result.LastRow = GetUInt16(arrData, offset + 2);
		result.FirstCol = GetUInt16(arrData, offset + 4);
		result.LastCol = GetUInt16(arrData, offset + 6);
		return result;
	}

	public static byte[] GetRPNData(byte[] arrData, int offset, int length)
	{
		if (length == 0)
		{
			return new byte[0];
		}
		List<byte> list = new List<byte>(length * 2);
		int num = 0;
		byte @byte = GetByte(arrData, offset + num);
		if (@byte == 32 || @byte == 64 || @byte == 96)
		{
			int num2 = (GetByte(arrData, offset + num + 1) + 1) * (GetInt16(arrData, offset + num + 2) + 1) + 1;
			int num3 = 0;
			while (num2 > 0)
			{
				num3 = GetByte(arrData, offset + length + num3) switch
				{
					2 => num3 + (GetInt16(arrData, offset + length + num3 + 1) + 4), 
					4 => num3 + 3, 
					_ => num3 + 9, 
				};
				num2--;
			}
			for (int i = offset + num; i < offset + length + num3; i++)
			{
				list.Add(arrData[i]);
			}
			return list.ToArray();
		}
		return GetBytes(arrData, offset, length);
	}

	protected static int FindNextBreak(IList<int> arrBreaks, int iCount, int curPos, ref int iStartIndex)
	{
		for (int i = iStartIndex; i < iCount; i++)
		{
			int num = arrBreaks[i];
			if (curPos <= num)
			{
				iStartIndex = i;
				return num;
			}
		}
		return -1;
	}

	[CLSCompliant(false)]
	public static void SetUInt16(byte[] arrData, int offset, ushort value)
	{
		byte b = (byte)(value & 0xFFu);
		byte b2 = (byte)((uint)(value >> 8) & 0xFFu);
		arrData[offset] = b;
		arrData[offset + 1] = b2;
	}

	public static void SetBit(byte[] arrData, int offset, bool value, int bitPos)
	{
		if (arrData == null)
		{
			throw new ArgumentNullException("arrData");
		}
		if (bitPos < 0 || bitPos > 7)
		{
			throw new ArgumentOutOfRangeException("bitPos", "Bit Position can be zero or greater than 7.");
		}
		if (value)
		{
			arrData[offset] |= (byte)(1 << bitPos);
		}
		else
		{
			arrData[offset] &= (byte)(~(1 << bitPos));
		}
	}

	public static void SetInt16(byte[] arrData, int offset, short value)
	{
		Buffer.BlockCopy(BitConverter.GetBytes(value), 0, arrData, offset, 2);
	}

	public static void SetInt32(byte[] arrData, int offset, int value)
	{
		Buffer.BlockCopy(BitConverter.GetBytes(value), 0, arrData, offset, 4);
	}

	[CLSCompliant(false)]
	public static void SetUInt32(byte[] arrData, int offset, uint value)
	{
		Buffer.BlockCopy(BitConverter.GetBytes(value), 0, arrData, offset, 4);
	}

	public static void SetDouble(byte[] arrData, int offset, double value)
	{
		Buffer.BlockCopy(BitConverter.GetBytes(value), 0, arrData, offset, 8);
	}

	public static void SetStringNoLenUpdateOffset(byte[] arrData, ref int offset, string value)
	{
		if (value != null && value.Length != 0)
		{
			byte[] bytes = Encoding.Unicode.GetBytes(value);
			arrData[offset] = 1;
			SetBytes(arrData, offset + 1, bytes, 0, bytes.Length);
			offset += bytes.Length + 1;
		}
	}

	public static void SetStringByteLen(byte[] arrData, int offset, string value)
	{
		arrData[offset] = (byte)value.Length;
		SetStringNoLen(arrData, offset + 1, value);
	}

	protected internal static void SetBytes(byte[] arrBuffer, int offset, byte[] value, int pos, int length)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (pos < 0)
		{
			throw new ArgumentOutOfRangeException("pos", "Position cannot be zeroless.");
		}
		if (length < 0)
		{
			throw new ArgumentOutOfRangeException("length", "Length of data to copy must be greater then zero.");
		}
		if (pos + length > value.Length)
		{
			throw new ArgumentOutOfRangeException("value", "Position or length has wrong value.");
		}
		Buffer.BlockCopy(value, pos, arrBuffer, offset, length);
	}

	[CLSCompliant(false)]
	protected internal void SetBitInVar(ref ushort variable, bool value, int bitPos)
	{
		if (bitPos < 0 || bitPos > 15)
		{
			throw new ArgumentOutOfRangeException("bitPos", "Bit Position can be zero or greater than 7.");
		}
		if (value)
		{
			variable |= (ushort)(1 << bitPos);
		}
		else
		{
			variable &= (ushort)(~(1 << bitPos));
		}
	}

	[CLSCompliant(false)]
	protected internal void SetBitInVar(ref uint variable, bool value, int bitPos)
	{
		if (bitPos < 0 || bitPos > 31)
		{
			throw new ArgumentOutOfRangeException("bitPos", "Bit Position can be zero or greater than 7.");
		}
		if (value)
		{
			variable |= (uint)(1 << bitPos);
		}
		else
		{
			variable &= (uint)(~(1 << bitPos));
		}
	}

	public int Get16BitStringSize(string strValue, bool isCompressed)
	{
		if (strValue == null || strValue.Length == 0)
		{
			return 2;
		}
		Encoding encoding = (isCompressed ? Encoding.UTF8 : Encoding.Unicode);
		return 3 + encoding.GetByteCount(strValue);
	}

	public virtual void ClearData()
	{
	}

	public virtual bool IsEqual(BiffRecordRaw raw)
	{
		throw new NotImplementedException();
	}

	public virtual void CopyTo(BiffRecordRaw raw)
	{
		throw new NotImplementedException();
	}

	public void CheckTypeCode(TBIFFRecord typeCode)
	{
		if (TypeCode != typeCode)
		{
			throw new ArgumentOutOfRangeException(typeCode.ToString() + " record was expected");
		}
	}

	public static bool CompareArrays(byte[] array1, int iStartIndex1, byte[] array2, int iStartIndex2, int iLength)
	{
		int num = 0;
		int num2 = iStartIndex1;
		int num3 = iStartIndex2;
		while (num < iLength && num2 < array1.Length && num3 < array2.Length)
		{
			if (array1[num2] != array2[num3])
			{
				return false;
			}
			num++;
			num2++;
			num3++;
		}
		if (num == iLength && num != 0)
		{
			return true;
		}
		return false;
	}

	public static bool CompareArrays(byte[] array1, byte[] array2)
	{
		if (array1 == null && array2 == null)
		{
			return true;
		}
		if (array1 == null || array2 == null)
		{
			return false;
		}
		int num = array1.Length;
		int num2 = array2.Length;
		if (num != num2)
		{
			return false;
		}
		if (num == 0 && num2 == 0)
		{
			return true;
		}
		return CompareArrays(array1, 0, array2, 0, array1.Length);
	}

	internal void SetRecordCode(int code)
	{
		m_iCode = code;
	}

	public virtual object Clone()
	{
		return MemberwiseClone();
	}

	public static byte[] CombineArrays(int iCombinedLength, List<byte[]> arrCombined)
	{
		if (arrCombined == null || arrCombined.Count == 0)
		{
			return new byte[0];
		}
		int count = arrCombined.Count;
		byte[] array = new byte[iCombinedLength];
		int num = 0;
		for (int i = 0; i < count; i++)
		{
			byte[] array2 = arrCombined[i];
			int num2 = array2.Length;
			Buffer.BlockCopy(array2, 0, array, num, num2);
			num += num2;
		}
		return array;
	}

	public static string GetString(byte[] arrData, int iOffset, int iLength, out int iReadBytes)
	{
		if (arrData == null)
		{
			throw new ArgumentNullException("arrData");
		}
		if (iOffset < 0 || iOffset + iLength > arrData.Length)
		{
			throw new ArgumentOutOfRangeException("iOffset");
		}
		if (iLength < 0)
		{
			throw new ArgumentOutOfRangeException("iLength");
		}
		byte num = arrData[iOffset];
		if (((num != 0) ? (2 * iLength) : iLength) + (iOffset + 1) > arrData.Length)
		{
			throw new WrongBiffRecordDataException($"String and m_data array do not fit each other");
		}
		Encoding encoding;
		if (num == 0)
		{
			iReadBytes = iLength;
			encoding = LatinEncoding;
		}
		else
		{
			iReadBytes = iLength * 2;
			encoding = Encoding.Unicode;
		}
		string @string = encoding.GetString(arrData, iOffset + 1, iReadBytes);
		iReadBytes++;
		return @string;
	}

	public static int SetStringNoLen(byte[] arrData, int iOffset, string strValue)
	{
		if (strValue == null || strValue.Length == 0)
		{
			return 0;
		}
		if (arrData == null)
		{
			throw new ArgumentNullException("arrData");
		}
		byte[] bytes = Encoding.Unicode.GetBytes(strValue);
		if (iOffset < 0 || iOffset + bytes.Length + 1 > arrData.Length)
		{
			throw new ArgumentOutOfRangeException("iOffset");
		}
		arrData[iOffset] = 1;
		iOffset++;
		bytes.CopyTo(arrData, iOffset);
		return bytes.Length + 1;
	}

	public static void SetString16BitUpdateOffset(byte[] arrData, ref int offset, string value)
	{
		SetUInt16(arrData, offset, (ushort)value.Length);
		offset += 2;
		SetStringNoLenUpdateOffset(arrData, ref offset, value);
	}

	public static bool GetBitFromVar(byte btOptions, int bitPos)
	{
		if (bitPos < 0 || bitPos >= 8)
		{
			throw new ArgumentOutOfRangeException("bitPos", "Bit Position cannot be less than 0 or greater 7.");
		}
		return GetBitFromVar((int)btOptions, bitPos);
	}

	public static bool GetBitFromVar(short sOptions, int bitPos)
	{
		switch (bitPos)
		{
		default:
			throw new ArgumentOutOfRangeException("bitPos", "Bit Position cannot be less than 0 or greater 15.");
		case 15:
			return sOptions < 0;
		case 0:
		case 1:
		case 2:
		case 3:
		case 4:
		case 5:
		case 6:
		case 7:
		case 8:
		case 9:
		case 10:
		case 11:
		case 12:
		case 13:
		case 14:
			return GetBitFromVar((int)sOptions, bitPos);
		}
	}

	[CLSCompliant(false)]
	public static bool GetBitFromVar(ushort usOptions, int bitPos)
	{
		if (bitPos < 0 || bitPos >= 16)
		{
			throw new ArgumentOutOfRangeException("bitPos", "Bit Position cannot be less than 0 or greater 15.");
		}
		return GetBitFromVar((int)usOptions, bitPos);
	}

	public static bool GetBitFromVar(int iOptions, int bitPos)
	{
		switch (bitPos)
		{
		default:
			throw new ArgumentOutOfRangeException("bitPos", "Bit Position cannot be less than 0 or greater 31.");
		case 31:
			return iOptions < 0;
		case 0:
		case 1:
		case 2:
		case 3:
		case 4:
		case 5:
		case 6:
		case 7:
		case 8:
		case 9:
		case 10:
		case 11:
		case 12:
		case 13:
		case 14:
		case 15:
		case 16:
		case 17:
		case 18:
		case 19:
		case 20:
		case 21:
		case 22:
		case 23:
		case 24:
		case 25:
		case 26:
		case 27:
		case 28:
		case 29:
		case 30:
			return (iOptions & (1 << bitPos)) != 0;
		}
	}

	[CLSCompliant(false)]
	public static bool GetBitFromVar(uint uiOptions, int bitPos)
	{
		if (bitPos < 0 || bitPos >= 32)
		{
			throw new ArgumentOutOfRangeException("bitPos", "Bit Position cannot be less than 0 or greater 31.");
		}
		return (uiOptions & (1 << bitPos)) != 0;
	}

	public static int SetBit(int iValue, int bitPos, bool value)
	{
		switch (bitPos)
		{
		default:
			throw new ArgumentOutOfRangeException("bitPos", "Bit Position cannot be less than 0 or greater 32.");
		case 31:
			iValue = Math.Abs(iValue);
			if (!value)
			{
				iValue = -iValue;
			}
			break;
		case 0:
		case 1:
		case 2:
		case 3:
		case 4:
		case 5:
		case 6:
		case 7:
		case 8:
		case 9:
		case 10:
		case 11:
		case 12:
		case 13:
		case 14:
		case 15:
		case 16:
		case 17:
		case 18:
		case 19:
		case 20:
		case 21:
		case 22:
		case 23:
		case 24:
		case 25:
		case 26:
		case 27:
		case 28:
		case 29:
		case 30:
			iValue = ((!value) ? (iValue & ~(1 << bitPos)) : (iValue | (1 << bitPos)));
			break;
		}
		return iValue;
	}

	public static int ReadArray(byte[] arrSource, int iOffset, byte[] arrDest)
	{
		if (arrSource == null)
		{
			throw new ArgumentNullException("arrSource");
		}
		if (arrDest == null)
		{
			throw new ArgumentNullException("arrDest");
		}
		int num = arrDest.Length;
		Buffer.BlockCopy(arrSource, iOffset, arrDest, 0, num);
		return iOffset + num;
	}
}
