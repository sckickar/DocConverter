using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using DocGen.Pdf.IO;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Xmp;

public class XmpMetadata : IPdfWrapper
{
	protected internal const string c_xpathRdf = "/x:xmpmeta/rdf:RDF";

	protected internal const string c_xmlnsUri = "http://www.w3.org/2000/xmlns/";

	protected internal const string c_xmlUri = "http://www.w3.org/XML/1998/namespace";

	protected internal const string c_rdfPrefix = "rdf";

	protected internal const string c_customSchema = "http://ns.adobe.com/pdfx/1.3/";

	protected internal const string c_xmlnsPrefix = "xmlns";

	protected internal const string c_xmlPefix = "xml";

	protected internal const string c_rdfUri = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";

	private const string c_startPacket = "begin=\"\ufeff\" id=\"W5M0MpCehiHzreSzNTczkc9d\"";

	private const string c_xmpMetaUri = "adobe:ns:meta/";

	private const string c_endPacket = "end=\"r\"";

	protected internal const string c_rdfPdfa = "http://www.aiim.org/pdfa/ns/id/";

	protected internal const string c_xap = "http://ns.adobe.com/xap/1.0/";

	protected internal const string c_pdfschema = "http://ns.adobe.com/pdf/1.3/";

	protected internal const string c_dublinSchema = "http://purl.org/dc/elements/1.1/";

	private const string c_zugferd1_0 = "urn:ferd:pdfa:CrossIndustryDocument:invoice:1p0#";

	private const string c_zugferd2_0 = "urn:zugferd:pdfa:CrossIndustryDocument:invoice:2p0#";

	private const string c_zugferd2_1 = "urn:factur-x:pdfa:CrossIndustryDocument:invoice:1p0#";

	private const string c_pdfaExtension = "http://www.aiim.org/pdfa/ns/extension/";

	private const string c_pdfaProperty = "http://www.aiim.org/pdfa/ns/property#";

	private const string c_pdfaSchema = "http://www.aiim.org/pdfa/ns/schema#";

	private const string c_rdfPdfua = "http://www.aiim.org/pdfua/ns/id/";

	private XDocument m_xmlDocument;

	private XmlNamespaceManager m_nmpManager;

	private DublinCoreSchema m_dublinCoreSchema;

	private PagedTextSchema m_pagedTextSchemaSchema;

	private BasicJobTicketSchema m_basicJobTicketSchema;

	private BasicSchema m_basicSchema;

	private RightsManagementSchema m_rightsManagementSchema;

	private PDFSchema m_pdfSchema;

	private CustomSchema m_customSchema;

	private PdfStream m_stream;

	internal bool isLoadedDocument;

	private PdfDocumentInformation m_documentInfo;

	internal bool m_hasAttributes;

	public XDocument XmlData => m_xmlDocument;

	internal PdfStream XmpStream => m_stream;

	public XmlNamespaceManager NamespaceManager => m_nmpManager;

	internal PdfDocumentInformation DocumentInfo
	{
		get
		{
			return m_documentInfo;
		}
		set
		{
			m_documentInfo = value;
		}
	}

	public DublinCoreSchema DublinCoreSchema
	{
		get
		{
			if (m_dublinCoreSchema == null && Rdf != null)
			{
				m_dublinCoreSchema = new DublinCoreSchema(this);
			}
			return m_dublinCoreSchema;
		}
	}

	public PagedTextSchema PagedTextSchema
	{
		get
		{
			if (m_pagedTextSchemaSchema == null && Rdf != null)
			{
				m_pagedTextSchemaSchema = new PagedTextSchema(this);
			}
			return m_pagedTextSchemaSchema;
		}
	}

	public BasicJobTicketSchema BasicJobTicketSchema
	{
		get
		{
			if (m_basicJobTicketSchema == null && Rdf != null)
			{
				m_basicJobTicketSchema = new BasicJobTicketSchema(this);
			}
			return m_basicJobTicketSchema;
		}
	}

	public BasicSchema BasicSchema
	{
		get
		{
			if (m_basicSchema == null && Rdf != null)
			{
				m_basicSchema = new BasicSchema(this);
			}
			return m_basicSchema;
		}
	}

	public RightsManagementSchema RightsManagementSchema
	{
		get
		{
			if (m_rightsManagementSchema == null && Rdf != null)
			{
				m_rightsManagementSchema = new RightsManagementSchema(this);
			}
			return m_rightsManagementSchema;
		}
	}

	public PDFSchema PDFSchema
	{
		get
		{
			if (m_pdfSchema == null && Rdf != null)
			{
				m_pdfSchema = new PDFSchema(this);
			}
			return m_pdfSchema;
		}
	}

