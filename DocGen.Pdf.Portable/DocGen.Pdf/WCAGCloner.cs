using System;
using System.Collections.Generic;
using System.Linq;
using DocGen.Pdf.IO;
using DocGen.Pdf.Parsing;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf;

internal class WCAGCloner : IDisposable
{
	private List<IPdfPrimitive> m_taggedObjects;

	private int structureElementCount;

	private PdfDictionary m_catalog;

	private PdfCrossTable m_crossTable;

	private Dictionary<IPdfPrimitive, object> pageCollection;

	private PdfDictionary m_currentPageDict;

	private PdfPageBase m_currentPage;

	private PdfDocumentBase m_document;

	private PdfDictionary m_tableDictionary;

	private PdfDictionary Catalog => m_catalog;

	private PdfDocumentBase Document => m_document;

	private Dictionary<IPdfPrimitive, object> PageCorrespondance => pageCollection;

	private List<IPdfPrimitive> TaggedPDFObjects
	{
		get
		{
			if (m_taggedObjects == null)
			{
				m_taggedObjects = new List<IPdfPrimitive>();
			}
			return m_taggedObjects;
		}
	}

	private PdfCrossTable CrossTable => m_crossTable;

	private PdfPageBase CurrentPage => m_currentPage;

	private PdfDictionary CurrentPageDictionary => m_currentPageDict;

	internal WCAGCloner(PdfCatalog catalog, PdfDocumentBase doc)
	{
		m_catalog = catalog;
		m_document = doc;
	}

