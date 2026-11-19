using System.ComponentModel.DataAnnotations;

namespace KONSolutions.Shared.Models.Comercial;

/// <summary>COMERCIAL.PEDIDOS_COMERCIALES — Fase 1: solo cabecera. Una misma tabla/pantalla
/// sirve para Proformas, Órdenes de Compra, Pedidos de Venta, etc., según COD_CLASE_OPERACION
/// (ver DES_PARAMETRO en SEGURIDAD.APLICACIONES_OPCIONES, mismo mecanismo que Entidades).</summary>
public class PedidoComercial
{
    public string COD_CLASE_OPERACION { get; set; } = "";
    public string COD_TIPO_OPERACION { get; set; } = "";
    public decimal? NUM_PEDIDO_COMERCIAL { get; set; }
    public string DES_PEDIDO_COMERCIAL { get; set; } = "";
    public string? COD_TIPO_COMPROBANTE_PAGO { get; set; }
    public string? NUM_DOCUMENTO { get; set; }
    public decimal? NUM_PROYECTO { get; set; }
    public string? COD_TIPO_PEDIDO_COMERCIAL { get; set; }
    public DateTime? FEC_PEDIDO { get; set; }
    public DateTime? FEC_VECIMIENTO { get; set; }
    public DateTime? FEC_ENTREGA { get; set; }
    public DateTime? FEC_ENTREGA_PUBLICA { get; set; }
    public string? COD_TIPO_ENTIDAD { get; set; }
    public decimal? COD_ENTIDAD { get; set; }
    public decimal? NUM_TEMPORADA { get; set; }
    public int? NUM_COLECCION { get; set; }
    public string? COD_MONEDA { get; set; }
    public decimal? FAC_TIPO_CAMBIO { get; set; }
    public decimal? NUM_UNIDAD_COMERCIAL { get; set; }
    public decimal? NUM_SECUENCIA_DIRECCION { get; set; }
    public string? COD_CENTRO_COSTO_SOLICITANTE { get; set; }
    public string? COD_CENTRO_COSTO_DESTINO { get; set; }
    public string? COD_SUCURSAL { get; set; }
    public string? COD_FORMA_PAGO { get; set; }
    public string? COD_CONDICION_PAGO { get; set; }
    public string? COD_TIPO_DESCUENTO { get; set; }
    public decimal? TAS_DESCUENTO { get; set; }
    public decimal? VAL_TOTAL { get; set; }
    public decimal? COD_ENTIDAD_VENDEDOR { get; set; }
    public string? NUM_PURCHASE_ORDER { get; set; }
    public string? COD_PAIS_DESTINO { get; set; }
    public decimal? NUM_LOTE_CONTROL { get; set; }
    public decimal? NUM_RENDICION_CUENTAS { get; set; }
    public string? COD_TIPO_MOTIVO { get; set; }
    public string? COD_MOTIVO { get; set; }
    public string? DES_OBSERVACION { get; set; }
    public decimal? NUM_PEDIDO_COMERCIAL_RELACIONADO { get; set; }
    public string? NUM_TRACKING { get; set; }
    public string? COD_INCOTERM { get; set; }
    public int? COD_TIPO_ESTADO { get; set; }
    public int? COD_ESTADO { get; set; }
    public string? COD_USUARIO_REGISTRO { get; set; }
    public string? COD_ESTACION_REGISTRO { get; set; }
    public DateTime? FEC_REGISTRO { get; set; }
    public string? COD_USUARIO_ACTUALIZACION { get; set; }
    public string? COD_ESTACION_ACTUALIZACION { get; set; }
    public DateTime? FEC_ACTUALIZACION { get; set; }

