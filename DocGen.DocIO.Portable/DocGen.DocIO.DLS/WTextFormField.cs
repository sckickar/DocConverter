using System;
using System.Text;
using DocGen.DocIO.DLS.XML;

namespace DocGen.DocIO.DLS;

public class WTextFormField : WFormField
{
	internal const string DEF_TEXT = "\u2002\u2002\u2002\u2002\u2002";

	private TextFormFieldType m_formFieldType;

	private string m_defText;

	private int m_maxLength;

	private string m_strTextFormat;

	private WTextRange m_text;

	private short m_iFieldSeparator;

	public override EntityType EntityType => EntityType.TextFormField;

	public TextFormFieldType Type
	{
		get
		{
			return m_formFieldType;
		}
		set
		{
			m_formFieldType = value;
			if (m_formFieldType == TextFormFieldType.CurrentDateText)
			{
				AppendDateTimeField(FieldType.FieldDate);
			}
			else if (m_formFieldType == TextFormFieldType.CurrentTimeText)
			{
				AppendDateTimeField(FieldType.FieldTime);
			}
		}
	}

	public string StringFormat
	{
		get
		{
			return m_strTextFormat;
		}
		set
		{
			m_strTextFormat = value;
		}
	}

	public string DefaultText
	{
		get
		{
			return m_defText;
		}
		set
		{
			m_defText = value;
		}
	}

	public int MaximumLength
	{
		get
		{
			return m_maxLength;
		}
		set
		{
			if (!m_doc.IsOpening && value < m_defText.Length && value != 0 && Type != TextFormFieldType.Calculation)
			{
				throw new ArgumentOutOfRangeException("MaximumLength is lower than current text length");
			}
			m_maxLength = value;
		}
	}

	public WTextRange TextRange
	{
		get
		{
			GetTextRangeValue();
			WTextRange firstTextRange = GetFirstTextRange();
			if (firstTextRange != null)
			{
				return firstTextRange;
			}
			return m_text;
		}
		set
		{
			m_text = value;
			SetTextRangeValue(m_text);
		}
	}

	public override string Text
	{
		get
		{
			GetTextRangeValue();
			return m_text.Text;
		}
		set
		{
			WTextRange firstTextRange = GetFirstTextRange();
			if (firstTextRange != null)
			{
				firstTextRange.Text = value;
				SetTextRangeValue(firstTextRange);
			}
			else
			{
				m_text.Text = value;
				SetTextRangeValue(m_text);
			}
		}
	}

	public WTextFormField(IWordDocument doc)
		: base(doc)
	{
		m_curFormFieldType = FormFieldType.TextInput;
		m_paraItemType = ParagraphItemType.TextFormField;
		base.FieldType = FieldType.FieldFormTextInput;
		base.Params = 128;
		m_defText = string.Empty;
		m_text = new WTextRange(doc);
		m_strTextFormat = string.Empty;
	}

	internal override void Close()
	{
		if (m_text != null)
		{
			m_text.Close();
			m_text = null;
		}
		if (m_defText != null)
		{
			m_defText = null;
		}
		if (m_strTextFormat != null)
		{
			m_strTextFormat = null;
		}
		base.Close();
	}

	protected override object CloneImpl()
	{
		WTextFormField wTextFormField = (WTextFormField)base.CloneImpl();
		wTextFormField.m_text = (WTextRange)m_text.Clone();
		wTextFormField.m_text.SetOwner(wTextFormField);
		return wTextFormField;
	}

	private WTextRange GetFirstTextRange()
	{
		bool flag = false;
		for (int i = 0; i < base.Range.Count; i++)
		{
			ParagraphItem paragraphItem = base.Range.Items[i] as ParagraphItem;
			if (flag && paragraphItem is WTextRange)
			{
				return paragraphItem as WTextRange;
			}
			if (!flag && paragraphItem is WFieldMark && (paragraphItem as WFieldMark).Type == FieldMarkType.FieldSeparator && base.FieldSeparator == paragraphItem)
			{
				flag = true;
			}
		}
		return null;
	}

	private void GetTextRangeValue()
	{
		if (base.Range.Count >= 1)
		{
			string text = string.Empty;
			for (int i = 0; i < base.Range.Count; i++)
			{
				text = ((!(base.Range.Items[i] is ParagraphItem)) ? (text + UpdateTextBodyItemText(base.Range.Items[i] as Entity)) : (text + UpdateParagraphItemText(base.Range.Items[i] as Entity)));
			}
			m_text.Text = text;
		}
	}

