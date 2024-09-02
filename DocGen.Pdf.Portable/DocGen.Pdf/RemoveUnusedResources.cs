using System.Collections.Generic;
using DocGen.Pdf.IO;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf;

internal class RemoveUnusedResources
{
	private HashSet<PdfName> m_colorSpaces = new HashSet<PdfName>();

	private HashSet<PdfName> m_xObject = new HashSet<PdfName>();

	private HashSet<PdfName> m_fonts = new HashSet<PdfName>();

	private HashSet<PdfName> m_patterns = new HashSet<PdfName>();

	private string m_currrentFont;

	internal HashSet<PdfName> ColorSpaces
	{
		get
		{
			return m_colorSpaces;
		}
		set
		{
			m_colorSpaces = value;
		}
	}

	internal HashSet<PdfName> XObjects
	{
		get
		{
			return m_xObject;
		}
		set
		{
			m_xObject = value;
		}
	}

	internal HashSet<PdfName> Fonts
	{
		get
		{
			return m_fonts;
		}
		set
		{
			m_fonts = value;
		}
	}

	internal HashSet<PdfName> Patterns
	{
		get
		{
			return m_patterns;
		}
		set
		{
			m_patterns = value;
		}
	}

	internal RemoveUnusedResources()
	{
		ColorSpaces = new HashSet<PdfName>();
		XObjects = new HashSet<PdfName>();
		Fonts = new HashSet<PdfName>();
		Patterns = new HashSet<PdfName>();
	}

	internal PdfDictionary RemoveUnusedResource(PdfRecordCollection recordCollection, PdfDictionary resources)
	{
		if (recordCollection != null)
		{
			foreach (PdfRecord item in recordCollection)
			{
				_ = string.Empty;
				switch (item.OperatorName.Trim())
				{
				case "CS":
				case "cs":
					if (ColorSpaces.Contains(new PdfName(item.Operands[0].TrimStart('/'))))
					{
						ColorSpaces.Add(new PdfName(item.Operands[0].TrimStart('/')));
					}
					break;
				case "SCN":
				case "scn":
					if (!Patterns.Contains(new PdfName(item.Operands[0].TrimStart('/'))))
					{
						Patterns.Add(new PdfName(item.Operands[0].TrimStart('/')));
					}
					break;
				case "Do":
					if (!XObjects.Contains(new PdfName(item.Operands[0].TrimStart('/'))))
					{
						XObjects.Add(new PdfName(item.Operands[0].TrimStart('/')));
					}
					break;
				case "Tf":
					if (!Fonts.Contains(new PdfName(item.Operands[0].TrimStart('/'))))
					{
						Fonts.Add(new PdfName(item.Operands[0].TrimStart('/')));
					}
					break;
				}
			}
		}
		resources = CleanUp(resources);
		return resources;
	}

	private void RenderFont(string[] fontElements)
	{
		for (int i = 0; i < fontElements.Length; i++)
		{
			if (fontElements[i].Contains("/"))
			{
				m_currrentFont = fontElements[i].Replace("/", "");
				break;
			}
		}
		Fonts.Add(new PdfName(m_currrentFont));
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

	internal PdfDictionary CleanUp(PdfDictionary resourceDic)
	{
		PdfDictionary pdfDictionary = CopyResources(resourceDic);
		if (pdfDictionary != null)
		{
			CleanUpResources(PdfCrossTable.Dereference(pdfDictionary["ColorSpace"]) as PdfDictionary, ColorSpaces);
			CleanUpResources(PdfCrossTable.Dereference(pdfDictionary["XObject"]) as PdfDictionary, XObjects);
			CleanUpResources(PdfCrossTable.Dereference(pdfDictionary["Font"]) as PdfDictionary, Fonts);
			CleanUpResources(PdfCrossTable.Dereference(pdfDictionary["Pattern"]) as PdfDictionary, Patterns);
		}
		ColorSpaces.Clear();
		Fonts.Clear();
		Patterns.Clear();
		XObjects.Clear();
		return pdfDictionary;
	}

	private void CleanUpResources(PdfDictionary resourcesDict, HashSet<PdfName> usedResources)
	{
		if (resourcesDict == null)
		{
			return;
		}
		List<PdfName> list = new List<PdfName>();
		foreach (PdfName key in resourcesDict.Keys)
		{
			list.Add(key);
		}
		foreach (PdfName item in list)
		{
			if (!usedResources.Contains(item))
			{
				resourcesDict.Remove(item);
			}
		}
	}

	private PdfDictionary CopyResources(PdfDictionary resourcesDict)
	{
		PdfDictionary pdfDictionary = new PdfDictionary();
		if (resourcesDict != null)
		{
			if (resourcesDict.ContainsKey("Font"))
			{
				PdfDictionary dict = PdfCrossTable.Dereference(resourcesDict["Font"]) as PdfDictionary;
				pdfDictionary["Font"] = CopyDictionary(dict);
			}
			if (resourcesDict.ContainsKey("XObject"))
			{
				PdfDictionary dict2 = PdfCrossTable.Dereference(resourcesDict["XObject"]) as PdfDictionary;
				pdfDictionary["XObject"] = CopyDictionary(dict2);
			}
			if (resourcesDict.ContainsKey("ColorSpace"))
			{
				PdfDictionary dict3 = PdfCrossTable.Dereference(resourcesDict["ColorSpace"]) as PdfDictionary;
				pdfDictionary["ColorSpace"] = CopyDictionary(dict3);
			}
			if (resourcesDict.ContainsKey("Pattern"))
			{
				PdfDictionary dict4 = PdfCrossTable.Dereference(resourcesDict["Pattern"]) as PdfDictionary;
				pdfDictionary["Pattern"] = CopyDictionary(dict4);
			}
			if (resourcesDict.ContainsKey("Shading"))
			{
				PdfDictionary value = PdfCrossTable.Dereference(resourcesDict["Shading"]) as PdfDictionary;
				pdfDictionary["Shading"] = value;
			}
			if (resourcesDict.ContainsKey("Properties"))
			{
				PdfDictionary value2 = PdfCrossTable.Dereference(resourcesDict["Properties"]) as PdfDictionary;
				pdfDictionary["Properties"] = value2;
			}
			if (resourcesDict.ContainsKey("ExtGState"))
			{
				PdfDictionary value3 = PdfCrossTable.Dereference(resourcesDict["ExtGState"]) as PdfDictionary;
				pdfDictionary["ExtGState"] = value3;
			}
		}
		return pdfDictionary;
	}

	private PdfDictionary CopyDictionary(PdfDictionary dict)
	{
		PdfDictionary pdfDictionary = new PdfDictionary();
		if (dict != null)
		{
			foreach (PdfName key in dict.Keys)
			{
				pdfDictionary[key] = dict[key];
			}
		}
		return pdfDictionary;
	}
}
