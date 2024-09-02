using System;
using System.ComponentModel;
using System.Text;

namespace SkiaSharp;

public static class StringUtilities
{
	internal const string NullTerminator = "\0";

	private static int GetUnicodeStringLength(SKTextEncoding encoding)
	{
		return encoding switch
		{
			SKTextEncoding.Utf8 => 1, 
			SKTextEncoding.Utf16 => 1, 
			SKTextEncoding.Utf32 => 2, 
			_ => throw new ArgumentOutOfRangeException("encoding", $"Encoding {encoding} is not supported."), 
		};
	}

	internal static int GetCharacterByteSize(this SKTextEncoding encoding)
	{
		return encoding switch
		{
			SKTextEncoding.Utf8 => 1, 
			SKTextEncoding.Utf16 => 2, 
			SKTextEncoding.Utf32 => 4, 
			_ => throw new ArgumentOutOfRangeException("encoding", $"Encoding {encoding} is not supported."), 
		};
	}

	public static int GetUnicodeCharacterCode(string character, SKTextEncoding encoding)
	{
		if (character == null)
		{
			throw new ArgumentNullException("character");
		}
		if (GetUnicodeStringLength(encoding) != character.Length)
		{
			throw new ArgumentException("character", "Only a single character can be specified.");
		}
		byte[] encodedText = GetEncodedText(character, encoding);
		return BitConverter.ToInt32(encodedText, 0);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use GetEncodedText(string, SKTextEncoding) instead.")]
	public static byte[] GetEncodedText(string text, SKEncoding encoding)
	{
		return GetEncodedText(text.AsSpan(), encoding.ToTextEncoding());
	}

	public static byte[] GetEncodedText(string text, SKTextEncoding encoding)
	{
		return GetEncodedText(text.AsSpan(), encoding);
	}

	internal static byte[] GetEncodedText(string text, SKTextEncoding encoding, bool addNull)
	{
		if (!string.IsNullOrEmpty(text) && addNull)
		{
			text += "\0";
		}
		return GetEncodedText(text.AsSpan(), encoding);
	}

	public static byte[] GetEncodedText(ReadOnlySpan<char> text, SKTextEncoding encoding)
	{
		return encoding switch
		{
			SKTextEncoding.Utf8 => Encoding.UTF8.GetBytes(text), 
			SKTextEncoding.Utf16 => Encoding.Unicode.GetBytes(text), 
			SKTextEncoding.Utf32 => Encoding.UTF32.GetBytes(text), 
			_ => throw new ArgumentOutOfRangeException("encoding", $"Encoding {encoding} is not supported."), 
		};
	}

	public static string GetString(IntPtr data, int dataLength, SKTextEncoding encoding)
	{
		return GetString(data.AsReadOnlySpan(dataLength), 0, dataLength, encoding);
	}

	public static string GetString(byte[] data, SKTextEncoding encoding)
	{
		return GetString(data, 0, data.Length, encoding);
	}

	public static string GetString(byte[] data, int index, int count, SKTextEncoding encoding)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		return encoding switch
		{
			SKTextEncoding.Utf8 => Encoding.UTF8.GetString(data, index, count), 
			SKTextEncoding.Utf16 => Encoding.Unicode.GetString(data, index, count), 
			SKTextEncoding.Utf32 => Encoding.UTF32.GetString(data, index, count), 
			_ => throw new ArgumentOutOfRangeException("encoding", $"Encoding {encoding} is not supported."), 
		};
	}

	public static string GetString(ReadOnlySpan<byte> data, SKTextEncoding encoding)
	{
		return GetString(data, 0, data.Length, encoding);
	}

	public unsafe static string GetString(ReadOnlySpan<byte> data, int index, int count, SKTextEncoding encoding)
	{
		data = data.Slice(index, count);
		if (data.Length == 0)
		{
			return string.Empty;
		}
		fixed (byte* bytes = data)
		{
			return encoding switch
			{
				SKTextEncoding.Utf8 => Encoding.UTF8.GetString(bytes, data.Length), 
				SKTextEncoding.Utf16 => Encoding.Unicode.GetString(bytes, data.Length), 
				SKTextEncoding.Utf32 => Encoding.UTF32.GetString(bytes, data.Length), 
				_ => throw new ArgumentOutOfRangeException("encoding", $"Encoding {encoding} is not supported."), 
			};
		}
	}
}
