using System;
using System.Collections.Generic;
using System.Globalization;

namespace DocGen.DocIO.DLS;

public class WSeqField : WField
{
	private CaptionNumberingFormat m_numberFormat = (CaptionNumberingFormat)(-1);

	private ParagraphItemCollection m_pItemColl;

	private string m_bookmarkname = string.Empty;

	private bool m_insertnextnumber = true;

	private int m_resetheadinglevel = -1;

	private int m_resetnumber = -1;

	private bool m_repeatnearestnumber;

	private bool m_hideresult;

	public override EntityType EntityType => EntityType.SeqField;

	public new string FormattingString => m_formattingString;

	public CaptionNumberingFormat NumberFormat
	{
		get
		{
			return m_numberFormat;
		}
		set
		{
			m_numberFormat = value;
			if (!base.Document.IsOpening && !base.Document.IsMailMerge)
			{
				string fieldCodeForUnknownFieldType = GetFieldCodeForUnknownFieldType();
				fieldCodeForUnknownFieldType = ClearSwitchString(fieldCodeForUnknownFieldType);
				string value2 = ConvertSwitchesToString();
				if (m_formattingString != string.Empty)
				{
					int num = fieldCodeForUnknownFieldType.LastIndexOf(m_formattingString);
					base.FieldCode = fieldCodeForUnknownFieldType.Insert(num + m_formattingString.Length, value2);
				}
				else if (BookmarkName != string.Empty)
				{
					int num2 = fieldCodeForUnknownFieldType.LastIndexOf(BookmarkName);
					base.FieldCode = fieldCodeForUnknownFieldType.Insert(num2 + BookmarkName.Length, value2);
				}
				else
				{
					int num3 = fieldCodeForUnknownFieldType.LastIndexOf(CaptionName);
					base.FieldCode = fieldCodeForUnknownFieldType.Insert(num3 + CaptionName.Length, value2);
				}
			}
		}
	}

	public string CaptionName
	{
		get
		{
			return m_fieldValue;
		}
		set
		{
			string fieldValue = m_fieldValue;
			m_fieldValue = value;
			if (!base.Document.IsOpening && !base.Document.IsMailMerge && base.FieldEnd != null)
			{
				string fieldCodeForUnknownFieldType = GetFieldCodeForUnknownFieldType();
				base.FieldCode = fieldCodeForUnknownFieldType.Replace(fieldValue, value);
			}
		}
	}

	public string BookmarkName
	{
		get
		{
			return m_bookmarkname;
		}
		set
		{
			string bookmarkname = m_bookmarkname;
			m_bookmarkname = value;
			if (!base.Document.IsOpening && !base.Document.IsMailMerge)
			{
				string fieldCodeForUnknownFieldType = GetFieldCodeForUnknownFieldType();
				if (fieldCodeForUnknownFieldType.Contains(m_bookmarkname))
				{
					base.FieldCode = fieldCodeForUnknownFieldType.Replace(bookmarkname, value);
					return;
				}
				int num = fieldCodeForUnknownFieldType.LastIndexOf(CaptionName);
				base.FieldCode = fieldCodeForUnknownFieldType.Insert(num + CaptionName.Length + 1, value);
			}
		}
	}

	public bool InsertNextNumber
	{
		get
		{
			return m_insertnextnumber;
		}
		set
		{
			m_insertnextnumber = value;
			SwitchUpdation(m_insertnextnumber, " \\n");
		}
	}

	public bool RepeatNearestNumber
	{
		get
		{
			return m_repeatnearestnumber;
		}
		set
		{
			m_repeatnearestnumber = value;
			SwitchUpdation(m_repeatnearestnumber, " \\c");
		}
	}

	public bool HideResult
	{
		get
		{
			return m_hideresult;
		}
		set
		{
			m_hideresult = value;
			SwitchUpdation(m_hideresult, " \\h");
		}
	}

	public int ResetNumber
	{
		get
		{
			return m_resetnumber;
		}
		set
		{
			int resetnumber = m_resetnumber;
			m_resetnumber = value;
			SwitchUpdationLevel(resetnumber.ToString(), " \\r ", m_resetnumber);
		}
	}

	public int ResetHeadingLevel
	{
		get
		{
			return m_resetheadinglevel;
		}
		set
		{
			int resetheadinglevel = m_resetheadinglevel;
			m_resetheadinglevel = value;
			SwitchUpdationLevel(resetheadinglevel.ToString(), " \\s ", m_resetheadinglevel);
		}
	}

