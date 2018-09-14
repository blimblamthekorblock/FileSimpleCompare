using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CompareFiles
{
    class Program
    {
        public static List<string> CsvQueue1 = new List<string>(); // used to dump logs to a text file
        public static bool debugWriteConsole = true;

        private static void Log(string input)
        {
            if (debugWriteConsole)
                Console.WriteLine(input);

            CsvQueue1.Add(input);
        }

        private static void CsvWrite(List<string> lines, string filename)
        {
            using (var writer = new StreamWriter(filename))
                foreach (var line in lines)
                    writer.WriteLine(line);
        }

        private static void DumpLogs()
        {
            CsvWrite(CsvQueue1, "output.log");
        }

        static void Main(string[] args)
        {
            var filename1 = @"C:\Users\student\Desktop\dan\CompareFiles\raw1.csv";
            var filename2 = @"C:\Users\student\Desktop\dan\CompareFiles\raw2.csv";
            if (args.Length == 2)
            {
                filename1 = args[0];
                filename2 = args[1];
            }

            var lines1 = ReadCsv(filename1);
            var lines2 = ReadCsv(filename2);

            Compare(lines1, lines2);

            Console.WriteLine("Done.");
            Console.ReadKey();
        }

        private static List<string> ReadCsv(string filename)
        {
            var output = new List<string>();

            using (var reader = new StreamReader(filename))
            {
                var line = "";
                while (line != null)
                {
                    line = reader.ReadLine();
                    output.Add(line);
                }

                if (output.Last() == null)
                    output.RemoveAt(output.Count - 1);
            }

            return output;
        }

        private static void Compare(List<string> lines1, List<string> lines2)
        {
            var filename1 = MemberInfoGetting.GetMemberName(() => lines1);
            var filename2 = MemberInfoGetting.GetMemberName(() => lines2);

            if (lines1.Count != lines2.Count)
                Log($"WARNING: files don't have the same length: {filename1}: {lines1.Count}. {filename2}: {lines2.Count}");

            for (int i = 0; i < lines1.Count; i++)
            {
                if (i > lines2.Count -1)
                    return;

                var line1 = ParseLine(lines1[i]);
                var line2 = ParseLine(lines2[i]);
                var template = $"{lines1[i]},{lines2[i]},{(i * 4):X8}";
                var template2 = "";

                // Check if both values aren't equal to 0
                if (lines1[i] == lines2[i] && lines2[i] == "00000000")
                {
                    template2 = "unknown";
                    goto end;
                }

                if (lines1[i] == lines2[i])
                {
                    template2 = "bytes";
                    goto end;
                }

                if (line1[0] == line2[1] &&
                    line1[1] == line2[0] &&
                    line1[2] == line2[3] &&
                    line1[3] == line2[2])
                {
                    template2 = "short";
                    goto end;
                }

                if (line1[0] == line2[3] &&
                    line1[1] == line2[2] &&
                    line1[2] == line2[1] &&
                    line1[3] == line2[0])
                {
                    template2 = "int";
                    goto end;
                }

                if (lines1[i] != lines2[i])
                {
                    template2 = "different";
                    goto end;
                }

            end:
                Log($"{template},{template2}");

            }
        }

        private static byte[] ParseLine(string input)
        {
            uint output = 0xCDCDCDCD;
            uint.TryParse(input, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out output);
            if (output == 0xCDCDCDCD)
                throw new Exception($"Cannot parse as a hex number: {input}");

            var bytes = BitConverter.GetBytes(output);
            return bytes;
        }

        public static class MemberInfoGetting // from stackoverflow.com/questions/9801624/get-name-of-a-variable-or-parameter
        {
            public static string GetMemberName<T>(Expression<Func<T>> memberExpression)
            {
                var expressionBody = (MemberExpression)memberExpression.Body;
                return expressionBody.Member.Name;
            }
        }
    }
}
