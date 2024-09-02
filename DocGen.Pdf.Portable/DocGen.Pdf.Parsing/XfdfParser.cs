using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Interactive;
using DocGen.Pdf.IO;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Parsing;

internal class XfdfParser
{
	private PdfLoadedDocument m_document;

	private Stream m_stream;

	private XDocument m_xmlDocument;

	private string m_richTextPrefix = "<?xml version=\"1.0\"?>";

	private Dictionary<string, PdfReferenceHolder> m_groupReferences = new Dictionary<string, PdfReferenceHolder>();

	private List<PdfDictionary> m_groupHolders = new List<PdfDictionary>();

	internal XfdfParser(Stream stream, PdfLoadedDocument document)
	{
		if (document != null)
		{
			m_document = document;
			m_stream = stream;
			m_xmlDocument = XDocument.Load(stream);
			return;
		}
		throw new ArgumentNullException("document");
	}

	internal void ParseAndImportAnnotationData()
	{
		if (m_xmlDocument == null || m_xmlDocument.NodeType != XmlNodeType.Document)
		{
			return;
		}
		XElement root = m_xmlDocument.Root;
		if (root != null && root.NodeType == XmlNodeType.Element)
		{
			CheckXfdf(root);
			foreach (XElement item in root.Elements())
			{
				XName name = item.Name;
				if (!(name != null) || !(name.LocalName == "Annots".ToLower()) || !item.HasElements)
				{
					continue;
				}
				foreach (XElement item2 in item.Elements())
				{
					if (item2.NodeType == XmlNodeType.Element)
					{
						ParseAnnotationData(item2);
					}
				}
			}
		}
		if (m_groupHolders.Count > 0)
		{
			foreach (PdfDictionary groupHolder in m_groupHolders)
			{
				if (groupHolder["IRT"] is PdfString pdfString && !string.IsNullOrEmpty(pdfString.Value))
				{
					if (m_groupReferences.ContainsKey(pdfString.Value))
					{
						groupHolder["IRT"] = m_groupReferences[pdfString.Value];
					}
					else
					{
						groupHolder.Remove("IRT");
					}
				}
			}
		}
		m_groupReferences.Clear();
		m_groupHolders.Clear();
	}

	private void ParseAnnotationData(XElement chileElement)
	{
		int result = -1;
		if (chileElement == null || !chileElement.HasAttributes)
		{
			return;
		}
		XAttribute xAttribute = chileElement.Attribute("Page".ToLower());
		if (xAttribute == null || string.IsNullOrEmpty(xAttribute.Value))
		{
			return;
		}
		int.TryParse(xAttribute.Value, out result);
		if (result < 0 || result >= m_document.Pages.Count)
		{
			return;
		}
		(m_document.Pages[result] as PdfLoadedPage).importAnnotation = true;
		PdfDictionary annotationData = GetAnnotationData(chileElement, result);
		if (annotationData.Count <= 0)
		{
			return;
		}
		PdfReferenceHolder pdfReferenceHolder = new PdfReferenceHolder(annotationData);
		if (annotationData.ContainsKey("NM") || annotationData.ContainsKey("IRT"))
		{
			AddReferenceToGroup(pdfReferenceHolder, annotationData);
		}
		PdfDictionary dictionary = m_document.Pages[result].Dictionary;
		if (dictionary != null)
		{
			if (!dictionary.ContainsKey("Annots"))
			{
				dictionary["Annots"] = new PdfArray();
			}
			IPdfPrimitive pdfPrimitive = dictionary["Annots"];
			if (((pdfPrimitive is PdfReferenceHolder) ? (pdfPrimitive as PdfReferenceHolder).Object : pdfPrimitive) is PdfArray pdfArray)
			{
				pdfArray.Elements.Add(pdfReferenceHolder);
				HandlePopUp(pdfArray, pdfReferenceHolder, annotationData);
				pdfArray.MarkChanged();
			}
		}
	}

	private PdfStream GetStream(XElement element)
	{
		PdfStream pdfStream = new PdfStream();
		if (element.HasElements)
		{
			foreach (XElement item in element.Elements())
			{
				GetAppearance(pdfStream, item);
			}
		}
		return pdfStream;
	}

	private PdfDictionary GetDictionary(XElement element)
	{
		PdfDictionary pdfDictionary = new PdfDictionary();
		if (element.HasElements)
		{
			foreach (XElement item in element.Elements())
			{
				GetAppearance(pdfDictionary, item);
			}
		}
		return pdfDictionary;
	}

	private PdfArray GetArray(XElement element)
	{
		PdfArray pdfArray = new PdfArray();
		if (element.HasElements)
		{
			foreach (XElement item in element.Elements())
			{
				AddArrayElements(pdfArray, item);
			}
		}
		return pdfArray;
	}

