using System;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using Elasticsearch.Net;

namespace Nest
{
	internal abstract class DocumentJsonConverterBase<TRequest> : JsonConverter where TRequest : IUntypedDocumentRequest
	{
		private readonly Type _genericRequestType;

		protected DocumentJsonConverterBase(Type genericRequestType)
		{
			_genericRequestType = genericRequestType;
		}

		public override bool CanRead => true;
		public override bool CanWrite => true;
		public override bool CanConvert(Type objectType) => true;

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			//not optimized but deserializing create requests is far from common practice
			var genericType = objectType.GetTypeInfo().GenericTypeArguments[0];
			var o = serializer.Deserialize(reader, genericType);
			// index, type and id are optional parameters on _genericRequestType but need to be passed to construct through reflection
			var x = _genericRequestType.CreateGenericInstance(genericType, typeof(DocumentPath<>).CreateGenericInstance(genericType, o), null, null, null);
			return x;
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			var untypedDocumentRequest = (IUntypedDocumentRequest)value;
			var o = untypedDocumentRequest.UntypedDocument;
			var v = serializer.GetConnectionSettings().SourceSerializer.SerializeToBytes(o);
			using(var ms = new MemoryStream(v))
			using(var streamReader = new StreamReader(ms))
			using(var reader = new JsonTextReader(streamReader))
				writer.WriteToken(reader);
			//writer.WriteRawValue(v);
		}
	}
}
