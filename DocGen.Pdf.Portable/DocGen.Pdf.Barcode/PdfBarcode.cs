using DocGen.Drawing;
using DocGen.Pdf.Graphics;

namespace DocGen.Pdf.Barcode;

public class PdfBarcode
{
	protected internal RectangleF bounds;

	private PdfColor backColor;

	private PdfColor barColor;

	private PdfColor textColor;

	private float narrowBarWidth;

	private float wideBarWidth;

	private PointF location;

	protected internal SizeF size;

	private string text = string.Empty;

	private PdfBarcodeQuietZones quietZones;

	private float width;

	private float height;

	protected float barHeight;

	private string extendedText = string.Empty;

	internal bool barHeightEnabled;

	internal bool isCustomSize;

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

	public PdfColor BarColor
	{
		get
		{
			return barColor;
		}
		set
		{
			barColor = value;
		}
	}

	public PdfColor TextColor
	{
		get
		{
			return textColor;
		}
		set
		{
			textColor = value;
		}
	}

	public float NarrowBarWidth
	{
		get
		{
			return narrowBarWidth;
		}
		set
		{
			narrowBarWidth = value;
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

	public PdfBarcodeQuietZones QuietZone
	{
		get
		{
			return quietZones;
		}
		set
		{
			quietZones = value;
		}
	}

	public float BarHeight
	{
		get
		{
			return barHeight;
		}
		set
		{
			barHeight = value;
			barHeightEnabled = true;
		}
	}

	public SizeF Size
	{
		get
		{
			if (size != SizeF.Empty)
			{
				return size;
			}
			return GetSizeValue();
		}
		set
		{
			size = value;
			isCustomSize = true;
		}
	}

	public RectangleF Bounds
	{
		get
		{
			return bounds;
		}
		set
		{
			bounds = value;
		}
	}

	internal string ExtendedText
	{
		get
		{
			return extendedText;
		}
		set
		{
			extendedText = value;
		}
	}

	public PdfBarcode()
	{
		Initialize();
	}

	public PdfBarcode(string text)
		: this()
	{
		Text = text;
	}

	protected internal virtual bool Validate(string data)
	{
		return true;
	}

	protected internal virtual SizeF GetSizeValue()
	{
		return new SizeF(0f, 0f);
	}

	private void Initialize()
	{
		barColor = new PdfColor(Color.FromArgb(255, 0, 0, 0));
		backColor = new PdfColor(Color.FromArgb(255, 255, 255, 255));
		textColor = new PdfColor(Color.FromArgb(255, 0, 0, 0));
		narrowBarWidth = 0.864f;
		wideBarWidth = 2.592f;
		PdfBarcodeQuietZones pdfBarcodeQuietZones = new PdfBarcodeQuietZones();
		pdfBarcodeQuietZones.All = 0f;
		quietZones = pdfBarcodeQuietZones;
		barHeight = 80f;
	}
}
