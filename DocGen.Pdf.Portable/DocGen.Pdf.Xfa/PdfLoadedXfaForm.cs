using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using DocGen.Drawing;
using DocGen.Pdf.Parsing;
using DocGen.Pdf.Primitives;
using DocGen.Pdf.Security;

namespace DocGen.Pdf.Xfa;

public class PdfLoadedXfaForm : PdfLoadedXfaField
{
	private PdfDictionary m_catalog;

	private Dictionary<string, PdfStream> m_xfaArray = new Dictionary<string, PdfStream>();

	private PdfArray m_imageArray = new PdfArray();

	private PdfLoadedDocument m_loadedDocument;

	private List<PdfLoadedXfaField> tryGetfields;

	private int m_fieldCount = 1;

	internal XmlWriter dataSetWriter;

	internal XmlDocument xmlDoc;

	internal XmlDocument dataSetDoc;

	private int count;

	private int nodeCount;

	private List<string> m_completefieldNames = new List<string>();

	private float m_width;

	private float m_height;

	private PointF m_location;

	private PdfXfaVisibility m_visibility;

	private bool m_readOnly;

	internal SizeF m_size = SizeF.Empty;

	private PdfLoadedXfaFlowDirection m_flowDirection;

	internal PdfDocument fDocument;

	internal bool is_modified;

	internal PointF currentPoint = PointF.Empty;

	internal float maxHeight;

	internal float maxWidth;

	internal float extraSize;

	internal List<float> columnWidths = new List<float>();

	internal bool isLocationPresent;

	internal PointF startPoint = PointF.Empty;

	internal float trackingHeight;

	private PdfXfaBorder m_border;

	private bool isXfaImport;

	internal SizeF bgSize = SizeF.Empty;

	internal List<float> backgroundHeight = new List<float>();

	internal int bgHeightCounter;

	private string m_originalXFA = string.Empty;

	internal PdfLoadedXfaPageBreak PageBreak;

	internal PointF cStartPoint = PointF.Empty;

	internal PdfLoadedXfaFlowDirection FlowDirection
	{
		get
		{
			return m_flowDirection;
		}
		set
		{
			m_flowDirection = value;
		}
	}

	internal PdfXfaBorder Border
	{
		get
		{
			return m_border;
		}
		set
		{
			if (value != null)
			{
				m_border = value;
			}
		}
	}

	public bool ReadOnly
	{
		get
		{
			return m_readOnly;
		}
		set
		{
			m_readOnly = value;
		}
	}

	public new PdfXfaVisibility Visibility
	{
		get
		{
			return m_visibility;
		}
		set
		{
			m_visibility = value;
		}
	}

	public float Width
	{
		get
		{
			return m_width;
		}
		set
		{
			m_width = value;
		}
	}

	public float Height
	{
		get
		{
			return m_height;
		}
		set
		{
			m_height = value;
		}
	}

	public PointF Location
	{
		get
		{
			return m_location;
		}
		set
		{
			m_location = value;
		}
	}

	public PdfLoadedXfaFieldCollection Fields
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

	internal PdfDictionary Catalog
	{
		get
		{
			return m_catalog;
		}
		set
		{
			m_catalog = value;
		}
	}

	internal Dictionary<string, PdfStream> XFAArray
	{
		get
		{
			return m_xfaArray;
		}
		set
		{
			m_xfaArray = value;
		}
	}

	public string[] FieldNames => m_fieldNames.ToArray();

	public string[] SubFormNames => m_subFormNames.ToArray();

	public string[] CompleteFieldNames => m_completefieldNames.ToArray();

	public string[] AreaNames => m_areaNames.ToArray();

