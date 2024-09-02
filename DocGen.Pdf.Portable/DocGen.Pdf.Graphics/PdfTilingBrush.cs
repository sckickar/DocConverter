using DocGen.Drawing;
using DocGen.Pdf.IO;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Graphics;

public class PdfTilingBrush : PdfBrush, IPdfWrapper
{
	private RectangleF m_box;

	private PdfGraphics m_graphics;

	private PdfStream m_brushStream;

	private PdfResources m_resources;

	private bool m_bStroking;

	private PdfPage m_page;

	private PointF m_location;

	private PdfTransformationMatrix m_transformationMatrix;

	internal bool isXPSBrush;

	private float m_outerSize;

	internal PointF Location
	{
		get
		{
			return m_location;
		}
		set
		{
			m_location = value;
		}
	}

	internal PdfTransformationMatrix TransformationMatrix
	{
		get
		{
			return m_transformationMatrix;
		}
		set
		{
			m_transformationMatrix = value;
		}
	}

	public RectangleF Rectangle => m_box;

	public SizeF Size => m_box.Size;

	public PdfGraphics Graphics
	{
		get
		{
			if (m_graphics == null)
			{
				m_graphics = new PdfGraphics(Size, ObtainResources, m_brushStream);
				m_graphics.InitializeCoordinates();
			}
			return m_graphics;
		}
	}

	internal PdfResources Resources => m_resources;

	internal bool Stroking
	{
		get
		{
			return m_bStroking;
		}
		set
		{
			m_bStroking = value;
		}
	}

	IPdfPrimitive IPdfWrapper.Element => m_brushStream;

	public PdfTilingBrush(RectangleF rectangle)
	{
		m_brushStream = new PdfStream();
		m_resources = new PdfResources();
		m_brushStream["Resources"] = m_resources;
		SetBox(rectangle);
		SetObligatoryFields();
	}

	public PdfTilingBrush(SizeF size, float outerSize)
		: this(new RectangleF(PointF.Empty, size), outerSize)
	{
	}

	public PdfTilingBrush(RectangleF rectangle, float outerSize)
	{
		m_outerSize = outerSize;
		m_brushStream = new PdfStream();
		m_resources = new PdfResources();
		m_brushStream["Resources"] = m_resources;
		SetBox(rectangle);
		SetObligatoryFields();
	}

	public PdfTilingBrush(RectangleF rectangle, PdfPage page)
	{
		m_page = page;
		m_brushStream = new PdfStream();
		m_resources = new PdfResources();
		m_brushStream["Resources"] = m_resources;
		SetBox(rectangle);
		SetObligatoryFields();
		Graphics.ColorSpace = page.Document.ColorSpace;
	}

	public PdfTilingBrush(SizeF size)
		: this(new RectangleF(PointF.Empty, size))
	{
	}

	public PdfTilingBrush(SizeF size, PdfPage page)
		: this(new RectangleF(PointF.Empty, size), page)
	{
	}

	internal PdfTilingBrush(RectangleF rectangle, float outerSize, PdfPage page, PointF location, PdfTransformationMatrix matrix)
	{
		m_outerSize = outerSize;
		m_page = page;
		m_location = location;
		m_transformationMatrix = matrix;
		m_brushStream = new PdfStream();
		m_resources = new PdfResources();
		m_brushStream["Resources"] = m_resources;
		SetBox(rectangle);
		SetObligatoryFields();
	}

