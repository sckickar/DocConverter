using System;
using System.Collections.Generic;

namespace DocGen.DocIO.DLS;

public class MergeFieldEventArgs : EventArgs
{
	private IWordDocument m_doc;

	private IWMergeField m_field;

	private object m_fieldValue;

	private int m_rowIndex;

	private string m_tableName;

	private string m_groupName;

	private WTextRange m_textRange;

	public IWordDocument Document => m_doc;

	public string FieldName => m_field.FieldName;

	public object FieldValue => m_fieldValue;

	public string TableName => m_tableName;

	public string GroupName => m_groupName;

	public int RowIndex => m_rowIndex;

	public WCharacterFormat CharacterFormat => m_textRange.CharacterFormat;

	public string Text
	{
		get
		{
			if (FieldValue == null)
			{
				return "";
			}
			string text = FieldValue.ToString();
			WMergeField wMergeField = CurrentMergeField as WMergeField;
			try
			{
				text = wMergeField.UpdateTextFormat(FieldValue.ToString());
				string numberFormat = wMergeField.NumberFormat;
				if (numberFormat == string.Empty)
				{
					wMergeField.RemoveMergeFormat(wMergeField.FieldCode, ref numberFormat);
				}
				if (numberFormat != string.Empty)
				{
					text = wMergeField.UpdateNumberFormat(text, numberFormat);
				}
				if (wMergeField.DateFormat != string.Empty)
				{
					DateTime result;
					if (FieldValue is DateTime)
					{
						DateTime currentDateTime = (DateTime)FieldValue;
						text = wMergeField.UpdateDateField("\\@ " + wMergeField.DateFormat + wMergeField.FormattingString, currentDateTime);
					}
					else if (DateTime.TryParse(FieldValue.ToString(), out result))
					{
						text = wMergeField.UpdateDateField("\\@ " + wMergeField.DateFormat + wMergeField.FormattingString, result);
						text = wMergeField.UpdateTextFormat(text.ToString());
					}
				}
			}
			catch (Exception)
			{
			}
			return text;
		}
		set
		{
			m_fieldValue = value;
			m_textRange.Text = Text;
		}
	}

	public IWMergeField CurrentMergeField => m_field;

	public WTextRange TextRange => m_textRange;

	public MergeFieldEventArgs(IWordDocument doc, string tableName, int rowIndex, IWMergeField field, object value)
	{
		m_doc = doc;
		m_field = field;
		m_fieldValue = value;
		m_rowIndex = rowIndex;
		m_tableName = tableName;
		m_textRange = new WTextRange(m_doc);
		m_textRange.Text = Text;
		List<WCharacterFormat> resultCharacterFormatting = (m_field as WField).GetResultCharacterFormatting();
		if (resultCharacterFormatting.Count > 0)
		{
			m_textRange.ApplyCharacterFormat(resultCharacterFormatting[0]);
		}
	}

	internal MergeFieldEventArgs(IWordDocument doc, string tableName, int rowIndex, IWMergeField field, object value, string groupName)
	{
		m_doc = doc;
		m_field = field;
		m_fieldValue = value;
		m_rowIndex = rowIndex;
		m_tableName = tableName;
		m_groupName = groupName;
		m_textRange = new WTextRange(m_doc);
		m_textRange.Text = Text;
		List<WCharacterFormat> resultCharacterFormatting = (m_field as WField).GetResultCharacterFormatting();
		if (resultCharacterFormatting.Count > 0)
		{
			m_textRange.ApplyCharacterFormat(resultCharacterFormatting[0]);
		}
	}
}
