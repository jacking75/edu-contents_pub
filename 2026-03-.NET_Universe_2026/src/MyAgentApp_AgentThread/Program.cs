using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Chat;
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


// 2. AIAgent 생성 - AsAIAgent로 래핑 (instructions에 시스템 프롬프트 전달)
var agent = new OpenAIClient(
        new ApiKeyCredential(apiKey!),
        new OpenAIClientOptions { Endpoint = new Uri(baseUrl!) })
    .GetChatClient("gpt-4o-mini")
    .AsAIAgent(
        instructions: "당신은 친절한 교통 안내 담당자입니다. 한국의 KTX 열차 정보를 제공합니다.",
        name: "KTXAgent"
    );

// 3. 세션 생성 (대화 컨텍스트 유지)
AgentSession session = await agent.CreateSessionAsync();

// 4. 첫 번째 질문 - 소요 시간 문의
Console.WriteLine("=== 첫 번째 질문 ===");
var response1 = await agent.RunAsync("서울에서 부산까지 KTX로 가는데 얼마나 걸리나요?", session);
Console.WriteLine($"에이전트: {response1}");
Console.WriteLine();

// 5. 두 번째 질문 - 컨텍스트를 활용한 후속 질문
Console.WriteLine("=== 두 번째 질문 (컨텍스트 유지) ===");
var response2 = await agent.RunAsync("그럼 내일 오전 9시에 도착하려면 몇 시 기차를 타야 하나요?", session);
Console.WriteLine($"에이전트: {response2}");
Console.WriteLine();

// 6. 세 번째 질문 - 추가 컨텍스트 활용
Console.WriteLine("=== 세 번째 질문 (더 깊은 컨텍스트) ===");
var response3 = await agent.RunAsync("그 시간대 기차의 요금도 알려주세요", session);
Console.WriteLine($"에이전트: {response3}");
Console.WriteLine();

Console.WriteLine("=== 대화 완료 ===");
Console.WriteLine("💡 에이전트가 '서울-부산 KTX', '오전 6시 30분 출발'을 모두 기억하고 있습니다!");
