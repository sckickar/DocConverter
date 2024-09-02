using System;
using System.IO;

namespace DocGen.DocIO.ReaderWriter.Biff_Records.Structures;

[CLSCompliant(false)]
internal class DOP95
{
	private Copts80 m_copts80;

	private DOPDescriptor m_dopBase;

	internal Copts80 Copts80
	{
		get
		{
			if (m_copts80 == null)
			{
				m_copts80 = new Copts80(m_dopBase);
			}
			return m_copts80;
		}
	}

	internal DOP95(DOPDescriptor dopBase)
	{
		m_dopBase = dopBase;
	}

	internal void Parse(Stream stream)
	{
		Copts80.Parse(stream);
	}

	internal void Write(Stream stream)
	{
		Copts80.Write(stream);
	}
}
