namespace DocGen.Pdf.Barcode;

internal struct PdfDataMatrixSymbolAttribute
{
	internal readonly int SymbolRow;

	internal readonly int SymbolColumn;

	internal readonly int HoriDataRegion;

	internal readonly int VertDataRegion;

	internal readonly int DataCodewords;

	internal readonly int CorrectionCodewords;

	internal readonly int InterleavedBlock;

	internal readonly int InterleavedDataBlock;

	internal PdfDataMatrixSymbolAttribute(int m_SymbolRow, int m_SymbolColumn, int m_horiDataRegions, int m_vertDataRegions, int m_dataCodewords, int m_correctionCodewords, int m_interleavedBlock, int m_interleavedDataBlock)
	{
		SymbolRow = m_SymbolRow;
		SymbolColumn = m_SymbolColumn;
		HoriDataRegion = m_horiDataRegions;
		VertDataRegion = m_vertDataRegions;
		DataCodewords = m_dataCodewords;
		CorrectionCodewords = m_correctionCodewords;
		InterleavedBlock = m_interleavedBlock;
		InterleavedDataBlock = m_interleavedDataBlock;
	}
}
