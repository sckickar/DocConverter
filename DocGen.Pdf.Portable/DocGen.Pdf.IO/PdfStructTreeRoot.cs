using System;
using System.Collections.Generic;
using DocGen.Drawing;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.IO;

internal class PdfStructTreeRoot : PdfDictionary
{
	private PdfArray m_childSTR;

	private PdfPageBase m_pdfPage;

	[ThreadStatic]
	private static int m_id;

	private RectangleF m_BBoxBounds;

	private PdfArray m_nodeTree;

	private static object s_syncObject = new object();

	[ThreadStatic]
	private static int m_pageStructId;

	private PdfArray m_structTreeChild;

	private List<int> m_orderList = new List<int>();

	private Dictionary<string, IPdfPrimitive> m_ParentTagCollection;

	private PdfDictionary m_currentTreeRootChild;

	private List<PdfDictionary> m_structParentHierarchy;

	private PdfArray m_treeRootNode;

	private Dictionary<string, IPdfPrimitive> m_structChildCollection;

	private List<string> m_structElementTags;

	private PdfArray m_treeRootChilds;

	private PdfDictionary m_currentParent;

	internal bool isNewRow;

	internal bool m_SplitTable;

	private PdfDictionary m_structElementParent;

	internal bool isNewTable;

	private bool m_isNewListItem;

	private bool m_isNewList;

	internal bool m_isSubList;

	private PdfDictionary m_subListData;

	private PdfArray m_subListCollection;

	private bool m_isOrderAdded;

	internal bool m_isNestedGridRendered;

	private PdfArray m_prevRootNode;

	internal bool m_isChildGrid;

	private int m_orderIndex;

	private bool m_hasOrder;

	private PdfArray m_prevStructParent;

	internal bool m_isImage;

	private bool m_autoTag;

	internal bool m_WordtoPDFTaggedObject;

	private List<PdfStructureElement> structureElementsCollection = new List<PdfStructureElement>();

	private PdfStructureElement m_currentStructureElement;

	private PdfDictionary m_currentElementDictionay;

	internal List<int> OrderList
	{
		get
		{
			return m_orderList;
		}
		set
		{
			m_orderList = value;
		}
	}

	internal PdfArray TreeRootNode
	{
		get
		{
			return m_treeRootNode;
		}
		set
		{
			m_treeRootNode = value;
		}
	}

	internal bool IsNewList
	{
		get
		{
			return m_isNewList;
		}
		set
		{
			m_isNewList = value;
		}
	}

	internal bool IsNewListItem
	{
		get
		{
			return m_isNewListItem;
		}
		set
		{
			m_isNewListItem = value;
		}
	}

	internal bool HasOrder
	{
		get
		{
			return m_hasOrder;
		}
		set
		{
			m_hasOrder = true;
		}
	}

	public PdfStructTreeRoot()
	{
		base["Type"] = new PdfName("StructTreeRoot");
		m_id = 0;
		m_childSTR = new PdfArray();
		m_nodeTree = new PdfArray();
		m_structTreeChild = new PdfArray();
		m_ParentTagCollection = new Dictionary<string, IPdfPrimitive>();
		m_currentTreeRootChild = new PdfDictionary();
		m_treeRootNode = new PdfArray();
		m_structChildCollection = new Dictionary<string, IPdfPrimitive>();
		m_structElementTags = new List<string>();
		m_currentParent = new PdfDictionary();
		m_treeRootChilds = new PdfArray();
		m_structElementParent = new PdfDictionary();
		m_subListData = new PdfDictionary();
		m_subListCollection = new PdfArray();
		m_prevRootNode = new PdfArray();
		m_prevStructParent = new PdfArray();
	}

	internal int Add(string structType, string altText, PdfPageBase page, RectangleF bounds)
	{
		m_pdfPage = page;
		m_BBoxBounds = bounds;
		int result = Add(structType, altText, m_BBoxBounds);
		m_pdfPage = null;
		return result;
	}

	internal int Add(string structType, string altText, RectangleF bounds)
	{
		lock (s_syncObject)
		{
			PdfDictionary pdfDictionary = new PdfDictionary();
			pdfDictionary["S"] = new PdfName(structType);
			pdfDictionary["P"] = new PdfReferenceHolder(this);
			if (m_pdfPage != null)
			{
				m_id = m_pdfPage.m_id;
			}
			pdfDictionary["K"] = new PdfNumber(m_id++);
			pdfDictionary["Lang"] = new PdfString("English");
			if (structType != "P")
			{
				pdfDictionary["Alt"] = new PdfString(altText);
			}
			if (m_pdfPage != null)
			{
				pdfDictionary["Pg"] = new PdfReferenceHolder(m_pdfPage);
			}
			PdfDictionary pdfDictionary2 = new PdfDictionary();
			_ = new float[0];
			pdfDictionary2["BBox"] = new PdfArray(new float[4] { bounds.X, bounds.Y, bounds.Width, bounds.Height });
			if (structType == "P" && bounds != RectangleF.Empty)
			{
				pdfDictionary["A"] = pdfDictionary2;
			}
			PdfReferenceHolder element = new PdfReferenceHolder(pdfDictionary);
			if (m_pdfPage != null && m_pdfPage.Dictionary.ContainsKey("StructParents"))
			{
				PdfNumber element2 = m_pdfPage.Dictionary["StructParents"] as PdfNumber;
				if (!m_nodeTree.Contains(element2))
				{
					m_pdfPage.m_childSTR = new PdfArray();
					m_pdfPage.m_childSTR.Add(element);
					m_nodeTree.Add(element2);
					m_nodeTree.Add(new PdfReferenceHolder(m_pdfPage.m_childSTR));
				}
				else
				{
					m_pdfPage.m_childSTR.Add(element);
				}
			}
			PdfDictionary pdfDictionary3 = new PdfDictionary();
			pdfDictionary3["Nums"] = m_nodeTree;
			base["ParentTree"] = new PdfReferenceHolder(pdfDictionary3);
			base["ParentTreeNextKey"] = new PdfNumber(1);
			return m_id - 1;
		}
	}

