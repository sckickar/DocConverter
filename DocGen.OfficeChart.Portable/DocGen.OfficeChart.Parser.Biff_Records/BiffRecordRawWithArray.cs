using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using DocGen.Drawing;
using DocGen.OfficeChart.Implementation;
using DocGen.OfficeChart.Implementation.Exceptions;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[CLSCompliant(false)]
internal abstract class BiffRecordRawWithArray : BiffRecordRaw, IDisposable
{
	protected internal byte[] m_data;

	private bool m_bAutoGrow;

	public override byte[] Data
	{
		get
		{
			if (base.NeedInfill)
			{
				InfillInternalData(OfficeVersion.Excel97to2003);
			}
			return m_data;
		}
		set
		{
			if (value != null)
			{
				m_data = value;
			}
		}
	}

	public override bool AutoGrowData
	{
		get
		{
			return m_bAutoGrow;
		}
		set
		{
			m_bAutoGrow = value;
		}
	}

	protected BiffRecordRawWithArray()
	{
		if (NeedDataArray)
		{
			m_data = new byte[0];
		}
	}

	protected BiffRecordRawWithArray(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	protected BiffRecordRawWithArray(int iReserve)
		: base(iReserve)
	{
		if (iReserve < 0)
		{
			throw new ArgumentOutOfRangeException("iReserve", "Reserved memory count must be greater than zero.");
		}
		m_data = new byte[iReserve];
	}

	~BiffRecordRawWithArray()
	{
		Dispose();
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_data = new byte[iLength];
		m_iLength = iLength;
		provider.CopyTo(iOffset, m_data, 0, iLength);
		ParseStructure();
		if (!NeedDataArray)
		{
			m_data = new byte[0];
			AutoGrowData = true;
		}
	}

	public abstract void InfillInternalData(OfficeVersion version);

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		InfillInternalData(OfficeVersion.Excel97to2003);
		if (m_iLength > 0)
		{
			provider.WriteBytes(iOffset, m_data, 0, m_iLength);
		}
	}

