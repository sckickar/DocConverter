using System;
using System.Collections.Generic;
using System.IO;
using DocGen.DocIO.DLS;

namespace DocGen.DocIO.ReaderWriter.DataStreamParser.Escher;

internal class msofbtRGFOPTE : DocIOSortedList<int, FOPTEBase>
{
	public const int DEF_TXID = 128;

	public const uint DEF_LINE_WIDTH_PT = 12700u;

	public const float DEF_LINE_WIDTH = 0.75f;

	public const uint DEF_COLOR_EMPTY = 4278190080u;

	public const uint DEF_NO_LINE = 524288u;

	public const uint DEF_COLOR_FILL = 1048592u;

	public const uint DEF_NO_COLOR_FILL = 1048576u;

	public const uint DEF_BEHIND_DOC = 2097184u;

	public const uint DEF_NOT_BEHIND_DOC = 2097152u;

	public const uint DEF_FIT_TEXT_TO_SHAPE = 131072u;

	public const uint DEF_BACKGROND_SHAPE = 65537u;

	private int m_prevPid;

	internal void Add(FOPTEBase fopteBase)
	{
		Add(fopteBase.Id, fopteBase);
	}

	internal int Write(Stream stream)
	{
		int num = (int)stream.Position;
		foreach (FOPTEBase value in Values)
		{
			if (value.Id > 10000)
			{
				FOPTEBase fOPTEBase = value.Clone();
				fOPTEBase.Id -= 10000;
				fOPTEBase.Write(stream);
			}
			else
			{
				value.Write(stream);
			}
		}
		foreach (FOPTEBase value2 in Values)
		{
			if (value2 is FOPTEComplex fOPTEComplex)
			{
				fOPTEComplex.WriteData(stream);
			}
		}
		return (int)(stream.Position - num);
	}

	internal void Read(Stream stream, int length)
	{
		long num = stream.Position + length;
		while (stream.Position < num)
		{
			byte[] array = new byte[4];
			stream.Read(array, 0, 2);
			short num2 = BitConverter.ToInt16(array, 0);
			int num3 = num2 & 0x3FFF;
			bool isBid = (num2 & 0x4000) != 0;
			bool num4 = (num2 & 0x8000) != 0;
			stream.Read(array, 0, 4);
			uint num5 = BitConverter.ToUInt32(array, 0);
			if (num4)
			{
				Add(num3, new FOPTEComplex(num3, isBid, (int)num5));
				num -= num5;
				m_prevPid = num3;
				continue;
			}
			if (num3 < m_prevPid)
			{
				m_prevPid = num3;
				num3 += 10000;
			}
			else
			{
				m_prevPid = num3;
			}
			Add(num3, new FOPTEBid(num3, isBid, num5));
		}
		foreach (FOPTEBase value in Values)
		{
			if (value is FOPTEComplex fOPTEComplex)
			{
				fOPTEComplex.ReadData(stream);
			}
		}
	}

	internal List<FOPTEBase> GetPostProps()
	{
		List<FOPTEBase> list = new List<FOPTEBase>();
		foreach (FOPTEBase value in Values)
		{
			if (value.Id < 118 && value.Id != 4)
			{
				list.Add(value);
			}
		}
		return list;
	}
}
