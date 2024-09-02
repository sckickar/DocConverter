using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using DocGen.Pdf.Interactive;
using DocGen.Pdf.IO;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Parsing;

internal class JsonParser
{
	private PdfLoadedDocument m_document;

	private string annotation = string.Empty;

	private string beginLineStyle = string.Empty;

	private string endLineStyle = string.Empty;

	private string values = string.Empty;

	private bool isBasicStyle = true;

	private string style = string.Empty;

	private Dictionary<string, string> dataStream = new Dictionary<string, string>();

	private Dictionary<string, string> fields = new Dictionary<string, string>();

	private List<PdfDictionary> m_groupHolders = new List<PdfDictionary>();

	private Dictionary<string, PdfReferenceHolder> m_groupReferences = new Dictionary<string, PdfReferenceHolder>();

	private bool m_isNormalAppearanceAdded;

	internal JsonParser(Stream stream, PdfLoadedDocument document)
	{
		m_document = document;
	}

	internal void ImportAnnotationData(Stream stream)
	{
		if (stream == null)
		{
			return;
		}
		string text = null;
		string text2 = null;
		stream.Position = 0L;
		PdfReader pdfReader = new PdfReader(stream);
		string nextJsonToken = pdfReader.GetNextJsonToken();
		pdfReader.Position = 0L;
		while (nextJsonToken != null && nextJsonToken != string.Empty)
		{
			if (nextJsonToken == "type")
			{
				while (nextJsonToken != "}")
				{
					if (nextJsonToken != "{" && nextJsonToken != "}" && nextJsonToken != "\"" && nextJsonToken != ",")
					{
						text = nextJsonToken;
						do
						{
							nextJsonToken = pdfReader.GetNextJsonToken();
						}
						while (nextJsonToken != ":");
						nextJsonToken = pdfReader.GetNextJsonToken();
						if (nextJsonToken == "{")
						{
							text2 = getJsonObject(nextJsonToken, text, pdfReader);
						}
						else if (nextJsonToken == "[")
						{
							string text3 = string.Empty;
							while (nextJsonToken != "]")
							{
								text3 += nextJsonToken;
								nextJsonToken = pdfReader.GetNextJsonToken();
							}
							text2 = text3 + "]";
							text2 = text2.Replace("\\r\\n", "\r\n");
						}
						else
						{
							nextJsonToken = pdfReader.GetNextJsonToken();
							string text4 = string.Empty;
							pdfReader.m_importFormData = true;
							while (nextJsonToken != "\"")
							{
								text4 += nextJsonToken;
								nextJsonToken = pdfReader.GetNextJsonToken();
							}
							pdfReader.m_importFormData = false;
							text2 = text4;
						}
					}
					if (text != null && text2 != null)
					{
						fields.Add(text, XmlConvert.DecodeName(text2));
						text = null;
						text2 = null;
					}
					nextJsonToken = pdfReader.GetNextJsonToken();
				}
				if (fields.Count > 0)
				{
					parseAnnotationData(fields);
					fields.Clear();
				}
			}
			nextJsonToken = pdfReader.GetNextJsonToken();
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
		stream.Dispose();
	}

	private void parseAnnotationData(Dictionary<string, string> annotData)
	{
		int result = -1;
		string s = string.Empty;
		string type = string.Empty;
		foreach (KeyValuePair<string, string> annotDatum in annotData)
		{
			if (annotDatum.Key == "page")
			{
				s = annotDatum.Value;
			}
			if (annotDatum.Key == "type")
			{
				type = annotDatum.Value;
			}
		}
		int.TryParse(s, out result);
		if (result >= 0 && result < m_document.Pages.Count)
		{
			(m_document.Pages[result] as PdfLoadedPage).importAnnotation = true;
			PdfDictionary annotationData = GetAnnotationData(type, result, annotData);
			if (annotationData.Count > 0)
			{
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
						pdfArray.MarkChanged();
					}
				}
			}
		}
		beginLineStyle = string.Empty;
		endLineStyle = string.Empty;
	}

	private PdfDictionary GetAnnotationData(string type, int pageindex, Dictionary<string, string> key_Value)
	{
		PdfDictionary pdfDictionary = new PdfDictionary();
		pdfDictionary.SetName("Type", "Annot");
		bool flag = true;
		switch (type.ToLower())
		{
		case "line":
			pdfDictionary.SetName("Subtype", "Line");
			break;
		case "circle":
			pdfDictionary.SetName("Subtype", "Circle");
			break;
		case "square":
			pdfDictionary.SetName("Subtype", "Square");
			break;
		case "polyline":
			pdfDictionary.SetName("Subtype", "PolyLine");
			break;
		case "polygon":
			pdfDictionary.SetName("Subtype", "Polygon");
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
		case "redact":
			pdfDictionary.SetName("Subtype", "Redact");
			annotation = "redact";
			break;
		case "caret":
			pdfDictionary.SetName("Subtype", "Caret");
			break;
		default:
			flag = false;
			break;
		}
		if (flag)
		{
			AddAnnotationData(pdfDictionary, key_Value, pageindex);
		}
		return pdfDictionary;
	}

	private void AddAnnotationData(PdfDictionary annotDictionary, Dictionary<string, string> key_Value, int index)
	{
		List<float> list = new List<float>();
		PdfDictionary pdfDictionary = new PdfDictionary();
		PdfDictionary pdfDictionary2 = new PdfDictionary();
		foreach (KeyValuePair<string, string> item in key_Value)
		{
			string value = item.Value;
			switch (item.Key.ToLower())
			{
			case "start":
			case "end":
				AddLinePoints(list, item.Value);
				if (list.Count == 4)
				{
					annotDictionary.SetProperty("L", new PdfArray(list.ToArray()));
					list.Clear();
					list = null;
				}
				break;
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
			case "style":
			case "width":
			case "intensity":
			case "dashes":
				AddBorderStyle(item.Key, item.Value, pdfDictionary, pdfDictionary2);
				break;
			case "rect":
			{
				float[] array6 = ObtainFloatPoints(value);
				if (array6 != null && array6.Length == 4)
				{
					annotDictionary.SetProperty("Rect", new PdfArray(array6));
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
			case "oc":
				if (annotation == "redact" && !string.IsNullOrEmpty(value))
				{
					PdfArray colorArray = GetColorArray(value);
					if (colorArray != null)
					{
						annotDictionary.SetProperty("IC", colorArray);
					}
				}
				break;
			case "interior-color":
				if (!string.IsNullOrEmpty(value))
				{
					PdfArray colorArray3 = GetColorArray(value);
					if (colorArray3 != null)
					{
						annotDictionary.SetProperty("IC", colorArray3);
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
				AddString(annotDictionary, "Subj", value);
				break;
			case "title":
				AddString(annotDictionary, "T", value);
				break;
			case "rotation":
				if (value.GetType() == typeof(int))
				{
					AddInt(annotDictionary, "Rotate", value);
				}
				else
				{
					AddFloat(annotDictionary, "Rotate", value);
				}
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
				if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result3))
				{
					annotDictionary.SetNumber("LLE", result3);
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
				if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
				{
					annotDictionary.SetNumber("CA", result);
				}
				break;
			}
			case "defaultstyle":
				AddString(annotDictionary, "DS", item.Value);
				break;
			case "defaultappearance":
				AddString(annotDictionary, "DA", item.Value);
				break;
			case "contents-richtext":
			{
				string str = TrimEscapeCharacters(item.Value);
				annotDictionary.SetString("RC", str);
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
					string[] array5 = value.Split(',');
					for (int k = 0; k < array5.Length; k++)
					{
						PdfAnnotationFlags pdfAnnotationFlags2 = MapAnnotationFlags(array5[k]);
						pdfAnnotationFlags = ((k != 0) ? (pdfAnnotationFlags | pdfAnnotationFlags2) : pdfAnnotationFlags2);
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
			case "repeat":
				if (!string.IsNullOrEmpty(value))
				{
					annotDictionary?.SetBoolean("Repeat", (value == "true" || value == "yes") ? true : false);
				}
				break;
			case "overlaytext":
				annotDictionary.SetString("OverlayText", value);
				break;
			case "contents":
			{
				string text = TrimEscapeCharacters(item.Value);
				if (!string.IsNullOrEmpty(text))
				{
					annotDictionary.SetString("Contents", text);
				}
				break;
			}
			case "q":
			{
				if (int.TryParse(value, out var result2))
				{
					annotDictionary.SetNumber("Q", result2);
				}
				break;
			}
			case "inklist":
			{
				PdfArray pdfArray = new PdfArray();
				string[] array2 = item.Value.Replace("gesture:[", "").Replace("[", "").Split(new string[2] { "]", "];" }, StringSplitOptions.None);
				for (int i = 0; i < array2.Length; i++)
				{
					string[] array3 = array2[i].Split(',', ';');
					if (array3 != null && array3.Length != 0)
					{
						List<float> list3 = new List<float>();
						string[] array4 = array3;
						foreach (string value3 in array4)
						{
							AddFloatPoints(list3, value3);
						}
						if (list3.Count > 0 && list3.Count % 2 == 0)
						{
							pdfArray.Add(new PdfArray(list3.ToArray()));
						}
						list3.Clear();
					}
				}
				annotDictionary.SetProperty("InkList", pdfArray);
				break;
			}
			case "head":
				beginLineStyle = MapLineEndingStyle(item.Value).ToString();
				break;
			case "tail":
				endLineStyle = MapLineEndingStyle(item.Value).ToString();
				break;
			case "rate":
			case "file":
			case "bits":
			case "mode":
			case "size":
			case "length":
			case "filter":
			case "modification":
			case "creation":
			case "channels":
			case "encoding":
				dataStream.Add(item.Key, item.Value);
				break;
			case "data":
				values = item.Value;
				break;
			case "vertices":
			{
				if (string.IsNullOrEmpty(item.Value))
				{
					break;
				}
				string[] array = value.Split(',', ';');
				if (array != null && array.Length != 0)
				{
					List<float> list2 = new List<float>();
					string[] array2 = array;
					foreach (string value2 in array2)
					{
						AddFloatPoints(list2, value2);
					}
					if (list2.Count > 0 && list2.Count % 2 == 0)
					{
						annotDictionary.SetProperty("Vertices", new PdfArray(list2.ToArray()));
					}
				}
				break;
			}
			case "customdata":
				AddString(annotDictionary, "CustomData", TrimEscapeCharacters(value));
				break;
			case "appearance":
				AddAppearanceData(annotDictionary, value);
				break;
			default:
				if (m_document.ImportCustomData && item.Key != "type" && item.Key != "page")
				{
					annotDictionary.SetString(item.Key, TrimEscapeCharacters(value));
				}
				break;
			case "itex":
				break;
			}
		}
		AddMeasureDictionary(annotDictionary, key_Value);
		if (!string.IsNullOrEmpty(beginLineStyle))
		{
			if (!string.IsNullOrEmpty(endLineStyle))
			{
				PdfArray pdfArray2 = new PdfArray();
				pdfArray2.Add(new PdfName(beginLineStyle));
				pdfArray2.Add(new PdfName(endLineStyle));
				annotDictionary.SetProperty("LE", pdfArray2);
			}
			else
			{
				annotDictionary.SetName("LE", beginLineStyle);
			}
		}
		else if (!string.IsNullOrEmpty(endLineStyle))
		{
			annotDictionary.SetName("LE", beginLineStyle);
		}
		if (pdfDictionary2.Count > 0)
		{
			pdfDictionary2.SetProperty("Type", new PdfName("Border"));
			annotDictionary.SetProperty("BS", new PdfReferenceHolder(pdfDictionary2));
		}
		if (pdfDictionary.Count > 0)
		{
			annotDictionary.SetProperty("BE", new PdfReferenceHolder(pdfDictionary));
		}
		AddStreamData(dataStream, annotDictionary, values);
	}

	private string TrimEscapeCharacters(string value)
	{
		if (value != null)
		{
			if (value.Contains("\\r"))
			{
				value = value.Replace("\\r", "\r");
			}
			if (value.Contains("\\n"))
			{
				value = value.Replace("\\n", "\n");
			}
			if (value.Contains("\\\""))
			{
				value = value.Replace("\\\"", "\"");
			}
		}
		return value;
	}

	private void AddStreamData(Dictionary<string, string> dataValues, PdfDictionary annotDictionary, string values)
	{
		string value = (annotDictionary["Subtype"] as PdfName).Value;
		byte[] bytes = GetBytes(values);
		if (bytes == null || bytes.Length == 0)
		{
			return;
		}
		if (value == "sound")
		{
			PdfStream pdfStream = new PdfStream();
			pdfStream.SetName("Type", "Sound");
			foreach (KeyValuePair<string, string> dataValue in dataValues)
			{
				switch (dataValue.Key)
				{
				case "bits":
				case "rate":
				case "channels":
					AddInt(pdfStream, dataValue.Key, dataValue.Value);
					break;
				case "encoding":
					if (!string.IsNullOrEmpty(dataValue.Value))
					{
						pdfStream.SetName("E", dataValue.Value);
					}
					break;
				case "filter":
					pdfStream.AddFilter("FlateDecode");
					break;
				}
			}
			pdfStream.Data = bytes;
			annotDictionary.SetProperty("Sound", new PdfReferenceHolder(pdfStream));
		}
		else
		{
			if (!(value == "FileAttachment"))
			{
				return;
			}
			PdfDictionary pdfDictionary = new PdfDictionary();
			PdfStream pdfStream2 = new PdfStream();
			PdfDictionary pdfDictionary2 = new PdfDictionary();
			pdfDictionary.SetName("Type", "Filespec");
			foreach (KeyValuePair<string, string> dataValue2 in dataValues)
			{
				switch (dataValue2.Key)
				{
				case "file":
					AddString(pdfDictionary, "F", dataValue2.Value);
					AddString(pdfDictionary, "UF", dataValue2.Value);
					break;
				case "size":
				{
					if (int.TryParse(dataValue2.Value, out var result))
					{
						pdfDictionary2.SetNumber("Size", result);
						pdfStream2.SetNumber("DL", result);
					}
					break;
				}
				case "creation":
					AddString(pdfDictionary2, "creation", "CreationDate");
					break;
				case "modification":
					AddString(pdfDictionary2, "modification", "ModDate");
					break;
				}
			}
			pdfStream2.SetProperty("Params", pdfDictionary2);
			pdfStream2.Data = bytes;
			pdfStream2.AddFilter("FlateDecode");
			PdfDictionary pdfDictionary3 = new PdfDictionary();
			pdfDictionary3.SetProperty("F", new PdfReferenceHolder(pdfStream2));
			pdfDictionary.SetProperty("EF", pdfDictionary3);
			annotDictionary.SetProperty("FS", new PdfReferenceHolder(pdfDictionary));
		}
	}

	private void AddInt(PdfDictionary dictionary, string key, string value)
	{
		if (int.TryParse(value, out var result))
		{
			dictionary.SetNumber(key, result);
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

	private void AddString(PdfDictionary dictionary, string key, string value)
	{
		if (!string.IsNullOrEmpty(value))
		{
			dictionary.SetString(key, value);
		}
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

	private void AddBorderStyle(string key, string value, PdfDictionary borderEffectDictionary, PdfDictionary borderStyleDictionary)
	{
		switch (value)
		{
		case "dash":
			style = "D";
			break;
		case "solid":
			style = "S";
			break;
		case "bevelled":
			style = "B";
			break;
		case "inset":
			style = "I";
			break;
		case "underline":
			style = "U";
			break;
		case "cloudy":
			style = "C";
			isBasicStyle = false;
			break;
		}
		if (key == "width" && float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
		{
			borderStyleDictionary.SetNumber("W", result);
		}
		if (key == "intensity" && float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result2))
		{
			borderEffectDictionary.SetNumber("I", result2);
		}
		if (!string.IsNullOrEmpty(style))
		{
			(isBasicStyle ? borderStyleDictionary : borderEffectDictionary).SetName("S", style);
		}
		if (key == "dashes")
		{
			float[] array = ObtainFloatPoints(value);
			if (array != null && array.Length != 0)
			{
				borderStyleDictionary.SetProperty("D", new PdfArray(array));
			}
		}
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

	private string getJsonObject(string token, string key, PdfReader reader)
	{
		string text = string.Empty;
		string empty = string.Empty;
		if (key == "rect")
		{
			while (token != "}")
			{
				while (token != ":")
				{
					token = reader.GetNextJsonToken();
				}
				token = reader.GetNextJsonToken();
				token = reader.GetNextJsonToken();
				text += token;
				token = reader.GetNextJsonToken();
				token = reader.GetNextJsonToken();
				if (token == ",")
				{
					text += token;
					token = reader.GetNextJsonToken();
				}
			}
			return text;
		}
		token = reader.GetNextJsonToken();
		while (token != "}")
		{
			if (token != "\"")
			{
				if (token == ",")
				{
					token = ";";
				}
				text += token;
			}
			token = reader.GetNextJsonToken();
		}
		return text;
	}

	private void AddMeasureDictionary(PdfDictionary annotDictionary, Dictionary<string, string> element)
	{
		Dictionary<string, string> dictionary = null;
		Dictionary<string, string> dictionary2 = null;
		Dictionary<string, string> dictionary3 = null;
		Dictionary<string, string> dictionary4 = null;
		Dictionary<string, string> dictionary5 = null;
		PdfDictionary pdfDictionary = new PdfDictionary();
		PdfArray pdfArray = new PdfArray();
		PdfArray pdfArray2 = new PdfArray();
		PdfArray pdfArray3 = new PdfArray();
		PdfArray pdfArray4 = new PdfArray();
		PdfArray pdfArray5 = new PdfArray();
		PdfDictionary pdfDictionary2 = new PdfDictionary();
		PdfDictionary pdfDictionary3 = new PdfDictionary();
		PdfDictionary pdfDictionary4 = new PdfDictionary();
		PdfDictionary pdfDictionary5 = new PdfDictionary();
		PdfDictionary pdfDictionary6 = new PdfDictionary();
		pdfDictionary.Items.Add(new PdfName("A"), pdfArray2);
		pdfDictionary.Items.Add(new PdfName("D"), pdfArray);
		pdfDictionary.Items.Add(new PdfName("X"), pdfArray3);
		pdfDictionary.Items.Add(new PdfName("T"), pdfArray4);
		pdfDictionary.Items.Add(new PdfName("V"), pdfArray5);
		if (element.ContainsKey("type1"))
		{
			pdfDictionary.SetName("Type", "Measure");
		}
		foreach (KeyValuePair<string, string> item in element)
		{
			_ = item.Value;
			switch (item.Key.ToLower())
			{
			case "ratevalue":
				pdfDictionary.SetString("R", item.Value);
				break;
			case "subtype":
				pdfDictionary.SetString("Subtype", item.Value);
				break;
			case "targetunitconversion":
				pdfDictionary.SetString("TargetUnitConversion", item.Value);
				break;
			case "area":
			{
				string value5 = item.Value;
				dictionary = new Dictionary<string, string>();
				dictionary = AddDictionaryData(dictionary, value5);
				break;
			}
			case "distance":
			{
				string value4 = item.Value;
				dictionary2 = new Dictionary<string, string>();
				dictionary2 = AddDictionaryData(dictionary2, value4);
				break;
			}
			case "xformat":
			{
				string value3 = item.Value;
				dictionary3 = new Dictionary<string, string>();
				dictionary3 = AddDictionaryData(dictionary3, value3);
				break;
			}
			case "tformat":
			{
				string value2 = item.Value;
				dictionary4 = new Dictionary<string, string>();
				dictionary4 = AddDictionaryData(dictionary4, value2);
				break;
			}
			case "vformat":
			{
				string value = item.Value;
				dictionary5 = new Dictionary<string, string>();
				dictionary5 = AddDictionaryData(dictionary5, value);
				break;
			}
			}
		}
		if (dictionary3 != null)
		{
			AddElements(dictionary3, pdfDictionary4);
			pdfArray3.Add(pdfDictionary4);
		}
		if (dictionary != null)
		{
			AddElements(dictionary, pdfDictionary3);
			pdfArray2.Add(pdfDictionary3);
		}
		if (dictionary2 != null)
		{
			AddElements(dictionary2, pdfDictionary2);
			pdfArray.Add(pdfDictionary2);
		}
		if (dictionary5 != null)
		{
			AddElements(dictionary5, pdfDictionary6);
			pdfArray5.Add(pdfDictionary6);
		}
		if (dictionary4 != null)
		{
			AddElements(dictionary4, pdfDictionary5);
			pdfArray4.Add(pdfDictionary5);
		}
		if (pdfDictionary.Items.Count > 0 && pdfDictionary.ContainsKey("Type"))
		{
			annotDictionary.Items.Add(new PdfName("Measure"), new PdfReferenceHolder(pdfDictionary));
		}
	}

	private void AddElements(Dictionary<string, string> element, PdfDictionary dictionary)
	{
		foreach (KeyValuePair<string, string> item in element)
		{
			string value = item.Value;
			switch (item.Key.ToLower())
			{
			case "d":
			{
				if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
				{
					dictionary.Items.Add(new PdfName("D"), new PdfNumber(result));
				}
				break;
			}
			case "c":
			{
				if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result2))
				{
					dictionary.Items.Add(new PdfName("C"), new PdfNumber(result2));
				}
				break;
			}
			case "rt":
				if (value != null)
				{
					dictionary.Items.Add(new PdfName("RT"), new PdfString(value));
				}
				break;
			case "rd":
				if (value != null)
				{
					dictionary.Items.Add(new PdfName("RD"), new PdfString(value));
				}
				break;
			case "ss":
				if (value != null)
				{
					dictionary.Items.Add(new PdfName("SS"), new PdfString(value));
				}
				break;
			case "u":
				if (value != null)
				{
					dictionary.Items.Add(new PdfName("U"), new PdfString(value));
				}
				break;
			case "f":
				if (value != null)
				{
					dictionary.Items.Add(new PdfName("F"), new PdfName(value));
				}
				break;
			case "fd":
				if (value != null)
				{
					dictionary.Items.Add(new PdfName("FD"), new PdfString(value));
				}
				break;
			case "type":
				if (value != null)
				{
					dictionary.Items.Add(new PdfName("Type"), new PdfName(value));
				}
				break;
			}
		}
	}

	public Dictionary<string, string> AddDictionaryData(Dictionary<string, string> data, string value)
	{
		string text = "";
		for (int i = 0; i < value.Length; i++)
		{
			text = ((value[i] != ':' && value[i] != ';') ? (text + value[i]) : (text + "#"));
		}
		string[] array = text.Split('#');
		for (int j = 0; j < array.Length - 1; j += 2)
		{
			data.Add(array[j], array[j + 1]);
		}
		return data;
	}

	private void AddAppearanceData(PdfDictionary dictionary, string value)
	{
		if (!string.IsNullOrEmpty(value))
		{
			byte[] array = Convert.FromBase64String(value);
			if (array != null && array.Length != 0)
			{
				string @string = Encoding.UTF8.GetString(array, 0, array.Length);
				List<string> list = ConvertToDictionaryItems(@string);
				Dictionary<string, object> dictionary2 = new Dictionary<string, object>();
				for (int i = 0; i < list.Count; i += 2)
				{
					if (list[i].Length > 2)
					{
						string key = list[i].Substring(1, list[i].Length - 2);
						object value2 = SplitJSONAppearance(list[i + 1]);
						dictionary2[key] = value2;
					}
				}
				PdfDictionary pdfPrimitive = new PdfDictionary();
				foreach (KeyValuePair<string, object> item in dictionary2)
				{
					dictionary["AP"] = new PdfReferenceHolder(ParseDictionaryItems(item.Value, pdfPrimitive));
				}
			}
		}
		m_isNormalAppearanceAdded = false;
	}

	internal IPdfPrimitive GetAppearanceStreamFromJson(string value)
	{
		if (!string.IsNullOrEmpty(value) && SplitJSONAppearance(value) is Dictionary<string, object> dict)
		{
			return GetAppearanceData(dict);
		}
		return null;
	}

	private IPdfPrimitive GetAppearanceData(Dictionary<string, object> dict)
	{
		foreach (KeyValuePair<string, object> item in dict)
		{
			if (item.Key == "N")
			{
				return ParseDictionaryItems(item.Value, new PdfStream());
			}
			if (item.Value is Dictionary<string, object> dict2)
			{
				return GetAppearanceData(dict2);
			}
		}
		return null;
	}

	private object SplitJSONAppearance(string value)
	{
		if (value.Length == 0)
		{
			return null;
		}
		if (value[0] == '{' && value[value.Length - 1] == '}')
		{
			return SplitDictionaryItems(value);
		}
		if (value[0] == '[' && value[value.Length - 1] == ']')
		{
			return SplitArrayItems(value);
		}
		if (value[0] == '"' && value[value.Length - 1] == '"')
		{
			return SplitSubStringJson(value);
		}
		if (char.IsDigit(value[0]) || value[0] == '-')
		{
			if (value.Contains("."))
			{
				GetFloatFromJson(value);
			}
			else
			{
				GetIntFromJson(value);
			}
		}
		if (value == "true")
		{
			return true;
		}
		if (value == "false")
		{
			return false;
		}
		return null;
	}

	private Dictionary<string, object> SplitDictionaryItems(string json)
	{
		List<string> list = ConvertToDictionaryItems(json);
		if (list.Count % 2 != 0)
		{
			return null;
		}
		Dictionary<string, object> dictionary = new Dictionary<string, object>(list.Count / 2);
		for (int i = 0; i < list.Count; i += 2)
		{
			dictionary[list[i].Substring(1, list[i].Length - 2)] = SplitJSONAppearance(list[i + 1]);
		}
		return dictionary;
	}

	private List<object> SplitArrayItems(string json)
	{
		List<string> list = ConvertToDictionaryItems(json);
		List<object> list2 = new List<object>(list.Count);
		for (int i = 0; i < list.Count; i++)
		{
			list2.Add(SplitJSONAppearance(list[i]));
		}
		return list2;
	}

	private string SplitSubStringJson(string json)
	{
		return json.Substring(1, json.Length - 2).Replace("\\", string.Empty);
	}

	private int GetIntFromJson(string json)
	{
		int.TryParse(json, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result);
		return result;
	}

	private double GetFloatFromJson(string value)
	{
		double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result);
		return result;
	}

	private List<string> ConvertToDictionaryItems(string value)
	{
		List<string> list = new List<string>();
		if (value.Length == 2)
		{
			return list;
		}
		int num = 0;
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 1; i < value.Length - 1; i++)
		{
			switch (value[i])
			{
			case '[':
			case '{':
				num++;
				break;
			case ']':
			case '}':
				num--;
				break;
			case '"':
				i = AddChunkValues(escapeString: true, i, value, stringBuilder);
				continue;
			case ',':
			case ':':
				if (num == 0)
				{
					list.Add(stringBuilder.ToString());
					stringBuilder.Length = 0;
					continue;
				}
				break;
			}
			stringBuilder.Append(value[i]);
		}
		list.Add(stringBuilder.ToString());
		return list;
	}

	private int AddChunkValues(bool escapeString, int index, string value, StringBuilder chunkBuilder)
	{
		chunkBuilder.Append(value[index]);
		for (int i = index + 1; i < value.Length; i++)
		{
			if (value[i] == '\\')
			{
				if (escapeString)
				{
					chunkBuilder.Append(value[i]);
				}
				chunkBuilder.Append(value[i + 1]);
				i++;
			}
			else
			{
				if (value[i] == '"')
				{
					chunkBuilder.Append(value[i]);
					return i;
				}
				chunkBuilder.Append(value[i]);
			}
		}
		return value.Length - 1;
	}

	private IPdfPrimitive ParseDictionaryItems(object elementValue, IPdfPrimitive pdfPrimitive)
	{
		PdfDictionary pdfDictionary = pdfPrimitive as PdfDictionary;
		PdfArray pdfArray = pdfPrimitive as PdfArray;
		if (elementValue != null)
		{
			if (elementValue is Dictionary<string, object>)
			{
				foreach (KeyValuePair<string, object> item in elementValue as Dictionary<string, object>)
				{
					string key = item.Key;
					key = XmlConvert.DecodeName(key);
					if (key != null)
					{
						switch (key.Length)
						{
						case 6:
						{
							switch (key[0])
							{
							case 's':
								break;
							case 'M':
								goto IL_0455;
							case 'L':
								goto IL_046b;
							case 'A':
								goto IL_0481;
							case 'F':
								goto IL_0497;
							case 'H':
								goto IL_04ad;
							case 'D':
								goto IL_04c3;
							case 'C':
								goto IL_04ea;
							case 'E':
								goto IL_0500;
							case 'B':
								goto IL_0527;
							case 'W':
								goto IL_053d;
							default:
								goto end_IL_0061;
							}
							if (!(key == "stream"))
							{
								if (!(key == "string"))
								{
									break;
								}
								if (item.Value != null && item.Value is Dictionary<string, object>)
								{
									byte[] streamData = GetStreamData(item.Value as Dictionary<string, object>);
									if (streamData != null)
									{
										return new PdfString(streamData);
									}
								}
								return new PdfString(item.Value as string);
							}
							PdfStream pdfPrimitive4 = new PdfStream();
							return ParseDictionaryItems(item.Value, pdfPrimitive4) as PdfStream;
						}
						case 5:
						{
							char c = key[4];
							if ((uint)c <= 100u)
							{
								if (c != 'H')
								{
									if (c != 'V')
									{
										if (c != 'd' || !(key == "fixed"))
										{
											break;
										}
										float.TryParse(item.Value as string, NumberStyles.Float, CultureInfo.InvariantCulture, out var result2);
										return new PdfNumber(result2);
									}
									if (!(key == "StemV"))
									{
										break;
									}
								}
								else if (!(key == "StemH"))
								{
									break;
								}
							}
							else if (c != 'h')
							{
								if (c != 's')
								{
									if (c != 'y' || !(key == "array"))
									{
										break;
									}
									PdfArray pdfPrimitive3 = new PdfArray();
									return ParseDictionaryItems(item.Value, pdfPrimitive3) as PdfArray;
								}
								if (!(key == "Flags"))
								{
									break;
								}
							}
							else if (!(key == "Width"))
							{
								break;
							}
							goto IL_0cc2;
						}
						case 4:
						{
							char c = key[0];
							if ((uint)c <= 78u)
							{
								if (c != 'B')
								{
									if (c != 'F')
									{
										if (c != 'N' || !(key == "Name"))
										{
											break;
										}
									}
									else if (!(key == "Font"))
									{
										break;
									}
								}
								else if (!(key == "BBox"))
								{
									break;
								}
							}
							else
							{
								if ((uint)c > 84u)
								{
									switch (c)
									{
									case 'n':
										if (!(key == "name"))
										{
											break;
										}
										return new PdfName(item.Value as string);
									case 'd':
									{
										if (!(key == "dict"))
										{
											if (!(key == "data"))
											{
												break;
											}
											if (!(pdfDictionary is PdfStream pdfStream) || item.Value == null)
											{
												continue;
											}
											pdfStream.Data = GetStreamData(item.Value as Dictionary<string, object>);
											if (pdfStream == null)
											{
												continue;
											}
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
												continue;
											}
											if (pdfStream.ContainsKey("Length"))
											{
												pdfStream.Remove("Length");
											}
											if (pdfStream.ContainsKey("Filter"))
											{
												pdfStream.Remove("Filter");
											}
											continue;
										}
										PdfDictionary pdfPrimitive2 = new PdfDictionary();
										return ParseDictionaryItems(item.Value, pdfPrimitive2) as PdfDictionary;
									}
									}
									break;
								}
								if (c != 'R')
								{
									if (c != 'T' || !(key == "Type"))
									{
										break;
									}
								}
								else if (!(key == "Rows"))
								{
									break;
								}
							}
							goto IL_0cc2;
						}
						case 7:
						{
							char c = key[0];
							if ((uint)c <= 76u)
							{
								if (c != 'C')
								{
									if (c != 'D')
									{
										if (c != 'L' || !(key == "Leading"))
										{
											break;
										}
									}
									else if (!(key == "Descent"))
									{
										break;
									}
								}
								else if (!(key == "Columns"))
								{
									break;
								}
							}
							else if ((uint)c <= 83u)
							{
								if (c != 'P')
								{
									if (c != 'S' || !(key == "Subtype"))
									{
										break;
									}
								}
								else if (!(key == "ProcSet") && !(key == "Pattern"))
								{
									break;
								}
							}
							else
							{
								if (c != 'X')
								{
									if (c != 'b' || !(key == "boolean"))
									{
										break;
									}
									bool.TryParse(item.Value as string, out var result);
									return new PdfBoolean(result);
								}
								if (!(key == "XObject"))
								{
									break;
								}
							}
							goto IL_0cc2;
						}
						case 3:
						{
							char c = key[0];
							if (c != 'A')
							{
								if (c != 'i' || !(key == "int"))
								{
									break;
								}
								int.TryParse(item.Value as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result3);
								return new PdfNumber(result3);
							}
							if (!(key == "AIS"))
							{
								break;
							}
							goto IL_0cc2;
						}
						case 1:
						{
							char c = key[0];
							if ((uint)c <= 75u)
							{
								if (c != 'I' && c != 'K')
								{
									break;
								}
							}
							else
							{
								if (c == 'N')
								{
									if (m_isNormalAppearanceAdded)
									{
										pdfDictionary[key] = ParseDictionaryItems(item.Value, pdfDictionary);
										continue;
									}
									m_isNormalAppearanceAdded = true;
									PdfDictionary pdfDictionary2 = new PdfDictionary();
									pdfDictionary2[key] = new PdfReferenceHolder(ParseDictionaryItems(item.Value, pdfDictionary2));
									return pdfDictionary2;
								}
								if (c != 'S' && c != 'W')
								{
									break;
								}
							}
							goto IL_0cc2;
						}
						case 9:
							switch (key[0])
							{
							case 'R':
								break;
							case 'C':
								goto IL_076f;
							case 'I':
								goto IL_0785;
							case 'E':
								goto IL_079b;
							case 'F':
								goto IL_07b1;
							default:
								goto end_IL_0061;
							}
							if (!(key == "Resources"))
							{
								break;
							}
							goto IL_0cc2;
						case 8:
							switch (key[4])
							{
							case 'F':
								break;
							case 'd':
								goto IL_07dd;
							case 'N':
								goto IL_07f3;
							case 'B':
								goto IL_0809;
							case 'i':
								goto IL_081f;
							case 's':
								goto IL_0846;
							case 'r':
								goto IL_085c;
							case 'T':
								goto IL_0872;
							case 'k':
								goto IL_0888;
							case 'C':
								goto IL_089e;
							default:
								goto end_IL_0061;
							}
							if (!(key == "BaseFont"))
							{
								break;
							}
							goto IL_0cc2;
						case 11:
						{
							char c = key[1];
							if ((uint)c <= 101u)
							{
								if (c != 'I')
								{
									if (c != 'a')
									{
										if (c != 'e' || !(key == "DecodeParms"))
										{
											break;
										}
									}
									else if (!(key == "PatternType"))
									{
										break;
									}
								}
								else if (!(key == "CIDToGIDMap"))
								{
									break;
								}
							}
							else if (c != 'h')
							{
								if (c != 'n')
								{
									if (c != 't' || !(key == "ItalicAngle"))
									{
										break;
									}
								}
								else if (!(key == "Interpolate"))
								{
									break;
								}
							}
							else if (!(key == "ShadingType"))
							{
								break;
							}
							goto IL_0cc2;
						}
						case 2:
						{
							char c = key[1];
							if ((uint)c <= 77u)
							{
								if (c != 'A')
								{
									if (c != 'M' || !(key == "BM"))
									{
										break;
									}
								}
								else if (!(key == "CA"))
								{
									break;
								}
							}
							else if (c != 'S')
							{
								if (c != 'W')
								{
									if (c != 'a' || !(key == "ca"))
									{
										break;
									}
								}
								else if (!(key == "DW"))
								{
									break;
								}
							}
							else if (!(key == "CS"))
							{
								break;
							}
							goto IL_0cc2;
						}
						case 12:
						{
							char c = key[0];
							if (c != 'F')
							{
								if (c != 'M' || !(key == "MissingWidth"))
								{
									break;
								}
							}
							else if (!(key == "FunctionType"))
							{
								break;
							}
							goto IL_0cc2;
						}
						case 10:
						{
							char c = key[0];
							if (c != 'C')
							{
								if (c != 'S' || !(key == "Supplement"))
								{
									break;
								}
							}
							else if (!(key == "ColorSpace"))
							{
								break;
							}
							goto IL_0cc2;
						}
						case 13:
							if (!(key == "CIDSystemInfo"))
							{
								break;
							}
							goto IL_0cc2;
						case 16:
							{
								if (!(key == "BitsPerComponent"))
								{
									break;
								}
								goto IL_0cc2;
							}
							IL_085c:
							if (!(key == "Ordering"))
							{
								break;
							}
							goto IL_0cc2;
							IL_081f:
							if (!(key == "AvgWidth") && !(key == "MaxWidth"))
							{
								break;
							}
							goto IL_0cc2;
							IL_0846:
							if (!(key == "Registry"))
							{
								break;
							}
							goto IL_0cc2;
							IL_04c3:
							if (!(key == "Decode") && !(key == "Domain"))
							{
								break;
							}
							goto IL_0cc2;
							IL_07f3:
							if (!(key == "FontName"))
							{
								break;
							}
							goto IL_0cc2;
							IL_0809:
							if (!(key == "FontBBox"))
							{
								break;
							}
							goto IL_0cc2;
							IL_07dd:
							if (!(key == "Encoding"))
							{
								break;
							}
							goto IL_0cc2;
							IL_079b:
							if (!(key == "ExtGState"))
							{
								break;
							}
							goto IL_0cc2;
							IL_07b1:
							if (!(key == "FirstChar"))
							{
								break;
							}
							goto IL_0cc2;
							IL_0785:
							if (!(key == "ImageMask"))
							{
								break;
							}
							goto IL_0cc2;
							IL_0481:
							if (!(key == "Ascent"))
							{
								break;
							}
							goto IL_0cc2;
							IL_0497:
							if (!(key == "Filter"))
							{
								break;
							}
							goto IL_0cc2;
							IL_076f:
							if (!(key == "CapHeight"))
							{
								break;
							}
							goto IL_0cc2;
							IL_0527:
							if (!(key == "Bounds"))
							{
								break;
							}
							goto IL_0cc2;
							IL_0455:
							if (!(key == "Matrix"))
							{
								break;
							}
							goto IL_0cc2;
							IL_0cc2:
							pdfDictionary[key] = ParseDictionaryItems(item.Value, pdfDictionary);
							continue;
							IL_053d:
							if (!(key == "Widths"))
							{
								break;
							}
							goto IL_0cc2;
							IL_04ea:
							if (!(key == "Coords"))
							{
								break;
							}
							goto IL_0cc2;
							IL_0500:
							if (!(key == "Extend") && !(key == "Encode"))
							{
								break;
							}
							goto IL_0cc2;
							IL_04ad:
							if (!(key == "Height"))
							{
								break;
							}
							goto IL_0cc2;
							IL_046b:
							if (!(key == "Length"))
							{
								break;
							}
							goto IL_0cc2;
							IL_089e:
							if (!(key == "LastChar"))
							{
								break;
							}
							goto IL_0cc2;
							IL_0888:
							if (!(key == "BlackIs1"))
							{
								break;
							}
							goto IL_0cc2;
							IL_0872:
							if (!(key == "FormType"))
							{
								break;
							}
							goto IL_0cc2;
							end_IL_0061:
							break;
						}
					}
					if (pdfDictionary != null)
					{
						PdfDictionary pdfPrimitive5 = new PdfDictionary();
						pdfDictionary[key] = new PdfReferenceHolder(ParseDictionaryItems(item.Value, pdfPrimitive5));
					}
				}
			}
			else if (elementValue is List<object>)
			{
				List<object> list = elementValue as List<object>;
				for (int i = 0; i < list.Count; i++)
				{
					object obj = list[i];
					if (!(obj is Dictionary<string, object>))
					{
						continue;
					}
					foreach (KeyValuePair<string, object> item2 in obj as Dictionary<string, object>)
					{
						string key2 = item2.Key;
						switch (XmlConvert.DecodeName(key2))
						{
						case "int":
						{
							int.TryParse(item2.Value as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result6);
							pdfArray.Add(new PdfNumber(result6));
							break;
						}
						case "fixed":
						{
							float.TryParse(item2.Value as string, NumberStyles.Float, CultureInfo.InvariantCulture, out var result5);
							pdfArray.Add(new PdfNumber(result5));
							break;
						}
						case "name":
							pdfArray.Add(new PdfName(item2.Value as string));
							break;
						case "string":
							if (item2.Value != null && item2.Value is Dictionary<string, object>)
							{
								byte[] streamData2 = GetStreamData(item2.Value as Dictionary<string, object>);
								if (streamData2 != null)
								{
									pdfArray.Add(new PdfString(streamData2));
								}
							}
							else
							{
								pdfArray.Add(new PdfString(item2.Value as string));
							}
							break;
						case "dict":
						{
							PdfDictionary pdfPrimitive8 = new PdfDictionary();
							pdfPrimitive8 = ParseDictionaryItems(item2.Value, pdfPrimitive8) as PdfDictionary;
							pdfArray.Add(new PdfReferenceHolder(pdfPrimitive8));
							break;
						}
						case "array":
						{
							PdfArray pdfPrimitive7 = new PdfArray();
							pdfPrimitive7 = ParseDictionaryItems(item2.Value, pdfPrimitive7) as PdfArray;
							pdfArray.Add(pdfPrimitive7);
							break;
						}
						case "boolean":
						{
							bool.TryParse(item2.Value as string, out var result4);
							pdfArray.Add(new PdfBoolean(result4));
							break;
						}
						case "stream":
						{
							PdfStream pdfPrimitive6 = new PdfStream();
							pdfPrimitive6 = ParseDictionaryItems(item2.Value, pdfPrimitive6) as PdfStream;
							pdfArray.Add(pdfPrimitive6);
							break;
						}
						}
					}
				}
			}
		}
		return pdfPrimitive;
	}

	private byte[] GetStreamData(Dictionary<string, object> element)
	{
		byte[] result = null;
		string text = string.Empty;
		_ = string.Empty;
		foreach (KeyValuePair<string, object> item in element)
		{
			switch (item.Key)
			{
			case "mode":
				_ = item.Value;
				break;
			case "encoding":
				text = item.Value as string;
				break;
			case "bytes":
				if (text == "hex")
				{
					result = GetBytes(item.Value as string);
				}
				else if (text == "ascii" && item.Value != null)
				{
					result = Encoding.GetEncoding("UTF-8").GetBytes(item.Value as string);
				}
				return result;
			}
		}
		return result;
	}
}
