using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComponentExtractor
{
    class Program
    {
        static void Main(string[] args)
        {
            //ConsoleTesting();
            var graphPreparationData = new List<List<ComponentEvaluationMetric>>();
            //var minimumSupport = 0.5;
            for (var minimumSupport = 0.0; minimumSupport <= 1.05; minimumSupport += 0.05)
            {
                graphPreparationData.Add(GenerateAccuracyMetrics(minimumSupport));
            }

            var list = GenerateGraphData(graphPreparationData);
            var plotString = "";

            using (StreamWriter writer = new StreamWriter(@"C:\Users\Ajith\Desktop\support-vs-f1score.txt"))
            {
                plotString += "[ ";
                for (var i = 0; i < list.Count; i++)
                {
                    plotString += list[i].X *100 + " " + list[i].Y + " ; ";
                }
                plotString += " ]";
                writer.WriteLine(plotString);
            }

            Console.WriteLine("Completed !");
            Console.ReadLine();
        }

        private static List<ComponentEvaluationMetric> GenerateAccuracyMetrics(double minimumSupport)
        {
            var idList = new int[] { 434, 482, 483, 502, 505 };
            var componentList = new List<ComponentEvaluationMetric>();

            foreach (var productId in idList)
            {
                var query = string.Format(@"select nouns_in_review from reviewdata where product_ref_id = {0}", productId);
                var dataSet = Utility.MySqlAccess.ExecuteQuery(query);

                var transactions = new List<string>();
                var distinctNounList = new List<string>();
                foreach (DataRow row in dataSet.Tables[0].Rows)
                {
                    var nounArray = row[0].ToString().Split(',');
                    foreach (var noun in nounArray)
                        distinctNounList.Add(noun);
                    transactions.Add(row[0].ToString().Replace(",", "|"));
                }

                AprioriAlgorithm.IApriori ap = new AprioriAlgorithm.Apriori();
                var output = ap.ProcessTransaction(minimumSupport, minimumSupport, distinctNounList.Distinct(), transactions.ToArray());
                var actualComponentSet = Utility.MySqlAccess.ExecuteQuery(string.Format("select component_list from manually_labelled_components where product_ref_id ={0}", productId));

                if (actualComponentSet.Tables[0].Rows.Count > 0)
                {
                    var actualComponents = actualComponentSet.Tables[0].Rows[0][0].ToString().Split(',');
                    componentList.Add(new ComponentEvaluationMetric { ProductId = productId, MinimumSupport = minimumSupport, PredictedComponentList = output.FrequentItems.Select(a => a.Name).ToList(), ActualComponentList = actualComponents.ToList() });

                    foreach (var component in componentList)
                    {
                        Console.WriteLine(component.ProductId);
                        Console.WriteLine(string.Join(",", component.PredictedComponentList.OrderBy(x => x).ToArray()));
                        Console.WriteLine("MetricValue: " + component.MetricValue);
                        Console.WriteLine("Support: " + minimumSupport);
                        Console.WriteLine("----------------------------------------");
                    }
                }
            }
            return componentList;

        }
        private static void ConsoleTesting()
        {
        start:
            Console.WriteLine("Enter Product ID: ");
            var productId = 0; // 434,482,483,502,505
            var input = Console.ReadLine();

            if (int.TryParse(input, out productId))
            {
                //get the noun list for every product
                //var productId = 2213;
                Console.WriteLine("Product Name : " + Utility.MySqlAccess.ExecuteQuery(string.Format("select item_name from item where item_id ={0}", productId)).Tables[0].Rows[0][0].ToString());
                var query = string.Format(@"select nouns_in_review from reviewdata where product_ref_id = {0}", productId);
                var dataSet = Utility.MySqlAccess.ExecuteQuery(query);

                var transactions = new List<string>();
                var distinctNounList = new List<string>();
                foreach (DataRow row in dataSet.Tables[0].Rows)
                {
                    var nounArray = row[0].ToString().Split(',');
                    foreach (var noun in nounArray)
                        distinctNounList.Add(noun);
                    transactions.Add(row[0].ToString().Replace(",", "|"));
                }

                AprioriAlgorithm.IApriori ap = new AprioriAlgorithm.Apriori();
                var output = ap.ProcessTransaction(0.3, 0.3, distinctNounList.Distinct(), transactions.ToArray());
                Console.WriteLine("Total Reviews : " + transactions.Count);
                foreach (var item in output.FrequentItems)
                    Console.WriteLine(item.Name + " | " + item.Support);
                if (output.FrequentItems.Count == 0)
                    Console.WriteLine("No components found.");

                //Console.ReadLine();
                goto start;
            }
            Console.WriteLine("Exiting...");
        }

        private static List<Plot> GenerateGraphData(List<List<ComponentEvaluationMetric>> graphPreparationData)
        {
            var plotList = new List<Plot>();
            foreach(var item in graphPreparationData)
            {
                var x = item[0].MinimumSupport;
                var y = item.Select(m => m.MetricValue).Sum() / item.Count;
                plotList.Add(new Plot { X = x, Y = y });
            }
            return plotList;
        }
    }

    public class ComponentEvaluationMetric
    {
        public int ProductId { get; set; }
        public List<string> PredictedComponentList { get; set; }
        public List<string> ActualComponentList { get; set; }
        public double Precision
        {
            get
            {
                if (PredictedComponentList.Count == 0) return 0;
                var matchCount = 0.0;
                foreach (var predictedItem in PredictedComponentList)
                {
                    foreach (var actualItem in ActualComponentList)
                    {
                        var actualItemList = actualItem.Trim().Split('/');
                        if (actualItemList.Contains(predictedItem.Trim()))
                            matchCount++;
                    }
                }
                return matchCount/PredictedComponentList.Count;
            }
        }
        public double Recall
        {
            get
            {
                var matchCount = 0.0;
                foreach (var predictedItem in PredictedComponentList)
                {
                    foreach (var actualItem in ActualComponentList)
                    {
                        var actualItemList = actualItem.Trim().Split('/');
                        if (actualItemList.Contains(predictedItem.Trim()))
                            matchCount++;
                    }
                }
                return matchCount/ActualComponentList.Count;
            }
        }
        public double MetricValue
        {
            get
            {
                var p = Precision;
                var r = Recall;
                return (p == 0.0 && r == 0.0) ? 0.0 : ((2 * p * r) / (p + r));
            }
        }
        public double MinimumSupport { get; set; }
    }

    public class Plot
    {
        public double X { get; set; }
        public double Y { get; set; }
    }
}
