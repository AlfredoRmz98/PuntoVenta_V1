using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PuntoVenta.DTO;
using PuntoVenta.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PuntoVenta.Controllers
{
    public class UsuarioController : Controller
    {
        private readonly PuntoVentaContext _context;
        private readonly IConfiguration configuration;

        public UsuarioController(PuntoVentaContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View("_Login");
        }


        /// <summary>
        /// Creación de nuevo Registro Usuario
        /// </summary>
        /// <param name="usuario">Objeto del tipo Usuario</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registro([Bind("Id_usuario,nombre,apellido_paterno,apellido_materno,contrasena")] Usuario usuario)
        {
            //Verficamos que los datos sean válidos
            if (!ModelState.IsValid)
            {
                return BadRequest("los datos no son válidos");
            }
            
            if (string.IsNullOrEmpty(usuario.correo) || string.IsNullOrEmpty(usuario.contrasena))
            {
                return BadRequest("El correo o contraseña no pueden estar vacíos.");
            }
            _context.Add(usuario);
            await _context.SaveChangesAsync();
            return Ok();
        }
        /// <summary>
        /// Método de Login para el usuario 
        /// </summary>
        /// <param name="credencialesUsuario">Objeto de datos que contiene correo y contraseña</param>
        /// <returns>regresa el token para autenticar la sesión del usuario</returns>
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] CredencialesUsuario credencialesUsuario)
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
            var respuesta = await ConstruirToken(credencialesUsuario);
            if (respuesta == null) { return StatusCode(500, "Error al generar el token."); }

            return Ok(respuesta);
        }


        /// <summary>
        /// Metodo para construir el token de acceso 
        /// </summary>
        /// <param name="credencialesUsuario">Contiene el correo y contraseña</param>
        /// <returns>Objeto con el Toke y expiración</returns>
        public async Task<RespuestaAutenticacion> ConstruirToken(CredencialesUsuario credencialesUsuario)
        {
            var usuario = await _context.Usuario.Where(x => x.correo == credencialesUsuario.email).Select(x => x.id_usuario).FirstOrDefaultAsync();
            if(usuario == null){    return null;    }
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["keyjwt"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expiration = DateTime.UtcNow.AddHours(9);

            var securityToken  = new JwtSecurityToken(issuer: null, audience: null, expires: expiration, signingCredentials: creds);

            return new RespuestaAutenticacion()
            {
                Token = new JwtSecurityTokenHandler().WriteToken(securityToken),
                Expíracion = expiration
            };
        }
    }
}
