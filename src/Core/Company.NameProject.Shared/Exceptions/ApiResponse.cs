using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Company.NameProject.Shared.Exceptions
{
    public class ApiResponse<T>
    {
        public string Token { get; set; }
        public int StatusCode { get; set; }
        public List<string> Messages { get; set; } = new();
        public T Data { get; set; }

        // ✅ SUCCESS
        public static ApiResponse<T> Success(T data, string message = "OK", int statusCode = 200)
        {
            return new ApiResponse<T>
            {
                Token = Guid.NewGuid().ToString(),
                StatusCode = statusCode,
                Messages = new List<string> { message },
                Data = data
            };
        }

        // ❌ FAIL (un mensaje)
        public static ApiResponse<T> Fail(string message, int statusCode = 400)
        {
            return new ApiResponse<T>
            {
                Token = Guid.NewGuid().ToString(),
                StatusCode = statusCode,
                Messages = new List<string> { message },
                Data = default
            };
        }

        // ❌ FAIL (varios mensajes)
        public static ApiResponse<T> Fail(List<string> messages, int statusCode = 400)
        {
            return new ApiResponse<T>
            {
                Token = Guid.NewGuid().ToString(),
                StatusCode = statusCode,
                Messages = messages,
                Data = default
            };
        }
    }
}
