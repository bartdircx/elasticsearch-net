using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nest;
using NUnit.Framework;
using Nest.Tests.Integration.Yaml;


namespace Nest.Tests.Integration.Yaml.IndicesGetTemplate
{
	public partial class IndicesGetTemplateTests
	{	
	
		public class IndicesGetTemplate20GetMissingYamlBase : YamlTestsBase
		{
			public IndicesGetTemplate20GetMissingYamlBase() : base()
			{	

				//do indices.delete_template 
				_status = this._client.IndicesDeleteTemplate("*", nv=>nv
					.Add("ignore","404")
				);
				_response = _status.Deserialize<dynamic>();

			}
		}


		public class GetMissingTemplatePost0903Tests : IndicesGetTemplate20GetMissingYamlBase
		{
			[Test]
			public void GetMissingTemplatePost0903Test()
			{	

				//do indices.get_template 
				_status = this._client.IndicesGetTemplate("test");
				_response = _status.Deserialize<dynamic>();

			}
		}

		public class GetMissingTemplatePre0903Tests : IndicesGetTemplate20GetMissingYamlBase
		{
			[Test]
			public void GetMissingTemplatePre0903Test()
			{	

				//do indices.delete_template 
				_status = this._client.IndicesDeleteTemplate("test", nv=>nv
					.Add("ignore","404")
				);
				_response = _status.Deserialize<dynamic>();

				//do indices.get_template 
				_status = this._client.IndicesGetTemplate("test");
				_response = _status.Deserialize<dynamic>();

			}
		}
	}
}

