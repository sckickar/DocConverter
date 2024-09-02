using System;
using System.Globalization;
using DocGen.Drawing;
using DocGen.OfficeChart.Implementation;
using DocGen.OfficeChart.Implementation.Collections;
using DocGen.OfficeChart.Implementation.Exceptions;

namespace DocGen.OfficeChart.Parser.Biff_Records.Formula;

[Preserve(AllMembers = true)]
[Token(FormulaToken.tName1)]
[Token(FormulaToken.tName2)]
[Token(FormulaToken.tName3)]
[CLSCompliant(false)]
internal class NamePtg : Ptg, IRangeGetter
{
	private ushort m_usIndex;

	public ushort ExternNameIndex
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

	[Preserve]
	public NamePtg()
	{
	}

	[Preserve]
	public NamePtg(DataProvider provider, int offset, OfficeVersion version)
		: base(provider, offset, version)
	{
	}

	[Preserve]
	public NamePtg(string strFormula, IWorkbook parent)
	{
		IName name = parent.Names[strFormula];
		if (name == null)
		{
			throw new ArgumentNullException("Extern name " + strFormula + " does not exist");
		}
		m_usIndex = (ushort)((name as NameImpl).Index + 1);
	}

	[Preserve]
	public NamePtg(string strFormula, IWorkbook book, IWorksheet sheet)
	{
		IName name;
		if (sheet.Names.Contains(strFormula))
		{
			name = sheet.Names[strFormula];
		}
		else
		{
			if (!book.Names.Contains(strFormula))
			{
				throw new ArgumentException("Unknown name", strFormula);
			}
			name = book.Names[strFormula];
		}
		m_usIndex = (ushort)((name as NameImpl).Index + 1);
	}

	[Preserve]
	public NamePtg(int iNameIndex)
	{
		m_usIndex = (ushort)(iNameIndex + 1);
	}

	public override int GetSize(OfficeVersion version)
	{
		return 5;
	}

	public override string ToString()
	{
		return "( NameIndex = " + m_usIndex + " )";
	}

	public override string ToString(FormulaUtil formulaUtil, int iRow, int iColumn, bool bR1C1, NumberFormatInfo numberFormat, bool isForSerialization)
	{
		if (formulaUtil == null)
		{
			return ToString();
		}
		WorkbookNamesCollection obj = formulaUtil.ParentWorkbook.Names as WorkbookNamesCollection;
		if (obj.Count <= m_usIndex - 1 || m_usIndex < 1)
		{
			throw new ParseException();
		}
		return obj.GetNameByIndex(m_usIndex - 1).Name;
	}

	public override byte[] ToByteArray(OfficeVersion version)
	{
		byte[] array = base.ToByteArray(version);
		BitConverter.GetBytes(m_usIndex).CopyTo(array, 1);
		return array;
	}

	public static FormulaToken IndexToCode(int index)
	{
		return index switch
		{
			1 => FormulaToken.tName1, 
			2 => FormulaToken.tName2, 
			3 => FormulaToken.tName3, 
			_ => throw new ArgumentOutOfRangeException("index"), 
		};
	}

	public IRange GetRange(IWorkbook book, IWorksheet sheet)
	{
		if (book == null)
		{
			throw new ArgumentNullException("book");
		}
		return book.Names[ExternNameIndex - 1] as IRange;
	}

	public Rectangle GetRectangle()
	{
		throw new NotSupportedException();
	}

	public override void InfillPTG(DataProvider provider, ref int offset, OfficeVersion version)
	{
		m_usIndex = provider.ReadUInt16(offset);
		offset += GetSize(version) - 1;
	}
}
