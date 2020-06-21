using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TwiVoiceWebService.Data;
using TwiVoiceWebService.Models;

namespace TwiVoiceWebService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SampleModelsController : ControllerBase
    {
        private readonly TwiVoiceWebServiceContext _context;

        public SampleModelsController(TwiVoiceWebServiceContext context)
        {
            _context = context;
        }

        // GET: api/SampleModels
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SampleModel>>> GetSampleModel()
        {
            return await _context.SampleModel.ToListAsync();
        }

        // GET: api/SampleModels/5
        [HttpGet("{id}")]
        public async Task<ActionResult<SampleModel>> GetSampleModel(string id)
        {
            var sampleModel = await _context.SampleModel.FindAsync(id);

            if (sampleModel == null)
            {
                return NotFound();
            }

            return sampleModel;
        }

        // PUT: api/SampleModels/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSampleModel(string id, SampleModel sampleModel)
        {
            if (id != sampleModel.Id)
            {
                return BadRequest();
            }

            _context.Entry(sampleModel).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SampleModelExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/SampleModels
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<SampleModel>> PostSampleModel(SampleModel sampleModel)
        {
            _context.SampleModel.Add(sampleModel);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (SampleModelExists(sampleModel.Id))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetSampleModel", new { id = sampleModel.Id }, sampleModel);
        }

        // DELETE: api/SampleModels/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<SampleModel>> DeleteSampleModel(string id)
        {
            var sampleModel = await _context.SampleModel.FindAsync(id);
            if (sampleModel == null)
            {
                return NotFound();
            }

            _context.SampleModel.Remove(sampleModel);
            await _context.SaveChangesAsync();

            return sampleModel;
        }

        private bool SampleModelExists(string id)
        {
            return _context.SampleModel.Any(e => e.Id == id);
        }
    }
}
