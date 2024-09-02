using System.Collections.Generic;
using System.Text;

namespace DocGen.Pdf.Barcode;

public class PdfCode39ExtendedBarcode : PdfCode39Barcode
{
	private Dictionary<char, char[]> extendedCodes;

	public PdfCode39ExtendedBarcode()
	{
		InitializeCode39Extended();
	}

	public PdfCode39ExtendedBarcode(string text)
		: this()
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < text.Length; i++)
		{
			char[] array = extendedCodes[text[i]];
			if (array != null)
			{
				for (int j = 0; j < array.Length; j++)
				{
					stringBuilder.Append(array[j]);
				}
			}
		}
		base.ExtendedText = stringBuilder.ToString();
		base.Text = text;
	}

	protected internal override char[] CalculateCheckDigit()
	{
		if (!base.EnableCheckDigit)
		{
			return null;
		}
		int num = 0;
		GetExtendedTextValue();
		string text = (base.ExtendedText.Equals(string.Empty) ? base.Text : base.ExtendedText);
		foreach (char key in text)
		{
			BarcodeSymbolTable barcodeSymbolTable = base.BarcodeSymbols[key];
			num += barcodeSymbolTable.CheckDigit;
		}
		num %= base.BarcodeSymbols.Count - 1;
		return new char[1] { GetSymbol(num) };
	}

	private void InitializeCode39Extended()
	{
		Initialize();
		base.ValidatorExpression = "^[\\x00-\\x7F]+$";
		extendedCodes = new Dictionary<char, char[]>();
		extendedCodes['\0'] = new char[2] { '%', 'U' };
		extendedCodes['\u0001'] = new char[2] { '$', 'A' };
		extendedCodes['\u0002'] = new char[2] { '$', 'B' };
		extendedCodes['\u0003'] = new char[2] { '$', 'C' };
		extendedCodes['\u0004'] = new char[2] { '$', 'D' };
		extendedCodes['\u0005'] = new char[2] { '$', 'E' };
		extendedCodes['\u0006'] = new char[2] { '$', 'F' };
		extendedCodes['\a'] = new char[2] { '$', 'G' };
		extendedCodes['\b'] = new char[2] { '$', 'H' };
		extendedCodes['\t'] = new char[2] { '$', 'I' };
		extendedCodes['\n'] = new char[2] { '$', 'J' };
		extendedCodes['\v'] = new char[2] { '$', 'K' };
		extendedCodes['\f'] = new char[2] { '$', 'L' };
		extendedCodes['\r'] = new char[2] { '$', 'M' };
		extendedCodes['\u000e'] = new char[2] { '$', 'N' };
		extendedCodes['\u000f'] = new char[2] { '$', 'O' };
		extendedCodes['\u0010'] = new char[2] { '$', 'P' };
		extendedCodes['\u0011'] = new char[2] { '$', 'Q' };
		extendedCodes['\u0012'] = new char[2] { '$', 'R' };
		extendedCodes['\u0013'] = new char[2] { '$', 'S' };
		extendedCodes['\u0014'] = new char[2] { '$', 'T' };
		extendedCodes['\u0015'] = new char[2] { '$', 'U' };
		extendedCodes['\u0016'] = new char[2] { '$', 'V' };
		extendedCodes['\u0017'] = new char[2] { '$', 'W' };
		extendedCodes['\u0018'] = new char[2] { '$', 'X' };
		extendedCodes['\u0019'] = new char[2] { '$', 'Y' };
		extendedCodes['\u001a'] = new char[2] { '$', 'Z' };
		extendedCodes['\u001b'] = new char[2] { '%', 'A' };
		extendedCodes['\u001c'] = new char[2] { '%', 'B' };
		extendedCodes['\u001d'] = new char[2] { '%', 'C' };
		extendedCodes['\u001e'] = new char[2] { '%', 'D' };
		extendedCodes['\u001f'] = new char[2] { '%', 'E' };
		extendedCodes[' '] = new char[1] { ' ' };
		extendedCodes['!'] = new char[2] { '/', 'A' };
		extendedCodes['"'] = new char[2] { '/', 'B' };
		extendedCodes['#'] = new char[2] { '/', 'C' };
		extendedCodes['$'] = new char[2] { '/', 'D' };
		extendedCodes['%'] = new char[2] { '/', 'E' };
		extendedCodes['&'] = new char[2] { '/', 'F' };
		extendedCodes['\''] = new char[2] { '/', 'G' };
		extendedCodes['('] = new char[2] { '/', 'H' };
		extendedCodes[')'] = new char[2] { '/', 'I' };
		extendedCodes['*'] = new char[2] { '/', 'J' };
		extendedCodes['+'] = new char[2] { '/', 'K' };
		extendedCodes[','] = new char[2] { '/', 'L' };
		extendedCodes['-'] = new char[1] { '-' };
		extendedCodes['.'] = new char[1] { '.' };
		extendedCodes['/'] = new char[2] { '/', 'O' };
		extendedCodes['0'] = new char[1] { '0' };
		extendedCodes['1'] = new char[1] { '1' };
		extendedCodes['2'] = new char[1] { '2' };
		extendedCodes['3'] = new char[1] { '3' };
		extendedCodes['4'] = new char[1] { '4' };
		extendedCodes['5'] = new char[1] { '5' };
		extendedCodes['6'] = new char[1] { '6' };
		extendedCodes['7'] = new char[1] { '7' };
		extendedCodes['8'] = new char[1] { '8' };
		extendedCodes['9'] = new char[1] { '9' };
		extendedCodes[':'] = new char[2] { '/', 'Z' };
		extendedCodes[';'] = new char[2] { '%', 'F' };
		extendedCodes['<'] = new char[2] { '%', 'G' };
		extendedCodes['='] = new char[2] { '%', 'H' };
		extendedCodes['>'] = new char[2] { '%', 'I' };
		extendedCodes['?'] = new char[2] { '%', 'J' };
		extendedCodes['@'] = new char[2] { '%', 'V' };
		extendedCodes['A'] = new char[1] { 'A' };
		extendedCodes['B'] = new char[1] { 'B' };
		extendedCodes['C'] = new char[1] { 'C' };
		extendedCodes['D'] = new char[1] { 'D' };
		extendedCodes['E'] = new char[1] { 'E' };
		extendedCodes['F'] = new char[1] { 'F' };
		extendedCodes['G'] = new char[1] { 'G' };
		extendedCodes['H'] = new char[1] { 'H' };
		extendedCodes['I'] = new char[1] { 'I' };
		extendedCodes['J'] = new char[1] { 'J' };
		extendedCodes['K'] = new char[1] { 'K' };
		extendedCodes['L'] = new char[1] { 'L' };
		extendedCodes['M'] = new char[1] { 'M' };
		extendedCodes['N'] = new char[1] { 'N' };
		extendedCodes['O'] = new char[1] { 'O' };
		extendedCodes['P'] = new char[1] { 'P' };
		extendedCodes['Q'] = new char[1] { 'Q' };
		extendedCodes['R'] = new char[1] { 'R' };
		extendedCodes['S'] = new char[1] { 'S' };
		extendedCodes['T'] = new char[1] { 'T' };
		extendedCodes['U'] = new char[1] { 'U' };
		extendedCodes['V'] = new char[1] { 'V' };
		extendedCodes['W'] = new char[1] { 'W' };
		extendedCodes['X'] = new char[1] { 'X' };
		extendedCodes['Y'] = new char[1] { 'Y' };
		extendedCodes['Z'] = new char[1] { 'Z' };
		extendedCodes['['] = new char[2] { '%', 'K' };
		extendedCodes['\\'] = new char[2] { '%', 'L' };
		extendedCodes[']'] = new char[2] { '%', 'M' };
		extendedCodes['^'] = new char[2] { '%', 'N' };
		extendedCodes['_'] = new char[2] { '%', 'O' };
		extendedCodes['`'] = new char[2] { '%', 'W' };
		extendedCodes['a'] = new char[2] { '+', 'A' };
		extendedCodes['b'] = new char[2] { '+', 'B' };
		extendedCodes['c'] = new char[2] { '+', 'C' };
		extendedCodes['d'] = new char[2] { '+', 'D' };
		extendedCodes['e'] = new char[2] { '+', 'E' };
		extendedCodes['f'] = new char[2] { '+', 'F' };
		extendedCodes['g'] = new char[2] { '+', 'G' };
		extendedCodes['h'] = new char[2] { '+', 'H' };
		extendedCodes['i'] = new char[2] { '+', 'I' };
		extendedCodes['j'] = new char[2] { '+', 'J' };
		extendedCodes['k'] = new char[2] { '+', 'K' };
		extendedCodes['l'] = new char[2] { '+', 'L' };
		extendedCodes['m'] = new char[2] { '+', 'M' };
		extendedCodes['n'] = new char[2] { '+', 'N' };
		extendedCodes['o'] = new char[2] { '+', 'O' };
		extendedCodes['p'] = new char[2] { '+', 'P' };
		extendedCodes['q'] = new char[2] { '+', 'Q' };
		extendedCodes['r'] = new char[2] { '+', 'R' };
		extendedCodes['s'] = new char[2] { '+', 'S' };
		extendedCodes['t'] = new char[2] { '+', 'T' };
		extendedCodes['u'] = new char[2] { '+', 'U' };
		extendedCodes['v'] = new char[2] { '+', 'V' };
		extendedCodes['w'] = new char[2] { '+', 'W' };
		extendedCodes['x'] = new char[2] { '+', 'X' };
		extendedCodes['y'] = new char[2] { '+', 'Y' };
		extendedCodes['z'] = new char[2] { '+', 'Z' };
		extendedCodes['{'] = new char[2] { '%', 'P' };
		extendedCodes['|'] = new char[2] { '%', 'Q' };
		extendedCodes['}'] = new char[2] { '%', 'R' };
		extendedCodes['~'] = new char[2] { '%', 'S' };
		extendedCodes['\u007f'] = new char[2] { '%', 'T' };
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

	protected internal override void GetExtendedTextValue()
	{
		string obj = base.Text;
		string text = "";
		string text2 = obj;
		foreach (char key in text2)
		{
			char[] array = extendedCodes[key];
			foreach (char c in array)
			{
				text += c;
			}
		}
		base.ExtendedText = text;
	}
}
