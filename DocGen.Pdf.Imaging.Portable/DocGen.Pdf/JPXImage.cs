using System;
using System.Collections.Generic;
using System.IO;
using SkiaSharp;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;

namespace DocGen.Pdf;

internal class JPXImage
{
	private string[][] decoder_pinfo = new string[20][]
	{
		new string[4] { "u", "[on|off]", "", "off" },
		new string[4] { "v", "[on|off]", "", "off" },
		new string[4] { "verbose", "[on|off]", "", "on" },
		new string[4] { "pfile", "", "   ", null },
		new string[4] { "res", "", " ", null },
		new string[4] { "i", "", "", null },
		new string[4] { "o", "", " ", null },
		new string[4] { "rate", "", " ", "-1" },
		new string[4] { "nbytes", "", " ", "-1" },
		new string[4] { "parsing", null, "true, ", "on" },
		new string[4] { "ncb_quit", "", "", "-1" },
		new string[4] { "l_quit", "", "", "-1" },
		new string[4] { "m_quit", "", "", "-1" },
		new string[4] { "poc_quit", null, "", "off" },
		new string[4] { "one_tp", null, "", "off" },
		new string[4] { "comp_transf", null, "", "on" },
		new string[4] { "debug", null, "", "off" },
		new string[4] { "cdstr_info", null, "", "off" },
		new string[4] { "nocolorspace", null, "", "off" },
		new string[4] { "", null, "", "off" }
	};

	internal Image FromStream(Stream stream)
	{
		return CreateImageFromJPXStream(stream) as Image;
	}

	internal Bitmap FromStreamNet(Stream stream)
	{
		return CreateImageFromJPXStream(stream) as Bitmap;
	}

