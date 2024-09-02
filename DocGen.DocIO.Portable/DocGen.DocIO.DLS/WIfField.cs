using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DocGen.Layouting;

namespace DocGen.DocIO.DLS;

public class WIfField : WField
{
	private const char FieldTextStart = '\u0013';

	private const char FieldTextEnd = '\u0015';

	private const string PARAGRAPHMARK = "\r";

	private string m_expression1;

	private string m_expression2;

	private string m_operator;

	private string m_trueText;

	private string m_falseText;

	private int m_inc;

	private PseudoMergeField m_expField1;

	private PseudoMergeField m_expField2;

	private List<Entity> m_trueTextField;

	private List<Entity> m_falseTextField;

	private List<PseudoMergeField> m_mergeFields;

	private WFieldMark nestedFieldEnd;

	internal PseudoMergeField Expression1
	{
		get
		{
			if (m_expField1 == null)
			{
				CheckExpStrings();
				m_expField1 = new PseudoMergeField(m_expression1);
			}
			return m_expField1;
		}
	}

	internal PseudoMergeField Expression2
	{
		get
		{
			if (m_expField2 == null)
			{
				CheckExpStrings();
				m_expField2 = new PseudoMergeField(m_expression2);
			}
			return m_expField2;
		}
	}

	internal List<Entity> TrueTextField
	{
		get
		{
			if (m_trueTextField == null)
			{
				m_trueTextField = new List<Entity>();
			}
			return m_trueTextField;
		}
	}

	internal List<Entity> FalseTextField
	{
		get
		{
			if (m_falseTextField == null)
			{
				m_falseTextField = new List<Entity>();
			}
			return m_falseTextField;
		}
	}

	internal List<PseudoMergeField> MergeFields
	{
		get
		{
			if (m_mergeFields == null)
			{
				m_mergeFields = new List<PseudoMergeField>();
				UpdateMergeFields();
			}
			return m_mergeFields;
		}
	}

	public WIfField(IWordDocument doc)
		: base(doc)
	{
	}

	internal void UpdateIfField()
	{
		bool flag = false;
		ParseResult();
		string numberFormat = string.Empty;
		string text = RemoveMergeFormat(base.FieldCode, ref numberFormat);
		text = RemoveText(text, "if");
		List<int> operatorIndexForDoubleQuotes = null;
		string operatorValue = null;
		List<string> list = SplitIfArguments(text, ref operatorIndexForDoubleQuotes, ref operatorValue);
		if (list.Count > 0)
		{
			if (list[0].Contains('\u0013'.ToString()) && list[0].Contains('\u0015'.ToString()))
			{
				list[0] = list[0].Replace('\u0013'.ToString(), "").Replace('\u0015'.ToString(), "");
			}
			flag = UpdateCondition(list[0], operatorIndexForDoubleQuotes, operatorValue) == "1";
		}
		List<Entity> list2 = new List<Entity>();
		list2 = (flag ? TrueTextField : FalseTextField);
		bool flag2 = HaveAutoNumFieldInResult(list2);
		try
		{
			if (!flag2)
			{
				UpdateIfFieldResult(flag);
			}
			else
			{
				RemoveFieldSeparatorAndResultForAutoNumField(base.FieldSeparator);
			}
		}
		catch (Exception)
		{
			base.FieldResult = "Error! Unknown op code for conditional.";
		}
	}

	private void RemoveFieldSeparatorAndResultForAutoNumField(WFieldMark fieldMark)
	{
		WField parentField = fieldMark.ParentField;
		WParagraph ownerParagraph = fieldMark.OwnerParagraph;
		WTextRange entity = parentField.FieldSeparator.NextSibling as WTextRange;
		ownerParagraph.Items.Remove(entity);
		ownerParagraph.Items.Remove(parentField.FieldSeparator);
	}

	private bool HaveAutoNumFieldInResult(List<Entity> fieldResult)
	{
		for (int i = 0; i < fieldResult.Count; i++)
		{
			if (fieldResult[i] is WField && (fieldResult[i] as WField).FieldType == FieldType.FieldAutoNum)
			{
				return true;
			}
		}
		return false;
	}

	private string GetEntityText(Entity entity, bool isFirstCall)
	{
		if (isFirstCall)
		{
			base.IsFieldSeparator = false;
			base.IsSkip = false;
			m_nestedFields.Clear();
		}
		string text = string.Empty;
		if (entity is ParagraphItem)
		{
			text = UpdateTextForParagraphItem(entity, isUpdateNestedFields: true);
		}
		else if (entity is WParagraph)
		{
			text = UpdateTextForTextBodyItem(entity, isUpdateNestedFields: true);
		}
		return text.Replace(ControlChar.CarriegeReturn, string.Empty);
	}

	private string ConvertFieldResultToString(List<Entity> fieldResult)
	{
		string text = string.Empty;
		bool isFirstCall = true;
		foreach (Entity item in fieldResult)
		{
			text += GetEntityText(item, isFirstCall);
			isFirstCall = false;
		}
		return text;
	}

	private void UpdateIfFieldResult(bool result)
	{
		string text = RemoveMergeFormat(base.FieldCode);
		string text2 = string.Empty;
		int num = -1;
		if (text.Contains("\\*"))
		{
			num = text.LastIndexOf("\\*");
			text2 = text.Substring(num);
		}
		else if (text.Contains("\\@"))
		{
			num = text.LastIndexOf("\\@");
			text2 = text.Substring(num);
		}
		List<Entity> list = new List<Entity>();
		list = (result ? TrueTextField : FalseTextField);
		RemoveNestedSetField(ref list);
		base.FieldResult = ConvertFieldResultToString(list);
		double result2 = 0.0;
		bool flag = false;
		if (double.TryParse(base.FieldResult, out result2))
		{
			flag = true;
		}
		if (!(base.Owner is WParagraph) || base.FieldEnd == null)
		{
			return;
		}
		CheckFieldSeparator();
		RemovePreviousResult();
		if (base.OwnerParagraph == base.FieldEnd.OwnerParagraph)
		{
			if (!string.IsNullOrEmpty(text2) && list.Count > 1 && IsAllFieldResultTextRange(list))
			{
				(list[0] as WTextRange).Text = base.FieldResult;
				RemoveOtherTextRange(list);
			}
			for (int i = 0; i < list.Count; i++)
			{
				int indexInOwnerCollection = base.FieldEnd.GetIndexInOwnerCollection();
				if (list[i] is WTextRange)
				{
					if (flag && string.IsNullOrEmpty(text2))
					{
						string numberFormat = string.Empty;
						text = RemoveMergeFormat(base.FieldCode, ref numberFormat);
						if (!string.IsNullOrEmpty(numberFormat))
						{
							(list[i] as WTextRange).Text = UpdateNumberFormat((list[i] as WTextRange).Text, numberFormat);
						}
					}
					else if (!string.IsNullOrEmpty(text2) && num > text.IndexOf("\\#"))
					{
						(list[i] as WTextRange).Text = UpdateTextFormat((list[i] as WTextRange).Text, text2);
					}
					else if (!string.IsNullOrEmpty(text2) && flag)
					{
						(list[i] as WTextRange).Text = "Error! Picture switch must be first formatting switch.";
					}
				}
				base.OwnerParagraph.Items.Insert(indexInOwnerCollection, list[i]);
				if (base.OwnerParagraph.ChildEntities[indexInOwnerCollection] is InlineContentControl)
				{
					foreach (ParagraphItem paragraphItem in (base.OwnerParagraph.ChildEntities[indexInOwnerCollection] as InlineContentControl).ParagraphItems)
					{
						((IWidget)paragraphItem).InitLayoutInfo();
					}
				}
				(base.OwnerParagraph.ChildEntities[indexInOwnerCollection] as IWidget).InitLayoutInfo();
			}
		}
		else if (base.FieldSeparator.OwnerParagraph != base.FieldEnd.OwnerParagraph)
		{
			WTextBody ownerTextBody = base.FieldEnd.OwnerParagraph.OwnerTextBody;
			if (list.Count == 0)
			{
				MergeFieldSeparatorAndFieldEndParagraph();
			}
			else
			{
				for (int j = 0; j < list.Count; j++)
				{
					if (list[j] is ParagraphItem)
					{
						if (list[j].EntityType != EntityType.TextRange || !((list[j] as WTextRange).Text == string.Empty))
						{
							base.FieldSeparator.OwnerParagraph.Items.Add(list[j]);
						}
						if (j == list.Count - 1)
						{
							MergeFieldSeparatorAndFieldEndParagraph();
						}
					}
					else if (j == list.Count - 1 && list[j] is WParagraph)
					{
						int count = (list[j] as WParagraph).ChildEntities.Count;
						for (int k = 0; k < count; k++)
						{
							Entity entity = (list[j] as WParagraph).ChildEntities[(list[j] as WParagraph).ChildEntities.Count - 1];
							base.FieldEnd.OwnerParagraph.ChildEntities.Insert(0, entity);
						}
					}
					else
					{
						int indexInOwnerCollection2 = base.FieldEnd.OwnerParagraph.GetIndexInOwnerCollection();
						ownerTextBody.Items.Insert(indexInOwnerCollection2, list[j]);
					}
				}
			}
		}
		else
		{
			UpdateIfFieldResult(list);
		}
		base.IsFieldRangeUpdated = false;
	}