	private string UpdateTextBodyItemText(Entity entity)
	{
		string text = string.Empty;
		if (entity is WParagraph)
		{
			for (int i = 0; i < (entity as WParagraph).Items.Count; i++)
			{
				text += UpdateParagraphItemText((entity as WParagraph).Items[i]);
				if (m_iFieldSeparator == 0)
				{
					return text;
				}
			}
		}
		else if (entity is WTable)
		{
			text += UpdateTextForTable(entity);
		}
		return text;
	}

	private string UpdateTextForTable(Entity entity)
	{
		string text = string.Empty;
		for (int i = 0; i < (entity as WTable).Rows.Count; i++)
		{
			WTableRow wTableRow = (entity as WTable).Rows[i];
			for (int j = 0; j < wTableRow.Cells.Count; j++)
			{
				WTableCell wTableCell = wTableRow.Cells[j];
				for (int k = 0; k < wTableCell.Items.Count; k++)
				{
					text += UpdateTextBodyItemText(wTableCell.Items[k]);
					if (m_iFieldSeparator == 0)
					{
						return text;
					}
				}
			}
		}
		return text;
	}

	private string UpdateParagraphItemText(Entity entity)
	{
		string result = string.Empty;
		if (m_iFieldSeparator > 0 && entity is WTextRange)
		{
			result = (entity as WTextRange).Text;
		}
		else if (entity is WFieldMark && (entity as WFieldMark).Type == FieldMarkType.FieldSeparator && base.FieldSeparator == entity)
		{
			m_iFieldSeparator++;
		}
		else if (entity is WFieldMark && (entity as WFieldMark).Type == FieldMarkType.FieldEnd && base.FieldEnd == entity)
		{
			m_iFieldSeparator--;
		}
		return result;
	}

	private void SetTextRangeValue(WTextRange textRange)
	{
		RemovePreviousText();
		for (int i = GetIndexInOwnerCollection(); i < base.OwnerParagraph.Items.Count; i++)
		{
			ParagraphItem paragraphItem = base.OwnerParagraph.Items[i];
			if (paragraphItem is WFieldMark && (paragraphItem as WFieldMark).Type == FieldMarkType.FieldSeparator && base.FieldSeparator == paragraphItem)
			{
				base.OwnerParagraph.Items.Insert(i + 1, textRange.Clone());
				break;
			}
		}
		base.IsFieldRangeUpdated = false;
	}

	private void RemovePreviousText()
	{
		m_iFieldSeparator = 0;
		for (int i = 0; i < base.Range.Count; i++)
		{
			int count = base.Range.Count;
			if (base.Range.Items[i] is ParagraphItem)
			{
				RemoveParagraphItem(base.Range.Items[i] as Entity);
			}
			else
			{
				RemoveTextBodyItem(base.Range.Items[i] as Entity);
			}
			if (base.Range.Count < count)
			{
				i -= count - base.Range.Count;
			}
		}
	}

	private void RemoveTextBodyItem(Entity entity)
	{
		if (entity is WParagraph)
		{
			for (int i = 0; i < (entity as WParagraph).Items.Count; i++)
			{
				int count = (entity as WParagraph).Items.Count;
				RemoveParagraphItem((entity as WParagraph).Items[i]);
				if (m_iFieldSeparator == 0)
				{
					InsertParagraphItems(entity as WParagraph);
					(entity.Owner as WTextBody).Items.Remove(entity);
					break;
				}
				if ((entity as WParagraph).Items.Count < count)
				{
					i -= count - (entity as WParagraph).Items.Count;
				}
			}
			if (m_iFieldSeparator >= 1 && (entity as WParagraph).Items.Count == 0)
			{
				(entity.Owner as WTextBody).Items.Remove(entity);
			}
		}
		else if (entity is WTable)
		{
			(entity.Owner as WTextBody).Items.Remove(entity);
		}
	}

	private void InsertParagraphItems(WParagraph paragraph)
	{
		for (int i = GetIndexInOwnerCollection(); i < base.OwnerParagraph.Items.Count; i++)
		{
			ParagraphItem paragraphItem = base.OwnerParagraph.Items[i];
			if (paragraphItem is WFieldMark && (paragraphItem as WFieldMark).Type == FieldMarkType.FieldSeparator)
			{
				int num = 1;
				while (paragraph.Items.Count != 0)
				{
					base.OwnerParagraph.Items.Insert(i + num, paragraph.Items[0]);
					num++;
				}
				break;
			}
		}
	}

