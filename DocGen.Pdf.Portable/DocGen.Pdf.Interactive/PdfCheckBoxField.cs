using DocGen.Drawing;
using DocGen.Pdf.Graphics;

namespace DocGen.Pdf.Interactive;

public class PdfCheckBoxField : PdfCheckFieldBase
{
	private bool m_checked;

	public bool Checked
	{
		get
		{
			return m_checked;
		}
		set
		{
			if (m_checked != value)
			{
				m_checked = value;
				if (m_checked)
				{
					base.Dictionary.SetName("V", "Yes");
				}
				else
				{
					base.Dictionary.Remove("V");
				}
				NotifyPropertyChanged("Checked");
			}
		}
	}

	public PdfCheckBoxField(PdfPageBase page, string name)
		: base(page, name)
	{
	}

	internal override void Save()
	{
		base.Save();
		if (Form != null || isXfa)
		{
			if (!Checked)
			{
				base.Widget.AppearanceState = "Off";
			}
			else
			{
				base.Widget.AppearanceState = "Yes";
			}
		}
	}

	internal override void Draw()
	{
		base.Draw();
		PaintParams paintParams = new PaintParams(Bounds, base.BackBrush, base.ForeBrush, base.BorderPen, base.BorderStyle, base.BorderWidth, base.ShadowBrush, base.RotationAngle);
		PdfCheckFieldState state = PdfCheckFieldState.Checked;
		if (!Checked)
		{
			state = PdfCheckFieldState.Unchecked;
		}
		FieldPainter.DrawCheckBox(Page.Graphics, paintParams, StyleToString(base.Style), state);
	}

	protected override void DrawAppearance()
	{
		base.DrawAppearance();
		PaintParams paintParams = new PaintParams(new RectangleF(PointF.Empty, base.Size), base.BackBrush, base.ForeBrush, base.BorderPen, base.BorderStyle, base.BorderWidth, base.ShadowBrush, base.RotationAngle);
		FieldPainter.DrawCheckBox(base.Widget.ExtendedAppearance.Normal.On.Graphics, paintParams, StyleToString(base.Style), PdfCheckFieldState.Checked, base.Font);
		FieldPainter.DrawCheckBox(base.Widget.ExtendedAppearance.Normal.Off.Graphics, paintParams, StyleToString(base.Style), PdfCheckFieldState.Unchecked, base.Font);
		FieldPainter.DrawCheckBox(base.Widget.ExtendedAppearance.Pressed.On.Graphics, paintParams, StyleToString(base.Style), PdfCheckFieldState.PressedChecked, base.Font);
		FieldPainter.DrawCheckBox(base.Widget.ExtendedAppearance.Pressed.Off.Graphics, paintParams, StyleToString(base.Style), PdfCheckFieldState.PressedUnchecked, base.Font);
	}
}