	internal void Load(PdfCatalog catalog)
	{
		Catalog = catalog;
		if (catalog != null && catalog.ContainsKey("AcroForm"))
		{
			PdfDictionary pdfDictionary = null;
			if (catalog["AcroForm"] is PdfReferenceHolder)
			{
				pdfDictionary = (catalog["AcroForm"] as PdfReferenceHolder).Object as PdfDictionary;
			}
			else if (catalog["AcroForm"] is PdfDictionary)
			{
				pdfDictionary = catalog["AcroForm"] as PdfDictionary;
			}
			if (pdfDictionary != null && pdfDictionary.ContainsKey("XFA") && pdfDictionary["XFA"] is PdfArray pdfArray)
			{
				for (int i = 0; i < pdfArray.Count; i += 2)
				{
					if (!(pdfArray[i] is PdfString))
					{
						continue;
					}
					string value = (pdfArray[i] as PdfString).Value;
					PdfReferenceHolder pdfReferenceHolder = pdfArray[i + 1] as PdfReferenceHolder;
					if (pdfReferenceHolder != null && pdfReferenceHolder.Object is PdfStream pdfStream)
					{
						if (pdfReferenceHolder.Reference != null && catalog.LoadedDocument != null && catalog.LoadedDocument.WasEncrypted && !pdfStream.Decrypted)
						{
							pdfStream.Decrypt(catalog.LoadedDocument.CrossTable.Encryptor, pdfReferenceHolder.Reference.ObjNum);
						}
						if (value == "template" || value == "datasets")
						{
							pdfStream.Clone(pdfStream.CrossTable);
							XFAArray.Add(value, pdfStream.ClonedObject as PdfStream);
						}
						else if (!XFAArray.ContainsKey(value))
						{
							XFAArray.Add(value, pdfStream);
						}
						else
						{
							XFAArray.Add(GetName(value), pdfStream);
						}
					}
				}
			}
		}
		if (m_xfaArray.Count > 0)
		{
			byte[] decompressedData = m_xfaArray["template"].GetDecompressedData();
			xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(Encoding.UTF8.GetString(decompressedData, 0, decompressedData.Length));
			m_originalXFA = xmlDoc.InnerText;
			if (m_xfaArray.ContainsKey("datasets"))
			{
				byte[] decompressedData2 = m_xfaArray["datasets"].GetDecompressedData();
				dataSetDoc = new XmlDocument();
				dataSetDoc.LoadXml(Encoding.UTF8.GetString(decompressedData2, 0, decompressedData2.Length));
			}
			Fields.parent = this;
			ReadForm();
		}
	}

	internal void Save(PdfLoadedDocument document)
	{
		acroForm = document.Form;
		if (acroForm != null)
		{
			acroForm.SetDefaultAppearance(applyDefault: false);
		}
		if (m_xfaArray.Count <= 0)
		{
			return;
		}
		m_loadedDocument = document;
		PdfStream pdfStream = new PdfStream();
		XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
		xmlWriterSettings.Indent = true;
		xmlWriterSettings.OmitXmlDeclaration = true;
		xmlWriterSettings.Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
		dataSetWriter = XmlWriter.Create(pdfStream.InternalStream, xmlWriterSettings);
		dataSetWriter.WriteStartElement("xfa", "datasets", "http://www.xfa.org/schema/xfa-data/1.0/");
		dataSetWriter.WriteStartElement("xfa", "data", null);
		SaveForm();
		dataSetWriter.WriteEndElement();
		dataSetWriter.WriteEndElement();
		dataSetWriter.Close();
		if (m_xfaArray.ContainsKey("datasets"))
		{
			if (isXfaImport)
			{
				pdfStream.Dispose();
				pdfStream = new PdfStream();
				string innerXml = dataSetDoc.InnerXml;
				byte[] bytes = Encoding.UTF8.GetBytes(innerXml);
				pdfStream.InternalStream = new MemoryStream(bytes);
			}
			m_xfaArray["datasets"] = pdfStream;
		}
		else
		{
			Dictionary<string, PdfStream> dictionary = new Dictionary<string, PdfStream>();
			foreach (KeyValuePair<string, PdfStream> item in m_xfaArray)
			{
				dictionary.Add(item.Key, item.Value);
				if (item.Key == "template")
				{
					dictionary.Add("datasets", pdfStream);
				}
			}
			m_xfaArray = dictionary;
		}
		is_modified = (is_modified ? (m_originalXFA != xmlDoc.InnerText) : is_modified);
		PdfStream pdfStream2 = new PdfStream();
		pdfStream2.Write(xmlDoc.InnerXml);
		m_xfaArray["template"] = pdfStream2;
		if (m_loadedDocument != null && !isXfaImport && is_modified && (m_loadedDocument.Security.Permissions & PdfPermissionsFlags.FillFields) == PdfPermissionsFlags.FillFields && Catalog.ContainsKey("Perms"))
		{
			PdfDictionary pdfDictionary = null;
			pdfDictionary = ((!(Catalog["Perms"] is PdfReferenceHolder)) ? (Catalog["Perms"] as PdfDictionary) : ((Catalog["Perms"] as PdfReferenceHolder).Object as PdfDictionary));
			if (pdfDictionary.ContainsKey("UR3"))
			{
				pdfDictionary.Remove("UR3");
			}
		}
		PdfDictionary pdfDictionary2 = new PdfDictionary();
		pdfDictionary2.SetProperty("Names", m_imageArray);
		if (!m_loadedDocument.Catalog.ContainsKey("Names"))
		{
			PdfDictionary pdfDictionary3 = new PdfDictionary();
			pdfDictionary3.SetProperty("XFAImages", new PdfReferenceHolder(pdfDictionary2));
			m_loadedDocument.Catalog.SetProperty("Names", new PdfReferenceHolder(pdfDictionary3));
		}
		else
		{
			PdfDictionary pdfDictionary4 = (m_catalog["Names"] as PdfReferenceHolder).Object as PdfDictionary;
			if (pdfDictionary4.ContainsKey("XFAImages"))
			{
				PdfArray pdfArray = ((pdfDictionary4["XFAImages"] as PdfReferenceHolder).Object as PdfDictionary)["Names"] as PdfArray;
				for (int i = 0; i < m_imageArray.Count; i++)
				{
					pdfArray.Add(m_imageArray[i]);
				}
			}
			else if (m_imageArray.Count > 0)
			{
				pdfDictionary4.SetProperty("XFAImages", pdfDictionary2);
			}
		}
		if (!m_loadedDocument.Form.Dictionary.ContainsKey("XFA"))
		{
			return;
		}
		new PdfResources();
		PdfArray pdfArray2 = new PdfArray();
		foreach (KeyValuePair<string, PdfStream> item2 in m_xfaArray)
		{
			if (item2.Key.Contains("[") && item2.Key.Contains("]"))
			{
				pdfArray2.Add(new PdfString(item2.Key.Split('[')[0]));
			}
			else
			{
				pdfArray2.Add(new PdfString(item2.Key));
			}
			pdfArray2.Add(new PdfReferenceHolder(item2.Value));
		}
		PdfDictionary catalog = m_catalog;
		if (catalog != null && catalog.ContainsKey("AcroForm"))
		{
			PdfDictionary pdfDictionary5 = null;
			if (catalog["AcroForm"] is PdfReferenceHolder)
			{
				PdfReferenceHolder obj = catalog["AcroForm"] as PdfReferenceHolder;
				obj.IsSaving = true;
				pdfDictionary5 = obj.Object as PdfDictionary;
			}
			else if (catalog["AcroForm"] is PdfDictionary)
			{
				pdfDictionary5 = catalog["AcroForm"] as PdfDictionary;
			}
			if (pdfDictionary5 != null && pdfDictionary5.ContainsKey("XFA"))
			{
				pdfDictionary5.Remove("XFA");
				pdfDictionary5.SetProperty("XFA", pdfArray2);
			}
		}
	}

