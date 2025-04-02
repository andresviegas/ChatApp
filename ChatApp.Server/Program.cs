using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://0.0.0.0:5029"); // Escuta em qualquer IP da máquina

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});
builder.Services.AddSignalR();

var app = builder.Build();
app.UseCors();
app.UseRouting();
app.MapHub<ChatHub>("/chatHub");

app.Run();

// Para publicar:
// dotnet publish -c Release -r win-x64 --self-contained