	internal void Add(PdfStructureElement structElement, PdfPageBase page, PdfDictionary annotDictionary)
	{
		if (page is PdfPage)
		{
			PdfDocument document = (page as PdfPage).Document;
			if (document != null && document.AutoTag)
			{
				m_autoTag = true;
			}
		}
		PdfDictionary pdfDictionary = new PdfDictionary();
		pdfDictionary["S"] = new PdfName(ConvertToEquivalentTag(structElement.TagType));
		List<PdfStructureElement> elements = FindRootParent(structElement);
		bool flag = false;
		bool hasStructElement = false;
		if (structElement.Parent != null)
		{
			flag = IncludeParentForStructElement(structElement, elements, out hasStructElement);
			pdfDictionary["P"] = m_currentParent["P"];
		}
		else
		{
			pdfDictionary["P"] = new PdfReferenceHolder(this);
		}
		if (!m_autoTag)
		{
			pdfDictionary["Name"] = new PdfName(structElement.m_name);
		}
		PdfDictionary pdfDictionary2 = new PdfDictionary();
		if (page != null)
		{
			pdfDictionary2["Pg"] = new PdfReferenceHolder(page);
		}
		pdfDictionary2["Type"] = new PdfName("OBJR");
		pdfDictionary2["Obj"] = new PdfReferenceHolder(annotDictionary);
		pdfDictionary["K"] = pdfDictionary2;
		if (structElement != null && page != null && !annotDictionary.ContainsKey("StructParent"))
		{
			annotDictionary["StructParent"] = new PdfNumber(m_pageStructId++);
		}
		if (page != null && !page.Dictionary.ContainsKey("Tabs"))
		{
			page.Dictionary["Tabs"] = new PdfName("S");
		}
		if (structElement.Order > 0 && !m_isOrderAdded)
		{
			m_orderList.Add(structElement.Order - 1);
			m_hasOrder = true;
		}
		else if (!m_isOrderAdded)
		{
			m_orderList.Add(-1);
		}
		PdfReferenceHolder element = new PdfReferenceHolder(pdfDictionary);
		if (page != null && annotDictionary.ContainsKey("StructParent"))
		{
			PdfNumber element2 = annotDictionary["StructParent"] as PdfNumber;
			if (!m_nodeTree.Contains(element2))
			{
				m_nodeTree.Add(element2);
				m_nodeTree.Add(element);
			}
		}
		PdfDictionary pdfDictionary3 = new PdfDictionary();
		pdfDictionary3["Nums"] = m_nodeTree;
		base["ParentTree"] = new PdfReferenceHolder(pdfDictionary3);
		base["ParentTreeNextKey"] = new PdfNumber(m_pageStructId);
		if (structElement.Parent != null)
		{
			m_treeRootNode.Add(new PdfReferenceHolder(pdfDictionary));
			if (!m_isSubList && !m_isChildGrid)
			{
				bool flag2 = false;
				if (!m_autoTag && !flag && m_structTreeChild.Count != 0)
				{
					for (int i = 0; i < m_structTreeChild.Count; i++)
					{
						PdfDictionary currentElement = PdfCrossTable.Dereference(m_structTreeChild[i]) as PdfDictionary;
						flag2 = IncludeIdenticalParent(currentElement);
						if (flag2)
						{
							break;
						}
					}
				}
				if (!flag && !flag2)
				{
					m_structTreeChild.Add(new PdfReferenceHolder(m_currentTreeRootChild));
				}
				base["K"] = m_structTreeChild;
			}
		}
		else
		{
			m_structTreeChild.Add(new PdfReferenceHolder(pdfDictionary));
		}
		base["K"] = m_structTreeChild;
	}

