using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eVote360.Core.Application.ViewModels.PoliticalParty
{
    public class SavePoliticalPartyViewModel
    {
        public required int Id { get; set; }

        [Required(ErrorMessage = "Debe ingresar el nombre del partido")]
        [DataType(DataType.Text)]
        public string Name { get; set; }

        [DataType(DataType.Text)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Debe ingresar las siglas del partido")]
        [DataType(DataType.Text)]
        public string Acronym { get; set; }
        
        [DataType(DataType.Upload)]
        public IFormFile? Logo { get; set; }

        public string? ExistingLogoPath { get; set; }

        [Required(ErrorMessage = "El estado es requerido")]
        public bool Status { get; set; }
    }
}
