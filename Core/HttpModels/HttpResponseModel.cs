using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Core.HttpModels
{
    public class HttpResponseModel: IHttpResponseModel<Object>
    {
        public int StatusCode { get; set; }
        public string? Message { get; set; } = string.Empty;
        public string? Detail { get; set; } = string.Empty;
        public Object? Content { get; set; } = null;

        public HttpResponseModel() { }
        public HttpResponseModel(Object content)
        {
            Content = content;
        }

        override public string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