	protected void CheckOffsetAndLength(int offset, int length)
	{
		int num = m_data.Length;
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

	protected byte[] GetBytes(int offset, int length)
	{
		if (length == 0)
		{
			return new byte[0];
		}
		CheckOffsetAndLength(offset, length);
		byte[] array = new byte[length];
		Buffer.BlockCopy(m_data, offset, array, 0, length);
		return array;
	}

	protected byte GetByte(int offset)
	{
		CheckOffsetAndLength(offset, 1);
		return m_data[offset];
	}

	protected ushort GetUInt16(int offset)
	{
		CheckOffsetAndLength(offset, 2);
		return BitConverter.ToUInt16(m_data, offset);
	}

	protected short GetInt16(int offset)
	{
		CheckOffsetAndLength(offset, 2);
		return BitConverter.ToInt16(m_data, offset);
	}

	protected int GetInt32(int offset)
	{
		CheckOffsetAndLength(offset, 4);
		return BitConverter.ToInt32(m_data, offset);
	}

	protected uint GetUInt32(int offset)
	{
		CheckOffsetAndLength(offset, 4);
		return BitConverter.ToUInt32(m_data, offset);
	}

	protected long GetInt64(int offset)
	{
		CheckOffsetAndLength(offset, 28);
		return BitConverter.ToInt64(m_data, offset);
	}

	protected ulong GetUInt64(int offset)
	{
		CheckOffsetAndLength(offset, 8);
		return BitConverter.ToUInt64(m_data, offset);
	}

	protected float GetFloat(int offset)
	{
		CheckOffsetAndLength(offset, 4);
		return BitConverter.ToSingle(m_data, offset);
	}

	protected double GetDouble(int offset)
	{
		CheckOffsetAndLength(offset, 8);
		return BitConverter.ToDouble(m_data, offset);
	}

	protected bool GetBit(int offset, int bitPos)
	{
		if (bitPos < 0 || bitPos > 7)
		{
			throw new ArgumentOutOfRangeException("bitPos", "Bit Position can be zeroless or greater than 7.");
		}
		CheckOffsetAndLength(offset, 1);
		return (m_data[offset] & (1 << bitPos)) != 0;
	}

	protected string GetString16BitUpdateOffset(ref int offset, out bool asciiString)
	{
		int uInt = GetUInt16(offset);
		offset += 2;
		asciiString = false;
		if (uInt > 0)
		{
			int iBytesInString;
			string @string = GetString(offset, uInt, out iBytesInString);
			offset += iBytesInString + 1;
			asciiString = iBytesInString == uInt;
			return @string;
		}
		offset++;
		return string.Empty;
	}

	protected string GetString16BitUpdateOffset(ref int offset)
	{
		bool asciiString;
		return GetString16BitUpdateOffset(ref offset, out asciiString);
	}

	protected string GetStringUpdateOffset(ref int offset, int iStrLen)
	{
		if (iStrLen > 0)
		{
			int iBytesInString;
			string @string = GetString(offset, iStrLen, out iBytesInString);
			offset += iBytesInString + 1;
			return @string;
		}
		return string.Empty;
	}

	protected string GetStringByteLen(int offset)
	{
		int iStrLen = m_data[offset];
		return GetString(offset + 1, iStrLen);
	}

	protected string GetStringByteLen(int offset, out int iBytes)
	{
		int iStrLen = m_data[offset];
		return GetString(offset + 1, iStrLen, out iBytes);
	}

	protected internal string GetString(int offset, int iStrLen)
	{
		int iBytesInString;
		return GetString(offset, iStrLen, out iBytesInString);
	}

	protected internal string GetString(int offset, int iStrLen, out int iBytesInString)
	{
		return GetString(offset, iStrLen, out iBytesInString, isByteCounted: false);
	}

	protected internal string GetString(int offset, int iStrLen, out int iBytesInString, bool isByteCounted)
	{
		byte num = m_data[offset];
		if (((num != 0 && !isByteCounted) ? (2 * iStrLen) : iStrLen) + (offset + 1) > m_iLength)
		{
			throw new WrongBiffRecordDataException($"String and m_data array do not fit each other {base.TypeCode}.");
		}
		if (num == 0)
		{
			iBytesInString = iStrLen;
			CheckOffsetAndLength(offset + 1, iStrLen);
			return BiffRecordRaw.LatinEncoding.GetString(m_data, offset + 1, iStrLen);
		}
		iBytesInString = (isByteCounted ? iStrLen : (iStrLen * 2));
		CheckOffsetAndLength(offset + 1, iBytesInString);
		return Encoding.Unicode.GetString(m_data, offset + 1, iBytesInString);
	}

	protected string GetUnkTypeString(int offset, IList<int> continuePos, int continueCount, ref int iBreakIndex, out int length, out byte[] rich, out byte[] extended)
	{
		string text = null;
		int num = 3;
		rich = null;
		extended = null;
		ushort num2 = BitConverter.ToUInt16(m_data, offset);
		byte num3 = m_data[offset + 2];
		bool flag = (num3 & 1) == 1;
		bool flag2 = (num3 & 4) != 0;
		bool flag3 = (num3 & 8) != 0;
		int num4 = 3;
		short num5 = 0;
		int num6 = 0;
		if (flag3)
		{
			num5 = GetInt16(offset + num4);
			num4 += 2;
			num += 2;
		}
		if (flag2)
		{
			num6 = GetInt32(offset + num4);
			num4 += 4;
			num += 4;
		}
		int num7 = offset + num4;
		int num8 = 0;
		Encoding encoding = (flag ? Encoding.Unicode : BiffRecordRaw.LatinEncoding);
		while (num8 < num2)
		{
			int num9 = (flag ? ((num2 - num8) * 2) : (num2 - num8));
			int num10 = BiffRecordRaw.FindNextBreak(continuePos, continueCount, num7, ref iBreakIndex) - num7;
			if (num9 <= num10)
			{
				string @string = encoding.GetString(m_data, num7, num9);
				text = ((text == null) ? @string : (text + @string));
				num += num9;
				break;
			}
			string string2 = encoding.GetString(m_data, num7, num10);
			text = ((text == null) ? string2 : (text + string2));
			num8 += (flag ? (num10 / 2) : num10);
			if (m_data[num7 + num10] == 0 || m_data[num7 + num10] == 1)
			{
				flag = m_data[num7 + num10] == 1;
				encoding = (flag ? Encoding.Unicode : BiffRecordRaw.LatinEncoding);
				num7++;
				num++;
			}
			num7 += num10;
			num += num10;
		}
		if (flag3)
		{
			int num11 = num5 * 4;
			rich = GetBytes(offset + num, num11);
			num += num11;
		}
		if (flag2)
		{
			extended = GetBytes(offset + num, num6);
			num += num6;
		}
		length = num;
		if (text == null)
		{
			return string.Empty;
		}
		return text;
	}

	protected TAddr GetAddr(int offset)
	{
		TAddr result = default(TAddr);
		result.FirstRow = GetUInt16(offset);
		result.LastRow = GetUInt16(offset + 2);
		result.FirstCol = GetUInt16(offset + 4);
		result.LastCol = GetUInt16(offset + 6);
		return result;
	}

	protected Rectangle GetAddrAsRectangle(int offset)
	{
		int uInt = GetUInt16(offset);
		int uInt2 = GetUInt16(offset + 2);
		ushort uInt3 = GetUInt16(offset + 4);
		int uInt4 = GetUInt16(offset + 6);
		return Rectangle.FromLTRB(uInt3, uInt, uInt4, uInt2);
	}

	protected void EnlargeDataStorageIfNeeded(int offset, int length)
	{
		if (m_data != null && offset + length <= m_data.Length)
		{
			return;
		}
		int num = ((m_data != null) ? m_data.Length : 0);
		int num2 = Math.Min(offset * 2 + length + 16, MaximumMemorySize);
		if (num2 > num)
		{
			byte[] array = new byte[num2];
			if (num > 0 && m_data != null)
			{
				Buffer.BlockCopy(m_data, 0, array, 0, num);
			}
			m_data = array;
		}
	}

	protected internal void Reserve(int length)
	{
		if (m_data.Length <= length)
		{
			byte[] array = new byte[length];
			Buffer.BlockCopy(m_data, 0, array, 0, m_data.Length);
			m_data = array;
		}
	}

	protected internal void SetBytes(int offset, byte[] value, int pos, int length)
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
		if (AutoGrowData)
		{
			EnlargeDataStorageIfNeeded(offset, length);
		}
		else if (offset + length > m_data.Length)
		{
			throw new ArgumentOutOfRangeException("m_data", "Internal array size is too small.");
		}
		Buffer.BlockCopy(value, pos, m_data, offset, length);
	}

