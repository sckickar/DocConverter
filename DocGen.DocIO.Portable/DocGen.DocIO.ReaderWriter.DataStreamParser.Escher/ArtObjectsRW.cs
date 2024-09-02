using System;
using System.IO;
using DocGen.DocIO.DLS;
using DocGen.DocIO.ReaderWriter.Biff_Records;

namespace DocGen.DocIO.ReaderWriter.DataStreamParser.Escher;

[CLSCompliant(false)]
internal class ArtObjectsRW : BaseWordRecord
{
	private Stream m_stream;

	private DocIOSortedList<WordSubdocument, DocIOSortedList<int, FileShapeAddress>> m_fspas;

	private DocIOSortedList<WordSubdocument, DocIOSortedList<int, TextBoxStoryDescriptor>> m_txbxs;

	private DocIOSortedList<WordSubdocument, DocIOSortedList<int, BreakDescriptor>> m_txbxBkds;

	private int m_txBxMainEndPos;

	private int m_txBxHeaderEndPos;

	internal DocIOSortedList<int, FileShapeAddress> MainDocFSPAs
	{
		get
		{
			if (m_fspas.ContainsKey(WordSubdocument.Main))
			{
				return m_fspas[WordSubdocument.Main];
			}
			return null;
		}
	}

	internal DocIOSortedList<int, TextBoxStoryDescriptor> MainDocTxBxs
	{
		get
		{
			if (m_txbxs.ContainsKey(WordSubdocument.Main))
			{
				return m_txbxs[WordSubdocument.Main];
			}
			return null;
		}
	}

	internal DocIOSortedList<int, BreakDescriptor> MainDocTxBxBKDs
	{
		get
		{
			if (m_txbxBkds.ContainsKey(WordSubdocument.Main))
			{
				return m_txbxBkds[WordSubdocument.Main];
			}
			return null;
		}
	}

	internal DocIOSortedList<int, FileShapeAddress> HfDocFSPAs
	{
		get
		{
			if (m_fspas.ContainsKey(WordSubdocument.HeaderFooter))
			{
				return m_fspas[WordSubdocument.HeaderFooter];
			}
			return null;
		}
	}

	internal DocIOSortedList<int, TextBoxStoryDescriptor> HfDocTxBxs
	{
		get
		{
			if (m_txbxs.ContainsKey(WordSubdocument.HeaderFooter))
			{
				return m_txbxs[WordSubdocument.HeaderFooter];
			}
			return null;
		}
	}

	internal DocIOSortedList<int, BreakDescriptor> HfDocTxBxBKDs
	{
		get
		{
			if (m_txbxBkds.ContainsKey(WordSubdocument.HeaderFooter))
			{
				return m_txbxBkds[WordSubdocument.HeaderFooter];
			}
			return null;
		}
	}

	internal int StructsCount => ((MainDocFSPAs != null) ? MainDocFSPAs.Count : 0) + ((HfDocFSPAs != null) ? HfDocFSPAs.Count : 0) + ((HfDocTxBxs != null) ? HfDocTxBxs.Count : 0) + ((MainDocTxBxs != null) ? MainDocTxBxs.Count : 0);

	internal ArtObjectsRW(Fib fib, Stream stream)
		: this()
	{
		Read(stream, fib);
	}

	internal ArtObjectsRW()
	{
		m_fspas = new DocIOSortedList<WordSubdocument, DocIOSortedList<int, FileShapeAddress>>();
		m_txbxBkds = new DocIOSortedList<WordSubdocument, DocIOSortedList<int, BreakDescriptor>>();
		m_txbxs = new DocIOSortedList<WordSubdocument, DocIOSortedList<int, TextBoxStoryDescriptor>>();
	}

	internal void AddFSPA(FileShapeAddress fspa, WordSubdocument docType, int pos)
	{
		if (!m_fspas.ContainsKey(docType))
		{
			DocIOSortedList<int, FileShapeAddress> value = new DocIOSortedList<int, FileShapeAddress>();
			m_fspas.Add(docType, value);
		}
		m_fspas[docType].Add(pos, fspa);
	}

