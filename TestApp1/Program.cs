// See https://aka.ms/new-console-template for more information
using AppCore.Helper;
using AppCore.Services;

List<string> documentids = new List<string>();
//documentids.Add("639066");

documentids.Add("8077027626");
documentids.Add("7005143316");
documentids.Add("7005038331");
documentids.Add("7005152519");
documentids.Add("7005153245");
documentids.Add("7005153085");
documentids.Add("7005152384");
documentids.Add("7005153131");
documentids.Add("7005171877");
documentids.Add("7005171588");
documentids.Add("7005153547");
documentids.Add("7005153547");
documentids.Add("7005155721");
documentids.Add("7005160589");
documentids.Add("7005171816");
documentids.Add("7005152924");
documentids.Add("7005152934");
documentids.Add("7005154892");
documentids.Add("7005169968");
documentids.Add("7005166076");
documentids.Add("7005167428");
documentids.Add("7005155173");
documentids.Add("7005153137");
documentids.Add("7005166176");
documentids.Add("7005132878");
documentids.Add("7005178622");
documentids.Add("7005170420");
documentids.Add("7005159420");
documentids.Add("7005155785");
documentids.Add("7005166169");
documentids.Add("7005163219");
documentids.Add("7005186704");
documentids.Add("7005153286");
documentids.Add("7005161311");
documentids.Add("7005165759");
documentids.Add("7005177533");
documentids.Add("7005186170");
documentids.Add("7005160448");
documentids.Add("7005185732");
documentids.Add("7005183370");
documentids.Add("7005179428");
documentids.Add("7005186753");
documentids.Add("7005185764");
documentids.Add("7005159739");
documentids.Add("7005176651");
documentids.Add("7005175540");
documentids.Add("7005177200");
documentids.Add("7005163081");
documentids.Add("7005174993");
documentids.Add("7005153921");
documentids.Add("7005165747");
documentids.Add("7005168554");
documentids.Add("7005152463");
documentids.Add("7005171608");
documentids.Add("7005176492");
documentids.Add("7005194924");
documentids.Add("7005173280");
documentids.Add("7005193724");
documentids.Add("7005187745");
documentids.Add("7005186150");
documentids.Add("7005171634");
documentids.Add("7005171890");
documentids.Add("7005174800");
documentids.Add("7005182352");
documentids.Add("7005185675");
documentids.Add("7005195032");

//documentids.Add("63906697995952146186");
//documentids.Add("63906697996089476011");
//documentids.Add("63906697996226254970");
//documentids.Add("63906697996357900694");
//documentids.Add("63906697996484185715");
//documentids.Add("63906697996612714037");
//documentids.Add("63906697996749308626");
//documentids.Add("63906697996879398943");
//documentids.Add("63906697997011396450");
//documentids.Add("63906697997238053891");
//documentids.Add("63906697997408820561");
//documentids.Add("63906697997958603722");
//documentids.Add("63906697998098676961");
//documentids.Add("63906697998363503388");
//documentids.Add("63906697998491961187");
//documentids.Add("63906697998750557785");
//documentids.Add("63906697998877231176");
//documentids.Add("63906697999006102131");
//documentids.Add("63906697999134238536");

ArsAutomationAgent agent = new ArsAutomationAgent();
await agent.ExecuteAsync(documentids);

//bool yesno = true;
//Console.WriteLine("Enter text to encrypt");

//while (yesno)
//{
//    string password = Console.ReadLine()!;
//    if(string.IsNullOrWhiteSpace(password) )
//    {
//        yesno = false;
//    }
//    Console.WriteLine(AESSecurity.Encrypt(password));
//}

