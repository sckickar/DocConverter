namespace DocGen.Pdf;

internal class ArithmeticDecoder
{
	private Jbig2StreamReader reader;

	private ArithmeticDecoderStats m_genericRegionStats;

	private ArithmeticDecoderStats m_refinementRegionStats;

	private ArithmeticDecoderStats m_iadhStats;

	private ArithmeticDecoderStats m_iadwStats;

	private ArithmeticDecoderStats m_iaexStats;

	private ArithmeticDecoderStats m_iaaiStats;

	private ArithmeticDecoderStats m_iadtStats;

	private ArithmeticDecoderStats m_iaitStats;

	private ArithmeticDecoderStats m_iafsStats;

	private ArithmeticDecoderStats m_iadsStats;

	private ArithmeticDecoderStats m_iardxStats;

	private ArithmeticDecoderStats m_iardyStats;

	private ArithmeticDecoderStats m_iardwStats;

	private ArithmeticDecoderStats m_iardhStats;

	private ArithmeticDecoderStats m_iariStats;

	private ArithmeticDecoderStats m_iaidStats;

	private BitOperation m_bitOperation = new BitOperation();

	private int[] m_contextSize = new int[4] { 16, 13, 10, 10 };

	internal int[] referredToContextSize = new int[2] { 13, 10 };

	private long m_buffer0;

	private long m_buffer1;

	private long c;

	private long a;

	private long m_previous;

	private int m_counter;

	private int[] qeTable = new int[47]
	{
		1442906112, 872480768, 402718720, 180420608, 86048768, 35717120, 1442906112, 1409351680, 1208025088, 939589632,
		805371904, 604045312, 469827584, 369164288, 1442906112, 1409351680, 1359020032, 1208025088, 939589632, 872480768,
		805371904, 671154176, 604045312, 570490880, 469827584, 402718720, 369164288, 335609856, 302055424, 285278208,
		180420608, 163643392, 144769024, 86048768, 71368704, 44105728, 35717120, 21037056, 17891328, 8716288,
		4784128, 2424832, 1376256, 589824, 327680, 65536, 1442906112
	};

	private int[] nmpsTable = new int[47]
	{
		1, 2, 3, 4, 5, 38, 7, 8, 9, 10,
		11, 12, 13, 29, 15, 16, 17, 18, 19, 20,
		21, 22, 23, 24, 25, 26, 27, 28, 29, 30,
		31, 32, 33, 34, 35, 36, 37, 38, 39, 40,
		41, 42, 43, 44, 45, 45, 46
	};

	private int[] nlpsTable = new int[47]
	{
		1, 6, 9, 12, 29, 33, 6, 14, 14, 14,
		17, 18, 20, 21, 14, 14, 15, 16, 17, 18,
		19, 19, 20, 21, 22, 23, 24, 25, 26, 27,
		28, 29, 30, 31, 32, 33, 34, 35, 36, 37,
		38, 39, 40, 41, 42, 43, 46
	};

	private int[] switchTable = new int[47]
	{
		1, 0, 0, 0, 0, 0, 1, 0, 0, 0,
		0, 0, 0, 0, 1, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0
	};

	internal ArithmeticDecoderStats GenericRegionStats => m_genericRegionStats;

	internal ArithmeticDecoderStats RefinementRegionStats => m_refinementRegionStats;

	internal ArithmeticDecoderStats IadhStats => m_iadhStats;

	internal ArithmeticDecoderStats IadwStats => m_iadwStats;

	internal ArithmeticDecoderStats IaexStats => m_iaexStats;

	internal ArithmeticDecoderStats IaaiStats => m_iaaiStats;

	internal ArithmeticDecoderStats IadtStats => m_iadtStats;

	internal ArithmeticDecoderStats IaitStats => m_iaitStats;

	internal ArithmeticDecoderStats IafsStats => m_iafsStats;

	internal ArithmeticDecoderStats IadsStats => m_iadsStats;

	internal ArithmeticDecoderStats IardxStats => m_iardxStats;

	internal ArithmeticDecoderStats IardyStats => m_iardyStats;

	internal ArithmeticDecoderStats IardwStats => m_iardwStats;

	internal ArithmeticDecoderStats IardhStats => m_iardhStats;

	internal ArithmeticDecoderStats IariStats => m_iariStats;

	internal ArithmeticDecoderStats IaidStats => m_iaidStats;

	private ArithmeticDecoder()
	{
	}

