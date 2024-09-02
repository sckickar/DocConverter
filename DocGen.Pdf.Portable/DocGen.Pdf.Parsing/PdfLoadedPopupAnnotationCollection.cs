using System;
using System.Collections.Generic;
using DocGen.Pdf.Interactive;
using DocGen.Pdf.IO;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Parsing;

public class PdfLoadedPopupAnnotationCollection : PdfCollection
{
	internal PdfDictionary m_annotDictionary;

	private bool m_isReview;

	private PdfLoadedPage m_loadedPage;

	private const int ReviewFlag = 30;

	private const int CommentFlag = 28;

	public PdfLoadedPopupAnnotation this[int index]
	{
		get
		{
			if (index < 0 || base.List.Count <= 0 || index >= base.List.Count)
			{
				throw new IndexOutOfRangeException("index");
			}
			return base.List[index] as PdfLoadedPopupAnnotation;
		}
	}

	internal PdfLoadedPopupAnnotationCollection(PdfLoadedPage page, PdfDictionary annotDictionary, bool isReview)
	{
		m_isReview = isReview;
		m_loadedPage = page;
		m_annotDictionary = annotDictionary;
		if (isReview)
		{
			GetReviewHistory(page, annotDictionary);
		}
		else
		{
			GetComments(page, annotDictionary);
		}
	}

	private void GetReviewHistory(PdfLoadedPage page, PdfDictionary annotDictionary)
	{
		if (IsReviewAnnot(annotDictionary))
		{
			return;
		}
		List<PdfReference> list = new List<PdfReference>();
		List<PdfDictionary> list2 = new List<PdfDictionary>();
		list2.Add(annotDictionary);
		PdfReference reference = page.CrossTable.GetReference(annotDictionary);
		list.Add(reference);
		foreach (PdfLoadedAnnotation annotation in page.Annotations)
		{
			if (!annotation.Dictionary.ContainsKey("IRT") || !(annotation is PdfLoadedPopupAnnotation))
			{
				continue;
			}
			PdfLoadedPopupAnnotation pdfLoadedPopupAnnotation = annotation as PdfLoadedPopupAnnotation;
			if (!IsReviewAnnot(pdfLoadedPopupAnnotation.Dictionary))
			{
				continue;
			}
			IPdfPrimitive pdfPrimitive = pdfLoadedPopupAnnotation.Dictionary["IRT"];
			if (pdfPrimitive == null)
			{
				continue;
			}
			PdfReferenceHolder pdfReferenceHolder = pdfPrimitive as PdfReferenceHolder;
			if (pdfReferenceHolder != null)
			{
				PdfReference reference2 = pdfReferenceHolder.Reference;
				if (list != null && reference2 != null && list.Contains(reference2))
				{
					base.List.Add(pdfLoadedPopupAnnotation);
					PdfReference reference3 = page.CrossTable.GetReference(pdfLoadedPopupAnnotation.Dictionary);
					list.Add(reference3);
				}
				else if (pdfReferenceHolder.Object is PdfDictionary item && list2.Contains(item))
				{
					base.List.Add(pdfLoadedPopupAnnotation);
					list2.Add(pdfLoadedPopupAnnotation.Dictionary);
				}
			}
		}
		list.Clear();
		list2.Clear();
	}

	private void GetComments(PdfLoadedPage page, PdfDictionary annotDictionary)
	{
		if (IsReviewAnnot(annotDictionary))
		{
			return;
		}
		PdfReference reference = page.CrossTable.GetReference(annotDictionary);
		foreach (PdfLoadedAnnotation annotation in page.Annotations)
		{
			if (!annotation.Dictionary.ContainsKey("IRT") || !(annotation is PdfLoadedPopupAnnotation))
			{
				continue;
			}
			PdfLoadedPopupAnnotation pdfLoadedPopupAnnotation = annotation as PdfLoadedPopupAnnotation;
			if (IsReviewAnnot(pdfLoadedPopupAnnotation.Dictionary))
			{
				continue;
			}
			IPdfPrimitive pdfPrimitive = pdfLoadedPopupAnnotation.Dictionary["IRT"];
			if (pdfPrimitive == null)
			{
				continue;
			}
			PdfReferenceHolder pdfReferenceHolder = pdfPrimitive as PdfReferenceHolder;
			if (pdfReferenceHolder != null)
			{
				PdfReference reference2 = pdfReferenceHolder.Reference;
				if (reference != null && reference2 != null && reference == reference2)
				{
					base.List.Add(pdfLoadedPopupAnnotation);
				}
				else if (pdfReferenceHolder.Object is PdfDictionary pdfDictionary && annotDictionary == pdfDictionary)
				{
					base.List.Add(pdfLoadedPopupAnnotation);
				}
			}
		}
	}

	private bool IsReviewAnnot(PdfDictionary annotDictionary)
	{
		if (annotDictionary.ContainsKey("State") || annotDictionary.ContainsKey("StateModel"))
		{
			return true;
		}
		return false;
	}

	public void Add(PdfPopupAnnotation popupAnnotation)
	{
		if (!IsReviewAnnot())
		{
			if (base.List.Count <= 0 || !m_isReview)
			{
				popupAnnotation.Dictionary.SetProperty("IRT", new PdfReferenceHolder(m_annotDictionary));
			}
			else
			{
				popupAnnotation.Dictionary.SetProperty("IRT", new PdfReferenceHolder(base.List[base.List.Count - 1] as PdfAnnotation));
			}
			popupAnnotation.Dictionary.SetProperty("F", m_isReview ? new PdfNumber(30) : new PdfNumber(28));
			if (m_isReview)
			{
				popupAnnotation.Dictionary.SetProperty("State", new PdfString(popupAnnotation.State.ToString()));
				popupAnnotation.Dictionary.SetProperty("StateModel", new PdfString(popupAnnotation.StateModel.ToString()));
			}
			else
			{
				popupAnnotation.Dictionary.SetDateTime("CreationDate", DateTime.Now);
			}
			base.List.Add(popupAnnotation);
			AddInnerCommentOrReview(m_loadedPage, popupAnnotation);
			return;
		}
		throw new PdfException("Could not add comments/reviews to the review");
	}

