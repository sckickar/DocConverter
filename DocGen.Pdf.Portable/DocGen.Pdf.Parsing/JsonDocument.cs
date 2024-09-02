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

internal class JsonDocument
{
	private string pdfFilePath = string.Empty;

	private PdfLoadedDocument m_document;

	private bool m_skipBorderStyle;

	private Dictionary<string, string> table = new Dictionary<string, string>();

	private PdfExportAnnotationCollection m_annotationCollection;

	private int count = 1;

	private bool flag;

	private string jsonData = "{\"pdfAnnotation\":{ \"0\":{ \"shapeAnnotation\":[";

	private bool m_exportAppearance;

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

	internal bool ExportAppearance
	{
		get
		{
			return m_exportAppearance;
		}
		set
		{
			m_exportAppearance = value;
		}
	}

	internal JsonDocument(string filename)
	{
		pdfFilePath = filename;
	}

	internal void ExportAnnotations(Stream stream, PdfLoadedDocument document)
	{
		m_document = document;
		if (m_document == null)
		{
			return;
		}
		if (m_annotationCollection != null && m_annotationCollection.Count > 0)
		{
			foreach (PdfAnnotation item in m_annotationCollection)
			{
				if (item is PdfLoadedAnnotation annotation)
				{
					PdfLoadedPage loadedPage = item.LoadedPage;
					int index = m_document.Pages.IndexOf(loadedPage);
					ExportAnnotationData(stream, annotation, index, count);
					count++;
				}
			}
			return;
		}
		ExportAnnotationData(m_document, m_document.PageCount, stream);
	}

	private void ExportAnnotationData(Stream stream, PdfLoadedAnnotation annotation, int index, int count)
	{
		byte[] bytes;
		if (!flag)
		{
			bytes = Encoding.GetEncoding("UTF-8").GetBytes(jsonData);
			stream.Write(bytes, 0, bytes.Length);
		}
		_ = m_document.Pages[index];
		flag = true;
		if (!(annotation is PdfLoadedFileLinkAnnotation) && !(annotation is PdfLoadedTextWebLinkAnnotation) && !(annotation is PdfLoadedDocumentLinkAnnotation) && !(annotation is PdfLoadedUriAnnotation))
		{
			ExportAnnotationData(annotation, index, annotation.Dictionary);
		}
		jsonData = convertToJson(table);
		if (m_annotationCollection.Count > count)
		{
			jsonData += ",";
		}
		else
		{
			jsonData += "]}}}";
		}
		bytes = Encoding.GetEncoding("UTF-8").GetBytes(jsonData);
		stream.Write(bytes, 0, bytes.Length);
		table.Clear();
	}

	private void ExportAnnotationData(PdfLoadedDocument document, int pageCount, Stream stream)
	{
		bool flag = false;
		string s = "{\"pdfAnnotation\":{";
		byte[] bytes = Encoding.GetEncoding("UTF-8").GetBytes(s);
		stream.Write(bytes, 0, bytes.Length);
		for (int i = 0; i < pageCount; i++)
		{
			PdfLoadedPage pdfLoadedPage = document.Pages[i] as PdfLoadedPage;
			if (pdfLoadedPage.Annotations.Count > 0)
			{
				s = ((!(i != 0 && flag)) ? " " : ",");
				s = s + "\"" + i + "\":{ \"shapeAnnotation\":[";
				bytes = Encoding.GetEncoding("UTF-8").GetBytes(s);
				stream.Write(bytes, 0, bytes.Length);
				flag = true;
			}
			int num = 0;
			foreach (PdfAnnotation annotation in pdfLoadedPage.Annotations)
			{
				num++;
				if (annotation is PdfLoadedAnnotation pdfLoadedAnnotation)
				{
					ExportAnnotationData(pdfLoadedAnnotation, i, pdfLoadedAnnotation.Dictionary);
					s = convertToJson(table);
					if (num < pdfLoadedPage.Annotations.Count)
					{
						s += ",";
					}
					bytes = Encoding.GetEncoding("UTF-8").GetBytes(s);
					stream.Write(bytes, 0, bytes.Length);
					table.Clear();
				}
			}
			if (pdfLoadedPage.Annotations.Count > 0)
			{
				s = "]}";
				bytes = Encoding.GetEncoding("UTF-8").GetBytes(s);
				stream.Write(bytes, 0, bytes.Length);
			}
		}
		s = "}}";
		bytes = Encoding.GetEncoding("UTF-8").GetBytes(s);
		stream.Write(bytes, 0, bytes.Length);
	}

