using FluentValidation;
using Homework2.Api.Contracts;
using Homework2.Api.Services;
using Homework2.Api.Validators;
using Homework2.Application.Interfaces;
using Homework2.Application.Services;
using Homework2.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();
builder.Services.AddControllers();
builder.Services.AddScoped<IValidator<CreateProductRequest>, CreateProductRequestValidator>();
builder.Services.AddScoped<IValidator<GetProductListRequest>, GetProductListRequestValidator>();
builder.Services.AddScoped<IValidator<Homework2.Products.CreateProductRequest>, GrpcCreateProductRequestValidator>();
builder.Services.AddScoped<IValidator<Homework2.Products.GetProductRequest>, GrpcGetProductRequestValidator>();
builder.Services.AddScoped<IValidator<Homework2.Products.GetProductsListRequest>, GrpcGetProductsListRequestValidator>();


builder.Services.AddSingleton<IProductRepository, ProductRepository>();
builder.Services.AddSingleton<IProductService, ProductService>();

var app = builder.Build();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(context => Results.Problem(
        statusCode: StatusCodes.Status500InternalServerError,
        title: "Internal server error."
    ).ExecuteAsync(context));
});

app.MapGrpcService<ProductGrpcService>();
app.MapControllers();
app.MapGet("/", () => "Hello World!");

app.Run();