	private void SetObligatoryFields()
	{
		m_brushStream["PatternType"] = new PdfNumber(1);
		m_brushStream["PaintType"] = new PdfNumber(1);
		m_brushStream["TilingType"] = new PdfNumber(1);
		m_brushStream["XStep"] = new PdfNumber(m_box.Right - m_box.Left);
		m_brushStream["YStep"] = new PdfNumber(m_box.Bottom - m_box.Top);
		if (m_page != null || m_outerSize != 0f)
		{
			float num = ((m_page != null) ? m_page.Size.Height : m_outerSize);
			if (m_transformationMatrix == null)
			{
				float num2 = num % Rectangle.Size.Height - Location.Y;
				m_brushStream["Matrix"] = new PdfArray(new float[6] { 1f, 0f, 0f, 1f, m_location.X, num2 });
			}
			else
			{
				float[] elements = m_transformationMatrix.Matrix.Elements;
				float num3 = ((!(num > Rectangle.Size.Height)) ? (num % Rectangle.Size.Height + m_transformationMatrix.OffsetY) : (m_transformationMatrix.OffsetY - num % Rectangle.Size.Height));
				m_brushStream["Matrix"] = new PdfArray(new float[6]
				{
					elements[0],
					elements[1],
					elements[2],
					elements[3],
					elements[4],
					num3
				});
			}
		}
	}

	private void SetBox(RectangleF box)
	{
		m_box = box;
		m_brushStream["BBox"] = PdfArray.FromRectangle(m_box);
	}

	private PdfResources ObtainResources()
	{
		return Resources;
	}

	public override PdfBrush Clone()
	{
		PdfTilingBrush pdfTilingBrush = new PdfTilingBrush(Rectangle, m_outerSize, m_page, Location, m_transformationMatrix);
		if (isXPSBrush && m_transformationMatrix != null && m_transformationMatrix.Matrix != null)
		{
			pdfTilingBrush.m_brushStream["Matrix"] = new PdfArray(m_transformationMatrix.Matrix.Elements);
		}
		pdfTilingBrush.m_brushStream.Data = m_brushStream.Data;
		pdfTilingBrush.m_resources = new PdfResources(m_resources);
		pdfTilingBrush.m_brushStream["Resources"] = pdfTilingBrush.m_resources;
		return pdfTilingBrush;
	}

	internal override bool MonitorChanges(PdfBrush brush, PdfStreamWriter streamWriter, PdfGraphics.GetResources getResources, bool saveChanges, PdfColorSpace currentColorSpace)
	{
		bool result = false;
		if (brush != this)
		{
			streamWriter.SetColorSpace("Pattern", m_bStroking);
			PdfName name = getResources().GetName(this);
			streamWriter.SetColourWithPattern(null, name, m_bStroking);
			result = true;
		}
		return result;
	}

	internal override bool MonitorChanges(PdfBrush brush, PdfStreamWriter streamWriter, PdfGraphics.GetResources getResources, bool saveChanges, PdfColorSpace currentColorSpace, bool check)
	{
		bool result = false;
		if (brush != this)
		{
			streamWriter.SetColorSpace("Pattern", m_bStroking);
			PdfName name = getResources().GetName(this);
			streamWriter.SetColourWithPattern(null, name, m_bStroking);
			result = true;
		}
		return result;
	}

	internal override bool MonitorChanges(PdfBrush brush, PdfStreamWriter streamWriter, PdfGraphics.GetResources getResources, bool saveChanges, PdfColorSpace currentColorSpace, bool check, bool iccbased)
	{
		bool result = false;
		if (brush != this)
		{
			streamWriter.SetColorSpace("Pattern", m_bStroking);
			PdfName name = getResources().GetName(this);
			streamWriter.SetColourWithPattern(null, name, m_bStroking);
			result = true;
		}
		return result;
	}

	internal override bool MonitorChanges(PdfBrush brush, PdfStreamWriter streamWriter, PdfGraphics.GetResources getResources, bool saveChanges, PdfColorSpace currentColorSpace, bool check, bool iccbased, bool indexed)
	{
		bool result = false;
		if (brush != this)
		{
			streamWriter.SetColorSpace("Pattern", m_bStroking);
			PdfName name = getResources().GetName(this);
			streamWriter.SetColourWithPattern(null, name, m_bStroking);
			result = true;
		}
		return result;
	}

	internal override void ResetChanges(PdfStreamWriter streamWriter)
	{
	}
}