	internal void AddTxbx(WordSubdocument docType, TextBoxStoryDescriptor txbxStoryDesc, BreakDescriptor txbxBKDesc, int pos)
	{
		if (!m_txbxs.ContainsKey(docType))
		{
			DocIOSortedList<int, TextBoxStoryDescriptor> value = new DocIOSortedList<int, TextBoxStoryDescriptor>();
			m_txbxs.Add(docType, value);
		}
		m_txbxs[docType].Add(pos, txbxStoryDesc);
		if (!m_txbxBkds.ContainsKey(docType))
		{
			DocIOSortedList<int, BreakDescriptor> value2 = new DocIOSortedList<int, BreakDescriptor>();
			m_txbxBkds.Add(docType, value2);
		}
		m_txbxBkds[docType].Add(pos, txbxBKDesc);
		if (docType == WordSubdocument.Main)
		{
			m_txBxMainEndPos = pos + 3;
		}
		if (docType == WordSubdocument.HeaderFooter)
		{
			m_txBxHeaderEndPos = pos + 3;
		}
	}

	internal void Read(Stream stream, Fib fib)
	{
		m_stream = stream;
		ReadShapeFSPA(WordSubdocument.Main, (int)fib.FibRgFcLcb97FcPlcSpaMom, (int)fib.FibRgFcLcb97LcbPlcSpaMom);
		ReadShapeFSPA(WordSubdocument.HeaderFooter, (int)fib.FibRgFcLcb97FcPlcSpaHdr, (int)fib.FibRgFcLcb97LcbPlcSpaHdr);
		ReadTxbx(WordSubdocument.Main, (int)fib.FibRgFcLcb97FcPlcftxbxTxt, (int)fib.FibRgFcLcb97LcbPlcftxbxTxt);
		ReadTxbx(WordSubdocument.HeaderFooter, (int)fib.FibRgFcLcb97FcPlcfHdrtxbxTxt, (int)fib.FibRgFcLcb97LcbPlcfHdrtxbxTxt);
		ReadTxbxBkd(WordSubdocument.Main, (int)fib.FibRgFcLcb97FcPlcfTxbxBkd, (int)fib.FibRgFcLcb97LcbPlcfTxbxBkd);
		ReadTxbxBkd(WordSubdocument.HeaderFooter, (int)fib.FibRgFcLcb97FcPlcfTxbxHdrBkd, (int)fib.FibRgFcLcb97LcbPlcfTxbxHdrBkd);
	}

	internal void Write(Stream stream, Fib fib, int endMain, int endHeader)
	{
		m_stream = stream;
		WriteFSPAs(fib, endMain, endHeader);
		WriteTxBxs(fib, endMain);
		WriteTxBxBKDs(fib);
	}

	internal int GetTxbxPosition(bool isHdrTxbx, int index)
	{
		if (isHdrTxbx && HfDocTxBxs != null)
		{
			if (index != HfDocTxBxs.Count)
			{
				return GetKey(HfDocTxBxs, index);
			}
			return GetKey(HfDocTxBxs, index - 1) + 3;
		}
		if (MainDocTxBxs != null)
		{
			if (index != MainDocTxBxs.Count)
			{
				return GetKey(MainDocTxBxs, index);
			}
			return GetKey(MainDocTxBxs, index - 1) + 3;
		}
		return 0;
	}

	internal int GetShapeObjectId(WordSubdocument subDocType, int txbxIndex)
	{
		TextBoxStoryDescriptor textBoxStoryDescriptor = ((subDocType != WordSubdocument.TextBox) ? GetByIndex(HfDocTxBxs, txbxIndex) : GetByIndex(MainDocTxBxs, txbxIndex));
		return textBoxStoryDescriptor.ShapeIdent;
	}

