using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi.Clients;
using Microsoft.VisualStudio.Services.WebApi;
using System;
using System.Collections.Generic;

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

        public IEnumerable<string> GetReleaseDefinition(string project)
        {
            List<string> releaseDefinitionsList = new List<string>();
            var definitions = releaseClient.GetReleaseDefinitionsAsync2(project);
            foreach (var def in definitions.Result)
            {
                releaseDefinitionsList.Add(def.Name);
            }
            return releaseDefinitionsList;
        }
    }
}