	internal int Add(PdfStructureElement structElement, PdfPageBase page, RectangleF bounds)
	{
		m_currentStructureElement = structElement;
		if (page is PdfPage)
		{
			PdfDocument document = (page as PdfPage).Document;
			if (document != null && document.AutoTag)
			{
				m_autoTag = true;
			}
			else
			{
				m_autoTag = false;
			}
			if (document != null && !m_autoTag)
			{
				_ = document.m_parnetTagDicitionaryCollection;
			}
			if (document != null)
			{
				m_WordtoPDFTaggedObject = document.m_WordtoPDFTagged;
			}
		}
		if (m_SplitTable)
		{
			m_autoTag = false;
		}
		PdfDictionary pdfDictionary = new PdfDictionary();
		pdfDictionary["S"] = new PdfName(ConvertToEquivalentTag(structElement.TagType));
		if (ConvertToEquivalentTag(structElement.TagType) == "TH")
		{
			if (structElement.Scope != ScopeType.None)
			{
				page.Graphics.TableSpan.SetProperty("Scope", new PdfName(structElement.Scope));
			}
			pdfDictionary.SetProperty("A", page.Graphics.TableSpan);
		}
		if (ConvertToEquivalentTag(structElement.TagType) == "TD")
		{
			pdfDictionary.SetProperty("A", page.Graphics.TableSpan);
		}
		if (ConvertToEquivalentTag(structElement.TagType) == "Figure")
		{
			_ = (page as PdfPage).Document.PageSettings.Margins;
			RectangleF actualBounds = (page as PdfPage).Section.GetActualBounds(page as PdfPage, includeMargins: true);
			if (bounds.Width <= 0f)
			{
				bounds.Width = 0.5f;
			}
			if (bounds.Height <= 0f)
			{
				bounds.Height = 0.5f;
			}
			bounds = new RectangleF(bounds.X + actualBounds.X, page.Graphics.Size.Height - (bounds.Y + actualBounds.Y + bounds.Height), bounds.Width, bounds.Height);
			PdfDictionary pdfDictionary2 = new PdfDictionary();
			pdfDictionary2.SetName("O", "Layout");
			pdfDictionary2["BBox"] = PdfArray.FromRectangle(bounds);
			pdfDictionary2.SetName("Placement", "Block");
			pdfDictionary.SetProperty("A", new PdfReferenceHolder(pdfDictionary2));
		}
		bool flag = false;
		if (!page.Dictionary.ContainsKey("StructParents"))
		{
			m_structElementTags.Clear();
			m_ParentTagCollection.Clear();
			m_structChildCollection.Clear();
		}
		List<PdfStructureElement> elements = FindRootParent(structElement);
		bool hasStructElement = false;
		if (structElement.Parent != null)
		{
			flag = IncludeParentForStructElement(structElement, elements, out hasStructElement);
			pdfDictionary["P"] = m_currentParent["P"];
		}
		else
		{
			pdfDictionary["P"] = new PdfReferenceHolder(this);
		}
		if (!m_autoTag)
		{
			pdfDictionary["Name"] = new PdfName(structElement.m_name);
		}
		if (structElement.Language != null)
		{
			pdfDictionary["Lang"] = new PdfString(structElement.Language);
		}
		else
		{
			pdfDictionary["Lang"] = new PdfString("en-US");
		}
		if (structElement.Title != null)
		{
			pdfDictionary["T"] = new PdfString(structElement.Title);
		}
		if (structElement.ActualText != null)
		{
			pdfDictionary["ActualText"] = new PdfString(structElement.ActualText);
		}
		if (page != null)
		{
			pdfDictionary["Pg"] = new PdfReferenceHolder(page);
		}
		if (structElement.AlternateText != null)
		{
			pdfDictionary["Alt"] = new PdfString(structElement.AlternateText);
		}
		else if (structElement.TagType == PdfTagType.Figure && m_isImage)
		{
			string value = Guid.NewGuid().ToString();
			pdfDictionary["Alt"] = new PdfString(value);
		}
		PdfReferenceHolder element = new PdfReferenceHolder(pdfDictionary);
		if (structElement != null && page != null && !page.Dictionary.ContainsKey("StructParents"))
		{
			page.Dictionary["StructParents"] = new PdfNumber(m_pageStructId++);
			page.m_id = 0;
		}
		pdfDictionary["Type"] = new PdfName("StructElem");
		pdfDictionary["K"] = new PdfNumber(page.m_id++);
		if (page != null && !page.Dictionary.ContainsKey("Tabs"))
		{
			page.Dictionary["Tabs"] = new PdfName("S");
		}
		if (page != null && page.Dictionary.ContainsKey("StructParents"))
		{
			PdfNumber element2 = page.Dictionary["StructParents"] as PdfNumber;
			if (!m_nodeTree.Contains(element2))
			{
				page.m_childSTR = new PdfArray();
				page.m_childSTR.Add(element);
				m_nodeTree.Add(element2);
				m_nodeTree.Add(page.m_childSTR);
			}
			else
			{
				page.m_childSTR.Add(element);
			}
		}
		PdfDictionary pdfDictionary3 = new PdfDictionary();
		pdfDictionary3["Nums"] = m_nodeTree;
		base["ParentTree"] = new PdfReferenceHolder(pdfDictionary3);
		base["ParentTreeNextKey"] = new PdfNumber(m_pageStructId);
		if (structElement.Order > 0 && !m_isOrderAdded)
		{
			m_orderList.Add(structElement.Order - 1);
			m_hasOrder = true;
		}
		else if (!m_isOrderAdded && !hasStructElement)
		{
			m_orderList.Add(-1);
		}
		m_isOrderAdded = false;
		m_currentElementDictionay = pdfDictionary;
		if (structElement.Parent != null)
		{
			m_treeRootNode.Add(new PdfReferenceHolder(pdfDictionary));
			if ((!m_isSubList && !m_isChildGrid) || m_SplitTable)
			{
				bool flag2 = false;
				if (!m_autoTag && !flag && m_structTreeChild.Count != 0)
				{
					for (int i = 0; i < m_structTreeChild.Count; i++)
					{
						PdfDictionary currentElement = PdfCrossTable.Dereference(m_structTreeChild[i]) as PdfDictionary;
						flag2 = IncludeIdenticalParent(currentElement);
						if (flag2)
						{
							break;
						}
					}
				}
				if (!flag && !flag2)
				{
					m_structTreeChild.Add(new PdfReferenceHolder(m_currentTreeRootChild));
				}
			}
			if (m_SplitTable && isNewTable)
			{
				isNewTable = false;
				isNewRow = false;
				m_autoTag = true;
			}
		}
		else if (m_WordtoPDFTaggedObject && structElement.m_isActiveSetTag && structureElementsCollection.Contains(structElement))
		{
			GroupMapping(pdfDictionary);
		}
		else
		{
			m_structTreeChild.Add(new PdfReferenceHolder(pdfDictionary));
		}
		base["K"] = m_structTreeChild;
		if (m_WordtoPDFTaggedObject && structElement.m_isActiveSetTag && !structureElementsCollection.Contains(structElement))
		{
			structureElementsCollection.Add(structElement);
		}
		m_currentStructureElement = null;
		m_currentElementDictionay = null;
		return page.m_id - 1;
	}

