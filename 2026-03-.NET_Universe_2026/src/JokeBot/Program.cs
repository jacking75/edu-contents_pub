using Microsoft.Agents.AI;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using System.Text;

// ┌────────────────────────────────────────────────────┐
// │ Step 1: API 키 및 모델 설정                        │
// └────────────────────────────────────────────────────┘
string apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")
    ?? throw new Exception("OPENAI_API_KEY 환경 변수를 설정해주세요");

var baseUrl = Environment.GetEnvironmentVariable("OPENAI_BASE_URL")
    ?? throw new Exception("OPENAI_BASE_URL 환경 변수를 설정해주세요");

string model = Environment.GetEnvironmentVariable("OPENAI_MODEL")
    ?? "gpt-4o-mini";

// ┌────────────────────────────────────────────────────┐
// │ Step 2: ChatClient 생성                            │
// └────────────────────────────────────────────────────┘
var openAIClientOptions = new OpenAIClientOptions
{
    Endpoint = new Uri(baseUrl!)
};
var chatClient = new OpenAIClient(new ApiKeyCredential(apiKey!), openAIClientOptions).GetChatClient(model);

// ┌────────────────────────────────────────────────────┐
// │ Step 3: AIAgent 생성 (핵심!)                       │
// └────────────────────────────────────────────────────┘
AIAgent jokeBot = chatClient.AsAIAgent(
    instructions: @"
        당신은 유머 감각이 뛰어난 코미디언입니다.
        
        역할:
        - 사용자가 요청한 주제에 대한 재미있는 농담을 만듭니다
        - 농담은 건전하고 가족 친화적이어야 합니다
        - 말장난이나 상황극을 활용하세요
        
        스타일:
        - 친근하고 재미있는 톤으로 말합니다
        - 이모지를 적절히 사용하세요 (😄, 😂, 🤣)
        - 농담 후에는 청중의 반응을 기다리세요
        
        제약:
        - 불쾌감을 줄 수 있는 농담은 피하세요
        - 정치, 종교, 성별 관련 민감한 주제는 다루지 마세요
    "
);

// ┌────────────────────────────────────────────────────┐
// │ Step 4: 사용자 입력 받기                           │
// └────────────────────────────────────────────────────┘
Console.WriteLine("╔════════════════════════════════════════╗");
Console.WriteLine("║        🤖 JokeBot에 오신 것을 환영합니다!       ║");
Console.WriteLine("║     재미있는 농담을 들려드릴게요! 😄    ║");
Console.WriteLine("╚════════════════════════════════════════╝");
Console.WriteLine();

Console.Write("어떤 주제의 농담을 듣고 싶으세요? ");
string topic = Console.ReadLine() ?? "컴퓨터";

// ┌────────────────────────────────────────────────────┐
// │ Step 5: 에이전트 실행                              │
// └────────────────────────────────────────────────────┘
Console.WriteLine();
Console.WriteLine("🤔 생각 중...");
Console.WriteLine();

var joke = await jokeBot.RunAsync($"{topic}에 대한 농담을 해줘");

// ┌────────────────────────────────────────────────────┐
// │ Step 6: 결과 출력                                  │
// └────────────────────────────────────────────────────┘
Console.WriteLine("🎭 JokeBot:");
Console.WriteLine(joke);
Console.WriteLine();

Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
Console.WriteLine("프로그램을 종료하려면 아무 키나 누르세요...");
Console.ReadKey();

