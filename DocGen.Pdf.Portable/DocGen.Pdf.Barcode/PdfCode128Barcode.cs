using System.Collections.Generic;
using System.Text;

namespace DocGen.Pdf.Barcode;

public class PdfCode128Barcode : PdfUnidimensionalBarcode
{
	private static bool isnumb;

	public PdfCode128Barcode()
	{
		Initialize();
	}

	public PdfCode128Barcode(string text)
		: this()
	{
		base.Text = text;
	}

	protected internal override char[] CalculateCheckDigit()
	{
		string text = null;
		if (!base.EnableCheckDigit)
		{
			return null;
		}
		int num = 0;
		string text2 = base.Text;
		if (text2.Length % 2 == 1)
		{
			text2 = "0" + text2;
		}
		for (int i = 0; i < text2.Length; i += 2)
		{
			int num2 = int.Parse(text2.Substring(i, 2));
			foreach (KeyValuePair<char, BarcodeSymbolTable> barcodeSymbol in base.BarcodeSymbols)
			{
				BarcodeSymbolTable value = barcodeSymbol.Value;
				if (value.Symbol == (ushort)num2)
				{
					if (value != null)
					{
						num += value.CheckDigit * (i / 2 + 1);
					}
					text += value.Symbol;
					break;
				}
			}
		}
		num += 105;
		num %= 103;
		char symbol = GetSymbol(num);
		base.Text = text;
		return new char[1] { symbol };
	}

	internal override string GetTextToEncode()
	{
		string text = base.Text;
		if (!Validate(base.Text))
		{
			throw new BarcodeException("Barcode text contains characters that are not accepted by this barcode specification.");
		}
		string text2 = (base.ExtendedText.Equals(string.Empty) ? base.Text.Trim('*') : base.ExtendedText.Trim('*'));
		if (isCheckDigitAdded || !base.EnableCheckDigit)
		{
			return text2;
		}
		char[] array = CalculateCheckDigit();
		if (array == null || array.Length == 0)
		{
			return text2;
		}
		if (base.ShowCheckDigit && !isCheckDigitAdded)
		{
			if (text2[text2.Length - 1] != array[^1])
			{
				char[] array2 = array;
				foreach (char c in array2)
				{
					text2 += c;
				}
			}
			isCheckDigitAdded = true;
			if (base.ExtendedText.Equals(string.Empty))
			{
				text2 = base.Text;
				char[] array2 = array;
				foreach (char c2 in array2)
				{
					text2 += c2;
				}
			}
		}
		base.Text = text;
		if (base.ShowCheckDigit)
		{
			char[] array2 = array;
			foreach (char c3 in array2)
			{
				base.Text += c3;
			}
		}
		isCheckDigitAdded = true;
		if (base.ExtendedText.Equals(string.Empty))
		{
			text2 = base.Text;
			char[] array2 = array;
			foreach (char c4 in array2)
			{
				text2 += c4;
			}
		}
		return text2;
	}

	protected string GetDataToEncode(string originalData)
	{
		_ = new byte[0];
		if (originalData.Length % 2 == 0)
		{
			originalData.Split(new char[1] { '(' });
		}
		StringBuilder stringBuilder = new StringBuilder();
		string text = originalData;
		if (originalData.Length % 2 == 1)
		{
			text = "0" + text;
		}
		for (int i = 0; i < text.Length; i += 2)
		{
			char value = (char)int.Parse(text.Substring(i, 2));
			stringBuilder.Append(value);
		}
		return stringBuilder.ToString();
	}

