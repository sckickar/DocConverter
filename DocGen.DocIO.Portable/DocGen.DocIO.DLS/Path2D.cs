using System.Collections.Generic;
using System.Text;
using DocGen.Drawing;

namespace DocGen.DocIO.DLS;

internal class Path2D
{
	internal enum Path2DElements : ushort
	{
		Close = 1,
		MoveTo,
		LineTo,
		ArcTo,
		QuadBezTo,
		CubicBezTo
	}

	private string m_pathCommandType;

	private List<PointF> m_pathPoints;

	private double m_width;

	private double m_height;

	private bool m_isStroke = true;

	private List<string> m_pathElementList;

	internal string PathCommandType => m_pathCommandType;

	internal List<PointF> PathPoints => m_pathPoints;

	internal List<string> PathElements => m_pathElementList ?? (m_pathElementList = new List<string>());

	internal double Width
	{
		get
		{
			return m_width;
		}
		set
		{
			m_width = value;
		}
	}

	internal double Height
	{
		get
		{
			return m_height;
		}
		set
		{
			m_height = value;
		}
	}

	internal bool IsStroke
	{
		get
		{
			return m_isStroke;
		}
		set
		{
			m_isStroke = value;
		}
	}

	internal Path2D(string pathCommandType, List<PointF> pathPoints)
	{
		m_pathCommandType = pathCommandType;
		m_pathPoints = pathPoints;
	}

	internal Path2D()
	{
	}

	internal Path2D Clone()
	{
		Path2D path2D = (Path2D)MemberwiseClone();
		List<string> list = new List<string>();
		foreach (string pathElement in m_pathElementList)
		{
			list.Add(pathElement);
		}
		path2D.m_pathElementList = list;
		return path2D;
	}

	internal void Close()
	{
		if (m_pathPoints != null)
		{
			m_pathPoints.Clear();
			m_pathPoints = null;
		}
		if (m_pathElementList != null)
		{
			m_pathElementList.Clear();
			m_pathElementList = null;
		}
	}

	internal bool Compare(Path2D path2D)
	{
		if (Width != path2D.Width || Height != path2D.Height)
		{
			return false;
		}
		for (int i = 0; i < PathElements.Count; i++)
		{
			if (PathElements[i] != path2D.PathElements[i])
			{
				return false;
			}
		}
		return true;
	}

	internal StringBuilder GetAsString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(Width + ";");
		stringBuilder.Append(Height + ";");
		foreach (string pathElement in PathElements)
		{
			stringBuilder.Append(pathElement.ToString() + ";");
		}
		return stringBuilder;
	}
}
