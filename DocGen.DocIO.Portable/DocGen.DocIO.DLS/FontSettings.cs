using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DocGen.DocIO.ReaderWriter.Biff_Records;
using DocGen.Drawing;
using DocGen.Office;

namespace DocGen.DocIO.DLS;

public class FontSettings
{
	internal Dictionary<string, Stream> FontStreams = new Dictionary<string, Stream>(StringComparer.CurrentCultureIgnoreCase);

	private FallbackFonts _fallbackFonts;

	public FallbackFonts FallbackFonts => _fallbackFonts ?? (_fallbackFonts = new FallbackFonts());

	public event SubstituteFontEventHandler SubstituteFont;

	internal DocGen.Drawing.Font GetFont(string fontName, float fontSize, FontStyle fontStyle, FontScriptType scriptType)
	{
		DocGen.Drawing.Font font = null;
		bool hasStylesAndWeights = true;
		string text = ((WordDocument.RenderHelper != null) ? WordDocument.RenderHelper.GetFontName(fontName, fontSize, fontStyle, scriptType, ref hasStylesAndWeights) : fontName);
		font = new DocGen.Drawing.Font((fontName.ToLower().Contains(text.ToLower()) && text != string.Empty) ? fontName : text, fontSize, fontStyle);
		if (this.SubstituteFont != null && (font.Name.ToLower() != fontName.ToLower() || font.Style != fontStyle || !hasStylesAndWeights))
		{
			SubstituteFontEventArgs substituteFontEventArgs = new SubstituteFontEventArgs(fontName, "Microsoft Sans Serif", fontStyle);
			while (this.SubstituteFont != null)
			{
				string fontStyleValue = GetFontStyleValue(fontStyle);
				string alternateFontName = substituteFontEventArgs.AlternateFontName;
				if (!FontStreams.ContainsKey(fontName.ToLower() + "_" + fontStyleValue.ToLower()))
				{
					this.SubstituteFont(this, substituteFontEventArgs);
				}
				if (substituteFontEventArgs.AlternateFontStream != null && substituteFontEventArgs.AlternateFontStream.Length > 0)
				{
					Stream alternateFontStream = substituteFontEventArgs.AlternateFontStream;
					if (!FontStreams.ContainsKey(fontName.ToLower() + "_" + fontStyleValue.ToLower()))
					{
						FontStreams.Add(substituteFontEventArgs.OriginalFontName.ToLower() + "_" + fontStyleValue.ToLower(), alternateFontStream);
					}
					alternateFontStream.Position = 0L;
					return GetFontFromStream(alternateFontStream, fontName, fontSize, fontStyle);
				}
				if (substituteFontEventArgs.AlternateFontStream == null && FontStreams.ContainsKey(fontName.ToLower() + "_" + fontStyleValue.ToLower()))
				{
					return font;
				}
				text = WordDocument.RenderHelper.GetFontName(substituteFontEventArgs.AlternateFontName, fontSize, fontStyle, scriptType);
				font = new DocGen.Drawing.Font(text, fontSize, fontStyle);
				if (font.Name == substituteFontEventArgs.AlternateFontName || alternateFontName != "Microsoft Sans Serif" || alternateFontName == substituteFontEventArgs.AlternateFontName)
				{
					break;
				}
			}
		}
		return font;
	}

	private string GetFontStyleValue(FontStyle fontStyle)
	{
		string text = string.Empty;
		if ((fontStyle & FontStyle.Bold) == FontStyle.Bold)
		{
			text = "Bold";
		}
		if (!string.IsNullOrEmpty(text) && (fontStyle & FontStyle.Italic) == FontStyle.Italic)
		{
			text = "BoldItalic";
		}
		else if ((fontStyle & FontStyle.Italic) == FontStyle.Italic)
		{
			text = "Italic";
		}
		else if (string.IsNullOrEmpty(text))
		{
			text = "Regular";
		}
		return text;
	}

