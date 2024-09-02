using System;
using System.Globalization;
using DocGen.DocIO.ReaderWriter.Biff_Records;

namespace DocGen.DocIO.DLS;

internal class FormFieldPropertiesConverter
{
	public static void ReadFormFieldProperties(WFormField formField, FormField frmField)
	{
		formField.Name = frmField.Title;
		if (frmField.Title == null)
		{
			return;
		}
		formField.Help = frmField.Help;
		formField.MacroOnEnd = frmField.MacroOnEnd;
		formField.MacroOnStart = frmField.MacroOnStart;
		formField.StatusBarHelp = frmField.Tooltip;
		formField.Value = frmField.Value;
		formField.Params = frmField.Params;
		if (frmField.FieldType == FieldType.FieldFormDropDown)
		{
			WDropDownFormField wDropDownFormField = formField as WDropDownFormField;
			wDropDownFormField.DefaultDropDownValue = frmField.DefaultDropDownValue;
			wDropDownFormField.DropDownSelectedIndex = frmField.DropDownIndex;
			for (int i = 0; i < frmField.DropDownItems.Count; i++)
			{
				wDropDownFormField.DropDownItems.Add(frmField.DropDownItems[i]);
			}
			if (frmField.DropDownItems.Count > 0)
			{
				wDropDownFormField.DropDownValue = frmField.DropDownValue;
			}
		}
		else if (frmField.FieldType == FieldType.FieldFormCheckBox)
		{
			WCheckBox obj = formField as WCheckBox;
			obj.SetCheckBoxSizeValue(frmField.CheckBoxSize / 2);
			obj.DefaultCheckBoxValue = frmField.DefaultCheckBoxValue;
		}
		else if (frmField.FieldType == FieldType.FieldFormTextInput)
		{
			WTextFormField wTextFormField = formField as WTextFormField;
			wTextFormField.MaximumLength = frmField.MaxLength;
			wTextFormField.StringFormat = frmField.Format;
			wTextFormField.SetTextFormFieldType(frmField.TextFormFieldType);
			wTextFormField.DefaultText = frmField.DefaultTextInputValue;
			if (wTextFormField.Type == TextFormFieldType.RegularText)
			{
				wTextFormField.TextFormat = GetTextFormat(frmField.Format);
			}
		}
	}

	public static void WriteFormFieldProperties(FormField frmField, WFormField formField)
	{
		frmField.Help = formField.Help;
		frmField.MacroOnEnd = formField.MacroOnEnd;
		frmField.MacroOnStart = formField.MacroOnStart;
		frmField.Params = (short)formField.Params;
		frmField.Title = formField.Name;
		frmField.Tooltip = formField.StatusBarHelp;
		frmField.Value = formField.Value;
		if (formField.FormFieldType == FormFieldType.DropDown)
		{
			WDropDownFormField wDropDownFormField = formField as WDropDownFormField;
			for (int i = 0; i < wDropDownFormField.DropDownItems.Count; i++)
			{
				frmField.DropDownItems.Add(wDropDownFormField.DropDownItems[i].Text);
			}
			frmField.DefaultDropDownValue = wDropDownFormField.DefaultDropDownValue;
			frmField.DropDownIndex = wDropDownFormField.DropDownSelectedIndex;
			if (wDropDownFormField.DropDownItems.Count > 0)
			{
				if (wDropDownFormField.DropDownItems.Count <= wDropDownFormField.DropDownSelectedIndex)
				{
					throw new ArgumentException("DropDownItem with index " + wDropDownFormField.DropDownSelectedIndex + " doesn't exist");
				}
				frmField.DropDownValue = wDropDownFormField.DropDownValue;
			}
		}
		else if (formField.FormFieldType == FormFieldType.CheckBox)
		{
			WCheckBox wCheckBox = formField as WCheckBox;
			frmField.CheckBoxSize = wCheckBox.CheckBoxSize * 2;
			frmField.DefaultCheckBoxValue = wCheckBox.DefaultCheckBoxValue;
		}
		else if (formField.FormFieldType == FormFieldType.TextInput)
		{
			WTextFormField wTextFormField = formField as WTextFormField;
			frmField.MaxLength = wTextFormField.MaximumLength;
			frmField.TextFormFieldType = wTextFormField.Type;
			if (wTextFormField.Type == TextFormFieldType.RegularText)
			{
				frmField.Format = GetStringTextFormat(wTextFormField);
				frmField.DefaultTextInputValue = FormatText(wTextFormField.TextFormat, wTextFormField.DefaultText);
			}
			else
			{
				frmField.Format = wTextFormField.StringFormat;
				frmField.DefaultTextInputValue = wTextFormField.DefaultText;
			}
		}
	}

	private static TextFormat GetTextFormat(string formFieldFormat)
	{
		return formFieldFormat switch
		{
			"UPPERCASE" => TextFormat.Uppercase, 
			"LOWERCASE" => TextFormat.Lowercase, 
			"FIRST CAPITAL" => TextFormat.FirstCapital, 
			"TITLE CASE" => TextFormat.Titlecase, 
			_ => TextFormat.None, 
		};
	}

