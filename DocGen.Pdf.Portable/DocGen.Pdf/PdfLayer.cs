using System;
using System.Collections.Generic;
using System.Text;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.IO;
using DocGen.Pdf.Parsing;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf;

public class PdfLayer : IPdfWrapper
{
	private PdfPageBase m_page;

	private PdfGraphics m_graphics;

	internal PdfStream m_content;

	private PdfGraphicsState m_graphicsState;

	private bool m_clipPageTemplates;

	private bool m_bSaved;

	private PdfColorSpace m_colorspace;

	private string m_layerid;

	private string m_name;

	internal bool m_visible = true;

	internal PdfDictionary m_printOption;

	internal PdfDictionary m_usage;

	private PdfPrintState printState;

	internal bool m_isEndState;

	private PdfDictionary m_dictionary = new PdfDictionary();

	private PdfReferenceHolder m_refholder;

	private PdfLayer layer;

	private PdfDocumentBase document;

	internal List<PdfPageBase> pages = new List<PdfPageBase>();

	private PdfDocumentLayerCollection m_layer;

	private bool m_sublayer;

	internal int m_sublayerposition;

	internal PdfArray sublayer = new PdfArray();

	internal bool m_locked;

	internal PdfArray m_lock;

	internal List<PdfLayer> m_parentLayer = new List<PdfLayer>();

	internal List<PdfLayer> m_child = new List<PdfLayer>();

	internal PdfLayer parent;

	private Dictionary<PdfGraphics, PdfGraphics> graphics = new Dictionary<PdfGraphics, PdfGraphics>();

	private Dictionary<PdfPageBase, PdfGraphics> pageGraphics = new Dictionary<PdfPageBase, PdfGraphics>();

	private bool m_pagePasrsed;

	private bool m_contentParsed;

	internal List<string> xobject = new List<string>();

	internal PdfReferenceHolder ReferenceHolder
	{
		get
		{
			return m_refholder;
		}
		set
		{
			m_refholder = value;
		}
	}

	internal PdfDictionary Dictionary
	{
		get
		{
			return m_dictionary;
		}
		set
		{
			m_dictionary = value;
		}
	}

	internal PdfColorSpace Colorspace
	{
		get
		{
			return m_colorspace;
		}
		set
		{
			m_colorspace = value;
		}
	}

	internal PdfPageBase Page
	{
		get
		{
			if (!m_pagePasrsed)
			{
				ParsingLayerPage();
			}
			return m_page;
		}
		set
		{
			m_page = value;
		}
	}

	internal PdfDocumentBase Document
	{
		get
		{
			return document;
		}
		set
		{
			document = value;
		}
	}

	internal string LayerId
	{
		get
		{
			if (!m_pagePasrsed)
			{
				ParsingLayerPage();
			}
			return m_layerid;
		}
		set
		{
			m_layerid = value;
		}
	}

	internal PdfLayer Layer
	{
		get
		{
			return layer;
		}
		set
		{
			layer = value;
		}
	}

	public string Name
	{
		get
		{
			return m_name;
		}
		set
		{
			m_name = value;
			if (Dictionary != null && m_name != null && Name != string.Empty)
			{
				Dictionary.SetProperty("Name", new PdfString(m_name));
			}
		}
	}

	public bool Visible
	{
		get
		{
			if (Dictionary != null && Dictionary.ContainsKey("Visible"))
			{
				m_visible = (Dictionary["Visible"] as PdfBoolean).Value;
			}
			return m_visible;
		}
		set
		{
			m_visible = value;
			if (Dictionary != null)
			{
				Dictionary.SetProperty("Visible", new PdfBoolean(value));
			}
			SetVisibility(value);
		}
	}

	private PdfGraphics Graphics
	{
		get
		{
			if (m_graphics == null || m_bSaved)
			{
				CreateGraphics(Page);
			}
			return m_graphics;
		}
	}

	public PdfPrintState PrintState
	{
		get
		{
			return printState;
		}
		set
		{
			printState = value;
			if (m_printOption != null)
			{
				if (printState.Equals(PdfPrintState.AlwaysPrint))
				{
					m_printOption.SetProperty("PrintState", new PdfName("ON"));
				}
				else if (PrintState.Equals(PdfPrintState.NeverPrint))
				{
					m_printOption.SetProperty("PrintState", new PdfName("OFF"));
				}
			}
			else
			{
				SetPrintState();
			}
		}
	}

