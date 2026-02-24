using Task_Manager;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddDependencies(builder.Configuration);

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
});

app.UseHttpsRedirection();

app.UseCors();                // ✅ must be before auth

app.UseAuthentication();     // ✅ MISSING — must come before UseAuthorization
app.UseAuthorization();

app.MapControllers();

// app.MapHub<ChatHub>("/hubs/chat");        // ← add later when SignalR is ready
// app.MapHub<PresenceHub>("/hubs/presence"); // ← add later when SignalR is ready

app.Run();