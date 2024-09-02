using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using DocGen.Drawing;
using DocGen.Pdf.Interactive;
using DocGen.Pdf.IO;
using DocGen.Pdf.Primitives;
using DocGen.Pdf.Security;
using DocGen.Pdf.Xfa;

namespace DocGen.Pdf.Parsing;

public class PdfLoadedForm : PdfForm
{
	private class NodeInfo
	{
		private int m_count;

		private PdfArray m_fields;

		internal PdfArray Fields
		{
			get
			{
				return m_fields;
			}
			set
			{
				m_fields = value;
			}
		}

		internal int Count
		{
			get
			{
				return m_count;
			}
			set
			{
				m_count = value;
			}
		}

		internal NodeInfo(PdfArray fields, int count)
		{
			m_fields = fields;
			m_count = count;
		}
	}

	private PdfLoadedFormFieldCollection m_fields;

	private ExportFormSettings m_formSettings = new ExportFormSettings();

	private ImportFormSettings m_settings = new ImportFormSettings();

	private PdfFormFieldsTabOrder pdfFormFieldsTabOrder;

	private PdfCrossTable m_crossTable;

	private List<PdfDictionary> m_terminalFields = new List<PdfDictionary>();

	private bool m_isModified;

	private bool m_isXFAForm;

	private bool m_hasFieldFontRetrieved;

	internal bool isUR3;

	private PdfLoadedXfaForm m_loadedXfa;

	internal Dictionary<string, List<PdfDictionary>> m_widgetDictionary;

	private bool m_formHasKids;

	private bool m_exportEmptyFields;

	private Dictionary<string, PdfDictionary> m_fdfFields;

	private Dictionary<string, List<string>> m_xmlFields;

	private Dictionary<string, List<string>> m_xdfdFields;

	private string uniquekey = string.Empty;

	private Dictionary<string, string> fdfRichTextTable = new Dictionary<string, string>();

	private Dictionary<string, string> m_xfdfRichText = new Dictionary<string, string>();

	internal List<string> m_terminalAddedFieldsNames = new List<string>();

	private bool isJsonFormat;

	private string m_jsonDelimiters = "()<>[]{}/%,\":";

	internal bool isEmpty = true;

	private bool isSignature;

	private List<string> selectedList;

	private string currentFieldName = string.Empty;

	private string previousFieldName = string.Empty;

	internal PdfLoadedXfaForm LoadedXfa
	{
		get
		{
			if (m_loadedXfa == null)
			{
				m_loadedXfa = new PdfLoadedXfaForm();
			}
			return m_loadedXfa;
		}
		set
		{
			m_loadedXfa = value;
		}
	}

	public new PdfLoadedFormFieldCollection Fields
	{
		get
		{
			if (m_fields == null)
			{
				m_fields = new PdfLoadedFormFieldCollection(this);
			}
			return m_fields;
		}
	}

	public bool EnableXfaFormFill
	{
		get
		{
			return m_enableXfaFormfill;
		}
		set
		{
			m_enableXfaFormfill = value;
			if (value && m_loadedXfa == null && m_isXFAForm)
			{
				m_loadedXfa = LoadedXfa;
				m_loadedXfa.Load(CrossTable.Document.Catalog);
				LoadedXfa = m_loadedXfa;
			}
		}
	}

	public bool ExportEmptyFields
	{
		get
		{
			return m_exportEmptyFields;
		}
		set
		{
			m_exportEmptyFields = value;
		}
	}

	public override bool ReadOnly
	{
		get
		{
			return base.ReadOnly;
		}
		set
		{
			base.ReadOnly = value;
			foreach (PdfField field in Fields)
			{
				field.ReadOnly = value;
			}
		}
	}

	internal override SignatureFlags SignatureFlags
	{
		get
		{
			return base.SignatureFlags;
		}
		set
		{
			base.SignatureFlags = value;
			IsModified = true;
			Dictionary.SetNumber("SigFlags", (int)value);
		}
	}

	internal override bool NeedAppearances
	{
		get
		{
			return base.NeedAppearances;
		}
		set
		{
			base.NeedAppearances = value;
			IsModified = true;
		}
	}

	internal override PdfResources Resources
	{
		get
		{
			return base.Resources;
		}
		set
		{
			base.Resources = value;
			IsModified = true;
			Dictionary.SetProperty("DR", value);
		}
	}

	internal bool IsModified
	{
		get
		{
			return m_isModified;
		}
		set
		{
			m_isModified = value;
		}
	}

	internal PdfCrossTable CrossTable => m_crossTable;

	internal List<PdfDictionary> TerminalFields
	{
		get
		{
			return m_terminalFields;
		}
		set
		{
			m_terminalFields = value;
		}
	}

	internal bool IsXFAForm
	{
		get
		{
			return m_isXFAForm;
		}
		set
		{
			m_isXFAForm = value;
		}
	}

	internal bool IsFormContainsKids
	{
		get
		{
			return m_formHasKids;
		}
		set
		{
			m_formHasKids = value;
		}
	}

	internal PdfLoadedForm(PdfDictionary formDictionary, PdfCrossTable crossTable)
		: this(crossTable)
	{
		Initialize(formDictionary, crossTable);
	}

	internal PdfLoadedForm(PdfCrossTable crossTable)
	{
		m_crossTable = crossTable;
		if (crossTable.DocumentCatalog != null && crossTable.DocumentCatalog.ContainsKey("AcroForm") && PdfCrossTable.Dereference(crossTable.DocumentCatalog["AcroForm"]) is PdfDictionary pdfDictionary && pdfDictionary.ContainsKey("NeedAppearances"))
		{
			Dictionary.SetBoolean("NeedAppearances", NeedAppearances);
		}
		CrossTable.Document.Catalog.BeginSave += Dictionary_BeginSave;
		CrossTable.Document.Catalog.Modify();
		if (crossTable.Document.Catalog.ContainsKey("Perms"))
		{
			CheckPerms(crossTable.Document.Catalog);
		}
	}

	internal PdfField GetField(string nodeName)
	{
		foreach (PdfField field in Fields)
		{
			if (field.Name.Replace("\\", string.Empty) == nodeName)
			{
				return field;
			}
		}
		return null;
	}

	private void CheckPerms(PdfCatalog catalog)
	{
		PdfDictionary pdfDictionary = null;
		if (catalog["Perms"] is PdfDictionary)
		{
			pdfDictionary = catalog["Perms"] as PdfDictionary;
		}
		else if (catalog["Perms"] is PdfReferenceHolder)
		{
			pdfDictionary = (catalog["Perms"] as PdfReferenceHolder).Object as PdfDictionary;
		}
		if (pdfDictionary.ContainsKey("UR3"))
		{
			isUR3 = true;
		}
	}

	private void Initialize(PdfDictionary formDictionary, PdfCrossTable crossTable)
	{
		if (formDictionary == null)
		{
			throw new ArgumentNullException("dictionary");
		}
		if (crossTable == null)
		{
			throw new ArgumentNullException("crossTable");
		}
		Dictionary = formDictionary;
		if (Dictionary.ContainsKey("XFA"))
		{
			m_isXFAForm = true;
		}
		CreateFields();
		if (Dictionary.ContainsKey("NeedAppearances"))
		{
			PdfBoolean pdfBoolean = m_crossTable.GetObject(Dictionary["NeedAppearances"]) as PdfBoolean;
			base.NeedAppearances = pdfBoolean.Value;
			base.SetAppearanceDictionary = false;
		}
		else
		{
			base.SetAppearanceDictionary = false;
		}
		if (Dictionary.ContainsKey("SigFlags"))
		{
			PdfNumber pdfNumber = m_crossTable.GetObject(Dictionary["SigFlags"]) as PdfNumber;
			base.SignatureFlags = (SignatureFlags)pdfNumber.IntValue;
		}
		if (Dictionary.ContainsKey("DR") && PdfCrossTable.Dereference(Dictionary["DR"]) is PdfDictionary baseDictionary)
		{
			PdfResources resources = (Resources = new PdfResources(baseDictionary));
			base.Resources = resources;
		}
	}

	private void CreateFields()
	{
		PdfArray pdfArray = null;
		if (Dictionary.ContainsKey("Fields"))
		{
			pdfArray = m_crossTable.GetObject(Dictionary["Fields"]) as PdfArray;
		}
		int i = 0;
		Stack<NodeInfo> stack = new Stack<NodeInfo>();
		while (pdfArray != null)
		{
			for (; i < pdfArray.Count; i++)
			{
				PdfDictionary pdfDictionary = m_crossTable.GetObject(pdfArray[i]) as PdfDictionary;
				PdfArray pdfArray2 = null;
				if (pdfDictionary != null && pdfDictionary.ContainsKey("Kids"))
				{
					pdfArray2 = m_crossTable.GetObject(pdfDictionary["Kids"]) as PdfArray;
					if (pdfArray2 != null)
					{
						for (int j = 0; j < pdfArray2.Count; j++)
						{
							if (PdfCrossTable.Dereference(pdfArray2[j]) is PdfDictionary pdfDictionary2 && !pdfDictionary2.ContainsKey("Parent"))
							{
								pdfDictionary2["Parent"] = new PdfReferenceHolder(pdfDictionary);
							}
						}
					}
				}
				if (pdfArray2 == null)
				{
					if (pdfDictionary == null || m_terminalFields.Contains(pdfDictionary))
					{
						continue;
					}
					m_terminalFields.Add(pdfDictionary);
					if (pdfDictionary.ContainsKey("T"))
					{
						string value = (pdfDictionary.Items[new PdfName("T")] as PdfString).Value;
						if (!m_terminalAddedFieldsNames.Contains(value))
						{
							m_terminalAddedFieldsNames.Add(value);
						}
					}
					continue;
				}
				if (!pdfDictionary.ContainsKey("FT") || IsNode(pdfArray2))
				{
					NodeInfo item = new NodeInfo(pdfArray, i);
					stack.Push(item);
					IsFormContainsKids = true;
					i = -1;
					pdfArray = pdfArray2;
					continue;
				}
				m_terminalFields.Add(pdfDictionary);
				if (pdfDictionary.ContainsKey("T"))
				{
					string value2 = (pdfDictionary.Items[new PdfName("T")] as PdfString).Value;
					if (!m_terminalAddedFieldsNames.Contains(value2))
					{
						m_terminalAddedFieldsNames.Add(value2);
					}
				}
			}
			if (stack.Count != 0)
			{
				NodeInfo nodeInfo = stack.Pop();
				pdfArray = nodeInfo.Fields;
				i = nodeInfo.Count + 1;
				continue;
			}
			break;
		}
	}

	private bool IsNode(PdfArray kids)
	{
		bool result = false;
		if (kids.Count >= 1 && m_crossTable.GetObject(kids[0]) is PdfDictionary pdfDictionary && pdfDictionary.ContainsKey("Subtype") && (m_crossTable.GetObject(pdfDictionary["Subtype"]) as PdfName).Value != "Widget")
		{
			result = true;
		}
		return result;
	}

	public void ExportData(Stream stream, DataFormat dataFormat, string formName)
	{
		if (dataFormat == DataFormat.Xml)
		{
			ExportDataXML(stream);
		}
		if (dataFormat == DataFormat.Fdf)
		{
			ExportDataFDF(stream, formName);
		}
		if (dataFormat == DataFormat.XFdf)
		{
			ExportDataXFDF(stream, formName);
		}
		if (dataFormat == DataFormat.Json)
		{
			isJsonFormat = true;
			ExportDataJSON(stream);
			isJsonFormat = false;
		}
	}

	public void ExportData(Stream stream, ExportFormSettings settings)
	{
		m_formSettings = settings;
		m_formSettings.AsPerSpecification = true;
		ExportData(stream, m_formSettings.DataFormat, m_formSettings.FormName);
	}

	public void FlattenFields()
	{
		bool flatten = base.Flatten;
		base.Flatten = true;
		FlttenFormFields();
		if (m_fields.Count == 0 && !(CrossTable.Document as PdfLoadedDocument).m_isXfaDocument)
		{
			int num = CrossTable.PdfObjects.IndexOf(Dictionary);
			Dictionary.Clear();
			if (num != -1)
			{
				CrossTable.PdfObjects.Remove(num);
			}
		}
		else if (base.SetAppearanceDictionary && (Dictionary.ContainsKey("NeedAppearances") || (base.SignatureFlags == SignatureFlags.None && NeedAppearances)))
		{
			Dictionary.SetBoolean("NeedAppearances", NeedAppearances);
		}
		base.Flatten = flatten;
	}

