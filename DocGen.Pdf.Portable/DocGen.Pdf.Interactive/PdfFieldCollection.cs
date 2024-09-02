using System;
using System.Collections.Generic;
using DocGen.Pdf.IO;
using DocGen.Pdf.Parsing;
using DocGen.Pdf.Primitives;
using DocGen.Pdf.Xfa;

namespace DocGen.Pdf.Interactive;

public class PdfFieldCollection : PdfCollection, IPdfWrapper
{
	internal string c_exisingFieldException = "The field with '{0}' name already exists";

	private PdfArray m_array = new PdfArray();

	private Dictionary<string, int> m_fieldNames;

	public virtual PdfField this[int index] => (PdfField)base.List[index];

	public PdfField this[string name]
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

	internal PdfArray Items => m_array;

	IPdfPrimitive IPdfWrapper.Element => m_array;

	public int Add(PdfField field)
	{
		if (field == null)
		{
			throw new ArgumentNullException("field");
		}
		return DoAdd(field);
	}

	internal void Add(PdfXfaForm collection, string subformName)
	{
		if (collection != null)
		{
			DoAdd(collection, subformName);
		}
	}

	public void Insert(int index, PdfField field)
	{
		if (field == null)
		{
			throw new ArgumentNullException("field");
		}
		DoInsert(index, field);
	}

	public bool Contains(PdfField field)
	{
		return base.List.Contains(field);
	}

	public int IndexOf(PdfField field)
	{
		if (field == null)
		{
			throw new ArgumentNullException("field");
		}
		return base.List.IndexOf(field);
	}

	public void Remove(PdfField field)
	{
		if (field == null)
		{
			throw new ArgumentNullException("field");
		}
		DoRemove(field);
	}

	public void RemoveAt(int index)
	{
		_ = this[index];
		DoRemoveAt(index);
	}

	public void Clear()
	{
		DoClear();
	}

	internal int Add(PdfField field, PdfPageBase newPage)
	{
		PdfField pdfField = null;
		if (field is PdfLoadedField)
		{
			pdfField = InsertLoadedField(field as PdfLoadedField, newPage);
		}
		else if (field.Page != null)
		{
			pdfField = field;
		}
		int result = DoAdd(pdfField);
		if (field is PdfLoadedField && pdfField.ReadOnly != (field as PdfLoadedField).ReadOnly)
		{
			pdfField.ReadOnly = (field as PdfLoadedField).ReadOnly;
		}
		return result;
	}

	protected virtual void DoAdd(PdfXfaForm form, string subformName)
	{
		PdfReferenceHolder pdfReferenceHolder = new PdfReferenceHolder(new PdfDictionary
		{
			Items = 
			{
				{
					new PdfName("Kids"),
					(IPdfPrimitive)form.m_acroFields.Items
				},
				{
					new PdfName("T"),
					(IPdfPrimitive)new PdfString(subformName)
				}
			}
		});
		m_array.Add(pdfReferenceHolder);
		foreach (PdfReferenceHolder item in form.m_acroFields.Items)
		{
			if (item != null)
			{
				PdfDictionary pdfDictionary = item.Object as PdfDictionary;
				if (pdfDictionary.ContainsKey(new PdfName("Kids")) && pdfDictionary.ContainsKey(new PdfName("T")))
				{
					pdfDictionary.Items.Add(new PdfName("Parent"), pdfReferenceHolder);
				}
			}
		}
		base.List.Add(form.m_acroFields.Items);
	}

