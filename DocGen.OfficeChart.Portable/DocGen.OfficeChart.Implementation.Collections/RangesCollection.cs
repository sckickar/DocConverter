using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using DocGen.Drawing;
using DocGen.OfficeChart.Calculate;
using DocGen.OfficeChart.Interfaces;
using DocGen.OfficeChart.Parser.Biff_Records.Formula;

namespace DocGen.OfficeChart.Implementation.Collections;

internal class RangesCollection : CollectionBaseEx<IRange>, IEnumerable<IRange>, IEnumerable, IRanges, IParentApplication, IRange, ICombinedRange, INativePTG
{
	private const string DEF_WRONG_WORKSHEET = "Can't operate with ranges from different worksheet";

	private IWorksheet m_worksheet;

	private int m_iFirstRow;

	private int m_iFirstColumn;

	private int m_iLastRow;

	private int m_iLastColumn;

	private RTFStringArray m_rtfString;

	internal string CalculatedValue
	{
		get
		{
			if (base.Parent is WorksheetImpl && ((WorksheetImpl)base.Parent).CalcEngine != null)
			{
				string cellRef = RangeInfo.GetAlphaLabel(Column) + Row;
				return ((WorksheetImpl)base.Parent).CalcEngine.PullUpdatedValue(cellRef);
			}
			return null;
		}
	}

	public string Address
	{
		get
		{
			CheckDisposed();
			StringBuilder stringBuilder = new StringBuilder();
			if (base.Count == 0)
			{
				return string.Empty;
			}
			IRange range = base.InnerList[0];
			stringBuilder.Append(range.Address);
			string addressSeparator = GetAddressSeparator();
			int i = 1;
			for (int count = base.Count; i < count; i++)
			{
				stringBuilder.Append(addressSeparator);
				range = base.InnerList[i];
				stringBuilder.Append(range.Address);
			}
			return stringBuilder.ToString();
		}
	}

	public string AddressLocal
	{
		get
		{
			CheckDisposed();
			StringBuilder stringBuilder = new StringBuilder();
			if (base.Count == 0)
			{
				return string.Empty;
			}
			IRange range = base.InnerList[0];
			stringBuilder.Append(range.AddressLocal);
			string addressSeparator = GetAddressSeparator();
			int i = 1;
			for (int count = base.Count; i < count; i++)
			{
				stringBuilder.Append(addressSeparator);
				range = base.InnerList[i];
				stringBuilder.Append(range.AddressLocal);
			}
			return stringBuilder.ToString();
		}
	}

	public string AddressGlobal
	{
		get
		{
			CheckDisposed();
			StringBuilder stringBuilder = new StringBuilder();
			if (base.Count == 0)
			{
				return string.Empty;
			}
			IRange range = base.InnerList[0];
			stringBuilder.Append(range.AddressGlobal);
			string addressSeparator = GetAddressSeparator();
			int i = 1;
			for (int count = base.Count; i < count; i++)
			{
				stringBuilder.Append(addressSeparator);
				range = base.InnerList[i];
				stringBuilder.Append(range.AddressGlobal);
			}
			return stringBuilder.ToString();
		}
	}

	public string AddressR1C1
	{
		get
		{
			CheckDisposed();
			StringBuilder stringBuilder = new StringBuilder();
			if (base.Count == 0)
			{
				return string.Empty;
			}
			IRange range = base.InnerList[0];
			stringBuilder.Append(range.AddressR1C1);
			string addressSeparator = GetAddressSeparator();
			int i = 1;
			for (int count = base.Count; i < count; i++)
			{
				stringBuilder.Append(addressSeparator);
				range = base.InnerList[i];
				stringBuilder.Append(range.AddressR1C1);
			}
			return stringBuilder.ToString();
		}
	}

	public string AddressR1C1Local
	{
		get
		{
			CheckDisposed();
			StringBuilder stringBuilder = new StringBuilder();
			if (base.Count == 0)
			{
				return string.Empty;
			}
			IRange range = base.InnerList[0];
			stringBuilder.Append(range.AddressR1C1Local);
			string addressSeparator = GetAddressSeparator();
			int i = 1;
			for (int count = base.Count; i < count; i++)
			{
				stringBuilder.Append(addressSeparator);
				range = base.InnerList[i];
				stringBuilder.Append(range.AddressR1C1Local);
			}
			return stringBuilder.ToString();
		}
	}

	public bool Boolean
	{
		get
		{
			CheckDisposed();
			if (base.Count == 0)
			{
				return false;
			}
			IRange range = base.InnerList[0];
			bool boolean = range.Boolean;
			int i = 0;
			for (int count = base.Count; i < count; i++)
			{
				range = base.InnerList[i];
				if (boolean != range.Boolean)
				{
					return false;
				}
			}
			return boolean;
		}
		set
		{
			CheckDisposed();
			int i = 0;
			for (int count = base.Count; i < count; i++)
			{
				base.InnerList[i].Boolean = value;
			}
		}
	}

	public IBorders Borders
	{
		get
		{
			CheckDisposed();
			return CellStyle.Borders;
		}
	}

	public IRange[] Cells
	{
		get
		{
			CheckDisposed();
			List<IRange> list = new List<IRange>();
			int i = 0;
			for (int count = base.Count; i < count; i++)
			{
				IRange range = base.InnerList[i];
				list.AddRange(range.Cells);
			}
			return list.ToArray();
		}
	}

	public int Column
	{
		get
		{
			CheckDisposed();
			return m_iFirstColumn;
		}
	}

	public int ColumnGroupLevel
	{
		get
		{
			CheckDisposed();
			if (base.Count == 0)
			{
				return int.MinValue;
			}
			IRange range = base.InnerList[0];
			int columnGroupLevel = range.ColumnGroupLevel;
			int i = 0;
			for (int count = base.Count; i < count; i++)
			{
				range = base.InnerList[i];
				if (columnGroupLevel != range.ColumnGroupLevel)
				{
					return int.MinValue;
				}
			}
			return columnGroupLevel;
		}
	}

	public double ColumnWidth
	{
		get
		{
			CheckDisposed();
			if (base.Count == 0)
			{
				return double.MinValue;
			}
			IRange range = base.InnerList[0];
			double columnWidth = range.ColumnWidth;
			int i = 0;
			for (int count = base.Count; i < count; i++)
			{
				range = base.InnerList[i];
				if (columnWidth != range.ColumnWidth)
				{
					return double.MinValue;
				}
			}
			return columnWidth;
		}
		set
		{
			CheckDisposed();
			int i = 0;
			for (int count = base.Count; i < count; i++)
			{
				base.InnerList[i].ColumnWidth = value;
			}
		}
	}

	int IRange.Count
	{
		get
		{
			CheckDisposed();
			int num = 0;
			int i = 0;
			for (int count = base.Count; i < count; i++)
			{
				IRange range = base.InnerList[i];
				num += range.Count;
			}
			return num;
		}
	}

	public DateTime DateTime
	{
		get
		{
			CheckDisposed();
			if (base.Count == 0)
			{
				return DateTime.MinValue;
			}
			IRange range = base.InnerList[0];
			DateTime dateTime = range.DateTime;
			int i = 0;
			for (int count = base.Count; i < count; i++)
			{
				range = base.InnerList[i];
				if (dateTime != range.DateTime)
				{
					return DateTime.MinValue;
				}
			}
			return dateTime;
		}
		set
		{
			CheckDisposed();
			int i = 0;
			for (int count = base.Count; i < count; i++)
			{
				base.InnerList[i].DateTime = value;
			}
		}
	}

	public string DisplayText
	{
		get
		{
			CheckDisposed();
			if (base.Count == 0)
			{
				return null;
			}
			IRange range = base.InnerList[0];
			string displayText = range.DisplayText;
			int i = 0;
			for (int count = base.Count; i < count; i++)
			{
				range = base.InnerList[i];
				if (displayText != range.DisplayText)
				{
					return null;
				}
			}
			return displayText;
		}
	}

