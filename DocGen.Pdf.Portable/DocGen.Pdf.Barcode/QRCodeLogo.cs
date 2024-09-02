using System.IO;
using DocGen.Pdf.Graphics;

namespace DocGen.Pdf.Barcode;

public class QRCodeLogo
{
	private PdfImage image;

	public PdfImage Image
	{
		internal get
		{
			return image;
		}
		set
		{
			image = value;
		}
	}

	public QRCodeLogo()
	{
	}

	public QRCodeLogo(PdfImage logoImage)
	{
		Image = logoImage;
	}

	public QRCodeLogo(Stream logoImageStream)
	{
		Image = new PdfBitmap(logoImageStream);
	}
}
