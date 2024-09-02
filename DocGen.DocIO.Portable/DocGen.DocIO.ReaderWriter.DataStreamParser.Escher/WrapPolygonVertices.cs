using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DocGen.Drawing;

namespace DocGen.DocIO.ReaderWriter.DataStreamParser.Escher;

internal class WrapPolygonVertices
{
	private int m_key;

	private msofbtRGFOPTE m_prop;

	private List<PointF> m_coords;

	private uint m_nelems;

	private uint m_nelemsalloc;

	private uint m_cbelem;

	internal List<PointF> Coords
	{
		get
		{
			return m_coords;
		}
		set
		{
			m_coords = value;
		}
	}

	internal uint nElems
	{
		get
		{
			return m_nelems;
		}
		set
		{
			m_nelems = value;
		}
	}

	internal uint nElemsAlloc
	{
		get
		{
			return m_nelemsalloc;
		}
		set
		{
			m_nelemsalloc = value;
		}
	}

	internal uint cbElem
	{
		get
		{
			return m_cbelem;
		}
		set
		{
			m_cbelem = value;
		}
	}

	internal WrapPolygonVertices(msofbtRGFOPTE prop, int key)
	{
		m_prop = prop;
		m_key = key;
		m_coords = new List<PointF>();
		readArrayData();
	}

	internal bool Compare(WrapPolygonVertices wrapPolygonVertices)
	{
		if (nElems != wrapPolygonVertices.nElems || nElemsAlloc != wrapPolygonVertices.nElemsAlloc || cbElem != wrapPolygonVertices.cbElem)
		{
			return false;
		}
		if (Coords.Count != wrapPolygonVertices.Coords.Count)
		{
			return false;
		}
		for (int i = 0; i < Coords.Count; i++)
		{
			if (Coords[i].X != wrapPolygonVertices.Coords[i].X || Coords[i].Y != wrapPolygonVertices.Coords[i].Y)
			{
				return false;
			}
		}
		return true;
	}

	internal StringBuilder GetAsString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(nElems + ";");
		stringBuilder.Append(nElemsAlloc + ";");
		stringBuilder.Append(cbElem + ";");
		foreach (PointF coord in Coords)
		{
			stringBuilder.Append(coord.X + ";" + coord.Y + ";");
		}
		return stringBuilder;
	}

	private void readArrayData()
	{
		MemoryStream memoryStream = new MemoryStream((m_prop[m_key] as FOPTEComplex).Value);
		new StreamReader(memoryStream);
		byte[] array = new byte[2];
		memoryStream.Read(array, 0, 2);
		m_nelems = BitConverter.ToUInt16(array, 0);
		memoryStream.Read(array, 0, 2);
		m_nelemsalloc = BitConverter.ToUInt16(array, 0);
		memoryStream.Read(array, 0, 2);
		m_cbelem = BitConverter.ToUInt16(array, 0);
		for (int i = 0; i < (int)m_nelems; i++)
		{
			array = new byte[m_cbelem];
			memoryStream.Read(array, 0, (int)m_cbelem);
			int num = BitConverter.ToInt32(array, 0);
			int num2 = BitConverter.ToInt32(array, 4);
			m_coords.Add(new PointF(num, num2));
		}
		memoryStream.Dispose();
	}
}
