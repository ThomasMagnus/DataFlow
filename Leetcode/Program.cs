using System.Diagnostics;
using System.Reactive.Linq;
using System.Threading.Tasks.Dataflow;

namespace LeetCode;

public class Program
{
    public async static Task Main(string[] args)
    {
        TransformBlock<string, string> downloadString = new TransformBlock<string, string>(async url =>
        {
            Console.WriteLine("Downloading '{0}'", url);

            return await new HttpClient(new HttpClientHandler { AutomaticDecompression = System.Net.DecompressionMethods.GZip }).GetStringAsync(url);
        });

        TransformBlock<string, string[]> creatingWordList = new TransformBlock<string, string[]>(text =>
        {
            Console.WriteLine("Creating word list...");

            char[] tokens = text.Select(x => char.IsLetter(x) ? x : ' ').ToArray();

            text = new string(tokens);

            return text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        });

        TransformBlock<string[], string[]> filterWordsList = new TransformBlock<string[], string[]>(words =>
        {
            Console.WriteLine("Filtering word list");

            return words
                .Where(x => x.Length > 3)
                .Distinct()
                .ToArray();
        });

        TransformManyBlock<string[], string> findReversedWords = new TransformManyBlock<string[], string>(words =>
        {
            Console.WriteLine("Finding reversed words...");

            HashSet<string> wordSet = new HashSet<string>();

            return from word in words.AsParallel()
                   let reverse = new string(word.Reverse().ToArray())
                   where word != reverse && wordSet.Contains(reverse)
                   select word;
        });

        ActionBlock<string> printReversedWords = new ActionBlock<string>(reversedWord =>
        {
            Console.WriteLine("Found reversed words {0}/{1}", reversedWord, new string(reversedWord.Reverse().ToArray()));
        });


        DataflowLinkOptions linkOptions = new DataflowLinkOptions { PropagateCompletion = true };

        downloadString.LinkTo(creatingWordList, linkOptions);
        creatingWordList.LinkTo(filterWordsList, linkOptions);
        filterWordsList.LinkTo(findReversedWords, linkOptions);
        findReversedWords.LinkTo(printReversedWords, linkOptions);

        downloadString.Post("http://www.gutenberg.org/cache/epub/16452/pg16452.txt");

        downloadString.Complete();

        printReversedWords.Completion.Wait();
    }

    async static Task Test1()
    {
        Console.WriteLine("Начало выполнение функции Test1");

        await Task.Delay(3000);

        Console.WriteLine("Окончание выполнения функции Test1");
    }

    async static Task Test2()
    {
        Console.WriteLine("Начало выполнение функции Test2");

        await Task.Delay(2000);

        Console.WriteLine("Окончание выполнения функции Test2");
    }
}