	public IRange End
	{
		get
		{
			CheckDisposed();
			if (m_iLastRow < 1 || m_iLastColumn < 1)
			{
				return null;
			}
			return Worksheet[m_iLastRow, m_iLastColumn];
		}
	}

	public IRange EntireColumn
	{
		get
		{
			CheckDisposed();
			return GetEntireColumnRow(bIsColumn: true);
		}
	}

	public IRange EntireRow
	{
		get
		{
			CheckDisposed();
			return GetEntireColumnRow(bIsColumn: false);
		}
	}

	public string Error
	{
		get
		{
			CheckDisposed();
			if (base.Count == 0)
			{
				return null;
			}
			IRange range = base.InnerList[0];
			string error = range.Error;
			int i = 0;
			for (int count = base.Count; i < count; i++)
			{
				range = base.InnerList[i];
				if (error != range.Error)
				{
					return null;
				}
			}
			return error;
		}
		set
		{
			CheckDisposed();
			int i = 0;
			for (int count = base.Count; i < count; i++)
			{
				base.InnerList[i].Error = value;
			}
		}
	}

	public string Formula
	{
		get
		{
			CheckDisposed();
			if (base.Count == 0)
			{
				return null;
			}
			IRange range = base.InnerList[0];
			string formula = range.Formula;
			int i = 0;
			for (int count = base.Count; i < count; i++)
			{
				range = base.InnerList[i];
				if (formula != range.Formula)
				{
					return null;
				}
			}
			return formula;
		}
		set
		{
			CheckDisposed();
			int i = 0;
			for (int count = base.Count; i < count; i++)
			{
				base.InnerList[i].Formula = value;
			}
		}
	}

	public string FormulaR1C1
	{
		get
		{
			CheckDisposed();
			if (base.Count == 0)
			{
				return null;
			}
			IRange range = base.InnerList[0];
			string formulaR1C = range.FormulaR1C1;
			int i = 0;
			for (int count = base.Count; i < count; i++)
			{
				range = base.InnerList[i];
				if (formulaR1C != range.FormulaR1C1)
				{
					return null;
				}
			}
			return formulaR1C;
		}
		set
		{
			CheckDisposed();
			int i = 0;
			for (int count = base.Count; i < count; i++)
			{
				base.InnerList[i].FormulaR1C1 = value;
			}
		}
	}

	public string FormulaArray
	{
		get
		{
			CheckDisposed();
			if (base.Count == 0)
			{
				return null;
			}
			IRange range = base.InnerList[0];
			string formulaArray = range.FormulaArray;
			int i = 0;
			for (int count = base.Count; i < count; i++)
			{
				range = base.InnerList[i];
				if (formulaArray != range.FormulaArray)
				{
					return null;
				}
			}
			return formulaArray;
		}
		set
		{
			CheckDisposed();
			int i = 0;
			for (int count = base.Count; i < count; i++)
			{
				base.InnerList[i].FormulaArray = value;
			}
		}
	}

	public string FormulaArrayR1C1
	{
		get
		{
			CheckDisposed();
			if (base.Count == 0)
			{
				return null;
			}
			IRange range = base.InnerList[0];
			string formulaArrayR1C = range.FormulaArrayR1C1;
			int i = 0;
			for (int count = base.Count; i < count; i++)
			{
				range = base.InnerList[i];
				if (formulaArrayR1C != range.FormulaArrayR1C1)
				{
					return null;
				}
			}
			return formulaArrayR1C;
		}
		set
		{
			CheckDisposed();
			int i = 0;
			for (int count = base.Count; i < count; i++)
			{
				base.InnerList[i].FormulaArrayR1C1 = value;
			}
		}
	}

	public bool FormulaHidden
	{
		get
		{
			CheckDisposed();
			if (base.Count == 0)
			{
				return false;
			}
			IRange range = base.InnerList[0];
			bool formulaHidden = range.FormulaHidden;
			int i = 0;
			for (int count = base.Count; i < count; i++)
			{
				range = base.InnerList[i];
				if (formulaHidden != range.FormulaHidden)
				{
					return false;
				}
			}
			return formulaHidden;
		}
		set
		{
			CheckDisposed();
			int i = 0;
			for (int count = base.Count; i < count; i++)
			{
				base.InnerList[i].FormulaHidden = value;
			}
		}
	}

	public DateTime FormulaDateTime
	{
		get
		{
			CheckDisposed();
			if (base.Count == 0)
			{
				return DateTime.MinValue;
			}
			IRange range = base.InnerList[0];
			DateTime formulaDateTime = range.FormulaDateTime;
			int i = 0;
			for (int count = base.Count; i < count; i++)
			{
				range = base.InnerList[i];
				if (formulaDateTime != range.FormulaDateTime)
				{
					return DateTime.MinValue;
				}
			}
			return formulaDateTime;
		}
		set
		{
			CheckDisposed();
			int i = 0;
			for (int count = base.Count; i < count; i++)
			{
				base.InnerList[i].FormulaDateTime = value;
			}
		}
	}

	public bool HasDataValidation
	{
		get
		{
			CheckDisposed();
			if (base.Count == 0)
			{
				return false;
			}
			_ = base.InnerList[0];
			return false;
		}
	}

	public bool HasBoolean
	{
		get
		{
			CheckDisposed();
			if (base.Count == 0)
			{
				return false;
			}
			IRange range = base.InnerList[0];
			bool hasBoolean = range.HasBoolean;
			int i = 0;
			for (int count = base.Count; i < count; i++)
			{
				range = base.InnerList[i];
				if (hasBoolean != range.HasBoolean)
				{
					return false;
				}
			}
			return hasBoolean;
		}
	}

	public bool HasDateTime
	{
		get
		{
			CheckDisposed();
			if (base.Count == 0)
			{
				return false;
			}
			IRange range = base.InnerList[0];
			bool hasDateTime = range.HasDateTime;
			int i = 0;
			for (int count = base.Count; i < count; i++)
			{
				range = base.InnerList[i];
				if (hasDateTime != range.HasDateTime)
				{
					return false;
				}
			}
			return hasDateTime;
		}
	}

	public bool HasFormulaBoolValue
	{
		get
		{
			CheckDisposed();
			if (base.Count == 0)
			{
				return false;
			}
			IRange range = base.InnerList[0];
			bool hasFormulaBoolValue = range.HasFormulaBoolValue;
			int i = 0;
			for (int count = base.Count; i < count; i++)
			{
				range = base.InnerList[i];
				if (hasFormulaBoolValue != range.HasFormulaBoolValue)
				{
					return false;
				}
			}
			return hasFormulaBoolValue;
		}
	}

	public bool HasFormulaErrorValue
	{
		get
		{
			CheckDisposed();
			if (base.Count == 0)
			{
				return false;
			}
			IRange range = base.InnerList[0];
			bool hasFormulaErrorValue = range.HasFormulaErrorValue;
			int i = 0;
			for (int count = base.Count; i < count; i++)
			{
				range = base.InnerList[i];
				if (hasFormulaErrorValue != range.HasFormulaErrorValue)
				{
					return false;
				}
			}
			return hasFormulaErrorValue;
		}
	}

	public bool HasFormulaDateTime
	{
		get
		{
			CheckDisposed();
			if (base.Count == 0)
			{
				return false;
			}
			IRange range = base.InnerList[0];
			bool hasFormulaDateTime = range.HasFormulaDateTime;
			int i = 0;
			for (int count = base.Count; i < count; i++)
			{
				range = base.InnerList[i];
				if (hasFormulaDateTime != range.HasFormulaDateTime)
				{
					return false;
				}
			}
			return hasFormulaDateTime;
		}
	}