	internal void SaveForm()
	{
		dataSetWriter.WriteStartElement(base.Name);
		SaveAttributes(this);
		SaveSubForms(this, dataSetWriter);
		dataSetWriter.WriteEndElement();
	}

	internal void SaveSubForms(PdfLoadedXfaForm form, XmlWriter dataSetWriter)
	{
		foreach (KeyValuePair<string, PdfXfaField> item in form.Fields.FieldCollection)
		{
			if (item.Value is PdfLoadedXfaForm)
			{
				PdfLoadedXfaForm pdfLoadedXfaForm = item.Value as PdfLoadedXfaForm;
				pdfLoadedXfaForm.acroForm = form.acroForm;
				SaveAttributes(pdfLoadedXfaForm);
				bool flag = true;
				if (dataSetDoc != null)
				{
					string[] array = pdfLoadedXfaForm.nodeName.Split('[');
					string text = string.Empty;
					string[] array2 = array;
					foreach (string text2 in array2)
					{
						if (text2.Contains("]"))
						{
							int num = text2.IndexOf(']') + 2;
							if (text2.Length > num)
							{
								text = text + "/" + text2.Substring(num);
							}
						}
						else
						{
							text += text2;
						}
					}
					while (text.Contains("#"))
					{
						int num2 = text.IndexOf("#");
						if (num2 != -1)
						{
							string text3 = text.Substring(0, num2 - 1);
							string text4 = text.Substring(num2);
							int num3 = text4.IndexOf("/");
							string text5 = string.Empty;
							if (num3 != -1)
							{
								text5 = text4.Substring(num3);
							}
							text = text3 + text5;
							text = text.TrimEnd(new char[1] { '/' });
						}
					}
					text = "//" + text;
					if (dataSetDoc.SelectSingleNode(text) == null)
					{
						flag = false;
					}
				}
				if (!string.IsNullOrEmpty(pdfLoadedXfaForm.Name) && !pdfLoadedXfaForm.Name.Contains("#subform") && flag)
				{
					dataSetWriter.WriteStartElement(pdfLoadedXfaForm.Name);
				}
				pdfLoadedXfaForm.dataSetDoc = form.dataSetDoc;
				form.SaveSubForms(pdfLoadedXfaForm, dataSetWriter);
				if (!string.IsNullOrEmpty(pdfLoadedXfaForm.Name) && !pdfLoadedXfaForm.Name.Contains("#subform") && flag)
				{
					dataSetWriter.WriteEndElement();
				}
			}
			else if (item.Value is PdfLoadedXfaArea)
			{
				PdfLoadedXfaArea pdfLoadedXfaArea = item.Value as PdfLoadedXfaArea;
				pdfLoadedXfaArea.acroForm = form.acroForm;
				bool flag2 = true;
				if (dataSetDoc != null)
				{
					string[] array3 = pdfLoadedXfaArea.nodeName.Split('[');
					string text6 = string.Empty;
					string[] array2 = array3;
					foreach (string text7 in array2)
					{
						if (text7.Contains("]"))
						{
							int num4 = text7.IndexOf(']') + 2;
							if (text7.Length > num4)
							{
								text6 = text6 + "/" + text7.Substring(num4);
							}
						}
						else
						{
							text6 += text7;
						}
					}
					while (text6.Contains("#"))
					{
						int num5 = text6.IndexOf("#");
						if (num5 != -1)
						{
							string text8 = text6.Substring(0, num5 - 1);
							string text9 = text6.Substring(num5);
							int num6 = text9.IndexOf("/");
							string text10 = string.Empty;
							if (num6 != -1)
							{
								text10 = text9.Substring(num6);
							}
							text6 = text8 + text10;
							text6 = text6.TrimEnd(new char[1] { '/' });
						}
					}
					text6 = "//" + text6;
					if (dataSetDoc.SelectSingleNode(text6) == null)
					{
						flag2 = false;
					}
				}
				if (!string.IsNullOrEmpty(pdfLoadedXfaArea.Name) && !pdfLoadedXfaArea.Name.Contains("#area") && flag2)
				{
					dataSetWriter.WriteStartElement(pdfLoadedXfaArea.Name);
				}
				pdfLoadedXfaArea.Save(pdfLoadedXfaArea, dataSetWriter, dataSetDoc, form);
				if (!string.IsNullOrEmpty(pdfLoadedXfaArea.Name) && !pdfLoadedXfaArea.Name.Contains("#area") && flag2)
				{
					dataSetWriter.WriteEndElement();
				}
			}
			else if (item.Value is PdfLoadedXfaTextBoxField)
			{
				if (item.Value is PdfLoadedXfaTextBoxField pdfLoadedXfaTextBoxField)
				{
					pdfLoadedXfaTextBoxField.Fill(dataSetWriter, form);
				}
			}
			else if (item.Value is PdfLoadedXfaNumericField)
			{
				if (item.Value is PdfLoadedXfaNumericField pdfLoadedXfaNumericField)
				{
					pdfLoadedXfaNumericField.Fill(dataSetWriter, form);
				}
			}
			else if (item.Value is PdfLoadedXfaDateTimeField)
			{
				if (item.Value is PdfLoadedXfaDateTimeField pdfLoadedXfaDateTimeField)
				{
					pdfLoadedXfaDateTimeField.Fill(dataSetWriter, form);
				}
			}
			else if (item.Value is PdfLoadedXfaCheckBoxField)
			{
				if (item.Value is PdfLoadedXfaCheckBoxField pdfLoadedXfaCheckBoxField)
				{
					pdfLoadedXfaCheckBoxField.Fill(dataSetWriter, form);
				}
			}
			else if (item.Value is PdfLoadedXfaRadioButtonGroup)
			{
				if (item.Value is PdfLoadedXfaRadioButtonGroup pdfLoadedXfaRadioButtonGroup)
				{
					pdfLoadedXfaRadioButtonGroup.Fill(dataSetWriter, form);
				}
			}
			else if (item.Value is PdfLoadedXfaListBoxField)
			{
				if (item.Value is PdfLoadedXfaListBoxField pdfLoadedXfaListBoxField)
				{
					pdfLoadedXfaListBoxField.Fill(dataSetWriter, form);
				}
			}
			else if (item.Value is PdfLoadedXfaComboBoxField)
			{
				if (item.Value is PdfLoadedXfaComboBoxField pdfLoadedXfaComboBoxField)
				{
					pdfLoadedXfaComboBoxField.Fill(dataSetWriter, form);
				}
			}
			else if (item.Value != null && !(item.Value is PdfLoadedXfaField))
			{
				SaveNewXfaItems(form, item.Value);
			}
		}
	}

