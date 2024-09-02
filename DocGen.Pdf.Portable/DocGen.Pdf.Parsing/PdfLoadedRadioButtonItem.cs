using System.Collections.Generic;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Parsing;

public class PdfLoadedRadioButtonItem : PdfLoadedStateItem
{
	private string m_optionValue;

	public string Value
	{
		get
		{
			return GetItemValue();
		}
		set
		{
			SetItemValue(value);
			base.Field.NotifyPropertyChanged("Value", Parent.Items.IndexOf(this));
		}
	}

	public string OptionValue
	{
		get
		{
			return m_optionValue;
		}
		internal set
		{
			m_optionValue = value;
			base.Field.NotifyPropertyChanged("OptionValue", Parent.Items.IndexOf(this));
		}
	}

	public bool Selected
	{
		get
		{
			return Parent.Items.IndexOf(this) == Parent.SelectedIndex;
		}
		set
		{
			if (value)
			{
				int selectedIndex = Parent.Items.IndexOf(this);
				Parent.SelectedIndex = selectedIndex;
			}
			base.Field.NotifyPropertyChanged("Selected", Parent.Items.IndexOf(this));
		}
	}

	internal new PdfLoadedRadioButtonListField Parent => base.Parent as PdfLoadedRadioButtonListField;

	internal PdfLoadedRadioButtonItem(PdfLoadedStyledField field, int index, PdfDictionary dictionary)
		: base(field, index, dictionary)
	{
	}

	private string GetItemValue()
	{
		string text = string.Empty;
		PdfName pdfName = null;
		if (base.Dictionary.ContainsKey("AS"))
		{
			pdfName = base.CrossTable.GetObject(base.Dictionary["AS"]) as PdfName;
			if (pdfName != null && pdfName.Value != "Off")
			{
				text = PdfName.DecodeName(pdfName.Value);
			}
		}
		if (text == string.Empty && base.Dictionary.ContainsKey("AP"))
		{
			PdfDictionary pdfDictionary = base.CrossTable.GetObject(base.Dictionary["AP"]) as PdfDictionary;
			if (pdfDictionary.ContainsKey("N"))
			{
				PdfReference reference = base.CrossTable.GetReference(pdfDictionary["N"]);
				PdfDictionary obj = base.CrossTable.GetObject(reference) as PdfDictionary;
				List<object> list = new List<object>();
				foreach (PdfName key in obj.Keys)
				{
					list.Add(key);
				}
				int i = 0;
				for (int count = list.Count; i < count; i++)
				{
					pdfName = list[i] as PdfName;
					if (pdfName.Value != "Off")
					{
						text = PdfName.DecodeName(pdfName.Value);
						break;
					}
				}
			}
		}
		return text;
	}

	private void SetItemValue(string value)
	{
		if (base.Dictionary.ContainsKey("AP"))
		{
			PdfDictionary pdfDictionary = base.CrossTable.GetObject(base.Dictionary["AP"]) as PdfDictionary;
			if (pdfDictionary.ContainsKey("N"))
			{
				PdfReference reference = base.CrossTable.GetReference(pdfDictionary["N"]);
				pdfDictionary = base.CrossTable.GetObject(reference) as PdfDictionary;
				string itemValue = GetItemValue();
				if (pdfDictionary.ContainsKey(itemValue))
				{
					PdfReference reference2 = base.CrossTable.GetReference(pdfDictionary[itemValue]);
					pdfDictionary.Remove(Value);
					pdfDictionary.SetProperty(value, new PdfReferenceHolder(reference2, base.CrossTable));
				}
			}
		}
		if (value == Parent.SelectedValue)
		{
			base.Dictionary.SetName("AS", value);
		}
		else
		{
			base.Dictionary.SetName("AS", "Off");
		}
	}

	internal PdfLoadedRadioButtonItem Clone()
	{
		return (PdfLoadedRadioButtonItem)MemberwiseClone();
	}
}
