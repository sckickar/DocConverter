using System.Collections.Generic;
using DocGen.DocIO.DLS.XML;

namespace DocGen.DocIO.DLS;

internal class ListOverrideStyle : Style
{
	private ListOverrideLevelCollection m_overrideLevels;

	internal int m_res1;

	internal int m_res2;

	internal string listStyleName;

	internal int m_unused1;

	internal int m_unused2;

	private long m_listID = 1720085641L;

	public override StyleType StyleType => StyleType.OtherStyle;

	internal ListOverrideLevelCollection OverrideLevels => m_overrideLevels;

	internal long ListID
	{
		get
		{
			return m_listID;
		}
		set
		{
			m_listID = value;
		}
	}

	internal ListOverrideStyle(WordDocument doc)
		: base(doc)
	{
		m_overrideLevels = new ListOverrideLevelCollection(doc);
		m_overrideLevels.SetOwner(this);
	}

	public override IStyle Clone()
	{
		return (IStyle)CloneImpl();
	}

	protected override object CloneImpl()
	{
		ListOverrideStyle listOverrideStyle = (ListOverrideStyle)base.CloneImpl();
		listOverrideStyle.m_overrideLevels = new ListOverrideLevelCollection(base.Document);
		listOverrideStyle.m_overrideLevels.SetOwner(listOverrideStyle);
		m_overrideLevels.CloneToImpl(listOverrideStyle.m_overrideLevels);
		return listOverrideStyle;
	}

	internal override void CloneRelationsTo(WordDocument doc, OwnerHolder nextOwner)
	{
		if (doc == base.Document)
		{
			return;
		}
		SetOwner(doc);
		OverrideLevels.SetOwner(this);
		foreach (OverrideLevelFormat overrideLevel in OverrideLevels)
		{
			overrideLevel.SetOwner(this);
			overrideLevel.OverrideListLevel.SetOwner(overrideLevel);
			overrideLevel.OverrideListLevel.CharacterFormat.SetOwner(overrideLevel.OverrideListLevel);
			overrideLevel.OverrideListLevel.ParagraphFormat.SetOwner(overrideLevel.OverrideListLevel);
			if (overrideLevel.OverrideListLevel.PicBullet != null)
			{
				overrideLevel.OverrideListLevel.PicBullet.CloneRelationsTo(base.Document, overrideLevel.OverrideListLevel);
			}
		}
	}

	internal override void Close()
	{
		base.Close();
		if (m_overrideLevels == null || m_overrideLevels.Count == 0)
		{
			m_overrideLevels = null;
			return;
		}
		_ = m_overrideLevels.Count;
		foreach (KeyValuePair<short, short> item in m_overrideLevels.LevelIndex)
		{
			m_overrideLevels[item.Key].Close();
		}
		m_overrideLevels.Close();
		m_overrideLevels = null;
	}

	protected override void InitXDLSHolder()
	{
		base.InitXDLSHolder();
		base.XDLSHolder.AddElement("override-levels", m_overrideLevels);
	}

	protected override void WriteXmlAttributes(IXDLSAttributeWriter writer)
	{
		base.WriteXmlAttributes(writer);
		if (m_res1 != 0)
		{
			writer.WriteValue("Res1", m_res1);
		}
		if (m_res2 != 0)
		{
			writer.WriteValue("Res2", m_res2);
		}
		if (m_unused1 != 0)
		{
			writer.WriteValue("Unused1", m_unused1);
		}
		if (m_unused2 != 0)
		{
			writer.WriteValue("Unused2", m_unused2);
		}
	}

	protected override void ReadXmlAttributes(IXDLSAttributeReader reader)
	{
		base.ReadXmlAttributes(reader);
		if (reader.HasAttribute("Res1"))
		{
			m_res1 = reader.ReadInt("Res1");
		}
		if (reader.HasAttribute("Res2"))
		{
			m_res2 = reader.ReadInt("Res2");
		}
		if (reader.HasAttribute("Unused1"))
		{
			m_unused1 = reader.ReadInt("Unused1");
		}
		if (reader.HasAttribute("Unused2"))
		{
			m_unused2 = reader.ReadInt("Unused2");
		}
	}

	internal bool Compare(ListOverrideStyle listOverrideStyle)
	{
		if (ListID != listOverrideStyle.ListID)
		{
			return false;
		}
		if (base.IsCustom != listOverrideStyle.IsCustom)
		{
			return false;
		}
		if (base.IsPrimaryStyle != listOverrideStyle.IsPrimaryStyle)
		{
			return false;
		}
		if (base.IsSemiHidden != listOverrideStyle.IsSemiHidden)
		{
			return false;
		}
		if (base.LinkedStyleName != listOverrideStyle.LinkedStyleName)
		{
			return false;
		}
		if (base.UnhideWhenUsed != listOverrideStyle.UnhideWhenUsed)
		{
			return false;
		}
		if (OverrideLevels != null && listOverrideStyle.OverrideLevels != null)
		{
			if (!OverrideLevels.Compare(listOverrideStyle.OverrideLevels))
			{
				return false;
			}
		}
		else if ((OverrideLevels != null && listOverrideStyle.OverrideLevels == null) || (OverrideLevels == null && listOverrideStyle.OverrideLevels != null))
		{
			return false;
		}
		if (!IsEquivalentListStyle(listOverrideStyle.listStyleName, listStyleName, listOverrideStyle.Document))
		{
			return false;
		}
		return true;
	}

	private bool IsEquivalentListStyle(string sourceListStyleName, string destListStyleName, WordDocument doc)
	{
		ListStyle listStyle = null;
		if (doc != null)
		{
			listStyle = doc.ListStyles.FindByName(sourceListStyleName);
		}
		ListStyle listStyle2 = null;
		if (base.Document != null)
		{
			listStyle2 = base.Document.ListStyles.FindByName(destListStyleName);
		}
		if ((listStyle == null && listStyle2 == null) || (listStyle != null && listStyle2 != null && listStyle.Compare(listStyle2)))
		{
			return true;
		}
		return false;
	}
}
