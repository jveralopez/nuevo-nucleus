namespace NucleusRH.Shared;

/// <summary>
/// Resultado de una operacion con detalles de error
/// </summary>
public class OperationResult<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Error { get; set; }
    public string? ErrorDetail { get; set; }
    public string? ErrorCode { get; set; }

    public static OperationResult<T> Ok(T data) => new() { Success = true, Data = data };
    public static OperationResult<T> Fail(string error, string? detail = null, string? code = null) 
        => new() { Success = false, Error = error, ErrorDetail = detail, ErrorCode = code };
}

/// <summary>
/// Manejo de errores detallado por tipo de excepcion
/// </summary>
public static class ErrorHandler
{
    public static (string Error, string Detail, string Code) GetErrorDetails(Exception ex, string? context = null)
    {
        return ex switch
        {
            // Errores de base de datos
            Microsoft.EntityFrameworkCore.DbUpdateException dbEx =>
                HandleDbUpdateException(dbEx, context),
            
            // Errores de validacion
            ArgumentException argEx =>
                ("Parámetro inválido", $"{argEx.Message}", "INVALID_ARGUMENT"),
            
            // Errores de clave duplicada
            Microsoft.EntityFrameworkCore.InMemoryDbInMemoryDatabaseServiceException _ when ex.Message.Contains("duplicate") =>
                ("Registro duplicado", $"Ya existe un registro con los mismos datos únicos. Verifique que el {context} no esté repetido.", "DUPLICATE_KEY"),
            
            // Errores de timeout
            TimeoutException _ =>
                ("Tiempo de espera agotado", "La operación tardó demasiado. Intente nuevamente.", "TIMEOUT"),
            
            // Errores de HTTP
            HttpRequestException httpEx =>
                HandleHttpException(httpEx, context),
            
            // Error de recurso no encontrado
            KeyNotFoundException _ =>
                ("Recurso no encontrado", $"El {context} solicitado no existe o fue eliminado.", "NOT_FOUND"),
            
            // Error de validacion de negocio
            InvalidOperationException opEx when opEx.Message.Contains("cannot") || opEx.Message.Contains("invalid") =>
                ("Operación no válida", opEx.Message, "INVALID_OPERATION"),
            
            // Error generico
            _ => ("Error interno", FormatDetailedError(ex), "INTERNAL_ERROR")
        };
    }

    private static (string Error, string Detail, string Code) HandleDbUpdateException(Microsoft.EntityFrameworkCore.DbUpdateException ex, string? context)
    {
        // Analizar la excepcion para dar mensajes especificos
        var innerMessage = ex.InnerException?.Message ?? ex.Message;

        if (innerMessage.Contains("UNIQUE constraint") || innerMessage.Contains("duplicate"))
        {
            // Extraer que campo causo el error
            var fieldMatch = System.Text.RegularExpressions.Regex.Match(innerMessage, @"(column|field):\s*(\w+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            var field = fieldMatch.Success ? fieldMatch.Groups[2].Value : "registro";
            
            return ("Registro duplicado", 
                    $"Ya existe un {context} con este valor. Verifique que los datos únicos (como código, número o identificación) no estén repetidos.", 
                    "DUPLICATE_KEY");
        }

        if (innerMessage.Contains("FOREIGN KEY constraint"))
        {
            return ("Referencia inválida", 
                    $"No se puede {context} porque hace referencia a un registro que no existe. Verifique que el ID de relación sea válido.", 
                    "FOREIGN_KEY_VIOLATION");
        }

        if (innerMessage.Contains("NOT NULL constraint"))
        {
            var fieldMatch = System.Text.RegularExpressions.Regex.Match(innerMessage, @"(column|field):\s*(\w+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            var field = fieldMatch.Success ? fieldMatch.Groups[2].Value : "campo requerido";
            
            return ("Campo requerido", 
                    $"Falta completar el campo '{field}'. Es obligatorio para {context}.", 
                    "REQUIRED_FIELD");
        }

        return ("Error de base de datos", 
                $"No se pudo guardar los cambios. Detalle: {innerMessage.Substring(0, Math.Min(100, innerMessage.Length))}...", 
                "DB_ERROR");
    }

    private static (string Error, string Detail, string Code) HandleHttpException(HttpRequestException ex, string? context)
    {
        if (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            return ("No autorizado", 
                    "Sesión expirada o credenciales inválidas. Por favor, inicie sesión nuevamente.", 
                    "UNAUTHORIZED");
        }

        if (ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
        {
            return ("Acceso denegado", 
                    $"No tiene permisos para {context}. Contacte al administrador.", 
                    "FORBIDDEN");
        }

        if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return ("Recurso no encontrado", 
                    $"El servicio externo o recurso solicitado no está disponible.", 
                    "NOT_FOUND");
        }

        if (ex.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
        {
            return ("Servicio no disponible", 
                    $"El servicio de {context} no está disponible en este momento. Intente más tarde.", 
                    "SERVICE_UNAVAILABLE");
        }

        return ("Error de comunicación", 
                $"No se pudo comunicar con el servicio de {context}. Verifique la conexión.", 
                "COMMUNICATION_ERROR");
    }

    private static string FormatDetailedError(Exception ex)
    {
        var message = ex.Message;
        
        // Incluir info de inner exception si esta disponible
        if (ex.InnerException != null)
        {
            message += $" | Causa: {ex.InnerException.Message}";
        }

        // Limitar longitud
        if (message.Length > 500)
        {
            message = message.Substring(0, 500) + "...";
        }

        return message;
    }
}

/// <summary>
/// Extension para usar en endpoints
/// </summary>
public static class ResultsExtensions
{
    public static IResult HandleError(Exception ex, string context)
    {
        var (error, detail, code) = ErrorHandler.GetErrorDetails(ex, context);
        
        return Results.Problem(
            detail: detail,
            title: error,
            statusCode: code switch
            {
                "NOT_FOUND" => 404,
                "DUPLICATE_KEY" => 409,
                "INVALID_ARGUMENT" => 400,
                "REQUIRED_FIELD" => 400,
                "INVALID_OPERATION" => 422,
                "UNAUTHORIZED" => 401,
                "FORBIDDEN" => 403,
                "SERVICE_UNAVAILABLE" => 503,
                "TIMEOUT" => 504,
                _ => 500
            },
            extensions: new Dictionary<string, object?>
            {
                ["errorCode"] = code,
                ["context"] = context,
                ["timestamp"] = DateTime.UtcNow
            }
        );
    }
}
