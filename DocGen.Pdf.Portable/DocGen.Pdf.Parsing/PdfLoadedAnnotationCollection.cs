using System;
using System.Collections.Generic;
using DocGen.Drawing;
using DocGen.Pdf.Interactive;
using DocGen.Pdf.IO;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Parsing;

public class PdfLoadedAnnotationCollection : PdfAnnotationCollection
{
	private PdfLoadedPage m_page;

	private bool m_flatten;

	public override PdfAnnotation this[int index]
	{
		get
		{
			int count = base.List.Count;
			if (count < 0 || index >= count)
			{
				throw new IndexOutOfRangeException("index");
			}
			PdfAnnotation pdfAnnotation = base.List[index] as PdfAnnotation;
			if (pdfAnnotation is PdfLoadedAnnotation)
			{
				(pdfAnnotation as PdfLoadedAnnotation).m_loadedpage = Page;
			}
			else
			{
				pdfAnnotation?.SetPage(Page);
			}
			return pdfAnnotation;
		}
	}

	public PdfAnnotation this[string text]
	{
		get
		{
			if (text == null)
			{
				throw new ArgumentNullException("text");
			}
			if (text == string.Empty)
			{
				throw new ArgumentException("Annotation text can't be empty");
			}
			int annotationIndex = GetAnnotationIndex(text);
			if (annotationIndex == -1)
			{
				throw new ArgumentException("Incorrect field name");
			}
			return this[annotationIndex];
		}
	}

	public PdfLoadedPage Page
	{
		get
		{
			return m_page;
		}
		set
		{
			m_page = value;
		}
	}

	public bool Flatten
	{
		get
		{
			return m_flatten;
		}
		set
		{
			m_flatten = value;
			PdfLoadedDocument pdfLoadedDocument = m_page.Document as PdfLoadedDocument;
			bool flag = false;
			if (pdfLoadedDocument != null && pdfLoadedDocument.Catalog != null && pdfLoadedDocument.Catalog.ContainsKey("AcroForm") && PdfCrossTable.Dereference(pdfLoadedDocument.Catalog["AcroForm"]) is PdfDictionary pdfDictionary && pdfDictionary.ContainsKey("Fields") && PdfCrossTable.Dereference(pdfDictionary["Fields"]) is PdfArray { Count: >0 })
			{
				flag = true;
			}
			if (m_flatten && !flag)
			{
				PdfCrossTable crossTable = m_page.CrossTable;
				if (Page.Dictionary.ContainsKey("Annots") && crossTable.GetObject(Page.Dictionary["Annots"]) is PdfArray pdfArray2)
				{
					for (int i = 0; i < pdfArray2.Count; i++)
					{
						if (crossTable.GetObject(pdfArray2[i]) is PdfDictionary pdfDictionary2)
						{
							if (pdfDictionary2.ContainsKey("FT"))
							{
								pdfDictionary2.Remove("FT");
							}
							if (pdfDictionary2.ContainsKey("V"))
							{
								pdfDictionary2.Remove("V");
							}
						}
					}
				}
			}
			if (!m_flatten || pdfLoadedDocument == null || pdfLoadedDocument.m_redactAnnotationCollection.Count <= 0)
			{
				return;
			}
			for (int j = 0; j < pdfLoadedDocument.m_redactAnnotationCollection.Count; j++)
			{
				if (pdfLoadedDocument.m_redactAnnotationCollection[j] is PdfLoadedRedactionAnnotation pdfLoadedRedactionAnnotation)
				{
					pdfLoadedRedactionAnnotation.Flatten = true;
				}
			}
		}
	}

	internal PdfLoadedAnnotationCollection(PdfLoadedPage page)
	{
		if (page == null)
		{
			throw new ArgumentException("page");
		}
		m_page = page;
		int i = 0;
		for (int count = m_page.TerminalAnnotation.Count; i < count; i++)
		{
			PdfAnnotation annotation = GetAnnotation(i);
			if (annotation != null)
			{
				annotation.m_addingOldAnnotation = true;
				DoAdd(annotation);
				annotation.m_addingOldAnnotation = false;
			}
		}
		Page = m_page;
	}

	private void ldAnnotation_NameChanded(string name)
	{
		if (!IsValidName(name))
		{
			throw new ArgumentException("Annotation with the same name already exist");
		}
	}

	public override int Add(PdfAnnotation annotation)
	{
		if (annotation == null)
		{
			throw new ArgumentNullException("annotation");
		}
		if (annotation is PdfTextMarkupAnnotation)
		{
			PdfTextMarkupAnnotation obj = annotation as PdfTextMarkupAnnotation;
			annotation.SetPage(m_page);
			obj.SetQuadPoints(m_page.Size);
		}
		return DoAdd(annotation);
	}

