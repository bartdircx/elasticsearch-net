﻿using System;
using FluentAssertions;
using SearchApis;

namespace Nest.Tests.Literate.SearchAPIs.RequestBodySearch
{
	public class FromAndSize
	{
		/**
		 * Pagination of results can be done by using the from and size parameters. 
		 * The from parameter defines the offset from the first result you want to fetch. 
		 * The size parameter allows you to configure the maximum amount of hits to be returned.
		 */

		public class Usage : SearchUsageBase
		{
			protected override object ExpectedJson =>
				new { from = 10, size = 12 };

			public override int ExpectStatusCode => 200;

			public override bool ExpectIsValid => true;

			public override void AssertUrl(string url) => url.Should().EndWith("");

			protected override SearchRequest Initializer =>
				new SearchRequest()
				{
					From = 10,
					Size = 12
				};

			protected override Func<SearchDescriptor<object>, ISearchRequest> Fluent => s => s
					.From(10)
					.Size(12);
		}
	}
}