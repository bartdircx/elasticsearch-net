using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nest;
using NUnit.Framework;
using Nest.Tests.Integration.Yaml;


namespace Nest.Tests.Integration.Yaml.Update
{
	public partial class UpdateTests
	{	


		public class ParentTests : YamlTestsBase
		{
			[Test]
			public void ParentTest()
			{	

				//do indices.create 
				_body = new {
					mappings= new {
						test= new {
							_parent= new {
								type= "foo"
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

				//do update 
				_body = new {
					doc= new {
						foo= "baz"
					},
					upsert= new {
						foo= "bar"
					}
				};
				_status = this._client.UpdatePost("test_1", "test", "1", _body);
				_response = _status.Deserialize<dynamic>();

				//do update 
				_body = new {
					doc= new {
						foo= "baz"
					},
					upsert= new {
						foo= "bar"
					}
				};
				_status = this._client.UpdatePost("test_1", "test", "1", _body, nv=>nv
					.Add("parent","5")
				);
				_response = _status.Deserialize<dynamic>();

				//do get 
				_status = this._client.Get("test_1", "test", "1", nv=>nv
					.Add("parent","5")
					.Add("fields","System.Collections.Generic.List`1[System.Object]")
				);
				_response = _status.Deserialize<dynamic>();

				//do update 
				_body = new {
					doc= new {
						foo= "baz"
					}
				};
				_status = this._client.UpdatePost("test_1", "test", "1", _body);
				_response = _status.Deserialize<dynamic>();

				//do update 
				_body = new {
					doc= new {
						foo= "baz"
					}
				};
				_status = this._client.UpdatePost("test_1", "test", "1", _body, nv=>nv
					.Add("parent","5")
					.Add("fields","foo")
				);
				_response = _status.Deserialize<dynamic>();

			}
		}
	}
}