    // Desnormalizados del SP
    public string DES_CENTRO_COSTO { get; set; } = "";
    public string DES_COMERCIAL { get; set; } = "";
    public string DES_NOMBRE_COMPLETO { get; set; } = "";
    public string DES_CONDICION_PAGO { get; set; } = "";
    public string DES_FORMA_PAGO { get; set; } = "";
    public string DES_INCOTERM { get; set; } = "";
    public string DES_MONEDA { get; set; } = "";
    public string DES_MOTIVO { get; set; } = "";
    public string DES_PAIS { get; set; } = "";
    public string DES_PROYECTO { get; set; } = "";
    public string DES_TIPO_DESCUENTO { get; set; } = "";
    public string DES_TIPO_PEDIDO_COMERCIAL { get; set; } = "";
    public string DES_UNIDAD_COMERCIAL { get; set; } = "";
    public string DES_TEMPORADA { get; set; } = "";
    public string DES_COLECCION { get; set; } = "";
    public string DES_TIPO_COMPROBANTE_PAGO { get; set; } = "";
    public string DES_BACKCOLOR { get; set; } = "";
    public string DES_ESTADO { get; set; } = "";
    public string DES_FORECOLOR { get; set; } = "";
    public string DES_DIRECCION { get; set; } = "";
    public string DES_RENDICION_CUENTAS { get; set; } = "";
    /// <summary>Nombre del vendedor. Requiere que PROC_COMERCIAL_PEDIDOS_COMERCIALES_SHOWALL
    /// joinee ESTANDAR.ENTIDADES por COD_ENTIDAD_VENDEDOR — hoy el SP sólo devuelve el código.
    /// Mientras no esté, la columna de la grilla muestra el código.</summary>
    public string DES_ENTIDAD_VENDEDOR { get; set; } = "";
    /// <summary>Dirección de entrega ya resuelta por el SP: la de la sucursal o la de la
    /// entidad, según el FLG_POSITIVO del tipo de operación. Que lo decida el SP evita que
    /// la pantalla tenga que adivinar de cuál de las dos columnas sacarla.</summary>
    public string DES_DIRECCION_SUCURSAL { get; set; } = "";
    /// <summary>true si el pedido tiene líneas vinculadas a requerimientos de stock (nació de ellos). Lo calcula el SP.
    /// Con él, las cantidades y los ítems no se editan, y no se puede eliminar el pedido.</summary>
    public bool? FLG_POR_REQUERIMIENTO { get; set; }

    // ── Operación de origen ──────────────────────────────────────────────────
    // Cuando el tipo de operación de este pedido está sincronizado con otro (una Orden de
    // Compra que nació de una Proforma, por ejemplo), el SP trae también la clase y el tipo
    // de operación de ORIGEN. Todo null si el tipo de operación no sincroniza con ninguno.
    public string? COD_CLASE_OPERACION_ORIGEN { get; set; }
    public string? DES_CLASE_OPERACION_ORIGEN { get; set; }
    public string? COD_TIPO_OPERACION_ORIGEN { get; set; }
    public string? DES_TIPO_OPERACION_ORIGEN { get; set; }
    /// <summary>Los cuatro campos de arriba, ya armados por el propio SP en un solo texto
    /// ("ORDC-Órdenes de Compra / OC-Orden de Compra [Nº 127]"). Null si no hay origen.</summary>
    public string? DES_PEDIDO_ORIGEN { get; set; }

    // ── Notificación en curso (ENTORNO.NOTIFICACIONES) ───────────────────────
    // Sólo lectura: la escribe el flujo de aprobación, no la pantalla. El documento
    // guarda el número y el resto viene por JOIN, incluidos los colores del estado,
    // que salen de ESTANDAR.ESTADOS como los de cualquier otro estado del sistema.
    public long?    NUM_NOTIFICACION            { get; set; }
    public string?  DES_NOTIFICACION            { get; set; }
    public DateTime? FEC_NOTIFICACION           { get; set; }
    public string?  DES_ESTADO_NOTIFICACION     { get; set; }
    public string?  DES_BACKCOLOR_NOTIFICACION  { get; set; }
    public string?  DES_FORECOLOR_NOTIFICACION  { get; set; }

}

