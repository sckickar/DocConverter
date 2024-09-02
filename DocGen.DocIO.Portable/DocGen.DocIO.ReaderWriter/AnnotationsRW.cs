using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DocGen.DocIO.ReaderWriter.Biff_Records;

namespace DocGen.DocIO.ReaderWriter;

[CLSCompliant(false)]
internal class AnnotationsRW : SubDocumentRW
{
	internal class AnnotationBookmark
	{
		private int m_iStartPos;

		private int m_iEndPos;

		internal int Start
		{
			get
			{
				return m_iStartPos;
			}
			set
			{
				m_iStartPos = value;
			}
		}

		internal int End
		{
			get
			{
				return m_iEndPos;
			}
			set
			{
				m_iEndPos = value;
			}
		}

		internal AnnotationBookmark(int start, int end)
		{
			m_iEndPos = end;
			m_iStartPos = start;
			if (end < start)
			{
				end = start;
				m_iEndPos = end;
			}
		}
	}

	internal class AnnotationBookmarks
	{
		internal class Comparer : IComparer
		{
			public int Compare(object x, object y)
			{
				int num = (int)x;
				int num2 = (int)y;
				if (num > num2)
				{
					return 1;
				}
				if (num != num2)
				{
					return -1;
				}
				return 0;
			}
		}

		private BookmarkDescriptor m_descriptor;

		private List<int> m_keys = new List<int>();

		private int m_bookmarkCount;

		internal AnnotationBookmark this[int key]
		{
			get
			{
				if (m_keys.Contains(key))
				{
					int i = m_keys.IndexOf(key);
					return new AnnotationBookmark(m_descriptor.GetBeginPos(i), m_descriptor.GetEndPos(i));
				}
				return null;
			}
		}

		internal AnnotationBookmarks(BinaryReader reader, Fib fib)
		{
			Read(reader, fib);
		}

		internal AnnotationBookmarks()
		{
			m_descriptor = new BookmarkDescriptor();
		}

		internal void Read(BinaryReader reader, Fib fib)
		{
			ReadSttbf(reader, (int)fib.FibRgFcLcb97FcSttbfAtnBkmk, (int)fib.FibRgFcLcb97LcbSttbfAtnBkmk);
			if (m_bookmarkCount > 0)
			{
				m_descriptor = new BookmarkDescriptor(reader.BaseStream, m_bookmarkCount, (int)fib.FibRgFcLcb97FcPlcfAtnBkf, (int)fib.FibRgFcLcb97LcbPlcfAtnBkf, (int)fib.FibRgFcLcb97FcPlcfAtnBkl, (int)fib.FibRgFcLcb97LcbPlcfAtnBkl);
			}
		}

		internal void Add(int key, AnnotationBookmark bookmark)
		{
			m_keys.Add(key);
			m_descriptor.Add(bookmark.Start);
			m_descriptor.SetEndPos(m_descriptor.BookmarkCount - 1, bookmark.End);
		}

		internal void Write(BinaryWriter writer, Fib fib)
		{
			if (m_descriptor.BookmarkCount != 0)
			{
				int bookmarkCount = m_descriptor.BookmarkCount;
				int[] array = new int[bookmarkCount];
				int[] array2 = new int[bookmarkCount];
				for (int i = 0; i < m_descriptor.BookmarkCount; i++)
				{
					array[i] = m_descriptor.GetBeginPos(i);
					array2[i] = m_keys[i];
				}
				SortDictionaryValues(array, array2);
				fib.FibRgFcLcb97FcSttbfAtnBkmk = (uint)writer.BaseStream.Position;
				WriteSttbf(writer, array2);
				fib.FibRgFcLcb97LcbSttbfAtnBkmk = (uint)(writer.BaseStream.Position - fib.FibRgFcLcb97FcSttbfAtnBkmk);
				int end = fib.CcpText + 2;
				fib.FibRgFcLcb97FcPlcfAtnBkf = (uint)writer.BaseStream.Position;
				WriteBKF(writer, array, array2, end);
				fib.FibRgFcLcb97LcbPlcfAtnBkf = (uint)(writer.BaseStream.Position - fib.FibRgFcLcb97FcPlcfAtnBkf);
				fib.FibRgFcLcb97FcPlcfAtnBkl = (uint)writer.BaseStream.Position;
				WriteBKL(writer, end);
				fib.FibRgFcLcb97LcbPlcfAtnBkl = (uint)(writer.BaseStream.Position - fib.FibRgFcLcb97FcPlcfAtnBkl);
			}
		}

