using DocGen.OfficeChart.Implementation.Collections;

namespace DocGen.OfficeChart.Implementation.XmlReaders;

internal class DxfImpl
{
	private BordersCollection m_borders;

	private FillImpl m_fill;

	private FontImpl m_font;

	private FormatImpl m_format;

	public FormatImpl FormatRecord
	{
		get
		{
			return m_format;
		}
		set
		{
			m_format = value;
		}
	}

	public FillImpl Fill
	{
		get
		{
			return m_fill;
		}
		set
		{
			m_fill = value;
		}
	}

	public FontImpl Font
	{
		get
		{
			return m_font;
		}
		set
		{
			m_font = value;
		}
	}

	public BordersCollection Borders
	{
		get
		{
			return m_borders;
		}
		set
		{
			m_borders = value;
		}
	}

	public DxfImpl Clone(WorkbookImpl book)
	{
		DxfImpl obj = (DxfImpl)MemberwiseClone();
		obj.m_borders = (BordersCollection)m_borders.Clone(book);
		obj.m_fill = m_fill.Clone();
		obj.m_font = m_font.Clone(book);
		obj.m_format = (FormatImpl)m_format.Clone(book);
		return obj;
	}
}
