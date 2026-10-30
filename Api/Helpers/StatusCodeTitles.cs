namespace Api.Helpers;

public static class StatusCodeTitles
{
    public static string GetTitle(int statusCode)
    {
        return statusCode switch
        {
            400 => "Petición incorrecta",
            401 => "No autenticado",
            403 => "Acceso prohibido",
            404 => "Recurso no encontrado",
            405 => "Método no permitido",
            409 => "Conflicto",
            415 => "Formato no compatible",
            _ => "Error en la solicitud"
        };
    }

    public static ReadOnlySpan<int> StatusCodes => [400, 401, 403, 404, 405, 409, 415];
}
