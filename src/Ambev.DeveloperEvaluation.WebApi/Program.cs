using Ambev.DeveloperEvaluation.WebApi.Seed;
using Ambev.DeveloperEvaluation.Application.Commands.CreateSale;
using Ambev.DeveloperEvaluation.Application.Common;
using Ambev.DeveloperEvaluation.Application.EventHandlers;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.ORM;
using Ambev.DeveloperEvaluation.ORM.Repositories;
using Ambev.DeveloperEvaluation.WebApi.Middleware;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Ambev.DeveloperEvaluation.IoC;

var builder = WebApplication.CreateBuilder(args);

builder.RegisterDependencies();


var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

//What is this for?
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<DefaultContext>();
    db.Database.Migrate();
}

if (!app.Environment.IsProduction())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<DefaultContext>();
    await SaleDataSeeder.SeedAsync(db);
}

app.UseHttpsRedirection();

app.MapControllers();

await app.RunAsync();

public partial class Program { }