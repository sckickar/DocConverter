using System;
using System.Collections.Generic;
using System.IO;

namespace DocGen.DocIO.ReaderWriter.Biff_Records.Structures;

[CLSCompliant(false)]
internal class CharacterPropertiesPage : BaseWordRecord
{
	private const int DEF_FC_SIZE = 4;

	private uint[] m_arrFC;

	private CharacterPropertyException[] m_arrCHPX;

	internal uint[] FileCharPos => m_arrFC;

	internal CharacterPropertyException[] CharacterProperties => m_arrCHPX;

	internal int RunsCount
	{
		get
		{
			return m_arrCHPX.Length;
		}
		set
		{
			if (value < 0)
			{
				throw new ArgumentOutOfRangeException("RunsCount");
			}
			if (m_arrCHPX == null || value != m_arrCHPX.Length)
			{
				m_arrCHPX = new CharacterPropertyException[value];
				for (int i = 0; i < value; i++)
				{
					m_arrCHPX[i] = new CharacterPropertyException();
				}
				m_arrFC = new uint[value + 1];
			}
		}
	}

	internal override int Length => 512;

	internal CharacterPropertiesPage()
	{
	}

	internal CharacterPropertiesPage(FKPStructure structure)
	{
		Parse(structure);
	}

	internal override void Close()
	{
		base.Close();
		if (m_arrFC != null)
		{
			m_arrFC = null;
		}
		if (m_arrCHPX != null)
		{
			m_arrCHPX = null;
		}
	}

	private void Parse(FKPStructure structure)
	{
		byte[] pageData = structure.PageData;
		m_arrFC = new uint[structure.Count + 1];
		byte[] array = new byte[structure.Count];
		int num = (structure.Count + 1) * 4;
		Buffer.BlockCopy(pageData, 0, m_arrFC, 0, num);
		Array.Copy(pageData, num, array, 0, structure.Count);
		m_arrCHPX = new CharacterPropertyException[structure.Count];
		for (int i = 0; i < structure.Count; i++)
		{
			int iOffset = array[i] * 2;
			m_arrCHPX[i] = new CharacterPropertyException(pageData, iOffset);
		}
	}

	private FKPStructure Save()
	{
		FKPStructure fKPStructure = new FKPStructure();
		int runsCount = RunsCount;
		int num = 0;
		fKPStructure.Count = (byte)runsCount;
		int num2 = (runsCount + 1) * 4;
		Buffer.BlockCopy(m_arrFC, 0, fKPStructure.PageData, 0, num2);
		byte[] array = new byte[runsCount];
		int num3 = 511;
		for (int num4 = runsCount - 1; num4 >= 0; num4--)
		{
			if (m_arrCHPX[num4] != null)
			{
				int length = m_arrCHPX[num4].Length;
				int num5 = num3 - length;
				if (num5 % 2 != 0)
				{
					num5--;
				}
				num3 = num5;
				array[num4] = (byte)(num3 / 2);
				m_arrCHPX[num4].Save(fKPStructure.PageData, num3);
				num += length + 4 + 1;
			}
		}
		if (num > 512)
		{
			throw new Exception("FKP Chpx buffer overflow: " + num);
		}
		if (num3 < num2 + array.Length)
		{
			throw new Exception("FKP Chpx buffer overflow, ( chpx start at: " + num3 + "FC end: " + num2 + ", end of rgb: " + (num2 + array.Length));
		}
		array.CopyTo(fKPStructure.PageData, num2);
		return fKPStructure;
	}

	internal override int Save(byte[] arrData, int iOffset)
	{
		return Save().Save(arrData, iOffset);
	}

	internal int SaveToStream(BinaryWriter writer, Stream stream)
	{
		long position = stream.Position;
		Dictionary<int, byte> dictionary = new Dictionary<int, byte>();
		int runsCount = RunsCount;
		int num = 0;
		int num2 = (runsCount + 1) * 4;
		byte[] array = new byte[num2];
		Buffer.BlockCopy(m_arrFC, 0, array, 0, num2);
		stream.Write(array, 0, array.Length);
		byte[] array2 = new byte[runsCount];
		int num3 = 511;
		for (int num4 = runsCount - 1; num4 >= 0; num4--)
		{
			int ReturnIndex = -1;
			if (num4 < runsCount - 1 && IsChpxRepeats(num4, out ReturnIndex))
			{
				array2[num4] = dictionary[ReturnIndex];
			}
			else if (m_arrCHPX[num4] != null)
			{
				int length = m_arrCHPX[num4].Length;
				int num5 = num3 - length;
				if (num5 % 2 != 0)
				{
					num5--;
				}
				num3 = num5;
				array2[num4] = (byte)(num3 / 2);
				dictionary.Add(num4, array2[num4]);
				stream.Position = position + num3;
				m_arrCHPX[num4].Save(writer, stream, length);
				num += length + 4 + 1;
			}
		}
		if (num > 512)
		{
			throw new Exception("FKP Chpx buffer overflow: " + num);
		}
		long position2 = stream.Position;
		stream.Position = position + num2;
		stream.Write(array2, 0, array2.Length);
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		if (position2 > position + 511)
		{
			throw new Exception("chpx overflow");
		}
		stream.Position = position + 511;
		stream.WriteByte((byte)runsCount);
		return (int)stream.Position;
	}

	internal bool IsChpxRepeats(int CurrentIndex, out int ReturnIndex)
	{
		CharacterPropertyException ex = m_arrCHPX[CurrentIndex];
		bool result = false;
		ReturnIndex = -1;
		for (int num = m_arrCHPX.Length - 1; num > CurrentIndex; num--)
		{
			CharacterPropertyException ex2 = m_arrCHPX[num];
			ReturnIndex = num;
			if (ex.Length == ex2.Length && ex.ModifiersCount == ex2.ModifiersCount && (result = ex.Equals(ex2)))
			{
				break;
			}
		}
		return result;
	}
}
