using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace eVote360.Core.Application.ViewModels.PoliticalParty
{
    public class CreatePoliticalPartyViewModel
    {
        [Required(ErrorMessage = "Debe ingresar el nombre del partido")]
        [DataType(DataType.Text)]
        public string Name { get; set; }

        [DataType(DataType.Text)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Debe ingresar las siglas del partido")]
        [DataType(DataType.Text)]
        public string Acronym { get; set; }

        [Required(ErrorMessage = "Debe ingresar la foto del partido")]
        [DataType(DataType.Upload)]
        public IFormFile? Logo { get; set; }

        [Required(ErrorMessage = "El estado es requerido")]
        public bool Status { get; set; }
    }
}
