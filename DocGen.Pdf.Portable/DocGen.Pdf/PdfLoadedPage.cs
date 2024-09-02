using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Graphics.Fonts;
using DocGen.Pdf.Interactive;
using DocGen.Pdf.IO;
using DocGen.Pdf.Parsing;
using DocGen.Pdf.Primitives;
using DocGen.Pdf.Security;

namespace DocGen.Pdf;

public class PdfLoadedPage : PdfPageBase
{
	private PdfCrossTable m_crossTable;

	private bool m_bCheckResources;

	private PdfDocumentBase m_document;

	internal List<PdfAnnotation> m_unsupportedAnnotation = new List<PdfAnnotation>();

	private List<PdfDictionary> m_terminalannots = new List<PdfDictionary>();

	private PdfLoadedAnnotationCollection m_annots;

	private List<long> m_widgetReferences;

	private RectangleF m_mediaBox = RectangleF.Empty;

	private SizeF m_size = SizeF.Empty;

	private RectangleF m_cropBox = RectangleF.Empty;

	private RectangleF m_bleedBox = RectangleF.Empty;

	private RectangleF m_trimBox = RectangleF.Empty;

	private RectangleF m_artBox = RectangleF.Empty;

	private PdfResources m_resources;

	private PointF m_origin = PointF.Empty;

	private PdfArray m_annotsReference = new PdfArray();

	internal bool importAnnotation;

	private List<PdfStructureElement> m_pageElements;

	public new PdfLoadedAnnotationCollection Annotations
	{
		get
		{
			if (m_annots == null || importAnnotation)
			{
				CreateAnnotations(GetWidgetReferences());
			}
			return m_annots;
		}
		set
		{
			m_annots = value;
		}
	}

	public RectangleF MediaBox
	{
		get
		{
			if (m_mediaBox.Equals(RectangleF.Empty) && base.Dictionary.ContainsKey("MediaBox"))
			{
				PdfArray pdfArray = base.Dictionary.GetValue(CrossTable, "MediaBox", "Parent") as PdfArray;
				float floatValue = (pdfArray[2] as PdfNumber).FloatValue;
				float height = (((pdfArray[3] as PdfNumber).FloatValue != 0f) ? (pdfArray[3] as PdfNumber).FloatValue : (pdfArray[1] as PdfNumber).FloatValue);
				float floatValue2 = (pdfArray[0] as PdfNumber).FloatValue;
				float floatValue3 = (pdfArray[1] as PdfNumber).FloatValue;
				m_mediaBox = CalculateBounds(floatValue2, floatValue3, floatValue, height);
			}
			return m_mediaBox;
		}
	}

	public override SizeF Size
	{
		get
		{
			if (m_size == SizeF.Empty)
			{
				PdfArray pdfArray = null;
				PdfArray pdfArray2 = null;
				if (base.Dictionary != null)
				{
					pdfArray2 = base.Dictionary.GetValue(CrossTable, "MediaBox", "Parent") as PdfArray;
					pdfArray = base.Dictionary.GetValue(CrossTable, "CropBox", "Parent") as PdfArray;
				}
				float num = 0f;
				float num2 = 0f;
				PdfNumber pdfNumber = new PdfNumber(0);
				if (base.Dictionary.ContainsKey("Rotate"))
				{
					pdfNumber = base.Dictionary.GetValue(CrossTable, "Rotate", "Parent") as PdfNumber;
				}
				if (pdfArray != null && pdfNumber != null)
				{
					num = (pdfArray[2] as PdfNumber).FloatValue - (pdfArray[0] as PdfNumber).FloatValue;
					num2 = (pdfArray[3] as PdfNumber).FloatValue - (pdfArray[1] as PdfNumber).FloatValue;
					bool flag = true;
					if (pdfArray2 != null)
					{
						float num3 = (pdfArray2[2] as PdfNumber).FloatValue - (pdfArray2[0] as PdfNumber).FloatValue;
						if ((pdfArray2[3] as PdfNumber).FloatValue == 0f)
						{
							_ = (pdfArray2[1] as PdfNumber).FloatValue;
						}
						else
						{
							_ = (pdfArray2[3] as PdfNumber).FloatValue;
							_ = (pdfArray2[1] as PdfNumber).FloatValue;
						}
						if (num3 < num)
						{
							flag = false;
						}
					}
					if (((pdfNumber.FloatValue == 0f || pdfNumber.FloatValue == 180f) && num < num2) || ((pdfNumber.FloatValue == 90f || pdfNumber.FloatValue == 270f) && num > num2) || flag)
					{
						num = num;
						num2 = num2;
					}
					else if (pdfArray != null && pdfNumber.FloatValue == 0f && pdfArray2 != null)
					{
						num = (pdfArray2[2] as PdfNumber).FloatValue - (pdfArray2[0] as PdfNumber).FloatValue;
						num2 = (((pdfArray2[3] as PdfNumber).FloatValue != 0f) ? ((pdfArray2[3] as PdfNumber).FloatValue - (pdfArray2[1] as PdfNumber).FloatValue) : (pdfArray2[1] as PdfNumber).FloatValue);
					}
				}
				else if (pdfArray2 != null)
				{
					num = (pdfArray2[2] as PdfNumber).FloatValue - (pdfArray2[0] as PdfNumber).FloatValue;
					num2 = (((pdfArray2[3] as PdfNumber).FloatValue != 0f) ? ((pdfArray2[3] as PdfNumber).FloatValue - (pdfArray2[1] as PdfNumber).FloatValue) : (pdfArray2[1] as PdfNumber).FloatValue);
				}
				else
				{
					float[] array = new float[4]
					{
						0f,
						0f,
						PdfPageSize.Letter.Width,
						PdfPageSize.Letter.Height
					};
					base.Dictionary["MediaBox"] = new PdfArray(array);
					num = PdfPageSize.Letter.Width;
					num2 = PdfPageSize.Letter.Height;
				}
				if (num2 < 0f)
				{
					num2 = 0f - num2;
				}
				if (num < 0f)
				{
					num = 0f - num;
				}
				m_size = new SizeF(num, num2);
			}
			return m_size;
		}
	}

