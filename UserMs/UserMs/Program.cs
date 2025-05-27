


using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using UserMs;
using UserMs.Application.Handlers.ActivityHistory.NewFolder;
using UserMs.Application.Handlers.Auctioneer.Command;
using UserMs.Application.Handlers.Auctioneer.Queries;
using UserMs.Application.Handlers.Bidder.Command;
using UserMs.Application.Handlers.Bidder.Queries;
using UserMs.Application.Handlers.Roles_Permission.Commands;
using UserMs.Application.Handlers.Roles_Permission.Queries;
using UserMs.Application.Handlers.Roles.Queries;
using UserMs.Application.Handlers.Support.Command;
using UserMs.Application.Handlers.Support.Queries;
using UserMs.Application.Handlers.User_Roles.Commands;
using UserMs.Application.Handlers.User_Roles.Queries___Copia;
using UserMs.Application.Handlers.User_Roles.Queries;
using UserMs.Application.Handlers.User.Commands;
using UserMs.Application.Handlers.User.Queries;
using UserMs.Commoon.AutoMapper.Bidder;
using UserMs.Commoon.AutoMapper.NewFolder1;
using UserMs.Commoon.AutoMapper.Permission;
using UserMs.Commoon.AutoMapper.Role_Permission;
using UserMs.Commoon.AutoMapper.Role;
using UserMs.Commoon.AutoMapper.Support;
using UserMs.Commoon.AutoMapper.User;
using UserMs.Commoon.AutoMapper.UserRole;
using UserMs.Commoon.Dtos;
using UserMs.Commoon.Dtos.Users.Request.User;
using UserMs.Commoon.Dtos.Users.Response.Support;
using UserMs.Commoon.Dtos.Users.Response.User;
using UserMs.Core;
using UserMs.Core.Database;
using UserMs.Core.Interface;
using UserMs.Core.RabbitMQ;
using UserMs.Core.Repositories.Auctioneer;
using UserMs.Core.Repositories.Biddere;
using UserMs.Core.Repositories.RolesRepo;
using UserMs.Infrastructure;
using UserMs.Infrastructure.Database;
using UserMs.Infrastructure.Database.Context;
using UserMs.Infrastructure.Database.Context.Mongo;
using UserMs.Infrastructure.Database.Context.Postgress;
using UserMs.Infrastructure.JsonConverter.IUser;
using UserMs.Infrastructure.RabbitMQ;
using UserMs.Infrastructure.RabbitMQ.Connection;
using UserMs.Infrastructure.RabbitMQ.Consumer;
using UserMs.Core.Repositories.UserRepo;
using UserMs.Infrastructure.Repositories;
using UserMs.Infrastructure.Repositories.PermissionsRepo;
using UserMs.Infrastructure.Repositories.Roles_Permission;
using UserMs.Infrastructure.Repositories.RolesRepo;
using UserMs.Core.Repositories.RolePermissionRepo;
using UserMs.Core.Repositories.SupportsRepo;
using UserMs.Core.Repositories.UserRoleRepo;
using UserMs.Domain.Entities.Auctioneer;
using UserMs.Domain.Entities.Bidder;
using UserMs.Domain.Entities.Role_Permission;
using UserMs.Domain.Entities.Role;
using UserMs.Domain.Entities.UserEntity;
using UserMs.Domain.User_Roles;
using UserMs.Infrastructure.Repositories.User_Roles;
using UserMs.Domain.Entities.Permission;
using UserMs.Domain.Entities.Support;
using UserMs.Infrastructure.Repositories.Bidder;
using UserMs.Infrastructure.Repositories.Auctioneer;
using UserMs.Infrastructure.Repositories.Support;
using UserMs.Commoon.Dtos.Users.Request.Support;
using UserMs.Commoon.Dtos.Users.Response.Bidder;
using UserMs.Commoon.Dtos.Users.Request.Bidder;
using UserMs.Commoon.Dtos.Users.Response.Auctioneer;
using UserMs.Commoon.Dtos.Users.Request.Auctioneer;
using UserMs.Domain.Entities;
using UserMs.Domain.Entities.ActivityHistory;
using UserMs.Application.Handlers.ActivityHistory.NewFolder1;
using UserMs.Commoon.Dtos.Users.Response.ActivityHistory;
using UserMs.Core.Repositories.ActivityHistory;
using UserMs.Core.Repositories.ActivityHistoryRepo;
using UserMs.Core.Repositories.Bidders;
using UserMs.Core.Repositories.PermissionRepo;
using UserMs.Core.Repositories.Supports;
using UserMs.Infrastructure.Repositories.ActivityHistoryRepo;
using UserMs.Infrastructure.Service.Keycloak;
using UserMs.Core.Service.Keycloak;
using UserMs.Infrastructure.Repositories.ActivityHistory;
using UserMs.Infrastructure.Repositories.Bidders;
using UserMs.Infrastructure.Repositories.Supports;
using UserMs.Infrastructure.Repositories.Users;
using RabbitMQ.Client;



