using System.Text;
using DocGen.DocIO.DLS.Rendering;
using DocGen.DocIO.Rendering;
using DocGen.Drawing;
using DocGen.Layouting;
using DocGen.Office;

namespace DocGen.DocIO.DLS;

public class WMath : ParagraphItem, ILeafWidget, IWidget
{
	private OfficeMathParagraph m_mathPara;

	private new IWordDocument m_doc;

	public IOfficeMathParagraph MathParagraph => m_mathPara;

	public override EntityType EntityType => EntityType.Math;

	public bool IsInline => CheckMathIsInline();

	public WMath(IWordDocument doc)
		: base((WordDocument)doc)
	{
		m_mathPara = new OfficeMathParagraph(this);
		m_mathPara.DefaultMathCharacterFormat = new WCharacterFormat(doc, this);
		(m_mathPara.DefaultMathCharacterFormat as WCharacterFormat).FontName = "Cambria Math";
		(m_mathPara.DefaultMathCharacterFormat as WCharacterFormat).Italic = true;
		m_doc = doc;
		m_mathPara.m_documentLaTeXConverter = (m_doc as WordDocument).DocxLaTeXConveter;
	}

	protected override object CloneImpl()
	{
		WMath wMath = (WMath)base.CloneImpl();
		wMath.m_mathPara = m_mathPara.Clone();
		(wMath.MathParagraph as OfficeMathParagraph).SetOwner(wMath);
		return wMath;
	}

	internal override void Close()
	{
		if (m_mathPara != null)
		{
			m_mathPara.Close();
			m_mathPara = null;
		}
		base.Close();
	}

	public void ChangeToDisplay()
	{
		if (!IsInline)
		{
			return;
		}
		ParagraphItemCollection paragraphItemCollection = ((base.Owner is WParagraph) ? (base.Owner as WParagraph).Items : ((base.Owner is InlineContentControl) ? (base.Owner as InlineContentControl).ParagraphItems : null));
		int num = paragraphItemCollection.IndexOf(this);
		bool flag = HasRenderableItemBeforeMath(num, paragraphItemCollection);
		bool flag2 = HasRenderableItemAfterMath(num, paragraphItemCollection);
		if (flag && !flag2)
		{
			Break entity = new Break(m_doc);
			paragraphItemCollection.Insert(num, entity);
		}
		else if (!flag && flag2)
		{
			bool flag3 = false;
			int num2 = -1;
			for (int i = num + 1; i < paragraphItemCollection.Count; i++)
			{
				if (!IsRenderableItem(paragraphItemCollection[i]))
				{
					continue;
				}
				if (paragraphItemCollection[i] is WTextRange { Text: " " })
				{
					num2 = i;
					for (int j = i + 1; j < paragraphItemCollection.Count; j++)
					{
						flag3 = IsRenderableItem(paragraphItemCollection[j]);
						if (flag3)
						{
							break;
						}
					}
					continue;
				}
				flag3 = true;
				break;
			}
			if (!flag3 && num2 != -1)
			{
				paragraphItemCollection[num2].RemoveSelf();
				return;
			}
			Break entity2 = new Break(m_doc);
			paragraphItemCollection.Insert(num + 1, entity2);
		}
		else if (flag)
		{
			Break entity3 = new Break(m_doc);
			paragraphItemCollection.Insert(num + 1, entity3);
			entity3 = new Break(m_doc);
			paragraphItemCollection.Insert(num, entity3);
		}
	}

	internal byte[] GetAsImage()
	{
		try
		{
			DocumentLayouter documentLayouter = new DocumentLayouter();
			byte[] result = documentLayouter.ConvertAsImage(this);
			documentLayouter.Close();
			return result;
		}
		catch
		{
			return null;
		}
	}

	internal bool IsRenderableItem(ParagraphItem item)
	{
		if (item.EntityType == EntityType.BookmarkStart || item.EntityType == EntityType.BookmarkEnd || item.EntityType == EntityType.EditableRangeStart || item.EntityType == EntityType.EditableRangeEnd)
		{
			return false;
		}
		return true;
	}

	public void ChangeToInline()
	{
		if (!IsInline)
		{
			ParagraphItemCollection paragraphItemCollection = ((base.Owner is WParagraph) ? (base.Owner as WParagraph).Items : ((base.Owner is InlineContentControl) ? (base.Owner as InlineContentControl).ParagraphItems : null));
			int num = paragraphItemCollection.IndexOf(this);
			bool flag = HasRenderableItemBeforeMath(num, paragraphItemCollection);
			bool flag2 = HasRenderableItemAfterMath(num, paragraphItemCollection);
			if (!flag && !flag2)
			{
				AddEmptyTextRange(paragraphItemCollection, num + 1);
				return;
			}
			if (flag && !flag2)
			{
				RemovePreviousBreak(num, paragraphItemCollection);
				return;
			}
			if (!flag)
			{
				RemoveNextBreak(paragraphItemCollection);
				return;
			}
			RemovePreviousBreak(num, paragraphItemCollection);
			RemoveNextBreak(paragraphItemCollection);
		}
	}

	private bool HasRenderableItemAfterMath(int mathIndex, ParagraphItemCollection paraItems)
	{
		bool flag = false;
		int index = MathParagraph.Maths.Count - 1;
		int num = MathParagraph.Maths[index].Functions.Count - 1;
		IOfficeMathRunElement officeMathRunElement = null;
		if (num != -1)
		{
			officeMathRunElement = MathParagraph.Maths[index].Functions[num] as IOfficeMathRunElement;
		}
		if (officeMathRunElement != null && officeMathRunElement.Item is ParagraphItem && (officeMathRunElement.Item as ParagraphItem).EntityType == EntityType.Break)
		{
			flag = true;
		}
		else
		{
			for (int i = mathIndex + 1; i < paraItems.Count; i++)
			{
				flag = IsRenderableItem(paraItems[i]);
				if (flag)
				{
					break;
				}
			}
		}
		return flag;
	}

	private bool HasRenderableItemBeforeMath(int mathIndex, ParagraphItemCollection paraItems)
	{
		bool flag = false;
		IOfficeMathRunElement officeMathRunElement = null;
		if (MathParagraph.Maths[0].Functions.Count != 0)
		{
			officeMathRunElement = MathParagraph.Maths[0].Functions[0] as IOfficeMathRunElement;
		}
		if (officeMathRunElement != null && officeMathRunElement.Item is ParagraphItem && (officeMathRunElement.Item as ParagraphItem).EntityType == EntityType.Break)
		{
			flag = true;
		}
		else
		{
			for (int num = mathIndex - 1; num >= 0; num--)
			{
				flag = IsRenderableItem(paraItems[num]);
				if (flag)
				{
					break;
				}
			}
		}
		return flag;
	}

	private void RemovePreviousBreak(int mathIndex, ParagraphItemCollection paraItems)
	{
		if (MathParagraph.Maths[0].Functions[0] is IOfficeMathRunElement officeMathRunElement && officeMathRunElement.Item is ParagraphItem && (officeMathRunElement.Item as ParagraphItem).EntityType == EntityType.Break)
		{
			MathParagraph.Maths[0].Functions.Remove(officeMathRunElement);
		}
		else
		{
			int num = -1;
			for (int num2 = mathIndex - 1; num2 >= 0; num2--)
			{
				if (paraItems[num2].EntityType == EntityType.Break)
				{
					num = num2;
					break;
				}
			}
			if (num != -1)
			{
				paraItems.RemoveAt(num);
			}
		}
		mathIndex = paraItems.IndexOf(this);
		if (mathIndex + 1 < paraItems.Count)
		{
			bool flag = false;
			for (int i = mathIndex + 1; i < paraItems.Count; i++)
			{
				if (IsRenderableItem(paraItems[i]))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				AddEmptyTextRange(paraItems, mathIndex + 1);
			}
		}
		else
		{
			AddEmptyTextRange(paraItems, mathIndex + 1);
		}
	}

	private void AddEmptyTextRange(ParagraphItemCollection paraItems, int mathIndex)
	{
		WTextRange wTextRange = new WTextRange(m_doc);
		wTextRange.Text = " ";
		paraItems.Insert(mathIndex, wTextRange);
	}

	private void RemoveNextBreak(ParagraphItemCollection paraItems)
	{
		int num = paraItems.IndexOf(this);
		int index = MathParagraph.Maths.Count - 1;
		int num2 = MathParagraph.Maths[index].Functions.Count - 1;
		IOfficeMathRunElement officeMathRunElement = null;
		if (num2 != -1)
		{
			officeMathRunElement = MathParagraph.Maths[index].Functions[num2] as IOfficeMathRunElement;
		}
		if (officeMathRunElement != null && officeMathRunElement.Item is ParagraphItem && (officeMathRunElement.Item as ParagraphItem).EntityType == EntityType.Break)
		{
			MathParagraph.Maths[index].Functions.Remove(officeMathRunElement);
		}
		else
		{
			for (int i = num + 1; i < paraItems.Count; i++)
			{
				if (IsRenderableItem(paraItems[i]) && paraItems[i].EntityType == EntityType.Break)
				{
					paraItems[i].RemoveSelf();
					break;
				}
			}
		}
		num = paraItems.IndexOf(this);
		if (num + 1 < paraItems.Count)
		{
			bool flag = false;
			for (int j = num + 1; j < paraItems.Count; j++)
			{
				if (IsRenderableItem(paraItems[j]))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				AddEmptyTextRange(paraItems, num + 1);
			}
		}
		else
		{
			AddEmptyTextRange(paraItems, num + 1);
		}
	}

	internal void ApplyBaseFormat()
	{
		for (int i = 0; i < MathParagraph.Maths.Count; i++)
		{
			IOfficeMath officeMath = MathParagraph.Maths[i];
			IterateOfficeMath(officeMath);
		}
	}

	private void IterateOfficeMath(IOfficeMath officeMath)
	{
		for (int i = 0; i < officeMath.Functions.Count; i++)
		{
			IterateIntoFunction(officeMath.Functions[i]);
		}
	}

