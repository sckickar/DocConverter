using System.Collections.Generic;

namespace DocGen.Drawing.SkiaSharpHelper;

internal sealed class PathData
{
	private List<PointF> m_points;

	private List<byte> m_types;

	public PointF[] Points
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

	public byte[] Types
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

	public PathData()
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