	internal List<PdfLoadedXfaCheckBoxField> GetSameNameFields(string name, PdfLoadedXfaForm form)
	{
		List<PdfLoadedXfaCheckBoxField> list = new List<PdfLoadedXfaCheckBoxField>();
		string[] fieldNames = form.FieldNames;
		foreach (string text in fieldNames)
		{
			if (text.Contains(name) && form.Fields[text] is PdfLoadedXfaCheckBoxField item)
			{
				list.Add(item);
			}
		}
		return list;
	}

	internal void ReadForm()
	{
		XmlNode xmlNode = xmlDoc.GetElementsByTagName("subform")[0];
		ReadSubFormProperties(xmlNode, this, null);
		currentNode = (XmlElement)xmlNode;
		SetName(this, internalSubFormNames, isArea: false);
		ReadSubForm(xmlNode, this, m_fieldNames, m_subFormNames);
		GetCompeleteFieldNames(this);
	}

	private void ReadColumnWidth(XmlNode node, PdfLoadedXfaForm subform)
	{
		string value = node.Attributes["columnWidths"].Value;
		if (value != string.Empty)
		{
			string[] array = value.Split(' ');
			foreach (string value2 in array)
			{
				subform.columnWidths.Add(ConvertToPoint(value2));
			}
		}
	}

