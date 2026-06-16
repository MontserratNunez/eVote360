using System.ComponentModel.DataAnnotations;

namespace eVote360.Core.Application.ViewModels.Citizen
{
    public class SaveCitizenViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es requerido.")]
        [DataType(DataType.Text)]
        public required string Name { get; set; }

        [Required(ErrorMessage = "El apellido es requerido.")]
        [DataType(DataType.Text)]
        public required string LastName { get; set; }

        [Required(ErrorMessage = "El correo electrónico es requerido.")]
        [DataType(DataType.EmailAddress)]
        [EmailAddress(ErrorMessage = "Debe ingresar un correo electrónico válido.")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "El número de documento de identidad es requerido.")]
        [DataType(DataType.Text)]
        public required string DocumentNumber { get; set; }

        public bool Status { get; set; } = true;
    }
}