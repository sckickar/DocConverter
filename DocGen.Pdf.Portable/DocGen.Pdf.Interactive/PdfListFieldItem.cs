using System;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public class PdfListFieldItem : IPdfWrapper
{
	private int c_textIndex = 1;

	private int c_valueIndex;

	private string m_text = string.Empty;

	internal PdfField m_field;

	internal int m_index = -1;

	private string m_value = string.Empty;

	private PdfArray m_array = new PdfArray();

	public string Text
	{
		get
		{
			return m_text;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("Text");
			}
			if (m_text != value)
			{
				m_text = value;
				((PdfString)m_array[c_textIndex]).Value = m_text;
			}
			if (m_field != null)
			{
				m_field.NotifyPropertyChanged("Text", m_index);
			}
		}
	}

	public string Value
	{
		get
		{
			return m_value;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("Value");
			}
			if (m_value != value)
			{
				m_value = value;
				((PdfString)m_array[c_valueIndex]).Value = m_value;
			}
			if (m_field != null)
			{
				m_field.NotifyPropertyChanged("Value", m_index);
			}
		}
	}

	IPdfPrimitive IPdfWrapper.Element => m_array;

	public PdfListFieldItem()
	{
		Initialize(m_text, m_value);
	}

	public PdfListFieldItem(string text, string value)
	{
		Initialize(text, value);
	}

	private void Initialize(string text, string value)
	{
		if (c_valueIndex < c_textIndex)
		{
			m_array.Add(new PdfString(value));
			m_array.Add(new PdfString(text));
		}
		else
		{
			m_array.Add(new PdfString(text));
			m_array.Add(new PdfString(value));
		}
		m_text = text;
		m_value = value;
	}
}