	internal ArithmeticDecoder(Jbig2StreamReader reader)
	{
		this.reader = reader;
		m_genericRegionStats = new ArithmeticDecoderStats(2);
		m_refinementRegionStats = new ArithmeticDecoderStats(2);
		m_iadhStats = new ArithmeticDecoderStats(512);
		m_iadwStats = new ArithmeticDecoderStats(512);
		m_iaexStats = new ArithmeticDecoderStats(512);
		m_iaaiStats = new ArithmeticDecoderStats(512);
		m_iadtStats = new ArithmeticDecoderStats(512);
		m_iaitStats = new ArithmeticDecoderStats(512);
		m_iafsStats = new ArithmeticDecoderStats(512);
		m_iadsStats = new ArithmeticDecoderStats(512);
		m_iardxStats = new ArithmeticDecoderStats(512);
		m_iardyStats = new ArithmeticDecoderStats(512);
		m_iardwStats = new ArithmeticDecoderStats(512);
		m_iardhStats = new ArithmeticDecoderStats(512);
		m_iariStats = new ArithmeticDecoderStats(512);
		m_iaidStats = new ArithmeticDecoderStats(2);
	}

	internal void ResetIntegerStats(int symbolCodeLength)
	{
		m_iadhStats.reset();
		m_iadwStats.reset();
		m_iaexStats.reset();
		m_iaaiStats.reset();
		m_iadtStats.reset();
		m_iaitStats.reset();
		m_iafsStats.reset();
		m_iadsStats.reset();
		m_iardxStats.reset();
		m_iardyStats.reset();
		m_iardwStats.reset();
		m_iardhStats.reset();
		m_iariStats.reset();
		if (m_iaidStats.ContextSize == 1 << symbolCodeLength + 1)
		{
			m_iaidStats.reset();
		}
		else
		{
			m_iaidStats = new ArithmeticDecoderStats(1 << symbolCodeLength + 1);
		}
	}

	internal void ResetGenericStats(int template, ArithmeticDecoderStats previousStats)
	{
		int num = m_contextSize[template];
		if (previousStats != null && previousStats.ContextSize == num)
		{
			if (m_genericRegionStats.ContextSize == num)
			{
				m_genericRegionStats.overwrite(previousStats);
			}
			else
			{
				m_genericRegionStats = previousStats.copy();
			}
		}
		else if (m_genericRegionStats.ContextSize == num)
		{
			m_genericRegionStats.reset();
		}
		else
		{
			m_genericRegionStats = new ArithmeticDecoderStats(1 << num);
		}
	}

	internal void ResetRefinementStats(int template, ArithmeticDecoderStats previousStats)
	{
		int num = referredToContextSize[template];
		if (previousStats != null && previousStats.ContextSize == num)
		{
			if (m_refinementRegionStats.ContextSize == num)
			{
				m_refinementRegionStats.overwrite(previousStats);
			}
			else
			{
				m_refinementRegionStats = previousStats.copy();
			}
		}
		else if (m_refinementRegionStats.ContextSize == num)
		{
			m_refinementRegionStats.reset();
		}
		else
		{
			m_refinementRegionStats = new ArithmeticDecoderStats(1 << num);
		}
	}

	internal void Start()
	{
		m_buffer0 = reader.ReadByte();
		m_buffer1 = reader.ReadByte();
		c = m_bitOperation.Bit32Shift(m_buffer0 ^ 0xFF, 16, 0);
		ReadByte();
		c = m_bitOperation.Bit32Shift(c, 7, 0);
		m_counter -= 7;
		a = 2147483648L;
	}

	internal DecodeIntResult DecodeInt(ArithmeticDecoderStats stats)
	{
		m_previous = 1L;
		int num = DecodeIntBit(stats);
		long num2;
		if (DecodeIntBit(stats) != 0)
		{
			if (DecodeIntBit(stats) != 0)
			{
				if (DecodeIntBit(stats) != 0)
				{
					if (DecodeIntBit(stats) != 0)
					{
						if (DecodeIntBit(stats) != 0)
						{
							num2 = 0L;
							for (int i = 0; i < 32; i++)
							{
								num2 = m_bitOperation.Bit32Shift(num2, 1, 0) | DecodeIntBit(stats);
							}
							num2 += 4436;
						}
						else
						{
							num2 = 0L;
							for (int j = 0; j < 12; j++)
							{
								num2 = m_bitOperation.Bit32Shift(num2, 1, 0) | DecodeIntBit(stats);
							}
							num2 += 340;
						}
					}
					else
					{
						num2 = 0L;
						for (int k = 0; k < 8; k++)
						{
							num2 = m_bitOperation.Bit32Shift(num2, 1, 0) | DecodeIntBit(stats);
						}
						num2 += 84;
					}
				}
				else
				{
					num2 = 0L;
					for (int l = 0; l < 6; l++)
					{
						num2 = m_bitOperation.Bit32Shift(num2, 1, 0) | DecodeIntBit(stats);
					}
					num2 += 20;
				}
			}
			else
			{
				num2 = DecodeIntBit(stats);
				num2 = m_bitOperation.Bit32Shift(num2, 1, 0) | DecodeIntBit(stats);
				num2 = m_bitOperation.Bit32Shift(num2, 1, 0) | DecodeIntBit(stats);
				num2 = m_bitOperation.Bit32Shift(num2, 1, 0) | DecodeIntBit(stats);
				num2 += 4;
			}
		}
		else
		{
			num2 = DecodeIntBit(stats);
			num2 = m_bitOperation.Bit32Shift(num2, 1, 0) | DecodeIntBit(stats);
		}
		int intResult;
		if (num != 0)
		{
			if (num2 == 0L)
			{
				return new DecodeIntResult((int)num2, booleanResult: false);
			}
			intResult = (int)(-num2);
		}
		else
		{
			intResult = (int)num2;
		}
		return new DecodeIntResult(intResult, booleanResult: true);
	}

