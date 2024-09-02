using System;
using System.Globalization;
using System.Text.RegularExpressions;
using DocGen.Drawing;
using DocGen.OfficeChart.Implementation;
using DocGen.OfficeChart.Implementation.Exceptions;

namespace DocGen.OfficeChart.Parser.Biff_Records.Formula;

[Preserve(AllMembers = true)]
[Token(FormulaToken.tRef3d1)]
[Token(FormulaToken.tRef3d2)]
[Token(FormulaToken.tRef3d3)]
[CLSCompliant(false)]
internal class Ref3DPtg : RefPtg, IRangeGetter, ISheetReference, IReference
{
	private ushort m_usRefIndex;

	public ushort RefIndex
	{
		get
		{
			return m_usRefIndex;
		}
		set
		{
			m_usRefIndex = value;
		}
	}

	[Preserve]
	public Ref3DPtg()
	{
	}

	[Preserve]
	public Ref3DPtg(DataProvider provider, int offset, OfficeVersion version)
		: base(provider, offset, version)
	{
	}

	[Preserve]
	public Ref3DPtg(string strFormula, IWorkbook parent)
	{
		Match match = FormulaUtil.Cell3DRegex.Match(strFormula);
		if (!match.Success)
		{
			throw new ArgumentException("strFormula");
		}
		string value = match.Groups["SheetName"].Value;
		string value2 = match.Groups["Row1"].Value;
		string value3 = match.Groups["Column1"].Value;
		SetCellA1(value3, value2);
		SetSheetIndex(value, parent);
	}

	[Preserve]
	public Ref3DPtg(int iCellRow, int iCellColumn, int iSheetIndex, string strRow, string strColumn, bool bR1C1)
		: base(iCellRow, iCellColumn, strRow, strColumn, bR1C1)
	{
		m_usRefIndex = (ushort)iSheetIndex;
	}

	[Preserve]
	public Ref3DPtg(int iSheetIndex, int iRowIndex, int iColIndex, byte options)
		: base(iRowIndex, iColIndex, options)
	{
		m_usRefIndex = (ushort)iSheetIndex;
	}

	[Preserve]
	public Ref3DPtg(Ref3DPtg twin)
		: base(twin)
	{
		m_usRefIndex = twin.m_usRefIndex;
	}

	public override int GetSize(OfficeVersion version)
	{
		return base.GetSize(version) + 2;
	}

	public override string ToString()
	{
		return "[RefIndex=" + m_usRefIndex + " " + base.ToString() + "]";
	}

	public override byte[] ToByteArray(OfficeVersion version)
	{
		byte[] array = base.ToByteArray(version);
		int num = array.Length;
		Buffer.BlockCopy(array, 1, array, 3, num - 3);
		BitConverter.GetBytes(m_usRefIndex).CopyTo(array, 1);
		return array;
	}

	public override string ToString(FormulaUtil formulaUtil, int iRow, int iColumn, bool bR1C1, NumberFormatInfo numberFormat, bool isForSerialization)
	{
		if (formulaUtil == null)
		{
			return ToString();
		}
		string sheetName = GetSheetName(formulaUtil.ParentWorkbook, m_usRefIndex);
		sheetName = ((sheetName == null) ? string.Empty : ((!Area3DPtg.ValidateSheetName(sheetName)) ? (sheetName + "!") : ("'" + sheetName + "'!")));
		return sheetName + base.ToString(formulaUtil, iRow, iColumn, bR1C1, numberFormat, isForSerialization);
	}

	public string BaseToString(FormulaUtil formulaUtil, int iRow, int iColumn, bool bR1C1)
	{
		return base.ToString(formulaUtil, iRow, iColumn, bR1C1);
	}

	public override Ptg Offset(int iCurSheetIndex, int iTokenRow, int iTokenColumn, int iSourceSheetIndex, Rectangle rectSource, int iDestSheetIndex, Rectangle rectDest, out bool bChanged, WorkbookImpl book)
	{
		bChanged = false;
		if (m_usRefIndex == (ushort)iSourceSheetIndex)
		{
			Ref3DPtg ref3DPtg = (Ref3DPtg)base.Offset(iDestSheetIndex, iTokenRow, iTokenColumn, iDestSheetIndex, rectSource, iDestSheetIndex, rectDest, out bChanged, book);
			if (bChanged)
			{
				ref3DPtg.m_usRefIndex = (ushort)iDestSheetIndex;
			}
			return ref3DPtg;
		}
		return (Ptg)Clone();
	}

	public override int CodeToIndex()
	{
		return CodeToIndex(TokenCode);
	}

	public override FormulaToken GetCorrespondingErrorCode()
	{
		return RefError3dPtg.IndexToCode(CodeToIndex());
	}

	public static string GetSheetName(IWorkbook book, int refIndex)
	{
		if (book == null)
		{
			return null;
		}
		return ((WorkbookImpl)book).GetSheetNameByReference(refIndex, throwArgumentOutOfRange: false)?.Replace("'", "''");
	}

	protected void SetSheetIndex(string sheetName, IWorkbook parent)
	{
		WorkbookImpl workbookImpl = (WorkbookImpl)parent;
		if (sheetName[0] == '\'' && sheetName[sheetName.Length - 1] == '\'')
		{
			sheetName = sheetName.Substring(1, sheetName.Length - 2);
		}
		try
		{
			m_usRefIndex = (ushort)workbookImpl.AddSheetReference(sheetName);
		}
		catch (ArgumentException)
		{
			throw new ParseException();
		}
	}

	public new static FormulaToken IndexToCode(int index)
	{
		return index switch
		{
			1 => FormulaToken.tRef3d1, 
			2 => FormulaToken.tRef3d2, 
			3 => FormulaToken.tRef3d3, 
			_ => throw new ArgumentOutOfRangeException("index"), 
		};
	}

	public new static int CodeToIndex(FormulaToken token)
	{
		return token switch
		{
			FormulaToken.tRef3d1 => 1, 
			FormulaToken.tRef3d2 => 2, 
			FormulaToken.tRef3d3 => 3, 
			_ => throw new ArgumentOutOfRangeException("index"), 
		};
	}

	public new IRange GetRange(IWorkbook book, IWorksheet sheet)
	{
		if (book == null)
		{
			throw new ArgumentNullException("book");
		}
		WorkbookImpl workbookImpl = (WorkbookImpl)book;
		IRange range = null;
		if (!workbookImpl.IsExternalReference(m_usRefIndex))
		{
			sheet = workbookImpl.GetSheetByReference(m_usRefIndex, bThrowExceptions: false);
			if (sheet != null)
			{
				return sheet[RowIndex + 1, ColumnIndex + 1];
			}
			return workbookImpl.Worksheets[0][RowIndex + 1, ColumnIndex + 1];
		}
		return new ExternalRange(workbookImpl.GetExternSheet(m_usRefIndex), RowIndex + 1, ColumnIndex + 1);
	}

	public override void InfillPTG(DataProvider provider, ref int offset, OfficeVersion version)
	{
		m_usRefIndex = provider.ReadUInt16(offset);
		offset += 2;
		base.InfillPTG(provider, ref offset, version);
	}
}
