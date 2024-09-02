using System;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[AttributeUsage(AttributeTargets.Field)]
internal sealed class BiffRecordPosAttribute : Attribute, IComparable
{
	private int m_iPos;

	private int m_iSize;

	private bool m_bIsBit;

	private bool m_bIsString;

	private bool m_bIsString16Bit;

	private bool m_bIsOEMString;

	private bool m_bIsOEMString16Bit;

	private bool m_bIsFloat;

	private bool m_bSigned;

	public int Position => m_iPos;

	public int SizeOrBitPosition => m_iSize;

	public bool IsBit => m_bIsBit;

	public bool IsSigned => m_bSigned;

	public bool IsString => m_bIsString;

	public bool IsString16Bit => m_bIsString16Bit;

	public bool IsFloat => m_bIsFloat;

	public bool IsOEMString => m_bIsOEMString;

	public bool IsOEMString16Bit => m_bIsOEMString16Bit;

	public BiffRecordPosAttribute(int pos, int size, bool isSigned, TFieldType type)
	{
		m_iPos = pos;
		m_iSize = size;
		m_bSigned = isSigned;
		m_bIsBit = type == TFieldType.Bit;
		m_bIsString = type == TFieldType.String;
		m_bIsString16Bit = type == TFieldType.String16Bit;
		m_bIsOEMString = type == TFieldType.OEMString;
		m_bIsOEMString16Bit = type == TFieldType.OEMString16Bit;
		m_bIsFloat = type == TFieldType.Float;
	}

	public BiffRecordPosAttribute(int pos, int size, bool isSigned)
		: this(pos, size, isSigned, TFieldType.Integer)
	{
	}

	public BiffRecordPosAttribute(int pos, int size, TFieldType type)
		: this(pos, size, isSigned: false, type)
	{
	}

	public BiffRecordPosAttribute(int pos, TFieldType type)
		: this(pos, 0, isSigned: false, type)
	{
	}

	public BiffRecordPosAttribute(int pos, int size)
		: this(pos, size, isSigned: false)
	{
	}

	public int CompareTo(object obj)
	{
		RecordsPosComparer recordsPosComparer = new RecordsPosComparer();
		BiffRecordPosAttribute y = obj as BiffRecordPosAttribute;
		return recordsPosComparer.Compare(this, y);
	}
}
