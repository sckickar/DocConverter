using System;
using System.Collections.Generic;
using System.Globalization;
using DocGen.OfficeChart.Implementation;

namespace DocGen.OfficeChart.Parser.Biff_Records.Formula;

[Preserve(AllMembers = true)]
[Token(FormulaToken.tAttr)]
[CLSCompliant(false)]
internal class AttrPtg : FunctionVarPtg
{
	public const int SIZE = 4;

	private const int DEF_WORD_SIZE = 2;

	private const string DEF_SUM = "SUM";

	private const string DEF_IF = "IF";

	private const string DEF_GOTO = "GOTO";

	private const string DEF_CHOOSE = "CHOOSE";

	private const string DEF_NOT_IMPLEMENTED = "( tAttr not implemented )";

	private const ushort DEF_SPACE_AFTER_MASK = 4;

	private byte m_Options;

	private ushort m_usData;

	private ushort[] m_arrOffsets;

	public byte Options => m_Options;

	public ushort AttrData
	{
		get
		{
			return m_usData;
		}
		set
		{
			m_usData = value;
		}
	}

	public int AttrData1
	{
		get
		{
			return m_usData & 0xFF;
		}
		internal set
		{
			m_usData = (ushort)((m_usData & 0xFF00u) | ((uint)value & 0xFFu));
		}
	}

	public int SpaceCount
	{
		get
		{
			if (HasSpace)
			{
				return m_usData >> 8;
			}
			return -1;
		}
		set
		{
			if (HasSpace)
			{
				if (value < 1 || value > 255)
				{
					throw new ArgumentOutOfRangeException("value", "Value cannot be less than 1 and greater than 255.");
				}
				m_usData = (ushort)((m_usData & 0xFF) + value << 8);
				return;
			}
			throw new NotSupportedException();
		}
	}

	public bool SpaceAfterToken
	{
		get
		{
			if (HasSpace)
			{
				return (m_usData & 4) != 0;
			}
			return false;
		}
		set
		{
			if (HasSpace)
			{
				m_usData = (ushort)(value ? (m_usData | 4u) : (m_usData & 0xFFFFFFFBu));
				return;
			}
			throw new NotSupportedException();
		}
	}

	public bool HasSemiVolatile
	{
		get
		{
			return (m_Options & 1) == 1;
		}
		set
		{
			if (value)
			{
				m_Options |= 1;
			}
			else
			{
				m_Options &= 254;
			}
		}
	}

	public bool HasOptimizedIf
	{
		get
		{
			return (m_Options & 2) == 2;
		}
		set
		{
			if (value)
			{
				m_Options |= 2;
			}
			else
			{
				m_Options &= 253;
			}
		}
	}

	public bool HasOptimizedChoose
	{
		get
		{
			return (m_Options & 4) == 4;
		}
		set
		{
			if (value)
			{
				m_Options |= 4;
			}
			else
			{
				m_Options &= 251;
			}
		}
	}

	public bool HasOptGoto
	{
		get
		{
			return (m_Options & 8) == 8;
		}
		set
		{
			if (value)
			{
				m_Options |= 8;
			}
			else
			{
				m_Options &= 247;
			}
		}
	}

	public bool HasSum
	{
		get
		{
			return (m_Options & 0x10) == 16;
		}
		set
		{
			if (value)
			{
				m_Options |= 16;
			}
			else
			{
				m_Options &= 239;
			}
		}
	}

	public bool HasBaxcel
	{
		get
		{
			return (m_Options & 0x20) == 32;
		}
		set
		{
			if (value)
			{
				m_Options |= 32;
			}
			else
			{
				m_Options &= 223;
			}
		}
	}

	public bool HasSpace
	{
		get
		{
			return (m_Options & 0x40) == 64;
		}
		set
		{
			if (value)
			{
				m_Options |= 64;
			}
			else
			{
				m_Options &= 191;
			}
		}
	}

	[Preserve]
	public AttrPtg()
	{
		m_Options = 0;
		m_usData = 0;
		base.NumberOfArguments = 1;
		TokenCode = FormulaToken.tAttr;
	}

	[Preserve]
	public AttrPtg(DataProvider provider, int iOffset, OfficeVersion version)
		: base(provider, iOffset, version)
	{
	}

	[Preserve]
	public AttrPtg(byte options, ushort usData)
	{
		TokenCode = FormulaToken.tAttr;
		m_Options = options;
		m_usData = usData;
		base.NumberOfArguments = ((m_Options != 1) ? ((byte)1) : ((byte)0));
	}

	[Preserve]
	public AttrPtg(int options, int data)
		: this((byte)options, (ushort)data)
	{
	}

	public override int GetSize(OfficeVersion version)
	{
		if (HasOptimizedChoose)
		{
			return 4 + m_arrOffsets.Length * 2;
		}
		return 4;
	}

	public override void PushResultToStack(FormulaUtil formulaUtil, Stack<object> operands, bool isForSerialization)
	{
		if (HasSpace)
		{
			string text = new string(' ', SpaceCount);
			if (SpaceAfterToken)
			{
				object obj = operands.Pop();
				string text2 = obj.ToString();
				if (text2.EndsWith(" ") && text2[0] == ' ')
				{
					text2 = operands.Pop().ToString();
				}
				else
				{
					obj = null;
				}
				operands.Push(text2 + text);
				if (obj != null)
				{
					operands.Push(obj);
				}
			}
			else
			{
				operands.Push(this);
			}
		}
		else if (m_Options != 0)
		{
			base.PushResultToStack(formulaUtil, operands, isForSerialization);
		}
	}

	public override string ToString(FormulaUtil formulaUtil, int iRow, int iColumn, bool bR1C1, NumberFormatInfo numberFormat, bool isForSerialization)
	{
		if (HasSum)
		{
			return "SUM";
		}
		if (HasOptimizedIf)
		{
			return "IF";
		}
		if (HasOptGoto)
		{
			return "GOTO";
		}
		if (HasOptimizedChoose)
		{
			return "CHOOSE";
		}
		if (HasSpace)
		{
			int spaceCount = SpaceCount;
			return new string(' ', spaceCount);
		}
		if (m_Options == 0)
		{
			return string.Empty;
		}
		return "( tAttr not implemented )";
	}

	public override byte[] ToByteArray(OfficeVersion version)
	{
		byte[] array = new byte[GetSize(version)];
		int num = 0;
		array[num++] = 25;
		array[num++] = m_Options;
		Buffer.BlockCopy(BitConverter.GetBytes(m_usData), 0, array, num, 2);
		num += 2;
		if (HasOptimizedChoose)
		{
			Buffer.BlockCopy(m_arrOffsets, 0, array, num, GetSize(version) - num);
		}
		return array;
	}

	public override void InfillPTG(DataProvider provider, ref int offset, OfficeVersion version)
	{
		int num = offset;
		TokenCode = FormulaToken.tAttr;
		m_Options = provider.ReadByte(offset++);
		m_usData = provider.ReadUInt16(offset);
		offset += 2;
		if (HasOptimizedChoose)
		{
			int num2 = m_usData + 1;
			m_arrOffsets = new ushort[num2];
			for (int i = 0; i < num2; i++)
			{
				m_arrOffsets[i] = provider.ReadUInt16(offset);
				offset += 2;
			}
		}
		else
		{
			base.NumberOfArguments = ((m_Options != 1) ? ((byte)1) : ((byte)0));
		}
		offset = num + GetSize(version) - 1;
	}
}