	public WSeqField(IWordDocument doc)
		: base(doc)
	{
		m_paraItemType = ParagraphItemType.SeqField;
		m_pItemColl = new ParagraphItemCollection(doc as WordDocument);
		m_pItemColl.SetOwner(this);
		m_fieldType = FieldType.FieldSequence;
	}

	protected internal WSeqField(WField field)
		: base(field.Document)
	{
	}

	protected internal override void ParseFieldCode(string fieldCode)
	{
		UpdateFieldCode(fieldCode);
	}

	internal string GetSeqCaptionName()
	{
		string text = base.FieldCode.Trim().Substring(4);
		if (text.StartsWith("\"") && text.Substring(1).Contains("\""))
		{
			return text.Substring(1, text.IndexOf('"', 1) - 1);
		}
		if (!text.StartsWith("\""))
		{
			char[] separator = new char[2] { ' ', '"' };
			return text.Split(separator, StringSplitOptions.RemoveEmptyEntries)[0];
		}
		return null;
	}

	protected internal override void UpdateFieldCode(string fieldCode)
	{
		string fieldvalue = UpdateFieldValue(fieldCode);
		string[] fieldValues = GetFieldValues(fieldvalue);
		if (fieldValues.Length > 1)
		{
			string text = fieldValues[1];
			if (text.Length > 0)
			{
				string text2 = ClearStringFromOtherCharacters(text);
				switch (text[0])
				{
				case 'n':
					m_insertnextnumber = true;
					break;
				case 'c':
					m_repeatnearestnumber = true;
					break;
				case 'h':
					m_hideresult = true;
					break;
				case 'r':
					if (text2 != string.Empty)
					{
						m_resetnumber = int.Parse(text2);
					}
					else
					{
						m_resetnumber = 0;
					}
					break;
				case 's':
				{
					int result = 0;
					if (int.TryParse(text2, NumberStyles.Integer, CultureInfo.CurrentCulture, out result))
					{
						m_resetheadinglevel = result;
					}
					else
					{
						m_resetheadinglevel = 0;
					}
					break;
				}
				}
			}
		}
		for (int i = 1; i < fieldValues.Length; i++)
		{
			if (fieldValues[i] == "h " || fieldValues[i] == "h")
			{
				m_hideresult = true;
				break;
			}
		}
		string[] array = fieldValues[0].Trim().Split(' ');
		m_fieldValue = ((array.Length > 1) ? array[1] : string.Empty);
		try
		{
			if (array[2] != null)
			{
				m_bookmarkname = array[2];
			}
		}
		catch (IndexOutOfRangeException)
		{
		}
	}

	private string UpdateFieldValue(string fieldCode)
	{
		string fieldValue = fieldCode;
		string empty = string.Empty;
		m_formattingString = string.Empty;
		List<int> formatIndex = new List<int>();
		fieldValue = UpdateSwitchesIndexInFieldValue(fieldValue);
		while (fieldValue.Contains("\\*"))
		{
			fieldValue = UpdateFormatIndexAndFieldValue(fieldValue, ref formatIndex, "\\*");
		}
		while (fieldValue.Contains("\\#"))
		{
			fieldValue = UpdateFormatIndexAndFieldValue(fieldValue, ref formatIndex, "\\#");
		}
		formatIndex.Sort();
		for (int i = 0; i < formatIndex.Count; i++)
		{
			int length = ((i == formatIndex.Count - 1) ? (fieldCode.Length - formatIndex[i]) : (formatIndex[i + 1] - formatIndex[i]));
			empty = fieldCode.Substring(formatIndex[i], length);
			empty = empty.Substring(1, empty.Length - 1);
			if (empty.Contains("\\"))
			{
				empty = empty.Substring(0, empty.IndexOf("\\"));
			}
			ParseSwitches(empty);
		}
		return fieldValue;
	}

	private string UpdateSwitchesIndexInFieldValue(string fieldValue)
	{
		int num = fieldValue.LastIndexOf('\\');
		while (num > 0)
		{
			while (num > 0 && fieldValue[num] == '\\' && fieldValue[num - 1] == '\\')
			{
				fieldValue = fieldValue.Remove(num, 1);
				num--;
			}
			string text = fieldValue.Substring(0, num);
			if (!text.Contains("\\"))
			{
				break;
			}
			num = text.LastIndexOf('\\');
		}
		return fieldValue;
	}

	private string UpdateFormatIndexAndFieldValue(string fieldvalue, ref List<int> formatIndex, string seqSwitch)
	{
		fieldvalue.LastIndexOf(seqSwitch);
		if (!formatIndex.Contains(fieldvalue.LastIndexOf(seqSwitch)))
		{
			formatIndex.Add(fieldvalue.LastIndexOf(seqSwitch));
		}
		fieldvalue = UpdateFieldValue(fieldvalue, formatIndex, seqSwitch);
		return fieldvalue;
	}

