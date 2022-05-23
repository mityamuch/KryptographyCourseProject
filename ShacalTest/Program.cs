// See https://aka.ms/new-console-template for more information
using SHACAL;

using System.Text;
Console.WriteLine("Hello, World!");
byte[] content = Encoding.ASCII.GetBytes("abcdefghijklmnopqrst");


Random rnd = new Random();
byte[] key = new byte[64];
byte[] vector = new byte[8];
rnd.NextBytes(key);
rnd.NextBytes(vector);
ShacalService shacal = new ShacalService(key);

CipherContext cipherContext = new CipherContext(shacal,20,CipherContext.Mode.ECB,vector);


var content1 = cipherContext.Encrypt(content);
var content2 = cipherContext.Decrypt(content1);



//var content1=shacal.Encrypt(content);
//var content2 = shacal.Decrypt(content1);

Console.WriteLine(Encoding.ASCII.GetString(content2));
