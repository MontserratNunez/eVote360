using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eVote360.Core.Application.Dtos.Vote
{
    public class OcrValidationDto
    {
        public IFormFile File { get; set; }

        public int CitizenId { get; set; }
        public int ElectionId { get; set; }
    }
}
