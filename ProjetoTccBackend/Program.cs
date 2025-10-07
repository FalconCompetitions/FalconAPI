using System.Reflection;
using System.Text;
using ApiEstoqueASP.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Core;
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
using ProjetoTccBackend.Workers.Queues;

namespace ProjetoTccBackend
{
    public class ProjetoTccBackend
    {
        /// <summary>
        /// Cria as fun��es padr�o no sistema se elas n�o existirem.
        /// </summary>
        /// <param name="serviceProvider">Provedor de servi�os para obter os gerenciadores de fun��o e usu�rio.</param>
        /// <returns>Uma tarefa ass�ncrona.</returns>
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

        public static async Task CreateAdminUser(IServiceProvider serviceProvider)
        {
            var dbContext = serviceProvider.GetRequiredService<TccDbContext>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
            var signInManager = serviceProvider.GetRequiredService<SignInManager<User>>();

            List<User> adminUser = new List<User>()
            {
                new User()
                {
                    RA = "999999",
                    Email = "admin@gmail.com",
                    Name = "admin",
                    UserName = "admin",
                    EmailConfirmed = false,
                    PhoneNumberConfirmed = false,
                    TwoFactorEnabled = false,
                },
            };

            List<User> teacherUsers = new List<User>()
            {
                new User()
                {
                    RA = "222222",
                    Email = "professor1@gmail.com",
                    Name = "Jo�o",
                    UserName = "professor1@gmail.com",
                    EmailConfirmed = false,
                    PhoneNumberConfirmed = false,
                    TwoFactorEnabled = false,
                },
                new User()
                {
                    RA = "222223",
                    Email = "professor2@gmail.com",
                    Name = "�lvaro",
                    UserName = "professor2@gmail.com",
                    EmailConfirmed = false,
                    PhoneNumberConfirmed = false,
                    TwoFactorEnabled = false,
                },
                new User()
                {
                    RA = "222224",
                    Email = "professor3@gmail.com",
                    Name = "Manuel",
                    UserName = "professor3@gmail.com",
                    EmailConfirmed = false,
                    PhoneNumberConfirmed = false,
                    TwoFactorEnabled = false,
                },
                new User()
                {
                    RA = "222225",
                    Email = "professor4@gmail.com",
                    Name = "Renato Coach",
                    UserName = "professor4@gmail.com",
                    EmailConfirmed = false,
                    PhoneNumberConfirmed = false,
                    TwoFactorEnabled = false,
                },
            };

            List<User> studentUsers = new List<User>()
            {
                new User()
                {
                    RA = "111111",
                    Email = "aluno1@gmail.com",
                    Name = "Diego J�nior",
                    UserName = "aluno1@gmail.com",
                    EmailConfirmed = false,
                    PhoneNumberConfirmed = false,
                    TwoFactorEnabled = false,
                },
                new User()
                {
                    RA = "111112",
                    Email = "aluno2@gmail.com",
                    Name = "Can�rio Arrega�ado",
                    UserName = "aluno2@gmail.com",
                    EmailConfirmed = false,
                    PhoneNumberConfirmed = false,
                    TwoFactorEnabled = false,
                },
                new User()
                {
                    RA = "111113",
                    Email = "aluno3@gmail.com",
                    Name = "Roberto",
                    UserName = "aluno3@gmail.com",
                    EmailConfirmed = false,
                    PhoneNumberConfirmed = false,
                    TwoFactorEnabled = false,
                },
                new User()
                {
                    RA = "111114",
                    Email = "aluno4@gmail.com",
                    Name = "Coach J�nior",
                    UserName = "aluno4@gmail.com",
                    EmailConfirmed = false,
                    PhoneNumberConfirmed = false,
                    TwoFactorEnabled = false,
                },
            };

            foreach (User user in adminUser)
            {
                User? existentUser = await userManager.FindByEmailAsync(user.Email!);
                if (existentUser is null)
                {
                    var result = await userManager.CreateAsync(user, "00000000#Ra");
                    if (result.Succeeded)
                    {
                        var createdUser = await userManager.FindByEmailAsync(user.Email!);
                        await userManager.AddToRoleAsync(createdUser, "Admin");
                    }
                }
            }

            foreach (User user in teacherUsers)
            {
                User? existentUser = await userManager.FindByEmailAsync(user.Email!);
                if (existentUser is null)
                {
                    var result = await userManager.CreateAsync(user, "00000000#Ra");

                    if (result.Succeeded)
                    {
                        var createdUser = await userManager.FindByEmailAsync(user.Email!);
                        await userManager.AddToRoleAsync(createdUser, "Teacher");
                    }
                }
            }

            foreach (User user in studentUsers)
            {
                User? existentUser = await userManager.FindByEmailAsync(user.Email!);
                if (existentUser is null)
                {
                    var result = await userManager.CreateAsync(user, "00000000#Ra");

                    if (result.Succeeded)
                    {
                        var createdUser = await userManager.FindByEmailAsync(user.Email!);
                        await userManager.AddToRoleAsync(createdUser, "Student");
                    }
                }
            }

            await dbContext.SaveChangesAsync();
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
            builder.Services.AddDbContext<TccDbContext>(options =>
            {
                options.UseMySql(
                    builder.Configuration.GetConnectionString("DefaultConnection"),
                    ServerVersion.AutoDetect(
                        builder.Configuration.GetConnectionString("DefaultConnection")
                    )
                );
            });
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
            builder.Services.AddScoped<IAttachedFileRepository, AttachedFileRepository>();
            builder.Services.AddScoped<IGroupRepository, GroupRepository>();
            builder.Services.AddScoped<IGroupInviteRepository, GroupInviteRepository>();
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
            builder.Services.AddScoped<
                IExerciseInCompetitionRepository,
                ExerciseInCompetitionRepository
            >();
            builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
            builder.Services.AddScoped<IAnswerRepository, AnswerRepository>();
            builder.Services.AddScoped<
                IGroupExerciseAttemptRepository,
                GroupExerciseAttemptRepository
            >();
            builder.Services.AddScoped<
                IExerciseSubmissionQueueItemRepository,
                ExerciseSubmissionQueueItemRepository
            >();

            builder.Services.AddHttpContextAccessor();

            builder
                .Services.AddHttpClient(
                    "JudgeAPI",
                    client =>
                    {
                        client.BaseAddress = new Uri(builder.Configuration["JudgeApi:Url"]!);
                        //client.DefaultRequestHeaders.Add("Content-Type", "application/json");
                        //client.DefaultRequestHeaders.Add("Accept", "application/json");
                        client.Timeout = TimeSpan.FromSeconds(40);
                    }
                )
                .ConfigurePrimaryHttpMessageHandler(() =>
                {
                    return new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback = (
                            message,
                            cert,
                            chain,
                            errors
                        ) => true,
                    };
                });