	public PdfDocumentLayerCollection Layers
	{
		get
		{
			if (m_layer == null)
			{
				m_layer = new PdfDocumentLayerCollection(document, layer);
				m_layer.m_sublayer = true;
			}
			return m_layer;
		}
	}

	public bool Locked
	{
		get
		{
			return m_locked;
		}
		set
		{
			m_locked = value;
			SetLock(value);
		}
	}

	internal PdfStream ContentStream
	{
		get
		{
			if (!m_contentParsed)
			{
				ParseContent();
			}
			return m_content;
		}
	}

	IPdfPrimitive IPdfWrapper.Element => m_content;

	internal PdfLayer()
	{
		m_clipPageTemplates = true;
		m_content = new PdfStream();
	}

	public PdfGraphics CreateGraphics(PdfPageBase page)
	{
		PdfPage pdfPage = page as PdfPage;
		m_page = page;
		if (m_graphics == null)
		{
			PdfReferenceHolder element = new PdfReferenceHolder(layer);
			page.Contents.Add(element);
		}
		PdfResources resources = page.GetResources();
		if (string.IsNullOrEmpty(layer.LayerId))
		{
			layer.LayerId = "OCG_" + Guid.NewGuid();
		}
		if (resources != null && resources.ContainsKey("Properties"))
		{
			if (resources["Properties"] is PdfDictionary pdfDictionary)
			{
				pdfDictionary[layer.LayerId.TrimStart('/')] = layer.ReferenceHolder;
			}
			else
			{
				resources.AddProperties(layer.LayerId.TrimStart('/'), layer.ReferenceHolder);
			}
		}
		else
		{
			resources.AddProperties(layer.LayerId.TrimStart('/'), layer.ReferenceHolder);
		}
		if (m_graphics == null)
		{
			PdfGraphics.GetResources resources2 = Page.GetResources;
			bool flag = false;
			if (page.Dictionary.ContainsKey("MediaBox"))
			{
				flag = true;
			}
			PdfArray pdfArray = page.Dictionary.GetValue("MediaBox", "Parent") as PdfArray;
			float num = 0f;
			float num2 = 0f;
			float num3 = 0f;
			float num4 = 0f;
			if (pdfArray != null)
			{
				num = (pdfArray[0] as PdfNumber).FloatValue;
				num2 = (pdfArray[1] as PdfNumber).FloatValue;
				num3 = (pdfArray[2] as PdfNumber).FloatValue;
				num4 = (pdfArray[3] as PdfNumber).FloatValue;
			}
			PdfArray pdfArray2 = null;
			if (page.Dictionary.ContainsKey("CropBox"))
			{
				pdfArray2 = page.Dictionary.GetValue("CropBox", "Parent") as PdfArray;
				float floatValue = (pdfArray2[0] as PdfNumber).FloatValue;
				float floatValue2 = (pdfArray2[1] as PdfNumber).FloatValue;
				float floatValue3 = (pdfArray2[2] as PdfNumber).FloatValue;
				float floatValue4 = (pdfArray2[3] as PdfNumber).FloatValue;
				if ((floatValue < 0f || floatValue2 < 0f || floatValue3 < 0f || floatValue4 < 0f) && Math.Floor(Math.Abs(floatValue2)) == Math.Floor(Math.Abs(page.Size.Height)) && Math.Floor(Math.Abs(floatValue)) == Math.Floor(Math.Abs(page.Size.Width)))
				{
					RectangleF rectangleF = new RectangleF(Math.Min(floatValue, floatValue3), Math.Min(floatValue2, floatValue4), Math.Max(floatValue, floatValue3), Math.Max(floatValue2, floatValue4));
					m_graphics = new PdfGraphics(new SizeF(rectangleF.Width, rectangleF.Height), resources2, m_content);
				}
				else
				{
					m_graphics = new PdfGraphics(page.Size, resources2, m_content);
					m_graphics.m_cropBox = pdfArray2;
				}
			}
			else if ((num < 0f || num2 < 0f || num3 < 0f || num4 < 0f) && Math.Floor(Math.Abs(num2)) == Math.Floor(Math.Abs(page.Size.Height)) && Math.Floor(Math.Abs(num3)) == Math.Floor(page.Size.Width))
			{
				RectangleF rectangleF2 = new RectangleF(Math.Min(num, num3), Math.Min(num2, num4), Math.Max(num, num3), Math.Max(num2, num4));
				m_graphics = new PdfGraphics(new SizeF(rectangleF2.Width, rectangleF2.Height), resources2, m_content);
			}
			else
			{
				m_graphics = new PdfGraphics(page.Size, resources2, m_content);
			}
			if (flag)
			{
				m_graphics.MediaBoxUpperRightBound = num4;
			}
			if (pdfPage != null)
			{
				PdfSectionCollection pdfSectionCollection = pdfPage.Section.Parent;
				if (pdfSectionCollection != null)
				{
					m_graphics.ColorSpace = pdfSectionCollection.Document.ColorSpace;
					Colorspace = pdfSectionCollection.Document.ColorSpace;
				}
			}
			if (!graphics.ContainsKey(m_graphics))
			{
				graphics[m_graphics] = m_graphics;
			}
			if (!pageGraphics.ContainsKey(page))
			{
				pageGraphics[page] = m_graphics;
			}
			m_content.BeginSave += BeginSaveContent;
		}
		else if (!pages.Contains(page))
		{
			GraphicsContent(page);
		}
		else if (page is PdfLoadedPage && !graphics.ContainsKey(m_graphics))
		{
			GraphicsContent(page);
		}
		else if (pageGraphics.ContainsKey(page))
		{
			m_graphics = pageGraphics[page];
			return m_graphics;
		}
		PdfArray contents = page.Contents;
		PdfStream pdfStream = new PdfStream();
		PdfStream pdfStream2 = new PdfStream();
		byte b = 113;
		byte b2 = 10;
		byte b3 = 81;
		byte b4 = 32;
		byte[] array2 = (pdfStream.Data = new byte[4] { b4, b, b4, b2 });
		contents.Insert(0, new PdfReferenceHolder(pdfStream));
		array2[0] = b4;
		array2[1] = b3;
		array2[2] = b4;
		array2[3] = b2;
		pdfStream2.Data = array2;
		contents.Insert(2, new PdfReferenceHolder(pdfStream2));
		m_graphics.StreamWriter.Write(Environment.NewLine);
		m_graphicsState = m_graphics.Save();
		if ((page.Origin.X >= 0f && page.Origin.Y >= 0f) || Math.Sign(page.Origin.X) != Math.Sign(page.Origin.Y))
		{
			m_graphics.InitializeCoordinates();
		}
		else
		{
			m_graphics.InitializeCoordinates(page);
		}
		if (PdfGraphics.TransparencyObject)
		{
			m_graphics.SetTransparencyGroup(page);
		}
		if ((Page is PdfLoadedPage && page.Rotation != 0) || page.Dictionary.ContainsKey("Rotate"))
		{
			PdfNumber pdfNumber = null;
			if (page.Dictionary.ContainsKey("Rotate"))
			{
				pdfNumber = page.Dictionary["Rotate"] as PdfNumber;
				if (pdfNumber == null)
				{
					pdfNumber = PdfCrossTable.Dereference(page.Dictionary["Rotate"]) as PdfNumber;
				}
			}
			else if (page.Rotation != 0)
			{
				pdfNumber = new PdfNumber(0);
				if (page.Rotation == PdfPageRotateAngle.RotateAngle90)
				{
					pdfNumber.FloatValue = 90f;
				}
				if (page.Rotation == PdfPageRotateAngle.RotateAngle180)
				{
					pdfNumber.FloatValue = 180f;
				}
				if (page.Rotation == PdfPageRotateAngle.RotateAngle270)
				{
					pdfNumber.FloatValue = 270f;
				}
			}
			if (pdfNumber.FloatValue == 90f)
			{
				m_graphics.TranslateTransform(0f, page.Size.Height);
				m_graphics.RotateTransform(-90f);
				m_graphics.m_clipBounds.Size = new SizeF(page.Size.Height, page.Size.Width);
			}
			else if (pdfNumber.FloatValue == 180f)
			{
				m_graphics.TranslateTransform(page.Size.Width, page.Size.Height);
				m_graphics.RotateTransform(-180f);
			}
			else if (pdfNumber.FloatValue == 270f)
			{
				m_graphics.TranslateTransform(page.Size.Width, 0f);
				m_graphics.RotateTransform(-270f);
				m_graphics.m_clipBounds.Size = new SizeF(page.Size.Height, page.Size.Width);
			}
		}
		if (pdfPage != null)
		{
			RectangleF actualBounds = pdfPage.Section.GetActualBounds(pdfPage, includeMargins: true);
			PdfMargins margins = pdfPage.Section.PageSettings.Margins;
			if (m_clipPageTemplates)
			{
				if (page.Origin.X >= 0f && page.Origin.Y >= 0f)
				{
					m_graphics.ClipTranslateMargins(actualBounds);
				}
			}
			else
			{
				m_graphics.ClipTranslateMargins(actualBounds.X, actualBounds.Y, margins.Left, margins.Top, margins.Right, margins.Bottom);
			}
		}
		if (!pages.Contains(page))
		{
			pages.Add(page);
		}
		m_graphics.SetLayer(this);
		m_bSaved = false;
		return m_graphics;
	}

