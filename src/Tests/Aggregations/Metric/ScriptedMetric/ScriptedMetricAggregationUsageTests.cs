﻿using System;
using FluentAssertions;
using Nest;
using Tests.Framework;
using Tests.Framework.Integration;
using Tests.Framework.ManagedElasticsearch.Clusters;
using Tests.Framework.MockData;
using System.Collections.Generic;
using Tests.Framework.ManagedElasticsearch.NodeSeeders;

namespace Tests.Aggregations.Metric.ScriptedMetric
{
	public class ScriptedMetricAggregationUsageTests : ProjectsOnlyAggregationUsageTestBase
	{
		class Scripted
		{
			public string Language { get; set; }
			public string Combine { get; set; }
			public string Reduce { get; set; }
			public string Map { get; set; }
			public string Init { get; set; }
		}
		public ScriptedMetricAggregationUsageTests(ReadOnlyCluster i, EndpointUsage usage) : base(i, usage) { }

		private Scripted Script = new Scripted
		{
			Language = "painless",
			Init = "params._agg.commits = []",
			Map = "if (doc['state'].value == \"Stable\") { params._agg.commits.add(doc['numberOfCommits'].value) }",
			Combine = "def sum = 0.0; for (c in params._agg.commits) { sum += c } return sum",
			Reduce = "def sum = 0.0; for (a in params._aggs) { sum += a } return sum",
		};

		protected override object ExpectJson => new
		{
			aggs = new
			{
				sum_the_hard_way = new
				{
					scripted_metric = new
					{
						init_script = new { source = Script.Init },
						map_script = new { source = Script.Map },
						combine_script = new { source = Script.Combine },
						reduce_script = new { source = Script.Reduce }
					}
				}
			}
		};

		protected override Func<SearchDescriptor<Project>, ISearchRequest> Fluent => s => s
			.Index(DefaultSeeder.ProjectsAliasFilter)
			.Aggregations(a => a
				.ScriptedMetric("sum_the_hard_way", sm => sm
					.InitScript(ss => ss.Source(Script.Init))
					.MapScript(ss => ss.Source(Script.Map))
					.CombineScript(ss => ss.Source(Script.Combine))
					.ReduceScript(ss => ss.Source(Script.Reduce))
				)
			);

		protected override SearchRequest<Project> Initializer =>
			new SearchRequest<Project>(DefaultSeeder.ProjectsAliasFilter)
			{
				Aggregations = new ScriptedMetricAggregation("sum_the_hard_way")
				{
					InitScript = new InlineScript(Script.Init),
					MapScript = new InlineScript(Script.Map),
					CombineScript = new InlineScript(Script.Combine),
					ReduceScript = new InlineScript(Script.Reduce)
				}
			};

		protected override void ExpectResponse(ISearchResponse<Project> response)
		{
			response.ShouldBeValid();
			var sumTheHardWay = response.Aggs.ScriptedMetric("sum_the_hard_way");
			sumTheHardWay.Should().NotBeNull();
			sumTheHardWay.Value<int>().Should().BeGreaterThan(0);
		}
	}

	/// <summary>
	/// Multiple scripted metric with dictionary result
	/// </summary>
	public class ScriptedMetricMultiAggregationTests : ProjectsOnlyAggregationUsageTestBase
	{
		class Scripted
		{
			public string Language { get; set; }
			public string Combine { get; set; }
			public string Reduce { get; set; }
			public string Map { get; set; }
			public string Init { get; set; }
		}

		private Scripted First = new Scripted
		{
			Language = "painless",
			Init = "params._agg.map = [:]",
			Map =
				"if (params._agg.map.containsKey(doc['state'].value))" +
				"    params._agg.map[doc['state'].value] += 1;" +
				"else" +
				"    params._agg.map[doc['state'].value] = 1;",

			Reduce =
				"def reduce = [:];" +
				"for (agg in params._aggs)" +
				"{" +
				"    for (entry in agg.map.entrySet())" +
				"    {" +
				"        if (reduce.containsKey(entry.getKey()))" +
				"            reduce[entry.getKey()] += entry.getValue();" +
				"        else" +
				"            reduce[entry.getKey()] = entry.getValue();" +
				"    }" +
				"}" +
				"return reduce;"
		};