	internal override void Close()
	{
		base.Close();
		m_stream = null;
		if (m_fspas != null)
		{
			foreach (DocIOSortedList<int, FileShapeAddress> value in m_fspas.Values)
			{
				value.Clear();
			}
			m_fspas.Clear();
			m_fspas = null;
		}
		if (m_txbxs != null)
		{
			foreach (DocIOSortedList<int, TextBoxStoryDescriptor> value2 in m_txbxs.Values)
			{
				value2.Clear();
			}
			m_txbxs.Clear();
			m_txbxs = null;
		}
		if (m_txbxBkds == null)
		{
			return;
		}
		foreach (DocIOSortedList<int, BreakDescriptor> value3 in m_txbxBkds.Values)
		{
			value3.Clear();
		}
		m_txbxBkds.Clear();
		m_txbxBkds = null;
	}

	internal FileShapeAddress FindFileShape(WordSubdocument docType, int CP)
	{
		return m_fspas[docType]?[CP];
	}

	private void ReadShapeFSPA(WordSubdocument docType, int pos, int length)
	{
		m_stream.Position = pos;
		DocIOSortedList<int, FileShapeAddress> docIOSortedList = new DocIOSortedList<int, FileShapeAddress>();
		m_fspas[docType] = docIOSortedList;
		if (length == 0)
		{
			return;
		}
		int[] positions = GetPositions(26, length);
		int i = 0;
		for (int num = positions.Length - 1; i < num; i++)
		{
			FileShapeAddress fileShapeAddress = new FileShapeAddress(m_stream);
			bool flag = false;
			for (int j = 0; j < docIOSortedList.Keys.Count; j++)
			{
				if (docIOSortedList[docIOSortedList.Keys[j]].Spid == fileShapeAddress.Spid)
				{
					flag = true;
				}
			}
			if (!flag)
			{
				docIOSortedList.Add(positions[i], fileShapeAddress);
			}
		}
	}

	private void ReadTxbx(WordSubdocument docType, int pos, int length)
	{
		if (length > 0)
		{
			DocIOSortedList<int, TextBoxStoryDescriptor> docIOSortedList = new DocIOSortedList<int, TextBoxStoryDescriptor>();
			m_txbxs[docType] = docIOSortedList;
			m_stream.Position = pos;
			int[] positions = GetPositions(TextBoxStoryDescriptor.DEF_TXBX_LENGTH, length);
			for (int i = 0; i < positions.Length - 1; i++)
			{
				TextBoxStoryDescriptor value = new TextBoxStoryDescriptor(m_stream);
				docIOSortedList.Add(positions[i], value);
			}
		}
	}

	private void ReadTxbxBkd(WordSubdocument docType, int pos, int length)
	{
		if (length > 0)
		{
			DocIOSortedList<int, BreakDescriptor> docIOSortedList = new DocIOSortedList<int, BreakDescriptor>();
			m_txbxBkds[docType] = docIOSortedList;
			m_stream.Position = pos;
			int[] positions = GetPositions(6, length);
			for (int i = 0; i < positions.Length - 1; i++)
			{
				BreakDescriptor value = new BreakDescriptor(m_stream);
				docIOSortedList.Add(positions[i], value);
			}
		}
	}

	private int[] GetPositions(int structSize, int length)
	{
		int num = (length - 4) / (structSize + 4) + 1;
		int[] array = new int[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = (int)BaseWordRecord.ReadUInt32(m_stream);
		}
		return array;
	}

	private void WriteFSPAs(Fib fib, int endMain, int endHeader)
	{
		if (MainDocFSPAs != null || HfDocFSPAs != null)
		{
			if (MainDocFSPAs != null)
			{
				fib.FibRgFcLcb97FcPlcSpaMom = (uint)m_stream.Position;
				WriteArtObjectsFSPAs(MainDocFSPAs, endMain);
				fib.FibRgFcLcb97LcbPlcSpaMom = (uint)(m_stream.Position - fib.FibRgFcLcb97FcPlcSpaMom);
			}
			if (HfDocFSPAs != null)
			{
				fib.FibRgFcLcb97FcPlcSpaHdr = (uint)m_stream.Position;
				WriteArtObjectsFSPAs(HfDocFSPAs, endHeader);
				fib.FibRgFcLcb97LcbPlcSpaHdr = (uint)(m_stream.Position - fib.FibRgFcLcb97FcPlcSpaHdr);
			}
		}
	}

