using System;
using System.Collections.Generic;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.IO;
using DocGen.Pdf.Primitives;
using DocGen.Pdf.Xfa;

namespace DocGen.Pdf.Parsing;

public class PdfLoadedChoiceField : PdfLoadedStyledField
{
	public PdfLoadedListItemCollection Values => GetListItemCollection();

	public int[] SelectedIndex
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

	public string[] SelectedValue
	{
		get
		{
			return ObtainSelectedValue();
		}
		set
		{
			AssignSelectedValue(value);
			NotifyPropertyChanged("SelectedIndex");
		}
	}

	public PdfLoadedListItemCollection SelectedItem
	{
		get
		{
			PdfLoadedListItemCollection pdfLoadedListItemCollection = new PdfLoadedListItemCollection(this);
			int[] selectedIndex = SelectedIndex;
			foreach (int num in selectedIndex)
			{
				if (num > -1 && Values.Count > 0 && Values.Count > num)
				{
					PdfLoadedListItem item = Values[num];
					pdfLoadedListItemCollection.AddItem(item);
				}
			}
			return pdfLoadedListItemCollection;
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
		}
	}

	internal PdfLoadedChoiceField(PdfDictionary dictionary, PdfCrossTable crossTable)
		: base(dictionary, crossTable)
	{
	}

	protected int[] ObtainSelectedIndex()
	{
		List<int> list = new List<int>();
		if (base.Dictionary.ContainsKey("I"))
		{
			if (base.CrossTable.GetObject(base.Dictionary["I"]) is PdfArray pdfArray)
			{
				if (pdfArray.Count > 0)
				{
					for (int i = 0; i < pdfArray.Count; i++)
					{
						PdfNumber pdfNumber = base.CrossTable.GetObject(pdfArray[i]) as PdfNumber;
						list.Add(pdfNumber.IntValue);
					}
				}
			}
			else if (base.CrossTable.GetObject(base.Dictionary["I"]) is PdfNumber pdfNumber2)
			{
				list.Add(pdfNumber2.IntValue);
			}
		}
		if (list.Count == 0)
		{
			list.Add(-1);
		}
		return list.ToArray();
	}

