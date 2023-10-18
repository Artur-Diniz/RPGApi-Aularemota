using Microsoft.AspNetCore.Mvc;
using RpgApi.Data;
using RpgApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace RpgApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DisputasController : ControllerBase
    {
        private readonly DataContext _context;
        public DisputasController(DataContext context)
        {
            _context = context;
        }
        [HttpPost("Arma")]
        public async Task<IActionResult> AtaqueComArmasAsync(Disputa d)
        {
            try
            {
                Personagem atacante = await _context.TB_PERSONAGENS
                .Include(p => p.Arma)
                .FirstOrDefaultAsync(p => p.Id == d.AtacanteId);
                Personagem oponente = await _context.TB_PERSONAGENS
                .FirstOrDefaultAsync(p => p.Id == d.OponenteId);
                int dano = atacante.Arma.Dano + (new Random().Next(atacante.Forca));
                dano = dano - new Random().Next(oponente.Defesa);
                if(dano > 0)
                oponente.PontosVida = oponente.PontosVida - (int)dano;
                if(oponente.PontosVida <= 0)
                d.Narracao =$"{oponente.Nome} foi derrotado";

                _context.TB_PERSONAGENS.Update(oponente);
                await _context.SaveChangesAsync();
                StringBuilder dados = new StringBuilder();

                dados.AppendFormat("Atacante: {0}.", atacante.Nome);
                dados.AppendFormat("Oponente: {0}.", oponente.Nome);
                dados.AppendFormat("Pontos de Vida do atacante {0}.", atacante.PontosVida);
                dados.AppendFormat("Pontos de Vida do Oponente {0}.", oponente.PontosVida);
                dados.AppendFormat(" Arma Ultilizada: {0}.", atacante.Arma.Nome);
                dados.AppendFormat(" Dano: {0}.", dano);

                d.Narracao += dados.ToString();
                d.DataDisputa = DateTime.Now;
                _context.TB_DISPUTAS.Add(d);
                _context.SaveChanges();

                
                
                return Ok(d);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost("Habilidade")]
        public async Task<IActionResult> AtaqueComHabilidade(Disputa d)
        {
            try
            {
                Personagem atacante = await _context.TB_PERSONAGENS
                .Include(p => p.PersonagemHabilidades)
                .ThenInclude(ph => ph.Habilidade)
                .FirstOrDefaultAsync(p => p.Id == d.AtacanteId);

                Personagem oponente = await _context.TB_PERSONAGENS
                .FirstOrDefaultAsync(p => p.Id == d.OponenteId);

                PersonagemHabilidade ph = await _context.TB_PERSONAGENS_HABILIDADES
                .Include(p => p.Habilidade)
                .FirstOrDefaultAsync(phBusca => phBusca.HabilidadeId == d.HabilidadeId
                && phBusca.PersonagemId == d.AtacanteId);
                return Ok(d);
            }
            catch(System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
    }
}