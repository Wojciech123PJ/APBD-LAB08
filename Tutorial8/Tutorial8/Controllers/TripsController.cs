using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Tutorial8.Services;

namespace Tutorial8.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TripsController : ControllerBase
    {
        private readonly ITripsService _tripsService;

        public TripsController(ITripsService tripsService)
        {
            _tripsService = tripsService;
        }

        // Ten endpoint będzie pobierał wszystkie dostępne wycieczki wraz z ich podstawowymi informacjami
        [HttpGet]
        public async Task<IActionResult> GetTrips()
        {
            var trips = await _tripsService.GetTrips();
            return Ok(trips);
        }

        // [HttpGet("{id}")]
        // public async Task<IActionResult> GetTrip(int id)
        // {
        //     // if( await DoesTripExist(id)){
        //     //  return NotFound();
        //     // }
        //     // var trip = ... GetTrip(id);
        //     var trip = GetTrips();
        //     return Ok();
        // }
    }
}
