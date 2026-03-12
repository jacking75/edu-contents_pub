using Anthropic;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;

// ── 1. AI_PROVIDER 환경 변수에 따라 AIAgent 생성 ─────────────────────────
string provider = Environment.GetEnvironmentVariable("AI_PROVIDER") ?? "OpenAI";

AIAgent agent = provider switch
{
    "OpenAI" => CreateOpenAIAgent(),
    "AzureOpenAI" => CreateAzureOpenAIAgent(),
    "Anthropic" => CreateAnthropicAgent(),
    _ => throw new ArgumentException($"지원하지 않는 프로바이더: {provider}")
};

Console.WriteLine($"✅ 사용 중인 AI 프로바이더: {provider}");

// ── 2. 대화 실행 ──────────────────────────────────────────────────────────
//   기존의 List<ChatMessage> + GetResponseAsync() 대신
//   agent.RunAsync(string) 한 줄로 완결됩니다.
var response = await agent.RunAsync("안녕하세요!");
Console.WriteLine($"\n{response}");


// ── 프로바이더 팩토리 메서드 ──────────────────────────────────────────────

// [OpenAI] GetChatClient → AsAIAgent
// 커스텀 Base URL(OpenAI 호환 API)도 동일하게 지원
static AIAgent CreateOpenAIAgent()
{
    var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")
        ?? throw new ArgumentException("OPENAI_API_KEY 환경 변수가 필요합니다.");

    var baseUrl = Environment.GetEnvironmentVariable("OPENAI_BASE_URL");

    OpenAIClientOptions options = string.IsNullOrWhiteSpace(baseUrl)
        ? new OpenAIClientOptions()
        : new OpenAIClientOptions { Endpoint = new Uri(baseUrl) };

    return new OpenAIClient(new ApiKeyCredential(apiKey), options)
        .GetChatClient("gpt-4o-mini")
        .AsAIAgent(
            name: "OpenAIAgent",
            instructions: "You are a helpful assistant."
        );
}

// [Azure OpenAI] AzureOpenAIClient → GetChatClient → AsAIAgent
// API Key 또는 DefaultAzureCredential 자동 선택
static AIAgent CreateAzureOpenAIAgent()
{
    var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")
        ?? throw new ArgumentException("AZURE_OPENAI_ENDPOINT 환경 변수가 필요합니다.");

    var deployment = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT") ?? "gpt-4o-mini";
    var apiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY");

    AzureOpenAIClient azureClient = string.IsNullOrWhiteSpace(apiKey)
        ? new AzureOpenAIClient(new Uri(endpoint), new DefaultAzureCredential())
        : new AzureOpenAIClient(new Uri(endpoint), new ApiKeyCredential(apiKey));

    return azureClient
        .GetChatClient(deployment)
        .AsAIAgent(
            name: "AzureOpenAIAgent",
            instructions: "You are a helpful assistant."
        );
}

// [Anthropic] AnthropicClient → AsAIAgent
// 기존의 OpenAI 호환 우회 방식 대신,
// Microsoft.Agents.AI.Anthropic 공식 패키지 사용
static AIAgent CreateAnthropicAgent()
{
    var apiKey = Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY")
        ?? throw new ArgumentException("ANTHROPIC_API_KEY 환경 변수가 필요합니다.");

    var model = Environment.GetEnvironmentVariable("ANTHROPIC_MODEL")
        ?? "claude-3-5-sonnet-20241022";

    // ✅ APIKey(X) → ApiKey(O) : 공식 Anthropic SDK의 실제 프로퍼티명
    AnthropicClient client = new() { ApiKey = apiKey };

    return client.AsAIAgent(
        model: model,
        name: "AnthropicAgent",
        instructions: "You are a helpful assistant."
    );
}

