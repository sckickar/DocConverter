using System;
using System.IO;

namespace DocGen.DocIO.DLS;

public class AddDictionaryEventArgs : EventArgs
{
	private string m_LanguageCode;

	private Stream m_DictionaryStream;

	public string LanguageCode => m_LanguageCode;

	public Stream DictionaryStream
	{
		get
		{
			return m_DictionaryStream;
		}
		set
		{
			m_DictionaryStream = value;
		}
	}

	internal AddDictionaryEventArgs(string orignalLanguagecode, string alternateLanguagecode)
	{
		m_LanguageCode = orignalLanguagecode;
	}
}
