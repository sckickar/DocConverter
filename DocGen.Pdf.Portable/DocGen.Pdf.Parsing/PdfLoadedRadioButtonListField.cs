using System;
using DocGen.Pdf.Interactive;
using DocGen.Pdf.IO;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Parsing;

public class PdfLoadedRadioButtonListField : PdfLoadedStateField
{
	private const string CHECK_SYMBOL = "l";

	public new PdfLoadedRadioButtonItemCollection Items
	{
		get
		{
			return base.Items as PdfLoadedRadioButtonItemCollection;
		}
		internal set
		{
			base.Items = value;
		}
	}

	public int SelectedIndex
	{
		get
		{
			return ObtainSelectedIndex();
		}
		set
		{
			AssignSelectedIndex(value);
			NotifyPropertyChanged("SelectedIndex");
		}
	}

	public string SelectedValue
	{
		get
		{
			int selectedIndex = SelectedIndex;
			if (selectedIndex <= -1)
			{
				return null;
			}
			return Items[selectedIndex].Value;
		}
		set
		{
			AssignSelectedValue(value);
			NotifyPropertyChanged("SelectedValue");
		}
	}

	public PdfLoadedRadioButtonItem SelectedItem
	{
		get
		{
			int selectedIndex = SelectedIndex;
			PdfLoadedRadioButtonItem result = null;
			if (selectedIndex > -1)
			{
				result = Items[selectedIndex];
			}
			return result;
		}
	}

	public string Value
	{
		get
		{
			return Items[base.DefaultIndex].Value;
		}
		set
		{
			Items[base.DefaultIndex].Value = value;
			NotifyPropertyChanged("Value");
		}
	}

	internal PdfLoadedRadioButtonListField(PdfDictionary dictionary, PdfCrossTable crossTable)
		: base(dictionary, crossTable, new PdfLoadedRadioButtonItemCollection())
	{
		if (Items != null && Items.Count > 0)
		{
			RetrieveOptionValue();
		}
	}

	internal PdfLoadedRadioButtonItem GetItem(string value)
	{
		foreach (PdfLoadedRadioButtonItem item in Items)
		{
			if (item.Value == value || item.OptionValue == value)
			{
				return item;
			}
		}
		return null;
	}

	internal override PdfLoadedStateItem GetItem(int index, PdfDictionary itemDictionary)
	{
		return new PdfLoadedRadioButtonItem(this, index, itemDictionary);
	}

	private int ObtainSelectedIndex()
	{
		int result = -1;
		PdfLoadedRadioButtonItemCollection items = Items;
		int i = 0;
		for (int count = items.Count; i < count; i++)
		{
			PdfLoadedRadioButtonItem pdfLoadedRadioButtonItem = items[i];
			PdfDictionary dictionary = pdfLoadedRadioButtonItem.Dictionary;
			IPdfPrimitive pdfPrimitive = PdfLoadedField.SearchInParents(dictionary, base.CrossTable, "V");
			PdfName pdfName = pdfPrimitive as PdfName;
			PdfString pdfString = null;
			if (pdfName == null)
			{
				pdfString = pdfPrimitive as PdfString;
			}
			if (!dictionary.ContainsKey("AS") || (!(pdfName != null) && pdfString == null))
			{
				continue;
			}
			PdfName pdfName2 = base.CrossTable.GetObject(dictionary["AS"]) as PdfName;
			if (!(pdfName2.Value.ToLower() != "off"))
			{
				continue;
			}
			if (pdfName != null && pdfName.Value.ToLower() != "off")
			{
				if (pdfName2.Value == pdfName.Value || pdfLoadedRadioButtonItem.OptionValue == pdfName.Value)
				{
					result = i;
				}
				break;
			}
			if (pdfString != null && pdfString.Value.ToLower() != "off")
			{
				if (pdfName2.Value == pdfString.Value || pdfLoadedRadioButtonItem.OptionValue == pdfString.Value)
				{
					result = i;
				}
				break;
			}
		}
		return result;
	}

	private void AssignSelectedIndex(int value)
	{
		if (SelectedIndex != value)
		{
			PdfLoadedRadioButtonItem pdfLoadedRadioButtonItem = Items[value];
			UncheckOthers(pdfLoadedRadioButtonItem, PdfLoadedStateField.GetItemValue(pdfLoadedRadioButtonItem.Dictionary, base.CrossTable), check: true);
			pdfLoadedRadioButtonItem.Checked = true;
			string text = PdfName.EncodeName(pdfLoadedRadioButtonItem.Value);
			if (text != null)
			{
				base.Dictionary.SetName("V", text);
				base.Dictionary.SetName("DV", text);
			}
		}
	}