	internal void ReadSubFormProperties(XmlNode node, PdfLoadedXfaForm ff, PdfLoadedXfaForm form)
	{
		if (node.Attributes["x"] != null)
		{
			ff.Location = new PointF(ConvertToPoint(node.Attributes["x"].Value), ff.Location.Y);
			isLocationPresent = true;
		}
		if (node.Attributes["y"] != null)
		{
			ff.Location = new PointF(ff.Location.X, ConvertToPoint(node.Attributes["y"].Value));
			isLocationPresent = true;
		}
		if (node.Attributes["w"] != null)
		{
			ff.Width = ConvertToPoint(node.Attributes["w"].Value);
		}
		if (node.Attributes["h"] != null)
		{
			ff.Height = ConvertToPoint(node.Attributes["h"].Value);
		}
		if (node.Attributes["layout"] != null)
		{
			switch (node.Attributes["layout"].Value)
			{
			case "tb":
				ff.FlowDirection = PdfLoadedXfaFlowDirection.TopToBottom;
				break;
			case "lr-tb":
				ff.FlowDirection = PdfLoadedXfaFlowDirection.LeftToRight;
				break;
			case "rl-tb":
				ff.FlowDirection = PdfLoadedXfaFlowDirection.RightToLeft;
				break;
			case "row":
				ff.FlowDirection = PdfLoadedXfaFlowDirection.Row;
				break;
			case "table":
				ff.FlowDirection = PdfLoadedXfaFlowDirection.Table;
				break;
			default:
				ff.FlowDirection = PdfLoadedXfaFlowDirection.None;
				break;
			}
		}
		if (node.Attributes["columnWidths"] != null)
		{
			ReadColumnWidth(node, ff);
		}
		if (node["bind"] != null)
		{
			if (form != null && form.bindingName != string.Empty)
			{
				ff.bindingName = form.bindingName + "." + ReadBinding(node["bind"], ff);
			}
			else
			{
				ff.bindingName = ReadBinding(node["bind"], ff);
			}
		}
		if (node["margin"] != null)
		{
			ff.ReadMargin(node["margin"]);
		}
		if (node["border"] != null)
		{
			ff.m_border = new PdfXfaBorder();
			ff.m_border.Read(node["border"]);
		}
	}