	internal void Clear()
	{
		if (m_graphics != null)
		{
			m_graphics.StreamWriter.Clear();
		}
		if (m_content != null)
		{
			m_content = null;
		}
		if (m_graphics != null)
		{
			m_graphics = null;
		}
	}

	private void SetVisibility(bool value)
	{
		PdfDictionary catalog = document.Catalog;
		PdfDictionary pdfDictionary = null;
		if (catalog.ContainsKey("OCProperties"))
		{
			pdfDictionary = catalog["OCProperties"] as PdfDictionary;
			if (pdfDictionary == null)
			{
				pdfDictionary = (document.Catalog["OCProperties"] as PdfReferenceHolder).Object as PdfDictionary;
			}
		}
		if (pdfDictionary == null)
		{
			return;
		}
		PdfArray pdfArray = null;
		PdfArray pdfArray2 = null;
		PdfDictionary pdfDictionary2 = pdfDictionary["D"] as PdfDictionary;
		if (pdfDictionary2 == null)
		{
			pdfDictionary2 = PdfCrossTable.Dereference(Dictionary["D"]) as PdfDictionary;
		}
		if (pdfDictionary2 == null)
		{
			return;
		}
		if (pdfDictionary2.ContainsKey("ON"))
		{
			pdfArray2 = pdfDictionary2["ON"] as PdfArray;
			if (pdfArray2 == null)
			{
				pdfArray2 = (pdfDictionary2["ON"] as PdfReferenceHolder).Object as PdfArray;
			}
		}
		if (pdfDictionary2.ContainsKey("OFF"))
		{
			pdfArray = pdfDictionary2["OFF"] as PdfArray;
			if (pdfArray == null)
			{
				pdfArray = (pdfDictionary2["OFF"] as PdfReferenceHolder).Object as PdfArray;
			}
		}
		_ = m_refholder;
		if (!(m_refholder != null))
		{
			return;
		}
		if (!value)
		{
			pdfArray2?.Remove(m_refholder);
			if (pdfArray == null)
			{
				pdfArray = new PdfArray();
				pdfDictionary2.Items.Add(new PdfName("OFF"), pdfArray);
			}
			pdfArray?.Remove(m_refholder);
			pdfArray.Add(m_refholder);
		}
		else if (value)
		{
			pdfArray?.Remove(m_refholder);
			if (pdfArray2 == null)
			{
				pdfArray2 = new PdfArray();
				pdfDictionary2.Items.Add(new PdfName("ON"), pdfArray2);
			}
			pdfArray2?.Remove(m_refholder);
			pdfArray2.Add(m_refholder);
		}
	}

