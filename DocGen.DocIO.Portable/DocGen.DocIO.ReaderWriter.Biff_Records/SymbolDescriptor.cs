using System;

namespace DocGen.DocIO.ReaderWriter.Biff_Records;

internal class SymbolDescriptor
{
	internal const int DEF_STRUCT_SIZE = 4;

	internal const int DEF_EXT_VALUE = 240;

	private short m_fontCode;

	private byte m_charSpecifier;

	private byte m_charSpecifierExt = 240;

	internal short FontCode
	{
		get
		{
			return m_fontCode;
		}
		set
		{
			m_fontCode = value;
		}
	}

	internal byte CharCode
	{
		get
		{
			return m_charSpecifier;
		}
		set
		{
			m_charSpecifier = value;
		}
	}

	internal byte CharCodeExt
	{
		get
		{
			return m_charSpecifierExt;
		}
		set
		{
			m_charSpecifierExt = value;
		}
	}

	internal SymbolDescriptor()
	{
	}

	internal void Parse(byte[] operand)
	{
		m_fontCode = BitConverter.ToInt16(operand, 0);
		m_charSpecifier = operand[2];
		m_charSpecifierExt = operand[3];
	}

	internal byte[] Save()
	{
		byte[] array = new byte[4];
		BitConverter.GetBytes(m_fontCode).CopyTo(array, 0);
		array[2] = m_charSpecifier;
		array[3] = m_charSpecifierExt;
		return array;
	}
}
