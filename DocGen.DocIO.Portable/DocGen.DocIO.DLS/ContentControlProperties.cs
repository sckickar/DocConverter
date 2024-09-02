using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DocGen.DocIO.DLS.XML;
using DocGen.Drawing;
using DocGen.Office;

namespace DocGen.DocIO.DLS;

public class ContentControlProperties : XDLSSerializableBase
{
	internal const byte AppearanceKey = 0;

	internal const byte ColorKey = 1;

	internal const byte CheckedKey = 2;

	internal const byte CalendarTypeKey = 3;

	internal const byte DisplayFormatKey = 4;

	internal const byte DisplayLocaleKey = 5;

	internal const byte StorageFormatKey = 6;

	internal const byte LockContentControlKey = 7;

	internal const byte LockContentskey = 8;

	internal const byte IsTemporarykey = 9;

	internal const byte MultilineKey = 10;

	internal const byte IsShowingPlaceHolderTextKey = 11;

	internal const byte CheckedStateKey = 12;

	internal const byte UnCheckedStateKey = 13;

	private string m_title = string.Empty;

	private XmlMapping m_xmlMapping;

	private DocPartList m_DocPartList;

	private DocPartObj m_DocPartObj;

	private ContentControlListItems m_sdtListItem;

	private ContentControlType m_contentControlType;

	private string m_Label;

	private WCharacterFormat m_CharacterFormat;

	private uint m_TabIndex;

	private string m_Tag;

	private byte m_bFlags;

	private ContentRepeatingType m_ContentRepeatingType;

	private string m_id;

	private CalendarType m_calendarType;

	private string m_dateFormat;

	private LocaleIDs m_LID;

	private string m_fullDate;

	private string m_placeHolderDocPartId;

	private ContentControlDateStorageFormat m_dateStorage;

	private ContentControlAppearance m_appearance;

	private ushort m_flag;

	private Color m_color;

	private CheckBoxState m_checkedState;

	private CheckBoxState m_unCheckedState;

	private byte m_bFlagA;

	private Entity m_owner;

	private Dictionary<string, Stream> m_xmlProps;

	public CheckBoxState CheckedState
	{
		get
		{
			return m_checkedState;
		}
		set
		{
			if (base.Document != null && !base.Document.IsOpening && !base.Document.IsCloning && Type != ContentControlType.CheckBox)
			{
				throw new Exception("Checked property is available only for CheckBox content control");
			}
			m_flag = (ushort)((m_flag & 0xEFFFu) | 0x1000u);
			m_checkedState = value;
			if (IsChecked && !base.Document.IsOpening && !base.Document.IsCloning)
			{
				ChangeCheckboxState(IsChecked);
			}
		}
	}

	public CheckBoxState UncheckedState
	{
		get
		{
			return m_unCheckedState;
		}
		set
		{
			if (base.Document != null && !base.Document.IsOpening && !base.Document.IsCloning && Type != ContentControlType.CheckBox)
			{
				throw new Exception("Checked property is available only for CheckBox content control");
			}
			m_unCheckedState = value;
			m_flag = (ushort)((m_flag & 0xDFFFu) | 0x2000u);
			if (!IsChecked && !base.Document.IsOpening && !base.Document.IsCloning)
			{
				ChangeCheckboxState(IsChecked);
			}
		}
	}

	internal string ID
	{
		get
		{
			return m_id;
		}
		set
		{
			m_id = value;
		}
	}

	public string DateDisplayFormat
	{
		get
		{
			return m_dateFormat;
		}
		set
		{
			if (base.Document != null && !base.Document.IsOpening && Type != ContentControlType.Date)
			{
				throw new Exception("DateDisplayFormat property is available for Date content control alone");
			}
			m_flag = (ushort)((m_flag & 0xFFEFu) | 0x10u);
			m_dateFormat = value;
		}
	}

	public LocaleIDs DateDisplayLocale
	{
		get
		{
			return m_LID;
		}
		set
		{
			if (base.Document != null && !base.Document.IsOpening && Type != ContentControlType.Date)
			{
				throw new Exception("DateDisplayLocale property is available for Date content control alone");
			}
			m_flag = (ushort)((m_flag & 0xFFDFu) | 0x20u);
			m_LID = value;
		}
	}

	internal string FullDate
	{
		get
		{
			return m_fullDate;
		}
		set
		{
			m_fullDate = value;
		}
	}

	internal string PlaceHolderDocPartId
	{
		get
		{
			return m_placeHolderDocPartId;
		}
		set
		{
			m_placeHolderDocPartId = value;
		}
	}

