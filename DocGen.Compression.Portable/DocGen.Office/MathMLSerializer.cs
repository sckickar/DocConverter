using System;
using System.Globalization;
using System.Xml;

namespace DocGen.Office;

internal class MathMLSerializer
{
	private DocumentSerializer m_documentSerializer;

	private const string M_namespace = "http://schemas.openxmlformats.org/officeDocument/2006/math";

	private XmlWriter m_writer;

	internal void SerializeMathPara(XmlWriter writer, IOfficeMathParagraph mathPara, DocumentSerializer documentSerializer)
	{
		m_writer = writer;
		m_documentSerializer = documentSerializer;
		writer.WriteStartElement("oMathPara", "http://schemas.openxmlformats.org/officeDocument/2006/math");
		if (!(mathPara as OfficeMathParagraph).IsDefault)
		{
			SerilaizeMathParaProperties(mathPara);
		}
		for (int i = 0; i < mathPara.Maths.Count; i++)
		{
			m_writer.WriteStartElement("oMath", "http://schemas.openxmlformats.org/officeDocument/2006/math");
			SerializeMath(mathPara.Maths[i]);
			m_writer.WriteEndElement();
		}
		writer.WriteEndElement();
	}

	private void SerilaizeMathParaProperties(IOfficeMathParagraph mathPara)
	{
		m_writer.WriteStartElement("oMathParaPr", "http://schemas.openxmlformats.org/officeDocument/2006/math");
		SerializeMathParaJustification(mathPara);
		m_writer.WriteEndElement();
	}

	private void SerializeMathParaJustification(IOfficeMathParagraph mathPara)
	{
		m_writer.WriteStartElement("jc", "http://schemas.openxmlformats.org/officeDocument/2006/math");
		switch (mathPara.Justification)
		{
		case MathJustification.Center:
			m_writer.WriteAttributeString("val", "http://schemas.openxmlformats.org/officeDocument/2006/math", "center");
			break;
		case MathJustification.Left:
			m_writer.WriteAttributeString("val", "http://schemas.openxmlformats.org/officeDocument/2006/math", "left");
			break;
		case MathJustification.Right:
			m_writer.WriteAttributeString("val", "http://schemas.openxmlformats.org/officeDocument/2006/math", "right");
			break;
		default:
			m_writer.WriteAttributeString("val", "http://schemas.openxmlformats.org/officeDocument/2006/math", "centerGroup");
			break;
		}
		m_writer.WriteEndElement();
	}

	internal void SerializeMath(XmlWriter writer, IOfficeMath officeMath, DocumentSerializer documentSerializer)
	{
		m_writer = writer;
		m_documentSerializer = documentSerializer;
		SerializeMath(officeMath);
	}

	private void SerializeMath(IOfficeMath officeMath)
	{
		if ((officeMath as OfficeMath).HasValue(74))
		{
			m_writer.WriteStartElement("argPr", "http://schemas.openxmlformats.org/officeDocument/2006/math");
			m_writer.WriteStartElement("argSz", "http://schemas.openxmlformats.org/officeDocument/2006/math");
			m_writer.WriteAttributeString("val", "http://schemas.openxmlformats.org/officeDocument/2006/math", ToString(officeMath.ArgumentSize));
			m_writer.WriteEndElement();
			m_writer.WriteEndElement();
		}
		for (int i = 0; i < officeMath.Functions.Count; i++)
		{
			SerializeMathFunction(officeMath.Functions[i]);
		}
	}

	private void SerializeMathFunction(IOfficeMathFunctionBase officeMathFunction)
	{
		switch (officeMathFunction.Type)
		{
		case MathFunctionType.Accent:
			SerializeMathAccent(officeMathFunction as OfficeMathAccent);
			break;
		case MathFunctionType.Bar:
			SerializeMathBar(officeMathFunction as OfficeMathBar);
			break;
		case MathFunctionType.BorderBox:
			SerializeMathBorderBox(officeMathFunction as OfficeMathBorderBox);
			break;
		case MathFunctionType.Box:
			SerializeMathBox(officeMathFunction as OfficeMathBox);
			break;
		case MathFunctionType.Delimiter:
			SerializeMathDelimiter(officeMathFunction as OfficeMathDelimiter);
			break;
		case MathFunctionType.EquationArray:
			SerializeMathEqArray(officeMathFunction as OfficeMathEquationArray);
			break;
		case MathFunctionType.Fraction:
			SerializeMathFraction(officeMathFunction as OfficeMathFraction);
			break;
		case MathFunctionType.Function:
			SerializeMathFunc(officeMathFunction as OfficeMathFunction);
			break;
		case MathFunctionType.GroupCharacter:
			SerializeMathGroupChar(officeMathFunction as OfficeMathGroupCharacter);
			break;
		case MathFunctionType.Limit:
		{
			OfficeMathLimit officeMathLimit = officeMathFunction as OfficeMathLimit;
			if (officeMathLimit.LimitType == MathLimitType.LowerLimit)
			{
				SerializeMathLowerLimit(officeMathLimit);
			}
			else
			{
				SerializeMathUpperLimit(officeMathLimit);
			}
			break;
		}
		case MathFunctionType.Matrix:
			SerializeMathMatrix(officeMathFunction as OfficeMathMatrix);
			break;
		case MathFunctionType.NArray:
			SerializeMathNAry(officeMathFunction as OfficeMathNArray);
			break;
		case MathFunctionType.Phantom:
			SerializeMathPhantom(officeMathFunction as OfficeMathPhantom);
			break;
		case MathFunctionType.Radical:
			SerializeMathRadical(officeMathFunction as OfficeMathRadical);
			break;
		case MathFunctionType.LeftSubSuperscript:
			SerializeMathLeftScript(officeMathFunction as OfficeMathLeftScript);
			break;
		case MathFunctionType.SubSuperscript:
		{
			OfficeMathScript officeMathScript = officeMathFunction as OfficeMathScript;
			if (officeMathScript.ScriptType == MathScriptType.Subscript)
			{
				SerializeMathSubScript(officeMathScript);
			}
			else
			{
				SerializeMathSuperScript(officeMathScript);
			}
			break;
		}
		case MathFunctionType.RightSubSuperscript:
			SerializeMathRightScript(officeMathFunction as OfficeMathRightScript);
			break;
		case MathFunctionType.RunElement:
			SerializeMathText(officeMathFunction as OfficeMathRunElement);
			break;
		}
	}

