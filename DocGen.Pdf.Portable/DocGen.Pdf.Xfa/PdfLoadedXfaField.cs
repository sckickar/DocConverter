using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Parsing;

namespace DocGen.Pdf.Xfa;

public abstract class PdfLoadedXfaField : PdfXfaField
{
	internal XmlNode currentNode;

	internal XmlNode dataSetNode;

	internal string nodeName = string.Empty;

	internal PdfLoadedXfaField parent;

	internal string bindingName = string.Empty;

	internal bool isRendered;

	internal List<string> m_fieldNames = new List<string>();

	internal List<string> m_subFormNames = new List<string>();

	internal List<string> internalFieldNames = new List<string>();

	internal List<string> internalSubFormNames = new List<string>();

	internal bool isUnNamedSubForm;

	internal List<string> m_areaNames = new List<string>();

	internal PdfLoadedXfaFieldCollection m_fields = new PdfLoadedXfaFieldCollection();

	internal PdfLoadedForm acroForm;

	internal string m_name = string.Empty;

	internal bool isBindingMatchNone;

	internal string GetFieldName(List<string> fieldNameList, string name)
	{
		string text = name + "[0]";
		if (fieldNameList.Contains(text))
		{
			int num = 1;
			text = name + "[" + num++ + "]";
			while (fieldNameList.Contains(text))
			{
				text = name + "[" + num++ + "]";
			}
		}
		return text;
	}

	internal int GetSameNameFieldsCount(string name)
	{
		int num = 0;
		char[] separator = new char[1] { '[' };
		foreach (string internalFieldName in internalFieldNames)
		{
			if (internalFieldName.Split(separator)[0] == name)
			{
				num++;
			}
		}
		return num;
	}

	internal void SetName(PdfLoadedXfaField field, List<string> subFormNamesCollection, bool isArea)
	{
		if (field.currentNode.Attributes != null && field.currentNode.Attributes["name"] != null)
		{
			field.Name = field.currentNode.Attributes["name"].Value;
		}
		else
		{
			field.isUnNamedSubForm = true;
			field.Name = (isArea ? "#area" : "#subform");
		}
		field.m_name = GetFieldName(subFormNamesCollection, field.Name);
		if (field.parent != null && !string.IsNullOrEmpty(field.parent.nodeName))
		{
			field.nodeName = field.parent.nodeName + "." + field.m_name;
		}
		else
		{
			field.nodeName = field.m_name;
		}
	}

	internal float ConvertToPoint(string value)
	{
		float result = 0f;
		if (value.Contains("pt"))
		{
			result = Convert.ToSingle(value.Trim('p', 't', 'm'), CultureInfo.InvariantCulture);
		}
		else if (value.Contains("cm"))
		{
			result = Convert.ToSingle(value.Trim('p', 't', 'c', 'm'), CultureInfo.InvariantCulture);
			result *= 28.346457f;
		}
		else if (value.Contains("m"))
		{
			try
			{
				result = Convert.ToSingle(value.Trim('p', 't', 'm'), CultureInfo.InvariantCulture);
			}
			catch
			{
				result = Convert.ToSingle(Regex.Replace(value, "[^0-9.-]", ""), CultureInfo.InvariantCulture);
			}
			result *= 2.8346457f;
		}
		else if (value.Contains("in"))
		{
			result = Convert.ToSingle(value.Trim('i', 'n'), CultureInfo.InvariantCulture);
			result *= 72f;
		}
		return result;
	}

	internal void ReadMargin(XmlNode node)
	{
		for (int i = 0; i < node.Attributes.Count; i++)
		{
			if (node.Attributes[i].Name == "bottomInset")
			{
				base.Margins.Bottom = ConvertToPoint(node.Attributes[i].Value);
			}
			if (node.Attributes[i].Name == "topInset")
			{
				base.Margins.Top = ConvertToPoint(node.Attributes[i].Value);
			}
			if (node.Attributes[i].Name == "leftInset")
			{
				base.Margins.Left = ConvertToPoint(node.Attributes[i].Value);
			}
			if (node.Attributes[i].Name == "rightInset")
			{
				base.Margins.Right = ConvertToPoint(node.Attributes[i].Value);
			}
		}
	}

