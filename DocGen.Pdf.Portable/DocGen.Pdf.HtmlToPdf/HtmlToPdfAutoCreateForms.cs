using DocGen.Drawing;

namespace DocGen.Pdf.HtmlToPdf;

internal class HtmlToPdfAutoCreateForms
{
	private string m_elementId;

	private string m_elementValue;

	private bool m_isReadOnly;

	private bool m_isSelected;

	private string m_elementType;

	private int m_elementPageNo;

	private RectangleF m_elementBounds;

	private string m_optionValue;

	internal string OptionValue
	{
		get
		{
			return m_optionValue;
		}
		set
		{
			m_optionValue = value;
		}
	}

	internal string ElementId
	{
		get
		{
			return m_elementId;
		}
		set
		{
			m_elementId = value;
		}
	}

	internal string ElementValue
	{
		get
		{
			return m_elementValue;
		}
		set
		{
			m_elementValue = value;
		}
	}

	internal bool IsReadOnly
	{
		get
		{
			return m_isReadOnly;
		}
		set
		{
			m_isReadOnly = value;
		}
	}

	internal bool IsSelected
	{
		get
		{
			return m_isSelected;
		}
		set
		{
			m_isSelected = value;
		}
	}

	internal string ElementType
	{
		get
		{
			return m_elementType;
		}
		set
		{
			m_elementType = value;
		}
	}

	internal int ElementPageNo
	{
		get
		{
			return m_elementPageNo;
		}
		set
		{
			m_elementPageNo = value;
		}
	}

	internal RectangleF ElementBounds
	{
		get
		{
			return m_elementBounds;
		}
		set
		{
			m_elementBounds = value;
		}
	}

	internal HtmlToPdfAutoCreateForms(string id, string value, bool isReadonly, bool selected, string type, int pageNo, RectangleF bounds, string optionValue)
	{
		m_elementId = id;
		m_elementValue = value;
		m_isReadOnly = isReadonly;
		m_isSelected = selected;
		m_elementType = type;
		m_elementPageNo = pageNo;
		m_elementBounds = bounds;
		m_optionValue = optionValue;
	}
}
