1. This project is implement JWT(json web token) authentication in asp.net core 3.1 webapi
2. The following nuget packages are installed
    Microsoft.AspNetCore.Authentication.JwtBearer
    Microsoft.AspNetCore.Identity.EntityFrameworkCore
    Microsoft.EntityFrameworkCore.SqlServer
    Microsoft.EntityFrameworkCore.Tools
3. To register EF, Asp.net Identity and JWT token service added below code to startup.cs (inside ConfigureService method)
   //For EF
            services.AddDbContext<ApplicationDbContext>(optionAction =>
            {
                optionAction.UseSqlServer(Configuration.GetConnectionString("DefaultDbConnection"));
            });

            //For Identity
            services.AddIdentity<IdentityUser, IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

            //Authentication
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidAudience = Configuration["JWT:ValidAudience"],
                    ValidIssuer = Configuration["JWT:ValidIssuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JWT:Secret"]))
                };
            });
4. For add authentication and authorization in request handling pipeline added below lines of code in Startup.cs(inside Configure method)
            app.UseAuthentication();
            app.UseAuthorization();
5. The IdentityUser class is responsible for user add, password check etc (user related actions)
6. The IdentityRole class is reponsible for role add, role check etc (role related actions)
7. To add Authentication over a controller or a specific action method use [Authorize] 
8. To add Role based authorization pass the required role in AuthorizeFilter constructor
