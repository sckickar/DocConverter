using System;
using System.Collections.Generic;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Interactive;
using DocGen.Pdf.IO;
using DocGen.Pdf.Primitives;
using DocGen.Pdf.Security;
using DocGen.Pdf.Xfa;

namespace DocGen.Pdf.Parsing;

public abstract class PdfLoadedStateField : PdfLoadedStyledField
{
	private PdfLoadedStateItemCollection m_items;

	private bool m_bUnchecking;

	public PdfLoadedStateItemCollection Items
	{
		get
		{
			return m_items;
		}
		internal set
		{
			m_items = value;
		}
	}

	internal PdfLoadedStateField(PdfDictionary dictionary, PdfCrossTable crossTable, PdfLoadedStateItemCollection items)
		: base(dictionary, crossTable)
	{
		if (crossTable == null)
		{
			throw new ArgumentNullException("crossTable");
		}
		if (dictionary == null)
		{
			throw new ArgumentNullException("dictionary");
		}
		if (items == null)
		{
			throw new ArgumentNullException("items");
		}
		PdfArray pdfArray = base.Kids;
		m_items = items;
		if (pdfArray != null)
		{
			for (int i = 0; i < pdfArray.Count; i++)
			{
				PdfDictionary itemDictionary = crossTable.GetObject(pdfArray[i]) as PdfDictionary;
				PdfLoadedStateItem item = GetItem(i, itemDictionary);
				m_items.Add(item);
			}
			return;
		}
		bool flag = false;
		if (dictionary.ContainsKey("Parent") && PdfCrossTable.Dereference(dictionary["Parent"]) is PdfDictionary pdfDictionary && pdfDictionary.ContainsKey("Kids") && pdfDictionary["Kids"] is PdfArray && pdfDictionary.ContainsKey("FT") && (pdfDictionary["FT"] as PdfName).Value == "Btn")
		{
			pdfArray = base.CrossTable.GetObject(pdfDictionary["Kids"]) as PdfArray;
			for (int j = 0; j < pdfArray.Count; j++)
			{
				PdfDictionary itemDictionary2 = crossTable.GetObject(pdfArray[j]) as PdfDictionary;
				PdfLoadedStateItem item2 = GetItem(j, itemDictionary2);
				m_items.Add(item2);
				flag = true;
			}
		}
		if (pdfArray != null && flag)
		{
			return;
		}
		PdfLoadedStateItem item3 = GetItem(0, dictionary);
		if (item3 is PdfLoadedRadioButtonItem)
		{
			if ((item3 as PdfLoadedRadioButtonItem).Value != "")
			{
				m_items.Add(item3);
			}
		}
		else
		{
			m_items.Add(item3);
		}
	}

	internal abstract PdfLoadedStateItem GetItem(int index, PdfDictionary itemDictionary);

	private PdfTemplate GetStateTemplate(PdfCheckFieldState state, PdfLoadedStateItem item)
	{
		PdfDictionary pdfDictionary = ((item != null) ? item.Dictionary : base.Dictionary);
		string text = ((state == PdfCheckFieldState.Checked) ? GetItemValue(pdfDictionary, base.CrossTable) : "Off");
		PdfTemplate pdfTemplate = null;
		if (pdfDictionary.ContainsKey("AP"))
		{
			PdfDictionary pdfDictionary2 = PdfCrossTable.Dereference((PdfCrossTable.Dereference(pdfDictionary["AP"]) as PdfDictionary)["N"]) as PdfDictionary;
			if (!string.IsNullOrEmpty(text))
			{
				PdfStream pdfStream = PdfCrossTable.Dereference(pdfDictionary2[text]) as PdfStream;
				if (pdfStream != null && pdfStream.InternalStream != null)
				{
					pdfTemplate = new PdfTemplate(pdfStream);
				}
				if (item == null && pdfTemplate != null && text == "Off" && pdfStream.Encrypt && pdfStream.Decrypted && !pdfStream.IsDecrypted)
				{
					pdfTemplate = null;
				}
			}
		}
		return pdfTemplate;
	}

	protected void SetCheckedStatus(bool value)
	{
		if (value)
		{
			string itemValue = GetItemValue(base.Dictionary, base.CrossTable);
			base.Dictionary.SetName("V", itemValue);
			base.Dictionary.SetProperty("AS", new PdfName(itemValue));
		}
		else
		{
			base.Dictionary.Remove("V");
			base.Dictionary.SetProperty("AS", new PdfName("Off"));
		}
		base.Changed = true;
	}