var builder = WebApplication.CreateBuilder(args);

// Agregar servicios


// Registrar el serializador de GUID
BsonSerializer.RegisterSerializer(typeof(Guid), new GuidSerializer(GuidRepresentation.Standard));
// Registro de los perfiles de AutoMapper
var profileTypes = new[]
{
    typeof(UserProfile),
    typeof(RoleProfile),
    typeof(PermissionProfile),
    typeof(RolePermissionProfile),
    typeof(UserRolesProfile),
    typeof(SupportProfile),
    typeof(BidderProfile),
    typeof(AuctioneerProfile)

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
    typeof(DeleteUsersCommandHandler).Assembly,

    typeof(CreateHistoryActivityCommandHandler).Assembly,
    typeof(GetActivitiesByUserQueryHandler).Assembly,

    typeof(GetRolesAllQueryHandler).Assembly,
    typeof(GetRolesByIdQueryHandler).Assembly,
    typeof(GetRolesByNameQueryHandler).Assembly,

    typeof(CreateRolePermissionCommandHandler).Assembly,
    typeof(DeleteRolePermissionCommandHandler).Assembly,
    //typeof(UpdateRolePermissionCommandHandler).Assembly,
    typeof(GetRolesPermissionByIdQueryHandler).Assembly,
    typeof(GetRolesPermissionByRoleQueryHandler).Assembly,
    typeof(GetRolesPermissionsAllQueryHandler).Assembly,

    typeof(CreateUserRoleCommandHandler).Assembly,
    typeof(DeleteUserRoleCommandHandler).Assembly,
   // typeof(UpdateUserRoleCommandHandler).Assembly,
    typeof(GetUserRolesByIdQueryHandler).Assembly,
    typeof(GetUserRolesByUserEmailQueryHandler).Assembly,
    typeof(GetUserRolesByRoleNameQueryHandler).Assembly,
    typeof(GetUsersRolesQueryHandler).Assembly,

    typeof(GetSupportByIdQueryHandler).Assembly,
    typeof(GetSupportByUserEmailQueryHandler).Assembly,
    typeof(GetSupportAllQueryHandler).Assembly,
    typeof(CreateSupportCommandHandler).Assembly,
    typeof(UpdateSupportCommandHandler).Assembly,
    typeof(DeleteSupportCommandHandler).Assembly,

    typeof(GetAuctioneerByIdQueryHandler).Assembly,
    typeof(GetAuctioneerByUserEmailQueryHandler).Assembly,
    typeof(GetAuctioneerAllQueryHandler).Assembly,
    typeof(CreateAuctioneerCommandHandler).Assembly,
    typeof(UpdateAuctioneerCommandHandler).Assembly,
    typeof(DeleteAuctioneerCommandHandler).Assembly,

    typeof(GetBidderByIdQueryHandler).Assembly,
    typeof(GetBidderByUserEmailQueryHandler).Assembly,
    typeof(GetBidderAllQueryHandler).Assembly,
    typeof(CreateBidderCommandHandler).Assembly,
    typeof(UpdateBidderCommandHandler).Assembly,
    typeof(DeleteBidderCommandHandler).Assembly




));
//**********************************************************************

builder.Services.AddScoped(sp =>
{
    var dbContext = sp.GetRequiredService<IUserDbContextMongo>();
    return dbContext.Database.GetCollection<Users>("Users"); // Nombre de la colección en MongoDB
});

builder.Services.AddScoped(sp =>
{
    var dbContext = sp.GetRequiredService<IUserDbContextMongo>();
    return dbContext.Database.GetCollection<Roles>("Roles"); // Nombre de la colección en MongoDB
});

