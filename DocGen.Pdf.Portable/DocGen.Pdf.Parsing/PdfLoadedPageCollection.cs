using System;
using System.Collections;
using System.Collections.Generic;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Interactive;
using DocGen.Pdf.IO;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Parsing;

public class PdfLoadedPageCollection : IEnumerable
{
	private PdfDocumentBase m_document;

	private PdfCrossTable m_crossTable;

	private Dictionary<PdfDictionary, PdfPageBase> m_pagesCash;

	private PdfLoadedDocument m_loadedDocument;

	private int m_pageDuplicaton;

	internal static int m_repeatIndex;

	internal static int m_parentKidsCount;

	internal static int m_parentKidsCounttemp;

	internal static int m_nestedPages;

	private int m_sectionCount;

	private IPdfPrimitive m_pageCatalogObj;

	private PdfDictionary m_pageNodeDictionary;

	private int m_pageNodeCount;

	private PdfArray m_nodeKids;

	private int m_lastPageIndex;

	private int m_lastKidIndex;

	private PdfCrossTable m_lastCrossTable;

	private int m_pageIndex = -1;

	internal Dictionary<PdfDictionary, int> m_pageIndexCollection = new Dictionary<PdfDictionary, int>();

	private bool m_invalidPageNode;

	internal bool m_closeCompletely;

	private bool m_parseInvalidPages;

	private List<long> objectReference;

	public int SectionCount
	{
		get
		{
			IPdfPrimitive pointer = m_document.Catalog["Pages"];
			PdfDictionary pdfDictionary = m_crossTable.GetObject(pointer) as PdfDictionary;
			int result = -1;
			if (pdfDictionary != null && PdfCrossTable.Dereference(pdfDictionary["Kids"]) is PdfArray pdfArray)
			{
				result = pdfArray.Count;
			}
			return result;
		}
	}

	private PdfLoadedDocument LoadedDocument => m_loadedDocument;

	public PdfPageBase this[int index] => GetPage(index);

	public int Count
	{
		get
		{
			int result = 0;
			if (PdfCrossTable.Dereference(m_document.Catalog["Pages"]) is PdfDictionary node)
			{
				result = GetNodeCount(node);
			}
			return result;
		}
	}

	private Dictionary<PdfDictionary, PdfPageBase> PageCache
	{
		get
		{
			if (m_pagesCash == null)
			{
				m_pagesCash = new Dictionary<PdfDictionary, PdfPageBase>();
			}
			return m_pagesCash;
		}
	}

	internal PdfLoadedPageCollection(PdfDocumentBase document, PdfCrossTable crossTable)
	{
		if (document == null)
		{
			throw new ArgumentNullException("document");
		}
		if (crossTable == null)
		{
			throw new ArgumentNullException("crossTable");
		}
		m_document = document;
		m_crossTable = crossTable;
	}

	public PdfPageBase Add()
	{
		return Insert(Count);
	}

	public PdfPageBase Add(SizeF size)
	{
		return Insert(Count, size);
	}

	public PdfPageBase Add(SizeF size, PdfMargins margins)
	{
		return Insert(Count, size, margins);
	}

	public PdfPageBase Add(SizeF size, PdfMargins margins, PdfPageRotateAngle rotation)
	{
		return Insert(Count, size, margins, rotation);
	}

	internal PdfPageBase Add(SizeF size, PdfMargins margins, PdfPageRotateAngle rotation, int location)
	{
		return Insert(location, size, margins, rotation);
	}

	internal PdfPageBase Add(PdfLoadedDocument ldDoc, PdfPageBase page, List<PdfArray> destinations)
	{
		if (ldDoc == null)
		{
			throw new ArgumentNullException("ldDoc");
		}
		if (page == null)
		{
			throw new ArgumentNullException("page");
		}
		PdfTemplate contentTemplate = page.ContentTemplate;
		contentTemplate.m_origin = page.Origin;
		PdfPage pdfPage = Add(page.Size, new PdfMargins(), page.Rotation) as PdfPage;
		if (ldDoc.m_isPageMerging)
		{
			pdfPage.isMergingPage = true;
		}
		if (page.Graphics.m_cropBox != null)
		{
			pdfPage.Graphics.m_cropBox = page.Graphics.m_cropBox;
			pdfPage.Dictionary["CropBox"] = pdfPage.Graphics.m_cropBox;
		}
		if (page != null && page.Dictionary.ContainsKey("MediaBox") && contentTemplate != null && contentTemplate.m_content.ContainsKey("BBox"))
		{
			PdfArray obj = PdfCrossTable.Dereference(page.Dictionary["MediaBox"]) as PdfArray;
			float floatValue = (obj[0] as PdfNumber).FloatValue;
			float floatValue2 = (obj[1] as PdfNumber).FloatValue;
			float floatValue3 = (obj[2] as PdfNumber).FloatValue;
			float floatValue4 = (obj[3] as PdfNumber).FloatValue;
			if (obj != null && floatValue > 0f && floatValue2 > 0f && floatValue3 > 0f && floatValue4 > 0f)
			{
				contentTemplate.m_content["BBox"] = page.Dictionary["MediaBox"];
				contentTemplate.m_origin.X = 0f - page.Origin.X;
			}
		}
		PointF origin = contentTemplate.m_origin;
		if (contentTemplate != null)
		{
			pdfPage.Graphics.DrawPdfTemplate(contentTemplate, origin);
		}
		pdfPage.ImportAnnotations(ldDoc, page, destinations);
		if (page.Rotation == PdfPageRotateAngle.RotateAngle90)
		{
			pdfPage.Graphics.TranslateTransform(0f, pdfPage.Size.Height);
			pdfPage.Graphics.RotateTransform(-90f);
			pdfPage.Graphics.TranslateTransform(0f, 0f);
		}
		else if (page.Rotation == PdfPageRotateAngle.RotateAngle180)
		{
			pdfPage.Graphics.TranslateTransform(pdfPage.Size.Width, pdfPage.Size.Height);
			pdfPage.Graphics.RotateTransform(-180f);
			pdfPage.Graphics.TranslateTransform(0f, 0f);
		}
		else if (page.Rotation == PdfPageRotateAngle.RotateAngle270)
		{
			pdfPage.Graphics.TranslateTransform(page.Size.Width, 0f);
			pdfPage.Graphics.RotateTransform(-270f);
			pdfPage.Graphics.TranslateTransform(0f, 0f);
		}
		if (ldDoc.IsOptimizeIdentical && pdfPage.Dictionary.ContainsKey("Resources"))
		{
			pdfPage.repeatedReferenceCollection = new List<PdfReference>();
			pdfPage.DestinationDocument = ldDoc.DestinationDocument;
			PdfDictionary baseDictionary = PdfCrossTable.Dereference(pdfPage.Dictionary["Resources"]) as PdfDictionary;
			pdfPage.RemoveIdenticalResources(new PdfResources(baseDictionary), pdfPage);
			pdfPage.repeatedReferenceCollection.Clear();
			pdfPage.repeatedReferenceCollection = null;
		}
		return pdfPage;
	}

	internal PdfPageBase Add(PdfLoadedDocument ldDoc, PdfPageBase page)
	{
		if (ldDoc == null)
		{
			throw new ArgumentNullException("ldDoc");
		}
		if (page == null)
		{
			throw new ArgumentNullException("page");
		}
		PdfTemplate content = page.GetContent();
		PdfPage pdfPage = Add(page.Size, new PdfMargins(), page.Rotation) as PdfPage;
		PointF origin = page.Origin;
		if (content != null)
		{
			pdfPage.Graphics.DrawPdfTemplate(content, origin);
		}
		if (pdfPage.Document != null && !pdfPage.Document.EnableMemoryOptimization)
		{
			pdfPage.ImportAnnotations(ldDoc, page);
		}
		return pdfPage;
	}

	public PdfPageBase Insert(int index)
	{
		return Insert(index, SizeF.Empty);
	}

	public PdfPageBase Insert(int index, SizeF size)
	{
		return Insert(index, size, null);
	}

	public PdfPageBase Insert(int index, SizeF size, PdfMargins margins)
	{
		PdfPageRotateAngle rotation = PdfPageRotateAngle.RotateAngle0;
		return Insert(index, size, margins, rotation);
	}

	public PdfPageBase Insert(int index, SizeF size, PdfMargins margins, PdfPageRotateAngle rotation)
	{
		PdfPageOrientation orientation = ((size.Width > size.Height) ? PdfPageOrientation.Landscape : PdfPageOrientation.Portrait);
		if (m_document is PdfLoadedDocument { m_duplicatePage: not false } pdfLoadedDocument)
		{
			index = pdfLoadedDocument.m_duplicatePageIndex;
		}
		return Insert(index, size, margins, rotation, orientation);
	}

	public void RemoveAt(int index)
	{
		PdfPageBase page = GetPage(index);
		Remove(page);
	}