	private static string GetStringTextFormat(WTextFormField formField)
	{
		return formField.TextFormat switch
		{
			TextFormat.Uppercase => "UPPERCASE", 
			TextFormat.Lowercase => "LOWERCASE", 
			TextFormat.FirstCapital => "FIRST CAPITAL", 
			TextFormat.Titlecase => "TITLE CASE", 
			_ => string.Empty, 
		};
	}

	private static NumberFormat GetNumberFormat(string formFieldFormat)
	{
		switch (formFieldFormat)
		{
		case "0":
			return NumberFormat.WholeNumber;
		case "0,00":
			return NumberFormat.FloatingPoint;
		case "0%":
			return NumberFormat.WholeNumberPercent;
		case "0,00%":
			return NumberFormat.FloatingPointPercent;
		case "#\ufffd##0":
			return NumberFormat.WholeNumberWithSpace;
		case "#\ufffd##0,00":
			return NumberFormat.FloatingPointWithSpace;
		default:
			if (formFieldFormat.StartsWith("#\ufffd##0,00 "))
			{
				return NumberFormat.CurrencyFormat;
			}
			return NumberFormat.None;
		}
	}

	private static string GetStringNumberFormat(NumberFormat numberFormat)
	{
		return numberFormat switch
		{
			NumberFormat.WholeNumber => "0", 
			NumberFormat.FloatingPoint => "0,00", 
			NumberFormat.WholeNumberPercent => "0%", 
			NumberFormat.FloatingPointPercent => "0,00%", 
			NumberFormat.WholeNumberWithSpace => "#\ufffd##0", 
			NumberFormat.FloatingPointWithSpace => "#\ufffd##0,00", 
			NumberFormat.CurrencyFormat => "#\ufffd##0.00 " + CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol + ";(#\ufffd##0.00 " + CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol + ")", 
			_ => string.Empty, 
		};
	}

	private static string GetDefaultNumberValue(NumberFormat numberFormat)
	{
		switch (numberFormat)
		{
		case NumberFormat.WholeNumber:
		case NumberFormat.WholeNumberWithSpace:
			return "0";
		case NumberFormat.FloatingPoint:
		case NumberFormat.FloatingPointWithSpace:
			return "0,00";
		case NumberFormat.WholeNumberPercent:
			return "0%";
		case NumberFormat.FloatingPointPercent:
			return "0,00%";
		case NumberFormat.CurrencyFormat:
			return "0,00 " + CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol;
		default:
			return string.Empty;
		}
	}

	internal static string FormatText(TextFormat textFormat, string text)
	{
		if (text != string.Empty)
		{
			switch (textFormat)
			{
			case TextFormat.Uppercase:
				return text.ToUpper();
			case TextFormat.Lowercase:
				return text.ToLower();
			case TextFormat.FirstCapital:
				return text[0].ToString().ToUpper() + text.Remove(0, 1);
			case TextFormat.Titlecase:
			{
				string[] array = text.Split(new char[1] { ' ' });
				for (int i = 0; i < array.Length; i++)
				{
					string text2 = array[i];
					_ = text2[0].ToString().ToUpper() + text2.Remove(0, 1);
					array[i] = text2;
				}
				text = string.Empty;
				int num = array.Length;
				for (int j = 0; j < num; j++)
				{
					text += array[j];
					if (j < num - 1)
					{
						text += " ";
					}
				}
				return text;
			}
			}
		}
		return text;
	}

	private static string FormatNumberText(string format, NumberFormat numberFormat, string inputData)
	{
		if (numberFormat == NumberFormat.None)
		{
			return inputData;
		}
		inputData = inputData.Replace('.', ',');
		double num = 0.0;
		string empty = string.Empty;
		try
		{
			num = Convert.ToDouble(inputData);
		}
		catch
		{
			return GetDefaultNumberValue(numberFormat);
		}
		switch (numberFormat)
		{
		case NumberFormat.WholeNumberPercent:
			num *= 100.0;
			num = Math.Floor(num);
			num /= 100.0;
			goto IL_0089;
		case NumberFormat.WholeNumber:
			num = Math.Floor(num);
			goto IL_0089;
		case NumberFormat.FloatingPoint:
		case NumberFormat.FloatingPointPercent:
			return ConvertNumberToString(format, num);
		case NumberFormat.WholeNumberWithSpace:
			num = Math.Floor(num);
			goto case NumberFormat.FloatingPointWithSpace;
		case NumberFormat.FloatingPointWithSpace:
		case NumberFormat.CurrencyFormat:
			empty = ConvertNumberToString(format, num);
			if (num < 1000.0)
			{
				empty = empty.Substring(1, empty.Length - 1);
			}
			return empty;
		default:
			{
				if (format != string.Empty)
				{
					return ConvertNumberToString(format, num);
				}
				return string.Empty;
			}
			IL_0089:
			return ConvertNumberToString(format, num);
		}
	}

	private static string ConvertNumberToString(string format, double dValue)
	{
		double num = 0.0;
		if (format[format.Length - 1] == '%')
		{
			dValue *= 100.0;
			num = Math.Round(dValue, 2);
			if (num > dValue)
			{
				num -= 0.01;
			}
			num /= 100.0;
		}
		else
		{
			num = Math.Round(dValue, 2);
			if (num > dValue)
			{
				num -= 0.01;
			}
		}
		return num.ToString(format, CultureInfo.InvariantCulture);
	}
}
