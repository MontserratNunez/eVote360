using eVote360.Core.Application.Dtos;
using System.ComponentModel.DataAnnotations;

namespace eVote360.Core.Application.ViewModels.PoliticalAlliance
{
    public class CreatePoliticalAllianceViewModel
    {
        [Required(ErrorMessage = "Debe seleccionar un partido político.")]
        public int PoliticalPartyId { get; set; }

        public List<DropdownDto> Parties { get; set; } = new();
    }
}
