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
        public bool Success { get; set; }
        public object? Content { get; set; } = null;


        public HttpResponseModel() { }
        public HttpResponseModel(int StatusCode, string? Message, bool Success, object? Content)
        {
            this.StatusCode = StatusCode;
            this.Message = Message;
            this.Success = Success;
            this.Content = Content;
        }

        override public string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
