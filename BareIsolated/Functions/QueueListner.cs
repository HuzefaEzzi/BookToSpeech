using System;
using System.Text;
using System.Xml.Linq;
using Azure.Storage.Queues.Models;
using BookToSpeech.FunctionsApp.Models;
using BookToSpeech.FunctionsApp.Utils;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace BookToSpeech.FunctionsApp.Functions
{
    public class QueueListner
    {
        private readonly ILogger<QueueListner> _logger;

        public QueueListner(ILogger<QueueListner> logger)
        {
            _logger = logger;
        }

        [Function(nameof(QueueListner))]
        public async Task Run([QueueTrigger("sample-worker", Connection = "AzureWebJobsStorage")] QueueMessage message)
        {
            _logger.LogInformation($"C# Queue trigger function processed: {message.MessageText}");
            OutputData inputData = message.Body.ToObjectFromJson<OutputData>();
            string name = inputData.InputFileName;


            string firstFiveLetters = FileUtils.TrimFileName(name);

            string containerName = $"audio-{firstFiveLetters}-{inputData.Id}";


            BlobStorageHandler outputHandler = new BlobStorageHandler("AzureWebJobsStorage", containerName);
            BlobStorageHandler inputHandler = new BlobStorageHandler("AzureWebJobsStorage", inputData.ContainerName);

            string accessToken;
            _logger.LogInformation("Attempting token exchange. Please wait...\n");

            // Add your subscription key here
            // If your resource isn't in WEST US, change the endpoint
            Authentication auth = new Authentication("https://eastus.api.cognitive.microsoft.com/sts/v1.0/issueToken", Environment.GetEnvironmentVariable("AZURE_CONGENTIVE_KEY"));
            try
            {
                accessToken = await auth.FetchTokenAsync().ConfigureAwait(false);
                _logger.LogInformation("Successfully obtained an access token. \n");
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to obtain an access token.");
                _logger.LogError(ex.ToString());
                _logger.LogError(ex.Message);
                return;
            }
          //  await Parallel.ForEachAsync((inputData.Files), async (file, token) =>
           // {
                try
                {
                    await Convert(accessToken, inputData.Files, outputHandler, inputHandler);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error converting file:{inputData.Files}");
                }
           // });
        }

        private async Task Convert(string accessToken, string inputPath, BlobStorageHandler outputHandler, BlobStorageHandler inputHandler)
        {
            var fileName = Path.GetFileNameWithoutExtension(inputPath);
           
            _logger.LogInformation($"Processing {inputPath} ");
            //File.ReadAllText(inputPath);//todod this needsto read form bplob
            string text = await inputHandler.DownloadTextAsync(inputPath);

            string host = "https://eastus.tts.speech.microsoft.com/cognitiveservices/v1";

            // Create SSML document.
            XDocument body = new XDocument(
                    new XElement("speak",
                        new XAttribute("version", "1.0"),
                        new XAttribute(XNamespace.Xml + "lang", "en-US"),
                        new XElement("voice",
                            new XAttribute(XNamespace.Xml + "lang", "en-US"),
                            new XAttribute(XNamespace.Xml + "gender", "Male"),
                            new XAttribute("name", "en-US-DavisNeural"), // Short name for "Microsoft Server Speech Text to Speech Voice (en-US, Jessa24KRUS)"
                            text)));
            using (HttpClient client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromMinutes(30);
                using (HttpRequestMessage request = new HttpRequestMessage())
                {
                    // Set the HTTP method
                    request.Method = HttpMethod.Post;
                    // Construct the URI
                    request.RequestUri = new Uri(host);
                    // Set the content type header
                    request.Content = new StringContent(body.ToString(), Encoding.UTF8, "application/ssml+xml");
                    // Set additional header, such as Authorization and User-Agent
                    request.Headers.Add("Authorization", "Bearer " + accessToken);
                    request.Headers.Add("Connection", "Keep-Alive");
                    // Update your resource name
                    request.Headers.Add("User-Agent", "huzefa-tts-test");
                    // Audio output format. See API reference for full list.
                    request.Headers.Add("X-Microsoft-OutputFormat", "riff-24khz-16bit-mono-pcm");
                    // Create a request
                    _logger.LogInformation($"Calling the TTS service{inputPath}. Please wait... \n");
                    using (HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false))
                    {
                        response.EnsureSuccessStatusCode();
                        // Asynchronously read the response
                        using (Stream dataStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                        {
                            _logger.LogInformation($"Your speech file {inputPath} is being written to file...");
                            await outputHandler.UploadContent(dataStream, fileName + ".wav");
                            //using (FileStream fileStream = new(Path.Combine(outputPath, Path.GetFileNameWithoutExtension(inputPath) + ".wav"), FileMode.Create, FileAccess.Write, FileShare.Write))
                            //{
                            //    await dataStream.CopyToAsync(fileStream).ConfigureAwait(false);
                            //    fileStream.Close();
                            //}
                            _logger.LogInformation($"\nYour file is ready {inputPath}. Press any key to exit.");
                        }
                    }
                }
            }
            _logger.LogInformation($"Completed {inputPath} ");

        }

    }
}