	private PdfPopupAnnotationCollection GetCommentsOrReview(PdfAnnotation annotation, bool isReview)
	{
		if (annotation is PdfPopupAnnotation)
		{
			PdfPopupAnnotation pdfPopupAnnotation = annotation as PdfPopupAnnotation;
			if (isReview)
			{
				return pdfPopupAnnotation.ReviewHistory;
			}
			return pdfPopupAnnotation.Comments;
		}
		if (annotation is PdfRectangleAnnotation)
		{
			PdfRectangleAnnotation pdfRectangleAnnotation = annotation as PdfRectangleAnnotation;
			if (isReview)
			{
				return pdfRectangleAnnotation.ReviewHistory;
			}
			return pdfRectangleAnnotation.Comments;
		}
		if (annotation is PdfCircleAnnotation)
		{
			PdfCircleAnnotation pdfCircleAnnotation = annotation as PdfCircleAnnotation;
			if (isReview)
			{
				return pdfCircleAnnotation.ReviewHistory;
			}
			return pdfCircleAnnotation.Comments;
		}
		if (annotation is PdfLineAnnotation)
		{
			PdfLineAnnotation pdfLineAnnotation = annotation as PdfLineAnnotation;
			if (isReview)
			{
				return pdfLineAnnotation.ReviewHistory;
			}
			return pdfLineAnnotation.Comments;
		}
		if (annotation is PdfSquareAnnotation)
		{
			PdfSquareAnnotation pdfSquareAnnotation = annotation as PdfSquareAnnotation;
			if (isReview)
			{
				return pdfSquareAnnotation.ReviewHistory;
			}
			return pdfSquareAnnotation.Comments;
		}
		if (annotation is PdfEllipseAnnotation)
		{
			PdfEllipseAnnotation pdfEllipseAnnotation = annotation as PdfEllipseAnnotation;
			if (isReview)
			{
				return pdfEllipseAnnotation.ReviewHistory;
			}
			return pdfEllipseAnnotation.Comments;
		}
		if (annotation is PdfFreeTextAnnotation)
		{
			PdfFreeTextAnnotation pdfFreeTextAnnotation = annotation as PdfFreeTextAnnotation;
			if (isReview)
			{
				return pdfFreeTextAnnotation.ReviewHistory;
			}
			return pdfFreeTextAnnotation.Comments;
		}
		if (annotation is PdfTextMarkupAnnotation)
		{
			PdfTextMarkupAnnotation pdfTextMarkupAnnotation = annotation as PdfTextMarkupAnnotation;
			if (isReview)
			{
				return pdfTextMarkupAnnotation.ReviewHistory;
			}
			return pdfTextMarkupAnnotation.Comments;
		}
		if (annotation is PdfAttachmentAnnotation)
		{
			PdfAttachmentAnnotation pdfAttachmentAnnotation = annotation as PdfAttachmentAnnotation;
			if (isReview)
			{
				return pdfAttachmentAnnotation.ReviewHistory;
			}
			return pdfAttachmentAnnotation.Comments;
		}
		if (annotation is PdfRubberStampAnnotation)
		{
			PdfRubberStampAnnotation pdfRubberStampAnnotation = annotation as PdfRubberStampAnnotation;
			if (isReview)
			{
				return pdfRubberStampAnnotation.ReviewHistory;
			}
			return pdfRubberStampAnnotation.Comments;
		}
		if (annotation is PdfInkAnnotation)
		{
			PdfInkAnnotation pdfInkAnnotation = annotation as PdfInkAnnotation;
			if (isReview)
			{
				return pdfInkAnnotation.ReviewHistory;
			}
			return pdfInkAnnotation.Comments;
		}
		if (annotation is PdfSoundAnnotation)
		{
			PdfSoundAnnotation pdfSoundAnnotation = annotation as PdfSoundAnnotation;
			if (isReview)
			{
				return pdfSoundAnnotation.ReviewHistory;
			}
			return pdfSoundAnnotation.Comments;
		}
		if (annotation is PdfPolygonAnnotation)
		{
			PdfPolygonAnnotation pdfPolygonAnnotation = annotation as PdfPolygonAnnotation;
			if (isReview)
			{
				return pdfPolygonAnnotation.ReviewHistory;
			}
			return pdfPolygonAnnotation.Comments;
		}
		if (annotation is PdfPolyLineAnnotation)
		{
			PdfPolyLineAnnotation pdfPolyLineAnnotation = annotation as PdfPolyLineAnnotation;
			if (isReview)
			{
				return pdfPolyLineAnnotation.ReviewHistory;
			}
			return pdfPolyLineAnnotation.Comments;
		}
		if (annotation is PdfCircleMeasurementAnnotation)
		{
			PdfCircleMeasurementAnnotation pdfCircleMeasurementAnnotation = annotation as PdfCircleMeasurementAnnotation;
			if (isReview)
			{
				return pdfCircleMeasurementAnnotation.ReviewHistory;
			}
			return pdfCircleMeasurementAnnotation.Comments;
		}
		if (annotation is PdfLineMeasurementAnnotation)
		{
			PdfLineMeasurementAnnotation pdfLineMeasurementAnnotation = annotation as PdfLineMeasurementAnnotation;
			if (isReview)
			{
				return pdfLineMeasurementAnnotation.ReviewHistory;
			}
			return pdfLineMeasurementAnnotation.Comments;
		}
		if (annotation is PdfSquareMeasurementAnnotation)
		{
			PdfSquareMeasurementAnnotation pdfSquareMeasurementAnnotation = annotation as PdfSquareMeasurementAnnotation;
			if (isReview)
			{
				return pdfSquareMeasurementAnnotation.ReviewHistory;
			}
			return pdfSquareMeasurementAnnotation.Comments;
		}
		return null;
	}

	private void DoAddComments(PdfAnnotation annotation)
	{
		PdfPopupAnnotationCollection commentsOrReview = GetCommentsOrReview(annotation, isReview: false);
		if (commentsOrReview == null)
		{
			return;
		}
		commentsOrReview.page = Page;
		for (int i = 0; i < commentsOrReview.Count; i++)
		{
			PdfPopupAnnotation pdfPopupAnnotation = commentsOrReview[i];
			DoAddState(pdfPopupAnnotation);
			if (pdfPopupAnnotation == null)
			{
				continue;
			}
			if (pdfPopupAnnotation.Comments.Count != 0)
			{
				DoAddComments(pdfPopupAnnotation);
			}
			if (pdfPopupAnnotation.ReviewHistory.Count != 0)
			{
				for (int j = 0; j < pdfPopupAnnotation.ReviewHistory.Count; j++)
				{
					DoAddState(pdfPopupAnnotation.ReviewHistory[j]);
				}
			}
		}
	}

	private void DoAddReviewHistory(PdfAnnotation annotation)
	{
		PdfPopupAnnotationCollection commentsOrReview = GetCommentsOrReview(annotation, isReview: true);
		if (commentsOrReview != null)
		{
			commentsOrReview.page = Page;
			for (int i = 0; i < commentsOrReview.Count; i++)
			{
				DoAddState(commentsOrReview[i]);
			}
		}
	}

	private void DoAddState(PdfAnnotation popupAnnoataion)
	{
		if (popupAnnoataion != null)
		{
			popupAnnoataion.SetPage(m_page);
			PdfArray pdfArray = null;
			if (m_page.Dictionary.ContainsKey("Annots"))
			{
				pdfArray = PdfCrossTable.Dereference(m_page.Dictionary["Annots"]) as PdfArray;
			}
			if (pdfArray == null)
			{
				pdfArray = new PdfArray();
			}
			PdfReferenceHolder element = new PdfReferenceHolder(popupAnnoataion);
			if (!pdfArray.Contains(element))
			{
				pdfArray.Add(element);
				m_page.Dictionary.SetProperty("Annots", pdfArray);
			}
		}
	}

	protected override int DoAdd(PdfAnnotation annot)
	{
		int result = -1;
		if (annot == null)
		{
			throw new ArgumentNullException("annotation");
		}
		annot.SetPage(m_page);
		PdfArray pdfArray = null;
		if (m_page.Dictionary.ContainsKey("Annots"))
		{
			pdfArray = PdfCrossTable.Dereference(m_page.Dictionary["Annots"]) as PdfArray;
		}
		if (pdfArray == null)
		{
			pdfArray = new PdfArray();
		}
		PdfReferenceHolder element = new PdfReferenceHolder(annot);
		bool flag = false;
		if (annot.Dictionary != null && annot.Dictionary.ContainsKey("Subtype"))
		{
			PdfName pdfName = PdfCrossTable.Dereference(annot.Dictionary["Subtype"]) as PdfName;
			if (pdfName != null && !m_savePopup)
			{
				flag = pdfName.Value == "Popup";
			}
		}
		if (!annot.m_addingOldAnnotation && !flag && !pdfArray.Contains(element))
		{
			pdfArray.Add(element);
			m_page.Dictionary.SetProperty("Annots", pdfArray);
		}
		if (!annot.unSupportedAnnotation)
		{
			result = base.DoAdd(annot);
		}
		else
		{
			m_page.m_unsupportedAnnotation.Add(annot);
		}
		if (annot != null && Page != null)
		{
			DoAddReviewHistory(annot);
			DoAddComments(annot);
		}
		PdfLoadedDocument pdfLoadedDocument = m_page.Document as PdfLoadedDocument;
		if (pdfLoadedDocument != null && annot is PdfLoadedRedactionAnnotation && !pdfLoadedDocument.m_redactAnnotationCollection.Contains(annot))
		{
			pdfLoadedDocument.m_redactAnnotationCollection.Add(annot);
		}
		if (pdfLoadedDocument != null && !LoadedAnnotation(annot) && m_page.Annotations != null && !m_page.Annotations.m_savePopup)
		{
			AnnotationAddedArgs annotationAddedArgs = new AnnotationAddedArgs();
			annotationAddedArgs.Annotation = annot;
			pdfLoadedDocument.OnAnnotationAdded(annotationAddedArgs);
		}
		return result;
	}

	internal new bool LoadedAnnotation(PdfAnnotation annot)
	{
		if (annot is PdfLoadedAnnotation)
		{
			return true;
		}
		return false;
	}

