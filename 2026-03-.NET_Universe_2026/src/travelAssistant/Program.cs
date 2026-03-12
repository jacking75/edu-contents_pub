using Microsoft.Agents.AI;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;

// ┌────────────────────────────────────────────────────┐
// │ 1. 기본 설정                                       │
// └────────────────────────────────────────────────────┘
string apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")
    ?? throw new Exception("OPENAI_API_KEY를 설정해주세요");

var baseUrl = Environment.GetEnvironmentVariable("OPENAI_BASE_URL");
string model = Environment.GetEnvironmentVariable("OPENAI_MODEL") ?? "gpt-4o-mini";

var openAIClientOptions = new OpenAIClientOptions();
if (!string.IsNullOrEmpty(baseUrl))
    openAIClientOptions.Endpoint = new Uri(baseUrl);

// ┌────────────────────────────────────────────────────┐
// │ 2. AI 여행 가이드 어시스턴트 생성 (AsAIAgent)     │
// └────────────────────────────────────────────────────┘
// ✅ GetChatClient().AsAIAgent() 체이닝으로 에이전트 완성
// ✅ System 메시지(instructions)는 AsAIAgent의 파라미터로 선언
// ✅ 수동 SystemChatMessage + List<ChatMessage> 관리가 완전히 불필요
AIAgent travelGuide = new OpenAIClient(new ApiKeyCredential(apiKey), openAIClientOptions)
    .GetChatClient(model)
    .AsAIAgent(
        name: "TravelGuide",
        instructions: @"
당신은 친절하고 박식한 여행 가이드 어시스턴트입니다.

역할:
- 여행지 추천, 여행 계획 수립을 도와줍니다
- 고객의 선호도와 예산을 고려하여 맞춤 추천을 합니다
- 이전 대화 내용을 정확히 기억하고 활용합니다

응답 방식:
- 친근하고 열정적인 톤으로 말하세요
- 고객이 언급한 도시, 날짜, 선호사항을 정확히 기억하세요
- '그곳', '거기', '그때', '아까 말한' 같은 표현을 문맥으로 이해하세요
- 구체적이고 실용적인 조언을 제공하세요

응답 예시:
- 고객: '일본 여행 가고 싶어요'
  답변: 일본 여행 좋으시네요! 어느 도시를 생각하고 계신가요? 도쿄, 오사카, 교토 등 각각 매력이 있어요 ✈️

- 고객: '도쿄요'
  답변: 도쿄 선택하셨군요! 며칠 정도 여행을 계획하고 계신가요?

- 고객: '3박 4일이요. 거기 날씨는 어때요?'
  답변: 도쿄로 3박 4일 여행이시군요! 언제 가실 예정이신가요? 계절마다 날씨가 많이 다르답니다 🌸
"
    );

// ┌────────────────────────────────────────────────────┐
// │ 3. 대화 세션 생성 (핵심!)                         │
// └────────────────────────────────────────────────────┘
// ✅ AgentSession이 대화 이력을 자동 관리
//    → 수동 conversationHistory.Add(...) 완전 제거
AgentSession session = await travelGuide.CreateSessionAsync();

// ┌────────────────────────────────────────────────────┐
// │ 4. 사용자 인터페이스                               │
// └────────────────────────────────────────────────────┘
Console.WriteLine("╔════════════════════════════════════════════╗");
Console.WriteLine("║    ✈️  TravelGuide에 오신 것을 환영합니다!      ║");
Console.WriteLine("║      여행 계획을 도와드리겠습니다! 🗺️      ║");
Console.WriteLine("╚════════════════════════════════════════════╝");
Console.WriteLine();
Console.WriteLine("💡 팁: 이전 대화를 기억합니다!");
Console.WriteLine("   '그곳', '거기', '아까 말한' 같은 표현을 자유롭게 사용하세요");
Console.WriteLine();
Console.WriteLine("📝 테스트 예시:");
Console.WriteLine("   1. '제주도 여행 가고 싶어요'");
Console.WriteLine("   2. '거기 맛집 추천해줘'");
Console.WriteLine("   3. '아까 말한 곳 근처 호텔도 알려줘'");
Console.WriteLine();
Console.WriteLine("종료하려면 'exit'를 입력하세요");
Console.WriteLine(new string('─', 50));
Console.WriteLine();

// ┌────────────────────────────────────────────────────┐
// │ 5. 대화 루프 (메인 로직)                          │
// └────────────────────────────────────────────────────┘
while (true)
{
    Console.Write("You: ");
    string userInput = Console.ReadLine() ?? "";

    if (string.IsNullOrWhiteSpace(userInput))
        continue;

    if (userInput.ToLower() == "exit")
    {
        Console.WriteLine("\n👋 안녕히 가세요! 즐거운 여행 되세요!");
        break;
    }

    try
    {
        // ✅ session을 전달하는 것만으로 대화 컨텍스트 자동 유지
        // ✅ RunStreamingAsync로 실시간 스트리밍 출력
        Console.Write("Assistant: ");

        await foreach (var update in travelGuide.RunStreamingAsync(userInput, session))
        {
            // update.Text: 스트리밍으로 도착하는 텍스트 조각
            Console.Write(update.Text);
        }

        Console.WriteLine();
        Console.WriteLine();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ 오류: {ex.Message}");
        Console.WriteLine();
    }
}