	private void GroupMapping(PdfDictionary pdfStructElement)
	{
		for (int i = 0; i < m_structTreeChild.Count; i++)
		{
			PdfDictionary pdfDictionary = PdfCrossTable.Dereference(m_structTreeChild[i]) as PdfDictionary;
			if (pdfDictionary.ContainsKey("Name") && pdfStructElement.ContainsKey("Name"))
			{
				PdfName obj = pdfStructElement["Name"] as PdfName;
				PdfName pdfName = pdfDictionary["Name"] as PdfName;
				if (obj == pdfName)
				{
					MappingGroupKids(pdfDictionary, pdfStructElement);
				}
			}
		}
	}

	private void MappingGroupKids(PdfDictionary currentElement, PdfDictionary pdfStructElement)
	{
		PdfArray pdfArray = null;
		if (currentElement.ContainsKey("K"))
		{
			pdfArray = PdfCrossTable.Dereference(currentElement["K"]) as PdfArray;
		}
		if (pdfArray == null)
		{
			pdfArray = new PdfArray();
		}
		if (pdfArray.Count == 0)
		{
			PdfNumber element = PdfCrossTable.Dereference(currentElement["K"]) as PdfNumber;
			pdfArray.Add(element);
		}
		PdfNumber element2 = PdfCrossTable.Dereference(pdfStructElement["K"]) as PdfNumber;
		pdfArray.Add(element2);
		currentElement["K"] = pdfArray;
	}