	private void IterateIntoFunction(IOfficeMathFunctionBase officeMathFunction)
	{
		switch (officeMathFunction.Type)
		{
		case MathFunctionType.Accent:
			IterateOfficeMath((officeMathFunction as IOfficeMathAccent).Equation);
			break;
		case MathFunctionType.Bar:
			IterateOfficeMath((officeMathFunction as IOfficeMathBar).Equation);
			break;
		case MathFunctionType.BorderBox:
			IterateOfficeMath((officeMathFunction as IOfficeMathBorderBox).Equation);
			break;
		case MathFunctionType.Box:
			IterateOfficeMath((officeMathFunction as IOfficeMathBox).Equation);
			break;
		case MathFunctionType.Delimiter:
		{
			IOfficeMathDelimiter officeMathDelimiter = officeMathFunction as IOfficeMathDelimiter;
			for (int j = 0; j < officeMathDelimiter.Equation.Count; j++)
			{
				IterateOfficeMath(officeMathDelimiter.Equation[j]);
			}
			break;
		}
		case MathFunctionType.EquationArray:
		{
			IOfficeMathEquationArray officeMathEquationArray = officeMathFunction as IOfficeMathEquationArray;
			for (int i = 0; i < officeMathEquationArray.Equation.Count; i++)
			{
				IterateOfficeMath(officeMathEquationArray.Equation[i]);
			}
			break;
		}
		case MathFunctionType.Fraction:
		{
			IOfficeMathFraction officeMathFraction = officeMathFunction as IOfficeMathFraction;
			IterateOfficeMath(officeMathFraction.Numerator);
			IterateOfficeMath(officeMathFraction.Denominator);
			break;
		}
		case MathFunctionType.Function:
		{
			IOfficeMathFunction officeMathFunction2 = officeMathFunction as IOfficeMathFunction;
			IterateOfficeMath(officeMathFunction2.FunctionName);
			IterateOfficeMath(officeMathFunction2.Equation);
			break;
		}
		case MathFunctionType.GroupCharacter:
			IterateOfficeMath((officeMathFunction as IOfficeMathGroupCharacter).Equation);
			break;
		case MathFunctionType.Limit:
		{
			IOfficeMathLimit officeMathLimit = officeMathFunction as IOfficeMathLimit;
			IterateOfficeMath(officeMathLimit.Equation);
			IterateOfficeMath(officeMathLimit.Limit);
			break;
		}
		case MathFunctionType.Matrix:
		{
			IOfficeMathMatrix officeMathMatrix = officeMathFunction as IOfficeMathMatrix;
			for (int k = 0; k < officeMathMatrix.Rows.Count; k++)
			{
				for (int l = 0; l < (officeMathMatrix.Rows[k] as OfficeMathMatrixRow).Arguments.Count; l++)
				{
					IterateOfficeMath((officeMathMatrix.Rows[k] as OfficeMathMatrixRow).Arguments[l]);
				}
			}
			break;
		}
		case MathFunctionType.NArray:
		{
			IOfficeMathNArray officeMathNArray = officeMathFunction as IOfficeMathNArray;
			IterateOfficeMath(officeMathNArray.Subscript);
			IterateOfficeMath(officeMathNArray.Superscript);
			IterateOfficeMath(officeMathNArray.Equation);
			break;
		}
		case MathFunctionType.Phantom:
			IterateOfficeMath((officeMathFunction as IOfficeMathPhantom).Equation);
			break;
		case MathFunctionType.Radical:
		{
			IOfficeMathRadical officeMathRadical = officeMathFunction as IOfficeMathRadical;
			IterateOfficeMath(officeMathRadical.Degree);
			IterateOfficeMath(officeMathRadical.Equation);
			break;
		}
		case MathFunctionType.LeftSubSuperscript:
		{
			OfficeMathLeftScript officeMathLeftScript = officeMathFunction as OfficeMathLeftScript;
			IterateOfficeMath(officeMathLeftScript.Subscript);
			IterateOfficeMath(officeMathLeftScript.Superscript);
			IterateOfficeMath(officeMathLeftScript.Equation);
			break;
		}
		case MathFunctionType.SubSuperscript:
		{
			IOfficeMathScript officeMathScript = officeMathFunction as IOfficeMathScript;
			IterateOfficeMath(officeMathScript.Equation);
			IterateOfficeMath(officeMathScript.Script);
			break;
		}
		case MathFunctionType.RightSubSuperscript:
		{
			IOfficeMathRightScript officeMathRightScript = officeMathFunction as IOfficeMathRightScript;
			IterateOfficeMath(officeMathRightScript.Equation);
			IterateOfficeMath(officeMathRightScript.Subscript);
			IterateOfficeMath(officeMathRightScript.Superscript);
			break;
		}
		case MathFunctionType.RunElement:
		{
			IOfficeMathRunElement officeMathRunElement = officeMathFunction as IOfficeMathRunElement;
			if (officeMathRunElement.Item != null && (officeMathRunElement.Item as ParagraphItem).ParaItemCharFormat != null && base.OwnerParagraph != null && base.OwnerParagraph.BreakCharacterFormat != null && base.OwnerParagraph.BreakCharacterFormat.BaseFormat != null)
			{
				(officeMathRunElement.Item as ParagraphItem).ParaItemCharFormat.ApplyBase(base.OwnerParagraph.BreakCharacterFormat.BaseFormat);
			}
			break;
		}
		}
	}

	private bool CheckMathIsInline()
	{
		ParagraphItemCollection paragraphItemCollection = ((base.Owner is WParagraph) ? (base.Owner as WParagraph).Items : ((base.Owner is InlineContentControl) ? (base.Owner as InlineContentControl).ParagraphItems : null));
		if (paragraphItemCollection != null && paragraphItemCollection.Count > 0)
		{
			int mathIndex = paragraphItemCollection.IndexOf(this);
			bool flag = HasRenderableItemBeforeMath(mathIndex, paragraphItemCollection);
			bool flag2 = HasRenderableItemAfterMath(mathIndex, paragraphItemCollection);
			bool flag3 = IsMathAfterBreak(mathIndex, paragraphItemCollection);
			bool flag4 = IsMathBeforeBreak(paragraphItemCollection);
			if (!flag && flag2)
			{
				return !flag4;
			}
			if (flag && !flag2)
			{
				return !flag3;
			}
			if (flag)
			{
				return !(flag3 || flag4);
			}
		}
		if (base.Owner is InlineContentControl && (base.Owner as InlineContentControl).OwnerParagraph != null)
		{
			WParagraph ownerParagraph = (base.Owner as InlineContentControl).OwnerParagraph;
			int mathIndex2 = ownerParagraph.ChildEntities.IndexOf(base.Owner);
			bool num = HasRenderableItemBeforeMath(mathIndex2, ownerParagraph.Items);
			bool flag5 = HasRenderableItemAfterMath(mathIndex2, ownerParagraph.Items);
			return num || flag5;
		}
		return false;
	}

	private bool IsMathBeforeBreak(ParagraphItemCollection paraItems)
	{
		bool flag = false;
		int num = MathParagraph.Maths.Count - 1;
		int num2 = MathParagraph.Maths[num].Functions.Count - 1;
		IOfficeMathRunElement officeMathRunElement = null;
		if (num2 != -1)
		{
			officeMathRunElement = MathParagraph.Maths[num].Functions[num2] as IOfficeMathRunElement;
		}
		if (officeMathRunElement != null && officeMathRunElement.Item is ParagraphItem && (officeMathRunElement.Item as ParagraphItem).EntityType == EntityType.Break)
		{
			flag = true;
		}
		for (int i = num + 1; i < paraItems.Count; i++)
		{
			if (flag)
			{
				break;
			}
			if (!IsRenderableItem(paraItems[i]))
			{
				continue;
			}
			if (paraItems[i].EntityType == EntityType.Break)
			{
				if ((paraItems[i] as Break).BreakType != 0)
				{
					return (paraItems[i] as Break).BreakType != BreakType.ColumnBreak;
				}
				return false;
			}
			return false;
		}
		return flag;
	}

	private bool IsMathAfterBreak(int mathIndex, ParagraphItemCollection paraItems)
	{
		IOfficeMathRunElement officeMathRunElement = null;
		if (MathParagraph.Maths[0].Functions.Count != 0)
		{
			officeMathRunElement = MathParagraph.Maths[0].Functions[0] as IOfficeMathRunElement;
		}
		if (officeMathRunElement != null && officeMathRunElement.Item is ParagraphItem && (officeMathRunElement.Item as ParagraphItem).EntityType == EntityType.Break)
		{
			return true;
		}
		for (int num = mathIndex - 1; num >= 0; num--)
		{
			if (IsRenderableItem(paraItems[num]))
			{
				return paraItems[num].EntityType == EntityType.Break;
			}
		}
		return false;
	}

	protected override void CreateLayoutInfo()
	{
		m_layoutInfo = new LayoutInfo(ChildrenLayoutDirection.Horizontal);
		WParagraph wParagraph = base.OwnerParagraph;
		if (base.Owner is InlineContentControl || base.Owner is XmlParagraphItem || base.Owner is GroupShape || base.Owner is ChildGroupShape)
		{
			wParagraph = GetOwnerParagraphValue();
		}
		Entity ownerEntity = wParagraph.GetOwnerEntity();
		if ((wParagraph.IsInCell && ((IWidget)wParagraph).LayoutInfo.IsClipped) || ownerEntity is Shape || ownerEntity is WTextBox || ownerEntity is ChildShape)
		{
			m_layoutInfo.IsClipped = true;
		}
		m_layoutInfo.IsVerticalText = ((IWidget)wParagraph).LayoutInfo.IsVerticalText;
	}

	internal override void InitLayoutInfo(Entity entity, ref bool isLastTOCEntry)
	{
		InitializingLayoutInfo();
		if (this == entity)
		{
			isLastTOCEntry = true;
		}
	}