	public void Remove(PdfPageBase page)
	{
		int num = IndexOf(page);
		if (num <= -1)
		{
			return;
		}
		PdfLoadedDocument pdfLoadedDocument = m_document as PdfLoadedDocument;
		Dictionary<PdfPageBase, object> dictionary = null;
		if (pdfLoadedDocument != null)
		{
			dictionary = pdfLoadedDocument.CreateBookmarkDestinationDictionary();
		}
		if (dictionary != null)
		{
			List<object> list = null;
			if (dictionary.ContainsKey(page))
			{
				list = dictionary[page] as List<object>;
			}
			if (list != null)
			{
				for (int i = 0; i < list.Count; i++)
				{
					PdfBookmarkBase pdfBookmarkBase = list[i] as PdfBookmarkBase;
					PdfDestination wrapper = null;
					if (pdfBookmarkBase != null)
					{
						if (pdfBookmarkBase.Dictionary["A"] != null)
						{
							pdfBookmarkBase.Dictionary.SetProperty("A", wrapper);
						}
						pdfBookmarkBase.Dictionary.SetProperty("Dest", wrapper);
					}
				}
			}
		}
		page.m_removedPage = true;
		RemovePdfPageTemplates(pdfLoadedDocument, page);
		PdfDictionary pdfDictionary = ((IPdfWrapper)page).Element as PdfDictionary;
		int localIndex;
		PdfDictionary parent = GetParent(num, out localIndex, zeroValid: true);
		if (pdfDictionary != null)
		{
			pdfDictionary["Parent"] = new PdfReferenceHolder(parent);
		}
		PdfArray nodeKids = GetNodeKids(parent);
		if (num == 0)
		{
			PdfCrossTable crossTable = m_document.CrossTable;
			if (crossTable.DocumentCatalog != null)
			{
				PdfArray pdfArray = crossTable.DocumentCatalog["OpenAction"] as PdfArray;
				if (pdfArray != null)
				{
					pdfArray.Remove(new PdfReferenceHolder(pdfDictionary));
				}
				else
				{
					PdfReferenceHolder pdfReferenceHolder = crossTable.DocumentCatalog["OpenAction"] as PdfReferenceHolder;
					if (pdfReferenceHolder != null)
					{
						if (pdfReferenceHolder.Object is PdfDictionary pdfDictionary2)
						{
							pdfArray = pdfDictionary2["D"] as PdfArray;
						}
						pdfArray?.Remove(new PdfReferenceHolder(pdfDictionary));
					}
				}
			}
		}
		PdfReferenceHolder pdfReferenceHolder2 = null;
		foreach (PdfReferenceHolder item in nodeKids)
		{
			if (item.Object == pdfDictionary)
			{
				pdfReferenceHolder2 = item;
				break;
			}
		}
		if (pdfReferenceHolder2 != null)
		{
			RemoveFormFields(pdfReferenceHolder2);
			nodeKids.Remove(pdfReferenceHolder2);
		}
		UpdateCountDecrement(parent);
		ResetPageCollection();
	}

	internal void RemoveFormFields(PdfReferenceHolder pageHolder)
	{
		if (m_document is PdfLoadedDocument { Form: not null } pdfLoadedDocument)
		{
			pdfLoadedDocument.Form.Fields.RemoveContainingField(pageHolder);
		}
	}

	private void RemovePdfPageTemplates(PdfLoadedDocument loadedDocument, PdfPageBase pdfPageBase)
	{
		PdfDictionary pdfDictionary = PdfCrossTable.Dereference(loadedDocument.Catalog["Names"]) as PdfDictionary;
		if (pdfDictionary != null && pdfDictionary.ContainsKey("Pages") && PdfCrossTable.Dereference(pdfDictionary["Pages"]) is PdfDictionary pdfDictionary2 && pdfDictionary2.ContainsKey("Names") && PdfCrossTable.Dereference(pdfDictionary2["Names"]) is PdfArray namedCollection)
		{
			PdfArray updatedPdfPagetemplates = GetUpdatedPdfPagetemplates(namedCollection, loadedDocument, pdfPageBase);
			PdfDictionary pdfDictionary3 = new PdfDictionary();
			pdfDictionary3["Names"] = new PdfReferenceHolder(updatedPdfPagetemplates);
			pdfDictionary["Pages"] = new PdfReferenceHolder(pdfDictionary3);
		}
		if (pdfDictionary != null && pdfDictionary.ContainsKey("Templates") && PdfCrossTable.Dereference(pdfDictionary["Templates"]) is PdfDictionary pdfDictionary4 && pdfDictionary4.ContainsKey("Names") && PdfCrossTable.Dereference(pdfDictionary4["Names"]) is PdfArray namedCollection2)
		{
			PdfArray updatedPdfPagetemplates2 = GetUpdatedPdfPagetemplates(namedCollection2, loadedDocument, pdfPageBase);
			PdfDictionary pdfDictionary5 = new PdfDictionary();
			pdfDictionary5["Names"] = new PdfReferenceHolder(updatedPdfPagetemplates2);
			pdfDictionary["Templates"] = new PdfReferenceHolder(pdfDictionary5);
		}
	}

	private PdfArray GetUpdatedPdfPagetemplates(PdfArray namedCollection, PdfLoadedDocument loadedDocument, PdfPageBase pdfPageBase)
	{
		if (namedCollection.Count > 0)
		{
			for (int i = 1; i <= namedCollection.Count; i += 2)
			{
				if (PdfCrossTable.Dereference(namedCollection[i]) is PdfDictionary dic && loadedDocument.Pages.GetPage(dic) == pdfPageBase)
				{
					namedCollection.RemoveAt(i - 1);
					namedCollection.RemoveAt(i - 1);
					return namedCollection;
				}
			}
		}
		return namedCollection;
	}

