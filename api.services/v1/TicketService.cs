using api.models.DTO;
using api.models.Entities;
using api.models.Responses;
using api.services.Handlers;
using api.services.Repositores;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;

namespace api.services.v1
{
    public class TicketService : ITicketRepository
    {
        private const string TicketSelect = @"select
                id as Id,
                fecha_alta as FechaAlta,
                autor as Autor,
                status as Status,
                asignado as Asignado,
                progress as Progress,
                titulo as Titulo,
                descripcion as Descripcion,
                area_autor as AreaAutor,
                area_ticket as AreaTicket,
                fecha_update as FechaUpdate,
                visible as Visible
            from tickets";

        public async Task<List<Ticket>> GetAll()
        {
            string json = SqliteHandler.GetJson($"{TicketSelect} where visible = 1 order by id");
            return await Task.FromResult(JsonConvert.DeserializeObject<List<Ticket>>(json) ?? new List<Ticket>());
        }

        public async Task<Ticket?> GetById(int id)
        {
            string json = SqliteHandler.GetJson(
                $"{TicketSelect} where id = @id and visible = 1",
                new SqliteParameter("@id", id));

            var tickets = JsonConvert.DeserializeObject<List<Ticket>>(json);
            return await Task.FromResult(tickets?.FirstOrDefault());
        }

        public async Task<GeneralResponse> Create(TicketDTO ticket)
        {
            GeneralResponse validation = ValidateTicket(ticket);
            if (!validation.Estado)
            {
                return validation;
            }

            string fechaAlta = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            bool visible = IsVisibleStatus(ticket.Status);

            bool success = SqliteHandler.Exec(
                @"insert into tickets
                    (fecha_alta, autor, status, asignado, progress, titulo, descripcion, area_autor, area_ticket, fecha_update, visible)
                  values
                    (@fechaAlta, @autor, @status, @asignado, @progress, @titulo, @descripcion, @areaAutor, @areaTicket, @fechaUpdate, @visible)",
                new SqliteParameter("@fechaAlta", fechaAlta),
                new SqliteParameter("@autor", ticket.Autor),
                new SqliteParameter("@status", ticket.Status),
                NullableParameter("@asignado", OptionalText(ticket.Asignado)),
                new SqliteParameter("@progress", ProgressValue(ticket.Progress)),
                new SqliteParameter("@titulo", ticket.Titulo.Trim()),
                new SqliteParameter("@descripcion", ticket.Descripcion.Trim()),
                new SqliteParameter("@areaAutor", ticket.AreaAutor),
                new SqliteParameter("@areaTicket", ticket.AreaTicket),
                NullableParameter("@fechaUpdate", null),
                new SqliteParameter("@visible", visible ? 1 : 0));

            return Response(success, success ? 1 : 0, success ? "Ticket creado correctamente" : "Error al crear el ticket");
        }

        public async Task<GeneralResponse> Update(int id, TicketDTO ticket)
        {
            Ticket? existing = await GetById(id);
            if (existing == null)
            {
                return Response(false, 0, "Ticket no encontrado");
            }

            GeneralResponse validation = ValidateTicket(ticket);
            if (!validation.Estado)
            {
                return validation;
            }

            string fechaAlta = existing.FechaAlta;
            string fechaUpdate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            bool visible = IsVisibleStatus(ticket.Status);

            bool success = SqliteHandler.Exec(
                @"update tickets set
                    fecha_alta = @fechaAlta,
                    autor = @autor,
                    status = @status,
                    asignado = @asignado,
                    progress = @progress,
                    titulo = @titulo,
                    descripcion = @descripcion,
                    area_autor = @areaAutor,
                    area_ticket = @areaTicket,
                    fecha_update = @fechaUpdate,
                    visible = @visible
                  where id = @id",
                new SqliteParameter("@fechaAlta", fechaAlta),
                new SqliteParameter("@autor", ticket.Autor),
                new SqliteParameter("@status", ticket.Status),
                NullableParameter("@asignado", OptionalText(ticket.Asignado)),
                new SqliteParameter("@progress", ProgressValue(ticket.Progress)),
                new SqliteParameter("@titulo", ticket.Titulo.Trim()),
                new SqliteParameter("@descripcion", ticket.Descripcion.Trim()),
                new SqliteParameter("@areaAutor", ticket.AreaAutor),
                new SqliteParameter("@areaTicket", ticket.AreaTicket),
                new SqliteParameter("@fechaUpdate", fechaUpdate),
                new SqliteParameter("@visible", visible ? 1 : 0),
                new SqliteParameter("@id", id));

            return Response(success, success ? 1 : 0, success ? "Ticket actualizado correctamente" : "Error al actualizar el ticket");
        }

        private static GeneralResponse ValidateTicket(TicketDTO ticket)
        {
            if (string.IsNullOrWhiteSpace(ticket.Titulo))
            {
                return Response(false, 0, "El titulo es obligatorio");
            }

            if (string.IsNullOrWhiteSpace(ticket.Descripcion))
            {
                return Response(false, 0, "La descripcion es obligatoria");
            }

            if (!Exists("AspNetUsers", "Id", ticket.Autor))
            {
                return Response(false, 0, "El autor no existe");
            }

            if (!Exists("status", "id", ticket.Status))
            {
                return Response(false, 0, "El status no existe");
            }

            if (!Exists("areas", "id", ticket.AreaAutor))
            {
                return Response(false, 0, "El area del autor no existe");
            }

            if (!Exists("areas", "id", ticket.AreaTicket))
            {
                return Response(false, 0, "El area del ticket no existe");
            }

            return Response(true, 1, "Validacion correcta");
        }

        private static bool IsVisibleStatus(int statusId)
        {
            string description = SqliteHandler.GetScalar(
                "select lower(descripcion) from status where id = @id",
                new SqliteParameter("@id", statusId));

            return description != "declined" && description != "finished";
        }

        private static bool Exists(string table, string column, int id)
        {
            string count = SqliteHandler.GetScalar(
                $"select count(1) from {table} where {column} = @id",
                new SqliteParameter("@id", id));

            return int.TryParse(count, out int value) && value > 0;
        }

        private static string? OptionalText(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static string ProgressValue(string? progress)
        {
            return string.IsNullOrWhiteSpace(progress) ? "0%" : progress.Trim();
        }

        private static SqliteParameter NullableParameter(string name, object? value)
        {
            return new SqliteParameter(name, value ?? DBNull.Value);
        }

        private static GeneralResponse Response(bool estado, int codigo, string mensaje)
        {
            return new GeneralResponse { Estado = estado, Codigo = codigo, Mensaje = mensaje };
        }
    }
}
