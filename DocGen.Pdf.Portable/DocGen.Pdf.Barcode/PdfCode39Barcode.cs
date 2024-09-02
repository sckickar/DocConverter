using System.Collections.Generic;

namespace DocGen.Pdf.Barcode;

public class PdfCode39Barcode : PdfUnidimensionalBarcode
{
	public PdfCode39Barcode()
	{
		if (!(this is PdfCode32Barcode))
		{
			Initialize();
		}
	}

	public PdfCode39Barcode(string text)
		: this()
	{
		base.Text = text;
	}

	internal void Initialize()
	{
		base.StartSymbol = '*';
		base.StopSymbol = '*';
		base.ValidatorExpression = "^[\\x41-\\x5A\\x30-\\x39\\x20\\-\\.\\$\\/\\+\\%\\ ]+$";
		base.BarcodeSymbols['0'] = new BarcodeSymbolTable('0', 0, new byte[9] { 1, 1, 1, 3, 3, 1, 3, 1, 1 });
		base.BarcodeSymbols['1'] = new BarcodeSymbolTable('1', 1, new byte[9] { 3, 1, 1, 3, 1, 1, 1, 1, 3 });
		base.BarcodeSymbols['2'] = new BarcodeSymbolTable('2', 2, new byte[9] { 1, 1, 3, 3, 1, 1, 1, 1, 3 });
		base.BarcodeSymbols['3'] = new BarcodeSymbolTable('3', 3, new byte[9] { 3, 1, 3, 3, 1, 1, 1, 1, 1 });
		base.BarcodeSymbols['4'] = new BarcodeSymbolTable('4', 4, new byte[9] { 1, 1, 1, 3, 3, 1, 1, 1, 3 });
		base.BarcodeSymbols['5'] = new BarcodeSymbolTable('5', 5, new byte[9] { 3, 1, 1, 3, 3, 1, 1, 1, 1 });
		base.BarcodeSymbols['6'] = new BarcodeSymbolTable('6', 6, new byte[9] { 1, 1, 3, 3, 3, 1, 1, 1, 1 });
		base.BarcodeSymbols['7'] = new BarcodeSymbolTable('7', 7, new byte[9] { 1, 1, 1, 3, 1, 1, 3, 1, 3 });
		base.BarcodeSymbols['8'] = new BarcodeSymbolTable('8', 8, new byte[9] { 3, 1, 1, 3, 1, 1, 3, 1, 1 });
		base.BarcodeSymbols['9'] = new BarcodeSymbolTable('9', 9, new byte[9] { 1, 1, 3, 3, 1, 1, 3, 1, 1 });
		base.BarcodeSymbols['A'] = new BarcodeSymbolTable('A', 10, new byte[9] { 3, 1, 1, 1, 1, 3, 1, 1, 3 });
		base.BarcodeSymbols['B'] = new BarcodeSymbolTable('B', 11, new byte[9] { 1, 1, 3, 1, 1, 3, 1, 1, 3 });
		base.BarcodeSymbols['C'] = new BarcodeSymbolTable('C', 12, new byte[9] { 3, 1, 3, 1, 1, 3, 1, 1, 1 });
		base.BarcodeSymbols['D'] = new BarcodeSymbolTable('D', 13, new byte[9] { 1, 1, 1, 1, 3, 3, 1, 1, 3 });
		base.BarcodeSymbols['E'] = new BarcodeSymbolTable('E', 14, new byte[9] { 3, 1, 1, 1, 3, 3, 1, 1, 1 });
		base.BarcodeSymbols['F'] = new BarcodeSymbolTable('F', 15, new byte[9] { 1, 1, 3, 1, 3, 3, 1, 1, 1 });
		base.BarcodeSymbols['G'] = new BarcodeSymbolTable('G', 16, new byte[9] { 1, 1, 1, 1, 1, 3, 3, 1, 3 });
		base.BarcodeSymbols['H'] = new BarcodeSymbolTable('H', 17, new byte[9] { 3, 1, 1, 1, 1, 3, 3, 1, 1 });
		base.BarcodeSymbols['I'] = new BarcodeSymbolTable('I', 18, new byte[9] { 1, 1, 3, 1, 1, 3, 3, 1, 1 });
		base.BarcodeSymbols['J'] = new BarcodeSymbolTable('J', 19, new byte[9] { 1, 1, 1, 1, 3, 3, 3, 1, 1 });
		base.BarcodeSymbols['K'] = new BarcodeSymbolTable('K', 20, new byte[9] { 3, 1, 1, 1, 1, 1, 1, 3, 3 });
		base.BarcodeSymbols['L'] = new BarcodeSymbolTable('L', 21, new byte[9] { 1, 1, 3, 1, 1, 1, 1, 3, 3 });
		base.BarcodeSymbols['M'] = new BarcodeSymbolTable('M', 22, new byte[9] { 3, 1, 3, 1, 1, 1, 1, 3, 1 });
		base.BarcodeSymbols['N'] = new BarcodeSymbolTable('N', 23, new byte[9] { 1, 1, 1, 1, 3, 1, 1, 3, 3 });
		base.BarcodeSymbols['O'] = new BarcodeSymbolTable('O', 24, new byte[9] { 3, 1, 1, 1, 3, 1, 1, 3, 1 });
		base.BarcodeSymbols['P'] = new BarcodeSymbolTable('P', 25, new byte[9] { 1, 1, 3, 1, 3, 1, 1, 3, 1 });
		base.BarcodeSymbols['Q'] = new BarcodeSymbolTable('Q', 26, new byte[9] { 1, 1, 1, 1, 1, 1, 3, 3, 3 });
		base.BarcodeSymbols['R'] = new BarcodeSymbolTable('R', 27, new byte[9] { 3, 1, 1, 1, 1, 1, 3, 3, 1 });
		base.BarcodeSymbols['S'] = new BarcodeSymbolTable('S', 28, new byte[9] { 1, 1, 3, 1, 1, 1, 3, 3, 1 });
		base.BarcodeSymbols['T'] = new BarcodeSymbolTable('T', 29, new byte[9] { 1, 1, 1, 1, 3, 1, 3, 3, 1 });
		base.BarcodeSymbols['U'] = new BarcodeSymbolTable('U', 30, new byte[9] { 3, 3, 1, 1, 1, 1, 1, 1, 3 });
		base.BarcodeSymbols['V'] = new BarcodeSymbolTable('V', 31, new byte[9] { 1, 3, 3, 1, 1, 1, 1, 1, 3 });
		base.BarcodeSymbols['W'] = new BarcodeSymbolTable('W', 32, new byte[9] { 3, 3, 3, 1, 1, 1, 1, 1, 1 });
		base.BarcodeSymbols['X'] = new BarcodeSymbolTable('X', 33, new byte[9] { 1, 3, 1, 1, 3, 1, 1, 1, 3 });
		base.BarcodeSymbols['Y'] = new BarcodeSymbolTable('Y', 34, new byte[9] { 3, 3, 1, 1, 3, 1, 1, 1, 1 });
		base.BarcodeSymbols['Z'] = new BarcodeSymbolTable('Z', 35, new byte[9] { 1, 3, 3, 1, 3, 1, 1, 1, 1 });
		base.BarcodeSymbols['-'] = new BarcodeSymbolTable('-', 36, new byte[9] { 1, 3, 1, 1, 1, 1, 3, 1, 3 });
		base.BarcodeSymbols['.'] = new BarcodeSymbolTable('.', 37, new byte[9] { 3, 3, 1, 1, 1, 1, 3, 1, 1 });
		base.BarcodeSymbols[' '] = new BarcodeSymbolTable(' ', 38, new byte[9] { 1, 3, 3, 1, 1, 1, 3, 1, 1 });
		base.BarcodeSymbols['$'] = new BarcodeSymbolTable('$', 39, new byte[9] { 1, 3, 1, 3, 1, 3, 1, 1, 1 });
		base.BarcodeSymbols['/'] = new BarcodeSymbolTable('/', 40, new byte[9] { 1, 3, 1, 3, 1, 1, 1, 3, 1 });
		base.BarcodeSymbols['+'] = new BarcodeSymbolTable('+', 41, new byte[9] { 1, 3, 1, 1, 1, 3, 1, 3, 1 });
		base.BarcodeSymbols['%'] = new BarcodeSymbolTable('%', 42, new byte[9] { 1, 1, 1, 3, 1, 3, 1, 3, 1 });
		base.BarcodeSymbols['*'] = new BarcodeSymbolTable('*', 0, new byte[9] { 1, 3, 1, 1, 3, 1, 3, 1, 1 });
	}

	protected internal override char[] CalculateCheckDigit()
	{
		if (!base.EnableCheckDigit)
		{
			return null;
		}
		int num = 0;
		string text = (base.ExtendedText.Equals(string.Empty) ? base.Text : base.ExtendedText);
		foreach (char key in text)
		{
			BarcodeSymbolTable barcodeSymbolTable = base.BarcodeSymbols[key];
			num += barcodeSymbolTable.CheckDigit;
		}
		num %= base.BarcodeSymbols.Count - 1;
		return new char[1] { GetSymbol(num) };
	}

	private char GetSymbol(int checkValue)
	{
		foreach (KeyValuePair<char, BarcodeSymbolTable> barcodeSymbol in base.BarcodeSymbols)
		{
			BarcodeSymbolTable value = barcodeSymbol.Value;
			if (value.CheckDigit == checkValue)
			{
				return value.Symbol;
			}
		}
		return '\0';
	}
}
