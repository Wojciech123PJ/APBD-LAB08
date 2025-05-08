using Microsoft.Data.SqlClient;
using Tutorial8.Models.DTOs;

namespace Tutorial8.Services;

public class TripsService : ITripsService
{
    // private readonly string _connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=APBD;Integrated Security=True;";
    private readonly string _connectionString = "Data Source=localhost, 1433; User=SA; Password=yourStrong(!)Password; Initial Catalog=master; Integrated Security=False;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False";
    
    public async Task<List<TripDTO>> GetTrips()
    {
        var trips = new List<TripDTO>();

        // string command = "SELECT IdTrip, Name FROM Trip";
        string query = "SELECT t.IdTrip, t.Name, t.Description, t.DateFrom, t.DateTo, t.MaxPeople, c.Name AS CountryName FROM Trip t LEFT JOIN Country_Trip ct ON t.IdTrip = ct.IdTrip LEFT JOIN Country c ON c.IdCountry = ct.IdCountry ORDER BY t.IdTrip";
        
        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(query, conn))
        {
            await conn.OpenAsync();

            using (var reader = await cmd.ExecuteReaderAsync())
            {
                
                while (await reader.ReadAsync())
                    /*
                       while (await reader.ReadAsync())
                       Używane, gdy oczekujesz wielu rekordów.
                       Pętla działa dopóki są kolejne wiersze do odczytania.
                       Typowe dla zapytań SELECT, które zwracają listę wyników.
                     */
                {
                    
                    /*
                    // int idOrdinal = reader.GetOrdinal("IdTrip");
                    trips.Add(new TripDTO()
                    {
                        // Id = reader.GetInt32(idOrdinal),
                        Id = idTrip,
                        Name = reader.GetString(1),
                        description = reader.GetString(2),
                        StartDate = reader.GetDateTime(3),
                        EndDate = reader.GetDateTime(4),
                        maxPeople = reader.GetInt32(5),
                        Countries = new List<CountryDTO>()
                    });
                    */
                    // int idTrip = reader.GetOrdinal("IdTrip");
                    int idTrip = reader.GetInt32(reader.GetOrdinal("IdTrip"));
                    var existingTrip = trips.FirstOrDefault(t => t.Id == idTrip);
                    if (existingTrip == null)
                    {
                        existingTrip = new TripDTO
                        {
                            Id = idTrip,
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            description = reader.GetString(reader.GetOrdinal("Description")),
                            StartDate = reader.GetDateTime(reader.GetOrdinal("DateFrom")),
                            EndDate = reader.GetDateTime(reader.GetOrdinal("DateTo")),
                            maxPeople = reader.GetInt32(reader.GetOrdinal("MaxPeople")),
                            Countries = new List<CountryDTO>()
                        };
                        trips.Add(existingTrip);
                    }

                    
                    if (!reader.IsDBNull(reader.GetOrdinal("CountryName")))
                    {
                        existingTrip.Countries.Add(new CountryDTO
                        {
                            Name = reader.GetString(reader.GetOrdinal("CountryName"))
                        });
                    }
                }
            }
        }
        return trips;
    }

    public async Task<bool> DoesTripExist(int tripId)
    {
        // string query = "SELECT * FROM Trip WHERE IdTrip = @tripId";
        string query = "SELECT 1 FROM Trip WHERE IdTrip = @tripId";
        
        await using var conn = new SqlConnection(_connectionString);
        await using var cmd = new SqlCommand(query, conn);
        
        cmd.Parameters.AddWithValue("@tripId", tripId);
        
        await conn.OpenAsync();
        
        var result = (int?)await cmd.ExecuteScalarAsync();
        
        return result > 0;
    }

    public async Task<bool> IsTripFull(int tripId)
    {
        string query = @"SELECT 
                        (SELECT COUNT(*) FROM Client_Trip WHERE IdTrip = @tripId) AS CurrentCount,
                        (SELECT MaxPeople FROM Trip WHERE IdTrip = @tripId) AS MaxPeople;";
        
        await using var conn = new SqlConnection(_connectionString);
        await using var cmd = new SqlCommand(query, conn);
        await conn.OpenAsync();
        cmd.Parameters.AddWithValue("@tripId", tripId);
        
        using var reader = await cmd.ExecuteReaderAsync();
        
        /*
           Używane, gdy oczekujesz maksymalnie jednego rekordu.
           Sprawdza tylko pierwszy wynik — przydaje się np. do sprawdzenia istnienia wiersza albo pobrania jednego elementu.
         */
        if (await reader.ReadAsync())
        {
            int currentCount = reader.GetInt32(reader.GetOrdinal("CurrentCount"));
            int maxPeople = reader.GetInt32(reader.GetOrdinal("MaxPeople"));
            
            return currentCount >= maxPeople;
        }

        return true;
    }


    public async Task<bool> IsClientRegisteredForTrip(int clientId, int tripId)
    {
        string query = @"SELECT *
                        FROM Client_Trip
                        WHERE IdClient = @clientId AND IdTrip = @tripID;";
        
        await using var conn = new SqlConnection(_connectionString);
        await using var cmd = new SqlCommand(query, conn);
        
        cmd.Parameters.AddWithValue("@clientId", clientId);
        cmd.Parameters.AddWithValue("@tripId", tripId);
        
        await conn.OpenAsync();
        
        var result = (int?)await cmd.ExecuteScalarAsync();
        
        return result > 0;
    }
}


/*
 * | Scenariusz                      | Użyj                       |
   | ------------------------------- | -------------------------- |
   | Pobierasz wiele klientów        | `while`                    |
   | Pobierasz tylko 1 klienta po ID | `if`                       |
   | Sprawdzasz istnienie rekordu    | `if` + `return true/false` |
   
*/