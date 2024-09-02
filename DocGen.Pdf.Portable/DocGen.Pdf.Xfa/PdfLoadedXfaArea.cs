using System.Collections.Generic;
using System.Xml;

namespace DocGen.Pdf.Xfa;

public class PdfLoadedXfaArea : PdfLoadedXfaField
{
	public PdfLoadedXfaFieldCollection Fields
	{
		get
		{
			return m_fields;
		}
		internal set
		{
			m_fields = value;
		}
	}

	internal PdfLoadedXfaArea()
	{
	}

	internal void Save(PdfLoadedXfaArea area, XmlWriter dataSetWriter, XmlDocument dataSetDoc, PdfLoadedXfaForm form)
	{
		foreach (KeyValuePair<string, PdfXfaField> item in area.Fields.FieldCollection)
		{
			if (item.Value is PdfLoadedXfaForm)
			{
				PdfLoadedXfaForm obj = item.Value as PdfLoadedXfaForm;
				obj.acroForm = form.acroForm;
				obj.SaveSubForms(obj, dataSetWriter);
			}
			if (item.Value is PdfLoadedXfaArea)
			{
				PdfLoadedXfaArea pdfLoadedXfaArea = item.Value as PdfLoadedXfaArea;
				pdfLoadedXfaArea.acroForm = form.acroForm;
				bool flag = true;
				if (dataSetDoc != null)
				{
					string[] array = pdfLoadedXfaArea.nodeName.Split('[');
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
				if (pdfLoadedXfaArea.Name != null && !pdfLoadedXfaArea.Name.Contains("#area"))
				{
					if (pdfLoadedXfaArea.Name != null && !pdfLoadedXfaArea.Name.Contains("#subform") && flag)
					{
						dataSetWriter.WriteStartElement(pdfLoadedXfaArea.Name);
					}
					Save(pdfLoadedXfaArea, dataSetWriter, dataSetDoc, form);
					if (pdfLoadedXfaArea.Name != null && !pdfLoadedXfaArea.Name.Contains("#subform") && flag)
					{
						dataSetWriter.WriteEndElement();
					}
				}
				else
				{
					Save(pdfLoadedXfaArea, dataSetWriter, dataSetDoc, form);
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
			else if (item.Value is PdfLoadedXfaComboBoxField && item.Value is PdfLoadedXfaComboBoxField pdfLoadedXfaComboBoxField)
			{
				pdfLoadedXfaComboBoxField.Fill(dataSetWriter, form);
			}
		}
	}

	internal void ReadArea(XmlNode subNode, PdfLoadedXfaArea area, PdfLoadedXfaForm form, List<string> fieldsNames, List<string> subFormNames, XmlDocument dataSetDoc)
	{
		foreach (XmlNode item in subNode)
		{
			switch (item.Name)
			{
			case "subform":
			{
				PdfLoadedXfaForm pdfLoadedXfaForm = new PdfLoadedXfaForm();
				pdfLoadedXfaForm.parent = area;
				pdfLoadedXfaForm.ReadSubFormProperties(item, pdfLoadedXfaForm, form);
				pdfLoadedXfaForm.currentNode = item;
				SetName(pdfLoadedXfaForm, area.internalSubFormNames, isArea: false);
				if (!area.internalSubFormNames.Contains(pdfLoadedXfaForm.m_name))
				{
					area.internalSubFormNames.Add(pdfLoadedXfaForm.m_name);
				}
				if (pdfLoadedXfaForm.isUnNamedSubForm)
				{
					pdfLoadedXfaForm.internalFieldNames = area.internalFieldNames;
					pdfLoadedXfaForm.internalSubFormNames = area.internalSubFormNames;
				}
				pdfLoadedXfaForm.ReadSubForm(item, pdfLoadedXfaForm, fieldsNames, subFormNames);
				area.Fields.Add(pdfLoadedXfaForm, pdfLoadedXfaForm.m_name);
				area.m_subFormNames.Add(pdfLoadedXfaForm.m_name);
				subFormNames.Add(pdfLoadedXfaForm.m_name);
				if (pdfLoadedXfaForm.isUnNamedSubForm)
				{
					area.internalFieldNames = pdfLoadedXfaForm.internalFieldNames;
					area.internalSubFormNames = pdfLoadedXfaForm.internalSubFormNames;
				}
				break;
			}
			case "field":
				ReadField(item, area, fieldsNames, subFormNames, dataSetDoc);
				break;
			case "exclGroup":
			{
				PdfLoadedXfaRadioButtonGroup pdfLoadedXfaRadioButtonGroup = new PdfLoadedXfaRadioButtonGroup();
				pdfLoadedXfaRadioButtonGroup.parent = area;
				pdfLoadedXfaRadioButtonGroup.ReadField(item, dataSetDoc);
				string fieldName = GetFieldName(area.internalFieldNames, pdfLoadedXfaRadioButtonGroup.Name);
				area.Fields.Add(pdfLoadedXfaRadioButtonGroup, fieldName);
				pdfLoadedXfaRadioButtonGroup.nodeName = area.nodeName + "." + fieldName;
				area.m_fieldNames.Add(fieldName);
				fieldsNames.Add(fieldName);
				if (!area.internalFieldNames.Contains(fieldName))
				{
					area.internalFieldNames.Add(fieldName);
				}
				break;
			}
			case "area":
			{
				PdfLoadedXfaArea pdfLoadedXfaArea = new PdfLoadedXfaArea();
				pdfLoadedXfaArea.parent = area;
				pdfLoadedXfaArea.currentNode = item;
				SetName(pdfLoadedXfaArea, area.internalSubFormNames, isArea: true);
				if (!area.internalSubFormNames.Contains(pdfLoadedXfaArea.m_name))
				{
					area.internalSubFormNames.Add(pdfLoadedXfaArea.m_name);
				}
				pdfLoadedXfaArea.internalFieldNames = area.internalFieldNames;
				pdfLoadedXfaArea.internalSubFormNames = area.internalSubFormNames;
				ReadArea(item, pdfLoadedXfaArea, form, fieldsNames, subFormNames, dataSetDoc);
				area.internalFieldNames = pdfLoadedXfaArea.internalFieldNames;
				area.internalSubFormNames = pdfLoadedXfaArea.internalSubFormNames;
				area.Fields.Add(pdfLoadedXfaArea, pdfLoadedXfaArea.m_name);
				area.m_areaNames.Add(pdfLoadedXfaArea.m_name);
				area.internalSubFormNames.Add(pdfLoadedXfaArea.m_name);
				break;
			}
			}
		}
	}
}
