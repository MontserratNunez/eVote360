using System.ComponentModel.DataAnnotations;

namespace eVote360.Core.Application.ViewModels.Vote
{
    public class VoteViewModel
    {
        [Required(ErrorMessage = "Debe ingresar su número de documento.")]
        public string DocumentNumber { get; set; }
    }

}