	public bool HasFormulaNumberValue
	{
		get
		{
			CheckDisposed();
			if (base.Count == 0)
			{
				return false;
			}
			IRange range = base.InnerList[0];
			bool hasFormulaNumberValue = range.HasFormulaNumberValue;
			int i = 0;
			for (int count = base.Count; i < count; i++)
			{
				range = base.InnerList[i];
				if (hasFormulaNumberValue != range.HasFormulaNumberValue)
				{
					return false;
				}
			}
			return hasFormulaNumberValue;
		}
	}

	public bool HasFormulaStringValue
	{
		get
		{
			CheckDisposed();
			if (base.Count == 0)
			{
				return false;
			}
			IRange range = base.InnerList[0];
			bool hasFormulaStringValue = range.HasFormulaStringValue;
			int i = 0;
			for (int count = base.Count; i < count; i++)
			{
				range = base.InnerList[i];
				if (hasFormulaStringValue != range.HasFormulaStringValue)
				{
					return false;
				}
			}
			return hasFormulaStringValue;
		}
	}

	public bool HasFormula
	{
		get
		{
			CheckDisposed();
			if (base.Count == 0)
			{
				return false;
			}
			IRange range = base.InnerList[0];
			bool hasFormula = range.HasFormula;
			int i = 0;
			for (int count = base.Count; i < count; i++)
			{
				range = base.InnerList[i];
				if (hasFormula != range.HasFormula)
				{
					return false;
				}
			}
			return hasFormula;
		}
	}

	public bool HasFormulaArray
	{
		get
		{
			CheckDisposed();
			if (base.Count == 0)
			{
				return false;
			}
			IRange range = base.InnerList[0];
			bool hasFormulaArray = range.HasFormulaArray;
			int i = 0;
			for (int count = base.Count; i < count; i++)
			{
				range = base.InnerList[i];
				if (hasFormulaArray != range.HasFormulaArray)
				{
					return false;
				}
			}
			return hasFormulaArray;
		}
	}

	public bool HasNumber
	{
		get
		{
			CheckDisposed();
			if (base.Count == 0)
			{
				return false;
			}
			IRange range = base.InnerList[0];
			bool hasNumber = range.HasNumber;
			int i = 0;
			for (int count = base.Count; i < count; i++)
			{
				range = base.InnerList[i];
				if (hasNumber != range.HasNumber)
				{
					return false;
				}
			}
			return hasNumber;
		}
	}

	public bool HasRichText
	{
		get
		{
			CheckDisposed();
			if (base.Count == 0)
			{
				return false;
			}
			IRange range = base.InnerList[0];
			bool hasRichText = range.HasRichText;
			int i = 0;
			for (int count = base.Count; i < count; i++)
			{
				range = base.InnerList[i];
				if (hasRichText != range.HasRichText)
				{
					return false;
				}
			}
			return hasRichText;
		}
	}

	public bool HasString
	{
		get
		{
			CheckDisposed();
			if (base.Count == 0)
			{
				return false;
			}
			IRange range = base.InnerList[0];
			bool hasString = range.HasString;
			int i = 0;
			for (int count = base.Count; i < count; i++)
			{
				range = base.InnerList[i];
				if (hasString != range.HasString)
				{
					return false;
				}
			}
			return hasString;
		}
	}

	public bool HasStyle
	{
		get
		{
			CheckDisposed();
			if (base.Count == 0)
			{
				return false;
			}
			IRange range = base.InnerList[0];
			bool hasStyle = range.HasStyle;
			int i = 0;
			for (int count = base.Count; i < count; i++)
			{
				range = base.InnerList[i];
				if (hasStyle != range.HasStyle)
				{
					return false;
				}
			}
			return hasStyle;
		}
	}

	public OfficeHAlign HorizontalAlignment
	{
		get
		{
			CheckDisposed();
			if (base.Count == 0)
			{
				return OfficeHAlign.HAlignGeneral;
			}
			IRange range = base.InnerList[0];
			OfficeHAlign horizontalAlignment = range.HorizontalAlignment;
			int i = 0;
			for (int count = base.Count; i < count; i++)
			{
				range = base.InnerList[i];
				if (horizontalAlignment != range.HorizontalAlignment)
				{
					return OfficeHAlign.HAlignGeneral;
				}
			}
			return horizontalAlignment;
		}
		set
		{
			CheckDisposed();
			int i = 0;
			for (int count = base.Count; i < count; i++)
			{
				base.InnerList[i].HorizontalAlignment = value;
			}
		}
	}

	public int IndentLevel
	{
		get
		{
			CheckDisposed();
			if (base.Count == 0)
			{
				return int.MinValue;
			}
			IRange range = base.InnerList[0];
			int indentLevel = range.IndentLevel;
			int i = 0;
			for (int count = base.Count; i < count; i++)
			{
				range = base.InnerList[i];
				if (indentLevel != range.IndentLevel)
				{
					return int.MinValue;
				}
			}
			return indentLevel;
		}
		set
		{
			CheckDisposed();
			int i = 0;
			for (int count = base.Count; i < count; i++)
			{
				base.InnerList[i].IndentLevel = value;
			}
		}
	}

	public bool IsBlank
	{
		get
		{
			CheckDisposed();
			if (base.Count == 0)
			{
				return false;
			}
			IRange range = base.InnerList[0];
			bool isBlank = range.IsBlank;
			int i = 0;
			for (int count = base.Count; i < count; i++)
			{
				range = base.InnerList[i];
				if (isBlank != range.IsBlank)
				{
					return false;
				}
			}
			return isBlank;
		}
	}

	public bool IsBoolean
	{
		get
		{
			CheckDisposed();
			if (base.Count == 0)
			{
				return false;
			}
			IRange range = base.InnerList[0];
			bool isBoolean = range.IsBoolean;
			int i = 0;
			for (int count = base.Count; i < count; i++)
			{
				range = base.InnerList[i];
				if (isBoolean != range.IsBoolean)
				{
					return false;
				}
			}
			return isBoolean;
		}
	}

	public bool IsError
	{
		get
		{
			CheckDisposed();
			if (base.Count == 0)
			{
				return false;
			}
			IRange range = base.InnerList[0];
			bool isError = range.IsError;
			int i = 0;
			for (int count = base.Count; i < count; i++)
			{
				range = base.InnerList[i];
				if (isError != range.IsError)
				{
					return false;
				}
			}
			return isError;
		}
	}

	public bool IsGroupedByColumn
	{
		get
		{
			CheckDisposed();
			if (base.Count == 0)
			{
				return false;
			}
			IRange range = base.InnerList[0];
			bool isGroupedByColumn = range.IsGroupedByColumn;
			int i = 0;
			for (int count = base.Count; i < count; i++)
			{
				range = base.InnerList[i];
				if (isGroupedByColumn != range.IsGroupedByColumn)
				{
					return false;
				}
			}
			return isGroupedByColumn;
		}
	}

	public bool IsGroupedByRow
	{
		get
		{
			CheckDisposed();
			if (base.Count == 0)
			{
				return false;
			}
			IRange range = base.InnerList[0];
			bool isGroupedByRow = range.IsGroupedByRow;
			int i = 0;
			for (int count = base.Count; i < count; i++)
			{
				range = base.InnerList[i];
				if (isGroupedByRow != range.IsGroupedByRow)
				{
					return false;
				}
			}
			return isGroupedByRow;
		}
	}

	public bool IsInitialized
	{
		get
		{
			CheckDisposed();
			if (base.Count == 0)
			{
				return false;
			}
			IRange range = base.InnerList[0];
			bool isInitialized = range.IsInitialized;
			int i = 0;
			for (int count = base.Count; i < count; i++)
			{
				range = base.InnerList[i];
				if (isInitialized != range.IsInitialized)
				{
					return false;
				}
			}
			return isInitialized;
		}
	}

