using System;
using System.ComponentModel;
using DocGen.Pdf.IO;
using DocGen.Pdf.Parsing;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public abstract class PdfField : IPdfWrapper, INotifyPropertyChanged
{
	private string m_name = string.Empty;

	private PdfPageBase m_page;

	private FieldFlags m_flags;

	private PdfForm m_form;

	private string m_mappingName = string.Empty;

	private bool m_export = true;

	private bool m_readOnly;

	private bool m_required;

	private string m_toolTip = string.Empty;

	private PdfDictionary m_dictionary = new PdfDictionary();

	private bool m_flatten;

	private bool m_disableAutoFormat;

	private int m_rotationAngle;

	internal bool isXfa;

	private PdfTag m_tag;

	private int m_tabIndex;

	private int m_annotationIndex;

	private bool m_complexScript;

	private PdfLayer layer;

	internal PdfLoadedStyledField m_loadedStyleField;

	public virtual string Name => m_name;

	public virtual PdfForm Form => m_form;

	public virtual string MappingName
	{
		get
		{
			return m_mappingName;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("MappingName");
			}
			if (m_mappingName != value)
			{
				m_mappingName = value;
				m_dictionary.SetString("TM", m_mappingName);
				NotifyPropertyChanged("MappingName");
			}
		}
	}

	public virtual bool Export
	{
		get
		{
			return m_export;
		}
		set
		{
			if (m_export != value)
			{
				m_export = value;
				if (m_export)
				{
					Flags -= 4;
				}
				else
				{
					Flags |= FieldFlags.NoExport;
				}
			}
			NotifyPropertyChanged("Export");
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
			NotifyPropertyChanged("ReadOnly");
		}
	}

	public virtual bool Required
	{
		get
		{
			return m_required;
		}
		set
		{
			if (m_required != value)
			{
				m_required = value;
				if (m_required)
				{
					Flags |= FieldFlags.Required;
				}
				else
				{
					Flags -= 2;
				}
				NotifyPropertyChanged("Required");
			}
		}
	}

	public virtual string ToolTip
	{
		get
		{
			return m_toolTip;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("ToolTip");
			}
			if (m_toolTip != value)
			{
				m_toolTip = value;
				m_dictionary.SetString("TU", m_toolTip);
				NotifyPropertyChanged("ToolTip");
			}
		}
	}

	public virtual PdfPageBase Page
	{
		get
		{
			return m_page;
		}
		internal set
		{
			m_page = value;
		}
	}

	internal bool ComplexScript
	{
		get
		{
			if (Form != null)
			{
				return m_complexScript | Form.ComplexScript;
			}
			return m_complexScript;
		}
		set
		{
			m_complexScript = value;
		}
	}

	public bool Flatten
	{
		get
		{
			bool flag = m_flatten;
			if (Form != null)
			{
				flag |= Form.Flatten;
			}
			return flag;
		}
		set
		{
			m_flatten = value;
			NotifyPropertyChanged("Flatten");
		}
	}

	internal virtual FieldFlags Flags
	{
		get
		{
			if (Dictionary.ContainsKey("Ff") && PdfCrossTable.Dereference(Dictionary["Ff"]) is PdfNumber pdfNumber)
			{
				m_flags = (FieldFlags)pdfNumber.IntValue;
			}
			return m_flags;
		}
		set
		{
			if (m_flags != value)
			{
				m_flags = value;
				m_dictionary.SetNumber("Ff", (int)m_flags);
			}
		}
	}

	internal PdfDictionary Dictionary
	{
		get
		{
			return m_dictionary;
		}
		set
		{
			m_dictionary = value;
		}
	}

	internal int RotationAngle
	{
		get
		{
			return m_rotationAngle;
		}
		set
		{
			m_rotationAngle = value;
		}
	}

	public bool DisableAutoFormat
	{
		get
		{
			bool flag = m_disableAutoFormat;
			if (Form != null)
			{
				flag |= Form.DisableAutoFormat;
			}
			return flag;
		}
		set
		{
			m_disableAutoFormat = value;
			NotifyPropertyChanged("DisableAutoFormat");
		}
	}

	public PdfTag PdfTag
	{
		get
		{
			return m_tag;
		}
		set
		{
			m_tag = value;
			NotifyPropertyChanged("PdfTag");
		}
	}

	public int TabIndex
	{
		get
		{
			PdfLoadedStyledField pdfLoadedStyledField = this as PdfLoadedStyledField;
			PdfLoadedPage pdfLoadedPage = Page as PdfLoadedPage;
			if (pdfLoadedStyledField != null && pdfLoadedPage != null)
			{
				PdfDictionary widgetAnnotation = pdfLoadedStyledField.GetWidgetAnnotation(pdfLoadedStyledField.Dictionary, pdfLoadedStyledField.CrossTable);
				if (widgetAnnotation != null)
				{
					PdfReference reference = pdfLoadedPage.CrossTable.GetReference(widgetAnnotation);
					if (reference != null)
					{
						m_tabIndex = pdfLoadedPage.AnnotsReference.IndexOf(reference);
					}
				}
			}
			return m_tabIndex;
		}
		set
		{
			m_tabIndex = value;
			if (Page != null && Page.FormFieldsTabOrder == PdfFormFieldsTabOrder.Manual && this is PdfLoadedField)
			{
				PdfLoadedStyledField pdfLoadedStyledField = this as PdfLoadedStyledField;
				PdfLoadedPage obj = Page as PdfLoadedPage;
				PdfAnnotation pdfAnnotation = new PdfLoadedWidgetAnnotation(pdfLoadedStyledField.Dictionary, pdfLoadedStyledField.CrossTable, pdfLoadedStyledField.Bounds);
				PdfReference reference = obj.CrossTable.GetReference(((IPdfWrapper)pdfAnnotation).Element);
				int num = obj.AnnotsReference.IndexOf(reference);
				if (num < 0)
				{
					num = AnnotationIndex;
				}
				PdfArray primitive = Page.Annotations.Rearrange(reference, m_tabIndex, num);
				obj.Dictionary.SetProperty("Annots", primitive);
			}
			NotifyPropertyChanged("TabIndex");
		}
	}

	internal int AnnotationIndex
	{
		get
		{
			return m_annotationIndex;
		}
		set
		{
			m_annotationIndex = value;
		}
	}

	public PdfLayer Layer
	{
		get
		{
			if (layer == null)
			{
				layer = GetDocumentLayer();
			}
			return layer;
		}
		set
		{
			layer = value;
			if (layer != null)
			{
				Dictionary.SetProperty("OC", layer.ReferenceHolder);
			}
			else
			{
				Dictionary.Remove("OC");
			}
			NotifyPropertyChanged("Layer");
		}
	}

	IPdfPrimitive IPdfWrapper.Element => m_dictionary;

	public event PropertyChangedEventHandler PropertyChanged;

	internal void NotifyPropertyChanged(string propertyName, int m_index)
	{
		if (this.PropertyChanged != null)
		{
			this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}
		if (Form == null || Form.Fields == null)
		{
			return;
		}
		FormPropertyChangedEventArgs formPropertyChangedEventArgs = new FormPropertyChangedEventArgs(this);
		if (this is PdfRadioButtonListItem)
		{
			PdfRadioButtonListItem pdfRadioButtonListItem = (PdfRadioButtonListItem)this;
			PdfRadioButtonListField field = pdfRadioButtonListItem.m_field;
			formPropertyChangedEventArgs.Index = field.Items.IndexOf(pdfRadioButtonListItem);
			formPropertyChangedEventArgs.Field = field;
		}
		else if (this is PdfListBoxField)
		{
			if (((PdfListBoxField)this).Items != null)
			{
				formPropertyChangedEventArgs.Index = m_index;
			}
		}
		else if (this is PdfComboBoxField)
		{
			if (((PdfComboBoxField)this).Items != null)
			{
				formPropertyChangedEventArgs.Index = m_index;
			}
		}
		else if (this is PdfLoadedRadioButtonListField)
		{
			if (((PdfLoadedRadioButtonListField)this).Items != null)
			{
				formPropertyChangedEventArgs.Index = m_index;
			}
		}
		else if (this is PdfLoadedListBoxField)
		{
			if (((PdfLoadedListBoxField)this).Items != null)
			{
				formPropertyChangedEventArgs.Index = m_index;
			}
		}
		else if (this is PdfLoadedComboBoxField && ((PdfLoadedComboBoxField)this).Items != null)
		{
			formPropertyChangedEventArgs.Index = m_index;
		}
		formPropertyChangedEventArgs.PropertyName = propertyName;
		Form.OnFormPropertyChanged(formPropertyChangedEventArgs);
	}

	internal void NotifyPropertyChanged(string propertyName)
	{
		NotifyPropertyChanged(propertyName, -1);
	}

	public PdfField(PdfPageBase page, string name)
	{
		if (page != null && this is PdfSignatureField)
		{
			PdfSignatureField pdfSignatureField = this as PdfSignatureField;
			if (page is PdfLoadedPage pdfLoadedPage)
			{
				if (pdfLoadedPage.Document != null)
				{
					pdfSignatureField.m_fieldAutoNaming = pdfLoadedPage.Document.ObtainForm().FieldAutoNaming;
				}
			}
			else if (page is PdfPage { Document: not null } pdfPage)
			{
				pdfSignatureField.m_fieldAutoNaming = pdfPage.Document.ObtainForm().FieldAutoNaming;
			}
		}
		Initialize();
		m_name = name;
		m_page = page;
		m_dictionary.SetProperty("T", new PdfString(name));
	}

	internal PdfField()
	{
		Initialize();
	}

	public string GetValue(string key)
	{
		PdfName key2 = new PdfName(key);
		if (Dictionary.ContainsKey(key2))
		{
			PdfString pdfString = PdfCrossTable.Dereference(Dictionary[key2]) as PdfString;
			PdfName pdfName = PdfCrossTable.Dereference(Dictionary[key2]) as PdfName;
			if (pdfString != null)
			{
				return pdfString.Value;
			}
			if (pdfName != null)
			{
				return pdfName.Value;
			}
			throw new PdfException(key + " key is not found");
		}
		throw new PdfException(key + " key is not found");
	}

	public void SetValue(string key, string value)
	{
		if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
		{
			Dictionary.SetProperty(key, new PdfString(value));
		}
	}

	internal void SetForm(PdfForm form)
	{
		m_form = form;
		DefineDefaultAppearance();
	}

	internal virtual void Save()
	{
		bool flag = Form != null && Form.ReadOnly;
		if (m_readOnly || flag)
		{
			Flags |= FieldFlags.ReadOnly;
		}
	}

	internal abstract void Draw();

	internal virtual void ApplyName(string name)
	{
		m_name = name;
		Dictionary.SetProperty("T", new PdfString(name));
	}

	internal virtual PdfField Clone(PdfPageBase page)
	{
		if (page == null)
		{
			throw new ArgumentNullException("page");
		}
		PdfField pdfField = null;
		if (!(page as PdfPage).Section.ParentDocument.EnableMemoryOptimization)
		{
			pdfField = MemberwiseClone() as PdfField;
			pdfField.Dictionary = new PdfDictionary(Dictionary);
			pdfField.m_page = page;
			if (pdfField is PdfLoadedRadioButtonListField && pdfField is PdfLoadedRadioButtonListField pdfLoadedRadioButtonListField && pdfLoadedRadioButtonListField.Items.Count > 0)
			{
				foreach (PdfLoadedRadioButtonItem item in pdfLoadedRadioButtonListField.Items)
				{
					item.Page = page;
				}
			}
			if (pdfField is PdfLoadedButtonField)
			{
				pdfField.Page = page;
			}
			pdfField.Dictionary["P"] = new PdfReferenceHolder(page);
		}
		else if (this is PdfLoadedField)
		{
			PdfDictionary pdfDictionary = new PdfDictionary(Dictionary);
			pdfDictionary.Remove("Parent");
			if (!(this is PdfLoadedSignatureField))
			{
				pdfDictionary.Remove("P");
			}
			pdfDictionary.Remove("Kids");
			PdfDictionary pdfDictionary2 = pdfDictionary.Clone((page as PdfPage).Section.ParentDocument.CrossTable) as PdfDictionary;
			if (this is PdfLoadedButtonField)
			{
				pdfField = (this as PdfLoadedButtonField).Clone(pdfDictionary2, page as PdfPage);
			}
			else if (this is PdfLoadedCheckBoxField)
			{
				pdfField = (this as PdfLoadedCheckBoxField).Clone(pdfDictionary2, page as PdfPage);
			}
			else if (this is PdfLoadedComboBoxField)
			{
				pdfField = (this as PdfLoadedComboBoxField).Clone(pdfDictionary2, page as PdfPage);
			}
			else if (this is PdfLoadedListBoxField)
			{
				pdfField = (this as PdfLoadedListBoxField).Clone(pdfDictionary2, page as PdfPage);
			}
			else if (this is PdfLoadedRadioButtonListField)
			{
				pdfField = (this as PdfLoadedRadioButtonListField).Clone(pdfDictionary2, page as PdfPage);
			}
			else if (this is PdfLoadedSignatureField)
			{
				pdfField = (this as PdfLoadedSignatureField).Clone(pdfDictionary2, page as PdfPage);
			}
			else if (this is PdfLoadedTextBoxField)
			{
				pdfField = (this as PdfLoadedTextBoxField).Clone(pdfDictionary2, page as PdfPage);
			}
			else if (!pdfDictionary2.ContainsKey("FT") && this is PdfLoadedStyledField)
			{
				pdfField = (this as PdfLoadedStyledField).Clone(pdfDictionary2, page as PdfPage);
			}
			PdfLoadedField pdfLoadedField = this as PdfLoadedField;
			pdfField.DisableAutoFormat = pdfLoadedField.DisableAutoFormat;
			pdfField.Export = pdfLoadedField.Export;
			pdfField.Flags = pdfLoadedField.Flags;
			pdfField.Flatten = pdfLoadedField.Flatten;
			if (pdfField.MappingName != null)
			{
				pdfField.MappingName = pdfLoadedField.MappingName;
			}
			pdfField.Required = pdfLoadedField.Required;
			pdfField.RotationAngle = pdfLoadedField.RotationAngle;
			if (pdfLoadedField.ToolTip != null)
			{
				pdfField.ToolTip = pdfLoadedField.ToolTip;
			}
			if (pdfLoadedField is PdfLoadedTextBoxField { m_font: not null } pdfLoadedTextBoxField)
			{
				(pdfField as PdfLoadedTextBoxField).Font = pdfLoadedTextBoxField.m_font;
			}
		}
		return pdfField;
	}

	protected virtual void DefineDefaultAppearance()
	{
		if (!(this is PdfRadioButtonListField pdfRadioButtonListField))
		{
			return;
		}
		for (int i = 0; i < pdfRadioButtonListField.Items.Count; i++)
		{
			PdfRadioButtonListItem pdfRadioButtonListItem = pdfRadioButtonListField.Items[i];
			if (pdfRadioButtonListItem.Font != null)
			{
				Form.Resources.Add(pdfRadioButtonListField.Items[i].Font, new PdfName(pdfRadioButtonListItem.Widget.DefaultAppearance.FontName));
			}
		}
	}

	protected virtual void Initialize()
	{
		m_dictionary.BeginSave += Dictionary_BeginSave;
	}

	private void Dictionary_BeginSave(object sender, SavePdfPrimitiveEventArgs ars)
	{
		if (!(this is PdfSignatureField))
		{
			Save();
		}
		else
		{
			if (Dictionary == null || !Dictionary.ContainsKey("Kids") || !Dictionary.ContainsKey("TU") || !(Dictionary["Kids"] is PdfArray pdfArray))
			{
				return;
			}
			for (int i = 0; i < pdfArray.Count; i++)
			{
				PdfReferenceHolder pdfReferenceHolder = pdfArray.Elements[i] as PdfReferenceHolder;
				if (pdfReferenceHolder != null && pdfReferenceHolder.Object is PdfDictionary pdfDictionary && !pdfDictionary.ContainsKey("TU") && Dictionary["TU"] is PdfString pdfString)
				{
					pdfDictionary.SetString("TU", pdfString.Value);
				}
			}
		}
	}

	private PdfLayer GetDocumentLayer()
	{
		if (Dictionary.ContainsKey("OC"))
		{
			IPdfPrimitive pdfPrimitive = Dictionary["OC"];
			PdfLoadedPage pdfLoadedPage = Page as PdfLoadedPage;
			if (pdfPrimitive != null && pdfLoadedPage != null && pdfLoadedPage.Document != null)
			{
				PdfDocumentLayerCollection layers = pdfLoadedPage.Document.Layers;
				if (layers != null)
				{
					IsMatched(layers, pdfPrimitive, pdfLoadedPage);
				}
			}
		}
		return layer;
	}

	private void IsMatched(PdfDocumentLayerCollection layerCollection, IPdfPrimitive expectedObject, PdfLoadedPage page)
	{
		for (int i = 0; i < layerCollection.Count; i++)
		{
			IPdfPrimitive referenceHolder = layerCollection[i].ReferenceHolder;
			if (referenceHolder != null && referenceHolder.Equals(expectedObject))
			{
				if (layerCollection[i].Name != null)
				{
					layer = layerCollection[i];
					break;
				}
			}
			else if (layerCollection[i].Layers != null && layerCollection[i].Layers.Count > 0)
			{
				IsMatched(layerCollection[i].Layers, expectedObject, page);
			}
		}
	}

	internal PdfArray GetCropOrMediaBox(PdfPageBase page, PdfArray cropOrMediaBox)
	{
		if (page != null && page.Dictionary.ContainsKey("CropBox"))
		{
			cropOrMediaBox = PdfCrossTable.Dereference(page.Dictionary["CropBox"]) as PdfArray;
		}
		else if (page != null && page.Dictionary.ContainsKey("MediaBox"))
		{
			cropOrMediaBox = PdfCrossTable.Dereference(page.Dictionary["MediaBox"]) as PdfArray;
		}
		return cropOrMediaBox;
	}
}
