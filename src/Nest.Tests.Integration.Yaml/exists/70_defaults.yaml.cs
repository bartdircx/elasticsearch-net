using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nest;
using NUnit.Framework;
using Nest.Tests.Integration.Yaml;


namespace Nest.Tests.Integration.Yaml.Exists
{
	public partial class ExistsTests
	{	


		public class ClientSideDefaultTypeTests : YamlTestsBase
		{
			[Test]
			public void ClientSideDefaultTypeTest()
			{	

				//do index 
				_body = new {
					foo= "bar"
				};
				_status = this._client.IndexPost("test_1", "test", "1", _body);
				_response = _status.Deserialize<dynamic>();

				//do exists 
				_status = this._client.ExistsHead("test_1", "_all", "1");
				_response = _status.Deserialize<dynamic>();

				//is_true ; 
				this.IsTrue(_response);

			}
		}
	}
}