builder.Services.AddScoped(sp =>
{
    var dbContext = sp.GetRequiredService<IUserDbContextMongo>();
    return dbContext.Database.GetCollection<RolePermissions>("RolePermissions"); // Nombre de la colección en MongoDB
});

builder.Services.AddScoped(sp =>
{
    var dbContext = sp.GetRequiredService<IUserDbContextMongo>();
    return dbContext.Database.GetCollection<UserRoles>("UserRoles"); // Nombre de la colección en MongoDB
});

builder.Services.AddScoped(sp =>
{
    var dbContext = sp.GetRequiredService<IUserDbContextMongo>();
    return dbContext.Database.GetCollection<Permissions>("Permissions"); // Nombre de la colección en MongoDB
});

builder.Services.AddScoped(sp =>
{
    var dbContext = sp.GetRequiredService<IUserDbContextMongo>();
    return dbContext.Database.GetCollection<Supports>("Supports"); // Nombre de la colección en MongoDB
});

builder.Services.AddScoped(sp =>
{
    var dbContext = sp.GetRequiredService<IUserDbContextMongo>();
    return dbContext.Database.GetCollection<Bidders>("Bidders"); // Nombre de la colección en MongoDB
});


builder.Services.AddScoped(sp =>
{
    var dbContext = sp.GetRequiredService<IUserDbContextMongo>();
    return dbContext.Database.GetCollection<ActivityHistory>("ActivityHistories"); // Nombre de la colección en MongoDB
});

builder.Services.AddScoped(sp =>
{
    var dbContext = sp.GetRequiredService<IUserDbContextMongo>();
    return dbContext.Database.GetCollection<Auctioneers>("Auctioneers"); // Nombre de la colección en MongoDB
});

//***********************Configuracion de Repositorios*********************
builder.Services.AddTransient<IUserDbContext, UserDbContext>();
builder.Services.AddTransient<IUserRepository, UsersRepository>();
builder.Services.AddTransient<IUserRepositoryMongo, UsersRepositoryMongo>();
builder.Services.AddTransient<IAuthMsService, AuthMsService>();
builder.Services.AddTransient<IKeycloakService, KeycloakService>();
builder.Services.AddTransient<IRolesRepository, RoleRepository>();
builder.Services.AddTransient<IPermissionRepositoryMongo, PermissionRepository>();
builder.Services.AddTransient<IRolePermissionRepository, RolePermissionRepository>();
builder.Services.AddTransient<IRolePermissionRepositoryMongo, RolePermissionRepositoryMongo>();
builder.Services.AddTransient<IUserRoleRepository, UserRolesRepository>();
builder.Services.AddTransient<IUserRoleRepositoryMongo, UserRolesRepositoryMongo>();
builder.Services.AddTransient<IBidderRepository, BidderRepository>();
builder.Services.AddTransient<IBidderRepositoryMongo, BidderRepositoryMongo>();
builder.Services.AddTransient<ISupportRepository, SupportRepository>();
builder.Services.AddTransient<ISupportRepositoryMongo, SupportRepositoryMongo>();
builder.Services.AddTransient<IAuctioneerRepository, AuctioneerRepository>();
builder.Services.AddTransient<IAuctioneerRepositoryMongo, AuctioneerRepositoryMongo>();
builder.Services.AddTransient<IActivityHistoryRepository, ActivityHistoryRepository>();
builder.Services.AddTransient<IActivityHistoryRepositoryMongo, ActivityHistoryRepositoryMongo>();
builder.Services.AddTransient<IEmailSender, EmailSender>();


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
builder.Services.AddSingleton<IRabbitMQConsumer, RabbitMQConsumer>();
builder.Services.AddSingleton<IConnectionRabbbitMQ, RabbitMQConnection>();
builder.Services.AddSingleton(typeof(IEventBus<>), typeof(RabbitMQProducer<>));
builder.Services.AddSingleton<IConnectionFactory>(provider =>
{
    return new ConnectionFactory
    {
        HostName = "localhost",
        Port = 5672,
        UserName = "guest",
        Password = "guest",
    };
});

