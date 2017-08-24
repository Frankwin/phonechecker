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

        private static void Main(string[] args)
        {
            if (ReadParameters(args, out string fileFolder, out DateTime? startDate)) return;

            var files = Directory.GetFiles(fileFolder);

            var numbers = ReadPhoneNumberDictionary();

            foreach (var filename in files)
            {
                using (var inputFile = new StreamReader(Path.Combine(fileFolder, filename)))
                {
                    Console.WriteLine("Checking Numbers in " + filename);

                    var method = filename.Contains("voice") ? "called" : "texted";

                    ReadFile(inputFile, out List<string> headerRow, out List<List<string>> dataRow);

                    AnalyzeData(dataRow, headerRow, numbers, startDate, method);
                }
            }
            Console.WriteLine("Done checking");
            Console.WriteLine("Press <enter> to continue");
            Console.ReadLine();
        }

        /// <summary>
        /// Analyses the data in all the dataRows and writes a message if a phone number from the dictionary of phoneNumberDictionary
        /// is found after the start date
        /// </summary>
        /// <param name="dataRows">DataRows to analyze</param>
        /// <param name="headerRow">HeaderRow with column names</param>
        /// <param name="phoneNumberDictionary">Dictionary of phonenumbers to search for</param>
        /// <param name="startDate">StartDate to look for</param>
        /// <param name="method">Method of communication (called/texted)</param>
        private static void AnalyzeData(IEnumerable<List<string>> dataRows, List<string> headerRow, Dictionary<string, string> phoneNumberDictionary, DateTime? startDate, string method)
        {
            foreach (var row in dataRows)
            {
                var dateColumn = headerRow.FindIndex(r => r.Contains("Date"));
                var numberColumn = headerRow.FindIndex(r => r.Contains("Number"));

                foreach (var number in phoneNumberDictionary)
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

        /// <summary>
        /// Read the contents of a file into a header row and a list of datarows
        /// </summary>
        /// <param name="inputFile">Input file to read</param>
        /// <param name="headerRow">List of headers</param>
        /// <param name="dataRow">List of datarows, each consisting of a List of strings</param>
        private static void ReadFile(StreamReader inputFile, out List<string> headerRow, out List<List<string>> dataRow)
        {
            headerRow = new List<string>();
            dataRow = new List<List<string>>();

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
        }

        /// <summary>
        /// Read the parameters passed into the program
        /// </summary>
        /// <param name="args">Parameters to deal with</param>
        /// <param name="fileFolder">File folder location</param>
        /// <param name="startDate">Start Date</param>
        /// <returns></returns>
        private static bool ReadParameters(string[] args, out string fileFolder, out DateTime? startDate)
        {
            if (args.Length < RequiredParameterCount)
            {
                Console.WriteLine("At least 1 parameter is required.");
                Console.WriteLine(UsageString);
                fileFolder = "";
                startDate = null;
                return true;
            }

            fileFolder = args[0];
            startDate = null;

            if (args.Length == 2)
            {
                startDate = DateTime.Parse(args[1]);
            }
            return false;
        }

        /// <summary>
        /// Read in the Dictionary of phone numbers from the configuration file
        /// </summary>
        /// <returns>Dictionary of names and phonenumbers from the configuration file.</returns>
        private static Dictionary<string, string> ReadPhoneNumberDictionary()
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