public class PedidoComercialInsertDto
{
    [Required] public string COD_CLASE_OPERACION { get; set; } = "";
    [Required] public string COD_TIPO_OPERACION { get; set; } = "";
    /// <summary>0/null para alta nueva — el SP autonumera dentro de la clase/tipo de operación.</summary>
    public decimal? NUM_PEDIDO_COMERCIAL { get; set; }
    [Required] public string DES_PEDIDO_COMERCIAL { get; set; } = "";
    public string? COD_TIPO_COMPROBANTE_PAGO { get; set; }
    public string? NUM_DOCUMENTO { get; set; }
    public decimal? NUM_PROYECTO { get; set; }
    public string? COD_TIPO_PEDIDO_COMERCIAL { get; set; }
    public DateTime? FEC_PEDIDO { get; set; }
    public DateTime? FEC_VECIMIENTO { get; set; }
    public DateTime? FEC_ENTREGA { get; set; }
    public DateTime? FEC_ENTREGA_PUBLICA { get; set; }
    public string? COD_TIPO_ENTIDAD { get; set; }
    [Required] public decimal? COD_ENTIDAD { get; set; }
    public decimal? NUM_TEMPORADA { get; set; }
    public int? NUM_COLECCION { get; set; }
    public string? COD_MONEDA { get; set; }
    public decimal? FAC_TIPO_CAMBIO { get; set; }
    public decimal? NUM_UNIDAD_COMERCIAL { get; set; }
    public decimal? NUM_SECUENCIA_DIRECCION { get; set; }
    public string? COD_CENTRO_COSTO_SOLICITANTE { get; set; }
    public string? COD_CENTRO_COSTO_DESTINO { get; set; }
    public string? COD_SUCURSAL { get; set; }
    public string? COD_FORMA_PAGO { get; set; }
    public string? COD_CONDICION_PAGO { get; set; }
    public string? COD_TIPO_DESCUENTO { get; set; }
    public decimal? TAS_DESCUENTO { get; set; }
    public decimal? VAL_TOTAL { get; set; }
    public decimal? COD_ENTIDAD_VENDEDOR { get; set; }
    public string? NUM_PURCHASE_ORDER { get; set; }
    public string? COD_PAIS_DESTINO { get; set; }
    public decimal? NUM_LOTE_CONTROL { get; set; }
    public decimal? NUM_RENDICION_CUENTAS { get; set; }
    public string? COD_TIPO_MOTIVO { get; set; }
    public string? COD_MOTIVO { get; set; }
    public string? DES_OBSERVACION { get; set; }
    public decimal? NUM_PEDIDO_COMERCIAL_RELACIONADO { get; set; }
    public string? NUM_TRACKING { get; set; }
    public string? COD_INCOTERM { get; set; }
    public int? COD_TIPO_ESTADO { get; set; }
    public int? COD_ESTADO { get; set; }
}

public class PedidoComercialUpdateDto
{
    [Required] public string COD_CLASE_OPERACION { get; set; } = "";
    [Required] public string COD_TIPO_OPERACION { get; set; } = "";
    [Required] public decimal NUM_PEDIDO_COMERCIAL { get; set; }
    [Required] public string DES_PEDIDO_COMERCIAL { get; set; } = "";
    public string? COD_TIPO_COMPROBANTE_PAGO { get; set; }
    public string? NUM_DOCUMENTO { get; set; }
    public decimal? NUM_PROYECTO { get; set; }
    public string? COD_TIPO_PEDIDO_COMERCIAL { get; set; }
    public DateTime? FEC_PEDIDO { get; set; }
    public DateTime? FEC_VECIMIENTO { get; set; }
    public DateTime? FEC_ENTREGA { get; set; }
    public DateTime? FEC_ENTREGA_PUBLICA { get; set; }
    public string? COD_TIPO_ENTIDAD { get; set; }
    [Required] public decimal? COD_ENTIDAD { get; set; }
    public decimal? NUM_TEMPORADA { get; set; }
    public int? NUM_COLECCION { get; set; }
    public string? COD_MONEDA { get; set; }
    public decimal? FAC_TIPO_CAMBIO { get; set; }
    public decimal? NUM_UNIDAD_COMERCIAL { get; set; }
    public decimal? NUM_SECUENCIA_DIRECCION { get; set; }
    public string? COD_CENTRO_COSTO_SOLICITANTE { get; set; }
    public string? COD_CENTRO_COSTO_DESTINO { get; set; }
    public string? COD_SUCURSAL { get; set; }
    public string? COD_FORMA_PAGO { get; set; }
    public string? COD_CONDICION_PAGO { get; set; }
    public string? COD_TIPO_DESCUENTO { get; set; }
    public decimal? TAS_DESCUENTO { get; set; }
    public decimal? VAL_TOTAL { get; set; }
    public decimal? COD_ENTIDAD_VENDEDOR { get; set; }
    public string? NUM_PURCHASE_ORDER { get; set; }
    public string? COD_PAIS_DESTINO { get; set; }
    public decimal? NUM_LOTE_CONTROL { get; set; }
    public decimal? NUM_RENDICION_CUENTAS { get; set; }
    public string? COD_TIPO_MOTIVO { get; set; }
    public string? COD_MOTIVO { get; set; }
    public string? DES_OBSERVACION { get; set; }
    public decimal? NUM_PEDIDO_COMERCIAL_RELACIONADO { get; set; }
    public string? NUM_TRACKING { get; set; }
    public string? COD_INCOTERM { get; set; }
    public int? COD_TIPO_ESTADO { get; set; }
    public int? COD_ESTADO { get; set; }
}
