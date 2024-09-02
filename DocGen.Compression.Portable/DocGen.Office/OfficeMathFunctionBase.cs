namespace DocGen.Office;

internal abstract class OfficeMathFunctionBase : OwnerHolder, IOfficeMathFunctionBase, IOfficeMathEntity
{
	internal MathFunctionType m_type;

	public MathFunctionType Type => m_type;

	internal OfficeMathFunctionBase(IOfficeMathEntity owner)
		: base(owner)
	{
		m_type = (MathFunctionType)0;
	}

	internal override void Close()
	{
		base.Close();
	}

	internal abstract OfficeMathFunctionBase Clone(IOfficeMathEntity owner);

	internal IOfficeRunFormat GetDefaultControlProperties()
	{
		return GetBaseMathParagraph(this).DefaultMathCharacterFormat.Clone();
	}

	private OfficeMathParagraph GetBaseMathParagraph(OfficeMathFunctionBase mathFunction)
	{
		IOfficeMathEntity ownerMathEntity = mathFunction.OwnerMathEntity;
		while (!(ownerMathEntity is IOfficeMathParagraph) && ownerMathEntity != null)
		{
			ownerMathEntity = ownerMathEntity.OwnerMathEntity;
		}
		return ownerMathEntity as OfficeMathParagraph;
	}
}
