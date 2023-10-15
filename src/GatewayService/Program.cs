using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.Authority = builder.Configuration["IdentityServiceUrl"];
        opt.RequireHttpsMetadata = false; // false because we're using http
        opt.TokenValidationParameters.ValidateAudience = false;
        opt.TokenValidationParameters.NameClaimType = "username"; //makes it easy to use inside our api controllers
    });

var app = builder.Build();

app.MapReverseProxy();

app.UseAuthentication();
app.UseAuthorization();

app.Run();