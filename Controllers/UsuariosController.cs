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

    private async Task<bool> UsuariosExistente(string username)
    {

        if (await _context.TB_USUARIOS.AnyAsync(x => x.Username.ToLower() == username.ToLower()))
        {
            return true;

        }
        return false;

    }

    [HttpPost("Registro")]
    public async Task<IActionResult> RegistrarUsuario(Usuario user)
    {
        try
        {
            if (await UsuariosExistente(user.Username))
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


    [HttpPost("Autentificar")]

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
                usuario.DataAcesso = System.DateTime.Now;
                _context.TB_USUARIOS.Update(usuario);
                await _context.SaveChangesAsync(); //Confirma a alteração no banco

                return Ok(usuario);
            }
        }
        catch (System.Exception ex)
        {

            return BadRequest(ex.Message);
        }
    }


    //Método para alteração de Senha.
    [HttpPut("AlterarSenha")]
    public async Task<IActionResult> AlterarSenhaUsuario(Usuario credenciais)
    {
        try
        {
            Usuario usuario = await _context.TB_USUARIOS //Busca o usuário no banco através do login
               .FirstOrDefaultAsync(x => x.Username.ToLower().Equals(credenciais.Username.ToLower()));

            if (usuario == null) //Se não achar nenhum usuário pelo login, retorna mensagem.
                throw new System.Exception("Usuário não encontrado.");

            Criptografia.CriarPasswordHash(credenciais.PasswordString, out byte[] hash, out byte[] salt);
            usuario.PasswordHash = hash; //Se o usuário existir, executa a criptografia (linha 122)
            usuario.PasswordSalt = salt; //guardando o hash e o salt nas propriedades do usuário (linhas 123/124)

            _context.TB_USUARIOS.Update(usuario);
            int linhasAfetadas = await _context.SaveChangesAsync(); //Confirma a alteração no banco
            return Ok(linhasAfetadas); //Retorna as linhas afetadas (Geralmente sempre 1 linha msm)
        }
        catch (System.Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("GetAll")]
    public async Task<IActionResult> GetUsuarios()
    {
        try
        {
            List<Usuario> lista = await _context.TB_USUARIOS.ToListAsync();
            return Ok(lista);
        }
        catch (System.Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }


}