	private bool IncludeIdenticalParent(PdfDictionary currentElement)
	{
		bool flag = false;
		if (currentElement.ContainsKey("Name") && m_currentTreeRootChild.ContainsKey("Name"))
		{
			PdfName obj = m_currentTreeRootChild["Name"] as PdfName;
			PdfName pdfName = currentElement["Name"] as PdfName;
			if (obj == pdfName)
			{
				PdfArray pdfArray = PdfCrossTable.Dereference(currentElement["K"]) as PdfArray;
				PdfDictionary pdfDictionary = PdfCrossTable.Dereference(currentElement["K"]) as PdfDictionary;
				PdfNumber pdfNumber = PdfCrossTable.Dereference(currentElement["K"]) as PdfNumber;
				if (!(PdfCrossTable.Dereference(m_currentTreeRootChild["K"]) is PdfArray pdfArray2))
				{
					if (PdfCrossTable.Dereference(m_currentTreeRootChild["K"]) is PdfDictionary pdfDictionary2)
					{
						if (pdfArray != null)
						{
							for (int i = 0; i < pdfArray.Count; i++)
							{
								if (PdfCrossTable.Dereference(pdfArray[i]) is PdfDictionary pdfDictionary3)
								{
									PdfName obj2 = pdfDictionary2["Name"] as PdfName;
									PdfName pdfName2 = pdfDictionary3["Name"] as PdfName;
									if (obj2 == pdfName2)
									{
										m_currentTreeRootChild = pdfDictionary2;
										flag = IncludeIdenticalParent(pdfDictionary3);
									}
									if (flag)
									{
										return flag;
									}
								}
							}
							pdfDictionary2["P"] = new PdfReferenceHolder(currentElement);
							pdfArray.Add(new PdfReferenceHolder(pdfDictionary2));
							flag = true;
						}
						else
						{
							PdfArray pdfArray3 = new PdfArray();
							if (pdfDictionary != null)
							{
								PdfName obj3 = pdfDictionary2["Name"] as PdfName;
								PdfName pdfName3 = pdfDictionary["Name"] as PdfName;
								if (obj3 == pdfName3)
								{
									m_currentTreeRootChild = pdfDictionary2;
									flag = IncludeIdenticalParent(pdfDictionary);
								}
								if (flag)
								{
									return flag;
								}
								pdfArray3.Add(new PdfReferenceHolder(pdfDictionary));
							}
							if (pdfNumber != null)
							{
								pdfArray3.Add(new PdfReferenceHolder(pdfNumber));
							}
							pdfDictionary2["P"] = new PdfReferenceHolder(currentElement);
							pdfArray3.Add(new PdfReferenceHolder(pdfDictionary2));
							currentElement["K"] = new PdfReferenceHolder(pdfArray3);
							flag = true;
						}
					}
				}
				else if (pdfArray2 != null && PdfCrossTable.Dereference(pdfArray2[0]) is PdfDictionary pdfDictionary4)
				{
					if (pdfArray != null)
					{
						for (int j = 0; j < pdfArray.Count; j++)
						{
							if (!(PdfCrossTable.Dereference(pdfArray[j]) is PdfDictionary pdfDictionary5))
							{
								continue;
							}
							PdfName pdfName4 = pdfDictionary4["Name"] as PdfName;
							PdfName pdfName5 = pdfDictionary5["Name"] as PdfName;
							if (pdfName4 == pdfName5)
							{
								m_currentTreeRootChild = pdfDictionary4;
								if (m_WordtoPDFTaggedObject && m_currentStructureElement.m_isActiveSetTag && structureElementsCollection.Contains(m_currentStructureElement) && pdfName4.Value == m_currentStructureElement.m_name)
								{
									MappingGroupKids(pdfDictionary5, m_currentElementDictionay);
									flag = true;
								}
								else
								{
									flag = IncludeIdenticalParent(pdfDictionary5);
								}
							}
							if (flag)
							{
								return flag;
							}
						}
						pdfDictionary4["P"] = new PdfReferenceHolder(currentElement);
						pdfArray.Add(new PdfReferenceHolder(pdfDictionary4));
						flag = true;
					}
					else
					{
						PdfArray pdfArray4 = new PdfArray();
						if (pdfDictionary != null)
						{
							PdfName obj4 = pdfDictionary4["Name"] as PdfName;
							PdfName pdfName6 = pdfDictionary["Name"] as PdfName;
							if (obj4 == pdfName6)
							{
								m_currentTreeRootChild = pdfDictionary4;
								flag = IncludeIdenticalParent(pdfDictionary);
							}
							if (flag)
							{
								return flag;
							}
							pdfArray4.Add(new PdfReferenceHolder(pdfDictionary));
						}
						if (pdfNumber != null)
						{
							pdfArray4.Add(new PdfReferenceHolder(pdfNumber));
						}
						pdfDictionary4["P"] = new PdfReferenceHolder(currentElement);
						pdfArray4.Add(new PdfReferenceHolder(pdfDictionary4));
						currentElement["K"] = new PdfReferenceHolder(pdfArray4);
						flag = true;
					}
				}
				return flag;
			}
			PdfArray pdfArray5 = PdfCrossTable.Dereference(currentElement["K"]) as PdfArray;
			PdfDictionary pdfDictionary6 = PdfCrossTable.Dereference(currentElement["K"]) as PdfDictionary;
			if (pdfArray5 != null)
			{
				for (int k = 0; k < pdfArray5.Count; k++)
				{
					if (PdfCrossTable.Dereference(pdfArray5[k]) is PdfDictionary currentElement2)
					{
						flag = IncludeIdenticalParent(currentElement2);
						if (flag)
						{
							return flag;
						}
					}
				}
			}
			else if (pdfDictionary6 != null)
			{
				return IncludeIdenticalParent(pdfDictionary6);
			}
		}
		return flag;
	}

	private bool IncludeParentForGroupElements(List<PdfStructureElement> elements)
	{
		m_structParentHierarchy = new List<PdfDictionary>();
		m_currentTreeRootChild = new PdfDictionary();
		if (CheckTableChild(elements) || CheckSubList(elements))
		{
			return true;
		}
		return AddEntriesForStructElement(elements);
	}

	private bool AddEntriesForStructElement(List<PdfStructureElement> elements)
	{
		if (isNewTable || IsNewList)
		{
			m_structElementParent = new PdfDictionary();
			m_treeRootChilds = new PdfArray();
			m_structElementParent["S"] = new PdfName(ConvertToEquivalentTag(elements[0].TagType));
			m_structElementParent["P"] = new PdfReferenceHolder(this);
			if (!m_isOrderAdded && elements[0].Order >= 0)
			{
				OrderList.Add(elements[0].Order - 1);
			}
		}
		int index = 1;
		m_structParentHierarchy.Add(m_structElementParent);
		if (!m_autoTag)
		{
			m_structElementParent["Name"] = new PdfName(elements[0].m_name);
		}
		bool result = false;
		if (isNewTable || IsNewList)
		{
			m_treeRootNode = new PdfArray();
			SetParentForStructHierarchy(elements, m_structElementParent, index);
			SetChildForStructElement(m_structParentHierarchy, m_treeRootChilds);
			m_currentTreeRootChild = m_structParentHierarchy[0];
			m_currentParent = m_structParentHierarchy[m_structParentHierarchy.Count - 1];
			if (isNewTable)
			{
				m_prevRootNode = m_treeRootNode;
			}
			isNewTable = false;
			isNewRow = false;
			IsNewList = false;
			IsNewListItem = false;
		}
		else
		{
			if (isNewRow || IsNewListItem)
			{
				m_treeRootNode = new PdfArray();
				SetParentForStructHierarchy(elements, m_structElementParent, index);
				SetChildForStructElement(m_structParentHierarchy, m_treeRootChilds);
				m_currentParent = m_structParentHierarchy[m_structParentHierarchy.Count - 1];
				isNewRow = false;
				IsNewListItem = false;
			}
			m_currentTreeRootChild = m_structParentHierarchy[0];
			result = true;
		}
		return result;
	}