	internal void ReadMargin(XmlNode node, PdfMargins margins)
	{
		if (node.Attributes.Count > 0)
		{
			for (int i = 0; i < node.Attributes.Count; i++)
			{
				if (node.Attributes[i].Name == "bottomInset")
				{
					margins.Bottom = ConvertToPoint(node.Attributes[i].Value);
				}
				if (node.Attributes[i].Name == "topInset")
				{
					margins.Top = ConvertToPoint(node.Attributes[i].Value);
				}
				if (node.Attributes[i].Name == "leftInset")
				{
					margins.Left = ConvertToPoint(node.Attributes[i].Value);
				}
				if (node.Attributes[i].Name == "rightInset")
				{
					margins.Right = ConvertToPoint(node.Attributes[i].Value);
				}
			}
		}
		else
		{
			margins.Left = 2f;
			margins.Right = 2f;
		}
	}

	internal string ReadBinding(XmlNode node, PdfLoadedXfaForm ff)
	{
		string result = string.Empty;
		if (node.Attributes["ref"] != null)
		{
			result = node.Attributes["ref"].Value;
			result = result.Replace("$record.", "");
			result = result.Replace("[*]", "");
		}
		if (node.Attributes["match"] != null && node.Attributes["match"].Value == "none")
		{
			ff.isBindingMatchNone = true;
		}
		return result;
	}

	internal void SetSize(XmlNode node, string attribute, float value)
	{
		if (node.Attributes[attribute] != null)
		{
			node.Attributes[attribute].Value = value + "pt";
			return;
		}
		XmlAttribute xmlAttribute = node.OwnerDocument.CreateAttribute(attribute);
		xmlAttribute.Value = value + "pt";
		node.Attributes.Append(xmlAttribute);
	}