		private void SortDictionaryValues(int[] keys, int[] values)
		{
			for (int i = 0; i < keys.Length - 1; i++)
			{
				for (int j = i + 1; j < keys.Length; j++)
				{
					if (keys[i] > keys[j])
					{
						int num = keys[i];
						keys[i] = keys[j];
						keys[j] = num;
						int num2 = values[i];
						values[i] = values[j];
						values[j] = num2;
					}
				}
			}
		}

		internal void Close()
		{
			if (m_descriptor != null)
			{
				m_descriptor = null;
			}
			if (m_keys != null)
			{
				m_keys.Clear();
				m_keys = null;
			}
		}

		private void ReadSttbf(BinaryReader reader, int start, int length)
		{
			if (length > 0)
			{
				reader.BaseStream.Position = start + 2;
				m_bookmarkCount = reader.ReadInt16();
				reader.ReadInt16();
				for (int i = 0; i < m_bookmarkCount; i++)
				{
					reader.ReadInt32();
					m_keys.Add(reader.ReadInt32());
					reader.ReadInt32();
				}
			}
		}

		private void WriteSttbf(BinaryWriter writer, int[] values)
		{
			writer.Write((short)(-1));
			writer.Write((short)m_descriptor.BookmarkCount);
			writer.Write((short)10);
			for (int i = 0; i < values.Length; i++)
			{
				writer.Write(16777216);
				writer.Write(values[i]);
				writer.Write(-1);
			}
		}

		private void WriteBKF(BinaryWriter writer, int[] keys, int[] values, int end)
		{
			int[] array = new int[keys.Length];
			int num = 0;
			for (int i = 0; i < keys.Length; i++)
			{
				array[num] = m_keys.IndexOf(values[i]);
				writer.Write(keys[i]);
				num++;
			}
			writer.Write(end);
			for (int j = 0; j < keys.Length; j++)
			{
				writer.Write(array[j]);
			}
		}

		private void WriteBKL(BinaryWriter writer, int end)
		{
			for (int i = 0; i < m_descriptor.BookmarkCount; i++)
			{
				writer.Write(m_descriptor.GetEndPos(i));
			}
			writer.Write(end);
		}
	}

	private List<string> m_grpXstAtnOwners;

	private AnnotationBookmarks m_bookmarks;

	private AnnotationDescriptor m_currDescriptor;

	private int m_descIndex = -1;

	internal AnnotationsRW(Stream stream, Fib fib)
		: base(stream, fib)
	{
	}

	internal AnnotationsRW()
	{
		m_bookmarks = new AnnotationBookmarks();
	}

	internal override void Read(Stream stream, Fib fib)
	{
		base.Read(stream, fib);
		ReadGXAO((int)fib.FibRgFcLcb97FcGrpXstAtnOwners, (int)fib.FibRgFcLcb97LcbGrpXstAtnOwners);
		m_bookmarks = new AnnotationBookmarks(m_reader, fib);
	}

	internal override void Write(Stream stream, Fib fib)
	{
		base.Write(stream, fib);
		WriteGXAO();
		m_bookmarks.Write(m_writer, fib);
	}

	internal void AddDescriptor(AnnotationDescriptor atrd, int pos, int bkmkStart, int bkmkEnd)
	{
		AddDescriptor(atrd, pos);
		int tagBkmk = atrd.TagBkmk;
		if (tagBkmk != -1)
		{
			m_bookmarks.Add(tagBkmk, new AnnotationBookmark(bkmkStart, bkmkEnd));
		}
	}

	internal void AddDescriptor(AnnotationDescriptor atrd, int pos)
	{
		m_descriptorsAnnot.Add(atrd);
		AddRefPosition(pos);
	}

	internal int AddGXAO(string gxao)
	{
		int num = m_grpXstAtnOwners.IndexOf(gxao);
		if (num == -1)
		{
			num = m_grpXstAtnOwners.Count;
			m_grpXstAtnOwners.Add(gxao);
		}
		return num;
	}

	internal AnnotationDescriptor GetDescriptor(int index)
	{
		if (index != m_descIndex && index < m_descriptorsAnnot.Count)
		{
			m_currDescriptor = m_descriptorsAnnot[index];
			m_descIndex = index;
		}
		return m_currDescriptor;
	}

	internal string GetUser(int index)
	{
		AnnotationDescriptor descriptor = GetDescriptor(index);
		if (descriptor == null)
		{
			return "";
		}
		if (descriptor.IndexToGrpOwner < m_grpXstAtnOwners.Count)
		{
			return m_grpXstAtnOwners[descriptor.IndexToGrpOwner].ToString();
		}
		return "";
	}

	internal int GetBookmarkStartOffset(int index)
	{
		AnnotationDescriptor descriptor = GetDescriptor(index);
		if (descriptor.TagBkmk == -1)
		{
			return 0;
		}
		AnnotationBookmark annotationBookmark = m_bookmarks[descriptor.TagBkmk];
		if (annotationBookmark == null)
		{
			return 0;
		}
		return m_refPositions[index] - annotationBookmark.Start;
	}