	private object CreateImageFromJPXStream(Stream stream)
	{
		JPXRandomAccessStream jPXRandomAccessStream = new JPXRandomAccessStream(stream, 262144, 262144, int.MaxValue);
		JPXParameters pl = new JPXParameters(GetParameters(decoder_pinfo));
		JPXFormatReader jPXFormatReader = new JPXFormatReader(jPXRandomAccessStream);
		jPXFormatReader.readFileFormat();
		if (jPXFormatReader.JP2FFUsed)
		{
			jPXRandomAccessStream.seek(jPXFormatReader.FirstCodeStreamPos);
		}
		HeaderInformation headerInformation = new HeaderInformation();
		HeaderDecoder headerDecoder = new HeaderDecoder(jPXRandomAccessStream, pl, headerInformation);
		int numComps = headerDecoder.NumComps;
		_ = headerInformation.sizValue.NumTiles;
		DecodeHelper decoderHelper = headerDecoder.DecoderHelper;
		int[] array = new int[numComps];
		for (int i = 0; i < numComps; i++)
		{
			array[i] = headerDecoder.GetActualBitDepth(i);
		}
		BitstreamReader bitstreamReader = BitstreamReader.createInstance(jPXRandomAccessStream, headerDecoder, pl, decoderHelper, cdstrInfo: false, headerInformation);
		EntropyDecoder src = headerDecoder.createEntropyDecoder(bitstreamReader, pl);
		DeScalerROI src2 = headerDecoder.createROIDeScaler(src, pl, decoderHelper);
		WaveletTransformInverse waveletTransformInverse = WaveletTransformInverse.createInstance(headerDecoder.createDequantizer(src2, array, decoderHelper), decoderHelper);
		int imgRes = bitstreamReader.ImgRes;
		waveletTransformInverse.ImgResLevel = imgRes;
		BlockImageDataSource blockImageDataSource = new InverseComponetTransformation(new ImageDataConverter(waveletTransformInverse, 0), decoderHelper, array, pl);
		int numComps2 = blockImageDataSource.NumComps;
		int num = ((numComps2 == 4) ? 4 : 3);
		switch (numComps2)
		{
		default:
			throw new ApplicationException("Unsupported PixelFormat.  " + numComps2 + " components.");
		case 1:
		case 3:
		case 4:
		{
			Bitmap bitmap = null;
			SKBitmap sKBitmap = null;
			bitmap = new Bitmap(blockImageDataSource.ImgWidth, blockImageDataSource.ImgHeight);
			sKBitmap = (bitmap.m_sKBitmap = new SKBitmap(new SKImageInfo(blockImageDataSource.ImgWidth, blockImageDataSource.ImgHeight, SKColorType.Rgb888x)));
			_ = new byte[blockImageDataSource.ImgWidth * numComps2];
			JPXImageCoordinates numTiles = blockImageDataSource.getNumTiles(null);
			int num2 = 0;
			List<byte> list = new List<byte>();
			for (int j = 0; j < numTiles.y; j++)
			{
				int num3 = 0;
				while (num3 < numTiles.x)
				{
					blockImageDataSource.setTile(num3, j);
					int tileComponentHeight = blockImageDataSource.getTileComponentHeight(num2, 0);
					int tileComponentWidth = blockImageDataSource.getTileComponentWidth(num2, 0);
					int num4 = blockImageDataSource.getCompUpperLeftCornerX(0) - (int)Math.Ceiling((double)blockImageDataSource.ImgULX / (double)blockImageDataSource.getCompSubsX(0));
					int num5 = blockImageDataSource.getCompUpperLeftCornerY(0) - (int)Math.Ceiling((double)blockImageDataSource.ImgULY / (double)blockImageDataSource.getCompSubsY(0));
					DataBlockInt[] array2 = new DataBlockInt[numComps2];
					int[] array3 = new int[numComps2];
					int[] array4 = new int[numComps2];
					int[] array5 = new int[numComps2];
					for (int k = 0; k < numComps2; k++)
					{
						array2[k] = new DataBlockInt();
						array3[k] = 1 << blockImageDataSource.getNomRangeBits(0) - 1;
						array4[k] = (1 << blockImageDataSource.getNomRangeBits(0)) - 1;
						array5[k] = blockImageDataSource.getFixedPoint(0);
					}
					for (int l = 0; l < tileComponentHeight; l++)
					{
						for (int num6 = numComps2 - 1; num6 >= 0; num6--)
						{
							array2[num6].ulx = 0;
							array2[num6].uly = l;
							array2[num6].w = tileComponentWidth;
							array2[num6].h = 1;
							blockImageDataSource.getInternCompData(array2[num6], num6);
						}
						int[] array6 = new int[numComps2];
						for (int num7 = numComps2 - 1; num7 >= 0; num7--)
						{
							array6[num7] = array2[num7].offset + tileComponentWidth - 1;
						}
						byte[] array7 = new byte[tileComponentWidth * num];
						for (int num8 = tileComponentWidth - 1; num8 >= 0; num8--)
						{
							int[] array8 = new int[numComps2];
							for (int num9 = numComps2 - 1; num9 >= 0; num9--)
							{
								array8[num9] = (array2[num9].data_array[array6[num9]--] >> array5[num9]) + array3[num9];
								array8[num9] = ((array8[num9] >= 0) ? ((array8[num9] > array4[num9]) ? array4[num9] : array8[num9]) : 0);
								if (blockImageDataSource.getNomRangeBits(num9) != 8)
								{
									array8[num9] = (int)Math.Round((double)array8[num9] / Math.Pow(2.0, blockImageDataSource.getNomRangeBits(num9)) * 255.0);
								}
							}
							int num10 = num8 * num;
							switch (numComps2)
							{
							case 1:
								array7[num10] = (byte)array8[0];
								array7[num10 + 1] = (byte)array8[0];
								array7[num10 + 2] = (byte)array8[0];
								break;
							case 3:
								array7[num10] = (byte)array8[0];
								array7[num10 + 1] = (byte)array8[1];
								array7[num10 + 2] = (byte)array8[2];
								break;
							case 4:
							{
								double num11 = 255.0;
								double num12 = (double)array8[0] / num11;
								double num13 = (double)array8[1] / num11;
								double num14 = (double)array8[2] / num11;
								double num15 = (double)array8[3] / num11;
								double num16 = 0.0;
								double num17 = 0.0;
								double num18 = 0.0;
								double num19 = -1.12;
								double num20 = -1.12;
								double num21 = -1.21;
								if (-1.0 != num12 || num19 != num13 || num20 != num14 || num21 != num15)
								{
									double num22 = num12;
									double num23 = num13;
									double num24 = num14;
									double num25 = num15;
									num16 = 255.0 * (1.0 - num22) * (1.0 - num25);
									num17 = 255.0 * (1.0 - num23) * (1.0 - num25);
									num18 = 255.0 * (1.0 - num24) * (1.0 - num25);
								}
								num16 = ((num16 > 255.0) ? 255.0 : ((num16 < 0.0) ? 0.0 : num16));
								num17 = ((num17 > 255.0) ? 255.0 : ((num17 < 0.0) ? 0.0 : num17));
								num18 = ((num18 > 255.0) ? 255.0 : ((num18 < 0.0) ? 0.0 : num18));
								array7[num10 + 2] = (byte)num16;
								array7[num10 + 1] = (byte)num17;
								array7[num10] = (byte)num18;
								break;
							}
							}
						}
						list.AddRange(array7);
						int num26 = 0;
						for (int m = 0; m < tileComponentWidth; m++)
						{
							sKBitmap.SetPixel(num4 + m, num5 + l, new SKColor(array7[num26], array7[num26 + 1], array7[num26 + 2]));
							num26 += 3;
						}
					}
					num3++;
					num2++;
				}
			}
			return bitmap;
		}
		}
	}

