using System.Collections.Generic;

namespace DocGen.PdfViewer.Base;

internal class EncryptionCollection
{
	private readonly List<EncryptionStdHelper> encryptionCollection;

	public bool HasEncryption => encryptionCollection.Count != 0;

	public EncryptionCollection()
	{
		encryptionCollection = new List<EncryptionStdHelper>();
	}

	public void PushEncryption(EncryptionStdHelper encryption)
	{
		encryptionCollection.Add(encryption);
	}

	public void PopEncryption()
	{
		encryptionCollection.Remove(Countable.Last(encryptionCollection));
	}

	public byte Decrypt(byte b)
	{
		byte b2 = encryptionCollection[0].Decrypt(b);
		for (int i = 1; i < encryptionCollection.Count; i++)
		{
			b2 = encryptionCollection[i].Decrypt(b2);
		}
		return b2;
	}
}
