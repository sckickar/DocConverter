using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using DocGen.Pdf.Interactive;
using DocGen.Pdf.IO;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Parsing;

internal class XFdfDocument
{
	private Dictionary<object, object> table = new Dictionary<object, object>();

	private string PdfFilePath = "";

	private bool isAnnotationExport;

	private PdfLoadedDocument m_document;

	private PdfExportAnnotationCollection m_annotationCollection;

	private Dictionary<string, object> annotationObjects = new Dictionary<string, object>();

	private bool m_skipBorderStyle;

	private List<string> m_annotationAttributes;

	private string isContainsRV = string.Empty;

	internal IPdfPrimitive trailerId;

	internal bool ExportAppearance;

	internal bool isExportAppearance;

	internal PdfLoadedAnnotationType[] AnnotationTypes = new PdfLoadedAnnotationType[0];

	private bool stampHasImageAppearance;

	private bool isStampAnnotation;

	internal bool IsExportAnnotations
	{
		get
		{
			return isAnnotationExport;
		}
		set
		{
			isAnnotationExport = value;
		}
	}

	internal PdfExportAnnotationCollection AnnotationCollection
	{
		get
		{
			return m_annotationCollection;
		}
		set
		{
			m_annotationCollection = value;
		}
	}

	public XFdfDocument(string filename)
	{
		PdfFilePath = filename;
	}

	internal void SetFields(object fieldName, object Fieldvalue)
	{
		table[fieldName] = Fieldvalue;
	}

	internal void SetFields(object fieldName, object Fieldvalue, string uniqueKey)
	{
		table[fieldName] = Fieldvalue;
		isContainsRV = uniqueKey;
	}

	internal void Save(Stream stream)
	{
		XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
		xmlWriterSettings.Encoding = Encoding.UTF8;
		xmlWriterSettings.Indent = true;
		XmlWriter xmlWriter = XmlWriter.Create(stream, xmlWriterSettings);
		xmlWriter.WriteStartDocument();
		xmlWriter.WriteStartElement("xfdf".ToLower(), "http://ns.adobe.com/xfdf/");
		xmlWriter.WriteAttributeString("xml", "space", null, "preserve");
		if (isAnnotationExport)
		{
			xmlWriter.WriteStartElement("Annots".ToLower());
			if (m_document != null)
			{
				if (m_annotationCollection != null && m_annotationCollection.Count > 0)
				{
					foreach (PdfAnnotation item in m_annotationCollection)
					{
						if (item is PdfLoadedAnnotation annotation)
						{
							PdfLoadedPage loadedPage = item.LoadedPage;
							int index = m_document.Pages.IndexOf(loadedPage);
							ExportAnnotationData(annotation, index, xmlWriter);
						}
					}
				}
				else
				{
					for (int i = 0; i < m_document.PageCount; i++)
					{
						foreach (PdfAnnotation annotation2 in (m_document.Pages[i] as PdfLoadedPage).Annotations)
						{
							if (!(annotation2 is PdfLoadedAnnotation pdfLoadedAnnotation))
							{
								continue;
							}
							if (AnnotationTypes.Length != 0)
							{
								if (pdfLoadedAnnotation is PdfLoadedRectangleAnnotation || pdfLoadedAnnotation is PdfLoadedRubberStampAnnotation)
								{
									ExportAnnotationData(pdfLoadedAnnotation, i, xmlWriter);
									continue;
								}
								for (int j = 0; j < AnnotationTypes.Length; j++)
								{
									if (pdfLoadedAnnotation.Type == AnnotationTypes[j])
									{
										ExportAnnotationData(pdfLoadedAnnotation, i, xmlWriter);
									}
								}
							}
							else
							{
								ExportAnnotationData(pdfLoadedAnnotation, i, xmlWriter);
							}
						}
					}
				}
			}
		}
		else
		{
			xmlWriter.WriteStartElement("Fields".ToLower());
			WriteFormData(xmlWriter);
		}
		xmlWriter.WriteEndElement();
		xmlWriter.WriteStartElement("f");
		xmlWriter.WriteAttributeString("href", PdfFilePath);
		xmlWriter.WriteEndElement();
		xmlWriter.WriteEndElement();
		xmlWriter.WriteEndDocument();
		xmlWriter.Flush();
	}

	internal void Save(Stream stream, bool isacrobat)
	{
		if (!isacrobat)
		{
			return;
		}
		XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
		xmlWriterSettings.Encoding = Encoding.UTF8;
		xmlWriterSettings.Indent = true;
		XmlWriter xmlWriter = XmlWriter.Create(stream, xmlWriterSettings);
		xmlWriter.WriteStartDocument();
		xmlWriter.WriteStartElement("xfdf".ToLower(), "http://ns.adobe.com/xfdf/");
		xmlWriter.WriteAttributeString("xml", "space", null, "preserve");
		xmlWriter.WriteStartElement("f");
		xmlWriter.WriteAttributeString("href", PdfFilePath);
		xmlWriter.WriteEndElement();
		Dictionary<object, object> dictionary = new Dictionary<object, object>();
		bool flag = false;
		dictionary = GetElements(table);
		if (dictionary.Count > 0)
		{
			xmlWriter.WriteStartElement("Fields".ToLower());
		}
		foreach (KeyValuePair<object, object> item in dictionary)
		{
			string text = (string)item.Key;
			xmlWriter.WriteStartElement("field");
			xmlWriter.WriteAttributeString("Name".ToLower(), text.ToString());
			object obj = dictionary[text];
			if (obj.GetType().Name == "PdfArray")
			{
				foreach (PdfString item2 in obj as PdfArray)
				{
					xmlWriter.WriteStartElement("value");
					xmlWriter.WriteString(item2.Value.ToString());
					xmlWriter.WriteEndElement();
					flag = true;
				}
			}
			if (obj is Dictionary<object, object>)
			{
				Dictionary<object, object> value = (Dictionary<object, object>)obj;
				WriteFieldName(value, xmlWriter);
			}
			else if ((!flag && !item.Value.ToString().EndsWith(isContainsRV)) || (!flag && isContainsRV == ""))
			{
				xmlWriter.WriteStartElement("value");
				xmlWriter.WriteString(obj.ToString());
				xmlWriter.WriteEndElement();
			}
			else if (item.Value.ToString().EndsWith(isContainsRV) && isContainsRV != "")
			{
				xmlWriter.WriteStartElement("value-richtext");
				string text2 = item.Value.ToString();
				if (text2.StartsWith("<?xml version=\"1.0\"?>"))
				{
					text2 = text2.Remove(0, 21);
				}
				text2 = text2.Remove(text2.Length - isContainsRV.Length, isContainsRV.Length);
				xmlWriter.WriteRaw(text2);
				xmlWriter.WriteEndElement();
			}
			xmlWriter.WriteEndElement();
			flag = false;
		}
		if (dictionary.Count > 0)
		{
			xmlWriter.WriteEndElement();
		}
		PdfArray pdfArray = trailerId as PdfArray;
		byte[] array = null;
		byte[] array2 = null;
		array = Encoding.GetEncoding("UTF-8").GetBytes("");
		array2 = Encoding.GetEncoding("UTF-8").GetBytes("");
		if (pdfArray != null && pdfArray.Count >= 1)
		{
			array = (pdfArray[0] as PdfString).Bytes;
			array2 = (pdfArray[1] as PdfString).Bytes;
		}
		xmlWriter.WriteStartElement("Ids".ToLower());
		xmlWriter.WriteAttributeString("original", PdfString.BytesToHex(array));
		xmlWriter.WriteAttributeString("modified", PdfString.BytesToHex(array2));
		xmlWriter.WriteEndElement();
		xmlWriter.WriteEndElement();
		xmlWriter.WriteEndDocument();
		xmlWriter.Flush();
	}