builder.Services.AddSingleton<IConnectionRabbbitMQ>(provider =>
{
    var connectionFactory = provider.GetRequiredService<IConnectionFactory>();
    var rabbitMQConnection = new RabbitMQConnection(connectionFactory);
    rabbitMQConnection.InitializeAsync().Wait(); // ?? Ejecutar inicialización antes de inyectarlo
    return rabbitMQConnection;
}); builder.Services.AddSingleton<IRabbitMQConsumer, RabbitMQConsumer>();
// Usa la misma instancia de `RabbitMQConnection` para el Producer


builder.Services.AddSingleton<IEventBus<CreateUserDto>>(provider =>
{
    var rabbitMQConnection = provider.GetRequiredService<RabbitMQConnection>();
    return new RabbitMQProducer<CreateUserDto>(rabbitMQConnection);
});

builder.Services.AddSingleton<IEventBus<UpdateUserDto>>(provider =>
{
    var rabbitMQConnection = provider.GetRequiredService<RabbitMQConnection>();
    return new RabbitMQProducer<UpdateUserDto>(rabbitMQConnection);
});


builder.Services.AddSingleton<IEventBus<GetUsersDto>>(provider =>
{
    var rabbitMQConnection = provider.GetRequiredService<RabbitMQConnection>();
    return new RabbitMQProducer<GetUsersDto>(rabbitMQConnection);
});

builder.Services.AddSingleton<IEventBus<GetActivityHistoryDto>>(provider =>
{
    var rabbitMQConnection = provider.GetRequiredService<RabbitMQConnection>();
    return new RabbitMQProducer<GetActivityHistoryDto>(rabbitMQConnection);
});





builder.Services.AddSingleton<IEventBus<CreateAuctioneerDto>>(provider =>
{
    var rabbitMQConnection = provider.GetRequiredService<RabbitMQConnection>();
    return new RabbitMQProducer<CreateAuctioneerDto>(rabbitMQConnection);
});

builder.Services.AddSingleton<IEventBus<UpdateAuctioneerDto>>(provider =>
{
    var rabbitMQConnection = provider.GetRequiredService<RabbitMQConnection>();
    return new RabbitMQProducer<UpdateAuctioneerDto>(rabbitMQConnection);
});


builder.Services.AddSingleton<IEventBus<GetAuctioneerDto>>(provider =>
{
    var rabbitMQConnection = provider.GetRequiredService<RabbitMQConnection>();
    return new RabbitMQProducer<GetAuctioneerDto>(rabbitMQConnection);
});

builder.Services.AddSingleton<IEventBus<CreateBidderDto>>(provider =>
{
    var rabbitMQConnection = provider.GetRequiredService<RabbitMQConnection>();
    return new RabbitMQProducer<CreateBidderDto>(rabbitMQConnection);
});

builder.Services.AddSingleton<IEventBus<UpdateBidderDto>>(provider =>
{
    var rabbitMQConnection = provider.GetRequiredService<RabbitMQConnection>();
    return new RabbitMQProducer<UpdateBidderDto>(rabbitMQConnection);
});


builder.Services.AddSingleton<IEventBus<GetBidderDto>>(provider =>
{
    var rabbitMQConnection = provider.GetRequiredService<RabbitMQConnection>();
    return new RabbitMQProducer<GetBidderDto>(rabbitMQConnection);
});

builder.Services.AddSingleton<IEventBus<CreateSupportDto>>(provider =>
{
    var rabbitMQConnection = provider.GetRequiredService<RabbitMQConnection>();
    return new RabbitMQProducer<CreateSupportDto>(rabbitMQConnection);
});

builder.Services.AddSingleton<IEventBus<UpdateSupportDto>>(provider =>
{
    var rabbitMQConnection = provider.GetRequiredService<RabbitMQConnection>();
    return new RabbitMQProducer<UpdateSupportDto>(rabbitMQConnection);
});


builder.Services.AddSingleton<IEventBus<GetSupportDto>>(provider =>
{
    var rabbitMQConnection = provider.GetRequiredService<RabbitMQConnection>();
    return new RabbitMQProducer<GetSupportDto>(rabbitMQConnection);
});
//  Usa la misma instancia de `RabbitMQConnection` para el Consumer
builder.Services.AddSingleton<RabbitMQConsumer>(provider =>
{
    var rabbitMQConnection = provider.GetRequiredService<IConnectionRabbbitMQ>();
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
//builder.Services.KeycloakConfiguration(builder.Configuration);

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