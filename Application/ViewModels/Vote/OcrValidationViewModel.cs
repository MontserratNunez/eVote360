using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;


namespace eVote360.Core.Application.ViewModels.Vote
{
    public class OcrValidationViewModel
    {
        [Required(ErrorMessage = "Debe subir una imagen de su cédula.")]
        public IFormFile File { get; set; }
    }

}
