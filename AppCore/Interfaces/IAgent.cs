using AppCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppCore.Interfaces
{

    public interface IAgent
    {
        string Name { get; }
        Task<AgentResult> ExecuteAsync(AgentContext context, CancellationToken ct);
    }
}