	internal string GetCorrectName(string name)
	{
		List<string> list = new List<string>();
		foreach (PdfAnnotation item in base.List)
		{
			list.Add(item.Text);
		}
		string text = name;
		int num = 0;
		while (list.IndexOf(text) != -1)
		{
			text = name + num;
			num++;
		}
		return text;
	}

	internal bool IsValidName(string name)
	{
		foreach (PdfAnnotation item in base.List)
		{
			if (item.Text == name)
			{
				return false;
			}
		}
		return true;
	}

	private int GetAnnotationIndex(string text)
	{
		int num = -1;
		bool flag = false;
		foreach (PdfAnnotation item in base.List)
		{
			num++;
			if (item.Name == text)
			{
				flag = true;
				break;
			}
			if (!string.IsNullOrEmpty(item.Name) && item.Name.Split('(')[0] == text)
			{
				return num;
			}
		}
		if (!flag)
		{
			foreach (PdfAnnotation item2 in base.List)
			{
				num++;
				if (item2.Text == text)
				{
					break;
				}
				if (!string.IsNullOrEmpty(item2.Text) && item2.Text.Split('(')[0] == text)
				{
					return num;
				}
			}
		}
		if (!flag && num == base.List.Count - 1 && base.List[base.List.Count - 1] is PdfAnnotation pdfAnnotation3 && pdfAnnotation3.Name != text && pdfAnnotation3.Text != text)
		{
			num = -1;
		}
		return num;
	}