	internal void ImportingStructureElements(int startIndex, int endIndex, PdfLoadedDocument ldDoc, Dictionary<IPdfPrimitive, object> pageCorrespondance, Dictionary<int, PdfPageBase> loadedPageCollection)
	{
		for (int i = startIndex; i <= endIndex; i++)
		{
			PdfPageBase pdfPageBase = loadedPageCollection[i];
			if (pdfPageBase == null)
			{
				continue;
			}
			pageCollection = new Dictionary<IPdfPrimitive, object>();
			PdfPageBase pdfPageBase2 = pageCorrespondance[((IPdfWrapper)pdfPageBase).Element] as PdfPageBase;
			pageCollection[((IPdfWrapper)pdfPageBase).Element] = pdfPageBase2;
			m_currentPageDict = pdfPageBase2.Dictionary;
			m_currentPage = pdfPageBase2;
			if (PdfCrossTable.Dereference(ldDoc.Catalog["StructTreeRoot"]) is PdfDictionary original)
			{
				m_crossTable = ldDoc.CrossTable;
				PdfDictionary pdfDictionary = new PdfDictionary();
				PdfDictionary dictionary = CopyPDFDictionary(original);
				StructureDictionaryCopier(pdfDictionary, dictionary, null);
				PdfStructureElement structTreeRoot = ldDoc.GetStructTreeRoot(pdfDictionary);
				if (structTreeRoot != null)
				{
					m_tableDictionary = structTreeRoot.Dictionary;
				}
				ImportWCAG(structTreeRoot);
			}
		}
		List<IPdfPrimitive> taggedPDFObjects = TaggedPDFObjects;
		PdfDictionary pdfDictionary2 = new PdfDictionary();
		PdfDictionary pdfDictionary3 = new PdfDictionary();
		PdfDictionary pdfDictionary4 = new PdfDictionary();
		PdfArray pdfArray = new PdfArray();
		PdfDictionary pdfDictionary5 = new PdfDictionary();
		PdfArray pdfArray2 = new PdfArray();
		if (ldDoc.IsPDFMerge && !m_document.m_isFirstDocument && m_document.m_structElemnt != null)
		{
			pdfDictionary2 = m_document.m_structElemnt;
			if (pdfDictionary2 != null && PdfCrossTable.Dereference(pdfDictionary2["K"]) is PdfDictionary pdfDictionary6)
			{
				if (PdfCrossTable.Dereference(pdfDictionary6["K"]) is PdfDictionary pdfDictionary7)
				{
					if (PdfCrossTable.Dereference(pdfDictionary7["K"]) is PdfArray pdfArray3)
					{
						foreach (IPdfPrimitive item in taggedPDFObjects)
						{
							PdfDictionary dictionary2 = item as PdfDictionary;
							UpdateParentKey(dictionary2, pdfDictionary7);
							pdfArray3.Add(new PdfReferenceHolder(item));
						}
					}
				}
				else if (PdfCrossTable.Dereference(pdfDictionary6["K"]) is PdfArray pdfArray4)
				{
					foreach (IPdfPrimitive item2 in taggedPDFObjects)
					{
						PdfDictionary dictionary3 = item2 as PdfDictionary;
						UpdateParentKey(dictionary3, pdfDictionary6);
						pdfArray4.Add(new PdfReferenceHolder(item2));
					}
				}
				pdfDictionary5 = PdfCrossTable.Dereference(pdfDictionary2["ParentTree"]) as PdfDictionary;
				if (pdfDictionary5 != null)
				{
					pdfArray2 = PdfCrossTable.Dereference(pdfDictionary5["Nums"]) as PdfArray;
					if (pdfArray2 == null)
					{
						pdfArray2 = new PdfArray();
					}
				}
			}
		}
		else
		{
			pdfDictionary2["Type"] = new PdfName("StructTreeRoot");
			if (!ldDoc.IsPDFMerge)
			{
				Catalog["StructTreeRoot"] = new PdfReferenceHolder(pdfDictionary2);
			}
			if (m_tableDictionary != null && m_tableDictionary.ContainsKey("S"))
			{
				PdfName pdfName = m_tableDictionary["S"] as PdfName;
				if (pdfName != null && pdfName.Value != "Document")
				{
					pdfDictionary3["S"] = new PdfName("Document");
					pdfDictionary4["P"] = new PdfReferenceHolder(pdfDictionary3);
					pdfDictionary2["K"] = new PdfReferenceHolder(pdfDictionary3);
					pdfDictionary3["K"] = new PdfReferenceHolder(pdfDictionary4);
					pdfDictionary3["P"] = new PdfReferenceHolder(pdfDictionary2);
					pdfDictionary4["K"] = pdfArray;
					pdfDictionary4["S"] = new PdfName(pdfName.Value);
				}
				else
				{
					pdfDictionary4["S"] = new PdfName("Document");
					pdfDictionary4["P"] = new PdfReferenceHolder(pdfDictionary2);
					pdfDictionary2["K"] = new PdfReferenceHolder(pdfDictionary4);
					pdfDictionary4["K"] = pdfArray;
				}
			}
			foreach (IPdfPrimitive item3 in taggedPDFObjects)
			{
				PdfDictionary dictionary4 = item3 as PdfDictionary;
				UpdateParentKey(dictionary4, pdfDictionary4);
				pdfArray.Add(new PdfReferenceHolder(item3));
			}
			pdfDictionary2["ParentTree"] = new PdfReferenceHolder(pdfDictionary5);
			if (ldDoc.IsPDFMerge)
			{
				m_document.m_structElemnt = pdfDictionary2;
			}
			m_document.m_isFirstDocument = false;
		}
		int num = 0;
		if (ldDoc.IsPDFMerge && pdfDictionary2.ContainsKey("ParentTreeNextKey") && pdfDictionary2["ParentTreeNextKey"] is PdfNumber pdfNumber)
		{
			num += pdfNumber.IntValue;
		}
		for (int j = startIndex; j <= endIndex; j++)
		{
			PdfPageBase pdfPageBase3 = loadedPageCollection[j];
			PdfPageBase pdfPageBase4 = pageCorrespondance[((IPdfWrapper)pdfPageBase3).Element] as PdfPageBase;
			List<IPdfPrimitive> mcrObjectCollection = pdfPageBase4.McrObjectCollection;
			if (mcrObjectCollection == null || mcrObjectCollection.Count <= 0)
			{
				continue;
			}
			foreach (IPdfPrimitive item4 in mcrObjectCollection)
			{
				pdfArray2.Add(new PdfNumber(num));
				pdfArray2.Add(new PdfReferenceHolder(item4));
				num++;
			}
			pdfPageBase4.Dictionary["Tabs"] = new PdfName("S");
		}
		for (int k = startIndex; k <= endIndex; k++)
		{
			PdfPageBase pdfPageBase5 = loadedPageCollection[k];
			PdfPageBase pdfPageBase6 = pageCorrespondance[((IPdfWrapper)pdfPageBase5).Element] as PdfPageBase;
			List<IPdfPrimitive> mcrContentCollection = pdfPageBase6.McrContentCollection;
			if (mcrContentCollection.Count > 0)
			{
				pdfPageBase6.Dictionary["Tabs"] = new PdfName("S");
				pdfPageBase6.Dictionary["StructParents"] = new PdfNumber(num);
				pdfArray2.Add(new PdfNumber(num));
				PdfArray pdfArray5 = new PdfArray();
				Dictionary<int, IPdfPrimitive> dictionary5 = new Dictionary<int, IPdfPrimitive>();
				foreach (IPdfPrimitive item5 in mcrContentCollection)
				{
					if (!(item5 is PdfDictionary pdfDictionary8))
					{
						continue;
					}
					if (PdfCrossTable.Dereference(pdfDictionary8["K"]) is PdfNumber pdfNumber2)
					{
						dictionary5[pdfNumber2.IntValue] = pdfDictionary8;
					}
					else if (PdfCrossTable.Dereference(pdfDictionary8["K"]) is PdfDictionary pdfDictionary9 && pdfDictionary9.ContainsKey("MCID"))
					{
						if (PdfCrossTable.Dereference(pdfDictionary9["MCID"]) is PdfNumber pdfNumber3)
						{
							dictionary5[pdfNumber3.IntValue] = pdfDictionary8;
						}
					}
					else
					{
						if (!(PdfCrossTable.Dereference(pdfDictionary8["K"]) is PdfArray pdfArray6))
						{
							continue;
						}
						foreach (IPdfPrimitive element in pdfArray6.Elements)
						{
							if (PdfCrossTable.Dereference(element) is PdfNumber pdfNumber4)
							{
								dictionary5[pdfNumber4.IntValue] = pdfDictionary8;
							}
							else if (PdfCrossTable.Dereference(element) is PdfDictionary pdfDictionary10 && pdfDictionary10.ContainsKey("MCID") && PdfCrossTable.Dereference(pdfDictionary10["MCID"]) is PdfNumber pdfNumber5)
							{
								dictionary5[pdfNumber5.IntValue] = pdfDictionary8;
							}
						}
					}
				}
				int[] array = dictionary5.Keys.ToArray();
				Array.Sort(array);
				int num2 = array[^1];
				for (int l = 0; l <= num2; l++)
				{
					IPdfPrimitive value = null;
					dictionary5.TryGetValue(l, out value);
					if (value != null)
					{
						pdfArray5.Add(new PdfReferenceHolder(value));
					}
					else
					{
						pdfArray5.Add(new PdfNull());
					}
				}
				pdfArray2.Add(pdfArray5);
				num++;
			}
			pdfPageBase6.McrContentCollection.Clear();
			pdfPageBase6.McrObjectCollection.Clear();
			pdfPageBase6.ImportedAnnotationReference.Clear();
		}
		taggedPDFObjects.Clear();
		TaggedPDFObjects.Clear();
		structureElementCount = 0;
		pdfDictionary2["ParentTreeNextKey"] = new PdfNumber(num);
		pdfDictionary5["Nums"] = pdfArray2;
		if (!(PdfCrossTable.Dereference(ldDoc.Catalog["StructTreeRoot"]) is PdfDictionary pdfDictionary11) || !pdfDictionary11.ContainsKey("RoleMap") || !(PdfCrossTable.Dereference(pdfDictionary11["RoleMap"]) is PdfDictionary pdfDictionary12))
		{
			return;
		}
		PdfDictionary pdfDictionary13 = pdfDictionary12.Clone(CrossTable) as PdfDictionary;
		if (pdfDictionary2.ContainsKey("RoleMap"))
		{
			PdfDictionary pdfDictionary14 = PdfCrossTable.Dereference(pdfDictionary2["RoleMap"]) as PdfDictionary;
			if (pdfDictionary14 != null && pdfDictionary13 != null)
			{
				foreach (KeyValuePair<PdfName, IPdfPrimitive> item6 in pdfDictionary13.Items)
				{
					PdfName key = item6.Key;
					IPdfPrimitive pdfPrimitive = item6.Value.Clone(CrossTable);
					if (!(pdfPrimitive is PdfNull))
					{
						pdfDictionary14.SetProperty(key, pdfPrimitive);
					}
				}
			}
			pdfDictionary2["RoleMap"] = new PdfDictionary(pdfDictionary14);
		}
		else
		{
			pdfDictionary2["RoleMap"] = new PdfDictionary(pdfDictionary13);
		}
	}

