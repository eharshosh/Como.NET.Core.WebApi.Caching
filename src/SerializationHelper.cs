using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;

namespace Como.WebApi.Caching
{
    public class SerializationHelper
    {
        private readonly IDictionary<string, OutputFormatter> _outputFormatters;

        public SerializationHelper(IOptions<MvcOptions> mvcOptions)
        {            
            _outputFormatters = new Dictionary<string, OutputFormatter>();
            foreach (var formatter in mvcOptions.Value.OutputFormatters.OfType<OutputFormatter>())
            {
                foreach (var mediaType in formatter.SupportedMediaTypes)
                {
                    _outputFormatters[mediaType] = formatter;
                }
            }            
        }

        private byte[] SerializeObjectToJson(object obj)
        {
            var jsonOutputFormatter = (JsonOutputFormatter) _outputFormatters[RawActionResult.JsonContentType];
            using (var outputStream = new MemoryStream())
            {
                using (var streamWriter = new StreamWriter(outputStream))
                {
                    jsonOutputFormatter.WriteObject(streamWriter, obj);
                }

                return outputStream.ToArray();
            }
        }

        public async Task<byte[]> Serialize(string outputContentType, object value)
        {
            if (!_outputFormatters.TryGetValue(outputContentType, out var formatter))            
            {
                throw new UnsupportedContentTypeException(
                    $"Output formatter for the content type '{outputContentType}' was not found or is not supported");
            }

            using (var outputStream = new MemoryStream())
            {
                var ctx = new DefaultHttpContext {Response = {Body = outputStream}};
                var cta = new OutputFormatterWriteContext(ctx, (stream, encoding) => new StreamWriter(stream, encoding),
                    value.GetType(), value);
                if (formatter is TextOutputFormatter tof)
                {
                    await tof.WriteResponseBodyAsync(cta, Encoding.UTF8);    
                }
                else
                {
                    await formatter.WriteResponseBodyAsync(cta);
                }
                
                var result = outputStream.ToArray();
                return result;
            }
        }

        public string ComputeHash(object target)
        {
            var serialized = SerializeObjectToJson(target);
            using (var sha1 = new SHA1Managed())
            {
                var hash = sha1.ComputeHash(serialized);
                var sb = new StringBuilder(hash.Length * 2);
                foreach (var b in hash)
                {
                    sb.Append(b.ToString("x2"));
                }

                return sb.ToString();
            }
        }
    }
}