using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace DocGen.Pdf.Barcode;

public class PdfCode128CBarcode : PdfUnidimensionalBarcode
{
	public PdfCode128CBarcode()
	{
		Initialize();
		base.EnableCheckDigit = true;
	}

	public PdfCode128CBarcode(string text)
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

	protected internal override bool Validate(string data)
	{
		if (new Regex(base.ValidatorExpression).Matches(data).Count == data.Length)
		{
			return true;
		}
		return false;
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
		if (base.EnableCheckDigit && !isCheckDigitAdded)
		{
			char[] array2 = array;
			foreach (char c in array2)
			{
				text2 += c;
			}
		}
		if (!isCheckDigitAdded)
		{
			if (text2[text2.Length - 1] != array[^1])
			{
				char[] array2 = array;
				foreach (char c2 in array2)
				{
					text2 += c2;
				}
			}
			isCheckDigitAdded = true;
			if (base.ExtendedText.Equals(string.Empty))
			{
				text2 = base.Text;
				char[] array2 = array;
				foreach (char c3 in array2)
				{
					text2 += c3;
				}
			}
		}
		base.Text = text;
		if (base.ShowCheckDigit)
		{
			char[] array2 = array;
			foreach (char c4 in array2)
			{
				base.Text += c4;
			}
		}
		isCheckDigitAdded = true;
		return text2;
	}

	internal override List<byte[]> GetTextToEncodeList()
	{
		string text = base.Text;
		if (!Validate(base.Text))
		{
			throw new BarcodeException("Barcode text contains characters that are not accepted by this barcode specification.");
		}
		if (base.Text.Length % 2 != 0 && base.Text.Length % 2 != 0)
		{
			base.Text = "0" + base.Text;
			text = base.Text;
		}
		int num = 1;
		int num2 = 0;
		List<byte[]> list = new List<byte[]>();
		list.Add(base.BarcodeSymbolsString["StartCodeC"].Bars);
		num2 += num * base.BarcodeSymbolsString["StartCodeC"].CheckDigit;
		while (text != string.Empty)
		{
			string key = text.Substring(0, 2);
			list.Add(base.BarcodeSymbolsString[key].Bars);
			num2 += num++ * base.BarcodeSymbolsString[key].CheckDigit;
			text = text.Remove(0, 2);
		}
		string text2 = (num2 % 103).ToString();
		if (text2.Length == 1)
		{
			text2 = "0" + text2;
		}
		list.Add(base.BarcodeSymbolsString[text2].Bars);
		list.Add(base.BarcodeSymbolsString["Stop"].Bars);
		return list;
	}

