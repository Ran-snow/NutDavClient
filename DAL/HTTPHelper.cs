using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace DAL
{
    public class HTTPHelper
    {
        public event Action<string, int, double, long> OnProgressHandler;
        private static readonly object StaticLockObj = new object();
        private System.Timers.Timer timer;
        private string fileName;
        private long totalLength = 0;
        private long realTimeLength = 0;
        private long lastlLength = 0;
        private int timeCost = 0;

        public HTTPHelper()
        {
            timer = new System.Timers.Timer(1000);
            timer.Elapsed += Timer_Elapsed;
        }

        private void Reset()
        {
            fileName = string.Empty;
            totalLength = 0;
            realTimeLength = 0;
            lastlLength = 0;
            timeCost = 0;
            timer.Enabled = false;
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            double plannedSpeed = Math.Round(realTimeLength * 1.0 / totalLength, 2);

            long downloadSpeed = realTimeLength - lastlLength;
            lastlLength = realTimeLength;

            timeCost++;

            OnProgressHandler?.Invoke(fileName, timeCost, plannedSpeed, downloadSpeed);
        }

        public async Task CrazyDowmload(string uri, DirectoryInfo directoryInfo, int blockLength = 1)
        {
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(uri),
                Method = HttpMethod.Head,
            };

            var response = await GetHttpClient(new Uri(uri).Host).SendAsync(request).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            long? contentLength = response.Content.Headers.ContentLength;
            if (contentLength == null)
            {
                throw new InvalidDataException("Response content is nil");
            }
            else
            {
                totalLength = contentLength.Value;
            }
            if (response.Content.Headers.ContentType.MediaType != "application/x-msdownload")
            {
                throw new InvalidDataException($"Response content mediaType is {response.Content.Headers.ContentType.MediaType}, not application/x-msdownload");
            }

            List<Task> tasks = new List<Task>();
            long step = (1L << 20) * blockLength; // 2^10 * 2^10 == 1MB
            var blockCount = Math.Ceiling(contentLength.Value * 1.0 / step);
            fileName = GetFileName(response);
            timer.Enabled = true;
            Console.WriteLine($"Start download ! total length{contentLength.Value}");
            for (int i = 0; i < blockCount; i++)
            {
                long start = i * step;
                long end = (i + 1) * step - 1;
                if ((contentLength.Value - 1) < end)
                {
                    end = contentLength.Value - 1;
                }

                tasks.Add(Down1oadBlockAsnyc(i, uri, response.Headers.ETag?.Tag, step, start, end));
            }
            Task.WaitAll(tasks.ToArray());

            using (FileStream fileOut = new FileStream(Path.Combine(directoryInfo.FullName, fileName), FileMode.Create))
            {
                for (int id = 0; id < blockCount; id++)
                {
                    using (FileStream fileStream = new FileStream($"{id}.dat", FileMode.Open))
                    {
                        int b;
                        while ((b = fileStream.ReadByte()) != -1)
                        {
                            fileOut.WriteByte((byte)b);
                        }
                    }
                    File.Delete($"{id}.dat");
                }
            }

            Reset();
            Console.WriteLine($"End download ! Enjoy yourself !");
        }

        private async Task Down1oadBlockAsnyc(int id, string uri, string etag, long step, long start, long end)
        {
            using (FileStream file = new FileStream($"{id}.dat", FileMode.Create))
            {
                var request = new HttpRequestMessage
                {
                    RequestUri = new Uri(uri),
                    Method = HttpMethod.Get,
                    Headers =
                    {
                        Range = new RangeHeaderValue(start,end)
                    }
                };

                var response = await GetHttpClient(new Uri(uri).Host).SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
                if (response.Headers.ETag?.Tag != etag) throw new InvalidDataException("ETag is changed!");

                using (var stream = await response.Content.ReadAsStreamAsync())
                {
                    var length = end - start + 1;
                    var consumed = 0;
                    var buffer = new byte[step];
                    while (consumed < length)
                    {
                        var read = await stream.ReadAsync(buffer, 0, buffer.Length);
                        await file.WriteAsync(buffer, 0, read);
                        consumed += read;

                        lock (StaticLockObj)
                        {
                            realTimeLength += read;
                        }

                        Console.WriteLine($"ID:{id} \t Thread:{Thread.CurrentThread.ManagedThreadId} \t {Math.Round(100 * consumed * 1.0 / length, 2)}%");
                    }
                }
            }
        }

        private static string GetFileName(HttpResponseMessage response)
        {
            //1. Content-Disposition: attachment; filename="name"
            if (!string.IsNullOrEmpty(response.Content.Headers.ContentDisposition?.FileName))
            {
                return response.Content.Headers.ContentDisposition.FileName;
            }

            //2. URL
            string url = response.RequestMessage.RequestUri.AbsoluteUri;
            var files = url.Split('/');
            if (files != null && (files.Length - 1) > 0)
            {
                var ext = files[files.Length - 1].Split('.');
                if (ext != null && (ext.Length - 1) > 0)
                {
                    return files[files.Length - 1];
                }
            }

            //3. 301
            // ...

            return Guid.NewGuid().ToString("N");
        }

        public static HttpClient GetHttpClient(string uri)
        {
            ConcurrentDictionary<string, HttpClient> httpclients = new ConcurrentDictionary<string, HttpClient>();

            if (!httpclients.ContainsKey(uri))
            {
                httpclients.TryAdd(uri, new HttpClient());
            }

            return httpclients[uri];
        }
    }
}