	void IWidget.InitLayoutInfo()
	{
		InitializingLayoutInfo();
	}

	private void InitializingLayoutInfo()
	{
		m_layoutInfo = null;
		for (int i = 0; i < MathParagraph.Maths.Count; i++)
		{
			IOfficeMath officeMath = MathParagraph.Maths[i];
			for (int j = 0; j < officeMath.Functions.Count; j++)
			{
				IOfficeMathFunctionBase officeMathFunctionBase = officeMath.Functions[j];
				if (officeMathFunctionBase.Type == MathFunctionType.RunElement)
				{
					IOfficeMathRunElement officeMathRunElement = officeMathFunctionBase as IOfficeMathRunElement;
					if (officeMathRunElement.Item is IWidget)
					{
						(officeMathRunElement.Item as IWidget).InitLayoutInfo();
					}
				}
			}
		}
	}

	SizeF ILeafWidget.Measure(DrawingContext dc)
	{
		return default(SizeF);
	}

	internal bool Compare(WMath revMath)
	{
		bool isComparing = revMath.Document.IsComparing;
		bool isComparing2 = base.Document.IsComparing;
		revMath.Document.IsComparing = true;
		base.Document.IsComparing = true;
		if (IsInline != revMath.IsInline)
		{
			return false;
		}
		if ((MathParagraph != null && revMath.MathParagraph == null) || (MathParagraph == null && revMath.MathParagraph != null))
		{
			return false;
		}
		if (MathParagraph != null && revMath.MathParagraph != null)
		{
			return Compare(MathParagraph as OfficeMathParagraph, revMath.MathParagraph as OfficeMathParagraph);
		}
		revMath.Document.IsComparing = isComparing;
		base.Document.IsComparing = isComparing2;
		return true;
	}

	internal bool Compare(OfficeMathParagraph orgMathPara, OfficeMathParagraph revMathPara)
	{
		if (orgMathPara.IsDefault != revMathPara.IsDefault || orgMathPara.Justification != revMathPara.Justification)
		{
			return false;
		}
		if ((orgMathPara.Maths != null && revMathPara.Maths == null) || (orgMathPara.Maths == null && revMathPara.Maths != null) || orgMathPara.Maths.Count != revMathPara.Maths.Count)
		{
			return false;
		}
		if (orgMathPara.Maths != null && revMathPara.Maths != null)
		{
			for (int i = 0; i < orgMathPara.Maths.Count; i++)
			{
				OfficeMath orgOfficeMath = orgMathPara.Maths[i] as OfficeMath;
				OfficeMath revOfficeMath = revMathPara.Maths[i] as OfficeMath;
				if (!Compare(orgOfficeMath, revOfficeMath))
				{
					return false;
				}
			}
		}
		return true;
	}

	internal bool Compare(OfficeMathFunctionBase orgOfficeMathFunctionBase, OfficeMathFunctionBase revOfficeMathFunctionBase)
	{
		if (orgOfficeMathFunctionBase is OfficeMathLimit && revOfficeMathFunctionBase is OfficeMathLimit)
		{
			if (!Compare(orgOfficeMathFunctionBase as OfficeMathLimit, revOfficeMathFunctionBase as OfficeMathLimit))
			{
				return false;
			}
		}
		else if (orgOfficeMathFunctionBase is OfficeMathDelimiter && revOfficeMathFunctionBase is OfficeMathDelimiter)
		{
			if (!Compare(orgOfficeMathFunctionBase as OfficeMathDelimiter, revOfficeMathFunctionBase as OfficeMathDelimiter))
			{
				return false;
			}
		}
		else if (orgOfficeMathFunctionBase is OfficeMathGroupCharacter && revOfficeMathFunctionBase is OfficeMathGroupCharacter)
		{
			if (!Compare(orgOfficeMathFunctionBase as OfficeMathGroupCharacter, revOfficeMathFunctionBase as OfficeMathGroupCharacter))
			{
				return false;
			}
		}
		else if (orgOfficeMathFunctionBase is OfficeMathLeftScript && revOfficeMathFunctionBase is OfficeMathLeftScript)
		{
			if (!Compare(orgOfficeMathFunctionBase as OfficeMathLeftScript, revOfficeMathFunctionBase as OfficeMathLeftScript))
			{
				return false;
			}
		}
		else if (orgOfficeMathFunctionBase is OfficeMathFunction && revOfficeMathFunctionBase is OfficeMathFunction)
		{
			if (!Compare(orgOfficeMathFunctionBase as OfficeMathFunction, revOfficeMathFunctionBase as OfficeMathFunction))
			{
				return false;
			}
		}
		else if (orgOfficeMathFunctionBase is OfficeMathFraction && revOfficeMathFunctionBase is OfficeMathFraction)
		{
			if (!Compare(orgOfficeMathFunctionBase as OfficeMathFraction, revOfficeMathFunctionBase as OfficeMathFraction))
			{
				return false;
			}
		}
		else if (orgOfficeMathFunctionBase is OfficeMathEquationArray && revOfficeMathFunctionBase is OfficeMathEquationArray)
		{
			if (!Compare(orgOfficeMathFunctionBase as OfficeMathEquationArray, revOfficeMathFunctionBase as OfficeMathEquationArray))
			{
				return false;
			}
		}
		else if (orgOfficeMathFunctionBase is OfficeMathBorderBox && revOfficeMathFunctionBase is OfficeMathBorderBox)
		{
			if (!Compare(orgOfficeMathFunctionBase as OfficeMathBorderBox, revOfficeMathFunctionBase as OfficeMathBorderBox))
			{
				return false;
			}
		}
		else if (orgOfficeMathFunctionBase is OfficeMathMatrix && revOfficeMathFunctionBase is OfficeMathMatrix)
		{
			if (!Compare(orgOfficeMathFunctionBase as OfficeMathMatrix, revOfficeMathFunctionBase as OfficeMathMatrix))
			{
				return false;
			}
		}
		else if (orgOfficeMathFunctionBase is OfficeMathNArray && revOfficeMathFunctionBase is OfficeMathNArray)
		{
			if (!Compare(orgOfficeMathFunctionBase as OfficeMathNArray, revOfficeMathFunctionBase as OfficeMathNArray))
			{
				return false;
			}
		}
		else if (orgOfficeMathFunctionBase is OfficeMathRadical && revOfficeMathFunctionBase is OfficeMathRadical)
		{
			if (!Compare(orgOfficeMathFunctionBase as OfficeMathRadical, revOfficeMathFunctionBase as OfficeMathRadical))
			{
				return false;
			}
		}
		else if (orgOfficeMathFunctionBase is OfficeMathScript && revOfficeMathFunctionBase is OfficeMathScript)
		{
			if (!Compare(orgOfficeMathFunctionBase as OfficeMathScript, revOfficeMathFunctionBase as OfficeMathScript))
			{
				return false;
			}
		}
		else if (orgOfficeMathFunctionBase is OfficeMathBox && revOfficeMathFunctionBase is OfficeMathBox)
		{
			if (!Compare(orgOfficeMathFunctionBase as OfficeMathBox, revOfficeMathFunctionBase as OfficeMathBox))
			{
				return false;
			}
		}
		else if (orgOfficeMathFunctionBase is OfficeMathAccent && revOfficeMathFunctionBase is OfficeMathAccent)
		{
			if (!Compare(orgOfficeMathFunctionBase as OfficeMathAccent, revOfficeMathFunctionBase as OfficeMathAccent))
			{
				return false;
			}
		}
		else if (orgOfficeMathFunctionBase is OfficeMathPhantom && revOfficeMathFunctionBase is OfficeMathPhantom)
		{
			if (!Compare(orgOfficeMathFunctionBase as OfficeMathPhantom, revOfficeMathFunctionBase as OfficeMathPhantom))
			{
				return false;
			}
		}
		else if (orgOfficeMathFunctionBase is OfficeMathBar && revOfficeMathFunctionBase is OfficeMathBar)
		{
			if (!Compare(orgOfficeMathFunctionBase as OfficeMathBar, revOfficeMathFunctionBase as OfficeMathBar))
			{
				return false;
			}
		}
		else if (orgOfficeMathFunctionBase is OfficeMathPhantom && revOfficeMathFunctionBase is OfficeMathPhantom)
		{
			if (!Compare(orgOfficeMathFunctionBase as OfficeMathPhantom, revOfficeMathFunctionBase as OfficeMathPhantom))
			{
				return false;
			}
		}
		else
		{
			if (!(orgOfficeMathFunctionBase is OfficeMathRunElement) || !(revOfficeMathFunctionBase is OfficeMathRunElement))
			{
				return false;
			}
			if (!Compare(orgOfficeMathFunctionBase as OfficeMathRunElement, revOfficeMathFunctionBase as OfficeMathRunElement))
			{
				return false;
			}
		}
		return true;
	}

	internal bool Compare(OfficeMathBox orgOfficeMathBox, OfficeMathBox revOfficeMathBox)
	{
		if (orgOfficeMathBox.Alignment != revOfficeMathBox.Alignment || orgOfficeMathBox.EnableDifferential != revOfficeMathBox.EnableDifferential || orgOfficeMathBox.NoBreak != revOfficeMathBox.NoBreak || orgOfficeMathBox.OperatorEmulator != revOfficeMathBox.OperatorEmulator)
		{
			return false;
		}
		if ((orgOfficeMathBox.Equation != null && revOfficeMathBox.Equation == null) || (orgOfficeMathBox.Equation == null && revOfficeMathBox.Equation != null) || (orgOfficeMathBox.Break != null && revOfficeMathBox.Break == null) || (orgOfficeMathBox.Break == null && revOfficeMathBox.Break != null))
		{
			return false;
		}
		if (orgOfficeMathBox.Equation != null && revOfficeMathBox.Equation != null && !Compare(orgOfficeMathBox.Equation as OfficeMath, revOfficeMathBox.Equation as OfficeMath))
		{
			return false;
		}
		if (orgOfficeMathBox.Break != null && revOfficeMathBox.Break != null && !Compare(orgOfficeMathBox.Break as OfficeMathBreak, revOfficeMathBox.Break as OfficeMathBreak))
		{
			return false;
		}
		return true;
	}

