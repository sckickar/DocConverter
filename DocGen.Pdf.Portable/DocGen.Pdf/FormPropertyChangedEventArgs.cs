using System;
using DocGen.Pdf.Interactive;

namespace DocGen.Pdf;

internal class FormPropertyChangedEventArgs : EventArgs
{
	private string m_propertyName;

	private PdfField m_FormField;

	private int m_index = -1;

	internal PdfField Field
	{
		get
		{
			return m_FormField;
		}
		set
		{
			m_FormField = value;
		}
	}

	internal string PropertyName
	{
		get
		{
			return m_propertyName;
		}
		set
		{
			m_propertyName = value;
		}
	}

	internal int Index
	{
		get
		{
			return m_index;
		}
		set
		{
			m_index = value;
		}
	}

	internal FormPropertyChangedEventArgs(PdfField form)
	{
		m_FormField = form;
	}
}
