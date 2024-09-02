namespace DocGen.DocIO.ODF.Base.ODFImplementation;

internal class PageLayoutProperties : MarginBorderProperties
{
	private double m_pageWidth;

	private PrintOrientation m_pageOrientation;

	private double m_pageHeight;

	private PageOrder m_printPageOrder;

	private string m_scaleTo;

	private TableCentering m_tableCentering;

	private string m_printableObjects;

	private string m_firstPageNumber;

	private double m_paddingLeft;

	private double m_paddingRight;

	private double m_paddingTop;

	private double m_paddingBottom;

	internal double PaddingBottom
	{
		get
		{
			return m_paddingBottom;
		}
		set
		{
			m_paddingBottom = value;
		}
	}

	internal double PaddingTop
	{
		get
		{
			return m_paddingTop;
		}
		set
		{
			m_paddingTop = value;
		}
	}

	internal double PaddingRight
	{
		get
		{
			return m_paddingRight;
		}
		set
		{
			m_paddingRight = value;
		}
	}

	internal double PaddingLeft
	{
		get
		{
			return m_paddingLeft;
		}
		set
		{
			m_paddingLeft = value;
		}
	}

	internal double PageWidth
	{
		get
		{
			return m_pageWidth;
		}
		set
		{
			m_pageWidth = value;
		}
	}

	internal double PageHeight
	{
		get
		{
			return m_pageHeight;
		}
		set
		{
			m_pageHeight = value;
		}
	}

	internal PrintOrientation PageOrientation
	{
		get
		{
			return m_pageOrientation;
		}
		set
		{
			m_pageOrientation = value;
		}
	}

	internal PageOrder PrintPageOrder
	{
		get
		{
			return m_printPageOrder;
		}
		set
		{
			m_printPageOrder = value;
		}
	}

	internal string ScaleTo
	{
		get
		{
			return m_scaleTo;
		}
		set
		{
			m_scaleTo = value;
		}
	}

	internal TableCentering TableCentering
	{
		get
		{
			return m_tableCentering;
		}
		set
		{
			m_tableCentering = value;
		}
	}

	internal string PrintableObjects
	{
		get
		{
			return m_printableObjects;
		}
		set
		{
			m_printableObjects = value;
		}
	}

	internal string FirstPageNumber
	{
		get
		{
			return m_firstPageNumber;
		}
		set
		{
			m_firstPageNumber = value;
		}
	}
}