		private Scripted Second = new Scripted
		{
			Language = "painless",
			Combine = "def sum = 0.0; for (c in params._agg.commits) { sum += c } return sum",
			Reduce = "def sum = 0.0; for (a in params._aggs) { sum += a } return sum",
			Map = "if (doc['state'].value == \"Stable\") { params._agg.commits.add(doc['numberOfCommits'].value) }",
			Init = "params._agg.commits = []"
		};

		public ScriptedMetricMultiAggregationTests(ReadOnlyCluster i, EndpointUsage usage) : base(i, usage) { }

		protected override object ExpectJson => new
		{
			aggs = new
			{
				by_state_total = new
				{
					scripted_metric = new
					{
						init_script = new
						{
							source = First.Init,
							lang = First.Language
						},
						map_script = new
						{
							source = First.Map,
							lang = First.Language
						},
						reduce_script = new
						{
							source = First.Reduce,
							lang = First.Language
						}
					}
				},
				total_commits = new
				{
					scripted_metric = new
					{
						init_script = new
						{
							source = Second.Init,
							lang = Second.Language
						},
						map_script = new
						{
							source = Second.Map,
							lang = Second.Language
						},
						combine_script = new
						{
							source = Second.Combine,
							lang = Second.Language
						},
						reduce_script = new
						{
							source = Second.Reduce,
							lang = Second.Language
						}
					}
				}
			}
		};

		protected override Func<SearchDescriptor<Project>, ISearchRequest> Fluent => s => s
			.Index(DefaultSeeder.ProjectsAliasFilter)
			.Aggregations(a => a
				.ScriptedMetric("by_state_total", sm => sm
					.InitScript(ss => ss.Source(First.Init).Lang(First.Language))
					.MapScript(ss => ss.Source(First.Map).Lang(First.Language))
					.ReduceScript(ss => ss.Source(First.Reduce).Lang(First.Language))
				)
				.ScriptedMetric("total_commits", sm => sm
					.InitScript(ss => ss.Source(Second.Init).Lang(Second.Language))
					.MapScript(ss => ss.Source(Second.Map).Lang(Second.Language))
					.CombineScript(ss => ss.Source(Second.Combine).Lang(Second.Language))
					.ReduceScript(ss => ss.Source(Second.Reduce).Lang(Second.Language))
				)
			);

		protected override SearchRequest<Project> Initializer =>
			new SearchRequest<Project>(DefaultSeeder.ProjectsAliasFilter)
			{
				Aggregations =
					new ScriptedMetricAggregation("by_state_total")
					{
						InitScript = new InlineScript(First.Init) {Lang = First.Language},
						MapScript = new InlineScript(First.Map) {Lang = First.Language},
						ReduceScript = new InlineScript(First.Reduce) {Lang = First.Language}
					} &&
					new ScriptedMetricAggregation("total_commits")
					{
						InitScript = new InlineScript(Second.Init) {Lang = Second.Language},
						MapScript = new InlineScript(Second.Map) {Lang = Second.Language},
						CombineScript = new InlineScript(Second.Combine) {Lang = Second.Language},
						ReduceScript = new InlineScript(Second.Reduce) {Lang = Second.Language}
					}
			};

		protected override void ExpectResponse(ISearchResponse<Project> response)
		{
			response.ShouldBeValid();
			var by_state_total = response.Aggs.ScriptedMetric("by_state_total");
			var total_commits = response.Aggs.ScriptedMetric("total_commits");

			by_state_total.Should().NotBeNull();
			total_commits.Should().NotBeNull();

			by_state_total.Value<IDictionary<string, int>>().Should().NotBeNull();
			total_commits.Value<int>().Should().BeGreaterThan(0);
		}
	}
}