	private bool IsAllFieldResultTextRange(List<Entity> fieldResult)
	{
		foreach (Entity item in fieldResult)
		{
			if (!(item is WTextRange) && !(item is BookmarkStart) && !(item is BookmarkEnd))
			{
				return false;
			}
		}
		return true;
	}

	private void RemoveOtherTextRange(List<Entity> fieldResult)
	{
		for (int i = 1; i < fieldResult.Count; i++)
		{
			if (fieldResult[i] is WTextRange)
			{
				fieldResult.Remove(fieldResult[i]);
				i--;
			}
		}
	}

	private void RemoveNestedSetField(ref List<Entity> fieldResult)
	{
		for (int i = 0; i < fieldResult.Count; i++)
		{
			if (fieldResult[i] is WField && (fieldResult[i] as WField).FieldType == FieldType.FieldSet)
			{
				WField wField = fieldResult[i] as WField;
				wField.UpdateSetFields();
				fieldResult.Remove(wField);
				i--;
			}
		}
	}

	private void UpdateIfFieldResult(List<Entity> fieldResult)
	{
		int num = base.FieldSeparator.GetIndexInOwnerCollection() + 1;
		WTextBody ownerTextBody = base.FieldSeparator.OwnerParagraph.OwnerTextBody;
		int num2 = base.FieldSeparator.OwnerParagraph.GetIndexInOwnerCollection();
		bool flag = true;
		for (int i = 0; i < fieldResult.Count; i++)
		{
			if (fieldResult[i] is ParagraphItem)
			{
				if ((fieldResult[i].EntityType == EntityType.TextRange && (fieldResult[i] as WTextRange).Text == string.Empty) || fieldResult[i].EntityType == EntityType.BookmarkStart || fieldResult[i].EntityType == EntityType.BookmarkEnd || fieldResult[i].EntityType == EntityType.EditableRangeStart || fieldResult[i].EntityType == EntityType.EditableRangeEnd)
				{
					continue;
				}
				if (flag && !(fieldResult[fieldResult.Count - 1] is ParagraphItem))
				{
					WParagraph ownerParagraph = base.FieldSeparator.OwnerParagraph;
					WParagraph wParagraph = fieldResult[i].Owner.Clone() as WParagraph;
					wParagraph.ClearItems();
					while (ownerParagraph.ChildEntities[0] != base.FieldSeparator)
					{
						wParagraph.ChildEntities.Add(ownerParagraph.ChildEntities[0]);
					}
					wParagraph.ChildEntities.Add(ownerParagraph.ChildEntities[0]);
					ownerTextBody.Items.Insert(num2, wParagraph);
					num = base.FieldSeparator.GetIndexInOwnerCollection() + 1;
					num2 = base.FieldSeparator.OwnerParagraph.GetIndexInOwnerCollection();
				}
				flag = false;
				base.FieldSeparator.OwnerParagraph.Items.Insert(num, fieldResult[i]);
				if (fieldResult[i] is InlineContentControl)
				{
					base.FieldSeparator.OwnerParagraph.HasSDTInlineItem = true;
				}
				num++;
			}
			else if (i == fieldResult.Count - 1)
			{
				if (fieldResult[i] is WParagraph)
				{
					MoveFieldEndToFieldResultLastPara(fieldResult[i] as WParagraph);
					ownerTextBody.Items.Insert(num2 + 1, fieldResult[i]);
					continue;
				}
				ownerTextBody.Items.Insert(num2 + 1, fieldResult[i]);
				num2++;
				WParagraph wParagraph2 = new WParagraph(base.Document);
				MoveFieldEndToFieldResultLastPara(wParagraph2);
				ownerTextBody.Items.Insert(num2 + 1, wParagraph2);
			}
			else
			{
				ownerTextBody.Items.Insert(num2 + 1, fieldResult[i]);
				num2++;
			}
		}
	}

	private void MergeFieldSeparatorAndFieldEndParagraph()
	{
		WTextBody ownerTextBody = base.FieldEnd.OwnerParagraph.OwnerTextBody;
		WParagraph ownerParagraph = base.FieldEnd.OwnerParagraph;
		int num;
		for (num = 0; num < ownerParagraph.ChildEntities.Count; num++)
		{
			WFieldMark wFieldMark = null;
			WFieldMark wFieldMark2 = null;
			if (ownerParagraph.ChildEntities[num] is WField && (ownerParagraph.ChildEntities[num] as WField).FieldEnd != null)
			{
				wFieldMark = (ownerParagraph.ChildEntities[num] as WField).FieldEnd;
				wFieldMark.SetOwner((ownerParagraph.ChildEntities[num] as WField).FieldEnd.OwnerParagraph);
				(ownerParagraph.ChildEntities[num] as WField).FieldEnd = null;
			}
			if (ownerParagraph.ChildEntities[num] is WField && (ownerParagraph.ChildEntities[num] as WField).FieldSeparator != null)
			{
				wFieldMark2 = (ownerParagraph.ChildEntities[num] as WField).FieldSeparator;
				wFieldMark2.SetOwner((ownerParagraph.ChildEntities[num] as WField).FieldSeparator.OwnerParagraph);
				(ownerParagraph.ChildEntities[num] as WField).FieldSeparator = null;
			}
			base.FieldSeparator.OwnerParagraph.ChildEntities.Add(ownerParagraph.ChildEntities[num]);
			if (base.FieldSeparator.OwnerParagraph.ChildEntities[base.FieldSeparator.OwnerParagraph.ChildEntities.Count - 1] is WField)
			{
				WField wField = base.FieldSeparator.OwnerParagraph.ChildEntities[base.FieldSeparator.OwnerParagraph.ChildEntities.Count - 1] as WField;
				if (wField.FieldEnd == null || !(wField.FieldEnd.Owner is WParagraph))
				{
					wField.FieldEnd = wFieldMark;
				}
				if (wField.FieldSeparator == null || !(wField.FieldSeparator.Owner is WParagraph))
				{
					wField.FieldSeparator = wFieldMark2;
				}
			}
			num--;
		}
		ownerTextBody.Items.Remove(ownerParagraph);
	}

	private void MoveFieldEndToFieldResultLastPara(WParagraph para)
	{
		int indexInOwnerCollection = base.FieldEnd.GetIndexInOwnerCollection();
		WParagraph ownerParagraph = base.FieldEnd.OwnerParagraph;
		int num;
		for (num = indexInOwnerCollection; num < ownerParagraph.ChildEntities.Count; num++)
		{
			WFieldMark wFieldMark = null;
			if (ownerParagraph.ChildEntities[num] is WField && (ownerParagraph.ChildEntities[num] as WField).FieldEnd != null)
			{
				wFieldMark = (ownerParagraph.ChildEntities[num] as WField).FieldEnd;
				wFieldMark.SetOwner((ownerParagraph.ChildEntities[num] as WField).FieldEnd.OwnerParagraph);
				(ownerParagraph.ChildEntities[num] as WField).FieldEnd = null;
			}
			para.ChildEntities.Add(ownerParagraph.ChildEntities[num]);
			if (para.ChildEntities[para.ChildEntities.Count - 1] is WField)
			{
				WField wField = para.ChildEntities[para.ChildEntities.Count - 1] as WField;
				if (wField.FieldEnd == null || !(wField.FieldEnd.Owner is WParagraph))
				{
					wField.FieldEnd = wFieldMark;
				}
				if (wField.FieldSeparator == null || !(wField.FieldSeparator.Owner is WParagraph))
				{
					wField.FieldSeparator = null;
				}
			}
			num--;
		}
		if (ownerParagraph.ChildEntities.Count == 0)
		{
			ownerParagraph.RemoveSelf();
		}
	}

