using FluentValidation;
using FluentValidation.AspNetCore;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ShopNT.Services.Dtos.BrandDtos;
using ShopNT.Services.Profiles;
using ShopNT.Core.Entities;
using ShopNT.Core.Repositories;
using ShopNT.Data;
using ShopNT.Data.Repositories;
using System.Text;
using ShopNT.Services.Interfaces;
using ShopNT.Services.Implementations;
using ShopNT.Api.Middlewares;
using Microsoft.AspNetCore.Mvc;
using ShopNT.Services.Exceptions;
using AutoMapper;

var builder = WebApplication.CreateBuilder(args);



builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState.Where(x => x.Value.Errors.Count() > 0)
            .Select(x => new RestExceptionError(x.Key, x.Value.Errors.First().ErrorMessage)).ToList();

            return new BadRequestObjectResult(new { message = "", errors });
        };
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ShopNTApi",
        Version = "v1",
        Description = "An API to perform Employee operations",
        TermsOfService = new Uri("https://example.com/terms"),
        Contact = new OpenApiContact
        {
            Name = "John Walkner",
            Email = "John.Walkner@gmail.com",
            Url = new Uri("https://twitter.com/jwalkner"),
        },
        License = new OpenApiLicense
        {
            Name = "Employee API LICX",
            Url = new Uri("https://example.com/license"),
        }
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 1safsfsdfdfd\"",
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
             new string[] {}
     }
 });
});
builder.Services.AddDbContext<ShopNTDbContext>(opt =>
{
    opt.UseSqlServer(builder.Configuration.GetConnectionString("Default"));
});

builder.Services.AddIdentity<AppUser, IdentityRole>(opt =>
{
    opt.Password.RequiredLength = 8;
    opt.Password.RequireNonAlphanumeric = false;
}).AddDefaultTokenProviders().AddEntityFrameworkStores<ShopNTDbContext>();

builder.Services.AddFluentValidationRulesToSwagger();

builder.Services.AddHttpContextAccessor();

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<BrandPostDtoValidator>();

builder.Services.AddScoped<IBrandRepository,BrandRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IBrandService,BrandService>();
builder.Services.AddScoped<IProductService, ProductService>();

builder.Services.AddScoped(provider =>
new MapperConfiguration(opt =>
{
    opt.AddProfile(new MapperProfile(provider.GetService<IHttpContextAccessor>()));
}).CreateMapper());

builder.Services.AddAuthentication(opt =>
{
    opt.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

}).AddJwtBearer(opt =>
{
    opt.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidAudience = builder.Configuration["Security:Audience"],
        ValidIssuer = builder.Configuration["Security:Issuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Security:Secret"]))
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.MapControllers();

app.Run();