	internal bool Compare(OfficeMathBar orgOfficeMathBar, OfficeMathBar revOfficeMathBar)
	{
		if (orgOfficeMathBar.BarTop != revOfficeMathBar.BarTop)
		{
			return false;
		}
		if ((orgOfficeMathBar.Equation != null && revOfficeMathBar.Equation == null) || (orgOfficeMathBar.Equation == null && revOfficeMathBar.Equation != null))
		{
			return false;
		}
		if (orgOfficeMathBar.Equation != null && revOfficeMathBar.Equation != null && !Compare(orgOfficeMathBar.Equation as OfficeMath, revOfficeMathBar.Equation as OfficeMath))
		{
			return false;
		}
		return true;
	}

	internal bool Compare(OfficeMathRightScript orgOfficeMathBox, OfficeMathRightScript revOfficeMathBox)
	{
		if (orgOfficeMathBox.IsSkipAlign != revOfficeMathBox.IsSkipAlign)
		{
			return false;
		}
		if ((orgOfficeMathBox.Equation != null && revOfficeMathBox.Equation == null) || (orgOfficeMathBox.Equation == null && revOfficeMathBox.Equation != null) || (orgOfficeMathBox.Subscript != null && revOfficeMathBox.Subscript == null) || (orgOfficeMathBox.Subscript == null && revOfficeMathBox.Subscript != null) || (orgOfficeMathBox.Superscript != null && revOfficeMathBox.Superscript == null) || (orgOfficeMathBox.Superscript == null && revOfficeMathBox.Superscript != null))
		{
			return false;
		}
		if (orgOfficeMathBox.Equation != null && revOfficeMathBox.Equation != null && !Compare(orgOfficeMathBox.Equation as OfficeMath, revOfficeMathBox.Equation as OfficeMath))
		{
			return false;
		}
		if (orgOfficeMathBox.Subscript != null && revOfficeMathBox.Subscript != null && !Compare(orgOfficeMathBox.Subscript as OfficeMath, revOfficeMathBox.Subscript as OfficeMath))
		{
			return false;
		}
		if (orgOfficeMathBox.Superscript != null && revOfficeMathBox.Superscript != null && !Compare(orgOfficeMathBox.Superscript as OfficeMath, revOfficeMathBox.Superscript as OfficeMath))
		{
			return false;
		}
		return true;
	}

	internal bool Compare(OfficeMathPhantom orgOfficeMathPhantom, OfficeMathPhantom revOfficeMathPhantom)
	{
		if (orgOfficeMathPhantom.Show != revOfficeMathPhantom.Show || orgOfficeMathPhantom.Smash != revOfficeMathPhantom.Smash || orgOfficeMathPhantom.Transparent != revOfficeMathPhantom.Transparent || orgOfficeMathPhantom.ZeroAscent != revOfficeMathPhantom.ZeroAscent || orgOfficeMathPhantom.ZeroDescent != revOfficeMathPhantom.ZeroDescent || orgOfficeMathPhantom.IsDefault != revOfficeMathPhantom.IsDefault)
		{
			return false;
		}
		if ((orgOfficeMathPhantom.Equation != null && revOfficeMathPhantom.Equation == null) || (orgOfficeMathPhantom.Equation == null && revOfficeMathPhantom.Equation != null))
		{
			return false;
		}
		if (orgOfficeMathPhantom.Equation != null && revOfficeMathPhantom.Equation != null && !Compare(orgOfficeMathPhantom.Equation as OfficeMath, revOfficeMathPhantom.Equation as OfficeMath))
		{
			return false;
		}
		return true;
	}

	internal bool Compare(OfficeMathAccent orgOfficeMathAccent, OfficeMathAccent revOfficeMathAccent)
	{
		if (orgOfficeMathAccent.AccentCharacter != revOfficeMathAccent.AccentCharacter)
		{
			return false;
		}
		if ((orgOfficeMathAccent.Equation != null && revOfficeMathAccent.Equation == null) || (orgOfficeMathAccent.Equation == null && revOfficeMathAccent.Equation != null))
		{
			return false;
		}
		if (orgOfficeMathAccent.Equation != null && revOfficeMathAccent.Equation != null && !Compare(orgOfficeMathAccent.Equation as OfficeMath, revOfficeMathAccent.Equation as OfficeMath))
		{
			return false;
		}
		return true;
	}

	internal bool Compare(OfficeMathScript orgOfficeMathScript, OfficeMathScript revOfficeMathScript)
	{
		if (orgOfficeMathScript.ScriptType != revOfficeMathScript.ScriptType)
		{
			return false;
		}
		if ((orgOfficeMathScript.Equation != null && revOfficeMathScript.Equation == null) || (orgOfficeMathScript.Equation == null && revOfficeMathScript.Equation != null) || (orgOfficeMathScript.Script != null && revOfficeMathScript.Script == null) || (orgOfficeMathScript.Script == null && revOfficeMathScript.Script != null))
		{
			return false;
		}
		if (orgOfficeMathScript.Equation != null && revOfficeMathScript.Equation != null && !Compare(orgOfficeMathScript.Equation as OfficeMath, revOfficeMathScript.Equation as OfficeMath))
		{
			return false;
		}
		if (orgOfficeMathScript.Script != null && revOfficeMathScript.Script != null && !Compare(orgOfficeMathScript.Script as OfficeMath, revOfficeMathScript.Script as OfficeMath))
		{
			return false;
		}
		return true;
	}

	internal bool Compare(OfficeMathRadical orgOfficeMathRadical, OfficeMathRadical revOfficeMathRadical)
	{
		if (orgOfficeMathRadical.HideDegree != revOfficeMathRadical.HideDegree)
		{
			return false;
		}
		if ((orgOfficeMathRadical.Equation != null && revOfficeMathRadical.Equation == null) || (orgOfficeMathRadical.Equation == null && revOfficeMathRadical.Equation != null) || (orgOfficeMathRadical.Degree != null && revOfficeMathRadical.Degree == null) || (orgOfficeMathRadical.Degree == null && revOfficeMathRadical.Degree != null))
		{
			return false;
		}
		if (orgOfficeMathRadical.Equation != null && revOfficeMathRadical.Equation != null && !Compare(orgOfficeMathRadical.Equation as OfficeMath, revOfficeMathRadical.Equation as OfficeMath))
		{
			return false;
		}
		if (orgOfficeMathRadical.Degree != null && revOfficeMathRadical.Degree != null && !Compare(orgOfficeMathRadical.Degree as OfficeMath, revOfficeMathRadical.Degree as OfficeMath))
		{
			return false;
		}
		return true;
	}

	internal bool Compare(OfficeMathNArray orgOfficeMathNArray, OfficeMathNArray revOfficeMathNArray)
	{
		if (orgOfficeMathNArray.HasGrow != revOfficeMathNArray.HasGrow || orgOfficeMathNArray.HideLowerLimit != revOfficeMathNArray.HideLowerLimit || orgOfficeMathNArray.HideUpperLimit != revOfficeMathNArray.HideUpperLimit || orgOfficeMathNArray.SubSuperscriptLimit != revOfficeMathNArray.SubSuperscriptLimit || orgOfficeMathNArray.NArrayCharacter != revOfficeMathNArray.NArrayCharacter)
		{
			return false;
		}
		if ((orgOfficeMathNArray.Equation != null && revOfficeMathNArray.Equation == null) || (orgOfficeMathNArray.Equation == null && revOfficeMathNArray.Equation != null) || (orgOfficeMathNArray.Subscript != null && revOfficeMathNArray.Subscript == null) || (orgOfficeMathNArray.Subscript == null && revOfficeMathNArray.Subscript != null) || (orgOfficeMathNArray.Superscript != null && revOfficeMathNArray.Superscript == null) || (orgOfficeMathNArray.Superscript == null && revOfficeMathNArray.Superscript != null))
		{
			return false;
		}
		if (orgOfficeMathNArray.Equation != null && revOfficeMathNArray.Equation != null && !Compare(orgOfficeMathNArray.Equation as OfficeMath, revOfficeMathNArray.Equation as OfficeMath))
		{
			return false;
		}
		if (orgOfficeMathNArray.Subscript != null && revOfficeMathNArray.Subscript != null && !Compare(orgOfficeMathNArray.Subscript as OfficeMath, revOfficeMathNArray.Subscript as OfficeMath))
		{
			return false;
		}
		if (orgOfficeMathNArray.Superscript != null && revOfficeMathNArray.Superscript != null && !Compare(orgOfficeMathNArray.Superscript as OfficeMath, revOfficeMathNArray.Superscript as OfficeMath))
		{
			return false;
		}
		return true;
	}