	internal string ParseResult()
	{
		TrueTextField.Clear();
		FalseTextField.Clear();
		Entity entity = null;
		string expressionText = string.Empty;
		int num = 0;
		string text = string.Empty;
		bool expressionFound = false;
		bool readTrueText = false;
		bool readFalseText = false;
		bool flag = true;
		bool isContinuousAdd = false;
		bool isIfFieldResult = false;
		List<Entity> updatedRange = GetUpdatedRange();
		bool isFirstCall = true;
		bool isDoubleQuote = false;
		List<string> operators = new List<string>(new string[6] { "<=", ">=", "<>", "=", "<", ">" });
		string numberFormat = string.Empty;
		string text2 = RemoveMergeFormat(base.FieldCode, ref numberFormat);
		text2 = RemoveText(text2, "if");
		if (GetOperatorIndex(operators, text2, ref isDoubleQuote).Count == 0)
		{
			string text3 = string.Empty;
			for (int i = 0; i < updatedRange.Count; i++)
			{
				if (updatedRange[i].EntityType == EntityType.TextRange)
				{
					text3 += (updatedRange[i] as WTextRange).Text;
					if (text3.Contains("IF"))
					{
						expressionFound = true;
						readTrueText = true;
						(updatedRange[i] as WTextRange).Text = text3.Replace("IF", "");
						entity = updatedRange[i];
						num = i;
						break;
					}
				}
			}
		}
		while (entity != null || flag)
		{
			flag = false;
			if (!expressionFound)
			{
				ReadExpression(ref expressionText, ref entity, ref readTrueText, ref expressionFound, ref isFirstCall);
			}
			if (readTrueText && entity != null)
			{
				ReadTrueResult(ref text, ref entity, ref readTrueText, ref isIfFieldResult, ref isFirstCall);
			}
			if (!readTrueText && !readFalseText && expressionFound)
			{
				text = text.TrimStart();
				if (!StartsWithExt(text, ControlChar.DoubleQuoteString) && !StartsWithExt(text, ControlChar.RightDoubleQuoteString) && !StartsWithExt(text, ControlChar.LeftDoubleQuoteString) && !StartsWithExt(text, ControlChar.DoubleLowQuoteString) && (text.EndsWith(ControlChar.DoubleQuoteString) || text.EndsWith(ControlChar.RightDoubleQuoteString) || text.EndsWith(ControlChar.LeftDoubleQuoteString)))
				{
					isContinuousAdd = true;
				}
				text = string.Empty;
				readFalseText = true;
			}
			if (readFalseText && entity != null)
			{
				ReadFalseResult(ref text, ref entity, ref readFalseText, ref isContinuousAdd, ref isIfFieldResult, ref isFirstCall);
			}
			if (entity == null)
			{
				if (updatedRange.Count <= num || (!readTrueText && !readFalseText && expressionFound))
				{
					break;
				}
				entity = updatedRange[num];
				num++;
			}
			if (!expressionFound)
			{
				expressionText += GetEntityText(entity, isFirstCall);
				isFirstCall = false;
			}
		}
		TrimFieldResults(TrueTextField);
		TrimFieldResults(FalseTextField);
		return expressionText;
	}

	private List<Entity> GetUpdatedRange()
	{
		bool flag = false;
		List<Entity> entityList = new List<Entity>();
		List<Entity> entityList2 = new List<Entity>();
		int i = 0;
		bool flag2 = false;
		for (; i < base.Range.Items.Count; i++)
		{
			Entity entity = base.Range.Items[i] as Entity;
			if (entity is ParagraphItem)
			{
				if (base.FieldSeparator == entity || base.FieldEnd == entity)
				{
					flag = true;
				}
				else
				{
					if (nestedFieldEnd == null)
					{
						GetClonedParagraphItem(entity, ref entityList);
					}
					if (nestedFieldEnd != null && entity is WIfField)
					{
						base.Range.Items.Clear();
						UpdateFieldRange();
					}
					if (nestedFieldEnd != null && entity == nestedFieldEnd)
					{
						nestedFieldEnd = null;
					}
				}
			}
			if (entity is WParagraph)
			{
				if (!flag2 && !(entityList[entityList.Count - 1] is WParagraph) && nestedFieldEnd == null)
				{
					flag2 = true;
					WTextRange wTextRange = new WTextRange(entity.Document);
					wTextRange.Text = "\r".ToString();
					entityList.Add(wTextRange);
				}
				WParagraph wParagraph = entity.Clone() as WParagraph;
				WParagraph wParagraph2 = entity as WParagraph;
				int count = wParagraph2.ChildEntities.Count;
				WIfField wIfField = null;
				bool flag3 = false;
				bool flag4 = false;
				for (int j = 0; j < count; j++)
				{
					Entity entity2 = wParagraph2.ChildEntities[j];
					if (base.FieldSeparator == entity2 || base.FieldEnd == entity2)
					{
						flag = true;
						break;
					}
					if (nestedFieldEnd == null)
					{
						GetClonedParagraphItem(entity2, ref entityList2);
					}
					if (wIfField == null && nestedFieldEnd != null && entity2 is WIfField)
					{
						wIfField = entity2 as WIfField;
						base.Range.Items.Clear();
						UpdateFieldRange();
					}
					if (nestedFieldEnd != null && entity2 == nestedFieldEnd)
					{
						flag3 = ((nestedFieldEnd.OwnerParagraph != nestedFieldEnd.ParentField.OwnerParagraph) ? true : false);
						wIfField = null;
						nestedFieldEnd = null;
					}
					count = wParagraph2.ChildEntities.Count;
				}
				wParagraph.ClearItems();
				List<Entity> list = new List<Entity>();
				foreach (Entity item in entityList2)
				{
					if (item is ParagraphItem)
					{
						wParagraph.ChildEntities.Add(item);
					}
					else
					{
						list.Add(item);
					}
				}
				if (flag3 && entityList.Count > 0 && entityList[entityList.Count - 1] is WParagraph && (entityList[entityList.Count - 1] as WParagraph).Items.Count > 0 && (entityList[entityList.Count - 1] as WParagraph).LastItem is WFieldMark && ((entityList[entityList.Count - 1] as WParagraph).LastItem as WFieldMark).Type == FieldMarkType.FieldEnd)
				{
					WParagraph wParagraph3 = entityList[entityList.Count - 1] as WParagraph;
					while (wParagraph.ChildEntities.Count > 0)
					{
						wParagraph3.ChildEntities.Add(wParagraph.ChildEntities[0]);
						flag4 = true;
					}
				}
				else if ((wParagraph2.ChildEntities.Count == 0 || wParagraph.ChildEntities.Count > 0) && (nestedFieldEnd == null || (wIfField != null && wIfField.FieldEnd == nestedFieldEnd)))
				{
					entityList.Add(wParagraph);
					flag4 = true;
				}
				foreach (Entity item2 in list)
				{
					entityList.Add(item2);
					flag4 = true;
				}
				if (flag4)
				{
					entityList2.Clear();
				}
			}
			if (nestedFieldEnd == null)
			{
				entityList2.Clear();
			}
			if (entity is WTable && nestedFieldEnd == null)
			{
				entityList.Add(GetClonedTable(entity, isRefFieldUpdate: false));
			}
			if (flag)
			{
				break;
			}
		}
		entityList2.Clear();
		return entityList;
	}

