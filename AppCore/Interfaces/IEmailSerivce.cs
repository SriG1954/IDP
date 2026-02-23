using Microsoft.Graph.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppCore.Interfaces
{
    public interface IEmailSerivce
    {
        Task<List<Message>> FetchInboxMessagesAsync(CancellationToken ct);
        Task<int> ProcessInboxBatchForDateAsync(DateOnly date, int batchSize = 25, CancellationToken ct = default);
    }
}
