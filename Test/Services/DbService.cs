using System.Data.Common;
using Test.Models.DTOs;
using Microsoft.Data.SqlClient;

namespace Test.Services;

public class DbService : IDbService
{
    private readonly string _connectionString;

    public DbService(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("DefaultConnection");
    }

    public async Task<VisitResponseDto> GetVisitDetailsByIdAsync(int VisitId)
    {
        var query =
            @"SELECT v.date, c.first_name, c.last_name, c.date_of_birth, m.mechanic_id, m.licence_number, s.name, vs.service_fee
                      FROM Visit v
                      JOIN Client c ON v.client_id = c.client_id
                      JOIN Mechanic m ON v.mechanic_id = m.mechanic_id
                      JOIN Visit_Service vs ON v.visit_id = vs.visit_id
                      JOIN Service s ON s.service_id = vs.service_id
                      WHERE v.visit_id = @VisitId";
        await using var con = new SqlConnection(_connectionString);
        await using var cmd = new SqlCommand(query, con);
        cmd.Parameters.AddWithValue("@VisitId", VisitId);
        await con.OpenAsync();
        await using var rdr = await cmd.ExecuteReaderAsync();

        VisitResponseDto? response = null;

        while (await rdr.ReadAsync())
        {
            if (response == null)
            {
                response = new VisitResponseDto
                {
                    Date = rdr.GetDateTime(0),
                    Client = new ClientDto()
                    {
                        FirstName = rdr.GetString(1),
                        LastName = rdr.GetString(2),
                        BirthDate = rdr.GetDateTime(3),
                    },
                    Mechanic = new MechanicDto()
                    {
                        MechanicId = rdr.GetInt32(4),
                        LicenceNumber = rdr.GetString(5),
                    },
                    VisitServices = new List<VisitServicesDto>()
                }; 
            }
            response.VisitServices.Add(new VisitServicesDto()
            {
                Name = rdr.GetString(6),
                ServiceFee = rdr.GetDecimal(7),
            });
        }
        return response;
    
    }
    
    public async Task CreateVisitAsync(CreateVisitDto dto)
    {
        await using var con = new SqlConnection(_connectionString);
        await using var cmd = new SqlCommand();
        cmd.Connection = con;
        await con.OpenAsync();
        
        DbTransaction transaction = await con.BeginTransactionAsync();
        cmd.Transaction = transaction as SqlTransaction;

        try
        {
            //visit already exist
            cmd.CommandText = @"SELECT 1 FROM Visit WHERE visit_id = @VisitId";
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("@VisitId", dto.VisitId);
            var exists = await cmd.ExecuteScalarAsync();
            if (exists is not null)
                throw new Exception("Visit already exist");

            //client exist or not
            cmd.Parameters.Clear();
            cmd.CommandText = @"SELECT 1 FROM Client WHERE client_id = @ClientId";
            cmd.Parameters.AddWithValue("@ClientId", dto.ClientId);
            var isClientExist = await cmd.ExecuteScalarAsync();
            if (isClientExist is null)
                throw new Exception("Client not exist");
            

            //mechanic with the given ln
            cmd.Parameters.Clear();
            cmd.CommandText = @"SELECT mechanic_id FROM Mechanic WHERE licence_number = @LicenceNumber";
            cmd.Parameters.AddWithValue("@LicenceNumber", dto.MechanicLicenceNumber);
            var mechanicIdObj = await cmd.ExecuteScalarAsync();
            if (mechanicIdObj is null)
                throw new Exception("Mechanic not exist");
            int mechanicId = Convert.ToInt32(mechanicIdObj);

            cmd.Parameters.Clear();
            cmd.CommandText = @"INSERT INTO Visit(visit_id, client_id, mechanic_id, date)
                               VALUES(@VisitId, @ClientId, @MechanicId, @Date)";
            cmd.Parameters.AddWithValue("@VisitId", dto.VisitId);
            cmd.Parameters.AddWithValue("@ClientId", dto.ClientId);
            cmd.Parameters.AddWithValue("@MechanicId", mechanicId);
            cmd.Parameters.AddWithValue("@Date", DateTime.Now);
            await cmd.ExecuteNonQueryAsync();
            
            //total service check
            foreach (var service in dto.Services)
            {
                cmd.Parameters.Clear();
                cmd.CommandText = @"SELECT service_id FROM Service WHERE name = @ServiceName";
                cmd.Parameters.AddWithValue("@ServiceName", service.Name);
                var serviceIdObj = await cmd.ExecuteScalarAsync();
                if (serviceIdObj is null)
                    throw new Exception("Service not exist");
                int serviceId = Convert.ToInt32(serviceIdObj);

                cmd.Parameters.Clear();
                cmd.CommandText = @"INSERT INTO Visit_Service(visit_id, service_id, service_fee)
                                    VALUES(@VisitId, @ServiceId, @ServiceFee)";
                cmd.Parameters.AddWithValue("@VisitId", dto.VisitId);
                cmd.Parameters.AddWithValue("@ServiceId", serviceId);
                cmd.Parameters.AddWithValue("@ServiceFee", service.ServiceFee);

                await cmd.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}