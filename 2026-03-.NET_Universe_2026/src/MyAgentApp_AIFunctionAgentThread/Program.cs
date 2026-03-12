using System.ComponentModel;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using Microsoft.Extensions.AI;

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

// 1. 날씨 조회 함수 정의 ([Description] 속성으로 도구 메타데이터 제공)
[Description("지정된 도시의 현재 날씨를 조회합니다")]
static string GetWeather([Description("날씨를 조회할 도시 이름")] string city)
{
    return city switch
    {
        "서울" => "맑음, 기온 15도",
        "부산" => "흐림, 기온 18도",
        _ => "정보 없음"
    };
}

// 2. ChatClient 생성 후 AIAgent로 변환 (AIFunctionFactory로 도구 등록)
var agent = new OpenAIClient(
        new ApiKeyCredential(apiKey!),
        new OpenAIClientOptions { Endpoint = new Uri(baseUrl!) })
    .GetChatClient("gpt-4o-mini")
    .AsAIAgent(
        instructions: "당신은 날씨 정보를 제공하는 친절한 도우미입니다.",
        tools: [AIFunctionFactory.Create(GetWeather)]
    );

// 3. AgentSession 생성 (멀티턴 대화 컨텍스트 유지)
var session = await agent.CreateSessionAsync();

Console.WriteLine("=== 날씨 조회 멀티턴 예제 (세션으로 대화 맥락 유지) ===");
Console.WriteLine();

// 4. 첫 번째 질문: 날씨 조회
Console.WriteLine("사용자: 서울 날씨 알려줘");
Console.WriteLine();
var response1 = await agent.RunAsync("서울 날씨 알려줘", session);
Console.WriteLine($"에이전트: {response1}");
Console.WriteLine();

// 5. 두 번째 질문: 앞 질문의 날씨 정보를 참조해야만 답할 수 있는 질문
//    "거기" = 세션에 기억된 서울, "그 날씨" = 앞서 조회한 맑음/15도
Console.WriteLine("사용자: 거기 날씨면 오늘 외출하기 좋을까? 옷차림도 추천해줘");
Console.WriteLine();
var response2 = await agent.RunAsync("거기 날씨면 오늘 외출하기 좋을까? 옷차림도 추천해줘", session);
Console.WriteLine($"에이전트: {response2}");
Console.WriteLine();

// 6. 세 번째 질문: 다른 도시 비교 (이전 서울 날씨 맥락과 함께)
Console.WriteLine("사용자: 부산은 어때? 서울이랑 비교해줘");
Console.WriteLine();
var response3 = await agent.RunAsync("부산은 어때? 서울이랑 비교해줘", session);
Console.WriteLine($"에이전트: {response3}");
Console.WriteLine();

Console.WriteLine("=== 실행 완료 ===");
Console.WriteLine("💡 session 덕분에 에이전트가 이전 대화 내용을 기억하고 연결하여 답변했습니다!");
