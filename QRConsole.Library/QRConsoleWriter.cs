using System;
using System.IO;
using ZXing.Common;
using ZXing.QrCode;

namespace QRConsole.Library
{
    public static class QRConsoleWriter
    {
        public static readonly char FullBlack = ' ';
        public static readonly char BottomBlack = '\u2580';
        public static readonly char TopBlack = '\u2584';
        public static readonly char FullWhite = '\u2588';

        public static void Write(string content, TextWriter output)
        {
            var barCode = new QRCodeWriter();

            BitMatrix bits = barCode.encode(content, ZXing.BarcodeFormat.QR_CODE, 0, 0);

            int offsetX = 2;
            int offsetY = 2;
            int width = bits.Width - 2;
            int height = bits.Height - 2;

            for (int y = offsetX; y < height; y += 2)
            {
                for (int x = offsetY; x < width; x += 1)
                {
                    bool top = bits[x, y + 0];
                    bool bottom = true;
                    if (y < height - 1)
                        bottom = bits[x, y + 1];

                    if (top && bottom)
                        output.Write(FullBlack);
                    else if (top)
                        output.Write(TopBlack);
                    else if (bottom)
                        output.Write(BottomBlack);
                    else
                        output.Write(FullWhite);
                }
                output.WriteLine();
            }
        }
    }
}
