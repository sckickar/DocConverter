using DocGen.Drawing;

namespace DocGen.Pdf.Graphics;

public class PdfUnitConverter
{
	internal static readonly float HorizontalResolution;

	internal static readonly float VerticalResolution;

	internal static readonly float HorizontalSize;

	internal static readonly float VerticalSize;

	internal static readonly float PxHorizontalResolution;

	internal static readonly float PxVerticalResolution;

	private double[] m_proportions;

	static PdfUnitConverter()
	{
		HorizontalResolution = 96f;
		VerticalResolution = 96f;
	}

	public PdfUnitConverter()
	{
		UpdateProportions(HorizontalResolution);
	}

	public PdfUnitConverter(float dpi)
	{
		UpdateProportions(dpi);
	}

	public float ConvertUnits(float value, PdfGraphicsUnit from, PdfGraphicsUnit to)
	{
		return ConvertFromPixels(ConvertToPixels(value, from), to);
	}

	public float ConvertToPixels(float value, PdfGraphicsUnit from)
	{
		return (float)((double)value * m_proportions[(int)from]);
	}

	public RectangleF ConvertToPixels(RectangleF rect, PdfGraphicsUnit from)
	{
		float x = ConvertToPixels(rect.X, from);
		float y = ConvertToPixels(rect.Y, from);
		float width = ConvertToPixels(rect.Width, from);
		float height = ConvertToPixels(rect.Height, from);
		return new RectangleF(x, y, width, height);
	}

	public PointF ConvertToPixels(PointF point, PdfGraphicsUnit from)
	{
		float x = ConvertToPixels(point.X, from);
		float y = ConvertToPixels(point.Y, from);
		return new PointF(x, y);
	}

	public SizeF ConvertToPixels(SizeF size, PdfGraphicsUnit from)
	{
		float width = ConvertToPixels(size.Width, from);
		float height = ConvertToPixels(size.Height, from);
		return new SizeF(width, height);
	}

	public float ConvertFromPixels(float value, PdfGraphicsUnit to)
	{
		return (float)((double)value / m_proportions[(int)to]);
	}

	public RectangleF ConvertFromPixels(RectangleF rect, PdfGraphicsUnit to)
	{
		float x = ConvertFromPixels(rect.X, to);
		float y = ConvertFromPixels(rect.Y, to);
		float width = ConvertFromPixels(rect.Width, to);
		float height = ConvertFromPixels(rect.Height, to);
		return new RectangleF(x, y, width, height);
	}

	public PointF ConvertFromPixels(PointF point, PdfGraphicsUnit to)
	{
		float x = ConvertFromPixels(point.X, to);
		float y = ConvertFromPixels(point.Y, to);
		return new PointF(x, y);
	}

	public SizeF ConvertFromPixels(SizeF size, PdfGraphicsUnit to)
	{
		float width = ConvertFromPixels(size.Width, to);
		float height = ConvertFromPixels(size.Height, to);
		return new SizeF(width, height);
	}

	private void UpdateProportions(float pixelPerInch)
	{
		m_proportions = new double[7]
		{
			(double)pixelPerInch / 2.54,
			(double)pixelPerInch / 6.0,
			1.0,
			(double)pixelPerInch / 72.0,
			pixelPerInch,
			(double)pixelPerInch / 300.0,
			(double)pixelPerInch / 25.4
		};
	}
}
