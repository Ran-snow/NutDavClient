using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace DAL
{
    public static class HTTPHelper
    {
        public async static Task CrazyDowmload(string uri)
        {
            using var httpClient = new HttpClient();
            using var request = new HttpRequestMessage
            {
                RequestUri = new Uri(uri),
                Method = HttpMethod.Head,
            };

            var response = await httpClient.SendAsync(request).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            long? contentLength = response.Content.Headers.ContentLength;

            if (contentLength == null)
            {
                return;
            }

            Console.WriteLine($"contentLength：{contentLength.Value}");
            Console.WriteLine(response.Headers.ETag.Tag);

            List<Task> tasks = new List<Task>();
            long step = 1L << 20;
            step *= 10;

            var blockCount = Math.Ceiling(contentLength.Value * 1.0 / step);

            for (int i = 0; i < blockCount; i++)
            {
                long start = i * step;
                long end = (i + 1) * step - 1;
                if ((contentLength.Value - 1) < end)
                {
                    end = contentLength.Value - 1;
                }

                tasks.Add(Down1oadBlockAsnyc(i, uri, step, start, end));
            }

            Task.WaitAll(tasks.ToArray());

            using FileStream fileOut = new FileStream("666.exe", FileMode.Create);

            int b;
            for (int id = 0; id < blockCount; id++)
            {
                using FileStream fileStream = new FileStream($"{id}.dat", FileMode.Open);

                while ((b = fileStream.ReadByte()) != -1)
                {
                    fileOut.WriteByte((byte)b);
                }
            }

            for (int id = 0; id < blockCount; id++)
            {
                File.Delete($"{id}.dat");
            }

            Console.WriteLine(contentLength);
        }

        private async static Task Down1oadBlockAsnyc(int id, string uri, long step, long start, long end)
        {
            await using FileStream file = new FileStream($"{id}.dat", FileMode.Create);

            using var httpClient = new HttpClient();
            using var request = new HttpRequestMessage
            {
                RequestUri = new Uri(uri),
                Method = HttpMethod.Get,
                Headers =
                {
                    Range = new RangeHeaderValue(start,end)
                }
            };

            using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
            using var stream = await response.Content.ReadAsStreamAsync();
            var length = end - start + 1;
            var consumed = 0;
            var buffer = new byte[step];
            while (consumed < length)
            {
                var read = await stream.ReadAsync(buffer);
                await file.WriteAsync(buffer, 0, read);
                consumed += read;

                Console.WriteLine($"ID:{id} \t Thread:{Thread.CurrentThread.ManagedThreadId} \t {Math.Round(consumed * 1.0 / length, 2) * 100}%");
            }

            Console.WriteLine($"ID:{id} 完成");
        }
    }
}
