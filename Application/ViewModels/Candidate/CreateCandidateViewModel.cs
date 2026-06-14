using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eVote360.Core.Application.ViewModels.Candidate
{
    public class CreateCandidateViewModel
    {
        [Required(ErrorMessage = "El nombre del candidato es requerido.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "El apellido del candidato es requerido.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Debe ingresar la foto del candidato es requerida.")]
        public IFormFile? Photo { get; set; }

        [Required(ErrorMessage = "El estado es requerido.")]
        public bool Status { get; set; } = true;
    }
}
