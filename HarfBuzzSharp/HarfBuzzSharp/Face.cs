using System;

namespace HarfBuzzSharp;

public class Face : NativeObject
{
	private class StaticFace : Face
	{
		public StaticFace(IntPtr handle)
			: base(handle)
		{
		}

		protected override void Dispose(bool disposing)
		{
		}
	}

	private static readonly Lazy<Face> emptyFace = new Lazy<Face>(() => new StaticFace(HarfBuzzApi.hb_face_get_empty()));

	public static Face Empty => emptyFace.Value;

	public int Index
	{
		get
		{
			return (int)HarfBuzzApi.hb_face_get_index(Handle);
		}
		set
		{
			HarfBuzzApi.hb_face_set_index(Handle, (uint)value);
		}
	}

	public int UnitsPerEm
	{
		get
		{
			return (int)HarfBuzzApi.hb_face_get_upem(Handle);
		}
		set
		{
			HarfBuzzApi.hb_face_set_upem(Handle, (uint)value);
		}
	}

	public int GlyphCount
	{
		get
		{
			return (int)HarfBuzzApi.hb_face_get_glyph_count(Handle);
		}
		set
		{
			HarfBuzzApi.hb_face_set_glyph_count(Handle, (uint)value);
		}
	}

	public unsafe Tag[] Tables
	{
		get
		{
			uint num = default(uint);
			uint num2 = HarfBuzzApi.hb_face_get_table_tags(Handle, 0u, &num, null);
			Tag[] array = new Tag[num2];
			fixed (Tag* ptr = array)
			{
				void* table_tags = ptr;
				HarfBuzzApi.hb_face_get_table_tags(Handle, 0u, &num2, (uint*)table_tags);
			}
			return array;
		}
	}

	public bool IsImmutable => HarfBuzzApi.hb_face_is_immutable(Handle);

	public Face(Blob blob, uint index)
		: this(blob, (int)index)
	{
	}

	public Face(Blob blob, int index)
		: this(IntPtr.Zero)
	{
		if (blob == null)
		{
			throw new ArgumentNullException("blob");
		}
		if (index < 0)
		{
			throw new ArgumentOutOfRangeException("index", "Index must be non negative.");
		}
		Handle = HarfBuzzApi.hb_face_create(blob.Handle, (uint)index);
	}

	public Face(GetTableDelegate getTable)
		: this(getTable, null)
	{
	}

	public unsafe Face(GetTableDelegate getTable, ReleaseDelegate destroy)
		: this(IntPtr.Zero)
	{
		if (getTable == null)
		{
			throw new ArgumentNullException("getTable");
		}
		Handle = HarfBuzzApi.hb_face_create_for_tables(DelegateProxies.GetTableDelegateProxy, (void*)DelegateProxies.CreateMultiUserData(getTable, destroy, this), DelegateProxies.ReleaseDelegateProxyForMulti);
	}

	internal Face(IntPtr handle)
		: base(handle)
	{
	}

	public Blob ReferenceTable(Tag table)
	{
		return new Blob(HarfBuzzApi.hb_face_reference_table(Handle, table));
	}

	public void MakeImmutable()
	{
		HarfBuzzApi.hb_face_make_immutable(Handle);
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}

	protected override void DisposeHandler()
	{
		if (Handle != IntPtr.Zero)
		{
			HarfBuzzApi.hb_face_destroy(Handle);
		}
	}
}