	public RectangleF CropBox
	{
		get
		{
			if (m_cropBox == RectangleF.Empty && base.Dictionary.ContainsKey("CropBox"))
			{
				PdfArray pdfArray = base.Dictionary.GetValue(CrossTable, "CropBox", "Parent") as PdfArray;
				float floatValue = (pdfArray[2] as PdfNumber).FloatValue;
				float height = (((pdfArray[3] as PdfNumber).FloatValue != 0f) ? (pdfArray[3] as PdfNumber).FloatValue : (pdfArray[1] as PdfNumber).FloatValue);
				float floatValue2 = (pdfArray[0] as PdfNumber).FloatValue;
				float floatValue3 = (pdfArray[1] as PdfNumber).FloatValue;
				m_cropBox = CalculateBounds(floatValue2, floatValue3, floatValue, height);
			}
			return m_cropBox;
		}
	}

	public RectangleF BleedBox
	{
		get
		{
			if (m_bleedBox == RectangleF.Empty && base.Dictionary.ContainsKey("BleedBox"))
			{
				PdfArray pdfArray = base.Dictionary.GetValue(CrossTable, "BleedBox", "Parent") as PdfArray;
				float floatValue = (pdfArray[2] as PdfNumber).FloatValue;
				float height = (((pdfArray[3] as PdfNumber).FloatValue != 0f) ? (pdfArray[3] as PdfNumber).FloatValue : (pdfArray[1] as PdfNumber).FloatValue);
				float floatValue2 = (pdfArray[0] as PdfNumber).FloatValue;
				float floatValue3 = (pdfArray[1] as PdfNumber).FloatValue;
				m_bleedBox = CalculateBounds(floatValue2, floatValue3, floatValue, height);
			}
			return m_bleedBox;
		}
	}

	public RectangleF TrimBox
	{
		get
		{
			if (m_trimBox == RectangleF.Empty && base.Dictionary.ContainsKey("TrimBox"))
			{
				PdfArray pdfArray = base.Dictionary.GetValue(CrossTable, "TrimBox", "Parent") as PdfArray;
				float floatValue = (pdfArray[2] as PdfNumber).FloatValue;
				float height = (((pdfArray[3] as PdfNumber).FloatValue != 0f) ? (pdfArray[3] as PdfNumber).FloatValue : (pdfArray[1] as PdfNumber).FloatValue);
				float floatValue2 = (pdfArray[0] as PdfNumber).FloatValue;
				float floatValue3 = (pdfArray[1] as PdfNumber).FloatValue;
				m_trimBox = CalculateBounds(floatValue2, floatValue3, floatValue, height);
			}
			return m_trimBox;
		}
	}

	public RectangleF ArtBox
	{
		get
		{
			if (m_artBox == RectangleF.Empty && base.Dictionary.ContainsKey("ArtBox"))
			{
				PdfArray pdfArray = base.Dictionary.GetValue(CrossTable, "ArtBox", "Parent") as PdfArray;
				float floatValue = (pdfArray[2] as PdfNumber).FloatValue;
				float height = (((pdfArray[3] as PdfNumber).FloatValue != 0f) ? (pdfArray[3] as PdfNumber).FloatValue : (pdfArray[1] as PdfNumber).FloatValue);
				float floatValue2 = (pdfArray[0] as PdfNumber).FloatValue;
				float floatValue3 = (pdfArray[1] as PdfNumber).FloatValue;
				m_artBox = CalculateBounds(floatValue2, floatValue3, floatValue, height);
			}
			return m_artBox;
		}
	}

	public PdfDocumentBase Document => m_document;

	internal PdfCrossTable CrossTable => m_crossTable;

	internal List<PdfDictionary> TerminalAnnotation
	{
		get
		{
			return m_terminalannots;
		}
		set
		{
			m_terminalannots = value;
		}
	}

	internal override PointF Origin
	{
		get
		{
			if (m_origin == PointF.Empty)
			{
				PdfArray obj = base.Dictionary.GetValue(CrossTable, "MediaBox", "Parent") as PdfArray;
				float floatValue = (obj[0] as PdfNumber).FloatValue;
				float floatValue2 = (obj[1] as PdfNumber).FloatValue;
				m_origin = new PointF(floatValue, floatValue2);
			}
			return m_origin;
		}
	}

	internal PdfArray AnnotsReference
	{
		get
		{
			return m_annotsReference;
		}
		set
		{
			m_annotsReference = value;
		}
	}

	public PdfStructureElement[] StructureElements
	{
		get
		{
			PdfLoadedDocument pdfLoadedDocument = m_document as PdfLoadedDocument;
			if (m_pageElements == null && pdfLoadedDocument != null)
			{
				m_pageElements = new List<PdfStructureElement>();
				PdfStructureElement structureElement = pdfLoadedDocument.StructureElement;
				if (structureElement != null)
				{
					GetElements(structureElement);
				}
			}
			return m_pageElements.ToArray();
		}
	}

	public event EventHandler BeginSave;