	internal void ReadField(XmlNode node, PdfLoadedXfaForm form, List<string> fieldNames, List<string> subFormNames, XmlDocument dataSetDoc)
	{
		if (node["ui"] == null)
		{
			return;
		}
		if (node["ui"]["textEdit"] != null || node["ui"]["passwordEdit"] != null)
		{
			PdfLoadedXfaTextBoxField pdfLoadedXfaTextBoxField = new PdfLoadedXfaTextBoxField();
			pdfLoadedXfaTextBoxField.parent = form;
			pdfLoadedXfaTextBoxField.Read(node, dataSetDoc);
			string fieldName = GetFieldName(form.internalFieldNames, pdfLoadedXfaTextBoxField.Name);
			form.Fields.Add(pdfLoadedXfaTextBoxField, fieldName);
			pdfLoadedXfaTextBoxField.nodeName = pdfLoadedXfaTextBoxField.nodeName + "." + fieldName;
			fieldNames.Add(fieldName);
			if (form != this)
			{
				form.m_fieldNames.Add(fieldName);
			}
			if (!form.internalFieldNames.Contains(fieldName))
			{
				form.internalFieldNames.Add(fieldName);
			}
		}
		else if (node["ui"]["numericEdit"] != null)
		{
			PdfLoadedXfaNumericField pdfLoadedXfaNumericField = new PdfLoadedXfaNumericField();
			pdfLoadedXfaNumericField.parent = form;
			pdfLoadedXfaNumericField.Read(node, dataSetDoc);
			string fieldName2 = GetFieldName(form.internalFieldNames, pdfLoadedXfaNumericField.Name);
			form.Fields.Add(pdfLoadedXfaNumericField, fieldName2);
			pdfLoadedXfaNumericField.nodeName = pdfLoadedXfaNumericField.nodeName + "." + fieldName2;
			fieldNames.Add(fieldName2);
			if (form != this)
			{
				form.m_fieldNames.Add(fieldName2);
			}
			if (!form.internalFieldNames.Contains(fieldName2))
			{
				form.internalFieldNames.Add(fieldName2);
			}
		}
		else if (node["ui"]["checkButton"] != null)
		{
			PdfLoadedXfaCheckBoxField pdfLoadedXfaCheckBoxField = new PdfLoadedXfaCheckBoxField();
			pdfLoadedXfaCheckBoxField.parent = form;
			pdfLoadedXfaCheckBoxField.Read(node, dataSetDoc);
			string fieldName3 = GetFieldName(form.internalFieldNames, pdfLoadedXfaCheckBoxField.Name);
			form.Fields.Add(pdfLoadedXfaCheckBoxField, fieldName3);
			pdfLoadedXfaCheckBoxField.nodeName = pdfLoadedXfaCheckBoxField.nodeName + "." + fieldName3;
			fieldNames.Add(fieldName3);
			if (form != this)
			{
				form.m_fieldNames.Add(fieldName3);
			}
			if (!form.internalFieldNames.Contains(fieldName3))
			{
				form.internalFieldNames.Add(fieldName3);
			}
		}
		else if (node["ui"]["choiceList"] != null)
		{
			XmlAttributeCollection attributes = node["ui"]["choiceList"].Attributes;
			if (attributes["open"] != null)
			{
				switch (attributes["open"].Value.ToLower())
				{
				case "always":
				case "multiselect":
				case "usercontrol":
				{
					PdfLoadedXfaListBoxField pdfLoadedXfaListBoxField = new PdfLoadedXfaListBoxField();
					pdfLoadedXfaListBoxField.parent = form;
					pdfLoadedXfaListBoxField.ReadField(node, dataSetDoc);
					string fieldName5 = GetFieldName(form.internalFieldNames, pdfLoadedXfaListBoxField.Name);
					form.Fields.Add(pdfLoadedXfaListBoxField, fieldName5);
					pdfLoadedXfaListBoxField.nodeName = pdfLoadedXfaListBoxField.nodeName + "." + fieldName5;
					fieldNames.Add(fieldName5);
					if (form != this)
					{
						form.m_fieldNames.Add(fieldName5);
					}
					if (!form.internalFieldNames.Contains(fieldName5))
					{
						form.internalFieldNames.Add(fieldName5);
					}
					break;
				}
				case "onentry":
				{
					PdfLoadedXfaComboBoxField pdfLoadedXfaComboBoxField = new PdfLoadedXfaComboBoxField();
					pdfLoadedXfaComboBoxField.parent = form;
					pdfLoadedXfaComboBoxField.ReadField(node, dataSetDoc);
					string fieldName4 = GetFieldName(form.internalFieldNames, pdfLoadedXfaComboBoxField.Name);
					form.Fields.Add(pdfLoadedXfaComboBoxField, fieldName4);
					pdfLoadedXfaComboBoxField.nodeName = pdfLoadedXfaComboBoxField.nodeName + "." + fieldName4;
					fieldNames.Add(fieldName4);
					if (form != this)
					{
						form.m_fieldNames.Add(fieldName4);
					}
					if (!form.internalFieldNames.Contains(fieldName4))
					{
						form.internalFieldNames.Add(fieldName4);
					}
					break;
				}
				}
			}
			else
			{
				PdfLoadedXfaComboBoxField pdfLoadedXfaComboBoxField2 = new PdfLoadedXfaComboBoxField();
				pdfLoadedXfaComboBoxField2.parent = form;
				pdfLoadedXfaComboBoxField2.ReadField(node, dataSetDoc);
				string fieldName6 = GetFieldName(form.internalFieldNames, pdfLoadedXfaComboBoxField2.Name);
				form.Fields.Add(pdfLoadedXfaComboBoxField2, fieldName6);
				pdfLoadedXfaComboBoxField2.nodeName = pdfLoadedXfaComboBoxField2.nodeName + "." + fieldName6;
				fieldNames.Add(fieldName6);
				if (form != this)
				{
					form.m_fieldNames.Add(fieldName6);
				}
				if (!form.internalFieldNames.Contains(fieldName6))
				{
					form.internalFieldNames.Add(fieldName6);
				}
			}
		}
		else if (node["ui"]["dateTimeEdit"] != null)
		{
			PdfLoadedXfaDateTimeField pdfLoadedXfaDateTimeField = new PdfLoadedXfaDateTimeField();
			pdfLoadedXfaDateTimeField.parent = form;
			pdfLoadedXfaDateTimeField.Read(node, dataSetDoc);
			string fieldName7 = GetFieldName(form.internalFieldNames, pdfLoadedXfaDateTimeField.Name);
			form.Fields.Add(pdfLoadedXfaDateTimeField, fieldName7);
			pdfLoadedXfaDateTimeField.nodeName = pdfLoadedXfaDateTimeField.nodeName + "." + fieldName7;
			fieldNames.Add(fieldName7);
			if (form != this)
			{
				form.m_fieldNames.Add(fieldName7);
			}
			if (!form.internalFieldNames.Contains(fieldName7))
			{
				form.internalFieldNames.Add(fieldName7);
			}
		}
		else if (node["ui"]["button"] != null)
		{
			PdfLoadedXfaButtonField pdfLoadedXfaButtonField = new PdfLoadedXfaButtonField();
			pdfLoadedXfaButtonField.parent = form;
			pdfLoadedXfaButtonField.Read(node);
			string fieldName8 = GetFieldName(form.internalFieldNames, pdfLoadedXfaButtonField.Name);
			form.Fields.AddStaticFields(pdfLoadedXfaButtonField, fieldName8);
			if (!form.internalFieldNames.Contains(fieldName8))
			{
				form.internalFieldNames.Add(fieldName8);
			}
		}
		else if (node["ui"]["imageEdit"] != null)
		{
			PdfLoadedXfaImage pdfLoadedXfaImage = new PdfLoadedXfaImage();
			pdfLoadedXfaImage.parent = form;
			pdfLoadedXfaImage.ReadField(node, dataSetDoc);
			string fieldName9 = GetFieldName(form.internalFieldNames, pdfLoadedXfaImage.Name);
			form.Fields.AddStaticFields(pdfLoadedXfaImage, fieldName9);
			if (!form.internalFieldNames.Contains(fieldName9))
			{
				form.internalFieldNames.Add(fieldName9);
			}
		}
	}

