using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using BitMiracle.LibTiff.Classic.Internal;

namespace BitMiracle.LibTiff.Classic;

public class Tiff : IDisposable
{
	internal enum PostDecodeMethodType
	{
		pdmNone,
		pdmSwab16Bit,
		pdmSwab24Bit,
		pdmSwab32Bit,
		pdmSwab64Bit
	}

	private class codecList
	{
		public codecList next;

		public TiffCodec codec;
	}

	private class clientInfoLink
	{
		public clientInfoLink next;

		public object data;

		public string name;
	}

	public delegate void TiffExtendProc(Tiff tif);

	public delegate void FaxFillFunc(byte[] buffer, int offset, int[] runs, int thisRunOffset, int nextRunOffset, int width);

	private static TiffErrorHandler m_errorHandler;

	private static TiffExtendProc m_extender;

	private static readonly TiffFieldInfo[] tiffFieldInfo = new TiffFieldInfo[172]
	{
		new TiffFieldInfo(TiffTag.SUBFILETYPE, 1, 1, TiffType.LONG, 5, okToChange: true, passCount: false, "SubfileType"),
		new TiffFieldInfo(TiffTag.SUBFILETYPE, 1, 1, TiffType.SHORT, 5, okToChange: true, passCount: false, "SubfileType"),
		new TiffFieldInfo(TiffTag.OSUBFILETYPE, 1, 1, TiffType.SHORT, 5, okToChange: true, passCount: false, "OldSubfileType"),
		new TiffFieldInfo(TiffTag.IMAGEWIDTH, 1, 1, TiffType.LONG, 1, okToChange: false, passCount: false, "ImageWidth"),
		new TiffFieldInfo(TiffTag.IMAGEWIDTH, 1, 1, TiffType.SHORT, 1, okToChange: false, passCount: false, "ImageWidth"),
		new TiffFieldInfo(TiffTag.IMAGELENGTH, 1, 1, TiffType.LONG, 1, okToChange: true, passCount: false, "ImageLength"),
		new TiffFieldInfo(TiffTag.IMAGELENGTH, 1, 1, TiffType.SHORT, 1, okToChange: true, passCount: false, "ImageLength"),
		new TiffFieldInfo(TiffTag.BITSPERSAMPLE, -1, -1, TiffType.SHORT, 6, okToChange: false, passCount: false, "BitsPerSample"),
		new TiffFieldInfo(TiffTag.BITSPERSAMPLE, -1, -1, TiffType.LONG, 6, okToChange: false, passCount: false, "BitsPerSample"),
		new TiffFieldInfo(TiffTag.COMPRESSION, -1, 1, TiffType.SHORT, 7, okToChange: false, passCount: false, "Compression"),
		new TiffFieldInfo(TiffTag.COMPRESSION, -1, 1, TiffType.LONG, 7, okToChange: false, passCount: false, "Compression"),
		new TiffFieldInfo(TiffTag.PHOTOMETRIC, 1, 1, TiffType.SHORT, 8, okToChange: false, passCount: false, "PhotometricInterpretation"),
		new TiffFieldInfo(TiffTag.PHOTOMETRIC, 1, 1, TiffType.LONG, 8, okToChange: false, passCount: false, "PhotometricInterpretation"),
		new TiffFieldInfo(TiffTag.THRESHHOLDING, 1, 1, TiffType.SHORT, 9, okToChange: true, passCount: false, "Threshholding"),
		new TiffFieldInfo(TiffTag.CELLWIDTH, 1, 1, TiffType.SHORT, 0, okToChange: true, passCount: false, "CellWidth"),
		new TiffFieldInfo(TiffTag.CELLLENGTH, 1, 1, TiffType.SHORT, 0, okToChange: true, passCount: false, "CellLength"),
		new TiffFieldInfo(TiffTag.FILLORDER, 1, 1, TiffType.SHORT, 10, okToChange: false, passCount: false, "FillOrder"),
		new TiffFieldInfo(TiffTag.DOCUMENTNAME, -1, -1, TiffType.ASCII, 65, okToChange: true, passCount: false, "DocumentName"),
		new TiffFieldInfo(TiffTag.IMAGEDESCRIPTION, -1, -1, TiffType.ASCII, 65, okToChange: true, passCount: false, "ImageDescription"),
		new TiffFieldInfo(TiffTag.MAKE, -1, -1, TiffType.ASCII, 65, okToChange: true, passCount: false, "Make"),
		new TiffFieldInfo(TiffTag.MODEL, -1, -1, TiffType.ASCII, 65, okToChange: true, passCount: false, "Model"),
		new TiffFieldInfo(TiffTag.STRIPOFFSETS, -1, -1, TiffType.LONG, 25, okToChange: false, passCount: false, "StripOffsets"),
		new TiffFieldInfo(TiffTag.STRIPOFFSETS, -1, -1, TiffType.LONG8, 25, okToChange: false, passCount: false, "StripOffsets"),
		new TiffFieldInfo(TiffTag.STRIPOFFSETS, -1, -1, TiffType.SHORT, 25, okToChange: false, passCount: false, "StripOffsets"),
		new TiffFieldInfo(TiffTag.ORIENTATION, 1, 1, TiffType.SHORT, 15, okToChange: false, passCount: false, "Orientation"),
		new TiffFieldInfo(TiffTag.SAMPLESPERPIXEL, 1, 1, TiffType.SHORT, 16, okToChange: false, passCount: false, "SamplesPerPixel"),
		new TiffFieldInfo(TiffTag.ROWSPERSTRIP, 1, 1, TiffType.LONG, 17, okToChange: false, passCount: false, "RowsPerStrip"),
		new TiffFieldInfo(TiffTag.ROWSPERSTRIP, 1, 1, TiffType.SHORT, 17, okToChange: false, passCount: false, "RowsPerStrip"),
		new TiffFieldInfo(TiffTag.STRIPBYTECOUNTS, -1, -1, TiffType.LONG, 24, okToChange: false, passCount: false, "StripByteCounts"),
		new TiffFieldInfo(TiffTag.STRIPBYTECOUNTS, -1, -1, TiffType.LONG8, 24, okToChange: false, passCount: false, "StripByteCounts"),
		new TiffFieldInfo(TiffTag.STRIPBYTECOUNTS, -1, -1, TiffType.SHORT, 24, okToChange: false, passCount: false, "StripByteCounts"),
		new TiffFieldInfo(TiffTag.MINSAMPLEVALUE, -2, -1, TiffType.SHORT, 18, okToChange: true, passCount: false, "MinSampleValue"),
		new TiffFieldInfo(TiffTag.MAXSAMPLEVALUE, -2, -1, TiffType.SHORT, 19, okToChange: true, passCount: false, "MaxSampleValue"),
		new TiffFieldInfo(TiffTag.XRESOLUTION, 1, 1, TiffType.RATIONAL, 3, okToChange: true, passCount: false, "XResolution"),
		new TiffFieldInfo(TiffTag.YRESOLUTION, 1, 1, TiffType.RATIONAL, 3, okToChange: true, passCount: false, "YResolution"),
		new TiffFieldInfo(TiffTag.PLANARCONFIG, 1, 1, TiffType.SHORT, 20, okToChange: false, passCount: false, "PlanarConfiguration"),
		new TiffFieldInfo(TiffTag.PAGENAME, -1, -1, TiffType.ASCII, 65, okToChange: true, passCount: false, "PageName"),
		new TiffFieldInfo(TiffTag.XPOSITION, 1, 1, TiffType.RATIONAL, 4, okToChange: true, passCount: false, "XPosition"),
		new TiffFieldInfo(TiffTag.YPOSITION, 1, 1, TiffType.RATIONAL, 4, okToChange: true, passCount: false, "YPosition"),
		new TiffFieldInfo(TiffTag.FREEOFFSETS, -1, -1, TiffType.LONG, 0, okToChange: false, passCount: false, "FreeOffsets"),
		new TiffFieldInfo(TiffTag.FREEBYTECOUNTS, -1, -1, TiffType.LONG, 0, okToChange: false, passCount: false, "FreeByteCounts"),
		new TiffFieldInfo(TiffTag.GRAYRESPONSEUNIT, 1, 1, TiffType.SHORT, 0, okToChange: true, passCount: false, "GrayResponseUnit"),
		new TiffFieldInfo(TiffTag.GRAYRESPONSECURVE, -1, -1, TiffType.SHORT, 0, okToChange: true, passCount: false, "GrayResponseCurve"),
		new TiffFieldInfo(TiffTag.RESOLUTIONUNIT, 1, 1, TiffType.SHORT, 22, okToChange: true, passCount: false, "ResolutionUnit"),
		new TiffFieldInfo(TiffTag.PAGENUMBER, 2, 2, TiffType.SHORT, 23, okToChange: true, passCount: false, "PageNumber"),
		new TiffFieldInfo(TiffTag.COLORRESPONSEUNIT, 1, 1, TiffType.SHORT, 0, okToChange: true, passCount: false, "ColorResponseUnit"),
		new TiffFieldInfo(TiffTag.TRANSFERFUNCTION, -1, -1, TiffType.SHORT, 44, okToChange: true, passCount: false, "TransferFunction"),
		new TiffFieldInfo(TiffTag.SOFTWARE, -1, -1, TiffType.ASCII, 65, okToChange: true, passCount: false, "Software"),
		new TiffFieldInfo(TiffTag.DATETIME, 20, 20, TiffType.ASCII, 65, okToChange: true, passCount: false, "DateTime"),
		new TiffFieldInfo(TiffTag.ARTIST, -1, -1, TiffType.ASCII, 65, okToChange: true, passCount: false, "Artist"),
		new TiffFieldInfo(TiffTag.HOSTCOMPUTER, -1, -1, TiffType.ASCII, 65, okToChange: true, passCount: false, "HostComputer"),
		new TiffFieldInfo(TiffTag.WHITEPOINT, 2, 2, TiffType.RATIONAL, 65, okToChange: true, passCount: false, "WhitePoint"),
		new TiffFieldInfo(TiffTag.PRIMARYCHROMATICITIES, 6, 6, TiffType.RATIONAL, 65, okToChange: true, passCount: false, "PrimaryChromaticities"),
		new TiffFieldInfo(TiffTag.COLORMAP, -1, -1, TiffType.SHORT, 26, okToChange: true, passCount: false, "ColorMap"),
		new TiffFieldInfo(TiffTag.HALFTONEHINTS, 2, 2, TiffType.SHORT, 37, okToChange: true, passCount: false, "HalftoneHints"),
		new TiffFieldInfo(TiffTag.TILEWIDTH, 1, 1, TiffType.LONG, 2, okToChange: false, passCount: false, "TileWidth"),
		new TiffFieldInfo(TiffTag.TILEWIDTH, 1, 1, TiffType.SHORT, 2, okToChange: false, passCount: false, "TileWidth"),
		new TiffFieldInfo(TiffTag.TILELENGTH, 1, 1, TiffType.LONG, 2, okToChange: false, passCount: false, "TileLength"),
		new TiffFieldInfo(TiffTag.TILELENGTH, 1, 1, TiffType.SHORT, 2, okToChange: false, passCount: false, "TileLength"),
		new TiffFieldInfo(TiffTag.TILEOFFSETS, -1, 1, TiffType.LONG, 25, okToChange: false, passCount: false, "TileOffsets"),
		new TiffFieldInfo(TiffTag.TILEOFFSETS, -1, 1, TiffType.LONG8, 25, okToChange: false, passCount: false, "TileOffsets"),
		new TiffFieldInfo(TiffTag.TILEBYTECOUNTS, -1, 1, TiffType.LONG, 24, okToChange: false, passCount: false, "TileByteCounts"),
		new TiffFieldInfo(TiffTag.TILEBYTECOUNTS, -1, 1, TiffType.SHORT, 24, okToChange: false, passCount: false, "TileByteCounts"),
		new TiffFieldInfo(TiffTag.TILEBYTECOUNTS, -1, 1, TiffType.LONG8, 24, okToChange: false, passCount: false, "TileByteCounts"),
		new TiffFieldInfo(TiffTag.SUBIFD, -1, -1, TiffType.IFD, 49, okToChange: true, passCount: true, "SubIFD"),
		new TiffFieldInfo(TiffTag.SUBIFD, -1, -1, TiffType.IFD8, 49, okToChange: true, passCount: true, "SubIFD"),
		new TiffFieldInfo(TiffTag.SUBIFD, -1, -1, TiffType.LONG, 49, okToChange: true, passCount: true, "SubIFD"),
		new TiffFieldInfo(TiffTag.SUBIFD, -1, -1, TiffType.LONG8, 49, okToChange: true, passCount: true, "SubIFD"),
		new TiffFieldInfo(TiffTag.INKSET, 1, 1, TiffType.SHORT, 65, okToChange: false, passCount: false, "InkSet"),
		new TiffFieldInfo(TiffTag.INKNAMES, -1, -1, TiffType.ASCII, 46, okToChange: true, passCount: true, "InkNames"),
		new TiffFieldInfo(TiffTag.NUMBEROFINKS, 1, 1, TiffType.SHORT, 65, okToChange: true, passCount: false, "NumberOfInks"),
		new TiffFieldInfo(TiffTag.DOTRANGE, 2, 2, TiffType.SHORT, 65, okToChange: false, passCount: false, "DotRange"),
		new TiffFieldInfo(TiffTag.DOTRANGE, 2, 2, TiffType.BYTE, 65, okToChange: false, passCount: false, "DotRange"),
		new TiffFieldInfo(TiffTag.TARGETPRINTER, -1, -1, TiffType.ASCII, 65, okToChange: true, passCount: false, "TargetPrinter"),
		new TiffFieldInfo(TiffTag.EXTRASAMPLES, -1, -1, TiffType.SHORT, 31, okToChange: false, passCount: true, "ExtraSamples"),
		new TiffFieldInfo(TiffTag.EXTRASAMPLES, -1, -1, TiffType.BYTE, 31, okToChange: false, passCount: true, "ExtraSamples"),
		new TiffFieldInfo(TiffTag.SAMPLEFORMAT, -1, -1, TiffType.SHORT, 32, okToChange: false, passCount: false, "SampleFormat"),
		new TiffFieldInfo(TiffTag.SMINSAMPLEVALUE, -2, -1, TiffType.NOTYPE, 33, okToChange: true, passCount: false, "SMinSampleValue"),
		new TiffFieldInfo(TiffTag.SMAXSAMPLEVALUE, -2, -1, TiffType.NOTYPE, 34, okToChange: true, passCount: false, "SMaxSampleValue"),
		new TiffFieldInfo(TiffTag.CLIPPATH, -1, -3, TiffType.BYTE, 65, okToChange: false, passCount: true, "ClipPath"),
		new TiffFieldInfo(TiffTag.XCLIPPATHUNITS, 1, 1, TiffType.SLONG, 65, okToChange: false, passCount: false, "XClipPathUnits"),
		new TiffFieldInfo(TiffTag.XCLIPPATHUNITS, 1, 1, TiffType.SSHORT, 65, okToChange: false, passCount: false, "XClipPathUnits"),
		new TiffFieldInfo(TiffTag.XCLIPPATHUNITS, 1, 1, TiffType.SBYTE, 65, okToChange: false, passCount: false, "XClipPathUnits"),
		new TiffFieldInfo(TiffTag.YCLIPPATHUNITS, 1, 1, TiffType.SLONG, 65, okToChange: false, passCount: false, "YClipPathUnits"),
		new TiffFieldInfo(TiffTag.YCLIPPATHUNITS, 1, 1, TiffType.SSHORT, 65, okToChange: false, passCount: false, "YClipPathUnits"),
		new TiffFieldInfo(TiffTag.YCLIPPATHUNITS, 1, 1, TiffType.SBYTE, 65, okToChange: false, passCount: false, "YClipPathUnits"),
		new TiffFieldInfo(TiffTag.YCBCRCOEFFICIENTS, 3, 3, TiffType.RATIONAL, 65, okToChange: false, passCount: false, "YCbCrCoefficients"),
		new TiffFieldInfo(TiffTag.YCBCRSUBSAMPLING, 2, 2, TiffType.SHORT, 39, okToChange: false, passCount: false, "YCbCrSubsampling"),
		new TiffFieldInfo(TiffTag.YCBCRPOSITIONING, 1, 1, TiffType.SHORT, 40, okToChange: false, passCount: false, "YCbCrPositioning"),
		new TiffFieldInfo(TiffTag.REFERENCEBLACKWHITE, 6, 6, TiffType.RATIONAL, 41, okToChange: true, passCount: false, "ReferenceBlackWhite"),
		new TiffFieldInfo(TiffTag.REFERENCEBLACKWHITE, 6, 6, TiffType.LONG, 41, okToChange: true, passCount: false, "ReferenceBlackWhite"),
		new TiffFieldInfo(TiffTag.XMLPACKET, -3, -3, TiffType.BYTE, 65, okToChange: false, passCount: true, "XMLPacket"),
		new TiffFieldInfo(TiffTag.MATTEING, 1, 1, TiffType.SHORT, 31, okToChange: false, passCount: false, "Matteing"),
		new TiffFieldInfo(TiffTag.DATATYPE, -2, -1, TiffType.SHORT, 32, okToChange: false, passCount: false, "DataType"),
		new TiffFieldInfo(TiffTag.IMAGEDEPTH, 1, 1, TiffType.LONG, 35, okToChange: false, passCount: false, "ImageDepth"),
		new TiffFieldInfo(TiffTag.IMAGEDEPTH, 1, 1, TiffType.SHORT, 35, okToChange: false, passCount: false, "ImageDepth"),
		new TiffFieldInfo(TiffTag.TILEDEPTH, 1, 1, TiffType.LONG, 36, okToChange: false, passCount: false, "TileDepth"),
		new TiffFieldInfo(TiffTag.TILEDEPTH, 1, 1, TiffType.SHORT, 36, okToChange: false, passCount: false, "TileDepth"),
		new TiffFieldInfo(TiffTag.PIXAR_IMAGEFULLWIDTH, 1, 1, TiffType.LONG, 65, okToChange: true, passCount: false, "ImageFullWidth"),
		new TiffFieldInfo(TiffTag.PIXAR_IMAGEFULLLENGTH, 1, 1, TiffType.LONG, 65, okToChange: true, passCount: false, "ImageFullLength"),
		new TiffFieldInfo(TiffTag.PIXAR_TEXTUREFORMAT, -1, -1, TiffType.ASCII, 65, okToChange: true, passCount: false, "TextureFormat"),
		new TiffFieldInfo(TiffTag.PIXAR_WRAPMODES, -1, -1, TiffType.ASCII, 65, okToChange: true, passCount: false, "TextureWrapModes"),
		new TiffFieldInfo(TiffTag.PIXAR_FOVCOT, 1, 1, TiffType.FLOAT, 65, okToChange: true, passCount: false, "FieldOfViewCotangent"),
		new TiffFieldInfo(TiffTag.PIXAR_MATRIX_WORLDTOSCREEN, 16, 16, TiffType.FLOAT, 65, okToChange: true, passCount: false, "MatrixWorldToScreen"),
		new TiffFieldInfo(TiffTag.PIXAR_MATRIX_WORLDTOCAMERA, 16, 16, TiffType.FLOAT, 65, okToChange: true, passCount: false, "MatrixWorldToCamera"),
		new TiffFieldInfo(TiffTag.COPYRIGHT, -1, -1, TiffType.ASCII, 65, okToChange: true, passCount: false, "Copyright"),
		new TiffFieldInfo(TiffTag.RICHTIFFIPTC, -3, -3, TiffType.LONG, 65, okToChange: false, passCount: true, "RichTIFFIPTC"),
		new TiffFieldInfo(TiffTag.PHOTOSHOP, -3, -3, TiffType.BYTE, 65, okToChange: false, passCount: true, "Photoshop"),
		new TiffFieldInfo(TiffTag.EXIFIFD, 1, 1, TiffType.LONG, 65, okToChange: false, passCount: false, "EXIFIFDOffset"),
		new TiffFieldInfo(TiffTag.ICCPROFILE, -3, -3, TiffType.UNDEFINED, 65, okToChange: false, passCount: true, "ICC Profile"),
		new TiffFieldInfo(TiffTag.GPSIFD, 1, 1, TiffType.LONG, 65, okToChange: false, passCount: false, "GPSIFDOffset"),
		new TiffFieldInfo(TiffTag.STONITS, 1, 1, TiffType.DOUBLE, 65, okToChange: false, passCount: false, "StoNits"),
		new TiffFieldInfo(TiffTag.INTEROPERABILITYIFD, 1, 1, TiffType.LONG, 65, okToChange: false, passCount: false, "InteroperabilityIFDOffset"),
		new TiffFieldInfo(TiffTag.DNGVERSION, 4, 4, TiffType.BYTE, 65, okToChange: false, passCount: false, "DNGVersion"),
		new TiffFieldInfo(TiffTag.DNGBACKWARDVERSION, 4, 4, TiffType.BYTE, 65, okToChange: false, passCount: false, "DNGBackwardVersion"),
		new TiffFieldInfo(TiffTag.UNIQUECAMERAMODEL, -1, -1, TiffType.ASCII, 65, okToChange: true, passCount: false, "UniqueCameraModel"),
		new TiffFieldInfo(TiffTag.LOCALIZEDCAMERAMODEL, -1, -1, TiffType.ASCII, 65, okToChange: true, passCount: false, "LocalizedCameraModel"),
		new TiffFieldInfo(TiffTag.LOCALIZEDCAMERAMODEL, -1, -1, TiffType.BYTE, 65, okToChange: true, passCount: true, "LocalizedCameraModel"),
		new TiffFieldInfo(TiffTag.CFAPLANECOLOR, -1, -1, TiffType.BYTE, 65, okToChange: false, passCount: true, "CFAPlaneColor"),
		new TiffFieldInfo(TiffTag.CFALAYOUT, 1, 1, TiffType.SHORT, 65, okToChange: false, passCount: false, "CFALayout"),
		new TiffFieldInfo(TiffTag.LINEARIZATIONTABLE, -1, -1, TiffType.SHORT, 65, okToChange: false, passCount: true, "LinearizationTable"),
		new TiffFieldInfo(TiffTag.BLACKLEVELREPEATDIM, 2, 2, TiffType.SHORT, 65, okToChange: false, passCount: false, "BlackLevelRepeatDim"),
		new TiffFieldInfo(TiffTag.BLACKLEVEL, -1, -1, TiffType.LONG, 65, okToChange: false, passCount: true, "BlackLevel"),
		new TiffFieldInfo(TiffTag.BLACKLEVEL, -1, -1, TiffType.SHORT, 65, okToChange: false, passCount: true, "BlackLevel"),
		new TiffFieldInfo(TiffTag.BLACKLEVEL, -1, -1, TiffType.RATIONAL, 65, okToChange: false, passCount: true, "BlackLevel"),
		new TiffFieldInfo(TiffTag.BLACKLEVELDELTAH, -1, -1, TiffType.SRATIONAL, 65, okToChange: false, passCount: true, "BlackLevelDeltaH"),
		new TiffFieldInfo(TiffTag.BLACKLEVELDELTAV, -1, -1, TiffType.SRATIONAL, 65, okToChange: false, passCount: true, "BlackLevelDeltaV"),
		new TiffFieldInfo(TiffTag.WHITELEVEL, -2, -2, TiffType.LONG, 65, okToChange: false, passCount: false, "WhiteLevel"),
		new TiffFieldInfo(TiffTag.WHITELEVEL, -2, -2, TiffType.SHORT, 65, okToChange: false, passCount: false, "WhiteLevel"),
		new TiffFieldInfo(TiffTag.DEFAULTSCALE, 2, 2, TiffType.RATIONAL, 65, okToChange: false, passCount: false, "DefaultScale"),
		new TiffFieldInfo(TiffTag.BESTQUALITYSCALE, 1, 1, TiffType.RATIONAL, 65, okToChange: false, passCount: false, "BestQualityScale"),
		new TiffFieldInfo(TiffTag.DEFAULTCROPORIGIN, 2, 2, TiffType.LONG, 65, okToChange: false, passCount: false, "DefaultCropOrigin"),
		new TiffFieldInfo(TiffTag.DEFAULTCROPORIGIN, 2, 2, TiffType.SHORT, 65, okToChange: false, passCount: false, "DefaultCropOrigin"),
		new TiffFieldInfo(TiffTag.DEFAULTCROPORIGIN, 2, 2, TiffType.RATIONAL, 65, okToChange: false, passCount: false, "DefaultCropOrigin"),
		new TiffFieldInfo(TiffTag.DEFAULTCROPSIZE, 2, 2, TiffType.LONG, 65, okToChange: false, passCount: false, "DefaultCropSize"),
		new TiffFieldInfo(TiffTag.DEFAULTCROPSIZE, 2, 2, TiffType.SHORT, 65, okToChange: false, passCount: false, "DefaultCropSize"),
		new TiffFieldInfo(TiffTag.DEFAULTCROPSIZE, 2, 2, TiffType.RATIONAL, 65, okToChange: false, passCount: false, "DefaultCropSize"),
		new TiffFieldInfo(TiffTag.COLORMATRIX1, -1, -1, TiffType.SRATIONAL, 65, okToChange: false, passCount: true, "ColorMatrix1"),
		new TiffFieldInfo(TiffTag.COLORMATRIX2, -1, -1, TiffType.SRATIONAL, 65, okToChange: false, passCount: true, "ColorMatrix2"),
		new TiffFieldInfo(TiffTag.CAMERACALIBRATION1, -1, -1, TiffType.SRATIONAL, 65, okToChange: false, passCount: true, "CameraCalibration1"),
		new TiffFieldInfo(TiffTag.CAMERACALIBRATION2, -1, -1, TiffType.SRATIONAL, 65, okToChange: false, passCount: true, "CameraCalibration2"),
		new TiffFieldInfo(TiffTag.REDUCTIONMATRIX1, -1, -1, TiffType.SRATIONAL, 65, okToChange: false, passCount: true, "ReductionMatrix1"),
		new TiffFieldInfo(TiffTag.REDUCTIONMATRIX2, -1, -1, TiffType.SRATIONAL, 65, okToChange: false, passCount: true, "ReductionMatrix2"),
		new TiffFieldInfo(TiffTag.ANALOGBALANCE, -1, -1, TiffType.RATIONAL, 65, okToChange: false, passCount: true, "AnalogBalance"),
		new TiffFieldInfo(TiffTag.ASSHOTNEUTRAL, -1, -1, TiffType.SHORT, 65, okToChange: false, passCount: true, "AsShotNeutral"),
		new TiffFieldInfo(TiffTag.ASSHOTNEUTRAL, -1, -1, TiffType.RATIONAL, 65, okToChange: false, passCount: true, "AsShotNeutral"),
		new TiffFieldInfo(TiffTag.ASSHOTWHITEXY, 2, 2, TiffType.RATIONAL, 65, okToChange: false, passCount: false, "AsShotWhiteXY"),
		new TiffFieldInfo(TiffTag.BASELINEEXPOSURE, 1, 1, TiffType.SRATIONAL, 65, okToChange: false, passCount: false, "BaselineExposure"),
		new TiffFieldInfo(TiffTag.BASELINENOISE, 1, 1, TiffType.RATIONAL, 65, okToChange: false, passCount: false, "BaselineNoise"),
		new TiffFieldInfo(TiffTag.BASELINESHARPNESS, 1, 1, TiffType.RATIONAL, 65, okToChange: false, passCount: false, "BaselineSharpness"),
		new TiffFieldInfo(TiffTag.BAYERGREENSPLIT, 1, 1, TiffType.LONG, 65, okToChange: false, passCount: false, "BayerGreenSplit"),
		new TiffFieldInfo(TiffTag.LINEARRESPONSELIMIT, 1, 1, TiffType.RATIONAL, 65, okToChange: false, passCount: false, "LinearResponseLimit"),
		new TiffFieldInfo(TiffTag.CAMERASERIALNUMBER, -1, -1, TiffType.ASCII, 65, okToChange: true, passCount: false, "CameraSerialNumber"),
		new TiffFieldInfo(TiffTag.LENSINFO, 4, 4, TiffType.RATIONAL, 65, okToChange: false, passCount: false, "LensInfo"),
		new TiffFieldInfo(TiffTag.CHROMABLURRADIUS, 1, 1, TiffType.RATIONAL, 65, okToChange: false, passCount: false, "ChromaBlurRadius"),
		new TiffFieldInfo(TiffTag.ANTIALIASSTRENGTH, 1, 1, TiffType.RATIONAL, 65, okToChange: false, passCount: false, "AntiAliasStrength"),
		new TiffFieldInfo(TiffTag.SHADOWSCALE, 1, 1, TiffType.RATIONAL, 65, okToChange: false, passCount: false, "ShadowScale"),
		new TiffFieldInfo(TiffTag.DNGPRIVATEDATA, -1, -1, TiffType.BYTE, 65, okToChange: false, passCount: true, "DNGPrivateData"),
		new TiffFieldInfo(TiffTag.MAKERNOTESAFETY, 1, 1, TiffType.SHORT, 65, okToChange: false, passCount: false, "MakerNoteSafety"),
		new TiffFieldInfo(TiffTag.CALIBRATIONILLUMINANT1, 1, 1, TiffType.SHORT, 65, okToChange: false, passCount: false, "CalibrationIlluminant1"),
		new TiffFieldInfo(TiffTag.CALIBRATIONILLUMINANT2, 1, 1, TiffType.SHORT, 65, okToChange: false, passCount: false, "CalibrationIlluminant2"),
		new TiffFieldInfo(TiffTag.RAWDATAUNIQUEID, 16, 16, TiffType.BYTE, 65, okToChange: false, passCount: false, "RawDataUniqueID"),
		new TiffFieldInfo(TiffTag.ORIGINALRAWFILENAME, -1, -1, TiffType.ASCII, 65, okToChange: true, passCount: false, "OriginalRawFileName"),
		new TiffFieldInfo(TiffTag.ORIGINALRAWFILENAME, -1, -1, TiffType.BYTE, 65, okToChange: true, passCount: true, "OriginalRawFileName"),
		new TiffFieldInfo(TiffTag.ORIGINALRAWFILEDATA, -1, -1, TiffType.UNDEFINED, 65, okToChange: false, passCount: true, "OriginalRawFileData"),
		new TiffFieldInfo(TiffTag.ACTIVEAREA, 4, 4, TiffType.LONG, 65, okToChange: false, passCount: false, "ActiveArea"),
		new TiffFieldInfo(TiffTag.ACTIVEAREA, 4, 4, TiffType.SHORT, 65, okToChange: false, passCount: false, "ActiveArea"),
		new TiffFieldInfo(TiffTag.MASKEDAREAS, -1, -1, TiffType.LONG, 65, okToChange: false, passCount: true, "MaskedAreas"),
		new TiffFieldInfo(TiffTag.ASSHOTICCPROFILE, -1, -1, TiffType.UNDEFINED, 65, okToChange: false, passCount: true, "AsShotICCProfile"),
		new TiffFieldInfo(TiffTag.ASSHOTPREPROFILEMATRIX, -1, -1, TiffType.SRATIONAL, 65, okToChange: false, passCount: true, "AsShotPreProfileMatrix"),
		new TiffFieldInfo(TiffTag.CURRENTICCPROFILE, -1, -1, TiffType.UNDEFINED, 65, okToChange: false, passCount: true, "CurrentICCProfile"),
		new TiffFieldInfo(TiffTag.CURRENTPREPROFILEMATRIX, -1, -1, TiffType.SRATIONAL, 65, okToChange: false, passCount: true, "CurrentPreProfileMatrix")
	};

	private static readonly TiffFieldInfo[] exifFieldInfo = new TiffFieldInfo[58]
	{
		new TiffFieldInfo(TiffTag.EXIF_EXPOSURETIME, 1, 1, TiffType.RATIONAL, 65, okToChange: true, passCount: false, "ExposureTime"),
		new TiffFieldInfo(TiffTag.EXIF_FNUMBER, 1, 1, TiffType.RATIONAL, 65, okToChange: true, passCount: false, "FNumber"),
		new TiffFieldInfo(TiffTag.EXIF_EXPOSUREPROGRAM, 1, 1, TiffType.SHORT, 65, okToChange: true, passCount: false, "ExposureProgram"),
		new TiffFieldInfo(TiffTag.EXIF_SPECTRALSENSITIVITY, -1, -1, TiffType.ASCII, 65, okToChange: true, passCount: false, "SpectralSensitivity"),
		new TiffFieldInfo(TiffTag.EXIF_ISOSPEEDRATINGS, -1, -1, TiffType.SHORT, 65, okToChange: true, passCount: true, "ISOSpeedRatings"),
		new TiffFieldInfo(TiffTag.EXIF_OECF, -1, -1, TiffType.UNDEFINED, 65, okToChange: true, passCount: true, "OptoelectricConversionFactor"),
		new TiffFieldInfo(TiffTag.EXIF_EXIFVERSION, 4, 4, TiffType.UNDEFINED, 65, okToChange: true, passCount: false, "ExifVersion"),
		new TiffFieldInfo(TiffTag.EXIF_DATETIMEORIGINAL, 20, 20, TiffType.ASCII, 65, okToChange: true, passCount: false, "DateTimeOriginal"),
		new TiffFieldInfo(TiffTag.EXIF_DATETIMEDIGITIZED, 20, 20, TiffType.ASCII, 65, okToChange: true, passCount: false, "DateTimeDigitized"),
		new TiffFieldInfo(TiffTag.EXIF_COMPONENTSCONFIGURATION, 4, 4, TiffType.UNDEFINED, 65, okToChange: true, passCount: false, "ComponentsConfiguration"),
		new TiffFieldInfo(TiffTag.EXIF_COMPRESSEDBITSPERPIXEL, 1, 1, TiffType.RATIONAL, 65, okToChange: true, passCount: false, "CompressedBitsPerPixel"),
		new TiffFieldInfo(TiffTag.EXIF_SHUTTERSPEEDVALUE, 1, 1, TiffType.SRATIONAL, 65, okToChange: true, passCount: false, "ShutterSpeedValue"),
		new TiffFieldInfo(TiffTag.EXIF_APERTUREVALUE, 1, 1, TiffType.RATIONAL, 65, okToChange: true, passCount: false, "ApertureValue"),
		new TiffFieldInfo(TiffTag.EXIF_BRIGHTNESSVALUE, 1, 1, TiffType.SRATIONAL, 65, okToChange: true, passCount: false, "BrightnessValue"),
		new TiffFieldInfo(TiffTag.EXIF_EXPOSUREBIASVALUE, 1, 1, TiffType.SRATIONAL, 65, okToChange: true, passCount: false, "ExposureBiasValue"),
		new TiffFieldInfo(TiffTag.EXIF_MAXAPERTUREVALUE, 1, 1, TiffType.RATIONAL, 65, okToChange: true, passCount: false, "MaxApertureValue"),
		new TiffFieldInfo(TiffTag.EXIF_SUBJECTDISTANCE, 1, 1, TiffType.RATIONAL, 65, okToChange: true, passCount: false, "SubjectDistance"),
		new TiffFieldInfo(TiffTag.EXIF_METERINGMODE, 1, 1, TiffType.SHORT, 65, okToChange: true, passCount: false, "MeteringMode"),
		new TiffFieldInfo(TiffTag.EXIF_LIGHTSOURCE, 1, 1, TiffType.SHORT, 65, okToChange: true, passCount: false, "LightSource"),
		new TiffFieldInfo(TiffTag.EXIF_FLASH, 1, 1, TiffType.SHORT, 65, okToChange: true, passCount: false, "Flash"),
		new TiffFieldInfo(TiffTag.EXIF_FOCALLENGTH, 1, 1, TiffType.RATIONAL, 65, okToChange: true, passCount: false, "FocalLength"),
		new TiffFieldInfo(TiffTag.EXIF_SUBJECTAREA, -1, -1, TiffType.SHORT, 65, okToChange: true, passCount: true, "SubjectArea"),
		new TiffFieldInfo(TiffTag.EXIF_MAKERNOTE, -1, -1, TiffType.UNDEFINED, 65, okToChange: true, passCount: true, "MakerNote"),
		new TiffFieldInfo(TiffTag.EXIF_USERCOMMENT, -1, -1, TiffType.UNDEFINED, 65, okToChange: true, passCount: true, "UserComment"),
		new TiffFieldInfo(TiffTag.EXIF_SUBSECTIME, -1, -1, TiffType.ASCII, 65, okToChange: true, passCount: false, "SubSecTime"),
		new TiffFieldInfo(TiffTag.EXIF_SUBSECTIMEORIGINAL, -1, -1, TiffType.ASCII, 65, okToChange: true, passCount: false, "SubSecTimeOriginal"),
		new TiffFieldInfo(TiffTag.EXIF_SUBSECTIMEDIGITIZED, -1, -1, TiffType.ASCII, 65, okToChange: true, passCount: false, "SubSecTimeDigitized"),
		new TiffFieldInfo(TiffTag.EXIF_FLASHPIXVERSION, 4, 4, TiffType.UNDEFINED, 65, okToChange: true, passCount: false, "FlashpixVersion"),
		new TiffFieldInfo(TiffTag.EXIF_COLORSPACE, 1, 1, TiffType.SHORT, 65, okToChange: true, passCount: false, "ColorSpace"),
		new TiffFieldInfo(TiffTag.EXIF_PIXELXDIMENSION, 1, 1, TiffType.LONG, 65, okToChange: true, passCount: false, "PixelXDimension"),
		new TiffFieldInfo(TiffTag.EXIF_PIXELXDIMENSION, 1, 1, TiffType.SHORT, 65, okToChange: true, passCount: false, "PixelXDimension"),
		new TiffFieldInfo(TiffTag.EXIF_PIXELYDIMENSION, 1, 1, TiffType.LONG, 65, okToChange: true, passCount: false, "PixelYDimension"),
		new TiffFieldInfo(TiffTag.EXIF_PIXELYDIMENSION, 1, 1, TiffType.SHORT, 65, okToChange: true, passCount: false, "PixelYDimension"),
		new TiffFieldInfo(TiffTag.EXIF_RELATEDSOUNDFILE, 13, 13, TiffType.ASCII, 65, okToChange: true, passCount: false, "RelatedSoundFile"),
		new TiffFieldInfo(TiffTag.EXIF_FLASHENERGY, 1, 1, TiffType.RATIONAL, 65, okToChange: true, passCount: false, "FlashEnergy"),
		new TiffFieldInfo(TiffTag.EXIF_SPATIALFREQUENCYRESPONSE, -1, -1, TiffType.UNDEFINED, 65, okToChange: true, passCount: true, "SpatialFrequencyResponse"),
		new TiffFieldInfo(TiffTag.EXIF_FOCALPLANEXRESOLUTION, 1, 1, TiffType.RATIONAL, 65, okToChange: true, passCount: false, "FocalPlaneXResolution"),
		new TiffFieldInfo(TiffTag.EXIF_FOCALPLANEYRESOLUTION, 1, 1, TiffType.RATIONAL, 65, okToChange: true, passCount: false, "FocalPlaneYResolution"),
		new TiffFieldInfo(TiffTag.EXIF_FOCALPLANERESOLUTIONUNIT, 1, 1, TiffType.SHORT, 65, okToChange: true, passCount: false, "FocalPlaneResolutionUnit"),
		new TiffFieldInfo(TiffTag.EXIF_SUBJECTLOCATION, 2, 2, TiffType.SHORT, 65, okToChange: true, passCount: false, "SubjectLocation"),
		new TiffFieldInfo(TiffTag.EXIF_EXPOSUREINDEX, 1, 1, TiffType.RATIONAL, 65, okToChange: true, passCount: false, "ExposureIndex"),
		new TiffFieldInfo(TiffTag.EXIF_SENSINGMETHOD, 1, 1, TiffType.SHORT, 65, okToChange: true, passCount: false, "SensingMethod"),
		new TiffFieldInfo(TiffTag.EXIF_FILESOURCE, 1, 1, TiffType.UNDEFINED, 65, okToChange: true, passCount: false, "FileSource"),
		new TiffFieldInfo(TiffTag.EXIF_SCENETYPE, 1, 1, TiffType.UNDEFINED, 65, okToChange: true, passCount: false, "SceneType"),
		new TiffFieldInfo(TiffTag.EXIF_CFAPATTERN, -1, -1, TiffType.UNDEFINED, 65, okToChange: true, passCount: true, "CFAPattern"),
		new TiffFieldInfo(TiffTag.EXIF_CUSTOMRENDERED, 1, 1, TiffType.SHORT, 65, okToChange: true, passCount: false, "CustomRendered"),
		new TiffFieldInfo(TiffTag.EXIF_EXPOSUREMODE, 1, 1, TiffType.SHORT, 65, okToChange: true, passCount: false, "ExposureMode"),
		new TiffFieldInfo(TiffTag.EXIF_WHITEBALANCE, 1, 1, TiffType.SHORT, 65, okToChange: true, passCount: false, "WhiteBalance"),
		new TiffFieldInfo(TiffTag.EXIF_DIGITALZOOMRATIO, 1, 1, TiffType.RATIONAL, 65, okToChange: true, passCount: false, "DigitalZoomRatio"),
		new TiffFieldInfo(TiffTag.EXIF_FOCALLENGTHIN35MMFILM, 1, 1, TiffType.SHORT, 65, okToChange: true, passCount: false, "FocalLengthIn35mmFilm"),
		new TiffFieldInfo(TiffTag.EXIF_SCENECAPTURETYPE, 1, 1, TiffType.SHORT, 65, okToChange: true, passCount: false, "SceneCaptureType"),
		new TiffFieldInfo(TiffTag.EXIF_GAINCONTROL, 1, 1, TiffType.RATIONAL, 65, okToChange: true, passCount: false, "GainControl"),
		new TiffFieldInfo(TiffTag.EXIF_CONTRAST, 1, 1, TiffType.SHORT, 65, okToChange: true, passCount: false, "Contrast"),
		new TiffFieldInfo(TiffTag.EXIF_SATURATION, 1, 1, TiffType.SHORT, 65, okToChange: true, passCount: false, "Saturation"),
		new TiffFieldInfo(TiffTag.EXIF_SHARPNESS, 1, 1, TiffType.SHORT, 65, okToChange: true, passCount: false, "Sharpness"),
		new TiffFieldInfo(TiffTag.EXIF_DEVICESETTINGDESCRIPTION, -1, -1, TiffType.UNDEFINED, 65, okToChange: true, passCount: true, "DeviceSettingDescription"),
		new TiffFieldInfo(TiffTag.EXIF_SUBJECTDISTANCERANGE, 1, 1, TiffType.SHORT, 65, okToChange: true, passCount: false, "SubjectDistanceRange"),
		new TiffFieldInfo(TiffTag.EXIF_IMAGEUNIQUEID, 33, 33, TiffType.ASCII, 65, okToChange: true, passCount: false, "ImageUniqueID")
	};

