using System;
using System.Buffers;
using System.Runtime.InteropServices;
using System.Text;

namespace HarfBuzzSharp;

public class Buffer : NativeObject
{
	public const int DefaultReplacementCodepoint = 65533;

	public ContentType ContentType
	{
		get
		{
			return HarfBuzzApi.hb_buffer_get_content_type(Handle);
		}
		set
		{
			HarfBuzzApi.hb_buffer_set_content_type(Handle, value);
		}
	}

	public Direction Direction
	{
		get
		{
			return HarfBuzzApi.hb_buffer_get_direction(Handle);
		}
		set
		{
			HarfBuzzApi.hb_buffer_set_direction(Handle, value);
		}
	}

	public Language Language
	{
		get
		{
			return new Language(HarfBuzzApi.hb_buffer_get_language(Handle));
		}
		set
		{
			HarfBuzzApi.hb_buffer_set_language(Handle, value.Handle);
		}
	}

	public BufferFlags Flags
	{
		get
		{
			return HarfBuzzApi.hb_buffer_get_flags(Handle);
		}
		set
		{
			HarfBuzzApi.hb_buffer_set_flags(Handle, value);
		}
	}

	public ClusterLevel ClusterLevel
	{
		get
		{
			return HarfBuzzApi.hb_buffer_get_cluster_level(Handle);
		}
		set
		{
			HarfBuzzApi.hb_buffer_set_cluster_level(Handle, value);
		}
	}

	public uint ReplacementCodepoint
	{
		get
		{
			return HarfBuzzApi.hb_buffer_get_replacement_codepoint(Handle);
		}
		set
		{
			HarfBuzzApi.hb_buffer_set_replacement_codepoint(Handle, value);
		}
	}

	public uint InvisibleGlyph
	{
		get
		{
			return HarfBuzzApi.hb_buffer_get_invisible_glyph(Handle);
		}
		set
		{
			HarfBuzzApi.hb_buffer_set_invisible_glyph(Handle, value);
		}
	}

	public Script Script
	{
		get
		{
			return HarfBuzzApi.hb_buffer_get_script(Handle);
		}
		set
		{
			HarfBuzzApi.hb_buffer_set_script(Handle, value);
		}
	}

	public int Length
	{
		get
		{
			return (int)HarfBuzzApi.hb_buffer_get_length(Handle);
		}
		set
		{
			HarfBuzzApi.hb_buffer_set_length(Handle, (uint)value);
		}
	}

	public UnicodeFunctions UnicodeFunctions
	{
		get
		{
			return new UnicodeFunctions(HarfBuzzApi.hb_buffer_get_unicode_funcs(Handle));
		}
		set
		{
			HarfBuzzApi.hb_buffer_set_unicode_funcs(Handle, value.Handle);
		}
	}

	public GlyphInfo[] GlyphInfos
	{
		get
		{
			GlyphInfo[] result = GetGlyphInfoSpan().ToArray();
			GC.KeepAlive(this);
			return result;
		}
	}

	public GlyphPosition[] GlyphPositions
	{
		get
		{
			GlyphPosition[] result = GetGlyphPositionSpan().ToArray();
			GC.KeepAlive(this);
			return result;
		}
	}

	internal Buffer(IntPtr handle)
		: base(handle)
	{
	}

	public Buffer()
		: this(HarfBuzzApi.hb_buffer_create())
	{
	}

	public void Add(int codepoint, int cluster)
	{
		Add((uint)codepoint, (uint)cluster);
	}

	public void Add(uint codepoint, uint cluster)
	{
		if (Length != 0 && ContentType != ContentType.Unicode)
		{
			throw new InvalidOperationException("Non empty buffer's ContentType must be of type Unicode.");
		}
		if (ContentType == ContentType.Glyphs)
		{
			throw new InvalidOperationException("ContentType must not be of type Glyphs");
		}
		HarfBuzzApi.hb_buffer_add(Handle, codepoint, cluster);
	}

	public void AddUtf8(string utf8text)
	{
		AddUtf8(Encoding.UTF8.GetBytes(utf8text), 0, -1);
	}