	internal void ReadStaticField(XmlNode node, PdfLoadedXfaForm form, XmlDocument dataSetDoc)
	{
		if (node["ui"] != null && node["ui"]["textEdit"] != null)
		{
			PdfLoadedXfaTextElement pdfLoadedXfaTextElement = new PdfLoadedXfaTextElement();
			pdfLoadedXfaTextElement.parent = form;
			pdfLoadedXfaTextElement.Read(node);
			string fieldName = GetFieldName(form.internalFieldNames, pdfLoadedXfaTextElement.Name);
			form.Fields.AddStaticFields(pdfLoadedXfaTextElement, fieldName);
			if (!form.internalFieldNames.Contains(fieldName))
			{
				form.internalFieldNames.Add(fieldName);
			}
		}
		else if (node["ui"] != null && node["ui"]["imageEdit"] != null)
		{
			PdfLoadedXfaImage pdfLoadedXfaImage = new PdfLoadedXfaImage();
			pdfLoadedXfaImage.parent = form;
			pdfLoadedXfaImage.ReadField(node, dataSetDoc);
			string fieldName2 = GetFieldName(form.internalFieldNames, pdfLoadedXfaImage.Name);
			form.Fields.AddStaticFields(pdfLoadedXfaImage, fieldName2);
			if (!form.internalFieldNames.Contains(fieldName2))
			{
				form.internalFieldNames.Add(fieldName2);
			}
		}
		else if (node["value"] != null && node["value"]["line"] != null)
		{
			PdfLoadedXfaLine pdfLoadedXfaLine = new PdfLoadedXfaLine();
			pdfLoadedXfaLine.parent = form;
			pdfLoadedXfaLine.ReadField(node);
			string fieldName3 = GetFieldName(form.internalFieldNames, pdfLoadedXfaLine.Name);
			form.Fields.AddStaticFields(pdfLoadedXfaLine, fieldName3);
			if (!form.internalFieldNames.Contains(fieldName3))
			{
				form.internalFieldNames.Add(fieldName3);
			}
		}
		else if (node["value"] != null && node["value"]["rectangle"] != null)
		{
			PdfLoadedXfaRectangleField pdfLoadedXfaRectangleField = new PdfLoadedXfaRectangleField();
			pdfLoadedXfaRectangleField.parent = form;
			pdfLoadedXfaRectangleField.ReadField(node);
			string fieldName4 = GetFieldName(form.internalFieldNames, pdfLoadedXfaRectangleField.Name);
			form.Fields.AddStaticFields(pdfLoadedXfaRectangleField, fieldName4);
			if (!form.internalFieldNames.Contains(fieldName4))
			{
				form.internalFieldNames.Add(fieldName4);
			}
		}
		else if (node["value"] != null && node["value"]["arc"] != null)
		{
			PdfLoadedXfaCircleField pdfLoadedXfaCircleField = new PdfLoadedXfaCircleField();
			pdfLoadedXfaCircleField.parent = form;
			pdfLoadedXfaCircleField.ReadField(node);
			string fieldName5 = GetFieldName(form.internalFieldNames, pdfLoadedXfaCircleField.Name);
			form.Fields.AddStaticFields(pdfLoadedXfaCircleField, fieldName5);
			if (!form.internalFieldNames.Contains(fieldName5))
			{
				form.internalFieldNames.Add(fieldName5);
			}
		}
	}

