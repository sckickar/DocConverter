using System;
using System.Collections.Generic;
using System.IO;
using DocGen.Drawing;

namespace DocGen.OfficeChart.Implementation.Shapes;

internal class GradientStops : List<GradientStopImpl>
{
	internal const int MaxPosition = 100000;

	private int m_iAngle;

	private GradientType m_gradientType;

	private Rectangle m_fillToRect;

	private Rectangle m_tileRect;

	public int Angle
	{
		get
		{
			return m_iAngle;
		}
		set
		{
			m_iAngle = value;
		}
	}

	public GradientType GradientType
	{
		get
		{
			return m_gradientType;
		}
		set
		{
			m_gradientType = value;
		}
	}

	public Rectangle FillToRect
	{
		get
		{
			return m_fillToRect;
		}
		set
		{
			m_fillToRect = value;
		}
	}

	public Rectangle TileRect
	{
		get
		{
			return m_tileRect;
		}
		set
		{
			m_tileRect = value;
		}
	}

	public bool IsDoubled
	{
		get
		{
			int count = base.Count;
			bool result = true;
			if (count <= 2)
			{
				result = false;
			}
			else
			{
				int num = 0;
				int num2 = count - 1;
				while (num <= num2)
				{
					GradientStopImpl gradientStopImpl = base[num];
					GradientStopImpl gradientStopImpl2 = base[num2];
					if (gradientStopImpl.ColorObject != gradientStopImpl2.ColorObject || gradientStopImpl.Position != 100000 - gradientStopImpl2.Position)
					{
						result = false;
						break;
					}
					num++;
					num2--;
				}
			}
			return result;
		}
	}

	public GradientStops()
	{
	}

	public GradientStops(byte[] data)
	{
		Parse(data);
	}

	public void Serialize(Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		byte[] bytes = BitConverter.GetBytes(base.Count);
		stream.Write(bytes, 0, bytes.Length);
		bytes = BitConverter.GetBytes(m_iAngle);
		stream.Write(bytes, 0, bytes.Length);
		stream.WriteByte((byte)m_gradientType);
		bytes = BitConverter.GetBytes(m_fillToRect.Left);
		stream.Write(bytes, 0, bytes.Length);
		bytes = BitConverter.GetBytes(m_fillToRect.Top);
		stream.Write(bytes, 0, bytes.Length);
		bytes = BitConverter.GetBytes(m_fillToRect.Right);
		stream.Write(bytes, 0, bytes.Length);
		bytes = BitConverter.GetBytes(m_fillToRect.Bottom);
		stream.Write(bytes, 0, bytes.Length);
		int i = 0;
		for (int count = base.Count; i < count; i++)
		{
			base[i].Serialize(stream);
		}
	}

	private void Parse(byte[] data)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		int num = 0;
		_ = data.Length;
		int num2 = BitConverter.ToInt32(data, num);
		num += 4;
		m_iAngle = BitConverter.ToInt32(data, num);
		num += 4;
		m_gradientType = (GradientType)data[num];
		num++;
		int left = BitConverter.ToInt32(data, num);
		num += 4;
		int top = BitConverter.ToInt32(data, num);
		num += 4;
		int right = BitConverter.ToInt32(data, num);
		num += 4;
		int bottom = BitConverter.ToInt32(data, num);
		num += 4;
		m_fillToRect = Rectangle.FromLTRB(left, top, right, bottom);
		for (int i = 0; i < num2; i++)
		{
			GradientStopImpl item = new GradientStopImpl(data, num);
			num += 12;
			Add(item);
		}
	}

	public void DoubleGradientStops()
	{
		int count = base.Count;
		if (count != 0)
		{
			GradientStopImpl gradientStopImpl = base[count - 1];
			int position = gradientStopImpl.Position;
			int num2 = (gradientStopImpl.Position = position >> 1);
			if (position != 100000)
			{
				gradientStopImpl = gradientStopImpl.Clone();
				gradientStopImpl.Position = 100000 - num2;
				Add(gradientStopImpl);
			}
			for (int num3 = count - 2; num3 >= 0; num3--)
			{
				GradientStopImpl gradientStopImpl2 = base[num3];
				position = (gradientStopImpl2.Position >>= 1);
				gradientStopImpl2 = gradientStopImpl2.Clone();
				gradientStopImpl2.Position = 100000 - position;
				Add(gradientStopImpl2);
			}
		}
	}

	public void InvertGradientStops()
	{
		int count = base.Count;
		if (count != 0)
		{
			for (int i = 0; i < count; i++)
			{
				GradientStopImpl gradientStopImpl = base[i];
				int position = gradientStopImpl.Position;
				gradientStopImpl.Position = 100000 - position;
			}
		}
	}

	public GradientStops ShrinkGradientStops()
	{
		GradientStops gradientStops = new GradientStops();
		gradientStops.m_iAngle = m_iAngle;
		gradientStops.m_gradientType = m_gradientType;
		gradientStops.m_fillToRect = m_fillToRect;
		int i = 0;
		for (int count = base.Count; i < count; i++)
		{
			GradientStopImpl gradientStopImpl = base[i];
			if (gradientStopImpl.Position > 50000)
			{
				break;
			}
			gradientStopImpl = gradientStopImpl.Clone();
			gradientStopImpl.Position <<= 1;
			gradientStops.Add(gradientStopImpl);
		}
		return gradientStops;
	}

	public GradientStops Clone()
	{
		GradientStops gradientStops = new GradientStops();
		gradientStops.m_iAngle = m_iAngle;
		gradientStops.m_gradientType = m_gradientType;
		gradientStops.m_fillToRect = m_fillToRect;
		gradientStops.m_tileRect = m_tileRect;
		int i = 0;
		for (int count = base.Count; i < count; i++)
		{
			gradientStops.Add(base[i].Clone());
		}
		return gradientStops;
	}

	internal bool EqualColors(GradientStops gradientStops)
	{
		if (gradientStops == null)
		{
			return false;
		}
		bool result = false;
		int count = base.Count;
		if (gradientStops.Count == count)
		{
			result = true;
			for (int i = 0; i < count; i++)
			{
				GradientStopImpl gradientStopImpl = base[i];
				GradientStopImpl stop = gradientStops[i];
				if (!gradientStopImpl.EqualsWithoutTransparency(stop))
				{
					result = false;
					break;
				}
			}
		}
		return result;
	}

	internal void Dispose()
	{
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			enumerator.Current.Dispose();
		}
	}
}
