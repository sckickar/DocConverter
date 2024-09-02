using System;

namespace DocGen.DocIO.ReaderWriter.Biff_Records;

[CLSCompliant(false)]
internal class Spacings
{
	private short m_left = -1;

	private short m_right = -1;

	private short m_top = -1;

	private short m_bottom = -1;

	private byte m_cellNumber;

	internal short Left
	{
		get
		{
			return m_left;
		}
		set
		{
			if (value < 0 || value > 31680)
			{
				m_left = 0;
			}
			else
			{
				m_left = value;
			}
		}
	}

	internal short Right
	{
		get
		{
			return m_right;
		}
		set
		{
			if (value < 0 || value > 31680)
			{
				m_right = 0;
			}
			else
			{
				m_right = value;
			}
		}
	}

	internal short Top
	{
		get
		{
			return m_top;
		}
		set
		{
			if (value < 0 || value > 31680)
			{
				m_top = 0;
			}
			else
			{
				m_top = value;
			}
		}
	}

	internal short Bottom
	{
		get
		{
			return m_bottom;
		}
		set
		{
			if (value < 0 || value > 31680)
			{
				m_bottom = 0;
			}
			else
			{
				m_bottom = value;
			}
		}
	}

	internal int CellNumber
	{
		get
		{
			return m_cellNumber;
		}
		set
		{
			m_cellNumber = (byte)value;
		}
	}

	internal bool IsEmpty
	{
		get
		{
			if (m_left == -1 && m_top == -1 && m_bottom == -1)
			{
				return m_right == -1;
			}
			return false;
		}
	}

	internal Spacings()
	{
	}

	internal Spacings(SinglePropertyModifierRecord sprm)
	{
		Parse(sprm);
	}

	internal void Parse(SinglePropertyModifierRecord sprm)
	{
		m_cellNumber = sprm.ByteArray[0];
		byte b = sprm.ByteArray[2];
		if (b > 15)
		{
			return;
		}
		byte b2 = sprm.ByteArray[3];
		if (b2 == 3 || b2 == 0)
		{
			short num = BitConverter.ToInt16(sprm.ByteArray, 4);
			if (((uint)b & (true ? 1u : 0u)) != 0)
			{
				Top = num;
			}
			if ((b & 2u) != 0)
			{
				Left = num;
			}
			if ((b & 4u) != 0)
			{
				Bottom = num;
			}
			if ((b & 8u) != 0)
			{
				Right = num;
			}
		}
	}

	internal void Save(SinglePropertyModifierArray modifierArray, int options, int cellNumber)
	{
		if (m_top != -1)
		{
			modifierArray.Add(SaveSingleRecord(1, m_top, options));
		}
		if (m_left != -1)
		{
			modifierArray.Add(SaveSingleRecord(2, m_left, options));
		}
		if (m_bottom != -1)
		{
			modifierArray.Add(SaveSingleRecord(4, m_bottom, options));
		}
		if (m_right != -1)
		{
			modifierArray.Add(SaveSingleRecord(8, m_right, options));
		}
	}

	internal Spacings Clone()
	{
		return new Spacings
		{
			m_bottom = Bottom,
			m_cellNumber = (byte)CellNumber,
			m_left = Left,
			m_right = Right,
			m_top = Top
		};
	}

	private SinglePropertyModifierRecord SaveSingleRecord(byte type, short dist, int options)
	{
		byte[] array = new byte[6]
		{
			m_cellNumber,
			(byte)(m_cellNumber + 1),
			type,
			3,
			0,
			0
		};
		BitConverter.GetBytes(dist).CopyTo(array, 4);
		return new SinglePropertyModifierRecord(options)
		{
			ByteArray = array
		};
	}
}