	internal string ReadField(XmlNode node, PdfLoadedXfaArea form, List<string> fieldNames, List<string> subFormNames, XmlDocument dataSetDoc)
	{
		string text = string.Empty;
		if (node["ui"] != null)
		{
			if (node["ui"]["textEdit"] != null || node["ui"]["passwordEdit"] != null)
			{
				PdfLoadedXfaTextBoxField pdfLoadedXfaTextBoxField = new PdfLoadedXfaTextBoxField();
				pdfLoadedXfaTextBoxField.parent = form;
				pdfLoadedXfaTextBoxField.Read(node, dataSetDoc);
				text = GetFieldName(form.internalFieldNames, pdfLoadedXfaTextBoxField.Name);
				form.Fields.Add(pdfLoadedXfaTextBoxField, text);
				pdfLoadedXfaTextBoxField.nodeName = pdfLoadedXfaTextBoxField.nodeName + "." + text;
				fieldNames.Add(text);
				if (form != this)
				{
					form.m_fieldNames.Add(text);
				}
				if (!form.internalFieldNames.Contains(text))
				{
					form.internalFieldNames.Add(text);
				}
			}
			else if (node["ui"]["numericEdit"] != null)
			{
				PdfLoadedXfaNumericField pdfLoadedXfaNumericField = new PdfLoadedXfaNumericField();
				pdfLoadedXfaNumericField.parent = form;
				pdfLoadedXfaNumericField.Read(node, dataSetDoc);
				text = GetFieldName(form.internalFieldNames, pdfLoadedXfaNumericField.Name);
				form.Fields.Add(pdfLoadedXfaNumericField, text);
				pdfLoadedXfaNumericField.nodeName = pdfLoadedXfaNumericField.nodeName + "." + text;
				fieldNames.Add(text);
				if (form != this)
				{
					form.m_fieldNames.Add(text);
				}
				if (!form.internalFieldNames.Contains(text))
				{
					form.internalFieldNames.Add(text);
				}
			}
			else if (node["ui"]["checkButton"] != null)
			{
				PdfLoadedXfaCheckBoxField pdfLoadedXfaCheckBoxField = new PdfLoadedXfaCheckBoxField();
				pdfLoadedXfaCheckBoxField.parent = form;
				pdfLoadedXfaCheckBoxField.Read(node, dataSetDoc);
				text = GetFieldName(form.internalFieldNames, pdfLoadedXfaCheckBoxField.Name);
				form.Fields.Add(pdfLoadedXfaCheckBoxField, text);
				pdfLoadedXfaCheckBoxField.nodeName = pdfLoadedXfaCheckBoxField.nodeName + "." + text;
				fieldNames.Add(text);
				if (form != this)
				{
					form.m_fieldNames.Add(text);
				}
				if (!form.internalFieldNames.Contains(text))
				{
					form.internalFieldNames.Add(text);
				}
			}
			else if (node["ui"]["choiceList"] != null)
			{
				XmlAttributeCollection attributes = node["ui"]["choiceList"].Attributes;
				if (attributes["open"] != null)
				{
					switch (attributes["open"].Value.ToLower())
					{
					case "always":
					case "multiselect":
					case "usercontrol":
					{
						PdfLoadedXfaListBoxField pdfLoadedXfaListBoxField = new PdfLoadedXfaListBoxField();
						pdfLoadedXfaListBoxField.parent = form;
						pdfLoadedXfaListBoxField.ReadField(node, dataSetDoc);
						text = GetFieldName(form.internalFieldNames, pdfLoadedXfaListBoxField.Name);
						form.Fields.Add(pdfLoadedXfaListBoxField, text);
						pdfLoadedXfaListBoxField.nodeName = pdfLoadedXfaListBoxField.nodeName + "." + text;
						fieldNames.Add(text);
						if (form != this)
						{
							form.m_fieldNames.Add(text);
						}
						if (!form.internalFieldNames.Contains(text))
						{
							form.internalFieldNames.Add(text);
						}
						break;
					}
					case "onentry":
					{
						PdfLoadedXfaComboBoxField pdfLoadedXfaComboBoxField = new PdfLoadedXfaComboBoxField();
						pdfLoadedXfaComboBoxField.parent = form;
						pdfLoadedXfaComboBoxField.ReadField(node, dataSetDoc);
						text = GetFieldName(form.internalFieldNames, pdfLoadedXfaComboBoxField.Name);
						form.Fields.Add(pdfLoadedXfaComboBoxField, text);
						pdfLoadedXfaComboBoxField.nodeName = pdfLoadedXfaComboBoxField.nodeName + "." + text;
						fieldNames.Add(text);
						if (form != this)
						{
							form.m_fieldNames.Add(text);
						}
						if (!form.internalFieldNames.Contains(text))
						{
							form.internalFieldNames.Add(text);
						}
						break;
					}
					}
				}
				else
				{
					PdfLoadedXfaComboBoxField pdfLoadedXfaComboBoxField2 = new PdfLoadedXfaComboBoxField();
					pdfLoadedXfaComboBoxField2.parent = form;
					pdfLoadedXfaComboBoxField2.ReadField(node, dataSetDoc);
					text = GetFieldName(form.internalFieldNames, pdfLoadedXfaComboBoxField2.Name);
					form.Fields.Add(pdfLoadedXfaComboBoxField2, text);
					pdfLoadedXfaComboBoxField2.nodeName = pdfLoadedXfaComboBoxField2.nodeName + "." + text;
					fieldNames.Add(text);
					if (form != this)
					{
						form.m_fieldNames.Add(text);
					}
					if (!form.internalFieldNames.Contains(text))
					{
						form.internalFieldNames.Add(text);
					}
				}
			}
			else if (node["ui"]["dateTimeEdit"] != null)
			{
				PdfLoadedXfaDateTimeField pdfLoadedXfaDateTimeField = new PdfLoadedXfaDateTimeField();
				pdfLoadedXfaDateTimeField.parent = form;
				pdfLoadedXfaDateTimeField.Read(node, dataSetDoc);
				text = GetFieldName(form.internalFieldNames, pdfLoadedXfaDateTimeField.Name);
				form.Fields.Add(pdfLoadedXfaDateTimeField, text);
				pdfLoadedXfaDateTimeField.nodeName = pdfLoadedXfaDateTimeField.nodeName + "." + text;
				fieldNames.Add(text);
				if (form != this)
				{
					form.m_fieldNames.Add(text);
				}
				if (!form.internalFieldNames.Contains(text))
				{
					form.internalFieldNames.Add(text);
				}
			}
		}
		return text;
	}
}