	private void SetLock(bool isSetLock)
	{
		PdfDictionary catalog = document.Catalog;
		PdfDictionary pdfDictionary = null;
		if (catalog.ContainsKey("OCProperties"))
		{
			pdfDictionary = catalog["OCProperties"] as PdfDictionary;
			if (pdfDictionary == null)
			{
				pdfDictionary = (document.Catalog["OCProperties"] as PdfReferenceHolder).Object as PdfDictionary;
			}
		}
		if (pdfDictionary == null)
		{
			return;
		}
		PdfDictionary pdfDictionary2 = pdfDictionary["D"] as PdfDictionary;
		if (pdfDictionary2 == null)
		{
			pdfDictionary2 = (pdfDictionary["D"] as PdfReferenceHolder).Object as PdfDictionary;
		}
		if (pdfDictionary2 == null)
		{
			return;
		}
		PdfArray pdfArray = pdfDictionary2["Locked"] as PdfArray;
		if (!(m_refholder != null))
		{
			return;
		}
		if (isSetLock)
		{
			if (pdfArray != null)
			{
				if (!pdfArray.Contains(m_refholder))
				{
					pdfArray.Add(m_refholder);
				}
			}
			else
			{
				m_lock = new PdfArray();
				m_lock.Add(m_refholder);
				pdfDictionary2.Items.Add(new PdfName("Locked"), m_lock);
			}
		}
		else if (!isSetLock)
		{
			pdfArray?.Remove(m_refholder);
		}
	}

