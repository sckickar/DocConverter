using System;
using DocGen.Drawing;
using DocGen.OfficeChart.Implementation.Shapes;

namespace DocGen.OfficeChart.Implementation;

internal class InteriorWrapper : CommonWrapper, IInterior, IOptimizedUpdate
{
	private ExtendedFormatImpl m_xFormat;

	private GradientWrapper m_gradient;

	public OfficeKnownColors PatternColorIndex
	{
		get
		{
			return m_xFormat.PatternColorIndex;
		}
		set
		{
			BeginUpdate();
			if (m_gradient != null)
			{
				FillPattern = OfficePattern.Solid;
			}
			m_xFormat.PatternColorIndex = value;
			EndUpdate();
		}
	}

	public Color PatternColor
	{
		get
		{
			return m_xFormat.PatternColor;
		}
		set
		{
			BeginUpdate();
			if (m_gradient != null)
			{
				FillPattern = OfficePattern.Solid;
			}
			m_xFormat.PatternColor = value;
			EndUpdate();
		}
	}

	public OfficeKnownColors ColorIndex
	{
		get
		{
			return m_xFormat.ColorIndex;
		}
		set
		{
			BeginUpdate();
			if (m_gradient != null)
			{
				FillPattern = OfficePattern.Solid;
			}
			m_xFormat.ColorIndex = value;
			EndUpdate();
		}
	}

	public Color Color
	{
		get
		{
			return m_xFormat.Color;
		}
		set
		{
			BeginUpdate();
			if (m_gradient != null)
			{
				FillPattern = OfficePattern.Solid;
			}
			m_xFormat.Color = value;
			EndUpdate();
		}
	}

	public IGradient Gradient => m_gradient;

	public OfficePattern FillPattern
	{
		get
		{
			return m_xFormat.FillPattern;
		}
		set
		{
			if (m_xFormat.Workbook.Version == OfficeVersion.Excel97to2003 && value == OfficePattern.Gradient)
			{
				throw new ArgumentException("Excel97to2003 version does not support gradient fill type.");
			}
			BeginUpdate();
			m_xFormat.FillPattern = value;
			if (value == OfficePattern.Gradient)
			{
				CreateGradientWrapper();
			}
			else
			{
				m_gradient = null;
				m_xFormat.Gradient = null;
			}
			EndUpdate();
		}
	}

	public ExtendedFormatImpl Wrapped => m_xFormat;

	public event EventHandler AfterChangeEvent;

	public InteriorWrapper()
	{
	}

	public InteriorWrapper(ExtendedFormatImpl format)
	{
		if (format == null)
		{
			throw new ArgumentNullException("format");
		}
		m_xFormat = format;
		if (format.FillPattern == OfficePattern.Gradient)
		{
			CreateGradientWrapper();
		}
	}

	private void WrappedGradientAfterChangeEvent(object sender, EventArgs e)
	{
		BeginUpdate();
		m_xFormat.Gradient = m_gradient.Wrapped;
		EndUpdate();
	}

	private void CreateGradientWrapper()
	{
		WorkbookImpl workbook = m_xFormat.Workbook;
		ShapeFillImpl shapeFillImpl = (ShapeFillImpl)m_xFormat.Gradient;
		if (shapeFillImpl == null)
		{
			shapeFillImpl = new ShapeFillImpl(workbook.Application, m_xFormat);
			shapeFillImpl.FillType = OfficeFillType.Gradient;
		}
		m_gradient = new GradientWrapper(shapeFillImpl);
		m_gradient.AfterChangeEvent += WrappedGradientAfterChangeEvent;
		BeginUpdate();
		m_xFormat.Gradient = m_gradient.Wrapped;
		EndUpdate();
	}

	public override void BeginUpdate()
	{
		if (base.BeginCallsCount == 0)
		{
			m_xFormat = (ExtendedFormatImpl)Wrapped.Clone();
		}
		base.BeginUpdate();
	}

	public override void EndUpdate()
	{
		base.EndUpdate();
		if (base.BeginCallsCount == 0)
		{
			m_xFormat.Workbook.SetChanged();
			if (this.AfterChangeEvent != null)
			{
				this.AfterChangeEvent(this, EventArgs.Empty);
			}
		}
	}

	internal void Dispose()
	{
		m_xFormat.clearAll();
		m_gradient.Dispose();
	}
}
