//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection.Metadata;
//using System.Text;
//using System.Threading.Tasks;

//namespace ChatApp.ConsoleClient
//{
//    class Consola
//    {
//        static void Main(string[] args)
//        {
//            Console.WriteLine("Identifique-se...");
//            String user = Console.ReadLine();
//            Console.WriteLine("Olá, " + user + ". Pode escrever as suas mensagens para o chat. Escreva sair e faça enter para sair.");

//            while (user != "")
//            {

//                String mensagem = Console.ReadLine();
//                Console.WriteLine(user + ": " + mensagem);

//                if (mensagem == "sair")
//                {
//                    Console.WriteLine("A sair do chat...");
//                    break;
//                }
//            }

//            Console.ReadKey();
//        }
//    }
//}