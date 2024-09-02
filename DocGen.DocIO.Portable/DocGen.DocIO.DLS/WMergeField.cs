using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using DocGen.DocIO.DLS.XML;

namespace DocGen.DocIO.DLS;

public class WMergeField : WField, IWMergeField, IWField, IWTextRange, IParagraphItem, IEntity
{
	protected string m_fieldName = "";

	private string m_textBefore = "";

	private string m_textAfter = "";

	private string m_prefix = "";

	private string m_numberFormat = "";

	private string m_dateFormat = "";

	private ParagraphItemCollection m_pItemColl;

	private readonly string SlashSymbol = '\\'.ToString();

	private readonly string InvertedCommas = '"'.ToString();

	public override EntityType EntityType => EntityType.MergeField;

	public string FieldName
	{
		get
		{
			return m_fieldName;
		}
		set
		{
			string fieldName = m_fieldName;
			m_fieldName = value;
			if (!base.Document.IsOpening && !base.Document.IsMailMerge && base.FieldEnd != null && fieldName != value)
			{
				string fieldCodeForUnknownFieldType = GetFieldCodeForUnknownFieldType();
				fieldCodeForUnknownFieldType = fieldCodeForUnknownFieldType.Replace(fieldName, value);
				base.FieldCode = fieldCodeForUnknownFieldType;
			}
		}
	}

	public string TextBefore
	{
		get
		{
			return m_textBefore;
		}
		set
		{
			string textBefore = m_textBefore;
			m_textBefore = value;
			if (!base.Document.IsOpening && !base.Document.IsMailMerge)
			{
				string fieldCodeForUnknownFieldType = GetFieldCodeForUnknownFieldType();
				if (fieldCodeForUnknownFieldType.ToLower().Contains("\\b"))
				{
					base.FieldCode = fieldCodeForUnknownFieldType.Replace(textBefore, value);
				}
				else
				{
					base.FieldCode = fieldCodeForUnknownFieldType + "\\b \"" + value + "\"";
				}
			}
		}
	}

	public string TextAfter
	{
		get
		{
			return m_textAfter;
		}
		set
		{
			string textAfter = m_textAfter;
			m_textAfter = value;
			if (!base.Document.IsOpening && !base.Document.IsMailMerge)
			{
				string fieldCodeForUnknownFieldType = GetFieldCodeForUnknownFieldType();
				if (fieldCodeForUnknownFieldType.ToLower().Contains("\\f"))
				{
					base.FieldCode = fieldCodeForUnknownFieldType.Replace(textAfter, value);
				}
				else
				{
					base.FieldCode = fieldCodeForUnknownFieldType + "\\f \"" + value + "\"";
				}
			}
		}
	}

	public string Prefix
	{
		get
		{
			return m_prefix;
		}
		internal set
		{
			m_prefix = value;
		}
	}

	public string NumberFormat => m_numberFormat;

	public string DateFormat => m_dateFormat;

	[Obsolete("This property has been deprecated. Use the Text property of WField class to get result text of the field.")]
	public ParagraphItemCollection TextItems
	{
		get
		{
			if (m_pItemColl == null)
			{
				m_pItemColl = new ParagraphItemCollection(base.Document);
				m_pItemColl.SetOwner(this);
			}
			return m_pItemColl;
		}
	}

	public WMergeField(IWordDocument doc)
		: base(doc)
	{
		m_paraItemType = ParagraphItemType.MergeField;
		m_pItemColl = new ParagraphItemCollection(doc as WordDocument);
		m_pItemColl.SetOwner(this);
		m_fieldType = FieldType.FieldMergeField;
	}

	protected override object CloneImpl()
	{
		WMergeField wMergeField = (WMergeField)base.CloneImpl();
		if (m_pItemColl != null)
		{
			wMergeField.m_pItemColl = new ParagraphItemCollection(base.Document);
			wMergeField.m_pItemColl.SetOwner(wMergeField);
			m_pItemColl.CloneItemsTo(wMergeField.m_pItemColl);
		}
		return wMergeField;
	}

	internal override void CloneRelationsTo(WordDocument doc, OwnerHolder nextOwner)
	{
		base.CloneRelationsTo(doc, nextOwner);
		if (m_pItemColl != null)
		{
			int i = 0;
			for (int count = m_pItemColl.Count; i < count; i++)
			{
				m_pItemColl[i].CloneRelationsTo(doc, nextOwner);
			}
		}
	}