	internal override PdfResources GetResources()
	{
		if (m_resources == null)
		{
			if (!base.Dictionary.ContainsKey("Resources") || m_bCheckResources)
			{
				m_resources = base.GetResources();
				if ((m_resources.ObtainNames().Count == 0 || m_resources.Items.Count == 0) && base.Dictionary.ContainsKey("Parent"))
				{
					IPdfPrimitive pdfPrimitive = base.Dictionary["Parent"];
					PdfDictionary pdfDictionary = null;
					pdfDictionary = ((!(pdfPrimitive is PdfReferenceHolder)) ? (pdfPrimitive as PdfDictionary) : ((pdfPrimitive as PdfReferenceHolder).Object as PdfDictionary));
					if (pdfDictionary.ContainsKey("Resources"))
					{
						pdfPrimitive = pdfDictionary["Resources"];
						if (pdfPrimitive is PdfDictionary && (pdfPrimitive as PdfDictionary).Items.Count > 0)
						{
							base.Dictionary["Resources"] = pdfPrimitive;
							m_resources = new PdfResources((PdfDictionary)pdfPrimitive);
							PdfDictionary pdfDictionary2 = new PdfDictionary();
							if (m_resources.ContainsKey("XObject") && m_resources["XObject"] is PdfDictionary xObject)
							{
								if (PdfCrossTable.Dereference(base.Dictionary["Contents"]) is PdfArray pdfArray)
								{
									for (int i = 0; i < pdfArray.Count; i++)
									{
										byte[] decompressedData = (PdfCrossTable.Dereference(pdfArray[i]) as PdfStream).GetDecompressedData();
										ParseXobjectImages(decompressedData, xObject, pdfDictionary2);
									}
								}
								else
								{
									byte[] decompressedData2 = (PdfCrossTable.Dereference(base.Dictionary["Contents"]) as PdfStream).GetDecompressedData();
									ParseXobjectImages(decompressedData2, xObject, pdfDictionary2);
								}
								m_resources.SetProperty("XObject", pdfDictionary2);
								SetResources(m_resources);
							}
						}
						else if (pdfPrimitive is PdfReferenceHolder)
						{
							bool flag = false;
							PdfReferenceHolder pdfReferenceHolder = pdfPrimitive as PdfReferenceHolder;
							if (pdfReferenceHolder != null)
							{
								PdfDictionary pdfDictionary3 = pdfReferenceHolder.Object as PdfDictionary;
								if (pdfDictionary3.Items.Count == m_resources.Items.Count || m_resources.Items.Count == 0)
								{
									foreach (KeyValuePair<PdfName, IPdfPrimitive> item in m_resources.Items)
									{
										if (pdfDictionary3.Items.ContainsKey(item.Key))
										{
											if (pdfDictionary3.Items.ContainsValue(m_resources[item.Key]))
											{
												flag = true;
											}
											continue;
										}
										flag = false;
										break;
									}
									if (flag || m_resources.Items.Count == 0)
									{
										base.Dictionary["Resources"] = pdfPrimitive;
										m_resources = new PdfResources((PdfDictionary)(pdfPrimitive as PdfReferenceHolder).Object);
									}
									SetResources(m_resources);
								}
							}
						}
					}
				}
			}
			else
			{
				IPdfPrimitive pdfPrimitive2 = base.Dictionary["Resources"];
				PdfDictionary pdfDictionary4 = m_crossTable.GetObject(pdfPrimitive2) as PdfDictionary;
				if (pdfDictionary4 != null)
				{
					m_resources = new PdfResources(pdfDictionary4);
				}
				if (pdfDictionary4 != null && pdfDictionary4 != pdfPrimitive2 && m_crossTable.Document.PdfObjects.IndexOf(pdfDictionary4) > -1)
				{
					m_crossTable.Document.PdfObjects.ReregisterReference(pdfDictionary4, m_resources);
					if (!m_crossTable.IsMerging)
					{
						m_resources.Position = -1;
					}
				}
				else
				{
					if (m_resources == null)
					{
						m_resources = new PdfResources();
					}
					base.Dictionary["Resources"] = m_resources;
				}
				if (base.Dictionary.ContainsKey("Parent") && PdfCrossTable.Dereference(base.Dictionary["Parent"]) is PdfDictionary pdfDictionary5 && pdfDictionary5.ContainsKey("Resources"))
				{
					PdfReferenceHolder pdfReferenceHolder2 = pdfDictionary5["Resources"] as PdfReferenceHolder;
					PdfReferenceHolder pdfReferenceHolder3 = pdfPrimitive2 as PdfReferenceHolder;
					if (pdfReferenceHolder3 != null && pdfReferenceHolder2 != null && pdfReferenceHolder2.Reference == pdfReferenceHolder3.Reference && PdfCrossTable.Dereference(pdfPrimitive2) is PdfDictionary baseDictionary)
					{
						m_resources = new PdfResources(baseDictionary);
					}
				}
				SetResources(m_resources);
			}
			m_bCheckResources = true;
		}
		return m_resources;
	}

	private int CalculateHash(byte[] b)
	{
		int num = 0;
		int num2 = b.Length;
		for (int i = 0; i < num2; i++)
		{
			num = num * 31 + b[i];
		}
		return num;
	}

	private void ParseXobjectImages(byte[] pageContent, PdfDictionary xObject, PdfDictionary xobjects)
	{
		PdfRecordCollection pdfRecordCollection = new ContentParser(pageContent).ReadContent();
		if (pdfRecordCollection == null)
		{
			return;
		}
		for (int i = 0; i < pdfRecordCollection.RecordCollection.Count; i++)
		{
			if (pdfRecordCollection.RecordCollection[i].OperatorName == "Do")
			{
				xObject.ContainsKey(pdfRecordCollection.RecordCollection[i].Operands[0]);
				PdfReferenceHolder pdfReferenceHolder = xObject[pdfRecordCollection.RecordCollection[i].Operands[0].TrimStart('/')] as PdfReferenceHolder;
				if (pdfReferenceHolder != null)
				{
					xobjects[pdfRecordCollection.RecordCollection[i].Operands[0].TrimStart('/')] = pdfReferenceHolder;
				}
			}
		}
	}