	private bool CheckTableChild(List<PdfStructureElement> elements)
	{
		if (m_isChildGrid)
		{
			if (isNewTable)
			{
				m_prevRootNode = m_treeRootNode;
			}
			SetSubEntries(elements, m_currentParent);
			return true;
		}
		if (m_isNestedGridRendered)
		{
			m_treeRootNode = m_prevRootNode;
			m_isNestedGridRendered = false;
		}
		return false;
	}

	private bool IncludeParentForStructElement(PdfStructureElement structElement, List<PdfStructureElement> elements, out bool hasStructElement)
	{
		if (m_autoTag && m_WordtoPDFTaggedObject && (AddTags(elements).Contains("Table") || AddTags(elements).Contains("List")))
		{
			m_autoTag = false;
		}
		if (m_autoTag && (AddTags(elements).Contains("Table") || AddTags(elements).Contains("List")))
		{
			hasStructElement = true;
			return IncludeParentForGroupElements(elements);
		}
		m_structParentHierarchy = new List<PdfDictionary>();
		m_currentTreeRootChild = new PdfDictionary();
		PdfDictionary pdfDictionary = new PdfDictionary();
		hasStructElement = false;
		pdfDictionary["S"] = new PdfName(ConvertToEquivalentTag(elements[0].TagType));
		pdfDictionary["P"] = new PdfReferenceHolder(this);
		if (!m_autoTag)
		{
			pdfDictionary["Name"] = new PdfName(elements[0].m_name);
		}
		int index = 1;
		m_structParentHierarchy.Add(pdfDictionary);
		bool result = false;
		string text = AddTags(elements);
		if (IsIdenticalStructureElement(text) && m_autoTag)
		{
			result = true;
			m_currentTreeRootChild = m_ParentTagCollection[text] as PdfDictionary;
			m_treeRootNode = m_structChildCollection[text] as PdfArray;
		}
		else
		{
			m_structElementTags.Add(AddTags(elements));
			m_treeRootNode = new PdfArray();
			SetParentForStructHierarchy(elements, pdfDictionary, index);
			SetTreeRootNodes(m_structParentHierarchy);
			m_currentTreeRootChild = m_structParentHierarchy[0];
			if (m_autoTag)
			{
				m_ParentTagCollection.Add(text, m_structParentHierarchy[0]);
				m_structChildCollection.Add(text, m_treeRootNode);
			}
			m_currentParent = m_structParentHierarchy[m_structParentHierarchy.Count - 1];
		}
		return result;
	}

	private bool CheckSubList(List<PdfStructureElement> elements)
	{
		if (m_isSubList)
		{
			SetSubEntries(elements, m_currentParent);
			return true;
		}
		return false;
	}

	private void SetSubEntries(List<PdfStructureElement> structElements, PdfDictionary currentParent)
	{
		if (IsNewList || isNewTable)
		{
			m_subListData = new PdfDictionary();
			m_treeRootNode.Add(new PdfReferenceHolder(m_subListData));
			m_subListCollection = new PdfArray();
			m_subListData["S"] = new PdfName(ConvertToEquivalentTag(structElements[0].TagType));
			m_subListData["P"] = new PdfReferenceHolder(currentParent["P"]);
		}
		int index = 1;
		m_structParentHierarchy.Add(m_subListData);
		if (IsNewList || isNewTable)
		{
			m_treeRootNode = new PdfArray();
			SetParentForStructHierarchy(structElements, m_subListData, index);
			SetChildForStructElement(m_structParentHierarchy, m_subListCollection);
			m_currentTreeRootChild = m_structParentHierarchy[0];
			m_currentParent = m_structParentHierarchy[m_structParentHierarchy.Count - 1];
			IsNewList = false;
			IsNewListItem = false;
			isNewTable = false;
			isNewRow = false;
		}
		else
		{
			if (IsNewListItem || isNewRow)
			{
				m_treeRootNode = new PdfArray();
				SetParentForStructHierarchy(structElements, m_subListData, index);
				SetChildForStructElement(m_structParentHierarchy, m_subListCollection);
				m_currentParent = m_structParentHierarchy[m_structParentHierarchy.Count - 1];
				IsNewListItem = false;
				isNewRow = false;
			}
			m_currentTreeRootChild = m_structParentHierarchy[0];
		}
	}

	private string AddTags(List<PdfStructureElement> elements)
	{
		string text = "";
		foreach (PdfStructureElement element in elements)
		{
			text += element.TagType;
		}
		return text.TrimEnd();
	}

	private bool IsIdenticalStructureElement(string tag)
	{
		bool result = false;
		if (m_structElementTags.Contains(tag))
		{
			result = true;
		}
		return result;
	}

	private void SetParentForStructHierarchy(List<PdfStructureElement> structElements, PdfDictionary structElementParent, int index)
	{
		if (index < structElements.Count)
		{
			PdfDictionary pdfDictionary = new PdfDictionary();
			pdfDictionary["S"] = new PdfName(ConvertToEquivalentTag(structElements[index].TagType));
			if (!m_autoTag)
			{
				pdfDictionary["Name"] = new PdfName(structElements[index].m_name);
			}
			if (((structElementParent["S"] as PdfName).Value == "Table" || (structElementParent["S"] as PdfName).Value == "L") && m_prevStructParent.Count > 0)
			{
				pdfDictionary["P"] = m_prevStructParent[0];
			}
			else
			{
				pdfDictionary["P"] = new PdfReferenceHolder(structElementParent);
			}
			if (structElements[index].AttributeDictionary != null)
			{
				pdfDictionary["A"] = structElements[index].AttributeDictionary;
			}
			m_structParentHierarchy.Add(pdfDictionary);
			SetParentForStructHierarchy(structElements, pdfDictionary, ++index);
		}
	}