	public int LastColumn => m_iLastColumn;

	public int LastRow => m_iLastRow;

	public double Number
	{
		get
		{
			CheckDisposed();
			if (base.Count == 0)
			{
				return double.MinValue;
			}
			IRange range = base.InnerList[0];
			double number = range.Number;
			int i = 0;
			for (int count = base.Count; i < count; i++)
			{
				range = base.InnerList[i];
				if (number != range.Number)
				{
					return double.MinValue;
				}
			}
			return number;
		}
		set
		{
			CheckDisposed();
			int i = 0;
			for (int count = base.Count; i < count; i++)
			{
				base.InnerList[i].Number = value;
			}
		}
	}

	public string NumberFormat
	{
		get
		{
			CheckDisposed();
			return RangeImpl.GetNumberFormat(base.InnerList);
		}
		set
		{
			CheckDisposed();
			int i = 0;
			for (int count = base.Count; i < count; i++)
			{
				base.InnerList[i].NumberFormat = value;
			}
		}
	}

	public int Row
	{
		get
		{
			CheckDisposed();
			return m_iFirstRow;
		}
	}

	public int RowGroupLevel
	{
		get
		{
			CheckDisposed();
			if (base.Count == 0)
			{
				return int.MinValue;
			}
			IRange range = base.InnerList[0];
			int rowGroupLevel = range.RowGroupLevel;
			int i = 0;
			for (int count = base.Count; i < count; i++)
			{
				range = base.InnerList[i];
				if (rowGroupLevel != range.RowGroupLevel)
				{
					return int.MinValue;
				}
			}
			return rowGroupLevel;
		}
	}

	public double RowHeight
	{
		get
		{
			CheckDisposed();
			if (base.Count == 0)
			{
				return double.MinValue;
			}
			IRange range = base.InnerList[0];
			double rowHeight = range.RowHeight;
			int i = 0;
			for (int count = base.Count; i < count; i++)
			{
				range = base.InnerList[i];
				if (rowHeight != range.RowHeight)
				{
					return double.MinValue;
				}
			}
			return rowHeight;
		}
		set
		{
			CheckDisposed();
			int i = 0;
			for (int count = base.Count; i < count; i++)
			{
				base.InnerList[i].RowHeight = value;
			}
		}
	}

	public IRange[] Rows
	{
		get
		{
			CheckDisposed();
			return GetColumnRows(bIsColumn: false);
		}
	}

	public IRange[] Columns
	{
		get
		{
			CheckDisposed();
			return GetColumnRows(bIsColumn: true);
		}
	}

	public IStyle CellStyle
	{
		get
		{
			CheckDisposed();
			return new StyleArrayWrapper(this);
		}
		set
		{
			CheckDisposed();
			if (value == null)
			{
				throw new ArgumentNullException("CellStyle");
			}
			CellStyleName = value.Name;
		}
	}

	public string CellStyleName
	{
		get
		{
			CheckDisposed();
			return RangeImpl.GetCellStyleName(base.List);
		}
		set
		{
			CheckDisposed();
			int i = 0;
			for (int count = base.Count; i < count; i++)
			{
				base.InnerList[i].CellStyleName = value;
			}
		}
	}

	public string Text
	{
		get
		{
			CheckDisposed();
			if (base.Count == 0)
			{
				return null;
			}
			IRange range = base.InnerList[0];
			string text = range.Text;
			int i = 0;
			for (int count = base.Count; i < count; i++)
			{
				range = base.InnerList[i];
				if (text != range.Text)
				{
					return null;
				}
			}
			return text;
		}
		set
		{
			CheckDisposed();
			int i = 0;
			for (int count = base.Count; i < count; i++)
			{
				base.InnerList[i].Text = value;
			}
		}
	}

	public TimeSpan TimeSpan
	{
		get
		{
			CheckDisposed();
			if (base.Count == 0)
			{
				return TimeSpan.MinValue;
			}
			IRange range = base.InnerList[0];
			TimeSpan timeSpan = range.TimeSpan;
			int i = 0;
			for (int count = base.Count; i < count; i++)
			{
				range = base.InnerList[i];
				if (timeSpan != range.TimeSpan)
				{
					return TimeSpan.MinValue;
				}
			}
			return timeSpan;
		}
		set
		{
			CheckDisposed();
			int i = 0;
			for (int count = base.Count; i < count; i++)
			{
				base.InnerList[i].TimeSpan = value;
			}
		}
	}

	public string Value
	{
		get
		{
			CheckDisposed();
			if (base.Count == 0)
			{
				return null;
			}
			IRange range = base.InnerList[0];
			string value = range.Value;
			int i = 0;
			for (int count = base.Count; i < count; i++)
			{
				range = base.InnerList[i];
				if (value != range.Value)
				{
					return null;
				}
			}
			return value;
		}
		set
		{
			CheckDisposed();
			int i = 0;
			for (int count = base.Count; i < count; i++)
			{
				base.InnerList[i].Value = value;
			}
		}
	}

	public object Value2
	{
		get
		{
			CheckDisposed();
			if (base.Count == 0)
			{
				return null;
			}
			IRange range = base.InnerList[0];
			object value = range.Value2;
			int i = 0;
			for (int count = base.Count; i < count; i++)
			{
				range = base.InnerList[i];
				if (!value.Equals(range.Value2))
				{
					return null;
				}
			}
			return value;
		}
		set
		{
			CheckDisposed();
			int i = 0;
			for (int count = base.Count; i < count; i++)
			{
				base.InnerList[i].Value2 = value;
			}
		}
	}

	public OfficeVAlign VerticalAlignment
	{
		get
		{
			CheckDisposed();
			if (base.Count == 0)
			{
				return OfficeVAlign.VAlignTop;
			}
			IRange range = base.InnerList[0];
			OfficeVAlign verticalAlignment = range.VerticalAlignment;
			int i = 0;
			for (int count = base.Count; i < count; i++)
			{
				range = base.InnerList[i];
				if (verticalAlignment != range.VerticalAlignment)
				{
					return OfficeVAlign.VAlignTop;
				}
			}
			return verticalAlignment;
		}
		set
		{
			CheckDisposed();
			int i = 0;
			for (int count = base.Count; i < count; i++)
			{
				base.InnerList[i].VerticalAlignment = value;
			}
		}
	}

	public IWorksheet Worksheet
	{
		get
		{
			CheckDisposed();
			return m_worksheet;
		}
	}

	public IRange this[int row, int column]
	{
		get
		{
			CheckDisposed();
			return Worksheet.UsedRange[row, column];
		}
		set
		{
			CheckDisposed();
			Worksheet.UsedRange[row, column] = value;
		}
	}

	public IRange this[int row, int column, int lastRow, int lastColumn]
	{
		get
		{
			CheckDisposed();
			return Worksheet.UsedRange[row, column, lastRow, lastColumn];
		}
	}

	public IRange this[string name] => this[name, false];

	public IRange this[string name, bool IsR1C1Notation]
	{
		get
		{
			CheckDisposed();
			return Worksheet.UsedRange[name, IsR1C1Notation];
		}
	}

	public string FormulaStringValue
	{
		get
		{
			CheckDisposed();
			if (base.Count == 0)
			{
				return null;
			}
			IRange range = base.InnerList[0];
			string formulaStringValue = range.FormulaStringValue;
			int i = 0;
			for (int count = base.Count; i < count; i++)
			{
				range = base.InnerList[i];
				if (formulaStringValue != range.FormulaStringValue)
				{
					return null;
				}
			}
			return formulaStringValue;
		}
		set
		{
			CheckDisposed();
			int i = 0;
			for (int count = base.Count; i < count; i++)
			{
				base.InnerList[i].FormulaStringValue = value;
			}
		}
	}

