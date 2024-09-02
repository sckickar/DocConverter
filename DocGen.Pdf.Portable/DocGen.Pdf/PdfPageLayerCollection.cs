using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DocGen.Pdf.IO;
using DocGen.Pdf.Parsing;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf;

public class PdfPageLayerCollection : PdfCollection
{
	private PdfPageBase m_page;

	internal bool m_sublayer;

	private int parentLayerCount;

	internal PdfDictionary m_OptionalContent = new PdfDictionary();

	private List<string> m_layerCollection = new List<string>();

	private bool m_isLayerContainsResource;

	private static object s_syncLockLayer = new object();

	private bool isLayerPresent;

	private int m_bdcCount;

	private Dictionary<PdfReferenceHolder, PdfDictionary> documentLayers;

	private static object m_resourceLock = new object();

	internal bool isOptimizeContent;

	private bool m_parseLayer;

	public PdfPageLayer this[int index]
	{
		get
		{
			return base.List[index] as PdfPageLayer;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("layer");
			}
			if (value.Page != m_page)
			{
				throw new ArgumentException("The layer belongs to another page");
			}
			PdfPageLayer pdfPageLayer = this[index];
			if (pdfPageLayer != null)
			{
				RemoveLayer(pdfPageLayer);
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

	public PdfPageLayerCollection()
	{
	}

	public PdfPageLayerCollection(PdfPageBase page)
	{
		if (page == null)
		{
			throw new ArgumentNullException("page");
		}
		m_page = page;
		if (page != null)
		{
			ParseLayers(page);
		}
	}

	internal PdfPageLayerCollection(PdfPageBase page, bool initializeLayer)
	{
		if (page == null)
		{
			throw new ArgumentNullException("page");
		}
		m_page = page;
		if (page != null)
		{
			m_parseLayer = initializeLayer;
			ParseLayers(page);
		}
	}

	public PdfPageLayer Add()
	{
		lock (s_syncLockLayer)
		{
			PdfPageLayer pdfPageLayer = new PdfPageLayer(m_page);
			pdfPageLayer.Name = string.Empty;
			Add(pdfPageLayer);
			return pdfPageLayer;
		}
	}

	public PdfPageLayer Add(string LayerName, bool Visible)
	{
		lock (s_syncLockLayer)
		{
			PdfPageLayer pdfPageLayer = new PdfPageLayer(m_page);
			pdfPageLayer.Name = LayerName;
			pdfPageLayer.Visible = Visible;
			pdfPageLayer.LayerId = "OCG_" + Guid.NewGuid();
			Add(pdfPageLayer);
			return pdfPageLayer;
		}
	}

	public PdfPageLayer Add(string LayerName)
	{
		lock (s_syncLockLayer)
		{
			PdfPageLayer pdfPageLayer = new PdfPageLayer(m_page);
			pdfPageLayer.Name = LayerName;
			pdfPageLayer.LayerId = "OCG_" + Guid.NewGuid();
			Add(pdfPageLayer);
			return pdfPageLayer;
		}
	}

	public int Add(PdfPageLayer layer)
	{
		lock (s_syncLockLayer)
		{
			if (layer == null)
			{
				throw new ArgumentNullException("layer");
			}
			if (layer.Page != m_page)
			{
				throw new ArgumentException("The layer belongs to another page");
			}
			base.List.Add(layer);
			int num = base.List.Count - 1;
			AddLayer(num, layer);
			if (layer.LayerId != null)
			{
				if (m_page is PdfPage)
				{
					CreateLayer(layer);
				}
				else
				{
					CreateLayerLoadedPage(layer);
				}
			}
			return num;
		}
	}

	private void CreateLayer(PdfPageLayer layer)
	{
		PdfPage pdfPage = m_page as PdfPage;
		PdfDocumentBase pdfDocumentBase = pdfPage.Document;
		if (pdfDocumentBase == null)
		{
			pdfDocumentBase = pdfPage.Section.ParentDocument;
		}
		PdfDictionary pdfDictionary = new PdfDictionary();
		IPdfPrimitive value = CreateOptionalContentDictionary(layer);
		pdfDictionary["OCGs"] = value;
		pdfDictionary["D"] = CreateOptionalContentViews(layer);
		pdfDocumentBase?.Catalog.SetProperty("OCProperties", pdfDictionary);
	}

	private PdfDictionary setPrintOption(PdfPageLayer layer)
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

	private void CreateLayerLoadedPage(PdfPageLayer layer)
	{
		PdfLoadedPage pdfLoadedPage = m_page as PdfLoadedPage;
		PdfDictionary pdfDictionary = new PdfDictionary();
		IPdfPrimitive value = CreateOptionalContentDictionary(layer, isLoadedPage: true);
		bool flag = false;
		if (pdfLoadedPage != null && pdfLoadedPage.Document != null && pdfLoadedPage.Document.Catalog != null && pdfLoadedPage.Document.Catalog.ContainsKey("OCProperties") && PdfCrossTable.Dereference(pdfLoadedPage.Document.Catalog["OCProperties"]) is PdfDictionary pdfDictionary2 && pdfDictionary2.ContainsKey("OCGs"))
		{
			if (PdfCrossTable.Dereference(pdfDictionary2["OCGs"]) is PdfArray pdfArray)
			{
				flag = true;
				if (!pdfArray.Contains(layer.ReferenceHolder))
				{
					pdfArray.Add(layer.ReferenceHolder);
				}
			}
			if (pdfDictionary2.ContainsKey("D") && pdfDictionary2["D"] is PdfDictionary pdfDictionary3)
			{
				PdfArray pdfArray2 = PdfCrossTable.Dereference(pdfDictionary3["ON"]) as PdfArray;
				PdfArray pdfArray3 = PdfCrossTable.Dereference(pdfDictionary3["Order"]) as PdfArray;
				PdfArray pdfArray4 = PdfCrossTable.Dereference(pdfDictionary3["OFF"]) as PdfArray;
				PdfArray pdfArray5 = PdfCrossTable.Dereference(pdfDictionary3["AS"]) as PdfArray;
				if (pdfArray2 == null)
				{
					pdfArray2 = (PdfArray)(pdfDictionary3["ON"] = new PdfArray());
				}
				if (pdfArray3 != null && !pdfArray3.Contains(layer.ReferenceHolder))
				{
					pdfArray3.Add(layer.ReferenceHolder);
				}
				if (layer.Visible && !pdfArray2.Contains(layer.ReferenceHolder))
				{
					pdfArray2.Add(layer.ReferenceHolder);
				}
				if (!layer.Visible && pdfArray4 != null && !pdfArray4.Contains(layer.ReferenceHolder))
				{
					pdfArray4.Add(layer.ReferenceHolder);
				}
				if (pdfArray5 != null && pdfArray5.Count > 0 && PdfCrossTable.Dereference(pdfArray5[0]) is PdfDictionary pdfDictionary4 && pdfDictionary4.ContainsKey("OCGs") && PdfCrossTable.Dereference(pdfDictionary4["OCGs"]) is PdfArray pdfArray6 && !pdfArray6.Contains(layer.ReferenceHolder))
				{
					pdfArray6.Add(layer.ReferenceHolder);
				}
			}
		}
		if (!flag && pdfLoadedPage != null && pdfLoadedPage.Document != null && pdfLoadedPage.Document.Catalog != null)
		{
			pdfDictionary["OCGs"] = value;
			pdfDictionary["D"] = CreateOptionalContentViews(layer, isLoadedPage: true);
			pdfLoadedPage.Document.Catalog.SetProperty("OCProperties", pdfDictionary);
		}
	}

	public void Insert(int index, PdfPageLayer layer)
	{
		if (index < 0)
		{
			throw new ArgumentOutOfRangeException("index", "Value can not be less 0");
		}
		if (layer == null)
		{
			throw new ArgumentNullException("layer");
		}
		if (layer.Page != m_page)
		{
			throw new ArgumentException("The layer belongs to another page");
		}
		if (!base.List.Contains(layer) && layer.LayerId != null)
		{
			int index2 = Add(layer);
			base.List.RemoveAt(index2);
		}
		base.List.Insert(index, layer);
		InsertLayer(index, layer);
	}

	private IPdfPrimitive CreateOptionalContentDictionary(PdfPageLayer layer)
	{
		PdfPage pdfPage = m_page as PdfPage;
		PdfDocumentBase pdfDocumentBase = pdfPage.Document;
		if (pdfDocumentBase == null)
		{
			pdfDocumentBase = pdfPage.Section.ParentDocument;
		}
		PdfDictionary pdfDictionary = new PdfDictionary();
		pdfDictionary["Name"] = new PdfString(layer.Name);
		pdfDictionary["Type"] = new PdfName("OCG");
		pdfDictionary["LayerID"] = new PdfName(layer.LayerId);
		pdfDictionary["Visible"] = new PdfBoolean(layer.Visible);
		if (layer.PrintState.Equals(PdfPrintState.AlwaysPrint) || layer.PrintState.Equals(PdfPrintState.NeverPrint) || layer.PrintState.Equals(PdfPrintState.PrintWhenVisible))
		{
			layer.m_usage = setPrintOption(layer);
			pdfDictionary["Usage"] = new PdfReferenceHolder(layer.m_usage);
			pdfDocumentBase?.m_printLayer.Add(new PdfReferenceHolder(pdfDictionary));
		}
		PdfReferenceHolder pdfReferenceHolder = new PdfReferenceHolder(pdfDictionary);
		pdfDocumentBase?.primitive.Insert(pdfDocumentBase.m_positon, pdfReferenceHolder);
		layer.Dictionary = pdfDictionary;
		layer.ReferenceHolder = pdfReferenceHolder;
		if (pdfDocumentBase != null)
		{
			if (!m_sublayer)
			{
				layer.m_sublayer = false;
				if (pdfDocumentBase.m_sublayerposition > 0)
				{
					int count = m_page.Contents.Count;
					m_page.Contents.RemoveAt(parentLayerCount - 1);
					PdfStream pdfStream = new PdfStream();
					byte[] bytes = Encoding.UTF8.GetBytes("EMC\n");
					pdfStream.Write(bytes);
					m_page.Contents.Insert(count - 2, new PdfReferenceHolder(pdfStream));
				}
				pdfDocumentBase.m_sublayerposition = 0;
				pdfDocumentBase.m_sublayer = new PdfArray();
				pdfDocumentBase.m_order.Insert(pdfDocumentBase.m_orderposition, pdfReferenceHolder);
				pdfDocumentBase.m_orderposition++;
				parentLayerCount = m_page.Contents.Count;
			}
			else
			{
				layer.m_sublayer = true;
				pdfDocumentBase.m_sublayer.Insert(pdfDocumentBase.m_sublayerposition, pdfReferenceHolder);
				if (pdfDocumentBase.m_sublayerposition != 0)
				{
					pdfDocumentBase.m_order.RemoveAt(pdfDocumentBase.m_orderposition - 1);
					pdfDocumentBase.m_orderposition--;
				}
				pdfDocumentBase.m_order.Insert(pdfDocumentBase.m_orderposition, pdfDocumentBase.m_sublayer);
				pdfDocumentBase.m_sublayerposition++;
				pdfDocumentBase.m_orderposition++;
			}
			if (layer.Visible)
			{
				pdfDocumentBase.m_on.Insert(pdfDocumentBase.m_onpositon, pdfReferenceHolder);
				pdfDocumentBase.m_onpositon++;
			}
			else
			{
				pdfDocumentBase.m_off.Insert(pdfDocumentBase.m_offpositon, pdfReferenceHolder);
				pdfDocumentBase.m_offpositon++;
			}
			pdfDocumentBase.m_positon++;
		}
		pdfPage.GetResources().AddProperties(layer.LayerId, pdfReferenceHolder);
		PdfArray result = null;
		if (pdfDocumentBase != null)
		{
			result = pdfDocumentBase.primitive;
		}
		return result;
	}

	private IPdfPrimitive CreateOptionalContentDictionary(PdfPageLayer layer, bool isLoadedPage)
	{
		PdfLoadedPage pdfLoadedPage = m_page as PdfLoadedPage;
		PdfDictionary pdfDictionary = new PdfDictionary();
		pdfDictionary["Name"] = new PdfString(layer.Name);
		pdfDictionary["Type"] = new PdfName("OCG");
		pdfDictionary["LayerID"] = new PdfName(layer.LayerId);
		pdfDictionary["Visible"] = new PdfBoolean(layer.Visible);
		if (layer.PrintState.Equals(PdfPrintState.AlwaysPrint) || layer.PrintState.Equals(PdfPrintState.NeverPrint) || layer.PrintState.Equals(PdfPrintState.PrintWhenVisible))
		{
			layer.m_usage = setPrintOption(layer);
			pdfDictionary["Usage"] = new PdfReferenceHolder(layer.m_usage);
			pdfLoadedPage.Document.m_printLayer.Add(new PdfReferenceHolder(pdfDictionary));
		}
		PdfReferenceHolder pdfReferenceHolder = new PdfReferenceHolder(pdfDictionary);
		pdfLoadedPage.Document.primitive.Insert(pdfLoadedPage.Document.m_positon, pdfReferenceHolder);
		layer.Dictionary = pdfDictionary;
		layer.ReferenceHolder = pdfReferenceHolder;
		if (!m_sublayer)
		{
			layer.m_sublayer = false;
			if (pdfLoadedPage.Document.m_sublayerposition > 0)
			{
				int count = m_page.Contents.Count;
				m_page.Contents.RemoveAt(parentLayerCount - 1);
				PdfStream pdfStream = new PdfStream();
				byte[] bytes = Encoding.UTF8.GetBytes("EMC\n");
				pdfStream.Write(bytes);
				m_page.Contents.Insert(count - 2, new PdfReferenceHolder(pdfStream));
			}
			pdfLoadedPage.Document.m_sublayerposition = 0;
			pdfLoadedPage.Document.m_sublayer = new PdfArray();
			pdfLoadedPage.Document.m_order.Insert(pdfLoadedPage.Document.m_orderposition, pdfReferenceHolder);
			pdfLoadedPage.Document.m_orderposition++;
			parentLayerCount = m_page.Contents.Count;
		}
		else
		{
			layer.m_sublayer = true;
			pdfLoadedPage.Document.m_sublayer.Insert(pdfLoadedPage.Document.m_sublayerposition, pdfReferenceHolder);
			if (pdfLoadedPage.Document.m_sublayerposition != 0)
			{
				pdfLoadedPage.Document.m_order.RemoveAt(pdfLoadedPage.Document.m_orderposition - 1);
				pdfLoadedPage.Document.m_orderposition--;
			}
			pdfLoadedPage.Document.m_order.Insert(pdfLoadedPage.Document.m_orderposition, pdfLoadedPage.Document.m_sublayer);
			pdfLoadedPage.Document.m_sublayerposition++;
			pdfLoadedPage.Document.m_orderposition++;
		}
		if (layer.Visible)
		{
			pdfLoadedPage.Document.m_on.Insert(pdfLoadedPage.Document.m_onpositon, pdfReferenceHolder);
			pdfLoadedPage.Document.m_onpositon++;
		}
		else
		{
			pdfLoadedPage.Document.m_off.Insert(pdfLoadedPage.Document.m_offpositon, pdfReferenceHolder);
			pdfLoadedPage.Document.m_offpositon++;
		}
		pdfLoadedPage.Document.m_positon++;
		PdfResources resources = pdfLoadedPage.GetResources();
		if (resources != null && resources.ContainsKey("Properties"))
		{
			if (resources["Properties"] is PdfDictionary pdfDictionary2)
			{
				m_isLayerContainsResource = true;
				pdfDictionary2[layer.LayerId] = pdfReferenceHolder;
			}
			else
			{
				resources.AddProperties(layer.LayerId, pdfReferenceHolder);
			}
		}
		else
		{
			resources.AddProperties(layer.LayerId, pdfReferenceHolder);
		}
		return pdfLoadedPage.Document.primitive;
	}

	private void WriteEndMark()
	{
		PdfStream pdfStream = new PdfStream();
		byte[] bytes = Encoding.ASCII.GetBytes("EMC\n");
		pdfStream.Write(bytes);
		m_page.Contents.Add(new PdfReferenceHolder(pdfStream));
	}

	private IPdfPrimitive CreateOptionalContentViews(PdfPageLayer layer)
	{
		PdfPage pdfPage = m_page as PdfPage;
		PdfDocumentBase pdfDocumentBase = pdfPage.Document;
		if (pdfDocumentBase == null)
		{
			pdfDocumentBase = pdfPage.Section.ParentDocument;
		}
		PdfArray pdfArray = new PdfArray();
		m_OptionalContent["Name"] = new PdfString("Layers");
		if (pdfDocumentBase != null)
		{
			m_OptionalContent["Order"] = pdfDocumentBase.m_order;
			m_OptionalContent["ON"] = pdfDocumentBase.m_on;
			m_OptionalContent["OFF"] = pdfDocumentBase.m_off;
		}
		PdfArray pdfArray2 = new PdfArray();
		pdfArray2.Add(new PdfName("Print"));
		PdfDictionary pdfDictionary = new PdfDictionary();
		pdfDictionary.SetProperty("Category", pdfArray2);
		if (pdfDocumentBase != null)
		{
			pdfDictionary.SetProperty("OCGs", pdfDocumentBase.m_printLayer);
		}
		pdfDictionary.SetProperty("Event", new PdfName("Print"));
		pdfArray.Add(new PdfReferenceHolder(pdfDictionary));
		if (PdfDocument.ConformanceLevel != PdfConformanceLevel.Pdf_A3B && PdfDocument.ConformanceLevel != PdfConformanceLevel.Pdf_A3A && PdfDocument.ConformanceLevel != PdfConformanceLevel.Pdf_A3U && PdfDocument.ConformanceLevel != PdfConformanceLevel.Pdf_A2B && PdfDocument.ConformanceLevel != PdfConformanceLevel.Pdf_A2A && PdfDocument.ConformanceLevel != PdfConformanceLevel.Pdf_A2U)
		{
			m_OptionalContent["AS"] = pdfArray;
		}
		return m_OptionalContent;
	}

	private IPdfPrimitive CreateOptionalContentViews(PdfPageLayer layer, bool isLoadedPage)
	{
		PdfLoadedPage pdfLoadedPage = m_page as PdfLoadedPage;
		PdfArray pdfArray = new PdfArray();
		m_OptionalContent["Name"] = new PdfString("Layers");
		m_OptionalContent["Order"] = pdfLoadedPage.Document.m_order;
		m_OptionalContent["ON"] = pdfLoadedPage.Document.m_on;
		m_OptionalContent["OFF"] = pdfLoadedPage.Document.m_off;
		PdfArray pdfArray2 = new PdfArray();
		pdfArray2.Add(new PdfName("Print"));
		PdfDictionary pdfDictionary = new PdfDictionary();
		pdfDictionary.SetProperty("Category", pdfArray2);
		pdfDictionary.SetProperty("OCGs", pdfLoadedPage.Document.m_printLayer);
		pdfDictionary.SetProperty("Event", new PdfName("Print"));
		pdfArray.Add(new PdfReferenceHolder(pdfDictionary));
		m_OptionalContent["AS"] = pdfArray;
		return m_OptionalContent;
	}

	public void Remove(PdfPageLayer layer)
	{
		if (layer == null)
		{
			throw new ArgumentNullException("layer");
		}
		base.List.Remove(layer);
		RemoveLayer(layer);
	}

	public void Remove(string name)
	{
		for (int i = 0; i < base.List.Count; i++)
		{
			PdfPageLayer pdfPageLayer = base.List[i] as PdfPageLayer;
			if (pdfPageLayer.Name == name)
			{
				RemoveLayer(pdfPageLayer);
				base.List.Remove(pdfPageLayer);
				break;
			}
		}
	}

	public void RemoveAt(int index)
	{
		if (index < 0 || index > base.List.Count - 1)
		{
			throw new ArgumentOutOfRangeException("index", "Value can not be less 0 and greater List.Count - 1");
		}
		PdfPageLayer pdfPageLayer = this[index];
		base.List.RemoveAt(index);
		if (pdfPageLayer != null)
		{
			RemoveLayer(pdfPageLayer);
		}
	}

	public bool Contains(PdfPageLayer layer)
	{
		if (layer == null)
		{
			throw new ArgumentNullException("layer");
		}
		return base.List.Contains(layer);
	}

	public int IndexOf(PdfPageLayer layer)
	{
		if (layer == null)
		{
			throw new ArgumentNullException("layer");
		}
		return base.List.IndexOf(layer);
	}

	public void Clear()
	{
		int i = 0;
		for (int count = base.List.Count; i < count; i++)
		{
			RemoveAt(0);
		}
	}

	internal void CombineContent(Stream stream)
	{
		byte b = 113;
		byte b2 = 10;
		byte b3 = 81;
		byte b4 = 32;
		byte[] array = new byte[4] { b4, b, b4, b2 };
		stream.Write(array, 0, array.Length);
		array[0] = b4;
		array[1] = b3;
		array[2] = b4;
		array[3] = b2;
		PdfLoadedPage pdfLoadedPage = m_page as PdfLoadedPage;
		bool flag = false;
		bool flag2 = false;
		if (pdfLoadedPage != null)
		{
			flag = true;
			if (pdfLoadedPage.Document != null && pdfLoadedPage.Document is PdfLoadedDocument pdfLoadedDocument)
			{
				flag2 = pdfLoadedDocument.m_redactionPages.Contains(pdfLoadedPage);
			}
		}
		else if ((m_page as PdfPage).Imported)
		{
			flag = true;
		}
		byte[] array2 = PdfString.StringToByte("\r\n");
		if (pdfLoadedPage != null && (isLayerPresent || pdfLoadedPage.Contents.Count > 0 || flag2))
		{
			CombineProcess(pdfLoadedPage, flag, stream, array2);
		}
		else if (pdfLoadedPage == null && m_page != null && flag && base.Count == 0)
		{
			int i = 0;
			for (int count = m_page.Contents.Count; i < count; i++)
			{
				if (PdfCrossTable.Dereference(m_page.Contents[i]) is PdfStream pdfStream)
				{
					byte[] decompressedData = pdfStream.GetDecompressedData();
					if (decompressedData != null)
					{
						stream.Write(decompressedData, 0, decompressedData.Length);
					}
					stream.Write(array2, 0, array2.Length);
				}
			}
		}
		else
		{
			int j = 0;
			for (int count2 = base.Count; j < count2; j++)
			{
				PdfPageLayer pdfPageLayer = this[j];
				if (pdfPageLayer.m_graphicsState != null && flag && isOptimizeContent)
				{
					pdfPageLayer.Graphics.Restore(pdfPageLayer.m_graphicsState);
					pdfPageLayer.m_graphicsState = null;
				}
				PdfStream pdfStream2 = ((IPdfWrapper)pdfPageLayer).Element as PdfStream;
				byte[] array3 = null;
				array3 = ((!flag) ? pdfStream2.Data : pdfStream2.GetDecompressedData());
				if (array3 != null)
				{
					stream.Write(array3, 0, array3.Length);
				}
				stream.Write(array2, 0, array2.Length);
			}
		}
		stream.Write(array, 0, array.Length);
		stream.Write(array2, 0, array2.Length);
	}

	private void CombineProcess(PdfLoadedPage lPage, bool decompress, Stream stream, byte[] endl)
	{
		for (int i = 0; i < lPage.Contents.Count; i++)
		{
			PdfStream pdfStream = null;
			IPdfPrimitive pdfPrimitive = lPage.Contents[i];
			if (pdfPrimitive is PdfReferenceHolder)
			{
				pdfStream = (lPage.Contents[i] as PdfReferenceHolder).Object as PdfStream;
			}
			else if (pdfPrimitive is PdfStream)
			{
				pdfStream = pdfPrimitive as PdfStream;
			}
			byte[] array = null;
			if (pdfStream != null)
			{
				array = ((!decompress) ? pdfStream.Data : pdfStream.GetDecompressedData());
				if (array != null)
				{
					stream.Write(array, 0, array.Length);
				}
				stream.Write(endl, 0, endl.Length);
			}
		}
	}

	private void AddLayer(int index, PdfPageLayer layer)
	{
		if (layer == null)
		{
			throw new ArgumentNullException("layer");
		}
		if (m_page != null && !m_page.m_parseLayerGraphics)
		{
			AddSaveRestoreState();
		}
		PdfReferenceHolder element = new PdfReferenceHolder(layer);
		m_page.Contents.Add(element);
	}

	private void RemoveLayer(PdfPageLayer layer)
	{
		if (layer == null)
		{
			throw new ArgumentNullException("layer not to be null");
		}
		PdfDictionary pdfDictionary = null;
		if (m_page == null)
		{
			return;
		}
		RemoveLayerContent(layer);
		if (PdfCrossTable.Dereference(m_page.Dictionary["Resources"]) is PdfDictionary pdfDictionary2 && PdfCrossTable.Dereference(pdfDictionary2["Properties"]) is PdfDictionary pdfDictionary3 && layer.LayerId != null && pdfDictionary3.ContainsKey(layer.LayerId.TrimStart('/')))
		{
			pdfDictionary3.Remove(layer.LayerId.TrimStart('/'));
		}
		PdfPage pdfPage = m_page as PdfPage;
		PdfLoadedPage pdfLoadedPage = m_page as PdfLoadedPage;
		if (pdfPage != null)
		{
			RemoveLayerReference(pdfPage, layer);
			PdfDocumentBase pdfDocumentBase = pdfPage.Document;
			if (pdfDocumentBase == null)
			{
				pdfDocumentBase = pdfPage.Section.ParentDocument;
			}
			if (pdfDocumentBase != null && pdfDocumentBase.Catalog.ContainsKey("OCProperties"))
			{
				pdfDictionary = PdfCrossTable.Dereference(pdfDocumentBase.Catalog["OCProperties"]) as PdfDictionary;
			}
		}
		else if (pdfLoadedPage != null)
		{
			RemoveLayerReference(pdfLoadedPage, layer, isloaded: true);
			if (pdfLoadedPage.Document != null && pdfLoadedPage.Document.Catalog.ContainsKey("OCProperties"))
			{
				pdfDictionary = PdfCrossTable.Dereference(pdfLoadedPage.Document.Catalog["OCProperties"]) as PdfDictionary;
			}
		}
		if (pdfDictionary == null)
		{
			return;
		}
		if (PdfCrossTable.Dereference(pdfDictionary["OCGs"]) is PdfArray pdfArray && pdfArray.Contains(layer.ReferenceHolder))
		{
			pdfArray.Remove(layer.ReferenceHolder);
		}
		if (!(PdfCrossTable.Dereference(pdfDictionary["D"]) is PdfDictionary pdfDictionary4))
		{
			return;
		}
		PdfArray pdfArray2 = PdfCrossTable.Dereference(pdfDictionary4["Order"]) as PdfArray;
		PdfArray pdfArray3 = PdfCrossTable.Dereference(pdfDictionary4["OFF"]) as PdfArray;
		PdfArray pdfArray4 = PdfCrossTable.Dereference(pdfDictionary4["ON"]) as PdfArray;
		if (PdfCrossTable.Dereference(pdfDictionary4["AS"]) is PdfArray { Count: >0 } pdfArray5)
		{
			for (int i = 0; i < pdfArray5.Count; i++)
			{
				if (PdfCrossTable.Dereference(pdfArray5[i]) is PdfDictionary pdfDictionary5 && pdfDictionary5.ContainsKey("OCGs") && pdfDictionary5["OCGs"] is PdfArray pdfArray6 && pdfArray6.Contains(layer.ReferenceHolder))
				{
					pdfArray6.Remove(layer.ReferenceHolder);
				}
			}
		}
		if (pdfArray2 != null && pdfArray2.Contains(layer.ReferenceHolder))
		{
			pdfArray2.Remove(layer.ReferenceHolder);
		}
		if (layer.Visible && pdfArray4 != null && pdfArray4.Contains(layer.ReferenceHolder))
		{
			pdfArray4.Remove(layer.ReferenceHolder);
		}
		else if (pdfArray3 != null && pdfArray3.Contains(layer.ReferenceHolder))
		{
			pdfArray3.Remove(layer.ReferenceHolder);
		}
	}

	private void RemoveLayerReference(PdfLoadedPage page, PdfPageLayer layer, bool isloaded)
	{
		if (page.Document != null && page.Document.primitive != null && layer.ReferenceHolder != null && page.Document.primitive.Contains(layer.ReferenceHolder))
		{
			page.Document.primitive.Remove(layer.ReferenceHolder);
			page.Document.m_positon--;
		}
		if (page.Document != null && page.Document.m_order != null && layer.ReferenceHolder != null && page.Document.m_order.Contains(layer.ReferenceHolder))
		{
			page.Document.m_order.Remove(layer.ReferenceHolder);
			page.Document.m_orderposition--;
		}
		if (layer.Visible && page.Document != null && page.Document.m_on != null && layer.ReferenceHolder != null && page.Document.m_on.Contains(layer.ReferenceHolder))
		{
			page.Document.m_on.Remove(layer.ReferenceHolder);
			page.Document.m_onpositon--;
		}
		else if (page.Document != null && page.Document.m_off != null && layer.ReferenceHolder != null && page.Document.m_off.Contains(layer.ReferenceHolder))
		{
			page.Document.m_off.Remove(layer.ReferenceHolder);
			page.Document.m_offpositon--;
		}
	}

	private void RemoveLayerReference(PdfPage page, PdfPageLayer layer)
	{
		PdfDocumentBase pdfDocumentBase = page.Document;
		if (pdfDocumentBase == null)
		{
			pdfDocumentBase = page.Section.ParentDocument;
		}
		if (pdfDocumentBase != null)
		{
			if (pdfDocumentBase.primitive != null && layer.ReferenceHolder != null && pdfDocumentBase.primitive.Contains(layer.ReferenceHolder))
			{
				pdfDocumentBase.primitive.Remove(layer.ReferenceHolder);
				pdfDocumentBase.m_positon--;
			}
			if (pdfDocumentBase.m_order != null && layer.ReferenceHolder != null && pdfDocumentBase.m_order.Contains(layer.ReferenceHolder))
			{
				pdfDocumentBase.m_order.Remove(layer.ReferenceHolder);
				pdfDocumentBase.m_orderposition--;
			}
			if (layer.Visible && pdfDocumentBase.m_on != null && layer.ReferenceHolder != null && pdfDocumentBase.m_on.Contains(layer.ReferenceHolder))
			{
				pdfDocumentBase.m_on.Remove(layer.ReferenceHolder);
				pdfDocumentBase.m_onpositon--;
			}
			else if (pdfDocumentBase.m_off != null && layer.ReferenceHolder != null && pdfDocumentBase.m_off.Contains(layer.ReferenceHolder))
			{
				pdfDocumentBase.m_off.Remove(layer.ReferenceHolder);
				pdfDocumentBase.m_offpositon--;
			}
		}
	}

	private void InsertLayer(int index, PdfPageLayer layer)
	{
		if (layer == null)
		{
			throw new ArgumentNullException("layer");
		}
		PdfReferenceHolder element = new PdfReferenceHolder(layer);
		if (layer.ReferenceHolder != null && layer.LayerId != null)
		{
			if (m_page.Contents.Contains(element))
			{
				int index2 = m_page.Contents.IndexOf(element);
				m_page.Contents.RemoveAt(index2);
				m_page.Contents.Insert(index, element);
			}
		}
		else
		{
			m_page.Contents.Insert(index, element);
		}
		if (m_page is PdfPage pdfPage)
		{
			PdfDocumentBase pdfDocumentBase = pdfPage.Document;
			if (pdfDocumentBase == null)
			{
				pdfDocumentBase = pdfPage.Section.ParentDocument;
			}
			if (pdfDocumentBase != null)
			{
				if (pdfDocumentBase.primitive != null && layer.ReferenceHolder != null && pdfDocumentBase.primitive.Contains(layer.ReferenceHolder))
				{
					int index3 = pdfDocumentBase.primitive.IndexOf(layer.ReferenceHolder);
					pdfDocumentBase.primitive.RemoveAt(index3);
					pdfDocumentBase.primitive.Insert(index, layer.ReferenceHolder);
				}
				if (pdfDocumentBase.m_order != null && layer.ReferenceHolder != null && pdfDocumentBase.m_order.Contains(layer.ReferenceHolder))
				{
					int index4 = pdfDocumentBase.m_order.IndexOf(layer.ReferenceHolder);
					pdfDocumentBase.m_order.RemoveAt(index4);
					pdfDocumentBase.m_order.Insert(index, layer.ReferenceHolder);
				}
			}
		}
		else if (m_page is PdfLoadedPage { Document: not null } pdfLoadedPage && pdfLoadedPage.Document.Catalog.ContainsKey("OCProperties") && PdfCrossTable.Dereference(pdfLoadedPage.Document.Catalog["OCProperties"]) is PdfDictionary pdfDictionary)
		{
			if (PdfCrossTable.Dereference(pdfDictionary["OCGs"]) is PdfArray pdfArray && pdfArray.Contains(layer.ReferenceHolder))
			{
				pdfArray.Remove(layer.ReferenceHolder);
				pdfArray.Insert(index, layer.ReferenceHolder);
			}
			if (PdfCrossTable.Dereference(pdfDictionary["D"]) is PdfDictionary pdfDictionary2 && PdfCrossTable.Dereference(pdfDictionary2["Order"]) is PdfArray pdfArray2 && pdfArray2.Contains(layer.ReferenceHolder))
			{
				pdfArray2.Remove(layer.ReferenceHolder);
				pdfArray2.Insert(index, layer.ReferenceHolder);
			}
		}
	}

	private void ParseLayers(PdfPageBase loadedPage)
	{
		if (loadedPage == null)
		{
			throw new ArgumentNullException("loadedPage");
		}
		if (m_page == null)
		{
			return;
		}
		PdfArray pdfArray = null;
		if (m_parseLayer)
		{
			if (m_page.Dictionary.ContainsKey("Contents"))
			{
				pdfArray = PdfCrossTable.Dereference(m_page.Dictionary["Contents"]) as PdfArray;
				if (pdfArray == null && PdfCrossTable.Dereference(m_page.Dictionary["Contents"]) is PdfStream obj)
				{
					pdfArray = new PdfArray();
					pdfArray.Add(new PdfReferenceHolder(obj));
				}
			}
			if (pdfArray == null)
			{
				return;
			}
		}
		else
		{
			pdfArray = m_page.Contents;
		}
		PdfDictionary pdfDictionary = null;
		lock (m_resourceLock)
		{
			pdfDictionary = ((m_page.Dictionary == null || !m_page.Dictionary.ContainsKey("Resources")) ? m_page.GetResources() : (PdfCrossTable.Dereference(m_page.Dictionary["Resources"]) as PdfDictionary));
			if (pdfDictionary == null)
			{
				pdfDictionary = m_page.GetResources();
			}
		}
		PdfDictionary pdfDictionary2 = null;
		PdfDictionary pdfDictionary3 = null;
		Dictionary<PdfReferenceHolder, PdfPageLayer> dictionary = new Dictionary<PdfReferenceHolder, PdfPageLayer>();
		PdfLoadedPage pdfLoadedPage = loadedPage as PdfLoadedPage;
		if (pdfLoadedPage != null)
		{
			pdfDictionary3 = PdfCrossTable.Dereference(pdfDictionary["Properties"]) as PdfDictionary;
			if (pdfLoadedPage.Document != null)
			{
				pdfDictionary2 = PdfCrossTable.Dereference(pdfLoadedPage.Document.Catalog["OCProperties"]) as PdfDictionary;
				documentLayers = pdfLoadedPage.Document.documentLayerCollection;
			}
		}
		if (pdfDictionary2 != null && pdfDictionary2.ContainsKey("OCGs") && documentLayers == null)
		{
			documentLayers = new Dictionary<PdfReferenceHolder, PdfDictionary>();
			if (PdfCrossTable.Dereference(pdfDictionary2["OCGs"]) is PdfArray { Count: >0 } pdfArray2)
			{
				for (int i = 0; i < pdfArray2.Count; i++)
				{
					PdfReferenceHolder pdfReferenceHolder = pdfArray2[i] as PdfReferenceHolder;
					if (pdfReferenceHolder != null && pdfReferenceHolder.Object is PdfDictionary value && !documentLayers.ContainsKey(pdfReferenceHolder))
					{
						documentLayers[pdfReferenceHolder] = value;
					}
				}
				if (pdfLoadedPage != null && pdfLoadedPage.Document != null)
				{
					pdfLoadedPage.Document.documentLayerCollection = documentLayers;
				}
			}
		}
		bool isPropertieLayer = false;
		bool isResourceLayer = false;
		PageContainsLayer(pdfDictionary3, pdfDictionary, out isPropertieLayer, out isResourceLayer);
		if (pdfDictionary2 != null && (isPropertieLayer || isResourceLayer))
		{
			PdfDictionary pdfDictionary4 = null;
			PdfReferenceHolder pdfReferenceHolder2 = null;
			if (pdfDictionary3 != null && pdfDictionary3.Items != null && isPropertieLayer)
			{
				foreach (KeyValuePair<PdfName, IPdfPrimitive> item in pdfDictionary3.Items)
				{
					string value2 = item.Key.Value;
					pdfReferenceHolder2 = item.Value as PdfReferenceHolder;
					pdfDictionary4 = PdfCrossTable.Dereference(item.Value) as PdfDictionary;
					if (pdfDictionary4 == null || !(pdfReferenceHolder2 != null) || documentLayers == null || m_layerCollection.Contains(value2) || (!documentLayers.ContainsKey(pdfReferenceHolder2) && !pdfDictionary4.ContainsKey("OCGs")))
					{
						continue;
					}
					if (pdfDictionary4.ContainsKey("OCGs"))
					{
						if (!(PdfCrossTable.Dereference(pdfDictionary4["OCGs"]) is PdfArray pdfArray3))
						{
							pdfDictionary4 = PdfCrossTable.Dereference(pdfDictionary4["OCGs"]) as PdfDictionary;
							if (pdfDictionary4 != null)
							{
								pdfReferenceHolder2 = pdfDictionary4["OCGs"] as PdfReferenceHolder;
								if (pdfReferenceHolder2 != null && documentLayers.ContainsKey(pdfReferenceHolder2))
								{
									AddLayer(loadedPage, pdfDictionary4, pdfReferenceHolder2, value2, dictionary, isResourceLayer: false);
								}
							}
							continue;
						}
						for (int j = 0; j < pdfArray3.Count; j++)
						{
							if (!(pdfArray3[j] is PdfReferenceHolder))
							{
								continue;
							}
							pdfReferenceHolder2 = pdfArray3[j] as PdfReferenceHolder;
							if (pdfReferenceHolder2 != null)
							{
								pdfDictionary4 = (pdfArray3[j] as PdfReferenceHolder).Object as PdfDictionary;
								if (pdfDictionary4 != null && documentLayers.ContainsKey(pdfReferenceHolder2))
								{
									AddLayer(loadedPage, pdfDictionary4, pdfReferenceHolder2, value2, dictionary, isResourceLayer: false);
								}
							}
						}
					}
					else
					{
						AddLayer(loadedPage, pdfDictionary4, pdfReferenceHolder2, value2, dictionary, isResourceLayer: false);
					}
				}
			}
			if (isResourceLayer)
			{
				ParseResourceLayer(pdfDictionary, pdfDictionary4, pdfReferenceHolder2, loadedPage, dictionary, documentLayers);
			}
		}
		if (pdfDictionary2 != null && dictionary.Count > 0)
		{
			CheckVisible(pdfDictionary2, dictionary);
			SortLayerList(pdfDictionary2, dictionary);
			isLayerPresent = true;
		}
		if (loadedPage.Imported && pdfArray.Count > 0)
		{
			for (int k = 0; k < pdfArray.Count; k++)
			{
				IPdfPrimitive pdfPrimitive = pdfArray[k];
				if (pdfPrimitive != null && PdfCrossTable.Dereference(pdfPrimitive) is PdfStream pdfStream)
				{
					pdfStream.Decompress();
				}
			}
		}
		if (!m_parseLayer || loadedPage.Imported)
		{
			AddSaveRestoreState();
		}
	}

	private void AddSaveRestoreState()
	{
		PdfArray contents = m_page.Contents;
		PdfStream pdfStream = new PdfStream();
		PdfStream pdfStream2 = new PdfStream();
		byte b = 113;
		byte b2 = 10;
		byte b3 = 81;
		byte b4 = 32;
		m_page.m_parseLayerGraphics = true;
		byte[] array = new byte[4] { b4, b, b4, b2 };
		pdfStream.Data = array;
		contents.Insert(0, new PdfReferenceHolder(pdfStream));
		array[0] = b4;
		array[1] = b3;
		array[2] = b4;
		array[3] = b2;
		pdfStream2.Data = array;
		contents.Insert(contents.Count, new PdfReferenceHolder(pdfStream2));
	}

	private void CheckVisible(PdfDictionary ocproperties, Dictionary<PdfReferenceHolder, PdfPageLayer> m_layerDictionary)
	{
		if (ocproperties == null || !(PdfCrossTable.Dereference(ocproperties["D"]) is PdfDictionary pdfDictionary) || !(PdfCrossTable.Dereference(pdfDictionary["OFF"]) is PdfArray pdfArray) || m_layerDictionary.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < pdfArray.Count; i++)
		{
			if (!m_layerDictionary.ContainsKey(pdfArray[i] as PdfReferenceHolder))
			{
				continue;
			}
			PdfPageLayer pdfPageLayer = m_layerDictionary[pdfArray[i] as PdfReferenceHolder];
			if (pdfPageLayer != null)
			{
				pdfPageLayer.m_visible = false;
				if (pdfPageLayer.Dictionary != null && pdfPageLayer.Dictionary.ContainsKey("Visible"))
				{
					pdfPageLayer.Dictionary.SetProperty("Visible", new PdfBoolean(value: false));
				}
			}
		}
	}

	private void SortLayerList(PdfDictionary ocPropertie, Dictionary<PdfReferenceHolder, PdfPageLayer> layerCollection)
	{
		if (ocPropertie == null || !(PdfCrossTable.Dereference(ocPropertie["OCGs"]) is PdfArray pdfArray))
		{
			return;
		}
		int count = base.List.Count;
		base.List.Clear();
		for (int i = 0; i < pdfArray.Count; i++)
		{
			if (count > base.List.Count && layerCollection.ContainsKey(pdfArray[i] as PdfReferenceHolder))
			{
				base.List.Add(layerCollection[pdfArray[i] as PdfReferenceHolder]);
			}
		}
	}

	private void PageContainsLayer(PdfDictionary propertie, PdfDictionary resource, out bool isPropertieLayer, out bool isResourceLayer)
	{
		isPropertieLayer = false;
		isResourceLayer = false;
		if (propertie != null)
		{
			isPropertieLayer = true;
		}
		if (resource == null || !resource.ContainsKey("XObject") || !(PdfCrossTable.Dereference(resource["XObject"]) is PdfDictionary pdfDictionary))
		{
			return;
		}
		foreach (KeyValuePair<PdfName, IPdfPrimitive> item in pdfDictionary.Items)
		{
			if (item.Value is PdfReferenceHolder && PdfCrossTable.Dereference(item.Value) is PdfDictionary pdfDictionary2 && pdfDictionary2.ContainsKey("OC"))
			{
				isResourceLayer = true;
				break;
			}
		}
	}

	private void AddLayer(PdfPageBase page, PdfDictionary dictionary, PdfReferenceHolder reference, string key, Dictionary<PdfReferenceHolder, PdfPageLayer> pageLayerCollection, bool isResourceLayer)
	{
		PdfPageLayer pdfPageLayer = new PdfPageLayer(page);
		pdfPageLayer.isResourceLayer = isResourceLayer;
		if (PdfCrossTable.Dereference(dictionary["Usage"]) is PdfDictionary pdfDictionary)
		{
			PdfDictionary pdfDictionary2 = PdfCrossTable.Dereference(pdfDictionary["Print"]) as PdfDictionary;
			PdfDictionary pdfDictionary3 = PdfCrossTable.Dereference(pdfDictionary["View"]) as PdfDictionary;
			if (pdfDictionary2 != null)
			{
				pdfPageLayer.m_printOption = pdfDictionary2;
				foreach (KeyValuePair<PdfName, IPdfPrimitive> item in pdfDictionary2.Items)
				{
					if (item.Key.Value.Equals("PrintState"))
					{
						PdfName pdfName = item.Value as PdfName;
						if (pdfName != null && pdfName.Value.Equals("ON"))
						{
							pdfPageLayer.PrintState = PdfPrintState.AlwaysPrint;
						}
						else
						{
							pdfPageLayer.PrintState = PdfPrintState.NeverPrint;
						}
						break;
					}
				}
			}
			if (pdfDictionary3 != null)
			{
				foreach (KeyValuePair<PdfName, IPdfPrimitive> item2 in pdfDictionary3.Items)
				{
					if (item2.Key.Value.Equals("ViewState"))
					{
						PdfName pdfName2 = item2.Value as PdfName;
						if (pdfName2 != null && pdfName2.Value.Equals("OFF"))
						{
							pdfPageLayer.m_visible = false;
							break;
						}
					}
				}
			}
		}
		base.List.Add(pdfPageLayer);
		if (!pageLayerCollection.ContainsKey(reference))
		{
			pageLayerCollection.Add(reference, pdfPageLayer);
		}
		pdfPageLayer.Dictionary = dictionary;
		pdfPageLayer.ReferenceHolder = reference;
		m_layerCollection.Add(key.TrimStart('/'));
		pdfPageLayer.LayerId = key.TrimStart('/');
		if (dictionary.ContainsKey("Name") && PdfCrossTable.Dereference(dictionary["Name"]) is PdfString pdfString)
		{
			pdfPageLayer.Name = pdfString.Value;
		}
	}

	private void ParseResourceLayer(PdfDictionary resource, PdfDictionary layerDictionary, PdfReferenceHolder layerReference, PdfPageBase loadedPage, Dictionary<PdfReferenceHolder, PdfPageLayer> pageLayerCollection, Dictionary<PdfReferenceHolder, PdfDictionary> documentLayers)
	{
		if (!resource.ContainsKey("XObject") || !(PdfCrossTable.Dereference(resource["XObject"]) is PdfDictionary pdfDictionary))
		{
			return;
		}
		foreach (KeyValuePair<PdfName, IPdfPrimitive> item in pdfDictionary.Items)
		{
			if (!(item.Value is PdfReferenceHolder))
			{
				continue;
			}
			string value = item.Key.Value;
			if (!((item.Value as PdfReferenceHolder).Object is PdfDictionary pdfDictionary2) || !pdfDictionary2.ContainsKey("OC"))
			{
				continue;
			}
			layerDictionary = PdfCrossTable.Dereference(pdfDictionary2["OC"]) as PdfDictionary;
			if (layerDictionary == null || documentLayers == null || (!layerDictionary.ContainsKey("Name") && !layerDictionary.ContainsKey("OCGs")))
			{
				continue;
			}
			if (!(PdfCrossTable.Dereference(layerDictionary["OCGs"]) is PdfArray pdfArray))
			{
				if (PdfCrossTable.Dereference(layerDictionary["OCGs"]) is PdfDictionary pdfDictionary3)
				{
					layerReference = layerDictionary["OCGs"] as PdfReferenceHolder;
					layerDictionary = pdfDictionary3;
					if (layerReference != null && documentLayers.ContainsKey(layerReference))
					{
						AddLayer(loadedPage, layerDictionary, layerReference, value, pageLayerCollection, isResourceLayer: true);
					}
				}
				else
				{
					layerDictionary = PdfCrossTable.Dereference(pdfDictionary2["OC"]) as PdfDictionary;
					layerReference = pdfDictionary2["OC"] as PdfReferenceHolder;
					if (layerReference != null && documentLayers.ContainsKey(layerReference))
					{
						AddLayer(loadedPage, layerDictionary, layerReference, value, pageLayerCollection, isResourceLayer: true);
					}
				}
				continue;
			}
			for (int i = 0; i < pdfArray.Count; i++)
			{
				layerDictionary = PdfCrossTable.Dereference(pdfArray[i]) as PdfDictionary;
				layerReference = pdfArray[i] as PdfReferenceHolder;
				if (layerReference != null && documentLayers.ContainsKey(layerReference))
				{
					AddLayer(loadedPage, layerDictionary, layerReference, value, pageLayerCollection, isResourceLayer: true);
				}
			}
		}
	}

	private void RemoveLayerContent(PdfPageLayer layer)
	{
		bool isSkip = false;
		PdfArray pdfArray = new PdfArray();
		PdfArray pdfArray2 = new PdfArray();
		List<PdfStream> list = new List<PdfStream>();
		for (int i = 0; i < m_page.Contents.Count; i++)
		{
			PdfReferenceHolder pdfReferenceHolder = m_page.Contents[i] as PdfReferenceHolder;
			if (pdfReferenceHolder != null && pdfReferenceHolder.Reference == null)
			{
				pdfArray.Add(m_page.Contents[i]);
			}
			else
			{
				pdfArray2.Add(m_page.Contents[i]);
			}
		}
		ContentParser contentParser = null;
		MemoryStream memoryStream = new MemoryStream();
		byte[] array = PdfString.StringToByte("\r\n");
		for (int j = 0; j < pdfArray2.Count; j++)
		{
			PdfStream obj = PdfCrossTable.Dereference(pdfArray2[j]) as PdfStream;
			obj.Decompress();
			obj.InternalStream.WriteTo(memoryStream);
			memoryStream.Write(array, 0, array.Length);
			contentParser = new ContentParser(memoryStream.ToArray());
		}
		PdfStream data = new PdfStream();
		if (contentParser != null)
		{
			data = FindLayersContent(layer, contentParser, data, isSkip);
			list.Add(data);
		}
		PdfStream pdfStream = null;
		if (layer.m_graphics != null && layer.m_graphics.StreamWriter != null)
		{
			pdfStream = layer.m_graphics.StreamWriter.GetStream();
		}
		for (int k = 0; k < pdfArray.Count; k++)
		{
			contentParser = null;
			memoryStream = new MemoryStream();
			PdfStream pdfStream2 = PdfCrossTable.Dereference(pdfArray[k]) as PdfStream;
			if (pdfStream2 != pdfStream)
			{
				if (m_page is PdfLoadedPage)
				{
					pdfStream2.Decompress();
				}
				pdfStream2.InternalStream.WriteTo(memoryStream);
				contentParser = new ContentParser(memoryStream.ToArray());
				PdfStream data2 = new PdfStream();
				data2 = FindLayersContent(layer, contentParser, data2, isSkip);
				if (pdfStream2.Data.Length != 0 && data2.Data.Length != 0)
				{
					pdfStream2.Clear();
					pdfStream2.Write(data2.Data);
				}
				list.Add(pdfStream2);
			}
			memoryStream.Dispose();
		}
		m_page.Contents.Clear();
		for (int l = 0; l < list.Count; l++)
		{
			m_page.Contents.Add(new PdfReferenceHolder(list[l]));
		}
		list.Clear();
		list = null;
		memoryStream?.Dispose();
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

	private void ProcessBeginMarkContent(PdfPageLayer parser, string mOperator, string[] operands, PdfStream data)
	{
		if ("BDC".Equals(mOperator.ToString()))
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
		StreamWrite(operands, mOperator, skip: true, data);
		if ("EMC".Equals(mOperator.ToString()) && m_bdcCount > 0)
		{
			m_bdcCount--;
		}
	}

	private PdfStream FindLayersContent(PdfPageLayer layer, ContentParser parser, PdfStream data, bool isSkip)
	{
		PdfRecordCollection pdfRecordCollection = parser.ReadContent();
		for (int i = 0; i < pdfRecordCollection.RecordCollection.Count; i++)
		{
			string operatorName = pdfRecordCollection.RecordCollection[i].OperatorName;
			switch (operatorName)
			{
			case "BMC":
			case "EMC":
			case "BDC":
				ProcessBeginMarkContent(layer, operatorName, pdfRecordCollection.RecordCollection[i].Operands, data);
				isSkip = true;
				break;
			}
			if (operatorName == "Do" && pdfRecordCollection.RecordCollection[i].Operands[0].TrimStart('/').Equals(layer.LayerId))
			{
				isSkip = true;
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
				if (!isSkip)
				{
					StreamWrite(pdfRecordCollection.RecordCollection[i].Operands, operatorName, skip: false, data);
				}
				break;
			default:
				if (!isSkip)
				{
					StreamWrite(pdfRecordCollection.RecordCollection[i].Operands, operatorName, skip: true, data);
				}
				break;
			}
			isSkip = false;
		}
		data.Compress = true;
		return data;
	}
}
