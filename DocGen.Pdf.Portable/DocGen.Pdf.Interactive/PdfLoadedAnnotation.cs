using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using DocGen.Pdf.IO;
using DocGen.Pdf.Parsing;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public abstract class PdfLoadedAnnotation : PdfAnnotation
{
	internal delegate void BeforeNameChangesEventHandler(string name);

	public int ObjectID;

	private PdfCrossTable m_crossTable;

	private bool m_Changed;

	private int m_defaultIndex;

	private string m_fileName;

	internal PdfLoadedPage m_loadedpage;

	private string m_annotationID;

	private PdfAnnotation m_loadedPopup;

	internal PdfLoadedAnnotationType m_type = PdfLoadedAnnotationType.Null;

	private bool m_isCreationDateObtained;

	private DateTime m_creationDate;

	public DateTime CreationDate
	{
		get
		{
			if (!m_isCreationDateObtained)
			{
				return ObtainCreationDate();
			}
			return m_creationDate;
		}
	}

	public PdfLoadedAnnotationType Type
	{
		get
		{
			return m_type;
		}
		internal set
		{
			m_type = value;
			NotifyPropertyChanged("Type");
		}
	}

	internal bool Changed
	{
		get
		{
			return m_Changed;
		}
		set
		{
			m_Changed = value;
		}
	}

	internal PdfCrossTable CrossTable
	{
		get
		{
			return m_crossTable;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("CrossTable");
			}
			if (m_crossTable != value)
			{
				m_crossTable = value;
			}
		}
	}

	internal new virtual PdfAnnotation Popup
	{
		get
		{
			return m_loadedPopup;
		}
		set
		{
			if (value != null && ValidPopup(value.Dictionary, isSupportedPopup: true))
			{
				if ((Page != null && value is PdfPopupAnnotation) || value is PdfLoadedPopupAnnotation)
				{
					if (m_loadedPopup != null)
					{
						RemoveAnnoationFromPage(Page, m_loadedPopup);
					}
					m_loadedPopup = value;
					bool changed = base.Dictionary.Changed;
					m_loadedPopup.Dictionary.SetProperty("Parent", new PdfReferenceHolder(this));
					base.Dictionary.SetProperty("Popup", new PdfReferenceHolder(m_loadedPopup));
					if (!changed && value is PdfLoadedPopupAnnotation)
					{
						m_loadedPopup.Dictionary.FreezeChanges(m_loadedPopup.Dictionary);
						base.Dictionary.FreezeChanges(base.Dictionary);
					}
				}
				else
				{
					m_loadedPopup = value;
				}
			}
			else if (value == null && m_loadedPopup != null && Page != null)
			{
				RemoveAnnoationFromPage(Page, m_loadedPopup);
				m_loadedPopup = value;
			}
		}
	}

	public new PdfLoadedPage Page
	{
		get
		{
			return m_loadedpage;
		}
		set
		{
			m_loadedpage = value;
			NotifyPropertyChanged("Page");
		}
	}

	internal bool IsPopup => ValidPopup(base.Dictionary, isSupportedPopup: false);

	internal event BeforeNameChangesEventHandler BeforeNameChanges;

	internal PdfLoadedAnnotation(PdfDictionary dictionary, PdfCrossTable crossTable)
	{
		if (dictionary == null)
		{
			throw new ArgumentNullException("dictionary");
		}
		if (crossTable == null)
		{
			throw new ArgumentNullException("crossTable");
		}
		base.Dictionary = dictionary;
		m_crossTable = crossTable;
	}

	public void SetText(string text)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		if (text == string.Empty)
		{
			throw new ArgumentException("The text can't be empty");
		}
		if (Text != text)
		{
			new PdfString(text);
			base.Dictionary.SetString("T", text);
			Changed = true;
		}
	}

	public List<string> GetValues(string name)
	{
		List<string> list = new List<string>();
		PdfName key = new PdfName(name);
		if (base.Dictionary.ContainsKey(key))
		{
			PdfArray pdfArray = PdfCrossTable.Dereference(base.Dictionary[key]) as PdfArray;
			PdfString pdfString = PdfCrossTable.Dereference(base.Dictionary[key]) as PdfString;
			PdfName pdfName = PdfCrossTable.Dereference(base.Dictionary[key]) as PdfName;
			if (pdfArray != null)
			{
				for (int i = 0; i < pdfArray.Count; i++)
				{
					if (pdfArray[i] is PdfString)
					{
						PdfString pdfString2 = pdfArray[i] as PdfString;
						list.Add(pdfString2.Value);
					}
					else if (pdfArray[i] is PdfNumber)
					{
						PdfNumber pdfNumber = pdfArray[i] as PdfNumber;
						list.Add(pdfNumber.FloatValue.ToString());
					}
					else if (pdfArray[i] is PdfName)
					{
						PdfName pdfName2 = pdfArray[i] as PdfName;
						list.Add(pdfName2.Value);
					}
				}
			}
			else if (pdfString != null)
			{
				list.Add(pdfString.Value);
			}
			else
			{
				if (!(pdfName != null))
				{
					throw new PdfException(name + " key is not found");
				}
				list.Add(pdfName.Value);
			}
			return list;
		}
		throw new PdfException(name + " key is not found");
	}

	public new void SetValues(string key, string value)
	{
		if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
		{
			base.Dictionary.SetProperty(key, new PdfString(value));
		}
	}

	internal static IPdfPrimitive SearchInParents(PdfDictionary dictionary, PdfCrossTable crossTable, string value)
	{
		IPdfPrimitive pdfPrimitive = null;
		PdfDictionary pdfDictionary = dictionary;
		while (pdfPrimitive == null && pdfDictionary != null)
		{
			if (pdfDictionary.ContainsKey(value))
			{
				pdfPrimitive = crossTable.GetObject(pdfDictionary[value]);
			}
			else
			{
				pdfDictionary = ((!pdfDictionary.ContainsKey("Parent")) ? null : (crossTable.GetObject(pdfDictionary["Parent"]) as PdfDictionary));
			}
		}
		return pdfPrimitive;
	}

	internal static IPdfPrimitive GetValue(PdfDictionary dictionary, PdfCrossTable crossTable, string value, bool inheritable)
	{
		IPdfPrimitive result = null;
		if (dictionary.ContainsKey(value))
		{
			result = crossTable.GetObject(dictionary[value]);
		}
		else if (inheritable)
		{
			result = SearchInParents(dictionary, crossTable, value);
		}
		return result;
	}

	internal PdfDictionary GetWidgetAnnotation(PdfDictionary dictionary, PdfCrossTable crossTable)
	{
		PdfDictionary result = null;
		if (dictionary.ContainsKey("Kids"))
		{
			PdfArray pdfArray = crossTable.GetObject(dictionary["Kids"]) as PdfArray;
			PdfReference reference = crossTable.GetReference(pdfArray[m_defaultIndex]);
			result = crossTable.GetObject(reference) as PdfDictionary;
		}
		if (dictionary.ContainsKey("Subtype") && (CrossTable.GetObject(dictionary["Subtype"]) as PdfName).Value == "Widget")
		{
			result = dictionary;
		}
		return result;
	}

	private DateTime ObtainCreationDate()
	{
		m_isCreationDateObtained = true;
		if (base.Dictionary.ContainsKey("CreationDate") && PdfCrossTable.Dereference(base.Dictionary["CreationDate"]) is PdfString dateTimeStringValue)
		{
			m_creationDate = base.Dictionary.GetDateTime(dateTimeStringValue);
		}
		return m_creationDate;
	}

	internal override void ApplyText(string text)
	{
		SetText(text);
	}

	internal virtual void BeginSave()
	{
	}

	internal void ExportText(Stream stream, ref int objectid)
	{
		bool flag = false;
		PdfArray pdfArray = null;
		if (base.Dictionary.ContainsKey("Kids"))
		{
			pdfArray = CrossTable.GetObject(base.Dictionary["Kids"]) as PdfArray;
			if (pdfArray != null)
			{
				for (int i = 0; i < pdfArray.Count; i++)
				{
					flag = flag || pdfArray[i] is PdfLoadedAnnotation;
				}
			}
		}
		PdfString pdfString = GetValue(base.Dictionary, CrossTable, "Contents", inheritable: true) as PdfString;
		string text = "";
		if (pdfString != null)
		{
			text = pdfString.Value;
		}
		if (!(!validateString(text) || flag))
		{
			return;
		}
		if (flag)
		{
			for (int j = 0; j < pdfArray.Count; j++)
			{
				if (pdfArray[j] is PdfLoadedAnnotation pdfLoadedAnnotation)
				{
					pdfLoadedAnnotation.ExportText(stream, ref objectid);
				}
			}
			ObjectID = objectid;
			objectid++;
			StringBuilder stringBuilder = new StringBuilder();
			PdfString pdfString2 = new PdfString(text);
			pdfString2.Encode = PdfString.ForceEncoding.ASCII;
			byte[] bytes = Encoding.GetEncoding("UTF-8").GetBytes(pdfString2.Value);
			stringBuilder.AppendFormat("{0} 0 obj<</T <{1}> /Kids [", ObjectID, PdfString.BytesToHex(bytes));
			for (int k = 0; k < pdfArray.Count; k++)
			{
				if (pdfArray[k] is PdfLoadedAnnotation { ObjectID: not 0 } pdfLoadedAnnotation2)
				{
					stringBuilder.AppendFormat("{0} 0 R ", pdfLoadedAnnotation2.ObjectID);
				}
			}
			stringBuilder.Append("]>>endobj\n");
			PdfString pdfString3 = new PdfString(stringBuilder.ToString());
			pdfString3.Encode = PdfString.ForceEncoding.ASCII;
			byte[] bytes2 = Encoding.GetEncoding("UTF-8").GetBytes(pdfString3.Value);
			stream.Write(bytes2, 0, bytes2.Length);
		}
		else
		{
			ObjectID = objectid;
			objectid++;
			if (GetType().Name == "PdfLoadedCheckBoxField" || GetType().Name == "PdfLoadedRadioButtonListField")
			{
				text = "/" + text;
			}
			else
			{
				PdfString pdfString4 = new PdfString(text);
				pdfString4.Encode = PdfString.ForceEncoding.ASCII;
				byte[] bytes3 = Encoding.GetEncoding("UTF-8").GetBytes(pdfString4.Value);
				text = "<" + PdfString.BytesToHex(bytes3) + ">";
			}
			StringBuilder stringBuilder2 = new StringBuilder();
			PdfString pdfString5 = new PdfString(Text)
			{
				Encode = PdfString.ForceEncoding.ASCII
			};
			stringBuilder2.AppendFormat(arg1: PdfString.BytesToHex(Encoding.GetEncoding("UTF-8").GetBytes(pdfString5.Value)), format: "{0} 0 obj<</T <{1}> /Contents {2} >>endobj\n", arg0: ObjectID, arg2: text);
			PdfString pdfString6 = new PdfString(stringBuilder2.ToString());
			pdfString6.Encode = PdfString.ForceEncoding.ASCII;
			byte[] bytes4 = Encoding.GetEncoding("UTF-8").GetBytes(pdfString6.Value);
			stream.Write(bytes4, 0, bytes4.Length);
		}
	}

	internal static bool validateString(string text1)
	{
		if (text1 != null)
		{
			return text1.Length == 0;
		}
		return true;
	}

	internal List<string> ExportAnnotation(ref PdfWriter writer, ref int currentID, List<string> annotID, int pageIndex, bool hasAppearance)
	{
		string text = " 0 obj\r\n";
		string text2 = "\r\nendobj\r\n";
		PdfDictionary dictionary = base.Dictionary;
		m_annotationID = currentID.ToString();
		writer.Write(currentID + text + "<<");
		Dictionary<int, IPdfPrimitive> dictionaries = new Dictionary<int, IPdfPrimitive>();
		List<int> streamReferences = new List<int>();
		annotID.Add(m_annotationID);
		dictionary.Items.Add(new PdfName("Page"), new PdfNumber(pageIndex));
		GetEntriesInDictionary(ref dictionaries, ref streamReferences, ref currentID, dictionary, writer, hasAppearance);
		dictionary.Remove("Page");
		writer.Write(">>" + text2);
		while (dictionaries.Count > 0)
		{
			foreach (int item in new List<int>(dictionaries.Keys))
			{
				if (dictionaries[item] is PdfDictionary)
				{
					if (dictionaries[item] is PdfDictionary pdfDictionary)
					{
						if (pdfDictionary.ContainsKey("Type"))
						{
							PdfName pdfName = pdfDictionary["Type"] as PdfName;
							if (pdfName != null && pdfName.Value == "Annot")
							{
								annotID.Add(item.ToString());
								pdfDictionary.Items.Add(new PdfName("Page"), new PdfNumber(pageIndex));
							}
						}
						writer.Write(item + text + "<<");
						GetEntriesInDictionary(ref dictionaries, ref streamReferences, ref currentID, pdfDictionary, writer, hasAppearance);
						if (pdfDictionary.ContainsKey("Page"))
						{
							pdfDictionary.Remove("Page");
						}
						writer.Write(">>");
						if (streamReferences.Contains(item))
						{
							AppendStream(dictionaries[item] as PdfStream, writer);
						}
						writer.Write(text2);
					}
				}
				else if (dictionaries[item] is PdfName)
				{
					PdfName pdfName2 = dictionaries[item] as PdfName;
					if (pdfName2 != null)
					{
						writer.Write(item + text + pdfName2.ToString() + text2);
					}
				}
				else if (dictionaries[item] is PdfArray)
				{
					if (dictionaries[item] is PdfArray array)
					{
						writer.Write(item + text);
						AppendArrayElements(array, writer, ref currentID, hasAppearance, ref dictionaries, ref streamReferences);
						writer.Write(text2);
					}
				}
				else if (dictionaries[item] is PdfBoolean)
				{
					if (dictionaries[item] is PdfBoolean pdfBoolean)
					{
						writer.Write(item + text + (pdfBoolean.Value ? "true" : "false") + text2);
					}
				}
				else if (dictionaries[item] is PdfString && dictionaries[item] is PdfString pdfString)
				{
					writer.Write(item + text + "(" + GetFormattedString(pdfString.Value) + ")" + text2);
				}
				dictionaries.Remove(item);
			}
		}
		currentID++;
		return annotID;
	}

	private void GetEntriesInDictionary(ref Dictionary<int, IPdfPrimitive> dictionaries, ref List<int> streamReferences, ref int currentID, PdfDictionary dictionary, PdfWriter writer, bool hasAppearance)
	{
		bool flag = false;
		foreach (PdfName key in dictionary.Keys)
		{
			if (!hasAppearance && key.Value == "AP")
			{
				continue;
			}
			if (key.Value != "P")
			{
				writer.Write(key.ToString());
			}
			if (key.Value == "Sound" || key.Value == "F" || hasAppearance)
			{
				flag = true;
			}
			IPdfPrimitive pdfPrimitive = dictionary[key];
			if (pdfPrimitive is PdfString)
			{
				writer.Write("(" + GetFormattedString((pdfPrimitive as PdfString).Value) + ")");
			}
			else if (pdfPrimitive is PdfName)
			{
				writer.Write((pdfPrimitive as PdfName).ToString());
			}
			else if (pdfPrimitive is PdfArray)
			{
				AppendArrayElements(pdfPrimitive as PdfArray, writer, ref currentID, flag, ref dictionaries, ref streamReferences);
			}
			else if (pdfPrimitive is PdfNumber)
			{
				writer.Write(" " + (pdfPrimitive as PdfNumber).FloatValue.ToString(CultureInfo.InvariantCulture));
			}
			else if (pdfPrimitive is PdfBoolean)
			{
				writer.Write(" " + ((pdfPrimitive as PdfBoolean).Value ? "true" : "false"));
			}
			else if (pdfPrimitive is PdfDictionary)
			{
				writer.Write("<<");
				GetEntriesInDictionary(ref dictionaries, ref streamReferences, ref currentID, pdfPrimitive as PdfDictionary, writer, hasAppearance);
				writer.Write(">>");
			}
			else if (pdfPrimitive is PdfReferenceHolder)
			{
				int num = (Page.Document as PdfLoadedDocument).Pages.IndexOf(Page);
				if (key.Value == "Parent")
				{
					writer.Write(" " + m_annotationID + " 0 R");
					writer.Write("/Page " + num);
				}
				else if (key.Value == "IRT")
				{
					PdfReferenceHolder pdfReferenceHolder = pdfPrimitive as PdfReferenceHolder;
					if (pdfReferenceHolder != null && pdfReferenceHolder.Object is PdfDictionary pdfDictionary && pdfDictionary.ContainsKey("NM") && pdfDictionary["NM"] is PdfString pdfString)
					{
						writer.Write("(" + GetFormattedString(pdfString.Value) + ")");
					}
				}
				else if (key.Value != "P")
				{
					PdfReferenceHolder pdfReferenceHolder2 = pdfPrimitive as PdfReferenceHolder;
					if (pdfReferenceHolder2 != null)
					{
						currentID++;
						writer.Write(" " + currentID + " 0 R");
						if (flag)
						{
							streamReferences.Add(currentID);
						}
						dictionaries.Add(currentID, pdfReferenceHolder2.Object);
					}
				}
			}
			flag = false;
		}
	}

	private void AppendStream(PdfStream stream, PdfWriter writer)
	{
		if (stream != null && stream.Data != null && stream.Data.Length != 0)
		{
			writer.Write("stream\r\n");
			writer.Write(stream.Data);
			writer.Write("\r\nendstream");
		}
	}

	private void AppendElement(IPdfPrimitive element, PdfWriter writer, ref int currentID, bool isStream, ref Dictionary<int, IPdfPrimitive> dictionaries, ref List<int> streamReferences)
	{
		if (element is PdfNumber)
		{
			writer.Write((element as PdfNumber).FloatValue);
		}
		else if (element is PdfName)
		{
			writer.Write((element as PdfName).ToString());
		}
		else if (element is PdfString)
		{
			writer.Write("(" + GetFormattedString((element as PdfString).Value) + ")");
		}
		else if (element is PdfBoolean)
		{
			writer.Write((element as PdfBoolean).Value ? "true" : "false");
		}
		else if (element is PdfReferenceHolder)
		{
			PdfReferenceHolder pdfReferenceHolder = element as PdfReferenceHolder;
			currentID++;
			writer.Write(currentID + " 0 R");
			if (isStream)
			{
				streamReferences.Add(currentID);
			}
			dictionaries.Add(currentID, pdfReferenceHolder.Object);
		}
		else if (element is PdfArray)
		{
			AppendArrayElements(element as PdfArray, writer, ref currentID, isStream, ref dictionaries, ref streamReferences);
		}
		else if (element is PdfDictionary)
		{
			writer.Write("<<");
			GetEntriesInDictionary(ref dictionaries, ref streamReferences, ref currentID, element as PdfDictionary, writer, isStream);
			writer.Write(">>");
		}
	}

	private void AppendArrayElements(PdfArray array, PdfWriter writer, ref int currentID, bool isStream, ref Dictionary<int, IPdfPrimitive> dictionaries, ref List<int> streamReferences)
	{
		writer.Write("[");
		if (array != null && array.Elements.Count > 0)
		{
			int count = array.Elements.Count;
			for (int i = 0; i < count; i++)
			{
				IPdfPrimitive pdfPrimitive = array.Elements[i];
				if (i != 0 && (pdfPrimitive is PdfNumber || pdfPrimitive is PdfReferenceHolder || pdfPrimitive is PdfBoolean))
				{
					writer.Write(" ");
				}
				AppendElement(pdfPrimitive, writer, ref currentID, isStream, ref dictionaries, ref streamReferences);
			}
		}
		writer.Write("]");
	}

	private string GetFormattedString(string value)
	{
		string text = "";
		foreach (int num in value)
		{
			if (num == 40 || num == 41)
			{
				text += "\\";
			}
			if (num == 13 || num == 10)
			{
				if (num == 13)
				{
					text += "\\r";
				}
				if (num == 10)
				{
					text += "\\n";
				}
			}
			else
			{
				text += (char)num;
			}
		}
		return text;
	}

	internal bool ValidPopup(PdfDictionary dictionary, bool isSupportedPopup)
	{
		if (dictionary != null && dictionary.ContainsKey("Subtype"))
		{
			PdfName pdfName = PdfCrossTable.Dereference(dictionary["Subtype"]) as PdfName;
			if (pdfName != null)
			{
				bool flag = pdfName.Value == "Popup";
				if (flag && isSupportedPopup && base.Dictionary != null && base.Dictionary.ContainsKey("Subtype"))
				{
					pdfName = PdfCrossTable.Dereference(base.Dictionary["Subtype"]) as PdfName;
					if (pdfName != null && (pdfName.Value == "FreeText" || pdfName.Value == "Sound" || pdfName.Value == "FileAttachment"))
					{
						return false;
					}
				}
				return flag;
			}
		}
		return false;
	}
}
