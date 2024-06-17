using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ClinicPlatformWebAPI.Helpers.Models
{
    public class HttpResponseModel : IHttpResponseModel<object>
    {
        public int StatusCode { get; set; }
        public string? Message { get; set; } = string.Empty;
        public string? Detail { get; set; } = string.Empty;
        public object? Content { get; set; } = null;

        public HttpResponseModel() { }
        public HttpResponseModel(object content)
        {
            Content = content;
        }

        override public string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
