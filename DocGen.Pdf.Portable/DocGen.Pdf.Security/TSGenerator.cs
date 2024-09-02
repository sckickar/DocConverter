namespace DocGen.Pdf.Security;

internal class TSGenerator
{
	private class SGenerator
	{
		private int m_count;

		private bool m_stop;

		private void Run(object ignored)
		{
			while (!m_stop)
			{
				m_count++;
			}
		}
	}
}
