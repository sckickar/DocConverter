using System.Collections.Generic;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Interactive;

namespace DocGen.Pdf.Xfa;

public class PdfXfaRadioButtonField : PdfXfaStyledField
{
	private string m_toolTip = string.Empty;

	private PdfXfaRotateAngle m_rotate;

	private PdfXfaCheckedStyle m_checkedStyle = PdfXfaCheckedStyle.Circle;

	private PdfXfaCheckBoxAppearance m_radioButtonAppearance = PdfXfaCheckBoxAppearance.Round;

	private bool m_isChecked;

	private float m_radioButtonSize = 10f;

	private PdfXfaCaption m_caption = new PdfXfaCaption();

	internal new PdfXfaField parent;

	public bool IsChecked
	{
		get
		{
			return m_isChecked;
		}
		set
		{
			m_isChecked = value;
		}
	}

	public PdfXfaCaption Caption
	{
		get
		{
			return m_caption;
		}
		set
		{
			m_caption = value;
		}
	}

	public PdfXfaCheckedStyle CheckedStyle
	{
		get
		{
			return m_checkedStyle;
		}
		set
		{
			m_checkedStyle = value;
		}
	}

	public PdfXfaCheckBoxAppearance RadioButtonAppearance
	{
		get
		{
			return m_radioButtonAppearance;
		}
		set
		{
			m_radioButtonAppearance = value;
		}
	}

	public float RadioButtonSize
	{
		get
		{
			return m_radioButtonSize;
		}
		set
		{
			m_radioButtonSize = value;
		}
	}

	public PdfXfaRadioButtonField(string name, SizeF size)
	{
		base.Name = name;
		base.Width = size.Width;
		base.Height = size.Height;
	}

	public PdfXfaRadioButtonField(string name, float width, float height)
	{
		base.Name = name;
		base.Width = width;
		base.Height = height;
	}

	internal void Save(XfaWriter xfaWriter, int index)
	{
		xfaWriter.Write.WriteStartElement("field");
		if (base.Name != null)
		{
			xfaWriter.Write.WriteAttributeString("name", base.Name);
		}
		xfaWriter.SetSize(base.Height, base.Width, 0f, 0f);
		xfaWriter.SetRPR(base.Rotate, base.Visibility, base.ReadOnly);
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		dictionary.Add("shape", RadioButtonAppearance.ToString().ToLower());
		dictionary.Add("mark", CheckedStyle.ToString().ToLower());
		dictionary.Add("size", RadioButtonSize + "pt");
		xfaWriter.WriteUI("checkButton", dictionary, base.Border);
		xfaWriter.WriteItems(index.ToString());
		if (IsChecked)
		{
			xfaWriter.WriteValue(index.ToString(), 0);
		}
		SetMFTP(xfaWriter);
		if (Caption != null)
		{
			Caption.Save(xfaWriter);
		}
		xfaWriter.Write.WriteEndElement();
	}

	internal new void SetMFTP(XfaWriter xfaWriter)
	{
		xfaWriter.WriteFontInfo(base.Font, base.ForeColor);
		xfaWriter.WriteMargins(base.Margins);
		if (base.ToolTip != null && base.ToolTip != "")
		{
			xfaWriter.WriteToolTip(base.ToolTip);
		}
		xfaWriter.WritePragraph(base.VerticalAlignment, base.HorizontalAlignment);
	}

