using System;
using System.IO;
using DocGen.Drawing;

namespace DocGen.DocIO.DLS;

public class SubstituteFontEventArgs : EventArgs
{
	private string m_originalFontName;

	private string m_alternateFontName;

	private Stream m_alternateFontStream;

	private FontStyle m_fontStyle;

	[Obsolete("This property has been deprecated. Use the OriginalFontName property of SubstituteFontEventArgs class to get the original font name which need to be substituted.")]
	public string OrignalFontName
	{
		get
		{
			return m_originalFontName;
		}
		internal set
		{
			m_originalFontName = value;
		}
	}

	public string OriginalFontName
	{
		get
		{
			return m_originalFontName;
		}
		internal set
		{
			m_originalFontName = value;
		}
	}

	public string AlternateFontName
	{
		get
		{
			return m_alternateFontName;
		}
		set
		{
			m_alternateFontName = value;
		}
	}

	public FontStyle FontStyle
	{
		get
		{
			return m_fontStyle;
		}
		internal set
		{
			m_fontStyle = value;
		}
	}

	public Stream AlternateFontStream
	{
		get
		{
			return m_alternateFontStream;
		}
		set
		{
			m_alternateFontStream = value;
		}
	}

	internal SubstituteFontEventArgs(string orignalFontName, string alternateFontName, FontStyle fontStyle)
	{
		OrignalFontName = orignalFontName;
		AlternateFontName = alternateFontName;
		OriginalFontName = orignalFontName;
		FontStyle = fontStyle;
	}
}
