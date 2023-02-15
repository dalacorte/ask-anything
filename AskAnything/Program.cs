using AskAnything;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using static AskAnything.CompletionResponse;

var appsettingReader = new AppsettingsReader();
var secrets = appsettingReader.ReadSection<OpenAI>("OpenAI");

if (args.Length > 0)
{
    using (var client = new HttpClient())
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(secrets.Scheme, secrets.APIKey);

        var builder = new UriBuilder("https://api.openai.com/v1/completions");

        var httpRequest = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri(builder.ToString())
        };

        var completion = new Completion();
        completion.Model = "text-davinci-001";
        completion.Prompt = args[0];
        completion.MaxTokens = 100;
        completion.Temperature = 1;
        //completion.TopP = 1;
        //completion.N = 1;
        //completion.Stream = false;
        //completion.Logprobs = null;
        //completion.Stop = "\n";

        var json = JsonConvert.SerializeObject(completion);

        httpRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");

        var responseMessage = await client.SendAsync(httpRequest);

        try
        {
            var response = JsonConvert.DeserializeObject<Response>(await responseMessage.Content.ReadAsStringAsync());
            var text = response.Choices[0].Text;

            GuessCommand(text);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}
else
{
    Console.WriteLine("Ask something, anything");
}

static void GuessCommand(string command)
{
    Console.WriteLine("-> GPT-3 response:");
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine(command);

    var lastIndex = command.LastIndexOf('\n');

    var guess = command.Substring(lastIndex + 1);

    Console.ResetColor();

    TextCopy.ClipboardService.SetText(guess);
}