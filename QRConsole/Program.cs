using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Extensions.CommandLineUtils;
using QRConsole.Library;

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
            if (noWait == false)
            {
                Console.WriteLine("Press any key to print the QR code");
                Console.ReadKey(true);
            }

            if (clear)
                Console.Clear();
            else
                Console.WriteLine();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                Console.OutputEncoding = Encoding.Unicode;
            else
                Console.OutputEncoding = Encoding.UTF8;

            Console.ForegroundColor = ConsoleColor.White;

            try
            {
                QRConsoleWriter.Write(content, Console.Out);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }
            finally
            {
                Console.ResetColor();
            }
         }
    }
}
