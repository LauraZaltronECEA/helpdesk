using api.models.Entities;
using api.models.Responses;
using api.services.Handlers;
using api.services.Repositores;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;

namespace api.services.v1
{
    public class StatusService : IStatusRepository
    {
        public async Task<List<Status>> GetAll()
        {
            string json = SqliteHandler.GetJson("select id as Id, descripcion as Descripcion from status order by id");
            return await Task.FromResult(JsonConvert.DeserializeObject<List<Status>>(json) ?? new List<Status>());
        }

        public async Task<Status?> GetById(int id)
        {
            string json = SqliteHandler.GetJson(
                "select id as Id, descripcion as Descripcion from status where id = @id",
                new SqliteParameter("@id", id));

            var statuses = JsonConvert.DeserializeObject<List<Status>>(json);
            return await Task.FromResult(statuses?.FirstOrDefault());
        }

        public async Task<GeneralResponse> Create(Status status)
        {
            if (string.IsNullOrWhiteSpace(status.Descripcion))
            {
                return await Task.FromResult(Response(false, 0, "La descripcion es obligatoria"));
            }

            bool success = SqliteHandler.Exec(
                "insert into status (descripcion) values (@descripcion)",
                new SqliteParameter("@descripcion", status.Descripcion.Trim()));

            return await Task.FromResult(Response(success, success ? 1 : 0, success ? "Status creado correctamente" : "Error al crear el status"));
        }

        public async Task<GeneralResponse> Update(int id, Status status)
        {
            if (await GetById(id) == null)
            {
                return Response(false, 0, "Status no encontrado");
            }

            if (string.IsNullOrWhiteSpace(status.Descripcion))
            {
                return Response(false, 0, "La descripcion es obligatoria");
            }

            bool success = SqliteHandler.Exec(
                "update status set descripcion = @descripcion where id = @id",
                new SqliteParameter("@descripcion", status.Descripcion.Trim()),
                new SqliteParameter("@id", id));

            return Response(success, success ? 1 : 0, success ? "Status actualizado correctamente" : "Error al actualizar el status");
        }

        public async Task<GeneralResponse> Delete(int id)
        {
            if (await GetById(id) == null)
            {
                return Response(false, 0, "Status no encontrado");
            }

            if (IsStatusUsed(id))
            {
                return Response(false, 0, "No se puede eliminar el status porque tiene tickets asociados");
            }

            bool success = SqliteHandler.Exec(
                "delete from status where id = @id",
                new SqliteParameter("@id", id));

            return Response(success, success ? 1 : 0, success ? "Status eliminado correctamente" : "Error al eliminar el status");
        }

        private static bool IsStatusUsed(int id)
        {
            string count = SqliteHandler.GetScalar(
                "select count(1) from tickets where status = @id",
                new SqliteParameter("@id", id));

            return int.TryParse(count, out int value) && value > 0;
        }

        private static GeneralResponse Response(bool estado, int codigo, string mensaje)
        {
            return new GeneralResponse { Estado = estado, Codigo = codigo, Mensaje = mensaje };
        }
    }
}