	internal static string GetItemValue(PdfDictionary dictionary, PdfCrossTable crossTable)
	{
		string text = string.Empty;
		PdfName pdfName = null;
		if (dictionary.ContainsKey("AS"))
		{
			pdfName = crossTable.GetObject(dictionary["AS"]) as PdfName;
			if (pdfName != null && pdfName.Value != "Off")
			{
				text = pdfName.Value;
			}
		}
		if (text == string.Empty && dictionary.ContainsKey("AP"))
		{
			PdfDictionary pdfDictionary = crossTable.GetObject(dictionary["AP"]) as PdfDictionary;
			if (pdfDictionary.ContainsKey("N"))
			{
				PdfReference reference = crossTable.GetReference(pdfDictionary["N"]);
				PdfDictionary obj = crossTable.GetObject(reference) as PdfDictionary;
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
						text = pdfName.Value;
						break;
					}
				}
			}
		}
		return text;
	}

	internal void UncheckOthers(PdfLoadedStateItem child, string value, bool check)
	{
		if (m_bUnchecking)
		{
			return;
		}
		m_bUnchecking = true;
		int num = -1;
		int i = 0;
		for (int count = Items.Count; i < count; i++)
		{
			PdfLoadedStateItem pdfLoadedStateItem = Items[i];
			if (pdfLoadedStateItem != child)
			{
				if (GetItemValue(pdfLoadedStateItem.Dictionary, base.CrossTable) == value && check)
				{
					num = i;
					pdfLoadedStateItem.Checked = true;
				}
				else
				{
					pdfLoadedStateItem.Checked = false;
				}
			}
			else
			{
				if (!pdfLoadedStateItem.Checked && value != null)
				{
					pdfLoadedStateItem.Checked = true;
				}
				num = i;
			}
		}
		if (base.Form != null && base.Form.m_enableXfaFormfill && (base.Form as PdfLoadedForm).IsXFAForm)
		{
			string fieldName = Name.Replace("\\", string.Empty);
			PdfLoadedXfaField xfaField = (base.Form as PdfLoadedForm).GetXfaField(fieldName);
			if (xfaField != null && xfaField is PdfLoadedXfaCheckBoxField)
			{
				(xfaField as PdfLoadedXfaCheckBoxField).IsChecked = true;
			}
			else if (xfaField != null && xfaField is PdfLoadedXfaRadioButtonGroup)
			{
				PdfLoadedXfaRadioButtonGroup pdfLoadedXfaRadioButtonGroup = xfaField as PdfLoadedXfaRadioButtonGroup;
				for (int j = 0; j < pdfLoadedXfaRadioButtonGroup.Fields.Length; j++)
				{
					PdfLoadedXfaRadioButtonField pdfLoadedXfaRadioButtonField = pdfLoadedXfaRadioButtonGroup.Fields[j];
					if (j == num)
					{
						pdfLoadedXfaRadioButtonField.IsChecked = true;
					}
					else
					{
						pdfLoadedXfaRadioButtonField.IsChecked = false;
					}
				}
			}
		}
		m_bUnchecking = false;
	}

	internal void ApplyAppearance(PdfDictionary widget, PdfLoadedStateItem item)
	{
		if (widget != null && widget.ContainsKey("AP"))
		{
			PdfDictionary pdfDictionary = base.CrossTable.GetObject(widget["AP"]) as PdfDictionary;
			if (pdfDictionary != null && pdfDictionary.ContainsKey("N"))
			{
				string empty = string.Empty;
				empty = ((item == null) ? GetItemValue(base.Dictionary, base.CrossTable) : GetItemValue(item.Dictionary, item.CrossTable));
				if ((!base.Flatten || !((PdfField)this).Form.Flatten) && isRotationModified)
				{
					base.Changed = true;
					base.FieldChanged = true;
				}
				RectangleF rectangleF = item?.Bounds ?? base.Bounds;
				if (!(PdfCrossTable.Dereference(pdfDictionary["N"]) is PdfDictionary))
				{
					PdfDictionary pdfDictionary3 = new PdfDictionary();
					PdfTemplate pdfTemplate = new PdfTemplate(rectangleF.Size);
					PdfTemplate pdfTemplate2 = new PdfTemplate(rectangleF.Size);
					DrawStateItem(pdfTemplate.Graphics, PdfCheckFieldState.Checked, item);
					DrawStateItem(pdfTemplate2.Graphics, PdfCheckFieldState.Unchecked, item);
					pdfDictionary3.SetProperty("Off", new PdfReferenceHolder(pdfTemplate2));
					pdfDictionary3.SetProperty(empty, new PdfReferenceHolder(pdfTemplate));
					pdfDictionary["N"] = new PdfReferenceHolder(pdfDictionary3);
				}
				else if (base.FieldChanged)
				{
					PdfDictionary pdfDictionary3 = new PdfDictionary();
					PdfTemplate pdfTemplate3 = new PdfTemplate(rectangleF.Size);
					PdfTemplate pdfTemplate4 = new PdfTemplate(rectangleF.Size);
					DrawStateItem(pdfTemplate3.Graphics, PdfCheckFieldState.Checked, item);
					DrawStateItem(pdfTemplate4.Graphics, PdfCheckFieldState.Unchecked, item);
					pdfDictionary3.SetProperty(empty, new PdfReferenceHolder(pdfTemplate3));
					pdfDictionary3.SetProperty("Off", new PdfReferenceHolder(pdfTemplate4));
					pdfDictionary.Remove("N");
					pdfDictionary["N"] = new PdfReferenceHolder(pdfDictionary3);
				}
				PdfDictionary pdfDictionary4 = PdfCrossTable.Dereference(pdfDictionary["D"]) as PdfDictionary;
				if (pdfDictionary4 == null)
				{
					PdfTemplate pdfTemplate5 = new PdfTemplate(rectangleF.Size);
					PdfTemplate pdfTemplate6 = new PdfTemplate(rectangleF.Size);
					DrawStateItem(pdfTemplate5.Graphics, PdfCheckFieldState.PressedChecked, item);
					DrawStateItem(pdfTemplate6.Graphics, PdfCheckFieldState.PressedUnchecked, item);
					if (pdfDictionary4 != null)
					{
						pdfDictionary4.SetProperty("Off", new PdfReferenceHolder(pdfTemplate6));
						pdfDictionary4.SetProperty(empty, new PdfReferenceHolder(pdfTemplate5));
						pdfDictionary["D"] = new PdfReferenceHolder(pdfDictionary4);
					}
				}
				else if (base.FieldChanged)
				{
					pdfDictionary4 = new PdfDictionary();
					PdfTemplate pdfTemplate7 = new PdfTemplate(rectangleF.Size);
					PdfTemplate pdfTemplate8 = new PdfTemplate(rectangleF.Size);
					DrawStateItem(pdfTemplate7.Graphics, PdfCheckFieldState.PressedChecked, item);
					DrawStateItem(pdfTemplate8.Graphics, PdfCheckFieldState.PressedUnchecked, item);
					if (pdfDictionary4 != null)
					{
						pdfDictionary4.SetProperty("Off", new PdfReferenceHolder(pdfTemplate8));
						pdfDictionary4.SetProperty(empty, new PdfReferenceHolder(pdfTemplate7));
						pdfDictionary.Remove("D");
						pdfDictionary["D"] = new PdfReferenceHolder(pdfDictionary4);
					}
				}
			}
			widget.SetProperty("AP", pdfDictionary);
		}
		else if (((PdfField)this).Form.SetAppearanceDictionary)
		{
			((PdfField)this).Form.NeedAppearances = true;
		}
	}

	internal void DrawStateItem(PdfGraphics graphics, PdfCheckFieldState state, PdfLoadedStateItem item)
	{
		GetGraphicsProperties(out var graphicsProperties, item);
		bool flag = false;
		if ((this is PdfLoadedCheckBoxField || item == null) && graphics != null && graphics.Page != null && graphics.Page.Rotation != 0 && !base.FieldChanged && base.Form.NeedAppearances && graphics.Page.Rotation != PdfPageRotateAngle.RotateAngle180)
		{
			flag = true;
		}
		if (graphics != null && graphics.Page != null)
		{
			graphics.Save();
			if (!flag)
			{
				if (graphics.Page.Rotation == PdfPageRotateAngle.RotateAngle90)
				{
					graphics.TranslateTransform(graphics.Size.Width, graphics.Size.Height);
					graphics.RotateTransform(90f);
				}
				else if (graphics.Page.Rotation == PdfPageRotateAngle.RotateAngle180)
				{
					graphics.TranslateTransform(graphics.Size.Width, graphics.Size.Height);
					graphics.RotateTransform(-180f);
				}
				else if (graphics.Page.Rotation == PdfPageRotateAngle.RotateAngle270)
				{
					graphics.TranslateTransform(graphics.Size.Width, graphics.Size.Height);
					graphics.RotateTransform(270f);
				}
			}
		}
		if (!base.Flatten)
		{
			graphicsProperties.Rect.Location = PointF.Empty;
		}
		RectangleF rectangleF = item?.Bounds ?? base.Bounds;
		PaintParams paintParams = new PaintParams(graphicsProperties.Rect, graphicsProperties.BackBrush, graphicsProperties.ForeBrush, graphicsProperties.Pen, graphicsProperties.Style, graphicsProperties.BorderWidth, graphicsProperties.ShadowBrush, graphicsProperties.RotationAngle);
		if (base.FieldChanged || (base.Flatten && !base.Form.NeedAppearances && graphicsProperties.RotationAngle == 0 && !base.Form.Dictionary.ContainsKey("NeedAppearances")))
		{
			if (graphicsProperties.Font.Size >= 0f)
			{
				graphicsProperties.Font = null;
			}
			if (this is PdfLoadedCheckBoxField)
			{
				FieldPainter.DrawCheckBox(graphics, paintParams, StyleToString(base.Style), state, graphicsProperties.Font);
			}
			else if (this is PdfLoadedRadioButtonListField)
			{
				FieldPainter.DrawRadioButton(graphics, paintParams, StyleToString(base.Style), state);
			}
		}
		else
		{
			graphics.StreamWriter.SetTextRenderingMode(TextRenderingMode.Fill);
			PdfTemplate stateTemplate = GetStateTemplate(state, item);
			if (stateTemplate != null)
			{
				if (flag && item == null)
				{
					rectangleF = RotateField(base.Bounds, Page.Size, Page.Rotation);
				}
				else if (flag && item != null)
				{
					rectangleF = RotateField(item.Bounds, Page.Size, Page.Rotation);
				}
				PdfLoadedDocument pdfLoadedDocument = null;
				bool flag2 = false;
				if (base.CrossTable != null && base.CrossTable.Document != null && base.CrossTable.Document is PdfLoadedDocument { Security: not null, IsEncrypted: not false } pdfLoadedDocument2 && pdfLoadedDocument2.Security.Enabled && pdfLoadedDocument2.Security.EncryptionOptions == PdfEncryptionOptions.EncryptAllContents)
				{
					flag2 = true;
				}
				PdfStream content = stateTemplate.m_content;
				if (content != null && flag2 && content.Encrypt && !content.IsDecrypted && this is PdfLoadedCheckBoxField)
				{
					graphicsProperties.Font = null;
					FieldPainter.DrawCheckBox(graphics, paintParams, StyleToString(base.Style), state, graphicsProperties.Font);
				}
				else
				{
					graphics.DrawPdfTemplate(stateTemplate, rectangleF.Location, rectangleF.Size);
				}
			}
		}
		graphics.Restore();
	}

	public void RemoveAt(int index)
	{
		if (Items != null && Items.Count != 0)
		{
			PdfLoadedStateItem item = Items[index];
			Remove(item);
		}
	}

	public void Remove(PdfLoadedStateItem item)
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

	internal RectangleF RotateField(RectangleF rect, SizeF size, PdfPageRotateAngle angle)
	{
		RectangleF result = default(RectangleF);
		if (angle == PdfPageRotateAngle.RotateAngle180)
		{
			float x = size.Width - (rect.X + rect.Width);
			float y = size.Height - (rect.Y + rect.Height);
			result = new RectangleF(x, y, rect.Width, rect.Height);
		}
		if (angle == PdfPageRotateAngle.RotateAngle270)
		{
			float y = size.Width - (rect.X + rect.Width);
			float x = rect.Y;
			result = new RectangleF(x, y, rect.Height, rect.Width);
		}
		if (angle == PdfPageRotateAngle.RotateAngle90)
		{
			float x = size.Height - (rect.Y + rect.Height);
			float y = rect.X;
			result = new RectangleF(x, y, rect.Height, rect.Width);
		}
		return result;
	}
}
