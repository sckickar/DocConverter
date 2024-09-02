using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using SkiaSharp;
using DocGen.Drawing;
using DocGen.Pdf.Exporting;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Interactive;
using DocGen.Pdf.IO;
using DocGen.Pdf.Parsing;
using DocGen.Pdf.Primitives;
using DocGen.PdfViewer.Base;

namespace DocGen.Pdf.Redaction;

internal class PdfTextParserNet
{
	private PdfLoadedPage m_loadedPage;

	private List<RectangleF> m_bounds = new List<RectangleF>();

	private List<PdfRedaction> m_redactions;

	private Page m_page;

	private PdfImageRendererNet m_renderer;

	private float pt = 1.3333f;

	internal bool redactionTrackProcess;

	internal PdfTextParserNet(PdfLoadedPage page, List<PdfRedaction> redactions)
	{
		m_loadedPage = page;
		m_redactions = redactions;
		CombineBounds();
	}

	internal void Process()
	{
		_ = m_loadedPage.Graphics;
		DocGen.PdfViewer.Base.DeviceCMYK deviceCMYK = new DocGen.PdfViewer.Base.DeviceCMYK();
		m_page = new Page(m_loadedPage);
		m_page.Initialize(m_loadedPage, needParsing: true);
		PdfUnitConverter pdfUnitConverter = new PdfUnitConverter();
		float num = 1f;
		if (m_loadedPage.Document != null && m_loadedPage.Document.Catalog != null && m_loadedPage.Document.Catalog.ContainsKey("StructTreeRoot"))
		{
			m_loadedPage.Document.Catalog.Remove("StructTreeRoot");
		}
		Bitmap image = ((!(m_page.CropBox != RectangleF.Empty) || !(m_page.CropBox != m_page.MediaBox)) ? new Bitmap((int)(m_page.Bounds.Width * num), (int)(m_page.Bounds.Height * num)) : new Bitmap((int)(pdfUnitConverter.ConvertToPixels(m_page.CropBox.Width - m_page.CropBox.X, PdfGraphicsUnit.Point) * num), (int)(pdfUnitConverter.ConvertToPixels(m_page.CropBox.Height - m_page.CropBox.Y, PdfGraphicsUnit.Point) * num)));
		MemoryStream memoryStream = new MemoryStream();
		using (GraphicsHelper g = new GraphicsHelper(image))
		{
			if (deviceCMYK == null)
			{
				deviceCMYK = new DocGen.PdfViewer.Base.DeviceCMYK();
			}
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
			PdfPageResourcesHelper pageResources = PageResourceLoader.Instance.GetPageResources(m_loadedPage);
			m_renderer = new PdfImageRendererNet(m_page.RecordCollection, pageResources, g, newPage: false, m_page.Height, m_page.CurrentLeftLocation, deviceCMYK);
			m_renderer.redactions = m_redactions;
			m_renderer.pageRotation = (float)m_page.Rotation;
			m_renderer.m_loadedPage = m_loadedPage;
			List<RectangleF> list = new List<RectangleF>();
			foreach (RectangleF bound in m_bounds)
			{
				list.Add(new RectangleF(bound.X, bound.Y, bound.Width, bound.Height));
			}
			m_renderer.RedactionBounds = list;
			m_renderer.isFindText = true;
			memoryStream = m_renderer.RenderAsImage();
			m_renderer.isFindText = false;
		}
		if (m_loadedPage.Dictionary.ContainsKey("Contents"))
		{
			PdfStream stream = m_loadedPage.Graphics.StreamWriter.GetStream();
			stream.Data = memoryStream.ToArray();
			stream.Compress = true;
			PdfArray arrayFromReferenceHolder = GetArrayFromReferenceHolder(m_loadedPage.Dictionary["Contents"]);
			if (arrayFromReferenceHolder != null)
			{
				for (int i = 0; i < arrayFromReferenceHolder.Count; i++)
				{
					IPdfPrimitive pdfPrimitive = arrayFromReferenceHolder[i];
					if (pdfPrimitive is PdfReferenceHolder && (pdfPrimitive as PdfReferenceHolder).Reference != null && m_loadedPage != null && m_loadedPage.CrossTable != null && m_loadedPage.CrossTable.PdfObjects != null)
					{
						IPdfPrimitive @object = m_loadedPage.CrossTable.GetObject(pdfPrimitive);
						int num2 = m_loadedPage.CrossTable.PdfObjects.IndexOf(@object);
						if (num2 != -1)
						{
							m_loadedPage.CrossTable.PdfObjects.Remove(num2);
						}
					}
				}
			}
			arrayFromReferenceHolder?.Clear();
			m_loadedPage.Dictionary["Contents"] = new PdfReferenceHolder(stream);
			m_loadedPage.Graphics.StreamWriter = new PdfStreamWriter(stream);
		}
		if (m_renderer.PdfPaths.Count != 0)
		{
			PdfBrush brush = new PdfSolidBrush(Color.White);
			for (int j = 0; j < m_renderer.PdfPaths.Count; j++)
			{
				PdfPath pdfPath = m_renderer.PdfPaths[j];
				pdfPath.FillMode = PdfFillMode.Alternate;
				m_loadedPage.Graphics.DrawPath(PdfPens.White, brush, pdfPath);
			}
		}
		if (redactionTrackProcess)
		{
			RedactionProgressEventArgs redactionProgressEventArgs = new RedactionProgressEventArgs();
			redactionProgressEventArgs.m_progress = 50f;
			if (m_loadedPage != null && m_loadedPage.Document != null && m_loadedPage.Document is PdfLoadedDocument)
			{
				(m_loadedPage.Document as PdfLoadedDocument).OnTrackProgress(redactionProgressEventArgs);
			}
		}
		PdfLoadedAnnotationCollection annotations = m_loadedPage.Annotations;
		for (int k = 0; k < annotations.Count; k++)
		{
			PdfDictionary dictionary = annotations[k].Dictionary;
			PdfCrossTable crossTable = m_loadedPage.CrossTable;
			PdfName name = PdfLoadedAnnotation.GetValue(dictionary, crossTable, "Subtype", inheritable: true) as PdfName;
			PdfLoadedAnnotationType annotationType = annotations.GetAnnotationType(name, dictionary, crossTable);
			RectangleF rect = RectangleF.Empty;
			bool isValidAnnotation = true;
			switch (annotationType)
			{
			case PdfLoadedAnnotationType.TextWebLinkAnnotation:
				if (annotations[k] is PdfLoadedTextWebLinkAnnotation pdfLoadedTextWebLinkAnnotation)
				{
					rect = pdfLoadedTextWebLinkAnnotation.Bounds;
				}
				break;
			case PdfLoadedAnnotationType.DocumentLinkAnnotation:
			{
				PdfFileLinkAnnotation pdfFileLinkAnnotation = null;
				if (annotations[k] is PdfLoadedDocumentLinkAnnotation pdfLoadedDocumentLinkAnnotation)
				{
					rect = pdfLoadedDocumentLinkAnnotation.Bounds;
				}
				else if (annotations[k] is PdfFileLinkAnnotation pdfFileLinkAnnotation2)
				{
					rect = pdfFileLinkAnnotation2.Bounds;
				}
				break;
			}
			case PdfLoadedAnnotationType.FileLinkAnnotation:
				if (annotations[k] is PdfLoadedFileLinkAnnotation pdfLoadedFileLinkAnnotation2)
				{
					rect = pdfLoadedFileLinkAnnotation2.Bounds;
				}
				else if (annotations[k] is PdfFileLinkAnnotation pdfFileLinkAnnotation3)
				{
					rect = pdfFileLinkAnnotation3.Bounds;
				}
				break;
			case PdfLoadedAnnotationType.WatermarkAnnotation:
			{
				PdfWatermarkAnnotation pdfWatermarkAnnotation = null;
				if (annotations[k] is PdfLoadedWatermarkAnnotation pdfLoadedWatermarkAnnotation)
				{
					rect = pdfLoadedWatermarkAnnotation.Bounds;
				}
				else if (annotations[k] is PdfWatermarkAnnotation pdfWatermarkAnnotation2)
				{
					rect = pdfWatermarkAnnotation2.Bounds;
				}
				break;
			}
			case PdfLoadedAnnotationType.TextAnnotation:
			case PdfLoadedAnnotationType.TextMarkupAnnotation:
			case PdfLoadedAnnotationType.CaretAnnotation:
			case PdfLoadedAnnotationType.MovieAnnotation:
			case PdfLoadedAnnotationType.PrinterMarkAnnotation:
			case PdfLoadedAnnotationType.TrapNetworkAnnotation:
			{
				if (!(annotations[k] is PdfTextMarkupAnnotation pdfTextMarkupAnnotation))
				{
					break;
				}
				rect = pdfTextMarkupAnnotation.Bounds;
				if (pdfTextMarkupAnnotation.BoundsCollection == null || pdfTextMarkupAnnotation.BoundsCollection.Count <= 0)
				{
					break;
				}
				bool flag = false;
				foreach (RectangleF item2 in pdfTextMarkupAnnotation.BoundsCollection)
				{
					if (IsFoundRect(item2))
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					annotations.RemoveAt(k);
					k--;
					continue;
				}
				break;
			}
			case PdfLoadedAnnotationType.FileAttachmentAnnotation:
			{
				PdfAttachmentAnnotation pdfAttachmentAnnotation = null;
				if (annotations[k] is PdfLoadedAttachmentAnnotation pdfLoadedAttachmentAnnotation)
				{
					rect = pdfLoadedAttachmentAnnotation.Bounds;
				}
				else if (annotations[k] is PdfAttachmentAnnotation pdfAttachmentAnnotation2)
				{
					rect = pdfAttachmentAnnotation2.Bounds;
				}
				break;
			}
			case PdfLoadedAnnotationType.FreeTextAnnotation:
			{
				PdfFreeTextAnnotation pdfFreeTextAnnotation = null;
				if (annotations[k] is PdfLoadedFreeTextAnnotation pdfLoadedFreeTextAnnotation)
				{
					rect = pdfLoadedFreeTextAnnotation.Bounds;
				}
				else if (annotations[k] is PdfFreeTextAnnotation pdfFreeTextAnnotation2)
				{
					rect = pdfFreeTextAnnotation2.Bounds;
				}
				break;
			}
			case PdfLoadedAnnotationType.LineAnnotation:
			{
				PdfLineAnnotation pdfLineAnnotation = null;
				PdfLoadedLineAnnotation pdfLoadedLineAnnotation = annotations[k] as PdfLoadedLineAnnotation;
				int[] array4 = new int[4];
				if (pdfLoadedLineAnnotation != null)
				{
					array4 = pdfLoadedLineAnnotation.LinePoints;
					rect = pdfLoadedLineAnnotation.Bounds;
				}
				else if (annotations[k] is PdfLineAnnotation pdfLineAnnotation2)
				{
					array4 = pdfLineAnnotation2.LinePoints;
					rect = pdfLineAnnotation2.Bounds;
				}
				bool flag3 = false;
				foreach (PdfRedaction redaction in m_redactions)
				{
					if (!redaction.TextOnly && IsLineIntersectRectangle(redaction.Bounds, array4[0], m_loadedPage.Graphics.Size.Height - (float)array4[1], array4[2], m_loadedPage.Graphics.Size.Height - (float)array4[3]))
					{
						redaction.m_success = true;
						flag3 = true;
						break;
					}
				}
				if (flag3)
				{
					annotations.RemoveAt(k);
					k--;
					continue;
				}
				break;
			}
			case PdfLoadedAnnotationType.CircleAnnotation:
			{
				PdfCircleAnnotation pdfCircleAnnotation = null;
				if (annotations[k] is PdfLoadedCircleAnnotation pdfLoadedCircleAnnotation)
				{
					rect = pdfLoadedCircleAnnotation.Bounds;
				}
				else if (annotations[k] is PdfCircleAnnotation pdfCircleAnnotation2)
				{
					rect = pdfCircleAnnotation2.Bounds;
				}
				break;
			}
			case PdfLoadedAnnotationType.EllipseAnnotation:
			{
				PdfEllipseAnnotation pdfEllipseAnnotation = null;
				if (annotations[k] is PdfLoadedEllipseAnnotation pdfLoadedEllipseAnnotation)
				{
					rect = pdfLoadedEllipseAnnotation.Bounds;
				}
				else if (annotations[k] is PdfEllipseAnnotation pdfEllipseAnnotation2)
				{
					rect = pdfEllipseAnnotation2.Bounds;
				}
				break;
			}
			case PdfLoadedAnnotationType.RichMediaAnnotation:
			{
				PdfRichMediaAnnotation pdfRichMediaAnnotation = null;
				if (annotations[k] is PdfLoadedRichMediaAnnotation pdfLoadedRichMediaAnnotation)
				{
					rect = pdfLoadedRichMediaAnnotation.Bounds;
				}
				else if (annotations[k] is PdfRichMediaAnnotation pdfRichMediaAnnotation2)
				{
					rect = pdfRichMediaAnnotation2.Bounds;
				}
				break;
			}
			case PdfLoadedAnnotationType.SquareAnnotation:
			{
				PdfSquareAnnotation pdfSquareAnnotation = null;
				if (annotations[k] is PdfLoadedSquareAnnotation pdfLoadedSquareAnnotation)
				{
					rect = pdfLoadedSquareAnnotation.Bounds;
				}
				else if (annotations[k] is PdfSquareAnnotation pdfSquareAnnotation2)
				{
					rect = pdfSquareAnnotation2.Bounds;
				}
				break;
			}
			case PdfLoadedAnnotationType.RectangleAnnotation:
			{
				PdfRectangleAnnotation pdfRectangleAnnotation = null;
				if (annotations[k] is PdfLoadedRectangleAnnotation pdfLoadedRectangleAnnotation)
				{
					rect = pdfLoadedRectangleAnnotation.Bounds;
				}
				else if (annotations[k] is PdfRectangleAnnotation pdfRectangleAnnotation2)
				{
					rect = pdfRectangleAnnotation2.Bounds;
				}
				break;
			}
			case PdfLoadedAnnotationType.PolygonAnnotation:
				if (annotations[k] is PdfLoadedPolygonAnnotation pdfLoadedPolygonAnnotation)
				{
					int[] polygonPoints = pdfLoadedPolygonAnnotation.PolygonPoints;
					float[] array = new float[polygonPoints.Length];
					int num3 = 0;
					int[] array2 = polygonPoints;
					foreach (int num4 in array2)
					{
						array[num3] = num4;
						num3++;
					}
					rect = GetBoundsFromPoints(array, out isValidAnnotation);
				}
				break;
			case PdfLoadedAnnotationType.PolyLineAnnotation:
				if (annotations[k] is PdfLoadedPolyLineAnnotation pdfLoadedPolyLineAnnotation)
				{
					int[] polylinePoints = pdfLoadedPolyLineAnnotation.PolylinePoints;
					float[] array3 = new float[polylinePoints.Length];
					int num5 = 0;
					int[] array2 = polylinePoints;
					foreach (int num6 in array2)
					{
						array3[num5] = num6;
						num5++;
					}
					rect = GetBoundsFromPoints(array3, out isValidAnnotation);
				}
				break;
			case PdfLoadedAnnotationType.LinkAnnotation:
				if (annotations[k] is PdfLoadedFileLinkAnnotation pdfLoadedFileLinkAnnotation)
				{
					rect = pdfLoadedFileLinkAnnotation.Bounds;
				}
				else if (annotations[k] is PdfLoadedUriAnnotation pdfLoadedUriAnnotation)
				{
					rect = pdfLoadedUriAnnotation.Bounds;
				}
				else if (annotations[k] is PdfUriAnnotation pdfUriAnnotation)
				{
					rect = pdfUriAnnotation.Bounds;
				}
				break;
			case PdfLoadedAnnotationType.Highlight:
			case PdfLoadedAnnotationType.Underline:
			case PdfLoadedAnnotationType.StrikeOut:
			case PdfLoadedAnnotationType.Squiggly:
			case PdfLoadedAnnotationType.ScreenAnnotation:
				if (annotations[k] is PdfLoadedTextMarkupAnnotation pdfLoadedTextMarkupAnnotation)
				{
					if (pdfLoadedTextMarkupAnnotation.BoundsCollection != null && pdfLoadedTextMarkupAnnotation.BoundsCollection.Count > 0)
					{
						bool flag2 = false;
						foreach (RectangleF item3 in pdfLoadedTextMarkupAnnotation.BoundsCollection)
						{
							if (IsFoundRect(item3))
							{
								flag2 = true;
								break;
							}
						}
						if (flag2)
						{
							annotations.RemoveAt(k);
							k--;
							continue;
						}
					}
					rect = pdfLoadedTextMarkupAnnotation.Bounds;
				}
				else if (annotations[k] is PdfTextMarkupAnnotation pdfTextMarkupAnnotation2)
				{
					rect = pdfTextMarkupAnnotation2.Bounds;
				}
				break;
			case PdfLoadedAnnotationType.PopupAnnotation:
			{
				PdfPopupAnnotation pdfPopupAnnotation = null;
				if (annotations[k] is PdfLoadedPopupAnnotation pdfLoadedPopupAnnotation)
				{
					rect = pdfLoadedPopupAnnotation.Bounds;
				}
				else if (annotations[k] is PdfPopupAnnotation pdfPopupAnnotation2)
				{
					rect = pdfPopupAnnotation2.Bounds;
				}
				break;
			}
			case PdfLoadedAnnotationType.RubberStampAnnotation:
			{
				PdfRubberStampAnnotation pdfRubberStampAnnotation = null;
				if (annotations[k] is PdfLoadedRubberStampAnnotation pdfLoadedRubberStampAnnotation)
				{
					rect = pdfLoadedRubberStampAnnotation.Bounds;
				}
				else if (annotations[k] is PdfRubberStampAnnotation pdfRubberStampAnnotation2)
				{
					rect = pdfRubberStampAnnotation2.Bounds;
				}
				break;
			}
			case PdfLoadedAnnotationType.SoundAnnotation:
			{
				PdfSoundAnnotation pdfSoundAnnotation = null;
				if (annotations[k] is PdfLoadedSoundAnnotation pdfLoadedSoundAnnotation)
				{
					rect = pdfLoadedSoundAnnotation.Bounds;
				}
				else if (annotations[k] is PdfSoundAnnotation pdfSoundAnnotation2)
				{
					rect = pdfSoundAnnotation2.Bounds;
				}
				break;
			}
			case PdfLoadedAnnotationType.InkAnnotation:
			{
				PdfInkAnnotation pdfInkAnnotation = null;
				if (annotations[k] is PdfLoadedInkAnnotation pdfLoadedInkAnnotation)
				{
					List<float> inkList = pdfLoadedInkAnnotation.InkList;
					rect = GetBoundsFromPoints(inkList.ToArray(), out isValidAnnotation);
				}
				else if (annotations[k] is PdfInkAnnotation pdfInkAnnotation2)
				{
					List<float> inkList2 = pdfInkAnnotation2.InkList;
					rect = GetBoundsFromPoints(inkList2.ToArray(), out isValidAnnotation);
				}
				break;
			}
			default:
				rect = default(RectangleF);
				isValidAnnotation = false;
				break;
			}
			if (isValidAnnotation && IsFoundRect(rect))
			{
				annotations.RemoveAt(k);
				k--;
			}
		}
		List<PdfReferenceHolder> list2 = new List<PdfReferenceHolder>();
		if (m_loadedPage.Dictionary.ContainsKey("Annots"))
		{
			PdfArray pdfArray = m_loadedPage.CrossTable.GetObject(m_loadedPage.Dictionary["Annots"]) as PdfArray;
			_ = m_loadedPage.Document;
			if (pdfArray != null)
			{
				for (int m = 0; m < pdfArray.Count; m++)
				{
					PdfDictionary pdfDictionary = m_loadedPage.CrossTable.GetObject(pdfArray[m]) as PdfDictionary;
					PdfReferenceHolder item = pdfArray[m] as PdfReferenceHolder;
					if (pdfDictionary != null && pdfDictionary.ContainsKey("Subtype"))
					{
						PdfName pdfName = pdfDictionary["Subtype"] as PdfName;
						if (pdfName != null && pdfName.Value == "Widget")
						{
							list2.Add(item);
						}
					}
				}
			}
		}
		PdfLoadedForm form = (m_loadedPage.Document as PdfLoadedDocument).Form;
		if (form != null)
		{
			PdfLoadedFormFieldCollection fields = form.Fields;
			bool flag4 = true;
			for (int n = 0; n < fields.Count; n++)
			{
				PdfLoadedField pdfLoadedField = fields[n] as PdfLoadedField;
				if ((pdfLoadedField != null && pdfLoadedField.Page != m_loadedPage) || pdfLoadedField == null)
				{
					continue;
				}
				RectangleF rect2 = RectangleF.Empty;
				PdfDictionary dictionary2 = pdfLoadedField.Dictionary;
				PdfCrossTable crossTable2 = form.CrossTable;
				PdfName pdfName2 = PdfLoadedField.GetValue(dictionary2, crossTable2, "FT", inheritable: true) as PdfName;
				PdfLoadedFieldTypes pdfLoadedFieldTypes = PdfLoadedFieldTypes.Null;
				if (pdfName2 != null)
				{
					pdfLoadedFieldTypes = form.Fields.GetFieldType(pdfName2, dictionary2, crossTable2);
				}
				switch (pdfLoadedFieldTypes)
				{
				case PdfLoadedFieldTypes.PushButton:
					rect2 = (pdfLoadedField as PdfLoadedButtonField).Bounds;
					break;
				case PdfLoadedFieldTypes.CheckBox:
					rect2 = (pdfLoadedField as PdfLoadedCheckBoxField).Bounds;
					break;
				case PdfLoadedFieldTypes.RadioButton:
				{
					PdfArray kids2 = (pdfLoadedField as PdfLoadedRadioButtonListField).Kids;
					if (kids2 != null && kids2.Count > 0)
					{
						for (int num8 = 0; num8 < kids2.Count; num8++)
						{
							PdfDictionary pdfDictionary3 = null;
							if (kids2[num8] is PdfReferenceHolder)
							{
								pdfDictionary3 = (kids2[num8] as PdfReferenceHolder).Object as PdfDictionary;
							}
							else if (kids2[num8] is PdfDictionary)
							{
								pdfDictionary3 = kids2[num8] as PdfDictionary;
							}
							if (pdfDictionary3 == null || !pdfDictionary3.ContainsKey("Rect"))
							{
								continue;
							}
							if (pdfDictionary3["Rect"] is PdfArray pdfArray3)
							{
								rect2 = pdfArray3.ToRectangle();
								rect2.Y = m_loadedPage.Graphics.Size.Height - (rect2.Y + rect2.Height);
							}
							if (!IsEmptyRect(rect2) && IsFoundRect(rect2))
							{
								PdfReferenceHolder element2 = ((kids2[num8] is PdfReferenceHolder) ? (kids2[num8] as PdfReferenceHolder) : new PdfReferenceHolder(kids2[num8]));
								if (m_loadedPage.Dictionary.ContainsKey("Annots"))
								{
									PdfArray pdfArray4 = form.CrossTable.GetObject(m_loadedPage.Dictionary["Annots"]) as PdfArray;
									pdfArray4.Remove(element2);
									pdfArray4.MarkChanged();
									m_loadedPage.Dictionary.SetProperty("Annots", pdfArray4);
								}
								(pdfLoadedField as PdfLoadedRadioButtonListField).Dictionary.Modify();
								kids2.RemoveAt(num8);
								kids2.MarkChanged();
								num8--;
							}
						}
					}
					else
					{
						rect2 = (pdfLoadedField as PdfLoadedRadioButtonListField).Bounds;
					}
					break;
				}
				case PdfLoadedFieldTypes.TextField:
				{
					PdfLoadedTextBoxField pdfLoadedTextBoxField = pdfLoadedField as PdfLoadedTextBoxField;
					if (pdfLoadedTextBoxField.Kids != null && pdfLoadedTextBoxField.Kids.Count > 1)
					{
						PdfArray kids = pdfLoadedTextBoxField.Kids;
						for (int num7 = 0; num7 < kids.Count; num7++)
						{
							PdfDictionary pdfDictionary2 = null;
							if (kids[num7] is PdfReferenceHolder)
							{
								pdfDictionary2 = (kids[num7] as PdfReferenceHolder).Object as PdfDictionary;
							}
							else if (kids[num7] is PdfDictionary)
							{
								pdfDictionary2 = kids[num7] as PdfDictionary;
							}
							if (pdfDictionary2 == null || !pdfDictionary2.ContainsKey("Rect"))
							{
								continue;
							}
							rect2 = (pdfDictionary2["Rect"] as PdfArray).ToRectangle();
							rect2.Y = m_loadedPage.Graphics.Size.Height - (rect2.Y + rect2.Height);
							if (!IsEmptyRect(rect2) && IsFoundRect(rect2) && IsKidInSamePage(pdfDictionary2))
							{
								PdfReferenceHolder element = ((kids[num7] is PdfReferenceHolder) ? (kids[num7] as PdfReferenceHolder) : new PdfReferenceHolder(kids[num7]));
								if (m_loadedPage.Dictionary.ContainsKey("Annots"))
								{
									PdfArray pdfArray2 = form.CrossTable.GetObject(m_loadedPage.Dictionary["Annots"]) as PdfArray;
									pdfArray2.Remove(element);
									pdfArray2.MarkChanged();
									m_loadedPage.Dictionary.SetProperty("Annots", pdfArray2);
								}
								(pdfLoadedField as PdfLoadedTextBoxField).Dictionary.Modify();
								kids.RemoveAt(num7);
								kids.MarkChanged();
								num7--;
								flag4 = false;
							}
						}
					}
					else
					{
						rect2 = pdfLoadedTextBoxField.Bounds;
					}
					break;
				}
				case PdfLoadedFieldTypes.ComboBox:
					rect2 = (pdfLoadedField as PdfLoadedComboBoxField).Bounds;
					break;
				case PdfLoadedFieldTypes.ListBox:
					rect2 = (pdfLoadedField as PdfLoadedListBoxField).Bounds;
					break;
				case PdfLoadedFieldTypes.SignatureField:
					rect2 = (pdfLoadedField as PdfLoadedSignatureField).Bounds;
					break;
				case PdfLoadedFieldTypes.Null:
					rect2 = (pdfLoadedField as PdfLoadedStyledField).Bounds;
					break;
				}
				bool flag5 = IsEmptyRect(rect2);
				if (flag4 && !flag5 && IsFoundRect(rect2))
				{
					form.Fields.RemoveAt(n);
					n--;
				}
				flag4 = true;
			}
		}
		list2 = null;
		PdfImageInfo[] array5 = null;
		array5 = m_loadedPage.GetImagesInfo();
		PdfPageRotateAngle rotation = m_loadedPage.Rotation;
		if (array5 != null)
		{
			PdfImageInfo[] array6 = array5;
			foreach (PdfImageInfo pdfImageInfo in array6)
			{
				RectangleF bounds = pdfImageInfo.Bounds;
				bounds.X *= pt;
				bounds.Y *= pt;
				bounds.Width *= pt;
				bounds.Height *= pt;
				SKBitmap sKBitmap = null;
				MemoryStream memoryStream2 = null;
				Bitmap image2 = pdfImageInfo.Image;
				if (image2 != null)
				{
					byte[] imageData = image2.m_imageData;
					if (imageData != null)
					{
						sKBitmap = SKBitmap.Decode(imageData);
						memoryStream2 = new MemoryStream();
					}
					else if (pdfImageInfo.ImageStream != null && pdfImageInfo.ImageStream is MemoryStream memoryStream3)
					{
						sKBitmap = SKBitmap.Decode(memoryStream3.ToArray());
						memoryStream2 = new MemoryStream();
					}
				}
				bool flag6 = false;
				if (sKBitmap != null)
				{
					SKBitmap sKBitmap2 = sKBitmap;
					SKCanvas sKCanvas = new SKCanvas(sKBitmap);
					SizeF size = m_loadedPage.Size;
					foreach (PdfRedaction redaction2 in m_redactions)
					{
						if (redaction2.TextOnly)
						{
							continue;
						}
						RectangleF rectangleF = RectangleF.Intersect(new RectangleF(redaction2.Bounds.X * pt, redaction2.Bounds.Y * pt, redaction2.Bounds.Width * pt, redaction2.Bounds.Height * pt), bounds);
						if (!(rectangleF != RectangleF.Empty) || (int)rectangleF.Height <= 0)
						{
							continue;
						}
						flag6 = true;
						RectangleF empty = RectangleF.Empty;
						if (pdfImageInfo.internalRotation == 0f && rotation != 0 && (m_loadedPage.CropBox == RectangleF.Empty || m_loadedPage.CropBox == m_loadedPage.MediaBox))
						{
							switch (rotation)
							{
							case PdfPageRotateAngle.RotateAngle90:
							{
								rectangleF = new RectangleF(rectangleF.Y, size.Height * pt - rectangleF.X - rectangleF.Width, rectangleF.Height, rectangleF.Width);
								float num12 = (size.Height - (pdfImageInfo.Bounds.X + pdfImageInfo.Bounds.Width) - pdfImageInfo.Bounds.Y) * pt;
								empty = new RectangleF((rectangleF.X - bounds.Y) * (float)sKBitmap2.Width / bounds.Height, (rectangleF.Y - (bounds.Y + num12)) * (float)sKBitmap2.Height / bounds.Width, rectangleF.Width * (float)sKBitmap2.Width / bounds.Height, rectangleF.Height * (float)sKBitmap2.Height / bounds.Width);
								break;
							}
							case PdfPageRotateAngle.RotateAngle180:
							{
								rectangleF = new RectangleF(size.Width * pt - rectangleF.X - rectangleF.Width, size.Height * pt - rectangleF.Y - rectangleF.Height, rectangleF.Width, rectangleF.Height);
								float num10 = (size.Width - (pdfImageInfo.Bounds.Width + pdfImageInfo.Bounds.X) - pdfImageInfo.Bounds.X) * pt;
								float num11 = (size.Height - (pdfImageInfo.Bounds.Height + pdfImageInfo.Bounds.Y) - pdfImageInfo.Bounds.Y) * pt;
								empty = new RectangleF((rectangleF.X - (bounds.X + num10)) * (float)sKBitmap2.Width / bounds.Width, (rectangleF.Y - (bounds.Y + num11)) * (float)sKBitmap2.Height / bounds.Height, rectangleF.Width * (float)sKBitmap2.Width / bounds.Width, rectangleF.Height * (float)sKBitmap2.Height / bounds.Height);
								break;
							}
							case PdfPageRotateAngle.RotateAngle270:
							{
								rectangleF = new RectangleF(size.Width * pt - rectangleF.Y - rectangleF.Height, rectangleF.X, rectangleF.Height, rectangleF.Width);
								float num9 = (size.Width - (pdfImageInfo.Bounds.Y + pdfImageInfo.Bounds.Height) - pdfImageInfo.Bounds.X) * pt;
								empty = new RectangleF((rectangleF.X - (bounds.X + num9)) * (float)sKBitmap2.Width / bounds.Height, (rectangleF.Y - bounds.X) * (float)sKBitmap2.Height / bounds.Width, rectangleF.Width * (float)sKBitmap2.Width / bounds.Height, rectangleF.Height * (float)sKBitmap2.Height / bounds.Width);
								break;
							}
							default:
								empty = new RectangleF((rectangleF.X - bounds.X) * (float)sKBitmap2.Width / bounds.Width, (rectangleF.Y - bounds.Y) * (float)sKBitmap2.Height / bounds.Height, rectangleF.Width * (float)sKBitmap2.Width / bounds.Width, rectangleF.Height * (float)sKBitmap2.Height / bounds.Height);
								break;
							}
						}
						else
						{
							empty = new RectangleF((rectangleF.X - bounds.X) * (float)sKBitmap2.Width / bounds.Width, (rectangleF.Y - bounds.Y) * (float)sKBitmap2.Height / bounds.Height, rectangleF.Width * (float)sKBitmap2.Width / bounds.Width, rectangleF.Height * (float)sKBitmap2.Height / bounds.Height);
						}
						sKCanvas.Save();
						sKCanvas.Translate(empty.X, empty.Y);
						SKPath sKPath = new SKPath();
						sKPath.AddRect(new SKRect(0f, 0f, empty.Width, empty.Height));
						sKCanvas.ClipPath(sKPath);
						sKCanvas.Clear(SKColors.White);
						sKPath.Dispose();
						sKCanvas.Restore();
						redaction2.m_success = true;
					}
					sKBitmap.Encode(memoryStream2, SKEncodedImageFormat.Png, 100);
				}
				if (flag6 && memoryStream2 != null)
				{
					PdfBitmap image3 = new PdfBitmap(memoryStream2);
					m_loadedPage.ReplaceImage(pdfImageInfo.Index, image3);
				}
			}
		}
		foreach (PdfRedaction redaction3 in m_redactions)
		{
			if (redaction3.FillColor != Color.Transparent)
			{
				PdfPath pdfPath2 = new PdfPath();
				pdfPath2.AddRectangle(new RectangleF(redaction3.Bounds.X, redaction3.Bounds.Y, redaction3.Bounds.Width, redaction3.Bounds.Height));
				pdfPath2.CloseAllFigures();
				if (redaction3.PathRedaction)
				{
					if (redaction3.m_success)
					{
						m_loadedPage.Graphics.DrawPath(new PdfPen(new PdfColor(redaction3.FillColor.A, redaction3.FillColor.R, redaction3.FillColor.G, redaction3.FillColor.B)), pdfPath2);
					}
				}
				else if (redaction3.m_success)
				{
					m_loadedPage.Graphics.DrawPath(new PdfSolidBrush(new PdfColor(redaction3.FillColor.A, redaction3.FillColor.R, redaction3.FillColor.G, redaction3.FillColor.B)), pdfPath2);
				}
			}
			if (redaction3.AppearanceEnabled && redaction3.m_success)
			{
				m_loadedPage.Graphics.DrawPdfTemplate(redaction3.Appearance, new PointF(redaction3.Bounds.Location.X, redaction3.Bounds.Location.Y));
			}
		}
		if (redactionTrackProcess)
		{
			RedactionProgressEventArgs redactionProgressEventArgs2 = new RedactionProgressEventArgs();
			redactionProgressEventArgs2.m_progress = 100f;
			if (m_loadedPage != null && m_loadedPage.Document != null && m_loadedPage.Document is PdfLoadedDocument)
			{
				(m_loadedPage.Document as PdfLoadedDocument).OnTrackProgress(redactionProgressEventArgs2);
			}
		}
	}

