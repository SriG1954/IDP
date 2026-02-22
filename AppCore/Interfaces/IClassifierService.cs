using AppCore.EntityModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppCore.Interfaces
{
    public interface IClassifierService
    {
        Task RunJobAsync(DocumentJob job, CancellationToken ct);
    }
}
