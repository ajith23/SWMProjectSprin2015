using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PosTagger
{
    class Program
    {
        static void Main(string[] args)
        {
            start:
            Console.WriteLine("Retrieving rows...");
            var query = @"SELECT id, review_title, review_text
                            FROM  `reviewdata` 
                            WHERE `pos_tagged_review` is null
                            LIMIT 100 ";

            var dataSet = Utility.MySqlAccess.ExecuteQuery(query);

            if (dataSet.Tables[0].Rows.Count > 0)
            {
                Console.WriteLine("Retrieved " + dataSet.Tables[0].Rows.Count + " rows.");
                Console.WriteLine("Processing rows...");
                var index = 1;

                foreach (DataRow row in dataSet.Tables[0].Rows)
                {
                    try
                    {
                        var taggedData = Utility.PartOfSpeech.GetTaggedSentenceWithPartOfSpeech((row[1].ToString() + " . " + row[2].ToString()).Replace("'", " "));
                        var nouns = Utility.PartOfSpeech.GetNounsForSentence(taggedData);
                        var insertQuery = string.Format(@"UPDATE reviewdata SET  `pos_tagged_review` =  '{0}', `nouns_in_review` =  '{1}' WHERE id = {2}", taggedData, nouns, row[0].ToString());
                        Utility.MySqlAccess.ExecuteNonQuery(insertQuery);
                        Console.WriteLine("Inserted row " + (index++));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
                goto start;
            }
            else
            {
                Console.WriteLine("All reviews are tagged !");
            }

            Console.ReadLine();
        }
    }
}
