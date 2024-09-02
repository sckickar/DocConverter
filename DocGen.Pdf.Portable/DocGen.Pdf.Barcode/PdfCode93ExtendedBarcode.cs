using System.Collections.Generic;
using System.Text;

namespace DocGen.Pdf.Barcode;

public class PdfCode93ExtendedBarcode : PdfCode93Barcode
{
	private Dictionary<char, char[]> extendedCodes;

	public PdfCode93ExtendedBarcode()
	{
		InitializeCode93Extended();
	}

	public PdfCode93ExtendedBarcode(string text)
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
		GetExtendedText();
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
		_ = new char[1];
		return GetCheckSumSymbols();
	}

	protected internal new char[] GetCheckSumSymbols()
	{
		string text = base.ExtendedText;
		char[] array = new char[2];
		int num = 0;
		string text2 = text;
		int length = text2.Length;
		for (int i = 0; i < length; i++)
		{
			int num2 = (length - i) % 20;
			if (num2 == 0)
			{
				num2 = 20;
			}
			int checkDigit = base.BarcodeSymbols[text2[i]].CheckDigit;
			num += checkDigit * num2;
		}
		num %= 47;
		char c = ' ';
		foreach (KeyValuePair<char, BarcodeSymbolTable> barcodeSymbol in base.BarcodeSymbols)
		{
			BarcodeSymbolTable value = barcodeSymbol.Value;
			if (value.CheckDigit == num)
			{
				c = value.Symbol;
				break;
			}
		}
		string text3 = base.ExtendedText;
		text3 += c;
		array[0] = c;
		text = text3;
		num = 0;
		text2 = text;
		length = text2.Length;
		for (int j = 0; j < length; j++)
		{
			int num3 = (length - j) % 15;
			if (num3 == 0)
			{
				num3 = 15;
			}
			int checkDigit2 = base.BarcodeSymbols[text2[j]].CheckDigit;
			num += checkDigit2 * num3;
		}
		num %= 47;
		text += num;
		char c2 = ' ';
		foreach (KeyValuePair<char, BarcodeSymbolTable> barcodeSymbol2 in base.BarcodeSymbols)
		{
			BarcodeSymbolTable value2 = barcodeSymbol2.Value;
			if (value2.CheckDigit == num)
			{
				c2 = value2.Symbol;
				break;
			}
		}
		text3 += c2;
		array[1] = c2;
		return array;
	}

	private void InitializeCode93Extended()
	{
		Initialize();
		base.ValidatorExpression = "^[\\x00-\\x7F\\x00fb\\x00fd\\x00fe\\'þ'\\'ü'\\'ý']+$";
		extendedCodes = new Dictionary<char, char[]>();
		extendedCodes['\0'] = new char[2] { 'ü', 'U' };
		extendedCodes['\u0001'] = new char[2] { 'û', 'A' };
		extendedCodes['\u0002'] = new char[2] { 'û', 'B' };
		extendedCodes['\u0003'] = new char[2] { 'û', 'C' };
		extendedCodes['\u0004'] = new char[2] { 'û', 'D' };
		extendedCodes['\u0005'] = new char[2] { 'û', 'E' };
		extendedCodes['\u0006'] = new char[2] { 'û', 'F' };
		extendedCodes['\a'] = new char[2] { 'û', 'G' };
		extendedCodes['\b'] = new char[2] { 'û', 'H' };
		extendedCodes['\t'] = new char[2] { 'û', 'I' };
		extendedCodes['\n'] = new char[2] { 'û', 'J' };
		extendedCodes['\v'] = new char[2] { 'û', 'K' };
		extendedCodes['\f'] = new char[2] { 'û', 'L' };
		extendedCodes['\r'] = new char[2] { 'û', 'M' };
		extendedCodes['\u000e'] = new char[2] { 'û', 'N' };
		extendedCodes['\u000f'] = new char[2] { 'û', 'O' };
		extendedCodes['\u0010'] = new char[2] { 'û', 'P' };
		extendedCodes['\u0011'] = new char[2] { 'û', 'Q' };
		extendedCodes['\u0012'] = new char[2] { 'û', 'R' };
		extendedCodes['\u0013'] = new char[2] { 'û', 'S' };
		extendedCodes['\u0014'] = new char[2] { 'û', 'T' };
		extendedCodes['\u0015'] = new char[2] { 'û', 'U' };
		extendedCodes['\u0016'] = new char[2] { 'û', 'V' };
		extendedCodes['\u0017'] = new char[2] { 'û', 'W' };
		extendedCodes['\u0018'] = new char[2] { 'û', 'X' };
		extendedCodes['\u0019'] = new char[2] { 'û', 'Y' };
		extendedCodes['\u001a'] = new char[2] { 'û', 'Z' };
		extendedCodes['\u001b'] = new char[2] { 'ü', 'A' };
		extendedCodes['\u001c'] = new char[2] { 'ü', 'B' };
		extendedCodes['\u001d'] = new char[2] { 'ü', 'C' };
		extendedCodes['\u001e'] = new char[2] { 'ü', 'D' };
		extendedCodes['\u001f'] = new char[2] { 'ü', 'E' };
		extendedCodes[' '] = new char[1] { ' ' };
		extendedCodes['!'] = new char[2] { 'ý', 'A' };
		extendedCodes['"'] = new char[2] { 'ý', 'B' };
		extendedCodes['#'] = new char[2] { 'ý', 'C' };
		extendedCodes['$'] = new char[1] { '$' };
		extendedCodes['%'] = new char[1] { '%' };
		extendedCodes['&'] = new char[2] { 'ý', 'F' };
		extendedCodes['\''] = new char[2] { 'ý', 'G' };
		extendedCodes['('] = new char[2] { 'ý', 'H' };
		extendedCodes[')'] = new char[2] { 'ý', 'I' };
		extendedCodes['*'] = new char[2] { 'ý', 'J' };
		extendedCodes['+'] = new char[1] { '+' };
		extendedCodes[','] = new char[2] { 'ý', 'L' };
		extendedCodes['-'] = new char[1] { '-' };
		extendedCodes['.'] = new char[1] { '.' };
		extendedCodes['/'] = new char[1] { '/' };
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
		extendedCodes[':'] = new char[2] { 'ý', 'Z' };
		extendedCodes[';'] = new char[2] { 'ü', 'F' };
		extendedCodes['<'] = new char[2] { 'ü', 'G' };
		extendedCodes['='] = new char[2] { 'ü', 'H' };
		extendedCodes['>'] = new char[2] { 'ü', 'I' };
		extendedCodes['?'] = new char[2] { 'ü', 'J' };
		extendedCodes['@'] = new char[2] { 'ü', 'V' };
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
		extendedCodes['['] = new char[2] { 'ü', 'K' };
		extendedCodes['\\'] = new char[2] { 'ü', 'L' };
		extendedCodes[']'] = new char[2] { 'ü', 'M' };
		extendedCodes['^'] = new char[2] { 'ü', 'N' };
		extendedCodes['_'] = new char[2] { 'ü', 'O' };
		extendedCodes['`'] = new char[2] { 'ü', 'W' };
		extendedCodes['a'] = new char[2] { 'þ', 'A' };
		extendedCodes['b'] = new char[2] { 'þ', 'B' };
		extendedCodes['c'] = new char[2] { 'þ', 'C' };
		extendedCodes['d'] = new char[2] { 'þ', 'D' };
		extendedCodes['e'] = new char[2] { 'þ', 'E' };
		extendedCodes['f'] = new char[2] { 'þ', 'F' };
		extendedCodes['g'] = new char[2] { 'þ', 'G' };
		extendedCodes['h'] = new char[2] { 'þ', 'H' };
		extendedCodes['i'] = new char[2] { 'þ', 'I' };
		extendedCodes['j'] = new char[2] { 'þ', 'J' };
		extendedCodes['k'] = new char[2] { 'þ', 'K' };
		extendedCodes['l'] = new char[2] { 'þ', 'L' };
		extendedCodes['m'] = new char[2] { 'þ', 'M' };
		extendedCodes['n'] = new char[2] { 'þ', 'N' };
		extendedCodes['o'] = new char[2] { 'þ', 'O' };
		extendedCodes['p'] = new char[2] { 'þ', 'P' };
		extendedCodes['q'] = new char[2] { 'þ', 'Q' };
		extendedCodes['r'] = new char[2] { 'þ', 'R' };
		extendedCodes['s'] = new char[2] { 'þ', 'S' };
		extendedCodes['t'] = new char[2] { 'þ', 'T' };
		extendedCodes['u'] = new char[2] { 'þ', 'U' };
		extendedCodes['v'] = new char[2] { 'þ', 'V' };
		extendedCodes['w'] = new char[2] { 'þ', 'W' };
		extendedCodes['x'] = new char[2] { 'þ', 'X' };
		extendedCodes['y'] = new char[2] { 'þ', 'Y' };
		extendedCodes['z'] = new char[2] { 'þ', 'Z' };
		extendedCodes['{'] = new char[2] { 'ü', 'P' };
		extendedCodes['|'] = new char[2] { 'ü', 'Q' };
		extendedCodes['}'] = new char[2] { 'ü', 'R' };
		extendedCodes['~'] = new char[2] { 'ü', 'S' };
		extendedCodes['\u007f'] = new char[2] { 'ü', 'T' };
	}

	private void GetExtendedText()
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