	public void ReArrange(int[] orderArray)
	{
		int[] array = new int[orderArray.Length];
		int[] array2 = new int[orderArray.Length];
		int[] array3 = new int[orderArray.Length];
		int num = 0;
		int num2 = orderArray.Length;
		int num3 = num2;
		int count = Count;
		int num4 = 0;
		int num5 = 0;
		for (int i = 0; i < orderArray.Length; i++)
		{
			if (orderArray[i] >= Count)
			{
				throw new ArgumentException("The page Index is not Valid");
			}
		}
		IPdfPrimitive pointer = m_document.Catalog["Pages"];
		PdfDictionary pdfDictionary = m_crossTable.GetObject(pointer) as PdfDictionary;
		PdfArray pdfArray = null;
		if (pdfDictionary != null)
		{
			pdfArray = PdfCrossTable.Dereference(pdfDictionary["Kids"]) as PdfArray;
		}
		m_parentKidsCount = Count;
		for (int j = 0; j < num2; j++)
		{
			for (int k = j + 1; k < num2; k++)
			{
				if (orderArray[j] == orderArray[k])
				{
					m_pageDuplicaton = 1;
					if (array[k] == 0)
					{
						num5++;
						array[k] = 1;
						array2[num] = k;
						num++;
					}
				}
			}
		}
		if (m_pageDuplicaton == 1)
		{
			for (int l = 0; l < array2.Length; l++)
			{
				for (int m = l + 1; m < array2.Length; m++)
				{
					if (array2[m] != 0 && array2[l] > array2[m])
					{
						int num6 = array2[l];
						array2[l] = array2[m];
						array2[m] = num6;
					}
				}
			}
			num4 = (m_repeatIndex = array2[0]);
			int num7 = num5;
			if (num2 > count)
			{
				int num8 = num4;
				for (int n = 0; n < num7; n++)
				{
					_ = Count;
					PdfPageBase page = GetPage(orderArray[num8]);
					m_loadedDocument.Pages.Add(m_loadedDocument, page);
					array3[num8] = 1;
					num8++;
				}
			}
			else
			{
				int num9 = num4;
				for (int num10 = 0; num10 < num7; num10++)
				{
					_ = Count;
					PdfPageBase page2 = GetPage(orderArray[num9]);
					m_loadedDocument.Pages.Add(m_loadedDocument, page2);
					array3[num9] = 1;
					num9++;
					_ = Count;
				}
			}
			for (int num11 = num4; num11 < num3; num11++)
			{
				if (array3[num11] == 0)
				{
					_ = Count;
					PdfPageBase page3 = GetPage(orderArray[num11]);
					m_loadedDocument.Pages.Add(m_loadedDocument, page3);
				}
			}
		}
		PdfReference pdfReference = null;
		List<long> list = new List<long>();
		int localIndex;
		PdfDictionary parent = GetParent(0, out localIndex, zeroValid: true);
		PdfArray nodeKids = GetNodeKids(parent);
		m_parentKidsCounttemp = nodeKids.Count;
		int num12 = nodeKids.Count;
		for (int num13 = 0; num13 < nodeKids.Count; num13++)
		{
			PdfReferenceHolder pdfReferenceHolder = nodeKids[num13] as PdfReferenceHolder;
			if (pdfReferenceHolder != null)
			{
				pdfReference = pdfReferenceHolder.Reference;
			}
			if (pdfReference != null)
			{
				list.Add(pdfReference.ObjNum);
			}
		}
		if (num12 >= Count)
		{
			m_nestedPages = 0;
		}
		int num14 = 0;
		while (num12 < Count)
		{
			m_nestedPages = 1;
			PdfDictionary parent2 = GetParent(num12, out localIndex, zeroValid: true);
			if (PdfCrossTable.Dereference(parent2["Kids"]) is PdfArray pdfArray2)
			{
				for (int num15 = 0; num15 < GetNodeKids(parent2).Count; num15++)
				{
					if (PdfCrossTable.Dereference(pdfArray2[num15]) is PdfDictionary pdfDictionary2)
					{
						PdfReferenceHolder pdfReferenceHolder2 = pdfArray2[num15] as PdfReferenceHolder;
						if (pdfReferenceHolder2 != null)
						{
							pdfReference = pdfReferenceHolder2.Reference;
						}
						if (pdfDictionary2["Type"].ToString() == "/Pages")
						{
							num12 = GetInnerKids(pdfReferenceHolder2, nodeKids, list, num15, num12);
						}
						if (pdfDictionary2["Type"].ToString() == "/Page" && pdfReference != null && !list.Contains(pdfReference.ObjNum))
						{
							list.Add(pdfReference.ObjNum);
							nodeKids.Insert(num12, GetNodeKids(parent2)[num15]);
							num12++;
						}
					}
				}
			}
			num14++;
		}
		m_parentKidsCounttemp = nodeKids.Count;
		nodeKids.ReArrange(orderArray);
		num12 = ((m_nestedPages != 1) ? m_parentKidsCount : m_parentKidsCounttemp);
		int num16 = num12 - orderArray.Length;
		if (num16 != 0)
		{
			for (int num17 = 0; num17 < num16; num17++)
			{
				UpdateCountDecrement(parent);
			}
		}
		if (m_nestedPages == 1 && pdfArray != null && pdfArray.Count > 0)
		{
			PdfArray pdfArray3 = PdfCrossTable.Dereference(parent["Kids"]) as PdfArray;
			PdfReferenceHolder[] array4 = null;
			PdfReferenceHolder[] array5 = null;
			if (pdfArray3 != null)
			{
				array4 = new PdfReferenceHolder[pdfArray3.Count];
				array5 = new PdfReferenceHolder[pdfArray3.Count];
				PdfDictionary pdfDictionary3 = PdfCrossTable.Dereference(pdfArray[0]) as PdfDictionary;
				PdfReferenceHolder primitive = null;
				if (pdfDictionary3 != null && pdfDictionary3.ContainsKey("Parent"))
				{
					primitive = pdfDictionary3["Parent"] as PdfReferenceHolder;
					IPdfPrimitive pdfPrimitive = m_document.Catalog["Pages"];
					if (pdfPrimitive != null && pdfPrimitive is PdfReferenceHolder)
					{
						PdfReferenceHolder pdfReferenceHolder3 = (PdfReferenceHolder)pdfPrimitive;
						if (pdfReferenceHolder3 != null && pdfReferenceHolder3.Reference != null)
						{
							if (pdfDictionary3["Parent"] is PdfReferenceHolder)
							{
								PdfReferenceHolder pdfReferenceHolder4 = pdfDictionary3["Parent"] as PdfReferenceHolder;
								if (pdfReferenceHolder4 != null && pdfReferenceHolder4.Reference != null)
								{
									primitive = ((pdfReferenceHolder3.Reference.ObjNum != pdfReferenceHolder4.Reference.ObjNum) ? pdfReferenceHolder3 : (pdfDictionary3["Parent"] as PdfReferenceHolder));
								}
							}
						}
						else
						{
							primitive = pdfReferenceHolder3;
						}
					}
				}
				PdfArray pdfArray4 = new PdfArray();
				for (int num18 = 0; num18 < pdfArray3.Count; num18++)
				{
					PdfArray pdfArray5 = new PdfArray();
					PdfArray pdfArray6 = new PdfArray();
					array4[num18] = pdfArray3[num18] as PdfReferenceHolder;
					PdfDictionary pdfDictionary4 = array4[num18].Object as PdfDictionary;
					PdfDictionary pdfDictionary5 = null;
					PdfDictionary pdfDictionary6 = array4[num18].Object as PdfDictionary;
					PdfArray pdfArray7 = new PdfArray();
					new PdfArray();
					if (pdfDictionary6 != null)
					{
						if (pdfDictionary6.ContainsKey("CropBox"))
						{
							_ = pdfDictionary6["CropBox"];
						}
						if (pdfDictionary6.ContainsKey("MediaBox"))
						{
							pdfArray7 = pdfDictionary6["MediaBox"] as PdfArray;
						}
					}
					if (pdfDictionary4 == null)
					{
						continue;
					}
					pdfDictionary5 = PdfCrossTable.Dereference(pdfDictionary4["Parent"]) as PdfDictionary;
					if (pdfDictionary4["Type"].ToString() == "/Pages")
					{
						if (pdfDictionary4.ContainsKey("CropBox"))
						{
							pdfArray5 = pdfDictionary4["CropBox"] as PdfArray;
							pdfDictionary4.SetProperty("CropBox", pdfArray5);
						}
						if (pdfDictionary4.ContainsKey("MediaBox"))
						{
							pdfArray6 = pdfDictionary4["MediaBox"] as PdfArray;
							pdfDictionary4.SetProperty("MediaBox", pdfArray6);
						}
					}
					else if (pdfDictionary5 != null)
					{
						if (pdfDictionary5.ContainsKey("CropBox"))
						{
							pdfArray5 = pdfDictionary5["CropBox"] as PdfArray;
							pdfDictionary4.SetProperty("CropBox", pdfArray5);
						}
						if (pdfDictionary5.ContainsKey("MediaBox"))
						{
							pdfArray6 = pdfDictionary5["MediaBox"] as PdfArray;
							PdfArray pdfArray8 = pdfDictionary4["CropBox"] as PdfArray;
							if (pdfArray8 != null && pdfArray8.Count > 1)
							{
								PdfNumber pdfNumber = pdfArray8.Elements[0] as PdfNumber;
								PdfNumber pdfNumber2 = pdfArray8.Elements[1] as PdfNumber;
								if (pdfNumber != null && pdfNumber.FloatValue == 0f && pdfNumber2 != null && pdfNumber2.FloatValue == 0f)
								{
									pdfDictionary4.SetProperty("MediaBox", pdfArray6);
								}
							}
							else if (pdfArray8 == null)
							{
								pdfDictionary4.SetProperty("MediaBox", pdfArray6);
							}
						}
					}
					if (pdfDictionary4["Type"].ToString() == "/Page")
					{
						if (pdfDictionary4.ContainsKey("CropBox"))
						{
							pdfArray5 = pdfDictionary6["CropBox"] as PdfArray;
							pdfDictionary4.SetProperty("CropBox", pdfArray5);
						}
						if (pdfDictionary4.ContainsKey("MediaBox"))
						{
							pdfArray6 = pdfDictionary6["MediaBox"] as PdfArray;
							if (pdfArray7.Count > 0 && pdfArray7 != pdfArray6)
							{
								pdfDictionary4.SetProperty("MediaBox", pdfArray7);
							}
						}
					}
					pdfDictionary4.SetProperty("Parent", primitive);
					array5[num18] = pdfDictionary4["Parent"] as PdfReferenceHolder;
					pdfArray4.Add(array4[num18]);
				}
				m_parentKidsCounttemp = pdfArray4.Count;
				pdfDictionary.SetProperty("Kids", pdfArray4);
				pdfDictionary.SetNumber("Count", orderArray.Length);
				if (pdfArray4.Count == 0)
				{
					parent.SetNumber("Count", 0);
				}
			}
		}
		ResetPageCollection();
	}

