using System.Collections.Generic;

namespace DocGen.Pdf.Barcode;

public class PdfCode32Barcode : PdfCode39Barcode
{
	private char[] checkSumSymbols;

	private bool isTextEncoded;

	private string encodedText = "";

	private const int m_encodedtextLength = 6;

	public PdfCode32Barcode()
	{
		InitializeCode32();
	}

	public PdfCode32Barcode(string text)
		: this()
	{
		base.Text = text;
	}

	protected internal override char[] CalculateCheckDigit()
	{
		int num = base.Text[0] - 48;
		int num2 = 2 * (base.Text[1] - 48);
		int num3 = base.Text[2] - 48;
		int num4 = 2 * (base.Text[3] - 48);
		int num5 = base.Text[4] - 48;
		int num6 = 2 * (base.Text[5] - 48);
		int num7 = base.Text[6] - 48;
		int num8 = 2 * (base.Text[7] - 48);
		int num9 = num2 / 10 + num4 / 10 + num6 / 10 + num8 / 10 + num2 % 10 + num4 % 10 + num6 % 10 + num8 % 10;
		int num10 = num + num3 + num5 + num7;
		int num11 = (num9 + num10) % 10;
		return new char[1] { (char)(num11 + 48) };
	}

	protected string ObtainBarcodeSymbols()
	{
		string text = "";
		checkSumSymbols = CalculateCheckDigit();
		if (checkSumSymbols != null)
		{
			for (int i = 0; i < checkSumSymbols.Length; i++)
			{
				if (base.EnableCheckDigit)
				{
					text += checkSumSymbols[i];
				}
			}
		}
		return GetDataToEncode(base.Text + text);
	}

	protected string GetDataToEncode(string originalData)
	{
		string text = string.Empty;
		int num = int.Parse(originalData);
		while (num != 0)
		{
			int num2 = num % 32;
			num /= 32;
			foreach (KeyValuePair<char, BarcodeSymbolTable> barcodeSymbol in base.BarcodeSymbols)
			{
				BarcodeSymbolTable value = barcodeSymbol.Value;
				if (value.CheckDigit == num2)
				{
					text = value.Symbol + text;
				}
			}
		}
		if (text.Length < 6)
		{
			text = new string('0', 6 - text.Length) + text;
		}
		return text;
	}

	internal override string GetTextToEncode()
	{
		if (isTextEncoded && encodedText != string.Empty)
		{
			return encodedText;
		}
		base.Text = base.Text.TrimStart('A');
		if (base.Text.Length != 8)
		{
			throw new BarcodeException("Barcode Text Length that are not accepted by this barcode specification.");
		}
		if (!Validate(base.Text))
		{
			throw new BarcodeException("Barcode text contains characters that are not accepted by this barcode specification.");
		}
		string text = (base.ExtendedText.Equals(string.Empty) ? base.Text.Trim('*') : base.ExtendedText.Trim('*'));
		text = ObtainBarcodeSymbols();
		isTextEncoded = true;
		encodedText = text;
		if (base.EnableCheckDigit)
		{
			base.Text += checkSumSymbols[0];
		}
		base.Text = "A" + base.Text;
		return encodedText;
	}

	private void InitializeCode32()
	{
		base.StartSymbol = '*';
		base.StopSymbol = '*';
		base.ValidatorExpression = "^[\\x41-\\x5A\\x30-\\x39\\x20\\-\\*\\.\\/\\+\\%]+$";
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
		base.BarcodeSymbols['B'] = new BarcodeSymbolTable('B', 10, new byte[9] { 1, 1, 3, 1, 1, 3, 1, 1, 3 });
		base.BarcodeSymbols['C'] = new BarcodeSymbolTable('C', 11, new byte[9] { 3, 1, 3, 1, 1, 3, 1, 1, 1 });
		base.BarcodeSymbols['D'] = new BarcodeSymbolTable('D', 12, new byte[9] { 1, 1, 1, 1, 3, 3, 1, 1, 3 });
		base.BarcodeSymbols['F'] = new BarcodeSymbolTable('F', 13, new byte[9] { 1, 1, 3, 1, 3, 3, 1, 1, 1 });
		base.BarcodeSymbols['G'] = new BarcodeSymbolTable('G', 14, new byte[9] { 1, 1, 1, 1, 1, 3, 3, 1, 3 });
		base.BarcodeSymbols['H'] = new BarcodeSymbolTable('H', 15, new byte[9] { 3, 1, 1, 1, 1, 3, 3, 1, 1 });
		base.BarcodeSymbols['J'] = new BarcodeSymbolTable('J', 16, new byte[9] { 1, 1, 1, 1, 3, 3, 3, 1, 1 });
		base.BarcodeSymbols['K'] = new BarcodeSymbolTable('K', 17, new byte[9] { 3, 1, 1, 1, 1, 1, 1, 3, 3 });
		base.BarcodeSymbols['L'] = new BarcodeSymbolTable('L', 18, new byte[9] { 1, 1, 3, 1, 1, 1, 1, 3, 3 });
		base.BarcodeSymbols['M'] = new BarcodeSymbolTable('M', 19, new byte[9] { 3, 1, 3, 1, 1, 1, 1, 3, 1 });
		base.BarcodeSymbols['N'] = new BarcodeSymbolTable('N', 20, new byte[9] { 1, 1, 1, 1, 3, 1, 1, 3, 3 });
		base.BarcodeSymbols['P'] = new BarcodeSymbolTable('P', 21, new byte[9] { 1, 1, 3, 1, 3, 1, 1, 3, 1 });
		base.BarcodeSymbols['Q'] = new BarcodeSymbolTable('Q', 22, new byte[9] { 1, 1, 1, 1, 1, 1, 3, 3, 3 });
		base.BarcodeSymbols['R'] = new BarcodeSymbolTable('R', 23, new byte[9] { 3, 1, 1, 1, 1, 1, 3, 3, 1 });
		base.BarcodeSymbols['S'] = new BarcodeSymbolTable('S', 24, new byte[9] { 1, 1, 3, 1, 1, 1, 3, 3, 1 });
		base.BarcodeSymbols['T'] = new BarcodeSymbolTable('T', 25, new byte[9] { 1, 1, 1, 1, 3, 1, 3, 3, 1 });
		base.BarcodeSymbols['U'] = new BarcodeSymbolTable('U', 26, new byte[9] { 3, 3, 1, 1, 1, 1, 1, 1, 3 });
		base.BarcodeSymbols['V'] = new BarcodeSymbolTable('V', 27, new byte[9] { 1, 3, 3, 1, 1, 1, 1, 1, 3 });
		base.BarcodeSymbols['W'] = new BarcodeSymbolTable('W', 28, new byte[9] { 3, 3, 3, 1, 1, 1, 1, 1, 1 });
		base.BarcodeSymbols['X'] = new BarcodeSymbolTable('X', 29, new byte[9] { 1, 3, 1, 1, 3, 1, 1, 1, 3 });
		base.BarcodeSymbols['Y'] = new BarcodeSymbolTable('Y', 30, new byte[9] { 3, 3, 1, 1, 3, 1, 1, 1, 1 });
		base.BarcodeSymbols['Z'] = new BarcodeSymbolTable('Z', 31, new byte[9] { 1, 3, 3, 1, 3, 1, 1, 1, 1 });
		base.BarcodeSymbols['*'] = new BarcodeSymbolTable('*', 0, new byte[9] { 1, 3, 1, 1, 3, 1, 3, 1, 1 });
	}
}
