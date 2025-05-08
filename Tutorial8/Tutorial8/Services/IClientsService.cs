using Microsoft.AspNetCore.Mvc;
using Tutorial8.Models.DTOs;

namespace Tutorial8.Services;

public interface IClientsService
{
    Task<List<ClientDTO>> GetClients();
    Task<bool> DoesClientExist(int clientId);
    Task<List<ClientTripDTO>> GetClientsTrips(int id);
    Task<int> AddClient(ClientCreateDTO clientDto);
    Task<bool> DoesPeselExist(string pesel);
    // Task<IActionResult> RegisterClientForTrip(int clientId, int tripId);
    Task RegisterClientForTrip(int clientId, int tripId);
    Task RemoveClientFromTrip(int clientId, int tripId);
}