	private int GetInnerKids(PdfReferenceHolder pdfReference, PdfArray kids, List<long> kidsReference, int klen, int kidsLength1)
	{
		PdfDictionary node = pdfReference.Object as PdfDictionary;
		PdfArray nodeKids = GetNodeKids(node);
		if (nodeKids != null && nodeKids.Elements.Count > 0 && nodeKids.Elements[0] is PdfReferenceHolder)
		{
			PdfReferenceHolder pdfReferenceHolder = nodeKids.Elements[0] as PdfReferenceHolder;
			if (pdfReferenceHolder != null && pdfReferenceHolder.Object != null)
			{
				if (pdfReferenceHolder.Object is PdfDictionary pdfDictionary && pdfDictionary["Type"].ToString() == "/Page")
				{
					if (pdfReferenceHolder.Reference != null && !kidsReference.Contains(pdfReferenceHolder.Reference.ObjNum))
					{
						kidsReference.Add(pdfReferenceHolder.Reference.ObjNum);
						kids.Insert(klen, pdfReferenceHolder);
						kidsLength1++;
					}
				}
				else if (kids.Contains(pdfReference))
				{
					kids.Remove(pdfReference);
				}
			}
		}
		for (int i = 0; i < nodeKids.Count; i++)
		{
			PdfReferenceHolder pdfReferenceHolder2 = nodeKids[i] as PdfReferenceHolder;
			if (!(pdfReferenceHolder2 != null))
			{
				continue;
			}
			PdfDictionary pdfDictionary2 = pdfReferenceHolder2.Object as PdfDictionary;
			if (pdfDictionary2 != null && pdfDictionary2["Type"].ToString() == "/Page")
			{
				if (pdfReferenceHolder2.Reference != null && !kidsReference.Contains(pdfReferenceHolder2.Reference.ObjNum))
				{
					kidsReference.Add(pdfReferenceHolder2.Reference.ObjNum);
					kids.Insert(klen + i, pdfReferenceHolder2);
					kidsLength1++;
				}
			}
			else if (pdfDictionary2["Type"].ToString() == "/Pages")
			{
				GetInnerKids(pdfReferenceHolder2, kids, kidsReference, klen, kidsLength1);
			}
		}
		return kidsLength1;
	}

	private void UpdateCountDecrement(PdfDictionary parent)
	{
		while (parent != null)
		{
			if (GetNodeCount(parent) - 1 == 0)
			{
				PdfDictionary obj = parent;
				if (PdfCrossTable.Dereference(parent["Parent"]) is PdfDictionary pdfDictionary && pdfDictionary["Kids"] is PdfArray pdfArray)
				{
					pdfArray.Remove(new PdfReferenceHolder(obj));
				}
			}
			int value = GetNodeCount(parent) - 1;
			parent.SetNumber("Count", value);
			parent = PdfCrossTable.Dereference(parent["Parent"]) as PdfDictionary;
		}
	}

	public PdfPageBase Insert(int index, SizeF size, PdfMargins margins, PdfPageRotateAngle rotation, PdfPageOrientation orientation)
	{
		if (size == SizeF.Empty)
		{
			size = PdfPageSize.A4;
		}
		PdfPage pdfPage = new PdfPage();
		PdfPageSettings pdfPageSettings = new PdfPageSettings(size, orientation, 0f);
		pdfPageSettings.Size = size;
		if (margins == null)
		{
			margins = new PdfMargins();
			margins.All = 40f;
		}
		pdfPageSettings.Margins = margins;
		pdfPageSettings.Rotate = rotation;
		PdfSection pdfSection = new PdfSection(m_document, pdfPageSettings);
		pdfSection.DropCropBox();
		pdfSection.Add(pdfPage);
		if (((IPdfWrapper)pdfSection).Element is PdfDictionary pdfDictionary)
		{
			int localIndex;
			PdfDictionary parent = GetParent(index, out localIndex, zeroValid: false);
			if (parent.ContainsKey("Rotate"))
			{
				int num = 90;
				int num2 = (int)pdfPage.Rotation * num;
				if (parent["Rotate"] is PdfNumber pdfNumber && pdfNumber.IntValue != num2 && !pdfDictionary.ContainsKey("Rotate"))
				{
					pdfPage.Dictionary["Rotate"] = new PdfNumber(num2);
				}
			}
			pdfDictionary["Parent"] = new PdfReferenceHolder(parent);
			GetNodeKids(parent).Insert(localIndex, new PdfReferenceHolder(pdfDictionary));
			UpdateCount(parent);
			if (((IPdfWrapper)pdfPage).Element is PdfDictionary key)
			{
				PageCache[key] = pdfPage;
			}
		}
		if (m_document is PdfLoadedDocument pdfLoadedDocument)
		{
			pdfPage.Graphics.ColorSpace = pdfLoadedDocument.ColorSpace;
			pdfPage.Graphics.Layer.Colorspace = pdfLoadedDocument.ColorSpace;
		}
		ResetPageCollection();
		return pdfPage;
	}

	public void Insert(int index, PdfPageBase loadedPage)
	{
		if (loadedPage == null)
		{
			throw new ArgumentNullException("loadedPage");
		}
		if (index < 0)
		{
			throw new ArgumentOutOfRangeException("index", "Value cannot be less than zero");
		}
		if (index > Count)
		{
			throw new ArgumentOutOfRangeException("index", "Value can not be more than number of pages in the document.");
		}
		PdfDictionary dictionary = loadedPage.Dictionary;
		if (PageCache.ContainsKey(dictionary))
		{
			DuplicatePage(loadedPage, index);
			return;
		}
		int localIndex;
		PdfDictionary parent = GetParent(index, out localIndex, zeroValid: false);
		if (dictionary.ContainsKey("Parent"))
		{
			PdfDictionary pdfDictionary = PdfCrossTable.Dereference(dictionary["Parent"]) as PdfDictionary;
			if (pdfDictionary != null)
			{
				if (pdfDictionary.ContainsKey("MediaBox") && !dictionary.ContainsKey("MediaBox"))
				{
					dictionary.Items.Add((PdfName)"MediaBox", pdfDictionary["MediaBox"]);
				}
				if (pdfDictionary.ContainsKey("Rotate") && !dictionary.ContainsKey("Rotate"))
				{
					dictionary.Items.Add((PdfName)"Rotate", pdfDictionary["Rotate"]);
				}
			}
			if (pdfDictionary != null && !dictionary.ContainsKey("MediaBox") && PdfCrossTable.Dereference(pdfDictionary["Parent"]) is PdfDictionary pdfDictionary2 && pdfDictionary2["MediaBox"] != null)
			{
				dictionary.Items.Add((PdfName)"MediaBox", pdfDictionary2["MediaBox"]);
			}
			dictionary["Parent"] = new PdfReferenceHolder(parent);
		}
		if (dictionary.ContainsKey("Contents"))
		{
			PdfArray pdfArray = loadedPage.ReInitializeContentReference();
			if (pdfArray != null && pdfArray.Elements.Count > 0)
			{
				dictionary["Contents"] = pdfArray;
			}
		}
		if (dictionary.ContainsKey("Resources"))
		{
			PdfDictionary pdfDictionary3 = loadedPage.ReinitializePageResources();
			if (pdfDictionary3 != null)
			{
				dictionary["Resources"] = new PdfReferenceHolder(pdfDictionary3);
			}
		}
		if (dictionary.ContainsKey("Annots"))
		{
			PdfLoadedPage pdfLoadedPage = loadedPage as PdfLoadedPage;
			PdfCatalog pdfCatalog = null;
			if (pdfLoadedPage != null && pdfLoadedPage.Document != null && pdfLoadedPage.Document.Catalog != null)
			{
				pdfCatalog = pdfLoadedPage.Document.Catalog;
			}
			if (pdfCatalog != null)
			{
				PdfDictionary pdfDictionary4 = null;
				if (pdfCatalog.ContainsKey("AcroForm"))
				{
					pdfDictionary4 = PdfCrossTable.Dereference(pdfCatalog["AcroForm"]) as PdfDictionary;
				}
				loadedPage.ReInitializePageAnnotation(pdfDictionary4);
				if (pdfDictionary4 != null)
				{
					PdfCatalog catalog = m_document.Catalog;
					if (catalog.ContainsKey("AcroForm"))
					{
						PdfDictionary destinationForm = PdfCrossTable.Dereference(catalog["AcroForm"]) as PdfDictionary;
						pdfDictionary4 = MapAcroFromFields(pdfDictionary4, destinationForm);
					}
					catalog[(PdfName)"AcroForm"] = new PdfReferenceHolder(pdfDictionary4);
					catalog.Modify();
				}
			}
		}
		if (dictionary.ContainsKey("Thumb"))
		{
			loadedPage.ReInitializeThumbnail();
		}
		GetNodeKids(parent).Insert(localIndex, new PdfReferenceHolder(dictionary));
		UpdateCount(parent);
		PageCache[dictionary] = loadedPage;
		ResetPageCollection();
	}