	internal CustomSchema CustomSchema
	{
		get
		{
			if (m_customSchema == null && Rdf != null)
			{
				m_customSchema = new CustomSchema(this, "pdfx", "http://ns.adobe.com/pdfx/1.3/");
			}
			return m_customSchema;
		}
	}

	internal XElement Xmpmeta => XmlData.Descendants().SingleOrDefault((XElement p) => p.Name.LocalName == "xmpmeta") ?? throw new ArgumentNullException("node");

	internal XElement Rdf
	{
		get
		{
			XElement xElement = null;
			foreach (XElement item in XmlData.Descendants())
			{
				if (item.Name.LocalName == "RDF")
				{
					xElement = item;
					break;
				}
			}
			XName name = XmlData.Root.Name;
			if (xElement == null)
			{
				foreach (XElement item2 in XmlData.Descendants())
				{
					if (item2.Name.LocalName == name)
					{
						xElement = item2;
						break;
					}
				}
				if (xElement == null)
				{
					throw new ArgumentNullException("node");
				}
			}
			return xElement;
		}
	}

	IPdfPrimitive IPdfWrapper.Element => m_stream;

	public XmpMetadata(PdfDocumentInformation documentInfo)
	{
		Init(documentInfo);
	}

	public XmpMetadata(XDocument xmp)
	{
		if (xmp == null)
		{
			throw new ArgumentNullException("xmpMetadata");
		}
		m_stream = new PdfStream();
		m_stream.BeginSave += BeginSave;
		m_stream.EndSave += EndSave;
		Load(xmp);
	}

	public void Load(XDocument xmp)
	{
		if (xmp == null)
		{
			throw new ArgumentNullException("xmp");
		}
		Reset();
		m_xmlDocument = xmp;
		m_nmpManager = new XmlNamespaceManager(m_xmlDocument.CreateReader().NameTable);
		ImportNamespaces(m_xmlDocument.Root, m_nmpManager);
		if (DocumentInfo != null)
		{
			DocumentInfo.Catalog.SetProperty("Metadata", new PdfReferenceHolder(this));
		}
	}

	public void Add(XElement schema)
	{
		if (schema == null)
		{
			throw new ArgumentNullException("schema");
		}
		ImportNamespaces(schema, m_nmpManager);
		Rdf.Add(schema);
	}

	private void Init(PdfDocumentInformation documentInfo)
	{
		m_xmlDocument = new XDocument();
		m_nmpManager = new XmlNamespaceManager(XmlData.CreateReader().NameTable);
		m_stream = new PdfStream();
		m_documentInfo = documentInfo;
		InitStream();
		CreateStartPacket();
		CreateXmpmeta();
		CreateRdf(documentInfo);
		CreateEndPacket();
	}

	private void InitStream()
	{
		m_stream.BeginSave += BeginSave;
		m_stream.EndSave += EndSave;
		m_stream["Type"] = new PdfName("Metadata");
		m_stream["Subtype"] = new PdfName("XML");
		m_stream.Compress = false;
	}

	private void CreateStartPacket()
	{
		string data = "begin=\"\ufeff\" id=\"W5M0MpCehiHzreSzNTczkc9d\"";
		XProcessingInstruction content = new XProcessingInstruction("xpacket", data);
		XmlData.Add(content);
	}

	private void CreateXmpmeta()
	{
		XElement content = CreateElement("x", "xmpmeta", "adobe:ns:meta/");
		XmlData.Add(content);
	}