	public void AddUtf8(byte[] bytes)
	{
		AddUtf8(new ReadOnlySpan<byte>(bytes));
	}

	public void AddUtf8(ReadOnlySpan<byte> text)
	{
		AddUtf8(text, 0, -1);
	}

	public unsafe void AddUtf8(ReadOnlySpan<byte> text, int itemOffset, int itemLength)
	{
		fixed (byte* ptr = text)
		{
			AddUtf8((IntPtr)ptr, text.Length, itemOffset, itemLength);
		}
	}

	public void AddUtf8(IntPtr text, int textLength)
	{
		AddUtf8(text, textLength, 0, -1);
	}

	public unsafe void AddUtf8(IntPtr text, int textLength, int itemOffset, int itemLength)
	{
		if (itemOffset < 0)
		{
			throw new ArgumentOutOfRangeException("itemOffset", "ItemOffset must be non negative.");
		}
		if (Length != 0 && ContentType != ContentType.Unicode)
		{
			throw new InvalidOperationException("Non empty buffer's ContentType must be of type Unicode.");
		}
		if (ContentType == ContentType.Glyphs)
		{
			throw new InvalidOperationException("ContentType must not be Glyphs");
		}
		HarfBuzzApi.hb_buffer_add_utf8(Handle, (void*)text, textLength, (uint)itemOffset, itemLength);
	}

	public void AddUtf16(string text)
	{
		AddUtf16(text, 0, -1);
	}

	public unsafe void AddUtf16(string text, int itemOffset, int itemLength)
	{
		fixed (char* ptr = text)
		{
			AddUtf16((IntPtr)ptr, text.Length, itemOffset, itemLength);
		}
	}

	public unsafe void AddUtf16(ReadOnlySpan<byte> text)
	{
		fixed (byte* ptr = text)
		{
			AddUtf16((IntPtr)ptr, text.Length / 2);
		}
	}

	public void AddUtf16(ReadOnlySpan<char> text)
	{
		AddUtf16(text, 0, -1);
	}

	public unsafe void AddUtf16(ReadOnlySpan<char> text, int itemOffset, int itemLength)
	{
		fixed (char* ptr = text)
		{
			AddUtf16((IntPtr)ptr, text.Length, itemOffset, itemLength);
		}
	}

	public void AddUtf16(IntPtr text, int textLength)
	{
		AddUtf16(text, textLength, 0, -1);
	}

	public unsafe void AddUtf16(IntPtr text, int textLength, int itemOffset, int itemLength)
	{
		if (itemOffset < 0)
		{
			throw new ArgumentOutOfRangeException("itemOffset", "ItemOffset must be non negative.");
		}
		if (Length != 0 && ContentType != ContentType.Unicode)
		{
			throw new InvalidOperationException("Non empty buffer's ContentType must be of type Unicode.");
		}
		if (ContentType == ContentType.Glyphs)
		{
			throw new InvalidOperationException("ContentType must not be of type Glyphs");
		}
		HarfBuzzApi.hb_buffer_add_utf16(Handle, (ushort*)(void*)text, textLength, (uint)itemOffset, itemLength);
	}

	public void AddUtf32(string text)
	{
		AddUtf32(Encoding.UTF32.GetBytes(text));
	}

	public unsafe void AddUtf32(ReadOnlySpan<byte> text)
	{
		fixed (byte* ptr = text)
		{
			AddUtf32((IntPtr)ptr, text.Length / 4);
		}
	}

	public void AddUtf32(ReadOnlySpan<uint> text)
	{
		AddUtf32(text, 0, -1);
	}

	public unsafe void AddUtf32(ReadOnlySpan<uint> text, int itemOffset, int itemLength)
	{
		fixed (uint* ptr = text)
		{
			AddUtf32((IntPtr)ptr, text.Length, itemOffset, itemLength);
		}
	}

	public void AddUtf32(ReadOnlySpan<int> text)
	{
		AddUtf32(text, 0, -1);
	}

