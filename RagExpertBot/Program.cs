using dotenv.net;
using Microsoft.Extensions.Logging; // Assuming you're using Microsoft.Extensions.Logging for structured logging
using Microsoft.KernelMemory;

public class ChatBot
{
    private readonly IKernelMemory _memory;
    private readonly ILogger<ChatBot> _logger;

    // Constructor that injects dependencies - encourages testability and clean separation of concerns.
    public ChatBot(IKernelMemory memory, ILogger<ChatBot> logger)
    {
        _memory = memory;
        _logger = logger;
    }

    // Initializes documents into the Kernel Memory for use later. Using async to prevent blocking.
    public async Task InitializeDocumentsAsync()
    {
        try
        {
            _logger.LogInformation("Importing documents into memory...");

            // Import both the PDF document and the web page into the Kernel Memory for further retrieval.
            await _memory.ImportDocumentAsync("A_Simple_Guide_to_Retrieval_Augmented_Ge_v4_MEAP.pdf", documentId: "doc001");
            await _memory.ImportWebPageAsync("https://learn.microsoft.com/en-us/azure/databricks/generative-ai/tutorials/ai-cookbook/fundamentals-retrieval-augmented-generation", documentId: "doc002");

            _logger.LogInformation("Documents imported successfully.");
        }
        catch (Exception ex)
        {
            // Log the error and decide if you want to rethrow or handle the error gracefully.
            _logger.LogError(ex, "Error occurred while importing documents.");
            throw; // Rethrow to allow upstream handling or further reporting
        }
    }

    // Verifies whether the documents are ready for querying. Returning a boolean helps streamline control flow.
    public async Task<bool> AreDocumentsReadyAsync()
    {
        _logger.LogInformation("Checking if documents are ready...");

        // Check both documents for readiness in parallel, improving efficiency.
        bool isDoc001Ready = await _memory.IsDocumentReadyAsync("doc001");
        bool isDoc002Ready = await _memory.IsDocumentReadyAsync("doc002");

        return isDoc001Ready && isDoc002Ready;
    }

    // Primary loop that accepts user input and interacts with the memory to answer questions.
    public async Task StartChatAsync()
    {
        Console.WriteLine("Hi, the documents are ready. You can start asking questions!");

        // Infinite loop for the chat interaction. For robustness, this could be terminated by a specific command.
        while (true)
        {
            string userInput = Console.ReadLine();

            // Input validation - Ensures that empty or invalid inputs are not processed.
            if (string.IsNullOrWhiteSpace(userInput))
            {
                Console.WriteLine("Please enter a valid question.");
                continue;
            }

            await HandleUserInputAsync(userInput);
        }
    }

    // Handles the user's input by querying the Kernel Memory and displaying the response.
    private async Task HandleUserInputAsync(string input)
    {
        try
        {
            // Query Kernel Memory with the input to get the answer. Asynchronous handling to avoid blocking the main thread.
            var answer = await _memory.AskAsync(input);

            // Display the result of the query.
            Console.WriteLine($"Question: {input}\n\nAnswer: {answer.Result}");

            // Display sources for transparency. This promotes trustworthiness in generated answers.
            DisplaySources(answer);
        }
        catch (Exception ex)
        {
            // Log errors encountered during question processing.
            _logger.LogError(ex, "Error occurred while processing the user's question.");
            Console.WriteLine("Sorry, an error occurred while processing your request.");
        }
    }

    // Displays the sources from where the data was pulled to form the answer. Essential for retrieval-augmented generation.
    private void DisplaySources(MemoryAnswer answer)
    {
        if (answer.RelevantSources != null && answer.RelevantSources.Any())
        {
            Console.WriteLine("Source(s): ");
            foreach (var source in answer.RelevantSources)
            {
                // If the source has partitions, display its last update to give context to the user.
                var partition = source.Partitions.FirstOrDefault();
                var lastUpdate = partition?.LastUpdate ?? DateTime.MinValue; // Safe handling of null partitions
                Console.WriteLine($" - {source.SourceName} - {source.Link}[{lastUpdate:D}]");
            }
        }
        else
        {
            Console.WriteLine("No sources available for this answer.");
        }
    }
}

public class Program
{
    public static async Task Main(string[] args)
    {
        // Load environment variables from .env file. This ensures sensitive information like API keys are not hardcoded.
        DotEnv.Load();
        var env = DotEnv.Read();

        // Fetch API Key from environment, with basic validation to ensure it's not missing.
        var apiKey = env["OPENAI_API_KEY"];
        if (string.IsNullOrEmpty(apiKey))
        {
            Console.WriteLine("OpenAI API key is missing.");
            return; // Exit early if the API key is not provided.
        }

        // Use ILoggerFactory for proper logging support and create a logger instance for the ChatBot.
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<ChatBot>();

        // Build Kernel Memory using OpenAI Defaults. Inject the logger to trace key events.
        var memory = new KernelMemoryBuilder()
            .WithOpenAIDefaults(apiKey, loggerFactory: loggerFactory)
            .Build<MemoryServerless>();

        var chatBot = new ChatBot(memory, logger);

        try
        {
            // Initialize documents asynchronously and start the chat process once the documents are ready.
            await chatBot.InitializeDocumentsAsync();

            if (await chatBot.AreDocumentsReadyAsync())
            {
                // If documents are ready, proceed to interactive chat mode.
                await chatBot.StartChatAsync();
            }
            else
            {
                Console.WriteLine("Documents are not ready yet. Please try again later.");
            }
        }
        catch (Exception ex)
        {
            // Top-level exception handling to ensure any uncaught errors are logged.
            logger.LogError(ex, "An error occurred while starting the chat.");
        }
    }
}