	private void GetClonedFieldItem(Entity entity, ref List<Entity> entityList)
	{
		if (entity is WField && (((entity as WField).FieldSeparator != null && (entity as WField).FieldEnd != null) || (entity is WIfField && (entity as WField).FieldEnd != null)) && (entity as WField).FieldType != FieldType.FieldHyperlink)
		{
			(entity as WField).Update();
			nestedFieldEnd = (entity as WField).FieldEnd;
			if (entity is WIfField)
			{
				WIfField wIfField = (WIfField)entity.Clone();
				wIfField.IsSkip = true;
				wIfField.IsUpdated = true;
				wIfField.FieldEnd = null;
				wIfField.FieldSeparator = null;
				if (entityList.Count == 0 || entityList[entityList.Count - 1] is ParagraphItem)
				{
					entityList.Add(wIfField);
				}
				else if (entityList[entityList.Count - 1] is WParagraph)
				{
					(entityList[entityList.Count - 1] as WParagraph).ChildEntities.Add(wIfField);
				}
				else
				{
					WParagraph wParagraph = new WParagraph(entity.Document);
					wParagraph.ChildEntities.Add(wIfField);
					entityList.Add(wParagraph);
				}
				WTextRange wTextRange = new WTextRange(entity.Document);
				wTextRange.Text = '\u0013'.ToString();
				entityList.Add(wTextRange);
			}
			int num = 0;
			if ((entity as WField).FieldSeparator != null && (entity as WField).Range.Items.Contains((entity as WField).FieldSeparator))
			{
				num = (entity as WField).Range.Items.IndexOf((entity as WField).FieldSeparator) + 1;
			}
			else if ((entity as WField).FieldSeparator != null && (entity as WField).Range.Items.Contains((entity as WField).FieldSeparator.OwnerParagraph))
			{
				num = (entity as WField).Range.Items.IndexOf((entity as WField).FieldSeparator.OwnerParagraph);
			}
			for (int i = num; i < (entity as WField).Range.Items.Count && (entity as WField).Range.Items[i] != (entity as WField).FieldEnd; i++)
			{
				if ((entity as WField).FieldSeparator != null && (entity as WField).Range.Items[i] == (entity as WField).FieldSeparator.OwnerParagraph)
				{
					if (((entity as WField).Range.Items[i] as WParagraph).LastItem == (entity as WField).FieldSeparator)
					{
						WTextRange wTextRange2 = new WTextRange(entity.Document);
						wTextRange2.Text = "\r".ToString();
						entityList.Add(wTextRange2);
						continue;
					}
					WParagraph wParagraph2 = (entity as WField).FieldSeparator.OwnerParagraph.Clone() as WParagraph;
					for (int j = (entity as WField).FieldSeparator.GetIndexInOwnerCollection() + 1; j < (entity as WField).FieldSeparator.OwnerParagraph.ChildEntities.Count && (entity as WField).FieldEnd != (entity as WField).FieldSeparator.OwnerParagraph.ChildEntities[j]; j++)
					{
						entityList.Add(wParagraph2.ChildEntities[j]);
						if (j == (entity as WField).FieldSeparator.OwnerParagraph.ChildEntities.Count - 1 && (entity as WField).FieldEnd.OwnerParagraph != (entity as WField).FieldSeparator.OwnerParagraph)
						{
							WTextRange wTextRange3 = new WTextRange(entity.Document);
							wTextRange3.Text = "\r".ToString();
							entityList.Add(wTextRange3);
						}
					}
					continue;
				}
				if ((entity as WField).Range.Items[i] == (entity as WField).FieldEnd.OwnerParagraph)
				{
					WParagraph wParagraph3 = ((entity as WField).Range.Items[i] as Entity) as WParagraph;
					WParagraph wParagraph4 = wParagraph3.Clone() as WParagraph;
					wParagraph4.ClearItems();
					for (int k = 0; k < wParagraph3.ChildEntities.Count && (entity as WField).FieldEnd != wParagraph3.ChildEntities[k]; k++)
					{
						wParagraph4.Items.Add(wParagraph3.ChildEntities[k].Clone());
					}
					if ((entity as WField).FieldEnd != wParagraph3.ChildEntities[0])
					{
						entityList.Add(wParagraph4);
					}
					continue;
				}
				Entity entity2 = (entity as WField).Range.Items[i] as Entity;
				if (entity2 is WTextRange)
				{
					WTextRange wTextRange4 = entity2.Clone() as WTextRange;
					if (entity2.Owner is WParagraph)
					{
						int indexInOwnerCollection = entity2.GetIndexInOwnerCollection();
						wTextRange4 = ((entity2 as WTextRange).OwnerParagraph.Clone() as WParagraph)[indexInOwnerCollection] as WTextRange;
					}
					string empty = string.Empty;
					if ((entity as WField).FieldType != FieldType.FieldSet)
					{
						empty = wTextRange4.Text;
						wTextRange4.Text = "\u0013" + empty + "\u0015";
						entityList.Add(wTextRange4);
					}
				}
				else if (entity2 != null)
				{
					if (entity2.Owner is WParagraph)
					{
						int indexInOwnerCollection2 = entity2.GetIndexInOwnerCollection();
						WParagraph wParagraph5 = entity2.Owner.Clone() as WParagraph;
						entityList.Add(wParagraph5[indexInOwnerCollection2]);
					}
					else
					{
						entityList.Add(entity2.Clone());
					}
				}
			}
			if (entity is WIfField && entityList.Count > 0)
			{
				WTextRange wTextRange5 = new WTextRange(entity.Document);
				wTextRange5.Text = '\u0015'.ToString();
				WFieldMark wFieldMark = (WFieldMark)(entity as WField).FieldEnd.Clone();
				wFieldMark.SkipDocxItem = true;
				if (entityList[entityList.Count - 1] is ParagraphItem)
				{
					entityList.Add(wTextRange5);
					entityList.Add(wFieldMark);
					return;
				}
				if (entityList[entityList.Count - 1] is WParagraph)
				{
					(entityList[entityList.Count - 1] as WParagraph).ChildEntities.Add(wTextRange5);
					(entityList[entityList.Count - 1] as WParagraph).ChildEntities.Add(wFieldMark);
					return;
				}
				WParagraph wParagraph6 = (entity as WIfField).FieldEnd.OwnerParagraph.Clone() as WParagraph;
				wParagraph6.ChildEntities.Clear();
				wParagraph6.ChildEntities.Add(wTextRange5);
				wParagraph6.ChildEntities.Add(wFieldMark);
				entityList.Add(wParagraph6);
			}
		}
		else
		{
			entityList.Add(entity.Clone());
		}
	}

	private void GetClonedParagraphItem(Entity entity, ref List<Entity> entityList)
	{
		if (entity is WField)
		{
			if (!(entity as WField).IsFieldWithoutSeparator)
			{
				(entity as WField).CheckFieldSeparator();
			}
			if ((entity as WField).FieldType == FieldType.FieldSet)
			{
				entityList.Add(entity.Clone());
			}
			GetClonedFieldItem(entity, ref entityList);
		}
		else if (entity.Owner is WParagraph)
		{
			int indexInOwnerCollection = entity.GetIndexInOwnerCollection();
			WParagraph wParagraph = entity.Owner.Clone() as WParagraph;
			entityList.Add(wParagraph.ChildEntities[indexInOwnerCollection]);
		}
		else
		{
			entityList.Add(entity.Clone());
		}
	}

	private Entity SplitEntity(Entity entity, string remaining, ref bool isFirstCall)
	{
		Entity entity2 = entity.Clone();
		string text = GetEntityText(entity, isFirstCall);
		isFirstCall = false;
		if (text.Length - remaining.Length >= 0)
		{
			text = text.Substring(0, text.Length - remaining.Length);
		}
		if (entity is WTextRange)
		{
			(entity as WTextRange).Text = remaining;
			(entity2 as WTextRange).Text = text;
		}
		if (entity is WParagraph)
		{
			WParagraph wParagraph = entity2 as WParagraph;
			WParagraph wParagraph2 = entity as WParagraph;
			string entityText = GetEntityText(wParagraph, isFirstCall: false);
			int num = entityText.Length - remaining.Length;
			entityText = string.Empty;
			for (int i = 0; i < wParagraph.ChildEntities.Count; i++)
			{
				ParagraphItem entity3 = wParagraph.ChildEntities[i] as ParagraphItem;
				entityText += GetEntityText(entity3, isFirstCall: false);
				if (entityText.Length < num)
				{
					continue;
				}
				entityText = entityText.Remove(0, num);
				Entity entity4 = SplitEntity(wParagraph2.ChildEntities[i], entityText, ref isFirstCall);
				ParagraphItem paragraphItem;
				for (int j = 0; j < i; j++)
				{
					paragraphItem = wParagraph2.ChildEntities[0] as ParagraphItem;
					if (paragraphItem is WField)
					{
						(paragraphItem as WField).Document.IsSkipFieldDetach = true;
					}
					wParagraph2.ChildEntities.RemoveAt(0);
					if (paragraphItem is WField)
					{
						(paragraphItem as WField).Document.IsSkipFieldDetach = false;
					}
				}
				paragraphItem = wParagraph.ChildEntities[i] as ParagraphItem;
				if (paragraphItem is WField)
				{
					(paragraphItem as WField).Document.IsSkipFieldDetach = true;
				}
				wParagraph.ChildEntities.RemoveAt(i);
				if (paragraphItem is WField)
				{
					(paragraphItem as WField).Document.IsSkipFieldDetach = false;
				}
				wParagraph.ChildEntities.Insert(i, entity4);
				int count = wParagraph.ChildEntities.Count;
				if (i == count - 1)
				{
					break;
				}
				for (int k = i + 1; k < count; k++)
				{
					paragraphItem = wParagraph.ChildEntities[i + 1] as ParagraphItem;
					if (paragraphItem is WField)
					{
						(paragraphItem as WField).Document.IsSkipFieldDetach = true;
					}
					wParagraph.ChildEntities.RemoveAt(i + 1);
					if (paragraphItem is WField)
					{
						(paragraphItem as WField).Document.IsSkipFieldDetach = false;
					}
				}
				break;
			}
		}
		return entity2;
	}

