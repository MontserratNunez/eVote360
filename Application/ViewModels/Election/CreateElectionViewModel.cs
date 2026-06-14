using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eVote360.Core.Application.ViewModels.Election
{
    public class CreateElectionViewModel
    {
        [Required(ErrorMessage = "El nombre de la elección es requerido.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "La fecha de realización es requerida.")]
        public DateTime Date { get; set; }
    }
}
