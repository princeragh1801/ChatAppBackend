using Microsoft.EntityFrameworkCore;
using ChatApp.Database;
using ChatApp.Interfaces;
using ChatApp.Services;
using ChatApp.Hubs;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ChatApp.ExceptionHandler;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("SqlServerConnectionString");
builder.Services.AddDbContext<BackendContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddControllers();

builder.Services.AddCors(c =>
{
    c.AddPolicy("CorsPolicy",
     options => options.WithOrigins(["http://localhost:3000"])
    .AllowAnyMethod()
    .AllowAnyHeader()
    .AllowCredentials());
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IUserService, UserService>(); 
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<IHubService, HubService>();

builder.Services.AddSwaggerGen(opt =>
{
    // Add Bearer token authorization
    opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });

    opt.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[]{}
        }
    });
});



builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("jkweoerjdsioaoitnksadiuhewiuejkjkdsoweoiwkt")),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

builder.Services.AddExceptionHandler<AppExceptionHandler>();
builder.Services.AddSignalR();
builder.Services.AddAuthorization();
builder.Services.AddMemoryCache();

var app = builder.Build();
app.UseCors("CorsPolicy");
app.UseExceptionHandler(_ => { });
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseWebSockets();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/chatHub");
app.Run();
