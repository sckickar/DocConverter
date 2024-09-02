using System;
using System.Collections.Generic;

namespace DocGen.Office;

internal class LaTeXConverter
{
	private const string m_groupStart = "{";

	private const string m_groupEnd = "}";

	private const string m_squareStart = "[";

	private const string m_squareEnd = "]";

	private const string m_controlStart = "\\";

	private const string m_space = " ";

	private const string m_matrixRowEnd = "\\\\";

	private const string m_matrixColEnd = "&";

	private const string m_superScript = "^";

	private const string m_subScript = "_";

	private const string m_lowLimit = "\\below";

	private string m_laTexString;

	private string m_currentLaTexElement;

	private int m_laTexStringPosition;

	private IOfficeMathParagraph m_mathPara;

	private DocumentLaTeXConverter m_documentLaTeXConverter;

	private OfficeMath m_currOfficeMath;

	private bool m_isToUseExistingCurrElement;

	private MathStyleType m_currMathStyleType;

	private MathFontType m_currMathFontType = MathFontType.Roman;

	private bool m_currHasNormalText;

	private bool isFunctionNameProcessing;

	internal void ParseMathPara(string laTexString, IOfficeMathParagraph mathPara, DocumentLaTeXConverter documentLaTeXConverter)
	{
		m_laTexString = laTexString;
		m_mathPara = mathPara;
		m_documentLaTeXConverter = documentLaTeXConverter;
		ParseMathEquation();
		Close();
	}

	private void ParseMathEquation()
	{
		while (m_laTexStringPosition < m_laTexString.Length || m_isToUseExistingCurrElement)
		{
			if (m_isToUseExistingCurrElement)
			{
				m_isToUseExistingCurrElement = false;
			}
			else
			{
				m_currentLaTexElement = GetLaTexElement();
			}
			switch (m_currentLaTexElement)
			{
			case "\\dot":
			case "\\hat":
			case "\\bar":
			case "\\vec":
			case "\\ddot":
			case "\\hvec":
			case "\\dddot":
			case "\\check":
			case "\\acute":
			case "\\grave":
			case "\\breve":
			case "\\widehat":
			case "\\widetilde":
				ParseMathAccent();
				break;
			case "\\underline":
			case "\\overline":
				ParseMathBar();
				break;
			case "\\box":
				ParseMathBox();
				break;
			case "\\fbox":
			case "\\boxed":
				ParseMathBorderBox();
				break;
			case "\\left":
				ParseMathDelimiter();
				break;
			case "\\eqarray":
				ParseEquationArray();
				break;
			case "\\frac":
			case "\\sfrac":
				ParseMathFraction();
				break;
			case "\\sin":
			case "\\sec":
			case "\\cos":
			case "\\cot":
			case "\\csc":
			case "\\tan":
			case "\\log":
			case "\\lim":
			case "\\min":
			case "\\max":
			case "\\sinh":
			case "\\sech":
			case "\\cosh":
			case "\\coth":
			case "\\csch":
			case "\\tanh":
			case "\\arcsin":
			case "\\arccos":
			case "\\arctan":
			case "\\ln":
				ParseMathFunction();
				break;
			case "\\overbrace":
			case "\\underbrace":
				ParseMathGroupCharacter();
				break;
			case "\\begin":
				ParseMathMatrix();
				break;
			case "\\sum":
			case "\\int":
			case "\\iint":
			case "\\oint":
			case "\\prod":
			case "\\coint":
			case "\\aoint":
			case "\\iiint":
			case "\\oiint":
			case "\\cwint":
			case "\\amalg":
			case "\\bigodot":
			case "\\bigudot":
			case "\\bigotimes":
			case "\\biguplus":
			case "\\bigwedge":
			case "\\bigoplus":
			case "\\bigcap":
			case "\\oiiint":
			case "\\bigcup":
			case "\\bigvee":
				ParseMathNArray();
				break;
			case "\\sqrt":
				ParseMathRadical();
				break;
			case "{":
				ParseMathGroupStart();
				break;
			case "\\mathbit":
				ParseMathStyleType(MathStyleType.BoldItalic);
				break;
			case "\\mathbf":
				ParseMathStyleType(MathStyleType.Bold);
				break;
			case "\\mathbb":
				ParseMathFontType(MathFontType.DoubleStruck);
				break;
			case "\\mathfrak":
				ParseMathFontType(MathFontType.Fraktur);
				break;
			case "\\mathtt":
				ParseMathFontType(MathFontType.Monospace);
				break;
			case "\\mathsf":
				ParseMathFontType(MathFontType.SansSerif);
				break;
			case "\\mathcal":
			case "\\mathscr":
				ParseMathFontType(MathFontType.Script);
				break;
			case "\\mathrm":
				ParseMathHasNormalText();
				break;
			default:
				ParseDefaultLaTeXElement();
				break;
			}
		}
	}

	private void ParseMathAccent()
	{
		int laTexStringPosition = m_laTexStringPosition;
		string currentLaTexElement = m_currentLaTexElement;
		string text = GetEquationString();
		if (text == " " && m_currentLaTexElement == " ")
		{
			text = GetEquationString();
		}
		if (text != null || (text == null && m_currentLaTexElement == null))
		{
			m_currOfficeMath = ((m_currOfficeMath == null) ? (m_mathPara.Maths.Add(m_mathPara.Maths.Count) as OfficeMath) : m_currOfficeMath);
			OfficeMathAccent officeMathAccent = m_currOfficeMath.Functions.Add(m_currOfficeMath.Functions.Count, MathFunctionType.Accent) as OfficeMathAccent;
			switch (currentLaTexElement)
			{
			case "\\dot":
				officeMathAccent.AccentCharacter = '\u0307'.ToString();
				break;
			case "\\ddot":
				officeMathAccent.AccentCharacter = '\u0308'.ToString();
				break;
			case "\\dddot":
				officeMathAccent.AccentCharacter = '\u20db'.ToString();
				break;
			case "\\hat":
			case "\\widehat":
				officeMathAccent.AccentCharacter = '\u0302'.ToString();
				break;
			case "\\check":
				officeMathAccent.AccentCharacter = '\u030c'.ToString();
				break;
			case "\\acute":
				officeMathAccent.AccentCharacter = '\u0301'.ToString();
				break;
			case "\\grave":
				officeMathAccent.AccentCharacter = '\u0300'.ToString();
				break;
			case "\\breve":
				officeMathAccent.AccentCharacter = '\u0306'.ToString();
				break;
			case "\\widetilde":
				officeMathAccent.AccentCharacter = '\u0303'.ToString();
				break;
			case "\\bar":
				officeMathAccent.AccentCharacter = '\u0305'.ToString();
				break;
			case "\\vec":
				officeMathAccent.AccentCharacter = '\u20d7'.ToString();
				break;
			case "\\hvec":
				officeMathAccent.AccentCharacter = '\u20d1'.ToString();
				break;
			}
			if (!string.IsNullOrEmpty(text))
			{
				if (text.StartsWith("\\bar{") && text.EndsWith("}") && officeMathAccent.AccentCharacter == '\u0305'.ToString())
				{
					officeMathAccent.AccentCharacter = '\u033f'.ToString();
					text = text.Substring(5, text.Length - 6);
				}
				ResetAndParseMath(text, officeMathAccent.Equation as OfficeMath);
			}
		}
		else
		{
			int num = laTexStringPosition - currentLaTexElement.Length;
			string runElement = m_laTexString.Substring(num, m_laTexStringPosition - num);
			SetRunElement(runElement);
		}
	}

	private void ParseMathBar()
	{
		int laTexStringPosition = m_laTexStringPosition;
		string currentLaTexElement = m_currentLaTexElement;
		string equationString = GetEquationString();
		if (equationString == " " && m_currentLaTexElement == " ")
		{
			equationString = GetEquationString();
		}
		if (equationString != null || (equationString == null && m_currentLaTexElement == null))
		{
			m_currentLaTexElement = GetLaTexElement();
			if (m_currentLaTexElement == "^" || m_currentLaTexElement == "_")
			{
				ParseMathSubSuperScript(currentLaTexElement + "{" + equationString + "}", laTexStringPosition);
				return;
			}
			if (m_currentLaTexElement != null)
			{
				m_isToUseExistingCurrElement = true;
			}
			m_currOfficeMath = ((m_currOfficeMath == null) ? (m_mathPara.Maths.Add(m_mathPara.Maths.Count) as OfficeMath) : m_currOfficeMath);
			OfficeMathBar officeMathBar = m_currOfficeMath.Functions.Add(m_currOfficeMath.Functions.Count, MathFunctionType.Bar) as OfficeMathBar;
			if (!(currentLaTexElement == "\\overline"))
			{
				if (currentLaTexElement == "\\underline")
				{
					officeMathBar.BarTop = false;
				}
			}
			else
			{
				officeMathBar.BarTop = true;
			}
			if (!string.IsNullOrEmpty(equationString))
			{
				ResetAndParseMath(equationString, officeMathBar.Equation as OfficeMath);
			}
		}
		else
		{
			int num = laTexStringPosition - currentLaTexElement.Length;
			string runElement = m_laTexString.Substring(num, m_laTexStringPosition - num);
			SetRunElement(runElement);
		}
	}

	private void ParseMathBox()
	{
		int laTexStringPosition = m_laTexStringPosition;
		string currentLaTexElement = m_currentLaTexElement;
		string equationString = GetEquationString();
		if (equationString == " " && m_currentLaTexElement == " ")
		{
			equationString = GetEquationString();
		}
		if (equationString != null || (equationString == null && m_currentLaTexElement == null))
		{
			m_currOfficeMath = ((m_currOfficeMath == null) ? (m_mathPara.Maths.Add(m_mathPara.Maths.Count) as OfficeMath) : m_currOfficeMath);
			OfficeMathBox officeMathBox = m_currOfficeMath.Functions.Add(m_currOfficeMath.Functions.Count, MathFunctionType.Box) as OfficeMathBox;
			officeMathBox.NoBreak = false;
			if (!string.IsNullOrEmpty(equationString))
			{
				ResetAndParseMath(equationString, officeMathBox.Equation as OfficeMath);
			}
		}
		else
		{
			int num = laTexStringPosition - currentLaTexElement.Length;
			string runElement = m_laTexString.Substring(num, m_laTexStringPosition - num);
			SetRunElement(runElement);
		}
	}

	private void ParseMathBorderBox()
	{
		int laTexStringPosition = m_laTexStringPosition;
		string currentLaTexElement = m_currentLaTexElement;
		string equationString = GetEquationString();
		if (equationString == " " && m_currentLaTexElement == " ")
		{
			equationString = GetEquationString();
		}
		if (equationString != null || (equationString == null && m_currentLaTexElement == null))
		{
			m_currentLaTexElement = GetLaTexElement();
			if (m_currentLaTexElement == "^" || m_currentLaTexElement == "_")
			{
				ParseMathSubSuperScript(currentLaTexElement + "{" + equationString + "}", laTexStringPosition);
				return;
			}
			if (m_currentLaTexElement != null)
			{
				m_isToUseExistingCurrElement = true;
			}
			m_currOfficeMath = ((m_currOfficeMath == null) ? (m_mathPara.Maths.Add(m_mathPara.Maths.Count) as OfficeMath) : m_currOfficeMath);
			OfficeMathBorderBox officeMathBorderBox = m_currOfficeMath.Functions.Add(m_currOfficeMath.Functions.Count, MathFunctionType.BorderBox) as OfficeMathBorderBox;
			if (!string.IsNullOrEmpty(equationString))
			{
				ResetAndParseMath(equationString, officeMathBorderBox.Equation as OfficeMath);
			}
		}
		else
		{
			int num = laTexStringPosition - currentLaTexElement.Length;
			string runElement = m_laTexString.Substring(num, m_laTexStringPosition - num);
			SetRunElement(runElement);
		}
	}

	private void ParseMathDelimiter()
	{
		int laTexStringPosition = m_laTexStringPosition;
		string currentLaTexElement = m_currentLaTexElement;
		string beginChar = null;
		string endChar = null;
		string sepChar = null;
		List<string> inBetweenStrings = new List<string>();
		string equationString = string.Empty;
		if (CheckDelimiterSyntax(ref beginChar, ref endChar, ref sepChar, ref inBetweenStrings, ref equationString))
		{
			m_currentLaTexElement = GetLaTexElement();
			if (!(m_currentLaTexElement == "^") && !(m_currentLaTexElement == "_"))
			{
				if (m_currentLaTexElement != null)
				{
					m_isToUseExistingCurrElement = true;
				}
				m_currOfficeMath = ((m_currOfficeMath == null) ? (m_mathPara.Maths.Add(m_mathPara.Maths.Count) as OfficeMath) : m_currOfficeMath);
				OfficeMathDelimiter officeMathDelimiter = m_currOfficeMath.Functions.Add(m_currOfficeMath.Functions.Count, MathFunctionType.Delimiter) as OfficeMathDelimiter;
				officeMathDelimiter.BeginCharacter = beginChar;
				officeMathDelimiter.EndCharacter = endChar;
				officeMathDelimiter.Seperator = sepChar;
				{
					foreach (string item in inBetweenStrings)
					{
						ResetAndParseMath(item, officeMathDelimiter.Equation.Add(officeMathDelimiter.Equation.Count) as OfficeMath);
					}
					return;
				}
			}
			ParseMathSubSuperScript(equationString, laTexStringPosition);
		}
		else
		{
			int num = laTexStringPosition - currentLaTexElement.Length;
			string runElement = m_laTexString.Substring(num, m_laTexStringPosition - num).Replace("\\ ", " ");
			SetRunElement(runElement);
		}
	}

	private bool CheckDelimiterSyntax(ref string beginChar, ref string endChar, ref string sepChar, ref List<string> inBetweenStrings, ref string equationString)
	{
		Stack<string> stack = new Stack<string>();
		stack.Push(m_currentLaTexElement);
		equationString += m_currentLaTexElement;
		beginChar = GetDelimiterString(ref equationString);
		if (beginChar == null)
		{
			return false;
		}
		string text = string.Empty;
		while (stack.Count > 0 && m_laTexStringPosition < m_laTexString.Length)
		{
			string laTexElement = GetLaTexElement();
			equationString += laTexElement;
			switch (laTexElement)
			{
			case "\\left":
				text += laTexElement;
				stack.Push(laTexElement);
				break;
			case "\\right":
				if (stack.Count == 1)
				{
					inBetweenStrings.Add(text.Trim());
					endChar = GetDelimiterString(ref equationString);
					if (endChar == null)
					{
						return false;
					}
				}
				else
				{
					text += laTexElement;
				}
				stack.Pop();
				break;
			case "\\middle":
				if (stack.Count == 1)
				{
					string delimiterString = GetDelimiterString(ref equationString);
					if (delimiterString == null)
					{
						return false;
					}
					if (sepChar == null)
					{
						sepChar = delimiterString;
					}
					inBetweenStrings.Add(text.Trim());
					text = string.Empty;
				}
				else
				{
					text += laTexElement;
				}
				break;
			default:
				text += laTexElement;
				break;
			}
		}
		return stack.Count == 0;
	}

