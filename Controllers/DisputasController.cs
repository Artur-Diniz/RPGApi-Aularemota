using Microsoft.AspNetCore.Mvc;
using RpgApi.Data;
using RpgApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Linq;

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
                if (dano > 0)
                    oponente.PontosVida = oponente.PontosVida - (int)dano;
                if (oponente.PontosVida <= 0)
                    d.Narracao = $"{oponente.Nome} foi derrotado";

                _context.TB_PERSONAGENS.Update(oponente);
                await _context.SaveChangesAsync();
                StringBuilder dados = new StringBuilder();

                _context.TB_PERSONAGENS.Update(oponente);
                await _context.SaveChangesAsync();

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
        public async Task<IActionResult> AtaqueComHabilidadeAsync(Disputa d)
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

                if (ph != null)
                {
                    int dano = ph.Habilidade.Dano + (new Random().Next(atacante.Inteligencia));
                    dano = dano - new Random().Next(oponente.Defesa);

                    if (dano > 0)
                    {
                        oponente.PontosVida = oponente.PontosVida - dano;
                    }
                    if (oponente.PontosVida <= 0)
                    {
                        d.Narracao += $"{oponente.Nome} foi derrotado";
                    }



                    StringBuilder dados = new StringBuilder();
                    dados.AppendFormat("atacante: {0}.", atacante.Nome);
                    dados.AppendFormat("Oponete: {0}.", oponente.Nome);
                    dados.AppendFormat("Pontos de vida do atacante: {0}", atacante.PontosVida);
                    dados.AppendFormat("Pontos de vida do oponente:{0}", oponente.PontosVida);
                    dados.AppendFormat("Habilidade Ultilizada {0}", ph.Habilidade.Nome);
                    dados.AppendFormat("Dano {0} ", dano);

                    _context.TB_PERSONAGENS.Update(oponente);
                    await _context.SaveChangesAsync();

                    d.Narracao += dados.ToString();
                    d.DataDisputa = DateTime.Now;
                    _context.TB_DISPUTAS.Add(d);
                    _context.SaveChanges();

                }
                else
                {


                    d.Narracao = $"{atacante.Nome} não possui esta Habilidade";
                }


                return Ok(d);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    // [HttpPost("DisputaEmgrupo1")]
    //     public async 
            
   [HttpPost("DisputaEmGrupo")]
        public async Task<IActionResult> DisputaEmGrupoAsync(Disputa d)
        {
            try
            {

                d.Resultados = new List<String>(); // para instanciar

                // busca na base dos personagens informados no parametro de armas e habilidades
                List<Personagem> personagens = await _context.TB_PERSONAGENS
                    .Include(p => p.Arma)
                    .Include(p => p.PersonagemHabilidades).ThenInclude(ph => ph.Habilidade)
                    .Where(p => d.ListaIdpersonagem.Contains(p.Id)).ToListAsync();

                //Contagem de personagens ainda vivos
                int qtdPersonagensVivos = personagens.FindAll(p => p.PontosVida > 0).Count;

                //um while para contar até o ultimo personagem 
                while (qtdPersonagensVivos > 1)
                {
                    // seleciona personagens atcantes + o sorteio 
                    List<Personagem> atacantes = personagens.Where(p => p.PontosVida > 0).ToList();
                    Personagem atacante = atacantes[new Random().Next(atacantes.Count)];
                    d.AtacanteId = atacante.Id;

                    // seleciona personagens oponentes + sorteio
                    List<Personagem> oponentes = personagens.Where(p => p.Id != atacante.Id && p.PontosVida > 0).ToList();
                    Personagem oponente = oponentes[new Random().Next(oponentes.Count)];
                    d.OponenteId = oponente.Id;

                    //redefine os valores da variaveis a cada while
                    int dano = 0;
                    string ataqueUsado = string.Empty;
                    string resultado = string.Empty;

                    //Sorteio entre ataque com arma ou habilidade
                    bool ataqueUsaArma = new Random().Next(1) == 0;

                    if (ataqueUsaArma && atacante.Arma != null)
                    {

                        //sorteio de força
                        dano = atacante.Arma.Dano + new Random().Next(atacante.Forca);
                        dano = dano - new Random().Next(oponente.Defesa);
                        ataqueUsado = atacante.Arma.Nome;

                        if (dano > 0)
                            oponente.PontosVida = oponente.PontosVida - (int)dano;

                        // formatação da mensagem 
                        resultado =
                            string.Format($"{atacante.Nome} atacou {oponente.Nome} usando{ataqueUsado} com o dano {dano}");
                        d.Narracao += resultado; // concatena os resultados.
                        d.Resultados.Add(resultado); // concatena os resultados.


                    }
                    else if (atacante.PersonagemHabilidades.Count != 0)
                    {
                        // sorteia uma habilidade e seleciona
                        int sorteioHabilidadeId = new Random().Next(atacante.PersonagemHabilidades.Count);
                        Habilidade habilidadeEscolhida = atacante.PersonagemHabilidades[sorteioHabilidadeId].Habilidade;
                        ataqueUsado = habilidadeEscolhida.Nome;

                        // Sorteio da da inteligência somada ao dano 
                        dano = habilidadeEscolhida.Dano + new Random().Next(atacante.Inteligencia);
                        dano = dano - new Random().Next(oponente.Defesa);

                        if (dano > 0)
                        {
                            oponente.PontosVida = oponente.PontosVida - (int)dano;
                        }

                        resultado =
                            string.Format($"{atacante.Nome} atacou {oponente.Nome} usando{ataqueUsado} com o dano {dano}");
                        d.Narracao += resultado; // concatena os resultados.
                        d.Resultados.Add(resultado); // concatena os resultados.
                    }

                    if (!string.IsNullOrEmpty(ataqueUsado))
                    {
                        atacante.Vitorias++;
                        oponente.Derrotas++;
                        atacante.Disputas++;
                        oponente.Disputas++;

                        d.id = 0;
                        d.DataDisputa = DateTime.Now;
                        _context.TB_DISPUTAS.Add(d);
                        await _context.SaveChangesAsync();

                    }

                    qtdPersonagensVivos = personagens.FindAll(p => p.PontosVida > 0).Count;

                    if (qtdPersonagensVivos == 1)
                    {
                        string resultadoFinal =
                            $"{atacante.Nome.ToUpper()} é Campeão com {atacante.PontosVida} pontos de vida restantes!";

                        d.Narracao += resultadoFinal;
                        d.Resultados.Add(resultadoFinal);

                        break;
                    }
                }
                

                // atualiza os pontos de vida dos personagens 
                _context.TB_PERSONAGENS.UpdateRange(personagens);
                await _context.SaveChangesAsync();

                return Ok(d);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }}



        [HttpDelete("ApagarDisputas")]
        public async Task<IActionResult> DeleteAsync()
        {
            try
            {
                List<Disputa> disutas = await _context.TB_DISPUTAS.ToListAsync();

                _context.TB_DISPUTAS.RemoveRange(disutas);
                await _context.SaveChangesAsync();

                return Ok("Disputas apagadas");
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("Listar")]
        public async Task<IActionResult> ListarAsync()
        {

            try
            {
                List<Disputa> disputas =
                 await _context.TB_DISPUTAS.ToListAsync();

                return Ok(disputas);

            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("PersonagemRandom")]
        public async Task<IActionResult> Soretio()
        {

            List<Personagem> personagens = 
                await _context.TB_PERSONAGENS.ToListAsync();

            int sorteio = new Random().Next(personagens.Count);

            Personagem p = personagens[sorteio];

            string msg =
                string.Format($"Nº Sorteado {sorteio}. Personagem {p.Nome}");
            
            return Ok();
        }

      
        
    }

}