using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace MicrosoftCrawlerAssessment
{
    class Program
    {
        private static readonly string PageUri = @"https://en.wikipedia.org/wiki/Microsoft";
        private static readonly HttpClient Client = new HttpClient();

        public static void Main(string[] args)
        {
            //Run continuously
            while (true)
            {
                //Get user input
                Console.WriteLine("INFO: Microsoft Wikipedia History Parser!");
                Console.WriteLine("INFO: Program supports blank inputs.");
                Console.WriteLine("INFO: Words to exclude are case sensitive.");
                Console.WriteLine("INFO: Enter the top number of words to get (Positive Integer):");
                string topInput = Console.ReadLine();
                int top = String.IsNullOrEmpty(topInput) ? 10 : Int32.Parse(topInput);
                Console.WriteLine("INFO: Enter words to exclude as a COMMA SEPARATED list (Microsoft,Windows,etc.):");
                string[] blacklistInput = Console.ReadLine().Split(',');
                Console.WriteLine("INFO: Getting top " + top + " words, excluding: " + String.Join(",", blacklistInput) + "...");
                List<string> blacklist = blacklistInput.ToList();

                //Get page content and load using HtmlAgilityPack
                string htmlContent = CompletePageCrawlRequest().Result;
                HtmlDocument document = new HtmlDocument();
                document.LoadHtml(htmlContent);
                var historyNode = document.GetElementbyId("History").ParentNode;

                //Get all inner text in the specified section
                string historyText = GetAllInnerText(historyNode, "<h2>");

                //Get and print top sorted words according to inputs
                PrintSortedWords(top, blacklist, historyText);

                Console.WriteLine("INFO: Success!");
            }
        }

        public static string GetAllInnerText(HtmlNode startNode, string htmlSeparator)
        {
            string allInnerText = string.Empty;
            var curNode = startNode.NextSibling;

            while (curNode != null)
            {
                //break out of the parse loop if we reach the next section
                if (curNode.OuterHtml.StartsWith(htmlSeparator))
                    break;
                else
                {
                    string curInnerText = HtmlEntity.DeEntitize(curNode.InnerText);
                    allInnerText += curInnerText;
                    curNode = curNode.NextSibling;
                }
            }
            return allInnerText;
        }

        public static void PrintSortedWords(int top, List<string> blacklist, string textContent)
        {
            List<string> allWords = textContent.Split(' ', '\n', '.', ',', '"').ToList();
            Dictionary<string, int> wordCounts = new Dictionary<string, int>();
            foreach (string word in allWords)
            {
                if (!string.IsNullOrEmpty(word) && !blacklist.Contains(word))
                {
                    if (wordCounts.ContainsKey(word))
                        wordCounts[word]++;
                    else
                        wordCounts.Add(word, 1);
                }
            }
            var sorted = wordCounts.ToList();
            sorted.Sort((pair1, pair2) => pair2.Value.CompareTo(pair1.Value));
            string results = string.Join("\n", sorted.Select(x => x.Key + " => " + x.Value).ToArray());
            for (int i = 0; i < top; i++)
                Console.WriteLine(sorted[i]);
        }

        public static async Task<string> CompletePageCrawlRequest()
        {
            try
            {
                HttpResponseMessage response = await Client.GetAsync(PageUri);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                return await Task.FromResult(responseBody);
            }
            catch (Exception e)
            {
                Console.WriteLine("EXCEPTION Message: {0}, StackTrace: {1}", e.Message, e.StackTrace);
                return string.Empty;
            }
        }
    }
}
