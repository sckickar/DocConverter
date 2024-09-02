using System;
using System.Collections.Generic;

namespace DocGen.Pdf.Security;

internal class ParamUtility
{
	private readonly Dictionary<string, string> m_algorithms = new Dictionary<string, string>();

	internal ParamUtility()
	{
		AddAlgorithm("DESEDE", "DESEDEWRAP", "TDEA", new DerObjectID("1.3.14.3.2.17"), PKCSOIDs.IdAlgCms3DesWrap);
		AddAlgorithm("DESEDE3", PKCSOIDs.DesEde3Cbc);
		AddAlgorithm("RC2", PKCSOIDs.RC2Cbc, PKCSOIDs.IdAlgCmsRC2Wrap);
	}

	private void AddAlgorithm(string name, params object[] objects)
	{
		m_algorithms[name] = name;
		foreach (object obj in objects)
		{
			m_algorithms[obj.ToString()] = name;
		}
	}

	internal KeyParameter CreateKeyParameter(string algorithm, byte[] bytes, int offset, int length)
	{
		string text = m_algorithms[algorithm.ToUpperInvariant()];
		if (text == null)
		{
			throw new Exception("Invalid entry. Algorithm " + algorithm + " not recognised.");
		}
		switch (text)
		{
		case "DES":
			return new DataEncryptionParameter(bytes, offset, length);
		case "DESEDE":
		case "DESEDE3":
			return new DESedeAlgorithmParameter(bytes, offset, length);
		default:
			return new KeyParameter(bytes, offset, length);
		}
	}
}
