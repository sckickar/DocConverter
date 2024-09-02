using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using DocGen.DocIO.DLS.XML;
using DocGen.DocIO.Rendering;
using DocGen.Drawing;
using DocGen.Layouting;

namespace DocGen.DocIO.DLS;

public class TableOfContent : ParagraphItem, ILayoutInfo
{
	private const int DEF_UPPER_HEADING_LEVEL = 3;

	private const int DEF_LOWER_HEADING_LEVEL = 1;

	private const char DEF_HEADING_LEVELS_SWITCH = 'o';

	private const char DEF_HYPERLINK_SWITCH = 'h';

	private const char DEF_PAGE_NUMBERS_SWITCH = 'n';

	private const char DEF_SEPARATOR_SWITCH = 'p';

	private const char DEF_USE_OUTLINE_SWITCH = 'u';

	private const char DEF_USE_FIELDS_SWITCH = 'f';

	private const char DEF_STYLES_SWITCH = 't';

	private const char DEF_INCLUDE_NEWLINE_CHARACTERS_SWITCH = 'x';

	private const char DEF_TABLE_OF_FIGURES_SWITCH = 'c';

	private const char DEF_INCLUDE_CAPTIONLABEL_AND_NUMBERS = 'a';

	private WField m_tocField;

	private int m_upperHeadingLevel = 3;

	private int m_lowerHeadingLevel = 1;

	private string m_tableID;

	private Dictionary<int, List<WParagraphStyle>> m_tocStyles;

	private Dictionary<Entity, Entity> m_tocEntryEntities;

	private string m_lstSepar = CultureInfo.CurrentCulture.TextInfo.ListSeparator;

	private Dictionary<int, List<string>> m_tocLevels;

	private WParagraph m_tocParagraph;

	internal Entity m_tocEntryLastEntity;

	internal WFieldMark m_captionEndEntity;

	private string m_tableOfFiguresLabel;

	private byte m_bFlags = 45;

	private byte m_bFlags1 = 45;

	internal TabLeader m_tabLeader = TabLeader.Dotted;

	internal bool m_docTocEntryLastEntityReached;

	public string TableOfFiguresLabel
	{
		get
		{
			OnGetValue();
			return m_tableOfFiguresLabel;
		}
		set
		{
			OnChange();
			m_tableOfFiguresLabel = value;
			if (!base.Document.IsOpening)
			{
				UpdateTOCFieldCode();
			}
		}
	}

	public bool IncludeCaptionLabelsAndNumbers
	{
		get
		{
			OnGetValue();
			return (m_bFlags1 & 0x20) >> 5 != 0;
		}
		set
		{
			OnChange();
			m_bFlags1 = (byte)((m_bFlags1 & 0xDFu) | ((value ? 1u : 0u) << 5));
			if (!base.Document.IsOpening)
			{
				UpdateTOCFieldCode();
			}
		}
	}

	public bool UseHeadingStyles
	{
		get
		{
			OnGetValue();
			return (m_bFlags & 1) != 0;
		}
		set
		{
			OnChange();
			m_bFlags = (byte)((m_bFlags & 0xFEu) | (value ? 1u : 0u));
			if (!base.Document.IsOpening)
			{
				UpdateTOCFieldCode();
			}
		}
	}

	public int UpperHeadingLevel
	{
		get
		{
			OnGetValue();
			return m_upperHeadingLevel;
		}
		set
		{
			CheckLevelNumber("UpperHeadingLevel", value);
			OnChange();
			m_upperHeadingLevel = value;
			if (!base.Document.IsOpening)
			{
				UpdateTOCFieldCode();
			}
		}
	}

	public int LowerHeadingLevel
	{
		get
		{
			OnGetValue();
			return m_lowerHeadingLevel;
		}
		set
		{
			CheckLevelNumber("LowerHeadingLevel", value);
			OnChange();
			m_lowerHeadingLevel = value;
			if (!base.Document.IsOpening)
			{
				UpdateTOCFieldCode();
			}
		}
	}