	public unsafe void AddUtf32(ReadOnlySpan<int> text, int itemOffset, int itemLength)
	{
		fixed (int* ptr = text)
		{
			AddUtf32((IntPtr)ptr, text.Length, itemOffset, itemLength);
		}
	}

	public void AddUtf32(IntPtr text, int textLength)
	{
		AddUtf32(text, textLength, 0, -1);
	}

	public unsafe void AddUtf32(IntPtr text, int textLength, int itemOffset, int itemLength)
	{
		if (itemOffset < 0)
		{
			throw new ArgumentOutOfRangeException("itemOffset", "ItemOffset must be non negative.");
		}
		if (Length != 0 && ContentType != ContentType.Unicode)
		{
			throw new InvalidOperationException("Non empty buffer's ContentType must be of type Unicode.");
		}
		if (ContentType == ContentType.Glyphs)
		{
			throw new InvalidOperationException("ContentType must not be of type Glyphs");
		}
		HarfBuzzApi.hb_buffer_add_utf32(Handle, (uint*)(void*)text, textLength, (uint)itemOffset, itemLength);
	}

	public void AddCodepoints(ReadOnlySpan<uint> text)
	{
		AddCodepoints(text, 0, -1);
	}

	public unsafe void AddCodepoints(ReadOnlySpan<uint> text, int itemOffset, int itemLength)
	{
		fixed (uint* ptr = text)
		{
			AddCodepoints((IntPtr)ptr, text.Length, itemOffset, itemLength);
		}
	}

	public void AddCodepoints(ReadOnlySpan<int> text)
	{
		AddCodepoints(text, 0, -1);
	}

	public unsafe void AddCodepoints(ReadOnlySpan<int> text, int itemOffset, int itemLength)
	{
		fixed (int* ptr = text)
		{
			AddCodepoints((IntPtr)ptr, text.Length, itemOffset, itemLength);
		}
	}

	public void AddCodepoints(IntPtr text, int textLength)
	{
		AddCodepoints(text, textLength, 0, -1);
	}

	public unsafe void AddCodepoints(IntPtr text, int textLength, int itemOffset, int itemLength)
	{
		if (itemOffset < 0)
		{
			throw new ArgumentOutOfRangeException("itemOffset", "ItemOffset must be non negative.");
		}
		if (Length != 0 && ContentType != ContentType.Unicode)
		{
			throw new InvalidOperationException("Non empty buffer's ContentType must be of type Unicode.");
		}
		if (ContentType == ContentType.Glyphs)
		{
			throw new InvalidOperationException("ContentType must not be of type Glyphs");
		}
		HarfBuzzApi.hb_buffer_add_codepoints(Handle, (uint*)(void*)text, textLength, (uint)itemOffset, itemLength);
	}

	public unsafe ReadOnlySpan<GlyphInfo> GetGlyphInfoSpan()
	{
		uint length = default(uint);
		GlyphInfo* pointer = HarfBuzzApi.hb_buffer_get_glyph_infos(Handle, &length);
		return new ReadOnlySpan<GlyphInfo>(pointer, (int)length);
	}

	public unsafe ReadOnlySpan<GlyphPosition> GetGlyphPositionSpan()
	{
		uint length = default(uint);
		GlyphPosition* pointer = HarfBuzzApi.hb_buffer_get_glyph_positions(Handle, &length);
		return new ReadOnlySpan<GlyphPosition>(pointer, (int)length);
	}

	public void GuessSegmentProperties()
	{
		if (ContentType != ContentType.Unicode)
		{
			throw new InvalidOperationException("ContentType must be of type Unicode.");
		}
		HarfBuzzApi.hb_buffer_guess_segment_properties(Handle);
	}

	public void ClearContents()
	{
		HarfBuzzApi.hb_buffer_clear_contents(Handle);
	}

	public void Reset()
	{
		HarfBuzzApi.hb_buffer_reset(Handle);
	}

	public void Append(Buffer buffer)
	{
		Append(buffer, 0, -1);
	}