	private PdfStream GetSaveState()
	{
		PdfStream pdfStream = new PdfStream();
		new PdfStreamWriter(pdfStream).Write("q");
		return pdfStream;
	}

	private PdfStream GetRestoreState()
	{
		PdfStream pdfStream = new PdfStream();
		new PdfStreamWriter(pdfStream).Write("Q");
		return pdfStream;
	}

	private bool IsKidInSamePage(PdfDictionary kid)
	{
		PdfReference reference = m_loadedPage.CrossTable.GetReference(m_loadedPage.Dictionary);
		PdfReference pdfReference = null;
		PdfReferenceHolder pdfReferenceHolder = kid["P"] as PdfReferenceHolder;
		if (pdfReferenceHolder != null)
		{
			pdfReference = pdfReferenceHolder.Reference;
		}
		if (pdfReference != null && reference.ObjNum == pdfReference.ObjNum && reference.GenNum == pdfReference.GenNum)
		{
			return true;
		}
		return false;
	}

	private RectangleF GetBoundsFromPoints(float[] points, out bool isValidAnnotation)
	{
		int num = 0;
		if (points.Length != 0)
		{
			float num2 = points[0];
			float num3 = points[0];
			float num4 = m_loadedPage.Graphics.Size.Height - points[1];
			float num5 = m_loadedPage.Graphics.Size.Height - points[1];
			foreach (float num6 in points)
			{
				if (num % 2 == 0)
				{
					if (num2 > num6)
					{
						num2 = num6;
					}
					if (num3 < num6)
					{
						num3 = num6;
					}
				}
				else
				{
					if (num4 > m_loadedPage.Graphics.Size.Height - num6)
					{
						num4 = m_loadedPage.Graphics.Size.Height - num6;
					}
					if (num5 < m_loadedPage.Graphics.Size.Height - num6)
					{
						num5 = m_loadedPage.Graphics.Size.Height - num6;
					}
				}
				num++;
			}
			isValidAnnotation = true;
			return new RectangleF(num2, num4, num3 - num2, num5 - num4);
		}
		isValidAnnotation = false;
		return default(RectangleF);
	}