	protected string GetDataToEncode(string originalData)
	{
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
		base.StartSymbol = 'þ';
		base.StopSymbol = 'ÿ';
		base.ValidatorExpression = "[0-9]";
		base.BarcodeSymbolsString["00"] = new BarcodeSymbolTable(0, new byte[6] { 2, 1, 2, 2, 2, 2 });
		base.BarcodeSymbolsString["01"] = new BarcodeSymbolTable(1, new byte[6] { 2, 2, 2, 1, 2, 2 });
		base.BarcodeSymbolsString["02"] = new BarcodeSymbolTable(2, new byte[6] { 2, 2, 2, 2, 2, 1 });
		base.BarcodeSymbolsString["03"] = new BarcodeSymbolTable(3, new byte[6] { 1, 2, 1, 2, 2, 3 });
		base.BarcodeSymbolsString["04"] = new BarcodeSymbolTable(4, new byte[6] { 1, 2, 1, 3, 2, 2 });
		base.BarcodeSymbolsString["05"] = new BarcodeSymbolTable(5, new byte[6] { 1, 3, 1, 2, 2, 2 });
		base.BarcodeSymbolsString["06"] = new BarcodeSymbolTable(6, new byte[6] { 1, 2, 2, 2, 1, 3 });
		base.BarcodeSymbolsString["07"] = new BarcodeSymbolTable(7, new byte[6] { 1, 2, 2, 3, 1, 2 });
		base.BarcodeSymbolsString["08"] = new BarcodeSymbolTable(8, new byte[6] { 1, 3, 2, 2, 1, 2 });
		base.BarcodeSymbolsString["09"] = new BarcodeSymbolTable(9, new byte[6] { 2, 2, 1, 2, 1, 3 });
		base.BarcodeSymbolsString["10"] = new BarcodeSymbolTable(10, new byte[6] { 2, 2, 1, 3, 1, 2 });
		base.BarcodeSymbolsString["11"] = new BarcodeSymbolTable(11, new byte[6] { 2, 3, 1, 2, 1, 2 });
		base.BarcodeSymbolsString["12"] = new BarcodeSymbolTable(12, new byte[6] { 1, 1, 2, 2, 3, 2 });
		base.BarcodeSymbolsString["13"] = new BarcodeSymbolTable(13, new byte[6] { 1, 2, 2, 1, 3, 2 });
		base.BarcodeSymbolsString["14"] = new BarcodeSymbolTable(14, new byte[6] { 1, 2, 2, 2, 3, 1 });
		base.BarcodeSymbolsString["15"] = new BarcodeSymbolTable(15, new byte[6] { 1, 1, 3, 2, 2, 2 });
		base.BarcodeSymbolsString["16"] = new BarcodeSymbolTable(16, new byte[6] { 1, 2, 3, 1, 2, 2 });
		base.BarcodeSymbolsString["17"] = new BarcodeSymbolTable(17, new byte[6] { 1, 2, 3, 2, 2, 1 });
		base.BarcodeSymbolsString["18"] = new BarcodeSymbolTable(18, new byte[6] { 2, 2, 3, 2, 1, 1 });
		base.BarcodeSymbolsString["19"] = new BarcodeSymbolTable(19, new byte[6] { 2, 2, 1, 1, 3, 2 });
		base.BarcodeSymbolsString["20"] = new BarcodeSymbolTable(20, new byte[6] { 2, 2, 1, 2, 3, 1 });
		base.BarcodeSymbolsString["21"] = new BarcodeSymbolTable(21, new byte[6] { 2, 1, 3, 2, 1, 2 });
		base.BarcodeSymbolsString["22"] = new BarcodeSymbolTable(22, new byte[6] { 2, 2, 3, 1, 1, 2 });
		base.BarcodeSymbolsString["23"] = new BarcodeSymbolTable(23, new byte[6] { 3, 1, 2, 1, 3, 1 });
		base.BarcodeSymbolsString["24"] = new BarcodeSymbolTable(24, new byte[6] { 3, 1, 1, 2, 2, 2 });
		base.BarcodeSymbolsString["25"] = new BarcodeSymbolTable(25, new byte[6] { 3, 2, 1, 1, 2, 2 });
		base.BarcodeSymbolsString["26"] = new BarcodeSymbolTable(26, new byte[6] { 3, 2, 1, 2, 2, 1 });
		base.BarcodeSymbolsString["27"] = new BarcodeSymbolTable(27, new byte[6] { 3, 1, 2, 2, 1, 2 });
		base.BarcodeSymbolsString["28"] = new BarcodeSymbolTable(28, new byte[6] { 3, 2, 2, 1, 1, 2 });
		base.BarcodeSymbolsString["29"] = new BarcodeSymbolTable(29, new byte[6] { 3, 2, 2, 2, 1, 1 });
		base.BarcodeSymbolsString["30"] = new BarcodeSymbolTable(30, new byte[6] { 2, 1, 2, 1, 2, 3 });
		base.BarcodeSymbolsString["31"] = new BarcodeSymbolTable(31, new byte[6] { 2, 1, 2, 3, 2, 1 });
		base.BarcodeSymbolsString["32"] = new BarcodeSymbolTable(32, new byte[6] { 2, 3, 2, 1, 2, 1 });
		base.BarcodeSymbolsString["33"] = new BarcodeSymbolTable(33, new byte[6] { 1, 1, 1, 3, 2, 3 });
		base.BarcodeSymbolsString["34"] = new BarcodeSymbolTable(34, new byte[6] { 1, 3, 1, 1, 2, 3 });
		base.BarcodeSymbolsString["35"] = new BarcodeSymbolTable(35, new byte[6] { 1, 3, 1, 3, 2, 1 });
		base.BarcodeSymbolsString["36"] = new BarcodeSymbolTable(36, new byte[6] { 1, 1, 2, 3, 1, 3 });
		base.BarcodeSymbolsString["37"] = new BarcodeSymbolTable(37, new byte[6] { 1, 3, 2, 1, 1, 3 });
		base.BarcodeSymbolsString["38"] = new BarcodeSymbolTable(38, new byte[6] { 1, 3, 2, 3, 1, 1 });
		base.BarcodeSymbolsString["39"] = new BarcodeSymbolTable(39, new byte[6] { 2, 1, 1, 3, 1, 3 });
		base.BarcodeSymbolsString["40"] = new BarcodeSymbolTable(40, new byte[6] { 2, 3, 1, 1, 1, 3 });
		base.BarcodeSymbolsString["41"] = new BarcodeSymbolTable(41, new byte[6] { 2, 3, 1, 3, 1, 1 });
		base.BarcodeSymbolsString["42"] = new BarcodeSymbolTable(42, new byte[6] { 1, 1, 2, 1, 3, 3 });
		base.BarcodeSymbolsString["43"] = new BarcodeSymbolTable(43, new byte[6] { 1, 1, 2, 3, 3, 1 });
		base.BarcodeSymbolsString["44"] = new BarcodeSymbolTable(44, new byte[6] { 1, 3, 2, 1, 3, 1 });
		base.BarcodeSymbolsString["45"] = new BarcodeSymbolTable(45, new byte[6] { 1, 1, 3, 1, 2, 3 });
		base.BarcodeSymbolsString["46"] = new BarcodeSymbolTable(46, new byte[6] { 1, 1, 3, 3, 2, 1 });
		base.BarcodeSymbolsString["47"] = new BarcodeSymbolTable(47, new byte[6] { 1, 3, 3, 1, 2, 1 });
		base.BarcodeSymbolsString["48"] = new BarcodeSymbolTable(48, new byte[6] { 3, 1, 3, 1, 2, 1 });
		base.BarcodeSymbolsString["49"] = new BarcodeSymbolTable(49, new byte[6] { 2, 1, 1, 3, 3, 1 });
		base.BarcodeSymbolsString["50"] = new BarcodeSymbolTable(50, new byte[6] { 2, 3, 1, 1, 3, 1 });
		base.BarcodeSymbolsString["51"] = new BarcodeSymbolTable(51, new byte[6] { 2, 1, 3, 1, 1, 3 });
		base.BarcodeSymbolsString["52"] = new BarcodeSymbolTable(52, new byte[6] { 2, 1, 3, 3, 1, 1 });
		base.BarcodeSymbolsString["53"] = new BarcodeSymbolTable(53, new byte[6] { 2, 1, 3, 1, 3, 1 });
		base.BarcodeSymbolsString["54"] = new BarcodeSymbolTable(54, new byte[6] { 3, 1, 1, 1, 2, 3 });
		base.BarcodeSymbolsString["55"] = new BarcodeSymbolTable(55, new byte[6] { 3, 1, 1, 3, 2, 1 });
		base.BarcodeSymbolsString["56"] = new BarcodeSymbolTable(56, new byte[6] { 3, 3, 1, 1, 2, 1 });
		base.BarcodeSymbolsString["57"] = new BarcodeSymbolTable(57, new byte[6] { 3, 1, 2, 1, 1, 3 });
		base.BarcodeSymbolsString["58"] = new BarcodeSymbolTable(58, new byte[6] { 3, 1, 2, 3, 1, 1 });
		base.BarcodeSymbolsString["59"] = new BarcodeSymbolTable(59, new byte[6] { 3, 3, 2, 1, 1, 1 });
		base.BarcodeSymbolsString["60"] = new BarcodeSymbolTable(60, new byte[6] { 3, 1, 4, 1, 1, 1 });
		base.BarcodeSymbolsString["61"] = new BarcodeSymbolTable(61, new byte[6] { 2, 2, 1, 4, 1, 1 });
		base.BarcodeSymbolsString["62"] = new BarcodeSymbolTable(62, new byte[6] { 4, 3, 1, 1, 1, 1 });
		base.BarcodeSymbolsString["63"] = new BarcodeSymbolTable(63, new byte[6] { 1, 1, 1, 2, 2, 4 });
		base.BarcodeSymbolsString["64"] = new BarcodeSymbolTable(64, new byte[6] { 1, 1, 1, 4, 2, 2 });
		base.BarcodeSymbolsString["65"] = new BarcodeSymbolTable(65, new byte[6] { 1, 2, 1, 1, 2, 4 });
		base.BarcodeSymbolsString["66"] = new BarcodeSymbolTable(66, new byte[6] { 1, 2, 1, 4, 2, 1 });
		base.BarcodeSymbolsString["67"] = new BarcodeSymbolTable(67, new byte[6] { 1, 4, 1, 1, 2, 2 });
		base.BarcodeSymbolsString["68"] = new BarcodeSymbolTable(68, new byte[6] { 1, 4, 1, 2, 2, 1 });
		base.BarcodeSymbolsString["69"] = new BarcodeSymbolTable(69, new byte[6] { 1, 1, 2, 2, 1, 4 });
		base.BarcodeSymbolsString["70"] = new BarcodeSymbolTable(70, new byte[6] { 1, 1, 2, 4, 1, 2 });
		base.BarcodeSymbolsString["71"] = new BarcodeSymbolTable(71, new byte[6] { 1, 2, 2, 1, 1, 4 });
		base.BarcodeSymbolsString["72"] = new BarcodeSymbolTable(72, new byte[6] { 1, 2, 2, 4, 1, 1 });
		base.BarcodeSymbolsString["73"] = new BarcodeSymbolTable(73, new byte[6] { 1, 4, 2, 1, 1, 2 });
		base.BarcodeSymbolsString["74"] = new BarcodeSymbolTable(74, new byte[6] { 1, 4, 2, 2, 1, 1 });
		base.BarcodeSymbolsString["75"] = new BarcodeSymbolTable(75, new byte[6] { 2, 4, 1, 2, 1, 1 });
		base.BarcodeSymbolsString["76"] = new BarcodeSymbolTable(76, new byte[6] { 2, 2, 1, 1, 1, 4 });
		base.BarcodeSymbolsString["77"] = new BarcodeSymbolTable(77, new byte[6] { 4, 1, 3, 1, 1, 1 });
		base.BarcodeSymbolsString["78"] = new BarcodeSymbolTable(78, new byte[6] { 2, 4, 1, 1, 1, 2 });
		base.BarcodeSymbolsString["79"] = new BarcodeSymbolTable(79, new byte[6] { 1, 3, 4, 1, 1, 1 });
		base.BarcodeSymbolsString["80"] = new BarcodeSymbolTable(80, new byte[6] { 1, 1, 1, 2, 4, 2 });
		base.BarcodeSymbolsString["81"] = new BarcodeSymbolTable(81, new byte[6] { 1, 2, 1, 1, 4, 2 });
		base.BarcodeSymbolsString["82"] = new BarcodeSymbolTable(82, new byte[6] { 1, 2, 1, 2, 4, 1 });
		base.BarcodeSymbolsString["83"] = new BarcodeSymbolTable(83, new byte[6] { 1, 1, 4, 2, 1, 2 });
		base.BarcodeSymbolsString["84"] = new BarcodeSymbolTable(84, new byte[6] { 1, 2, 4, 1, 1, 2 });
		base.BarcodeSymbolsString["85"] = new BarcodeSymbolTable(85, new byte[6] { 1, 2, 4, 2, 1, 1 });
		base.BarcodeSymbolsString["86"] = new BarcodeSymbolTable(86, new byte[6] { 4, 1, 1, 2, 1, 2 });
		base.BarcodeSymbolsString["87"] = new BarcodeSymbolTable(87, new byte[6] { 4, 2, 1, 1, 1, 2 });
		base.BarcodeSymbolsString["88"] = new BarcodeSymbolTable(88, new byte[6] { 4, 2, 1, 2, 1, 1 });
		base.BarcodeSymbolsString["89"] = new BarcodeSymbolTable(89, new byte[6] { 2, 1, 2, 1, 4, 1 });
		base.BarcodeSymbolsString["90"] = new BarcodeSymbolTable(90, new byte[6] { 2, 1, 4, 1, 2, 1 });
		base.BarcodeSymbolsString["91"] = new BarcodeSymbolTable(91, new byte[6] { 4, 1, 2, 1, 2, 1 });
		base.BarcodeSymbolsString["92"] = new BarcodeSymbolTable(92, new byte[6] { 1, 1, 1, 1, 4, 3 });
		base.BarcodeSymbolsString["93"] = new BarcodeSymbolTable(93, new byte[6] { 1, 1, 1, 3, 4, 1 });
		base.BarcodeSymbolsString["94"] = new BarcodeSymbolTable(94, new byte[6] { 1, 3, 1, 1, 4, 1 });
		base.BarcodeSymbolsString["95"] = new BarcodeSymbolTable(95, new byte[6] { 1, 1, 4, 1, 1, 3 });
		base.BarcodeSymbolsString["96"] = new BarcodeSymbolTable(96, new byte[6] { 1, 1, 4, 3, 1, 1 });
		base.BarcodeSymbolsString["97"] = new BarcodeSymbolTable(97, new byte[6] { 4, 1, 1, 1, 1, 3 });
		base.BarcodeSymbolsString["98"] = new BarcodeSymbolTable(98, new byte[6] { 4, 1, 1, 3, 1, 1 });
		base.BarcodeSymbolsString["99"] = new BarcodeSymbolTable(99, new byte[6] { 1, 1, 3, 1, 4, 1 });
		base.BarcodeSymbolsString["100"] = new BarcodeSymbolTable(100, new byte[6] { 1, 1, 4, 1, 3, 1 });
		base.BarcodeSymbolsString["101"] = new BarcodeSymbolTable(101, new byte[6] { 3, 1, 1, 1, 4, 1 });
		base.BarcodeSymbolsString["102"] = new BarcodeSymbolTable(102, new byte[6] { 4, 1, 1, 1, 3, 1 });
		base.BarcodeSymbolsString["CodeB"] = new BarcodeSymbolTable(100, new byte[6] { 1, 1, 4, 1, 3, 1 });
		base.BarcodeSymbolsString["CodeA"] = new BarcodeSymbolTable(101, new byte[6] { 3, 1, 1, 1, 4, 1 });
		base.BarcodeSymbolsString["FNC1"] = new BarcodeSymbolTable(102, new byte[6] { 4, 1, 1, 1, 3, 1 });
		base.BarcodeSymbolsString["StartCodeA"] = new BarcodeSymbolTable(103, new byte[6] { 2, 1, 1, 4, 1, 2 });
		base.BarcodeSymbolsString["StartCodeB"] = new BarcodeSymbolTable(104, new byte[6] { 2, 1, 1, 2, 1, 4 });
		base.BarcodeSymbolsString["StartCodeC"] = new BarcodeSymbolTable(105, new byte[6] { 2, 1, 1, 2, 3, 2 });
		base.BarcodeSymbolsString["Stop"] = new BarcodeSymbolTable(106, new byte[7] { 2, 3, 3, 1, 1, 1, 2 });
		base.BarcodeSymbolsString["CodeC"] = new BarcodeSymbolTable(99, new byte[6] { 1, 1, 3, 1, 4, 1 });
		base.BarcodeSymbols['\0'] = new BarcodeSymbolTable('\0', 0, new byte[6] { 2, 1, 2, 2, 2, 2 });
		base.BarcodeSymbols['\u0001'] = new BarcodeSymbolTable('\u0001', 1, new byte[6] { 2, 2, 2, 1, 2, 2 });
		base.BarcodeSymbols['\u0002'] = new BarcodeSymbolTable('\u0002', 2, new byte[6] { 2, 2, 2, 2, 2, 1 });
		base.BarcodeSymbols['\u0003'] = new BarcodeSymbolTable('\u0003', 3, new byte[6] { 1, 2, 1, 2, 2, 3 });
		base.BarcodeSymbols['\u0004'] = new BarcodeSymbolTable('\u0004', 4, new byte[6] { 1, 2, 1, 3, 2, 2 });
		base.BarcodeSymbols['\u0005'] = new BarcodeSymbolTable('\u0005', 5, new byte[6] { 1, 3, 1, 2, 2, 2 });
		base.BarcodeSymbols['\u0006'] = new BarcodeSymbolTable('\u0006', 6, new byte[6] { 1, 2, 2, 2, 1, 3 });
		base.BarcodeSymbols['\a'] = new BarcodeSymbolTable('\a', 7, new byte[6] { 1, 2, 2, 3, 1, 2 });
		base.BarcodeSymbols['\b'] = new BarcodeSymbolTable('\b', 8, new byte[6] { 1, 3, 2, 2, 1, 2 });
		base.BarcodeSymbols['\t'] = new BarcodeSymbolTable('\t', 9, new byte[6] { 2, 2, 1, 2, 1, 3 });
		base.BarcodeSymbols['\n'] = new BarcodeSymbolTable('\n', 10, new byte[6] { 2, 2, 1, 3, 1, 2 });
		base.BarcodeSymbols['\v'] = new BarcodeSymbolTable('\v', 11, new byte[6] { 2, 3, 1, 2, 1, 2 });
		base.BarcodeSymbols['\f'] = new BarcodeSymbolTable('\f', 12, new byte[6] { 1, 1, 2, 2, 3, 2 });
		base.BarcodeSymbols['\r'] = new BarcodeSymbolTable('\r', 13, new byte[6] { 1, 2, 2, 1, 3, 2 });
		base.BarcodeSymbols['\u000e'] = new BarcodeSymbolTable('\u000e', 14, new byte[6] { 1, 2, 2, 2, 3, 1 });
		base.BarcodeSymbols['\u000f'] = new BarcodeSymbolTable('\u000f', 15, new byte[6] { 1, 1, 3, 2, 2, 2 });
		base.BarcodeSymbols['\u0010'] = new BarcodeSymbolTable('\u0010', 16, new byte[6] { 1, 2, 3, 1, 2, 2 });
		base.BarcodeSymbols['\u0011'] = new BarcodeSymbolTable('\u0011', 17, new byte[6] { 1, 2, 3, 2, 2, 1 });
		base.BarcodeSymbols['\u0012'] = new BarcodeSymbolTable('\u0012', 18, new byte[6] { 2, 2, 3, 2, 1, 1 });
		base.BarcodeSymbols['\u0013'] = new BarcodeSymbolTable('\u0013', 19, new byte[6] { 2, 2, 1, 1, 3, 2 });
		base.BarcodeSymbols['\u0014'] = new BarcodeSymbolTable('\u0014', 20, new byte[6] { 2, 2, 1, 2, 3, 1 });
		base.BarcodeSymbols['\u0015'] = new BarcodeSymbolTable('\u0015', 21, new byte[6] { 2, 1, 3, 2, 1, 2 });
		base.BarcodeSymbols['\u0016'] = new BarcodeSymbolTable('\u0016', 22, new byte[6] { 2, 2, 3, 1, 1, 2 });
		base.BarcodeSymbols['\u0017'] = new BarcodeSymbolTable('\u0017', 23, new byte[6] { 3, 1, 2, 1, 3, 1 });
		base.BarcodeSymbols['\u0018'] = new BarcodeSymbolTable('\u0018', 24, new byte[6] { 3, 1, 1, 2, 2, 2 });
		base.BarcodeSymbols['\u0019'] = new BarcodeSymbolTable('\u0019', 25, new byte[6] { 3, 2, 1, 1, 2, 2 });
		base.BarcodeSymbols['\u001a'] = new BarcodeSymbolTable('\u001a', 26, new byte[6] { 3, 2, 1, 2, 2, 1 });
		base.BarcodeSymbols['\u001b'] = new BarcodeSymbolTable('\u001b', 27, new byte[6] { 3, 1, 2, 2, 1, 2 });
		base.BarcodeSymbols['\u001c'] = new BarcodeSymbolTable('\u001c', 28, new byte[6] { 3, 2, 2, 1, 1, 2 });
		base.BarcodeSymbols['\u001d'] = new BarcodeSymbolTable('\u001d', 29, new byte[6] { 3, 2, 2, 2, 1, 1 });
		base.BarcodeSymbols['\u001e'] = new BarcodeSymbolTable('\u001e', 30, new byte[6] { 2, 1, 2, 1, 2, 3 });
		base.BarcodeSymbols['\u001f'] = new BarcodeSymbolTable('\u001f', 31, new byte[6] { 2, 1, 2, 3, 2, 1 });
		base.BarcodeSymbols[' '] = new BarcodeSymbolTable(' ', 32, new byte[6] { 2, 3, 2, 1, 2, 1 });
		base.BarcodeSymbols['!'] = new BarcodeSymbolTable('!', 33, new byte[6] { 1, 1, 1, 3, 2, 3 });
		base.BarcodeSymbols['"'] = new BarcodeSymbolTable('"', 34, new byte[6] { 1, 3, 1, 1, 2, 3 });
		base.BarcodeSymbols['#'] = new BarcodeSymbolTable('#', 35, new byte[6] { 1, 3, 1, 3, 2, 1 });
		base.BarcodeSymbols['$'] = new BarcodeSymbolTable('$', 36, new byte[6] { 1, 1, 2, 3, 1, 3 });
		base.BarcodeSymbols['%'] = new BarcodeSymbolTable('%', 37, new byte[6] { 1, 3, 2, 1, 1, 3 });
		base.BarcodeSymbols['&'] = new BarcodeSymbolTable('&', 38, new byte[6] { 1, 3, 2, 3, 1, 1 });
		base.BarcodeSymbols['\''] = new BarcodeSymbolTable('\'', 39, new byte[6] { 2, 1, 1, 3, 1, 3 });
		base.BarcodeSymbols['('] = new BarcodeSymbolTable('(', 40, new byte[6] { 2, 3, 1, 1, 1, 3 });
		base.BarcodeSymbols[')'] = new BarcodeSymbolTable(')', 41, new byte[6] { 2, 3, 1, 3, 1, 1 });
		base.BarcodeSymbols['*'] = new BarcodeSymbolTable('*', 42, new byte[6] { 1, 1, 2, 1, 3, 3 });
		base.BarcodeSymbols['+'] = new BarcodeSymbolTable('+', 43, new byte[6] { 1, 1, 2, 3, 3, 1 });
		base.BarcodeSymbols[','] = new BarcodeSymbolTable(',', 44, new byte[6] { 1, 3, 2, 1, 3, 1 });
		base.BarcodeSymbols['-'] = new BarcodeSymbolTable('-', 45, new byte[6] { 1, 1, 3, 1, 2, 3 });
		base.BarcodeSymbols['.'] = new BarcodeSymbolTable('.', 46, new byte[6] { 1, 1, 3, 3, 2, 1 });
		base.BarcodeSymbols['/'] = new BarcodeSymbolTable('/', 47, new byte[6] { 1, 3, 3, 1, 2, 1 });
		base.BarcodeSymbols['0'] = new BarcodeSymbolTable('0', 48, new byte[6] { 3, 1, 3, 1, 2, 1 });
		base.BarcodeSymbols['1'] = new BarcodeSymbolTable('1', 49, new byte[6] { 2, 1, 1, 3, 3, 1 });
		base.BarcodeSymbols['2'] = new BarcodeSymbolTable('2', 50, new byte[6] { 2, 3, 1, 1, 3, 1 });
		base.BarcodeSymbols['3'] = new BarcodeSymbolTable('3', 51, new byte[6] { 2, 1, 3, 1, 1, 3 });
		base.BarcodeSymbols['4'] = new BarcodeSymbolTable('4', 52, new byte[6] { 2, 1, 3, 3, 1, 1 });
		base.BarcodeSymbols['5'] = new BarcodeSymbolTable('5', 53, new byte[6] { 2, 1, 3, 1, 3, 1 });
		base.BarcodeSymbols['6'] = new BarcodeSymbolTable('6', 54, new byte[6] { 3, 1, 1, 1, 2, 3 });
		base.BarcodeSymbols['7'] = new BarcodeSymbolTable('7', 55, new byte[6] { 3, 1, 1, 3, 2, 1 });
		base.BarcodeSymbols['8'] = new BarcodeSymbolTable('8', 56, new byte[6] { 3, 3, 1, 1, 2, 1 });
		base.BarcodeSymbols['9'] = new BarcodeSymbolTable('9', 57, new byte[6] { 3, 1, 2, 1, 1, 3 });
		base.BarcodeSymbols[':'] = new BarcodeSymbolTable(':', 58, new byte[6] { 3, 1, 2, 3, 1, 1 });
		base.BarcodeSymbols[';'] = new BarcodeSymbolTable(';', 59, new byte[6] { 3, 3, 2, 1, 1, 1 });
		base.BarcodeSymbols['<'] = new BarcodeSymbolTable('<', 60, new byte[6] { 3, 1, 4, 1, 1, 1 });
		base.BarcodeSymbols['='] = new BarcodeSymbolTable('=', 61, new byte[6] { 2, 2, 1, 4, 1, 1 });
		base.BarcodeSymbols['>'] = new BarcodeSymbolTable('>', 62, new byte[6] { 4, 3, 1, 1, 1, 1 });
		base.BarcodeSymbols['?'] = new BarcodeSymbolTable('?', 63, new byte[6] { 1, 1, 1, 2, 2, 4 });
		base.BarcodeSymbols['@'] = new BarcodeSymbolTable('@', 64, new byte[6] { 1, 1, 1, 4, 2, 2 });
		base.BarcodeSymbols['A'] = new BarcodeSymbolTable('A', 65, new byte[6] { 1, 2, 1, 1, 2, 4 });
		base.BarcodeSymbols['B'] = new BarcodeSymbolTable('B', 66, new byte[6] { 1, 2, 1, 4, 2, 1 });
		base.BarcodeSymbols['C'] = new BarcodeSymbolTable('C', 67, new byte[6] { 1, 4, 1, 1, 2, 2 });
		base.BarcodeSymbols['D'] = new BarcodeSymbolTable('D', 68, new byte[6] { 1, 4, 1, 2, 2, 1 });
		base.BarcodeSymbols['E'] = new BarcodeSymbolTable('E', 69, new byte[6] { 1, 1, 2, 2, 1, 4 });
		base.BarcodeSymbols['F'] = new BarcodeSymbolTable('F', 70, new byte[6] { 1, 1, 2, 4, 1, 2 });
		base.BarcodeSymbols['G'] = new BarcodeSymbolTable('G', 71, new byte[6] { 1, 2, 2, 1, 1, 4 });
		base.BarcodeSymbols['H'] = new BarcodeSymbolTable('H', 72, new byte[6] { 1, 2, 2, 4, 1, 1 });
		base.BarcodeSymbols['I'] = new BarcodeSymbolTable('I', 73, new byte[6] { 1, 4, 2, 1, 1, 2 });
		base.BarcodeSymbols['J'] = new BarcodeSymbolTable('J', 74, new byte[6] { 1, 4, 2, 2, 1, 1 });
		base.BarcodeSymbols['K'] = new BarcodeSymbolTable('K', 75, new byte[6] { 2, 4, 1, 2, 1, 1 });
		base.BarcodeSymbols['L'] = new BarcodeSymbolTable('L', 76, new byte[6] { 2, 2, 1, 1, 1, 4 });
		base.BarcodeSymbols['M'] = new BarcodeSymbolTable('M', 77, new byte[6] { 4, 1, 3, 1, 1, 1 });
		base.BarcodeSymbols['N'] = new BarcodeSymbolTable('N', 78, new byte[6] { 2, 4, 1, 1, 1, 2 });
		base.BarcodeSymbols['O'] = new BarcodeSymbolTable('O', 79, new byte[6] { 1, 3, 4, 1, 1, 1 });
		base.BarcodeSymbols['P'] = new BarcodeSymbolTable('P', 80, new byte[6] { 1, 1, 1, 2, 4, 2 });
		base.BarcodeSymbols['Q'] = new BarcodeSymbolTable('Q', 81, new byte[6] { 1, 2, 1, 1, 4, 2 });
		base.BarcodeSymbols['R'] = new BarcodeSymbolTable('R', 82, new byte[6] { 1, 2, 1, 2, 4, 1 });
		base.BarcodeSymbols['S'] = new BarcodeSymbolTable('S', 83, new byte[6] { 1, 1, 4, 2, 1, 2 });
		base.BarcodeSymbols['T'] = new BarcodeSymbolTable('T', 84, new byte[6] { 1, 2, 4, 1, 1, 2 });
		base.BarcodeSymbols['U'] = new BarcodeSymbolTable('U', 85, new byte[6] { 1, 2, 4, 2, 1, 1 });
		base.BarcodeSymbols['V'] = new BarcodeSymbolTable('V', 86, new byte[6] { 4, 1, 1, 2, 1, 2 });
		base.BarcodeSymbols['W'] = new BarcodeSymbolTable('W', 87, new byte[6] { 4, 2, 1, 1, 1, 2 });
		base.BarcodeSymbols['X'] = new BarcodeSymbolTable('X', 88, new byte[6] { 4, 2, 1, 2, 1, 1 });
		base.BarcodeSymbols['Y'] = new BarcodeSymbolTable('Y', 89, new byte[6] { 2, 1, 2, 1, 4, 1 });
		base.BarcodeSymbols['Z'] = new BarcodeSymbolTable('Z', 90, new byte[6] { 2, 1, 4, 1, 2, 1 });
		base.BarcodeSymbols['['] = new BarcodeSymbolTable('[', 91, new byte[6] { 4, 1, 2, 1, 2, 1 });
		base.BarcodeSymbols['\\'] = new BarcodeSymbolTable('\\', 92, new byte[6] { 1, 1, 1, 1, 4, 3 });
		base.BarcodeSymbols[']'] = new BarcodeSymbolTable(']', 93, new byte[6] { 1, 1, 1, 3, 4, 1 });
		base.BarcodeSymbols['^'] = new BarcodeSymbolTable('^', 94, new byte[6] { 1, 3, 1, 1, 4, 1 });
		base.BarcodeSymbols['_'] = new BarcodeSymbolTable('_', 95, new byte[6] { 1, 1, 4, 1, 1, 3 });
		base.BarcodeSymbols['`'] = new BarcodeSymbolTable('`', 96, new byte[6] { 1, 1, 4, 3, 1, 1 });
		base.BarcodeSymbols['a'] = new BarcodeSymbolTable('a', 97, new byte[6] { 4, 1, 1, 1, 1, 3 });
		base.BarcodeSymbols['b'] = new BarcodeSymbolTable('b', 98, new byte[6] { 4, 1, 1, 3, 1, 1 });
		base.BarcodeSymbols['c'] = new BarcodeSymbolTable('c', 99, new byte[6] { 1, 1, 3, 1, 4, 1 });
		base.BarcodeSymbols['ð'] = new BarcodeSymbolTable('ð', 102, new byte[6] { 4, 1, 1, 1, 3, 1 });
		base.BarcodeSymbols['ú'] = new BarcodeSymbolTable('ú', 101, new byte[6] { 3, 1, 1, 1, 4, 1 });
		base.BarcodeSymbols['û'] = new BarcodeSymbolTable('û', 100, new byte[6] { 1, 1, 4, 1, 3, 1 });
		base.BarcodeSymbols['þ'] = new BarcodeSymbolTable('þ', 105, new byte[6] { 2, 1, 1, 2, 3, 2 });
		base.BarcodeSymbols['ÿ'] = new BarcodeSymbolTable('ÿ', -1, new byte[7] { 2, 3, 3, 1, 1, 1, 2 });
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
