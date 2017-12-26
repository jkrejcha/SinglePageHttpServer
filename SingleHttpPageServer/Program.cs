using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SingleHttpPageServer
{
	class Program
	{
		internal static SimpleHttpServer Server;

		static void Main(string[] args)
		{
			Configuration c = new Configuration();
			c.ConfigType = Configuration.ConfigurationType.Memory;

		}
	}
}
