using System.Collections.Generic;
using System.IO;
using DocGen.Pdf.IO;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

internal class PdfImageExtractionHelper
{
	private List<IPdfPrimitive> pdfPrimitivesCollection;

	private List<Stream> imageStreams;

	internal PdfDictionary Dictionary { get; set; }

	internal Stream[] ImageStreams
	{
		get
		{
			if (imageStreams == null)
			{
				imageStreams = new List<Stream>();
				ExtractImages();
			}
			return imageStreams.ToArray();
		}
	}

	internal PdfImageExtractionHelper(PdfDictionary dictionary)
	{
		Dictionary = dictionary;
	}

	internal void ExtractImages()
	{
		PdfDictionary dictionary = Dictionary;
		if (dictionary.ContainsKey("AP") && PdfCrossTable.Dereference(dictionary["AP"]) is PdfDictionary pdfDictionary && PdfCrossTable.Dereference(pdfDictionary["N"]) is PdfDictionary pdfDictionary2 && pdfDictionary2.ContainsKey("Resources") && PdfCrossTable.Dereference(pdfDictionary2["Resources"]) is PdfDictionary pdfDictionary3)
		{
			PdfDictionary xObj = PdfCrossTable.Dereference(pdfDictionary3["XObject"]) as PdfDictionary;
			pdfPrimitivesCollection = new List<IPdfPrimitive>();
			ReadImageData(xObj);
			pdfPrimitivesCollection.Clear();
			pdfPrimitivesCollection = null;
		}
	}

	private void ReadImageData(PdfDictionary xObj)
	{
		if (xObj == null)
		{
			return;
		}
		foreach (IPdfPrimitive value in xObj.Values)
		{
			PdfDictionary pdfDictionary = PdfCrossTable.Dereference(value) as PdfDictionary;
			if (pdfDictionary != null && pdfDictionary.ContainsKey("Subtype") && (pdfDictionary["Subtype"] as PdfName).Value == "Image")
			{
				if (pdfDictionary is PdfStream)
				{
					Stream embeddedImageStream = new ImageStructureNet(pdfDictionary, new PdfMatrix()).GetEmbeddedImageStream();
					if (embeddedImageStream != null)
					{
						imageStreams.Add(embeddedImageStream);
					}
				}
			}
			else if (pdfDictionary != null && pdfDictionary.ContainsKey("Subtype") && (pdfDictionary["Subtype"] as PdfName).Value == "Form" && pdfDictionary != null && pdfDictionary.ContainsKey("Resources") && PdfCrossTable.Dereference(pdfDictionary["Resources"]) is PdfDictionary pdfDictionary2 && PdfCrossTable.Dereference(pdfDictionary2["XObject"]) is PdfDictionary pdfDictionary3 && !IsRepeatedEntry(pdfDictionary3))
			{
				ReadImageData(pdfDictionary3);
			}
		}
	}

	private bool IsRepeatedEntry(IPdfPrimitive primitive)
	{
		if (pdfPrimitivesCollection.Contains(primitive))
		{
			return true;
		}
		pdfPrimitivesCollection.Add(primitive);
		return false;
	}
}
