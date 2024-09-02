namespace DocGen.Pdf.Barcode;

public class PdfCodabarBarcode : PdfUnidimensionalBarcode
{
	public PdfCodabarBarcode()
	{
		Initialize();
	}

	public PdfCodabarBarcode(string text)
		: this()
	{
		base.Text = text;
	}

	private void Initialize()
	{
		base.StartSymbol = 'A';
		base.StopSymbol = 'B';
		base.ValidatorExpression = "^[\\d\\-\\$\\:\\/\\.\\+]+$";
		base.BarcodeSymbols['0'] = new BarcodeSymbolTable('0', 0, new byte[7] { 1, 1, 1, 1, 1, 2, 2 });
		base.BarcodeSymbols['1'] = new BarcodeSymbolTable('1', 0, new byte[7] { 1, 1, 1, 1, 2, 2, 1 });
		base.BarcodeSymbols['2'] = new BarcodeSymbolTable('2', 0, new byte[7] { 1, 1, 1, 2, 1, 1, 2 });
		base.BarcodeSymbols['3'] = new BarcodeSymbolTable('3', 0, new byte[7] { 2, 2, 1, 1, 1, 1, 1 });
		base.BarcodeSymbols['4'] = new BarcodeSymbolTable('4', 0, new byte[7] { 1, 1, 2, 1, 1, 2, 1 });
		base.BarcodeSymbols['5'] = new BarcodeSymbolTable('5', 0, new byte[7] { 2, 1, 1, 1, 1, 2, 1 });
		base.BarcodeSymbols['6'] = new BarcodeSymbolTable('6', 0, new byte[7] { 1, 2, 1, 1, 1, 1, 2 });
		base.BarcodeSymbols['7'] = new BarcodeSymbolTable('7', 0, new byte[7] { 1, 2, 1, 1, 2, 1, 1 });
		base.BarcodeSymbols['8'] = new BarcodeSymbolTable('8', 0, new byte[7] { 1, 2, 2, 1, 1, 1, 1 });
		base.BarcodeSymbols['9'] = new BarcodeSymbolTable('9', 0, new byte[7] { 2, 1, 1, 2, 1, 1, 1 });
		base.BarcodeSymbols['-'] = new BarcodeSymbolTable('-', 0, new byte[7] { 1, 1, 1, 2, 2, 1, 1 });
		base.BarcodeSymbols['$'] = new BarcodeSymbolTable('$', 0, new byte[7] { 1, 1, 2, 2, 1, 1, 1 });
		base.BarcodeSymbols[':'] = new BarcodeSymbolTable(':', 0, new byte[7] { 2, 1, 1, 1, 2, 1, 2 });
		base.BarcodeSymbols['/'] = new BarcodeSymbolTable('/', 0, new byte[7] { 2, 1, 2, 1, 1, 1, 2 });
		base.BarcodeSymbols['.'] = new BarcodeSymbolTable('.', 0, new byte[7] { 2, 1, 2, 1, 2, 1, 1 });
		base.BarcodeSymbols['+'] = new BarcodeSymbolTable('+', 0, new byte[7] { 1, 1, 2, 1, 2, 1, 2 });
		base.BarcodeSymbols['A'] = new BarcodeSymbolTable('A', 0, new byte[7] { 1, 1, 2, 2, 1, 2, 1 });
		base.BarcodeSymbols['B'] = new BarcodeSymbolTable('B', 0, new byte[7] { 1, 1, 1, 2, 1, 2, 2 });
	}
}