	private void SetTreeRootNodes(List<PdfDictionary> treeRootItems)
	{
		PdfDictionary pdfDictionary = new PdfDictionary();
		for (int i = 0; i < treeRootItems.Count; i++)
		{
			pdfDictionary = treeRootItems[i];
			if (i + 1 < treeRootItems.Count - 1)
			{
				pdfDictionary["K"] = new PdfReferenceHolder(treeRootItems[i + 1]);
			}
			else if (i + 1 == treeRootItems.Count - 1)
			{
				pdfDictionary["K"] = new PdfReferenceHolder(m_treeRootNode);
			}
			treeRootItems.RemoveAt(i);
			treeRootItems.Insert(i, pdfDictionary);
		}
	}

	private void SetChildForStructElement(List<PdfDictionary> treeRootItems, PdfArray m_childs)
	{
		PdfDictionary pdfDictionary = new PdfDictionary();
		for (int i = 0; i < treeRootItems.Count; i++)
		{
			pdfDictionary = treeRootItems[i];
			if ((pdfDictionary["S"] as PdfName).Value == "Table" || (pdfDictionary["S"] as PdfName).Value == "L")
			{
				m_childs.Add(new PdfReferenceHolder(treeRootItems[i + 1]));
				pdfDictionary["K"] = m_childs;
			}
			else if ((pdfDictionary["S"] as PdfName).Value == "TR" || (pdfDictionary["S"] as PdfName).Value == "LI")
			{
				pdfDictionary["K"] = new PdfReferenceHolder(m_treeRootNode);
			}
			else if (i < treeRootItems.Count - 1 && !pdfDictionary.ContainsKey("K"))
			{
				pdfDictionary["K"] = new PdfReferenceHolder(treeRootItems[i + 1]);
				if (isNewTable || IsNewList)
				{
					m_prevStructParent.Clear();
					m_prevStructParent.Insert(0, pdfDictionary["K"]);
				}
			}
			treeRootItems.RemoveAt(i);
			treeRootItems.Insert(i, pdfDictionary);
		}
	}

	internal void Dispose()
	{
		m_currentParent.Clear();
		m_currentTreeRootChild.Clear();
		m_orderList.Clear();
		m_structChildCollection.Clear();
		m_structElementParent.Clear();
		m_structElementTags.Clear();
		m_ParentTagCollection.Clear();
		m_subListCollection.Clear();
		m_subListData.Clear();
		m_treeRootChilds.Clear();
		m_treeRootNode.Clear();
	}

	private List<PdfStructureElement> FindRootParent(PdfStructureElement structElement)
	{
		List<PdfStructureElement> list = new List<PdfStructureElement>();
		list.Insert(0, structElement);
		AddOrderFromStructure(structElement);
		while (structElement.Parent != null)
		{
			list.Insert(0, structElement.Parent);
			AddOrderFromStructure(structElement.Parent);
			structElement = structElement.Parent;
		}
		return list;
	}

	private void AddOrderFromStructure(PdfStructureElement structElement)
	{
		if (structElement.Order > 0 && !m_isOrderAdded && !m_orderList.Contains(structElement.Order - 1))
		{
			OrderList.Add(structElement.Order - 1);
			m_isOrderAdded = true;
			m_hasOrder = true;
		}
	}

	internal string ConvertToEquivalentTag(PdfTagType tag)
	{
		string result = "";
		switch (tag)
		{
		case PdfTagType.Paragraph:
			result = "P";
			break;
		case PdfTagType.Figure:
			result = "Figure";
			break;
		case PdfTagType.Article:
			result = "Art";
			break;
		case PdfTagType.Annotation:
			result = "Annot";
			break;
		case PdfTagType.BibliographyEntry:
			result = "Bibentry";
			break;
		case PdfTagType.BlockQuotation:
			result = "BlockQuote";
			break;
		case PdfTagType.Caption:
			result = PdfTagType.Caption.ToString();
			break;
		case PdfTagType.Code:
			result = PdfTagType.Code.ToString();
			break;
		case PdfTagType.Division:
			result = "Div";
			break;
		case PdfTagType.Document:
			result = "Document";
			break;
		case PdfTagType.Form:
			result = "Form";
			break;
		case PdfTagType.Formula:
			result = "Formula";
			break;
		case PdfTagType.Index:
			result = "Index";
			break;
		case PdfTagType.Heading:
			result = "H";
			break;
		case PdfTagType.HeadingLevel1:
			result = "H1";
			break;
		case PdfTagType.HeadingLevel2:
			result = "H2";
			break;
		case PdfTagType.HeadingLevel3:
			result = "H3";
			break;
		case PdfTagType.HeadingLevel4:
			result = "H4";
			break;
		case PdfTagType.HeadingLevel5:
			result = "H5";
			break;
		case PdfTagType.HeadingLevel6:
			result = "H6";
			break;
		case PdfTagType.Label:
			result = "Lbl";
			break;
		case PdfTagType.Link:
			result = "Link";
			break;
		case PdfTagType.List:
			result = "L";
			break;
		case PdfTagType.ListItem:
			result = "LI";
			break;
		case PdfTagType.ListBody:
			result = "LBody";
			break;
		case PdfTagType.Note:
			result = "Note";
			break;
		case PdfTagType.Part:
			result = "Part";
			break;
		case PdfTagType.Quotation:
			result = "Quote";
			break;
		case PdfTagType.Reference:
			result = "Reference";
			break;
		case PdfTagType.Section:
			result = "Sect";
			break;
		case PdfTagType.Span:
			result = "Span";
			break;
		case PdfTagType.Table:
			result = "Table";
			break;
		case PdfTagType.TableDataCell:
			result = "TD";
			break;
		case PdfTagType.TableHeader:
			result = "TH";
			break;
		case PdfTagType.TableOfContent:
			result = "TOC";
			break;
		case PdfTagType.TableOfContentItem:
			result = "TOCI";
			break;
		case PdfTagType.TableRow:
			result = "TR";
			break;
		case PdfTagType.TableHeaderRowGroup:
			result = "THead";
			break;
		case PdfTagType.TableBodyRowGroup:
			result = "TBody";
			break;
		}
		return result;
	}

