using System.Text;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;

namespace DocGen.Pdf.Barcode;

public abstract class PdfBidimensionalBarcode
{
	private string text;

	private PdfColor backColor;

	private PointF location;

	private PdfBarcodeQuietZones quietZone;

	private float xDimension;

	private SizeF size;

	private PdfColor foreColor;

	public virtual SizeF Size
	{
		get
		{
			return size;
		}
		set
		{
			size = value;
		}
	}

	public string Text
	{
		get
		{
			return text;
		}
		set
		{
			text = value;
		}
	}

	public PdfColor BackColor
	{
		get
		{
			return backColor;
		}
		set
		{
			backColor = value;
		}
	}

	public PdfBarcodeQuietZones QuietZone
	{
		get
		{
			return quietZone;
		}
		set
		{
			quietZone = value;
		}
	}

	public float XDimension
	{
		get
		{
			return xDimension;
		}
		set
		{
			xDimension = value;
		}
	}

	public PointF Location
	{
		get
		{
			return location;
		}
		set
		{
			location = value;
		}
	}

	public PdfColor ForeColor
	{
		get
		{
			return foreColor;
		}
		set
		{
			foreColor = value;
		}
	}

	public PdfBidimensionalBarcode()
	{
		quietZone = new PdfBarcodeQuietZones();
	}

	internal byte[] GetData()
	{
		return Encoding.UTF8.GetBytes(text);
	}

	public abstract void Draw(PdfGraphics graphics);

	public abstract void Draw(PdfGraphics graphics, PointF location);

	public abstract void Draw(PdfPageBase page, PointF location);

	public abstract void Draw(PdfPageBase page);
}
