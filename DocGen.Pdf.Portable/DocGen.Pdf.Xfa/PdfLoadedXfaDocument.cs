using System.Collections.Generic;
using System.IO;
using System.Xml;
using DocGen.Pdf.Parsing;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Xfa;

public class PdfLoadedXfaDocument
{
	private PdfLoadedXfaForm m_form;

	private PdfFileStructure m_fileStructure = new PdfFileStructure();

	private PdfLoadedDocument m_document;

	private bool m_flatten;

	private XmlDocument m_xmlData;

	public PdfLoadedXfaForm XfaForm
	{
		get
		{
			if (m_form == null)
			{
				return null;
			}
			return m_form;
		}
		internal set
		{
			m_form = value;
		}
	}

	public XmlDocument XmlData
	{
		get
		{
			if (m_xmlData == null && XfaForm != null)
			{
				LoadXDP(XfaForm.XFAArray);
			}
			return m_xmlData;
		}
	}

	public PdfLoadedXfaDocument(Stream file)
	{
		file.Position = 0L;
		m_document = new PdfLoadedDocument(file, openAndRepair: false, isXfaDocument: true);
		m_fileStructure = m_document.FileStructure;
		if (m_document.Form != null)
		{
			m_form = new PdfLoadedXfaForm();
			XfaForm.Load(m_document.Catalog);
			m_document.Form.LoadedXfa = XfaForm;
		}
		m_document.m_isXfaDocument = true;
	}

	public PdfLoadedXfaDocument(Stream file, string password)
	{
		file.Position = 0L;
		m_document = new PdfLoadedDocument(file, password, openAndRepair: false, isXfaDocument: true);
		m_fileStructure = m_document.FileStructure;
		if (m_document.Form != null)
		{
			m_form = new PdfLoadedXfaForm();
			XfaForm.Load(m_document.Catalog);
			m_document.Form.LoadedXfa = XfaForm;
		}
		m_document.m_isXfaDocument = true;
	}

	public void Save(Stream stream)
	{
		m_document.Save(stream);
	}

	public void Close()
	{
		m_document.Close(completely: true);
		if (XfaForm != null && XfaForm.fDocument != null)
		{
			XfaForm.fDocument.Close(completely: true);
		}
	}

	private void LoadXDP(Dictionary<string, PdfStream> xfaArray)
	{
		using MemoryStream memoryStream = new MemoryStream();
		foreach (KeyValuePair<string, PdfStream> item in xfaArray)
		{
			byte[] decompressedData = item.Value.GetDecompressedData();
			memoryStream.Write(decompressedData, 0, decompressedData.Length);
		}
		if (memoryStream.CanRead && memoryStream.Length > 0)
		{
			memoryStream.Position = 0L;
			m_xmlData = new XmlDocument();
			m_xmlData.Load(memoryStream);
		}
	}
}
