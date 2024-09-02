using System.Collections.Generic;

namespace DocGen.Pdf.Barcode;

public class PdfCode11Barcode : PdfUnidimensionalBarcode
{
	public PdfCode11Barcode()
	{
		Initialize();
	}

	public PdfCode11Barcode(string text)
		: this()
	{
		base.Text = text;
	}

	protected internal override char[] CalculateCheckDigit()
	{
		string text = base.Text;
		int num = 0;
		int num2 = 1;
		while (num != -1)
		{
			int[] array = new int[text.Length];
			for (int num3 = text.Length - 1; num3 >= 0; num3--)
			{
				array[num3] = num2;
				num2++;
				if (num2 == 11 && num == 0)
				{
					num2 = 1;
				}
				if (num2 == 10 && num == 1)
				{
					num2 = 1;
				}
			}
			int num4 = 0;
			for (int num5 = text.Length - 1; num5 >= 0; num5--)
			{
				if (text[num5] == '-')
				{
					num4 += 10 * array[num5];
				}
				else
				{
					int num6 = int.Parse(text[num5].ToString());
					num4 += num6 * array[num5];
				}
			}
			num4 %= 11;
			text += GetSymbol(num4);
			num = ((text.Length < 10 || text.Length - base.Text.Length > 2 || num == 1) ? (-1) : (num + 1));
		}
		return text.Substring(base.Text.Length, text.Length - base.Text.Length).ToCharArray();
	}

	private void Initialize()
	{
		base.StartSymbol = '*';
		base.StopSymbol = '*';
		base.ValidatorExpression = "^[0-9\\-]*$";
		base.BarcodeSymbols['0'] = new BarcodeSymbolTable('0', 0, new byte[5] { 1, 1, 1, 1, 2 });
		base.BarcodeSymbols['1'] = new BarcodeSymbolTable('1', 1, new byte[5] { 2, 1, 1, 1, 2 });
		base.BarcodeSymbols['2'] = new BarcodeSymbolTable('2', 2, new byte[5] { 1, 2, 1, 1, 2 });
		base.BarcodeSymbols['3'] = new BarcodeSymbolTable('3', 3, new byte[5] { 2, 2, 1, 1, 1 });
		base.BarcodeSymbols['4'] = new BarcodeSymbolTable('4', 4, new byte[5] { 1, 1, 2, 1, 2 });
		base.BarcodeSymbols['5'] = new BarcodeSymbolTable('5', 5, new byte[5] { 2, 1, 2, 1, 1 });
		base.BarcodeSymbols['6'] = new BarcodeSymbolTable('6', 6, new byte[5] { 1, 2, 2, 1, 1 });
		base.BarcodeSymbols['7'] = new BarcodeSymbolTable('7', 7, new byte[5] { 1, 1, 1, 2, 2 });
		base.BarcodeSymbols['8'] = new BarcodeSymbolTable('8', 8, new byte[5] { 2, 1, 1, 2, 1 });
		base.BarcodeSymbols['9'] = new BarcodeSymbolTable('9', 9, new byte[5] { 2, 1, 1, 1, 1 });
		base.BarcodeSymbols['-'] = new BarcodeSymbolTable('-', 10, new byte[5] { 1, 1, 2, 1, 1 });
		base.BarcodeSymbols['*'] = new BarcodeSymbolTable('*', 0, new byte[5] { 1, 1, 2, 2, 1 });
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