	private void CombineBounds()
	{
		foreach (PdfRedaction redaction in m_redactions)
		{
			if (m_loadedPage.Equals(redaction.page))
			{
				m_bounds.Add(new RectangleF(redaction.Bounds.X, redaction.Bounds.Y, redaction.Bounds.Width, redaction.Bounds.Height));
				m_loadedPage.m_RedactionBounds.Add(new RectangleF(redaction.Bounds.X, redaction.Bounds.Y, redaction.Bounds.Width, redaction.Bounds.Height));
			}
		}
	}

	private PdfDictionary GetObject(IPdfPrimitive primitive)
	{
		PdfDictionary result = null;
		if (primitive is PdfDictionary)
		{
			result = primitive as PdfDictionary;
		}
		else if (primitive is PdfReferenceHolder)
		{
			result = (primitive as PdfReferenceHolder).Object as PdfDictionary;
		}
		return result;
	}

	private bool IsLineIntersectRectangle(RectangleF redactBounds, double p1X, double p1Y, double p2X, double p2Y)
	{
		double num = p1X;
		double num2 = p2X;
		if (p1X > p2X)
		{
			num = p2X;
			num2 = p1X;
		}
		if (num2 > (double)(redactBounds.X + redactBounds.Width))
		{
			num2 = redactBounds.X + redactBounds.Width;
		}
		if (num < (double)redactBounds.X)
		{
			num = redactBounds.X;
		}
		if (num > num2)
		{
			return false;
		}
		double num3 = p1Y;
		double num4 = p2Y;
		double num5 = p2X - p1X;
		if (num5 > 1E-07)
		{
			double num6 = (p2Y - p1Y) / num5;
			double num7 = p1Y - num6 * p1X;
			num3 = num6 * num + num7;
			num4 = num6 * num2 + num7;
		}
		if (num3 > num4)
		{
			double num8 = num4;
			num4 = num3;
			num3 = num8;
		}
		if (num4 > (double)(redactBounds.Y + redactBounds.Height))
		{
			num4 = redactBounds.Y + redactBounds.Height;
		}
		if (num3 < (double)redactBounds.Y)
		{
			num3 = redactBounds.Y;
		}
		if (num3 > num4)
		{
			return false;
		}
		return true;
	}

