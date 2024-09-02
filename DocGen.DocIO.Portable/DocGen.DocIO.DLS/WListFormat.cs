using System;
using DocGen.DocIO.DLS.XML;

namespace DocGen.DocIO.DLS;

public class WListFormat : FormatBase
{
	internal const int ListLevelNumberKey = 0;

	private const int ListTypeKey = 1;

	internal const int CustomStyleNameKey = 2;

	private const int RestartKey = 3;

	internal const int LfoStyleNameKey = 4;

	internal const int DEF_START_LISTID = 1720085641;

	[ThreadStatic]
	internal static string m_currentStyleName;

	[ThreadStatic]
	internal static int m_currLevelNumber;

	private byte m_bFlags;

	public int ListLevelNumber
	{
		get
		{
			return (int)base[0];
		}
		set
		{
			if (value > 8 || value < 0)
			{
				throw new ArgumentException("List level must be less 8 and greater then 0");
			}
			base[0] = value;
			m_currLevelNumber = value;
		}
	}

	public ListType ListType => (ListType)base[1];

	public bool RestartNumbering
	{
		get
		{
			return (bool)base[3];
		}
		set
		{
			if (value && CurrentListStyle != null)
			{
				ListStyle listStyle = CurrentListStyle.Clone() as ListStyle;
				listStyle.Name = ((CurrentListStyle.ListType == ListType.Bulleted) ? ("Bulleted_" + Guid.NewGuid()) : ("Numbered_" + Guid.NewGuid()));
				listStyle.SetNewListID(base.Document);
				m_doc.ListStyles.Add(listStyle);
				base[2] = listStyle.Name;
				m_currentStyleName = listStyle.Name;
				base[1] = listStyle.ListType;
			}
			base[3] = value;
		}
	}

	public string CustomStyleName => (string)base[2];

	public ListStyle CurrentListStyle
	{
		get
		{
			if ((string)base[2] != string.Empty)
			{
				return base.Document.ListStyles.FindByName(CustomStyleName);
			}
			return null;
		}
	}

	public WListLevel CurrentListLevel
	{
		get
		{
			if ((string)base[2] == string.Empty)
			{
				return null;
			}
			if (ListLevelNumber >= CurrentListStyle.Levels.Count)
			{
				return null;
			}
			return CurrentListStyle.Levels[ListLevelNumber];
		}
	}

	internal string LFOStyleName
	{
		get
		{
			return (string)base[4];
		}
		set
		{
			base[4] = value;
		}
	}

	internal WParagraph OwnerParagraph => (WParagraph)base.OwnerBase;

	internal bool IsListRemoved
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

	internal bool IsEmptyList
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

	public WListFormat(IWParagraph owner)
		: base(owner.Document, (Entity)owner)
	{
	}

	public WListFormat(WordDocument doc, WParagraphStyle owner)
		: base(doc)
	{
		SetOwner(owner);
	}

	internal WListFormat(WordDocument doc, WNumberingStyle owner)
		: base(doc)
	{
		SetOwner(owner);
	}

	internal WListFormat(WordDocument doc, WTableStyle owner)
		: base(doc)
	{
		SetOwner(owner);
	}

	protected override object GetDefValue(int key)
	{
		return key switch
		{
			0 => 0, 
			1 => ListType.NoList, 
			3 => false, 
			2 => string.Empty, 
			4 => null, 
			_ => throw new ArgumentException("key has invalid value"), 
		};
	}

	protected override void WriteXmlAttributes(IXDLSAttributeWriter writer)
	{
		base.WriteXmlAttributes(writer);
		if (HasKey(0))
		{
			writer.WriteValue("LevelNumber", ListLevelNumber);
		}
		if (HasKey(2))
		{
			writer.WriteValue("Name", CustomStyleName);
		}
		if (HasKey(1))
		{
			writer.WriteValue("ListType", ListType);
		}
		if (HasKey(4))
		{
			writer.WriteValue("LfoStyleName", LFOStyleName);
		}
	}

	protected override void ReadXmlAttributes(IXDLSAttributeReader reader)
	{
		base.ReadXmlAttributes(reader);
		if (reader.HasAttribute("LfoStyleName"))
		{
			LFOStyleName = reader.ReadString("LfoStyleName");
		}
		if (reader.HasAttribute("LevelNumber"))
		{
			base[0] = reader.ReadInt("LevelNumber");
		}
		if (reader.HasAttribute("Name"))
		{
			base[2] = reader.ReadString("Name");
		}
		if (reader.HasAttribute("ListType"))
		{
			base[1] = (ListType)(object)reader.ReadEnum("ListType", typeof(ListType));
		}
	}

