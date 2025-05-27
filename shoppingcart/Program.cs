using ShoppingBasket.CQRS.Commands;
using ShoppingBasket.CQRS.Queries;
using ShoppingBasket.Repository;
using ShoppingBasket.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IBasketRepository, SqlBasketRepository>();
builder.Services.AddScoped<IProductLockRepository, SqlProductLockRepository>();

builder.Services.AddScoped<BasketCommandHandler>();
builder.Services.AddScoped<BasketQueryHandler>();

builder.Services.AddScoped<BasketService>();

builder.Services.AddHttpClient<IProductService, HttpProductService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();