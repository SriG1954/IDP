// See https://aka.ms/new-console-template for more information
using AppCore.Helper;

bool yesno = true;
Console.WriteLine("Enter text to encrypt");

while (yesno)
{
    string password = Console.ReadLine()!;
    if(string.IsNullOrWhiteSpace(password) )
    {
        yesno = false;
    }
    Console.WriteLine(AESSecurity.Encrypt(password));
}

