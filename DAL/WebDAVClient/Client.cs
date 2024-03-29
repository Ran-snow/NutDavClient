using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Text;
using System.Threading.Tasks;
using WebDAVClient.Helpers;
using WebDAVClient.HttpClient;
using WebDAVClient.Model;

namespace WebDAVClient
{
    public class Client : IClient
    {
        private static readonly HttpMethod PropFind = new HttpMethod("PROPFIND");
        private static readonly HttpMethod MoveMethod = new HttpMethod("MOVE");
        private static readonly HttpMethod CopyMethod = new HttpMethod("COPY");

        private static readonly HttpMethod MkCol = new HttpMethod(WebRequestMethods.Http.MkCol);

        private const int HttpStatusCode_MultiStatus = 207;

        // http://webdav.org/specs/rfc4918.html#METHOD_PROPFIND
        private const string PropFindRequestContent =
            "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" +
            "<propfind xmlns=\"DAV:\">" +
            "<allprop/>" +
            //"  <propname/>" +
            //"  <prop>" +
            //"    <creationdate/>" +
            //"    <getlastmodified/>" +
            //"    <displayname/>" +
            //"    <getcontentlength/>" +
            //"    <getcontenttype/>" +
            //"    <getetag/>" +
            //"    <resourcetype/>" +
            //"  </prop> " +
            "</propfind>";

        private static readonly string AssemblyVersion = typeof(IClient).Assembly.GetName().Version.ToString();

        private readonly IHttpClientWrapper _httpClientWrapper;
        private string _server;
        private string _encodedBasePath;

        public event Action<int, double, long> OnProgressHandler;

        #region WebDAV connection parameters

        /// <summary>
        /// Specify the WebDAV hostname (required).
        /// </summary>
        public string Server
        {
            get { return _server; }
        }

        public void SetServer(string server)
        {
            _server = server.TrimEnd('/');
        }

        /// <summary>
        /// Specify the path of a WebDAV directory to use as 'root' (default: /)
        /// </summary>
        public string BasePath { get; private set; } = "/";

        /// <summary>
        /// Specify the path of a WebDAV directory to use as 'root' (default: /)
        /// </summary>
        /// <param name="basePath"></param>
        public async Task SetBasePath(string basePath)
        {
            basePath = basePath.Trim('/');
            if (string.IsNullOrEmpty(basePath))
            {
                BasePath = "/";
            }
            else
            {
                BasePath = "/" + basePath + "/";
            }

            if (string.IsNullOrEmpty(Server))
            {
                throw new WebDAVException("Please set server before basePath");
            }

            if (Port == null)
            {
                SetPort(Server.StartsWith("https") ? 443 : 80);
            }

            await SetEncodedBasePath(isForce: true);
        }

        /// <summary>
        /// Specify an port (default: null = auto-detect)
        /// </summary>
        public int? Port { get; private set; }

        /// <summary>
        /// Specify an port (default: null = auto-detect)
        /// </summary>
        /// <param name="port"></param>
        public void SetPort(int port)
        {
            Port = port;
        }

        /// <summary>
        /// Specify the UserAgent (and UserAgent version) string to use in requests
        /// </summary>
        public string UserAgent { get; set; }

        /// <summary>
        /// Specify the UserAgent (and UserAgent version) string to use in requests
        /// </summary>
        public string UserAgentVersion { get; set; }

        /// <summary>
        /// Specify additional headers to be sent with every request
        /// </summary>
        public ICollection<KeyValuePair<string, string>> CustomHeaders { get; set; }

        /// <summary>
        /// Specify the certificates validation logic
        /// </summary>
        public RemoteCertificateValidationCallback ServerCertificateValidationCallback { get; set; }
        #endregion