	private string UpdateFieldValue(string fieldValue, List<int> formatIndex, string seqSwitch)
	{
		int num = fieldValue.Substring(formatIndex[formatIndex.Count - 1] + 1).IndexOf("\\");
		fieldValue = ((num != -1) ? fieldValue.Remove(formatIndex[formatIndex.Count - 1], num + 1) : fieldValue.Substring(0, formatIndex[formatIndex.Count - 1]));
		return fieldValue;
	}

	private void ParseSwitches(string seqFormat)
	{
		string empty = string.Empty;
		if (seqFormat.Length <= 0)
		{
			return;
		}
		empty = ClearStringFromOtherCharacters(seqFormat);
		switch (seqFormat[0])
		{
		case '#':
			m_formattingString = empty;
			break;
		case '*':
			if (empty == null)
			{
				break;
			}
			switch (empty.Length)
			{
			case 5:
				switch (empty[0])
				{
				case 'r':
					if (empty == "roman")
					{
						m_numberFormat = CaptionNumberingFormat.LowerRoman;
					}
					break;
				case 'R':
					if (empty == "ROMAN")
					{
						m_numberFormat = CaptionNumberingFormat.Roman;
					}
					break;
				case 'L':
					if (empty == "Lower")
					{
						m_numberFormat = CaptionNumberingFormat.Lowercase;
					}
					break;
				case 'U':
					if (empty == "Upper")
					{
						m_numberFormat = CaptionNumberingFormat.Uppercase;
					}
					break;
				}
				break;
			case 10:
				switch (empty[0])
				{
				case 'A':
					if (empty == "ALPHABETIC")
					{
						m_numberFormat = CaptionNumberingFormat.Alphabetic;
					}
					break;
				case 'a':
					if (empty == "alphabetic")
					{
						m_numberFormat = CaptionNumberingFormat.LowerAlphabetic;
					}
					break;
				case 'D':
					if (empty == "DollarText")
					{
						m_numberFormat = CaptionNumberingFormat.DollarText;
					}
					break;
				}
				break;
			case 7:
				switch (empty[3])
				{
				case 'i':
					if (empty == "Ordinal")
					{
						m_numberFormat = CaptionNumberingFormat.Ordinal;
					}
					break;
				case 'T':
					if (empty == "OrdText")
					{
						m_numberFormat = CaptionNumberingFormat.OrdinalText;
					}
					break;
				}
				break;
			case 8:
				switch (empty[0])
				{
				case 'C':
					if (empty == "CardText")
					{
						m_numberFormat = CaptionNumberingFormat.CardinalText;
					}
					break;
				case 'F':
					if (empty == "FirstCap")
					{
						m_numberFormat = CaptionNumberingFormat.FirstCapital;
					}
					break;
				}
				break;
			case 6:
				if (empty == "Arabic")
				{
					m_numberFormat = CaptionNumberingFormat.Number;
				}
				break;
			case 3:
				if (empty == "Hex")
				{
					m_numberFormat = CaptionNumberingFormat.Hexa;
				}
				break;
			case 4:
				if (empty == "Caps")
				{
					m_numberFormat = CaptionNumberingFormat.TitleCase;
				}
				break;
			case 9:
				break;
			}
			break;
		}
	}

	protected internal string[] GetFieldValues(string fieldvalue)
	{
		List<int> list = new List<int>();
		List<KeyValuePair<int, int>> list2 = new List<KeyValuePair<int, int>>();
		int key = 0;
		int num = 0;
		bool flag = true;
		for (int i = 0; i < fieldvalue.Length; i++)
		{
			if (fieldvalue[i] == '\\')
			{
				list.Add(i);
			}
			else if (i < fieldvalue.Length - 1 && fieldvalue[i] == '"')
			{
				if (flag)
				{
					key = i;
					flag = false;
				}
				else
				{
					num = i;
					flag = true;
					list2.Add(new KeyValuePair<int, int>(key, num));
				}
			}
		}
		List<int> list3 = new List<int>(list);
		foreach (int item in list)
		{
			foreach (KeyValuePair<int, int> item2 in list2)
			{
				if (item2.Key < item && item2.Value > item)
				{
					list3.Remove(item);
					break;
				}
			}
		}
		string[] array = new string[list3.Count + 1];
		if (list3.Count > 0)
		{
			int num2 = 0;
			int num3 = 0;
			for (num3 = 0; num3 < list3.Count; num3++)
			{
				array[num3] = fieldvalue.Substring(num2, list3[num3] - num2);
				num2 = list3[num3] + 1;
			}
			array[num3] = fieldvalue.Substring(num2);
		}
		else
		{
			array[0] = fieldvalue;
		}
		return array;
	}

