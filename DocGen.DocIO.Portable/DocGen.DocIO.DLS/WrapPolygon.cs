using System.Collections.Generic;
using System.Text;
using DocGen.Drawing;

namespace DocGen.DocIO.DLS;

internal class WrapPolygon
{
	private byte m_bFlags;

	private List<PointF> m_vertices;

	internal bool Edited
	{
		get
		{
			return (m_bFlags & 1) != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xFEu) | (value ? 1u : 0u));
		}
	}

	internal List<PointF> Vertices
	{
		get
		{
			return m_vertices;
		}
		set
		{
			m_vertices = value;
		}
	}

	internal WrapPolygon()
	{
		m_vertices = new List<PointF>();
	}

	internal WrapPolygon Clone()
	{
		WrapPolygon wrapPolygon = new WrapPolygon();
		if (Vertices != null)
		{
			wrapPolygon.Vertices = new List<PointF>();
			foreach (PointF vertex in Vertices)
			{
				PointF item = new PointF(vertex.X, vertex.Y);
				wrapPolygon.Vertices.Add(item);
			}
		}
		return wrapPolygon;
	}

	internal void Close()
	{
		if (m_vertices != null)
		{
			m_vertices.Clear();
			m_vertices = null;
		}
	}

	internal bool Compare(WrapPolygon wrapPolygon)
	{
		if (Vertices != null && wrapPolygon.Vertices != null)
		{
			if (Vertices.Count != wrapPolygon.Vertices.Count)
			{
				return false;
			}
			for (int i = 0; i < Vertices.Count; i++)
			{
				if (Vertices[i].X != wrapPolygon.Vertices[i].X || Vertices[i].Y != wrapPolygon.Vertices[i].Y)
				{
					return false;
				}
			}
		}
		return true;
	}

	internal StringBuilder GetAsString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		string text = (Edited ? "1" : "0");
		stringBuilder.Append(text + ";");
		foreach (PointF vertex in Vertices)
		{
			stringBuilder.Append(vertex.X + ";" + vertex.Y + ";");
		}
		return stringBuilder;
	}
}