	private void ExportAnnotationData(PdfLoadedAnnotation annotation, int pageIndex, PdfDictionary dictionary)
	{
		bool hasAppearance = false;
		string annotationType = GetAnnotationType(dictionary);
		m_skipBorderStyle = false;
		if (string.IsNullOrEmpty(annotationType))
		{
			return;
		}
		table.Add("type", annotationType);
		table.Add("page", pageIndex.ToString());
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
			table.Add("start", array[0].ToString(CultureInfo.InvariantCulture) + "," + array[1].ToString(CultureInfo.InvariantCulture));
			table.Add("end", array[2].ToString(CultureInfo.InvariantCulture) + "," + array[3].ToString(CultureInfo.InvariantCulture));
			break;
		}
		case "Stamp":
			hasAppearance = true;
			break;
		case "Square":
			hasAppearance = true;
			break;
		}
		if (dictionary != null && dictionary.ContainsKey("BE") && dictionary.ContainsKey("BS") && PdfCrossTable.Dereference(dictionary["BE"]) is PdfDictionary pdfDictionary && pdfDictionary.ContainsKey("S"))
		{
			m_skipBorderStyle = true;
		}
		WriteDictionary(annotation, pageIndex, dictionary, hasAppearance);
	}

	private void WriteDictionary(PdfLoadedAnnotation annotation, int pageIndex, PdfDictionary dictionary, bool hasAppearance)
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
			if (value == "P" || value == "Parent")
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
					WriteDictionary(annotation, pageIndex, pdfDictionary, hasAppearance: false);
					break;
				case "BE":
					WriteDictionary(annotation, pageIndex, pdfDictionary, hasAppearance: false);
					break;
				case "IRT":
					if (pdfDictionary.ContainsKey("NM"))
					{
						table.Add("inreplyto", GetValue(pdfDictionary["NM"]));
					}
					break;
				}
			}
			else if (value2 is PdfDictionary)
			{
				WriteDictionary(annotation, pageIndex, value2 as PdfDictionary, hasAppearance: false);
			}
			else if (!flag || (flag && value != "S"))
			{
				WriteAttribute(value, item.Value, pageIndex, dictionary);
			}
		}
		if (dictionary.ContainsKey("Measure"))
		{
			ExportMeasureDictionary(dictionary);
		}
		if ((ExportAppearance || hasAppearance) && dictionary.ContainsKey("AP"))
		{
			MemoryStream appearanceString = GetAppearanceString(dictionary["AP"]);
			if (appearanceString != null && appearanceString.Length > 0 && appearanceString.CanRead && appearanceString.CanSeek)
			{
				table.Add("appearance", Convert.ToBase64String(appearanceString.ToArray()));
			}
			if (appearanceString != null)
			{
				appearanceString.Dispose();
				appearanceString = null;
			}
		}
		if (dictionary.ContainsKey("Sound"))
		{
			IPdfPrimitive pdfPrimitive = dictionary["Sound"];
			if (!(((pdfPrimitive is PdfReferenceHolder) ? (pdfPrimitive as PdfReferenceHolder).Object : pdfPrimitive) is PdfStream pdfStream))
			{
				return;
			}
			if (pdfStream.ContainsKey("B"))
			{
				table.Add("bits", GetValue(pdfStream["B"]));
			}
			if (pdfStream.ContainsKey("C"))
			{
				table.Add("channels", GetValue(pdfStream["C"]));
			}
			if (pdfStream.ContainsKey("E"))
			{
				table.Add("encoding", GetValue(pdfStream["E"]));
			}
			if (pdfStream.ContainsKey("R"))
			{
				table.Add("rate", GetValue(pdfStream["R"]));
			}
			if (pdfStream.Data.Length == 0)
			{
				return;
			}
			string value3 = PdfString.BytesToHex(pdfStream.Data);
			if (!string.IsNullOrEmpty(value3))
			{
				table.Add("MODE".ToLower(), "raw");
				table.Add("encodings", "hex");
				if (pdfStream.ContainsKey("Length"))
				{
					table.Add("Length".ToLower(), GetValue(pdfStream["Length"]));
				}
				if (pdfStream.ContainsKey("Filter"))
				{
					table.Add("Filter".ToLower(), GetValue(pdfStream["Filter"]));
				}
				table.Add("data", value3);
			}
		}
		else
		{
			if (!dictionary.ContainsKey("FS"))
			{
				return;
			}
			IPdfPrimitive pdfPrimitive2 = dictionary["FS"];
			if (!(((pdfPrimitive2 is PdfReferenceHolder) ? (pdfPrimitive2 as PdfReferenceHolder).Object : pdfPrimitive2) is PdfDictionary pdfDictionary2))
			{
				return;
			}
			if (pdfDictionary2.ContainsKey("F"))
			{
				table.Add("file", GetValue(pdfDictionary2["F"]));
			}
			if (!pdfDictionary2.ContainsKey("EF"))
			{
				return;
			}
			pdfPrimitive2 = pdfDictionary2["EF"];
			if (!(((pdfPrimitive2 is PdfReferenceHolder) ? (pdfPrimitive2 as PdfReferenceHolder).Object : pdfPrimitive2) is PdfDictionary pdfDictionary3) || !pdfDictionary3.ContainsKey("F"))
			{
				return;
			}
			pdfPrimitive2 = pdfDictionary3["F"];
			if (!(((pdfPrimitive2 is PdfReferenceHolder) ? (pdfPrimitive2 as PdfReferenceHolder).Object : pdfPrimitive2) is PdfStream pdfStream2))
			{
				return;
			}
			if (pdfStream2.ContainsKey("Params"))
			{
				pdfPrimitive2 = pdfStream2["Params"];
				if (((pdfPrimitive2 is PdfReferenceHolder) ? (pdfPrimitive2 as PdfReferenceHolder).Object : pdfPrimitive2) is PdfDictionary pdfDictionary4)
				{
					if (pdfDictionary4.ContainsKey("CreationDate"))
					{
						PdfString dateTimeStringValue = PdfCrossTable.Dereference(pdfDictionary4["CreationDate"]) as PdfString;
						DateTime dateTime = dictionary.GetDateTime(dateTimeStringValue);
						table.Add("creation", dateTime.ToString());
					}
					if (pdfDictionary4.ContainsKey("ModDate"))
					{
						PdfString dateTimeStringValue2 = PdfCrossTable.Dereference(pdfDictionary4["CreationDate"]) as PdfString;
						DateTime dateTime2 = dictionary.GetDateTime(dateTimeStringValue2);
						table.Add("modification", dateTime2.ToString());
					}
					if (pdfDictionary4.ContainsKey("Size"))
					{
						table.Add("Size".ToLower(), GetValue(pdfDictionary4["Size"]));
					}
					if (pdfDictionary4.ContainsKey("CheckSum"))
					{
						string value4 = BitConverter.ToString(Encoding.UTF8.GetBytes(GetValue(pdfDictionary4["CheckSum"]))).Replace("-", "");
						table.Add("checksum", value4);
					}
				}
			}
			string value5 = PdfString.BytesToHex(pdfStream2.Data);
			if (!string.IsNullOrEmpty(value5))
			{
				table.Add("MODE".ToLower(), "RAW".ToLower());
				table.Add("Encoding".ToLower(), "HEX".ToLower());
				if (pdfStream2.ContainsKey("Length"))
				{
					table.Add("Length".ToLower(), GetValue(pdfStream2["Length"]));
				}
				if (pdfStream2.ContainsKey("Filter"))
				{
					table.Add("Filter".ToLower(), GetValue(pdfStream2["Filter"]));
				}
				table.Add("DATA".ToLower(), value5);
			}
		}
	}

	private string GetAnnotationType(PdfDictionary dictionary)
	{
		string result = string.Empty;
		if (dictionary.ContainsKey("Subtype"))
		{
			PdfName pdfName = PdfCrossTable.Dereference(dictionary["Subtype"]) as PdfName;
			if (pdfName != null)
			{
				result = pdfName.Value;
			}
		}
		return result;
	}

	private void WriteAttribute(string key, IPdfPrimitive primitive, int p, PdfDictionary dictionary)
	{
		if (key != null)
		{
			string color2;
			PdfString obj;
			Dictionary<string, string> dictionary4;
			switch (key.Length)
			{
			case 1:
				switch (key[0])
				{
				case 'C':
				{
					string color = GetColor(primitive);
					if (color != null)
					{
						table.Add("color", color);
					}
					return;
				}
				case 'M':
				{
					PdfString dateTimeStringValue = PdfCrossTable.Dereference(dictionary["M"]) as PdfString;
					DateTime dateTime = dictionary.GetDateTime(dateTimeStringValue);
					table.Add("date", dateTime.ToString());
					return;
				}
				case 'T':
					if (!table.ContainsKey("Title".ToLower()))
					{
						table.Add("Title".ToLower(), GetValue(primitive));
					}
					return;
				case 'W':
					table.Add("Width".ToLower(), GetValue(primitive));
					return;
				case 'S':
					break;
				case 'D':
					if (!table.ContainsKey("dashes"))
					{
						table.Add("dashes", GetValue(primitive));
					}
					return;
				case 'I':
					table.Add("intensity", GetValue(primitive));
					return;
				case 'F':
					if (primitive is PdfNumber pdfNumber)
					{
						string text2 = ((PdfAnnotationFlags)pdfNumber.IntValue).ToString().ToLower();
						text2 = text2.Replace(" ", "");
						table.Add("Flags".ToLower(), text2);
					}
					return;
				case 'A':
				case 'R':
				case 'X':
					goto IL_0ce6;
				default:
					goto end_IL_0012;
				case 'L':
				case 'P':
					return;
				}
				switch (GetValue(primitive))
				{
				case "D":
					table.Add("style", "dash");
					break;
				case "C":
					table.Add("style", "cloudy");
					break;
				case "S":
					table.Add("style", "solid");
					break;
				case "B":
					table.Add("style", "bevelled");
					break;
				case "I":
					table.Add("style", "inset");
					break;
				case "U":
					table.Add("style", "underline");
					break;
				}
				return;
			case 2:
			{
				switch (key[1])
				{
				case 'A':
					break;
				case 'C':
					goto IL_0242;
				case 'M':
					if (!(key == "NM"))
					{
						goto end_IL_0012;
					}
					table.Add("Name".ToLower(), GetValue(primitive));
					return;
				case 'E':
					goto IL_027c;
				case 'D':
					if (!(key == "RD"))
					{
						goto end_IL_0012;
					}
					table.Add("fringe", GetValue(primitive));
					return;
				case 'T':
					goto IL_02a6;
				case 'L':
					goto IL_02cb;
				case 'P':
					if (!(key == "CP"))
					{
						goto end_IL_0012;
					}
					table.Add("caption-style", GetValue(primitive));
					return;
				case 'S':
					goto IL_0305;
				case 'a':
					goto IL_032a;
				default:
					goto end_IL_0012;
				}
				if (!(key == "DA"))
				{
					if (!(key == "CA"))
					{
						break;
					}
					table.Add("opacity", GetValue(primitive));
					return;
				}
				PdfString pdfString2 = PdfCrossTable.Dereference(dictionary["DA"]) as PdfString;
				table.Add("defaultappearance", pdfString2.Value);
				return;
			}
			case 4:
				switch (key[0])
				{
				case 'N':
					if (!(key == "Name"))
					{
						break;
					}
					table.Add("icon", GetValue(primitive));
					return;
				case 'S':
					if (!(key == "Subj"))
					{
						break;
					}
					table.Add("Subject".ToLower(), GetValue(primitive));
					return;
				case 'R':
				{
					if (!(key == "Rect"))
					{
						break;
					}
					string[] array2 = GetValue(primitive).Split(',');
					Dictionary<string, string> dictionary3 = new Dictionary<string, string>();
					dictionary3.Add("x", array2[0]);
					dictionary3.Add("y", array2[1]);
					dictionary3.Add("width", array2[2]);
					dictionary3.Add("height", array2[3]);
					table.Add(key.ToLower(), convertToJson(dictionary3));
					return;
				}
				case 'T':
					if (!(key == "Type"))
					{
						break;
					}
					return;
				case 'I':
					if (!(key == "ITEx"))
					{
						break;
					}
					return;
				}
				break;
			case 12:
				switch (key[0])
				{
				case 'C':
				{
					if (!(key == "CreationDate"))
					{
						break;
					}
					PdfString dateTimeStringValue2 = PdfCrossTable.Dereference(dictionary["CreationDate"]) as PdfString;
					DateTime dateTime2 = dictionary.GetDateTime(dateTimeStringValue2);
					table.Add(key.ToLower(), dateTime2.ToString());
					return;
				}
				case 'G':
					if (!(key == "GroupNesting"))
					{
						break;
					}
					return;
				}
				break;
			case 6:
			{
				char c = key[0];
				if (c != 'B')
				{
					switch (c)
					{
					case 'R':
						if (!(key == "Rotate"))
						{
							break;
						}
						table.Add("rotation", GetValue(primitive));
						return;
					case 'P':
						if (!(key == "Parent"))
						{
							break;
						}
						return;
					}
					break;
				}
				if (!(key == "Border"))
				{
					break;
				}
				goto IL_0ce6;
			}
			case 3:
				switch (key[0])
				{
				case 'L':
					if (!(key == "LLE"))
					{
						break;
					}
					table.Add("leaderExtend", GetValue(primitive));
					return;
				case 'C':
					if (!(key == "Cap"))
					{
						break;
					}
					table.Add("caption", GetValue(primitive));
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
					if (PdfCrossTable.Dereference(dictionary["Contents"]) is PdfString pdfString)
					{
						string value = pdfString.Value;
						if (!string.IsNullOrEmpty(value))
						{
							table.Add("contents", GetValidString(value));
						}
					}
					return;
				case 'V':
				{
					if (!(key == "Vertices"))
					{
						break;
					}
					if (!(PdfCrossTable.Dereference(dictionary["Vertices"]) is PdfArray { Count: >0, Count: var num } pdfArray4) || num % 2 != 0)
					{
						return;
					}
					string text = string.Empty;
					for (int j = 0; j < num - 1; j++)
					{
						if (pdfArray4.Elements[j] is PdfNumber primitive2)
						{
							text = text + GetValue(primitive2) + ((j % 2 != 0) ? ";" : ",");
						}
					}
					if (pdfArray4.Elements[num - 1] is PdfNumber primitive3)
					{
						text += GetValue(primitive3);
					}
					if (!string.IsNullOrEmpty(text))
					{
						table.Add("vertices", text);
					}
					return;
				}
				}
				break;
			case 7:
				switch (key[0])
				{
				case 'I':
				{
					if (!(key == "InkList"))
					{
						break;
					}
					Dictionary<string, string> dictionary2 = new Dictionary<string, string>();
					if (PdfCrossTable.Dereference(dictionary["InkList"]) is PdfArray { Count: >0 } pdfArray2)
					{
						PdfArray[] array = new PdfArray[pdfArray2.Count];
						_ = new string[pdfArray2.Count];
						for (int i = 0; i < pdfArray2.Count; i++)
						{
							PdfArray pdfArray3 = pdfArray2[i] as PdfArray;
							array[i] = pdfArray3;
						}
						dictionary2.Add("gesture", convertToJsonArray(array));
						table.Add("inklist", convertToJson(dictionary2));
					}
					return;
				}
				case 'S':
					if (!(key == "Subtype"))
					{
						break;
					}
					return;
				}
				break;
			case 10:
				if (!(key == "QuadPoints"))
				{
					break;
				}
				table.Add("Coords".ToLower(), GetValue(primitive));
				return;
			case 16:
				if (!(key == "MeasurementTypes"))
				{
					break;
				}
				return;
			case 5:
				{
					if (!(key == "Sound"))
					{
						break;
					}
					return;
				}
				IL_027c:
				if (!(key == "LE"))
				{
					break;
				}
				if (primitive is PdfArray)
				{
					PdfArray pdfArray = primitive as PdfArray;
					if (pdfArray.Count == 2)
					{
						table.Add("head", GetValue(pdfArray.Elements[0]));
						table.Add("tail", GetValue(pdfArray.Elements[1]));
					}
				}
				else if (primitive is PdfName)
				{
					table.Add("head", GetValue(primitive));
				}
				return;
				IL_0242:
				if (!(key == "IC"))
				{
					if (!(key == "RC"))
					{
						break;
					}
					if (dictionary.ContainsKey("RC"))
					{
						string text3 = (dictionary["RC"] as PdfString).Value;
						int num2 = text3.IndexOf("<body");
						if (num2 > 0)
						{
							text3 = text3.Substring(num2);
						}
						table.Add("contents-richtext", GetValidString(text3));
					}
					return;
				}
				color2 = GetColor(primitive);
				if (color2 != null)
				{
					table.Add("interior-color", color2);
				}
				return;
				IL_032a:
				if (!(key == "ca"))
				{
					break;
				}
				goto IL_0ce6;
				IL_0305:
				if (!(key == "DS"))
				{
					if (!(key == "FS"))
					{
						break;
					}
					return;
				}
				if (!dictionary.ContainsKey("DS"))
				{
					return;
				}
				obj = PdfCrossTable.Dereference(dictionary["DS"]) as PdfString;
				dictionary4 = new Dictionary<string, string>();
				if (obj != null)
				{
					string[] array3 = (dictionary["DS"] as PdfString).Value.Split(new char[1] { ';' });
					for (int k = 0; k < array3.Length; k++)
					{
						if (!string.IsNullOrEmpty(array3[k]) && array3[k].Contains(","))
						{
							array3[k] = XmlConvert.EncodeName(array3[k]);
						}
						string[] array4 = array3[k].Split(new char[1] { ':' });
						dictionary4.Add(array4[0], array4[1]);
					}
				}
				table.Add("defaultStyle", convertToJson(dictionary4));
				return;
				IL_02a6:
				if (!(key == "IT"))
				{
					if (!(key == "RT"))
					{
						break;
					}
					table.Add("replyType", GetValue(primitive).ToLower());
					return;
				}
				table.Add(key, GetValue(primitive));
				return;
				IL_02cb:
				if (!(key == "LL"))
				{
					if (!(key == "CL"))
					{
						break;
					}
					table.Add("callout", GetValue(primitive));
					return;
				}
				table.Add("leaderLength", GetValue(primitive));
				return;
				IL_0ce6:
				table.Add(key.ToLower(), GetValue(primitive));
				return;
				end_IL_0012:
				break;
			}
		}
		table.Add(key, GetValue(primitive));
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
			text = ((primitive as PdfBoolean).Value ? "true" : "false");
		}
		else if (primitive is PdfString)
		{
			text = (primitive as PdfString).Value;
			if (text.StartsWith("[") || text.StartsWith("{"))
			{
				text = " " + text;
			}
			text = GetValidString(text);
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
		return text;
	}

	private string GetValidString(string value)
	{
		if (value.Contains("\""))
		{
			value = ((!Regex.Match(value, "(?<!\\\\)\"").Success) ? value.Replace("\"", "\\\"") : Regex.Replace(value, "(?<!\\\\)\"", "\\\""));
		}
		if (value.Contains("\r"))
		{
			value = value.Replace("\r", "\\r");
		}
		if (value.Contains("\n"))
		{
			value = value.Replace("\n", "\\n");
		}
		return value;
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

	private string convertToJson(Dictionary<string, string> value)
	{
		int num = 0;
		string text = "{";
		foreach (KeyValuePair<string, string> item in value)
		{
			string text2 = item.Value;
			if (text2.StartsWith("{") || text2.StartsWith("["))
			{
				text = text + "\"" + XmlConvert.EncodeName(Convert.ToString(item.Key)) + "\":" + Convert.ToString(text2);
			}
			else
			{
				if (text2.StartsWith(" ") && text2.Length > 1 && (text2[1] == '[' || text2[1] == '{'))
				{
					text2 = text2.TrimStart();
				}
				text = text + "\"" + XmlConvert.EncodeName(Convert.ToString(item.Key)) + "\":\"" + Convert.ToString(text2) + "\"";
			}
			if (num < value.Count - 1)
			{
				text += ",";
			}
			num++;
		}
		return text + "}";
	}

	private string convertToJsonArray(PdfArray[] value)
	{
		string text = "[";
		for (int i = 0; i < value.Length; i++)
		{
			text = text + "[" + GetValue(value[i]) + "]";
			if (i < value.Length - 1)
			{
				text += ",";
			}
		}
		return text + "]";
	}

	private void ExportMeasureDictionary(PdfDictionary dictionary)
	{
		IPdfPrimitive pdfPrimitive = dictionary["Measure"];
		if (((pdfPrimitive is PdfReferenceHolder) ? (pdfPrimitive as PdfReferenceHolder).Object : pdfPrimitive) is PdfDictionary pdfDictionary)
		{
			if (pdfDictionary.ContainsKey("Type"))
			{
				table.Add("type1", "Measure");
			}
			if (pdfDictionary.ContainsKey("R"))
			{
				table.Add("ratevalue", GetValue(pdfDictionary["R"]));
			}
			if (pdfDictionary.ContainsKey("Subtype"))
			{
				table.Add("SubType", GetValue(pdfDictionary["Subtype"]));
			}
			if (pdfDictionary.ContainsKey("TargetUnitConversion"))
			{
				table.Add("TargetUnitConversion", GetValue(pdfDictionary["TargetUnitConversion"]));
			}
			if (pdfDictionary.ContainsKey("A") && pdfDictionary["A"] is PdfArray pdfArray && pdfArray.Elements.Count > 0)
			{
				IPdfPrimitive pdfPrimitive2 = pdfArray.Elements[0];
				PdfDictionary measurementDetails = ((pdfPrimitive2 is PdfReferenceHolder) ? (pdfPrimitive2 as PdfReferenceHolder).Object : pdfPrimitive2) as PdfDictionary;
				ExportMeasureFormatDetails("area", measurementDetails);
			}
			if (pdfDictionary.ContainsKey("D") && pdfDictionary["D"] is PdfArray pdfArray2 && pdfArray2.Elements.Count > 0)
			{
				IPdfPrimitive pdfPrimitive3 = pdfArray2.Elements[0];
				PdfDictionary measurementDetails2 = ((pdfPrimitive3 is PdfReferenceHolder) ? (pdfPrimitive3 as PdfReferenceHolder).Object : pdfPrimitive3) as PdfDictionary;
				ExportMeasureFormatDetails("distance", measurementDetails2);
			}
			if (pdfDictionary.ContainsKey("X") && pdfDictionary["X"] is PdfArray pdfArray3 && pdfArray3.Elements.Count > 0)
			{
				IPdfPrimitive pdfPrimitive4 = pdfArray3.Elements[0];
				PdfDictionary measurementDetails3 = ((pdfPrimitive4 is PdfReferenceHolder) ? (pdfPrimitive4 as PdfReferenceHolder).Object : pdfPrimitive4) as PdfDictionary;
				ExportMeasureFormatDetails("xformat", measurementDetails3);
			}
			if (pdfDictionary.ContainsKey("T") && pdfDictionary["T"] is PdfArray pdfArray4 && pdfArray4.Elements.Count > 0)
			{
				IPdfPrimitive pdfPrimitive5 = pdfArray4.Elements[0];
				PdfDictionary measurementDetails4 = ((pdfPrimitive5 is PdfReferenceHolder) ? (pdfPrimitive5 as PdfReferenceHolder).Object : pdfPrimitive5) as PdfDictionary;
				ExportMeasureFormatDetails("tformat", measurementDetails4);
			}
			if (pdfDictionary.ContainsKey("V") && pdfDictionary["V"] is PdfArray pdfArray5 && pdfArray5.Elements.Count > 0)
			{
				IPdfPrimitive pdfPrimitive6 = pdfArray5.Elements[0];
				PdfDictionary measurementDetails5 = ((pdfPrimitive6 is PdfReferenceHolder) ? (pdfPrimitive6 as PdfReferenceHolder).Object : pdfPrimitive6) as PdfDictionary;
				ExportMeasureFormatDetails("vformat", measurementDetails5);
			}
		}
	}

	private void ExportMeasureFormatDetails(string key, PdfDictionary measurementDetails)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		if (measurementDetails.ContainsKey("C"))
		{
			dictionary.Add("c", GetValue(measurementDetails["C"]));
		}
		if (measurementDetails.ContainsKey("F"))
		{
			dictionary.Add("f", GetValue(measurementDetails["F"]));
		}
		if (measurementDetails.ContainsKey("D"))
		{
			dictionary.Add("d", GetValue(measurementDetails["D"]));
		}
		if (measurementDetails.ContainsKey("RD"))
		{
			dictionary.Add("rd", GetValue(measurementDetails["RD"]));
		}
		if (measurementDetails.ContainsKey("U"))
		{
			dictionary.Add("u", GetValue(measurementDetails["U"]));
		}
		if (measurementDetails.ContainsKey("RT"))
		{
			dictionary.Add("rt", GetValue(measurementDetails["RT"]));
		}
		if (measurementDetails.ContainsKey("SS"))
		{
			dictionary.Add("ss", GetValue(measurementDetails["SS"]));
		}
		if (measurementDetails.ContainsKey("FD"))
		{
			dictionary.Add("fd", GetValue(measurementDetails["FD"]));
		}
		if (measurementDetails.ContainsKey("Type"))
		{
			dictionary.Add("Type", GetValue(measurementDetails["Type"]));
		}
		table.Add(key, convertToJson(dictionary));
	}

	private MemoryStream GetAppearanceString(IPdfPrimitive primitive)
	{
		MemoryStream memoryStream = new MemoryStream();
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		Dictionary<string, string> dictionary2 = new Dictionary<string, string>();
		if (PdfCrossTable.Dereference(primitive) is PdfDictionary dictionary3)
		{
			WriteAppearanceDictionary(dictionary, dictionary3);
		}
		dictionary2.Add("ap", convertToJson(dictionary));
		string s = convertToJson(dictionary2);
		byte[] bytes = Encoding.GetEncoding("UTF-8").GetBytes(s);
		memoryStream.Write(bytes, 0, bytes.Length);
		return memoryStream;
	}

	private void WriteAppearanceDictionary(Dictionary<string, string> textWriter, PdfDictionary dictionary)
	{
		if (dictionary == null || dictionary.Count <= 0)
		{
			return;
		}
		foreach (KeyValuePair<PdfName, IPdfPrimitive> item in dictionary.Items)
		{
			WriteObject(textWriter, item.Key.Value, item.Value, null);
		}
	}

	private void WriteObject(Dictionary<string, string> textWriter, string key, IPdfPrimitive primitive, List<Dictionary<string, string>> arrayWriter)
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
				PdfStream pdfStream = null;
				pdfStream = ((m_document == null || m_document.CrossTable == null) ? (primitive as PdfStream) : ((primitive as PdfStream).Clone(m_document.CrossTable) as PdfStream));
				if (pdfStream == null || pdfStream.Data.Length == 0)
				{
					break;
				}
				Dictionary<string, string> dictionary8 = new Dictionary<string, string>();
				WriteAppearanceDictionary(dictionary8, pdfStream);
				Dictionary<string, string> dictionary9 = new Dictionary<string, string>();
				string value = GetValue(pdfStream["Subtype"]);
				if (pdfStream.ContainsKey("Subtype") && "Image" == value)
				{
					dictionary9.Add("mode", "raw");
					dictionary9.Add("encoding", "hex");
					string value2 = BitConverter.ToString(pdfStream.Data).Replace("-", "");
					if (!string.IsNullOrEmpty(value2))
					{
						dictionary9.Add("bytes", value2);
					}
				}
				else if (!pdfStream.ContainsKey("Type") && !pdfStream.ContainsKey("Subtype"))
				{
					dictionary9.Add("mode", "raw");
					dictionary9.Add("encoding", "hex");
					string value3 = BitConverter.ToString(pdfStream.Data).Replace("-", "");
					if (!string.IsNullOrEmpty(value3))
					{
						dictionary9.Add("bytes", value3);
					}
				}
				else if (pdfStream.ContainsKey("Subtype") && ("Form" == value || "CIDFontType0C" == value || "OpenType" == value))
				{
					dictionary9.Add("mode", "raw");
					dictionary9.Add("encoding", "hex");
					string value4 = BitConverter.ToString(pdfStream.GetDecompressedData()).Replace("-", "");
					if (!string.IsNullOrEmpty(value4))
					{
						dictionary9.Add("bytes", value4);
					}
				}
				else
				{
					dictionary9.Add("mode", "filtered");
					dictionary9.Add("encoding", "ascii");
					byte[] decompressedData = pdfStream.GetDecompressedData();
					string @string = Encoding.UTF8.GetString(decompressedData, 0, decompressedData.Length);
					if (!string.IsNullOrEmpty(@string))
					{
						dictionary9["bytes"] = @string;
					}
				}
				dictionary8["data"] = convertToJson(dictionary9);
				Dictionary<string, string> dictionary10 = new Dictionary<string, string>();
				dictionary10["stream"] = convertToJson(dictionary8);
				if (key != null)
				{
					textWriter.Add(key, convertToJson(dictionary10));
				}
				else
				{
					arrayWriter.Add(dictionary10);
				}
				break;
			}
			case 'i':
			{
				if (!(text == "DocGen.Pdf.Primitives.PdfString") || !(primitive is PdfString pdfString))
				{
					break;
				}
				Dictionary<string, string> dictionary11 = new Dictionary<string, string>();
				if (PdfString.IsUnicode(pdfString.Value))
				{
					Dictionary<string, string> dictionary12 = new Dictionary<string, string>();
					dictionary12.Add("encoding", "hex");
					string value5 = BitConverter.ToString(pdfString.Bytes).Replace("-", "");
					if (!string.IsNullOrEmpty(value5))
					{
						dictionary12.Add("bytes", value5);
					}
					dictionary11.Add("string", convertToJson(dictionary12));
				}
				else
				{
					dictionary11.Add("string", pdfString.Value);
				}
				if (key != null)
				{
					textWriter.Add(key, convertToJson(dictionary11));
				}
				else
				{
					arrayWriter.Add(dictionary11);
				}
				break;
			}
			case 'b':
			{
				if (!(text == "DocGen.Pdf.Primitives.PdfNumber") || !(primitive is PdfNumber pdfNumber))
				{
					break;
				}
				if (pdfNumber.IsInteger)
				{
					Dictionary<string, string> dictionary5 = new Dictionary<string, string>();
					dictionary5.Add("int", pdfNumber.IntValue.ToString());
					if (key != null)
					{
						textWriter.Add(key, convertToJson(dictionary5));
					}
					else
					{
						arrayWriter.Add(dictionary5);
					}
					break;
				}
				if (pdfNumber.IsLong)
				{
					Dictionary<string, string> dictionary6 = new Dictionary<string, string>();
					dictionary6.Add("int", pdfNumber.LongValue.ToString());
					if (key != null)
					{
						textWriter.Add(key, convertToJson(dictionary6));
					}
					else
					{
						arrayWriter.Add(dictionary6);
					}
					break;
				}
				string text2 = Math.Round(pdfNumber.FloatValue, 6).ToString(CultureInfo.InvariantCulture);
				if (!text2.Contains("."))
				{
					text2 += ".000000";
				}
				Dictionary<string, string> dictionary7 = new Dictionary<string, string>();
				dictionary7.Add("fixed", text2.ToString());
				if (key != null)
				{
					textWriter.Add(key, convertToJson(dictionary7));
				}
				else
				{
					arrayWriter.Add(dictionary7);
				}
				break;
			}
			}
			break;
		case 33:
			switch (text[30])
			{
			case 'a':
			{
				if (!(text == "DocGen.Pdf.Primitives.PdfName"))
				{
					break;
				}
				PdfName pdfName = primitive as PdfName;
				if (pdfName != null)
				{
					Dictionary<string, string> dictionary15 = new Dictionary<string, string>();
					dictionary15.Add("name", pdfName.Value);
					if (key != null)
					{
						textWriter.Add(key, convertToJson(dictionary15));
					}
					else
					{
						arrayWriter.Add(dictionary15);
					}
				}
				break;
			}
			case 'u':
				if (text == "DocGen.Pdf.Primitives.PdfNull")
				{
					Dictionary<string, string> dictionary14 = new Dictionary<string, string>();
					dictionary14.Add("null", "null");
					if (key != null)
					{
						textWriter.Add(key, convertToJson(dictionary14));
					}
					else
					{
						arrayWriter.Add(dictionary14);
					}
				}
				break;
			}
			break;
		case 44:
			if (text == "DocGen.Pdf.Primitives.PdfReferenceHolder")
			{
				PdfReferenceHolder pdfReferenceHolder = primitive as PdfReferenceHolder;
				if (pdfReferenceHolder != null)
				{
					WriteObject(textWriter, key, pdfReferenceHolder.Object, arrayWriter);
				}
			}
			break;
		case 39:
			if (text == "DocGen.Pdf.Primitives.PdfDictionary" && primitive is PdfDictionary dictionary2)
			{
				Dictionary<string, string> dictionary3 = new Dictionary<string, string>();
				Dictionary<string, string> dictionary4 = new Dictionary<string, string>();
				WriteAppearanceDictionary(dictionary4, dictionary2);
				dictionary3.Add("dict", convertToJson(dictionary4));
				if (key != null)
				{
					textWriter.Add(key, convertToJson(dictionary3));
				}
				else
				{
					arrayWriter.Add(dictionary3);
				}
			}
			break;
		case 36:
			if (text == "DocGen.Pdf.Primitives.PdfBoolean" && primitive is PdfBoolean pdfBoolean)
			{
				Dictionary<string, string> dictionary13 = new Dictionary<string, string>();
				dictionary13.Add("boolean", pdfBoolean.Value ? "true" : "false");
				if (key != null)
				{
					textWriter.Add(key, convertToJson(dictionary13));
				}
				else
				{
					arrayWriter.Add(dictionary13);
				}
			}
			break;
		case 34:
			if (text == "DocGen.Pdf.Primitives.PdfArray")
			{
				List<Dictionary<string, string>> list = new List<Dictionary<string, string>>();
				WriteArray(list, primitive as PdfArray);
				Dictionary<string, string> dictionary = new Dictionary<string, string>();
				dictionary.Add("array", convertListToJson(list));
				if (key != null)
				{
					textWriter.Add(key, convertToJson(dictionary));
				}
				else
				{
					arrayWriter.Add(dictionary);
				}
			}
			break;
		}
	}

	private void WriteArray(List<Dictionary<string, string>> textWriter, PdfArray array)
	{
		if (array == null)
		{
			return;
		}
		foreach (IPdfPrimitive element in array.Elements)
		{
			WriteObject(null, null, element, textWriter);
		}
	}

	private string convertListToJson(List<Dictionary<string, string>> value)
	{
		string text = "[";
		for (int i = 0; i < value.Count; i++)
		{
			text += convertToJson(value[i]);
			if (i < value.Count - 1)
			{
				text += ",";
			}
		}
		return text + "]";
	}

	internal string GetJsonAppearanceString(IPdfPrimitive primitive)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		if (PdfCrossTable.Dereference(primitive) is PdfDictionary primitive2)
		{
			WriteObject(dictionary, "N", primitive2, null);
		}
		return convertToJson(dictionary);
	}
}
