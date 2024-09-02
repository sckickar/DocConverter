using System;

namespace DocGen.Pdf;

internal abstract class DataBlock
{
	public const int TYPE_BYTE = 0;

	public const int TYPE_SHORT = 1;

	public const int TYPE_INT = 3;

	public const int TYPE_FLOAT = 4;

	public int ulx;

	public int uly;

	public int w;

	public int h;

	public int offset;

	public int scanw;

	public bool progressive;

	public abstract int DataType { get; }

	public abstract object Data { get; set; }

	public static int getSize(int type)
	{
		switch (type)
		{
		case 0:
			return 8;
		case 1:
			return 16;
		case 3:
		case 4:
			return 32;
		default:
			throw new ArgumentException();
		}
	}

	public override string ToString()
	{
		string text = "";
		switch (DataType)
		{
		case 0:
			text = "Unsigned Byte";
			break;
		case 1:
			text = "Short";
			break;
		case 3:
			text = "Integer";
			break;
		case 4:
			text = "Float";
			break;
		}
		return "DataBlk: upper-left(" + ulx + "," + uly + "), width=" + w + ", height=" + h + ", progressive=" + progressive + ", offset=" + offset + ", scanw=" + scanw + ", type=" + text;
	}
}