	private void UpdateParentKey(PdfDictionary dictionary, PdfDictionary root)
	{
		dictionary["P"] = new PdfReferenceHolder(root);
		if (!dictionary.ContainsKey("K") || !(PdfCrossTable.Dereference(dictionary["K"]) is PdfArray { Count: >0 } pdfArray))
		{
			return;
		}
		for (int i = 0; i < pdfArray.Count; i++)
		{
			if (PdfCrossTable.Dereference(pdfArray[i]) is PdfDictionary { isSkip: false } pdfDictionary)
			{
				UpdateParentKey(pdfDictionary, dictionary);
			}
		}
		PdfArray pdfArray2 = CopyArray(pdfArray);
		if (pdfArray2 != null && pdfArray2.Count > 0)
		{
			dictionary["K"] = new PdfReferenceHolder(pdfArray2);
		}
	}

	private PdfArray CopyArray(PdfArray original)
	{
		PdfArray pdfArray = new PdfArray();
		for (int i = 0; i < original.Count; i++)
		{
			PdfDictionary pdfDictionary = PdfCrossTable.Dereference(original[i]) as PdfDictionary;
			PdfNumber pdfNumber = PdfCrossTable.Dereference(original[i]) as PdfNumber;
			if ((pdfDictionary != null && !pdfDictionary.isSkip) || pdfNumber != null)
			{
				pdfArray.Add(original[i]);
			}
		}
		return pdfArray;
	}

