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
    public class TwiRequestsController : ControllerBase
    {
        private readonly TwiVoiceWebServiceContext _context;

        public TwiRequestsController(TwiVoiceWebServiceContext context)
        {
            _context = context;
        }

        // GET: api/TwiRequests
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TwiRequest>>> GetTwiRequest()
        {
            return await _context.TwiRequest.ToListAsync();
        }

        // GET: api/TwiRequests/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TwiRequest>> GetTwiRequest(Guid id)
        {
            var twiRequest = await _context.TwiRequest.FindAsync(id);

            if (twiRequest == null)
            {
                return NotFound();
            }

            return twiRequest;
        }

        // PUT: api/TwiRequests/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTwiRequest(Guid id, TwiRequest twiRequest)
        {
            if (id != twiRequest.Id)
            {
                return BadRequest();
            }

            _context.Entry(twiRequest).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TwiRequestExists(id))
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

        // POST: api/TwiRequests
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<TwiRequest>> PostTwiRequest(TwiRequest twiRequest)
        {
            //_context.TwiRequest.Add(twiRequest);
            //await _context.SaveChangesAsync();

            return CreatedAtAction("GetTwiRequest", new { id = twiRequest.Id }, twiRequest);
        }

        // DELETE: api/TwiRequests/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<TwiRequest>> DeleteTwiRequest(Guid id)
        {
            var twiRequest = await _context.TwiRequest.FindAsync(id);
            if (twiRequest == null)
            {
                return NotFound();
            }

            _context.TwiRequest.Remove(twiRequest);
            await _context.SaveChangesAsync();

            return twiRequest;
        }

        private bool TwiRequestExists(Guid id)
        {
            return _context.TwiRequest.Any(e => e.Id == id);
        }
    }
}