	protected virtual int DoAdd(PdfField field)
	{
		if (field is PdfStyledField)
		{
			PdfStyledField pdfStyledField = field as PdfStyledField;
			if (field.Page is PdfPage)
			{
				(field.Page as PdfPage).Annotations.Add(pdfStyledField.Widget);
			}
			else if (field.Page is PdfLoadedPage)
			{
				(field.Page as PdfLoadedPage).Annotations.Add(pdfStyledField.Widget);
			}
			if (PdfDocument.ConformanceLevel == PdfConformanceLevel.None && pdfStyledField.Actions != null && pdfStyledField.Actions.Calculate != null)
			{
				if (field.Form.Dictionary.ContainsKey("CO"))
				{
					if (PdfCrossTable.Dereference(field.Form.Dictionary["CO"]) is PdfArray pdfArray)
					{
						pdfArray.Add(new PdfReferenceHolder(field.Dictionary));
					}
				}
				else
				{
					PdfArray pdfArray2 = new PdfArray();
					pdfArray2.Add(new PdfReferenceHolder(field.Dictionary));
					field.Form.Dictionary["CO"] = new PdfReferenceHolder(pdfArray2);
				}
			}
		}
		else if (field is PdfSignatureField)
		{
			PdfSignatureField pdfSignatureField = field as PdfSignatureField;
			if (field.Page is PdfPage)
			{
				PdfPage pdfPage = field.Page as PdfPage;
				if (!pdfPage.Annotations.Contains(pdfSignatureField.Widget))
				{
					pdfPage.Annotations.Add(pdfSignatureField.Widget);
				}
			}
			else if (field.Page is PdfLoadedPage)
			{
				PdfLoadedPage pdfLoadedPage = field.Page as PdfLoadedPage;
				if (!pdfLoadedPage.Annotations.Contains(pdfSignatureField.Widget))
				{
					pdfLoadedPage.Annotations.Add(pdfSignatureField.Widget);
				}
			}
		}
		m_array.Add(new PdfReferenceHolder(field));
		base.List.Add(field);
		if (field.Dictionary != null && field.Dictionary.ContainsKey("Kids") && field.Dictionary["Kids"] is PdfArray pdfArray3)
		{
			for (int i = 0; i < pdfArray3.Count; i++)
			{
				PdfReferenceHolder pdfReferenceHolder = pdfArray3.Elements[i] as PdfReferenceHolder;
				if (!(pdfReferenceHolder != null))
				{
					continue;
				}
				PdfDictionary pdfDictionary = pdfReferenceHolder.Object as PdfDictionary;
				PdfStructTreeRoot structTreeRoot = PdfCatalog.StructTreeRoot;
				if (pdfDictionary != null && structTreeRoot != null)
				{
					if (field.PdfTag != null && field.PdfTag is PdfStructureElement)
					{
						structTreeRoot.Add(field.PdfTag as PdfStructureElement, field.Page, pdfDictionary);
					}
					else
					{
						structTreeRoot.Add(new PdfStructureElement(PdfTagType.Form), field.Page, pdfDictionary);
					}
				}
			}
		}
		field.AnnotationIndex = base.List.Count - 1;
		return base.List.Count - 1;
	}

	protected virtual void DoInsert(int index, PdfField field)
	{
		m_array.Insert(index, new PdfReferenceHolder(field));
		base.List.Insert(index, field);
	}

	protected virtual void DoRemove(PdfField field)
	{
		int index = base.List.IndexOf(field);
		m_array.RemoveAt(index);
		base.List.Remove(field);
	}

	protected virtual void DoRemoveAt(int index)
	{
		m_array.RemoveAt(index);
		base.List.RemoveAt(index);
	}

	protected new virtual void DoClear()
	{
		m_array.Clear();
		base.List.Clear();
	}

	internal bool RemoveContainingFieldItems(PdfDictionary fieldDictionary, PdfReferenceHolder pageReferenceHolder, out bool removeField)
	{
		bool result = false;
		removeField = false;
		if (fieldDictionary.ContainsKey("Kids") && fieldDictionary["Kids"] is PdfArray pdfArray)
		{
			for (int num = pdfArray.Count - 1; num >= 0; num--)
			{
				PdfReferenceHolder pdfReferenceHolder = null;
				PdfDictionary pdfDictionary = null;
				pdfDictionary = ((!(pdfArray[num] is PdfReferenceHolder)) ? (pdfArray[num] as PdfDictionary) : ((pdfArray[num] as PdfReferenceHolder).Object as PdfDictionary));
				if (pdfDictionary != null && pdfDictionary.ContainsKey("P"))
				{
					pdfReferenceHolder = pdfDictionary["P"] as PdfReferenceHolder;
				}
				if (pdfReferenceHolder != null && pdfReferenceHolder == pageReferenceHolder)
				{
					pdfArray.RemoveAt(num);
					pdfArray.MarkChanged();
					result = true;
				}
			}
			if (pdfArray.Count == 0)
			{
				removeField = true;
			}
		}
		return result;
	}