	private PdfAnnotation GetAnnotation(int index)
	{
		PdfDictionary pdfDictionary = m_page.TerminalAnnotation[index];
		PdfCrossTable crossTable = m_page.CrossTable;
		PdfAnnotation pdfAnnotation = null;
		if (pdfDictionary != null && pdfDictionary.ContainsKey("Subtype"))
		{
			PdfName name = PdfLoadedAnnotation.GetValue(pdfDictionary, crossTable, "Subtype", inheritable: true) as PdfName;
			PdfLoadedAnnotationType annotationType = GetAnnotationType(name, pdfDictionary, crossTable);
			if (m_page.Document is PdfLoadedDocument { annotationsTypeToBeIgnored: not null } pdfLoadedDocument && pdfLoadedDocument.annotationsTypeToBeIgnored.Contains(annotationType))
			{
				return null;
			}
			if (PdfCrossTable.Dereference(pdfDictionary["Rect"]) is PdfArray pdfArray)
			{
				RectangleF rectangleF = default(RectangleF);
				if (pdfArray.Elements.Count > 3)
				{
					rectangleF = pdfArray.ToRectangle();
				}
				else
				{
					pdfDictionary["Rect"] = PdfArray.FromRectangle(rectangleF);
				}
				string text = string.Empty;
				if (pdfDictionary.ContainsKey("Contents") && PdfCrossTable.Dereference(pdfDictionary["Contents"]) is PdfString pdfString)
				{
					text = pdfString.Value.ToString();
				}
				switch (annotationType)
				{
				case PdfLoadedAnnotationType.TextWebLinkAnnotation:
					pdfAnnotation = CreateTextWebLinkAnnotation(pdfDictionary, crossTable, text);
					SetAnnotationType(pdfAnnotation, PdfLoadedAnnotationType.TextWebLinkAnnotation);
					break;
				case PdfLoadedAnnotationType.DocumentLinkAnnotation:
					pdfAnnotation = CreateDocumentLinkAnnotation(pdfDictionary, crossTable, rectangleF);
					SetAnnotationType(pdfAnnotation, PdfLoadedAnnotationType.DocumentLinkAnnotation);
					break;
				case PdfLoadedAnnotationType.FileLinkAnnotation:
					if (!pdfDictionary.ContainsKey("A"))
					{
						break;
					}
					if (PdfCrossTable.Dereference(pdfDictionary["A"]) is PdfDictionary pdfDictionary6 && pdfDictionary6.ContainsKey("F"))
					{
						PdfDictionary pdfDictionary7 = PdfCrossTable.Dereference(pdfDictionary6["F"]) as PdfDictionary;
						PdfString pdfString3 = null;
						if (pdfDictionary7 != null && pdfDictionary7.ContainsKey("F"))
						{
							if (PdfCrossTable.Dereference(pdfDictionary7["F"]) is PdfString pdfString4)
							{
								pdfAnnotation = CreateFileLinkAnnotation(pdfDictionary, crossTable, rectangleF, pdfString4.Value);
							}
						}
						else if (pdfDictionary7 != null && pdfDictionary7.ContainsKey("UF") && PdfCrossTable.Dereference(pdfDictionary7["UF"]) is PdfString pdfString5)
						{
							pdfAnnotation = CreateFileLinkAnnotation(pdfDictionary, crossTable, rectangleF, pdfString5.Value);
						}
					}
					if (pdfAnnotation != null)
					{
						SetAnnotationType(pdfAnnotation, PdfLoadedAnnotationType.FileLinkAnnotation);
					}
					break;
				case PdfLoadedAnnotationType.CaretAnnotation:
					pdfAnnotation = CreateCaretAnnotation(pdfDictionary, crossTable, rectangleF);
					SetAnnotationType(pdfAnnotation, PdfLoadedAnnotationType.CaretAnnotation);
					break;
				case PdfLoadedAnnotationType.FileAttachmentAnnotation:
				{
					PdfDictionary pdfDictionary2 = PdfCrossTable.Dereference(pdfDictionary["FS"]) as PdfDictionary;
					PdfString pdfString2 = null;
					if (pdfDictionary2 != null && pdfDictionary2.ContainsKey("F"))
					{
						pdfString2 = pdfDictionary2["F"] as PdfString;
					}
					else if (pdfDictionary2 != null && pdfDictionary2.ContainsKey("UF"))
					{
						pdfString2 = pdfDictionary2["UF"] as PdfString;
					}
					if (pdfString2 == null)
					{
						pdfString2 = new PdfString("");
					}
					pdfAnnotation = CreateFileAttachmentAnnotation(pdfDictionary, crossTable, rectangleF, pdfString2.Value);
					SetAnnotationType(pdfAnnotation, PdfLoadedAnnotationType.FileAttachmentAnnotation);
					break;
				}
				case PdfLoadedAnnotationType.FreeTextAnnotation:
					pdfAnnotation = CreateFreeTextAnnotation(pdfDictionary, crossTable, rectangleF, text);
					SetAnnotationType(pdfAnnotation, PdfLoadedAnnotationType.FreeTextAnnotation);
					break;
				case PdfLoadedAnnotationType.LineAnnotation:
					pdfAnnotation = CreateLineAnnotation(pdfDictionary, crossTable, rectangleF, text);
					SetAnnotationType(pdfAnnotation, PdfLoadedAnnotationType.LineAnnotation);
					break;
				case PdfLoadedAnnotationType.CircleAnnotation:
					pdfAnnotation = CreateCircleAnnotation(pdfDictionary, crossTable, rectangleF, text);
					SetAnnotationType(pdfAnnotation, PdfLoadedAnnotationType.CircleAnnotation);
					break;
				case PdfLoadedAnnotationType.EllipseAnnotation:
					pdfAnnotation = CreateEllipseAnnotation(pdfDictionary, crossTable, rectangleF, text);
					SetAnnotationType(pdfAnnotation, PdfLoadedAnnotationType.EllipseAnnotation);
					break;
				case PdfLoadedAnnotationType.SquareAnnotation:
					pdfAnnotation = CreateSquareAnnotation(pdfDictionary, crossTable, rectangleF, text);
					SetAnnotationType(pdfAnnotation, PdfLoadedAnnotationType.SquareAnnotation);
					break;
				case PdfLoadedAnnotationType.RectangleAnnotation:
					pdfAnnotation = CreateRectangleAnnotation(pdfDictionary, crossTable, rectangleF, text);
					SetAnnotationType(pdfAnnotation, PdfLoadedAnnotationType.RectangleAnnotation);
					break;
				case PdfLoadedAnnotationType.PolygonAnnotation:
					pdfAnnotation = CreatePolygonAnnotation(pdfDictionary, crossTable, rectangleF, text);
					SetAnnotationType(pdfAnnotation, PdfLoadedAnnotationType.PolygonAnnotation);
					break;
				case PdfLoadedAnnotationType.PolyLineAnnotation:
					pdfAnnotation = CreatePolyLineAnnotation(pdfDictionary, crossTable, rectangleF, text);
					SetAnnotationType(pdfAnnotation, PdfLoadedAnnotationType.PolyLineAnnotation);
					break;
				case PdfLoadedAnnotationType.RedactionAnnotation:
					pdfAnnotation = CreateRedactionAnnotation(pdfDictionary, crossTable);
					SetAnnotationType(pdfAnnotation, PdfLoadedAnnotationType.RedactionAnnotation);
					break;
				case PdfLoadedAnnotationType.RichMediaAnnotation:
					if (pdfDictionary.ContainsKey("RichMediaContent"))
					{
						PdfDictionary pdfDictionary2 = PdfCrossTable.Dereference(pdfDictionary["RichMediaContent"]) as PdfDictionary;
						pdfAnnotation = CreateRichMediaAnnotation(pdfDictionary, crossTable, rectangleF);
						if (pdfAnnotation != null)
						{
							SetAnnotationType(pdfAnnotation, PdfLoadedAnnotationType.RichMediaAnnotation);
						}
					}
					break;
				case PdfLoadedAnnotationType.LinkAnnotation:
				{
					PdfString pdfString2 = null;
					if (pdfDictionary.ContainsKey("A"))
					{
						PdfDictionary pdfDictionary3 = new PdfDictionary();
						PdfArray pdfArray2 = new PdfArray();
						if (PdfCrossTable.Dereference(pdfDictionary["A"]) is PdfDictionary pdfDictionary4 && pdfDictionary4.ContainsKey("S"))
						{
							pdfArray2 = PdfCrossTable.Dereference(pdfDictionary4["D"]) as PdfArray;
							PdfName pdfName = PdfCrossTable.Dereference(pdfDictionary4["S"]) as PdfName;
							if (pdfName != null && pdfName.Value == "GoToR")
							{
								if (PdfCrossTable.Dereference(pdfDictionary4["F"]) is PdfString)
								{
									if (PdfCrossTable.Dereference(pdfDictionary4["F"]) is PdfString fileName)
									{
										pdfAnnotation = CreateFileRemoteGoToLinkAnnotation(pdfDictionary, crossTable, fileName, pdfArray2, rectangleF);
									}
								}
								else if (PdfCrossTable.Dereference(pdfDictionary4["F"]) is PdfDictionary && PdfCrossTable.Dereference(pdfDictionary4["F"]) is PdfDictionary pdfDictionary5)
								{
									if (pdfDictionary5.ContainsKey("F"))
									{
										pdfString2 = PdfCrossTable.Dereference(pdfDictionary5["F"]) as PdfString;
									}
									else if (pdfDictionary5.ContainsKey("UF"))
									{
										pdfString2 = PdfCrossTable.Dereference(pdfDictionary5["UF"]) as PdfString;
									}
									if (pdfString2 != null)
									{
										pdfAnnotation = CreateFileRemoteGoToLinkAnnotation(pdfDictionary, crossTable, pdfString2, pdfArray2, rectangleF);
									}
								}
							}
							else if (pdfName != null && pdfName.Value == "URI")
							{
								pdfAnnotation = CreateLinkAnnotation(pdfDictionary, crossTable, rectangleF, text);
							}
						}
					}
					else
					{
						pdfAnnotation = CreateLinkAnnotation(pdfDictionary, crossTable, rectangleF, text);
					}
					if (pdfAnnotation != null)
					{
						SetAnnotationType(pdfAnnotation, PdfLoadedAnnotationType.LinkAnnotation);
					}
					break;
				}
				case PdfLoadedAnnotationType.Highlight:
				case PdfLoadedAnnotationType.Underline:
				case PdfLoadedAnnotationType.StrikeOut:
				case PdfLoadedAnnotationType.Squiggly:
					pdfAnnotation = CreateMarkupAnnotation(pdfDictionary, crossTable, rectangleF);
					if (annotationType == PdfLoadedAnnotationType.Highlight)
					{
						SetAnnotationType(pdfAnnotation, PdfLoadedAnnotationType.Highlight);
					}
					if (annotationType == PdfLoadedAnnotationType.Squiggly)
					{
						SetAnnotationType(pdfAnnotation, PdfLoadedAnnotationType.Squiggly);
					}
					if (annotationType == PdfLoadedAnnotationType.StrikeOut)
					{
						SetAnnotationType(pdfAnnotation, PdfLoadedAnnotationType.StrikeOut);
					}
					if (annotationType == PdfLoadedAnnotationType.Underline)
					{
						SetAnnotationType(pdfAnnotation, PdfLoadedAnnotationType.Underline);
					}
					break;
				case PdfLoadedAnnotationType.MovieAnnotation:
					pdfAnnotation = CreateMovieAnnotation(pdfDictionary, crossTable, rectangleF);
					pdfAnnotation.unSupportedAnnotation = true;
					SetAnnotationType(pdfAnnotation, PdfLoadedAnnotationType.MovieAnnotation);
					break;
				case PdfLoadedAnnotationType.PopupAnnotation:
					pdfAnnotation = CreatePopupAnnotation(pdfDictionary, crossTable, rectangleF, text);
					SetAnnotationType(pdfAnnotation, PdfLoadedAnnotationType.PopupAnnotation);
					break;
				case PdfLoadedAnnotationType.PrinterMarkAnnotation:
					pdfAnnotation = CreatePrinterMarkAnnotation(pdfDictionary, crossTable, rectangleF);
					pdfAnnotation.unSupportedAnnotation = true;
					SetAnnotationType(pdfAnnotation, PdfLoadedAnnotationType.PrinterMarkAnnotation);
					break;
				case PdfLoadedAnnotationType.RubberStampAnnotation:
					pdfAnnotation = CreateRubberStampAnnotation(pdfDictionary, crossTable, rectangleF, text);
					SetAnnotationType(pdfAnnotation, PdfLoadedAnnotationType.RubberStampAnnotation);
					break;
				case PdfLoadedAnnotationType.ScreenAnnotation:
					pdfAnnotation = CreateScreenAnnotation(pdfDictionary, crossTable, rectangleF);
					pdfAnnotation.unSupportedAnnotation = true;
					SetAnnotationType(pdfAnnotation, PdfLoadedAnnotationType.ScreenAnnotation);
					break;
				case PdfLoadedAnnotationType.SoundAnnotation:
				{
					PdfDictionary pdfDictionary2 = PdfCrossTable.Dereference(pdfDictionary["Sound"]) as PdfDictionary;
					pdfAnnotation = CreateSoundAnnotation(pdfDictionary, crossTable, rectangleF);
					SetAnnotationType(pdfAnnotation, PdfLoadedAnnotationType.SoundAnnotation);
					break;
				}
				case PdfLoadedAnnotationType.TextAnnotation:
					pdfAnnotation = CreateTextAnnotation(pdfDictionary, crossTable);
					SetAnnotationType(pdfAnnotation, PdfLoadedAnnotationType.TextAnnotation);
					break;
				case PdfLoadedAnnotationType.TextMarkupAnnotation:
					pdfAnnotation = CreateTextMarkupAnnotation(pdfDictionary, crossTable);
					SetAnnotationType(pdfAnnotation, PdfLoadedAnnotationType.TextMarkupAnnotation);
					break;
				case PdfLoadedAnnotationType.TrapNetworkAnnotation:
					pdfAnnotation = CreateTrapNetworkAnnotation(pdfDictionary, crossTable, rectangleF);
					pdfAnnotation.unSupportedAnnotation = true;
					SetAnnotationType(pdfAnnotation, PdfLoadedAnnotationType.TrapNetworkAnnotation);
					break;
				case PdfLoadedAnnotationType.WatermarkAnnotation:
					pdfAnnotation = CreateWatermarkAnnotation(pdfDictionary, crossTable, rectangleF);
					SetAnnotationType(pdfAnnotation, PdfLoadedAnnotationType.WatermarkAnnotation);
					break;
				case PdfLoadedAnnotationType.WidgetAnnotation:
					pdfAnnotation = CreateWidgetAnnotation(pdfDictionary, crossTable, rectangleF);
					SetAnnotationType(pdfAnnotation, PdfLoadedAnnotationType.WidgetAnnotation);
					break;
				case PdfLoadedAnnotationType.InkAnnotation:
					pdfAnnotation = CreateInkAnnotation(pdfDictionary, crossTable, rectangleF);
					SetAnnotationType(pdfAnnotation, PdfLoadedAnnotationType.InkAnnotation);
					break;
				}
				if (pdfAnnotation is PdfLoadedAnnotation pdfLoadedAnnotation)
				{
					pdfLoadedAnnotation.BeforeNameChanges += ldAnnotation_NameChanded;
				}
				return pdfAnnotation;
			}
			return pdfAnnotation;
		}
		return pdfAnnotation;
	}

