using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

public class ChatHub : Hub
{
    private static Dictionary<string, string> _users = new Dictionary<string, string>(); // ConnectionId -> Nome

    //acionada quando um cliente se conecta
    public override async Task OnConnectedAsync()
    {
        string timestamp = DateTime.Now.ToString("HH:mm:ss");
        Console.WriteLine($"[{timestamp}] Cliente conectado: {Context.ConnectionId}");
        await base.OnConnectedAsync();
    }

    //acionada quando um cliente se desconecta
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        string timestamp = DateTime.Now.ToString("HH:mm:ss");

        if (_users.TryGetValue(Context.ConnectionId, out string? userName))
        {

            Console.WriteLine($"[{timestamp}] Cliente desconectado: {userName} ({Context.ConnectionId})");
            _users.Remove(Context.ConnectionId);
        }
        else
        {
            Console.WriteLine($"[{timestamp}] Cliente desconhecido desconectado: {Context.ConnectionId}");
        }

        await Clients.Others.SendAsync("UserDisconnected", userName, timestamp);

        await base.OnDisconnectedAsync(exception);
    }

    //acionada quando um cliente se regista (ou seja, quando ele abre o chat e se identifica)
    public async Task RegisterUser(string user)
    {
        string timestamp = DateTime.Now.ToString("HH:mm:ss");

        if (!_users.ContainsKey(Context.ConnectionId))
        {
            _users[Context.ConnectionId] = user;
            Console.WriteLine($"[{timestamp}] Novo utilizador identificado: {user} ({Context.ConnectionId})");

            // Envia ao novo utilizador a lista de quem já está no chat
            await Clients.Caller.SendAsync("ReceiveConnectedUsers", _users.Values, timestamp);

            // Informa todos os outros utilizadores sobre o novo utilizador
            await Clients.Others.SendAsync("UserConnected", user, timestamp);
        }
    }

    //acionada quando um cliente envia uma mensagem (e o servidor a recebe, claro)
    public async Task SendMessage(string user, string message)
    {
        string timestamp = DateTime.Now.ToString("HH:mm:ss");

        Console.WriteLine($"[{timestamp}] Mensagem recebida de {user}: {message}");

        if (message == "\\help")
                {
            await Clients.Caller.SendAsync("ReceiveMessage", "ChatBot", "Comandos disponíveis: \n\\help - Mostra esta mensagem de ajuda\n\\list - Mostra a lista de utilizadores conectados\n\\sair - Sai do chat", timestamp);
            return;
        }

        if (message == "\\list")
        {
            await Clients.Caller.SendAsync("ReceiveConnectedUsers", _users.Values, timestamp);
            return;
        }

        //envia a mensagem de volta para todos os clientes
        await Clients.All.SendAsync("ReceiveMessage", user, message, timestamp);
    }
}

    