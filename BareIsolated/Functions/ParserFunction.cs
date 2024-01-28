using BookToSpeech.FunctionsApp.Models;
using BookToSpeech.FunctionsApp.Utils;
using BookToSpeech.EpubParser;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;


namespace BookToSpeech.FunctionsApp.Functions
{
    public class ParserFunction
    {
        private readonly ILogger<ParserFunction> _logger;

        public ParserFunction(ILogger<ParserFunction> logger)
        {
            _logger = logger;
        }

        [Function(nameof(ParserFunction))]
        public async Task<ParsedOutputType> Run([BlobTrigger("samples-workitems/{name}", Connection = "AzureWebJobsStorage")] Stream stream, string name)
        {

            _logger.LogInformation($"C# Blob trigger function Processed blob\n Name: {name} \n");
            var contents = ExtractPlainText.Run(stream);

            string firstFiveLetters = FileUtils.TrimFileName(name);
            string id = Random.Shared.Next().ToString();
            string containerName = $"parsed-{firstFiveLetters}-{id}";
            BlobStorageHandler handler = new BlobStorageHandler("AzureWebJobsStorage", containerName);

            List<string> files = new List<string>();
            foreach (var item in contents)
            {
                string fileName = FileUtils.MakeValidFileName(item.Item1);
                files.Add(fileName);
                _logger.LogInformation($"writing File: {fileName}");
                await handler.UploadContent(item.Item2, fileName);
            }
            //todo find out how we can put messages in queue when parsing file individually
            return new ParsedOutputType()
            {
                Name = containerName,
                OutputData = files.Select(s => new OutputData()
                {
                    Id = id,
                    ContainerName = containerName,
                    InputFileName = name,
                    Files = s
                })
            };
        }


    }

}
