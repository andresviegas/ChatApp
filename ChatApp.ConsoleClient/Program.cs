using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;

class Program
{ 
    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetConsoleWindow();

    static async Task Main()
    {
        var originalColor = Console.ForegroundColor;

        //Console.InputEncoding = Encoding.UTF8;
        //Console.OutputEncoding = Encoding.UTF8; // Para reconhecer caracteres com acentos. Não funciona, não reconhece acentos em vogais.

        // Lê a configuração do arquivo
        var config = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json")
            .Build();

        var serverUrl = config["ChatServer:Url"];
        string user = config["User:Name"];
        string userRole = config["User:Role"];


        //Esta secção era para identificar o utilizador
        //Agora o utilizador já entra identificado, conforme o ficheiro appsettings.json
        //Console.WriteLine("Identifique-se...");
        //while (string.IsNullOrWhiteSpace(user))
        //{
        //    Console.WriteLine("Nome inválido. Por favor insira um nome válido");
        //    return;
        //}

        Console.WriteLine($"Olá, {user}! Podes escrever as tuas mensagens para o chat. Escreve '\\help' para consultares os comandos que podes executar. ");

        var connection = new HubConnectionBuilder()
            .WithUrl(serverUrl)
            .WithAutomaticReconnect()
            .Build();

        // 🔹 Quando receber a lista de utilizadores já conectados
        connection.On<string[], string>("ReceiveConnectedUsers", (users, timestamp) =>
        {
            Console.WriteLine($"[{timestamp}] Utilizadores conectados neste momento: " + (users.Length > 1 ? string.Join(", ", users) : "és só tu. Espera que mais pessoas se liguem."));
        });

        // 🔹 Quando um novo utilizador entra
        connection.On<string, string>("UserConnected", (newUser, timestamp) =>
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"{newUser} entrou no chat.");
            Console.ForegroundColor = originalColor;
        });

        connection.On<string[]>("ReceiveChatHistory", (history) =>
        {
            Console.WriteLine("Histórico de mensagens:");
            foreach (var message in history)
            {
                Console.WriteLine(message);
            }
        });

        // 🔹 Quando um utilizador sai. Não existe código no servidor para acionar isto ainda
        connection.On<string, string>("UserDisconnected", (disconnectedUser, timestamp) =>
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[{timestamp}] {disconnectedUser} saiu do chat.");
        });

        //    Quando alguém (pode ser o próprio) envia uma mensagem
        connection.On<string, string, string>("ReceiveMessage", (sender, message, timestamp) =>
        {

            // 🔹 Traz a janela para frente ao receber mensagem
            IntPtr handle = GetConsoleWindow();
            SetForegroundWindow(handle);

            Console.Write($"[{timestamp}] ");

            if (sender == user)
            {
                Console.ForegroundColor = ConsoleColor.Green;  // Cor para o próprio utilizador
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Blue;   // Cor para os outros utilizadores
            }

            Console.Write($"{sender}: ");
            Console.ForegroundColor = originalColor;
            Console.WriteLine($"{message}");
        });

        await connection.StartAsync();

        await connection.InvokeAsync("RegisterUser", user);

        Console.WriteLine("Conectado ao chat! Escreve mensagens para enviar:");

        await connection.InvokeAsync("GetChatHistory");

        while (true)
        {
            var message = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(message)) continue;


            if (message.ToLower() == "\\sair")
            {
                Console.WriteLine("A sair do chat...");
                break;
            }

            await connection.InvokeAsync("SendMessage", user, message);
        }
    }
}



