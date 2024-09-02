using System;
using System.Collections;
using System.Collections.Generic;
using DocGen.OfficeChart.Parser.Biff_Records;

namespace DocGen.OfficeChart.Implementation.Collections;

internal class StylesCollection : CollectionBaseEx<IStyle>, IStyles, IEnumerable
{
	private Dictionary<StyleImpl, StyleImpl> m_map = new Dictionary<StyleImpl, StyleImpl>();

	private Dictionary<string, StyleImpl> m_dictStyles;

	private Dictionary<int, StyleImpl> m_hashIndexToStyle;

	private WorkbookImpl m_holder;

	private EventHandler m_beforeChange;

	private EventHandler m_afterChange;

	public IStyle this[string name]
	{
		get
		{
			if (!m_dictStyles.TryGetValue(name, out var value))
			{
				throw new ArgumentException("Style with specified name does not exist. Name: " + name, "value");
			}
			return value;
		}
	}

	public Dictionary<StyleImpl, StyleImpl> Map => m_map;

	public StylesCollection(IApplication application, object parent)
		: base(application, parent)
	{
		m_dictStyles = new Dictionary<string, StyleImpl>();
		object obj = FindParent(typeof(WorkbookImpl));
		if (obj == null)
		{
			throw new ArgumentException("Style collection must be in Workbook object tree.");
		}
		m_holder = (WorkbookImpl)obj;
		m_beforeChange = OnStyleBeforeChange;
		m_afterChange = OnStyleAfterChange;
	}

