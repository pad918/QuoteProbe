using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using TrashSearch.Data;
using TrashSearch.Services;
using Toolbelt.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<AudioDownloaderService>();
builder.Services.AddSingleton<FileTranscriberService>();
var databaseService = new DatabaseService();
builder.Services.AddSingleton(databaseService);
builder.Services.AddSingleton(new IndexerService().SetDatabase(databaseService));

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
builder.Services.AddCors(options =>
{
	options.AddPolicy(name: MyAllowSpecificOrigins,
					  policy =>
					  {
						  policy.AllowAnyOrigin();
					  });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Add live reload for css files in development mode
if (app.Environment.IsDevelopment())
    app.UseCssLiveReload();

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.UseCors(MyAllowSpecificOrigins);

app.Run();