	private const int TIFF_VERSION = 42;

	private const int TIFF_BIGTIFF_VERSION = 43;

	private const short TIFF_BIGENDIAN = 19789;

	private const short TIFF_LITTLEENDIAN = 18761;

	private const short MDI_LITTLEENDIAN = 20549;

	private const float D50_X0 = 96.425f;

	private const float D50_Y0 = 100f;

	private const float D50_Z0 = 82.468f;

	internal const int STRIP_SIZE_DEFAULT = 8192;

	internal const TiffFlags STRIPCHOP_DEFAULT = TiffFlags.STRIPCHOP;

	internal const bool DEFAULT_EXTRASAMPLE_AS_ALPHA = true;

	internal const bool CHECK_JPEG_YCBCR_SUBSAMPLING = true;

	internal static readonly Encoding Latin1Encoding = Encoding.GetEncoding("Latin1");

	internal string m_name;

	internal int m_mode;

	internal TiffFlags m_flags;

	internal ulong m_diroff;

	internal TiffDirectory m_dir;

	internal int m_row;

	internal int m_curstrip;

	internal int m_curtile;

	internal int m_tilesize;

	internal TiffCodec m_currentCodec;

	internal int m_scanlinesize;

	internal byte[] m_rawdata;

	internal int m_rawdatasize;

	internal int m_rawcp;

	internal int m_rawcc;

	internal object m_clientdata;

	internal PostDecodeMethodType m_postDecodeMethod;

	internal TiffTagMethods m_tagmethods;

	private ulong m_nextdiroff;

	private ulong[] m_dirlist;

	private int m_dirlistsize;

	private short m_dirnumber;

	private TiffHeader m_header;

	private int[] m_typeshift;

	private uint[] m_typemask;

	private short m_curdir;

	private ulong m_curoff;

	private ulong m_dataoff;

	private short m_nsubifd;

	private ulong m_subifdoff;

	private int m_col;

	private bool m_decodestatus;

	private TiffFieldInfo[] m_fieldinfo;

	private int m_nfields;

	private TiffFieldInfo m_foundfield;

	private clientInfoLink m_clientinfo;

	private TiffCodec[] m_builtInCodecs;

	private codecList m_registeredCodecs;

	private TiffTagMethods m_defaultTagMethods;

	private bool m_disposed;

	private Stream m_fileStream;

	private TiffStream m_stream;

	private static readonly uint[] typemask = new uint[19]
	{
		0u, 255u, 4294967295u, 65535u, 4294967295u, 4294967295u, 255u, 255u, 65535u, 4294967295u,
		4294967295u, 4294967295u, 4294967295u, 4294967295u, 0u, 0u, 4294967295u, 4294967295u, 4294967295u
	};

	private static readonly int[] bigTypeshift = new int[19]
	{
		0, 24, 0, 16, 0, 0, 24, 24, 16, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0
	};

	private static readonly int[] litTypeshift = new int[19];

	private static readonly string[] photoNames = new string[9] { "min-is-white", "min-is-black", "RGB color", "palette color (RGB from colormap)", "transparency mask", "separated", "YCbCr", "7 (0x7)", "CIE L*a*b*" };

	private static readonly string[] orientNames = new string[9] { "0 (0x0)", "row 0 top, col 0 lhs", "row 0 top, col 0 rhs", "row 0 bottom, col 0 rhs", "row 0 bottom, col 0 lhs", "row 0 lhs, col 0 top", "row 0 rhs, col 0 top", "row 0 rhs, col 0 bottom", "row 0 lhs, col 0 bottom" };

	private const int NOSTRIP = -1;

	private const int NOTILE = -1;

	internal const int O_RDONLY = 0;

	internal const int O_WRONLY = 1;

	internal const int O_CREAT = 256;

	internal const int O_TRUNC = 512;

	internal const int O_RDWR = 2;

	private static readonly byte[] TIFFBitRevTable = new byte[256]
	{
		0, 128, 64, 192, 32, 160, 96, 224, 16, 144,
		80, 208, 48, 176, 112, 240, 8, 136, 72, 200,
		40, 168, 104, 232, 24, 152, 88, 216, 56, 184,
		120, 248, 4, 132, 68, 196, 36, 164, 100, 228,
		20, 148, 84, 212, 52, 180, 116, 244, 12, 140,
		76, 204, 44, 172, 108, 236, 28, 156, 92, 220,
		60, 188, 124, 252, 2, 130, 66, 194, 34, 162,
		98, 226, 18, 146, 82, 210, 50, 178, 114, 242,
		10, 138, 74, 202, 42, 170, 106, 234, 26, 154,
		90, 218, 58, 186, 122, 250, 6, 134, 70, 198,
		38, 166, 102, 230, 22, 150, 86, 214, 54, 182,
		118, 246, 14, 142, 78, 206, 46, 174, 110, 238,
		30, 158, 94, 222, 62, 190, 126, 254, 1, 129,
		65, 193, 33, 161, 97, 225, 17, 145, 81, 209,
		49, 177, 113, 241, 9, 137, 73, 201, 41, 169,
		105, 233, 25, 153, 89, 217, 57, 185, 121, 249,
		5, 133, 69, 197, 37, 165, 101, 229, 21, 149,
		85, 213, 53, 181, 117, 245, 13, 141, 77, 205,
		45, 173, 109, 237, 29, 157, 93, 221, 61, 189,
		125, 253, 3, 131, 67, 195, 35, 163, 99, 227,
		19, 147, 83, 211, 51, 179, 115, 243, 11, 139,
		75, 203, 43, 171, 107, 235, 27, 155, 91, 219,
		59, 187, 123, 251, 7, 135, 71, 199, 39, 167,
		103, 231, 23, 151, 87, 215, 55, 183, 119, 247,
		15, 143, 79, 207, 47, 175, 111, 239, 31, 159,
		95, 223, 63, 191, 127, 255
	};

	private static readonly byte[] TIFFNoBitRevTable = new byte[256]
	{
		0, 1, 2, 3, 4, 5, 6, 7, 8, 9,
		10, 11, 12, 13, 14, 15, 16, 17, 18, 19,
		20, 21, 22, 23, 24, 25, 26, 27, 28, 29,
		30, 31, 32, 33, 34, 35, 36, 37, 38, 39,
		40, 41, 42, 43, 44, 45, 46, 47, 48, 49,
		50, 51, 52, 53, 54, 55, 56, 57, 58, 59,
		60, 61, 62, 63, 64, 65, 66, 67, 68, 69,
		70, 71, 72, 73, 74, 75, 76, 77, 78, 79,
		80, 81, 82, 83, 84, 85, 86, 87, 88, 89,
		90, 91, 92, 93, 94, 95, 96, 97, 98, 99,
		100, 101, 102, 103, 104, 105, 106, 107, 108, 109,
		110, 111, 112, 113, 114, 115, 116, 117, 118, 119,
		120, 121, 122, 123, 124, 125, 126, 127, 128, 129,
		130, 131, 132, 133, 134, 135, 136, 137, 138, 139,
		140, 141, 142, 143, 144, 145, 146, 147, 148, 149,
		150, 151, 152, 153, 154, 155, 156, 157, 158, 159,
		160, 161, 162, 163, 164, 165, 166, 167, 168, 169,
		170, 171, 172, 173, 174, 175, 176, 177, 178, 179,
		180, 181, 182, 183, 184, 185, 186, 187, 188, 189,
		190, 191, 192, 193, 194, 195, 196, 197, 198, 199,
		200, 201, 202, 203, 204, 205, 206, 207, 208, 209,
		210, 211, 212, 213, 214, 215, 216, 217, 218, 219,
		220, 221, 222, 223, 224, 225, 226, 227, 228, 229,
		230, 231, 232, 233, 234, 235, 236, 237, 238, 239,
		240, 241, 242, 243, 244, 245, 246, 247, 248, 249,
		250, 251, 252, 253, 254, 255
	};

	private ulong PenultimateDirectoryOffset { get; set; }

	public static string AssemblyVersion => Assembly.GetExecutingAssembly().FullName.Split(new char[1] { ',' })[1].Split(new char[1] { '=' })[1];

	private static Tiff Open(string fileName, string mode, TiffErrorHandler errorHandler)
	{
		return Open(fileName, mode, errorHandler, null);
	}

	private static Tiff Open(string fileName, string mode, TiffErrorHandler errorHandler, TiffExtendProc extender)
	{
		getMode(mode, "Open", out var m, out var a);
		FileStream fileStream = null;
		try
		{
			fileStream = ((a != FileAccess.Read) ? File.Open(fileName, m, a) : File.Open(fileName, m, a, FileShare.Read));
		}
		catch (Exception ex)
		{
			Error("Open", "Failed to open '{0}'. {1}", fileName, ex.Message);
			return null;
		}
		Tiff tiff = ClientOpen(fileName, mode, fileStream, new TiffStream(), errorHandler, extender);
		if (tiff == null)
		{
			fileStream.Dispose();
		}
		else
		{
			tiff.m_fileStream = fileStream;
		}
		return tiff;
	}

	private static Tiff ClientOpen(string name, string mode, object clientData, TiffStream stream, TiffErrorHandler errorHandler)
	{
		return ClientOpen(name, mode, clientData, stream, errorHandler, null);
	}

	private static Tiff ClientOpen(string name, string mode, object clientData, TiffStream stream, TiffErrorHandler errorHandler, TiffExtendProc extender)
	{
		if (mode == null || mode.Length == 0)
		{
			ErrorExt(null, clientData, "ClientOpen", "{0}: mode string should contain at least one char", name);
			return null;
		}
		FileMode m;
		FileAccess a;
		int mode2 = getMode(mode, "ClientOpen", out m, out a);
		Tiff tiff = new Tiff();
		tiff.m_name = name;
		tiff.m_mode = mode2 & -769;
		tiff.m_curdir = -1;
		tiff.m_curoff = 0uL;
		tiff.m_curstrip = -1;
		tiff.m_row = -1;
		tiff.m_clientdata = clientData;
		if (stream == null)
		{
			ErrorExt(tiff, clientData, "ClientOpen", "TiffStream is null pointer.");
			return null;
		}
		tiff.m_stream = stream;
		tiff.m_currentCodec = tiff.m_builtInCodecs[0];
		tiff.m_flags = TiffFlags.MSB2LSB;
		if (mode2 == 0 || mode2 == 2)
		{
			tiff.m_flags |= TiffFlags.STRIPCHOP;
		}
		int length = mode.Length;
		for (int i = 0; i < length; i++)
		{
			switch (mode[i])
			{
			case 'b':
				if (((uint)mode2 & 0x100u) != 0)
				{
					tiff.m_flags |= TiffFlags.SWAB;
				}
				break;
			case 'B':
				tiff.m_flags = (tiff.m_flags & ~TiffFlags.FILLORDER) | TiffFlags.MSB2LSB;
				break;
			case 'L':
				tiff.m_flags = (tiff.m_flags & ~TiffFlags.FILLORDER) | TiffFlags.LSB2MSB;
				break;
			case 'H':
				tiff.m_flags = (tiff.m_flags & ~TiffFlags.FILLORDER) | TiffFlags.LSB2MSB;
				break;
			case 'C':
				if (mode2 == 0)
				{
					tiff.m_flags |= TiffFlags.STRIPCHOP;
				}
				break;
			case 'c':
				if (mode2 == 0)
				{
					tiff.m_flags &= ~TiffFlags.STRIPCHOP;
				}
				break;
			case 'h':
				tiff.m_flags |= TiffFlags.HEADERONLY;
				break;
			case '4':
				tiff.m_flags |= TiffFlags.NOBIGTIFF;
				break;
			case '8':
				tiff.m_flags |= TiffFlags.ISBIGTIFF;
				break;
			}
		}
		if (((uint)tiff.m_mode & 0x200u) != 0 || !tiff.readHeaderOkWithoutExceptions(ref tiff.m_header))
		{
			if (tiff.m_mode == 0)
			{
				ErrorExt(tiff, tiff.m_clientdata, name, "Cannot read TIFF header");
				return null;
			}
			if ((tiff.m_flags & TiffFlags.SWAB) == TiffFlags.SWAB)
			{
				tiff.m_header.tiff_magic = 19789;
			}
			else
			{
				tiff.m_header.tiff_magic = 18761;
			}
			if ((tiff.m_flags & TiffFlags.ISBIGTIFF) == TiffFlags.ISBIGTIFF)
			{
				tiff.m_header.tiff_version = 43;
				tiff.m_header.tiff_diroff = 0uL;
				tiff.m_header.tiff_fill = 0;
				tiff.m_header.tiff_offsize = 8;
				if ((tiff.m_flags & TiffFlags.SWAB) == TiffFlags.SWAB)
				{
					SwabShort(ref tiff.m_header.tiff_version);
					SwabShort(ref tiff.m_header.tiff_offsize);
				}
			}
			else
			{
				tiff.m_header.tiff_version = 42;
				tiff.m_header.tiff_diroff = 0uL;
				tiff.m_header.tiff_fill = 8;
				if ((tiff.m_flags & TiffFlags.SWAB) == TiffFlags.SWAB)
				{
					SwabShort(ref tiff.m_header.tiff_version);
				}
			}
			tiff.seekFile(0L, SeekOrigin.Begin);
			if (!tiff.writeHeaderOK(tiff.m_header))
			{
				ErrorExt(tiff, tiff.m_clientdata, name, "Error writing TIFF header");
				tiff.m_mode = 0;
				return null;
			}
			tiff.initOrder(tiff.m_header.tiff_magic);
			tiff.setupDefaultDirectory();
			tiff.m_diroff = 0uL;
			tiff.m_dirlist = null;
			tiff.m_dirlistsize = 0;
			tiff.m_dirnumber = 0;
			return tiff;
		}
		if (tiff.m_header.tiff_magic != 19789 && tiff.m_header.tiff_magic != 18761 && tiff.m_header.tiff_magic != 20549)
		{
			ErrorExt(tiff, tiff.m_clientdata, name, "Not a TIFF or MDI file, bad magic number {0} (0x{1:x})", tiff.m_header.tiff_magic, tiff.m_header.tiff_magic);
			tiff.m_mode = 0;
			return null;
		}
		tiff.initOrder(tiff.m_header.tiff_magic);
		if ((tiff.m_flags & TiffFlags.SWAB) == TiffFlags.SWAB)
		{
			SwabShort(ref tiff.m_header.tiff_version);
			SwabBigTiffValue(ref tiff.m_header.tiff_diroff, tiff.m_header.tiff_version == 43, isShort: false);
		}
		if (tiff.m_header.tiff_version == 43 && (tiff.m_flags & TiffFlags.NOBIGTIFF) == TiffFlags.NOBIGTIFF)
		{
			ErrorExt(tiff, tiff.m_clientdata, name, "This is a BigTIFF file. Non-BigTIFF mode '32' is forced");
			tiff.m_mode = 0;
			return null;
		}
		if (tiff.m_header.tiff_version == 42 && (tiff.m_flags & TiffFlags.ISBIGTIFF) == TiffFlags.ISBIGTIFF)
		{
			ErrorExt(tiff, tiff.m_clientdata, name, "This is a non-BigTIFF file. BigTIFF mode '64' is forced");
			tiff.m_mode = 0;
			return null;
		}
		if (tiff.m_header.tiff_version != 42 && tiff.m_header.tiff_version != 43)
		{
			ErrorExt(tiff, tiff.m_clientdata, name, "Not a TIFF file, bad version number {0} (0x{1:x})", tiff.m_header.tiff_version, tiff.m_header.tiff_version);
			tiff.m_mode = 0;
			return null;
		}
		tiff.m_flags |= TiffFlags.MYBUFFER;
		tiff.m_rawcp = 0;
		tiff.m_rawdata = null;
		tiff.m_rawdatasize = 0;
		if ((tiff.m_flags & TiffFlags.HEADERONLY) == TiffFlags.HEADERONLY)
		{
			return tiff;
		}
		switch (mode[0])
		{
		case 'r':
			tiff.m_nextdiroff = tiff.m_header.tiff_diroff;
			if (tiff.ReadDirectory())
			{
				tiff.m_rawcc = -1;
				tiff.m_flags |= TiffFlags.BUFFERSETUP;
				return tiff;
			}
			break;
		case 'a':
			tiff.setupDefaultDirectory();
			return tiff;
		}
		tiff.m_mode = 0;
		return null;
	}

	private static TiffErrorHandler setErrorHandlerImpl(TiffErrorHandler errorHandler)
	{
		TiffErrorHandler errorHandler2 = m_errorHandler;
		m_errorHandler = errorHandler;
		return errorHandler2;
	}

	private static TiffExtendProc setTagExtenderImpl(TiffExtendProc extender)
	{
		TiffExtendProc extender2 = m_extender;
		m_extender = extender;
		return extender2;
	}

	private static TiffErrorHandler getErrorHandler(Tiff tif)
	{
		return m_errorHandler;
	}

	private static bool defaultTransferFunction(TiffDirectory td)
	{
		short[][] td_transferfunction = td.td_transferfunction;
		td_transferfunction[0] = null;
		td_transferfunction[1] = null;
		td_transferfunction[2] = null;
		if (td.td_bitspersample >= 30)
		{
			return false;
		}
		int num = 1 << (int)td.td_bitspersample;
		td_transferfunction[0] = new short[num];
		td_transferfunction[0][0] = 0;
		for (int i = 1; i < num; i++)
		{
			double x = (double)i / ((double)num - 1.0);
			td_transferfunction[0][i] = (short)Math.Floor(65535.0 * Math.Pow(x, 2.2) + 0.5);
		}
		if (td.td_samplesperpixel - td.td_extrasamples > 1)
		{
			td_transferfunction[1] = new short[num];
			Buffer.BlockCopy(td_transferfunction[0], 0, td_transferfunction[1], 0, td_transferfunction[0].Length * 2);
			td_transferfunction[2] = new short[num];
			Buffer.BlockCopy(td_transferfunction[0], 0, td_transferfunction[2], 0, td_transferfunction[0].Length * 2);
		}
		return true;
	}

	private static void defaultRefBlackWhite(TiffDirectory td)
	{
		td.td_refblackwhite = new float[6];
		if (td.td_photometric == Photometric.YCBCR)
		{
			td.td_refblackwhite[0] = 0f;
			td.td_refblackwhite[1] = (td.td_refblackwhite[3] = (td.td_refblackwhite[5] = 255f));
			td.td_refblackwhite[2] = (td.td_refblackwhite[4] = 128f);
			return;
		}
		for (int i = 0; i < 3; i++)
		{
			td.td_refblackwhite[2 * i] = 0f;
			td.td_refblackwhite[2 * i + 1] = (1L << (int)td.td_bitspersample) - 1;
		}
	}

	internal static ulong readULong(byte[] buffer, int offset)
	{
		return (ulong)((long)((ulong)buffer[offset++] & 0xFFuL) + ((long)(buffer[offset++] & 0xFF) << 8) + ((long)(buffer[offset++] & 0xFF) << 16) + ((long)(buffer[offset++] & 0xFF) << 24) + ((long)(buffer[offset++] & 0xFF) << 32) + ((long)(buffer[offset++] & 0xFF) << 40) + ((long)(buffer[offset++] & 0xFF) << 48)) + ((ulong)buffer[offset++] << 56);
	}

	internal static int readInt(byte[] buffer, int offset)
	{
		return (buffer[offset++] & 0xFF) + ((buffer[offset++] & 0xFF) << 8) + ((buffer[offset++] & 0xFF) << 16) + (buffer[offset++] << 24);
	}

	internal static void writeInt(int value, byte[] buffer, int offset)
	{
		buffer[offset++] = (byte)value;
		buffer[offset++] = (byte)(value >> 8);
		buffer[offset++] = (byte)(value >> 16);
		buffer[offset++] = (byte)(value >> 24);
	}

	internal static void writeULong(ulong value, byte[] buffer, int offset)
	{
		buffer[offset++] = (byte)value;
		buffer[offset++] = (byte)(value >> 8);
		buffer[offset++] = (byte)(value >> 16);
		buffer[offset++] = (byte)(value >> 24);
		buffer[offset++] = (byte)(value >> 32);
		buffer[offset++] = (byte)(value >> 40);
		buffer[offset++] = (byte)(value >> 48);
		buffer[offset++] = (byte)(value >> 56);
	}

	internal static short readShort(byte[] buffer, int offset)
	{
		return (short)((short)(buffer[offset] & 0xFF) + (short)((buffer[offset + 1] & 0xFF) << 8));
	}

	internal static void fprintf(Stream fd, string format, params object[] list)
	{
		string s = string.Format(CultureInfo.InvariantCulture, format, list);
		byte[] bytes = Latin1Encoding.GetBytes(s);
		fd.Write(bytes, 0, bytes.Length);
	}

	private static string encodeOctalString(byte value)
	{
		return string.Format(CultureInfo.InvariantCulture, "\\{0}{1}{2}", (value >> 6) & 7, (value >> 3) & 7, value & 7);
	}

	private void setupBuiltInCodecs()
	{
		m_builtInCodecs = new TiffCodec[19]
		{
			new TiffCodec(this, (Compression)(-1), "Not configured"),
			new DumpModeCodec(this, Compression.NONE, "None"),
			new LZWCodec(this, Compression.LZW, "LZW"),
			new PackBitsCodec(this, Compression.PACKBITS, "PackBits"),
			new TiffCodec(this, Compression.THUNDERSCAN, "ThunderScan"),
			new TiffCodec(this, Compression.NEXT, "NeXT"),
			new JpegCodec(this, Compression.JPEG, "JPEG"),
			new OJpegCodec(this, Compression.OJPEG, "Old-style JPEG"),
			new CCITTCodec(this, Compression.CCITTRLE, "CCITT RLE"),
			new CCITTCodec(this, Compression.CCITTRLEW, "CCITT RLE/W"),
			new CCITTCodec(this, Compression.CCITTFAX3, "CCITT Group 3"),
			new CCITTCodec(this, Compression.CCITTFAX4, "CCITT Group 4"),
			new TiffCodec(this, Compression.JBIG, "ISO JBIG"),
			new DeflateCodec(this, Compression.DEFLATE, "Deflate"),
			new DeflateCodec(this, Compression.ADOBE_DEFLATE, "AdobeDeflate"),
			new TiffCodec(this, Compression.PIXARLOG, "PixarLog"),
			new TiffCodec(this, Compression.SGILOG, "SGILog"),
			new TiffCodec(this, Compression.SGILOG24, "SGILog24"),
			null
		};
	}

	internal static bool isPseudoTag(TiffTag t)
	{
		return t > TiffTag.DCSHUESHIFTVALUES;
	}

	private bool isFillOrder(FillOrder o)
	{
		return ((uint)m_flags & (uint)o) == (uint)o;
	}

	private static int BITn(int n)
	{
		return 1 << (n & 0x1F);
	}

	private bool okToChangeTag(TiffTag tag)
	{
		TiffFieldInfo tiffFieldInfo = FindFieldInfo(tag, TiffType.NOTYPE);
		if (tiffFieldInfo == null)
		{
			ErrorExt(this, m_clientdata, "SetField", "{0}: Unknown {1}tag {2}", m_name, isPseudoTag(tag) ? "pseudo-" : string.Empty, tag);
			return false;
		}
		if (tag != TiffTag.IMAGELENGTH && (m_flags & TiffFlags.BEENWRITING) == TiffFlags.BEENWRITING && !tiffFieldInfo.OkToChange)
		{
			ErrorExt(this, m_clientdata, "SetField", "{0}: Cannot modify tag \"{1}\" while writing", m_name, tiffFieldInfo.Name);
			return false;
		}
		return true;
	}

	private void setupDefaultDirectory()
	{
		int size;
		TiffFieldInfo[] fieldInfo = getFieldInfo(out size);
		setupFieldInfo(fieldInfo, size);
		m_dir = new TiffDirectory();
		m_postDecodeMethod = PostDecodeMethodType.pdmNone;
		m_foundfield = null;
		m_tagmethods = m_defaultTagMethods;
		if (m_extender != null)
		{
			m_extender(this);
		}
		SetField(TiffTag.COMPRESSION, Compression.NONE);
		m_flags &= ~TiffFlags.DIRTYDIRECT;
		m_flags &= ~TiffFlags.ISTILED;
		m_tilesize = -1;
		m_scanlinesize = -1;
	}

	private bool advanceDirectory(ref ulong nextdir, out long off)
	{
		off = 0L;
		if (!seekOK((long)nextdir) || !readDirCountOK(out var value, m_header.tiff_version == 43))
		{
			ErrorExt(this, m_clientdata, "advanceDirectory", "{0}: Error fetching directory count", m_name);
			return false;
		}
		if ((m_flags & TiffFlags.SWAB) == TiffFlags.SWAB)
		{
			SwabBigTiffValue(ref value, m_header.tiff_version == 43, isShort: true);
		}
		off = seekFile((long)value * (long)TiffDirEntry.SizeInBytes(m_header.tiff_version == 43), SeekOrigin.Current);
		if (m_header.tiff_version == 43)
		{
			if (!readUlongOK(out nextdir))
			{
				issueAdvanceDirectoryWarning("advanceDirectory");
			}
		}
		else
		{
			if (!readUIntOK(out var value2))
			{
				issueAdvanceDirectoryWarning("advanceDirectory");
			}
			nextdir = value2;
		}
		if ((m_flags & TiffFlags.SWAB) == TiffFlags.SWAB)
		{
			SwabBigTiffValue(ref nextdir, m_header.tiff_version == 43, isShort: false);
		}
		return true;
	}

	private void issueAdvanceDirectoryWarning(string module)
	{
		string format = "{0}: Error reading next directory offset. Treating as no next directory.";
		WarningExt(this, m_clientdata, module, format, m_name);
	}

	internal static void setString(out string cpp, string cp)
	{
		cpp = cp;
	}

	internal static void setShortArray(out short[] wpp, short[] wp, int n)
	{
		wpp = new short[n];
		for (int i = 0; i < n; i++)
		{
			wpp[i] = wp[i];
		}
	}

	internal static void setLongArray(out int[] lpp, int[] lp, int n)
	{
		lpp = new int[n];
		for (int i = 0; i < n; i++)
		{
			lpp[i] = lp[i];
		}
	}

	internal static void setLong8Array(out long[] lpp, long[] lp, int n)
	{
		lpp = new long[n];
		for (int i = 0; i < n; i++)
		{
			lpp[i] = lp[i];
		}
	}

	internal static void setFloatArray(out float[] fpp, float[] fp, int n)
	{
		fpp = new float[n];
		for (int i = 0; i < n; i++)
		{
			fpp[i] = fp[i];
		}
	}

	internal bool fieldSet(int field)
	{
		return (m_dir.td_fieldsset[field / 32] & BITn(field)) != 0;
	}

	internal void setFieldBit(int field)
	{
		m_dir.td_fieldsset[field / 32] |= BITn(field);
	}

	internal void clearFieldBit(int field)
	{
		m_dir.td_fieldsset[field / 32] &= ~BITn(field);
	}

	private static TiffFieldInfo[] getFieldInfo(out int size)
	{
		size = tiffFieldInfo.Length;
		return tiffFieldInfo;
	}

	private static TiffFieldInfo[] getExifFieldInfo(out int size)
	{
		size = exifFieldInfo.Length;
		return exifFieldInfo;
	}

	private void setupFieldInfo(TiffFieldInfo[] info, int n)
	{
		m_nfields = 0;
		MergeFieldInfo(info, n);
	}

	private TiffType sampleToTagType()
	{
		int num = howMany8(m_dir.td_bitspersample);
		switch (m_dir.td_sampleformat)
		{
		case SampleFormat.IEEEFP:
			if (num != 4)
			{
				return TiffType.DOUBLE;
			}
			return TiffType.FLOAT;
		case SampleFormat.INT:
			if (num > 1)
			{
				if (num > 2)
				{
					return TiffType.SLONG;
				}
				return TiffType.SSHORT;
			}
			return TiffType.SBYTE;
		case SampleFormat.UINT:
			if (num > 1)
			{
				if (num > 2)
				{
					return TiffType.LONG;
				}
				return TiffType.SHORT;
			}
			return TiffType.BYTE;
		case SampleFormat.VOID:
			return TiffType.UNDEFINED;
		default:
			return TiffType.UNDEFINED;
		}
	}

	private static TiffFieldInfo createAnonFieldInfo(TiffTag tag, TiffType field_type)
	{
		return new TiffFieldInfo(tag, -3, -3, field_type, 65, okToChange: true, passCount: true, null)
		{
			Name = string.Format(CultureInfo.InvariantCulture, "Tag {0}", tag)
		};
	}

	internal static int dataSize(TiffType type)
	{
		switch (type)
		{
		case TiffType.BYTE:
		case TiffType.ASCII:
		case TiffType.SBYTE:
		case TiffType.UNDEFINED:
			return 1;
		case TiffType.SHORT:
		case TiffType.SSHORT:
			return 2;
		case TiffType.LONG:
		case TiffType.RATIONAL:
		case TiffType.SLONG:
		case TiffType.SRATIONAL:
		case TiffType.FLOAT:
		case TiffType.IFD:
			return 4;
		case TiffType.DOUBLE:
		case TiffType.LONG8:
		case TiffType.SLONG8:
		case TiffType.IFD8:
			return 8;
		default:
			return 0;
		}
	}

	private long extractData(TiffDirEntry dir)
	{
		int tdir_type = (int)dir.tdir_type;
		if (m_header.tiff_magic == 19789)
		{
			return (long)((dir.tdir_offset >> m_typeshift[tdir_type]) & m_typemask[tdir_type]);
		}
		return (long)(dir.tdir_offset & m_typemask[tdir_type]);
	}

	private bool byteCountLooksBad(TiffDirectory td)
	{
		if ((td.td_stripbytecount[0] != 0L || td.td_stripoffset[0] == 0L) && (td.td_compression != Compression.NONE || td.td_stripbytecount[0] <= (ulong)(getFileSize() - (long)td.td_stripoffset[0])))
		{
			if (m_mode == 0 && td.td_compression == Compression.NONE)
			{
				return td.td_stripbytecount[0] < (ulong)(ScanlineSize() * td.td_imagelength);
			}
			return false;
		}
		return true;
	}

	private static int howMany8(int x)
	{
		if ((x & 7) == 0)
		{
			return x >> 3;
		}
		return (x >> 3) + 1;
	}

	private bool estimateStripByteCounts(TiffDirEntry[] dir, long dircount)
	{
		m_dir.td_stripbytecount = new ulong[m_dir.td_nstrips];
		if (m_dir.td_compression != Compression.NONE)
		{
			long fileSize = getFileSize();
			long num = ((m_header.tiff_version == 43) ? (TiffHeader.SizeInBytes(isBigTiff: true) + 8 + dircount * TiffDirEntry.SizeInBytes(isBigTiff: true) + 8) : (TiffHeader.SizeInBytes(isBigTiff: false) + 2 + dircount * TiffDirEntry.SizeInBytes(isBigTiff: false) + 4));
			for (short num2 = 0; num2 < dircount; num2++)
			{
				int num3 = DataWidth(dir[num2].tdir_type);
				if (num3 == 0)
				{
					ErrorExt(this, m_clientdata, "estimateStripByteCounts", "{0}: Cannot determine size of unknown tag type {1}", m_name, dir[num2].tdir_type);
					return false;
				}
				num3 *= dir[num2].tdir_count;
				if (num3 > 4)
				{
					num += num3;
				}
			}
			num = fileSize - num;
			if (m_dir.td_planarconfig == PlanarConfig.SEPARATE)
			{
				num /= m_dir.td_samplesperpixel;
			}
			int i;
			for (i = 0; i < m_dir.td_nstrips; i++)
			{
				m_dir.td_stripbytecount[i] = (uint)num;
			}
			i--;
			if (m_dir.td_stripoffset[i] + m_dir.td_stripbytecount[i] > (ulong)fileSize)
			{
				m_dir.td_stripbytecount[i] = (ulong)fileSize - m_dir.td_stripoffset[i];
			}
		}
		else if (IsTiled())
		{
			int num4 = TileSize();
			for (int j = 0; j < m_dir.td_nstrips; j++)
			{
				m_dir.td_stripbytecount[j] = (uint)num4;
			}
		}
		else
		{
			int num5 = ScanlineSize();
			int num6 = m_dir.td_imagelength / m_dir.td_stripsperimage;
			for (int k = 0; k < m_dir.td_nstrips; k++)
			{
				m_dir.td_stripbytecount[k] = (uint)(num5 * num6);
			}
		}
		setFieldBit(24);
		if (!fieldSet(17))
		{
			m_dir.td_rowsperstrip = m_dir.td_imagelength;
		}
		return true;
	}

	private void missingRequired(string tagname)
	{
		ErrorExt(this, m_clientdata, "missingRequired", "{0}: TIFF directory is missing required \"{1}\" field", m_name, tagname);
	}

	private int fetchFailed(TiffDirEntry dir)
	{
		string format = "Error fetching data for field \"{0}\"";
		TiffFieldInfo tiffFieldInfo = FieldWithTag(dir.tdir_tag);
		if (tiffFieldInfo.Bit == 65)
		{
			WarningExt(this, m_clientdata, m_name, format, tiffFieldInfo.Name);
		}
		else
		{
			ErrorExt(this, m_clientdata, m_name, format, tiffFieldInfo.Name);
		}
		return 0;
	}

	private static long readDirectoryFind(TiffDirEntry[] dir, ulong dircount, TiffTag tagid)
	{
		for (ulong num = 0uL; num < dircount; num++)
		{
			if (dir[num].tdir_tag == tagid)
			{
				return (long)num;
			}
		}
		return -1L;
	}

	private bool checkDirOffset(ulong diroff)
	{
		if (diroff == 0L)
		{
			return false;
		}
		short num = 0;
		while (num < m_dirnumber && m_dirlist != null)
		{
			if (m_dirlist[num] == diroff)
			{
				return false;
			}
			num++;
		}
		m_dirnumber++;
		if (m_dirnumber > m_dirlistsize)
		{
			ulong[] dirlist = Realloc(m_dirlist, m_dirnumber - 1, 2 * m_dirnumber);
			m_dirlistsize = 2 * m_dirnumber;
			m_dirlist = dirlist;
		}
		m_dirlist[m_dirnumber - 1] = diroff;
		return true;
	}

	private ulong fetchDirectory(ulong diroff, out TiffDirEntry[] pdir, out ulong nextdiroff)
	{
		m_diroff = diroff;
		nextdiroff = 0uL;
		TiffDirEntry[] array = null;
		pdir = null;
		if (!seekOK((long)m_diroff))
		{
			ErrorExt(this, m_clientdata, "fetchDirectory", "{0}: Seek error accessing TIFF directory", m_name);
			return 0uL;
		}
		if (!readDirCountOK(out var dircount, m_header.tiff_version == 43))
		{
			ErrorExt(this, m_clientdata, "fetchDirectory", "{0}: Can not read TIFF directory count", m_name);
			return 0uL;
		}
		if ((m_flags & TiffFlags.SWAB) == TiffFlags.SWAB)
		{
			SwabBigTiffValue(ref dircount, m_header.tiff_version == 43, isShort: true);
		}
		array = new TiffDirEntry[dircount];
		if (!readDirEntryOk(array, dircount, m_header.tiff_version == 43))
		{
			ErrorExt(this, m_clientdata, "fetchDirectory", "{0}: Can not read TIFF directory", m_name);
			return 0uL;
		}
		if (m_header.tiff_version == 43)
		{
			readUlongOK(out var value);
			nextdiroff = value;
		}
		else
		{
			int value2 = 0;
			readIntOK(out value2);
			nextdiroff = (ulong)value2;
		}
		if ((m_flags & TiffFlags.SWAB) == TiffFlags.SWAB)
		{
			ulong value = nextdiroff;
			SwabBigTiffValue(ref value, m_header.tiff_version == 43, isShort: false);
			nextdiroff = value;
		}
		pdir = array;
		return dircount;
	}

