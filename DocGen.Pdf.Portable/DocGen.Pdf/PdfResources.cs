using System;
using System.Collections.Generic;
using DocGen.Pdf.ColorSpace;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.IO;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf;

internal class PdfResources : PdfDictionary
{
	private Dictionary<IPdfPrimitive, PdfName> m_names;

	private PdfDictionary m_properties = new PdfDictionary();

	private string m_originalFontName;

	private PdfDocument m_document;

	private int m_imageCounter;

	private string m_imageName = "Im";

	private int m_fontCounter;

	private string m_fontName = "F";

	private int m_colorSpaceCounter;

	private string m_colorSpaceName = "Cs";

	private int m_brushCounter;

	private string m_brushName = "Br";

	private int m_templateCounter;

	private string m_templateName = "Tp";

	private int m_transparencyCounter;

	private string m_transparencyName = "Tr";

	private int m_dColorSpaceCounter;

	private string m_dColorSpaceName = "Dc";

	private Dictionary<IPdfPrimitive, PdfName> Names => ObtainNames();

	internal string OriginalFontName
	{
		get
		{
			return m_originalFontName;
		}
		set
		{
			m_originalFontName = value;
		}
	}

	internal PdfDocument Document
	{
		get
		{
			return m_document;
		}
		set
		{
			m_document = value;
		}
	}

	internal PdfResources()
	{
	}

	internal PdfResources(PdfDictionary baseDictionary)
		: base(baseDictionary)
	{
	}

	internal PdfName GetName(IPdfWrapper obj)
	{
		PdfTemplate pdfTemplate = obj as PdfTemplate;
		if (obj == null)
		{
			throw new ArgumentNullException("obj");
		}
		IPdfPrimitive element = obj.Element;
		PdfName pdfName = null;
		if (Names.ContainsKey(element))
		{
			pdfName = Names[element];
		}
		if (m_originalFontName != null)
		{
			PdfName pdfName2 = new PdfName(m_originalFontName);
			foreach (KeyValuePair<IPdfPrimitive, PdfName> name in Names)
			{
				if (name.Value == pdfName2)
				{
					pdfName = name.Value;
					m_originalFontName = null;
					break;
				}
			}
			if (pdfName == null)
			{
				string text = "";
				text = (PdfDocument.EnableUniqueResourceNaming ? GenerateName() : GenerateName(obj));
				pdfName = new PdfName(text);
				Names[element] = pdfName;
				Add(obj, pdfName);
			}
		}
		else if (pdfName == null)
		{
			string text2 = "";
			text2 = ((pdfTemplate != null) ? ((pdfTemplate.CustomPdfTemplateName == null) ? GenerateName() : pdfTemplate.CustomPdfTemplateName) : (PdfDocument.EnableUniqueResourceNaming ? GenerateName() : GenerateName(obj)));
			pdfName = new PdfName(text2);
			Names[element] = pdfName;
			Add(obj, pdfName);
		}
		return pdfName;
	}

	internal Dictionary<IPdfPrimitive, PdfName> ObtainNames()
	{
		if (m_names == null)
		{
			m_names = new Dictionary<IPdfPrimitive, PdfName>();
		}
		IPdfPrimitive pdfPrimitive = base["Font"];
		if (pdfPrimitive != null)
		{
			PdfReferenceHolder pdfReferenceHolder = pdfPrimitive as PdfReferenceHolder;
			PdfDictionary pdfDictionary = pdfPrimitive as PdfDictionary;
			if (pdfReferenceHolder != null)
			{
				pdfDictionary = PdfCrossTable.Dereference(pdfPrimitive) as PdfDictionary;
			}
			if (pdfDictionary != null)
			{
				foreach (KeyValuePair<PdfName, IPdfPrimitive> item in pdfDictionary.Items)
				{
					if (item.Value != null)
					{
						IPdfPrimitive key = PdfCrossTable.Dereference(item.Value);
						PdfName value = null;
						m_names.TryGetValue(item.Key, out value);
						if (value == null)
						{
							PdfName key2 = item.Key;
							m_names[key] = key2;
						}
					}
				}
			}
		}
		return m_names;
	}

	internal void RequireProcSet(string procSetName)
	{
		if (procSetName == null)
		{
			throw new ArgumentNullException("procSetName");
		}
		PdfArray pdfArray = base["ProcSet"] as PdfArray;
		if (pdfArray == null)
		{
			pdfArray = (PdfArray)(base["ProcSet"] = new PdfArray());
		}
		PdfName element = new PdfName(procSetName);
		if (!pdfArray.Contains(element))
		{
			pdfArray.Add(element);
		}
	}

	private string GenerateName()
	{
		return Guid.NewGuid().ToString();
	}

