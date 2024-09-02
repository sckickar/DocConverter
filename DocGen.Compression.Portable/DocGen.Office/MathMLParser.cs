using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml;

namespace DocGen.Office;

internal class MathMLParser
{
	private DocumentParser m_documentParser;

	private Regex m_isFloatValue = new Regex("^.*\\d+(\\.*\\d+)*$");

	private Regex m_hasAlphabet = new Regex("^[^ A-Za-z_@/#&+-]*$");

	internal MathMLParser()
	{
	}

	internal void ParseMathPara(XmlReader reader, IOfficeMathParagraph mathPara, DocumentParser documentParser)
	{
		m_documentParser = documentParser;
		while (reader.NodeType != XmlNodeType.Element)
		{
			reader.Read();
		}
		if (reader.IsEmptyElement)
		{
			return;
		}
		string localName = reader.LocalName;
		reader.Read();
		if (localName == reader.LocalName && reader.NodeType == XmlNodeType.EndElement)
		{
			return;
		}
		SkipWhitespaces(reader);
		while (reader.LocalName != localName)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				string localName2 = reader.LocalName;
				if (!(localName2 == "oMath"))
				{
					if (localName2 == "oMathParaPr")
					{
						ParseMathParaProperties(reader, mathPara);
					}
				}
				else
				{
					OfficeMath officeMath = mathPara.Maths.Add(mathPara.Maths.Count) as OfficeMath;
					ParseMath(reader, officeMath);
				}
				reader.Read();
			}
			else
			{
				reader.Read();
			}
		}
	}

	internal void ParseMath(XmlReader reader, OfficeMath officeMath, DocumentParser documentParser)
	{
		m_documentParser = documentParser;
		ParseMath(reader, officeMath);
	}

	internal void ParseMath(XmlReader reader, OfficeMath officeMath)
	{
		while (reader.NodeType != XmlNodeType.Element)
		{
			reader.Read();
		}
		if (reader.IsEmptyElement)
		{
			return;
		}
		string localName = reader.LocalName;
		reader.Read();
		if (localName == reader.LocalName && reader.NodeType == XmlNodeType.EndElement)
		{
			return;
		}
		SkipWhitespaces(reader);
		while (reader.LocalName != localName)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				switch (reader.LocalName)
				{
				case "f":
				{
					OfficeMathFraction mathFraction = officeMath.Functions.Add(officeMath.Functions.Count, MathFunctionType.Fraction) as OfficeMathFraction;
					ParseMathFraction(reader, mathFraction);
					break;
				}
				case "func":
				{
					OfficeMathFunction mathFunc = officeMath.Functions.Add(officeMath.Functions.Count, MathFunctionType.Function) as OfficeMathFunction;
					ParseMathFunc(reader, mathFunc);
					break;
				}
				case "r":
				{
					OfficeMathRunElement mathParaItem = officeMath.Functions.Add(officeMath.Functions.Count, MathFunctionType.RunElement) as OfficeMathRunElement;
					m_documentParser.ParseMathRun(reader, mathParaItem);
					break;
				}
				case "box":
				{
					OfficeMathBox mathBox = officeMath.Functions.Add(officeMath.Functions.Count, MathFunctionType.Box) as OfficeMathBox;
					ParseMathBox(reader, mathBox);
					break;
				}
				case "borderBox":
				{
					OfficeMathBorderBox mathBorderBox = officeMath.Functions.Add(officeMath.Functions.Count, MathFunctionType.BorderBox) as OfficeMathBorderBox;
					ParseMathBorderBox(reader, mathBorderBox);
					break;
				}
				case "d":
				{
					OfficeMathDelimiter mathDelimiter = officeMath.Functions.Add(officeMath.Functions.Count, MathFunctionType.Delimiter) as OfficeMathDelimiter;
					ParseMathDelimiter(reader, mathDelimiter);
					break;
				}
				case "acc":
				{
					OfficeMathAccent mathAccent = officeMath.Functions.Add(officeMath.Functions.Count, MathFunctionType.Accent) as OfficeMathAccent;
					ParseMathAccent(reader, mathAccent);
					break;
				}
				case "bar":
				{
					OfficeMathBar mathBar = officeMath.Functions.Add(officeMath.Functions.Count, MathFunctionType.Bar) as OfficeMathBar;
					ParseMathBar(reader, mathBar);
					break;
				}
				case "groupChr":
				{
					OfficeMathGroupCharacter mathGroupChar = officeMath.Functions.Add(officeMath.Functions.Count, MathFunctionType.GroupCharacter) as OfficeMathGroupCharacter;
					ParseMathGroupChar(reader, mathGroupChar);
					break;
				}
				case "eqArr":
				{
					OfficeMathEquationArray mathEqArray = officeMath.Functions.Add(officeMath.Functions.Count, MathFunctionType.EquationArray) as OfficeMathEquationArray;
					ParseMathEqArray(reader, mathEqArray);
					break;
				}
				case "sSub":
				{
					OfficeMathScript officeMathScript2 = officeMath.Functions.Add(officeMath.Functions.Count, MathFunctionType.SubSuperscript) as OfficeMathScript;
					officeMathScript2.ScriptType = MathScriptType.Subscript;
					ParseMathScript(reader, officeMathScript2);
					break;
				}
				case "sSup":
				{
					OfficeMathScript officeMathScript = officeMath.Functions.Add(officeMath.Functions.Count, MathFunctionType.SubSuperscript) as OfficeMathScript;
					officeMathScript.ScriptType = MathScriptType.Superscript;
					ParseMathScript(reader, officeMathScript);
					break;
				}
				case "sPre":
				{
					OfficeMathLeftScript mathLeftScript = officeMath.Functions.Add(officeMath.Functions.Count, MathFunctionType.LeftSubSuperscript) as OfficeMathLeftScript;
					ParseMathLeftScript(reader, mathLeftScript);
					break;
				}
				case "sSubSup":
				{
					OfficeMathRightScript mathRightScript = officeMath.Functions.Add(officeMath.Functions.Count, MathFunctionType.RightSubSuperscript) as OfficeMathRightScript;
					ParseMathRightScript(reader, mathRightScript);
					break;
				}
				case "rad":
				{
					OfficeMathRadical mathRadical = officeMath.Functions.Add(officeMath.Functions.Count, MathFunctionType.Radical) as OfficeMathRadical;
					ParseMathRadical(reader, mathRadical);
					break;
				}
				case "nary":
				{
					OfficeMathNArray mathNAry = officeMath.Functions.Add(officeMath.Functions.Count, MathFunctionType.NArray) as OfficeMathNArray;
					ParseMathNAry(reader, mathNAry);
					break;
				}
				case "m":
				{
					OfficeMathMatrix officeMathMatrix = officeMath.Functions.Add(officeMath.Functions.Count, MathFunctionType.Matrix) as OfficeMathMatrix;
					ParseMathMatrix(reader, officeMathMatrix);
					if (officeMathMatrix != null)
					{
						officeMathMatrix.UpdateColumns();
						officeMathMatrix.ApplyColumnProperties();
					}
					break;
				}
				case "phant":
				{
					OfficeMathPhantom mathPhantom = officeMath.Functions.Add(officeMath.Functions.Count, MathFunctionType.Phantom) as OfficeMathPhantom;
					ParseMathPhantom(reader, mathPhantom);
					break;
				}
				case "limLow":
				{
					OfficeMathLimit officeMathLimit2 = officeMath.Functions.Add(officeMath.Functions.Count, MathFunctionType.Limit) as OfficeMathLimit;
					officeMathLimit2.LimitType = MathLimitType.LowerLimit;
					ParseMathLimit(reader, officeMathLimit2);
					break;
				}
				case "limUpp":
				{
					OfficeMathLimit officeMathLimit = officeMath.Functions.Add(officeMath.Functions.Count, MathFunctionType.Limit) as OfficeMathLimit;
					officeMathLimit.LimitType = MathLimitType.UpperLimit;
					ParseMathLimit(reader, officeMathLimit);
					break;
				}
				case "argPr":
					ParseMathArgumentProperties(reader, officeMath);
					break;
				}
				reader.Read();
			}
			else
			{
				reader.Read();
			}
		}
	}

	private void ParseMathArgumentProperties(XmlReader reader, OfficeMath officeMath)
	{
		while (reader.NodeType != XmlNodeType.Element)
		{
			reader.Read();
		}
		if (reader.IsEmptyElement)
		{
			return;
		}
		string localName = reader.LocalName;
		reader.Read();
		if (localName == reader.LocalName && reader.NodeType == XmlNodeType.EndElement)
		{
			return;
		}
		SkipWhitespaces(reader);
		while (reader.LocalName != localName)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				if (reader.LocalName == "argSz")
				{
					string attribute = reader.GetAttribute("val", reader.NamespaceURI);
					if (!string.IsNullOrEmpty(attribute))
					{
						officeMath.ArgumentSize = (int)GetNumericValue(attribute);
					}
				}
				reader.Read();
			}
			else
			{
				reader.Read();
			}
		}
	}

	internal void ParseMathProperties(XmlReader reader, OfficeMathProperties mathProperties)
	{
		while (reader.NodeType != XmlNodeType.Element)
		{
			reader.Read();
		}
		if (reader.IsEmptyElement)
		{
			return;
		}
		string localName = reader.LocalName;
		reader.Read();
		if (localName == reader.LocalName && reader.NodeType == XmlNodeType.EndElement)
		{
			return;
		}
		SkipWhitespaces(reader);
		while (reader.LocalName != localName)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				switch (reader.LocalName)
				{
				case "mathFont":
				{
					string attribute4 = reader.GetAttribute("val", reader.NamespaceURI);
					if (!string.IsNullOrEmpty(attribute4))
					{
						mathProperties.MathFont = attribute4;
					}
					break;
				}
				case "brkBin":
					ParseBreakOnBinaryOperator(reader, mathProperties);
					break;
				case "brkBinSub":
					ParseBreakOnSubtractOperator(reader, mathProperties);
					break;
				case "smallFrac":
					mathProperties.SmallFraction = GetBooleanValue(reader);
					break;
				case "dispDef":
					mathProperties.DisplayMathDefaults = GetBooleanValue(reader);
					break;
				case "lMargin":
				{
					string attribute3 = reader.GetAttribute("val", reader.NamespaceURI);
					if (!string.IsNullOrEmpty(attribute3))
					{
						mathProperties.LeftMargin = (int)Math.Round(GetFloatValue(attribute3, reader.LocalName));
					}
					break;
				}
				case "rMargin":
				{
					string attribute2 = reader.GetAttribute("val", reader.NamespaceURI);
					if (!string.IsNullOrEmpty(attribute2))
					{
						mathProperties.RightMargin = (int)Math.Round(GetFloatValue(attribute2, reader.LocalName));
					}
					break;
				}
				case "defJc":
					ParseDefaultJustification(reader, mathProperties);
					break;
				case "wrapIndent":
				{
					string attribute = reader.GetAttribute("val", reader.NamespaceURI);
					if (!string.IsNullOrEmpty(attribute))
					{
						mathProperties.WrapIndent = (int)Math.Round(GetFloatValue(attribute, reader.LocalName));
					}
					break;
				}
				case "wrapRight":
					mathProperties.WrapRight = GetBooleanValue(reader);
					break;
				case "intLim":
					ParseLimitLocationType(reader, mathProperties);
					break;
				case "naryLim":
					ParseNArtLimitLocationType(reader, mathProperties);
					break;
				}
				reader.Read();
			}
			else
			{
				reader.Read();
			}
		}
	}

	private void ParseNArtLimitLocationType(XmlReader reader, OfficeMathProperties mathProperties)
	{
		string attribute = reader.GetAttribute("val", reader.NamespaceURI);
		if (!(attribute == "subSup"))
		{
			if (attribute == "undOvr")
			{
				mathProperties.NAryLimitLocation = LimitLocationType.UnderOver;
			}
		}
		else
		{
			mathProperties.NAryLimitLocation = LimitLocationType.SubSuperscript;
		}
	}

	private void ParseLimitLocationType(XmlReader reader, OfficeMathProperties mathProperties)
	{
		string attribute = reader.GetAttribute("val", reader.NamespaceURI);
		if (!(attribute == "undOvr"))
		{
			if (attribute == "subSup")
			{
				mathProperties.IntegralLimitLocations = LimitLocationType.SubSuperscript;
			}
		}
		else
		{
			mathProperties.IntegralLimitLocations = LimitLocationType.UnderOver;
		}
	}

	private void ParseDefaultJustification(XmlReader reader, OfficeMathProperties mathProperties)
	{
		switch (reader.GetAttribute("val", reader.NamespaceURI))
		{
		case "center":
			mathProperties.DefaultJustification = MathJustification.Center;
			break;
		case "left":
			mathProperties.DefaultJustification = MathJustification.Left;
			break;
		case "right":
			mathProperties.DefaultJustification = MathJustification.Right;
			break;
		case "centerGroup":
			mathProperties.DefaultJustification = MathJustification.CenterGroup;
			break;
		}
	}

	private void ParseBreakOnSubtractOperator(XmlReader reader, OfficeMathProperties mathProperties)
	{
		switch (reader.GetAttribute("val", reader.NamespaceURI))
		{
		case "+-":
			mathProperties.BreakOnBinarySubtraction = BreakOnBinarySubtractionType.PlusMinus;
			break;
		case "-+":
			mathProperties.BreakOnBinarySubtraction = BreakOnBinarySubtractionType.MinusPlus;
			break;
		case "--":
			mathProperties.BreakOnBinarySubtraction = BreakOnBinarySubtractionType.MinusMinus;
			break;
		}
	}

	private void ParseBreakOnBinaryOperator(XmlReader reader, OfficeMathProperties mathProperties)
	{
		switch (reader.GetAttribute("val", reader.NamespaceURI))
		{
		case "after":
			mathProperties.BreakOnBinaryOperators = BreakOnBinaryOperatorsType.After;
			break;
		case "repeat":
			mathProperties.BreakOnBinaryOperators = BreakOnBinaryOperatorsType.Repeat;
			break;
		case "before":
			mathProperties.BreakOnBinaryOperators = BreakOnBinaryOperatorsType.Before;
			break;
		}
	}

	private void ParseMathLimit(XmlReader reader, OfficeMathLimit mathLimit)
	{
		while (reader.NodeType != XmlNodeType.Element)
		{
			reader.Read();
		}
		if (reader.IsEmptyElement)
		{
			return;
		}
		string localName = reader.LocalName;
		reader.Read();
		if (localName == reader.LocalName && reader.NodeType == XmlNodeType.EndElement)
		{
			return;
		}
		SkipWhitespaces(reader);
		while (reader.LocalName != localName)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				switch (reader.LocalName)
				{
				case "limLowPr":
				case "limUppPr":
					ParseMathLimitProperties(reader, mathLimit);
					break;
				case "e":
					ParseMath(reader, mathLimit.Equation as OfficeMath);
					break;
				case "lim":
					ParseMath(reader, mathLimit.Limit as OfficeMath);
					break;
				}
				reader.Read();
			}
			else
			{
				reader.Read();
			}
		}
	}

	private void ParseMathLimitProperties(XmlReader reader, OfficeMathLimit mathLimit)
	{
		while (reader.NodeType != XmlNodeType.Element)
		{
			reader.Read();
		}
		if (reader.IsEmptyElement)
		{
			return;
		}
		string localName = reader.LocalName;
		reader.Read();
		if (localName == reader.LocalName && reader.NodeType == XmlNodeType.EndElement)
		{
			return;
		}
		SkipWhitespaces(reader);
		while (!(reader.LocalName == localName) || reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				if (reader.LocalName == "ctrlPr")
				{
					mathLimit.ControlProperties = ParseMathControlProperties(reader, mathLimit, mathLimit.ControlProperties);
				}
				reader.Read();
			}
			else
			{
				reader.Read();
			}
		}
	}

	private void ParseMathPhantom(XmlReader reader, OfficeMathPhantom mathPhantom)
	{
		while (reader.NodeType != XmlNodeType.Element)
		{
			reader.Read();
		}
		if (reader.IsEmptyElement)
		{
			return;
		}
		string localName = reader.LocalName;
		reader.Read();
		if (localName == reader.LocalName && reader.NodeType == XmlNodeType.EndElement)
		{
			return;
		}
		SkipWhitespaces(reader);
		while (reader.LocalName != localName)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				string localName2 = reader.LocalName;
				if (!(localName2 == "phantPr"))
				{
					if (localName2 == "e")
					{
						ParseMath(reader, mathPhantom.Equation as OfficeMath);
					}
				}
				else
				{
					ParseMathPhantomProperties(reader, mathPhantom);
				}
				reader.Read();
			}
			else
			{
				reader.Read();
			}
		}
	}

	private void ParseMathPhantomProperties(XmlReader reader, OfficeMathPhantom mathPhantom)
	{
		while (reader.NodeType != XmlNodeType.Element)
		{
			reader.Read();
		}
		if (reader.IsEmptyElement)
		{
			return;
		}
		string localName = reader.LocalName;
		reader.Read();
		if (localName == reader.LocalName && reader.NodeType == XmlNodeType.EndElement)
		{
			return;
		}
		SkipWhitespaces(reader);
		while (!(reader.LocalName == localName) || reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				switch (reader.LocalName)
				{
				case "transp":
					mathPhantom.Transparent = GetBooleanValue(reader);
					break;
				case "zeroDesc":
					mathPhantom.ZeroDescent = GetBooleanValue(reader);
					break;
				case "zeroAsc":
					mathPhantom.ZeroAscent = GetBooleanValue(reader);
					break;
				case "zeroWid":
					mathPhantom.ZeroWidth = GetBooleanValue(reader);
					break;
				case "show":
					mathPhantom.Show = GetBooleanValue(reader);
					break;
				case "ctrlPr":
					mathPhantom.ControlProperties = ParseMathControlProperties(reader, mathPhantom, mathPhantom.ControlProperties);
					break;
				}
				reader.Read();
			}
			else
			{
				reader.Read();
			}
		}
	}

	private void ParseMathMatrix(XmlReader reader, OfficeMathMatrix mathMatrix)
	{
		while (reader.NodeType != XmlNodeType.Element)
		{
			reader.Read();
		}
		if (reader.IsEmptyElement)
		{
			return;
		}
		string localName = reader.LocalName;
		reader.Read();
		if (localName == reader.LocalName && reader.NodeType == XmlNodeType.EndElement)
		{
			return;
		}
		SkipWhitespaces(reader);
		while (reader.LocalName != localName)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				string localName2 = reader.LocalName;
				if (!(localName2 == "mPr"))
				{
					if (localName2 == "mr")
					{
						OfficeMathMatrixRow mathMatrixRow = mathMatrix.Rows.Add(mathMatrix.Rows.Count) as OfficeMathMatrixRow;
						ParseMathMatrixRow(reader, mathMatrixRow);
					}
				}
				else
				{
					ParseMathMatrixProperties(reader, mathMatrix);
				}
				reader.Read();
			}
			else
			{
				reader.Read();
			}
		}
	}

	private void ParseMathMatrixProperties(XmlReader reader, OfficeMathMatrix mathMatrix)
	{
		string value = string.Empty;
		string value2 = string.Empty;
		while (reader.NodeType != XmlNodeType.Element)
		{
			reader.Read();
		}
		if (reader.IsEmptyElement)
		{
			return;
		}
		string localName = reader.LocalName;
		reader.Read();
		if (localName == reader.LocalName && reader.NodeType == XmlNodeType.EndElement)
		{
			return;
		}
		SkipWhitespaces(reader);
		while (!(reader.LocalName == localName) || reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				switch (reader.LocalName)
				{
				case "baseJc":
					ParseMathMatrixJustification(reader, mathMatrix);
					break;
				case "plcHide":
					mathMatrix.HidePlaceHolders = GetBooleanValue(reader);
					break;
				case "rSpRule":
					mathMatrix.RowSpacingRule = ParseSpacingRule(reader);
					break;
				case "cSp":
				{
					string attribute = reader.GetAttribute("val", reader.NamespaceURI);
					if (!string.IsNullOrEmpty(attribute))
					{
						float num = (float)Math.Round(GetFloatValue(attribute, reader.LocalName), 2);
						mathMatrix.SetPropertyValue(34, num);
					}
					break;
				}
				case "cGpRule":
					mathMatrix.ColumnSpacingRule = ParseSpacingRule(reader);
					break;
				case "rSp":
					value = reader.GetAttribute("val", reader.NamespaceURI);
					break;
				case "cGp":
					value2 = reader.GetAttribute("val", reader.NamespaceURI);
					break;
				case "mcs":
					ParserMathColumns(reader, mathMatrix);
					break;
				case "ctrlPr":
					mathMatrix.ControlProperties = ParseMathControlProperties(reader, mathMatrix, mathMatrix.ControlProperties);
					break;
				}
				reader.Read();
			}
			else
			{
				reader.Read();
			}
		}
		if (!string.IsNullOrEmpty(value) && (mathMatrix.RowSpacingRule == SpacingRule.Exactly || mathMatrix.RowSpacingRule == SpacingRule.Multiple))
		{
			float num2 = (float)Math.Round(GetSpacingValue(value, "rSp", mathMatrix.RowSpacingRule), 2);
			mathMatrix.SetPropertyValue(27, num2);
		}
		if (!string.IsNullOrEmpty(value2) && (mathMatrix.ColumnSpacingRule == SpacingRule.Exactly || mathMatrix.ColumnSpacingRule == SpacingRule.Multiple))
		{
			float num3 = 0f;
			num3 = ((mathMatrix.ColumnSpacingRule != SpacingRule.Multiple) ? ((float)Math.Round(GetSpacingValue(value2, "cGp", mathMatrix.ColumnSpacingRule), 2)) : ((float)Math.Floor(GetSpacingValue(value2, "cGp", mathMatrix.ColumnSpacingRule))));
			mathMatrix.SetPropertyValue(36, num3);
		}
	}

	private void ParserMathColumns(XmlReader reader, OfficeMathMatrix mathMatrix)
	{
		while (reader.NodeType != XmlNodeType.Element)
		{
			reader.Read();
		}
		if (reader.IsEmptyElement)
		{
			return;
		}
		string localName = reader.LocalName;
		reader.Read();
		if (localName == reader.LocalName && reader.NodeType == XmlNodeType.EndElement)
		{
			return;
		}
		SkipWhitespaces(reader);
		while (reader.LocalName != localName)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				if (reader.LocalName == "mc")
				{
					MatrixColumnProperties matrixColumnProperties = new MatrixColumnProperties(mathMatrix);
					ParseMathMatrixColumn(reader, matrixColumnProperties);
					mathMatrix.ColumnProperties.Add(matrixColumnProperties);
				}
				reader.Read();
			}
			else
			{
				reader.Read();
			}
		}
	}

	private void ParseMathMatrixColumn(XmlReader reader, MatrixColumnProperties mathMatrixColumnProperties)
	{
		while (reader.NodeType != XmlNodeType.Element)
		{
			reader.Read();
		}
		if (reader.IsEmptyElement)
		{
			return;
		}
		string localName = reader.LocalName;
		reader.Read();
		if (localName == reader.LocalName && reader.NodeType == XmlNodeType.EndElement)
		{
			return;
		}
		SkipWhitespaces(reader);
		while (reader.LocalName != localName)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				if (reader.LocalName == "mcPr")
				{
					ParseMathMatrixColumnProperties(reader, mathMatrixColumnProperties);
				}
				reader.Read();
			}
			else
			{
				reader.Read();
			}
		}
	}

	private void ParseMathMatrixColumnProperties(XmlReader reader, MatrixColumnProperties mathMatrixColumnProperties)
	{
		while (reader.NodeType != XmlNodeType.Element)
		{
			reader.Read();
		}
		if (reader.IsEmptyElement)
		{
			return;
		}
		string localName = reader.LocalName;
		reader.Read();
		if (localName == reader.LocalName && reader.NodeType == XmlNodeType.EndElement)
		{
			return;
		}
		SkipWhitespaces(reader);
		while (!(reader.LocalName == localName) || reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				string localName2 = reader.LocalName;
				if (!(localName2 == "mcJc"))
				{
					if (localName2 == "count")
					{
						string attribute = reader.GetAttribute("val", reader.NamespaceURI);
						if (!string.IsNullOrEmpty(attribute))
						{
							mathMatrixColumnProperties.Count = (int)GetNumericValue(attribute);
						}
					}
				}
				else
				{
					ParseMathMatrixColumnJustification(reader, mathMatrixColumnProperties);
				}
				reader.Read();
			}
			else
			{
				reader.Read();
			}
		}
	}

	private void ParseMathMatrixColumnJustification(XmlReader reader, MatrixColumnProperties mathMatrixColumnProperties)
	{
		switch (reader.GetAttribute("val", reader.NamespaceURI))
		{
		case "left":
			mathMatrixColumnProperties.Alignment = MathHorizontalAlignment.Left;
			break;
		case "right":
			mathMatrixColumnProperties.Alignment = MathHorizontalAlignment.Right;
			break;
		case "center":
			mathMatrixColumnProperties.Alignment = MathHorizontalAlignment.Center;
			break;
		}
	}

	private SpacingRule ParseSpacingRule(XmlReader reader)
	{
		return reader.GetAttribute("val", reader.NamespaceURI) switch
		{
			"0" => SpacingRule.Single, 
			"1" => SpacingRule.OneAndHalf, 
			"2" => SpacingRule.Double, 
			"3" => SpacingRule.Exactly, 
			"4" => SpacingRule.Multiple, 
			_ => SpacingRule.Single, 
		};
	}

	private void ParseMathMatrixJustification(XmlReader reader, OfficeMathMatrix mathMatrix)
	{
		switch (reader.GetAttribute("val", reader.NamespaceURI).ToLower())
		{
		case "top":
			mathMatrix.VerticalAlignment = MathVerticalAlignment.Top;
			break;
		case "bottom":
			mathMatrix.VerticalAlignment = MathVerticalAlignment.Bottom;
			break;
		case "center":
			mathMatrix.VerticalAlignment = MathVerticalAlignment.Center;
			break;
		}
	}

	private void ParseMathMatrixRow(XmlReader reader, OfficeMathMatrixRow mathMatrixRow)
	{
		while (reader.NodeType != XmlNodeType.Element)
		{
			reader.Read();
		}
		if (reader.IsEmptyElement)
		{
			return;
		}
		string localName = reader.LocalName;
		reader.Read();
		if (localName == reader.LocalName && reader.NodeType == XmlNodeType.EndElement)
		{
			return;
		}
		SkipWhitespaces(reader);
		while (reader.LocalName != localName)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				if (reader.LocalName == "e")
				{
					OfficeMath officeMath = new OfficeMath(mathMatrixRow);
					ParseMath(reader, officeMath);
					mathMatrixRow.m_args.InnerList.Add(officeMath);
				}
				reader.Read();
			}
			else
			{
				reader.Read();
			}
		}
	}

	private void ParseMathNAry(XmlReader reader, OfficeMathNArray mathNAry)
	{
		while (reader.NodeType != XmlNodeType.Element)
		{
			reader.Read();
		}
		if (reader.IsEmptyElement)
		{
			return;
		}
		string localName = reader.LocalName;
		reader.Read();
		if (localName == reader.LocalName && reader.NodeType == XmlNodeType.EndElement)
		{
			return;
		}
		SkipWhitespaces(reader);
		while (reader.LocalName != localName)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				switch (reader.LocalName)
				{
				case "naryPr":
					ParseMathNAryProperties(reader, mathNAry);
					break;
				case "e":
					ParseMath(reader, mathNAry.Equation as OfficeMath);
					break;
				case "sub":
					ParseMath(reader, mathNAry.Subscript as OfficeMath);
					break;
				case "sup":
					ParseMath(reader, mathNAry.Superscript as OfficeMath);
					break;
				}
				reader.Read();
			}
			else
			{
				reader.Read();
			}
		}
	}

	private void ParseMathNAryProperties(XmlReader reader, OfficeMathNArray mathNAry)
	{
		while (reader.NodeType != XmlNodeType.Element)
		{
			reader.Read();
		}
		if (reader.IsEmptyElement)
		{
			return;
		}
		string localName = reader.LocalName;
		reader.Read();
		if (localName == reader.LocalName && reader.NodeType == XmlNodeType.EndElement)
		{
			return;
		}
		SkipWhitespaces(reader);
		while (!(reader.LocalName == localName) || reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				switch (reader.LocalName)
				{
				case "chr":
				{
					string attribute2 = reader.GetAttribute("val", reader.NamespaceURI);
					if (!string.IsNullOrEmpty(attribute2))
					{
						mathNAry.NArrayCharacter = attribute2;
					}
					break;
				}
				case "limLoc":
				{
					string attribute = reader.GetAttribute("val", reader.NamespaceURI);
					if (!string.IsNullOrEmpty(attribute))
					{
						if (attribute.ToLower() == "subsup")
						{
							mathNAry.SubSuperscriptLimit = true;
						}
						else
						{
							mathNAry.SubSuperscriptLimit = false;
						}
					}
					break;
				}
				case "grow":
					mathNAry.HasGrow = GetBooleanValue(reader);
					break;
				case "subHide":
					mathNAry.HideLowerLimit = GetBooleanValue(reader);
					break;
				case "supHide":
					mathNAry.HideUpperLimit = GetBooleanValue(reader);
					break;
				case "ctrlPr":
					mathNAry.ControlProperties = ParseMathControlProperties(reader, mathNAry, mathNAry.ControlProperties);
					break;
				}
				reader.Read();
			}
			else
			{
				reader.Read();
			}
		}
	}

	private void ParseMathRadical(XmlReader reader, OfficeMathRadical mathRadical)
	{
		while (reader.NodeType != XmlNodeType.Element)
		{
			reader.Read();
		}
		if (reader.IsEmptyElement)
		{
			return;
		}
		string localName = reader.LocalName;
		reader.Read();
		if (localName == reader.LocalName && reader.NodeType == XmlNodeType.EndElement)
		{
			return;
		}
		SkipWhitespaces(reader);
		while (reader.LocalName != localName)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				switch (reader.LocalName)
				{
				case "radPr":
					ParseMathRadicalProperties(reader, mathRadical);
					break;
				case "e":
					ParseMath(reader, mathRadical.Equation as OfficeMath);
					break;
				case "deg":
					ParseMath(reader, mathRadical.Degree as OfficeMath);
					break;
				}
				reader.Read();
			}
			else
			{
				reader.Read();
			}
		}
	}

	private void ParseMathRadicalProperties(XmlReader reader, OfficeMathRadical mathRadical)
	{
		while (reader.NodeType != XmlNodeType.Element)
		{
			reader.Read();
		}
		if (reader.IsEmptyElement)
		{
			return;
		}
		string localName = reader.LocalName;
		reader.Read();
		if (localName == reader.LocalName && reader.NodeType == XmlNodeType.EndElement)
		{
			return;
		}
		SkipWhitespaces(reader);
		while (!(reader.LocalName == localName) || reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				string localName2 = reader.LocalName;
				if (!(localName2 == "degHide"))
				{
					if (localName2 == "ctrlPr")
					{
						mathRadical.ControlProperties = ParseMathControlProperties(reader, mathRadical, mathRadical.ControlProperties);
					}
				}
				else
				{
					mathRadical.HideDegree = GetBooleanValue(reader);
				}
				reader.Read();
			}
			else
			{
				reader.Read();
			}
		}
	}

	private void ParseMathRightScript(XmlReader reader, OfficeMathRightScript mathRightScript)
	{
		while (reader.NodeType != XmlNodeType.Element)
		{
			reader.Read();
		}
		if (reader.IsEmptyElement)
		{
			return;
		}
		string localName = reader.LocalName;
		reader.Read();
		if (localName == reader.LocalName && reader.NodeType == XmlNodeType.EndElement)
		{
			return;
		}
		SkipWhitespaces(reader);
		while (reader.LocalName != localName)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				switch (reader.LocalName)
				{
				case "sSubSupPr":
					ParseMathRightScriptProperties(reader, mathRightScript);
					break;
				case "e":
					ParseMath(reader, mathRightScript.Equation as OfficeMath);
					break;
				case "sup":
					ParseMath(reader, mathRightScript.Superscript as OfficeMath);
					break;
				case "sub":
					ParseMath(reader, mathRightScript.Subscript as OfficeMath);
					break;
				}
				reader.Read();
			}
			else
			{
				reader.Read();
			}
		}
	}

	private void ParseMathRightScriptProperties(XmlReader reader, OfficeMathRightScript mathRightScript)
	{
		while (reader.NodeType != XmlNodeType.Element)
		{
			reader.Read();
		}
		if (reader.IsEmptyElement)
		{
			return;
		}
		string localName = reader.LocalName;
		reader.Read();
		if (localName == reader.LocalName && reader.NodeType == XmlNodeType.EndElement)
		{
			return;
		}
		SkipWhitespaces(reader);
		while (!(reader.LocalName == localName) || reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				string localName2 = reader.LocalName;
				if (!(localName2 == "alnScr"))
				{
					if (localName2 == "ctrlPr")
					{
						mathRightScript.ControlProperties = ParseMathControlProperties(reader, mathRightScript, mathRightScript.ControlProperties);
					}
				}
				else
				{
					mathRightScript.IsSkipAlign = GetBooleanValue(reader);
				}
				reader.Read();
			}
			else
			{
				reader.Read();
			}
		}
	}

	private void ParseMathLeftScript(XmlReader reader, OfficeMathLeftScript mathLeftScript)
	{
		while (reader.NodeType != XmlNodeType.Element)
		{
			reader.Read();
		}
		if (reader.IsEmptyElement)
		{
			return;
		}
		string localName = reader.LocalName;
		reader.Read();
		if (localName == reader.LocalName && reader.NodeType == XmlNodeType.EndElement)
		{
			return;
		}
		SkipWhitespaces(reader);
		while (reader.LocalName != localName)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				switch (reader.LocalName)
				{
				case "sPrePr":
					ParseMathLeftScriptProperties(reader, mathLeftScript);
					break;
				case "e":
					ParseMath(reader, mathLeftScript.Equation as OfficeMath);
					break;
				case "sup":
					ParseMath(reader, mathLeftScript.Superscript as OfficeMath);
					break;
				case "sub":
					ParseMath(reader, mathLeftScript.Subscript as OfficeMath);
					break;
				}
				reader.Read();
			}
			else
			{
				reader.Read();
			}
		}
	}

	private void ParseMathLeftScriptProperties(XmlReader reader, OfficeMathLeftScript mathLeftScript)
	{
		while (reader.NodeType != XmlNodeType.Element)
		{
			reader.Read();
		}
		if (reader.IsEmptyElement)
		{
			return;
		}
		string localName = reader.LocalName;
		reader.Read();
		if (localName == reader.LocalName && reader.NodeType == XmlNodeType.EndElement)
		{
			return;
		}
		SkipWhitespaces(reader);
		while (!(reader.LocalName == localName) || reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				if (reader.LocalName == "ctrlPr")
				{
					mathLeftScript.ControlProperties = ParseMathControlProperties(reader, mathLeftScript, mathLeftScript.ControlProperties);
				}
				reader.Read();
			}
			else
			{
				reader.Read();
			}
		}
	}

	private void ParseMathScript(XmlReader reader, OfficeMathScript mathScript)
	{
		while (reader.NodeType != XmlNodeType.Element)
		{
			reader.Read();
		}
		if (reader.IsEmptyElement)
		{
			return;
		}
		string localName = reader.LocalName;
		reader.Read();
		if (localName == reader.LocalName && reader.NodeType == XmlNodeType.EndElement)
		{
			return;
		}
		SkipWhitespaces(reader);
		while (reader.LocalName != localName)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				switch (reader.LocalName)
				{
				case "sSupPr":
				case "sSubPr":
					ParseMathScriptProperties(reader, mathScript);
					break;
				case "e":
					ParseMath(reader, mathScript.Equation as OfficeMath);
					break;
				case "sub":
				case "sup":
					ParseMath(reader, mathScript.Script as OfficeMath);
					break;
				}
				reader.Read();
			}
			else
			{
				reader.Read();
			}
		}
	}

	private void ParseMathScriptProperties(XmlReader reader, OfficeMathScript mathScript)
	{
		while (reader.NodeType != XmlNodeType.Element)
		{
			reader.Read();
		}
		if (reader.IsEmptyElement)
		{
			return;
		}
		string localName = reader.LocalName;
		reader.Read();
		if (localName == reader.LocalName && reader.NodeType == XmlNodeType.EndElement)
		{
			return;
		}
		SkipWhitespaces(reader);
		while (!(reader.LocalName == localName) || reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				if (reader.LocalName == "ctrlPr")
				{
					mathScript.ControlProperties = ParseMathControlProperties(reader, mathScript, mathScript.ControlProperties);
				}
				reader.Read();
			}
			else
			{
				reader.Read();
			}
		}
	}

	private void ParseMathEqArray(XmlReader reader, OfficeMathEquationArray mathEqArray)
	{
		while (reader.NodeType != XmlNodeType.Element)
		{
			reader.Read();
		}
		if (reader.IsEmptyElement)
		{
			return;
		}
		string localName = reader.LocalName;
		reader.Read();
		if (localName == reader.LocalName && reader.NodeType == XmlNodeType.EndElement)
		{
			return;
		}
		SkipWhitespaces(reader);
		while (reader.LocalName != localName)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				string localName2 = reader.LocalName;
				if (!(localName2 == "eqArrPr"))
				{
					if (localName2 == "e")
					{
						OfficeMath officeMath = new OfficeMath(mathEqArray);
						ParseMath(reader, officeMath);
						mathEqArray.m_equation.InnerList.Add(officeMath);
					}
				}
				else
				{
					ParseMathEqArrayProperties(reader, mathEqArray);
				}
				reader.Read();
			}
			else
			{
				reader.Read();
			}
		}
	}

	private void ParseMathEqArrayProperties(XmlReader reader, OfficeMathEquationArray mathEqArray)
	{
		string value = string.Empty;
		while (reader.NodeType != XmlNodeType.Element)
		{
			reader.Read();
		}
		if (reader.IsEmptyElement)
		{
			return;
		}
		string localName = reader.LocalName;
		reader.Read();
		if (localName == reader.LocalName && reader.NodeType == XmlNodeType.EndElement)
		{
			return;
		}
		SkipWhitespaces(reader);
		while (!(reader.LocalName == localName) || reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				switch (reader.LocalName)
				{
				case "baseJc":
					ParseMathEqArrayJustification(reader, mathEqArray);
					break;
				case "maxDist":
					mathEqArray.ExpandEquationContainer = GetBooleanValue(reader);
					break;
				case "objDist":
					mathEqArray.ExpandEquationContent = GetBooleanValue(reader);
					break;
				case "rSp":
					value = reader.GetAttribute("val", reader.NamespaceURI);
					break;
				case "rSpRule":
					mathEqArray.RowSpacingRule = ParseSpacingRule(reader);
					break;
				case "ctrlPr":
					mathEqArray.ControlProperties = ParseMathControlProperties(reader, mathEqArray, mathEqArray.ControlProperties);
					break;
				}
				reader.Read();
			}
			else
			{
				reader.Read();
			}
		}
		if (!string.IsNullOrEmpty(value) && (mathEqArray.RowSpacingRule == SpacingRule.Exactly || mathEqArray.RowSpacingRule == SpacingRule.Multiple))
		{
			float num = (float)Math.Round(GetSpacingValue(value, "rSp", mathEqArray.RowSpacingRule), 2);
			mathEqArray.SetPropertyValue(27, num);
		}
	}

	private void ParseMathEqArrayJustification(XmlReader reader, OfficeMathEquationArray mathEqArray)
	{
		switch (reader.GetAttribute("val", reader.NamespaceURI).ToLower())
		{
		case "top":
			mathEqArray.VerticalAlignment = MathVerticalAlignment.Top;
			break;
		case "bottom":
			mathEqArray.VerticalAlignment = MathVerticalAlignment.Bottom;
			break;
		case "center":
			mathEqArray.VerticalAlignment = MathVerticalAlignment.Center;
			break;
		}
	}

	private void ParseMathGroupChar(XmlReader reader, OfficeMathGroupCharacter mathGroupChar)
	{
		while (reader.NodeType != XmlNodeType.Element)
		{
			reader.Read();
		}
		if (reader.IsEmptyElement)
		{
			return;
		}
		string localName = reader.LocalName;
		reader.Read();
		if (localName == reader.LocalName && reader.NodeType == XmlNodeType.EndElement)
		{
			return;
		}
		SkipWhitespaces(reader);
		while (reader.LocalName != localName)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				string localName2 = reader.LocalName;
				if (!(localName2 == "groupChrPr"))
				{
					if (localName2 == "e")
					{
						ParseMath(reader, mathGroupChar.Equation as OfficeMath);
					}
				}
				else
				{
					ParseGroupCharProperties(reader, mathGroupChar);
				}
				reader.Read();
			}
			else
			{
				reader.Read();
			}
		}
	}

	private void ParseGroupCharProperties(XmlReader reader, OfficeMathGroupCharacter mathGroupChar)
	{
		while (reader.NodeType != XmlNodeType.Element)
		{
			reader.Read();
		}
		if (reader.IsEmptyElement)
		{
			return;
		}
		string localName = reader.LocalName;
		reader.Read();
		if (localName == reader.LocalName && reader.NodeType == XmlNodeType.EndElement)
		{
			return;
		}
		SkipWhitespaces(reader);
		while (!(reader.LocalName == localName) || reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				switch (reader.LocalName)
				{
				case "chr":
				{
					string attribute3 = reader.GetAttribute("val", reader.NamespaceURI);
					if (!string.IsNullOrEmpty(attribute3))
					{
						mathGroupChar.GroupCharacter = attribute3;
					}
					break;
				}
				case "pos":
				{
					string attribute2 = reader.GetAttribute("val", reader.NamespaceURI);
					if (!string.IsNullOrEmpty(attribute2) && attribute2.ToLower() == "top")
					{
						mathGroupChar.HasCharacterTop = true;
					}
					break;
				}
				case "vertJc":
				{
					string attribute = reader.GetAttribute("val", reader.NamespaceURI);
					if (!string.IsNullOrEmpty(attribute) && attribute.ToLower() == "bot")
					{
						mathGroupChar.HasAlignTop = false;
					}
					break;
				}
				case "ctrlPr":
					mathGroupChar.ControlProperties = ParseMathControlProperties(reader, mathGroupChar, mathGroupChar.ControlProperties);
					break;
				}
				reader.Read();
			}
			else
			{
				reader.Read();
			}
		}
	}

	private void ParseMathBar(XmlReader reader, OfficeMathBar mathBar)
	{
		while (reader.NodeType != XmlNodeType.Element)
		{
			reader.Read();
		}
		if (reader.IsEmptyElement)
		{
			return;
		}
		string localName = reader.LocalName;
		reader.Read();
		if (localName == reader.LocalName && reader.NodeType == XmlNodeType.EndElement)
		{
			return;
		}
		SkipWhitespaces(reader);
		while (reader.LocalName != localName)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				string localName2 = reader.LocalName;
				if (!(localName2 == "barPr"))
				{
					if (localName2 == "e")
					{
						ParseMath(reader, mathBar.Equation as OfficeMath);
					}
				}
				else
				{
					ParseBarProperties(reader, mathBar);
				}
				reader.Read();
			}
			else
			{
				reader.Read();
			}
		}
	}

	private void ParseBarProperties(XmlReader reader, OfficeMathBar mathBar)
	{
		while (reader.NodeType != XmlNodeType.Element)
		{
			reader.Read();
		}
		if (reader.IsEmptyElement)
		{
			return;
		}
		string localName = reader.LocalName;
		reader.Read();
		if (localName == reader.LocalName && reader.NodeType == XmlNodeType.EndElement)
		{
			return;
		}
		SkipWhitespaces(reader);
		while (!(reader.LocalName == localName) || reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				string localName2 = reader.LocalName;
				if (!(localName2 == "pos"))
				{
					if (localName2 == "ctrlPr")
					{
						mathBar.ControlProperties = ParseMathControlProperties(reader, mathBar, mathBar.ControlProperties);
					}
				}
				else
				{
					string attribute = reader.GetAttribute("val", reader.NamespaceURI);
					if (!string.IsNullOrEmpty(attribute) && attribute.ToLower() == "top")
					{
						mathBar.BarTop = true;
					}
				}
				reader.Read();
			}
			else
			{
				reader.Read();
			}
		}
	}

	private void ParseMathAccent(XmlReader reader, OfficeMathAccent mathAccent)
	{
		while (reader.NodeType != XmlNodeType.Element)
		{
			reader.Read();
		}
		if (reader.IsEmptyElement)
		{
			return;
		}
		string localName = reader.LocalName;
		reader.Read();
		if (localName == reader.LocalName && reader.NodeType == XmlNodeType.EndElement)
		{
			return;
		}
		SkipWhitespaces(reader);
		while (reader.LocalName != localName)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				string localName2 = reader.LocalName;
				if (!(localName2 == "accPr"))
				{
					if (localName2 == "e")
					{
						ParseMath(reader, mathAccent.Equation as OfficeMath);
					}
				}
				else
				{
					ParseAccentProperties(reader, mathAccent);
				}
				reader.Read();
			}
			else
			{
				reader.Read();
			}
		}
	}

	private void ParseAccentProperties(XmlReader reader, OfficeMathAccent mathAccent)
	{
		while (reader.NodeType != XmlNodeType.Element)
		{
			reader.Read();
		}
		if (reader.IsEmptyElement)
		{
			return;
		}
		string localName = reader.LocalName;
		reader.Read();
		if (localName == reader.LocalName && reader.NodeType == XmlNodeType.EndElement)
		{
			return;
		}
		SkipWhitespaces(reader);
		while (!(reader.LocalName == localName) || reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				string localName2 = reader.LocalName;
				if (!(localName2 == "chr"))
				{
					if (localName2 == "ctrlPr")
					{
						mathAccent.ControlProperties = ParseMathControlProperties(reader, mathAccent, mathAccent.ControlProperties);
					}
				}
				else
				{
					string attribute = reader.GetAttribute("val", reader.NamespaceURI);
					if (!string.IsNullOrEmpty(attribute))
					{
						mathAccent.AccentCharacter = attribute;
					}
				}
				reader.Read();
			}
			else
			{
				reader.Read();
			}
		}
	}

	private IOfficeRunFormat ParseMathControlProperties(XmlReader reader, OfficeMathFunctionBase mathFunction, IOfficeRunFormat controlProperties)
	{
		while (reader.NodeType != XmlNodeType.Element)
		{
			reader.Read();
		}
		if (!reader.IsEmptyElement)
		{
			string localName = reader.LocalName;
			reader.Read();
			if (!(localName == reader.LocalName) || reader.NodeType != XmlNodeType.EndElement)
			{
				SkipWhitespaces(reader);
				while (!(reader.LocalName == localName) || reader.NodeType != XmlNodeType.EndElement)
				{
					if (reader.NodeType == XmlNodeType.Element)
					{
						if (reader.LocalName == "rPr")
						{
							controlProperties = m_documentParser.ParseMathControlFormat(reader, mathFunction);
						}
						reader.Read();
					}
					else
					{
						reader.Read();
					}
				}
			}
		}
		return controlProperties;
	}

	internal void ParseMathRunFormat(XmlReader reader, OfficeMathFormat mathFormat)
	{
		while (reader.NodeType != XmlNodeType.Element)
		{
			reader.Read();
		}
		if (reader.IsEmptyElement)
		{
			return;
		}
		string localName = reader.LocalName;
		reader.Read();
		if (localName == reader.LocalName && reader.NodeType == XmlNodeType.EndElement)
		{
			return;
		}
		SkipWhitespaces(reader);
		while (!(reader.LocalName == localName) || reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				switch (reader.LocalName)
				{
				case "aln":
					mathFormat.HasAlignment = GetBooleanValue(reader);
					break;
				case "brk":
				{
					OfficeMathRunElement obj = mathFormat.OwnerMathEntity as OfficeMathRunElement;
					OfficeMath officeMath = obj.OwnerMathEntity as OfficeMath;
					if (obj != null)
					{
						mathFormat.Break = officeMath.Breaks.Add(officeMath.Breaks.Count) as OfficeMathBreak;
						ParseOfficeMathBreak(reader, (OfficeMathBreak)mathFormat.Break);
					}
					break;
				}
				case "lit":
					mathFormat.HasLiteral = GetBooleanValue(reader);
					break;
				case "nor":
					mathFormat.HasNormalText = GetBooleanValue(reader);
					break;
				case "scr":
					ParseMathRunFormatScript(reader, mathFormat);
					break;
				case "sty":
					ParseMathRunFormatStyle(reader, mathFormat);
					break;
				}
				reader.Read();
			}
			else
			{
				reader.Read();
			}
		}
	}

	private void ParseMathRunFormatScript(XmlReader reader, OfficeMathFormat mathFormat)
	{
		switch (reader.GetAttribute("val", reader.NamespaceURI).ToLower())
		{
		case "double-struck":
			mathFormat.Font = MathFontType.DoubleStruck;
			break;
		case "fraktur":
			mathFormat.Font = MathFontType.Fraktur;
			break;
		case "monospace":
			mathFormat.Font = MathFontType.Monospace;
			break;
		case "sans-serif":
			mathFormat.Font = MathFontType.SansSerif;
			break;
		case "script":
			mathFormat.Font = MathFontType.Script;
			break;
		case "roman":
			mathFormat.Font = MathFontType.Roman;
			break;
		}
	}

	private void ParseMathRunFormatStyle(XmlReader reader, OfficeMathFormat mathFormat)
	{
		switch (reader.GetAttribute("val", reader.NamespaceURI).ToLower())
		{
		case "b":
			mathFormat.Style = MathStyleType.Bold;
			break;
		case "bi":
			mathFormat.Style = MathStyleType.BoldItalic;
			break;
		case "p":
			mathFormat.Style = MathStyleType.Regular;
			break;
		case "i":
			mathFormat.Style = MathStyleType.Italic;
			break;
		}
	}

	private void ParseMathParaProperties(XmlReader reader, IOfficeMathParagraph mathPara)
	{
		while (reader.NodeType != XmlNodeType.Element)
		{
			reader.Read();
		}
		if (reader.IsEmptyElement)
		{
			return;
		}
		string localName = reader.LocalName;
		reader.Read();
		if (localName == reader.LocalName && reader.NodeType == XmlNodeType.EndElement)
		{
			return;
		}
		SkipWhitespaces(reader);
		while (!(reader.LocalName == localName) || reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				if (reader.LocalName == "jc")
				{
					ParseMathJustification(reader, mathPara);
				}
				reader.Read();
			}
			else
			{
				reader.Read();
			}
		}
	}

	private void ParseMathJustification(XmlReader reader, IOfficeMathParagraph mathPara)
	{
		switch (reader.GetAttribute("val", reader.NamespaceURI).ToLower())
		{
		case "center":
			mathPara.Justification = MathJustification.Center;
			break;
		case "left":
			mathPara.Justification = MathJustification.Left;
			break;
		case "right":
			mathPara.Justification = MathJustification.Right;
			break;
		case "centergroup":
			mathPara.Justification = MathJustification.CenterGroup;
			break;
		}
	}

	private void ParseMathBox(XmlReader reader, OfficeMathBox mathBox)
	{
		while (reader.NodeType != XmlNodeType.Element)
		{
			reader.Read();
		}
		if (reader.IsEmptyElement)
		{
			return;
		}
		string localName = reader.LocalName;
		reader.Read();
		if (localName == reader.LocalName && reader.NodeType == XmlNodeType.EndElement)
		{
			return;
		}
		SkipWhitespaces(reader);
		while (!(reader.LocalName == localName) || reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				string localName2 = reader.LocalName;
				if (!(localName2 == "boxPr"))
				{
					if (localName2 == "e")
					{
						ParseMath(reader, mathBox.Equation as OfficeMath);
					}
				}
				else
				{
					ParseMathBoxProperties(reader, mathBox);
				}
				reader.Read();
			}
			else
			{
				reader.Read();
			}
		}
	}

	private void ParseMathBoxProperties(XmlReader reader, OfficeMathBox mathBox)
	{
		while (reader.NodeType != XmlNodeType.Element)
		{
			reader.Read();
		}
		if (reader.IsEmptyElement)
		{
			return;
		}
		string localName = reader.LocalName;
		reader.Read();
		if (localName == reader.LocalName && reader.NodeType == XmlNodeType.EndElement)
		{
			return;
		}
		SkipWhitespaces(reader);
		while (!(reader.LocalName == localName) || reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				switch (reader.LocalName)
				{
				case "aln":
					mathBox.Alignment = GetBooleanValue(reader);
					break;
				case "diff":
					mathBox.EnableDifferential = GetBooleanValue(reader);
					break;
				case "opEmu":
					mathBox.OperatorEmulator = GetBooleanValue(reader);
					break;
				case "noBreak":
					mathBox.NoBreak = GetBooleanValue(reader);
					break;
				case "brk":
					if (mathBox.OwnerMathEntity is OfficeMath officeMath)
					{
						mathBox.Break = officeMath.Breaks.Add(officeMath.Breaks.Count) as OfficeMathBreak;
						ParseOfficeMathBreak(reader, (OfficeMathBreak)mathBox.Break);
					}
					break;
				case "ctrlPr":
					mathBox.ControlProperties = ParseMathControlProperties(reader, mathBox, mathBox.ControlProperties);
					break;
				}
				reader.Read();
			}
			else
			{
				reader.Read();
			}
		}
	}

	private void ParseMathBorderBox(XmlReader reader, OfficeMathBorderBox mathBorderBox)
	{
		while (reader.NodeType != XmlNodeType.Element)
		{
			reader.Read();
		}
		if (reader.IsEmptyElement)
		{
			return;
		}
		string localName = reader.LocalName;
		reader.Read();
		if (localName == reader.LocalName && reader.NodeType == XmlNodeType.EndElement)
		{
			return;
		}
		SkipWhitespaces(reader);
		while (!(reader.LocalName == localName) || reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				string localName2 = reader.LocalName;
				if (!(localName2 == "borderBoxPr"))
				{
					if (localName2 == "e")
					{
						ParseMath(reader, mathBorderBox.Equation as OfficeMath);
					}
				}
				else
				{
					ParseMathBorderBoxProperties(reader, mathBorderBox);
				}
				reader.Read();
			}
			else
			{
				reader.Read();
			}
		}
	}

	private void ParseMathBorderBoxProperties(XmlReader reader, OfficeMathBorderBox mathBorderBox)
	{
		while (reader.NodeType != XmlNodeType.Element)
		{
			reader.Read();
		}
		if (reader.IsEmptyElement)
		{
			return;
		}
		string localName = reader.LocalName;
		reader.Read();
		if (localName == reader.LocalName && reader.NodeType == XmlNodeType.EndElement)
		{
			return;
		}
		SkipWhitespaces(reader);
		while (!(reader.LocalName == localName) || reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				switch (reader.LocalName)
				{
				case "hideBot":
					mathBorderBox.HideBottom = GetBooleanValue(reader);
					break;
				case "hideLeft":
					mathBorderBox.HideLeft = GetBooleanValue(reader);
					break;
				case "hideRight":
					mathBorderBox.HideRight = GetBooleanValue(reader);
					break;
				case "hideTop":
					mathBorderBox.HideTop = GetBooleanValue(reader);
					break;
				case "strikeBLTR":
					mathBorderBox.StrikeDiagonalUp = GetBooleanValue(reader);
					break;
				case "strikeH":
					mathBorderBox.StrikeHorizontal = GetBooleanValue(reader);
					break;
				case "strikeTLBR":
					mathBorderBox.StrikeDiagonalDown = GetBooleanValue(reader);
					break;
				case "strikeV":
					mathBorderBox.StrikeVertical = GetBooleanValue(reader);
					break;
				case "ctrlPr":
					mathBorderBox.ControlProperties = ParseMathControlProperties(reader, mathBorderBox, mathBorderBox.ControlProperties);
					break;
				}
				reader.Read();
			}
			else
			{
				reader.Read();
			}
		}
	}

	private void ParseMathDelimiter(XmlReader reader, OfficeMathDelimiter mathDelimiter)
	{
		while (reader.NodeType != XmlNodeType.Element)
		{
			reader.Read();
		}
		if (reader.IsEmptyElement)
		{
			return;
		}
		string localName = reader.LocalName;
		reader.Read();
		if (localName == reader.LocalName && reader.NodeType == XmlNodeType.EndElement)
		{
			return;
		}
		SkipWhitespaces(reader);
		while (!(reader.LocalName == localName) || reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				string localName2 = reader.LocalName;
				if (!(localName2 == "dPr"))
				{
					if (localName2 == "e")
					{
						OfficeMath officeMath = new OfficeMath(mathDelimiter);
						ParseMath(reader, officeMath);
						mathDelimiter.m_equation.InnerList.Add(officeMath);
					}
				}
				else
				{
					ParseMathDelimiterProperties(reader, mathDelimiter);
				}
				reader.Read();
			}
			else
			{
				reader.Read();
			}
		}
	}

	private void ParseMathDelimiterProperties(XmlReader reader, OfficeMathDelimiter mathDelimiter)
	{
		while (reader.NodeType != XmlNodeType.Element)
		{
			reader.Read();
		}
		if (reader.IsEmptyElement)
		{
			return;
		}
		string localName = reader.LocalName;
		reader.Read();
		if (localName == reader.LocalName && reader.NodeType == XmlNodeType.EndElement)
		{
			return;
		}
		SkipWhitespaces(reader);
		while (!(reader.LocalName == localName) || reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				switch (reader.LocalName)
				{
				case "begChr":
				{
					string attribute2 = reader.GetAttribute("val", reader.NamespaceURI);
					if (attribute2 != null)
					{
						mathDelimiter.BeginCharacter = attribute2;
					}
					break;
				}
				case "endChr":
				{
					string attribute3 = reader.GetAttribute("val", reader.NamespaceURI);
					if (attribute3 != null)
					{
						mathDelimiter.EndCharacter = attribute3;
					}
					break;
				}
				case "grow":
					mathDelimiter.IsGrow = GetBooleanValue(reader);
					break;
				case "sepChr":
				{
					string attribute = reader.GetAttribute("val", reader.NamespaceURI);
					if (!string.IsNullOrEmpty(attribute))
					{
						mathDelimiter.Seperator = attribute;
					}
					break;
				}
				case "shp":
					ParseMathDelimiterShape(reader, mathDelimiter);
					break;
				case "ctrlPr":
					mathDelimiter.ControlProperties = ParseMathControlProperties(reader, mathDelimiter, mathDelimiter.ControlProperties);
					break;
				}
				reader.Read();
			}
			else
			{
				reader.Read();
			}
		}
	}

	private void ParseMathFraction(XmlReader reader, OfficeMathFraction mathFraction)
	{
		while (reader.NodeType != XmlNodeType.Element)
		{
			reader.Read();
		}
		if (reader.IsEmptyElement)
		{
			return;
		}
		string localName = reader.LocalName;
		reader.Read();
		if (localName == reader.LocalName && reader.NodeType == XmlNodeType.EndElement)
		{
			return;
		}
		SkipWhitespaces(reader);
		while (!(reader.LocalName == localName) || reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				switch (reader.LocalName)
				{
				case "fPr":
					ParseMathFractionProperties(reader, mathFraction);
					break;
				case "num":
					ParseMath(reader, mathFraction.Numerator as OfficeMath);
					break;
				case "den":
					ParseMath(reader, mathFraction.Denominator as OfficeMath);
					break;
				}
				reader.Read();
			}
			else
			{
				reader.Read();
			}
		}
	}

	private void ParseMathFractionProperties(XmlReader reader, OfficeMathFraction mathFraction)
	{
		while (reader.NodeType != XmlNodeType.Element)
		{
			reader.Read();
		}
		if (reader.IsEmptyElement)
		{
			return;
		}
		string localName = reader.LocalName;
		reader.Read();
		if (localName == reader.LocalName && reader.NodeType == XmlNodeType.EndElement)
		{
			return;
		}
		SkipWhitespaces(reader);
		while (!(reader.LocalName == localName) || reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				string localName2 = reader.LocalName;
				if (!(localName2 == "type"))
				{
					if (localName2 == "ctrlPr")
					{
						mathFraction.ControlProperties = ParseMathControlProperties(reader, mathFraction, mathFraction.ControlProperties);
					}
				}
				else
				{
					ParseMathFractionType(reader, mathFraction);
				}
				reader.Read();
			}
			else
			{
				reader.Read();
			}
		}
	}

	private void ParseMathFractionType(XmlReader reader, OfficeMathFraction mathFraction)
	{
		switch (reader.GetAttribute("val", reader.NamespaceURI).ToLower())
		{
		case "nobar":
			mathFraction.FractionType = MathFractionType.NoFractionBar;
			break;
		case "skw":
			mathFraction.FractionType = MathFractionType.SkewedFractionBar;
			break;
		case "lin":
			mathFraction.FractionType = MathFractionType.FractionInline;
			break;
		case "bar":
			mathFraction.FractionType = MathFractionType.NormalFractionBar;
			break;
		}
	}

	private void ParseMathFunc(XmlReader reader, OfficeMathFunction mathFunc)
	{
		while (reader.NodeType != XmlNodeType.Element)
		{
			reader.Read();
		}
		if (reader.IsEmptyElement)
		{
			return;
		}
		string localName = reader.LocalName;
		reader.Read();
		if (localName == reader.LocalName && reader.NodeType == XmlNodeType.EndElement)
		{
			return;
		}
		SkipWhitespaces(reader);
		while (!(reader.LocalName == localName) || reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				switch (reader.LocalName)
				{
				case "funcPr":
					ParseMathFuncProperties(reader, mathFunc);
					break;
				case "fName":
					ParseMath(reader, mathFunc.FunctionName as OfficeMath);
					break;
				case "e":
					ParseMath(reader, mathFunc.Equation as OfficeMath);
					break;
				}
				reader.Read();
			}
			else
			{
				reader.Read();
			}
		}
	}

	private void ParseMathFuncProperties(XmlReader reader, OfficeMathFunction mathFunc)
	{
		while (reader.NodeType != XmlNodeType.Element)
		{
			reader.Read();
		}
		if (reader.IsEmptyElement)
		{
			return;
		}
		string localName = reader.LocalName;
		reader.Read();
		if (localName == reader.LocalName && reader.NodeType == XmlNodeType.EndElement)
		{
			return;
		}
		SkipWhitespaces(reader);
		while (!(reader.LocalName == localName) || reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				if (reader.LocalName == "ctrlPr")
				{
					mathFunc.ControlProperties = ParseMathControlProperties(reader, mathFunc, mathFunc.ControlProperties);
				}
				reader.Read();
			}
			else
			{
				reader.Read();
			}
		}
	}

	private void ParseMathDelimiterShape(XmlReader reader, OfficeMathDelimiter mathDelimiter)
	{
		if (reader.GetAttribute("val", reader.NamespaceURI) == "match")
		{
			mathDelimiter.DelimiterShape = MathDelimiterShapeType.Match;
		}
		else
		{
			mathDelimiter.DelimiterShape = MathDelimiterShapeType.Centered;
		}
	}

	private void ParseOfficeMathBreak(XmlReader reader, OfficeMathBreak mathBreak)
	{
		string attribute = reader.GetAttribute("alnAt", reader.NamespaceURI);
		if (mathBreak != null && attribute != null)
		{
			mathBreak.AlignAt = (int)GetNumericValue(attribute);
		}
	}

	private bool GetBooleanValue(XmlReader reader)
	{
		bool result = true;
		if (reader.AttributeCount > 0)
		{
			switch (reader.GetAttribute("val", reader.NamespaceURI))
			{
			case null:
			case "0":
			case "false":
			case "off":
				result = false;
				break;
			}
		}
		return result;
	}

	private float GetNumericValue(string value)
	{
		float result = 0f;
		float.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out result);
		if (value != null && (double)result == 0.0 && m_isFloatValue.IsMatch(value) && m_hasAlphabet.IsMatch(value))
		{
			string[] array = value.Split(new char[1] { '.' }, StringSplitOptions.RemoveEmptyEntries);
			float.TryParse(value.StartsWith(".") ? ("0." + array[0]) : ((array.Length > 1) ? (array[0] + "." + array[1]) : array[0]), NumberStyles.Number, CultureInfo.InvariantCulture, out result);
		}
		return result;
	}

	private float GetFloatValue(string value, string elementName)
	{
		switch (elementName)
		{
		case "cSp":
		case "lMargin":
		case "rMargin":
		case "wrapIndent":
			return GetNumericValue(value) / 20f;
		default:
			return 0f;
		}
	}

	private float GetSpacingValue(string value, string elementName, SpacingRule spacingRule)
	{
		if (spacingRule == SpacingRule.Exactly)
		{
			return GetNumericValue(value) / 20f;
		}
		if (!(elementName == "rSp"))
		{
			if (elementName == "cGp")
			{
				return GetNumericValue(value) * 6f;
			}
			return 0f;
		}
		return GetNumericValue(value) / 2f;
	}

	private void SkipWhitespaces(XmlReader reader)
	{
		if (reader.NodeType != XmlNodeType.Element)
		{
			while (reader.NodeType == XmlNodeType.Whitespace)
			{
				reader.Read();
			}
		}
	}
}