	internal JPXParameters GetParameters(string[][] pinfo)
	{
		JPXParameters jPXParameters = new JPXParameters();
		string[][] parameterInfo = BitstreamReader.ParameterInfo;
		if (parameterInfo != null)
		{
			for (int num = parameterInfo.Length - 1; num >= 0; num--)
			{
				jPXParameters.Add(parameterInfo[num][0], parameterInfo[num][3]);
			}
		}
		parameterInfo = EntropyDecoder.ParameterInfo;
		if (parameterInfo != null)
		{
			for (int num2 = parameterInfo.Length - 1; num2 >= 0; num2--)
			{
				jPXParameters.Add(parameterInfo[num2][0], parameterInfo[num2][3]);
			}
		}
		parameterInfo = DeScalerROI.ParameterInfo;
		if (parameterInfo != null)
		{
			for (int num3 = parameterInfo.Length - 1; num3 >= 0; num3--)
			{
				jPXParameters.Add(parameterInfo[num3][0], parameterInfo[num3][3]);
			}
		}
		parameterInfo = Dequantizer.ParameterInfo;
		if (parameterInfo != null)
		{
			for (int num4 = parameterInfo.Length - 1; num4 >= 0; num4--)
			{
				jPXParameters.Add(parameterInfo[num4][0], parameterInfo[num4][3]);
			}
		}
		parameterInfo = InverseComponetTransformation.ParameterInfo;
		if (parameterInfo != null)
		{
			for (int num5 = parameterInfo.Length - 1; num5 >= 0; num5--)
			{
				jPXParameters.Add(parameterInfo[num5][0], parameterInfo[num5][3]);
			}
		}
		parameterInfo = HeaderDecoder.ParameterInfo;
		if (parameterInfo != null)
		{
			for (int num6 = parameterInfo.Length - 1; num6 >= 0; num6--)
			{
				jPXParameters.Add(parameterInfo[num6][0], parameterInfo[num6][3]);
			}
		}
		parameterInfo = pinfo;
		if (parameterInfo != null)
		{
			for (int num7 = parameterInfo.Length - 1; num7 >= 0; num7--)
			{
				jPXParameters.Add(parameterInfo[num7][0], parameterInfo[num7][3]);
			}
		}
		return jPXParameters;
	}
}
