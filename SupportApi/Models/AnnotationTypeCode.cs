﻿using System;
using System.Collections.Generic;
using System.Text;

namespace SupportApi.Models
{
    public enum AnnotationTypeCode
    {
        UNKNOWN = -1,
        TEXT = 1,
        LINK = 2,
        FREETEXT =3,
        LINE =4,
        SQUARE = 5,
        CIRCLE = 6,
        POLYGON = 7,
        POLYLINE = 8,
        HIGHLIGHT = 9,
        UNDERLINE = 10,
        SQUIGGLY = 11,
        STRIKEOUT = 12,
        STAMP = 13,
        CARET = 14,
        INK = 15,
        POPUP = 16,
        FILEATTACHMENT = 17,
        SOUND = 18,
        MOVIE = 19,
        WIDGET = 20,
        SCREEN = 21,
        PRINTERMARK = 22,
        TRAPNET = 23,
        WATERMARK = 24,
        THREED = 25,
        REDACT = 26,
        SIGNATURE = 27,
        THREADBEAD = 150,
    }
}
