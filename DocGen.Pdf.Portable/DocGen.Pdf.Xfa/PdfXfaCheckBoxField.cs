using System.Collections.Generic;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Interactive;

namespace DocGen.Pdf.Xfa;

public class PdfXfaCheckBoxField : PdfXfaStyledField
{
	private PdfXfaCheckedStyle m_checkedStyle;

	private PdfXfaCheckBoxAppearance m_checkBoxAppearance;

	private bool m_isChecked;

	private float m_checkBoxSize = 10f;

	private PdfXfaCaption m_caption = new PdfXfaCaption();

	internal new PdfXfaForm parent;

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

	public PdfXfaCheckBoxAppearance CheckBoxAppearance
	{
		get
		{
			return m_checkBoxAppearance;
		}
		set
		{
			m_checkBoxAppearance = value;
		}
	}

	public float CheckBoxSize
	{
		get
		{
			return m_checkBoxSize;
		}
		set
		{
			m_checkBoxSize = value;
		}
	}

	public PdfXfaCheckBoxField(string name, SizeF size)
	{
		base.Width = size.Width;
		base.Height = size.Height;
		base.Name = name;
	}

	public PdfXfaCheckBoxField(string name, SizeF size, bool isChecked)
	{
		base.Width = size.Width;
		base.Height = size.Height;
		base.Name = name;
		IsChecked = isChecked;
	}

	public PdfXfaCheckBoxField(string name, float width, float height)
	{
		base.Width = width;
		base.Height = height;
		base.Name = name;
	}

	public PdfXfaCheckBoxField(string name, float width, float height, bool isChecked)
	{
		base.Width = width;
		base.Height = height;
		base.Name = name;
		IsChecked = isChecked;
	}

	internal void Save(XfaWriter xfaWriter)
	{
		if (base.Name == "" || base.Name == string.Empty)
		{
			base.Name = "checkBox" + xfaWriter.m_fieldCount;
		}
		xfaWriter.Write.WriteStartElement("field");
		xfaWriter.Write.WriteAttributeString("name", base.Name);
		xfaWriter.SetSize(base.Height, base.Width, 0f, 0f);
		xfaWriter.SetRPR(base.Rotate, base.Visibility, base.ReadOnly);
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		dictionary.Add("shape", CheckBoxAppearance.ToString().ToLower());
		dictionary.Add("mark", CheckedStyle.ToString().ToLower());
		dictionary.Add("size", CheckBoxSize + "pt");
		xfaWriter.WriteUI("checkButton", dictionary, base.Border);
		SetMFTP(xfaWriter);
		if (Caption != null)
		{
			Caption.Save(xfaWriter);
		}
		xfaWriter.Write.WriteStartElement("items");
		xfaWriter.Write.WriteStartElement("integer");
		xfaWriter.Write.WriteString("1");
		xfaWriter.Write.WriteEndElement();
		xfaWriter.Write.WriteStartElement("integer");
		xfaWriter.Write.WriteString("0");
		xfaWriter.Write.WriteEndElement();
		xfaWriter.Write.WriteStartElement("integer");
		xfaWriter.Write.WriteString("2");
		xfaWriter.Write.WriteEndElement();
		xfaWriter.Write.WriteEndElement();
		xfaWriter.Write.WriteStartElement("value");
		xfaWriter.Write.WriteStartElement("integer");
		if (IsChecked)
		{
			xfaWriter.Write.WriteString("1");
		}
		else
		{
			xfaWriter.Write.WriteString("0");
		}
		xfaWriter.Write.WriteEndElement();
		xfaWriter.Write.WriteEndElement();
		xfaWriter.Write.WriteEndElement();
	}

