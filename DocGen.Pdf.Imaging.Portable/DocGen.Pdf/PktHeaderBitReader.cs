using System.IO;

namespace DocGen.Pdf;

internal class PktHeaderBitReader
{
	internal JPXRandomAccessStream in_Renamed;

	internal MemoryStream bais;

	internal bool usebais;

	internal int bbuf;

	internal int bpos;

	internal int nextbbuf;

	internal PktHeaderBitReader(JPXRandomAccessStream in_Renamed)
	{
		this.in_Renamed = in_Renamed;
		usebais = false;
	}

	internal PktHeaderBitReader(MemoryStream bais)
	{
		this.bais = bais;
		usebais = true;
	}

	internal int readBit()
	{
		if (bpos == 0)
		{
			if (bbuf != 255)
			{
				if (usebais)
				{
					bbuf = bais.ReadByte();
				}
				else
				{
					bbuf = in_Renamed.read();
				}
				bpos = 8;
				if (bbuf == 255)
				{
					if (usebais)
					{
						nextbbuf = bais.ReadByte();
					}
					else
					{
						nextbbuf = in_Renamed.read();
					}
				}
			}
			else
			{
				bbuf = nextbbuf;
				bpos = 7;
			}
		}
		return (bbuf >> --bpos) & 1;
	}

	internal int readBits(int n)
	{
		if (n <= bpos)
		{
			return (bbuf >> (bpos -= n)) & ((1 << n) - 1);
		}
		int num = 0;
		do
		{
			num <<= bpos;
			n -= bpos;
			num |= readBits(bpos);
			if (bbuf != 255)
			{
				if (usebais)
				{
					bbuf = bais.ReadByte();
				}
				else
				{
					bbuf = in_Renamed.read();
				}
				bpos = 8;
				if (bbuf == 255)
				{
					if (usebais)
					{
						nextbbuf = bais.ReadByte();
					}
					else
					{
						nextbbuf = in_Renamed.read();
					}
				}
			}
			else
			{
				bbuf = nextbbuf;
				bpos = 7;
			}
		}
		while (n > bpos);
		num <<= n;
		return num | ((bbuf >> (bpos -= n)) & ((1 << n) - 1));
	}

	internal virtual void sync()
	{
		bbuf = 0;
		bpos = 0;
	}

	internal virtual void setInput(JPXRandomAccessStream in_Renamed)
	{
		this.in_Renamed = in_Renamed;
		bbuf = 0;
		bpos = 0;
	}

	internal virtual void setInput(MemoryStream bais)
	{
		this.bais = bais;
		bbuf = 0;
		bpos = 0;
	}
}
