using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using DocGen.Pdf.Interactive;
using DocGen.Pdf.IO;
using DocGen.Pdf.Primitives;
using DocGen.Pdf.Security;

namespace DocGen.Pdf.Parsing;

public class PdfLoadedFormFieldCollection : PdfFieldCollection
{
	private PdfLoadedForm m_form;

	private List<string> m_fieldNames;

	private List<string> m_indexedFieldNames;

	private List<string> m_actualFieldNames;

	private List<string> m_indexedActualFieldNames;

	internal List<string> m_addedFieldNames = new List<string>();

	private bool m_isCreateNewFormField;

	internal bool m_isImported;

	public override PdfField this[int index]
	{
		get
		{
			int count = base.List.Count;
			if (count < 0 || index >= count)
			{
				throw new IndexOutOfRangeException("index");
			}
			return base.List[index] as PdfField;
		}
	}

	public new PdfField this[string name]
	{
		get
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			if (name == string.Empty)
			{
				throw new ArgumentException("Field name can't be empty");
			}
			int fieldIndex = GetFieldIndex(name);
			if (fieldIndex == -1)
			{
				throw new ArgumentException("Incorrect field name");
			}
			return this[fieldIndex];
		}
	}

	public PdfLoadedForm Form
	{
		get
		{
			return m_form;
		}
		set
		{
			m_form = value;
		}
	}

	public PdfLoadedFormFieldCollection(PdfLoadedForm form)
	{
		if (form == null)
		{
			throw new ArgumentException("form");
		}
		m_form = form;
		int i = 0;
		for (int count = m_form.TerminalFields.Count; i < count; i++)
		{
			PdfField field = GetField(i);
			if (field != null)
			{
				DoAdd(field);
			}
		}
	}

	public PdfLoadedFormFieldCollection()
	{
	}

	public bool ValidateSignatures(out List<PdfSignatureValidationResult> results)
	{
		PdfSignatureValidationOptions options = new PdfSignatureValidationOptions();
		return ValidateSignatures(options, out results);
	}

	public bool ValidateSignatures(PdfSignatureValidationOptions options, out List<PdfSignatureValidationResult> results)
	{
		bool flag = true;
		results = new List<PdfSignatureValidationResult>();
		for (int i = 0; i < base.Count; i++)
		{
			if (this[i] is PdfLoadedSignatureField { IsSigned: not false } pdfLoadedSignatureField)
			{
				PdfSignatureValidationResult pdfSignatureValidationResult = pdfLoadedSignatureField.ValidateSignature(options);
				if (pdfSignatureValidationResult != null)
				{
					results.Add(pdfSignatureValidationResult);
					flag = flag && pdfSignatureValidationResult.IsSignatureValid;
				}
			}
		}
		if (results.Count == 0)
		{
			results = null;
			flag = false;
		}
		return flag;
	}

	public bool ValidateSignatures(X509Certificate2Collection rootCertificates, out List<PdfSignatureValidationResult> results)
	{
		PdfSignatureValidationOptions options = new PdfSignatureValidationOptions();
		return ValidateSignatures(rootCertificates, options, out results);
	}

	public bool ValidateSignatures(X509Certificate2Collection rootCertificates, PdfSignatureValidationOptions options, out List<PdfSignatureValidationResult> results)
	{
		bool flag = true;
		results = new List<PdfSignatureValidationResult>();
		for (int i = 0; i < base.Count; i++)
		{
			if (this[i] is PdfLoadedSignatureField { IsSigned: not false } pdfLoadedSignatureField)
			{
				PdfSignatureValidationResult pdfSignatureValidationResult = pdfLoadedSignatureField.ValidateSignature(rootCertificates, options);
				if (pdfSignatureValidationResult != null)
				{
					results.Add(pdfSignatureValidationResult);
					flag = flag && pdfSignatureValidationResult.IsSignatureValid;
				}
			}
		}
		if (results.Count == 0)
		{
			results = null;
			flag = false;
		}
		return flag;
	}

	private PdfField GetField(int index)
	{
		PdfDictionary dictionary = m_form.TerminalFields[index];
		PdfCrossTable crossTable = m_form.CrossTable;
		PdfField pdfField = null;
		PdfName pdfName = PdfLoadedField.GetValue(dictionary, crossTable, "FT", inheritable: true) as PdfName;
		PdfLoadedFieldTypes pdfLoadedFieldTypes = PdfLoadedFieldTypes.Null;
		if (pdfName != null)
		{
			pdfLoadedFieldTypes = GetFieldType(pdfName, dictionary, crossTable);
		}
		switch (pdfLoadedFieldTypes)
		{
		case PdfLoadedFieldTypes.PushButton:
			pdfField = CreatePushButton(dictionary, crossTable);
			break;
		case PdfLoadedFieldTypes.CheckBox:
			pdfField = CreateCheckBox(dictionary, crossTable);
			break;
		case PdfLoadedFieldTypes.RadioButton:
			pdfField = CreateRadioButton(dictionary, crossTable);
			break;
		case PdfLoadedFieldTypes.TextField:
			pdfField = CreateTextField(dictionary, crossTable);
			break;
		case PdfLoadedFieldTypes.ComboBox:
			pdfField = CreateComboBox(dictionary, crossTable);
			break;
		case PdfLoadedFieldTypes.ListBox:
			pdfField = CreateListBox(dictionary, crossTable);
			break;
		case PdfLoadedFieldTypes.SignatureField:
			pdfField = CreateSignatureField(dictionary, crossTable);
			break;
		case PdfLoadedFieldTypes.Null:
			pdfField = new PdfLoadedStyledField(dictionary, crossTable);
			pdfField.SetForm(Form);
			break;
		}
		if (pdfField is PdfLoadedField pdfLoadedField)
		{
			pdfLoadedField.SetForm(Form);
			pdfLoadedField.BeforeNameChanges += ldField_NameChanded;
		}
		return pdfField;
	}

	private PdfField GetField(PdfDictionary dictionary)
	{
		PdfCrossTable crossTable = m_form.CrossTable;
		PdfField pdfField = null;
		PdfName pdfName = PdfLoadedField.GetValue(dictionary, crossTable, "FT", inheritable: true) as PdfName;
		PdfLoadedFieldTypes pdfLoadedFieldTypes = PdfLoadedFieldTypes.Null;
		if (pdfName != null)
		{
			pdfLoadedFieldTypes = GetFieldType(pdfName, dictionary, crossTable);
		}
		switch (pdfLoadedFieldTypes)
		{
		case PdfLoadedFieldTypes.PushButton:
			pdfField = CreatePushButton(dictionary, crossTable);
			break;
		case PdfLoadedFieldTypes.CheckBox:
			pdfField = CreateCheckBox(dictionary, crossTable);
			break;
		case PdfLoadedFieldTypes.RadioButton:
			pdfField = CreateRadioButton(dictionary, crossTable);
			break;
		case PdfLoadedFieldTypes.TextField:
			pdfField = CreateTextField(dictionary, crossTable);
			break;
		case PdfLoadedFieldTypes.ComboBox:
			pdfField = CreateComboBox(dictionary, crossTable);
			break;
		case PdfLoadedFieldTypes.ListBox:
			pdfField = CreateListBox(dictionary, crossTable);
			break;
		case PdfLoadedFieldTypes.SignatureField:
			pdfField = CreateSignatureField(dictionary, crossTable);
			break;
		case PdfLoadedFieldTypes.Null:
			pdfField = new PdfLoadedStyledField(dictionary, crossTable);
			pdfField.SetForm(Form);
			break;
		}
		if (pdfField is PdfLoadedField pdfLoadedField)
		{
			pdfLoadedField.SetForm(Form);
			pdfLoadedField.BeforeNameChanges += ldField_NameChanded;
		}
		return pdfField;
	}

	private PdfField CreateSignatureField(PdfDictionary dictionary, PdfCrossTable crossTable)
	{
		PdfLoadedSignatureField pdfLoadedSignatureField = new PdfLoadedSignatureField(dictionary, crossTable);
		pdfLoadedSignatureField.SetForm(Form);
		return pdfLoadedSignatureField;
	}

	private PdfField CreateListBox(PdfDictionary dictionary, PdfCrossTable crossTable)
	{
		PdfLoadedListBoxField pdfLoadedListBoxField = new PdfLoadedListBoxField(dictionary, crossTable);
		pdfLoadedListBoxField.SetForm(Form);
		return pdfLoadedListBoxField;
	}

	private PdfField CreateComboBox(PdfDictionary dictionary, PdfCrossTable crossTable)
	{
		PdfLoadedComboBoxField pdfLoadedComboBoxField = new PdfLoadedComboBoxField(dictionary, crossTable);
		pdfLoadedComboBoxField.SetForm(Form);
		return pdfLoadedComboBoxField;
	}

	private PdfField CreateTextField(PdfDictionary dictionary, PdfCrossTable crossTable)
	{
		PdfLoadedTextBoxField pdfLoadedTextBoxField = new PdfLoadedTextBoxField(dictionary, crossTable);
		pdfLoadedTextBoxField.SetForm(Form);
		return pdfLoadedTextBoxField;
	}

	private PdfField CreateRadioButton(PdfDictionary dictionary, PdfCrossTable crossTable)
	{
		if (dictionary.ContainsKey("Kids") && PdfCrossTable.Dereference(dictionary["Kids"]) is PdfArray { Count: >=0 } pdfArray)
		{
			List<int> list = new List<int>();
			for (int i = 0; i < pdfArray.Count; i++)
			{
				if (PdfCrossTable.Dereference(pdfArray[i]) is PdfDictionary pdfDictionary && pdfDictionary.ContainsKey("P"))
				{
					if (PdfCrossTable.Dereference(pdfDictionary["P"]) is PdfDictionary pdfDictionary2 && pdfDictionary2.ContainsKey("Annots") && PdfCrossTable.Dereference(pdfDictionary2["Annots"]) is PdfArray pdfArray2 && !pdfArray2.Contains(pdfArray[i]))
					{
						list.Add(i);
					}
				}
				else if (PdfCrossTable.Dereference(pdfArray[i]) is PdfNull)
				{
					list.Add(i);
				}
			}
			for (int num = list.Count - 1; num >= 0; num--)
			{
				pdfArray.RemoveAt(list[num]);
			}
			if (pdfArray.Count == 0)
			{
				return null;
			}
			list.Clear();
		}
		PdfLoadedRadioButtonListField pdfLoadedRadioButtonListField = new PdfLoadedRadioButtonListField(dictionary, crossTable);
		pdfLoadedRadioButtonListField.SetForm(Form);
		return pdfLoadedRadioButtonListField;
	}

	internal void CreateFormFieldsFromWidgets(int startFormFieldIndex)
	{
		for (int i = startFormFieldIndex; i < m_form.TerminalFields.Count; i++)
		{
			PdfField field = GetField(i);
			if (field != null)
			{
				DoAdd(field);
			}
		}
		if (m_form.m_widgetDictionary == null)
		{
			return;
		}
		foreach (KeyValuePair<string, List<PdfDictionary>> item in m_form.m_widgetDictionary)
		{
			if (item.Value.Count > 1)
			{
				PdfField field2 = GetField(item.Value[0]);
				PdfLoadedRadioButtonListField pdfLoadedRadioButtonListField = field2 as PdfLoadedRadioButtonListField;
				for (int j = 1; j < item.Value.Count; j++)
				{
					field2 = GetField(item.Value[j]);
					if (field2 != null && field2 is PdfLoadedRadioButtonListField pdfLoadedRadioButtonListField2)
					{
						for (int k = 0; k < pdfLoadedRadioButtonListField2.Items.Count; k++)
						{
							pdfLoadedRadioButtonListField?.Items.Add(pdfLoadedRadioButtonListField2.Items[k]);
						}
					}
				}
				m_form.TerminalFields.Add(field2.Dictionary);
				if (field2 != null)
				{
					DoAdd(field2);
				}
			}
			else
			{
				m_form.TerminalFields.Add(item.Value[0]);
				PdfField field3 = GetField(m_form.TerminalFields.Count - 1);
				if (field3 != null)
				{
					DoAdd(field3);
				}
			}
		}
	}

	private PdfField CreateCheckBox(PdfDictionary dictionary, PdfCrossTable crossTable)
	{
		PdfLoadedCheckBoxField pdfLoadedCheckBoxField = new PdfLoadedCheckBoxField(dictionary, crossTable);
		pdfLoadedCheckBoxField.SetForm(Form);
		return pdfLoadedCheckBoxField;
	}

	private PdfField CreatePushButton(PdfDictionary dictionary, PdfCrossTable crossTable)
	{
		PdfLoadedButtonField pdfLoadedButtonField = new PdfLoadedButtonField(dictionary, crossTable);
		pdfLoadedButtonField.SetForm(Form);
		return pdfLoadedButtonField;
	}

	internal PdfLoadedFieldTypes GetFieldType(PdfName name, PdfDictionary dictionary, PdfCrossTable crossTable)
	{
		string value = name.Value;
		PdfLoadedFieldTypes result = PdfLoadedFieldTypes.Null;
		PdfNumber pdfNumber = PdfLoadedField.GetValue(dictionary, crossTable, "Ff", inheritable: true) as PdfNumber;
		int num = 0;
		if (pdfNumber != null)
		{
			num = pdfNumber.IntValue;
		}
		switch (value.ToLower())
		{
		case "btn":
			result = (((num & 0x10000) == 0) ? (((num & 0x8000) == 0) ? PdfLoadedFieldTypes.CheckBox : PdfLoadedFieldTypes.RadioButton) : PdfLoadedFieldTypes.PushButton);
			break;
		case "tx":
			result = PdfLoadedFieldTypes.TextField;
			break;
		case "ch":
			result = (((num & 0x20000) == 0) ? PdfLoadedFieldTypes.ListBox : PdfLoadedFieldTypes.ComboBox);
			break;
		case "sig":
			result = PdfLoadedFieldTypes.SignatureField;
			break;
		}
		return result;
	}

	protected override int DoAdd(PdfField field)
	{
		if (field == null)
		{
			throw new ArgumentNullException("field");
		}
		field.SetForm(m_form);
		bool flag = false;
		PdfArray pdfArray = null;
		pdfArray = ((!m_form.Dictionary.ContainsKey("Fields")) ? new PdfArray() : (m_form.CrossTable.GetObject(m_form.Dictionary["Fields"]) as PdfArray));
		if (field.Dictionary.Items.ContainsKey(new PdfName("Parent")) && !m_isImported)
		{
			flag = true;
		}
		PdfReferenceHolder element = new PdfReferenceHolder(field);
		if (!pdfArray.Contains(element))
		{
			if (IsValidName(field.Name) && !flag)
			{
				pdfArray.Add(new PdfReferenceHolder(field));
				m_form.Dictionary.SetProperty("Fields", pdfArray);
			}
			else if (m_form.FieldAutoNaming && !flag)
			{
				string correctName = GetCorrectName(field.Name);
				field.ApplyName(correctName);
				pdfArray.Add(new PdfReferenceHolder(field));
				m_form.Dictionary.SetProperty("Fields", pdfArray);
			}
			else
			{
				if ((IsValidName(field.Name) && flag) || (m_form.FieldAutoNaming && flag))
				{
					m_addedFieldNames.Add(field.Name);
					return base.DoAdd(field);
				}
				if (base.Count <= 0)
				{
					return base.DoAdd(field);
				}
				{
					IEnumerator enumerator = GetEnumerator();
					try
					{
						while (enumerator.MoveNext())
						{
							PdfField pdfField = (PdfField)enumerator.Current;
							if (!(pdfField.Name == field.Name))
							{
								continue;
							}
							if (field is PdfTextBoxField && (pdfField is PdfTextBoxField || pdfField is PdfLoadedTextBoxField))
							{
								if (pdfField is PdfLoadedTextBoxField)
								{
									PdfArray pdfArray2 = pdfField.Dictionary.Items[new PdfName("Kids")] as PdfArray;
									(field as PdfTextBoxField).Widget.Dictionary?.Remove("Parent");
									(field as PdfTextBoxField).Widget.Parent = pdfField;
									if (field.Dictionary.Items[new PdfName("Kids")] is PdfArray pdfArray3)
									{
										foreach (PdfReferenceHolder element2 in pdfArray3.Elements)
										{
											if (element2 != null)
											{
												pdfArray2?.Add(element2);
											}
										}
									}
								}
								else
								{
									(field as PdfTextBoxField).Widget.Dictionary?.Remove("Parent");
									(field as PdfTextBoxField).Widget.Parent = pdfField;
									if (field is PdfTextBoxField)
									{
										PdfTextBoxField pdfTextBoxField = field as PdfTextBoxField;
										if (field.Page is PdfPage)
										{
											(field.Page as PdfPage).Annotations.Add(pdfTextBoxField.Widget);
										}
										else if (field.Page is PdfLoadedPage)
										{
											(field.Page as PdfLoadedPage).Annotations.Add(pdfTextBoxField.Widget);
										}
									}
									if (!(pdfField as PdfTextBoxField).m_array.Contains(new PdfReferenceHolder((pdfField as PdfTextBoxField).Widget)))
									{
										(pdfField as PdfTextBoxField).m_array.Add(new PdfReferenceHolder((pdfField as PdfTextBoxField).Widget));
									}
									(pdfField as PdfTextBoxField).m_array.Add(new PdfReferenceHolder((field as PdfTextBoxField).Widget));
									pdfField.Dictionary.SetProperty("Kids", (pdfField as PdfTextBoxField).m_array);
									(pdfField as PdfTextBoxField).fieldItems.Add(field);
								}
								return base.Count - 1;
							}
							if (!(field is PdfSignatureField))
							{
								continue;
							}
							PdfSignatureField pdfSignatureField = field as PdfSignatureField;
							PdfDictionary dictionary = pdfSignatureField.Widget.Dictionary;
							if (dictionary != null && dictionary.ContainsKey("Parent"))
							{
								dictionary.Remove("Parent");
							}
							pdfSignatureField.Widget.Parent = pdfField;
							PdfArray pdfArray4 = null;
							PdfArray pdfArray5 = null;
							if (pdfField.Dictionary.ContainsKey("Kids"))
							{
								pdfArray4 = pdfField.Dictionary.Items[new PdfName("Kids")] as PdfArray;
							}
							if (field.Dictionary.ContainsKey("Kids"))
							{
								pdfArray5 = field.Dictionary.Items[new PdfName("Kids")] as PdfArray;
							}
							if (pdfArray5 != null)
							{
								if (pdfArray4 == null)
								{
									pdfArray4 = new PdfArray();
								}
								foreach (PdfReferenceHolder element3 in pdfArray5.Elements)
								{
									if (element3 != null)
									{
										pdfArray4.Add(element3);
									}
								}
							}
							pdfField.Dictionary.SetProperty("Kids", pdfArray4);
							pdfSignatureField.m_SkipKidsCertificate = true;
							PdfCrossTable crossTable = null;
							if (field.Page is PdfPage)
							{
								PdfPage pdfPage = field.Page as PdfPage;
								crossTable = pdfPage.CrossTable;
								if (!pdfPage.Annotations.Contains(pdfSignatureField.Widget))
								{
									pdfPage.Annotations.Add(pdfSignatureField.Widget);
								}
							}
							else if (field.Page is PdfLoadedPage)
							{
								PdfLoadedPage pdfLoadedPage = field.Page as PdfLoadedPage;
								crossTable = pdfLoadedPage.CrossTable;
								if (!pdfLoadedPage.Annotations.Contains(pdfSignatureField.Widget))
								{
									pdfLoadedPage.Annotations.Add(pdfSignatureField.Widget);
								}
							}
							if (pdfField is PdfLoadedSignatureField)
							{
								PdfLoadedStyledField pdfLoadedStyledField = new PdfLoadedStyledField(dictionary, crossTable);
								PdfLoadedSignatureItem item = new PdfLoadedSignatureItem(pdfLoadedStyledField, pdfArray4.Count - 1, dictionary);
								(pdfField as PdfLoadedSignatureField).Items.Add(item);
								field.m_loadedStyleField = pdfLoadedStyledField;
							}
							return base.Count - 1;
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
		}
		m_addedFieldNames.Add(field.Name);
		int result = base.DoAdd(field);
		PdfLoadedForm form = m_form;
		if (form != null && !(field is PdfLoadedField))
		{
			form.OnFormFieldAdded(new FormFieldsAddedArgs(field)
			{
				MethodName = "Field Add"
			});
		}
		return result;
	}

	protected override void DoInsert(int index, PdfField field)
	{
		if (index < 0 || index > base.List.Count)
		{
			throw new IndexOutOfRangeException();
		}
		if (field == null)
		{
			throw new ArgumentNullException("field");
		}
		field.SetForm(m_form);
		PdfReferenceHolder element = new PdfReferenceHolder(field);
		if (!(field is PdfLoadedField))
		{
			PdfArray pdfArray = null;
			pdfArray = ((!m_form.Dictionary.ContainsKey("Fields")) ? new PdfArray() : (m_form.CrossTable.GetObject(m_form.Dictionary["Fields"]) as PdfArray));
			if (m_form.IsFormContainsKids)
			{
				if (m_form.Fields.List[index] is PdfField)
				{
					PdfField pdfField = m_form.Fields.List[index] as PdfField;
					PdfReferenceHolder obj = new PdfReferenceHolder(pdfField);
					if (pdfField.Dictionary.Items.ContainsKey(new PdfName("Parent")))
					{
						PdfArray pdfArray2 = ((pdfField.Dictionary["Parent"] as PdfReferenceHolder).Object as PdfDictionary).Items[new PdfName("Kids")] as PdfArray;
						for (int i = 0; i < pdfArray2.Count; i++)
						{
							if ((pdfArray2[i] as PdfReferenceHolder).Equals(obj))
							{
								pdfArray2.RemoveAt(i);
								pdfArray2.Insert(i, element);
							}
						}
					}
				}
			}
			else
			{
				pdfArray.Insert(index, new PdfReferenceHolder(field));
			}
			m_form.Dictionary.SetProperty("Fields", pdfArray);
		}
		PdfLoadedForm form = m_form;
		if (form != null && !(field is PdfLoadedField))
		{
			FormFieldsAddedArgs formFieldsAddedArgs = new FormFieldsAddedArgs(field);
			formFieldsAddedArgs.MethodName = "Field Insert";
			formFieldsAddedArgs.Index = index;
			form.OnFormFieldAdded(formFieldsAddedArgs);
		}
		base.DoInsert(index, field);
	}

	protected override void DoRemove(PdfField field)
	{
		if (field == null)
		{
			throw new ArgumentNullException("field");
		}
		m_form.RemoveFromDictionaries(field);
		if (m_fieldNames != null)
		{
			m_fieldNames.Remove(field.Name);
		}
		if (m_addedFieldNames != null)
		{
			m_addedFieldNames.Remove(field.Name);
		}
		if (m_indexedFieldNames != null)
		{
			m_indexedFieldNames.Remove(field.Name);
		}
		PdfLoadedForm form = m_form;
		if (form != null)
		{
			FormFieldsRemovedArgs formFieldsRemovedArgs = new FormFieldsRemovedArgs(field);
			formFieldsRemovedArgs.MethodName = "Field Remove";
			form.OnFormFieldRemoved(formFieldsRemovedArgs);
		}
		base.DoRemove(field);
	}

	protected override void DoRemoveAt(int index)
	{
		if (index < 0 || index > base.List.Count)
		{
			throw new IndexOutOfRangeException();
		}
		PdfField pdfField = base.List[index] as PdfField;
		if (pdfField is PdfLoadedField)
		{
			m_form.RemoveFromDictionaries(pdfField);
			if (m_fieldNames != null)
			{
				m_fieldNames.Remove(pdfField.Name);
			}
			if (m_addedFieldNames != null)
			{
				m_addedFieldNames.Remove(pdfField.Name);
			}
			if (m_indexedFieldNames != null)
			{
				m_indexedFieldNames.Remove(pdfField.Name);
			}
		}
		PdfLoadedForm form = m_form;
		if (form != null)
		{
			FormFieldsRemovedArgs formFieldsRemovedArgs = new FormFieldsRemovedArgs(pdfField);
			formFieldsRemovedArgs.Index = index;
			formFieldsRemovedArgs.MethodName = "Field Remove At";
			form.OnFormFieldRemoved(formFieldsRemovedArgs);
		}
		base.DoRemoveAt(index);
	}

	internal void RemoveContainingField(PdfReferenceHolder pageReferenceHolder)
	{
		for (int num = base.Items.Count - 1; num >= 0; num--)
		{
			if ((base.Items[num] as PdfReferenceHolder).Object is PdfDictionary pdfDictionary)
			{
				bool removeField;
				if (pdfDictionary.ContainsKey("P"))
				{
					PdfReferenceHolder pdfReferenceHolder = null;
					pdfReferenceHolder = pdfDictionary["P"] as PdfReferenceHolder;
					if (pdfReferenceHolder != null && pdfReferenceHolder == pageReferenceHolder)
					{
						DoRemoveAt(num);
					}
				}
				else if (pdfDictionary.ContainsKey("Kids") && RemoveContainingFieldItems(pdfDictionary, pageReferenceHolder, out removeField) && base.List[num] is PdfLoadedField)
				{
					if (removeField)
					{
						DoRemoveAt(num);
					}
					else
					{
						(base.List[num] as PdfLoadedField).BeginSave();
					}
				}
			}
		}
	}

	protected override void DoClear()
	{
		int i = 0;
		for (int count = base.List.Count; i < count; i++)
		{
			if (base.List[i] is PdfLoadedField pdfLoadedField)
			{
				m_form.RemoveFromDictionaries(pdfLoadedField);
				PdfLoadedForm form = m_form;
				if (form != null)
				{
					FormFieldsRemovedArgs formFieldsRemovedArgs = new FormFieldsRemovedArgs(pdfLoadedField);
					formFieldsRemovedArgs.MethodName = "Field Clear";
					form.OnFormFieldRemoved(formFieldsRemovedArgs);
				}
			}
		}
		m_addedFieldNames.Clear();
		m_form.TerminalFields.Clear();
		base.DoClear();
	}

	internal bool IsValidName(string name)
	{
		return !m_addedFieldNames.Contains(name);
	}

	internal string GetCorrectName(string name)
	{
		List<string> list = new List<string>();
		foreach (PdfField item in base.List)
		{
			list.Add(item.Name);
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

	internal void AddFieldDictionary(PdfDictionary field)
	{
		if (field == null)
		{
			throw new ArgumentNullException("field");
		}
		base.List.Add(field);
		base.Items.Add(new PdfReferenceHolder(field));
	}

	private void ldField_NameChanded(string name)
	{
		if (!IsValidName(name))
		{
			throw new ArgumentException("Field with the same name already exist");
		}
	}

	private int GetFieldIndex(string name)
	{
		int num = -1;
		if (m_fieldNames == null)
		{
			m_fieldNames = new List<string>();
			m_indexedFieldNames = new List<string>();
			foreach (PdfField item in base.List)
			{
				m_fieldNames.Add(item.Name);
				if (item.Name != null)
				{
					m_indexedFieldNames.Add(item.Name.Split('[')[0]);
				}
			}
		}
		string empty = string.Empty;
		for (int i = 0; i < m_fieldNames.Count; i++)
		{
			empty = m_fieldNames[i];
			if (empty != null && empty.Contains("_") && empty[1] == '_')
			{
				empty = empty.Trim();
				m_fieldNames[i] = empty.Replace("_", "s");
			}
		}
		if (m_fieldNames.Contains(name))
		{
			num = m_fieldNames.IndexOf(name);
		}
		else if (m_indexedFieldNames.Contains(name))
		{
			num = m_indexedFieldNames.IndexOf(name);
		}
		else if (m_addedFieldNames.Contains(name))
		{
			num = m_addedFieldNames.IndexOf(name);
		}
		if (num < 0)
		{
			if (m_actualFieldNames == null)
			{
				m_actualFieldNames = new List<string>();
				m_indexedActualFieldNames = new List<string>();
				foreach (PdfField item2 in base.List)
				{
					if (item2 is PdfLoadedField)
					{
						m_actualFieldNames.Add((item2 as PdfLoadedField).ActualFieldName);
						m_indexedActualFieldNames.Add((item2 as PdfLoadedField).ActualFieldName.Split('[')[0]);
					}
					else
					{
						m_actualFieldNames.Add(item2.Name);
						m_indexedActualFieldNames.Add(item2.Name.Split('[')[0]);
					}
				}
			}
			if (m_actualFieldNames.Contains(name))
			{
				num = m_actualFieldNames.IndexOf(name);
			}
			else if (m_indexedActualFieldNames.Contains(name))
			{
				num = m_indexedActualFieldNames.IndexOf(name);
			}
		}
		return num;
	}

	internal void ResetFieldNames()
	{
		m_fieldNames = null;
		m_actualFieldNames = null;
	}

	private PdfField GetNamedField(string name)
	{
		PdfField result = null;
		foreach (PdfField item in base.List)
		{
			if (item.Name == name)
			{
				result = item;
			}
		}
		return result;
	}

	public bool TryGetField(string fieldName, out PdfLoadedField field)
	{
		field = null;
		int fieldIndex = GetFieldIndex(fieldName);
		if (fieldIndex > -1)
		{
			field = base.List[fieldIndex] as PdfLoadedField;
			return true;
		}
		return false;
	}

	public bool TryGetValue(string fieldName, out string fieldValue)
	{
		fieldValue = string.Empty;
		int fieldIndex = GetFieldIndex(fieldName);
		if (fieldIndex > -1)
		{
			fieldValue = (base.List[fieldIndex] as PdfLoadedTextBoxField).Text;
			return true;
		}
		return false;
	}
}
