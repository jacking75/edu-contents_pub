using Microsoft.Agents.AI;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;

// ── 환경 변수에서 API 키와 Base URL 가져오기 ──────────────────────────────
var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
var baseUrl = Environment.GetEnvironmentVariable("OPENAI_BASE_URL");

if (string.IsNullOrWhiteSpace(apiKey))
    throw new ArgumentException("환경 변수 'OPENAI_API_KEY'가 설정되지 않았습니다.");

if (string.IsNullOrWhiteSpace(baseUrl))
    throw new ArgumentException("환경 변수 'OPENAI_BASE_URL'가 설정되지 않았습니다.");

// ── 1. AIAgent 생성 (AsAIAgent 확장 메서드 사용) ─────────────────────────
//   OpenAI 호환 API (Custom Base URL) 를 사용합니다.
//   GetChatClient() → AsAIAgent() 체이닝으로 단 두 줄에 에이전트 완성.
var openAIClientOptions = new OpenAIClientOptions
{
    Endpoint = new Uri(baseUrl!)
};

AIAgent ktxAgent = new OpenAIClient(new ApiKeyCredential(apiKey!), openAIClientOptions)
    .GetChatClient("gpt-4o-mini")
    .AsAIAgent(
        name: "KTX안내봇",
        instructions: "당신은 친절한 교통 안내 담당자입니다. 한국의 KTX 열차 정보를 제공합니다."
    );

// ========================================================================
// 🔥 핵심: 각 고객마다 독립적인 AgentSession 생성
//    → 대화 이력(컨텍스트)은 Session이 자동 관리하므로
//      수동 List<ChatMessage> 조작이 완전히 불필요합니다.
// ========================================================================

// ── 2-A. 고객 A 세션 생성 (서울-부산) ────────────────────────────────────
Console.WriteLine("====================================");
Console.WriteLine("👤 고객 A의 대화 시작 (서울-부산)");
Console.WriteLine("====================================");

AgentSession sessionA = await ktxAgent.CreateSessionAsync();

// 고객 A - 첫 번째 질문
var responseA1 = await ktxAgent.RunAsync("서울에서 부산까지 KTX로 가는데 얼마나 걸리나요?", sessionA);
Console.WriteLine($"에이전트 → 고객A: {responseA1}");
Console.WriteLine();

// 고객 A - 두 번째 질문 (sessionA가 이전 대화를 자동으로 기억)
var responseA2 = await ktxAgent.RunAsync("그럼 내일 오전 9시에 도착하려면 몇 시 기차를 타야 하나요?", sessionA);
Console.WriteLine($"에이전트 → 고객A: {responseA2}");
Console.WriteLine();


// ── 2-B. 고객 B 세션 생성 (서울-대전) ────────────────────────────────────
Console.WriteLine("====================================");
Console.WriteLine("👤 고객 B의 대화 시작 (서울-대전)");
Console.WriteLine("====================================");

AgentSession sessionB = await ktxAgent.CreateSessionAsync();

// 고객 B - 첫 번째 질문
var responseB1 = await ktxAgent.RunAsync("서울에서 대전까지 KTX 소요 시간을 알려주세요", sessionB);
Console.WriteLine($"에이전트 → 고객B: {responseB1}");
Console.WriteLine();

// 고객 B - 두 번째 질문 (sessionB가 이전 대화를 자동으로 기억)
var responseB2 = await ktxAgent.RunAsync("그 경로의 첫차 시간이 언제인가요?", sessionB);
Console.WriteLine($"에이전트 → 고객B: {responseB2}");
Console.WriteLine();


// ── 2-C. 고객 A가 다시 질문 (sessionA 컨텍스트 유지 확인) ─────────────────
Console.WriteLine("====================================");
Console.WriteLine("👤 고객 A가 다시 질문 (컨텍스트 유지)");
Console.WriteLine("====================================");

// 고객 A - 세 번째 질문 (sessionA는 이전 두 번의 대화를 그대로 기억)
var responseA3 = await ktxAgent.RunAsync("아까 말한 그 시간대 기차의 요금도 알려주세요", sessionA);
Console.WriteLine($"에이전트 → 고객A: {responseA3}");
Console.WriteLine();


// ── 결과 요약 ──────────────────────────────────────────────────────────────
Console.WriteLine("====================================");
Console.WriteLine("✅ 결과 요약");
Console.WriteLine("====================================");
Console.WriteLine("💡 하나의 AIAgent 인스턴스(ktxAgent)를 공유하지만");
Console.WriteLine("   각 고객마다 독립적인 AgentSession(sessionA, sessionB)으로 대화를 분리!");
Console.WriteLine();
Console.WriteLine("✔️ 고객 A: 서울-부산 / 오전 9시 도착 / 요금 문의  (sessionA)");
Console.WriteLine("✔️ 고객 B: 서울-대전 / 첫차 시간 문의             (sessionB)");
Console.WriteLine();
Console.WriteLine("✔️ 두 대화는 서로 영향을 주지 않고 독립적으로 관리됨!");