	public bool UseTableEntryFields
	{
		get
		{
			return (m_bFlags & 2) >> 1 != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xFDu) | ((value ? 1u : 0u) << 1));
			if (!base.Document.IsOpening)
			{
				UpdateTOCFieldCode();
			}
		}
	}

	public string TableID
	{
		get
		{
			return m_tableID;
		}
		set
		{
			m_tableID = value;
			if (!base.Document.IsOpening)
			{
				UpdateTOCFieldCode();
			}
		}
	}

	public bool RightAlignPageNumbers
	{
		get
		{
			OnGetValue();
			return (m_bFlags & 4) >> 2 != 0;
		}
		set
		{
			OnChange();
			m_bFlags = (byte)((m_bFlags & 0xFBu) | ((value ? 1u : 0u) << 2));
			if (!base.Document.IsOpening)
			{
				UpdateTOCFieldCode();
			}
		}
	}

	public bool IncludePageNumbers
	{
		get
		{
			OnGetValue();
			return (m_bFlags & 0x20) >> 5 != 0;
		}
		set
		{
			OnChange();
			m_bFlags = (byte)((m_bFlags & 0xDFu) | ((value ? 1u : 0u) << 5));
			if (!base.Document.IsOpening)
			{
				UpdateTOCFieldCode();
			}
		}
	}

	public bool UseHyperlinks
	{
		get
		{
			OnGetValue();
			return (m_bFlags & 8) >> 3 != 0;
		}
		set
		{
			OnChange();
			m_bFlags = (byte)((m_bFlags & 0xF7u) | ((value ? 1u : 0u) << 3));
			if (!base.Document.IsOpening)
			{
				UpdateTOCFieldCode();
			}
		}
	}

	public bool UseOutlineLevels
	{
		get
		{
			OnGetValue();
			return (m_bFlags & 0x10) >> 4 != 0;
		}
		set
		{
			OnChange();
			m_bFlags = (byte)((m_bFlags & 0xEFu) | ((value ? 1u : 0u) << 4));
			if (!base.Document.IsOpening)
			{
				UpdateTOCFieldCode();
			}
		}
	}

	public override EntityType EntityType => EntityType.TOC;

	internal string FormattingString
	{
		get
		{
			return m_tocField.m_formattingString;
		}
		set
		{
			m_tocField.m_formattingString = value;
		}
	}

	internal WField TOCField => m_tocField;

	internal Dictionary<int, List<WParagraphStyle>> TOCStyles
	{
		get
		{
			if (m_tocStyles == null)
			{
				m_tocStyles = new Dictionary<int, List<WParagraphStyle>>();
			}
			return m_tocStyles;
		}
	}

	internal Dictionary<Entity, Entity> TOCEntryEntities => m_tocEntryEntities ?? (m_tocEntryEntities = new Dictionary<Entity, Entity>());

	internal Dictionary<int, List<string>> TOCLevels
	{
		get
		{
			if (m_tocLevels == null)
			{
				m_tocLevels = new Dictionary<int, List<string>>();
			}
			return m_tocLevels;
		}
	}

	private WParagraph LastTOCParagraph
	{
		get
		{
			if (m_tocParagraph == null)
			{
				m_tocParagraph = base.OwnerParagraph;
			}
			return m_tocParagraph;
		}
	}

	private bool InvalidFormatString
	{
		get
		{
			return (m_bFlags & 0x40) >> 6 != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xBFu) | ((value ? 1u : 0u) << 6));
		}
	}

	private bool FormattingParsed
	{
		get
		{
			return (m_bFlags & 0x80) >> 1 != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0x7Fu) | ((value ? 1u : 0u) << 7));
		}
	}

	public bool IncludeNewLineCharacters
	{
		get
		{
			OnGetValue();
			return (m_bFlags1 & 2) >> 1 != 0;
		}
		set
		{
			OnChange();
			m_bFlags1 = (byte)((m_bFlags1 & 0xFDu) | ((value ? 1u : 0u) << 1));
			if (!base.Document.IsOpening)
			{
				UpdateTOCFieldCode();
			}
		}
	}

	SizeF ILayoutInfo.Size
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	SyncFont ILayoutInfo.Font
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	bool ILayoutInfo.IsClipped
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	bool ILayoutInfo.IsVerticalText
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	bool ILayoutInfo.IsSkip
	{
		get
		{
			return true;
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	bool ILayoutInfo.IsSkipBottomAlign
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	bool ILayoutInfo.IsLineContainer
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	ChildrenLayoutDirection ILayoutInfo.ChildrenLayoutDirection
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	bool ILayoutInfo.IsLineBreak
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	bool ILayoutInfo.TextWrap
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	bool ILayoutInfo.IsPageBreakItem
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	bool ILayoutInfo.IsFirstItemInPage
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	bool ILayoutInfo.IsKeepWithNext
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	bool ILayoutInfo.IsHiddenRow
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public TableOfContent(IWordDocument doc)
		: base(doc as WordDocument)
	{
		m_tocField = new WField(doc);
		m_tocField.FieldType = FieldType.FieldTOC;
		m_tableID = string.Empty;
	}

	public TableOfContent(IWordDocument doc, string switches)
		: this(doc)
	{
		TOCField.m_formattingString = switches;
		m_bFlags &= 247;
		ParseSwitches();
	}

	public void SetTOCLevelStyle(int levelNumber, string styleName)
	{
		CheckLevelNumber("levelNumber", levelNumber);
		SetStyleForTOCLevel(levelNumber, styleName, onSetProperty: true);
		if (!base.Document.IsOpening)
		{
			UpdateTOCFieldCode();
		}
	}

	public string GetTOCLevelStyle(int levelNumber)
	{
		return GetTOCLevelStyles(levelNumber)[0];
	}

	public List<string> GetTOCLevelStyles(int levelNumber)
	{
		if (levelNumber < m_lowerHeadingLevel || levelNumber > m_upperHeadingLevel)
		{
			throw new ArgumentException("Level index must be >= LowerHeadingLevel and <= UpperHeadingLevel");
		}
		ParseSwitches();
		List<string> list = new List<string>();
		if (m_tocStyles.ContainsKey(levelNumber))
		{
			foreach (WParagraphStyle item in m_tocStyles[levelNumber])
			{
				list.Add(item.Name);
			}
		}
		else
		{
			WParagraphStyle wParagraphStyle = GetBuiltinStyle((BuiltinStyle)levelNumber) as WParagraphStyle;
			list.Add(wParagraphStyle.Name);
		}
		return list;
	}

	internal WField FindKey()
	{
		foreach (KeyValuePair<WField, TableOfContent> item in base.Document.TOC)
		{
			if (item.Value == this)
			{
				return item.Key;
			}
		}
		return null;
	}

	private void ParseSwitches()
	{
		if (FormattingParsed)
		{
			return;
		}
		string text = TOCField.m_formattingString;
		if (text.Contains("\\* MERGEFORMAT"))
		{
			text = text.Remove(text.IndexOf("\\* MERGEFORMAT")).Trim();
		}
		else if (text.Contains("\\* Mergeformat"))
		{
			text = text.Remove(text.IndexOf("\\* Mergeformat")).Trim();
		}
		string[] array = text.Split('\\');
		bool flag = false;
		int i = 0;
		for (int num = array.Length; i < num; i++)
		{
			string text2 = array[i];
			if (text2.Length != 0)
			{
				switch (text2[0])
				{
				case 'o':
					m_bFlags = (byte)((m_bFlags & 0xFEu) | 1u);
					flag = true;
					ParseHeadingLevels(text2);
					break;
				case 'h':
					m_bFlags = (byte)((m_bFlags & 0xF7u) | 8u);
					break;
				case 'n':
					m_bFlags &= 223;
					break;
				case 'p':
					m_bFlags &= 251;
					break;
				case 'u':
					m_bFlags = (byte)((m_bFlags & 0xEFu) | 0x10u);
					break;
				case 'f':
					ParseUseField(text2);
					break;
				case 't':
					ParseHeaderStylesAndUpdateLevel(text2, flag);
					break;
				case 'x':
					m_bFlags1 = (byte)((m_bFlags1 & 0xFDu) | 2u);
					break;
				case 'c':
					ParseLabelName(text2);
					break;
				case 'a':
					m_bFlags1 &= 223;
					ParseLabelName(text2);
					break;
				}
			}
		}
		if (TOCStyles.Count == 0 && !flag)
		{
			m_upperHeadingLevel = 9;
		}
		if (!flag && m_tableOfFiguresLabel != null)
		{
			m_bFlags &= 254;
		}
		FormattingParsed = true;
	}

	private IWParagraphStyle GetBuiltinStyle(BuiltinStyle builtinStyle)
	{
		string name = Style.BuiltInToName(builtinStyle);
		IWParagraphStyle iWParagraphStyle = m_doc.Styles.FindByName(name, StyleType.ParagraphStyle) as IWParagraphStyle;
		if (iWParagraphStyle == null)
		{
			iWParagraphStyle = (IWParagraphStyle)Style.CreateBuiltinStyle(builtinStyle, m_doc);
			m_doc.Styles.Add(iWParagraphStyle);
		}
		return iWParagraphStyle;
	}

	private void CreateDefStylesColl()
	{
		for (int i = 1; i <= 9; i++)
		{
			List<WParagraphStyle> list = new List<WParagraphStyle>();
			BuiltinStyle builtinStyle = (BuiltinStyle)i;
			list.Add(GetBuiltinStyle(builtinStyle) as WParagraphStyle);
			TOCStyles.Add(i, list);
		}
	}

	private void UpdateFormattingString()
	{
		if (InvalidFormatString)
		{
			TOCField.m_formattingString = string.Empty;
			if (((uint)m_bFlags & (true ? 1u : 0u)) != 0)
			{
				UpdateTOCLevels();
			}
			UpdateHeaderStyles();
			UpdateUsePageNumbers();
			UpdatePageNumberAlign();
			UpdateUseField();
			UpdateHyperlinks();
			UpdateNewLineCharacters();
			UpdateUseOutlineLevels();
			UpdateTableOfFigureLabel();
			UpdateCaptionLabelAndNumbers();
			FormattingParsed = true;
		}
	}

	internal void UpdateTOCFieldCode()
	{
		UpdateFormattingString();
		if (base.NextSibling is WTextRange && (base.NextSibling as WTextRange).NextSibling is WFieldMark)
		{
			(base.NextSibling as WTextRange).Text = "TOC " + m_tocField.FormattingString;
			return;
		}
		RemovePreviousFieldCodeItems();
		WTextRange wTextRange = new WTextRange(base.Document);
		wTextRange.Text = "TOC " + m_tocField.FormattingString;
		base.OwnerParagraph.Items.Insert(Index + 1, wTextRange);
	}

	private void RemovePreviousFieldCodeItems()
	{
		int num = Index + 1;
		while (num < base.OwnerParagraph.Items.Count && !(base.OwnerParagraph.Items[num] is WFieldMark))
		{
			base.OwnerParagraph.Items.RemoveAt(num);
			num--;
			num++;
		}
	}

	private void UpdateTOCLevels()
	{
		string text = $"\\o \"{m_lowerHeadingLevel}-{m_upperHeadingLevel}\" ";
		TOCField.m_formattingString += text;
	}

	private void UpdateHyperlinks()
	{
		if ((m_bFlags & 8) >> 3 != 0)
		{
			TOCField.m_formattingString += "\\h \\z ";
		}
	}

	private void UpdateNewLineCharacters()
	{
		if (IncludeNewLineCharacters)
		{
			TOCField.m_formattingString += "\\x ";
		}
	}

	private void UpdateUsePageNumbers()
	{
		if (!IncludePageNumbers)
		{
			TOCField.m_formattingString += "\\n ";
		}
	}

	private void UpdatePageNumberAlign()
	{
		if ((m_bFlags & 4) >> 2 == 0)
		{
			TOCField.m_formattingString += "\\p \" \" ";
		}
	}

	private void UpdateUseOutlineLevels()
	{
		if ((m_bFlags & 0x10) >> 4 != 0)
		{
			TOCField.m_formattingString += "\\u ";
		}
	}

	private void UpdateUseField()
	{
		if (UseTableEntryFields)
		{
			WField tOCField = TOCField;
			tOCField.m_formattingString = tOCField.m_formattingString + "\\f " + m_tableID;
		}
	}

	private void UpdateHeaderStyles()
	{
		if (m_tocStyles == null || m_tocStyles.Count == 0)
		{
			return;
		}
		TOCField.m_formattingString += "\\t \"";
		for (int i = m_lowerHeadingLevel; i <= m_upperHeadingLevel; i++)
		{
			if (!TOCStyles.ContainsKey(i))
			{
				continue;
			}
			foreach (WParagraphStyle item in TOCStyles[i])
			{
				if (Style.BuiltinStyleLoader.BuiltinStyleNames[i] != ((IStyle)item).Name)
				{
					WField tOCField = TOCField;
					tOCField.m_formattingString = tOCField.m_formattingString + ((IStyle)item).Name + m_lstSepar + i + m_lstSepar;
				}
			}
		}
		TOCField.m_formattingString += "\"";
	}

	private void UpdateTableOfFigureLabel()
	{
		if (!string.IsNullOrEmpty(m_tableOfFiguresLabel) && IncludeCaptionLabelsAndNumbers)
		{
			string text = $"\\c \"{m_tableOfFiguresLabel}\" ";
			TOCField.m_formattingString += text;
		}
	}

	private void UpdateCaptionLabelAndNumbers()
	{
		if (!string.IsNullOrEmpty(m_tableOfFiguresLabel) && !IncludeCaptionLabelsAndNumbers)
		{
			string text = $"\\a \"{m_tableOfFiguresLabel}\" ";
			TOCField.m_formattingString += text;
		}
	}

	private void ParseLabelName(string optionString)
	{
		string text = optionString.Substring(1).Trim();
		if (text.StartsWith("\"") && text.Substring(1).Contains("\""))
		{
			m_tableOfFiguresLabel = text.Substring(1, text.IndexOf('"', 1) - 1);
		}
		else if (!text.StartsWith("\"") && text.Length >= 1)
		{
			char[] separator = new char[2] { ' ', '"' };
			string[] array = text.Split(separator, StringSplitOptions.RemoveEmptyEntries);
			m_tableOfFiguresLabel = array[0];
		}
	}

	private void ParseHeadingLevels(string optionString)
	{
		MatchCollection matchCollection = new Regex("[0-9]").Matches(optionString);
		if (matchCollection.Count == 2)
		{
			m_lowerHeadingLevel = int.Parse(matchCollection[0].Groups[0].Value);
			m_upperHeadingLevel = int.Parse(matchCollection[1].Groups[0].Value);
		}
		else if (matchCollection.Count == 0)
		{
			m_upperHeadingLevel = 9;
		}
	}

	private void ParseNumberAlignment(string optionString)
	{
		if (new Regex("[\"][ ][\"]").Match(optionString).Captures.Count == 1)
		{
			m_bFlags &= 251;
		}
	}

	private void ParseUseField(string optionString)
	{
		UseTableEntryFields = true;
		m_tableID = optionString.Substring(1, optionString.Length - 1).Trim();
	}

	private void ParseHeaderStylesAndUpdateLevel(string optionString, bool isHeadingLevelDefined)
	{
		char separator = m_lstSepar.ToCharArray()[0];
		optionString = optionString.Replace('“', '"');
		optionString = optionString.Replace('”', '"');
		string[] array = optionString.Split('"')[1].Split(separator);
		bool flag = false;
		bool flag2 = false;
		int num = -1;
		List<string> list = new List<string>();
		for (int i = 0; i < array.Length; i++)
		{
			if (flag)
			{
				i++;
			}
			if (i >= array.Length)
			{
				break;
			}
			if (i != array.Length - 1)
			{
				if (!IsHeadingStyleContainsLevel(array[i + 1]))
				{
					flag2 = true;
				}
			}
			else
			{
				if (!IsHeadingStyleContainsLevel(array[i]))
				{
					flag2 = true;
				}
				if (string.IsNullOrEmpty(array[i]))
				{
					continue;
				}
			}
			if (!list.Contains(array[i]))
			{
				if (!IsHeadingStyleContainsLevel(array[i]))
				{
					list.Add(array[i]);
				}
				else
				{
					list.Add(array[i + 1]);
				}
				if (flag2)
				{
					num = ((i == 0 || i == array.Length - 1) ? 1 : ((!flag) ? list.Count : 9));
					flag = false;
					flag2 = false;
				}
				else
				{
					num = int.Parse(array[i + 1]);
					flag = true;
				}
				SetStyleForTOCLevel(num, array[i], onSetProperty: false);
			}
		}
		if (TOCStyles.Count > 0 && !isHeadingLevelDefined)
		{
			m_bFlags &= 254;
		}
		InvalidFormatString = false;
	}

	private bool IsHeadingStyleContainsLevel(string text)
	{
		int result = 0;
		return int.TryParse(text, out result);
	}

	private void OnChange()
	{
		ParseSwitches();
		InvalidFormatString = true;
	}

	private void OnGetValue()
	{
		ParseSwitches();
	}

	private void SetStyleForTOCLevel(int levelNumber, string styleName, bool onSetProperty)
	{
		if (onSetProperty)
		{
			OnChange();
		}
		BuiltinStyle builtinStyle = Style.NameToBuiltIn(styleName);
		IWParagraphStyle iWParagraphStyle = m_doc.Styles.FindByName(styleName, StyleType.ParagraphStyle) as IWParagraphStyle;
		IWParagraphStyle iWParagraphStyle2 = m_doc.Styles.FindByName(styleName.ToLower(), StyleType.ParagraphStyle) as IWParagraphStyle;
		if ((iWParagraphStyle != null || builtinStyle != BuiltinStyle.User) && iWParagraphStyle == null && iWParagraphStyle2 == null)
		{
			iWParagraphStyle = (IWParagraphStyle)Style.CreateBuiltinStyle(builtinStyle, m_doc);
			m_doc.Styles.Add(iWParagraphStyle);
		}
		IWParagraphStyle iWParagraphStyle3 = null;
		if (iWParagraphStyle != null)
		{
			iWParagraphStyle3 = iWParagraphStyle;
		}
		else if (iWParagraphStyle2 != null)
		{
			iWParagraphStyle3 = iWParagraphStyle2;
		}
		if (iWParagraphStyle3 == null)
		{
			return;
		}
		if (TOCStyles.ContainsKey(levelNumber))
		{
			List<WParagraphStyle> list = TOCStyles[levelNumber];
			if (!list.Contains(iWParagraphStyle3 as WParagraphStyle))
			{
				list.Add(iWParagraphStyle3 as WParagraphStyle);
			}
		}
		else
		{
			List<WParagraphStyle> list2 = new List<WParagraphStyle>();
			list2.Add(iWParagraphStyle3 as WParagraphStyle);
			TOCStyles.Add(levelNumber, list2);
		}
	}

	private void CheckLevelNumber(string parameterName, int levelNumber)
	{
		if (levelNumber < 1 || levelNumber > 9)
		{
			throw new ArgumentOutOfRangeException(parameterName, "Level number value must be greater than 1 and smaller than 10.");
		}
	}

	internal List<string> UpdateTOCStyleLevels()
	{
		List<string> list = new List<string>();
		List<string> list2 = new List<string>();
		if (TOCLevels.Count > 0)
		{
			TOCLevels.Clear();
		}
		if (TOCStyles.Count > 0)
		{
			foreach (KeyValuePair<int, List<WParagraphStyle>> tOCStyle in TOCStyles)
			{
				if (tOCStyle.Key < m_lowerHeadingLevel || tOCStyle.Key > m_upperHeadingLevel)
				{
					continue;
				}
				List<string> list3 = new List<string>();
				foreach (WParagraphStyle item2 in tOCStyle.Value)
				{
					if (!list2.Contains(item2.Name))
					{
						list3.Add(item2.Name);
						list2.Add(item2.Name);
					}
				}
				TOCLevels.Add(tOCStyle.Key, list3);
			}
		}
		if (UseHeadingStyles)
		{
			string text = "Heading ";
			for (int i = m_lowerHeadingLevel; i <= m_upperHeadingLevel; i++)
			{
				string item = text + i;
				if (!list2.Contains(item))
				{
					List<string> list4;
					if (TOCLevels.ContainsKey(i))
					{
						list4 = TOCLevels[i];
					}
					else
					{
						list4 = new List<string>();
						TOCLevels.Add(i, list4);
					}
					list4.Add(item);
					list2.Add(item);
				}
			}
			foreach (Style style3 in base.Document.Styles)
			{
				if (!(style3 is WParagraphStyle))
				{
					continue;
				}
				int num = (byte)(style3 as WParagraphStyle).ParagraphFormat.OutlineLevel + 1;
				string name = (style3 as WParagraphStyle).Name;
				if (num > 0 && num <= 9 && num >= m_lowerHeadingLevel && num <= m_upperHeadingLevel && !name.ToLower().StartsWith("heading") && !name.ToLower().StartsWith("normal") && !list2.Contains(name))
				{
					List<string> list5;
					if (TOCLevels.ContainsKey(num))
					{
						list5 = TOCLevels[num];
					}
					else
					{
						list5 = new List<string>();
						TOCLevels.Add(num, list5);
					}
					list5.Add(name);
					list2.Add(name);
				}
			}
		}
		foreach (Style style4 in base.Document.Styles)
		{
			if (style4 is WCharacterStyle && (style4 as WCharacterStyle).LinkedStyleName != null && list2.Contains((style4 as WCharacterStyle).LinkedStyleName) && !list.Contains((style4 as WCharacterStyle).Name))
			{
				list.Add((style4 as WCharacterStyle).Name);
			}
		}
		list2.Clear();
		return list;
	}

	internal List<ParagraphItem> ParseDocument(List<string> tocLinkCharacterStyleNames)
	{
		List<ParagraphItem> list = new List<ParagraphItem>();
		bool isParsingTOCParagraph = false;
		foreach (IWSection section in base.Document.Sections)
		{
			ParseTextBody(section.Body, tocLinkCharacterStyleNames, list, ref isParsingTOCParagraph);
		}
		return list;
	}

	private void ParseTextBody(WTextBody textBody, List<string> tocLinkCharacterStyleNames, List<ParagraphItem> tocParaItems, ref bool isParsingTOCParagraph)
	{
		for (int i = 0; i < textBody.Items.Count; i++)
		{
			if (textBody.Items[i] is WParagraph)
			{
				IWParagraph iWParagraph = textBody.Items[i] as WParagraph;
				ParseParagraph(iWParagraph, tocLinkCharacterStyleNames, tocParaItems, ref isParsingTOCParagraph);
				i = textBody.Items.IndexOf(iWParagraph);
			}
			else if (textBody.Items[i] is WTable)
			{
				IWTable iWTable = textBody.ChildEntities[i] as WTable;
				ParseTable(iWTable, tocLinkCharacterStyleNames, tocParaItems, ref isParsingTOCParagraph);
				i = textBody.Items.IndexOf(iWTable);
			}
			else if (textBody.Items[i] is BlockContentControl)
			{
				BlockContentControl blockContentControl = textBody.ChildEntities[i] as BlockContentControl;
				ParseTextBody(blockContentControl.TextBody, tocLinkCharacterStyleNames, tocParaItems, ref isParsingTOCParagraph);
				i = textBody.Items.IndexOf(blockContentControl);
			}
		}
	}

	private void ParseTable(IWTable table, List<string> tocLinkCharacterStyleNames, List<ParagraphItem> tocParaItems, ref bool isParsingTOCParagraph)
	{
		for (int i = 0; i < table.Rows.Count; i++)
		{
			WTableRow wTableRow = table.Rows[i];
			for (int j = 0; j < wTableRow.Cells.Count; j++)
			{
				WTableCell textBody = wTableRow.Cells[j];
				ParseTextBody(textBody, tocLinkCharacterStyleNames, tocParaItems, ref isParsingTOCParagraph);
			}
		}
	}

	private bool CheckTableOfFiguresLabel(IWParagraph paragraph)
	{
		if (TableOfFiguresLabel != null)
		{
			foreach (ParagraphItem childEntity in paragraph.ChildEntities)
			{
				if (!(childEntity is WSeqField))
				{
					continue;
				}
				WSeqField wSeqField = (WSeqField)childEntity;
				if (wSeqField.FieldEnd.OwnerParagraph != paragraph)
				{
					continue;
				}
				string value = TableOfFiguresLabel.Replace(' ', '_').ToLower();
				string seqCaptionName = wSeqField.GetSeqCaptionName();
				if (seqCaptionName != null && seqCaptionName.ToLower().Equals(value))
				{
					if (IncludeCaptionLabelsAndNumbers)
					{
						return true;
					}
					if (wSeqField.FieldEnd.Index + 1 < paragraph.ChildEntities.Count)
					{
						m_captionEndEntity = wSeqField.FieldEnd;
						return true;
					}
					break;
				}
			}
		}
		return false;
	}

	private void ParseParagraph(IWParagraph paragraph, List<string> tocLinkCharacterStyleNames, List<ParagraphItem> tocParaItems, ref bool isParsingTOCParagraph)
	{
		bool isTocReferLinkStyle = false;
		bool flag = false;
		if (m_doc.m_tocEntryLastEntity is WParagraph && m_doc.m_tocEntryLastEntity == paragraph)
		{
			m_docTocEntryLastEntityReached = true;
		}
		if ((!(paragraph as WParagraph).IsEmptyParagraph() || paragraph.ListFormat.ListType == ListType.Numbered) && (CheckOutlineParagraph(paragraph) || CheckParagraphStyle(paragraph.StyleName) || CheckTableOfFiguresLabel(paragraph) || CheckAndGetTOCLinkStyle(paragraph as WParagraph, ref isTocReferLinkStyle, tocLinkCharacterStyleNames, tocParaItems) != ""))
		{
			CheckAndSplitParagraph(paragraph);
			if (!string.IsNullOrEmpty(paragraph.Text) || paragraph.ListFormat.ListType == ListType.Numbered || (paragraph as WParagraph).IsContainsInLineImage())
			{
				int startIndex = 0;
				int num = 0;
				int num2 = -1;
				bool flag2 = false;
				foreach (ParagraphItem item in paragraph.Items)
				{
					if (!isParsingTOCParagraph && base.OwnerParagraph == paragraph)
					{
						isParsingTOCParagraph = true;
					}
					if (item is WPicture && (item as WPicture).TextWrappingStyle == TextWrappingStyle.Inline)
					{
						startIndex = paragraph.Items.IndexOf(item);
						break;
					}
					if (item is WTextRange && !(item is WField) && num == 0)
					{
						if (base.OwnerParagraph != paragraph || !((item as WTextRange).Text == "TOC"))
						{
							if (!((item as WTextRange).Text == ControlChar.Tab))
							{
								startIndex = ((num2 > -1) ? num2 : paragraph.Items.IndexOf(item));
								break;
							}
							num2 = -1;
						}
					}
					else if (item is WField || item is TableOfContent)
					{
						if (item is WField && num == 0)
						{
							num2 = paragraph.Items.IndexOf(item);
						}
						num++;
						flag2 = true;
					}
					else if (flag2 && item is WFieldMark && ((item as WFieldMark).Type == FieldMarkType.FieldSeparator || (item as WFieldMark).Type == FieldMarkType.FieldEnd))
					{
						flag2 = false;
						num--;
					}
				}
				if (!isParsingTOCParagraph)
				{
					InsertBookmark(paragraph, null, startIndex, paragraph.Items.Count + 1, ref isTocReferLinkStyle, tocLinkCharacterStyleNames, tocParaItems);
				}
				flag = true;
			}
		}
		else if (!isParsingTOCParagraph && paragraph.ListFormat.CurrentListStyle != null)
		{
			WListLevel nearLevel = paragraph.ListFormat.CurrentListStyle.GetNearLevel(paragraph.ListFormat.ListLevelNumber);
			base.Document.UpdateListValue(paragraph as WParagraph, paragraph.ListFormat, nearLevel);
		}
		if (isParsingTOCParagraph && TOCField != null && paragraph.Items.IndexOf(TOCField.FieldEnd) != -1)
		{
			isParsingTOCParagraph = false;
		}
		List<int> list = new List<int>();
		for (int i = 0; i < paragraph.Items.Count; i++)
		{
			if (paragraph.Items[i] is WTextBox)
			{
				WTextBox wTextBox = paragraph.Items[i] as WTextBox;
				ParseTextBody(wTextBox.TextBoxBody, tocLinkCharacterStyleNames, tocParaItems, ref isParsingTOCParagraph);
			}
			else if (paragraph.Items[i] is Shape)
			{
				Shape shape = paragraph.Items[i] as Shape;
				ParseTextBody(shape.TextBody, tocLinkCharacterStyleNames, tocParaItems, ref isParsingTOCParagraph);
			}
			else if (paragraph.Items[i] is WField && (paragraph.Items[i] as WField).FieldType == FieldType.FieldTOCEntry && UseTableEntryFields)
			{
				list.Add(i);
			}
		}
		for (int j = 0; j < list.Count; j++)
		{
			int num3 = list[j] + j * 2;
			list.RemoveAt(j);
			WField wField = paragraph.Items[num3] as WField;
			int num4 = num3;
			for (IEntity entity = wField; entity != null; entity = entity.NextSibling)
			{
				num4++;
				if (entity is WFieldMark && (entity as WFieldMark).Type == FieldMarkType.FieldEnd)
				{
					break;
				}
			}
			bool flag3 = false;
			string[] array = wField.FormattingString.Split('\\');
			foreach (string text in array)
			{
				if (text.StartsWith("f"))
				{
					flag3 = !string.IsNullOrEmpty(text.Substring(1).Replace(" ", ""));
					break;
				}
			}
			if (!flag3)
			{
				InsertBookmark(paragraph, wField, num3, num4 + 1, ref isTocReferLinkStyle, tocLinkCharacterStyleNames, tocParaItems);
				m_tocEntryLastEntity = wField;
				if (m_docTocEntryLastEntityReached)
				{
					m_doc.m_tocEntryLastEntity = m_tocEntryLastEntity;
				}
			}
		}
		if (flag)
		{
			m_tocEntryLastEntity = paragraph as WParagraph;
			if (m_docTocEntryLastEntityReached)
			{
				m_doc.m_tocEntryLastEntity = m_tocEntryLastEntity;
			}
		}
		isTocReferLinkStyle = false;
	}

	private bool CheckOutlineParagraph(IWParagraph paragraph)
	{
		if (UseOutlineLevels && paragraph.ParagraphFormat.PropertiesHash.ContainsKey(56))
		{
			int num = (int)(paragraph.ParagraphFormat.OutlineLevel + 1);
			if (num >= m_lowerHeadingLevel)
			{
				return num <= m_upperHeadingLevel;
			}
			return false;
		}
		return false;
	}

	private void CheckAndSplitParagraph(IWParagraph paragraph)
	{
		if (!paragraph.Text.Contains(ControlChar.Tab) && !paragraph.Text.Contains(ControlChar.LineFeed) && !paragraph.Text.Contains(ControlChar.CarriegeReturn))
		{
			return;
		}
		for (int i = 0; i < paragraph.Items.Count; i++)
		{
			if (paragraph.Items[i] is WTextRange && !(paragraph.Items[i] is WField))
			{
				WTextRange wTextRange = paragraph.Items[i] as WTextRange;
				if (wTextRange.Text != ControlChar.Tab && wTextRange.Text.Contains(ControlChar.Tab))
				{
					UpdateTabCharacters(wTextRange);
				}
				if (wTextRange.Text.Contains(ControlChar.LineFeed))
				{
					UpdateNewLineCharacters(wTextRange, ControlChar.LineFeed);
					break;
				}
				if (wTextRange.Text.Contains(ControlChar.CarriegeReturn))
				{
					UpdateNewLineCharacters(wTextRange, ControlChar.CarriegeReturn);
					break;
				}
			}
		}
	}

	private void UpdateTabCharacters(WTextRange textRange)
	{
		WParagraph ownerParagraph = textRange.OwnerParagraph;
		string text = textRange.Text;
		int num = text.IndexOf(ControlChar.Tab);
		int num2 = ownerParagraph.Items.IndexOf(textRange);
		string text2 = text.Substring(num + 1);
		WTextRange wTextRange = textRange.Clone() as WTextRange;
		if (num > 0)
		{
			wTextRange.Text = text.Substring(num);
			textRange.Text = text.Substring(0, num);
		}
		else if (text2 != string.Empty)
		{
			wTextRange.Text = text2;
			textRange.Text = ControlChar.Tab;
		}
		ownerParagraph.Items.Insert(num2 + 1, wTextRange);
	}

	private void UpdateNewLineCharacters(WTextRange textRange, string splitText)
	{
		WParagraph ownerParagraph = textRange.OwnerParagraph;
		string text = textRange.Text;
		int num = text.IndexOf(splitText);
		int num2 = ownerParagraph.Items.IndexOf(textRange);
		string text2 = text.Substring(num + 1);
		if (text2 != string.Empty)
		{
			WTextRange wTextRange = textRange.Clone() as WTextRange;
			wTextRange.Text = text2;
			ownerParagraph.Items.Insert(num2 + 1, wTextRange);
			textRange.Text = text.Substring(0, num);
		}
		else
		{
			textRange.Text = text.Substring(0, num);
		}
		CreateParagraph(ownerParagraph, num2 + 1);
		if (textRange.Text == string.Empty)
		{
			ownerParagraph.Items.Remove(textRange);
		}
	}

	private void CreateParagraph(WParagraph paragraph, int index)
	{
		WTextBody ownerTextBody = paragraph.OwnerTextBody;
		int num = ownerTextBody.Items.IndexOf(paragraph);
		WParagraph wParagraph = paragraph.Clone() as WParagraph;
		wParagraph.Items.Clear();
		ownerTextBody.Items.Insert(num + 1, wParagraph);
		while (paragraph.Items.Count > index)
		{
			wParagraph.Items.Insert(wParagraph.Items.Count, paragraph.Items[index]);
		}
	}

	private void SplitTOCParagraph(WParagraph tocParagraph, ref int paraIndex)
	{
		if (tocParagraph.ChildEntities.Count > 0 && !(tocParagraph.ChildEntities[0] is TableOfContent) && ContainsValidItems(tocParagraph))
		{
			WParagraph wParagraph = tocParagraph.Clone() as WParagraph;
			wParagraph.ChildEntities.Clear();
			wParagraph.ChildEntities.Add(tocParagraph.ChildEntities[0]);
			while (tocParagraph.ChildEntities.Count > 0 && !(tocParagraph.ChildEntities[0] is TableOfContent))
			{
				wParagraph.ChildEntities.Add(tocParagraph.ChildEntities[0]);
			}
			tocParagraph.OwnerTextBody.ChildEntities.Insert(paraIndex, wParagraph);
			paraIndex++;
		}
	}

	private bool ContainsValidItems(WParagraph paragraph)
	{
		foreach (Entity childEntity in paragraph.ChildEntities)
		{
			if (!(childEntity is BookmarkStart) && !(childEntity is BookmarkEnd))
			{
				if (childEntity is TableOfContent)
				{
					break;
				}
				return true;
			}
		}
		return false;
	}

	internal void RemoveUpdatedTocEntries()
	{
		WParagraph ownerParagraph = base.OwnerParagraph;
		WTextBody ownerTextBody = ownerParagraph.OwnerTextBody;
		int paraIndex = ownerTextBody.Items.IndexOf(ownerParagraph);
		SplitTOCParagraph(ownerParagraph, ref paraIndex);
		ownerParagraph = base.OwnerParagraph;
		bool flag = true;
		bool flag2 = false;
		int i = paraIndex;
		int num = 0;
		for (; i < ownerTextBody.Items.Count; i++)
		{
			if (!(ownerTextBody.Items[i] is WParagraph))
			{
				continue;
			}
			WParagraph wParagraph = ownerTextBody.Items[i] as WParagraph;
			int num2 = 0;
			if (i == paraIndex)
			{
				num2 = wParagraph.Items.IndexOf(this);
			}
			int num3;
			for (num3 = num2; num3 < wParagraph.Items.Count; num3++)
			{
				if (wParagraph.Items[num3] == this)
				{
					for (int j = num3; j + 1 < wParagraph.Items.Count; j++)
					{
						if (wParagraph.Items[j + 1] is WFieldMark && (wParagraph.Items[j + 1] as WFieldMark).Type == FieldMarkType.FieldSeparator)
						{
							num3 = j;
							break;
						}
					}
					if (wParagraph.Items[num3 + 1] is WFieldMark && (wParagraph.Items[num3 + 1] as WFieldMark).Type == FieldMarkType.FieldSeparator)
					{
						num++;
						num3++;
						continue;
					}
				}
				if (wParagraph.Items[num3] is WFieldMark)
				{
					WFieldMark wFieldMark = wParagraph.Items[num3] as WFieldMark;
					if (wFieldMark.Type == FieldMarkType.FieldSeparator)
					{
						num++;
					}
					else if (wFieldMark.Type == FieldMarkType.FieldEnd)
					{
						flag2 = TOCField.FieldEnd == wFieldMark;
						if ((num3 == 0 || flag2) && num == 1)
						{
							break;
						}
						num--;
					}
				}
				else
				{
					if (wParagraph.Items[num3] is WTextRange && (wParagraph.Items[num3] as WTextRange).Text == "TOC")
					{
						flag = false;
						break;
					}
					if (wParagraph.Items[num3] is WField && (wParagraph.Items[num3] as WField).FieldType == FieldType.FieldHyperlink)
					{
						string text = (wParagraph.Items[num3] as WField).FieldValue.Replace('"'.ToString(), string.Empty);
						if (text.StartsWith("_Toc"))
						{
							Bookmark bookmark = base.Document.Bookmarks.FindByName(text);
							if (bookmark != null)
							{
								base.Document.Bookmarks.Remove(bookmark);
							}
						}
					}
				}
				wParagraph.Items.Remove(wParagraph.Items[num3]);
				num3--;
			}
			if (wParagraph.Items.Count == 0)
			{
				ownerTextBody.Items.Remove(wParagraph);
				i--;
				continue;
			}
			if ((wParagraph.Items[0] is WFieldMark || flag2) && flag)
			{
				int num4 = 0;
				for (int k = Index; k < ownerParagraph.Items.Count; k++)
				{
					ParagraphItem paragraphItem = ownerParagraph.Items[k];
					wParagraph.Items.Insert(num4, paragraphItem);
					num4++;
					if (paragraphItem is WFieldMark && (paragraphItem as WFieldMark).Type == FieldMarkType.FieldSeparator)
					{
						break;
					}
					if (wParagraph != ownerParagraph)
					{
						k--;
					}
				}
				WTextRange wTextRange = new WTextRange(base.Document);
				wTextRange.Text = "TOC";
				wParagraph.Items.Insert(num4, wTextRange);
				if (wParagraph != ownerParagraph)
				{
					ownerTextBody.Items.Remove(ownerParagraph);
				}
				break;
			}
			if (!flag)
			{
				break;
			}
		}
	}

	private bool CheckParagraphStyle(string styleName)
	{
		styleName = ((styleName == null) ? "normal" : styleName.ToLower().Replace(" ", ""));
		foreach (KeyValuePair<int, List<string>> tOCLevel in TOCLevels)
		{
			foreach (string item in tOCLevel.Value)
			{
				string value = item.ToLower().Replace(" ", "");
				if (styleName.StartsWith(value))
				{
					return true;
				}
			}
		}
		return false;
	}

	private string CheckAndGetTOCLinkStyle(WParagraph paragraph, ref bool isTocReferLinkStyle, List<string> tocLinkCharacterStyleNames, List<ParagraphItem> tocParaItems)
	{
		int tOCValidItemIndex = GetTOCValidItemIndex(paragraph);
		if (tOCValidItemIndex == int.MinValue)
		{
			return "";
		}
		for (int i = tOCValidItemIndex; i < paragraph.ChildEntities.Count; i++)
		{
			if (!(m_doc.m_tocEntryLastEntity is WParagraph) && m_doc.m_tocEntryLastEntity == paragraph.ChildEntities[i])
			{
				m_docTocEntryLastEntityReached = true;
			}
			switch (paragraph.ChildEntities[i].EntityType)
			{
			case EntityType.TextRange:
				if (tocLinkCharacterStyleNames.Contains((paragraph.ChildEntities[i] as WTextRange).CharacterFormat.CharStyleName))
				{
					isTocReferLinkStyle = true;
					if (!tocParaItems.Contains(paragraph.ChildEntities[i] as ParagraphItem))
					{
						ParagraphItem paragraphItem2 = paragraph.ChildEntities[i] as ParagraphItem;
						tocParaItems.Add(paragraphItem2);
						m_tocEntryLastEntity = paragraphItem2;
						if (m_docTocEntryLastEntityReached)
						{
							m_doc.m_tocEntryLastEntity = m_tocEntryLastEntity;
						}
					}
					return (paragraph.ChildEntities[i] as WTextRange).CharacterFormat.CharStyleName;
				}
				return "";
			case EntityType.Picture:
				if (tocLinkCharacterStyleNames.Contains((paragraph.ChildEntities[i] as WPicture).CharacterFormat.CharStyleName))
				{
					isTocReferLinkStyle = true;
					if (!tocParaItems.Contains(paragraph.ChildEntities[i] as ParagraphItem))
					{
						ParagraphItem paragraphItem3 = paragraph.ChildEntities[i] as ParagraphItem;
						tocParaItems.Add(paragraphItem3);
						m_tocEntryLastEntity = paragraphItem3;
						if (m_docTocEntryLastEntityReached)
						{
							m_doc.m_tocEntryLastEntity = m_tocEntryLastEntity;
						}
					}
					return (paragraph.ChildEntities[i] as WPicture).CharacterFormat.CharStyleName;
				}
				return "";
			case EntityType.MergeField:
				if (tocLinkCharacterStyleNames.Contains((paragraph.ChildEntities[i] as WMergeField).CharacterFormat.CharStyleName))
				{
					isTocReferLinkStyle = true;
					if (!tocParaItems.Contains(paragraph.ChildEntities[i] as ParagraphItem))
					{
						ParagraphItem paragraphItem = paragraph.ChildEntities[i] as ParagraphItem;
						tocParaItems.Add(paragraphItem);
						m_tocEntryLastEntity = paragraphItem;
						if (m_docTocEntryLastEntityReached)
						{
							m_doc.m_tocEntryLastEntity = m_tocEntryLastEntity;
						}
					}
					return (paragraph.ChildEntities[i] as WMergeField).CharacterFormat.CharStyleName;
				}
				return "";
			}
		}
		return "";
	}

	private int GetTOCLevel(string styleName, WParagraph paragraph)
	{
		int result = 0;
		if (paragraph.ParagraphFormat.PropertiesHash.ContainsKey(56))
		{
			result = (int)(paragraph.ParagraphFormat.OutlineLevel + 1);
		}
		else
		{
			styleName = styleName.ToLower().Replace(" ", "");
			foreach (KeyValuePair<int, List<string>> tOCLevel in TOCLevels)
			{
				foreach (string item in tOCLevel.Value)
				{
					string value = item.ToLower().Replace(" ", "");
					if (styleName.StartsWith(value))
					{
						result = tOCLevel.Key;
						return result;
					}
				}
			}
		}
		return result;
	}

	private void InsertBookmark(IWParagraph paragraph, WField field, int startIndex, int endIndex, ref bool isTocReferLinkStyle, List<string> tocLinkCharacterStyleNames, List<ParagraphItem> tocParaItems)
	{
		string text = GenerateBookmarkName();
		BookmarkStart entity = new BookmarkStart(base.Document, text);
		paragraph.Items.Insert(startIndex, entity);
		InsertBookmarkHyperlink(paragraph, field, text, ref isTocReferLinkStyle, tocLinkCharacterStyleNames, tocParaItems);
		if (field == null)
		{
			endIndex = paragraph.Items.Count;
		}
		BookmarkEnd entity2 = new BookmarkEnd(base.Document, text);
		paragraph.Items.Insert(endIndex, entity2);
	}

	private void InsertBookmarkHyperlink(IWParagraph paragraph, WField field, string bookmark, ref bool isTocReferLinkStyle, List<string> tocLinkCharacterStyleNames, List<ParagraphItem> tocParaItems)
	{
		int num = 0;
		WParagraph wParagraph = paragraph as WParagraph;
		num = ((FormattingString.Contains("\\c") || FormattingString.Contains("\\a")) ? 1 : (isTocReferLinkStyle ? GetTOCLevel(CheckAndGetTOCLinkStyle(wParagraph, ref isTocReferLinkStyle, tocLinkCharacterStyleNames, tocParaItems), wParagraph) : GetTOCLevel(paragraph.StyleName, wParagraph)));
		string text = string.Empty;
		bool flag = true;
		if (field != null)
		{
			text = field.FieldValue;
			num = 1;
			string[] array = field.FormattingString.Split('\\');
			foreach (string text2 in array)
			{
				if (text2.StartsWith("l"))
				{
					if (!int.TryParse(text2.Substring(1).Replace(" ", ""), out num))
					{
						num = 1;
					}
				}
				else if (text2.StartsWith("n"))
				{
					flag = false;
				}
			}
		}
		WParagraph wParagraph2 = CreateTOCParagraph(num);
		CreateHyperlink(paragraph, wParagraph2, text, bookmark, ref isTocReferLinkStyle, tocLinkCharacterStyleNames);
		if (IncludePageNumbers && flag)
		{
			AddTabsAndPageRefField(wParagraph2, bookmark);
		}
		if (wParagraph2 == null || TOCEntryEntities.ContainsKey(wParagraph2))
		{
			return;
		}
		if (isTocReferLinkStyle && m_tocEntryLastEntity != null)
		{
			TOCEntryEntities.Add(wParagraph2, m_tocEntryLastEntity);
		}
		else if (field != null || wParagraph != null)
		{
			if (field != null)
			{
				TOCEntryEntities.Add(wParagraph2, field);
			}
			else
			{
				TOCEntryEntities.Add(wParagraph2, wParagraph);
			}
		}
	}

	private void CreateHyperlink(IWParagraph paragraph, WParagraph tocParagraph, string text, string bookmark, ref bool isTocReferLinkStyle, List<string> tocLinkStyles)
	{
		WField wField = new WField(base.Document);
		wField.FieldType = FieldType.FieldHyperlink;
		wField.CharacterFormat.CharStyleName = "Hyperlink";
		tocParagraph.Items.Add(wField);
		WTextRange wTextRange = new WTextRange(base.Document);
		wTextRange.Text = "HYPERLINK";
		tocParagraph.Items.Add(wTextRange);
		wField.FieldSeparator = tocParagraph.AppendFieldMark(FieldMarkType.FieldSeparator);
		wField.FieldSeparator.CharacterFormat.CharStyleName = "Hyperlink";
		if (!string.IsNullOrEmpty(text))
		{
			if (isTocReferLinkStyle)
			{
				bool flag = false;
				WParagraphStyle paragraphStyle = (paragraph as WParagraph).ParaStyle as WParagraphStyle;
				foreach (ParagraphItem item in paragraph.Items)
				{
					if (item is WField && (item as WField).FieldType == FieldType.FieldTOCEntry)
					{
						flag = true;
					}
					WTextRange wTextRange2 = ((item is WField) ? null : (item as WTextRange));
					if (flag && wTextRange2 != null && wTextRange2.Text != "\" " && wTextRange2.Text != ControlChar.Tab)
					{
						AppendTextToTocParagraph(wTextRange2, wTextRange2.Text, paragraphStyle, tocParagraph);
					}
					if (flag && item is WFieldMark && (item as WFieldMark).Type == FieldMarkType.FieldEnd)
					{
						break;
					}
				}
			}
			else
			{
				tocParagraph.AppendText(text).CharacterFormat.CharStyleName = "Hyperlink";
			}
		}
		else
		{
			CreateHyperLink(paragraph, tocParagraph, isTocReferLinkStyle, tocLinkStyles);
		}
		WFieldMark wFieldMark = new WFieldMark(base.Document, FieldMarkType.FieldEnd);
		tocParagraph.Items.Add(wFieldMark);
		wFieldMark.CharacterFormat.CharStyleName = "Hyperlink";
		wField.FieldEnd = wFieldMark;
		new Hyperlink(wField)
		{
			Type = HyperlinkType.Bookmark,
			BookmarkName = bookmark
		};
	}

	private void CreateHyperLink(IWParagraph paragraph, WParagraph tocParagraph, bool isTocReferLinkStyle, List<string> tocLinkStyles)
	{
		bool isTabAdded = false;
		UpdateList(paragraph, tocParagraph, ref isTabAdded);
		WParagraphStyle paragraphStyle = (paragraph as WParagraph).ParaStyle as WParagraphStyle;
		if (!isTocReferLinkStyle)
		{
			int num = 0;
			bool flag = false;
			Entity entity = null;
			int i = 0;
			if (!IncludeCaptionLabelsAndNumbers && m_captionEndEntity != null)
			{
				i = m_captionEndEntity.Index + 1;
				m_captionEndEntity = null;
			}
			for (; i < paragraph.ChildEntities.Count; i++)
			{
				ParagraphItem paragraphItem = paragraph.ChildEntities[i] as ParagraphItem;
				if (entity == null && !(paragraphItem is BookmarkStart) && !(paragraphItem is BookmarkEnd) && !(paragraphItem is EditableRangeStart) && !(paragraphItem is EditableRangeEnd) && !(paragraphItem is WCommentMark))
				{
					entity = paragraphItem;
				}
				WTextRange wTextRange = ((paragraphItem is WField) ? null : (paragraphItem as WTextRange));
				if (paragraphItem is InlineContentControl && (paragraphItem as InlineContentControl).MappedItem is WTextRange)
				{
					wTextRange = (paragraphItem as InlineContentControl).MappedItem as WTextRange;
				}
				if (paragraph.ChildEntities[i] is WPicture && (paragraph.ChildEntities[i] as WPicture).TextWrappingStyle == TextWrappingStyle.Inline)
				{
					tocParagraph.Items.Add(paragraph.ChildEntities[i].Clone());
				}
				else if (wTextRange != null && num == 0)
				{
					if (wTextRange.Text != ControlChar.Tab)
					{
						string text = wTextRange.Text;
						if (entity == paragraphItem)
						{
							text = text.TrimStart();
							if (text == string.Empty)
							{
								entity = null;
							}
						}
						AppendTextToTocParagraph(wTextRange, text, paragraphStyle, tocParagraph);
					}
					else
					{
						if (!IsNeedToAddTabStop(paragraph as WParagraph, i))
						{
							continue;
						}
						if (!isTabAdded)
						{
							bool isTabStopPosFromStyle = false;
							float position = UpdateTabStopPosition(tocParagraph, ref isTabStopPosFromStyle);
							if (!isTabStopPosFromStyle)
							{
								tocParagraph.ParagraphFormat.Tabs.AddTab(position, TabJustification.Left, TabLeader.NoLeader);
							}
							tocParagraph.AppendText(ControlChar.Tab);
							isTabAdded = true;
						}
						else
						{
							AppendTextToTocParagraph(wTextRange, ControlChar.Space, paragraphStyle, tocParagraph);
						}
					}
				}
				else if (IncludeNewLineCharacters && paragraphItem is Break && ((paragraphItem as Break).BreakType == BreakType.TextWrappingBreak || (paragraphItem as Break).BreakType == BreakType.LineBreak) && (paragraphItem as Break).TextRange.Text != ControlChar.CarriegeReturn)
				{
					tocParagraph.AppendBreak(BreakType.LineBreak);
				}
				else if (paragraphItem is WField || paragraphItem is TableOfContent)
				{
					num++;
					flag = true;
				}
				else if (flag && paragraphItem is WFieldMark && ((paragraphItem as WFieldMark).Type == FieldMarkType.FieldSeparator || (paragraphItem as WFieldMark).Type == FieldMarkType.FieldEnd))
				{
					num--;
					flag = false;
				}
			}
		}
		else
		{
			createHyperLinkForLinkStyle(paragraph, tocParagraph, tocLinkStyles);
		}
		for (int num2 = tocParagraph.ChildEntities.Count - 1; num2 >= 0; num2--)
		{
			ParagraphItem paragraphItem2 = tocParagraph.ChildEntities[num2] as ParagraphItem;
			if (!(paragraphItem2 is BookmarkStart) && !(paragraphItem2 is BookmarkEnd) && !(paragraphItem2 is EditableRangeStart) && !(paragraphItem2 is EditableRangeEnd) && !(paragraphItem2 is WCommentMark))
			{
				if (!(paragraphItem2 is WTextRange))
				{
					break;
				}
				(paragraphItem2 as WTextRange).Text = (paragraphItem2 as WTextRange).Text.TrimEnd();
				if ((paragraphItem2 as WTextRange).Text != string.Empty)
				{
					break;
				}
			}
		}
	}

	private void AppendTextToTocParagraph(WTextRange paragraphTextRange, string txtValue, WParagraphStyle paragraphStyle, WParagraph tocParagraph)
	{
		IWTextRange iWTextRange = tocParagraph.AppendText(txtValue);
		WCharacterStyle charStyle = paragraphTextRange.CharacterFormat.CharStyle;
		if (paragraphTextRange.CharacterFormat.HasKey(2) && (!paragraphStyle.CharacterFormat.HasKey(2) || paragraphTextRange.CharacterFormat.PropertiesHash[2].ToString() != paragraphStyle.CharacterFormat.PropertiesHash[2].ToString()))
		{
			iWTextRange.CharacterFormat.FontName = paragraphTextRange.CharacterFormat.FontName;
		}
		if (IsNeedToApplyFormatting(4, paragraphStyle, paragraphTextRange, charStyle))
		{
			iWTextRange.CharacterFormat.Bold = paragraphTextRange.CharacterFormat.Bold;
		}
		if (IsNeedToApplyFormatting(5, paragraphStyle, paragraphTextRange, charStyle))
		{
			iWTextRange.CharacterFormat.Italic = paragraphTextRange.CharacterFormat.Italic;
		}
		if (IsNeedToApplyFormatting(2, paragraphStyle, paragraphTextRange, charStyle))
		{
			iWTextRange.CharacterFormat.FontName = paragraphTextRange.CharacterFormat.FontName;
		}
		if (IsNeedToApplyFormatting(63, paragraphStyle, paragraphTextRange, charStyle))
		{
			iWTextRange.CharacterFormat.HighlightColor = paragraphTextRange.CharacterFormat.HighlightColor;
		}
		if (IsNeedToApplyFormatting(10, paragraphStyle, paragraphTextRange, charStyle))
		{
			iWTextRange.CharacterFormat.SubSuperScript = paragraphTextRange.CharacterFormat.SubSuperScript;
		}
		if (IsNeedToApplyFormatting(55, paragraphStyle, paragraphTextRange, charStyle))
		{
			iWTextRange.CharacterFormat.SmallCaps = paragraphTextRange.CharacterFormat.SmallCaps;
		}
		if (IsNeedToApplyFormatting(54, paragraphStyle, paragraphTextRange, charStyle))
		{
			iWTextRange.CharacterFormat.AllCaps = paragraphTextRange.CharacterFormat.AllCaps;
		}
		if (IsNeedToApplyFormatting(53, paragraphStyle, paragraphTextRange, charStyle))
		{
			iWTextRange.CharacterFormat.Hidden = paragraphTextRange.CharacterFormat.Hidden;
		}
		if (IsNeedToApplyFormatting(6, paragraphStyle, paragraphTextRange, charStyle))
		{
			iWTextRange.CharacterFormat.Strikeout = paragraphTextRange.CharacterFormat.Strikeout;
		}
		if (IsNeedToApplyFormatting(14, paragraphStyle, paragraphTextRange, charStyle))
		{
			iWTextRange.CharacterFormat.DoubleStrike = paragraphTextRange.CharacterFormat.DoubleStrike;
		}
		iWTextRange.CharacterFormat.CharStyleName = "Hyperlink";
	}

	private bool IsNeedToApplyFormatting(short key, WParagraphStyle paragraphStyle, WTextRange paragraphTextRange, WCharacterStyle charStyle)
	{
		if (!paragraphStyle.CharacterFormat.HasValue(key) && (paragraphTextRange.CharacterFormat.HasValue(key) || (charStyle != null && charStyle.CharacterFormat.HasValue(key))))
		{
			return true;
		}
		return false;
	}

	private bool IsNeedToAddTabStop(WParagraph paragraph, int currentTabItemIndex)
	{
		if (currentTabItemIndex == 0 || currentTabItemIndex == paragraph.ChildEntities.Count - 1)
		{
			return false;
		}
		if (!IsNeedToAddTabStop(0, currentTabItemIndex + 1, paragraph))
		{
			return false;
		}
		return IsNeedToAddTabStop(currentTabItemIndex, paragraph.ChildEntities.Count, paragraph);
	}

	private bool IsNeedToAddTabStop(int startIndex, int endIndex, WParagraph ownerParagraph)
	{
		for (int i = startIndex; i < endIndex; i++)
		{
			WTextRange wTextRange = ((ownerParagraph.ChildEntities[i] is WField) ? null : (ownerParagraph.ChildEntities[i] as WTextRange));
			if (wTextRange != null && wTextRange.Text != ControlChar.Tab && !string.IsNullOrEmpty(wTextRange.Text))
			{
				return true;
			}
		}
		return false;
	}

	private float UpdateTabStopPosition(WParagraph paragraph, ref bool isTabStopPosFromStyle)
	{
		float num = 0f;
		float tabPosition = GetTabPosition(paragraph);
		num += paragraph.ParagraphFormat.LeftIndent;
		DrawingContext drawingContext = new DrawingContext();
		int num2 = 0;
		foreach (ParagraphItem item in paragraph.Items)
		{
			WTextRange wTextRange = ((item is WField) ? null : (item as WTextRange));
			if (wTextRange != null && num2 == 0)
			{
				num += drawingContext.MeasureTextRange(wTextRange, wTextRange.Text).Width;
				if (num > tabPosition)
				{
					num = 0f;
					num += paragraph.ParagraphFormat.LeftIndent;
					num += drawingContext.MeasureTextRange(wTextRange, wTextRange.Text).Width;
				}
			}
			else if (item is WField || item is TableOfContent)
			{
				num2++;
			}
			else if (item is WFieldMark && ((item as WFieldMark).Type == FieldMarkType.FieldSeparator || (item as WFieldMark).Type == FieldMarkType.FieldEnd))
			{
				num2--;
			}
		}
		int num3 = 11;
		float num4 = num + (float)num3;
		if (num >= 77f)
		{
			if (paragraph.ParaStyle != null && paragraph.ParaStyle.ParagraphFormat.Tabs.Count != 0)
			{
				return GetTabPositionBasedParagraphStyle(paragraph.ParaStyle.ParagraphFormat.Tabs, ref isTabStopPosFromStyle, num, num4);
			}
			return num4;
		}
		num4 = ((int)num / num3 + 2) * num3;
		if (paragraph.ParaStyle != null && paragraph.ParaStyle.ParagraphFormat.Tabs.Count != 0)
		{
			return GetTabPositionBasedParagraphStyle(paragraph.ParaStyle.ParagraphFormat.Tabs, ref isTabStopPosFromStyle, num, num4);
		}
		return num4;
	}

	private float GetTabPositionBasedParagraphStyle(TabCollection tabs, ref bool isTabStopPosFromStyle, float preTextLength, float tabStopPosition)
	{
		for (int i = 0; i < tabs.Count; i++)
		{
			if ((tabs[i].Position > preTextLength && i != tabs.Count - 1) || tabs[i].Position == tabStopPosition)
			{
				isTabStopPosFromStyle = true;
				return tabs[i].Position;
			}
		}
		return tabStopPosition;
	}

	private void createHyperLinkForLinkStyle(IWParagraph paragraph, WParagraph tocParagraph, List<string> tocLinkStyles)
	{
		int tOCValidItemIndex = GetTOCValidItemIndex(paragraph as WParagraph);
		if (tOCValidItemIndex == int.MinValue)
		{
			return;
		}
		for (int i = tOCValidItemIndex; i < paragraph.ChildEntities.Count; i++)
		{
			bool flag = false;
			WParagraphStyle wParagraphStyle = (paragraph as WParagraph).ParaStyle as WParagraphStyle;
			switch (paragraph.ChildEntities[i].EntityType)
			{
			case EntityType.TextRange:
			case EntityType.MergeField:
				if (tocLinkStyles.Contains((paragraph.ChildEntities[i] as WTextRange).CharacterFormat.CharStyleName))
				{
					if (paragraph.ChildEntities[i] is WTextRange && !(paragraph.ChildEntities[i] is WField) && (paragraph.ChildEntities[i] as WTextRange).Text != ControlChar.Tab)
					{
						WTextRange wTextRange = paragraph.ChildEntities[i] as WTextRange;
						IWTextRange iWTextRange = tocParagraph.AppendText(wTextRange.Text);
						WCharacterStyle charStyle = wTextRange.CharacterFormat.CharStyle;
						if (!wParagraphStyle.CharacterFormat.HasValue(4) && wTextRange.CharacterFormat.HasValue(4))
						{
							iWTextRange.CharacterFormat.Bold = wTextRange.CharacterFormat.Bold;
						}
						if (!wParagraphStyle.CharacterFormat.HasValue(5) && wTextRange.CharacterFormat.HasValue(5))
						{
							iWTextRange.CharacterFormat.Italic = wTextRange.CharacterFormat.Italic;
						}
						if (IsNeedToApplyFontName(wTextRange, charStyle))
						{
							iWTextRange.CharacterFormat.FontName = wTextRange.CharacterFormat.FontName;
						}
					}
				}
				else
				{
					flag = true;
				}
				break;
			case EntityType.Picture:
				if (tocLinkStyles.Contains((paragraph.ChildEntities[i] as WPicture).CharacterFormat.CharStyleName))
				{
					WPicture entity = (WPicture)paragraph.ChildEntities[i].Clone();
					tocParagraph.ChildEntities.Insert(tocParagraph.ChildEntities.Count, entity);
				}
				else if (!paragraph.ChildEntities[i].IsFloatingItem(isTextWrapAround: false))
				{
					flag = true;
				}
				break;
			case EntityType.Field:
				if ((paragraph.ChildEntities[i] as WField).FieldType == FieldType.FieldTOCEntry)
				{
					i++;
					if (paragraph.ChildEntities[i] is WTextRange && tocLinkStyles.Contains((paragraph.ChildEntities[i] as WTextRange).CharacterFormat.CharStyleName))
					{
						for (; !(paragraph.ChildEntities[i] is WFieldMark) || (paragraph.ChildEntities[i] as WFieldMark).Type != FieldMarkType.FieldEnd; i++)
						{
						}
						break;
					}
					if (paragraph.ChildEntities[i] is WTextRange)
					{
						flag = true;
						break;
					}
					if (tocLinkStyles.Contains((paragraph.ChildEntities[i] as WField).CharacterFormat.CharStyleName))
					{
						break;
					}
					flag = true;
				}
				if (paragraph.ChildEntities[i] is WField)
				{
					do
					{
						i++;
					}
					while (!(paragraph.ChildEntities[i] is WFieldMark) || (paragraph.ChildEntities[i] as WFieldMark).Type != 0);
				}
				break;
			case EntityType.Break:
				if (!tocLinkStyles.Contains((paragraph.ChildEntities[i] as Break).CharacterFormat.CharStyleName))
				{
					flag = true;
				}
				else if ((paragraph.ChildEntities[i] as Break).TextRange.Text != ControlChar.Tab)
				{
					IWTextRange iWTextRange = tocParagraph.AppendText(" ");
					if (!wParagraphStyle.CharacterFormat.HasValue(4) && (paragraph.ChildEntities[i] as Break).CharacterFormat.HasValue(4))
					{
						iWTextRange.CharacterFormat.Bold = (paragraph.ChildEntities[i] as Break).CharacterFormat.Bold;
					}
					if (!wParagraphStyle.CharacterFormat.HasValue(5) && (paragraph.ChildEntities[i] as Break).CharacterFormat.HasValue(5))
					{
						iWTextRange.CharacterFormat.Italic = (paragraph.ChildEntities[i] as Break).CharacterFormat.Italic;
					}
					iWTextRange.CharacterFormat.CharStyleName = "Hyperlink";
				}
				break;
			}
			if (flag)
			{
				break;
			}
		}
	}

	private bool IsNeedToApplyFontName(WTextRange textrange, WCharacterStyle characterStyle)
	{
		string linkedStyleName = characterStyle.LinkedStyleName;
		string fontName = (m_doc.Styles.FindByName(linkedStyleName) as WParagraphStyle).CharacterFormat.FontName;
		if (!textrange.CharacterFormat.HasValue(2) || !(textrange.CharacterFormat.FontName != fontName))
		{
			if (characterStyle.CharacterFormat.HasValue(2))
			{
				return characterStyle.CharacterFormat.FontName != fontName;
			}
			return false;
		}
		return true;
	}

	private int GetTOCValidItemIndex(WParagraph paragraph)
	{
		for (int i = 0; i < paragraph.ChildEntities.Count; i++)
		{
			if ((paragraph.ChildEntities[i] is WTextRange && (paragraph.ChildEntities[i] as WTextRange).Text.Trim() == "") || paragraph.ChildEntities[i] is WIfField || paragraph.ChildEntities[i] is BookmarkStart || paragraph.ChildEntities[i] is BookmarkEnd || paragraph.ChildEntities[i].IsFloatingItem(isTextWrapAround: false) || (paragraph.ChildEntities[i] is WField && (paragraph.ChildEntities[i] as WField).FieldType == FieldType.FieldTOCEntry) || paragraph.ChildEntities[i] is Break)
			{
				continue;
			}
			if (paragraph.ChildEntities[i] is WField)
			{
				do
				{
					i++;
					if (paragraph.ChildEntities.Count - 1 < ++i)
					{
						return int.MinValue;
					}
				}
				while (!(paragraph.ChildEntities[i] is WFieldMark) || (paragraph.ChildEntities[i] as WFieldMark).Type != 0);
				return ++i;
			}
			return i;
		}
		return int.MinValue;
	}

	private void AddTabsAndPageRefField(WParagraph paragraph, string bookmark)
	{
		WTextRange wTextRange;
		if (RightAlignPageNumbers)
		{
			WParagraphStyle wParagraphStyle = paragraph.ParaStyle as WParagraphStyle;
			if (wParagraphStyle.ParagraphFormat.Tabs.Count == 0 || CheckTabJustification(wParagraphStyle))
			{
				paragraph.ParagraphFormat.Tabs.AddTab(GetTabPosition(paragraph), TabJustification.Right, m_tabLeader);
			}
			wTextRange = new WTextRange(base.Document);
			wTextRange.Text = ControlChar.Tab;
			paragraph.Items.Insert(paragraph.Items.Count - 1, wTextRange);
		}
		WField wField = new WField(base.Document);
		wField.FieldType = FieldType.FieldPageRef;
		wField.m_fieldValue = bookmark + " \\h";
		paragraph.Items.Insert(paragraph.Items.Count - 1, wField);
		WTextRange wTextRange2 = new WTextRange(base.Document);
		wTextRange2.Text = "PAGEREF " + bookmark + " \\h";
		wTextRange2.ApplyCharacterFormat(wField.CharacterFormat);
		paragraph.Items.Insert(paragraph.Items.Count - 1, wTextRange2);
		WFieldMark wFieldMark = new WFieldMark(base.Document, FieldMarkType.FieldSeparator);
		paragraph.Items.Insert(paragraph.Items.Count - 1, wFieldMark);
		wField.FieldSeparator = wFieldMark;
		wTextRange = new WTextRange(base.Document);
		paragraph.Items.Insert(paragraph.Items.Count - 1, wTextRange);
		wFieldMark = new WFieldMark(base.Document, FieldMarkType.FieldEnd);
		paragraph.Items.Insert(paragraph.Items.Count - 1, wFieldMark);
		wField.FieldEnd = wFieldMark;
	}

	private bool CheckTabJustification(WParagraphStyle paraStyle)
	{
		foreach (Tab tab in paraStyle.ParagraphFormat.Tabs)
		{
			if (!tab.PropertiesHash.ContainsKey(1))
			{
				return false;
			}
		}
		return true;
	}

	private float GetTabPosition(Entity entity)
	{
		float result = 0f;
		Entity entity2 = entity;
		while (!(entity2 is WSection) && entity2.Owner != null)
		{
			entity2 = entity2.Owner;
		}
		if (entity2 is WSection)
		{
			result = (((entity2 as WSection).Columns.Count <= 1) ? ((float)((double)(entity2 as WSection).PageSetup.ClientWidth - 0.5)) : ((float)((double)(entity2 as WSection).Columns[0].Width - 0.5)));
		}
		return result;
	}

	private WParagraph CreateTOCParagraph(int level)
	{
		WTextBody ownerTextBody = LastTOCParagraph.OwnerTextBody;
		int num = ownerTextBody.Items.IndexOf(LastTOCParagraph);
		int num2 = LastTOCParagraph.Items.IndexOf(this);
		if (num2 > 0)
		{
			CreateParagraph(LastTOCParagraph, num2);
			m_tocParagraph = base.OwnerParagraph;
		}
		num = ownerTextBody.Items.IndexOf(LastTOCParagraph);
		WParagraph wParagraph = new WParagraph(base.Document);
		ownerTextBody.Items.Insert(num, wParagraph);
		level += 18;
		if (FormattingString.Contains("\\c") || FormattingString.Contains("\\a"))
		{
			wParagraph.ApplyStyle(BuiltinStyle.TableOfFigures, isDomChanges: false);
		}
		else
		{
			wParagraph.ApplyStyle((BuiltinStyle)level, isDomChanges: false);
		}
		bool flag = true;
		if (LastTOCParagraph == base.OwnerParagraph)
		{
			for (int i = 0; i < LastTOCParagraph.Items.Count; i++)
			{
				if (LastTOCParagraph.Items[i] == this)
				{
					wParagraph.Items.Insert(wParagraph.Items.Count, LastTOCParagraph.Items[i]);
					i--;
				}
				else if (LastTOCParagraph.Items[i] is WFieldMark)
				{
					wParagraph.Items.Insert(wParagraph.Items.Count, LastTOCParagraph.Items[i]);
					i--;
					flag = false;
				}
				else if (LastTOCParagraph.Items[i] is WTextRange)
				{
					if (flag)
					{
						wParagraph.Items.Insert(wParagraph.Items.Count, LastTOCParagraph.Items[i]);
					}
					else
					{
						LastTOCParagraph.Items.Remove(LastTOCParagraph.Items[i]);
					}
					i--;
					if (!flag)
					{
						break;
					}
				}
			}
		}
		return wParagraph;
	}

	private string GenerateBookmarkName()
	{
		m_doc.m_tocBookmarkID++;
		string text = "_Toc" + $"{m_doc.m_tocBookmarkID:0000000000}";
		while (m_doc.Bookmarks.FindByName(text) != null)
		{
			m_doc.m_tocBookmarkID++;
			text = "_Toc" + $"{m_doc.m_tocBookmarkID:0000000000}";
		}
		return text;
	}

	internal void UpdatePageNumbers(Dictionary<Entity, int> tocEntryPageNumbers)
	{
		WParagraph ownerParagraph = base.OwnerParagraph;
		WTextBody ownerTextBody = ownerParagraph.OwnerTextBody;
		int num = ownerTextBody.ChildEntities.IndexOf(ownerParagraph);
		int num2 = ownerTextBody.ChildEntities.IndexOf(LastTOCParagraph);
		int i = num;
		int num3 = 0;
		for (; i < num2; i++)
		{
			if (num3 >= tocEntryPageNumbers.Count)
			{
				break;
			}
			WParagraph wParagraph = ownerTextBody.ChildEntities[i] as WParagraph;
			Entity entity = null;
			if (wParagraph != null && TOCEntryEntities.ContainsKey(wParagraph))
			{
				entity = TOCEntryEntities[wParagraph];
			}
			if (wParagraph == null || entity == null || !tocEntryPageNumbers.ContainsKey(entity))
			{
				continue;
			}
			if (wParagraph.Items.Count > 2 && wParagraph.Items[wParagraph.Items.Count - 3] is WTextRange)
			{
				string text = tocEntryPageNumbers[entity].ToString();
				if (entity.GetOwnerSection(entity) is WSection wSection)
				{
					text = wSection.PageSetup.GetNumberFormatValue((byte)wSection.PageSetup.PageNumberStyle, tocEntryPageNumbers[entity]);
				}
				(wParagraph.Items[wParagraph.Items.Count - 3] as WTextRange).Text = text;
			}
			num3++;
		}
	}

	private void UpdateList(IWParagraph paragraph, WParagraph tocParagraph, ref bool isTabAdded)
	{
		WParagraph wParagraph = paragraph as WParagraph;
		WListFormat wListFormat = null;
		WParagraphStyle wParagraphStyle = wParagraph.ParaStyle as WParagraphStyle;
		if (wParagraph.ListFormat.ListType != ListType.NoList || wParagraph.ListFormat.IsEmptyList)
		{
			wListFormat = wParagraph.ListFormat;
		}
		else if (wParagraphStyle != null)
		{
			wListFormat = wParagraphStyle.GetListFormatIncludeBaseStyle();
		}
		if (wListFormat == null || wListFormat.CurrentListStyle == null)
		{
			return;
		}
		ListStyle currentListStyle = wListFormat.CurrentListStyle;
		int levelNumber = 0;
		WParagraphStyle levelNumberStyle = null;
		if (wParagraph.ListFormat.HasKey(0))
		{
			levelNumber = wParagraph.ListFormat.ListLevelNumber;
		}
		else if (wParagraphStyle != null)
		{
			levelNumber = wParagraphStyle.GetListLevelNumberIncludeBaseStyle(ref levelNumberStyle);
		}
		WListLevel wListLevel = currentListStyle.GetNearLevel(levelNumber);
		if (levelNumberStyle == null || levelNumberStyle.IsCustom || !(wListLevel.ParaStyleName != levelNumberStyle.Name))
		{
			ListOverrideStyle listOverrideStyle = null;
			if (wListFormat.LFOStyleName != null && wListFormat.LFOStyleName.Length > 0)
			{
				listOverrideStyle = base.Document.ListOverrides.FindByName(wListFormat.LFOStyleName);
			}
			if (listOverrideStyle != null && listOverrideStyle.OverrideLevels.HasOverrideLevel(levelNumber) && listOverrideStyle.OverrideLevels[levelNumber].OverrideFormatting)
			{
				wListLevel = listOverrideStyle.OverrideLevels[levelNumber].OverrideListLevel;
			}
			string text = base.Document.UpdateListValue(wParagraph, wListFormat, wListLevel);
			if (text != string.Empty)
			{
				AddListValueAndTab(paragraph, tocParagraph, wParagraphStyle, text, ref isTabAdded, wListLevel.CharacterFormat);
			}
		}
	}

	private void AddListValueAndTab(IWParagraph paragraph, WParagraph tocParagraph, WParagraphStyle tocStyle, string listValue, ref bool isTabAdded, WCharacterFormat characterFormat)
	{
		IWTextRange iWTextRange = tocParagraph.AppendText(listValue);
		WParagraphStyle obj = (paragraph as WParagraph).ParaStyle as WParagraphStyle;
		if (!obj.CharacterFormat.HasValue(4) && paragraph.BreakCharacterFormat.HasValue(4))
		{
			iWTextRange.CharacterFormat.Bold = paragraph.BreakCharacterFormat.Bold;
		}
		if (!obj.CharacterFormat.HasValue(5) && paragraph.BreakCharacterFormat.HasValue(5))
		{
			iWTextRange.CharacterFormat.Italic = paragraph.BreakCharacterFormat.Italic;
		}
		if (characterFormat.HasKey(2))
		{
			iWTextRange.CharacterFormat.FontName = characterFormat.FontName;
		}
		else if (paragraph.BreakCharacterFormat.HasKey(2))
		{
			iWTextRange.CharacterFormat.FontName = paragraph.BreakCharacterFormat.FontName;
		}
		iWTextRange.CharacterFormat.CharStyleName = "Hyperlink";
		WListLevel wListLevel = characterFormat.OwnerBase as WListLevel;
		if (wListLevel.FollowCharacter == FollowCharacterType.Tab)
		{
			bool isTabStopPosFromStyle = false;
			float position = UpdateTabStopPosition(tocParagraph, ref isTabStopPosFromStyle);
			if (!isTabStopPosFromStyle)
			{
				tocParagraph.ParagraphFormat.Tabs.AddTab(position, TabJustification.Left, TabLeader.NoLeader);
			}
			iWTextRange = tocParagraph.AppendText(ControlChar.Tab);
		}
		else if (wListLevel.FollowCharacter == FollowCharacterType.Space || wListLevel.FollowCharacter == FollowCharacterType.Nothing)
		{
			iWTextRange = tocParagraph.AppendText(ControlChar.Space);
		}
		isTabAdded = true;
	}

	protected override void CreateLayoutInfo()
	{
		m_layoutInfo = new LayoutInfo();
		if (TOCField.FieldSeparator != null)
		{
			TOCField.SetSkipForFieldCode(base.NextSibling);
		}
	}

	internal override void InitLayoutInfo(Entity entity, ref bool isLastTOCEntry)
	{
		m_layoutInfo = null;
		if (this == entity)
		{
			isLastTOCEntry = true;
		}
	}

	protected override void InitXDLSHolder()
	{
		if (InvalidFormatString)
		{
			UpdateFormattingString();
		}
		base.XDLSHolder.AddElement("toc-field", m_tocField);
	}

	protected override void WriteXmlAttributes(IXDLSAttributeWriter writer)
	{
		base.WriteXmlAttributes(writer);
		writer.WriteValue("type", ParagraphItemType.TOC);
	}

	protected override object CloneImpl()
	{
		if (InvalidFormatString)
		{
			UpdateFormattingString();
		}
		TableOfContent tableOfContent = (TableOfContent)base.CloneImpl();
		tableOfContent.m_tocField = (WField)m_tocField.Clone();
		if (m_tocStyles != null)
		{
			tableOfContent.m_tocStyles = new Dictionary<int, List<WParagraphStyle>>();
			foreach (KeyValuePair<int, List<WParagraphStyle>> tocStyle in m_tocStyles)
			{
				tableOfContent.m_tocStyles.Add(tocStyle.Key, new List<WParagraphStyle>());
				foreach (WParagraphStyle item in tocStyle.Value)
				{
					tableOfContent.m_tocStyles[tocStyle.Key].Add((WParagraphStyle)item.Clone());
				}
			}
		}
		return tableOfContent;
	}

	internal override void Close()
	{
		base.Close();
		m_tocField = null;
		if (m_tocStyles != null)
		{
			foreach (List<WParagraphStyle> value in m_tocStyles.Values)
			{
				value.Clear();
			}
			m_tocStyles.Clear();
			m_tocStyles = null;
		}
		if (m_tocLevels == null)
		{
			return;
		}
		foreach (List<string> value2 in m_tocLevels.Values)
		{
			value2.Clear();
		}
		m_tocLevels.Clear();
		m_tocLevels = null;
	}

	internal bool Compare(TableOfContent tableOfContent)
	{
		if (tableOfContent == null)
		{
			return false;
		}
		if (base.SkipDocxItem != tableOfContent.SkipDocxItem || UseHeadingStyles != tableOfContent.UseHeadingStyles || UseTableEntryFields != tableOfContent.UseTableEntryFields || RightAlignPageNumbers != tableOfContent.RightAlignPageNumbers || IncludePageNumbers != tableOfContent.IncludePageNumbers || UseHyperlinks != tableOfContent.UseHyperlinks || UseOutlineLevels != tableOfContent.UseOutlineLevels || InvalidFormatString != tableOfContent.InvalidFormatString || FormattingParsed != tableOfContent.FormattingParsed || IncludeNewLineCharacters != tableOfContent.IncludeNewLineCharacters || UpperHeadingLevel != tableOfContent.UpperHeadingLevel || LowerHeadingLevel != tableOfContent.LowerHeadingLevel || TableID != tableOfContent.TableID || FormattingString != tableOfContent.FormattingString || EntityType != tableOfContent.EntityType)
		{
			return false;
		}
		if ((TOCField != null && tableOfContent.TOCField == null) || (tableOfContent.TOCField != null && TOCField == null))
		{
			return false;
		}
		if (base.ParaItemCharFormat != null && tableOfContent.ParaItemCharFormat != null && !base.ParaItemCharFormat.Compare(tableOfContent.ParaItemCharFormat))
		{
			return false;
		}
		return true;
	}

	internal StringBuilder GetAsString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(base.SkipDocxItem ? "1" : "0;");
		stringBuilder.Append(UseHeadingStyles ? "1" : "0;");
		stringBuilder.Append(UseTableEntryFields ? "1" : "0;");
		stringBuilder.Append(RightAlignPageNumbers ? "1" : "0;");
		stringBuilder.Append(IncludePageNumbers ? "1" : "0;");
		stringBuilder.Append(UseHyperlinks ? "1" : "0;");
		stringBuilder.Append(UseOutlineLevels ? "1" : "0;");
		stringBuilder.Append(InvalidFormatString ? "1" : "0;");
		stringBuilder.Append(FormattingParsed ? "1" : "0;");
		stringBuilder.Append(IncludeNewLineCharacters ? "1" : "0;");
		stringBuilder.Append(UpperHeadingLevel + ";");
		stringBuilder.Append(LowerHeadingLevel + ";");
		stringBuilder.Append(TableID + ";");
		stringBuilder.Append(FormattingString + ";");
		stringBuilder.Append((int)EntityType + ";");
		stringBuilder.Append(TOCField.GetAsString(traverseTillSeparator: false));
		return stringBuilder;
	}
}