	private void ReadExpression(ref string expressionText, ref Entity entity, ref bool readTrueText, ref bool expressionFound, ref bool isFirstCall)
	{
		string text = expressionText;
		if (ContainsOperator(ref text))
		{
			string text2 = text;
			if (ReachedEndOfExpression(ref text2))
			{
				expressionFound = true;
				expressionText = expressionText.Substring(0, expressionText.Length - text2.Length);
				if (entity == null)
				{
					entity = GetTextRange(text2);
				}
				else
				{
					SplitEntity(entity, text2, ref isFirstCall);
				}
				readTrueText = true;
			}
			else
			{
				entity = null;
			}
		}
		else
		{
			entity = null;
		}
	}

	private bool ReachedEndOfExpression(ref string text)
	{
		if (StartsWithExt(text, " "))
		{
			text = text.TrimStart(' ');
		}
		if (StartsWithExt(text, ControlChar.DoubleQuoteString) || StartsWithExt(text, ControlChar.RightDoubleQuoteString) || StartsWithExt(text, ControlChar.LeftDoubleQuoteString) || StartsWithExt(text, ControlChar.DoubleLowQuoteString))
		{
			if (text.Length > 0)
			{
				text = text.Remove(0, 1);
			}
			for (int i = 0; i < text.Length; i++)
			{
				if ((text[i] == ControlChar.DoubleQuote || text[i] == ControlChar.RightDoubleQuote || text[i] == ControlChar.LeftDoubleQuote) && IsNeedToSplitText(text, i))
				{
					text = ((text.Length - 1 == i) ? string.Empty : text.Substring(i + 1));
					return true;
				}
			}
		}
		else
		{
			if (StartsWithExt(text.TrimStart(), "\\*") && text.Substring(text.IndexOf("*") + 1).Trim().ToUpper() == "MERGEFORMAT")
			{
				return true;
			}
			for (int j = 0; j < text.Length; j++)
			{
				if ((text[j] == ' ' || text[j] == ControlChar.DoubleQuote || text[j] == ControlChar.RightDoubleQuote || text[j] == ControlChar.LeftDoubleQuote || text[j] == ControlChar.DoubleLowQuote) && IsNeedToSplitText(text, j))
				{
					text = ((text.Length - 1 == j) ? string.Empty : text.Substring(j + 1));
					return true;
				}
			}
		}
		return false;
	}

	private bool IsNeedToSplitText(string text, int i)
	{
		int num = -1;
		int num2 = -1;
		for (int j = 0; j < text.Length; j++)
		{
			if (text[j] == '\u0013')
			{
				num = j;
			}
			else if (text[j] == '\u0015')
			{
				num2 = j;
			}
			if (num < i && i < num2)
			{
				return false;
			}
		}
		if (num != -1 && num2 == -1 && i != 0 && i > num)
		{
			return false;
		}
		return true;
	}

	private bool ContainsOperator(ref string text)
	{
		if (text.Contains(ControlChar.DoubleQuoteString) || text.Contains(ControlChar.RightDoubleQuoteString) || text.Contains(ControlChar.LeftDoubleQuoteString) || text.Contains(ControlChar.DoubleLowQuoteString))
		{
			int indexOfDoubleQuote = GetIndexOfDoubleQuote(text);
			int indexOfOperator = GetIndexOfOperator(text);
			if (indexOfDoubleQuote < indexOfOperator)
			{
				text = text.Substring(indexOfDoubleQuote + 1);
				if (!text.Contains(ControlChar.DoubleQuoteString) && !text.Contains(ControlChar.RightDoubleQuoteString) && !text.Contains(ControlChar.LeftDoubleQuoteString) && !text.Contains(ControlChar.DoubleLowQuoteString))
				{
					return false;
				}
				int indexOfDoubleQuote2 = GetIndexOfDoubleQuote(text);
				if (text.Length - 1 == indexOfDoubleQuote2)
				{
					return false;
				}
				text = text.Substring(indexOfDoubleQuote2 + 1);
			}
		}
		string[] array = new string[6] { "<=", ">=", "<>", "=", "<", ">" };
		foreach (string text2 in array)
		{
			if (text.Contains(text2))
			{
				int startIndex = text.LastIndexOf(text2);
				text = text.Substring(startIndex);
				text = text.Replace(text2, string.Empty);
				return true;
			}
		}
		return false;
	}

	private int GetIndexOfOperator(string text)
	{
		string[] array = new string[6] { "<=", ">=", "<>", "=", "<", ">" };
		foreach (string value in array)
		{
			if (text.Contains(value))
			{
				return text.IndexOf(value);
			}
		}
		return -1;
	}

	private void ReadTrueResult(ref string text, ref Entity entity, ref bool readTrueText, ref bool isIfFieldResult, ref bool isFirstCall)
	{
		string text2 = text.Trim();
		if (entity is WIfField && (entity as WIfField).nestedFieldEnd == null && !isIfFieldResult)
		{
			entity = null;
			if (text.Trim() == string.Empty)
			{
				text += ControlChar.DoubleQuoteString;
				isIfFieldResult = true;
			}
			return;
		}
		if (entity is WParagraph && !isIfFieldResult)
		{
			WParagraph para = entity as WParagraph;
			int count = para.ChildEntities.Count;
			CheckIfField(ref para, ref text, ref isIfFieldResult);
			if (count == 1 && para.ChildEntities.Count == 0)
			{
				entity = null;
				return;
			}
		}
		if (entity is WFieldMark && (entity as WFieldMark).Type == FieldMarkType.FieldEnd && (entity as WFieldMark).SkipDocxItem)
		{
			if (isIfFieldResult)
			{
				text += ControlChar.DoubleQuoteString;
				isIfFieldResult = false;
			}
			string text3 = text;
			if (ReachedEndOfExpression(ref text3))
			{
				readTrueText = false;
				text = text.Substring(0, text.Length - text3.Length);
				entity = null;
			}
			else
			{
				entity = null;
			}
			return;
		}
		if (entity is WParagraph)
		{
			WParagraph para2 = entity as WParagraph;
			int count2 = para2.ChildEntities.Count;
			CheckIfFieldEnd(ref para2, ref isIfFieldResult);
			if (count2 == 1 && para2.ChildEntities.Count == 0)
			{
				entity = null;
				return;
			}
		}
		if (!(entity is WField) || (entity as WField).FieldType != FieldType.FieldSet)
		{
			text += GetEntityText(entity, isFirstCall);
		}
		isFirstCall = false;
		string text4 = text;
		if (ReachedEndOfExpression(ref text4))
		{
			Entity entity2 = SplitEntity(entity, text4, ref isFirstCall);
			if (entity2 != null)
			{
				TrueTextField.Add(entity2);
			}
			text = text.Substring(0, text.Length - text4.Length);
			readTrueText = false;
		}
		else if (entity is Break)
		{
			string text5 = text.Trim();
			if (StartsWithExt(text5, ControlChar.DoubleQuoteString) || StartsWithExt(text5, ControlChar.RightDoubleQuoteString) || StartsWithExt(text5, ControlChar.LeftDoubleQuoteString) || StartsWithExt(text5, ControlChar.DoubleLowQuoteString))
			{
				TrueTextField.Add(entity);
			}
			entity = null;
		}
		else if (TrueTextField.Count > 0 && (!(entity is WField) || (entity as WField).FieldType != FieldType.FieldSet) && StartsWithExt(GetEntityText(entity, isFirstCall), " ") && !StartsWithExt(text2, ControlChar.DoubleQuoteString) && !StartsWithExt(text2, ControlChar.RightDoubleQuoteString) && !StartsWithExt(text2, ControlChar.LeftDoubleQuoteString) && !StartsWithExt(text2, ControlChar.DoubleLowQuoteString) && HasRenderableParaItem(TrueTextField))
		{
			readTrueText = false;
		}
		else
		{
			TrueTextField.Add(entity);
			entity = null;
		}
	}

