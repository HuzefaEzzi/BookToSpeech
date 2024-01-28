using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;


namespace BookToSpeech.FunctionsApp.Models
{
    //public class ParsedOutputType
    //{
    //    public string Name { get; set; }

    //    [QueueOutput("sample-worker", Connection = "AzureWebJobsStorage")]
    //    public OutputData OutputData { get; set; }
    //}

    //public class OutputData
    //{
    //    public string Id { get; set; }
    //    public string[] Files { get; set; }
    //    public string InputFileName { get; set; }

    //    public string ContainerName { get; set; }
    //}


    public class ParsedOutputType
    {
        public string Name { get; set; }

        [QueueOutput("sample-worker", Connection = "AzureWebJobsStorage")]
        public IEnumerable<OutputData> OutputData { get; set; }
    }

    public class OutputData
    {
        public string Id { get; set; }
        public string Files { get; set; }
        public string InputFileName { get; set; }

        public string ContainerName { get; set; }
    }
}