	private void SetPrintState()
	{
		PdfDictionary catalog = document.Catalog;
		PdfDictionary pdfDictionary = null;
		PdfDictionary pdfDictionary2 = null;
		if (catalog.ContainsKey("OCProperties"))
		{
			pdfDictionary = catalog["OCProperties"] as PdfDictionary;
			if (pdfDictionary == null)
			{
				pdfDictionary = (document.Catalog["OCProperties"] as PdfReferenceHolder).Object as PdfDictionary;
			}
		}
		PdfArray pdfArray = pdfDictionary["OCGs"] as PdfArray;
		if (pdfArray == null)
		{
			pdfArray = (pdfDictionary["OCGs"] as PdfReferenceHolder).Object as PdfArray;
		}
		pdfDictionary2 = (Dictionary.ContainsKey("Usage") ? (PdfCrossTable.Dereference(Dictionary["Usage"]) as PdfDictionary) : new PdfDictionary());
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
		pdfDictionary2.SetProperty("Print", new PdfReferenceHolder(layer.m_printOption));
		layer.m_usage = pdfDictionary2;
		Dictionary.SetProperty("Usage", layer.m_usage);
		PdfArray pdfArray2 = new PdfArray();
		pdfArray2.Add(new PdfName("Print"));
		PdfDictionary pdfDictionary3 = new PdfDictionary();
		pdfDictionary3.SetProperty("Category", pdfArray2);
		pdfDictionary3.SetProperty("OCGs", pdfArray);
		pdfDictionary3.SetProperty("Event", new PdfName("Print"));
		PdfArray pdfArray3 = new PdfArray();
		pdfArray3.Add(new PdfReferenceHolder(pdfDictionary3));
		PdfDictionary pdfDictionary4 = pdfDictionary["D"] as PdfDictionary;
		if (pdfDictionary4 == null)
		{
			pdfDictionary4 = (pdfDictionary["D"] as PdfReferenceHolder).Object as PdfDictionary;
		}
		pdfDictionary4["AS"] = pdfArray3;
	}

	private void BeginSaveContent(object sender, SavePdfPrimitiveEventArgs e)
	{
		if (layer.graphics == null)
		{
			return;
		}
		using Dictionary<PdfGraphics, PdfGraphics>.Enumerator enumerator = graphics.GetEnumerator();
		if (enumerator.MoveNext())
		{
			KeyValuePair<PdfGraphics, PdfGraphics> current = enumerator.Current;
			m_graphics = current.Key;
			if (!m_graphics.isEmptyLayer)
			{
				BeginLayer(m_graphics);
				m_graphics.EndMarkContent();
			}
			m_graphics.StreamWriter.Write("Q" + Environment.NewLine);
			graphics.Remove(current.Key);
		}
	}

