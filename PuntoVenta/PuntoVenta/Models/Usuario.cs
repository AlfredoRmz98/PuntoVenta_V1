using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace PuntoVenta.Models
{
    /// <summary>
    /// Atributos de datos para la validación de la contraseña
    /// </summary>
    public class ValidarContrasenaAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object value, ValidationContext validationContext)
        {
            var password = value as string;
            if(string.IsNullOrWhiteSpace(password) || password.Length < 8 ||
                !Regex.IsMatch(password,@"[A-Z]")  ||
                !Regex.IsMatch(password, @"[!@#$%^&*(),.?""':;{}|<>]"))
            {
                return new ValidationResult("La contraseña debe tener al menos 8 caracteres, incluir una mayúscula y un carácter especial.");
            }
            return ValidationResult.Success;
        }
    }
    public partial class Usuario
    {
        [Key]
        [Required]
        public string id_usuario { get; set; } = null!;
        public string correo { get; set; }
        public string Nombre { get; set; }
        public string apellido_paterno { get; set; }
        public string apellido_materno { get; set; }
        [ValidarContrasena]
        public string contrasena {  get; set; }
    }
}
