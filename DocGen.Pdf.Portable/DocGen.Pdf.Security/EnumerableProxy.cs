using System.Collections;

namespace DocGen.Pdf.Security;

internal class EnumerableProxy : IEnumerable
{
	private readonly IEnumerable m_value;

	internal EnumerableProxy(IEnumerable value)
	{
		m_value = value;
	}

	public IEnumerator GetEnumerator()
	{
		return m_value.GetEnumerator();
	}
}
