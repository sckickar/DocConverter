using System;
using DocGen.DocIO.DLS.XML;

namespace DocGen.DocIO.DLS;

public class ListStyle : XDLSSerializableBase, IStyle
{
	private const int DEF_MULTIPLIER = 72;

	internal const string DEF_BULLLET_FIRST = "\uf0b7";

	internal const string DEF_BULLLET_SECOND = "o";

	internal const string DEF_BULLLET_THIRD = "\uf0a7";

	private ListLevelCollection m_levels;

	private ListType m_listType;

	private string m_name;

	private string m_baseLstStyle;

	private byte m_bFlags;

	private long m_listId = 1720085641L;

	private string m_styleLink;

	internal long ListID
	{
		get
		{
			return m_listId;
		}
		set
		{
			m_listId = value;
		}
	}

	internal string StyleLink
	{
		get
		{
			return m_styleLink;
		}
		set
		{
			m_styleLink = value;
		}
	}

	public string Name
	{
		get
		{
			return m_name;
		}
		set
		{
			m_name = value;
		}
	}

	public ListType ListType
	{
		get
		{
			return m_listType;
		}
		set
		{
			m_listType = value;
		}
	}

	public ListLevelCollection Levels => m_levels;

	public StyleType StyleType => StyleType.OtherStyle;

	internal bool IsHybrid
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

	internal bool IsSimple
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

