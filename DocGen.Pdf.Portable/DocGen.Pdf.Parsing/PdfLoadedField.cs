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

public abstract class PdfLoadedField : PdfField
{
	internal delegate void BeforeNameChangesEventHandler(string name);

	public int ObjectID;

	private PdfCrossTable m_crossTable;

	private bool m_Changed;

	private bool m_fieldChanged;

	internal int m_defaultIndex;

	private string m_name;

	private PdfPageBase m_page;

	private PdfLoadedForm m_form;

	internal bool ExportEmptyField;

	internal PdfReferenceHolder m_requiredReference;

	internal bool isAcrobat;

	private List<string> selectedList;

	public override string Name
	{
		get
		{
			m_name = GetFieldName();
			return m_name;
		}
	}

	public override string MappingName
	{
		get
		{
			string text = base.MappingName;
			if ((text == null || text == string.Empty) && GetValue(base.Dictionary, m_crossTable, "TM", inheritable: false) is PdfString pdfString)
			{
				text = pdfString.Value;
			}
			return text;
		}
		set
		{
			base.MappingName = value;
			NotifyPropertyChanged("MappingName");
			Changed = true;
		}
	}

	public override string ToolTip
	{
		get
		{
			PdfString pdfString = GetValue(base.Dictionary, m_crossTable, "TU", inheritable: false) as PdfString;
			string result = null;
			if (pdfString != null)
			{
				result = pdfString.Value;
			}
			return result;
		}
		set
		{
			base.ToolTip = value;
			Changed = true;
		}
	}

	public override PdfPageBase Page
	{
		get
		{
			if (m_page == null)
			{
				m_page = GetLoadedPage();
			}
			else if (m_page != null && m_page is PdfLoadedPage && (Changed || (Form != null && Form.Flatten) || base.Flatten))
			{
				m_page = GetLoadedPage();
			}
			return m_page;
		}
		internal set
		{
			m_page = value;
		}
	}

	public override bool ReadOnly
	{
		get
		{
			if ((FieldFlags.ReadOnly & Flags) == 0)
			{
				return Form.ReadOnly;
			}
			return true;
		}
		set
		{
			if (value || Form.ReadOnly)
			{
				Flags |= FieldFlags.ReadOnly;
			}
			else
			{
				if (Flags == FieldFlags.ReadOnly)
				{
					Flags |= FieldFlags.Default;
				}
				Flags &= ~FieldFlags.ReadOnly;
			}
			NotifyPropertyChanged("ReadOnly");
		}
	}

	public override bool Required
	{
		get
		{
			return (FieldFlags.Required & Flags) != 0;
		}
		set
		{
			if (value)
			{
				Flags |= FieldFlags.Required;
			}
			else
			{
				Flags &= ~FieldFlags.Required;
			}
			NotifyPropertyChanged("Required");
		}
	}

	public override bool Export
	{
		get
		{
			return (FieldFlags.NoExport & Flags) == 0;
		}
		set
		{
			if (value)
			{
				Flags &= ~FieldFlags.NoExport;
			}
			else
			{
				Flags |= FieldFlags.NoExport;
			}
			NotifyPropertyChanged("Export");
		}
	}

	internal override FieldFlags Flags
	{
		get
		{
			FieldFlags fieldFlags = base.Flags;
			if (fieldFlags == FieldFlags.Default && GetValue(base.Dictionary, m_crossTable, "Ff", inheritable: true) is PdfNumber pdfNumber)
			{
				fieldFlags = (FieldFlags)pdfNumber.IntValue;
			}
			return fieldFlags;
		}
		set
		{
			base.Flags = value;
			Changed = true;
		}
	}

	internal string ActualFieldName
	{
		get
		{
			string result = null;
			if (GetValue(base.Dictionary, m_crossTable, "T", inheritable: false) is PdfString pdfString)
			{
				result = pdfString.Value;
			}
			return result;
		}
	}