	public ContentControlDateStorageFormat DateStorageFormat
	{
		get
		{
			return m_dateStorage;
		}
		set
		{
			if (base.Document != null && !base.Document.IsOpening && Type != ContentControlType.Date)
			{
				throw new Exception("DateStorageFormat property is available for Date content control alone");
			}
			m_flag = (ushort)((m_flag & 0xFFBFu) | 0x40u);
			m_dateStorage = value;
		}
	}

	public CalendarType DateCalendarType
	{
		get
		{
			return m_calendarType;
		}
		set
		{
			if (base.Document != null && !base.Document.IsOpening && Type != ContentControlType.Date)
			{
				throw new Exception("DateCalendarType property is available for Date content control alone");
			}
			m_flag = (ushort)((m_flag & 0xFFF7u) | 8u);
			m_calendarType = value;
		}
	}

	public bool IsChecked
	{
		get
		{
			return (m_bFlags & 0x80) >> 7 != 0;
		}
		set
		{
			if (base.Document != null && !base.Document.IsOpening && Type != ContentControlType.CheckBox)
			{
				throw new Exception("Checked property is available only for CheckBox content control");
			}
			m_flag = (ushort)((m_flag & 0xFFFBu) | 4u);
			m_bFlags = (byte)((m_bFlags & 0x7Fu) | ((value ? 1u : 0u) << 7));
			if (!base.Document.IsOpening)
			{
				ChangeCheckboxState(value);
			}
		}
	}

	public bool LockContentControl
	{
		get
		{
			return (m_bFlags & 0x40) >> 6 != 0;
		}
		set
		{
			if (value && IsTemporary)
			{
				IsTemporary = false;
			}
			m_flag = (ushort)((m_flag & 0xFF7Fu) | 0x80u);
			m_bFlags = (byte)((m_bFlags & 0xBFu) | ((value ? 1u : 0u) << 6));
		}
	}

	public bool LockContents
	{
		get
		{
			return (m_bFlags & 0x20) >> 5 != 0;
		}
		set
		{
			m_flag = (ushort)((m_flag & 0xFEFFu) | 0x100u);
			m_bFlags = (byte)((m_bFlags & 0xDFu) | ((value ? 1u : 0u) << 5));
		}
	}

	public bool Multiline
	{
		get
		{
			return (m_bFlags & 0x10) >> 4 != 0;
		}
		set
		{
			if (base.Document != null && !base.Document.IsOpening && !base.Document.IsCloning && (Type == ContentControlType.RichText || Type == ContentControlType.RepeatingSection || Type == ContentControlType.BuildingBlockGallery))
			{
				throw new Exception("Multiline property is not available for " + Type.ToString() + " content control");
			}
			m_flag = (ushort)((m_flag & 0xFBFFu) | 0x400u);
			m_bFlags = (byte)((m_bFlags & 0xEFu) | ((value ? 1u : 0u) << 4));
		}
	}

	public ContentControlAppearance Appearance
	{
		get
		{
			return m_appearance;
		}
		set
		{
			m_flag = (ushort)((m_flag & 0xFFFEu) | 1u);
			m_appearance = value;
		}
	}

	public Color Color
	{
		get
		{
			return m_color;
		}
		set
		{
			m_flag = (ushort)((m_flag & 0xFFFDu) | 2u);
			m_color = value;
		}
	}

	public string Title
	{
		get
		{
			return m_title;
		}
		set
		{
			m_title = value;
		}
	}

