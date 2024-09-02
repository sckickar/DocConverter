using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DocGen.Pdf.Barcode;

public class PdfCode128ABarcode : PdfUnidimensionalBarcode
{
	public PdfCode128ABarcode()
	{
		base.EnableCheckDigit = true;
		Initialize();
	}

	public PdfCode128ABarcode(string text)
		: this()
	{
		base.Text = text;
	}

	protected internal override char[] CalculateCheckDigit()
	{
		if (!base.EnableCheckDigit)
		{
			return null;
		}
		int num = 0;
		string obj = (base.ExtendedText.Equals(string.Empty) ? base.Text : base.ExtendedText);
		int num2 = 0;
		string text = obj;
		foreach (char key in text)
		{
			BarcodeSymbolTable barcodeSymbolTable = base.BarcodeSymbols[key];
			if (barcodeSymbolTable == null)
			{
				throw new BarcodeException("Barcode Text contains characters that are not accepted by this barcode specification.");
			}
			num += barcodeSymbolTable.CheckDigit * (num2 + 1);
			num2++;
		}
		num += 103;
		num %= 103;
		return new char[1] { GetSymbol(num) };
	}

	protected internal override bool Validate(string data)
	{
		if (new Regex(base.ValidatorExpression).Matches(data).Count == data.Length)
		{
			return true;
		}
		return false;
	}

