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
        // URLs of the hosted files
        public static string xmlURL = "https://raw.githubusercontent.com/uggmonster/cse445_a4/main/NationalParks.xml";
        public static string xmlErrorURL = "https://raw.githubusercontent.com/uggmonster/cse445_a4/main/NationalParksErrors.xml";
        public static string xsdURL = "https://raw.githubusercontent.com/uggmonster/cse445_a4/main/NationalParks.xsd";

        public static void Main(string[] args)
        {
            // 1) Valid XML against the XSD: expect "No errors are found"
            string result = Verification(xmlURL, xsdURL);
            Console.WriteLine(result);

            // 2) XML with injected errors against the XSD: expect the list of errors
            result = Verification(xmlErrorURL, xsdURL);
            Console.WriteLine(result);

            // 3) Convert the valid XML to JSON
            result = Xml2Json(xmlURL);
            Console.WriteLine(result);
        }

        // Q2.1
        // Validates the XML at xmlUrl against the XSD at xsdUrl.
        // Returns "No errors are found" if valid, otherwise all error messages found.
        public static string Verification(string xmlUrl, string xsdUrl)
        {
            StringBuilder errors = new StringBuilder();
            try
            {
                string xsdText = DownloadContent(xsdUrl);
                string xmlText = DownloadContent(xmlUrl);

                // Configure the reader to validate against the schema
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.ValidationType = ValidationType.Schema;
                using (XmlReader xsdReader = XmlReader.Create(new StringReader(xsdText)))
                {
                    settings.Schemas.Add(null, xsdReader);
                }

                // Collect every schema validation error (does not stop at the first one)
                settings.ValidationEventHandler += (sender, e) =>
                {
                    errors.AppendLine(e.Severity + ": " + e.Message
                        + " (Line " + e.Exception.LineNumber + ", Position " + e.Exception.LinePosition + ")");
                };

                // Reading the whole document triggers validation
                using (XmlReader reader = XmlReader.Create(new StringReader(xmlText), settings))
                {
                    while (reader.Read()) { }
                }
            }
            catch (XmlException ex)
            {
                // Well-formedness error (e.g. missing closing tag)
                errors.AppendLine("XML Error: " + ex.Message);
            }
            catch (Exception ex)
            {
                // Download failure or invalid XSD
                errors.AppendLine("Error: " + ex.Message);
            }

            return errors.Length == 0 ? "No errors are found" : errors.ToString().Trim();
        }

        // Q2.2
        // Converts the (valid) XML at xmlUrl into JSON text that Newtonsoft can convert back to XML.
        public static string Xml2Json(string xmlUrl)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(DownloadContent(xmlUrl));

            // Serialize from the root element (skips the XML declaration); attributes become "@Name"
            string json = JsonConvert.SerializeXmlNode(doc.DocumentElement, Newtonsoft.Json.Formatting.Indented);

            // Newtonsoft only creates arrays for repeated elements, so force
            // NationalPark and Phone to always be arrays as required.
            JObject root = JObject.Parse(json);
            JObject parks = (JObject)root["NationalParks"];
            parks["NationalPark"] = ToArray(parks["NationalPark"]);
            foreach (JObject park in parks["NationalPark"].Children<JObject>())
            {
                if (park["Phone"] != null)
                {
                    park["Phone"] = ToArray(park["Phone"]);
                }
            }

            // Optional Rating is only present in the JSON when it exists in the XML
            return root.ToString(Newtonsoft.Json.Formatting.Indented);
        }

        // Wraps a single token in a JSON array (returns it unchanged if already an array)
        private static JArray ToArray(JToken token)
        {
            return token is JArray ? (JArray)token : new JArray(token);
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
