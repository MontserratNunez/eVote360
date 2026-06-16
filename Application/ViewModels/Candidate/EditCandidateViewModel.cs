using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eVote360.Core.Application.ViewModels.Candidate
{

    public class EditCandidateViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del candidato es requerido.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "El apellido del candidato es requerido.")]
        public string LastName { get; set; }

        [DataType(DataType.Upload)]
        public IFormFile? Photo { get; set; }

        public string? ExistingPhotoPath { get; set; }

        public bool Status { get; set; }
    }

}
