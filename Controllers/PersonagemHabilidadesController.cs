using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using RpgApi.Models;
using RpgApi.Data;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;


namespace RpgApi.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class PersonagemHabilidadesController : ControllerBase
    {

        private readonly DataContext _context;

        public PersonagemHabilidadesController(DataContext context)
        {

            _context = context;
        }

       [HttpPost]
        public async Task<IActionResult> AddPersonagemHabilidadeAsync(PersonagemHabilidade novoPersonagemHabilidade)
        {
            try
            {
                Personagem personagem = await _context.TB_PERSONAGENS
                .Include(p => p.Arma)
                .Include(p => p.PersonagemHabilidades).ThenInclude(ps => ps.Habilidade)
                .FirstOrDefaultAsync(p => p.Id == novoPersonagemHabilidade.PersonagemId);

                if (personagem == null)
                {
                    throw new System.Exception("Personagem com esse Id não foi identificado.");
                }
                Habilidade habilidade = await _context.TB_HABILIDADES
                                        .FirstOrDefaultAsync(h => h.Id == novoPersonagemHabilidade.HabilidadeId);

                if (habilidade == null)
                {
                    throw new System.Exception("Habilidade não encontrada.");
                }
                    PersonagemHabilidade ph = new PersonagemHabilidade();
                    ph.Personagem = personagem;
                    ph.Habilidade = habilidade;
                    await _context.TB_PERSONAGENS_HABILIDADES.AddAsync(ph);
                    int linhasAfetadas = await _context.SaveChangesAsync();


                    return Ok(linhasAfetadas);
                
   
            }    
            catch (System.Exception ex)
            {

                return BadRequest(ex.Message);
            }

        }

        [HttpGet("GetbyId/{id}")]
        public async Task<IActionResult> ListaPersonagemHabilidades(int Id )
        {
            try
            {
               
                    List<PersonagemHabilidade> Lista = await _context.TB_PERSONAGENS_HABILIDADES.ToListAsync();
                       
                
                    return Ok(Lista);

                
                   
                


            }
            catch(System.Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        [HttpGet("GetHabilidades")]
        public async Task<IActionResult> ListaHabilidades( PersonagemHabilidade Habilidade )
        {

            try
            {   
                List<PersonagemHabilidade> habilidade = await _context.TB_PERSONAGENS_HABILIDADES.ToListAsync();
                return Ok (Habilidade);

            }
            catch(System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // [HttpPost("DeletePersonagemHabilidade")]
        // public async Task<IActionResult> DeletePH( Personagem id)
        // {

        //     try
        //     {


        //     }
        //     catch(System.Exception ex)
        //     {
        //         return BadRequest(ex.Message);
        //     }
        // }
    }
 }
 

    