	protected override void ReadXmlAttributes(IXDLSAttributeReader reader)
	{
		base.ReadXmlAttributes(reader);
		m_fieldName = reader.ReadString("FieldName");
		if (reader.HasAttribute("BeforeText"))
		{
			m_textBefore = reader.ReadString("BeforeText");
		}
		if (reader.HasAttribute("AfterText"))
		{
			m_textAfter = reader.ReadString("AfterText");
		}
		if (reader.HasAttribute("NumberFormat"))
		{
			m_numberFormat = reader.ReadString("NumberFormat");
		}
		if (reader.HasAttribute("DateFormat"))
		{
			m_dateFormat = reader.ReadString("DateFormat");
		}
		if (reader.HasAttribute("Prefix"))
		{
			m_prefix = reader.ReadString("Prefix");
		}
	}

	protected override void WriteXmlAttributes(IXDLSAttributeWriter writer)
	{
		base.WriteXmlAttributes(writer);
		if (FieldName != string.Empty)
		{
			writer.WriteValue("FieldName", FieldName);
		}
		if (m_textBefore != string.Empty)
		{
			writer.WriteValue("BeforeText", TextBefore);
		}
		if (m_textAfter != string.Empty)
		{
			writer.WriteValue("AfterText", TextAfter);
		}
		if (m_numberFormat != string.Empty)
		{
			writer.WriteValue("NumberFormat", NumberFormat);
		}
		if (m_dateFormat != string.Empty)
		{
			writer.WriteValue("DateFormat", DateFormat);
		}
		if (m_prefix != string.Empty)
		{
			writer.WriteValue("Prefix", m_prefix);
		}
	}

	protected internal override void ParseFieldCode(string fieldCode)
	{
		UpdateFieldCode(fieldCode);
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

	protected internal override void UpdateFieldCode(string fieldCode)
	{
		bool flag = true;
		string fieldvalue = UpdateFieldValue(fieldCode);
		string[] fieldValues = GetFieldValues(fieldvalue);
		for (int i = 1; i < fieldValues.Length; i++)
		{
			string text = fieldValues[i];
			if (text.Length <= 0)
			{
				continue;
			}
			string text2 = ClearStringFromOtherCharacters(text);
			switch (text[0])
			{
			case 'B':
			case 'b':
				m_textBefore = text2;
				flag = false;
				continue;
			case 'F':
			case 'f':
				m_textAfter = text2;
				flag = false;
				continue;
			case 'M':
			case 'V':
			case 'm':
			case 'v':
				flag = false;
				continue;
			}
			if (flag)
			{
				ref string reference = ref fieldValues[0];
				reference = reference + "\\" + fieldValues[i];
			}
		}
		ParseFieldName(fieldValues[0]);
	}

	internal void ApplyBaseFormat()
	{
		for (int i = 0; i < TextItems.Count; i++)
		{
			TextItems[i].ParaItemCharFormat.ApplyBase(base.CharacterFormat.BaseFormat);
		}
	}

	internal override void Close()
	{
		if (m_pItemColl != null)
		{
			m_pItemColl.Close();
			m_pItemColl = null;
		}
		if (m_textAfter != null)
		{
			m_textAfter = null;
		}
		if (m_textBefore != null)
		{
			m_textBefore = null;
		}
		if (m_fieldName != null)
		{
			m_fieldName = null;
		}
		if (m_dateFormat != null)
		{
			m_dateFormat = null;
		}
		if (m_numberFormat != null)
		{
			m_numberFormat = null;
		}
		if (m_prefix != null)
		{
			m_prefix = null;
		}
		base.Close();
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
			UpdateMergeFieldResult();
		}
	}

	internal void ParseFieldName(string fieldName)
	{
		ParseFieldName(fieldName, ref m_prefix, ref m_fieldName);
	}

	internal void ParseFieldName(string fieldName, ref string prefix, ref string nameOfField)
	{
		string[] array = fieldName.Trim().Split(' ');
		array[0] = array[0].ToUpper();
		string text = "";
		for (int i = 0; i < array.Length; i++)
		{
			text = text + array[i] + " ";
		}
		fieldName = text;
		if (text.StartsWith("MERGEFIELD") && (text.Contains("\\") || text.Contains(":")))
		{
			ParseFieldNameHavingGroupExpression(fieldName, ref prefix, ref nameOfField);
		}
		else
		{
			ParseFieldNameUsingRegex(fieldName, ref prefix, ref nameOfField);
		}
	}