	internal bool Compare(OfficeMathMatrix orgOfficeMathMatrix, OfficeMathMatrix revOfficeMathMatrix)
	{
		if (orgOfficeMathMatrix.HidePlaceHolders != revOfficeMathMatrix.HidePlaceHolders || orgOfficeMathMatrix.VerticalAlignment != revOfficeMathMatrix.VerticalAlignment || orgOfficeMathMatrix.ColumnWidth != revOfficeMathMatrix.ColumnWidth || orgOfficeMathMatrix.ColumnSpacingRule != revOfficeMathMatrix.ColumnSpacingRule || orgOfficeMathMatrix.ColumnSpacing != revOfficeMathMatrix.ColumnSpacing || orgOfficeMathMatrix.RowSpacing != revOfficeMathMatrix.RowSpacing || orgOfficeMathMatrix.RowSpacingRule != revOfficeMathMatrix.RowSpacingRule)
		{
			return false;
		}
		if ((orgOfficeMathMatrix.Columns != null && revOfficeMathMatrix.Columns == null) || (orgOfficeMathMatrix.Rows == null && revOfficeMathMatrix.Rows != null) || (orgOfficeMathMatrix.ColumnProperties != null && revOfficeMathMatrix.ColumnProperties == null))
		{
			return false;
		}
		if (orgOfficeMathMatrix.Columns != null && revOfficeMathMatrix.Columns != null)
		{
			for (int i = 0; i < orgOfficeMathMatrix.Columns.Count; i++)
			{
				if (!Compare(orgOfficeMathMatrix.Columns[i] as OfficeMathMatrixColumn, revOfficeMathMatrix.Columns[i] as OfficeMathMatrixColumn))
				{
					return false;
				}
			}
		}
		if (orgOfficeMathMatrix.Rows != null && revOfficeMathMatrix.Rows != null)
		{
			for (int j = 0; j < orgOfficeMathMatrix.Rows.Count; j++)
			{
				if (!Compare(orgOfficeMathMatrix.Rows[j] as OfficeMathMatrixRow, revOfficeMathMatrix.Rows[j] as OfficeMathMatrixRow))
				{
					return false;
				}
			}
		}
		if (orgOfficeMathMatrix.ColumnProperties != null && revOfficeMathMatrix.ColumnProperties != null)
		{
			for (int k = 0; k < orgOfficeMathMatrix.ColumnProperties.Count; k++)
			{
				if (!Compare(orgOfficeMathMatrix.ColumnProperties[k], revOfficeMathMatrix.ColumnProperties[k]))
				{
					return false;
				}
			}
		}
		return true;
	}

	internal bool Compare(OfficeMathBorderBox orgOfficeMathBorderBox, OfficeMathBorderBox revOfficeMathBorderBox)
	{
		if (orgOfficeMathBorderBox.HideTop != revOfficeMathBorderBox.HideTop || orgOfficeMathBorderBox.HideBottom != revOfficeMathBorderBox.HideBottom || orgOfficeMathBorderBox.HideRight != revOfficeMathBorderBox.HideRight || orgOfficeMathBorderBox.HideLeft != revOfficeMathBorderBox.HideLeft || orgOfficeMathBorderBox.StrikeDiagonalUp != revOfficeMathBorderBox.StrikeDiagonalUp || orgOfficeMathBorderBox.StrikeDiagonalDown != revOfficeMathBorderBox.StrikeDiagonalDown || orgOfficeMathBorderBox.StrikeVertical != revOfficeMathBorderBox.StrikeVertical || orgOfficeMathBorderBox.StrikeHorizontal != revOfficeMathBorderBox.StrikeHorizontal)
		{
			return false;
		}
		if ((orgOfficeMathBorderBox.Equation != null && revOfficeMathBorderBox.Equation == null) || (orgOfficeMathBorderBox.Equation == null && revOfficeMathBorderBox.Equation != null))
		{
			return false;
		}
		if (orgOfficeMathBorderBox.Equation != null && revOfficeMathBorderBox.Equation != null && !Compare(orgOfficeMathBorderBox.Equation as OfficeMath, revOfficeMathBorderBox.Equation as OfficeMath))
		{
			return false;
		}
		return true;
	}

	internal bool Compare(OfficeMathEquationArray orgOfficeMathEquationArray, OfficeMathEquationArray revOfficeMathEquationArray)
	{
		if (orgOfficeMathEquationArray.ExpandEquationContainer != revOfficeMathEquationArray.ExpandEquationContainer || orgOfficeMathEquationArray.ExpandEquationContent != revOfficeMathEquationArray.ExpandEquationContent || orgOfficeMathEquationArray.VerticalAlignment != revOfficeMathEquationArray.VerticalAlignment || orgOfficeMathEquationArray.RowSpacing != revOfficeMathEquationArray.RowSpacing || orgOfficeMathEquationArray.RowSpacingRule != revOfficeMathEquationArray.RowSpacingRule)
		{
			return false;
		}
		if ((orgOfficeMathEquationArray.Equation != null && revOfficeMathEquationArray.Equation == null) || (orgOfficeMathEquationArray.Equation == null && revOfficeMathEquationArray.Equation != null))
		{
			return false;
		}
		if (orgOfficeMathEquationArray.Equation != null && revOfficeMathEquationArray.Equation != null && !Compare(orgOfficeMathEquationArray.Equation as OfficeMath, revOfficeMathEquationArray.Equation as OfficeMath))
		{
			return false;
		}
		return true;
	}

	internal bool Compare(OfficeMathFraction orgOfficeMathFraction, OfficeMathFraction revOfficeMathFraction)
	{
		if (orgOfficeMathFraction.FractionType != revOfficeMathFraction.FractionType)
		{
			return false;
		}
		if ((orgOfficeMathFraction.Denominator != null && revOfficeMathFraction.Denominator == null) || (orgOfficeMathFraction.Denominator == null && revOfficeMathFraction.Denominator != null) || (orgOfficeMathFraction.Numerator != null && revOfficeMathFraction.Numerator == null) || (orgOfficeMathFraction.Numerator == null && revOfficeMathFraction.Numerator != null))
		{
			return false;
		}
		if (orgOfficeMathFraction.Denominator != null && revOfficeMathFraction.Denominator != null && !Compare(orgOfficeMathFraction.Denominator as OfficeMath, revOfficeMathFraction.Denominator as OfficeMath))
		{
			return false;
		}
		if (orgOfficeMathFraction.Numerator != null && revOfficeMathFraction.Numerator != null && !Compare(orgOfficeMathFraction.Numerator as OfficeMath, revOfficeMathFraction.Numerator as OfficeMath))
		{
			return false;
		}
		return true;
	}

	internal bool Compare(OfficeMathFunction orgOfficeMathFunction, OfficeMathFunction revOfficeMathFunction)
	{
		if ((orgOfficeMathFunction.Equation != null && revOfficeMathFunction.Equation == null) || (orgOfficeMathFunction.Equation == null && revOfficeMathFunction.Equation != null) || (orgOfficeMathFunction.FunctionName != null && revOfficeMathFunction.FunctionName == null) || (orgOfficeMathFunction.FunctionName == null && revOfficeMathFunction.FunctionName != null))
		{
			return false;
		}
		if (orgOfficeMathFunction.Equation != null && revOfficeMathFunction.Equation != null && !Compare(orgOfficeMathFunction.Equation as OfficeMath, revOfficeMathFunction.Equation as OfficeMath))
		{
			return false;
		}
		if (orgOfficeMathFunction.FunctionName != null && revOfficeMathFunction.FunctionName != null && !Compare(orgOfficeMathFunction.FunctionName as OfficeMath, revOfficeMathFunction.FunctionName as OfficeMath))
		{
			return false;
		}
		return true;
	}

	internal bool Compare(OfficeMathLeftScript orgOfficeMathLeftScript, OfficeMathLeftScript revOfficeMathLeftScript)
	{
		if ((orgOfficeMathLeftScript.Equation != null && revOfficeMathLeftScript.Equation == null) || (orgOfficeMathLeftScript.Equation == null && revOfficeMathLeftScript.Equation != null) || (orgOfficeMathLeftScript.Subscript != null && revOfficeMathLeftScript.Subscript == null) || (orgOfficeMathLeftScript.Subscript == null && revOfficeMathLeftScript.Subscript != null) || (orgOfficeMathLeftScript.Superscript != null && revOfficeMathLeftScript.Superscript == null) || (orgOfficeMathLeftScript.Superscript == null && revOfficeMathLeftScript.Superscript != null))
		{
			return false;
		}
		if (orgOfficeMathLeftScript.Equation != null && revOfficeMathLeftScript.Equation != null && !Compare(orgOfficeMathLeftScript.Equation as OfficeMath, revOfficeMathLeftScript.Equation as OfficeMath))
		{
			return false;
		}
		if (orgOfficeMathLeftScript.Subscript != null && revOfficeMathLeftScript.Subscript != null && !Compare(orgOfficeMathLeftScript.Subscript as OfficeMath, revOfficeMathLeftScript.Subscript as OfficeMath))
		{
			return false;
		}
		if (orgOfficeMathLeftScript.Superscript != null && revOfficeMathLeftScript.Superscript != null && !Compare(orgOfficeMathLeftScript.Superscript as OfficeMath, revOfficeMathLeftScript.Superscript as OfficeMath))
		{
			return false;
		}
		return true;
	}

	internal bool Compare(OfficeMathGroupCharacter orgOfficeMathGroupCharacter, OfficeMathGroupCharacter revOfficeMathGroupCharacter)
	{
		if (orgOfficeMathGroupCharacter.HasAlignTop != revOfficeMathGroupCharacter.HasAlignTop || orgOfficeMathGroupCharacter.HasCharacterTop != revOfficeMathGroupCharacter.HasCharacterTop || orgOfficeMathGroupCharacter.GroupCharacter != revOfficeMathGroupCharacter.GroupCharacter)
		{
			return false;
		}
		if ((orgOfficeMathGroupCharacter.Equation != null && revOfficeMathGroupCharacter.Equation == null) || (orgOfficeMathGroupCharacter.Equation == null && revOfficeMathGroupCharacter.Equation != null))
		{
			return false;
		}
		if (orgOfficeMathGroupCharacter.Equation != null && revOfficeMathGroupCharacter.Equation != null && !Compare(orgOfficeMathGroupCharacter.Equation as OfficeMath, revOfficeMathGroupCharacter.Equation as OfficeMath))
		{
			return false;
		}
		return true;
	}

