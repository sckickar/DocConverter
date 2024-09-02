using System;
using System.Globalization;
using DocGen.Drawing;
using DocGen.OfficeChart.Implementation;
using DocGen.OfficeChart.Implementation.Exceptions;

namespace DocGen.OfficeChart.Parser.Biff_Records.Formula;

[Preserve(AllMembers = true)]
[Token(FormulaToken.tNameX1)]
[Token(FormulaToken.tNameX2)]
[Token(FormulaToken.tNameX3)]
[CLSCompliant(false)]
internal class NameXPtg : Ptg, ISheetReference, IReference, IRangeGetter
{
	private ushort m_usRefIndex;

	private ushort m_usNameIndex;

	public ushort NameIndex
	{
		get
		{
			return m_usNameIndex;
		}
		set
		{
			m_usNameIndex = value;
		}
	}

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
	public NameXPtg()
	{
	}

	[Preserve]
	public NameXPtg(DataProvider provider, int offset, OfficeVersion version)
		: base(provider, offset, version)
	{
	}

	[Preserve]
	public NameXPtg(string strFormula, IWorkbook parent)
	{
		IName name = parent.Names[strFormula];
		if (name == null)
		{
			throw new ArgumentNullException("Extern name " + strFormula + " does not exist");
		}
		m_usNameIndex = (ushort)((name as NameImpl).Index + 1);
		Ptg ptg = ((NameImpl)name).Record.FormulaTokens[0];
		if (ptg is Area3DPtg)
		{
			m_usRefIndex = ((Area3DPtg)ptg).RefIndex;
		}
		else if (ptg is Ref3DPtg)
		{
			m_usRefIndex = ((Ref3DPtg)ptg).RefIndex;
		}
	}

	[Preserve]
	public NameXPtg(int iBookIndex, int iNameIndex)
	{
		m_usRefIndex = (ushort)iBookIndex;
		m_usNameIndex = (ushort)(iNameIndex + 1);
	}

	public override int GetSize(OfficeVersion version)
	{
		return 7;
	}

	public override string ToString(FormulaUtil formulaUtil, int iRow, int iColumn, bool bR1C1, NumberFormatInfo numberFormat, bool isForSerialization)
	{
		return ToString(formulaUtil, iRow, iColumn, bR1C1, numberFormat, isForSerialization, null);
	}

	public override string ToString(FormulaUtil formulaUtil, int row, int col, bool bR1C1, NumberFormatInfo numberInfo, bool isForSerialization, IWorksheet sheet)
	{
		if (formulaUtil == null)
		{
			return $"( ExternNameIndex = {m_usNameIndex}, RefIndex = {m_usRefIndex} )";
		}
		WorkbookImpl workbookImpl = (WorkbookImpl)formulaUtil.ParentWorkbook;
		if (workbookImpl.IsLocalReference(m_usRefIndex))
		{
			if ((formulaUtil.ParentWorkbook as WorkbookImpl).InnerNamesColection.Count <= m_usNameIndex - 1 || m_usNameIndex < 1)
			{
				throw new ParseException();
			}
			IName name = (formulaUtil.ParentWorkbook as WorkbookImpl).InnerNamesColection[m_usNameIndex - 1];
			IWorksheet worksheet = name.Worksheet;
			if (worksheet != sheet && worksheet != null)
			{
				return $"'{worksheet.Name}'!{name.Name}";
			}
			return name.Name;
		}
		int bookIndex = workbookImpl.GetBookIndex(m_usRefIndex);
		ExternWorkbookImpl externWorkbookImpl = workbookImpl.ExternWorkbooks[bookIndex];
		ExternNameImpl externNameImpl = externWorkbookImpl.ExternNames[m_usNameIndex - 1];
		if (workbookImpl.Version == OfficeVersion.Excel97to2003 || externWorkbookImpl.URL == null || !isForSerialization)
		{
			if (Area3DPtg.ValidateSheetName(externNameImpl.Name))
			{
				return $"'{externWorkbookImpl.URL}'!{externNameImpl.Name}";
			}
			if (externWorkbookImpl.URL == null)
			{
				return externNameImpl.Name;
			}
			return $"'{externWorkbookImpl.URL}'!{externNameImpl.Name}";
		}
		if (externWorkbookImpl.IsAddInFunctions && externWorkbookImpl.Worksheets.Count == 0)
		{
			return $"'{externWorkbookImpl.URL}'!'{externNameImpl.Name}'";
		}
		return string.Format("{0}!{1}", "[" + (bookIndex + 1) + "]", externNameImpl.Name);
	}

	public string BaseToString(FormulaUtil formulaUtil, int iRow, int iColumn, bool bR1C1)
	{
		return ToString(formulaUtil, iRow, iColumn, bR1C1);
	}

	public override byte[] ToByteArray(OfficeVersion version)
	{
		byte[] array = base.ToByteArray(version);
		BitConverter.GetBytes(m_usRefIndex).CopyTo(array, 1);
		BitConverter.GetBytes(m_usNameIndex).CopyTo(array, 3);
		return array;
	}

	public static FormulaToken IndexToCode(int index)
	{
		return index switch
		{
			1 => FormulaToken.tNameX1, 
			2 => FormulaToken.tNameX2, 
			3 => FormulaToken.tNameX3, 
			_ => throw new ArgumentOutOfRangeException("index"), 
		};
	}

	public IRange GetRange(IWorkbook book, IWorksheet sheet)
	{
		if (book == null)
		{
			throw new ArgumentNullException("book");
		}
		WorkbookImpl obj = (WorkbookImpl)book;
		obj.CheckForInternalReference(RefIndex);
		return (NameImpl)obj.Names[NameIndex - 1];
	}

	public Rectangle GetRectangle()
	{
		throw new NotSupportedException();
	}

	public Ptg UpdateRectangle(Rectangle rectangle)
	{
		throw new NotSupportedException();
	}

	public override void InfillPTG(DataProvider provider, ref int offset, OfficeVersion version)
	{
		m_usRefIndex = provider.ReadUInt16(offset);
		offset += 2;
		m_usNameIndex = provider.ReadUInt16(offset);
		offset += GetSize(version) - 3;
	}
}
