using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PhoneChecker
{
    public class Program
    {
        static void Main()
        {
            var files = Directory.GetFiles(@"C:\Files\Mya");
            var numbers = new Dictionary<string, string>
            {
                {"Amanda", "985-5660"},
                {"Anella", "919-1030"},
                {"Keara", "860-5948"},
                {"Jazzy", "688-2884"}
            };
            
            foreach (var filename in files)
            {
                var inputFile = new StreamReader(Path.Combine(@"C:\Files\Mya", filename));
                var headerRow = new List<string>();
                var dataRow = new List<List<string>>();
                var method = filename.Contains("voice") ? "called" : "texted";

                while (!inputFile.EndOfStream)
                {
                    var buffer = inputFile.ReadLine();
                    if (buffer != null && (buffer.StartsWith("Usage") || buffer.StartsWith("\"") ||
                                           buffer.StartsWith("Total") || string.IsNullOrWhiteSpace(buffer))) continue;

                    if (buffer != null && buffer.StartsWith("Date"))
                    {
                        headerRow = buffer.Split(',').ToList();
                    }

                    if (buffer != null)
                    {
                        dataRow.Add(buffer.Split(',').ToList());
                    }
                }
                Console.WriteLine("Checking Numbers in " + filename);

                foreach (var row in dataRow)
                {
                    var dateColumn = headerRow.FindIndex(r => r.Contains("Date"));
                    var numberColumn = headerRow.FindIndex(r => r.Contains("Number"));

                    foreach (var number in numbers)
                    {
                        if (row[numberColumn].Contains(number.Value) && DateTime.Parse(row[dateColumn]) >= new DateTime(2017, 08, 01))
                        {
                            Console.WriteLine("On " + row[dateColumn] + " " + row[numberColumn] + " (" + number.Key + ") was " + method);
                        }
                    }
                }
            }
            Console.WriteLine("Done checking");
            Console.WriteLine("Press <enter> to continue");
            Console.ReadLine();
        }
    }
}
