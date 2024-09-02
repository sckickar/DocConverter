namespace DocGen.DocIO.ODF.Base.ODFImplementation;

internal class OPicture : OParagraphItem
{
	private float m_widthScale = 100f;

	private float m_heightScale = 100f;

	private float m_height;

	private float m_width;

	private string m_name;

	private float m_horizontalPosition;

	private float m_verticalPosition;

	private int m_orderIndex;

	private int m_spid;

	private string m_oPictureHRef;

	private TextWrappingStyle m_wrappingStyle;

	internal float Height
	{
		get
		{
			return m_height;
		}
		set
		{
			m_height = value;
		}
	}

	internal float Width
	{
		get
		{
			return m_width;
		}
		set
		{
			m_width = value;
		}
	}

	internal float HeightScale
	{
		get
		{
			return m_heightScale;
		}
		set
		{
			m_heightScale = value;
		}
	}

	internal float WidthScale
	{
		get
		{
			return m_widthScale;
		}
		set
		{
			m_widthScale = value;
		}
	}

	public string Name
	{
		get
		{
			return m_name;
		}
		set
		{
			m_name = value;
		}
	}

	internal float HorizontalPosition
	{
		get
		{
			return m_horizontalPosition;
		}
		set
		{
			m_horizontalPosition = value;
		}
	}

	internal float VerticalPosition
	{
		get
		{
			return m_verticalPosition;
		}
		set
		{
			m_verticalPosition = value;
		}
	}

	internal int OrderIndex
	{
		get
		{
			return m_orderIndex;
		}
		set
		{
			m_orderIndex = value;
		}
	}

	internal int ShapeId
	{
		get
		{
			return m_spid;
		}
		set
		{
			m_spid = value;
		}
	}

	internal string OPictureHRef
	{
		get
		{
			return m_oPictureHRef;
		}
		set
		{
			m_oPictureHRef = value;
		}
	}

	internal TextWrappingStyle TextWrappingStyle
	{
		get
		{
			return m_wrappingStyle;
		}
		set
		{
			m_wrappingStyle = value;
		}
	}

	internal OPicture()
	{
	}

	internal void SetWidthScaleValue(float value)
	{
		m_widthScale = value;
	}

	internal void SetHeightScaleValue(float value)
	{
		m_heightScale = value;
	}
}
