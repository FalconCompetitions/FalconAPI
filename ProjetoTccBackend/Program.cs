using System.Reflection;
using System.Text;
using ApiEstoqueASP.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ProjetoTccBackend.Database;
using ProjetoTccBackend.Filters;
using ProjetoTccBackend.Hubs;
using ProjetoTccBackend.Middlewares;
using ProjetoTccBackend.Models;
using ProjetoTccBackend.Repositories;
using ProjetoTccBackend.Repositories.Interfaces;
using ProjetoTccBackend.Services;
using ProjetoTccBackend.Services.Interfaces;
using ProjetoTccBackend.Swagger.Extensions;
using ProjetoTccBackend.Swagger.Filters;
using ProjetoTccBackend.Workers;

namespace ProjetoTccBackend
{
    public class ProjetoTccBackend
    {
        /// <summary>
        /// Cria as funções padrão no sistema se elas não existirem.
        /// </summary>
        /// <param name="serviceProvider">Provedor de serviços para obter os gerenciadores de função e usuário.</param>
        /// <returns>Uma tarefa assíncrona.</returns>
        public static async Task CreateRoles(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<User>>();

            string[] roleNames = { "Admin", "Student", "Teacher" };

            IdentityResult roleResult;

            foreach (string roleName in roleNames)
            {
                bool roleExist = await roleManager.RoleExistsAsync(roleName);
                if (roleExist is false)
                {
                    roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }

        public static void ExecuteMigrations(WebApplication app)
        {
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<TccDbContext>();
                db.Database.Migrate();
            }
        }

        private static void ConfigureWebSocketOptions(WebApplication app)
        {
            var options = new WebSocketOptions()
            {
                KeepAliveInterval = TimeSpan.FromMinutes(2),
                AllowedOrigins = { "http://localhost:3000" },
            };

            app.UseWebSockets(options);
        }

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();

            // Add services to the container.
            //builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
            builder.Services.AddDbContext<TccDbContext>();
            builder
                .Services.AddIdentity<User, IdentityRole>(options =>
                {
                    options.User.AllowedUserNameCharacters =
                        options.User.AllowedUserNameCharacters + " ";
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequiredLength = 8;
                })
                .AddEntityFrameworkStores<TccDbContext>()
                .AddDefaultTokenProviders();

            builder.Services.AddMemoryCache();

            // Repositories
            builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            builder.Services.AddScoped<IGroupRepository, GroupRepository>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<ILogRepository, LogRepository>(); // Adicionado para Log
            builder.Services.AddScoped<IExerciseInputRepository, ExerciseInputRepository>();
            builder.Services.AddScoped<IExerciseOutputRepository, ExerciseOutputRepository>();
            builder.Services.AddScoped<IExerciseRepository, ExerciseRepository>();
            builder.Services.AddScoped<ICompetitionRepository, CompetitionRepository>();
            builder.Services.AddScoped<
                ICompetitionRankingRepository,
                CompetitionRankingRepository
            >();
            builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
            builder.Services.AddScoped<
                IGroupExerciseAttemptRepository,
                GroupExerciseAttemptRepository
            >();

            builder.Services.AddHttpContextAccessor();

            builder.Services.AddHttpClient(
                "JudgeAPI",
                client =>
                {
                    client.BaseAddress = new Uri(builder.Configuration["JudgeApiUrl"]!);
                    //client.DefaultRequestHeaders.Add("Content-Type", "application/json");
                    //client.DefaultRequestHeaders.Add("Accept", "application/json");
                    client.Timeout = TimeSpan.FromSeconds(20);
                }
            );

            // Services
            builder.Services.AddSingleton<ICompetitionStateService, CompetitionStateService>();
            builder.Services.AddScoped<IJudgeService, JudgeService>();
            builder.Services.AddScoped<ITokenService, TokenService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IGroupService, GroupService>();
            builder.Services.AddScoped<ILogService, LogService>(); // Adicionado para Log
            builder.Services.AddScoped<ICompetitionRankingService, CompetitionRankingService>();
            builder.Services.AddScoped<IExerciseService, ExerciseService>();
            builder.Services.AddScoped<ICompetitionService, CompetitionService>();
            builder.Services.AddScoped<IGroupAttemptService, GroupAttemptService>();
            builder.Services.AddScoped<IQuestionService, QuestionService>();

            builder.Services.AddSignalR();

            builder.Services.AddHostedService<CompetitionStateWorker>();

            builder
                .Services.AddControllers(options =>
                {
                    options.Filters.Add<ValidateModelStateFilter>();
                })
                .ConfigureApiBehaviorOptions(options =>
                {
                    options.SuppressModelStateInvalidFilter = true;
                })
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.MaxDepth = 4;
                    options.JsonSerializerOptions.ReferenceHandler = System
                        .Text
                        .Json
                        .Serialization
                        .ReferenceHandler
                        .IgnoreCycles;
                });

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                string xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

