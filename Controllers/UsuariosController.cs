using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using RpgApi.Data;
using Microsoft.EntityFrameworkCore;
using RpgApi.Models;
using RpgApi.Utils;

namespace RpgApi.Controllers;

[ApiController]
[Route("(Controller)")]


public class UsuariosController : ControllerBase
{
    private readonly DataContext _context;

    public UsuariosController(DataContext Context)
    {

        _context = Context;

    }

    private async Task<bool> UsuarioExistente(string username)
    {

        if (await _context.TB_USUARIOS.AnyAsync(x => x.Username.ToLower() == username.ToLower()))
        {
            return true;

        }
        return false;

    }

    [HttpPost("Registrar")]
    public async Task<IActionResult> RegistrarUsuario(Usuario user)
    {
        try
        {
            if (await UsuarioExistente(user.Username))
                throw new System.Exception("Nome de usuário ja existe");

            Criptografia.CriarPasswordHash(user.PasswordString, out byte[] hash, out byte[] salt);
            user.PasswordString = string.Empty;
            user.PasswordHash = hash;
            user.PasswordSalt = salt;
            await _context.TB_USUARIOS.AddAsync(user);
            await _context.SaveChangesAsync();

            return Ok(user.Id);

        }
        catch (System.Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }


    [HttpPost("Autenticar")]

    public async Task<IActionResult> AutenticarUsuario(Usuario credenciais)
    {

        try
        {

            Usuario? usuario = await _context.TB_USUARIOS
                .FirstOrDefaultAsync(x => x.Username.ToLower().Equals(credenciais.Username.ToLower()));

            if (usuario == null)
            {

                throw new System.Exception("Usuário não encontrado.");
            }
            else if (!Criptografia.VerificarPasswordHash(credenciais.PasswordString, usuario.PasswordHash, usuario.PasswordSalt))
            {
                throw new System.Exception("Senha Incorreta.");
            }
            else
            {

                return Ok(usuario);
            }
        }
        catch (System.Exception ex)
        {

            return BadRequest(ex.Message);
        }
    }
}
