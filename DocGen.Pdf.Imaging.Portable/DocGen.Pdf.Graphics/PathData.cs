using System.Collections.Generic;
using DocGen.Drawing;

namespace DocGen.Pdf.Graphics;

internal sealed class PathData
{
	private List<PointF> m_points;

	private List<byte> m_types;

	internal PointF[] Points
	{
		get
		{
			return m_points.ToArray();
		}
		set
		{
			m_points = new List<PointF>(value);
		}
	}

	internal byte[] Types
	{
		get
		{
			return m_types.ToArray();
		}
		set
		{
			m_types = new List<byte>(value);
		}
	}

	internal PathData()
	{
		m_points = new List<PointF>();
		m_types = new List<byte>();
	}

	internal void AddPoint(params PointF[] points)
	{
		m_points.AddRange(points);
	}

	internal void AddPointType(byte skPathVerbValue)
	{
		m_types.Add(skPathVerbValue);
	}
}
