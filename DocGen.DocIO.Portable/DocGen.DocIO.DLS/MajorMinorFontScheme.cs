using System.Collections.Generic;

namespace DocGen.DocIO.DLS;

internal class MajorMinorFontScheme
{
	private Dictionary<string, string> m_fontTypefaces;

	private FontSchemeStruct m_fontSchemeStruct;

	private List<FontSchemeStruct> m_fontSchemeList;

	internal Dictionary<string, string> FontTypeface
	{
		get
		{
			return m_fontTypefaces;
		}
		set
		{
			m_fontTypefaces = new Dictionary<string, string>();
		}
	}

	internal List<FontSchemeStruct> FontSchemeList
	{
		get
		{
			return m_fontSchemeList;
		}
		set
		{
			m_fontSchemeList = new List<FontSchemeStruct>();
		}
	}

	internal FontSchemeStruct FontSchemeStructure
	{
		get
		{
			return m_fontSchemeStruct;
		}
		set
		{
			m_fontSchemeStruct = default(FontSchemeStruct);
		}
	}

	public MajorMinorFontScheme()
	{
		m_fontSchemeStruct = default(FontSchemeStruct);
		m_fontTypefaces = new Dictionary<string, string>();
		m_fontSchemeList = new List<FontSchemeStruct>();
	}

	internal void Close()
	{
		if (m_fontTypefaces != null)
		{
			m_fontTypefaces.Clear();
			m_fontTypefaces = null;
		}
		if (m_fontSchemeList != null)
		{
			m_fontSchemeList.Clear();
			m_fontSchemeList = null;
		}
	}
}