	public double FormulaNumberValue
	{
		get
		{
			CheckDisposed();
			if (base.Count == 0)
			{
				return double.MinValue;
			}
			IRange range = base.InnerList[0];
			double formulaNumberValue = range.FormulaNumberValue;
			int i = 0;
			for (int count = base.Count; i < count; i++)
			{
				range = base.InnerList[i];
				if (formulaNumberValue != range.FormulaNumberValue)
				{
					return double.MinValue;
				}
			}
			return formulaNumberValue;
		}
		set
		{
			CheckDisposed();
			int i = 0;
			for (int count = base.Count; i < count; i++)
			{
				base.InnerList[i].FormulaNumberValue = value;
			}
		}
	}

	public bool FormulaBoolValue
	{
		get
		{
			CheckDisposed();
			if (base.Count == 0)
			{
				return false;
			}
			IRange range = base.InnerList[0];
			bool formulaBoolValue = range.FormulaBoolValue;
			int i = 1;
			for (int count = base.Count; i < count; i++)
			{
				range = base.InnerList[i];
				if (formulaBoolValue != range.FormulaBoolValue)
				{
					return false;
				}
			}
			return formulaBoolValue;
		}
		set
		{
			CheckDisposed();
			int i = 0;
			for (int count = base.Count; i < count; i++)
			{
				base.InnerList[i].FormulaBoolValue = value;
			}
		}
	}

	public string FormulaErrorValue
	{
		get
		{
			CheckDisposed();
			if (base.Count == 0)
			{
				return null;
			}
			IRange range = base.InnerList[0];
			string formulaErrorValue = range.FormulaErrorValue;
			int i = 1;
			for (int count = base.Count; i < count; i++)
			{
				range = base.InnerList[i];
				if (formulaErrorValue != range.FormulaErrorValue)
				{
					return null;
				}
			}
			return formulaErrorValue;
		}
		set
		{
			CheckDisposed();
			int i = 0;
			for (int count = base.Count; i < count; i++)
			{
				base.InnerList[i].FormulaErrorValue = value;
			}
		}
	}

	public IRichTextString RichText
	{
		get
		{
			CheckDisposed();
			if (m_rtfString == null)
			{
				m_rtfString = new RTFStringArray(this);
			}
			return m_rtfString;
		}
	}

	public bool IsMerged
	{
		get
		{
			CheckDisposed();
			if (base.Count == 0)
			{
				return false;
			}
			IRange range = base.InnerList[0];
			bool isMerged = range.IsMerged;
			int i = 0;
			for (int count = base.Count; i < count; i++)
			{
				range = base.InnerList[i];
				if (isMerged != range.IsMerged)
				{
					return false;
				}
			}
			return isMerged;
		}
	}

	public IRange MergeArea
	{
		get
		{
			CheckDisposed();
			RangesCollection rangesCollection = base.AppImplementation.CreateRangesCollection(Worksheet);
			int i = 0;
			for (int count = base.Count; i < count; i++)
			{
				RangeImpl rangeImpl = (RangeImpl)base.InnerList[i];
				rangesCollection.Add(rangeImpl.MergeArea);
			}
			return rangesCollection;
		}
	}

	public bool WrapText
	{
		get
		{
			CheckDisposed();
			return RangeImpl.GetWrapText(Cells);
		}
		set
		{
			CheckDisposed();
			RangeImpl.SetWrapText(Cells, value);
		}
	}

	public bool HasExternalFormula
	{
		get
		{
			int i = 0;
			for (int count = base.Count; i < count; i++)
			{
				if (!base.InnerList[i].HasExternalFormula)
				{
					return false;
				}
			}
			return true;
		}
	}

	public ExcelIgnoreError IgnoreErrorOptions
	{
		get
		{
			CheckDisposed();
			if (base.Count == 0)
			{
				return ExcelIgnoreError.None;
			}
			_ = base.InnerList;
			return ExcelIgnoreError.All;
		}
		set
		{
			CheckDisposed();
			_ = base.InnerList;
		}
	}

	public bool? IsStringsPreserved
	{
		get
		{
			return (m_worksheet as WorksheetImpl).GetStringPreservedValue(this);
		}
		set
		{
			(m_worksheet as WorksheetImpl).SetStringPreservedValue(this, value);
		}
	}

	public BuiltInStyles? BuiltInStyle
	{
		get
		{
			CheckDisposed();
			if (base.Count == 0)
			{
				return null;
			}
			BuiltInStyles? builtInStyles = base.InnerList[0].BuiltInStyle;
			int i = 0;
			for (int count = base.Count; i < count; i++)
			{
				if (builtInStyles != base.InnerList[i].BuiltInStyle)
				{
					builtInStyles = null;
					break;
				}
			}
			return builtInStyles;
		}
		set
		{
			CheckDisposed();
			int i = 0;
			for (int count = base.Count; i < count; i++)
			{
				base.InnerList[i].BuiltInStyle = value;
			}
		}
	}

	public string AddressGlobal2007 => AddressGlobal;

	public int CellsCount
	{
		get
		{
			int num = 0;
			int i = 0;
			for (int count = base.Count; i < count; i++)
			{
				ICombinedRange combinedRange = (ICombinedRange)base.InnerList[i];
				num += combinedRange.CellsCount;
			}
			return num;
		}
	}

	public string WorksheetName => Worksheet.Name;

	public new IRange this[int index]
	{
		get
		{
			CheckDisposed();
			return base.InnerList[index];
		}
		set
		{
			CheckDisposed();
			if (value == null)
			{
				throw new ArgumentNullException();
			}
			base.InnerList[index] = value;
		}
	}

	public RangesCollection(IApplication application, object parent)
		: base(application, parent)
	{
		SetParents();
		m_iFirstRow = m_worksheet.Workbook.MaxRowCount + 1;
		m_iFirstColumn = m_worksheet.Workbook.MaxColumnCount + 1;
	}

	private void SetParents()
	{
		m_worksheet = FindParent(typeof(IWorksheet)) as IWorksheet;
		if (m_worksheet == null)
		{
			throw new ArgumentNullException("Worksheet", "Can't find parent worksheet");
		}
	}

	public IRange Activate()
	{
		CheckDisposed();
		return null;
	}

	public IRange Activate(bool scroll)
	{
		CheckDisposed();
		return null;
	}

	public IRange Group(OfficeGroupBy groupBy)
	{
		CheckDisposed();
		int i = 0;
		for (int count = base.Count; i < count; i++)
		{
			base.InnerList[i].Group(groupBy);
		}
		return this;
	}

	public IRange Group(OfficeGroupBy groupBy, bool bCollapsed)
	{
		CheckDisposed();
		int i = 0;
		for (int count = base.Count; i < count; i++)
		{
			base.InnerList[i].Group(groupBy, bCollapsed);
		}
		return this;
	}

	public void SubTotal(int groupBy, ConsolidationFunction function, int[] totalList)
	{
		throw new NotSupportedException();
	}

	public void SubTotal(int groupBy, ConsolidationFunction function, int[] totalList, bool replace, bool pageBreaks, bool summaryBelowData)
	{
		throw new NotSupportedException();
	}

	public void Merge()
	{
		CheckDisposed();
		int i = 0;
		for (int count = base.Count; i < count; i++)
		{
			base.InnerList[i].Merge();
		}
	}

	public void Merge(bool clearCells)
	{
		CheckDisposed();
		int i = 0;
		for (int count = base.Count; i < count; i++)
		{
			base.InnerList[i].Merge(clearCells);
		}
	}

	public IRange Ungroup(OfficeGroupBy groupBy)
	{
		CheckDisposed();
		int i = 0;
		for (int count = base.Count; i < count; i++)
		{
			base.InnerList[i].Ungroup(groupBy);
		}
		return this;
	}

	public void UnMerge()
	{
		CheckDisposed();
		int i = 0;
		for (int count = base.Count; i < count; i++)
		{
			base.InnerList[i].UnMerge();
		}
	}

