using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Tutorial8.Models.DTOs;
using Tutorial8.Services;

namespace Tutorial8.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientsController : ControllerBase
    {
        private readonly IClientsService _clientsService;
        private readonly ITripsService _tripsService;   // można tak????

        public ClientsController(IClientsService clientsService, ITripsService tripsService)
        {
            _clientsService = clientsService;
            _tripsService = tripsService;
        }

        
        [HttpGet]
        public async Task<IActionResult> GetClients()
        {
            var clients = await _clientsService.GetClients();
            return Ok(clients);
        }

        
        // GET api/clients/{id}/trips
        // Ten endpoint będzie pobierał wszystkie wycieczki powiązane z konkretnym klientem.
        [HttpGet("{id}/trips")]
        public async Task<IActionResult> GetClientsTrips(int id)
        {
            if (!await _clientsService.DoesClientExist(id))
            {
                // return NotFound();
                return NotFound($"Client with ID {id} does not exist.");
            }
            
            var trips = await _clientsService.GetClientsTrips(id);
            
            if (trips == null || !trips.Any())
            {
                return NotFound($"Client with ID {id} is not registered for any trips.");
            }
            
            return Ok(trips);
        }

        // Ten endpoint utworzy nowy rekord klienta.
        // POST api/clients
        [HttpPost]
        public async Task<IActionResult> AddClient(ClientCreateDTO clientDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            
            if (await _clientsService.DoesPeselExist(clientDto.Pesel))
                return Conflict($"Pesel {clientDto.Pesel} already exists.");

            try
            {
                int newClientId = await _clientsService.AddClient(clientDto);
                return CreatedAtAction(nameof(GetClients), new { id = newClientId }, new { Id = newClientId });
                // return Ok(newClientId);
            }
            catch (SqlException)
            {
                return StatusCode(500, "Database error occurred.");
            }
        }
        
        
        // PUT /api/clients/{id}/trips/{tripId}
        // Ten endpoint zarejestruje klienta na konkretną wycieczkę.
        [HttpPut("{id}/trips/{tripId}")]
        public async Task<IActionResult> RegisterClientForTrip(int id, int tripId)
        {
            try
            {
                if (!await _clientsService.DoesClientExist(id))
                    return NotFound($"Client with ID {id} doesn't exists.");

                if (!await _tripsService.DoesTripExist(tripId))
                    return NotFound($"Trip with ID {tripId} doesn't exists.");

                if (await _tripsService.IsTripFull(tripId))
                    return Conflict($"Trip with ID {tripId} is full."); // Conflict czy Bad Request?
                
                if (await _tripsService.IsClientRegisteredForTrip(id, tripId))
                    return Conflict($"Client is already registered for trip {tripId}.");

                await _clientsService.RegisterClientForTrip(id, tripId);

                return Created($"api/clients/{id}/trips/{tripId}", new { Message = "Successfully registered client for the trip." }); // ??
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An internal error occurred: {ex.Message}");
            }
        }
        
                
        // DELETE /api/clients/{id}/trips/{tripId}
        // Ten endpoint usunie rejestrację klienta z wycieczki.
        [HttpDelete("{id}/trips/{tripId}")]
        public async Task<IActionResult> RemoveClientFromTrip(int id, int tripId)
        {
            try
            {
                if (!await _clientsService.DoesClientExist(id))
                    return NotFound($"Client with ID {id} doesn't exists.");

                if (!await _tripsService.DoesTripExist(tripId))
                    return NotFound($"Trip with ID {tripId} doesn't exists.");

                if (!await _tripsService.IsClientRegisteredForTrip(id, tripId))
                    return NotFound($"Client with ID {id} is not registered for trip with ID {tripId}."); // ?
                
                
                await _clientsService.RemoveClientFromTrip(id, tripId);

                return Ok(new { Message = $"Client with ID {id} removed from trip with ID {tripId}." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An internal error occurred: {ex.Message}");
            }
        }
    }
}