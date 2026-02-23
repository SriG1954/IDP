using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppCore.EntityModels
{

    public partial class MailSyncState
    {
        public long MailSyncStateId { get; set; }

        public string Mailbox { get; set; } = default!;
        public string FolderId { get; set; } = "inbox";
        public DateTime DateUtc { get; set; }  // store as date

        public string? OdataNextLink { get; set; }

        public DateTime UpdatedAtUtc { get; set; }
    }
}
