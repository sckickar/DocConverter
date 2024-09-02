namespace DocGen.Pdf;

internal class ByteToBitInput
{
	internal ByteInputBuffer in_Renamed;

	internal int bbuf;

	internal int bpos = -1;

	public ByteToBitInput(ByteInputBuffer in_Renamed)
	{
		this.in_Renamed = in_Renamed;
	}

	public int readBit()
	{
		if (bpos < 0)
		{
			if ((bbuf & 0xFF) != 255)
			{
				bbuf = in_Renamed.read();
				bpos = 7;
			}
			else
			{
				bbuf = in_Renamed.read();
				bpos = 6;
			}
		}
		return (bbuf >> bpos--) & 1;
	}

	public virtual bool checkBytePadding()
	{
		if (bpos < 0 && (bbuf & 0xFF) == 255)
		{
			bbuf = in_Renamed.read();
			bpos = 6;
		}
		if (bpos >= 0 && (bbuf & ((1 << bpos + 1) - 1)) != 85 >> 7 - bpos)
		{
			return true;
		}
		if (bbuf != -1)
		{
			if (bbuf == 255 && bpos == 0)
			{
				if ((in_Renamed.read() & 0xFF) >= 128)
				{
					return true;
				}
			}
			else if (in_Renamed.read() != -1)
			{
				return true;
			}
		}
		return false;
	}

	internal void flush()
	{
		bbuf = 0;
		bpos = -1;
	}

	internal void setByteArray(byte[] buf, int off, int len)
	{
		in_Renamed.setByteArray(buf, off, len);
		bbuf = 0;
		bpos = -1;
	}
}
