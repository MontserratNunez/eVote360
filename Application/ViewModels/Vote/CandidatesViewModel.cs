using eVote360.Core.Application.Dtos.Vote;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eVote360.Core.Application.ViewModels.Vote
{
    public class CandidatesViewModel
    {
        public int PositionId { get; set; }
        public string PositionName { get; set; }

        public List<CandidateOptionViewModel> Candidates { get; set; } = new();

        [Required(ErrorMessage = "Debe seleccionar un candidato antes de votar.")]
        public int? SelectedAssignPositionId { get; set; }
    }
}
