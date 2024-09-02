using System;
using System.Collections.Generic;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Interactive;
using DocGen.Pdf.IO;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Parsing;

public class PdfLoadedComboBoxField : PdfLoadedChoiceField
{
	private PdfLoadedComboBoxItemCollection m_items;

	public new bool ComplexScript
	{
		get
		{
			return base.ComplexScript;
		}
		set
		{
			base.ComplexScript = value;
			NotifyPropertyChanged("ComplexScript");
		}
	}

	internal bool IsAutoFontSize
	{
		get
		{
			bool result = false;
			if (base.CrossTable.Document is PdfLoadedDocument pdfLoadedDocument)
			{
				PdfLoadedForm form = pdfLoadedDocument.Form;
				if (form != null && form.Dictionary.ContainsKey("DA"))
				{
					PdfString pdfString = form.Dictionary.Items[new PdfName("DA")] as PdfString;
					float height = 0f;
					if (pdfString != null)
					{
						FontName(pdfString.Value, out height);
						if (height == 0f)
						{
							if (base.Dictionary.ContainsKey("Kids"))
							{
								bool flag = false;
								PdfArray pdfArray = base.Dictionary["Kids"] as PdfArray;
								PdfDictionary pdfDictionary = null;
								if (base.Dictionary.ContainsKey("DA"))
								{
									PdfString pdfString2 = base.Dictionary.Items[new PdfName("DA")] as PdfString;
									height = 0f;
									if (pdfString2 != null)
									{
										FontName(pdfString2.Value, out height);
									}
									if (height == 0f)
									{
										flag = true;
									}
								}
								if (pdfArray != null && (flag || !base.Dictionary.ContainsKey("DA")))
								{
									foreach (PdfReferenceHolder element in pdfArray.Elements)
									{
										if (element != null)
										{
											pdfDictionary = element.Object as PdfDictionary;
										}
										if (pdfDictionary != null && !pdfDictionary.ContainsKey("DA"))
										{
											result = true;
											continue;
										}
										PdfString pdfString3 = pdfDictionary.Items[new PdfName("DA")] as PdfString;
										height = 0f;
										if (pdfString3 != null)
										{
											FontName(pdfString3.Value, out height);
										}
										if (height == 0f)
										{
											result = true;
										}
									}
								}
							}
							else if (!base.Dictionary.ContainsKey("DA"))
							{
								result = true;
							}
							else
							{
								PdfString pdfString4 = base.Dictionary.Items[new PdfName("DA")] as PdfString;
								height = 0f;
								if (pdfString4 != null)
								{
									FontName(pdfString4.Value, out height);
								}
								if (height == 0f)
								{
									result = true;
								}
							}
						}
					}
				}
			}
			return result;
		}
	}

	public bool Editable
	{
		get
		{
			return (FieldFlags.Edit & Flags) != 0;
		}
		set
		{
			if (value)
			{
				Flags |= FieldFlags.Edit;
			}
			else
			{
				Flags -= 262144;
			}
			NotifyPropertyChanged("Editable");
		}
	}

	public PdfLoadedComboBoxItemCollection Items
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

	public new int SelectedIndex
	{
		get
		{
			return ObtainSelectedIndex()[0];
		}
		set
		{
			AssignSelectedIndex(new int[1] { value });
			NotifyPropertyChanged("SelectedIndex");
		}
	}

	public new string SelectedValue
	{
		get
		{
			string[] array = ObtainSelectedValue();
			if (array.Length != 0)
			{
				return array[0];
			}
			return null;
		}
		set
		{
			AssignSelectedValue(new string[1] { value });
			NotifyPropertyChanged("SelectedValue");
		}
	}

	internal PdfLoadedComboBoxField(PdfDictionary dictionary, PdfCrossTable crossTable)
		: base(dictionary, crossTable)
	{
		PdfArray kids = base.Kids;
		m_items = new PdfLoadedComboBoxItemCollection();
		if (kids != null)
		{
			for (int i = 0; i < kids.Count; i++)
			{
				PdfDictionary dictionary2 = crossTable.GetObject(kids[i]) as PdfDictionary;
				PdfLoadedComboBoxItem item = new PdfLoadedComboBoxItem(this, i, dictionary2);
				m_items.Add(item);
			}
		}
	}