	internal bool Compare(OfficeMathDelimiter orgOfficeMathDelimiter, OfficeMathDelimiter revOfficeMathDelimiter)
	{
		if (orgOfficeMathDelimiter.IsGrow != revOfficeMathDelimiter.IsGrow || orgOfficeMathDelimiter.BeginCharacter != revOfficeMathDelimiter.BeginCharacter || orgOfficeMathDelimiter.EndCharacter != revOfficeMathDelimiter.EndCharacter || orgOfficeMathDelimiter.DelimiterShape != revOfficeMathDelimiter.DelimiterShape)
		{
			return false;
		}
		if ((orgOfficeMathDelimiter.Equation != null && revOfficeMathDelimiter.Equation == null) || (orgOfficeMathDelimiter.Equation == null && revOfficeMathDelimiter.Equation != null))
		{
			return false;
		}
		if (orgOfficeMathDelimiter.Equation != null && revOfficeMathDelimiter.Equation != null && !Compare(orgOfficeMathDelimiter.Equation as OfficeMath, revOfficeMathDelimiter.Equation as OfficeMath))
		{
			return false;
		}
		return true;
	}

	internal bool Compare(OfficeMathLimit orgOfficeMathBox, OfficeMathLimit revOfficeMathBox)
	{
		if (orgOfficeMathBox.LimitType != revOfficeMathBox.LimitType)
		{
			return false;
		}
		if ((orgOfficeMathBox.Equation != null && revOfficeMathBox.Equation == null) || (orgOfficeMathBox.Equation == null && revOfficeMathBox.Equation != null) || (orgOfficeMathBox.Limit != null && revOfficeMathBox.Limit == null) || (orgOfficeMathBox.Limit == null && revOfficeMathBox.Limit != null))
		{
			return false;
		}
		if (orgOfficeMathBox.Equation != null && revOfficeMathBox.Equation != null && !Compare(orgOfficeMathBox.Equation as OfficeMath, revOfficeMathBox.Equation as OfficeMath))
		{
			return false;
		}
		if (orgOfficeMathBox.Limit != null && revOfficeMathBox.Limit != null && !Compare(orgOfficeMathBox.Limit as OfficeMath, revOfficeMathBox.Limit as OfficeMath))
		{
			return false;
		}
		return true;
	}

	internal bool Compare(OfficeMathRunElement orgOfficeMathRunElement, OfficeMathRunElement revOfficeMathRunElement)
	{
		if ((orgOfficeMathRunElement.Item != null && revOfficeMathRunElement.Item == null) || (orgOfficeMathRunElement.Item == null && revOfficeMathRunElement.Item != null) || (orgOfficeMathRunElement.MathFormat != null && revOfficeMathRunElement.MathFormat == null) || (orgOfficeMathRunElement.MathFormat == null && revOfficeMathRunElement.MathFormat != null))
		{
			return false;
		}
		if (orgOfficeMathRunElement.Item != null && revOfficeMathRunElement.Item != null && orgOfficeMathRunElement.Item is WTextRange && revOfficeMathRunElement.Item is WTextRange && ((orgOfficeMathRunElement.Item as WTextRange).Text != (revOfficeMathRunElement.Item as WTextRange).Text || !(orgOfficeMathRunElement.Item as WTextRange).CharacterFormat.Compare((revOfficeMathRunElement.Item as WTextRange).CharacterFormat)))
		{
			return false;
		}
		if (orgOfficeMathRunElement.MathFormat != null && revOfficeMathRunElement.MathFormat != null && !Compare(orgOfficeMathRunElement.MathFormat as OfficeMathFormat, revOfficeMathRunElement.MathFormat as OfficeMathFormat))
		{
			return false;
		}
		return true;
	}

	internal bool Compare(OfficeMathFormat orgOfficeMathFormat, OfficeMathFormat revOfficeMathFormat)
	{
		if (orgOfficeMathFormat.HasAlignment != revOfficeMathFormat.HasAlignment || orgOfficeMathFormat.HasLiteral != revOfficeMathFormat.HasLiteral || orgOfficeMathFormat.HasNormalText != revOfficeMathFormat.HasNormalText || orgOfficeMathFormat.Font != revOfficeMathFormat.Font || orgOfficeMathFormat.Style != revOfficeMathFormat.Style)
		{
			return false;
		}
		if ((orgOfficeMathFormat.Break != null && revOfficeMathFormat.Break == null) || (orgOfficeMathFormat.Break == null && revOfficeMathFormat.Break != null))
		{
			return false;
		}
		if (orgOfficeMathFormat.Break != null && revOfficeMathFormat.Break != null && !Compare(orgOfficeMathFormat.Break as OfficeMathBreak, revOfficeMathFormat.Break as OfficeMathBreak))
		{
			return false;
		}
		return true;
	}

	internal bool Compare(OfficeMath orgOfficeMath, OfficeMath revOfficeMath)
	{
		if (orgOfficeMath.AlignPoint != revOfficeMath.AlignPoint || orgOfficeMath.ArgumentSize != revOfficeMath.ArgumentSize || orgOfficeMath.NestingLevel != revOfficeMath.NestingLevel)
		{
			return false;
		}
		if ((orgOfficeMath.Functions != null && revOfficeMath.Functions == null) || (orgOfficeMath.Functions == null && revOfficeMath.Functions != null) || orgOfficeMath.Functions.Count != revOfficeMath.Functions.Count || (orgOfficeMath.Breaks != null && revOfficeMath.Breaks == null) || (orgOfficeMath.Breaks == null && revOfficeMath.Breaks != null) || orgOfficeMath.Breaks.Count != revOfficeMath.Breaks.Count)
		{
			return false;
		}
		if (orgOfficeMath.Functions != null && revOfficeMath.Functions != null)
		{
			for (int i = 0; i < orgOfficeMath.Functions.Count; i++)
			{
				OfficeMathFunctionBase orgOfficeMathFunctionBase = orgOfficeMath.Functions[i] as OfficeMathFunctionBase;
				OfficeMathFunctionBase revOfficeMathFunctionBase = revOfficeMath.Functions[i] as OfficeMathFunctionBase;
				if (!Compare(orgOfficeMathFunctionBase, revOfficeMathFunctionBase))
				{
					return false;
				}
			}
		}
		if (orgOfficeMath.Breaks != null && revOfficeMath.Breaks != null)
		{
			for (int j = 0; j < orgOfficeMath.Breaks.Count; j++)
			{
				OfficeMathBreak orgOfficeMathBreak = orgOfficeMath.Breaks[j] as OfficeMathBreak;
				OfficeMathBreak revOfficeMathBreak = revOfficeMath.Breaks[j] as OfficeMathBreak;
				if (!Compare(orgOfficeMathBreak, revOfficeMathBreak))
				{
					return false;
				}
			}
		}
		return true;
	}

	internal bool Compare(MatrixColumnProperties orgMatrixColumnProperties, MatrixColumnProperties revMatrixColumnProperties)
	{
		if (orgMatrixColumnProperties.Alignment != revMatrixColumnProperties.Alignment || orgMatrixColumnProperties.Count != revMatrixColumnProperties.Count)
		{
			return false;
		}
		return true;
	}

	internal bool Compare(OfficeMathMatrixColumn orgOfficeMathMatrixColumn, OfficeMathMatrixColumn revOfficeMathMatrixColumn)
	{
		if (orgOfficeMathMatrixColumn.ColumnIndex != revOfficeMathMatrixColumn.ColumnIndex || orgOfficeMathMatrixColumn.HorizontalAlignment != revOfficeMathMatrixColumn.HorizontalAlignment)
		{
			return false;
		}
		if ((orgOfficeMathMatrixColumn.Arguments != null && revOfficeMathMatrixColumn.Arguments == null) || (orgOfficeMathMatrixColumn.Arguments == null && revOfficeMathMatrixColumn.Arguments != null) || orgOfficeMathMatrixColumn.Arguments.Count != revOfficeMathMatrixColumn.Arguments.Count)
		{
			return false;
		}
		for (int i = 0; i < orgOfficeMathMatrixColumn.Arguments.Count; i++)
		{
			OfficeMathMatrixRow officeMathMatrixRow = orgOfficeMathMatrixColumn.Arguments[i] as OfficeMathMatrixRow;
			OfficeMathMatrixRow officeMathMatrixRow2 = revOfficeMathMatrixColumn.Arguments[i] as OfficeMathMatrixRow;
			if (officeMathMatrixRow != null && officeMathMatrixRow2 != null && !Compare(officeMathMatrixRow, officeMathMatrixRow2))
			{
				return false;
			}
		}
		return true;
	}

	internal bool Compare(OfficeMathMatrixRow orgMathMatrixRow, OfficeMathMatrixRow revMathMatrixRow)
	{
		if (orgMathMatrixRow.RowIndex != revMathMatrixRow.RowIndex)
		{
			return false;
		}
		if ((orgMathMatrixRow.Arguments != null && revMathMatrixRow.Arguments == null) || (orgMathMatrixRow.Arguments == null && revMathMatrixRow.Arguments != null) || orgMathMatrixRow.Arguments.Count != revMathMatrixRow.Arguments.Count)
		{
			return false;
		}
		for (int i = 0; i < orgMathMatrixRow.Arguments.Count; i++)
		{
			OfficeMath officeMath = orgMathMatrixRow.Arguments[i] as OfficeMath;
			OfficeMath officeMath2 = revMathMatrixRow.Arguments[i] as OfficeMath;
			if (officeMath != null && officeMath2 != null && !Compare(officeMath, officeMath2))
			{
				return false;
			}
		}
		return true;
	}

	internal bool Compare(OfficeMathBreak orgOfficeMathBreak, OfficeMathBreak revOfficeMathBreak)
	{
		if (orgOfficeMathBreak.AlignAt != revOfficeMathBreak.AlignAt)
		{
			return false;
		}
		return true;
	}