	internal void SerializeMathProperties(XmlWriter writer, OfficeMathProperties mathProperties)
	{
		m_writer = writer;
		m_writer.WriteStartElement("mathPr", "http://schemas.openxmlformats.org/officeDocument/2006/math");
		if (mathProperties.HasValue(66))
		{
			m_writer.WriteStartElement("mathFont", "http://schemas.openxmlformats.org/officeDocument/2006/math");
			m_writer.WriteAttributeString("val", "http://schemas.openxmlformats.org/officeDocument/2006/math", mathProperties.MathFont);
			m_writer.WriteEndElement();
		}
		if (mathProperties.HasValue(58))
		{
			SerializeBreakOnBinaryOperator(mathProperties);
		}
		if (mathProperties.HasValue(59))
		{
			SerializeBreakOnSubtractOperator(mathProperties);
		}
		if (mathProperties.HasValue(71))
		{
			m_writer.WriteStartElement("smallFrac", "http://schemas.openxmlformats.org/officeDocument/2006/math");
			m_writer.WriteAttributeString("val", "http://schemas.openxmlformats.org/officeDocument/2006/math", (!mathProperties.SmallFraction) ? "0" : "1");
			m_writer.WriteEndElement();
		}
		if (mathProperties.HasValue(61))
		{
			m_writer.WriteStartElement("dispDef", "http://schemas.openxmlformats.org/officeDocument/2006/math");
			m_writer.WriteAttributeString("val", "http://schemas.openxmlformats.org/officeDocument/2006/math", (!mathProperties.DisplayMathDefaults) ? "0" : "1");
			m_writer.WriteEndElement();
		}
		if (mathProperties.HasValue(65))
		{
			m_writer.WriteStartElement("lMargin", "http://schemas.openxmlformats.org/officeDocument/2006/math");
			m_writer.WriteAttributeString("val", "http://schemas.openxmlformats.org/officeDocument/2006/math", ToString(mathProperties.LeftMargin * 20));
			m_writer.WriteEndElement();
		}
		if (mathProperties.HasValue(70))
		{
			m_writer.WriteStartElement("rMargin", "http://schemas.openxmlformats.org/officeDocument/2006/math");
			m_writer.WriteAttributeString("val", "http://schemas.openxmlformats.org/officeDocument/2006/math", ToString(mathProperties.RightMargin * 20));
			m_writer.WriteEndElement();
		}
		if (mathProperties.HasValue(60))
		{
			SerializeDefaultJustification(mathProperties);
		}
		if (mathProperties.HasValue(72))
		{
			m_writer.WriteStartElement("wrapIndent", "http://schemas.openxmlformats.org/officeDocument/2006/math");
			m_writer.WriteAttributeString("val", "http://schemas.openxmlformats.org/officeDocument/2006/math", ToString(mathProperties.WrapIndent * 20));
			m_writer.WriteEndElement();
		}
		if (mathProperties.HasValue(73))
		{
			SerializeBoolProperty("wrapRight", mathProperties.WrapRight);
		}
		if (mathProperties.HasValue(63))
		{
			SerializeIntergralLimitLocation(mathProperties);
		}
		if (mathProperties.HasValue(67))
		{
			SerializeNAryLimitLocation(mathProperties);
		}
		m_writer.WriteEndElement();
	}

	private void SerializeNAryLimitLocation(OfficeMathProperties mathProperties)
	{
		m_writer.WriteStartElement("naryLim", "http://schemas.openxmlformats.org/officeDocument/2006/math");
		if (mathProperties.NAryLimitLocation == LimitLocationType.SubSuperscript)
		{
			m_writer.WriteAttributeString("val", "http://schemas.openxmlformats.org/officeDocument/2006/math", "subSup");
		}
		else
		{
			m_writer.WriteAttributeString("val", "http://schemas.openxmlformats.org/officeDocument/2006/math", "undOvr");
		}
		m_writer.WriteEndElement();
	}

	private void SerializeIntergralLimitLocation(OfficeMathProperties mathProperties)
	{
		m_writer.WriteStartElement("intLim", "http://schemas.openxmlformats.org/officeDocument/2006/math");
		if (mathProperties.IntegralLimitLocations == LimitLocationType.UnderOver)
		{
			m_writer.WriteAttributeString("val", "http://schemas.openxmlformats.org/officeDocument/2006/math", "undOvr");
		}
		else
		{
			m_writer.WriteAttributeString("val", "http://schemas.openxmlformats.org/officeDocument/2006/math", "subSup");
		}
		m_writer.WriteEndElement();
	}

	private void SerializeDefaultJustification(OfficeMathProperties mathProperties)
	{
		m_writer.WriteStartElement("defJc", "http://schemas.openxmlformats.org/officeDocument/2006/math");
		switch (mathProperties.DefaultJustification)
		{
		case MathJustification.Center:
			m_writer.WriteAttributeString("val", "http://schemas.openxmlformats.org/officeDocument/2006/math", "center");
			break;
		case MathJustification.Left:
			m_writer.WriteAttributeString("val", "http://schemas.openxmlformats.org/officeDocument/2006/math", "left");
			break;
		case MathJustification.Right:
			m_writer.WriteAttributeString("val", "http://schemas.openxmlformats.org/officeDocument/2006/math", "right");
			break;
		default:
			m_writer.WriteAttributeString("val", "http://schemas.openxmlformats.org/officeDocument/2006/math", "centerGroup");
			break;
		}
		m_writer.WriteEndElement();
	}

	private void SerializeBreakOnSubtractOperator(OfficeMathProperties mathProperties)
	{
		m_writer.WriteStartElement("brkBinSub", "http://schemas.openxmlformats.org/officeDocument/2006/math");
		switch (mathProperties.BreakOnBinarySubtraction)
		{
		case BreakOnBinarySubtractionType.PlusMinus:
			m_writer.WriteAttributeString("val", "http://schemas.openxmlformats.org/officeDocument/2006/math", "++");
			break;
		case BreakOnBinarySubtractionType.MinusPlus:
			m_writer.WriteAttributeString("val", "http://schemas.openxmlformats.org/officeDocument/2006/math", "-+");
			break;
		default:
			m_writer.WriteAttributeString("val", "http://schemas.openxmlformats.org/officeDocument/2006/math", "--");
			break;
		}
		m_writer.WriteEndElement();
	}

	private void SerializeBreakOnBinaryOperator(OfficeMathProperties mathProperties)
	{
		m_writer.WriteStartElement("brkBin", "http://schemas.openxmlformats.org/officeDocument/2006/math");
		switch (mathProperties.BreakOnBinaryOperators)
		{
		case BreakOnBinaryOperatorsType.After:
			m_writer.WriteAttributeString("val", "http://schemas.openxmlformats.org/officeDocument/2006/math", "after");
			break;
		case BreakOnBinaryOperatorsType.Repeat:
			m_writer.WriteAttributeString("val", "http://schemas.openxmlformats.org/officeDocument/2006/math", "repeat");
			break;
		default:
			m_writer.WriteAttributeString("val", "http://schemas.openxmlformats.org/officeDocument/2006/math", "before");
			break;
		}
		m_writer.WriteEndElement();
	}

	private void SerializeMathUpperLimit(OfficeMathLimit officeMathLimit)
	{
		m_writer.WriteStartElement("limUpp", "http://schemas.openxmlformats.org/officeDocument/2006/math");
		SerializeMathUpperLimitProperties(officeMathLimit);
		m_writer.WriteStartElement("e", "http://schemas.openxmlformats.org/officeDocument/2006/math");
		SerializeMath(officeMathLimit.Equation);
		m_writer.WriteEndElement();
		m_writer.WriteStartElement("lim", "http://schemas.openxmlformats.org/officeDocument/2006/math");
		SerializeMath(officeMathLimit.Limit);
		m_writer.WriteEndElement();
		m_writer.WriteEndElement();
	}

	private void SerializeMathUpperLimitProperties(OfficeMathLimit officeMathLimit)
	{
		m_writer.WriteStartElement("limUppPr", "http://schemas.openxmlformats.org/officeDocument/2006/math");
		if (officeMathLimit.ControlProperties != null)
		{
			m_writer.WriteStartElement("ctrlPr", "http://schemas.openxmlformats.org/officeDocument/2006/math");
			m_documentSerializer.SerializeControlProperties(officeMathLimit.ControlProperties);
			m_writer.WriteEndElement();
		}
		m_writer.WriteEndElement();
	}

