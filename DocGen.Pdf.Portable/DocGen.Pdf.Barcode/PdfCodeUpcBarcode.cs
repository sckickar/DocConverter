using System;

namespace DocGen.Pdf.Barcode;

public class PdfCodeUpcBarcode : PdfUnidimensionalBarcode
{
	public PdfCodeUpcBarcode()
	{
		base.EnableCheckDigit = true;
		Initialize();
	}

	public PdfCodeUpcBarcode(string text)
		: this()
	{
		base.Text = text;
	}

	protected internal override char[] CalculateCheckDigit()
	{
		double result;
		bool flag = double.TryParse(base.Text, out result);
		if ((flag && base.Text.Length == 11) || (flag && base.Text.Length == 12))
		{
			string text = (base.ExtendedText.Equals(string.Empty) ? base.Text : base.ExtendedText);
			int num = 0;
			int num2 = 0;
			for (int i = 1; i <= text.Length; i++)
			{
				if (i <= 11)
				{
					num2 = Convert.ToInt32(text.Substring(i - 1, 1));
					num = ((i % 2 != 0) ? (num + num2 * 3) : (num + num2));
				}
			}
			int num3 = (10 - num % 10) % 10;
			char[] array = text.ToCharArray();
			string text2 = string.Empty;
			for (int j = 0; j < array.Length; j++)
			{
				if (j == 0)
				{
					text2 += 6;
				}
				switch (j)
				{
				case 6:
					text2 = text2 + "B" + array[j];
					break;
				default:
					text2 += array[j];
					break;
				case 11:
					break;
				}
			}
			text = text2;
			text += num3;
			text += 6;
			return text.ToCharArray();
		}
		return null;
	}

	private void Initialize()
	{
		base.StartSymbol = 'ý';
		base.StopSymbol = 'ÿ';
		base.ValidatorExpression = "^[\\x00-\\x7F]";
		base.BarcodeSymbols['0'] = new BarcodeSymbolTable('0', 16, new byte[4] { 3, 2, 1, 1 });
		base.BarcodeSymbols['1'] = new BarcodeSymbolTable('1', 17, new byte[4] { 2, 2, 2, 1 });
		base.BarcodeSymbols['2'] = new BarcodeSymbolTable('2', 18, new byte[4] { 2, 1, 2, 2 });
		base.BarcodeSymbols['3'] = new BarcodeSymbolTable('3', 19, new byte[4] { 1, 4, 1, 1 });
		base.BarcodeSymbols['4'] = new BarcodeSymbolTable('4', 20, new byte[4] { 1, 1, 3, 2 });
		base.BarcodeSymbols['5'] = new BarcodeSymbolTable('5', 21, new byte[4] { 1, 2, 3, 1 });
		base.BarcodeSymbols['6'] = new BarcodeSymbolTable('6', 22, new byte[4] { 1, 1, 1, 4 });
		base.BarcodeSymbols['7'] = new BarcodeSymbolTable('7', 23, new byte[4] { 1, 3, 1, 2 });
		base.BarcodeSymbols['8'] = new BarcodeSymbolTable('8', 24, new byte[4] { 1, 2, 1, 3 });
		base.BarcodeSymbols['9'] = new BarcodeSymbolTable('9', 25, new byte[4] { 3, 1, 1, 2 });
		base.BarcodeSymbols['B'] = new BarcodeSymbolTable('B', 11, new byte[4] { 1, 1, 1, 1 });
	}
}