	private PdfDictionary BuildParentTree(PdfArray numsRoot)
	{
		int num = 40;
		int[] array = new int[numsRoot.Count / 2];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = i;
		}
		PdfArray pdfArray = new PdfArray();
		for (int j = 0; j < numsRoot.Count; j++)
		{
			if (j % 2 != 0)
			{
				pdfArray.Add(numsRoot[j]);
			}
		}
		int num2 = num;
		PdfDictionary[] array2 = new PdfDictionary[(array.Length + num - 1) / num];
		for (int k = 0; k < array2.Length; k++)
		{
			int l = k * num;
			int num3 = Math.Min(l + num, array.Length);
			PdfDictionary pdfDictionary = new PdfDictionary();
			PdfArray pdfArray2 = new PdfArray();
			pdfArray2.Add(new PdfNumber(array[l]));
			pdfArray2.Add(new PdfNumber(array[num3 - 1]));
			pdfDictionary[new PdfName("Limits")] = pdfArray2;
			pdfArray2 = new PdfArray();
			for (; l < num3; l++)
			{
				pdfArray2.Add(new PdfNumber(array[l]));
				pdfArray2.Add(pdfArray[l]);
			}
			pdfDictionary[new PdfName("Nums")] = pdfArray2;
			array2[k] = pdfDictionary;
		}
		int num4 = array2.Length;
		while (num4 > num)
		{
			num2 *= num;
			int num5 = (array.Length + num2 - 1) / num2;
			for (int m = 0; m < num5; m++)
			{
				int n = m * num;
				int num6 = Math.Min(n + num, num4);
				PdfDictionary pdfDictionary2 = new PdfDictionary();
				PdfArray pdfArray3 = new PdfArray();
				pdfArray3.Add(new PdfNumber(array[m * num2]));
				pdfArray3.Add(new PdfNumber(array[Math.Min((m + 1) * num2, array.Length) - 1]));
				pdfDictionary2[new PdfName("Limits")] = pdfArray3;
				pdfArray3 = new PdfArray();
				for (; n < num6; n++)
				{
					pdfArray3.Add(array2[n]);
				}
				pdfDictionary2[new PdfName("Kids")] = pdfArray3;
				array2[m] = pdfDictionary2;
			}
			num4 = num5;
		}
		PdfArray pdfArray4 = new PdfArray();
		for (int num7 = 0; num7 < num4; num7++)
		{
			pdfArray4.Add(new PdfReferenceHolder(array2[num7]));
		}
		return new PdfDictionary { [new PdfName("Kids")] = new PdfReferenceHolder(pdfArray4) };
	}

	private PdfDictionary CopyPDFDictionary(PdfDictionary original)
	{
		PdfDictionary pdfDictionary = new PdfDictionary();
		if (original != null)
		{
			foreach (PdfName item in original.Keys.ToList())
			{
				if (item.Value == "Type" || item.Value == "K")
				{
					pdfDictionary[item.Value] = original[item];
				}
			}
		}
		return pdfDictionary;
	}

	private void ImportWCAG(PdfStructureElement elementRoot)
	{
		List<IPdfPrimitive> list = CurrentPage.McrContentCollection;
		List<IPdfPrimitive> list2 = CurrentPage.McrObjectCollection;
		if (list == null)
		{
			list = new List<IPdfPrimitive>();
		}
		if (list2 == null)
		{
			list2 = new List<IPdfPrimitive>();
		}
		if (elementRoot != null)
		{
			PdfStructureElement[] array = ((elementRoot.ChildElements != null && elementRoot.ChildElements.Length != 0) ? elementRoot.ChildElements : new PdfStructureElement[1] { elementRoot });
			foreach (PdfStructureElement pdfStructureElement in array)
			{
				PdfDictionary dictionary = pdfStructureElement.Dictionary;
				if (pdfStructureElement.Page != null || !IsChildPageIsNull(pdfStructureElement))
				{
					TaggedPDFObjects.Add(dictionary);
					AddElements(dictionary, list, list2);
					if (pdfStructureElement.ChildElements != null && pdfStructureElement.ChildElements.Length != 0)
					{
						UpdateChildStructureElemtents(pdfStructureElement, list, list2, dictionary);
					}
				}
			}
		}
		CurrentPage.McrContentCollection = list;
		CurrentPage.McrObjectCollection = list2;
	}

	private bool IsChildPageIsNull(PdfStructureElement structureElement)
	{
		bool flag = true;
		if (structureElement.ChildElements != null && structureElement.ChildElements.Length != 0)
		{
			PdfStructureElement[] childElements = structureElement.ChildElements;
			foreach (PdfStructureElement pdfStructureElement in childElements)
			{
				if (pdfStructureElement.Page != null)
				{
					return false;
				}
				if (pdfStructureElement.ChildElements != null)
				{
					flag = IsChildPageIsNull(pdfStructureElement);
					if (!flag)
					{
						return false;
					}
				}
			}
		}
		return flag;
	}

	private PdfStructureElement IsChildInnerPageIsNull(PdfStructureElement structureElement)
	{
		if (structureElement.ChildElements != null && structureElement.ChildElements.Length != 0)
		{
			for (int i = 0; i < structureElement.ChildElements.Length; i++)
			{
				PdfStructureElement pdfStructureElement = structureElement.ChildElements[i];
				if (pdfStructureElement.Page != null)
				{
					return pdfStructureElement;
				}
				if (pdfStructureElement.ChildElements != null)
				{
					PdfStructureElement pdfStructureElement2 = IsChildInnerPageIsNull(pdfStructureElement);
					if (pdfStructureElement2 != null && pdfStructureElement2.Page != null)
					{
						return pdfStructureElement2;
					}
				}
			}
		}
		return null;
	}

	private void UpdateChildStructureElemtents(PdfStructureElement structureElement, List<IPdfPrimitive> McrContentCollection, List<IPdfPrimitive> McrObjectCollection, PdfDictionary parent)
	{
		for (int i = 0; i < structureElement.ChildElements.Length; i++)
		{
			PdfStructureElement pdfStructureElement = structureElement.ChildElements[i];
			PdfDictionary dictionary = pdfStructureElement.Dictionary;
			dictionary = GetChildFromParent(i, parent);
			if (dictionary == null)
			{
				continue;
			}
			bool flag = true;
			if (dictionary.ContainsKey("S"))
			{
				PdfName pdfName = PdfCrossTable.Dereference(dictionary["S"]) as PdfName;
				if (pdfName != null && (pdfName.Value == "TH" || pdfName.Value == "TD" || pdfName.Value == "TR"))
				{
					flag = false;
				}
			}
			if (pdfStructureElement.Page == null)
			{
				PdfStructureElement pdfStructureElement2 = IsChildInnerPageIsNull(pdfStructureElement);
				if (pdfStructureElement2 == null && !dictionary.ContainsKey("MCID") && !dictionary.ContainsKey("Obj") && flag)
				{
					RemoveElement(parent, dictionary);
					continue;
				}
				if (pdfStructureElement2 != null && !pdfStructureElement2.Dictionary.ContainsKey("K") && !pdfStructureElement2.Dictionary.ContainsKey("MCID") && !pdfStructureElement2.Dictionary.ContainsKey("Obj"))
				{
					RemoveElement(parent, dictionary);
					continue;
				}
			}
			else if (!dictionary.ContainsKey("K") && !dictionary.ContainsKey("MCID") && !dictionary.ContainsKey("Obj") && flag)
			{
				RemoveElement(parent, dictionary);
				continue;
			}
			AddElements(dictionary, McrContentCollection, McrObjectCollection);
			if (pdfStructureElement.ChildElements != null && pdfStructureElement.ChildElements.Length != 0)
			{
				UpdateChildStructureElemtents(pdfStructureElement, McrContentCollection, McrObjectCollection, dictionary);
			}
		}
	}

	private void RemoveElement(PdfDictionary parent, PdfDictionary elementDictionary)
	{
		PdfArray pdfArray = PdfCrossTable.Dereference(parent["K"]) as PdfArray;
		int num = 0;
		if (pdfArray == null || pdfArray.Count <= 0)
		{
			return;
		}
		foreach (IPdfPrimitive item in pdfArray)
		{
			PdfDictionary pdfDictionary = PdfCrossTable.Dereference(item) as PdfDictionary;
			if (elementDictionary == pdfDictionary)
			{
				pdfDictionary.isSkip = true;
				pdfArray.Skip = true;
				break;
			}
			num++;
		}
	}

	private PdfDictionary GetChildFromParent(int i, PdfDictionary parent)
	{
		PdfDictionary pdfDictionary = null;
		if (PdfCrossTable.Dereference(parent["K"]) is PdfArray { Count: >0 } pdfArray)
		{
			List<PdfDictionary> list = new List<PdfDictionary>();
			foreach (IPdfPrimitive item2 in pdfArray)
			{
				if (PdfCrossTable.Dereference(item2) is PdfDictionary item)
				{
					list.Add(item);
				}
				if (list.Count > i)
				{
					break;
				}
			}
			if (i < list.Count)
			{
				return list[i];
			}
			return null;
		}
		return PdfCrossTable.Dereference(parent["K"]) as PdfDictionary;
	}

	private void AddElements(PdfDictionary elementDictionary, List<IPdfPrimitive> McrContentCollection, List<IPdfPrimitive> McrObjectCollection)
	{
		if (elementDictionary == null || !elementDictionary.ContainsKey("K"))
		{
			return;
		}
		if (PdfCrossTable.Dereference(elementDictionary["K"]) is PdfNumber)
		{
			AddMarkedContentTypeElements(elementDictionary, McrContentCollection);
		}
		else if (PdfCrossTable.Dereference(elementDictionary["K"]) is PdfDictionary pdfDictionary && pdfDictionary.ContainsKey("Type"))
		{
			if (pdfDictionary.ContainsKey("MCID"))
			{
				if (PdfCrossTable.Dereference(pdfDictionary["MCID"]) is PdfNumber)
				{
					AddMarkedContentTypeElements(elementDictionary, McrContentCollection);
				}
			}
			else
			{
				AddObjectTypeElements(pdfDictionary, McrObjectCollection, elementDictionary);
			}
		}
		else
		{
			if (!(PdfCrossTable.Dereference(elementDictionary["K"]) is PdfArray pdfArray))
			{
				return;
			}
			List<IPdfPrimitive> list = new List<IPdfPrimitive>();
			foreach (IPdfPrimitive element in pdfArray.Elements)
			{
				if (PdfCrossTable.Dereference(element) is PdfNumber)
				{
					if (elementDictionary.ContainsKey("Pg"))
					{
						AddMarkedContentTypeElements(elementDictionary, McrContentCollection);
					}
					else
					{
						list.Add(element);
					}
				}
				else
				{
					if (!(PdfCrossTable.Dereference(element) is PdfDictionary pdfDictionary2) || !pdfDictionary2.ContainsKey("Type"))
					{
						continue;
					}
					if (pdfDictionary2.ContainsKey("MCID"))
					{
						if (PdfCrossTable.Dereference(pdfDictionary2["MCID"]) is PdfNumber)
						{
							AddMarkedContentTypeElements(elementDictionary, McrContentCollection);
						}
					}
					else
					{
						AddObjectTypeElements(pdfDictionary2, McrObjectCollection, elementDictionary);
					}
				}
			}
			foreach (IPdfPrimitive item in list)
			{
				pdfArray.Remove(item);
			}
			list.Clear();
		}
	}

	private void AddMarkedContentTypeElements(PdfDictionary elementDictionary, List<IPdfPrimitive> McrContentCollection)
	{
		McrContentCollection.Add(elementDictionary);
		elementDictionary["Pg"] = new PdfReferenceHolder(CurrentPageDictionary);
	}

	private void AddObjectTypeElements(PdfDictionary kDictionaryType, List<IPdfPrimitive> McrObjectCollection, PdfDictionary elementDictionary)
	{
		PdfName pdfName = PdfCrossTable.Dereference(kDictionaryType["Type"]) as PdfName;
		if (pdfName != null && pdfName.Value == "OBJR")
		{
			McrObjectCollection.Add(elementDictionary);
			if (PdfCrossTable.Dereference(kDictionaryType["Obj"]) is PdfDictionary pdfDictionary)
			{
				pdfDictionary["StructParent"] = new PdfNumber(structureElementCount);
				structureElementCount++;
				kDictionaryType["Pg"] = new PdfReferenceHolder(CurrentPageDictionary);
			}
		}
	}

	private void StructureDictionaryCopier(PdfDictionary copyDictionary, PdfDictionary dictionary, IPdfPrimitive parent)
	{
		if (dictionary == null || dictionary.Count <= 0)
		{
			return;
		}
		bool flag = true;
		if (dictionary.ContainsKey("S"))
		{
			PdfName pdfName = PdfCrossTable.Dereference(dictionary["S"]) as PdfName;
			if (pdfName != null && pdfName.Value == "Document" && dictionary.ContainsKey("Pg"))
			{
				flag = false;
			}
		}
		if (dictionary.ContainsKey("Pg") && flag && PdfCrossTable.Dereference(dictionary["Pg"]) is PdfDictionary key)
		{
			object value = null;
			PageCorrespondance.TryGetValue(key, out value);
			if (value == null)
			{
				bool flag2 = true;
				if (dictionary.ContainsKey("K") && PdfCrossTable.Dereference(dictionary["K"]) is PdfArray child)
				{
					flag2 = IsAllChildIsNull(child);
				}
				if (flag2)
				{
					return;
				}
			}
			else
			{
				copyDictionary["Pg"] = new PdfReferenceHolder(CurrentPageDictionary);
			}
		}
		foreach (KeyValuePair<PdfName, IPdfPrimitive> item in dictionary.Items)
		{
			if (item.Key.Value == "Pg")
			{
				continue;
			}
			if (item.Key.Value == "P")
			{
				if (parent != null)
				{
					copyDictionary["P"] = new PdfReferenceHolder(parent);
				}
			}
			else if (item.Key.Value == "Obj")
			{
				PdfReferenceHolder pdfReferenceHolder = dictionary["Obj"] as PdfReferenceHolder;
				if (!(pdfReferenceHolder != null) || !(pdfReferenceHolder.Reference != null))
				{
					continue;
				}
				long value2 = -1L;
				CrossTable.PrevCloneReference.TryGetValue(pdfReferenceHolder.Reference.ObjNum, out value2);
				if (value2 > 0)
				{
					PdfDictionary value3 = null;
					CurrentPage.ImportedAnnotationReference.TryGetValue(value2, out value3);
					if (value3 != null)
					{
						copyDictionary["Obj"] = new PdfReferenceHolder(value3);
					}
				}
				else
				{
					PdfDictionary value4 = null;
					CurrentPage.ImportedAnnotationReference.TryGetValue(pdfReferenceHolder.Reference.ObjNum, out value4);
					if (value4 != null)
					{
						copyDictionary["Obj"] = new PdfReferenceHolder(value4);
					}
				}
			}
			else
			{
				StructureObjectFinder(copyDictionary, item.Key.Value, item.Value);
			}
		}
	}

	private bool IsAllChildIsNull(PdfArray child)
	{
		bool flag = true;
		foreach (IPdfPrimitive item in child)
		{
			PdfDictionary pdfDictionary = PdfCrossTable.Dereference(item) as PdfDictionary;
			if (pdfDictionary != null && pdfDictionary.ContainsKey("Pg"))
			{
				if (!(PdfCrossTable.Dereference(pdfDictionary["Pg"]) is PdfDictionary key))
				{
					continue;
				}
				object value = null;
				PageCorrespondance.TryGetValue(key, out value);
				if (value != null)
				{
					return false;
				}
				if (pdfDictionary.ContainsKey("K") && PdfCrossTable.Dereference(pdfDictionary["K"]) is PdfArray child2)
				{
					flag = IsAllChildIsNull(child2);
					if (!flag)
					{
						return flag;
					}
				}
			}
			else if (pdfDictionary != null && pdfDictionary.ContainsKey("K") && PdfCrossTable.Dereference(pdfDictionary["K"]) is PdfArray child3)
			{
				flag = IsAllChildIsNull(child3);
				if (!flag)
				{
					return flag;
				}
			}
		}
		return flag;
	}

	private void StructureObjectFinder(PdfDictionary dictionary, string key, IPdfPrimitive primitive)
	{
		if (primitive == null)
		{
			return;
		}
		string text = primitive.GetType().ToString();
		if (text == null)
		{
			return;
		}
		switch (text.Length)
		{
		case 35:
			switch (text[32])
			{
			case 'e':
				_ = text == "DocGen.Pdf.Primitives.PdfStream";
				break;
			case 'i':
				if (text == "DocGen.Pdf.Primitives.PdfString" && primitive is PdfString pdfString)
				{
					dictionary[key] = new PdfString(pdfString.Value);
				}
				break;
			case 'b':
				if (text == "DocGen.Pdf.Primitives.PdfNumber" && primitive is PdfNumber value)
				{
					dictionary[key] = value;
				}
				break;
			}
			break;
		case 33:
			switch (text[30])
			{
			case 'a':
				if (text == "DocGen.Pdf.Primitives.PdfName")
				{
					PdfName pdfName = primitive as PdfName;
					if (pdfName != null)
					{
						dictionary[key] = new PdfName(pdfName.Value);
					}
				}
				break;
			case 'u':
				if (text == "DocGen.Pdf.Primitives.PdfNull")
				{
					dictionary[key] = new PdfNull();
				}
				break;
			}
			break;
		case 44:
			if (text == "DocGen.Pdf.Primitives.PdfReferenceHolder")
			{
				PdfReferenceHolder pdfReferenceHolder = primitive as PdfReferenceHolder;
				if (pdfReferenceHolder != null && pdfReferenceHolder.Object != null)
				{
					StructureObjectFinder(dictionary, key, pdfReferenceHolder.Object);
				}
			}
			break;
		case 39:
			if (text == "DocGen.Pdf.Primitives.PdfDictionary" && primitive is PdfDictionary dictionary2)
			{
				PdfDictionary pdfDictionary = new PdfDictionary();
				StructureDictionaryCopier(pdfDictionary, dictionary2, dictionary);
				if (pdfDictionary.Count > 0)
				{
					dictionary[key] = new PdfReferenceHolder(pdfDictionary);
				}
			}
			break;
		case 36:
			if (text == "DocGen.Pdf.Primitives.PdfBoolean" && primitive is PdfBoolean pdfBoolean)
			{
				dictionary[key] = new PdfBoolean(pdfBoolean.Value);
			}
			break;
		case 34:
			if (text == "DocGen.Pdf.Primitives.PdfArray")
			{
				PdfArray pdfArray = new PdfArray();
				StructureArrayCloner(pdfArray, primitive as PdfArray, dictionary);
				if (pdfArray.Count > 0)
				{
					dictionary[key] = pdfArray;
				}
			}
			break;
		}
	}

	private void StructureArrayCloner(PdfArray dictionary, PdfArray array, PdfDictionary parent)
	{
		foreach (IPdfPrimitive element in array.Elements)
		{
			AddStructureArrayElement(dictionary, element, parent);
		}
	}

	private void AddStructureArrayElement(PdfArray array, IPdfPrimitive element, PdfDictionary parent)
	{
		string text = element.GetType().ToString();
		if (text == null)
		{
			return;
		}
		switch (text.Length)
		{
		case 35:
			switch (text[32])
			{
			case 'i':
				if (text == "DocGen.Pdf.Primitives.PdfString" && element is PdfString pdfString)
				{
					array.Add(new PdfString(pdfString.Value));
				}
				break;
			case 'b':
				if (text == "DocGen.Pdf.Primitives.PdfNumber" && element is PdfNumber element2)
				{
					array.Add(element2);
				}
				break;
			case 'e':
				_ = text == "DocGen.Pdf.Primitives.PdfStream";
				break;
			}
			break;
		case 34:
			if (text == "DocGen.Pdf.Primitives.PdfArray")
			{
				PdfArray pdfArray = new PdfArray();
				StructureArrayCloner(pdfArray, element as PdfArray, parent);
				if (pdfArray.Count > 0)
				{
					array.Add(pdfArray);
				}
			}
			break;
		case 33:
			if (text == "DocGen.Pdf.Primitives.PdfName")
			{
				PdfName pdfName = element as PdfName;
				if (pdfName != null)
				{
					array.Add(new PdfName(pdfName.Value));
				}
			}
			break;
		case 36:
			if (text == "DocGen.Pdf.Primitives.PdfBoolean" && element is PdfBoolean pdfBoolean)
			{
				array.Add(new PdfBoolean(pdfBoolean.Value));
			}
			break;
		case 39:
			if (text == "DocGen.Pdf.Primitives.PdfDictionary" && element is PdfDictionary dictionary)
			{
				PdfDictionary pdfDictionary = new PdfDictionary();
				StructureDictionaryCopier(pdfDictionary, dictionary, parent);
				if (pdfDictionary.Count > 0)
				{
					array.Add(new PdfReferenceHolder(pdfDictionary));
				}
			}
			break;
		case 44:
			if (text == "DocGen.Pdf.Primitives.PdfReferenceHolder")
			{
				PdfReferenceHolder pdfReferenceHolder = element as PdfReferenceHolder;
				if (pdfReferenceHolder != null && pdfReferenceHolder.Object != null)
				{
					AddStructureArrayElement(array, pdfReferenceHolder.Object, parent);
				}
			}
			break;
		}
	}

	public void Dispose()
	{
		if (TaggedPDFObjects != null)
		{
			TaggedPDFObjects.Clear();
		}
		m_taggedObjects = null;
		structureElementCount = 0;
	}
}
