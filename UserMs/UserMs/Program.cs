

using AuthMs.Infrastructure;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using UserMs;
using UserMs.Application.Handlers.User.Commands;
using UserMs.Application.Handlers.User.Queries;
using UserMs.Commoon.AutoMapper;
using UserMs.Commoon.Dtos.Users.Request;
using UserMs.Core;
using UserMs.Core.Database;
using UserMs.Core.Interface;
using UserMs.Core.RabbitMQ;
using UserMs.Core.Repositories;
using UserMs.Infrastructure;
using UserMs.Infrastructure.Database;
using UserMs.Infrastructure.Database.Factory.Postgres;
using UserMs.Infrastructure.JsonConverter.IUser;
using UserMs.Infrastructure.RabbitMQ;
using UserMs.Infrastructure.RabbitMQ.Connection;
using UserMs.Infrastructure.RabbitMQ.Consumer;
using UserMs.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Agregar servicios
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Registrar el serializador de GUID
BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

// Registro de los perfiles de AutoMapper
var profileTypes = new[]
{
    typeof(UserProfile)
};

foreach (var profileType in profileTypes)
{
    builder.Services.AddAutoMapper(profileType);
}
// Add services to the container.
builder.Services.AddControllers();

//*************************Configuracion de MongoDB*********************
builder.Services.AddSingleton<IUserDbContextMongo>(sp =>
{
    const string connectionString = "mongodb+srv://yadefreitas19:08092001@cluster0.owy2d.mongodb.net/?retryWrites=true&w=majority&appName=Cluster0";
    var databaseName = "UserMs";
    return new UserDbContextMongo(connectionString, databaseName);
});
//***********************************************************************

//******************Configuracion de Swagger*********************
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGenWithAuth(builder.Configuration);
builder.Services.KeycloakConfiguration(builder.Configuration);

builder.Services.Configure<HttpClientUrl>(
    builder.Configuration.GetSection("HttpClientAddress"));

//***********************Configuracion de MediatR*********************
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(
    typeof(CreateUsersCommandHandler).Assembly,
    typeof(GetUsersQueryHandler).Assembly,
    typeof(GetUsersByIdQueryHandler).Assembly,
    typeof(UpdateUsersCommandHandler).Assembly,
    typeof(DeleteUsersCommandHandler).Assembly
));
//**********************************************************************

//***********************Configuracion de Repositorios*********************
builder.Services.AddTransient<IUserDbContext, UserDbContext>();
builder.Services.AddTransient<IUserRepository, UsersRepository>();
builder.Services.AddTransient<IAuthMsService, AuthMsService>();
builder.Services.AddTransient<IKeycloakRepository, KeycloakRepository>();
builder.Services.AddScoped<IKeycloakRepository, KeycloakRepository>();

//***********************************************************************
builder.Services.AddControllers().AddJsonOptions(options =>
{

    options.JsonSerializerOptions.Converters.Add(new UserIdJsonConverter());
    options.JsonSerializerOptions.Converters.Add(new UserEmailJsonConverter());
    options.JsonSerializerOptions.Converters.Add(new UserPasswordJsonConverter());
    options.JsonSerializerOptions.Converters.Add(new UserAddressJsonConverter());
    options.JsonSerializerOptions.Converters.Add(new UserNameJsonConverter());
    options.JsonSerializerOptions.Converters.Add(new UserPhoneJsonConverter());

});


// *********************Configuracion de RabbitMQ*********************

// Configura la dependencia en tu contenedor 
builder.Services.AddSingleton<RabbitMQConnection>(provider =>
{
    var rabbitMQConnection = new RabbitMQConnection();
    rabbitMQConnection.InitializeAsync().Wait(); //  Inicialización segura
    return rabbitMQConnection;
});

// Usa la misma instancia de `RabbitMQConnection` para el Producer
builder.Services.AddSingleton<IEventBus<CreateUsersDto>>(provider =>
{
    var rabbitMQConnection = provider.GetRequiredService<RabbitMQConnection>();
    return new RabbitMQProducer<CreateUsersDto>(rabbitMQConnection);
});

//  Usa la misma instancia de `RabbitMQConnection` para el Consumer
builder.Services.AddSingleton<RabbitMQConsumer>(provider =>
{
    var rabbitMQConnection = provider.GetRequiredService<RabbitMQConnection>();
    return new RabbitMQConsumer(rabbitMQConnection);
});

// Iniciar el consumidor automáticamente con `RabbitMQBackgroundService`
builder.Services.AddHostedService<RabbitMQBackgroundService>();

//***********************************************************************************

//*******************************Configuracion de Postgress ************************************

var BusinessConnectionString = Environment.GetEnvironmentVariable("DB_BUSINESS_CONNECTION_STRING") ??
    builder.Configuration.GetConnectionString("PostgresSQLConnection");

builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseNpgsql(BusinessConnectionString));

//**********************************************************************************    

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});
var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => "Connected!");

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();