	private void CreateRdf(PdfDocumentInformation documentInfo)
	{
		XElement xElement = CreateElement("rdf", "RDF", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
		XNamespace xNamespace = AddNamespace("rdf", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
		if (!string.IsNullOrEmpty(documentInfo.Producer) || !string.IsNullOrEmpty(documentInfo.Keywords))
		{
			XNamespace xNamespace2 = AddNamespace("pdf", "http://ns.adobe.com/pdf/1.3/");
			XElement xElement2 = CreateElement("rdf", "Description", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
			xElement2.SetAttributeValue(xNamespace + "about", "");
			if (!string.IsNullOrEmpty(documentInfo.Producer))
			{
				XElement xElement3 = new XElement(xNamespace2 + "Producer", new XAttribute(XNamespace.Xmlns + "pdf", xNamespace2));
				xElement3.Value = documentInfo.Producer;
				xElement2.Add(xElement3);
			}
			if (!string.IsNullOrEmpty(documentInfo.Keywords))
			{
				XElement xElement4 = new XElement(xNamespace2 + "Keywords", new XAttribute(XNamespace.Xmlns + "pdf", xNamespace2));
				xElement4.Value = documentInfo.Keywords;
				xElement2.Add(xElement4);
			}
			xElement.Add(xElement2);
		}
		if ((PdfDocument.ConformanceLevel == PdfConformanceLevel.None || string.IsNullOrEmpty(documentInfo.Creator)) && (!string.IsNullOrEmpty(documentInfo.Creator) || documentInfo.m_creationDate.ToString() != null || documentInfo.m_modificationDate.ToString() != null))
		{
			XElement xElement5 = CreateElement("rdf", "Description", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
			xElement5.SetAttributeValue(xNamespace + "about", "");
			XNamespace xNamespace3 = AddNamespace("xap", "http://ns.adobe.com/xap/1.0/");
			xElement5.SetAttributeValue(XNamespace.Xmlns + "xap", xNamespace3);
			if (!string.IsNullOrEmpty(documentInfo.Creator))
			{
				xElement5.SetElementValue(xNamespace3 + "CreatorTool", documentInfo.Creator);
			}
			if (documentInfo.m_creationDate.ToString() != null && !documentInfo.isRemoveCreationDate)
			{
				string dateTime = (dateTime = GetDateTime(documentInfo.CreationDate));
				xElement5.SetElementValue(xNamespace3 + "CreateDate", dateTime);
			}
			if (documentInfo.m_modificationDate.ToString() != null && !documentInfo.isRemoveModifyDate)
			{
				string dateTime2 = GetDateTime(documentInfo.m_modificationDate);
				xElement5.SetElementValue(xNamespace3 + "ModifyDate", dateTime2);
			}
			xElement.Add(xElement5);
		}
		XNamespace xNamespace4 = AddNamespace("dc", "http://purl.org/dc/elements/1.1/");
		XElement xElement6 = CreateElement("rdf", "Description", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
		xElement6.SetAttributeValue(xNamespace + "about", "");
		xElement6.SetAttributeValue(XNamespace.Xmlns + "dc", xNamespace4);
		xElement6.SetElementValue(xNamespace4 + "format", "application/pdf");
		CreateDublinCoreContainer(xElement6, "title", documentInfo.Title, defaultLang: true, XmpArrayType.Alt);
		CreateDublinCoreContainer(xElement6, "description", documentInfo.Subject, defaultLang: true, XmpArrayType.Alt);
		CreateDublinCoreContainer(xElement6, "subject", documentInfo.Keywords, defaultLang: false, XmpArrayType.Bag);
		CreateDublinCoreContainer(xElement6, "creator", documentInfo.Author, defaultLang: false, XmpArrayType.Seq);
		xElement.Add(xElement6);
		if (PdfDocument.ConformanceLevel == PdfConformanceLevel.Pdf_A1B || PdfDocument.ConformanceLevel == PdfConformanceLevel.Pdf_A1A)
		{
			XNamespace xNamespace5 = AddNamespace("pdfaid", "http://www.aiim.org/pdfa/ns/id/");
			XElement xElement7 = CreateElement("rdf", "Description", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
			xElement7.SetAttributeValue(xNamespace + "about", "");
			xElement7.SetElementValue(xNamespace5 + "part", "1");
			if (PdfDocument.ConformanceLevel == PdfConformanceLevel.Pdf_A1B)
			{
				xElement7.SetElementValue(xNamespace5 + "conformance", "B");
			}
			else
			{
				xElement7.SetElementValue(xNamespace5 + "conformance", "A");
			}
			xElement7.SetAttributeValue(XNamespace.Xmlns + "pdfaid", xNamespace5);
			xElement.Add(xElement7);
			AddPdfUAConformance(xElement, xNamespace, documentInfo);
		}
		else if (PdfDocument.ConformanceLevel == PdfConformanceLevel.Pdf_A3B || PdfDocument.ConformanceLevel == PdfConformanceLevel.Pdf_A3A || PdfDocument.ConformanceLevel == PdfConformanceLevel.Pdf_A3U)
		{
			XElement xElement8 = CreateConformanceElement(PdfDocument.ConformanceLevel, xNamespace);
			xElement.Add(xElement8);
			AddPdfUAConformance(xElement, xNamespace, documentInfo);
			if (documentInfo.ZugferdConformanceLevel != 0)
			{
				AddZugferdConformance(xElement, xElement8, xNamespace, documentInfo);
			}
		}
		else if (PdfDocument.ConformanceLevel == PdfConformanceLevel.Pdf_A2B || PdfDocument.ConformanceLevel == PdfConformanceLevel.Pdf_A2A || PdfDocument.ConformanceLevel == PdfConformanceLevel.Pdf_A2U)
		{
			xElement.Add(CreateConformanceElement(PdfDocument.ConformanceLevel, xNamespace));
			AddPdfUAConformance(xElement, xNamespace, documentInfo);
		}
		else if (PdfDocument.ConformanceLevel == PdfConformanceLevel.Pdf_A4 || PdfDocument.ConformanceLevel == PdfConformanceLevel.Pdf_A4E || PdfDocument.ConformanceLevel == PdfConformanceLevel.Pdf_A4F)
		{
			XElement content = CreateConformanceElement(PdfDocument.ConformanceLevel, xNamespace);
			xElement.Add(content);
			AddPdfUAConformance(xElement, xNamespace, documentInfo);
		}
		else
		{
			NamespaceManager.AddNamespace("pdfaid", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
		}
		Xmpmeta.Add(xElement);
		if (documentInfo.m_autoTag && PdfDocument.ConformanceLevel == PdfConformanceLevel.None)
		{
			XNamespace xNamespace6 = AddNamespace("pdfuaid", "http://www.aiim.org/pdfua/ns/id/");
			XElement xElement9 = CreateElement("rdf", "Description", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
			xElement9.SetAttributeValue(xNamespace6 + "part", "1");
			xElement9.SetAttributeValue(XNamespace.Xmlns + "pdfuaid", xNamespace6);
			xElement.Add(xElement9);
		}
		if (!documentInfo.ConformanceEnabled && !string.IsNullOrEmpty(documentInfo.Creator) && documentInfo.m_creationDate.ToString() != null && documentInfo.m_modificationDate.ToString() != null)
		{
			XElement xElement10 = CreateElement("rdf", "Description", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
			XNamespace xNamespace7 = AddNamespace("xap", "http://ns.adobe.com/xap/1.0/");
			xElement10.SetAttributeValue(xNamespace + "about", "");
			xElement10.SetAttributeValue(XNamespace.Xmlns + "xap", xNamespace7);
			xElement10.SetElementValue(xNamespace7 + "CreatorTool", documentInfo.Creator);
			if (!documentInfo.isRemoveCreationDate)
			{
				string dateTime3 = GetDateTime(documentInfo.CreationDate);
				xElement10.SetElementValue(xNamespace7 + "CreateDate", dateTime3);
			}
			if (!documentInfo.isRemoveModifyDate)
			{
				string dateTime4 = GetDateTime(documentInfo.m_modificationDate);
				xElement10.SetElementValue(xNamespace7 + "ModifyDate", dateTime4);
			}
			xElement.Add(xElement10);
		}
	}

	private void AddZugferdConformance(XElement rdf, XElement pdfA, XNamespace xNamespace, PdfDocumentInformation documentInfo)
	{
		string empty = string.Empty;
		string empty2 = string.Empty;
		if (documentInfo.ZugferdVersion == ZugferdVersion.ZugferdVersion1_0)
		{
			empty = "zf";
			empty2 = "urn:ferd:pdfa:CrossIndustryDocument:invoice:1p0#";
			if (documentInfo.ZugferdConformanceLevel.Equals(ZugferdConformanceLevel.EN16931) || documentInfo.ZugferdConformanceLevel.Equals(ZugferdConformanceLevel.Minimum))
			{
				documentInfo.ZugferdConformanceLevel = ZugferdConformanceLevel.Basic;
			}
		}
		else if (documentInfo.ZugferdVersion == ZugferdVersion.ZugferdVersion2_1)
		{
			empty = "fx";
			empty2 = "urn:factur-x:pdfa:CrossIndustryDocument:invoice:1p0#";
		}
		else
		{
			empty = "fx";
			empty2 = "urn:zugferd:pdfa:CrossIndustryDocument:invoice:2p0#";
		}
		XNamespace xNamespace2 = AddNamespace(empty, empty2);
		pdfA = CreateElement("rdf", "Description", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
		pdfA.SetAttributeValue(xNamespace + "about", "");
		pdfA.SetAttributeValue(XNamespace.Xmlns + empty, xNamespace2);
		string value = documentInfo.ZugferdConformanceLevel.ToString().ToUpper();
		if (documentInfo.ZugferdConformanceLevel.Equals(ZugferdConformanceLevel.EN16931))
		{
			value = "EN 16931";
		}
		pdfA.SetElementValue(xNamespace2 + "ConformanceLevel", value);
		if (documentInfo.ZugferdVersion == ZugferdVersion.ZugferdVersion1_0)
		{
			pdfA.SetElementValue(xNamespace2 + "DocumentFileName", "ZUGFeRD-invoice.xml");
		}
		else if (documentInfo.ZugferdVersion == ZugferdVersion.ZugferdVersion2_1)
		{
			pdfA.SetElementValue(xNamespace2 + "DocumentFileName", "factur-x.xml");
		}
		else
		{
			pdfA.SetElementValue(xNamespace2 + "DocumentFileName", "zugferd-invoice.xml");
		}
		pdfA.SetElementValue(xNamespace2 + "DocumentType", "INVOICE");
		if (documentInfo.ZugferdVersion == ZugferdVersion.ZugferdVersion2_1)
		{
			pdfA.SetElementValue(xNamespace2 + "Version", "2.1");
		}
		else
		{
			pdfA.SetElementValue(xNamespace2 + "Version", "1.0");
		}
		rdf.Add(pdfA);
		XNamespace value2 = AddNamespace("pdfaExtension", "http://www.aiim.org/pdfa/ns/extension/");
		XNamespace xNamespace3 = AddNamespace("pdfaSchema", "http://www.aiim.org/pdfa/ns/schema#");
		XNamespace xNamespace4 = AddNamespace("pdfaProperty", "http://www.aiim.org/pdfa/ns/property#");
		pdfA = CreateElement("rdf", "Description", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
		pdfA.SetAttributeValue(xNamespace + "about", "");
		pdfA.SetAttributeValue(XNamespace.Xmlns + "pdfaExtension", value2);
		pdfA.SetAttributeValue(XNamespace.Xmlns + "pdfaSchema", xNamespace3);
		pdfA.SetAttributeValue(XNamespace.Xmlns + "pdfaProperty", xNamespace4);
		XElement xElement = CreateElement("pdfaExtension", "schemas", "http://www.aiim.org/pdfa/ns/extension/");
		XElement xElement2 = CreateElement("rdf", "Bag", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
		XElement xElement3 = CreateElement("rdf", "li", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
		xElement3.SetAttributeValue(xNamespace + "parseType", "Resource");
		xElement3.SetElementValue(xNamespace3 + "schema", "ZUGFeRD PDFA Extension Schema");
		if (documentInfo.ZugferdVersion == ZugferdVersion.ZugferdVersion1_0)
		{
			xElement3.SetElementValue(xNamespace3 + "namespaceURI", "urn:ferd:pdfa:CrossIndustryDocument:invoice:1p0#");
		}
		else if (documentInfo.ZugferdVersion == ZugferdVersion.ZugferdVersion2_1)
		{
			xElement3.SetElementValue(xNamespace3 + "namespaceURI", "urn:factur-x:pdfa:CrossIndustryDocument:invoice:1p0#");
		}
		else
		{
			xElement3.SetElementValue(xNamespace3 + "namespaceURI", "urn:zugferd:pdfa:CrossIndustryDocument:invoice:2p0#");
		}
		xElement3.SetElementValue(xNamespace3 + "prefix", empty);
		XElement xElement4 = CreateElement("pdfaSchema", "property", "http://www.aiim.org/pdfa/ns/schema#");
		XElement xElement5 = CreateElement("rdf", "Seq", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
		XElement xElement6 = CreateElement("rdf", "li", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
		xElement6.SetAttributeValue(xNamespace + "parseType", "Resource");
		xElement6.SetElementValue(xNamespace4 + "name", "DocumentFileName");
		xElement6.SetElementValue(xNamespace4 + "valueType", "Text");
		xElement6.SetElementValue(xNamespace4 + "category", "external");
		xElement6.SetElementValue(xNamespace4 + "description", "name of the embedded XML invoice file");
		xElement5.Add(xElement6);
		xElement6 = CreateElement("rdf", "li", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
		xElement6.SetAttributeValue(xNamespace + "parseType", "Resource");
		xElement6.SetElementValue(xNamespace4 + "name", "DocumentType");
		xElement6.SetElementValue(xNamespace4 + "valueType", "Text");
		xElement6.SetElementValue(xNamespace4 + "category", "external");
		xElement6.SetElementValue(xNamespace4 + "description", "INVOICE");
		xElement5.Add(xElement6);
		xElement6 = CreateElement("rdf", "li", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
		xElement6.SetAttributeValue(xNamespace + "parseType", "Resource");
		xElement6.SetElementValue(xNamespace4 + "name", "Version");
		xElement6.SetElementValue(xNamespace4 + "valueType", "Text");
		xElement6.SetElementValue(xNamespace4 + "category", "external");
		xElement6.SetElementValue(xNamespace4 + "description", "The actual version of the ZUGFeRD XML schema");
		xElement5.Add(xElement6);
		xElement6 = CreateElement("rdf", "li", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
		xElement6.SetAttributeValue(xNamespace + "parseType", "Resource");
		xElement6.SetElementValue(xNamespace4 + "name", "ConformanceLevel");
		xElement6.SetElementValue(xNamespace4 + "valueType", "Text");
		xElement6.SetElementValue(xNamespace4 + "category", "external");
		xElement6.SetElementValue(xNamespace4 + "description", "The conformance level of the embedded ZUGFeRD data");
		xElement5.Add(xElement6);
		xElement4.Add(xElement5);
		xElement3.Add(xElement4);
		xElement2.Add(xElement3);
		xElement.Add(xElement2);
		pdfA.Add(xElement);
		rdf.Add(pdfA);
	}

	private XElement CreateConformanceElement(PdfConformanceLevel conformance, XNamespace xNamespace)
	{
		XNamespace xNamespace2 = AddNamespace("pdfaid", "http://www.aiim.org/pdfa/ns/id/");
		XElement xElement = CreateElement("rdf", "Description", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
		xElement.SetAttributeValue(xNamespace + "about", "");
		if (conformance == PdfConformanceLevel.Pdf_A2B || PdfDocument.ConformanceLevel == PdfConformanceLevel.Pdf_A2A || PdfDocument.ConformanceLevel == PdfConformanceLevel.Pdf_A2U)
		{
			xElement.SetElementValue(xNamespace2 + "part", "2");
			switch (conformance)
			{
			case PdfConformanceLevel.Pdf_A2B:
				xElement.SetElementValue(xNamespace2 + "conformance", "B");
				break;
			case PdfConformanceLevel.Pdf_A2A:
				xElement.SetElementValue(xNamespace2 + "conformance", "A");
				break;
			default:
				xElement.SetElementValue(xNamespace2 + "conformance", "U");
				break;
			}
		}
		else
		{
			switch (conformance)
			{
			case PdfConformanceLevel.Pdf_A3B:
			case PdfConformanceLevel.Pdf_A3A:
			case PdfConformanceLevel.Pdf_A3U:
				xElement.SetElementValue(xNamespace2 + "part", "3");
				switch (conformance)
				{
				case PdfConformanceLevel.Pdf_A3B:
					xElement.SetElementValue(xNamespace2 + "conformance", "B");
					break;
				case PdfConformanceLevel.Pdf_A3A:
					xElement.SetElementValue(xNamespace2 + "conformance", "A");
					break;
				default:
					xElement.SetElementValue(xNamespace2 + "conformance", "U");
					break;
				}
				break;
			case PdfConformanceLevel.Pdf_A4:
			case PdfConformanceLevel.Pdf_A4E:
			case PdfConformanceLevel.Pdf_A4F:
				xElement.SetElementValue(xNamespace2 + "part", "4");
				switch (conformance)
				{
				case PdfConformanceLevel.Pdf_A4E:
					xElement.SetElementValue(xNamespace2 + "conformance", "E");
					break;
				case PdfConformanceLevel.Pdf_A4F:
					xElement.SetElementValue(xNamespace2 + "conformance", "F");
					break;
				}
				xElement.SetElementValue(xNamespace2 + "rev", "2020");
				break;
			}
		}
		xElement.SetAttributeValue(XNamespace.Xmlns + "pdfaid", xNamespace2);
		return xElement;
	}

	private void CreateUAConformanceElement(XElement rdf, XNamespace xNamespace)
	{
		XNamespace value = AddNamespace("pdfaExtension", "http://www.aiim.org/pdfa/ns/extension/");
		XNamespace xNamespace2 = AddNamespace("pdfaSchema", "http://www.aiim.org/pdfa/ns/schema#");
		XNamespace xNamespace3 = AddNamespace("pdfaProperty", "http://www.aiim.org/pdfa/ns/property#");
		XElement xElement = CreateElement("rdf", "Description", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
		xElement.SetAttributeValue(xNamespace + "about", "");
		xElement.SetAttributeValue(XNamespace.Xmlns + "pdfaExtension", value);
		xElement.SetAttributeValue(XNamespace.Xmlns + "pdfaSchema", xNamespace2);
		xElement.SetAttributeValue(XNamespace.Xmlns + "pdfaProperty", xNamespace3);
		XElement xElement2 = CreateElement("pdfaExtension", "schemas", "http://www.aiim.org/pdfa/ns/extension/");
		XElement xElement3 = CreateElement("rdf", "Bag", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
		XElement xElement4 = CreateElement("rdf", "li", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
		XElement xElement5 = CreateElement("rdf", "Description", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
		xElement5.SetAttributeValue(xNamespace2 + "namespaceURI", "http://www.aiim.org/pdfua/ns/id/");
		xElement5.SetAttributeValue(xNamespace2 + "prefix", "pdfuaid");
		xElement5.SetAttributeValue(xNamespace2 + "schema", "PDF/UA identification schema");
		XElement xElement6 = CreateElement("pdfaSchema", "property", "http://www.aiim.org/pdfa/ns/schema#");
		XElement xElement7 = CreateElement("rdf", "Seq", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
		XElement xElement8 = CreateElement("rdf", "li", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
		xElement8.SetAttributeValue(xNamespace3 + "category", "internal");
		xElement8.SetAttributeValue(xNamespace3 + "description", "PDF/UA version identifier");
		xElement8.SetAttributeValue(xNamespace3 + "name", "part");
		xElement8.SetAttributeValue(xNamespace3 + "valueType", "Integer");
		xElement7.Add(xElement8);
		xElement8 = CreateElement("rdf", "li", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
		xElement8.SetAttributeValue(xNamespace3 + "category", "internal");
		xElement8.SetAttributeValue(xNamespace3 + "description", "PDF/UA amendment identifier");
		xElement8.SetAttributeValue(xNamespace3 + "name", "amd");
		xElement8.SetAttributeValue(xNamespace3 + "valueType", "Text");
		xElement7.Add(xElement8);
		xElement8 = CreateElement("rdf", "li", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
		xElement8.SetAttributeValue(xNamespace3 + "category", "internal");
		xElement8.SetAttributeValue(xNamespace3 + "description", "PDF/UA corrigenda identifier");
		xElement8.SetAttributeValue(xNamespace3 + "name", "corr");
		xElement8.SetAttributeValue(xNamespace3 + "valueType", "Text");
		xElement7.Add(xElement8);
		xElement6.Add(xElement7);
		xElement5.Add(xElement6);
		xElement4.Add(xElement5);
		xElement3.Add(xElement4);
		xElement2.Add(xElement3);
		xElement.Add(xElement2);
		rdf.Add(xElement);
	}

	private void AddPdfUAConformance(XElement rdf, XNamespace xNamespace, PdfDocumentInformation documentInfo)
	{
		if (documentInfo != null && documentInfo.m_autoTag)
		{
			XNamespace xNamespace2 = AddNamespace("pdfuaid", "http://www.aiim.org/pdfua/ns/id/");
			XElement xElement = CreateElement("rdf", "Description", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
			xElement.SetElementValue(xNamespace2 + "part", "1");
			xElement.SetAttributeValue(xNamespace + "about", "");
			xElement.SetAttributeValue(XNamespace.Xmlns + "pdfuaid", xNamespace2);
			rdf.Add(xElement);
			if (PdfDocument.ConformanceLevel != PdfConformanceLevel.Pdf_X1A2001)
			{
				CreateUAConformanceElement(rdf, xNamespace);
			}
		}
	}

	private void CreateDublinCoreContainer(XElement dublinDesc, string containerName, string value, bool defaultLang, XmpArrayType element)
	{
		if (!string.IsNullOrEmpty(value))
		{
			XElement xElement = CreateElement("dc", containerName, "http://purl.org/dc/elements/1.1/");
			_ = (XNamespace?)AddNamespace("rdf", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
			XElement xElement2 = CreateElement("rdf", element.ToString(), "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
			XElement xElement3 = CreateElement("rdf", "li", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
			if (containerName == "subject" || (containerName == "creator" && PdfDocument.ConformanceLevel != 0))
			{
				char[] separator = new char[2] { ',', ';' };
				string[] array = value.Split(separator);
				for (int i = 0; i < array.Length; i++)
				{
					if (i > 0)
					{
						xElement3 = CreateElement("rdf", "li", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
					}
					xElement3.Value = array[i].Trim();
					xElement2.Add(xElement3);
				}
			}
			else
			{
				xElement3.Value = value;
				xElement2.Add(xElement3);
			}
			xElement.Add(xElement2);
			dublinDesc.Add(xElement);
			if (defaultLang)
			{
				xElement3.SetAttributeValue(XNamespace.Xml + "lang", "x-default");
			}
		}
		else if (DocumentInfo.ConformanceEnabled)
		{
			XElement xElement4 = CreateElement("dc", containerName, "http://purl.org/dc/elements/1.1/");
			_ = (XNamespace?)AddNamespace("rdf", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
			XElement xElement5 = CreateElement("rdf", element.ToString(), "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
			XElement xElement6 = CreateElement("rdf", "li", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
			xElement5.Add(xElement6);
			xElement4.Add(xElement5);
			dublinDesc.Add(xElement4);
			if (defaultLang)
			{
				xElement6.SetAttributeValue(XNamespace.Xml + "lang", "x-default");
			}
		}
	}

	private void CreateEndPacket()
	{
		string data = "end=\"r\"";
		XProcessingInstruction content = new XProcessingInstruction("xpacket", data);
		XmlData.Add(content);
	}

	private void Reset()
	{
		m_xmlDocument = null;
		m_nmpManager = null;
		m_dublinCoreSchema = null;
	}

	private void ImportNamespaces(XElement elm, XmlNamespaceManager nsm)
	{
		if (elm == null)
		{
			throw new ArgumentNullException("elm");
		}
		if (nsm == null)
		{
			throw new ArgumentNullException("nsm");
		}
		string prefixOfNamespace = elm.GetPrefixOfNamespace(elm.Name.Namespace);
		string baseUri = elm.BaseUri;
		if (prefixOfNamespace != null && prefixOfNamespace.Length > 0 && baseUri != null && !nsm.HasNamespace(prefixOfNamespace))
		{
			nsm.AddNamespace(prefixOfNamespace, baseUri);
		}
		if (!elm.HasElements)
		{
			return;
		}
		for (XNode xNode = elm.FirstNode; xNode != null; xNode = xNode.NextNode)
		{
			XNode xNode2 = xNode;
			if (xNode2.NodeType == XmlNodeType.Element)
			{
				ImportNamespaces(xNode2 as XElement, nsm);
			}
		}
	}

	private string GetDateTime(DateTime dateTime)
	{
		int minutes = new DateTimeOffset(dateTime).Offset.Minutes;
		string text = minutes.ToString();
		if (minutes >= 0 && minutes <= 9)
		{
			text = "0" + text;
		}
		int hours = new DateTimeOffset(dateTime).Offset.Hours;
		string text2 = hours.ToString();
		if (hours >= 0 && hours <= 9)
		{
			text2 = "0" + text2;
		}
		string empty = string.Empty;
		if (hours < 0)
		{
			if (text2.Length == 2)
			{
				text2 = (-hours).ToString();
				text2 = "-0" + text2;
			}
			return dateTime.ToString("s") + text2 + ":" + text;
		}
		return dateTime.ToString("s") + "+" + text2 + ":" + text;
	}

	internal XElement CreateElement(string name)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		return new XElement(name);
	}

	internal XElement CreateElement(XNamespace prefix, string localName, string namespaceURI)
	{
		if (prefix == null)
		{
			throw new ArgumentNullException("prefix");
		}
		if (localName == null)
		{
			throw new ArgumentNullException("localName");
		}
		XElement xElement = null;
		XNamespace xNamespace = namespaceURI;
		xElement = ((NamespaceManager.HasNamespace(prefix.NamespaceName) || !(prefix != "xml") || !(prefix != "xmlns")) ? new XElement(xNamespace + localName) : new XElement(xNamespace + localName, new XAttribute(XNamespace.Xmlns + prefix.NamespaceName, xNamespace)));
		namespaceURI = AddNamespace(prefix.NamespaceName, namespaceURI);
		return xElement;
	}

	internal XAttribute CreateAttribute(XNamespace name, string value)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		return new XAttribute(XNamespace.Xmlns + name.NamespaceName, value);
	}

	internal XAttribute CreateAttribute(XNamespace prefix, string localName, string namespaceURI, string value)
	{
		if (prefix == null)
		{
			throw new ArgumentNullException("prefix");
		}
		if (localName == null)
		{
			throw new ArgumentNullException("localName");
		}
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		XNamespace? xNamespace = namespaceURI;
		namespaceURI = AddNamespace(prefix.NamespaceName, namespaceURI);
		return new XAttribute(xNamespace + localName, value);
	}

	internal string AddNamespace(string prefix, string namespaceURI)
	{
		if (prefix == null)
		{
			throw new ArgumentNullException("prefix");
		}
		string result = namespaceURI;
		if (!NamespaceManager.HasNamespace(prefix) && prefix != "xml" && prefix != "xmlns")
		{
			if (namespaceURI == null)
			{
				throw new ArgumentNullException("namespaceURI");
			}
			NamespaceManager.AddNamespace(prefix, namespaceURI);
		}
		else
		{
			result = NamespaceManager.LookupNamespace(prefix);
		}
		return result;
	}

	private void BeginSave(object sender, SavePdfPrimitiveEventArgs ars)
	{
		PdfStreamWriter pdfStreamWriter = new PdfStreamWriter(m_stream);
		try
		{
			pdfStreamWriter.Write(XmlData.ToString());
		}
		catch (Exception)
		{
			XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
			xmlWriterSettings.CheckCharacters = false;
			StringWriter stringWriter = new StringWriter();
			using (XmlWriter writer = XmlWriter.Create(stringWriter, xmlWriterSettings))
			{
				XmlData.Save(writer);
			}
			string text = stringWriter.ToString();
			pdfStreamWriter.Write(text);
		}
	}

	private void EndSave(object sender, SavePdfPrimitiveEventArgs ars)
	{
		m_stream.Clear();
	}
}