	private void UpdateStyleNameAndType(ListStyle destinationListStyle)
	{
		base[2] = destinationListStyle.Name;
		m_currentStyleName = destinationListStyle.Name;
		base[1] = destinationListStyle.ListType;
	}

	private bool IsListStyleAdded(WordDocument destDocument, string name, ref string newStyleName)
	{
		if (string.IsNullOrEmpty(destDocument.Settings.DuplicateListStyleNames))
		{
			return false;
		}
		string[] array = destDocument.Settings.DuplicateListStyleNames.Split(',');
		for (int i = 0; i + 1 < array.Length; i += 2)
		{
			if (array[i] == name)
			{
				newStyleName = array[i + 1];
				return true;
			}
		}
		return false;
	}

	private void AddListStyleToDestination(WordDocument doc, ListStyle destinationListStyle)
	{
		string text = destinationListStyle.Name;
		while (destinationListStyle.IsSameListNameOrIDExists(doc.ListStyles, 0L, text))
		{
			text = ((destinationListStyle.ListType == ListType.Bulleted) ? ("Bulleted_" + Guid.NewGuid()) : ("Numbered_" + Guid.NewGuid()));
		}
		Settings settings = doc.Settings;
		settings.DuplicateListStyleNames = settings.DuplicateListStyleNames + destinationListStyle.Name + "," + text + ",";
		if (text != destinationListStyle.Name)
		{
			destinationListStyle.Name = text;
			UpdateStyleNameAndType(destinationListStyle);
		}
		doc.ListStyles.Add(destinationListStyle);
	}

	internal void CloneListRelationsTo(WordDocument doc, string styleName)
	{
		SetOwnerDoc(doc);
		if (ListType != ListType.NoList && CurrentListStyle != null)
		{
			ListStyle currentListStyle = CurrentListStyle;
			ListStyle listStyle = doc.ListStyles.GetEquivalentStyle(currentListStyle);
			if (currentListStyle != null)
			{
				if (listStyle == null || ((doc.ImportOptions & ImportOptions.ListRestartNumbering) != 0 && listStyle.ListType == ListType.Numbered))
				{
					string newStyleName = string.Empty;
					if (listStyle == null)
					{
						listStyle = (ListStyle)currentListStyle.Clone();
						if (doc.ListStyles.HasSameListId(listStyle))
						{
							listStyle.SetNewListID(doc);
						}
					}
					else
					{
						listStyle = (ListStyle)currentListStyle.Clone();
						listStyle.SetNewListID(doc);
					}
					if (IsListStyleAdded(doc, listStyle.Name, ref newStyleName))
					{
						listStyle = doc.ListStyles.FindByName(newStyleName);
						UpdateStyleNameAndType(listStyle);
					}
					else
					{
						AddListStyleToDestination(doc, listStyle);
					}
				}
				else
				{
					UpdateStyleNameAndType(listStyle);
				}
				if (styleName != null && listStyle != null && ListLevelNumber < listStyle.Levels.Count && listStyle.Levels[ListLevelNumber] != null)
				{
					listStyle.Levels[ListLevelNumber].ParaStyleName = styleName;
				}
			}
		}
		if (LFOStyleName == null)
		{
			return;
		}
		ListOverrideStyle listOverrideStyle = base.Document.ListOverrides.FindByName(LFOStyleName);
		if (listOverrideStyle != null)
		{
			ListOverrideStyle equivalentStyle = doc.ListOverrides.GetEquivalentStyle(listOverrideStyle);
			if (equivalentStyle == null)
			{
				doc.ListOverrides.Add((ListOverrideStyle)listOverrideStyle.Clone());
			}
			else
			{
				LFOStyleName = equivalentStyle.Name;
			}
		}
	}

	internal void ImportListFormat(WListFormat srcListFormat)
	{
		ImportContainer(srcListFormat);
		if (srcListFormat.ListType != ListType.NoList)
		{
			ListStyle currentListStyle = srcListFormat.CurrentListStyle;
			if (currentListStyle != null && base.Document.ListStyles.FindByName(currentListStyle.Name) == null)
			{
				base.Document.ListStyles.Add((ListStyle)currentListStyle.Clone());
			}
		}
		if (srcListFormat.LFOStyleName != null && srcListFormat.Document != null && srcListFormat.Document.ListOverrides != null && base.Document.ListOverrides.FindByName(srcListFormat.LFOStyleName) == null)
		{
			ListOverrideStyle listOverrideStyle = srcListFormat.Document.ListOverrides.FindByName(srcListFormat.LFOStyleName);
			if (listOverrideStyle != null)
			{
				base.Document.ListOverrides.Add((ListOverrideStyle)listOverrideStyle.Clone());
			}
		}
	}

