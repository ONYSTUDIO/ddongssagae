using System;
using System.Collections.Generic;
using System.Text;

namespace Helen
{
    public class UriQueryHelper
    {
        public static Dictionary<string, string> ParseQueryParameters(string url)
        {
            var result = new Dictionary<string, string>();
            if (string.IsNullOrEmpty(url))
                return result;

            var uri = new Uri(url);
            if (string.IsNullOrEmpty(uri.Query))
                return result;

            var queryBody = uri.Query.TrimStart('?');
            if (string.IsNullOrEmpty(queryBody))
                return result;

            var parameters = queryBody.Split('&');
            if (parameters == null || parameters.Length == 0)
                return result;

            foreach (var parameter in parameters)
            {
                var keyValuePair = parameter.Split('=');
                result[keyValuePair[0]] = keyValuePair[1];
            }

            return result;
        }
    }
}

namespace Helen
{
    public class UriQueryBuilder
    {
        private readonly string uri = string.Empty;

        private readonly Dictionary<string, string> queries =
            new Dictionary<string, string>();

        public UriQueryBuilder(string uri)
        {
            this.uri = uri;
        }

        public UriQueryBuilder AppendQueryParameter(string key, string value)
        {
            queries[key] = value;

            return this;
        }

        public string Build()
        {
            var builder = new StringBuilder().Append(uri);

            var addCount = 0;

            foreach (var query in queries)
            {
                if (string.IsNullOrEmpty(query.Key) ||
                    string.IsNullOrEmpty(query.Value))
                    continue;

                var seperator = (addCount == 0) ?
                    '?' : '&';

                builder.AppendFormat(
                    "{0}{1}={2}",
                    seperator, query.Key, query.Value);

                ++addCount;
            }

            return builder.ToString().Replace(" ", "%20");
        }

        public override string ToString()
        {
            return Build();
        }
    }
}
