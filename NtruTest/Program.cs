// See https://aka.ms/new-console-template for more information
using NTRUEncrypt;
using System.Text;


byte[] content = Encoding.ASCII.GetBytes("abcdefghijklmnopqrst");
Key key = new Key();

byte[] encrypted = NTRUEncyptService.Encrypt(key.PublicKey(), content);
byte[] decrypted = NTRUEncyptService.Decrypt(key, encrypted);



Console.WriteLine(Encoding.ASCII.GetString(decrypted));

