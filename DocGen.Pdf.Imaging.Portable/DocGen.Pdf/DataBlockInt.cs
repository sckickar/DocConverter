using System;

namespace DocGen.Pdf;

internal class DataBlockInt : DataBlock
{
	public int[] data_array;

	public override int DataType => 3;

	public override object Data
	{
		get
		{
			return data_array;
		}
		set
		{
			data_array = (int[])value;
		}
	}

	public virtual int[] DataInt
	{
		get
		{
			return data_array;
		}
		set
		{
			data_array = value;
		}
	}

	public DataBlockInt()
	{
	}

	public DataBlockInt(int ulx, int uly, int w, int h)
	{
		base.ulx = ulx;
		base.uly = uly;
		base.w = w;
		base.h = h;
		offset = 0;
		scanw = w;
		data_array = new int[w * h];
	}

	public DataBlockInt(DataBlockInt src)
	{
		ulx = src.ulx;
		uly = src.uly;
		w = src.w;
		h = src.h;
		offset = 0;
		scanw = w;
		data_array = new int[w * h];
		for (int i = 0; i < h; i++)
		{
			Array.Copy(src.data_array, i * src.scanw, data_array, i * scanw, w);
		}
	}

	public override string ToString()
	{
		string text = base.ToString();
		if (data_array != null)
		{
			text = text + ",data=" + data_array.Length + " bytes";
		}
		return text;
	}
}
