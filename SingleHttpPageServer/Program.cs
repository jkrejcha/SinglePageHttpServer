using System;

namespace SingleHttpPageServer
{
	public static class Program
	{
		private static SimpleHttpServer _server;

		public static void Main(String[] args)
		{
			String configFile = args.Length == 0 ? "config.json" : args[0];
			Configuration c = Configuration.Load(Configuration.ConfigurationType.JsonFile, configFile);
			_server = new SimpleHttpServer(c);
			_server.Start();
		}
	}
}