	private void SerializeMathLowerLimit(OfficeMathLimit officeMathLimit)
	{
		m_writer.WriteStartElement("limLow", "http://schemas.openxmlformats.org/officeDocument/2006/math");
		SerializeMathLowerLimitProperties(officeMathLimit);
		m_writer.WriteStartElement("e", "http://schemas.openxmlformats.org/officeDocument/2006/math");
		SerializeMath(officeMathLimit.Equation);
		m_writer.WriteEndElement();
		m_writer.WriteStartElement("lim", "http://schemas.openxmlformats.org/officeDocument/2006/math");
		SerializeMath(officeMathLimit.Limit);
		m_writer.WriteEndElement();
		m_writer.WriteEndElement();
	}

	private void SerializeMathLowerLimitProperties(OfficeMathLimit officeMathLimit)
	{
		m_writer.WriteStartElement("limLowPr", "http://schemas.openxmlformats.org/officeDocument/2006/math");
		if (officeMathLimit.ControlProperties != null)
		{
			m_writer.WriteStartElement("ctrlPr", "http://schemas.openxmlformats.org/officeDocument/2006/math");
			m_documentSerializer.SerializeControlProperties(officeMathLimit.ControlProperties);
			m_writer.WriteEndElement();
		}
		m_writer.WriteEndElement();
	}

	private void SerializeMathRightScript(OfficeMathRightScript officeMathRightScript)
	{
		m_writer.WriteStartElement("sSubSup", "http://schemas.openxmlformats.org/officeDocument/2006/math");
		SerializeMathRightScriptProperties(officeMathRightScript);
		m_writer.WriteStartElement("e", "http://schemas.openxmlformats.org/officeDocument/2006/math");
		SerializeMath(officeMathRightScript.Equation);
		m_writer.WriteEndElement();
		m_writer.WriteStartElement("sub", "http://schemas.openxmlformats.org/officeDocument/2006/math");
		SerializeMath(officeMathRightScript.Subscript);
		m_writer.WriteEndElement();
		m_writer.WriteStartElement("sup", "http://schemas.openxmlformats.org/officeDocument/2006/math");
		SerializeMath(officeMathRightScript.Superscript);
		m_writer.WriteEndElement();
		m_writer.WriteEndElement();
	}

	private void SerializeMathRightScriptProperties(OfficeMathRightScript officeMathRightScript)
	{
		m_writer.WriteStartElement("sSubSupPr", "http://schemas.openxmlformats.org/officeDocument/2006/math");
		if (officeMathRightScript.HasValue(52))
		{
			SerializeBoolProperty("alnScr", officeMathRightScript.IsSkipAlign);
		}
		if (officeMathRightScript.ControlProperties != null)
		{
			m_writer.WriteStartElement("ctrlPr", "http://schemas.openxmlformats.org/officeDocument/2006/math");
			m_documentSerializer.SerializeControlProperties(officeMathRightScript.ControlProperties);
			m_writer.WriteEndElement();
		}
		m_writer.WriteEndElement();
	}

	private void SerializeMathLeftScript(OfficeMathLeftScript officeMathLeftScript)
	{
		m_writer.WriteStartElement("sPre", "http://schemas.openxmlformats.org/officeDocument/2006/math");
		SerializeMathLeftScriptProperties(officeMathLeftScript);
		m_writer.WriteStartElement("sub", "http://schemas.openxmlformats.org/officeDocument/2006/math");
		SerializeMath(officeMathLeftScript.Subscript);
		m_writer.WriteEndElement();
		m_writer.WriteStartElement("sup", "http://schemas.openxmlformats.org/officeDocument/2006/math");
		SerializeMath(officeMathLeftScript.Superscript);
		m_writer.WriteEndElement();
		m_writer.WriteStartElement("e", "http://schemas.openxmlformats.org/officeDocument/2006/math");
		SerializeMath(officeMathLeftScript.Equation);
		m_writer.WriteEndElement();
		m_writer.WriteEndElement();
	}

	private void SerializeMathLeftScriptProperties(OfficeMathLeftScript officeMathLeftScript)
	{
		m_writer.WriteStartElement("sPrePr", "http://schemas.openxmlformats.org/officeDocument/2006/math");
		if (officeMathLeftScript.ControlProperties != null)
		{
			m_writer.WriteStartElement("ctrlPr", "http://schemas.openxmlformats.org/officeDocument/2006/math");
			m_documentSerializer.SerializeControlProperties(officeMathLeftScript.ControlProperties);
			m_writer.WriteEndElement();
		}
		m_writer.WriteEndElement();
	}

	private void SerializeMathSuperScript(OfficeMathScript officeMathScript)
	{
		m_writer.WriteStartElement("sSup", "http://schemas.openxmlformats.org/officeDocument/2006/math");
		SerializeMathSuperScriptProperties(officeMathScript);
		m_writer.WriteStartElement("e", "http://schemas.openxmlformats.org/officeDocument/2006/math");
		SerializeMath(officeMathScript.Equation);
		m_writer.WriteEndElement();
		m_writer.WriteStartElement("sup", "http://schemas.openxmlformats.org/officeDocument/2006/math");
		SerializeMath(officeMathScript.Script);
		m_writer.WriteEndElement();
		m_writer.WriteEndElement();
	}

	private void SerializeMathSuperScriptProperties(OfficeMathScript officeMathScript)
	{
		m_writer.WriteStartElement("sSupPr", "http://schemas.openxmlformats.org/officeDocument/2006/math");
		if (officeMathScript.ControlProperties != null)
		{
			m_writer.WriteStartElement("ctrlPr", "http://schemas.openxmlformats.org/officeDocument/2006/math");
			m_documentSerializer.SerializeControlProperties(officeMathScript.ControlProperties);
			m_writer.WriteEndElement();
		}
		m_writer.WriteEndElement();
	}

	private void SerializeMathSubScript(OfficeMathScript officeMathScript)
	{
		m_writer.WriteStartElement("sSub", "http://schemas.openxmlformats.org/officeDocument/2006/math");
		SerializeMathSubScriptProperties(officeMathScript);
		m_writer.WriteStartElement("e", "http://schemas.openxmlformats.org/officeDocument/2006/math");
		SerializeMath(officeMathScript.Equation);
		m_writer.WriteEndElement();
		m_writer.WriteStartElement("sub", "http://schemas.openxmlformats.org/officeDocument/2006/math");
		SerializeMath(officeMathScript.Script);
		m_writer.WriteEndElement();
		m_writer.WriteEndElement();
	}

	private void SerializeMathSubScriptProperties(OfficeMathScript officeMathScript)
	{
		m_writer.WriteStartElement("sSubPr", "http://schemas.openxmlformats.org/officeDocument/2006/math");
		if (officeMathScript.ControlProperties != null)
		{
			m_writer.WriteStartElement("ctrlPr", "http://schemas.openxmlformats.org/officeDocument/2006/math");
			m_documentSerializer.SerializeControlProperties(officeMathScript.ControlProperties);
			m_writer.WriteEndElement();
		}
		m_writer.WriteEndElement();
	}

	private void SerializeMathText(OfficeMathRunElement officeMathParaItem)
	{
		m_writer.WriteStartElement("r", "http://schemas.openxmlformats.org/officeDocument/2006/math");
		m_documentSerializer.SerializeMathRun(officeMathParaItem);
		m_writer.WriteEndElement();
	}

