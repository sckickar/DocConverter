using DocGen.DocIO.DLS;

namespace DocGen.Layouting;

internal class EquationField
{
	private WField m_eqFieldEntity;

	private LayoutedEQFields m_layouttedEQField;

	internal WField EQFieldEntity
	{
		get
		{
			return m_eqFieldEntity;
		}
		set
		{
			m_eqFieldEntity = value;
		}
	}

	internal LayoutedEQFields LayouttedEQField
	{
		get
		{
			return m_layouttedEQField;
		}
		set
		{
			m_layouttedEQField = value;
		}
	}
}
