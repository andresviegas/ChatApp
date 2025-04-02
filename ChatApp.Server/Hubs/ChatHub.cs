using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using static System.Runtime.InteropServices.JavaScript.JSType;

public class ChatHub : Hub
{
    private static Dictionary<string, string> _users = new Dictionary<string, string>(); // ConnectionId -> Nome

    private static string _dbPath = "chat.db";

    public ChatHub()
    {
        using var connection = new SqliteConnection($"Data Source={_dbPath}");
        connection.Open();
        var command = connection.CreateCommand();
        command.CommandText = "CREATE TABLE IF NOT EXISTS chat (Id INTEGER PRIMARY KEY, timestamp TEXT, user TEXT, message TEXT)";
        command.ExecuteNonQuery();
       
    }

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

    // Acionada logo após o registo, para mostrar o histórico de mensagens
    public async Task GetChatHistory()

    {  
        // Carregar histórico do dia
        using var connection = new SqliteConnection($"Data Source={_dbPath}");
        connection.Open();

        string date = DateTime.Now.ToString("yyyy-MM-dd");
        string query = "SELECT Timestamp, User, Message FROM chat WHERE Timestamp LIKE @Date";
        using var command = new SqliteCommand(query, connection);
        command.Parameters.AddWithValue("@Date", date + "%");

        using var reader = command.ExecuteReader();
        List<string> messages = new List<string>();
        while (reader.Read())
        {
            string timestamp = reader.GetString(0);
            string user = reader.GetString(1);
            string message = reader.GetString(2);
            messages.Add($"{timestamp} {user}: {message}");
        }

        await Clients.Caller.SendAsync("ReceiveChatHistory", messages);
        await base.OnConnectedAsync();

    }

    //acionada quando um cliente envia uma mensagem (e o servidor a recebe, claro)
    public async Task SendMessage(string user, string message)
    {
        string hora = DateTime.Now.ToString("HH:mm:ss");
        string data = DateTime.Now.ToString("yyyy-MM-dd");
        string timestamp = data + " " + hora;

        Console.WriteLine($"[{hora}] Mensagem recebida de {user}: {message}");

        using var connection = new SqliteConnection($"Data Source={_dbPath}");
        connection.Open();

        string insertCmd = "INSERT INTO chat (Timestamp, User, Message) VALUES (@timestamp, @User, @Message)";
        using var command = new SqliteCommand(insertCmd, connection);
        command.Parameters.AddWithValue("@timestamp", timestamp);
        command.Parameters.AddWithValue("@User", user);
        command.Parameters.AddWithValue("@Message", message);
        command.ExecuteNonQuery();

        if (message == "\\help")
                {
            await Clients.Caller.SendAsync("ReceiveMessage", "ChatBot", "Comandos disponíveis: \n\\help - Mostra esta mensagem de ajuda\n\\list - Mostra a lista de utilizadores conectados\n\\sair - Sai do chat", hora);
            return;
        }

        if (message == "\\list")
        {
            await Clients.Caller.SendAsync("ReceiveConnectedUsers", _users.Values, hora);
            return;
        }

        //envia a mensagem de volta para todos os clientes
        await Clients.All.SendAsync("ReceiveMessage", user, message, hora);
    }
}

    