	private void ReadFalseResult(ref string text, ref Entity entity, ref bool readFalseText, ref bool isContinuousAdd, ref bool isIfFieldResult, ref bool isFirstCall)
	{
		string text2 = text;
		if (entity is WIfField && (entity as WIfField).nestedFieldEnd == null && !isIfFieldResult)
		{
			entity = null;
			if (text.Trim() == string.Empty)
			{
				text += ControlChar.DoubleQuoteString;
				isIfFieldResult = true;
			}
			return;
		}
		if (entity is WParagraph && !isIfFieldResult)
		{
			WParagraph para = entity as WParagraph;
			int count = para.ChildEntities.Count;
			CheckIfField(ref para, ref text, ref isIfFieldResult);
			if (count == 1 && para.ChildEntities.Count == 0)
			{
				entity = null;
				return;
			}
		}
		if (entity is WFieldMark && (entity as WFieldMark).Type == FieldMarkType.FieldEnd && (entity as WFieldMark).SkipDocxItem)
		{
			if (isIfFieldResult)
			{
				text += ControlChar.DoubleQuoteString;
				isIfFieldResult = false;
			}
			string text3 = text;
			if (ReachedEndOfExpression(ref text3))
			{
				readFalseText = false;
				text = string.Empty;
				entity = null;
			}
			else
			{
				entity = null;
			}
			return;
		}
		if (entity is WParagraph)
		{
			WParagraph para2 = entity as WParagraph;
			int count2 = para2.ChildEntities.Count;
			CheckIfFieldEnd(ref para2, ref isIfFieldResult);
			if (count2 == 1 && para2.ChildEntities.Count == 0)
			{
				entity = null;
				return;
			}
		}
		if (!isContinuousAdd)
		{
			if (!(entity is WField) || (entity as WField).FieldType != FieldType.FieldSet)
			{
				text += GetEntityText(entity, isFirstCall);
			}
			isFirstCall = false;
			string text4 = text;
			if (ReachedEndOfExpression(ref text4))
			{
				readFalseText = false;
				Entity entity2 = SplitEntity(entity, text4, ref isFirstCall);
				if (entity2 != null)
				{
					FalseTextField.Add(entity2);
				}
				text = string.Empty;
			}
			else if (entity is Break)
			{
				string text5 = text.Trim();
				if (StartsWithExt(text5, ControlChar.DoubleQuoteString) || StartsWithExt(text5, ControlChar.RightDoubleQuoteString) || StartsWithExt(text5, ControlChar.LeftDoubleQuoteString) || StartsWithExt(text5, ControlChar.DoubleLowQuoteString))
				{
					FalseTextField.Add(entity);
				}
			}
			else if (FalseTextField.Count > 0 && (!(entity is WField) || (entity as WField).FieldType != FieldType.FieldSet) && StartsWithExt(GetEntityText(entity, isFirstCall), " ") && !StartsWithExt(text2, ControlChar.DoubleQuoteString) && !StartsWithExt(text2, ControlChar.RightDoubleQuoteString) && !StartsWithExt(text2, ControlChar.LeftDoubleQuoteString) && !StartsWithExt(text2, ControlChar.DoubleLowQuoteString) && HasRenderableParaItem(FalseTextField))
			{
				readFalseText = false;
			}
			else
			{
				FalseTextField.Add(entity);
			}
		}
		else
		{
			string text6 = string.Empty;
			if (!(entity is WField) || (entity as WField).FieldType != FieldType.FieldSet)
			{
				text6 = GetEntityText(entity, isFirstCall);
			}
			isFirstCall = false;
			if (text6.Contains(ControlChar.DoubleQuoteString) || text6.Contains(ControlChar.LeftDoubleQuoteString) || text6.Contains(ControlChar.RightDoubleQuoteString) || text6.Contains(ControlChar.DoubleLowQuoteString))
			{
				string remaining = text6[..GetIndexOfDoubleQuote(text6)];
				Entity entity3 = SplitEntity(entity, remaining, ref isFirstCall);
				if (entity3 != null)
				{
					FalseTextField.Add(entity3);
				}
				readFalseText = false;
			}
			else if (entity is Break)
			{
				string text7 = text.Trim();
				if (StartsWithExt(text7, ControlChar.DoubleQuoteString) || StartsWithExt(text7, ControlChar.RightDoubleQuoteString) || StartsWithExt(text7, ControlChar.LeftDoubleQuoteString) || StartsWithExt(text7, ControlChar.DoubleLowQuoteString))
				{
					FalseTextField.Add(entity);
				}
			}
			else if (FalseTextField.Count > 0 && (!(entity is WField) || (entity as WField).FieldType != FieldType.FieldSet) && StartsWithExt(GetEntityText(entity, isFirstCall), " ") && !StartsWithExt(text2, ControlChar.DoubleQuoteString) && !StartsWithExt(text2, ControlChar.RightDoubleQuoteString) && !StartsWithExt(text2, ControlChar.LeftDoubleQuoteString) && !StartsWithExt(text2, ControlChar.DoubleLowQuoteString) && HasRenderableParaItem(FalseTextField))
			{
				readFalseText = false;
			}
			else
			{
				FalseTextField.Add(entity);
			}
		}
		entity = null;
	}

	private bool HasRenderableParaItem(List<Entity> resultItems)
	{
		for (int num = resultItems.Count - 1; num >= 0; num--)
		{
			Entity entity = resultItems[num];
			if (entity is WParagraph || entity is WTable)
			{
				return false;
			}
			if (!(entity is BookmarkStart) && !(entity is BookmarkEnd) && !(entity is EditableRangeStart) && !(entity is EditableRangeEnd))
			{
				if (entity is InlineContentControl)
				{
					Entity entity2 = (entity as InlineContentControl).ParagraphItems.LastItem;
					while (entity2 is BookmarkStart || entity2 is BookmarkEnd || entity2 is EditableRangeStart || entity2 is EditableRangeEnd)
					{
						entity2 = entity2.PreviousSibling as Entity;
					}
					if (!(entity2 is WPicture) && !(entity2 is WTextBox) && !(entity2 is Shape))
					{
						return entity2 is GroupShape;
					}
					return true;
				}
				if (!(entity is WPicture) && !(entity is WTextBox) && !(entity is Shape))
				{
					return entity is GroupShape;
				}
				return true;
			}
		}
		return false;
	}

	private void TrimFieldResults(List<Entity> fieldResult)
	{
		TrimEmptyText(fieldResult);
		TrimDoubleQuotes(fieldResult);
		if (fieldResult.Count > 0 && fieldResult[0] is WParagraph)
		{
			int num = 0;
			WParagraph item = fieldResult[0] as WParagraph;
			foreach (Entity childEntity in (fieldResult[0].Clone() as WParagraph).ChildEntities)
			{
				fieldResult.Insert(num++, childEntity);
			}
			fieldResult.Remove(item);
		}
		else
		{
			foreach (Entity item4 in fieldResult)
			{
				if (item4 is WTextRange && !string.IsNullOrEmpty((item4 as WTextRange).Text) && (item4 as WTextRange).Text == "\r")
				{
					(item4 as WTextRange).Text = (item4 as WTextRange).Text.Replace("\r".ToString(), string.Empty);
					break;
				}
				if (!(item4 is WParagraph))
				{
					continue;
				}
				int num2 = fieldResult.IndexOf(item4);
				WParagraph wParagraph = item4 as WParagraph;
				foreach (Entity childEntity2 in (item4.Clone() as WParagraph).ChildEntities)
				{
					fieldResult.Insert(num2++, childEntity2);
				}
				if (fieldResult.Count - 1 > fieldResult.IndexOf(item4) || (wParagraph.ChildEntities.Count != 0 && HasNoParagraphMark(wParagraph)))
				{
					fieldResult.Remove(wParagraph);
				}
				else
				{
					wParagraph.ChildEntities.Clear();
				}
				break;
			}
		}
		TrimParagraphmark(fieldResult);
	}

