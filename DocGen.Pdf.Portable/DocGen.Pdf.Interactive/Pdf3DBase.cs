using System;
using System.IO;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

internal class Pdf3DBase : IPdfWrapper
{
	private string m_fileName = string.Empty;

	private Pdf3DStream m_stream = new Pdf3DStream();

	private Stream m_data;

	public Pdf3DStream Stream
	{
		get
		{
			return m_stream;
		}
		set
		{
			m_stream = value;
		}
	}

	public string FileName
	{
		get
		{
			return m_fileName;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("FileName");
			}
			if (value.Length == 0)
			{
				throw new ArithmeticException("FileName can't be empty string.");
			}
			m_fileName = value;
		}
	}

	public IPdfPrimitive Element => m_stream;

	public Pdf3DBase(Stream data)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		m_data = data;
		m_stream.BeginSave += Stream_BeginSave;
	}

	private void Stream_BeginSave(object sender, SavePdfPrimitiveEventArgs ars)
	{
		Save();
	}

	protected void Save()
	{
		byte[] array = Pdf3DStream.StreamToBytes(m_data);
		m_stream.Clear();
		m_stream.InternalStream.Write(array, 0, array.Length);
	}
}
