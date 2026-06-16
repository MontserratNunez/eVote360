using eVote360.Core.Application.Dtos.Vote;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eVote360.Core.Application.Interfaces
{
    public interface IEmailService
    {
        Task SendEmail(string receptor, string subject, string body);
    }
}
