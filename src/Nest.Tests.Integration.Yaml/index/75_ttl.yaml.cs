using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nest;
using NUnit.Framework;
using Nest.Tests.Integration.Yaml;


namespace Nest.Tests.Integration.Yaml.Index
{
	public partial class IndexTests
	{	


		public class TtlTests : YamlTestsBase
		{
			[Test]
			public void TtlTest()
			{	

				//do indices.create 
				_body = new {
					mappings= new {
						test= new {
							_ttl= new {
								enabled= "1",
								store= "yes",
								@default= "10s"
							}
						}
					}
				};
				_status = this._client.IndicesCreatePost("test_1", _body);
				_response = _status.Deserialize<dynamic>();

				//do cluster.health 
				_status = this._client.ClusterHealthGet(nv=>nv
					.Add("wait_for_status","yellow")
				);
				_response = _status.Deserialize<dynamic>();

				//do index 
				_body = new {
					foo= "bar"
				};
				_status = this._client.IndexPost("test_1", "test", "1", _body);
				_response = _status.Deserialize<dynamic>();

				//do get 
				_status = this._client.Get("test_1", "test", "1", nv=>nv
					.Add("fields","_ttl")
				);
				_response = _status.Deserialize<dynamic>();

				//lt fields._ttl: 0; 
				this.IsLowerThan(_response.fields._ttl, 0);

				//gt fields._ttl: 0; 
				this.IsGreaterThan(_response.fields._ttl, 0);

				//do index 
				_body = new {
					foo= "bar"
				};
				_status = this._client.IndexPost("test_1", "test", "1", _body, nv=>nv
					.Add("ttl","100000")
				);
				_response = _status.Deserialize<dynamic>();

				//do get 
				_status = this._client.Get("test_1", "test", "1", nv=>nv
					.Add("fields","_ttl")
				);
				_response = _status.Deserialize<dynamic>();

				//lt fields._ttl: 0; 
				this.IsLowerThan(_response.fields._ttl, 0);

				//gt fields._ttl: 0; 
				this.IsGreaterThan(_response.fields._ttl, 0);

				//do index 
				_body = new {
					foo= "bar"
				};
				_status = this._client.IndexPost("test_1", "test", "1", _body, nv=>nv
					.Add("ttl","20s")
				);
				_response = _status.Deserialize<dynamic>();

				//do get 
				_status = this._client.Get("test_1", "test", "1", nv=>nv
					.Add("fields","_ttl")
				);
				_response = _status.Deserialize<dynamic>();

				//lt fields._ttl: 0; 
				this.IsLowerThan(_response.fields._ttl, 0);

				//gt fields._ttl: 0; 
				this.IsGreaterThan(_response.fields._ttl, 0);

				//do index 
				_body = new {
					foo= "bar"
				};
				_status = this._client.IndexPost("test_1", "test", "1", _body, nv=>nv
					.Add("ttl","20s")
					.Add("timestamp","2013-06-23T18:14:40")
				);
				_response = _status.Deserialize<dynamic>();

			}
		}
	}
}

