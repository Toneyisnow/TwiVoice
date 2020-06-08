using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TwiVoiceWebService.Models;

namespace TwiVoiceWebService.Data
{
    public class TwiVoiceWebServiceContext : DbContext
    {
        public TwiVoiceWebServiceContext (DbContextOptions<TwiVoiceWebServiceContext> options)
            : base(options)
        {
        }

        public DbSet<TwiVoiceWebService.Models.TwiRequest> TwiRequest { get; set; }
    }
}
