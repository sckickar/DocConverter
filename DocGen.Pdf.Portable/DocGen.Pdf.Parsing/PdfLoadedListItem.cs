using System;
using DocGen.Pdf.IO;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Parsing;

public class PdfLoadedListItem
{
	private string m_text;

	private string m_value;

	private PdfLoadedChoiceField m_field;

	private PdfCrossTable m_crossTable;

	public string Text
	{
		get
		{
			return m_text;
		}
		set
		{
			AssignText(value);
		}
	}

	public string Value
	{
		get
		{
			string text = m_value;
			if (text == null)
			{
				text = Text;
			}
			return text;
		}
		set
		{
			AssignValue(value);
		}
	}

	internal PdfLoadedListItem(string text, string value, PdfLoadedChoiceField field, PdfCrossTable cTable)
		: this(text, value)
	{
		m_field = field;
		m_crossTable = cTable;
	}

	public PdfLoadedListItem(string text, string value)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		m_text = text;
		m_value = value;
	}

	private void AssignText(string value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("text");
		}
		if (!(m_text != value))
		{
			return;
		}
		PdfDictionary dictionary = m_field.Dictionary;
		if (!dictionary.ContainsKey("Opt"))
		{
			return;
		}
		PdfArray pdfArray = m_crossTable.GetObject(dictionary["Opt"]) as PdfArray;
		PdfArray pdfArray2 = new PdfArray();
		pdfArray2.Add(new PdfString(m_value));
		pdfArray2.Add(new PdfString(value));
		int i = 0;
		for (int count = pdfArray.Count; i < count; i++)
		{
			PdfArray pdfArray3 = m_crossTable.GetObject(pdfArray[i]) as PdfArray;
			if ((m_crossTable.GetObject(pdfArray3[1]) as PdfString).Value == m_text)
			{
				m_text = value;
				pdfArray.RemoveAt(i);
				pdfArray.Insert(i, pdfArray2);
			}
		}
		dictionary.SetProperty("Opt", pdfArray);
		m_field.Changed = true;
	}

	private void AssignValue(string value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (!(m_value != value))
		{
			return;
		}
		PdfDictionary dictionary = m_field.Dictionary;
		if (!dictionary.ContainsKey("Opt"))
		{
			return;
		}
		PdfArray pdfArray = m_crossTable.GetObject(dictionary["Opt"]) as PdfArray;
		PdfArray pdfArray2 = new PdfArray();
		pdfArray2.Add(new PdfString(value));
		pdfArray2.Add(new PdfString(m_text));
		int i = 0;
		for (int count = pdfArray.Count; i < count; i++)
		{
			PdfArray pdfArray3 = m_crossTable.GetObject(pdfArray[i]) as PdfArray;
			if ((m_crossTable.GetObject(pdfArray3[1]) as PdfString).Value == m_value)
			{
				m_value = value;
				pdfArray.RemoveAt(i);
				pdfArray.Insert(i, pdfArray2);
			}
		}
		dictionary.SetProperty("Opt", pdfArray);
		m_field.Changed = true;
	}
}
