using System;
using DocGen.DocIO.DLS;
using DocGen.DocIO.ReaderWriter;
using DocGen.DocIO.ReaderWriter.DataStreamParser.Escher;

namespace DocGen.DocIO;

[CLSCompliant(false)]
internal abstract class WordRWAdapterBase
{
	protected IWordReader m_mainReader;

	protected IWordWriter m_mainWriter;

	protected int m_textPos;

	internal WordRWAdapterBase()
	{
	}

	protected void Read(IWordReader reader)
	{
		m_mainReader = reader;
		m_mainReader.ReadDocumentHeader(null);
		ReadBody(m_mainReader);
		m_mainReader.ReadDocumentEnd();
	}

	protected virtual void ReadBody(IWordReader reader)
	{
		ReadStyleSheet(reader);
		ReadSubDocumentBody(reader, WordSubdocument.Footnote);
		ReadSubDocumentBody(reader, WordSubdocument.Annotation);
		ReadSubDocumentBody(reader, WordSubdocument.Endnote);
		ReadShapeObjectsBody(reader);
		(reader as WordReader).UnfreezeStreamPos();
		m_textPos = reader.CurrentTextPosition;
		while (reader.ReadChunk() != WordChunkType.DocumentEnd)
		{
			ReadChunkBefore(reader);
			ReadChunk(reader);
		}
		ReadHFBody(reader);
	}

	protected abstract void ReadHFBody(IWordReader reader);

	protected abstract void ReadTextBoxBody(WordSubdocumentReader txbxReader, int txbxIndex);

	protected virtual void ReadChunk(IWordReaderBase baseReader)
	{
		IWordReader wordReader = baseReader as IWordReader;
		switch (wordReader.ChunkType)
		{
		case WordChunkType.SectionEnd:
			ReadSectionEnd(wordReader);
			break;
		case WordChunkType.PageBreak:
			ReadBreak(wordReader, BreakType.PageBreak);
			break;
		case WordChunkType.ColumnBreak:
			ReadBreak(wordReader, BreakType.ColumnBreak);
			break;
		case WordChunkType.Footnote:
			ReadFootnote(wordReader);
			break;
		case WordChunkType.Annotation:
			ReadAnnotation(wordReader);
			break;
		default:
			ReadChunkBase(wordReader);
			return;
		}
		m_textPos = wordReader.CurrentTextPosition;
	}

	protected virtual void ReadChunkBase(IWordReaderBase reader)
	{
		switch (reader.ChunkType)
		{
		case WordChunkType.Text:
			if (reader is IWordReader)
			{
				IWordReader wordReader2 = reader as IWordReader;
				if (wordReader2.IsEndnote || wordReader2.IsFootnote)
				{
					ReadFootnote(wordReader2);
					break;
				}
			}
			ReadText(reader);
			break;
		case WordChunkType.ParagraphEnd:
			ReadParagraphEnd(reader);
			break;
		case WordChunkType.Image:
			ReadImage(reader);
			break;
		case WordChunkType.Table:
			ReadTable(reader);
			break;
		case WordChunkType.TableRow:
			ReadTableRow(reader);
			break;
		case WordChunkType.TableCell:
			ReadTableCell(reader);
			break;
		case WordChunkType.FieldBeginMark:
			ReadField(reader);
			break;
		case WordChunkType.FieldEndMark:
			ReadFieldEnd(reader);
			break;
		case WordChunkType.LineBreak:
			ReadLineBreak(reader);
			break;
		case WordChunkType.Shape:
			ReadShape(reader);
			break;
		case WordChunkType.Symbol:
			if (reader is IWordReader)
			{
				IWordReader wordReader = reader as IWordReader;
				if (wordReader.IsEndnote || wordReader.IsFootnote)
				{
					ReadFootnote(wordReader);
					break;
				}
			}
			ReadSymbol(reader);
			break;
		case WordChunkType.Footnote:
			if (reader is WordFootnoteReader || reader is WordEndnoteReader)
			{
				ReadFootnoteMarker(reader);
			}
			break;
		case WordChunkType.CurrentPageNumber:
			ReadCurrentPageNumber(reader);
			break;
		}
		m_textPos = reader.CurrentTextPosition;
	}

	protected abstract void ReadSubDocumentBody(IWordReaderBase reader, WordSubdocument subDocument);

	protected abstract void ReadChunkBefore(IWordReaderBase reader);

	protected abstract void ReadPageBreak(IWordReader reader);

	protected abstract void ReadBreak(IWordReader reader, BreakType breakType);

	protected abstract void ReadSectionEnd(IWordReader reader);

	protected abstract void ReadField(IWordReaderBase reader);

	protected abstract void ReadTable(IWordReaderBase reader);

	protected abstract void ReadTableRow(IWordReaderBase reader);

	protected abstract void ReadTableCell(IWordReaderBase reader);

	protected abstract void ReadImage(IWordReaderBase reader);

	protected abstract void ReadParagraphEnd(IWordReaderBase reader);

	protected abstract void ReadText(IWordReaderBase reader);

	protected abstract void ReadStyleSheet(IWordReader reader);

	protected abstract void ReadLineBreak(IWordReaderBase reader);

	protected abstract void ReadShape(IWordReaderBase reader);

	protected abstract void ReadTextBoxShape(IWordReaderBase reader, TextBoxShape txtShape);

	protected abstract void ReadImageShape(IWordReaderBase reader, PictureShape imageShape);

	protected abstract void ReadShapeObjectsBody(IWordReader reader);

	protected abstract void ReadSymbol(IWordReaderBase reader);

	protected abstract void ReadFieldEnd(IWordReaderBase reader);

	protected abstract void ReadCurrentPageNumber(IWordReaderBase reader);

	protected abstract void ReadAnnotationBody(IWordReaderBase reader);

	protected abstract void ReadAnnotation(IWordReader reader);

	protected abstract void ReadFootnoteBody(IWordReaderBase reader);

	protected abstract void ReadFootnote(IWordReader reader);

	protected abstract void ReadFootnoteMarker(IWordReaderBase reader);
}
