using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MyGet.Tools.SemVerExplorer.Code
{
    public sealed class NuGetV3SearchService
    {
        public const string DefaultServiceIndexUrl = "https://api.nuget.org/v3/index.json";

        private static readonly JsonSerializer DefaultSerializer = new JsonSerializer { DateParseHandling = DateParseHandling.DateTimeOffset };
        private static readonly HttpClient HttpClient = new HttpClient();

        private readonly string _serviceIndexUrl;

        public static class ServiceTypes
        {
            public static readonly string SearchAutocomplete = "SearchAutocompleteService";
        }

        public NuGetV3SearchService(string serviceIndexUrl = DefaultServiceIndexUrl)
        {
            _serviceIndexUrl = serviceIndexUrl;
        }

        public async Task<Dictionary<string, List<Uri>>> GetServicesAsync()
        {
            var serviceIndex = await ExecuteRequestAsync<JObject>(_serviceIndexUrl);

            var result = new Dictionary<string, List<Uri>>();

            JToken resources;
            if (serviceIndex.TryGetValue("resources", out resources))
            {
                foreach (var resource in resources)
                {
                    JToken type = resource["@type"];
                    JToken id = resource["@id"];

                    if (type == null || id == null)
                    {
                        continue;
                    }

                    if (type.Type == JTokenType.Array)
                    {
                        foreach (var nType in type)
                        {
                            AddEndpoint(result, nType, id);
                        }
                    }
                    else
                    {
                        AddEndpoint(result, type, id);
                    }
                }
            }

            return result;
        }

        public async Task<NuGetV3AutocompleteResult> AutocompleteAsync(string query = "", string id = "", string version = "", string supportedFramework = null, bool prerelease = false, int skip = 0, int take = 100)
        {
            var queryString = string.Empty;
            if (!string.IsNullOrEmpty(query))
            {
                queryString = "q=" + UrlEncode(query)
                               + "&supportedFramework=" + UrlEncode(supportedFramework)
                               + "&prerelease=" + prerelease.ToString().ToLowerInvariant()
                               + "&skip=" + skip
                               + "&take=" + take;
            }
            else if (!string.IsNullOrEmpty(id))
            {
                queryString = "id=" + UrlEncode(id)
                               + "&version=" + UrlEncode(version)
                               + "&supportedFramework=" + UrlEncode(supportedFramework)
                               + "&prerelease=" + prerelease.ToString().ToLowerInvariant()
                               + "&skip=" + skip
                               + "&take=" + take;
            }

            return await ExecuteServiceRequestAsync<NuGetV3AutocompleteResult>(ServiceTypes.SearchAutocomplete, null, queryString);
        }

        private string UrlEncode(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                return Uri.EscapeUriString(value);
            }

            return null;
        }

        private static void AddEndpoint(IDictionary<string, List<Uri>> result, JToken typeToken, JToken idToken)
        {
            string type = (string)typeToken;
            string id = (string)idToken;

            if (type == null || id == null)
            {
                return;
            }

            if (type.Contains("/"))
            {
                //string version = type.Substring(type.IndexOf("/", StringComparison.Ordinal) + 1);
                type = type.Substring(0, type.IndexOf("/", StringComparison.Ordinal));
            }

            List<Uri> ids;
            if (!result.TryGetValue(type, out ids))
            {
                ids = new List<Uri>();
                result.Add(type, ids);
            }

            Uri uri;
            if (Uri.TryCreate(id, UriKind.Absolute, out uri))
            {
                ids.Add(new Uri(id));
            }
        }

        private async Task<T> ExecuteServiceRequestAsync<T>(string serviceType, string path = null, string query = null)
        {
            using (var reader = new JsonTextReader(new StreamReader(
                await ExecuteServiceRequestStreamAsync(serviceType, path, query))))
            {
                return DefaultSerializer.Deserialize<T>(reader);
            }
        }

        private async Task<Stream> ExecuteServiceRequestStreamAsync(string serviceType, string path = null, string query = null)
        {
            var services = await GetServicesAsync();

            if (!services.ContainsKey(serviceType))
            {
                throw new NotSupportedException(
                    string.Format("The package source does not expose a {0} service type.", serviceType));
            }

            var serviceEndpoints = services[serviceType];
            foreach (var serviceEndpoint in serviceEndpoints)
            {
                var serviceUrl = new UriBuilder(serviceEndpoint);

                if (!string.IsNullOrEmpty(path))
                {
                    serviceUrl.Path = serviceUrl.Path.TrimEnd('/') + "/" + path;
                }

                if (!string.IsNullOrEmpty(query))
                {
                    serviceUrl.Query = query;
                }

                try
                {
                    return await ExecuteRequestAsync(serviceUrl.Uri.ToString());
                }
                catch (Exception)
                {
                    if (serviceEndpoint == serviceEndpoints.Last())
                    {
                        throw;
                    }
                }
            }

            throw new Exception("The request failed for an unknown reason.");
        }

        private async Task<T> ExecuteRequestAsync<T>(string url)
        {
            using (var reader = new JsonTextReader(new StreamReader(
                await ExecuteRequestAsync(url))))
            {
                return DefaultSerializer.Deserialize<T>(reader);
            }
        }

        private async Task<Stream> ExecuteRequestAsync(string url)
        {
            return await HttpClient.GetStreamAsync(url);
        }
    }
}
