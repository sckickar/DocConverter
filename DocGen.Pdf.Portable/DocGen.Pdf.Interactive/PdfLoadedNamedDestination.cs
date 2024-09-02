using DocGen.Drawing;
using DocGen.Pdf.IO;
using DocGen.Pdf.Parsing;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public class PdfLoadedNamedDestination : PdfNamedDestination
{
	public override PdfDestination Destination
	{
		get
		{
			return ObtainDestination();
		}
		set
		{
			base.Destination = value;
		}
	}

	public override string Title
	{
		get
		{
			string result = string.Empty;
			if (base.Dictionary.ContainsKey("Title"))
			{
				PdfString pdfString = null;
				pdfString = ((base.CrossTable.Document == null) ? (PdfCrossTable.Dereference(base.Dictionary["Title"]) as PdfString) : (base.CrossTable.GetObject(base.Dictionary["Title"]) as PdfString));
				result = pdfString.Value;
			}
			return result;
		}
		set
		{
			base.Title = value;
		}
	}

	internal PdfLoadedNamedDestination(PdfDictionary dictionary, PdfCrossTable crossTable)
		: base(dictionary, crossTable)
	{
	}

	private PdfDestination ObtainDestination()
	{
		if (base.Dictionary.ContainsKey("D") && base.Destination == null && base.CrossTable.GetObject(base.Dictionary["D"]) is PdfArray { Count: >1 } pdfArray)
		{
			PdfReferenceHolder pdfReferenceHolder = pdfArray[0] as PdfReferenceHolder;
			PdfPageBase pdfPageBase = null;
			if (pdfReferenceHolder != null && base.CrossTable.GetObject(pdfReferenceHolder) is PdfDictionary dic)
			{
				pdfPageBase = (base.CrossTable.Document as PdfLoadedDocument).Pages.GetPage(dic);
			}
			PdfName pdfName = pdfArray[1] as PdfName;
			if (pdfName != null)
			{
				if ((pdfName.Value == "FitBH" || pdfName.Value == "FitH") && pdfArray.Count > 2)
				{
					PdfNumber pdfNumber = pdfArray[2] as PdfNumber;
					if (pdfPageBase != null)
					{
						float y = ((pdfNumber == null) ? 0f : (pdfPageBase.Size.Height - pdfNumber.FloatValue));
						base.Destination = new PdfDestination(pdfPageBase, new PointF(0f, y));
						if (pdfPageBase is PdfLoadedPage)
						{
							base.Destination.PageIndex = ((pdfPageBase as PdfLoadedPage).Document as PdfLoadedDocument).Pages.IndexOf(pdfPageBase);
						}
						base.Destination.Mode = PdfDestinationMode.FitH;
						base.Destination.isModified = false;
						if (pdfNumber == null)
						{
							base.Destination.SetValidation(valid: false);
						}
						if (pdfName.Value == "FitH")
						{
							base.Destination.Mode = PdfDestinationMode.FitH;
						}
						else
						{
							base.Destination.Mode = PdfDestinationMode.FitBH;
						}
					}
				}
				else if (pdfName.Value == "XYZ" && pdfArray.Count > 3)
				{
					PdfNumber pdfNumber2 = pdfArray[2] as PdfNumber;
					PdfNumber pdfNumber3 = pdfArray[3] as PdfNumber;
					PdfNumber pdfNumber4 = null;
					if (pdfArray.Count > 4)
					{
						pdfNumber4 = pdfArray[4] as PdfNumber;
					}
					if (pdfPageBase != null)
					{
						float y2 = ((pdfNumber3 == null) ? 0f : (pdfPageBase.Size.Height - pdfNumber3.FloatValue));
						float x = pdfNumber2?.FloatValue ?? 0f;
						base.Destination = new PdfDestination(pdfPageBase, new PointF(x, y2));
						if (pdfPageBase is PdfLoadedPage)
						{
							base.Destination.PageIndex = ((pdfPageBase as PdfLoadedPage).Document as PdfLoadedDocument).Pages.IndexOf(pdfPageBase);
						}
						if (pdfNumber4 != null)
						{
							base.Destination.Zoom = pdfNumber4.FloatValue;
						}
						base.Destination.isModified = false;
						if (pdfNumber2 == null || pdfNumber3 == null || pdfNumber4 == null)
						{
							base.Destination.SetValidation(valid: false);
						}
					}
				}
				else if (pdfName.Value == "FitR" && pdfArray.Count > 5)
				{
					PdfNumber pdfNumber5 = pdfArray[2] as PdfNumber;
					PdfNumber pdfNumber6 = pdfArray[3] as PdfNumber;
					PdfNumber pdfNumber7 = pdfArray[4] as PdfNumber;
					PdfNumber pdfNumber8 = pdfArray[5] as PdfNumber;
					if (pdfPageBase != null)
					{
						pdfNumber5 = ((pdfNumber5 == null) ? new PdfNumber(0) : pdfNumber5);
						pdfNumber6 = ((pdfNumber6 == null) ? new PdfNumber(0) : pdfNumber6);
						pdfNumber7 = ((pdfNumber7 == null) ? new PdfNumber(0) : pdfNumber7);
						pdfNumber8 = ((pdfNumber8 == null) ? new PdfNumber(0) : pdfNumber8);
						base.Destination = new PdfDestination(pdfPageBase, new RectangleF(pdfNumber5.FloatValue, pdfNumber6.FloatValue, pdfNumber7.FloatValue, pdfNumber8.FloatValue));
						base.Destination.PageIndex = ((pdfPageBase as PdfLoadedPage).Document as PdfLoadedDocument).Pages.IndexOf(pdfPageBase);
						base.Destination.Mode = PdfDestinationMode.FitR;
						base.Destination.isModified = false;
					}
				}
				else if (pdfPageBase != null && pdfName.Value == "Fit")
				{
					base.Destination = new PdfDestination(pdfPageBase);
					if (pdfPageBase is PdfLoadedPage)
					{
						base.Destination.PageIndex = ((pdfPageBase as PdfLoadedPage).Document as PdfLoadedDocument).Pages.IndexOf(pdfPageBase);
					}
					base.Destination.Mode = PdfDestinationMode.FitToPage;
					base.Destination.isModified = false;
				}
				else if (pdfName.Value == "FitV" || pdfName.Value == "FitBV")
				{
					if (pdfArray[2] is PdfNumber pdfNumber9)
					{
						base.Destination = new PdfDestination(pdfPageBase, new PointF(pdfNumber9.FloatValue, 0f));
					}
					if (pdfPageBase is PdfLoadedPage)
					{
						base.Destination.PageIndex = ((pdfPageBase as PdfLoadedPage).Document as PdfLoadedDocument).Pages.IndexOf(pdfPageBase);
					}
					if (pdfName.Value == "FitV")
					{
						base.Destination.Mode = PdfDestinationMode.FitV;
					}
					else
					{
						base.Destination.Mode = PdfDestinationMode.FitBV;
					}
				}
			}
		}
		return base.Destination;
	}
}