	private void DuplicatePage(PdfPageBase page, int index)
	{
		if (m_document is PdfLoadedDocument pdfLoadedDocument)
		{
			pdfLoadedDocument.m_duplicatePage = true;
			pdfLoadedDocument.m_duplicatePageIndex = index;
			bool fieldAutoNaming = true;
			bool flag = false;
			if (pdfLoadedDocument.Catalog != null && pdfLoadedDocument.Catalog.ContainsKey("AcroForm") && PdfCrossTable.Dereference(pdfLoadedDocument.Catalog["AcroForm"]) is PdfDictionary pdfDictionary && pdfDictionary.ContainsKey("Fields") && PdfCrossTable.Dereference(pdfDictionary["Fields"]) is PdfArray { Count: >0 } && pdfLoadedDocument.Form != null)
			{
				fieldAutoNaming = pdfLoadedDocument.Form.FieldAutoNaming;
				pdfLoadedDocument.Form.FieldAutoNaming = false;
				flag = true;
			}
			SaveAnnotations(page);
			PdfPageBase pdfPageBase = pdfLoadedDocument.ImportPage(pdfLoadedDocument, page);
			pdfLoadedDocument.m_duplicatePage = false;
			pdfLoadedDocument.m_duplicatePageIndex = 0;
			if (page is PdfLoadedPage)
			{
				PageCache.Remove(pdfPageBase.Dictionary);
				GetPage(pdfPageBase.Dictionary);
			}
			GroupAcroFormFields(pdfLoadedDocument, pdfPageBase.Dictionary);
			if (flag)
			{
				pdfLoadedDocument.Form.FieldAutoNaming = fieldAutoNaming;
				pdfLoadedDocument.m_duplicateAcroform = true;
				pdfLoadedDocument.UpdateFormFields();
				pdfLoadedDocument.m_duplicateAcroform = false;
			}
		}
	}

	private void SaveAnnotations(PdfPageBase page)
	{
		if (page is PdfPage)
		{
			PdfPage pdfPage = page as PdfPage;
			if (pdfPage.Annotations == null)
			{
				return;
			}
			{
				foreach (PdfAnnotation annotation in pdfPage.Annotations)
				{
					if (!(annotation is PdfLoadedAnnotation))
					{
						annotation.InstanceSave();
					}
				}
				return;
			}
		}
		if (page.Annotations == null)
		{
			return;
		}
		foreach (PdfAnnotation annotation2 in page.Annotations)
		{
			if (!(annotation2 is PdfLoadedAnnotation))
			{
				annotation2.InstanceSave();
			}
		}
	}

	private void GroupAcroFormFields(PdfLoadedDocument loadedDocument, PdfDictionary pageDictionary)
	{
		PdfDictionary catalog = loadedDocument.Catalog;
		PdfArray pdfArray = null;
		if (pageDictionary.ContainsKey("Annots"))
		{
			pdfArray = PdfCrossTable.Dereference(pageDictionary["Annots"]) as PdfArray;
		}
		if (pdfArray == null || (pdfArray != null && pdfArray.Count == 0) || !catalog.ContainsKey("AcroForm") || !(PdfCrossTable.Dereference(catalog["AcroForm"]) is PdfDictionary pdfDictionary) || !pdfDictionary.ContainsKey("Fields") || !(PdfCrossTable.Dereference(pdfDictionary["Fields"]) is PdfArray { Count: >0 } pdfArray2))
		{
			return;
		}
		Dictionary<string, PdfDictionary> dictionary = new Dictionary<string, PdfDictionary>();
		List<string> list = new List<string>();
		for (int i = 0; i < pdfArray2.Count; i++)
		{
			if (!(PdfCrossTable.Dereference(pdfArray2[i]) is PdfDictionary pdfDictionary2) || !pdfDictionary2.ContainsKey("T") || !(pdfDictionary2["T"] is PdfString pdfString))
			{
				continue;
			}
			if (pdfDictionary2.ContainsKey("FT"))
			{
				list.Add(pdfString.Value);
				dictionary[pdfString.Value] = pdfDictionary2;
				continue;
			}
			string fieldName = GetFieldName(pdfDictionary2);
			if (!string.IsNullOrEmpty(fieldName))
			{
				list.Add(fieldName);
				dictionary[fieldName] = pdfDictionary2;
			}
		}
		if (pdfArray != null && pdfArray.Count > 0)
		{
			for (int j = 0; j < pdfArray.Count; j++)
			{
				if (!(PdfCrossTable.Dereference(pdfArray[j]) is PdfDictionary pdfDictionary3) || !pdfDictionary3.ContainsKey("Subtype"))
				{
					continue;
				}
				PdfName pdfName = pdfDictionary3["Subtype"] as PdfName;
				if (!(pdfName != null) || !(pdfName.Value == "Widget"))
				{
					continue;
				}
				PdfDictionary value = null;
				if (!pdfDictionary3.ContainsKey("PMD"))
				{
					PdfString pdfString2 = pdfDictionary3["T"] as PdfString;
					if (pdfString2 != null)
					{
						dictionary.TryGetValue(pdfString2.Value, out value);
					}
					if (value != null)
					{
						PdfDictionary fieldGroup = value;
						GroupField(value, fieldGroup, pdfDictionary, pdfArray2, pdfDictionary3, pdfString2);
						continue;
					}
					if (PdfCrossTable.Dereference(pdfDictionary3["Parent"]) is PdfDictionary pdfDictionary4 && pdfDictionary4.ContainsKey("T"))
					{
						PdfString pdfString3 = pdfDictionary4["T"] as PdfString;
						dictionary.TryGetValue(pdfString3.Value, out value);
					}
					if (value != null)
					{
						PdfDictionary fieldGroup2 = value;
						GroupField(value, fieldGroup2, pdfDictionary, pdfArray2, pdfDictionary3, pdfString2);
					}
					continue;
				}
				PdfString pdfString4 = pdfDictionary3["T"] as PdfString;
				if (pdfString4 != null)
				{
					pdfDictionary3["T"] = new PdfString(pdfString4.Value + 0);
					pdfDictionary3["Parent"] = new PdfReferenceHolder(pdfDictionary);
					pdfArray2.Add(new PdfReferenceHolder(pdfDictionary3));
					continue;
				}
				if (PdfCrossTable.Dereference(pdfDictionary3["Parent"]) is PdfDictionary pdfDictionary5 && pdfDictionary5.ContainsKey("T"))
				{
					PdfString pdfString5 = pdfDictionary5["T"] as PdfString;
					dictionary.TryGetValue(pdfString5.Value, out value);
				}
				if (value != null)
				{
					PdfDictionary fieldGroup3 = value;
					GroupField(value, fieldGroup3, pdfDictionary, pdfArray2, pdfDictionary3, pdfString4);
				}
			}
		}
		pdfDictionary["Fields"] = pdfArray2;
	}

	private string GetFieldName(PdfDictionary acroField)
	{
		if (PdfCrossTable.Dereference(acroField["Kids"]) is PdfArray pdfArray)
		{
			for (int i = 0; i < pdfArray.Count; i++)
			{
				if (PdfCrossTable.Dereference(pdfArray[i]) is PdfDictionary pdfDictionary)
				{
					if (!pdfDictionary.ContainsKey("FT"))
					{
						return GetFieldName(pdfDictionary);
					}
					if (pdfDictionary["T"] is PdfString pdfString)
					{
						return pdfString.Value;
					}
				}
			}
		}
		return string.Empty;
	}

	private void GroupField(PdfDictionary fieldDictionary, PdfDictionary fieldGroup, PdfDictionary acroForm, PdfArray acroFormFields, PdfDictionary annot, PdfString fieldNameValue)
	{
		PdfArray pdfArray = fieldDictionary["Kids"] as PdfArray;
		if (pdfArray == null)
		{
			pdfArray = new PdfArray();
			fieldGroup = new PdfDictionary();
			if (fieldDictionary.ContainsKey("T") && fieldNameValue != null)
			{
				fieldGroup["T"] = new PdfString(fieldNameValue.Value);
			}
			if (fieldDictionary.ContainsKey("FT"))
			{
				fieldGroup["FT"] = fieldDictionary["FT"];
			}
			if (fieldDictionary.ContainsKey("DA"))
			{
				fieldGroup["DA"] = fieldDictionary["DA"];
			}
			if (fieldDictionary.ContainsKey("V"))
			{
				fieldGroup["V"] = fieldDictionary["V"];
			}
			if (fieldDictionary.ContainsKey("Opt"))
			{
				fieldGroup["Opt"] = fieldDictionary["Opt"];
			}
			if (fieldDictionary.ContainsKey("DV"))
			{
				fieldGroup["DV"] = fieldDictionary["DV"];
			}
			if (fieldDictionary.ContainsKey("I"))
			{
				fieldGroup["I"] = fieldDictionary["I"];
			}
			if (fieldDictionary.ContainsKey("AA"))
			{
				fieldGroup["AA"] = fieldDictionary["AA"];
			}
			fieldGroup["Kids"] = pdfArray;
			fieldGroup["Parent"] = new PdfReferenceHolder(acroForm);
			acroFormFields.Remove(new PdfReferenceHolder(fieldDictionary));
			acroFormFields.Add(new PdfReferenceHolder(fieldGroup));
			fieldDictionary.Remove("Parent");
			fieldDictionary.Remove("T");
			fieldDictionary.Remove("V");
			fieldDictionary.Remove("FT");
			fieldDictionary.Remove("Opt");
			fieldDictionary.Remove("DV");
			fieldDictionary.Remove("I");
			fieldDictionary.Remove("AA");
			fieldDictionary["Parent"] = new PdfReferenceHolder(fieldGroup);
			pdfArray.Add(new PdfReferenceHolder(fieldDictionary));
		}
		else if (!fieldDictionary.ContainsKey("FT"))
		{
			pdfArray = GetFieldArray(fieldDictionary);
			fieldGroup = PdfCrossTable.Dereference((PdfCrossTable.Dereference(pdfArray[0]) as PdfDictionary)["Parent"]) as PdfDictionary;
		}
		annot.Remove("Parent");
		annot["Parent"] = new PdfReferenceHolder(fieldGroup);
		annot.Remove("FT");
		annot.Remove("T");
		annot.Remove("V");
		annot.Remove("Opt");
		annot.Remove("DV");
		annot.Remove("I");
		annot.Remove("AA");
		pdfArray.Add(new PdfReferenceHolder(annot));
	}

