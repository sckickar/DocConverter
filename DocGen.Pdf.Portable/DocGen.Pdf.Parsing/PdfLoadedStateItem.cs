using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Interactive;
using DocGen.Pdf.IO;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Parsing;

public class PdfLoadedStateItem : PdfLoadedFieldItem
{
	public bool Checked
	{
		get
		{
			bool result = false;
			PdfName pdfName = PdfCrossTable.Dereference(base.Dictionary["AS"]) as PdfName;
			if (pdfName == null)
			{
				PdfName pdfName2 = PdfLoadedField.GetValue(base.Parent.Dictionary, base.Parent.CrossTable, "V", inheritable: false) as PdfName;
				if (pdfName2 != null)
				{
					result = pdfName2.Value == PdfLoadedStateField.GetItemValue(base.Dictionary, base.CrossTable);
				}
			}
			else
			{
				result = pdfName.Value != "Off";
			}
			return result;
		}
		set
		{
			if ((FieldFlags.ReadOnly & base.Field.Flags) == 0 && value != Checked)
			{
				SetCheckedStatus(value);
				((PdfField)base.Field).Form.SetAppearanceDictionary = true;
			}
			base.Field.NotifyPropertyChanged("Checked", m_collectionIndex);
		}
	}

	public PdfColor BackColor
	{
		get
		{
			return GetBackColor();
		}
		set
		{
			PdfDictionary widgetAnnotation = base.Field.GetWidgetAnnotation(base.Dictionary, base.CrossTable);
			if (widgetAnnotation != null)
			{
				if (widgetAnnotation.ContainsKey("MK"))
				{
					PdfDictionary obj = base.CrossTable.GetObject(widgetAnnotation["MK"]) as PdfDictionary;
					PdfArray value2 = value.ToArray();
					obj["BG"] = value2;
				}
				else
				{
					PdfDictionary pdfDictionary = new PdfDictionary();
					PdfArray value3 = value.ToArray();
					pdfDictionary["BG"] = value3;
					widgetAnnotation["MK"] = pdfDictionary;
				}
				((PdfField)base.Field).Form.SetAppearanceDictionary = true;
			}
			if (!base.Field.Form.NeedAppearances)
			{
				base.Field.Changed = true;
				base.Field.FieldChanged = true;
			}
			base.Field.NotifyPropertyChanged("BackColor", m_collectionIndex);
		}
	}

	public PdfColor ForeColor
	{
		get
		{
			PdfDictionary widgetAnnotation = base.Field.GetWidgetAnnotation(base.Dictionary, base.CrossTable);
			PdfColor result = new PdfColor(0, 0, 0);
			if (widgetAnnotation != null && widgetAnnotation.ContainsKey("DA"))
			{
				PdfString pdfString = base.CrossTable.GetObject(widgetAnnotation["DA"]) as PdfString;
				return base.Field.GetForeColour(pdfString.Value);
			}
			if (widgetAnnotation != null && widgetAnnotation.GetValue(base.CrossTable, "DA", "Parent") is PdfString pdfString2)
			{
				return base.Field.GetForeColour(pdfString2.Value);
			}
			return result;
		}
		set
		{
			PdfDictionary widgetAnnotation = base.Field.GetWidgetAnnotation(base.Dictionary, base.CrossTable);
			float height = 0f;
			string text = null;
			if (widgetAnnotation != null && widgetAnnotation.ContainsKey("DA"))
			{
				PdfString pdfString = widgetAnnotation["DA"] as PdfString;
				text = base.Field.FontName(pdfString.Value, out height);
			}
			else if (widgetAnnotation != null && base.Dictionary.ContainsKey("DA"))
			{
				PdfString pdfString2 = base.Dictionary["DA"] as PdfString;
				text = base.Field.FontName(pdfString2.Value, out height);
			}
			if (text != null)
			{
				PdfDefaultAppearance pdfDefaultAppearance = new PdfDefaultAppearance();
				pdfDefaultAppearance.FontName = text;
				pdfDefaultAppearance.FontSize = height;
				pdfDefaultAppearance.ForeColor = value;
				widgetAnnotation["DA"] = new PdfString(pdfDefaultAppearance.ToString());
			}
			else
			{
				PdfDefaultAppearance pdfDefaultAppearance2 = new PdfDefaultAppearance();
				pdfDefaultAppearance2.FontName = base.Font.Name;
				pdfDefaultAppearance2.FontSize = base.Font.Size;
				pdfDefaultAppearance2.ForeColor = value;
				widgetAnnotation["DA"] = new PdfString(pdfDefaultAppearance2.ToString());
			}
			((PdfField)base.Field).Form.SetAppearanceDictionary = true;
			if (!base.Field.Form.NeedAppearances)
			{
				base.Field.Changed = true;
				base.Field.FieldChanged = true;
			}
			base.Field.NotifyPropertyChanged("ForeColor", m_collectionIndex);
		}
	}

