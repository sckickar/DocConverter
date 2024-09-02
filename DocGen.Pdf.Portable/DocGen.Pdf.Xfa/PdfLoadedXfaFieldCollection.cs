using System.Collections;
using System.Collections.Generic;

namespace DocGen.Pdf.Xfa;

public class PdfLoadedXfaFieldCollection : IEnumerable
{
	private Dictionary<string, PdfXfaField> m_fieldCollection;

	internal PdfLoadedXfaField parent;

	private Dictionary<string, PdfXfaField> m_completeFields = new Dictionary<string, PdfXfaField>();

	internal Dictionary<string, PdfXfaField> CompleteFields
	{
		get
		{
			return m_completeFields;
		}
		set
		{
			m_completeFields = value;
		}
	}

	public PdfLoadedXfaField this[string fieldName]
	{
		get
		{
			if (m_fieldCollection.ContainsKey(fieldName))
			{
				return m_fieldCollection[fieldName] as PdfLoadedXfaField;
			}
			return null;
		}
		internal set
		{
			if (m_fieldCollection.ContainsKey(fieldName))
			{
				m_fieldCollection[fieldName] = value;
			}
		}
	}

	internal PdfXfaField this[int index]
	{
		get
		{
			List<PdfXfaField> list = new List<PdfXfaField>();
			foreach (KeyValuePair<string, PdfXfaField> item in m_fieldCollection)
			{
				list.Add(item.Value);
			}
			return list[index];
		}
	}

	internal Dictionary<string, PdfXfaField> FieldCollection => m_fieldCollection;

	public int Count => m_fieldCollection.Count;

	public PdfLoadedXfaFieldCollection()
	{
		m_fieldCollection = new Dictionary<string, PdfXfaField>();
	}

	internal string Add(PdfLoadedXfaField xfaField)
	{
		string text = xfaField.Name + "[0]";
		if (xfaField != null && xfaField.Name != null && (m_fieldCollection.ContainsKey(text) || CompleteFields.ContainsKey(text)))
		{
			int num = 0;
			while (m_fieldCollection.ContainsKey(text) || CompleteFields.ContainsKey(text))
			{
				num++;
				text = xfaField.Name + "[" + num + "]";
			}
			m_fieldCollection.Add(text, xfaField);
		}
		else if (xfaField != null && xfaField.Name != null && !m_fieldCollection.ContainsKey(text))
		{
			m_fieldCollection.Add(text, xfaField);
		}
		CompleteFields.Add(text, xfaField);
		return text;
	}

	internal void Add(PdfLoadedXfaField field, string fieldName)
	{
		if (!m_fieldCollection.ContainsKey(fieldName) && !CompleteFields.ContainsKey(fieldName))
		{
			m_fieldCollection.Add(fieldName, field);
			CompleteFields.Add(fieldName, field);
		}
	}

	private string GetName(string name)
	{
		string text = name + "[0]";
		if (m_fieldCollection.ContainsKey(text))
		{
			int num = 0;
			while (m_fieldCollection.ContainsKey(text))
			{
				num++;
				text = name + "[" + num + "]";
			}
		}
		return text;
	}

	internal void AddStaticFields(PdfLoadedXfaField xfaField, string fieldName)
	{
		if (!CompleteFields.ContainsKey(fieldName))
		{
			CompleteFields.Add(fieldName, xfaField);
		}
	}

	internal void AddStaticFields(PdfLoadedXfaField xfaField)
	{
		string key = xfaField.Name + "[0]";
		if (xfaField != null && xfaField.Name != null && CompleteFields.ContainsKey(key))
		{
			int num = 0;
			while (CompleteFields.ContainsKey(key))
			{
				num++;
				key = xfaField.Name + "[" + num + "]";
			}
			CompleteFields.Add(key, xfaField);
		}
		else if (xfaField != null && xfaField.Name != null && !CompleteFields.ContainsKey(key))
		{
			CompleteFields.Add(key, xfaField);
		}
	}

	public void Add(PdfXfaField xfaField)
	{
		if (xfaField is PdfXfaRadioButtonField)
		{
			throw new PdfException("Can't add single radio button, need to add the radio button in group (PdfXfaRadioButtonGroup).");
		}
		if (xfaField is PdfXfaForm)
		{
			PdfXfaForm pdfXfaForm = xfaField as PdfXfaForm;
			if (pdfXfaForm.Name != string.Empty)
			{
				string name = GetName(pdfXfaForm.Name);
				if (parent != null)
				{
					parent.m_subFormNames.Add(name);
				}
				m_fieldCollection.Add(name, pdfXfaForm.Clone() as PdfXfaField);
			}
			else
			{
				string name2 = GetName("subform");
				if (parent != null)
				{
					parent.m_subFormNames.Add(name2);
				}
				m_fieldCollection.Add(name2, pdfXfaForm.Clone() as PdfXfaField);
			}
		}
		else if (xfaField.Name != string.Empty)
		{
			string name3 = GetName(xfaField.Name);
			if (parent != null)
			{
				parent.m_fieldNames.Add(name3);
			}
			m_fieldCollection.Add(GetName(xfaField.Name), xfaField);
		}
		else if (xfaField is PdfXfaTextElement)
		{
			string name4 = GetName("textElement");
			if (parent != null)
			{
				parent.m_fieldNames.Add(name4);
			}
			m_fieldCollection.Add(GetName(xfaField.Name), xfaField);
		}
		else
		{
			if (!(xfaField is PdfXfaLine))
			{
				throw new PdfException("Field name is invalid.");
			}
			string name5 = GetName("line");
			if (parent != null)
			{
				parent.m_fieldNames.Add(name5);
			}
			m_fieldCollection.Add(GetName(xfaField.Name), xfaField);
		}
	}

	public void Remove(PdfLoadedXfaField lField)
	{
		lField.currentNode.ParentNode.RemoveChild(lField.currentNode);
		string[] array = lField.nodeName.Split('.');
		if (lField.parent != null)
		{
			lField.parent.m_fields.m_fieldCollection.Remove(array[^1]);
			return;
		}
		lField.currentNode = null;
		if (!m_fieldCollection.ContainsValue(lField))
		{
			return;
		}
		foreach (KeyValuePair<string, PdfXfaField> item in m_fieldCollection)
		{
			if (item.Value == lField)
			{
				m_fieldCollection.Remove(item.Key);
			}
		}
	}

	public void RemoveAt(int index)
	{
		int num = 0;
		foreach (KeyValuePair<string, PdfXfaField> item in m_fieldCollection)
		{
			if (num == index)
			{
				m_fieldCollection.Remove(item.Key);
				break;
			}
			num++;
		}
	}

	public void Clear()
	{
		if (parent != null)
		{
			parent.currentNode.RemoveAll();
		}
		m_fieldCollection.Clear();
	}

	public IEnumerator GetEnumerator()
	{
		return m_fieldCollection.Values.GetEnumerator();
	}
}
