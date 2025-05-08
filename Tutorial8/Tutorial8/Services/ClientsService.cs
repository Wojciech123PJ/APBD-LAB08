using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Tutorial8.Models.DTOs;

namespace Tutorial8.Services;

public class ClientsService : IClientsService
{
    // private readonly string _connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=APBD;Integrated Security=True;";
    private readonly string _connectionString = "Data Source=localhost, 1433; User=SA; Password=yourStrong(!)Password; Initial Catalog=master; Integrated Security=False;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False";
    
    public async Task<List<ClientDTO>> GetClients()
    {
        var clients = new List<ClientDTO>();

        // string command = "SELECT IdTrip, Name FROM Trip";
        string query = "SELECT * FROM Client";
        
        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(query, conn))
        {
            await conn.OpenAsync();

            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    int idOrdinal = reader.GetOrdinal("IdClient");
                    clients.Add(new ClientDTO()
                    {
                        Id = reader.GetInt32(idOrdinal),
                        // FirstName = reader.GetString(1),
                        FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                        // LastName = reader.GetString(2),
                        LastName = reader.GetString(reader.GetOrdinal("LastName")),
                        // Email = reader.GetString(3),
                        Email = reader.GetString(reader.GetOrdinal("Email")),
                        // Telephone = reader.GetString(4),
                        Telephone = reader.GetString(reader.GetOrdinal("Telephone")),
                        // Pesel = reader.GetString(5),
                        Pesel = reader.GetString(reader.GetOrdinal("Pesel")),
                    });
                }
            }
        }

        return clients;
    }

    public async Task<bool> DoesClientExist(int id)
    {
        string query = "SELECT * FROM Client WHERE IdClient = @id";
        
        await using var conn = new SqlConnection(_connectionString);
        await using var cmd = new SqlCommand(query, conn);
        
        cmd.Parameters.AddWithValue("@id", id);
        
        await conn.OpenAsync();
        
        var result = (int?)await cmd.ExecuteScalarAsync();
        
        return result > 0;
        // return result != null;
    }

    public async Task<List<ClientTripDTO>> GetClientsTrips(int id)
    {
        string query = @"SELECT t.IdTrip, t.Name, t.Description, t.DateFrom, t.DateTo, t.MaxPeople, ct.RegisteredAt, ct.PaymentDate
                        FROM Trip t
                        JOIN Client_Trip ct ON t.IdTrip = ct.IdTrip
                        WHERE ct.IdClient = @id";
        
        await using var conn = new SqlConnection(_connectionString);
        await using var cmd = new SqlCommand(query, conn);
        
        cmd.Parameters.AddWithValue("@id", id);
        
        await conn.OpenAsync();
        
        using var reader = await cmd.ExecuteReaderAsync();
        
        var trips = new List<ClientTripDTO>();
        while (await reader.ReadAsync())
        {
            trips.Add(new ClientTripDTO
            {
                Name = reader.GetString(reader.GetOrdinal("Name")),
                Description = reader.GetString(reader.GetOrdinal("Description")),
                DateFrom = reader.GetDateTime(reader.GetOrdinal("DateFrom")),
                DateTo = reader.GetDateTime(reader.GetOrdinal("DateTo")),
                MaxPeople = reader.GetInt32(reader.GetOrdinal("MaxPeople")),
                
                // PaymentDate = reader.GetInt32(reader.GetOrdinal("PaymentDate")),
                // RegisteredAt = reader.GetInt32(reader.GetOrdinal("RegisteredAt")),
                
                // PaymentDate = reader.IsDBNull(reader.GetOrdinal("PaymentDate")) 
                //     ? (DateTime?)null 
                //     : reader.GetDateTime(reader.GetOrdinal("PaymentDate")),
                //
                // RegisteredAt = reader.IsDBNull(reader.GetOrdinal("RegisteredAt")) 
                //     ? (DateTime?)null 
                //     : reader.GetDateTime(reader.GetOrdinal("RegisteredAt")),
                
                PaymentDate = reader.IsDBNull(reader.GetOrdinal("PaymentDate"))
                    ? (int?)null
                    : reader.GetInt32(reader.GetOrdinal("PaymentDate")),
                
                RegisteredAt = reader.IsDBNull(reader.GetOrdinal("RegisteredAt"))
                    ? (int?)null
                    : reader.GetInt32(reader.GetOrdinal("RegisteredAt")),
            });
        }
        return trips;
    }

    public async Task<int> AddClient(ClientCreateDTO clientDto)
    {
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        string query = @"
                        INSERT INTO Client (FirstName, LastName, Email, Telephone, Pesel)
                        OUTPUT INSERTED.IdClient
                        VALUES (@FirstName, @LastName, @Email, @Telephone, @Pesel);";
        
        await using var cmd = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@FirstName", clientDto.FirstName);
        cmd.Parameters.AddWithValue("@LastName", clientDto.LastName);
        cmd.Parameters.AddWithValue("@Email", clientDto.Email);
        cmd.Parameters.AddWithValue("@Telephone", clientDto.Telephone);
        cmd.Parameters.AddWithValue("@Pesel", clientDto.Pesel);
        
        var newId = (int)await cmd.ExecuteScalarAsync();
        return newId;
    }
    
    public async Task<bool> DoesPeselExist(string pesel)
    {
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        const string query = "SELECT COUNT(1) FROM Client WHERE Pesel = @Pesel";
        await using var cmd = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@Pesel", pesel);

        var result = (int?)await cmd.ExecuteScalarAsync();
        
        return result > 0;
    }

    public async Task RegisterClientForTrip(int clientId, int tripId)
    // public async Task<IActionResult> RegisterClientForTrip(int clientId, int tripId)
    {
        string query = @"
                        INSERT INTO Client_Trip (IdClient, IdTrip, RegisteredAt, PaymentDate)
                        VALUES (@clientId, @tripId, @now, null)";
        
        await using var conn = new SqlConnection(_connectionString);
        await using var cmd = new SqlCommand(query, conn);
        await conn.OpenAsync();
        cmd.Parameters.AddWithValue("@clientId", clientId);
        cmd.Parameters.AddWithValue("@tripId", tripId);
        
        string formattedDate = DateTime.Now.ToString("yyyyMMdd");
        int registeredAtInt = int.Parse(formattedDate);
        cmd.Parameters.AddWithValue("@now", registeredAtInt);
        
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task RemoveClientFromTrip(int clientId, int tripId)
    {
        string query = @"
                        DELETE FROM Client_Trip
                        WHERE IdClient = @clientId AND IdTrip = @tripId";
        
        await using var conn = new SqlConnection(_connectionString);
        await using var cmd = new SqlCommand(query, conn);
        await conn.OpenAsync();
        cmd.Parameters.AddWithValue("@clientId", clientId);
        cmd.Parameters.AddWithValue("@tripId", tripId);
        
        await cmd.ExecuteNonQueryAsync();
    }
}