	private void ExportDataXFDF(Stream stream, string formName)
	{
		XFdfDocument xFdfDocument = new XFdfDocument(formName);
		uniquekey = Guid.NewGuid().ToString();
		for (int i = 0; i < Fields.Count; i++)
		{
			PdfLoadedField pdfLoadedField = (PdfLoadedField)Fields[i];
			if (!pdfLoadedField.Export)
			{
				continue;
			}
			pdfLoadedField.ExportEmptyField = ExportEmptyFields;
			PdfName pdfName = PdfLoadedField.GetValue(pdfLoadedField.Dictionary, pdfLoadedField.CrossTable, "FT", inheritable: true) as PdfName;
			PdfDictionary font = null;
			string text = pdfLoadedField.Name;
			pdfLoadedField.GetEncodedFontDictionary(pdfLoadedField.Dictionary, out font);
			if (font != null)
			{
				text = pdfLoadedField.UpdateEncodedValue(text, font);
			}
			if (!(pdfName != null))
			{
				continue;
			}
			switch (pdfName.Value)
			{
			case "Tx":
			{
				PdfString pdfString4 = PdfLoadedField.GetValue(pdfLoadedField.Dictionary, pdfLoadedField.CrossTable, "V", inheritable: true) as PdfString;
				if (m_formSettings.AsPerSpecification)
				{
					if (pdfLoadedField.Dictionary.ContainsKey("RV"))
					{
						if (PdfLoadedField.GetValue(pdfLoadedField.Dictionary, pdfLoadedField.CrossTable, "RV", inheritable: true) is PdfString pdfString5)
						{
							pdfString5.Value += uniquekey;
							xFdfDocument.SetFields(text, pdfString5.Value, uniquekey);
						}
					}
					else if (PdfLoadedField.GetValue(pdfLoadedField.Dictionary, pdfLoadedField.CrossTable, "V", inheritable: true) is PdfString pdfString6)
					{
						pdfString6.Value = pdfLoadedField.UpdateEncodedValue(pdfString6.Value, font);
						string value5 = pdfString6.Value;
						PdfLoadedTextBoxField pdfLoadedTextBoxField = pdfLoadedField as PdfLoadedTextBoxField;
						string fieldvalue2 = value5;
						if (pdfLoadedTextBoxField != null && pdfLoadedTextBoxField.Multiline)
						{
							value5 = value5.Replace("\n", "");
							value5 = value5.Replace("\r", "\r\n");
							fieldvalue2 = value5;
						}
						xFdfDocument.SetFields(text, fieldvalue2);
					}
				}
				else if (pdfString4 != null)
				{
					pdfString4.Value = pdfLoadedField.UpdateEncodedValue(pdfString4.Value, font);
					xFdfDocument.SetFields(text, pdfString4.Value);
				}
				else if (ExportEmptyFields)
				{
					xFdfDocument.SetFields(text, "");
				}
				break;
			}
			case "Ch":
			{
				if (m_formSettings.AsPerSpecification)
				{
					string empty = string.Empty;
					if (pdfLoadedField.GetType().Name == "PdfLoadedListBoxField")
					{
						if (PdfLoadedField.GetValue(pdfLoadedField.Dictionary, pdfLoadedField.CrossTable, "V", inheritable: true) is PdfArray fieldvalue)
						{
							xFdfDocument.SetFields(text, fieldvalue);
							break;
						}
						PdfString pdfString2 = PdfLoadedField.GetValue(pdfLoadedField.Dictionary, pdfLoadedField.CrossTable, "V", inheritable: true) as PdfString;
						if (pdfString2 != null)
						{
							pdfString2.Value = pdfLoadedField.UpdateEncodedValue(pdfString2.Value, font);
							xFdfDocument.SetFields(text, pdfString2.Value);
						}
						else if (pdfString2 == null && pdfLoadedField.Dictionary.ContainsKey("I"))
						{
							empty = (pdfLoadedField as PdfLoadedListBoxField).SelectedValue[0];
							empty = pdfLoadedField.UpdateEncodedValue(empty, font);
							xFdfDocument.SetFields(text, empty);
						}
						break;
					}
					PdfName pdfName3 = PdfLoadedField.GetValue(pdfLoadedField.Dictionary, pdfLoadedField.CrossTable, "V", inheritable: true) as PdfName;
					if (pdfName3 != null)
					{
						xFdfDocument.SetFields(text, pdfName3.Value);
						break;
					}
					PdfString pdfString3 = PdfLoadedField.GetValue(pdfLoadedField.Dictionary, pdfLoadedField.CrossTable, "V", inheritable: true) as PdfString;
					if (pdfString3 != null)
					{
						pdfString3.Value = pdfLoadedField.UpdateEncodedValue(pdfString3.Value, font);
						xFdfDocument.SetFields(text, pdfString3.Value);
					}
					else if (pdfString3 == null && pdfLoadedField.Dictionary.ContainsKey("I"))
					{
						empty = (pdfLoadedField as PdfLoadedComboBoxField).SelectedValue;
						empty = pdfLoadedField.UpdateEncodedValue(empty, font);
						xFdfDocument.SetFields(text, empty);
					}
					break;
				}
				IPdfPrimitive value2 = PdfLoadedField.GetValue(pdfLoadedField.Dictionary, pdfLoadedField.CrossTable, "V", inheritable: true);
				string value3 = null;
				object obj2 = null;
				string[] array = null;
				if (value2 != null)
				{
					obj2 = GetExportValue(value2);
					if (obj2 is string)
					{
						value3 = obj2 as string;
					}
					else
					{
						array = (obj2 as List<string>).ToArray();
					}
				}
				if (pdfLoadedField.GetType().Name == "PdfLoadedListBoxField")
				{
					if (value2 == null && pdfLoadedField.Dictionary.ContainsKey("I"))
					{
						if ((pdfLoadedField as PdfLoadedListBoxField).SelectedValue.Length > 1)
						{
							array = (pdfLoadedField as PdfLoadedListBoxField).SelectedValue;
						}
						else
						{
							value3 = (pdfLoadedField as PdfLoadedListBoxField).SelectedValue[0];
						}
					}
					if (!string.IsNullOrEmpty(value3))
					{
						value3 = pdfLoadedField.UpdateEncodedValue(value3, font);
						xFdfDocument.SetFields(text, value3);
					}
					else if (array.Length >= 1)
					{
						int num = 0;
						PdfArray pdfArray2 = new PdfArray();
						string[] array2 = array;
						foreach (string value4 in array2)
						{
							pdfArray2.Insert(num, new PdfString(pdfLoadedField.UpdateEncodedValue(value4, font)));
							num++;
						}
						xFdfDocument.SetFields(text, pdfArray2);
					}
					else if (ExportEmptyFields)
					{
						xFdfDocument.SetFields(text, "");
					}
				}
				else
				{
					if (value2 == null && pdfLoadedField.Dictionary.ContainsKey("I"))
					{
						value3 = (pdfLoadedField as PdfLoadedComboBoxField).SelectedValue;
					}
					if (!string.IsNullOrEmpty(value3))
					{
						value3 = pdfLoadedField.UpdateEncodedValue(value3, font);
						xFdfDocument.SetFields(text, value3);
					}
					else if (ExportEmptyFields)
					{
						xFdfDocument.SetFields(text, "");
					}
				}
				break;
			}
			case "Btn":
			{
				IPdfPrimitive value = PdfLoadedField.GetValue(pdfLoadedField.Dictionary, pdfLoadedField.CrossTable, "V", inheritable: true);
				PdfLoadedRadioButtonListField pdfLoadedRadioButtonListField = null;
				if (value != null)
				{
					string text2 = pdfLoadedField.GetExportValue(pdfLoadedField, value);
					if (!string.IsNullOrEmpty(text2))
					{
						if (m_formSettings.AsPerSpecification)
						{
							text2 = HexToString(text2);
						}
						if (pdfLoadedField is PdfLoadedRadioButtonListField)
						{
							pdfLoadedRadioButtonListField = pdfLoadedField as PdfLoadedRadioButtonListField;
						}
						if (!pdfLoadedField.Dictionary.ContainsKey("Opt") || (pdfLoadedRadioButtonListField != null && pdfLoadedRadioButtonListField.SelectedIndex == -1))
						{
							text2 = pdfLoadedField.UpdateEncodedValue(text2, font);
							xFdfDocument.SetFields(text, text2);
						}
						else
						{
							if (pdfLoadedField.Dictionary == null || !pdfLoadedField.Dictionary.ContainsKey("Opt"))
							{
								break;
							}
							PdfArray pdfArray = PdfCrossTable.Dereference(pdfLoadedField.Dictionary["Opt"]) as PdfArray;
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
									text2 = pdfLoadedField.UpdateEncodedValue(text2, font);
									xFdfDocument.SetFields(text, text2);
								}
							}
							catch
							{
							}
						}
					}
					else if (pdfLoadedField is PdfLoadedRadioButtonListField || pdfLoadedField is PdfLoadedCheckBoxField)
					{
						if (ExportEmptyFields)
						{
							xFdfDocument.SetFields(text, "");
						}
						else
						{
							xFdfDocument.SetFields(text, "Off");
						}
					}
					break;
				}
				if (pdfLoadedField is PdfLoadedRadioButtonListField)
				{
					xFdfDocument.SetFields(text, pdfLoadedField.GetAppearanceStateValue(pdfLoadedField));
					break;
				}
				PdfDictionary widgetAnnotation = pdfLoadedField.GetWidgetAnnotation(pdfLoadedField.Dictionary, pdfLoadedField.CrossTable);
				if (widgetAnnotation != null)
				{
					PdfName pdfName2 = widgetAnnotation["AS"] as PdfName;
					if (pdfName2 != null)
					{
						xFdfDocument.SetFields(text, pdfName2.Value);
					}
					else if (ExportEmptyFields)
					{
						xFdfDocument.SetFields(text, "");
					}
				}
				break;
			}
			}
		}
		if (m_formSettings.AsPerSpecification)
		{
			xFdfDocument.trailerId = CrossTable.Trailer["ID"];
			xFdfDocument.Save(stream, m_formSettings.AsPerSpecification);
		}
		else
		{
			xFdfDocument.Save(stream);
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

	internal PdfLoadedXfaField GetXfaField(string fieldName)
	{
		return LoadedXfa.TryGetFieldByCompleteName(fieldName);
	}

	private void ExportDataFDF(Stream stream, string formName)
	{
		new BinaryWriter(stream);
		PdfString pdfString = null;
		StringBuilder stringBuilder = null;
		PdfString pdfString2 = null;
		byte[] array = null;
		int objectid = 1;
		PdfString pdfString3 = null;
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		Dictionary<string, object> map = new Dictionary<string, object>();
		if (m_formSettings.AsPerSpecification)
		{
			pdfString = new PdfString("%FDF-1.2\r");
			pdfString.Encode = PdfString.ForceEncoding.ASCII;
			array = Encoding.GetEncoding("UTF-8").GetBytes(pdfString.Value);
			stream.Write(array, 0, array.Length);
			PdfString pdfString4 = new PdfString("%âãÏÓ\r\n");
			pdfString.Encode = PdfString.ForceEncoding.ASCII;
			array = Encoding.GetEncoding("UTF-8").GetBytes(pdfString4.Value);
			stream.Write(array, 0, array.Length);
			stringBuilder = new StringBuilder();
			stringBuilder.AppendFormat("1 0 obj\r");
			stringBuilder.AppendFormat("<</FDF<</F(");
			pdfString2 = new PdfString(stringBuilder.ToString());
			pdfString2.Encode = PdfString.ForceEncoding.ASCII;
			array = Encoding.GetEncoding("UTF-8").GetBytes(pdfString2.Value);
			stream.Write(array, 0, array.Length);
			pdfString3 = new PdfString(formName);
			pdfString3.Encode = PdfString.ForceEncoding.ASCII;
			array = Encoding.GetEncoding("UTF-8").GetBytes(pdfString3.Value);
			stream.Write(array, 0, array.Length);
			stringBuilder = new StringBuilder();
			stringBuilder.AppendFormat(")");
			pdfString2 = new PdfString(stringBuilder.ToString());
			pdfString2.Encode = PdfString.ForceEncoding.ASCII;
			array = Encoding.GetEncoding("UTF-8").GetBytes(pdfString2.Value);
			stream.Write(array, 0, array.Length);
		}
		else
		{
			pdfString = new PdfString("%FDF-1.2\n");
			pdfString.Encode = PdfString.ForceEncoding.ASCII;
			array = Encoding.GetEncoding("UTF-8").GetBytes(pdfString.Value);
			stream.Write(array, 0, array.Length);
		}
		for (int i = 0; i < Fields.Count; i++)
		{
			PdfLoadedField pdfLoadedField = (PdfLoadedField)Fields[i];
			pdfLoadedField.ExportEmptyField = ExportEmptyFields;
			if (pdfLoadedField.Export)
			{
				if (m_formSettings.AsPerSpecification)
				{
					pdfLoadedField.ExportField(dictionary);
				}
				else
				{
					pdfLoadedField.ExportField(stream, ref objectid);
				}
			}
		}
		if (m_formSettings.AsPerSpecification)
		{
			if (dictionary.Count > 0)
			{
				map = GetElements(dictionary);
			}
			PdfArray pdfArray = GroupFieldNames(map);
			PdfWriter writer = new PdfWriter(stream);
			if (pdfArray.Count > 0)
			{
				stringBuilder = new StringBuilder();
				stringBuilder.AppendFormat("/Fields");
				pdfString2 = new PdfString(stringBuilder.ToString());
				pdfString2.Encode = PdfString.ForceEncoding.ASCII;
				array = Encoding.GetEncoding("UTF-8").GetBytes(pdfString2.Value);
				stream.Write(array, 0, array.Length);
				AppendArrayElements(pdfArray, writer);
			}
			stringBuilder = new StringBuilder();
			stringBuilder.Append("/ID[");
			if (CrossTable.Trailer["ID"] is PdfArray pdfArray2)
			{
				for (int j = 0; j < pdfArray2.Count; j++)
				{
					pdfString2 = new PdfString((pdfArray2[j] as PdfString).Value);
					pdfString2.Encode = PdfString.ForceEncoding.ASCII;
					byte[] bytes = pdfString2.Bytes;
					stringBuilder.AppendFormat("<{0}>", PdfString.BytesToHex(bytes));
				}
			}
			pdfString2 = new PdfString(stringBuilder.ToString());
			pdfString2.Encode = PdfString.ForceEncoding.ASCII;
			array = Encoding.GetEncoding("UTF-8").GetBytes(pdfString2.Value);
			stream.Write(array, 0, array.Length);
			stream.Flush();
			stringBuilder = new StringBuilder();
			stringBuilder.Append("]");
			stringBuilder.AppendFormat("/UF({0})>>", formName);
			stringBuilder.Append("/Type/Catalog>>\r");
			stringBuilder.Append("endobj\r");
			stringBuilder.Append("trailer\r\n");
			stringBuilder.AppendFormat("<</Root 1 0 R>>\r\n");
			stringBuilder.AppendFormat("%%EOF\r\n");
			pdfString2 = new PdfString(stringBuilder.ToString());
			pdfString2.Encode = PdfString.ForceEncoding.ASCII;
			array = Encoding.GetEncoding("UTF-8").GetBytes(pdfString2.Value);
			stream.Write(array, 0, array.Length);
			stream.Flush();
			return;
		}
		stringBuilder = new StringBuilder();
		pdfString3 = new PdfString(formName);
		pdfString3.Encode = PdfString.ForceEncoding.ASCII;
		array = Encoding.GetEncoding("UTF-8").GetBytes(pdfString3.Value);
		stringBuilder.AppendFormat("{0} 0 obj<</F <{1}>  /Fields [", objectid, PdfString.BytesToHex(array));
		for (int k = 0; k < Fields.Count; k++)
		{
			PdfLoadedField pdfLoadedField2 = (PdfLoadedField)Fields[k];
			if (pdfLoadedField2.Export && pdfLoadedField2.ObjectID != 0)
			{
				stringBuilder.AppendFormat("{0} 0 R ", pdfLoadedField2.ObjectID);
			}
		}
		stringBuilder.Append("]>>endobj\n");
		stringBuilder.AppendFormat("{0} 0 obj<</Version /1.4 /FDF {1} 0 R>>endobj\n", objectid + 1, objectid);
		stringBuilder.AppendFormat("trailer\n<</Root {0} 0 R>>\n", objectid + 1);
		pdfString2 = new PdfString(stringBuilder.ToString());
		pdfString2.Encode = PdfString.ForceEncoding.ASCII;
		array = Encoding.GetEncoding("UTF-8").GetBytes(pdfString2.Value);
		stream.Write(array, 0, array.Length);
		stream.Flush();
	}

	private void AppendArrayElements(PdfArray array, PdfWriter writer)
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
				AppendElement(pdfPrimitive, writer);
			}
		}
		writer.Write("]");
	}

	private void AppendElement(IPdfPrimitive element, PdfWriter writer)
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
			byte[] preamble = Encoding.BigEndianUnicode.GetPreamble();
			string value = (element as PdfString).Value;
			bool num = NonAsciiCheck(value);
			byte[] array = null;
			array = Encoding.GetEncoding("UTF-8").GetBytes("(");
			writer.m_stream.Write(array, 0, array.Length);
			byte[] bytes;
			if (num)
			{
				bytes = Encoding.BigEndianUnicode.GetBytes(value);
				bytes = EscapeSymbols(bytes);
			}
			else
			{
				bytes = Encoding.GetEncoding("UTF-8").GetBytes(value);
				bytes = PdfString.EscapeSymbols(bytes, isFormField: true);
			}
			if (num)
			{
				writer.m_stream.Write(preamble, 0, preamble.Length);
			}
			writer.m_stream.Write(bytes, 0, bytes.Length);
			array = Encoding.GetEncoding("UTF-8").GetBytes(")");
			writer.m_stream.Write(array, 0, array.Length);
		}
		else if (element is PdfBoolean)
		{
			writer.Write((element as PdfBoolean).Value ? "true" : "false");
		}
		else if (element is PdfArray)
		{
			AppendArrayElements(element as PdfArray, writer);
		}
		else if (element is PdfDictionary)
		{
			writer.Write("<<");
			GetEntriesInDictionary(element as PdfDictionary, writer);
			writer.Write(">>");
		}
	}

	private void GetEntriesInDictionary(PdfDictionary dictionary, PdfWriter writer)
	{
		foreach (PdfName key in dictionary.Keys)
		{
			writer.Write(key.ToString());
			IPdfPrimitive pdfPrimitive = dictionary[key];
			if (pdfPrimitive is PdfString)
			{
				byte[] preamble = Encoding.BigEndianUnicode.GetPreamble();
				string value = (pdfPrimitive as PdfString).Value;
				bool num = NonAsciiCheck(value);
				byte[] array = null;
				array = Encoding.GetEncoding("UTF-8").GetBytes("(");
				writer.m_stream.Write(array, 0, array.Length);
				byte[] bytes;
				if (num)
				{
					bytes = Encoding.BigEndianUnicode.GetBytes(value);
					bytes = EscapeSymbols(bytes);
				}
				else
				{
					bytes = Encoding.GetEncoding("UTF-8").GetBytes(value);
					bytes = PdfString.EscapeSymbols(bytes, isFormField: true);
				}
				if (num)
				{
					writer.m_stream.Write(preamble, 0, preamble.Length);
				}
				writer.m_stream.Write(bytes, 0, bytes.Length);
				array = Encoding.GetEncoding("UTF-8").GetBytes(")");
				writer.m_stream.Write(array, 0, array.Length);
			}
			else if (pdfPrimitive is PdfName)
			{
				writer.Write((pdfPrimitive as PdfName).ToString());
			}
			else if (pdfPrimitive is PdfArray)
			{
				AppendArrayElements(pdfPrimitive as PdfArray, writer);
			}
			else if (pdfPrimitive is PdfNumber)
			{
				writer.Write(" " + (pdfPrimitive as PdfNumber).FloatValue);
			}
			else if (pdfPrimitive is PdfBoolean)
			{
				writer.Write(" " + ((pdfPrimitive as PdfBoolean).Value ? "true" : "false"));
			}
			else if (pdfPrimitive is PdfDictionary)
			{
				writer.Write("<<");
				GetEntriesInDictionary(pdfPrimitive as PdfDictionary, writer);
				writer.Write(">>");
			}
		}
	}

	private bool NonAsciiCheck(string value)
	{
		for (int i = 0; i < value.Length; i++)
		{
			if (value[i] > 'ÿ')
			{
				return true;
			}
		}
		return false;
	}

	internal byte[] EscapeSymbols(byte[] data)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		MemoryStream memoryStream = new MemoryStream();
		int i = 0;
		for (int num = data.Length; i < num; i++)
		{
			byte b = data[i];
			switch (b)
			{
			case 40:
			case 41:
				memoryStream.WriteByte(92);
				memoryStream.WriteByte(b);
				break;
			case 13:
				memoryStream.WriteByte(92);
				memoryStream.WriteByte(114);
				break;
			case 10:
				memoryStream.WriteByte(92);
				memoryStream.WriteByte(110);
				break;
			case 92:
				memoryStream.WriteByte(92);
				memoryStream.WriteByte(b);
				break;
			default:
				memoryStream.WriteByte(b);
				break;
			}
		}
		byte[] result = PdfStream.StreamToBytes(memoryStream);
		memoryStream.Dispose();
		return result;
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
			text += (char)num;
		}
		return text;
	}

	private PdfArray GroupFieldNames(Dictionary<string, object> map)
	{
		PdfArray pdfArray = new PdfArray();
		foreach (KeyValuePair<string, object> item in map)
		{
			string key = item.Key;
			object obj = item.Value;
			PdfDictionary pdfDictionary = new PdfDictionary();
			pdfDictionary.SetProperty("T", new PdfString(key));
			if (obj is Dictionary<string, object>)
			{
				pdfDictionary.SetProperty("Kids", GroupFieldNames((Dictionary<string, object>)obj));
			}
			else
			{
				List<PdfString> list = obj as List<PdfString>;
				if (list != null && list.Count > 1)
				{
					PdfString primitive = list[0];
					PdfString primitive2 = list[1];
					pdfDictionary.SetProperty("RV", primitive);
					pdfDictionary.SetProperty("V", primitive2);
				}
				else
				{
					if (list != null)
					{
						obj = list[0];
					}
					if (obj is PdfString)
					{
						pdfDictionary.SetProperty("V", obj as PdfString);
					}
					if (obj is PdfName)
					{
						pdfDictionary.SetProperty("V", obj as PdfName);
					}
					if (obj is PdfArray)
					{
						pdfDictionary.SetProperty("V", obj as PdfArray);
					}
				}
			}
			pdfArray.Add(pdfDictionary);
		}
		return pdfArray;
	}

	internal Dictionary<string, object> GetElements(Dictionary<string, object> table)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		foreach (KeyValuePair<string, object> item in table)
		{
			object key = item.Key;
			object value = item.Value;
			Dictionary<string, object> dictionary2 = dictionary;
			if (key.ToString().Contains("."))
			{
				string[] array = key.ToString().Split('.');
				for (int i = 0; i < array.Length; i++)
				{
					if (dictionary2.ContainsKey(array[i]))
					{
						Dictionary<string, object> table2 = (Dictionary<string, object>)dictionary2[array[i]];
						GetElements(table2);
						dictionary2 = (Dictionary<string, object>)dictionary2[array[i]];
					}
					else if (i == array.Length - 1)
					{
						dictionary2.Add(array[i], value);
					}
					else
					{
						Dictionary<string, object> value2 = new Dictionary<string, object>();
						dictionary2.Add(array[i], value2);
						dictionary2 = (Dictionary<string, object>)dictionary2[array[i]];
					}
				}
			}
			else
			{
				dictionary2.Add(key as string, value);
			}
		}
		return dictionary;
	}

	internal void ExportDataXML(Stream stream)
	{
		XmlTextWriter xmlTextWriter = new XmlTextWriter(stream, new UTF8Encoding());
		xmlTextWriter.Formatting = Formatting.Indented;
		xmlTextWriter.WriteStartDocument();
		if (m_formSettings.AsPerSpecification)
		{
			xmlTextWriter.WriteStartElement("fields", "");
			xmlTextWriter.WriteAttributeString("xmlns", "xfdf", null, "http://ns.adobe.com/xfdf-transition/");
		}
		else
		{
			xmlTextWriter.WriteStartElement("Fields", "");
		}
		Dictionary<object, object> dictionary = new Dictionary<object, object>();
		Dictionary<object, object> dictionary2 = new Dictionary<object, object>();
		uniquekey = Guid.NewGuid().ToString();
		for (int i = 0; i < Fields.Count; i++)
		{
			PdfLoadedField pdfLoadedField = (PdfLoadedField)Fields[i];
			if (pdfLoadedField.Export)
			{
				pdfLoadedField.ExportEmptyField = ExportEmptyFields;
				if (m_formSettings.AsPerSpecification)
				{
					pdfLoadedField.ExportField(xmlTextWriter, dictionary, uniquekey);
				}
				else
				{
					pdfLoadedField.ExportField(xmlTextWriter);
				}
			}
		}
		if (m_formSettings.AsPerSpecification && dictionary.Count > 0)
		{
			dictionary2 = GetElements(dictionary);
			foreach (KeyValuePair<object, object> item in dictionary2)
			{
				string text = (string)item.Key;
				bool flag = false;
				if (text.EndsWith(uniquekey))
				{
					text = text.Remove(text.Length - uniquekey.Length, uniquekey.Length);
					flag = true;
				}
				object obj = dictionary2[item.Key.ToString()];
				if (obj is Dictionary<object, object>)
				{
					if (!XmlReader.IsName(text.ToString()) || text.ToString().Contains(":") || text.ToString().Contains(" "))
					{
						if (text.ToString().Contains(" ") && XmlReader.IsName(text.ToString().Replace(" ", "")) && !text.ToString().Contains(":"))
						{
							xmlTextWriter.WriteStartElement(text.ToString().Replace(" ", ""));
							if (!flag)
							{
								xmlTextWriter.WriteAttributeString("xfdf", "original", null, text.ToString());
							}
						}
						else
						{
							xmlTextWriter.WriteStartElement("group");
							if (!flag)
							{
								xmlTextWriter.WriteAttributeString("xfdf", "original", null, text);
							}
						}
					}
					else
					{
						xmlTextWriter.WriteStartElement(text.ToString());
					}
					if (flag)
					{
						xmlTextWriter.WriteAttributeString("xmlns", "xfa", null, "http://www.xfa.org/schema/xfa-data/1.0/");
						xmlTextWriter.WriteAttributeString("xfa", "contentType", null, "text/html");
						xmlTextWriter.WriteAttributeString("xfdf", "original", null, text);
					}
					Dictionary<object, object> value = (Dictionary<object, object>)obj;
					WriteFieldName(value, xmlTextWriter);
					xmlTextWriter.WriteEndElement();
					continue;
				}
				if (obj.GetType().Name == "PdfArray")
				{
					PdfArray obj2 = obj as PdfArray;
					if (text.ToString().Contains(" ") && XmlReader.IsName(text.ToString().Replace(" ", "")) && !text.ToString().Contains(":"))
					{
						xmlTextWriter.WriteStartElement(text.ToString().Replace(" ", ""));
						xmlTextWriter.WriteAttributeString("xfdf", "original", null, text.ToString());
					}
					else if (!XmlReader.IsName(text.ToString()) || text.ToString().Contains(":"))
					{
						xmlTextWriter.WriteStartElement("field");
						xmlTextWriter.WriteAttributeString("xfdf", "original", null, text.ToString());
					}
					else
					{
						xmlTextWriter.WriteStartElement(text.ToString());
					}
					foreach (PdfString item2 in obj2)
					{
						xmlTextWriter.WriteStartElement("value");
						xmlTextWriter.WriteString(item2.Value.ToString());
						xmlTextWriter.WriteEndElement();
					}
					xmlTextWriter.WriteEndElement();
					continue;
				}
				bool flag2 = false;
				if (!XmlReader.IsName(text.ToString()) || text.ToString().Contains(":"))
				{
					if (text.ToString().Contains(" ") && XmlReader.IsName(text.ToString().Replace(" ", "")) && !text.ToString().Contains(":"))
					{
						xmlTextWriter.WriteStartElement(text.ToString().Replace(" ", ""));
					}
					else
					{
						xmlTextWriter.WriteStartElement("field");
					}
					if (!flag)
					{
						xmlTextWriter.WriteAttributeString("xfdf", "original", null, text.ToString());
					}
				}
				else if (text.ToString().Contains(" "))
				{
					xmlTextWriter.WriteStartElement(text.ToString().Replace(" ", ""));
					if (!flag)
					{
						xmlTextWriter.WriteAttributeString("xfdf", "original", null, text.ToString());
					}
				}
				else
				{
					xmlTextWriter.WriteStartElement(text.ToString());
					flag2 = true;
				}
				if (flag)
				{
					xmlTextWriter.WriteAttributeString("xmlns", "xfa", null, "http://www.xfa.org/schema/xfa-data/1.0/");
					xmlTextWriter.WriteAttributeString("xfa", "contentType", null, "text/html");
					if (!flag2)
					{
						xmlTextWriter.WriteAttributeString("xfdf", "original", null, text);
					}
				}
				xmlTextWriter.WriteString(obj.ToString());
				xmlTextWriter.WriteEndElement();
			}
		}
		xmlTextWriter.WriteEndElement();
		xmlTextWriter.Flush();
	}

	private void WriteFieldName(Dictionary<object, object> value, XmlWriter textWriter)
	{
		foreach (KeyValuePair<object, object> item in value)
		{
			string text = item.Key.ToString();
			bool flag = false;
			if (text.EndsWith(uniquekey))
			{
				text = text.Remove(text.Length - uniquekey.Length, uniquekey.Length);
				flag = true;
			}
			if (item.Value is Dictionary<object, object>)
			{
				if (!XmlReader.IsName(text.ToString()) || text.ToString().Contains(":") || text.ToString().Contains(" "))
				{
					if (text.ToString().Contains(" ") && XmlReader.IsName(text.ToString().Replace(" ", "")) && !text.ToString().Contains(":"))
					{
						textWriter.WriteStartElement(text.ToString().Replace(" ", ""));
						if (!flag)
						{
							textWriter.WriteAttributeString("xfdf", "original", null, text.ToString());
						}
					}
					else
					{
						textWriter.WriteStartElement("group");
						if (!flag)
						{
							textWriter.WriteAttributeString("xfdf", "original", null, text);
						}
					}
				}
				else
				{
					textWriter.WriteStartElement(text.ToString());
				}
				if (flag)
				{
					textWriter.WriteAttributeString("xmlns", "xfa", null, "http://www.xfa.org/schema/xfa-data/1.0/");
					textWriter.WriteAttributeString("xfa", "contentType", null, "text/html");
					textWriter.WriteAttributeString("xfdf", "original", null, text);
				}
				Dictionary<object, object> value2 = (Dictionary<object, object>)item.Value;
				WriteFieldName(value2, textWriter);
				textWriter.WriteEndElement();
				continue;
			}
			if (item.Value.GetType().Name == "PdfArray")
			{
				PdfArray obj = item.Value as PdfArray;
				if (text.ToString().Contains(" ") && XmlReader.IsName(text.ToString().Replace(" ", "")) && !text.ToString().Contains(":"))
				{
					textWriter.WriteStartElement(text.ToString().Replace(" ", ""));
					textWriter.WriteAttributeString("xfdf", "original", null, text.ToString());
				}
				else if (!XmlReader.IsName(text.ToString()) || text.ToString().Contains(":"))
				{
					textWriter.WriteStartElement("field");
					textWriter.WriteAttributeString("xfdf", "original", null, text.ToString());
				}
				else
				{
					textWriter.WriteStartElement(text.ToString());
				}
				foreach (PdfString item2 in obj)
				{
					textWriter.WriteStartElement("value");
					textWriter.WriteString(item2.Value.ToString());
					textWriter.WriteEndElement();
				}
				textWriter.WriteEndElement();
				continue;
			}
			bool flag2 = false;
			if (!XmlReader.IsName(text.ToString()) || text.ToString().Contains(":"))
			{
				if (text.ToString().Contains(" ") && XmlReader.IsName(text.ToString().Replace(" ", "")) && !text.ToString().Contains(":"))
				{
					textWriter.WriteStartElement(text.ToString().Replace(" ", ""));
				}
				else
				{
					textWriter.WriteStartElement("field");
				}
				if (!flag)
				{
					textWriter.WriteAttributeString("xfdf", "original", null, text.ToString());
				}
			}
			else if (text.ToString().Contains(" "))
			{
				textWriter.WriteStartElement(text.ToString().Replace(" ", ""));
				if (!flag)
				{
					textWriter.WriteAttributeString("xfdf", "original", null, text.ToString());
				}
			}
			else
			{
				textWriter.WriteStartElement(text.ToString());
				flag2 = true;
			}
			if (flag)
			{
				textWriter.WriteAttributeString("xmlns", "xfa", null, "http://www.xfa.org/schema/xfa-data/1.0/");
				textWriter.WriteAttributeString("xfa", "contentType", null, "text/html");
				if (!flag2)
				{
					textWriter.WriteAttributeString("xfdf", "original", null, text);
				}
			}
			textWriter.WriteString(item.Value.ToString());
			textWriter.WriteEndElement();
		}
	}

	private bool HasEscapeCharacter(string text)
	{
		if (((text != null) & (text.Length > 0)) && char.IsDigit(text.ToCharArray()[0]))
		{
			return true;
		}
		string[] array = new string[20]
		{
			"?", "!", "/", "\\", "#", "$", "+", "{", "}", "(",
			")", "[", "]", "*", "&", "^", "<", ">", "@", "%"
		};
		for (int i = 0; i < array.Length; i++)
		{
			if (text.Contains(array[i]))
			{
				return true;
			}
		}
		return false;
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
						Dictionary<object, object> table2 = (Dictionary<object, object>)dictionary2[array[i]];
						GetElements(table2);
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

	private void fieldname(Dictionary<object, object> value, XmlWriter textWriter)
	{
		foreach (KeyValuePair<object, object> item in value)
		{
			string text = item.Key.ToString();
			bool flag = false;
			if (text.EndsWith(uniquekey))
			{
				text = text.Remove(text.Length - uniquekey.Length, uniquekey.Length);
				flag = true;
			}
			if (item.Value is Dictionary<object, object>)
			{
				if (HasEscapeCharacter(text))
				{
					textWriter.WriteStartElement("field");
					if (!flag)
					{
						textWriter.WriteAttributeString("xfdf", "original", null, text);
					}
					else
					{
						textWriter.WriteAttributeString("xmlns", "xfa", null, "http://www.xfa.org/schema/xfa-data/1.0/");
						textWriter.WriteAttributeString("xfa", "contentType", null, "text/html");
						textWriter.WriteAttributeString("xfdf", "original", null, text);
					}
				}
				else if (text.Contains(" "))
				{
					textWriter.WriteStartElement(item.Key.ToString().Replace(" ", ""));
					if (!flag)
					{
						textWriter.WriteAttributeString("xfdf", "original", null, text);
					}
					else
					{
						textWriter.WriteAttributeString("xmlns", "xfa", null, "http://www.xfa.org/schema/xfa-data/1.0/");
						textWriter.WriteAttributeString("xfa", "contentType", null, "text/html");
						textWriter.WriteAttributeString("xfdf", "original", null, text);
					}
				}
				else
				{
					textWriter.WriteStartElement(text, "");
					if (flag)
					{
						textWriter.WriteAttributeString("xmlns", "xfa", null, "http://www.xfa.org/schema/xfa-data/1.0/");
						textWriter.WriteAttributeString("xfa", "contentType", null, "text/html");
					}
				}
				fieldname((Dictionary<object, object>)item.Value, textWriter);
				textWriter.WriteEndElement();
				continue;
			}
			if (HasEscapeCharacter(text))
			{
				textWriter.WriteStartElement("field");
				if (!flag)
				{
					textWriter.WriteAttributeString("xfdf", "original", null, text);
				}
				else
				{
					textWriter.WriteAttributeString("xmlns", "xfa", null, "http://www.xfa.org/schema/xfa-data/1.0/");
					textWriter.WriteAttributeString("xfa", "contentType", null, "text/html");
					textWriter.WriteAttributeString("xfdf", "original", null, text);
				}
			}
			else if (text.Contains(" "))
			{
				textWriter.WriteStartElement(item.Key.ToString().Replace(" ", ""));
				if (!flag)
				{
					textWriter.WriteAttributeString("xfdf", "original", null, text);
				}
				else
				{
					textWriter.WriteAttributeString("xmlns", "xfa", null, "http://www.xfa.org/schema/xfa-data/1.0/");
					textWriter.WriteAttributeString("xfa", "contentType", null, "text/html");
					textWriter.WriteAttributeString("xfdf", "original", null, text);
				}
			}
			else
			{
				textWriter.WriteStartElement(text, "");
				if (flag)
				{
					textWriter.WriteAttributeString("xmlns", "xfa", null, "http://www.xfa.org/schema/xfa-data/1.0/");
					textWriter.WriteAttributeString("xfa", "contentType", null, "text/html");
				}
			}
			textWriter.WriteString(item.Value.ToString());
			textWriter.WriteEndElement();
		}
	}

	internal void ExportDataJSON(Stream stream)
	{
		Dictionary<string, List<string>> dictionary = new Dictionary<string, List<string>>();
		for (int i = 0; i < Fields.Count; i++)
		{
			PdfLoadedField pdfLoadedField = (PdfLoadedField)Fields[i];
			if (!pdfLoadedField.Export)
			{
				continue;
			}
			pdfLoadedField.ExportEmptyField = ExportEmptyFields;
			PdfName pdfName = PdfLoadedField.GetValue(pdfLoadedField.Dictionary, pdfLoadedField.CrossTable, "FT", inheritable: true) as PdfName;
			if (!(pdfName != null))
			{
				continue;
			}
			switch (pdfName.Value)
			{
			case "Tx":
				if (PdfLoadedField.GetValue(pdfLoadedField.Dictionary, pdfLoadedField.CrossTable, "V", inheritable: true) is PdfString pdfString2)
				{
					List<string> list9 = new List<string>();
					list9.Add(pdfString2.Value);
					dictionary.Add(pdfLoadedField.Name, list9);
				}
				else if (ExportEmptyFields)
				{
					List<string> list10 = new List<string>();
					list10.Add("");
					dictionary.Add(pdfLoadedField.Name, list10);
				}
				break;
			case "Ch":
			{
				IPdfPrimitive value2 = PdfLoadedField.GetValue(pdfLoadedField.Dictionary, pdfLoadedField.CrossTable, "V", inheritable: true);
				string text2 = null;
				object obj2 = null;
				string[] array = null;
				if (value2 != null)
				{
					obj2 = GetExportValue(value2);
					if (obj2 is string)
					{
						text2 = obj2 as string;
					}
					else
					{
						array = (obj2 as List<string>).ToArray();
					}
				}
				if (pdfLoadedField.GetType().Name == "PdfLoadedListBoxField")
				{
					if (value2 == null && pdfLoadedField.Dictionary.ContainsKey("I"))
					{
						text2 = (pdfLoadedField as PdfLoadedListBoxField).SelectedValue[0];
					}
					if (!string.IsNullOrEmpty(text2))
					{
						List<string> list11 = new List<string>();
						list11.Add(text2);
						dictionary.Add(pdfLoadedField.Name, list11);
					}
					else if (array != null && array.Length >= 1)
					{
						new PdfArray();
						List<string> list12 = new List<string>();
						string[] array2 = array;
						foreach (string item in array2)
						{
							list12.Add(item);
						}
						dictionary.Add(pdfLoadedField.Name, list12);
					}
					else if (ExportEmptyFields)
					{
						List<string> list13 = new List<string>();
						list13.Add("");
						dictionary.Add(pdfLoadedField.Name, list13);
					}
				}
				else
				{
					if (value2 == null && pdfLoadedField.Dictionary.ContainsKey("I"))
					{
						text2 = (pdfLoadedField as PdfLoadedComboBoxField).SelectedValue;
					}
					if (!string.IsNullOrEmpty(text2))
					{
						List<string> list14 = new List<string>();
						list14.Add(text2);
						dictionary.Add(pdfLoadedField.Name, list14);
					}
					else if (ExportEmptyFields)
					{
						List<string> list15 = new List<string>();
						list15.Add("");
						dictionary.Add(pdfLoadedField.Name, list15);
					}
				}
				break;
			}
			case "Btn":
			{
				IPdfPrimitive value = PdfLoadedField.GetValue(pdfLoadedField.Dictionary, pdfLoadedField.CrossTable, "V", inheritable: true);
				PdfLoadedRadioButtonListField pdfLoadedRadioButtonListField = null;
				if (value != null)
				{
					string text = pdfLoadedField.GetExportValue(pdfLoadedField, value);
					if (pdfLoadedField is PdfLoadedRadioButtonListField)
					{
						pdfLoadedRadioButtonListField = pdfLoadedField as PdfLoadedRadioButtonListField;
					}
					if (pdfLoadedRadioButtonListField != null && pdfLoadedRadioButtonListField.SelectedIndex == -1)
					{
						List<string> list = new List<string>();
						list.Add("Off");
						dictionary.Add(pdfLoadedField.Name, list);
					}
					else if (pdfLoadedField.Dictionary != null && pdfLoadedField.Dictionary.ContainsKey("Opt"))
					{
						PdfArray pdfArray = PdfCrossTable.Dereference(pdfLoadedField.Dictionary["Opt"]) as PdfArray;
						int result = 0;
						try
						{
							int.TryParse(text, out result);
							if (pdfArray != null && PdfCrossTable.Dereference(pdfArray[pdfLoadedRadioButtonListField.SelectedIndex]) is PdfString pdfString)
							{
								text = pdfString.Value;
							}
						}
						catch
						{
						}
						if (!string.IsNullOrEmpty(text))
						{
							List<string> list2 = new List<string>();
							list2.Add(text);
							dictionary.Add(pdfLoadedField.Name, list2);
						}
					}
					else if (!string.IsNullOrEmpty(text))
					{
						List<string> list3 = new List<string>();
						list3.Add(text);
						dictionary.Add(pdfLoadedField.Name, list3);
					}
					else if (pdfLoadedField is PdfLoadedCheckBoxField)
					{
						if (ExportEmptyFields)
						{
							List<string> list4 = new List<string>();
							list4.Add("");
							dictionary.Add(pdfLoadedField.Name, list4);
						}
						else
						{
							List<string> list5 = new List<string>();
							list5.Add("Off");
							dictionary.Add(pdfLoadedField.Name, list5);
						}
					}
					break;
				}
				if (pdfLoadedField is PdfLoadedRadioButtonListField)
				{
					List<string> list6 = new List<string>();
					list6.Add(pdfLoadedField.GetAppearanceStateValue(pdfLoadedField));
					dictionary.Add(pdfLoadedField.Name, list6);
					break;
				}
				PdfDictionary widgetAnnotation = pdfLoadedField.GetWidgetAnnotation(pdfLoadedField.Dictionary, pdfLoadedField.CrossTable);
				if (widgetAnnotation == null)
				{
					break;
				}
				PdfName pdfName2 = widgetAnnotation["AS"] as PdfName;
				if (pdfName2 != null)
				{
					List<string> list7 = new List<string>();
					list7.Add(pdfName2.Value);
					if (!dictionary.ContainsKey(pdfLoadedField.Name))
					{
						dictionary.Add(pdfLoadedField.Name, list7);
					}
				}
				else if (ExportEmptyFields)
				{
					List<string> list8 = new List<string>();
					list8.Add("");
					dictionary.Add(pdfLoadedField.Name, list8);
				}
				break;
			}
			}
		}
		byte[] bytes = Encoding.GetEncoding("UTF-8").GetBytes("{");
		stream.Write(bytes, 0, bytes.Length);
		int num = 0;
		foreach (KeyValuePair<string, List<string>> item2 in dictionary)
		{
			string text5;
			if (isJsonFormat && !IsJsonDelimiter(item2.Key))
			{
				string text3 = string.Join(",", item2.Value.ToArray());
				if (text3.Contains("\"") || text3.Contains("\\"))
				{
					string text4 = string.Empty;
					char[] array3 = text3.ToCharArray();
					for (int k = 0; k < array3.Length; k++)
					{
						if (array3[k] == '"' || array3[k].ToString() == "\\")
						{
							text4 += "\\";
						}
						text4 += array3[k];
					}
					text3 = text4;
				}
				text5 = "\"" + Convert.ToString(item2.Key) + "\":\"" + text3 + "\"";
			}
			else
			{
				text5 = "\"" + XmlConvert.EncodeName(Convert.ToString(item2.Key)) + "\":\"" + XmlConvert.EncodeName(string.Join(",", item2.Value.ToArray())) + "\"";
			}
			if (num > 0)
			{
				text5 = "," + text5;
			}
			byte[] bytes2 = Encoding.GetEncoding("UTF-8").GetBytes(text5);
			stream.Write(bytes2, 0, bytes2.Length);
			num++;
		}
		byte[] bytes3 = Encoding.GetEncoding("UTF-8").GetBytes("}");
		stream.Write(bytes3, 0, bytes3.Length);
	}

	internal bool IsJsonDelimiter(string str)
	{
		char[] array = str.ToCharArray();
		for (int i = 0; i < array.Length; i++)
		{
			string jsonDelimiters = m_jsonDelimiters;
			for (int j = 0; j < jsonDelimiters.Length; j++)
			{
				if (jsonDelimiters[j] == array[i])
				{
					return true;
				}
			}
		}
		return false;
	}

	private object GetExportValue(IPdfPrimitive fieldPrimitive)
	{
		string result = null;
		if (fieldPrimitive is PdfName)
		{
			result = (fieldPrimitive as PdfName).Value;
		}
		else if (fieldPrimitive is PdfString)
		{
			result = (fieldPrimitive as PdfString).Value;
		}
		else if (fieldPrimitive is PdfArray)
		{
			List<string> list = new List<string>();
			{
				foreach (IPdfPrimitive item in fieldPrimitive as PdfArray)
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
		return result;
	}

	internal void OnValidate(string nodeName)
	{
		if (nodeName.StartsWith("XML"))
		{
			throw new Exception("Element type names may not start with XML");
		}
		if (nodeName.StartsWith("_"))
		{
			throw new Exception("Element type names must start with a letter or underscore");
		}
		if (!char.IsLetter(nodeName[0]) && !char.IsNumber(nodeName[0]))
		{
			throw new Exception("Element type names must start with a letter or underscore");
		}
	}

	internal override void Dictionary_BeginSave(object sender, SavePdfPrimitiveEventArgs ars)
	{
		FlttenFormFields();
		if (m_fields.Count == 0 && !(ars.Writer.Document as PdfLoadedDocument).m_isXfaDocument && !EnableXfaFormFill)
		{
			PdfName pdfName = null;
			int num = CrossTable.PdfObjects.IndexOf(Dictionary);
			if (Dictionary != null && Dictionary.ContainsKey("Subtype"))
			{
				pdfName = Dictionary.Items[new PdfName("Subtype")] as PdfName;
			}
			if (pdfName == null || (pdfName != null && pdfName.Value != "Image"))
			{
				Dictionary.Clear();
				(sender as PdfDictionary).Remove("AcroForm");
				if (num != -1)
				{
					CrossTable.PdfObjects.Remove(num);
				}
			}
		}
		if (base.IsDefaultAppearance)
		{
			Dictionary.SetBoolean("NeedAppearances", base.IsDefaultAppearance);
		}
		else if (!base.IsDefaultAppearance && Dictionary.ContainsKey("NeedAppearances"))
		{
			Dictionary.SetBoolean("NeedAppearances", base.IsDefaultAppearance);
		}
		bool flag = EnableXfaFormFill;
		if (!flag && ars.Writer.Document is PdfLoadedDocument pdfLoadedDocument)
		{
			flag = pdfLoadedDocument.m_isXfaDocument;
		}
		if (!flag)
		{
			Dictionary.Remove("XFA");
		}
	}

	private void FlttenFormFields()
	{
		for (int i = 0; i < Fields.Count; i++)
		{
			PdfLoadedField pdfLoadedField = Fields[i] as PdfLoadedField;
			if (pdfLoadedField != null && pdfLoadedField.DisableAutoFormat && pdfLoadedField.Dictionary.ContainsKey("AA"))
			{
				pdfLoadedField.Dictionary.Remove("AA");
				pdfLoadedField.BeginSave();
			}
			if (pdfLoadedField != null && pdfLoadedField.Dictionary != null)
			{
				int num = 0;
				PdfDictionary dictionary = pdfLoadedField.Dictionary;
				bool flag = false;
				if (pdfLoadedField is PdfLoadedSignatureField)
				{
					flag = (pdfLoadedField as PdfLoadedSignatureField).IsSigned;
				}
				if (dictionary.ContainsKey("F") && PdfCrossTable.Dereference(dictionary["F"]) is PdfNumber pdfNumber)
				{
					num = pdfNumber.IntValue;
				}
				PdfArray pdfArray = null;
				if (pdfLoadedField.Dictionary.ContainsKey("Kids"))
				{
					pdfArray = PdfCrossTable.Dereference(pdfLoadedField.Dictionary["Kids"]) as PdfArray;
				}
				if (pdfLoadedField.Flatten && num != 6)
				{
					if (pdfLoadedField.Page != null || pdfArray != null)
					{
						pdfLoadedField.Draw();
					}
					Fields.Remove(pdfLoadedField);
					int num2 = CrossTable.PdfObjects.IndexOf(pdfLoadedField.Dictionary);
					if (num2 != -1)
					{
						CrossTable.PdfObjects.Remove(num2);
					}
					i--;
				}
				else if (pdfLoadedField.Changed || (base.SetAppearanceDictionary && !flag))
				{
					pdfLoadedField.BeginSave();
				}
				else if (pdfLoadedField.Flatten)
				{
					Fields.Remove(pdfLoadedField);
					int num3 = CrossTable.PdfObjects.IndexOf(pdfLoadedField.Dictionary);
					if (num3 != -1)
					{
						CrossTable.PdfObjects.Remove(num3);
					}
					i--;
				}
			}
			else
			{
				PdfField pdfField = Fields[i];
				if (pdfField.Flatten)
				{
					Fields.Remove(pdfField);
					pdfField.Draw();
					i--;
				}
				else
				{
					pdfField.Save();
				}
			}
		}
	}

	internal override void Clear()
	{
		if (m_fields != null)
		{
			m_fields.Clear();
		}
		if (m_pageMap != null)
		{
			m_pageMap.Clear();
		}
		if (m_terminalFields != null)
		{
			m_terminalFields.Clear();
		}
		Dictionary.Clear();
	}

	internal void RemoveFromDictionaries(PdfField field)
	{
		if (m_fields != null && m_fields.Count > 0)
		{
			PdfName key = new PdfName("Fields");
			PdfArray pdfArray = m_crossTable.GetObject(Dictionary[key]) as PdfArray;
			PdfReferenceHolder pdfReferenceHolder = new PdfReferenceHolder(field.Dictionary);
			pdfArray.Remove(pdfReferenceHolder);
			pdfArray.MarkChanged();
			if (!IsFormContainsKids || !field.Dictionary.Items.ContainsKey(new PdfName("Parent")))
			{
				for (int i = 0; i < pdfArray.Count; i++)
				{
					PdfDictionary pdfDictionary = PdfCrossTable.Dereference(CrossTable.GetObject(pdfArray[i])) as PdfDictionary;
					PdfName key2 = new PdfName("Kids");
					if (pdfDictionary == null || !pdfDictionary.ContainsKey(key2))
					{
						continue;
					}
					PdfArray pdfArray2 = CrossTable.GetObject(pdfDictionary[key2]) as PdfArray;
					pdfArray2.Remove(pdfReferenceHolder);
					for (int j = 0; j < pdfArray2.Count; j++)
					{
						if (pdfArray2.Elements[j] is PdfNull)
						{
							pdfArray2.RemoveAt(j);
							j = -1;
						}
					}
				}
			}
			else if (field.Dictionary.Items.ContainsKey(new PdfName("Parent")))
			{
				PdfArray pdfArray3 = null;
				if (PdfCrossTable.Dereference(field.Dictionary["Parent"]) is PdfDictionary pdfDictionary2 && pdfDictionary2.ContainsKey("Kids") && PdfCrossTable.Dereference(pdfDictionary2["Kids"]) is PdfArray pdfArray4)
				{
					for (int k = 0; k < pdfArray4.Count; k++)
					{
						if ((pdfArray4[k] as PdfReferenceHolder).Equals(pdfReferenceHolder))
						{
							pdfArray4.Remove(pdfReferenceHolder);
						}
					}
				}
			}
			Dictionary.SetProperty(key, pdfArray);
		}
		if (field is PdfLoadedField)
		{
			DeleteFromPages(field);
			DeleteAnnottation(field);
		}
	}

	internal new void DeleteFromPages(PdfField field)
	{
		PdfDictionary dictionary = field.Dictionary;
		PdfName key = new PdfName("Kids");
		PdfName key2 = new PdfName("Annots");
		PdfName key3 = new PdfName("P");
		if (dictionary.ContainsKey(key))
		{
			PdfArray pdfArray = CrossTable.GetObject(dictionary[key]) as PdfArray;
			int i = 0;
			for (int count = pdfArray.Count; i < count; i++)
			{
				PdfReferenceHolder pdfReferenceHolder = pdfArray[i] as PdfReferenceHolder;
				PdfDictionary pdfDictionary = CrossTable.GetObject(pdfReferenceHolder) as PdfDictionary;
				PdfReference pointer = null;
				if (pdfDictionary.ContainsKey(key3) && !(pdfDictionary["P"] is PdfNull))
				{
					pointer = CrossTable.GetReference(pdfDictionary[key3]);
				}
				else if (dictionary.ContainsKey(key3) && !(dictionary["P"] is PdfNull))
				{
					pointer = CrossTable.GetReference(dictionary[key3]);
				}
				else if (field.Page != null)
				{
					pointer = CrossTable.GetReference(field.Page.Dictionary);
				}
				if (CrossTable.GetObject(pointer) is PdfDictionary pdfDictionary2 && pdfDictionary2.ContainsKey(key2))
				{
					PdfArray pdfArray2 = CrossTable.GetObject(pdfDictionary2[key2]) as PdfArray;
					pdfArray2.Remove(pdfReferenceHolder);
					pdfArray2.MarkChanged();
					pdfDictionary2.SetProperty(key2, pdfArray2);
					IPdfPrimitive @object = m_crossTable.GetObject(pdfDictionary);
					int num = m_crossTable.PdfObjects.IndexOf(@object);
					if (num != -1)
					{
						m_crossTable.PdfObjects.Remove(num);
					}
				}
				else if (field is PdfLoadedField && pdfReferenceHolder != null)
				{
					PdfLoadedField obj = field as PdfLoadedField;
					obj.m_requiredReference = pdfReferenceHolder;
					if (field.Page != null && field.Page.Dictionary.ContainsKey(key2))
					{
						PdfPageBase page = field.Page;
						PdfArray obj2 = CrossTable.GetObject(page.Dictionary[key2]) as PdfArray;
						obj2.Remove(new PdfReferenceHolder(pdfDictionary));
						obj2.MarkChanged();
					}
					if (CrossTable.PdfObjects.Contains(pdfDictionary))
					{
						CrossTable.PdfObjects.Remove(CrossTable.PdfObjects.IndexOf(pdfDictionary));
					}
					obj.m_requiredReference = null;
				}
			}
		}
		else
		{
			PdfReference pointer2 = null;
			if (dictionary.ContainsKey(key3) && !(dictionary["P"] is PdfNull))
			{
				pointer2 = CrossTable.GetReference(dictionary[key3]);
			}
			else if (field.Page != null)
			{
				pointer2 = CrossTable.GetReference(field.Page.Dictionary);
			}
			if (CrossTable.GetObject(pointer2) is PdfDictionary pdfDictionary3 && pdfDictionary3.ContainsKey(key2))
			{
				PdfArray pdfArray3 = CrossTable.GetObject(pdfDictionary3[key2]) as PdfArray;
				pdfArray3.Remove(new PdfReferenceHolder(dictionary));
				pdfArray3.MarkChanged();
				pdfDictionary3.SetProperty(key2, pdfArray3);
			}
			else if (field.Page != null)
			{
				PdfPageBase page2 = field.Page;
				if (page2.Dictionary.ContainsKey(key2))
				{
					PdfArray obj3 = CrossTable.GetObject(page2.Dictionary[key2]) as PdfArray;
					obj3.Remove(new PdfReferenceHolder(dictionary));
					obj3.MarkChanged();
				}
			}
		}
		if (field.Page == null || !(field.Page is PdfLoadedPage { Document: not null } pdfLoadedPage))
		{
			return;
		}
		PdfCatalog catalog = pdfLoadedPage.Document.Catalog;
		if (catalog == null || !catalog.ContainsKey("Perms") || !(PdfCrossTable.Dereference(catalog["Perms"]) is PdfDictionary pdfDictionary4) || !pdfDictionary4.ContainsKey("DocMDP"))
		{
			return;
		}
		PdfReferenceHolder pdfReferenceHolder2 = pdfDictionary4["DocMDP"] as PdfReferenceHolder;
		if (dictionary.ContainsKey("V"))
		{
			PdfReferenceHolder pdfReferenceHolder3 = dictionary["V"] as PdfReferenceHolder;
			if (pdfReferenceHolder3 != null && pdfReferenceHolder2 != null && pdfReferenceHolder3.Reference != null && pdfReferenceHolder2.Reference != null && pdfReferenceHolder3.Reference == pdfReferenceHolder2.Reference)
			{
				pdfDictionary4.Remove("DocMDP");
			}
		}
	}

	internal void DeleteAnnottation(PdfField field)
	{
		PdfDictionary dictionary = field.Dictionary;
		PdfName key = new PdfName("Kids");
		if (dictionary.ContainsKey(key))
		{
			PdfArray pdfArray = m_crossTable.GetObject(dictionary[key]) as PdfArray;
			pdfArray.Clear();
			dictionary.SetProperty(key, pdfArray);
		}
	}

	internal void CreateFormFields(PdfLoadedPage page, List<long> widgetReferences)
	{
		if (m_widgetDictionary == null)
		{
			m_widgetDictionary = new Dictionary<string, List<PdfDictionary>>();
		}
		if (!page.Dictionary.ContainsKey("Annots") || !(PdfCrossTable.Dereference(page.Dictionary["Annots"]) is PdfArray pdfArray))
		{
			return;
		}
		for (int i = 0; i < pdfArray.Count; i++)
		{
			PdfDictionary pdfDictionary = PdfCrossTable.Dereference(pdfArray[i]) as PdfDictionary;
			PdfReferenceHolder pdfReferenceHolder = pdfArray[i] as PdfReferenceHolder;
			if (pdfDictionary != null && pdfDictionary.ContainsKey("Subtype"))
			{
				PdfName pdfName = pdfDictionary.Items[new PdfName("Subtype")] as PdfName;
				if (pdfName != null && pdfName.Value == "Widget")
				{
					if (!TerminalFields.Contains(pdfDictionary) && pdfDictionary.ContainsKey("T"))
					{
						string value = (pdfDictionary.Items[new PdfName("T")] as PdfString).Value;
						if (!m_terminalAddedFieldsNames.Contains(value))
						{
							if (m_widgetDictionary.ContainsKey(value))
							{
								m_widgetDictionary[value].Add(pdfDictionary);
							}
							else
							{
								List<PdfDictionary> list = new List<PdfDictionary>();
								list.Add(pdfDictionary);
								m_widgetDictionary.Add(value, list);
							}
						}
					}
					else if (pdfDictionary.ContainsKey("Parent") && !TerminalFields.Contains(pdfDictionary) && (pdfDictionary.Items[new PdfName("Parent")] as PdfReferenceHolder).Object is PdfDictionary pdfDictionary2)
					{
						if (!pdfDictionary2.ContainsKey("Fields") && pdfDictionary2.ContainsKey("T"))
						{
							string value2 = (pdfDictionary2.Items[new PdfName("T")] as PdfString).Value;
							if (pdfReferenceHolder.Reference != null && !widgetReferences.Contains(pdfReferenceHolder.Reference.ObjNum) && !TerminalFields.Contains(pdfDictionary2) && !m_terminalAddedFieldsNames.Contains(value2))
							{
								TerminalFields.Add(pdfDictionary2);
							}
							else if (pdfDictionary2.ContainsKey("Kids") && pdfDictionary2.Count == 1)
							{
								pdfDictionary.Remove("Parent");
							}
						}
						else if (!pdfDictionary2.ContainsKey("Kids"))
						{
							pdfDictionary.Remove("Parent");
						}
					}
				}
			}
			if (pdfDictionary != null)
			{
				PdfName pdfName2 = null;
				PdfName pdfName3 = PdfCrossTable.Dereference(pdfDictionary["FT"]) as PdfName;
				if (pdfName3 != null && pdfName3.Value == "Sig")
				{
					isSignature = true;
				}
				if (pdfDictionary.ContainsKey("Subtype"))
				{
					pdfName2 = pdfDictionary.Items[new PdfName("Subtype")] as PdfName;
				}
				if (pdfDictionary.ContainsKey("P") && pdfDictionary.ContainsKey("AP") && (page.Document as PdfLoadedDocument).Pages.m_pageIndexCollection != null && !(page.Document as PdfLoadedDocument).Pages.m_pageIndexCollection.ContainsKey(page.Dictionary) && !isSignature && pdfName2 != null && pdfName2.Value == "Widget")
				{
					pdfDictionary.Remove("P");
					pdfDictionary.SetProperty("P", new PdfReferenceHolder(page.Dictionary));
					pdfDictionary.Modify();
				}
			}
			if (pdfReferenceHolder != null && pdfReferenceHolder.Reference != null && !page.AnnotsReference.Contains(pdfReferenceHolder.Reference))
			{
				page.AnnotsReference.Add(pdfReferenceHolder.Reference);
			}
		}
	}

	internal override string GetCorrectName(string name)
	{
		List<string> list = new List<string>();
		for (int i = 0; i < Fields.Count; i++)
		{
			list.Add(Fields[i].Name);
		}
		string text = name;
		int num = 0;
		while (list.IndexOf(text) != -1)
		{
			text = name + num;
			num++;
		}
		return text;
	}

	private PdfLoadedFieldImportError[] ImportData(Stream fileName, DataFormat dataFormat, bool continueImportOnError)
	{
		return dataFormat switch
		{
			DataFormat.Xml => ImportDataXML(fileName, continueImportOnError), 
			DataFormat.Fdf => ImportDataFDF(fileName, continueImportOnError), 
			DataFormat.XFdf => ImportDataXFDF(fileName, continueImportOnError), 
			_ => null, 
		};
	}

	public PdfLoadedFieldImportError[] ImportDataXML(Stream stream, bool continueImportOnError)
	{
		XElement xElement = XElement.Load(stream);
		if (xElement.Name.LocalName.ToUpper() != "fields".ToUpper())
		{
			throw new ArgumentException("The XML form data stream is not valid");
		}
		if (xElement.Name.LocalName != "fields")
		{
			m_settings.AsPerSpecification = false;
		}
		m_xmlFields = new Dictionary<string, List<string>>();
		List<PdfLoadedFieldImportError> list = new List<PdfLoadedFieldImportError>();
		ImportXMLData(xElement.Elements(), continueImportOnError, list);
		if (m_settings.AsPerSpecification)
		{
			foreach (KeyValuePair<string, List<string>> xmlField in m_xmlFields)
			{
				string text = xmlField.Key;
				object fieldValue = xmlField.Value[0];
				if (xmlField.Value.Count > 1)
				{
					text = text.Remove(text.Length - 6, 6);
					string[] array = new string[xmlField.Value.Count];
					for (int i = 0; i < xmlField.Value.Count; i++)
					{
						array[i] = xmlField.Value[i];
					}
					fieldValue = array;
				}
				PdfLoadedField pdfLoadedField = null;
				try
				{
					pdfLoadedField = (PdfLoadedField)Fields[text];
					if (pdfLoadedField != null)
					{
						pdfLoadedField.isAcrobat = true;
						pdfLoadedField.ImportFieldValue(fieldValue);
					}
				}
				catch (Exception exception)
				{
					if (text.Contains(".value"))
					{
						text = text.Substring(0, text.Length - 6);
						try
						{
							pdfLoadedField = (PdfLoadedField)Fields[text];
							if (pdfLoadedField != null)
							{
								pdfLoadedField.isAcrobat = true;
								pdfLoadedField.ImportFieldValue(fieldValue);
							}
						}
						catch
						{
							if (!continueImportOnError)
							{
								throw;
							}
							PdfLoadedFieldImportError item = new PdfLoadedFieldImportError(pdfLoadedField, exception);
							list.Add(item);
						}
					}
					else
					{
						if (!continueImportOnError)
						{
							throw;
						}
						PdfLoadedFieldImportError item2 = new PdfLoadedFieldImportError(pdfLoadedField, exception);
						list.Add(item2);
					}
				}
			}
			m_xmlFields.Clear();
		}
		if (list.Count == 0)
		{
			return null;
		}
		return list.ToArray();
	}

	public void ImportDataXML(byte[] array)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		MemoryStream stream = new MemoryStream(array);
		if (m_settings.IgnoreErrors)
		{
			ImportDataXML(stream, continueImportOnError: true);
		}
		else
		{
			ImportDataXML(stream, continueImportOnError: false);
		}
	}

	public void ImportDataXML(Stream stream)
	{
		XElement xElement = XElement.Load(stream);
		if (xElement.Name.LocalName.ToUpper() != "fields".ToUpper())
		{
			throw new ArgumentException("The XML form data stream is not valid");
		}
		List<PdfLoadedFieldImportError> list = new List<PdfLoadedFieldImportError>();
		ImportXMLData(xElement.Elements(), continueImportOnError: false, list);
	}

	private void ImportXMLData(IEnumerable<XElement> xmlnode, bool continueImportOnError, List<PdfLoadedFieldImportError> list)
	{
		XNamespace xNamespace = "http://ns.adobe.com/xfdf-transition/";
		foreach (XElement item2 in xmlnode)
		{
			if (item2.FirstNode is XText xText)
			{
				string value = xText.Value;
				XElement parent = xText.Parent;
				string text = "";
				while (parent.Name.LocalName.ToUpper() != "fields".ToUpper())
				{
					if (text.Length > 0)
					{
						text = "." + text;
					}
					text = ((!m_settings.AsPerSpecification) ? (XmlConvert.DecodeName(parent.Name.LocalName) + text) : ((parent.Attributes(xNamespace + "original") == null || parent.Attributes(xNamespace + "original").Count() <= 0) ? (XmlConvert.DecodeName(parent.Name.LocalName) + text) : (parent.Attributes(xNamespace + "original").First().Value + text)));
					parent = parent.Parent;
				}
				PdfLoadedField pdfLoadedField = null;
				try
				{
					if (m_settings.AsPerSpecification)
					{
						if (m_xmlFields.ContainsKey(text))
						{
							m_xmlFields[text].Add(value);
						}
						else
						{
							List<string> list2 = new List<string>();
							list2.Add(value);
							m_xmlFields[text] = list2;
						}
					}
					else
					{
						text = XmlConvert.DecodeName(text);
						pdfLoadedField = (PdfLoadedField)Fields[text];
						pdfLoadedField?.ImportFieldValue(value);
					}
				}
				catch (Exception exception)
				{
					if (!continueImportOnError)
					{
						throw;
					}
					PdfLoadedFieldImportError item = new PdfLoadedFieldImportError(pdfLoadedField, exception);
					list.Add(item);
				}
			}
			if (item2.Elements() != null)
			{
				ImportXMLData(item2.Elements(), continueImportOnError, list);
			}
		}
	}

	public void ImportDataXFDF(byte[] array)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		MemoryStream stream = new MemoryStream(array);
		ImportDataXFDF(stream);
	}

	public void ImportDataXFDF(Stream stream)
	{
		if (stream != null)
		{
			if (m_settings.AsPerSpecification)
			{
				ImportDataXFDF(stream, continueImportOnError: true);
			}
			else
			{
				ImportDataXFDF(stream, continueImportOnError: false);
			}
		}
	}

	private void ImportXFDFNodes(IEnumerable<XElement> fieldElements, XNamespace ns)
	{
		foreach (XElement fieldElement in fieldElements)
		{
			string text = "";
			if (fieldElement.Attribute("name") != null)
			{
				text = fieldElement.Attribute("name").Value.Trim();
			}
			if (text.Length <= 0)
			{
				continue;
			}
			IEnumerable<XElement> enumerable = fieldElement.Descendants(ns + "field");
			if (enumerable.Count() > 0)
			{
				ImportXFDFNodes(enumerable, ns);
				continue;
			}
			XElement xElement = null;
			if (fieldElement.Descendants(ns + "value") != null && fieldElement.Descendants(ns + "value").Count() > 0)
			{
				xElement = fieldElement.Descendants(ns + "value").First();
			}
			IEnumerable<XElement> enumerable2 = fieldElement.Descendants();
			if (xElement != null)
			{
				XElement xElement2 = fieldElement;
				string text2 = "";
				while (xElement2.Name.LocalName != "fields")
				{
					if (text2.Length > 0)
					{
						text2 = "." + text2;
					}
					text2 = ((xElement2.Attribute("name") == null || !(xElement2.Attribute("name").Value != "")) ? (xElement2.Name.LocalName + text2) : (xElement2.Attribute("name").Value + text2));
					xElement2 = xElement2.Parent;
				}
				text = text2;
				if (m_xdfdFields.ContainsKey(text))
				{
					m_xdfdFields[text].Add(xElement.Value);
					continue;
				}
				List<string> list = new List<string>();
				if (enumerable2 != null && enumerable2.Count() > 1)
				{
					foreach (XElement item2 in enumerable2)
					{
						list.Add(item2.Value);
					}
					m_xdfdFields[text] = list;
				}
				else
				{
					list.Add(xElement.Value);
					m_xdfdFields[text] = list;
				}
				continue;
			}
			if (fieldElement.Descendants(ns + "value-richtext") != null && fieldElement.Descendants(ns + "value-richtext").Count() > 0)
			{
				xElement = fieldElement.Descendants(ns + "value-richtext").First();
			}
			if (xElement == null)
			{
				continue;
			}
			XElement xElement3 = fieldElement;
			string text3 = "";
			while (xElement3.Name.LocalName != "fields")
			{
				if (text3.Length > 0)
				{
					text3 = "." + text3;
				}
				text3 = ((xElement3.Attribute("name") == null || !(xElement3.Attribute("name").Value != "")) ? (xElement3.Name.LocalName + text3) : (xElement3.Attribute("name").Value + text3));
				xElement3 = xElement3.Parent;
			}
			text = text3;
			string item = xElement.ToString();
			XElement xElement4 = null;
			if (xElement.Descendants(ns + "value") != null && xElement.Descendants(ns + "value").Count() > 0)
			{
				xElement4 = xElement.Descendants().First();
			}
			if (xElement4 != null)
			{
				item = xElement4.ToString();
			}
			if (m_xdfdFields.ContainsKey(text))
			{
				m_xdfdFields[text].Add(item);
			}
			else
			{
				List<string> list2 = new List<string>();
				list2.Add(item);
				m_xdfdFields[text] = list2;
			}
			if (!m_xfdfRichText.ContainsKey(text))
			{
				m_xfdfRichText[text] = string.Concat((from x in xElement.Nodes()
					select x.ToString()).ToArray());
			}
		}
	}

	private PdfLoadedFieldImportError[] ImportDataXFDF(Stream stream, bool continueImportOnError)
	{
		List<PdfLoadedFieldImportError> list = new List<PdfLoadedFieldImportError>();
		if (stream != null)
		{
			m_xdfdFields = new Dictionary<string, List<string>>();
			XElement xElement = XElement.Load(stream);
			XNamespace @namespace = xElement.Name.Namespace;
			_ = (XNamespace?)"http://ns.adobe.com/xfdf/";
			XElement xElement2 = xElement.Descendants().First();
			IEnumerable<XElement> fieldElements = xElement.Descendants(@namespace + "field");
			xElement.Descendants(@namespace + "value");
			IEnumerable<XElement> enumerable = xElement.Descendants(@namespace + "ids");
			if (xElement2 != null && xElement2.Attribute("href") != null)
			{
				xElement2.Attribute("href").Value.Trim();
			}
			if (enumerable != null && enumerable.Count() != 1)
			{
				m_settings.AsPerSpecification = false;
			}
			ImportXFDFNodes(fieldElements, @namespace);
			foreach (KeyValuePair<string, List<string>> xdfdField in m_xdfdFields)
			{
				string key = xdfdField.Key;
				string text = null;
				if (m_xfdfRichText != null && m_xfdfRichText.Count > 0 && m_xfdfRichText.ContainsKey(key))
				{
					text = m_xfdfRichText[key];
				}
				object fieldValue = xdfdField.Value[0];
				if (xdfdField.Value.Count > 1)
				{
					string[] array = new string[xdfdField.Value.Count];
					for (int i = 0; i < xdfdField.Value.Count; i++)
					{
						array[i] = xdfdField.Value[i];
					}
					fieldValue = array;
				}
				PdfLoadedField pdfLoadedField = null;
				try
				{
					pdfLoadedField = (PdfLoadedField)Fields[key];
					if (pdfLoadedField != null)
					{
						if (text != null)
						{
							pdfLoadedField.Dictionary.SetProperty("RV", new PdfString(text));
						}
						pdfLoadedField.isAcrobat = m_settings.AsPerSpecification;
						pdfLoadedField.ImportFieldValue(fieldValue);
					}
				}
				catch (Exception exception)
				{
					if (!continueImportOnError)
					{
						throw;
					}
					PdfLoadedFieldImportError item = new PdfLoadedFieldImportError(pdfLoadedField, exception);
					list.Add(item);
				}
			}
		}
		if (list.Count == 0)
		{
			return null;
		}
		return list.ToArray();
	}

	public void ImportDataJson(byte[] array)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		MemoryStream stream = new MemoryStream(array);
		ImportDataJson(stream);
	}

	public void ImportDataJson(Stream stream)
	{
		if (stream == null)
		{
			return;
		}
		string text = null;
		string text2 = null;
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		new StreamReader(stream);
		PdfReader pdfReader = new PdfReader(stream);
		string nextJsonToken = pdfReader.GetNextJsonToken();
		pdfReader.Position = 0L;
		while (nextJsonToken != null && nextJsonToken != string.Empty)
		{
			if (nextJsonToken != "{" && nextJsonToken != "}" && nextJsonToken != "\"" && nextJsonToken != ",")
			{
				text = nextJsonToken;
				do
				{
					nextJsonToken = pdfReader.GetNextJsonToken();
				}
				while (nextJsonToken != ":");
				do
				{
					nextJsonToken = pdfReader.GetNextJsonToken();
				}
				while (nextJsonToken != "\"");
				string text3 = string.Empty;
				do
				{
					pdfReader.m_importFormData = true;
					nextJsonToken = pdfReader.GetNextJsonToken();
					if (nextJsonToken != "\"")
					{
						text3 += nextJsonToken;
					}
				}
				while (nextJsonToken != "\"");
				pdfReader.m_importFormData = false;
				text2 = text3;
			}
			if (text != null && text2 != null)
			{
				dictionary[XmlConvert.DecodeName(text)] = XmlConvert.DecodeName(text2);
				text = null;
				text2 = null;
			}
			nextJsonToken = pdfReader.GetNextJsonToken();
		}
		PdfLoadedField field = null;
		foreach (KeyValuePair<string, string> item in dictionary)
		{
			Fields.TryGetField(item.Key, out field);
			field?.ImportFieldValue(item.Value);
		}
		stream.Dispose();
	}

	public void ImportDataFDF(Stream stream)
	{
		if (m_settings.IgnoreErrors)
		{
			ImportDataFDF(stream, continueImportOnError: true);
		}
		else
		{
			ImportDataFDF(stream, continueImportOnError: false);
		}
	}

	public void ImportDataFDF(byte[] array)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		MemoryStream stream = new MemoryStream(array);
		if (m_settings.IgnoreErrors)
		{
			ImportDataFDF(stream, continueImportOnError: true);
		}
		else
		{
			ImportDataFDF(stream, continueImportOnError: false);
		}
	}

	public PdfLoadedFieldImportError[] ImportDataFDF(Stream stream, bool continueImportOnError)
	{
		FdfParser fdfParser = new FdfParser(stream);
		fdfParser.fdfImport = true;
		fdfParser.ParseObjectTrailer();
		PdfReader pdfReader = new PdfReader(stream);
		pdfReader.Position = 0L;
		string nextToken = pdfReader.GetNextToken();
		if (nextToken.StartsWith("%"))
		{
			nextToken = pdfReader.GetNextToken();
			if (!nextToken.StartsWith("FDF-"))
			{
				throw new Exception("The source is not a valid FDF file because it does not start with\"%FDF-\"");
			}
		}
		nextToken = pdfReader.GetNextToken();
		nextToken = pdfReader.GetNextToken();
		if ((nextToken != null && nextToken != "âãÏÓ" && nextToken != "Ã¢Ã£Ã\u008fÃ\u0093") || !fdfParser.hasTrailer)
		{
			m_settings.AsPerSpecification = false;
		}
		if (!m_settings.AsPerSpecification)
		{
			Dictionary<string, string[]> dictionary = new Dictionary<string, string[]>();
			List<PdfLoadedFieldImportError> list = new List<PdfLoadedFieldImportError>();
			string text = "";
			nextToken = pdfReader.GetNextToken();
			while (nextToken != null && nextToken != string.Empty)
			{
				if (nextToken.ToUpper() == "T")
				{
					text = GetFieldName(pdfReader, out nextToken);
				}
				currentFieldName = text;
				if (nextToken.ToUpper() == "V")
				{
					if (previousFieldName == string.Empty)
					{
						previousFieldName = currentFieldName;
					}
					if (currentFieldName != previousFieldName)
					{
						selectedList = null;
						previousFieldName = currentFieldName;
					}
					else
					{
						currentFieldName = text;
						previousFieldName = currentFieldName;
					}
					GetFieldValue(pdfReader, nextToken, text, dictionary);
				}
				nextToken = pdfReader.GetNextToken();
			}
			if (selectedList != null && selectedList.Count > 1)
			{
				dictionary[text] = selectedList.ToArray();
			}
			PdfLoadedField pdfLoadedField = null;
			foreach (KeyValuePair<string, string[]> item3 in dictionary)
			{
				try
				{
					string name = item3.Key.ToString();
					pdfLoadedField = (PdfLoadedField)Fields[name];
					if (pdfLoadedField != null)
					{
						base.DisableAutoFormat = true;
						pdfLoadedField.ImportFieldValue(item3.Value);
					}
				}
				catch (Exception exception)
				{
					if (!continueImportOnError)
					{
						throw;
					}
					PdfLoadedFieldImportError item = new PdfLoadedFieldImportError(pdfLoadedField, exception);
					list.Add(item);
				}
			}
			if (list.Count == 0)
			{
				return null;
			}
			return list.ToArray();
		}
		PdfLoadedField field = null;
		List<PdfLoadedFieldImportError> list2 = new List<PdfLoadedFieldImportError>();
		FdfParser fdfParser2 = new FdfParser(stream);
		fdfParser2.fdfImport = true;
		fdfParser2.ParseObjectData();
		m_fdfFields = new Dictionary<string, PdfDictionary>();
		Dictionary<string, IPdfPrimitive> objects = fdfParser2.FdfObjects.Objects;
		if (objects != null && objects.Count > 0 && objects.ContainsKey("trailer"))
		{
			if (objects["trailer"] is PdfDictionary pdfDictionary && pdfDictionary.ContainsKey("Root"))
			{
				PdfReferenceHolder pdfReferenceHolder = pdfDictionary["Root"] as PdfReferenceHolder;
				if (pdfReferenceHolder != null)
				{
					PdfReference reference = pdfReferenceHolder.Reference;
					if (reference != null)
					{
						string key = reference.ObjNum + " " + reference.GenNum;
						if (objects.ContainsKey(key) && objects[key] is PdfDictionary pdfDictionary2 && pdfDictionary2.ContainsKey("FDF"))
						{
							if (pdfDictionary2["FDF"] is PdfDictionary pdfDictionary3 && pdfDictionary3.ContainsKey("Fields"))
							{
								PdfArray primitive = pdfDictionary3["Fields"] as PdfArray;
								PdfDictionary pdfDictionary4 = new PdfDictionary();
								pdfDictionary4.SetProperty("Kids", primitive);
								ReadFDFFields(pdfDictionary4, "");
							}
							objects.Remove(key);
						}
					}
				}
			}
			objects.Remove("trailer");
		}
		foreach (KeyValuePair<string, PdfDictionary> fdfField in m_fdfFields)
		{
			try
			{
				string fieldName = fdfField.Key.ToString();
				Fields.TryGetField(fieldName, out field);
				if (field == null)
				{
					continue;
				}
				field.isAcrobat = true;
				if (fdfField.Value.ContainsKey("RV"))
				{
					field.Dictionary.SetProperty("RV", fdfField.Value["RV"]);
				}
				if (fdfField.Value["V"] is PdfString pdfString)
				{
					field.ImportFieldValue(pdfString.Value);
				}
				else if (fdfField.Value["V"] is PdfArray pdfArray)
				{
					string[] array = new string[pdfArray.Count];
					for (int i = 0; i < pdfArray.Count; i++)
					{
						if (pdfArray[i] is PdfString pdfString2)
						{
							array[i] = pdfString2.Value;
						}
					}
					field.ImportFieldValue(array);
				}
				else
				{
					PdfName pdfName = fdfField.Value["V"] as PdfName;
					if (pdfName != null)
					{
						field.ImportFieldValue(pdfName.Value);
					}
				}
			}
			catch (Exception exception2)
			{
				if (!continueImportOnError)
				{
					throw;
				}
				PdfLoadedFieldImportError item2 = new PdfLoadedFieldImportError(field, exception2);
				list2.Add(item2);
			}
		}
		fdfParser2.Dispose();
		m_fdfFields.Clear();
		if (list2.Count == 0)
		{
			return null;
		}
		return list2.ToArray();
	}

	private void ReadFDFFields(PdfDictionary kidNodes, string name)
	{
		if (kidNodes["Kids"] is PdfArray { Count: not 0 } pdfArray)
		{
			kidNodes.Remove("Kids");
			for (int i = 0; i < pdfArray.Count; i++)
			{
				PdfDictionary pdfDictionary = new PdfDictionary();
				pdfDictionary = kidNodes;
				PdfDictionary obj = pdfArray[i] as PdfDictionary;
				PdfString pdfString = obj["T"] as PdfString;
				string text = name;
				if (pdfString != null)
				{
					text = text + "." + pdfString.Value;
				}
				pdfDictionary = obj;
				pdfDictionary.Remove("T");
				ReadFDFFields(pdfDictionary, text);
			}
		}
		else
		{
			if (name.Length > 0)
			{
				name = name.Substring(1);
			}
			m_fdfFields[name] = kidNodes;
		}
	}

	private string GetFieldName(PdfReader reader, out string token)
	{
		string text = "";
		token = reader.GetNextToken();
		if (!string.IsNullOrEmpty(token))
		{
			if (token == "<")
			{
				token = reader.GetNextToken();
				if (!string.IsNullOrEmpty(token) && token != ">")
				{
					byte[] data = new PdfString().HexToBytes(token);
					token = PdfString.ByteToString(data);
					text = token;
				}
			}
			else
			{
				token = reader.GetNextToken();
				if (!string.IsNullOrEmpty(token))
				{
					string text2 = " ";
					while (text2 != ")")
					{
						text2 = reader.GetNextToken();
						if (!string.IsNullOrEmpty(text2) && text2 != ")")
						{
							token = token + " " + text2;
						}
					}
					text = token;
					if (text[0].Equals('_'))
					{
						text = text.Replace("_", "s");
					}
					token = text2;
				}
			}
		}
		return text;
	}

	private void GetFieldValue(PdfReader reader, string token, string fieldName, Dictionary<string, string[]> table)
	{
		token = reader.GetNextToken();
		if (string.IsNullOrEmpty(token))
		{
			return;
		}
		if (token == "[")
		{
			token = reader.GetNextToken();
			if (!string.IsNullOrEmpty(token))
			{
				List<string> list = new List<string>();
				while (token != "]")
				{
					token = GetFieldValue(reader, token, isMultiSelect: true, table, fieldName, list);
				}
				if (!table.ContainsKey(fieldName) && list.Count > 0)
				{
					table.Add(fieldName, list.ToArray());
				}
			}
		}
		else
		{
			GetFieldValue(reader, token, isMultiSelect: false, table, fieldName, null);
		}
	}

	private string GetFieldValue(PdfReader reader, string token, bool isMultiSelect, Dictionary<string, string[]> table, string fieldName, List<string> fieldValues)
	{
		if (token == "<")
		{
			token = reader.GetNextToken();
			if (!string.IsNullOrEmpty(token) && token != ">")
			{
				token = PdfString.ByteToString(new PdfString().HexToBytes(token));
				currentFieldName = fieldName;
				if (isMultiSelect)
				{
					fieldValues.Add(token);
				}
				else if (!table.ContainsKey(fieldName))
				{
					if (selectedList == null)
					{
						selectedList = new List<string>();
						selectedList.Add(token);
					}
					table.Add(fieldName, new string[1] { token });
				}
				else if (table.ContainsKey(fieldName) && selectedList != null)
				{
					selectedList.Add(token);
				}
			}
		}
		else if (isMultiSelect)
		{
			while (token != ">" && token != ")" && token != "]")
			{
				if (!string.IsNullOrEmpty(token) && (token == "/" || token != ")"))
				{
					token = reader.GetNextToken();
					if (!string.IsNullOrEmpty(token) && token != ">" && token != ")")
					{
						string text = " ";
						while (text != ")" && text != ">")
						{
							text = reader.GetNextToken();
							if (!string.IsNullOrEmpty(text) && text != ">" && text != ")" && text != "/")
							{
								token = token + " " + text;
							}
							fieldValues.Add(token);
						}
					}
				}
				token = reader.GetNextToken();
			}
		}
		else
		{
			while (token != ">" && token != ")")
			{
				if (!string.IsNullOrEmpty(token) && (token == "/" || token != ")"))
				{
					token = reader.GetNextToken();
					if (!string.IsNullOrEmpty(token) && token != ">" && token != ")")
					{
						isEmpty = false;
						string text2 = " ";
						while (text2 != ")" && text2 != ">")
						{
							text2 = reader.GetNextToken();
							if (!string.IsNullOrEmpty(text2) && text2 != ">" && text2 != ")" && text2 != "/" && !token.Contains("/"))
							{
								token = token + " " + text2;
							}
							else if ((text2 == "/" || token.Contains("/")) && text2 != ")")
							{
								token += text2;
							}
						}
						if (!table.ContainsKey(fieldName))
						{
							table.Add(fieldName, new string[1] { token });
						}
					}
				}
				if (isEmpty && !table.ContainsKey(fieldName))
				{
					table.Add(fieldName, new string[1] { string.Empty });
				}
				isEmpty = true;
				token = reader.GetNextToken();
			}
		}
		return token;
	}

	public void HighlightFields(bool highlight)
	{
		PdfDictionary pdfDictionary = new PdfDictionary();
		pdfDictionary["Type"] = new PdfName("Action");
		pdfDictionary["S"] = new PdfName("JavaScript");
		if (highlight)
		{
			pdfDictionary["JS"] = new PdfString("app.runtimeHighlight = true;");
		}
		else
		{
			pdfDictionary["JS"] = new PdfString("app.runtimeHighlight = false;");
		}
		CrossTable.Document.Catalog["OpenAction"] = pdfDictionary;
		CrossTable.Document.Catalog.Modify();
	}

	public bool OnlyHexInString(string test)
	{
		return Regex.IsMatch(test, "\\A\\b[0-9a-fA-F]+\\b\\Z");
	}

	public void ImportData(Stream stream, ImportFormSettings settings)
	{
		m_settings = settings;
		if (m_settings.DataFormat == DataFormat.Fdf)
		{
			ImportDataFDF(stream);
		}
		if (m_settings.DataFormat == DataFormat.XFdf)
		{
			ImportDataXFDF(stream);
		}
		if (m_settings.DataFormat == DataFormat.Xml)
		{
			if (m_settings.IgnoreErrors)
			{
				ImportDataXML(stream, continueImportOnError: true);
			}
			else
			{
				ImportDataXML(stream, continueImportOnError: false);
			}
		}
		if (m_settings.DataFormat == DataFormat.Json)
		{
			ImportDataJson(stream);
		}
	}

	internal bool HasAnyFlattenedField()
	{
		if (base.Flatten)
		{
			return true;
		}
		if (Fields != null && Fields.Count > 0)
		{
			foreach (PdfField field in Fields)
			{
				if (field.Flatten)
				{
					return true;
				}
			}
		}
		return false;
	}

	private int CompareKidsElement(PdfReferenceHolder referenceHolder1, PdfReferenceHolder referenceHolder2)
	{
		int result = 0;
		PdfArray rectangle = GetRectangle(referenceHolder1.Object as PdfDictionary);
		PdfArray rectangle2 = GetRectangle(referenceHolder2.Object as PdfDictionary);
		float floatValue = (rectangle[0] as PdfNumber).FloatValue;
		float floatValue2 = (rectangle[1] as PdfNumber).FloatValue;
		float floatValue3 = (rectangle2[0] as PdfNumber).FloatValue;
		float floatValue4 = (rectangle2[1] as PdfNumber).FloatValue;
		if (pdfFormFieldsTabOrder == PdfFormFieldsTabOrder.Row)
		{
			int num = floatValue4.CompareTo(floatValue2);
			result = ((num == 0) ? floatValue.CompareTo(floatValue3) : num);
		}
		else if (pdfFormFieldsTabOrder == PdfFormFieldsTabOrder.Column)
		{
			int num = floatValue.CompareTo(floatValue3);
			result = ((num == 0) ? floatValue4.CompareTo(floatValue2) : num);
		}
		return result;
	}

	private void SortFieldItems(PdfField field)
	{
		PdfLoadedTextBoxField pdfLoadedTextBoxField = field as PdfLoadedTextBoxField;
		PdfLoadedListBoxField pdfLoadedListBoxField = field as PdfLoadedListBoxField;
		PdfLoadedCheckBoxField pdfLoadedCheckBoxField = field as PdfLoadedCheckBoxField;
		PdfLoadedRadioButtonListField pdfLoadedRadioButtonListField = field as PdfLoadedRadioButtonListField;
		if (pdfLoadedTextBoxField != null)
		{
			pdfLoadedTextBoxField.Items.InternalList.Sort((object item1, object item2) => CompareFieldItem(item1, item2));
		}
		else if (pdfLoadedListBoxField != null)
		{
			pdfLoadedListBoxField.Items.InternalList.Sort((object item1, object item2) => CompareFieldItem(item1, item2));
		}
		else if (pdfLoadedCheckBoxField != null)
		{
			pdfLoadedCheckBoxField.Items.InternalList.Sort((object item1, object item2) => CompareFieldItem(item1, item2));
		}
		else
		{
			pdfLoadedRadioButtonListField?.Items.InternalList.Sort((object item1, object item2) => CompareFieldItem(item1, item2));
		}
	}

	private int CompareFieldItem(object item1, object item2)
	{
		PdfLoadedFieldItem pdfLoadedFieldItem = item1 as PdfLoadedFieldItem;
		PdfLoadedFieldItem pdfLoadedFieldItem2 = item2 as PdfLoadedFieldItem;
		int result = 0;
		if (pdfLoadedFieldItem != null && pdfLoadedFieldItem2 != null)
		{
			int pageIndex = pdfLoadedFieldItem.PageIndex;
			int pageIndex2 = pdfLoadedFieldItem2.PageIndex;
			PdfArray rectangle = GetRectangle(pdfLoadedFieldItem.Dictionary);
			PdfArray rectangle2 = GetRectangle(pdfLoadedFieldItem2.Dictionary);
			if (rectangle != null && rectangle2 != null)
			{
				float floatValue = (PdfCrossTable.Dereference(rectangle[0]) as PdfNumber).FloatValue;
				float floatValue2 = (PdfCrossTable.Dereference(rectangle[1]) as PdfNumber).FloatValue;
				float floatValue3 = (PdfCrossTable.Dereference(rectangle2[0]) as PdfNumber).FloatValue;
				float floatValue4 = (PdfCrossTable.Dereference(rectangle2[1]) as PdfNumber).FloatValue;
				if (pdfFormFieldsTabOrder == PdfFormFieldsTabOrder.Row)
				{
					int num = pageIndex.CompareTo(pageIndex2);
					if (num != 0)
					{
						result = num;
					}
					else
					{
						num = floatValue4.CompareTo(floatValue2);
						result = ((num == 0) ? floatValue.CompareTo(floatValue3) : num);
					}
				}
				else if (pdfFormFieldsTabOrder == PdfFormFieldsTabOrder.Column)
				{
					int num = pageIndex.CompareTo(pageIndex2);
					if (num != 0)
					{
						result = num;
					}
					else
					{
						num = floatValue.CompareTo(floatValue3);
						result = ((num == 0) ? floatValue4.CompareTo(floatValue2) : num);
					}
				}
			}
		}
		return result;
	}

	private int CompareField(object formField1, object formField2)
	{
		PdfField pdfField = formField1 as PdfField;
		PdfField pdfField2 = formField2 as PdfField;
		int result = 0;
		if (pdfField.Page is PdfLoadedPage && pdfField2.Page is PdfLoadedPage)
		{
			int num = ((pdfField.Page as PdfLoadedPage).Document as PdfLoadedDocument).Pages.IndexOf(SortItemPageIndex(pdfField, pageTabOrder: false));
			int value = ((pdfField2.Page as PdfLoadedPage).Document as PdfLoadedDocument).Pages.IndexOf(SortItemPageIndex(pdfField2, pageTabOrder: false));
			PdfArray pdfArray = (pdfField.Dictionary.ContainsKey("Kids") ? GetItemRectangle(pdfField.Dictionary, pdfField as PdfLoadedField) : GetRectangle(pdfField.Dictionary));
			PdfArray pdfArray2 = (pdfField2.Dictionary.ContainsKey("Kids") ? GetItemRectangle(pdfField2.Dictionary, pdfField2 as PdfLoadedField) : GetRectangle(pdfField2.Dictionary));
			if (pdfArray != null && pdfArray2 != null)
			{
				float num2 = (PdfCrossTable.Dereference(pdfArray[3]) as PdfNumber).FloatValue - (PdfCrossTable.Dereference(pdfArray[1]) as PdfNumber).FloatValue;
				float num3 = (PdfCrossTable.Dereference(pdfArray2[3]) as PdfNumber).FloatValue - (PdfCrossTable.Dereference(pdfArray2[1]) as PdfNumber).FloatValue;
				float floatValue = (PdfCrossTable.Dereference(pdfArray[0]) as PdfNumber).FloatValue;
				float floatValue2 = (PdfCrossTable.Dereference(pdfArray[1]) as PdfNumber).FloatValue;
				float floatValue3 = (PdfCrossTable.Dereference(pdfArray2[0]) as PdfNumber).FloatValue;
				float floatValue4 = (PdfCrossTable.Dereference(pdfArray2[1]) as PdfNumber).FloatValue;
				int num4 = num.CompareTo(value);
				if (pdfFormFieldsTabOrder == PdfFormFieldsTabOrder.Row)
				{
					int num5 = floatValue4.CompareTo(floatValue2);
					if (num5 != 0 && ((num5 == -1 && floatValue2 > floatValue4 && floatValue2 - num2 / 2f < floatValue4) || (num5 == 1 && floatValue4 > floatValue2 && floatValue4 - num3 / 2f < floatValue2)))
					{
						num5 = 0;
					}
					result = ((num4 != 0) ? num4 : ((num5 == 0) ? floatValue.CompareTo(floatValue3) : num5));
				}
				else if (pdfFormFieldsTabOrder == PdfFormFieldsTabOrder.Column)
				{
					int num5 = floatValue.CompareTo(floatValue3);
					result = ((num4 != 0) ? num4 : ((num5 == 0) ? floatValue4.CompareTo(floatValue2) : num5));
				}
				else if (pdfFormFieldsTabOrder == PdfFormFieldsTabOrder.Manual || pdfFormFieldsTabOrder == PdfFormFieldsTabOrder.Widget || pdfFormFieldsTabOrder == PdfFormFieldsTabOrder.None)
				{
					int num5 = pdfField.TabIndex.CompareTo(pdfField2.TabIndex);
					result = ((num4 == 0) ? num5 : num4);
				}
			}
		}
		return result;
	}

	internal PdfLoadedFormFieldCollection OrderFormFields(PdfFormFieldsTabOrder formFieldsTabOrder)
	{
		pdfFormFieldsTabOrder = formFieldsTabOrder;
		PdfLoadedFormFieldCollection fields = Fields;
		fields.InternalList.Sort((object pdfField1, object pdfField2) => CompareField(pdfField1, pdfField2));
		if (fields != null && fields.Count == 1)
		{
			PdfField field = fields[0];
			SortFieldItems(field);
		}
		if (!OrderIPdfPrimitives(fields, setTabOrder: true, formFieldsTabOrder) && fields[0] != null && fields[0].Page != null && fields[0].Page is PdfLoadedPage && (fields[0].Page as PdfLoadedPage).Document is PdfLoadedDocument pdfLoadedDocument)
		{
			for (int i = 0; i < pdfLoadedDocument.Pages.Count; i++)
			{
				pdfLoadedDocument.Pages[0].FormFieldsTabOrder = formFieldsTabOrder;
			}
		}
		fields.ResetFieldNames();
		return fields;
	}

	private bool OrderIPdfPrimitives(PdfLoadedFormFieldCollection formFieldCollection, bool setTabOrder, PdfFormFieldsTabOrder formFieldsTabOrder)
	{
		bool result = true;
		if (Fields.Form.Dictionary.ContainsKey("Fields") && PdfCrossTable.Dereference(Fields.Form.Dictionary["Fields"]) is PdfArray pdfArray)
		{
			for (int i = 0; i < formFieldCollection.Count; i++)
			{
				if (i >= pdfArray.Count)
				{
					result = false;
					break;
				}
				if (setTabOrder && formFieldCollection[i].Page != null)
				{
					formFieldCollection[i].Page.FormFieldsTabOrder = formFieldsTabOrder;
				}
				if (setTabOrder)
				{
					SetTabOrderToKids(formFieldCollection[i], formFieldsTabOrder);
				}
				int num = pdfArray.Elements.IndexOf(new PdfReferenceHolder(formFieldCollection[i].Dictionary));
				if (num != -1 && i != num)
				{
					IPdfPrimitive pdfPrimitive = pdfArray.Elements[num];
					pdfArray.Elements.Remove(pdfPrimitive);
					pdfArray.Insert(i, pdfPrimitive);
				}
			}
		}
		return result;
	}

	private void SetTabOrderToKids(PdfField field, PdfFormFieldsTabOrder formFieldsTabOrder)
	{
		PdfLoadedTextBoxField pdfLoadedTextBoxField = field as PdfLoadedTextBoxField;
		PdfLoadedListBoxField pdfLoadedListBoxField = field as PdfLoadedListBoxField;
		PdfLoadedCheckBoxField pdfLoadedCheckBoxField = field as PdfLoadedCheckBoxField;
		PdfLoadedRadioButtonListField pdfLoadedRadioButtonListField = field as PdfLoadedRadioButtonListField;
		if (pdfLoadedTextBoxField != null && pdfLoadedTextBoxField.Items != null)
		{
			for (int i = 0; i < pdfLoadedTextBoxField.Items.Count; i++)
			{
				if (pdfLoadedTextBoxField.Items[i].Page != null)
				{
					pdfLoadedTextBoxField.Items[i].Page.FormFieldsTabOrder = formFieldsTabOrder;
				}
			}
		}
		else if (pdfLoadedListBoxField != null && pdfLoadedListBoxField.Items != null)
		{
			for (int j = 0; j < pdfLoadedListBoxField.Items.Count; j++)
			{
				if (pdfLoadedListBoxField.Items[j].Page != null)
				{
					pdfLoadedListBoxField.Items[j].Page.FormFieldsTabOrder = formFieldsTabOrder;
				}
			}
		}
		else if (pdfLoadedCheckBoxField != null && pdfLoadedCheckBoxField.Items != null)
		{
			for (int k = 0; k < pdfLoadedCheckBoxField.Items.Count; k++)
			{
				if (pdfLoadedCheckBoxField.Items[k].Page != null)
				{
					pdfLoadedCheckBoxField.Items[k].Page.FormFieldsTabOrder = formFieldsTabOrder;
				}
			}
		}
		else
		{
			if (pdfLoadedRadioButtonListField == null || pdfLoadedRadioButtonListField.Items == null)
			{
				return;
			}
			for (int l = 0; l < pdfLoadedRadioButtonListField.Items.Count; l++)
			{
				if (pdfLoadedRadioButtonListField.Items[l].Page != null)
				{
					pdfLoadedRadioButtonListField.Items[l].Page.FormFieldsTabOrder = formFieldsTabOrder;
				}
			}
		}
	}

	internal PdfLoadedFormFieldCollection OrderFormFields()
	{
		return OrderFormFields(null);
	}

	internal PdfLoadedFormFieldCollection OrderFormFields(Dictionary<int, PdfFormFieldsTabOrder> tabCollection)
	{
		Dictionary<int, PdfFormFieldsTabOrder> dictionary = null;
		bool flag = true;
		if (tabCollection != null && tabCollection.Count > 0)
		{
			dictionary = tabCollection;
		}
		else
		{
			flag = false;
			dictionary = new Dictionary<int, PdfFormFieldsTabOrder>();
		}
		Dictionary<int, List<PdfField>> dictionary2 = new Dictionary<int, List<PdfField>>();
		PdfLoadedFormFieldCollection fields = Fields;
		if (fields != null && fields.Count > 0 && fields[0].Page != null && fields[0].Page is PdfLoadedPage { Document: not null, Document: PdfLoadedDocument document })
		{
			for (int i = 0; i < fields.Count; i++)
			{
				PdfField pdfField3 = Fields[i];
				if (document.Pages == null || pdfField3.Page == null)
				{
					continue;
				}
				PdfPageBase page = SortItemPageIndex(pdfField3, pageTabOrder: true);
				int num = document.Pages.IndexOf(page);
				if (num >= 0)
				{
					if (dictionary2.ContainsKey(num))
					{
						dictionary2[num].Add(pdfField3);
					}
					else
					{
						List<PdfField> list = new List<PdfField>();
						list.Add(pdfField3);
						dictionary2[num] = list;
					}
					if (!dictionary.ContainsKey(num))
					{
						dictionary[num] = document.Pages[num].FormFieldsTabOrder;
					}
					if (flag)
					{
						document.Pages[num].FormFieldsTabOrder = dictionary[num];
					}
				}
			}
			int num2 = 0;
			foreach (KeyValuePair<int, List<PdfField>> item2 in dictionary2)
			{
				pdfFormFieldsTabOrder = dictionary[item2.Key];
				if (pdfFormFieldsTabOrder != PdfFormFieldsTabOrder.Structure)
				{
					List<PdfField> value = item2.Value;
					value.Sort((PdfField pdfField1, PdfField pdfField2) => CompareField(pdfField1, pdfField2));
					if (value != null && value.Count == 1)
					{
						SortFieldItems(value[0]);
					}
					for (int j = 0; j < value.Count; j++)
					{
						int num3 = fields.IndexOf(value[j]);
						if (num3 != -1 && num3 != num2 + j)
						{
							PdfField item = fields.InternalList[num3] as PdfField;
							fields.InternalList.Remove(item);
							fields.InternalList.Insert(num2 + j, item);
						}
					}
				}
				num2 += item2.Value.Count;
			}
			OrderIPdfPrimitives(fields, setTabOrder: false, PdfFormFieldsTabOrder.None);
		}
		fields.ResetFieldNames();
		return fields;
	}

	private RectangleF GetBounds(PdfField lField)
	{
		PdfLoadedTextBoxField pdfLoadedTextBoxField = lField as PdfLoadedTextBoxField;
		PdfLoadedListBoxField pdfLoadedListBoxField = lField as PdfLoadedListBoxField;
		PdfLoadedCheckBoxField pdfLoadedCheckBoxField = lField as PdfLoadedCheckBoxField;
		PdfLoadedRadioButtonListField pdfLoadedRadioButtonListField = lField as PdfLoadedRadioButtonListField;
		PdfLoadedComboBoxField pdfLoadedComboBoxField = lField as PdfLoadedComboBoxField;
		PdfLoadedSignatureField pdfLoadedSignatureField = lField as PdfLoadedSignatureField;
		PdfLoadedButtonField pdfLoadedButtonField = lField as PdfLoadedButtonField;
		PdfLoadedChoiceField pdfLoadedChoiceField = lField as PdfLoadedChoiceField;
		if (pdfLoadedTextBoxField != null)
		{
			if (pdfLoadedTextBoxField.Items != null && pdfLoadedTextBoxField.Items.Count > 0)
			{
				return pdfLoadedTextBoxField.Items[0].Bounds;
			}
			return pdfLoadedTextBoxField.Bounds;
		}
		if (pdfLoadedListBoxField != null)
		{
			if (pdfLoadedListBoxField.Items != null && pdfLoadedListBoxField.Items.Count > 0)
			{
				return pdfLoadedListBoxField.Items[0].Bounds;
			}
			return pdfLoadedListBoxField.Bounds;
		}
		if (pdfLoadedCheckBoxField != null)
		{
			if (pdfLoadedCheckBoxField.Items != null && pdfLoadedCheckBoxField.Items.Count > 0)
			{
				return pdfLoadedCheckBoxField.Items[0].Bounds;
			}
			return pdfLoadedCheckBoxField.Bounds;
		}
		if (pdfLoadedRadioButtonListField != null)
		{
			if (pdfLoadedRadioButtonListField.Items != null && pdfLoadedRadioButtonListField.Items.Count > 0)
			{
				return pdfLoadedRadioButtonListField.Items[0].Bounds;
			}
			return pdfLoadedRadioButtonListField.Bounds;
		}
		if (pdfLoadedComboBoxField != null)
		{
			if (pdfLoadedComboBoxField.Items != null && pdfLoadedComboBoxField.Items.Count > 0)
			{
				return pdfLoadedComboBoxField.Items[0].Bounds;
			}
			return pdfLoadedComboBoxField.Bounds;
		}
		return pdfLoadedSignatureField?.Bounds ?? pdfLoadedButtonField?.Bounds ?? pdfLoadedChoiceField?.Bounds ?? RectangleF.Empty;
	}

	private PdfPageBase SortItemPageIndex(PdfField lField, bool pageTabOrder)
	{
		PdfPageBase page = lField.Page;
		_ = lField.Dictionary;
		PdfLoadedTextBoxField pdfLoadedTextBoxField = lField as PdfLoadedTextBoxField;
		PdfLoadedListBoxField pdfLoadedListBoxField = lField as PdfLoadedListBoxField;
		PdfLoadedCheckBoxField pdfLoadedCheckBoxField = lField as PdfLoadedCheckBoxField;
		PdfLoadedRadioButtonListField pdfLoadedRadioButtonListField = lField as PdfLoadedRadioButtonListField;
		PdfLoadedComboBoxField pdfLoadedComboBoxField = lField as PdfLoadedComboBoxField;
		PdfLoadedSignatureField pdfLoadedSignatureField = lField as PdfLoadedSignatureField;
		PdfLoadedButtonField pdfLoadedButtonField = lField as PdfLoadedButtonField;
		PdfFormFieldsTabOrder pdfFormFieldsTabOrder = this.pdfFormFieldsTabOrder;
		this.pdfFormFieldsTabOrder = (pageTabOrder ? lField.Page.FormFieldsTabOrder : pdfFormFieldsTabOrder);
		SortFieldItems(lField);
		if (pdfLoadedTextBoxField != null && pdfLoadedTextBoxField.Items != null && pdfLoadedTextBoxField.Items.Count > 1)
		{
			page = pdfLoadedTextBoxField.Items[0].Page;
		}
		else if (pdfLoadedListBoxField != null && pdfLoadedListBoxField.Items != null && pdfLoadedListBoxField.Items.Count > 1)
		{
			page = pdfLoadedListBoxField.Items[0].Page;
		}
		else if (pdfLoadedCheckBoxField != null && pdfLoadedCheckBoxField.Items != null && pdfLoadedCheckBoxField.Items.Count > 1)
		{
			page = pdfLoadedCheckBoxField.Items[0].Page;
		}
		else if (pdfLoadedRadioButtonListField != null && pdfLoadedRadioButtonListField.Items != null && pdfLoadedRadioButtonListField.Items.Count > 1)
		{
			page = pdfLoadedRadioButtonListField.Items[0].Page;
		}
		else if (pdfLoadedComboBoxField != null && pdfLoadedComboBoxField.Items != null && pdfLoadedComboBoxField.Items.Count > 1)
		{
			page = pdfLoadedComboBoxField.Items[0].Page;
		}
		else if (pdfLoadedSignatureField != null && pdfLoadedSignatureField.Items != null && pdfLoadedSignatureField.Items.Count > 1)
		{
			page = pdfLoadedSignatureField.Items[0].Page;
		}
		else if (pdfLoadedButtonField != null && pdfLoadedButtonField.Items != null && pdfLoadedButtonField.Items.Count > 1)
		{
			page = pdfLoadedButtonField.Items[0].Page;
		}
		this.pdfFormFieldsTabOrder = pdfFormFieldsTabOrder;
		if (page == null)
		{
			page = lField.Page;
		}
		return page;
	}

	private PdfArray GetItemRectangle(PdfDictionary rectangle, PdfLoadedField lField)
	{
		if (rectangle.ContainsKey("Kids") && PdfCrossTable.Dereference(rectangle["Kids"]) is PdfArray pdfArray && pdfArray.Elements.Count >= 1)
		{
			if (pdfArray.Count == 1 || lField == null)
			{
				PdfDictionary rectangle2 = PdfCrossTable.Dereference(pdfArray.Elements[0]) as PdfDictionary;
				return GetRectangle(rectangle2);
			}
			PdfLoadedTextBoxField pdfLoadedTextBoxField = lField as PdfLoadedTextBoxField;
			PdfLoadedListBoxField pdfLoadedListBoxField = lField as PdfLoadedListBoxField;
			PdfLoadedCheckBoxField pdfLoadedCheckBoxField = lField as PdfLoadedCheckBoxField;
			PdfLoadedRadioButtonListField pdfLoadedRadioButtonListField = lField as PdfLoadedRadioButtonListField;
			PdfLoadedComboBoxField pdfLoadedComboBoxField = lField as PdfLoadedComboBoxField;
			PdfLoadedSignatureField pdfLoadedSignatureField = lField as PdfLoadedSignatureField;
			PdfLoadedButtonField pdfLoadedButtonField = lField as PdfLoadedButtonField;
			if (pdfLoadedTextBoxField != null && pdfLoadedTextBoxField.Items != null && pdfLoadedTextBoxField.Items.Count > 1)
			{
				{
					IEnumerator enumerator = pdfLoadedTextBoxField.Items.GetEnumerator();
					try
					{
						if (enumerator.MoveNext())
						{
							PdfLoadedFieldItem pdfLoadedFieldItem = (PdfLoadedFieldItem)enumerator.Current;
							return GetRectangle(pdfLoadedFieldItem.Dictionary);
						}
					}
					finally
					{
						IDisposable disposable = enumerator as IDisposable;
						if (disposable != null)
						{
							disposable.Dispose();
						}
					}
				}
			}
			else if (pdfLoadedListBoxField != null && pdfLoadedListBoxField.Items != null && pdfLoadedListBoxField.Items.Count > 1)
			{
				{
					IEnumerator enumerator = pdfLoadedListBoxField.Items.GetEnumerator();
					try
					{
						if (enumerator.MoveNext())
						{
							PdfLoadedFieldItem pdfLoadedFieldItem2 = (PdfLoadedFieldItem)enumerator.Current;
							return GetRectangle(pdfLoadedFieldItem2.Dictionary);
						}
					}
					finally
					{
						IDisposable disposable2 = enumerator as IDisposable;
						if (disposable2 != null)
						{
							disposable2.Dispose();
						}
					}
				}
			}
			else if (pdfLoadedCheckBoxField != null && pdfLoadedCheckBoxField.Items != null && pdfLoadedCheckBoxField.Items.Count > 1)
			{
				{
					IEnumerator enumerator = pdfLoadedCheckBoxField.Items.GetEnumerator();
					try
					{
						if (enumerator.MoveNext())
						{
							PdfLoadedFieldItem pdfLoadedFieldItem3 = (PdfLoadedFieldItem)enumerator.Current;
							return GetRectangle(pdfLoadedFieldItem3.Dictionary);
						}
					}
					finally
					{
						IDisposable disposable3 = enumerator as IDisposable;
						if (disposable3 != null)
						{
							disposable3.Dispose();
						}
					}
				}
			}
			else if (pdfLoadedRadioButtonListField != null && pdfLoadedRadioButtonListField.Items != null && pdfLoadedRadioButtonListField.Items.Count > 1)
			{
				{
					IEnumerator enumerator = pdfLoadedRadioButtonListField.Items.GetEnumerator();
					try
					{
						if (enumerator.MoveNext())
						{
							PdfLoadedFieldItem pdfLoadedFieldItem4 = (PdfLoadedFieldItem)enumerator.Current;
							return GetRectangle(pdfLoadedFieldItem4.Dictionary);
						}
					}
					finally
					{
						IDisposable disposable4 = enumerator as IDisposable;
						if (disposable4 != null)
						{
							disposable4.Dispose();
						}
					}
				}
			}
			else if (pdfLoadedComboBoxField != null && pdfLoadedComboBoxField.Items != null && pdfLoadedComboBoxField.Items.Count > 1)
			{
				{
					IEnumerator enumerator = pdfLoadedComboBoxField.Items.GetEnumerator();
					try
					{
						if (enumerator.MoveNext())
						{
							PdfLoadedFieldItem pdfLoadedFieldItem5 = (PdfLoadedFieldItem)enumerator.Current;
							return GetRectangle(pdfLoadedFieldItem5.Dictionary);
						}
					}
					finally
					{
						IDisposable disposable5 = enumerator as IDisposable;
						if (disposable5 != null)
						{
							disposable5.Dispose();
						}
					}
				}
			}
			else if (pdfLoadedSignatureField != null && pdfLoadedSignatureField.Items != null && pdfLoadedSignatureField.Items.Count > 1)
			{
				{
					IEnumerator enumerator = pdfLoadedSignatureField.Items.GetEnumerator();
					try
					{
						if (enumerator.MoveNext())
						{
							PdfLoadedFieldItem pdfLoadedFieldItem6 = (PdfLoadedFieldItem)enumerator.Current;
							return GetRectangle(pdfLoadedFieldItem6.Dictionary);
						}
					}
					finally
					{
						IDisposable disposable6 = enumerator as IDisposable;
						if (disposable6 != null)
						{
							disposable6.Dispose();
						}
					}
				}
			}
			else
			{
				if (pdfLoadedButtonField == null || pdfLoadedButtonField.Items == null || pdfLoadedButtonField.Items.Count <= 1)
				{
					PdfDictionary rectangle3 = PdfCrossTable.Dereference(pdfArray.Elements[0]) as PdfDictionary;
					return GetRectangle(rectangle3);
				}
				{
					IEnumerator enumerator = pdfLoadedButtonField.Items.GetEnumerator();
					try
					{
						if (enumerator.MoveNext())
						{
							PdfLoadedFieldItem pdfLoadedFieldItem7 = (PdfLoadedFieldItem)enumerator.Current;
							return GetRectangle(pdfLoadedFieldItem7.Dictionary);
						}
					}
					finally
					{
						IDisposable disposable7 = enumerator as IDisposable;
						if (disposable7 != null)
						{
							disposable7.Dispose();
						}
					}
				}
			}
		}
		return null;
	}

	private PdfArray GetRectangle(PdfDictionary rectangle)
	{
		if (rectangle.ContainsKey("Rect"))
		{
			return PdfCrossTable.Dereference(rectangle["Rect"]) as PdfArray;
		}
		return null;
	}
}