	internal void ReadSubForm(XmlNode subNode, PdfLoadedXfaForm form, List<string> fieldsNames, List<string> subFormNames)
	{
		foreach (XmlNode item in subNode)
		{
			switch (item.Name)
			{
			case "subformSet":
			case "subform":
			{
				PdfLoadedXfaForm pdfLoadedXfaForm = new PdfLoadedXfaForm();
				pdfLoadedXfaForm.Fields.parent = pdfLoadedXfaForm;
				pdfLoadedXfaForm.parent = form;
				pdfLoadedXfaForm.currentNode = item;
				SetName(pdfLoadedXfaForm, form.internalSubFormNames, isArea: false);
				ReadSubFormProperties(item, pdfLoadedXfaForm, form);
				if (!form.internalSubFormNames.Contains(pdfLoadedXfaForm.m_name))
				{
					form.internalSubFormNames.Add(pdfLoadedXfaForm.m_name);
				}
				if (pdfLoadedXfaForm.isUnNamedSubForm)
				{
					pdfLoadedXfaForm.internalFieldNames = form.internalFieldNames;
					pdfLoadedXfaForm.internalSubFormNames = form.internalSubFormNames;
				}
				ReadSubForm(item, pdfLoadedXfaForm, fieldsNames, subFormNames);
				form.Fields.Add(pdfLoadedXfaForm, pdfLoadedXfaForm.m_name);
				form.m_subFormNames.Add(pdfLoadedXfaForm.m_name);
				if (form != this)
				{
					subFormNames.Add(pdfLoadedXfaForm.m_name);
				}
				if (pdfLoadedXfaForm.isUnNamedSubForm)
				{
					form.internalFieldNames = pdfLoadedXfaForm.internalFieldNames;
					form.internalSubFormNames = pdfLoadedXfaForm.internalSubFormNames;
				}
				break;
			}
			case "field":
				ReadField(item, form, fieldsNames, subFormNames, dataSetDoc);
				break;
			case "draw":
				ReadStaticField(item, form, dataSetDoc);
				break;
			case "exclGroup":
			{
				PdfLoadedXfaRadioButtonGroup pdfLoadedXfaRadioButtonGroup = new PdfLoadedXfaRadioButtonGroup();
				pdfLoadedXfaRadioButtonGroup.parent = form;
				pdfLoadedXfaRadioButtonGroup.ReadField(item, dataSetDoc);
				string fieldName = GetFieldName(form.internalFieldNames, pdfLoadedXfaRadioButtonGroup.Name);
				form.Fields.Add(pdfLoadedXfaRadioButtonGroup, fieldName);
				form.m_fieldNames.Add(fieldName);
				pdfLoadedXfaRadioButtonGroup.nodeName = pdfLoadedXfaRadioButtonGroup.nodeName + "." + fieldName;
				fieldsNames.Add(fieldName);
				if (!form.internalFieldNames.Contains(fieldName))
				{
					form.internalFieldNames.Add(fieldName);
				}
				break;
			}
			case "area":
			{
				PdfLoadedXfaArea pdfLoadedXfaArea = new PdfLoadedXfaArea();
				pdfLoadedXfaArea.Fields.parent = pdfLoadedXfaArea;
				pdfLoadedXfaArea.parent = form;
				pdfLoadedXfaArea.currentNode = item;
				SetName(pdfLoadedXfaArea, form.internalSubFormNames, isArea: true);
				if (!form.m_subFormNames.Contains(pdfLoadedXfaArea.m_name))
				{
					form.internalSubFormNames.Add(pdfLoadedXfaArea.m_name);
				}
				pdfLoadedXfaArea.internalFieldNames = form.internalFieldNames;
				pdfLoadedXfaArea.internalSubFormNames = form.internalSubFormNames;
				pdfLoadedXfaArea.ReadArea(item, pdfLoadedXfaArea, form, fieldsNames, subFormNames, dataSetDoc);
				form.internalFieldNames = pdfLoadedXfaArea.internalFieldNames;
				form.internalSubFormNames = pdfLoadedXfaArea.internalSubFormNames;
				form.Fields.Add(pdfLoadedXfaArea, pdfLoadedXfaArea.m_name);
				form.m_areaNames.Add(pdfLoadedXfaArea.m_name);
				break;
			}
			case "breakAfter":
			case "break":
			case "overflow":
			case "breakBefore":
				form.PageBreak = new PdfLoadedXfaPageBreak();
				form.PageBreak.Read(item);
				break;
			}
		}
	}

	public PdfLoadedXfaField[] TryGetFieldsByName(string name)
	{
		tryGetfields = new List<PdfLoadedXfaField>();
		GetFields(name, this, isTryGetFlag: false);
		is_modified = true;
		return tryGetfields.ToArray();
	}

	public PdfLoadedXfaField TryGetFieldByCompleteName(string name)
	{
		name = name.Replace("].", "]/");
		string[] array = name.Split('/');
		PdfLoadedXfaField result = null;
		PdfLoadedXfaField pdfLoadedXfaField = this;
		is_modified = true;
		for (int i = 1; i < array.Length; i++)
		{
			PdfLoadedXfaField[] array2 = (pdfLoadedXfaField as PdfLoadedXfaForm).TryGetFieldsByName(array[i], isTryGetFlag: true);
			if (array2.Length != 0)
			{
				if (array2[0] is PdfLoadedXfaForm)
				{
					pdfLoadedXfaField = array2[0] as PdfLoadedXfaForm;
					continue;
				}
				return array2[0];
			}
			return null;
		}
		return result;
	}

	public void ImportXfaData(Stream stream)
	{
		if (stream != null && dataSetDoc != null)
		{
			isXfaImport = true;
			dataSetDoc.Load(stream);
		}
	}

	public void ExportXfaData(Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		if (dataSetDoc != null)
		{
			dataSetDoc.Save(stream);
		}
	}

	private string GetName(string name)
	{
		string text = name;
		int num = 0;
		do
		{
			text = name + "[" + num++ + "]";
		}
		while (XFAArray.ContainsKey(text));
		return text;
	}

	private void SaveAttributes(PdfLoadedXfaForm lForm)
	{
		if (lForm.ReadOnly)
		{
			if (lForm.currentNode.Attributes["access"] != null)
			{
				lForm.currentNode.Attributes["access"].Value = "readOnly";
				return;
			}
			XmlAttribute xmlAttribute = xmlDoc.CreateAttribute("access");
			xmlAttribute.InnerText = "readOnly";
			lForm.currentNode.Attributes.Append(xmlAttribute);
		}
	}

	private void GetCompeleteFieldNames(PdfLoadedXfaForm form)
	{
		foreach (KeyValuePair<string, PdfXfaField> item in form.Fields.FieldCollection)
		{
			if (item.Value is PdfLoadedXfaForm)
			{
				GetCompeleteFieldNames(item.Value as PdfLoadedXfaForm);
			}
			else if (item.Value is PdfLoadedXfaArea)
			{
				GetCompeleteFieldNames(item.Value as PdfLoadedXfaArea);
			}
			else
			{
				m_completefieldNames.Add((item.Value as PdfLoadedXfaField).nodeName);
			}
		}
	}