	internal Dictionary<object, object> GetElements(Dictionary<object, object> table)
	{
		Dictionary<object, object> dictionary = new Dictionary<object, object>();
		foreach (KeyValuePair<object, object> item in table)
		{
			object key = item.Key;
			object value = item.Value;
			Dictionary<object, object> dictionary2 = dictionary;
			if (key.ToString().Contains("."))
			{
				string[] array = key.ToString().Split('.');
				for (int i = 0; i < array.Length; i++)
				{
					if (dictionary2.ContainsKey(array[i]))
					{
						Dictionary<object, object> dictionary3 = (Dictionary<object, object>)dictionary2[array[i]];
						GetElements(dictionary3);
						dictionary2 = (Dictionary<object, object>)dictionary2[array[i]];
					}
					else if (i == array.Length - 1)
					{
						dictionary2.Add(array[i], value);
					}
					else
					{
						Dictionary<object, object> value2 = new Dictionary<object, object>();
						dictionary2.Add(array[i], value2);
						dictionary2 = (Dictionary<object, object>)dictionary2[array[i]];
					}
				}
			}
			else
			{
				dictionary2.Add(key, value);
			}
		}
		return dictionary;
	}

	private void WriteFieldName(Dictionary<object, object> value, XmlWriter textWriter)
	{
		foreach (KeyValuePair<object, object> item in value)
		{
			if (item.Value is Dictionary<object, object>)
			{
				textWriter.WriteStartElement("field");
				textWriter.WriteAttributeString("Name".ToLower(), item.Key.ToString());
				WriteFieldName((Dictionary<object, object>)item.Value, textWriter);
				textWriter.WriteEndElement();
				continue;
			}
			textWriter.WriteStartElement("field");
			textWriter.WriteAttributeString("Name".ToLower(), item.Key.ToString());
			if (item.Value.GetType().Name == "PdfArray")
			{
				foreach (PdfString item2 in item.Value as PdfArray)
				{
					textWriter.WriteStartElement("value");
					textWriter.WriteString(item2.Value.ToString());
					textWriter.WriteEndElement();
				}
			}
			else
			{
				if (!item.Value.ToString().EndsWith(isContainsRV) || isContainsRV == "")
				{
					textWriter.WriteStartElement("value");
					textWriter.WriteString(item.Value.ToString());
				}
				else
				{
					textWriter.WriteStartElement("value-richtext");
					string text = item.Value.ToString();
					if (text.StartsWith("<?xml version=\"1.0\"?>"))
					{
						text = text.Remove(0, 21);
					}
					text = text.Remove(text.Length - isContainsRV.Length, isContainsRV.Length);
					textWriter.WriteRaw(text);
				}
				textWriter.WriteEndElement();
			}
			textWriter.WriteEndElement();
		}
	}

	internal void ExportAnnotations(Stream stream, PdfLoadedDocument document)
	{
		m_document = document;
		Save(stream);
	}

	private void WriteFormData(XmlWriter textWriter)
	{
		foreach (KeyValuePair<object, object> item in table)
		{
			textWriter.WriteStartElement("field");
			textWriter.WriteAttributeString("Name".ToLower(), item.Key.ToString());
			if (item.Value.GetType().Name == "PdfArray")
			{
				foreach (PdfString item2 in item.Value as PdfArray)
				{
					textWriter.WriteStartElement("value");
					textWriter.WriteString(item2.Value.ToString());
					textWriter.WriteEndElement();
				}
			}
			else
			{
				textWriter.WriteStartElement("value");
				textWriter.WriteString(item.Value.ToString());
				textWriter.WriteEndElement();
			}
			textWriter.WriteEndElement();
		}
	}

	private void ExportAnnotationData(PdfLoadedAnnotation annotation, int index, XmlWriter textWriter)
	{
		if (annotation.Dictionary != null && !(annotation is PdfLoadedFileLinkAnnotation) && !(annotation is PdfLoadedTextWebLinkAnnotation) && !(annotation is PdfLoadedDocumentLinkAnnotation) && !(annotation is PdfLoadedUriAnnotation))
		{
			WriteAnnotationData(annotation, index, textWriter, annotation.Dictionary);
		}
	}

	private void WriteAnnotationData(PdfLoadedAnnotation annotation, int pageIndex, XmlWriter textWriter, PdfDictionary dictionary)
	{
		bool hasAppearance = false;
		string annotationType = GetAnnotationType(dictionary);
		m_skipBorderStyle = false;
		if (string.IsNullOrEmpty(annotationType))
		{
			return;
		}
		_ = m_annotationAttributes;
		m_annotationAttributes = new List<string>();
		textWriter.WriteStartElement(annotationType.ToLower());
		textWriter.WriteAttributeString("page", pageIndex.ToString());
		switch (annotationType)
		{
		case "Line":
		{
			PdfLoadedLineAnnotation pdfLoadedLineAnnotation = annotation as PdfLoadedLineAnnotation;
			float[] array = null;
			if (!pdfLoadedLineAnnotation.Dictionary.ContainsKey("L") || !(PdfCrossTable.Dereference(pdfLoadedLineAnnotation.Dictionary["L"]) is PdfArray pdfArray))
			{
				break;
			}
			array = new float[pdfArray.Count];
			int num = 0;
			foreach (PdfNumber item in pdfArray)
			{
				array[num] = item.FloatValue;
				num++;
			}
			textWriter.WriteAttributeString("start", array[0].ToString(CultureInfo.InvariantCulture) + "," + array[1].ToString(CultureInfo.InvariantCulture));
			textWriter.WriteAttributeString("end", array[2].ToString(CultureInfo.InvariantCulture) + "," + array[3].ToString(CultureInfo.InvariantCulture));
			break;
		}
		case "Stamp":
			hasAppearance = true;
			isStampAnnotation = true;
			break;
		case "Square":
			if (dictionary.ContainsKey("IT"))
			{
				PdfName pdfName = dictionary["IT"] as PdfName;
				if (pdfName != null && pdfName.Value == "SquareImage")
				{
					hasAppearance = true;
				}
			}
			break;
		}
		if (dictionary != null && dictionary.ContainsKey("BE") && dictionary.ContainsKey("BS") && PdfCrossTable.Dereference(dictionary["BE"]) is PdfDictionary pdfDictionary && pdfDictionary.ContainsKey("S"))
		{
			m_skipBorderStyle = true;
		}
		WriteDictionary(dictionary, pageIndex, textWriter, hasAppearance);
		textWriter.WriteEndElement();
		m_annotationAttributes.Clear();
		if (isStampAnnotation)
		{
			isStampAnnotation = false;
		}
	}

