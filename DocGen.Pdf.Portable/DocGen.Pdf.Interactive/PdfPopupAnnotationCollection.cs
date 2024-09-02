using System;
using DocGen.Pdf.IO;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public class PdfPopupAnnotationCollection : PdfCollection
{
	internal PdfDictionary annotDictionary;

	private bool isReview;

	internal PdfPageBase page;

	private int ReviewFlag = 30;

	private int CommentFlag = 28;

	public PdfPopupAnnotation this[int index]
	{
		get
		{
			if (index < 0 || base.List.Count <= 0 || index >= base.List.Count)
			{
				throw new IndexOutOfRangeException("index");
			}
			return base.List[index] as PdfPopupAnnotation;
		}
	}

	internal PdfPopupAnnotationCollection(PdfAnnotation pdfAnnotation, bool isReview)
	{
		annotDictionary = pdfAnnotation.Dictionary;
		this.isReview = isReview;
		if (pdfAnnotation.Page != null)
		{
			page = pdfAnnotation.Page;
		}
		else if (pdfAnnotation.LoadedPage != null)
		{
			page = pdfAnnotation.LoadedPage;
		}
	}

	public void Add(PdfPopupAnnotation popupAnnotation)
	{
		if (!IsReviewAnnot())
		{
			if (base.List.Count <= 0 || !isReview)
			{
				popupAnnotation.Dictionary.SetProperty("IRT", new PdfReferenceHolder(annotDictionary));
			}
			else
			{
				popupAnnotation.Dictionary.SetProperty("IRT", new PdfReferenceHolder(base.List[base.List.Count - 1] as PdfAnnotation));
			}
			if (popupAnnotation.AnnotationFlags == PdfAnnotationFlags.Locked)
			{
				if (isReview)
				{
					ReviewFlag = 128;
				}
				else
				{
					CommentFlag = 128;
				}
			}
			popupAnnotation.Dictionary.SetProperty("F", isReview ? new PdfNumber(ReviewFlag) : new PdfNumber(CommentFlag));
			CommentFlag = 28;
			ReviewFlag = 30;
			if (isReview)
			{
				popupAnnotation.Dictionary.SetProperty("State", new PdfString(popupAnnotation.State.ToString()));
				popupAnnotation.Dictionary.SetProperty("StateModel", new PdfString(popupAnnotation.StateModel.ToString()));
			}
			else
			{
				popupAnnotation.Dictionary.SetDateTime("CreationDate", DateTime.Now);
			}
			base.List.Add(popupAnnotation);
			AddInnerCommentOrReview(page, popupAnnotation);
			return;
		}
		throw new PdfException("Could not add comments/reviews to the review");
	}

	private void DoAddPage(PdfDictionary annotDictionary, PdfPopupAnnotation annotation)
	{
		PdfDictionary pdfDictionary = null;
		if (annotDictionary.ContainsKey("P"))
		{
			pdfDictionary = PdfCrossTable.Dereference(annotDictionary["P"]) as PdfDictionary;
		}
		else if (page != null)
		{
			pdfDictionary = page.Dictionary;
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

	private void AddInnerCommentOrReview(PdfPageBase page, PdfPopupAnnotation popupAnnotation)
	{
		if (page != null)
		{
			DoAddComments(popupAnnotation);
			DoAddReviewHistory(popupAnnotation);
		}
		DoAddPage(annotDictionary, popupAnnotation);
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
		for (int i = 0; i < commentsOrReview.Count; i++)
		{
			PdfPopupAnnotation pdfPopupAnnotation = commentsOrReview[i];
			if (pdfPopupAnnotation == null)
			{
				continue;
			}
			DoAddPage(annotDictionary, pdfPopupAnnotation);
			if (pdfPopupAnnotation.Comments.Count > 0)
			{
				DoAddComments(pdfPopupAnnotation);
			}
			if (pdfPopupAnnotation.ReviewHistory.Count > 0)
			{
				for (int j = 0; j < pdfPopupAnnotation.ReviewHistory.Count; j++)
				{
					DoAddPage(annotDictionary, pdfPopupAnnotation.ReviewHistory[j]);
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
				DoAddPage(annotDictionary, commentsOrReview[i]);
			}
		}
	}

	public void Remove(PdfPopupAnnotation popupAnnotation)
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
		if (isReview)
		{
			PdfDictionary pdfDictionary = null;
			PdfDictionary pdfDictionary2 = null;
			if (index == 0)
			{
				pdfDictionary2 = annotDictionary;
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
		DoRemovePage(annotDictionary, annotation);
	}

	private void DoRemovePage(PdfDictionary annotDictionary, PdfAnnotation annotation)
	{
		if (page != null)
		{
			if (page is PdfLoadedPage pdfLoadedPage)
			{
				pdfLoadedPage.Annotations.Remove(annotation);
				DoRemoveChildAnnots(pdfLoadedPage, annotation);
			}
			else
			{
				PdfPage pdfPage = page as PdfPage;
				pdfPage.Annotations.Remove(annotation);
				DoRemoveChildAnnots(pdfPage, annotation);
			}
		}
		PdfDictionary pdfDictionary = null;
		if (annotDictionary.ContainsKey("P"))
		{
			pdfDictionary = PdfCrossTable.Dereference(annotDictionary["P"]) as PdfDictionary;
		}
		else if (page != null)
		{
			pdfDictionary = page.Dictionary;
		}
		if (pdfDictionary != null && PdfCrossTable.Dereference(pdfDictionary["Annots"]) is PdfArray pdfArray)
		{
			pdfArray.Remove(new PdfReferenceHolder(annotation));
		}
		base.List.Remove(annotation);
	}

	private void DoRemoveChildAnnots(PdfPageBase lPage, PdfAnnotation annot)
	{
		PdfPopupAnnotationCollection commentsOrReview = GetCommentsOrReview(annot, isReview: false);
		PdfPopupAnnotationCollection commentsOrReview2 = GetCommentsOrReview(annot, isReview: true);
		if (commentsOrReview2 != null)
		{
			foreach (PdfAnnotation item in commentsOrReview2)
			{
				DoRemovePage(annotDictionary, item);
			}
		}
		if (commentsOrReview == null)
		{
			return;
		}
		foreach (PdfAnnotation item2 in commentsOrReview)
		{
			DoRemovePage(annotDictionary, item2);
		}
	}

	private bool IsReviewAnnot()
	{
		if (annotDictionary["F"] is PdfNumber pdfNumber && pdfNumber.IntValue == ReviewFlag)
		{
			return true;
		}
		return false;
	}
}
