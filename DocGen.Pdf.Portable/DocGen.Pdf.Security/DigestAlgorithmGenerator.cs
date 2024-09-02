namespace DocGen.Pdf.Security;

internal class DigestAlgorithmGenerator : IAlgorithmGenerator
{
	private const long m_count = 10L;

	private long m_stateCount;

	private long m_seedCount;

	private IMessageDigest m_digest;

	private byte[] m_state;

	private byte[] m_bytes;

	internal DigestAlgorithmGenerator(IMessageDigest digest)
	{
		m_digest = digest;
		m_bytes = new byte[digest.MessageDigestSize];
		m_seedCount = 1L;
		m_state = new byte[digest.MessageDigestSize];
		m_stateCount = 1L;
	}

	public void AddMaterial(byte[] bytes)
	{
		lock (this)
		{
			DigestUpdate(bytes);
			DigestUpdate(m_bytes);
			DigestDoFinal(m_bytes);
		}
	}

	public void AddMaterial(long value)
	{
		lock (this)
		{
			AddToCounter(value);
			DigestUpdate(m_bytes);
			DigestDoFinal(m_bytes);
		}
	}

	public void FillNextBytes(byte[] bytes)
	{
		FillNextBytes(bytes, 0, bytes.Length);
	}

	public void FillNextBytes(byte[] bytes, int index, int length)
	{
		lock (this)
		{
			int num = 0;
			GenerateState();
			int num2 = index + length;
			for (int i = index; i < num2; i++)
			{
				if (num == m_state.Length)
				{
					GenerateState();
					num = 0;
				}
				bytes[i] = m_state[num++];
			}
		}
	}

	private void GenerateState()
	{
		AddToCounter(m_stateCount++);
		DigestUpdate(m_state);
		DigestUpdate(m_bytes);
		DigestDoFinal(m_state);
		if (m_stateCount % 10 == 0L)
		{
			DigestUpdate(m_bytes);
			AddToCounter(m_seedCount++);
			DigestDoFinal(m_bytes);
		}
	}

	private void AddToCounter(long value)
	{
		ulong num = (ulong)value;
		for (int i = 0; i != 8; i++)
		{
			m_digest.Update((byte)num);
			num >>= 8;
		}
	}

	private void DigestUpdate(byte[] bytes)
	{
		m_digest.Update(bytes, 0, bytes.Length);
	}

	private void DigestDoFinal(byte[] bytes)
	{
		m_digest.DoFinal(bytes, 0);
	}
}