	protected void AssignSelectedIndex(int[] value)
	{
		if (value.Length == 0 || value.Length > Values.Count)
		{
			throw new ArgumentOutOfRangeException("SelectedIndex");
		}
		int[] array = value;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] >= Values.Count)
			{
				throw new ArgumentOutOfRangeException("SelectedIndex");
			}
		}
		if (ReadOnly)
		{
			return;
		}
		Array.Sort(value);
		base.Dictionary.SetProperty("I", new PdfArray(value));
		List<string> list = new List<string>();
		array = value;
		foreach (int num in array)
		{
			if (num >= 0)
			{
				list.Add(Values[num].Value);
			}
		}
		if (list.Count > 0)
		{
			AssignSelectedValue(list.ToArray());
		}
		base.Changed = true;
	}

	protected string[] ObtainSelectedValue()
	{
		List<string> list = new List<string>();
		if (base.Dictionary.ContainsKey("V"))
		{
			IPdfPrimitive @object = base.CrossTable.GetObject(base.Dictionary["V"]);
			if (@object is PdfString)
			{
				list.Add((@object as PdfString).Value);
			}
			else
			{
				PdfArray pdfArray = @object as PdfArray;
				for (int i = 0; i < pdfArray.Count; i++)
				{
					PdfString pdfString = pdfArray[i] as PdfString;
					list.Add(pdfString.Value);
				}
			}
		}
		else
		{
			int[] selectedIndex = SelectedIndex;
			foreach (int num in selectedIndex)
			{
				if (num > -1)
				{
					list.Add(Values[num].Value);
				}
			}
		}
		return list.ToArray();
	}

	protected void AssignSelectedValue(string[] values)
	{
		List<int> list = new List<int>();
		PdfLoadedListItemCollection values2 = Values;
		if (!ReadOnly)
		{
			string[] array = values;
			foreach (string text in array)
			{
				bool flag = false;
				for (int j = 0; j < values2.Count; j++)
				{
					if (values2[j].Value == text || values2[j].Text == text)
					{
						flag = true;
						list.Add(j);
					}
				}
				if (!flag && this is PdfLoadedComboBoxField && !(this as PdfLoadedComboBoxField).Editable)
				{
					throw new ArgumentOutOfRangeException("index");
				}
			}
			if (values.Length > 1 && !(this as PdfLoadedListBoxField).MultiSelect)
			{
				list.RemoveRange(1, list.Count - 1);
				values = new string[1] { values2[list[0]].Value };
			}
			if (list.Count != 0)
			{
				list.Sort();
				base.Dictionary.SetProperty("I", new PdfArray(list.ToArray()));
			}
			else
			{
				base.Dictionary.Remove("I");
			}
		}
		if (base.Dictionary.ContainsKey("V"))
		{
			IPdfPrimitive @object = base.CrossTable.GetObject(base.Dictionary["V"]);
			if (@object == null || @object is PdfString)
			{
				if (this is PdfLoadedListBoxField)
				{
					PdfArray pdfArray = new PdfArray();
					string[] array = values;
					foreach (string value in array)
					{
						pdfArray.Add(new PdfString(value));
					}
					base.Dictionary.SetProperty("V", pdfArray);
				}
				else
				{
					string value2 = values[0].ToString();
					base.Dictionary["V"] = new PdfString(value2);
				}
			}
			else
			{
				PdfArray pdfArray2 = @object as PdfArray;
				pdfArray2.Clear();
				string[] array = values;
				foreach (string value3 in array)
				{
					pdfArray2.Add(new PdfString(value3));
				}
				base.Dictionary.SetProperty("V", pdfArray2);
			}
		}
		else if (this is PdfLoadedComboBoxField)
		{
			base.Dictionary.SetString("V", values[0]);
		}
		else
		{
			PdfArray pdfArray3 = new PdfArray();
			string[] array = values;
			foreach (string value4 in array)
			{
				pdfArray3.Add(new PdfString(value4));
			}
			base.Dictionary.SetProperty("V", pdfArray3);
		}
		base.Changed = true;
		if (!base.Form.m_enableXfaFormfill || !(base.Form as PdfLoadedForm).IsXFAForm || values.Length == 0)
		{
			return;
		}
		string fieldName = Name.Replace("\\", string.Empty);
		PdfLoadedXfaField xfaField = (base.Form as PdfLoadedForm).GetXfaField(fieldName);
		if (xfaField == null)
		{
			return;
		}
		if (xfaField is PdfLoadedXfaListBoxField)
		{
			if (xfaField is PdfLoadedXfaListBoxField pdfLoadedXfaListBoxField && values2.Count >= values.Length)
			{
				pdfLoadedXfaListBoxField.SelectedItems = values;
			}
		}
		else if (xfaField is PdfLoadedXfaComboBoxField)
		{
			PdfLoadedXfaComboBoxField pdfLoadedXfaComboBoxField = xfaField as PdfLoadedXfaComboBoxField;
			if (pdfLoadedXfaComboBoxField != null && pdfLoadedXfaComboBoxField.Items != null && pdfLoadedXfaComboBoxField.Items.Contains(values[0]))
			{
				pdfLoadedXfaComboBoxField.SelectedValue = values[0];
			}
			else if (pdfLoadedXfaComboBoxField != null && pdfLoadedXfaComboBoxField.HiddenItems != null && pdfLoadedXfaComboBoxField.HiddenItems.Contains(values[0]))
			{
				pdfLoadedXfaComboBoxField.SelectedValue = values[0];
			}
		}
	}

	internal PdfLoadedListItemCollection GetListItemCollection()
	{
		PdfLoadedListItemCollection pdfLoadedListItemCollection = new PdfLoadedListItemCollection(this);
		PdfArray pdfArray = PdfLoadedField.GetValue(base.Dictionary, base.CrossTable, "Opt", inheritable: true) as PdfArray;
		if (pdfArray == null && base.Dictionary.ContainsKey("Kids"))
		{
			PdfArray pdfArray2 = PdfCrossTable.Dereference(base.Dictionary["Kids"]) as PdfArray;
			for (int i = 0; i < pdfArray2.Count; i++)
			{
				pdfArray = PdfLoadedField.GetValue(PdfCrossTable.Dereference(pdfArray2[i]) as PdfDictionary, base.CrossTable, "Opt", inheritable: true) as PdfArray;
				if (pdfArray != null)
				{
					break;
				}
			}
		}
		if (pdfArray != null)
		{
			int j = 0;
			for (int count = pdfArray.Count; j < count; j++)
			{
				IPdfPrimitive @object = base.CrossTable.GetObject(pdfArray[j]);
				PdfLoadedListItem pdfLoadedListItem = null;
				if (@object is PdfString)
				{
					pdfLoadedListItem = new PdfLoadedListItem((@object as PdfString).Value, null, this, base.CrossTable);
				}
				else
				{
					PdfArray pdfArray3 = @object as PdfArray;
					PdfString pdfString = base.CrossTable.GetObject(pdfArray3[0]) as PdfString;
					pdfLoadedListItem = new PdfLoadedListItem((base.CrossTable.GetObject(pdfArray3[1]) as PdfString).Value, pdfString.Value, this, base.CrossTable);
				}
				pdfLoadedListItemCollection.AddItem(pdfLoadedListItem);
			}
		}
		return pdfLoadedListItemCollection;
	}
}