	internal void SerializeMathRunFormat(XmlWriter writer, OfficeMathFormat mathFormat)
	{
		m_writer = writer;
		if (!mathFormat.IsDefault)
		{
			m_writer.WriteStartElement("rPr", "http://schemas.openxmlformats.org/officeDocument/2006/math");
			if (mathFormat.HasValue(53))
			{
				SerializeBoolProperty("aln", mathFormat.HasAlignment);
			}
			SerializeMathBreak((OfficeMathBreak)mathFormat.Break);
			if (mathFormat.HasValue(54))
			{
				SerializeBoolProperty("lit", mathFormat.HasLiteral);
			}
			if (mathFormat.HasValue(55))
			{
				SerializeBoolProperty("nor", mathFormat.HasNormalText);
			}
			if (mathFormat.HasValue(56))
			{
				SerializeMathRunFormatScript(mathFormat);
			}
			if (mathFormat.HasValue(57))
			{
				SerializeMathRunFormatStyle(mathFormat);
			}
			m_writer.WriteEndElement();
		}
	}

	private void SerializeMathRunFormatScript(OfficeMathFormat mathFormat)
	{
		m_writer.WriteStartElement("scr", "http://schemas.openxmlformats.org/officeDocument/2006/math");
		switch (mathFormat.Font)
		{
		case MathFontType.DoubleStruck:
			m_writer.WriteAttributeString("val", "http://schemas.openxmlformats.org/officeDocument/2006/math", "double-struck");
			break;
		case MathFontType.Fraktur:
			m_writer.WriteAttributeString("val", "http://schemas.openxmlformats.org/officeDocument/2006/math", "fraktur");
			break;
		case MathFontType.Monospace:
			m_writer.WriteAttributeString("val", "http://schemas.openxmlformats.org/officeDocument/2006/math", "monospace");
			break;
		case MathFontType.SansSerif:
			m_writer.WriteAttributeString("val", "http://schemas.openxmlformats.org/officeDocument/2006/math", "sans-serif");
			break;
		case MathFontType.Script:
			m_writer.WriteAttributeString("val", "http://schemas.openxmlformats.org/officeDocument/2006/math", "script");
			break;
		default:
			m_writer.WriteAttributeString("val", "http://schemas.openxmlformats.org/officeDocument/2006/math", "roman");
			break;
		}
		m_writer.WriteEndElement();
	}

	private void SerializeMathRunFormatStyle(OfficeMathFormat mathFormat)
	{
		m_writer.WriteStartElement("sty", "http://schemas.openxmlformats.org/officeDocument/2006/math");
		switch (mathFormat.Style)
		{
		case MathStyleType.Bold:
			m_writer.WriteAttributeString("val", "http://schemas.openxmlformats.org/officeDocument/2006/math", "b");
			break;
		case MathStyleType.BoldItalic:
			m_writer.WriteAttributeString("val", "http://schemas.openxmlformats.org/officeDocument/2006/math", "bi");
			break;
		case MathStyleType.Regular:
			m_writer.WriteAttributeString("val", "http://schemas.openxmlformats.org/officeDocument/2006/math", "p");
			break;
		default:
			m_writer.WriteAttributeString("val", "http://schemas.openxmlformats.org/officeDocument/2006/math", "i");
			break;
		}
		m_writer.WriteEndElement();
	}

	private void SerializeMathAccent(OfficeMathAccent mathAccent)
	{
		m_writer.WriteStartElement("acc", "http://schemas.openxmlformats.org/officeDocument/2006/math");
		SerializeMathAccentProperties(mathAccent);
		m_writer.WriteStartElement("e", "http://schemas.openxmlformats.org/officeDocument/2006/math");
		SerializeMath(mathAccent.Equation);
		m_writer.WriteEndElement();
		m_writer.WriteEndElement();
	}

	private void SerializeMathAccentProperties(OfficeMathAccent mathAccent)
	{
		m_writer.WriteStartElement("accPr", "http://schemas.openxmlformats.org/officeDocument/2006/math");
		if (mathAccent.HasValue(1))
		{
			m_writer.WriteStartElement("chr", "http://schemas.openxmlformats.org/officeDocument/2006/math");
			m_writer.WriteAttributeString("val", "http://schemas.openxmlformats.org/officeDocument/2006/math", mathAccent.AccentCharacter);
			m_writer.WriteEndElement();
		}
		if (mathAccent.ControlProperties != null)
		{
			m_writer.WriteStartElement("ctrlPr", "http://schemas.openxmlformats.org/officeDocument/2006/math");
			m_documentSerializer.SerializeControlProperties(mathAccent.ControlProperties);
			m_writer.WriteEndElement();
		}
		m_writer.WriteEndElement();
	}

	private void SerializeMathBar(OfficeMathBar mathBar)
	{
		m_writer.WriteStartElement("bar", "http://schemas.openxmlformats.org/officeDocument/2006/math");
		SerializeMathBarProperties(mathBar);
		m_writer.WriteStartElement("e", "http://schemas.openxmlformats.org/officeDocument/2006/math");
		SerializeMath(mathBar.Equation);
		m_writer.WriteEndElement();
		m_writer.WriteEndElement();
	}

	private void SerializeMathBarProperties(OfficeMathBar mathBar)
	{
		m_writer.WriteStartElement("barPr", "http://schemas.openxmlformats.org/officeDocument/2006/math");
		if (mathBar.HasValue(2))
		{
			m_writer.WriteStartElement("pos", "http://schemas.openxmlformats.org/officeDocument/2006/math");
			m_writer.WriteAttributeString("val", "http://schemas.openxmlformats.org/officeDocument/2006/math", (!mathBar.BarTop) ? "bot" : "top");
			m_writer.WriteEndElement();
		}
		if (mathBar.ControlProperties != null)
		{
			m_writer.WriteStartElement("ctrlPr", "http://schemas.openxmlformats.org/officeDocument/2006/math");
			m_documentSerializer.SerializeControlProperties(mathBar.ControlProperties);
			m_writer.WriteEndElement();
		}
		m_writer.WriteEndElement();
	}

	private void SerializeMathBox(OfficeMathBox mathBox)
	{
		m_writer.WriteStartElement("box", "http://schemas.openxmlformats.org/officeDocument/2006/math");
		SerializeMathBoxProperties(mathBox);
		m_writer.WriteStartElement("e", "http://schemas.openxmlformats.org/officeDocument/2006/math");
		SerializeMath(mathBox.Equation);
		m_writer.WriteEndElement();
		m_writer.WriteEndElement();
	}

	private void SerializeMathBoxProperties(OfficeMathBox mathBox)
	{
		m_writer.WriteStartElement("boxPr", "http://schemas.openxmlformats.org/officeDocument/2006/math");
		if (mathBox.HasValue(11))
		{
			SerializeBoolProperty("aln", mathBox.Alignment);
		}
		if (mathBox.HasValue(14))
		{
			SerializeBoolProperty("diff", mathBox.EnableDifferential);
		}
		if (mathBox.HasValue(15))
		{
			SerializeBoolProperty("opEmu", mathBox.OperatorEmulator);
		}
		if (mathBox.HasValue(12))
		{
			SerializeBoolProperty("noBreak", mathBox.NoBreak);
		}
		SerializeMathBreak((OfficeMathBreak)mathBox.Break);
		if (mathBox.ControlProperties != null)
		{
			m_writer.WriteStartElement("ctrlPr", "http://schemas.openxmlformats.org/officeDocument/2006/math");
			m_documentSerializer.SerializeControlProperties(mathBox.ControlProperties);
			m_writer.WriteEndElement();
		}
		m_writer.WriteEndElement();
	}

