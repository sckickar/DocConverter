using System;
using System.Collections;
using DocGen.Pdf.Parsing;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public class PdfFormFieldCollection : PdfFieldCollection
{
	private PdfForm m_form;

	internal PdfForm Form
	{
		get
		{
			return m_form;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("form");
			}
			m_form = value;
		}
	}

	protected override int DoAdd(PdfField field)
	{
		field.SetForm(Form);
		string empty = string.Empty;
		empty = ((!(field is PdfLoadedField)) ? field.Name : (field as PdfLoadedField).ActualFieldName);
		if (string.IsNullOrEmpty(empty))
		{
			empty = Guid.NewGuid().ToString();
		}
		m_form.FieldNames.Add(empty);
		if (m_form.FieldAutoNaming)
		{
			string correctName = m_form.GetCorrectName(empty);
			field.ApplyName(correctName);
		}
		else
		{
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
						if (!(pdfField.Name == field.Name) || !(field is PdfTextBoxField) || !(pdfField is PdfTextBoxField))
						{
							continue;
						}
						(field as PdfTextBoxField).Widget.Dictionary?.Remove("Parent");
						(field as PdfTextBoxField).Widget.Parent = pdfField;
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
						}
						if (!(pdfField as PdfTextBoxField).m_array.Contains(new PdfReferenceHolder((pdfField as PdfTextBoxField).Widget)))
						{
							(pdfField as PdfTextBoxField).m_array.Add(new PdfReferenceHolder((pdfField as PdfTextBoxField).Widget));
						}
						(pdfField as PdfTextBoxField).m_array.Add(new PdfReferenceHolder((field as PdfTextBoxField).Widget));
						pdfField.Dictionary.SetProperty("Kids", (pdfField as PdfTextBoxField).m_array);
						(pdfField as PdfTextBoxField).fieldItems.Add(field);
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
		PdfForm form = m_form;
		if (form != null && !(field is PdfLoadedField))
		{
			FormFieldsAddedArgs formFieldsAddedArgs = new FormFieldsAddedArgs(field);
			formFieldsAddedArgs.MethodName = "Field Add";
			form.OnFormFieldAdded(formFieldsAddedArgs);
		}
		return base.DoAdd(field);
	}

	protected override void DoInsert(int index, PdfField field)
	{
		if (!IsValidName(field.Name))
		{
			throw new PdfDocumentException(string.Format(c_exisingFieldException, field.Name));
		}
		field.SetForm(Form);
		base.DoInsert(index, field);
	}

	protected override void DoRemove(PdfField field)
	{
		field.SetForm(null);
		PdfForm form = m_form;
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
		PdfField pdfField = (PdfField)base.Items[index];
		pdfField.SetForm(null);
		PdfForm form = m_form;
		if (form != null)
		{
			FormFieldsRemovedArgs formFieldsRemovedArgs = new FormFieldsRemovedArgs(pdfField);
			formFieldsRemovedArgs.Index = index;
			formFieldsRemovedArgs.MethodName = "Field Remove At";
			form.OnFormFieldRemoved(formFieldsRemovedArgs);
		}
		base.DoRemoveAt(index);
	}

	protected override void DoClear()
	{
		IEnumerator enumerator = GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				PdfField pdfField = (PdfField)enumerator.Current;
				PdfForm form = m_form;
				if (form != null)
				{
					FormFieldsRemovedArgs formFieldsRemovedArgs = new FormFieldsRemovedArgs(pdfField);
					formFieldsRemovedArgs.MethodName = "Field Clear";
					form.OnFormFieldRemoved(formFieldsRemovedArgs);
				}
				m_form.DeleteFromPages(pdfField);
				m_form.DeleteAnnotation(pdfField);
				pdfField.Page = null;
				pdfField.Dictionary.Clear();
				pdfField.SetForm(null);
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
		base.DoClear();
	}

	private bool IsValidName(string name)
	{
		return m_form.FieldNames.Contains(name);
	}
}
