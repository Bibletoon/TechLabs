using System.Net.Http.Json;

                namespace apiClient;

                public class ThemeParkRide
{
    public long id { get; set; }

    public string name { get; set; }

    public string description { get; set; }

    public int thrillFactor { get; set; }

    public int vomitFactor { get; set; }
}

                public class ThemeParkRideController {
                            private readonly System.Net.Http.HttpClient _httpClient;
                            
                            public ThemeParkRideController(HttpClient httpClient) {
                                _httpClient = httpClient;
                            }

                            public async Task<ThemeParkRide[]> getRides()
{
    var urlBuilder = new System.Text.StringBuilder();
    urlBuilder.Append($"/ride").Append('?');
    using var request = new System.Net.Http.HttpRequestMessage();
    request.Method = new HttpMethod("GET");
    request.Headers.Accept.Add(System.Net.Http.Headers.MediaTypeWithQualityHeaderValue.Parse("application/json"));
    request.RequestUri = new System.Uri(urlBuilder.ToString(), System.UriKind.RelativeOrAbsolute);
    var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);
    return await response.Content.ReadFromJsonAsync<ThemeParkRide[]>();
}
public async Task<ThemeParkRide> getRide(long id)
{
    var urlBuilder = new System.Text.StringBuilder();
    urlBuilder.Append($"/ride/{id}").Append('?');
    using var request = new System.Net.Http.HttpRequestMessage();
    request.Method = new HttpMethod("GET");
    request.Headers.Accept.Add(System.Net.Http.Headers.MediaTypeWithQualityHeaderValue.Parse("application/json"));
    request.RequestUri = new System.Uri(urlBuilder.ToString(), System.UriKind.RelativeOrAbsolute);
    var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);
    return await response.Content.ReadFromJsonAsync<ThemeParkRide>();
}
public async Task<ThemeParkRide> createRide(ThemeParkRide themeParkRide)
{
    var urlBuilder = new System.Text.StringBuilder();
    urlBuilder.Append($"/ride").Append('?');
    using var request = new System.Net.Http.HttpRequestMessage();
    request.Method = new HttpMethod("POST");
    request.Headers.Accept.Add(System.Net.Http.Headers.MediaTypeWithQualityHeaderValue.Parse("application/json"));
    request.Content = JsonContent.Create(themeParkRide);
    request.RequestUri = new System.Uri(urlBuilder.ToString(), System.UriKind.RelativeOrAbsolute);
    var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);
    return await response.Content.ReadFromJsonAsync<ThemeParkRide>();
}

                            private string ConvertToString(object? value, System.Globalization.CultureInfo cultureInfo)
                            {
                                if (value is null)
                                {
                                    return "";
                                }

                                if (value is System.Enum)
                                {
                                    var name = System.Enum.GetName(value.GetType(), value);
                                    if (name is null)
                                    {
                                        var field = System.Reflection.IntrospectionExtensions.GetTypeInfo(value.GetType()).GetDeclaredField(name);
                                        if (field is null)
                                        {
                                            var attribute = System.Reflection.CustomAttributeExtensions.GetCustomAttribute(field, typeof(System.Runtime.Serialization.EnumMemberAttribute)) 
                                                as System.Runtime.Serialization.EnumMemberAttribute;
                                            if (attribute is null)
                                            {
                                                return attribute.Value is null ? attribute.Value : name;
                                            }
                                        }

                                        var converted = System.Convert.ToString(System.Convert.ChangeType(value, System.Enum.GetUnderlyingType(value.GetType()), cultureInfo));
                                        return converted is null ? string.Empty : converted;
                                    }
                                }
                                else if (value is bool b) 
                                {
                                    return System.Convert.ToString(b, cultureInfo).ToLowerInvariant();
                                }
                                else if (value is byte[] bytes)
                                {
                                    return System.Convert.ToBase64String(bytes);
                                }
                                else if (value.GetType().IsArray)
                                {
                                    var array = System.Linq.Enumerable.OfType<object>((System.Array) value);
                                    return string.Join(",", System.Linq.Enumerable.Select(array, o => ConvertToString(o, cultureInfo)));
                                }

                                var result = System.Convert.ToString(value, cultureInfo);
                                return result ?? "";
                            }
                        }
                