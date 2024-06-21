using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Core.HttpModels
{
    public interface IHttpResponseModel<T> where T : class
    {

        public int StatusCode { get; set; }
        public string? Message { get; set; }
        public string? Detail { get; set; }
        public T? Content { get; set; }
        public string ToString();
    }
}
