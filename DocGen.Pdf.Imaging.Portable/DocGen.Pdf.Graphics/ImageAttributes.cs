using System.Collections.Generic;
using DocGen.Drawing;

namespace DocGen.Pdf.Graphics;

internal sealed class ImageAttributes
{
	private ColorMap[] m_colorMap;

	private float m_transparency;

	private Dictionary<ColorAdjustType, ColorMatrix> m_colormatrices;

	internal ColorMap[] ColorMap => m_colorMap;

	internal Dictionary<ColorAdjustType, ColorMatrix> ColorMatrices => m_colormatrices;

	internal ImageAttributes()
	{
	}

	~ImageAttributes()
	{
		Dispose();
	}

	internal void SetColorMatrix(ColorMatrix newColorMatrix, ColorMatrixFlag mode, ColorAdjustType type)
	{
		if (newColorMatrix != null)
		{
			m_transparency = newColorMatrix.Matrix33;
			if (m_colormatrices == null)
			{
				m_colormatrices = new Dictionary<ColorAdjustType, ColorMatrix>();
			}
			if (!m_colormatrices.ContainsKey(type))
			{
				m_colormatrices.Add(type, newColorMatrix);
			}
			else
			{
				m_colormatrices[type] = newColorMatrix;
			}
			m_colormatrices[type].Flag = mode;
		}
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
