using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PuntoVenta.DTO;
using PuntoVenta.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace PuntoVenta.Controllers
{
    public class LoginController : Controller
    {
        private readonly PuntoVentaContext _context;
        private readonly IConfiguration _configuration;

        public LoginController(PuntoVentaContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<IActionResult> Index()
        {
            return View();
        }
        /// <summary>
        /// Método de Login para el usuario 
        /// </summary>
        /// <param name="credencialesUsuario">Objeto de datos que contiene correo y contraseña</param>
        /// <returns>regresa el token para autenticar la sesión del usuario</returns>
        [HttpPost]
        public async Task<IActionResult> LoginUser([FromBody] CredencialesUsuario credencialesUsuario)
        {
            if (credencialesUsuario == null || string.IsNullOrEmpty(credencialesUsuario.email) || string.IsNullOrEmpty(credencialesUsuario.password))
            {
                return BadRequest("Credenciales inválidas.");
            }
            //busca la existencia del usuario mediante el correo
            var usuario = await _context.Usuario.Where(x => x.correo == credencialesUsuario.email).FirstOrDefaultAsync();

            if (usuario == null) { return NotFound("Usuario no encontrado"); }
            //Valida la contraseña
            if (usuario.contrasena != credencialesUsuario.password) { return Unauthorized("Contraseña incorrecta"); }
            //Genera el token de autenticación
            var respuesta = await ConstruirToken(credencialesUsuario, usuario);
            if (respuesta == null) { return StatusCode(500, "Error al generar el token."); }

            return Ok(respuesta);
        }


        /// <summary>
        /// Metodo para construir el token de acceso 
        /// </summary>
        /// <param name="credencialesUsuario">Contiene el correo y contraseña</param>
        /// <returns>Objeto con el Toke y expiración</returns>
        public async Task<RespuestaAutenticacion> ConstruirToken(CredencialesUsuario credencialesUsuario, Usuario usuario)
        {
            //var usuario = await _context.Usuario.Where(x => x.correo == credencialesUsuario.email).Select(x => x.id_usuario).FirstOrDefaultAsync();
            if (usuario == null) { return null; }
            if (_configuration == null)
            {
                throw new Exception("La configuración no se ha inicializado.");
            }
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["keyjwt"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expiration = DateTime.UtcNow.AddHours(9);

            var securityToken = new JwtSecurityToken(issuer: null, audience: null, expires: expiration, signingCredentials: creds);

            return new RespuestaAutenticacion()
            {
                Token = new JwtSecurityTokenHandler().WriteToken(securityToken),
                Expíracion = expiration
            };
        }
    }
}