            // Services
            builder.Services.AddSingleton<ICompetitionStateService, CompetitionStateService>();
            builder.Services.AddScoped<IAttachedFileService, AttachedFileService>();
            builder.Services.AddScoped<IJudgeService, JudgeService>();
            builder.Services.AddScoped<ITokenService, TokenService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IGroupInviteService, GroupInviteService>();
            builder.Services.AddScoped<IGroupService, GroupService>();
            builder.Services.AddScoped<ILogService, LogService>(); // Adicionado para Log
            builder.Services.AddScoped<ICompetitionRankingService, CompetitionRankingService>();
            builder.Services.AddScoped<IExerciseService, ExerciseService>();
            builder.Services.AddScoped<ICompetitionService, CompetitionService>();
            builder.Services.AddScoped<IGroupAttemptService, GroupAttemptService>();
            builder.Services.AddScoped<IQuestionService, QuestionService>();

            // Queues
            builder.Services.AddSingleton<ExerciseSubmissionQueue>();

            builder.Services.AddSignalR();

            builder.Services.AddHostedService<CompetitionStateWorker>();
            builder.Services.AddHostedService<ExerciseSubmissionWorker>();

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
                    options.JsonSerializerOptions.MaxDepth = 8;
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
                                // Mant�m a l�gica para SignalR
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

            builder.Services.Configure<KestrelServerOptions>(options =>
            {
                options.Limits.MaxRequestBodySize = 40 * 1024 * 1024; // 40MB
            });

            /*
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("StudentUser", policy => policy.AddRequirements(new StudentUserRole());
            });
            */

            builder.Services.AddCors(options =>
            {
                options.AddPolicy(
                    "FrontendAppPolicy",
                    policy =>
                    {
                        policy
                            .WithOrigins("https://localhost:3000")
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .AllowCredentials();
                    }
                );

                options.AddPolicy(
                    "JudgeApiPolicy",
                    policy =>
                    {
                        policy
                            .WithOrigins("https://localhost:8000")
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .AllowCredentials();
                    }
                );
            });

            var app = builder.Build();

            ExecuteMigrations(app);

            CreateRoles(app.Services.CreateScope().ServiceProvider!).GetAwaiter().GetResult();
            CreateAdminUser(app.Services.CreateScope().ServiceProvider!).GetAwaiter().GetResult();

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

                // Adicione no pipeline logo ap�s app.UseRouting();
            }

            app.UseRouting();

            app.UseCors("FrontendAppPolicy");
            app.UseCors("JudgeApiPolicy");

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
    // Plano em pseudoc�digo:
    // 1. Antes de iniciar o pipeline, adicionar um middleware que apague todos os cookies da requisi��o.
    // 2. O middleware ser� executado no in�cio de cada execu��o do app (a cada requisi��o).
    // 3. Para cada cookie presente, definir o mesmo nome com valor vazio e expira��o no passado.
    // 4. Adicionar esse middleware antes de qualquer autentica��o ou autoriza��o.

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