	public void Append(Buffer buffer, int start, int end)
	{
		if (buffer.Length == 0)
		{
			throw new ArgumentException("Buffer must be non empty.", "buffer");
		}
		if (buffer.ContentType != ContentType)
		{
			throw new InvalidOperationException("ContentType must be of same type.");
		}
		HarfBuzzApi.hb_buffer_append(Handle, buffer.Handle, (uint)start, (uint)((end == -1) ? buffer.Length : end));
	}

	public void NormalizeGlyphs()
	{
		if (ContentType != ContentType.Glyphs)
		{
			throw new InvalidOperationException("ContentType must be of type Glyphs.");
		}
		if (GlyphPositions.Length == 0)
		{
			throw new InvalidOperationException("GlyphPositions can't be empty.");
		}
		HarfBuzzApi.hb_buffer_normalize_glyphs(Handle);
	}

	public void Reverse()
	{
		HarfBuzzApi.hb_buffer_reverse(Handle);
	}

	public void ReverseRange(int start, int end)
	{
		HarfBuzzApi.hb_buffer_reverse_range(Handle, (uint)start, (uint)((end == -1) ? Length : end));
	}

	public void ReverseClusters()
	{
		HarfBuzzApi.hb_buffer_reverse_clusters(Handle);
	}

	public string SerializeGlyphs()
	{
		return SerializeGlyphs(0, -1, null, SerializeFormat.Text, SerializeFlag.Default);
	}

	public string SerializeGlyphs(int start, int end)
	{
		return SerializeGlyphs(start, end, null, SerializeFormat.Text, SerializeFlag.Default);
	}

	public string SerializeGlyphs(Font font)
	{
		return SerializeGlyphs(0, -1, font, SerializeFormat.Text, SerializeFlag.Default);
	}

	public string SerializeGlyphs(Font font, SerializeFormat format, SerializeFlag flags)
	{
		return SerializeGlyphs(0, -1, font, format, flags);
	}

	public unsafe string SerializeGlyphs(int start, int end, Font font, SerializeFormat format, SerializeFlag flags)
	{
		if (Length == 0)
		{
			throw new InvalidOperationException("Buffer should not be empty.");
		}
		if (ContentType != ContentType.Glyphs)
		{
			throw new InvalidOperationException("ContentType should be of type Glyphs.");
		}
		if (end == -1)
		{
			end = Length;
		}
		using IMemoryOwner<byte> memoryOwner = MemoryPool<byte>.Shared.Rent();
		using MemoryHandle memoryHandle = memoryOwner.Memory.Pin();
		int length = memoryOwner.Memory.Length;
		uint num = (uint)start;
		StringBuilder stringBuilder = new StringBuilder(length);
		uint len = default(uint);
		while (num < end)
		{
			num += HarfBuzzApi.hb_buffer_serialize_glyphs(Handle, num, (uint)end, memoryHandle.Pointer, (uint)length, &len, font?.Handle ?? IntPtr.Zero, format, flags);
			stringBuilder.Append(Marshal.PtrToStringAnsi((IntPtr)memoryHandle.Pointer, (int)len));
		}
		return stringBuilder.ToString();
	}

	public void DeserializeGlyphs(string data)
	{
		DeserializeGlyphs(data, null, SerializeFormat.Text);
	}

	public void DeserializeGlyphs(string data, Font font)
	{
		DeserializeGlyphs(data, font, SerializeFormat.Text);
	}

	public unsafe void DeserializeGlyphs(string data, Font font, SerializeFormat format)
	{
		if (Length != 0)
		{
			throw new InvalidOperationException("Buffer must be empty.");
		}
		if (ContentType == ContentType.Glyphs)
		{
			throw new InvalidOperationException("ContentType must not be Glyphs.");
		}
		HarfBuzzApi.hb_buffer_deserialize_glyphs(Handle, data, -1, null, font?.Handle ?? IntPtr.Zero, format);
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}

	protected override void DisposeHandler()
	{
		if (Handle != IntPtr.Zero)
		{
			HarfBuzzApi.hb_buffer_destroy(Handle);
		}
	}
}
