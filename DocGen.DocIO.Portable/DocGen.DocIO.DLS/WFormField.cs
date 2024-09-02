using System.Text;
using DocGen.DocIO.DLS.XML;
using DocGen.DocIO.ReaderWriter.Biff_Records;

namespace DocGen.DocIO.DLS;

public abstract class WFormField : WField
{
	internal const int DEF_VALUE = 25;

	protected FormFieldType m_curFormFieldType;

	private short m_params;

	private string m_title;

	private string m_help;

	private string m_tooltip;

	private string m_macroOnStart;

	private string m_macroOnEnd;

	private bool m_bHasFFData = true;

	public FormFieldType FormFieldType => m_curFormFieldType;

	public string Name
	{
		get
		{
			return m_title;
		}
		set
		{
			ApplyNewBookmarkName(m_title, value);
			m_title = value;
		}
	}

	public string Help
	{
		get
		{
			return m_help;
		}
		set
		{
			m_help = value;
			m_params = (short)BaseWordRecord.SetBitsByMask(m_params, 128, 7, 1);
		}
	}

	public string StatusBarHelp
	{
		get
		{
			return m_tooltip;
		}
		set
		{
			m_tooltip = value;
			m_params = (short)BaseWordRecord.SetBitsByMask(m_params, 256, 8, 1);
		}
	}

	public string MacroOnStart
	{
		get
		{
			return m_macroOnStart;
		}
		set
		{
			m_macroOnStart = value;
		}
	}

	public string MacroOnEnd
	{
		get
		{
			return m_macroOnEnd;
		}
		set
		{
			m_macroOnEnd = value;
		}
	}

	internal int Value
	{
		get
		{
			return (m_params & 0x7C) >> 2;
		}
		set
		{
			m_params = (short)((m_params & -125) | (value << 2));
		}
	}

	internal int Params
	{
		get
		{
			return m_params;
		}
		set
		{
			m_params = (short)value;
		}
	}

	public bool Enabled
	{
		get
		{
			return (m_params & 0x200) == 0;
		}
		set
		{
			int value2 = ((!value) ? 1 : 0);
			m_params = (short)BaseWordRecord.SetBitsByMask(m_params, 512, 9, value2);
		}
	}

	public bool CalculateOnExit
	{
		get
		{
			return (m_params & 0x4000) == 16384;
		}
		set
		{
			m_params = (value ? ((short)BaseWordRecord.SetBitsByMask(m_params, 16384, 14, 1)) : ((short)BaseWordRecord.SetBitsByMask(m_params, 16384, 14, 0)));
		}
	}

	internal bool HasFFData
	{
		get
		{
			return m_bHasFFData;
		}
		set
		{
			m_bHasFFData = value;
		}
	}

	public WFormField(IWordDocument doc)
		: base(doc)
	{
		m_paraItemType = ParagraphItemType.FormField;
		m_title = string.Empty;
		m_help = string.Empty;
		m_tooltip = string.Empty;
		m_macroOnStart = string.Empty;
		m_macroOnEnd = string.Empty;
	}

	protected WFormField(WFormField formField, IWordDocument doc)
		: this(doc)
	{
		Help = formField.Help;
		MacroOnEnd = formField.MacroOnEnd;
		MacroOnStart = formField.MacroOnStart;
		Params = formField.Params;
		Name = formField.Name;
		StatusBarHelp = formField.StatusBarHelp;
		Value = formField.Value;
		base.FieldType = formField.FieldType;
	}

	protected override object CloneImpl()
	{
		WFormField obj = (WFormField)base.CloneImpl();
		obj.CharacterFormat.ImportContainer(base.CharacterFormat);
		return obj;
	}

	internal override void AttachToParagraph(WParagraph paragraph, int itemPos)
	{
		base.AttachToParagraph(paragraph, itemPos);
		if (!base.Document.IsOpening)
		{
			AttachForTextBody(paragraph.Owner as WTextBody);
		}
	}

	internal override void Detach()
	{
		base.Detach();
		WParagraph ownerParagraph = base.OwnerParagraph;
		DetachForTextBody(ownerParagraph.Owner as WTextBody);
	}

	private void AttachForTextBody(WTextBody textBody)
	{
		if (textBody != null && textBody.IsFormFieldsCreated)
		{
			textBody.FormFields.Add(this);
			if (textBody is WTableCell && textBody.Owner.Owner is WTable wTable)
			{
				AttachForTextBody(wTable.Owner as WTextBody);
			}
		}
	}

