using Microsoft.AspNetCore.Http.Json;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;


record UserDto (string UserName, string Password);

internal static class Program
{
	internal static void Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);

		var securityScheme = new OpenApiSecurityScheme()
		{
			Name = "Authorization",
			Type = SecuritySchemeType.ApiKey,
			Scheme = "Bearer",
			BearerFormat = "JWT",
			In = ParameterLocation.Header,
			Description = "API Authentication System",
		};

		var securityReq = new OpenApiSecurityRequirement()
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
		};

		builder.Services.AddEndpointsApiExplorer();


		// Add JWT configuration
		builder.Services.AddAuthentication(options =>
		{
			options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
			options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
			options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
		}).AddJwtBearer(options =>
		{
			options.TokenValidationParameters = new TokenValidationParameters
			{
			ValidIssuer = builder.Configuration["Jwt:Issuer"],
				ValidAudience = builder.Configuration["Jwt:Audience"],
				IssuerSigningKey = new SymmetricSecurityKey
					(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
				ValidateIssuer = true,
				ValidateAudience = true,
				ValidateLifetime = false,
				ValidateIssuerSigningKey = true
			};
		});


		// Configure JSON options.
		builder.Services.Configure<JsonOptions>(options =>
		{
			options.SerializerOptions.IncludeFields = true;
		});

		// Add services to the container.

		builder.Services.AddControllers();
		// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
		builder.Services.AddEndpointsApiExplorer();
		builder.Services.AddSwaggerGen(options =>
		{
			options.SwaggerDoc("v1", new OpenApiInfo
			{
				Version = "v1",
				Title = "MinimalAPI-Auth",
				Description = "MinimalAPI-Auth Tester",
				TermsOfService = new Uri("https://example.com/terms"),
				Contact = new OpenApiContact
				{
					Name = "Example Contact",
					Url = new Uri("https://example.com/contact")
				},
				License = new OpenApiLicense
				{
					Name = "Example License",
					Url = new Uri("https://example.com/license")
				}
			});
			// var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
			// options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
			options.AddSecurityDefinition("Bearer", securityScheme);
			options.AddSecurityRequirement(securityReq);
		});

		builder.Services.AddAuthorization();
		builder.Services.AddEndpointsApiExplorer();

		var app = builder.Build();



		// Configure the HTTP request pipeline.
		if (app.Environment.IsDevelopment())
		{
			app.UseSwagger();
			app.UseSwaggerUI();
		}

		app.UseSwaggerUI(options =>
		{
			options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
			options.RoutePrefix = string.Empty;
		});

		app.UseAuthorization();

		app.MapControllers();

		app.UseAuthentication();
		app.UseAuthorization();

		app.MapPost("/GetToken", [AllowAnonymous] (UserDto user) =>
		{

			if (user.UserName=="uid" && user.Password=="pw")
			{
				var issuer = builder.Configuration["Jwt:Issuer"];
				var audience = builder.Configuration["Jwt:Audience"];
				var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]));
				var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

				// Now its ime to define the jwt token which will be responsible of creating our tokens
				var jwtTokenHandler = new JwtSecurityTokenHandler();

				// We get our secret from the appsettings
				var key = Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Key"]);

				// we define our token descriptor
					// We need to utilise claims which are properties in our token which gives information about the token
					// which belong to the specific user who it belongs to
					// so it could contain their id, name, email the good part is that these information
					// are generated by our server and identity framework which is valid and trusted
				var tokenDescriptor = new SecurityTokenDescriptor
				{
					Subject = new ClaimsIdentity(new []
					{
						new Claim("Id", "1"),
						new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
						new Claim(JwtRegisteredClaimNames.Email, user.UserName),
						// the JTI is used for our refresh token which we will be convering in the next video
						new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
					}),
					// the life span of the token needs to be shorter and utilise refresh token to keep the user signedin
					// but since this is a demo app we can extend it to fit our current need
					Expires = DateTime.UtcNow.AddHours(6),
					Audience = audience,
					Issuer = issuer,
					// here we are adding the encryption alogorithim information which will be used to decrypt our token
					SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512Signature)
				};

				var token = jwtTokenHandler.CreateToken(tokenDescriptor);

				var jwtToken = jwtTokenHandler.WriteToken(token);

				return Results.Ok(jwtToken);
			}
			else
			{
				return Results.Unauthorized();
			}
		});

		app.MapGet("/UseToken", [Authorize(AuthenticationSchemes = "Bearer")] () =>
		{
			return Results.Ok();
		});

		app.Run();
	}
}