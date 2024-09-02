using System;
using System.Collections.Generic;
using DocGen.Pdf.IO;
using DocGen.Pdf.Parsing;
using DocGen.Pdf.Primitives;
using DocGen.Pdf.Security;
using DocGen.Pdf.Xfa;

namespace DocGen.Pdf.Interactive;

public class PdfForm : IPdfWrapper
{
	private PdfFormFieldCollection m_fields = new PdfFormFieldCollection();

	private PdfResources m_resources;

	private bool m_readOnly;

	private SignatureFlags m_signatureFlags;

	private PdfDictionary m_dictionary = new PdfDictionary();

	private bool m_needAppearances = true;

	private bool m_flatten;

	private bool m_changeName = true;

	private List<string> m_fieldName = new List<string>();

	private List<string> m_fieldNames = new List<string>();

	private bool m_isXFA;

	private bool m_disableAutoFormat;

	internal Dictionary<PdfDictionary, PdfPageBase> m_pageMap = new Dictionary<PdfDictionary, PdfPageBase>();

	private bool m_setAppearanceDictionary;

	private bool m_isDefaultEncoding = true;

	private bool m_isDefaultAppearance;

	internal bool isXfaForm;

	private bool m_complexScript;

	internal bool m_enableXfaFormfill;

	private PdfXfaDocument m_xfa;

	internal PdfXfaDocument Xfa
	{
		get
		{
			return m_xfa;
		}
		set
		{
			m_xfa = value;
		}
	}

	internal bool IsDefaultAppearance
	{
		get
		{
			return m_isDefaultAppearance;
		}
		set
		{
			m_isDefaultAppearance = value;
		}
	}

	public bool ComplexScript
	{
		get
		{
			return m_complexScript;
		}
		set
		{
			m_complexScript = value;
		}
	}

	public bool IsDefaultEncoding
	{
		get
		{
			return m_isDefaultEncoding;
		}
		set
		{
			m_isDefaultEncoding = value;
		}
	}

	internal List<string> FieldNames => m_fieldName;

	internal bool IsXFA
	{
		get
		{
			return m_isXFA;
		}
		set
		{
			m_isXFA = true;
		}
	}

	public PdfFormFieldCollection Fields => m_fields;

	public bool Flatten
	{
		get
		{
			return m_flatten;
		}
		set
		{
			m_flatten = value;
		}
	}

	public virtual bool ReadOnly
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

	public bool FieldAutoNaming
	{
		get
		{
			return m_changeName;
		}
		set
		{
			m_changeName = value;
		}
	}

	internal virtual bool NeedAppearances
	{
		get
		{
			return m_needAppearances;
		}
		set
		{
			if (m_needAppearances != value)
			{
				m_needAppearances = value;
			}
		}
	}

	internal virtual SignatureFlags SignatureFlags
	{
		get
		{
			return m_signatureFlags;
		}
		set
		{
			if (m_signatureFlags != value)
			{
				m_signatureFlags = value;
				m_dictionary.SetNumber("SigFlags", (int)m_signatureFlags);
			}
		}
	}

