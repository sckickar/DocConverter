using System;

namespace DocGen.DocIO.DLS;

public class Hyphenation
{
	private IWordDocument m_document;

	public bool AutoHyphenation
	{
		get
		{
			return (m_document as WordDocument).DOP.AutoHyphen;
		}
		set
		{
			(m_document as WordDocument).DOP.AutoHyphen = value;
		}
	}

	public bool HyphenateCaps
	{
		get
		{
			return (m_document as WordDocument).DOP.HyphCapitals;
		}
		set
		{
			(m_document as WordDocument).DOP.HyphCapitals = value;
		}
	}

	public float HyphenationZone
	{
		get
		{
			return (m_document as WordDocument).DOP.DxaHotZ / 20;
		}
		set
		{
			if ((double)value < 0.05 || value > 1584f)
			{
				throw new ArgumentOutOfRangeException("Hyphenation zone must be between 0.05 pt and 1584 pt.");
			}
			(m_document as WordDocument).DOP.DxaHotZ = (int)(value * 20f);
		}
	}

	public int ConsecutiveHyphensLimit
	{
		get
		{
			return (m_document as WordDocument).DOP.ConsecHypLim;
		}
		set
		{
			if (value < 0 || value > 32767)
			{
				throw new ArgumentOutOfRangeException("Consecutive hyphens limit must be between 0 and 32767.");
			}
			(m_document as WordDocument).DOP.ConsecHypLim = value;
		}
	}

	internal Hyphenation(IWordDocument document)
	{
		m_document = document;
	}

	internal void Close()
	{
		m_document = null;
	}
}
