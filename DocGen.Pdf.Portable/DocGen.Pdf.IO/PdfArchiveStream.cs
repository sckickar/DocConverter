using System;
using System.IO;
using DocGen.Pdf.Primitives;
using DocGen.Pdf.Security;

namespace DocGen.Pdf.IO;

internal class PdfArchiveStream : PdfStream
{
	private class ObjInfo
	{
		internal IPdfPrimitive Obj;

		internal int Index;

		internal ObjInfo(IPdfPrimitive obj)
		{
			Obj = obj;
			Index = 0;
		}
	}

	private SortedListEx m_indices;

	private MemoryStream m_objects;

	private StreamWriter m_writer;

	private IPdfWriter m_objectWriter;

	private PdfDocumentBase m_document;

	internal int ObjCount => m_indices.Count;

	internal PdfArchiveStream(PdfDocumentBase document)
	{
		if (document == null)
		{
			throw new ArgumentNullException("document");
		}
		m_document = document;
		m_objects = new MemoryStream(1000);
		m_objectWriter = new PdfWriter(m_objects);
		m_objectWriter.Document = m_document;
		m_indices = new SortedListEx(16);
	}

	public void SaveObject(IPdfPrimitive obj, PdfReference reference)
	{
		long position = m_objectWriter.Position;
		m_indices[position] = reference.ObjNum;
		PdfSecurity security = m_document.Security;
		bool enabled = security.Enabled;
		security.Enabled = false;
		lock (PdfDocument.Cache)
		{
			obj.Save(m_objectWriter);
		}
		security.Enabled = enabled;
		m_objectWriter.Write("\r\n");
	}

	public int GetIndex(long objNum)
	{
		return m_indices.IndexOfValue(objNum);
	}

	public override void Save(IPdfWriter writer)
	{
		using (MemoryStream memoryStream = new MemoryStream((int)m_objects.Length + 100))
		{
			using (m_writer = new StreamWriter(memoryStream))
			{
				SaveIndices();
				m_writer.Flush();
				base["First"] = new PdfNumber(m_writer.BaseStream.Position);
				SaveObjects();
				m_writer.Flush();
				base.Data = memoryStream.ToArray();
			}
		}
		base["N"] = new PdfNumber(m_indices.Count);
		base["Type"] = new PdfName("ObjStm");
		base.Save(writer);
	}

	internal new void Clear()
	{
		m_indices.Clear();
		if (m_objectWriter != null)
		{
			m_objectWriter = null;
		}
		base.Clear();
	}

	private void SaveObjects()
	{
		byte[] array = m_objects.ToArray();
		m_writer.BaseStream.Write(array, 0, array.Length);
	}

	private void SaveIndices()
	{
		foreach (long key in m_indices.Keys)
		{
			m_writer.Write(m_indices[key]);
			m_writer.Write(" ");
			m_writer.Write(key);
			m_writer.Write("\r\n");
		}
	}
}