	internal long DecodeIAID(long codeLen, ArithmeticDecoderStats stats)
	{
		m_previous = 1L;
		for (long num = 0L; num < codeLen; num++)
		{
			int num2 = DecodeBit(m_previous, stats);
			m_previous = m_bitOperation.Bit32Shift(m_previous, 1, 0) | num2;
		}
		return m_previous - (1 << (int)codeLen);
	}

	internal int DecodeBit(long context, ArithmeticDecoderStats stats)
	{
		int num = m_bitOperation.Bit8Shift(stats.getContextCodingTableValue((int)context), 1, 1);
		int num2 = stats.getContextCodingTableValue((int)context) & 1;
		int num3 = qeTable[num];
		a -= num3;
		int result;
		if (c < a)
		{
			if ((a & 0x80000000u) != 0L)
			{
				result = num2;
			}
			else
			{
				if (a < num3)
				{
					result = 1 - num2;
					if (switchTable[num] != 0)
					{
						stats.setContextCodingTableValue((int)context, (nlpsTable[num] << 1) | (1 - num2));
					}
					else
					{
						stats.setContextCodingTableValue((int)context, (nlpsTable[num] << 1) | num2);
					}
				}
				else
				{
					result = num2;
					stats.setContextCodingTableValue((int)context, (nmpsTable[num] << 1) | num2);
				}
				do
				{
					if (m_counter == 0)
					{
						ReadByte();
					}
					a = m_bitOperation.Bit32Shift(a, 1, 0);
					c = m_bitOperation.Bit32Shift(c, 1, 0);
					m_counter--;
				}
				while ((a & 0x80000000u) == 0L);
			}
		}
		else
		{
			c -= a;
			if (a < num3)
			{
				result = num2;
				stats.setContextCodingTableValue((int)context, (nmpsTable[num] << 1) | num2);
			}
			else
			{
				result = 1 - num2;
				if (switchTable[num] != 0)
				{
					stats.setContextCodingTableValue((int)context, (nlpsTable[num] << 1) | (1 - num2));
				}
				else
				{
					stats.setContextCodingTableValue((int)context, (nlpsTable[num] << 1) | num2);
				}
			}
			a = num3;
			do
			{
				if (m_counter == 0)
				{
					ReadByte();
				}
				a = m_bitOperation.Bit32Shift(a, 1, 0);
				c = m_bitOperation.Bit32Shift(c, 1, 0);
				m_counter--;
			}
			while ((a & 0x80000000u) == 0L);
		}
		return result;
	}

	private void ReadByte()
	{
		if (m_buffer0 == 255)
		{
			if (m_buffer1 > 143)
			{
				m_counter = 8;
				return;
			}
			m_buffer0 = m_buffer1;
			m_buffer1 = reader.ReadByte();
			c = c + 65024 - m_bitOperation.Bit32Shift(m_buffer0, 9, 0);
			m_counter = 7;
		}
		else
		{
			m_buffer0 = m_buffer1;
			m_buffer1 = reader.ReadByte();
			c = c + 65280 - m_bitOperation.Bit32Shift(m_buffer0, 8, 0);
			m_counter = 8;
		}
	}

	private int DecodeIntBit(ArithmeticDecoderStats stats)
	{
		int num = DecodeBit(m_previous, stats);
		if (m_previous < 256)
		{
			m_previous = m_bitOperation.Bit32Shift(m_previous, 1, 0) | num;
		}
		else
		{
			m_previous = ((m_bitOperation.Bit32Shift(m_previous, 1, 0) | num) & 0x1FF) | 0x100;
		}
		return num;
	}
}