	private void ParseFieldNameUsingRegex(string fieldName, ref string prefix, ref string nameOfField)
	{
		Match match = new Regex("MERGEFIELD\\s+\"?([^:\"]+):?([^\"]*)\"?").Match(fieldName.Trim());
		if (match.Groups[2].Length == 0)
		{
			prefix = "";
			nameOfField = match.Groups[1].Value;
		}
		else
		{
			prefix = match.Groups[1].Value;
			nameOfField = match.Groups[2].Value;
		}
	}

	private void ParseFieldNameHavingGroupExpression(string fieldName, ref string prefix, ref string nameOfField)
	{
		bool flag = false;
		string text = fieldName;
		text = text.Replace("MERGEFIELD ", string.Empty).Trim();
		if (text.IndexOf("\"") == 0 && text.LastIndexOf("\"") == text.Length - 1)
		{
			text = text.Remove(0, 1);
			text = text.Remove(text.Length - 1, 1);
		}
		if (text.Contains("\\\"") || text.Contains("\\\\"))
		{
			text = text.Replace("\\", "");
		}
		if (text.Contains(":"))
		{
			prefix = text.Substring(0, text.IndexOf(":"));
			if (prefix == "BeginGroup" || prefix == "EndGroup" || prefix == "TableStart" || prefix == "TableEnd" || prefix == "Image")
			{
				flag = true;
				if (!fieldName.Contains("\\"))
				{
					ParseFieldNameUsingRegex(fieldName, ref prefix, ref nameOfField);
					return;
				}
			}
			else
			{
				prefix = string.Empty;
			}
		}
		if (flag)
		{
			text = text.Substring(text.IndexOf(":") + 1, text.Length - (text.IndexOf(":") + 1));
		}
		nameOfField = text;
	}

