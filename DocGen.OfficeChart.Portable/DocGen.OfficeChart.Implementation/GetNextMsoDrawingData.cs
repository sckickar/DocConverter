using System;
using DocGen.OfficeChart.Parser.Biff_Records;

namespace DocGen.OfficeChart.Implementation;

[CLSCompliant(false)]
internal delegate BiffRecordRaw[] GetNextMsoDrawingData();
