using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppCore.Models
{
    public sealed class ConfidenceRouter
    {
        //private readonly IRuleProvider _rules;
        //public ConfidenceRouter(IRuleProvider rules) => _rules = rules;

        //public async Task<RouteDecision> DecideAsync(string label, double confidence, CancellationToken ct)
        //{
        //    var th = await _rules.GetModelThresholdAsync(label, ct)
        //             ?? await _rules.GetModelThresholdAsync("default", ct);

        //    if (confidence >= th.AutoAcceptThreshold) return RouteDecision.AutoAccept;
        //    if (confidence >= th.HumanReviewThreshold) return RouteDecision.MoreExtraction; // add more signals
        //    return RouteDecision.HumanInTheLoop;
        //}
    }

    public enum RouteDecision { AutoAccept, MoreExtraction, HumanInTheLoop }
}
