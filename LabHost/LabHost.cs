// Mini host de ASP.NET (estilo Cassini) para ejecutar el MVC 5 bajo Wine/Sikarugir en macOS.
// IIS Express depende de http.sys, que Wine no implementa; este host usa sockets TCP
// y entrega cada solicitud al runtime real de ASP.NET (System.Web) de .NET Framework 4.8.
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Hosting;

namespace LabHost
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string dir = Path.GetFullPath(args.Length > 0 ? args[0] : ".");
            int port = args.Length > 1 ? int.Parse(args[1]) : 44300;
            string bin = Path.Combine(dir, "bin");
            Directory.CreateDirectory(bin);
            string self = typeof(Program).Assembly.Location;
            string target = Path.Combine(bin, Path.GetFileName(self));
            File.Copy(self, target, true);

            var host = (AspNetHost)ApplicationHost.CreateApplicationHost(typeof(AspNetHost), "/", dir);
            Console.WriteLine("LabHost escuchando en http://localhost:" + port + "/  (dir: " + dir + ")");
            host.Run(port);
        }
    }

    public class AspNetHost : MarshalByRefObject
    {
        public override object InitializeLifetimeService() { return null; }

        public void Run(int port)
        {
            ThreadPool.SetMinThreads(64, 64);
            var listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            while (true)
            {
                var client = listener.AcceptTcpClient();
                ThreadPool.QueueUserWorkItem(delegate { Handle(client); });
            }
        }

        private static void Handle(TcpClient client)
        {
            try
            {
                client.ReceiveTimeout = 15000;
                using (client)
                using (var stream = client.GetStream())
                {
                    var req = HttpRequestParser.Read(stream);
                    if (req == null) { Console.WriteLine("EMPTY-CONN"); return; }
                    Console.WriteLine("REQ " + req.Method + " " + req.RawUrl + " len=" + req.Body.Length);
                    var wr = new Worker(req, stream, client);
                    HttpRuntime.ProcessRequest(wr);
                }
            }
            catch (Exception ex) { Console.WriteLine("Error: " + ex); }
        }
    }

    public class ParsedRequest
    {
        public string Method, RawUrl, Path, Query, Version;
        public Dictionary<string, string> Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public byte[] Body = new byte[0];
    }

    public static class HttpRequestParser
    {
        public static ParsedRequest Read(Stream s)
        {
            var head = new MemoryStream();
            int state = 0, b;
            while (state < 4 && (b = s.ReadByte()) >= 0)
            {
                head.WriteByte((byte)b);
                if ((state % 2 == 0 && b == '\r') || (state % 2 == 1 && b == '\n')) state++; else state = (b == '\r') ? 1 : 0;
            }
            if (head.Length == 0) return null;
            var lines = Encoding.ASCII.GetString(head.ToArray()).Split(new[] { "\r\n" }, StringSplitOptions.None);
            var parts = lines[0].Split(' ');
            if (parts.Length < 3) return null;
            var r = new ParsedRequest { Method = parts[0], RawUrl = parts[1], Version = parts[2] };
            int q = r.RawUrl.IndexOf('?');
            r.Path = Uri.UnescapeDataString(q >= 0 ? r.RawUrl.Substring(0, q) : r.RawUrl);
            r.Query = q >= 0 ? r.RawUrl.Substring(q + 1) : "";
            for (int i = 1; i < lines.Length && lines[i].Length > 0; i++)
            {
                int c = lines[i].IndexOf(':');
                if (c > 0) r.Headers[lines[i].Substring(0, c).Trim()] = lines[i].Substring(c + 1).Trim();
            }
            string cl;
            if (r.Headers.TryGetValue("Content-Length", out cl))
            {
                int n = int.Parse(cl); var buf = new byte[n]; int got = 0;
                while (got < n) { int k = s.Read(buf, got, n - got); if (k <= 0) break; got += k; }
                r.Body = buf;
            }
            return r;
        }
    }

    public class Worker : HttpWorkerRequest
    {
        private readonly ParsedRequest _r; private readonly Stream _out; private readonly TcpClient _c;
        private int _status = 200; private string _desc = "OK";
        private readonly List<string[]> _hdr = new List<string[]>();
        private readonly MemoryStream _body = new MemoryStream();
        private static readonly string Root = HttpRuntime.AppDomainAppPath;

        public Worker(ParsedRequest r, Stream o, TcpClient c) { _r = r; _out = o; _c = c; }

        public override string GetUriPath() { return _r.Path; }
        public override string GetQueryString() { return _r.Query; }
        public override string GetRawUrl() { return _r.RawUrl; }
        public override string GetHttpVerbName() { return _r.Method; }
        public override string GetHttpVersion() { return _r.Version; }
        public override string GetRemoteAddress() { return "127.0.0.1"; }
        public override int GetRemotePort() { return 0; }
        public override string GetLocalAddress() { return "127.0.0.1"; }
        public override int GetLocalPort() { return ((IPEndPoint)_c.Client.LocalEndPoint).Port; }
        public override string GetFilePath() { return _r.Path; }
        public override string GetFilePathTranslated() { return MapPath(_r.Path); }
        public override string GetPathInfo() { return ""; }
        public override string GetAppPath() { return HttpRuntime.AppDomainAppVirtualPath; }
        public override string GetAppPathTranslated() { return Root; }
        public override string GetServerVariable(string name)
        {
            switch (name) { case "HTTP_HOST": return GetKnownRequestHeader(HeaderHost); case "SERVER_NAME": return "localhost"; default: return ""; }
        }
        public override string MapPath(string virtualPath)
        {
            string p = virtualPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            return Path.Combine(Root, p);
        }
        public override string GetKnownRequestHeader(int index)
        {
            string v; return _r.Headers.TryGetValue(GetKnownRequestHeaderName(index), out v) ? v : null;
        }
        public override string[][] GetUnknownRequestHeaders()
        {
            var known = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < RequestHeaderMaximum; i++) known.Add(GetKnownRequestHeaderName(i));
            var l = new List<string[]>();
            foreach (var kv in _r.Headers) if (!known.Contains(kv.Key)) l.Add(new[] { kv.Key, kv.Value });
            return l.ToArray();
        }
        public override byte[] GetPreloadedEntityBody() { return _r.Body; }
        public override bool IsEntireEntityBodyIsPreloaded() { return true; }
        public override int GetPreloadedEntityBodyLength() { return _r.Body.Length; }
        public override int ReadEntityBody(byte[] buffer, int size) { return 0; }
        public override string GetProtocol() { return "http"; }
        public override bool IsSecure() { return false; }

        public override void SendStatus(int statusCode, string statusDescription) { _status = statusCode; _desc = statusDescription; }
        public override void SendKnownResponseHeader(int index, string value) { _hdr.Add(new[] { GetKnownResponseHeaderName(index), value }); }
        public override void SendUnknownResponseHeader(string name, string value) { _hdr.Add(new[] { name, value }); }
        public override void SendResponseFromMemory(byte[] data, int length) { _body.Write(data, 0, length); }
        public override void SendResponseFromMemory(IntPtr data, int length)
        {
            var b = new byte[length]; System.Runtime.InteropServices.Marshal.Copy(data, b, 0, length); _body.Write(b, 0, length);
        }
        public override void SendResponseFromFile(string filename, long offset, long length)
        {
            using (var f = File.OpenRead(filename))
            {
                f.Seek(offset, SeekOrigin.Begin);
                long left = length < 0 ? f.Length - offset : length; var buf = new byte[8192];
                while (left > 0) { int k = f.Read(buf, 0, (int)Math.Min(buf.Length, left)); if (k <= 0) break; _body.Write(buf, 0, k); left -= k; }
            }
        }
        public override void SendResponseFromFile(IntPtr handle, long offset, long length)
        {
            using (var f = new FileStream(new Microsoft.Win32.SafeHandles.SafeFileHandle(handle, false), FileAccess.Read))
            {
                f.Seek(offset, SeekOrigin.Begin);
                long left = length; var buf = new byte[8192];
                while (left > 0) { int k = f.Read(buf, 0, (int)Math.Min(buf.Length, left)); if (k <= 0) break; _body.Write(buf, 0, k); left -= k; }
            }
        }
        public override void FlushResponse(bool finalFlush) { }
        public override void EndOfRequest()
        {
            var sb = new StringBuilder();
            sb.Append("HTTP/1.1 ").Append(_status).Append(' ').Append(_desc).Append("\r\n");
            foreach (var h in _hdr)
                if (!h[0].Equals("Content-Length", StringComparison.OrdinalIgnoreCase) && !h[0].Equals("Connection", StringComparison.OrdinalIgnoreCase))
                    sb.Append(h[0]).Append(": ").Append(h[1]).Append("\r\n");
            sb.Append("Content-Length: ").Append(_body.Length).Append("\r\nConnection: close\r\n\r\n");
            var head = Encoding.UTF8.GetBytes(sb.ToString());
            _out.Write(head, 0, head.Length);
            if (_r.Method != "HEAD") _body.WriteTo(_out);
            _out.Flush();
        }
        public override void CloseConnection() { _c.Close(); }
    }
}

namespace LabHost { static class Ext { } }
