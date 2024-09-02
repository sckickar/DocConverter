using System;
using System.Collections.Generic;
using DocGen.DocIO.ReaderWriter.Biff_Records;

namespace DocGen.DocIO.ReaderWriter;

[CLSCompliant(false)]
internal class WordStyleSheet
{
	internal const string DEF_FONT_NAME = "Times New Roman";

	private const string DEF_NORMAL_STYLE = "Normal";

	private const string DEF_DPF_STYLE = "Default Paragraph Font";

	private const string DEF_LIST_FONT_NAME = "Wingdings";

	private const int DEF_STDCOUNT = 15;

	internal bool IsFixedIndex13HasStyle;

	internal bool IsFixedIndex14HasStyle;

	internal string FixedIndex13StyleName = string.Empty;

	internal string FixedIndex14StyleName = string.Empty;

	private List<string> m_fontNameList = new List<string>();

	private Dictionary<string, int> m_fontNames = new Dictionary<string, int>();

	private string[] m_defFontNames = new string[6] { "Times New Roman", "Symbol", "Arial", "Verdana", "Wingdings", "Courier New" };

	private List<WordStyle> m_styleList = new List<WordStyle>();

	private int m_defStyleIndex;

	private Dictionary<string, string> m_fontSubstitutionTable;

	private Dictionary<int, string> m_styleNames;

	internal Dictionary<int, string> StyleNames
	{
		get
		{
			if (m_styleNames == null)
			{
				m_styleNames = new Dictionary<int, string>();
			}
			return m_styleNames;
		}
	}

	internal Dictionary<string, string> FontSubstitutionTable
	{
		get
		{
			if (m_fontSubstitutionTable == null)
			{
				m_fontSubstitutionTable = new Dictionary<string, string>();
			}
			return m_fontSubstitutionTable;
		}
		set
		{
			m_fontSubstitutionTable = value;
		}
	}

	internal List<string> FontNamesList => m_fontNameList;

	internal int DefaultStyleIndex => m_defStyleIndex;

	internal int StylesCount => m_styleList.Count;

	internal WordStyleSheet()
	{
		WordStyle wordStyle = new WordStyle(this, "Normal")
		{
			ID = 0
		};
		m_defStyleIndex = AddStyle(wordStyle);
		UpdateFontNames(m_defFontNames);
		wordStyle.CHPX.PropertyModifiers.SetUShortValue(19023, 0);
		for (int i = 0; i < 14; i++)
		{
			AddEmptyStyle();
		}
	}

	internal WordStyleSheet(bool createDefCharStyle)
	{
		m_defStyleIndex = AddStyle(new WordStyle(this, "Normal")
		{
			ID = 0
		});
		UpdateFontNames(m_defFontNames);
		if (createDefCharStyle)
		{
			for (int i = 1; i < 10; i++)
			{
				AddEmptyStyle();
			}
			AddStyle(new WordStyle(this, "Default Paragraph Font")
			{
				ID = 65,
				IsCharacterStyle = true,
				BaseStyleIndex = 4095,
				HasUpe = true
			});
			for (int j = 11; j < 15; j++)
			{
				AddEmptyStyle();
			}
		}
		else
		{
			for (int k = 0; k < 14; k++)
			{
				AddEmptyStyle();
			}
		}
	}

	internal WordStyle CreateStyle(string name)
	{
		return CreateStyle(name, characterStyle: false);
	}

	internal WordStyle CreateStyle(string name, bool characterStyle)
	{
		WordStyle wordStyle = new WordStyle(this, name, characterStyle);
		AddStyle(wordStyle);
		return wordStyle;
	}

	internal WordStyle CreateStyle(string name, int index)
	{
		if (index < 15)
		{
			throw new ArgumentOutOfRangeException("index must be greater than 14");
		}
		ValidateNameParameter(name, index);
		while (StylesCount < index)
		{
			AddEmptyStyle();
		}
		WordStyle wordStyle = new WordStyle(this, name);
		AddStyle(wordStyle);
		return wordStyle;
	}

	internal int AddStyle(WordStyle style)
	{
		if (style == null)
		{
			throw new ArgumentNullException("style");
		}
		m_styleList.Add(style);
		return m_styleList.Count - 1;
	}