	internal int GetBookmarkEndOffset(int index)
	{
		AnnotationDescriptor descriptor = GetDescriptor(index);
		if (descriptor.TagBkmk == -1)
		{
			return 0;
		}
		AnnotationBookmark annotationBookmark = m_bookmarks[descriptor.TagBkmk];
		if (annotationBookmark == null)
		{
			return 0;
		}
		return annotationBookmark.End - m_refPositions[index];
	}

	internal int GetPosition(int index)
	{
		return m_refPositions[index];
	}

	private void ReadGXAO(int pos, int length)
	{
		if (length > 0)
		{
			m_reader.BaseStream.Position = pos;
			for (int num = m_reader.ReadInt16(); num != 0; num = ((m_reader.BaseStream.Position != pos + length) ? m_reader.ReadInt16() : 0))
			{
				byte[] array = m_reader.ReadBytes(num * 2);
				string @string = Encoding.Unicode.GetString(array, 0, array.Length);
				AddGXAO(@string);
			}
		}
	}

	private void WriteGXAO()
	{
		if (m_grpXstAtnOwners.Count > 0)
		{
			m_fib.FibRgFcLcb97FcGrpXstAtnOwners = (uint)m_writer.BaseStream.Position;
			string text = null;
			int i = 0;
			for (int count = m_grpXstAtnOwners.Count; i < count; i++)
			{
				text = m_grpXstAtnOwners[i];
				short value = (short)text.Length;
				m_writer.Write(value);
				m_writer.Write(Encoding.Unicode.GetBytes(text));
			}
			m_fib.FibRgFcLcb97LcbGrpXstAtnOwners = (uint)(m_writer.BaseStream.Position - m_fib.FibRgFcLcb97FcGrpXstAtnOwners);
		}
	}

	internal override void Close()
	{
		base.Close();
		if (m_grpXstAtnOwners != null)
		{
			m_grpXstAtnOwners.Clear();
			m_grpXstAtnOwners = null;
		}
		if (m_bookmarks != null)
		{
			m_bookmarks = null;
		}
		if (m_currDescriptor != null)
		{
			m_currDescriptor = null;
		}
	}

	protected override void Init()
	{
		base.Init();
		m_grpXstAtnOwners = new List<string>();
	}

	protected override void ReadTxtPositions()
	{
		int fibRgFcLcb97LcbPlcfandTxt = (int)m_fib.FibRgFcLcb97LcbPlcfandTxt;
		if (fibRgFcLcb97LcbPlcfandTxt > 0)
		{
			m_reader.BaseStream.Position = m_fib.FibRgFcLcb97FcPlcfandTxt;
			int count = fibRgFcLcb97LcbPlcfandTxt / 4;
			ReadTxtPositions(count);
		}
	}

	protected override void ReadDescriptors()
	{
		int fibRgFcLcb97lcbPlcfandRef = (int)m_fib.FibRgFcLcb97lcbPlcfandRef;
		if (fibRgFcLcb97lcbPlcfandRef > 0)
		{
			m_reader.BaseStream.Position = m_fib.FibRgFcLcb97FcPlcfandRef;
			ReadDescriptors(fibRgFcLcb97lcbPlcfandRef, 30);
			base.ReadDescriptors();
		}
	}

	protected override void ReadDescriptor(BinaryReader reader, int pos, int posNext)
	{
		if (reader.BaseStream.Position < reader.BaseStream.Length)
		{
			base.ReadDescriptor(reader, pos, posNext);
			m_descriptorsAnnot.Add(new AnnotationDescriptor(reader));
		}
	}

	protected override void WriteDescriptors()
	{
		m_fib.FibRgFcLcb97FcPlcfandRef = (uint)m_writer.BaseStream.Position;
		WriteRefPositions(m_endReference);
		int i = 0;
		for (int count = m_descriptorsAnnot.Count; i < count; i++)
		{
			m_descriptorsAnnot[i].Write(m_writer);
		}
		m_fib.FibRgFcLcb97lcbPlcfandRef = (uint)(m_writer.BaseStream.Position - m_fib.FibRgFcLcb97FcPlcfandRef);
	}

	protected override void WriteTxtPositions()
	{
		if (m_txtPositions.Count > 0)
		{
			m_fib.FibRgFcLcb97FcPlcfandTxt = (uint)m_writer.BaseStream.Position;
			WriteTxtPositionsBase();
			m_fib.FibRgFcLcb97LcbPlcfandTxt = (uint)(m_writer.BaseStream.Position - m_fib.FibRgFcLcb97FcPlcfandTxt);
		}
	}
}
