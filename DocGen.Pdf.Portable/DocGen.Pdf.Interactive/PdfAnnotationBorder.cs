using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public class PdfAnnotationBorder : IPdfWrapper
{
	private float m_horizontalRadius;

	private float m_verticalRadius;

	private float m_borderWidth = 1f;

	private PdfArray m_array = new PdfArray();

	public float HorizontalRadius
	{
		get
		{
			return m_horizontalRadius;
		}
		set
		{
			if (m_horizontalRadius != value)
			{
				m_horizontalRadius = value;
				SetNumber(0, value);
			}
		}
	}

	public float VerticalRadius
	{
		get
		{
			return m_verticalRadius;
		}
		set
		{
			if (m_verticalRadius != value)
			{
				m_verticalRadius = value;
				SetNumber(1, value);
			}
		}
	}

	public float Width
	{
		get
		{
			return m_borderWidth;
		}
		set
		{
			if (m_borderWidth != value)
			{
				m_borderWidth = value;
				SetNumber(2, value);
			}
		}
	}

	IPdfPrimitive IPdfWrapper.Element => m_array;

	public PdfAnnotationBorder()
	{
		Initialize(m_borderWidth, m_horizontalRadius, m_verticalRadius);
	}

	public PdfAnnotationBorder(float borderWidth)
	{
		Initialize(borderWidth, m_horizontalRadius, m_verticalRadius);
		Width = borderWidth;
	}

	public PdfAnnotationBorder(float borderWidth, float horizontalRadius, float verticalRadius)
	{
		Initialize(borderWidth, horizontalRadius, verticalRadius);
	}

	private void Initialize(float borderWidth, float horizontalRadius, float verticalRadius)
	{
		m_array.Add(new PdfNumber(horizontalRadius), new PdfNumber(verticalRadius), new PdfNumber(borderWidth));
	}

	private void SetNumber(int index, float value)
	{
		(m_array[index] as PdfNumber).FloatValue = value;
	}
}