	private void Initialize()
	{
		base.StartSymbol = '\u0087';
		base.StopSymbol = '\u0088';
		base.ValidatorExpression = "^[\\x00-\\x7F]";
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

	internal override List<byte[]> GetTextToEncodeList()
	{
		string text = base.Text;
		PdfCode128ABarcode pdfCode128ABarcode = new PdfCode128ABarcode();
		PdfCode128BBarcode pdfCode128BBarcode = new PdfCode128BBarcode();
		PdfCode128CBarcode pdfCode128CBarcode = new PdfCode128CBarcode();
		int num = 1;
		int num2 = 0;
		List<byte[]> list = new List<byte[]>();
		if (text.Length >= 2)
		{
			string text2 = text.Substring(0, 2);
			bool flag = !text2.ToUpper().Equals(text2);
			if (pdfCode128CBarcode.BarcodeSymbolsString.ContainsKey(text2))
			{
				list.Add(pdfCode128CBarcode.BarcodeSymbolsString["StartCodeC"].Bars);
				num2 += num * pdfCode128CBarcode.BarcodeSymbolsString["StartCodeC"].CheckDigit;
			}
			else if (!flag)
			{
				list.Add(pdfCode128CBarcode.BarcodeSymbolsString["StartCodeA"].Bars);
				num2 += num * pdfCode128CBarcode.BarcodeSymbolsString["StartCodeA"].CheckDigit;
			}
			else
			{
				list.Add(pdfCode128CBarcode.BarcodeSymbolsString["StartCodeB"].Bars);
				num2 += num * pdfCode128CBarcode.BarcodeSymbolsString["StartCodeB"].CheckDigit;
			}
		}
		else if (pdfCode128ABarcode.BarcodeSymbols.ContainsKey(text[0]))
		{
			list.Add(pdfCode128CBarcode.BarcodeSymbolsString["StartCodeA"].Bars);
			num2 += num * pdfCode128CBarcode.BarcodeSymbolsString["StartCodeA"].CheckDigit;
		}
		else
		{
			list.Add(pdfCode128CBarcode.BarcodeSymbolsString["StartCodeB"].Bars);
			num2 += num * pdfCode128CBarcode.BarcodeSymbolsString["StartCodeB"].CheckDigit;
		}
		bool flag2 = false;
		bool flag3 = false;
		string text3 = text;
		while (text3.Length > 0)
		{
			if (text3.Length == 1)
			{
				char key = text3[0];
				if (pdfCode128ABarcode.BarcodeSymbols.ContainsKey(key))
				{
					if (!flag3 && (isnumb || flag2))
					{
						list.Add(pdfCode128CBarcode.BarcodeSymbolsString["CodeA"].Bars);
						num2 += num++ * pdfCode128CBarcode.BarcodeSymbolsString["CodeA"].CheckDigit;
					}
					list.Add(pdfCode128ABarcode.BarcodeSymbols[key].Bars);
					num2 += num++ * pdfCode128ABarcode.BarcodeSymbols[key].CheckDigit;
					flag3 = true;
				}
				else
				{
					if (!flag2 & (isnumb || flag3))
					{
						list.Add(pdfCode128CBarcode.BarcodeSymbolsString["CodeB"].Bars);
						num2 += num++ * pdfCode128CBarcode.BarcodeSymbolsString["CodeB"].CheckDigit;
					}
					list.Add(pdfCode128BBarcode.BarcodeSymbols[key].Bars);
					num2 += num++ * pdfCode128BBarcode.BarcodeSymbols[key].CheckDigit;
					flag2 = true;
				}
				break;
			}
			string text4 = text3.Substring(0, 2);
			if (!flag2 && !flag3 && pdfCode128CBarcode.BarcodeSymbolsString.ContainsKey(text4))
			{
				list.Add(pdfCode128CBarcode.BarcodeSymbolsString[text4].Bars);
				num2 += num++ * pdfCode128CBarcode.BarcodeSymbolsString[text4].CheckDigit;
				text3 = text3.Remove(0, 2);
				isnumb = true;
				flag2 = false;
				continue;
			}
			if (pdfCode128ABarcode.BarcodeSymbols.ContainsKey(text4[0]) && pdfCode128ABarcode.BarcodeSymbols.ContainsKey(text4[1]))
			{
				if (!flag3 && (isnumb || flag2))
				{
					list.Add(pdfCode128CBarcode.BarcodeSymbolsString["CodeA"].Bars);
					num2 += num++ * pdfCode128CBarcode.BarcodeSymbolsString["CodeA"].CheckDigit;
				}
				list.Add(pdfCode128ABarcode.BarcodeSymbols[text4[0]].Bars);
				num2 += num++ * pdfCode128ABarcode.BarcodeSymbols[text4[0]].CheckDigit;
				list.Add(pdfCode128ABarcode.BarcodeSymbols[text4[1]].Bars);
				num2 += num++ * pdfCode128ABarcode.BarcodeSymbols[text4[1]].CheckDigit;
				text3 = text3.Remove(0, 2);
				flag3 = true;
				continue;
			}
			if (pdfCode128BBarcode.BarcodeSymbols.ContainsKey(text4[0]) && pdfCode128BBarcode.BarcodeSymbols.ContainsKey(text4[1]))
			{
				if (!flag2 && (isnumb || flag3))
				{
					list.Add(pdfCode128CBarcode.BarcodeSymbolsString["CodeB"].Bars);
					num2 += num++ * pdfCode128CBarcode.BarcodeSymbolsString["CodeB"].CheckDigit;
				}
				list.Add(pdfCode128BBarcode.BarcodeSymbols[text4[0]].Bars);
				num2 += num++ * pdfCode128BBarcode.BarcodeSymbols[text4[0]].CheckDigit;
				list.Add(pdfCode128BBarcode.BarcodeSymbols[text4[1]].Bars);
				num2 += num++ * pdfCode128BBarcode.BarcodeSymbols[text4[1]].CheckDigit;
				text3 = text3.Remove(0, 2);
				flag2 = true;
				continue;
			}
			if (pdfCode128ABarcode.BarcodeSymbols.ContainsKey(text4[0]))
			{
				if (!flag3 && (isnumb || flag2))
				{
					list.Add(pdfCode128CBarcode.BarcodeSymbolsString["CodeA"].Bars);
					num2 += num++ * pdfCode128CBarcode.BarcodeSymbolsString["CodeA"].CheckDigit;
				}
				list.Add(pdfCode128ABarcode.BarcodeSymbols[text4[0]].Bars);
				num2 += num++ * pdfCode128ABarcode.BarcodeSymbols[text4[0]].CheckDigit;
				flag3 = true;
			}
			else
			{
				if (!flag2 & (isnumb || flag3))
				{
					list.Add(pdfCode128CBarcode.BarcodeSymbolsString["CodeB"].Bars);
					num2 += num++ * pdfCode128CBarcode.BarcodeSymbolsString["CodeB"].CheckDigit;
				}
				list.Add(pdfCode128BBarcode.BarcodeSymbols[text4[0]].Bars);
				num2 += num++ * pdfCode128BBarcode.BarcodeSymbols[text4[0]].CheckDigit;
				flag2 = true;
			}
			if (pdfCode128ABarcode.BarcodeSymbols.ContainsKey(text4[1]))
			{
				if (!flag3 && (isnumb || flag2))
				{
					list.Add(pdfCode128CBarcode.BarcodeSymbolsString["CodeA"].Bars);
					num2 += num++ * pdfCode128CBarcode.BarcodeSymbolsString["CodeA"].CheckDigit;
				}
				list.Add(pdfCode128ABarcode.BarcodeSymbols[text4[1]].Bars);
				num2 += num++ * pdfCode128ABarcode.BarcodeSymbols[text4[1]].CheckDigit;
				flag3 = true;
			}
			else
			{
				if (!flag2 & (isnumb || flag3))
				{
					list.Add(pdfCode128CBarcode.BarcodeSymbolsString["CodeB"].Bars);
					num2 += num++ * pdfCode128CBarcode.BarcodeSymbolsString["CodeB"].CheckDigit;
				}
				list.Add(pdfCode128BBarcode.BarcodeSymbols[text4[1]].Bars);
				num2 += num++ * pdfCode128BBarcode.BarcodeSymbols[text4[1]].CheckDigit;
				flag2 = true;
			}
			text3 = text3.Remove(0, 2);
		}
		if (flag3)
		{
			char symbol = pdfCode128ABarcode.GetSymbol(num2 % 103);
			list.Add(pdfCode128ABarcode.BarcodeSymbols[symbol].Bars);
		}
		else if (flag2)
		{
			char symbol2 = pdfCode128BBarcode.GetSymbol(num2 % 103);
			list.Add(pdfCode128BBarcode.BarcodeSymbols[symbol2].Bars);
		}
		else
		{
			string text5 = (num2 % 103).ToString();
			if (text5.Length == 1)
			{
				text5 = "0" + text5;
			}
			list.Add(pdfCode128CBarcode.BarcodeSymbolsString[text5].Bars);
		}
		isnumb = false;
		list.Add(pdfCode128CBarcode.BarcodeSymbolsString["Stop"].Bars);
		return list;
	}
}