	protected void Initialize()
	{
		base.StartSymbol = 'ù';
		base.StopSymbol = 'ÿ';
		base.ValidatorExpression = "[\0\u0001\u0002\u0003\u0004\u0005\u0006\a\b\t\n\v\f\r\u000e\u000f\u0010\u0011\u0012\u0013\u0014\u0015\u0016\u0017\u0018\u0019\u001a\u001b\u001c\u001d\u001e\u001f !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_ðñòóô]";
		base.BarcodeSymbols['\0'] = new BarcodeSymbolTable('\0', 64, new byte[6] { 1, 1, 1, 4, 2, 2 });
		base.BarcodeSymbols['\u0001'] = new BarcodeSymbolTable('\u0001', 65, new byte[6] { 1, 2, 1, 1, 2, 4 });
		base.BarcodeSymbols['\u0002'] = new BarcodeSymbolTable('\u0002', 66, new byte[6] { 1, 2, 1, 4, 2, 1 });
		base.BarcodeSymbols['\u0003'] = new BarcodeSymbolTable('\u0003', 67, new byte[6] { 1, 4, 1, 1, 2, 2 });
		base.BarcodeSymbols['\u0004'] = new BarcodeSymbolTable('\u0004', 68, new byte[6] { 1, 4, 1, 2, 2, 1 });
		base.BarcodeSymbols['\u0005'] = new BarcodeSymbolTable('\u0005', 69, new byte[6] { 1, 1, 2, 2, 1, 4 });
		base.BarcodeSymbols['\u0006'] = new BarcodeSymbolTable('\u0006', 70, new byte[6] { 1, 1, 2, 4, 1, 2 });
		base.BarcodeSymbols['\a'] = new BarcodeSymbolTable('\a', 71, new byte[6] { 1, 2, 2, 1, 1, 4 });
		base.BarcodeSymbols['\b'] = new BarcodeSymbolTable('\b', 72, new byte[6] { 1, 2, 2, 4, 1, 1 });
		base.BarcodeSymbols['\t'] = new BarcodeSymbolTable('\t', 73, new byte[6] { 1, 4, 2, 1, 1, 2 });
		base.BarcodeSymbols['\n'] = new BarcodeSymbolTable('\n', 74, new byte[6] { 1, 4, 2, 2, 1, 1 });
		base.BarcodeSymbols['\v'] = new BarcodeSymbolTable('\v', 75, new byte[6] { 2, 4, 1, 2, 1, 1 });
		base.BarcodeSymbols['\f'] = new BarcodeSymbolTable('\f', 76, new byte[6] { 2, 2, 1, 1, 1, 4 });
		base.BarcodeSymbols['\r'] = new BarcodeSymbolTable('\r', 77, new byte[6] { 4, 1, 3, 1, 1, 1 });
		base.BarcodeSymbols['\u000e'] = new BarcodeSymbolTable('\u000e', 78, new byte[6] { 2, 4, 1, 1, 1, 2 });
		base.BarcodeSymbols['\u000f'] = new BarcodeSymbolTable('\u000f', 79, new byte[6] { 1, 3, 4, 1, 1, 1 });
		base.BarcodeSymbols['\u0010'] = new BarcodeSymbolTable('\u0010', 80, new byte[6] { 1, 1, 1, 2, 4, 2 });
		base.BarcodeSymbols['\u0011'] = new BarcodeSymbolTable('\u0011', 81, new byte[6] { 1, 2, 1, 1, 4, 2 });
		base.BarcodeSymbols['\u0012'] = new BarcodeSymbolTable('\u0012', 82, new byte[6] { 1, 2, 1, 2, 4, 1 });
		base.BarcodeSymbols['\u0013'] = new BarcodeSymbolTable('\u0013', 83, new byte[6] { 1, 1, 4, 2, 1, 2 });
		base.BarcodeSymbols['\u0014'] = new BarcodeSymbolTable('\u0014', 84, new byte[6] { 1, 2, 4, 1, 1, 2 });
		base.BarcodeSymbols['\u0015'] = new BarcodeSymbolTable('\u0015', 85, new byte[6] { 1, 2, 4, 2, 1, 1 });
		base.BarcodeSymbols['\u0016'] = new BarcodeSymbolTable('\u0016', 86, new byte[6] { 4, 1, 1, 2, 1, 2 });
		base.BarcodeSymbols['\u0017'] = new BarcodeSymbolTable('\u0017', 87, new byte[6] { 4, 2, 1, 1, 1, 2 });
		base.BarcodeSymbols['\u0018'] = new BarcodeSymbolTable('\u0018', 88, new byte[6] { 4, 2, 1, 2, 1, 1 });
		base.BarcodeSymbols['\u0019'] = new BarcodeSymbolTable('\u0019', 89, new byte[6] { 2, 1, 2, 1, 4, 1 });
		base.BarcodeSymbols['\u001a'] = new BarcodeSymbolTable('\u001a', 90, new byte[6] { 2, 1, 4, 1, 2, 1 });
		base.BarcodeSymbols['\u001b'] = new BarcodeSymbolTable('\u001b', 91, new byte[6] { 4, 1, 2, 1, 2, 1 });
		base.BarcodeSymbols['\u001c'] = new BarcodeSymbolTable('\u001c', 92, new byte[6] { 1, 1, 1, 1, 4, 3 });
		base.BarcodeSymbols['\u001d'] = new BarcodeSymbolTable('\u001d', 93, new byte[6] { 1, 1, 1, 3, 4, 1 });
		base.BarcodeSymbols['\u001e'] = new BarcodeSymbolTable('\u001e', 94, new byte[6] { 1, 3, 1, 1, 4, 1 });
		base.BarcodeSymbols['\u001f'] = new BarcodeSymbolTable('\u001f', 95, new byte[6] { 1, 1, 4, 1, 1, 3 });
		base.BarcodeSymbols[' '] = new BarcodeSymbolTable(' ', 0, new byte[6] { 2, 1, 2, 2, 2, 2 });
		base.BarcodeSymbols['!'] = new BarcodeSymbolTable('!', 1, new byte[6] { 2, 2, 2, 1, 2, 2 });
		base.BarcodeSymbols['"'] = new BarcodeSymbolTable('"', 2, new byte[6] { 2, 2, 2, 2, 2, 1 });
		base.BarcodeSymbols['#'] = new BarcodeSymbolTable('#', 3, new byte[6] { 1, 2, 1, 2, 2, 3 });
		base.BarcodeSymbols['$'] = new BarcodeSymbolTable('$', 4, new byte[6] { 1, 2, 1, 3, 2, 2 });
		base.BarcodeSymbols['%'] = new BarcodeSymbolTable('%', 5, new byte[6] { 1, 3, 1, 2, 2, 2 });
		base.BarcodeSymbols['&'] = new BarcodeSymbolTable('&', 6, new byte[6] { 1, 2, 2, 2, 1, 3 });
		base.BarcodeSymbols['\''] = new BarcodeSymbolTable('\'', 7, new byte[6] { 1, 2, 2, 3, 1, 2 });
		base.BarcodeSymbols['('] = new BarcodeSymbolTable('(', 8, new byte[6] { 1, 3, 2, 2, 1, 2 });
		base.BarcodeSymbols[')'] = new BarcodeSymbolTable(')', 9, new byte[6] { 2, 2, 1, 2, 1, 3 });
		base.BarcodeSymbols['*'] = new BarcodeSymbolTable('*', 10, new byte[6] { 2, 2, 1, 3, 1, 2 });
		base.BarcodeSymbols['+'] = new BarcodeSymbolTable('+', 11, new byte[6] { 2, 3, 1, 2, 1, 2 });
		base.BarcodeSymbols[','] = new BarcodeSymbolTable(',', 12, new byte[6] { 1, 1, 2, 2, 3, 2 });
		base.BarcodeSymbols['-'] = new BarcodeSymbolTable('-', 13, new byte[6] { 1, 2, 2, 1, 3, 2 });
		base.BarcodeSymbols['.'] = new BarcodeSymbolTable('.', 14, new byte[6] { 1, 2, 2, 2, 3, 1 });
		base.BarcodeSymbols['/'] = new BarcodeSymbolTable('/', 15, new byte[6] { 1, 1, 3, 2, 2, 2 });
		base.BarcodeSymbols['0'] = new BarcodeSymbolTable('0', 16, new byte[6] { 1, 2, 3, 1, 2, 2 });
		base.BarcodeSymbols['1'] = new BarcodeSymbolTable('1', 17, new byte[6] { 1, 2, 3, 2, 2, 1 });
		base.BarcodeSymbols['2'] = new BarcodeSymbolTable('2', 18, new byte[6] { 2, 2, 3, 2, 1, 1 });
		base.BarcodeSymbols['3'] = new BarcodeSymbolTable('3', 19, new byte[6] { 2, 2, 1, 1, 3, 2 });
		base.BarcodeSymbols['4'] = new BarcodeSymbolTable('4', 20, new byte[6] { 2, 2, 1, 2, 3, 1 });
		base.BarcodeSymbols['5'] = new BarcodeSymbolTable('5', 21, new byte[6] { 2, 1, 3, 2, 1, 2 });
		base.BarcodeSymbols['6'] = new BarcodeSymbolTable('6', 22, new byte[6] { 2, 2, 3, 1, 1, 2 });
		base.BarcodeSymbols['7'] = new BarcodeSymbolTable('7', 23, new byte[6] { 3, 1, 2, 1, 3, 1 });
		base.BarcodeSymbols['8'] = new BarcodeSymbolTable('8', 24, new byte[6] { 3, 1, 1, 2, 2, 2 });
		base.BarcodeSymbols['9'] = new BarcodeSymbolTable('9', 25, new byte[6] { 3, 2, 1, 1, 2, 2 });
		base.BarcodeSymbols[':'] = new BarcodeSymbolTable(':', 26, new byte[6] { 3, 2, 1, 2, 2, 1 });
		base.BarcodeSymbols[';'] = new BarcodeSymbolTable(';', 27, new byte[6] { 3, 1, 2, 2, 1, 2 });
		base.BarcodeSymbols['<'] = new BarcodeSymbolTable('<', 28, new byte[6] { 3, 2, 2, 1, 1, 2 });
		base.BarcodeSymbols['='] = new BarcodeSymbolTable('=', 29, new byte[6] { 3, 2, 2, 2, 1, 1 });
		base.BarcodeSymbols['>'] = new BarcodeSymbolTable('>', 30, new byte[6] { 2, 1, 2, 1, 2, 3 });
		base.BarcodeSymbols['?'] = new BarcodeSymbolTable('?', 31, new byte[6] { 2, 1, 2, 3, 2, 1 });
		base.BarcodeSymbols['@'] = new BarcodeSymbolTable('@', 32, new byte[6] { 2, 3, 2, 1, 2, 1 });
		base.BarcodeSymbols['A'] = new BarcodeSymbolTable('A', 33, new byte[6] { 1, 1, 1, 3, 2, 3 });
		base.BarcodeSymbols['B'] = new BarcodeSymbolTable('B', 34, new byte[6] { 1, 3, 1, 1, 2, 3 });
		base.BarcodeSymbols['C'] = new BarcodeSymbolTable('C', 35, new byte[6] { 1, 3, 1, 3, 2, 1 });
		base.BarcodeSymbols['D'] = new BarcodeSymbolTable('D', 36, new byte[6] { 1, 1, 2, 3, 1, 3 });
		base.BarcodeSymbols['E'] = new BarcodeSymbolTable('E', 37, new byte[6] { 1, 3, 2, 1, 1, 3 });
		base.BarcodeSymbols['F'] = new BarcodeSymbolTable('F', 38, new byte[6] { 1, 3, 2, 3, 1, 1 });
		base.BarcodeSymbols['G'] = new BarcodeSymbolTable('G', 39, new byte[6] { 2, 1, 1, 3, 1, 3 });
		base.BarcodeSymbols['H'] = new BarcodeSymbolTable('H', 40, new byte[6] { 2, 3, 1, 1, 1, 3 });
		base.BarcodeSymbols['I'] = new BarcodeSymbolTable('I', 41, new byte[6] { 2, 3, 1, 3, 1, 1 });
		base.BarcodeSymbols['J'] = new BarcodeSymbolTable('J', 42, new byte[6] { 1, 1, 2, 1, 3, 3 });
		base.BarcodeSymbols['K'] = new BarcodeSymbolTable('K', 43, new byte[6] { 1, 1, 2, 3, 3, 1 });
		base.BarcodeSymbols['L'] = new BarcodeSymbolTable('L', 44, new byte[6] { 1, 3, 2, 1, 3, 1 });
		base.BarcodeSymbols['M'] = new BarcodeSymbolTable('M', 45, new byte[6] { 1, 1, 3, 1, 2, 3 });
		base.BarcodeSymbols['N'] = new BarcodeSymbolTable('N', 46, new byte[6] { 1, 1, 3, 3, 2, 1 });
		base.BarcodeSymbols['O'] = new BarcodeSymbolTable('O', 47, new byte[6] { 1, 3, 3, 1, 2, 1 });
		base.BarcodeSymbols['P'] = new BarcodeSymbolTable('P', 48, new byte[6] { 3, 1, 3, 1, 2, 1 });
		base.BarcodeSymbols['Q'] = new BarcodeSymbolTable('Q', 49, new byte[6] { 2, 1, 1, 3, 3, 1 });
		base.BarcodeSymbols['R'] = new BarcodeSymbolTable('R', 50, new byte[6] { 2, 3, 1, 1, 3, 1 });
		base.BarcodeSymbols['S'] = new BarcodeSymbolTable('S', 51, new byte[6] { 2, 1, 3, 1, 1, 3 });
		base.BarcodeSymbols['T'] = new BarcodeSymbolTable('T', 52, new byte[6] { 2, 1, 3, 3, 1, 1 });
		base.BarcodeSymbols['U'] = new BarcodeSymbolTable('U', 53, new byte[6] { 2, 1, 3, 1, 3, 1 });
		base.BarcodeSymbols['V'] = new BarcodeSymbolTable('V', 54, new byte[6] { 3, 1, 1, 1, 2, 3 });
		base.BarcodeSymbols['W'] = new BarcodeSymbolTable('W', 55, new byte[6] { 3, 1, 1, 3, 2, 1 });
		base.BarcodeSymbols['X'] = new BarcodeSymbolTable('X', 56, new byte[6] { 3, 3, 1, 1, 2, 1 });
		base.BarcodeSymbols['Y'] = new BarcodeSymbolTable('Y', 57, new byte[6] { 3, 1, 2, 1, 1, 3 });
		base.BarcodeSymbols['Z'] = new BarcodeSymbolTable('Z', 58, new byte[6] { 3, 1, 2, 3, 1, 1 });
		base.BarcodeSymbols['['] = new BarcodeSymbolTable('[', 59, new byte[6] { 3, 3, 2, 1, 1, 1 });
		base.BarcodeSymbols['\\'] = new BarcodeSymbolTable('\\', 60, new byte[6] { 3, 1, 4, 1, 1, 1 });
		base.BarcodeSymbols[']'] = new BarcodeSymbolTable(']', 61, new byte[6] { 2, 2, 1, 4, 1, 1 });
		base.BarcodeSymbols['^'] = new BarcodeSymbolTable('^', 62, new byte[6] { 4, 3, 1, 1, 1, 1 });
		base.BarcodeSymbols['_'] = new BarcodeSymbolTable('_', 63, new byte[6] { 1, 1, 1, 2, 2, 4 });
		base.BarcodeSymbols['ð'] = new BarcodeSymbolTable('ð', 102, new byte[6] { 4, 1, 1, 1, 3, 1 });
		base.BarcodeSymbols['ñ'] = new BarcodeSymbolTable('ñ', 97, new byte[6] { 4, 1, 1, 1, 1, 3 });
		base.BarcodeSymbols['ò'] = new BarcodeSymbolTable('ò', 96, new byte[6] { 1, 1, 4, 3, 1, 1 });
		base.BarcodeSymbols['ó'] = new BarcodeSymbolTable('ó', 101, new byte[6] { 3, 1, 1, 1, 4, 1 });
		base.BarcodeSymbols['ô'] = new BarcodeSymbolTable('ô', 98, new byte[6] { 4, 1, 1, 3, 1, 1 });
		base.BarcodeSymbols['ü'] = new BarcodeSymbolTable('ü', 99, new byte[6] { 1, 1, 3, 1, 4, 1 });
		base.BarcodeSymbols['û'] = new BarcodeSymbolTable('û', 100, new byte[6] { 1, 1, 4, 1, 3, 1 });
		base.BarcodeSymbols['ù'] = new BarcodeSymbolTable('ù', 103, new byte[6] { 2, 1, 1, 4, 1, 2 });
		base.BarcodeSymbols['ÿ'] = new BarcodeSymbolTable('ÿ', -1, new byte[7] { 2, 3, 3, 1, 1, 1, 2 });
	}

	internal char GetSymbol(int checkValue)
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
