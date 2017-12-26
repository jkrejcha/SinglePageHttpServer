using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SingleHttpPageServer
{
	public class SimpleHttpServer
	{
		public Configuration Config;

		/// <summary>
		/// File to byte mappings. Multiple files can be served by the same name.
		/// Names are case-insensitive.
		/// </summary>
		private Dictionary<List<String>, byte[]> Cache = new Dictionary<List<string>, byte[]>();

		private byte[] DefaultFile;

		public bool Running { get; private set; }

		/// <summary>
		/// Start the server loop.
		/// </summary>
		public void Start()
		{
			CacheFiles(Config.MacrosEnabled);
			HttpListener listener = new HttpListener();
			foreach (UInt16 port in Config.Ports)
			{
				listener.Prefixes.Add("http://*:" + port + "/");
			}
			listener.Start();
			Running = true;
			while (true)
			{
				HandleRequest(listener.GetContext());
			}
		}

		private void HandleRequest(HttpListenerContext context)
		{
			HttpListenerRequest request = context.Request;
			HttpListenerResponse response = context.Response;
			response.StatusCode = Config.ResponseCode;
			List<String> CachedValue = GetCachedFile(request.RawUrl);
			byte[] data = null;
			if (CachedValue == null)
			{
				data = DefaultFile;
			}
			else
			{
				data = Cache[CachedValue];
			}
			response.ContentLength64 = data.LongLength;
			response.ContentEncoding = Encoding.UTF8;
			response.OutputStream.Write(data, 0, data.Length);
			response.OutputStream.Close();
		}

		private List<String> GetCachedFile(String path)
		{
			foreach (List<String> filePaths in Cache.Keys)
			{
				if (filePaths.Contains(path)) return filePaths;
				if (filePaths.Contains("/" + path)) return filePaths;
			}
			return null;
		}

		private void CacheFiles(bool replaceMacros = true)
		{
			foreach (String filePath in Config.FileMappings.Keys)
			{
				Cache.Add(Config.FileMappings[filePath], CacheFile(filePath, replaceMacros));
			}
			DefaultFile = CacheFile(Config.DefaultFilePath, replaceMacros);
		}

		private byte[] CacheFile(String filePath, bool replaceMacros = true)
		{
			String fileData = String.Empty;
			try
			{
				fileData = System.IO.File.ReadAllText(filePath, Encoding.UTF8);
			}
			catch (Exception ex)
			{
				// TODO: Log error.
				return new byte[0];
			}
			if (replaceMacros && fileData.Length > 2) fileData = ReplaceMacros(fileData);
			return Encoding.UTF8.GetBytes(fileData);
		}

		private String ReplaceMacros(String data)
		{
			foreach (String macro in Config.Macros.Keys)
			{
				data = data.Replace("{" + macro + "}", Config.Macros[macro]);
			}
			return data.Replace("{code}", Config.ResponseCode.ToString());
		}
	}
}
