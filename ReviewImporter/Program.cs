using AprioriAlgorithm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace ReviewImporter
{
    class Program
    {
        //static double minSupport = 50.0;
        //static double minConfidence = 50.0;
        static void Main(string[] args)
        {
            Console.WriteLine("Enter complete file path: ");
            var filePath = Console.ReadLine();
            if (File.Exists(filePath))
            {
                var xml = "<reviews>" + Utility.FileManager.ReadFile(filePath) + "</reviews>";

                var doc = XDocument.Parse(RemoveInvalidXmlChars(xml));
                var reviewCount = doc.Root.Descendants("review").Count();
                var processed = 0;
                var errorList = new List<XElement>();
                var exceptionList = new List<string>();
                foreach (var element in doc.Root.Descendants("review"))
                {
                    try
                    {
                        var reviewProcessingStart = DateTime.Now;

                        var product = element.Descendants("product_name").FirstOrDefault().Value.Trim();
                        var category = element.Descendants("product_type").FirstOrDefault().Value.Trim();

                        var helpful = element.Descendants("helpful").FirstOrDefault().Value.Trim();
                        var rating = Convert.ToDouble(element.Descendants("rating").FirstOrDefault().Value.Trim());
                        var title = element.Descendants("title").FirstOrDefault().Value.Trim().Replace("'", "").Replace('"', ' ');
                        var location = element.Descendants("reviewer_location").FirstOrDefault().Value.Trim();
                        var reviewText = element.Descendants("review_text").FirstOrDefault().Value.Trim().Replace("'", "").Replace('"', ' ');

                        var sentiment = 0;
                        if (rating < 3) sentiment = -1;
                        if (rating > 3) sentiment = 1;

                        var query = string.Format(@"insert into reviewdata (review_title, rating, helpful, location, review_text, product_ref_id, sentiment, manually_labelled)
                            values ('{0}', {1}, '{2}', '{3}', '{4}', {5}, {6}, {7})",
                                                                    title, rating, helpful, location, reviewText,
                                                                    Utility.MySqlAccess.GetProductId(product, Utility.MySqlAccess.GetCategoryId(category), 0),
                                                                    sentiment, 1);

                        Utility.MySqlAccess.ExecuteNonQuery(query);
                        processed++;

                        var reviewProcessingEnd = DateTime.Now;

                        Console.WriteLine("Processed " + processed + " / " + reviewCount + "  | time taken: " + (reviewProcessingEnd - reviewProcessingStart));
                        //Console.WriteLine(category.Trim() + " \t" + product.Trim());
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Failed " + processed + " / " + reviewCount);
                        processed++;
                        errorList.Add(element);
                        exceptionList.Add(ex.Message);
                    }
                }

                using (StreamWriter writer = new StreamWriter(filePath.TrimEnd('\\') + @" \error.txt"))
                {
                    for (var i = 0; i < errorList.Count; i++)
                    {
                        writer.WriteLine(errorList[i].ToString());
                        writer.WriteLine(exceptionList[i].ToString());
                        writer.WriteLine("--------------------------");
                    }
                }
                Console.WriteLine("Completed Import. Error file written in path - " + filePath + @"\error.txt");
                Console.ReadLine();
            }
        }

        static string RemoveInvalidXmlChars(string text)
        {
            var validXmlChars = text.Where(ch => XmlConvert.IsXmlChar(ch)).ToArray();
            var xmlString = new string(validXmlChars);
            xmlString = xmlString.Replace(" & ", " and ");
            xmlString = xmlString.Replace("& ", " and ");
            xmlString = xmlString.Replace(" &", " and ");
            xmlString = xmlString.Replace("&", " and ");
            return xmlString;
        }
    }
}
