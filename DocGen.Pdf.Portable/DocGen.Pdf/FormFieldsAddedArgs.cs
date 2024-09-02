using System;
using DocGen.Pdf.Interactive;

namespace DocGen.Pdf;

internal class FormFieldsAddedArgs : EventArgs
{
	private PdfField m_FormField;

	private PdfListFieldItem m_Item;

	private PdfRadioButtonListItem m_Items;

	private int m_index = -1;

	private string m_methodName;

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

	internal string MethodName
	{
		get
		{
			return m_methodName;
		}
		set
		{
			m_methodName = value;
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

	internal PdfListFieldItem Item
	{
		get
		{
			return m_Item;
		}
		set
		{
			m_Item = value;
		}
	}

	internal PdfRadioButtonListItem Items
	{
		get
		{
			return m_Items;
		}
		set
		{
			m_Items = value;
		}
	}

	internal FormFieldsAddedArgs(PdfField form)
	{
		m_FormField = form;
	}
}