	internal override void Draw()
	{
		base.Draw();
		RectangleF bounds = base.Bounds;
		bounds.Location = PointF.Empty;
		string text = string.Empty;
		if (SelectedIndex != -1)
		{
			text = base.SelectedItem[0].Text;
		}
		else if (SelectedIndex == -1 && base.Dictionary.ContainsKey("V"))
		{
			if (PdfCrossTable.Dereference(base.Dictionary["V"]) is PdfString pdfString)
			{
				text = pdfString.Value;
			}
		}
		else if (base.Dictionary.ContainsKey("DV"))
		{
			text = ((!(base.Dictionary["DV"] is PdfString)) ? (PdfCrossTable.Dereference(base.Dictionary["DV"]) as PdfString).Value : (base.Dictionary["DV"] as PdfString).Value);
		}
		PdfTemplate pdfTemplate = new PdfTemplate(bounds.Size);
		PdfArray kids = base.Kids;
		if (kids != null && kids.Count > 1)
		{
			for (int i = 0; i < kids.Count; i++)
			{
				if (!CheckFieldFlagValue(kids[i]))
				{
					PdfLoadedFieldItem pdfLoadedFieldItem = Items[i];
					pdfTemplate = new PdfTemplate(pdfLoadedFieldItem.Size);
					bounds = pdfLoadedFieldItem.Bounds;
					bounds.Location = PointF.Empty;
					DrawComboBox(pdfTemplate.Graphics, pdfLoadedFieldItem);
					pdfTemplate.Graphics.DrawString(text, pdfLoadedFieldItem.Font, pdfLoadedFieldItem.ForeBrush, bounds, pdfLoadedFieldItem.StringFormat);
					if (pdfLoadedFieldItem.Page != null && pdfLoadedFieldItem.Page.Graphics != null)
					{
						pdfLoadedFieldItem.Page.Graphics.DrawPdfTemplate(pdfTemplate, pdfLoadedFieldItem.Bounds.Location);
					}
				}
			}
		}
		else
		{
			DrawComboBox(Page.Graphics, null, text);
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
		PdfLoadedComboBoxField pdfLoadedComboBoxField = new PdfLoadedComboBoxField(dictionary, crossTable);
		pdfLoadedComboBoxField.Page = page;
		pdfLoadedComboBoxField.SetName(GetFieldName());
		pdfLoadedComboBoxField.Widget.Dictionary = base.Widget.Dictionary.Clone(crossTable) as PdfDictionary;
		return pdfLoadedComboBoxField;
	}

	internal new PdfLoadedStyledField Clone()
	{
		PdfLoadedComboBoxField pdfLoadedComboBoxField = MemberwiseClone() as PdfLoadedComboBoxField;
		pdfLoadedComboBoxField.Dictionary = base.Dictionary.Clone(base.CrossTable) as PdfDictionary;
		pdfLoadedComboBoxField.Widget.Dictionary = base.Widget.Dictionary.Clone(base.CrossTable) as PdfDictionary;
		pdfLoadedComboBoxField.Items = new PdfLoadedComboBoxItemCollection();
		for (int i = 0; i < Items.Count; i++)
		{
			PdfLoadedComboBoxItem item = new PdfLoadedComboBoxItem(pdfLoadedComboBoxField, i, Items[i].Dictionary);
			pdfLoadedComboBoxField.Items.Add(item);
		}
		return pdfLoadedComboBoxField;
	}

	internal override PdfLoadedFieldItem CreateLoadedItem(PdfDictionary dictionary)
	{
		base.CreateLoadedItem(dictionary);
		PdfLoadedComboBoxItem pdfLoadedComboBoxItem = new PdfLoadedComboBoxItem(this, m_items.Count, dictionary);
		m_items.Add(pdfLoadedComboBoxItem);
		if (base.Kids == null)
		{
			base.Dictionary["Kids"] = new PdfArray();
		}
		base.Kids.Add(new PdfReferenceHolder(dictionary));
		return pdfLoadedComboBoxItem;
	}

	private void ApplyAppearance(PdfDictionary widget, PdfLoadedFieldItem item)
	{
		if (widget != null && widget.ContainsKey("AP") && !base.Form.NeedAppearances)
		{
			if (base.CrossTable.GetObject(widget["AP"]) is PdfDictionary pdfDictionary && pdfDictionary.ContainsKey("N"))
			{
				if (item != null)
				{
					_ = item.Bounds;
				}
				else
				{
					_ = base.Bounds;
				}
				PdfTemplate pdfTemplate = new PdfTemplate(base.Bounds.Size);
				DrawComboBox(pdfTemplate.Graphics, item);
				pdfDictionary.Remove("N");
				pdfDictionary.SetProperty("N", new PdfReferenceHolder(pdfTemplate));
				widget.SetProperty("AP", pdfDictionary);
			}
		}
		else if (((PdfField)this).Form.ReadOnly || ReadOnly)
		{
			((PdfField)this).Form.SetAppearanceDictionary = true;
		}
		else if (((PdfField)this).Form.SetAppearanceDictionary)
		{
			((PdfField)this).Form.NeedAppearances = true;
		}
	}

	private void DrawComboBox(PdfGraphics graphics, PdfLoadedFieldItem item)
	{
		GetGraphicsProperties(out var graphicsProperties, item);
		graphicsProperties.Rect.Location = PointF.Empty;
		PaintParams paintParams = new PaintParams(graphicsProperties.Rect, graphicsProperties.BackBrush, graphicsProperties.ForeBrush, graphicsProperties.Pen, graphicsProperties.Style, graphicsProperties.BorderWidth, graphicsProperties.ShadowBrush, graphicsProperties.RotationAngle);
		string text = null;
		if (base.SelectedItem.Count > 0)
		{
			_ = SelectedIndex;
			if (SelectedIndex != -1 && !base.Flatten)
			{
				text = base.SelectedItem[0].Text;
				goto IL_00cb;
			}
		}
		if (base.Dictionary.ContainsKey("DV") && !base.Flatten && PdfCrossTable.Dereference(base.Dictionary["DV"]) is PdfString pdfString)
		{
			text = pdfString.Value;
		}
		goto IL_00cb;
		IL_00cb:
		if (base.SelectedItem.Count == 0)
		{
			string selectedValue = SelectedValue;
			FieldPainter.DrawComboBox(graphics, paintParams, (selectedValue != null) ? selectedValue : "", graphicsProperties.Font, graphicsProperties.StringFormat);
		}
		else if (text != null && !base.Flatten)
		{
			FieldPainter.DrawComboBox(graphics, paintParams, (text != null) ? text : "", graphicsProperties.Font, graphicsProperties.StringFormat);
		}
		else
		{
			FieldPainter.DrawComboBox(graphics, paintParams);
		}
	}

	private void DrawComboBox(PdfGraphics graphics, PdfLoadedFieldItem item, string text)
	{
		GetGraphicsProperties(out var graphicsProperties, item);
		PaintParams paintParams = new PaintParams(graphicsProperties.Rect, graphicsProperties.BackBrush, graphicsProperties.ForeBrush, graphicsProperties.Pen, graphicsProperties.Style, graphicsProperties.BorderWidth, graphicsProperties.ShadowBrush, graphicsProperties.RotationAngle);
		if (graphicsProperties.Font.Height > base.Bounds.Height)
		{
			SetFittingFontSize(ref graphicsProperties, paintParams, text);
		}
		FieldPainter.DrawComboBox(graphics, paintParams, text, graphicsProperties.Font, graphicsProperties.StringFormat);
	}

	private void SetFittingFontSize(ref GraphicsProperties gp, PaintParams prms, string text)
	{
		float num = 0f;
		float num2 = 0f;
		num2 = ((prms.BorderStyle != PdfBorderStyle.Beveled && prms.BorderStyle != PdfBorderStyle.Inset) ? (gp.Rect.Width - 4f * prms.BorderWidth) : (gp.Rect.Width - 8f * prms.BorderWidth));
		float num3 = gp.Rect.Height - 2f * gp.BorderWidth;
		float num4 = 0.248f;
		_ = gp.Font is PdfStandardFont;
		if (text.EndsWith(" "))
		{
			gp.StringFormat.MeasureTrailingSpaces = true;
		}
		for (float num5 = 0f; num5 <= gp.Rect.Height; num5 += 1f)
		{
			if (gp.Font is PdfStandardFont)
			{
				gp.Font.Size = num5;
			}
			else
			{
				gp.Font.Size = num5;
			}
			SizeF sizeF = gp.Font.MeasureString(text, gp.StringFormat);
			if (!(sizeF.Width > gp.Rect.Width) && !(sizeF.Height > num3))
			{
				continue;
			}
			num = num5;
			do
			{
				num -= 0.001f;
				gp.Font.Size = num;
				float lineWidth = gp.Font.GetLineWidth(text, gp.StringFormat);
				if (num < num4)
				{
					gp.Font.Size = num4;
					break;
				}
				sizeF = gp.Font.MeasureString(text, gp.StringFormat);
				if (lineWidth < num2 && sizeF.Height < num3)
				{
					gp.Font.Size = num;
					break;
				}
			}
			while (num > num4);
			break;
		}
	}

	internal override float GetFontHeight(PdfFontFamily family)
	{
		float num = 0f;
		List<float> list = new List<float>();
		foreach (PdfLoadedListItem item in base.SelectedItem)
		{
			PdfFont pdfFont = new PdfStandardFont(family, 12f);
			list.Add(pdfFont.MeasureString(item.Text).Width);
		}
		if (base.SelectedItem.Count == 0 && base.Values.Count != 0)
		{
			PdfFont pdfFont2 = new PdfStandardFont(family, 12f);
			float num2 = pdfFont2.MeasureString(base.Values[0].Text).Width;
			int i = 1;
			for (int count = base.Values.Count; i < count; i++)
			{
				float width = pdfFont2.MeasureString(base.Values[i].Text).Width;
				num2 = ((num2 > width) ? num2 : width);
				list.Add(num2);
			}
		}
		list.Sort();
		float num3 = 0f;
		num3 = ((list.Count <= 0) ? 12f : (12f * (base.Bounds.Size.Width - 4f * base.BorderWidth) / list[list.Count - 1]));
		if (base.SelectedItem.Count != 0)
		{
			PdfFont pdfFont3 = new PdfStandardFont(family, num3);
			string selectedValue = SelectedValue;
			SizeF sizeF = pdfFont3.MeasureString(selectedValue);
			if (sizeF.Width > base.Bounds.Width || sizeF.Height > base.Bounds.Height)
			{
				float num4 = base.Bounds.Width - 4f * base.BorderWidth;
				float num5 = base.Bounds.Height - 4f * base.BorderWidth;
				float num6 = 0.248f;
				for (float num7 = 1f; num7 <= base.Bounds.Height; num7 += 1f)
				{
					pdfFont3.Size = num7;
					SizeF sizeF2 = pdfFont3.MeasureString(selectedValue);
					if (!(sizeF2.Width > base.Bounds.Width) && !(sizeF2.Height > num5))
					{
						continue;
					}
					num = num7;
					do
					{
						num = (pdfFont3.Size = num - 0.001f);
						float lineWidth = pdfFont3.GetLineWidth(selectedValue, base.StringFormat);
						if (num < num6)
						{
							pdfFont3.Size = num6;
							break;
						}
						sizeF2 = pdfFont3.MeasureString(selectedValue, base.StringFormat);
						if (lineWidth < num4 && sizeF2.Height < num5)
						{
							pdfFont3.Size = num;
							break;
						}
					}
					while (num > num6);
					num3 = num;
					break;
				}
			}
		}
		else if (num3 > 12f)
		{
			num3 = 12f;
		}
		return num3;
	}

	public void RemoveAt(int index)
	{
		if (Items != null && Items.Count != 0)
		{
			PdfLoadedComboBoxItem item = Items[index];
			Remove(item);
		}
	}

	public void Remove(PdfLoadedComboBoxItem item)
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