	internal void BeginLayer(PdfGraphics currentGraphics)
	{
		if (graphics != null)
		{
			if (graphics.ContainsKey(currentGraphics))
			{
				m_graphics = graphics[currentGraphics];
			}
			else
			{
				m_graphics = currentGraphics;
			}
		}
		if (m_graphics == null || string.IsNullOrEmpty(m_name))
		{
			return;
		}
		m_graphics.isEmptyLayer = true;
		if (m_parentLayer.Count != 0)
		{
			for (int i = 0; i < m_parentLayer.Count; i++)
			{
				if (!string.IsNullOrEmpty(m_parentLayer[i].LayerId))
				{
					byte[] bytes = Encoding.UTF8.GetBytes("/OC /" + m_parentLayer[i].LayerId.TrimStart('/') + " BDC\n");
					m_graphics.StreamWriter.Write(bytes);
				}
			}
		}
		byte[] bytes2 = Encoding.UTF8.GetBytes("/OC /" + LayerId + " BDC\n");
		if (Name != null && Name != string.Empty)
		{
			m_graphics.StreamWriter.Write(bytes2);
			m_isEndState = true;
		}
		else
		{
			m_content.Write(bytes2);
		}
	}

	private void GraphicsContent(PdfPageBase page)
	{
		PdfGraphics.GetResources resources = page.GetResources;
		PdfStream pdfStream = new PdfStream();
		PdfArray pdfArray = null;
		if (page.Dictionary.ContainsKey("CropBox"))
		{
			pdfArray = page.Dictionary.GetValue("CropBox", "Parent") as PdfArray;
			float num = 0f;
			float num2 = 0f;
			float num3 = 0f;
			float num4 = 0f;
			if (pdfArray != null)
			{
				if (pdfArray[0] is PdfNumber)
				{
					num = (pdfArray[0] as PdfNumber).FloatValue;
				}
				if (pdfArray[1] is PdfNumber)
				{
					num2 = (pdfArray[1] as PdfNumber).FloatValue;
				}
				if (pdfArray[2] is PdfNumber)
				{
					num3 = (pdfArray[2] as PdfNumber).FloatValue;
				}
				if (pdfArray[3] is PdfNumber)
				{
					num4 = (pdfArray[3] as PdfNumber).FloatValue;
				}
				if ((num < 0f || num2 < 0f || num3 < 0f || num4 < 0f) && Math.Floor(Math.Abs(num2)) == Math.Floor(Math.Abs(page.Size.Height)) && Math.Floor(Math.Abs(num)) == Math.Floor(Math.Abs(page.Size.Width)))
				{
					RectangleF rectangleF = new RectangleF(Math.Min(num, num3), Math.Min(num2, num4), Math.Max(num, num3), Math.Max(num2, num4));
					m_graphics = new PdfGraphics(new SizeF(rectangleF.Width, rectangleF.Height), resources, pdfStream);
				}
				else
				{
					m_graphics = new PdfGraphics(page.Size, resources, pdfStream);
					m_graphics.m_cropBox = pdfArray;
				}
			}
		}
		else
		{
			m_graphics = new PdfGraphics(page.Size, resources, pdfStream);
		}
		page.Contents.Add(new PdfReferenceHolder(pdfStream));
		pdfStream.BeginSave += BeginSaveContent;
		if (!graphics.ContainsKey(m_graphics))
		{
			graphics[m_graphics] = m_graphics;
		}
		if (!pageGraphics.ContainsKey(page))
		{
			pageGraphics[page] = m_graphics;
		}
	}

	private void ParseContent()
	{
		if (layer.Page == null || !(layer.Page is PdfLoadedPage))
		{
			return;
		}
		PdfPageLayerCollection layers = layer.Page.Layers;
		if (layers == null)
		{
			return;
		}
		for (int i = 0; i < layers.Count; i++)
		{
			PdfPageLayer pdfPageLayer = layers[i];
			if (pdfPageLayer.Name != null && pdfPageLayer.Name != string.Empty && pdfPageLayer.ReferenceHolder == layer.ReferenceHolder && pdfPageLayer != null)
			{
				layer.m_content = pdfPageLayer.m_content;
				PdfGraphics.GetResources resources = layer.Page.GetResources;
				layer.m_graphics = new PdfGraphics(layer.Page.Size, resources, layer.m_content);
				break;
			}
		}
		m_contentParsed = true;
	}

