using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eVote360.Core.Application.ViewModels.Vote
{
    public class VerifyCodeViewModel
    {
        [Required(ErrorMessage = "Debe ingresar el código de verificación enviado a su correo electrónico.")]
        public string Code { get; set; } = string.Empty;
    }
}
