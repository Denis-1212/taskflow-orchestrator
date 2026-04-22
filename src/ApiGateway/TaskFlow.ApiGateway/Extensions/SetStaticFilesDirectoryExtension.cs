namespace TaskFlow.ApiGateway.Extensions;

using Microsoft.Extensions.FileProviders;

public static class SetStaticFilesDirectoryExtension
{

    #region Methods

    public static IApplicationBuilder SetStaticFilesDirectory(this IApplicationBuilder builder)
    {
        string wwwrootPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot");
        var env = builder.ApplicationServices.GetRequiredService<IWebHostEnvironment>();

        if (env.IsDevelopment())
        {
            string projectRoot = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "../../../"));
            wwwrootPath = Path.Combine(projectRoot, "client/taskflow-orchestrator-frontend/dist");

            if (!Directory.Exists(wwwrootPath))
            {
                throw new DirectoryNotFoundException(wwwrootPath);
            }
        }

        return builder.UseStaticFiles(
            new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(wwwrootPath)
            });
    }

    #endregion

}
