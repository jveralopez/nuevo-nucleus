using ConfiguracionService.Domain.Models;
using ConfiguracionService.Domain.Requests;
using ConfiguracionService.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace ConfiguracionService.Services;

public class ConfiguracionCatalogosService
{
    private readonly ConfiguracionDbContext _db;

    public ConfiguracionCatalogosService(ConfiguracionDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyCollection<CatalogoItem>> GetCatalogosAsync(string tipo)
    {
        return await _db.Catalogos.AsNoTracking()
            .Where(c => c.Tipo == tipo)
            .OrderBy(c => c.Nombre)
            .ToListAsync();
    }

    public async Task<CatalogoItem> CreateCatalogoAsync(CreateCatalogoItemRequest request)
    {
        var item = new CatalogoItem
        {
            Tipo = request.Tipo.Trim(),
            Codigo = request.Codigo.Trim(),
            Nombre = request.Nombre.Trim(),
            Activo = request.Activo,
            MetadataJson = request.MetadataJson
        };
        _db.Catalogos.Add(item);
        await _db.SaveChangesAsync();
        return item;
    }

    public async Task<CatalogoItem?> UpdateCatalogoAsync(Guid id, UpdateCatalogoItemRequest request)
    {
        var item = await _db.Catalogos.FirstOrDefaultAsync(c => c.Id == id);
        if (item == null) return null;
        item.Codigo = request.Codigo.Trim();
        item.Nombre = request.Nombre.Trim();
        item.Activo = request.Activo;
        item.MetadataJson = request.MetadataJson;
        item.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return item;
    }

    public async Task<bool> DeleteCatalogoAsync(Guid id)
    {
        var item = await _db.Catalogos.FirstOrDefaultAsync(c => c.Id == id);
        if (item == null) return false;
        _db.Catalogos.Remove(item);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<IReadOnlyCollection<Parametro>> GetParametrosAsync()
    {
        return await _db.Parametros.AsNoTracking().OrderBy(p => p.Clave).ToListAsync();
    }

    public async Task<Parametro?> GetParametroAsync(string clave)
    {
        return await _db.Parametros.AsNoTracking().FirstOrDefaultAsync(p => p.Clave == clave);
    }

    public async Task<Parametro> UpsertParametroAsync(UpsertParametroRequest request)
    {
        var clave = request.Clave.Trim();
        var parametro = await _db.Parametros.FirstOrDefaultAsync(p => p.Clave == clave);
        if (parametro == null)
        {
            parametro = new Parametro
            {
                Clave = clave,
                Valor = request.Valor.Trim(),
                Descripcion = request.Descripcion,
                Activo = request.Activo
            };
            _db.Parametros.Add(parametro);
        }
        else
        {
            parametro.Valor = request.Valor.Trim();
            parametro.Descripcion = request.Descripcion;
            parametro.Activo = request.Activo;
            parametro.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        return parametro;
    }
}
