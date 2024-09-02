using System.IO;

namespace DocGen.Pdf.Graphics;

internal static class PdfGraphicsExtension
{
	internal static PdfImage GetImage(this PdfGraphics graphics, Stream stream)
	{
		PdfImage pdfImage;
		if (graphics.OptimizeIdenticalImages && graphics.Page != null)
		{
			if (graphics.Page is PdfPage { Document: not null } pdfPage)
			{
				PdfDocument document = pdfPage.Document;
				stream.Position = 0L;
				byte[] array = new byte[(int)stream.Length];
				stream.Read(array, 0, array.Length);
				stream.Position = 0L;
				string key = document.CreateHashFromStream(array);
				if (document.ImageCollection.ContainsKey(key))
				{
					pdfImage = document.ImageCollection[key];
				}
				else
				{
					pdfImage = new PdfTiffImage(new MemoryStream(array));
					document.ImageCollection.Add(key, pdfImage);
				}
			}
			else
			{
				pdfImage = new PdfTiffImage(stream);
			}
		}
		else
		{
			pdfImage = new PdfTiffImage(stream);
		}
		return pdfImage;
	}
}
