using eVote360.Core.Application.Dtos;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eVote360.Core.Application.ViewModels.AssignPosition
{

    public class CreateAssignPositionViewModel
    {
        [Required(ErrorMessage = "Debe seleccionar un candidato.")]
        public int CandidateId { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un puesto.")]
        public int ElectivePositionId { get; set; }

        public List<DropdownDto> Candidates { get; set; } = new();

        public List<DropdownDto> Positions { get; set; } = new();
    }

}
