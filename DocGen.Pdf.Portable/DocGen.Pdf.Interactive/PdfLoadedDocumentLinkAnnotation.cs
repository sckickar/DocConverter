using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.IO;
using DocGen.Pdf.Parsing;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public class PdfLoadedDocumentLinkAnnotation : PdfLoadedStyledAnnotation
{
	private PdfCrossTable m_crossTable;

	private PdfDestination pdfDestination;

	public PdfDestination Destination
	{
		get
		{
			if (pdfDestination == null)
			{
				pdfDestination = ObtainDestination();
			}
			return pdfDestination;
		}
		set
		{
			base.Dictionary.SetProperty("Dest", value);
		}
	}

	internal PdfLoadedDocumentLinkAnnotation(PdfDictionary dictionary, PdfCrossTable crossTable, RectangleF rectangle)
		: base(dictionary, crossTable)
	{
		base.Dictionary = dictionary;
		m_crossTable = crossTable;
	}

	private PdfDestination ObtainDestination()
	{
		PdfDestination pdfDestination = null;
		if (base.Dictionary.ContainsKey("Dest"))
		{
			IPdfPrimitive @object = base.CrossTable.GetObject(base.Dictionary["Dest"]);
			PdfArray pdfArray = @object as PdfArray;
			PdfName pdfName = @object as PdfName;
			PdfString pdfString = @object as PdfString;
			if (base.CrossTable.Document is PdfLoadedDocument pdfLoadedDocument)
			{
				if (pdfName != null)
				{
					pdfArray = pdfLoadedDocument.GetNamedDestination(pdfName);
				}
				else if (pdfString != null)
				{
					pdfArray = pdfLoadedDocument.GetNamedDestination(pdfString);
				}
			}
			if (pdfArray != null)
			{
				PdfReferenceHolder pdfReferenceHolder = pdfArray[0] as PdfReferenceHolder;
				PdfPageBase pdfPageBase = null;
				if (pdfReferenceHolder == null && pdfArray[0] is PdfNumber)
				{
					PdfNumber pdfNumber = pdfArray[0] as PdfNumber;
					pdfPageBase = (base.CrossTable.Document as PdfLoadedDocument).Pages[pdfNumber.IntValue];
					PdfName pdfName2 = pdfArray[1] as PdfName;
					if (pdfName2 != null)
					{
						if (pdfName2.Value == "XYZ")
						{
							PdfNumber pdfNumber2 = pdfArray[2] as PdfNumber;
							PdfNumber pdfNumber3 = pdfArray[3] as PdfNumber;
							PdfNumber pdfNumber4 = pdfArray[4] as PdfNumber;
							float y = ((pdfNumber3 == null) ? 0f : (pdfPageBase.Size.Height - pdfNumber3.FloatValue));
							float x = pdfNumber2?.FloatValue ?? 0f;
							pdfDestination = new PdfDestination(pdfPageBase, new PointF(x, y));
							if (pdfNumber4 != null)
							{
								pdfDestination.Zoom = pdfNumber4.FloatValue;
							}
							pdfDestination.isModified = false;
							if (pdfNumber2 == null || pdfNumber3 == null || pdfNumber4 == null)
							{
								pdfDestination.SetValidation(valid: false);
							}
						}
					}
					else if (pdfPageBase != null && pdfName2.Value == "Fit")
					{
						pdfDestination = new PdfDestination(pdfPageBase);
						pdfDestination.Mode = PdfDestinationMode.FitToPage;
						pdfDestination.isModified = false;
					}
					else if (pdfPageBase != null && pdfName2.Value == "FitR")
					{
						if (pdfArray.Count == 6)
						{
							PdfNumber pdfNumber5 = pdfArray[2] as PdfNumber;
							PdfNumber pdfNumber6 = pdfArray[3] as PdfNumber;
							PdfNumber pdfNumber7 = pdfArray[4] as PdfNumber;
							PdfNumber pdfNumber8 = pdfArray[5] as PdfNumber;
							pdfDestination = new PdfDestination(pdfPageBase, new RectangleF(pdfNumber5.FloatValue, pdfNumber6.FloatValue, pdfNumber7.FloatValue, pdfNumber8.FloatValue));
							pdfDestination.Mode = PdfDestinationMode.FitR;
						}
					}
					else if (pdfPageBase != null && (pdfName2.Value == "FitV" || pdfName2.Value == "FitBV"))
					{
						if (pdfArray[2] is PdfNumber pdfNumber9)
						{
							pdfDestination = new PdfDestination(pdfPageBase, new PointF(pdfNumber9.FloatValue, 0f));
						}
						if (pdfName2.Value == "FitV")
						{
							pdfDestination.Mode = PdfDestinationMode.FitV;
						}
						else
						{
							pdfDestination.Mode = PdfDestinationMode.FitBV;
						}
					}
					else if (pdfPageBase != null && (pdfName2.Value == "FitBH" || pdfName2.Value == "FitH"))
					{
						PdfNumber pdfNumber10 = pdfArray[2] as PdfNumber;
						float y2 = ((pdfNumber10 == null) ? 0f : (pdfPageBase.Size.Height - pdfNumber10.FloatValue));
						pdfDestination = new PdfDestination(pdfPageBase, new PointF(0f, y2));
						if (pdfNumber10 == null)
						{
							pdfDestination.SetValidation(valid: false);
						}
						if (pdfName2.Value == "FitH")
						{
							pdfDestination.Mode = PdfDestinationMode.FitH;
						}
						else
						{
							pdfDestination.Mode = PdfDestinationMode.FitBH;
						}
					}
				}
				if (pdfReferenceHolder != null)
				{
					PdfDictionary dic = base.CrossTable.GetObject(pdfReferenceHolder) as PdfDictionary;
					pdfPageBase = (base.CrossTable.Document as PdfLoadedDocument).Pages.GetPage(dic);
					PdfName pdfName3 = pdfArray[1] as PdfName;
					if (pdfName3 != null)
					{
						if (pdfName3.Value == "XYZ")
						{
							PdfNumber pdfNumber11 = pdfArray[2] as PdfNumber;
							PdfNumber pdfNumber12 = pdfArray[3] as PdfNumber;
							PdfNumber pdfNumber13 = null;
							if (pdfArray.Count > 4)
							{
								pdfNumber13 = pdfArray[4] as PdfNumber;
							}
							float y3 = ((pdfNumber12 == null) ? 0f : (pdfPageBase.Size.Height - pdfNumber12.FloatValue));
							float x2 = pdfNumber11?.FloatValue ?? 0f;
							pdfDestination = new PdfDestination(pdfPageBase, new PointF(x2, y3));
							if (pdfNumber13 != null)
							{
								pdfDestination.Zoom = pdfNumber13.FloatValue;
							}
							pdfDestination.isModified = false;
							if (pdfNumber11 == null || pdfNumber12 == null || pdfNumber13 == null)
							{
								pdfDestination.SetValidation(valid: false);
							}
						}
					}
					else if (pdfPageBase != null && pdfName3.Value == "Fit")
					{
						pdfDestination = new PdfDestination(pdfPageBase);
						pdfDestination.Mode = PdfDestinationMode.FitToPage;
						pdfDestination.isModified = false;
					}
					else if (pdfPageBase != null && pdfName3.Value == "FitR")
					{
						if (pdfArray.Count == 6)
						{
							PdfNumber pdfNumber14 = pdfArray[2] as PdfNumber;
							PdfNumber pdfNumber15 = pdfArray[3] as PdfNumber;
							PdfNumber pdfNumber16 = pdfArray[4] as PdfNumber;
							PdfNumber pdfNumber17 = pdfArray[5] as PdfNumber;
							pdfDestination = new PdfDestination(pdfPageBase, new RectangleF(pdfNumber14.FloatValue, pdfNumber15.FloatValue, pdfNumber16.FloatValue, pdfNumber17.FloatValue));
							pdfDestination.Mode = PdfDestinationMode.FitR;
						}
					}
					else if (pdfPageBase != null && (pdfName3.Value == "FitV" || pdfName3.Value == "FitBV"))
					{
						if (pdfArray[2] is PdfNumber pdfNumber18)
						{
							pdfDestination = new PdfDestination(pdfPageBase, new PointF(pdfNumber18.FloatValue, 0f));
						}
						if (pdfName3.Value == "FitV")
						{
							pdfDestination.Mode = PdfDestinationMode.FitV;
						}
						else
						{
							pdfDestination.Mode = PdfDestinationMode.FitBV;
						}
					}
					else if (pdfPageBase != null && (pdfName3.Value == "FitBH" || pdfName3.Value == "FitH"))
					{
						PdfNumber pdfNumber19 = pdfArray[2] as PdfNumber;
						float y4 = ((pdfNumber19 == null) ? 0f : (pdfPageBase.Size.Height - pdfNumber19.FloatValue));
						pdfDestination = new PdfDestination(pdfPageBase, new PointF(0f, y4));
						if (pdfNumber19 == null)
						{
							pdfDestination.SetValidation(valid: false);
						}
						if (pdfName3.Value == "FitH")
						{
							pdfDestination.Mode = PdfDestinationMode.FitH;
						}
						else
						{
							pdfDestination.Mode = PdfDestinationMode.FitBH;
						}
					}
				}
			}
		}
		else if (base.Dictionary.ContainsKey("A") && pdfDestination == null)
		{
			IPdfPrimitive object2 = base.CrossTable.GetObject(base.Dictionary["A"]);
			object2 = (object2 as PdfDictionary)["D"];
			if (object2 is PdfReferenceHolder)
			{
				object2 = (object2 as PdfReferenceHolder).Object;
			}
			PdfArray pdfArray2 = object2 as PdfArray;
			PdfName pdfName4 = object2 as PdfName;
			PdfString pdfString2 = object2 as PdfString;
			if (base.CrossTable.Document is PdfLoadedDocument pdfLoadedDocument2)
			{
				if (pdfName4 != null)
				{
					pdfArray2 = pdfLoadedDocument2.GetNamedDestination(pdfName4);
				}
				else if (pdfString2 != null)
				{
					pdfArray2 = pdfLoadedDocument2.GetNamedDestination(pdfString2);
				}
			}
			if (pdfArray2 != null)
			{
				PdfReferenceHolder pdfReferenceHolder2 = pdfArray2[0] as PdfReferenceHolder;
				PdfPageBase pdfPageBase2 = null;
				if (pdfReferenceHolder2 != null && PdfCrossTable.Dereference(pdfReferenceHolder2) is PdfDictionary dic2)
				{
					pdfPageBase2 = (base.CrossTable.Document as PdfLoadedDocument).Pages.GetPage(dic2);
				}
				if (pdfPageBase2 != null)
				{
					PdfName pdfName5 = pdfArray2[1] as PdfName;
					if (pdfName5.Value == "FitBH" || pdfName5.Value == "FitH")
					{
						PdfNumber pdfNumber20 = pdfArray2[2] as PdfNumber;
						float y5 = ((pdfNumber20 == null) ? 0f : (pdfPageBase2.Size.Height - pdfNumber20.FloatValue));
						pdfDestination = new PdfDestination(pdfPageBase2, new PointF(0f, y5));
						if (pdfNumber20 == null)
						{
							pdfDestination.SetValidation(valid: false);
						}
					}
					else if (pdfName5.Value == "XYZ")
					{
						PdfNumber pdfNumber21 = pdfArray2[2] as PdfNumber;
						PdfNumber pdfNumber22 = pdfArray2[3] as PdfNumber;
						PdfNumber pdfNumber23 = pdfArray2[4] as PdfNumber;
						if (pdfPageBase2 != null)
						{
							float y6 = ((pdfNumber22 == null) ? 0f : (pdfPageBase2.Size.Height - pdfNumber22.FloatValue));
							float x3 = pdfNumber21?.FloatValue ?? 0f;
							pdfDestination = new PdfDestination(pdfPageBase2, new PointF(x3, y6));
							if (pdfNumber23 != null)
							{
								pdfDestination.Zoom = pdfNumber23.FloatValue;
							}
							if (pdfNumber21 == null || pdfNumber22 == null || pdfNumber23 == null)
							{
								pdfDestination.SetValidation(valid: false);
							}
						}
					}
					else if (pdfName5.Value == "FitR")
					{
						if (pdfArray2.Count == 6)
						{
							PdfNumber pdfNumber24 = pdfArray2[2] as PdfNumber;
							PdfNumber pdfNumber25 = pdfArray2[3] as PdfNumber;
							PdfNumber pdfNumber26 = pdfArray2[4] as PdfNumber;
							PdfNumber pdfNumber27 = pdfArray2[5] as PdfNumber;
							pdfDestination = new PdfDestination(pdfPageBase2, new RectangleF(pdfNumber24.FloatValue, pdfNumber25.FloatValue, pdfNumber26.FloatValue, pdfNumber27.FloatValue));
						}
					}
					else if (pdfPageBase2 != null)
					{
						if (pdfName5.Value == "Fit")
						{
							pdfDestination = new PdfDestination(pdfPageBase2);
							pdfDestination.Mode = PdfDestinationMode.FitToPage;
							pdfDestination.isModified = false;
						}
						else if (pdfName5.Value == "FitV")
						{
							pdfDestination = new PdfDestination(pdfPageBase2);
							pdfDestination.Mode = PdfDestinationMode.FitV;
							pdfDestination.isModified = false;
						}
					}
				}
			}
		}
		return pdfDestination;
	}

	internal override void FlattenAnnot(bool flattenPopUps)
	{
		SaveAndFlatten(isExternalFlatten: true, flattenPopUps);
	}

	private void SaveAndFlatten(bool isExternalFlatten, bool isExternalFlattenPopUps)
	{
		if (!(base.Flatten || base.Page.Annotations.Flatten || isExternalFlatten))
		{
			return;
		}
		if (base.Dictionary["AP"] != null && PdfCrossTable.Dereference(base.Dictionary["AP"]) is PdfDictionary pdfDictionary && pdfDictionary.ContainsKey("N") && PdfCrossTable.Dereference(pdfDictionary["N"]) is PdfDictionary pdfDictionary2 && pdfDictionary2 is PdfStream template)
		{
			PdfTemplate pdfTemplate = new PdfTemplate(template);
			if (pdfTemplate != null)
			{
				PdfGraphics pdfGraphics = ObtainlayerGraphics();
				PdfGraphicsState state = base.Page.Graphics.Save();
				if (Opacity < 1f)
				{
					base.Page.Graphics.SetTransparency(Opacity);
				}
				if (pdfGraphics != null)
				{
					pdfGraphics.DrawPdfTemplate(pdfTemplate, Bounds.Location, Bounds.Size);
				}
				else
				{
					base.Page.Graphics.DrawPdfTemplate(pdfTemplate, Bounds.Location, Bounds.Size);
				}
				base.Page.Graphics.Restore(state);
			}
		}
		RemoveAnnoationFromPage(base.Page, this);
		if (Popup != null && (base.Flatten || base.Page.Annotations.Flatten || isExternalFlatten))
		{
			RemoveAnnoationFromPage(base.Page, Popup);
		}
	}

	protected override void Save()
	{
		CheckFlatten();
		SaveAndFlatten(isExternalFlatten: false, isExternalFlattenPopUps: false);
		if (pdfDestination != null && pdfDestination.isModified)
		{
			if (base.Dictionary.ContainsKey("Dest"))
			{
				base.Dictionary.SetProperty("Dest", pdfDestination);
			}
			else if (base.Dictionary.ContainsKey("A") && base.CrossTable.GetObject(base.Dictionary["A"]) is PdfDictionary pdfDictionary && pdfDictionary.ContainsKey("D"))
			{
				pdfDictionary.SetProperty("D", pdfDestination);
			}
		}
	}
}
