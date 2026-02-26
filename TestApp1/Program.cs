// See https://aka.ms/new-console-template for more information
using AppCore.Helper;
using AppCore.Services;

List<string> documentids = new List<string>();
//documentids.Add("639066");
documentids.Add("7005435232");
documentids.Add("7005434845");
documentids.Add("7005434743");

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