	private bool HasNoParagraphMark(WParagraph para)
	{
		for (int num = para.ChildEntities.Count - 1; num >= 0; num--)
		{
			if (para.ChildEntities[num] is WTextRange && !string.IsNullOrEmpty((para.ChildEntities[num] as WTextRange).Text))
			{
				if ((para.ChildEntities[num] as WTextRange).Text == "\r")
				{
					return false;
				}
				return true;
			}
		}
		return true;
	}

	private void TrimParagraphmark(List<Entity> fieldResult)
	{
		for (int i = 0; i < fieldResult.Count; i++)
		{
			Entity entity = fieldResult[i];
			if (entity is WTextRange && (entity as WTextRange).Text == "\r")
			{
				fieldResult.RemoveAt(i);
			}
			else if (entity is WParagraph && (entity as WParagraph).Text.Contains("\r"))
			{
				WParagraph wParagraph = entity as WParagraph;
				if (wParagraph.LastItem is WTextRange && (wParagraph.LastItem as WTextRange).Text == "\r")
				{
					wParagraph.LastItem.RemoveSelf();
				}
			}
		}
	}

	private void TrimEmptyText(List<Entity> fieldResult)
	{
		bool flag = false;
		for (int i = 0; i < fieldResult.Count; i++)
		{
			Entity entity = fieldResult[i];
			if (!(entity is WTextRange) && !(entity is WParagraph))
			{
				continue;
			}
			if ((entity is WTextRange && (entity as WTextRange).Text != null && (entity as WTextRange).Text == string.Empty) || (entity is WParagraph && (entity as WParagraph).Text != null && (entity as WParagraph).Text == string.Empty && !flag))
			{
				fieldResult.RemoveAt(i);
				i--;
			}
			else if (entity is WParagraph && ((entity as WParagraph).Text.StartsWith("\"") || (entity as WParagraph).Text.StartsWith('“'.ToString()) || (entity as WParagraph).Text.EndsWith("\"") || (entity as WParagraph).Text.EndsWith('“'.ToString()) || flag || (entity as WParagraph).Text.Contains(" ")))
			{
				if ((entity as WParagraph).Text.EndsWith("\""))
				{
					flag = false;
					break;
				}
				flag = true;
			}
			else if (entity is WTextRange && ((entity as WTextRange).Text.StartsWith("\"") || (entity as WTextRange).Text.StartsWith('“'.ToString()) || (entity as WTextRange).Text.EndsWith("\"") || (entity as WTextRange).Text.EndsWith('“'.ToString()) || flag || (entity as WTextRange).Text.Contains(" ")))
			{
				if ((entity as WTextRange).Text.EndsWith("\""))
				{
					flag = false;
					break;
				}
				flag = true;
			}
			else if (i < fieldResult.Count - 1)
			{
				fieldResult.RemoveRange(i + 1, fieldResult.Count - 1 - i);
				break;
			}
		}
	}

	private void TrimDoubleQuotes(List<Entity> fieldResult)
	{
		bool isStart = true;
		bool isEnd = false;
		int num = fieldResult.Count;
		for (int i = 0; i < fieldResult.Count; i++)
		{
			if (fieldResult[i] is WTextRange)
			{
				WTextRange textRange = fieldResult[i] as WTextRange;
				if (textRange.Text.Contains('\u0013'.ToString()) || textRange.Text.Contains('\u0015'.ToString()))
				{
					RemoveFieldTextStartEndChar(ref textRange, ref fieldResult, ref i);
				}
				else
				{
					TrimDoubleQuotesinTextRange(ref textRange, ref isStart, ref isEnd);
				}
			}
			if (isStart && fieldResult.Count != 0 && fieldResult[i] is WParagraph)
			{
				WParagraph wParagraph = fieldResult[i] as WParagraph;
				for (int j = 0; j < wParagraph.ChildEntities.Count; j++)
				{
					if ((fieldResult[i] as WParagraph).ChildEntities[j] is WTextRange)
					{
						WTextRange textRange2 = wParagraph.ChildEntities[j] as WTextRange;
						if (textRange2.Text.Contains('\u0013'.ToString()) || textRange2.Text.Contains('\u0015'.ToString()))
						{
							RemoveFieldTextStartEndChar(ref textRange2);
						}
						else
						{
							TrimDoubleQuotesinTextRange(ref textRange2, ref isStart, ref isEnd);
						}
						if (!isStart && textRange2.Text == string.Empty)
						{
							wParagraph.ChildEntities.RemoveAt(j);
						}
					}
				}
			}
			if (!isStart && !isEnd && fieldResult.Count != 0 && fieldResult[i] is WParagraph)
			{
				WParagraph wParagraph2 = fieldResult[i] as WParagraph;
				for (int k = 0; k < wParagraph2.ChildEntities.Count; k++)
				{
					ParagraphItem paragraphItem = wParagraph2.ChildEntities[k] as ParagraphItem;
					WField wField = null;
					if (paragraphItem is WField && (wField = paragraphItem as WField).FieldSeparator != null && (paragraphItem as WField).FieldEnd != null)
					{
						k = ((wField.FieldSeparator.OwnerParagraph != wParagraph2) ? wParagraph2.ChildEntities.Count : wField.FieldSeparator.Index);
					}
					else if (paragraphItem is WTextRange)
					{
						WTextRange textRange3 = wParagraph2.ChildEntities[k] as WTextRange;
						if (textRange3.Text.Contains('\u0013'.ToString()) || textRange3.Text.Contains('\u0015'.ToString()))
						{
							RemoveFieldTextStartEndChar(ref textRange3);
						}
						else
						{
							TrimDoubleQuotesinTextRange(ref textRange3, ref isStart, ref isEnd);
						}
						if (isEnd && textRange3.Text == string.Empty)
						{
							wParagraph2.ChildEntities.RemoveAt(k);
						}
					}
				}
				if (isEnd)
				{
					num = i;
				}
			}
			if (isEnd)
			{
				break;
			}
		}
		for (int num2 = fieldResult.Count - 1; num2 > num; num2--)
		{
			fieldResult.RemoveAt(num2);
		}
		m_inc = 0;
	}

	private void RemoveFieldTextStartEndChar(ref WTextRange textRange, ref List<Entity> fieldResult, ref int entityIndex)
	{
		if (textRange.Text.StartsWith('\u0013'.ToString()) && textRange.Text.EndsWith('\u0015'.ToString()))
		{
			textRange.Text = textRange.Text.Replace('\u0013'.ToString(), string.Empty).Replace('\u0015'.ToString(), string.Empty);
		}
		else if (textRange.Text.StartsWith('\u0013'.ToString()) && !textRange.Text.EndsWith('\u0015'.ToString()))
		{
			m_inc++;
			textRange.Text = textRange.Text.Replace('\u0013'.ToString(), string.Empty).Replace('\u0015'.ToString(), string.Empty);
		}
		else if (!textRange.Text.StartsWith('\u0013'.ToString()) && textRange.Text.EndsWith('\u0015'.ToString()))
		{
			m_inc--;
			textRange.Text = textRange.Text.Replace('\u0013'.ToString(), string.Empty).Replace('\u0015'.ToString(), string.Empty);
		}
		if (textRange.Text != null && textRange.Text == string.Empty)
		{
			fieldResult.RemoveAt(entityIndex);
			entityIndex--;
		}
	}

	private void RemoveFieldTextStartEndChar(ref WTextRange textRange)
	{
		if (textRange.Text.StartsWith('\u0013'.ToString()) && textRange.Text.EndsWith('\u0015'.ToString()))
		{
			textRange.Text = textRange.Text.Replace('\u0013'.ToString(), string.Empty).Replace('\u0015'.ToString(), string.Empty);
		}
		else if (textRange.Text.StartsWith('\u0013'.ToString()) && !textRange.Text.EndsWith('\u0015'.ToString()))
		{
			m_inc++;
			textRange.Text = textRange.Text.Replace('\u0013'.ToString(), string.Empty).Replace('\u0015'.ToString(), string.Empty);
		}
		else if (!textRange.Text.StartsWith('\u0013'.ToString()) && textRange.Text.EndsWith('\u0015'.ToString()))
		{
			m_inc--;
			textRange.Text = textRange.Text.Replace('\u0013'.ToString(), string.Empty).Replace('\u0015'.ToString(), string.Empty);
		}
	}

