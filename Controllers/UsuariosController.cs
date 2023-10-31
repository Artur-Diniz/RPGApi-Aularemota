using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using RpgApi.Data;
using Microsoft.EntityFrameworkCore;
using RpgApi.Models;
using RpgApi.Utils;
using System.Collections.Generic;

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

    [HttpPost("Registrar")]
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

    [HttpGet("{usuarioId}")]
        public async Task<IActionResult> GetUsuario(int usuarioId)
        {

            try
            {
                Usuario usuario = await _context.TB_USUARIOS //encontra o usuario no banco de dados

                    .FirstOrDefaultAsync(x => x.Id == usuarioId);

                return Ok(usuario);
            }
             catch (System. Exception ex)
                { 
                    return BadRequest(ex.Message);
                }
        }
    
    [HttpGet("GEtByLogin/{login}")]
        public async Task<IActionResult> GetUsuario(string login)
        {

            try
            {
                Usuario usuario = await _context.TB_USUARIOS
                .FirstOrDefaultAsync(x => x.Username.ToLower() == login.ToLower());

                return Ok();
            }
            catch (System. Exception ex)
                { 
                    return BadRequest(ex.Message);
                }
            
        }

    [HttpPut("AtualizatLocalizacao")]
        public async  Task<IActionResult> AtualizatLocalizacao(Usuario u)
        {

            try
            {
                Usuario usuario = await _context.TB_USUARIOS
                    .FirstOrDefaultAsync(x => x.Id == u.Id);
                
                usuario.Latitude = u.Latitude;
                usuario.Longitude = u.Longitude;

                var attach = _context.Attach(usuario);
                attach.Property(x => x.Id).IsModified = false;
                attach.Property(x => x.Latitude).IsModified = true;
                attach.Property(x => x.Longitude).IsModified = true;

                int linhasAfetadas = await _context.SaveChangesAsync();

                return Ok(linhasAfetadas);
            }
            catch (System. Exception ex)
            { 
                return BadRequest(ex.Message);
            }
        }
    
    [HttpPut("AtualizarEmail")]
        public async Task<IActionResult> AtualizarEmail(Usuario u)
        {
            try
            {
                Usuario usuario = await _context.TB_USUARIOS
                    .FirstOrDefaultAsync(x => x.Id == u.Id);
                

            usuario.Email = u.Email;

            var attach = _context.Attach(usuario);
            attach.Property(x => x.Id).IsModified = false;
            attach.Property(x => x.Email).IsModified = true;

            int linhasAfetadas = await _context.SaveChangesAsync();
            return Ok(linhasAfetadas);

            }
             catch (System. Exception ex)
            { 
                return BadRequest(ex.Message);
            }
        }

    [HttpPut("AtualizarFoto")]
        public async Task<IActionResult> AtualizarFoto(Usuario u)
        {
            try
            {
                Usuario usuario = await _context.TB_USUARIOS
                    .FirstOrDefaultAsync(x => x.Id == u.Id);
                
                usuario.Foto = u.Foto;

                var attach = _context.Attach(usuario);
                attach.Property(x => x.Id).IsModified = false;
                attach.Property(x => x.Foto).IsModified = true;

                int linhaAfetadas = await _context.SaveChangesAsync();
                return Ok(linhaAfetadas);
            
            } 
             catch (System. Exception ex)
            { 
                return BadRequest(ex.Message);
            }
        }

    [HttpPost]
        public async Task<IActionResult> Add(Arma novaArma)
        {

            try
            {
                if (novaArma.Dano == 0)
                {
                    throw new System.Exception("O dano da Arma não pode ser 0");
                }

                Personagem personagem = await _context.TB_PERSONAGENS
                    .FirstOrDefaultAsync(p => p.Id == novaArma.PersonagemId);

                if (personagem == null)
                {
                    throw new System.Exception("Não contem personagem com esse Id.");
                }

                Arma buscaArma = await _context.TB_ARMAS
                    .FirstOrDefaultAsync(a => a.PersonagemId == novaArma.PersonagemId);

                if (buscaArma != null)
                {
                    throw new System.Exception("o personagem atual ja possue uma arma ");
                }

                await _context.TB_ARMAS.AddAsync(novaArma);
                await _context.SaveChangesAsync();

                return Ok(novaArma.Id);

            }
             catch (System. Exception ex)
            { 
                return BadRequest(ex.Message);
            }
        }


}