	private void SerializeMathBreak(OfficeMathBreak mathBreak)
	{
		if (mathBreak != null)
		{
			m_writer.WriteStartElement("brk", "http://schemas.openxmlformats.org/officeDocument/2006/math");
			if (mathBreak.HasValue(16))
			{
				m_writer.WriteAttributeString("alnAt", "http://schemas.openxmlformats.org/officeDocument/2006/math", ToString(mathBreak.AlignAt));
			}
			m_writer.WriteEndElement();
		}
	}

	private void SerializeMathBorderBox(OfficeMathBorderBox mathBorderBox)
	{
		m_writer.WriteStartElement("borderBox", "http://schemas.openxmlformats.org/officeDocument/2006/math");
		SerializeMathBorderBoxProperties(mathBorderBox);
		m_writer.WriteStartElement("e", "http://schemas.openxmlformats.org/officeDocument/2006/math");
		SerializeMath(mathBorderBox.Equation);
		m_writer.WriteEndElement();
		m_writer.WriteEndElement();
	}

	private void SerializeMathBorderBoxProperties(OfficeMathBorderBox mathBorderBox)
	{
		m_writer.WriteStartElement("borderBoxPr", "http://schemas.openxmlformats.org/officeDocument/2006/math");
		if (mathBorderBox.HasValue(4))
		{
			SerializeBoolProperty("hideBot", mathBorderBox.HideBottom);
		}
		if (mathBorderBox.HasValue(6))
		{
			SerializeBoolProperty("hideLeft", mathBorderBox.HideLeft);
		}
		if (mathBorderBox.HasValue(5))
		{
			SerializeBoolProperty("hideRight", mathBorderBox.HideRight);
		}
		if (mathBorderBox.HasValue(3))
		{
			SerializeBoolProperty("hideTop", mathBorderBox.HideTop);
		}
		if (mathBorderBox.HasValue(7))
		{
			SerializeBoolProperty("strikeBLTR", mathBorderBox.StrikeDiagonalUp);
		}
		if (mathBorderBox.HasValue(10))
		{
			SerializeBoolProperty("strikeH", mathBorderBox.StrikeHorizontal);
		}
		if (mathBorderBox.HasValue(8))
		{
			SerializeBoolProperty("strikeTLBR", mathBorderBox.StrikeDiagonalDown);
		}
		if (mathBorderBox.HasValue(9))
		{
			SerializeBoolProperty("strikeV", mathBorderBox.StrikeVertical);
		}
		if (mathBorderBox.ControlProperties != null)
		{
			m_writer.WriteStartElement("ctrlPr", "http://schemas.openxmlformats.org/officeDocument/2006/math");
			m_documentSerializer.SerializeControlProperties(mathBorderBox.ControlProperties);
			m_writer.WriteEndElement();
		}
		m_writer.WriteEndElement();
	}

	private void SerializeMathDelimiter(OfficeMathDelimiter mathDelimiter)
	{
		m_writer.WriteStartElement("d", "http://schemas.openxmlformats.org/officeDocument/2006/math");
		SerializeMathDelimiterProperties(mathDelimiter);
		for (int i = 0; i < mathDelimiter.Equation.Count; i++)
		{
			m_writer.WriteStartElement("e", "http://schemas.openxmlformats.org/officeDocument/2006/math");
			SerializeMath(mathDelimiter.Equation[i]);
			m_writer.WriteEndElement();
		}
		m_writer.WriteEndElement();
	}

	private void SerializeMathDelimiterProperties(OfficeMathDelimiter mathDelimiter)
	{
		m_writer.WriteStartElement("dPr", "http://schemas.openxmlformats.org/officeDocument/2006/math");
		if (mathDelimiter.HasValue(17))
		{
			m_writer.WriteStartElement("begChr", "http://schemas.openxmlformats.org/officeDocument/2006/math");
			m_writer.WriteAttributeString("val", "http://schemas.openxmlformats.org/officeDocument/2006/math", mathDelimiter.BeginCharacter);
			m_writer.WriteEndElement();
		}
		if (mathDelimiter.HasValue(18))
		{
			m_writer.WriteStartElement("endChr", "http://schemas.openxmlformats.org/officeDocument/2006/math");
			m_writer.WriteAttributeString("val", "http://schemas.openxmlformats.org/officeDocument/2006/math", mathDelimiter.EndCharacter);
			m_writer.WriteEndElement();
		}
		if (mathDelimiter.HasValue(21))
		{
			m_writer.WriteStartElement("sepChr", "http://schemas.openxmlformats.org/officeDocument/2006/math");
			m_writer.WriteAttributeString("val", "http://schemas.openxmlformats.org/officeDocument/2006/math", mathDelimiter.Seperator);
			m_writer.WriteEndElement();
		}
		if (mathDelimiter.HasValue(22))
		{
			SerializeBoolProperty("grow", mathDelimiter.IsGrow);
		}
		if (mathDelimiter.HasValue(23))
		{
			SerializeMathDelimiterShape(mathDelimiter);
		}
		if (mathDelimiter.ControlProperties != null)
		{
			m_writer.WriteStartElement("ctrlPr", "http://schemas.openxmlformats.org/officeDocument/2006/math");
			m_documentSerializer.SerializeControlProperties(mathDelimiter.ControlProperties);
			m_writer.WriteEndElement();
		}
		m_writer.WriteEndElement();
	}

	private void SerializeMathDelimiterShape(OfficeMathDelimiter mathDelimiter)
	{
		m_writer.WriteStartElement("shp", "http://schemas.openxmlformats.org/officeDocument/2006/math");
		if (mathDelimiter.DelimiterShape == MathDelimiterShapeType.Match)
		{
			m_writer.WriteAttributeString("val", "http://schemas.openxmlformats.org/officeDocument/2006/math", "match");
		}
		else
		{
			m_writer.WriteAttributeString("val", "http://schemas.openxmlformats.org/officeDocument/2006/math", "centered");
		}
		m_writer.WriteEndElement();
	}

	private void SerializeMathEqArray(OfficeMathEquationArray mathEqArray)
	{
		m_writer.WriteStartElement("eqArr", "http://schemas.openxmlformats.org/officeDocument/2006/math");
		SerializeMathEqArrayProperties(mathEqArray);
		for (int i = 0; i < mathEqArray.Equation.Count; i++)
		{
			m_writer.WriteStartElement("e", "http://schemas.openxmlformats.org/officeDocument/2006/math");
			SerializeMath(mathEqArray.Equation[i]);
			m_writer.WriteEndElement();
		}
		m_writer.WriteEndElement();
	}

	private void SerializeMathEqArrayProperties(OfficeMathEquationArray mathEqArray)
	{
		m_writer.WriteStartElement("eqArrPr", "http://schemas.openxmlformats.org/officeDocument/2006/math");
		if (mathEqArray.HasValue(24))
		{
			SerializeMathEqArrayAlignment(mathEqArray);
		}
		if (mathEqArray.HasValue(25))
		{
			SerializeBoolProperty("maxDist", mathEqArray.ExpandEquationContainer);
		}
		if (mathEqArray.HasValue(26))
		{
			SerializeBoolProperty("objDist", mathEqArray.ExpandEquationContent);
		}
		if (mathEqArray.HasValue(27) && (mathEqArray.RowSpacingRule == SpacingRule.Exactly || mathEqArray.RowSpacingRule == SpacingRule.Multiple))
		{
			SerializeRowSpacing(mathEqArray.RowSpacing, mathEqArray.RowSpacingRule);
		}
		if (mathEqArray.HasValue(28))
		{
			m_writer.WriteStartElement("rSpRule", "http://schemas.openxmlformats.org/officeDocument/2006/math");
			SerializeSpacingRule(mathEqArray.RowSpacingRule);
			m_writer.WriteEndElement();
		}
		if (mathEqArray.ControlProperties != null)
		{
			m_writer.WriteStartElement("ctrlPr", "http://schemas.openxmlformats.org/officeDocument/2006/math");
			m_documentSerializer.SerializeControlProperties(mathEqArray.ControlProperties);
			m_writer.WriteEndElement();
		}
		m_writer.WriteEndElement();
	}