	public new PdfForm Form
	{
		get
		{
			if (m_form != null)
			{
				return m_form;
			}
			return base.Form;
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

	internal PdfDictionary Parent
	{
		get
		{
			PdfDictionary result = null;
			if (base.Dictionary.ContainsKey("Parent"))
			{
				result = m_crossTable.GetObject(base.Dictionary["Parent"]) as PdfDictionary;
			}
			return result;
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

	internal bool FieldChanged
	{
		get
		{
			return m_fieldChanged;
		}
		set
		{
			m_fieldChanged = value;
		}
	}

	internal int DefaultIndex
	{
		get
		{
			return m_defaultIndex;
		}
		set
		{
			m_defaultIndex = value;
		}
	}

	internal event BeforeNameChangesEventHandler BeforeNameChanges;

	internal PdfLoadedField(PdfDictionary dictionary, PdfCrossTable crossTable)
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

	public void SetName(string name)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (name == string.Empty)
		{
			throw new ArgumentException("The name can't be empty");
		}
		if (Name == null || !(Name != name))
		{
			return;
		}
		string[] array = Name.Split('.');
		int num = array.Length;
		if (!(array[num - 1] == name))
		{
			PdfString primitive = new PdfString(name);
			if (m_form != null)
			{
				this.BeforeNameChanges(name);
			}
			base.Dictionary.SetProperty("T", primitive);
			Changed = true;
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
			else if (pdfDictionary.ContainsKey("Parent"))
			{
				pdfDictionary = crossTable.GetObject(pdfDictionary["Parent"]) as PdfDictionary;
				if (pdfDictionary != null && !pdfDictionary.ContainsKey(value) && value == "Ff")
				{
					break;
				}
			}
			else
			{
				pdfDictionary = null;
			}
		}
		return pdfPrimitive;
	}

	internal static IPdfPrimitive GetValue(PdfDictionary dictionary, PdfCrossTable crossTable, string value, bool inheritable)
	{
		IPdfPrimitive pdfPrimitive = null;
		if (dictionary.ContainsKey(value))
		{
			pdfPrimitive = crossTable.GetObject(dictionary[value]);
		}
		else if (inheritable)
		{
			pdfPrimitive = SearchInParents(dictionary, crossTable, value);
		}
		if (pdfPrimitive != null && pdfPrimitive is PdfString)
		{
			bool flag = false;
			if (crossTable.Document != null && crossTable.Document is PdfLoadedDocument)
			{
				flag = (crossTable.Document as PdfLoadedDocument).WasEncrypted;
			}
			if (flag && (pdfPrimitive as PdfString).Hex && !(pdfPrimitive as PdfString).m_isParentDecrypted)
			{
				PdfReference reference = crossTable.GetReference(dictionary);
				if (reference != null)
				{
					(pdfPrimitive as PdfString).Decrypt(crossTable.Encryptor, reference.ObjNum);
				}
			}
		}
		return pdfPrimitive;
	}

	internal PdfDictionary GetWidgetAnnotation(PdfDictionary dictionary, PdfCrossTable crossTable)
	{
		PdfDictionary pdfDictionary = null;
		if (dictionary.ContainsKey("Kids"))
		{
			base.Dictionary = dictionary;
			PdfArray pdfArray = ObtainKids();
			if (pdfArray != null && pdfArray.Count > 0)
			{
				PdfReference reference = crossTable.GetReference(pdfArray[m_defaultIndex]);
				pdfDictionary = crossTable.GetObject(reference) as PdfDictionary;
			}
		}
		else if (dictionary.ContainsKey("Subtype") && (CrossTable.GetObject(dictionary["Subtype"]) as PdfName).Value == "Widget")
		{
			pdfDictionary = dictionary;
		}
		if (pdfDictionary == null)
		{
			pdfDictionary = dictionary;
		}
		return pdfDictionary;
	}

	private PdfArray ObtainKids()
	{
		PdfArray pdfArray = null;
		if (base.Dictionary.ContainsKey("Kids"))
		{
			pdfArray = CrossTable.GetObject(base.Dictionary["Kids"]) as PdfArray;
			if (pdfArray != null && pdfArray.Count > 0)
			{
				return RecursiveCall(pdfArray);
			}
		}
		return pdfArray;
	}

	private PdfArray ObtainKids(PdfDictionary dictionary)
	{
		PdfArray result = null;
		if (dictionary.ContainsKey("Kids"))
		{
			result = CrossTable.GetObject(dictionary["Kids"]) as PdfArray;
		}
		return result;
	}

	private PdfArray RecursiveCall(PdfArray kids)
	{
		if (kids != null && kids.Count > 0)
		{
			for (int i = 0; i < kids.Count; i++)
			{
				if (PdfCrossTable.Dereference(kids[i]) is PdfDictionary pdfDictionary && pdfDictionary.ContainsKey("T") && pdfDictionary.ContainsKey("Kids"))
				{
					PdfArray pdfArray = ObtainKids(pdfDictionary);
					if (pdfArray != null)
					{
						return RecursiveCall(pdfArray);
					}
				}
			}
		}
		return kids;
	}

	internal List<PdfDictionary> GetWidgetAnnotations(PdfDictionary dictionary, PdfCrossTable crossTable)
	{
		List<PdfDictionary> list = new List<PdfDictionary>();
		if (dictionary.ContainsKey("Kids"))
		{
			if (crossTable.GetObject(dictionary["Kids"]) is PdfArray { Count: >0 } pdfArray)
			{
				foreach (IPdfPrimitive item2 in pdfArray)
				{
					PdfReference reference = crossTable.GetReference(item2);
					if (reference != null && crossTable.GetObject(reference) is PdfDictionary item)
					{
						list.Add(item);
					}
				}
			}
		}
		else if (dictionary.ContainsKey("Subtype"))
		{
			PdfName pdfName = CrossTable.GetObject(dictionary["Subtype"]) as PdfName;
			if (pdfName != null && pdfName.Value == "Widget")
			{
				list.Add(dictionary);
			}
		}
		if (list.Count == 0)
		{
			list.Add(dictionary);
		}
		return list;
	}

	internal PdfHighlightMode GetHighLight(PdfDictionary dictionary, PdfCrossTable crossTable)
	{
		PdfName pdfName = null;
		if (dictionary.ContainsKey("Kids"))
		{
			PdfDictionary widgetAnnotation = GetWidgetAnnotation(dictionary, crossTable);
			if (widgetAnnotation.ContainsKey("H"))
			{
				pdfName = crossTable.GetObject(widgetAnnotation["H"]) as PdfName;
			}
		}
		else if (dictionary.ContainsKey("H"))
		{
			pdfName = crossTable.GetObject(dictionary["H"]) as PdfName;
		}
		PdfHighlightMode result = PdfHighlightMode.NoHighlighting;
		if (pdfName != null)
		{
			switch (pdfName.Value)
			{
			case "I":
				result = PdfHighlightMode.Invert;
				break;
			case "O":
				result = PdfHighlightMode.Outline;
				break;
			case "P":
				result = PdfHighlightMode.Push;
				break;
			}
		}
		return result;
	}

	internal abstract override void Draw();

	internal abstract PdfLoadedFieldItem CreateLoadedItem(PdfDictionary dictionary);

	internal override void ApplyName(string name)
	{
		SetName(name);
	}

	internal virtual void BeginSave()
	{
	}

	private PdfPageBase GetLoadedPage()
	{
		PdfPageBase pdfPageBase = base.Page;
		if (pdfPageBase == null)
		{
			PdfLoadedDocument pdfLoadedDocument = CrossTable.Document as PdfLoadedDocument;
			PdfDictionary pdfDictionary = GetWidgetAnnotation(base.Dictionary, CrossTable);
			if (pdfDictionary == null)
			{
				pdfDictionary = base.Dictionary;
			}
			if (pdfDictionary.ContainsKey("P") && !(PdfCrossTable.Dereference(pdfDictionary["P"]) is PdfNull))
			{
				if (CrossTable.GetObject(pdfDictionary["P"]) is PdfDictionary dic)
				{
					pdfPageBase = pdfLoadedDocument.Pages.GetPage(dic);
				}
			}
			else
			{
				pdfPageBase = FindWidgetPageReference(pdfDictionary, pdfLoadedDocument);
			}
			if (pdfPageBase == null && base.Dictionary.ContainsKey("Kids") && CrossTable.GetObject(base.Dictionary["Kids"]) is PdfArray pdfArray)
			{
				for (int i = 0; i < pdfArray.Count; i++)
				{
					pdfDictionary = PdfCrossTable.Dereference(pdfArray[i]) as PdfDictionary;
					pdfPageBase = FindWidgetPageReference(pdfDictionary, pdfLoadedDocument);
				}
			}
		}
		return pdfPageBase;
	}

	private PdfPageBase FindWidgetPageReference(PdfDictionary widget, PdfLoadedDocument doc)
	{
		PdfPageBase result = null;
		PdfReference reference = CrossTable.GetReference(widget);
		foreach (PdfPageBase page in doc.Pages)
		{
			PdfArray pdfArray = page.ObtainAnnotations();
			if (pdfArray == null)
			{
				continue;
			}
			for (int i = 0; i < pdfArray.Count; i++)
			{
				PdfReferenceHolder pdfReferenceHolder = pdfArray[i] as PdfReferenceHolder;
				if (pdfReferenceHolder != null && pdfReferenceHolder.Reference == reference)
				{
					return page;
				}
				if (m_requiredReference != null && m_requiredReference.Reference == pdfReferenceHolder.Reference)
				{
					return page;
				}
			}
		}
		return result;
	}

	internal void ExportField(XmlWriter textWriter)
	{
		PdfName pdfName = GetValue(base.Dictionary, CrossTable, "FT", inheritable: true) as PdfName;
		PdfDictionary font = null;
		string text = Name;
		GetEncodedFontDictionary(base.Dictionary, out font);
		if (font != null)
		{
			text = UpdateEncodedValue(text, font);
		}
		if (!(pdfName != null))
		{
			return;
		}
		switch (pdfName.Value)
		{
		case "Tx":
		{
			PdfString pdfString2 = GetValue(base.Dictionary, CrossTable, "V", inheritable: true) as PdfString;
			if (pdfString2 != null || ExportEmptyField)
			{
				textWriter.WriteStartElement(XmlConvert.EncodeName(text), "");
				if (pdfString2 != null)
				{
					pdfString2.Value = UpdateEncodedValue(pdfString2.Value, font);
					textWriter.WriteString(pdfString2.Value);
				}
				else if (ExportEmptyField)
				{
					textWriter.WriteString("");
				}
				textWriter.WriteEndElement();
			}
			break;
		}
		case "Ch":
		{
			IPdfPrimitive value2 = GetValue(base.Dictionary, CrossTable, "V", inheritable: true);
			string text3 = null;
			if (value2 != null)
			{
				text3 = GetExportValue(this, value2);
			}
			if (value2 == null && base.Dictionary.ContainsKey("I"))
			{
				text3 = ((!(GetType().Name == "PdfLoadedListBoxField")) ? (this as PdfLoadedComboBoxField).SelectedValue : (this as PdfLoadedListBoxField).SelectedValue[0]);
			}
			if ((selectedList == null && !string.IsNullOrEmpty(text3)) || ExportEmptyField)
			{
				textWriter.WriteStartElement(XmlConvert.EncodeName(Name), "");
				if (!string.IsNullOrEmpty(text3))
				{
					textWriter.WriteString(text3);
				}
				else if (ExportEmptyField)
				{
					textWriter.WriteString("");
				}
				textWriter.WriteEndElement();
			}
			else
			{
				if (selectedList == null || selectedList.Count <= 0)
				{
					break;
				}
				textWriter.WriteStartElement(XmlConvert.EncodeName(Name), "");
				foreach (string selected in selectedList)
				{
					if (!string.IsNullOrEmpty(selected))
					{
						textWriter.WriteString(selected);
					}
					if (selectedList.IndexOf(selected) < selectedList.Count - 1)
					{
						textWriter.WriteString(",");
					}
				}
				textWriter.WriteEndElement();
			}
			break;
		}
		case "Btn":
		{
			IPdfPrimitive value = GetValue(base.Dictionary, CrossTable, "V", inheritable: true);
			PdfLoadedRadioButtonListField pdfLoadedRadioButtonListField = null;
			if (value != null)
			{
				string text2 = GetExportValue(this, value);
				if (this is PdfLoadedRadioButtonListField)
				{
					pdfLoadedRadioButtonListField = this as PdfLoadedRadioButtonListField;
				}
				if (pdfLoadedRadioButtonListField != null && pdfLoadedRadioButtonListField.SelectedIndex == -1)
				{
					textWriter.WriteStartElement(XmlConvert.EncodeName(Name), "");
					if (ExportEmptyField)
					{
						textWriter.WriteString("");
					}
					else
					{
						textWriter.WriteString("Off");
					}
					textWriter.WriteEndElement();
				}
				else if (base.Dictionary != null && base.Dictionary.ContainsKey("Opt"))
				{
					PdfArray pdfArray = PdfCrossTable.Dereference(base.Dictionary["Opt"]) as PdfArray;
					int result = 0;
					try
					{
						int.TryParse(text2, out result);
						if (pdfArray != null)
						{
							PdfString pdfString = null;
							pdfString = ((pdfLoadedRadioButtonListField == null) ? (PdfCrossTable.Dereference(pdfArray[result]) as PdfString) : (PdfCrossTable.Dereference(pdfArray[pdfLoadedRadioButtonListField.SelectedIndex]) as PdfString));
							if (pdfString != null)
							{
								text2 = pdfString.Value;
							}
						}
					}
					catch
					{
					}
					if (!string.IsNullOrEmpty(text2) || ExportEmptyField)
					{
						textWriter.WriteStartElement(XmlConvert.EncodeName(Name), "");
						if (text2 != null)
						{
							textWriter.WriteString(text2);
						}
						else if (ExportEmptyField)
						{
							textWriter.WriteString("");
						}
						textWriter.WriteEndElement();
					}
				}
				else if (!string.IsNullOrEmpty(text2) || ExportEmptyField)
				{
					textWriter.WriteStartElement(XmlConvert.EncodeName(Name), "");
					if (text2 != null)
					{
						textWriter.WriteString(text2);
					}
					else if (ExportEmptyField)
					{
						textWriter.WriteString("");
					}
					textWriter.WriteEndElement();
				}
				else if (this is PdfLoadedCheckBoxField)
				{
					textWriter.WriteStartElement(XmlConvert.EncodeName(Name), "");
					if (ExportEmptyField)
					{
						textWriter.WriteString("");
					}
					else
					{
						textWriter.WriteString("Off");
					}
					textWriter.WriteEndElement();
				}
				break;
			}
			if (this is PdfLoadedRadioButtonListField)
			{
				textWriter.WriteStartElement(XmlConvert.EncodeName(Name), "");
				textWriter.WriteString(GetAppearanceStateValue(this));
				textWriter.WriteEndElement();
				break;
			}
			PdfName pdfName2 = GetWidgetAnnotation(base.Dictionary, CrossTable)["AS"] as PdfName;
			if (pdfName2 != null || ExportEmptyField)
			{
				textWriter.WriteStartElement(XmlConvert.EncodeName(Name), "");
				if (pdfName2 != null)
				{
					textWriter.WriteString(pdfName2.Value);
				}
				else
				{
					textWriter.WriteString("");
				}
				textWriter.WriteEndElement();
			}
			break;
		}
		}
	}

	internal void ExportField(XmlWriter textWriter, Dictionary<object, object> table, string uniquekey)
	{
		PdfName obj = GetValue(base.Dictionary, CrossTable, "FT", inheritable: true) as PdfName;
		bool flag = false;
		string text = Name;
		PdfDictionary font = null;
		GetEncodedFontDictionary(base.Dictionary, out font);
		if (font != null)
		{
			text = UpdateEncodedValue(text, font);
		}
		switch (obj.Value)
		{
		case "Tx":
		{
			PdfString pdfString2 = GetValue(base.Dictionary, CrossTable, "V", inheritable: true) as PdfString;
			if (base.Dictionary.ContainsKey("RV"))
			{
				text += uniquekey;
			}
			if (pdfString2 != null && !flag)
			{
				pdfString2.Value = UpdateEncodedValue(pdfString2.Value, font);
				SetFields(text, ReplaceCRtoLF(pdfString2.Value), table);
			}
			break;
		}
		case "Ch":
		{
			PdfName pdfName2 = GetValue(base.Dictionary, CrossTable, "V", inheritable: true) as PdfName;
			string empty = string.Empty;
			if (pdfName2 != null)
			{
				pdfName2.Value = UpdateEncodedValue(pdfName2.Value, font);
				SetFields(text, pdfName2.Value, table);
				break;
			}
			if (GetValue(base.Dictionary, CrossTable, "V", inheritable: true) is PdfString pdfString3)
			{
				pdfString3.Value = UpdateEncodedValue(pdfString3.Value, font);
				SetFields(text, pdfString3.Value, table);
				break;
			}
			PdfArray pdfArray2 = GetValue(base.Dictionary, CrossTable, "V", inheritable: true) as PdfArray;
			if (pdfArray2 != null)
			{
				SetFields(text, pdfArray2, table);
			}
			if (pdfArray2 == null && base.Dictionary.ContainsKey("I"))
			{
				empty = ((!(GetType().Name == "PdfLoadedListBoxField")) ? (this as PdfLoadedComboBoxField).SelectedValue : (this as PdfLoadedListBoxField).SelectedValue[0]);
				empty = UpdateEncodedValue(empty, font);
				SetFields(text, empty, table);
			}
			break;
		}
		case "Btn":
		{
			IPdfPrimitive value = GetValue(base.Dictionary, CrossTable, "V", inheritable: true);
			PdfLoadedRadioButtonListField pdfLoadedRadioButtonListField = null;
			if (value != null)
			{
				string text2 = string.Empty;
				if (value is PdfArray)
				{
					GetMultipleSelectedItems(this, value);
				}
				else
				{
					text2 = GetExportValue(this, value);
				}
				if (!string.IsNullOrEmpty(text2))
				{
					text2 = HexToString(text2);
					if (this is PdfLoadedRadioButtonListField)
					{
						pdfLoadedRadioButtonListField = this as PdfLoadedRadioButtonListField;
					}
					if (!base.Dictionary.ContainsKey("Opt") || (pdfLoadedRadioButtonListField != null && pdfLoadedRadioButtonListField.SelectedIndex == -1))
					{
						SetFields(text, text2, table);
					}
					else
					{
						if (base.Dictionary == null || !base.Dictionary.ContainsKey("Opt"))
						{
							break;
						}
						PdfArray pdfArray = PdfCrossTable.Dereference(base.Dictionary["Opt"]) as PdfArray;
						int result = 0;
						try
						{
							int.TryParse(text2, out result);
							if (pdfArray != null)
							{
								PdfString pdfString = null;
								pdfString = ((pdfLoadedRadioButtonListField == null) ? (PdfCrossTable.Dereference(pdfArray[result]) as PdfString) : (PdfCrossTable.Dereference(pdfArray[pdfLoadedRadioButtonListField.SelectedIndex]) as PdfString));
								if (pdfString != null)
								{
									text2 = pdfString.Value;
								}
							}
							if (!string.IsNullOrEmpty(text2))
							{
								text2 = UpdateEncodedValue(text2, font);
								SetFields(text, text2, table);
							}
							break;
						}
						catch
						{
							break;
						}
					}
				}
				else if (this is PdfLoadedRadioButtonListField || this is PdfLoadedCheckBoxField)
				{
					SetFields(text, "Off", table);
				}
				break;
			}
			if (this is PdfLoadedRadioButtonListField)
			{
				SetFields(text, GetAppearanceStateValue(this), table);
				break;
			}
			PdfDictionary widgetAnnotation = GetWidgetAnnotation(base.Dictionary, CrossTable);
			if (widgetAnnotation != null)
			{
				PdfName pdfName = widgetAnnotation["AS"] as PdfName;
				if (pdfName != null)
				{
					SetFields(text, pdfName.Value, table);
				}
			}
			break;
		}
		}
	}

	private string HexToString(string text)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < text.Length; i++)
		{
			if (text[i] == '#')
			{
				string text2 = text.Substring(i + 1, 2);
				int num = int.Parse(text2, NumberStyles.HexNumber);
				if (num > 127 && num < 255)
				{
					stringBuilder.Append("%" + text2);
				}
				else
				{
					stringBuilder.Append((char)num);
				}
				i += 2;
			}
			else
			{
				stringBuilder.Append(text[i]);
			}
		}
		return stringBuilder.ToString();
	}

	private string ReplaceCRtoLF(string value)
	{
		if (this is PdfLoadedTextBoxField { Multiline: false })
		{
			return value;
		}
		value = value.Replace("\r\n", "\r");
		return value.Replace("\r", "\n");
	}

	private string ReverseLFtoCR(string value)
	{
		if (this is PdfLoadedTextBoxField { Multiline: false })
		{
			return value;
		}
		return value.Replace("\r\n", "\r").Replace("\n", "\r");
	}

	internal string GetExportValue(PdfLoadedField field, IPdfPrimitive buttonFieldPrimitive)
	{
		string text = null;
		PdfName pdfName = buttonFieldPrimitive as PdfName;
		if (pdfName != null)
		{
			text = pdfName.Value;
		}
		else if (buttonFieldPrimitive is PdfString)
		{
			if (buttonFieldPrimitive is PdfString pdfString)
			{
				text = pdfString.Value;
			}
		}
		else if (buttonFieldPrimitive is PdfArray)
		{
			selectedList = new List<string>();
			foreach (IPdfPrimitive item in buttonFieldPrimitive as PdfArray)
			{
				if (item is PdfName)
				{
					text = (item as PdfName).Value;
					selectedList.Add((item as PdfName).Value);
				}
				else if (item is PdfString)
				{
					text = (item as PdfString).Value;
					selectedList.Add((item as PdfString).Value);
				}
			}
		}
		if (text != null && field is PdfLoadedRadioButtonListField pdfLoadedRadioButtonListField)
		{
			PdfLoadedRadioButtonItem selectedItem = pdfLoadedRadioButtonListField.SelectedItem;
			if (selectedItem != null && selectedItem.Value == text && !string.IsNullOrEmpty(selectedItem.Value))
			{
				text = selectedItem.Value;
			}
		}
		return text;
	}

	private object GetMultipleSelectedItems(PdfLoadedField field, IPdfPrimitive buttonFieldPrimitive)
	{
		string result = null;
		if (buttonFieldPrimitive is PdfArray)
		{
			List<string> list = new List<string>();
			{
				foreach (IPdfPrimitive item in buttonFieldPrimitive as PdfArray)
				{
					if (item is PdfName)
					{
						list.Add((item as PdfName).Value);
					}
					else if (item is PdfString)
					{
						list.Add((item as PdfString).Value);
					}
				}
				return list;
			}
		}
		if (buttonFieldPrimitive is PdfString pdfString)
		{
			result = pdfString.Value;
		}
		return result;
	}

	internal string GetAppearanceStateValue(PdfLoadedField field)
	{
		List<PdfDictionary> widgetAnnotations = field.GetWidgetAnnotations(field.Dictionary, field.CrossTable);
		string text = null;
		if (widgetAnnotations != null)
		{
			foreach (PdfDictionary item in widgetAnnotations)
			{
				PdfName pdfName = item["AS"] as PdfName;
				if (pdfName != null && pdfName.Value != "Off")
				{
					text = pdfName.Value;
				}
			}
		}
		if (text == null && ExportEmptyField)
		{
			text = "";
		}
		else if (text == null)
		{
			text = "Off";
		}
		return text;
	}

	internal void ExportField(Stream stream, ref int objectid)
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
					flag = flag || pdfArray[i] is PdfLoadedField;
				}
			}
		}
		PdfName pdfName = GetValue(base.Dictionary, CrossTable, "FT", inheritable: true) as PdfName;
		string strValue = "";
		string[] array = null;
		if (pdfName != null)
		{
			switch (pdfName.Value)
			{
			case "Tx":
				if (GetValue(base.Dictionary, CrossTable, "V", inheritable: true) is PdfString pdfString2)
				{
					strValue = pdfString2.Value;
				}
				break;
			case "Ch":
			{
				IPdfPrimitive value2 = GetValue(base.Dictionary, CrossTable, "V", inheritable: true);
				string text2 = null;
				if (value2 != null)
				{
					if (value2 is PdfArray)
					{
						array = (GetMultipleSelectedItems(this, value2) as List<string>).ToArray();
					}
					else
					{
						array = new string[1];
						text2 = (array[0] = GetExportValue(this, value2));
					}
				}
				if (value2 == null && base.Dictionary.ContainsKey("I"))
				{
					text2 = ((!(GetType().Name == "PdfLoadedListBoxField")) ? (this as PdfLoadedComboBoxField).SelectedValue : (this as PdfLoadedListBoxField).SelectedValue[0]);
				}
				if (!string.IsNullOrEmpty(text2))
				{
					strValue = text2;
				}
				break;
			}
			case "Btn":
			{
				IPdfPrimitive value = GetValue(base.Dictionary, CrossTable, "V", inheritable: true);
				PdfLoadedRadioButtonListField pdfLoadedRadioButtonListField = null;
				if (value != null)
				{
					string text = GetExportValue(this, value);
					if (this is PdfLoadedRadioButtonListField)
					{
						pdfLoadedRadioButtonListField = this as PdfLoadedRadioButtonListField;
					}
					if (pdfLoadedRadioButtonListField != null && pdfLoadedRadioButtonListField.SelectedIndex == -1)
					{
						if (!ExportEmptyField)
						{
							strValue = "Off";
						}
					}
					else if (base.Dictionary != null && base.Dictionary.ContainsKey("Opt"))
					{
						PdfArray pdfArray2 = PdfCrossTable.Dereference(base.Dictionary["Opt"]) as PdfArray;
						int result = 0;
						try
						{
							int.TryParse(text, out result);
							if (pdfArray2 != null)
							{
								PdfString pdfString = null;
								pdfString = ((pdfLoadedRadioButtonListField == null) ? (PdfCrossTable.Dereference(pdfArray2[result]) as PdfString) : (PdfCrossTable.Dereference(pdfArray2[pdfLoadedRadioButtonListField.SelectedIndex]) as PdfString));
								if (pdfString != null)
								{
									text = pdfString.Value;
								}
							}
						}
						catch
						{
						}
						if (!string.IsNullOrEmpty(text))
						{
							strValue = text;
						}
					}
					else if (!string.IsNullOrEmpty(text))
					{
						strValue = text;
					}
					else if (this is PdfLoadedCheckBoxField && !ExportEmptyField)
					{
						strValue = "Off";
					}
					break;
				}
				if (this is PdfLoadedRadioButtonListField)
				{
					strValue = GetAppearanceStateValue(this);
					break;
				}
				PdfDictionary widgetAnnotation = GetWidgetAnnotation(base.Dictionary, CrossTable);
				if (widgetAnnotation != null)
				{
					PdfName pdfName2 = widgetAnnotation["AS"] as PdfName;
					if (pdfName2 != null)
					{
						strValue = pdfName2.Value;
					}
				}
				break;
			}
			}
		}
		if (array != null)
		{
			string[] array2 = array;
			foreach (string strValue2 in array2)
			{
				ExportSelectedFDFField(stream, strValue2, pdfArray, flag, ref objectid);
			}
		}
		else
		{
			ExportSelectedFDFField(stream, strValue, pdfArray, flag, ref objectid);
		}
	}

	private void ExportSelectedFDFField(Stream stream, string strValue1, PdfArray kids, bool flag, ref int objectid)
	{
		string empty = string.Empty;
		if (!(!validateString(strValue1) || ExportEmptyField || flag))
		{
			return;
		}
		if (flag)
		{
			for (int i = 0; i < kids.Count; i++)
			{
				if (kids[i] is PdfLoadedField { Export: not false } pdfLoadedField)
				{
					pdfLoadedField.ExportField(stream, ref objectid);
				}
			}
			ObjectID = objectid;
			objectid++;
			StringBuilder stringBuilder = new StringBuilder();
			PdfString pdfString = new PdfString(strValue1);
			pdfString.Encode = PdfString.ForceEncoding.ASCII;
			byte[] bytes = Encoding.GetEncoding("UTF-8").GetBytes(pdfString.Value);
			stringBuilder.AppendFormat("{0} 0 obj<</T <{1}> /Kids [", ObjectID, PdfString.BytesToHex(bytes));
			for (int j = 0; j < kids.Count; j++)
			{
				if (kids[j] is PdfLoadedField { Export: not false, ObjectID: not 0 } pdfLoadedField2)
				{
					stringBuilder.AppendFormat("{0} 0 R ", pdfLoadedField2.ObjectID);
				}
			}
			stringBuilder.Append("]>>endobj\n");
			PdfString pdfString2 = new PdfString(stringBuilder.ToString());
			pdfString2.Encode = PdfString.ForceEncoding.ASCII;
			byte[] bytes2 = Encoding.GetEncoding("UTF-8").GetBytes(pdfString2.Value);
			stream.Write(bytes2, 0, bytes2.Length);
		}
		else
		{
			ObjectID = objectid;
			objectid++;
			if (GetType().Name == "PdfLoadedCheckBoxField" || GetType().Name == "PdfLoadedRadioButtonListField")
			{
				empty = "/" + strValue1;
			}
			else
			{
				PdfString pdfString3 = new PdfString(strValue1);
				pdfString3.Encode = PdfString.ForceEncoding.ASCII;
				byte[] bytes3 = Encoding.GetEncoding("UTF-8").GetBytes(pdfString3.Value);
				empty = "<" + PdfString.BytesToHex(bytes3) + ">";
			}
			StringBuilder stringBuilder2 = new StringBuilder();
			PdfString pdfString4 = new PdfString(Name)
			{
				Encode = PdfString.ForceEncoding.ASCII
			};
			stringBuilder2.AppendFormat(arg1: PdfString.BytesToHex(Encoding.GetEncoding("UTF-8").GetBytes(pdfString4.Value)), format: "{0} 0 obj<</T <{1}> /V {2} >>endobj\n", arg0: ObjectID, arg2: empty);
			PdfString pdfString5 = new PdfString(stringBuilder2.ToString());
			pdfString5.Encode = PdfString.ForceEncoding.ASCII;
			byte[] bytes4 = Encoding.GetEncoding("UTF-8").GetBytes(pdfString5.Value);
			stream.Write(bytes4, 0, bytes4.Length);
		}
	}

	internal void ExportField(Dictionary<string, object> table)
	{
		PdfArray pdfArray = null;
		PdfName pdfName = GetValue(base.Dictionary, CrossTable, "FT", inheritable: true) as PdfName;
		string value = "";
		if (!(pdfName != null))
		{
			return;
		}
		switch (pdfName.Value)
		{
		case "Tx":
		{
			List<PdfString> list = new List<PdfString>();
			if (base.Dictionary.ContainsKey("RV"))
			{
				PdfString item = GetValue(base.Dictionary, CrossTable, "RV", inheritable: true) as PdfString;
				list.Add(item);
			}
			if (GetValue(base.Dictionary, CrossTable, "V", inheritable: true) is PdfString pdfString2)
			{
				value = pdfString2.Value;
				list.Add(new PdfString(value));
				SetFields(Name, list, table);
			}
			break;
		}
		case "Ch":
		{
			IPdfPrimitive value3 = GetValue(base.Dictionary, CrossTable, "V", inheritable: true);
			if (value3 != null)
			{
				pdfArray = value3 as PdfArray;
				string exportValue = GetExportValue(this, value3);
				if (!string.IsNullOrEmpty(exportValue))
				{
					if (selectedList != null && selectedList.Count >= 1)
					{
						SetFields(Name, pdfArray, table);
						break;
					}
					value = exportValue;
					SetFields(Name, new PdfString(exportValue), table);
				}
				else if (pdfArray != null)
				{
					SetFields(Name, pdfArray, table);
				}
			}
			else if (value3 == null && base.Dictionary.ContainsKey("I"))
			{
				SetFields(Fieldvalue: new PdfString((!(GetType().Name == "PdfLoadedListBoxField")) ? (this as PdfLoadedComboBoxField).SelectedValue : (this as PdfLoadedListBoxField).SelectedValue[0]), fieldName: Name, table: table);
			}
			break;
		}
		case "Btn":
		{
			IPdfPrimitive value2 = GetValue(base.Dictionary, CrossTable, "V", inheritable: true);
			PdfLoadedRadioButtonListField pdfLoadedRadioButtonListField = null;
			if (value2 != null)
			{
				string text = GetExportValue(this, value2);
				if (this is PdfLoadedRadioButtonListField)
				{
					pdfLoadedRadioButtonListField = this as PdfLoadedRadioButtonListField;
				}
				if (pdfLoadedRadioButtonListField != null && pdfLoadedRadioButtonListField.SelectedIndex == -1)
				{
					value = "Off";
					SetFields(Name, new PdfString(value), table);
					break;
				}
				if (base.Dictionary != null && base.Dictionary.ContainsKey("Opt"))
				{
					PdfArray pdfArray2 = PdfCrossTable.Dereference(base.Dictionary["Opt"]) as PdfArray;
					int result = 0;
					try
					{
						int.TryParse(text, out result);
						if (pdfArray2 != null)
						{
							PdfString pdfString = null;
							pdfString = ((pdfLoadedRadioButtonListField == null) ? (PdfCrossTable.Dereference(pdfArray2[result]) as PdfString) : (PdfCrossTable.Dereference(pdfArray2[pdfLoadedRadioButtonListField.SelectedIndex]) as PdfString));
							if (pdfString != null)
							{
								text = pdfString.Value;
							}
						}
						if (!string.IsNullOrEmpty(text))
						{
							SetFields(Name, new PdfString(text), table);
						}
						break;
					}
					catch
					{
						break;
					}
				}
				if (!string.IsNullOrEmpty(text))
				{
					SetFields(Name, new PdfName(text), table);
				}
				else if (this is PdfLoadedCheckBoxField)
				{
					SetFields(Name, new PdfString("Off"), table);
				}
				break;
			}
			if (this is PdfLoadedRadioButtonListField)
			{
				value = GetAppearanceStateValue(this);
			}
			else
			{
				PdfDictionary widgetAnnotation = GetWidgetAnnotation(base.Dictionary, CrossTable);
				if (widgetAnnotation != null)
				{
					PdfName pdfName2 = widgetAnnotation["AS"] as PdfName;
					if (pdfName2 != null)
					{
						value = pdfName2.Value;
					}
				}
			}
			if (!string.IsNullOrEmpty(value))
			{
				SetFields(Name, new PdfName(value), table);
			}
			break;
		}
		}
	}

	internal void SetFields(object fieldName, object Fieldvalue, Dictionary<object, object> table)
	{
		table[fieldName] = Fieldvalue;
	}

	internal void SetFields(string fieldName, object Fieldvalue, Dictionary<string, object> table)
	{
		table[fieldName] = Fieldvalue;
	}

	internal void ImportFieldValue(object FieldValue)
	{
		PdfName pdfName = GetValue(base.Dictionary, CrossTable, "FT", inheritable: true) as PdfName;
		string text = FieldValue as string;
		string[] array = null;
		if (text == null)
		{
			array = FieldValue as string[];
			if (array != null)
			{
				text = array[0];
			}
		}
		if (text == null)
		{
			return;
		}
		switch (pdfName.Value)
		{
		case "Tx":
			if (text != null)
			{
				(this as PdfLoadedTextBoxField).m_isImportFields = true;
				(this as PdfLoadedTextBoxField).Text = ReverseLFtoCR(text);
			}
			break;
		case "Ch":
			if (GetType().Name == "PdfLoadedListBoxField")
			{
				if (array == null)
				{
					string[] array2 = text.Split(',');
					if (array2.Length > 1)
					{
						array = array2;
					}
				}
				PdfLoadedListBoxField pdfLoadedListBoxField = this as PdfLoadedListBoxField;
				if (array != null)
				{
					pdfLoadedListBoxField.SelectedValue = array;
					break;
				}
				pdfLoadedListBoxField.SelectedValue = new string[1] { text.Split(',')[0] };
			}
			else if (GetType().Name == "PdfLoadedComboBoxField")
			{
				PdfLoadedComboBoxField pdfLoadedComboBoxField = this as PdfLoadedComboBoxField;
				if (isAcrobat && pdfLoadedComboBoxField != null && pdfLoadedComboBoxField.Dictionary.ContainsKey("AP"))
				{
					pdfLoadedComboBoxField.Dictionary.Remove("AP");
				}
				pdfLoadedComboBoxField.SelectedValue = text;
			}
			break;
		case "Btn":
			if (this is PdfLoadedCheckBoxField pdfLoadedCheckBoxField)
			{
				if (text.ToUpper() == "off".ToUpper() || text.ToUpper() == "no".ToUpper())
				{
					pdfLoadedCheckBoxField.Checked = false;
				}
				else if (ContainsExportValue(text, pdfLoadedCheckBoxField.Dictionary))
				{
					pdfLoadedCheckBoxField.Checked = true;
				}
				else if (text.ToUpper() == "on".ToUpper() || text.ToUpper() == "yes".ToUpper())
				{
					pdfLoadedCheckBoxField.Checked = true;
				}
				else
				{
					pdfLoadedCheckBoxField.Checked = false;
				}
			}
			else
			{
				if (!(GetType().Name == "PdfLoadedRadioButtonListField"))
				{
					break;
				}
				PdfLoadedRadioButtonListField pdfLoadedRadioButtonListField = this as PdfLoadedRadioButtonListField;
				string text2 = text;
				if (text.Contains("%"))
				{
					text = text.Replace("%", "#");
				}
				else if (!text.Contains("#"))
				{
					text = PdfName.EncodeName(text);
				}
				if (pdfLoadedRadioButtonListField.Dictionary.ContainsKey("Opt") && PdfCrossTable.Dereference(pdfLoadedRadioButtonListField.Dictionary["Opt"]) is PdfArray pdfArray)
				{
					for (int i = 0; i < pdfArray.Count; i++)
					{
						if (PdfCrossTable.Dereference(pdfArray[i]) is PdfString pdfString && (pdfString.Value == text || pdfString.Value == text2))
						{
							text = i.ToString();
							break;
						}
					}
				}
				pdfLoadedRadioButtonListField.SelectedValue = text;
			}
			break;
		}
	}

	private bool ContainsExportValue(string value, PdfDictionary dictionary)
	{
		bool flag = false;
		PdfDictionary pdfDictionary = new PdfDictionary();
		PdfDictionary pdfDictionary2 = new PdfDictionary();
		PdfDictionary pdfDictionary3 = new PdfDictionary();
		if (dictionary.ContainsKey("Kids"))
		{
			if (CrossTable.GetObject(dictionary["Kids"]) is PdfArray pdfArray)
			{
				for (int i = 0; i < pdfArray.Count; i++)
				{
					DefaultIndex = i;
					pdfDictionary = GetWidgetAnnotation(dictionary, CrossTable);
					if (pdfDictionary != null && pdfDictionary.ContainsKey("AP") && CrossTable.GetObject(pdfDictionary["AP"]) is PdfDictionary pdfDictionary4 && pdfDictionary4.ContainsKey("N") && PdfCrossTable.Dereference(pdfDictionary4["N"]) is PdfDictionary pdfDictionary5 && pdfDictionary5.ContainsKey(value))
					{
						return true;
					}
				}
			}
		}
		else
		{
			pdfDictionary = GetWidgetAnnotation(dictionary, CrossTable);
			if (pdfDictionary != null && pdfDictionary.ContainsKey("AP") && CrossTable.GetObject(pdfDictionary["AP"]) is PdfDictionary pdfDictionary6 && pdfDictionary6.ContainsKey("N") && PdfCrossTable.Dereference(pdfDictionary6["N"]) is PdfDictionary pdfDictionary7 && pdfDictionary7.ContainsKey(value))
			{
				flag = true;
			}
			if (isAcrobat && !flag && pdfDictionary.ContainsKey("AS"))
			{
				string value2 = (CrossTable.GetObject(pdfDictionary["AS"]) as PdfName).Value;
				if (value2 == "Off" && pdfDictionary.ContainsKey("Opt"))
				{
					PdfArray pdfArray2 = PdfCrossTable.Dereference(pdfDictionary["Opt"]) as PdfArray;
					for (int j = 0; j < pdfArray2.Count; j++)
					{
						if ((pdfArray2[j] as PdfString).Value == value)
						{
							flag = true;
						}
					}
				}
				else if (value2 != "Off")
				{
					flag = true;
				}
			}
		}
		return flag;
	}

	internal static bool validateString(string text1)
	{
		if (text1 != null)
		{
			return text1.Length == 0;
		}
		return true;
	}

	internal string GetFieldName()
	{
		string text = null;
		PdfString pdfString = null;
		if (!base.Dictionary.ContainsKey("Parent"))
		{
			pdfString = GetValue(base.Dictionary, m_crossTable, "T", inheritable: false) as PdfString;
		}
		else
		{
			PdfDictionary pdfDictionary = m_crossTable.GetObject(base.Dictionary["Parent"]) as PdfDictionary;
			if (pdfDictionary != null)
			{
				while (pdfDictionary.ContainsKey("Parent"))
				{
					if (pdfDictionary.ContainsKey("T"))
					{
						text = ((text == null) ? (GetValue(pdfDictionary, m_crossTable, "T", inheritable: false) as PdfString).Value : string.Format("{0}.{1}", (GetValue(pdfDictionary, m_crossTable, "T", inheritable: false) as PdfString).Value, text));
					}
					pdfDictionary = m_crossTable.GetObject(pdfDictionary["Parent"]) as PdfDictionary;
					if (pdfDictionary != null && !pdfDictionary.ContainsKey("T"))
					{
						break;
					}
				}
				if (pdfDictionary.ContainsKey("T"))
				{
					text = ((text == null) ? (GetValue(pdfDictionary, m_crossTable, "T", inheritable: false) as PdfString).Value : string.Format("{0}.{1}", (GetValue(pdfDictionary, m_crossTable, "T", inheritable: false) as PdfString).Value, text));
					if (GetValue(base.Dictionary, m_crossTable, "T", inheritable: false) is PdfString pdfString2)
					{
						text += $".{pdfString2.Value}";
					}
				}
				else if (base.Dictionary.ContainsKey("T"))
				{
					pdfString = GetValue(base.Dictionary, m_crossTable, "T", inheritable: false) as PdfString;
				}
			}
			else
			{
				pdfString = GetValue(base.Dictionary, m_crossTable, "T", inheritable: false) as PdfString;
			}
		}
		if (pdfString != null)
		{
			text = pdfString.Value;
		}
		return text;
	}

	internal string ReplaceNotUsedCharacters(string input, Dictionary<string, string> encodingDifference, FontStructure fontStructure)
	{
		char[] array = input.ToCharArray();
		string text = string.Empty;
		char[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			char c = array2[i];
			int num = c;
			if (encodingDifference.ContainsKey(num.ToString()))
			{
				string text2 = encodingDifference[num.ToString()];
				text = (((text2.Length <= 1 || !(fontStructure.fontType.Value != "Type3")) && (c <= '\u007f' || c > 'ÿ' || !(fontStructure.fontType.Value == "Type1") || !(fontStructure.BaseFontEncoding != "WinAnsiEncoding") || !(fontStructure.FontEncoding == "Encoding") || !(fontStructure.FontName != "ZapfDingbats"))) ? (text + text2) : (text + c));
			}
			else
			{
				text += c;
			}
		}
		return text;
	}

	internal string UpdateEncodedValue(string value, PdfDictionary font)
	{
		string text = value;
		bool flag = false;
		FontStructure fontStructure = null;
		if (CrossTable != null && CrossTable.m_pdfDocumentEncoding != null)
		{
			fontStructure = new FontStructure(CrossTable.m_pdfDocumentEncoding);
			return text = ReplaceNotUsedCharacters(text, fontStructure.DifferencesDictionary, fontStructure);
		}
		if (CrossTable != null && CrossTable.DocumentCatalog != null)
		{
			if (PdfCrossTable.Dereference(CrossTable.DocumentCatalog["AcroForm"]) is PdfDictionary pdfDictionary && pdfDictionary.ContainsKey("DR") && PdfCrossTable.Dereference(pdfDictionary["DR"]) is PdfDictionary pdfDictionary2 && pdfDictionary2.ContainsKey("Encoding") && PdfCrossTable.Dereference(pdfDictionary2["Encoding"]) is PdfDictionary pdfDictionary3 && pdfDictionary3.ContainsKey("PDFDocEncoding") && PdfCrossTable.Dereference(pdfDictionary3["PDFDocEncoding"]) is PdfDictionary pdfDictionary4 && pdfDictionary4.ContainsKey("Differences"))
			{
				PdfDictionary pdfDictionary5 = new PdfDictionary();
				pdfDictionary5["Differences"] = pdfDictionary4["Differences"];
				PdfDictionary pdfDictionary6 = new PdfDictionary();
				pdfDictionary6["Subtype"] = new PdfName("Type1");
				pdfDictionary6["Encoding"] = new PdfReferenceHolder(pdfDictionary5);
				fontStructure = new FontStructure(pdfDictionary6);
				if (fontStructure != null && fontStructure.DifferencesDictionary.Count > 0)
				{
					CrossTable.m_pdfDocumentEncoding = pdfDictionary6;
					return text = ReplaceNotUsedCharacters(text, fontStructure.DifferencesDictionary, fontStructure);
				}
			}
			if (!flag && value != null && font != null && font.Items != null && font.Items.Count > 0)
			{
				foreach (KeyValuePair<PdfName, IPdfPrimitive> item in font.Items)
				{
					fontStructure = new FontStructure(PdfCrossTable.Dereference(item.Value) as PdfDictionary);
					if (fontStructure != null && fontStructure.DifferencesDictionary.Count > 0)
					{
						text = ReplaceNotUsedCharacters(text, fontStructure.DifferencesDictionary, fontStructure);
					}
				}
			}
		}
		return text;
	}

	internal void GetEncodedFontDictionary(PdfDictionary fieldDictionary, out PdfDictionary font)
	{
		PdfArray pdfArray = null;
		font = null;
		if (!fieldDictionary.ContainsKey("AP") && fieldDictionary.ContainsKey("Kids"))
		{
			pdfArray = PdfCrossTable.Dereference(fieldDictionary["Kids"]) as PdfArray;
		}
		if (!fieldDictionary.ContainsKey("AP") && pdfArray == null)
		{
			return;
		}
		PdfDictionary pdfDictionary = null;
		if (pdfArray != null && pdfArray.Count > 0)
		{
			if (PdfCrossTable.Dereference(pdfArray[0]) is PdfDictionary pdfDictionary2 && pdfDictionary2.ContainsKey("AP"))
			{
				pdfDictionary = PdfCrossTable.Dereference(pdfDictionary2["AP"]) as PdfDictionary;
			}
		}
		else
		{
			pdfDictionary = PdfCrossTable.Dereference(base.Dictionary["AP"]) as PdfDictionary;
		}
		if (pdfDictionary != null && pdfDictionary.ContainsKey("N") && PdfCrossTable.Dereference(pdfDictionary["N"]) is PdfDictionary pdfDictionary3 && pdfDictionary3.ContainsKey("Resources") && PdfCrossTable.Dereference(pdfDictionary3["Resources"]) is PdfDictionary pdfDictionary4 && pdfDictionary4.ContainsKey("Font"))
		{
			font = PdfCrossTable.Dereference(pdfDictionary4["Font"]) as PdfDictionary;
		}
	}
}
