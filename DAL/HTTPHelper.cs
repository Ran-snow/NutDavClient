using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace DAL
{
    public class HTTPHelper
    {
        private async Task Down1oadBlock(int id, long start, long end)
        {
            await using FileStream file = new FileStream("{id}.dat", FileMode.Create);

            using var httpCtient = new HttpClient();
            using var request = new HttpRequestMessage
            {
                RequestUri = new Uri("https://www.baidu.com"),
                Method = HttpMethod.Get,
                Headers =
                {
                    Range = new RangeHeaderValue (start,end)
                }
            };

            using var response = await httpCtient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            using var stream = await response.Content.ReadAsStreamAsync();
            var length = end - start + 1;
            var consumed = 0;
            var buffer = new byte[1024 * 100];
            while (consumed<length)
            {
                var read = await stream.ReadAsync(buffer);
                await file.WriteAsync(buffer, 0, read);
                consumed += read;
            }
        }
    }
}
  