	private PdfNumber GetFixed(XElement element)
	{
		if (element.HasAttributes)
		{
			XAttribute xAttribute = element.Attribute("VAL");
			if (xAttribute != null && float.TryParse(xAttribute.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
			{
				return new PdfNumber(result);
			}
		}
		return null;
	}

	private PdfNumber GetInt(XElement element)
	{
		if (element.HasAttributes)
		{
			XAttribute xAttribute = element.Attribute("VAL");
			if (xAttribute != null && int.TryParse(xAttribute.Value, out var result))
			{
				return new PdfNumber(result);
			}
		}
		return null;
	}

	private PdfString GetString(XElement element)
	{
		if (element.HasAttributes)
		{
			XAttribute xAttribute = element.Attribute("VAL");
			if (xAttribute != null)
			{
				return new PdfString(xAttribute.Value);
			}
			if (element.Name.ToString().Contains("STRING"))
			{
				return new PdfString(GetData(element));
			}
		}
		return null;
	}

	private PdfName GetName(XElement element)
	{
		if (element.HasAttributes)
		{
			XAttribute xAttribute = element.Attribute("VAL");
			if (xAttribute != null && !string.IsNullOrEmpty(xAttribute.Value))
			{
				return new PdfName(xAttribute.Value);
			}
		}
		return null;
	}

	private PdfBoolean GetBoolean(XElement element)
	{
		if (element.HasAttributes)
		{
			XAttribute xAttribute = element.Attribute("VAL");
			if (xAttribute != null && !string.IsNullOrEmpty(xAttribute.Value))
			{
				return new PdfBoolean(xAttribute.Value.ToLower() == "true");
			}
		}
		return null;
	}

	private byte[] GetData(XElement element)
	{
		if (!string.IsNullOrEmpty(element.Value) && element.HasAttributes)
		{
			XAttribute xAttribute = element.Attribute("MODE");
			XAttribute xAttribute2 = element.Attribute("ENCODING");
			if (xAttribute != null && xAttribute2 != null)
			{
				if (xAttribute.Value == "FILTERED" && xAttribute2.Value == "ASCII")
				{
					return Encoding.UTF8.GetBytes(element.Value);
				}
				if (xAttribute.Value == "RAW" && xAttribute2.Value == "HEX")
				{
					return GetBytes(element.Value);
				}
			}
			else if (xAttribute2 != null && xAttribute2.Value == "HEX")
			{
				return GetBytes(element.Value);
			}
		}
		return null;
	}

	private PdfDictionary GetAppearance(PdfDictionary appearance, XElement element)
	{
		if (element != null)
		{
			XName name = element.Name;
			if (name.LocalName != null)
			{
				switch (name.LocalName)
				{
				case "STREAM":
				{
					PdfStream stream = GetStream(element);
					if (stream != null)
					{
						PdfReferenceHolder primitive = new PdfReferenceHolder(stream);
						AddKey(primitive, appearance, element);
					}
					break;
				}
				case "DICT":
				{
					PdfDictionary dictionary = GetDictionary(element);
					if (dictionary != null)
					{
						PdfReferenceHolder primitive2 = new PdfReferenceHolder(dictionary);
						AddKey(primitive2, appearance, element);
					}
					break;
				}
				case "ARRAY":
				{
					PdfArray array = GetArray(element);
					AddKey(array, appearance, element);
					break;
				}
				case "FIXED":
				{
					PdfNumber @fixed = GetFixed(element);
					AddKey(@fixed, appearance, element);
					break;
				}
				case "INT":
				{
					PdfNumber @int = GetInt(element);
					AddKey(@int, appearance, element);
					break;
				}
				case "STRING":
				{
					PdfString @string = GetString(element);
					AddKey(@string, appearance, element);
					break;
				}
				case "NAME":
				{
					PdfName name2 = GetName(element);
					AddKey(name2, appearance, element);
					break;
				}
				case "BOOL":
				{
					PdfBoolean boolean = GetBoolean(element);
					AddKey(boolean, appearance, element);
					break;
				}
				case "DATA":
				{
					byte[] data = GetData(element);
					if (data == null || data.Length == 0 || !(appearance is PdfStream pdfStream))
					{
						break;
					}
					pdfStream.Data = data;
					if (!pdfStream.ContainsKey("Type") && !pdfStream.ContainsKey("Subtype"))
					{
						pdfStream.Decompress();
					}
					bool flag = false;
					if (pdfStream.ContainsKey("Subtype"))
					{
						PdfName pdfName = pdfStream["Subtype"] as PdfName;
						if (pdfName != null && pdfName.Value == "Image")
						{
							flag = true;
						}
					}
					if (flag)
					{
						pdfStream.Compress = false;
						break;
					}
					if (pdfStream.ContainsKey("Length"))
					{
						pdfStream.Remove("Length");
					}
					if (pdfStream.ContainsKey("Filter"))
					{
						pdfStream.Remove("Filter");
					}
					break;
				}
				}
			}
		}
		return appearance;
	}

	private void AddBorderStyle(PdfDictionary annotDictionary, XElement element)
	{
		if (!element.HasAttributes)
		{
			return;
		}
		PdfDictionary pdfDictionary = new PdfDictionary();
		PdfDictionary pdfDictionary2 = new PdfDictionary();
		XAttribute xAttribute = element.Attribute("width");
		if (xAttribute != null && float.TryParse(xAttribute.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
		{
			pdfDictionary2.SetNumber("W", result);
		}
		bool flag = true;
		xAttribute = element.Attribute("style");
		if (xAttribute != null)
		{
			string text = string.Empty;
			switch (xAttribute.Value)
			{
			case "dash":
				text = "D";
				break;
			case "solid":
				text = "S";
				break;
			case "bevelled":
				text = "B";
				break;
			case "inset":
				text = "I";
				break;
			case "underline":
				text = "U";
				break;
			case "cloudy":
				text = "C";
				flag = false;
				break;
			}
			if (!string.IsNullOrEmpty(text))
			{
				(flag ? pdfDictionary2 : pdfDictionary).SetName("S", text);
				xAttribute = element.Attribute("intensity");
				if (!flag && xAttribute != null)
				{
					if (float.TryParse(xAttribute.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result2))
					{
						pdfDictionary.SetNumber("I", result2);
					}
				}
				else
				{
					xAttribute = element.Attribute("dashes");
					if (xAttribute != null)
					{
						float[] array = ObtainFloatPoints(xAttribute.Value);
						if (array != null && array.Length != 0)
						{
							pdfDictionary2.SetProperty("D", new PdfArray(array));
						}
					}
				}
			}
		}
		if (pdfDictionary.Count > 0)
		{
			annotDictionary.SetProperty("BE", new PdfReferenceHolder(pdfDictionary));
		}
		else
		{
			pdfDictionary.Clear();
			pdfDictionary.IsSaving = false;
			pdfDictionary = null;
		}
		if (pdfDictionary2.Count > 0)
		{
			pdfDictionary2.SetProperty("Type", new PdfName("Border"));
			annotDictionary.SetProperty("BS", new PdfReferenceHolder(pdfDictionary2));
		}
		else
		{
			pdfDictionary2.Clear();
			pdfDictionary2.IsSaving = false;
			pdfDictionary2 = null;
		}
	}

	private void AddMeasureDictionary(PdfDictionary annotDictionary, XElement element)
	{
		XElement xElement = null;
		XElement xElement2 = null;
		XElement xElement3 = null;
		XElement xElement4 = null;
		foreach (XElement item in element.Elements())
		{
			if (item.NodeType == XmlNodeType.Element)
			{
				XName name = item.Name;
				if (name != null && name.LocalName.ToLower() == "measure")
				{
					xElement = item;
					break;
				}
			}
		}
		PdfDictionary pdfDictionary = new PdfDictionary();
		PdfArray pdfArray = new PdfArray();
		PdfArray pdfArray2 = new PdfArray();
		PdfArray pdfArray3 = new PdfArray();
		PdfDictionary pdfDictionary2 = new PdfDictionary();
		PdfDictionary pdfDictionary3 = new PdfDictionary();
		PdfDictionary pdfDictionary4 = new PdfDictionary();
		pdfDictionary.Items.Add(new PdfName("A"), pdfArray2);
		pdfDictionary.Items.Add(new PdfName("D"), pdfArray);
		pdfDictionary.Items.Add(new PdfName("X"), pdfArray3);
		if (xElement != null)
		{
			pdfDictionary.SetName("Type", "Measure");
			if (xElement.HasAttributes)
			{
				XAttribute xAttribute = xElement.Attribute("rateValue");
				if (xAttribute != null)
				{
					pdfDictionary.SetString("R", xAttribute.Value);
				}
			}
			foreach (XElement item2 in xElement.Elements())
			{
				if (item2.NodeType == XmlNodeType.Element)
				{
					XName name2 = item2.Name;
					if (name2 != null && name2.LocalName.ToLower() == "area")
					{
						xElement2 = item2;
					}
					if (name2 != null && name2.LocalName.ToLower() == "distance")
					{
						xElement3 = item2;
					}
					if (name2 != null && name2.LocalName.ToLower() == "xformat")
					{
						xElement4 = item2;
					}
				}
			}
		}
		if (xElement4 != null)
		{
			AddElements(xElement4, pdfDictionary4);
			pdfArray3.Add(pdfDictionary4);
		}
		if (xElement2 != null)
		{
			AddElements(xElement2, pdfDictionary3);
			pdfArray2.Add(pdfDictionary3);
		}
		if (xElement3 != null)
		{
			AddElements(xElement3, pdfDictionary2);
			pdfArray.Add(pdfDictionary2);
		}
		if (pdfDictionary.Items.Count > 0 && pdfDictionary.ContainsKey("Type"))
		{
			annotDictionary.Items.Add(new PdfName("Measure"), new PdfReferenceHolder(pdfDictionary));
		}
	}

	private void ParseInnerElements(PdfDictionary annotDictionary, XElement element, int pageIndex)
	{
		if (!element.HasElements)
		{
			return;
		}
		foreach (XElement item in element.Elements())
		{
			if (item.NodeType != XmlNodeType.Element)
			{
				continue;
			}
			XName name = item.Name;
			if (!(name != null))
			{
				continue;
			}
			switch (name.LocalName.ToLower())
			{
			case "popup":
			{
				if (!item.HasAttributes)
				{
					break;
				}
				PdfDictionary annotationData = GetAnnotationData(item, pageIndex);
				if (annotationData.Count > 0)
				{
					PdfReferenceHolder pdfReferenceHolder = new PdfReferenceHolder(annotationData);
					annotDictionary.SetProperty("Popup", pdfReferenceHolder);
					if (annotationData.ContainsKey("NM"))
					{
						AddReferenceToGroup(pdfReferenceHolder, annotationData);
					}
				}
				break;
			}
			case "contents":
			{
				string text = item.Value.ToString();
				if (!string.IsNullOrEmpty(text))
				{
					text = text.Replace("&lt;", "<");
					text = text.Replace("&gt;", ">");
					annotDictionary.SetString("Contents", text);
				}
				break;
			}
			case "contents-richtext":
			{
				XNode firstNode = item.FirstNode;
				if (firstNode != null)
				{
					string text2 = firstNode.ToString(SaveOptions.DisableFormatting);
					string str = item.Value.ToString();
					if (!string.IsNullOrEmpty(text2) && !annotDictionary.ContainsKey("Contents"))
					{
						annotDictionary.SetString("RC", m_richTextPrefix + text2);
						annotDictionary.SetString("Contents", str);
					}
					else if (!string.IsNullOrEmpty(text2))
					{
						annotDictionary.SetString("RC", m_richTextPrefix + text2);
					}
				}
				break;
			}
			case "defaultstyle":
				AddString(annotDictionary, "DS", item.Value);
				break;
			case "defaultappearance":
				AddString(annotDictionary, "DA", item.Value);
				break;
			case "vertices":
			{
				if (string.IsNullOrEmpty(item.Value))
				{
					break;
				}
				string[] array2 = item.Value.Split(',', ';');
				if (array2 != null && array2.Length != 0)
				{
					List<float> list = new List<float>();
					string[] array3 = array2;
					foreach (string value3 in array3)
					{
						AddFloatPoints(list, value3);
					}
					if (list.Count > 0 && list.Count % 2 == 0)
					{
						annotDictionary.SetProperty("Vertices", new PdfArray(list.ToArray()));
					}
				}
				break;
			}
			case "appearance":
			{
				if (string.IsNullOrEmpty(item.Value))
				{
					break;
				}
				byte[] array = Convert.FromBase64String(item.Value);
				if (array == null || array.Length == 0)
				{
					break;
				}
				XDocument xDocument = XDocument.Load(new MemoryStream(array));
				if (xDocument == null)
				{
					break;
				}
				IEnumerable<XElement> enumerable = xDocument.Elements();
				if (enumerable == null)
				{
					break;
				}
				foreach (XElement item2 in enumerable)
				{
					if (item2 == null || !item2.HasElements)
					{
						continue;
					}
					XName name2 = item2.Name;
					if (!(name2 != null) || !(name2.LocalName == "DICT"))
					{
						continue;
					}
					XAttribute xAttribute3 = item2.Attribute("KEY");
					if (xAttribute3 == null || string.IsNullOrEmpty(xAttribute3.Value) || !(xAttribute3.Value == "AP"))
					{
						continue;
					}
					PdfDictionary pdfDictionary4 = new PdfDictionary();
					foreach (XElement item3 in item2.Elements())
					{
						GetAppearance(pdfDictionary4, item3);
					}
					if (pdfDictionary4.Count > 0)
					{
						annotDictionary.SetProperty("AP", pdfDictionary4);
					}
				}
				break;
			}
			case "imagedata":
				AddImageToAppearance(annotDictionary, item.Value);
				break;
			case "inklist":
			{
				if (!item.HasElements)
				{
					break;
				}
				PdfArray pdfArray = new PdfArray();
				foreach (XElement item4 in item.Elements())
				{
					if (item4.NodeType != XmlNodeType.Element)
					{
						continue;
					}
					XName name3 = item4.Name;
					if (item4 == null || !(name3 != null) || !(name3.LocalName.ToLower() == "gesture") || string.IsNullOrEmpty(item4.Value))
					{
						continue;
					}
					string[] array4 = item4.Value.Split(',', ';');
					if (array4 != null && array4.Length != 0)
					{
						List<float> list2 = new List<float>();
						string[] array3 = array4;
						foreach (string value4 in array3)
						{
							AddFloatPoints(list2, value4);
						}
						if (list2.Count > 0 && list2.Count % 2 == 0)
						{
							pdfArray.Add(new PdfArray(list2.ToArray()));
						}
						list2.Clear();
					}
				}
				annotDictionary.SetProperty("InkList", pdfArray);
				break;
			}
			case "data":
			{
				if (string.IsNullOrEmpty(item.Value))
				{
					break;
				}
				byte[] bytes = GetBytes(item.Value);
				if (bytes == null || bytes.Length == 0 || !annotDictionary.ContainsKey("Subtype"))
				{
					break;
				}
				string value = (annotDictionary["Subtype"] as PdfName).Value;
				if (value == "FileAttachment")
				{
					PdfDictionary pdfDictionary = new PdfDictionary();
					pdfDictionary.SetName("Type", "Filespec");
					AddString(pdfDictionary, element, "file", "F");
					AddString(pdfDictionary, element, "file", "UF");
					PdfStream pdfStream = new PdfStream();
					PdfDictionary pdfDictionary2 = new PdfDictionary();
					XAttribute xAttribute = element.Attribute("size");
					if (xAttribute != null && int.TryParse(xAttribute.Value, out var result))
					{
						pdfDictionary2.SetNumber("Size", result);
						pdfStream.SetNumber("DL", result);
					}
					AddString(pdfDictionary2, element, "modification", "ModDate");
					AddString(pdfDictionary2, element, "creation", "CreationDate");
					pdfStream.SetProperty("Params", pdfDictionary2);
					AddString(pdfStream, element, "mimetype", "Subtype");
					pdfStream.Data = bytes;
					pdfStream.AddFilter("FlateDecode");
					PdfDictionary pdfDictionary3 = new PdfDictionary();
					pdfDictionary3.SetProperty("F", new PdfReferenceHolder(pdfStream));
					pdfDictionary.SetProperty("EF", pdfDictionary3);
					annotDictionary.SetProperty("FS", new PdfReferenceHolder(pdfDictionary));
				}
				else
				{
					if (!(value == "Sound"))
					{
						break;
					}
					PdfStream pdfStream2 = new PdfStream();
					pdfStream2.SetName("Type", "Sound");
					AddNumber(pdfStream2, element, "bits", "B");
					AddNumber(pdfStream2, element, "rate", "R");
					AddNumber(pdfStream2, element, "channels", "C");
					XAttribute xAttribute2 = element.Attribute("encoding");
					if (xAttribute2 != null)
					{
						string value2 = xAttribute2.Value;
						if (!string.IsNullOrEmpty(value2))
						{
							pdfStream2.SetName("E", value2);
						}
					}
					pdfStream2.Data = bytes;
					xAttribute2 = element.Attribute("filter");
					if (xAttribute2 != null)
					{
						pdfStream2.AddFilter("FlateDecode");
					}
					annotDictionary.SetProperty("Sound", new PdfReferenceHolder(pdfStream2));
				}
				break;
			}
			}
		}
	}

	private void AddNumber(PdfDictionary dictionary, XElement element, string attributeName, string key)
	{
		if (element.HasAttributes)
		{
			XAttribute xAttribute = element.Attribute(attributeName);
			if (xAttribute != null)
			{
				AddInt(dictionary, key, xAttribute.Value);
			}
		}
	}

	private void AddString(PdfDictionary dictionary, XElement element, string attributeName, string key)
	{
		if (element.HasAttributes)
		{
			XAttribute xAttribute = element.Attribute(attributeName);
			if (xAttribute != null)
			{
				AddString(dictionary, key, xAttribute.Value);
			}
		}
	}

	private void AddKey(IPdfPrimitive primitive, PdfDictionary dictionary, XElement element)
	{
		if (primitive != null && element.HasAttributes)
		{
			XAttribute xAttribute = element.Attribute("KEY");
			if (xAttribute != null)
			{
				dictionary.SetProperty(xAttribute.Value, primitive);
			}
		}
	}

	private PdfArray GetColorArray(string value)
	{
		string text = value.Replace("#", string.Empty);
		int num = int.Parse(text.Substring(0, 2), NumberStyles.HexNumber);
		int num2 = int.Parse(text.Substring(2, 2), NumberStyles.HexNumber);
		int num3 = int.Parse(text.Substring(4, 2), NumberStyles.HexNumber);
		if (num >= 0 && num2 >= 0 && num3 >= 0)
		{
			return new PdfArray
			{
				new PdfNumber((float)num / 255f),
				new PdfNumber((float)num2 / 255f),
				new PdfNumber((float)num3 / 255f)
			};
		}
		return null;
	}

	private void AddReferenceToGroup(PdfReferenceHolder holder, PdfDictionary dictionary)
	{
		PdfString pdfString = dictionary["NM"] as PdfString;
		if (pdfString != null && !string.IsNullOrEmpty(pdfString.Value))
		{
			if (m_groupReferences.ContainsKey(pdfString.Value))
			{
				m_groupReferences[pdfString.Value] = holder;
			}
			else
			{
				m_groupReferences.Add(pdfString.Value, holder);
			}
			if (dictionary.ContainsKey("IRT"))
			{
				m_groupHolders.Add(dictionary);
			}
		}
		else if (pdfString == null)
		{
			if (dictionary.ContainsKey("IRT"))
			{
				pdfString = dictionary["IRT"] as PdfString;
			}
			if (pdfString != null && !string.IsNullOrEmpty(pdfString.Value) && m_groupReferences.ContainsKey(pdfString.Value))
			{
				PdfReferenceHolder value = m_groupReferences[pdfString.Value];
				dictionary["IRT"] = value;
			}
		}
	}

	private void AddLineEndStyle(XElement element, PdfDictionary annotDictionary)
	{
		if (!element.HasAttributes)
		{
			return;
		}
		string text = string.Empty;
		XAttribute xAttribute = element.Attribute("head");
		if (xAttribute != null)
		{
			text = MapLineEndingStyle(xAttribute.Value).ToString();
		}
		string value = string.Empty;
		XAttribute xAttribute2 = element.Attribute("tail");
		if (xAttribute2 != null)
		{
			value = MapLineEndingStyle(xAttribute2.Value).ToString();
		}
		if (!string.IsNullOrEmpty(text))
		{
			if (!string.IsNullOrEmpty(value))
			{
				PdfArray pdfArray = new PdfArray();
				pdfArray.Add(new PdfName(text));
				pdfArray.Add(new PdfName(value));
				annotDictionary.SetProperty("LE", pdfArray);
			}
			else
			{
				annotDictionary.SetName("LE", text);
			}
		}
		else if (!string.IsNullOrEmpty(value))
		{
			annotDictionary.SetName("LE", text);
		}
	}

	private void AddAnnotationData(PdfDictionary annotDictionary, XElement element, int pageIndex)
	{
		AddBorderStyle(annotDictionary, element);
		ApplyAttributeValues(annotDictionary, element.Attributes());
		ParseInnerElements(annotDictionary, element, pageIndex);
		AddMeasureDictionary(annotDictionary, element);
	}

	private void AddLinePoints(List<float> linePoints, string value)
	{
		if (linePoints == null || string.IsNullOrEmpty(value) || !value.Contains(","))
		{
			return;
		}
		string[] array = value.Split(',');
		for (int i = 0; i < array.Length; i++)
		{
			if (float.TryParse(array[i], NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
			{
				linePoints.Add(result);
			}
		}
	}

	private float[] ObtainFloatPoints(string value)
	{
		List<float> list = new List<float>();
		if (!string.IsNullOrEmpty(value))
		{
			float result2;
			if (value.Contains(","))
			{
				string[] array = value.Split(',');
				for (int i = 0; i < array.Length; i++)
				{
					if (float.TryParse(array[i], NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
					{
						list.Add(result);
					}
				}
			}
			else if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out result2))
			{
				list.Add(result2);
			}
		}
		if (list.Count <= 0)
		{
			return null;
		}
		return list.ToArray();
	}

	private string GetFormatedString(string value)
	{
		if (!string.IsNullOrEmpty(value))
		{
			value = value.Replace("&lt;", "<");
			value = value.Replace("&gt;", ">");
		}
		return value;
	}

	private void HandlePopUp(PdfArray annots, PdfReferenceHolder holder, PdfDictionary annotDictionary)
	{
		if (annotDictionary.ContainsKey("Popup"))
		{
			PdfReferenceHolder pdfReferenceHolder = annotDictionary["Popup"] as PdfReferenceHolder;
			if (pdfReferenceHolder != null)
			{
				(pdfReferenceHolder.Object as PdfDictionary).SetProperty("Parent", holder);
				annots.Add(pdfReferenceHolder);
			}
		}
	}

	private void CheckXfdf(XElement element)
	{
		XName name = element.Name;
		if (name != null && name.LocalName.ToLower() != "xfdf")
		{
			throw new Exception("Cannot import XFdf file. File format has been corrupted");
		}
	}

	private void AddArrayElements(PdfArray array, XElement element)
	{
		if (element == null)
		{
			return;
		}
		XName name = element.Name;
		if (!(name != null))
		{
			return;
		}
		string localName = name.LocalName;
		if (localName == null)
		{
			return;
		}
		switch (localName.Length)
		{
		case 6:
			switch (localName[3])
			{
			case 'E':
				if (localName == "STREAM")
				{
					PdfStream stream = GetStream(element);
					if (stream != null)
					{
						AddArrayElement(array, new PdfReferenceHolder(stream));
					}
				}
				break;
			case 'I':
				if (localName == "STRING")
				{
					PdfString @string = GetString(element);
					AddArrayElement(array, @string);
				}
				break;
			}
			break;
		case 4:
			switch (localName[0])
			{
			case 'D':
				if (localName == "DICT")
				{
					PdfDictionary dictionary = GetDictionary(element);
					if (dictionary != null)
					{
						AddArrayElement(array, new PdfReferenceHolder(dictionary));
					}
				}
				break;
			case 'N':
				if (localName == "NAME")
				{
					PdfName name2 = GetName(element);
					AddArrayElement(array, name2);
				}
				break;
			case 'B':
				if (localName == "BOOL")
				{
					PdfBoolean boolean = GetBoolean(element);
					AddArrayElement(array, boolean);
				}
				break;
			}
			break;
		case 5:
			switch (localName[0])
			{
			case 'A':
				if (localName == "ARRAY")
				{
					PdfArray array2 = GetArray(element);
					AddArrayElement(array, array2);
				}
				break;
			case 'F':
				if (localName == "FIXED")
				{
					PdfNumber @fixed = GetFixed(element);
					AddArrayElement(array, @fixed);
				}
				break;
			}
			break;
		case 3:
			if (localName == "INT")
			{
				PdfNumber @int = GetInt(element);
				AddArrayElement(array, @int);
			}
			break;
		}
	}

	private void ApplyAttributeValues(PdfDictionary annotDictionary, IEnumerable<XAttribute> collection)
	{
		foreach (XAttribute item in collection)
		{
			string value = item.Value;
			XName name = item.Name;
			if (!(name != null))
			{
				continue;
			}
			switch (name.LocalName.ToLower())
			{
			case "state":
				AddString(annotDictionary, "State", value);
				break;
			case "statemodel":
				AddString(annotDictionary, "StateModel", value);
				break;
			case "replytype":
				if (value == "group")
				{
					annotDictionary.SetName("RT", "Group");
				}
				break;
			case "inreplyto":
				AddString(annotDictionary, "IRT", value);
				break;
			case "rect":
			{
				float[] array2 = ObtainFloatPoints(value);
				if (array2 != null && array2.Length == 4)
				{
					annotDictionary.SetProperty("Rect", new PdfArray(array2));
				}
				break;
			}
			case "color":
				if (!string.IsNullOrEmpty(value))
				{
					PdfArray colorArray2 = GetColorArray(value);
					if (colorArray2 != null)
					{
						annotDictionary.SetProperty("C", colorArray2);
					}
				}
				break;
			case "interior-color":
				if (!string.IsNullOrEmpty(value))
				{
					PdfArray colorArray = GetColorArray(value);
					if (colorArray != null)
					{
						annotDictionary.SetProperty("IC", colorArray);
					}
				}
				break;
			case "date":
				AddString(annotDictionary, "M", value);
				break;
			case "creationdate":
				AddString(annotDictionary, "CreationDate", value);
				break;
			case "name":
				AddString(annotDictionary, "NM", value);
				break;
			case "icon":
				if (!string.IsNullOrEmpty(value))
				{
					annotDictionary.SetName("Name", value);
				}
				break;
			case "subject":
				AddString(annotDictionary, "Subj", GetFormatedString(value));
				break;
			case "title":
				AddString(annotDictionary, "T", GetFormatedString(value));
				break;
			case "rotation":
				AddInt(annotDictionary, "Rotate", value);
				break;
			case "justification":
				AddInt(annotDictionary, "Q", value);
				break;
			case "fringe":
				AddFloatPoints(annotDictionary, ObtainFloatPoints(value), "RD");
				break;
			case "it":
				if (!string.IsNullOrEmpty(value))
				{
					annotDictionary.SetName("IT", value);
				}
				break;
			case "leaderlength":
				AddFloat(annotDictionary, "LL", value);
				break;
			case "leaderextend":
			{
				if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
				{
					annotDictionary.SetNumber("LLE", result);
				}
				break;
			}
			case "caption":
				if (!string.IsNullOrEmpty(value))
				{
					annotDictionary.SetBoolean("Cap", value.ToLower() == "yes");
				}
				break;
			case "caption-style":
				if (!string.IsNullOrEmpty(value))
				{
					annotDictionary.SetName("CP", value);
				}
				break;
			case "callout":
				AddFloatPoints(annotDictionary, ObtainFloatPoints(value), "CL");
				break;
			case "coords":
				AddFloatPoints(annotDictionary, ObtainFloatPoints(value), "QuadPoints");
				break;
			case "border":
				AddFloatPoints(annotDictionary, ObtainFloatPoints(value), "Border");
				break;
			case "opacity":
			{
				if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result2))
				{
					annotDictionary.SetNumber("CA", result2);
				}
				break;
			}
			case "flags":
			{
				if (string.IsNullOrEmpty(value))
				{
					break;
				}
				PdfAnnotationFlags pdfAnnotationFlags = PdfAnnotationFlags.Default;
				if (value.Contains(","))
				{
					string[] array = value.Split(',');
					for (int i = 0; i < array.Length; i++)
					{
						PdfAnnotationFlags pdfAnnotationFlags2 = MapAnnotationFlags(array[i]);
						pdfAnnotationFlags = ((i != 0) ? (pdfAnnotationFlags | pdfAnnotationFlags2) : pdfAnnotationFlags2);
					}
				}
				else
				{
					pdfAnnotationFlags = MapAnnotationFlags(value);
				}
				annotDictionary.SetNumber("F", (int)pdfAnnotationFlags);
				break;
			}
			case "open":
				if (!string.IsNullOrEmpty(value))
				{
					annotDictionary?.SetBoolean("Open", (value == "true" || value == "yes") ? true : false);
				}
				break;
			case "calibrate":
				AddString(annotDictionary, "Calibrate", value);
				break;
			case "customdata":
				AddString(annotDictionary, "CustomData", value);
				break;
			case "overlaytext":
				annotDictionary.SetString("OverlayText", value);
				break;
			case "repeat":
				annotDictionary.SetBoolean("Repeat", (value == "true" || value == "yes") ? true : false);
				break;
			default:
				if (m_document.ImportCustomData)
				{
					annotDictionary.SetString(name.LocalName, value);
				}
				break;
			case "page":
			case "head":
			case "tail":
			case "itex":
			case "start":
			case "style":
			case "width":
			case "intensity":
			case "end":
				break;
			}
		}
	}

	private PdfDictionary GetAnnotationData(XElement element, int pageIndex)
	{
		PdfDictionary pdfDictionary = new PdfDictionary();
		pdfDictionary.SetName("Type", "Annot");
		bool flag = true;
		XName name = element.Name;
		if (!string.IsNullOrEmpty(name.LocalName))
		{
			switch (name.LocalName.ToLower())
			{
			case "line":
			{
				pdfDictionary.SetName("Subtype", "Line");
				XAttribute xAttribute = element.Attribute("start");
				XAttribute xAttribute2 = element.Attribute("end");
				if (xAttribute != null && xAttribute2 != null)
				{
					List<float> list = new List<float>();
					AddLinePoints(list, xAttribute.Value);
					AddLinePoints(list, xAttribute2.Value);
					if (list.Count == 4)
					{
						pdfDictionary.SetProperty("L", new PdfArray(list.ToArray()));
					}
					list.Clear();
					list = null;
				}
				AddLineEndStyle(element, pdfDictionary);
				break;
			}
			case "circle":
				pdfDictionary.SetName("Subtype", "Circle");
				break;
			case "square":
				pdfDictionary.SetName("Subtype", "Square");
				break;
			case "polyline":
				pdfDictionary.SetName("Subtype", "PolyLine");
				AddLineEndStyle(element, pdfDictionary);
				break;
			case "polygon":
				pdfDictionary.SetName("Subtype", "Polygon");
				AddLineEndStyle(element, pdfDictionary);
				break;
			case "ink":
				pdfDictionary.SetName("Subtype", "Ink");
				break;
			case "popup":
				pdfDictionary.SetName("Subtype", "Popup");
				break;
			case "text":
				pdfDictionary.SetName("Subtype", "Text");
				break;
			case "freetext":
				pdfDictionary.SetName("Subtype", "FreeText");
				AddLineEndStyle(element, pdfDictionary);
				break;
			case "stamp":
				pdfDictionary.SetName("Subtype", "Stamp");
				break;
			case "highlight":
				pdfDictionary.SetName("Subtype", "Highlight");
				break;
			case "squiggly":
				pdfDictionary.SetName("Subtype", "Squiggly");
				break;
			case "underline":
				pdfDictionary.SetName("Subtype", "Underline");
				break;
			case "strikeout":
				pdfDictionary.SetName("Subtype", "StrikeOut");
				break;
			case "fileattachment":
				pdfDictionary.SetName("Subtype", "FileAttachment");
				break;
			case "sound":
				pdfDictionary.SetName("Subtype", "Sound");
				break;
			case "caret":
				pdfDictionary.SetName("Subtype", "Caret");
				break;
			case "redact":
				pdfDictionary.SetName("Subtype", "Redact");
				break;
			default:
				flag = false;
				break;
			}
			if (flag)
			{
				AddAnnotationData(pdfDictionary, element, pageIndex);
			}
		}
		return pdfDictionary;
	}

	private void AddImageToAppearance(PdfDictionary annotDictionary, string value)
	{
		MemoryStream memoryStream = new MemoryStream(Convert.FromBase64String(value.Replace("data:image/png;base64,", string.Empty).Replace("data:image/jpg;base64,", string.Empty).Replace("data:image/bmp;base64,", string.Empty)));
		PdfImage image = new PdfBitmap(memoryStream);
		memoryStream.Dispose();
		PdfArray obj = PdfCrossTable.Dereference(annotDictionary["Rect"]) as PdfArray;
		float x = 0f;
		float y = 0f;
		float floatValue = (obj[2] as PdfNumber).FloatValue;
		float floatValue2 = (obj[3] as PdfNumber).FloatValue;
		PdfTemplate pdfTemplate = new PdfTemplate(new RectangleF(x, y, floatValue, floatValue2));
		SetMatrix(pdfTemplate.m_content, annotDictionary);
		pdfTemplate.Graphics.DrawImage(image, x, y, floatValue, floatValue2);
		PdfReferenceHolder primitive = new PdfReferenceHolder(pdfTemplate);
		PdfDictionary pdfDictionary = new PdfDictionary();
		pdfDictionary.SetProperty("N", primitive);
		annotDictionary.SetProperty("AP", pdfDictionary);
	}

	private void SetMatrix(PdfDictionary template, PdfDictionary annotDictionary)
	{
		PdfArray pdfArray = null;
		float[] array = new float[0];
		if (!(PdfCrossTable.Dereference(template["BBox"]) is PdfArray pdfArray2))
		{
			return;
		}
		if (annotDictionary.ContainsKey("Rotate"))
		{
			PdfNumber pdfNumber = PdfCrossTable.Dereference(annotDictionary["Rotate"]) as PdfNumber;
			if (pdfNumber.IntValue != 0)
			{
				int num = pdfNumber.IntValue;
				if (num == 0)
				{
					num *= 90;
				}
				PdfTransformationMatrix pdfTransformationMatrix = new PdfTransformationMatrix();
				pdfTransformationMatrix.Rotate(num);
				pdfArray = new PdfArray(pdfTransformationMatrix.Matrix.Elements);
				template["Matrix"] = pdfArray;
			}
		}
		else
		{
			array = new float[6]
			{
				1f,
				0f,
				0f,
				1f,
				0f - (pdfArray2[0] as PdfNumber).FloatValue,
				0f - (pdfArray2[1] as PdfNumber).FloatValue
			};
			template["Matrix"] = new PdfArray(array);
		}
	}

	private PdfLineEndingStyle MapLineEndingStyle(string style)
	{
		return style.ToLower() switch
		{
			"butt" => PdfLineEndingStyle.Butt, 
			"circle" => PdfLineEndingStyle.Circle, 
			"closedarrow" => PdfLineEndingStyle.ClosedArrow, 
			"diamond" => PdfLineEndingStyle.Diamond, 
			"openarrow" => PdfLineEndingStyle.OpenArrow, 
			"rclosedarrow" => PdfLineEndingStyle.RClosedArrow, 
			"ropenarrow" => PdfLineEndingStyle.ROpenArrow, 
			"slash" => PdfLineEndingStyle.Slash, 
			"square" => PdfLineEndingStyle.Square, 
			_ => PdfLineEndingStyle.None, 
		};
	}

	private PdfAnnotationFlags MapAnnotationFlags(string flag)
	{
		return flag.ToLower() switch
		{
			"hidden" => PdfAnnotationFlags.Hidden, 
			"invisible" => PdfAnnotationFlags.Invisible, 
			"locked" => PdfAnnotationFlags.Locked, 
			"norotate" => PdfAnnotationFlags.NoRotate, 
			"noview" => PdfAnnotationFlags.NoView, 
			"nozoom" => PdfAnnotationFlags.NoZoom, 
			"print" => PdfAnnotationFlags.Print, 
			"readonly" => PdfAnnotationFlags.ReadOnly, 
			"togglenoview" => PdfAnnotationFlags.ToggleNoView, 
			_ => PdfAnnotationFlags.Default, 
		};
	}

	private void AddFloatPoints(PdfDictionary dictionary, float[] points, string key)
	{
		if (points != null && points.Length != 0)
		{
			dictionary.SetProperty(key, new PdfArray(points));
		}
	}

	private void AddFloatPoints(List<float> collection, string value)
	{
		if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
		{
			collection.Add(result);
		}
	}

	private void AddFloat(PdfDictionary dictionary, string key, string value)
	{
		if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
		{
			dictionary.SetNumber(key, result);
		}
	}

	private void AddInt(PdfDictionary dictionary, string key, string value)
	{
		if (int.TryParse(value, out var result))
		{
			dictionary.SetNumber(key, result);
		}
	}

	private void AddString(PdfDictionary dictionary, string key, string value)
	{
		if (!string.IsNullOrEmpty(value))
		{
			dictionary.SetString(key, value);
		}
	}

	private byte[] GetBytes(string hex)
	{
		return new PdfString().HexToBytes(hex);
	}

	private void AddArrayElement(PdfArray array, IPdfPrimitive primitive)
	{
		if (primitive != null)
		{
			array.Add(primitive);
		}
	}

	private void AddElements(XElement element, PdfDictionary dictionary)
	{
		if (element.HasAttributes)
		{
			XAttribute xAttribute = element.Attribute("d");
			if (xAttribute != null && float.TryParse(xAttribute.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
			{
				dictionary.Items.Add(new PdfName("D"), new PdfNumber(result));
			}
			xAttribute = element.Attribute("C".ToLower());
			if (xAttribute != null && float.TryParse(xAttribute.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result2))
			{
				dictionary.Items.Add(new PdfName("C"), new PdfNumber(result2));
			}
			xAttribute = element.Attribute("rt");
			if (xAttribute != null)
			{
				dictionary.Items.Add(new PdfName("RT"), new PdfString(xAttribute.Value));
			}
			xAttribute = element.Attribute("rd");
			if (xAttribute != null)
			{
				dictionary.Items.Add(new PdfName("RD"), new PdfString(xAttribute.Value));
			}
			xAttribute = element.Attribute("SS".ToLower());
			if (xAttribute != null)
			{
				dictionary.Items.Add(new PdfName("SS"), new PdfString(xAttribute.Value));
			}
			xAttribute = element.Attribute("U".ToLower());
			if (xAttribute != null)
			{
				dictionary.Items.Add(new PdfName("U"), new PdfString(xAttribute.Value));
			}
			xAttribute = element.Attribute("F".ToLower());
			if (xAttribute != null)
			{
				dictionary.Items.Add(new PdfName("F"), new PdfName(xAttribute.Value));
			}
			xAttribute = element.Attribute("FD".ToLower());
			if (xAttribute != null)
			{
				dictionary.Items.Add(new PdfName("FD"), new PdfBoolean(xAttribute.Value == "yes"));
			}
		}
	}
}
