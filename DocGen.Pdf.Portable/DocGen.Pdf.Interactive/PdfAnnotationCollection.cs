using System;
using System.Collections.Generic;
using DocGen.Pdf.IO;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public class PdfAnnotationCollection : PdfCollection, IPdfWrapper
{
	private string AlreadyExistsAnnotationError = "This annotatation had been already added to page";

	private string MissingAnnotationException = "Annotation is not contained in collection.";

	private PdfPage m_page;

	private PdfArray m_annotations = new PdfArray();

	private Dictionary<PdfDictionary, PdfAnnotation> m_popupCollection = new Dictionary<PdfDictionary, PdfAnnotation>();

	internal bool m_savePopup;

	public virtual PdfAnnotation this[int index]
	{
		get
		{
			if (index < 0 || index > base.Count - 1)
			{
				throw new ArgumentOutOfRangeException("index", "Index is out of range.");
			}
			return (PdfAnnotation)base.List[index];
		}
	}

	internal PdfArray Annotations
	{
		get
		{
			return m_annotations;
		}
		set
		{
			m_annotations = value;
		}
	}

	IPdfPrimitive IPdfWrapper.Element => m_annotations;

	public PdfAnnotationCollection()
	{
	}

	public PdfAnnotationCollection(PdfPage page)
	{
		if (page == null)
		{
			throw new ArgumentNullException("page");
		}
		m_page = page;
	}

	public virtual int Add(PdfAnnotation annotation)
	{
		if (annotation == null)
		{
			throw new ArgumentNullException("annotation");
		}
		SetPrint(annotation);
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
		commentsOrReview.page = m_page;
		for (int i = 0; i < commentsOrReview.Count; i++)
		{
			PdfPopupAnnotation pdfPopupAnnotation = commentsOrReview[i];
			if (pdfPopupAnnotation == null)
			{
				continue;
			}
			DoAddState(pdfPopupAnnotation);
			if (pdfPopupAnnotation.Comments.Count > 0)
			{
				DoAddComments(pdfPopupAnnotation);
			}
			if (pdfPopupAnnotation.ReviewHistory.Count > 0)
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
			commentsOrReview.page = m_page;
			for (int i = 0; i < commentsOrReview.Count; i++)
			{
				DoAddState(commentsOrReview[i]);
			}
		}
	}

	public void Clear()
	{
		DoClear();
		for (int num = base.Count - 1; num >= 0; num--)
		{
			RemoveAt(num);
		}
		base.List.Clear();
	}

	public bool Contains(PdfAnnotation annotation)
	{
		if (annotation == null)
		{
			throw new ArgumentNullException("annotation");
		}
		return base.List.Contains(annotation);
	}

	public int IndexOf(PdfAnnotation annotation)
	{
		if (annotation == null)
		{
			throw new ArgumentNullException("annotation");
		}
		return base.List.IndexOf(annotation);
	}

	public void Insert(int index, PdfAnnotation annotation)
	{
		DoInsert(index, annotation);
	}

	public void RemoveAt(int index)
	{
		if (index < 0 || index > base.Count - 1)
		{
			throw new ArgumentOutOfRangeException("index", "Index is out of range.");
		}
		RemoveAnnotationAt(index);
	}

	public void Remove(PdfAnnotation annot)
	{
		if (annot == null)
		{
			throw new ArgumentNullException("annotation");
		}
		DoRemove(annot);
	}

	public void SetPrint(PdfAnnotation annot)
	{
		if (m_page.Document != null && (m_page.Document.Conformance == PdfConformanceLevel.Pdf_A1B || m_page.Document.Conformance == PdfConformanceLevel.Pdf_A1A || m_page.Document.Conformance == PdfConformanceLevel.Pdf_A2B || m_page.Document.Conformance == PdfConformanceLevel.Pdf_A3B || m_page.Document.Conformance == PdfConformanceLevel.Pdf_A4 || m_page.Document.Conformance == PdfConformanceLevel.Pdf_A4E || m_page.Document.Conformance == PdfConformanceLevel.Pdf_A4F))
		{
			annot.Dictionary.SetNumber("F", 4);
		}
	}

	private int AddAnnotation(PdfAnnotation annotation)
	{
		annotation.SetPage(m_page);
		base.List.Add(annotation);
		int result = base.List.Count - 1;
		m_annotations.Add(new PdfReferenceHolder(annotation));
		return result;
	}

	private void InsertAnnotation(int index, PdfAnnotation annotation)
	{
		annotation.SetPage(m_page);
		base.List.Insert(index, annotation);
		m_annotations.Insert(index, new PdfReferenceHolder(annotation));
	}

	private void RemoveAnnotation(PdfAnnotation annotation)
	{
		int index = base.List.IndexOf(annotation);
		annotation.SetPage(null);
		base.List.Remove(annotation);
		m_annotations.RemoveAt(index);
	}

	private void RemoveAnnotationAt(int index)
	{
		DoRemoveAt(index);
	}

	protected virtual int DoAdd(PdfAnnotation annot)
	{
		if (annot is PdfRichMediaAnnotation && (annot as PdfRichMediaAnnotation).content == null)
		{
			throw new PdfException("Rich media content cannot be null");
		}
		annot.SetPage(m_page);
		if (m_page != null && annot is PdfTextMarkupAnnotation)
		{
			(annot as PdfTextMarkupAnnotation).SetQuadPoints(m_page.Size);
		}
		if (annot is PdfRedactionAnnotation)
		{
			PdfPageBase page = GetPage(annot);
			PdfRedactionAnnotation pdfRedactionAnnotation = annot as PdfRedactionAnnotation;
			if (pdfRedactionAnnotation.Flatten)
			{
				if (!(page is PdfLoadedPage))
				{
					throw new PdfException("Redaction annotation cannot be flatten while creating");
				}
				pdfRedactionAnnotation.ApplyRedaction(page as PdfLoadedPage);
			}
		}
		m_annotations.Add(new PdfReferenceHolder(annot));
		base.List.Add(annot);
		PdfPageBase page2 = GetPage(annot);
		if (page2 != null && !LoadedAnnotation(annot) && page2 is PdfPage)
		{
			PdfDocument document = (page2 as PdfPage).Document;
			if (document != null && page2.Annotations != null && !page2.Annotations.m_savePopup)
			{
				AnnotationAddedArgs annotationAddedArgs = new AnnotationAddedArgs();
				annotationAddedArgs.Annotation = annot;
				document.OnAnnotationAdded(annotationAddedArgs);
			}
		}
		if (annot != null && m_page != null)
		{
			DoAddReviewHistory(annot);
			DoAddComments(annot);
		}
		ParsingPopUpAnnotation(annot);
		PdfStructTreeRoot structTreeRoot = PdfCatalog.StructTreeRoot;
		if (structTreeRoot != null && !(annot is WidgetAnnotation) && annot.Page != null)
		{
			if (annot.PdfTag != null && annot.PdfTag is PdfStructureElement)
			{
				structTreeRoot.Add(annot.PdfTag as PdfStructureElement, annot.Page, annot.Dictionary);
			}
			else if (annot.Dictionary.ContainsKey("Subtype") && (annot.Dictionary["Subtype"] as PdfName).Value == "Link")
			{
				structTreeRoot.Add(new PdfStructureElement(PdfTagType.Link), annot.Page, annot.Dictionary);
			}
			else
			{
				structTreeRoot.Add(new PdfStructureElement(PdfTagType.Annotation), annot.Page, annot.Dictionary);
			}
		}
		return base.List.Count - 1;
	}

	internal bool LoadedAnnotation(PdfAnnotation annot)
	{
		if (annot is PdfLoadedAnnotation)
		{
			return true;
		}
		return false;
	}

	private PdfPageBase GetPage(PdfAnnotation annotation)
	{
		PdfPageBase pdfPageBase = m_page;
		if (pdfPageBase == null && annotation.Page != null)
		{
			pdfPageBase = annotation.Page;
		}
		else if (pdfPageBase == null && annotation.LoadedPage != null)
		{
			pdfPageBase = annotation.LoadedPage;
		}
		return pdfPageBase;
	}

	private void DoAddState(PdfAnnotation popupAnnoataion)
	{
		PdfPageBase page = GetPage(popupAnnoataion);
		if (page != null)
		{
			popupAnnoataion.Dictionary.SetProperty("P", new PdfReferenceHolder(page));
		}
		popupAnnoataion.Dictionary.SetProperty("Rect", PdfArray.FromRectangle(popupAnnoataion.Bounds));
		m_annotations.Add(new PdfReferenceHolder(popupAnnoataion));
	}

	protected virtual void DoInsert(int index, PdfAnnotation annot)
	{
		m_annotations.Insert(index, new PdfReferenceHolder(annot));
		base.List.Insert(index, annot);
	}

	protected new virtual void DoClear()
	{
		m_annotations.Clear();
		base.List.Clear();
	}

	protected virtual void DoRemoveAt(int index)
	{
		PdfAnnotation pdfAnnotation = base.List[index] as PdfAnnotation;
		PdfReferenceHolder pdfReferenceHolder = new PdfReferenceHolder(pdfAnnotation.Dictionary);
		if (m_annotations.Count > index)
		{
			m_annotations.RemoveAt(index);
		}
		if (pdfReferenceHolder != null && m_annotations.Contains(pdfReferenceHolder))
		{
			m_annotations.Remove(pdfReferenceHolder);
			AnnotationRemovedEvent(pdfAnnotation);
		}
		if (pdfAnnotation != null)
		{
			PdfPageBase page = GetPage(pdfAnnotation);
			if (page is PdfLoadedPage)
			{
				(page as PdfLoadedPage).RemoveFromDictionaries(pdfAnnotation);
			}
			RemovePopupAnnotation(pdfAnnotation);
		}
		AnnotationRemovedEvent(pdfAnnotation);
		base.List.RemoveAt(index);
	}

	private void RemovePopupAnnotation(PdfAnnotation annot)
	{
		PdfReferenceHolder pdfReferenceHolder = null;
		if (annot.m_comments == null || annot.m_comments.Count <= 0)
		{
			return;
		}
		foreach (PdfAnnotation comment in annot.m_comments)
		{
			if (comment == null)
			{
				continue;
			}
			if (comment is PdfPopupAnnotation)
			{
				PdfPopupAnnotation pdfPopupAnnotation = (PdfPopupAnnotation)comment;
				if (pdfPopupAnnotation.ReviewHistory.Count > 0)
				{
					foreach (PdfPopupAnnotation item in pdfPopupAnnotation.ReviewHistory)
					{
						if (item != null)
						{
							pdfReferenceHolder = new PdfReferenceHolder(item.Dictionary);
							if (m_annotations.Contains(pdfReferenceHolder))
							{
								m_annotations.Remove(pdfReferenceHolder);
								AnnotationRemovedEvent(item);
							}
						}
					}
				}
			}
			pdfReferenceHolder = new PdfReferenceHolder(comment.Dictionary);
			if (m_annotations.Contains(pdfReferenceHolder))
			{
				m_annotations.Remove(pdfReferenceHolder);
				AnnotationRemovedEvent(comment);
			}
		}
		if (annot.m_reviewHistory.Count <= 0)
		{
			return;
		}
		foreach (PdfPopupAnnotation item2 in annot.m_reviewHistory)
		{
			if (item2 != null)
			{
				pdfReferenceHolder = new PdfReferenceHolder(item2.Dictionary);
				if (m_annotations.Contains(pdfReferenceHolder))
				{
					m_annotations.Remove(pdfReferenceHolder);
					AnnotationRemovedEvent(item2);
				}
			}
		}
	}

	protected virtual void DoRemove(PdfAnnotation annot)
	{
		int num = base.List.IndexOf(annot);
		PdfReferenceHolder pdfReferenceHolder = new PdfReferenceHolder(annot.Dictionary);
		if (m_annotations.Count > num)
		{
			m_annotations.RemoveAt(num);
		}
		if (pdfReferenceHolder != null && m_annotations.Contains(pdfReferenceHolder))
		{
			m_annotations.Remove(pdfReferenceHolder);
			AnnotationRemovedEvent(annot);
		}
		RemovePopupAnnotation(annot);
		AnnotationRemovedEvent(annot);
		base.List.RemoveAt(num);
	}

	internal void AnnotationRemovedEvent(PdfAnnotation annot)
	{
		PdfPageBase page = GetPage(annot);
		if (page != null && !LoadedAnnotation(annot) && page is PdfPage)
		{
			PdfDocument document = (page as PdfPage).Document;
			if (document != null)
			{
				AnnotationRemovedArgs annotationRemovedArgs = new AnnotationRemovedArgs();
				annotationRemovedArgs.Annotation = annot;
				document.OnAnnotationRemoved(annotationRemovedArgs);
			}
		}
	}

	private void ParsingPopUpAnnotation(PdfAnnotation annot)
	{
		if (!annot.Dictionary.ContainsKey("Popup") && !(annot is PdfLoadedPopupAnnotation) && !(annot is PdfPopupAnnotation))
		{
			return;
		}
		bool flag = false;
		bool flag2 = false;
		PdfName pdfName = null;
		if (annot.Dictionary.ContainsKey("Subtype"))
		{
			pdfName = annot.Dictionary["Subtype"] as PdfName;
			if (pdfName != null)
			{
				if (pdfName.Value == "Popup")
				{
					flag = true;
				}
				if (pdfName.Value == "FreeText" || pdfName.Value == "Sound" || pdfName.Value == "FileAttachment")
				{
					flag2 = true;
				}
			}
		}
		if (annot is PdfLoadedPopupAnnotation && annot.Popup == null && flag && !flag2 && annot.Dictionary.ContainsKey("Parent") && PdfCrossTable.Dereference(annot.Dictionary["Parent"]) is PdfDictionary key && m_popupCollection.ContainsKey(key))
		{
			PdfAnnotation pdfAnnotation = m_popupCollection[key];
			if (pdfAnnotation != null && pdfAnnotation is PdfLoadedAnnotation)
			{
				(pdfAnnotation as PdfLoadedAnnotation).Popup = annot as PdfLoadedPopupAnnotation;
				m_popupCollection.Remove(key);
			}
		}
		if (annot.Popup == null && !flag && !flag2 && !m_popupCollection.ContainsKey(annot.Dictionary))
		{
			m_popupCollection.Add(annot.Dictionary, annot);
		}
		if (flag && !m_savePopup)
		{
			m_annotations.Remove(new PdfReferenceHolder(annot));
			base.List.Remove(annot);
		}
	}
}