	private void SerializeSpacingRule(SpacingRule spacingRule)
	{
		switch (spacingRule)
		{
		case SpacingRule.OneAndHalf:
			m_writer.WriteAttributeString("val", "http://schemas.openxmlformats.org/officeDocument/2006/math", "1");
			break;
		case SpacingRule.Double:
			m_writer.WriteAttributeString("val", "http://schemas.openxmlformats.org/officeDocument/2006/math", "2");
			break;
		case SpacingRule.Exactly:
			m_writer.WriteAttributeString("val", "http://schemas.openxmlformats.org/officeDocument/2006/math", "3");
			break;
		case SpacingRule.Multiple:
			m_writer.WriteAttributeString("val", "http://schemas.openxmlformats.org/officeDocument/2006/math", "4");
			break;
		default:
			m_writer.WriteAttributeString("val", "http://schemas.openxmlformats.org/officeDocument/2006/math", "0");
			break;
		}
	}

	private void SerializeMathEqArrayAlignment(OfficeMathEquationArray mathEqArray)
	{
		m_writer.WriteStartElement("baseJc", "http://schemas.openxmlformats.org/officeDocument/2006/math");
		switch (mathEqArray.VerticalAlignment)
		{
		case MathVerticalAlignment.Top:
			m_writer.WriteAttributeString("val", "http://schemas.openxmlformats.org/officeDocument/2006/math", "top");
			break;
		case MathVerticalAlignment.Bottom:
			m_writer.WriteAttributeString("val", "http://schemas.openxmlformats.org/officeDocument/2006/math", "bottom");
			break;
		default:
			m_writer.WriteAttributeString("val", "http://schemas.openxmlformats.org/officeDocument/2006/math", "centered");
			break;
		}
		m_writer.WriteEndElement();
	}

	private void SerializeMathFraction(OfficeMathFraction mathFraction)
	{
		m_writer.WriteStartElement("f", "http://schemas.openxmlformats.org/officeDocument/2006/math");
		SerializeMathFractionProperties(mathFraction);
		m_writer.WriteStartElement("num", "http://schemas.openxmlformats.org/officeDocument/2006/math");
		SerializeMath(mathFraction.Numerator);
		m_writer.WriteEndElement();
		m_writer.WriteStartElement("den", "http://schemas.openxmlformats.org/officeDocument/2006/math");
		SerializeMath(mathFraction.Denominator);
		m_writer.WriteEndElement();
		m_writer.WriteEndElement();
	}

	private void SerializeMathFractionProperties(OfficeMathFraction mathFraction)
	{
		m_writer.WriteStartElement("fPr", "http://schemas.openxmlformats.org/officeDocument/2006/math");
		if (mathFraction.HasValue(29))
		{
			SerializeMathFractionType(mathFraction);
		}
		if (mathFraction.ControlProperties != null)
		{
			m_writer.WriteStartElement("ctrlPr", "http://schemas.openxmlformats.org/officeDocument/2006/math");
			m_documentSerializer.SerializeControlProperties(mathFraction.ControlProperties);
			m_writer.WriteEndElement();
		}
		m_writer.WriteEndElement();
	}

	private void SerializeMathFractionType(OfficeMathFraction mathFraction)
	{
		m_writer.WriteStartElement("type", "http://schemas.openxmlformats.org/officeDocument/2006/math");
		switch (mathFraction.FractionType)
		{
		case MathFractionType.NoFractionBar:
			m_writer.WriteAttributeString("val", "http://schemas.openxmlformats.org/officeDocument/2006/math", "noBar");
			break;
		case MathFractionType.SkewedFractionBar:
			m_writer.WriteAttributeString("val", "http://schemas.openxmlformats.org/officeDocument/2006/math", "skw");
			break;
		case MathFractionType.FractionInline:
			m_writer.WriteAttributeString("val", "http://schemas.openxmlformats.org/officeDocument/2006/math", "lin");
			break;
		default:
			m_writer.WriteAttributeString("val", "http://schemas.openxmlformats.org/officeDocument/2006/math", "bar");
			break;
		}
		m_writer.WriteEndElement();
	}

	private void SerializeMathFunc(OfficeMathFunction mathFunc)
	{
		m_writer.WriteStartElement("func", "http://schemas.openxmlformats.org/officeDocument/2006/math");
		SerializeMathFuncProperties(mathFunc);
		m_writer.WriteStartElement("fName", "http://schemas.openxmlformats.org/officeDocument/2006/math");
		SerializeMath(mathFunc.FunctionName);
		m_writer.WriteEndElement();
		m_writer.WriteStartElement("e", "http://schemas.openxmlformats.org/officeDocument/2006/math");
		SerializeMath(mathFunc.Equation);
		m_writer.WriteEndElement();
		m_writer.WriteEndElement();
	}

	private void SerializeMathFuncProperties(OfficeMathFunction mathFunc)
	{
		m_writer.WriteStartElement("funcPr", "http://schemas.openxmlformats.org/officeDocument/2006/math");
		if (mathFunc.ControlProperties != null)
		{
			m_writer.WriteStartElement("ctrlPr", "http://schemas.openxmlformats.org/officeDocument/2006/math");
			m_documentSerializer.SerializeControlProperties(mathFunc.ControlProperties);
			m_writer.WriteEndElement();
		}
		m_writer.WriteEndElement();
	}

	private void SerializeMathGroupChar(OfficeMathGroupCharacter mathGroupChar)
	{
		m_writer.WriteStartElement("groupChr", "http://schemas.openxmlformats.org/officeDocument/2006/math");
		SerializeMathGroupCharProperties(mathGroupChar);
		m_writer.WriteStartElement("e", "http://schemas.openxmlformats.org/officeDocument/2006/math");
		SerializeMath(mathGroupChar.Equation);
		m_writer.WriteEndElement();
		m_writer.WriteEndElement();
	}