	public PdfColor BorderColor
	{
		get
		{
			PdfDictionary widgetAnnotation = base.Field.GetWidgetAnnotation(base.Dictionary, base.CrossTable);
			PdfColor result = new PdfColor(Color.Transparent);
			if (widgetAnnotation.ContainsKey("MK"))
			{
				PdfDictionary pdfDictionary = base.CrossTable.GetObject(widgetAnnotation["MK"]) as PdfDictionary;
				if (pdfDictionary.ContainsKey("BC"))
				{
					PdfArray array = pdfDictionary["BC"] as PdfArray;
					return CreateColor(array);
				}
			}
			return result;
		}
		set
		{
			PdfDictionary widgetAnnotation = base.Field.GetWidgetAnnotation(base.Dictionary, base.CrossTable);
			if (widgetAnnotation != null)
			{
				if (widgetAnnotation.ContainsKey("MK"))
				{
					PdfDictionary obj = base.CrossTable.GetObject(widgetAnnotation["MK"]) as PdfDictionary;
					PdfArray value2 = value.ToArray();
					obj["BC"] = value2;
				}
				else
				{
					PdfDictionary pdfDictionary = new PdfDictionary();
					PdfArray value3 = value.ToArray();
					pdfDictionary["BC"] = value3;
					widgetAnnotation["MK"] = pdfDictionary;
				}
				((PdfField)base.Field).Form.SetAppearanceDictionary = true;
			}
			base.Field.Changed = true;
			base.Field.FieldChanged = true;
			base.Field.NotifyPropertyChanged("BorderColor", m_collectionIndex);
		}
	}

	internal PdfLoadedStateItem(PdfLoadedStyledField field, int index, PdfDictionary dictionary)
		: base(field, index, dictionary)
	{
	}

	private void SetCheckedStatus(bool value)
	{
		string text = PdfLoadedStateField.GetItemValue(base.Dictionary, base.CrossTable);
		(base.Parent as PdfLoadedStateField).UncheckOthers(this, text, value);
		if (value)
		{
			if (text == null || text == string.Empty)
			{
				text = "Yes";
			}
			base.Parent.Dictionary.SetName("V", text);
			base.Dictionary.SetProperty("AS", new PdfName(text));
			base.Dictionary.SetProperty("V", new PdfName(text));
		}
		else
		{
			PdfName pdfName = PdfCrossTable.Dereference(base.Parent.Dictionary["V"]) as PdfName;
			if (pdfName != null && text == pdfName.Value)
			{
				base.Parent.Dictionary.Remove("V");
			}
			base.Dictionary.SetProperty("AS", new PdfName("Off"));
		}
		base.Parent.Changed = true;
	}

	internal PdfColor GetBackColor()
	{
		PdfDictionary widgetAnnotation = base.Field.GetWidgetAnnotation(base.Dictionary, base.CrossTable);
		PdfColor result = default(PdfColor);
		if (widgetAnnotation.ContainsKey("MK"))
		{
			PdfDictionary pdfDictionary = base.CrossTable.GetObject(widgetAnnotation["MK"]) as PdfDictionary;
			if (pdfDictionary.ContainsKey("BG"))
			{
				PdfArray array = pdfDictionary["BG"] as PdfArray;
				return CreateColor(array);
			}
		}
		return result;
	}

	private PdfColor CreateColor(PdfArray array)
	{
		int count = array.Count;
		PdfColor result = PdfColor.Empty;
		float[] array2 = new float[array.Count];
		int i = 0;
		for (int count2 = array.Count; i < count2; i++)
		{
			PdfNumber pdfNumber = base.CrossTable.GetObject(array[i]) as PdfNumber;
			array2[i] = pdfNumber.FloatValue;
		}
		switch (count)
		{
		case 1:
			result = ((!((double)array2[0] > 0.0) || !((double)array2[0] <= 1.0)) ? new PdfColor((int)(byte)array2[0]) : new PdfColor(array2[0]));
			break;
		case 3:
			result = (((!((double)array2[0] > 0.0) || !((double)array2[0] <= 1.0)) && (!((double)array2[1] > 0.0) || !((double)array2[1] <= 1.0)) && (!((double)array2[2] > 0.0) || !((double)array2[2] <= 1.0))) ? new PdfColor((byte)array2[0], (byte)array2[1], (byte)array2[2]) : new PdfColor(array2[0], array2[1], array2[2]));
			break;
		case 4:
			result = (((!((double)array2[0] > 0.0) || !((double)array2[0] <= 1.0)) && (!((double)array2[1] > 0.0) || !((double)array2[1] <= 1.0)) && (!((double)array2[2] > 0.0) || !((double)array2[2] <= 1.0)) && (!((double)array2[3] > 0.0) || !((double)array2[3] <= 1.0))) ? new PdfColor((byte)array2[0], (byte)array2[1], (byte)array2[2], (byte)array2[3]) : new PdfColor(array2[0], array2[1], array2[2], array2[3]));
			break;
		}
		return result;
	}
}
