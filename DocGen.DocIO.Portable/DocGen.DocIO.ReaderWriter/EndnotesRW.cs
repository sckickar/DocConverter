using System;
using System.IO;

namespace DocGen.DocIO.ReaderWriter;

[CLSCompliant(false)]
internal class EndnotesRW : FootnotesRW
{
	internal EndnotesRW()
	{
	}

	internal EndnotesRW(Stream stream, Fib fib)
		: base(stream, fib)
	{
	}

	protected override void WriteTxtPositions()
	{
		if (m_txtPositions.Count > 0)
		{
			m_fib.FibRgFcLcb97FcPlcfendTxt = (uint)m_writer.BaseStream.Position;
			WriteTxtPositionsBase();
			m_fib.FibRgFcLcb97LcbPlcfendTxt = (uint)(m_writer.BaseStream.Position - m_fib.FibRgFcLcb97FcPlcfendTxt);
		}
	}

	protected override void WriteDescriptors()
	{
		if (m_descrFootEndntes.Count > 0)
		{
			m_fib.FibRgFcLcb97FcPlcfendRef = (uint)m_writer.BaseStream.Position;
			WriteRefPositions(m_endReference);
			short num = 0;
			int i = 0;
			for (int count = m_descrFootEndntes.Count; i < count; i++)
			{
				num = m_descrFootEndntes[i];
				m_writer.Write(num);
			}
			m_fib.FibRgFcLcb97LcbPlcfendRef = (uint)(m_writer.BaseStream.Position - m_fib.FibRgFcLcb97FcPlcfendRef);
		}
	}

	protected override void ReadTxtPositions()
	{
		int fibRgFcLcb97LcbPlcfendTxt = (int)m_fib.FibRgFcLcb97LcbPlcfendTxt;
		if (fibRgFcLcb97LcbPlcfendTxt > 0)
		{
			m_reader.BaseStream.Position = m_fib.FibRgFcLcb97FcPlcfendTxt;
			int count = fibRgFcLcb97LcbPlcfendTxt / 4;
			ReadTxtPositions(count);
		}
	}

	protected override void ReadDescriptors()
	{
		if (m_fib.FibRgFcLcb97LcbPlcfendRef != 0)
		{
			m_reader.BaseStream.Position = m_fib.FibRgFcLcb97FcPlcfendRef;
			_ = new byte[m_fib.FibRgFcLcb97LcbPlcfendRef];
			m_reader.ReadBytes((int)m_fib.FibRgFcLcb97LcbPlcfendRef);
			m_reader.BaseStream.Position = m_fib.FibRgFcLcb97FcPlcfendRef;
			ReadDescriptors((int)m_fib.FibRgFcLcb97LcbPlcfendRef, 2);
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