	public IStyle Add(string name, object BasedOn)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (m_dictStyles.ContainsKey(name))
		{
			throw new ArgumentException("Name of style must be unique.");
		}
		IStyle style = null;
		if (BasedOn == null)
		{
			style = base.AppImplementation.CreateStyle(m_holder, name);
		}
		else if (BasedOn is string)
		{
			style = base.AppImplementation.CreateStyle(m_holder, name, (StyleImpl)this[(string)BasedOn]);
		}
		else if (BasedOn is StyleImpl)
		{
			style = base.AppImplementation.CreateStyle(m_holder, name, (StyleImpl)BasedOn);
		}
		if (style != null)
		{
			base.Add(style);
		}
		return style;
	}

	public IStyle Add(string name)
	{
		return Add(name, null);
	}

	public IStyles Merge(object Workbook)
	{
		return Merge(Workbook, overwrite: false);
	}

	public IStyles Merge(object Workbook, bool overwrite)
	{
		if (Workbook == null)
		{
			throw new ArgumentNullException("Workbook");
		}
		if (!(Workbook is WorkbookImpl))
		{
			throw new ArgumentException("Wrong argument type", "Workbook");
		}
		WorkbookImpl workbook = (WorkbookImpl)Workbook;
		Merge(workbook, overwrite ? ExcelStyleMergeOptions.Replace : ExcelStyleMergeOptions.Leave);
		return this;
	}

	public void Remove(string styleName)
	{
		if (styleName != null && styleName.Length != 0)
		{
			m_dictStyles.TryGetValue(styleName, out var value);
			if (value != null && !value.BuiltIn)
			{
				int xFormatIndex = value.XFormatIndex;
				m_map.Remove(value);
				Remove(value);
				m_dictStyles.Remove(styleName);
				value.Workbook.RemoveExtenededFormatIndex(xFormatIndex);
			}
		}
	}

	public new void Add(IStyle style)
	{
		string name = style.Name;
		if (ContainsName(name))
		{
			StyleImpl styleImpl = (StyleImpl)style;
			m_dictStyles.TryGetValue(name, out var value);
			if (styleImpl.Index == value.Index && styleImpl.BuiltIn == value.BuiltIn)
			{
				throw new ArgumentException($"Collection already contains style with same names {style.Name}.");
			}
			if (styleImpl.BuiltIn)
			{
				m_dictStyles[name] = styleImpl;
			}
			else if (!value.BuiltIn && !m_holder.IsWorkbookOpening)
			{
				throw new ArgumentException($"Collection already contains style with same names {style.Name}.");
			}
		}
		base.Add(style);
	}

	public void Add(IStyle style, bool bReplace)
	{
		if (ContainsName(style.Name))
		{
			if (!bReplace)
			{
				return;
			}
			int i = 0;
			for (int count = base.List.Count; i < count; i++)
			{
				if (base.List[i].Name == style.Name)
				{
					base.List[i] = style;
					break;
				}
			}
		}
		else
		{
			base.Add(style);
		}
	}

	public bool Contains(string name)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (name.Length == 0)
		{
			throw new ArgumentException("name - string cannot be empty.");
		}
		return ContainsName(name);
	}

	public IStyle ContainsSameStyle(IStyle style)
	{
		if (style == null)
		{
			throw new ArgumentNullException("style");
		}
		StyleImpl styleImpl = null;
		styleImpl = style as StyleImpl;
		styleImpl.NotCompareNames = true;
		style = m_map[styleImpl];
		styleImpl.NotCompareNames = false;
		return style;
	}

	public static bool CompareStyles(IStyle source, IStyle destination)
	{
		if (source.Color == destination.Color && source.PatternColor == destination.PatternColor && source.FillPattern == destination.FillPattern && source.NumberFormat == destination.NumberFormat && source.FormulaHidden == destination.FormulaHidden && source.HorizontalAlignment == destination.HorizontalAlignment && source.VerticalAlignment == destination.VerticalAlignment && source.WrapText == destination.WrapText && source.Font.Equals(destination.Font) && source.IncludeAlignment == destination.IncludeAlignment && source.IncludeBorder == destination.IncludeBorder && source.IncludeFont == destination.IncludeFont && source.IncludeNumberFormat == destination.IncludeNumberFormat && source.IncludePatterns == destination.IncludePatterns && source.IncludeProtection == destination.IncludeProtection && source.IndentLevel == destination.IndentLevel && CompareBorders(source.Borders, destination.Borders) && source.Locked == destination.Locked)
		{
			return source.ShrinkToFit == destination.ShrinkToFit;
		}
		return false;
	}

	public static bool CompareBorders(IBorders source, IBorders destination)
	{
		if (CompareBorder(source[OfficeBordersIndex.EdgeBottom], destination[OfficeBordersIndex.EdgeBottom]) && CompareBorder(source[OfficeBordersIndex.EdgeLeft], destination[OfficeBordersIndex.EdgeLeft]) && CompareBorder(source[OfficeBordersIndex.EdgeRight], destination[OfficeBordersIndex.EdgeRight]) && CompareBorder(source[OfficeBordersIndex.EdgeTop], destination[OfficeBordersIndex.EdgeTop]) && CompareBorder(source[OfficeBordersIndex.DiagonalDown], destination[OfficeBordersIndex.DiagonalDown]))
		{
			return CompareBorder(source[OfficeBordersIndex.DiagonalUp], destination[OfficeBordersIndex.DiagonalUp]);
		}
		return false;
	}

	public static bool CompareBorder(IBorder source, IBorder destination)
	{
		if (source.ColorObject == destination.ColorObject && source.LineStyle == destination.LineStyle)
		{
			return source.ShowDiagonalLine == destination.ShowDiagonalLine;
		}
		return false;
	}

	public Dictionary<string, string> Merge(IWorkbook workbook, ExcelStyleMergeOptions option)
	{
		Dictionary<int, int> dicFontIndexes;
		Dictionary<int, int> hashExtFormatIndexes;
		return Merge(workbook, option, out dicFontIndexes, out hashExtFormatIndexes);
	}

	public Dictionary<string, string> Merge(IWorkbook workbook, ExcelStyleMergeOptions option, out Dictionary<int, int> dicFontIndexes, out Dictionary<int, int> hashExtFormatIndexes)
	{
		if (!(workbook is WorkbookImpl))
		{
			throw new ArgumentException("Wrong argument type", "Workbook");
		}
		WorkbookImpl workbookImpl = (WorkbookImpl)workbook;
		hashExtFormatIndexes = null;
		dicFontIndexes = null;
		if (workbookImpl == m_holder)
		{
			return null;
		}
		StylesCollection innerStyles = workbookImpl.InnerStyles;
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		hashExtFormatIndexes = m_holder.InnerExtFormats.Merge(workbookImpl.InnerExtFormats, out dicFontIndexes);
		int i = 0;
		for (int count = innerStyles.Count; i < count; i++)
		{
			StyleImpl styleImpl = innerStyles[i] as StyleImpl;
			string text = styleImpl.Name;
			bool flag = m_dictStyles.ContainsKey(text);
			bool isBuiltInCustomized = styleImpl.IsBuiltInCustomized;
			bool builtIn = styleImpl.BuiltIn;
			if (!builtIn || (builtIn && isBuiltInCustomized))
			{
				switch (option)
				{
				case ExcelStyleMergeOptions.CreateDiffName:
					text = CollectionBaseEx<IStyle>.GenerateDefaultName(this, text + "_");
					dictionary.Add(text, text);
					break;
				case ExcelStyleMergeOptions.Leave:
					text = null;
					break;
				default:
					throw new ArgumentOutOfRangeException("option");
				case ExcelStyleMergeOptions.Replace:
					break;
				}
				if (text != null)
				{
					StyleRecord styleRecord = (StyleRecord)((ICloneable)styleImpl.Record).Clone();
					int extendedFormatIndex = styleRecord.ExtendedFormatIndex;
					styleRecord.ExtendedFormatIndex = (ushort)hashExtFormatIndexes[extendedFormatIndex];
					styleRecord.StyleName = text;
					Add(styleRecord);
				}
			}
			else if (builtIn && !flag)
			{
				dictionary.Add(text, text);
				if (text != null)
				{
					StyleRecord styleRecord2 = (StyleRecord)((ICloneable)styleImpl.Record).Clone();
					int extendedFormatIndex2 = styleRecord2.ExtendedFormatIndex;
					styleRecord2.ExtendedFormatIndex = (ushort)hashExtFormatIndexes[extendedFormatIndex2];
					styleRecord2.StyleName = text;
					Add(styleRecord2);
				}
			}
		}
		return dictionary;
	}

	public string GenerateDefaultName(string strStart)
	{
		return CollectionBaseEx<IStyle>.GenerateDefaultName(strStart, m_dictStyles.Values);
	}

	public string GenerateDefaultName(string strStart, Dictionary<string, StyleRecord> hashNamesInFile)
	{
		return CollectionBaseEx<IStyle>.GenerateDefaultName(strStart, m_dictStyles.Values, hashNamesInFile.Keys);
	}

	public StyleImpl CreateBuiltInStyle(string strName)
	{
		if (strName == null)
		{
			throw new ArgumentNullException("strName");
		}
		if (strName.Length == 0)
		{
			throw new ArgumentException("strName - string cannot be empty");
		}
		StyleImpl styleImpl = base.AppImplementation.CreateStyle(m_holder, strName, bIsBuildIn: true);
		base.Add(styleImpl);
		return styleImpl;
	}

	public StyleImpl GetByXFIndex(int index)
	{
		if (m_hashIndexToStyle != null && m_hashIndexToStyle.ContainsKey(index))
		{
			return m_hashIndexToStyle[index];
		}
		int i = 0;
		for (int count = base.Count; i < count; i++)
		{
			StyleImpl styleImpl = base[i] as StyleImpl;
			if (styleImpl.Index == index)
			{
				return styleImpl;
			}
		}
		return null;
	}

	public void UpdateStyleRecords()
	{
		List<IStyle> innerList = base.InnerList;
		int i = 0;
		for (int count = innerList.Count; i < count; i++)
		{
			((StyleImpl)innerList[i]).UpdateStyleRecord();
		}
	}

	internal IStyle Find(string styleName)
	{
		return this[styleName];
	}

	internal bool ContainsName(string styleName)
	{
		return m_dictStyles.ContainsKey(styleName);
	}

	[CLSCompliant(false)]
	public StyleImpl Add(StyleRecord style)
	{
		if (style == null)
		{
			throw new ArgumentNullException("style");
		}
		StyleImpl styleImpl = base.AppImplementation.CreateStyle(m_holder, style);
		Add(styleImpl);
		return styleImpl;
	}

	public override object Clone(object parent)
	{
		if (parent == null)
		{
			throw new ArgumentNullException("parent");
		}
		StylesCollection stylesCollection = new StylesCollection(base.Application, parent);
		List<IStyle> innerList = base.InnerList;
		int i = 0;
		for (int count = innerList.Count; i < count; i++)
		{
			StyleImpl styleImpl = (StyleImpl)innerList[i];
			styleImpl = (StyleImpl)styleImpl.Clone(stylesCollection);
			stylesCollection.Add(styleImpl);
		}
		return stylesCollection;
	}

	protected override void OnClearComplete()
	{
		m_dictStyles.Clear();
		m_map.Clear();
		if (m_hashIndexToStyle != null)
		{
			m_hashIndexToStyle.Clear();
		}
		base.OnClearComplete();
	}

	protected override void OnInsertComplete(int index, IStyle value)
	{
		string name = value.Name;
		if (!m_dictStyles.ContainsKey(name))
		{
			m_dictStyles[value.Name] = (StyleImpl)value;
		}
		StyleImpl styleImpl = (StyleImpl)value;
		if (m_hashIndexToStyle != null)
		{
			m_hashIndexToStyle[styleImpl.Index] = styleImpl;
		}
		m_map.Add(styleImpl, styleImpl);
		styleImpl.BeforeChange += m_beforeChange;
		styleImpl.AfterChange += m_afterChange;
		base.OnInsertComplete(index, value);
	}

	protected override void OnRemoveComplete(int index, IStyle value)
	{
		StyleImpl styleImpl = (StyleImpl)value;
		m_dictStyles.Remove(styleImpl.Name);
		if (m_hashIndexToStyle != null)
		{
			m_hashIndexToStyle.Remove(styleImpl.Index);
		}
		m_map.Remove(styleImpl);
		styleImpl.BeforeChange -= m_beforeChange;
		styleImpl.AfterChange -= m_afterChange;
		base.OnRemoveComplete(index, value);
	}

	protected override void OnSetComplete(int index, IStyle oldValue, IStyle newValue)
	{
		StyleImpl styleImpl = (StyleImpl)oldValue;
		StyleImpl styleImpl2 = (StyleImpl)newValue;
		if (m_hashIndexToStyle != null)
		{
			m_hashIndexToStyle.Remove(styleImpl.Index);
			m_hashIndexToStyle[styleImpl2.Index] = styleImpl2;
		}
		m_dictStyles.Remove(styleImpl.Name);
		m_map.Remove(styleImpl);
		m_map.Add(styleImpl2, styleImpl2);
		if (m_dictStyles.ContainsKey(styleImpl2.Name))
		{
			throw new ArgumentException("Collection cannot contain two styles with same name.");
		}
		m_dictStyles[styleImpl2.Name] = styleImpl2;
		base.OnSetComplete(index, oldValue, newValue);
	}

	private void OnStyleBeforeChange(object sender, EventArgs args)
	{
		if (sender == null)
		{
			throw new ArgumentNullException("sender");
		}
		if (args == null)
		{
			throw new ArgumentNullException("args");
		}
		StyleImpl styleImpl = (StyleImpl)sender;
		m_map.Remove(styleImpl);
		styleImpl.BeforeChange -= m_beforeChange;
	}

	private void OnStyleAfterChange(object sender, EventArgs args)
	{
		if (sender == null)
		{
			throw new ArgumentNullException("sender");
		}
		if (args == null)
		{
			throw new ArgumentNullException("args");
		}
		StyleImpl styleImpl = (StyleImpl)sender;
		m_map.Add(styleImpl, styleImpl);
		styleImpl.BeforeChange += m_beforeChange;
	}

	internal void ClearStylesHash()
	{
		m_hashIndexToStyle = null;
		m_map = null;
		m_dictStyles = null;
	}

	internal void Dispose()
	{
		foreach (StyleImpl inner in base.InnerList)
		{
			inner.Dispose();
		}
	}
}
