using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Extensions.CommandLineUtils;
using ZXing.Common;
using ZXing.QrCode;

namespace QRConsole
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var cmd = new CommandLineApplication();

            cmd.Name = "QRConsole";
            cmd.FullName = "QRConsole";
            cmd.Description = "Tool that encodes input value into a QR code and prints it in the console";

            cmd.HelpOption("-h|--help|--herp");

            CommandOption contentTextOption = cmd.Option("-t|--text", "Text to encode into a QR code", CommandOptionType.SingleValue);
            CommandOption contentFileOption = cmd.Option("-f|--file", "File containing text to encode into a QR code", CommandOptionType.SingleValue);
            CommandOption clearOption = cmd.Option("-c|--clear", "Clears the console before printing the QR code", CommandOptionType.NoValue);
            CommandOption noWaitOption = cmd.Option("-w|--no-wait", "Does not pause before printing the QR code", CommandOptionType.NoValue);
            CommandOption base64Option = cmd.Option("-b|--base64", "Encodes the file content to base 64 before encoding it to QR code (useful for binary files)", CommandOptionType.NoValue);
            CommandOption encodingOption = cmd.Option("-e|--encoding", "Encoding of the file content", CommandOptionType.SingleValue);

            if (args.Length == 0)
            {
                cmd.ShowHelp();
                return;
            }

            try
            {
                cmd.Execute(args);
            }
            catch (CommandParsingException ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }

            if (contentTextOption.HasValue() == false && contentFileOption.HasValue() == false)
            {
                cmd.ShowHelp();
                return;
            }
            else if (contentTextOption.HasValue() && contentFileOption.HasValue())
            {
                Console.WriteLine("Text and file are mutually exclusive");
                return;
            }

            string content = null;

            if (contentTextOption.HasValue())
                content = contentTextOption.Value();
            else if (contentFileOption.HasValue())
            {
                string filename = Path.GetFullPath(contentFileOption.Value());

                if (File.Exists(filename) == false)
                {
                    Console.WriteLine($"Impossible to find file '{filename}'");
                    return;
                }

                Encoding encoding = Encoding.UTF8;

                if (encodingOption.HasValue())
                    encoding = RetrieveEncoding(encodingOption.Value());

                if (base64Option.HasValue())
                    content = Convert.ToBase64String(File.ReadAllBytes(filename));
                else
                    content = File.ReadAllText(filename, encoding);
            }

            new Program().Run(content, noWaitOption.HasValue(), clearOption.HasValue());
        }

        private static Encoding RetrieveEncoding(string value)
        {
            int codePage;
            if (int.TryParse(value, out codePage))
            {
                try
                {
                    return Encoding.GetEncoding(codePage);
                }
                catch (Exception)
                {
                    Console.WriteLine($"Invalid encoding '{codePage}', fallback to 'UTF-8'");
                    return Encoding.UTF8;
                }
            }

            try
            {
                return Encoding.GetEncoding(value);
            }
            catch (Exception)
            {
                Console.WriteLine($"Invalid encoding '{value}', fallback to 'UTF-8'");
                return Encoding.UTF8;
            }
        }

        private void Run(string content, bool noWait, bool clear)
        {
            var barCode = new QRCodeWriter();

            BitMatrix bits;

            try
            {
                bits = barCode.encode(content, ZXing.BarcodeFormat.QR_CODE, 0, 0);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }

            string fullBlack = " ";
            string bottomBlack = "\u2580";
            string topBlack = "\u2584";
            string fullWhite = "\u2588";

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                Console.OutputEncoding = Encoding.Unicode;
            else
                Console.OutputEncoding = Encoding.UTF8;

            int offsetX = 2;
            int offsetY = 2;
            int width = bits.Width - 2;
            int height = bits.Height - 2;

            if (noWait == false)
            {
                Console.WriteLine("Press any key to print the QR code");
                Console.ReadKey(true);
            }

            if (clear)
                Console.Clear();
            else
                Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.White;

            try
            {
                for (int y = offsetX; y < height; y += 2)
                {
                    for (int x = offsetY; x < width; x += 1)
                    {
                        bool top = bits[x, y + 0];
                        bool bottom = true;
                        if (y < height - 1)
                            bottom = bits[x, y + 1];

                        if (top && bottom)
                            Console.Write(fullBlack);
                        else if (top)
                            Console.Write(topBlack);
                        else if (bottom)
                            Console.Write(bottomBlack);
                        else
                            Console.Write(fullWhite);
                    }
                    Console.WriteLine();
                }
            }
            finally
            {
                Console.ResetColor();
            }
         }
    }
}
