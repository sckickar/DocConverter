using System;
using System.ComponentModel;
using System.IO;

namespace SkiaSharp;

public class SKDocument : SKObject, ISKReferenceCounted, ISKSkipObjectRegistration
{
	public const float DefaultRasterDpi = 72f;

	internal SKDocument(IntPtr handle, bool owns)
		: base(handle, owns)
	{
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}

	public void Abort()
	{
		SkiaApi.sk_document_abort(Handle);
	}

	public unsafe SKCanvas BeginPage(float width, float height)
	{
		return SKObject.OwnedBy(SKCanvas.GetObject(SkiaApi.sk_document_begin_page(Handle, width, height, null), owns: false), this);
	}

	public unsafe SKCanvas BeginPage(float width, float height, SKRect content)
	{
		return SKObject.OwnedBy(SKCanvas.GetObject(SkiaApi.sk_document_begin_page(Handle, width, height, &content), owns: false), this);
	}

	public void EndPage()
	{
		SkiaApi.sk_document_end_page(Handle);
	}

	public void Close()
	{
		SkiaApi.sk_document_close(Handle);
	}

	public static SKDocument CreateXps(string path)
	{
		return CreateXps(path, 72f);
	}

	public static SKDocument CreateXps(Stream stream)
	{
		return CreateXps(stream, 72f);
	}

	public static SKDocument CreateXps(SKWStream stream)
	{
		return CreateXps(stream, 72f);
	}

	public static SKDocument CreateXps(string path, float dpi)
	{
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		SKWStream sKWStream = SKFileWStream.OpenStream(path);
		return SKObject.Owned(CreateXps(sKWStream, dpi), sKWStream);
	}

	public static SKDocument CreateXps(Stream stream, float dpi)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		SKManagedWStream sKManagedWStream = new SKManagedWStream(stream);
		return SKObject.Owned(CreateXps(sKManagedWStream, dpi), sKManagedWStream);
	}

	public static SKDocument CreateXps(SKWStream stream, float dpi)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		return SKObject.Referenced(GetObject(SkiaApi.sk_document_create_xps_from_stream(stream.Handle, dpi)), stream);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use CreatePdf(SKWStream, SKDocumentPdfMetadata) instead.")]
	public static SKDocument CreatePdf(SKWStream stream, SKDocumentPdfMetadata metadata, float dpi)
	{
		metadata.RasterDpi = dpi;
		return CreatePdf(stream, metadata);
	}

	public static SKDocument CreatePdf(string path)
	{
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		SKWStream sKWStream = SKFileWStream.OpenStream(path);
		return SKObject.Owned(CreatePdf(sKWStream), sKWStream);
	}

	public static SKDocument CreatePdf(Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		SKManagedWStream sKManagedWStream = new SKManagedWStream(stream);
		return SKObject.Owned(CreatePdf(sKManagedWStream), sKManagedWStream);
	}

	public static SKDocument CreatePdf(SKWStream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		return SKObject.Referenced(GetObject(SkiaApi.sk_document_create_pdf_from_stream(stream.Handle)), stream);
	}

	public static SKDocument CreatePdf(string path, float dpi)
	{
		return CreatePdf(path, new SKDocumentPdfMetadata(dpi));
	}

	public static SKDocument CreatePdf(Stream stream, float dpi)
	{
		return CreatePdf(stream, new SKDocumentPdfMetadata(dpi));
	}

	public static SKDocument CreatePdf(SKWStream stream, float dpi)
	{
		return CreatePdf(stream, new SKDocumentPdfMetadata(dpi));
	}

	public static SKDocument CreatePdf(string path, SKDocumentPdfMetadata metadata)
	{
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		SKWStream sKWStream = SKFileWStream.OpenStream(path);
		return SKObject.Owned(CreatePdf(sKWStream, metadata), sKWStream);
	}

	public static SKDocument CreatePdf(Stream stream, SKDocumentPdfMetadata metadata)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		SKManagedWStream sKManagedWStream = new SKManagedWStream(stream);
		return SKObject.Owned(CreatePdf(sKManagedWStream, metadata), sKManagedWStream);
	}

	public unsafe static SKDocument CreatePdf(SKWStream stream, SKDocumentPdfMetadata metadata)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		using SKString sKString = SKString.Create(metadata.Title);
		using SKString sKString2 = SKString.Create(metadata.Author);
		using SKString sKString3 = SKString.Create(metadata.Subject);
		using SKString sKString4 = SKString.Create(metadata.Keywords);
		using SKString sKString5 = SKString.Create(metadata.Creator);
		using SKString sKString6 = SKString.Create(metadata.Producer);
		SKDocumentPdfMetadataInternal sKDocumentPdfMetadataInternal = default(SKDocumentPdfMetadataInternal);
		sKDocumentPdfMetadataInternal.fTitle = sKString?.Handle ?? IntPtr.Zero;
		sKDocumentPdfMetadataInternal.fAuthor = sKString2?.Handle ?? IntPtr.Zero;
		sKDocumentPdfMetadataInternal.fSubject = sKString3?.Handle ?? IntPtr.Zero;
		sKDocumentPdfMetadataInternal.fKeywords = sKString4?.Handle ?? IntPtr.Zero;
		sKDocumentPdfMetadataInternal.fCreator = sKString5?.Handle ?? IntPtr.Zero;
		sKDocumentPdfMetadataInternal.fProducer = sKString6?.Handle ?? IntPtr.Zero;
		sKDocumentPdfMetadataInternal.fRasterDPI = metadata.RasterDpi;
		sKDocumentPdfMetadataInternal.fPDFA = (metadata.PdfA ? ((byte)1) : ((byte)0));
		sKDocumentPdfMetadataInternal.fEncodingQuality = metadata.EncodingQuality;
		SKDocumentPdfMetadataInternal sKDocumentPdfMetadataInternal2 = sKDocumentPdfMetadataInternal;
		if (metadata.Creation.HasValue)
		{
			SKTimeDateTimeInternal sKTimeDateTimeInternal = SKTimeDateTimeInternal.Create(metadata.Creation.Value);
			sKDocumentPdfMetadataInternal2.fCreation = &sKTimeDateTimeInternal;
		}
		if (metadata.Modified.HasValue)
		{
			SKTimeDateTimeInternal sKTimeDateTimeInternal2 = SKTimeDateTimeInternal.Create(metadata.Modified.Value);
			sKDocumentPdfMetadataInternal2.fModified = &sKTimeDateTimeInternal2;
		}
		return SKObject.Referenced(GetObject(SkiaApi.sk_document_create_pdf_from_stream_with_metadata(stream.Handle, &sKDocumentPdfMetadataInternal2)), stream);
	}

	internal static SKDocument GetObject(IntPtr handle)
	{
		if (!(handle == IntPtr.Zero))
		{
			return new SKDocument(handle, owns: true);
		}
		return null;
	}
}