        public Client(NetworkCredential credential = null, TimeSpan? uploadTimeout = null, IWebProxy proxy = null)
        {
            var handler = new HttpClientHandler();
            if (proxy != null && handler.SupportsProxy)
                handler.Proxy = proxy;
            if (handler.SupportsAutomaticDecompression)
                handler.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            if (credential != null)
            {
                handler.Credentials = credential;
                handler.PreAuthenticate = true;
            }
            else
            {
                handler.UseDefaultCredentials = true;
            }

            var client = new System.Net.Http.HttpClient(handler);
            client.DefaultRequestHeaders.ExpectContinue = false;

            System.Net.Http.HttpClient uploadClient = null;
            if (uploadTimeout != null)
            {
                uploadClient = new System.Net.Http.HttpClient(handler);
                uploadClient.DefaultRequestHeaders.ExpectContinue = false;
                uploadClient.Timeout = uploadTimeout.Value;
            }

            _httpClientWrapper = new HttpClientWrapper(client, uploadClient ?? client);
        }

        public Client(IHttpClientWrapper httpClientWrapper)
        {
            _httpClientWrapper = httpClientWrapper;
        }

        #region WebDAV operations

        /// <summary>
        /// List all files present on the server.
        /// </summary>
        /// <param name="path">List only files in this path</param>
        /// <param name="depth">Recursion depth</param>
        /// <returns>A list of files (entries without a trailing slash) and directories (entries with a trailing slash)</returns>
        public async Task<IEnumerable<Item>> List(string path = "/", int? depth = 1)
        {
            var listUri = await GetServerUrl(path, true).ConfigureAwait(false);

            // Depth header: http://webdav.org/specs/rfc4918.html#rfc.section.9.1.4
            IDictionary<string, string> headers = new Dictionary<string, string>();
            if (depth != null)
            {
                headers.Add("Depth", depth.ToString());
            }

            if (CustomHeaders != null)
            {
                foreach (var keyValuePair in CustomHeaders)
                {
                    headers.Add(keyValuePair);
                }
            }

            HttpResponseMessage response = null;

            try
            {
                response = await HttpRequest(listUri.Uri, PropFind, headers, Encoding.UTF8.GetBytes(PropFindRequestContent)).ConfigureAwait(false);

                if (response.StatusCode != HttpStatusCode.OK &&
                    (int)response.StatusCode != HttpStatusCode_MultiStatus)
                {
                    throw new WebDAVException((int)response.StatusCode, "Failed retrieving items in folder.");
                }

                using (var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                {
                    var items = ResponseParser.ParseItems(stream);

                    if (items == null)
                    {
                        throw new WebDAVException("Failed deserializing data returned from server.");
                    }

                    var listUrl = listUri.ToString();

                    var result = new List<Item>(items.Count());
                    foreach (var item in items)
                    {
                        // If it's not a collection, add it to the result
                        if (!item.IsCollection)
                        {
                            result.Add(item);
                        }
                        else
                        {
                            // If it's not the requested parent folder, add it to the result
                            var fullHref = await GetServerUrl(item.Href, true).ConfigureAwait(false);
                            if (!string.Equals(fullHref.ToString(), listUrl, StringComparison.CurrentCultureIgnoreCase))
                            {
                                result.Add(item);
                            }
                        }
                    }
                    return result;
                }

            }
            finally
            {
                if (response != null)
                    response.Dispose();
            }
        }

        /// <summary>
        /// List all files present on the server.
        /// </summary>
        /// <returns>A list of files (entries without a trailing slash) and directories (entries with a trailing slash)</returns>
        public async Task<Item> GetFolder(string path = "/")
        {
            var listUri = await GetServerUrl(path, true).ConfigureAwait(false);
            return await Get(listUri.Uri).ConfigureAwait(false);
        }

        /// <summary>
        /// List all files present on the server.
        /// </summary>
        /// <returns>A list of files (entries without a trailing slash) and directories (entries with a trailing slash)</returns>
        public async Task<Item> GetFile(string path = "/")
        {
            var listUri = await GetServerUrl(path, false).ConfigureAwait(false);
            return await Get(listUri.Uri).ConfigureAwait(false);
        }

        /// <summary>
        /// List all files present on the server.
        /// </summary>
        /// <returns>A list of files (entries without a trailing slash) and directories (entries with a trailing slash)</returns>
        private async Task<Item> Get(Uri listUri)
        {

            // Depth header: http://webdav.org/specs/rfc4918.html#rfc.section.9.1.4
            IDictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("Depth", "0");

            if (CustomHeaders != null)
            {
                foreach (var keyValuePair in CustomHeaders)
                {
                    headers.Add(keyValuePair);
                }
            }


            HttpResponseMessage response = null;

            try
            {
                response = await HttpRequest(listUri, PropFind, headers, Encoding.UTF8.GetBytes(PropFindRequestContent)).ConfigureAwait(false);

                if (response.StatusCode != HttpStatusCode.OK &&
                    (int)response.StatusCode != HttpStatusCode_MultiStatus)
                {
                    throw new WebDAVException((int)response.StatusCode, string.Format("Failed retrieving item/folder (Status Code: {0})", response.StatusCode));
                }

                using (var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                {
                    var result = ResponseParser.ParseItem(stream);

                    if (result == null)
                    {
                        throw new WebDAVException("Failed deserializing data returned from server.");
                    }

                    return result;
                }
            }
            finally
            {
                if (response != null)
                    response.Dispose();
            }
        }

        /// <summary>
        /// Download a file from the server
        /// </summary>
        /// <param name="remoteFilePath">Source path and filename of the file on the server</param>
        public async Task<Stream> Download(string remoteFilePath)
        {
            var headers = new Dictionary<string, string> { { "translate", "f" } };
            if (CustomHeaders != null)
            {
                foreach (var keyValuePair in CustomHeaders)
                {
                    headers.Add(keyValuePair.Key, keyValuePair.Value);
                }
            }
            return await DownloadFile(remoteFilePath, headers);
        }

        /// <summary>
        /// Download a part of file from the server
        /// </summary>
        /// <param name="remoteFilePath">Source path and filename of the file on the server</param>
        /// <param name="startBytes">Start bytes of content</param>
        /// <param name="endBytes">End bytes of content</param>
        public async Task<Stream> DownloadPartial(string remoteFilePath, long startBytes, long endBytes)
        {
            var headers = new Dictionary<string, string> { { "translate", "f" }, { "Range", "bytes=" + startBytes + "-" + endBytes } };
            if (CustomHeaders != null)
            {
                foreach (var keyValuePair in CustomHeaders)
                {
                    headers.Add(keyValuePair.Key, keyValuePair.Value);
                }
            }
            return await DownloadFile(remoteFilePath, headers);
        }

        private async Task DownloadPartial(string id, TransmissionHelper transmissionHelper, string remoteFilePath, long startBytes, long endBytes, long step)
        {
            var headers = new Dictionary<string, string> { { "translate", "f" }, { "Range", "bytes=" + startBytes + "-" + endBytes } };
            if (CustomHeaders != null)
            {
                foreach (var keyValuePair in CustomHeaders)
                {
                    headers.Add(keyValuePair.Key, keyValuePair.Value);
                }
            }

            using (FileStream file = new FileStream($"{id}.dat", FileMode.Create))
            using (var stream = await DownloadFile(remoteFilePath, headers))
            {
                var length = endBytes - startBytes + 1;
                var consumed = 0;
                var buffer = new byte[step];
                while (consumed < length)
                {
                    var read = await stream.ReadAsync(buffer, 0, buffer.Length);
                    await file.WriteAsync(buffer, 0, read);
                    consumed += read;
                    transmissionHelper.IncreaseRealTimeLength(read);
                }
            }
        }

        /// <summary>
        /// MutilateThread Download
        /// </summary>
        /// <param name="remoteFile">file url</param>
        /// <param name="localFileDirectory">local dic</param>
        /// <param name="blockLength">N MB</param>
        /// <returns></returns>
        public async Task DownloadCrazy(Item remoteFile, DirectoryInfo localFileDirectory, int blockLength = 1)
        {
            TransmissionHelper transmissionHelper = new TransmissionHelper();
            transmissionHelper.OnProgressHandler += (timeCost, plannedSpeed, downloadSpeed) =>
            {
                this.OnProgressHandler?.Invoke(timeCost, plannedSpeed, downloadSpeed);
            };

            try
            {
                if (remoteFile == null || remoteFile.IsCollection || remoteFile.ContentLength == null)
                {
                    throw new WebDAVException("remoteFile is nil or collection");
                }

                long? contentLength = remoteFile.ContentLength.Value;

                if (contentLength == null)
                {
                    throw new WebDAVException("Response content is nil");
                }
                else
                {
                    transmissionHelper.SetTotalLength(remoteFile.ContentLength.Value);
                }

                List<Task> tasks = new List<Task>();
                long step = (1L << 20) * blockLength; // 2^10 * 2^10 == 1MB
                var blockCount = Math.Ceiling(remoteFile.ContentLength.Value * 1.0 / step);
                string id = Guid.NewGuid().ToString("N");

                transmissionHelper.Start();

                Console.WriteLine($"Start download ! total length{remoteFile.ContentLength.Value}");
                for (int i = 0; i < blockCount; i++)
                {
                    long start = i * step;
                    long end = (i + 1) * step - 1;
                    if ((remoteFile.ContentLength.Value - 1) < end)
                    {
                        end = remoteFile.ContentLength.Value - 1;
                    }

                    tasks.Add(DownloadPartial(id + i, transmissionHelper, remoteFile.Href, start, end, step));
                }

                await Task.Run(() => Task.WaitAll(tasks.ToArray()));
                transmissionHelper.Stop();

                Console.WriteLine("Files is merging ....");
                using (FileStream fileOut =
                    new FileStream(Path.Combine(localFileDirectory.FullName.TrimEnd('/') + "/" + remoteFile.DisplayName.TrimStart('/')), FileMode.Create))
                {
                    for (int i = 0; i < blockCount; i++)
                    {
                        using (FileStream fileStream = new FileStream($"{id + i}.dat", FileMode.Open))
                        {
                            await fileStream.CopyToAsync(fileOut);
                        }
                        File.Delete($"{id + i}.dat");
                        Console.WriteLine($"{i} is OK");
                    }
                }

                Console.WriteLine($"End download ! Enjoy yourself !");
            }
            catch (Exception ex)
            {
                transmissionHelper.Stop();
                throw ex;
            }
        }

        /// <summary>
        /// Upload a file to the server
        /// </summary>
        /// <param name="remoteFilePath">Source path and filename of the file on the server</param>
        /// <param name="content"></param>
        /// <param name="name"></param>
        public async Task<bool> Upload(string remoteFilePath, Stream content, string name)
        {
            // Should not have a trailing slash.
            var uploadUri = await GetServerUrl(remoteFilePath.TrimEnd('/') + "/" + name.TrimStart('/'), false).ConfigureAwait(false);

            IDictionary<string, string> headers = new Dictionary<string, string>();
            if (CustomHeaders != null)
            {
                foreach (var keyValuePair in CustomHeaders)
                {
                    headers.Add(keyValuePair);
                }
            }

            HttpResponseMessage response = null;

            try
            {
                response = await HttpUploadRequest(uploadUri.Uri, HttpMethod.Put, content, headers).ConfigureAwait(false);

                if (response.StatusCode != HttpStatusCode.OK &&
                    response.StatusCode != HttpStatusCode.NoContent &&
                    response.StatusCode != HttpStatusCode.Created)
                {
                    throw new WebDAVException((int)response.StatusCode, "Failed uploading file.");
                }

                return response.IsSuccessStatusCode;
            }
            finally
            {
                if (response != null) response.Dispose();
            }
        }

        public async Task UploadCrazy(string localFile, string remoteFileDirectory, int blockLength = 1)
        {
            TransmissionHelper transmissionHelper = new TransmissionHelper();
            transmissionHelper.OnProgressHandler += (timeCost, plannedSpeed, downloadSpeed) =>
            {
                this.OnProgressHandler?.Invoke(timeCost, plannedSpeed, downloadSpeed);
            };

            try
            {
                if (!File.Exists(localFile))
                {
                    throw new WebDAVException($"{localFile} is not file.");
                }

                FileInfo fileInfo = new FileInfo(localFile);
                using (Stream stream = File.OpenRead(localFile))
                {

                    transmissionHelper.SetTotalLength(fileInfo.Length);

                    List<Task> tasks = new List<Task>();
                    long step = (1L << 20) * blockLength; // 2^10 * 2^10 == 1MB
                    var blockCount = Math.Ceiling(fileInfo.Length * 1.0 / step);
                    string id = Guid.NewGuid().ToString("N");

                    transmissionHelper.Start();

                    Console.WriteLine($"Start upload ! total length{fileInfo.Length}");
                    for (int i = 0; i < blockCount; i++)
                    {
                        long start = i * step;
                        long end = (i + 1) * step - 1;
                        if ((fileInfo.Length - 1) < end)
                        {
                            end = fileInfo.Length - 1;
                        }

                        int stepStream = (int)(end - start + 1);
                        byte[] buffer = new byte[stepStream];

                        stream.Seek(start, SeekOrigin.Begin);
                        await stream.ReadAsync(buffer, 0, stepStream);

                        tasks.Add(UploadPartial(transmissionHelper, new MemoryStream(buffer), remoteFileDirectory, start, end, stream.Length));
                    }

                    await Task.Run(() => Task.WaitAll(tasks.ToArray()));
                }

                transmissionHelper.Stop();

                Console.WriteLine($"End uplaod ! Enjoy yourself !");
            }
            catch (Exception ex)
            {
                transmissionHelper.Stop();
                throw ex;
            }
        }

        private async Task UploadPartial(TransmissionHelper transmissionHelper, Stream content, string remoteFileName, long startBytes, long endBytes, long totalLength)
        {
            Console.WriteLine($"startBytes:{startBytes}  endBytes:{endBytes}");

            // Should not have a trailing slash.
            var uploadUri = await GetServerUrl(remoteFileName, false).ConfigureAwait(false);

            HttpResponseMessage response = null;

            try
            {
                Console.WriteLine("current stream length:" + content.Length);
                response = await HttpUploadRequest(uploadUri.Uri, HttpMethod.Put, content, null, startBytes, endBytes, totalLength).ConfigureAwait(false);

                if (response.StatusCode != HttpStatusCode.OK &&
                response.StatusCode != HttpStatusCode.NoContent &&
                response.StatusCode != HttpStatusCode.Created)
                {
                    throw new WebDAVException((int)response.StatusCode, "Failed uploading file.");
                }

                if (response.IsSuccessStatusCode)
                {
                    transmissionHelper.IncreaseRealTimeLength(endBytes - startBytes + 1);
                }
            }
            finally
            {
                if (response != null)
                {
                    response.Dispose();
                }

                content.Close();
            }
        }

        /// <summary>
        /// Partial upload a part of file to the server
        /// </summary>
        /// <param name="remoteFilePath">Source path and filename of the file on the server</param>
        /// <param name="content">Partial content to update</param>
        /// <param name="name">Name of the file to update</param>
        /// <param name="startBytes">Start byte position of the target content</param>
        /// <param name="endBytes">End bytes of the target content. Must match the length of <paramref name="content"/> plus <paramref name="startBytes"/></param>
        public async Task<bool> UploadPartial(string remoteFilePath, Stream content, string name, long startBytes, long endBytes)
        {
            if (startBytes + content.Length != endBytes)
            {
                throw new InvalidOperationException("The length of the given content plus the startBytes must match the endBytes.");
            }

            // Should not have a trailing slash.
            var uploadUri = await GetServerUrl(remoteFilePath.TrimEnd('/') + "/" + name.TrimStart('/'), false).ConfigureAwait(false);

            HttpResponseMessage response = null;

            try
            {
                response = await HttpUploadRequest(uploadUri.Uri, HttpMethod.Put, content, null, startBytes, endBytes).ConfigureAwait(false);

                if (response.StatusCode != HttpStatusCode.OK &&
                    response.StatusCode != HttpStatusCode.NoContent &&
                    response.StatusCode != HttpStatusCode.Created)
                {
                    throw new WebDAVException((int)response.StatusCode, "Failed uploading file.");
                }

                return response.IsSuccessStatusCode;
            }
            finally
            {
                if (response != null)
                    response.Dispose();
            }
        }

        /// <summary>
        /// Create a directory on the server
        /// </summary>
        /// <param name="remotePath">Destination path of the directory on the server</param>
        /// <param name="name"></param>
        public async Task<bool> CreateDir(string remotePath, string name)
        {
            // Should not have a trailing slash.
            var dirUri = await GetServerUrl(remotePath.TrimEnd('/') + "/" + name.TrimStart('/'), false).ConfigureAwait(false);

            IDictionary<string, string> headers = new Dictionary<string, string>();
            if (CustomHeaders != null)
            {
                foreach (var keyValuePair in CustomHeaders)
                {
                    headers.Add(keyValuePair);
                }
            }

            HttpResponseMessage response = null;

            try
            {
                response = await HttpRequest(dirUri.Uri, MkCol, headers).ConfigureAwait(false);

                if (response.StatusCode == HttpStatusCode.Conflict)
                    throw new WebDAVConflictException((int)response.StatusCode, "Failed creating folder.");

                if (response.StatusCode != HttpStatusCode.OK &&
                    response.StatusCode != HttpStatusCode.NoContent &&
                    response.StatusCode != HttpStatusCode.Created)
                {
                    throw new WebDAVException((int)response.StatusCode, "Failed creating folder.");
                }

                return response.IsSuccessStatusCode;
            }
            finally
            {
                if (response != null)
                    response.Dispose();
            }
        }

        public async Task DeleteFolder(string href)
        {
            var listUri = await GetServerUrl(href, true).ConfigureAwait(false);
            await Delete(listUri.Uri).ConfigureAwait(false);
        }

        public async Task DeleteFile(string href)
        {
            var listUri = await GetServerUrl(href, false).ConfigureAwait(false);
            await Delete(listUri.Uri).ConfigureAwait(false);
        }

        private async Task Delete(Uri listUri)
        {
            IDictionary<string, string> headers = new Dictionary<string, string>();
            if (CustomHeaders != null)
            {
                foreach (var keyValuePair in CustomHeaders)
                {
                    headers.Add(keyValuePair);
                }
            }

            var response = await HttpRequest(listUri, HttpMethod.Delete, headers).ConfigureAwait(false);

            if (response.StatusCode != HttpStatusCode.OK &&
                response.StatusCode != HttpStatusCode.NoContent)
            {
                throw new WebDAVException((int)response.StatusCode, "Failed deleting item.");
            }
        }

        public async Task<bool> MoveFolder(string srcFolderPath, string dstFolderPath)
        {
            // Should have a trailing slash.
            var srcUri = await GetServerUrl(srcFolderPath, true).ConfigureAwait(false);
            var dstUri = await GetServerUrl(dstFolderPath, true).ConfigureAwait(false);

            return await Move(srcUri.Uri, dstUri.Uri).ConfigureAwait(false);
        }

        public async Task<bool> MoveFile(string srcFilePath, string dstFilePath)
        {
            // Should not have a trailing slash.
            var srcUri = await GetServerUrl(srcFilePath, false).ConfigureAwait(false);
            var dstUri = await GetServerUrl(dstFilePath, false).ConfigureAwait(false);

            return await Move(srcUri.Uri, dstUri.Uri).ConfigureAwait(false);
        }

        public async Task<bool> CopyFile(string srcFilePath, string dstFilePath)
        {
            // Should not have a trailing slash.
            var srcUri = await GetServerUrl(srcFilePath, false).ConfigureAwait(false);
            var dstUri = await GetServerUrl(dstFilePath, false).ConfigureAwait(false);

            return await Copy(srcUri.Uri, dstUri.Uri).ConfigureAwait(false);
        }

        public async Task<bool> CopyFolder(string srcFolderPath, string dstFolderPath)
        {
            // Should have a trailing slash.
            var srcUri = await GetServerUrl(srcFolderPath, true).ConfigureAwait(false);
            var dstUri = await GetServerUrl(dstFolderPath, true).ConfigureAwait(false);

            return await Copy(srcUri.Uri, dstUri.Uri).ConfigureAwait(false);
        }

        private async Task<bool> Move(Uri srcUri, Uri dstUri)
        {
            const string requestContent = "MOVE";

            IDictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("Destination", dstUri.ToString());

            if (CustomHeaders != null)
            {
                foreach (var keyValuePair in CustomHeaders)
                {
                    headers.Add(keyValuePair);
                }
            }

            var response = await HttpRequest(srcUri, MoveMethod, headers, Encoding.UTF8.GetBytes(requestContent)).ConfigureAwait(false);

            if (response.StatusCode != HttpStatusCode.OK &&
                response.StatusCode != HttpStatusCode.Created)
            {
                throw new WebDAVException((int)response.StatusCode, "Failed moving file.");
            }

            return response.IsSuccessStatusCode;
        }

        private async Task<bool> Copy(Uri srcUri, Uri dstUri)
        {
            const string requestContent = "COPY";

            IDictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("Destination", dstUri.ToString());

            if (CustomHeaders != null)
            {
                foreach (var keyValuePair in CustomHeaders)
                {
                    headers.Add(keyValuePair);
                }
            }

            var response = await HttpRequest(srcUri, CopyMethod, headers, Encoding.UTF8.GetBytes(requestContent)).ConfigureAwait(false);

            if (response.StatusCode != HttpStatusCode.OK &&
                response.StatusCode != HttpStatusCode.Created)
            {
                throw new WebDAVException((int)response.StatusCode, "Failed copying file.");
            }

            return response.IsSuccessStatusCode;
        }

        private async Task<Stream> DownloadFile(String remoteFilePath, Dictionary<string, string> header)
        {
            // Should not have a trailing slash.
            var downloadUri = await GetServerUrl(remoteFilePath, false).ConfigureAwait(false);
            var response = await HttpRequest(downloadUri.Uri, HttpMethod.Get, header).ConfigureAwait(false);
            //using (var response = await HttpRequest(downloadUri.Uri, HttpMethod.Get, header).ConfigureAwait(false))
            //{
            if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.PartialContent)
            {
                return await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            }
            throw new WebDAVException((int)response.StatusCode, "Failed retrieving file.");
            //}
        }

        #endregion

        #region Server communication

        /// <summary>
        /// Perform the WebDAV call and fire the callback when finished.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="method"></param>
        /// <param name="headers"></param>
        /// <param name="content"></param>
        private async Task<HttpResponseMessage> HttpRequest(Uri uri, HttpMethod method, IDictionary<string, string> headers = null, byte[] content = null)
        {
            using (var request = new HttpRequestMessage(method, uri))
            {
                request.Headers.Connection.Add("Keep-Alive");
                if (!string.IsNullOrWhiteSpace(UserAgent))
                    request.Headers.UserAgent.Add(new ProductInfoHeaderValue(UserAgent, UserAgentVersion));
                else
                    request.Headers.UserAgent.Add(new ProductInfoHeaderValue("WebDAVClient", AssemblyVersion));

                if (headers != null)
                {
                    foreach (string key in headers.Keys)
                    {
                        request.Headers.Add(key, headers[key]);
                    }
                }

                // Need to send along content?
                if (content != null)
                {
                    request.Content = new ByteArrayContent(content);
                    request.Content.Headers.ContentType = new MediaTypeHeaderValue("text/xml");
                }

                return await _httpClientWrapper.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Perform the WebDAV call and fire the callback when finished.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="headers"></param>
        /// <param name="method"></param>
        /// <param name="content"></param>
        private async Task<HttpResponseMessage> HttpUploadRequest(Uri uri, HttpMethod method, Stream content, IDictionary<string, string> headers = null, long? startbytes = null, long? endbytes = null, long? totalLength = null)
        {
            using (var request = new HttpRequestMessage(method, uri))
            {
                request.Headers.Connection.Add("Keep-Alive");
                if (!string.IsNullOrWhiteSpace(UserAgent))
                    request.Headers.UserAgent.Add(new ProductInfoHeaderValue(UserAgent, UserAgentVersion));
                else
                    request.Headers.UserAgent.Add(new ProductInfoHeaderValue("WebDAVClient", AssemblyVersion));

                if (headers != null)
                {
                    foreach (string key in headers.Keys)
                    {
                        request.Headers.Add(key, headers[key]);
                    }
                }

                // Need to send along content?
                if (content != null)
                {
                    request.Content = new StreamContent(content);
                    if (startbytes.HasValue && endbytes.HasValue)
                    {
                        var len = totalLength.HasValue ? totalLength.ToString() : "*";
                        request.Content.Headers.ContentRange = ContentRangeHeaderValue.Parse($"bytes {startbytes}-{endbytes}/{len}");
                        request.Content.Headers.ContentLength = endbytes - startbytes + 1;
                    }
                }

                return await _httpClientWrapper.SendUploadAsync(request).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Try to create an Uri with kind UriKind.Absolute
        /// This particular implementation also works on Mono/Linux 
        /// It seems that on Mono it is expected behaviour that uris
        /// of kind /a/b are indeed absolute uris since it referes to a file in /a/b. 
        /// https://bugzilla.xamarin.com/show_bug.cgi?id=30854
        /// </summary>
        /// <param name="uriString"></param>
        /// <param name="uriResult"></param>
        /// <returns></returns>
        private static bool TryCreateAbsolute(string uriString, out Uri uriResult)
        {
            return Uri.TryCreate(uriString, UriKind.Absolute, out uriResult) && uriResult.Scheme != Uri.UriSchemeFile;
        }

        private async Task<UriBuilder> GetServerUrl(string path, bool appendTrailingSlash)
        {
            await SetEncodedBasePath();

            // If we've been asked for the "root" folder
            if (string.IsNullOrEmpty(path))
            {
                // If the resolved base path is an absolute URI, use it
                Uri absoluteBaseUri;
                if (TryCreateAbsolute(_encodedBasePath, out absoluteBaseUri))
                {
                    return new UriBuilder(absoluteBaseUri);
                }

                // Otherwise, use the resolved base path relatively to the server
                var baseUri = new UriBuilder(_server) { Path = _encodedBasePath, Port = (int)Port };
                return baseUri;
            }

            // If the requested path is absolute, use it
            Uri absoluteUri;
            if (TryCreateAbsolute(path, out absoluteUri))
            {
                var baseUri = new UriBuilder(absoluteUri);
                return baseUri;
            }
            else
            {
                // Otherwise, create a URI relative to the server
                UriBuilder baseUri;
                if (TryCreateAbsolute(_encodedBasePath, out absoluteUri))
                {
                    baseUri = new UriBuilder(absoluteUri);

                    baseUri.Path = baseUri.Path.TrimEnd('/') + "/" + path.TrimStart('/');

                    if (appendTrailingSlash && !baseUri.Path.EndsWith("/"))
                        baseUri.Path += "/";
                }
                else
                {
                    baseUri = new UriBuilder(_server) { Port = (int)Port };

                    // Ensure we don't add the base path twice
                    var finalPath = path;
                    if (!finalPath.StartsWith(_encodedBasePath, StringComparison.InvariantCultureIgnoreCase))
                    {
                        finalPath = _encodedBasePath.TrimEnd('/') + "/" + path.Trim('/');
                    }
                    if (appendTrailingSlash)
                        finalPath = finalPath.TrimEnd('/') + "/";

                    baseUri.Path = finalPath;
                }


                return baseUri;
            }
        }

        private async Task SetEncodedBasePath(bool isForce = false)
        {
            // Resolve the base path on the server
            if (_encodedBasePath == null || isForce)
            {
                var baseUri = new UriBuilder(_server) { Path = BasePath, Port = Port.Value };
                var root = await Get(baseUri.Uri).ConfigureAwait(false);

                _encodedBasePath = root.Href;
            }
        }

        #endregion

        #region WebDAV Connection Helpers

        public bool ServerCertificateValidation(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certification, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            if (ServerCertificateValidationCallback != null)
            {
                return ServerCertificateValidationCallback(sender, certification, chain, sslPolicyErrors);
            }
            return false;
        }

        #endregion
    }
}