	private bool IsFoundRect(RectangleF rect)
	{
		bool result = false;
		foreach (PdfRedaction redaction in m_redactions)
		{
			if (!redaction.TextOnly && IntersectWith(redaction.Bounds, rect))
			{
				redaction.m_success = true;
				result = true;
				break;
			}
		}
		return result;
	}

	private bool IsEmptyRect(RectangleF rect)
	{
		if (!(rect.Width <= 0f))
		{
			return rect.Height <= 0f;
		}
		return true;
	}

	private bool IntersectWith(RectangleF rect1, RectangleF rect2)
	{
		if (rect2.X < rect1.X + rect1.Width && rect1.X < rect2.X + rect2.Width && rect2.Y < rect1.Y + rect1.Height)
		{
			return rect1.Y < rect2.Y + rect2.Height;
		}
		return false;
	}

	private PdfArray GetArrayFromReferenceHolder(IPdfPrimitive primitive)
	{
		if (primitive is PdfReferenceHolder)
		{
			PdfReferenceHolder pdfReferenceHolder = primitive as PdfReferenceHolder;
			if (pdfReferenceHolder.Object is PdfReferenceHolder)
			{
				return GetArrayFromReferenceHolder(pdfReferenceHolder.Object);
			}
			return pdfReferenceHolder.Object as PdfArray;
		}
		return primitive as PdfArray;
	}

	private PdfStream GetStreamFromRefernceHolder(IPdfPrimitive primitive)
	{
		if (primitive is PdfReferenceHolder)
		{
			PdfReferenceHolder pdfReferenceHolder = primitive as PdfReferenceHolder;
			if (pdfReferenceHolder.Object is PdfReferenceHolder)
			{
				return GetStreamFromRefernceHolder(pdfReferenceHolder.Object);
			}
			return pdfReferenceHolder.Object as PdfStream;
		}
		return primitive as PdfStream;
	}
}
