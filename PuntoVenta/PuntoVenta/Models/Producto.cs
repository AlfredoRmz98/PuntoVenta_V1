using System;
using System.Collections.Generic;

namespace PuntoVenta.Models;

public partial class Producto
{
    public string Codigo { get; set; } = null!;

    public string? Nombre { get; set; }

    public string? Marca { get; set; }

    public string? Descripcion { get; set; }

    public decimal? Precio { get; set; }
}
