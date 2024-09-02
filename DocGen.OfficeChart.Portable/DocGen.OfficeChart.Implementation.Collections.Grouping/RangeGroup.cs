using System;
using System.Collections;
using DocGen.Drawing;

namespace DocGen.OfficeChart.Implementation.Collections.Grouping;

internal class RangeGroup : CommonObject, IRange, IParentApplication, IEnumerable
{
	protected int m_iFirstRow;

	protected int m_iFirstColumn;

	protected int m_iLastRow;

	protected int m_iLastColumn;

	private WorksheetGroup m_sheetGroup;

	private RichTextStringGroup m_richText;

	private RangeGroup m_rangeEnd;

	protected StyleGroup m_style;

	public int Count => m_sheetGroup.Count;

	public IRange this[int index] => GetRange(index);

	public WorkbookImpl Workbook => m_sheetGroup.ParentWorkbook;

	public string Address
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	public string AddressLocal => RangeImpl.GetAddressLocal(m_iFirstRow, m_iFirstColumn, m_iLastRow, m_iLastColumn);

	public string AddressGlobal
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	public string AddressR1C1
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	public string AddressR1C1Local => RangeImpl.GetAddressLocal(m_iFirstRow, m_iFirstColumn, m_iLastRow, m_iLastColumn, bR1C1: true);

	public bool Boolean
	{
		get
		{
			if (m_sheetGroup.IsEmpty)
			{
				return false;
			}
			bool boolean = GetRange(0).Boolean;
			int i = 1;
			for (int count = m_sheetGroup.Count; i < count; i++)
			{
				bool boolean2 = GetRange(i).Boolean;
				if (boolean != boolean2)
				{
					return false;
				}
			}
			return boolean;
		}
		set
		{
			int i = 0;
			for (int count = m_sheetGroup.Count; i < count; i++)
			{
				GetRange(i).Boolean = value;
			}
		}
	}

	public IBorders Borders => CellStyle.Borders;

	public IRange[] Cells
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public int Column => m_iFirstColumn;

