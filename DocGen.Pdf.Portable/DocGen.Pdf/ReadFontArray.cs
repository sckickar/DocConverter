using System;
using System.Text;

namespace DocGen.Pdf;

internal class ReadFontArray
{
	private byte[] nextValue_2 = new byte[2];

	private byte[] nextValue_4 = new byte[4];

	private byte[] m_data;

	private int m_pointer;

	public int Pointer
	{
		get
		{
			return m_pointer;
		}
		set
		{
			m_pointer = value;
		}
	}

	public byte[] Data
	{
		get
		{
			return m_data;
		}
		set
		{
			m_data = value;
		}
	}

	public ReadFontArray(byte[] data, int pointer)
	{
		m_data = data;
		m_pointer = pointer;
	}

	public ReadFontArray(byte[] data)
	{
		m_data = data;
	}

	public byte getnextbyte()
	{
		byte result = Data[Pointer];
		Pointer++;
		return result;
	}

	public int getnextUint32()
	{
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < 4; i++)
		{
			num2 = ((Pointer < Data.Length) ? (Data[Pointer] & 0xFF) : 0);
			num += num2 << 8 * (3 - i);
			Pointer++;
		}
		return num;
	}

	public int getnextUint64()
	{
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < 8; i++)
		{
			num2 = Data[Pointer];
			if (num2 < 0)
			{
				num2 = 256 + num2;
			}
			num += num2 << 8 * (7 - i);
			Pointer++;
		}
		return num;
	}

	public string getnextUint32AsTag()
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < 4; i++)
		{
			char value = (char)Data[Pointer];
			stringBuilder.Append(value);
			Pointer++;
		}
		return stringBuilder.ToString();
	}

	public int getnextUint16()
	{
		int num = 0;
		for (int i = 0; i < 2; i++)
		{
			if (Data.Length != 0)
			{
				int num2 = Data[Pointer] & 0xFF;
				num += num2 << 8 * (1 - i);
			}
			Pointer++;
		}
		return num;
	}

	public ushort getnextUshort()
	{
		nextValue_2[0] = 0;
		nextValue_2[1] = 0;
		for (int num = 1; num >= 0; num--)
		{
			if (Data.Length != 0 && Pointer < Data.Length)
			{
				nextValue_2[num] = Data[Pointer];
				Pointer++;
			}
		}
		return BitConverter.ToUInt16(nextValue_2, 0);
	}

	public ulong getnextULong()
	{
		nextValue_4[0] = 0;
		nextValue_4[1] = 0;
		nextValue_4[2] = 0;
		nextValue_4[3] = 0;
		for (int num = 3; num >= 0; num--)
		{
			if (Data.Length != 0 && Pointer < Data.Length)
			{
				nextValue_4[num] = Data[Pointer];
				Pointer++;
			}
		}
		return BitConverter.ToUInt32(nextValue_4, 0);
	}

	public sbyte ReadChar()
	{
		return Read();
	}

	public long getLongDateTime()
	{
		byte[] array = new byte[8];
		for (int num = 7; num >= 0; num--)
		{
			if (Data.Length != 0 && Pointer < Data.Length)
			{
				array[num] = Data[Pointer];
				Pointer++;
			}
		}
		return BitConverter.ToInt64(array, 0);
	}

	public float getFixed()
	{
		return (float)getnextshort() + (float)(getnextUshort() / 65536);
	}

	public sbyte Read()
	{
		sbyte result = (sbyte)Data[Pointer];
		Pointer++;
		return result;
	}

	public uint getULong()
	{
		nextValue_4[0] = 0;
		nextValue_4[1] = 0;
		nextValue_4[2] = 0;
		nextValue_4[3] = 0;
		for (int num = 3; num >= 0; num--)
		{
			if (Data.Length != 0 && Pointer < Data.Length)
			{
				nextValue_4[num] = Data[Pointer];
				Pointer++;
			}
		}
		return BitConverter.ToUInt32(nextValue_4, 0);
	}

	public short getnextshort()
	{
		nextValue_2[0] = 0;
		nextValue_2[1] = 0;
		for (int num = 1; num >= 0; num--)
		{
			if (Data.Length != 0 && Pointer < Data.Length)
			{
				nextValue_2[num] = Data[Pointer];
				Pointer++;
			}
		}
		return BitConverter.ToInt16(nextValue_2, 0);
	}

	public float Get2Dot14()
	{
		return (float)getnextshort() / 16384f;
	}
}
