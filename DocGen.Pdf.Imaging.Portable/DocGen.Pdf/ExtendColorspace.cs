using DocGen.Pdf.Primitives;

namespace DocGen.Pdf;

internal class ExtendColorspace
{
	private IPdfPrimitive m_colorValueArray;

	public IPdfPrimitive ColorSpaceValueArray
	{
		get
		{
			return m_colorValueArray;
		}
		set
		{
			m_colorValueArray = value;
		}
	}

	public ExtendColorspace(IPdfPrimitive refHolderColorspace)
	{
		m_colorValueArray = refHolderColorspace;
	}
}
