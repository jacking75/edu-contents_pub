using OpenAI;
using OpenAI.Chat;
using Microsoft.Agents.AI;
using System.ClientModel;

// 환경 변수에서 API 키와 Base URL 가져오기
var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
var baseUrl = Environment.GetEnvironmentVariable("OPENAI_BASE_URL");

if (string.IsNullOrWhiteSpace(apiKey))
{
    throw new ArgumentException("환경 변수 'OPENAI_API_KEY'가 설정되지 않았습니다.");
}

if (string.IsNullOrWhiteSpace(baseUrl))
{
    throw new ArgumentException("환경 변수 'OPENAI_BASE_URL'가 설정되지 않았습니다.");
}

// 1. ChatClient 생성 (OpenAI 호환 API 사용)
var openAIClientOptions = new OpenAIClientOptions
{
    Endpoint = new Uri(baseUrl!)
};

var chatClient = new OpenAIClient(new ApiKeyCredential(apiKey!), openAIClientOptions)
    .GetChatClient("gpt-4o-mini");

// 2. AIAgent로 변환
AIAgent agent = chatClient.AsAIAgent(
    instructions: "당신은 친절한 고객 지원 담당자입니다."
);

// 3. 에이전트 실행
// 3. 대화 루프 시작
Console.WriteLine("=== AI 고객 지원 챗봇 ===");
Console.WriteLine("종료하려면 'exit' 또는 'quit'을 입력하세요.");
Console.WriteLine();

while (true)
{
    Console.Write("사용자: ");
    var userInput = Console.ReadLine();

    // 입력이 null이거나 종료 명령어인 경우 루프 종료
    if (string.IsNullOrWhiteSpace(userInput))
    {
        Console.WriteLine("입력이 비어 있습니다. 다시 입력해 주세요.");
        continue;
    }

    if (userInput.Equals("exit", StringComparison.OrdinalIgnoreCase) ||
        userInput.Equals("quit", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine("챗봇을 종료합니다. 감사합니다!");
        break;
    }

    // 4. 에이전트 실행 및 응답 출력
    Console.Write("AI: ");
    var response = await agent.RunAsync(userInput);
    Console.WriteLine(response);
    Console.WriteLine();
}