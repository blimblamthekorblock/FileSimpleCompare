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
            var lines1 = ReadCsv(@"C:\Users\student\Desktop\dan\raw1.csv");
            var lines2 = ReadCsv(@"C:\Users\student\Desktop\dan\raw2.csv");

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
                if (i > lines2.Count)
                    return;

                var line1 = ParseLine(lines1[i]);
                var line2 = ParseLine(lines2[i]);

                if (line1[0] == line2[0] &&
                    line1[1] == line2[1] &&
                    line1[2] == line2[2] &&
                    line1[3] == line2[3] &&
                    line1[0] == 0x0 &&
                    line1[1] == 0x0 &&
                    line1[2] == 0x0 &&
                    line1[3] == 0x0
                    )
                {
                    Log($"{lines1[i]},{lines2[i]},unknown");
                    continue;
                }

                if (line1[0] == line2[0] &&
                    line1[1] == line2[1] &&
                    line1[2] == line2[2] &&
                    line1[3] == line2[3])
                {
                    Log($"{lines1[i]},{lines2[i]},bytes");
                    continue;
                }

                if (line1[0] == line2[1] &&
                    line1[1] == line2[0] &&
                    line1[2] == line2[3] &&
                    line1[3] == line2[2])
                {
                    Log($"{lines1[i]},{lines2[i]},short");
                    continue;
                }

                if (line1[0] == line2[3] &&
                            line1[1] == line2[2] &&
                            line1[2] == line2[1] &&
                            line1[3] == line2[0])
                {
                    Log($"{lines1[i]},{lines2[i]},int");
                    continue;
                }

                if (line1[0] != line2[0] &&
                        line1[1] != line2[1] &&
                        line1[2] != line2[2] &&
                        line1[3] != line2[3])
                {
                    Log($"{lines1[i]},{lines2[i]},different");
                    continue;
                }

            }
        }

        private static byte[] ParseLine(string input)
        {
            uint output = 0;
            uint.TryParse(input, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out output);
            var bytes = BitConverter.GetBytes(output);
            return bytes;
        }

        public static class MemberInfoGetting
        {
            public static string GetMemberName<T>(Expression<Func<T>> memberExpression)
            {
                MemberExpression expressionBody = (MemberExpression)memberExpression.Body;
                return expressionBody.Member.Name;
            }
        }

    }
}
