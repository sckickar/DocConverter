using System.Collections.Generic;
using DocGen.Drawing.DocIOHelper;

namespace DocGen.Drawing.SkiaSharpHelper;

internal sealed class ImageAttributes : IImageAttributes
{
	private ColorMap[] m_colorMap;

	private float m_transparency;

	private Dictionary<ColorAdjustType, ColorMatrix> m_colormatrices;

	internal ColorMap[] ColorMap => m_colorMap;

	internal float Transparency => m_transparency;

	internal Dictionary<ColorAdjustType, ColorMatrix> ColorMatrices => m_colormatrices;

	~ImageAttributes()
	{
		Dispose();
	}

	public void SetWrapMode(WrapMode mode)
	{
	}

	public void SetColorMatrix(IColorMatrix newColorMatrix, ColorMatrixFlag mode, ColorAdjustType type)
	{
		if (newColorMatrix != null)
		{
			ColorMatrix colorMatrix = newColorMatrix as ColorMatrix;
			m_transparency = colorMatrix.Matrix33;
			if (m_colormatrices == null)
			{
				m_colormatrices = new Dictionary<ColorAdjustType, ColorMatrix>();
			}
			if (!m_colormatrices.ContainsKey(type))
			{
				m_colormatrices.Add(type, colorMatrix);
			}
			else
			{
				m_colormatrices[type] = colorMatrix;
			}
			m_colormatrices[type].Flag = mode;
		}
	}

	public void SetGamma(float gamma, ColorAdjustType type)
	{
	}

	internal void SetRemapTable(ColorMap[] colorMap)
	{
		m_colorMap = colorMap;
	}

	internal void Dispose()
	{
		if (m_colorMap != null)
		{
			m_colorMap = null;
		}
		if (m_colormatrices != null)
		{
			m_colormatrices.Clear();
			m_colormatrices = null;
		}
	}
}