	private string GenerateName(IPdfWrapper obj)
	{
		if (obj is PdfFont)
		{
			PdfDictionary resourceDictionary = FetchResourceDictionary("Font");
			int fontCounter = m_fontCounter;
			return SetResourceName(m_fontName, resourceDictionary, fontCounter, out m_fontCounter);
		}
		if (obj is PdfTemplate)
		{
			PdfDictionary resourceDictionary2 = FetchResourceDictionary("XObject");
			int templateCounter = m_templateCounter;
			return SetResourceName(m_templateName, resourceDictionary2, templateCounter, out m_templateCounter);
		}
		if (obj is PdfImage)
		{
			PdfDictionary resourceDictionary3 = FetchResourceDictionary("XObject");
			int imageCounter = m_imageCounter;
			return SetResourceName(m_imageName, resourceDictionary3, imageCounter, out m_imageCounter);
		}
		if (obj is PdfBrush)
		{
			PdfDictionary resourceDictionary4 = FetchResourceDictionary("Pattern");
			int brushCounter = m_brushCounter;
			return SetResourceName(m_brushName, resourceDictionary4, brushCounter, out m_brushCounter);
		}
		if (obj is PdfTransparency)
		{
			PdfDictionary resourceDictionary5 = FetchResourceDictionary("ExtGState");
			int transparencyCounter = m_transparencyCounter;
			return SetResourceName(m_transparencyName, resourceDictionary5, transparencyCounter, out m_transparencyCounter);
		}
		if (obj is PdfColorSpaces)
		{
			PdfDictionary resourceDictionary6 = FetchResourceDictionary("ColorSpace");
			int colorSpaceCounter = m_colorSpaceCounter;
			return SetResourceName(m_colorSpaceName, resourceDictionary6, colorSpaceCounter, out m_colorSpaceCounter);
		}
		if (obj is PdfDictionary)
		{
			PdfDictionary resourceDictionary7 = FetchResourceDictionary("ColorSpace");
			int dColorSpaceCounter = m_dColorSpaceCounter;
			return SetResourceName(m_dColorSpaceName, resourceDictionary7, dColorSpaceCounter, out m_dColorSpaceCounter);
		}
		return Guid.NewGuid().ToString();
	}

	private string SetResourceName(string name, PdfDictionary resourceDictionary, int counter, out int updatedCounter)
	{
		string text = "";
		updatedCounter = counter;
		text = name + updatedCounter;
		if (resourceDictionary != null)
		{
			while (resourceDictionary.ContainsKey(text))
			{
				updatedCounter++;
				text = name + updatedCounter;
			}
		}
		updatedCounter++;
		return text;
	}

	private PdfDictionary FetchResourceDictionary(string DictionaryProperty)
	{
		if (base[DictionaryProperty] is PdfReferenceHolder)
		{
			return (base[DictionaryProperty] as PdfReferenceHolder).Object as PdfDictionary;
		}
		return base[DictionaryProperty] as PdfDictionary;
	}

	private void Add(IPdfWrapper obj, PdfName name)
	{
		if (obj is PdfFont font)
		{
			Add(font, name);
			return;
		}
		if (obj is PdfTemplate template)
		{
			Add(template, name);
			return;
		}
		if (obj is PdfImage image)
		{
			Add(image, name);
			return;
		}
		if (obj is PdfBrush brush)
		{
			Add(brush, name);
			return;
		}
		if (obj is PdfTransparency transparancy)
		{
			Add(transparancy, name);
			return;
		}
		PdfColorSpaces pdfColorSpaces = obj as PdfColorSpaces;
		if (pdfColorSpaces != null)
		{
			Add(pdfColorSpaces, name);
		}
		else if (obj is PdfDictionary)
		{
			Add(pdfColorSpaces, name);
		}
	}

	internal void Add(PdfFont font, PdfName name)
	{
		PdfDictionary pdfDictionary = null;
		IPdfPrimitive pdfPrimitive = base["Font"];
		if (pdfPrimitive == null)
		{
			pdfDictionary = (PdfDictionary)(base["Font"] = new PdfDictionary());
		}
		else
		{
			PdfReferenceHolder pdfReferenceHolder = pdfPrimitive as PdfReferenceHolder;
			pdfDictionary = pdfPrimitive as PdfDictionary;
			if (pdfReferenceHolder != null)
			{
				pdfDictionary = PdfCrossTable.Dereference(pdfPrimitive) as PdfDictionary;
			}
			if (pdfDictionary == null)
			{
				pdfDictionary = (PdfDictionary)(base["Font"] = new PdfDictionary());
			}
		}
		pdfDictionary[name] = new PdfReferenceHolder(((IPdfWrapper)font).Element);
	}

	internal void AddProperties(string layerid, PdfReferenceHolder reff)
	{
		m_properties[layerid] = reff;
		base["Properties"] = m_properties;
	}