	private void WriteDictionary(PdfDictionary dictionary, int pageIndex, XmlWriter textWriter, bool hasAppearance)
	{
		bool flag = false;
		if (dictionary.ContainsKey("Type"))
		{
			PdfName pdfName = PdfCrossTable.Dereference(dictionary["Type"]) as PdfName;
			if (pdfName != null && pdfName.Value == "Border" && m_skipBorderStyle)
			{
				flag = true;
			}
		}
		foreach (KeyValuePair<PdfName, IPdfPrimitive> item in dictionary.Items)
		{
			string value = item.Key.Value;
			if ((!hasAppearance && value == "AP") || value == "P" || value == "Parent")
			{
				continue;
			}
			IPdfPrimitive value2 = item.Value;
			if (value2 is PdfReferenceHolder)
			{
				if (!((value2 as PdfReferenceHolder).Object is PdfDictionary pdfDictionary))
				{
					continue;
				}
				switch (value)
				{
				case "BS":
					WriteDictionary(pdfDictionary, pageIndex, textWriter, hasAppearance: false);
					break;
				case "BE":
					WriteDictionary(pdfDictionary, pageIndex, textWriter, hasAppearance: false);
					break;
				case "IRT":
					if (pdfDictionary.ContainsKey("NM"))
					{
						textWriter.WriteAttributeString("inreplyto", GetValue(pdfDictionary["NM"]));
					}
					break;
				}
			}
			else if (value2 is PdfDictionary)
			{
				WriteDictionary(value2 as PdfDictionary, pageIndex, textWriter, hasAppearance: false);
			}
			else if (!flag || (flag && value != "S"))
			{
				WriteAttribute(textWriter, value, item.Value);
			}
		}
		if ((ExportAppearance || hasAppearance) && dictionary.ContainsKey("AP"))
		{
			MemoryStream appearanceString = GetAppearanceString(dictionary["AP"]);
			if (appearanceString != null && appearanceString.Length > 0 && appearanceString.CanRead && appearanceString.CanSeek)
			{
				textWriter.WriteStartElement("appearance");
				textWriter.WriteBase64(appearanceString.ToArray(), 0, (int)appearanceString.Length);
				textWriter.WriteEndElement();
			}
		}
		if (dictionary.ContainsKey("Measure"))
		{
			ExportMeasureDictionary(dictionary, textWriter);
		}
		if (dictionary.ContainsKey("Sound"))
		{
			IPdfPrimitive pdfPrimitive = dictionary["Sound"];
			if (((pdfPrimitive is PdfReferenceHolder) ? (pdfPrimitive as PdfReferenceHolder).Object : pdfPrimitive) is PdfStream pdfStream)
			{
				if (pdfStream.ContainsKey("B"))
				{
					textWriter.WriteAttributeString("bits", GetValue(pdfStream["B"]));
				}
				if (pdfStream.ContainsKey("C"))
				{
					textWriter.WriteAttributeString("channels", GetValue(pdfStream["C"]));
				}
				if (pdfStream.ContainsKey("E"))
				{
					textWriter.WriteAttributeString("encoding", GetValue(pdfStream["E"]));
				}
				if (pdfStream.ContainsKey("R"))
				{
					textWriter.WriteAttributeString("rate", GetValue(pdfStream["R"]));
				}
				if (pdfStream.Data.Length != 0)
				{
					string text = PdfString.BytesToHex(pdfStream.Data);
					if (!string.IsNullOrEmpty(text))
					{
						textWriter.WriteStartElement("data");
						textWriter.WriteAttributeString("MODE", "raw");
						textWriter.WriteAttributeString("Encoding".ToLower(), "hex");
						if (pdfStream.ContainsKey("Length"))
						{
							textWriter.WriteAttributeString("Length".ToLower(), GetValue(pdfStream["Length"]));
						}
						if (pdfStream.ContainsKey("Filter"))
						{
							textWriter.WriteAttributeString("Filter".ToLower(), GetValue(pdfStream["Filter"]));
						}
						textWriter.WriteRaw(text);
						textWriter.WriteEndElement();
					}
				}
			}
		}
		else if (dictionary.ContainsKey("FS"))
		{
			IPdfPrimitive pdfPrimitive2 = dictionary["FS"];
			if (((pdfPrimitive2 is PdfReferenceHolder) ? (pdfPrimitive2 as PdfReferenceHolder).Object : pdfPrimitive2) is PdfDictionary pdfDictionary2)
			{
				if (pdfDictionary2.ContainsKey("F"))
				{
					textWriter.WriteAttributeString("file", GetValue(pdfDictionary2["F"]));
				}
				if (pdfDictionary2.ContainsKey("EF"))
				{
					pdfPrimitive2 = pdfDictionary2["EF"];
					if (((pdfPrimitive2 is PdfReferenceHolder) ? (pdfPrimitive2 as PdfReferenceHolder).Object : pdfPrimitive2) is PdfDictionary pdfDictionary3 && pdfDictionary3.ContainsKey("F"))
					{
						pdfPrimitive2 = pdfDictionary3["F"];
						if (((pdfPrimitive2 is PdfReferenceHolder) ? (pdfPrimitive2 as PdfReferenceHolder).Object : pdfPrimitive2) is PdfStream pdfStream2)
						{
							if (pdfStream2.ContainsKey("Params"))
							{
								pdfPrimitive2 = pdfStream2["Params"];
								if (((pdfPrimitive2 is PdfReferenceHolder) ? (pdfPrimitive2 as PdfReferenceHolder).Object : pdfPrimitive2) is PdfDictionary pdfDictionary4)
								{
									if (pdfDictionary4.ContainsKey("CreationDate"))
									{
										textWriter.WriteAttributeString("creation", GetValue(pdfDictionary4["CreationDate"]));
									}
									if (pdfDictionary4.ContainsKey("ModDate"))
									{
										textWriter.WriteAttributeString("modification", GetValue(pdfDictionary4["ModDate"]));
									}
									if (pdfDictionary4.ContainsKey("Size"))
									{
										textWriter.WriteAttributeString("Size".ToLower(), GetValue(pdfDictionary4["Size"]));
									}
									if (pdfDictionary4.ContainsKey("CheckSum"))
									{
										string value3 = BitConverter.ToString(Encoding.UTF8.GetBytes(GetValue(pdfDictionary4["CheckSum"]))).Replace("-", "");
										textWriter.WriteAttributeString("checksum", value3);
									}
								}
							}
							string text2 = PdfString.BytesToHex(pdfStream2.Data);
							if (!string.IsNullOrEmpty(text2))
							{
								textWriter.WriteStartElement("DATA".ToLower());
								textWriter.WriteAttributeString("MODE", "RAW".ToLower());
								textWriter.WriteAttributeString("Encoding".ToLower(), "HEX".ToLower());
								if (pdfStream2.ContainsKey("Length"))
								{
									textWriter.WriteAttributeString("Length".ToLower(), GetValue(pdfStream2["Length"]));
								}
								if (pdfStream2.ContainsKey("Filter"))
								{
									textWriter.WriteAttributeString("Filter".ToLower(), GetValue(pdfStream2["Filter"]));
								}
								textWriter.WriteRaw(text2);
								textWriter.WriteEndElement();
							}
						}
					}
				}
			}
		}
		if (dictionary.ContainsKey("Vertices"))
		{
			textWriter.WriteStartElement("Vertices".ToLower());
			if (dictionary["Vertices"] is PdfArray { Count: >0, Count: var count } pdfArray && count % 2 == 0)
			{
				string text3 = string.Empty;
				for (int i = 0; i < count - 1; i++)
				{
					if (pdfArray.Elements[i] is PdfNumber primitive)
					{
						text3 = text3 + GetValue(primitive) + ((i % 2 != 0) ? ";" : ",");
					}
				}
				if (pdfArray.Elements[count - 1] is PdfNumber primitive2)
				{
					text3 += GetValue(primitive2);
				}
				if (!string.IsNullOrEmpty(text3))
				{
					textWriter.WriteRaw(text3);
				}
			}
			textWriter.WriteEndElement();
		}
		if (dictionary.ContainsKey("Popup") && (dictionary["Popup"] as PdfReferenceHolder).Object is PdfDictionary dictionary2)
		{
			WriteAnnotationData(null, pageIndex, textWriter, dictionary2);
		}
		if (dictionary.ContainsKey("DA") && dictionary["DA"] is PdfString pdfString)
		{
			WriteRawData(textWriter, "defaultappearance", pdfString.Value);
		}
		if (dictionary.ContainsKey("DS") && dictionary["DS"] is PdfString pdfString2)
		{
			WriteRawData(textWriter, "defaultstyle", pdfString2.Value);
		}
		if (dictionary.ContainsKey("InkList") && dictionary["InkList"] is PdfArray { Count: >0 } pdfArray2)
		{
			textWriter.WriteStartElement("InkList".ToLower());
			for (int j = 0; j < pdfArray2.Count; j++)
			{
				PdfArray primitive3 = pdfArray2[j] as PdfArray;
				textWriter.WriteElementString("gesture", GetValue(primitive3));
			}
			textWriter.WriteEndElement();
		}
		if (dictionary.ContainsKey("RC"))
		{
			string text4 = (dictionary["RC"] as PdfString).Value;
			int num = text4.IndexOf("<body");
			if (num > 0)
			{
				text4 = text4.Substring(num);
			}
			WriteRawData(textWriter, "contents-richtext", text4);
		}
		if (dictionary.ContainsKey("Contents") && dictionary["Contents"] is PdfString pdfString3 && !string.IsNullOrEmpty(pdfString3.Value))
		{
			textWriter.WriteStartElement("contents");
			textWriter.WriteString(pdfString3.Value);
			textWriter.WriteEndElement();
		}
	}

