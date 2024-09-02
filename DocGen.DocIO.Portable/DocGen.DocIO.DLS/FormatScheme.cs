using System.Collections.Generic;

namespace DocGen.DocIO.DLS;

internal class FormatScheme
{
	private string m_fmtSchemeName;

	private List<FillFormat> m_bgFillFormats;

	private List<FillFormat> m_fillFormats;

	private List<LineFormat> m_lnStyleList;

	private List<EffectFormat> m_effectList;

	internal string FmtName
	{
		get
		{
			return m_fmtSchemeName;
		}
		set
		{
			m_fmtSchemeName = value;
		}
	}

	internal List<FillFormat> BgFillFormats
	{
		get
		{
			return m_bgFillFormats;
		}
		set
		{
			m_bgFillFormats = new List<FillFormat>();
		}
	}

	internal List<FillFormat> FillFormats
	{
		get
		{
			return m_fillFormats;
		}
		set
		{
			m_fillFormats = new List<FillFormat>();
		}
	}

	internal List<LineFormat> LnStyleScheme
	{
		get
		{
			return m_lnStyleList;
		}
		set
		{
			m_lnStyleList = new List<LineFormat>();
		}
	}

	internal List<EffectFormat> EffectStyles
	{
		get
		{
			if (m_effectList == null)
			{
				m_effectList = new List<EffectFormat>();
			}
			return m_effectList;
		}
		set
		{
			m_effectList = value;
		}
	}

	internal FormatScheme()
	{
		m_fillFormats = new List<FillFormat>();
		m_bgFillFormats = new List<FillFormat>();
		m_lnStyleList = new List<LineFormat>();
		m_effectList = new List<EffectFormat>();
	}

	internal void Close()
	{
		if (m_bgFillFormats != null)
		{
			foreach (FillFormat bgFillFormat in m_bgFillFormats)
			{
				bgFillFormat.Close();
			}
			m_bgFillFormats.Clear();
			m_bgFillFormats = null;
		}
		if (m_fillFormats != null)
		{
			foreach (FillFormat fillFormat in m_fillFormats)
			{
				fillFormat.Close();
			}
			m_fillFormats.Clear();
			m_fillFormats = null;
		}
		if (m_effectList != null)
		{
			foreach (EffectFormat effect in m_effectList)
			{
				effect.Close();
			}
			m_effectList.Clear();
			m_effectList = null;
		}
		if (m_lnStyleList == null)
		{
			return;
		}
		foreach (LineFormat lnStyle in m_lnStyleList)
		{
			lnStyle.Close();
		}
		m_lnStyleList.Clear();
		m_lnStyleList = null;
	}
}
