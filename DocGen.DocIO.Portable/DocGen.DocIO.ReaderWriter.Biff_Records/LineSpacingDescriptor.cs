using System;

namespace DocGen.DocIO.ReaderWriter.Biff_Records;

internal class LineSpacingDescriptor
{
	private short m_dyaLine;

	private bool m_fMultLinespace;

	internal short LineSpacing
	{
		get
		{
			return m_dyaLine;
		}
		set
		{
			switch (LineSpacingRule)
			{
			case LineSpacingRule.AtLeast:
				m_dyaLine = value;
				break;
			case LineSpacingRule.Exactly:
				m_dyaLine = (short)(-value);
				break;
			default:
				throw new Exception("Trying to set unsupported line spacing rule.");
			}
		}
	}

	internal LineSpacingRule LineSpacingRule
	{
		get
		{
			if (m_fMultLinespace)
			{
				if (m_dyaLine <= 0)
				{
					return LineSpacingRule.Exactly;
				}
				return LineSpacingRule.Multiple;
			}
			if (m_dyaLine < 0)
			{
				return LineSpacingRule.Exactly;
			}
			return LineSpacingRule.AtLeast;
		}
		set
		{
			switch (value)
			{
			case LineSpacingRule.AtLeast:
				m_fMultLinespace = false;
				m_dyaLine = Math.Abs(m_dyaLine);
				break;
			case LineSpacingRule.Exactly:
				m_fMultLinespace = false;
				m_dyaLine = (short)(-Math.Abs(m_dyaLine));
				break;
			case LineSpacingRule.Multiple:
				m_fMultLinespace = true;
				m_dyaLine = Math.Abs(m_dyaLine);
				break;
			}
		}
	}

	internal LineSpacingDescriptor()
	{
	}

	internal LineSpacingDescriptor(byte[] operand)
	{
		Parse(operand);
	}

	internal void Parse(byte[] operand)
	{
		m_dyaLine = BitConverter.ToInt16(operand, 0);
		m_fMultLinespace = BitConverter.ToInt16(operand, 2) != 0;
	}

	internal byte[] Save()
	{
		byte[] array = new byte[4];
		BitConverter.GetBytes(m_dyaLine).CopyTo(array, 0);
		array[2] = (m_fMultLinespace ? ((byte)1) : ((byte)0));
		return array;
	}
}
