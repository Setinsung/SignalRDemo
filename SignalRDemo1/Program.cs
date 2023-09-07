using SignalRDemo1;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(opt =>
{
    opt.AddDefaultPolicy(b => b.WithOrigins("http://localhost:5173")
        .AllowAnyMethod().AllowAnyHeader().AllowCredentials()
    );
});

//builder.Services.AddSignalR();
builder.Services.AddSignalR().AddStackExchangeRedis("localhost", opt =>
{
    opt.Configuration.ChannelPrefix = "SignalRTest1_";
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapHub<ChatRoomHub>("/Hubs/ChatRoomHub"); // 路径不和现有路径冲突即可
app.MapControllers();

app.Run();
