using Microsoft.EntityFrameworkCore;
using RpgApi.Data;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<DataContext>(options =>
{
   options.UseSqlServer(builder.Configuration.GetConnectionString("ConexaoSomee"));

   //"ConexaoSomee": "workstation id=DB-DS-LUIZSOUZA23.mssql.somee.com;packet size=4096;user id=sahas2023;pwd=sasqlh@s;data source=DB-DS-LUIZSOUZA23.mssql.somee.com;persist security info=False;initial catalog=DB-DS-LUIZSOUZA23;TrustServerCertificate=True"
   // ESSA CONEXÃO É A DO PROFESSOR, COLOQUEI AQ POIS O JSON NÃO ACEITA LINHAS COMENTADAS 
});

builder.Services.AddControllers().AddNewtonsoftJson(options=>
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore 
);

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