	public int ColumnGroupLevel
	{
		get
		{
			if (m_sheetGroup.IsEmpty)
			{
				return int.MinValue;
			}
			int columnGroupLevel = GetRange(0).ColumnGroupLevel;
			int i = 1;
			for (int count = m_sheetGroup.Count; i < count; i++)
			{
				int columnGroupLevel2 = GetRange(i).ColumnGroupLevel;
				if (columnGroupLevel != columnGroupLevel2)
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
			if (m_sheetGroup.IsEmpty)
			{
				return double.MinValue;
			}
			double columnWidth = GetRange(0).ColumnWidth;
			int i = 1;
			for (int count = m_sheetGroup.Count; i < count; i++)
			{
				double columnWidth2 = GetRange(i).ColumnWidth;
				if (columnWidth != columnWidth2)
				{
					return double.MinValue;
				}
			}
			return columnWidth;
		}
		set
		{
			int i = 0;
			for (int count = m_sheetGroup.Count; i < count; i++)
			{
				GetRange(i).ColumnWidth = value;
			}
		}
	}

	int IRange.Count
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public DateTime DateTime
	{
		get
		{
			if (m_sheetGroup.IsEmpty)
			{
				return DateTime.MinValue;
			}
			DateTime dateTime = GetRange(0).DateTime;
			int i = 1;
			for (int count = m_sheetGroup.Count; i < count; i++)
			{
				DateTime dateTime2 = GetRange(i).DateTime;
				if (dateTime != dateTime2)
				{
					return DateTime.MinValue;
				}
			}
			return dateTime;
		}
		set
		{
			int i = 0;
			for (int count = m_sheetGroup.Count; i < count; i++)
			{
				GetRange(i).DateTime = value;
			}
		}
	}

	public string DisplayText
	{
		get
		{
			if (m_sheetGroup.IsEmpty)
			{
				return null;
			}
			string displayText = GetRange(0).DisplayText;
			int i = 1;
			for (int count = m_sheetGroup.Count; i < count; i++)
			{
				string displayText2 = GetRange(i).DisplayText;
				if (displayText != displayText2)
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
			if (m_rangeEnd == null)
			{
				m_rangeEnd = new RangeGroup(base.Application, this, m_iLastRow, m_iLastColumn);
			}
			return m_rangeEnd;
		}
	}

	public IRange EntireColumn
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public IRange EntireRow
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public string Error
	{
		get
		{
			if (m_sheetGroup.IsEmpty)
			{
				return null;
			}
			string error = GetRange(0).Error;
			int i = 1;
			for (int count = m_sheetGroup.Count; i < count; i++)
			{
				string error2 = GetRange(i).Error;
				if (error != error2)
				{
					return null;
				}
			}
			return error;
		}
		set
		{
			int i = 0;
			for (int count = m_sheetGroup.Count; i < count; i++)
			{
				GetRange(i).Error = value;
			}
		}
	}

	public string Formula
	{
		get
		{
			if (m_sheetGroup.IsEmpty)
			{
				return null;
			}
			string formula = GetRange(0).Formula;
			int i = 1;
			for (int count = m_sheetGroup.Count; i < count; i++)
			{
				string formula2 = GetRange(i).Formula;
				if (formula != formula2)
				{
					return null;
				}
			}
			return formula;
		}
		set
		{
			int i = 0;
			for (int count = m_sheetGroup.Count; i < count; i++)
			{
				GetRange(i).Formula = value;
			}
		}
	}

	public string FormulaR1C1
	{
		get
		{
			if (m_sheetGroup.IsEmpty)
			{
				return null;
			}
			string formulaR1C = GetRange(0).FormulaR1C1;
			int i = 1;
			for (int count = m_sheetGroup.Count; i < count; i++)
			{
				string formulaR1C2 = GetRange(i).FormulaR1C1;
				if (formulaR1C != formulaR1C2)
				{
					return null;
				}
			}
			return formulaR1C;
		}
		set
		{
			int i = 0;
			for (int count = m_sheetGroup.Count; i < count; i++)
			{
				GetRange(i).FormulaR1C1 = value;
			}
		}
	}

	public string FormulaArray
	{
		get
		{
			if (m_sheetGroup.IsEmpty)
			{
				return null;
			}
			string formulaArray = GetRange(0).FormulaArray;
			int i = 1;
			for (int count = m_sheetGroup.Count; i < count; i++)
			{
				string formulaArray2 = GetRange(i).FormulaArray;
				if (formulaArray != formulaArray2)
				{
					return null;
				}
			}
			return formulaArray;
		}
		set
		{
			int i = 0;
			for (int count = m_sheetGroup.Count; i < count; i++)
			{
				GetRange(i).FormulaArray = value;
			}
		}
	}

	public string FormulaArrayR1C1
	{
		get
		{
			if (m_sheetGroup.IsEmpty)
			{
				return null;
			}
			string formulaArrayR1C = GetRange(0).FormulaArrayR1C1;
			int i = 1;
			for (int count = m_sheetGroup.Count; i < count; i++)
			{
				string formulaArrayR1C2 = GetRange(i).FormulaArrayR1C1;
				if (formulaArrayR1C != formulaArrayR1C2)
				{
					return null;
				}
			}
			return formulaArrayR1C;
		}
		set
		{
			int i = 0;
			for (int count = m_sheetGroup.Count; i < count; i++)
			{
				GetRange(i).FormulaArrayR1C1 = value;
			}
		}
	}

	public bool FormulaHidden
	{
		get
		{
			if (m_sheetGroup.IsEmpty)
			{
				return false;
			}
			bool formulaHidden = GetRange(0).FormulaHidden;
			int i = 1;
			for (int count = m_sheetGroup.Count; i < count; i++)
			{
				bool formulaHidden2 = GetRange(i).FormulaHidden;
				if (formulaHidden != formulaHidden2)
				{
					return false;
				}
			}
			return formulaHidden;
		}
		set
		{
			int i = 0;
			for (int count = m_sheetGroup.Count; i < count; i++)
			{
				GetRange(i).FormulaHidden = value;
			}
		}
	}

	public DateTime FormulaDateTime
	{
		get
		{
			if (m_sheetGroup.IsEmpty)
			{
				return DateTime.MinValue;
			}
			DateTime formulaDateTime = GetRange(0).FormulaDateTime;
			int i = 1;
			for (int count = m_sheetGroup.Count; i < count; i++)
			{
				DateTime formulaDateTime2 = GetRange(i).FormulaDateTime;
				if (formulaDateTime != formulaDateTime2)
				{
					return DateTime.MinValue;
				}
			}
			return formulaDateTime;
		}
		set
		{
			int i = 0;
			for (int count = m_sheetGroup.Count; i < count; i++)
			{
				GetRange(i).FormulaDateTime = value;
			}
		}
	}

	public bool HasDataValidation => false;

	public bool HasBoolean
	{
		get
		{
			if (m_sheetGroup.IsEmpty)
			{
				return false;
			}
			bool hasBoolean = GetRange(0).HasBoolean;
			int i = 1;
			for (int count = m_sheetGroup.Count; i < count; i++)
			{
				bool hasBoolean2 = GetRange(i).HasBoolean;
				if (hasBoolean != hasBoolean2)
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
			if (m_sheetGroup.IsEmpty)
			{
				return false;
			}
			bool hasDateTime = GetRange(0).HasDateTime;
			int i = 1;
			for (int count = m_sheetGroup.Count; i < count; i++)
			{
				bool hasDateTime2 = GetRange(i).HasDateTime;
				if (hasDateTime != hasDateTime2)
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
			if (m_sheetGroup.IsEmpty)
			{
				return false;
			}
			bool hasFormulaBoolValue = GetRange(0).HasFormulaBoolValue;
			int i = 1;
			for (int count = m_sheetGroup.Count; i < count; i++)
			{
				bool hasFormulaBoolValue2 = GetRange(i).HasFormulaBoolValue;
				if (hasFormulaBoolValue != hasFormulaBoolValue2)
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
			if (m_sheetGroup.IsEmpty)
			{
				return false;
			}
			bool hasFormulaErrorValue = GetRange(0).HasFormulaErrorValue;
			int i = 1;
			for (int count = m_sheetGroup.Count; i < count; i++)
			{
				bool hasFormulaErrorValue2 = GetRange(i).HasFormulaErrorValue;
				if (hasFormulaErrorValue != hasFormulaErrorValue2)
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
			if (m_sheetGroup.IsEmpty)
			{
				return false;
			}
			bool flag = GetRange(0).HasFormulaDateTime;
			int i = 1;
			for (int count = m_sheetGroup.Count; i < count; i++)
			{
				bool hasFormulaDateTime = GetRange(i).HasFormulaDateTime;
				if (flag != hasFormulaDateTime)
				{
					flag = false;
					break;
				}
			}
			return flag;
		}
	}

	public bool HasFormulaNumberValue
	{
		get
		{
			if (m_sheetGroup.IsEmpty)
			{
				return false;
			}
			bool flag = GetRange(0).HasFormulaNumberValue;
			int i = 1;
			for (int count = m_sheetGroup.Count; i < count; i++)
			{
				bool hasFormulaNumberValue = GetRange(i).HasFormulaNumberValue;
				if (flag != hasFormulaNumberValue)
				{
					flag = false;
					break;
				}
			}
			return flag;
		}
	}

	public bool HasFormulaStringValue
	{
		get
		{
			if (m_sheetGroup.IsEmpty)
			{
				return false;
			}
			bool flag = GetRange(0).HasFormulaStringValue;
			int i = 1;
			for (int count = m_sheetGroup.Count; i < count; i++)
			{
				bool hasFormulaStringValue = GetRange(i).HasFormulaStringValue;
				if (flag != hasFormulaStringValue)
				{
					flag = false;
					break;
				}
			}
			return flag;
		}
	}

	public bool HasFormula
	{
		get
		{
			if (m_sheetGroup.IsEmpty)
			{
				return false;
			}
			bool hasFormula = GetRange(0).HasFormula;
			int i = 1;
			for (int count = m_sheetGroup.Count; i < count; i++)
			{
				bool hasFormula2 = GetRange(i).HasFormula;
				if (hasFormula != hasFormula2)
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
			if (m_sheetGroup.IsEmpty)
			{
				return false;
			}
			bool hasFormulaArray = GetRange(0).HasFormulaArray;
			int i = 1;
			for (int count = m_sheetGroup.Count; i < count; i++)
			{
				bool hasFormulaArray2 = GetRange(i).HasFormulaArray;
				if (hasFormulaArray != hasFormulaArray2)
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
			if (m_sheetGroup.IsEmpty)
			{
				return false;
			}
			bool hasNumber = GetRange(0).HasNumber;
			int i = 1;
			for (int count = m_sheetGroup.Count; i < count; i++)
			{
				bool hasNumber2 = GetRange(i).HasNumber;
				if (hasNumber != hasNumber2)
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
			if (m_sheetGroup.IsEmpty)
			{
				return false;
			}
			bool hasRichText = GetRange(0).HasRichText;
			int i = 1;
			for (int count = m_sheetGroup.Count; i < count; i++)
			{
				bool hasRichText2 = GetRange(i).HasRichText;
				if (hasRichText != hasRichText2)
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
			if (m_sheetGroup.IsEmpty)
			{
				return false;
			}
			bool hasString = GetRange(0).HasString;
			int i = 1;
			for (int count = m_sheetGroup.Count; i < count; i++)
			{
				bool hasString2 = GetRange(i).HasString;
				if (hasString != hasString2)
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
			if (m_sheetGroup.IsEmpty)
			{
				return false;
			}
			bool hasStyle = GetRange(0).HasStyle;
			int i = 1;
			for (int count = m_sheetGroup.Count; i < count; i++)
			{
				bool hasStyle2 = GetRange(i).HasStyle;
				if (hasStyle != hasStyle2)
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
			if (m_sheetGroup.IsEmpty)
			{
				return OfficeHAlign.HAlignGeneral;
			}
			OfficeHAlign horizontalAlignment = GetRange(0).HorizontalAlignment;
			int i = 1;
			for (int count = m_sheetGroup.Count; i < count; i++)
			{
				OfficeHAlign horizontalAlignment2 = GetRange(i).HorizontalAlignment;
				if (horizontalAlignment != horizontalAlignment2)
				{
					return OfficeHAlign.HAlignGeneral;
				}
			}
			return horizontalAlignment;
		}
		set
		{
			int i = 0;
			for (int count = m_sheetGroup.Count; i < count; i++)
			{
				GetRange(i).HorizontalAlignment = value;
			}
		}
	}

	public int IndentLevel
	{
		get
		{
			if (m_sheetGroup.IsEmpty)
			{
				return int.MinValue;
			}
			int indentLevel = GetRange(0).IndentLevel;
			int i = 1;
			for (int count = m_sheetGroup.Count; i < count; i++)
			{
				int indentLevel2 = GetRange(i).IndentLevel;
				if (indentLevel != indentLevel2)
				{
					return int.MinValue;
				}
			}
			return indentLevel;
		}
		set
		{
			int i = 0;
			for (int count = m_sheetGroup.Count; i < count; i++)
			{
				GetRange(i).IndentLevel = value;
			}
		}
	}

	public bool IsBlank
	{
		get
		{
			if (m_sheetGroup.IsEmpty)
			{
				return false;
			}
			bool isBlank = GetRange(0).IsBlank;
			int i = 1;
			for (int count = m_sheetGroup.Count; i < count; i++)
			{
				bool isBlank2 = GetRange(i).IsBlank;
				if (isBlank != isBlank2)
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
			if (m_sheetGroup.IsEmpty)
			{
				return false;
			}
			bool isBoolean = GetRange(0).IsBoolean;
			int i = 1;
			for (int count = m_sheetGroup.Count; i < count; i++)
			{
				bool isBoolean2 = GetRange(i).IsBoolean;
				if (isBoolean != isBoolean2)
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
			if (m_sheetGroup.IsEmpty)
			{
				return false;
			}
			bool isError = GetRange(0).IsError;
			int i = 1;
			for (int count = m_sheetGroup.Count; i < count; i++)
			{
				bool isError2 = GetRange(i).IsError;
				if (isError != isError2)
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
			if (m_sheetGroup.IsEmpty)
			{
				return false;
			}
			bool isGroupedByColumn = GetRange(0).IsGroupedByColumn;
			int i = 1;
			for (int count = m_sheetGroup.Count; i < count; i++)
			{
				bool isGroupedByColumn2 = GetRange(i).IsGroupedByColumn;
				if (isGroupedByColumn != isGroupedByColumn2)
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
			if (m_sheetGroup.IsEmpty)
			{
				return false;
			}
			bool isGroupedByRow = GetRange(0).IsGroupedByRow;
			int i = 1;
			for (int count = m_sheetGroup.Count; i < count; i++)
			{
				bool isGroupedByRow2 = GetRange(i).IsGroupedByRow;
				if (isGroupedByRow != isGroupedByRow2)
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
			if (m_sheetGroup.IsEmpty)
			{
				return false;
			}
			bool isInitialized = GetRange(0).IsInitialized;
			int i = 1;
			for (int count = m_sheetGroup.Count; i < count; i++)
			{
				bool isInitialized2 = GetRange(i).IsInitialized;
				if (isInitialized != isInitialized2)
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
			if (m_sheetGroup.IsEmpty)
			{
				return double.MinValue;
			}
			double number = GetRange(0).Number;
			int i = 1;
			for (int count = m_sheetGroup.Count; i < count; i++)
			{
				double number2 = GetRange(i).Number;
				if (number != number2)
				{
					return double.MinValue;
				}
			}
			return number;
		}
		set
		{
			int i = 0;
			for (int count = m_sheetGroup.Count; i < count; i++)
			{
				if (BitConverter.DoubleToInt64Bits(value) == BitConverter.DoubleToInt64Bits(-0.0))
				{
					value = 0.0;
				}
				GetRange(i).Number = value;
			}
		}
	}

	public string NumberFormat
	{
		get
		{
			if (m_sheetGroup.IsEmpty)
			{
				return null;
			}
			string numberFormat = GetRange(0).NumberFormat;
			int i = 1;
			for (int count = m_sheetGroup.Count; i < count; i++)
			{
				string numberFormat2 = GetRange(i).NumberFormat;
				if (numberFormat != numberFormat2)
				{
					return null;
				}
			}
			return numberFormat;
		}
		set
		{
			int i = 0;
			for (int count = m_sheetGroup.Count; i < count; i++)
			{
				GetRange(i).NumberFormat = value;
			}
		}
	}

	public int Row => m_iFirstRow;

	public int RowGroupLevel
	{
		get
		{
			if (m_sheetGroup.IsEmpty)
			{
				return int.MinValue;
			}
			int rowGroupLevel = GetRange(0).RowGroupLevel;
			int i = 1;
			for (int count = m_sheetGroup.Count; i < count; i++)
			{
				int rowGroupLevel2 = GetRange(i).RowGroupLevel;
				if (rowGroupLevel != rowGroupLevel2)
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
			if (m_sheetGroup.IsEmpty)
			{
				return double.MinValue;
			}
			double rowHeight = GetRange(0).RowHeight;
			int i = 1;
			for (int count = m_sheetGroup.Count; i < count; i++)
			{
				double rowHeight2 = GetRange(i).RowHeight;
				if (rowHeight != rowHeight2)
				{
					return double.MinValue;
				}
			}
			return rowHeight;
		}
		set
		{
			int i = 0;
			for (int count = m_sheetGroup.Count; i < count; i++)
			{
				GetRange(i).RowHeight = value;
			}
		}
	}

	public IRange[] Rows
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public IRange[] Columns
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public IStyle CellStyle
	{
		get
		{
			if (m_style == null)
			{
				m_style = new StyleGroup(base.Application, this);
			}
			return m_style;
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public string CellStyleName
	{
		get
		{
			if (m_sheetGroup.IsEmpty)
			{
				return null;
			}
			string cellStyleName = GetRange(0).CellStyleName;
			int i = 1;
			for (int count = m_sheetGroup.Count; i < count; i++)
			{
				string cellStyleName2 = GetRange(i).CellStyleName;
				if (cellStyleName != cellStyleName2)
				{
					return null;
				}
			}
			return cellStyleName;
		}
		set
		{
			int i = 0;
			for (int count = m_sheetGroup.Count; i < count; i++)
			{
				GetRange(i).CellStyleName = value;
			}
		}
	}

	public string Text
	{
		get
		{
			if (m_sheetGroup.IsEmpty)
			{
				return null;
			}
			string text = GetRange(0).Text;
			int i = 1;
			for (int count = m_sheetGroup.Count; i < count; i++)
			{
				string text2 = GetRange(i).Text;
				if (text != text2)
				{
					return null;
				}
			}
			return text;
		}
		set
		{
			int i = 0;
			for (int count = m_sheetGroup.Count; i < count; i++)
			{
				GetRange(i).Text = value;
			}
		}
	}

	public TimeSpan TimeSpan
	{
		get
		{
			if (m_sheetGroup.IsEmpty)
			{
				return TimeSpan.MinValue;
			}
			TimeSpan timeSpan = GetRange(0).TimeSpan;
			int i = 1;
			for (int count = m_sheetGroup.Count; i < count; i++)
			{
				TimeSpan timeSpan2 = GetRange(i).TimeSpan;
				if (timeSpan != timeSpan2)
				{
					return TimeSpan.MinValue;
				}
			}
			return timeSpan;
		}
		set
		{
			int i = 0;
			for (int count = m_sheetGroup.Count; i < count; i++)
			{
				GetRange(i).TimeSpan = value;
			}
		}
	}

	public string Value
	{
		get
		{
			if (m_sheetGroup.IsEmpty)
			{
				return null;
			}
			string value = GetRange(0).Value;
			int i = 1;
			for (int count = m_sheetGroup.Count; i < count; i++)
			{
				string value2 = GetRange(i).Value;
				if (value != value2)
				{
					return null;
				}
			}
			return value;
		}
		set
		{
			int i = 0;
			for (int count = m_sheetGroup.Count; i < count; i++)
			{
				GetRange(i).Value = value;
			}
		}
	}

	public string CalculatedValue => null;

	public object Value2
	{
		get
		{
			if (m_sheetGroup.IsEmpty)
			{
				return null;
			}
			object value = GetRange(0).Value2;
			int i = 1;
			for (int count = m_sheetGroup.Count; i < count; i++)
			{
				object value2 = GetRange(i).Value2;
				if (value != value2)
				{
					return null;
				}
			}
			return value;
		}
		set
		{
			int i = 0;
			for (int count = m_sheetGroup.Count; i < count; i++)
			{
				GetRange(i).Value2 = value;
			}
		}
	}

	public OfficeVAlign VerticalAlignment
	{
		get
		{
			if (m_sheetGroup.IsEmpty)
			{
				return OfficeVAlign.VAlignTop;
			}
			OfficeVAlign verticalAlignment = GetRange(0).VerticalAlignment;
			int i = 1;
			for (int count = m_sheetGroup.Count; i < count; i++)
			{
				OfficeVAlign verticalAlignment2 = GetRange(i).VerticalAlignment;
				if (verticalAlignment != verticalAlignment2)
				{
					return OfficeVAlign.VAlignTop;
				}
			}
			return verticalAlignment;
		}
		set
		{
			int i = 0;
			for (int count = m_sheetGroup.Count; i < count; i++)
			{
				GetRange(i).VerticalAlignment = value;
			}
		}
	}

	public IWorksheet Worksheet => m_sheetGroup;

	public IRange this[int row, int column]
	{
		get
		{
			return new RangeGroup(base.Application, m_sheetGroup, row, column, row, column);
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	public IRange this[int row, int column, int lastRow, int lastColumn] => new RangeGroup(base.Application, m_sheetGroup, row, column, lastRow, lastColumn);

	public IRange this[string name] => this[name, false];

	public IRange this[string name, bool IsR1C1Notation] => new RangeGroup(base.Application, m_sheetGroup, name, IsR1C1Notation);

	public string FormulaStringValue
	{
		get
		{
			if (m_sheetGroup.IsEmpty)
			{
				return null;
			}
			string formulaStringValue = GetRange(0).FormulaStringValue;
			int i = 1;
			for (int count = m_sheetGroup.Count; i < count; i++)
			{
				string formulaStringValue2 = GetRange(i).FormulaStringValue;
				if (formulaStringValue != formulaStringValue2)
				{
					return null;
				}
			}
			return formulaStringValue;
		}
		set
		{
			int i = 0;
			for (int count = m_sheetGroup.Count; i < count; i++)
			{
				GetRange(i).FormulaStringValue = value;
			}
		}
	}

	public double FormulaNumberValue
	{
		get
		{
			if (m_sheetGroup.IsEmpty)
			{
				return double.MinValue;
			}
			double formulaNumberValue = GetRange(0).FormulaNumberValue;
			int i = 1;
			for (int count = m_sheetGroup.Count; i < count; i++)
			{
				double formulaNumberValue2 = GetRange(i).FormulaNumberValue;
				if (formulaNumberValue != formulaNumberValue2)
				{
					return double.MinValue;
				}
			}
			return formulaNumberValue;
		}
		set
		{
			int i = 0;
			for (int count = m_sheetGroup.Count; i < count; i++)
			{
				GetRange(i).FormulaNumberValue = value;
			}
		}
	}

	public bool FormulaBoolValue
	{
		get
		{
			if (m_sheetGroup.IsEmpty)
			{
				return false;
			}
			bool formulaBoolValue = GetRange(0).FormulaBoolValue;
			int i = 1;
			for (int count = m_sheetGroup.Count; i < count; i++)
			{
				bool formulaBoolValue2 = GetRange(i).FormulaBoolValue;
				if (formulaBoolValue != formulaBoolValue2)
				{
					return false;
				}
			}
			return formulaBoolValue;
		}
		set
		{
			int i = 0;
			for (int count = m_sheetGroup.Count; i < count; i++)
			{
				GetRange(i).FormulaBoolValue = value;
			}
		}
	}

	public string FormulaErrorValue
	{
		get
		{
			if (m_sheetGroup.IsEmpty)
			{
				return null;
			}
			string formulaErrorValue = GetRange(0).FormulaErrorValue;
			int i = 1;
			for (int count = m_sheetGroup.Count; i < count; i++)
			{
				string formulaStringValue = GetRange(i).FormulaStringValue;
				if (formulaErrorValue != formulaStringValue)
				{
					return null;
				}
			}
			return formulaErrorValue;
		}
		set
		{
			int i = 0;
			for (int count = m_sheetGroup.Count; i < count; i++)
			{
				GetRange(i).FormulaErrorValue = value;
			}
		}
	}

	public IRichTextString RichText
	{
		get
		{
			if (m_richText == null)
			{
				m_richText = new RichTextStringGroup(base.Application, this);
			}
			return m_richText;
		}
	}

	public bool IsMerged
	{
		get
		{
			if (m_sheetGroup.IsEmpty)
			{
				return false;
			}
			bool isMerged = GetRange(0).IsMerged;
			int i = 1;
			for (int count = m_sheetGroup.Count; i < count; i++)
			{
				bool isMerged2 = GetRange(i).IsMerged;
				if (isMerged != isMerged2)
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
			throw new NotImplementedException();
		}
	}

	public bool WrapText
	{
		get
		{
			if (m_sheetGroup.IsEmpty)
			{
				return false;
			}
			bool wrapText = GetRange(0).WrapText;
			int i = 1;
			for (int count = m_sheetGroup.Count; i < count; i++)
			{
				bool wrapText2 = GetRange(i).WrapText;
				if (wrapText != wrapText2)
				{
					return false;
				}
			}
			return wrapText;
		}
		set
		{
			int i = 0;
			for (int count = m_sheetGroup.Count; i < count; i++)
			{
				GetRange(i).WrapText = value;
			}
		}
	}

	public bool HasExternalFormula
	{
		get
		{
			if (m_sheetGroup.IsEmpty)
			{
				return false;
			}
			int i = 0;
			for (int count = m_sheetGroup.Count; i < count; i++)
			{
				if (!GetRange(i).HasExternalFormula)
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
			if (m_sheetGroup.IsEmpty)
			{
				return ExcelIgnoreError.None;
			}
			return ExcelIgnoreError.All;
		}
		set
		{
		}
	}

	public BuiltInStyles? BuiltInStyle
	{
		get
		{
			if (m_sheetGroup.IsEmpty)
			{
				return null;
			}
			BuiltInStyles? builtInStyles = GetRange(0).BuiltInStyle;
			int i = 1;
			for (int count = m_sheetGroup.Count; i < count; i++)
			{
				if (!builtInStyles.HasValue)
				{
					break;
				}
				if (GetRange(i).BuiltInStyle != builtInStyles)
				{
					builtInStyles = null;
					break;
				}
			}
			return builtInStyles;
		}
		set
		{
			int i = 0;
			for (int count = m_sheetGroup.Count; i < count; i++)
			{
				GetRange(i).BuiltInStyle = value;
			}
		}
	}

	protected RangeGroup(IApplication application, object parent)
		: base(application, parent)
	{
		FindParents();
	}

	public RangeGroup(IApplication application, object parent, int iFirstRow, int iFirstColumn)
		: this(application, parent, iFirstRow, iFirstColumn, iFirstRow, iFirstColumn)
	{
	}

	public RangeGroup(IApplication application, object parent, int iFirstRow, int iFirstColumn, int iLastRow, int iLastColumn)
		: this(application, parent)
	{
		m_iFirstRow = iFirstRow;
		m_iFirstColumn = iFirstColumn;
		m_iLastRow = iLastRow;
		m_iLastColumn = iLastColumn;
	}

	public RangeGroup(IApplication application, object parent, string name)
		: this(application, parent, name, IsR1C1Notation: false)
	{
	}

	public RangeGroup(IApplication application, object parent, string name, bool IsR1C1Notation)
		: this(application, parent)
	{
		WorksheetGroup obj = (WorksheetGroup)parent;
		if (obj.IsEmpty)
		{
			throw new NotSupportedException("Sheets collection cannot be empty.");
		}
		IRange range = obj[0].Range[name, IsR1C1Notation];
		m_iFirstRow = range.Row;
		m_iFirstColumn = range.Column;
		m_iLastRow = range.LastRow;
		m_iLastColumn = range.LastColumn;
	}

	private void FindParents()
	{
		m_sheetGroup = FindParent(typeof(WorksheetGroup)) as WorksheetGroup;
		if (m_sheetGroup == null)
		{
			throw new ArgumentOutOfRangeException("parent", "Can't find parent worksheet group.");
		}
	}

	private IRange GetRange(int iSheetIndex)
	{
		return m_sheetGroup[iSheetIndex].Range[m_iFirstRow, m_iFirstColumn, m_iLastRow, m_iLastColumn];
	}

	public IRange Activate()
	{
		throw new NotSupportedException();
	}

	public IRange Activate(bool scroll)
	{
		throw new NotSupportedException();
	}

	public IRange Group(OfficeGroupBy groupBy)
	{
		throw new NotSupportedException();
	}

	public IRange Group(OfficeGroupBy groupBy, bool bCollapsed)
	{
		throw new NotSupportedException();
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
		int i = 0;
		for (int count = m_sheetGroup.Count; i < count; i++)
		{
			GetRange(i).Merge();
		}
	}

	public void Merge(bool clearCells)
	{
		int i = 0;
		for (int count = m_sheetGroup.Count; i < count; i++)
		{
			GetRange(i).Merge(clearCells);
		}
	}

	public IRange Ungroup(OfficeGroupBy groupBy)
	{
		throw new NotSupportedException();
	}

	public void UnMerge()
	{
		int i = 0;
		for (int count = m_sheetGroup.Count; i < count; i++)
		{
			GetRange(i).UnMerge();
		}
	}

	public void FreezePanes()
	{
		throw new NotSupportedException();
	}

	public void Clear()
	{
		int i = 0;
		for (int count = m_sheetGroup.Count; i < count; i++)
		{
			GetRange(i).Clear();
		}
	}

	public void Clear(bool isClearFormat)
	{
		int i = 0;
		for (int count = m_sheetGroup.Count; i < count; i++)
		{
			GetRange(i).Clear(isClearFormat);
		}
	}

	public void Clear(OfficeMoveDirection direction)
	{
		int i = 0;
		for (int count = m_sheetGroup.Count; i < count; i++)
		{
			GetRange(i).Clear(direction);
		}
	}

	public void Clear(OfficeClearOptions option)
	{
		int i = 0;
		for (int count = m_sheetGroup.Count; i < count; i++)
		{
			GetRange(i).Clear(option);
		}
	}

	public void Clear(OfficeMoveDirection direction, OfficeCopyRangeOptions options)
	{
		int i = 0;
		for (int count = m_sheetGroup.Count; i < count; i++)
		{
			GetRange(i).Clear(direction, options);
		}
	}

	public void MoveTo(IRange destination)
	{
		throw new NotImplementedException();
	}

	public void MoveTo(IRange destination, bool bUpdateFormula)
	{
		throw new NotImplementedException();
	}

	public IRange CopyTo(IRange destination)
	{
		throw new NotImplementedException();
	}

	public IRange CopyTo(IRange destination, bool bUpdateFormula)
	{
		throw new NotImplementedException();
	}

	public IRange CopyTo(IRange destination, OfficeCopyRangeOptions options)
	{
		throw new NotImplementedException();
	}

	public IRange IntersectWith(IRange range)
	{
		throw new NotImplementedException();
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
		throw new NotImplementedException();
	}

	IRange IRange.FindFirst(double findValue, OfficeFindType flags)
	{
		throw new NotImplementedException();
	}

	IRange IRange.FindFirst(bool findValue)
	{
		throw new NotImplementedException();
	}

	IRange IRange.FindFirst(DateTime findValue)
	{
		throw new NotImplementedException();
	}

	public IRange FindFirst(TimeSpan findValue)
	{
		throw new NotImplementedException();
	}

	public IRange[] FindAll(string findValue, OfficeFindType flags)
	{
		throw new NotImplementedException();
	}

	public IRange[] FindAll(double findValue, OfficeFindType flags)
	{
		throw new NotImplementedException();
	}

	public IRange[] FindAll(bool findValue)
	{
		throw new NotImplementedException();
	}

	public IRange[] FindAll(DateTime findValue)
	{
		throw new NotImplementedException();
	}

	public IRange[] FindAll(TimeSpan findValue)
	{
		throw new NotImplementedException();
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
		OfficeKnownColors nearestColor = Workbook.GetNearestColor(borderColor);
		BorderAround(borderLine, nearestColor);
	}

	public void BorderAround(OfficeLineStyle borderLine, OfficeKnownColors borderColor)
	{
		int i = 0;
		for (int count = m_sheetGroup.Count; i < count; i++)
		{
			GetRange(i).BorderAround(borderLine, borderColor);
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
		OfficeKnownColors nearestColor = Workbook.GetNearestColor(borderColor);
		BorderInside(borderLine, nearestColor);
	}

	public void BorderInside(OfficeLineStyle borderLine, OfficeKnownColors borderColor)
	{
		int i = 0;
		for (int count = m_sheetGroup.Count; i < count; i++)
		{
			GetRange(i).BorderInside(borderLine, borderColor);
		}
	}

	public void BorderNone()
	{
		int i = 0;
		for (int count = m_sheetGroup.Count; i < count; i++)
		{
			GetRange(i).BorderNone();
		}
	}

	public void CollapseGroup(OfficeGroupBy groupBy)
	{
		int i = 0;
		for (int count = m_sheetGroup.Count; i < count; i++)
		{
			GetRange(i).CollapseGroup(groupBy);
		}
	}

	public void ExpandGroup(OfficeGroupBy groupBy)
	{
		int i = 0;
		for (int count = m_sheetGroup.Count; i < count; i++)
		{
			GetRange(i).ExpandGroup(groupBy);
		}
	}

	public void ExpandGroup(OfficeGroupBy groupBy, ExpandCollapseFlags flags)
	{
		int i = 0;
		for (int count = m_sheetGroup.Count; i < count; i++)
		{
			GetRange(i).ExpandGroup(groupBy, flags);
		}
	}

	public IEnumerator GetEnumerator()
	{
		throw new NotImplementedException();
	}
}