	private void SetAnnotationType(PdfAnnotation annotation, PdfLoadedAnnotationType type)
	{
		(annotation as PdfLoadedAnnotation).m_type = type;
	}

	internal PdfLoadedAnnotationType GetAnnotationType(PdfName name, PdfDictionary dictionary, PdfCrossTable crossTable)
	{
		string value = name.Value;
		PdfLoadedAnnotationType result = PdfLoadedAnnotationType.Null;
		switch (value.ToLower())
		{
		case "sound":
			result = PdfLoadedAnnotationType.SoundAnnotation;
			break;
		case "popup":
		case "text":
			result = PdfLoadedAnnotationType.PopupAnnotation;
			break;
		case "link":
		{
			PdfDictionary pdfDictionary = null;
			if (dictionary.ContainsKey("A"))
			{
				pdfDictionary = PdfCrossTable.Dereference(dictionary["A"]) as PdfDictionary;
			}
			if (pdfDictionary != null && pdfDictionary.ContainsKey("S"))
			{
				name = PdfCrossTable.Dereference(pdfDictionary["S"]) as PdfName;
				if (name != null)
				{
					PdfArray arr = PdfCrossTable.Dereference(dictionary["Border"]) as PdfArray;
					bool flag = FindAnnotation(arr);
					if (name.Value == "URI")
					{
						result = (flag ? PdfLoadedAnnotationType.TextWebLinkAnnotation : PdfLoadedAnnotationType.LinkAnnotation);
					}
					else if (name.Value == "Launch")
					{
						result = PdfLoadedAnnotationType.FileLinkAnnotation;
					}
					else if (name.Value == "GoToR")
					{
						result = PdfLoadedAnnotationType.LinkAnnotation;
					}
					else if (name.Value == "GoTo")
					{
						result = PdfLoadedAnnotationType.DocumentLinkAnnotation;
					}
				}
			}
			else if (dictionary.ContainsKey("Subtype"))
			{
				PdfName pdfName = PdfCrossTable.Dereference(dictionary["Subtype"]) as PdfName;
				if (pdfName != null && pdfName.Value == "Link")
				{
					result = PdfLoadedAnnotationType.DocumentLinkAnnotation;
				}
			}
			break;
		}
		case "fileattachment":
			result = PdfLoadedAnnotationType.FileAttachmentAnnotation;
			break;
		case "line":
			result = PdfLoadedAnnotationType.LineAnnotation;
			break;
		case "circle":
			if (PdfLoadedAnnotation.GetValue(dictionary, crossTable, "Rect", inheritable: true) is PdfArray pdfArray2)
			{
				RectangleF rectangleF2 = pdfArray2.ToRectangle();
				result = ((rectangleF2.Width == rectangleF2.Height) ? PdfLoadedAnnotationType.CircleAnnotation : PdfLoadedAnnotationType.EllipseAnnotation);
			}
			break;
		case "square":
			if (PdfLoadedAnnotation.GetValue(dictionary, crossTable, "Rect", inheritable: true) is PdfArray pdfArray)
			{
				RectangleF rectangleF = pdfArray.ToRectangle();
				result = ((rectangleF.Width == rectangleF.Height) ? PdfLoadedAnnotationType.SquareAnnotation : PdfLoadedAnnotationType.RectangleAnnotation);
			}
			break;
		case "polygon":
			result = PdfLoadedAnnotationType.PolygonAnnotation;
			break;
		case "redact":
			result = PdfLoadedAnnotationType.RedactionAnnotation;
			break;
		case "polyline":
			result = PdfLoadedAnnotationType.PolyLineAnnotation;
			break;
		case "widget":
			result = PdfLoadedAnnotationType.WidgetAnnotation;
			break;
		case "highlight":
			result = PdfLoadedAnnotationType.Highlight;
			break;
		case "underline":
			result = PdfLoadedAnnotationType.Underline;
			break;
		case "strikeout":
			result = PdfLoadedAnnotationType.StrikeOut;
			break;
		case "squiggly":
			result = PdfLoadedAnnotationType.Squiggly;
			break;
		case "stamp":
			result = PdfLoadedAnnotationType.RubberStampAnnotation;
			break;
		case "ink":
			result = PdfLoadedAnnotationType.InkAnnotation;
			break;
		case "freetext":
			result = PdfLoadedAnnotationType.FreeTextAnnotation;
			break;
		case "caret":
			result = PdfLoadedAnnotationType.CaretAnnotation;
			break;
		case "watermark":
			result = PdfLoadedAnnotationType.WatermarkAnnotation;
			break;
		case "screen":
			result = PdfLoadedAnnotationType.ScreenAnnotation;
			break;
		case "3d":
			result = PdfLoadedAnnotationType.MovieAnnotation;
			break;
		case "richmedia":
			result = PdfLoadedAnnotationType.RichMediaAnnotation;
			break;
		}
		return result;
	}