	private string GetDelimiterString(ref string equationString)
	{
		string result = string.Empty;
		m_currentLaTexElement = GetLaTexElement();
		equationString += m_currentLaTexElement;
		if (m_currentLaTexElement == "\\")
		{
			m_currentLaTexElement = GetLaTexElement();
			equationString += m_currentLaTexElement;
		}
		if (new List<string>
		{
			"\\lfloor",
			"\\rfloor",
			"\\lceil",
			"\\rceil",
			"\\langle",
			"\\rangle",
			'.'.ToString(),
			'|'.ToString(),
			'('.ToString(),
			')'.ToString(),
			'['.ToString(),
			']'.ToString(),
			'{'.ToString(),
			'}'.ToString()
		}.Contains(m_currentLaTexElement))
		{
			switch (m_currentLaTexElement)
			{
			case "\\lfloor":
				result = '⌊'.ToString();
				break;
			case "\\rfloor":
				result = '⌋'.ToString();
				break;
			case "\\lceil":
				result = '⌈'.ToString();
				break;
			case "\\rceil":
				result = '⌉'.ToString();
				break;
			case "\\langle":
				result = '〈'.ToString();
				break;
			case "\\rangle":
				result = '〉'.ToString();
				break;
			default:
				if (m_currentLaTexElement != '.'.ToString())
				{
					result = m_currentLaTexElement;
				}
				break;
			}
			return result;
		}
		return null;
	}

	private void ParseEquationArray()
	{
		int laTexStringPosition = m_laTexStringPosition;
		string currentLaTexElement = m_currentLaTexElement;
		string equationString = GetEquationString();
		if (equationString != null || (equationString == null && m_currentLaTexElement == null))
		{
			List<string> list = SplitEquationArray(equationString);
			m_currOfficeMath = ((m_currOfficeMath == null) ? (m_mathPara.Maths.Add(m_mathPara.Maths.Count) as OfficeMath) : m_currOfficeMath);
			OfficeMathEquationArray officeMathEquationArray = m_currOfficeMath.Functions.Add(m_currOfficeMath.Functions.Count, MathFunctionType.EquationArray) as OfficeMathEquationArray;
			if (list.Count != 0)
			{
				foreach (string item in list)
				{
					ResetAndParseMath(item, officeMathEquationArray.Equation.Add(officeMathEquationArray.Equation.Count) as OfficeMath);
				}
				return;
			}
			officeMathEquationArray.Equation.Add(officeMathEquationArray.Equation.Count);
		}
		else
		{
			int num = laTexStringPosition - currentLaTexElement.Length;
			string runElement = m_laTexString.Substring(num, m_laTexStringPosition - num);
			SetRunElement(runElement);
		}
	}

	private List<string> SplitEquationArray(string equationString)
	{
		List<string> list = new List<string>();
		string text = string.Empty;
		if (equationString != null)
		{
			for (int i = 0; i < equationString.Length; i++)
			{
				if (equationString[i] == '@')
				{
					if (equationString.Substring(i).Contains("&") || equationString.Substring(i + 1).Contains("@"))
					{
						list.Add(text.Trim());
						text = string.Empty;
					}
				}
				else
				{
					text += equationString[i];
				}
			}
			list.Add(text.Trim());
		}
		return list;
	}

	private void ParseMathFraction()
	{
		int laTexStringPosition = m_laTexStringPosition;
		string currentLaTexElement = m_currentLaTexElement;
		string text = GetEquationString();
		if (m_currentLaTexElement == " " && text == " ")
		{
			text = GetEquationString();
		}
		string text2 = string.Empty;
		if ((m_currentLaTexElement == " " && text == " ") || (m_currentLaTexElement == null && text == null))
		{
			text = string.Empty;
		}
		else
		{
			text2 = GetEquationString();
		}
		if (text != null && text2 != null)
		{
			m_currentLaTexElement = GetLaTexElement();
			if (m_currentLaTexElement == "^" || m_currentLaTexElement == "_")
			{
				ParseMathSubSuperScript(currentLaTexElement + "{" + text + "}{" + text2 + "}", laTexStringPosition);
				return;
			}
			if (m_currentLaTexElement != null)
			{
				m_isToUseExistingCurrElement = true;
			}
			m_currOfficeMath = ((m_currOfficeMath == null) ? (m_mathPara.Maths.Add(m_mathPara.Maths.Count) as OfficeMath) : m_currOfficeMath);
			OfficeMathFraction officeMathFraction = m_currOfficeMath.Functions.Add(m_currOfficeMath.Functions.Count, MathFunctionType.Fraction) as OfficeMathFraction;
			if (!(currentLaTexElement == "\\frac"))
			{
				if (currentLaTexElement == "\\sfrac")
				{
					officeMathFraction.FractionType = MathFractionType.SkewedFractionBar;
				}
			}
			else
			{
				officeMathFraction.FractionType = MathFractionType.NormalFractionBar;
			}
			if (text != string.Empty)
			{
				ResetAndParseMath(text, officeMathFraction.Numerator as OfficeMath);
			}
			if (text2 != string.Empty)
			{
				ResetAndParseMath(text2, officeMathFraction.Denominator as OfficeMath);
			}
		}
		else
		{
			int num = laTexStringPosition - currentLaTexElement.Length;
			string runElement = m_laTexString.Substring(num, m_laTexStringPosition - num);
			SetRunElement(runElement);
		}
	}

	private void ParseMathNoFractionBar(string numerator, int startLaTexPos)
	{
		if (m_currentLaTexElement != "\\atop")
		{
			return;
		}
		string text = GetEquationString();
		if (m_currentLaTexElement == text)
		{
			text = null;
		}
		if (numerator != null && text != null)
		{
			m_currOfficeMath = ((m_currOfficeMath == null) ? (m_mathPara.Maths.Add(m_mathPara.Maths.Count) as OfficeMath) : m_currOfficeMath);
			OfficeMathFraction officeMathFraction = m_currOfficeMath.Functions.Add(m_currOfficeMath.Functions.Count, MathFunctionType.Fraction) as OfficeMathFraction;
			officeMathFraction.FractionType = MathFractionType.NoFractionBar;
			if (numerator != string.Empty)
			{
				ResetAndParseMath(numerator, officeMathFraction.Numerator as OfficeMath);
			}
			if (text != string.Empty)
			{
				ResetAndParseMath(text, officeMathFraction.Denominator as OfficeMath);
			}
		}
		else
		{
			string runElement = m_laTexString.Substring(startLaTexPos, m_laTexStringPosition - startLaTexPos);
			SetRunElement(runElement);
		}
	}

	private void ParseMathFunction()
	{
		int laTexStringPosition = m_laTexStringPosition;
		string currentLaTexElement = m_currentLaTexElement;
		string text = GetEquationString();
		bool flag = false;
		string text2 = null;
		MathScriptType scriptType = MathScriptType.Superscript;
		bool flag2 = false;
		string text3 = null;
		if (m_currentLaTexElement == "^" || m_currentLaTexElement == "_")
		{
			flag = true;
			scriptType = ((!(m_currentLaTexElement == "^")) ? MathScriptType.Subscript : MathScriptType.Superscript);
			text2 = GetEquationString();
			if (text2 == m_currentLaTexElement)
			{
				text = GetEquationString();
				if (text != null && text == m_currentLaTexElement)
				{
					text = string.Empty;
					m_isToUseExistingCurrElement = true;
				}
			}
			else
			{
				text = GetEquationString();
			}
		}
		else if (m_currentLaTexElement == "\\below")
		{
			flag2 = true;
			text3 = GetEquationString();
			if (text3 == m_currentLaTexElement)
			{
				text = GetEquationString();
				if (text != null && text == m_currentLaTexElement)
				{
					text = string.Empty;
					m_isToUseExistingCurrElement = true;
				}
			}
			else
			{
				text = GetEquationString();
			}
		}
		if (text != null)
		{
			m_currOfficeMath = ((m_currOfficeMath == null) ? (m_mathPara.Maths.Add(m_mathPara.Maths.Count) as OfficeMath) : m_currOfficeMath);
			OfficeMathFunction officeMathFunction = m_currOfficeMath.Functions.Add(m_currOfficeMath.Functions.Count, MathFunctionType.Function) as OfficeMathFunction;
			if (flag)
			{
				OfficeMathScript officeMathScript = officeMathFunction.FunctionName.Functions.Add(0, MathFunctionType.SubSuperscript) as OfficeMathScript;
				officeMathScript.ScriptType = scriptType;
				if (text2 != null)
				{
					ResetAndParseMath(text2, officeMathScript.Script as OfficeMath);
				}
				ResetAndParseMath(currentLaTexElement.Trim('\\'), officeMathScript.Equation as OfficeMath);
				(officeMathScript.Equation.Functions[0] as OfficeMathRunElement).MathFormat.Style = MathStyleType.Regular;
			}
			else if (flag2)
			{
				OfficeMathLimit officeMathLimit = officeMathFunction.FunctionName.Functions.Add(0, MathFunctionType.Limit) as OfficeMathLimit;
				if (text3 != null)
				{
					ResetAndParseMath(text3, officeMathLimit.Limit as OfficeMath);
				}
				ResetAndParseMath(currentLaTexElement.Trim('\\'), officeMathLimit.Equation as OfficeMath);
				(officeMathLimit.Equation.Functions[0] as OfficeMathRunElement).MathFormat.Style = MathStyleType.Regular;
			}
			else
			{
				OfficeMath currOfficeMath = m_currOfficeMath;
				m_currOfficeMath = officeMathFunction.FunctionName as OfficeMath;
				SetRunElement(currentLaTexElement.Trim('\\'));
				(officeMathFunction.FunctionName.Functions[0] as OfficeMathRunElement).MathFormat.Style = MathStyleType.Regular;
				m_currOfficeMath = currOfficeMath;
			}
			if (!string.IsNullOrEmpty(text))
			{
				ResetAndParseMath(text, officeMathFunction.Equation as OfficeMath);
			}
		}
		else
		{
			int num = laTexStringPosition - currentLaTexElement.Length;
			string runElement = m_laTexString.Substring(num, m_laTexStringPosition - num);
			SetRunElement(runElement);
		}
	}

	private void ParseMathGroupCharacter()
	{
		int laTexStringPosition = m_laTexStringPosition;
		string currentLaTexElement = m_currentLaTexElement;
		string equationString = GetEquationString();
		if (equationString == " " && m_currentLaTexElement == " ")
		{
			equationString = GetEquationString();
		}
		if (equationString != null || (equationString == null && m_currentLaTexElement == null))
		{
			m_currOfficeMath = ((m_currOfficeMath == null) ? (m_mathPara.Maths.Add(m_mathPara.Maths.Count) as OfficeMath) : m_currOfficeMath);
			OfficeMathGroupCharacter officeMathGroupCharacter = m_currOfficeMath.Functions.Add(m_currOfficeMath.Functions.Count, MathFunctionType.GroupCharacter) as OfficeMathGroupCharacter;
			if (currentLaTexElement == "\\overbrace")
			{
				officeMathGroupCharacter.GroupCharacter = '⏞'.ToString();
				officeMathGroupCharacter.HasAlignTop = false;
				officeMathGroupCharacter.HasCharacterTop = true;
			}
			if (!string.IsNullOrEmpty(equationString))
			{
				ResetAndParseMath(equationString, officeMathGroupCharacter.Equation as OfficeMath);
			}
		}
		else
		{
			int num = laTexStringPosition - currentLaTexElement.Length;
			string runElement = m_laTexString.Substring(num, m_laTexStringPosition - num);
			SetRunElement(runElement);
		}
	}

	private void ParseMathLimit(string equationText, int startLaTexPos)
	{
		if (m_currentLaTexElement != "\\below")
		{
			return;
		}
		string equationString = GetEquationString();
		if (equationString == " " && m_currentLaTexElement == " ")
		{
			equationString = GetEquationString();
		}
		if (equationString != null)
		{
			m_currOfficeMath = ((m_currOfficeMath == null) ? (m_mathPara.Maths.Add(m_mathPara.Maths.Count) as OfficeMath) : m_currOfficeMath);
			OfficeMathLimit officeMathLimit = m_currOfficeMath.Functions.Add(m_currOfficeMath.Functions.Count, MathFunctionType.Limit) as OfficeMathLimit;
			if (equationText != null)
			{
				ResetAndParseMath(equationText, officeMathLimit.Equation as OfficeMath);
			}
			if (equationString != " ")
			{
				ResetAndParseMath(equationString, officeMathLimit.Limit as OfficeMath);
			}
		}
		else
		{
			string runElement = m_laTexString.Substring(startLaTexPos, m_laTexStringPosition - startLaTexPos);
			SetRunElement(runElement);
		}
	}

	private void ParseMathMatrix()
	{
		int laTexStringPosition = m_laTexStringPosition;
		string currentLaTexElement = m_currentLaTexElement;
		List<List<string>> matrixRows = null;
		int maxColCount = 0;
		m_currentLaTexElement = GetLaTexElement();
		if (m_currentLaTexElement == "{")
		{
			string inBetweenText = string.Empty;
			if (IsValidGroup(ref inBetweenText) && inBetweenText == "matrix" && CheckMatrixSyntax(ref matrixRows, ref maxColCount))
			{
				m_currentLaTexElement = GetLaTexElement();
				if (m_currentLaTexElement == "^" || m_currentLaTexElement == "_")
				{
					int num = laTexStringPosition - currentLaTexElement.Length;
					string equationString = m_laTexString.Substring(num, m_laTexStringPosition - 1 - num);
					ParseMathSubSuperScript(equationString, laTexStringPosition);
					return;
				}
				if (m_currentLaTexElement != null)
				{
					m_isToUseExistingCurrElement = true;
				}
				OfficeMath officeMath = ((m_currOfficeMath == null) ? (m_mathPara.Maths.Add(m_mathPara.Maths.Count) as OfficeMath) : m_currOfficeMath);
				OfficeMathMatrix officeMathMatrix = officeMath.Functions.Add(officeMath.Functions.Count, MathFunctionType.Matrix) as OfficeMathMatrix;
				for (int i = 0; i < matrixRows.Count; i++)
				{
					List<string> list = matrixRows[i];
					officeMathMatrix.Rows.Add(officeMathMatrix.Rows.Count);
					if (i == 0)
					{
						for (int j = 0; j < maxColCount; j++)
						{
							officeMathMatrix.Columns.Add(officeMathMatrix.Columns.Count);
						}
					}
					for (int k = 0; k < list.Count; k++)
					{
						string currString = list[k];
						ResetAndParseMath(currString, officeMathMatrix.Rows[i].Arguments[k] as OfficeMath);
					}
				}
			}
			else
			{
				int num2 = laTexStringPosition - currentLaTexElement.Length;
				string runElement = m_laTexString.Substring(num2, m_laTexStringPosition - num2);
				SetRunElement(runElement);
			}
		}
		else
		{
			SetRunElement(currentLaTexElement);
			m_isToUseExistingCurrElement = true;
		}
	}

