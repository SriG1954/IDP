using AppCore.Data;
using AppCore.EntityModels;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppCore.Interfaces
{
    public interface IBatchService
    {
        Task<bool> ExecuteAsync(CancellationToken ct);
    }
}
