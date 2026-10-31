using System;
using System.Xml.Schema;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Text;
/**
 * This template file is created for ASU CSE445 Distributed SW Dev Assignment 4.
 * Please do not modify or delete any existing class/variable/method names. However, you can add more variables and functions.
 * Uploading this file directly will not pass the autograder's compilation check, resulting in a grade of 0.
 * **/
namespace ConsoleApp1
{
    public class Submission
    {
        // Links to the files hosted on GitHub
        public static string xmlURL = "https://raw.githubusercontent.com/YOUR_USERNAME/YOUR_REPO/main/NationalParks.xml";
        public static string xmlErrorURL = "https://raw.githubusercontent.com/YOUR_USERNAME/YOUR_REPO/main/NationalParksErrors.xml";
        public static string xsdURL = "https://raw.githubusercontent.com/YOUR_USERNAME/YOUR_REPO/main/NationalParks.xsd";
        public static void Main(string[] args)
        {
            // Check the good XML file (should print "No errors are found")
            string result = Verification(xmlURL, xsdURL);
            Console.WriteLine(result);
            // Check the XML file with errors (should print the errors)
            result = Verification(xmlErrorURL, xsdURL);
            Console.WriteLine(result);
            // Convert the good XML file to JSON
            result = Xml2Json(xmlURL);
            Console.WriteLine(result);
        }
        // Q2.1
        // Checks the XML file against the XSD file and returns the errors found
        public static string Verification(string xmlUrl, string xsdUrl)
        {
            StringBuilder errors = new StringBuilder();
            try
            {
                string xsdText = DownloadContent(xsdUrl);
                string xmlText = DownloadContent(xmlUrl);
                // Set up the reader to check the XML against the schema
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.ValidationType = ValidationType.Schema;
                settings.Schemas.Add(null, XmlReader.Create(new StringReader(xsdText)));
                // Save every schema error in the list
                settings.ValidationEventHandler += (sender, e) =>
                {
                    errors.AppendLine("Error: " + e.Message + " (Line " + e.Exception.LineNumber + ", Position " + e.Exception.LinePosition + ")");
                };
                // Reading the whole file runs the check
                XmlReader reader = XmlReader.Create(new StringReader(xmlText), settings);
                while (reader.Read()) { }
            }
            catch (XmlException ex)
            {
                // The XML itself is broken (for example a missing closing tag)
                errors.AppendLine("XML Error: " + ex.Message);
            }
            catch (Exception ex)
            {
                // Something else went wrong (for example the download failed)
                errors.AppendLine("Error: " + ex.Message);
            }
            if (errors.Length == 0)
            {
                return "No errors are found";
            }
            return errors.ToString().Trim();
        }
        // Q2.2
        // Converts the XML file into JSON text
        public static string Xml2Json(string xmlUrl)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(DownloadContent(xmlUrl));
            // Newtonsoft does the XML to JSON conversion
            string json = JsonConvert.SerializeXmlNode(doc.DocumentElement, Newtonsoft.Json.Formatting.Indented);
            // Newtonsoft only makes an array when an element repeats,
            // so make sure NationalPark and Phone are always arrays
            JObject root = JObject.Parse(json);
            JObject parks = (JObject)root["NationalParks"];
            if (!(parks["NationalPark"] is JArray))
            {
                parks["NationalPark"] = new JArray(parks["NationalPark"]);
            }
            foreach (JObject park in parks["NationalPark"])
            {
                if (!(park["Phone"] is JArray))
                {
                    park["Phone"] = new JArray(park["Phone"]);
                }
            }
            return root.ToString(Newtonsoft.Json.Formatting.Indented);
        }
        // Helper method to download content from URL
        private static string DownloadContent(string url)
        {
            using (System.Net.WebClient client = new System.Net.WebClient())
            {
                return client.DownloadString(url);
            }
        }
    }
}
