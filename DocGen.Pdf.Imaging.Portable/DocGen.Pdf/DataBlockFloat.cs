using System;

namespace DocGen.Pdf;

internal class DataBlockFloat : DataBlock
{
	private float[] data;

	public override int DataType => 4;

	public override object Data
	{
		get
		{
			return data;
		}
		set
		{
			data = (float[])value;
		}
	}

	public virtual float[] DataFloat
	{
		get
		{
			return data;
		}
		set
		{
			data = value;
		}
	}

	public DataBlockFloat()
	{
	}

	public DataBlockFloat(int ulx, int uly, int w, int h)
	{
		base.ulx = ulx;
		base.uly = uly;
		base.w = w;
		base.h = h;
		offset = 0;
		scanw = w;
		data = new float[w * h];
	}

	public DataBlockFloat(DataBlockFloat src)
	{
		ulx = src.ulx;
		uly = src.uly;
		w = src.w;
		h = src.h;
		offset = 0;
		scanw = w;
		data = new float[w * h];
		for (int i = 0; i < h; i++)
		{
			Array.Copy(src.data, i * src.scanw, data, i * scanw, w);
		}
	}

	public override string ToString()
	{
		string text = base.ToString();
		if (data != null)
		{
			text = text + ",data=" + data.Length + " bytes";
		}
		return text;
	}
}