	internal int AddEmptyStyle()
	{
		int count = m_styleList.Count;
		m_styleList.Add(WordStyle.Empty);
		return count;
	}

	internal int StyleNameToIndex(string name, bool isCharacter)
	{
		if (string.IsNullOrEmpty(name))
		{
			throw new ArgumentNullException("Style name can't be null or empty");
		}
		int i = 0;
		for (int count = m_styleList.Count; i < count; i++)
		{
			WordStyle wordStyle = m_styleList[i];
			if (wordStyle.Name == name && wordStyle.IsCharacterStyle == isCharacter)
			{
				return i;
			}
		}
		return -1;
	}

	internal int StyleNameToIndex(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			throw new ArgumentNullException("Style name can't be null or empty");
		}
		int i = 0;
		for (int count = m_styleList.Count; i < count; i++)
		{
			if (m_styleList[i].Name == name)
			{
				return i;
			}
		}
		return -1;
	}

	internal int FontNameToIndex(string name)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (m_fontNames.ContainsKey(name))
		{
			return m_fontNames[name];
		}
		return -1;
	}

	internal WordStyle GetStyleByIndex(int index)
	{
		if (index < 0 || index > m_styleList.Count - 1)
		{
			index = 0;
		}
		return m_styleList[index];
	}

	internal WordStyle UpdateStyle(int index, string name)
	{
		if (index < 0 || index > StylesCount - 1)
		{
			throw new ArgumentOutOfRangeException($"Index should be between 0 and {StylesCount - 1}");
		}
		ValidateNameParameter(name, index);
		WordStyle wordStyle = GetStyleByIndex(index);
		if (wordStyle == WordStyle.Empty)
		{
			wordStyle = (m_styleList[index] = new WordStyle(this, name));
		}
		else
		{
			wordStyle.UpdateName(name);
		}
		return wordStyle;
	}

	internal void RemoveStyleByIndex(int index)
	{
		m_styleList.RemoveAt(index);
	}

	internal void InsertStyle(int index, WordStyle style)
	{
		m_styleList.Insert(index, style);
	}

	internal void UpdateFontSubstitutionTable(FontFamilyNameRecord ffnRecord)
	{
		if (ffnRecord.AlternativeFontName != null && ffnRecord.AlternativeFontName != string.Empty)
		{
			if (!FontSubstitutionTable.ContainsKey(ffnRecord.FontName))
			{
				m_fontSubstitutionTable.Add(ffnRecord.FontName, ffnRecord.AlternativeFontName);
			}
			else
			{
				FontSubstitutionTable[ffnRecord.FontName] = ffnRecord.AlternativeFontName;
			}
		}
	}

	internal void UpdateFontName(string name)
	{
		UpdateFontNames(new string[1] { name });
	}

	internal void UpdateFontNames(string[] names)
	{
		m_fontNameList.AddRange(names);
		for (int i = 0; i < names.Length; i++)
		{
			try
			{
				m_fontNames.Add(names[i], m_fontNames.Count);
			}
			catch (ArgumentNullException)
			{
			}
			catch (ArgumentException)
			{
			}
		}
	}

	internal void ClearFontNames()
	{
		m_fontNameList.Clear();
		m_fontNames.Clear();
	}

	internal void Close()
	{
		if (m_fontNameList != null)
		{
			m_fontNameList.Clear();
			m_fontNameList = null;
		}
		if (m_fontNames != null)
		{
			m_fontNames.Clear();
			m_fontNames = null;
		}
		m_defFontNames = null;
		if (m_styleList != null)
		{
			m_styleList.Clear();
			m_styleList = null;
		}
		if (m_fontSubstitutionTable != null)
		{
			m_fontSubstitutionTable.Clear();
			m_fontSubstitutionTable = null;
		}
		if (m_styleNames != null)
		{
			m_styleNames.Clear();
			m_styleNames = null;
		}
	}

	private void ValidateNameParameter(string name, int withoutIndex)
	{
		for (int i = 0; i < m_styleList.Count; i++)
		{
			_ = m_styleList[i];
		}
	}
}