	private PdfArray GetFieldArray(PdfDictionary acroField)
	{
		PdfArray pdfArray = PdfCrossTable.Dereference(acroField["Kids"]) as PdfArray;
		if (pdfArray != null)
		{
			for (int i = 0; i < pdfArray.Count; i++)
			{
				if (!(PdfCrossTable.Dereference(pdfArray[i]) is PdfDictionary pdfDictionary))
				{
					continue;
				}
				if (pdfDictionary.ContainsKey("FT"))
				{
					if (pdfDictionary.ContainsKey("Kids"))
					{
						pdfArray = PdfCrossTable.Dereference(pdfDictionary["Kids"]) as PdfArray;
					}
					if (pdfArray != null && pdfArray.Count == 1)
					{
						PdfArray pdfArray2 = new PdfArray();
						PdfDictionary pdfDictionary2 = pdfDictionary;
						PdfDictionary pdfDictionary3 = new PdfDictionary();
						if (pdfDictionary2.ContainsKey("T"))
						{
							pdfDictionary3["T"] = pdfDictionary2["T"];
						}
						if (pdfDictionary2.ContainsKey("FT"))
						{
							pdfDictionary3["FT"] = pdfDictionary2["FT"];
						}
						if (pdfDictionary2.ContainsKey("DA"))
						{
							pdfDictionary3["DA"] = pdfDictionary2["DA"];
						}
						if (pdfDictionary2.ContainsKey("V"))
						{
							pdfDictionary3["V"] = pdfDictionary2["V"];
						}
						if (pdfDictionary2.ContainsKey("Opt"))
						{
							pdfDictionary3["Opt"] = pdfDictionary2["Opt"];
						}
						if (pdfDictionary2.ContainsKey("DV"))
						{
							pdfDictionary3["DV"] = pdfDictionary2["DV"];
						}
						if (pdfDictionary2.ContainsKey("I"))
						{
							pdfDictionary3["I"] = pdfDictionary2["I"];
						}
						if (pdfDictionary2.ContainsKey("AA"))
						{
							pdfDictionary3["AA"] = pdfDictionary2["AA"];
						}
						pdfDictionary3["Kids"] = pdfArray2;
						pdfDictionary3["Parent"] = new PdfReferenceHolder(acroField);
						pdfDictionary2.Remove("Parent");
						pdfDictionary2.Remove("T");
						pdfDictionary2.Remove("V");
						pdfDictionary2.Remove("FT");
						pdfDictionary2.Remove("Opt");
						pdfDictionary2.Remove("DV");
						pdfDictionary2.Remove("I");
						pdfDictionary2.Remove("AA");
						pdfDictionary2["Parent"] = new PdfReferenceHolder(pdfDictionary3);
						pdfArray = new PdfArray();
						pdfArray.Add(new PdfReferenceHolder(pdfDictionary3));
						pdfArray2.Add(new PdfReferenceHolder(pdfDictionary2));
						acroField["Kids"] = pdfArray;
						return pdfArray2;
					}
					return pdfArray;
				}
				return GetFieldArray(pdfDictionary);
			}
		}
		return null;
	}

	private PdfDictionary MapAcroFromFields(PdfDictionary acroFormData, PdfDictionary destinationForm)
	{
		if (destinationForm != null && destinationForm.ContainsKey("Fields"))
		{
			PdfArray pdfArray = PdfCrossTable.Dereference(destinationForm["Fields"]) as PdfArray;
			PdfArray pdfArray2 = null;
			if (acroFormData.ContainsKey("Fields"))
			{
				pdfArray2 = PdfCrossTable.Dereference(acroFormData["Fields"]) as PdfArray;
			}
			if (pdfArray != null && pdfArray.Count > 0 && pdfArray2 != null)
			{
				List<string> list = new List<string>();
				for (int i = 0; i < pdfArray.Count; i++)
				{
					if (PdfCrossTable.Dereference(pdfArray[i]) is PdfDictionary pdfDictionary && pdfDictionary.ContainsKey("T") && pdfDictionary["T"] is PdfString pdfString)
					{
						list.Add(pdfString.Value);
					}
				}
				for (int j = 0; j < pdfArray2.Count; j++)
				{
					if (pdfArray.Contains(pdfArray2[j]))
					{
						continue;
					}
					if (PdfCrossTable.Dereference(pdfArray2[j]) is PdfDictionary pdfDictionary2 && pdfDictionary2.ContainsKey("T") && pdfDictionary2["T"] is PdfString pdfString2)
					{
						if (!list.Contains(pdfString2.Value))
						{
							list.Add(pdfString2.Value);
						}
						else
						{
							pdfDictionary2["T"] = new PdfString(pdfString2.Value + 0);
							list.Add(pdfString2.Value);
						}
					}
					pdfArray.Add(new PdfReferenceHolder(pdfArray2[j]));
				}
				acroFormData["Fields"] = pdfArray;
			}
		}
		return acroFormData;
	}

	internal PdfPageBase GetPage(PdfDictionary dic)
	{
		Dictionary<PdfDictionary, PdfPageBase> pageCache = PageCache;
		PdfPageBase pdfPageBase = null;
		if (pageCache.ContainsKey(dic))
		{
			pdfPageBase = pageCache[dic];
		}
		if (pdfPageBase == null)
		{
			pdfPageBase = (pageCache[dic] = new PdfLoadedPage(m_document, m_crossTable, dic));
		}
		return pdfPageBase;
	}

	internal void UpdateCount(PdfDictionary parent)
	{
		while (parent != null)
		{
			int value = GetNodeCount(parent) + 1;
			parent.SetNumber("Count", value);
			parent = PdfCrossTable.Dereference(parent["Parent"]) as PdfDictionary;
		}
	}

	internal int IndexOf(PdfPageBase page)
	{
		if (page == null)
		{
			throw new ArgumentNullException("page");
		}
		return GetIndex(page);
	}

	internal int GetIndex(PdfPageBase page)
	{
		if (page == null)
		{
			throw new ArgumentNullException("page");
		}
		int result = -1;
		if (m_pageIndexCollection.Count == 0)
		{
			ParsePageNodes(page);
		}
		if (m_pageIndexCollection.ContainsKey(page.Dictionary))
		{
			result = m_pageIndexCollection[page.Dictionary];
		}
		return result;
	}

	private void ParsePageNodes(PdfPageBase page)
	{
		PdfDictionary pages = new PdfDictionary();
		if (m_crossTable != null && m_crossTable.DocumentCatalog != null)
		{
			pages = PdfCrossTable.Dereference(m_crossTable.DocumentCatalog["Pages"]) as PdfDictionary;
		}
		else if (page is PdfLoadedPage)
		{
			pages = PdfCrossTable.Dereference((page as PdfLoadedPage).Document.Catalog["Pages"]) as PdfDictionary;
		}
		else if (page is PdfPage)
		{
			pages = PdfCrossTable.Dereference((page as PdfPage).Document.Catalog["Pages"]) as PdfDictionary;
		}
		FindKids(pages);
	}

	private void FindKids(PdfDictionary pages)
	{
		if (PdfCrossTable.Dereference(pages["Kids"]) is PdfArray pdfArray)
		{
			for (int i = 0; i < pdfArray.Count; i++)
			{
				FindPageNodes(PdfCrossTable.Dereference(pdfArray[i]) as PdfDictionary);
			}
		}
	}

	private void FindPageNodes(PdfDictionary pageNode)
	{
		if (pageNode != null && pageNode.ContainsKey("Type"))
		{
			string text = string.Empty;
			PdfName pdfName = PdfCrossTable.Dereference(pageNode["Type"]) as PdfName;
			if (pdfName != null)
			{
				text = pdfName.Value;
			}
			if (text == "Page")
			{
				m_pageIndexCollection.Add(pageNode, ++m_pageIndex);
			}
			else if (text == "Pages")
			{
				FindKids(pageNode);
			}
		}
	}

