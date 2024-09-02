namespace DocGen.Pdf.Xfa;

public class PdfXfaFieldCollection : PdfCollection
{
	private int subFormCount = 1;

	public PdfXfaField this[int offset] => base.List[offset] as PdfXfaField;

	public new int Count => base.List.Count;

	public void Add(PdfXfaField field)
	{
		if (field is PdfXfaRadioButtonField)
		{
			throw new PdfException("Can't add single radio button, need to add the radio button in group (PdfXfaRadioButtonGroup).");
		}
		if (field is PdfXfaForm)
		{
			base.List.Add((field as PdfXfaForm).Clone());
		}
		else
		{
			base.List.Add(field);
		}
	}

	public void Remove(PdfXfaField field)
	{
		if (base.List.Contains(field))
		{
			base.List.Remove(field);
		}
	}

	public void RemoveAt(int index)
	{
		base.List.RemoveAt(index);
	}

	public void Clear()
	{
		base.List.Clear();
	}

	public void insert(int index, PdfXfaField field)
	{
		if (field is PdfXfaRadioButtonField)
		{
			throw new PdfException("Can't insert single radio button, need to add the radio button in group (PdfXfaRadioButtonGroup).");
		}
		if (field is PdfXfaForm)
		{
			base.List.Insert(index, (field as PdfXfaForm).Clone());
		}
		else
		{
			base.List.Insert(index, field);
		}
	}

	public int IndexOf(PdfXfaField field)
	{
		return base.List.IndexOf(field);
	}

	internal object Clone()
	{
		PdfXfaFieldCollection pdfXfaFieldCollection = new PdfXfaFieldCollection();
		foreach (PdfXfaField item in base.List)
		{
			if (item is PdfXfaForm)
			{
				pdfXfaFieldCollection.Add((item as PdfXfaForm).Clone() as PdfXfaField);
			}
			else if (item is PdfXfaRectangleField)
			{
				pdfXfaFieldCollection.Add((item as PdfXfaRectangleField).Clone() as PdfXfaField);
			}
			else if (item is PdfXfaCircleField)
			{
				pdfXfaFieldCollection.Add((item as PdfXfaCircleField).Clone() as PdfXfaField);
			}
			else if (item is PdfXfaTextBoxField)
			{
				pdfXfaFieldCollection.Add((item as PdfXfaTextBoxField).Clone() as PdfXfaField);
			}
			else if (item is PdfXfaNumericField)
			{
				pdfXfaFieldCollection.Add((item as PdfXfaNumericField).Clone() as PdfXfaField);
			}
			else if (item is PdfXfaButtonField)
			{
				pdfXfaFieldCollection.Add((item as PdfXfaButtonField).Clone() as PdfXfaField);
			}
			else if (item is PdfXfaCheckBoxField)
			{
				pdfXfaFieldCollection.Add((item as PdfXfaCheckBoxField).Clone() as PdfXfaField);
			}
			else if (item is PdfXfaDateTimeField)
			{
				pdfXfaFieldCollection.Add((item as PdfXfaDateTimeField).Clone() as PdfXfaField);
			}
			else if (item is PdfXfaComboBoxField)
			{
				pdfXfaFieldCollection.Add((item as PdfXfaComboBoxField).Clone() as PdfXfaField);
			}
			else if (item is PdfXfaListBoxField)
			{
				pdfXfaFieldCollection.Add((item as PdfXfaListBoxField).Clone() as PdfXfaField);
			}
			else if (item is PdfXfaImage)
			{
				pdfXfaFieldCollection.Add((item as PdfXfaImage).Clone() as PdfXfaField);
			}
			else if (item is PdfXfaLine)
			{
				pdfXfaFieldCollection.Add((item as PdfXfaLine).Clone() as PdfXfaField);
			}
			else if (item is PdfXfaTextElement)
			{
				pdfXfaFieldCollection.Add((item as PdfXfaTextElement).Clone() as PdfXfaField);
			}
			else if (item is PdfXfaRadioButtonGroup)
			{
				pdfXfaFieldCollection.Add((item as PdfXfaRadioButtonGroup).Clone() as PdfXfaField);
			}
		}
		return pdfXfaFieldCollection;
	}
}
