using System;
using System.IO;
using DocGen.Drawing;
using DocGen.Pdf.Primitives;
using DocGen.Pdf.Xmp;

namespace DocGen.Pdf.Graphics;

public abstract class PdfImage : PdfShapeElement, IPdfWrapper
{
	private float m_scrollBarWidth;

	private float m_scrollBarHeight;

	private XmpMetadata m_imageMetadata;

	private bool m_enableMetadata;

	public virtual int Height { get; internal set; }

	public virtual int Width { get; internal set; }

	public virtual float HorizontalResolution { get; internal set; }

	public virtual float VerticalResolution { get; internal set; }

	public virtual SizeF PhysicalDimension => new SizeF(Width, Height);

	internal float JpegOrientationAngle { get; set; }

	internal PdfStream ImageStream { get; set; }

	internal bool EnableMetada
	{
		get
		{
			return m_enableMetadata;
		}
		set
		{
			m_enableMetadata = value;
		}
	}

	internal float ScrollBarWidth
	{
		get
		{
			return m_scrollBarWidth;
		}
		set
		{
			m_scrollBarWidth = value;
		}
	}

	internal float ScrollBarHeight
	{
		get
		{
			return m_scrollBarHeight;
		}
		set
		{
			m_scrollBarHeight = value;
		}
	}

	IPdfPrimitive IPdfWrapper.Element => ImageStream;

	public XmpMetadata Metadata
	{
		get
		{
			if (m_enableMetadata && m_imageMetadata == null)
			{
				m_imageMetadata = GetMetadata();
			}
			return m_imageMetadata;
		}
		set
		{
			if (m_enableMetadata)
			{
				m_imageMetadata = value;
			}
		}
	}

	public static PdfImage FromStream(Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		return new PdfBitmap(stream);
	}

	internal abstract XmpMetadata GetMetadata();

	internal void AddMetadata()
	{
		PdfStream xmpStream = m_imageMetadata.XmpStream;
		xmpStream["Type"] = new PdfName("Metadata");
		xmpStream["Subtype"] = new PdfName("XML");
		xmpStream["Length"] = new PdfNumber(xmpStream.Data.Length);
		ImageStream["Metadata"] = new PdfReferenceHolder(xmpStream);
	}

	internal static Stream CheckStreamExistance(Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		if (stream.Length <= 0)
		{
			throw new ArgumentException("The stream can't be empty", "stream");
		}
		return stream;
	}

	protected static SizeF GetPixelSize(float width, float height)
	{
		float horizontalResolution = PdfUnitConvertor.HorizontalResolution;
		float verticalResolution = PdfUnitConvertor.VerticalResolution;
		float width2 = new PdfUnitConvertor(horizontalResolution).ConvertToPixels(width, PdfGraphicsUnit.Point);
		float height2 = new PdfUnitConvertor(verticalResolution).ConvertToPixels(height, PdfGraphicsUnit.Point);
		return new SizeF(width2, height2);
	}

	internal abstract void Save();

	internal void SetContent(IPdfPrimitive content)
	{
		if (content == null)
		{
			throw new ArgumentNullException("content");
		}
		if (!(content is PdfStream))
		{
			throw new ArgumentException("The content is not a stream.", "content");
		}
		ImageStream = content as PdfStream;
	}

	protected override void DrawInternal(PdfGraphics graphics)
	{
		if (graphics == null)
		{
			throw new ArgumentNullException("graphics");
		}
		graphics.DrawImage(this, PointF.Empty);
	}

	protected override RectangleF GetBoundsInternal()
	{
		return new RectangleF(PointF.Empty, PhysicalDimension);
	}

	protected internal SizeF GetPointSize(float width, float height)
	{
		float horizontalResolution = PdfUnitConvertor.HorizontalResolution;
		float verticalResolution = PdfUnitConvertor.VerticalResolution;
		return GetPointSize(width, height, horizontalResolution, verticalResolution);
	}

	protected internal SizeF GetPointSize(float width, float height, float horizontalResolution, float verticalResolution)
	{
		PdfUnitConvertor pdfUnitConvertor = new PdfUnitConvertor(horizontalResolution);
		PdfUnitConvertor pdfUnitConvertor2 = new PdfUnitConvertor(verticalResolution);
		float width2 = pdfUnitConvertor.ConvertUnits(width, PdfGraphicsUnit.Pixel, PdfGraphicsUnit.Point);
		float height2 = pdfUnitConvertor2.ConvertUnits(height, PdfGraphicsUnit.Pixel, PdfGraphicsUnit.Point);
		return new SizeF(width2, height2);
	}

	protected void SetResolution(float horizontalResolution, float verticalResolution)
	{
		HorizontalResolution = horizontalResolution;
		VerticalResolution = verticalResolution;
	}

	internal abstract PdfImage Clone();
}