	private void WriteTxBxs(Fib fib, int endCharacter)
	{
		if (MainDocTxBxs != null || HfDocTxBxs != null)
		{
			if (MainDocTxBxs != null)
			{
				fib.FibRgFcLcb97FcPlcftxbxTxt = (uint)m_stream.Position;
				WriteArtObjectsTxBxs(MainDocTxBxs, endCharacter);
				fib.FibRgFcLcb97LcbPlcftxbxTxt = (uint)(m_stream.Position - fib.FibRgFcLcb97FcPlcftxbxTxt);
			}
			if (HfDocTxBxs != null)
			{
				fib.FibRgFcLcb97FcPlcfHdrtxbxTxt = (uint)m_stream.Position;
				WriteArtObjectsTxBxs(HfDocTxBxs, endCharacter);
				fib.FibRgFcLcb97LcbPlcfHdrtxbxTxt = (uint)(m_stream.Position - fib.FibRgFcLcb97FcPlcfHdrtxbxTxt);
			}
		}
	}

	private void WriteTxBxBKDs(Fib fib)
	{
		if (MainDocTxBxBKDs != null || HfDocTxBxBKDs != null)
		{
			if (MainDocTxBxBKDs != null)
			{
				fib.FibRgFcLcb97FcPlcfTxbxBkd = (uint)m_stream.Position;
				WriteArtObjectsTxBxBKDs(MainDocTxBxBKDs, m_txBxMainEndPos);
				fib.FibRgFcLcb97LcbPlcfTxbxBkd = (uint)(m_stream.Position - fib.FibRgFcLcb97FcPlcfTxbxBkd);
			}
			if (HfDocTxBxBKDs != null)
			{
				fib.FibRgFcLcb97FcPlcfTxbxHdrBkd = (uint)m_stream.Position;
				WriteArtObjectsTxBxBKDs(HfDocTxBxBKDs, m_txBxHeaderEndPos);
				fib.FibRgFcLcb97LcbPlcfTxbxHdrBkd = (uint)(m_stream.Position - fib.FibRgFcLcb97FcPlcfTxbxHdrBkd);
			}
		}
	}

	private void WriteArtObjectsTxBxBKDs(DocIOSortedList<int, BreakDescriptor> stList, int endPos)
	{
		foreach (int key in stList.Keys)
		{
			BaseWordRecord.WriteInt32(m_stream, key);
		}
		BaseWordRecord.WriteInt32(m_stream, endPos);
		foreach (BreakDescriptor value in stList.Values)
		{
			value.Write(m_stream);
		}
	}

	private void WriteArtObjectsTxBxs(DocIOSortedList<int, TextBoxStoryDescriptor> stList, int endPos)
	{
		foreach (int key in stList.Keys)
		{
			BaseWordRecord.WriteInt32(m_stream, key);
		}
		BaseWordRecord.WriteInt32(m_stream, endPos);
		foreach (TextBoxStoryDescriptor value in stList.Values)
		{
			value.Write(m_stream);
		}
	}

	private void WriteArtObjectsFSPAs(DocIOSortedList<int, FileShapeAddress> stList, int endPos)
	{
		foreach (int key in stList.Keys)
		{
			BaseWordRecord.WriteInt32(m_stream, key);
		}
		BaseWordRecord.WriteInt32(m_stream, endPos);
		foreach (FileShapeAddress value in stList.Values)
		{
			value.Write(m_stream);
		}
	}

	private TextBoxStoryDescriptor GetByIndex(DocIOSortedList<int, TextBoxStoryDescriptor> col, int index)
	{
		if (index > col.Count - 1 || index < 0)
		{
			return null;
		}
		return col.Values[index];
	}

	private int GetKey(DocIOSortedList<int, TextBoxStoryDescriptor> col, int index)
	{
		if (index > col.Count - 1 || index < 0)
		{
			return -1;
		}
		return col.Keys[index];
	}
}