	public void FreezePanes()
	{
		CheckDisposed();
		if (base.Count == 1)
		{
			base.InnerList[0].FreezePanes();
		}
	}

	void IRange.Clear()
	{
		CheckDisposed();
		int i = 0;
		for (int count = base.Count; i < count; i++)
		{
			base.InnerList[i].Clear();
		}
	}

	void IRange.Clear(bool isClearFormat)
	{
		CheckDisposed();
		int i = 0;
		for (int count = base.Count; i < count; i++)
		{
			base.InnerList[i].Clear(isClearFormat);
		}
	}

	void IRange.Clear(OfficeClearOptions option)
	{
		CheckDisposed();
		int i = 0;
		for (int count = base.Count; i < count; i++)
		{
			base.InnerList[i].Clear(option);
		}
	}

	void IRange.Clear(OfficeMoveDirection direction)
	{
		CheckDisposed();
		int i = 0;
		for (int count = base.Count; i < count; i++)
		{
			base.InnerList[i].Clear(direction);
		}
	}

	void IRange.Clear(OfficeMoveDirection direction, OfficeCopyRangeOptions options)
	{
		CheckDisposed();
		int i = 0;
		for (int count = base.Count; i < count; i++)
		{
			base.InnerList[i].Clear(direction, options);
		}
	}

	public void MoveTo(IRange destination)
	{
		CheckDisposed();
		if (destination == null)
		{
			throw new ArgumentNullException("destination");
		}
		int num = destination.Row - Row;
		int num2 = destination.Column - Column;
		int i = 0;
		for (int count = base.Count; i < count; i++)
		{
			IRange range = base.InnerList[i];
			int num3 = range.Row + num;
			int num4 = range.Column + num2;
			if (num3 <= m_worksheet.Workbook.MaxRowCount && num3 > 0 && num4 <= m_worksheet.Workbook.MaxColumnCount && num4 > 0)
			{
				range.MoveTo(destination.Worksheet[num3, num4]);
			}
		}
	}

	public IRange CopyTo(IRange destination)
	{
		return CopyTo(destination, OfficeCopyRangeOptions.All);
	}

	public IRange CopyTo(IRange destination, OfficeCopyRangeOptions options)
	{
		CheckDisposed();
		if (destination == null)
		{
			throw new ArgumentNullException("destination");
		}
		int num = destination.Row - Row;
		int num2 = destination.Column - Column;
		int i = 0;
		for (int count = base.Count; i < count; i++)
		{
			IRange range = base.InnerList[i];
			int num3 = range.Row + num;
			int num4 = range.Column + num2;
			if (num3 <= m_worksheet.Workbook.MaxRowCount && num3 > 0 && num4 <= m_worksheet.Workbook.MaxColumnCount && num4 > 0)
			{
				range.CopyTo(destination.Worksheet[num3, num4], options);
			}
		}
		return destination;
	}

	public IRange IntersectWith(IRange range)
	{
		CheckDisposed();
		RangesCollection rangesCollection = base.AppImplementation.CreateRangesCollection(Worksheet);
		int i = 0;
		for (int count = rangesCollection.Count; i < count; i++)
		{
			IRange range2 = base.InnerList[i];
			rangesCollection.Add(range2.IntersectWith(range));
		}
		if (rangesCollection.Count <= 0)
		{
			return null;
		}
		return rangesCollection;
	}

	public IRange MergeWith(IRange range)
	{
		throw new NotImplementedException();
	}

	public void AutofitRows()
	{
	}

	public void AutofitColumns()
	{
	}

	public IRange FindFirst(string findValue, OfficeFindType flags)
	{
		CheckDisposed();
		if (findValue == null)
		{
			return null;
		}
		bool num = (flags & OfficeFindType.Formula) == OfficeFindType.Formula;
		bool flag = (flags & OfficeFindType.Text) == OfficeFindType.Text;
		bool flag2 = (flags & OfficeFindType.FormulaStringValue) == OfficeFindType.FormulaStringValue;
		bool flag3 = (flags & OfficeFindType.Error) == OfficeFindType.Error;
		if (!(num || flag || flag2 || flag3))
		{
			throw new ArgumentException("Parameter flag is not valid.", "flags");
		}
		IList innerList = base.InnerList;
		int i = 0;
		for (int count = innerList.Count; i < count; i++)
		{
			IRange range = ((IRange)innerList[i]).FindFirst(findValue, flags);
			if (range != null)
			{
				return range;
			}
		}
		return null;
	}

	public IRange FindFirst(double findValue, OfficeFindType flags)
	{
		CheckDisposed();
		bool num = (flags & OfficeFindType.FormulaValue) == OfficeFindType.FormulaValue;
		bool flag = (flags & OfficeFindType.Number) == OfficeFindType.Number;
		if (!(num || flag))
		{
			throw new ArgumentException("Parameter flag is not valid.", "flags");
		}
		IList innerList = base.InnerList;
		int i = 0;
		for (int count = innerList.Count; i < count; i++)
		{
			IRange range = ((IRange)innerList[i]).FindFirst(findValue, flags);
			if (range != null)
			{
				return range;
			}
		}
		return null;
	}

	public IRange FindFirst(bool findValue)
	{
		CheckDisposed();
		IList innerList = base.InnerList;
		int i = 0;
		for (int count = innerList.Count; i < count; i++)
		{
			IRange range = ((IRange)innerList[i]).FindFirst(findValue);
			if (range != null)
			{
				return range;
			}
		}
		return null;
	}

	public IRange FindFirst(DateTime findValue)
	{
		CheckDisposed();
		IList innerList = base.InnerList;
		int i = 0;
		for (int count = innerList.Count; i < count; i++)
		{
			IRange range = ((IRange)innerList[i]).FindFirst(findValue);
			if (range != null)
			{
				return range;
			}
		}
		return null;
	}

	public IRange FindFirst(TimeSpan findValue)
	{
		CheckDisposed();
		IList innerList = base.InnerList;
		int i = 0;
		for (int count = innerList.Count; i < count; i++)
		{
			IRange range = ((IRange)innerList[i]).FindFirst(findValue);
			if (range != null)
			{
				return range;
			}
		}
		return null;
	}

	public IRange[] FindAll(DateTime findValue)
	{
		CheckDisposed();
		List<IRange> list = new List<IRange>();
		IList<IRange> innerList = base.InnerList;
		int i = 0;
		for (int count = innerList.Count; i < count; i++)
		{
			IRange[] array = innerList[i].FindAll(findValue);
			if (array != null)
			{
				list.AddRange(array);
			}
		}
		if (list.Count == 0)
		{
			return null;
		}
		return list.ToArray();
	}

	public IRange[] FindAll(TimeSpan findValue)
	{
		CheckDisposed();
		List<IRange> list = new List<IRange>();
		List<IRange> innerList = base.InnerList;
		int i = 0;
		for (int count = innerList.Count; i < count; i++)
		{
			IRange[] array = innerList[i].FindAll(findValue);
			if (array != null)
			{
				list.AddRange(array);
			}
		}
		if (list.Count == 0)
		{
			return null;
		}
		return list.ToArray();
	}

	public IRange[] FindAll(string findValue, OfficeFindType flags)
	{
		CheckDisposed();
		if (findValue == null)
		{
			return null;
		}
		bool num = (flags & OfficeFindType.Formula) == OfficeFindType.Formula;
		bool flag = (flags & OfficeFindType.Text) == OfficeFindType.Text;
		bool flag2 = (flags & OfficeFindType.FormulaStringValue) == OfficeFindType.FormulaStringValue;
		bool flag3 = (flags & OfficeFindType.Error) == OfficeFindType.Error;
		if (!(num || flag || flag2 || flag3))
		{
			throw new ArgumentException("Parameter flag is not valid.", "flags");
		}
		if (findValue == null)
		{
			return null;
		}
		List<IRange> list = new List<IRange>();
		List<IRange> innerList = base.InnerList;
		int i = 0;
		for (int count = innerList.Count; i < count; i++)
		{
			IRange[] array = innerList[i].FindAll(findValue, flags);
			if (array != null)
			{
				list.AddRange(array);
			}
		}
		if (list.Count == 0)
		{
			return null;
		}
		return list.ToArray();
	}