	internal string GetInstructionText()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(" MERGEFIELD ");
		if (Prefix != string.Empty)
		{
			stringBuilder.AppendFormat(Prefix);
			stringBuilder.Append(":");
		}
		stringBuilder.Append(FieldName);
		if (TextBefore != string.Empty)
		{
			stringBuilder.Append(" " + SlashSymbol + "b ");
			stringBuilder.Append(TextBefore);
		}
		if (TextAfter != string.Empty)
		{
			stringBuilder.Append(" " + SlashSymbol + "f ");
			stringBuilder.Append(TextAfter);
		}
		if (!string.IsNullOrEmpty(DateFormat))
		{
			stringBuilder.Append(" " + SlashSymbol + "@ ");
			stringBuilder.Append(InvertedCommas + DateFormat + InvertedCommas);
		}
		if (!string.IsNullOrEmpty(NumberFormat))
		{
			stringBuilder.Append(" " + SlashSymbol + "# ");
			stringBuilder.Append(InvertedCommas + NumberFormat + InvertedCommas);
		}
		if (base.TextFormat != 0)
		{
			stringBuilder.Append(" " + SlashSymbol + "* ");
			stringBuilder.Append(GetTextFormat(base.TextFormat));
		}
		stringBuilder.Append(base.FormattingString);
		return stringBuilder.ToString();
	}

	private string GetTextFormat(TextFormat format)
	{
		string result = "";
		switch (format)
		{
		case TextFormat.Uppercase:
			result = "Upper";
			break;
		case TextFormat.Lowercase:
			result = "Lower";
			break;
		case TextFormat.Titlecase:
			result = "Caps";
			break;
		case TextFormat.FirstCapital:
			result = "FirstCap";
			break;
		}
		return result;
	}

	private static string ClearStringFromOtherCharacters(string value)
	{
		string text = value.Remove(0, 1);
		text = text.Trim();
		char[] trimChars = new char[1] { '"' };
		return text.Trim(trimChars);
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
		if (fieldValue.Contains("\\#"))
		{
			fieldValue = UpdateFormatIndexAndFieldValue(fieldValue, ref formatIndex, "\\#");
		}
		else if (fieldValue.Contains("\\n"))
		{
			fieldValue = UpdateFormatIndexAndFieldValue(fieldValue, ref formatIndex, "\\n");
		}
		else if (fieldValue.Contains("\\N"))
		{
			fieldValue = UpdateFormatIndexAndFieldValue(fieldValue, ref formatIndex, "\\N");
		}
		if (fieldValue.Contains("\\@"))
		{
			fieldValue = UpdateFormatIndexAndFieldValue(fieldValue, ref formatIndex, "\\@");
		}
		else if (fieldValue.Contains("\\d"))
		{
			fieldValue = UpdateFormatIndexAndFieldValue(fieldValue, ref formatIndex, "\\d");
		}
		else if (fieldValue.Contains("\\D"))
		{
			fieldValue = UpdateFormatIndexAndFieldValue(fieldValue, ref formatIndex, "\\D");
		}
		else if (fieldValue.Contains("\\"))
		{
			fieldValue = UpdateFormatIndexAndFieldValue(fieldValue, ref formatIndex, "\\");
		}
		formatIndex.Sort();
		for (int i = 0; i < formatIndex.Count; i++)
		{
			int length = ((i == formatIndex.Count - 1) ? (fieldCode.Length - formatIndex[i]) : (formatIndex[i + 1] - formatIndex[i]));
			empty = fieldCode.Substring(formatIndex[i], length);
			empty = empty.Substring(1, empty.Length - 1);
			if (empty.Contains("\\") && empty[0] != '@')
			{
				empty = empty.Substring(0, empty.IndexOf("\\"));
			}
			ParseSwitches(empty);
		}
		return fieldValue;
	}

	private void ParseSwitches(string mergeFormat)
	{
		string empty = string.Empty;
		if (mergeFormat.Length <= 0)
		{
			return;
		}
		empty = ClearStringFromOtherCharacters(mergeFormat);
		switch (mergeFormat[0])
		{
		case '#':
		case 'N':
		case 'n':
			m_numberFormat = empty;
			break;
		case '@':
		case 'D':
		case 'd':
			m_dateFormat = empty;
			break;
		case '*':
			switch (empty.ToUpper())
			{
			case "UPPER":
				m_textFormat = TextFormat.Uppercase;
				break;
			case "LOWER":
				m_textFormat = TextFormat.Lowercase;
				break;
			case "CAPS":
				m_textFormat = TextFormat.Titlecase;
				break;
			case "FIRSTCAP":
				m_textFormat = TextFormat.Titlecase;
				break;
			default:
				m_formattingString = m_formattingString + " \\" + mergeFormat;
				break;
			}
			break;
		}
	}

	private string UpdateFieldValue(string fieldValue, List<int> formatIndex, string mergeSwitch)
	{
		if (mergeSwitch == "\\@")
		{
			fieldValue = fieldValue.Replace('\\', '/');
		}
		int num = fieldValue.Substring(formatIndex[formatIndex.Count - 1] + 1).IndexOf("\\");
		fieldValue = ((num != -1) ? fieldValue.Remove(formatIndex[formatIndex.Count - 1], num) : fieldValue.Substring(0, formatIndex[formatIndex.Count - 1]));
		return fieldValue;
	}

	private string UpdateFormatIndexAndFieldValue(string fieldvalue, ref List<int> formatIndex, string mergeSwitch)
	{
		int num = fieldvalue.LastIndexOf(mergeSwitch);
		char[] array = new char[10] { 'b', 'B', 'f', 'F', 'm', 'M', 'v', 'V', '\\', '"' };
		if (mergeSwitch == "\\" && num + 1 < fieldvalue.Length)
		{
			char[] array2 = array;
			foreach (char c in array2)
			{
				if (fieldvalue[fieldvalue.LastIndexOf("\\") + 1] == c)
				{
					return fieldvalue;
				}
			}
		}
		formatIndex.Add(fieldvalue.LastIndexOf(mergeSwitch));
		fieldvalue = UpdateFieldValue(fieldvalue, formatIndex, mergeSwitch);
		return fieldvalue;
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

	internal void UpdateMergeFieldResult()
	{
		CheckFieldSeparator();
		RemovePreviousResult();
		int indexInOwnerCollection = base.FieldEnd.GetIndexInOwnerCollection();
		char c = '«';
		char c2 = '»';
		WTextRange wTextRange = new WTextRange(base.Document);
		string text = ((Prefix != string.Empty) ? (Prefix + ":") : string.Empty);
		wTextRange.Text = ((m_fieldName != string.Empty) ? (TextBefore + c + text + m_fieldName + c2 + TextAfter) : "Error! No bookmark name given.");
		if (base.ResultFormat != null)
		{
			wTextRange.ApplyCharacterFormat(base.ResultFormat);
		}
		base.FieldEnd.OwnerParagraph.Items.Insert(indexInOwnerCollection, wTextRange);
	}
}
