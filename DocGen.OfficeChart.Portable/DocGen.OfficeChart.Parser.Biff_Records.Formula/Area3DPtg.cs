using System;
using System.Globalization;
using System.Text.RegularExpressions;
using DocGen.Drawing;
using DocGen.OfficeChart.Implementation;
using DocGen.OfficeChart.Implementation.Exceptions;

namespace DocGen.OfficeChart.Parser.Biff_Records.Formula;

[Preserve(AllMembers = true)]
[Token(FormulaToken.tArea3d1)]
[Token(FormulaToken.tArea3d2)]
[Token(FormulaToken.tArea3d3)]
[CLSCompliant(false)]
internal class Area3DPtg : AreaPtg, ISheetReference, IReference, IRangeGetter
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
	public Area3DPtg()
	{
	}

	[Preserve]
	public Area3DPtg(DataProvider provider, int offset, OfficeVersion version)
		: base(provider, offset, version)
	{
	}

	[Preserve]
	public Area3DPtg(string strFormula, IWorkbook parent)
	{
		Match match = FormulaUtil.CellRange3DRegex.Match(strFormula);
		if (!match.Success || !(match.Value == strFormula))
		{
			match = FormulaUtil.CellRange3DRegex2.Match(strFormula);
			if (!match.Success || !(match.Value == strFormula) || !(match.Groups["SheetName"].Value == match.Groups["SheetName2"].Value))
			{
				throw new ArgumentException("Not valid area 3D string.");
			}
		}
		SetValues(match, parent);
	}

	[Preserve]
	public Area3DPtg(Area3DPtg ptg)
		: base(ptg)
	{
		m_usRefIndex = ptg.m_usRefIndex;
	}

	[Preserve]
	public Area3DPtg(int iSheetIndex, int iFirstRow, int iFirstCol, int iLastRow, int iLastCol, byte firstOptions, byte lastOptions)
		: base(iFirstRow, iFirstCol, iLastRow, iLastCol, firstOptions, lastOptions)
	{
		m_usRefIndex = (ushort)iSheetIndex;
	}

	[Preserve]
	public Area3DPtg(int iCellRow, int iCellColumn, int iRefIndex, string strFirstRow, string strFirstColumn, string strLastRow, string strLastColumn, bool bR1C1, IWorkbook book)
		: base(iCellRow, iCellColumn, strFirstRow, strFirstColumn, strLastRow, strLastColumn, bR1C1, book)
	{
		m_usRefIndex = (ushort)iRefIndex;
	}

	public override int GetSize(OfficeVersion version)
	{
		return base.GetSize(version) + 2;
	}

	public override byte[] ToByteArray(OfficeVersion version)
	{
		byte[] array = base.ToByteArray(version);
		Buffer.BlockCopy(array, 1, array, 3, array.Length - 3);
		BitConverter.GetBytes(m_usRefIndex).CopyTo(array, 1);
		return array;
	}

	public override string ToString(FormulaUtil formulaUtil, int iRow, int iColumn, bool bR1C1, NumberFormatInfo numberFormat, bool isForSerialization)
	{
		string text = base.ToString(formulaUtil, iRow, iColumn, bR1C1, numberFormat, isForSerialization);
		string text2;
		if (formulaUtil == null)
		{
			text2 = "[ReferenceIndex = " + m_usRefIndex + " ] ";
		}
		else
		{
			text2 = ((WorkbookImpl)formulaUtil.ParentWorkbook).GetSheetNameByReference(m_usRefIndex, throwArgumentOutOfRange: false);
			if (text2 != null)
			{
				text2 = text2.Replace("'", "''");
				text2 = ((!ValidateSheetName(text2)) ? (text2 + "!") : ("'" + text2 + "'!"));
			}
			else
			{
				text2 = string.Empty;
			}
		}
		return text2 + text;
	}

	public static bool ValidateSheetName(string value)
	{
		char[] array = new char[26]
		{
			'!', '@', '#', '$', '%', '^', '&', '(', ')', '-',
			'\'', ';', ' ', '"', '[', ']', '~', '{', '}', '+',
			'|', ',', '=', '<', '>', '`'
		};
		int[] array2 = new int[3] { 88, 70, 68 };
		int num = value.IndexOfAny("123456789".ToCharArray(), 0);
		char c = 'R';
		char c2 = 'C';
		value = value.ToUpper();
		char[] array3 = value.ToCharArray();
		for (int i = 0; i < array.Length - 1; i++)
		{
			if (value.Contains(array[i].ToString()))
			{
				return true;
			}
		}
		if (char.IsDigit(array3[0]) || (array3.Length == 1 && (array3[0] == c2 || array3[0] == c)))
		{
			return true;
		}
		if ((array3[0] == c2 || array3[0] == c) && array3[1] != '0' && char.IsDigit(array3[1]))
		{
			return true;
		}
		if (num < 4 && num != -1)
		{
			for (int j = num; j < array3.Length; j++)
			{
				if (char.IsLetter(array3[j]) || j > num + 5)
				{
					return false;
				}
			}
			if (num == 3)
			{
				for (int k = 0; k < num; k++)
				{
					if (array3[k] < array2[k])
					{
						return true;
					}
					if (array3[k] != array2[k])
					{
						return false;
					}
				}
			}
			return true;
		}
		return false;
	}

	public override int CodeToIndex()
	{
		return CodeToIndex(TokenCode);
	}

	public override FormulaToken GetCorrespondingErrorCode()
	{
		return AreaError3DPtg.IndexToCode(CodeToIndex());
	}

	public override Ptg Offset(int iCurSheetIndex, int iTokenRow, int iTokenColumn, int iSourceSheetIndex, Rectangle rectSource, int iDestSheetIndex, Rectangle rectDest, out bool bChanged, WorkbookImpl book)
	{
		bChanged = false;
		if (m_usRefIndex == (ushort)iSourceSheetIndex)
		{
			Area3DPtg area3DPtg = (Area3DPtg)base.Offset(iDestSheetIndex, iTokenRow, iTokenColumn, iDestSheetIndex, rectSource, iDestSheetIndex, rectDest, out bChanged, book);
			if (bChanged)
			{
				area3DPtg.m_usRefIndex = (ushort)iDestSheetIndex;
			}
			return area3DPtg;
		}
		return (Ptg)Clone();
	}

	public override AreaPtg ConvertToErrorPtg()
	{
		return new AreaError3DPtg(this);
	}

	public string BaseToString(FormulaUtil formulaUtil, int iRow, int iColumn, bool bR1C1)
	{
		return base.ToString(formulaUtil, iRow, iColumn, bR1C1);
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

	protected void SetValues(Match m, IWorkbook parent)
	{
		string value = m.Groups["SheetName"].Value;
		string value2 = m.Groups["Column1"].Value;
		string value3 = m.Groups["Row1"].Value;
		string value4 = m.Groups["Column2"].Value;
		string value5 = m.Groups["Row2"].Value;
		SetArea(0, 0, value3, value2, value5, value4, bR1C1: false, parent);
		SetSheetIndex(value, parent);
	}

	public override void InfillPTG(DataProvider provider, ref int offset, OfficeVersion version)
	{
		m_usRefIndex = provider.ReadUInt16(offset);
		offset += 2;
		base.InfillPTG(provider, ref offset, version);
	}

	public new static FormulaToken IndexToCode(int index)
	{
		return index switch
		{
			1 => FormulaToken.tArea3d1, 
			2 => FormulaToken.tArea3d2, 
			3 => FormulaToken.tArea3d3, 
			_ => throw new ArgumentOutOfRangeException("index", "Must be less than 4 and greater than than 0."), 
		};
	}

	public new static int CodeToIndex(FormulaToken code)
	{
		return code switch
		{
			FormulaToken.tArea3d1 => 1, 
			FormulaToken.tArea3d2 => 2, 
			FormulaToken.tArea3d3 => 3, 
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
				return sheet[base.FirstRow + 1, base.FirstColumn + 1, base.LastRow + 1, base.LastColumn + 1];
			}
			return workbookImpl.Worksheets[0][base.FirstRow + 1, base.FirstColumn + 1, base.LastRow + 1, base.LastColumn + 1];
		}
		return new ExternalRange(workbookImpl.GetExternSheet(m_usRefIndex), base.FirstRow + 1, base.FirstColumn + 1, base.LastRow + 1, base.LastColumn + 1);
	}
}
