using api.models.Entities;
using api.models.Responses;
using api.services.Handlers;
using api.services.Repositores;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;

namespace api.services.v1
{
    public class AreaService : IAreaRepository
    {
        public async Task<List<Area>> GetAll()
        {
            string json = SqliteHandler.GetJson("select id as Id, descripcion as Descripcion from areas order by id");
            return await Task.FromResult(JsonConvert.DeserializeObject<List<Area>>(json) ?? new List<Area>());
        }

        public async Task<Area?> GetById(int id)
        {
            string json = SqliteHandler.GetJson(
                "select id as Id, descripcion as Descripcion from areas where id = @id",
                new SqliteParameter("@id", id));

            var areas = JsonConvert.DeserializeObject<List<Area>>(json);
            return await Task.FromResult(areas?.FirstOrDefault());
        }

        public async Task<GeneralResponse> Create(Area area)
        {
            if (string.IsNullOrWhiteSpace(area.Descripcion))
            {
                return await Task.FromResult(Response(false, 0, "La descripcion es obligatoria"));
            }

            bool success = SqliteHandler.Exec(
                "insert into areas (descripcion) values (@descripcion)",
                new SqliteParameter("@descripcion", area.Descripcion.Trim()));

            return await Task.FromResult(Response(success, success ? 1 : 0, success ? "Area creada correctamente" : "Error al crear el area"));
        }

        public async Task<GeneralResponse> Update(int id, Area area)
        {
            if (await GetById(id) == null)
            {
                return Response(false, 0, "Area no encontrada");
            }

            if (string.IsNullOrWhiteSpace(area.Descripcion))
            {
                return Response(false, 0, "La descripcion es obligatoria");
            }

            bool success = SqliteHandler.Exec(
                "update areas set descripcion = @descripcion where id = @id",
                new SqliteParameter("@descripcion", area.Descripcion.Trim()),
                new SqliteParameter("@id", id));

            return Response(success, success ? 1 : 0, success ? "Area actualizada correctamente" : "Error al actualizar el area");
        }

        public async Task<GeneralResponse> Delete(int id)
        {
            if (await GetById(id) == null)
            {
                return Response(false, 0, "Area no encontrada");
            }

            if (IsAreaUsed(id))
            {
                return Response(false, 0, "No se puede eliminar el area porque tiene tickets asociados");
            }

            bool success = SqliteHandler.Exec(
                "delete from areas where id = @id",
                new SqliteParameter("@id", id));

            return Response(success, success ? 1 : 0, success ? "Area eliminada correctamente" : "Error al eliminar el area");
        }

        private static bool IsAreaUsed(int id)
        {
            string count = SqliteHandler.GetScalar(
                "select count(1) from tickets where area_autor = @id or area_ticket = @id",
                new SqliteParameter("@id", id));

            return int.TryParse(count, out int value) && value > 0;
        }

        private static GeneralResponse Response(bool estado, int codigo, string mensaje)
        {
            return new GeneralResponse { Estado = estado, Codigo = codigo, Mensaje = mensaje };
        }
    }
}