	private void AddInnerCommentOrReview(PdfLoadedPage page, PdfPopupAnnotation popupAnnotation)
	{
		if (page != null)
		{
			DoAddComments(popupAnnotation);
			DoAddReviewHistory(popupAnnotation);
		}
		DoAddPage(m_annotDictionary, popupAnnotation);
	}

	private void DoAddPage(PdfDictionary annotDictionary, PdfPopupAnnotation annotation)
	{
		PdfDictionary pdfDictionary = null;
		if (annotDictionary.ContainsKey("P"))
		{
			pdfDictionary = PdfCrossTable.Dereference(annotDictionary["P"]) as PdfDictionary;
		}
		else if (m_loadedPage != null)
		{
			pdfDictionary = m_loadedPage.Dictionary;
		}
		if (pdfDictionary != null)
		{
			PdfArray pdfArray = PdfCrossTable.Dereference(pdfDictionary["Annots"]) as PdfArray;
			if (pdfArray == null)
			{
				pdfArray = new PdfArray();
				pdfDictionary.SetProperty("Annots", pdfArray);
			}
			annotation.Dictionary.SetProperty("P", new PdfReferenceHolder(pdfDictionary));
			annotation.Dictionary.SetProperty("Rect", PdfArray.FromRectangle(annotation.Bounds));
			pdfArray.Add(new PdfReferenceHolder(annotation));
		}
	}

	public void Remove(PdfAnnotation popupAnnotation)
	{
		RemoveAt(base.List.IndexOf(popupAnnotation));
	}

	public void RemoveAt(int index)
	{
		if (index < 0 && index >= base.List.Count)
		{
			throw new PdfException("Index", new IndexOutOfRangeException());
		}
		PdfAnnotation annotation = base.List[index] as PdfAnnotation;
		if (m_isReview)
		{
			PdfDictionary pdfDictionary = null;
			PdfDictionary pdfDictionary2 = null;
			if (index == 0)
			{
				pdfDictionary2 = m_annotDictionary;
			}
			if (index > 0 && index < base.List.Count)
			{
				pdfDictionary2 = (base.List[index - 1] as PdfAnnotation).Dictionary;
			}
			if (index + 1 < base.List.Count)
			{
				pdfDictionary = (base.List[index + 1] as PdfAnnotation).Dictionary;
			}
			if (pdfDictionary != null && pdfDictionary2 != null)
			{
				pdfDictionary.SetProperty("IRT", new PdfReferenceHolder(pdfDictionary2));
			}
		}
		DoRemovePage(m_annotDictionary, annotation);
	}

	private void DoRemovePage(PdfDictionary annotDictionary, PdfAnnotation annotation)
	{
		if (m_loadedPage != null)
		{
			m_loadedPage.Annotations.Remove(annotation);
			DoRemoveChildAnnots(m_loadedPage, annotation);
		}
		PdfDictionary pdfDictionary = null;
		if (annotDictionary.ContainsKey("P"))
		{
			pdfDictionary = PdfCrossTable.Dereference(annotDictionary["P"]) as PdfDictionary;
		}
		else if (m_loadedPage != null)
		{
			pdfDictionary = m_loadedPage.Dictionary;
		}
		if (pdfDictionary != null && PdfCrossTable.Dereference(pdfDictionary["Annots"]) is PdfArray pdfArray)
		{
			pdfArray.Remove(new PdfReferenceHolder(annotation));
		}
		base.List.Remove(annotation);
	}

	private void DoRemoveChildAnnots(PdfLoadedPage lPage, PdfAnnotation annot)
	{
		PdfPopupAnnotationCollection commentsOrReview = GetCommentsOrReview(annot, isReview: false);
		PdfPopupAnnotationCollection commentsOrReview2 = GetCommentsOrReview(annot, isReview: true);
		if (commentsOrReview2 != null)
		{
			foreach (PdfAnnotation item in commentsOrReview2)
			{
				DoRemovePage(m_annotDictionary, item);
			}
		}
		if (commentsOrReview == null)
		{
			return;
		}
		foreach (PdfAnnotation item2 in commentsOrReview)
		{
			DoRemovePage(m_annotDictionary, item2);
		}
	}

	private bool IsReviewAnnot()
	{
		if (m_annotDictionary["F"] is PdfNumber { IntValue: 30 })
		{
			return true;
		}
		return false;
	}

	private void DoAddComments(PdfAnnotation annotation)
	{
		PdfPopupAnnotationCollection commentsOrReview = GetCommentsOrReview(annotation, isReview: false);
		if (commentsOrReview == null)
		{
			return;
		}
		for (int i = 0; i < commentsOrReview.Count; i++)
		{
			PdfPopupAnnotation pdfPopupAnnotation = commentsOrReview[i];
			DoAddPage(m_annotDictionary, pdfPopupAnnotation);
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
					DoAddPage(m_annotDictionary, pdfPopupAnnotation.ReviewHistory[j]);
				}
			}
		}
	}

	private void DoAddReviewHistory(PdfAnnotation annotation)
	{
		PdfPopupAnnotationCollection commentsOrReview = GetCommentsOrReview(annotation, isReview: true);
		if (commentsOrReview != null)
		{
			for (int i = 0; i < commentsOrReview.Count; i++)
			{
				DoAddPage(m_annotDictionary, commentsOrReview[i]);
			}
		}
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
}