	private void ParsingLayerPage()
	{
		if (document == null || !(document is PdfLoadedDocument))
		{
			return;
		}
		for (int i = 0; i < document.PageCount; i++)
		{
			PdfDictionary dictionary = (document as PdfLoadedDocument).Pages[i].Dictionary;
			PdfPageBase pagebase = (document as PdfLoadedDocument).Pages[i];
			if (!dictionary.ContainsKey("Resources"))
			{
				continue;
			}
			PdfDictionary pdfDictionary = PdfCrossTable.Dereference(dictionary["Resources"]) as PdfDictionary;
			if ((pdfDictionary == null || !pdfDictionary.ContainsKey("Properties")) && !pdfDictionary.ContainsKey("XObject"))
			{
				continue;
			}
			PdfDictionary pdfDictionary2 = PdfCrossTable.Dereference(pdfDictionary["Properties"]) as PdfDictionary;
			PdfDictionary pdfDictionary3 = PdfCrossTable.Dereference(pdfDictionary["XObject"]) as PdfDictionary;
			if (pdfDictionary2 != null)
			{
				foreach (KeyValuePair<PdfName, IPdfPrimitive> item in pdfDictionary2.Items)
				{
					if (item.Value is PdfReferenceHolder)
					{
						PdfReferenceHolder pdfReferenceHolder = item.Value as PdfReferenceHolder;
						PdfDictionary dictionary2 = pdfReferenceHolder.Object as PdfDictionary;
						PdfName key = item.Key;
						if (ParsingDictionary(dictionary2, pdfReferenceHolder, pagebase, key))
						{
							break;
						}
					}
				}
			}
			if (pdfDictionary3 == null)
			{
				continue;
			}
			foreach (KeyValuePair<PdfName, IPdfPrimitive> item2 in pdfDictionary3.Items)
			{
				PdfReferenceHolder pdfReferenceHolder2 = item2.Value as PdfReferenceHolder;
				PdfDictionary pdfDictionary4 = pdfReferenceHolder2.Object as PdfDictionary;
				if (pdfDictionary4.ContainsKey("OC"))
				{
					PdfName key2 = item2.Key;
					pdfReferenceHolder2 = pdfDictionary4["OC"] as PdfReferenceHolder;
					pdfDictionary4 = PdfCrossTable.Dereference(pdfDictionary4["OC"]) as PdfDictionary;
					if (ParsingDictionary(pdfDictionary4, pdfReferenceHolder2, pagebase, key2))
					{
						layer.xobject.Add(key2.Value.TrimStart('/'));
						break;
					}
				}
			}
		}
	}

	private bool ParsingDictionary(PdfDictionary dictionary, PdfReferenceHolder reference, PdfPageBase pagebase, PdfName layerID)
	{
		bool result = false;
		if (!dictionary.ContainsKey("Name") && dictionary.ContainsKey("OCGs"))
		{
			if (dictionary.ContainsKey("OCGs"))
			{
				if (!(PdfCrossTable.Dereference(dictionary["OCGs"]) is PdfArray pdfArray))
				{
					reference = dictionary["OCGs"] as PdfReferenceHolder;
					dictionary = PdfCrossTable.Dereference(dictionary["OCGs"]) as PdfDictionary;
					if (dictionary != null && dictionary.ContainsKey("Name"))
					{
						result = SetLayerPage(reference, pagebase, layerID);
					}
				}
				else
				{
					for (int i = 0; i < pdfArray.Count; i++)
					{
						if (pdfArray[i] is PdfReferenceHolder)
						{
							reference = pdfArray[i] as PdfReferenceHolder;
							dictionary = (pdfArray[i] as PdfReferenceHolder).Object as PdfDictionary;
							result = SetLayerPage(reference, pagebase, layerID);
						}
					}
				}
			}
		}
		else if (dictionary.ContainsKey("Name"))
		{
			result = SetLayerPage(reference, pagebase, layerID);
		}
		return result;
	}

	private bool SetLayerPage(PdfReferenceHolder reference, PdfPageBase pagebase, PdfName layerID)
	{
		bool result = false;
		if (layer.ReferenceHolder != null && layer.ReferenceHolder.Equals(reference))
		{
			layer.m_pagePasrsed = true;
			result = true;
			layer.LayerId = layerID.Value.TrimStart('/');
			layer.Page = pagebase;
			if (!layer.pages.Contains(pagebase))
			{
				layer.pages.Add(pagebase);
			}
		}
		return result;
	}
}