	private PdfAnnotation CreateFileRemoteGoToLinkAnnotation(PdfDictionary dictionary, PdfCrossTable crossTable, PdfString fileName, PdfArray destination, RectangleF rect)
	{
		PdfLoadedFileLinkAnnotation pdfLoadedFileLinkAnnotation = new PdfLoadedFileLinkAnnotation(dictionary, crossTable, destination, rect, fileName.Value.ToString());
		pdfLoadedFileLinkAnnotation.SetPage(m_page);
		(pdfLoadedFileLinkAnnotation as PdfLoadedAnnotation).m_loadedpage = Page;
		return pdfLoadedFileLinkAnnotation;
	}

	private PdfAnnotation CreateTextWebLinkAnnotation(PdfDictionary dictionary, PdfCrossTable crossTable, string text)
	{
		PdfLoadedTextWebLinkAnnotation pdfLoadedTextWebLinkAnnotation = new PdfLoadedTextWebLinkAnnotation(dictionary, crossTable, text);
		pdfLoadedTextWebLinkAnnotation.SetPage(m_page);
		(pdfLoadedTextWebLinkAnnotation as PdfLoadedAnnotation).m_loadedpage = Page;
		return pdfLoadedTextWebLinkAnnotation;
	}

	private PdfAnnotation CreateDocumentLinkAnnotation(PdfDictionary dictionary, PdfCrossTable crossTable, RectangleF rect)
	{
		PdfLoadedDocumentLinkAnnotation pdfLoadedDocumentLinkAnnotation = new PdfLoadedDocumentLinkAnnotation(dictionary, crossTable, rect);
		pdfLoadedDocumentLinkAnnotation.SetPage(m_page);
		(pdfLoadedDocumentLinkAnnotation as PdfLoadedAnnotation).m_loadedpage = Page;
		return pdfLoadedDocumentLinkAnnotation;
	}

	private PdfAnnotation CreateFileLinkAnnotation(PdfDictionary dictionary, PdfCrossTable crossTable, RectangleF rect, string filename)
	{
		PdfLoadedFileLinkAnnotation pdfLoadedFileLinkAnnotation = new PdfLoadedFileLinkAnnotation(dictionary, crossTable, rect, filename);
		pdfLoadedFileLinkAnnotation.SetPage(m_page);
		(pdfLoadedFileLinkAnnotation as PdfLoadedAnnotation).m_loadedpage = Page;
		return pdfLoadedFileLinkAnnotation;
	}

	private PdfAnnotation CreateWidgetAnnotation(PdfDictionary dictionary, PdfCrossTable crossTable, RectangleF rect)
	{
		PdfLoadedWidgetAnnotation pdfLoadedWidgetAnnotation = new PdfLoadedWidgetAnnotation(dictionary, crossTable, rect);
		pdfLoadedWidgetAnnotation.SetPage(m_page);
		(pdfLoadedWidgetAnnotation as PdfLoadedWidgetAnnotation).m_loadedpage = Page;
		return pdfLoadedWidgetAnnotation;
	}

	private PdfAnnotation CreateInkAnnotation(PdfDictionary dictionary, PdfCrossTable crossTable, RectangleF rect)
	{
		PdfLoadedInkAnnotation pdfLoadedInkAnnotation = new PdfLoadedInkAnnotation(dictionary, crossTable, rect);
		pdfLoadedInkAnnotation.SetPage(m_page);
		(pdfLoadedInkAnnotation as PdfLoadedAnnotation).m_loadedpage = Page;
		return pdfLoadedInkAnnotation;
	}

	private PdfAnnotation CreateWatermarkAnnotation(PdfDictionary dictionary, PdfCrossTable crossTable, RectangleF rect)
	{
		PdfLoadedWatermarkAnnotation pdfLoadedWatermarkAnnotation = new PdfLoadedWatermarkAnnotation(dictionary, crossTable, rect);
		pdfLoadedWatermarkAnnotation.SetPage(m_page);
		(pdfLoadedWatermarkAnnotation as PdfLoadedAnnotation).m_loadedpage = Page;
		return pdfLoadedWatermarkAnnotation;
	}

	private PdfAnnotation CreateRichMediaAnnotation(PdfDictionary dictionary, PdfCrossTable crossTable, RectangleF rect)
	{
		PdfLoadedRichMediaAnnotation pdfLoadedRichMediaAnnotation = new PdfLoadedRichMediaAnnotation(dictionary, crossTable, rect);
		pdfLoadedRichMediaAnnotation.SetPage(m_page);
		(pdfLoadedRichMediaAnnotation as PdfLoadedAnnotation).m_loadedpage = Page;
		return pdfLoadedRichMediaAnnotation;
	}

	private PdfAnnotation CreateTrapNetworkAnnotation(PdfDictionary dictionary, PdfCrossTable crossTable, RectangleF rect)
	{
		PdfLoadedTextMarkupAnnotation pdfLoadedTextMarkupAnnotation = new PdfLoadedTextMarkupAnnotation(dictionary, crossTable, rect);
		pdfLoadedTextMarkupAnnotation.SetPage(m_page);
		(pdfLoadedTextMarkupAnnotation as PdfLoadedAnnotation).m_loadedpage = Page;
		return pdfLoadedTextMarkupAnnotation;
	}

	private PdfAnnotation CreateTextMarkupAnnotation(PdfDictionary dictionary, PdfCrossTable crossTable)
	{
		PdfTextMarkupAnnotation pdfTextMarkupAnnotation = new PdfTextMarkupAnnotation();
		pdfTextMarkupAnnotation.SetPage(m_page);
		//(pdfTextMarkupAnnotation as PdfLoadedAnnotation).m_loadedpage = Page;
		return pdfTextMarkupAnnotation;
	}

	private PdfAnnotation CreateTextAnnotation(PdfDictionary dictionary, PdfCrossTable crossTable)
	{
		PdfTextMarkupAnnotation pdfTextMarkupAnnotation = new PdfTextMarkupAnnotation();
		pdfTextMarkupAnnotation.SetPage(m_page);
		//(pdfTextMarkupAnnotation as PdfLoadedAnnotation).m_loadedpage = Page;
		return pdfTextMarkupAnnotation;
	}

	private PdfAnnotation CreateSoundAnnotation(PdfDictionary dictionary, PdfCrossTable crossTable, RectangleF rect)
	{
		PdfLoadedSoundAnnotation pdfLoadedSoundAnnotation = new PdfLoadedSoundAnnotation(dictionary, crossTable, rect);
		pdfLoadedSoundAnnotation.SetPage(m_page);
		pdfLoadedSoundAnnotation.m_loadedpage = Page;
		return pdfLoadedSoundAnnotation;
	}

	private PdfAnnotation CreateScreenAnnotation(PdfDictionary dictionary, PdfCrossTable crossTable, RectangleF rect)
	{
		PdfLoadedTextMarkupAnnotation pdfLoadedTextMarkupAnnotation = new PdfLoadedTextMarkupAnnotation(dictionary, crossTable, rect);
		pdfLoadedTextMarkupAnnotation.SetPage(m_page);
		(pdfLoadedTextMarkupAnnotation as PdfLoadedAnnotation).m_loadedpage = Page;
		return pdfLoadedTextMarkupAnnotation;
	}

	private PdfAnnotation CreateRubberStampAnnotation(PdfDictionary dictionary, PdfCrossTable crossTable, RectangleF rect, string text)
	{
		PdfLoadedRubberStampAnnotation pdfLoadedRubberStampAnnotation = new PdfLoadedRubberStampAnnotation(dictionary, crossTable, rect, text);
		pdfLoadedRubberStampAnnotation.SetPage(m_page);
		(pdfLoadedRubberStampAnnotation as PdfLoadedAnnotation).m_loadedpage = Page;
		return pdfLoadedRubberStampAnnotation;
	}