	private void GetCompeleteFieldNames(PdfLoadedXfaArea form)
	{
		foreach (KeyValuePair<string, PdfXfaField> item in form.Fields.FieldCollection)
		{
			if (item.Value is PdfLoadedXfaForm)
			{
				GetCompeleteFieldNames(item.Value as PdfLoadedXfaForm);
			}
			else if (item.Value is PdfLoadedXfaArea)
			{
				GetCompeleteFieldNames(item.Value as PdfLoadedXfaArea);
			}
			else
			{
				m_completefieldNames.Add((item.Value as PdfLoadedXfaField).nodeName);
			}
		}
	}

	private void GetFields(string name, PdfLoadedXfaForm form, bool isTryGetFlag)
	{
		foreach (KeyValuePair<string, PdfXfaField> item in form.Fields.FieldCollection)
		{
			if (item.Key == name)
			{
				tryGetfields.Add(item.Value as PdfLoadedXfaField);
			}
			if (item.Value is PdfLoadedXfaForm && !isTryGetFlag)
			{
				PdfLoadedXfaForm pdfLoadedXfaForm = item.Value as PdfLoadedXfaForm;
				if (pdfLoadedXfaForm.Name == name)
				{
					tryGetfields.Add(pdfLoadedXfaForm);
				}
				GetFields(name, pdfLoadedXfaForm, isTryGetFlag);
			}
		}
	}

