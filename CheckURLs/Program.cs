using System.Text.RegularExpressions;

class Program
{
    static async Task Main(string[] args)
    {
        // Console.WriteLine("Enter the folder path:");
        // string folderPath = Console.ReadLine();
        // Console.WriteLine("Enter the output file path:");
        // string outputFilePath = Console.ReadLine();
        
        // string folderPath = "C:\\work\\demo\\DemoServer\\Controllers\\Demos";
        // string folderPath = "C:\\work\\demo\\DemoServer\\AdditionalLanguages\\nodejs\\demo";
        // string folderPath = "C:\\work\\demo\\DemoServer\\AdditionalLanguages\\go";
        // string folderPath = "C:\\work\\demo\\DemoServer\\AdditionalLanguages\\java\\src\\main\\java\\net\\ravendb\\demo";
        // string folderPath = "C:\\work\\demo\\DemoServer\\AdditionalLanguages\\php";
        string folderPath = "C:\\work\\demo\\DemoServer\\AdditionalLanguages\\python";
        
        // string folderPath = "C:\\work\\demo\\DemoServer\\Controllers\\Demos\\TextSearch";
        // string folderPath = "C:\\work\\demo\\DemoServer\\Controllers\\Demos\\Spatial";
        string outputFilePath = "E:\\CheckURLs\\invalid_Python.txt";

        HashSet<string> uniqueUrls = new HashSet<string>();
        await ProcessFolderAsync(folderPath, outputFilePath, uniqueUrls);
    }

    static async Task ProcessFolderAsync(string folderPath, string outputFilePath, HashSet<string> uniqueUrls)
    {
        try
        {
            foreach (var subFolder in Directory.EnumerateDirectories(folderPath, "*", SearchOption.AllDirectories))
            {
                foreach (var file in Directory.EnumerateFiles(subFolder, "metadata.json"))
                {
                    await ProcessFileAsync(file, outputFilePath, uniqueUrls, file);
                }
            }
            Console.WriteLine("Processing complete.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }

    static async Task ProcessFileAsync(string filePath, string outputFilePath, HashSet<string> uniqueUrls, string fileName)
    {
        using (var httpClient = new HttpClient())
        using (StreamWriter outputFile = new StreamWriter(outputFilePath, true))
        {
            // await outputFile.WriteLineAsync(Environment.NewLine + "name = " + fileName);
            await outputFile.WriteLineAsync("FileName = " + fileName);
            
            foreach (string line in File.ReadLines(filePath))
            {
                var match = Regex.Match(line, @"""Url""\s*:\s*""(https?://[^""]+)""");
                if (match.Success)
                {
                    string url = ExtractUrlFromLine(line);
                    if (uniqueUrls.Add(url) && !await UrlExistsAsync(httpClient, url))
                    {
                        await outputFile.WriteLineAsync("=> " + url);
                    }
                }
            }
        }
    }

    static string ExtractUrlFromLine(string line)
    {
        var match = Regex.Match(line, "\"Url\"\\s*:\\s*\"(.*?)\"");
        return match.Groups[1].Value;
    }
    
    static async Task<bool> UrlExistsAsync(HttpClient client, string url)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Head, url);
            var response = await client.SendAsync(request);

            var responseUriString = response.RequestMessage.RequestUri.ToString();
            var hasNotFound = responseUriString.Contains("not-found");

            // Ensure the response is disposed of after checking the URL
            response.Dispose();

            // If hasNotFound is FALSE than url is good.
            // if hasNotFound is TRUE then url is bad.
            return hasNotFound == false; 
        }
        catch
        {
            return false;
        }
    }
}
