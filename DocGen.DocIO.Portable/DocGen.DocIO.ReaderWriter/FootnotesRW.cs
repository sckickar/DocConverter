using System;
using System.IO;

namespace DocGen.DocIO.ReaderWriter;

[CLSCompliant(false)]
internal class FootnotesRW : SubDocumentRW
{
	internal int InitialDescriptorNumber
	{
		get
		{
			return m_iInitialDesctiptorNumber;
		}
		set
		{
			m_iInitialDesctiptorNumber = value;
		}
	}

	internal FootnotesRW()
	{
	}

	internal FootnotesRW(Stream stream, Fib fib)
		: base(stream, fib)
	{
	}

	internal void AddReferense(int pos, bool autoNumbered)
	{
		m_refPositions.Add(pos);
		if (autoNumbered)
		{
			m_autoCount++;
		}
		m_descrFootEndntes.Add((short)(autoNumbered ? m_autoCount : 0));
	}

	internal int GetDescriptor(int index)
	{
		return m_descrFootEndntes[index];
	}

	protected override void WriteTxtPositions()
	{
		if (m_txtPositions.Count > 0)
		{
			m_fib.FibRgFcLcb97FcPlcffndTxt = (uint)m_writer.BaseStream.Position;
			WriteTxtPositionsBase();
			m_fib.FibRgFcLcb97LcbPlcffndTxt = (uint)(m_writer.BaseStream.Position - m_fib.FibRgFcLcb97FcPlcffndTxt);
		}
	}

	protected override void WriteDescriptors()
	{
		if (m_descrFootEndntes.Count <= 0)
		{
			return;
		}
		m_fib.FibRgFcLcb97FcPlcffndRef = (uint)m_writer.BaseStream.Position;
		WriteRefPositions(m_endReference);
		foreach (short descrFootEndnte in m_descrFootEndntes)
		{
			m_writer.Write(descrFootEndnte);
		}
		m_fib.FibRgFcLcb97LcbPlcffndRef = (uint)(m_writer.BaseStream.Position - m_fib.FibRgFcLcb97FcPlcffndRef);
	}

	protected override void ReadTxtPositions()
	{
		int fibRgFcLcb97LcbPlcffndTxt = (int)m_fib.FibRgFcLcb97LcbPlcffndTxt;
		if (fibRgFcLcb97LcbPlcffndTxt > 0)
		{
			m_reader.BaseStream.Position = m_fib.FibRgFcLcb97FcPlcffndTxt;
			int count = fibRgFcLcb97LcbPlcffndTxt / 4;
			ReadTxtPositions(count);
		}
	}

	protected override void ReadDescriptors()
	{
		if (m_fib.FibRgFcLcb97LcbPlcffndRef != 0)
		{
			m_reader.BaseStream.Position = m_fib.FibRgFcLcb97FcPlcffndRef;
			_ = new byte[m_fib.FibRgFcLcb97LcbPlcffndRef];
			m_reader.ReadBytes((int)m_fib.FibRgFcLcb97LcbPlcffndRef);
			m_reader.BaseStream.Position = m_fib.FibRgFcLcb97FcPlcffndRef;
			ReadDescriptors((int)m_fib.FibRgFcLcb97LcbPlcffndRef, 2);
			base.ReadDescriptors();
		}
	}

	protected override void ReadDescriptor(BinaryReader reader, int pos, int posNext)
	{
		if (reader.BaseStream.Position < reader.BaseStream.Length)
		{
			m_descrFootEndntes.Add(reader.ReadInt16());
			base.ReadDescriptor(reader, pos, posNext);
		}
	}
}