	internal void EmbedDocumentFonts(WordDocument document)
	{
		if (document.FFNStringTable == null)
		{
			return;
		}
		FontFamilyNameRecord[] fontFamilyNameRecords = document.FFNStringTable.FontFamilyNameRecords;
		foreach (FontFamilyNameRecord fontFamilyNameRecord in fontFamilyNameRecords)
		{
			foreach (KeyValuePair<string, Dictionary<string, DictionaryEntry>> embedFont in fontFamilyNameRecord.EmbedFonts)
			{
				foreach (KeyValuePair<string, DictionaryEntry> item in embedFont.Value)
				{
					DictionaryEntry value = item.Value;
					string guidString = value.Key.ToString();
					guidString = ParseGuidString(guidString);
					if (guidString != null)
					{
						MemoryStream memoryStream = (MemoryStream)value.Value;
						MemoryStream memoryStream2 = new MemoryStream();
						DeObfuscateFont(memoryStream, memoryStream2, guidString);
						byte[] buffer = new byte[memoryStream2.Length];
						memoryStream2.Position = 0L;
						memoryStream2.Read(buffer, 0, (int)memoryStream2.Length);
						memoryStream2.Position = 0L;
						string text = fontFamilyNameRecord.FontName.ToLower();
						string text2 = embedFont.Key.Replace("embed", "");
						string key = text + "_" + text2;
						if (!document.FontSettings.FontStreams.ContainsKey(key) && WordDocument.RenderHelper != null && WordDocument.RenderHelper.IsValidFontStream(memoryStream2))
						{
							memoryStream2.Position = 0L;
							MemoryStream memoryStream3 = new MemoryStream(memoryStream2.ToArray());
							document.FontSettings.FontStreams.Add(key, memoryStream3);
							GetFontFromStream(memoryStream3, text, 11f, GetFontStyle(text2));
						}
						memoryStream.Position = 0L;
						memoryStream2.Dispose();
					}
				}
			}
		}
	}

	private FontStyle GetFontStyle(string style)
	{
		FontStyle fontStyle = FontStyle.Regular;
		return style.ToLower() switch
		{
			"bold" => FontStyle.Bold, 
			"italic" => FontStyle.Italic, 
			"bolditalic" => (FontStyle)3, 
			_ => FontStyle.Regular, 
		};
	}

	private void DeObfuscateFont(Stream font, MemoryStream outStream, string fontGuid)
	{
		byte[] array = new byte[16];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = Convert.ToByte(fontGuid.Substring(i * 2, 2), 16);
		}
		byte[] array2 = new byte[32];
		font.Position = 0L;
		font.Read(array2, 0, 32);
		for (int j = 0; j < 32; j++)
		{
			int num = array.Length - j % array.Length - 1;
			array2[j] ^= array[num];
		}
		outStream.Write(array2, 0, 32);
		array2 = new byte[4096];
		int num2 = 0;
		while ((num2 = font.Read(array2, 0, 4096)) > 0)
		{
			outStream.Write(array2, 0, num2);
		}
		outStream.Position = 0L;
	}

	private DocGen.Drawing.Font GetFontFromStream(Stream fontStream, string fontName, float fontSize, FontStyle fontStyle)
	{
		WordDocument.RenderHelper.CreateFont(fontStream, fontName, fontSize, fontStyle);
		return new DocGen.Drawing.Font(fontName, fontSize, fontStyle);
	}

	private string ParseGuidString(string guidString)
	{
		try
		{
			guidString = new Guid(guidString).ToString("N");
			return guidString;
		}
		catch (Exception)
		{
			return null;
		}
	}

	internal void Close()
	{
		if (FontStreams != null)
		{
			foreach (string key in FontStreams.Keys)
			{
				Stream stream = FontStreams[key];
				if (stream != null)
				{
					stream.Dispose();
					stream = null;
				}
			}
			FontStreams.Clear();
			FontStreams = null;
		}
		if (this.SubstituteFont != null)
		{
			this.SubstituteFont = null;
		}
		if (_fallbackFonts != null)
		{
			_fallbackFonts.Clear();
			_fallbackFonts = null;
		}
	}
}