	private void GetElements(PdfStructureElement element)
	{
		if (element.Page == this && element.TagType != PdfTagType.Document)
		{
			m_pageElements.Add(element);
		}
		else if (element.ChildElements.Length != 0)
		{
			PdfStructureElement[] childElements = element.ChildElements;
			foreach (PdfStructureElement element2 in childElements)
			{
				GetElements(element2);
			}
		}
	}

	internal PdfLoadedPage(PdfDocumentBase document, PdfCrossTable cTable, PdfDictionary dictionary)
		: base(dictionary)
	{
		if (document == null)
		{
			throw new ArgumentNullException("document");
		}
		if (cTable == null)
		{
			throw new ArgumentNullException("cTable");
		}
		m_document = document;
		m_crossTable = cTable;
		if (!cTable.PageCorrespondance.ContainsKey(base.Dictionary))
		{
			cTable.PageCorrespondance.Add(base.Dictionary, null);
		}
		if (m_document.IsPdfViewerDocumentDisable)
		{
			base.Dictionary.BeginSave += PageBeginSave;
			base.Dictionary.EndSave += PageEndSave;
		}
	}

	private bool CheckFormField(IPdfPrimitive iPdfPrimitive)
	{
		PdfLoadedDocument pdfLoadedDocument = Document as PdfLoadedDocument;
		if (m_widgetReferences == null && pdfLoadedDocument.Form != null)
		{
			m_widgetReferences = new List<long>();
			foreach (object field in pdfLoadedDocument.Form.Fields)
			{
				if (field is PdfLoadedField pdfLoadedField)
				{
					IPdfPrimitive widgetAnnotation = pdfLoadedField.GetWidgetAnnotation(pdfLoadedField.Dictionary, pdfLoadedField.CrossTable);
					bool isNew;
					PdfReference reference = Document.PdfObjects.GetReference(widgetAnnotation, out isNew);
					if (isNew)
					{
						reference = CrossTable.GetReference(widgetAnnotation);
					}
					m_widgetReferences.Add(reference.ObjNum);
				}
			}
		}
		PdfReferenceHolder pdfReferenceHolder = iPdfPrimitive as PdfReferenceHolder;
		if (pdfReferenceHolder != null && pdfLoadedDocument.Form != null && m_widgetReferences.Count > 0 && pdfReferenceHolder.Reference != null && m_widgetReferences.Contains(pdfReferenceHolder.Reference.ObjNum))
		{
			return true;
		}
		return false;
	}

	internal void RemoveFromDictionaries(PdfAnnotation annot)
	{
		if (base.Dictionary.ContainsKey("Annots"))
		{
			PdfArray pdfArray = m_crossTable.GetObject(base.Dictionary["Annots"]) as PdfArray;
			PdfReferenceHolder element = new PdfReferenceHolder(annot.Dictionary);
			if (annot.Dictionary.ContainsKey("Popup"))
			{
				PdfDictionary pdfDictionary = null;
				pdfDictionary = ((!(annot.Dictionary[new PdfName("Popup")] is PdfReferenceHolder)) ? (annot.Dictionary[new PdfName("Popup")] as PdfDictionary) : ((annot.Dictionary[new PdfName("Popup")] as PdfReferenceHolder).Object as PdfDictionary));
				PdfReferenceHolder element2 = new PdfReferenceHolder(pdfDictionary);
				pdfArray.Remove(element2);
				IPdfPrimitive @object = m_crossTable.GetObject(pdfDictionary);
				int num = m_crossTable.PdfObjects.IndexOf(@object);
				if (num != -1)
				{
					m_crossTable.PdfObjects.Remove(num);
				}
				RemoveAllReference(@object);
				TerminalAnnotation.Remove(pdfDictionary);
			}
			PdfReferenceHolder pdfReferenceHolder = null;
			if (annot.m_comments != null && annot.m_comments.Count > 0)
			{
				foreach (PdfAnnotation comment in annot.m_comments)
				{
					if (comment == null || !(comment is PdfPopupAnnotation))
					{
						continue;
					}
					PdfPopupAnnotation pdfPopupAnnotation = (PdfPopupAnnotation)comment;
					if (pdfPopupAnnotation.ReviewHistory.Count > 0)
					{
						foreach (PdfPopupAnnotation item in pdfPopupAnnotation.ReviewHistory)
						{
							if (item != null)
							{
								pdfReferenceHolder = new PdfReferenceHolder(item.Dictionary);
								if (pdfReferenceHolder != null && pdfArray.Contains(pdfReferenceHolder))
								{
									pdfArray.Remove(pdfReferenceHolder);
									Annotations.AnnotationRemovedEvent(item);
								}
							}
						}
					}
					pdfReferenceHolder = new PdfReferenceHolder(comment.Dictionary);
					if (pdfReferenceHolder != null && pdfArray.Contains(pdfReferenceHolder))
					{
						pdfArray.Remove(pdfReferenceHolder);
						Annotations.AnnotationRemovedEvent(comment);
					}
				}
			}
			if (annot.m_reviewHistory != null && annot.m_reviewHistory.Count > 0)
			{
				foreach (PdfPopupAnnotation item2 in annot.m_reviewHistory)
				{
					if (item2 != null)
					{
						pdfReferenceHolder = new PdfReferenceHolder(item2.Dictionary);
						if (pdfReferenceHolder != null && pdfArray.Contains(pdfReferenceHolder))
						{
							pdfArray.Remove(pdfReferenceHolder);
							Annotations.AnnotationRemovedEvent(item2);
						}
					}
				}
			}
			pdfArray.Remove(element);
			pdfArray.Remove(annot.Dictionary);
			pdfArray.MarkChanged();
			if (annot.m_comments != null && annot.m_comments.Count > 0)
			{
				foreach (PdfAnnotation comment2 in annot.m_comments)
				{
					if (comment2 == null || !(comment2 is PdfPopupAnnotation))
					{
						continue;
					}
					PdfPopupAnnotation pdfPopupAnnotation4 = (PdfPopupAnnotation)comment2;
					if (pdfPopupAnnotation4.ReviewHistory.Count <= 0)
					{
						continue;
					}
					foreach (PdfPopupAnnotation item3 in pdfPopupAnnotation4.ReviewHistory)
					{
						if (item3 != null)
						{
							IPdfPrimitive object2 = m_crossTable.GetObject(item3.Dictionary);
							int num2 = m_crossTable.PdfObjects.IndexOf(object2);
							if (num2 != -1)
							{
								m_crossTable.PdfObjects.Remove(num2);
							}
							RemoveAllReference(object2);
						}
					}
				}
			}
			IPdfPrimitive object3 = m_crossTable.GetObject(annot.Dictionary);
			int num3 = m_crossTable.PdfObjects.IndexOf(object3);
			if (num3 != -1)
			{
				m_crossTable.PdfObjects.Remove(num3);
			}
			RemoveAllReference(object3);
			if (annot is PdfRubberStampAnnotation { Appearance: not null } pdfRubberStampAnnotation && pdfRubberStampAnnotation.Appearance.Normal != null)
			{
				PdfTemplate normal = pdfRubberStampAnnotation.Appearance.Normal;
				if (normal != null)
				{
					RemoveAllReference(normal.m_content);
				}
			}
			TerminalAnnotation.Remove(annot.Dictionary);
			base.Dictionary.SetProperty("Annots", pdfArray);
		}
		_ = annot is PdfLoadedAnnotation;
	}