	private void SerializeMathGroupCharProperties(OfficeMathGroupCharacter mathGroupChar)
	{
		m_writer.WriteStartElement("groupChrPr", "http://schemas.openxmlformats.org/officeDocument/2006/math");
		if (mathGroupChar.HasValue(30))
		{
			m_writer.WriteStartElement("vertJc", "http://schemas.openxmlformats.org/officeDocument/2006/math");
			if (!mathGroupChar.HasAlignTop)
			{
				m_writer.WriteAttributeString("val", "http://schemas.openxmlformats.org/officeDocument/2006/math", "bot");
			}
			else
			{
				m_writer.WriteAttributeString("val", "http://schemas.openxmlformats.org/officeDocument/2006/math", "top");
			}
			m_writer.WriteEndElement();
		}
		if (mathGroupChar.HasValue(32))
		{
			m_writer.WriteStartElement("pos", "http://schemas.openxmlformats.org/officeDocument/2006/math");
			if (!mathGroupChar.HasCharacterTop)
			{
				m_writer.WriteAttributeString("val", "http://schemas.openxmlformats.org/officeDocument/2006/math", "bot");
			}
			else
			{
				m_writer.WriteAttributeString("val", "http://schemas.openxmlformats.org/officeDocument/2006/math", "top");
			}
			m_writer.WriteEndElement();
		}
		if (mathGroupChar.HasValue(31))
		{
			m_writer.WriteStartElement("chr", "http://schemas.openxmlformats.org/officeDocument/2006/math");
			m_writer.WriteAttributeString("val", "http://schemas.openxmlformats.org/officeDocument/2006/math", mathGroupChar.GroupCharacter);
			m_writer.WriteEndElement();
		}
		if (mathGroupChar.ControlProperties != null)
		{
			m_writer.WriteStartElement("ctrlPr", "http://schemas.openxmlformats.org/officeDocument/2006/math");
			m_documentSerializer.SerializeControlProperties(mathGroupChar.ControlProperties);
			m_writer.WriteEndElement();
		}
		m_writer.WriteEndElement();
	}

	private void SerializeMathMatrix(OfficeMathMatrix mathMatrix)
	{
		m_writer.WriteStartElement("m", "http://schemas.openxmlformats.org/officeDocument/2006/math");
		SerializeMathMatrixProperties(mathMatrix);
		for (int i = 0; i < mathMatrix.Rows.Count; i++)
		{
			m_writer.WriteStartElement("mr", "http://schemas.openxmlformats.org/officeDocument/2006/math");
			SerializeMathMatrixRow(mathMatrix.Rows[i] as OfficeMathMatrixRow);
			m_writer.WriteEndElement();
		}
		m_writer.WriteEndElement();
	}

	private void SerializeMathMatrixProperties(OfficeMathMatrix mathMatrix)
	{
		m_writer.WriteStartElement("mPr", "http://schemas.openxmlformats.org/officeDocument/2006/math");
		if (mathMatrix.HasValue(33))
		{
			SerializeMathMatrixAlign(mathMatrix);
		}
		if (mathMatrix.HasValue(37))
		{
			SerializeBoolProperty("plcHide", mathMatrix.HidePlaceHolders);
		}
		if (mathMatrix.HasValue(28))
		{
			m_writer.WriteStartElement("rSpRule", "http://schemas.openxmlformats.org/officeDocument/2006/math");
			SerializeSpacingRule(mathMatrix.RowSpacingRule);
			m_writer.WriteEndElement();
		}
		if (mathMatrix.HasValue(34))
		{
			m_writer.WriteStartElement("cSp", "http://schemas.openxmlformats.org/officeDocument/2006/math");
			m_writer.WriteAttributeString("val", "http://schemas.openxmlformats.org/officeDocument/2006/math", ToString(mathMatrix.ColumnWidth * 20f));
			m_writer.WriteEndElement();
		}
		if (mathMatrix.HasValue(35))
		{
			m_writer.WriteStartElement("cGpRule", "http://schemas.openxmlformats.org/officeDocument/2006/math");
			SerializeSpacingRule(mathMatrix.ColumnSpacingRule);
			m_writer.WriteEndElement();
		}
		if (mathMatrix.HasValue(27) && (mathMatrix.RowSpacingRule == SpacingRule.Exactly || mathMatrix.RowSpacingRule == SpacingRule.Multiple))
		{
			SerializeRowSpacing(mathMatrix.RowSpacing, mathMatrix.RowSpacingRule);
		}
		if (mathMatrix.HasValue(36) && (mathMatrix.ColumnSpacingRule == SpacingRule.Exactly || mathMatrix.ColumnSpacingRule == SpacingRule.Multiple))
		{
			string empty = string.Empty;
			empty = ((mathMatrix.ColumnSpacingRule != SpacingRule.Exactly) ? ToString((float)Math.Floor(mathMatrix.ColumnSpacing / 6f)) : ToString(mathMatrix.ColumnSpacing * 20f));
			m_writer.WriteStartElement("cGp", "http://schemas.openxmlformats.org/officeDocument/2006/math");
			m_writer.WriteAttributeString("val", "http://schemas.openxmlformats.org/officeDocument/2006/math", empty);
			m_writer.WriteEndElement();
		}
		SerializeMathMatrixColumns(mathMatrix);
		if (mathMatrix.ControlProperties != null)
		{
			m_writer.WriteStartElement("ctrlPr", "http://schemas.openxmlformats.org/officeDocument/2006/math");
			m_documentSerializer.SerializeControlProperties(mathMatrix.ControlProperties);
			m_writer.WriteEndElement();
		}
		m_writer.WriteEndElement();
	}

	private void SerializeRowSpacing(float rowSpacing, SpacingRule spacingRule)
	{
		string empty = string.Empty;
		empty = ((spacingRule != SpacingRule.Exactly) ? ToString(rowSpacing * 2f) : ToString(rowSpacing * 20f));
		m_writer.WriteStartElement("rSp", "http://schemas.openxmlformats.org/officeDocument/2006/math");
		m_writer.WriteAttributeString("val", "http://schemas.openxmlformats.org/officeDocument/2006/math", empty);
		m_writer.WriteEndElement();
	}

	private void SerializeMathMatrixAlign(OfficeMathMatrix mathMatrix)
	{
		m_writer.WriteStartElement("baseJc", "http://schemas.openxmlformats.org/officeDocument/2006/math");
		switch (mathMatrix.VerticalAlignment)
		{
		case MathVerticalAlignment.Top:
			m_writer.WriteAttributeString("val", "http://schemas.openxmlformats.org/officeDocument/2006/math", "top");
			break;
		case MathVerticalAlignment.Bottom:
			m_writer.WriteAttributeString("val", "http://schemas.openxmlformats.org/officeDocument/2006/math", "bottom");
			break;
		default:
			m_writer.WriteAttributeString("val", "http://schemas.openxmlformats.org/officeDocument/2006/math", "centered");
			break;
		}
		m_writer.WriteEndElement();
	}

	private void SerializeMathMatrixColumns(OfficeMathMatrix mathMatrix)
	{
		m_writer.WriteStartElement("mcs", "http://schemas.openxmlformats.org/officeDocument/2006/math");
		for (int i = 0; i < mathMatrix.ColumnProperties.Count; i++)
		{
			m_writer.WriteStartElement("mc", "http://schemas.openxmlformats.org/officeDocument/2006/math");
			SerializeMathMatrixColumnProperties(mathMatrix.ColumnProperties[i]);
			m_writer.WriteEndElement();
		}
		m_writer.WriteEndElement();
	}

	private void SerializeMathMatrixColumnProperties(MatrixColumnProperties mathMatrixColumnProperties)
	{
		m_writer.WriteStartElement("mcPr", "http://schemas.openxmlformats.org/officeDocument/2006/math");
		m_writer.WriteStartElement("count", "http://schemas.openxmlformats.org/officeDocument/2006/math");
		m_writer.WriteAttributeString("val", "http://schemas.openxmlformats.org/officeDocument/2006/math", ToString(mathMatrixColumnProperties.Count));
		m_writer.WriteEndElement();
		SerializeMathMatrixColumnAlign(mathMatrixColumnProperties);
		m_writer.WriteEndElement();
	}