	private void Add(PdfTemplate template, PdfName name)
	{
		PdfDictionary pdfDictionary = ((!(base["XObject"] is PdfReferenceHolder)) ? (base["XObject"] as PdfDictionary) : ((base["XObject"] as PdfReferenceHolder).Object as PdfDictionary));
		if (pdfDictionary == null)
		{
			pdfDictionary = (PdfDictionary)(base["XObject"] = new PdfDictionary());
		}
		pdfDictionary[name] = new PdfReferenceHolder(((IPdfWrapper)template).Element);
	}

	private void Add(PdfImage image, PdfName name)
	{
		PdfDictionary pdfDictionary = base["XObject"] as PdfDictionary;
		PdfReferenceHolder pdfReferenceHolder = base["XObject"] as PdfReferenceHolder;
		PdfDictionary pdfDictionary2 = new PdfDictionary();
		if (pdfDictionary == null && pdfReferenceHolder != null)
		{
			pdfDictionary2 = pdfReferenceHolder.Object as PdfDictionary;
		}
		if (pdfDictionary == null)
		{
			pdfDictionary = (PdfDictionary)(base["XObject"] = new PdfDictionary());
			foreach (KeyValuePair<PdfName, IPdfPrimitive> item in pdfDictionary2.Items)
			{
				pdfDictionary[item.Key] = item.Value as PdfReferenceHolder;
			}
		}
		pdfDictionary[name] = new PdfReferenceHolder(((IPdfWrapper)image).Element);
	}

	private void Add(PdfBrush brush, PdfName name)
	{
		IPdfPrimitive element = (brush as IPdfWrapper).Element;
		if (element != null)
		{
			PdfDictionary pdfDictionary = base["Pattern"] as PdfDictionary;
			if (pdfDictionary == null)
			{
				pdfDictionary = (PdfDictionary)(base["Pattern"] = new PdfDictionary());
			}
			pdfDictionary[name] = new PdfReferenceHolder(element);
		}
	}

	private void Add(PdfTransparency transparancy, PdfName name)
	{
		IPdfPrimitive element = ((IPdfWrapper)transparancy).Element;
		if (element != null)
		{
			PdfDictionary pdfDictionary = null;
			if (base["ExtGState"] is PdfDictionary)
			{
				pdfDictionary = base["ExtGState"] as PdfDictionary;
			}
			else if (base["ExtGState"] is PdfReferenceHolder)
			{
				pdfDictionary = (base["ExtGState"] as PdfReferenceHolder).Object as PdfDictionary;
			}
			if (pdfDictionary == null)
			{
				pdfDictionary = (PdfDictionary)(base["ExtGState"] = new PdfDictionary());
			}
			pdfDictionary[name] = new PdfReferenceHolder(element);
		}
	}

	internal void Add(PdfColorSpaces color, PdfName name)
	{
		PdfDictionary pdfDictionary = null;
		IPdfPrimitive pdfPrimitive = base["ColorSpace"];
		if (pdfPrimitive == null)
		{
			pdfDictionary = (PdfDictionary)(base["ColorSpace"] = new PdfDictionary());
		}
		else
		{
			PdfReferenceHolder pdfReferenceHolder = pdfPrimitive as PdfReferenceHolder;
			pdfDictionary = pdfPrimitive as PdfDictionary;
			if (pdfReferenceHolder != null)
			{
				pdfDictionary = PdfCrossTable.Dereference(pdfPrimitive) as PdfDictionary;
			}
		}
		pdfDictionary[name] = new PdfReferenceHolder(((IPdfWrapper)color).Element);
	}

	internal void Add(PdfDictionary color, PdfName name)
	{
		PdfDictionary pdfDictionary = null;
		IPdfPrimitive pdfPrimitive = base["ColorSpace"];
		if (pdfPrimitive == null)
		{
			pdfDictionary = (PdfDictionary)(base["ColorSpace"] = new PdfDictionary());
		}
		else
		{
			PdfReferenceHolder pdfReferenceHolder = pdfPrimitive as PdfReferenceHolder;
			pdfDictionary = pdfPrimitive as PdfDictionary;
			if (pdfReferenceHolder != null)
			{
				pdfDictionary = PdfCrossTable.Dereference(pdfPrimitive) as PdfDictionary;
			}
		}
		pdfDictionary[name] = new PdfReferenceHolder(((IPdfWrapper)color).Element);
	}

	internal void RemoveFont(string name)
	{
		IPdfPrimitive pdfPrimitive = null;
		if (m_names != null)
		{
			foreach (KeyValuePair<IPdfPrimitive, PdfName> name2 in m_names)
			{
				if (name2.Value == new PdfName(name))
				{
					pdfPrimitive = name2.Key;
					break;
				}
			}
		}
		if (pdfPrimitive != null)
		{
			m_names.Remove(pdfPrimitive);
		}
	}
}