	private void ResetPageCollection()
	{
		if (m_pageIndexCollection != null)
		{
			m_pageIndexCollection.Clear();
		}
		m_pageIndex = -1;
	}

	private PdfPageBase GetPage(int index)
	{
		if (!m_parseInvalidPages && m_document != null && m_document.CrossTable != null && m_document.CrossTable.CrossTable != null && m_document.CrossTable.CrossTable.IsStructureAltered && m_document.CrossTable.CrossTable.m_isOpenAndRepair)
		{
			objectReference = new List<long>();
			RemoveInvalidPageNodes();
			objectReference.Clear();
			objectReference = null;
			m_parseInvalidPages = true;
		}
		PdfDictionary parent = GetParent(index, out var localIndex, zeroValid: true, enableFastFetching: true);
		PdfArray nodeKids = GetNodeKids(parent);
		int num = localIndex;
		int num2 = 0;
		while (true)
		{
			parent = m_crossTable.GetObject(nodeKids[localIndex]) as PdfDictionary;
			string text = string.Empty;
			if (parent != null && parent.ContainsKey("Type"))
			{
				PdfName pdfName = PdfCrossTable.Dereference(parent["Type"]) as PdfName;
				if (pdfName != null)
				{
					text = pdfName.Value;
				}
			}
			if (!(text == "Pages"))
			{
				break;
			}
			num++;
			parent = m_crossTable.GetObject(nodeKids[num]) as PdfDictionary;
			PdfArray nodeKids2 = GetNodeKids(parent);
			if (nodeKids2 == null)
			{
				break;
			}
			if (nodeKids2.Count > 0)
			{
				parent = m_crossTable.GetObject(nodeKids2[num2]) as PdfDictionary;
				num2++;
				break;
			}
		}
		return GetPage(parent);
	}

	private bool IsNodeLeaf(PdfDictionary node)
	{
		return GetNodeCount(node) == 0;
	}

	private PdfArray GetNodeKids(PdfDictionary node)
	{
		PdfArray result = null;
		if (node != null)
		{
			IPdfPrimitive pointer = node["Kids"];
			result = m_crossTable.GetObject(pointer) as PdfArray;
		}
		return result;
	}

	private int GetNodeCount(PdfDictionary node)
	{
		IPdfPrimitive pointer = node["Count"];
		if (m_crossTable.GetObject(pointer) is PdfNumber pdfNumber)
		{
			return pdfNumber.IntValue;
		}
		return 0;
	}

	private PdfDictionary GetParent(int index, out int localIndex, bool zeroValid)
	{
		if (index < 0 && index > Count)
		{
			throw new ArgumentOutOfRangeException("index", "The index should be within this range: [0; Count]");
		}
		IPdfPrimitive pointer = m_document.Catalog["Pages"];
		PdfDictionary pdfDictionary = m_crossTable.GetObject(pointer) as PdfDictionary;
		int num = 0;
		localIndex = GetNodeCount(pdfDictionary);
		if (index == 0 && !zeroValid)
		{
			localIndex = 0;
		}
		else if (index < Count)
		{
			PdfArray nodeKids = GetNodeKids(pdfDictionary);
			if (m_document is PdfLoadedDocument && !(m_document as PdfLoadedDocument).EnableInitialLoadingOptimization)
			{
				for (int i = 0; i < nodeKids.Count; i++)
				{
					PdfReferenceHolder pdfReferenceHolder = nodeKids.Elements[i] as PdfReferenceHolder;
					if (!(pdfReferenceHolder != null))
					{
						continue;
					}
					foreach (KeyValuePair<PdfName, IPdfPrimitive> item in (pdfReferenceHolder.Object as PdfDictionary).Items)
					{
						if (item.Key.Value == "Kids")
						{
							PdfArray pdfArray = null;
							pdfArray = ((!(item.Value is PdfReferenceHolder)) ? (item.Value as PdfArray) : ((item.Value as PdfReferenceHolder).Object as PdfArray));
							if (pdfArray != null && pdfArray.Elements.Count == 0)
							{
								nodeKids.RemoveAt(i);
							}
						}
					}
				}
			}
			int j = 0;
			for (int count = nodeKids.Count; j < count; j++)
			{
				PdfDictionary pdfDictionary2 = m_crossTable.GetObject(nodeKids[j]) as PdfDictionary;
				string text = string.Empty;
				if (pdfDictionary2 != null)
				{
					PdfName pdfName = PdfCrossTable.Dereference(pdfDictionary2["Type"]) as PdfName;
					if (pdfName != null)
					{
						text = pdfName.Value;
					}
				}
				if (IsNodeLeaf(pdfDictionary2) && !(text == "Pages"))
				{
					if (num + j == index)
					{
						localIndex = j;
						break;
					}
					continue;
				}
				int nodeCount = GetNodeCount(pdfDictionary2);
				if (index < num + nodeCount + j)
				{
					num += j;
					pdfDictionary = pdfDictionary2;
					nodeKids = GetNodeKids(pdfDictionary);
					j = -1;
					count = nodeKids.Count;
					if (m_document is PdfLoadedDocument && (m_document as PdfLoadedDocument).EnableInitialLoadingOptimization && nodeCount == count)
					{
						text = ((m_crossTable.GetObject(nodeKids[0]) as PdfDictionary)["Type"] as PdfName).Value;
						if (!(text == "Pages"))
						{
							localIndex = index - num;
							break;
						}
					}
				}
				else
				{
					num += nodeCount - 1;
				}
			}
		}
		else
		{
			localIndex = GetNodeKids(pdfDictionary).Count;
		}
		return pdfDictionary;
	}

	private PdfDictionary GetParent(int index, out int localIndex, bool zeroValid, bool enableFastFetching)
	{
		if (index < 0 && index > Count)
		{
			throw new ArgumentOutOfRangeException("index", "The index should be within this range: [0; Count]");
		}
		if (!enableFastFetching)
		{
			return GetParent(index, out localIndex, zeroValid);
		}
		if (m_pageCatalogObj == null)
		{
			m_pageCatalogObj = m_document.Catalog["Pages"];
		}
		bool flag = false;
		PdfDictionary pdfDictionary = null;
		if (m_pageNodeDictionary == null)
		{
			m_pageNodeDictionary = m_crossTable.GetObject(m_pageCatalogObj) as PdfDictionary;
			pdfDictionary = m_pageNodeDictionary;
			m_pageNodeCount = GetNodeCount(pdfDictionary);
			m_lastCrossTable = m_crossTable;
			flag = true;
		}
		else if (m_lastCrossTable == m_crossTable)
		{
			pdfDictionary = m_pageNodeDictionary;
		}
		else
		{
			m_pageNodeDictionary = m_crossTable.GetObject(m_pageCatalogObj) as PdfDictionary;
			pdfDictionary = m_pageNodeDictionary;
			m_pageNodeCount = GetNodeCount(pdfDictionary);
			m_lastCrossTable = m_crossTable;
			flag = true;
		}
		int lowIndex = 0;
		if (m_pageNodeCount > 0)
		{
			localIndex = m_pageNodeCount;
		}
		else
		{
			localIndex = GetNodeCount(pdfDictionary);
		}
		if (index == 0 && !zeroValid)
		{
			localIndex = 0;
		}
		else if (index < Count)
		{
			PdfArray pdfArray = null;
			if (m_nodeKids == null || flag)
			{
				m_nodeKids = GetNodeKids(pdfDictionary);
				pdfArray = m_nodeKids;
				if (m_document is PdfLoadedDocument && !(m_document as PdfLoadedDocument).EnableInitialLoadingOptimization)
				{
					for (int i = 0; i < pdfArray.Count; i++)
					{
						PdfReferenceHolder pdfReferenceHolder = pdfArray.Elements[i] as PdfReferenceHolder;
						if (!(pdfReferenceHolder != null))
						{
							continue;
						}
						foreach (KeyValuePair<PdfName, IPdfPrimitive> item in (pdfReferenceHolder.Object as PdfDictionary).Items)
						{
							if (item.Key.Value == "Kids")
							{
								PdfArray pdfArray2 = null;
								pdfArray2 = ((!(item.Value is PdfReferenceHolder)) ? (item.Value as PdfArray) : ((item.Value as PdfReferenceHolder).Object as PdfArray));
								if (pdfArray2 != null && pdfArray2.Elements.Count == 0)
								{
									pdfArray.RemoveAt(i);
								}
							}
						}
					}
				}
			}
			else
			{
				pdfArray = m_nodeKids;
			}
			int kidStartIndex = 0;
			if ((m_lastPageIndex == index - 1 || m_lastPageIndex < index) && m_lastKidIndex < pdfArray.Count)
			{
				kidStartIndex = m_lastKidIndex;
			}
			bool isParentFetched = false;
			PdfDictionary node = null;
			int localIndex2 = 0;
			if (pdfArray.Count == Count)
			{
				if (pdfDictionary != null && !pdfDictionary.ContainsKey("Parent"))
				{
					kidStartIndex = index;
				}
				GetParentNode(kidStartIndex, pdfArray, lowIndex, index, out node, out localIndex2, out isParentFetched);
				if (!isParentFetched)
				{
					GetParentNode(0, pdfArray, lowIndex, index, out node, out localIndex2, out isParentFetched);
				}
			}
			else
			{
				GetParentNode(0, pdfArray, lowIndex, index, out node, out localIndex2, out isParentFetched);
			}
			if (node != null)
			{
				pdfDictionary = node;
			}
			if (localIndex2 != -1)
			{
				localIndex = localIndex2;
			}
			if (m_invalidPageNode)
			{
				localIndex = GetNodeKids(pdfDictionary).Count - 1;
			}
		}
		else
		{
			localIndex = GetNodeKids(pdfDictionary).Count;
		}
		m_lastPageIndex = index;
		return pdfDictionary;
	}

