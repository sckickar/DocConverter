using System.Collections.Generic;
using System.Text;

namespace DocGen.Pdf.Barcode;

public class PdfGS1Code128Barcode : PdfUnidimensionalBarcode
{
	public PdfGS1Code128Barcode()
	{
		Initialize();
	}

	public PdfGS1Code128Barcode(string text)
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
						text += value.Symbol;
					}
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
		string text = base.Text.Replace("[FNC1]", "");
		if (!Validate(base.Text.Replace("(", "").Replace(")", "").Replace(" ", "")
			.Replace("[FNC1]", "")))
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
		if (this != null)
		{
			text2 = base.Text;
			string text3 = "";
			while (text2.Length > 0)
			{
				int num = int.Parse(text2.Substring(0, 2));
				text3 = ((num >= 95) ? (text3 + (char)(num + 37)) : (text3 + (char)(num + 32)));
				text2 = text2.Remove(0, 2);
			}
			text2 = base.StartSymbol + 134 + text3 + base.StopSymbol;
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
		base.ValidatorExpression = "^[a-zA-Z0-9_]*$";
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
		string obj = base.Text.Replace("[FNC1]", "");
		if (!Validate(base.Text.Replace("(", "").Replace(")", "").Replace(" ", "")
			.Replace("[FNC1]", "")))
		{
			throw new BarcodeException("Barcode text does not meet the Barcode specification.");
		}
		PdfCode128BBarcode pdfCode128BBarcode = new PdfCode128BBarcode();
		PdfCode128CBarcode pdfCode128CBarcode = new PdfCode128CBarcode();
		int num = 1;
		int num2 = 0;
		List<byte[]> list = new List<byte[]> { pdfCode128CBarcode.BarcodeSymbolsString["StartCodeC"].Bars };
		num2 += num * pdfCode128CBarcode.BarcodeSymbolsString["StartCodeC"].CheckDigit;
		list.Add(pdfCode128CBarcode.BarcodeSymbolsString["FNC1"].Bars);
		num2 += num++ * pdfCode128CBarcode.BarcodeSymbolsString["FNC1"].CheckDigit;
		string[] array = obj.Split(new char[1] { '(' });
		bool flag = false;
		for (int i = 0; i < array.Length; i++)
		{
			if (string.IsNullOrEmpty(array[i]))
			{
				continue;
			}
			string[] array2 = array[i].Split(new char[1] { ')' });
			array2[0] = array2[0].Replace("(", "");
			array2[0] = array2[0].Replace(")", "");
			int aILength = GetAILength(array2[0]);
			if (aILength != 0 && array2[1].Trim().Length != aILength)
			{
				throw new BarcodeException("(" + array2[0].ToString() + ") AI should have the value of length " + aILength);
			}
			string text = array[i].Replace("(", "").Replace(")", "");
			while (text.Length > 0)
			{
				if (text.Length == 1)
				{
					char key = text[0];
					list.Add(pdfCode128CBarcode.BarcodeSymbolsString["CodeB"].Bars);
					num2 += num++ * pdfCode128CBarcode.BarcodeSymbolsString["CodeB"].CheckDigit;
					list.Add(pdfCode128BBarcode.BarcodeSymbols[key].Bars);
					num2 += num++ * pdfCode128BBarcode.BarcodeSymbols[key].CheckDigit;
					flag = true;
					break;
				}
				string text2 = text.Substring(0, 2);
				long result = 0L;
				if (!flag && pdfCode128CBarcode.BarcodeSymbolsString.ContainsKey(text2))
				{
					list.Add(pdfCode128CBarcode.BarcodeSymbolsString[text2].Bars);
					num2 += num++ * pdfCode128CBarcode.BarcodeSymbolsString[text2].CheckDigit;
					text = text.Remove(0, 2);
					flag = false;
					continue;
				}
				if (flag && long.TryParse(text, out result))
				{
					list.Add(pdfCode128CBarcode.BarcodeSymbolsString["CodeC"].Bars);
					num2 += num++ * pdfCode128CBarcode.BarcodeSymbolsString["CodeC"].CheckDigit;
					list.Add(pdfCode128CBarcode.BarcodeSymbolsString[text2].Bars);
					num2 += num++ * pdfCode128CBarcode.BarcodeSymbolsString[text2].CheckDigit;
					text = text.Remove(0, 2);
					flag = false;
					continue;
				}
				if (!flag)
				{
					list.Add(pdfCode128CBarcode.BarcodeSymbolsString["CodeB"].Bars);
					num2 += num++ * pdfCode128CBarcode.BarcodeSymbolsString["CodeB"].CheckDigit;
				}
				list.Add(pdfCode128BBarcode.BarcodeSymbols[text2[0]].Bars);
				num2 += num++ * pdfCode128BBarcode.BarcodeSymbols[text2[0]].CheckDigit;
				list.Add(pdfCode128BBarcode.BarcodeSymbols[text2[1]].Bars);
				num2 += num++ * pdfCode128BBarcode.BarcodeSymbols[text2[1]].CheckDigit;
				text = text.Remove(0, 2);
				flag = true;
			}
			if (aILength == 0 && i + 1 < array.Length)
			{
				list.Add(pdfCode128CBarcode.BarcodeSymbolsString["FNC1"].Bars);
				num2 += num++ * pdfCode128CBarcode.BarcodeSymbolsString["FNC1"].CheckDigit;
			}
		}
		if (flag)
		{
			char symbol = pdfCode128BBarcode.GetSymbol(num2 % 103);
			list.Add(pdfCode128BBarcode.BarcodeSymbols[symbol].Bars);
		}
		else
		{
			string text3 = (num2 % 103).ToString();
			if (text3.Length == 1)
			{
				text3 = "0" + text3;
			}
			list.Add(pdfCode128CBarcode.BarcodeSymbolsString[text3].Bars);
		}
		list.Add(pdfCode128CBarcode.BarcodeSymbolsString["Stop"].Bars);
		return list;
	}

	private int GetAILength(string ai)
	{
		return ai switch
		{
			"00" => 18, 
			"01" => 14, 
			"02" => 14, 
			"03" => 14, 
			"04" => 16, 
			"11" => 6, 
			"12" => 6, 
			"13" => 6, 
			"14" => 4, 
			"15" => 6, 
			"16" => 6, 
			"17" => 6, 
			"18" => 6, 
			"19" => 6, 
			"20" => 2, 
			"31" => 6, 
			"32" => 6, 
			"33" => 6, 
			"34" => 6, 
			"35" => 6, 
			"36" => 6, 
			"41" => 13, 
			_ => 0, 
		};
	}
}
