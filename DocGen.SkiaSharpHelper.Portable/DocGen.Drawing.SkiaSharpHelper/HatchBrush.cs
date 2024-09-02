using System;
using DocGen.Drawing.DocIOHelper;

namespace DocGen.Drawing.SkiaSharpHelper;

internal class HatchBrush : Brush, IHatchBrush, IBrush, IDisposable
{
	private HatchStyle m_hatchStyle;

	private Color m_foreColor;

	private Color m_backColor;

	internal Color BackgroundColor => m_backColor;

	internal Color ForegroundColor => m_foreColor;

	internal HatchStyle HatchStyle => m_hatchStyle;

	internal HatchBrush(HatchStyle hatchstyle, Color foreColor)
		: base(foreColor)
	{
		m_hatchStyle = hatchstyle;
		m_foreColor = foreColor;
	}

	internal HatchBrush(HatchStyle hatchstyle, Color foreColor, Color backColor)
		: base(foreColor)
	{
		m_hatchStyle = hatchstyle;
		m_foreColor = foreColor;
		m_backColor = backColor;
	}
}