	internal StringBuilder GetAsString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append('\u0019');
		stringBuilder.Append(GetProperties());
		stringBuilder.Append('\u0019');
		return stringBuilder;
	}

	internal StringBuilder GetProperties()
	{
		StringBuilder stringBuilder = new StringBuilder();
		string text = (IsInline ? "1" : "0");
		stringBuilder.Append(text + ";");
		if (MathParagraph != null)
		{
			stringBuilder.Append(GetAsString(MathParagraph as OfficeMathParagraph));
		}
		return stringBuilder;
	}

	internal StringBuilder GetAsString(OfficeMathLeftScript officeMathLeftScript)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (officeMathLeftScript.Equation != null)
		{
			stringBuilder.Append(GetAsString(officeMathLeftScript.Equation as OfficeMath));
		}
		if (officeMathLeftScript.Subscript != null)
		{
			stringBuilder.Append(GetAsString(officeMathLeftScript.Subscript as OfficeMath));
		}
		if (officeMathLeftScript.Superscript != null)
		{
			stringBuilder.Append(GetAsString(officeMathLeftScript.Superscript as OfficeMath));
		}
		return stringBuilder;
	}

	internal StringBuilder GetAsString(OfficeMathGroupCharacter officeMathGroupCharacter)
	{
		StringBuilder stringBuilder = new StringBuilder();
		string text = (officeMathGroupCharacter.HasAlignTop ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (officeMathGroupCharacter.HasCharacterTop ? "1" : "0");
		stringBuilder.Append(text + ";");
		stringBuilder.Append(officeMathGroupCharacter.GroupCharacter + ";");
		if (officeMathGroupCharacter.Equation != null)
		{
			stringBuilder.Append(GetAsString(officeMathGroupCharacter.Equation as OfficeMath));
		}
		return stringBuilder;
	}

	internal StringBuilder GetAsString(OfficeMathFunctionBase officeMathFunctionBase)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (officeMathFunctionBase != null && officeMathFunctionBase is OfficeMathLimit)
		{
			stringBuilder.Append(GetAsString(officeMathFunctionBase as OfficeMathLimit));
		}
		else if (officeMathFunctionBase != null && officeMathFunctionBase is OfficeMathDelimiter)
		{
			stringBuilder.Append(GetAsString(officeMathFunctionBase as OfficeMathDelimiter));
		}
		else if (officeMathFunctionBase != null && officeMathFunctionBase is OfficeMathGroupCharacter)
		{
			stringBuilder.Append(GetAsString(officeMathFunctionBase as OfficeMathGroupCharacter));
		}
		else if (officeMathFunctionBase != null && officeMathFunctionBase is OfficeMathLeftScript)
		{
			stringBuilder.Append(GetAsString(officeMathFunctionBase as OfficeMathLeftScript));
		}
		else if (officeMathFunctionBase != null && officeMathFunctionBase is OfficeMathFunction)
		{
			stringBuilder.Append(GetAsString(officeMathFunctionBase as OfficeMathFunction));
		}
		else if (officeMathFunctionBase != null && officeMathFunctionBase is OfficeMathFraction)
		{
			stringBuilder.Append(GetAsString(officeMathFunctionBase as OfficeMathFraction));
		}
		else if (officeMathFunctionBase != null && officeMathFunctionBase is OfficeMathEquationArray)
		{
			stringBuilder.Append(GetAsString(officeMathFunctionBase as OfficeMathEquationArray));
		}
		else if (officeMathFunctionBase != null && officeMathFunctionBase is OfficeMathBorderBox)
		{
			stringBuilder.Append(GetAsString(officeMathFunctionBase as OfficeMathBorderBox));
		}
		else if (officeMathFunctionBase != null && officeMathFunctionBase is OfficeMathMatrix)
		{
			stringBuilder.Append(GetAsString(officeMathFunctionBase as OfficeMathMatrix));
		}
		else if (officeMathFunctionBase != null && officeMathFunctionBase is OfficeMathNArray)
		{
			stringBuilder.Append(GetAsString(officeMathFunctionBase as OfficeMathNArray));
		}
		else if (officeMathFunctionBase != null && officeMathFunctionBase is OfficeMathRadical)
		{
			stringBuilder.Append(GetAsString(officeMathFunctionBase as OfficeMathRadical));
		}
		else if (officeMathFunctionBase != null && officeMathFunctionBase is OfficeMathScript)
		{
			stringBuilder.Append(GetAsString(officeMathFunctionBase as OfficeMathScript));
		}
		else if (officeMathFunctionBase != null && officeMathFunctionBase is OfficeMathBox)
		{
			stringBuilder.Append(GetAsString(officeMathFunctionBase as OfficeMathBox));
		}
		else if (officeMathFunctionBase != null && officeMathFunctionBase is OfficeMathAccent)
		{
			stringBuilder.Append(GetAsString(officeMathFunctionBase as OfficeMathAccent));
		}
		else if (officeMathFunctionBase != null && officeMathFunctionBase is OfficeMathPhantom)
		{
			stringBuilder.Append(GetAsString(officeMathFunctionBase as OfficeMathPhantom));
		}
		else if (officeMathFunctionBase != null && officeMathFunctionBase is OfficeMathBar)
		{
			stringBuilder.Append(GetAsString(officeMathFunctionBase as OfficeMathBar));
		}
		else if (officeMathFunctionBase != null && officeMathFunctionBase is OfficeMathPhantom)
		{
			stringBuilder.Append(GetAsString(officeMathFunctionBase as OfficeMathPhantom));
		}
		else if (officeMathFunctionBase != null && officeMathFunctionBase is OfficeMathRunElement)
		{
			stringBuilder.Append(GetAsString(officeMathFunctionBase as OfficeMathRunElement));
		}
		return stringBuilder;
	}

	internal StringBuilder GetAsString(OfficeMathRightScript officeMathRightScript)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(officeMathRightScript.IsSkipAlign + ";");
		if (officeMathRightScript.Equation != null)
		{
			stringBuilder.Append(GetAsString(officeMathRightScript.Equation as OfficeMath));
		}
		if (officeMathRightScript.Subscript != null)
		{
			stringBuilder.Append(GetAsString(officeMathRightScript.Subscript as OfficeMath));
		}
		if (officeMathRightScript.Superscript != null)
		{
			stringBuilder.Append(GetAsString(officeMathRightScript.Superscript as OfficeMath));
		}
		return stringBuilder;
	}

	internal StringBuilder GetAsString(OfficeMathRunElement mathRunElement)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (mathRunElement.Item != null && mathRunElement.Item is WTextRange)
		{
			stringBuilder.Append((mathRunElement.Item as WTextRange).Text);
		}
		if (mathRunElement.MathFormat != null)
		{
			stringBuilder.Append(GetAsString(mathRunElement.MathFormat as OfficeMathFormat));
		}
		return stringBuilder;
	}

	internal StringBuilder GetAsString(OfficeMathFormat officeMathFormat)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(officeMathFormat.HasAlignment + ";");
		stringBuilder.Append(officeMathFormat.HasLiteral + ";");
		stringBuilder.Append(officeMathFormat.HasNormalText + ";");
		stringBuilder.Append((int)officeMathFormat.Font + ";");
		stringBuilder.Append((int)officeMathFormat.Style + ";");
		if (officeMathFormat.Break != null)
		{
			stringBuilder.Append(GetAsString(officeMathFormat.Break as OfficeMathBreak));
		}
		return stringBuilder;
	}

	internal StringBuilder GetAsString(OfficeMath officeMath)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(officeMath.AlignPoint + ";");
		stringBuilder.Append(officeMath.ArgumentSize + ";");
		stringBuilder.Append(officeMath.NestingLevel + ";");
		if (officeMath.Functions != null)
		{
			for (int i = 0; i < officeMath.Functions.Count; i++)
			{
				OfficeMathFunctionBase officeMathFunctionBase = officeMath.Functions[i] as OfficeMathFunctionBase;
				stringBuilder.Append(GetAsString(officeMathFunctionBase));
			}
		}
		if (officeMath.Breaks != null)
		{
			for (int j = 0; j < officeMath.Breaks.Count; j++)
			{
				if (officeMath.Breaks[j] is OfficeMathBreak officeMathBreak)
				{
					stringBuilder.Append(GetAsString(officeMathBreak));
				}
			}
		}
		return stringBuilder;
	}

	internal StringBuilder GetAsString(OfficeMathPhantom officeMathPhantom)
	{
		StringBuilder stringBuilder = new StringBuilder();
		string text = (officeMathPhantom.Show ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (officeMathPhantom.Smash ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (officeMathPhantom.Transparent ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (officeMathPhantom.ZeroAscent ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (officeMathPhantom.ZeroDescent ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (officeMathPhantom.ZeroWidth ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (officeMathPhantom.IsDefault ? "1" : "0");
		stringBuilder.Append(text + ";");
		if (officeMathPhantom.Equation != null)
		{
			stringBuilder.Append(GetAsString(officeMathPhantom.Equation as OfficeMath));
		}
		return stringBuilder;
	}

	internal StringBuilder GetAsString(OfficeMathFraction officeMathFraction)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append((int)officeMathFraction.FractionType + ";");
		if (officeMathFraction.Denominator != null)
		{
			stringBuilder.Append(GetAsString(officeMathFraction.Denominator as OfficeMath));
		}
		if (officeMathFraction.Numerator != null)
		{
			stringBuilder.Append(GetAsString(officeMathFraction.Numerator as OfficeMath));
		}
		return stringBuilder;
	}

	internal StringBuilder GetAsString(OfficeMathFunction officeMathFunction)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (officeMathFunction.Equation != null)
		{
			stringBuilder.Append(GetAsString(officeMathFunction.Equation as OfficeMath));
		}
		if (officeMathFunction.FunctionName != null)
		{
			stringBuilder.Append(GetAsString(officeMathFunction.FunctionName as OfficeMath));
		}
		return stringBuilder;
	}

	internal StringBuilder GetAsString(OfficeMathEquationArray officeMathEquationArray)
	{
		StringBuilder stringBuilder = new StringBuilder();
		string text = (officeMathEquationArray.ExpandEquationContainer ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (officeMathEquationArray.ExpandEquationContent ? "1" : "0");
		stringBuilder.Append(text + ";");
		stringBuilder.Append((int)officeMathEquationArray.VerticalAlignment + ";");
		stringBuilder.Append(officeMathEquationArray.RowSpacing + ";");
		stringBuilder.Append((int)officeMathEquationArray.RowSpacingRule + ";");
		if (officeMathEquationArray.Equation != null)
		{
			stringBuilder.Append(GetAsString(officeMathEquationArray.Equation as OfficeMath));
		}
		return stringBuilder;
	}

	internal StringBuilder GetAsString(MatrixColumnProperties matrixColumnProperties)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(matrixColumnProperties.Count + ";");
		stringBuilder.Append((int)matrixColumnProperties.Alignment + ";");
		return stringBuilder;
	}

	internal StringBuilder GetAsString(OfficeMathMatrix officeMathMatrix)
	{
		StringBuilder stringBuilder = new StringBuilder();
		string text = (officeMathMatrix.HidePlaceHolders ? "1" : "0");
		stringBuilder.Append(text + ";");
		stringBuilder.Append((int)officeMathMatrix.VerticalAlignment + ";");
		stringBuilder.Append(officeMathMatrix.ColumnWidth + ";");
		stringBuilder.Append((int)officeMathMatrix.ColumnSpacingRule + ";");
		stringBuilder.Append(officeMathMatrix.ColumnSpacing + ";");
		stringBuilder.Append(officeMathMatrix.RowSpacing + ";");
		stringBuilder.Append((int)officeMathMatrix.RowSpacingRule + ";");
		if (officeMathMatrix.Columns != null)
		{
			for (int i = 0; i < officeMathMatrix.Columns.Count; i++)
			{
				if (officeMathMatrix.Columns[i] is OfficeMathMatrixColumn officeMathMatrixColumn)
				{
					stringBuilder.Append(GetAsString(officeMathMatrixColumn));
				}
			}
		}
		if (officeMathMatrix.Rows != null)
		{
			for (int j = 0; j < officeMathMatrix.Rows.Count; j++)
			{
				if (officeMathMatrix.Rows[j] is OfficeMathMatrixRow officeMathMatrixRow)
				{
					stringBuilder.Append(GetAsString(officeMathMatrixRow));
				}
			}
		}
		if (officeMathMatrix.ColumnProperties != null)
		{
			foreach (MatrixColumnProperties columnProperty in officeMathMatrix.ColumnProperties)
			{
				stringBuilder.Append(GetAsString(columnProperty));
			}
		}
		return stringBuilder;
	}

	internal StringBuilder GetAsString(OfficeMathDelimiter officeMathDelimiter)
	{
		StringBuilder stringBuilder = new StringBuilder();
		string text = (officeMathDelimiter.IsGrow ? "1" : "0");
		stringBuilder.Append(text + ";");
		stringBuilder.Append(officeMathDelimiter.BeginCharacter + ";");
		stringBuilder.Append(officeMathDelimiter.EndCharacter + ";");
		stringBuilder.Append(officeMathDelimiter.Seperator + ";");
		stringBuilder.Append((int)officeMathDelimiter.DelimiterShape + ";");
		if (officeMathDelimiter.Equation != null)
		{
			stringBuilder.Append(GetAsString(officeMathDelimiter.Equation as OfficeMath));
		}
		return stringBuilder;
	}

	internal StringBuilder GetAsString(OfficeMathAccent officeMathAccent)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(officeMathAccent.AccentCharacter + ";");
		if (officeMathAccent.Equation != null)
		{
			stringBuilder.Append(GetAsString(officeMathAccent.Equation as OfficeMath));
		}
		return stringBuilder;
	}

	internal StringBuilder GetAsString(OfficeMathScript officeMathScript)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append((int)officeMathScript.ScriptType + ";");
		if (officeMathScript.Equation != null)
		{
			stringBuilder.Append(GetAsString(officeMathScript.Equation as OfficeMath));
		}
		if (officeMathScript.Script != null)
		{
			stringBuilder.Append(GetAsString(officeMathScript.Script as OfficeMath));
		}
		return stringBuilder;
	}

	internal StringBuilder GetAsString(OfficeMathRadical officeMathRadical)
	{
		StringBuilder stringBuilder = new StringBuilder();
		string text = (officeMathRadical.HideDegree ? "1" : "0");
		stringBuilder.Append(text + ";");
		if (officeMathRadical.Equation != null)
		{
			stringBuilder.Append(GetAsString(officeMathRadical.Equation as OfficeMath));
		}
		if (officeMathRadical.Degree != null)
		{
			stringBuilder.Append(GetAsString(officeMathRadical.Degree as OfficeMath));
		}
		return stringBuilder;
	}

	internal StringBuilder GetAsString(OfficeMathMatrixColumn officeMathMatrixColumn)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(officeMathMatrixColumn.ColumnIndex + ";");
		stringBuilder.Append(officeMathMatrixColumn.HorizontalAlignment.ToString() + ";");
		if (officeMathMatrixColumn.Arguments != null)
		{
			for (int i = 0; i < officeMathMatrixColumn.Arguments.Count; i++)
			{
				if (officeMathMatrixColumn.Arguments[i] is OfficeMathMatrixRow officeMathMatrixRow)
				{
					stringBuilder.Append(GetAsString(officeMathMatrixRow));
				}
			}
		}
		return stringBuilder;
	}

	internal StringBuilder GetAsString(OfficeMathMatrixRow officeMathMatrixRow)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(officeMathMatrixRow.RowIndex + ";");
		if (officeMathMatrixRow.Arguments.Count > 0)
		{
			for (int i = 0; i < officeMathMatrixRow.Arguments.Count; i++)
			{
				if (officeMathMatrixRow.Arguments[i] is OfficeMath officeMath)
				{
					stringBuilder.Append(GetAsString(officeMath));
				}
			}
		}
		return stringBuilder;
	}

	internal StringBuilder GetAsString(OfficeMathBar officeMathBar)
	{
		StringBuilder stringBuilder = new StringBuilder();
		string text = (officeMathBar.BarTop ? "1" : "0");
		stringBuilder.Append(text + ";");
		if (officeMathBar.Equation != null)
		{
			stringBuilder.Append(GetAsString(officeMathBar.Equation as OfficeMath));
		}
		return stringBuilder;
	}

	internal StringBuilder GetAsString(OfficeMathNArray officeMathNArray)
	{
		StringBuilder stringBuilder = new StringBuilder();
		string text = (officeMathNArray.HasGrow ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (officeMathNArray.HideLowerLimit ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (officeMathNArray.HideUpperLimit ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (officeMathNArray.SubSuperscriptLimit ? "1" : "0");
		stringBuilder.Append(text + ";");
		stringBuilder.Append(officeMathNArray.NArrayCharacter + ";");
		if (officeMathNArray.Equation != null)
		{
			stringBuilder.Append(GetAsString(officeMathNArray.Equation as OfficeMath));
		}
		if (officeMathNArray.Subscript != null)
		{
			stringBuilder.Append(GetAsString(officeMathNArray.Subscript as OfficeMath));
		}
		if (officeMathNArray.Superscript != null)
		{
			stringBuilder.Append(GetAsString(officeMathNArray.Superscript as OfficeMath));
		}
		return stringBuilder;
	}

	internal StringBuilder GetAsString(OfficeMathLimit officeMathLimit)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append((int)officeMathLimit.LimitType + ";");
		if (officeMathLimit.Equation != null)
		{
			stringBuilder.Append(GetAsString(officeMathLimit.Equation as OfficeMath));
		}
		if (officeMathLimit.Limit != null)
		{
			stringBuilder.Append(GetAsString(officeMathLimit.Limit as OfficeMath));
		}
		return stringBuilder;
	}

	internal StringBuilder GetAsString(OfficeMathBorderBox officeMathBorderBox)
	{
		StringBuilder stringBuilder = new StringBuilder();
		string text = (officeMathBorderBox.HideTop ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (officeMathBorderBox.HideBottom ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (officeMathBorderBox.HideRight ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (officeMathBorderBox.HideLeft ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (officeMathBorderBox.StrikeDiagonalUp ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (officeMathBorderBox.StrikeDiagonalDown ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (officeMathBorderBox.StrikeVertical ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (officeMathBorderBox.StrikeHorizontal ? "1" : "0");
		stringBuilder.Append(text + ";");
		if (officeMathBorderBox.Equation != null)
		{
			stringBuilder.Append(GetAsString(officeMathBorderBox.Equation as OfficeMath));
		}
		return stringBuilder;
	}

	internal StringBuilder GetAsString(OfficeMathBox officeMathBox)
	{
		StringBuilder stringBuilder = new StringBuilder();
		string text = (officeMathBox.Alignment ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (officeMathBox.EnableDifferential ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (officeMathBox.NoBreak ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (officeMathBox.OperatorEmulator ? "1" : "0");
		stringBuilder.Append(text + ";");
		if (officeMathBox.Equation != null)
		{
			stringBuilder.Append(GetAsString(officeMathBox.Equation as OfficeMath));
		}
		if (officeMathBox.Break != null)
		{
			stringBuilder.Append(GetAsString(officeMathBox.Break as OfficeMathBreak));
		}
		return stringBuilder;
	}

	internal StringBuilder GetAsString(OfficeMathBreak officeMathBreak)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(officeMathBreak.AlignAt + ";");
		return stringBuilder;
	}

	internal StringBuilder GetAsString(OfficeMathParagraph officeMathParagraph)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (officeMathParagraph != null)
		{
			string text = (officeMathParagraph.IsDefault ? "1" : "0");
			stringBuilder.Append(text + ";");
			stringBuilder.Append((int)officeMathParagraph.Justification + ";");
			if (officeMathParagraph.Maths.Count > 0)
			{
				for (int i = 0; i < officeMathParagraph.Maths.Count; i++)
				{
					if (officeMathParagraph.Maths[i] is OfficeMath officeMath)
					{
						stringBuilder.Append(GetAsString(officeMath));
					}
				}
			}
		}
		return stringBuilder;
	}
}