	private void RemoveParagraphItem(Entity entity)
	{
		if (entity is WTextRange)
		{
			if (m_iFieldSeparator >= 1)
			{
				(entity as ParagraphItem).OwnerParagraph.Items.Remove(entity);
			}
		}
		else if (entity is WFieldMark && (entity as WFieldMark).Type == FieldMarkType.FieldSeparator)
		{
			m_iFieldSeparator++;
			if (m_iFieldSeparator > 1)
			{
				(entity as ParagraphItem).OwnerParagraph.Items.Remove(entity);
			}
		}
		else if (entity is WFieldMark && (entity as WFieldMark).Type == FieldMarkType.FieldEnd)
		{
			m_iFieldSeparator--;
			if (m_iFieldSeparator >= 1)
			{
				(entity as ParagraphItem).OwnerParagraph.Items.Remove(entity);
			}
		}
	}

	private void AppendDateTimeField(FieldType fieldType)
	{
		if (base.FieldSeparator != null && base.FieldSeparator.ParentField == this)
		{
			int indexInOwnerCollection = base.FieldSeparator.GetIndexInOwnerCollection();
			WField wField = new WField(base.Document);
			wField.FieldType = fieldType;
			base.OwnerParagraph.ChildEntities.Insert(indexInOwnerCollection, wField);
			indexInOwnerCollection++;
			WTextRange entity = new WTextRange(base.Document);
			base.OwnerParagraph.ChildEntities.Insert(indexInOwnerCollection, entity);
			indexInOwnerCollection++;
			WFieldMark wFieldMark = null;
			wFieldMark = new WFieldMark(m_doc, FieldMarkType.FieldSeparator);
			base.OwnerParagraph.ChildEntities.Insert(indexInOwnerCollection, wFieldMark);
			indexInOwnerCollection++;
			entity = new WTextRange(base.Document);
			base.OwnerParagraph.ChildEntities.Insert(indexInOwnerCollection, entity);
			indexInOwnerCollection++;
			wField.FieldSeparator = wFieldMark;
			wFieldMark = new WFieldMark(m_doc, FieldMarkType.FieldEnd);
			base.OwnerParagraph.ChildEntities.Insert(indexInOwnerCollection, wFieldMark);
			wField.FieldEnd = wFieldMark;
			string empty = string.Empty;
			DateTime currentDateTime = (WordDocument.DisableDateTimeUpdating ? DateTime.MaxValue : DateTime.Now);
			empty = (string.IsNullOrEmpty(base.FormattingString) ? ((fieldType == FieldType.FieldDate) ? currentDateTime.ToString("d") : currentDateTime.ToString("t")) : UpdateDateField(base.FormattingString, currentDateTime));
			wField.UpdateFieldResult(empty);
			UpdateFieldResult(empty);
		}
	}

	internal void SetTextFormFieldType(TextFormFieldType txtFormFieldType)
	{
		m_formFieldType = txtFormFieldType;
	}

	protected override void ReadXmlAttributes(IXDLSAttributeReader reader)
	{
		base.ReadXmlAttributes(reader);
		if (reader.HasAttribute("MaxLength"))
		{
			m_maxLength = reader.ReadInt("MaxLength");
		}
		if (reader.HasAttribute("DefaultText"))
		{
			m_defText = reader.ReadString("DefaultText");
		}
		if (reader.HasAttribute("StringTextFormat"))
		{
			m_strTextFormat = reader.ReadString("StringTextFormat");
		}
		if (reader.HasAttribute("TextType"))
		{
			m_formFieldType = (TextFormFieldType)(object)reader.ReadEnum("TextType", typeof(TextFormFieldType));
		}
	}

	protected override void WriteXmlAttributes(IXDLSAttributeWriter writer)
	{
		base.WriteXmlAttributes(writer);
		writer.WriteValue("MaxLength", m_maxLength);
		writer.WriteValue("DefaultText", m_defText);
		writer.WriteValue("StringTextFormat", m_strTextFormat);
		writer.WriteValue("TextType", (int)m_formFieldType);
	}

	protected override void InitXDLSHolder()
	{
		base.XDLSHolder.AddElement("text-range", m_text);
	}

	internal override StringBuilder GetAsString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append('\u0015');
		stringBuilder.Append(DefaultText + ";");
		stringBuilder.Append(MaximumLength + ";");
		stringBuilder.Append((int)Type + ";");
		stringBuilder.Append((int)base.TextFormat + ";");
		stringBuilder.Append(base.Enabled ? "1" : "0;");
		stringBuilder.Append(base.CalculateOnExit ? "1" : "0;");
		stringBuilder.Append(base.Help + ";");
		stringBuilder.Append('\u0015');
		return stringBuilder;
	}
}
