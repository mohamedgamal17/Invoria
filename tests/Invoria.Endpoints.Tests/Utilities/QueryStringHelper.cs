using System.Reflection;
using System.Web;

namespace Invoria.Endpoints.Tests.Utilities
{
    public static class QueryStringHelper
    {
        public static string ToQueryString(object obj)
        {
            var properties = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            var query = HttpUtility.ParseQueryString(string.Empty);

            foreach (var prop in properties)
            {
                var value = prop.GetValue(obj);
                if (value != null)
                    query[prop.Name] = value.ToString();
            }

            return query.ToString();
        }
    }
}
