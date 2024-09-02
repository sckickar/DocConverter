using System;
using DocGen.Drawing;
using DocGen.OfficeChart.Implementation.Shapes;

namespace DocGen.OfficeChart.Implementation;

internal class GradientWrapper : CommonWrapper, IGradient, IOptimizedUpdate
{
	private ShapeFillImpl m_gradient;

	public ChartColor BackColorObject => m_gradient.BackColorObject;

	public Color BackColor
	{
		get
		{
			return m_gradient.BackColor;
		}
		set
		{
			if (value != BackColor)
			{
				BeginUpdate();
				m_gradient.BackColor = value;
				EndUpdate();
			}
		}
	}

	public OfficeKnownColors BackColorIndex
	{
		get
		{
			return m_gradient.BackColorIndex;
		}
		set
		{
			if (value != BackColorIndex)
			{
				BeginUpdate();
				m_gradient.BackColorIndex = value;
				EndUpdate();
			}
		}
	}

	public ChartColor ForeColorObject => m_gradient.ForeColorObject;

	public Color ForeColor
	{
		get
		{
			return m_gradient.ForeColor;
		}
		set
		{
			if (value != ForeColor)
			{
				BeginUpdate();
				m_gradient.ForeColor = value;
				EndUpdate();
			}
		}
	}

	public OfficeKnownColors ForeColorIndex
	{
		get
		{
			return m_gradient.ForeColorIndex;
		}
		set
		{
			if (value != ForeColorIndex)
			{
				BeginUpdate();
				m_gradient.ForeColorIndex = value;
				EndUpdate();
			}
		}
	}

	public OfficeGradientStyle GradientStyle
	{
		get
		{
			return m_gradient.GradientStyle;
		}
		set
		{
			if (value != GradientStyle)
			{
				BeginUpdate();
				m_gradient.GradientStyle = value;
				EndUpdate();
			}
		}
	}

	public OfficeGradientVariants GradientVariant
	{
		get
		{
			return m_gradient.GradientVariant;
		}
		set
		{
			ValidateGradientVariant(value);
			if (value != GradientVariant)
			{
				BeginUpdate();
				m_gradient.GradientVariant = value;
				EndUpdate();
			}
		}
	}

	public ShapeFillImpl Wrapped => m_gradient;

	public event EventHandler AfterChangeEvent;

	public GradientWrapper()
	{
	}

	public GradientWrapper(ShapeFillImpl gradient)
	{
		if (gradient == null)
		{
			throw new ArgumentNullException("gradient");
		}
		m_gradient = gradient;
	}

	public int CompareTo(IGradient gradient)
	{
		return m_gradient.CompareTo(gradient);
	}

	public void TwoColorGradient()
	{
		BeginUpdate();
		m_gradient.TwoColorGradient();
		EndUpdate();
	}

	public void TwoColorGradient(OfficeGradientStyle style, OfficeGradientVariants variant)
	{
		BeginUpdate();
		m_gradient.TwoColorGradient(style, variant);
		EndUpdate();
	}

	public override void BeginUpdate()
	{
		if (base.BeginCallsCount == 0)
		{
			m_gradient = m_gradient.Clone(m_gradient.Parent);
		}
		base.BeginUpdate();
	}

	public override void EndUpdate()
	{
		base.EndUpdate();
		if (base.BeginCallsCount == 0)
		{
			((ExtendedFormatImpl)m_gradient.Parent).Workbook.SetChanged();
			if (this.AfterChangeEvent != null)
			{
				this.AfterChangeEvent(this, EventArgs.Empty);
			}
		}
	}

	private void ValidateGradientVariant(OfficeGradientVariants gradientVariant)
	{
		switch (GradientStyle)
		{
		case OfficeGradientStyle.Horizontal:
		case OfficeGradientStyle.Vertical:
		case OfficeGradientStyle.DiagonalUp:
		case OfficeGradientStyle.DiagonalDown:
			if (gradientVariant == OfficeGradientVariants.ShadingVariants_4)
			{
				throw new ArgumentException("Shading variant 4 is not valid for current gradient style.");
			}
			break;
		case OfficeGradientStyle.FromCenter:
			if (gradientVariant == OfficeGradientVariants.ShadingVariants_2 || gradientVariant == OfficeGradientVariants.ShadingVariants_3 || gradientVariant == OfficeGradientVariants.ShadingVariants_4)
			{
				throw new ArgumentException("Current shading variant is not valid for from center gradient style.");
			}
			break;
		}
	}

	internal void Dispose()
	{
		this.AfterChangeEvent = null;
		m_gradient.Clear();
	}
}
