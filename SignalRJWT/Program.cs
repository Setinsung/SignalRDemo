using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SignalRJWT;
using SignalRJWT.Hubs;
using SignalRJWT.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    var scheme = new OpenApiSecurityScheme()
    {
        Description = "Authorization header. \r\nExample: 'Bearer 12345abcdef'",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "Authorization"
        },
        Scheme = "oauth2",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
    };
    c.AddSecurityDefinition("Authorization", scheme);

    var requirement = new OpenApiSecurityRequirement();
    requirement[scheme] = new List<string>();
    c.AddSecurityRequirement(requirement);
});

builder.Services.AddCors(opt =>
{
    opt.AddDefaultPolicy(b => b.WithOrigins("http://localhost:5173")
        .AllowAnyMethod().AllowAnyHeader().AllowCredentials()
    );
});

builder.Services.AddMemoryCache();

builder.Services.AddDbContext<MyDbContext>(opt => {
    opt.UseSqlServer(builder.Configuration.GetConnectionString("Default"));
});
builder.Services.AddDataProtection();
builder.Services.AddIdentityCore<MyUser>(opt =>
{
    opt.Password.RequireDigit = false;
    opt.Password.RequireLowercase = false;
    opt.Password.RequireNonAlphanumeric = false;
    opt.Password.RequireUppercase = false;
    opt.Password.RequiredLength = 6;
    opt.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultEmailProvider;
    opt.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultEmailProvider;
});
new IdentityBuilder(typeof(MyUser), typeof(MyRole), builder.Services).AddEntityFrameworkStores<MyDbContext>()
    .AddDefaultTokenProviders()
    .AddRoleManager<RoleManager<MyRole>>()
    .AddUserManager<UserManager<MyUser>>();

builder.Services.AddOptions()
    .Configure<JwtSettings>(builder.Configuration.GetSection("JWT"));
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(opt =>
{
    opt.TokenValidationParameters = new()
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:SecKey"])),
    };
    // Ìí¼Ó¶îÍâµÄÅäÖÃ
    opt.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            StringValues accessToken = context.Request.Query["access_token"];
            PathString path = context.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/Hubs"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddSignalR().AddStackExchangeRedis("localhost", opt =>
{
    opt.Configuration.ChannelPrefix = "SignalRTest1_";
});


builder.Services.Configure<ConnStrOptions>(
    builder.Configuration.GetSection("ConnectionStrings"));
builder.Services.AddTransient<ImportExecutor>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapHub<ChatRoomHub>("/Hubs/ChatRoomHub");
app.MapControllers();

app.Run();
