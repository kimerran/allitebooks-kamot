using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CsQuery;
using Flurl.Http;
using System.Text.RegularExpressions;

namespace AllITEbooksScrapper
{
    class Program
    {
        static void Main(string[] args)
        {
            Task t = TrySearch();
            t.Wait();
        }

        static async Task TrySearch()
        {
            var tasks = new List<Task>();

            for (int i = 1; i <= 167; i++)
            {
                var url = $"http://www.allitebooks.com/web-development/page/{i}";
                tasks.Add(Perform(url));
            }
            await Task.WhenAll(tasks);
        }

        static async Task Perform(string url)
        {
            var client = url
                            .WithHeader("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8")
                            .WithHeader("Accept-Language", "en-US,en;q=0.8")
                            .WithHeader("Accept-Encoding", "gzip, deflate, sdch")
                            .WithHeader("Connection", "keep-alive")
                            .WithHeader("Host", "www.allitebooks.com");

            var results = await client.GetStringAsync();
            var dom = CQ.Create(results);
            var anchors = dom.Select("h2.entry-title");

            foreach (var anchor in anchors)
            {
                var child = anchor.FirstChild.ToString();
                Match match = Regex.Match(child, @"\""(.*)\""");

                var res = match.Groups[1];
                var level2 = res.Value.Split('"')[0];
                var client2 = level2
                                .WithHeader("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8")
                                .WithHeader("Accept-Language", "en-US,en;q=0.8")
                                .WithHeader("Accept-Encoding", "gzip, deflate, sdch")
                                .WithHeader("Connection", "keep-alive")
                                .WithHeader("Host", "www.allitebooks.com");

                var results2 = await client2.GetStringAsync();
                var dom2 = CQ.Create(results2);
                var downloadLinks = dom2.Select("span.download-links");

                foreach (var dlink in downloadLinks)
                {
                    var linkContent = dlink.FirstElementChild.ToString();

                    if (linkContent.Contains("file"))
                    {
                        Match match2 = Regex.Match(linkContent, @"([""'])(?:(?=(\\?))\2.)*?\1");
                        var res2 = match2.Value.Replace(@"""", "");
                        var pdfLink = res2;
                        Console.WriteLine($"{pdfLink}");
                    }
                }
            }
        }
    }
}