	internal PdfField SaveAcroForm(PdfPage page, RectangleF bounds, string name)
	{
		PdfCheckBoxField pdfCheckBoxField = new PdfCheckBoxField(page, name);
		pdfCheckBoxField.Widget.Dictionary.isXfa = (pdfCheckBoxField.isXfa = true);
		pdfCheckBoxField.Style = GetStyle(CheckedStyle);
		if (IsChecked)
		{
			pdfCheckBoxField.Checked = true;
		}
		if (base.Visibility == PdfXfaVisibility.Invisible)
		{
			pdfCheckBoxField.Visibility = PdfFormFieldVisibility.Hidden;
		}
		if (base.ReadOnly || parent.ReadOnly || parent.m_isReadOnly)
		{
			pdfCheckBoxField.ReadOnly = true;
		}
		if (base.Font != null)
		{
			pdfCheckBoxField.Font = base.Font;
		}
		if (base.Border != null)
		{
			base.Border.ApplyAcroBorder(pdfCheckBoxField);
		}
		if (base.ForeColor != PdfColor.Empty)
		{
			pdfCheckBoxField.ForeColor = base.ForeColor;
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
					Caption.Width = bounds2.Width - CheckBoxSize;
				}
				else
				{
					Caption.Width = bounds2.Height - CheckBoxSize;
				}
			}
			else if (base.Rotate == PdfXfaRotateAngle.RotateAngle90 || base.Rotate == PdfXfaRotateAngle.RotateAngle270)
			{
				Caption.Width = bounds2.Height - CheckBoxSize;
			}
			else
			{
				Caption.Width = bounds2.Width - CheckBoxSize;
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
			num = base.Width - (CheckBoxSize + base.Margins.Left + base.Margins.Right);
			num2 = base.Height - Caption.Width - (CheckBoxSize + base.Margins.Bottom + base.Margins.Top);
		}
		else
		{
			num = base.Width - Caption.Width - (CheckBoxSize + base.Margins.Left + base.Margins.Right);
			num2 = base.Height - (CheckBoxSize + base.Margins.Bottom + base.Margins.Top);
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
				bounds2 = ((Caption.Position != PdfXfaPosition.Top) ? new RectangleF(new PointF(bounds2.Location.X + num, bounds2.Location.Y + num2), new SizeF(CheckBoxSize, CheckBoxSize)) : new RectangleF(new PointF(bounds2.Location.X + num, bounds2.Location.Y + Caption.Width + num2), new SizeF(CheckBoxSize, CheckBoxSize)));
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
				bounds2 = ((Caption.Position != 0) ? new RectangleF(new PointF(bounds2.Location.X + num, bounds2.Location.Y + num2), new SizeF(CheckBoxSize, CheckBoxSize)) : new RectangleF(new PointF(bounds2.Location.X + Caption.Width + num, bounds2.Location.Y + num2), new SizeF(CheckBoxSize, CheckBoxSize)));
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
				bounds2 = ((Caption.Position != PdfXfaPosition.Top) ? new RectangleF(new PointF(bounds2.Location.X + num, bounds2.Location.Y + Caption.Width + num2), new SizeF(CheckBoxSize, CheckBoxSize)) : new RectangleF(new PointF(bounds2.Location.X + num, bounds2.Location.Y + num2), new SizeF(CheckBoxSize, CheckBoxSize)));
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
				bounds2 = ((Caption.Position != 0) ? new RectangleF(new PointF(bounds2.Location.X + Caption.Width + num, bounds2.Location.Y + num2), new SizeF(CheckBoxSize, CheckBoxSize)) : new RectangleF(new PointF(bounds2.Location.X + num, bounds2.Location.Y + num2), new SizeF(CheckBoxSize, CheckBoxSize)));
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
				bounds2 = ((Caption.Position != PdfXfaPosition.Top) ? new RectangleF(new PointF(bounds2.Location.X + num2, bounds2.Location.Y + num), new SizeF(CheckBoxSize, CheckBoxSize)) : new RectangleF(new PointF(bounds2.Location.X + Caption.Width + num2, bounds2.Location.Y + num), new SizeF(CheckBoxSize, CheckBoxSize)));
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
				bounds2 = ((Caption.Position != 0) ? new RectangleF(new PointF(bounds2.Location.X + num2, bounds2.Location.Y + num + Caption.Width), new SizeF(CheckBoxSize, CheckBoxSize)) : new RectangleF(new PointF(bounds2.Location.X + num2, bounds2.Location.Y + num), new SizeF(CheckBoxSize, CheckBoxSize)));
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
				bounds2 = ((Caption.Position != PdfXfaPosition.Bottom) ? new RectangleF(new PointF(bounds2.Location.X + num2, bounds2.Location.Y + num), new SizeF(CheckBoxSize, CheckBoxSize)) : new RectangleF(new PointF(bounds2.Location.X + Caption.Width + num2, bounds2.Location.Y + num), new SizeF(CheckBoxSize, CheckBoxSize)));
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
				bounds2 = ((Caption.Position != PdfXfaPosition.Right) ? new RectangleF(new PointF(bounds2.Location.X + num2, bounds2.Location.Y + num + Caption.Width), new SizeF(CheckBoxSize, CheckBoxSize)) : new RectangleF(new PointF(bounds2.Location.X + num2, bounds2.Location.Y + num), new SizeF(CheckBoxSize, CheckBoxSize)));
			}
		}
		pdfCheckBoxField.Bounds = bounds2;
		pdfCheckBoxField.Widget.WidgetAppearance.RotationAngle = GetRotationAngle();
		return pdfCheckBoxField;
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
		PdfXfaCheckBoxField obj = (PdfXfaCheckBoxField)MemberwiseClone();
		obj.Caption = Caption.Clone() as PdfXfaCaption;
		return obj;
	}
}
