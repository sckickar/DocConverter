using System;
using System.Collections.Generic;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Interactive;
using DocGen.Pdf.IO;
using DocGen.Pdf.Primitives;
using DocGen.Pdf.Xfa;

namespace DocGen.Pdf.Parsing;

public class PdfLoadedCheckBoxField : PdfLoadedStateField
{
	private const string CHECK_SYMBOL = "4";

	public bool Checked
	{
		get
		{
			bool result = false;
			if (Items.Count > 0)
			{
				result = Items[base.DefaultIndex].Checked;
			}
			return result;
		}
		set
		{
			if ((FieldFlags.ReadOnly & Flags) == 0)
			{
				if (Items.Count > 0)
				{
					Items[base.DefaultIndex].Checked = value;
				}
				else
				{
					SetCheckedStatus(value);
				}
				((PdfField)this).Form.SetAppearanceDictionary = true;
				if (base.Form.m_enableXfaFormfill && (base.Form as PdfLoadedForm).IsXFAForm)
				{
					string fieldName = Name.Replace("\\", string.Empty);
					if ((base.Form as PdfLoadedForm).GetXfaField(fieldName) is PdfLoadedXfaCheckBoxField pdfLoadedXfaCheckBoxField)
					{
						pdfLoadedXfaCheckBoxField.IsChecked = Checked;
					}
				}
			}
			NotifyPropertyChanged("Checked");
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
			AssignBackColor(value);
			if (!base.Form.NeedAppearances)
			{
				base.Changed = true;
				base.FieldChanged = true;
			}
			NotifyPropertyChanged("BackColor");
		}
	}

	public new PdfColor ForeColor
	{
		get
		{
			return base.ForeColor;
		}
		set
		{
			base.ForeColor = value;
			if (!base.Form.NeedAppearances)
			{
				base.Changed = true;
				base.FieldChanged = true;
			}
			NotifyPropertyChanged("ForeColor");
		}
	}

	public new PdfCheckBoxStyle Style
	{
		get
		{
			return base.Style;
		}
		set
		{
			if (base.Style != value)
			{
				base.FieldChanged = true;
			}
			base.Style = value;
			if (!base.Form.NeedAppearances)
			{
				base.Changed = true;
			}
			NotifyPropertyChanged("Style");
		}
	}

	public new PdfLoadedCheckBoxItemCollection Items
	{
		get
		{
			return base.Items as PdfLoadedCheckBoxItemCollection;
		}
		internal set
		{
			base.Items = value;
		}
	}

	internal PdfLoadedCheckBoxField(PdfDictionary dictionary, PdfCrossTable crossTable)
		: base(dictionary, crossTable, new PdfLoadedCheckBoxItemCollection())
	{
	}

	internal override PdfLoadedStateItem GetItem(int index, PdfDictionary itemDictionary)
	{
		return new PdfLoadedCheckBoxItem(this, index, itemDictionary);
	}

	internal override void Draw()
	{
		base.Draw();
		PdfArray kids = base.Kids;
		if (kids != null)
		{
			for (int i = 0; i < kids.Count; i++)
			{
				if (!CheckFieldFlagValue(kids[i]))
				{
					PdfLoadedCheckBoxItem pdfLoadedCheckBoxItem = Items[i];
					PdfCheckFieldState state = (pdfLoadedCheckBoxItem.Checked ? PdfCheckFieldState.Checked : PdfCheckFieldState.Unchecked);
					if (pdfLoadedCheckBoxItem.Page != null)
					{
						DrawStateItem(pdfLoadedCheckBoxItem.Page.Graphics, state, pdfLoadedCheckBoxItem);
					}
				}
			}
		}
		else
		{
			PdfCheckFieldState state2 = (Checked ? PdfCheckFieldState.Checked : PdfCheckFieldState.Unchecked);
			DrawStateItem(Page.Graphics, state2, null);
		}
	}

	internal override void BeginSave()
	{
		base.BeginSave();
		PdfArray kids = base.Kids;
		if (kids != null && kids.Count == Items.Count)
		{
			for (int i = 0; i < kids.Count; i++)
			{
				PdfDictionary widget = base.CrossTable.GetObject(kids[i]) as PdfDictionary;
				ApplyAppearance(widget, Items[i]);
			}
		}
		else
		{
			PdfDictionary widgetAnnotation = GetWidgetAnnotation(base.Dictionary, base.CrossTable);
			ApplyAppearance(widgetAnnotation, null);
		}
	}