	private bool fetchSubjectDistance(TiffDirEntry dir)
	{
		if (dir.tdir_count != 1 || dir.tdir_type != TiffType.RATIONAL)
		{
			WarningExt(this, m_clientdata, m_name, "incorrect count or type for SubjectDistance, tag ignored");
			return false;
		}
		bool result = false;
		byte[] buffer = new byte[8];
		if (fetchData(dir, buffer) != 0)
		{
			int[] array = new int[2]
			{
				readInt(buffer, 0),
				readInt(buffer, 4)
			};
			if (cvtRational(dir, array[0], array[1], out var rv))
			{
				result = SetField(dir.tdir_tag, (array[0] != -1) ? rv : (0f - rv));
			}
		}
		return result;
	}

	private bool checkDirCount(TiffDirEntry dir, int count)
	{
		if (count > dir.tdir_count)
		{
			WarningExt(this, m_clientdata, m_name, "incorrect count for field \"{0}\" ({1}, expecting {2}); tag ignored", FieldWithTag(dir.tdir_tag).Name, dir.tdir_count, count);
			return false;
		}
		if (count < dir.tdir_count)
		{
			WarningExt(this, m_clientdata, m_name, "incorrect count for field \"{0}\" ({1}, expecting {2}); tag trimmed", FieldWithTag(dir.tdir_tag).Name, dir.tdir_count, count);
			dir.tdir_count = count;
			return true;
		}
		return true;
	}

	private int fetchData(TiffDirEntry dir, byte[] buffer)
	{
		int num = DataWidth(dir.tdir_type);
		int num2 = dir.tdir_count * num;
		if (dir.tdir_count == 0 || num == 0 || num2 / num != dir.tdir_count)
		{
			fetchFailed(dir);
		}
		if (!seekOK((long)dir.tdir_offset))
		{
			fetchFailed(dir);
		}
		if (!readOK(buffer, num2))
		{
			fetchFailed(dir);
		}
		if ((m_flags & TiffFlags.SWAB) == TiffFlags.SWAB)
		{
			switch (dir.tdir_type)
			{
			case TiffType.SHORT:
			case TiffType.SSHORT:
			{
				short[] array4 = ByteArrayToShorts(buffer, 0, num2);
				SwabArrayOfShort(array4, dir.tdir_count);
				ShortsToByteArray(array4, 0, dir.tdir_count, buffer, 0);
				break;
			}
			case TiffType.LONG:
			case TiffType.SLONG:
			case TiffType.FLOAT:
			case TiffType.IFD:
			{
				int[] array3 = ByteArrayToInts(buffer, 0, num2);
				SwabArrayOfLong(array3, dir.tdir_count);
				IntsToByteArray(array3, 0, dir.tdir_count, buffer, 0);
				break;
			}
			case TiffType.LONG8:
			case TiffType.SLONG8:
			case TiffType.IFD8:
			{
				long[] array2 = ByteArrayToLong8(buffer, 0, num2);
				SwabArrayOfLong8(array2, 2 * dir.tdir_count);
				Long8ToByteArray(array2, 0, 2 * dir.tdir_count, buffer, 0);
				break;
			}
			case TiffType.RATIONAL:
			case TiffType.SRATIONAL:
			{
				int[] array = ByteArrayToInts(buffer, 0, num2);
				SwabArrayOfLong(array, 2 * dir.tdir_count);
				IntsToByteArray(array, 0, 2 * dir.tdir_count, buffer, 0);
				break;
			}
			case TiffType.DOUBLE:
				swab64BitData(buffer, 0, num2);
				break;
			}
		}
		return num2;
	}

	private int fetchString(TiffDirEntry dir, out string cp)
	{
		byte[] array = null;
		if (m_header.tiff_version != 43 && dir.tdir_count <= 4)
		{
			int value = (int)dir.tdir_offset;
			if ((m_flags & TiffFlags.SWAB) == TiffFlags.SWAB)
			{
				SwabLong(ref value);
			}
			array = new byte[4];
			writeInt(value, array, 0);
			cp = Latin1Encoding.GetString(array, 0, dir.tdir_count);
			return 1;
		}
		if (m_header.tiff_version == 43 && dir.tdir_count <= 8)
		{
			ulong value2 = dir.tdir_offset;
			if ((m_flags & TiffFlags.SWAB) == TiffFlags.SWAB)
			{
				SwabLong8(ref value2);
			}
			array = new byte[8];
			writeULong(value2, array, 0);
			cp = Latin1Encoding.GetString(array, 0, dir.tdir_count);
			return 1;
		}
		array = new byte[dir.tdir_count];
		int result = fetchData(dir, array);
		cp = Latin1Encoding.GetString(array, 0, dir.tdir_count);
		return result;
	}

	private bool cvtRational(TiffDirEntry dir, int num, int denom, out float rv)
	{
		if (denom == 0)
		{
			ErrorExt(this, m_clientdata, m_name, "{0}: Rational with zero denominator (num = {1})", FieldWithTag(dir.tdir_tag).Name, num);
			rv = float.NaN;
			return false;
		}
		rv = (float)num / (float)denom;
		return true;
	}

	private bool cvtRational(TiffDirEntry dir, uint num, uint denom, out float rv)
	{
		if (denom == 0)
		{
			ErrorExt(this, m_clientdata, m_name, "{0}: Rational with zero denominator (num = {1})", FieldWithTag(dir.tdir_tag).Name, num);
			rv = float.NaN;
			return false;
		}
		rv = (float)num / (float)denom;
		return true;
	}

	private float fetchRational(TiffDirEntry dir)
	{
		if (m_header.tiff_version == 43)
		{
			uint[] array = new uint[2];
			int tdir_count = dir.tdir_count;
			dir.tdir_count = 2;
			if (fetchULongArray(dir, array))
			{
				dir.tdir_count = tdir_count;
				if (cvtRational(dir, array[0], array[1], out var rv))
				{
					return rv;
				}
			}
		}
		else
		{
			byte[] array2 = new byte[8];
			if (fetchData(dir, array2) != 0)
			{
				if (dir.tdir_type == TiffType.SRATIONAL)
				{
					int[] array3 = new int[2]
					{
						readInt(array2, 0),
						readInt(array2, 4)
					};
					if (cvtRational(dir, array3[0], array3[1], out var rv2))
					{
						return rv2;
					}
				}
				else
				{
					uint[] array4 = new uint[2]
					{
						BitConverter.ToUInt32(array2, 0),
						BitConverter.ToUInt32(array2, 4)
					};
					if (cvtRational(dir, array4[0], array4[1], out var rv3))
					{
						return rv3;
					}
				}
			}
		}
		return 1f;
	}

	private float fetchFloat(TiffDirEntry dir)
	{
		return BitConverter.ToSingle(BitConverter.GetBytes((int)extractData(dir)), 0);
	}

	private bool fetchByteArray(TiffDirEntry dir, byte[] v)
	{
		if (m_header.tiff_version != 43 && dir.tdir_count <= 4)
		{
			int tdir_count = dir.tdir_count;
			if (m_header.tiff_magic == 19789)
			{
				if (tdir_count == 4)
				{
					v[3] = (byte)(dir.tdir_offset & 0xFF);
				}
				if (tdir_count >= 3)
				{
					v[2] = (byte)((dir.tdir_offset >> 8) & 0xFF);
				}
				if (tdir_count >= 2)
				{
					v[1] = (byte)((dir.tdir_offset >> 16) & 0xFF);
				}
				if (tdir_count >= 1)
				{
					v[0] = (byte)(dir.tdir_offset >> 24);
				}
			}
			else
			{
				if (tdir_count == 4)
				{
					v[3] = (byte)(dir.tdir_offset >> 24);
				}
				if (tdir_count >= 3)
				{
					v[2] = (byte)((dir.tdir_offset >> 16) & 0xFF);
				}
				if (tdir_count >= 2)
				{
					v[1] = (byte)((dir.tdir_offset >> 8) & 0xFF);
				}
				if (tdir_count >= 1)
				{
					v[0] = (byte)(dir.tdir_offset & 0xFF);
				}
			}
			return true;
		}
		if (m_header.tiff_version == 43 && dir.tdir_count <= 8)
		{
			int tdir_count2 = dir.tdir_count;
			if (m_header.tiff_magic == 19789)
			{
				if (tdir_count2 == 8)
				{
					v[7] = (byte)(dir.tdir_offset & 0xFF);
				}
				if (tdir_count2 >= 7)
				{
					v[6] = (byte)((dir.tdir_offset >> 8) & 0xFF);
				}
				if (tdir_count2 >= 6)
				{
					v[5] = (byte)((dir.tdir_offset >> 16) & 0xFF);
				}
				if (tdir_count2 >= 5)
				{
					v[4] = (byte)((dir.tdir_offset >> 24) & 0xFF);
				}
				if (tdir_count2 >= 4)
				{
					v[3] = (byte)((dir.tdir_offset >> 32) & 0xFF);
				}
				if (tdir_count2 >= 3)
				{
					v[2] = (byte)((dir.tdir_offset >> 40) & 0xFF);
				}
				if (tdir_count2 >= 2)
				{
					v[1] = (byte)((dir.tdir_offset >> 48) & 0xFF);
				}
				if (tdir_count2 >= 1)
				{
					v[0] = (byte)(dir.tdir_offset >> 56);
				}
			}
			else
			{
				if (tdir_count2 == 8)
				{
					v[7] = (byte)(dir.tdir_offset >> 56);
				}
				if (tdir_count2 >= 7)
				{
					v[6] = (byte)((dir.tdir_offset >> 48) & 0xFF);
				}
				if (tdir_count2 >= 6)
				{
					v[5] = (byte)((dir.tdir_offset >> 40) & 0xFF);
				}
				if (tdir_count2 >= 5)
				{
					v[4] = (byte)((dir.tdir_offset >> 32) & 0xFF);
				}
				if (tdir_count2 >= 4)
				{
					v[3] = (byte)((dir.tdir_offset >> 24) & 0xFF);
				}
				if (tdir_count2 >= 3)
				{
					v[2] = (byte)((dir.tdir_offset >> 16) & 0xFF);
				}
				if (tdir_count2 >= 2)
				{
					v[1] = (byte)((dir.tdir_offset >> 8) & 0xFF);
				}
				if (tdir_count2 >= 1)
				{
					v[0] = (byte)(dir.tdir_offset & 0xFF);
				}
			}
			return true;
		}
		return fetchData(dir, v) != 0;
	}

	private bool fetchShortArray(TiffDirEntry dir, short[] v)
	{
		if (m_header.tiff_version != 43 && dir.tdir_count <= 2)
		{
			int tdir_count = dir.tdir_count;
			if (m_header.tiff_magic == 19789)
			{
				if (tdir_count == 2)
				{
					v[1] = (short)(dir.tdir_offset & 0xFFFF);
				}
				if (tdir_count >= 1)
				{
					v[0] = (short)(dir.tdir_offset >> 16);
				}
			}
			else
			{
				if (tdir_count == 2)
				{
					v[1] = (short)(dir.tdir_offset >> 16);
				}
				if (tdir_count >= 1)
				{
					v[0] = (short)(dir.tdir_offset & 0xFFFF);
				}
			}
			return true;
		}
		if (m_header.tiff_version == 43 && dir.tdir_count <= 4)
		{
			int tdir_count2 = dir.tdir_count;
			if (m_header.tiff_magic == 19789)
			{
				if (tdir_count2 == 4)
				{
					v[3] = (short)(dir.tdir_offset & 0xFFFF);
				}
				if (tdir_count2 >= 3)
				{
					v[2] = (short)(dir.tdir_offset >> 48);
				}
				if (tdir_count2 >= 2)
				{
					v[1] = (short)(dir.tdir_offset >> 32);
				}
				if (tdir_count2 >= 1)
				{
					v[0] = (short)(dir.tdir_offset >> 16);
				}
			}
			else
			{
				if (tdir_count2 == 4)
				{
					v[3] = (short)(dir.tdir_offset >> 48);
				}
				if (tdir_count2 >= 3)
				{
					v[2] = (short)(dir.tdir_offset >> 32);
				}
				if (tdir_count2 >= 2)
				{
					v[1] = (short)(dir.tdir_offset >> 16);
				}
				if (tdir_count2 >= 1)
				{
					v[0] = (short)(dir.tdir_offset & 0xFFFF);
				}
			}
			return true;
		}
		byte[] array = new byte[dir.tdir_count * 2];
		int num = fetchData(dir, array);
		if (num != 0)
		{
			Buffer.BlockCopy(array, 0, v, 0, array.Length);
		}
		return num != 0;
	}

	private bool fetchShortPair(TiffDirEntry dir)
	{
		if (dir.tdir_count > 2)
		{
			WarningExt(this, m_clientdata, m_name, "unexpected count for field \"{0}\", {1}, expected 2; ignored", FieldWithTag(dir.tdir_tag).Name, dir.tdir_count);
			return false;
		}
		switch (dir.tdir_type)
		{
		case TiffType.BYTE:
		case TiffType.SBYTE:
		{
			byte[] array2 = new byte[4];
			if (fetchByteArray(dir, array2))
			{
				return SetField(dir.tdir_tag, array2[0], array2[1]);
			}
			return false;
		}
		case TiffType.SHORT:
		case TiffType.SSHORT:
		{
			short[] array = new short[2];
			if (fetchShortArray(dir, array))
			{
				return SetField(dir.tdir_tag, array[0], array[1]);
			}
			return false;
		}
		default:
			return false;
		}
	}

	private bool fetchULongArray(TiffDirEntry dir, uint[] v)
	{
		if (m_header.tiff_version != 43 && dir.tdir_count == 1)
		{
			v[0] = (uint)dir.tdir_offset;
			return true;
		}
		if (m_header.tiff_version == 43 && dir.tdir_count <= 2)
		{
			int tdir_count = dir.tdir_count;
			if (m_header.tiff_magic == 19789)
			{
				if (tdir_count == 2)
				{
					v[1] = (uint)(dir.tdir_offset & 0xFFFFFFFFu);
				}
				if (tdir_count >= 1)
				{
					v[0] = (uint)(dir.tdir_offset >> 32);
				}
			}
			else
			{
				if (tdir_count == 2)
				{
					v[1] = (uint)(dir.tdir_offset >> 32);
				}
				if (tdir_count >= 1)
				{
					v[0] = (uint)(dir.tdir_offset & 0xFFFFFFFFu);
				}
			}
			return true;
		}
		byte[] array = new byte[dir.tdir_count * 4];
		int num = fetchData(dir, array);
		if (num != 0)
		{
			Buffer.BlockCopy(array, 0, v, 0, array.Length);
		}
		return num != 0;
	}

	private bool fetchLongArray(TiffDirEntry dir, int[] v)
	{
		if (m_header.tiff_version != 43 && dir.tdir_count == 1)
		{
			v[0] = (int)dir.tdir_offset;
			return true;
		}
		if (m_header.tiff_version == 43 && dir.tdir_count <= 2)
		{
			int tdir_count = dir.tdir_count;
			if (m_header.tiff_magic == 19789)
			{
				if (tdir_count == 2)
				{
					v[1] = (int)(dir.tdir_offset & 0xFFFFFFFFu);
				}
				if (tdir_count >= 1)
				{
					v[0] = (int)(dir.tdir_offset >> 32);
				}
			}
			else
			{
				if (tdir_count == 2)
				{
					v[1] = (int)(dir.tdir_offset >> 32);
				}
				if (tdir_count >= 1)
				{
					v[0] = (int)(dir.tdir_offset & 0xFFFFFFFFu);
				}
			}
			return true;
		}
		byte[] array = new byte[dir.tdir_count * 4];
		int num = fetchData(dir, array);
		if (num != 0)
		{
			Buffer.BlockCopy(array, 0, v, 0, array.Length);
		}
		return num != 0;
	}

	private bool fetchLong8Array(TiffDirEntry dir, long[] v)
	{
		if (dir.tdir_count == 1)
		{
			v[0] = (long)dir.tdir_offset;
			return true;
		}
		byte[] array = new byte[dir.tdir_count * 8];
		int num = fetchData(dir, array);
		if (num != 0)
		{
			Buffer.BlockCopy(array, 0, v, 0, array.Length);
		}
		return num != 0;
	}

	private bool fetchRationalArray(TiffDirEntry dir, float[] v)
	{
		bool flag = false;
		byte[] buffer = new byte[dir.tdir_count * DataWidth(dir.tdir_type)];
		if (fetchData(dir, buffer) != 0)
		{
			int num = 0;
			int[] array = new int[2];
			for (int i = 0; i < dir.tdir_count; i++)
			{
				array[0] = readInt(buffer, num);
				num += 4;
				array[1] = readInt(buffer, num);
				num += 4;
				flag = ((dir.tdir_type != TiffType.SRATIONAL) ? cvtRational(dir, (uint)array[0], (uint)array[1], out v[i]) : cvtRational(dir, array[0], array[1], out v[i]));
				if (!flag)
				{
					break;
				}
			}
		}
		return flag;
	}

	private bool fetchFloatArray(TiffDirEntry dir, float[] v)
	{
		if (m_header.tiff_version != 43 && dir.tdir_count == 1)
		{
			v[0] = BitConverter.ToSingle(BitConverter.GetBytes(dir.tdir_offset), 0);
			return true;
		}
		if (m_header.tiff_version == 43 && dir.tdir_count <= 2)
		{
			int tdir_count = dir.tdir_count;
			if (m_header.tiff_magic == 19789)
			{
				if (tdir_count == 2)
				{
					v[1] = BitConverter.ToSingle(BitConverter.GetBytes(dir.tdir_offset), 4);
				}
				if (tdir_count >= 1)
				{
					v[0] = BitConverter.ToSingle(BitConverter.GetBytes(dir.tdir_offset), 0);
				}
			}
			else
			{
				if (tdir_count == 2)
				{
					v[1] = BitConverter.ToSingle(BitConverter.GetBytes(dir.tdir_offset), 0);
				}
				if (tdir_count >= 1)
				{
					v[0] = BitConverter.ToSingle(BitConverter.GetBytes(dir.tdir_offset), 4);
				}
			}
			return true;
		}
		int num = DataWidth(dir.tdir_type);
		byte[] array = new byte[dir.tdir_count * num];
		int num2 = fetchData(dir, array);
		if (num2 != 0)
		{
			int num3 = 0;
			for (int i = 0; i < num2 / 4; i++)
			{
				v[i] = BitConverter.ToSingle(array, num3);
				num3 += 4;
			}
		}
		return num2 != 0;
	}

	private bool fetchDoubleArray(TiffDirEntry dir, double[] v)
	{
		if (m_header.tiff_version == 43 && dir.tdir_count == 1)
		{
			v[0] = dir.tdir_offset;
			return true;
		}
		int num = DataWidth(dir.tdir_type);
		byte[] array = new byte[dir.tdir_count * num];
		int num2 = fetchData(dir, array);
		if (num2 != 0)
		{
			int num3 = 0;
			for (int i = 0; i < num2 / 8; i++)
			{
				v[i] = BitConverter.ToDouble(array, num3);
				num3 += 8;
			}
		}
		return num2 != 0;
	}

	private bool fetchAnyArray(TiffDirEntry dir, double[] v)
	{
		int num = 0;
		bool flag = false;
		switch (dir.tdir_type)
		{
		case TiffType.BYTE:
		case TiffType.SBYTE:
		{
			byte[] array3 = new byte[dir.tdir_count];
			flag = fetchByteArray(dir, array3);
			if (flag)
			{
				for (num = dir.tdir_count - 1; num >= 0; num--)
				{
					v[num] = (int)array3[num];
				}
			}
			if (!flag)
			{
				return false;
			}
			break;
		}
		case TiffType.SHORT:
		case TiffType.SSHORT:
		{
			short[] array5 = new short[dir.tdir_count];
			flag = fetchShortArray(dir, array5);
			if (flag)
			{
				for (num = dir.tdir_count - 1; num >= 0; num--)
				{
					v[num] = array5[num];
				}
			}
			if (!flag)
			{
				return false;
			}
			break;
		}
		case TiffType.LONG:
		case TiffType.SLONG:
		{
			int[] array2 = new int[dir.tdir_count];
			flag = fetchLongArray(dir, array2);
			if (flag)
			{
				for (num = dir.tdir_count - 1; num >= 0; num--)
				{
					v[num] = array2[num];
				}
			}
			if (!flag)
			{
				return false;
			}
			break;
		}
		case TiffType.RATIONAL:
		case TiffType.SRATIONAL:
		{
			float[] array4 = new float[dir.tdir_count];
			flag = fetchRationalArray(dir, array4);
			if (flag)
			{
				for (num = dir.tdir_count - 1; num >= 0; num--)
				{
					v[num] = array4[num];
				}
			}
			if (!flag)
			{
				return false;
			}
			break;
		}
		case TiffType.FLOAT:
		{
			float[] array = new float[dir.tdir_count];
			flag = fetchFloatArray(dir, array);
			if (flag)
			{
				for (num = dir.tdir_count - 1; num >= 0; num--)
				{
					v[num] = array[num];
				}
			}
			if (!flag)
			{
				return false;
			}
			break;
		}
		case TiffType.DOUBLE:
			return fetchDoubleArray(dir, v);
		default:
			ErrorExt(this, m_clientdata, m_name, "cannot read TIFF_ANY type {0} for field \"{1}\"", dir.tdir_type, FieldWithTag(dir.tdir_tag).Name);
			return false;
		}
		return true;
	}

	private bool fetchNormalTag(TiffDirEntry dir)
	{
		bool flag = false;
		TiffFieldInfo tiffFieldInfo = FieldWithTag(dir.tdir_tag);
		if (dir.tdir_count > 1)
		{
			switch (dir.tdir_type)
			{
			case TiffType.BYTE:
			case TiffType.SBYTE:
			{
				byte[] array2 = new byte[dir.tdir_count];
				flag = fetchByteArray(dir, array2);
				if (flag)
				{
					flag = ((!tiffFieldInfo.PassCount) ? SetField(dir.tdir_tag, array2) : SetField(dir.tdir_tag, dir.tdir_count, array2));
				}
				break;
			}
			case TiffType.SHORT:
			case TiffType.SSHORT:
			{
				short[] array3 = new short[dir.tdir_count];
				flag = fetchShortArray(dir, array3);
				if (flag)
				{
					flag = ((!tiffFieldInfo.PassCount) ? SetField(dir.tdir_tag, array3) : SetField(dir.tdir_tag, dir.tdir_count, array3));
				}
				break;
			}
			case TiffType.LONG:
			case TiffType.SLONG:
			{
				int[] array7 = new int[dir.tdir_count];
				flag = fetchLongArray(dir, array7);
				if (flag)
				{
					flag = ((!tiffFieldInfo.PassCount) ? SetField(dir.tdir_tag, array7) : SetField(dir.tdir_tag, dir.tdir_count, array7));
				}
				break;
			}
			case TiffType.LONG8:
			case TiffType.SLONG8:
			case TiffType.IFD8:
			{
				long[] array5 = new long[dir.tdir_count];
				flag = fetchLong8Array(dir, array5);
				if (flag)
				{
					flag = ((!tiffFieldInfo.PassCount) ? SetField(dir.tdir_tag, array5) : SetField(dir.tdir_tag, dir.tdir_count, array5));
				}
				break;
			}
			case TiffType.RATIONAL:
			case TiffType.SRATIONAL:
			{
				float[] array6 = new float[dir.tdir_count];
				flag = fetchRationalArray(dir, array6);
				if (flag)
				{
					flag = ((!tiffFieldInfo.PassCount) ? SetField(dir.tdir_tag, array6) : SetField(dir.tdir_tag, dir.tdir_count, array6));
				}
				break;
			}
			case TiffType.FLOAT:
			{
				float[] array = new float[dir.tdir_count];
				flag = fetchFloatArray(dir, array);
				if (flag)
				{
					flag = ((!tiffFieldInfo.PassCount) ? SetField(dir.tdir_tag, array) : SetField(dir.tdir_tag, dir.tdir_count, array));
				}
				break;
			}
			case TiffType.DOUBLE:
			{
				double[] array4 = new double[dir.tdir_count];
				flag = fetchDoubleArray(dir, array4);
				if (flag)
				{
					flag = ((!tiffFieldInfo.PassCount) ? SetField(dir.tdir_tag, array4) : SetField(dir.tdir_tag, dir.tdir_count, array4));
				}
				break;
			}
			case TiffType.ASCII:
			case TiffType.UNDEFINED:
			{
				flag = fetchString(dir, out var cp) != 0;
				if (flag)
				{
					flag = ((!tiffFieldInfo.PassCount) ? SetField(dir.tdir_tag, cp) : SetField(dir.tdir_tag, dir.tdir_count, cp));
				}
				break;
			}
			}
		}
		else if (checkDirCount(dir, 1))
		{
			int num = 0;
			long num2 = 0L;
			switch (dir.tdir_type)
			{
			case TiffType.BYTE:
			case TiffType.SHORT:
			case TiffType.SBYTE:
			case TiffType.SSHORT:
			{
				TiffType type = tiffFieldInfo.Type;
				if (type != TiffType.LONG && type != TiffType.SLONG)
				{
					short num4 = (short)extractData(dir);
					if (tiffFieldInfo.PassCount)
					{
						short[] array12 = new short[1] { num4 };
						flag = SetField(dir.tdir_tag, 1, array12);
					}
					else
					{
						flag = SetField(dir.tdir_tag, num4);
					}
				}
				else
				{
					num = (int)extractData(dir);
					if (tiffFieldInfo.PassCount)
					{
						int[] array13 = new int[1] { num };
						flag = SetField(dir.tdir_tag, 1, array13);
					}
					else
					{
						flag = SetField(dir.tdir_tag, num);
					}
				}
				break;
			}
			case TiffType.LONG:
			case TiffType.SLONG:
			case TiffType.IFD:
				num = (int)extractData(dir);
				if (tiffFieldInfo.PassCount)
				{
					int[] array9 = new int[1] { num };
					flag = SetField(dir.tdir_tag, 1, array9);
				}
				else
				{
					flag = SetField(dir.tdir_tag, num);
				}
				break;
			case TiffType.LONG8:
			case TiffType.SLONG8:
			case TiffType.IFD8:
				num2 = extractData(dir);
				if (tiffFieldInfo.PassCount)
				{
					long[] array8 = new long[1] { num2 };
					flag = SetField(dir.tdir_tag, 1, array8);
				}
				else
				{
					flag = SetField(dir.tdir_tag, num2);
				}
				break;
			case TiffType.RATIONAL:
			case TiffType.SRATIONAL:
			case TiffType.FLOAT:
			{
				float num3 = ((dir.tdir_type == TiffType.FLOAT) ? fetchFloat(dir) : fetchRational(dir));
				if (tiffFieldInfo.PassCount)
				{
					float[] array10 = new float[1] { num3 };
					flag = SetField(dir.tdir_tag, 1, array10);
				}
				else
				{
					flag = SetField(dir.tdir_tag, num3);
				}
				break;
			}
			case TiffType.DOUBLE:
			{
				double[] array11 = new double[1];
				flag = fetchDoubleArray(dir, array11);
				if (flag)
				{
					flag = ((!tiffFieldInfo.PassCount) ? SetField(dir.tdir_tag, array11[0]) : SetField(dir.tdir_tag, 1, array11));
				}
				break;
			}
			case TiffType.ASCII:
			case TiffType.UNDEFINED:
			{
				flag = fetchString(dir, out var cp2) != 0;
				if (flag)
				{
					flag = ((!tiffFieldInfo.PassCount) ? SetField(dir.tdir_tag, cp2) : SetField(dir.tdir_tag, 1, cp2));
				}
				break;
			}
			}
		}
		return flag;
	}

