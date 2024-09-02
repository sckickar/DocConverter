using System.IO;
using DocGen.Drawing;
using DocGen.Pdf.Barcode;

namespace DocGen.Pdf.Graphics;

public static class PdfBarcodeExtension
{
	public static Stream ToImage(this PdfBidimensionalBarcode barcode)
	{
		return barcode.ToImage(Size.Empty);
	}

	public static Stream ToImage(this PdfBidimensionalBarcode barcode, SizeF size)
	{
		if (barcode is PdfQRBarcode)
		{
			return new PdfQRBarcodeHelper(barcode as PdfQRBarcode).ToImage(size);
		}
		if (barcode is Pdf417Barcode)
		{
			return new Pdf417BarcodeHelper(barcode as Pdf417Barcode).ToImage(size);
		}
		return new PdfDataMatricBarcodeHelper(barcode as PdfDataMatrixBarcode).ToImage(size);
	}

	public static Stream ToImage(this PdfUnidimensionalBarcode barcode)
	{
		if (barcode is PdfEan13Barcode)
		{
			return new PdfEan13BarcodeHelper(barcode as PdfEan13Barcode).ToImage();
		}
		if (barcode is PdfEan8Barcode)
		{
			return new PdfEan8BarcodeHelper(barcode as PdfEan8Barcode).ToImage();
		}
		return new PdfUnidimensionalBarcodeHelper(barcode).ToImage();
	}

	public static Stream ToImage(this PdfUnidimensionalBarcode barcode, SizeF size)
	{
		if (barcode is PdfEan13Barcode)
		{
			return new PdfEan13BarcodeHelper(barcode as PdfEan13Barcode).ToImage(size);
		}
		if (barcode is PdfEan8Barcode)
		{
			return new PdfEan8BarcodeHelper(barcode as PdfEan8Barcode).ToImage(size);
		}
		return new PdfUnidimensionalBarcodeHelper(barcode).ToImage(size);
	}
}