	public IRange[] FindAll(double findValue, OfficeFindType flags)
	{
		CheckDisposed();
		bool num = (flags & OfficeFindType.FormulaValue) == OfficeFindType.FormulaValue;
		bool flag = (flags & OfficeFindType.Number) == OfficeFindType.Number;
		if (!(num || flag))
		{
			throw new ArgumentException("Parameter flag is not valid.", "flags");
		}
		List<IRange> list = new List<IRange>();
		List<IRange> innerList = base.InnerList;
		int i = 0;
		for (int count = innerList.Count; i < count; i++)
		{
			IRange[] array = innerList[i].FindAll(findValue, flags);
			if (array != null)
			{
				list.AddRange(array);
			}
		}
		if (list.Count == 0)
		{
			return null;
		}
		return list.ToArray();
	}

	public IRange[] FindAll(bool findValue)
	{
		CheckDisposed();
		List<IRange> list = new List<IRange>();
		List<IRange> innerList = base.InnerList;
		int i = 0;
		for (int count = innerList.Count; i < count; i++)
		{
			IRange[] array = innerList[i].FindAll(findValue);
			if (array != null)
			{
				list.AddRange(array);
			}
		}
		if (list.Count == 0)
		{
			return null;
		}
		return list.ToArray();
	}

	public void CopyToClipboard()
	{
		throw new NotSupportedException();
	}

	public void BorderAround()
	{
		BorderAround(OfficeLineStyle.Thin);
	}

	public void BorderAround(OfficeLineStyle borderLine)
	{
		BorderAround(borderLine, OfficeKnownColors.Black);
	}

	public void BorderAround(OfficeLineStyle borderLine, Color borderColor)
	{
		OfficeKnownColors nearestColor = m_worksheet.Workbook.GetNearestColor(borderColor);
		BorderAround(borderLine, nearestColor);
	}

	public void BorderAround(OfficeLineStyle borderLine, OfficeKnownColors borderColor)
	{
		int i = 0;
		for (int count = base.Count; i < count; i++)
		{
			base.InnerList[i].BorderAround(borderLine, borderColor);
		}
	}

	public void BorderInside()
	{
		BorderInside(OfficeLineStyle.Thin);
	}

	public void BorderInside(OfficeLineStyle borderLine)
	{
		BorderInside(borderLine, OfficeKnownColors.Black);
	}

	public void BorderInside(OfficeLineStyle borderLine, Color borderColor)
	{
		OfficeKnownColors nearestColor = m_worksheet.Workbook.GetNearestColor(borderColor);
		BorderInside(borderLine, nearestColor);
	}

	public void BorderInside(OfficeLineStyle borderLine, OfficeKnownColors borderColor)
	{
		int i = 0;
		for (int count = base.Count; i < count; i++)
		{
			base.InnerList[i].BorderInside(borderLine, borderColor);
		}
	}

	public void BorderNone()
	{
		int i = 0;
		for (int count = base.Count; i < count; i++)
		{
			base.InnerList[i].BorderNone();
		}
	}

	public void CollapseGroup(OfficeGroupBy groupBy)
	{
		int i = 0;
		for (int count = base.Count; i < count; i++)
		{
			base.InnerList[i].CollapseGroup(groupBy);
		}
	}

	public void ExpandGroup(OfficeGroupBy groupBy)
	{
		int i = 0;
		for (int count = base.Count; i < count; i++)
		{
			base.InnerList[i].ExpandGroup(groupBy);
		}
	}

	public void ExpandGroup(OfficeGroupBy groupBy, ExpandCollapseFlags flags)
	{
		int i = 0;
		for (int count = base.Count; i < count; i++)
		{
			base.InnerList[i].ExpandGroup(groupBy, flags);
		}
	}

	public string GetNewAddress(Dictionary<string, string> names, out string strSheetName)
	{
		strSheetName = m_worksheet.Name;
		if (names == null)
		{
			return Address;
		}
		StringBuilder stringBuilder = new StringBuilder();
		int count = base.Count;
		if (count == 0)
		{
			return string.Empty;
		}
		IRange range = base.InnerList[0];
		stringBuilder.Append(((ICombinedRange)range).GetNewAddress(names, out strSheetName));
		string addressSeparator = GetAddressSeparator();
		for (int i = 1; i < count; i++)
		{
			stringBuilder.Append(addressSeparator);
			range = base.InnerList[i];
			stringBuilder.Append(((ICombinedRange)range).GetNewAddress(names, out strSheetName));
		}
		return stringBuilder.ToString();
	}

	public IRange Clone(object parent, Dictionary<string, string> hashNewNames, WorkbookImpl book)
	{
		IWorksheet clonedObject = (m_worksheet as IInternalWorksheet).GetClonedObject(hashNewNames, book);
		RangesCollection rangesCollection = new RangesCollection(base.Application, clonedObject);
		List<IRange> innerList = base.InnerList;
		int i = 0;
		for (int count = innerList.Count; i < count; i++)
		{
			object obj = ((ICombinedRange)innerList[i]).Clone(rangesCollection, hashNewNames, book);
			rangesCollection.Add((IRange)obj);
		}
		return rangesCollection;
	}

	public void ClearConditionalFormats()
	{
		int i = 0;
		for (int count = base.Count; i < count; i++)
		{
			((ICombinedRange)this[i]).ClearConditionalFormats();
		}
	}

	public Rectangle[] GetRectangles()
	{
		int num = 0;
		int count = base.Count;
		for (int i = 0; i < count; i++)
		{
			ICombinedRange combinedRange = (ICombinedRange)this[i];
			num += combinedRange.GetRectanglesCount();
		}
		Rectangle[] array = new Rectangle[num];
		int j = 0;
		int num2 = 0;
		for (; j < count; j++)
		{
			Rectangle[] rectangles = ((ICombinedRange)this[j]).GetRectangles();
			rectangles.CopyTo(array, num2);
			num2 += rectangles.Length;
		}
		return array;
	}

	public int GetRectanglesCount()
	{
		int num = 0;
		int i = 0;
		for (int count = base.Count; i < count; i++)
		{
			ICombinedRange combinedRange = (ICombinedRange)this[i];
			num += combinedRange.GetRectanglesCount();
		}
		return num;
	}

	public new void Add(IRange range)
	{
		CheckDisposed();
		if (range == null)
		{
			throw new ArgumentNullException("range");
		}
		if (range.Worksheet != Worksheet)
		{
			throw new ArgumentException("Can't operate with ranges from different worksheet");
		}
		m_iFirstRow = Math.Min(m_iFirstRow, range.Row);
		m_iFirstColumn = Math.Min(m_iFirstColumn, range.Column);
		m_iLastRow = Math.Max(m_iLastRow, range.LastRow);
		m_iLastColumn = Math.Max(m_iLastColumn, range.LastColumn);
		base.InnerList.Add(range);
	}

	public void AddRange(IRange range)
	{
		CheckDisposed();
		if (range is RangesCollection)
		{
			RangesCollection rangesCollection = (RangesCollection)range;
			int i = 0;
			for (int count = rangesCollection.Count; i < count; i++)
			{
				rangesCollection.Add(rangesCollection[i]);
			}
		}
		else
		{
			Add(range);
		}
	}

