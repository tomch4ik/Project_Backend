﻿using Project_X_Data.Data;
using Project_X_Data.Data.Entities;
using Project_X_Data.Models.Rest;
using Project_X_Data.Services.Auth;
using Project_X_Data.Services.Kdf;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Project_X_Data.Data;

namespace Project_X_Data.Controllers.Api
{
    [Route("api/user")]
    [ApiController]
    public class UserController(DataAccessor dataAccessor, IConfiguration configuration, IKdfService kdfService, IAuthService authService) : ControllerBase
    {
        private readonly DataAccessor _dataAccessor = dataAccessor;
        private readonly IKdfService _kdfService = kdfService;
        private readonly IAuthService _authService = authService;
        private readonly IConfiguration _configuration = configuration;

        [HttpGet("jwt")]
        public RestResponse AuthenticateJwt()
        {
            RestResponse response = new();
            UserAccess userAccess;
            try
            {
                var (login, password) = GetBasicCredentials();
                userAccess = _dataAccessor.Authenticate(login, password)
                    ?? throw new Exception("Credentials rejected");
            }
            catch (Exception ex)
            {
                response.Status = RestStatus.Status401;
                response.Data = ex.Message;
                return response;
            }
            //Трансформуємо UserAccess у JWT
            var headerObject = new { alg = "HS256", typ = "JWT" };
            String headerJson = JsonSerializer.Serialize(headerObject);
            String header64 = Base64UrlTextEncoder.Encode(
                System.Text.Encoding.UTF8.GetBytes(headerJson)
            );
            //response.Data = header64;
            var payloadObject = new
            {
                //Стандартні поля
                iss = "Asp-32",   // Issuer	Identifies principal that issued the JWT.
                sub = userAccess.UserId,   // Subject	Identifies the subject of the JWT.
                aud = userAccess.RoleId,   // Audience	Identifies the recipients that the JWT is intended for. Each principal intended to process the JWT must identify itself with a value in the audience claim. If the principal processing the claim does not identify itself with a value in the aud claim when this claim is present, then the JWT must be rejected.
                exp = DateTime.Now.AddMinutes(10),   // Expiration Time	Identifies the expiration time on and after which the JWT must not be accepted for processing. The value must be a NumericDate:[9] either an integer or decimal, representing seconds past 1970-01-01 00:00:00Z.
                nbf = DateTime.Now,   // Not Before	Identifies the time on which the JWT will start to be accepted for processing. The value must be a NumericDate.
                iat = DateTime.Now,   // Issued at	Identifies the time at which the JWT was issued. The value must be a NumericDate.
                jti = Guid.NewGuid(),   // JWT ID	Case-sensitive unique identifier of the token even among different issuers.iss	Issuer	Identifies principal that issued the JWT.
                //Не стандартні поля
                name = userAccess.User.Name,
                email = userAccess.User.Email,
            };
            String payloadJson = JsonSerializer.Serialize(payloadObject);
            String payload64 = Base64UrlTextEncoder.Encode(
                System.Text.Encoding.UTF8.GetBytes(payloadJson)
            );

            String secret = _configuration.GetSection("Jwt").GetSection("Secret").Value
                ?? throw new KeyNotFoundException("Not found configuration 'Jwt.Secret'");
            String tokenBody = header64 + "." + payload64;

            String signature = Base64UrlTextEncoder.Encode(
                System.Security.Cryptography.HMACSHA256.HashData(
                 System.Text.Encoding.UTF8.GetBytes(secret),
                 System.Text.Encoding.UTF8.GetBytes(tokenBody)
             ));

            response.Data = tokenBody + '.' + signature;
            return response;
        }

        private (String, String) GetBasicCredentials()
        {
            String? header = HttpContext.Request.Headers.Authorization;
            if (header == null)      // Basic QWxhZGRpbjpvcGVuIHNlc2FtZQ==
            {
                throw new Exception("Authorization Header Required");
            }
            String credentials =    // 'Basic ' - length = 6
                header[6..];        // QWxhZGRpbjpvcGVuIHNlc2FtZQ==
            String userPass =       // Aladdin:open sesame
                System.Text.Encoding.UTF8.GetString(
                    Convert.FromBase64String(credentials));

            String[] parts = userPass.Split(':', 2);
            String login = parts[0];
            String password = parts[1];
            return (login, password);
        }