	private void SaveNewXfaItems(PdfLoadedXfaForm loadedXfa, PdfXfaField field)
	{
		XfaWriter xfaWriter = new XfaWriter();
		XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
		xmlWriterSettings.Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
		xmlWriterSettings.OmitXmlDeclaration = true;
		xmlWriterSettings.Indent = true;
		MemoryStream memoryStream = new MemoryStream();
		xfaWriter.Write = XmlWriter.Create(memoryStream, xmlWriterSettings);
		if (field is PdfXfaImage)
		{
			PdfXfaImage pdfXfaImage = field as PdfXfaImage;
			string text = Guid.NewGuid().ToString();
			if (!pdfXfaImage.isBase64Type)
			{
				m_imageArray.Add(new PdfString(text));
				m_imageArray.Add(new PdfReferenceHolder(pdfXfaImage.ImageStream));
			}
			pdfXfaImage.Save(m_fieldCount++, text, xfaWriter);
		}
		else if (field is PdfXfaTextBoxField)
		{
			PdfXfaTextBoxField pdfXfaTextBoxField = field as PdfXfaTextBoxField;
			dataSetWriter.WriteStartElement(pdfXfaTextBoxField.Name);
			dataSetWriter.WriteString(pdfXfaTextBoxField.Text);
			dataSetWriter.WriteEndElement();
			pdfXfaTextBoxField.Save(xfaWriter);
		}
		else if (field is PdfXfaForm)
		{
			PdfXfaForm obj = field as PdfXfaForm;
			obj.m_dataSetWriter = dataSetWriter;
			obj.AddSubForm(xfaWriter);
		}
		else if (field is PdfXfaCheckBoxField)
		{
			PdfXfaCheckBoxField pdfXfaCheckBoxField = field as PdfXfaCheckBoxField;
			dataSetWriter.WriteStartElement(pdfXfaCheckBoxField.Name);
			if (pdfXfaCheckBoxField.IsChecked)
			{
				dataSetWriter.WriteString("1");
			}
			else
			{
				dataSetWriter.WriteString("0");
			}
			dataSetWriter.WriteEndElement();
			pdfXfaCheckBoxField.Save(xfaWriter);
		}
		else if (field is PdfXfaRadioButtonGroup)
		{
			PdfXfaRadioButtonGroup pdfXfaRadioButtonGroup = field as PdfXfaRadioButtonGroup;
			dataSetWriter.WriteStartElement(pdfXfaRadioButtonGroup.Name);
			pdfXfaRadioButtonGroup.Save(xfaWriter);
			if (pdfXfaRadioButtonGroup.selectedItem > 0)
			{
				dataSetWriter.WriteString(pdfXfaRadioButtonGroup.selectedItem.ToString());
			}
			dataSetWriter.WriteEndElement();
		}
		else if (field is PdfXfaComboBoxField)
		{
			PdfXfaComboBoxField pdfXfaComboBoxField = field as PdfXfaComboBoxField;
			dataSetWriter.WriteStartElement(pdfXfaComboBoxField.Name);
			if (pdfXfaComboBoxField.SelectedValue != null)
			{
				if (pdfXfaComboBoxField.Items.Contains(pdfXfaComboBoxField.SelectedValue))
				{
					dataSetWriter.WriteString(pdfXfaComboBoxField.SelectedValue);
				}
			}
			else if (pdfXfaComboBoxField.SelectedIndex > 0 && pdfXfaComboBoxField.SelectedIndex - 1 <= pdfXfaComboBoxField.Items.Count)
			{
				dataSetWriter.WriteString(pdfXfaComboBoxField.Items[pdfXfaComboBoxField.SelectedIndex - 1]);
			}
			dataSetWriter.WriteEndElement();
			pdfXfaComboBoxField.Save(xfaWriter);
		}
		else if (field is PdfXfaListBoxField)
		{
			PdfXfaListBoxField pdfXfaListBoxField = field as PdfXfaListBoxField;
			dataSetWriter.WriteStartElement(pdfXfaListBoxField.Name);
			if (pdfXfaListBoxField.SelectedValue != null)
			{
				if (pdfXfaListBoxField.Items.Contains(pdfXfaListBoxField.SelectedValue))
				{
					dataSetWriter.WriteString(pdfXfaListBoxField.SelectedValue);
				}
			}
			else if (pdfXfaListBoxField.SelectedIndex > 0 && pdfXfaListBoxField.SelectedIndex - 1 <= pdfXfaListBoxField.Items.Count)
			{
				dataSetWriter.WriteString(pdfXfaListBoxField.Items[pdfXfaListBoxField.SelectedIndex - 1]);
			}
			dataSetWriter.WriteEndElement();
			pdfXfaListBoxField.Save(xfaWriter);
		}
		else if (field is PdfXfaCircleField)
		{
			(field as PdfXfaCircleField).Save(xfaWriter);
		}
		else if (field is PdfXfaRectangleField)
		{
			(field as PdfXfaRectangleField).Save(xfaWriter);
		}
		else if (field is PdfXfaLine)
		{
			(field as PdfXfaLine).Save(xfaWriter);
		}
		else if (field is PdfXfaDateTimeField)
		{
			PdfXfaDateTimeField pdfXfaDateTimeField = field as PdfXfaDateTimeField;
			_ = pdfXfaDateTimeField.Value;
			dataSetWriter.WriteStartElement(pdfXfaDateTimeField.Name);
			if (pdfXfaDateTimeField.Format == PdfXfaDateTimeFormat.Date)
			{
				dataSetWriter.WriteString(pdfXfaDateTimeField.Value.ToString(xfaWriter.GetDatePattern(pdfXfaDateTimeField.DatePattern)));
			}
			else if (pdfXfaDateTimeField.Format == PdfXfaDateTimeFormat.DateTime)
			{
				dataSetWriter.WriteString(pdfXfaDateTimeField.Value.ToString(xfaWriter.GetDateTimePattern(pdfXfaDateTimeField.DatePattern, pdfXfaDateTimeField.TimePattern)));
			}
			else if (pdfXfaDateTimeField.Format == PdfXfaDateTimeFormat.Time)
			{
				dataSetWriter.WriteString(pdfXfaDateTimeField.Value.ToString(xfaWriter.GetTimePattern(pdfXfaDateTimeField.TimePattern)));
			}
			dataSetWriter.WriteEndElement();
			pdfXfaDateTimeField.Save(xfaWriter);
		}
		else if (field is PdfXfaNumericField)
		{
			PdfXfaNumericField pdfXfaNumericField = field as PdfXfaNumericField;
			dataSetWriter.WriteStartElement(pdfXfaNumericField.Name);
			if (!double.IsNaN(pdfXfaNumericField.NumericValue))
			{
				if (pdfXfaNumericField.FieldType == PdfXfaNumericType.Integer)
				{
					pdfXfaNumericField.NumericValue = (int)pdfXfaNumericField.NumericValue;
				}
				dataSetWriter.WriteString(pdfXfaNumericField.NumericValue.ToString());
			}
			dataSetWriter.WriteEndElement();
			pdfXfaNumericField.Save(xfaWriter);
		}
		else if (field is PdfXfaTextElement)
		{
			(field as PdfXfaTextElement).Save(xfaWriter);
		}
		else if (field is PdfXfaButtonField)
		{
			(field as PdfXfaButtonField).Save(xfaWriter);
		}
		xfaWriter.Write.Close();
		XmlDocumentFragment xmlDocumentFragment = xmlDoc.CreateDocumentFragment();
		xmlDocumentFragment.InnerXml = Encoding.Default.GetString(memoryStream.GetBuffer());
		loadedXfa.currentNode.AppendChild(xmlDocumentFragment);
	}

	private PdfLoadedXfaField[] TryGetFieldsByName(string name, bool isTryGetFlag)
	{
		tryGetfields = new List<PdfLoadedXfaField>();
		GetFields(name, this, isTryGetFlag);
		is_modified = true;
		return tryGetfields.ToArray();
	}
}
