// See https://aka.ms/new-console-template for more information
using AppCore.Helper;
using AppCore.Services;

List<string> documentids = new List<string>();
documentids.Add("639066");

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

