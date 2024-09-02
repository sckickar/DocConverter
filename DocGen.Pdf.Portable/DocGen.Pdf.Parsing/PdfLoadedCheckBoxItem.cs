using System.Collections.Generic;
using DocGen.Pdf.Interactive;
using DocGen.Pdf.IO;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Parsing;

public class PdfLoadedCheckBoxItem : PdfLoadedStateItem
{
	private string exportValue;

	public PdfCheckBoxStyle Style
	{
		get
		{
			return base.Field.ObtainStyle();
		}
		set
		{
			base.Field.AssignStyle(value);
			if (!base.Field.Form.NeedAppearances)
			{
				base.Field.Changed = true;
				base.Field.FieldChanged = true;
			}
		}
	}

	public string ExportValue
	{
		get
		{
			if (exportValue == null)
			{
				exportValue = TryGetExportValue();
			}
			return exportValue;
		}
	}

	internal PdfLoadedCheckBoxItem(PdfLoadedStyledField field, int index, PdfDictionary dictionary)
		: base(field, index, dictionary)
	{
	}

	private void SetCheckedStatus(bool value)
	{
		string itemValue = PdfLoadedStateField.GetItemValue(base.Dictionary, base.CrossTable);
		if (value)
		{
			(base.Parent as PdfLoadedCheckBoxField).UncheckOthers(this, itemValue, value);
			base.Parent.Dictionary.SetName("V", itemValue);
			base.Dictionary.SetProperty("AS", new PdfName(itemValue));
		}
		else
		{
			PdfName pdfName = PdfCrossTable.Dereference(base.Parent.Dictionary["V"]) as PdfName;
			if (pdfName != null && itemValue == pdfName.Value)
			{
				base.Parent.Dictionary.Remove("V");
			}
			base.Dictionary.SetProperty("AS", new PdfName("Off"));
		}
		base.Parent.Changed = true;
	}

	internal PdfLoadedCheckBoxItem Clone()
	{
		return (PdfLoadedCheckBoxItem)MemberwiseClone();
	}

	private string TryGetExportValue()
	{
		exportValue = null;
		if (base.Dictionary.ContainsKey("AP"))
		{
			PdfDictionary pdfDictionary = PdfCrossTable.Dereference(base.Dictionary["AP"]) as PdfDictionary;
			PdfName pdfName = null;
			if (pdfDictionary == null || !pdfDictionary.ContainsKey("N"))
			{
				return exportValue;
			}
			if (PdfCrossTable.Dereference(pdfDictionary["N"]) is PdfDictionary pdfDictionary2)
			{
				List<object> list = new List<object>();
				foreach (PdfName key in pdfDictionary2.Keys)
				{
					list.Add(key);
				}
				int i = 0;
				for (int count = list.Count; i < count; i++)
				{
					pdfName = list[i] as PdfName;
					if (pdfName != null && pdfName.Value != "Off")
					{
						exportValue = pdfName.Value;
						return exportValue;
					}
				}
			}
		}
		return exportValue;
	}
}