	private MemoryStream GetAppearanceString(IPdfPrimitive primitive)
	{
		MemoryStream memoryStream = new MemoryStream();
		XmlWriter xmlWriter = XmlWriter.Create(memoryStream, new XmlWriterSettings
		{
			Encoding = Encoding.UTF8,
			Indent = true
		});
		xmlWriter.WriteStartElement("DICT");
		xmlWriter.WriteAttributeString("KEY", "AP");
		if (((primitive is PdfReferenceHolder) ? (primitive as PdfReferenceHolder).Object : primitive) is PdfDictionary dictionary)
		{
			WriteAppearanceDictionary(xmlWriter, dictionary);
		}
		xmlWriter.WriteEndElement();
		xmlWriter.Flush();
		return memoryStream;
	}

	private void WriteAppearanceDictionary(XmlWriter textWriter, PdfDictionary dictionary)
	{
		if (dictionary == null || dictionary.Count <= 0)
		{
			return;
		}
		foreach (KeyValuePair<PdfName, IPdfPrimitive> item in dictionary.Items)
		{
			WriteObject(textWriter, item.Key.Value, item.Value);
		}
	}

	private void WriteObject(XmlWriter textWriter, string key, IPdfPrimitive primitive)
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
			{
				if (!(text == "DocGen.Pdf.Primitives.PdfStream"))
				{
					break;
				}
				PdfStream pdfStream = (primitive as PdfStream).Clone(m_document.CrossTable) as PdfStream;
				if (pdfStream.Data.Length == 0)
				{
					break;
				}
				textWriter.WriteStartElement("STREAM");
				textWriter.WriteAttributeString("KEY", key);
				textWriter.WriteAttributeString("DEFINE", "");
				WriteAppearanceDictionary(textWriter, pdfStream);
				textWriter.WriteStartElement("DATA");
				string value = GetValue(pdfStream["Subtype"]);
				if (pdfStream.ContainsKey("Subtype") && "Image" == value)
				{
					textWriter.WriteAttributeString("MODE", "RAW");
					textWriter.WriteAttributeString("Encoding".ToUpper(), "HEX");
					string text3 = BitConverter.ToString(pdfStream.Data).Replace("-", "");
					if (!string.IsNullOrEmpty(text3))
					{
						textWriter.WriteRaw(text3);
					}
				}
				else if (!pdfStream.ContainsKey("Type") && !pdfStream.ContainsKey("Subtype"))
				{
					textWriter.WriteAttributeString("MODE", "RAW");
					textWriter.WriteAttributeString("Encoding".ToUpper(), "HEX");
					string text4 = BitConverter.ToString(pdfStream.Data).Replace("-", "");
					if (!string.IsNullOrEmpty(text4))
					{
						textWriter.WriteRaw(text4);
					}
				}
				else if (pdfStream.ContainsKey("Subtype") && "Form" == value && !isStampAnnotation)
				{
					textWriter.WriteAttributeString("MODE", "RAW");
					textWriter.WriteAttributeString("Encoding".ToUpper(), "HEX");
					string value2 = BitConverter.ToString(pdfStream.GetDecompressedData()).Replace("-", "");
					if (!string.IsNullOrEmpty(value2))
					{
						textWriter.WriteRaw(GetFormatedString(value2));
					}
				}
				else
				{
					byte[] decompressedData = pdfStream.GetDecompressedData();
					string @string = Encoding.UTF8.GetString(decompressedData, 0, decompressedData.Length);
					if (isStampAnnotation && !@string.Contains("TJ"))
					{
						textWriter.WriteAttributeString("MODE", "FILTERED");
						textWriter.WriteAttributeString("Encoding".ToUpper(), "ASCII");
						if (!string.IsNullOrEmpty(@string))
						{
							textWriter.WriteRaw(GetFormatedString(@string));
						}
					}
					else
					{
						textWriter.WriteAttributeString("MODE", "RAW");
						textWriter.WriteAttributeString("Encoding".ToUpper(), "HEX");
						string value3 = BitConverter.ToString(pdfStream.GetDecompressedData()).Replace("-", "");
						if (!string.IsNullOrEmpty(value3))
						{
							textWriter.WriteRaw(GetFormatedString(value3));
						}
					}
				}
				textWriter.WriteEndElement();
				textWriter.WriteEndElement();
				break;
			}
			case 'i':
				if (text == "DocGen.Pdf.Primitives.PdfString")
				{
					PdfString pdfString = primitive as PdfString;
					textWriter.WriteStartElement("STRING");
					textWriter.WriteAttributeString("KEY", key);
					textWriter.WriteAttributeString("VAL", pdfString.Value);
					textWriter.WriteEndElement();
				}
				break;
			case 'b':
			{
				if (!(text == "DocGen.Pdf.Primitives.PdfNumber"))
				{
					break;
				}
				PdfNumber pdfNumber = primitive as PdfNumber;
				if (pdfNumber.IsInteger)
				{
					textWriter.WriteStartElement("INT");
					textWriter.WriteAttributeString("KEY", key);
					textWriter.WriteAttributeString("VAL", pdfNumber.IntValue.ToString());
				}
				else if (pdfNumber.IsLong)
				{
					textWriter.WriteStartElement("INT");
					textWriter.WriteAttributeString("KEY", key);
					textWriter.WriteAttributeString("VAL", pdfNumber.LongValue.ToString());
				}
				else
				{
					textWriter.WriteStartElement("FIXED");
					textWriter.WriteAttributeString("KEY", key);
					string text2 = Math.Round(pdfNumber.FloatValue, 6).ToString();
					if (!text2.Contains("."))
					{
						text2 += ".000000";
					}
					textWriter.WriteAttributeString("VAL", text2);
				}
				textWriter.WriteEndElement();
				break;
			}
			}
			break;
		case 33:
			switch (text[30])
			{
			case 'a':
				if (text == "DocGen.Pdf.Primitives.PdfName")
				{
					PdfName pdfName = primitive as PdfName;
					textWriter.WriteStartElement("NAME");
					textWriter.WriteAttributeString("KEY", key);
					textWriter.WriteAttributeString("VAL", pdfName.Value);
					textWriter.WriteEndElement();
				}
				break;
			case 'u':
				if (text == "DocGen.Pdf.Primitives.PdfNull")
				{
					textWriter.WriteStartElement("NULL");
					textWriter.WriteAttributeString("KEY", key);
					textWriter.WriteEndElement();
				}
				break;
			}
			break;
		case 44:
			if (text == "DocGen.Pdf.Primitives.PdfReferenceHolder")
			{
				PdfReferenceHolder pdfReferenceHolder = primitive as PdfReferenceHolder;
				WriteObject(textWriter, key, pdfReferenceHolder.Object);
			}
			break;
		case 39:
			if (text == "DocGen.Pdf.Primitives.PdfDictionary")
			{
				PdfDictionary dictionary = primitive as PdfDictionary;
				textWriter.WriteStartElement("DICT");
				textWriter.WriteAttributeString("KEY", key);
				WriteAppearanceDictionary(textWriter, dictionary);
				textWriter.WriteEndElement();
			}
			break;
		case 36:
			if (text == "DocGen.Pdf.Primitives.PdfBoolean")
			{
				PdfBoolean pdfBoolean = primitive as PdfBoolean;
				textWriter.WriteStartElement("BOOL");
				textWriter.WriteAttributeString("KEY", key);
				textWriter.WriteAttributeString("VAL", pdfBoolean.Value ? "true" : "false");
				textWriter.WriteEndElement();
			}
			break;
		case 34:
			if (text == "DocGen.Pdf.Primitives.PdfArray")
			{
				textWriter.WriteStartElement("ARRAY");
				textWriter.WriteAttributeString("KEY", key);
				WriteArray(textWriter, primitive as PdfArray);
				textWriter.WriteEndElement();
			}
			break;
		}
	}

	private void WriteArray(XmlWriter textWriter, PdfArray array)
	{
		foreach (IPdfPrimitive element in array.Elements)
		{
			WriteArrayElement(textWriter, element);
		}
	}

	private void WriteArrayElement(XmlWriter textWriter, IPdfPrimitive element)
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
			{
				if (!(text == "DocGen.Pdf.Primitives.PdfString"))
				{
					break;
				}
				PdfString pdfString = element as PdfString;
				textWriter.WriteStartElement("STRING");
				if (Regex.IsMatch(pdfString.Value, "[\u0085-ÿ]") && pdfString.Hex)
				{
					byte[] data = pdfString.PdfEncode(m_document);
					pdfString.Value = PdfString.ByteToString(data);
					textWriter.WriteAttributeString("Encoding".ToUpper(), "HEX");
					if (!string.IsNullOrEmpty(pdfString.Value))
					{
						textWriter.WriteRaw(GetFormatedString(pdfString.Value));
					}
				}
				else
				{
					textWriter.WriteAttributeString("VAL", pdfString.Value);
				}
				textWriter.WriteEndElement();
				break;
			}
			case 'b':
			{
				if (!(text == "DocGen.Pdf.Primitives.PdfNumber"))
				{
					break;
				}
				PdfNumber pdfNumber = element as PdfNumber;
				if (pdfNumber.IsInteger)
				{
					textWriter.WriteStartElement("INT");
					textWriter.WriteAttributeString("VAL", pdfNumber.IntValue.ToString());
				}
				else if (pdfNumber.IsLong)
				{
					textWriter.WriteStartElement("INT");
					textWriter.WriteAttributeString("VAL", pdfNumber.LongValue.ToString());
				}
				else
				{
					textWriter.WriteStartElement("FIXED");
					string text3 = Math.Round(pdfNumber.FloatValue, 6).ToString();
					if (!text3.Contains(".") && !text3.Contains(","))
					{
						text3 += ".000000";
					}
					else if (text3.Contains(","))
					{
						text3 = text3.Replace(',', '.');
					}
					textWriter.WriteAttributeString("VAL", text3);
				}
				textWriter.WriteEndElement();
				break;
			}
			case 'e':
			{
				if (!(text == "DocGen.Pdf.Primitives.PdfStream"))
				{
					break;
				}
				PdfStream pdfStream = (element as PdfStream).Clone(m_document.CrossTable) as PdfStream;
				if (pdfStream.Data.Length == 0)
				{
					break;
				}
				textWriter.WriteStartElement("STREAM");
				textWriter.WriteAttributeString("DEFINE", "");
				WriteAppearanceDictionary(textWriter, pdfStream);
				textWriter.WriteStartElement("DATA");
				if (pdfStream.ContainsKey("Subtype") && "Image" == GetValue(pdfStream["Subtype"]))
				{
					textWriter.WriteAttributeString("MODE", "RAW");
					textWriter.WriteAttributeString("Encoding".ToUpper(), "HEX");
					string text2 = BitConverter.ToString(pdfStream.Data).Replace("-", "");
					if (!string.IsNullOrEmpty(text2))
					{
						textWriter.WriteRaw(text2);
					}
				}
				else
				{
					textWriter.WriteAttributeString("MODE", "FILTERED");
					textWriter.WriteAttributeString("Encoding".ToUpper(), "ASCII");
					byte[] decompressedData = pdfStream.GetDecompressedData();
					string @string = Encoding.UTF8.GetString(decompressedData, 0, decompressedData.Length);
					if (!string.IsNullOrEmpty(@string))
					{
						textWriter.WriteRaw(GetFormatedString(@string));
					}
				}
				textWriter.WriteEndElement();
				textWriter.WriteEndElement();
				break;
			}
			}
			break;
		case 34:
			if (text == "DocGen.Pdf.Primitives.PdfArray")
			{
				textWriter.WriteStartElement("ARRAY");
				WriteArray(textWriter, element as PdfArray);
				textWriter.WriteEndElement();
			}
			break;
		case 33:
			if (text == "DocGen.Pdf.Primitives.PdfName")
			{
				PdfName pdfName = element as PdfName;
				textWriter.WriteStartElement("NAME");
				textWriter.WriteAttributeString("VAL", pdfName.Value);
				textWriter.WriteEndElement();
			}
			break;
		case 36:
			if (text == "DocGen.Pdf.Primitives.PdfBoolean")
			{
				PdfBoolean pdfBoolean = element as PdfBoolean;
				textWriter.WriteStartElement("BOOL");
				textWriter.WriteAttributeString("VAL", pdfBoolean.Value ? "true" : "false");
				textWriter.WriteEndElement();
			}
			break;
		case 39:
			if (text == "DocGen.Pdf.Primitives.PdfDictionary")
			{
				PdfDictionary dictionary = element as PdfDictionary;
				textWriter.WriteStartElement("DICT");
				WriteAppearanceDictionary(textWriter, dictionary);
				textWriter.WriteEndElement();
			}
			break;
		case 44:
			if (text == "DocGen.Pdf.Primitives.PdfReferenceHolder")
			{
				PdfReferenceHolder pdfReferenceHolder = element as PdfReferenceHolder;
				if (pdfReferenceHolder.Object != null)
				{
					WriteArrayElement(textWriter, pdfReferenceHolder.Object);
				}
			}
			break;
		}
	}

	private void WriteAttribute(XmlWriter textWriter, string key, IPdfPrimitive primitive)
	{
		if (m_annotationAttributes == null || m_annotationAttributes.Contains(key))
		{
			return;
		}
		if (key != null)
		{
			switch (key.Length)
			{
			case 1:
				switch (key[0])
				{
				case 'C':
				{
					string color2 = GetColor(primitive);
					if (primitive is PdfNumber)
					{
						string value = GetValue(primitive);
						if (!string.IsNullOrEmpty(value) && !m_annotationAttributes.Contains("c"))
						{
							textWriter.WriteAttributeString("c", value);
							m_annotationAttributes.Add("c");
						}
					}
					if (!string.IsNullOrEmpty(color2) && !m_annotationAttributes.Contains("color"))
					{
						textWriter.WriteAttributeString("color", color2);
						m_annotationAttributes.Add("color");
					}
					return;
				}
				case 'M':
					if (!m_annotationAttributes.Contains("date"))
					{
						textWriter.WriteAttributeString("date", GetValue(primitive));
						m_annotationAttributes.Add("date");
					}
					return;
				case 'T':
					if (!m_annotationAttributes.Contains("Title".ToLower()))
					{
						textWriter.WriteAttributeString("Title".ToLower(), GetValue(primitive));
						m_annotationAttributes.Add("Title".ToLower());
					}
					return;
				case 'W':
					if (!m_annotationAttributes.Contains("Width".ToLower()))
					{
						textWriter.WriteAttributeString("Width".ToLower(), GetValue(primitive));
						m_annotationAttributes.Add("Width".ToLower());
					}
					return;
				case 'S':
					if (!m_annotationAttributes.Contains("style"))
					{
						switch (GetValue(primitive))
						{
						case "D":
							textWriter.WriteAttributeString("style", "dash");
							break;
						case "C":
							textWriter.WriteAttributeString("style", "cloudy");
							break;
						case "S":
							textWriter.WriteAttributeString("style", "solid");
							break;
						case "B":
							textWriter.WriteAttributeString("style", "bevelled");
							break;
						case "I":
							textWriter.WriteAttributeString("style", "inset");
							break;
						case "U":
							textWriter.WriteAttributeString("style", "underline");
							break;
						}
						m_annotationAttributes.Add("style");
					}
					return;
				case 'D':
					if (!m_annotationAttributes.Contains("dashes"))
					{
						textWriter.WriteAttributeString("dashes", GetValue(primitive));
						m_annotationAttributes.Add("dashes");
					}
					return;
				case 'I':
					if (!m_annotationAttributes.Contains("intensity"))
					{
						textWriter.WriteAttributeString("intensity", GetValue(primitive));
						m_annotationAttributes.Add("intensity");
					}
					return;
				case 'Q':
					if (!m_annotationAttributes.Contains("justification"))
					{
						textWriter.WriteAttributeString("justification", GetValue(primitive));
						m_annotationAttributes.Add("justification");
					}
					return;
				case 'F':
					if (primitive is PdfNumber pdfNumber && !m_annotationAttributes.Contains("Flags".ToLower()))
					{
						string text = ((PdfAnnotationFlags)pdfNumber.IntValue).ToString().ToLower();
						text = text.Replace(" ", "");
						textWriter.WriteAttributeString("Flags".ToLower(), text);
						m_annotationAttributes.Add("Flags".ToLower());
					}
					return;
				case 'L':
				case 'P':
					return;
				}
				break;
			case 2:
				switch (key[1])
				{
				case 'C':
					break;
				case 'M':
					if (!(key == "NM"))
					{
						goto end_IL_002c;
					}
					if (!m_annotationAttributes.Contains("Name".ToLower()))
					{
						textWriter.WriteAttributeString("Name".ToLower(), GetValue(primitive));
						m_annotationAttributes.Add("Name".ToLower());
					}
					return;
				case 'E':
					goto IL_02a9;
				case 'D':
					goto IL_02be;
				case 'T':
					goto IL_02e3;
				case 'L':
					goto IL_0308;
				case 'P':
					if (!(key == "CP"))
					{
						goto end_IL_002c;
					}
					if (!m_annotationAttributes.Contains("caption-style"))
					{
						textWriter.WriteAttributeString("caption-style", GetValue(primitive));
						m_annotationAttributes.Add("caption-style");
					}
					return;
				case 'A':
					goto IL_0342;
				case 'S':
					if (!(key == "DS") && !(key == "FS"))
					{
						goto end_IL_002c;
					}
					return;
				default:
					goto end_IL_002c;
				}
				switch (key)
				{
				case "IC":
				{
					string color = GetColor(primitive);
					if (!string.IsNullOrEmpty(color) && !m_annotationAttributes.Contains("interior-color"))
					{
						textWriter.WriteAttributeString("interior-color", color);
						m_annotationAttributes.Add("interior-color");
					}
					return;
				}
				case "OC":
					break;
				default:
					goto end_IL_002c;
				case "RC":
					return;
				}
				goto IL_0d7b;
			case 4:
				switch (key[0])
				{
				case 'N':
					if (!(key == "Name"))
					{
						goto end_IL_002c;
					}
					if (!m_annotationAttributes.Contains("icon"))
					{
						textWriter.WriteAttributeString("icon", GetValue(primitive));
						m_annotationAttributes.Add("icon");
					}
					return;
				case 'S':
					if (!(key == "Subj"))
					{
						goto end_IL_002c;
					}
					if (!m_annotationAttributes.Contains("Subject".ToLower()))
					{
						textWriter.WriteAttributeString("Subject".ToLower(), GetValue(primitive));
						m_annotationAttributes.Add("Subject".ToLower());
					}
					return;
				case 'R':
					break;
				case 'T':
					if (!(key == "Type"))
					{
						goto end_IL_002c;
					}
					return;
				case 'I':
					if (!(key == "ITEx"))
					{
						goto end_IL_002c;
					}
					return;
				case 'O':
					goto IL_03f5;
				default:
					goto end_IL_002c;
				}
				if (!(key == "Rect"))
				{
					break;
				}
				goto IL_079c;
			case 12:
			{
				char c = key[0];
				if (c != 'C')
				{
					if (c != 'G' || !(key == "GroupNesting"))
					{
						break;
					}
					return;
				}
				if (!(key == "CreationDate"))
				{
					break;
				}
				goto IL_079c;
			}
			case 6:
			{
				char c = key[4];
				if ((uint)c <= 101u)
				{
					if (c != 'a')
					{
						if (c != 'e' || !(key == "Border"))
						{
							break;
						}
						if (!m_annotationAttributes.Contains("Width".ToLower()) && !m_annotationAttributes.Contains("Border".ToLower()))
						{
							PdfArray pdfArray = primitive as PdfArray;
							PdfNumber primitive2 = pdfArray.Elements[2] as PdfNumber;
							textWriter.WriteAttributeString("Width".ToLower(), GetValue(primitive2));
							textWriter.WriteAttributeString("Border".ToLower(), GetValue(pdfArray));
							m_annotationAttributes.Add("Border".ToLower());
							m_annotationAttributes.Add("Width".ToLower());
						}
						return;
					}
					if (!(key == "Repeat"))
					{
						break;
					}
					goto IL_0d7b;
				}
				switch (c)
				{
				case 't':
					if (!(key == "Rotate"))
					{
						break;
					}
					if (!m_annotationAttributes.Contains("rotation"))
					{
						textWriter.WriteAttributeString("rotation", GetValue(primitive));
						m_annotationAttributes.Add("rotation");
					}
					return;
				case 'n':
					if (!(key == "Parent"))
					{
						break;
					}
					return;
				}
				break;
			}
			case 3:
			{
				char c = key[2];
				if (c != 'E')
				{
					if (c != 'O')
					{
						if (c != 'p' || !(key == "Cap"))
						{
							break;
						}
						if (!m_annotationAttributes.Contains("caption"))
						{
							textWriter.WriteAttributeString("caption", GetValue(primitive));
							m_annotationAttributes.Add("caption");
						}
						return;
					}
					if (!(key == "LLO"))
					{
						break;
					}
					goto IL_0d7b;
				}
				if (!(key == "LLE"))
				{
					break;
				}
				if (!m_annotationAttributes.Contains("leaderExtend"))
				{
					textWriter.WriteAttributeString("leaderExtend", GetValue(primitive));
					m_annotationAttributes.Add("leaderExtend");
				}
				return;
			}
			case 10:
			{
				char c = key[0];
				if (c != 'Q')
				{
					if (c != 'S' || !(key == "StateModel"))
					{
						break;
					}
					goto IL_0d7b;
				}
				if (!(key == "QuadPoints"))
				{
					break;
				}
				if (!m_annotationAttributes.Contains("Coords".ToLower()))
				{
					textWriter.WriteAttributeString("Coords".ToLower(), GetValue(primitive));
					m_annotationAttributes.Add("Coords".ToLower());
				}
				return;
			}
			case 7:
				switch (key[0])
				{
				case 'I':
					if (!(key == "InkList"))
					{
						break;
					}
					return;
				case 'S':
					if (!(key == "Subtype"))
					{
						break;
					}
					return;
				}
				break;
			case 8:
				switch (key[0])
				{
				case 'C':
					if (!(key == "Contents"))
					{
						break;
					}
					return;
				case 'V':
					if (!(key == "Vertices"))
					{
						break;
					}
					return;
				}
				break;
			case 16:
				if (!(key == "MeasurementTypes"))
				{
					break;
				}
				return;
			case 5:
				if (!(key == "State"))
				{
					break;
				}
				goto IL_0d7b;
			case 11:
				{
					if (!(key == "OverlayText"))
					{
						break;
					}
					goto IL_0d7b;
				}
				IL_02e3:
				if (!(key == "IT"))
				{
					if (!(key == "RT"))
					{
						break;
					}
					if (!m_annotationAttributes.Contains("replyType"))
					{
						textWriter.WriteAttributeString("replyType", GetValue(primitive).ToLower());
						m_annotationAttributes.Add("replyType");
					}
					return;
				}
				if (!m_annotationAttributes.Contains(key))
				{
					textWriter.WriteAttributeString(key, GetValue(primitive));
					m_annotationAttributes.Add(key);
				}
				return;
				IL_02be:
				if (!(key == "RD"))
				{
					if (!(key == "FD"))
					{
						break;
					}
					if (!m_annotationAttributes.Contains(key.ToLower()))
					{
						textWriter.WriteAttributeString(key.ToLower(), GetValue(primitive));
						m_annotationAttributes.Add(key.ToLower());
					}
					return;
				}
				if (!m_annotationAttributes.Contains("fringe"))
				{
					textWriter.WriteAttributeString("fringe", GetValue(primitive));
					m_annotationAttributes.Add("fringe");
				}
				return;
				IL_0d7b:
				if (!m_annotationAttributes.Contains(key))
				{
					textWriter.WriteAttributeString(key.ToLower(), GetValue(primitive));
					m_annotationAttributes.Add(key.ToLower());
				}
				return;
				IL_03f5:
				if (!(key == "Open"))
				{
					break;
				}
				goto IL_0d7b;
				IL_02a9:
				if (!(key == "LE"))
				{
					break;
				}
				if (primitive is PdfArray)
				{
					PdfArray pdfArray2 = primitive as PdfArray;
					if (pdfArray2.Count == 2)
					{
						textWriter.WriteAttributeString("head", GetValue(pdfArray2.Elements[0]));
						textWriter.WriteAttributeString("tail", GetValue(pdfArray2.Elements[1]));
					}
				}
				else if (primitive is PdfName && !m_annotationAttributes.Contains("head"))
				{
					textWriter.WriteAttributeString("head", GetValue(primitive));
					m_annotationAttributes.Add("head");
				}
				return;
				IL_079c:
				if (!m_annotationAttributes.Contains(key.ToLower()))
				{
					textWriter.WriteAttributeString(key.ToLower(), GetValue(primitive));
					m_annotationAttributes.Add(key.ToLower());
				}
				return;
				IL_0342:
				if (!(key == "CA"))
				{
					if (!(key == "DA"))
					{
						break;
					}
					return;
				}
				if (!m_annotationAttributes.Contains("opacity"))
				{
					textWriter.WriteAttributeString("opacity", GetValue(primitive));
					m_annotationAttributes.Add("opacity");
				}
				return;
				IL_0308:
				if (!(key == "LL"))
				{
					if (!(key == "CL"))
					{
						break;
					}
					if (!m_annotationAttributes.Contains("callout"))
					{
						textWriter.WriteAttributeString("callout", GetValue(primitive));
						m_annotationAttributes.Add("callout");
					}
					return;
				}
				if (!m_annotationAttributes.Contains("leaderLength"))
				{
					textWriter.WriteAttributeString("leaderLength", GetValue(primitive));
					m_annotationAttributes.Add("leaderLength");
				}
				return;
				end_IL_002c:
				break;
			}
		}
		if (!m_annotationAttributes.Contains(key))
		{
			textWriter.WriteAttributeString(key, GetValue(primitive));
			m_annotationAttributes.Add(key);
		}
	}

	private void WriteRawData(XmlWriter textWriter, string name, string value)
	{
		if (!string.IsNullOrEmpty(value))
		{
			textWriter.WriteStartElement(name);
			textWriter.WriteRaw(value);
			textWriter.WriteEndElement();
		}
	}

	private string GetColor(IPdfPrimitive primitive)
	{
		string result = string.Empty;
		if (primitive != null && primitive is PdfArray { Count: >=3 } pdfArray)
		{
			string text = Convert.ToInt32((pdfArray.Elements[0] as PdfNumber).FloatValue * 255f).ToString("X");
			string text2 = Convert.ToInt32((pdfArray.Elements[1] as PdfNumber).FloatValue * 255f).ToString("X");
			string text3 = Convert.ToInt32((pdfArray.Elements[2] as PdfNumber).FloatValue * 255f).ToString("X");
			result = "#" + ((text.Length == 1) ? ("0" + text) : text) + ((text2.Length == 1) ? ("0" + text2) : text2) + ((text3.Length == 1) ? ("0" + text3) : text3);
		}
		return result;
	}

	private string GetValue(IPdfPrimitive primitive)
	{
		string text = string.Empty;
		if (primitive is PdfName)
		{
			text = (primitive as PdfName).Value;
		}
		else if (primitive is PdfBoolean)
		{
			text = ((primitive as PdfBoolean).Value ? "yes" : "no");
		}
		else if (primitive is PdfString)
		{
			text = (primitive as PdfString).Value;
		}
		else if (primitive is PdfArray)
		{
			PdfArray pdfArray = primitive as PdfArray;
			if (pdfArray.Elements.Count > 0)
			{
				text = GetValue(pdfArray.Elements[0]);
			}
			for (int i = 1; i < pdfArray.Elements.Count; i++)
			{
				text = text + "," + GetValue(pdfArray.Elements[i]);
			}
		}
		else if (primitive is PdfNumber)
		{
			text = (primitive as PdfNumber).FloatValue.ToString(CultureInfo.InvariantCulture);
		}
		if (text.Contains("\u0002"))
		{
			text = text.Replace("\u0002", "‐");
		}
		return text;
	}

	private string GetAnnotationType(PdfDictionary dictionary)
	{
		string result = string.Empty;
		if (dictionary.ContainsKey("Subtype"))
		{
			PdfName pdfName = dictionary["Subtype"] as PdfName;
			if (pdfName != null)
			{
				result = pdfName.Value;
			}
		}
		return result;
	}

	private string GetFormatedString(string value)
	{
		value = value.Replace("&", "&amp;");
		value = value.Replace("<", "&lt;");
		value = value.Replace(">", "&gt;");
		return value;
	}

	private void ExportMeasureDictionary(PdfDictionary dictionary, XmlWriter textWriter)
	{
		textWriter.WriteStartElement("measure");
		IPdfPrimitive pdfPrimitive = dictionary["Measure"];
		if (((pdfPrimitive is PdfReferenceHolder) ? (pdfPrimitive as PdfReferenceHolder).Object : pdfPrimitive) is PdfDictionary pdfDictionary)
		{
			if (pdfDictionary.ContainsKey("R"))
			{
				textWriter.WriteAttributeString("rateValue", GetValue(pdfDictionary["R"]));
			}
			if (pdfDictionary.ContainsKey("A"))
			{
				IPdfPrimitive pdfPrimitive2 = (pdfDictionary["A"] as PdfArray).Elements[0];
				PdfDictionary measurementDetails = ((pdfPrimitive2 is PdfReferenceHolder) ? (pdfPrimitive2 as PdfReferenceHolder).Object : pdfPrimitive2) as PdfDictionary;
				textWriter.WriteStartElement("area");
				ExportMeasureFormatDetails(measurementDetails, textWriter);
				textWriter.WriteEndElement();
			}
			if (pdfDictionary.ContainsKey("D"))
			{
				IPdfPrimitive pdfPrimitive3 = (pdfDictionary["D"] as PdfArray).Elements[0];
				PdfDictionary measurementDetails2 = ((pdfPrimitive3 is PdfReferenceHolder) ? (pdfPrimitive3 as PdfReferenceHolder).Object : pdfPrimitive3) as PdfDictionary;
				textWriter.WriteStartElement("distance");
				ExportMeasureFormatDetails(measurementDetails2, textWriter);
				textWriter.WriteEndElement();
			}
			if (pdfDictionary.ContainsKey("X") && pdfDictionary["X"] is PdfArray pdfArray && pdfArray.Elements.Count > 0)
			{
				IPdfPrimitive pdfPrimitive4 = pdfArray.Elements[0];
				PdfDictionary measurementDetails3 = ((pdfPrimitive4 is PdfReferenceHolder) ? (pdfPrimitive4 as PdfReferenceHolder).Object : pdfPrimitive4) as PdfDictionary;
				textWriter.WriteStartElement("xformat");
				ExportMeasureFormatDetails(measurementDetails3, textWriter);
				textWriter.WriteEndElement();
			}
		}
		textWriter.WriteEndElement();
	}

	private void ExportMeasureFormatDetails(PdfDictionary measurementDetails, XmlWriter textWriter)
	{
		if (measurementDetails.ContainsKey("C"))
		{
			textWriter.WriteAttributeString("c", GetValue(measurementDetails["C"]));
		}
		if (measurementDetails.ContainsKey("F"))
		{
			textWriter.WriteAttributeString("f", GetValue(measurementDetails["F"]));
		}
		if (measurementDetails.ContainsKey("D"))
		{
			textWriter.WriteAttributeString("d", GetValue(measurementDetails["D"]));
		}
		if (measurementDetails.ContainsKey("RD"))
		{
			textWriter.WriteAttributeString("rd", GetValue(measurementDetails["RD"]));
		}
		if (measurementDetails.ContainsKey("U"))
		{
			textWriter.WriteAttributeString("u", GetValue(measurementDetails["U"]));
		}
		if (measurementDetails.ContainsKey("RT"))
		{
			textWriter.WriteAttributeString("rt", GetValue(measurementDetails["RT"]));
		}
		if (measurementDetails.ContainsKey("SS"))
		{
			textWriter.WriteAttributeString("ss", GetValue(measurementDetails["SS"]));
		}
		if (measurementDetails.ContainsKey("FD"))
		{
			textWriter.WriteAttributeString("fd", GetValue(measurementDetails["FD"]));
		}
	}
}
