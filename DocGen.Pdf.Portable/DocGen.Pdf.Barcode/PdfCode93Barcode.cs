using System;
using System.Collections.Generic;

namespace DocGen.Pdf.Barcode;

public class PdfCode93Barcode : PdfUnidimensionalBarcode
{
	public PdfCode93Barcode()
	{
		base.EnableCheckDigit = true;
		Initialize();
	}

	public PdfCode93Barcode(string text)
		: this()
	{
		base.Text = text;
	}

	internal void Initialize()
	{
		base.StartSymbol = '*';
		base.StopSymbol = 'ÿ';
		base.ValidatorExpression = "^[\\x41-\\x5A\\x30-\\x39\\x20\\-\\.\\$\\/\\+\\%\\ ]+$";
		base.BarcodeSymbols['0'] = new BarcodeSymbolTable('0', 0, new byte[6] { 1, 3, 1, 1, 1, 2 });
		base.BarcodeSymbols['1'] = new BarcodeSymbolTable('1', 1, new byte[6] { 1, 1, 1, 2, 1, 3 });
		base.BarcodeSymbols['2'] = new BarcodeSymbolTable('2', 2, new byte[6] { 1, 1, 1, 3, 1, 2 });
		base.BarcodeSymbols['3'] = new BarcodeSymbolTable('3', 3, new byte[6] { 1, 1, 1, 4, 1, 1 });
		base.BarcodeSymbols['4'] = new BarcodeSymbolTable('4', 4, new byte[6] { 1, 2, 1, 1, 1, 3 });
		base.BarcodeSymbols['5'] = new BarcodeSymbolTable('5', 5, new byte[6] { 1, 2, 1, 2, 1, 2 });
		base.BarcodeSymbols['6'] = new BarcodeSymbolTable('6', 6, new byte[6] { 1, 2, 1, 3, 1, 1 });
		base.BarcodeSymbols['7'] = new BarcodeSymbolTable('7', 7, new byte[6] { 1, 1, 1, 1, 1, 4 });
		base.BarcodeSymbols['8'] = new BarcodeSymbolTable('8', 8, new byte[6] { 1, 3, 1, 2, 1, 1 });
		base.BarcodeSymbols['9'] = new BarcodeSymbolTable('9', 9, new byte[6] { 1, 4, 1, 1, 1, 1 });
		base.BarcodeSymbols['A'] = new BarcodeSymbolTable('A', 10, new byte[6] { 2, 1, 1, 1, 1, 3 });
		base.BarcodeSymbols['B'] = new BarcodeSymbolTable('B', 11, new byte[6] { 2, 1, 1, 2, 1, 2 });
		base.BarcodeSymbols['C'] = new BarcodeSymbolTable('C', 12, new byte[6] { 2, 1, 1, 3, 1, 1 });
		base.BarcodeSymbols['D'] = new BarcodeSymbolTable('D', 13, new byte[6] { 2, 2, 1, 1, 1, 2 });
		base.BarcodeSymbols['E'] = new BarcodeSymbolTable('E', 14, new byte[6] { 2, 2, 1, 2, 1, 1 });
		base.BarcodeSymbols['F'] = new BarcodeSymbolTable('F', 15, new byte[6] { 2, 3, 1, 1, 1, 1 });
		base.BarcodeSymbols['G'] = new BarcodeSymbolTable('G', 16, new byte[6] { 1, 1, 2, 1, 1, 3 });
		base.BarcodeSymbols['H'] = new BarcodeSymbolTable('H', 17, new byte[6] { 1, 1, 2, 2, 1, 2 });
		base.BarcodeSymbols['I'] = new BarcodeSymbolTable('I', 18, new byte[6] { 1, 1, 2, 3, 1, 1 });
		base.BarcodeSymbols['J'] = new BarcodeSymbolTable('J', 19, new byte[6] { 1, 2, 2, 1, 1, 2 });
		base.BarcodeSymbols['K'] = new BarcodeSymbolTable('K', 20, new byte[6] { 1, 3, 2, 1, 1, 1 });
		base.BarcodeSymbols['L'] = new BarcodeSymbolTable('L', 21, new byte[6] { 1, 1, 1, 1, 2, 3 });
		base.BarcodeSymbols['M'] = new BarcodeSymbolTable('M', 22, new byte[6] { 1, 1, 1, 2, 2, 2 });
		base.BarcodeSymbols['N'] = new BarcodeSymbolTable('N', 23, new byte[6] { 1, 1, 1, 3, 2, 1 });
		base.BarcodeSymbols['O'] = new BarcodeSymbolTable('O', 24, new byte[6] { 1, 2, 1, 1, 2, 2 });
		base.BarcodeSymbols['P'] = new BarcodeSymbolTable('P', 25, new byte[6] { 1, 3, 1, 1, 2, 1 });
		base.BarcodeSymbols['Q'] = new BarcodeSymbolTable('Q', 26, new byte[6] { 2, 1, 2, 1, 1, 2 });
		base.BarcodeSymbols['R'] = new BarcodeSymbolTable('R', 27, new byte[6] { 2, 1, 2, 2, 1, 1 });
		base.BarcodeSymbols['S'] = new BarcodeSymbolTable('S', 28, new byte[6] { 2, 1, 1, 1, 2, 2 });
		base.BarcodeSymbols['T'] = new BarcodeSymbolTable('T', 29, new byte[6] { 2, 1, 1, 2, 2, 1 });
		base.BarcodeSymbols['U'] = new BarcodeSymbolTable('U', 30, new byte[6] { 2, 2, 1, 1, 2, 1 });
		base.BarcodeSymbols['V'] = new BarcodeSymbolTable('V', 31, new byte[6] { 2, 2, 2, 1, 1, 1 });
		base.BarcodeSymbols['W'] = new BarcodeSymbolTable('W', 32, new byte[6] { 1, 1, 2, 1, 2, 2 });
		base.BarcodeSymbols['X'] = new BarcodeSymbolTable('X', 33, new byte[6] { 1, 1, 2, 2, 2, 1 });
		base.BarcodeSymbols['Y'] = new BarcodeSymbolTable('Y', 34, new byte[6] { 1, 2, 2, 1, 2, 1 });
		base.BarcodeSymbols['Z'] = new BarcodeSymbolTable('Z', 35, new byte[6] { 1, 2, 3, 1, 1, 1 });
		base.BarcodeSymbols['-'] = new BarcodeSymbolTable('-', 36, new byte[6] { 1, 2, 1, 1, 3, 1 });
		base.BarcodeSymbols['.'] = new BarcodeSymbolTable('.', 37, new byte[6] { 3, 1, 1, 1, 1, 2 });
		base.BarcodeSymbols[' '] = new BarcodeSymbolTable(' ', 38, new byte[6] { 3, 1, 1, 2, 1, 1 });
		base.BarcodeSymbols['$'] = new BarcodeSymbolTable('$', 39, new byte[6] { 3, 2, 1, 1, 1, 1 });
		base.BarcodeSymbols['/'] = new BarcodeSymbolTable('/', 40, new byte[6] { 1, 1, 2, 1, 3, 1 });
		base.BarcodeSymbols['+'] = new BarcodeSymbolTable('+', 41, new byte[6] { 1, 1, 3, 1, 2, 1 });
		base.BarcodeSymbols['%'] = new BarcodeSymbolTable('%', 42, new byte[6] { 2, 1, 1, 1, 3, 1 });
		base.BarcodeSymbols['*'] = new BarcodeSymbolTable('*', 0, new byte[6] { 1, 1, 1, 1, 4, 1 });
		base.BarcodeSymbols['ÿ'] = new BarcodeSymbolTable('ÿ', 47, new byte[7] { 1, 1, 1, 1, 4, 1, 1 });
		base.BarcodeSymbols['û'] = new BarcodeSymbolTable('û', 43, new byte[6] { 1, 2, 1, 2, 2, 0 });
		base.BarcodeSymbols['ü'] = new BarcodeSymbolTable('ü', 44, new byte[6] { 3, 1, 2, 1, 1, 1 });
		base.BarcodeSymbols['ý'] = new BarcodeSymbolTable('ý', 45, new byte[6] { 3, 1, 1, 1, 2, 1 });
		base.BarcodeSymbols['þ'] = new BarcodeSymbolTable('þ', 46, new byte[6] { 1, 2, 2, 2, 1, 1 });
	}

	protected internal override char[] CalculateCheckDigit()
	{
		if (!base.EnableCheckDigit)
		{
			return null;
		}
		if (!base.ExtendedText.Equals(string.Empty))
		{
			_ = base.ExtendedText;
		}
		else
		{
			_ = base.Text;
		}
		_ = new char[2];
		return GetCheckSumSymbols();
	}

	protected internal char[] GetCheckSumSymbols()
	{
		string text = base.Text;
		char[] array = new char[2];
		int num = 0;
		int length = text.Length;
		for (int i = 0; i < length; i++)
		{
			int num2 = (length - i) % 20;
			if (num2 == 0)
			{
				num2 = 20;
			}
			int checkDigit = base.BarcodeSymbols[base.Text[i]].CheckDigit;
			num += checkDigit * num2;
		}
		num %= 47;
		array[0] = Convert.ToChar(num);
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
		string text2 = base.Text;
		text2 += c;
		array[0] = c;
		text = text2;
		num = 0;
		length = text.Length;
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
		text2 += c2;
		array[1] = c2;
		return array;
	}
}
