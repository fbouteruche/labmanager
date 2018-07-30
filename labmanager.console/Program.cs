using labmanager.core;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;

namespace labmanager.console
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            string environmentName = System.Environment.GetEnvironmentVariable("DOTNETCORE_ENVIRONMENT");
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            if(environmentName == "Development")
            {

                builder.AddUserSecrets<Program>();
            }

            IConfigurationRoot configuration = builder.Build();

            Uri organizationUri = new Uri(configuration["organization"]);
            string pat = configuration["pat"];
            string project = configuration["project"];

            ReleaseManager releaseManager = new ReleaseManager(organizationUri, pat);

            IEnumerable<string> releaseDefinitions = releaseManager.GetReleaseDefinition(project);


        }
    }
}
