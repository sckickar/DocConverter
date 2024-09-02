using System.Collections.Generic;
using System.IO;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Exporting;

public static class PdfImageExtractor
{
	public static PdfImageInfo[] GetImagesInfo(this PdfPageBase page)
	{
		return ImageExportHelperNet.GetImageExporter(page).ImagesInfo;
	}

	public static Stream[] ExtractImages(this PdfPageBase page)
	{
		ImageExportHelperNet imageExporter = ImageExportHelperNet.GetImageExporter(page);
		List<Stream> list = new List<Stream>();
		PdfImageInfo[] imagesInfo = imageExporter.ImagesInfo;
		foreach (PdfImageInfo pdfImageInfo in imagesInfo)
		{
			if (pdfImageInfo.ImageStream != null)
			{
				list.Add(pdfImageInfo.ImageStream);
			}
		}
		return list.ToArray();
	}

	public static void ReplaceImage(this PdfPageBase page, int index, PdfImage image)
	{
		new ImageExportHelperNet(page).ReplaceImage(index, image, page);
	}

	internal static void ReplaceImageStream(this PdfPageBase page, long objIndex, PdfReferenceHolder imageReference, PdfPageBase currentpage)
	{
		new ImageExportHelperNet(page).ReplaceImageStream(objIndex, imageReference, currentpage);
	}

	public static void RemoveImage(this PdfPageBase page, PdfImageInfo imageInfo)
	{
		new ImageExportHelperNet(page).RemoveImage(imageInfo);
	}
}