	private bool fetchPerSampleShorts(TiffDirEntry dir, out short pl)
	{
		pl = 0;
		short td_samplesperpixel = m_dir.td_samplesperpixel;
		bool result = false;
		if (checkDirCount(dir, td_samplesperpixel))
		{
			short[] array = new short[dir.tdir_count];
			if (fetchShortArray(dir, array))
			{
				int num = dir.tdir_count;
				if (td_samplesperpixel < num)
				{
					num = td_samplesperpixel;
				}
				bool flag = false;
				for (ushort num2 = 1; num2 < num; num2++)
				{
					if (array[num2] != array[0])
					{
						ErrorExt(this, m_clientdata, m_name, "Cannot handle different per-sample values for field \"{0}\"", FieldWithTag(dir.tdir_tag).Name);
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					pl = array[0];
					result = true;
				}
			}
		}
		return result;
	}

	private bool fetchPerSampleLongs(TiffDirEntry dir, out int pl)
	{
		pl = 0;
		short td_samplesperpixel = m_dir.td_samplesperpixel;
		bool result = false;
		if (checkDirCount(dir, td_samplesperpixel))
		{
			int[] array = new int[dir.tdir_count];
			if (fetchLongArray(dir, array))
			{
				int num = dir.tdir_count;
				if (td_samplesperpixel < num)
				{
					num = td_samplesperpixel;
				}
				bool flag = false;
				for (ushort num2 = 1; num2 < num; num2++)
				{
					if (array[num2] != array[0])
					{
						ErrorExt(this, m_clientdata, m_name, "Cannot handle different per-sample values for field \"{0}\"", FieldWithTag(dir.tdir_tag).Name);
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					pl = array[0];
					result = true;
				}
			}
		}
		return result;
	}

	private bool fetchPerSampleAnys(TiffDirEntry dir, out double pl)
	{
		pl = 0.0;
		short td_samplesperpixel = m_dir.td_samplesperpixel;
		bool result = false;
		if (checkDirCount(dir, td_samplesperpixel))
		{
			double[] array = new double[dir.tdir_count];
			if (fetchAnyArray(dir, array))
			{
				int num = dir.tdir_count;
				if (td_samplesperpixel < num)
				{
					num = td_samplesperpixel;
				}
				bool flag = false;
				for (ushort num2 = 1; num2 < num; num2++)
				{
					if (array[num2] != array[0])
					{
						ErrorExt(this, m_clientdata, m_name, "Cannot handle different per-sample values for field \"{0}\"", FieldWithTag(dir.tdir_tag).Name);
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					pl = array[0];
					result = true;
				}
			}
		}
		return result;
	}

	private bool fetchStripThing(TiffDirEntry dir, int nstrips, ref long[] lpp)
	{
		checkDirCount(dir, nstrips);
		if (lpp == null)
		{
			lpp = new long[nstrips];
		}
		else
		{
			Array.Clear(lpp, 0, lpp.Length);
		}
		bool flag = false;
		if (dir.tdir_type == TiffType.SHORT)
		{
			short[] array = new short[dir.tdir_count];
			flag = fetchShortArray(dir, array);
			if (flag)
			{
				for (int i = 0; i < nstrips && i < dir.tdir_count; i++)
				{
					lpp[i] = (ushort)array[i];
				}
			}
		}
		else if (nstrips != dir.tdir_count)
		{
			int[] array2 = new int[dir.tdir_count];
			flag = fetchLongArray(dir, array2);
			if (flag)
			{
				for (int j = 0; j < nstrips && j < dir.tdir_count; j++)
				{
					lpp[j] = (uint)array2[j];
				}
			}
		}
		else if (dir.tdir_type == TiffType.LONG8)
		{
			flag = fetchLong8Array(dir, lpp);
		}
		else
		{
			int[] array3 = new int[lpp.Length];
			flag = fetchLongArray(dir, array3);
			lpp = IntToLong(array3);
		}
		return flag;
	}

	private bool fetchStripThing(TiffDirEntry dir, int nstrips, ref ulong[] lpp)
	{
		long[] lpp2 = null;
		if (lpp != null)
		{
			lpp2 = new long[lpp.Length];
		}
		bool num = fetchStripThing(dir, nstrips, ref lpp2);
		if (num)
		{
			if (lpp == null)
			{
				lpp = new ulong[lpp2.Length];
			}
			Buffer.BlockCopy(lpp2, 0, lpp, 0, lpp2.Length * 8);
		}
		return num;
	}

	private bool fetchRefBlackWhite(TiffDirEntry dir)
	{
		if (dir.tdir_type == TiffType.RATIONAL && fetchNormalTag(dir))
		{
			for (int i = 0; i < m_dir.td_refblackwhite.Length; i++)
			{
				if (m_dir.td_refblackwhite[i] > 1f)
				{
					return true;
				}
			}
		}
		dir.tdir_type = TiffType.LONG;
		int[] array = new int[dir.tdir_count];
		bool flag = fetchLongArray(dir, array);
		dir.tdir_type = TiffType.RATIONAL;
		if (flag)
		{
			float[] array2 = new float[dir.tdir_count];
			for (int j = 0; j < dir.tdir_count; j++)
			{
				array2[j] = array[j];
			}
			flag = SetField(dir.tdir_tag, array2);
		}
		return flag;
	}

	private void chopUpSingleUncompressedStrip()
	{
		ulong num = m_dir.td_stripbytecount[0];
		ulong num2 = m_dir.td_stripoffset[0];
		int num3 = VTileSize(1);
		ulong num4;
		int num5;
		if (num3 > 8192)
		{
			num4 = (ulong)num3;
			num5 = 1;
		}
		else
		{
			if (num3 <= 0)
			{
				return;
			}
			num5 = 8192 / num3;
			num4 = (ulong)(num3 * num5);
		}
		if (num5 >= m_dir.td_rowsperstrip)
		{
			return;
		}
		ulong num6 = howMany(num, num4);
		if (num6 == 0L)
		{
			return;
		}
		ulong[] array = new ulong[num6];
		ulong[] array2 = new ulong[num6];
		for (ulong num7 = 0uL; num7 < num6; num7++)
		{
			if (num4 > num)
			{
				num4 = num;
			}
			array[num7] = num4;
			array2[num7] = num2;
			num2 += num4;
			num -= num4;
		}
		m_dir.td_nstrips = (int)num6;
		m_dir.td_stripsperimage = (int)num6;
		SetField(TiffTag.ROWSPERSTRIP, num5);
		m_dir.td_stripbytecount = array;
		m_dir.td_stripoffset = array2;
		m_dir.td_stripbytecountsorted = true;
	}

	internal static int roundUp(int x, int y)
	{
		return howMany(x, y) * y;
	}

	internal static int howMany(int x, int y)
	{
		long num = (x + ((long)y - 1L)) / y;
		if (num > int.MaxValue)
		{
			return 0;
		}
		return (int)num;
	}

	internal static ulong howMany(ulong x, ulong y)
	{
		return (x + (y - 1)) / y;
	}

	private ulong insertData(TiffType type, int v)
	{
		if (m_header.tiff_magic == 19789)
		{
			return (ulong)((v & m_typemask[(int)type]) << m_typeshift[(int)type]);
		}
		return (ulong)(v & m_typemask[(int)type]);
	}

	private static void resetFieldBit(int[] fields, short f)
	{
		fields[f / 32] &= ~BITn(f);
	}

	private static bool fieldSet(int[] fields, short f)
	{
		return (fields[f / 32] & BITn(f)) != 0;
	}

	private bool writeRational(TiffType type, TiffTag tag, ref TiffDirEntry dir, float v)
	{
		dir.tdir_tag = tag;
		dir.tdir_type = type;
		dir.tdir_count = 1;
		if (!writeRationalArray(ref dir, new float[1] { v }))
		{
			return false;
		}
		return true;
	}

	private bool writeRationalPair(TiffDirEntry[] entries, int dirOffset, TiffType type, TiffTag tag1, float v1, TiffTag tag2, float v2)
	{
		if (!writeRational(type, tag1, ref entries[dirOffset], v1))
		{
			return false;
		}
		if (!writeRational(type, tag2, ref entries[dirOffset + 1], v2))
		{
			return false;
		}
		return true;
	}

	private bool writeDirectory(bool done)
	{
		if (m_mode == 0)
		{
			return true;
		}
		if (done)
		{
			if ((m_flags & TiffFlags.POSTENCODE) == TiffFlags.POSTENCODE)
			{
				m_flags &= ~TiffFlags.POSTENCODE;
				if (!m_currentCodec.PostEncode())
				{
					ErrorExt(this, m_clientdata, m_name, "Error post-encoding before directory write");
					return false;
				}
			}
			m_currentCodec.Close();
			if (m_rawcc > 0 && (m_flags & TiffFlags.BEENWRITING) == TiffFlags.BEENWRITING && !flushData1())
			{
				ErrorExt(this, m_clientdata, m_name, "Error flushing data before directory write");
				return false;
			}
			if ((m_flags & TiffFlags.MYBUFFER) == TiffFlags.MYBUFFER && m_rawdata != null)
			{
				m_rawdata = null;
				m_rawcc = 0;
				m_rawdatasize = 0;
			}
			m_flags &= ~(TiffFlags.BUFFERSETUP | TiffFlags.BEENWRITING);
		}
		int num;
		long num2;
		TiffDirEntry[] array;
		while (true)
		{
			if (m_diroff == 0L && !linkDirectory())
			{
				return false;
			}
			num = 0;
			for (int i = 0; i <= 127; i++)
			{
				if (fieldSet(i) && i != 65)
				{
					num += ((i >= 5) ? 1 : 2);
				}
			}
			num += m_dir.td_customValueCount;
			num2 = num * TiffDirEntry.SizeInBytes(m_header.tiff_version == 43);
			array = new TiffDirEntry[num];
			for (int j = 0; j < num; j++)
			{
				array[j] = new TiffDirEntry();
			}
			if (m_header.tiff_version == 43)
			{
				m_dataoff = (ulong)((long)(m_diroff + 8) + num2 + 8);
			}
			else
			{
				m_dataoff = (ulong)((long)(m_diroff + 2) + num2 + 4);
			}
			if ((m_dataoff & 1) != 0L)
			{
				m_dataoff++;
			}
			seekFile((long)m_dataoff, SeekOrigin.Begin);
			m_curdir++;
			int num3 = 0;
			int[] array2 = new int[4];
			Buffer.BlockCopy(m_dir.td_fieldsset, 0, array2, 0, 16);
			if (fieldSet(array2, 31) && m_dir.td_extrasamples == 0)
			{
				resetFieldBit(array2, 31);
				num--;
				num2 -= TiffDirEntry.SizeInBytes(m_header.tiff_version == 43);
			}
			int k = 0;
			for (int num4 = m_nfields; num4 > 0; num4--, k++)
			{
				TiffFieldInfo tiffFieldInfo = m_fieldinfo[k];
				if (tiffFieldInfo.Bit == 65)
				{
					bool flag = false;
					for (int l = 0; l < m_dir.td_customValueCount; l++)
					{
						flag |= m_dir.td_customValues[l].info == tiffFieldInfo;
					}
					if (!flag)
					{
						continue;
					}
				}
				else if (!fieldSet(array2, tiffFieldInfo.Bit))
				{
					continue;
				}
				TiffTag tiffTag = TiffTag.IGNORE;
				switch (tiffFieldInfo.Bit)
				{
				case 25:
					tiffTag = (IsTiled() ? TiffTag.TILEOFFSETS : TiffTag.STRIPOFFSETS);
					if (tiffTag != tiffFieldInfo.Tag)
					{
						continue;
					}
					array[num3].tdir_tag = tiffTag;
					array[num3].tdir_count = m_dir.td_nstrips;
					if (m_header.tiff_version == 43)
					{
						array[num3].tdir_type = TiffType.LONG8;
						if (!writeLong8Array(ref array[num3], m_dir.td_stripoffset))
						{
							return false;
						}
					}
					else
					{
						array[num3].tdir_type = TiffType.LONG;
						if (!writeLongArray(ref array[num3], LongToInt(m_dir.td_stripoffset)))
						{
							return false;
						}
					}
					break;
				case 24:
					tiffTag = (IsTiled() ? TiffTag.TILEBYTECOUNTS : TiffTag.STRIPBYTECOUNTS);
					if (tiffTag != tiffFieldInfo.Tag)
					{
						continue;
					}
					array[num3].tdir_tag = tiffTag;
					array[num3].tdir_count = m_dir.td_nstrips;
					if (m_header.tiff_version == 43)
					{
						array[num3].tdir_type = TiffType.LONG8;
						if (!writeLong8Array(ref array[num3], m_dir.td_stripbytecount))
						{
							return false;
						}
					}
					else
					{
						array[num3].tdir_type = TiffType.LONG;
						if (!writeLongArray(ref array[num3], LongToInt(m_dir.td_stripbytecount)))
						{
							return false;
						}
					}
					break;
				case 17:
					setupShortLong(TiffTag.ROWSPERSTRIP, ref array[num3], m_dir.td_rowsperstrip);
					break;
				case 26:
					if (!writeShortTable(TiffTag.COLORMAP, ref array[num3], 3, m_dir.td_colormap))
					{
						return false;
					}
					break;
				case 1:
					setupShortLong(TiffTag.IMAGEWIDTH, ref array[num3++], m_dir.td_imagewidth);
					setupShortLong(TiffTag.IMAGELENGTH, ref array[num3], m_dir.td_imagelength);
					break;
				case 2:
					setupShortLong(TiffTag.TILEWIDTH, ref array[num3++], m_dir.td_tilewidth);
					setupShortLong(TiffTag.TILELENGTH, ref array[num3], m_dir.td_tilelength);
					break;
				case 7:
					setupShort(TiffTag.COMPRESSION, ref array[num3], (short)m_dir.td_compression);
					break;
				case 8:
					setupShort(TiffTag.PHOTOMETRIC, ref array[num3], (short)m_dir.td_photometric);
					break;
				case 4:
					if (!writeRationalPair(array, num3, TiffType.RATIONAL, TiffTag.XPOSITION, m_dir.td_xposition, TiffTag.YPOSITION, m_dir.td_yposition))
					{
						return false;
					}
					num3++;
					break;
				case 3:
					if (!writeRationalPair(array, num3, TiffType.RATIONAL, TiffTag.XRESOLUTION, m_dir.td_xresolution, TiffTag.YRESOLUTION, m_dir.td_yresolution))
					{
						return false;
					}
					num3++;
					break;
				case 6:
				case 18:
				case 19:
				case 32:
					if (!writePerSampleShorts(tiffFieldInfo.Tag, ref array[num3]))
					{
						return false;
					}
					break;
				case 33:
				case 34:
					if (!writePerSampleAnys(sampleToTagType(), tiffFieldInfo.Tag, ref array[num3]))
					{
						return false;
					}
					break;
				case 23:
				case 37:
				case 39:
					if (!setupShortPair(tiffFieldInfo.Tag, ref array[num3]))
					{
						return false;
					}
					break;
				case 46:
					if (!writeInkNames(ref array[num3]))
					{
						return false;
					}
					break;
				case 44:
					if (!writeTransferFunction(ref array[num3]))
					{
						return false;
					}
					break;
				case 49:
					array[num3].tdir_tag = tiffFieldInfo.Tag;
					array[num3].tdir_count = m_dir.td_nsubifd;
					if (array[num3].tdir_count > 0)
					{
						m_flags |= TiffFlags.INSUBIFD;
						m_nsubifd = (short)array[num3].tdir_count;
						if (array[num3].tdir_count > 1)
						{
							m_subifdoff = array[num3].tdir_offset;
						}
						else if ((m_flags & TiffFlags.ISBIGTIFF) == TiffFlags.ISBIGTIFF)
						{
							m_subifdoff = (ulong)((long)(m_diroff + 8) + (long)num3 * (long)TiffDirEntry.SizeInBytes(m_header.tiff_version == 43) + 4 + 8);
						}
						else
						{
							m_subifdoff = (ulong)((long)(m_diroff + 2) + (long)num3 * (long)TiffDirEntry.SizeInBytes(m_header.tiff_version == 43) + 4 + 4);
						}
					}
					if ((m_flags & TiffFlags.ISBIGTIFF) == TiffFlags.ISBIGTIFF)
					{
						array[num3].tdir_type = TiffType.IFD8;
						if (!writeLong8Array(ref array[num3], m_dir.td_subifd))
						{
							return false;
						}
					}
					else
					{
						array[num3].tdir_type = TiffType.LONG;
						if (!writeLongArray(ref array[num3], LongToInt(m_dir.td_subifd)))
						{
							return false;
						}
					}
					break;
				default:
					if (tiffFieldInfo.Tag == TiffTag.DOTRANGE)
					{
						if (!setupShortPair(tiffFieldInfo.Tag, ref array[num3]))
						{
							return false;
						}
					}
					else if (!writeNormalTag(ref array[num3], tiffFieldInfo))
					{
						return false;
					}
					break;
				}
				num3++;
				if (tiffFieldInfo.Bit != 65)
				{
					resetFieldBit(array2, tiffFieldInfo.Bit);
				}
			}
			if ((m_flags & TiffFlags.ISBIGTIFF) == TiffFlags.ISBIGTIFF || m_dataoff + 4 < uint.MaxValue)
			{
				break;
			}
			m_dataoff = m_diroff;
			if (!MakeBigTIFF())
			{
				return false;
			}
			m_diroff = 0uL;
		}
		ulong value = (ulong)num;
		ulong value2 = m_nextdiroff;
		if ((m_flags & TiffFlags.SWAB) == TiffFlags.SWAB)
		{
			int num5 = 0;
			while (value != 0L)
			{
				short value3 = (short)array[num5].tdir_tag;
				SwabShort(ref value3);
				array[num5].tdir_tag = (TiffTag)(ushort)value3;
				value3 = (short)array[num5].tdir_type;
				SwabShort(ref value3);
				array[num5].tdir_type = (TiffType)value3;
				SwabLong(ref array[num5].tdir_count);
				SwabBigTiffValue(ref array[num5].tdir_offset, m_header.tiff_version == 43, isShort: false);
				num5++;
				value--;
			}
			value = (ulong)num;
			SwabBigTiffValue(ref value, m_header.tiff_version == 43, isShort: true);
			SwabBigTiffValue(ref value2, m_header.tiff_version == 43, isShort: false);
		}
		seekFile((long)m_diroff, SeekOrigin.Begin);
		if (!writeDirCountOK((long)value, m_header.tiff_version == 43))
		{
			ErrorExt(this, m_clientdata, m_name, "Error writing directory count");
			return false;
		}
		if (!writeDirEntryOK(array, num2 / TiffDirEntry.SizeInBytes(m_header.tiff_version == 43), m_header.tiff_version == 43))
		{
			ErrorExt(this, m_clientdata, m_name, "Error writing directory contents");
			return false;
		}
		if (!writeDirOffOK((long)value2, m_header.tiff_version == 43))
		{
			ErrorExt(this, m_clientdata, m_name, "Error writing directory link");
			return false;
		}
		if (done)
		{
			FreeDirectory();
			m_flags &= ~TiffFlags.DIRTYDIRECT;
			m_currentCodec.Cleanup();
			CreateDirectory();
		}
		return true;
	}

	private bool MakeBigTIFF()
	{
		uint num = 4u;
		uint lp = (uint)m_header.tiff_diroff;
		ulong num2 = 8uL;
		int num3 = 0;
		uint num4 = 0u;
		uint num5 = 0u;
		long subifdoff = 0L;
		m_flags |= TiffFlags.ISBIGTIFF;
		m_header.tiff_version = 43;
		m_header.tiff_offsize = 8;
		m_header.tiff_fill = 0;
		m_header.tiff_diroff = 0uL;
		if ((m_flags & TiffFlags.NOBIGTIFF) == TiffFlags.NOBIGTIFF)
		{
			ErrorExt(this, Clientdata(), "TIFFCheckBigTIFF", "File > 2^32 and NO BigTIFF specified");
			return false;
		}
		if ((m_flags & TiffFlags.SWAB) == TiffFlags.SWAB)
		{
			SwabShort(ref m_header.tiff_version);
			SwabShort(ref m_header.tiff_offsize);
		}
		if (!seekOK(0L) || !writeHeaderOK(m_header))
		{
			ErrorExt(this, Clientdata(), m_name, "Error updating TIFF header", "");
			return false;
		}
		if ((m_flags & TiffFlags.SWAB) == TiffFlags.SWAB)
		{
			SwabShort(ref m_header.tiff_version);
			SwabShort(ref m_header.tiff_offsize);
		}
		ulong value3;
		while (lp != 0 && lp != m_diroff)
		{
			if (!seekOK(lp) || !readShortOK(out var value))
			{
				ErrorExt(this, m_clientdata, m_name, "Error reading TIFF directory");
				return false;
			}
			if ((m_flags & TiffFlags.SWAB) == TiffFlags.SWAB)
			{
				SwabShort(ref value);
			}
			ulong value2 = (ulong)value;
			long num6 = value * TiffDirEntry.SizeInBytes(isBigTiff: false);
			long num7 = value * TiffDirEntry.SizeInBytes(isBigTiff: true);
			TiffDirEntry[] array = new TiffDirEntry[value];
			TiffDirEntry[] array2 = new TiffDirEntry[value2];
			if (!seekOK((long)lp + 2L) || !readDirEntryOk(array, (ulong)value, isBigTiff: false))
			{
				ErrorExt(this, m_clientdata, m_name, "Error reading TIFF directory");
				return false;
			}
			value3 = m_dataoff;
			m_dataoff += (ulong)(8 + num7 + 8);
			for (short num8 = 0; num8 < value; num8++)
			{
				TiffDirEntry tiffDirEntry = array[num8];
				TiffDirEntry tiffDirEntry2 = (array2[num8] = new TiffDirEntry());
				if ((m_flags & TiffFlags.SWAB) == TiffFlags.SWAB)
				{
					SwabLong(ref tiffDirEntry.tdir_count);
					SwabBigTiffValue(ref tiffDirEntry.tdir_offset, m_header.tiff_version == 43, isShort: false);
				}
				tiffDirEntry2.tdir_tag = tiffDirEntry.tdir_tag;
				tiffDirEntry2.tdir_type = tiffDirEntry.tdir_type;
				tiffDirEntry2.tdir_count = tiffDirEntry.tdir_count;
				tiffDirEntry2.tdir_offset = tiffDirEntry.tdir_offset;
				switch (tiffDirEntry.tdir_type)
				{
				case TiffType.BYTE:
				case TiffType.ASCII:
				case TiffType.SBYTE:
				case TiffType.UNDEFINED:
					if (tiffDirEntry.tdir_count <= 4)
					{
						tiffDirEntry2.tdir_offset = tiffDirEntry.tdir_offset;
					}
					else if (tiffDirEntry.tdir_count <= 8)
					{
						byte[] array3 = new byte[tiffDirEntry.tdir_count * 4];
						seekFile((long)tiffDirEntry.tdir_offset, SeekOrigin.Begin);
						readFile(array3, 0, array3.Length);
						byte[] array7 = array3;
						if (m_header.tiff_magic == 19789)
						{
							tiffDirEntry2.tdir_offset = (ulong)array7[0] << 56;
							if (tiffDirEntry2.tdir_count >= 2)
							{
								tiffDirEntry2.tdir_offset |= (ulong)array7[1] << 48;
							}
							if (tiffDirEntry2.tdir_count >= 3)
							{
								tiffDirEntry2.tdir_offset |= (ulong)array7[2] << 40;
							}
							if (tiffDirEntry2.tdir_count >= 4)
							{
								tiffDirEntry2.tdir_offset |= (ulong)array7[3] << 32;
							}
							if (tiffDirEntry2.tdir_count >= 5)
							{
								tiffDirEntry2.tdir_offset |= (ulong)array7[4] << 24;
							}
							if (tiffDirEntry2.tdir_count >= 6)
							{
								tiffDirEntry2.tdir_offset |= (ulong)array7[5] << 16;
							}
							if (tiffDirEntry2.tdir_count >= 7)
							{
								tiffDirEntry2.tdir_offset |= (ulong)array7[6] << 8;
							}
							if (tiffDirEntry2.tdir_count == 8)
							{
								tiffDirEntry2.tdir_offset |= array7[7];
							}
						}
						else
						{
							tiffDirEntry2.tdir_offset = array7[0];
							if (tiffDirEntry2.tdir_count >= 2)
							{
								tiffDirEntry2.tdir_offset |= (ulong)array7[1] << 8;
							}
							if (tiffDirEntry2.tdir_count >= 3)
							{
								tiffDirEntry2.tdir_offset |= (ulong)array7[2] << 16;
							}
							if (tiffDirEntry2.tdir_count >= 4)
							{
								tiffDirEntry2.tdir_offset |= (ulong)array7[3] << 24;
							}
							if (tiffDirEntry2.tdir_count >= 5)
							{
								tiffDirEntry2.tdir_offset |= (ulong)array7[4] << 32;
							}
							if (tiffDirEntry2.tdir_count >= 6)
							{
								tiffDirEntry2.tdir_offset |= (ulong)array7[5] << 40;
							}
							if (tiffDirEntry2.tdir_count >= 7)
							{
								tiffDirEntry2.tdir_offset |= (ulong)array7[6] << 48;
							}
							if (tiffDirEntry2.tdir_count >= 8)
							{
								tiffDirEntry2.tdir_offset |= (ulong)array7[7] << 56;
							}
						}
					}
					else
					{
						tiffDirEntry2.tdir_offset = tiffDirEntry.tdir_offset;
					}
					break;
				case TiffType.SHORT:
				case TiffType.SSHORT:
					if (tiffDirEntry.tdir_count <= 2)
					{
						tiffDirEntry2.tdir_offset = tiffDirEntry.tdir_offset;
					}
					else if (tiffDirEntry.tdir_count <= 4)
					{
						byte[] array3 = new byte[tiffDirEntry.tdir_count * 2];
						seekFile((long)tiffDirEntry.tdir_offset, SeekOrigin.Begin);
						readFile(array3, 0, tiffDirEntry.tdir_count * 2);
						short[] array6 = ByteArrayToShorts(array3, 0, tiffDirEntry.tdir_count * 2);
						if (m_header.tiff_magic == 19789)
						{
							tiffDirEntry2.tdir_offset = (ulong)((long)array6[0] << 48);
							if (tiffDirEntry2.tdir_count >= 2)
							{
								tiffDirEntry2.tdir_offset = (ulong)((long)array6[1] << 32);
							}
							if (tiffDirEntry2.tdir_count >= 3)
							{
								tiffDirEntry2.tdir_offset = (ulong)((long)array6[2] << 16);
							}
							if (tiffDirEntry2.tdir_count == 4)
							{
								tiffDirEntry2.tdir_offset |= (ulong)array6[3] & 0xFFFFuL;
							}
						}
						else
						{
							tiffDirEntry2.tdir_offset = (ulong)array6[0] & 0xFFFFuL;
							if (tiffDirEntry2.tdir_count >= 2)
							{
								tiffDirEntry2.tdir_offset |= (ulong)((long)array6[1] << 16);
							}
							if (tiffDirEntry2.tdir_count >= 3)
							{
								tiffDirEntry2.tdir_offset |= (ulong)((long)array6[2] << 32);
							}
							if (tiffDirEntry2.tdir_count == 4)
							{
								tiffDirEntry2.tdir_offset |= (ulong)((long)array6[3] << 48);
							}
						}
					}
					else
					{
						tiffDirEntry2.tdir_offset = tiffDirEntry.tdir_offset;
					}
					break;
				case TiffType.LONG:
				case TiffType.FLOAT:
				case TiffType.IFD:
					if (tiffDirEntry.tdir_count <= 1)
					{
						tiffDirEntry2.tdir_offset = tiffDirEntry.tdir_offset;
					}
					else if (tiffDirEntry.tdir_count <= 2)
					{
						byte[] array3 = new byte[tiffDirEntry.tdir_count * 4];
						seekFile((long)tiffDirEntry.tdir_offset, SeekOrigin.Begin);
						readFile(array3, 0, tiffDirEntry.tdir_count * 4);
						int[] array5 = ByteArrayToInts(array3, 0, tiffDirEntry.tdir_count * 4);
						if (m_header.tiff_magic == 19789)
						{
							tiffDirEntry2.tdir_offset = (ulong)((long)array5[0] << 32);
							if (tiffDirEntry2.tdir_count == 2)
							{
								tiffDirEntry2.tdir_offset |= (ulong)(array5[1] & 0xFFFFFFFFu);
							}
						}
						else
						{
							tiffDirEntry2.tdir_offset = (ulong)(array5[0] & 0xFFFFFFFFu);
							if (tiffDirEntry2.tdir_count == 2)
							{
								tiffDirEntry2.tdir_offset |= (ulong)((long)array5[1] << 32);
							}
						}
					}
					else
					{
						tiffDirEntry2.tdir_offset = tiffDirEntry.tdir_offset;
					}
					break;
				case TiffType.RATIONAL:
				case TiffType.SRATIONAL:
					if (tiffDirEntry.tdir_count * 2 <= 2)
					{
						byte[] array3 = new byte[tiffDirEntry.tdir_count * 4 * 2];
						seekFile((long)tiffDirEntry.tdir_offset, SeekOrigin.Begin);
						readFile(array3, 0, tiffDirEntry.tdir_count * 4 * 2);
						int[] array4 = ByteArrayToInts(array3, 0, tiffDirEntry.tdir_count * 4 * 2);
						if (m_header.tiff_magic == 19789)
						{
							tiffDirEntry2.tdir_offset = (ulong)((long)array4[0] << 32);
							tiffDirEntry2.tdir_offset |= (ulong)(array4[1] & 0xFFFFFFFFu);
						}
						else
						{
							tiffDirEntry2.tdir_offset = (ulong)(array4[0] & 0xFFFFFFFFu);
							tiffDirEntry2.tdir_offset |= (ulong)((long)array4[1] << 32);
						}
					}
					else
					{
						tiffDirEntry2.tdir_offset = tiffDirEntry.tdir_offset;
					}
					break;
				default:
					tiffDirEntry2.tdir_offset = tiffDirEntry.tdir_offset;
					break;
				}
				if (tiffDirEntry2.tdir_tag == TiffTag.SUBIFD)
				{
					tiffDirEntry2.tdir_type = TiffType.IFD8;
					num4 = (uint)tiffDirEntry.tdir_count;
					num5 = (uint)((num4 > 1) ? tiffDirEntry.tdir_offset : ((ulong)((long)lp + 2L) + tiffDirEntry.tdir_offset));
					if (num4 <= 1)
					{
						tiffDirEntry.tdir_offset = 0uL;
						subifdoff = (long)(value3 + 8 + tiffDirEntry.tdir_offset);
					}
					else
					{
						subifdoff = (long)tiffDirEntry2.tdir_offset;
					}
				}
				if ((m_flags & TiffFlags.SWAB) == TiffFlags.SWAB)
				{
					SwabLong(ref tiffDirEntry2.tdir_count);
					SwabBigTiffValue(ref tiffDirEntry2.tdir_offset, m_header.tiff_version == 43, isShort: false);
				}
			}
			if ((m_flags & TiffFlags.SWAB) == TiffFlags.SWAB)
			{
				SwabBigTiffValue(ref value2, m_header.tiff_version == 43, isShort: true);
			}
			if (!seekOK((long)value3) || !writeDirCountOK((long)value2, isBigTiff: true) || !seekOK((long)(value3 + 8)) || !writeDirEntryOK(array2, (long)value2, isBigTiff: true))
			{
				ErrorExt(this, m_clientdata, m_name, "Error writing TIFF directory!");
				return false;
			}
			if (m_nsubifd != 0 && m_subifdoff == num5)
			{
				m_subifdoff = (ulong)subifdoff;
			}
			if (num3 == 0 && num2 == 8)
			{
				m_header.tiff_diroff = value3;
			}
			if ((m_flags & TiffFlags.SWAB) == TiffFlags.SWAB)
			{
				SwabBigTiffValue(ref value3, m_header.tiff_version == 43, isShort: false);
			}
			if (!seekOK((num3 != 0) ? subifdoff++ : ((long)num2)) || !writeDirOffOK((long)value3, isBigTiff: true))
			{
				ErrorExt(this, m_clientdata, m_name, "Error writing directory link!");
				return false;
			}
			if (num3 != 0)
			{
				num4--;
			}
			else
			{
				num = lp + 2 + (uint)(int)num6;
				num2 = value3 + 8 + (ulong)num7;
			}
			if (num4 != 0)
			{
				num3 = (int)num4;
			}
			if (!seekOK((num3 != 0) ? num5++ : num) || !readUIntOK(out lp))
			{
				ErrorExt(this, m_clientdata, m_name, "Error writing directory link!");
				return false;
			}
			if ((m_flags & TiffFlags.SWAB) == TiffFlags.SWAB)
			{
				SwabUInt(ref lp);
			}
		}
		value3 = 0uL;
		if (num2 == 8)
		{
			m_header.tiff_diroff = value3;
		}
		if ((m_flags & TiffFlags.SWAB) == TiffFlags.SWAB)
		{
			SwabBigTiffValue(ref value3, m_header.tiff_version == 43, isShort: false);
		}
		if (!seekOK((long)num2) || !writelongOK((long)value3))
		{
			ErrorExt(this, Clientdata(), m_name, "Error writing directory link", "");
			return false;
		}
		return true;
	}

	private bool writeNormalTag(ref TiffDirEntry dir, TiffFieldInfo fip)
	{
		short writeCount = fip.WriteCount;
		dir.tdir_tag = fip.Tag;
		dir.tdir_type = fip.Type;
		dir.tdir_count = writeCount;
		switch (fip.Type)
		{
		case TiffType.SHORT:
		case TiffType.SSHORT:
			if (fip.PassCount)
			{
				short[] v3;
				if (writeCount == -3)
				{
					FieldValue[] field6 = GetField(fip.Tag);
					int tdir_count3 = field6[0].ToInt();
					v3 = field6[1].ToShortArray();
					dir.tdir_count = tdir_count3;
				}
				else
				{
					FieldValue[] field7 = GetField(fip.Tag);
					writeCount = field7[0].ToShort();
					v3 = field7[1].ToShortArray();
					dir.tdir_count = writeCount;
				}
				if (!writeShortArray(ref dir, v3))
				{
					return false;
				}
			}
			else if (writeCount == 1)
			{
				short v4 = GetField(fip.Tag)[0].ToShort();
				dir.tdir_offset = insertData(dir.tdir_type, v4);
			}
			else
			{
				short[] v5 = GetField(fip.Tag)[0].ToShortArray();
				if (!writeShortArray(ref dir, v5))
				{
					return false;
				}
			}
			break;
		case TiffType.LONG:
		case TiffType.SLONG:
		case TiffType.IFD:
			if (fip.PassCount)
			{
				int[] v8;
				if (writeCount == -3)
				{
					FieldValue[] field14 = GetField(fip.Tag);
					int tdir_count6 = field14[0].ToInt();
					v8 = field14[1].ToIntArray();
					dir.tdir_count = tdir_count6;
				}
				else
				{
					FieldValue[] field15 = GetField(fip.Tag);
					writeCount = field15[0].ToShort();
					v8 = field15[1].ToIntArray();
					dir.tdir_count = writeCount;
				}
				if (!writeLongArray(ref dir, v8))
				{
					return false;
				}
			}
			else if (writeCount == 1)
			{
				FieldValue[] field16 = GetField(fip.Tag);
				dir.tdir_offset = field16[0].ToUInt();
			}
			else
			{
				int[] v9 = GetField(fip.Tag)[0].ToIntArray();
				if (!writeLongArray(ref dir, v9))
				{
					return false;
				}
			}
			break;
		case TiffType.RATIONAL:
		case TiffType.SRATIONAL:
			if (fip.PassCount)
			{
				float[] v10;
				if (writeCount == -3)
				{
					FieldValue[] field18 = GetField(fip.Tag);
					int tdir_count7 = field18[0].ToInt();
					v10 = field18[1].ToFloatArray();
					dir.tdir_count = tdir_count7;
				}
				else
				{
					FieldValue[] field19 = GetField(fip.Tag);
					writeCount = field19[0].ToShort();
					v10 = field19[1].ToFloatArray();
					dir.tdir_count = writeCount;
				}
				if (!writeRationalArray(ref dir, v10))
				{
					return false;
				}
			}
			else if (writeCount == 1)
			{
				float[] array5 = new float[1];
				FieldValue[] field20 = GetField(fip.Tag);
				array5[0] = field20[0].ToFloat();
				if (!writeRationalArray(ref dir, array5))
				{
					return false;
				}
			}
			else
			{
				float[] v11 = GetField(fip.Tag)[0].ToFloatArray();
				if (!writeRationalArray(ref dir, v11))
				{
					return false;
				}
			}
			break;
		case TiffType.FLOAT:
			if (fip.PassCount)
			{
				float[] v6;
				if (writeCount == -3)
				{
					FieldValue[] field11 = GetField(fip.Tag);
					int tdir_count5 = field11[0].ToInt();
					v6 = field11[1].ToFloatArray();
					dir.tdir_count = tdir_count5;
				}
				else
				{
					FieldValue[] field12 = GetField(fip.Tag);
					writeCount = field12[0].ToShort();
					v6 = field12[1].ToFloatArray();
					dir.tdir_count = writeCount;
				}
				if (!writeFloatArray(ref dir, v6))
				{
					return false;
				}
			}
			else if (writeCount == 1)
			{
				float[] array3 = new float[1];
				FieldValue[] field13 = GetField(fip.Tag);
				array3[0] = field13[0].ToFloat();
				if (!writeFloatArray(ref dir, array3))
				{
					return false;
				}
			}
			else
			{
				float[] v7 = GetField(fip.Tag)[0].ToFloatArray();
				if (!writeFloatArray(ref dir, v7))
				{
					return false;
				}
			}
			break;
		case TiffType.DOUBLE:
			if (fip.PassCount)
			{
				double[] v;
				if (writeCount == -3)
				{
					FieldValue[] field3 = GetField(fip.Tag);
					int tdir_count2 = field3[0].ToInt();
					v = field3[1].ToDoubleArray();
					dir.tdir_count = tdir_count2;
				}
				else
				{
					FieldValue[] field4 = GetField(fip.Tag);
					writeCount = field4[0].ToShort();
					v = field4[1].ToDoubleArray();
					dir.tdir_count = writeCount;
				}
				if (!writeDoubleArray(ref dir, v))
				{
					return false;
				}
			}
			else if (writeCount == 1)
			{
				double[] array = new double[1];
				FieldValue[] field5 = GetField(fip.Tag);
				array[0] = field5[0].ToDouble();
				if (!writeDoubleArray(ref dir, array))
				{
					return false;
				}
			}
			else
			{
				double[] v2 = GetField(fip.Tag)[0].ToDoubleArray();
				if (!writeDoubleArray(ref dir, v2))
				{
					return false;
				}
			}
			break;
		case TiffType.ASCII:
		{
			FieldValue[] field17 = GetField(fip.Tag);
			string s = ((!fip.PassCount) ? field17[0].ToString() : field17[1].ToString());
			byte[] bytes = Latin1Encoding.GetBytes(s);
			if (bytes.Length != 0 && bytes[^1] == 0)
			{
				dir.tdir_count = bytes.Length;
				if (!writeByteArray(ref dir, bytes))
				{
					return false;
				}
				break;
			}
			byte[] array4 = new byte[bytes.Length + 1];
			Buffer.BlockCopy(bytes, 0, array4, 0, bytes.Length);
			dir.tdir_count = array4.Length;
			if (!writeByteArray(ref dir, array4))
			{
				return false;
			}
			break;
		}
		case TiffType.BYTE:
		case TiffType.SBYTE:
			if (fip.PassCount)
			{
				byte[] cp2;
				if (writeCount == -3)
				{
					FieldValue[] field8 = GetField(fip.Tag);
					int tdir_count4 = field8[0].ToInt();
					cp2 = field8[1].ToByteArray();
					dir.tdir_count = tdir_count4;
				}
				else
				{
					FieldValue[] field9 = GetField(fip.Tag);
					writeCount = field9[0].ToShort();
					cp2 = field9[1].ToByteArray();
					dir.tdir_count = writeCount;
				}
				if (!writeByteArray(ref dir, cp2))
				{
					return false;
				}
			}
			else if (writeCount == 1)
			{
				byte[] array2 = new byte[1];
				FieldValue[] field10 = GetField(fip.Tag);
				array2[0] = field10[0].ToByte();
				if (!writeByteArray(ref dir, array2))
				{
					return false;
				}
			}
			else
			{
				byte[] cp3 = GetField(fip.Tag)[0].ToByteArray();
				if (!writeByteArray(ref dir, cp3))
				{
					return false;
				}
			}
			break;
		case TiffType.UNDEFINED:
		{
			byte[] cp;
			switch (writeCount)
			{
			case -1:
			{
				FieldValue[] field2 = GetField(fip.Tag);
				writeCount = field2[0].ToShort();
				cp = field2[1].ToByteArray();
				dir.tdir_count = writeCount;
				break;
			}
			case -3:
			{
				FieldValue[] field = GetField(fip.Tag);
				int tdir_count = field[0].ToInt();
				cp = field[1].ToByteArray();
				dir.tdir_count = tdir_count;
				break;
			}
			default:
				cp = GetField(fip.Tag)[0].ToByteArray();
				break;
			}
			if (!writeByteArray(ref dir, cp))
			{
				return false;
			}
			break;
		}
		}
		return true;
	}

	private void setupShortLong(TiffTag tag, ref TiffDirEntry dir, int v)
	{
		dir.tdir_tag = tag;
		dir.tdir_count = 1;
		if ((long)v > 65535L)
		{
			dir.tdir_type = TiffType.LONG;
			dir.tdir_offset = (uint)v;
		}
		else
		{
			dir.tdir_type = TiffType.SHORT;
			dir.tdir_offset = insertData(TiffType.SHORT, v);
		}
	}

	private void setupShort(TiffTag tag, ref TiffDirEntry dir, short v)
	{
		dir.tdir_tag = tag;
		dir.tdir_count = 1;
		dir.tdir_type = TiffType.SHORT;
		dir.tdir_offset = insertData(TiffType.SHORT, v);
	}

	private bool writePerSampleShorts(TiffTag tag, ref TiffDirEntry dir)
	{
		short[] array = new short[m_dir.td_samplesperpixel];
		short num = GetField(tag)[0].ToShort();
		for (short num2 = 0; num2 < m_dir.td_samplesperpixel; num2++)
		{
			array[num2] = num;
		}
		dir.tdir_tag = tag;
		dir.tdir_type = TiffType.SHORT;
		dir.tdir_count = m_dir.td_samplesperpixel;
		return writeShortArray(ref dir, array);
	}

	private bool writePerSampleAnys(TiffType type, TiffTag tag, ref TiffDirEntry dir)
	{
		double[] array = new double[m_dir.td_samplesperpixel];
		double num = GetField(tag)[0].ToDouble();
		for (short num2 = 0; num2 < m_dir.td_samplesperpixel; num2++)
		{
			array[num2] = num;
		}
		return writeAnyArray(type, tag, ref dir, m_dir.td_samplesperpixel, array);
	}

	private bool setupShortPair(TiffTag tag, ref TiffDirEntry dir)
	{
		short[] array = new short[2];
		FieldValue[] field = GetField(tag);
		array[0] = field[0].ToShort();
		array[1] = field[1].ToShort();
		dir.tdir_tag = tag;
		dir.tdir_type = TiffType.SHORT;
		dir.tdir_count = 2;
		return writeShortArray(ref dir, array);
	}

	private bool writeShortTable(TiffTag tag, ref TiffDirEntry dir, int n, short[][] table)
	{
		dir.tdir_tag = tag;
		dir.tdir_type = TiffType.SHORT;
		dir.tdir_count = 1 << (int)m_dir.td_bitspersample;
		ulong dataoff = m_dataoff;
		for (int i = 0; i < n; i++)
		{
			if (!writeData(ref dir, table[i], dir.tdir_count))
			{
				return false;
			}
		}
		dir.tdir_count *= n;
		dir.tdir_offset = dataoff;
		return true;
	}

	private bool writeByteArray(ref TiffDirEntry dir, byte[] cp)
	{
		if (m_header.tiff_version != 43 && dir.tdir_count <= 4)
		{
			if (m_header.tiff_magic == 19789)
			{
				dir.tdir_offset = (uint)(cp[0] << 24);
				if (dir.tdir_count >= 2)
				{
					dir.tdir_offset |= (uint)(cp[1] << 16);
				}
				if (dir.tdir_count >= 3)
				{
					dir.tdir_offset |= (uint)(cp[2] << 8);
				}
				if (dir.tdir_count == 4)
				{
					dir.tdir_offset |= cp[3];
				}
			}
			else
			{
				dir.tdir_offset = cp[0];
				if (dir.tdir_count >= 2)
				{
					dir.tdir_offset |= (uint)(cp[1] << 8);
				}
				if (dir.tdir_count >= 3)
				{
					dir.tdir_offset |= (uint)(cp[2] << 16);
				}
				if (dir.tdir_count == 4)
				{
					dir.tdir_offset |= (uint)(cp[3] << 24);
				}
			}
			return true;
		}
		if (m_header.tiff_version == 43 && dir.tdir_count <= 8)
		{
			if (m_header.tiff_magic == 19789)
			{
				dir.tdir_offset = (ulong)cp[0] << 56;
				if (dir.tdir_count >= 2)
				{
					dir.tdir_offset |= (ulong)cp[1] << 48;
				}
				if (dir.tdir_count >= 3)
				{
					dir.tdir_offset |= (ulong)cp[2] << 40;
				}
				if (dir.tdir_count >= 4)
				{
					dir.tdir_offset |= (ulong)cp[3] << 32;
				}
				if (dir.tdir_count >= 5)
				{
					dir.tdir_offset |= (ulong)cp[4] << 24;
				}
				if (dir.tdir_count >= 6)
				{
					dir.tdir_offset |= (ulong)cp[5] << 16;
				}
				if (dir.tdir_count >= 7)
				{
					dir.tdir_offset |= (ulong)cp[6] << 8;
				}
				if (dir.tdir_count == 8)
				{
					dir.tdir_offset |= cp[7];
				}
			}
			else
			{
				dir.tdir_offset = cp[0];
				if (dir.tdir_count >= 2)
				{
					dir.tdir_offset |= (ulong)cp[1] << 8;
				}
				if (dir.tdir_count >= 3)
				{
					dir.tdir_offset |= (ulong)cp[2] << 16;
				}
				if (dir.tdir_count >= 4)
				{
					dir.tdir_offset |= (ulong)cp[3] << 24;
				}
				if (dir.tdir_count >= 5)
				{
					dir.tdir_offset |= (ulong)cp[4] << 32;
				}
				if (dir.tdir_count >= 6)
				{
					dir.tdir_offset |= (ulong)cp[5] << 40;
				}
				if (dir.tdir_count >= 7)
				{
					dir.tdir_offset |= (ulong)cp[6] << 48;
				}
				if (dir.tdir_count >= 8)
				{
					dir.tdir_offset |= (ulong)cp[7] << 56;
				}
			}
			return true;
		}
		return writeData(ref dir, cp, dir.tdir_count);
	}

	private bool writeShortArray(ref TiffDirEntry dir, short[] v)
	{
		if (m_header.tiff_version != 43 && dir.tdir_count <= 2)
		{
			if (m_header.tiff_magic == 19789)
			{
				dir.tdir_offset = (uint)(v[0] << 16);
				if (dir.tdir_count == 2)
				{
					dir.tdir_offset |= (uint)(v[1] & 0xFFFF);
				}
			}
			else
			{
				dir.tdir_offset = (uint)v[0] & 0xFFFFu;
				if (dir.tdir_count == 2)
				{
					dir.tdir_offset |= (uint)(v[1] << 16);
				}
			}
			return true;
		}
		if (m_header.tiff_version == 43 && dir.tdir_count <= 4)
		{
			if (m_header.tiff_magic == 19789)
			{
				dir.tdir_offset = (ulong)((long)v[0] << 48);
				if (dir.tdir_count >= 2)
				{
					dir.tdir_offset |= (ulong)((long)v[1] << 32);
				}
				if (dir.tdir_count >= 3)
				{
					dir.tdir_offset |= (ulong)((long)v[2] << 16);
				}
				if (dir.tdir_count == 4)
				{
					dir.tdir_offset |= (ulong)v[3] & 0xFFFFuL;
				}
			}
			else
			{
				dir.tdir_offset = (ulong)v[0] & 0xFFFFuL;
				if (dir.tdir_count >= 2)
				{
					dir.tdir_offset |= (ulong)((long)v[1] << 16);
				}
				if (dir.tdir_count >= 3)
				{
					dir.tdir_offset |= (ulong)((long)v[2] << 32);
				}
				if (dir.tdir_count == 4)
				{
					dir.tdir_offset |= (ulong)((long)v[3] << 48);
				}
			}
			return true;
		}
		return writeData(ref dir, v, dir.tdir_count);
	}

	private bool writeLongArray(ref TiffDirEntry dir, int[] v)
	{
		if (m_header.tiff_version != 43 && dir.tdir_count == 1)
		{
			dir.tdir_offset = (uint)v[0];
			return true;
		}
		if (m_header.tiff_version == 43 && dir.tdir_count <= 2)
		{
			if (m_header.tiff_magic == 19789)
			{
				dir.tdir_offset = (ulong)((long)v[0] << 32);
				if (dir.tdir_count == 2)
				{
					dir.tdir_offset |= (ulong)(v[1] & 0xFFFFFFFFu);
				}
			}
			else
			{
				dir.tdir_offset = (ulong)(v[0] & 0xFFFFFFFFu);
				if (dir.tdir_count == 2)
				{
					dir.tdir_offset |= (ulong)((long)v[1] << 32);
				}
			}
		}
		return writeData(ref dir, v, dir.tdir_count);
	}

	private bool writeLongArray(ref TiffDirEntry dir, uint[] v)
	{
		int[] array = new int[v.Length];
		Buffer.BlockCopy(v, 0, array, 0, v.Length * 4);
		return writeLongArray(ref dir, array);
	}

	private bool writeLong8Array(ref TiffDirEntry dir, long[] v)
	{
		if (dir.tdir_count == 1)
		{
			dir.tdir_offset = (ulong)v[0];
			return true;
		}
		return writeData(ref dir, v, dir.tdir_count);
	}

	private bool writeLong8Array(ref TiffDirEntry dir, ulong[] v)
	{
		long[] array = new long[v.Length];
		Buffer.BlockCopy(v, 0, array, 0, v.Length * 8);
		return writeLong8Array(ref dir, array);
	}

	private bool writeRationalArray(ref TiffDirEntry dir, float[] v)
	{
		int[] array = new int[2 * dir.tdir_count];
		for (int i = 0; i < dir.tdir_count; i++)
		{
			int num = 1;
			float num2 = v[i];
			if (num2 < 0f)
			{
				if (dir.tdir_type == TiffType.RATIONAL)
				{
					WarningExt(this, m_clientdata, m_name, "\"{0}\": Information lost writing value ({1:G}) as (unsigned) RATIONAL", FieldWithTag(dir.tdir_tag).Name, num2);
					num2 = 0f;
				}
				else
				{
					num2 = 0f - num2;
					num = -1;
				}
			}
			int num3 = 1;
			if (num2 > 0f)
			{
				while (num2 < 268435460f && (long)num3 < 268435456L)
				{
					num2 *= 8f;
					num3 *= 8;
				}
			}
			array[2 * i] = (int)((double)num * ((double)num2 + 0.5));
			array[2 * i + 1] = num3;
		}
		if (m_header.tiff_version == 43 && dir.tdir_count == 1)
		{
			if (m_header.tiff_magic == 19789)
			{
				dir.tdir_offset = (ulong)((long)array[0] << 32);
				dir.tdir_offset |= (ulong)(array[1] & 0xFFFFFFFFu);
			}
			else
			{
				dir.tdir_offset = (ulong)(array[0] & 0xFFFFFFFFu);
				dir.tdir_offset |= (ulong)((long)array[1] << 32);
			}
			return true;
		}
		return writeData(ref dir, array, 2 * dir.tdir_count);
	}

	private bool writeFloatArray(ref TiffDirEntry dir, float[] v)
	{
		if (m_header.tiff_version != 43 && dir.tdir_count == 1)
		{
			dir.tdir_offset = BitConverter.ToUInt32(BitConverter.GetBytes(v[0]), 0);
			return true;
		}
		if (m_header.tiff_version == 43 && dir.tdir_count <= 2)
		{
			if (m_header.tiff_magic == 19789)
			{
				dir.tdir_offset = (ulong)v[0] << 32;
				if (dir.tdir_count == 2)
				{
					dir.tdir_offset |= (ulong)v[1] & 0xFFFFFFFFu;
				}
			}
			else
			{
				dir.tdir_offset = (ulong)v[0] & 0xFFFFFFFFu;
				if (dir.tdir_count == 2)
				{
					dir.tdir_offset |= (ulong)v[1] << 32;
				}
			}
			return true;
		}
		return writeData(ref dir, v, dir.tdir_count);
	}

	private bool writeDoubleArray(ref TiffDirEntry dir, double[] v)
	{
		if (m_header.tiff_version == 43 && dir.tdir_count == 1)
		{
			dir.tdir_offset = (ulong)v[0];
			return true;
		}
		return writeData(ref dir, v, dir.tdir_count);
	}

	private bool writeAnyArray(TiffType type, TiffTag tag, ref TiffDirEntry dir, int n, double[] v)
	{
		dir.tdir_tag = tag;
		dir.tdir_type = type;
		dir.tdir_count = n;
		bool flag = false;
		switch (type)
		{
		case TiffType.BYTE:
		case TiffType.SBYTE:
		{
			byte[] array4 = new byte[n];
			for (int l = 0; l < n; l++)
			{
				array4[l] = (byte)v[l];
			}
			if (!writeByteArray(ref dir, array4))
			{
				flag = true;
			}
			break;
		}
		case TiffType.SHORT:
		case TiffType.SSHORT:
		{
			short[] array = new short[n];
			for (int i = 0; i < n; i++)
			{
				array[i] = (short)v[i];
			}
			if (!writeShortArray(ref dir, array))
			{
				flag = true;
			}
			break;
		}
		case TiffType.LONG:
		case TiffType.SLONG:
		{
			int[] array3 = new int[n];
			for (int k = 0; k < n; k++)
			{
				array3[k] = (int)v[k];
			}
			if (!writeLongArray(ref dir, array3))
			{
				flag = true;
			}
			break;
		}
		case TiffType.FLOAT:
		{
			float[] array2 = new float[n];
			for (int j = 0; j < n; j++)
			{
				array2[j] = (float)v[j];
			}
			if (!writeFloatArray(ref dir, array2))
			{
				flag = true;
			}
			break;
		}
		case TiffType.DOUBLE:
			if (!writeDoubleArray(ref dir, v))
			{
				flag = true;
			}
			break;
		default:
			flag = true;
			break;
		}
		return !flag;
	}

	private bool writeTransferFunction(ref TiffDirEntry dir)
	{
		int num = m_dir.td_samplesperpixel - m_dir.td_extrasamples;
		int n = 1;
		bool flag = false;
		int elementCount = 1 << (int)m_dir.td_bitspersample;
		if (num < 0 || num > 2)
		{
			if (Compare(m_dir.td_transferfunction[0], m_dir.td_transferfunction[2], elementCount) != 0)
			{
				n = 3;
			}
			else
			{
				flag = true;
			}
		}
		if ((num == 2 || flag) && Compare(m_dir.td_transferfunction[0], m_dir.td_transferfunction[1], elementCount) != 0)
		{
			n = 3;
		}
		return writeShortTable(TiffTag.TRANSFERFUNCTION, ref dir, n, m_dir.td_transferfunction);
	}

	private bool writeInkNames(ref TiffDirEntry dir)
	{
		dir.tdir_tag = TiffTag.INKNAMES;
		dir.tdir_type = TiffType.ASCII;
		byte[] bytes = Latin1Encoding.GetBytes(m_dir.td_inknames);
		dir.tdir_count = bytes.Length;
		return writeByteArray(ref dir, bytes);
	}

	private bool writeData(ref TiffDirEntry dir, byte[] buffer, int count)
	{
		dir.tdir_offset = m_dataoff;
		count = dir.tdir_count * DataWidth(dir.tdir_type);
		if (seekOK((long)dir.tdir_offset) && writeOK(buffer, 0, count))
		{
			m_dataoff += (ulong)((count + 1) & -2);
			return true;
		}
		ErrorExt(this, m_clientdata, m_name, "Error writing data for field \"{0}\"", FieldWithTag(dir.tdir_tag).Name);
		return false;
	}

	private bool writeData(ref TiffDirEntry dir, short[] buffer, int count)
	{
		if ((m_flags & TiffFlags.SWAB) == TiffFlags.SWAB)
		{
			SwabArrayOfShort(buffer, count);
		}
		int num = count * 2;
		byte[] array = new byte[num];
		ShortsToByteArray(buffer, 0, count, array, 0);
		return writeData(ref dir, array, num);
	}

	private bool writeData(ref TiffDirEntry dir, long[] buffer, int count)
	{
		if ((m_flags & TiffFlags.SWAB) == TiffFlags.SWAB)
		{
			SwabArrayOfLong8(buffer, count);
		}
		int num = count * 8;
		byte[] array = new byte[num];
		Long8ToByteArray(buffer, 0, count, array, 0);
		return writeData(ref dir, array, num);
	}

	private bool writeData(ref TiffDirEntry dir, int[] cp, int cc)
	{
		if ((m_flags & TiffFlags.SWAB) == TiffFlags.SWAB)
		{
			SwabArrayOfLong(cp, cc);
		}
		int num = cc * 4;
		byte[] array = new byte[num];
		IntsToByteArray(cp, 0, cc, array, 0);
		return writeData(ref dir, array, num);
	}

	private bool writeData(ref TiffDirEntry dir, float[] cp, int cc)
	{
		int[] array = new int[cc];
		for (int i = 0; i < cc; i++)
		{
			byte[] bytes = BitConverter.GetBytes(cp[i]);
			array[i] = BitConverter.ToInt32(bytes, 0);
		}
		return writeData(ref dir, array, cc);
	}

	private bool writeData(ref TiffDirEntry dir, double[] buffer, int count)
	{
		if ((m_flags & TiffFlags.SWAB) == TiffFlags.SWAB)
		{
			SwabArrayOfDouble(buffer, count);
		}
		byte[] array = new byte[count * 8];
		Buffer.BlockCopy(buffer, 0, array, 0, array.Length);
		return writeData(ref dir, array, count * 8);
	}

	private void resetPenultimateDirectoryOffset()
	{
		PenultimateDirectoryOffset = 0uL;
	}

	private bool linkDirectory()
	{
		m_diroff = (ulong)((seekFile(0L, SeekOrigin.End) + 1) & -2);
		if ((m_flags & TiffFlags.ISBIGTIFF) != TiffFlags.ISBIGTIFF && m_diroff > uint.MaxValue)
		{
			if (!MakeBigTIFF())
			{
				return false;
			}
			m_diroff = (ulong)((seekFile(0L, SeekOrigin.End) + 1) & -2);
		}
		ulong value = m_diroff;
		if ((m_flags & TiffFlags.SWAB) == TiffFlags.SWAB)
		{
			SwabBigTiffValue(ref value, m_header.tiff_version == 43, isShort: false);
		}
		if ((m_flags & TiffFlags.INSUBIFD) == TiffFlags.INSUBIFD)
		{
			seekFile((long)m_subifdoff, SeekOrigin.Begin);
			if (!writeDirOffOK((long)value, m_header.tiff_version == 43))
			{
				ErrorExt(this, m_clientdata, "linkDirectory", "{0}: Error writing SubIFD directory link", m_name);
				return false;
			}
			m_nsubifd--;
			if (m_nsubifd != 0)
			{
				m_subifdoff += 4uL;
			}
			else
			{
				m_flags &= ~TiffFlags.INSUBIFD;
			}
			return true;
		}
		if (m_header.tiff_diroff == 0L)
		{
			m_header.tiff_diroff = m_diroff;
			if (m_header.tiff_version == 43)
			{
				seekFile(8L, SeekOrigin.Begin);
				if (!writelongOK((long)value))
				{
					ErrorExt(this, m_clientdata, m_name, "Error writing TIFF header");
					return false;
				}
			}
			else
			{
				seekFile(4L, SeekOrigin.Begin);
				if (!writeIntOK((int)value))
				{
					ErrorExt(this, m_clientdata, m_name, "Error writing TIFF header");
					return false;
				}
			}
			return true;
		}
		ulong value2 = Math.Max(PenultimateDirectoryOffset, m_header.tiff_diroff);
		do
		{
			PenultimateDirectoryOffset = value2;
			if (!seekOK((long)value2) || !readDirCountOK(out var value3, m_header.tiff_version == 43))
			{
				ErrorExt(this, m_clientdata, "linkDirectory", "Error fetching directory count");
				return false;
			}
			if ((m_flags & TiffFlags.SWAB) == TiffFlags.SWAB)
			{
				SwabBigTiffValue(ref value3, m_header.tiff_version == 43, isShort: true);
			}
			seekFile((long)value3 * (long)TiffDirEntry.SizeInBytes(m_header.tiff_version == 43), SeekOrigin.Current);
			if (m_header.tiff_version == 43)
			{
				if (!readUlongOK(out value2))
				{
					ErrorExt(this, m_clientdata, "linkDirectory", "Error fetching directory link");
					return false;
				}
			}
			else
			{
				if (!readUIntOK(out var value4))
				{
					ErrorExt(this, m_clientdata, "linkDirectory", "Error fetching directory link");
					return false;
				}
				value2 = value4;
			}
			if ((m_flags & TiffFlags.SWAB) == TiffFlags.SWAB)
			{
				SwabBigTiffValue(ref value2, m_header.tiff_version == 43, isShort: false);
			}
		}
		while (value2 != 0L);
		long num = seekFile(0L, SeekOrigin.Current);
		if (m_header.tiff_version == 43)
		{
			seekFile(num - 8, SeekOrigin.Begin);
			if (!writelongOK((long)value))
			{
				ErrorExt(this, m_clientdata, "linkDirectory", "Error writing directory link");
				return false;
			}
		}
		else
		{
			seekFile(num - 4, SeekOrigin.Begin);
			if (!writeIntOK((int)value))
			{
				ErrorExt(this, m_clientdata, "linkDirectory", "Error writing directory link");
				return false;
			}
		}
		return true;
	}

	private Tiff()
	{
		m_clientdata = 0;
		m_postDecodeMethod = PostDecodeMethodType.pdmNone;
		setupBuiltInCodecs();
		m_defaultTagMethods = new TiffTagMethods();
		if (m_errorHandler == null)
		{
			m_errorHandler = new TiffErrorHandler();
		}
	}

	private void Dispose(bool disposing)
	{
		if (m_disposed)
		{
			return;
		}
		if (disposing)
		{
			Close();
			if (m_fileStream != null)
			{
				m_fileStream.Dispose();
			}
		}
		m_disposed = true;
	}

	private bool WriteCustomDirectory(out long pdiroff)
	{
		resetPenultimateDirectoryOffset();
		pdiroff = -1L;
		if (m_mode == 0)
		{
			return true;
		}
		int num = 0;
		for (int i = 0; i <= 127; i++)
		{
			if (fieldSet(i) && i != 65)
			{
				num += ((i >= 5) ? 1 : 2);
			}
		}
		num += m_dir.td_customValueCount;
		int num2 = num * TiffDirEntry.SizeInBytes(m_header.tiff_version == 43);
		TiffDirEntry[] array = new TiffDirEntry[num];
		m_diroff = (ulong)((seekFile(0L, SeekOrigin.End) + 1) & -2);
		m_dataoff = (ulong)((long)(m_diroff + 2) + (long)num2 + 4);
		if ((m_dataoff & 1) != 0L)
		{
			m_dataoff++;
		}
		seekFile((long)m_dataoff, SeekOrigin.Begin);
		int[] array2 = new int[4];
		Buffer.BlockCopy(m_dir.td_fieldsset, 0, array2, 0, 16);
		int j = 0;
		for (int num3 = m_nfields; num3 > 0; num3--, j++)
		{
			TiffFieldInfo tiffFieldInfo = m_fieldinfo[j];
			if (tiffFieldInfo.Bit == 65)
			{
				bool flag = false;
				for (int k = 0; k < m_dir.td_customValueCount; k++)
				{
					flag |= m_dir.td_customValues[k].info == tiffFieldInfo;
				}
				if (!flag)
				{
					continue;
				}
			}
			else if (!fieldSet(array2, tiffFieldInfo.Bit))
			{
				continue;
			}
			if (tiffFieldInfo.Bit != 65)
			{
				resetFieldBit(array2, tiffFieldInfo.Bit);
			}
		}
		short value = (short)num;
		pdiroff = (long)m_nextdiroff;
		if ((m_flags & TiffFlags.SWAB) == TiffFlags.SWAB)
		{
			for (int l = 0; l < value; l++)
			{
				TiffDirEntry obj = array[l];
				short value2 = (short)obj.tdir_tag;
				SwabShort(ref value2);
				obj.tdir_tag = (TiffTag)(ushort)value2;
				value2 = (short)obj.tdir_type;
				SwabShort(ref value2);
				obj.tdir_type = (TiffType)value2;
				SwabLong(ref obj.tdir_count);
				SwabBigTiffValue(ref obj.tdir_offset, m_header.tiff_version == 43, isShort: false);
			}
			value = (short)num;
			SwabShort(ref value);
			int value3 = (int)pdiroff;
			SwabLong(ref value3);
			pdiroff = value3;
		}
		seekFile((long)m_diroff, SeekOrigin.Begin);
		if (!writeShortOK(value))
		{
			ErrorExt(this, m_clientdata, m_name, "Error writing directory count");
			return false;
		}
		if (!writeDirEntryOK(array, num2 / TiffDirEntry.SizeInBytes(m_header.tiff_version == 43), m_header.tiff_version == 43))
		{
			ErrorExt(this, m_clientdata, m_name, "Error writing directory contents");
			return false;
		}
		if (!writeIntOK((int)pdiroff))
		{
			ErrorExt(this, m_clientdata, m_name, "Error writing directory link");
			return false;
		}
		return true;
	}

	internal static void SwabUInt(ref uint lp)
	{
		byte[] array = new byte[4]
		{
			(byte)lp,
			(byte)(lp >> 8),
			(byte)(lp >> 16),
			(byte)(lp >> 24)
		};
		byte b = array[3];
		array[3] = array[0];
		array[0] = b;
		b = array[2];
		array[2] = array[1];
		array[1] = b;
		lp = array[0] & 0xFFu;
		lp += (uint)((array[1] & 0xFF) << 8);
		lp += (uint)((array[2] & 0xFF) << 16);
		lp += (uint)(array[3] << 24);
	}

	internal static ulong[] Realloc(ulong[] buffer, int elementCount, int newElementCount)
	{
		ulong[] array = new ulong[newElementCount];
		if (buffer != null)
		{
			int num = Math.Min(elementCount, newElementCount);
			Buffer.BlockCopy(buffer, 0, array, 0, num * 4);
		}
		return array;
	}

	internal static TiffFieldInfo[] Realloc(TiffFieldInfo[] buffer, int elementCount, int newElementCount)
	{
		TiffFieldInfo[] array = new TiffFieldInfo[newElementCount];
		if (buffer != null)
		{
			int length = Math.Min(elementCount, newElementCount);
			Array.Copy(buffer, array, length);
		}
		return array;
	}

	internal static TiffTagValue[] Realloc(TiffTagValue[] buffer, int elementCount, int newElementCount)
	{
		TiffTagValue[] array = new TiffTagValue[newElementCount];
		if (buffer != null)
		{
			int length = Math.Min(elementCount, newElementCount);
			Array.Copy(buffer, array, length);
		}
		return array;
	}

	internal bool setCompressionScheme(Compression scheme)
	{
		TiffCodec tiffCodec = FindCodec(scheme);
		if (tiffCodec == null)
		{
			tiffCodec = m_builtInCodecs[0];
		}
		m_decodestatus = tiffCodec.CanDecode;
		m_flags &= ~(TiffFlags.NOBITREV | TiffFlags.NOREADRAW);
		m_currentCodec = tiffCodec;
		return tiffCodec.Init();
	}

	private void postDecode(byte[] buffer, int offset, int count)
	{
		switch (m_postDecodeMethod)
		{
		case PostDecodeMethodType.pdmSwab16Bit:
			swab16BitData(buffer, offset, count);
			break;
		case PostDecodeMethodType.pdmSwab24Bit:
			swab24BitData(buffer, offset, count);
			break;
		case PostDecodeMethodType.pdmSwab32Bit:
			swab32BitData(buffer, offset, count);
			break;
		case PostDecodeMethodType.pdmSwab64Bit:
			swab64BitData(buffer, offset, count);
			break;
		}
	}

	private static bool checkJpegIsOJpeg(ref int v, TiffDirEntry[] dir, ulong dircount)
	{
		if ((v & 0xFFFF) == 7)
		{
			for (ulong num = 0uL; num < dircount; num++)
			{
				TiffTag tdir_tag = dir[num].tdir_tag;
				if ((uint)(tdir_tag - 512) <= 3u || (uint)(tdir_tag - 517) <= 4u)
				{
					v = 6;
					return true;
				}
			}
		}
		return false;
	}

	private void initOrder(int magic)
	{
		m_typemask = typemask;
		if (magic == 19789)
		{
			m_typeshift = bigTypeshift;
			m_flags |= TiffFlags.SWAB;
		}
		else
		{
			m_typeshift = litTypeshift;
		}
	}

	private static int getMode(string mode, string module, out FileMode m, out FileAccess a)
	{
		m = (FileMode)0;
		a = (FileAccess)0;
		int result = -1;
		if (mode.Length == 0)
		{
			return result;
		}
		switch (mode[0])
		{
		case 'r':
			m = FileMode.Open;
			a = FileAccess.Read;
			result = 0;
			if (mode.Length > 1 && mode[1] == '+')
			{
				a = FileAccess.ReadWrite;
				result = 2;
			}
			break;
		case 'w':
			m = FileMode.Create;
			a = FileAccess.ReadWrite;
			result = 770;
			break;
		case 'a':
			m = FileMode.Open;
			a = FileAccess.ReadWrite;
			result = 258;
			break;
		default:
			ErrorExt(null, 0, module, "\"{0}\": Bad mode", mode);
			break;
		}
		return result;
	}

	private static void printField(Stream fd, TiffFieldInfo fip, int value_count, object raw_data)
	{
		fprintf(fd, "  {0}: ", fip.Name);
		byte[] array = raw_data as byte[];
		sbyte[] array2 = raw_data as sbyte[];
		short[] array3 = raw_data as short[];
		ushort[] array4 = raw_data as ushort[];
		int[] array5 = raw_data as int[];
		uint[] array6 = raw_data as uint[];
		float[] array7 = raw_data as float[];
		double[] array8 = raw_data as double[];
		long[] array9 = raw_data as long[];
		ulong[] array10 = raw_data as ulong[];
		string text = raw_data as string;
		for (int i = 0; i < value_count; i++)
		{
			if (fip.Type == TiffType.BYTE || fip.Type == TiffType.SBYTE)
			{
				if (array != null)
				{
					fprintf(fd, "{0}", array[i]);
				}
				else if (array2 != null)
				{
					fprintf(fd, "{0}", array2[i]);
				}
			}
			else if (fip.Type == TiffType.UNDEFINED)
			{
				if (array != null)
				{
					fprintf(fd, "0x{0:x}", array[i]);
				}
			}
			else if (fip.Type == TiffType.SHORT || fip.Type == TiffType.SSHORT)
			{
				if (array3 != null)
				{
					fprintf(fd, "{0}", array3[i]);
				}
				else if (array4 != null)
				{
					fprintf(fd, "{0}", array4[i]);
				}
			}
			else if (fip.Type == TiffType.LONG || fip.Type == TiffType.SLONG)
			{
				if (array5 != null)
				{
					fprintf(fd, "{0}", array5[i]);
				}
				else if (array6 != null)
				{
					fprintf(fd, "{0}", array6[i]);
				}
			}
			else if (fip.Type == TiffType.LONG8 || fip.Type == TiffType.SLONG8)
			{
				if (array9 != null)
				{
					fprintf(fd, "{0}", array9[i]);
				}
				else if (array10 != null)
				{
					fprintf(fd, "{0}", array10[i]);
				}
			}
			else if (fip.Type == TiffType.IFD8)
			{
				if (array9 != null)
				{
					fprintf(fd, "0x{0:x}", array9[i]);
				}
				else if (array10 != null)
				{
					fprintf(fd, "0x{0:x}", array10[i]);
				}
			}
			else if (fip.Type == TiffType.RATIONAL || fip.Type == TiffType.SRATIONAL || fip.Type == TiffType.FLOAT)
			{
				if (array7 != null)
				{
					fprintf(fd, "{0}", array7[i]);
				}
			}
			else if (fip.Type == TiffType.IFD)
			{
				if (array5 != null)
				{
					fprintf(fd, "0x{0:x}", array5[i]);
				}
				else if (array6 != null)
				{
					fprintf(fd, "0x{0:x}", array6[i]);
				}
			}
			else
			{
				if (fip.Type == TiffType.ASCII)
				{
					if (text != null)
					{
						fprintf(fd, "{0}", text);
					}
					break;
				}
				if (fip.Type != TiffType.DOUBLE && fip.Type != TiffType.FLOAT)
				{
					fprintf(fd, "<unsupported data type in printField>");
					break;
				}
				if (array7 != null)
				{
					fprintf(fd, "{0}", array7[i]);
				}
				else if (array8 != null)
				{
					fprintf(fd, "{0}", array8[i]);
				}
			}
			if (i < value_count - 1)
			{
				fprintf(fd, ",");
			}
		}
		fprintf(fd, "\r\n");
	}

	private static bool prettyPrintField(Stream fd, TiffTag tag, int value_count, object raw_data)
	{
		FieldValue fieldValue = new FieldValue(raw_data);
		short[] array = fieldValue.ToShortArray();
		float[] array2 = fieldValue.ToFloatArray();
		double[] array3 = fieldValue.ToDoubleArray();
		switch (tag)
		{
		case TiffTag.INKSET:
			if (array != null)
			{
				fprintf(fd, "  Ink Set: ");
				if (array[0] == 1)
				{
					fprintf(fd, "CMYK\n");
				}
				else
				{
					fprintf(fd, "{0} (0x{1:x})\n", array[0], array[0]);
				}
				return true;
			}
			return false;
		case TiffTag.DOTRANGE:
			if (array != null)
			{
				fprintf(fd, "  Dot Range: {0}-{1}\n", array[0], array[1]);
				return true;
			}
			return false;
		case TiffTag.WHITEPOINT:
			if (array2 != null)
			{
				fprintf(fd, "  White Point: {0:G}-{1:G}\n", array2[0], array2[1]);
				return true;
			}
			return false;
		case TiffTag.REFERENCEBLACKWHITE:
			if (array2 != null)
			{
				fprintf(fd, "  Reference Black/White:\n");
				for (short num = 0; num < 3; num++)
				{
					fprintf(fd, "    {0,2:D}: {1,5:G} {2,5:G}\n", num, array2[2 * num], array2[2 * num + 1]);
				}
				return true;
			}
			return false;
		case TiffTag.XMLPACKET:
			if (raw_data is string text)
			{
				fprintf(fd, "  XMLPacket (XMP Metadata):\n");
				fprintf(fd, text.Substring(0, value_count));
				fprintf(fd, "\n");
				return true;
			}
			return false;
		case TiffTag.RICHTIFFIPTC:
			fprintf(fd, "  RichTIFFIPTC Data: <present>, {0} bytes\n", value_count * 4);
			return true;
		case TiffTag.PHOTOSHOP:
			fprintf(fd, "  Photoshop Data: <present>, {0} bytes\n", value_count);
			return true;
		case TiffTag.ICCPROFILE:
			fprintf(fd, "  ICC Profile: <present>, {0} bytes\n", value_count);
			return true;
		case TiffTag.STONITS:
			if (array3 != null)
			{
				fprintf(fd, "  Sample to Nits conversion factor: {0:e4}\n", array3[0]);
				return true;
			}
			return false;
		default:
			return false;
		}
	}

	private static void printAscii(Stream fd, string cp)
	{
		for (int i = 0; cp[i] != 0; i++)
		{
			if (!char.IsControl(cp[i]))
			{
				fprintf(fd, "{0}", cp[i]);
				continue;
			}
			string text = "\tt\bb\rr\nn\vv";
			int j;
			for (j = 0; text[j] != 0; j++)
			{
				if (text[j++] == cp[i])
				{
					break;
				}
			}
			if (text[j] != 0)
			{
				fprintf(fd, "\\{0}", text[j]);
			}
			else
			{
				fprintf(fd, "\\{0}", encodeOctalString((byte)(cp[i] & 0xFFu)));
			}
		}
	}

	private int readFile(byte[] buf, int offset, int size)
	{
		return m_stream.Read(m_clientdata, buf, offset, size);
	}

	private long seekFile(long off, SeekOrigin whence)
	{
		return m_stream.Seek(m_clientdata, off, whence);
	}

	private long getFileSize()
	{
		return m_stream.Size(m_clientdata);
	}

	private bool readOK(byte[] buf, int size)
	{
		return readFile(buf, 0, size) == size;
	}

	private bool readShortOK(out short value)
	{
		byte[] array = new byte[2];
		bool num = readOK(array, 2);
		value = 0;
		if (num)
		{
			value = (short)(array[0] & 0xFF);
			value += (short)((array[1] & 0xFF) << 8);
		}
		return num;
	}

	private bool readDirCountOK(out ulong dircount, bool isBigTiff)
	{
		if (isBigTiff)
		{
			ulong value;
			bool result = readUlongOK(out value);
			dircount = value;
			return result;
		}
		short value2;
		bool result2 = readShortOK(out value2);
		dircount = (ulong)value2;
		return result2;
	}

	private bool readUIntOK(out uint value)
	{
		int value2;
		bool num = readIntOK(out value2);
		if (num)
		{
			value = (uint)value2;
			return num;
		}
		value = 0u;
		return num;
	}

	private bool readUlongOK(out ulong value)
	{
		long value2;
		bool num = readLongOK(out value2);
		if (num)
		{
			value = (ulong)value2;
			return num;
		}
		value = 0uL;
		return num;
	}

	private bool readLongOK(out long value)
	{
		byte[] array = new byte[8];
		bool num = readOK(array, 8);
		value = 0L;
		if (num)
		{
			value = (long)array[0] & 0xFFL;
			value += (long)(array[1] & 0xFF) << 8;
			value += (long)(array[2] & 0xFF) << 16;
			value += (long)(array[3] & 0xFF) << 24;
			value += (long)(array[4] & 0xFF) << 32;
			value += (long)(array[5] & 0xFF) << 40;
			value += (long)(array[6] & 0xFF) << 48;
			value += (long)((ulong)array[7] << 56);
		}
		return num;
	}

	private bool readIntOK(out int value)
	{
		byte[] array = new byte[4];
		bool num = readOK(array, 4);
		value = 0;
		if (num)
		{
			value = array[0] & 0xFF;
			value += (array[1] & 0xFF) << 8;
			value += (array[2] & 0xFF) << 16;
			value += array[3] << 24;
		}
		return num;
	}

	private bool readDirEntryOk(TiffDirEntry[] dir, ulong dircount, bool isBigTiff)
	{
		int num = 0;
		num = ((!isBigTiff) ? 12 : 20);
		int num2 = num * (int)dircount;
		byte[] array = new byte[num2];
		bool num3 = readOK(array, num2);
		if (num3)
		{
			readDirEntry(dir, dircount, array, 0, isBigTiff);
		}
		return num3;
	}

	private static void readDirEntry(TiffDirEntry[] dir, ulong dircount, byte[] bytes, int offset, bool isBigTiff)
	{
		int num = offset;
		for (ulong num2 = 0uL; num2 < dircount; num2++)
		{
			TiffDirEntry tiffDirEntry = new TiffDirEntry();
			tiffDirEntry.tdir_tag = (TiffTag)(ushort)readShort(bytes, num);
			num += 2;
			tiffDirEntry.tdir_type = (TiffType)readShort(bytes, num);
			num += 2;
			if (isBigTiff)
			{
				tiffDirEntry.tdir_count = (int)readULong(bytes, num);
				num += 8;
				tiffDirEntry.tdir_offset = readULong(bytes, num);
				num += 8;
			}
			else
			{
				tiffDirEntry.tdir_count = readInt(bytes, num);
				num += 4;
				tiffDirEntry.tdir_offset = (uint)readInt(bytes, num);
				num += 4;
			}
			dir[num2] = tiffDirEntry;
		}
	}

	private bool readHeaderOkWithoutExceptions(ref TiffHeader header)
	{
		try
		{
			return readHeaderOk(ref header);
		}
		catch
		{
			WarningExt(this, m_clientdata, m_name, "Failed to read header");
			return false;
		}
	}

	private bool readHeaderOk(ref TiffHeader header)
	{
		bool flag = readShortOK(out header.tiff_magic);
		if (flag)
		{
			flag = readShortOK(out header.tiff_version);
		}
		if (flag)
		{
			if (header.tiff_version == 43)
			{
				flag = readShortOK(out header.tiff_offsize);
				if (flag)
				{
					flag = readShortOK(out header.tiff_fill);
				}
				if (flag)
				{
					flag = readUlongOK(out header.tiff_diroff);
				}
			}
			else
			{
				flag = readUIntOK(out var value);
				header.tiff_diroff = value;
			}
		}
		return flag;
	}

	private bool seekOK(long off)
	{
		return seekFile(off, SeekOrigin.Begin) == off;
	}

	private bool seek(int row, short sample)
	{
		if (row >= m_dir.td_imagelength)
		{
			ErrorExt(this, m_clientdata, m_name, "{0}: Row out of range, max {1}", row, m_dir.td_imagelength);
			return false;
		}
		int num;
		if (m_dir.td_planarconfig != PlanarConfig.SEPARATE)
		{
			num = ((m_dir.td_rowsperstrip != -1) ? (row / m_dir.td_rowsperstrip) : 0);
		}
		else
		{
			if (sample >= m_dir.td_samplesperpixel)
			{
				ErrorExt(this, m_clientdata, m_name, "{0}: Sample out of range, max {1}", sample, m_dir.td_samplesperpixel);
				return false;
			}
			num = ((m_dir.td_rowsperstrip != -1) ? (sample * m_dir.td_stripsperimage + row / m_dir.td_rowsperstrip) : 0);
		}
		if (num != m_curstrip)
		{
			if (!fillStrip(num))
			{
				return false;
			}
		}
		else if (row < m_row && !startStrip(num))
		{
			return false;
		}
		if (row != m_row)
		{
			if (!m_currentCodec.Seek(row - m_row))
			{
				return false;
			}
			m_row = row;
		}
		return true;
	}

	private int readRawStrip1(int strip, byte[] buf, int offset, int size, string module)
	{
		if (!seekOK((long)m_dir.td_stripoffset[strip]))
		{
			ErrorExt(this, m_clientdata, module, "{0}: Seek error at scanline {1}, strip {2}", m_name, m_row, strip);
			return -1;
		}
		int num = readFile(buf, offset, size);
		if (num != size)
		{
			ErrorExt(this, m_clientdata, module, "{0}: Read error at scanline {1}; got {2} bytes, expected {3}", m_name, m_row, num, size);
			return -1;
		}
		return size;
	}

	private int readRawTile1(int tile, byte[] buf, int offset, int size, string module)
	{
		if (!seekOK((long)m_dir.td_stripoffset[tile]))
		{
			ErrorExt(this, m_clientdata, module, "{0}: Seek error at row {1}, col {2}, tile {3}", m_name, m_row, m_col, tile);
			return -1;
		}
		int num = readFile(buf, offset, size);
		if (num != size)
		{
			ErrorExt(this, m_clientdata, module, "{0}: Read error at row {1}, col {2}; got {3} bytes, expected {4}", m_name, m_row, m_col, num, size);
			return -1;
		}
		return size;
	}

	private bool startStrip(int strip)
	{
		if ((m_flags & TiffFlags.CODERSETUP) != TiffFlags.CODERSETUP)
		{
			if (!m_currentCodec.SetupDecode())
			{
				return false;
			}
			m_flags |= TiffFlags.CODERSETUP;
		}
		m_curstrip = strip;
		m_row = strip % m_dir.td_stripsperimage * m_dir.td_rowsperstrip;
		m_rawcp = 0;
		if ((m_flags & TiffFlags.NOREADRAW) == TiffFlags.NOREADRAW)
		{
			m_rawcc = 0;
		}
		else
		{
			m_rawcc = (int)m_dir.td_stripbytecount[strip];
		}
		return m_currentCodec.PreDecode((short)(strip / m_dir.td_stripsperimage));
	}

	private bool startTile(int tile)
	{
		if ((m_flags & TiffFlags.CODERSETUP) != TiffFlags.CODERSETUP)
		{
			if (!m_currentCodec.SetupDecode())
			{
				return false;
			}
			m_flags |= TiffFlags.CODERSETUP;
		}
		m_curtile = tile;
		m_row = tile % howMany(m_dir.td_imagewidth, m_dir.td_tilewidth) * m_dir.td_tilelength;
		m_col = tile % howMany(m_dir.td_imagelength, m_dir.td_tilelength) * m_dir.td_tilewidth;
		m_rawcp = 0;
		if ((m_flags & TiffFlags.NOREADRAW) == TiffFlags.NOREADRAW)
		{
			m_rawcc = 0;
		}
		else
		{
			m_rawcc = (int)m_dir.td_stripbytecount[tile];
		}
		return m_currentCodec.PreDecode((short)(tile / m_dir.td_stripsperimage));
	}

	private bool checkRead(bool tiles)
	{
		if (m_mode == 1)
		{
			ErrorExt(this, m_clientdata, m_name, "File not open for reading");
			return false;
		}
		if (tiles ^ IsTiled())
		{
			ErrorExt(this, m_clientdata, m_name, tiles ? "Can not read tiles from a stripped image" : "Can not read scanlines from a tiled image");
			return false;
		}
		return true;
	}

	private static void swab16BitData(byte[] buffer, int offset, int count)
	{
		short[] array = ByteArrayToShorts(buffer, offset, count);
		SwabArrayOfShort(array, count / 2);
		ShortsToByteArray(array, 0, count / 2, buffer, offset);
	}

	private static void swab24BitData(byte[] buffer, int offset, int count)
	{
		SwabArrayOfTriples(buffer, offset, count / 3);
	}

	private static void swab32BitData(byte[] buffer, int offset, int count)
	{
		int[] array = ByteArrayToInts(buffer, offset, count);
		SwabArrayOfLong(array, count / 4);
		IntsToByteArray(array, 0, count / 4, buffer, offset);
	}

	private static void swab64BitData(byte[] buffer, int offset, int count)
	{
		int num = count / 8;
		double[] array = new double[num];
		int num2 = offset;
		for (int i = 0; i < num; i++)
		{
			array[i] = BitConverter.ToDouble(buffer, num2);
			num2 += 8;
		}
		SwabArrayOfDouble(array, num);
		num2 = offset;
		for (int j = 0; j < num; j++)
		{
			byte[] bytes = BitConverter.GetBytes(array[j]);
			Buffer.BlockCopy(bytes, 0, buffer, num2, bytes.Length);
			num2 += bytes.Length;
		}
	}

	internal bool fillStrip(int strip)
	{
		if ((m_flags & TiffFlags.NOREADRAW) != TiffFlags.NOREADRAW)
		{
			int num = (int)m_dir.td_stripbytecount[strip];
			if (num <= 0)
			{
				ErrorExt(this, m_clientdata, m_name, "{0}: Invalid strip byte count, strip {1}", num, strip);
				return false;
			}
			if (num > m_rawdatasize)
			{
				m_curstrip = -1;
				if ((m_flags & TiffFlags.MYBUFFER) != TiffFlags.MYBUFFER)
				{
					ErrorExt(this, m_clientdata, "fillStrip", "{0}: Data buffer too small to hold strip {1}", m_name, strip);
					return false;
				}
				ReadBufferSetup(null, roundUp(num, 1024));
			}
			if (readRawStrip1(strip, m_rawdata, 0, num, "fillStrip") != num)
			{
				return false;
			}
			if (!isFillOrder(m_dir.td_fillorder) && (m_flags & TiffFlags.NOBITREV) != TiffFlags.NOBITREV)
			{
				ReverseBits(m_rawdata, num);
			}
		}
		return startStrip(strip);
	}

	internal bool fillTile(int tile)
	{
		if ((m_flags & TiffFlags.NOREADRAW) != TiffFlags.NOREADRAW)
		{
			int num = (int)m_dir.td_stripbytecount[tile];
			if (num <= 0)
			{
				ErrorExt(this, m_clientdata, m_name, "{0}: Invalid tile byte count, tile {1}", num, tile);
				return false;
			}
			if (num > m_rawdatasize)
			{
				m_curtile = -1;
				if ((m_flags & TiffFlags.MYBUFFER) != TiffFlags.MYBUFFER)
				{
					ErrorExt(this, m_clientdata, "fillTile", "{0}: Data buffer too small to hold tile {1}", m_name, tile);
					return false;
				}
				ReadBufferSetup(null, roundUp(num, 1024));
			}
			if (readRawTile1(tile, m_rawdata, 0, num, "fillTile") != num)
			{
				return false;
			}
			if (!isFillOrder(m_dir.td_fillorder) && (m_flags & TiffFlags.NOBITREV) != TiffFlags.NOBITREV)
			{
				ReverseBits(m_rawdata, num);
			}
		}
		return startTile(tile);
	}

	private int summarize(int summand1, int summand2, string where)
	{
		int num = summand1 + summand2;
		if (num - summand1 != summand2)
		{
			ErrorExt(this, m_clientdata, m_name, "Integer overflow in {0}", where);
			num = 0;
		}
		return num;
	}

	private int multiply(int nmemb, int elem_size, string where)
	{
		int num = nmemb * elem_size;
		if (elem_size != 0 && num / elem_size != nmemb)
		{
			ErrorExt(this, m_clientdata, m_name, "Integer overflow in {0}", where);
			num = 0;
		}
		return num;
	}

	internal int newScanlineSize()
	{
		int nmemb;
		if (m_dir.td_planarconfig == PlanarConfig.CONTIG)
		{
			if (m_dir.td_photometric == Photometric.YCBCR && !IsUpSampled())
			{
				FieldValue[] field = GetField(TiffTag.YCBCRSUBSAMPLING);
				ushort num = field[0].ToUShort();
				ushort num2 = field[1].ToUShort();
				if (num * num2 == 0)
				{
					ErrorExt(this, m_clientdata, m_name, "Invalid YCbCr subsampling");
					return 0;
				}
				return ((m_dir.td_imagewidth + num - 1) / num * (num * num2 + 2) * m_dir.td_bitspersample + 7) / 8 / num2;
			}
			nmemb = multiply(m_dir.td_imagewidth, m_dir.td_samplesperpixel, "TIFFScanlineSize");
		}
		else
		{
			nmemb = m_dir.td_imagewidth;
		}
		return howMany8(multiply(nmemb, m_dir.td_bitspersample, "TIFFScanlineSize"));
	}

	internal int oldScanlineSize()
	{
		int num = multiply(m_dir.td_bitspersample, m_dir.td_imagewidth, "TIFFScanlineSize");
		if (m_dir.td_planarconfig == PlanarConfig.CONTIG)
		{
			num = multiply(num, m_dir.td_samplesperpixel, "TIFFScanlineSize");
		}
		return howMany8(num);
	}

	private bool writeCheckStrips(string module)
	{
		if ((m_flags & TiffFlags.BEENWRITING) != TiffFlags.BEENWRITING)
		{
			return WriteCheck(tiles: false, module);
		}
		return true;
	}

	private bool writeCheckTiles(string module)
	{
		if ((m_flags & TiffFlags.BEENWRITING) != TiffFlags.BEENWRITING)
		{
			return WriteCheck(tiles: true, module);
		}
		return true;
	}

	private void bufferCheck()
	{
		if ((m_flags & TiffFlags.BUFFERSETUP) != TiffFlags.BUFFERSETUP || m_rawdata == null)
		{
			WriteBufferSetup(null, -1);
		}
	}

	private bool writeOK(byte[] buffer, int offset, int count)
	{
		try
		{
			m_stream.Write(m_clientdata, buffer, offset, count);
		}
		catch (Exception)
		{
			Warning(this, "writeOK", "Failed to write {0} bytes", count);
			return false;
		}
		return true;
	}

	private bool writeHeaderOK(TiffHeader header)
	{
		resetPenultimateDirectoryOffset();
		bool flag = writeShortOK(header.tiff_magic);
		if (flag)
		{
			flag = writeShortOK(header.tiff_version);
		}
		if (header.tiff_version == 43)
		{
			if (flag)
			{
				flag = writeShortOK(header.tiff_offsize);
			}
			if (flag)
			{
				flag = writeShortOK(header.tiff_fill);
			}
			if (flag)
			{
				flag = writelongOK((long)header.tiff_diroff);
			}
		}
		else
		{
			if (flag)
			{
				flag = writeIntOK((int)header.tiff_diroff);
			}
			if (flag)
			{
				flag = writelongOK(0L);
			}
		}
		return flag;
	}

	private bool writeDirEntryOK(TiffDirEntry[] entries, long count, bool isBigTiff)
	{
		bool flag = true;
		for (long num = 0L; num < count; num++)
		{
			flag = writeShortOK((short)entries[num].tdir_tag);
			if (flag)
			{
				flag = writeShortOK((short)entries[num].tdir_type);
			}
			if (isBigTiff)
			{
				if (flag)
				{
					flag = writelongOK(entries[num].tdir_count);
				}
				if (flag)
				{
					flag = writelongOK((long)entries[num].tdir_offset);
				}
			}
			else
			{
				if (flag)
				{
					flag = writeIntOK(entries[num].tdir_count);
				}
				if (flag)
				{
					flag = writeIntOK((int)entries[num].tdir_offset);
				}
			}
			if (!flag)
			{
				break;
			}
		}
		return flag;
	}

	private bool writeShortOK(short value)
	{
		return writeOK(new byte[2]
		{
			(byte)value,
			(byte)(value >> 8)
		}, 0, 2);
	}

	private bool writeDirCountOK(long value, bool isBigTiff)
	{
		if (isBigTiff)
		{
			return writelongOK(value);
		}
		return writeShortOK((short)value);
	}

	private bool writeDirOffOK(long value, bool isBigTiff)
	{
		if (isBigTiff)
		{
			return writelongOK(value);
		}
		return writeIntOK((int)value);
	}

	private bool writeIntOK(int value)
	{
		return writeOK(new byte[4]
		{
			(byte)value,
			(byte)(value >> 8),
			(byte)(value >> 16),
			(byte)(value >> 24)
		}, 0, 4);
	}

	private bool writelongOK(long value)
	{
		return writeOK(new byte[8]
		{
			(byte)value,
			(byte)(value >> 8),
			(byte)(value >> 16),
			(byte)(value >> 24),
			(byte)(value >> 32),
			(byte)(value >> 40),
			(byte)(value >> 48),
			(byte)(value >> 56)
		}, 0, 8);
	}

	private bool isUnspecified(int f)
	{
		if (fieldSet(f))
		{
			return m_dir.td_imagelength == 0;
		}
		return false;
	}

	private bool growStrips(int delta)
	{
		ulong[] td_stripoffset = Realloc(m_dir.td_stripoffset, m_dir.td_nstrips, m_dir.td_nstrips + delta);
		ulong[] td_stripbytecount = Realloc(m_dir.td_stripbytecount, m_dir.td_nstrips, m_dir.td_nstrips + delta);
		m_dir.td_stripoffset = td_stripoffset;
		m_dir.td_stripbytecount = td_stripbytecount;
		Array.Clear(m_dir.td_stripoffset, m_dir.td_nstrips, delta);
		Array.Clear(m_dir.td_stripbytecount, m_dir.td_nstrips, delta);
		m_dir.td_nstrips += delta;
		return true;
	}

	private bool appendToStrip(int strip, byte[] buffer, int offset, long count)
	{
		if (m_dir.td_stripoffset[strip] == 0L || m_curoff == 0L)
		{
			if (m_dir.td_stripbytecount[strip] != 0L && m_dir.td_stripoffset[strip] != 0L && m_dir.td_stripbytecount[strip] >= (ulong)count)
			{
				if (!seekOK((long)m_dir.td_stripoffset[strip]))
				{
					ErrorExt(this, m_clientdata, "appendToStrip", "Seek error at scanline {0}", m_row);
					return false;
				}
			}
			else
			{
				m_dir.td_stripoffset[strip] = (ulong)seekFile(0L, SeekOrigin.End);
			}
			m_curoff = m_dir.td_stripoffset[strip];
			m_dir.td_stripbytecount[strip] = 0uL;
		}
		if (!writeOK(buffer, offset, (int)count))
		{
			ErrorExt(this, m_clientdata, "appendToStrip", "Write error at scanline {0}", m_row);
			return false;
		}
		m_curoff += (ulong)count;
		m_dir.td_stripbytecount[strip] += (ulong)count;
		return true;
	}

	internal bool flushData1()
	{
		if (m_rawcc > 0)
		{
			if (!isFillOrder(m_dir.td_fillorder) && (m_flags & TiffFlags.NOBITREV) != TiffFlags.NOBITREV)
			{
				ReverseBits(m_rawdata, m_rawcc);
			}
			if (!appendToStrip(IsTiled() ? m_curtile : m_curstrip, m_rawdata, 0, m_rawcc))
			{
				return false;
			}
			m_rawcc = 0;
			m_rawcp = 0;
		}
		return true;
	}

	public static string GetVersion()
	{
		return string.Format(CultureInfo.InvariantCulture, "LibTiff.Net, Version {0}\nCopyright (C) 2008-2020, Bit Miracle.", AssemblyVersion);
	}

	public static int GetR(int abgr)
	{
		return abgr & 0xFF;
	}

	public static int GetG(int abgr)
	{
		return (abgr >> 8) & 0xFF;
	}

	public static int GetB(int abgr)
	{
		return (abgr >> 16) & 0xFF;
	}

	public static int GetA(int abgr)
	{
		return (abgr >> 24) & 0xFF;
	}

	public TiffCodec FindCodec(Compression scheme)
	{
		for (codecList codecList = m_registeredCodecs; codecList != null; codecList = codecList.next)
		{
			if (codecList.codec.m_scheme == scheme)
			{
				return codecList.codec;
			}
		}
		for (int i = 0; m_builtInCodecs[i] != null; i++)
		{
			TiffCodec tiffCodec = m_builtInCodecs[i];
			if (tiffCodec.m_scheme == scheme)
			{
				return tiffCodec;
			}
		}
		return null;
	}

	public void RegisterCodec(TiffCodec codec)
	{
		if (codec == null)
		{
			throw new ArgumentNullException("codec");
		}
		codecList codecList = new codecList();
		codecList.codec = codec;
		codecList.next = m_registeredCodecs;
		m_registeredCodecs = codecList;
	}

	public void UnRegisterCodec(TiffCodec codec)
	{
		if (m_registeredCodecs == null)
		{
			return;
		}
		if (m_registeredCodecs.codec == codec)
		{
			codecList next = m_registeredCodecs.next;
			m_registeredCodecs = next;
			return;
		}
		for (codecList codecList = m_registeredCodecs; codecList != null; codecList = codecList.next)
		{
			if (codecList.next != null && codecList.next.codec == codec)
			{
				codecList next = codecList.next.next;
				codecList.next = next;
				return;
			}
		}
		ErrorExt(this, 0, "UnRegisterCodec", "Cannot remove compression scheme {0}; not registered", codec.m_name);
	}

	public bool IsCodecConfigured(Compression scheme)
	{
		TiffCodec tiffCodec = FindCodec(scheme);
		if (tiffCodec == null)
		{
			return false;
		}
		if (tiffCodec.CanEncode || tiffCodec.CanDecode)
		{
			return true;
		}
		return false;
	}

	public TiffCodec[] GetConfiguredCodecs()
	{
		int num = 0;
		for (int i = 0; m_builtInCodecs[i] != null; i++)
		{
			if (m_builtInCodecs[i] != null && IsCodecConfigured(m_builtInCodecs[i].m_scheme))
			{
				num++;
			}
		}
		for (codecList codecList = m_registeredCodecs; codecList != null; codecList = codecList.next)
		{
			num++;
		}
		TiffCodec[] array = new TiffCodec[num];
		int num2 = 0;
		for (codecList codecList2 = m_registeredCodecs; codecList2 != null; codecList2 = codecList2.next)
		{
			array[num2++] = codecList2.codec;
		}
		for (int j = 0; m_builtInCodecs[j] != null; j++)
		{
			if (m_builtInCodecs[j] != null && IsCodecConfigured(m_builtInCodecs[j].m_scheme))
			{
				array[num2++] = m_builtInCodecs[j];
			}
		}
		return array;
	}

	public static byte[] Realloc(byte[] array, int size)
	{
		byte[] array2 = new byte[size];
		if (array != null)
		{
			int count = Math.Min(array.Length, size);
			Buffer.BlockCopy(array, 0, array2, 0, count);
		}
		return array2;
	}

	public static int[] Realloc(int[] array, int size)
	{
		int[] array2 = new int[size];
		if (array != null)
		{
			int num = Math.Min(array.Length, size);
			Buffer.BlockCopy(array, 0, array2, 0, num * 4);
		}
		return array2;
	}

	public static int Compare(short[] first, short[] second, int elementCount)
	{
		for (int i = 0; i < elementCount; i++)
		{
			if (first[i] != second[i])
			{
				return first[i] - second[i];
			}
		}
		return 0;
	}

	public static Tiff Open(string fileName, string mode)
	{
		return Open(fileName, mode, null, null);
	}

	public static Tiff ClientOpen(string name, string mode, object clientData, TiffStream stream)
	{
		return ClientOpen(name, mode, clientData, stream, null, null);
	}

	public void Close()
	{
		Flush();
		m_stream.Close(m_clientdata);
		if (m_fileStream != null)
		{
			m_fileStream.Close();
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	public int GetTagListCount()
	{
		return m_dir.td_customValueCount;
	}

	public int GetTagListEntry(int index)
	{
		if (index < 0 || index >= m_dir.td_customValueCount)
		{
			return -1;
		}
		return (int)m_dir.td_customValues[index].info.Tag;
	}

	public void MergeFieldInfo(TiffFieldInfo[] info, int count)
	{
		m_foundfield = null;
		if (m_nfields <= 0)
		{
			m_fieldinfo = new TiffFieldInfo[count];
		}
		for (int i = 0; i < count; i++)
		{
			if (FindFieldInfo(info[i].Tag, info[i].Type) == null)
			{
				m_fieldinfo = Realloc(m_fieldinfo, m_nfields, m_nfields + 1);
				m_fieldinfo[m_nfields] = info[i];
				m_nfields++;
			}
		}
		IComparer comparer = new TagCompare();
		Array.Sort(m_fieldinfo, 0, m_nfields, comparer);
	}

	public TiffFieldInfo FindFieldInfo(TiffTag tag, TiffType type)
	{
		if (m_foundfield != null && m_foundfield.Tag == tag && (type == TiffType.NOTYPE || type == m_foundfield.Type))
		{
			return m_foundfield;
		}
		if (m_fieldinfo == null)
		{
			return null;
		}
		m_foundfield = null;
		TiffFieldInfo[] fieldinfo = m_fieldinfo;
		foreach (TiffFieldInfo tiffFieldInfo in fieldinfo)
		{
			if (tiffFieldInfo != null && tiffFieldInfo.Tag == tag && (type == TiffType.NOTYPE || type == tiffFieldInfo.Type))
			{
				m_foundfield = tiffFieldInfo;
				break;
			}
		}
		return m_foundfield;
	}

	public TiffFieldInfo FindFieldInfoByName(string name, TiffType type)
	{
		if (m_foundfield != null && m_foundfield.Name == name && (type == TiffType.NOTYPE || type == m_foundfield.Type))
		{
			return m_foundfield;
		}
		if (m_fieldinfo == null)
		{
			return null;
		}
		m_foundfield = null;
		TiffFieldInfo[] fieldinfo = m_fieldinfo;
		foreach (TiffFieldInfo tiffFieldInfo in fieldinfo)
		{
			if (tiffFieldInfo != null && tiffFieldInfo.Name == name && (type == TiffType.NOTYPE || type == tiffFieldInfo.Type))
			{
				m_foundfield = tiffFieldInfo;
				break;
			}
		}
		return m_foundfield;
	}

	public TiffFieldInfo FieldWithTag(TiffTag tag)
	{
		TiffFieldInfo tiffFieldInfo = FindFieldInfo(tag, TiffType.NOTYPE);
		if (tiffFieldInfo != null)
		{
			return tiffFieldInfo;
		}
		ErrorExt(this, m_clientdata, "FieldWithTag", "Internal error, unknown tag 0x{0:x}", tag);
		return null;
	}

	public TiffFieldInfo FieldWithName(string name)
	{
		TiffFieldInfo tiffFieldInfo = FindFieldInfoByName(name, TiffType.NOTYPE);
		if (tiffFieldInfo != null)
		{
			return tiffFieldInfo;
		}
		ErrorExt(this, m_clientdata, "FieldWithName", "Internal error, unknown tag {0}", name);
		return null;
	}

	public TiffTagMethods GetTagMethods()
	{
		return m_tagmethods;
	}

	public TiffTagMethods SetTagMethods(TiffTagMethods methods)
	{
		TiffTagMethods tagmethods = m_tagmethods;
		if (methods != null)
		{
			m_tagmethods = methods;
		}
		return tagmethods;
	}

	public object GetClientInfo(string name)
	{
		clientInfoLink clientInfoLink = m_clientinfo;
		while (clientInfoLink != null && clientInfoLink.name != name)
		{
			clientInfoLink = clientInfoLink.next;
		}
		return clientInfoLink?.data;
	}

	public void SetClientInfo(object data, string name)
	{
		clientInfoLink clientInfoLink = m_clientinfo;
		while (clientInfoLink != null && clientInfoLink.name != name)
		{
			clientInfoLink = clientInfoLink.next;
		}
		if (clientInfoLink != null)
		{
			clientInfoLink.data = data;
			return;
		}
		clientInfoLink = new clientInfoLink();
		clientInfoLink.next = m_clientinfo;
		clientInfoLink.name = name;
		clientInfoLink.data = data;
		m_clientinfo = clientInfoLink;
	}

	public bool Flush()
	{
		if (m_mode != 0)
		{
			if (!FlushData())
			{
				return false;
			}
			if ((m_flags & TiffFlags.DIRTYDIRECT) == TiffFlags.DIRTYDIRECT && !WriteDirectory())
			{
				return false;
			}
		}
		return true;
	}

	public bool FlushData()
	{
		if ((m_flags & TiffFlags.BEENWRITING) != TiffFlags.BEENWRITING)
		{
			return false;
		}
		if ((m_flags & TiffFlags.POSTENCODE) == TiffFlags.POSTENCODE)
		{
			m_flags &= ~TiffFlags.POSTENCODE;
			if (!m_currentCodec.PostEncode())
			{
				return false;
			}
		}
		return flushData1();
	}

	public FieldValue[] GetField(TiffTag tag)
	{
		TiffFieldInfo tiffFieldInfo = FindFieldInfo(tag, TiffType.NOTYPE);
		if (tiffFieldInfo != null && (isPseudoTag(tag) || fieldSet(tiffFieldInfo.Bit)))
		{
			return m_tagmethods.GetField(this, tag);
		}
		return null;
	}

	public FieldValue[] GetFieldDefaulted(TiffTag tag)
	{
		TiffDirectory dir = m_dir;
		FieldValue[] array = GetField(tag);
		if (array != null)
		{
			return array;
		}
		switch (tag)
		{
		case TiffTag.SUBFILETYPE:
			array = new FieldValue[1];
			array[0].Set(dir.td_subfiletype);
			break;
		case TiffTag.BITSPERSAMPLE:
			array = new FieldValue[1];
			array[0].Set(dir.td_bitspersample);
			break;
		case TiffTag.THRESHHOLDING:
			array = new FieldValue[1];
			array[0].Set(dir.td_threshholding);
			break;
		case TiffTag.FILLORDER:
			array = new FieldValue[1];
			array[0].Set(dir.td_fillorder);
			break;
		case TiffTag.ORIENTATION:
			array = new FieldValue[1];
			array[0].Set(dir.td_orientation);
			break;
		case TiffTag.SAMPLESPERPIXEL:
			array = new FieldValue[1];
			array[0].Set(dir.td_samplesperpixel);
			break;
		case TiffTag.ROWSPERSTRIP:
			array = new FieldValue[1];
			array[0].Set(dir.td_rowsperstrip);
			break;
		case TiffTag.MINSAMPLEVALUE:
			array = new FieldValue[1];
			array[0].Set(dir.td_minsamplevalue);
			break;
		case TiffTag.MAXSAMPLEVALUE:
			array = new FieldValue[1];
			array[0].Set(dir.td_maxsamplevalue);
			break;
		case TiffTag.PLANARCONFIG:
			array = new FieldValue[1];
			array[0].Set(dir.td_planarconfig);
			break;
		case TiffTag.RESOLUTIONUNIT:
			array = new FieldValue[1];
			array[0].Set(dir.td_resolutionunit);
			break;
		case TiffTag.PREDICTOR:
			if (m_currentCodec is CodecWithPredictor codecWithPredictor)
			{
				array = new FieldValue[1];
				array[0].Set(codecWithPredictor.GetPredictorValue());
			}
			break;
		case TiffTag.DOTRANGE:
			array = new FieldValue[2];
			array[0].Set(0);
			array[1].Set((1 << (int)dir.td_bitspersample) - 1);
			break;
		case TiffTag.INKSET:
			array = new FieldValue[1];
			array[0].Set(InkSet.CMYK);
			break;
		case TiffTag.NUMBEROFINKS:
			array = new FieldValue[1];
			array[0].Set(4);
			break;
		case TiffTag.EXTRASAMPLES:
			array = new FieldValue[2];
			array[0].Set(dir.td_extrasamples);
			array[1].Set(dir.td_sampleinfo);
			break;
		case TiffTag.MATTEING:
			array = new FieldValue[1];
			array[0].Set(dir.td_extrasamples == 1 && dir.td_sampleinfo[0] == ExtraSample.ASSOCALPHA);
			break;
		case TiffTag.TILEDEPTH:
			array = new FieldValue[1];
			array[0].Set(dir.td_tiledepth);
			break;
		case TiffTag.DATATYPE:
			array = new FieldValue[1];
			array[0].Set(dir.td_sampleformat - 1);
			break;
		case TiffTag.SAMPLEFORMAT:
			array = new FieldValue[1];
			array[0].Set(dir.td_sampleformat);
			break;
		case TiffTag.IMAGEDEPTH:
			array = new FieldValue[1];
			array[0].Set(dir.td_imagedepth);
			break;
		case TiffTag.YCBCRCOEFFICIENTS:
		{
			float[] o2 = new float[3] { 0.299f, 0.587f, 0.114f };
			array = new FieldValue[1];
			array[0].Set(o2);
			break;
		}
		case TiffTag.YCBCRSUBSAMPLING:
			array = new FieldValue[2];
			array[0].Set(dir.td_ycbcrsubsampling[0]);
			array[1].Set(dir.td_ycbcrsubsampling[1]);
			break;
		case TiffTag.YCBCRPOSITIONING:
			array = new FieldValue[1];
			array[0].Set(dir.td_ycbcrpositioning);
			break;
		case TiffTag.WHITEPOINT:
		{
			float[] o = new float[2] { 0.34574193f, 0.35856044f };
			array = new FieldValue[1];
			array[0].Set(o);
			break;
		}
		case TiffTag.TRANSFERFUNCTION:
			if (dir.td_transferfunction[0] == null && !defaultTransferFunction(dir))
			{
				ErrorExt(this, m_clientdata, m_name, "No space for \"TransferFunction\" tag");
				return null;
			}
			array = new FieldValue[3];
			array[0].Set(dir.td_transferfunction[0]);
			if (dir.td_samplesperpixel - dir.td_extrasamples > 1)
			{
				array[1].Set(dir.td_transferfunction[1]);
				array[2].Set(dir.td_transferfunction[2]);
			}
			break;
		case TiffTag.REFERENCEBLACKWHITE:
			if (dir.td_refblackwhite == null)
			{
				defaultRefBlackWhite(dir);
			}
			array = new FieldValue[1];
			array[0].Set(dir.td_refblackwhite);
			break;
		}
		return array;
	}

	public bool ReadDirectory()
	{
		m_diroff = m_nextdiroff;
		if (m_diroff == 0L)
		{
			return false;
		}
		if (!checkDirOffset(m_nextdiroff))
		{
			return false;
		}
		m_currentCodec.Cleanup();
		m_curdir++;
		TiffDirEntry[] pdir;
		ulong num = fetchDirectory(m_nextdiroff, out pdir, out m_nextdiroff);
		if (num == 0L)
		{
			ErrorExt(this, m_clientdata, "ReadDirectory", "{0}: Failed to read directory at offset {1}", m_name, m_nextdiroff);
			return false;
		}
		m_flags &= ~TiffFlags.BEENWRITING;
		FreeDirectory();
		setupDefaultDirectory();
		SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG);
		for (ulong num2 = 0uL; num2 < num; num2++)
		{
			TiffDirEntry tiffDirEntry = pdir[num2];
			if ((m_flags & TiffFlags.SWAB) == TiffFlags.SWAB)
			{
				short value = (short)tiffDirEntry.tdir_tag;
				SwabShort(ref value);
				tiffDirEntry.tdir_tag = (TiffTag)(ushort)value;
				value = (short)tiffDirEntry.tdir_type;
				SwabShort(ref value);
				tiffDirEntry.tdir_type = (TiffType)value;
				SwabLong(ref tiffDirEntry.tdir_count);
				SwabBigTiffValue(ref tiffDirEntry.tdir_offset, m_header.tiff_version == 43, isShort: false);
			}
			if (tiffDirEntry.tdir_tag == TiffTag.SAMPLESPERPIXEL)
			{
				if (!fetchNormalTag(pdir[num2]))
				{
					return false;
				}
				tiffDirEntry.tdir_tag = TiffTag.IGNORE;
			}
		}
		int i = 0;
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		for (ulong num3 = 0uL; num3 < num; num3++)
		{
			if (pdir[num3].tdir_tag == TiffTag.IGNORE)
			{
				continue;
			}
			if (i >= m_nfields)
			{
				i = 0;
			}
			if (pdir[num3].tdir_tag < m_fieldinfo[i].Tag)
			{
				if (!flag)
				{
					WarningExt(this, m_clientdata, "ReadDirectory", "{0}: invalid TIFF directory; tags are not sorted in ascending order", m_name);
					flag = true;
				}
				i = 0;
			}
			for (; i < m_nfields && m_fieldinfo[i].Tag < pdir[num3].tdir_tag; i++)
			{
			}
			if (i >= m_nfields || m_fieldinfo[i].Tag != pdir[num3].tdir_tag)
			{
				flag2 = true;
				continue;
			}
			if (m_fieldinfo[i].Bit == 0)
			{
				pdir[num3].tdir_tag = TiffTag.IGNORE;
				continue;
			}
			TiffFieldInfo tiffFieldInfo = m_fieldinfo[i];
			bool flag4 = false;
			while (pdir[num3].tdir_type != tiffFieldInfo.Type && i < m_nfields && tiffFieldInfo.Type != 0)
			{
				tiffFieldInfo = m_fieldinfo[++i];
				if (i >= m_nfields || tiffFieldInfo.Tag != pdir[num3].tdir_tag)
				{
					WarningExt(this, m_clientdata, "ReadDirectory", "{0}: wrong data type {1} for \"{2}\"; tag ignored", m_name, pdir[num3].tdir_type, m_fieldinfo[i - 1].Name);
					pdir[num3].tdir_tag = TiffTag.IGNORE;
					flag4 = true;
					break;
				}
			}
			if (flag4)
			{
				continue;
			}
			if (tiffFieldInfo.ReadCount != -1 && tiffFieldInfo.ReadCount != -3)
			{
				int count = tiffFieldInfo.ReadCount;
				if (tiffFieldInfo.ReadCount == -2)
				{
					count = m_dir.td_samplesperpixel;
				}
				if (!checkDirCount(pdir[num3], count))
				{
					pdir[num3].tdir_tag = TiffTag.IGNORE;
					continue;
				}
			}
			switch (pdir[num3].tdir_tag)
			{
			case TiffTag.COMPRESSION:
				if (pdir[num3].tdir_count == 1)
				{
					int v = (int)extractData(pdir[num3]);
					flag3 = checkJpegIsOJpeg(ref v, pdir, num);
					if (!SetField(pdir[num3].tdir_tag, v))
					{
						return false;
					}
					break;
				}
				if (pdir[num3].tdir_type == TiffType.LONG)
				{
					if (!fetchPerSampleLongs(pdir[num3], out var pl))
					{
						return false;
					}
					flag3 = checkJpegIsOJpeg(ref pl, pdir, num);
					if (!SetField(pdir[num3].tdir_tag, pl))
					{
						return false;
					}
				}
				else
				{
					if (!fetchPerSampleShorts(pdir[num3], out var pl2))
					{
						return false;
					}
					int v2 = pl2;
					flag3 = checkJpegIsOJpeg(ref v2, pdir, num);
					if (!SetField(pdir[num3].tdir_tag, (short)v2))
					{
						return false;
					}
				}
				pdir[num3].tdir_tag = TiffTag.IGNORE;
				break;
			case TiffTag.STRIPOFFSETS:
			case TiffTag.STRIPBYTECOUNTS:
			case TiffTag.TILEOFFSETS:
			case TiffTag.TILEBYTECOUNTS:
				setFieldBit(tiffFieldInfo.Bit);
				break;
			case TiffTag.IMAGEWIDTH:
			case TiffTag.IMAGELENGTH:
			case TiffTag.ROWSPERSTRIP:
			case TiffTag.PLANARCONFIG:
			case TiffTag.TILEWIDTH:
			case TiffTag.TILELENGTH:
			case TiffTag.EXTRASAMPLES:
			case TiffTag.IMAGEDEPTH:
			case TiffTag.TILEDEPTH:
				if (!fetchNormalTag(pdir[num3]))
				{
					return false;
				}
				pdir[num3].tdir_tag = TiffTag.IGNORE;
				break;
			}
		}
		if (flag2)
		{
			i = 0;
			for (ulong num4 = 0uL; num4 < num; num4++)
			{
				if (pdir[num4].tdir_tag == TiffTag.IGNORE)
				{
					continue;
				}
				if (i >= m_nfields || pdir[num4].tdir_tag < m_fieldinfo[i].Tag)
				{
					i = 0;
				}
				for (; i < m_nfields && m_fieldinfo[i].Tag < pdir[num4].tdir_tag; i++)
				{
				}
				if (i >= m_nfields || m_fieldinfo[i].Tag != pdir[num4].tdir_tag)
				{
					WarningExt(this, m_clientdata, "ReadDirectory", "{0}: unknown field with tag {1} (0x{2:x}) encountered", m_name, (ushort)pdir[num4].tdir_tag, (ushort)pdir[num4].tdir_tag);
					MergeFieldInfo(new TiffFieldInfo[1] { createAnonFieldInfo(pdir[num4].tdir_tag, pdir[num4].tdir_type) }, 1);
					for (i = 0; i < m_nfields && m_fieldinfo[i].Tag < pdir[num4].tdir_tag; i++)
					{
					}
				}
				TiffFieldInfo tiffFieldInfo2 = m_fieldinfo[i];
				while (pdir[num4].tdir_type != tiffFieldInfo2.Type && i < m_nfields && tiffFieldInfo2.Type != 0)
				{
					tiffFieldInfo2 = m_fieldinfo[++i];
					if (i >= m_nfields || tiffFieldInfo2.Tag != pdir[num4].tdir_tag)
					{
						WarningExt(this, m_clientdata, "ReadDirectory", "{0}: wrong data type {1} for \"{2}\"; tag ignored", m_name, pdir[num4].tdir_type, m_fieldinfo[i - 1].Name);
						pdir[num4].tdir_tag = TiffTag.IGNORE;
						break;
					}
				}
			}
		}
		if (m_dir.td_compression == Compression.OJPEG && m_dir.td_planarconfig == PlanarConfig.SEPARATE)
		{
			long num5 = readDirectoryFind(pdir, num, TiffTag.STRIPOFFSETS);
			if (num5 != -1 && pdir[num5].tdir_count == 1)
			{
				num5 = readDirectoryFind(pdir, num, TiffTag.STRIPBYTECOUNTS);
				if (num5 != -1 && pdir[num5].tdir_count == 1)
				{
					m_dir.td_planarconfig = PlanarConfig.CONTIG;
					WarningExt(this, m_clientdata, "ReadDirectory", "Planarconfig tag value assumed incorrect, assuming data is contig instead of chunky");
				}
			}
		}
		if (!fieldSet(1))
		{
			missingRequired("ImageLength");
			return false;
		}
		if (!fieldSet(2))
		{
			m_dir.td_nstrips = NumberOfStrips();
			m_dir.td_tilewidth = m_dir.td_imagewidth;
			m_dir.td_tilelength = m_dir.td_rowsperstrip;
			m_dir.td_tiledepth = m_dir.td_imagedepth;
			m_flags &= ~TiffFlags.ISTILED;
		}
		else
		{
			m_dir.td_nstrips = NumberOfTiles();
			m_flags |= TiffFlags.ISTILED;
		}
		if (m_dir.td_nstrips == 0)
		{
			ErrorExt(this, m_clientdata, "ReadDirectory", "{0}: cannot handle zero number of {1}", m_name, IsTiled() ? "tiles" : "strips");
			return false;
		}
		m_dir.td_stripsperimage = m_dir.td_nstrips;
		if (m_dir.td_planarconfig == PlanarConfig.SEPARATE)
		{
			m_dir.td_stripsperimage /= m_dir.td_samplesperpixel;
		}
		if (!fieldSet(25))
		{
			if (m_dir.td_compression != Compression.OJPEG || IsTiled() || m_dir.td_nstrips != 1)
			{
				missingRequired(IsTiled() ? "TileOffsets" : "StripOffsets");
				return false;
			}
			setFieldBit(25);
		}
		for (ulong num6 = 0uL; num6 < num; num6++)
		{
			if (pdir[num6].tdir_tag == TiffTag.IGNORE)
			{
				continue;
			}
			switch (pdir[num6].tdir_tag)
			{
			case TiffTag.BITSPERSAMPLE:
			case TiffTag.MINSAMPLEVALUE:
			case TiffTag.MAXSAMPLEVALUE:
			case TiffTag.SAMPLEFORMAT:
			case TiffTag.DATATYPE:
			{
				short pl4;
				if (pdir[num6].tdir_count == 1)
				{
					int num7 = (int)extractData(pdir[num6]);
					if (!SetField(pdir[num6].tdir_tag, num7))
					{
						return false;
					}
				}
				else if (pdir[num6].tdir_tag == TiffTag.BITSPERSAMPLE && pdir[num6].tdir_type == TiffType.LONG)
				{
					if (!fetchPerSampleLongs(pdir[num6], out var pl3) || !SetField(pdir[num6].tdir_tag, pl3))
					{
						return false;
					}
				}
				else if (!fetchPerSampleShorts(pdir[num6], out pl4) || !SetField(pdir[num6].tdir_tag, pl4))
				{
					return false;
				}
				break;
			}
			case TiffTag.SMINSAMPLEVALUE:
			case TiffTag.SMAXSAMPLEVALUE:
			{
				if (!fetchPerSampleAnys(pdir[num6], out var pl5) || !SetField(pdir[num6].tdir_tag, pl5))
				{
					return false;
				}
				break;
			}
			case TiffTag.STRIPOFFSETS:
			case TiffTag.TILEOFFSETS:
				if (!fetchStripThing(pdir[num6], m_dir.td_nstrips, ref m_dir.td_stripoffset))
				{
					return false;
				}
				break;
			case TiffTag.STRIPBYTECOUNTS:
			case TiffTag.TILEBYTECOUNTS:
				if (!fetchStripThing(pdir[num6], m_dir.td_nstrips, ref m_dir.td_stripbytecount))
				{
					return false;
				}
				break;
			case TiffTag.TRANSFERFUNCTION:
			case TiffTag.COLORMAP:
			{
				int num8 = 1 << (int)m_dir.td_bitspersample;
				if ((pdir[num6].tdir_tag == TiffTag.COLORMAP || pdir[num6].tdir_count != num8) && !checkDirCount(pdir[num6], 3 * num8))
				{
					break;
				}
				byte[] buffer = new byte[pdir[num6].tdir_count * 2];
				if (fetchData(pdir[num6], buffer) != 0)
				{
					int num9 = 1 << (int)m_dir.td_bitspersample;
					if (pdir[num6].tdir_count == num9)
					{
						short[] array = ByteArrayToShorts(buffer, 0, pdir[num6].tdir_count * 2);
						SetField(pdir[num6].tdir_tag, array, array, array);
					}
					else
					{
						num8 *= 2;
						short[] array2 = ByteArrayToShorts(buffer, 0, num8);
						short[] array3 = ByteArrayToShorts(buffer, num8, num8);
						short[] array4 = ByteArrayToShorts(buffer, 2 * num8, num8);
						SetField(pdir[num6].tdir_tag, array2, array3, array4);
					}
				}
				break;
			}
			case TiffTag.PAGENUMBER:
			case TiffTag.HALFTONEHINTS:
			case TiffTag.DOTRANGE:
			case TiffTag.YCBCRSUBSAMPLING:
				fetchShortPair(pdir[num6]);
				break;
			case TiffTag.REFERENCEBLACKWHITE:
				fetchRefBlackWhite(pdir[num6]);
				break;
			case TiffTag.OSUBFILETYPE:
			{
				FileType fileType = (FileType)0;
				switch ((OFileType)extractData(pdir[num6]))
				{
				case OFileType.REDUCEDIMAGE:
					fileType = FileType.REDUCEDIMAGE;
					break;
				case OFileType.PAGE:
					fileType = FileType.PAGE;
					break;
				}
				if (fileType != 0)
				{
					SetField(TiffTag.SUBFILETYPE, fileType);
				}
				break;
			}
			case TiffTag.COMPRESSION:
				if (!flag3)
				{
					fetchNormalTag(pdir[num6]);
				}
				break;
			default:
				fetchNormalTag(pdir[num6]);
				break;
			}
		}
		if (m_dir.td_compression == Compression.OJPEG)
		{
			if (!fieldSet(8))
			{
				WarningExt(this, m_clientdata, "ReadDirectory", "Photometric tag is missing, assuming data is YCbCr");
				if (!SetField(TiffTag.PHOTOMETRIC, Photometric.YCBCR))
				{
					return false;
				}
			}
			else if (m_dir.td_photometric == Photometric.RGB)
			{
				m_dir.td_photometric = Photometric.YCBCR;
				WarningExt(this, m_clientdata, "ReadDirectory", "Photometric tag value assumed incorrect, assuming data is YCbCr instead of RGB");
			}
			if (!fieldSet(6))
			{
				WarningExt(this, m_clientdata, "ReadDirectory", "BitsPerSample tag is missing, assuming 8 bits per sample");
				if (!SetField(TiffTag.BITSPERSAMPLE, 8))
				{
					return false;
				}
			}
			if (!fieldSet(16))
			{
				if (m_dir.td_photometric == Photometric.RGB || m_dir.td_photometric == Photometric.YCBCR)
				{
					WarningExt(this, m_clientdata, "ReadDirectory", "SamplesPerPixel tag is missing, assuming correct SamplesPerPixel value is 3");
					if (!SetField(TiffTag.SAMPLESPERPIXEL, 3))
					{
						return false;
					}
				}
				else if (m_dir.td_photometric == Photometric.MINISWHITE || m_dir.td_photometric == Photometric.MINISBLACK)
				{
					WarningExt(this, m_clientdata, "ReadDirectory", "SamplesPerPixel tag is missing, assuming correct SamplesPerPixel value is 1");
					if (!SetField(TiffTag.SAMPLESPERPIXEL, 1))
					{
						return false;
					}
				}
			}
		}
		if (m_dir.td_photometric == Photometric.PALETTE && !fieldSet(26))
		{
			missingRequired("Colormap");
			return false;
		}
		if (m_dir.td_compression != Compression.OJPEG)
		{
			if (!fieldSet(24))
			{
				if ((m_dir.td_planarconfig == PlanarConfig.CONTIG && m_dir.td_nstrips > 1) || (m_dir.td_planarconfig == PlanarConfig.SEPARATE && m_dir.td_nstrips != m_dir.td_samplesperpixel))
				{
					missingRequired("StripByteCounts");
					return false;
				}
				WarningExt(this, m_clientdata, "ReadDirectory", "{0}: TIFF directory is missing required \"{1}\" field, calculating from imagelength", m_name, FieldWithTag(TiffTag.STRIPBYTECOUNTS).Name);
				if (!estimateStripByteCounts(pdir, (long)num))
				{
					return false;
				}
			}
			else if (m_dir.td_nstrips == 1 && m_dir.td_stripoffset[0] != 0L && byteCountLooksBad(m_dir))
			{
				WarningExt(this, m_clientdata, "ReadDirectory", "{0}: Bogus \"{1}\" field, ignoring and calculating from imagelength", m_name, FieldWithTag(TiffTag.STRIPBYTECOUNTS).Name);
				if (!estimateStripByteCounts(pdir, (long)num))
				{
					return false;
				}
			}
			else if (m_dir.td_planarconfig == PlanarConfig.CONTIG && m_dir.td_nstrips > 2 && m_dir.td_compression == Compression.NONE && m_dir.td_stripbytecount[0] != m_dir.td_stripbytecount[1])
			{
				WarningExt(this, m_clientdata, "ReadDirectory", "{0}: Wrong \"{1}\" field, ignoring and calculating from imagelength", m_name, FieldWithTag(TiffTag.STRIPBYTECOUNTS).Name);
				if (!estimateStripByteCounts(pdir, (long)num))
				{
					return false;
				}
			}
		}
		pdir = null;
		if (!fieldSet(19))
		{
			m_dir.td_maxsamplevalue = (ushort)((1 << (int)m_dir.td_bitspersample) - 1);
		}
		if (m_dir.td_nstrips > 1)
		{
			m_dir.td_stripbytecountsorted = true;
			for (int j = 1; j < m_dir.td_nstrips; j++)
			{
				if (m_dir.td_stripoffset[j - 1] > m_dir.td_stripoffset[j])
				{
					m_dir.td_stripbytecountsorted = false;
					break;
				}
			}
		}
		if (!fieldSet(7))
		{
			SetField(TiffTag.COMPRESSION, Compression.NONE);
		}
		if (m_dir.td_nstrips == 1 && m_dir.td_compression == Compression.NONE && (m_flags & TiffFlags.STRIPCHOP) == TiffFlags.STRIPCHOP && (m_flags & TiffFlags.ISTILED) != TiffFlags.ISTILED)
		{
			chopUpSingleUncompressedStrip();
		}
		m_row = -1;
		m_curstrip = -1;
		m_col = -1;
		m_curtile = -1;
		m_tilesize = -1;
		m_scanlinesize = ScanlineSize();
		if (m_scanlinesize == 0)
		{
			ErrorExt(this, m_clientdata, "ReadDirectory", "{0}: cannot handle zero scanline size", m_name);
			return false;
		}
		if (IsTiled())
		{
			m_tilesize = TileSize();
			if (m_tilesize == 0)
			{
				ErrorExt(this, m_clientdata, "ReadDirectory", "{0}: cannot handle zero tile size", m_name);
				return false;
			}
		}
		else if (StripSize() == 0)
		{
			ErrorExt(this, m_clientdata, "ReadDirectory", "{0}: cannot handle zero strip size", m_name);
			return false;
		}
		return true;
	}

	public bool ReadCustomDirectory(long offset, TiffFieldInfo[] info, int count)
	{
		setupFieldInfo(info, count);
		TiffDirEntry[] pdir;
		ulong nextdiroff;
		ulong num = fetchDirectory((ulong)offset, out pdir, out nextdiroff);
		if (num == 0L)
		{
			ErrorExt(this, m_clientdata, "ReadCustomDirectory", "{0}: Failed to read custom directory at offset {1}", m_name, offset);
			return false;
		}
		FreeDirectory();
		m_dir = new TiffDirectory();
		int i = 0;
		for (ulong num2 = 0uL; num2 < num; num2++)
		{
			if ((m_flags & TiffFlags.SWAB) == TiffFlags.SWAB)
			{
				short value = (short)pdir[num2].tdir_tag;
				SwabShort(ref value);
				pdir[num2].tdir_tag = (TiffTag)(ushort)value;
				value = (short)pdir[num2].tdir_type;
				SwabShort(ref value);
				pdir[num2].tdir_type = (TiffType)value;
				SwabLong(ref pdir[num2].tdir_count);
				SwabBigTiffValue(ref pdir[num2].tdir_offset, m_header.tiff_version == 43, isShort: false);
			}
			if (i >= m_nfields || pdir[num2].tdir_tag == TiffTag.IGNORE)
			{
				continue;
			}
			for (; i < m_nfields && m_fieldinfo[i].Tag < pdir[num2].tdir_tag; i++)
			{
			}
			if (i >= m_nfields || m_fieldinfo[i].Tag != pdir[num2].tdir_tag)
			{
				WarningExt(this, m_clientdata, "ReadCustomDirectory", "{0}: unknown field with tag {1} (0x{2:x}) encountered", m_name, (ushort)pdir[num2].tdir_tag, (ushort)pdir[num2].tdir_tag);
				MergeFieldInfo(new TiffFieldInfo[1] { createAnonFieldInfo(pdir[num2].tdir_tag, pdir[num2].tdir_type) }, 1);
				for (i = 0; i < m_nfields && m_fieldinfo[i].Tag < pdir[num2].tdir_tag; i++)
				{
				}
			}
			if (m_fieldinfo[i].Bit == 0)
			{
				pdir[num2].tdir_tag = TiffTag.IGNORE;
				continue;
			}
			TiffFieldInfo tiffFieldInfo = m_fieldinfo[i];
			while (pdir[num2].tdir_type != tiffFieldInfo.Type && i < m_nfields && tiffFieldInfo.Type != 0)
			{
				tiffFieldInfo = m_fieldinfo[++i];
				if (i >= m_nfields || tiffFieldInfo.Tag != pdir[num2].tdir_tag)
				{
					WarningExt(this, m_clientdata, "ReadCustomDirectory", "{0}: wrong data type {1} for \"{2}\"; tag ignored", m_name, pdir[num2].tdir_type, m_fieldinfo[i - 1].Name);
					pdir[num2].tdir_tag = TiffTag.IGNORE;
				}
			}
			if (tiffFieldInfo.ReadCount != -1 && tiffFieldInfo.ReadCount != -3)
			{
				int count2 = tiffFieldInfo.ReadCount;
				if (tiffFieldInfo.ReadCount == -2)
				{
					count2 = m_dir.td_samplesperpixel;
				}
				if (!checkDirCount(pdir[num2], count2))
				{
					pdir[num2].tdir_tag = TiffTag.IGNORE;
					continue;
				}
			}
			if (pdir[num2].tdir_tag == TiffTag.EXIF_SUBJECTDISTANCE)
			{
				fetchSubjectDistance(pdir[num2]);
			}
			else
			{
				fetchNormalTag(pdir[num2]);
			}
		}
		return true;
	}

	public bool ReadEXIFDirectory(long offset)
	{
		int size;
		TiffFieldInfo[] info = getExifFieldInfo(out size);
		return ReadCustomDirectory(offset, info, size);
	}

	public int ScanlineSize()
	{
		int nmemb;
		if (m_dir.td_planarconfig == PlanarConfig.CONTIG)
		{
			if (m_dir.td_photometric == Photometric.YCBCR && !IsUpSampled())
			{
				short num = GetFieldDefaulted(TiffTag.YCBCRSUBSAMPLING)[0].ToShort();
				if (num == 0)
				{
					ErrorExt(this, m_clientdata, m_name, "Invalid YCbCr subsampling");
					return 0;
				}
				nmemb = roundUp(m_dir.td_imagewidth, num);
				nmemb = howMany8(multiply(nmemb, m_dir.td_bitspersample, "ScanlineSize"));
				return summarize(nmemb, multiply(2, nmemb / num, "VStripSize"), "VStripSize");
			}
			nmemb = multiply(m_dir.td_imagewidth, m_dir.td_samplesperpixel, "ScanlineSize");
		}
		else
		{
			nmemb = m_dir.td_imagewidth;
		}
		return howMany8(multiply(nmemb, m_dir.td_bitspersample, "ScanlineSize"));
	}

	public int RasterScanlineSize()
	{
		int num = multiply(m_dir.td_bitspersample, m_dir.td_imagewidth, "RasterScanlineSize");
		if (m_dir.td_planarconfig == PlanarConfig.CONTIG)
		{
			num = multiply(num, m_dir.td_samplesperpixel, "RasterScanlineSize");
			return howMany8(num);
		}
		return multiply(howMany8(num), m_dir.td_samplesperpixel, "RasterScanlineSize");
	}

	public int DefaultStripSize(int estimate)
	{
		return m_currentCodec.DefStripSize(estimate);
	}

	public int StripSize()
	{
		int num = m_dir.td_rowsperstrip;
		if (num > m_dir.td_imagelength)
		{
			num = m_dir.td_imagelength;
		}
		return VStripSize(num);
	}

	public int VStripSize(int rowCount)
	{
		if (rowCount == -1)
		{
			rowCount = m_dir.td_imagelength;
		}
		if (m_dir.td_planarconfig == PlanarConfig.CONTIG && m_dir.td_photometric == Photometric.YCBCR && !IsUpSampled())
		{
			FieldValue[] fieldDefaulted = GetFieldDefaulted(TiffTag.YCBCRSUBSAMPLING);
			short num = fieldDefaulted[0].ToShort();
			short num2 = fieldDefaulted[1].ToShort();
			int num3 = num * num2;
			if (num3 == 0)
			{
				ErrorExt(this, m_clientdata, m_name, "Invalid YCbCr subsampling");
				return 0;
			}
			int nmemb = roundUp(m_dir.td_imagewidth, num);
			int elem_size = howMany8(multiply(nmemb, m_dir.td_bitspersample, "VStripSize"));
			rowCount = roundUp(rowCount, num2);
			elem_size = multiply(rowCount, elem_size, "VStripSize");
			return summarize(elem_size, multiply(2, elem_size / num3, "VStripSize"), "VStripSize");
		}
		return multiply(rowCount, ScanlineSize(), "VStripSize");
	}

	public long RawStripSize(int strip)
	{
		long num = (long)m_dir.td_stripbytecount[strip];
		if (num <= 0)
		{
			ErrorExt(this, m_clientdata, m_name, "{0}: Invalid strip byte count, strip {1}", num, strip);
			num = -1L;
		}
		return num;
	}

	public int ComputeStrip(int row, short plane)
	{
		int num = 0;
		if (m_dir.td_rowsperstrip != -1)
		{
			num = row / m_dir.td_rowsperstrip;
		}
		if (m_dir.td_planarconfig == PlanarConfig.SEPARATE)
		{
			if (plane >= m_dir.td_samplesperpixel)
			{
				ErrorExt(this, m_clientdata, m_name, "{0}: Sample out of range, max {1}", plane, m_dir.td_samplesperpixel);
				return 0;
			}
			num += plane * m_dir.td_stripsperimage;
		}
		return num;
	}

	public int NumberOfStrips()
	{
		int num = ((m_dir.td_rowsperstrip == -1) ? 1 : howMany(m_dir.td_imagelength, m_dir.td_rowsperstrip));
		if (m_dir.td_planarconfig == PlanarConfig.SEPARATE)
		{
			num = multiply(num, m_dir.td_samplesperpixel, "NumberOfStrips");
		}
		return num;
	}

	public void DefaultTileSize(ref int width, ref int height)
	{
		m_currentCodec.DefTileSize(ref width, ref height);
	}

	public int TileSize()
	{
		return VTileSize(m_dir.td_tilelength);
	}

	public int VTileSize(int rowCount)
	{
		if (m_dir.td_tilelength == 0 || m_dir.td_tilewidth == 0 || m_dir.td_tiledepth == 0)
		{
			return 0;
		}
		int num2;
		if (m_dir.td_planarconfig == PlanarConfig.CONTIG && m_dir.td_photometric == Photometric.YCBCR && !IsUpSampled())
		{
			int nmemb = roundUp(m_dir.td_tilewidth, m_dir.td_ycbcrsubsampling[0]);
			int elem_size = howMany8(multiply(nmemb, m_dir.td_bitspersample, "VTileSize"));
			int num = m_dir.td_ycbcrsubsampling[0] * m_dir.td_ycbcrsubsampling[1];
			if (num == 0)
			{
				ErrorExt(this, m_clientdata, m_name, "Invalid YCbCr subsampling");
				return 0;
			}
			rowCount = roundUp(rowCount, m_dir.td_ycbcrsubsampling[1]);
			num2 = multiply(rowCount, elem_size, "VTileSize");
			num2 = summarize(num2, multiply(2, num2 / num, "VTileSize"), "VTileSize");
		}
		else
		{
			num2 = multiply(rowCount, TileRowSize(), "VTileSize");
		}
		return multiply(num2, m_dir.td_tiledepth, "VTileSize");
	}

	public long RawTileSize(int tile)
	{
		return RawStripSize(tile);
	}

	public int TileRowSize()
	{
		if (m_dir.td_tilelength == 0 || m_dir.td_tilewidth == 0)
		{
			return 0;
		}
		int num = multiply(m_dir.td_bitspersample, m_dir.td_tilewidth, "TileRowSize");
		if (m_dir.td_planarconfig == PlanarConfig.CONTIG)
		{
			num = multiply(num, m_dir.td_samplesperpixel, "TileRowSize");
		}
		return howMany8(num);
	}

	public int ComputeTile(int x, int y, int z, short plane)
	{
		if (m_dir.td_imagedepth == 1)
		{
			z = 0;
		}
		int num = m_dir.td_tilewidth;
		if (num == -1)
		{
			num = m_dir.td_imagewidth;
		}
		int num2 = m_dir.td_tilelength;
		if (num2 == -1)
		{
			num2 = m_dir.td_imagelength;
		}
		int num3 = m_dir.td_tiledepth;
		if (num3 == -1)
		{
			num3 = m_dir.td_imagedepth;
		}
		int result = 1;
		if (num != 0 && num2 != 0 && num3 != 0)
		{
			int num4 = howMany(m_dir.td_imagewidth, num);
			int num5 = howMany(m_dir.td_imagelength, num2);
			int num6 = howMany(m_dir.td_imagedepth, num3);
			result = ((m_dir.td_planarconfig != PlanarConfig.SEPARATE) ? (num4 * num5 * (z / num3) + num4 * (y / num2) + x / num) : (num4 * num5 * num6 * plane + num4 * num5 * (z / num3) + num4 * (y / num2) + x / num));
		}
		return result;
	}

	public bool CheckTile(int x, int y, int z, short plane)
	{
		if (x >= m_dir.td_imagewidth)
		{
			ErrorExt(this, m_clientdata, m_name, "{0}: Col out of range, max {1}", x, m_dir.td_imagewidth - 1);
			return false;
		}
		if (y >= m_dir.td_imagelength)
		{
			ErrorExt(this, m_clientdata, m_name, "{0}: Row out of range, max {1}", y, m_dir.td_imagelength - 1);
			return false;
		}
		if (z >= m_dir.td_imagedepth)
		{
			ErrorExt(this, m_clientdata, m_name, "{0}: Depth out of range, max {1}", z, m_dir.td_imagedepth - 1);
			return false;
		}
		if (m_dir.td_planarconfig == PlanarConfig.SEPARATE && plane >= m_dir.td_samplesperpixel)
		{
			ErrorExt(this, m_clientdata, m_name, "{0}: Sample out of range, max {1}", plane, m_dir.td_samplesperpixel - 1);
			return false;
		}
		return true;
	}

	public int NumberOfTiles()
	{
		int num = m_dir.td_tilewidth;
		if (num == -1)
		{
			num = m_dir.td_imagewidth;
		}
		int num2 = m_dir.td_tilelength;
		if (num2 == -1)
		{
			num2 = m_dir.td_imagelength;
		}
		int num3 = m_dir.td_tiledepth;
		if (num3 == -1)
		{
			num3 = m_dir.td_imagedepth;
		}
		int num4 = ((num != 0 && num2 != 0 && num3 != 0) ? multiply(multiply(howMany(m_dir.td_imagewidth, num), howMany(m_dir.td_imagelength, num2), "NumberOfTiles"), howMany(m_dir.td_imagedepth, num3), "NumberOfTiles") : 0);
		if (m_dir.td_planarconfig == PlanarConfig.SEPARATE)
		{
			num4 = multiply(num4, m_dir.td_samplesperpixel, "NumberOfTiles");
		}
		return num4;
	}

	public object Clientdata()
	{
		return m_clientdata;
	}

	public object SetClientdata(object data)
	{
		object clientdata = m_clientdata;
		m_clientdata = data;
		return clientdata;
	}

	public int GetMode()
	{
		return m_mode;
	}

	public int SetMode(int mode)
	{
		int mode2 = m_mode;
		m_mode = mode;
		return mode2;
	}

	public bool IsTiled()
	{
		return (m_flags & TiffFlags.ISTILED) == TiffFlags.ISTILED;
	}

	public bool IsByteSwapped()
	{
		return (m_flags & TiffFlags.SWAB) == TiffFlags.SWAB;
	}

	public bool IsUpSampled()
	{
		return (m_flags & TiffFlags.UPSAMPLED) == TiffFlags.UPSAMPLED;
	}

	public bool IsMSB2LSB()
	{
		return isFillOrder(FillOrder.MSB2LSB);
	}

	public bool IsBigEndian()
	{
		return m_header.tiff_magic == 19789;
	}

	public TiffStream GetStream()
	{
		return m_stream;
	}

	public int CurrentRow()
	{
		return m_row;
	}

	public short CurrentDirectory()
	{
		return m_curdir;
	}

	public short NumberOfDirectories()
	{
		ulong nextdir = m_header.tiff_diroff;
		short num = 0;
		HashSet<ulong> hashSet = new HashSet<ulong>();
		hashSet.Add(nextdir);
		long off;
		while (nextdir != 0L && advanceDirectory(ref nextdir, out off))
		{
			if (hashSet.Contains(nextdir))
			{
				throw new InvalidDataException("Loop detected while getting number of directories");
			}
			hashSet.Add(nextdir);
			num++;
		}
		return num;
	}

	public long CurrentDirOffset()
	{
		return (long)m_diroff;
	}

	public int CurrentStrip()
	{
		return m_curstrip;
	}

	public int CurrentTile()
	{
		return m_curtile;
	}

	public void ReadBufferSetup(byte[] buffer, int size)
	{
		m_rawdata = null;
		if (buffer != null)
		{
			m_rawdatasize = size;
			m_rawdata = buffer;
			m_flags &= ~TiffFlags.MYBUFFER;
			return;
		}
		m_rawdatasize = roundUp(size, 1024);
		if (m_rawdatasize > 0)
		{
			m_rawdata = new byte[m_rawdatasize];
		}
		else
		{
			ErrorExt(this, m_clientdata, "ReadBufferSetup", "{0}: No space for data buffer at scanline {1}", m_name, m_row);
			m_rawdatasize = 0;
		}
		m_flags |= TiffFlags.MYBUFFER;
	}

	public void WriteBufferSetup(byte[] buffer, int size)
	{
		if (m_rawdata != null)
		{
			if ((m_flags & TiffFlags.MYBUFFER) == TiffFlags.MYBUFFER)
			{
				m_flags &= ~TiffFlags.MYBUFFER;
			}
			m_rawdata = null;
		}
		if (size == -1)
		{
			size = (IsTiled() ? m_tilesize : StripSize());
			if (size < 8192)
			{
				size = 8192;
			}
			buffer = null;
		}
		if (buffer == null)
		{
			buffer = new byte[size];
			m_flags |= TiffFlags.MYBUFFER;
		}
		else
		{
			m_flags &= ~TiffFlags.MYBUFFER;
		}
		m_rawdata = buffer;
		m_rawdatasize = size;
		m_rawcc = 0;
		m_rawcp = 0;
		m_flags |= TiffFlags.BUFFERSETUP;
	}

	public bool SetupStrips()
	{
		if (IsTiled())
		{
			m_dir.td_stripsperimage = (isUnspecified(2) ? m_dir.td_samplesperpixel : NumberOfTiles());
		}
		else
		{
			m_dir.td_stripsperimage = (isUnspecified(17) ? m_dir.td_samplesperpixel : NumberOfStrips());
		}
		m_dir.td_nstrips = m_dir.td_stripsperimage;
		if (m_dir.td_planarconfig == PlanarConfig.SEPARATE)
		{
			m_dir.td_stripsperimage /= m_dir.td_samplesperpixel;
		}
		m_dir.td_stripoffset = new ulong[m_dir.td_nstrips];
		m_dir.td_stripbytecount = new ulong[m_dir.td_nstrips];
		setFieldBit(25);
		setFieldBit(24);
		return true;
	}

	public bool WriteCheck(bool tiles, string method)
	{
		if (m_mode == 0)
		{
			ErrorExt(this, m_clientdata, method, "{0}: File not open for writing", m_name);
			return false;
		}
		if (tiles ^ IsTiled())
		{
			ErrorExt(this, m_clientdata, m_name, tiles ? "Can not write tiles to a stripped image" : "Can not write scanlines to a tiled image");
			return false;
		}
		if (!fieldSet(1))
		{
			ErrorExt(this, m_clientdata, method, "{0}: Must set \"ImageWidth\" before writing data", m_name);
			return false;
		}
		if (m_dir.td_samplesperpixel == 1)
		{
			if (!fieldSet(20))
			{
				m_dir.td_planarconfig = PlanarConfig.CONTIG;
			}
		}
		else if (!fieldSet(20))
		{
			ErrorExt(this, m_clientdata, method, "{0}: Must set \"PlanarConfiguration\" before writing data", m_name);
			return false;
		}
		if (m_dir.td_stripoffset == null && !SetupStrips())
		{
			m_dir.td_nstrips = 0;
			ErrorExt(this, m_clientdata, method, "{0}: No space for {1} arrays", m_name, IsTiled() ? "tile" : "strip");
			return false;
		}
		m_tilesize = (IsTiled() ? TileSize() : (-1));
		m_scanlinesize = ScanlineSize();
		m_flags |= TiffFlags.BEENWRITING;
		return true;
	}

	public void FreeDirectory()
	{
		if (m_dir != null)
		{
			clearFieldBit(39);
			clearFieldBit(40);
			m_dir = null;
		}
	}

	public void CreateDirectory()
	{
		setupDefaultDirectory();
		m_diroff = 0uL;
		m_nextdiroff = 0uL;
		m_curoff = 0uL;
		m_row = -1;
		m_curstrip = -1;
	}

	public bool LastDirectory()
	{
		return m_nextdiroff == 0;
	}

	public bool SetDirectory(short number)
	{
		ulong nextdir = m_header.tiff_diroff;
		short num = number;
		while (num > 0 && nextdir != 0L)
		{
			if (!advanceDirectory(ref nextdir, out var _))
			{
				return false;
			}
			num--;
		}
		m_nextdiroff = nextdir;
		m_curdir = (short)(number - num - 1);
		m_dirnumber = 0;
		return ReadDirectory();
	}

	public bool SetSubDirectory(long offset)
	{
		m_nextdiroff = (ulong)offset;
		m_dirnumber = 0;
		return ReadDirectory();
	}

	public bool UnlinkDirectory(short number)
	{
		resetPenultimateDirectoryOffset();
		if (m_mode == 0)
		{
			ErrorExt(this, m_clientdata, "UnlinkDirectory", "Can not unlink directory in read-only file");
			return false;
		}
		ulong nextdir = m_header.tiff_diroff;
		long off = 4L;
		for (int num = number - 1; num > 0; num--)
		{
			if (nextdir == 0L)
			{
				ErrorExt(this, m_clientdata, "UnlinkDirectory", "Directory {0} does not exist", number);
				return false;
			}
			if (!advanceDirectory(ref nextdir, out off))
			{
				return false;
			}
		}
		if (!advanceDirectory(ref nextdir, out var _))
		{
			return false;
		}
		seekFile(off, SeekOrigin.Begin);
		if ((m_flags & TiffFlags.SWAB) == TiffFlags.SWAB)
		{
			SwabBigTiffValue(ref nextdir, m_header.tiff_version == 43, isShort: false);
		}
		if (!writeIntOK((int)nextdir))
		{
			ErrorExt(this, m_clientdata, "UnlinkDirectory", "Error writing directory link");
			return false;
		}
		m_currentCodec.Cleanup();
		if ((m_flags & TiffFlags.MYBUFFER) == TiffFlags.MYBUFFER && m_rawdata != null)
		{
			m_rawdata = null;
			m_rawcc = 0;
		}
		m_flags &= ~(TiffFlags.BUFFERSETUP | TiffFlags.BEENWRITING | TiffFlags.POSTENCODE);
		FreeDirectory();
		setupDefaultDirectory();
		m_diroff = 0uL;
		m_nextdiroff = 0uL;
		m_curoff = 0uL;
		m_row = -1;
		m_curstrip = -1;
		return true;
	}

	public bool SetField(TiffTag tag, params object[] value)
	{
		if (okToChangeTag(tag))
		{
			return m_tagmethods.SetField(this, tag, FieldValue.FromParams(value));
		}
		return false;
	}

	public bool WriteDirectory()
	{
		return writeDirectory(done: true);
	}

	public bool CheckpointDirectory()
	{
		if (m_dir.td_stripoffset == null)
		{
			SetupStrips();
		}
		bool result = writeDirectory(done: false);
		SetWriteOffset(seekFile(0L, SeekOrigin.End));
		return result;
	}

	public bool RewriteDirectory()
	{
		if (m_diroff == 0L)
		{
			return WriteDirectory();
		}
		resetPenultimateDirectoryOffset();
		if (m_header.tiff_diroff == m_diroff)
		{
			m_header.tiff_diroff = 0uL;
			m_diroff = 0uL;
			seekFile(4L, SeekOrigin.Begin);
			if (!writeDirOffOK((long)m_header.tiff_diroff, m_header.tiff_version == 43))
			{
				ErrorExt(this, m_clientdata, m_name, "Error updating TIFF header");
				return false;
			}
		}
		else
		{
			ulong value = m_header.tiff_diroff;
			do
			{
				if (!seekOK((long)value) || !readDirCountOK(out var value2, m_header.tiff_version == 43))
				{
					ErrorExt(this, m_clientdata, "RewriteDirectory", "Error fetching directory count");
					return false;
				}
				if ((m_flags & TiffFlags.SWAB) == TiffFlags.SWAB)
				{
					SwabBigTiffValue(ref value2, m_header.tiff_version == 43, isShort: true);
				}
				seekFile((long)value2 * (long)TiffDirEntry.SizeInBytes(m_header.tiff_version == 43), SeekOrigin.Current);
				if (m_header.tiff_version == 43)
				{
					if (!readUlongOK(out value))
					{
						ErrorExt(this, m_clientdata, "RewriteDirectory", "Error fetching directory link");
						return false;
					}
				}
				else
				{
					if (!readUIntOK(out var value3))
					{
						ErrorExt(this, m_clientdata, "RewriteDirectory", "Error fetching directory link");
						return false;
					}
					value = value3;
				}
				if ((m_flags & TiffFlags.SWAB) == TiffFlags.SWAB)
				{
					SwabBigTiffValue(ref value, m_header.tiff_version == 43, isShort: false);
				}
			}
			while (value != m_diroff && value != 0L);
			long num = seekFile(0L, SeekOrigin.Current);
			if (m_header.tiff_version == 43)
			{
				seekFile(num - 8, SeekOrigin.Begin);
			}
			else
			{
				seekFile(num - 4, SeekOrigin.Begin);
			}
			m_diroff = 0uL;
			if (!writeDirOffOK((long)m_diroff, m_header.tiff_version == 43))
			{
				ErrorExt(this, m_clientdata, "RewriteDirectory", "Error writing directory link");
				return false;
			}
		}
		return WriteDirectory();
	}

	public void PrintDirectory(Stream stream)
	{
		PrintDirectory(stream, TiffPrintFlags.NONE);
	}

	public void PrintDirectory(Stream stream, TiffPrintFlags flags)
	{
		fprintf(stream, "TIFF Directory at offset 0x{0:x} ({1})\r\n", m_diroff, m_diroff);
		if (fieldSet(5))
		{
			fprintf(stream, "  Subfile Type:");
			string text = " ";
			if ((m_dir.td_subfiletype & FileType.REDUCEDIMAGE) != 0)
			{
				fprintf(stream, "{0}reduced-resolution image", text);
				text = "/";
			}
			if ((m_dir.td_subfiletype & FileType.PAGE) != 0)
			{
				fprintf(stream, "{0}multi-page document", text);
				text = "/";
			}
			if ((m_dir.td_subfiletype & FileType.MASK) != 0)
			{
				fprintf(stream, "{0}transparency mask", text);
			}
			fprintf(stream, " ({0} = 0x{1:x})\r\n", m_dir.td_subfiletype, m_dir.td_subfiletype);
		}
		if (fieldSet(1))
		{
			fprintf(stream, "  Image Width: {0} Image Length: {1}", m_dir.td_imagewidth, m_dir.td_imagelength);
			if (fieldSet(35))
			{
				fprintf(stream, " Image Depth: {0}", m_dir.td_imagedepth);
			}
			fprintf(stream, "\r\n");
		}
		if (fieldSet(2))
		{
			fprintf(stream, "  Tile Width: {0} Tile Length: {1}", m_dir.td_tilewidth, m_dir.td_tilelength);
			if (fieldSet(36))
			{
				fprintf(stream, " Tile Depth: {0}", m_dir.td_tiledepth);
			}
			fprintf(stream, "\r\n");
		}
		if (fieldSet(3))
		{
			fprintf(stream, "  Resolution: {0:G}, {1:G}", m_dir.td_xresolution, m_dir.td_yresolution);
			if (fieldSet(22))
			{
				switch (m_dir.td_resolutionunit)
				{
				case ResUnit.NONE:
					fprintf(stream, " (unitless)");
					break;
				case ResUnit.INCH:
					fprintf(stream, " pixels/inch");
					break;
				case ResUnit.CENTIMETER:
					fprintf(stream, " pixels/cm");
					break;
				default:
					fprintf(stream, " (unit {0} = 0x{1:x})", m_dir.td_resolutionunit, m_dir.td_resolutionunit);
					break;
				}
			}
			fprintf(stream, "\r\n");
		}
		if (fieldSet(4))
		{
			fprintf(stream, "  Position: {0:G}, {1:G}\r\n", m_dir.td_xposition, m_dir.td_yposition);
		}
		if (fieldSet(6))
		{
			fprintf(stream, "  Bits/Sample: {0}\r\n", m_dir.td_bitspersample);
		}
		if (fieldSet(32))
		{
			fprintf(stream, "  Sample Format: ");
			switch (m_dir.td_sampleformat)
			{
			case SampleFormat.VOID:
				fprintf(stream, "void\r\n");
				break;
			case SampleFormat.INT:
				fprintf(stream, "signed integer\r\n");
				break;
			case SampleFormat.UINT:
				fprintf(stream, "unsigned integer\r\n");
				break;
			case SampleFormat.IEEEFP:
				fprintf(stream, "IEEE floating point\r\n");
				break;
			case SampleFormat.COMPLEXINT:
				fprintf(stream, "complex signed integer\r\n");
				break;
			case SampleFormat.COMPLEXIEEEFP:
				fprintf(stream, "complex IEEE floating point\r\n");
				break;
			default:
				fprintf(stream, "{0} (0x{1:x})\r\n", m_dir.td_sampleformat, m_dir.td_sampleformat);
				break;
			}
		}
		if (fieldSet(7))
		{
			TiffCodec tiffCodec = FindCodec(m_dir.td_compression);
			fprintf(stream, "  Compression Scheme: ");
			if (tiffCodec != null)
			{
				fprintf(stream, "{0}\r\n", tiffCodec.m_name);
			}
			else
			{
				fprintf(stream, "{0} (0x{1:x})\r\n", m_dir.td_compression, m_dir.td_compression);
			}
		}
		if (fieldSet(8))
		{
			fprintf(stream, "  Photometric Interpretation: ");
			if ((int)m_dir.td_photometric < photoNames.Length)
			{
				fprintf(stream, "{0}\r\n", photoNames[(int)m_dir.td_photometric]);
			}
			else
			{
				switch (m_dir.td_photometric)
				{
				case Photometric.LOGL:
					fprintf(stream, "CIE Log2(L)\r\n");
					break;
				case Photometric.LOGLUV:
					fprintf(stream, "CIE Log2(L) (u',v')\r\n");
					break;
				default:
					fprintf(stream, "{0} (0x{1:x})\r\n", m_dir.td_photometric, m_dir.td_photometric);
					break;
				}
			}
		}
		if (fieldSet(31) && m_dir.td_extrasamples != 0)
		{
			fprintf(stream, "  Extra Samples: {0}<", m_dir.td_extrasamples);
			string text2 = string.Empty;
			for (short num = 0; num < m_dir.td_extrasamples; num++)
			{
				switch (m_dir.td_sampleinfo[num])
				{
				case ExtraSample.UNSPECIFIED:
					fprintf(stream, "{0}unspecified", text2);
					break;
				case ExtraSample.ASSOCALPHA:
					fprintf(stream, "{0}assoc-alpha", text2);
					break;
				case ExtraSample.UNASSALPHA:
					fprintf(stream, "{0}unassoc-alpha", text2);
					break;
				default:
					fprintf(stream, "{0}{1} (0x{2:x})", text2, m_dir.td_sampleinfo[num], m_dir.td_sampleinfo[num]);
					break;
				}
				text2 = ", ";
			}
			fprintf(stream, ">\r\n");
		}
		if (fieldSet(46))
		{
			fprintf(stream, "  Ink Names: ");
			string[] array = m_dir.td_inknames.Split(new char[1]);
			for (int i = 0; i < array.Length; i++)
			{
				printAscii(stream, array[i]);
				fprintf(stream, ", ");
			}
			fprintf(stream, "\r\n");
		}
		if (fieldSet(9))
		{
			fprintf(stream, "  Thresholding: ");
			switch (m_dir.td_threshholding)
			{
			case Threshold.BILEVEL:
				fprintf(stream, "bilevel art scan\r\n");
				break;
			case Threshold.HALFTONE:
				fprintf(stream, "halftone or dithered scan\r\n");
				break;
			case Threshold.ERRORDIFFUSE:
				fprintf(stream, "error diffused\r\n");
				break;
			default:
				fprintf(stream, "{0} (0x{1:x})\r\n", m_dir.td_threshholding, m_dir.td_threshholding);
				break;
			}
		}
		if (fieldSet(10))
		{
			fprintf(stream, "  FillOrder: ");
			switch (m_dir.td_fillorder)
			{
			case FillOrder.MSB2LSB:
				fprintf(stream, "msb-to-lsb\r\n");
				break;
			case FillOrder.LSB2MSB:
				fprintf(stream, "lsb-to-msb\r\n");
				break;
			default:
				fprintf(stream, "{0} (0x{1:x})\r\n", m_dir.td_fillorder, m_dir.td_fillorder);
				break;
			}
		}
		if (fieldSet(39))
		{
			FieldValue[] field = GetField(TiffTag.YCBCRSUBSAMPLING);
			short num2 = field[0].ToShort();
			short num3 = field[1].ToShort();
			fprintf(stream, "  YCbCr Subsampling: {0}, {1}\r\n", num2, num3);
		}
		if (fieldSet(40))
		{
			fprintf(stream, "  YCbCr Positioning: ");
			switch (m_dir.td_ycbcrpositioning)
			{
			case YCbCrPosition.CENTERED:
				fprintf(stream, "centered\r\n");
				break;
			case YCbCrPosition.COSITED:
				fprintf(stream, "cosited\r\n");
				break;
			default:
				fprintf(stream, "{0} (0x{1:x})\r\n", m_dir.td_ycbcrpositioning, m_dir.td_ycbcrpositioning);
				break;
			}
		}
		if (fieldSet(37))
		{
			fprintf(stream, "  Halftone Hints: light {0} dark {1}\r\n", m_dir.td_halftonehints[0], m_dir.td_halftonehints[1]);
		}
		if (fieldSet(15))
		{
			fprintf(stream, "  Orientation: ");
			if ((int)m_dir.td_orientation < orientNames.Length)
			{
				fprintf(stream, "{0}\r\n", orientNames[(int)m_dir.td_orientation]);
			}
			else
			{
				fprintf(stream, "{0} (0x{1:x})\r\n", m_dir.td_orientation, m_dir.td_orientation);
			}
		}
		if (fieldSet(16))
		{
			fprintf(stream, "  Samples/Pixel: {0}\r\n", m_dir.td_samplesperpixel);
		}
		if (fieldSet(17))
		{
			fprintf(stream, "  Rows/Strip: ");
			if (m_dir.td_rowsperstrip == -1)
			{
				fprintf(stream, "(infinite)\r\n");
			}
			else
			{
				fprintf(stream, "{0}\r\n", m_dir.td_rowsperstrip);
			}
		}
		if (fieldSet(18))
		{
			fprintf(stream, "  Min Sample Value: {0}\r\n", m_dir.td_minsamplevalue);
		}
		if (fieldSet(19))
		{
			fprintf(stream, "  Max Sample Value: {0}\r\n", m_dir.td_maxsamplevalue);
		}
		if (fieldSet(33))
		{
			fprintf(stream, "  SMin Sample Value: {0:G}\r\n", m_dir.td_sminsamplevalue);
		}
		if (fieldSet(34))
		{
			fprintf(stream, "  SMax Sample Value: {0:G}\r\n", m_dir.td_smaxsamplevalue);
		}
		if (fieldSet(20))
		{
			fprintf(stream, "  Planar Configuration: ");
			switch (m_dir.td_planarconfig)
			{
			case PlanarConfig.CONTIG:
				fprintf(stream, "single image plane\r\n");
				break;
			case PlanarConfig.SEPARATE:
				fprintf(stream, "separate image planes\r\n");
				break;
			default:
				fprintf(stream, "{0} (0x{1:x})\r\n", m_dir.td_planarconfig, m_dir.td_planarconfig);
				break;
			}
		}
		if (fieldSet(23))
		{
			fprintf(stream, "  Page Number: {0}-{1}\r\n", m_dir.td_pagenumber[0], m_dir.td_pagenumber[1]);
		}
		if (fieldSet(26))
		{
			fprintf(stream, "  Color Map: ");
			if ((flags & TiffPrintFlags.COLORMAP) != 0)
			{
				fprintf(stream, string.Empty + "\r\n");
				int num4 = 1 << (int)m_dir.td_bitspersample;
				for (int j = 0; j < num4; j++)
				{
					fprintf(stream, "   {0,5}: {1,5} {2,5} {3,5}\r\n", j, m_dir.td_colormap[0][j], m_dir.td_colormap[1][j], m_dir.td_colormap[2][j]);
				}
			}
			else
			{
				fprintf(stream, "(present)\r\n");
			}
		}
		if (fieldSet(44))
		{
			fprintf(stream, "  Transfer Function: ");
			if ((flags & TiffPrintFlags.CURVES) != 0)
			{
				fprintf(stream, string.Empty + "\r\n");
				int num5 = 1 << (int)m_dir.td_bitspersample;
				for (int k = 0; k < num5; k++)
				{
					fprintf(stream, "    {0,2}: {0,5}", k, m_dir.td_transferfunction[0][k]);
					for (short num6 = 1; num6 < m_dir.td_samplesperpixel; num6++)
					{
						fprintf(stream, " {0,5}", m_dir.td_transferfunction[num6][k]);
					}
					fprintf(stream, string.Empty + "\r\n");
				}
			}
			else
			{
				fprintf(stream, "(present)\r\n");
			}
		}
		if (fieldSet(49) && m_dir.td_subifd != null)
		{
			fprintf(stream, "  SubIFD Offsets:");
			for (short num7 = 0; num7 < m_dir.td_nsubifd; num7++)
			{
				fprintf(stream, " {0,5}", m_dir.td_subifd[num7]);
			}
			fprintf(stream, string.Empty + "\r\n");
		}
		int tagListCount = GetTagListCount();
		for (int l = 0; l < tagListCount; l++)
		{
			TiffTag tagListEntry = (TiffTag)GetTagListEntry(l);
			TiffFieldInfo tiffFieldInfo = FieldWithTag(tagListEntry);
			if (tiffFieldInfo == null)
			{
				continue;
			}
			byte[] array2 = null;
			int num8;
			if (tiffFieldInfo.PassCount)
			{
				FieldValue[] field2 = GetField(tagListEntry);
				if (field2 == null)
				{
					continue;
				}
				num8 = field2[0].ToInt();
				array2 = field2[1].ToByteArray();
			}
			else
			{
				num8 = ((tiffFieldInfo.ReadCount == -1 || tiffFieldInfo.ReadCount == -3) ? 1 : ((tiffFieldInfo.ReadCount != -2) ? tiffFieldInfo.ReadCount : m_dir.td_samplesperpixel));
				if ((tiffFieldInfo.Type == TiffType.ASCII || tiffFieldInfo.ReadCount == -1 || tiffFieldInfo.ReadCount == -3 || tiffFieldInfo.ReadCount == -2 || num8 > 1) && tiffFieldInfo.Tag != TiffTag.PAGENUMBER && tiffFieldInfo.Tag != TiffTag.HALFTONEHINTS && tiffFieldInfo.Tag != TiffTag.YCBCRSUBSAMPLING && tiffFieldInfo.Tag != TiffTag.DOTRANGE)
				{
					FieldValue[] field3 = GetField(tagListEntry);
					if (field3 == null)
					{
						continue;
					}
					array2 = field3[0].ToByteArray();
				}
				else if (tiffFieldInfo.Tag != TiffTag.PAGENUMBER && tiffFieldInfo.Tag != TiffTag.HALFTONEHINTS && tiffFieldInfo.Tag != TiffTag.YCBCRSUBSAMPLING && tiffFieldInfo.Tag != TiffTag.DOTRANGE)
				{
					array2 = new byte[dataSize(tiffFieldInfo.Type) * num8];
					FieldValue[] field4 = GetField(tagListEntry);
					if (field4 == null)
					{
						continue;
					}
					array2 = field4[0].ToByteArray();
				}
				else
				{
					array2 = new byte[dataSize(tiffFieldInfo.Type) * num8];
					FieldValue[] field5 = GetField(tagListEntry);
					if (field5 == null)
					{
						continue;
					}
					byte[] array3 = field5[0].ToByteArray();
					byte[] array4 = field5[1].ToByteArray();
					Buffer.BlockCopy(array3, 0, array2, 0, array3.Length);
					Buffer.BlockCopy(array4, 0, array2, dataSize(tiffFieldInfo.Type), array4.Length);
				}
			}
			if (!prettyPrintField(stream, tagListEntry, num8, array2))
			{
				printField(stream, tiffFieldInfo, num8, array2);
			}
		}
		m_tagmethods.PrintDir(this, stream, flags);
		if ((flags & TiffPrintFlags.STRIPS) != 0 && fieldSet(25))
		{
			fprintf(stream, "  {0} {1}:\r\n", m_dir.td_nstrips, IsTiled() ? "Tiles" : "Strips");
			for (int m = 0; m < m_dir.td_nstrips; m++)
			{
				fprintf(stream, "    {0,3}: [{0,8}, {0,8}]\r\n", m, m_dir.td_stripoffset[m], m_dir.td_stripbytecount[m]);
			}
		}
	}

	public bool ReadScanline(byte[] buffer, int row)
	{
		return ReadScanline(buffer, 0, row, 0);
	}

	public bool ReadScanline(byte[] buffer, int row, short plane)
	{
		return ReadScanline(buffer, 0, row, plane);
	}

	public bool ReadScanline(byte[] buffer, int offset, int row, short plane)
	{
		if (!checkRead(tiles: false))
		{
			return false;
		}
		bool flag = seek(row, plane);
		if (flag)
		{
			flag = m_currentCodec.DecodeRow(buffer, offset, m_scanlinesize, plane);
			m_row = row + 1;
			if (flag)
			{
				postDecode(buffer, offset, m_scanlinesize);
			}
		}
		return flag;
	}

	public bool WriteScanline(byte[] buffer, int row)
	{
		return WriteScanline(buffer, 0, row, 0);
	}

	public bool WriteScanline(byte[] buffer, int row, short plane)
	{
		return WriteScanline(buffer, 0, row, plane);
	}

	public bool WriteScanline(byte[] buffer, int offset, int row, short plane)
	{
		if (!writeCheckStrips("WriteScanline"))
		{
			return false;
		}
		bufferCheck();
		bool flag = false;
		if (row >= m_dir.td_imagelength)
		{
			if (m_dir.td_planarconfig == PlanarConfig.SEPARATE)
			{
				ErrorExt(this, m_clientdata, m_name, "Can not change \"ImageLength\" when using separate planes");
				return false;
			}
			m_dir.td_imagelength = row + 1;
			flag = true;
		}
		int num;
		if (m_dir.td_planarconfig != PlanarConfig.SEPARATE)
		{
			num = ((m_dir.td_rowsperstrip != -1) ? (row / m_dir.td_rowsperstrip) : 0);
		}
		else
		{
			if (plane >= m_dir.td_samplesperpixel)
			{
				ErrorExt(this, m_clientdata, m_name, "{0}: Sample out of range, max {1}", plane, m_dir.td_samplesperpixel);
				return false;
			}
			num = ((m_dir.td_rowsperstrip != -1) ? (plane * m_dir.td_stripsperimage + row / m_dir.td_rowsperstrip) : 0);
		}
		if (num >= m_dir.td_nstrips && !growStrips(1))
		{
			return false;
		}
		if (num != m_curstrip)
		{
			if (!FlushData())
			{
				return false;
			}
			m_curstrip = num;
			if (num >= m_dir.td_stripsperimage && flag)
			{
				m_dir.td_stripsperimage = howMany(m_dir.td_imagelength, m_dir.td_rowsperstrip);
			}
			m_row = num % m_dir.td_stripsperimage * m_dir.td_rowsperstrip;
			if ((m_flags & TiffFlags.CODERSETUP) != TiffFlags.CODERSETUP)
			{
				if (!m_currentCodec.SetupEncode())
				{
					return false;
				}
				m_flags |= TiffFlags.CODERSETUP;
			}
			m_rawcc = 0;
			m_rawcp = 0;
			if (m_dir.td_stripbytecount[num] != 0)
			{
				m_dir.td_stripbytecount[num] = 0uL;
				m_curoff = 0uL;
			}
			if (!m_currentCodec.PreEncode(plane))
			{
				return false;
			}
			m_flags |= TiffFlags.POSTENCODE;
		}
		if (row != m_row)
		{
			if (row < m_row)
			{
				m_row = num % m_dir.td_stripsperimage * m_dir.td_rowsperstrip;
				m_rawcp = 0;
			}
			if (!m_currentCodec.Seek(row - m_row))
			{
				return false;
			}
			m_row = row;
		}
		postDecode(buffer, offset, m_scanlinesize);
		bool result = m_currentCodec.EncodeRow(buffer, offset, m_scanlinesize, plane);
		m_row = row + 1;
		return result;
	}

	public bool ReadRGBAImage(int width, int height, int[] raster)
	{
		return ReadRGBAImage(width, height, raster, stopOnError: false);
	}

	public bool ReadRGBAImage(int width, int height, int[] raster, bool stopOnError)
	{
		return ReadRGBAImageOriented(width, height, raster, Orientation.BOTLEFT, stopOnError);
	}

	public bool ReadRGBAImageOriented(int width, int height, int[] raster, Orientation orientation)
	{
		return ReadRGBAImageOriented(width, height, raster, orientation, stopOnError: false);
	}

	public bool ReadRGBAImageOriented(int width, int height, int[] raster, Orientation orientation, bool stopOnError)
	{
		bool result = false;
		if (RGBAImageOK(out var errorMsg))
		{
			TiffRgbaImage tiffRgbaImage = TiffRgbaImage.Create(this, stopOnError, out errorMsg);
			if (tiffRgbaImage != null)
			{
				tiffRgbaImage.ReqOrientation = orientation;
				result = tiffRgbaImage.GetRaster(raster, (height - tiffRgbaImage.Height) * width, width, tiffRgbaImage.Height);
			}
		}
		else
		{
			ErrorExt(this, m_clientdata, FileName(), "{0}", errorMsg);
			result = false;
		}
		return result;
	}

	public bool ReadRGBAStrip(int row, int[] raster)
	{
		if (IsTiled())
		{
			ErrorExt(this, m_clientdata, FileName(), "Can't use ReadRGBAStrip() with tiled file.");
			return false;
		}
		int num = GetFieldDefaulted(TiffTag.ROWSPERSTRIP)[0].ToInt();
		if (row % num != 0)
		{
			ErrorExt(this, m_clientdata, FileName(), "Row passed to ReadRGBAStrip() must be first in a strip.");
			return false;
		}
		if (RGBAImageOK(out var errorMsg))
		{
			TiffRgbaImage tiffRgbaImage = TiffRgbaImage.Create(this, stopOnError: false, out errorMsg);
			if (tiffRgbaImage != null)
			{
				tiffRgbaImage.row_offset = row;
				tiffRgbaImage.col_offset = 0;
				int height = num;
				if (row + num > tiffRgbaImage.Height)
				{
					height = tiffRgbaImage.Height - row;
				}
				return tiffRgbaImage.GetRaster(raster, 0, tiffRgbaImage.Width, height);
			}
			return true;
		}
		ErrorExt(this, m_clientdata, FileName(), "{0}", errorMsg);
		return false;
	}

	public bool ReadRGBATile(int col, int row, int[] raster)
	{
		if (!IsTiled())
		{
			ErrorExt(this, m_clientdata, FileName(), "Can't use ReadRGBATile() with stripped file.");
			return false;
		}
		int num = GetFieldDefaulted(TiffTag.TILEWIDTH)[0].ToInt();
		int num2 = GetFieldDefaulted(TiffTag.TILELENGTH)[0].ToInt();
		if (col % num != 0 || row % num2 != 0)
		{
			ErrorExt(this, m_clientdata, FileName(), "Row/col passed to ReadRGBATile() must be topleft corner of a tile.");
			return false;
		}
		string errorMsg;
		TiffRgbaImage tiffRgbaImage = TiffRgbaImage.Create(this, stopOnError: false, out errorMsg);
		if (!RGBAImageOK(out errorMsg) || tiffRgbaImage == null)
		{
			ErrorExt(this, m_clientdata, FileName(), "{0}", errorMsg);
			return false;
		}
		int num3 = ((row + num2 <= tiffRgbaImage.Height) ? num2 : (tiffRgbaImage.Height - row));
		int num4 = ((col + num <= tiffRgbaImage.Width) ? num : (tiffRgbaImage.Width - col));
		tiffRgbaImage.row_offset = row;
		tiffRgbaImage.col_offset = col;
		bool raster2 = tiffRgbaImage.GetRaster(raster, 0, num4, num3);
		if (num4 == num && num3 == num2)
		{
			return raster2;
		}
		for (int i = 0; i < num3; i++)
		{
			Buffer.BlockCopy(raster, (num3 - i - 1) * num4 * 4, raster, (num2 - i - 1) * num * 4, num4 * 4);
			Array.Clear(raster, (num2 - i - 1) * num + num4, num - num4);
		}
		for (int j = num3; j < num2; j++)
		{
			Array.Clear(raster, (num2 - j - 1) * num, num);
		}
		return raster2;
	}

	public bool RGBAImageOK(out string errorMsg)
	{
		errorMsg = null;
		if (!m_decodestatus)
		{
			errorMsg = "Sorry, requested compression method is not configured";
			return false;
		}
		switch (m_dir.td_bitspersample)
		{
		default:
			errorMsg = string.Format(CultureInfo.InvariantCulture, "Sorry, can not handle images with {0}-bit samples", m_dir.td_bitspersample);
			return false;
		case 1:
		case 2:
		case 4:
		case 8:
		case 16:
		{
			int num = m_dir.td_samplesperpixel - m_dir.td_extrasamples;
			Photometric photometric = Photometric.RGB;
			FieldValue[] field = GetField(TiffTag.PHOTOMETRIC);
			if (field == null)
			{
				switch (num)
				{
				case 1:
					photometric = Photometric.MINISBLACK;
					break;
				case 3:
					photometric = Photometric.RGB;
					break;
				default:
					errorMsg = string.Format(CultureInfo.InvariantCulture, "Missing needed {0} tag", "PhotometricInterpretation");
					return false;
				}
			}
			else
			{
				photometric = (Photometric)field[0].Value;
			}
			switch (photometric)
			{
			case Photometric.MINISWHITE:
			case Photometric.MINISBLACK:
			case Photometric.PALETTE:
				if (m_dir.td_planarconfig == PlanarConfig.CONTIG && m_dir.td_samplesperpixel != 1 && m_dir.td_bitspersample < 8)
				{
					errorMsg = string.Format(CultureInfo.InvariantCulture, "Sorry, can not handle contiguous data with {0}={1}, and {2}={3} and Bits/Sample={4}", "PhotometricInterpretation", photometric, "Samples/pixel", m_dir.td_samplesperpixel, m_dir.td_bitspersample);
					return false;
				}
				break;
			case Photometric.RGB:
				if (num < 3)
				{
					errorMsg = string.Format(CultureInfo.InvariantCulture, "Sorry, can not handle RGB image with {0}={1}", "Color channels", num);
					return false;
				}
				break;
			case Photometric.SEPARATED:
			{
				field = GetFieldDefaulted(TiffTag.INKSET);
				InkSet inkSet = (InkSet)field[0].ToByte();
				if (inkSet != InkSet.CMYK)
				{
					errorMsg = string.Format(CultureInfo.InvariantCulture, "Sorry, can not handle separated image with {0}={1}", "InkSet", inkSet);
					return false;
				}
				if (m_dir.td_samplesperpixel < 4)
				{
					errorMsg = string.Format(CultureInfo.InvariantCulture, "Sorry, can not handle separated image with {0}={1}", "Samples/pixel", m_dir.td_samplesperpixel);
					return false;
				}
				break;
			}
			case Photometric.LOGL:
				if (m_dir.td_compression != Compression.SGILOG)
				{
					errorMsg = string.Format(CultureInfo.InvariantCulture, "Sorry, LogL data must have {0}={1}", "Compression", Compression.SGILOG);
					return false;
				}
				break;
			case Photometric.LOGLUV:
				if (m_dir.td_compression != Compression.SGILOG && m_dir.td_compression != Compression.SGILOG24)
				{
					errorMsg = string.Format(CultureInfo.InvariantCulture, "Sorry, LogLuv data must have {0}={1} or {2}", "Compression", Compression.SGILOG, Compression.SGILOG24);
					return false;
				}
				if (m_dir.td_planarconfig != PlanarConfig.CONTIG)
				{
					errorMsg = string.Format(CultureInfo.InvariantCulture, "Sorry, can not handle LogLuv images with {0}={1}", "Planarconfiguration", m_dir.td_planarconfig);
					return false;
				}
				break;
			default:
				errorMsg = string.Format(CultureInfo.InvariantCulture, "Sorry, can not handle image with {0}={1}", "PhotometricInterpretation", photometric);
				return false;
			case Photometric.YCBCR:
			case Photometric.CIELAB:
				break;
			}
			return true;
		}
		}
	}

	public string FileName()
	{
		return m_name;
	}

	public string SetFileName(string name)
	{
		string name2 = m_name;
		m_name = name;
		return name2;
	}

	public static void Error(Tiff tif, string method, string format, params object[] args)
	{
		TiffErrorHandler errorHandler = getErrorHandler(tif);
		if (errorHandler != null)
		{
			errorHandler.ErrorHandler(tif, method, format, args);
			errorHandler.ErrorHandlerExt(tif, null, method, format, args);
		}
	}

	public static void Error(string method, string format, params object[] args)
	{
		Error(null, method, format, args);
	}

	public static void ErrorExt(Tiff tif, object clientData, string method, string format, params object[] args)
	{
		TiffErrorHandler errorHandler = getErrorHandler(tif);
		if (errorHandler != null)
		{
			errorHandler.ErrorHandler(tif, method, format, args);
			errorHandler.ErrorHandlerExt(tif, clientData, method, format, args);
		}
	}

	public static void ErrorExt(object clientData, string method, string format, params object[] args)
	{
		ErrorExt(null, clientData, method, format, args);
	}

	public static void Warning(Tiff tif, string method, string format, params object[] args)
	{
		TiffErrorHandler errorHandler = getErrorHandler(tif);
		if (errorHandler != null)
		{
			errorHandler.WarningHandler(tif, method, format, args);
			errorHandler.WarningHandlerExt(tif, null, method, format, args);
		}
	}

	public static void Warning(string method, string format, params object[] args)
	{
		Warning(null, method, format, args);
	}

	public static void WarningExt(Tiff tif, object clientData, string method, string format, params object[] args)
	{
		TiffErrorHandler errorHandler = getErrorHandler(tif);
		if (errorHandler != null)
		{
			errorHandler.WarningHandler(tif, method, format, args);
			errorHandler.WarningHandlerExt(tif, clientData, method, format, args);
		}
	}

	public static void WarningExt(object clientData, string method, string format, params object[] args)
	{
		WarningExt(null, clientData, method, format, args);
	}

	public static TiffErrorHandler SetErrorHandler(TiffErrorHandler errorHandler)
	{
		return setErrorHandlerImpl(errorHandler);
	}

	public static TiffExtendProc SetTagExtender(TiffExtendProc extender)
	{
		return setTagExtenderImpl(extender);
	}

	public int ReadTile(byte[] buffer, int offset, int x, int y, int z, short plane)
	{
		if (!checkRead(tiles: true) || !CheckTile(x, y, z, plane))
		{
			return -1;
		}
		return ReadEncodedTile(ComputeTile(x, y, z, plane), buffer, offset, -1);
	}

	public int ReadEncodedTile(int tile, byte[] buffer, int offset, int count)
	{
		if (!checkRead(tiles: true))
		{
			return -1;
		}
		if (tile >= m_dir.td_nstrips)
		{
			ErrorExt(this, m_clientdata, m_name, "{0}: Tile out of range, max {1}", tile, m_dir.td_nstrips);
			return -1;
		}
		if (count == -1)
		{
			count = m_tilesize;
		}
		else if (count > m_tilesize)
		{
			count = m_tilesize;
		}
		if (fillTile(tile) && m_currentCodec.DecodeTile(buffer, offset, count, (short)(tile / m_dir.td_stripsperimage)))
		{
			postDecode(buffer, offset, count);
			return count;
		}
		return -1;
	}

	public int ReadRawTile(int tile, byte[] buffer, int offset, int count)
	{
		if (!checkRead(tiles: true))
		{
			return -1;
		}
		if (tile >= m_dir.td_nstrips)
		{
			ErrorExt(this, m_clientdata, m_name, "{0}: Tile out of range, max {1}", tile, m_dir.td_nstrips);
			return -1;
		}
		if ((m_flags & TiffFlags.NOREADRAW) == TiffFlags.NOREADRAW)
		{
			ErrorExt(this, m_clientdata, m_name, "Compression scheme does not support access to raw uncompressed data");
			return -1;
		}
		ulong num = m_dir.td_stripbytecount[tile];
		if (count != -1 && (ulong)count < num)
		{
			num = (ulong)count;
		}
		return readRawTile1(tile, buffer, offset, (int)num, "ReadRawTile");
	}

	public int WriteTile(byte[] buffer, int x, int y, int z, short plane)
	{
		return WriteTile(buffer, 0, x, y, z, plane);
	}

	public int WriteTile(byte[] buffer, int offset, int x, int y, int z, short plane)
	{
		if (!CheckTile(x, y, z, plane))
		{
			return -1;
		}
		return WriteEncodedTile(ComputeTile(x, y, z, plane), buffer, offset, -1);
	}

	public int ReadEncodedStrip(int strip, byte[] buffer, int offset, int count)
	{
		if (!checkRead(tiles: false))
		{
			return -1;
		}
		if (strip >= m_dir.td_nstrips)
		{
			ErrorExt(this, m_clientdata, m_name, "{0}: Strip out of range, max {1}", strip, m_dir.td_nstrips);
			return -1;
		}
		int num = ((m_dir.td_rowsperstrip >= m_dir.td_imagelength) ? 1 : ((m_dir.td_imagelength + m_dir.td_rowsperstrip - 1) / m_dir.td_rowsperstrip));
		int num2 = strip % num;
		int num3 = m_dir.td_imagelength % m_dir.td_rowsperstrip;
		if (num2 != num - 1 || num3 == 0)
		{
			num3 = m_dir.td_rowsperstrip;
		}
		int num4 = VStripSize(num3);
		if (count == -1)
		{
			count = num4;
		}
		else if (count > num4)
		{
			count = num4;
		}
		if (fillStrip(strip) && m_currentCodec.DecodeStrip(buffer, offset, count, (short)(strip / m_dir.td_stripsperimage)))
		{
			postDecode(buffer, offset, count);
			return count;
		}
		return -1;
	}

	public int ReadRawStrip(int strip, byte[] buffer, int offset, int count)
	{
		if (!checkRead(tiles: false))
		{
			return -1;
		}
		if (strip >= m_dir.td_nstrips)
		{
			ErrorExt(this, m_clientdata, m_name, "{0}: Strip out of range, max {1}", strip, m_dir.td_nstrips);
			return -1;
		}
		if ((m_flags & TiffFlags.NOREADRAW) == TiffFlags.NOREADRAW)
		{
			ErrorExt(this, m_clientdata, m_name, "Compression scheme does not support access to raw uncompressed data");
			return -1;
		}
		ulong num = m_dir.td_stripbytecount[strip];
		if (num == 0)
		{
			ErrorExt(this, m_clientdata, m_name, "{0}: Invalid strip byte count, strip {1}", num, strip);
			return -1;
		}
		if (count != -1 && (ulong)count < num)
		{
			num = (ulong)count;
		}
		return readRawStrip1(strip, buffer, offset, (int)num, "ReadRawStrip");
	}

	public int WriteEncodedStrip(int strip, byte[] buffer, int count)
	{
		return WriteEncodedStrip(strip, buffer, 0, count);
	}

	public int WriteEncodedStrip(int strip, byte[] buffer, int offset, int count)
	{
		if (!writeCheckStrips("WriteEncodedStrip"))
		{
			return -1;
		}
		if (strip >= m_dir.td_nstrips)
		{
			if (m_dir.td_planarconfig == PlanarConfig.SEPARATE)
			{
				ErrorExt(this, m_clientdata, m_name, "Can not grow image by strips when using separate planes");
				return -1;
			}
			if (!growStrips(1))
			{
				return -1;
			}
			m_dir.td_stripsperimage = howMany(m_dir.td_imagelength, m_dir.td_rowsperstrip);
		}
		bufferCheck();
		m_curstrip = strip;
		m_row = strip % m_dir.td_stripsperimage * m_dir.td_rowsperstrip;
		if ((m_flags & TiffFlags.CODERSETUP) != TiffFlags.CODERSETUP)
		{
			if (!m_currentCodec.SetupEncode())
			{
				return -1;
			}
			m_flags |= TiffFlags.CODERSETUP;
		}
		m_rawcc = 0;
		m_rawcp = 0;
		if (m_dir.td_stripbytecount[strip] != 0)
		{
			m_curoff = 0uL;
		}
		m_flags &= ~TiffFlags.POSTENCODE;
		short plane = (short)(strip / m_dir.td_stripsperimage);
		if (!m_currentCodec.PreEncode(plane))
		{
			return -1;
		}
		postDecode(buffer, offset, count);
		if (!m_currentCodec.EncodeStrip(buffer, offset, count, plane))
		{
			return 0;
		}
		if (!m_currentCodec.PostEncode())
		{
			return -1;
		}
		if (!isFillOrder(m_dir.td_fillorder) && (m_flags & TiffFlags.NOBITREV) != TiffFlags.NOBITREV)
		{
			ReverseBits(m_rawdata, m_rawcc);
		}
		if (m_rawcc > 0 && !appendToStrip(strip, m_rawdata, 0, m_rawcc))
		{
			return -1;
		}
		m_rawcc = 0;
		m_rawcp = 0;
		return count;
	}

	public int WriteRawStrip(int strip, byte[] buffer, int count)
	{
		return WriteRawStrip(strip, buffer, 0, count);
	}

	public int WriteRawStrip(int strip, byte[] buffer, int offset, int count)
	{
		if (!writeCheckStrips("WriteRawStrip"))
		{
			return -1;
		}
		if (strip >= m_dir.td_nstrips)
		{
			if (m_dir.td_planarconfig == PlanarConfig.SEPARATE)
			{
				ErrorExt(this, m_clientdata, m_name, "Can not grow image by strips when using separate planes");
				return -1;
			}
			if (strip >= m_dir.td_stripsperimage)
			{
				m_dir.td_stripsperimage = howMany(m_dir.td_imagelength, m_dir.td_rowsperstrip);
			}
			if (!growStrips(1))
			{
				return -1;
			}
		}
		m_curstrip = strip;
		m_row = strip % m_dir.td_stripsperimage * m_dir.td_rowsperstrip;
		if (!appendToStrip(strip, buffer, offset, count))
		{
			return -1;
		}
		return count;
	}

	public int WriteEncodedTile(int tile, byte[] buffer, int count)
	{
		return WriteEncodedTile(tile, buffer, 0, count);
	}

	public int WriteEncodedTile(int tile, byte[] buffer, int offset, int count)
	{
		if (!writeCheckTiles("WriteEncodedTile"))
		{
			return -1;
		}
		if (tile >= m_dir.td_nstrips)
		{
			ErrorExt(this, m_clientdata, "WriteEncodedTile", "{0}: Tile {1} out of range, max {2}", m_name, tile, m_dir.td_nstrips);
			return -1;
		}
		bufferCheck();
		m_curtile = tile;
		m_rawcc = 0;
		m_rawcp = 0;
		if (m_dir.td_stripbytecount[tile] != 0)
		{
			m_curoff = 0uL;
		}
		m_row = tile % howMany(m_dir.td_imagelength, m_dir.td_tilelength) * m_dir.td_tilelength;
		m_col = tile % howMany(m_dir.td_imagewidth, m_dir.td_tilewidth) * m_dir.td_tilewidth;
		if ((m_flags & TiffFlags.CODERSETUP) != TiffFlags.CODERSETUP)
		{
			if (!m_currentCodec.SetupEncode())
			{
				return -1;
			}
			m_flags |= TiffFlags.CODERSETUP;
		}
		m_flags &= ~TiffFlags.POSTENCODE;
		short plane = (short)(tile / m_dir.td_stripsperimage);
		if (!m_currentCodec.PreEncode(plane))
		{
			return -1;
		}
		if (count < 1 || count > m_tilesize)
		{
			count = m_tilesize;
		}
		postDecode(buffer, offset, count);
		if (!m_currentCodec.EncodeTile(buffer, offset, count, plane))
		{
			return 0;
		}
		if (!m_currentCodec.PostEncode())
		{
			return -1;
		}
		if (!isFillOrder(m_dir.td_fillorder) && (m_flags & TiffFlags.NOBITREV) != TiffFlags.NOBITREV)
		{
			ReverseBits(m_rawdata, m_rawcc);
		}
		if (m_rawcc > 0 && !appendToStrip(tile, m_rawdata, 0, m_rawcc))
		{
			return -1;
		}
		m_rawcc = 0;
		m_rawcp = 0;
		return count;
	}

	public int WriteRawTile(int tile, byte[] buffer, int count)
	{
		return WriteRawTile(tile, buffer, 0, count);
	}

	public int WriteRawTile(int tile, byte[] buffer, int offset, int count)
	{
		if (!writeCheckTiles("WriteRawTile"))
		{
			return -1;
		}
		if (tile >= m_dir.td_nstrips)
		{
			ErrorExt(this, m_clientdata, "WriteRawTile", "{0}: Tile {1} out of range, max {2}", m_name, tile, m_dir.td_nstrips);
			return -1;
		}
		if (!appendToStrip(tile, buffer, offset, count))
		{
			return -1;
		}
		return count;
	}

	public void SetWriteOffset(long offset)
	{
		m_curoff = (uint)offset;
	}

	public static int DataWidth(TiffType type)
	{
		switch (type)
		{
		case TiffType.NOTYPE:
		case TiffType.BYTE:
		case TiffType.ASCII:
		case TiffType.SBYTE:
		case TiffType.UNDEFINED:
			return 1;
		case TiffType.SHORT:
		case TiffType.SSHORT:
			return 2;
		case TiffType.LONG:
		case TiffType.SLONG:
		case TiffType.FLOAT:
		case TiffType.IFD:
			return 4;
		case TiffType.RATIONAL:
		case TiffType.SRATIONAL:
		case TiffType.DOUBLE:
		case TiffType.LONG8:
		case TiffType.SLONG8:
		case TiffType.IFD8:
			return 8;
		default:
			return 0;
		}
	}

	public static void SwabShort(ref short value)
	{
		byte[] array = new byte[2]
		{
			(byte)value,
			(byte)(value >> 8)
		};
		byte b = array[1];
		array[1] = array[0];
		array[0] = b;
		value = (short)(array[0] & 0xFF);
		value += (short)((array[1] & 0xFF) << 8);
	}

	public static void SwabLong(ref int value)
	{
		byte[] array = new byte[4]
		{
			(byte)value,
			(byte)(value >> 8),
			(byte)(value >> 16),
			(byte)(value >> 24)
		};
		byte b = array[3];
		array[3] = array[0];
		array[0] = b;
		b = array[2];
		array[2] = array[1];
		array[1] = b;
		value = array[0] & 0xFF;
		value += (array[1] & 0xFF) << 8;
		value += (array[2] & 0xFF) << 16;
		value += array[3] << 24;
	}

	private static void SwabLong8(ref ulong value)
	{
		byte[] array = new byte[8]
		{
			(byte)value,
			(byte)(value >> 8),
			(byte)(value >> 16),
			(byte)(value >> 24),
			(byte)(value >> 32),
			(byte)(value >> 40),
			(byte)(value >> 48),
			(byte)(value >> 56)
		};
		byte b = array[7];
		array[7] = array[0];
		array[0] = b;
		b = array[6];
		array[6] = array[1];
		array[1] = b;
		b = array[5];
		array[5] = array[2];
		array[2] = b;
		b = array[4];
		array[4] = array[3];
		array[3] = b;
		value = (ulong)array[0] & 0xFFuL;
		value += (ulong)((long)(array[1] & 0xFF) << 8);
		value += (ulong)((long)(array[2] & 0xFF) << 16);
		value += (ulong)((long)(array[3] & 0xFF) << 24);
		value += (ulong)((long)(array[4] & 0xFF) << 32);
		value += (ulong)((long)(array[5] & 0xFF) << 40);
		value += (ulong)((long)(array[6] & 0xFF) << 48);
		value += (ulong)((long)(array[7] & 0xFF) << 56);
	}

	private static void SwabBigTiffValue(ref ulong value, bool isBigTiff, bool isShort)
	{
		if (isBigTiff)
		{
			SwabLong8(ref value);
		}
		else if (isShort)
		{
			short value2 = (short)value;
			SwabShort(ref value2);
			value = (ulong)value2;
		}
		else
		{
			int value3 = (int)value;
			SwabLong(ref value3);
			value = (ulong)value3;
		}
	}

	public static void SwabDouble(ref double value)
	{
		byte[] bytes = BitConverter.GetBytes(value);
		int[] array = new int[2];
		array[0] = BitConverter.ToInt32(bytes, 0);
		array[0] = BitConverter.ToInt32(bytes, 4);
		SwabArrayOfLong(array, 2);
		int num = array[0];
		array[0] = array[1];
		array[1] = num;
		Buffer.BlockCopy(BitConverter.GetBytes(array[0]), 0, bytes, 0, 4);
		Buffer.BlockCopy(BitConverter.GetBytes(array[1]), 0, bytes, 4, 4);
		value = BitConverter.ToDouble(bytes, 0);
	}

	public static void SwabArrayOfShort(short[] array, int count)
	{
		SwabArrayOfShort(array, 0, count);
	}

	public static void SwabArrayOfShort(short[] array, int offset, int count)
	{
		byte[] array2 = new byte[2];
		int num = 0;
		while (num < count)
		{
			array2[0] = (byte)array[offset];
			array2[1] = (byte)(array[offset] >> 8);
			byte b = array2[1];
			array2[1] = array2[0];
			array2[0] = b;
			array[offset] = (short)(array2[0] & 0xFF);
			array[offset] += (short)((array2[1] & 0xFF) << 8);
			num++;
			offset++;
		}
	}

	public static void SwabArrayOfTriples(byte[] array, int count)
	{
		SwabArrayOfTriples(array, 0, count);
	}

	public static void SwabArrayOfTriples(byte[] array, int offset, int count)
	{
		while (count-- > 0)
		{
			byte b = array[offset + 2];
			array[offset + 2] = array[offset];
			array[offset] = b;
			offset += 3;
		}
	}

	public static void SwabArrayOfLong(int[] array, int count)
	{
		SwabArrayOfLong(array, 0, count);
	}

	public static void SwabArrayOfLong8(long[] array, int count)
	{
		SwabArrayOfLong8(array, 0, count);
	}

	public static void SwabArrayOfLong(int[] array, int offset, int count)
	{
		byte[] array2 = new byte[4];
		int num = 0;
		while (num < count)
		{
			array2[0] = (byte)array[offset];
			array2[1] = (byte)(array[offset] >> 8);
			array2[2] = (byte)(array[offset] >> 16);
			array2[3] = (byte)(array[offset] >> 24);
			byte b = array2[3];
			array2[3] = array2[0];
			array2[0] = b;
			b = array2[2];
			array2[2] = array2[1];
			array2[1] = b;
			array[offset] = array2[0] & 0xFF;
			array[offset] += (array2[1] & 0xFF) << 8;
			array[offset] += (array2[2] & 0xFF) << 16;
			array[offset] += array2[3] << 24;
			num++;
			offset++;
		}
	}

	public static void SwabArrayOfLong8(long[] array, int offset, int count)
	{
		byte[] array2 = new byte[8];
		int num = 0;
		while (num < count)
		{
			array2[0] = (byte)array[offset];
			array2[1] = (byte)(array[offset] >> 8);
			array2[2] = (byte)(array[offset] >> 16);
			array2[3] = (byte)(array[offset] >> 24);
			array2[4] = (byte)(array[offset] >> 32);
			array2[5] = (byte)(array[offset] >> 40);
			array2[6] = (byte)(array[offset] >> 48);
			array2[7] = (byte)(array[offset] >> 56);
			byte b = array2[7];
			array2[7] = array2[0];
			array2[0] = b;
			b = array2[6];
			array2[6] = array2[1];
			array2[1] = b;
			b = array2[5];
			array2[5] = array2[2];
			array2[2] = b;
			b = array2[4];
			array2[4] = array2[3];
			array2[3] = b;
			array[offset] = array2[0] & 0xFF;
			array[offset] += (array2[1] & 0xFF) << 8;
			array[offset] += (array2[2] & 0xFF) << 16;
			array[offset] += (array2[3] & 0xFF) << 24;
			array[offset] += array2[4] & 0xFF;
			array[offset] += (array2[5] & 0xFF) << 8;
			array[offset] += (array2[6] & 0xFF) << 16;
			array[offset] += array2[7] << 24;
			num++;
			offset++;
		}
	}

	public static void SwabArrayOfDouble(double[] array, int count)
	{
		SwabArrayOfDouble(array, 0, count);
	}

	public static void SwabArrayOfDouble(double[] array, int offset, int count)
	{
		int[] array2 = new int[count * 8 / 4];
		Buffer.BlockCopy(array, offset * 8, array2, 0, array2.Length * 4);
		SwabArrayOfLong(array2, array2.Length);
		int num = 0;
		while (count-- > 0)
		{
			int num2 = array2[num];
			array2[num] = array2[num + 1];
			array2[num + 1] = num2;
			num += 2;
		}
		Buffer.BlockCopy(array2, 0, array, offset * 8, array2.Length * 4);
	}

	public static void ReverseBits(byte[] buffer, int count)
	{
		ReverseBits(buffer, 0, count);
	}

	public static void ReverseBits(byte[] buffer, int offset, int count)
	{
		while (count > 8)
		{
			buffer[offset] = TIFFBitRevTable[buffer[offset]];
			buffer[offset + 1] = TIFFBitRevTable[buffer[offset + 1]];
			buffer[offset + 2] = TIFFBitRevTable[buffer[offset + 2]];
			buffer[offset + 3] = TIFFBitRevTable[buffer[offset + 3]];
			buffer[offset + 4] = TIFFBitRevTable[buffer[offset + 4]];
			buffer[offset + 5] = TIFFBitRevTable[buffer[offset + 5]];
			buffer[offset + 6] = TIFFBitRevTable[buffer[offset + 6]];
			buffer[offset + 7] = TIFFBitRevTable[buffer[offset + 7]];
			offset += 8;
			count -= 8;
		}
		while (count-- > 0)
		{
			buffer[offset] = TIFFBitRevTable[buffer[offset]];
			offset++;
		}
	}

	public static byte[] GetBitRevTable(bool reversed)
	{
		if (!reversed)
		{
			return TIFFNoBitRevTable;
		}
		return TIFFBitRevTable;
	}

	public static int[] ByteArrayToInts(byte[] buffer, int offset, int count)
	{
		int num = count / 4;
		int[] array = new int[num];
		Buffer.BlockCopy(buffer, offset, array, 0, num * 4);
		return array;
	}

	public static long[] ByteArrayToLong8(byte[] buffer, int offset, int count)
	{
		int num = count / 8;
		long[] array = new long[num];
		Buffer.BlockCopy(buffer, offset, array, 0, num * 8);
		return array;
	}

	public static void Long8ToByteArray(long[] source, int srcOffset, int srcCount, byte[] bytes, int offset)
	{
		Buffer.BlockCopy(source, srcOffset * 8, bytes, offset, srcCount * 8);
	}

	public static void IntsToByteArray(int[] source, int srcOffset, int srcCount, byte[] bytes, int offset)
	{
		Buffer.BlockCopy(source, srcOffset * 4, bytes, offset, srcCount * 4);
	}

	public static short[] ByteArrayToShorts(byte[] buffer, int offset, int count)
	{
		int num = count / 2;
		short[] array = new short[num];
		Buffer.BlockCopy(buffer, offset, array, 0, num * 2);
		return array;
	}

	public static void ShortsToByteArray(short[] source, int srcOffset, int srcCount, byte[] bytes, int offset)
	{
		Buffer.BlockCopy(source, srcOffset * 2, bytes, offset, srcCount * 2);
	}

	private static long[] IntToLong(int[] inputArray)
	{
		long[] array = new long[inputArray.Length];
		for (int i = 0; i < inputArray.Length; i++)
		{
			array[i] = (uint)inputArray[i];
		}
		return array;
	}

	private static uint[] LongToInt(ulong[] inputArray)
	{
		uint[] array = new uint[inputArray.Length];
		for (int i = 0; i < inputArray.Length; i++)
		{
			array[i] = (uint)inputArray[i];
		}
		return array;
	}

	private static int[] LongToInt(long[] inputArray)
	{
		int[] array = new int[inputArray.Length];
		for (int i = 0; i < inputArray.Length; i++)
		{
			array[i] = (int)inputArray[i];
		}
		return array;
	}
}
