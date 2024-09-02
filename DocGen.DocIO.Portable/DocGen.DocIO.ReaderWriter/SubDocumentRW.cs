using System;
using System.Collections.Generic;
using System.IO;
using DocGen.DocIO.ReaderWriter.Biff_Records;

namespace DocGen.DocIO.ReaderWriter;

[CLSCompliant(false)]
internal abstract class SubDocumentRW
{
	protected Fib m_fib;

	protected List<int> m_txtPositions;

	protected List<int> m_refPositions;

	protected List<AnnotationDescriptor> m_descriptorsAnnot;

	protected List<short> m_descrFootEndntes;

	protected BinaryReader m_reader;

	protected BinaryWriter m_writer;

	private int m_endRefPosition = -1;

	protected int m_iCount;

	protected int m_iInitialDesctiptorNumber;

	protected int m_autoCount;

	protected int m_endReference;

	internal int m_footEndnoteRefIndex;

	internal int Count => m_iCount;

	internal SubDocumentRW(Stream stream, Fib fib)
		: this()
	{
		Read(stream, fib);
	}

	internal SubDocumentRW()
	{
		Init();
	}

	internal bool HasReference(int reference)
	{
		if (m_refPositions.Contains(reference))
		{
			m_footEndnoteRefIndex = m_refPositions.IndexOf(reference);
			return true;
		}
		return false;
	}

	internal bool HasReference(int startPosition, int endPosition, ref int textLength)
	{
		int num = ((m_footEndnoteRefIndex + 1 < m_refPositions.Count) ? m_refPositions[m_footEndnoteRefIndex + 1] : (-1));
		if (num != -1 && num > startPosition && num < endPosition)
		{
			textLength = num - startPosition;
			return true;
		}
		return false;
	}

	internal bool HasPosition(int position)
	{
		return m_txtPositions.Contains(position);
	}

	internal virtual void Read(Stream stream, Fib fib)
	{
		m_fib = fib;
		m_reader = new BinaryReader(stream);
		ReadTxtPositions();
		ReadDescriptors();
	}

	internal virtual void Write(Stream stream, Fib fib)
	{
		m_fib = fib;
		m_writer = new BinaryWriter(stream);
		m_endReference = m_fib.CcpText + m_fib.CcpFtn + m_fib.CcpHdd + m_fib.CcpAtn + m_fib.CcpEdn + m_fib.CcpTxbx + m_fib.CcpHdrTxbx;
		WriteTxtPositions();
		WriteDescriptors();
	}

	internal virtual void AddTxtPosition(int position)
	{
		m_txtPositions.Add(position);
	}

	internal virtual int GetTxtPosition(int index)
	{
		if (m_txtPositions.Count != 0)
		{
			return m_txtPositions[index];
		}
		return 0;
	}

	internal virtual void Close()
	{
		if (m_fib != null)
		{
			m_fib.Close();
			m_fib = null;
		}
		if (m_txtPositions != null)
		{
			m_txtPositions.Clear();
			m_txtPositions = null;
		}
		if (m_refPositions != null)
		{
			m_refPositions.Clear();
			m_refPositions = null;
		}
		if (m_descriptorsAnnot != null)
		{
			m_descriptorsAnnot.Clear();
			m_descriptorsAnnot = null;
		}
		if (m_descrFootEndntes != null)
		{
			m_descrFootEndntes.Clear();
			m_descrFootEndntes = null;
		}
		if (m_reader != null)
		{
			m_reader.Dispose();
			m_reader = null;
		}
		if (m_writer != null)
		{
			m_writer.Dispose();
			m_writer = null;
		}
	}

	protected virtual void ReadDescriptors()
	{
		if (m_endRefPosition != -1)
		{
			AddRefPosition(m_endRefPosition);
		}
	}

	protected abstract void WriteDescriptors();

	protected void ReadDescriptors(int length, int size)
	{
		PosStructReader.Read(m_reader, length, size, ReadDescriptor);
	}

	protected void AddRefPosition(int position)
	{
		m_refPositions.Add(position);
	}

	protected virtual void Init()
	{
		m_txtPositions = new List<int>();
		m_refPositions = new List<int>();
		m_descriptorsAnnot = new List<AnnotationDescriptor>();
		m_descrFootEndntes = new List<short>();
	}

	protected abstract void ReadTxtPositions();

	protected void ReadTxtPositions(int count)
	{
		m_iCount = count - 1;
		for (int i = 0; i < count; i++)
		{
			AddTxtPosition(m_reader.ReadInt32());
		}
	}

	protected void WriteTxtPositionsBase()
	{
		foreach (int txtPosition in m_txtPositions)
		{
			m_writer.Write(txtPosition);
		}
	}

	protected abstract void WriteTxtPositions();

	protected virtual void WriteRefPositions(int endPos)
	{
		foreach (int refPosition in m_refPositions)
		{
			m_writer.Write(refPosition);
		}
		if (m_refPositions.Count > 0)
		{
			m_writer.Write(endPos);
		}
	}

	protected virtual void ReadDescriptor(BinaryReader reader, int pos, int posNext)
	{
		AddRefPosition(pos);
		m_endRefPosition = posNext;
	}
}
