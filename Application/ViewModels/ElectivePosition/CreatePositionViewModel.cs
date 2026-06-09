using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eVote360.Core.Application.ViewModels.ElectivePosition
{

    public class CreatePositionViewModel
    {
        [Required(ErrorMessage = "El nombre del puesto es requerido.")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "La descripción es requerida.")]
        public string Description { get; set; } = null!;

        public bool Status { get; set; } = true;
    }

}