	internal bool IsBuiltInStyle
	{
		get
		{
			return (m_bFlags & 4) >> 2 != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xFBu) | ((value ? 1u : 0u) << 2));
		}
	}

	internal string BaseListStyleName
	{
		get
		{
			return m_baseLstStyle;
		}
		set
		{
			m_baseLstStyle = value;
		}
	}

	public ListStyle(IWordDocument doc, ListType listType)
		: this((WordDocument)doc)
	{
		m_listType = listType;
		CreateDefListLevels(listType);
	}

	internal ListStyle(WordDocument doc, ListType listType, bool isOneLevelList)
		: this(doc)
	{
		m_listType = listType;
		CreateEmptyListLevels(isOneLevelList);
	}

	internal ListStyle(WordDocument doc)
		: base(doc, doc)
	{
		m_levels = new ListLevelCollection(this);
		m_levels.SetOwner(this);
		SetNewListID(doc);
	}

	public static ListStyle CreateEmptyListStyle(IWordDocument doc, ListType listType, bool isOneLevelList)
	{
		return new ListStyle((WordDocument)doc, listType, isOneLevelList);
	}

	public IStyle Clone()
	{
		return CloneImpl() as IStyle;
	}

	void IStyle.Close()
	{
		Close();
	}

	internal new void Close()
	{
		if (m_levels == null || m_levels.Count == 0)
		{
			m_levels = null;
			return;
		}
		int count = m_levels.Count;
		for (int i = 0; i < count; i++)
		{
			m_levels[i].Close();
		}
		m_levels.Close();
		m_levels = null;
	}

	protected override object CloneImpl()
	{
		ListStyle listStyle = (ListStyle)base.CloneImpl();
		listStyle.m_levels = new ListLevelCollection(listStyle);
		m_levels.CloneToImpl(listStyle.m_levels);
		return listStyle;
	}

	protected override void InitXDLSHolder()
	{
		base.InitXDLSHolder();
		base.XDLSHolder.AddElement("levels", Levels);
	}

	protected override void WriteXmlAttributes(IXDLSAttributeWriter writer)
	{
		base.WriteXmlAttributes(writer);
		writer.WriteValue("Name", Name);
		writer.WriteValue("ListType", ListType);
	}

	protected override void ReadXmlAttributes(IXDLSAttributeReader reader)
	{
		base.ReadXmlAttributes(reader);
		m_name = reader.ReadString("Name");
		ListType = (ListType)(object)reader.ReadEnum("ListType", typeof(ListType));
	}

	internal void CreateDefListLevels(ListType listType)
	{
		Levels.Clear();
		base.Document.CreateListLevelImpl(this);
		if (listType == ListType.Bulleted)
		{
			for (float num = 0.5f; num < 4.5f; num += 1.5f)
			{
				Levels.Add(WListLevel.CreateDefBulletLvl((int)(72f * num), "\uf0b7", this));
				Levels.Add(WListLevel.CreateDefBulletLvl((int)(72.0 * ((double)num + 0.5)), "o", this));
				Levels.Add(WListLevel.CreateDefBulletLvl((int)(72f * (num + 1f)), "\uf0a7", this));
			}
			return;
		}
		int num2 = 0;
		for (float num3 = 0.5f; num3 < 4.5f; num3 += 1.5f)
		{
			Levels.Add(WListLevel.CreateDefNumberLvl((int)(72f * num3), num2++, ListPatternType.Arabic, ListNumberAlignment.Left, this));
			Levels.Add(WListLevel.CreateDefNumberLvl((int)(72.0 * ((double)num3 + 0.5)), num2++, ListPatternType.LowLetter, ListNumberAlignment.Right, this));
			Levels.Add(WListLevel.CreateDefNumberLvl((int)(72f * (num3 + 1f)), num2++, ListPatternType.LowRoman, ListNumberAlignment.Left, this));
		}
	}

	public WListLevel GetNearLevel(int levelNumber)
	{
		if (levelNumber < 0)
		{
			throw new ArgumentOutOfRangeException("number", "Value can not be less than 0");
		}
		if (levelNumber > Levels.Count - 1)
		{
			levelNumber = Levels.Count - 1;
		}
		return Levels[levelNumber];
	}

	internal void CreateEmptyListLevels(bool isOneLevelList)
	{
		int num = (isOneLevelList ? 1 : 9);
		for (int i = 0; i < num; i++)
		{
			Levels.Add(base.Document.CreateListLevelImpl(this));
		}
	}

	internal override void CloneRelationsTo(WordDocument doc, OwnerHolder nextOwner)
	{
		if (doc == base.Document)
		{
			return;
		}
		SetOwner(doc);
		Levels.SetOwner(this);
		for (int i = 0; i < Levels.Count; i++)
		{
			Levels[i].SetOwner(this);
			Levels[i].CharacterFormat.SetOwner(Levels[i]);
			Levels[i].ParagraphFormat.SetOwner(Levels[i]);
			if (Levels[i].PicBullet != null)
			{
				Levels[i].PicBullet.CloneRelationsTo(base.Document, Levels[i]);
			}
		}
	}

	internal bool Compare(ListStyle listStyle)
	{
		if (ListID != listStyle.ListID)
		{
			return false;
		}
		if (IsHybrid != listStyle.IsHybrid)
		{
			return false;
		}
		if (IsSimple != listStyle.IsSimple)
		{
			return false;
		}
		if (ListType != listStyle.ListType)
		{
			return false;
		}
		if (Levels != null && listStyle.Levels != null && !Levels.Compare(listStyle.Levels))
		{
			return false;
		}
		return true;
	}

	internal bool IsSameListNameOrIDExists(ListStyleCollection docListStyles, long listID, string styleName)
	{
		bool flag = string.IsNullOrEmpty(styleName);
		foreach (ListStyle docListStyle in docListStyles)
		{
			if (flag ? (listID == docListStyle.ListID) : (styleName == docListStyle.Name))
			{
				return true;
			}
		}
		return false;
	}

	internal void SetNewName(WordDocument doc)
	{
		while (IsSameListNameOrIDExists(doc.ListStyles, 0L, Name))
		{
			Name = ((ListType == ListType.Bulleted) ? ("Bulleted_" + Guid.NewGuid()) : ("Numbered_" + Guid.NewGuid()));
		}
	}

	internal void SetNewListID(WordDocument destDocument)
	{
		Random random = new Random();
		long listID = random.Next();
		if (destDocument.m_listStyles != null)
		{
			while (IsSameListNameOrIDExists(destDocument.m_listStyles, listID, null))
			{
				listID = random.Next();
			}
		}
		ListID = listID;
	}

	void IStyle.Remove()
	{
	}
}
