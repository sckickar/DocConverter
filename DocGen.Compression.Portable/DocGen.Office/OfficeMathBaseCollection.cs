namespace DocGen.Office;

internal class OfficeMathBaseCollection : CollectionImpl, IOfficeMathBaseCollection, ICollectionBase, IOfficeMathEntity
{
	public IOfficeMathFunctionBase this[int index] => (OfficeMathFunctionBase)base.InnerList[index];

	internal OfficeMathBaseCollection(OfficeMath owner)
		: base(owner)
	{
	}

	internal OfficeMathFunctionBase CreateFunction(MathFunctionType Type)
	{
		return Type switch
		{
			MathFunctionType.Accent => new OfficeMathAccent(base.OwnerMathEntity), 
			MathFunctionType.Bar => new OfficeMathBar(base.OwnerMathEntity), 
			MathFunctionType.BorderBox => new OfficeMathBorderBox(base.OwnerMathEntity), 
			MathFunctionType.Box => new OfficeMathBox(base.OwnerMathEntity), 
			MathFunctionType.Delimiter => new OfficeMathDelimiter(base.OwnerMathEntity), 
			MathFunctionType.EquationArray => new OfficeMathEquationArray(base.OwnerMathEntity), 
			MathFunctionType.Fraction => new OfficeMathFraction(base.OwnerMathEntity), 
			MathFunctionType.Function => new OfficeMathFunction(base.OwnerMathEntity), 
			MathFunctionType.GroupCharacter => new OfficeMathGroupCharacter(base.OwnerMathEntity), 
			MathFunctionType.Limit => new OfficeMathLimit(base.OwnerMathEntity), 
			MathFunctionType.Matrix => new OfficeMathMatrix(base.OwnerMathEntity), 
			MathFunctionType.NArray => new OfficeMathNArray(base.OwnerMathEntity), 
			MathFunctionType.Phantom => new OfficeMathPhantom(base.OwnerMathEntity), 
			MathFunctionType.Radical => new OfficeMathRadical(base.OwnerMathEntity), 
			MathFunctionType.LeftSubSuperscript => new OfficeMathLeftScript(base.OwnerMathEntity), 
			MathFunctionType.SubSuperscript => new OfficeMathScript(base.OwnerMathEntity), 
			MathFunctionType.RightSubSuperscript => new OfficeMathRightScript(base.OwnerMathEntity), 
			MathFunctionType.RunElement => new OfficeMathRunElement(base.OwnerMathEntity), 
			_ => null, 
		};
	}

	public IOfficeMathFunctionBase Add(int index, MathFunctionType mathFunctionType)
	{
		OfficeMathFunctionBase officeMathFunctionBase = CreateFunction(mathFunctionType);
		m_innerList.Insert(index, officeMathFunctionBase);
		return officeMathFunctionBase;
	}

	public IOfficeMathFunctionBase Add(MathFunctionType mathFunctionType)
	{
		OfficeMathFunctionBase officeMathFunctionBase = CreateFunction(mathFunctionType);
		m_innerList.Add(officeMathFunctionBase);
		return officeMathFunctionBase;
	}

	internal void CloneItemsTo(OfficeMathBaseCollection items)
	{
		CloneItemsTo(items, 0, base.Count - 1);
	}

	internal void CloneItemsTo(OfficeMathBaseCollection items, int startIndex, int endIndex)
	{
		for (int i = startIndex; i <= endIndex; i++)
		{
			OfficeMathFunctionBase item = (this[i] as OfficeMathFunctionBase).Clone(items.OwnerMathEntity);
			items.Add(item);
		}
	}

	internal override void Close()
	{
		if (m_innerList != null)
		{
			for (int i = 0; i < m_innerList.Count; i++)
			{
				(m_innerList[i] as OfficeMathFunctionBase).Close();
			}
		}
		base.Close();
	}
}