	private bool CheckMatrixSyntax(ref List<List<string>> matrixRows, ref int maxColCount)
	{
		Stack<string> stack = new Stack<string>();
		stack.Push("\\begin{matrix}");
		matrixRows = new List<List<string>>();
		string text = string.Empty;
		List<string> list = new List<string>();
		while (stack.Count > 0 && m_laTexStringPosition < m_laTexString.Length)
		{
			m_currentLaTexElement = GetLaTexElement();
			while (m_currentLaTexElement == " ")
			{
				m_currentLaTexElement = GetLaTexElement();
			}
			if (m_currentLaTexElement == "\\")
			{
				m_currentLaTexElement = GetLaTexElement();
				if (m_currentLaTexElement == "\\")
				{
					m_currentLaTexElement += "\\";
				}
			}
			switch (m_currentLaTexElement)
			{
			case "\\begin":
			{
				text += m_currentLaTexElement;
				string text2 = (m_currentLaTexElement = GetLaTexElement());
				string inBetweenText = string.Empty;
				if (IsValidGroup(ref inBetweenText) && inBetweenText == "matrix")
				{
					stack.Push("\\begin{matrix}");
				}
				text = text + text2 + inBetweenText + m_currentLaTexElement;
				break;
			}
			case "\\end":
			{
				string text2 = m_currentLaTexElement;
				m_currentLaTexElement = GetLaTexElement();
				text2 += m_currentLaTexElement;
				string inBetweenText2 = string.Empty;
				if (IsValidGroup(ref inBetweenText2) && inBetweenText2 == "matrix" && stack.Count == 1)
				{
					if (list.Count != 0 || text != string.Empty)
					{
						list.Add(text);
						text = string.Empty;
						if (list.Count > maxColCount)
						{
							maxColCount = list.Count;
						}
						matrixRows.Add(list);
						list = new List<string>();
					}
					stack.Pop();
				}
				else
				{
					if (stack.Count == 1)
					{
						return false;
					}
					text = text + text2 + inBetweenText2 + m_currentLaTexElement;
				}
				break;
			}
			case "&":
				if (stack.Count == 1)
				{
					list.Add(text);
					text = string.Empty;
				}
				else
				{
					text += m_currentLaTexElement;
				}
				break;
			case "\\\\":
				if (stack.Count == 1)
				{
					list.Add(text);
					text = string.Empty;
					if (list.Count > maxColCount)
					{
						maxColCount = list.Count;
					}
					matrixRows.Add(list);
					list = new List<string>();
				}
				else
				{
					text += m_currentLaTexElement;
				}
				break;
			default:
				text += m_currentLaTexElement;
				break;
			}
		}
		return stack.Count == 0;
	}

	private void ParseMathNArray()
	{
		int laTexStringPosition = m_laTexStringPosition;
		string currentLaTexElement = m_currentLaTexElement;
		string text = GetEquationString();
		string text2 = null;
		string text3 = null;
		for (int i = 0; i < 2; i++)
		{
			if (!(text == " "))
			{
				break;
			}
			if (!(m_currentLaTexElement == " "))
			{
				break;
			}
			text = GetEquationString();
		}
		if (m_currentLaTexElement == "_")
		{
			text2 = GetEquationString();
		}
		else if (m_currentLaTexElement == "^")
		{
			text3 = GetEquationString();
		}
		if (text2 != null)
		{
			text = GetEquationString();
			if (m_currentLaTexElement == "^")
			{
				text3 = GetEquationString();
			}
		}
		else if (text3 != null)
		{
			text = GetEquationString();
			if (m_currentLaTexElement == "_")
			{
				text2 = GetEquationString();
			}
		}
		if (text2 != null && text3 != null)
		{
			text = GetEquationString();
		}
		if (text != null)
		{
			bool flag = true;
			bool flag2 = false;
			if (m_currentLaTexElement == "}" && ((text.StartsWith("_") && text.Contains("^")) || (text.StartsWith("^") && text.Contains("_"))))
			{
				text = "{" + text + "}";
				flag2 = true;
			}
			string equationString = GetEquationString();
			if (equationString != null && flag2)
			{
				text = text + "{" + equationString + "}";
			}
			else if (equationString == null && (m_currentLaTexElement == "^" || m_currentLaTexElement == "_"))
			{
				text += m_currentLaTexElement;
				string equationString2 = GetEquationString();
				text = text + "{" + equationString2 + "}";
			}
			else if (equationString != null && ((equationString.StartsWith("_") && equationString.Contains("^")) || (equationString.StartsWith("^") && equationString.Contains("_"))))
			{
				text = text + "{" + equationString + "}";
			}
			else
			{
				m_isToUseExistingCurrElement = true;
				flag = false;
				if (equationString != null && m_currentLaTexElement != equationString)
				{
					m_currentLaTexElement = equationString;
				}
			}
			while (flag)
			{
				equationString = GetEquationString();
				if (equationString == null && (m_currentLaTexElement == "^" || m_currentLaTexElement == "_"))
				{
					text += m_currentLaTexElement;
					string equationString2 = GetEquationString();
					text = text + "{" + equationString2 + "}";
				}
				else if (equationString != null && ((equationString.StartsWith("_") && equationString.Contains("^")) || (equationString.StartsWith("^") && equationString.Contains("_"))))
				{
					text = text + "{" + equationString + "}";
				}
				else
				{
					m_isToUseExistingCurrElement = true;
					flag = false;
				}
			}
		}
		if (text != null || (text == null && m_currentLaTexElement == null))
		{
			m_currOfficeMath = ((m_currOfficeMath == null) ? (m_mathPara.Maths.Add(m_mathPara.Maths.Count) as OfficeMath) : m_currOfficeMath);
			OfficeMathNArray officeMathNArray = m_currOfficeMath.Functions.Add(m_currOfficeMath.Functions.Count, MathFunctionType.NArray) as OfficeMathNArray;
			officeMathNArray.NArrayCharacter = GetBasicNaryOperatorCharString(currentLaTexElement);
			if (text2 == null)
			{
				officeMathNArray.HideLowerLimit = true;
			}
			else
			{
				ResetAndParseMath(text2, officeMathNArray.Subscript as OfficeMath);
			}
			if (text3 == null)
			{
				officeMathNArray.HideUpperLimit = true;
			}
			else
			{
				ResetAndParseMath(text3, officeMathNArray.Superscript as OfficeMath);
			}
			if (text2 == null && text3 == null)
			{
				officeMathNArray.SubSuperscriptLimit = false;
			}
			if (text != " ")
			{
				ResetAndParseMath(text, officeMathNArray.Equation as OfficeMath);
			}
		}
		else
		{
			int num = laTexStringPosition - currentLaTexElement.Length;
			string runElement = m_laTexString.Substring(num, m_laTexStringPosition - num);
			SetRunElement(runElement);
		}
	}

	private void ParseMathRadical()
	{
		int laTexStringPosition = m_laTexStringPosition;
		string currentLaTexElement = m_currentLaTexElement;
		string degree = null;
		string equation = null;
		if (!CheckRadicalSyntax(ref degree, ref equation))
		{
			int num = laTexStringPosition - currentLaTexElement.Length;
			string runElement = m_laTexString.Substring(num, m_laTexStringPosition - num);
			SetRunElement(runElement);
			return;
		}
		m_currentLaTexElement = GetLaTexElement();
		if (m_currentLaTexElement == "^" || m_currentLaTexElement == "_")
		{
			int num2 = laTexStringPosition - currentLaTexElement.Length;
			string equationString = m_laTexString.Substring(num2, m_laTexStringPosition - 1 - num2);
			ParseMathSubSuperScript(equationString, laTexStringPosition);
			return;
		}
		if (m_currentLaTexElement != null)
		{
			m_isToUseExistingCurrElement = true;
		}
		m_currOfficeMath = ((m_currOfficeMath == null) ? (m_mathPara.Maths.Add(m_mathPara.Maths.Count) as OfficeMath) : m_currOfficeMath);
		OfficeMathRadical officeMathRadical = m_currOfficeMath.Functions.Add(m_currOfficeMath.Functions.Count, MathFunctionType.Radical) as OfficeMathRadical;
		if (degree != null && equation != null)
		{
			ResetAndParseMath(degree, officeMathRadical.Degree as OfficeMath);
		}
		else
		{
			officeMathRadical.HideDegree = true;
		}
		if (equation != null && equation != " ")
		{
			ResetAndParseMath(equation, officeMathRadical.Equation as OfficeMath);
		}
		else if (equation == null)
		{
			m_isToUseExistingCurrElement = true;
			if (degree != null)
			{
				SetRunElement("[" + degree + "]");
			}
		}
	}

	private bool CheckRadicalSyntax(ref string degree, ref string equation)
	{
		m_currentLaTexElement = GetLaTexElement();
		if (m_currentLaTexElement == " ")
		{
			m_currentLaTexElement = GetLaTexElement();
		}
		degree = CheckSquareBracSyntax();
		if (degree != null)
		{
			m_currentLaTexElement = GetLaTexElement();
		}
		bool result = false;
		bool flag = false;
		if (m_currentLaTexElement == "{")
		{
			result = IsValidGroup(ref equation);
		}
		else
		{
			flag = true;
		}
		if (flag && m_currentLaTexElement != null && m_currentLaTexElement.Length == 1 && (char.IsLetterOrDigit(m_currentLaTexElement[0]) || m_currentLaTexElement == " "))
		{
			equation = m_currentLaTexElement;
		}
		if (!flag)
		{
			return result;
		}
		return flag;
	}

	private bool IsValidGroup(ref string inBetweenText)
	{
		if (m_currentLaTexElement == "{")
		{
			inBetweenText = string.Empty;
			Stack<string> stack = new Stack<string>();
			stack.Push(m_currentLaTexElement);
			_ = string.Empty;
			while (stack.Count > 0 && m_laTexStringPosition < m_laTexString.Length)
			{
				m_currentLaTexElement = GetLaTexElement();
				switch (m_currentLaTexElement)
				{
				case "{":
					stack.Push(m_currentLaTexElement);
					inBetweenText += m_currentLaTexElement;
					break;
				case "}":
					if (stack.Count != 1)
					{
						inBetweenText += m_currentLaTexElement;
					}
					stack.Pop();
					break;
				case "\\left":
				{
					string beginChar = null;
					string endChar = null;
					string sepChar = null;
					string equationString = string.Empty;
					List<string> inBetweenStrings = new List<string>();
					if (CheckDelimiterSyntax(ref beginChar, ref endChar, ref sepChar, ref inBetweenStrings, ref equationString))
					{
						inBetweenText += equationString;
						break;
					}
					inBetweenText += equationString;
					return false;
				}
				default:
					inBetweenText += m_currentLaTexElement;
					break;
				}
			}
			return stack.Count == 0;
		}
		return false;
	}

	private string CheckSquareBracSyntax()
	{
		if (m_currentLaTexElement == "[")
		{
			string text = string.Empty;
			Stack<string> stack = new Stack<string>();
			stack.Push(m_currentLaTexElement);
			while (stack.Count > 0 && m_laTexStringPosition < m_laTexString.Length)
			{
				m_currentLaTexElement = GetLaTexElement();
				string currentLaTexElement = m_currentLaTexElement;
				if (!(currentLaTexElement == "["))
				{
					if (currentLaTexElement == "]")
					{
						if (stack.Count != 1)
						{
							text += m_currentLaTexElement;
						}
						stack.Pop();
					}
					else
					{
						text += m_currentLaTexElement;
					}
				}
				else
				{
					stack.Push(m_currentLaTexElement);
					text += m_currentLaTexElement;
				}
			}
			return text;
		}
		return null;
	}

	private void ParseMathGroupStart()
	{
		int startLaTexPos = m_laTexStringPosition - "{".Length;
		string inBetweenText = string.Empty;
		if (IsValidGroup(ref inBetweenText))
		{
			if ((inBetweenText.StartsWith("_") && inBetweenText.Contains("^")) || (inBetweenText.StartsWith("^") && inBetweenText.Contains("_")))
			{
				ParseMathLeftSubSuperScript(inBetweenText, startLaTexPos);
				return;
			}
			m_currentLaTexElement = GetLaTexElement();
			if (m_currentLaTexElement == "^" || m_currentLaTexElement == "_")
			{
				ParseMathSubSuperScript(inBetweenText, startLaTexPos);
				return;
			}
			if (m_currentLaTexElement == "\\below")
			{
				ParseMathLimit(inBetweenText, startLaTexPos);
				return;
			}
			if (m_currentLaTexElement == "\\atop")
			{
				ParseMathNoFractionBar(inBetweenText, startLaTexPos);
				return;
			}
			ResetAndParseMath(inBetweenText, m_currOfficeMath);
			m_isToUseExistingCurrElement = true;
		}
		else
		{
			SetRunElement("{");
			ResetAndParseMath(inBetweenText, m_currOfficeMath);
		}
	}

	private void ParseMathSubSuperScript(string equationString, int startLaTexPos)
	{
		if (!(m_currentLaTexElement == "^") && !(m_currentLaTexElement == "_"))
		{
			return;
		}
		MathScriptType mathScriptType = ((!(m_currentLaTexElement == "^")) ? MathScriptType.Subscript : MathScriptType.Superscript);
		string equationString2 = GetEquationString();
		if (equationString2 != null || m_currentLaTexElement == null)
		{
			if (m_currentLaTexElement != null)
			{
				m_currentLaTexElement = GetLaTexElement();
				if ((mathScriptType == MathScriptType.Superscript && m_currentLaTexElement == "_") || (mathScriptType == MathScriptType.Subscript && m_currentLaTexElement == "^"))
				{
					ParseMathRightSubSuperScript(equationString, equationString2, mathScriptType, startLaTexPos);
					return;
				}
				m_isToUseExistingCurrElement = true;
			}
			m_currOfficeMath = ((m_currOfficeMath == null) ? (m_mathPara.Maths.Add(m_mathPara.Maths.Count) as OfficeMath) : m_currOfficeMath);
			OfficeMathScript officeMathScript = m_currOfficeMath.Functions.Add(m_currOfficeMath.Functions.Count, MathFunctionType.SubSuperscript) as OfficeMathScript;
			officeMathScript.ScriptType = mathScriptType;
			if (equationString != null || equationString != " ")
			{
				ResetAndParseMath(equationString, officeMathScript.Equation as OfficeMath);
			}
			if (!string.IsNullOrEmpty(equationString2))
			{
				ResetAndParseMath(equationString2, officeMathScript.Script as OfficeMath);
			}
		}
		else
		{
			string runElement = m_laTexString.Substring(startLaTexPos, m_laTexStringPosition - startLaTexPos);
			SetRunElement(runElement);
		}
	}