        [HttpGet]
        public object Authenticate()
        {
            String? header = HttpContext.Request.Headers.Authorization;
            if (header == null)      // Basic QWxhZGRpbjpvcGVuIHNlc2FtZQ==
            {
                HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return new { Status = "Authorization Header Required" };
            }

            /* Д.З. Реалізувати повний цикл перевірок даних, що передаються
             * для автентифікації
             * - заголовок починається з 'Basic '
             * - credentials успішно декодуються з Base64
             * - userPass ділиться на дві частини (може не містити ":")
             */
            const string prefix = "Basic ";
            if (!header.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                return new { Status = "Invalid Authorization Scheme" };
            }

            String credentials =    // 'Basic ' - length = 6
                header[prefix.Length..];        // QWxhZGRpbjpvcGVуIHNlc2FtZQ==
            if (string.IsNullOrWhiteSpace(credentials))
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                return new { Status = "Empty credentials" };
            }

            String userPass;        // Aladdin:open sesame
            try
            {
                userPass = System.Text.Encoding.UTF8.GetString(
                    Convert.FromBase64String(credentials));
            }
            catch
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                return new { Status = "Invalid Base64 credentials" };
            }

            String[] parts = userPass.Split(':', 2);
            if (parts.Length != 2)
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                return new { Status = "Credentials must be in format login:password" };
            }

            String login = parts[0];
            String password = parts[1];

            var userAccess = _dataAccessor.Authenticate(login, password);

            if (userAccess == null)
            {
                HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return new { Status = "Credentials rejected" };
            }

            // зберігаємо у сесії факт успішної автентифікації
            //HttpContext.Session.SetString(
            //    "UserController::Authenticate",
            //    JsonSerializer.Serialize(userAccess)
            //);
            _authService.SetAuth(userAccess);
            return userAccess;
        }


        [HttpPost]
        public object SignUp()
        {
            return new { Status = "SignUp Works" };
        }

        [HttpPost("admin")]  // POST /api/user/admin
        public object SignUpAdmin()
        {
            return new { Status = "SignUpAdmin Works" };
        }
    }
}

/* Відмінності АРІ та MVC контролерів
 * MVC:
 *  адресація за назвою дії (Action) - різні дії -- різні адреси
 *  GET  /Home/Index     --> HomeController.Index()
 *  POST /Home/Index     --> HomeController.Index()
 *  GET  /Home/Privacy   --> HomeController.Privacy()
 *  повернення - IActionResult частіше за все View
 *  
 * API:
 *  адресація за анотацією [Route("api/user")], різниця
 *  у методах запиту
 *  GET  api/user  ---> [HttpGet] Authenticate()
 *  POST api/user  ---> [HttpPost] SignUp()
 *  PUT  api/user  ---> 
 *  
 *  C   POST
 *  R   GET
 *  U   PUT(replace) PATCH(partially update)
 *  D   DELETE
 */
/* Авторизація. Схеми.
 * 0) Кукі (Cookie) - заголовки НТТР-пакету, які зберігаються у клієнта
 *      та автоматично включаються ним до всіх наступних запитів до сервера
 *      "+" простота використання
 *      "-" автоматизовано тільки у браузерах, в інших програмах це справа
 *           програміста. 
 *      "-" відкритість, легкість перехоплення даних
 *      
 * 1) Сесії (серверні): базуються на Кукі, проте всі дані зберігаються
 *     на сервері, у куках передається тільки ідентифікатор сесії
 *     "+" покращена безпека
 *     "-" велике навантаження на сховище сервера
 *     
 * 2) Токени (клієнтські): клієнт зберігає токен, який лише перевіряється
 *     сервером.
 *     "+" відмова від кукі та сесій
 *     "-" більше навантаження на роботу сервера
 *  2а) Токени-ідентифікатори
 *  2б) Токени з даними (JWT)
 */