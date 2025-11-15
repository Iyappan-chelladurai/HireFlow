using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();


 

// --- Authentication: Validate JWT issued by Main API (Authority or symmetric key) ---
// Two common options:
// A) Validate JWT with Authority (OpenID Connect) -> AddJwtBearer with Authority
// B) Validate JWT with shared signing key -> TokenValidationParameters with IssuerSigningKey
 

var authSettings = builder.Configuration.GetSection("Auth");
var useAuthority = authSettings.GetValue<bool>("UseAuthority", false);


if (useAuthority)
{
    var authority = authSettings.GetValue<string>("Authority")!; // e.g., https://auth.yourdomain.com
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = authority;
        options.TokenValidationParameters.ValidateAudience = false; // if tokens don't have audience for microservice
    });
}
else
{
    var secret = authSettings.GetValue<string>("SigningKey") ?? throw new InvalidOperationException("SigningKey missing");
    var key = System.Text.Encoding.UTF8.GetBytes(secret);
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(key)
        };
    });
}


builder.Services.AddAuthorization();


 


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();
app.UseCors("AllowMainApi");
app.UseAuthentication();
app.UseAuthorization();


app.MapControllers();


// Simple health endpoint
app.MapGet("/health", () => Results.Ok(new { status = "Healthy", now = DateTime.UtcNow }));


app.Run();