	private void RemoveAllReference(IPdfPrimitive obj)
	{
		PdfDictionary pdfDictionary = obj as PdfDictionary;
		if (pdfDictionary == null && obj is PdfReferenceHolder)
		{
			pdfDictionary = (obj as PdfReferenceHolder).Object as PdfDictionary;
		}
		if (pdfDictionary == null)
		{
			return;
		}
		foreach (KeyValuePair<PdfName, IPdfPrimitive> item in pdfDictionary.Items)
		{
			if ((!(item.Value is PdfReferenceHolder) && !(item.Value is PdfDictionary)) || !(item.Key.Value != "P") || !(item.Key.Value != "Parent"))
			{
				continue;
			}
			IPdfPrimitive @object = m_crossTable.GetObject(item.Value);
			int num = m_crossTable.PdfObjects.IndexOf(@object);
			if (num != -1)
			{
				m_crossTable.PdfObjects.Remove(num);
			}
			RemoveAllReference(item.Value);
			if (PdfCrossTable.Dereference(item.Value) is PdfStream pdfStream)
			{
				if (pdfStream.InternalStream != null)
				{
					pdfStream.InternalStream.Dispose();
				}
				pdfStream.Dispose();
				pdfStream.Clear();
				pdfStream.isSkip = true;
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

	internal List<long> GetWidgetReferences()
	{
		PdfLoadedDocument pdfLoadedDocument = Document as PdfLoadedDocument;
		m_widgetReferences = new List<long>();
		if (pdfLoadedDocument != null && pdfLoadedDocument.Catalog != null && pdfLoadedDocument.Catalog.ContainsKey("AcroForm") && PdfCrossTable.Dereference(pdfLoadedDocument.Catalog["AcroForm"]) is PdfDictionary pdfDictionary && pdfDictionary.ContainsKey("Fields") && PdfCrossTable.Dereference(pdfDictionary["Fields"]) is PdfArray { Count: >0 } pdfArray)
		{
			for (int i = 0; i < pdfArray.Count; i++)
			{
				if (!(PdfCrossTable.Dereference(pdfArray[i]) is PdfDictionary pdfDictionary2))
				{
					continue;
				}
				IPdfPrimitive obj = pdfDictionary2;
				PdfArray pdfArray2 = null;
				if (pdfDictionary2.ContainsKey("Kids"))
				{
					pdfArray2 = PdfCrossTable.Dereference(pdfDictionary2["Kids"]) as PdfArray;
				}
				PdfReference pdfReference = null;
				bool isNew;
				if (pdfArray2 != null && pdfArray2.Count > 1)
				{
					for (int j = 0; j < pdfArray2.Count; j++)
					{
						obj = PdfCrossTable.Dereference(pdfArray2[j]) as PdfDictionary;
						if (obj != null)
						{
							pdfReference = Document.PdfObjects.GetReference(obj, out isNew);
							if (isNew)
							{
								pdfReference = CrossTable.GetReference(obj);
								Document.PdfObjects.Remove(Document.PdfObjects.Count - 1);
							}
							m_widgetReferences.Add(pdfReference.ObjNum);
							if (obj is PdfDictionary pdfDictionary3 && pdfDictionary3.ContainsKey("Kids") && PdfCrossTable.Dereference(pdfDictionary3["Kids"]) is PdfArray kidsArray)
							{
								ParseInnerKids(kidsArray, pdfDictionary3);
							}
						}
					}
					continue;
				}
				pdfReference = Document.PdfObjects.GetReference(obj, out isNew);
				if (isNew)
				{
					pdfReference = CrossTable.GetReference(obj);
					Document.PdfObjects.Remove(Document.PdfObjects.Count - 1);
				}
				m_widgetReferences.Add(pdfReference.ObjNum);
				PdfDictionary pdfDictionary4 = null;
				if (pdfArray2 != null && pdfArray2.Count == 1)
				{
					pdfDictionary4 = PdfCrossTable.Dereference(pdfArray2[0]) as PdfDictionary;
				}
				if (pdfDictionary4 != null && pdfDictionary4.ContainsKey("Kids"))
				{
					if (PdfCrossTable.Dereference(pdfDictionary2["Kids"]) is PdfArray kidsArray2)
					{
						ParseInnerKids(kidsArray2, pdfDictionary4);
					}
				}
				else if (pdfDictionary4 != null)
				{
					pdfReference = Document.PdfObjects.GetReference(pdfDictionary4, out isNew);
					if (isNew)
					{
						pdfReference = CrossTable.GetReference(pdfDictionary4);
						Document.PdfObjects.Remove(Document.PdfObjects.Count - 1);
					}
					m_widgetReferences.Add(pdfReference.ObjNum);
				}
			}
		}
		return m_widgetReferences;
	}

	private void ParseInnerKids(PdfArray kidsArray, PdfDictionary widget)
	{
		PdfReference pdfReference = null;
		bool isNew;
		if (kidsArray != null && kidsArray.Count > 1)
		{
			for (int i = 0; i < kidsArray.Count; i++)
			{
				if (PdfCrossTable.Dereference(kidsArray[i]) is PdfDictionary pdfDictionary)
				{
					pdfReference = Document.PdfObjects.GetReference(pdfDictionary, out isNew);
					if (isNew)
					{
						pdfReference = CrossTable.GetReference(pdfDictionary);
						Document.PdfObjects.Remove(Document.PdfObjects.Count - 1);
					}
					m_widgetReferences.Add(pdfReference.ObjNum);
					if (pdfDictionary != null && pdfDictionary.ContainsKey("Kids") && PdfCrossTable.Dereference(pdfDictionary["Kids"]) is PdfArray kidsArray2)
					{
						ParseInnerKids(kidsArray2, pdfDictionary);
					}
				}
			}
			return;
		}
		pdfReference = Document.PdfObjects.GetReference(widget, out isNew);
		if (isNew)
		{
			pdfReference = CrossTable.GetReference(widget);
			Document.PdfObjects.Remove(Document.PdfObjects.Count - 1);
		}
		m_widgetReferences.Add(pdfReference.ObjNum);
		PdfDictionary pdfDictionary2 = null;
		if (kidsArray != null && kidsArray.Count == 1)
		{
			pdfDictionary2 = PdfCrossTable.Dereference(kidsArray[0]) as PdfDictionary;
		}
		if (pdfDictionary2 != null && pdfDictionary2.ContainsKey("Kids"))
		{
			if (PdfCrossTable.Dereference(pdfDictionary2["Kids"]) is PdfArray kidsArray3)
			{
				ParseInnerKids(kidsArray3, pdfDictionary2);
			}
		}
		else if (pdfDictionary2 != null)
		{
			pdfReference = Document.PdfObjects.GetReference(pdfDictionary2, out isNew);
			if (isNew)
			{
				pdfReference = CrossTable.GetReference(pdfDictionary2);
				Document.PdfObjects.Remove(Document.PdfObjects.Count - 1);
			}
			m_widgetReferences.Add(pdfReference.ObjNum);
		}
	}

	internal void CreateAnnotations(List<long> widgetReferences)
	{
		PdfArray pdfArray = null;
		if (base.Dictionary.ContainsKey("Annots"))
		{
			pdfArray = PdfCrossTable.Dereference(base.Dictionary["Annots"]) as PdfArray;
			PdfLoadedDocument pdfLoadedDocument = Document as PdfLoadedDocument;
			if (pdfArray != null)
			{
				for (int i = 0; i < pdfArray.Count; i++)
				{
					PdfDictionary pdfDictionary = PdfCrossTable.Dereference(pdfArray[i]) as PdfDictionary;
					PdfReferenceHolder pdfReferenceHolder = pdfArray[i] as PdfReferenceHolder;
					if (pdfLoadedDocument.CrossTable != null && pdfLoadedDocument.CrossTable.Encryptor != null && pdfLoadedDocument.CrossTable.Encryptor.EncryptOnlyAttachment && pdfDictionary != null && pdfDictionary.ContainsKey("Subtype"))
					{
						PdfName pdfName = pdfDictionary.Items[new PdfName("Subtype")] as PdfName;
						if (pdfName != null && pdfName.Value == "FileAttachment")
						{
							PdfReferenceHolder pdfReferenceHolder2 = pdfDictionary["FS"] as PdfReferenceHolder;
							if (pdfReferenceHolder2 != null)
							{
								PdfDictionary pdfDictionary2 = pdfReferenceHolder2.Object as PdfDictionary;
								new PdfStream();
								PdfDictionary pdfDictionary3 = null;
								if (pdfDictionary2 != null && pdfDictionary2.ContainsKey("EF"))
								{
									if (pdfDictionary2["EF"] is PdfDictionary)
									{
										pdfDictionary3 = pdfDictionary2["EF"] as PdfDictionary;
									}
									else if (pdfDictionary2["EF"] is PdfReferenceHolder)
									{
										pdfDictionary3 = (pdfDictionary2["EF"] as PdfReferenceHolder).Object as PdfDictionary;
									}
									if (pdfDictionary3 != null)
									{
										PdfReferenceHolder pdfReferenceHolder3 = pdfDictionary3["F"] as PdfReferenceHolder;
										if (pdfReferenceHolder3 != null)
										{
											PdfReference reference = pdfReferenceHolder3.Reference;
											IPdfDecryptable pdfDecryptable;
											IPdfDecryptable pdfDecryptable2 = (pdfDecryptable = pdfReferenceHolder3.Object as PdfStream);
											if (pdfDecryptable != null)
											{
												if (pdfLoadedDocument.RaiseUserPassword && pdfLoadedDocument.m_password == string.Empty)
												{
													OnPdfPasswordEventArgs onPdfPasswordEventArgs = new OnPdfPasswordEventArgs();
													pdfLoadedDocument.PdfUserPassword(onPdfPasswordEventArgs);
													pdfLoadedDocument.m_password = onPdfPasswordEventArgs.UserPassword;
												}
												pdfLoadedDocument.CheckEncryption(pdfLoadedDocument.CrossTable.Encryptor.EncryptOnlyAttachment);
												pdfDecryptable.Decrypt(pdfLoadedDocument.CrossTable.Encryptor, reference.ObjNum);
											}
											((PdfStream)pdfDecryptable2).Decompress();
										}
									}
								}
							}
						}
					}
					if (pdfDictionary == null || !pdfDictionary.ContainsKey("Subtype"))
					{
						continue;
					}
					PdfName pdfName2 = pdfDictionary.Items[new PdfName("Subtype")] as PdfName;
					if (pdfLoadedDocument.IsEncrypted && pdfName2 != null && pdfName2.Value == "Link" && pdfDictionary.ContainsKey("A") && pdfDictionary["A"] is PdfDictionary && pdfDictionary["A"] is PdfDictionary pdfDictionary4 && pdfDictionary4.ContainsKey("URI"))
					{
						PdfString pdfString = null;
						if (pdfDictionary4["URI"] is PdfString)
						{
							pdfString = PdfCrossTable.Dereference(pdfDictionary4["URI"]) as PdfString;
						}
						if (pdfString != null && (object)pdfReferenceHolder != null)
						{
							PdfReferenceHolder pdfReferenceHolder4 = pdfReferenceHolder;
							if (pdfReferenceHolder4 != null && pdfReferenceHolder4.Reference != null)
							{
								pdfString.Decrypt(pdfLoadedDocument.CrossTable.Encryptor, pdfReferenceHolder4.Reference.ObjNum);
							}
						}
					}
					if (pdfName2 != null && pdfName2.Value != "Widget")
					{
						if (pdfDictionary != null && !m_terminalannots.Contains(pdfDictionary))
						{
							m_terminalannots.Add(pdfDictionary);
						}
					}
					else if (pdfName2 != null && pdfName2.Value == "Widget" && pdfReferenceHolder != null && pdfReferenceHolder.Reference != null && !widgetReferences.Contains(pdfReferenceHolder.Reference.ObjNum))
					{
						m_terminalannots.Add(pdfDictionary);
					}
				}
			}
		}
		if (importAnnotation)
		{
			importAnnotation = false;
		}
		m_annots = new PdfLoadedAnnotationCollection(this);
		Annotations = m_annots;
	}

	protected virtual void OnBeginSave(EventArgs e)
	{
		if (this.BeginSave != null)
		{
			this.BeginSave(this, e);
		}
	}

	private void PageBeginSave(object sender, SavePdfPrimitiveEventArgs args)
	{
		if (m_document.progressDelegate != null)
		{
			m_document.OnPageSave(this);
		}
		OnBeginSave(new EventArgs());
	}

	private void PageEndSave(object sender, SavePdfPrimitiveEventArgs args)
	{
	}

	internal override void Clear()
	{
		if (m_annots != null)
		{
			m_annots.Clear();
		}
		base.Clear();
		if (m_terminalannots != null)
		{
			m_terminalannots.Clear();
		}
		if (m_widgetReferences != null)
		{
			m_widgetReferences.Clear();
		}
		if (m_fontReference != null)
		{
			m_fontReference.Clear();
		}
		if (m_pageElements != null)
		{
			m_pageElements.Clear();
		}
	}

	private float GetContentHeight(string key, bool signedPDF)
	{
		MemoryStream memoryStream = new MemoryStream();
		if (!signedPDF)
		{
			base.Layers.CombineContent(memoryStream);
		}
		else
		{
			PdfArray pdfArray = new PdfArray();
			bool flag = false;
			foreach (IPdfPrimitive content in base.Contents)
			{
				pdfArray.Add(content);
				if (PdfCrossTable.Dereference(content) is PdfStream { Changed: not false } pdfStream)
				{
					flag = pdfStream.Changed;
				}
			}
			base.Layers.CombineContent(memoryStream);
			base.Contents.Clear();
			foreach (IPdfPrimitive item in pdfArray)
			{
				base.Contents.Add(item);
				if (!flag && PdfCrossTable.Dereference(item) is PdfStream { Changed: not false } pdfStream2)
				{
					pdfStream2.FreezeChanges(pdfStream2);
				}
			}
		}
		float result = 0f;
		string text = PdfString.ByteToString(memoryStream.ToArray());
		StringTokenizer stringTokenizer = new StringTokenizer(text);
		if (text.Contains(key))
		{
			int position = text.IndexOf(key);
			stringTokenizer.Position = position;
			string[] array = stringTokenizer.ReadLine().Split(' ');
			if (array.Length == 3)
			{
				float.TryParse(array[1], NumberStyles.Float, CultureInfo.InvariantCulture, out result);
			}
			if (result == 0f)
			{
				result = 12f;
			}
		}
		return result;
	}

	private float GetContentHeight(string key, MemoryStream data)
	{
		float result = 0f;
		string text = PdfString.ByteToString(data.ToArray());
		StringTokenizer stringTokenizer = new StringTokenizer(text);
		if (text.Contains(key))
		{
			int position = text.IndexOf(key);
			stringTokenizer.Position = position;
			string[] array = stringTokenizer.ReadLine().Split(' ');
			result = ((array.Length != 3) ? 12f : float.Parse(array[1], CultureInfo.InvariantCulture));
		}
		return result;
	}

	private PdfFontMetrics CreateFont(PdfDictionary fontDictionary, float height, PdfName baseFont)
	{
		PdfFontMetrics pdfFontMetrics = new PdfFontMetrics();
		if (fontDictionary.ContainsKey("FontDescriptor") && PdfCrossTable.Dereference(fontDictionary["FontDescriptor"]) is PdfDictionary pdfDictionary)
		{
			if (pdfDictionary.ContainsKey("Ascent") && PdfCrossTable.Dereference(pdfDictionary["Ascent"]) is PdfNumber pdfNumber)
			{
				pdfFontMetrics.Ascent = pdfNumber.IntValue;
			}
			if (pdfDictionary.ContainsKey("Descent") && PdfCrossTable.Dereference(pdfDictionary["Descent"]) is PdfNumber pdfNumber2)
			{
				pdfFontMetrics.Descent = pdfNumber2.IntValue;
			}
			pdfFontMetrics.Size = height;
			pdfFontMetrics.Height = pdfFontMetrics.Ascent - pdfFontMetrics.Descent;
			pdfFontMetrics.PostScriptName = baseFont.Value;
			PdfArray pdfArray = null;
			if (fontDictionary.ContainsKey("Widths") && PdfCrossTable.Dereference(fontDictionary["Widths"]) is PdfArray pdfArray2)
			{
				float[] array = new float[pdfArray2.Count];
				for (int i = 0; i < pdfArray2.Count; i++)
				{
					array[i] = (pdfArray2[i] as PdfNumber).IntValue;
				}
				pdfFontMetrics.WidthTable = new StandardWidthTable(array);
			}
			pdfFontMetrics.Name = baseFont.Value;
		}
		return pdfFontMetrics;
	}

	private PdfFontStyle GetFontStyle(string fontFamilyString)
	{
		int num = fontFamilyString.IndexOf("-");
		PdfFontStyle result = PdfFontStyle.Regular;
		if (num >= 0)
		{
			switch (fontFamilyString.Substring(num + 1, fontFamilyString.Length - num - 1))
			{
			case "Italic":
			case "Oblique":
				result = PdfFontStyle.Italic;
				break;
			case "Bold":
				result = PdfFontStyle.Bold;
				break;
			case "BoldItalic":
			case "BoldOblique":
				result = PdfFontStyle.Bold | PdfFontStyle.Italic;
				break;
			}
		}
		return result;
	}

	private PdfFontFamily GetFontFamily(string fontFamilyString)
	{
		int num = fontFamilyString.IndexOf("-");
		PdfFontFamily pdfFontFamily = PdfFontFamily.Helvetica;
		string text = fontFamilyString;
		if (num >= 0)
		{
			text = fontFamilyString.Substring(0, num);
		}
		if (text == "Times")
		{
			return PdfFontFamily.TimesRoman;
		}
		return (PdfFontFamily)Enum.Parse(typeof(PdfFontFamily), text, ignoreCase: true);
	}

	private string GetKey(PdfName fontName, string keyValue)
	{
		PdfResources pdfResources = null;
		if (base.Dictionary.ContainsKey("Resources") && PdfCrossTable.Dereference(base.Dictionary["Resources"]) is PdfDictionary baseDictionary)
		{
			pdfResources = new PdfResources(baseDictionary);
		}
		if (pdfResources != null && pdfResources.ContainsKey("Font") && pdfResources["Font"] is PdfDictionary)
		{
			Dictionary<PdfName, IPdfPrimitive> items = (pdfResources["Font"] as PdfDictionary).Items;
			_ = string.Empty;
			foreach (KeyValuePair<PdfName, IPdfPrimitive> item in items)
			{
				PdfDictionary pdfDictionary = (item.Value as PdfReferenceHolder).Object as PdfDictionary;
				if (pdfDictionary.ContainsKey(keyValue))
				{
					if ((CrossTable.GetObject(pdfDictionary[keyValue]) as PdfName).Value == fontName.Value)
					{
						return item.Key.Value;
					}
				}
				else if (pdfDictionary.ContainsKey("FontDescriptor") && CrossTable.GetObject(pdfDictionary["FontDescriptor"]) is PdfDictionary pdfDictionary2)
				{
					PdfName pdfName = CrossTable.GetObject(pdfDictionary2[keyValue]) as PdfName;
					if (pdfName != null && pdfName.Value == fontName.Value)
					{
						return item.Key.Value;
					}
				}
			}
		}
		return null;
	}

	private RectangleF CalculateBounds(float x, float y, float width, float height)
	{
		width -= x;
		if (height != y)
		{
			height -= y;
		}
		return new RectangleF(x, y, width, height);
	}

	internal RectangleF GetCropBox()
	{
		if (base.Dictionary.ContainsKey("CropBox"))
		{
			PdfArray pdfArray = base.Dictionary.GetValue(CrossTable, "CropBox", "Parent") as PdfArray;
			float floatValue = (pdfArray[2] as PdfNumber).FloatValue;
			float height = (((pdfArray[3] as PdfNumber).FloatValue != 0f) ? (pdfArray[3] as PdfNumber).FloatValue : (pdfArray[1] as PdfNumber).FloatValue);
			float floatValue2 = (pdfArray[0] as PdfNumber).FloatValue;
			float floatValue3 = (pdfArray[1] as PdfNumber).FloatValue;
			return new RectangleF(new PointF(floatValue2, floatValue3), new SizeF(floatValue, height));
		}
		return RectangleF.Empty;
	}
}