	internal bool Bibliograph
	{
		get
		{
			return (m_bFlags & 1) != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xFEu) | (value ? 1u : 0u));
		}
	}

	internal bool Citation
	{
		get
		{
			return (m_bFlags & 2) >> 1 != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xFDu) | ((value ? 1u : 0u) << 1));
		}
	}

	internal bool Unlocked
	{
		get
		{
			return (m_bFlagA & 2) >> 1 != 0;
		}
		set
		{
			m_bFlagA = (byte)((m_bFlagA & 0xFDu) | ((value ? 1u : 0u) << 1));
		}
	}

	public ContentControlListItems ContentControlListItems
	{
		get
		{
			if (base.Document != null && !base.Document.IsOpening && Type != ContentControlType.ComboBox && Type != ContentControlType.DropDownList)
			{
				throw new Exception("ContentControlListItems property is available only for ComboBox or DropDownList content controls");
			}
			if (m_sdtListItem == null)
			{
				m_sdtListItem = new ContentControlListItems();
			}
			return m_sdtListItem;
		}
	}

	public XmlMapping XmlMapping
	{
		get
		{
			if (m_xmlMapping == null)
			{
				m_xmlMapping = new XmlMapping(Owner);
			}
			return m_xmlMapping;
		}
		internal set
		{
			m_xmlMapping = value;
		}
	}

	internal Entity Owner => m_owner;

	internal DocPartList DocPartList
	{
		get
		{
			return m_DocPartList;
		}
		set
		{
			m_DocPartList = value;
		}
	}

	internal DocPartObj DocPartObj
	{
		get
		{
			return m_DocPartObj;
		}
		set
		{
			m_DocPartObj = value;
		}
	}

	public ContentControlType Type
	{
		get
		{
			return m_contentControlType;
		}
		internal set
		{
			m_contentControlType = value;
		}
	}

	internal string Label
	{
		get
		{
			return m_Label;
		}
		set
		{
			m_Label = value;
		}
	}

	internal WCharacterFormat CharacterFormat => m_CharacterFormat;

	public bool HasPlaceHolderText
	{
		get
		{
			return (m_bFlags & 4) >> 2 != 0;
		}
		internal set
		{
			m_flag = (ushort)((m_flag & 0xF7FFu) | 0x800u);
			m_bFlags = (byte)((m_bFlags & 0xFBu) | ((value ? 1u : 0u) << 2));
		}
	}

	internal uint TabIndex
	{
		get
		{
			return m_TabIndex;
		}
		set
		{
			m_TabIndex = value;
		}
	}

	public string Tag
	{
		get
		{
			return m_Tag;
		}
		set
		{
			m_Tag = value;
		}
	}

	public bool IsTemporary
	{
		get
		{
			return (m_bFlags & 8) >> 3 != 0;
		}
		set
		{
			if (value && LockContentControl)
			{
				LockContentControl = false;
			}
			m_flag = (ushort)((m_flag & 0xFDFFu) | 0x200u);
			m_bFlags = (byte)((m_bFlags & 0xF7u) | ((value ? 1u : 0u) << 3));
		}
	}

	internal ContentRepeatingType ContentRepeatingType
	{
		get
		{
			return m_ContentRepeatingType;
		}
		set
		{
			m_ContentRepeatingType = value;
		}
	}

	internal Dictionary<string, Stream> XmlProps
	{
		get
		{
			if (m_xmlProps == null)
			{
				m_xmlProps = new Dictionary<string, Stream>();
			}
			return m_xmlProps;
		}
	}

	internal ContentControlProperties(WordDocument doc, Entity ownerEntity)
		: base(doc, null)
	{
		m_CharacterFormat = new WCharacterFormat(doc);
		m_checkedState = new CheckBoxState();
		m_checkedState.ContentControlProperties = this;
		m_unCheckedState = new CheckBoxState();
		m_unCheckedState.ContentControlProperties = this;
		m_owner = ownerEntity;
	}

	internal ContentControlProperties(WordDocument doc)
		: base(doc, null)
	{
		m_CharacterFormat = new WCharacterFormat(doc);
		m_checkedState = new CheckBoxState();
		m_checkedState.ContentControlProperties = this;
		m_unCheckedState = new CheckBoxState();
		m_unCheckedState.ContentControlProperties = this;
	}

	internal void ChangeCheckboxState(bool isChecked)
	{
		string empty = string.Empty;
		string empty2 = string.Empty;
		if (isChecked)
		{
			empty = m_checkedState.Value;
			empty2 = m_checkedState.Font;
		}
		else
		{
			empty = m_unCheckedState.Value;
			empty2 = m_unCheckedState.Font;
		}
		if (m_owner is InlineContentControl inlineContentControl)
		{
			if (!string.IsNullOrEmpty(empty) && !string.IsNullOrEmpty(empty2))
			{
				if (inlineContentControl.ParagraphItems.Count != 0 && inlineContentControl.ParagraphItems[0] is WTextRange)
				{
					(inlineContentControl.ParagraphItems[0] as WTextRange).Text = empty;
					(inlineContentControl.ParagraphItems[0] as WTextRange).CharacterFormat.FontName = empty2;
					return;
				}
				WTextRange wTextRange = new WTextRange(base.Document);
				wTextRange.Text = empty;
				wTextRange.CharacterFormat.FontName = empty2;
				inlineContentControl.ParagraphItems.Add(wTextRange);
			}
		}
		else if (Owner is CellContentControl)
		{
			WTableCell ownerCell = (Owner as CellContentControl).OwnerCell;
			InsertParagraph(ownerCell, empty, empty2);
		}
		else if (Owner is BlockContentControl)
		{
			InsertParagraph((Owner as BlockContentControl).TextBody, empty, empty2);
		}
	}

	private void InsertParagraph(WTextBody textBody, string text, string fontName)
	{
		if (textBody.ChildEntities.Count > 0 && textBody.ChildEntities[0] is WParagraph)
		{
			bool flag = false;
			WParagraph wParagraph = textBody.ChildEntities[0] as WParagraph;
			for (int i = 0; i < wParagraph.ChildEntities.Count; i++)
			{
				if (wParagraph.ChildEntities[i] is WTextRange)
				{
					WTextRange obj = wParagraph.ChildEntities[i] as WTextRange;
					obj.Text = text;
					obj.CharacterFormat.FontName = fontName;
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				WTextRange wTextRange = new WTextRange(base.Document);
				wTextRange.Text = text;
				wTextRange.CharacterFormat.FontName = fontName;
				wParagraph.ChildEntities.Insert(0, wTextRange);
			}
		}
		else
		{
			WParagraph wParagraph2 = new WParagraph(base.Document);
			WTextRange wTextRange2 = new WTextRange(base.Document);
			wTextRange2.Text = text;
			wTextRange2.CharacterFormat.FontName = fontName;
			wParagraph2.ChildEntities.Insert(0, wTextRange2);
			textBody.ChildEntities.Insert(0, wParagraph2);
		}
	}

	internal bool HasKey(int propertyKey)
	{
		return (m_flag & (ushort)Math.Pow(2.0, propertyKey)) >> propertyKey != 0;
	}

	internal ContentControlProperties Clone()
	{
		return (ContentControlProperties)CloneImpl();
	}

	protected new object CloneImpl()
	{
		ContentControlProperties contentControlProperties = (ContentControlProperties)MemberwiseClone();
		if (m_sdtListItem != null)
		{
			contentControlProperties.m_sdtListItem = new ContentControlListItems();
			contentControlProperties.m_sdtListItem = m_sdtListItem.Clone();
		}
		if (m_CharacterFormat != null)
		{
			contentControlProperties.m_CharacterFormat = new WCharacterFormat(m_CharacterFormat.Document);
			contentControlProperties.m_CharacterFormat.ImportContainer(CharacterFormat);
			contentControlProperties.m_CharacterFormat.CopyProperties(CharacterFormat);
		}
		return contentControlProperties;
	}

	internal new void CloneRelationsTo(WordDocument doc, OwnerHolder nextOwner)
	{
		if (m_CharacterFormat != null && !string.IsNullOrEmpty(m_CharacterFormat.CharStyleName))
		{
			if (m_CharacterFormat != null)
			{
				m_CharacterFormat.CloneRelationsTo(doc, nextOwner);
			}
			m_CharacterFormat.SetOwner(doc);
		}
	}

	internal void SetOwnerContentControl(Entity owner)
	{
		m_owner = owner;
	}

	internal new void Close()
	{
		if (m_xmlMapping != null)
		{
			m_xmlMapping = null;
		}
		if (m_DocPartList != null)
		{
			m_DocPartList = null;
		}
		if (m_DocPartObj != null)
		{
			m_DocPartObj = null;
		}
		if (m_sdtListItem != null)
		{
			m_sdtListItem.Close();
			m_sdtListItem = null;
		}
		if (m_CharacterFormat != null)
		{
			m_CharacterFormat.Close();
			m_CharacterFormat = null;
		}
		if (m_xmlProps != null)
		{
			m_xmlProps.Clear();
			m_xmlProps = null;
		}
	}

	internal StringBuilder GetAsString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append((int)Type + ";");
		stringBuilder.Append((int)Appearance + ";");
		if (DateDisplayFormat != null)
		{
			stringBuilder.Append(DateDisplayFormat.ToString() + ";");
		}
		string text = (IsChecked ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (Multiline ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (Citation ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (IsTemporary ? "1" : "0");
		stringBuilder.Append(text + ";");
		if (CheckedState != null)
		{
			CheckedState.GetAsString();
		}
		if (UncheckedState != null)
		{
			UncheckedState.GetAsString();
		}
		return stringBuilder;
	}

	internal bool Compare(ContentControlProperties contentControlProperties)
	{
		if (Type != contentControlProperties.Type || Appearance != contentControlProperties.Appearance || DateDisplayFormat != contentControlProperties.DateDisplayFormat || IsChecked != contentControlProperties.IsChecked || Multiline != contentControlProperties.Multiline || Citation != contentControlProperties.Citation || IsTemporary != contentControlProperties.IsTemporary)
		{
			return false;
		}
		if (CheckedState == null || contentControlProperties.CheckedState == null || UncheckedState == null || contentControlProperties.UncheckedState == null)
		{
			return false;
		}
		if (CheckedState != null && contentControlProperties.CheckedState != null && !CheckedState.Compare(contentControlProperties.CheckedState))
		{
			return false;
		}
		if (UncheckedState != null && contentControlProperties.UncheckedState != null && !UncheckedState.Compare(contentControlProperties.UncheckedState))
		{
			return false;
		}
		return true;
	}
}
