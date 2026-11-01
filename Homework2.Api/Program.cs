using Homework2.Api.Services;
using Homework2.Application.Interfaces;
using Homework2.Application.Services;
using Homework2.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();

builder.Services.AddSingleton<IProductRepository, ProductRepository>();
builder.Services.AddSingleton<IProductService, ProductService>();

var app = builder.Build();

app.MapGrpcService<ProductGrpcService>();
app.MapGet("/", () => "Hello World!");

app.Run();
