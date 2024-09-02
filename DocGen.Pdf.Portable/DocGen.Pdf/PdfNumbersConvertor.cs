using System;
using System.Collections.Generic;
using System.Text;

namespace DocGen.Pdf;

internal class PdfNumbersConvertor
{
	private const float LetterLimit = 26f;

	private const int AcsiiStartIndex = 64;

	public static string Convert(int intArabic, PdfNumberStyle numberStyle)
	{
		return numberStyle switch
		{
			PdfNumberStyle.None => string.Empty, 
			PdfNumberStyle.Numeric => intArabic.ToString(), 
			PdfNumberStyle.LowerLatin => ArabicToLetter(intArabic).ToLower(), 
			PdfNumberStyle.LowerRoman => ArabicToRoman(intArabic).ToLower(), 
			PdfNumberStyle.UpperLatin => ArabicToLetter(intArabic), 
			PdfNumberStyle.UpperRoman => ArabicToRoman(intArabic), 
			_ => string.Empty, 
		};
	}

	private static string ArabicToRoman(int intArabic)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(GenerateNumber(ref intArabic, 1000, "M"));
		stringBuilder.Append(GenerateNumber(ref intArabic, 900, "CM"));
		stringBuilder.Append(GenerateNumber(ref intArabic, 500, "D"));
		stringBuilder.Append(GenerateNumber(ref intArabic, 400, "CD"));
		stringBuilder.Append(GenerateNumber(ref intArabic, 100, "C"));
		stringBuilder.Append(GenerateNumber(ref intArabic, 90, "XC"));
		stringBuilder.Append(GenerateNumber(ref intArabic, 50, "L"));
		stringBuilder.Append(GenerateNumber(ref intArabic, 40, "XL"));
		stringBuilder.Append(GenerateNumber(ref intArabic, 10, "X"));
		stringBuilder.Append(GenerateNumber(ref intArabic, 9, "IX"));
		stringBuilder.Append(GenerateNumber(ref intArabic, 5, "V"));
		stringBuilder.Append(GenerateNumber(ref intArabic, 4, "IV"));
		stringBuilder.Append(GenerateNumber(ref intArabic, 1, "I"));
		return stringBuilder.ToString();
	}

	private static string ArabicToLetter(int arabic)
	{
		Stack<int> stack = ConvertToLetter(arabic);
		StringBuilder stringBuilder = new StringBuilder();
		while (stack.Count > 0)
		{
			int number = stack.Pop();
			AppendChar(stringBuilder, number);
		}
		return stringBuilder.ToString();
	}

	private static string GenerateNumber(ref int value, int magnitude, string letter)
	{
		StringBuilder stringBuilder = new StringBuilder();
		while (value >= magnitude)
		{
			value -= magnitude;
			stringBuilder.Append(letter);
		}
		return stringBuilder.ToString();
	}

	private static Stack<int> ConvertToLetter(float arabic)
	{
		if (arabic <= 0f)
		{
			throw new ArgumentOutOfRangeException("arabic", "Value can not be less 0");
		}
		Stack<int> stack = new Stack<int>();
		while ((float)(int)arabic > 26f)
		{
			float num = arabic % 26f;
			if (num == 0f)
			{
				arabic = arabic / 26f - 1f;
				num = 26f;
			}
			else
			{
				arabic /= 26f;
			}
			stack.Push((int)num);
		}
		if (arabic > 0f)
		{
			stack.Push((int)arabic);
		}
		return stack;
	}

	private static void AppendChar(StringBuilder builder, int number)
	{
		if (builder == null)
		{
			throw new ArgumentNullException("builder");
		}
		if (number <= 0 || number > 26)
		{
			throw new ArgumentOutOfRangeException("number", "Value can not be less 0 and greater 26");
		}
		char value = (char)(64 + number);
		builder.Append(value);
	}
}