	private void ParseMathLeftSubSuperScript(string groupBetweenText, int startLaTexPos)
	{
		if ((!groupBetweenText.StartsWith("_") || !groupBetweenText.Contains("^")) && (!groupBetweenText.StartsWith("^") || !groupBetweenText.Contains("_")))
		{
			return;
		}
		string text = null;
		string text2 = null;
		string laTexString = m_laTexString;
		int laTexStringPosition = m_laTexStringPosition;
		string currentLaTexElement = m_currentLaTexElement;
		m_laTexString = groupBetweenText;
		m_laTexStringPosition = 1;
		if (groupBetweenText.StartsWith("_"))
		{
			text = GetEquationString();
		}
		else
		{
			text2 = GetEquationString();
		}
		if (text != null)
		{
			m_currentLaTexElement = GetLaTexElement();
			if (m_currentLaTexElement == "^")
			{
				text2 = GetEquationString();
			}
		}
		else if (text2 != null)
		{
			m_currentLaTexElement = GetLaTexElement();
			if (m_currentLaTexElement == "_")
			{
				text = GetEquationString();
			}
		}
		m_laTexString = laTexString;
		m_laTexStringPosition = laTexStringPosition;
		m_currentLaTexElement = currentLaTexElement;
		string text3 = null;
		if (text != null && text2 != null)
		{
			text3 = GetEquationString();
		}
		if (text3 != null || m_currentLaTexElement == null)
		{
			m_currOfficeMath = ((m_currOfficeMath == null) ? (m_mathPara.Maths.Add(m_mathPara.Maths.Count) as OfficeMath) : m_currOfficeMath);
			OfficeMathLeftScript officeMathLeftScript = m_currOfficeMath.Functions.Add(m_currOfficeMath.Functions.Count, MathFunctionType.LeftSubSuperscript) as OfficeMathLeftScript;
			if (text != " ")
			{
				ResetAndParseMath(text, officeMathLeftScript.Subscript as OfficeMath);
			}
			if (text2 != " ")
			{
				ResetAndParseMath(text2, officeMathLeftScript.Superscript as OfficeMath);
			}
			if (text3 != null && text3 != " ")
			{
				ResetAndParseMath(text3, officeMathLeftScript.Equation as OfficeMath);
			}
		}
		else
		{
			string runElement = m_laTexString.Substring(startLaTexPos, m_laTexStringPosition - startLaTexPos);
			SetRunElement(runElement);
		}
	}

	private void ParseMathRightSubSuperScript(string equationString, string scriptString, MathScriptType scriptType, int startLaTexPos)
	{
		string equationString2 = GetEquationString();
		if (equationString2 != null || m_currentLaTexElement == null)
		{
			m_currOfficeMath = ((m_currOfficeMath == null) ? (m_mathPara.Maths.Add(m_mathPara.Maths.Count) as OfficeMath) : m_currOfficeMath);
			OfficeMathRightScript officeMathRightScript = m_currOfficeMath.Functions.Add(m_currOfficeMath.Functions.Count, MathFunctionType.RightSubSuperscript) as OfficeMathRightScript;
			if (equationString != null && equationString != " ")
			{
				ResetAndParseMath(equationString, officeMathRightScript.Equation as OfficeMath);
			}
			if (scriptType == MathScriptType.Superscript)
			{
				if (scriptString != null && scriptString != " ")
				{
					ResetAndParseMath(scriptString, officeMathRightScript.Superscript as OfficeMath);
				}
				if (equationString2 != null && equationString2 != " ")
				{
					ResetAndParseMath(equationString2, officeMathRightScript.Subscript as OfficeMath);
				}
			}
			else
			{
				if (scriptString != null && scriptString != " ")
				{
					ResetAndParseMath(scriptString, officeMathRightScript.Subscript as OfficeMath);
				}
				if (equationString2 != null && equationString2 != " ")
				{
					ResetAndParseMath(equationString2, officeMathRightScript.Superscript as OfficeMath);
				}
			}
		}
		else
		{
			string runElement = m_laTexString.Substring(startLaTexPos, m_laTexStringPosition - startLaTexPos);
			SetRunElement(runElement);
		}
	}

	private void ParseDefaultLaTeXElement()
	{
		int laTexStringPosition = m_laTexStringPosition;
		string currentLaTexElement = m_currentLaTexElement;
		m_currentLaTexElement = GetLaTexElement();
		if (m_currentLaTexElement == "^" || m_currentLaTexElement == "_")
		{
			ParseMathSubSuperScript(currentLaTexElement, laTexStringPosition - currentLaTexElement.Length);
			return;
		}
		if (m_currentLaTexElement != null)
		{
			m_isToUseExistingCurrElement = true;
		}
		if (m_currentLaTexElement == " ")
		{
			if (currentLaTexElement == "\\")
			{
				return;
			}
			if (currentLaTexElement.StartsWith("\\") && GetSymbol(currentLaTexElement) != currentLaTexElement)
			{
				m_isToUseExistingCurrElement = false;
			}
		}
		SetRunElement(currentLaTexElement);
	}

	private void ParseMathFontType(MathFontType mathFontType)
	{
		int laTexStringPosition = m_laTexStringPosition;
		string currentLaTexElement = m_currentLaTexElement;
		m_currentLaTexElement = GetLaTexElement();
		if (m_currentLaTexElement == "{")
		{
			string inBetweenText = string.Empty;
			if (IsValidGroup(ref inBetweenText))
			{
				MathFontType currMathFontType = m_currMathFontType;
				if (m_currMathFontType == MathFontType.Roman)
				{
					m_currMathFontType = mathFontType;
				}
				ResetAndParseMath(inBetweenText, m_currOfficeMath);
				m_currMathFontType = currMathFontType;
			}
			else
			{
				int num = laTexStringPosition - currentLaTexElement.Length;
				string runElement = m_laTexString.Substring(num, m_laTexStringPosition - num);
				SetRunElement(runElement);
			}
		}
		else
		{
			SetRunElement(currentLaTexElement);
			m_isToUseExistingCurrElement = true;
		}
	}

	private void ParseMathStyleType(MathStyleType mathStyleType)
	{
		int laTexStringPosition = m_laTexStringPosition;
		string currentLaTexElement = m_currentLaTexElement;
		m_currentLaTexElement = GetLaTexElement();
		if (m_currentLaTexElement == "{")
		{
			string inBetweenText = string.Empty;
			if (IsValidGroup(ref inBetweenText))
			{
				MathStyleType currMathStyleType = m_currMathStyleType;
				if (m_currMathStyleType == MathStyleType.Italic)
				{
					m_currMathStyleType = mathStyleType;
				}
				ResetAndParseMath(inBetweenText, m_currOfficeMath);
				m_currMathStyleType = currMathStyleType;
			}
			else
			{
				int num = laTexStringPosition - currentLaTexElement.Length;
				string runElement = m_laTexString.Substring(num, m_laTexStringPosition - num);
				SetRunElement(runElement);
			}
		}
		else
		{
			SetRunElement(currentLaTexElement);
			m_isToUseExistingCurrElement = true;
		}
	}

	private void ParseMathHasNormalText()
	{
		int laTexStringPosition = m_laTexStringPosition;
		string currentLaTexElement = m_currentLaTexElement;
		m_currentLaTexElement = GetLaTexElement();
		if (m_currentLaTexElement == "{")
		{
			string inBetweenText = string.Empty;
			if (IsValidGroup(ref inBetweenText))
			{
				bool currHasNormalText = m_currHasNormalText;
				m_currHasNormalText = true;
				ResetAndParseMath(inBetweenText, m_currOfficeMath);
				m_currHasNormalText = currHasNormalText;
			}
			else
			{
				int num = laTexStringPosition - currentLaTexElement.Length;
				string runElement = m_laTexString.Substring(num, m_laTexStringPosition - num);
				SetRunElement(runElement);
			}
		}
		else
		{
			SetRunElement("\\mathrm");
			m_isToUseExistingCurrElement = true;
		}
	}

	private void SetRunElement(string runElementString)
	{
		if (string.IsNullOrEmpty(runElementString))
		{
			return;
		}
		if (m_currOfficeMath == null)
		{
			m_currOfficeMath = m_mathPara.Maths.Add(m_mathPara.Maths.Count) as OfficeMath;
		}
		if (runElementString.StartsWith("\\"))
		{
			runElementString = GetSymbol(runElementString);
		}
		if (m_currOfficeMath.Functions.Count > 0 && m_currOfficeMath.Functions[m_currOfficeMath.Functions.Count - 1] is OfficeMathRunElement && (m_currOfficeMath.Functions[m_currOfficeMath.Functions.Count - 1] as OfficeMathRunElement).MathFormat.Style == m_currMathStyleType && (m_currOfficeMath.Functions[m_currOfficeMath.Functions.Count - 1] as OfficeMathRunElement).MathFormat.Font == m_currMathFontType && (m_currOfficeMath.Functions[m_currOfficeMath.Functions.Count - 1] as OfficeMathRunElement).MathFormat.HasNormalText == m_currHasNormalText)
		{
			IOfficeMathRunElement officeMathRunElement = m_currOfficeMath.Functions[m_currOfficeMath.Functions.Count - 1] as OfficeMathRunElement;
			m_documentLaTeXConverter.AppendTextInMathRun(officeMathRunElement, runElementString);
			return;
		}
		IOfficeMathRunElement officeMathRunElement2 = m_currOfficeMath.Functions.Add(MathFunctionType.RunElement) as IOfficeMathRunElement;
		m_documentLaTeXConverter.CreateMathRunElement(officeMathRunElement2, runElementString);
		if (m_currMathStyleType != 0)
		{
			officeMathRunElement2.MathFormat.Style = m_currMathStyleType;
		}
		if (m_currMathFontType != MathFontType.Roman)
		{
			officeMathRunElement2.MathFormat.Font = m_currMathFontType;
		}
		if (m_currHasNormalText)
		{
			officeMathRunElement2.MathFormat.HasNormalText = m_currHasNormalText;
		}
	}

	private void ResetAndParseMath(string currString, OfficeMath currMath)
	{
		if (currString != null)
		{
			string laTexString = m_laTexString;
			OfficeMath currOfficeMath = m_currOfficeMath;
			int laTexStringPosition = m_laTexStringPosition;
			string currentLaTexElement = m_currentLaTexElement;
			bool isToUseExistingCurrElement = m_isToUseExistingCurrElement;
			m_laTexString = currString;
			m_currOfficeMath = currMath;
			m_laTexStringPosition = 0;
			m_currentLaTexElement = null;
			m_isToUseExistingCurrElement = false;
			ParseMathEquation();
			m_laTexString = laTexString;
			m_currOfficeMath = currOfficeMath;
			m_laTexStringPosition = laTexStringPosition;
			m_currentLaTexElement = currentLaTexElement;
			m_isToUseExistingCurrElement = isToUseExistingCurrElement;
		}
	}

	private string GetEquationString()
	{
		m_currentLaTexElement = GetLaTexElement();
		string inBetweenText = null;
		bool flag = false;
		bool flag2 = false;
		if (m_currentLaTexElement == "{")
		{
			flag = IsValidGroup(ref inBetweenText);
		}
		else
		{
			flag2 = true;
		}
		if (flag2 && m_currentLaTexElement != null && m_currentLaTexElement.Length == 1 && (char.IsLetterOrDigit(m_currentLaTexElement[0]) || m_currentLaTexElement == " "))
		{
			inBetweenText = m_currentLaTexElement;
		}
		else if (!flag)
		{
			inBetweenText = null;
		}
		return inBetweenText;
	}

	private string GetLaTexElement()
	{
		if (m_laTexStringPosition > m_laTexString.Length - 1)
		{
			return null;
		}
		m_currentLaTexElement = null;
		m_currentLaTexElement += m_laTexString[m_laTexStringPosition];
		m_laTexStringPosition++;
		if (m_currentLaTexElement == "\\")
		{
			while (m_laTexStringPosition < m_laTexString.Length && (char.IsLetter(m_laTexString[m_laTexStringPosition]) || m_laTexString[m_laTexStringPosition] == ';'))
			{
				m_currentLaTexElement += m_laTexString[m_laTexStringPosition];
				m_laTexStringPosition++;
			}
			return m_currentLaTexElement;
		}
		return m_currentLaTexElement;
	}

	private void Close()
	{
		m_laTexString = null;
		m_currOfficeMath = null;
		m_laTexStringPosition = 0;
		m_currentLaTexElement = null;
		m_isToUseExistingCurrElement = false;
	}

	private string GetSymbol(string latexString)
	{
		if (string.IsNullOrEmpty(latexString))
		{
			return latexString;
		}
		string text = null;
		if (text == null)
		{
			text = GetBasicMathCharString(latexString);
		}
		if (text == null)
		{
			text = GetGreekLetterCharString(latexString);
		}
		if (text == null)
		{
			text = GetLetterLikeSymbolCharString(latexString);
		}
		if (text == null)
		{
			text = GetOperatorsSymbolCharString(latexString);
		}
		if (text == null)
		{
			text = GetArrowCharString(latexString);
		}
		if (text == null)
		{
			text = GetNegatedCharString(latexString);
		}
		if (text == null)
		{
			text = GetGeometryCharString(latexString);
		}
		if (text == null)
		{
			text = GetNonRenderableCharString(latexString);
		}
		if (text == null)
		{
			text = latexString;
		}
		return text;
	}

	private string GetBasicMathCharString(string LaTexString)
	{
		return LaTexString switch
		{
			"\\pm" => '±'.ToString(), 
			"\\infty" => '∞'.ToString(), 
			"\\neq" => '≠'.ToString(), 
			"\\times" => '×'.ToString(), 
			"\\div" => '÷'.ToString(), 
			"\\propto" => '∝'.ToString(), 
			"\\ll" => '≪'.ToString(), 
			"\\gg" => '≫'.ToString(), 
			"\\le" => '≤'.ToString(), 
			"\\geq" => '≥'.ToString(), 
			"\\mp" => '∓'.ToString(), 
			"\\cong" => '≅'.ToString(), 
			"\\approx" => '≈'.ToString(), 
			"\\equiv" => '≡'.ToString(), 
			"\\forall" => '∀'.ToString(), 
			"\\complement" => '∁'.ToString(), 
			"\\partial" => '∂'.ToString(), 
			"\\cup" => '∪'.ToString(), 
			"\\cap" => '∩'.ToString(), 
			"\\emptyset" => '∅'.ToString(), 
			"\\degf" => '℉'.ToString(), 
			"\\degc" => '℃'.ToString(), 
			"\\nabla" => '∇'.ToString(), 
			"\\exists" => '∃'.ToString(), 
			"\\nexists" => '∄'.ToString(), 
			"\\in" => '∈'.ToString(), 
			"\\ni" => '∋'.ToString(), 
			"\\gets" => '←'.ToString(), 
			"\\uparrow" => '↑'.ToString(), 
			"\\rightarrow" => '→'.ToString(), 
			"\\downarrow" => '↓'.ToString(), 
			"\\leftrightarrow" => '↔'.ToString(), 
			"\\therefore" => '∴'.ToString(), 
			"\\lnot" => '¬'.ToString(), 
			"\\alpha" => 'α'.ToString(), 
			"\\beta" => 'β'.ToString(), 
			"\\gamma" => 'γ'.ToString(), 
			"\\delta" => 'δ'.ToString(), 
			"\\varepsilon" => 'ε'.ToString(), 
			"\\epsilon" => 'ϵ'.ToString(), 
			"\\theta" => 'θ'.ToString(), 
			"\\vartheta" => 'ϑ'.ToString(), 
			"\\mu" => 'μ'.ToString(), 
			"\\pi" => 'π'.ToString(), 
			"\\rho" => 'ρ'.ToString(), 
			"\\sigma" => 'σ'.ToString(), 
			"\\tau" => 'τ'.ToString(), 
			"\\varphi" => 'φ'.ToString(), 
			"\\omega" => 'ω'.ToString(), 
			"\\ast" => '*'.ToString(), 
			"\\bullet" => '∙'.ToString(), 
			"\\vdots" => '⋮'.ToString(), 
			"\\cdots" => '⋯'.ToString(), 
			"\\ddots" => '⋱'.ToString(), 
			"\\aleph" => 'ℵ'.ToString(), 
			"\\beth" => 'ℶ'.ToString(), 
			"\\qed" => '∎'.ToString(), 
			_ => null, 
		};
	}

