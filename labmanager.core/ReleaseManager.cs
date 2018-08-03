using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi.Clients;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi.Contracts;
using Microsoft.VisualStudio.Services.WebApi;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace labmanager.core
{
    public class ReleaseManager
    {
        public Uri OrganizationUri
        {
            get;
            private set;
        }

        private readonly string PAT;

        private ReleaseHttpClient2 releaseClient;


        public ReleaseManager(Uri organizationUri, string pat)
        {
            OrganizationUri = organizationUri;
            PAT = pat;
            VssConnection connection = new VssConnection(OrganizationUri, new VssBasicCredential(String.Empty, PAT));
            
            releaseClient = connection.GetClient<ReleaseHttpClient2>();
        }

        public IEnumerable<(string Name, int Id)> GetReleaseDefinitionEnvironements(string project, string name)
        {
            ReleaseDefinition releaseDefinition = GetRelease(project, name);
            return releaseDefinition.Environments.Select(x => (Name: x.Name, Id: x.Id));
        }

        private ReleaseDefinition GetRelease(string project, string name)
        {
            List<ReleaseDefinition> releaseDefinitions = releaseClient.GetReleaseDefinitionsAsync(project, name).Result;
            if (releaseDefinitions is null || releaseDefinitions.Count != 1)
            {
                throw new Exception($"Several release definitions match the name {name}");
            }
            int releaseId = releaseDefinitions[0].Id;
            ReleaseDefinition releaseDefinition = releaseClient.GetReleaseDefinitionAsync(project, releaseId).Result;
            return releaseDefinition;
        }

        
        public void CreateEnvironment(string project, string release, Guid environmentId, JObject jsonVariables)
        {
            ReleaseDefinition releaseDefinition = GetRelease(project, release);
            ReleaseDefinitionEnvironmentTemplate environmentTemplate = GetEnvironmentTemplate(project, environmentId);
            ReleaseDefinitionEnvironment environment = environmentTemplate.Environment;
            int maxRank = releaseDefinition.Environments.Max(x => x.Rank);
            environment.Rank = maxRank + 1;
            environment.Name = Guid.NewGuid().ToString();

            List<string> variables = environment.Variables.Keys.ToList();
            foreach (var variable in variables)
            {
                environment.Variables[variable] = new ConfigurationVariableValue() { Value = "" };
            }
            if(jsonVariables != null)
            {
                foreach(var variable in jsonVariables)
                {
                    environment.Variables[variable.Key] = new ConfigurationVariableValue() { Value = variable.Value.ToString() };
                }
            }


            releaseDefinition.Environments.Add(environment);
            Task<ReleaseDefinition> result = releaseClient.UpdateReleaseDefinitionAsync(releaseDefinition, project);
            ReleaseDefinition update = result.Result;
        }

        public ReleaseDefinitionEnvironmentTemplate GetEnvironmentTemplate(string project, Guid templateId)
        {
            ReleaseDefinitionEnvironmentTemplate result = null;
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(
                    new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                    Convert.ToBase64String(
                        System.Text.ASCIIEncoding.ASCII.GetBytes(
                            string.Format("{0}:{1}", "", PAT))));

                using (HttpResponseMessage response = client.GetAsync(
                            $"https://msfrbouter.vsrm.visualstudio.com/{project}/_apis/Release/definitions/environmenttemplates?templateid={templateId.ToString()}").Result)
                {
                    response.EnsureSuccessStatusCode();
                    string responseBody = response.Content.ReadAsStringAsync().Result;
                    result = JsonConvert.DeserializeObject<ReleaseDefinitionEnvironmentTemplate>(responseBody);
                }
            }
            return result;
        }

        public EnvironmentTemplateSummaryCollection GetEnvironmentTemplates(string project)
        {
            EnvironmentTemplateSummaryCollection result = null;
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(
                    new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                    Convert.ToBase64String(
                        System.Text.ASCIIEncoding.ASCII.GetBytes(
                            string.Format("{0}:{1}", "", PAT))));

                using (HttpResponseMessage response = client.GetAsync(
                            $"https://msfrbouter.vsrm.visualstudio.com/{project}/_apis/Release/definitions/environmenttemplates").Result)
                {
                    response.EnsureSuccessStatusCode();
                    string responseBody = response.Content.ReadAsStringAsync().Result;
                    result = JsonConvert.DeserializeObject<EnvironmentTemplateSummaryCollection>(responseBody);
                }
            }
            return result;
        }
    }
}
