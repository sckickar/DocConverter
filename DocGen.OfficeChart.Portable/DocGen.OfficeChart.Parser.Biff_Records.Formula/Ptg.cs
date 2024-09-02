using System;
using System.Globalization;
using System.Text;
using DocGen.Drawing;
using DocGen.OfficeChart.Implementation;

namespace DocGen.OfficeChart.Parser.Biff_Records.Formula;

[Preserve(AllMembers = true)]
internal abstract class Ptg : ICloneable
{
	private const int DEF_PTG_INDEX_DELTA = 32;

	private FormulaToken m_Code;

	public virtual bool IsOperation => false;

	public virtual FormulaToken TokenCode
	{
		get
		{
			return m_Code;
		}
		set
		{
			m_Code = value;
		}
	}

	[Preserve]
	protected Ptg()
	{
	}

	[Preserve]
	protected Ptg(DataProvider provider, int offset, OfficeVersion version)
	{
		m_Code = (FormulaToken)provider.ReadByte(offset);
		offset++;
		InfillPTG(provider, ref offset, version);
	}

	public static string GetString16Bit(byte[] data, int offset)
	{
		int iFullLength;
		return GetString16Bit(data, offset, out iFullLength);
	}

	public static string GetString16Bit(byte[] data, int offset, out int iFullLength)
	{
		if (offset + 3 >= data.Length)
		{
			throw new ArgumentOutOfRangeException("GetString16Bit: data array too small.");
		}
		ushort num = BitConverter.ToUInt16(data, offset);
		offset += 2;
		bool flag = BitConverter.ToBoolean(data, offset);
		offset++;
		iFullLength = (flag ? (3 + num * 2) : (3 + num));
		if (iFullLength >= data.Length)
		{
			throw new ArgumentOutOfRangeException("GetString16Bit: data array too small.");
		}
		if (!flag)
		{
			return BiffRecordRaw.LatinEncoding.GetString(data, offset, num);
		}
		return Encoding.Unicode.GetString(data, offset, num * 2);
	}

	public virtual void InfillPTG(DataProvider provider, ref int offset, OfficeVersion version)
	{
	}

	public abstract int GetSize(OfficeVersion version);

	public virtual byte[] ToByteArray(OfficeVersion version)
	{
		byte[] array = new byte[GetSize(version)];
		array[0] = (byte)TokenCode;
		return array;
	}

	public override string ToString()
	{
		return ToString(null, 0, 0, bR1C1: false);
	}

	public virtual string ToString(FormulaUtil formulaUtil)
	{
		return ToString(formulaUtil, 0, 0, bR1C1: false);
	}

	public virtual string ToString(FormulaUtil formulaUtil, int iRow, int iColumn, bool bR1C1)
	{
		return ToString(formulaUtil, iRow, iColumn, bR1C1, null, isForSerialization: false);
	}

	public virtual string ToString(int row, int col, bool bR1C1)
	{
		return ToString(null, row, col, bR1C1);
	}

	public virtual string ToString(FormulaUtil formulaUtil, int row, int col, bool bR1C1, NumberFormatInfo numberFormat)
	{
		return ToString(formulaUtil, row, col, bR1C1, numberFormat, isForSerialization: false);
	}

	public virtual string ToString(FormulaUtil formulaUtil, int row, int col, bool bR1C1, NumberFormatInfo numberFormat, bool isForSerialization)
	{
		return base.ToString();
	}

	public virtual Ptg Offset(int iRowOffset, int iColumnOffset, WorkbookImpl book)
	{
		return (Ptg)Clone();
	}

	public virtual Ptg Offset(int iCurSheetIndex, int iTokenRow, int iTokenColumn, int iSourceSheetIndex, Rectangle rectSource, int iDestSheetIndex, Rectangle rectDest, out bool bChanged, WorkbookImpl book)
	{
		bChanged = false;
		return (Ptg)Clone();
	}

	public static bool RectangleContains(Rectangle rect, int iRow, int iColumn)
	{
		if (rect.Left <= iColumn && rect.Right >= iColumn && rect.Top <= iRow && rect.Bottom >= iRow)
		{
			return true;
		}
		return false;
	}

	public virtual Ptg ConvertSharedToken(IWorkbook parent, int iRow, int iColumn)
	{
		return (Ptg)Clone();
	}

	public virtual Ptg ConvertPtgToNPtg(IWorkbook parent, int iRow, int iColumn)
	{
		return this;
	}

	public int CompareTo(Ptg token)
	{
		if (token == null)
		{
			return 1;
		}
		int num = TokenCode - token.TokenCode;
		if (num == 0)
		{
			num = GetSize(OfficeVersion.Excel2007) - token.GetSize(OfficeVersion.Excel2007);
		}
		if (num == 0)
		{
			num = CompareContent(token);
		}
		return num;
	}

	protected int CompareContent(Ptg token)
	{
		if (token == null)
		{
			throw new ArgumentNullException("token");
		}
		byte[] array = ToByteArray(OfficeVersion.Excel2007);
		byte[] array2 = token.ToByteArray(OfficeVersion.Excel2007);
		return (!BiffRecordRaw.CompareArrays(array, array2)) ? 1 : 0;
	}

	public static bool CompareArrays(Ptg[] arrTokens1, Ptg[] arrTokens2)
	{
		if (arrTokens1 == null && arrTokens2 == null)
		{
			return true;
		}
		if (arrTokens1 == null || arrTokens2 == null)
		{
			return false;
		}
		int num = arrTokens1.Length;
		if (num != arrTokens2.Length)
		{
			return false;
		}
		for (int i = 0; i < num; i++)
		{
			Ptg obj = arrTokens1[i];
			Ptg token = arrTokens2[i];
			if (obj.CompareTo(token) != 0)
			{
				return false;
			}
		}
		return true;
	}

	public virtual string ToString(FormulaUtil formulaUtil, int row, int col, bool bR1C1, NumberFormatInfo numberInfo, bool isForSerialization, IWorksheet sheet)
	{
		return ToString(formulaUtil, row, col, bR1C1, numberInfo, isForSerialization);
	}

	public object Clone()
	{
		return MemberwiseClone();
	}

	public static FormulaToken IndexToCode(FormulaToken baseToken, int index)
	{
		if (index < 1 || index > 3)
		{
			throw new ArgumentOutOfRangeException("index", "Value cannot be less than 1 or greater than 3.");
		}
		return baseToken + (index - 1) * 32;
	}
}