	private void SerializeMathMatrixColumnAlign(MatrixColumnProperties mathMatrixColumnProperties)
	{
		m_writer.WriteStartElement("mcJc", "http://schemas.openxmlformats.org/officeDocument/2006/math");
		switch (mathMatrixColumnProperties.Alignment)
		{
		case MathHorizontalAlignment.Left:
			m_writer.WriteAttributeString("val", "http://schemas.openxmlformats.org/officeDocument/2006/math", "left");
			break;
		case MathHorizontalAlignment.Right:
			m_writer.WriteAttributeString("val", "http://schemas.openxmlformats.org/officeDocument/2006/math", "right");
			break;
		default:
			m_writer.WriteAttributeString("val", "http://schemas.openxmlformats.org/officeDocument/2006/math", "center");
			break;
		}
		m_writer.WriteEndElement();
	}

	private void SerializeMathMatrixRow(OfficeMathMatrixRow mathMatrixRow)
	{
		for (int i = 0; i < mathMatrixRow.Arguments.Count; i++)
		{
			m_writer.WriteStartElement("e", "http://schemas.openxmlformats.org/officeDocument/2006/math");
			SerializeMath(mathMatrixRow.Arguments[i]);
			m_writer.WriteEndElement();
		}
	}

	private void SerializeMathNAry(OfficeMathNArray mathNAry)
	{
		m_writer.WriteStartElement("nary", "http://schemas.openxmlformats.org/officeDocument/2006/math");
		SerializeMathNAryProperties(mathNAry);
		m_writer.WriteStartElement("sub", "http://schemas.openxmlformats.org/officeDocument/2006/math");
		SerializeMath(mathNAry.Subscript);
		m_writer.WriteEndElement();
		m_writer.WriteStartElement("sup", "http://schemas.openxmlformats.org/officeDocument/2006/math");
		SerializeMath(mathNAry.Superscript);
		m_writer.WriteEndElement();
		m_writer.WriteStartElement("e", "http://schemas.openxmlformats.org/officeDocument/2006/math");
		SerializeMath(mathNAry.Equation);
		m_writer.WriteEndElement();
		m_writer.WriteEndElement();
	}

	private void SerializeMathNAryProperties(OfficeMathNArray mathNAry)
	{
		m_writer.WriteStartElement("naryPr", "http://schemas.openxmlformats.org/officeDocument/2006/math");
		if (mathNAry.HasValue(41))
		{
			m_writer.WriteStartElement("chr", "http://schemas.openxmlformats.org/officeDocument/2006/math");
			m_writer.WriteAttributeString("val", "http://schemas.openxmlformats.org/officeDocument/2006/math", mathNAry.NArrayCharacter);
			m_writer.WriteEndElement();
		}
		if (mathNAry.HasValue(42))
		{
			SerializeBoolProperty("grow", mathNAry.HasGrow);
		}
		if (mathNAry.HasValue(45))
		{
			m_writer.WriteStartElement("limLoc", "http://schemas.openxmlformats.org/officeDocument/2006/math");
			m_writer.WriteAttributeString("val", "http://schemas.openxmlformats.org/officeDocument/2006/math", (!mathNAry.SubSuperscriptLimit) ? "undOvr" : "subSup");
			m_writer.WriteEndElement();
		}
		if (mathNAry.HasValue(43))
		{
			SerializeBoolProperty("subHide", mathNAry.HideLowerLimit);
		}
		if (mathNAry.HasValue(44))
		{
			SerializeBoolProperty("supHide", mathNAry.HideUpperLimit);
		}
		if (mathNAry.ControlProperties != null)
		{
			m_writer.WriteStartElement("ctrlPr", "http://schemas.openxmlformats.org/officeDocument/2006/math");
			m_documentSerializer.SerializeControlProperties(mathNAry.ControlProperties);
			m_writer.WriteEndElement();
		}
		m_writer.WriteEndElement();
	}

	private void SerializeMathRadical(OfficeMathRadical mathRadical)
	{
		m_writer.WriteStartElement("rad", "http://schemas.openxmlformats.org/officeDocument/2006/math");
		SerializeMathRadicalProperties(mathRadical);
		m_writer.WriteStartElement("deg", "http://schemas.openxmlformats.org/officeDocument/2006/math");
		SerializeMath(mathRadical.Degree);
		m_writer.WriteEndElement();
		m_writer.WriteStartElement("e", "http://schemas.openxmlformats.org/officeDocument/2006/math");
		SerializeMath(mathRadical.Equation);
		m_writer.WriteEndElement();
		m_writer.WriteEndElement();
	}

	private void SerializeMathRadicalProperties(OfficeMathRadical mathRadical)
	{
		m_writer.WriteStartElement("radPr", "http://schemas.openxmlformats.org/officeDocument/2006/math");
		if (mathRadical.HasValue(51))
		{
			SerializeBoolProperty("degHide", mathRadical.HideDegree);
		}
		if (mathRadical.ControlProperties != null)
		{
			m_writer.WriteStartElement("ctrlPr", "http://schemas.openxmlformats.org/officeDocument/2006/math");
			m_documentSerializer.SerializeControlProperties(mathRadical.ControlProperties);
			m_writer.WriteEndElement();
		}
		m_writer.WriteEndElement();
	}

	private void SerializeMathPhantom(OfficeMathPhantom mathPhantom)
	{
		m_writer.WriteStartElement("phant", "http://schemas.openxmlformats.org/officeDocument/2006/math");
		if (!mathPhantom.IsDefault)
		{
			SerializeMathPhantomProperties(mathPhantom);
		}
		m_writer.WriteStartElement("e", "http://schemas.openxmlformats.org/officeDocument/2006/math");
		SerializeMath(mathPhantom.Equation);
		m_writer.WriteEndElement();
		m_writer.WriteEndElement();
	}

	private void SerializeMathPhantomProperties(OfficeMathPhantom mathPhantom)
	{
		m_writer.WriteStartElement("phantPr", "http://schemas.openxmlformats.org/officeDocument/2006/math");
		if (mathPhantom.HasValue(46))
		{
			SerializeBoolProperty("show", mathPhantom.Show);
		}
		if (mathPhantom.HasValue(49))
		{
			SerializeBoolProperty("zeroAsc", mathPhantom.ZeroAscent);
		}
		if (mathPhantom.HasValue(50))
		{
			SerializeBoolProperty("zeroDesc", mathPhantom.ZeroDescent);
		}
		if (mathPhantom.HasValue(48))
		{
			SerializeBoolProperty("transp", mathPhantom.Transparent);
		}
		if (mathPhantom.HasValue(51))
		{
			SerializeBoolProperty("zeroWid", mathPhantom.ZeroWidth);
		}
		if (mathPhantom.ControlProperties != null)
		{
			m_writer.WriteStartElement("ctrlPr", "http://schemas.openxmlformats.org/officeDocument/2006/math");
			m_documentSerializer.SerializeControlProperties(mathPhantom.ControlProperties);
			m_writer.WriteEndElement();
		}
		m_writer.WriteEndElement();
	}

	private void SerializeBoolProperty(string tag, bool value)
	{
		m_writer.WriteStartElement(tag, "http://schemas.openxmlformats.org/officeDocument/2006/math");
		if (!value)
		{
			m_writer.WriteAttributeString("val", "http://schemas.openxmlformats.org/officeDocument/2006/math", "0");
		}
		m_writer.WriteEndElement();
	}

	internal string ToString(float value)
	{
		return ((int)Math.Round(value)).ToString(CultureInfo.InvariantCulture);
	}
}
