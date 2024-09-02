using System;
using System.Collections.Generic;
using System.IO;
using DocGen.DocIO.DLS;

namespace DocGen.DocIO.ReaderWriter.Biff_Records;

[CLSCompliant(false)]
internal class Fields : BaseWordRecord
{
	internal int DEF_FLD_SIZE = 2;

	private BinaryReader m_reader;

	private DocIOSortedList<int, FieldDescriptor> m_curList;

	private Dictionary<WordSubdocument, DocIOSortedList<int, FieldDescriptor>> m_fieldsList = new Dictionary<WordSubdocument, DocIOSortedList<int, FieldDescriptor>>();

	internal DocIOSortedList<int, FieldDescriptor> MainFields
	{
		get
		{
			if (m_fieldsList.ContainsKey(WordSubdocument.Main))
			{
				return m_fieldsList[WordSubdocument.Main];
			}
			return null;
		}
	}

	internal DocIOSortedList<int, FieldDescriptor> HFFields
	{
		get
		{
			if (m_fieldsList.ContainsKey(WordSubdocument.HeaderFooter))
			{
				return m_fieldsList[WordSubdocument.HeaderFooter];
			}
			return null;
		}
	}

	internal DocIOSortedList<int, FieldDescriptor> FtnFields
	{
		get
		{
			if (m_fieldsList.ContainsKey(WordSubdocument.Footnote))
			{
				return m_fieldsList[WordSubdocument.Footnote];
			}
			return null;
		}
	}

	internal DocIOSortedList<int, FieldDescriptor> AtnFields
	{
		get
		{
			if (m_fieldsList.ContainsKey(WordSubdocument.Annotation))
			{
				return m_fieldsList[WordSubdocument.Annotation];
			}
			return null;
		}
	}

	internal DocIOSortedList<int, FieldDescriptor> EdnFields
	{
		get
		{
			if (m_fieldsList.ContainsKey(WordSubdocument.Endnote))
			{
				return m_fieldsList[WordSubdocument.Endnote];
			}
			return null;
		}
	}

	internal DocIOSortedList<int, FieldDescriptor> TxbxFields
	{
		get
		{
			if (m_fieldsList.ContainsKey(WordSubdocument.TextBox))
			{
				return m_fieldsList[WordSubdocument.TextBox];
			}
			return null;
		}
	}

	internal DocIOSortedList<int, FieldDescriptor> HdrTxbxFields
	{
		get
		{
			if (m_fieldsList.ContainsKey(WordSubdocument.HeaderTextBox))
			{
				return m_fieldsList[WordSubdocument.HeaderTextBox];
			}
			return null;
		}
	}

	internal Fields(Fib fib, BinaryReader reader)
	{
		m_fieldsList = new Dictionary<WordSubdocument, DocIOSortedList<int, FieldDescriptor>>();
		m_reader = reader;
		ReadFieldsForSubDoc(WordSubdocument.Main, fib.FibRgFcLcb97FcPlcfFldMom, fib.FibRgFcLcb97LcbPlcfFldMom);
		ReadFieldsForSubDoc(WordSubdocument.HeaderFooter, fib.FibRgFcLcb97FcPlcfFldHdr, fib.FibRgFcLcb97LcbPlcfFldHdr);
		ReadFieldsForSubDoc(WordSubdocument.Footnote, fib.FibRgFcLcb97FcPlcfFldFtn, fib.FibRgFcLcb97LcbPlcfFldFtn);
		ReadFieldsForSubDoc(WordSubdocument.Annotation, fib.FibRgFcLcb97FcPlcfFldAtn, fib.FibRgFcLcb97LcbPlcfFldAtn);
		ReadFieldsForSubDoc(WordSubdocument.Endnote, fib.FibRgFcLcb97FcPlcfFldEdn, fib.FibRgFcLcb97LcbPlcfFldEdn);
		ReadFieldsForSubDoc(WordSubdocument.TextBox, fib.FibRgFcLcb97FcPlcfFldTxbx, fib.FibRgFcLcb97LcbPlcfFldTxbx);
		ReadFieldsForSubDoc(WordSubdocument.HeaderTextBox, fib.FibRgFcLcb97FcPlcffldHdrTxbx, fib.FibRgFcLcb97LcbPlcffldHdrTxbx);
	}

	internal Fields()
	{
	}

	internal void AddField(WordSubdocument docType, FieldDescriptor fld, int pos)
	{
		if (!m_fieldsList.ContainsKey(docType))
		{
			DocIOSortedList<int, FieldDescriptor> value = new DocIOSortedList<int, FieldDescriptor>();
			m_fieldsList.Add(docType, value);
		}
		m_fieldsList[docType].Add(pos, fld);
	}

	internal DocIOSortedList<int, FieldDescriptor> GetFieldsForSubDoc(WordSubdocument type)
	{
		return m_fieldsList[type];
	}

	internal void Write(Stream stream, uint endPosition, WordSubdocument subDocument)
	{
		if (m_fieldsList.Count > 0 && m_fieldsList.ContainsKey(subDocument))
		{
			WriteFieldsForSubDocument(m_fieldsList[subDocument], stream, endPosition);
		}
	}

	internal override void Close()
	{
		base.Close();
		if (m_reader != null)
		{
			m_reader.Dispose();
		}
		if (m_curList != null)
		{
			m_curList.Clear();
			m_curList = null;
		}
		if (m_fieldsList == null)
		{
			return;
		}
		foreach (DocIOSortedList<int, FieldDescriptor> value in m_fieldsList.Values)
		{
			value.Clear();
		}
		m_fieldsList.Clear();
		m_fieldsList = null;
	}

	internal FieldDescriptor FindFld(WordSubdocument docType, int pos)
	{
		return m_fieldsList[docType][pos];
	}

	private void ReadFieldDescriptor(BinaryReader reader, int pos, int posNext)
	{
		FieldDescriptor value = new FieldDescriptor(reader);
		m_curList[pos] = value;
	}

	private void ReadFieldsForSubDoc(WordSubdocument docType, uint pos, uint length)
	{
		m_curList = new DocIOSortedList<int, FieldDescriptor>();
		m_fieldsList[docType] = m_curList;
		m_reader.BaseStream.Position = (int)pos;
		PosStructReader.Read(m_reader, (int)length, DEF_FLD_SIZE, ReadFieldDescriptor);
	}

	private void WriteFieldsForSubDocument(DocIOSortedList<int, FieldDescriptor> stList, Stream stream, uint endPosition)
	{
		int i = 0;
		for (int count = stList.Count; i < count; i++)
		{
			BaseWordRecord.WriteInt32(stream, stList.GetKey(i));
		}
		BaseWordRecord.WriteUInt32(stream, endPosition);
		foreach (int key in stList.Keys)
		{
			stList[key].Write(stream);
		}
	}
}