	private PdfField InsertLoadedField(PdfLoadedField field, PdfPageBase newPage)
	{
		if (!(newPage as PdfPage).Section.ParentDocument.EnableMemoryOptimization)
		{
			PdfDictionary dictionary = field.Dictionary;
			PdfDictionary dictionary2 = field.Page.Dictionary;
			PdfDictionary dictionary3 = newPage.Dictionary;
			field = field.Clone(newPage) as PdfLoadedField;
			PdfArray pdfArray = field.CrossTable.GetObject(dictionary["Kids"]) as PdfArray;
			PdfArray pdfArray2 = field.CrossTable.GetObject(dictionary2["Annots"]) as PdfArray;
			PdfArray pdfArray3 = field.CrossTable.GetObject(dictionary3["Annots"]) as PdfArray;
			if (pdfArray != null)
			{
				pdfArray = new PdfArray(pdfArray);
				field.Dictionary["Kids"] = pdfArray;
				UpdateReferences(pdfArray, pdfArray2, pdfArray3, field);
				field.Dictionary.Remove("P");
			}
			else
			{
				PdfReferenceHolder element = new PdfReferenceHolder(dictionary);
				int num = pdfArray2.IndexOf(element);
				if (num >= 0 && pdfArray3.Count > num)
				{
					field.Dictionary = PdfCrossTable.Dereference(pdfArray3[num]) as PdfDictionary;
				}
			}
			return field;
		}
		PdfArray pdfArray4 = null;
		int num2 = 0;
		if (newPage.Dictionary.ContainsKey("Annots"))
		{
			pdfArray4 = newPage.ObtainAnnotations();
			num2 = pdfArray4.Count;
		}
		else
		{
			pdfArray4 = new PdfArray();
			newPage.Dictionary.SetProperty("Annots", pdfArray4);
		}
		PdfField pdfField = field.Clone(newPage);
		if (pdfField is PdfLoadedTextBoxField)
		{
			PdfDictionary dictionary4 = (pdfField as PdfLoadedTextBoxField).Dictionary;
			if (dictionary4.ContainsKey("V"))
			{
				if (dictionary4["V"] is PdfString)
				{
					(dictionary4["V"] as PdfString).IsFormField = true;
				}
				else if (dictionary4["V"] is PdfReferenceHolder)
				{
					((dictionary4["V"] as PdfReferenceHolder).Object as PdfString).IsFormField = true;
				}
			}
		}
		bool flag = false;
		if (field.CrossTable.GetObject(field.Dictionary["Kids"]) is PdfArray { Count: >0 } pdfArray5 && !flag)
		{
			PdfCrossTable crossTable = (newPage as PdfPage).Section.ParentDocument.CrossTable;
			if (pdfField is PdfLoadedCheckBoxField)
			{
				PdfLoadedCheckBoxField pdfLoadedCheckBoxField = pdfField as PdfLoadedCheckBoxField;
				if (pdfLoadedCheckBoxField.Items.Count > 0)
				{
					pdfLoadedCheckBoxField.Items.DoClear();
				}
			}
			for (int i = 0; i < pdfArray5.Count; i++)
			{
				PdfReferenceHolder pdfReferenceHolder = pdfArray5[i] as PdfReferenceHolder;
				PdfDictionary pdfDictionary = PdfCrossTable.Dereference(pdfArray5[i]) as PdfDictionary;
				PdfDictionary pdfDictionary2 = new PdfDictionary(pdfDictionary);
				PdfName key = new PdfName("Parent");
				PdfName key2 = new PdfName("P");
				pdfDictionary2.Remove(key);
				pdfDictionary2.Remove(key2);
				PdfDictionary pdfDictionary3 = pdfDictionary2.Clone(crossTable) as PdfDictionary;
				pdfDictionary3[key] = new PdfReferenceHolder(pdfField);
				PdfPageBase pdfPageBase = null;
				bool flag2 = false;
				if (pdfDictionary.ContainsKey(key2))
				{
					if (!(PdfCrossTable.Dereference(pdfDictionary[key2]) is PdfDictionary key3) || !field.CrossTable.PageCorrespondance.ContainsKey(key3) || field.CrossTable.PageCorrespondance[key3] == null)
					{
						continue;
					}
					pdfPageBase = field.CrossTable.PageCorrespondance[key3] as PdfPageBase;
					if (pdfPageBase != newPage)
					{
						continue;
					}
					pdfDictionary3[key2] = new PdfReferenceHolder(pdfPageBase);
					flag2 = true;
				}
				else
				{
					pdfDictionary3[key2] = new PdfReferenceHolder(newPage);
					pdfPageBase = newPage;
					PdfDictionary dictionary5 = field.Page.Dictionary;
					if (dictionary5 != null && field.CrossTable.GetObject(dictionary5["Annots"]) is PdfArray pdfArray6 && pdfArray6.Contains(pdfArray5[i]))
					{
						flag2 = true;
					}
				}
				if (!flag2)
				{
					continue;
				}
				PdfLoadedFieldItem pdfLoadedFieldItem = (pdfField as PdfLoadedField).CreateLoadedItem(pdfDictionary3);
				if (pdfLoadedFieldItem != null && pdfLoadedFieldItem.Page != null && pdfPageBase != null)
				{
					pdfLoadedFieldItem.Page = pdfPageBase;
				}
				pdfArray4 = newPage.ObtainAnnotations();
				if (num2 < pdfArray4.Count)
				{
					for (int num3 = pdfArray4.Count - 1; num3 >= num2; num3--)
					{
						pdfArray4.RemoveAt(num3);
					}
				}
				pdfArray4.Add(new PdfReferenceHolder(pdfDictionary3));
				if (field.Page.IsTagged && pdfReferenceHolder != null && pdfReferenceHolder.Reference != null)
				{
					newPage.ImportedAnnotationReference[pdfReferenceHolder.Reference.ObjNum] = pdfDictionary3;
				}
				num2++;
			}
		}
		else
		{
			if (field.Page.IsTagged)
			{
				PdfReference reference = (field.Page as PdfLoadedPage).CrossTable.GetReference(field.Dictionary);
				if (reference != null)
				{
					newPage.ImportedAnnotationReference[reference.ObjNum] = pdfField.Dictionary;
				}
			}
			pdfArray4 = newPage.ObtainAnnotations();
			if (num2 < pdfArray4.Count)
			{
				for (int num4 = pdfArray4.Count - 1; num4 >= num2; num4--)
				{
					pdfArray4.RemoveAt(num4);
				}
			}
			pdfArray4.Add(new PdfReferenceHolder(pdfField));
			num2++;
		}
		return pdfField;
	}

