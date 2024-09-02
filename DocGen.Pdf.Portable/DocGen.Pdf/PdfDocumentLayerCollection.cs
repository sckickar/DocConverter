using System;
using System.Collections.Generic;
using System.IO;
using DocGen.Pdf.IO;
using DocGen.Pdf.Parsing;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf;

public class PdfDocumentLayerCollection : PdfCollection
{
	internal bool m_sublayer;

	internal PdfDictionary m_OptionalContent = new PdfDictionary();

	private bool m_isLayerContainsResource;

	private static object s_syncLockLayer = new object();

	private PdfDocumentBase document;

	private PdfLayer m_parent;

	private Dictionary<PdfReferenceHolder, PdfLayer> layerDictionary = new Dictionary<PdfReferenceHolder, PdfLayer>();

	private int m_bdcCount;

	public PdfLayer this[int index]
	{
		get
		{
			return base.List[index] as PdfLayer;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("layer");
			}
			PdfLayer pdfLayer = this[index];
			if (pdfLayer != null)
			{
				RemoveLayer(pdfLayer, isRemoveContent: true);
			}
			base.List[index] = value;
			InsertLayer(index, value);
		}
	}

	private bool IsSkip
	{
		get
		{
			if (m_bdcCount > 0)
			{
				return true;
			}
			return false;
		}
	}

	internal PdfDocumentLayerCollection()
	{
	}

	internal PdfDocumentLayerCollection(PdfDocumentBase document, PdfLayer layer)
	{
		this.document = document;
		m_parent = layer;
	}

	internal PdfDocumentLayerCollection(PdfDocumentBase document)
	{
		if (document == null)
		{
			throw new ArgumentNullException("document not to be null");
		}
		this.document = document;
		if (!(document is PdfLoadedDocument))
		{
			return;
		}
		PdfLoadedDocument pdfLoadedDocument = document as PdfLoadedDocument;
		PdfDictionary pdfDictionary = null;
		PdfReferenceHolder pdfReferenceHolder = null;
		if (!pdfLoadedDocument.Catalog.ContainsKey("OCProperties") || !(PdfCrossTable.Dereference(pdfLoadedDocument.Catalog["OCProperties"]) is PdfDictionary pdfDictionary2))
		{
			return;
		}
		if (pdfDictionary2.ContainsKey("OCGs") && PdfCrossTable.Dereference(pdfDictionary2["OCGs"]) is PdfArray)
		{
			PdfArray pdfArray = PdfCrossTable.Dereference(pdfDictionary2["OCGs"]) as PdfArray;
			for (int i = 0; i < pdfArray.Count; i++)
			{
				if (!(pdfArray[i] is PdfReferenceHolder))
				{
					continue;
				}
				pdfReferenceHolder = pdfArray[i] as PdfReferenceHolder;
				pdfDictionary = pdfReferenceHolder.Object as PdfDictionary;
				PdfLayer pdfLayer = new PdfLayer();
				if (pdfDictionary != null && pdfDictionary.ContainsKey("Name"))
				{
					PdfString pdfString = PdfCrossTable.Dereference(pdfDictionary["Name"]) as PdfString;
					pdfLayer.Name = pdfString.Value;
					pdfLayer.Dictionary = pdfDictionary;
					pdfLayer.ReferenceHolder = pdfReferenceHolder;
					IPdfPrimitive pdfPrimitive = PdfCrossTable.Dereference(pdfDictionary["LayerID"]);
					if (pdfPrimitive != null)
					{
						pdfLayer.LayerId = pdfPrimitive.ToString();
					}
					if (PdfCrossTable.Dereference(pdfDictionary["Usage"]) is PdfDictionary pdfDictionary3)
					{
						PdfDictionary pdfDictionary4 = PdfCrossTable.Dereference(pdfDictionary3["Print"]) as PdfDictionary;
						PdfDictionary pdfDictionary5 = PdfCrossTable.Dereference(pdfDictionary3["View"]) as PdfDictionary;
						if (pdfDictionary4 != null)
						{
							pdfLayer.m_printOption = pdfDictionary4;
							foreach (KeyValuePair<PdfName, IPdfPrimitive> item in pdfDictionary4.Items)
							{
								if (item.Key.Value.Equals("PrintState"))
								{
									if ((PdfCrossTable.Dereference(item.Value) as PdfName).Value.Equals("ON"))
									{
										pdfLayer.PrintState = PdfPrintState.AlwaysPrint;
									}
									else
									{
										pdfLayer.PrintState = PdfPrintState.NeverPrint;
									}
									break;
								}
							}
						}
						if (pdfDictionary5 != null)
						{
							foreach (KeyValuePair<PdfName, IPdfPrimitive> item2 in pdfDictionary5.Items)
							{
								if (item2.Key.Value.Equals("ViewState") && (PdfCrossTable.Dereference(item2.Value) as PdfName).Value.Equals("OFF"))
								{
									pdfLayer.m_visible = false;
									break;
								}
							}
						}
					}
				}
				pdfLayer.Document = document;
				pdfLayer.Layer = pdfLayer;
				layerDictionary[pdfReferenceHolder] = pdfLayer;
				base.List.Add(pdfLayer);
			}
		}
		CheckLayerLock(pdfDictionary2);
		CheckLayerVisible(pdfDictionary2);
		CheckParentLayer(pdfDictionary2);
		CreateLayerHierarchical(pdfDictionary2);
	}

	public PdfLayer Add(string name, bool visible)
	{
		lock (s_syncLockLayer)
		{
			PdfLayer pdfLayer = new PdfLayer();
			pdfLayer.Name = name;
			pdfLayer.Document = document;
			pdfLayer.Visible = visible;
			pdfLayer.LayerId = "OCG_" + Guid.NewGuid();
			pdfLayer.m_sublayerposition = 0;
			pdfLayer.Layer = pdfLayer;
			Add(pdfLayer);
			return pdfLayer;
		}
	}

	public PdfLayer Add(string name)
	{
		lock (s_syncLockLayer)
		{
			PdfLayer pdfLayer = new PdfLayer();
			pdfLayer.Document = document;
			pdfLayer.Name = name;
			pdfLayer.LayerId = "OCG_" + Guid.NewGuid();
			pdfLayer.m_sublayerposition = 0;
			pdfLayer.Layer = pdfLayer;
			Add(pdfLayer);
			return pdfLayer;
		}
	}

	private int Add(PdfLayer layer)
	{
		lock (s_syncLockLayer)
		{
			if (layer == null)
			{
				throw new ArgumentNullException("layer");
			}
			base.List.Add(layer);
			int result = base.List.Count - 1;
			if (document is PdfDocument)
			{
				CreateLayer(layer);
			}
			else
			{
				CreateLayerLoadedDocument(layer);
			}
			layer.Layer = layer;
			return result;
		}
	}

	private int AddNestedLayer(PdfLayer layer)
	{
		lock (s_syncLockLayer)
		{
			if (layer == null)
			{
				throw new ArgumentNullException("layer");
			}
			base.List.Add(layer);
			int result = base.List.Count - 1;
			layer.Layer = layer;
			return result;
		}
	}

	internal void CreateLayer(PdfLayer layer)
	{
		PdfDictionary pdfDictionary = new PdfDictionary();
		IPdfPrimitive value = CreateOptionalContentDictionary(layer);
		pdfDictionary["OCGs"] = value;
		pdfDictionary["D"] = CreateOptionalContentViews(layer);
		document.Catalog.SetProperty("OCProperties", pdfDictionary);
	}

	private PdfDictionary setPrintOption(PdfLayer layer)
	{
		PdfDictionary pdfDictionary = new PdfDictionary();
		layer.m_printOption = new PdfDictionary();
		layer.m_printOption.SetProperty("Subtype", new PdfName("Print"));
		if (layer.PrintState.Equals(PdfPrintState.NeverPrint))
		{
			layer.m_printOption.SetProperty("PrintState", new PdfName("OFF"));
		}
		else if (layer.PrintState.Equals(PdfPrintState.AlwaysPrint))
		{
			layer.m_printOption.SetProperty("PrintState", new PdfName("ON"));
		}
		pdfDictionary.SetProperty("Print", new PdfReferenceHolder(layer.m_printOption));
		return pdfDictionary;
	}

	internal void CreateLayerLoadedDocument(PdfLayer layer)
	{
		PdfDictionary pdfDictionary = new PdfDictionary();
		IPdfPrimitive pdfPrimitive = CreateOptionalContentDictionary(layer, isLoadedDocument: true);
		bool flag = false;
		if (document != null && document.Catalog != null && document.Catalog.ContainsKey("OCProperties") && m_isLayerContainsResource && PdfCrossTable.Dereference(document.Catalog["OCProperties"]) is PdfDictionary pdfDictionary2 && pdfDictionary2.ContainsKey("OCGs"))
		{
			PdfArray pdfArray = PdfCrossTable.Dereference(pdfDictionary2["OCGs"]) as PdfArray;
			PdfArray pdfArray2 = pdfPrimitive as PdfArray;
			if (pdfArray != null && pdfArray2 != null)
			{
				flag = true;
				foreach (IPdfPrimitive item in pdfArray2)
				{
					if (!pdfArray.Contains(item))
					{
						pdfArray.Add(item);
					}
				}
			}
			if (pdfDictionary2.ContainsKey("D") && PdfCrossTable.Dereference(pdfDictionary2["D"]) is PdfDictionary pdfDictionary3)
			{
				PdfArray pdfArray3 = null;
				PdfArray pdfArray4 = null;
				PdfArray pdfArray5 = null;
				if (!pdfDictionary3.ContainsKey("Order"))
				{
					pdfDictionary3["Order"] = document.m_order;
				}
				if (pdfDictionary3.ContainsKey("OFF"))
				{
					pdfArray4 = PdfCrossTable.Dereference(pdfDictionary3["OFF"]) as PdfArray;
				}
				if (pdfDictionary3.ContainsKey("ON"))
				{
					pdfArray3 = PdfCrossTable.Dereference(pdfDictionary3["ON"]) as PdfArray;
				}
				if (pdfDictionary3.ContainsKey("AS"))
				{
					pdfArray5 = PdfCrossTable.Dereference(pdfDictionary3["AS"]) as PdfArray;
				}
				if (pdfArray5 != null)
				{
					for (int i = 0; i < pdfArray5.Count; i++)
					{
						if (pdfArray5[i] is PdfReferenceHolder || pdfArray5[i] is PdfDictionary)
						{
							PdfDictionary pdfDictionary4 = null;
							pdfDictionary4 = ((!(pdfArray5[i] is PdfReferenceHolder)) ? (pdfArray5[i] as PdfDictionary) : ((pdfArray5[i] as PdfReferenceHolder).Object as PdfDictionary));
							if (pdfDictionary4 != null && pdfDictionary4["OCGs"] is PdfArray pdfArray6 && pdfArray2 != null && !pdfArray6.Contains(layer.ReferenceHolder))
							{
								pdfArray6.Add(layer.ReferenceHolder);
							}
						}
					}
				}
				if (layer.Visible)
				{
					if (pdfArray3 != null && pdfArray2 != null && !pdfArray3.Contains(layer.ReferenceHolder))
					{
						pdfArray3.Add(layer.ReferenceHolder);
					}
				}
				else if (pdfArray4 != null && pdfArray2 != null && !pdfArray4.Contains(layer.ReferenceHolder))
				{
					pdfArray4.Add(layer.ReferenceHolder);
				}
			}
		}
		if (!flag)
		{
			pdfDictionary["OCGs"] = pdfPrimitive;
			pdfDictionary["D"] = CreateOptionalContentViews(layer, isLoadedDocument: true);
			document.Catalog.SetProperty("OCProperties", pdfDictionary);
		}
	}

	public void Move(int index, PdfLayer layer)
	{
		if (index < 0)
		{
			throw new ArgumentOutOfRangeException("index", "Value can not be less 0");
		}
		if (layer == null)
		{
			throw new ArgumentNullException("layer");
		}
		for (int i = 0; i < base.List.Count; i++)
		{
			if (base.List[i].Equals(layer))
			{
				int index2 = IndexOf(base.List[i] as PdfLayer);
				base.List.RemoveAt(index2);
			}
		}
		base.List.Insert(index, layer);
		InsertLayer(index, layer);
	}

	private IPdfPrimitive CreateOptionalContentDictionary(PdfLayer layer)
	{
		PdfDictionary pdfDictionary = new PdfDictionary();
		pdfDictionary["Name"] = new PdfString(layer.Name);
		pdfDictionary["Type"] = new PdfName("OCG");
		pdfDictionary["LayerID"] = new PdfName(layer.LayerId);
		pdfDictionary["Visible"] = new PdfBoolean(layer.Visible);
		if (layer.PrintState.Equals(PdfPrintState.AlwaysPrint) || layer.PrintState.Equals(PdfPrintState.NeverPrint) || layer.PrintState.Equals(PdfPrintState.PrintWhenVisible))
		{
			layer.m_usage = setPrintOption(layer);
			pdfDictionary["Usage"] = new PdfReferenceHolder(layer.m_usage);
			document.m_printLayer.Add(new PdfReferenceHolder(pdfDictionary));
		}
		PdfReferenceHolder pdfReferenceHolder = new PdfReferenceHolder(pdfDictionary);
		document.primitive.Add(pdfReferenceHolder);
		layer.ReferenceHolder = pdfReferenceHolder;
		layer.Dictionary = pdfDictionary;
		PdfDictionary ocProperties = PdfCrossTable.Dereference(document.Catalog["OCProperties"]) as PdfDictionary;
		CreateSublayer(ocProperties, pdfReferenceHolder, layer);
		if (layer.Visible)
		{
			document.m_on.Add(pdfReferenceHolder);
		}
		else
		{
			document.m_off.Add(pdfReferenceHolder);
		}
		return document.primitive;
	}

	private IPdfPrimitive CreateOptionalContentDictionary(PdfLayer layer, bool isLoadedDocument)
	{
		PdfDictionary pdfDictionary = new PdfDictionary();
		pdfDictionary["Name"] = new PdfString(layer.Name);
		pdfDictionary["Type"] = new PdfName("OCG");
		pdfDictionary["LayerID"] = new PdfName(layer.LayerId);
		pdfDictionary["Visible"] = new PdfBoolean(layer.Visible);
		if (layer.PrintState.Equals(PdfPrintState.AlwaysPrint) || layer.PrintState.Equals(PdfPrintState.NeverPrint) || layer.PrintState.Equals(PdfPrintState.PrintWhenVisible))
		{
			layer.m_usage = setPrintOption(layer);
			pdfDictionary["Usage"] = new PdfReferenceHolder(layer.m_usage);
			document.m_printLayer.Add(new PdfReferenceHolder(pdfDictionary));
		}
		PdfReferenceHolder pdfReferenceHolder = new PdfReferenceHolder(pdfDictionary);
		document.primitive.Add(pdfReferenceHolder);
		layer.Dictionary = pdfDictionary;
		layer.ReferenceHolder = pdfReferenceHolder;
		PdfDictionary ocProperties = PdfCrossTable.Dereference(document.Catalog["OCProperties"]) as PdfDictionary;
		CreateSublayer(ocProperties, pdfReferenceHolder, layer);
		if (layer.Visible)
		{
			document.m_on.Add(pdfReferenceHolder);
		}
		else
		{
			document.m_off.Add(pdfReferenceHolder);
		}
		m_isLayerContainsResource = true;
		return document.primitive;
	}

	private void CreateSublayer(PdfDictionary ocProperties, PdfReferenceHolder reference, PdfLayer layer)
	{
		if (!m_sublayer)
		{
			if (ocProperties != null)
			{
				PdfArray pdfArray = null;
				if (PdfCrossTable.Dereference(ocProperties["D"]) is PdfDictionary pdfDictionary)
				{
					pdfArray = PdfCrossTable.Dereference(pdfDictionary["Order"]) as PdfArray;
				}
				if (document.m_order != null && pdfArray != null)
				{
					document.m_order = pdfArray;
				}
				document.m_order.Add(reference);
			}
			else
			{
				document.m_order.Add(reference);
			}
			return;
		}
		layer.parent = m_parent;
		if (ocProperties != null)
		{
			PdfArray pdfArray2 = null;
			if (PdfCrossTable.Dereference(ocProperties["D"]) is PdfDictionary pdfDictionary2)
			{
				pdfArray2 = PdfCrossTable.Dereference(pdfDictionary2["Order"]) as PdfArray;
			}
			if (document.m_order != null && pdfArray2 != null)
			{
				document.m_order = pdfArray2;
			}
		}
		if (m_parent.m_child.Count == 0)
		{
			m_parent.sublayer.Add(reference);
		}
		else if (document.m_order.Contains(m_parent.ReferenceHolder))
		{
			int num = document.m_order.IndexOf(m_parent.ReferenceHolder);
			document.m_order.RemoveAt(num + 1);
			m_parent.sublayer.Add(reference);
		}
		else
		{
			m_parent.sublayer.Add(reference);
		}
		if (document.m_order.Contains(m_parent.ReferenceHolder))
		{
			int num2 = document.m_order.IndexOf(m_parent.ReferenceHolder);
			document.m_order.Insert(num2 + 1, m_parent.sublayer);
		}
		else if (m_parent.parent != null)
		{
			m_parent.parent.sublayer.Contains(m_parent.ReferenceHolder);
			int num3 = m_parent.parent.sublayer.IndexOf(m_parent.ReferenceHolder);
			if (m_parent.sublayer.Count == 1)
			{
				m_parent.parent.sublayer.Insert(num3 + 1, m_parent.sublayer);
			}
			if (document.m_order.Contains(m_parent.parent.ReferenceHolder))
			{
				num3 = document.m_order.IndexOf(m_parent.parent.ReferenceHolder);
				document.m_order.RemoveAt(num3 + 1);
				document.m_order.Insert(num3 + 1, m_parent.parent.sublayer);
			}
		}
		else if (document is PdfLoadedDocument)
		{
			for (int i = 0; i < document.m_order.Count; i++)
			{
				if (document.m_order[i] is PdfArray && (document.m_order[i] as PdfArray).Contains(m_parent.ReferenceHolder))
				{
					int num4 = (document.m_order[i] as PdfArray).IndexOf(m_parent.ReferenceHolder);
					if (m_parent.sublayer.Count == 1)
					{
						(document.m_order[i] as PdfArray).Insert(num4 + 1, m_parent.sublayer);
						break;
					}
				}
			}
		}
		if (!m_parent.m_child.Contains(layer))
		{
			m_parent.m_child.Add(layer);
		}
		if (m_parent.m_parentLayer.Count == 0)
		{
			layer.m_parentLayer.Add(m_parent);
			return;
		}
		for (int j = 0; j < m_parent.m_parentLayer.Count; j++)
		{
			if (!layer.m_parentLayer.Contains(m_parent.m_parentLayer[j]))
			{
				layer.m_parentLayer.Add(m_parent.m_parentLayer[j]);
			}
		}
		if (!layer.m_parentLayer.Contains(m_parent))
		{
			layer.m_parentLayer.Add(m_parent);
		}
	}

	private IPdfPrimitive CreateOptionalContentViews(PdfLayer layer)
	{
		PdfArray pdfArray = new PdfArray();
		m_OptionalContent["Name"] = new PdfString("Layers");
		m_OptionalContent["Order"] = document.m_order;
		m_OptionalContent["ON"] = document.m_on;
		m_OptionalContent["OFF"] = document.m_off;
		PdfArray pdfArray2 = new PdfArray();
		pdfArray2.Add(new PdfName("Print"));
		PdfDictionary pdfDictionary = new PdfDictionary();
		pdfDictionary.SetProperty("Category", pdfArray2);
		pdfDictionary.SetProperty("OCGs", document.m_printLayer);
		pdfDictionary.SetProperty("Event", new PdfName("Print"));
		pdfArray.Add(new PdfReferenceHolder(pdfDictionary));
		m_OptionalContent["AS"] = pdfArray;
		return m_OptionalContent;
	}

	private IPdfPrimitive CreateOptionalContentViews(PdfLayer layer, bool isLoadedDocument)
	{
		PdfArray pdfArray = new PdfArray();
		m_OptionalContent["Name"] = new PdfString("Layers");
		m_OptionalContent["Order"] = document.m_order;
		m_OptionalContent["ON"] = document.m_on;
		m_OptionalContent["OFF"] = document.m_off;
		PdfArray pdfArray2 = new PdfArray();
		pdfArray2.Add(new PdfName("Print"));
		PdfDictionary pdfDictionary = new PdfDictionary();
		pdfDictionary.SetProperty("Category", pdfArray2);
		pdfDictionary.SetProperty("OCGs", document.m_printLayer);
		pdfDictionary.SetProperty("Event", new PdfName("Print"));
		pdfArray.Add(new PdfReferenceHolder(pdfDictionary));
		m_OptionalContent["AS"] = pdfArray;
		return m_OptionalContent;
	}

	public void Remove(PdfLayer layer)
	{
		if (layer == null)
		{
			throw new ArgumentNullException("layer not to be null");
		}
		base.List.Remove(layer);
		RemoveLayer(layer, isRemoveContent: false);
	}

	public void Remove(PdfLayer layer, bool removeGraphicalContent)
	{
		if (layer == null)
		{
			throw new ArgumentNullException("layer not to be null");
		}
		base.List.Remove(layer);
		RemoveLayer(layer, removeGraphicalContent);
	}

	public void Remove(string name)
	{
		for (int i = 0; i < base.List.Count; i++)
		{
			PdfLayer pdfLayer = base.List[i] as PdfLayer;
			if (pdfLayer.Name == name)
			{
				RemoveLayer(pdfLayer, isRemoveContent: false);
				base.List.Remove(pdfLayer);
				i--;
			}
		}
	}

	public void Remove(string name, bool removeGraphicalContent)
	{
		for (int i = 0; i < base.List.Count; i++)
		{
			PdfLayer pdfLayer = base.List[i] as PdfLayer;
			if (pdfLayer.Name == name)
			{
				RemoveLayer(pdfLayer, removeGraphicalContent);
				base.List.Remove(pdfLayer);
				i--;
			}
		}
	}

	public void RemoveAt(int index)
	{
		if (index < 0 || index > base.List.Count - 1)
		{
			throw new ArgumentOutOfRangeException("index", "Value can not be less 0 and greater List.Count - 1");
		}
		PdfLayer pdfLayer = this[index];
		base.List.RemoveAt(index);
		if (pdfLayer != null)
		{
			RemoveLayer(pdfLayer, isRemoveContent: false);
		}
	}

	public void RemoveAt(int index, bool removeGraphicalContent)
	{
		if (index < 0 || index > base.List.Count - 1)
		{
			throw new ArgumentOutOfRangeException("index", "Value can not be less 0 and greater List.Count - 1");
		}
		PdfLayer pdfLayer = this[index];
		base.List.RemoveAt(index);
		if (pdfLayer != null)
		{
			RemoveLayer(pdfLayer, removeGraphicalContent);
		}
	}

	public bool Contains(PdfLayer layer)
	{
		if (layer == null)
		{
			throw new ArgumentNullException("layer not to be null");
		}
		return base.List.Contains(layer);
	}

	public bool Contains(string name)
	{
		if (name == null)
		{
			throw new ArgumentNullException("layerName not to null");
		}
		bool result = false;
		for (int i = 0; i < base.List.Count; i++)
		{
			PdfLayer pdfLayer = base.List[i] as PdfLayer;
			if (pdfLayer.Name == name)
			{
				result = base.List.Contains(pdfLayer);
				break;
			}
		}
		return result;
	}

	public int IndexOf(PdfLayer layer)
	{
		if (layer == null)
		{
			throw new ArgumentNullException("layer not to be null");
		}
		return base.List.IndexOf(layer);
	}

	public void Clear()
	{
		int i = 0;
		for (int count = base.List.Count; i < count; i++)
		{
			PdfLayer layer = this[i];
			RemoveLayer(layer, isRemoveContent: true);
		}
		base.List.Clear();
	}

	private void RemoveLayer(PdfLayer layer, bool isRemoveContent)
	{
		if (layer == null)
		{
			throw new ArgumentNullException("layer not to be null");
		}
		PdfDictionary pdfDictionary = null;
		if (layer == null && document == null)
		{
			return;
		}
		if (document != null)
		{
			pdfDictionary = document.Catalog;
			if (pdfDictionary.ContainsKey("OCProperties") && PdfCrossTable.Dereference(pdfDictionary["OCProperties"]) is PdfDictionary pdfDictionary2)
			{
				if (PdfCrossTable.Dereference(pdfDictionary2["OCGs"]) is PdfArray ocGroup)
				{
					RemoveOCG(layer, ocGroup);
				}
				if (pdfDictionary2.ContainsKey("D") && PdfCrossTable.Dereference(pdfDictionary2["D"]) is PdfDictionary pdfDictionary3)
				{
					PdfArray on = null;
					PdfArray off = null;
					if (pdfDictionary3.ContainsKey("Order") && PdfCrossTable.Dereference(pdfDictionary3["Order"]) is PdfArray order)
					{
						List<PdfArray> arrayList = new List<PdfArray>();
						RemoveOrder(layer, order, arrayList);
					}
					if (pdfDictionary3.ContainsKey("Locked") && PdfCrossTable.Dereference(pdfDictionary3["Locked"]) is PdfArray locked)
					{
						RemoveLocked(layer, locked);
					}
					if (pdfDictionary3.ContainsKey("OFF"))
					{
						off = PdfCrossTable.Dereference(pdfDictionary3["OFF"]) as PdfArray;
					}
					if (pdfDictionary3.ContainsKey("ON"))
					{
						on = PdfCrossTable.Dereference(pdfDictionary3["ON"]) as PdfArray;
					}
					if (pdfDictionary3.ContainsKey("AS") && PdfCrossTable.Dereference(pdfDictionary3["AS"]) is PdfArray usage)
					{
						RemoveUsage(layer, usage);
					}
					RemoveVisible(layer, on, off);
				}
			}
		}
		if (isRemoveContent)
		{
			RemoveLayerContent(layer);
		}
	}

	private void InsertLayer(int index, PdfLayer layer)
	{
		if (layer == null)
		{
			throw new ArgumentNullException("layer not to be null");
		}
		PdfReferenceHolder element = new PdfReferenceHolder(layer);
		if (layer.ReferenceHolder != null && layer.LayerId != null)
		{
			if (layer.Page != null && layer.Page.Contents.Contains(element))
			{
				int index2 = layer.Page.Contents.IndexOf(element);
				layer.Page.Contents.RemoveAt(index2);
				layer.Page.Contents.Insert(index, element);
			}
		}
		else if (layer.Page != null)
		{
			layer.Page.Contents.Insert(index, element);
		}
		if (document == null || !document.Catalog.ContainsKey("OCProperties") || !(PdfCrossTable.Dereference(document.Catalog["OCProperties"]) is PdfDictionary pdfDictionary))
		{
			return;
		}
		PdfArray pdfArray = PdfCrossTable.Dereference(pdfDictionary["OCGs"]) as PdfArray;
		if (pdfDictionary.ContainsKey("D") && PdfCrossTable.Dereference(pdfDictionary["D"]) is PdfDictionary pdfDictionary2 && PdfCrossTable.Dereference(pdfDictionary2["Order"]) is PdfArray pdfArray2 && pdfArray != null && pdfArray2.Contains(layer.ReferenceHolder) && index < pdfArray2.Count && pdfArray2[index] is PdfReferenceHolder && index + 1 < pdfArray2.Count && index + 2 < pdfArray2.Count && pdfArray2[index + 1] is PdfReferenceHolder && pdfArray2[index + 2] is PdfReferenceHolder)
		{
			pdfArray2.Remove(layer.ReferenceHolder);
			pdfArray2.Insert(index, layer.ReferenceHolder);
			if (pdfArray.Contains(layer.ReferenceHolder))
			{
				pdfArray.Remove(layer.ReferenceHolder);
				pdfArray.Insert(index, layer.ReferenceHolder);
			}
		}
	}

	private void CheckLayerVisible(PdfDictionary ocProperties)
	{
		PdfLoadedDocument obj = document as PdfLoadedDocument;
		PdfArray pdfArray = null;
		if (!obj.Catalog.ContainsKey("OCProperties"))
		{
			return;
		}
		if (PdfCrossTable.Dereference(ocProperties["D"]) is PdfDictionary pdfDictionary && pdfDictionary.ContainsKey("OFF"))
		{
			pdfArray = PdfCrossTable.Dereference(pdfDictionary["OFF"]) as PdfArray;
		}
		if (pdfArray == null)
		{
			return;
		}
		for (int i = 0; i < pdfArray.Count; i++)
		{
			PdfReferenceHolder pdfReferenceHolder = pdfArray[i] as PdfReferenceHolder;
			if (layerDictionary == null || layerDictionary.Count <= 0 || !(pdfReferenceHolder != null) || !layerDictionary.ContainsKey(pdfReferenceHolder))
			{
				continue;
			}
			PdfLayer pdfLayer = layerDictionary[pdfReferenceHolder];
			if (pdfLayer != null)
			{
				pdfLayer.m_visible = false;
				if (pdfLayer.Dictionary != null && pdfLayer.Dictionary.ContainsKey("Visible"))
				{
					pdfLayer.Dictionary.SetProperty("Visible", new PdfBoolean(value: false));
				}
			}
		}
	}

	private void CheckLayerLock(PdfDictionary ocProperties)
	{
		PdfArray pdfArray = null;
		if (PdfCrossTable.Dereference(ocProperties["D"]) is PdfDictionary pdfDictionary && pdfDictionary.ContainsKey("Locked"))
		{
			pdfArray = PdfCrossTable.Dereference(pdfDictionary["Locked"]) as PdfArray;
		}
		if (pdfArray == null)
		{
			return;
		}
		for (int i = 0; i < pdfArray.Count; i++)
		{
			PdfLayer pdfLayer = layerDictionary[pdfArray[i] as PdfReferenceHolder];
			if (pdfLayer != null)
			{
				pdfLayer.m_locked = true;
			}
		}
	}

	private void RemoveOCG(PdfLayer layer, PdfArray ocGroup)
	{
		if (ocGroup != null && ocGroup.Contains(layer.ReferenceHolder))
		{
			ocGroup.Remove(layer.ReferenceHolder);
		}
	}

	private void RemoveUsage(PdfLayer layer, PdfArray m_usage)
	{
		if (m_usage == null)
		{
			return;
		}
		bool flag = false;
		for (int i = 0; i < m_usage.Count; i++)
		{
			if (!(m_usage[i] is PdfReferenceHolder) && !(m_usage[i] is PdfDictionary))
			{
				continue;
			}
			PdfDictionary pdfDictionary = null;
			pdfDictionary = ((!(m_usage[i] is PdfReferenceHolder)) ? (m_usage[i] as PdfDictionary) : ((m_usage[i] as PdfReferenceHolder).Object as PdfDictionary));
			if (pdfDictionary != null && pdfDictionary["OCGs"] is PdfArray pdfArray)
			{
				if (pdfArray.Contains(layer.ReferenceHolder))
				{
					pdfArray.Remove(layer.ReferenceHolder);
					flag = true;
				}
				if (flag)
				{
					break;
				}
			}
		}
	}

	private void RemoveOrder(PdfLayer layer, PdfArray order, List<PdfArray> arrayList)
	{
		bool flag = false;
		if (order != null)
		{
			for (int i = 0; i < order.Count; i++)
			{
				if (order[i] is PdfReferenceHolder)
				{
					if (!order[i].Equals(layer.ReferenceHolder))
					{
						continue;
					}
					if (i != order.Count - 1)
					{
						if (order[i + 1] is PdfArray)
						{
							order.RemoveAt(i);
							order.RemoveAt(i);
							flag = true;
						}
						else
						{
							order.RemoveAt(i);
							flag = true;
						}
					}
					else
					{
						order.RemoveAt(i);
						flag = true;
					}
					break;
				}
				if (order[i] is PdfArray)
				{
					arrayList.Add(order[i] as PdfArray);
				}
			}
		}
		if (!flag && arrayList != null)
		{
			int num;
			for (num = 0; num < arrayList.Count; num++)
			{
				order = arrayList[num];
				arrayList.RemoveAt(num);
				num--;
				RemoveOrder(layer, order, arrayList);
			}
		}
	}

	private void RemoveVisible(PdfLayer layer, PdfArray on, PdfArray off)
	{
		if (layer.Visible)
		{
			if (on != null && on.Contains(layer.ReferenceHolder))
			{
				on.Remove(layer.ReferenceHolder);
			}
		}
		else if (off != null && off.Contains(layer.ReferenceHolder))
		{
			off.Remove(layer.ReferenceHolder);
		}
	}

	private void RemoveLocked(PdfLayer layer, PdfArray locked)
	{
		if (locked != null && locked.Contains(layer.ReferenceHolder))
		{
			locked.Remove(layer.ReferenceHolder);
		}
	}

	private void RemoveLayerContent(PdfLayer layer)
	{
		PdfDictionary pdfDictionary = null;
		bool flag = false;
		PdfDictionary pdfDictionary2 = null;
		if (layer.Page == null)
		{
			return;
		}
		for (int i = 0; i < layer.pages.Count; i++)
		{
			if (PdfCrossTable.Dereference(layer.pages[i].Dictionary["Resources"]) is PdfDictionary pdfDictionary3)
			{
				pdfDictionary = PdfCrossTable.Dereference(pdfDictionary3["Properties"]) as PdfDictionary;
				pdfDictionary2 = PdfCrossTable.Dereference(pdfDictionary3["XObject"]) as PdfDictionary;
				if (pdfDictionary != null && !string.IsNullOrEmpty(layer.LayerId) && pdfDictionary.ContainsKey(layer.LayerId.TrimStart('/')))
				{
					pdfDictionary.Remove(layer.LayerId.TrimStart('/'));
				}
				if (pdfDictionary2 != null && layer.xobject.Count > 0)
				{
					foreach (KeyValuePair<PdfName, IPdfPrimitive> item in pdfDictionary2.Items)
					{
						if (layer.xobject.Contains(item.Key.Value.TrimStart('/')))
						{
							pdfDictionary2.Remove(item.Key);
							break;
						}
					}
				}
			}
			PdfArray contents = layer.pages[i].Contents;
			for (int j = 0; j < contents.Count; j++)
			{
				MemoryStream memoryStream = new MemoryStream();
				PdfStream pdfStream = new PdfStream();
				if (layer.pages[i] is PdfLoadedPage)
				{
					(PdfCrossTable.Dereference(contents[j]) as PdfStream).Decompress();
				}
				(PdfCrossTable.Dereference(contents[j]) as PdfStream).InternalStream.WriteTo(memoryStream);
				PdfRecordCollection pdfRecordCollection = new ContentParser(memoryStream.ToArray()).ReadContent();
				for (int k = 0; k < pdfRecordCollection.RecordCollection.Count; k++)
				{
					string operatorName = pdfRecordCollection.RecordCollection[k].OperatorName;
					switch (operatorName)
					{
					case "BMC":
					case "EMC":
					case "BDC":
						ProcessBeginMarkContent(layer, operatorName, pdfRecordCollection.RecordCollection[k].Operands, pdfStream);
						flag = true;
						break;
					}
					if (operatorName == "Do" && layer.xobject.Contains(pdfRecordCollection.RecordCollection[k].Operands[0].TrimStart('/')))
					{
						flag = true;
					}
					switch (operatorName)
					{
					case "q":
					case "Q":
					case "w":
					case "J":
					case "j":
					case "M":
					case "d":
					case "ri":
					case "i":
					case "gs":
					case "g":
					case "cm":
					case "G":
					case "rg":
					case "RG":
					case "k":
					case "K":
					case "cs":
					case "CS":
					case "scn":
					case "SCN":
					case "sc":
					case "SC":
						if (!flag)
						{
							StreamWrite(pdfRecordCollection.RecordCollection[k].Operands, operatorName, skip: false, pdfStream);
						}
						flag = false;
						break;
					default:
						if (!flag)
						{
							StreamWrite(pdfRecordCollection.RecordCollection[k].Operands, operatorName, skip: true, pdfStream);
						}
						flag = false;
						break;
					}
				}
				if (pdfStream.Data.Length != 0)
				{
					(PdfCrossTable.Dereference(layer.pages[i].Contents[j]) as PdfStream).Clear();
					(PdfCrossTable.Dereference(layer.pages[i].Contents[j]) as PdfStream).Write(pdfStream.Data);
				}
				memoryStream.Dispose();
			}
		}
	}

	private void CheckParentLayer(PdfDictionary ocProperties)
	{
		if (PdfCrossTable.Dereference(ocProperties["D"]) is PdfDictionary pdfDictionary && PdfCrossTable.Dereference(pdfDictionary["Order"]) is PdfArray array)
		{
			ParsingLayerOrder(null, array, layerDictionary);
		}
	}

	private void ParsingLayerOrder(PdfLayer parent, PdfArray array, Dictionary<PdfReferenceHolder, PdfLayer> layerDictionary)
	{
		PdfLayer pdfLayer = null;
		for (int i = 0; i < array.Count; i++)
		{
			PdfReferenceHolder key = array[i] as PdfReferenceHolder;
			if (array[i] is PdfReferenceHolder)
			{
				if (layerDictionary.ContainsKey(key))
				{
					pdfLayer = layerDictionary[key];
				}
				if (pdfLayer == null)
				{
					continue;
				}
				if (parent != null)
				{
					if (!parent.m_child.Contains(pdfLayer))
					{
						parent.m_child.Add(pdfLayer);
					}
					if (parent.m_parentLayer.Count == 0)
					{
						pdfLayer.m_parentLayer.Add(parent);
						pdfLayer.parent = parent;
					}
					else
					{
						for (int j = 0; j < parent.m_parentLayer.Count; j++)
						{
							if (!pdfLayer.m_parentLayer.Contains(parent.m_parentLayer[j]))
							{
								pdfLayer.m_parentLayer.Add(parent.m_parentLayer[j]);
							}
						}
						pdfLayer.m_parentLayer.Add(parent);
						pdfLayer.parent = parent;
					}
				}
				if (array.Count > i + 1 && PdfCrossTable.Dereference(array[i + 1]) is PdfArray)
				{
					i++;
					ParsingLayerOrder(pdfLayer, pdfLayer.sublayer = PdfCrossTable.Dereference(array[i]) as PdfArray, layerDictionary);
				}
			}
			else if (PdfCrossTable.Dereference(array[i]) is PdfArray)
			{
				if (!(PdfCrossTable.Dereference(array[i]) is PdfArray pdfArray) || (pdfArray != null && pdfArray.Count == 0))
				{
					break;
				}
				PdfCrossTable.Dereference(pdfArray[0]);
				if (pdfArray[0] is PdfString)
				{
					parent = null;
					ParsingLayerOrder(parent, pdfArray, layerDictionary);
				}
				else
				{
					parent = null;
					ParsingLayerOrder(parent, PdfCrossTable.Dereference(array[i]) as PdfArray, layerDictionary);
				}
			}
		}
	}

	private string FindOperator(int token)
	{
		return new string[79]
		{
			"b", "B", "bx", "Bx", "BDC", "BI", "BMC", "BT", "BX", "c",
			"cm", "CS", "cs", "d", "d0", "d1", "Do", "DP", "EI", "EMC",
			"ET", "EX", "f", "F", "fx", "G", "g", "gs", "h", "i",
			"ID", "j", "J", "K", "k", "l", "m", "M", "MP", "n",
			"q", "Q", "re", "RG", "rg", "ri", "s", "S", "SC", "sc",
			"SCN", "scn", "sh", "f*", "Tx", "Tc", "Td", "TD", "Tf", "Tj",
			"TJ", "TL", "Tm", "Tr", "Ts", "Tw", "Tz", "v", "w", "W",
			"W*", "Wx", "y", "T*", "b*", "B*", "'", "\"", "true"
		}.GetValue(token) as string;
	}

	private void StreamWrite(string[] operands, string mOperator, bool skip, PdfStream data)
	{
		PdfString pdfString = null;
		if (skip && IsSkip)
		{
			return;
		}
		if (operands != null)
		{
			for (int i = 0; i < operands.Length; i++)
			{
				pdfString = new PdfString(operands[i]);
				data.Write(pdfString.Bytes);
				data.Write(" ");
			}
		}
		pdfString = new PdfString(mOperator);
		data.Write(pdfString.Bytes);
		data.Write("\r\n");
	}

	private void ProcessBeginMarkContent(PdfLayer parser, string m_operator, string[] operands, PdfStream data)
	{
		if ("BDC".Equals(m_operator.ToString()))
		{
			string text = null;
			if (operands.Length > 1 && operands[0].TrimStart('/').Equals("OC"))
			{
				text = operands[1].ToString().TrimStart('/');
			}
			if (m_bdcCount > 0)
			{
				m_bdcCount++;
				return;
			}
			if (text != null && text.Equals(parser.LayerId))
			{
				m_bdcCount++;
			}
		}
		StreamWrite(operands, m_operator, skip: true, data);
		if ("EMC".Equals(m_operator.ToString()) && m_bdcCount > 0)
		{
			m_bdcCount--;
		}
	}

	private void CreateLayerHierarchical(PdfDictionary ocProperties)
	{
		if (!(PdfCrossTable.Dereference(ocProperties["D"]) is PdfDictionary pdfDictionary) || !pdfDictionary.ContainsKey("Order") || layerDictionary == null || layerDictionary.Count <= 0)
		{
			return;
		}
		base.List.Clear();
		foreach (KeyValuePair<PdfReferenceHolder, PdfLayer> item in layerDictionary)
		{
			PdfLayer value = item.Value;
			if (value != null)
			{
				if (value.parent == null && !base.List.Contains(value))
				{
					base.List.Add(value);
				}
				else if (value.m_child.Count > 0)
				{
					AddChildlayer(value.parent);
				}
				else if (value.parent != null && value.m_child.Count == 0 && !value.parent.Layers.Contains(value))
				{
					value.parent.Layers.AddNestedLayer(value);
				}
			}
		}
	}

	private void AddChildlayer(PdfLayer pdflayer)
	{
		for (int i = 0; i < pdflayer.m_child.Count; i++)
		{
			PdfLayer layer = pdflayer.m_child[i];
			if (!pdflayer.Layers.Contains(layer))
			{
				pdflayer.Layers.AddNestedLayer(layer);
			}
		}
	}
}
