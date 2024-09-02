using System;
using DocGen.Pdf.Graphics;

namespace DocGen.Pdf;

public class PdfCompositeField : PdfMultipleValueField
{
	private PdfAutomaticField[] m_automaticFields;

	private string m_text = string.Empty;

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
			m_text = value;
		}
	}

	public PdfAutomaticField[] AutomaticFields
	{
		get
		{
			return m_automaticFields;
		}
		set
		{
			m_automaticFields = value;
		}
	}

	public PdfCompositeField()
	{
	}

	public PdfCompositeField(PdfFont font)
		: base(font)
	{
	}

	public PdfCompositeField(PdfFont font, PdfBrush brush)
		: base(font, brush)
	{
	}

	public PdfCompositeField(PdfFont font, string text)
		: base(font)
	{
		Text = text;
	}

	public PdfCompositeField(PdfFont font, PdfBrush brush, string text)
		: base(font, brush)
	{
		Text = text;
	}

	public PdfCompositeField(string text, params PdfAutomaticField[] list)
	{
		m_automaticFields = list;
		Text = text;
	}

	public PdfCompositeField(PdfFont font, string text, params PdfAutomaticField[] list)
		: base(font)
	{
		Text = text;
		m_automaticFields = list;
	}

	public PdfCompositeField(PdfFont font, PdfBrush brush, string text, params PdfAutomaticField[] list)
		: base(font, brush)
	{
		Text = text;
		m_automaticFields = list;
	}

	protected internal override string GetValue(PdfGraphics graphics)
	{
		string[] array = null;
		if (m_automaticFields != null && m_automaticFields.Length != 0)
		{
			array = new string[m_automaticFields.Length];
			int num = 0;
			PdfAutomaticField[] automaticFields = m_automaticFields;
			foreach (PdfAutomaticField pdfAutomaticField in automaticFields)
			{
				array[num++] = pdfAutomaticField.GetValue(graphics);
			}
			string text = m_text;
			object[] args = array;
			return string.Format(text, args);
		}
		return m_text;
	}
}
