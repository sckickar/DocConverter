using System;
using System.Collections.Generic;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.CRN)]
[CLSCompliant(false)]
internal class CRNRecord : BiffRecordRaw, ICloneable
{
	private enum CellValueType
	{
		Nil = 0,
		Number = 1,
		String = 2,
		Boolean = 4,
		Error = 0x10
	}

	private const int DEF_VALUES_OFFSET = 4;

	private const string DEF_ERROR_MESSAGE = "Unknown data type";

	private static readonly byte[] DEF_RESERVED_BYTES = new byte[7];

	private const int DefaultSize = 8;

	[BiffRecordPos(0, 1)]
	private byte m_btLastCol;

	[BiffRecordPos(1, 1)]
	private byte m_btFirstCol;

	[BiffRecordPos(2, 2)]
	private ushort m_usRow;

	private List<object> m_arrValues = new List<object>();

	public override bool NeedDataArray => true;

	public byte LastColumn
	{
		get
		{
			return m_btLastCol;
		}
		set
		{
			m_btLastCol = value;
		}
	}

	public byte FirstColumn
	{
		get
		{
			return m_btFirstCol;
		}
		set
		{
			m_btFirstCol = value;
		}
	}

	public ushort Row
	{
		get
		{
			return m_usRow;
		}
		set
		{
			m_usRow = value;
		}
	}

	public override int MinimumRecordSize => 4;

	public List<object> Values => m_arrValues;

	public CRNRecord()
	{
	}

	public CRNRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public CRNRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_btLastCol = provider.ReadByte(iOffset);
		m_btFirstCol = provider.ReadByte(iOffset + 1);
		m_usRow = provider.ReadUInt16(iOffset + 2);
		int num = iOffset;
		iOffset += 4;
		m_arrValues.Clear();
		while (iOffset - num < iLength)
		{
			object value = GetValue(provider, ref iOffset);
			m_arrValues.Add(value);
		}
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_iLength = GetStoreSize(version);
		provider.WriteByte(iOffset, m_btLastCol);
		provider.WriteByte(iOffset + 1, m_btFirstCol);
		provider.WriteUInt16(iOffset + 2, m_usRow);
		iOffset += 4;
		int i = 0;
		for (int count = m_arrValues.Count; i < count; i++)
		{
			iOffset = SetValue(provider, iOffset, m_arrValues[i]);
		}
	}

	private object GetValue(DataProvider provider, ref int iOffset)
	{
		object result = null;
		byte num = provider.ReadByte(iOffset);
		iOffset++;
		switch ((CellValueType)num)
		{
		case CellValueType.Number:
			result = provider.ReadDouble(iOffset);
			iOffset += 8;
			break;
		case CellValueType.String:
			result = provider.ReadString16BitUpdateOffset(ref iOffset);
			break;
		case CellValueType.Boolean:
			result = provider.ReadBoolean(iOffset);
			iOffset += 8;
			break;
		case CellValueType.Error:
			result = provider.ReadByte(iOffset);
			iOffset += 8;
			break;
		case CellValueType.Nil:
			iOffset += 8;
			break;
		default:
			throw new ApplicationException("Unknown data type");
		}
		return result;
	}

	private int SetValue(DataProvider provider, int iOffset, object value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (value is double)
		{
			provider.WriteByte(iOffset, 1);
			iOffset++;
			provider.WriteDouble(iOffset, (double)value);
			iOffset += 8;
		}
		else if (value is string)
		{
			provider.WriteByte(iOffset, 2);
			iOffset++;
			string text = value as string;
			provider.WriteString16BitUpdateOffset(ref iOffset, text);
			if (text.Length == 0)
			{
				provider.WriteByte(iOffset++, 0);
			}
		}
		else if (value is bool)
		{
			provider.WriteByte(iOffset, 4);
			iOffset++;
			provider.WriteByte(iOffset++, ((bool)value) ? ((byte)1) : ((byte)0));
			provider.WriteBytes(iOffset, DEF_RESERVED_BYTES);
			iOffset += DEF_RESERVED_BYTES.Length;
		}
		else
		{
			if (!(value is byte))
			{
				throw new ArgumentOutOfRangeException("Wrong data type");
			}
			provider.WriteByte(iOffset++, 16);
			provider.WriteByte(iOffset++, (byte)value);
			provider.WriteBytes(iOffset, DEF_RESERVED_BYTES);
			iOffset += DEF_RESERVED_BYTES.Length;
		}
		return iOffset;
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		int num = 4;
		int i = 0;
		for (int count = m_arrValues.Count; i < count; i++)
		{
			num = ((!(m_arrValues[i] is string text)) ? (num + 9) : (num + (4 + text.Length * 2)));
		}
		return num;
	}

	public override object Clone()
	{
		CRNRecord obj = (CRNRecord)base.Clone();
		obj.m_arrValues = new List<object>(m_arrValues);
		return obj;
	}
}
