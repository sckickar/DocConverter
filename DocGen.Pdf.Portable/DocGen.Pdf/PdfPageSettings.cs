using System;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;

namespace DocGen.Pdf;

public class PdfPageSettings : ICloneable
{
	private PdfPageOrientation m_orientation;

	private SizeF m_size = PdfPageSize.A4;

	private PdfMargins m_margins = new PdfMargins();

	private PdfPageRotateAngle m_rotateAngle;

	private PdfGraphicsUnit m_logicalUnit = PdfGraphicsUnit.Point;

	private PointF m_origin = PointF.Empty;

	internal bool m_isRotation;

	private bool m_isOrientation;

	private PdfPageTransition m_transition;

	public PdfPageOrientation Orientation
	{
		get
		{
			return m_orientation;
		}
		set
		{
			m_isOrientation = true;
			if (m_orientation != value)
			{
				m_orientation = value;
				UpdateSize(value);
			}
		}
	}

	public SizeF Size
	{
		get
		{
			return m_size;
		}
		set
		{
			if (m_isOrientation)
			{
				AssignSize(value);
				return;
			}
			m_size = value;
			AssignOrientation();
		}
	}

	public float Width
	{
		get
		{
			return m_size.Width;
		}
		set
		{
			m_size.Width = value;
			AssignOrientation();
		}
	}

	public float Height
	{
		get
		{
			return m_size.Height;
		}
		set
		{
			m_size.Height = value;
			AssignOrientation();
		}
	}

	public PdfMargins Margins
	{
		get
		{
			return m_margins;
		}
		set
		{
			m_margins = value;
		}
	}

	public PdfPageRotateAngle Rotate
	{
		get
		{
			return m_rotateAngle;
		}
		set
		{
			m_rotateAngle = value;
			m_isRotation = true;
		}
	}

	public PdfPageTransition Transition
	{
		get
		{
			if (m_transition == null)
			{
				m_transition = new PdfPageTransition();
			}
			return m_transition;
		}
		set
		{
			m_transition = value;
		}
	}

	internal PdfGraphicsUnit Unit
	{
		get
		{
			return m_logicalUnit;
		}
		set
		{
			m_logicalUnit = value;
		}
	}

	internal PointF Origin
	{
		get
		{
			return m_origin;
		}
		set
		{
			m_origin = value;
		}
	}

	public PdfPageSettings()
	{
	}

	public PdfPageSettings(SizeF size)
	{
		m_size = size;
		AssignOrientation();
	}

	public PdfPageSettings(PdfPageOrientation pageOrientation)
	{
		m_orientation = pageOrientation;
		m_isOrientation = true;
		UpdateSize(pageOrientation);
	}

	public PdfPageSettings(SizeF size, PdfPageOrientation pageOrientation)
	{
		m_size = size;
		m_orientation = pageOrientation;
		m_isOrientation = true;
		UpdateSize(pageOrientation);
	}

	public PdfPageSettings(float margins)
	{
		m_margins.SetMargins(margins);
	}

	public PdfPageSettings(float leftMargin, float topMargin, float rightMargin, float bottomMargin)
	{
		m_margins.SetMargins(leftMargin, topMargin, rightMargin, bottomMargin);
	}

	public PdfPageSettings(SizeF size, float margins)
	{
		m_size = size;
		m_margins.SetMargins(margins);
		AssignOrientation();
	}

	public PdfPageSettings(SizeF size, float leftMargin, float topMargin, float rightMargin, float bottomMargin)
	{
		m_size = size;
		m_margins.SetMargins(leftMargin, topMargin, rightMargin, bottomMargin);
		AssignOrientation();
	}

	public PdfPageSettings(SizeF size, PdfPageOrientation pageOrientation, float margins)
	{
		m_size = size;
		m_orientation = pageOrientation;
		m_isOrientation = true;
		m_margins.SetMargins(margins);
		UpdateSize(pageOrientation);
	}

	public PdfPageSettings(SizeF size, PdfPageOrientation pageOrientation, float leftMargin, float topMargin, float rightMargin, float bottomMargin)
	{
		m_size = size;
		m_orientation = pageOrientation;
		m_isOrientation = true;
		m_margins.SetMargins(leftMargin, topMargin, rightMargin, bottomMargin);
		UpdateSize(pageOrientation);
	}

	public void SetMargins(float margins)
	{
		m_margins.SetMargins(margins);
	}

	public void SetMargins(float leftRight, float topBottom)
	{
		m_margins.SetMargins(leftRight, topBottom);
	}

	public void SetMargins(float left, float top, float right, float bottom)
	{
		m_margins.SetMargins(left, top, right, bottom);
	}

	public object Clone()
	{
		PdfPageSettings pdfPageSettings = (PdfPageSettings)MemberwiseClone();
		pdfPageSettings.m_margins = (PdfMargins)Margins.Clone();
		if (AssignTransition() != null)
		{
			pdfPageSettings.Transition = (PdfPageTransition)Transition.Clone();
		}
		return pdfPageSettings;
	}

	internal SizeF GetActualSize()
	{
		float width = Width - (Margins.Left + Margins.Right);
		float height = Height - (Margins.Top + Margins.Bottom);
		return new SizeF(width, height);
	}

	internal PdfPageTransition AssignTransition()
	{
		return m_transition;
	}

	private void UpdateSize(PdfPageOrientation orientation)
	{
		float num = Math.Min(Width, Height);
		float num2 = Math.Max(Width, Height);
		switch (orientation)
		{
		case PdfPageOrientation.Portrait:
			Size = new SizeF(num, num2);
			break;
		case PdfPageOrientation.Landscape:
			Size = new SizeF(num2, num);
			break;
		}
	}

	private void AssignSize(SizeF size)
	{
		float num = Math.Min(size.Width, size.Height);
		float num2 = Math.Max(size.Width, size.Height);
		if (Orientation == PdfPageOrientation.Portrait)
		{
			m_size = new SizeF(num, num2);
		}
		else
		{
			m_size = new SizeF(num2, num);
		}
	}

	private void AssignOrientation()
	{
		m_orientation = ((!(m_size.Height >= m_size.Width)) ? PdfPageOrientation.Landscape : PdfPageOrientation.Portrait);
	}
}
