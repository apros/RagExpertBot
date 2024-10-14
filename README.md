# ChatBot with Retrieval-Augmented Generation

This project implements a ChatBot using Retrieval-Augmented Generation (RAG) with Microsoft's Kernel Memory. The ChatBot can answer questions based on imported documents and web pages.

## Features

- Imports PDF documents and web pages into Kernel Memory
- Answers user questions using the imported knowledge base
- Provides sources for the answers, enhancing transparency
- Utilizes OpenAI's API for natural language processing

## Prerequisites

- .NET 6.0 or later
- OpenAI API key

## Setup

1. Clone the repository
2. Create a `.env` file in the root directory with your OpenAI API key:
   ```
   OPENAI_API_KEY=your_api_key_here
   ```
3. Ensure you have the required NuGet packages installed:
   - dotenv.net
   - Microsoft.Extensions.Logging
   - Microsoft.KernelMemory

## Usage

1. Run the program:
   ```
   dotnet run
   ```
2. The ChatBot will import the specified documents and web pages.
3. Once the documents are ready, you can start asking questions.
4. Type your questions and press Enter to receive answers.

   ![Screenshot 2024-10-13 211731](https://github.com/user-attachments/assets/f4ef4491-1764-4b48-9adb-afaefdf467c4)


## Project Structure

- `Program.cs`: Contains the main entry point and initialization logic
- `ChatBot.cs`: Implements the ChatBot class with core functionality

## Key Components

- `ChatBot`: Main class handling document import, user interaction, and question answering
- `KernelMemory`: Utilizes Microsoft's Kernel Memory for document storage and retrieval
- `ILogger`: Implements logging for better debugging and monitoring

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

MIT license

## Acknowledgements

- This project uses [Microsoft's Kernel Memory](https://github.com/microsoft/kernel-memory) for Retrieval-Augmented Generation.
- OpenAI's API is used for natural language processing.