	private PdfAnnotation CreatePrinterMarkAnnotation(PdfDictionary dictionary, PdfCrossTable crossTable, RectangleF rect)
	{
		PdfLoadedTextMarkupAnnotation pdfLoadedTextMarkupAnnotation = new PdfLoadedTextMarkupAnnotation(dictionary, crossTable, rect);
		pdfLoadedTextMarkupAnnotation.SetPage(m_page);
		(pdfLoadedTextMarkupAnnotation as PdfLoadedAnnotation).m_loadedpage = Page;
		return pdfLoadedTextMarkupAnnotation;
	}

	private PdfAnnotation CreatePopupAnnotation(PdfDictionary dictionary, PdfCrossTable crossTable, RectangleF rect, string text)
	{
		PdfLoadedPopupAnnotation pdfLoadedPopupAnnotation = new PdfLoadedPopupAnnotation(dictionary, crossTable, rect, text);
		pdfLoadedPopupAnnotation.SetPage(m_page);
		pdfLoadedPopupAnnotation.m_loadedpage = Page;
		return pdfLoadedPopupAnnotation;
	}

	private PdfAnnotation CreateMovieAnnotation(PdfDictionary dictionary, PdfCrossTable crossTable, RectangleF rect)
	{
		PdfLoadedTextMarkupAnnotation pdfLoadedTextMarkupAnnotation = new PdfLoadedTextMarkupAnnotation(dictionary, crossTable, rect);
		pdfLoadedTextMarkupAnnotation.SetPage(m_page);
		(pdfLoadedTextMarkupAnnotation as PdfLoadedAnnotation).m_loadedpage = Page;
		return pdfLoadedTextMarkupAnnotation;
	}

	private PdfAnnotation CreateMarkupAnnotation(PdfDictionary dictionary, PdfCrossTable crossTable, RectangleF rect)
	{
		PdfLoadedTextMarkupAnnotation pdfLoadedTextMarkupAnnotation = new PdfLoadedTextMarkupAnnotation(dictionary, crossTable, rect);
		pdfLoadedTextMarkupAnnotation.SetPage(m_page);
		pdfLoadedTextMarkupAnnotation.m_loadedpage = Page;
		return pdfLoadedTextMarkupAnnotation;
	}

	private PdfAnnotation CreateLinkAnnotation(PdfDictionary dictionary, PdfCrossTable crossTable, RectangleF rect, string text)
	{
		PdfLoadedUriAnnotation pdfLoadedUriAnnotation = new PdfLoadedUriAnnotation(dictionary, crossTable, rect, text);
		pdfLoadedUriAnnotation.SetPage(m_page);
		pdfLoadedUriAnnotation.m_loadedpage = Page;
		return pdfLoadedUriAnnotation;
	}

	private PdfAnnotation CreateLineAnnotation(PdfDictionary dictionary, PdfCrossTable crossTable, RectangleF rect, string text)
	{
		PdfLoadedLineAnnotation pdfLoadedLineAnnotation = new PdfLoadedLineAnnotation(dictionary, crossTable, rect, text);
		pdfLoadedLineAnnotation.SetPage(m_page);
		pdfLoadedLineAnnotation.m_loadedpage = Page;
		return pdfLoadedLineAnnotation;
	}

	private PdfAnnotation CreateCircleAnnotation(PdfDictionary dictionary, PdfCrossTable crossTable, RectangleF rect, string text)
	{
		PdfLoadedCircleAnnotation pdfLoadedCircleAnnotation = new PdfLoadedCircleAnnotation(dictionary, crossTable, rect, text);
		pdfLoadedCircleAnnotation.SetPage(m_page);
		pdfLoadedCircleAnnotation.m_loadedpage = Page;
		return pdfLoadedCircleAnnotation;
	}

	private PdfAnnotation CreateEllipseAnnotation(PdfDictionary dictionary, PdfCrossTable crossTable, RectangleF rectangle, string text)
	{
		PdfLoadedEllipseAnnotation pdfLoadedEllipseAnnotation = new PdfLoadedEllipseAnnotation(dictionary, crossTable, rectangle, text);
		pdfLoadedEllipseAnnotation.SetPage(m_page);
		pdfLoadedEllipseAnnotation.m_loadedpage = Page;
		return pdfLoadedEllipseAnnotation;
	}

	private PdfAnnotation CreateSquareAnnotation(PdfDictionary dictionary, PdfCrossTable crossTable, RectangleF rectangle, string text)
	{
		PdfLoadedSquareAnnotation pdfLoadedSquareAnnotation = new PdfLoadedSquareAnnotation(dictionary, crossTable, rectangle, text);
		pdfLoadedSquareAnnotation.SetPage(m_page);
		pdfLoadedSquareAnnotation.m_loadedpage = Page;
		return pdfLoadedSquareAnnotation;
	}

	private PdfAnnotation CreateRectangleAnnotation(PdfDictionary dictionary, PdfCrossTable crossTable, RectangleF rectangle, string text)
	{
		PdfLoadedRectangleAnnotation pdfLoadedRectangleAnnotation = new PdfLoadedRectangleAnnotation(dictionary, crossTable, rectangle, text);
		pdfLoadedRectangleAnnotation.SetPage(m_page);
		pdfLoadedRectangleAnnotation.m_loadedpage = Page;
		return pdfLoadedRectangleAnnotation;
	}

	private PdfAnnotation CreatePolygonAnnotation(PdfDictionary dictionary, PdfCrossTable crossTable, RectangleF rectangle, string text)
	{
		PdfLoadedPolygonAnnotation pdfLoadedPolygonAnnotation = new PdfLoadedPolygonAnnotation(dictionary, crossTable, rectangle, text);
		pdfLoadedPolygonAnnotation.SetPage(m_page);
		pdfLoadedPolygonAnnotation.m_loadedpage = Page;
		return pdfLoadedPolygonAnnotation;
	}

	private PdfAnnotation CreatePolyLineAnnotation(PdfDictionary dictionary, PdfCrossTable crossTable, RectangleF rectangle, string text)
	{
		PdfLoadedPolyLineAnnotation pdfLoadedPolyLineAnnotation = new PdfLoadedPolyLineAnnotation(dictionary, crossTable, rectangle, text);
		pdfLoadedPolyLineAnnotation.SetPage(m_page);
		pdfLoadedPolyLineAnnotation.m_loadedpage = Page;
		return pdfLoadedPolyLineAnnotation;
	}

	private PdfAnnotation CreateFreeTextAnnotation(PdfDictionary dictionary, PdfCrossTable crossTable, RectangleF rect, string text)
	{
		PdfLoadedFreeTextAnnotation pdfLoadedFreeTextAnnotation = new PdfLoadedFreeTextAnnotation(dictionary, crossTable, rect, text);
		pdfLoadedFreeTextAnnotation.SetPage(m_page);
		(pdfLoadedFreeTextAnnotation as PdfLoadedAnnotation).m_loadedpage = Page;
		return pdfLoadedFreeTextAnnotation;
	}

	private PdfAnnotation CreateRedactionAnnotation(PdfDictionary dictionary, PdfCrossTable crossTable)
	{
		PdfLoadedRedactionAnnotation pdfLoadedRedactionAnnotation = new PdfLoadedRedactionAnnotation(dictionary, crossTable);
		pdfLoadedRedactionAnnotation.SetPage(m_page);
		pdfLoadedRedactionAnnotation.m_loadedpage = Page;
		return pdfLoadedRedactionAnnotation;
	}