	private void GetParentNode(int kidStartIndex, PdfArray kids, int lowIndex, int pageIndex, out PdfDictionary node, out int localIndex, out bool isParentFetched)
	{
		isParentFetched = false;
		node = null;
		localIndex = -1;
		bool flag = false;
		int i = kidStartIndex;
		for (int count = kids.Count; i < count; i++)
		{
			PdfDictionary pdfDictionary = m_crossTable.GetObject(kids[i]) as PdfDictionary;
			string text = string.Empty;
			if (pdfDictionary != null && pdfDictionary.ContainsKey("Type"))
			{
				PdfName pdfName = PdfCrossTable.Dereference(pdfDictionary["Type"]) as PdfName;
				if (pdfName != null)
				{
					text = pdfName.Value;
				}
			}
			if (pdfDictionary != null)
			{
				if (IsNodeLeaf(pdfDictionary) && !(text == "Pages"))
				{
					if (lowIndex + i == pageIndex)
					{
						localIndex = i;
						isParentFetched = true;
						if (!flag)
						{
							m_lastKidIndex = i;
						}
						break;
					}
					continue;
				}
				int nodeCount = GetNodeCount(pdfDictionary);
				if (pageIndex < lowIndex + nodeCount + i)
				{
					flag = true;
					m_lastKidIndex = i;
					lowIndex += i;
					node = pdfDictionary;
					kids = GetNodeKids(node);
					i = -1;
					count = kids.Count;
					if (m_document is PdfLoadedDocument && (m_document as PdfLoadedDocument).EnableInitialLoadingOptimization && nodeCount == count)
					{
						text = ((m_crossTable.GetObject(kids[0]) as PdfDictionary)["Type"] as PdfName).Value;
						if (!(text == "Pages"))
						{
							localIndex = pageIndex - lowIndex;
							break;
						}
					}
				}
				else
				{
					lowIndex += nodeCount - 1;
				}
				continue;
			}
			kids.RemoveAt(i);
			m_invalidPageNode = true;
			if (node == null || !node.ContainsKey("Count") || !(PdfCrossTable.Dereference(node["Count"]) is PdfNumber pdfNumber))
			{
				continue;
			}
			int value = pdfNumber.IntValue - 1;
			node["Count"] = new PdfNumber(value);
			if (node.ContainsKey("Parent") && PdfCrossTable.Dereference(node["Parent"]) is PdfDictionary pdfDictionary2 && pdfDictionary2.ContainsKey("Count") && PdfCrossTable.Dereference(pdfDictionary2["Count"]) is PdfNumber pdfNumber2)
			{
				int value2 = pdfNumber2.IntValue - 1;
				pdfDictionary2["Count"] = new PdfNumber(value2);
				if (m_crossTable != null && m_pageCatalogObj != null && m_crossTable.GetObject(m_pageCatalogObj) is PdfDictionary pdfDictionary3 && PdfCrossTable.Dereference(pdfDictionary3["Count"]) is PdfNumber pdfNumber3)
				{
					int value3 = pdfNumber3.IntValue - 1;
					pdfDictionary3["Count"] = new PdfNumber(value3);
				}
			}
		}
	}

	internal void Clear(bool isCompletely)
	{
		if (m_pagesCash != null && m_pagesCash.Count > 0)
		{
			if (m_document != null && m_document.Catalog != null)
			{
				IPdfPrimitive pdfPrimitive = m_document.Catalog["Pages"];
				if (pdfPrimitive != null && m_crossTable != null && PdfCrossTable.Dereference(m_crossTable.GetObject(pdfPrimitive)) is PdfDictionary pdfDictionary)
				{
					pdfDictionary.Clear();
				}
			}
			m_pagesCash.Clear();
		}
		if (isCompletely && m_crossTable != null)
		{
			m_crossTable.isCompletely = true;
			m_crossTable.m_closeCompletely = m_closeCompletely;
			if (m_crossTable.CrossTable != null)
			{
				m_crossTable.CrossTable.m_closeCompletely = m_closeCompletely;
			}
			m_crossTable.Close(completely: true);
			m_crossTable.isCompletely = false;
		}
		m_crossTable = null;
		m_document = null;
		m_loadedDocument = null;
	}

	public IEnumerator GetEnumerator()
	{
		return new PdfLoadedPageEnumerator(this);
	}

	private void RemoveInvalidPageNodes()
	{
		m_pageCatalogObj = m_document.Catalog["Pages"];
		m_pageNodeDictionary = m_crossTable.GetObject(m_pageCatalogObj) as PdfDictionary;
		PdfReferenceHolder pdfReferenceHolder = m_pageCatalogObj as PdfReferenceHolder;
		if (pdfReferenceHolder != null && pdfReferenceHolder.Reference != null)
		{
			objectReference.Add(pdfReferenceHolder.Reference.ObjNum);
		}
		ParseKids(m_pageNodeDictionary);
	}

	private void ParseKids(PdfDictionary pages)
	{
		if (!(PdfCrossTable.Dereference(pages["Kids"]) is PdfArray pdfArray))
		{
			return;
		}
		for (int i = 0; i < pdfArray.Count; i++)
		{
			PdfReferenceHolder pdfReferenceHolder = pdfArray[i] as PdfReferenceHolder;
			if (pdfReferenceHolder != null && pdfReferenceHolder.Reference != null)
			{
				if (objectReference.Contains(pdfReferenceHolder.Reference.ObjNum))
				{
					RemovePageNode(pdfArray, i, pages);
					i--;
					continue;
				}
				objectReference.Add(pdfReferenceHolder.Reference.ObjNum);
			}
			bool invalid = false;
			ParsePageNodes(PdfCrossTable.Dereference(pdfArray[i]) as PdfDictionary, out invalid);
			if (invalid)
			{
				RemovePageNode(pdfArray, i, pages);
				i--;
			}
		}
	}

	private void ParsePageNodes(PdfDictionary pageNode, out bool invalid)
	{
		invalid = false;
		if (pageNode == null || !pageNode.ContainsKey("Type"))
		{
			return;
		}
		string text = string.Empty;
		PdfName pdfName = PdfCrossTable.Dereference(pageNode["Type"]) as PdfName;
		if (pdfName != null)
		{
			text = pdfName.Value;
		}
		if (text == "Page")
		{
			return;
		}
		if (text == "Pages")
		{
			ParseKids(pageNode);
			if (pageNode.ContainsKey("Count") && PdfCrossTable.Dereference(pageNode["Count"]) is PdfNumber { IntValue: 0 })
			{
				invalid = true;
			}
		}
		else
		{
			invalid = true;
		}
	}

	private void RemovePageNode(PdfArray kids, int i, PdfDictionary node)
	{
		kids.RemoveAt(i);
		if (node == null || !node.ContainsKey("Count") || !(PdfCrossTable.Dereference(node["Count"]) is PdfNumber pdfNumber))
		{
			return;
		}
		int value = pdfNumber.IntValue - 1;
		node["Count"] = new PdfNumber(value);
		if (node.ContainsKey("Parent") && PdfCrossTable.Dereference(node["Parent"]) is PdfDictionary pdfDictionary && pdfDictionary.ContainsKey("Count") && PdfCrossTable.Dereference(pdfDictionary["Count"]) is PdfNumber pdfNumber2)
		{
			int value2 = pdfNumber2.IntValue - 1;
			pdfDictionary["Count"] = new PdfNumber(value2);
			if (m_crossTable != null && m_pageCatalogObj != null && m_crossTable.GetObject(m_pageCatalogObj) is PdfDictionary pdfDictionary2 && PdfCrossTable.Dereference(pdfDictionary2["Count"]) is PdfNumber pdfNumber3)
			{
				int value3 = pdfNumber3.IntValue - 1;
				pdfDictionary2["Count"] = new PdfNumber(value3);
			}
		}
	}
}