	private static string ClearStringFromOtherCharacters(string value)
	{
		string text = value.Remove(0, 1);
		text = text.Trim();
		char[] trimChars = new char[1] { '"' };
		return text.Trim(trimChars);
	}

	private void SwitchUpdation(bool switchValue, string switchType)
	{
		if (base.Document.IsOpening || base.Document.IsMailMerge)
		{
			return;
		}
		string fieldCodeForUnknownFieldType = GetFieldCodeForUnknownFieldType();
		fieldCodeForUnknownFieldType = ClearFieldSwitch(fieldCodeForUnknownFieldType);
		if (switchValue)
		{
			string text = ConvertSwitchesToString();
			if (text != string.Empty)
			{
				if (fieldCodeForUnknownFieldType.Contains(text))
				{
					int num = fieldCodeForUnknownFieldType.LastIndexOf(text);
					base.FieldCode = fieldCodeForUnknownFieldType.Insert(num + text.Length, switchType);
				}
			}
			else if (m_formattingString != string.Empty)
			{
				int num2 = fieldCodeForUnknownFieldType.LastIndexOf(m_formattingString);
				base.FieldCode = fieldCodeForUnknownFieldType.Insert(num2 + m_formattingString.Length, switchType);
			}
			else if (BookmarkName != string.Empty)
			{
				int num3 = fieldCodeForUnknownFieldType.LastIndexOf(BookmarkName);
				base.FieldCode = fieldCodeForUnknownFieldType.Insert(num3 + BookmarkName.Length, switchType);
			}
			else
			{
				int num4 = fieldCodeForUnknownFieldType.LastIndexOf(CaptionName);
				base.FieldCode = fieldCodeForUnknownFieldType.Insert(num4 + CaptionName.Length, switchType);
			}
		}
		else
		{
			base.FieldCode = fieldCodeForUnknownFieldType;
		}
	}

	private void SwitchUpdationLevel(string oldValue, string switchType, int switchValue)
	{
		if (base.Document.IsOpening || base.Document.IsMailMerge)
		{
			return;
		}
		string fieldCodeForUnknownFieldType = GetFieldCodeForUnknownFieldType();
		fieldCodeForUnknownFieldType = ClearFieldSwitch(fieldCodeForUnknownFieldType);
		fieldCodeForUnknownFieldType = ClearFieldSwitchLevel(fieldCodeForUnknownFieldType, oldValue.ToString());
		string text = ConvertSwitchesToString();
		if (text != string.Empty)
		{
			if (fieldCodeForUnknownFieldType.Contains(text))
			{
				int num = fieldCodeForUnknownFieldType.LastIndexOf(text);
				base.FieldCode = fieldCodeForUnknownFieldType.Insert(num + text.Length, switchType + switchValue);
			}
		}
		else if (m_formattingString != string.Empty)
		{
			int num2 = fieldCodeForUnknownFieldType.LastIndexOf(m_formattingString);
			base.FieldCode = fieldCodeForUnknownFieldType.Insert(num2 + m_formattingString.Length, switchType + switchValue);
		}
		else if (BookmarkName != string.Empty)
		{
			int num3 = fieldCodeForUnknownFieldType.LastIndexOf(BookmarkName);
			base.FieldCode = fieldCodeForUnknownFieldType.Insert(num3 + BookmarkName.Length, switchType + switchValue);
		}
		else
		{
			int num4 = fieldCodeForUnknownFieldType.LastIndexOf(CaptionName);
			base.FieldCode = fieldCodeForUnknownFieldType.Insert(num4 + CaptionName.Length, switchType + switchValue);
		}
	}

	private string ClearFieldSwitch(string fieldCode)
	{
		fieldCode = fieldCode.Replace("\\n", string.Empty);
		fieldCode = fieldCode.Replace("\\c", string.Empty);
		fieldCode = fieldCode.Replace("\\h", string.Empty);
		fieldCode = fieldCode.Replace("\\r", string.Empty);
		fieldCode = fieldCode.Replace("\\s", string.Empty);
		return fieldCode;
	}

	private string ClearFieldSwitchLevel(string fieldCode, string oldLevel)
	{
		fieldCode = fieldCode.Replace(oldLevel, string.Empty);
		fieldCode = fieldCode.Replace(oldLevel, string.Empty);
		return fieldCode;
	}