	internal PdfRadioButtonListItem SaveAcroForm(PdfPage page, RectangleF bounds)
	{
		PdfRadioButtonListItem pdfRadioButtonListItem = new PdfRadioButtonListItem(base.Name);
		pdfRadioButtonListItem.isXfa = true;
		pdfRadioButtonListItem.Style = GetStyle(CheckedStyle);
		if (base.Font != null)
		{
			pdfRadioButtonListItem.Font = base.Font;
		}
		if (base.ReadOnly || (parent as PdfXfaRadioButtonGroup).ReadOnly || (parent as PdfXfaRadioButtonGroup).parent.m_isReadOnly)
		{
			pdfRadioButtonListItem.ReadOnly = true;
		}
		if (base.Border != null)
		{
			base.Border.ApplyAcroBorder(pdfRadioButtonListItem);
		}
		if (base.ForeColor != PdfColor.Empty)
		{
			pdfRadioButtonListItem.ForeColor = base.ForeColor;
		}
		if (base.Visibility == PdfXfaVisibility.Invisible)
		{
			pdfRadioButtonListItem.Visibility = PdfFormFieldVisibility.Hidden;
		}
		RectangleF bounds2 = default(RectangleF);
		SizeF size = GetSize();
		bounds2.Location = new PointF(bounds.Location.X + base.Margins.Left, bounds.Location.Y + base.Margins.Top);
		bounds2.Size = new SizeF(size.Width - (base.Margins.Right + base.Margins.Left), size.Height - (base.Margins.Top + base.Margins.Bottom));
		if (Caption.Width == 0f)
		{
			if (Caption.Position == PdfXfaPosition.Top || Caption.Position == PdfXfaPosition.Bottom)
			{
				if (base.Rotate == PdfXfaRotateAngle.RotateAngle90 || base.Rotate == PdfXfaRotateAngle.RotateAngle270)
				{
					Caption.Width = bounds2.Width - RadioButtonSize;
				}
				else
				{
					Caption.Width = bounds2.Height - RadioButtonSize;
				}
			}
			else if (base.Rotate == PdfXfaRotateAngle.RotateAngle90 || base.Rotate == PdfXfaRotateAngle.RotateAngle270)
			{
				Caption.Width = bounds2.Height - RadioButtonSize;
			}
			else
			{
				Caption.Width = bounds2.Width - RadioButtonSize;
			}
		}
		if (base.Visibility != PdfXfaVisibility.Invisible)
		{
			Caption.DrawText(page, bounds2, GetRotationAngle());
		}
		float num = 0f;
		float num2 = 0f;
		if (Caption.Position == PdfXfaPosition.Top || Caption.Position == PdfXfaPosition.Bottom)
		{
			num = base.Width - RadioButtonSize - (RadioButtonSize + base.Margins.Left + base.Margins.Right);
			num2 = base.Height - Caption.Width - (RadioButtonSize + base.Margins.Bottom + base.Margins.Top);
		}
		else
		{
			num = base.Width - Caption.Width - (RadioButtonSize + base.Margins.Left + base.Margins.Right);
			num2 = base.Height - (RadioButtonSize + base.Margins.Bottom + base.Margins.Top);
		}
		if (base.Rotate == PdfXfaRotateAngle.RotateAngle0)
		{
			if (Caption.Position == PdfXfaPosition.Top || Caption.Position == PdfXfaPosition.Bottom)
			{
				if (base.HorizontalAlignment == PdfXfaHorizontalAlignment.Left)
				{
					num = 0f;
				}
				else if (base.HorizontalAlignment == PdfXfaHorizontalAlignment.Center)
				{
					num /= 2f;
				}
				if (base.VerticalAlignment == PdfXfaVerticalAlignment.Top)
				{
					num2 = 0f;
				}
				else if (base.VerticalAlignment == PdfXfaVerticalAlignment.Middle)
				{
					num2 /= 2f;
				}
				bounds2 = ((Caption.Position != PdfXfaPosition.Top) ? new RectangleF(new PointF(bounds2.Location.X + num, bounds2.Location.Y + num2), new SizeF(RadioButtonSize, RadioButtonSize)) : new RectangleF(new PointF(bounds2.Location.X + num, bounds2.Location.Y + Caption.Width + num2), new SizeF(RadioButtonSize, RadioButtonSize)));
			}
			else if (Caption.Position == PdfXfaPosition.Left || Caption.Position == PdfXfaPosition.Right)
			{
				if (base.HorizontalAlignment == PdfXfaHorizontalAlignment.Left)
				{
					num = 0f;
				}
				else if (base.HorizontalAlignment == PdfXfaHorizontalAlignment.Center)
				{
					num /= 2f;
				}
				if (base.VerticalAlignment == PdfXfaVerticalAlignment.Top)
				{
					num2 = 0f;
				}
				else if (base.VerticalAlignment == PdfXfaVerticalAlignment.Middle)
				{
					num2 /= 2f;
				}
				bounds2 = ((Caption.Position != 0) ? new RectangleF(new PointF(bounds2.Location.X + num, bounds2.Location.Y + num2), new SizeF(RadioButtonSize, RadioButtonSize)) : new RectangleF(new PointF(bounds2.Location.X + Caption.Width + num, bounds2.Location.Y + num2), new SizeF(RadioButtonSize, RadioButtonSize)));
			}
		}
		else if (base.Rotate == PdfXfaRotateAngle.RotateAngle180)
		{
			if (Caption.Position == PdfXfaPosition.Top || Caption.Position == PdfXfaPosition.Bottom)
			{
				if (base.HorizontalAlignment == PdfXfaHorizontalAlignment.Right)
				{
					num = 0f;
				}
				else if (base.HorizontalAlignment == PdfXfaHorizontalAlignment.Center)
				{
					num /= 2f;
				}
				if (base.VerticalAlignment == PdfXfaVerticalAlignment.Bottom)
				{
					num2 = 0f;
				}
				else if (base.VerticalAlignment == PdfXfaVerticalAlignment.Middle)
				{
					num2 /= 2f;
				}
				bounds2 = ((Caption.Position != PdfXfaPosition.Top) ? new RectangleF(new PointF(bounds2.Location.X + num, bounds2.Location.Y + Caption.Width + num2), new SizeF(RadioButtonSize, RadioButtonSize)) : new RectangleF(new PointF(bounds2.Location.X + num, bounds2.Location.Y + num2), new SizeF(RadioButtonSize, RadioButtonSize)));
			}
			else if (Caption.Position == PdfXfaPosition.Left || Caption.Position == PdfXfaPosition.Right)
			{
				if (base.HorizontalAlignment == PdfXfaHorizontalAlignment.Right)
				{
					num = 0f;
				}
				else if (base.HorizontalAlignment == PdfXfaHorizontalAlignment.Center)
				{
					num /= 2f;
				}
				if (base.VerticalAlignment == PdfXfaVerticalAlignment.Bottom)
				{
					num2 = 0f;
				}
				else if (base.VerticalAlignment == PdfXfaVerticalAlignment.Middle)
				{
					num2 /= 2f;
				}
				bounds2 = ((Caption.Position != 0) ? new RectangleF(new PointF(bounds2.Location.X + Caption.Width + num, bounds2.Location.Y + num2), new SizeF(RadioButtonSize, RadioButtonSize)) : new RectangleF(new PointF(bounds2.Location.X + num, bounds2.Location.Y + num2), new SizeF(RadioButtonSize, RadioButtonSize)));
			}
		}
		else if (base.Rotate == PdfXfaRotateAngle.RotateAngle90)
		{
			if (Caption.Position == PdfXfaPosition.Top || Caption.Position == PdfXfaPosition.Bottom)
			{
				if (base.HorizontalAlignment == PdfXfaHorizontalAlignment.Right)
				{
					num = 0f;
				}
				else if (base.HorizontalAlignment == PdfXfaHorizontalAlignment.Center)
				{
					num /= 2f;
				}
				if (base.VerticalAlignment == PdfXfaVerticalAlignment.Top)
				{
					num2 = 0f;
				}
				else if (base.VerticalAlignment == PdfXfaVerticalAlignment.Middle)
				{
					num2 /= 2f;
				}
				bounds2 = ((Caption.Position != PdfXfaPosition.Top) ? new RectangleF(new PointF(bounds2.Location.X + num2, bounds2.Location.Y + num), new SizeF(RadioButtonSize, RadioButtonSize)) : new RectangleF(new PointF(bounds2.Location.X + Caption.Width + num2, bounds2.Location.Y + num), new SizeF(RadioButtonSize, RadioButtonSize)));
			}
			else if (Caption.Position == PdfXfaPosition.Left || Caption.Position == PdfXfaPosition.Right)
			{
				if (base.HorizontalAlignment == PdfXfaHorizontalAlignment.Right)
				{
					num = 0f;
				}
				else if (base.HorizontalAlignment == PdfXfaHorizontalAlignment.Center)
				{
					num /= 2f;
				}
				if (base.VerticalAlignment == PdfXfaVerticalAlignment.Top)
				{
					num2 = 0f;
				}
				else if (base.VerticalAlignment == PdfXfaVerticalAlignment.Middle)
				{
					num2 /= 2f;
				}
				bounds2 = ((Caption.Position != 0) ? new RectangleF(new PointF(bounds2.Location.X + num2, bounds2.Location.Y + num + Caption.Width), new SizeF(RadioButtonSize, RadioButtonSize)) : new RectangleF(new PointF(bounds2.Location.X + num2, bounds2.Location.Y + num), new SizeF(RadioButtonSize, RadioButtonSize)));
			}
		}
		else if (base.Rotate == PdfXfaRotateAngle.RotateAngle270)
		{
			if (Caption.Position == PdfXfaPosition.Top || Caption.Position == PdfXfaPosition.Bottom)
			{
				if (base.HorizontalAlignment == PdfXfaHorizontalAlignment.Left)
				{
					num = 0f;
				}
				else if (base.HorizontalAlignment == PdfXfaHorizontalAlignment.Center)
				{
					num /= 2f;
				}
				if (base.VerticalAlignment == PdfXfaVerticalAlignment.Bottom)
				{
					num2 = 0f;
				}
				else if (base.VerticalAlignment == PdfXfaVerticalAlignment.Middle)
				{
					num2 /= 2f;
				}
				bounds2 = ((Caption.Position != PdfXfaPosition.Bottom) ? new RectangleF(new PointF(bounds2.Location.X + num2, bounds2.Location.Y + num), new SizeF(RadioButtonSize, RadioButtonSize)) : new RectangleF(new PointF(bounds2.Location.X + Caption.Width + num2, bounds2.Location.Y + num), new SizeF(RadioButtonSize, RadioButtonSize)));
			}
			else if (Caption.Position == PdfXfaPosition.Left || Caption.Position == PdfXfaPosition.Right)
			{
				if (base.HorizontalAlignment == PdfXfaHorizontalAlignment.Left)
				{
					num = 0f;
				}
				else if (base.HorizontalAlignment == PdfXfaHorizontalAlignment.Center)
				{
					num /= 2f;
				}
				if (base.VerticalAlignment == PdfXfaVerticalAlignment.Bottom)
				{
					num2 = 0f;
				}
				else if (base.VerticalAlignment == PdfXfaVerticalAlignment.Middle)
				{
					num2 /= 2f;
				}
				bounds2 = ((Caption.Position != PdfXfaPosition.Right) ? new RectangleF(new PointF(bounds2.Location.X + num2, bounds2.Location.Y + num + Caption.Width), new SizeF(RadioButtonSize, RadioButtonSize)) : new RectangleF(new PointF(bounds2.Location.X + num2, bounds2.Location.Y + num), new SizeF(RadioButtonSize, RadioButtonSize)));
			}
		}
		pdfRadioButtonListItem.Bounds = bounds2;
		pdfRadioButtonListItem.Widget.WidgetAppearance.RotationAngle = GetRotationAngle();
		return pdfRadioButtonListItem;
	}

	private PdfCheckBoxStyle GetStyle(PdfXfaCheckedStyle style)
	{
		PdfCheckBoxStyle result = PdfCheckBoxStyle.Check;
		switch (style)
		{
		case PdfXfaCheckedStyle.Check:
			result = PdfCheckBoxStyle.Check;
			break;
		case PdfXfaCheckedStyle.Circle:
			result = PdfCheckBoxStyle.Circle;
			break;
		case PdfXfaCheckedStyle.Cross:
			result = PdfCheckBoxStyle.Cross;
			break;
		case PdfXfaCheckedStyle.Diamond:
			result = PdfCheckBoxStyle.Diamond;
			break;
		case PdfXfaCheckedStyle.Square:
			result = PdfCheckBoxStyle.Square;
			break;
		case PdfXfaCheckedStyle.Star:
			result = PdfCheckBoxStyle.Star;
			break;
		}
		return result;
	}

	public object Clone()
	{
		PdfXfaRadioButtonField obj = (PdfXfaRadioButtonField)MemberwiseClone();
		obj.Caption = Caption.Clone() as PdfXfaCaption;
		return obj;
	}
}
