using System.Collections.Generic;
using System.IO;

namespace DocGen.Pdf;

internal class JPXFormatReader
{
	private JPXRandomAccessStream in_Renamed;

	private List<object> codeStreamPos;

	private List<object> codeStreamLength;

	public bool JP2FFUsed;

	public virtual long[] CodeStreamPos
	{
		get
		{
			int count = codeStreamPos.Count;
			long[] array = new long[count];
			for (int i = 0; i < count; i++)
			{
				array[i] = (int)codeStreamPos[i];
			}
			return array;
		}
	}

	public virtual int FirstCodeStreamPos => (int)codeStreamPos[0];

	public virtual int FirstCodeStreamLength => (int)codeStreamLength[0];

	internal JPXFormatReader(JPXRandomAccessStream in_Renamed)
	{
		this.in_Renamed = in_Renamed;
	}

	public virtual void readFileFormat()
	{
		long longLength = 0L;
		bool flag = false;
		bool flag2 = false;
		try
		{
			if (in_Renamed.readInt() != 12 || in_Renamed.readInt() != 1783636000 || in_Renamed.readInt() != 218793738)
			{
				in_Renamed.seek(0);
				if (in_Renamed.readShort() != -177)
				{
					JP2FFUsed = false;
				}
				in_Renamed.seek(0);
				return;
			}
			JP2FFUsed = true;
			if (!readFileTypeBox())
			{
			}
			while (!flag2)
			{
				int pos = in_Renamed.Pos;
				int num = in_Renamed.readInt();
				if (pos + num == in_Renamed.length())
				{
					flag2 = true;
				}
				int num2 = in_Renamed.readInt();
				switch (num)
				{
				case 0:
					flag2 = true;
					num = in_Renamed.length() - in_Renamed.Pos;
					break;
				case 1:
					longLength = in_Renamed.readLong();
					throw new IOException("File too long.");
				default:
					longLength = 0L;
					break;
				}
				switch (num2)
				{
				case 1785737827:
					readContiguousCodeStreamBox(pos, num, longLength);
					break;
				case 1785737832:
					if (flag)
					{
						readJP2HeaderBox(pos, num, longLength);
					}
					flag = true;
					break;
				}
				if (!flag2)
				{
					in_Renamed.seek(pos + num);
				}
			}
		}
		catch (EndOfStreamException)
		{
		}
		_ = codeStreamPos.Count;
	}

	public virtual bool readFileTypeBox()
	{
		bool flag = false;
		_ = in_Renamed.Pos;
		int num = in_Renamed.readInt();
		if (in_Renamed.readInt() != 1718909296)
		{
			return false;
		}
		if (num == 1)
		{
			in_Renamed.readLong();
			throw new IOException("File too long.");
		}
		in_Renamed.readInt();
		in_Renamed.readInt();
		for (int num2 = (num - 16) / 4; num2 > 0; num2--)
		{
			if (in_Renamed.readInt() == 1785737760)
			{
				flag = true;
			}
		}
		if (!flag)
		{
			return false;
		}
		return true;
	}

	public virtual bool readJP2HeaderBox(long pos, int length, long longLength)
	{
		return true;
	}

	public virtual bool readContiguousCodeStreamBox(long pos, int length, long longLength)
	{
		int pos2 = in_Renamed.Pos;
		if (codeStreamPos == null)
		{
			codeStreamPos = new List<object>(10);
		}
		codeStreamPos.Add(pos2);
		if (codeStreamLength == null)
		{
			codeStreamLength = new List<object>(10);
		}
		codeStreamLength.Add(length);
		return true;
	}
}