	private string GetGreekLetterCharString(string LaTexString)
	{
		return LaTexString switch
		{
			"\\alpha" => 'α'.ToString(), 
			"\\beta" => 'β'.ToString(), 
			"\\gamma" => 'γ'.ToString(), 
			"\\delta" => 'δ'.ToString(), 
			"\\varepsilon" => 'ε'.ToString(), 
			"\\epsilon" => 'ϵ'.ToString(), 
			"\\zeta" => 'ζ'.ToString(), 
			"\\eta" => 'η'.ToString(), 
			"\\theta" => 'θ'.ToString(), 
			"\\vartheta" => 'ϑ'.ToString(), 
			"\\iota" => 'ι'.ToString(), 
			"\\kappa" => 'κ'.ToString(), 
			"\\lambda" => 'λ'.ToString(), 
			"\\mu" => 'μ'.ToString(), 
			"\\nu" => 'ν'.ToString(), 
			"\\xi" => 'ξ'.ToString(), 
			"\\pi" => 'π'.ToString(), 
			"\\varpi" => 'ϖ'.ToString(), 
			"\\rho" => 'ρ'.ToString(), 
			"\\varrho" => 'ϱ'.ToString(), 
			"\\sigma" => 'σ'.ToString(), 
			"\\varsigma" => 'ς'.ToString(), 
			"\\tau" => 'τ'.ToString(), 
			"\\upsilon" => 'υ'.ToString(), 
			"\\varphi" => 'φ'.ToString(), 
			"\\phi" => 'ϕ'.ToString(), 
			"\\chi" => 'χ'.ToString(), 
			"\\psi" => 'ψ'.ToString(), 
			"\\omega" => 'ω'.ToString(), 
			"\\Gamma" => 'Γ'.ToString(), 
			"\\Delta" => 'Δ'.ToString(), 
			"\\Theta" => 'Θ'.ToString(), 
			"\\Lambda" => 'Λ'.ToString(), 
			"\\Xi" => 'Ξ'.ToString(), 
			"\\Pi" => 'Π'.ToString(), 
			"\\Sigma" => 'Σ'.ToString(), 
			"\\Upsilon" => 'Υ'.ToString(), 
			"\\Phi" => 'Φ'.ToString(), 
			"\\Psi" => 'Ψ'.ToString(), 
			"\\Omega" => 'Ω'.ToString(), 
			_ => null, 
		};
	}

	private string GetLetterLikeSymbolCharString(string LaTexString)
	{
		return LaTexString switch
		{
			"\\forall" => '∀'.ToString(), 
			"\\complement" => '∁'.ToString(), 
			"\\mathbb{C}" => 'C'.ToString(), 
			"\\partial" => '∂'.ToString(), 
			"\\mathcal{g}" => 'g'.ToString(), 
			"\\mathcal{H}" => 'H'.ToString(), 
			"\\mathfrak{H}" => 'H'.ToString(), 
			"\\hbar" => 'ℏ'.ToString(), 
			"\\imath" => 'ı'.ToString(), 
			"\\mathfrak{I}" => 'I'.ToString(), 
			"\\varkappa" => 'ϰ'.ToString(), 
			"\\mathcal{L}" => 'L'.ToString(), 
			"\\ell" => 'l'.ToString(), 
			"\\mathbb{N}" => 'N'.ToString(), 
			"\\wp" => '℘'.ToString(), 
			"\\mathbb{Q}" => 'Q'.ToString(), 
			"\\mathcal{R}" => 'R'.ToString(), 
			"\\mathfrak{R}" => 'R'.ToString(), 
			"\\mathbb{R}" => 'R'.ToString(), 
			"\\mathbb{Z}" => 'Z'.ToString(), 
			"\\mathcal{B}" => 'B'.ToString(), 
			"\\mathcal{E}" => 'E'.ToString(), 
			"\\exists" => '∃'.ToString(), 
			"\\nexists" => '∄'.ToString(), 
			"\\mathcal{F}" => 'F'.ToString(), 
			"\\mathcal{M}" => 'M'.ToString(), 
			"\\mathcal{o}" => 'o'.ToString(), 
			"\\aleph" => 'ℵ'.ToString(), 
			"\\beth" => 'ℶ'.ToString(), 
			"\\gimel" => 'ℷ'.ToString(), 
			"\\daleth" => 'ℸ'.ToString(), 
			_ => null, 
		};
	}

	private string GetOperatorsSymbolCharString(string laTexString)
	{
		string text = null;
		if (text == null)
		{
			text = GetCommonBinaryOperatorCharString(laTexString);
		}
		if (text == null)
		{
			text = GetCommonRelationalOperatorCharString(laTexString);
		}
		if (text == null)
		{
			text = GetAdvancedBinaryOperatorsCharString(laTexString);
		}
		if (text == null)
		{
			text = GetAdvancedRelationalOperatorCharString(laTexString);
		}
		return text;
	}

	private string GetCommonBinaryOperatorCharString(string LaTexString)
	{
		return LaTexString switch
		{
			"\\div" => '÷'.ToString(), 
			"\\times" => '×'.ToString(), 
			"\\pm" => '±'.ToString(), 
			"\\mp" => '∓'.ToString(), 
			"\\propto" => '∝'.ToString(), 
			"\\ast" => '*'.ToString(), 
			"\\circ" => '∘'.ToString(), 
			"\\bullet" => '∙'.ToString(), 
			"\\cdot" => '⋅'.ToString(), 
			"\\cap" => '∩'.ToString(), 
			"\\cup" => '∪'.ToString(), 
			"\\uplus" => '⊎'.ToString(), 
			"\\sqcap" => '⊓'.ToString(), 
			"\\sqcup" => '⊔'.ToString(), 
			"\\land" => '∧'.ToString(), 
			"\\vee" => '∨'.ToString(), 
			_ => null, 
		};
	}

	private string GetCommonRelationalOperatorCharString(string LaTexString)
	{
		return LaTexString switch
		{
			"\\neq" => '≠'.ToString(), 
			"\\le" => '≤'.ToString(), 
			"\\geq" => '≥'.ToString(), 
			"\\nless" => '≮'.ToString(), 
			"\\nleq" => '≰'.ToString(), 
			"\\ngt" => '≯'.ToString(), 
			"\\ngeq" => '≱'.ToString(), 
			"\\equiv" => '≡'.ToString(), 
			"\\sim" => '∼'.ToString(), 
			"\\simeq" => '≃'.ToString(), 
			"\\approx" => '≈'.ToString(), 
			"\\cong" => '≅'.ToString(), 
			"\\nequiv" => '≢'.ToString(), 
			"\\nsimeq" => '≄'.ToString(), 
			"\\napprox" => '≉'.ToString(), 
			"\\ncong" => '≇'.ToString(), 
			"\\propto" => '∝'.ToString(), 
			"\\ll" => '≪'.ToString(), 
			"\\gg" => '≫'.ToString(), 
			"\\in" => '∈'.ToString(), 
			"\\ni" => '∋'.ToString(), 
			"\\notin" => '∉'.ToString(), 
			"\\subset" => '⊂'.ToString(), 
			"\\supset" => '⊃'.ToString(), 
			"\\subseteq" => '⊆'.ToString(), 
			"\\supseteq" => '⊇'.ToString(), 
			"\\prcue" => '≺'.ToString(), 
			"\\succ" => '≻'.ToString(), 
			"\\preccurlyeq" => '≼'.ToString(), 
			"\\succcurlyeq" => '≽'.ToString(), 
			"\\sqsubset" => '⊏'.ToString(), 
			"\\sqsupset" => '⊐'.ToString(), 
			"\\sqsubseteq" => '⊑'.ToString(), 
			"\\sqsupseteq" => '⊒'.ToString(), 
			"\\parallel" => '∥'.ToString(), 
			"\\bot" => '⊥'.ToString(), 
			"\\vdash" => '⊢'.ToString(), 
			"\\dashv" => '⊣'.ToString(), 
			"\\bowtie" => '⋈'.ToString(), 
			"\\asymp" => '≍'.ToString(), 
			_ => null, 
		};
	}

	private string GetBasicNaryOperatorCharString(string LaTexString)
	{
		return LaTexString switch
		{
			"\\sum" => '∑'.ToString(), 
			"\\int" => '∫'.ToString(), 
			"\\iint" => '∬'.ToString(), 
			"\\iiint" => '∭'.ToString(), 
			"\\oint" => '∮'.ToString(), 
			"\\oiint" => '∯'.ToString(), 
			"\\oiiint" => '∰'.ToString(), 
			"\\cwint" => '∱'.ToString(), 
			"\\coint" => '∲'.ToString(), 
			"\\aoint" => '∳'.ToString(), 
			"\\prod" => '∏'.ToString(), 
			"\\amalg" => '∐'.ToString(), 
			"\\bigcap" => '⋂'.ToString(), 
			"\\bigcup" => '⋃'.ToString(), 
			"\\bigwedge" => '⋀'.ToString(), 
			"\\bigvee" => '⋁'.ToString(), 
			"\\bigodot" => '⨀'.ToString(), 
			"\\bigotimes" => '⨂'.ToString(), 
			"\\bigoplus" => '⨁'.ToString(), 
			"\\biguplus" => '⨄'.ToString(), 
			"\\bigudot" => '⨃'.ToString(), 
			_ => null, 
		};
	}

	private string GetAdvancedBinaryOperatorsCharString(string LaTexString)
	{
		return LaTexString switch
		{
			"\\dotplus" => '∔'.ToString(), 
			"\\dotminus" => '∸'.ToString(), 
			"\\setminus" => '∖'.ToString(), 
			"\\Cap" => '⋒'.ToString(), 
			"\\Cup" => '⋓'.ToString(), 
			"\\boxminus" => '⊟'.ToString(), 
			"\\boxtimes" => '⊠'.ToString(), 
			"\\boxdot" => '⊡'.ToString(), 
			"\\boxplus" => '⊞'.ToString(), 
			"\\divideontimes" => '⋇'.ToString(), 
			"\\ltimes" => '⋉'.ToString(), 
			"\\rtimes" => '⋊'.ToString(), 
			"\\leftthreetimes" => '⋋'.ToString(), 
			"\\rightthreetimes" => '⋌'.ToString(), 
			"\\curlywedge" => '⋏'.ToString(), 
			"\\curlyvee" => '⋎'.ToString(), 
			"\\odash" => '⊝'.ToString(), 
			"\\intercal" => '⊺'.ToString(), 
			"\\oplus" => '⊕'.ToString(), 
			"\\ominus" => '⊖'.ToString(), 
			"\\otimes" => '⊗'.ToString(), 
			"\\oslash" => '⊘'.ToString(), 
			"\\odot" => '⊙'.ToString(), 
			"\\oast" => '⊛'.ToString(), 
			"\\ocirc" => '⊚'.ToString(), 
			"\\dag" => '†'.ToString(), 
			"\\ddag" => '‡'.ToString(), 
			"\\star" => '⋆'.ToString(), 
			"\\diamond" => '⋄'.ToString(), 
			"\\wr" => '≀'.ToString(), 
			"\\triangle" => '△'.ToString(), 
			"\\bigwedge" => '⋀'.ToString(), 
			"\\bigvee" => '⋁'.ToString(), 
			"\\bigodot" => '⨀'.ToString(), 
			"\\bigotimes" => '⨂'.ToString(), 
			"\\bigoplus" => '⨁'.ToString(), 
			"\\bigsqcap" => '⨅'.ToString(), 
			"\\bigsqcup" => '⨆'.ToString(), 
			"\\biguplus" => '⨄'.ToString(), 
			"\\bigudot" => '⨃'.ToString(), 
			_ => null, 
		};
	}

	private string GetAdvancedRelationalOperatorCharString(string LaTexString)
	{
		return LaTexString switch
		{
			"\\therefore" => '∴'.ToString(), 
			"\\because" => '∵'.ToString(), 
			"\\lll" => '⋘'.ToString(), 
			"\\ggg" => '⋙'.ToString(), 
			"\\leqq" => '≦'.ToString(), 
			"\\geqq" => '≧'.ToString(), 
			"\\lesssim" => '≲'.ToString(), 
			"\\gtrsim" => '≳'.ToString(), 
			"\\lessdot" => '⋖'.ToString(), 
			"\\gtrdot" => '⋗'.ToString(), 
			"\\lessgtr" => '≶'.ToString(), 
			"\\lesseqgtr" => '⋚'.ToString(), 
			"\\gtrless" => '≷'.ToString(), 
			"\\gtreqless" => '⋛'.ToString(), 
			"\\Doteq" => '≑'.ToString(), 
			"\\fallingdotseq" => '≒'.ToString(), 
			"\\risingdotseq" => '≓'.ToString(), 
			"\\backsim" => '∽'.ToString(), 
			"\\approxeq" => '≊'.ToString(), 
			"\\backsimeq" => '⋍'.ToString(), 
			"\\preccurlyeq" => '≼'.ToString(), 
			"\\succcurlyeq" => '≽'.ToString(), 
			"\\curlyeqprec" => '⋞'.ToString(), 
			"\\curlyeqsucc" => '⋟'.ToString(), 
			"\\precsim" => '≾'.ToString(), 
			"\\succsim" => '≿'.ToString(), 
			"\\eqless" => '⋜'.ToString(), 
			"\\eqgtr" => '⋝'.ToString(), 
			"\\subseteq" => '⊆'.ToString(), 
			"\\supseteq" => '⊇'.ToString(), 
			"\\vartriangleleft" => '⊲'.ToString(), 
			"\\vartriangleright" => '⊳'.ToString(), 
			"\\trianglelefteq" => '⊴'.ToString(), 
			"\\trianglerighteq" => '⊵'.ToString(), 
			"\\models" => '⊨'.ToString(), 
			"\\Subset" => '⋐'.ToString(), 
			"\\Supset" => '⋑'.ToString(), 
			"\\sqsubset" => '⊏'.ToString(), 
			"\\sqsupset" => '⊐'.ToString(), 
			"\\Vdash" => '⊩'.ToString(), 
			"\\Vvdash" => '⊪'.ToString(), 
			"\\eqcirc" => '≖'.ToString(), 
			"\\circeq" => '≗'.ToString(), 
			"\\Deltaeq" => '≜'.ToString(), 
			"\\bumpeq" => '≏'.ToString(), 
			"\\Bumpeq" => '≎'.ToString(), 
			"\\propto" => '∝'.ToString(), 
			"\\between" => '≬'.ToString(), 
			"\\pitchfork" => '⋔'.ToString(), 
			"\\doteq" => '≐'.ToString(), 
			"\\bowtie" => '⋈'.ToString(), 
			_ => null, 
		};
	}