	private void DetachForTextBody(WTextBody textBody)
	{
		if (textBody != null)
		{
			if (textBody.IsFormFieldsCreated)
			{
				textBody.FormFields.Remove(this);
			}
			if (textBody is WTableCell)
			{
				DetachForTextBody(textBody.Owner.Owner.Owner as WTextBody);
			}
		}
	}

	protected override void ReadXmlAttributes(IXDLSAttributeReader reader)
	{
		base.ReadXmlAttributes(reader);
		if (reader.HasAttribute("Params"))
		{
			Params = reader.ReadInt("Params");
		}
		if (reader.HasAttribute("Title"))
		{
			m_title = reader.ReadString("Title");
		}
		if (reader.HasAttribute("Help"))
		{
			m_help = reader.ReadString("Help");
		}
		if (reader.HasAttribute("Tooltip"))
		{
			m_tooltip = reader.ReadString("Tooltip");
		}
		if (reader.HasAttribute("MacroOnStart"))
		{
			m_macroOnStart = reader.ReadString("MacroOnStart");
		}
		if (reader.HasAttribute("MacroOnEnd"))
		{
			m_macroOnEnd = reader.ReadString("MacroOnEnd");
		}
	}

	protected override void WriteXmlAttributes(IXDLSAttributeWriter writer)
	{
		base.WriteXmlAttributes(writer);
		writer.WriteValue("Params", m_params);
		writer.WriteValue("Title", m_title);
		writer.WriteValue("Help", m_help);
		writer.WriteValue("Tooltip", m_tooltip);
		writer.WriteValue("MacroOnStart", m_macroOnStart);
		writer.WriteValue("MacroOnEnd", m_macroOnEnd);
	}

	private void ApplyNewBookmarkName(string oldName, string newName)
	{
		if ((base.Document == null || !base.Document.IsOpening) && base.Owner is WParagraph)
		{
			CheckFormFieldName(newName);
			bool flag = false;
			if (base.Document != null)
			{
				flag = ApplyInDocBkmkColl(oldName, newName);
			}
			if (!flag)
			{
				ApplyInOwnerParaColl(oldName, newName);
			}
			if (base.OwnerParagraph.OwnerTextBody != null && base.OwnerParagraph.OwnerTextBody.IsFormFieldsCreated)
			{
				base.OwnerParagraph.OwnerTextBody.FormFields.CorrectName(oldName, newName);
			}
		}
	}

	private bool ApplyInDocBkmkColl(string oldName, string newName)
	{
		bool result = false;
		BookmarkCollection bookmarks = base.Document.Bookmarks;
		if (bookmarks.Count > 0)
		{
			Bookmark bookmark = bookmarks[oldName];
			if (bookmark != null && bookmark.BookmarkStart != null && bookmark.BookmarkEnd != null)
			{
				bookmark.BookmarkStart.SetName(newName);
				bookmark.BookmarkEnd.SetName(newName);
				result = true;
			}
		}
		return result;
	}

	private void ApplyInOwnerParaColl(string oldName, string newName)
	{
		BookmarkStart bookmarkStart = null;
		BookmarkEnd bookmarkEnd = null;
		foreach (IParagraphItem item in base.OwnerParagraph.Items)
		{
			if (item is BookmarkStart && (item as BookmarkStart).Name == oldName)
			{
				bookmarkStart = item as BookmarkStart;
			}
			else if (item is BookmarkEnd && (item as BookmarkEnd).Name == oldName)
			{
				bookmarkEnd = item as BookmarkEnd;
				if (bookmarkStart != null)
				{
					break;
				}
			}
		}
		if (bookmarkStart != null && bookmarkEnd != null)
		{
			bookmarkStart.SetName(newName);
			bookmarkEnd.SetName(newName);
		}
	}

	private void CheckFormFieldName(string newName)
	{
		Bookmark bookmark = base.Document.Bookmarks[newName];
		if (bookmark == null)
		{
			return;
		}
		base.Document.Bookmarks.Remove(bookmark);
		foreach (WSection section in base.Document.Sections)
		{
			if (section.Body.FormFields != null)
			{
				WFormField wFormField = section.Body.FormFields[newName];
				if (wFormField != null)
				{
					wFormField.Name = string.Empty;
				}
			}
		}
	}

	internal abstract StringBuilder GetAsString();
}