	internal new PdfField Clone(PdfDictionary dictionary, PdfPage page)
	{
		PdfCrossTable crossTable = page.Section.ParentDocument.CrossTable;
		PdfLoadedCheckBoxField pdfLoadedCheckBoxField = new PdfLoadedCheckBoxField(dictionary, crossTable);
		pdfLoadedCheckBoxField.Page = page;
		pdfLoadedCheckBoxField.SetName(GetFieldName());
		pdfLoadedCheckBoxField.Widget.Dictionary = base.Widget.Dictionary.Clone(crossTable) as PdfDictionary;
		return pdfLoadedCheckBoxField;
	}

	internal new PdfLoadedStyledField Clone()
	{
		PdfLoadedCheckBoxField pdfLoadedCheckBoxField = MemberwiseClone() as PdfLoadedCheckBoxField;
		pdfLoadedCheckBoxField.Dictionary = base.Dictionary.Clone(base.CrossTable) as PdfDictionary;
		pdfLoadedCheckBoxField.Widget.Dictionary = base.Widget.Dictionary.Clone(base.CrossTable) as PdfDictionary;
		pdfLoadedCheckBoxField.Items = new PdfLoadedCheckBoxItemCollection();
		for (int i = 0; i < Items.Count; i++)
		{
			PdfLoadedCheckBoxItem item = new PdfLoadedCheckBoxItem(pdfLoadedCheckBoxField, i, Items[i].Dictionary);
			pdfLoadedCheckBoxField.Items.Add(item);
		}
		return pdfLoadedCheckBoxField;
	}

	internal override PdfLoadedFieldItem CreateLoadedItem(PdfDictionary dictionary)
	{
		base.CreateLoadedItem(dictionary);
		PdfLoadedCheckBoxItem pdfLoadedCheckBoxItem = null;
		if (Items != null)
		{
			pdfLoadedCheckBoxItem = new PdfLoadedCheckBoxItem(this, Items.Count, dictionary);
			Items.Add(pdfLoadedCheckBoxItem);
		}
		if (base.Kids == null)
		{
			base.Dictionary["Kids"] = new PdfArray();
		}
		base.Kids.Add(new PdfReferenceHolder(dictionary));
		return pdfLoadedCheckBoxItem;
	}

	public new void RemoveAt(int index)
	{
		if (Items != null && Items.Count != 0)
		{
			PdfLoadedCheckBoxItem item = Items[index];
			Remove(item);
		}
	}

	public void Remove(PdfLoadedCheckBoxItem item)
	{
		if (item == null)
		{
			throw new NullReferenceException("item");
		}
		int index = Items.Remove(item);
		if (base.Dictionary["Kids"] is PdfArray pdfArray)
		{
			pdfArray.RemoveAt(index);
			pdfArray.MarkChanged();
		}
	}

	public bool TryGetFieldItem(string exportValue, out PdfLoadedCheckBoxItem field)
	{
		field = null;
		if (base.Kids != null || Items != null)
		{
			for (int i = 0; i < Items.Count; i++)
			{
				PdfLoadedCheckBoxItem pdfLoadedCheckBoxItem = Items[i];
				if (pdfLoadedCheckBoxItem == null || pdfLoadedCheckBoxItem.Dictionary == null || !pdfLoadedCheckBoxItem.Dictionary.ContainsKey("AS"))
				{
					continue;
				}
				PdfName pdfName = PdfCrossTable.Dereference(pdfLoadedCheckBoxItem.Dictionary["AS"]) as PdfName;
				if (pdfLoadedCheckBoxItem.Dictionary.ContainsKey("AP"))
				{
					PdfDictionary pdfDictionary = PdfCrossTable.Dereference(pdfLoadedCheckBoxItem.Dictionary["AP"]) as PdfDictionary;
					PdfName pdfName2 = null;
					if (pdfDictionary == null || !pdfDictionary.ContainsKey("N") || !(PdfCrossTable.Dereference(pdfDictionary["N"]) is PdfDictionary pdfDictionary2))
					{
						continue;
					}
					List<object> list = new List<object>();
					foreach (PdfName key in pdfDictionary2.Keys)
					{
						list.Add(key);
					}
					int j = 0;
					for (int count = list.Count; j < count; j++)
					{
						pdfName2 = list[j] as PdfName;
						if (pdfName2 != null && pdfName2.Value != "Off" && pdfName2.Value == exportValue)
						{
							field = pdfLoadedCheckBoxItem;
							return true;
						}
					}
				}
				else if (pdfName != null && pdfName.Value != null && pdfName.Value == exportValue)
				{
					field = pdfLoadedCheckBoxItem;
					return true;
				}
			}
		}
		return false;
	}
}