	private string GetArrowCharString(string LaTexString)
	{
		return LaTexString switch
		{
			"\\get" => '←'.ToString(), 
			"\\rightarrow" => '→'.ToString(), 
			"\\uparrow" => '↑'.ToString(), 
			"\\downarrow" => '↓'.ToString(), 
			"\\leftrightarrow" => '↔'.ToString(), 
			"\\updownarrow" => '↕'.ToString(), 
			"\\Leftarrow" => '⇐'.ToString(), 
			"\\Rightarrow" => '⇒'.ToString(), 
			"\\Uparrow" => '⇑'.ToString(), 
			"\\Downarrow" => '⇓'.ToString(), 
			"\\Leftrightarrow" => '⇔'.ToString(), 
			"\\Updownarrow" => '⇕'.ToString(), 
			"\\longleftarrow" => '⟵'.ToString(), 
			"\\longrightarrow" => '⟶'.ToString(), 
			"\\longleftrightarrow" => '⟷'.ToString(), 
			"\\Longleftarrow" => '⟸'.ToString(), 
			"\\Longrightarrow" => '⟹'.ToString(), 
			"\\Longleftrightarrow" => '⟺'.ToString(), 
			"\\nearrow" => '↗'.ToString(), 
			"\\nwarrow" => '↖'.ToString(), 
			"\\searrow" => '↘'.ToString(), 
			"\\swarrow" => '↙'.ToString(), 
			"\\nleftarrow" => '↚'.ToString(), 
			"\\nrightarrow" => '↛'.ToString(), 
			"\\nleftrightarrow" => '↮'.ToString(), 
			"\\nLeftarrow" => '⇍'.ToString(), 
			"\\nRightarrow" => '⇏'.ToString(), 
			"\\nLeftrightarrow" => '⇎'.ToString(), 
			"\\dashleftarrow" => '⇠'.ToString(), 
			"\\dashrightarrow" => '⇢'.ToString(), 
			"\\mapstoleft" => '↤'.ToString(), 
			"\\mapsto" => '↦'.ToString(), 
			"\\longmapstoleft" => '⟻'.ToString(), 
			"\\longmapsto" => '⟼'.ToString(), 
			"\\hookleftarrow" => '↩'.ToString(), 
			"\\hookrightarrow" => '↪'.ToString(), 
			"\\leftharpoonup" => '↼'.ToString(), 
			"\\leftharpoondown" => '↽'.ToString(), 
			"\\rightharpoonup" => '⇀'.ToString(), 
			"\\rightharpoondown" => '⇁'.ToString(), 
			"\\upharpoonleft" => '↿'.ToString(), 
			"\\upharpoonright" => '↾'.ToString(), 
			"\\downharpoonleft" => '⇃'.ToString(), 
			"\\downharpoonright" => '⇂'.ToString(), 
			"\\leftrightharpoons" => '⇋'.ToString(), 
			"\\rlhar" => '⇌'.ToString(), 
			"\\leftleftarrows" => '⇇'.ToString(), 
			"\\rightrightarrows" => '⇉'.ToString(), 
			"\\upuparrows" => '⇈'.ToString(), 
			"\\downdownarrows" => '⇊'.ToString(), 
			"\\leftrightarrows" => '⇆'.ToString(), 
			"\\rightleftarrows" => '⇄'.ToString(), 
			"\\looparrowleft" => '↫'.ToString(), 
			"\\looparrowright" => '↬'.ToString(), 
			"\\leftarrowtail" => '↢'.ToString(), 
			"\\rightarrowtail" => '↣'.ToString(), 
			"\\Lsh" => '↰'.ToString(), 
			"\\Rsh" => '↱'.ToString(), 
			"\\ldsh" => '↲'.ToString(), 
			"\\rdsh" => '↳'.ToString(), 
			"\\Lleftarrow" => '⇚'.ToString(), 
			"\\Rrightarrow" => '⇛'.ToString(), 
			"\\twoheadleftarrow" => '↞'.ToString(), 
			"\\twoheadrightarrow" => '↠'.ToString(), 
			"\\curvearrowleft" => '↶'.ToString(), 
			"\\curvearrowright" => '↷'.ToString(), 
			"\\circlearrowleft" => '↺'.ToString(), 
			"\\circlearrowright" => '↻'.ToString(), 
			"\\multimap" => '⊸'.ToString(), 
			"\\leftrightwavearrow" => '↭'.ToString(), 
			"\\leftwavearrow" => '↜'.ToString(), 
			"\\rightwavearrow" => '↝'.ToString(), 
			"\\leftsquigarrow" => '⇜'.ToString(), 
			_ => null, 
		};
	}

	private string GetNegatedCharString(string LaTexString)
	{
		return LaTexString switch
		{
			"\\neq" => '≠'.ToString(), 
			"\\nless" => '≮'.ToString(), 
			"\\ngt" => '≯'.ToString(), 
			"\\nleq" => '≰'.ToString(), 
			"\\ngeq" => '≱'.ToString(), 
			"\\nequiv" => '≢'.ToString(), 
			"\\nsim" => '≁'.ToString(), 
			"\\nsimeq" => '≄'.ToString(), 
			"\\napprox" => '≉'.ToString(), 
			"\\ncong" => '≇'.ToString(), 
			"\\nasymp" => '≭'.ToString(), 
			"\\lneqq" => '≨'.ToString(), 
			"\\gneqq" => '≩'.ToString(), 
			"\\nprec" => '⊀'.ToString(), 
			"\\nsucc" => '⊁'.ToString(), 
			"\\npreccurlyeq" => '⋠'.ToString(), 
			"\\nsucccurlyeq" => '⋡'.ToString(), 
			"\\notin" => '∉'.ToString(), 
			"\\notni" => '∌'.ToString(), 
			"\\nsub" => '⊄'.ToString(), 
			"\\nsup" => '⊅'.ToString(), 
			"\\nsubseteq" => '⊈'.ToString(), 
			"\\nsupseteq" => '⊉'.ToString(), 
			"\\supsetneq" => '⊋'.ToString(), 
			"\\nsqsubseteq" => '⋢'.ToString(), 
			"\\nsqsupseteq" => '⋣'.ToString(), 
			"\\lnsim" => '⋦'.ToString(), 
			"\\gnsim" => '⋧'.ToString(), 
			"\\precnsim" => '⋨'.ToString(), 
			"\\succnsim" => '⋩'.ToString(), 
			"\\ntriangleleft" => '⋪'.ToString(), 
			"\\ntriangleright" => '⋫'.ToString(), 
			"\\ntrianglelefteq" => '⋬'.ToString(), 
			"\\ntrianglerighteq" => '⋭'.ToString(), 
			"\\nmid" => '∤'.ToString(), 
			"\\nparallel" => '∦'.ToString(), 
			"\\nvdash" => '⊬'.ToString(), 
			"\\nvDash" => '⊭'.ToString(), 
			"\\nVdash" => '⊮'.ToString(), 
			"\\nVDash" => '⊯'.ToString(), 
			"\\nexists" => '∄'.ToString(), 
			_ => null, 
		};
	}

	private string GetGeometryCharString(string LaTexString)
	{
		return LaTexString switch
		{
			"\\rightangle" => '∟'.ToString(), 
			"\\angle" => '∠'.ToString(), 
			"\\angmsd" => '∡'.ToString(), 
			"\\angsph" => '∢'.ToString(), 
			"\\angrtvb" => '⊾'.ToString(), 
			"\\righttriangle" => '⊿'.ToString(), 
			"\\epar" => '⋕'.ToString(), 
			"\\bot" => '⊥'.ToString(), 
			"\\nmid" => '∤'.ToString(), 
			"\\parallel" => '∥'.ToString(), 
			"\\nparallel" => '∦'.ToString(), 
			"\\colon" => '∶'.ToString(), 
			"\\Colon" => '∷'.ToString(), 
			"\\therefore" => '∴'.ToString(), 
			"\\because" => '∵'.ToString(), 
			"\\qed" => '∎'.ToString(), 
			_ => null, 
		};
	}

	private string GetNonRenderableCharString(string LaTexString)
	{
		if (LaTexString == "\\;")
		{
			return '\u2005'.ToString();
		}
		return null;
	}

	internal string GetLaTeX(IOfficeMathParagraph officeMathParagraph)
	{
		m_laTexString = string.Empty;
		m_mathPara = officeMathParagraph;
		m_documentLaTeXConverter = (officeMathParagraph as OfficeMathParagraph).m_documentLaTeXConverter;
		for (int i = 0; i < officeMathParagraph.Maths.Count; i++)
		{
			ProcessMath(officeMathParagraph.Maths[i]);
		}
		string laTexString = m_laTexString;
		Close();
		return laTexString;
	}

	private void ProcessMath(IOfficeMath officeMath)
	{
		ProcessMathFunctions(officeMath.Functions);
	}

	private void ProcessMathFunctions(IOfficeMathBaseCollection officeMathFunctions)
	{
		for (int i = 0; i < officeMathFunctions.Count; i++)
		{
			ProcessFunction(officeMathFunctions[i]);
		}
	}

	private void ProcessFunction(IOfficeMathFunctionBase officeMathFunctionBase)
	{
		switch (officeMathFunctionBase.Type)
		{
		case MathFunctionType.Accent:
			ProcessMathAccent(officeMathFunctionBase);
			break;
		case MathFunctionType.Bar:
			ProcessMathBar(officeMathFunctionBase);
			break;
		case MathFunctionType.Box:
			ProcessMathBox(officeMathFunctionBase);
			break;
		case MathFunctionType.BorderBox:
			ProcessMathBorderBox(officeMathFunctionBase);
			break;
		case MathFunctionType.Fraction:
			ProcessMathFraction(officeMathFunctionBase);
			break;
		case MathFunctionType.Function:
			ProcessMathFunction(officeMathFunctionBase);
			break;
		case MathFunctionType.Delimiter:
			ProcessMathDelimiter(officeMathFunctionBase);
			break;
		case MathFunctionType.GroupCharacter:
			ProcessMathGroupCharacter(officeMathFunctionBase);
			break;
		case MathFunctionType.Limit:
			ProcessMathLimit(officeMathFunctionBase);
			break;
		case MathFunctionType.Matrix:
			ProcessMathMatrix(officeMathFunctionBase);
			break;
		case MathFunctionType.NArray:
			ProcessMathNArray(officeMathFunctionBase);
			break;
		case MathFunctionType.Radical:
			ProcessMathRadical(officeMathFunctionBase);
			break;
		case MathFunctionType.SubSuperscript:
			ProcessMathSubSuperscript(officeMathFunctionBase);
			break;
		case MathFunctionType.LeftSubSuperscript:
			ProcessMathLeftSubSuperscript(officeMathFunctionBase);
			break;
		case MathFunctionType.RightSubSuperscript:
			ProcessMathRightSubSuperscript(officeMathFunctionBase);
			break;
		case MathFunctionType.EquationArray:
			ProcessEquationArrayElement(officeMathFunctionBase);
			break;
		case MathFunctionType.RunElement:
			ProcessRunElement(officeMathFunctionBase);
			break;
		case MathFunctionType.Phantom:
			break;
		}
	}

	private void ProcessMathAccent(IOfficeMathFunctionBase officeMathFunctionBase)
	{
		int num = Convert.ToInt32((officeMathFunctionBase as IOfficeMathAccent).AccentCharacter[0]);
		switch (num)
		{
		case 775:
			m_laTexString += "\\dot";
			break;
		case 776:
			m_laTexString += "\\ddot";
			break;
		case 8411:
			m_laTexString += "\\dddot";
			break;
		case 770:
			m_laTexString += "\\widehat";
			break;
		case 780:
			m_laTexString += "\\check";
			break;
		case 768:
			m_laTexString += "\\grave";
			break;
		case 774:
			m_laTexString += "\\breve";
			break;
		case 771:
			m_laTexString += "\\widetilde";
			break;
		case 769:
			m_laTexString += "\\acute";
			break;
		case 773:
			m_laTexString += "\\bar";
			break;
		case 831:
			m_laTexString += "\\bar{\\bar";
			break;
		case 8407:
			m_laTexString += "\\vec";
			break;
		case 8401:
			m_laTexString += "\\hvec";
			break;
		}
		m_laTexString += "{";
		ProcessMathFunctions((officeMathFunctionBase as IOfficeMathAccent).Equation.Functions);
		m_laTexString += "}";
		if (num == 831)
		{
			m_laTexString += "}";
		}
	}

	private void ProcessMathBar(IOfficeMathFunctionBase officeMathFunctionBase)
	{
		if ((officeMathFunctionBase as IOfficeMathBar).BarTop)
		{
			m_laTexString += "\\overline";
		}
		else
		{
			m_laTexString += "\\underline";
		}
		m_laTexString += "{";
		ProcessMathFunctions((officeMathFunctionBase as IOfficeMathBar).Equation.Functions);
		m_laTexString += "}";
	}

	private void ProcessMathBox(IOfficeMathFunctionBase officeMathFunctionBase)
	{
		m_laTexString += "\\box{";
		ProcessMathFunctions((officeMathFunctionBase as IOfficeMathBox).Equation.Functions);
		m_laTexString += "}";
	}

	private void ProcessMathBorderBox(IOfficeMathFunctionBase officeMathFunctionBase)
	{
		m_laTexString += "\\fbox{";
		ProcessMathFunctions((officeMathFunctionBase as IOfficeMathBorderBox).Equation.Functions);
		m_laTexString += "}";
	}

	private void ProcessMathFraction(IOfficeMathFunctionBase officeMathFunctionBase)
	{
		MathFractionType fractionType = (officeMathFunctionBase as IOfficeMathFraction).FractionType;
		switch (fractionType)
		{
		case MathFractionType.NormalFractionBar:
			m_laTexString += "\\frac";
			break;
		case MathFractionType.SkewedFractionBar:
		case MathFractionType.FractionInline:
			m_laTexString += "\\sfrac";
			break;
		}
		m_laTexString += "{";
		ProcessMathFunctions((officeMathFunctionBase as IOfficeMathFraction).Numerator.Functions);
		m_laTexString += "}";
		if (fractionType == MathFractionType.NoFractionBar)
		{
			m_laTexString += "\\atop";
		}
		m_laTexString += "{";
		ProcessMathFunctions((officeMathFunctionBase as IOfficeMathFraction).Denominator.Functions);
		m_laTexString += "}";
	}