	internal void ReOrderList(int maxLimit, List<int> orderList)
	{
		for (int i = 0; i < orderList.Count; i++)
		{
			if (orderList[i] < maxLimit)
			{
				continue;
			}
			int num = orderList[i];
			int num2 = 0;
			List<int> list = new List<int>(orderList);
			do
			{
				int num3 = 0;
				for (int j = 0; j < list.Count; j++)
				{
					if (list[j] >= 0 && list[j] < num)
					{
						num3 = j;
					}
				}
				if (num3 == 0)
				{
					num3 = list.IndexOf(num);
				}
				list.RemoveAt(num3);
				list.Insert(num3, -1);
				orderList[num3] = num2++;
				num = FindLowest(list, maxLimit);
			}
			while (num >= 0 && orderList.Count > 0);
			break;
		}
	}

	private int FindLowest(List<int> orderList, int maxLimit)
	{
		int result = -1;
		for (int i = 0; i < orderList.Count; i++)
		{
			if (orderList[i] >= 0 && orderList[i] >= maxLimit)
			{
				result = orderList[i];
			}
		}
		return result;
	}

	internal void ReArrange(PdfArray childElements, List<int> orderList)
	{
		PdfReferenceHolder[] array = new PdfReferenceHolder[childElements.Count];
		for (int i = 0; i < childElements.Count; i++)
		{
			array[i] = childElements.Elements[i] as PdfReferenceHolder;
		}
		if (childElements.Count == orderList.Count && !orderList.Contains(-1))
		{
			for (int j = 0; j < orderList.Count; j++)
			{
				if (orderList[j] > childElements.Count)
				{
					throw new ArgumentOutOfRangeException("Order value should not exceed the elements count.");
				}
				if (orderList[j] < childElements.Count)
				{
					childElements.Elements[orderList[j]] = array[j];
				}
			}
			return;
		}
		for (int k = 0; k < orderList.Count; k++)
		{
			if (orderList[k] >= 0)
			{
				PdfReferenceHolder pdfReferenceHolder = array[k];
				int index = childElements.IndexOf(pdfReferenceHolder);
				PdfReferenceHolder item = childElements.Elements[orderList[k]] as PdfReferenceHolder;
				int index2 = childElements.Elements.IndexOf(item);
				childElements.Elements.RemoveAt(index);
				childElements.Elements.Insert(index, item);
				childElements.Elements.RemoveAt(index2);
				childElements.Elements.Insert(index2, pdfReferenceHolder);
			}
		}
	}

	private List<int> ArrangeChildWithOrder(int elementCount)
	{
		List<int> list = new List<int>(OrderList.GetRange(m_orderIndex, elementCount));
		m_orderIndex += list.Count;
		return list;
	}

	internal void GetChildElements(PdfArray treeRootChild)
	{
		for (int i = 0; i < treeRootChild.Count; i++)
		{
			PdfDictionary pdfDictionary = (treeRootChild[i] as PdfReferenceHolder).Object as PdfDictionary;
			PdfName pdfName = pdfDictionary["S"] as PdfName;
			if (pdfName.Value != "Table" && pdfName.Value != "L" && pdfName.Value != "Form")
			{
				while (pdfDictionary.ContainsKey("K"))
				{
					if (!(PdfCrossTable.Dereference(pdfDictionary["K"]) is PdfDictionary pdfDictionary2))
					{
						if (PdfCrossTable.Dereference(pdfDictionary["K"]) is PdfArray pdfArray)
						{
							List<int> orderList = ArrangeChildWithOrder(pdfArray.Count);
							ReOrderList(pdfArray.Count, orderList);
							ReArrange(pdfArray, orderList);
						}
						break;
					}
					pdfDictionary = pdfDictionary2;
				}
			}
			else if (!m_WordtoPDFTaggedObject || !(pdfName.Value == "Table"))
			{
				List<int> list = ArrangeChildWithOrder(1);
				int num = i;
				for (int j = 0; j < num; j++)
				{
					list.Insert(j, -1);
				}
				ReArrange(treeRootChild, list);
			}
		}
	}
}