	public new void Remove(IRange range)
	{
		CheckDisposed();
		List<IRange> innerList = base.InnerList;
		int i = 0;
		for (int num = innerList.Count; i < num; i++)
		{
			IRange range2 = innerList[i];
			if (range.Worksheet == range2.Worksheet && range.AddressLocal == range2.AddressLocal)
			{
				innerList.RemoveAt(i);
				i--;
				num--;
			}
		}
		base.InnerList.Remove(range);
		EvaluateDimensions();
	}

	private void EvaluateDimensions()
	{
		CheckDisposed();
		m_iFirstRow = m_worksheet.Workbook.MaxRowCount + 1;
		m_iFirstColumn = m_worksheet.Workbook.MaxColumnCount + 1;
		m_iLastRow = 0;
		m_iLastColumn = 0;
		int i = 0;
		for (int count = base.Count; i < count; i++)
		{
			IRange range = base.InnerList[i];
			m_iFirstRow = Math.Min(m_iFirstRow, range.Row);
			m_iFirstColumn = Math.Min(m_iFirstColumn, range.Column);
			m_iLastRow = Math.Max(m_iLastRow, range.LastRow);
			m_iLastColumn = Math.Max(m_iLastColumn, range.LastColumn);
		}
	}

	private SortedList<int, KeyValuePair<int, int>> GetColumnRowIndexes(bool bIsColumn)
	{
		CheckDisposed();
		SortedList<int, KeyValuePair<int, int>> sortedList = new SortedList<int, KeyValuePair<int, int>>();
		int i = 0;
		for (int count = base.Count; i < count; i++)
		{
			IRange range = base.InnerList[i];
			if (range is RangesCollection)
			{
				SortedList<int, KeyValuePair<int, int>> columnRowIndexes = ((RangesCollection)range).GetColumnRowIndexes(bIsColumn);
				IList<int> keys = columnRowIndexes.Keys;
				IList<KeyValuePair<int, int>> values = columnRowIndexes.Values;
				int j = 0;
				for (int count2 = columnRowIndexes.Count; j < count2; j++)
				{
					AddRowColumnIndex(sortedList, keys[j], values[j]);
				}
			}
			else
			{
				int num = (bIsColumn ? range.Column : range.Row);
				int num2 = (bIsColumn ? range.LastColumn : range.LastRow);
				int iSecondaryStart = (bIsColumn ? range.Row : range.Column);
				int iSecondaryEnd = (bIsColumn ? range.LastRow : range.LastColumn);
				for (int k = num; k <= num2; k++)
				{
					AddRowColumnIndex(sortedList, k, iSecondaryStart, iSecondaryEnd);
				}
			}
		}
		return sortedList;
	}

	private void AddRowColumnIndex(SortedList<int, KeyValuePair<int, int>> list, int iIndex, KeyValuePair<int, int> entry)
	{
		CheckDisposed();
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		if (list.ContainsKey(iIndex))
		{
			KeyValuePair<int, int> keyValuePair = list[iIndex];
			int key = entry.Key;
			int value = entry.Value;
			int key2 = Math.Min(keyValuePair.Key, key);
			int value2 = Math.Max(keyValuePair.Value, value);
			entry = new KeyValuePair<int, int>(key2, value2);
		}
		list[iIndex] = entry;
	}

	private void AddRowColumnIndex(SortedList<int, KeyValuePair<int, int>> list, int iIndex, int iSecondaryStart, int iSecondaryEnd)
	{
		CheckDisposed();
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		KeyValuePair<int, int> value;
		if (!list.ContainsKey(iIndex))
		{
			value = new KeyValuePair<int, int>(iSecondaryStart, iSecondaryEnd);
		}
		else
		{
			KeyValuePair<int, int> keyValuePair = list[iIndex];
			int key = Math.Min(keyValuePair.Key, iSecondaryStart);
			int value2 = Math.Max(keyValuePair.Value, iSecondaryEnd);
			value = new KeyValuePair<int, int>(key, value2);
		}
		list[iIndex] = value;
	}

	private IRange GetEntireColumnRow(bool bIsColumn)
	{
		CheckDisposed();
		RangesCollection rangesCollection = base.AppImplementation.CreateRangesCollection(Worksheet);
		SortedList<int, KeyValuePair<int, int>> columnRowIndexes = GetColumnRowIndexes(bIsColumn);
		if (columnRowIndexes.Count == 0)
		{
			return null;
		}
		IList<int> keys = columnRowIndexes.Keys;
		int num = keys[0];
		int num2 = num;
		int num3 = (bIsColumn ? m_worksheet.UsedRange.Row : m_worksheet.UsedRange.Column);
		int num4 = (bIsColumn ? m_worksheet.UsedRange.LastRow : m_worksheet.UsedRange.LastColumn);
		int i = 1;
		IRange range;
		for (int count = columnRowIndexes.Count; i < count; i++)
		{
			int num5 = keys[i];
			if (num5 - num2 == 1)
			{
				num2 = num5;
				continue;
			}
			range = (bIsColumn ? Worksheet.Range[num3, num, num4, num2] : Worksheet.Range[num, num3, num2, num4]);
			rangesCollection.Add(range);
			num = (num2 = num5);
		}
		range = (bIsColumn ? Worksheet.Range[num3, num, num4, num2] : Worksheet.Range[num, num3, num2, num4]);
		rangesCollection.Add(range);
		if (rangesCollection.Count == 1)
		{
			return rangesCollection[0];
		}
		return rangesCollection;
	}

	private IRange[] GetColumnRows(bool bIsColumn)
	{
		CheckDisposed();
		SortedList<int, KeyValuePair<int, int>> columnRowIndexes = GetColumnRowIndexes(bIsColumn);
		IList<int> keys = columnRowIndexes.Keys;
		IList<KeyValuePair<int, int>> values = columnRowIndexes.Values;
		IRange[] array = new IRange[columnRowIndexes.Count];
		int i = 0;
		for (int count = columnRowIndexes.Count; i < count; i++)
		{
			int num = keys[i];
			KeyValuePair<int, int> keyValuePair = values[i];
			int key = keyValuePair.Key;
			int value = keyValuePair.Value;
			array[i] = (bIsColumn ? m_worksheet.Range[key, num, value, num] : m_worksheet.Range[num, key, num, value]);
		}
		return array;
	}

	private void CheckDisposed()
	{
	}

	private string GetAddressSeparator()
	{
		bool isFormulaParsed = base.AppImplementation.IsFormulaParsed;
		base.AppImplementation.IsFormulaParsed = false;
		string listSeparator = base.AppImplementation.CheckAndApplySeperators().TextInfo.ListSeparator;
		base.AppImplementation.IsFormulaParsed = isFormulaParsed;
		return listSeparator;
	}

	public Ptg[] GetNativePtg()
	{
		int count = base.List.Count;
		if (count == 0)
		{
			return null;
		}
		if (base.List[0] is RangesCollection)
		{
			throw new NotSupportedException("Not supported : Range collection as element in range collection");
		}
		List<Ptg> list = new List<Ptg>();
		INativePTG nativePTG = (INativePTG)base.List[0];
		Ptg item = FormulaUtil.CreatePtg(FormulaToken.tCellRangeList, new object[1] { "," });
		list.Add(nativePTG.GetNativePtg()[0]);
		for (int i = 1; i < count; i++)
		{
			if (base.List[i] is RangesCollection)
			{
				throw new NotSupportedException("Not supported : Range collection as element in range collection");
			}
			nativePTG = (INativePTG)base.List[i];
			list.Add(nativePTG.GetNativePtg()[0]);
			list.Add(item);
		}
		if (count > 1)
		{
			Ptg item2 = FormulaUtil.CreatePtg(FormulaToken.tParentheses, new object[1] { "(" });
			list.Add(item2);
		}
		return list.ToArray();
	}

	public new IEnumerator GetEnumerator()
	{
		return Cells.GetEnumerator();
	}
}