	private void ProcessMathFunction(IOfficeMathFunctionBase officeMathFunctionBase)
	{
		isFunctionNameProcessing = true;
		m_laTexString += "\\";
		ProcessMathFunctions((officeMathFunctionBase as OfficeMathFunction).FunctionName.Functions);
		isFunctionNameProcessing = false;
		m_laTexString += "{";
		ProcessMathFunctions((officeMathFunctionBase as OfficeMathFunction).Equation.Functions);
		m_laTexString += "}";
	}

	private void ProcessMathDelimiter(IOfficeMathFunctionBase officeMathFunctionBase)
	{
		m_laTexString += "\\left";
		AppendDelimiterCharacter((officeMathFunctionBase as IOfficeMathDelimiter).BeginCharacter);
		m_laTexString += " ";
		for (int i = 0; i < (officeMathFunctionBase as IOfficeMathDelimiter).Equation.Count; i++)
		{
			ProcessMathFunctions((officeMathFunctionBase as IOfficeMathDelimiter).Equation[i].Functions);
			if ((officeMathFunctionBase as IOfficeMathDelimiter).Equation.Count - 1 > i)
			{
				m_laTexString += "\\middle";
				AppendDelimiterCharacter((officeMathFunctionBase as IOfficeMathDelimiter).Seperator);
				m_laTexString += " ";
			}
		}
		m_laTexString += "\\right";
		AppendDelimiterCharacter((officeMathFunctionBase as IOfficeMathDelimiter).EndCharacter);
	}

	private void AppendDelimiterCharacter(string delimiterCharacter)
	{
		if (delimiterCharacter.Length > 0)
		{
			switch (Convert.ToInt32(delimiterCharacter[0]))
			{
			case 40:
				m_laTexString += '(';
				break;
			case 91:
				m_laTexString += '[';
				break;
			case 123:
				m_laTexString += '{';
				break;
			case 9001:
				m_laTexString += "\\langle";
				break;
			case 8968:
				m_laTexString += "\\lceil";
				break;
			case 8970:
				m_laTexString += "\\lfloor";
				break;
			case 41:
				m_laTexString += ')';
				break;
			case 93:
				m_laTexString += ']';
				break;
			case 125:
				m_laTexString += '}';
				break;
			case 9002:
				m_laTexString += "\\rangle";
				break;
			case 8969:
				m_laTexString += "\\rceil";
				break;
			case 8971:
				m_laTexString += "\\rfloor";
				break;
			case 124:
				m_laTexString += '|';
				break;
			}
		}
		else
		{
			m_laTexString += '.';
		}
	}

	private void ProcessMathGroupCharacter(IOfficeMathFunctionBase officeMathFunctionBase)
	{
		switch (Convert.ToInt32((officeMathFunctionBase as IOfficeMathGroupCharacter).GroupCharacter[0]))
		{
		case 9182:
			m_laTexString += "\\overbrace";
			break;
		case 9183:
			m_laTexString += "\\underbrace";
			break;
		}
		m_laTexString += "{";
		ProcessMathFunctions((officeMathFunctionBase as IOfficeMathGroupCharacter).Equation.Functions);
		m_laTexString += "}";
	}

	private void ProcessMathLimit(IOfficeMathFunctionBase officeMathFunctionBase)
	{
		if (!isFunctionNameProcessing)
		{
			m_laTexString += "{";
		}
		ProcessMathFunctions((officeMathFunctionBase as IOfficeMathLimit).Equation.Functions);
		if (!isFunctionNameProcessing)
		{
			m_laTexString += "}";
		}
		m_laTexString += "\\below";
		m_laTexString += "{";
		ProcessMathFunctions((officeMathFunctionBase as IOfficeMathLimit).Limit.Functions);
		m_laTexString += "}";
	}

	private void ProcessMathMatrix(IOfficeMathFunctionBase officeMathFunctionBase)
	{
		int count = (officeMathFunctionBase as IOfficeMathMatrix).Rows.Count;
		m_laTexString += "\\begin{matrix}";
		for (int i = 0; i < count; i++)
		{
			int count2 = (officeMathFunctionBase as IOfficeMathMatrix).Rows[i].Arguments.Count;
			for (int j = 0; j < count2; j++)
			{
				ProcessMathFunctions((officeMathFunctionBase as IOfficeMathMatrix).Rows[i].Arguments[j].Functions);
				if (j != count2 - 1)
				{
					m_laTexString += "&";
				}
			}
			m_laTexString += "\\\\";
		}
		m_laTexString += "\\end{matrix}";
	}

	private void ProcessMathNArray(IOfficeMathFunctionBase officeMathFunctionBase)
	{
		switch (Convert.ToInt32((officeMathFunctionBase as IOfficeMathNArray).NArrayCharacter[0]))
		{
		case 8721:
			m_laTexString += "\\sum";
			break;
		case 8747:
			m_laTexString += "\\int";
			break;
		case 8748:
			m_laTexString += "\\iint";
			break;
		case 8749:
			m_laTexString += "\\iiint";
			break;
		case 8750:
			m_laTexString += "\\oint";
			break;
		case 8751:
			m_laTexString += "\\oiint";
			break;
		case 8752:
			m_laTexString += "\\oiiint";
			break;
		case 8753:
			m_laTexString += "\\cwint";
			break;
		case 8754:
			m_laTexString += "\\coint";
			break;
		case 8755:
			m_laTexString += "\\aoint";
			break;
		case 8719:
			m_laTexString += "\\prod";
			break;
		case 8720:
			m_laTexString += "\\amalg";
			break;
		case 8898:
			m_laTexString += "\\bigcap";
			break;
		case 8899:
			m_laTexString += "\\bigcup";
			break;
		case 8896:
			m_laTexString += "\\bigwedge";
			break;
		case 8897:
			m_laTexString += "\\bigvee";
			break;
		case 10752:
			m_laTexString += "\\bigodot";
			break;
		case 10754:
			m_laTexString += "\\bigotimes";
			break;
		case 10753:
			m_laTexString += "\\bigoplus";
			break;
		case 10756:
			m_laTexString += "\\biguplus";
			break;
		case 10755:
			m_laTexString += "\\bigudot";
			break;
		}
		if ((officeMathFunctionBase as IOfficeMathNArray).Subscript.Functions.Count > 0)
		{
			m_laTexString += "_{";
			ProcessMathFunctions((officeMathFunctionBase as IOfficeMathNArray).Subscript.Functions);
			m_laTexString += "}";
		}
		if ((officeMathFunctionBase as IOfficeMathNArray).Superscript.Functions.Count > 0)
		{
			m_laTexString += "^{";
			ProcessMathFunctions((officeMathFunctionBase as IOfficeMathNArray).Superscript.Functions);
			m_laTexString += "}";
		}
		m_laTexString += "{";
		ProcessMathFunctions((officeMathFunctionBase as IOfficeMathNArray).Equation.Functions);
		m_laTexString += "}";
	}

	private void ProcessMathRadical(IOfficeMathFunctionBase officeMathFunctionBase)
	{
		m_laTexString += "\\sqrt";
		if ((officeMathFunctionBase as IOfficeMathRadical).Degree.Functions.Count > 0)
		{
			m_laTexString += "[";
			ProcessMathFunctions((officeMathFunctionBase as IOfficeMathRadical).Degree.Functions);
			m_laTexString += "]";
		}
		m_laTexString += "{";
		ProcessMathFunctions((officeMathFunctionBase as IOfficeMathRadical).Equation.Functions);
		m_laTexString += "}";
	}

	private void ProcessMathSubSuperscript(IOfficeMathFunctionBase officeMathFunctionBase)
	{
		if (!isFunctionNameProcessing)
		{
			m_laTexString += "{";
		}
		ProcessMathFunctions((officeMathFunctionBase as IOfficeMathScript).Equation.Functions);
		if (!isFunctionNameProcessing)
		{
			m_laTexString += "}";
		}
		m_laTexString += (((officeMathFunctionBase as IOfficeMathScript).ScriptType == MathScriptType.Subscript) ? "_" : "^");
		m_laTexString += "{";
		ProcessMathFunctions((officeMathFunctionBase as IOfficeMathScript).Script.Functions);
		m_laTexString += "}";
	}

	private void ProcessMathLeftSubSuperscript(IOfficeMathFunctionBase officeMathFunctionBase)
	{
		m_laTexString += "{";
		m_laTexString += "_{";
		ProcessMathFunctions((officeMathFunctionBase as IOfficeMathLeftScript).Subscript.Functions);
		m_laTexString += "}";
		m_laTexString += "^{";
		ProcessMathFunctions((officeMathFunctionBase as IOfficeMathLeftScript).Superscript.Functions);
		m_laTexString += "}";
		m_laTexString += "}";
		m_laTexString += "{";
		ProcessMathFunctions((officeMathFunctionBase as IOfficeMathLeftScript).Equation.Functions);
		m_laTexString += "}";
	}

	private void ProcessMathRightSubSuperscript(IOfficeMathFunctionBase officeMathFunctionBase)
	{
		m_laTexString += "{";
		ProcessMathFunctions((officeMathFunctionBase as IOfficeMathRightScript).Equation.Functions);
		m_laTexString += "}";
		m_laTexString += "_{";
		ProcessMathFunctions((officeMathFunctionBase as IOfficeMathRightScript).Subscript.Functions);
		m_laTexString += "}";
		m_laTexString += "^{";
		ProcessMathFunctions((officeMathFunctionBase as IOfficeMathRightScript).Superscript.Functions);
		m_laTexString += "}";
	}

	private void ProcessEquationArrayElement(IOfficeMathFunctionBase officeMathFunctionBase)
	{
		m_laTexString += "\\eqarray{";
		IOfficeMaths equation = (officeMathFunctionBase as IOfficeMathEquationArray).Equation;
		for (int i = 0; i < equation.Count; i++)
		{
			if (i > 0)
			{
				m_laTexString += "@";
			}
			IOfficeMath officeMath = equation[i];
			ProcessMathFunctions(officeMath.Functions);
		}
		m_laTexString += "}";
	}

	private void ProcessRunElement(IOfficeMathFunctionBase officeMathFunctionBase)
	{
		IOfficeMathRunElement officeMathRunElement = officeMathFunctionBase as IOfficeMathRunElement;
		string text = m_documentLaTeXConverter.GetText(officeMathRunElement);
		if (text == null)
		{
			return;
		}
		MathFontType font = officeMathRunElement.MathFormat.Font;
		MathStyleType style = officeMathRunElement.MathFormat.Style;
		bool hasNormalText = officeMathRunElement.MathFormat.HasNormalText;
		int num = 0;
		bool flag = true;
		bool flag2 = false;
		for (int i = 0; i < text.Length; i++)
		{
			string symbolLaTeX = GetSymbolLaTeX(text[i], font);
			if (symbolLaTeX != null)
			{
				if (num != 0)
				{
					for (int j = 0; j < num; j++)
					{
						m_laTexString += "}";
					}
					num = 0;
					flag = true;
				}
				m_laTexString += symbolLaTeX;
				flag2 = true;
				continue;
			}
			if (char.IsLetter(text[i]) && flag2)
			{
				m_laTexString += " ";
				flag2 = false;
			}
			if (flag)
			{
				num += ProcessMathStyleType(style);
				num += ProcessMathFontType(font);
				num += ProcessHasNormalText(hasNormalText);
				flag = false;
			}
			m_laTexString += text[i];
		}
		for (int k = 0; k < num; k++)
		{
			m_laTexString += "}";
		}
		num = 0;
	}

	private int ProcessMathFontType(MathFontType mathFontType)
	{
		switch (mathFontType)
		{
		case MathFontType.DoubleStruck:
			m_laTexString += "\\mathbb{";
			break;
		case MathFontType.Fraktur:
			m_laTexString += "\\mathfrak{";
			break;
		case MathFontType.Monospace:
			m_laTexString += "\\mathtt{";
			break;
		case MathFontType.SansSerif:
			m_laTexString += "\\mathsf{";
			break;
		case MathFontType.Script:
			m_laTexString += "\\mathscr{";
			break;
		default:
			return 0;
		}
		return 1;
	}

	private int ProcessMathStyleType(MathStyleType mathStyleType)
	{
		switch (mathStyleType)
		{
		case MathStyleType.BoldItalic:
			m_laTexString += "\\mathbit{";
			break;
		case MathStyleType.Bold:
			m_laTexString += "\\mathbf{";
			break;
		default:
			return 0;
		}
		return 1;
	}

	private int ProcessHasNormalText(bool hasNormalText)
	{
		if (hasNormalText)
		{
			m_laTexString += "\\mathrm{";
			return 1;
		}
		return 0;
	}

	private string GetSymbolLaTeX(char runElementText, MathFontType fontType)
	{
		string text = null;
		text = GetBasicMathLaTeX(runElementText);
		if (text == null)
		{
			text = GetGreekLetterLaTeX(runElementText);
		}
		if (text == null)
		{
			text = GetLetterLikeSymbolLaTeX(runElementText, fontType);
		}
		if (text == null)
		{
			text = GetCommonBinaryOperatorLaTeX(runElementText);
		}
		if (text == null)
		{
			text = GetCommonRelationalOperatorCharString(runElementText);
		}
		if (text == null)
		{
			text = GetAdvancedBinaryOperatorsCharString(runElementText);
		}
		if (text == null)
		{
			text = GetAdvancedRelationalOperatorCharString(runElementText);
		}
		if (text == null)
		{
			text = GetArrowLaTeX(runElementText);
		}
		if (text == null)
		{
			text = GetNegatedLaTeX(runElementText);
		}
		if (text == null)
		{
			text = GetGeometryLaTeX(runElementText);
		}
		return text;
	}

