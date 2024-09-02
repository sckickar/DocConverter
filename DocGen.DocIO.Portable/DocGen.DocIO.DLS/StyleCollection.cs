using System;
using System.Collections;
using System.Collections.Generic;
using DocGen.DocIO.DLS.XML;

namespace DocGen.DocIO.DLS;

public class StyleCollection : CollectionImpl, IEnumerable, IStyleCollection, ICollectionBase
{
	internal string m_FixedIndex13StyleName = string.Empty;

	internal string m_FixedIndex14StyleName = string.Empty;

	private byte m_bFlags;

	public IStyle this[int index] => (IStyle)base.InnerList[index];

	public bool FixedIndex13HasStyle
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

	public bool FixedIndex14HasStyle
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

	public string FixedIndex13StyleName
	{
		get
		{
			return m_FixedIndex13StyleName;
		}
		set
		{
			m_FixedIndex13StyleName = value;
		}
	}

	public string FixedIndex14StyleName
	{
		get
		{
			return m_FixedIndex14StyleName;
		}
		set
		{
			m_FixedIndex14StyleName = value;
		}
	}

	internal StyleCollection(WordDocument doc)
		: base(doc, doc)
	{
	}

	public int Add(IStyle style)
	{
		if (style == null)
		{
			throw new ArgumentNullException("style");
		}
		XDLSSerializableBase obj = (XDLSSerializableBase)style;
		obj.CloneRelationsTo(base.Document, null);
		obj.SetOwner(base.Document);
		if (string.IsNullOrEmpty(style.Name.Trim()))
		{
			UpdateAlternateStyleName(style);
		}
		return base.InnerList.Add(style);
	}

	public IStyle FindByName(string name)
	{
		return FindStyleByName(base.InnerList, name);
	}

	public IStyle FindByName(string name, StyleType styleType)
	{
		return FindStyleByName(base.InnerList, name, styleType);
	}

	internal IStyle FindByName(string name, StyleType styleType, ref List<string> styelNames, ref bool isDiffTypeStyleFound)
	{
		return FindStyleByName(base.InnerList, name, styleType, ref styelNames, ref isDiffTypeStyleFound);
	}

	public IStyle FindById(int styleId)
	{
		return FindStyleById(base.InnerList, styleId);
	}

	internal void CloneToImpl(CollectionImpl coll)
	{
		StyleCollection styleCollection = coll as StyleCollection;
		IStyle style = null;
		int i = 0;
		for (int count = base.InnerList.Count; i < count; i++)
		{
			style = base.InnerList[i] as IStyle;
			styleCollection.Add(style.Clone());
		}
	}

	internal void Remove(IStyle style)
	{
		base.InnerList.Remove(style);
	}

	private void UpdateAlternateStyleName(IStyle style)
	{
		int num = base.InnerList.Count;
		string text = "a" + num;
		while (IsSameNameExists(text))
		{
			num++;
			text = "a" + num;
		}
		style.Name = text;
		if (!string.IsNullOrEmpty((style as Style).StyleIDName) && base.Document.StyleNameIds.ContainsKey((style as Style).StyleIDName))
		{
			base.Document.StyleNameIds[(style as Style).StyleIDName] = style.Name;
		}
	}

	private bool IsSameNameExists(string styleName)
	{
		foreach (IStyle inner in base.InnerList)
		{
			if (inner.Name == styleName)
			{
				return true;
			}
		}
		return false;
	}

	internal static IStyle FindStyleByName(IList styles, string name)
	{
		IStyle result = null;
		for (int i = 0; i < styles.Count; i++)
		{
			if (styles[i] is IStyle style && style.Name == name)
			{
				result = style;
			}
		}
		return result;
	}

	internal static IStyle FindStyleByName(IList styles, string name, bool findFirstStyle)
	{
		IStyle result = null;
		for (int i = 0; i < styles.Count; i++)
		{
			if (styles[i] is IStyle style && style.Name == name)
			{
				result = style;
				break;
			}
		}
		return result;
	}

	internal IStyle FindFirstStyleByName(string name)
	{
		return FindStyleByName(base.InnerList, name, findFirstStyle: true);
	}

	internal Style FindStyleById(string name)
	{
		IList innerList = base.InnerList;
		Style result = null;
		for (int i = 0; i < innerList.Count; i++)
		{
			if (innerList[i] is Style style && style.StyleIDName == name)
			{
				result = style;
			}
		}
		return result;
	}

	internal static IStyle FindStyleByName(IList styles, string name, StyleType styleType)
	{
		if (name == null)
		{
			return null;
		}
		IStyle result = null;
		for (int i = 0; i < styles.Count; i++)
		{
			if (styles[i] is Style style && style.Name == name && style.StyleType == styleType)
			{
				result = style;
			}
		}
		return result;
	}

	internal IStyle FindStyleByName(IList styles, string name, StyleType styleType, ref List<string> styleNames, ref bool isDiffTypeStyleFound)
	{
		if (name == null)
		{
			return null;
		}
		IStyle result = null;
		for (int i = 0; i < styles.Count; i++)
		{
			if (!(styles[i] is Style style))
			{
				continue;
			}
			if (style.Name.StartsWith(name + "_"))
			{
				styleNames.Add(style.Name);
			}
			if (style.Name == name)
			{
				if (style.StyleType == styleType)
				{
					result = style;
				}
				else
				{
					isDiffTypeStyleFound = true;
				}
				styleNames.Add(style.Name);
			}
		}
		return result;
	}

	internal static IStyle FindStyleById(IList styles, int styleId)
	{
		IStyle result = null;
		for (int i = 0; i < styles.Count; i++)
		{
			if (styles[i] is Style style && style.StyleId == styleId)
			{
				result = style;
			}
		}
		return result;
	}
}
