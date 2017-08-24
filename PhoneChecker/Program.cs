using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PhoneChecker
{
    public class Program
    {
        private const int RequiredParameterCount = 1;
        private const string UsageString = "PhoneChecked <path> [startDate]";

        static void Main(string[] args)
        {
            if (args.Length < RequiredParameterCount)
            {
                Console.WriteLine("At least 1 parameter is required.");
                Console.WriteLine(UsageString);
                return;
            }

            var fileFolder = args[0];
            DateTime? startDate = null;

            if (args.Length == 2)
            {
                startDate = DateTime.Parse(args[1]);
            }

            var files = Directory.GetFiles(fileFolder);

            var numbers = ReadNumbers();

            foreach (var filename in files)
            {
                using (var inputFile = new StreamReader(Path.Combine(fileFolder, filename)))
                {
                    Console.WriteLine("Checking Numbers in " + filename);

                    var headerRow = new List<string>();
                    var dataRow = new List<List<string>>();
                    var method = filename.Contains("voice") ? "called" : "texted";

                    while (!inputFile.EndOfStream)
                    {
                        var buffer = inputFile.ReadLine();
                        if (buffer != null && (buffer.StartsWith("Usage") || buffer.StartsWith("\"") ||
                                               buffer.StartsWith("Total") ||
                                               string.IsNullOrWhiteSpace(buffer))) continue;

                        if (buffer != null && buffer.StartsWith("Date"))
                        {
                            headerRow = buffer.Split(',').ToList();
                            continue;
                        }

                        if (buffer == null) continue;

                        var line = buffer.Split(',').ToList();
                        if (line.Count > 7)
                        {
                            // Combine City & State 
                            line[2] = line[2].TrimStart('"') + ", " + line[3].TrimEnd('"');
                            line.RemoveAt(3);
                        }
                        dataRow.Add(line);
                    }

                    foreach (var row in dataRow)
                    {
                        var dateColumn = headerRow.FindIndex(r => r.Contains("Date"));
                        var numberColumn = headerRow.FindIndex(r => r.Contains("Number"));

                        foreach (var number in numbers)
                        {
                            if (row[numberColumn].Contains(number.Value) &&
                                (startDate == null || DateTime.Parse(row[dateColumn]) >= startDate))
                            {
                                Console.WriteLine("On " + row[dateColumn] + " " + row[numberColumn] + " (" +
                                                  number.Key + ") was " + method);
                            }
                        }
                    }
                }
            }
            Console.WriteLine("Done checking");
            Console.WriteLine("Press <enter> to continue");
            Console.ReadLine();
        }

        private static Dictionary<string, string> ReadNumbers()
        {
            Dictionary<string, string> numbers;

            using (var dataFile = new StreamReader("DataFiles/Numberlist.json"))
            {
                using (var reader = new JsonTextReader(dataFile))
                {
                    numbers = JToken.ReadFrom(reader).ToObject<Dictionary<string, string>>();
                }
            }
            return numbers;
        }
    }
}
