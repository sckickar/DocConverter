using System;
using System.Collections;
using System.Collections.Generic;

namespace DocGen.Pdf.Security;

internal sealed class CipherUtils
{
	private enum Algorithm
	{
		DES,
		DESEDE,
		RC2,
		RSA
	}

	private enum Mode
	{
		ECB,
		NONE,
		CBC,
		CTS
	}

	private enum PaddingType
	{
		NOPADDING,
		RAW,
		PKCS1,
		PKCS1PADDING,
		PKCS5,
		PKCS5PADDING,
		PKCS7,
		PKCS7PADDING,
		WITHCipherTextStealing,
		X923PADDING
	}

	private static readonly Dictionary<string, string> m_algorithms;

	private static readonly Dictionary<string, DerObjectID> m_ids;

	internal static ICollection Algorithms => m_ids.Keys;

	static CipherUtils()
	{
		m_algorithms = new Dictionary<string, string>();
		m_ids = new Dictionary<string, DerObjectID>();
		((Algorithm)(object)Enums.GetArbitraryValue(typeof(Algorithm))).ToString();
		((Mode)(object)Enums.GetArbitraryValue(typeof(Mode))).ToString();
		((PaddingType)(object)Enums.GetArbitraryValue(typeof(PaddingType))).ToString();
	}

	internal static IBufferedCipher GetCipher(string algorithm)
	{
		if (algorithm == null)
		{
			throw new ArgumentNullException("algorithm");
		}
		algorithm = algorithm.ToUpperInvariant();
		string text = null;
		if (m_algorithms.Count > 0)
		{
			text = m_algorithms[algorithm];
		}
		if (text != null)
		{
			algorithm = text;
		}
		string[] array = algorithm.Split('/');
		ICipher cipher = null;
		ICipherBlock cipher2 = null;
		string text2 = array[0];
		if (m_algorithms.Count > 0)
		{
			text = m_algorithms[text2];
		}
		if (text != null)
		{
			text2 = text;
		}
		Algorithm algorithm2;
		try
		{
			algorithm2 = (Algorithm)(object)Enums.GetEnumValue(typeof(Algorithm), text2);
		}
		catch (ArgumentException)
		{
			throw new Exception("Invalid cipher " + algorithm);
		}
		switch (algorithm2)
		{
		case Algorithm.DES:
			cipher = new DataEncryption();
			break;
		case Algorithm.DESEDE:
			cipher = new DesEdeAlogorithm();
			break;
		case Algorithm.RC2:
			cipher = new RC2Algorithm();
			break;
		case Algorithm.RSA:
			cipher2 = new RSAAlgorithm();
			break;
		default:
			throw new Exception("Invalid cipher " + algorithm);
		}
		bool flag = true;
		IPadding padding = null;
		if (array.Length > 2)
		{
			string text3 = array[2];
			PaddingType paddingType;
			if (text3 == "")
			{
				paddingType = PaddingType.RAW;
			}
			else if (text3 == "X9.23PADDING")
			{
				paddingType = PaddingType.X923PADDING;
			}
			else
			{
				try
				{
					paddingType = (PaddingType)(object)Enums.GetEnumValue(typeof(PaddingType), text3);
				}
				catch (ArgumentException)
				{
					throw new Exception("Invalid cipher " + algorithm);
				}
			}
			switch (paddingType)
			{
			case PaddingType.NOPADDING:
				flag = false;
				break;
			case PaddingType.PKCS1:
			case PaddingType.PKCS1PADDING:
				cipher2 = new Pkcs1Encoding(cipher2);
				break;
			case PaddingType.PKCS5:
			case PaddingType.PKCS5PADDING:
			case PaddingType.PKCS7:
			case PaddingType.PKCS7PADDING:
				padding = new Pkcs7Padding();
				break;
			default:
				throw new Exception("Invalid cipher " + algorithm);
			case PaddingType.RAW:
			case PaddingType.WITHCipherTextStealing:
				break;
			}
		}
		string text4 = "";
		if (array.Length > 1)
		{
			text4 = array[1];
			int num = -1;
			for (int i = 0; i < text4.Length; i++)
			{
				if (char.IsDigit(text4[i]))
				{
					num = i;
					break;
				}
			}
			string text5 = ((num >= 0) ? text4.Substring(0, num) : text4);
			try
			{
				switch ((text5 == "") ? Mode.NONE : ((Mode)(object)Enums.GetEnumValue(typeof(Mode), text5)))
				{
				case Mode.CBC:
					cipher = new CipherBlockChainingMode(cipher);
					break;
				case Mode.CTS:
					cipher = new CipherBlockChainingMode(cipher);
					break;
				default:
					throw new Exception("Invalid cipher " + algorithm);
				case Mode.ECB:
				case Mode.NONE:
					break;
				}
			}
			catch (ArgumentException)
			{
				throw new Exception("Invalid cipher " + algorithm);
			}
		}
		if (cipher != null)
		{
			if (padding != null)
			{
				return new BufferedBlockPadding(cipher, padding);
			}
			if (!flag || cipher.IsBlock)
			{
				return new BufferedCipher(cipher);
			}
			return new BufferedBlockPadding(cipher);
		}
		throw new Exception("Invalid cipher " + algorithm);
	}
}