	private PdfAnnotation CreateFileAttachmentAnnotation(PdfDictionary dictionary, PdfCrossTable crossTable, RectangleF rect, string filename)
	{
		PdfLoadedAttachmentAnnotation pdfLoadedAttachmentAnnotation = new PdfLoadedAttachmentAnnotation(dictionary, crossTable, rect, filename);
		pdfLoadedAttachmentAnnotation.SetPage(m_page);
		pdfLoadedAttachmentAnnotation.m_loadedpage = Page;
		return pdfLoadedAttachmentAnnotation;
	}

	private PdfAnnotation CreateCaretAnnotation(PdfDictionary dictionary, PdfCrossTable crossTable, RectangleF rect)
	{
		PdfLoadedTextMarkupAnnotation pdfLoadedTextMarkupAnnotation = new PdfLoadedTextMarkupAnnotation(dictionary, crossTable, rect);
		pdfLoadedTextMarkupAnnotation.SetPage(m_page);
		(pdfLoadedTextMarkupAnnotation as PdfLoadedAnnotation).m_loadedpage = Page;
		return pdfLoadedTextMarkupAnnotation;
	}

	protected override void DoInsert(int index, PdfAnnotation annot)
	{
		if (index < 0 || index > base.List.Count)
		{
			throw new IndexOutOfRangeException();
		}
		if (annot == null)
		{
			throw new ArgumentNullException("annotation");
		}
		annot.SetPage(m_page);
		if (!(annot is PdfLoadedAnnotation))
		{
			PdfArray pdfArray = null;
			if (m_page.Dictionary.ContainsKey("Annots"))
			{
				pdfArray = m_page.CrossTable.GetObject(m_page.Dictionary["Annots"]) as PdfArray;
			}
			if (pdfArray == null)
			{
				pdfArray = new PdfArray();
			}
			pdfArray.Insert(index, new PdfReferenceHolder(annot));
			m_page.Dictionary.SetProperty("Annots", pdfArray);
		}
		if (m_page.Document is PdfLoadedDocument pdfLoadedDocument && annot is PdfLoadedRedactionAnnotation && !pdfLoadedDocument.m_redactAnnotationCollection.Contains(annot))
		{
			pdfLoadedDocument.m_redactAnnotationCollection.Add(annot);
		}
		base.DoInsert(index, annot);
	}

	protected override void DoClear()
	{
		int i = 0;
		for (int count = base.List.Count; i < count; i++)
		{
			if (base.List[i] is PdfAnnotation annot)
			{
				m_page.RemoveFromDictionaries(annot);
			}
		}
		PdfLoadedDocument pdfLoadedDocument = null;
		if (m_page != null)
		{
			pdfLoadedDocument = m_page.Document as PdfLoadedDocument;
		}
		if (pdfLoadedDocument != null && pdfLoadedDocument.m_redactAnnotationCollection != null)
		{
			pdfLoadedDocument.m_redactAnnotationCollection.Clear();
		}
	}

	protected override void DoRemoveAt(int index)
	{
		if (index < 0 || index > base.List.Count)
		{
			throw new IndexOutOfRangeException();
		}
		PdfAnnotation pdfAnnotation = base.List[index] as PdfAnnotation;
		if (pdfAnnotation != null)
		{
			m_page.RemoveFromDictionaries(pdfAnnotation);
		}
		PdfLoadedDocument pdfLoadedDocument = null;
		if (m_page != null)
		{
			pdfLoadedDocument = m_page.Document as PdfLoadedDocument;
		}
		if (pdfLoadedDocument != null)
		{
			AnnotationRemovedArgs annotationRemovedArgs = new AnnotationRemovedArgs();
			annotationRemovedArgs.Annotation = pdfAnnotation;
			pdfLoadedDocument.OnAnnotationRemoved(annotationRemovedArgs);
		}
		if (pdfAnnotation is PdfLoadedRedactionAnnotation && pdfLoadedDocument != null && pdfLoadedDocument.m_redactAnnotationCollection.Contains(pdfAnnotation))
		{
			pdfLoadedDocument.m_redactAnnotationCollection.Remove(pdfAnnotation);
		}
		base.DoRemoveAt(index);
	}

	protected override void DoRemove(PdfAnnotation annot)
	{
		if (annot == null)
		{
			throw new ArgumentNullException("annotation");
		}
		if (!m_page.isFlatten)
		{
			m_page.RemoveFromDictionaries(annot);
		}
		PdfLoadedDocument pdfLoadedDocument = null;
		if (m_page != null)
		{
			pdfLoadedDocument = m_page.Document as PdfLoadedDocument;
		}
		if (pdfLoadedDocument != null)
		{
			AnnotationRemovedArgs annotationRemovedArgs = new AnnotationRemovedArgs();
			annotationRemovedArgs.Annotation = annot;
			pdfLoadedDocument.OnAnnotationRemoved(annotationRemovedArgs);
		}
		if (annot is PdfLoadedRedactionAnnotation && pdfLoadedDocument != null && pdfLoadedDocument.m_redactAnnotationCollection.Contains(annot))
		{
			pdfLoadedDocument.m_redactAnnotationCollection.Remove(annot);
		}
		base.DoRemove(annot);
	}

	internal bool FindAnnotation(PdfArray arr)
	{
		if (arr == null)
		{
			return false;
		}
		for (int i = 0; i < arr.Count; i++)
		{
			if (arr[i] is PdfArray)
			{
				PdfArray pdfArray = arr[i] as PdfArray;
				for (int j = 0; j < pdfArray.Count; j++)
				{
					PdfNumber pdfNumber = pdfArray[j] as PdfNumber;
					int num = 0;
					if (pdfNumber != null)
					{
						num = pdfNumber.IntValue;
					}
					if (num > 0)
					{
						return false;
					}
				}
			}
			else
			{
				int num2 = 0;
				if (arr[i] is PdfNumber pdfNumber2)
				{
					num2 = pdfNumber2.IntValue;
				}
				if (num2 > 0)
				{
					return false;
				}
			}
		}
		return true;
	}

	internal PdfArray Rearrange(PdfReference reference, int tabIndex, int index)
	{
		PdfArray pdfArray = m_page.CrossTable.GetObject(m_page.Dictionary["Annots"]) as PdfArray;
		if (pdfArray != null)
		{
			if (tabIndex > pdfArray.Count)
			{
				tabIndex = 0;
			}
			if (index >= pdfArray.Count)
			{
				index = m_page.AnnotsReference.IndexOf(reference);
			}
			PdfReferenceHolder pdfReferenceHolder = pdfArray.Elements[index] as PdfReferenceHolder;
			if (pdfReferenceHolder != null && pdfReferenceHolder.Object is PdfDictionary pdfDictionary && pdfDictionary.ContainsKey("Parent"))
			{
				PdfReferenceHolder pdfReferenceHolder2 = pdfDictionary["Parent"] as PdfReferenceHolder;
				if (pdfReferenceHolder.Reference == reference || (pdfReferenceHolder2 != null && reference == pdfReferenceHolder2.Reference))
				{
					IPdfPrimitive value = pdfArray[index];
					pdfArray.Elements[index] = pdfArray[tabIndex];
					pdfArray.Elements[tabIndex] = value;
				}
			}
		}
		return pdfArray;
	}
}
