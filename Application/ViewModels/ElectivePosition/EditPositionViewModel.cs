using System.ComponentModel.DataAnnotations;

namespace eVote360.Core.Application.ViewModels.ElectivePosition
{
    public class EditPositionViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del puesto es requerido.")]
        public required string Name { get; set; }

        [Required(ErrorMessage = "La descripción es requerida.")]
        public required string Description { get; set; }

        public bool Status { get; set; }
    }
}