	private string ClearSwitchString(string fieldCode)
	{
		fieldCode = fieldCode.Replace("\\* Arabic", string.Empty);
		fieldCode = fieldCode.Replace("\\* ALPHABETIC", string.Empty);
		fieldCode = fieldCode.Replace("\\*ROMAN", string.Empty);
		fieldCode = fieldCode.Replace("\\* roman", string.Empty);
		fieldCode = fieldCode.Replace("\\* Lower", string.Empty);
		fieldCode = fieldCode.Replace("\\* Upper", string.Empty);
		fieldCode = fieldCode.Replace("\\* Ordinal", string.Empty);
		fieldCode = fieldCode.Replace("\\* CardText", string.Empty);
		fieldCode = fieldCode.Replace("\\* OrdText", string.Empty);
		fieldCode = fieldCode.Replace("\\* Hexa", string.Empty);
		fieldCode = fieldCode.Replace("\\* DollarText", string.Empty);
		fieldCode = fieldCode.Replace("\\* FirstCap", string.Empty);
		fieldCode = fieldCode.Replace("\\* Caps", string.Empty);
		return fieldCode;
	}

	protected internal override string ConvertSwitchesToString()
	{
		string text = string.Empty;
		switch (m_numberFormat)
		{
		case CaptionNumberingFormat.Number:
			text += " \\* Arabic";
			break;
		case CaptionNumberingFormat.Alphabetic:
			text += " \\* ALPHABETIC";
			break;
		case CaptionNumberingFormat.Roman:
			text += " \\* ROMAN";
			break;
		case CaptionNumberingFormat.LowerRoman:
			text += "\\* roman";
			break;
		case CaptionNumberingFormat.Lowercase:
			text += "\\* Lower";
			break;
		case CaptionNumberingFormat.Uppercase:
			text += "\\* Upper";
			break;
		case CaptionNumberingFormat.LowerAlphabetic:
			text += "\\* alphabetic";
			break;
		case CaptionNumberingFormat.Ordinal:
			text += "\\* Ordinal";
			break;
		case CaptionNumberingFormat.CardinalText:
			text += "\\* CardText";
			break;
		case CaptionNumberingFormat.OrdinalText:
			text += "\\* OrdText";
			break;
		case CaptionNumberingFormat.Hexa:
			text += "\\* Hex";
			break;
		case CaptionNumberingFormat.DollarText:
			text += "\\* DollarText";
			break;
		case CaptionNumberingFormat.FirstCapital:
			text += "\\* FirstCap";
			break;
		case CaptionNumberingFormat.TitleCase:
			text += "\\* Caps";
			break;
		}
		return text;
	}

	internal void UpdateFieldMarks()
	{
		int indexInOwnerCollection = GetIndexInOwnerCollection();
		bool flag = false;
		if (base.FieldEnd != null && base.FieldEnd.ParentField != this)
		{
			if (base.FieldSeparator != null)
			{
				if (base.FieldSeparator.ParentField != this)
				{
					WFieldMark wFieldMark = new WFieldMark(base.Document, FieldMarkType.FieldSeparator);
					base.OwnerParagraph.ChildEntities.Insert(indexInOwnerCollection + 1, wFieldMark);
					base.FieldSeparator = wFieldMark;
				}
				if (base.FieldSeparator.Owner is WParagraph)
				{
					indexInOwnerCollection = base.FieldSeparator.GetIndexInOwnerCollection();
					WFieldMark wFieldMark2 = new WFieldMark(base.Document, FieldMarkType.FieldEnd);
					base.OwnerParagraph.ChildEntities.Insert(indexInOwnerCollection + 1, wFieldMark2);
					base.FieldEnd = wFieldMark2;
					flag = true;
				}
			}
			else
			{
				WFieldMark wFieldMark3 = new WFieldMark(base.Document, FieldMarkType.FieldEnd);
				base.OwnerParagraph.ChildEntities.Insert(indexInOwnerCollection + 1, wFieldMark3);
				base.FieldEnd = wFieldMark3;
			}
		}
		if (flag)
		{
			UpdateSequenceFieldResult();
		}
	}

	internal void UpdateSequenceFieldResult()
	{
		CheckFieldSeparator();
		RemovePreviousResult();
		int indexInOwnerCollection = base.FieldEnd.GetIndexInOwnerCollection();
		WTextRange wTextRange = new WTextRange(base.Document);
		wTextRange.Text = ((CaptionName != string.Empty) ? CaptionName : "Error! No bookmark name given.");
		if (base.ResultFormat != null)
		{
			wTextRange.ApplyCharacterFormat(base.ResultFormat);
		}
		base.FieldEnd.OwnerParagraph.Items.Insert(indexInOwnerCollection, wTextRange);
	}
}