                options.IncludeXmlComments(xmlPath);
                options.OperationFilter<SwaggerResponseExampleFilter>();
                options.OperationFilter<ForceJsonOnlyOperationFilter>();
            });
            builder.Services.AddSwaggerExamples(Assembly.GetExecutingAssembly());

            builder
                .Services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateIssuerSigningKey = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidIssuer = builder.Configuration["Jwt:Issuer"]!,
                        ValidAudience = builder.Configuration["Jwt:Audience"]!,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
                        ),

                        ClockSkew = TimeSpan.Zero,
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            // Nome do cookie que armazena o JWT
                            var cookieName = "CompetitionAuthToken";

                            var token = context.Request.Cookies[cookieName];
                            Console.WriteLine(token);

                            if (!string.IsNullOrEmpty(token))
                            {
                                context.Token = token;
                            }
                            else
                            {
                                // Mantém a lógica para SignalR
                                var accessToken = context.Request.Query["token"];
                                var path = context.HttpContext.Request.Path;
                                if (
                                    !string.IsNullOrEmpty(accessToken)
                                    && path.StartsWithSegments("/hub/competition")
                                )
                                {
                                    context.Token = accessToken;
                                }
                            }

                            return Task.CompletedTask;
                        },
                    };
                });

            /*
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("StudentUser", policy => policy.AddRequirements(new StudentUserRole());
            });
            */

            /*
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("FrontendAppPolicy", policy =>
                {
                    policy.WithOrigins("https://localhost:3000")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });
            */

            var app = builder.Build();

            ExecuteMigrations(app);

            CreateRoles(app.Services.CreateScope().ServiceProvider!).GetAwaiter().GetResult();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SupportedSubmitMethods(
                        new[]
                        {
                            Swashbuckle.AspNetCore.SwaggerUI.SubmitMethod.Get,
                            Swashbuckle.AspNetCore.SwaggerUI.SubmitMethod.Post,
                            Swashbuckle.AspNetCore.SwaggerUI.SubmitMethod.Put,
                            Swashbuckle.AspNetCore.SwaggerUI.SubmitMethod.Delete,
                        }
                    );
                });
                app.UseDeveloperExceptionPage();

                // Adicione no pipeline logo após app.UseRouting();
            }

            app.UseRouting();

            //app.UseCors("FrontendAppPolicy");
            app.UseCors(builder =>
                builder
                    .WithOrigins("http://localhost:3000") // ou seu frontend
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials()
            );

            ConfigureWebSocketOptions(app);

            app.UseMiddleware<ExceptionHandlingMiddleware>();
            //app.UseMiddleware<ResetCookiesMiddleware>();

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            app.MapHub<CompetitionHub>("/hub/competition");

            app.Run();
        }
    }

    //
    // Plano em pseudocódigo:
    // 1. Antes de iniciar o pipeline, adicionar um middleware que apague todos os cookies da requisição.
    // 2. O middleware será executado no início de cada execução do app (a cada requisição).
    // 3. Para cada cookie presente, definir o mesmo nome com valor vazio e expiração no passado.
    // 4. Adicionar esse middleware antes de qualquer autenticação ou autorização.

    // Middleware para resetar cookies

    /// <summary>
    /// Middleware that removes all cookies from the incoming HTTP request and sets their expiration to the past.
    /// This is only used in development
    /// </summary>
    /// <remarks>This middleware is designed to clear cookies at the beginning of the request pipeline. It
    /// iterates through all  cookies in the incoming request and appends them to the response with an empty value and
    /// an expiration date in  the past, effectively invalidating them. This ensures that no cookies are carried forward
    /// in the request lifecycle.  To use this middleware, add it to the application's middleware pipeline before any
    /// authentication or authorization  middleware to ensure cookies are cleared prior to those operations.</remarks>
    public class ResetCookiesMiddleware
    {
        private readonly RequestDelegate _next;

        public ResetCookiesMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            foreach (var cookie in context.Request.Cookies.Keys)
            {
                context.Response.Cookies.Append(
                    cookie,
                    "",
                    new CookieOptions { Expires = DateTimeOffset.UtcNow.AddDays(-1), Path = "/" }
                );
            }
            await _next(context);
        }
    }
}