	private void UpdateReferences(PdfArray kidsArray, PdfArray array, PdfArray newArray, PdfField field)
	{
		if (kidsArray == null)
		{
			return;
		}
		PdfLoadedField pdfLoadedField = field as PdfLoadedField;
		int i = 0;
		for (int count = kidsArray.Count; i < count; i++)
		{
			PdfReferenceHolder element = kidsArray[i] as PdfReferenceHolder;
			if (array == null)
			{
				continue;
			}
			int num = array.IndexOf(element);
			if (num < 0 && pdfLoadedField != null)
			{
				PdfLoadedPageCollection pages = (pdfLoadedField.CrossTable.Document as PdfLoadedDocument).Pages;
				Dictionary<IPdfPrimitive, object> pageCorrespondance = pdfLoadedField.CrossTable.PageCorrespondance;
				for (int j = 0; j < pages.Count; j++)
				{
					PdfPageBase pdfPageBase = pages[j];
					if (!pageCorrespondance.ContainsKey(((IPdfWrapper)pdfPageBase).Element) || !(pageCorrespondance[((IPdfWrapper)pdfPageBase).Element] is PdfPageBase pdfPageBase2))
					{
						continue;
					}
					newArray = pdfLoadedField.CrossTable.GetObject(pdfPageBase2.Dictionary["Annots"]) as PdfArray;
					if (!(PdfCrossTable.Dereference(kidsArray[i]) is PdfDictionary pdfDictionary) || !pdfDictionary.ContainsKey("P") || !(PdfCrossTable.Dereference(pdfDictionary["P"]) is PdfDictionary pdfDictionary2))
					{
						continue;
					}
					array = pdfLoadedField.CrossTable.GetObject(pdfDictionary2["Annots"]) as PdfArray;
					if (pdfLoadedField.CrossTable.PageCorrespondance[pdfDictionary2] is PdfPageBase pdfPageBase3 && pdfPageBase3 == pdfPageBase2 && array != null && newArray != null && array.Count == newArray.Count)
					{
						num = array.IndexOf(element);
						if (num >= 0)
						{
							break;
						}
					}
				}
			}
			if (num >= 0 && newArray != null && num < newArray.Count)
			{
				IPdfPrimitive pdfPrimitive = newArray[num];
				kidsArray.RemoveAt(i);
				kidsArray.Insert(i, pdfPrimitive);
				if (PdfCrossTable.Dereference(pdfPrimitive) is PdfDictionary pdfDictionary3 && pdfDictionary3.ContainsKey("Parent"))
				{
					pdfDictionary3["Parent"] = new PdfReferenceHolder(field);
				}
			}
		}
	}

	private int GetFieldIndex(string name)
	{
		int num = -1;
		if (m_fieldNames == null)
		{
			m_fieldNames = new Dictionary<string, int>();
			foreach (PdfField item in base.List)
			{
				num++;
				string[] array = item.Name.Split('[');
				m_fieldNames.Add(array[0], num);
			}
		}
		int value = -1;
		m_fieldNames.TryGetValue(name, out value);
		return value;
	}
}