	internal virtual PdfResources Resources
	{
		get
		{
			if (m_resources == null)
			{
				m_resources = new PdfResources();
				m_dictionary.SetProperty("DR", m_resources);
			}
			return m_resources;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("resources");
			}
			m_resources = value;
		}
	}

	internal virtual PdfDictionary Dictionary
	{
		get
		{
			return m_dictionary;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("Dictionary");
			}
			m_dictionary = value;
		}
	}

	public bool DisableAutoFormat
	{
		get
		{
			return m_disableAutoFormat;
		}
		set
		{
			m_disableAutoFormat = value;
		}
	}

	internal bool SetAppearanceDictionary
	{
		get
		{
			return m_setAppearanceDictionary;
		}
		set
		{
			m_setAppearanceDictionary = value;
		}
	}

	IPdfPrimitive IPdfWrapper.Element => m_dictionary;

	internal event EventHandler<FormPropertyChangedEventArgs> FormFieldPropertyChanged;

	internal event EventHandler<FormFieldsAddedArgs> FormFieldAdded;

	internal event EventHandler<FormFieldsRemovedArgs> FormFieldRemoved;

	internal void OnFormFieldAdded(FormFieldsAddedArgs e)
	{
		this.FormFieldAdded?.Invoke(this, e);
	}

	internal void OnFormFieldRemoved(FormFieldsRemovedArgs e)
	{
		this.FormFieldRemoved?.Invoke(this, e);
	}

	internal void OnFormPropertyChanged(FormPropertyChangedEventArgs e)
	{
		this.FormFieldPropertyChanged?.Invoke(this, e);
	}

	public PdfForm()
	{
		m_fields.Form = this;
		m_dictionary.SetProperty("Fields", m_fields);
		if (!(m_fields.Form is PdfLoadedForm))
		{
			m_dictionary.BeginSave += Dictionary_BeginSave;
		}
		m_setAppearanceDictionary = true;
	}

	internal virtual void Dictionary_BeginSave(object sender, SavePdfPrimitiveEventArgs ars)
	{
		if (m_signatureFlags != 0)
		{
			NeedAppearances = false;
		}
		if (!isXfaForm)
		{
			CheckFlatten();
		}
		if (m_fields.Count > 0 && SetAppearanceDictionary && SignatureFlags == SignatureFlags.None)
		{
			m_dictionary.SetBoolean("NeedAppearances", m_needAppearances);
		}
	}

	internal virtual void Clear()
	{
		if (m_fields != null)
		{
			m_fields.Clear();
			m_fields = null;
		}
		if (m_dictionary != null)
		{
			m_dictionary.Clear();
			m_dictionary = null;
		}
		m_fieldName.Clear();
		m_fieldNames.Clear();
		m_pageMap.Clear();
	}

	private void CheckFlatten()
	{
		for (int i = 0; i < m_fields.Count; i++)
		{
			PdfField pdfField = m_fields[i];
			if (pdfField.DisableAutoFormat && pdfField.Dictionary.ContainsKey("AA"))
			{
				pdfField.Dictionary.Remove("AA");
			}
			if (pdfField.Flatten)
			{
				int num = 0;
				PdfDictionary dictionary = pdfField.Dictionary;
				if (dictionary.ContainsKey("F"))
				{
					num = (dictionary["F"] as PdfNumber).IntValue;
				}
				if (num != 6)
				{
					AddFieldResourcesToPage(pdfField);
					pdfField.Draw();
					m_fields.Remove(pdfField);
					DeleteFromPages(pdfField);
					DeleteAnnotation(pdfField);
					i--;
				}
			}
			else
			{
				if (!(pdfField is PdfLoadedField))
				{
					continue;
				}
				if (pdfField is PdfLoadedTextBoxField && (pdfField.Dictionary.ContainsKey("AP") || (pdfField as PdfLoadedTextBoxField).Items.Count > 0))
				{
					if (IsXFA && pdfField.Dictionary.ContainsKey("MK"))
					{
						PdfDictionary pdfDictionary = pdfField.Dictionary["MK"] as PdfDictionary;
						if (pdfDictionary == null)
						{
							pdfDictionary = (pdfField.Dictionary["MK"] as PdfReferenceHolder).Object as PdfDictionary;
						}
						if (pdfDictionary.ContainsKey("BG"))
						{
							pdfDictionary.Remove("BG");
						}
					}
					if (!IsXFA)
					{
						(pdfField as PdfLoadedField).BeginSave();
					}
				}
				if (pdfField is PdfLoadedField && SignatureFlags == SignatureFlags.None && SetAppearanceDictionary)
				{
					(pdfField as PdfLoadedField).BeginSave();
				}
				else
				{
					pdfField.Save();
				}
			}
		}
	}

	private void AddFieldResourcesToPage(PdfField field)
	{
		PdfResources resources = field.Form.Resources;
		if (!resources.ContainsKey("Font"))
		{
			return;
		}
		PdfDictionary pdfDictionary = null;
		if (resources["Font"] as PdfReferenceHolder != null)
		{
			pdfDictionary = (resources["Font"] as PdfReferenceHolder).Object as PdfDictionary;
		}
		else if (resources["Font"] is PdfDictionary)
		{
			pdfDictionary = resources["Font"] as PdfDictionary;
		}
		foreach (PdfName key in pdfDictionary.Keys)
		{
			PdfResources resources2 = field.Page.GetResources();
			PdfDictionary pdfDictionary2 = null;
			if (resources2["Font"] is PdfDictionary)
			{
				pdfDictionary2 = resources2["Font"] as PdfDictionary;
			}
			else if (resources2["Font"] as PdfReferenceHolder != null)
			{
				pdfDictionary2 = (resources2["Font"] as PdfReferenceHolder).Object as PdfDictionary;
			}
			if (pdfDictionary2 == null || !pdfDictionary2.ContainsKey(key))
			{
				PdfReferenceHolder value = pdfDictionary[key] as PdfReferenceHolder;
				if (pdfDictionary2 == null)
				{
					PdfDictionary pdfDictionary3 = new PdfDictionary();
					pdfDictionary3.Items.Add(key, value);
					resources2["Font"] = pdfDictionary3;
				}
				else
				{
					pdfDictionary2.Items.Add(key, value);
				}
			}
		}
	}

	internal void DeleteFromPages(PdfField field)
	{
		PdfDictionary dictionary = field.Dictionary;
		if (dictionary.ContainsKey("Kids"))
		{
			PdfArray pdfArray = dictionary["Kids"] as PdfArray;
			int i = 0;
			for (int count = pdfArray.Count; i < count; i++)
			{
				PdfReferenceHolder pdfReferenceHolder = pdfArray[i] as PdfReferenceHolder;
				PdfDictionary pdfDictionary = PdfCrossTable.Dereference((pdfReferenceHolder.Object as PdfDictionary)["P"]) as PdfDictionary;
				if (!pdfDictionary.ContainsKey("Annots"))
				{
					continue;
				}
				if (pdfDictionary["Annots"] is PdfReferenceHolder)
				{
					PdfArray pdfArray2 = (pdfDictionary["Annots"] as PdfReferenceHolder).Object as PdfArray;
					pdfArray2.Remove(pdfReferenceHolder);
					pdfArray2.MarkChanged();
					pdfDictionary.SetProperty("Annots", pdfArray2);
				}
				else
				{
					if (!(pdfDictionary["Annots"] is PdfArray))
					{
						continue;
					}
					if (field.Page != null && field.Page is PdfPage)
					{
						PdfPage pdfPage = field.Page as PdfPage;
						PdfAnnotationCollection annotations = pdfPage.Annotations;
						if (annotations != null && annotations.Count == annotations.Annotations.Elements.Count)
						{
							int num = annotations.Annotations.IndexOf(pdfReferenceHolder);
							if (num >= 0 && num < annotations.Count)
							{
								pdfPage.Annotations.RemoveAt(num);
							}
						}
					}
					PdfArray pdfArray2 = pdfDictionary["Annots"] as PdfArray;
					pdfArray2.Remove(pdfReferenceHolder);
					pdfArray2.MarkChanged();
					pdfDictionary.SetProperty("Annots", pdfArray2);
				}
			}
		}
		else
		{
			PdfReferenceHolder pdfReferenceHolder2 = null;
			pdfReferenceHolder2 = ((!dictionary.ContainsKey("P")) ? new PdfReferenceHolder(field.Page.Dictionary) : (dictionary["P"] as PdfReferenceHolder));
			PdfDictionary pdfDictionary2 = pdfReferenceHolder2.Object as PdfDictionary;
			if (pdfDictionary2.ContainsKey("Annots"))
			{
				PdfArray pdfArray3 = pdfDictionary2["Annots"] as PdfArray;
				pdfArray3.Remove(new PdfReferenceHolder(dictionary));
				pdfArray3.MarkChanged();
				pdfDictionary2.SetProperty("Annots", pdfArray3);
			}
		}
	}

	internal void DeleteAnnotation(PdfField field)
	{
		PdfDictionary dictionary = field.Dictionary;
		if (dictionary.ContainsKey("Kids"))
		{
			PdfArray pdfArray = dictionary["Kids"] as PdfArray;
			pdfArray.Clear();
			dictionary.SetProperty("Kids", pdfArray);
		}
	}

	internal virtual string GetCorrectName(string name)
	{
		string text = name;
		m_fieldNames.Add(text);
		if (m_fieldName.Contains(name))
		{
			int num = m_fieldName.IndexOf(name);
			int num2 = m_fieldName.LastIndexOf(name);
			if (num != num2)
			{
				string[] array = Guid.NewGuid().ToString().Split('-');
				text = name + "_" + array[4];
				m_fieldName.RemoveAt(num2);
				m_fieldName.Add(text);
			}
		}
		return text;
	}

	public void SetDefaultAppearance(bool applyDefault)
	{
		NeedAppearances = applyDefault;
		SetAppearanceDictionary = !applyDefault;
		IsDefaultAppearance = applyDefault;
	}
}