	private void AssignSelectedValue(string value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("SelectedValue");
		}
		PdfLoadedRadioButtonItem item = GetItem(PdfName.DecodeName(value));
		if (item != null)
		{
			UncheckOthers(item, PdfLoadedStateField.GetItemValue(item.Dictionary, base.CrossTable), check: true);
			item.Checked = true;
			if (!isAcrobat)
			{
				string text = PdfName.EncodeName(item.Value);
				if (text != null)
				{
					base.Dictionary.SetName("V", text);
					base.Dictionary.SetName("DV", text);
				}
			}
			else
			{
				base.Dictionary.SetName("V", value);
				base.Dictionary.SetName("DV", value);
			}
		}
		else if (value == "Off")
		{
			UncheckOthers(null, value, check: true);
		}
	}

	internal override void Draw()
	{
		base.Draw();
		PdfArray kids = base.Kids;
		if (kids != null)
		{
			for (int i = 0; i < kids.Count; i++)
			{
				PdfLoadedRadioButtonItem pdfLoadedRadioButtonItem = Items[i];
				PdfCheckFieldState state = (pdfLoadedRadioButtonItem.Checked ? PdfCheckFieldState.Checked : PdfCheckFieldState.Unchecked);
				if (pdfLoadedRadioButtonItem.Page != null)
				{
					DrawStateItem(pdfLoadedRadioButtonItem.Page.Graphics, state, pdfLoadedRadioButtonItem);
				}
			}
		}
		else
		{
			PdfName pdfName = PdfCrossTable.Dereference(base.Dictionary["FT"]) as PdfName;
			if (base.Dictionary.ContainsKey("FT") && pdfName != null && pdfName.Value == "Btn")
			{
				PdfCheckFieldState state2 = ((SelectedIndex == base.DefaultIndex) ? PdfCheckFieldState.Checked : PdfCheckFieldState.Unchecked);
				DrawStateItem(Page.Graphics, state2, null);
			}
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
				PdfLoadedRadioButtonItem item = Items[i];
				ApplyAppearance(widget, item);
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
		PdfLoadedRadioButtonListField pdfLoadedRadioButtonListField = new PdfLoadedRadioButtonListField(dictionary, crossTable);
		pdfLoadedRadioButtonListField.Page = page;
		pdfLoadedRadioButtonListField.SetName(GetFieldName());
		pdfLoadedRadioButtonListField.Widget.Dictionary = base.Widget.Dictionary.Clone(crossTable) as PdfDictionary;
		return pdfLoadedRadioButtonListField;
	}

	internal new PdfLoadedStyledField Clone()
	{
		PdfLoadedRadioButtonListField pdfLoadedRadioButtonListField = MemberwiseClone() as PdfLoadedRadioButtonListField;
		pdfLoadedRadioButtonListField.Dictionary = base.Dictionary.Clone(base.CrossTable) as PdfDictionary;
		pdfLoadedRadioButtonListField.Widget.Dictionary = base.Widget.Dictionary.Clone(base.CrossTable) as PdfDictionary;
		pdfLoadedRadioButtonListField.Items = new PdfLoadedRadioButtonItemCollection();
		for (int i = 0; i < Items.Count; i++)
		{
			PdfLoadedRadioButtonItem item = new PdfLoadedRadioButtonItem(pdfLoadedRadioButtonListField, i, Items[i].Dictionary);
			pdfLoadedRadioButtonListField.Items.Add(item);
		}
		return pdfLoadedRadioButtonListField;
	}

	internal override PdfLoadedFieldItem CreateLoadedItem(PdfDictionary dictionary)
	{
		base.CreateLoadedItem(dictionary);
		PdfLoadedRadioButtonItem pdfLoadedRadioButtonItem = null;
		if (Items != null)
		{
			pdfLoadedRadioButtonItem = new PdfLoadedRadioButtonItem(this, Items.Count, dictionary);
			Items.Add(pdfLoadedRadioButtonItem);
		}
		if (base.Kids == null)
		{
			base.Dictionary["Kids"] = new PdfArray();
		}
		base.Kids.Add(new PdfReferenceHolder(dictionary));
		return pdfLoadedRadioButtonItem;
	}

	private void RetrieveOptionValue()
	{
		if (!base.Dictionary.ContainsKey("Opt"))
		{
			return;
		}
		IPdfPrimitive pdfPrimitive = base.Dictionary["Opt"];
		if (!(((pdfPrimitive is PdfReferenceHolder) ? (pdfPrimitive as PdfReferenceHolder).Object : pdfPrimitive) is PdfArray pdfArray))
		{
			return;
		}
		int num = ((pdfArray.Count <= Items.Count) ? pdfArray.Count : Items.Count);
		for (int i = 0; i < num; i++)
		{
			if (((pdfArray[i] is PdfReferenceHolder) ? (pdfArray[i] as PdfReferenceHolder).Object : pdfArray[i]) is PdfString pdfString)
			{
				Items[i].OptionValue = pdfString.Value;
			}
		}
	}

	public new void RemoveAt(int index)
	{
		if (Items != null && Items.Count != 0)
		{
			PdfLoadedRadioButtonItem item = Items[index];
			Remove(item);
		}
	}

	public void Remove(PdfLoadedRadioButtonItem item)
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
}