	private void TrimDoubleQuotesinTextRange(ref WTextRange textRange, ref bool isStart, ref bool isEnd)
	{
		int num = -1;
		if (isStart && StartsWithExt(textRange.Text, " "))
		{
			textRange.Text = textRange.Text.TrimStart();
		}
		if (isStart && !textRange.Text.Contains(ControlChar.DoubleLowQuoteString) && !StartsWithExt(textRange.Text, ControlChar.DoubleQuoteString) && !StartsWithExt(textRange.Text, ControlChar.RightDoubleQuoteString) && !StartsWithExt(textRange.Text, ControlChar.LeftDoubleQuoteString) && textRange.Text != string.Empty)
		{
			isStart = false;
		}
		if (isStart && (textRange.Text.Contains(ControlChar.DoubleLowQuoteString) || textRange.Text.Contains(ControlChar.DoubleQuoteString) || textRange.Text.Contains(ControlChar.RightDoubleQuoteString) || textRange.Text.Contains(ControlChar.LeftDoubleQuoteString)) && m_inc == 0)
		{
			string text = textRange.Text;
			num = GetIndexOfDoubleQuote(text);
			num++;
			textRange.Text = text.Substring(num, text.Length - num);
			isStart = false;
		}
		if (!isStart && !isEnd && (textRange.Text.EndsWith(ControlChar.DoubleLowQuoteString) || textRange.Text.EndsWith(ControlChar.DoubleQuoteString) || textRange.Text.EndsWith(ControlChar.RightDoubleQuoteString) || textRange.Text.EndsWith(ControlChar.LeftDoubleQuoteString)) && m_inc == 0)
		{
			string text2 = textRange.Text;
			num = GetIndexOfDoubleQuote(text2);
			textRange.Text = text2.Remove(num, text2.Length - num);
			isEnd = true;
		}
		else if (!isStart && !isEnd && (textRange.Text.Contains(ControlChar.DoubleLowQuoteString) || textRange.Text.Contains(ControlChar.DoubleQuoteString) || textRange.Text.Contains(ControlChar.RightDoubleQuoteString) || textRange.Text.Contains(ControlChar.LeftDoubleQuoteString)) && m_inc == 0)
		{
			string text3 = textRange.Text;
			num = GetIndexOfDoubleQuote(text3);
			num++;
			textRange.Text = text3.Substring(num, text3.Length - num);
			isEnd = true;
		}
	}

	private int GetIndexOfDoubleQuote(string text)
	{
		for (int i = 0; i < text.Length; i++)
		{
			if (text[i] == ControlChar.DoubleLowQuote || text[i] == ControlChar.DoubleQuote || text[i] == ControlChar.LeftDoubleQuote || text[i] == ControlChar.RightDoubleQuote)
			{
				return i;
			}
		}
		return 0;
	}

	private void CheckIfField(ref WParagraph para, ref string text, ref bool isIfFieldResult)
	{
		for (int i = 0; i < para.ChildEntities.Count; i++)
		{
			if (para.ChildEntities[i] is WIfField && (para.ChildEntities[i] as WIfField).FieldEnd == null && (para.ChildEntities[i] as WIfField).IsUpdated)
			{
				if (StartsWithExt(para.Text.Trim(), ControlChar.DoubleQuoteString) || StartsWithExt(para.Text.Trim(), ControlChar.LeftDoubleQuoteString) || StartsWithExt(para.Text.Trim(), ControlChar.RightDoubleQuoteString) || StartsWithExt(para.Text.Trim(), ControlChar.DoubleLowQuoteString))
				{
					para.ChildEntities.RemoveAt(i);
					continue;
				}
				if (text.Trim() != string.Empty)
				{
					para.ChildEntities.RemoveAt(i);
					continue;
				}
				WTextRange wTextRange = new WTextRange(para.Document);
				wTextRange.Text = ControlChar.DoubleQuoteString;
				para.ChildEntities.Insert(i, wTextRange);
				para.ChildEntities.RemoveAt(i + 1);
				isIfFieldResult = true;
			}
		}
	}

	private void CheckIfFieldEnd(ref WParagraph para, ref bool isIfFieldResult)
	{
		for (int i = 0; i < para.ChildEntities.Count; i++)
		{
			if (para.ChildEntities[i] is WFieldMark && (para.ChildEntities[i] as WFieldMark).SkipDocxItem)
			{
				if (isIfFieldResult)
				{
					WTextRange wTextRange = new WTextRange(para.Document);
					wTextRange.Text = ControlChar.DoubleQuoteString;
					para.ChildEntities.Insert(i, wTextRange);
					para.ChildEntities.RemoveAt(i + 1);
					isIfFieldResult = false;
				}
				else
				{
					para.ChildEntities.RemoveAt(i);
				}
			}
		}
	}

	protected internal override void ParseFieldCode(string fieldCode)
	{
		UpdateFieldCode(fieldCode);
	}

	protected internal override void UpdateFieldCode(string fieldCode)
	{
		char[] separator = new char[1] { '\\' };
		string[] array = fieldCode.Split(separator);
		ParseFieldFormat(array);
		m_fieldValue = array[0].Replace("IF", string.Empty);
	}

	private void CheckExpStrings()
	{
		if (m_expression1 == null)
		{
			ParseFieldValue();
		}
	}

	private void ParseFieldValue()
	{
		if (m_fieldValue != null && !(m_fieldValue == string.Empty))
		{
			Match match = new Regex("([<>=]+)").Match(m_fieldValue);
			m_operator = match.Groups[0].Value;
			int index = match.Index;
			m_expression1 = m_fieldValue.Substring(0, index).Replace("IF", string.Empty);
			index += m_operator.Length;
			string text = m_fieldValue.Substring(index, m_fieldValue.Length - index);
			MatchCollection matchCollection = new Regex("\\s+\"?([^\"]*)\"").Matches(text);
			if (matchCollection.Count == 3)
			{
				int index2 = matchCollection[0].Index;
				int index3 = matchCollection[1].Index;
				m_expression2 = text.Substring(index2, index3 - index2);
				index2 = matchCollection[1].Index;
				index3 = matchCollection[2].Index;
				m_trueText = text.Substring(index2, index3 - index2);
				index2 = index3;
				m_falseText = text.Substring(index2, text.Length - index2);
			}
		}
	}

	internal void UpdateExpString()
	{
		string text = " ";
		if (m_expField1 != null && m_expField1.Value != null)
		{
			m_expression1 = "\"" + m_expField1.Value + "\"" + text;
		}
		if (m_expField2 != null && m_expField2.Value != null)
		{
			m_expression2 = "\"" + m_expField2.Value + "\"" + text;
		}
		if (m_expression1 != null && m_expression2 != null && m_trueText != null && m_falseText != null)
		{
			m_fieldValue = m_expression1 + m_operator + text + m_expression2 + m_trueText + m_falseText;
		}
	}

	internal void UpdateMergeFields()
	{
		if (Expression1.FitMailMerge)
		{
			m_mergeFields.Add(Expression1);
		}
		if (Expression2.FitMailMerge)
		{
			m_mergeFields.Add(Expression2);
		}
	}

	protected override object CloneImpl()
	{
		WIfField obj = base.CloneImpl() as WIfField;
		obj.m_expField1 = null;
		obj.m_expField2 = null;
		obj.m_trueTextField = null;
		obj.m_falseTextField = null;
		return obj;
	}

	internal override void Close()
	{
		base.Close();
		m_expField1 = null;
		m_expField2 = null;
		if (m_trueTextField != null)
		{
			m_trueTextField.Clear();
			m_trueTextField = null;
		}
		if (m_falseTextField != null)
		{
			m_falseTextField.Clear();
			m_falseTextField = null;
		}
		if (m_mergeFields != null)
		{
			m_mergeFields.Clear();
			m_mergeFields = null;
		}
	}
}