	public void IncreaseIndentLevel()
	{
		if (m_currLevelNumber == 8)
		{
			throw new ArgumentException("List level must be less 8 and greater then 0");
		}
		base[0] = ++m_currLevelNumber;
	}

	public void DecreaseIndentLevel()
	{
		if (m_currLevelNumber == 0)
		{
			throw new ArgumentException("List level must be less 8 and greater then 0");
		}
		base[0] = --m_currLevelNumber;
	}

	public void ContinueListNumbering()
	{
		ApplyStyle(m_currentStyleName);
		ListLevelNumber = m_currLevelNumber;
	}

	public void ApplyStyle(string styleName)
	{
		base[2] = styleName;
		m_currentStyleName = styleName;
		ListStyle listStyle = null;
		if (!string.IsNullOrEmpty(m_currentStyleName))
		{
			listStyle = base.Document.ListStyles.FindByName(styleName);
			Style style = (Style)base.Document.Styles.FindByName(listStyle.StyleLink);
			if (style != null && style.IsCustom && base.OwnerBase is WParagraph && style.RangeCollection.Contains(OwnerParagraph))
			{
				style.RangeCollection.Remove(OwnerParagraph);
			}
		}
		ListStyle listStyle2 = null;
		if (!string.IsNullOrEmpty(styleName))
		{
			listStyle2 = base.Document.ListStyles.FindByName(styleName);
			if (base.OwnerBase is WParagraph)
			{
				Style style2 = (Style)base.Document.Styles.FindByName(listStyle2.StyleLink);
				if (style2 != null && style2.IsCustom)
				{
					style2.RangeCollection.Add(OwnerParagraph);
				}
			}
		}
		if (listStyle2 != null)
		{
			base[1] = listStyle2.ListType;
		}
	}

	public void ApplyDefBulletStyle()
	{
		if (base.Document.ListStyles.FindByName("Bulleted") == null)
		{
			CreateDefListStyles(ListType.Bulleted);
		}
		ApplyStyle("Bulleted");
	}

	public void ApplyDefNumberedStyle()
	{
		if (base.Document.ListStyles.FindByName("Numbered") == null)
		{
			CreateDefListStyles(ListType.Numbered);
		}
		ApplyStyle("Numbered");
	}

	public void RemoveList()
	{
		base[2] = string.Empty;
		m_currentStyleName = string.Empty;
		base[1] = ListType.NoList;
		IsListRemoved = true;
	}

	internal void SetListTypeKey(ListType listType)
	{
		base[1] = listType;
	}

	public override void ClearFormatting()
	{
		if (m_propertiesHash != null)
		{
			m_propertiesHash.Clear();
		}
		if (m_sprms != null)
		{
			m_sprms.Clear();
		}
		RemoveList();
	}

	private void CreateDefListStyles(ListType listType)
	{
		if (listType == ListType.Numbered)
		{
			ListStyle listStyle = new ListStyle(base.Document, ListType.Numbered);
			listStyle.Name = "Numbered";
			listStyle.ListType = ListType.Numbered;
			base.Document.ListStyles.Add(listStyle);
		}
		else
		{
			ListStyle listStyle2 = new ListStyle(base.Document, ListType.Bulleted);
			listStyle2.Name = "Bulleted";
			listStyle2.ListType = ListType.Bulleted;
			base.Document.ListStyles.Add(listStyle2);
		}
	}

	internal bool Compare(WListFormat listFormat)
	{
		if (ListLevelNumber != listFormat.ListLevelNumber)
		{
			return false;
		}
		if (listFormat.ListType != ListType.NoList && ListType != ListType.NoList)
		{
			ListStyle currentListStyle = listFormat.CurrentListStyle;
			if (currentListStyle != null && !base.Document.ListStyles.HasEquivalentStyle(currentListStyle))
			{
				return false;
			}
		}
		if ((listFormat.ListType == ListType.NoList && ListType != ListType.NoList) || (listFormat.ListType != ListType.NoList && ListType == ListType.NoList))
		{
			return false;
		}
		ListOverrideStyle listOverrideStyle = base.Document.ListOverrides.FindByName(LFOStyleName);
		if (listOverrideStyle != null && !listFormat.Document.ListOverrides.HasEquivalentStyle(listOverrideStyle))
		{
			return false;
		}
		if (listFormat.Document.IsComparing && !Compare(3, listFormat))
		{
			return false;
		}
		return true;
	}
}
