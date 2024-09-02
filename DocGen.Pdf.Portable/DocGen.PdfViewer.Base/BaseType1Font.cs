using System;
using System.Collections.Generic;
using System.Threading;

namespace DocGen.PdfViewer.Base;

internal class BaseType1Font : PostScriptObj, IBuildCharacterOwner
{
	private readonly CharacterBuilder buildChar;

	private readonly Dictionary<string, GlyphInfo> glyphOutlines;

	private readonly KeyProperty<PostScriptArray> fontMatrix;

	private readonly KeyProperty<FontData> fontInfo;

	private readonly KeyProperty<PostScriptArray> encoding;

	private readonly KeyProperty<PostScriptDict> charStrings;

	private readonly KeyProperty<KeyPrivate> priv;

	private object SyncObj = new object();

	public PostScriptArray FontMatrix => fontMatrix.GetValue();

	public FontData FontInfo => fontInfo.GetValue();

	public PostScriptArray Encoding => encoding.GetValue();

	public PostScriptDict CharStrings => charStrings.GetValue();

	public KeyPrivate Private => priv.GetValue();

	public BaseType1Font()
	{
		buildChar = new CharacterBuilder(this);
		glyphOutlines = new Dictionary<string, GlyphInfo>();
		fontMatrix = CreateProperty(new KeyPropertyDescriptor
		{
			Name = "FontMatrix"
		}, PostScriptArray.MatrixIdentity);
		fontInfo = CreateProperty(new KeyPropertyDescriptor
		{
			Name = "FontInfo"
		}, new FontData());
		encoding = CreateProperty(new KeyPropertyDescriptor
		{
			Name = "Encoding"
		}, Type1Converters.EncodingConverter, PresettedEncoding.StandardEncoding.ToArray());
		charStrings = CreateProperty<PostScriptDict>(new KeyPropertyDescriptor
		{
			Name = "CharStrings"
		});
		priv = CreateProperty<KeyPrivate>(new KeyPropertyDescriptor
		{
			Name = "Private"
		}, Type1Converters.PostScriptObjectConverter);
	}

	public ushort GetAdvancedWidth(Glyph glyph)
	{
		return GetGlyphData(glyph.Name).AdvancedWidth;
	}

	public void GetGlyphOutlines(Glyph glyph, double fontSize)
	{
		Monitor.Enter(SyncObj);
		try
		{
			GlyphOutlinesCollection glyphOutlinesCollection = GetGlyphData(glyph.Name).Oultlines.Clone();
			Matrix transformMatrix = FontMatrix.ToMatrix();
			transformMatrix.ScaleAppend(fontSize, 0.0 - fontSize, 0.0, 0.0);
			glyphOutlinesCollection.Transform(transformMatrix);
			glyph.Outlines = glyphOutlinesCollection;
		}
		finally
		{
			Monitor.Exit(SyncObj);
		}
	}

	public GlyphInfo GetGlyphData(string name)
	{
		if (!glyphOutlines.TryGetValue(name, out GlyphInfo value))
		{
			value = ReadGlyphData(name);
			glyphOutlines[name] = value;
		}
		return value;
	}

	public byte[] GetSubr(int index)
	{
		return Private.GetSubr(index);
	}

	public byte[] GetGlobalSubr(int index)
	{
		throw new NotImplementedException();
	}

	internal string GetGlyphName(ushort cid)
	{
		if (Encoding == null)
		{
			return ".notdef";
		}
		return Encoding.GetElementAs<string>(cid);
	}

	private GlyphInfo ReadGlyphData(string name)
	{
		PostScriptStrHelper elementAs = CharStrings.GetElementAs<PostScriptStrHelper>(name);
		if (elementAs != null)
		{
			buildChar.Execute(elementAs.ToByteArray());
			return new GlyphInfo(buildChar.GlyphOutlines, (ushort?)buildChar.Width);
		}
		return new GlyphInfo(new GlyphOutlinesCollection(), 0);
	}
}
