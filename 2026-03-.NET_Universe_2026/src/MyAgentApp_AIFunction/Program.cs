using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using System.ComponentModel;

// 환경 변수에서 API 키와 Base URL 가져오기
var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
var baseUrl = Environment.GetEnvironmentVariable("OPENAI_BASE_URL");

if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(baseUrl))
{
    throw new ArgumentException("환경 변수 'OPENAI_API_KEY' 또는 'OPENAI_BASE_URL'가 설정되지 않았습니다.");
}

// 1. 날씨 조회 함수 정의
[Description("지정된 도시의 현재 날씨를 조회합니다")]
static string GetWeather([Description("날씨를 조회할 도시 이름")] string city)
{
    return city switch
    {
        "서울" => "맑음, 기온 15도",
        "부산" => "흐림, 기온 18도",
        "제주" => "비, 기온 12도",
        "대구" => "맑음, 기온 17도",
        _ => "정보 없음"
    };
}

// 2. Tool을 포함한 Agent 생성
var agent = new OpenAIClient(
        new ApiKeyCredential(apiKey!),
        new OpenAIClientOptions { Endpoint = new Uri(baseUrl!) })
    .GetChatClient("gpt-4o-mini")
    .AsAIAgent(
        instructions: @"
당신은 날씨 정보를 제공하는 친절한 어시스턴트입니다.

목표: 사용자가 요청한 도시의 날씨 정보를 정확하게 전달합니다.

행동 방식:
- GetWeather 함수를 사용하여 실제 날씨 정보를 조회합니다
- 조회한 정보를 자연스럽고 친근한 말투로 전달합니다
- 날씨에 따라 적절한 조언을 추가합니다
- 한국의 주요 도시 이름을 정확히 이해합니다
",
        tools: [AIFunctionFactory.Create(GetWeather)]
    );

// 3. 사용자 인터페이스
Console.WriteLine("╔════════════════════════════════════════════╗");
Console.WriteLine("║      🌤️  날씨 조회 에이전트에 오신 것을 환영합니다!      ║");
Console.WriteLine("╚════════════════════════════════════════════╝");
Console.WriteLine();
Console.WriteLine("💡 도시 이름을 입력하면 날씨를 알려드립니다!");
Console.WriteLine("   (예: 서울 날씨 알려줘, 부산은 날씨가 어때?)");
Console.WriteLine();
Console.WriteLine("종료하려면 'exit'를 입력하세요");
Console.WriteLine(new string('─', 50));
Console.WriteLine();

// 4. 대화 루프
while (true)
{
    // 사용자 입력
    Console.Write("You: ");
    string userInput = Console.ReadLine() ?? "";

    if (string.IsNullOrWhiteSpace(userInput))
        continue;

    if (userInput.ToLower() == "exit")
    {
        Console.WriteLine("\n👋 안녕히 가세요! 좋은 하루 되세요!");
        break;
    }

    try
    {
        // 에이전트 실행
        Console.Write("Assistant: ");
        var response = await agent.RunAsync(userInput);
        Console.WriteLine(response);
        Console.WriteLine();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ 오류: {ex.Message}");
        Console.WriteLine();
    }
}