	private string GetBasicMathLaTeX(char runElementText)
	{
		return Convert.ToInt32(runElementText) switch
		{
			177 => "\\pm", 
			8734 => "\\infty", 
			8800 => "\\neq", 
			215 => "\\times", 
			247 => "\\div", 
			8733 => "\\propto", 
			8810 => "\\ll", 
			8811 => "\\gg", 
			8804 => "\\le", 
			8805 => "\\geq", 
			8723 => "\\mp", 
			8773 => "\\cong", 
			8776 => "\\approx", 
			8801 => "\\equiv", 
			8704 => "\\forall", 
			8705 => "\\complement", 
			8706 => "\\partial", 
			8730 => "\\sqrt", 
			8731 => "\\cbrt", 
			8732 => "\\qdrt", 
			8746 => "\\cup", 
			8745 => "\\cap", 
			8709 => "\\emptyset", 
			8457 => "\\degf", 
			8451 => "\\degc", 
			8711 => "\\nabla", 
			8707 => "\\exists", 
			8708 => "\\nexists", 
			8712 => "\\in", 
			8715 => "\\ni", 
			8592 => "\\gets", 
			8593 => "\\uparrow", 
			8594 => "\\rightarrow", 
			8595 => "\\downarrow", 
			8596 => "\\leftrightarrow", 
			8756 => "\\therefore", 
			172 => "\\lnot", 
			945 => "\\alpha", 
			946 => "\\beta", 
			947 => "\\gamma", 
			948 => "\\delta", 
			949 => "\\varepsilon", 
			1013 => "\\epsilon", 
			952 => "\\theta", 
			977 => "\\vartheta", 
			956 => "\\mu", 
			960 => "\\pi", 
			961 => "\\rho", 
			963 => "\\sigma", 
			964 => "\\tau", 
			966 => "\\varphi", 
			969 => "\\omega", 
			42 => "\\ast", 
			8729 => "\\bullet", 
			8942 => "\\vdots", 
			8943 => "\\cdots", 
			8945 => "\\ddots", 
			8501 => "\\aleph", 
			8502 => "\\beth", 
			8718 => "\\qed", 
			_ => null, 
		};
	}

	private string GetGreekLetterLaTeX(char runElementText)
	{
		return Convert.ToInt32(runElementText) switch
		{
			945 => "\\alpha", 
			946 => "\\beta", 
			947 => "\\gamma", 
			948 => "\\delta", 
			949 => "\\varepsilon", 
			1013 => "\\epsilon", 
			950 => "\\zeta", 
			951 => "\\eta", 
			952 => "\\theta", 
			977 => "\\vartheta", 
			953 => "\\iota", 
			954 => "\\kappa", 
			955 => "\\lambda", 
			956 => "\\mu", 
			957 => "\\nu", 
			958 => "\\xi", 
			960 => "\\pi", 
			982 => "\\varpi", 
			961 => "\\rho", 
			1009 => "\\varrho", 
			963 => "\\sigma", 
			962 => "\\varsigma", 
			964 => "\\tau", 
			965 => "\\upsilon", 
			966 => "\\varphi", 
			981 => "\\phi", 
			967 => "\\chi", 
			968 => "\\psi", 
			969 => "\\omega", 
			915 => "\\Gamma", 
			916 => "\\Delta", 
			920 => "\\Theta", 
			923 => "\\Lambda", 
			926 => "\\Xi", 
			928 => "\\Pi", 
			931 => "\\Sigma", 
			933 => "\\Upsilon", 
			934 => "\\Phi", 
			936 => "\\Psi", 
			937 => "\\Omega", 
			_ => null, 
		};
	}

	private string GetLetterLikeSymbolLaTeX(char runElementText, MathFontType fontType)
	{
		switch (Convert.ToInt32(runElementText))
		{
		case 8704:
			return "\\forall";
		case 8705:
			return "\\complement";
		case 8706:
			return "\\partial";
		case 8463:
			return "\\hbar";
		case 305:
			return "\\imath";
		case 1008:
			return "\\varkappa";
		case 108:
			if (fontType == MathFontType.Script)
			{
				return "\\ell";
			}
			return null;
		case 8472:
			return "\\wp";
		case 8707:
			return "\\exists";
		case 8708:
			return "\\nexists";
		case 8501:
			return "\\aleph";
		case 8502:
			return "\\beth";
		case 8503:
			return "\\gimel";
		case 8504:
			return "\\daleth";
		default:
			return null;
		}
	}

	private string GetCommonBinaryOperatorLaTeX(char runElementText)
	{
		return Convert.ToInt32(runElementText) switch
		{
			247 => "\\div", 
			215 => "\\times", 
			177 => "\\pm", 
			8723 => "\\mp", 
			8733 => "\\propto", 
			42 => "\\ast", 
			8728 => "\\circ", 
			8729 => "\\bullet", 
			8901 => "\\cdot", 
			8745 => "\\cap", 
			8746 => "\\cup", 
			8846 => "\\uplus", 
			8851 => "\\sqcap", 
			8852 => "\\sqcup", 
			8743 => "\\land", 
			8744 => "\\vee", 
			_ => null, 
		};
	}

	private string GetCommonRelationalOperatorCharString(char runElementText)
	{
		return Convert.ToInt32(runElementText) switch
		{
			8800 => "\\neq", 
			8804 => "\\le", 
			8805 => "\\geq", 
			8814 => "\\nless", 
			8816 => "\\nleq", 
			8815 => "\\ngt", 
			8817 => "\\ngeq", 
			8801 => "\\equiv", 
			8764 => "\\sim", 
			8771 => "\\simeq", 
			8776 => "\\approx", 
			8773 => "\\cong", 
			8802 => "\\nequiv", 
			8772 => "\\nsimeq", 
			8777 => "\\napprox", 
			8775 => "\\ncong", 
			8733 => "\\propto", 
			8810 => "\\ll", 
			8811 => "\\gg", 
			8712 => "\\in", 
			8715 => "\\ni", 
			8713 => "\\notin", 
			8834 => "\\subset", 
			8835 => "\\supset", 
			8838 => "\\subseteq", 
			8839 => "\\supseteq", 
			8826 => "\\prcue", 
			8827 => "\\succ", 
			8828 => "\\preccurlyeq", 
			8829 => "\\succcurlyeq", 
			8847 => "\\sqsubset", 
			8848 => "\\sqsupset", 
			8849 => "\\sqsubseteq", 
			8850 => "\\sqsupseteq", 
			8741 => "\\parallel", 
			8869 => "\\bot", 
			8866 => "\\vdash", 
			8867 => "\\dashv", 
			8904 => "\\bowtie", 
			8781 => "\\asymp", 
			_ => null, 
		};
	}

	private string GetAdvancedBinaryOperatorsCharString(char runElementText)
	{
		return Convert.ToInt32(runElementText) switch
		{
			8724 => "\\dotplus", 
			8760 => "\\dotminus", 
			8726 => "\\setminus", 
			8914 => "\\Cap", 
			8915 => "\\Cup", 
			8863 => "\\boxminus", 
			8864 => "\\boxtimes", 
			8865 => "\\boxdot", 
			8862 => "\\boxplus", 
			8903 => "\\divideontimes", 
			8905 => "\\ltimes", 
			8906 => "\\rtimes", 
			8907 => "\\leftthreetimes", 
			8908 => "\\rightthreetimes", 
			8911 => "\\curlywedge", 
			8910 => "\\curlyvee", 
			8861 => "\\odash", 
			8890 => "\\intercal", 
			8853 => "\\oplus", 
			8854 => "\\ominus", 
			8855 => "\\otimes", 
			8856 => "\\oslash", 
			8857 => "\\odot", 
			8859 => "\\oast", 
			8858 => "\\ocirc", 
			8224 => "\\dag", 
			8225 => "\\ddag", 
			8902 => "\\star", 
			8900 => "\\diamond", 
			8768 => "\\wr", 
			9651 => "\\triangle", 
			8896 => "\\bigwedge", 
			8897 => "\\bigvee", 
			10752 => "\\bigodot", 
			10754 => "\\bigotimes", 
			10753 => "\\bigoplus", 
			10757 => "\\bigsqcap", 
			10758 => "\\bigsqcup", 
			10756 => "\\biguplus", 
			10755 => "\\bigudot", 
			_ => null, 
		};
	}

	private string GetAdvancedRelationalOperatorCharString(char runElementText)
	{
		return Convert.ToInt32(runElementText) switch
		{
			8756 => "\\therefore", 
			8757 => "\\because", 
			8920 => "\\lll", 
			8921 => "\\ggg", 
			8806 => "\\leqq", 
			8807 => "\\geqq", 
			8818 => "\\lesssim", 
			8819 => "\\gtrsim", 
			8918 => "\\lessdot", 
			8919 => "\\gtrdot", 
			8822 => "\\lessgtr", 
			8922 => "\\lesseqgtr", 
			8823 => "\\gtrless", 
			8923 => "\\gtreqless", 
			8785 => "\\Doteq", 
			8786 => "\\fallingdotseq", 
			8787 => "\\risingdotseq", 
			8765 => "\\backsim", 
			8778 => "\\approxeq", 
			8909 => "\\backsimeq", 
			8828 => "\\preccurlyeq", 
			8829 => "\\succcurlyeq", 
			8926 => "\\curlyeqprec", 
			8927 => "\\curlyeqsucc", 
			8830 => "\\precsim", 
			8831 => "\\succsim", 
			8924 => "\\eqless", 
			8925 => "\\eqgtr", 
			8838 => "\\subseteq", 
			8839 => "\\supseteq", 
			8882 => "\\vartriangleleft", 
			8883 => "\\vartriangleright", 
			8884 => "\\trianglelefteq", 
			8885 => "\\trianglerighteq", 
			8872 => "\\models", 
			8912 => "\\Subset", 
			8913 => "\\Supset", 
			8847 => "\\sqsubset", 
			8848 => "\\sqsupset", 
			8873 => "\\Vdash", 
			8874 => "\\Vvdash", 
			8790 => "\\eqcirc", 
			8791 => "\\circeq", 
			8796 => "\\Deltaeq", 
			8783 => "\\bumpeq", 
			8782 => "\\Bumpeq", 
			8733 => "\\propto", 
			8812 => "\\between", 
			8916 => "\\pitchfork", 
			8784 => "\\doteq", 
			8904 => "\\bowtie", 
			_ => null, 
		};
	}

	private string GetArrowLaTeX(char runElementText)
	{
		return Convert.ToInt32(runElementText) switch
		{
			8592 => "\\get", 
			8594 => "\\rightarrow", 
			8593 => "\\uparrow", 
			8595 => "\\downarrow", 
			8596 => "\\leftrightarrow", 
			8597 => "\\updownarrow", 
			8656 => "\\Leftarrow", 
			8658 => "\\Rightarrow", 
			8657 => "\\Uparrow", 
			8659 => "\\Downarrow", 
			8660 => "\\Leftrightarrow", 
			8661 => "\\Updownarrow", 
			10229 => "\\longleftarrow", 
			10230 => "\\longrightarrow", 
			10231 => "\\longleftrightarrow", 
			10232 => "\\Longleftarrow", 
			10233 => "\\Longrightarrow", 
			10234 => "\\Longleftrightarrow", 
			8599 => "\\nearrow", 
			8598 => "\\nwarrow", 
			8600 => "\\searrow", 
			8601 => "\\swarrow", 
			8602 => "\\nleftarrow", 
			8603 => "\\nrightarrow", 
			8622 => "\\nleftrightarrow", 
			8653 => "\\nLeftarrow", 
			8655 => "\\nRightarrow", 
			8654 => "\\nLeftrightarrow", 
			8672 => "\\dashleftarrow", 
			8674 => "\\dashrightarrow", 
			8612 => "\\mapstoleft", 
			8614 => "\\mapsto", 
			10235 => "\\longmapstoleft", 
			10236 => "\\longmapsto", 
			8617 => "\\hookleftarrow", 
			8618 => "\\hookrightarrow", 
			8636 => "\\leftharpoonup", 
			8637 => "\\leftharpoondown", 
			8640 => "\\rightharpoonup", 
			8641 => "\\rightharpoondown", 
			8639 => "\\upharpoonleft", 
			8638 => "\\upharpoonright", 
			8643 => "\\downharpoonleft", 
			8642 => "\\downharpoonright", 
			8651 => "\\leftrightharpoons", 
			8652 => "\\rlhar", 
			8647 => "\\leftleftarrows", 
			8649 => "\\rightrightarrows", 
			8648 => "\\upuparrows", 
			8650 => "\\downdownarrows", 
			8646 => "\\leftrightarrows", 
			8644 => "\\rightleftarrows", 
			8619 => "\\looparrowleft", 
			8620 => "\\looparrowright", 
			8610 => "\\leftarrowtail", 
			8611 => "\\rightarrowtail", 
			8624 => "\\Lsh", 
			8625 => "\\Rsh", 
			8626 => "\\ldsh", 
			8627 => "\\rdsh", 
			8666 => "\\Lleftarrow", 
			8667 => "\\Rrightarrow", 
			8606 => "\\twoheadleftarrow", 
			8608 => "\\twoheadrightarrow", 
			8630 => "\\curvearrowleft", 
			8631 => "\\curvearrowright", 
			8634 => "\\circlearrowleft", 
			8635 => "\\circlearrowright", 
			8888 => "\\multimap", 
			8621 => "\\leftrightwavearrow", 
			8604 => "\\leftwavearrow", 
			8605 => "\\rightwavearrow", 
			8668 => "\\leftsquigarrow", 
			_ => null, 
		};
	}

	private string GetNegatedLaTeX(char runElementText)
	{
		return Convert.ToInt32(runElementText) switch
		{
			8800 => "\\neq", 
			8814 => "\\nless", 
			8815 => "\\ngt", 
			8816 => "\\nleq", 
			8817 => "\\ngeq", 
			8802 => "\\nequiv", 
			8769 => "\\nsim", 
			8772 => "\\nsimeq", 
			8777 => "\\napprox", 
			8775 => "\\ncong", 
			8813 => "\\nasymp", 
			8808 => "\\lneqq", 
			8809 => "\\gneqq", 
			8832 => "\\nprec", 
			8833 => "\\nsucc", 
			8928 => "\\npreccurlyeq", 
			8929 => "\\nsucccurlyeq", 
			8713 => "\\notin", 
			8716 => "\\notni", 
			8836 => "\\nsub", 
			8837 => "\\nsup", 
			8840 => "\\nsubseteq", 
			8841 => "\\nsupseteq", 
			8843 => "\\supsetneq", 
			8930 => "\\nsqsubseteq", 
			8931 => "\\nsqsupseteq", 
			8934 => "\\lnsim", 
			8935 => "\\gnsim", 
			8936 => "\\precnsim", 
			8937 => "\\succnsim", 
			8938 => "\\ntriangleleft", 
			8939 => "\\ntriangleright", 
			8940 => "\\ntrianglelefteq", 
			8941 => "\\ntrianglerighteq", 
			8740 => "\\nmid", 
			8742 => "\\nparallel", 
			8876 => "\\nvdash", 
			8877 => "\\nvDash", 
			8878 => "\\nVdash", 
			8879 => "\\nVDash", 
			8708 => "\\nexists", 
			_ => null, 
		};
	}

	private string GetGeometryLaTeX(char runElementText)
	{
		return Convert.ToInt32(runElementText) switch
		{
			8735 => "\\rightangle", 
			8736 => "\\angle", 
			8737 => "\\angmsd", 
			8738 => "\\angsph", 
			8894 => "\\angrtvb", 
			8895 => "\\righttriangle", 
			8917 => "\\epar", 
			8869 => "\\bot", 
			8740 => "\\nmid", 
			8741 => "\\parallel", 
			8742 => "\\nparallel", 
			8758 => "\\colon", 
			8759 => "\\Colon", 
			8756 => "\\therefore", 
			8757 => "\\because", 
			8718 => "\\qed", 
			_ => null, 
		};
	}
}
