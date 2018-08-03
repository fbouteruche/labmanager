using labmanager.core;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using McMaster.Extensions.CommandLineUtils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace labmanager.console
{
    class Program
    {
        private static readonly string environmentName = System.Environment.GetEnvironmentVariable("DOTNETCORE_ENVIRONMENT");
        private static IConfigurationRoot configuration;
        private static Uri organizationUri;
        private static string pat;
        private static string project;
        private static string release;

        static void Main(string[] args)
        {
            LoadConfiguration();

            ReleaseManager releaseManager = new ReleaseManager(organizationUri, pat);

            var app = new CommandLineApplication();

            // template commands
            app.Command("template", templateCmd =>
            {
                templateCmd.Command("list", listCmd =>
                 {
                     // list template
                     listCmd.OnExecute(() =>
                     {
                         Console.WriteLine(JsonConvert.SerializeObject(releaseManager.GetEnvironmentTemplates(project), Formatting.Indented));
                     });
                 });

                templateCmd.Command("show", showCmd =>
                {
                    CommandOption id = showCmd.Option("--id", "id of the template", CommandOptionType.SingleValue).IsRequired();
                    showCmd.OnExecute(() =>
                    {
                        Console.WriteLine(JsonConvert.SerializeObject(releaseManager.GetEnvironmentTemplate(project, Guid.Parse(id.Value())), Formatting.Indented));
                    });
                });
            });

            // environment commands
            app.Command("environment", environmentCmd =>
            {
                // list environments
                environmentCmd.OnExecute(() =>
                {
                    GetEnvironnements(releaseManager);
                });

                environmentCmd.Command("create", createCmd =>
                {
                    CommandOption variables = createCmd.Option("--variables", "set values for template variables in json format ie {'variablename':'variablevalue'}", CommandOptionType.SingleValue);
                    CommandArgument templateId = createCmd.Argument("templateId", "Id of the template").IsRequired();
                    createCmd.OnExecute(() =>
                    {
                        JObject jsonVariables = null;
                        jsonVariables = variables.HasValue() ? JsonConvert.DeserializeObject(variables.Value()) as JObject : null;
                        releaseManager.CreateEnvironment(project, release, Guid.Parse(templateId.Value), jsonVariables);
                    });
                });
            });
            
            app.Execute(args);
        }

        private static void GetEnvironnements(ReleaseManager releaseManager)
        {
            IEnumerable<(string Name, int Id)> environments = releaseManager.GetReleaseDefinitionEnvironements(project, release);
            foreach (var (Name, Id) in environments)
            {
                Console.WriteLine($"Name: {Name}");
                Console.WriteLine($"\tId: {Id}");
            }
        }

        private static void LoadConfiguration()
        {
            var builder = new ConfigurationBuilder()
                            .SetBasePath(Directory.GetCurrentDirectory())
                            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            if (environmentName == "Development")
            {

                builder.AddUserSecrets<Program>();
            }

            configuration = builder.Build();

            organizationUri = new Uri(configuration["organization"]);
            pat = configuration["pat"];
            project = configuration["project"];
            release = configuration["release"];
        }
    }
}
