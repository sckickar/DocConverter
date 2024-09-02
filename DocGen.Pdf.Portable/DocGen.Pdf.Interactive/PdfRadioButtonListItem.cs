using System;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public class PdfRadioButtonListItem : PdfCheckFieldBase, IPdfWrapper
{
	internal PdfRadioButtonListField m_field;

	private string m_value = string.Empty;

	public override PdfForm Form
	{
		get
		{
			if (m_field != null)
			{
				return m_field.Form;
			}
			return null;
		}
	}

	public override RectangleF Bounds
	{
		get
		{
			return base.Bounds;
		}
		set
		{
			base.Bounds = value;
		}
	}

	public string Value
	{
		get
		{
			return m_value;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("Value");
			}
			if (value.Length == 0)
			{
				throw new ArgumentException("Value can't be an empty string.");
			}
			m_value = value;
			NotifyPropertyChanged("Value");
		}
	}

	IPdfPrimitive IPdfWrapper.Element => ((IPdfWrapper)base.Widget).Element;

	public PdfRadioButtonListItem()
	{
	}

	public PdfRadioButtonListItem(string value)
	{
		Value = value;
	}

	protected override void Initialize()
	{
		base.Initialize();
		base.Widget.BeginSave += Widget_Save;
		base.Style = PdfCheckBoxStyle.Circle;
	}

	internal void SetField(PdfRadioButtonListField field)
	{
		base.Widget.Parent = field;
		PdfPage pdfPage = ((field != null) ? (field.Page as PdfPage) : (m_field.Page as PdfPage));
		if (pdfPage != null)
		{
			if (field == null)
			{
				int index = pdfPage.Annotations.IndexOf(base.Widget);
				pdfPage.Annotations.RemoveAt(index);
			}
			else
			{
				pdfPage.Annotations.Add(base.Widget);
			}
		}
		else
		{
			PdfLoadedPage pdfLoadedPage = ((field != null) ? (field.Page as PdfLoadedPage) : (m_field.Page as PdfLoadedPage));
			if (pdfLoadedPage != null)
			{
				PdfDictionary dictionary = pdfLoadedPage.Dictionary;
				PdfArray pdfArray = null;
				pdfArray = ((!dictionary.ContainsKey("Annots")) ? new PdfArray() : (pdfLoadedPage.CrossTable.GetObject(dictionary["Annots"]) as PdfArray));
				PdfReferenceHolder element = new PdfReferenceHolder(base.Widget);
				if (field == null)
				{
					int num = pdfArray.IndexOf(element);
					if (num >= 0)
					{
						pdfArray.RemoveAt(num);
					}
				}
				else
				{
					pdfArray.Add(element);
					if (!field.Page.Annotations.Contains(base.Widget))
					{
						field.Page.Annotations.Add(base.Widget);
					}
				}
				if (field != null)
				{
					field.Page.Dictionary.SetProperty("Annots", pdfArray);
				}
				else
				{
					m_field.Page.Dictionary.SetProperty("Annots", pdfArray);
				}
			}
		}
		m_field = field;
	}

	private void Widget_Save(object sender, EventArgs e)
	{
		Save();
	}

	internal override void Save()
	{
		base.Save();
		if (Form != null || isXfa)
		{
			string onMappingName = ObtainValue();
			base.Widget.ExtendedAppearance.Normal.OnMappingName = onMappingName;
			base.Widget.ExtendedAppearance.Pressed.OnMappingName = onMappingName;
			if (m_field.SelectedItem == this)
			{
				base.Widget.AppearanceState = ObtainValue();
			}
			else
			{
				base.Widget.AppearanceState = "Off";
			}
		}
	}

	protected override void DrawAppearance()
	{
		base.DrawAppearance();
		PaintParams paintParams = new PaintParams(new RectangleF(PointF.Empty, base.Size), base.BackBrush, base.ForeBrush, base.BorderPen, base.BorderStyle, base.BorderWidth, base.ShadowBrush, base.RotationAngle);
		FieldPainter.DrawRadioButton(base.Widget.ExtendedAppearance.Normal.On.Graphics, paintParams, StyleToString(base.Style), PdfCheckFieldState.Checked);
		FieldPainter.DrawRadioButton(base.Widget.ExtendedAppearance.Normal.Off.Graphics, paintParams, StyleToString(base.Style), PdfCheckFieldState.Unchecked);
		FieldPainter.DrawRadioButton(base.Widget.ExtendedAppearance.Pressed.On.Graphics, paintParams, StyleToString(base.Style), PdfCheckFieldState.PressedChecked);
		FieldPainter.DrawRadioButton(base.Widget.ExtendedAppearance.Pressed.Off.Graphics, paintParams, StyleToString(base.Style), PdfCheckFieldState.PressedUnchecked);
	}

	private string ObtainValue()
	{
		if (m_value == string.Empty)
		{
			return m_field.Items.IndexOf(this).ToString();
		}
		return m_value;
	}

	internal override void Draw()
	{
		RemoveAnnoationFromPage(m_field.Page, base.Widget);
		PaintParams paintParams = new PaintParams(Bounds, base.BackBrush, base.ForeBrush, base.BorderPen, base.BorderStyle, base.BorderWidth, base.ShadowBrush, base.RotationAngle);
		PdfCheckFieldState state = PdfCheckFieldState.Unchecked;
		if (m_field.SelectedIndex >= 0 && m_field.SelectedValue == Value)
		{
			state = PdfCheckFieldState.Checked;
		}
		FieldPainter.DrawRadioButton(m_field.Page.Graphics, paintParams, StyleToString(base.Style), state);
	}
}