	protected internal void SetBytes(int offset, byte[] value)
	{
		SetBytes(offset, value, 0, value.Length);
	}

	protected internal void SetByte(int offset, byte value)
	{
		if (AutoGrowData)
		{
			EnlargeDataStorageIfNeeded(offset, 1);
		}
		m_data[offset] = value;
	}

	protected internal void SetByte(int offset, byte value, int count)
	{
		byte[] array = new byte[count];
		for (int i = 0; i < count; i++)
		{
			array[i] = value;
		}
		SetBytes(offset, array, 0, count);
	}

	protected internal void SetUInt16(int offset, ushort value)
	{
		if (AutoGrowData)
		{
			EnlargeDataStorageIfNeeded(offset, 2);
		}
		byte b = (byte)(value & 0xFFu);
		byte b2 = (byte)((uint)(value >> 8) & 0xFFu);
		m_data[offset] = b;
		m_data[offset + 1] = b2;
	}

	protected internal void SetInt16(int offset, short value)
	{
		SetBytes(offset, BitConverter.GetBytes(value), 0, 2);
	}

	protected internal void SetInt32(int offset, int value)
	{
		if (AutoGrowData)
		{
			EnlargeDataStorageIfNeeded(offset, 4);
		}
		Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_data, offset, 4);
	}

	protected internal void SetUInt32(int offset, uint value)
	{
		SetBytes(offset, BitConverter.GetBytes(value), 0, 4);
	}

	protected internal void SetInt64(int offset, long value)
	{
		SetBytes(offset, BitConverter.GetBytes(value), 0, 8);
	}

	protected internal void SetUInt64(int offset, ulong value)
	{
		SetBytes(offset, BitConverter.GetBytes(value), 0, 8);
	}

	protected internal void SetFloat(int offset, float value)
	{
		SetBytes(offset, BitConverter.GetBytes(value), 0, 4);
	}

	protected internal void SetDouble(int offset, double value)
	{
		SetBytes(offset, BitConverter.GetBytes(value), 0, 8);
	}

	protected internal void SetBit(int offset, bool value, int bitPos)
	{
		if (bitPos < 0 || bitPos > 7)
		{
			throw new ArgumentOutOfRangeException("bitPos", "Bit Position can be zero or greater than 7.");
		}
		if (AutoGrowData)
		{
			EnlargeDataStorageIfNeeded(offset, 1);
		}
		if (value)
		{
			m_data[offset] |= (byte)(1 << bitPos);
		}
		else
		{
			m_data[offset] &= (byte)(~(1 << bitPos));
		}
	}

	protected internal void SetStringNoLenUpdateOffset(ref int offset, string value, bool isCompression)
	{
		if (value != null && value.Length != 0)
		{
			Encoding uTF = Encoding.UTF8;
			uTF = (isCompression ? uTF : Encoding.Unicode);
			byte[] bytes = uTF.GetBytes(value);
			byte value2 = ((!isCompression) ? ((byte)1) : ((byte)0));
			SetByte(offset, value2);
			SetBytes(offset + 1, bytes, 0, bytes.Length);
			offset += bytes.Length + 1;
		}
	}

	protected internal int SetStringNoLenDetectEncoding(int offset, string value)
	{
		if (IsAsciiString(value))
		{
			return SetStringNoLen(offset, value, bEmptyCompressed: true, bCompressed: true);
		}
		return SetStringNoLen(offset, value);
	}

	public static bool IsAsciiString(string strTextPart)
	{
		bool result = true;
		int num = strTextPart?.Length ?? 0;
		for (int i = 0; i < num; i++)
		{
			if (strTextPart[i] > '\u007f')
			{
				result = false;
				break;
			}
		}
		return result;
	}

	protected internal int SetStringNoLen(int offset, string value)
	{
		return SetStringNoLen(offset, value, bEmptyCompressed: false, bCompressed: false);
	}

	protected internal int SetStringNoLen(int offset, string value, bool bEmptyCompressed, bool bCompressed)
	{
		if (value == null || value.Length == 0)
		{
			if (bEmptyCompressed)
			{
				SetByte(offset, 0);
				return 1;
			}
			return 0;
		}
		byte[] bytes = (bCompressed ? Encoding.UTF8 : Encoding.Unicode).GetBytes(value);
		byte value2 = ((!bCompressed) ? ((byte)1) : ((byte)0));
		SetByte(offset, value2);
		SetBytes(offset + 1, bytes, 0, bytes.Length);
		return bytes.Length + 1;
	}

	protected internal int SetStringByteLen(int offset, string value)
	{
		SetByte(offset, (byte)value.Length);
		return SetStringNoLen(offset + 1, value) + 1;
	}

	protected internal int SetString16BitLen(int offset, string value)
	{
		ushort value2 = (ushort)(value?.Length ?? 0);
		SetUInt16(offset, value2);
		return 2 + SetStringNoLen(offset + 2, value);
	}

	protected internal int SetString16BitLen(int offset, string value, bool bEmptyCompressed, bool isCompressed)
	{
		SetUInt16(offset, (ushort)value.Length);
		return 2 + SetStringNoLen(offset + 2, value, bEmptyCompressed, isCompressed);
	}

	protected internal void SetString16BitUpdateOffset(ref int offset, string value)
	{
		SetUInt16(offset, (ushort)value.Length);
		offset += 2;
		SetStringNoLenUpdateOffset(ref offset, value, isCompression: false);
	}

	protected internal void SetString16BitUpdateOffset(ref int offset, string value, bool isCompressed)
	{
		SetUInt16(offset, (ushort)value.Length);
		offset += 2;
		SetStringNoLenUpdateOffset(ref offset, value, isCompressed);
	}

	protected internal void SetAddr(int offset, TAddr addr)
	{
		SetUInt16(offset, (ushort)addr.FirstRow);
		SetUInt16(offset + 2, (ushort)addr.LastRow);
		SetUInt16(offset + 4, (ushort)addr.FirstCol);
		SetUInt16(offset + 6, (ushort)addr.LastCol);
	}

	protected internal void SetAddr(int offset, Rectangle addr)
	{
		SetUInt16(offset, (ushort)addr.Top);
		SetUInt16(offset + 2, (ushort)addr.Bottom);
		SetUInt16(offset + 4, (ushort)addr.Left);
		SetUInt16(offset + 6, (ushort)addr.Right);
	}

	protected SortedList<BiffRecordPosAttribute, FieldInfo> GetSortedFields()
	{
		if (!BiffRecordRaw.m_ReflectCache.TryGetValue(m_iCode, out var value))
		{
			Type type = GetType();
			FieldInfo[] array = null;
			array = type.GetRuntimeFields().ToArray();
			value = new SortedList<BiffRecordPosAttribute, FieldInfo>(new RecordsPosComparer());
			int i = 0;
			for (int num = array.Length; i < num; i++)
			{
				FieldInfo fieldInfo = array[i];
				object[] array2 = fieldInfo.GetCustomAttributes(typeof(BiffRecordPosAttribute), inherit: true).ToArray();
				if (array2.Length != 0)
				{
					value.Add((BiffRecordPosAttribute)array2[0], fieldInfo);
				}
			}
			BiffRecordRaw.m_ReflectCache[m_iCode] = value;
		}
		return value;
	}

	protected void AutoExtractFields()
	{
		SortedList<BiffRecordPosAttribute, FieldInfo> sortedFields = GetSortedFields();
		IList<BiffRecordPosAttribute> keys = sortedFields.Keys;
		IList<FieldInfo> values = sortedFields.Values;
		int i = 0;
		for (int count = sortedFields.Count; i < count; i++)
		{
			BiffRecordPosAttribute attr = keys[i];
			values[i].SetValue(this, GetValueByAttributeType(attr));
		}
	}

	protected object GetValueByAttributeType(BiffRecordPosAttribute attr)
	{
		int position = attr.Position;
		if (attr.IsBit)
		{
			return GetBit(attr.Position, attr.SizeOrBitPosition);
		}
		if (attr.IsString)
		{
			byte iStrLen = m_data[attr.Position];
			return GetString(attr.Position + 1, iStrLen);
		}
		if (attr.IsString16Bit)
		{
			ushort uInt = GetUInt16(attr.Position);
			if (uInt <= 0)
			{
				return string.Empty;
			}
			return GetString(attr.Position + 2, uInt);
		}
		if (attr.IsOEMString)
		{
			byte b = m_data[attr.Position];
			if (attr.Position + b > m_data.Length)
			{
				throw new WrongBiffRecordDataException("Wrong Record data: string is too long.");
			}
			if (b != 0)
			{
				return BiffRecordRaw.LatinEncoding.GetString(GetBytes(attr.Position + 1, b), 0, b);
			}
			return "";
		}
		if (attr.IsOEMString16Bit)
		{
			ushort uInt2 = GetUInt16(attr.Position);
			if (attr.Position + uInt2 + 2 > m_data.Length)
			{
				throw new WrongBiffRecordDataException("Wrong Record data: string is too long.");
			}
			if (uInt2 != 0)
			{
				return BiffRecordRaw.LatinEncoding.GetString(GetBytes(attr.Position + 2, uInt2), 0, uInt2);
			}
			return "";
		}
		switch (attr.SizeOrBitPosition)
		{
		case 1:
			return GetByte(position);
		case 2:
			if (attr.IsSigned)
			{
				return GetInt16(position);
			}
			return GetUInt16(position);
		case 4:
			if (attr.IsFloat)
			{
				return GetFloat(position);
			}
			if (attr.IsSigned)
			{
				return GetInt32(position);
			}
			return GetUInt32(position);
		case 8:
			if (attr.IsFloat)
			{
				return GetDouble(position);
			}
			if (attr.IsSigned)
			{
				return GetInt64(position);
			}
			return GetUInt64(position);
		default:
			throw new ApplicationException("AutoReader - Unknown size of item field. Record." + base.TypeCode.ToString() + ". Code " + base.RecordCode);
		}
	}

	protected int AutoInfillFromFields()
	{
		SortedList<BiffRecordPosAttribute, FieldInfo> sortedFields = GetSortedFields();
		bool autoGrowData = AutoGrowData;
		AutoGrowData = true;
		int num = 0;
		int num2 = 0;
		IList<BiffRecordPosAttribute> keys = sortedFields.Keys;
		IList<FieldInfo> values = sortedFields.Values;
		int i = 0;
		for (int count = sortedFields.Count; i < count; i++)
		{
			BiffRecordPosAttribute biffRecordPosAttribute = keys[i];
			object value = values[i].GetValue(this);
			num = SetValueByAttributeType(biffRecordPosAttribute, value);
			num2 = Math.Max(num2, biffRecordPosAttribute.Position + num);
		}
		AutoGrowData = autoGrowData;
		return num2;
	}

	protected int SetValueByAttributeType(BiffRecordPosAttribute attr, object data)
	{
		int result = 0;
		int position = attr.Position;
		if (attr.IsOEMString)
		{
			byte[] bytes = BiffRecordRaw.LatinEncoding.GetBytes((string)data);
			SetByte(position, (byte)bytes.Length);
			SetBytes(position + 1, bytes, 0, bytes.Length);
			result = 1 + bytes.Length;
		}
		if (attr.IsOEMString16Bit)
		{
			byte[] bytes2 = BiffRecordRaw.LatinEncoding.GetBytes((string)data);
			SetUInt16(position, (ushort)bytes2.Length);
			SetBytes(position + 2, bytes2, 0, bytes2.Length);
			result = 2 + bytes2.Length;
		}
		else if (attr.IsString)
		{
			string text = (string)data;
			byte[] bytes3 = Encoding.Unicode.GetBytes(text);
			SetByte(position, (byte)text.Length);
			SetByte(position + 1, 1);
			SetBytes(position + 2, bytes3, 0, bytes3.Length);
			result = 2 + bytes3.Length;
		}
		else if (attr.IsString16Bit)
		{
			string text2 = (string)data;
			int num = text2?.Length ?? 0;
			SetUInt16(position, (ushort)num);
			result = 2;
			if (num > 0 && text2 != null)
			{
				byte[] bytes4 = Encoding.Unicode.GetBytes(text2);
				SetByte(position + 2, 1);
				SetBytes(position + 3, bytes4, 0, bytes4.Length);
				result = 3 + bytes4.Length;
			}
		}
		else if (attr.IsBit)
		{
			SetBit(position, (bool)data, attr.SizeOrBitPosition);
		}
		else
		{
			switch (attr.SizeOrBitPosition)
			{
			case 1:
				SetByte(position, (byte)data);
				break;
			case 2:
				if (attr.IsSigned)
				{
					SetInt16(position, (short)data);
				}
				else
				{
					SetUInt16(position, (ushort)data);
				}
				break;
			case 4:
				if (attr.IsFloat)
				{
					SetFloat(position, (float)data);
				}
				else if (attr.IsSigned)
				{
					SetInt32(position, (int)data);
				}
				else
				{
					SetUInt32(position, (uint)data);
				}
				break;
			case 8:
				if (attr.IsFloat)
				{
					SetDouble(position, (double)data);
				}
				else if (attr.IsSigned)
				{
					SetInt64(position, (long)data);
				}
				else
				{
					SetUInt64(position, (ulong)data);
				}
				break;
			}
			result = attr.SizeOrBitPosition;
		}
		return result;
	}

	public override void ClearData()
	{
		m_data = new byte[0];
	}

	public override bool IsEqual(BiffRecordRaw raw)
	{
		if (raw is BiffRecordRawWithArray biffRecordRawWithArray)
		{
			InfillInternalData(OfficeVersion.Excel2007);
			biffRecordRawWithArray.InfillInternalData(OfficeVersion.Excel2007);
			if (m_iLength == biffRecordRawWithArray.m_iLength)
			{
				for (int i = 0; i < m_iLength; i++)
				{
					if (m_data[i] != biffRecordRawWithArray.m_data[i])
					{
						return false;
					}
				}
			}
			return true;
		}
		return false;
	}

	public override void CopyTo(BiffRecordRaw raw)
	{
		if (base.RecordCode != raw.RecordCode)
		{
			throw new ArgumentException("Records should have same type for copy.");
		}
		BiffRecordRawWithArray biffRecordRawWithArray = raw as BiffRecordRawWithArray;
		InfillInternalData(OfficeVersion.Excel2007);
		biffRecordRawWithArray.m_data = new byte[base.Length];
		Array.Copy(m_data, 0, biffRecordRawWithArray.m_data, 0, base.Length);
		biffRecordRawWithArray.m_iLength = m_iLength;
		biffRecordRawWithArray.ParseStructure();
	}

	protected internal void SetInternalData(byte[] arrData)
	{
		SetInternalData(arrData, bNeedInfill: true);
	}

	protected void SetInternalData(byte[] arrData, bool bNeedInfill)
	{
		m_data = arrData;
		base.NeedInfill = bNeedInfill;
	}

	public abstract void ParseStructure();

	public override int GetStoreSize(OfficeVersion version)
	{
		int minimumRecordSize = MinimumRecordSize;
		if (minimumRecordSize == MaximumRecordSize)
		{
			return minimumRecordSize;
		}
		if (base.NeedInfill)
		{
			InfillInternalData(version);
			base.NeedInfill = false;
		}
		return m_iLength;
	}

	public virtual void Dispose()
	{
		OnDispose();
		m_data = null;
		GC.SuppressFinalize(this);
	}

	protected virtual void OnDispose()
	{
	}
}
