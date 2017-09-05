using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using CheckBin.Models;
using Newtonsoft.Json;

namespace CheckBin
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                if (args.Length != 1)
                {
                    Console.WriteLine("Usage:");
                    Console.WriteLine("CheckBin.exe [Path to CSV With Bins]");
                    Console.WriteLine("Example:");
                    Console.WriteLine(@"CheckBin.exe C:\CretitCard_BINS.csv");
                    return;
                }


                if (args[0] != String.Empty)
                {
                    var reader = new StreamReader(File.OpenRead(args[0]));
                    List<string> bins = new List<string>();
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        var values = line.Split(';');

                        bins.Add(values[0]);
                    }

                    var cards = bins.Select(b => CheckCreditCard(b)).ToList();
                    var export = new CsvExport<CreditCard>(cards);
                    export.ExportToFile(args[0] + "_output.csv");
                }

                else
                {
                    Console.WriteLine("Invalid Option. Try Again");
                }
            }

            catch (Exception ex)
            {

                Console.WriteLine("ERROR {0}", ex);
                Console.WriteLine("You might have the CSV open. Close it and try again");
                Console.ReadLine();
            }

        }

        public static CreditCard CheckCreditCard(string bin)
        {
            using (var client = new WebClient())
            {
                var json = client.DownloadString("http://www.binlist.net/json/" + bin);
                var response = JsonConvert.DeserializeObject<CreditCard>(json);
                Console.WriteLine("{0} Processing BIN: {1} Type:{2} Bank: {3}", DateTime.Now, response.bin, response.brand, response.bank);
                return response;
            }
        }

        public class CsvExport<T> where T : class
        {
            public List<T> Objects;

            public CsvExport(List<T> objects)
            {
                Objects = objects;
            }

            public string Export()
            {
                return Export(true);
            }

            public string Export(bool includeHeaderLine)
            {

                StringBuilder sb = new StringBuilder();
                //Get properties using reflection.
                IList<PropertyInfo> propertyInfos = typeof(T).GetProperties();

                if (includeHeaderLine)
                {
                    //add header line.
                    foreach (PropertyInfo propertyInfo in propertyInfos)
                    {
                        sb.Append(propertyInfo.Name).Append(",");
                    }
                    sb.Remove(sb.Length - 1, 1).AppendLine();
                }

                //add value for each property.
                foreach (T obj in Objects)
                {
                    foreach (PropertyInfo propertyInfo in propertyInfos)
                    {
                        sb.Append(MakeValueCsvFriendly(propertyInfo.GetValue(obj, null))).Append(",");
                    }
                    sb.Remove(sb.Length - 1, 1).AppendLine();
                }

                return sb.ToString();
            }

            //export to a file.
            public void ExportToFile(string path)
            {
                File.WriteAllText(path, Export());
            }

            //export as binary data.
            public byte[] ExportToBytes()
            {
                return Encoding.UTF8.GetBytes(Export());
            }

            //get the csv value for field.
            private string MakeValueCsvFriendly(object value)
            {
                if (value == null) return "";
                if (value is Nullable && ((INullable)value).IsNull) return "";

                if (value is DateTime)
                {
                    if (((DateTime)value).TimeOfDay.TotalSeconds == 0)
                        return ((DateTime)value).ToString("yyyy-MM-dd");
                    return ((DateTime)value).ToString("yyyy-MM-dd HH:mm:ss");
                }
                string output = value.ToString();

                if (output.Contains(",") || output.Contains("\""))
                    output = '"' + output.Replace("\"", "\"\"") + '"';

                return output;

            }
        }
    }
}
