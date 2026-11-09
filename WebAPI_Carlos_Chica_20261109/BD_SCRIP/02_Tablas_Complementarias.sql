USE [Soli_Compras]
GO

/*====================================================================
  Tablas complementarias para el flujo de autorizaciones de compra.
  Ejecutar DESPUÉS de Soli_Compras.Database.sql y de las tablas base
  que ya están en la carpeta BD_SCRIP.
====================================================================*/

/*------------------------------------------------------------------
  Detalles (artículos / servicios) de cada versión de una solicitud.
  El importe total de una versión = suma de (cantidad * precio unitario)
------------------------------------------------------------------*/
CREATE TABLE dbo.solicitud_detalles(
    [Id] [int] IDENTITY(1,1) NOT NULL,
    [solicitud_version_id] [int] NOT NULL,
    [descripcion] [varchar](200) NOT NULL,
    [cantidad] [decimal](12, 2) NOT NULL,
    [precio_unitario] [decimal](12, 2) NOT NULL,
PRIMARY KEY CLUSTERED ([Id] ASC) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE dbo.solicitud_detalles WITH CHECK ADD CONSTRAINT [FK_Detalle_Version]
FOREIGN KEY([solicitud_version_id]) REFERENCES dbo.solicitud_versiones([Id])
GO

/*------------------------------------------------------------------
  Etapas de aprobación que se fijan al enviar la solicitud.
  El orden indica la secuencia: 1 = primero, 2 = segundo, etc.
------------------------------------------------------------------*/
CREATE TABLE dbo.solicitud_etapas(
    [Id] [int] IDENTITY(1,1) NOT NULL,
    [solicitud_version_id] [int] NOT NULL,
    [orden] [int] NOT NULL,
    [etapa] [varchar](30) NOT NULL,   -- 'Responsable', 'Finanzas', 'Gerencia'
    [revisor_id] [int] NOT NULL,
PRIMARY KEY CLUSTERED ([Id] ASC) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE dbo.solicitud_etapas WITH CHECK ADD CONSTRAINT [FK_Etapa_Version]
FOREIGN KEY([solicitud_version_id]) REFERENCES dbo.solicitud_versiones([Id])
GO
ALTER TABLE dbo.solicitud_etapas WITH CHECK ADD CONSTRAINT [FK_Etapa_Revisor]
FOREIGN KEY([revisor_id]) REFERENCES dbo.colaboradores([Id])
GO

/*------------------------------------------------------------------
  Decisiones de revisión de cada versión.
  Una decisión solo puede tomarse sobre la etapa pendiente en orden.
------------------------------------------------------------------*/
CREATE TABLE dbo.solicitud_revisiones(
    [Id] [int] IDENTITY(1,1) NOT NULL,
    [solicitud_version_id] [int] NOT NULL,
    [etapa_id] [int] NULL,
    [orden_etapa] [int] NOT NULL,
    [etapa_nombre] [varchar](30) NOT NULL,
    [revisor_id] [int] NOT NULL,
    [decision] [varchar](20) NOT NULL DEFAULT 'PENDIENTE', -- PENDIENTE / APROBADO / RECHAZADO / DEVUELTO
    [comentario] [varchar](max) NULL,
    [fecha_decision] [datetime2](7) NULL,
PRIMARY KEY CLUSTERED ([Id] ASC) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE dbo.solicitud_revisiones WITH CHECK ADD CONSTRAINT [FK_Revision_Version]
FOREIGN KEY([solicitud_version_id]) REFERENCES dbo.solicitud_versiones([Id])
GO
ALTER TABLE dbo.solicitud_revisiones WITH CHECK ADD CONSTRAINT [FK_Revision_Etapa]
FOREIGN KEY([etapa_id]) REFERENCES dbo.solicitud_etapas([Id])
GO
ALTER TABLE dbo.solicitud_revisiones WITH CHECK ADD CONSTRAINT [FK_Revision_Revisor]
FOREIGN KEY([revisor_id]) REFERENCES dbo.colaboradores([Id])
GO

/*------------------------------------------------------------------
  Revisores designados por la empresa (Finanzas y Gerencia) y su suplente.
  Si el solicitante es el revisor de una etapa, se usa el suplente.
------------------------------------------------------------------*/
CREATE TABLE dbo.config_revisor(
    [Id] [int] IDENTITY(1,1) NOT NULL,
    [rol] [varchar](30) NOT NULL,      -- 'Finanzas' o 'Gerencia'
    [colaborador_id] [int] NOT NULL,
    [suplente_id] [int] NULL,
PRIMARY KEY CLUSTERED ([Id] ASC) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE dbo.config_revisor WITH CHECK ADD CONSTRAINT [FK_ConfigRevisor_Colaborador]
FOREIGN KEY([colaborador_id]) REFERENCES dbo.colaboradores([Id])
GO
ALTER TABLE dbo.config_revisor WITH CHECK ADD CONSTRAINT [FK_ConfigRevisor_Suplente]
FOREIGN KEY([suplente_id]) REFERENCES dbo.colaboradores([Id])
GO

/*------------------------------------------------------------------
  Detalles de las órdenes de compra (artículos, cantidades, precios).
------------------------------------------------------------------*/
CREATE TABLE dbo.orden_compra_detalles(
    [Id] [int] IDENTITY(1,1) NOT NULL,
    [orden_compra_id] [int] NOT NULL,
    [descripcion] [varchar](200) NOT NULL,
    [cantidad] [decimal](12, 2) NOT NULL,
    [precio_unitario] [decimal](12, 2) NOT NULL,
PRIMARY KEY CLUSTERED ([Id] ASC) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE dbo.orden_compra_detalles WITH CHECK ADD CONSTRAINT [FK_OrdenDetalle_Orden]
FOREIGN KEY([orden_compra_id]) REFERENCES dbo.ordenes_compra([id])
GO

/*------------------------------------------------------------------
  Recepciones de entregas (pueden ser parciales).
------------------------------------------------------------------*/
CREATE TABLE dbo.recepciones(
    [Id] [int] IDENTITY(1,1) NOT NULL,
    [orden_compra_id] [int] NOT NULL,
    [fecha_recepcion] [datetime2](7) NOT NULL,
    [colaborador_id] [int] NOT NULL,
    [observaciones] [varchar](500) NULL,
PRIMARY KEY CLUSTERED ([Id] ASC) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE dbo.recepciones WITH CHECK ADD CONSTRAINT [FK_Recepcion_Orden]
FOREIGN KEY([orden_compra_id]) REFERENCES dbo.ordenes_compra([id])
GO
ALTER TABLE dbo.recepciones WITH CHECK ADD CONSTRAINT [FK_Recepcion_Colaborador]
FOREIGN KEY([colaborador_id]) REFERENCES dbo.colaboradores([Id])
GO

/*------------------------------------------------------------------
  Datos de prueba para poder probar la API sin insertar nada a mano.
------------------------------------------------------------------*/
SET IDENTITY_INSERT dbo.colaboradores ON
INSERT INTO dbo.colaboradores ([Id],[cedula],[nombre],[correo]) VALUES
(1,'1001','Ana Perez (Solicitante)','ana@empresa.com'),
(2,'1002','Luis Gomez (Resp. TI)','luis@empresa.com'),
(3,'1003','Maria Ruiz (Suplente TI)','maria@empresa.com'),
(4,'1004','Carla Diaz (Finanzas)','carla@empresa.com'),
(5,'1005','Jorge Ruiz (Suplente Finanzas)','jorge@empresa.com'),
(6,'1006','Elena Torres (Gerencia)','elena@empresa.com'),
(7,'1007','Pedro Ramirez (Suplente Gerencia)','pedro@empresa.com')
SET IDENTITY_INSERT dbo.colaboradores OFF

SET IDENTITY_INSERT dbo.departamentos ON
INSERT INTO dbo.departamentos ([Id],[nombre],[presupuesto_anual],[responsable_id],[suplente_id]) VALUES
(1,'Tecnologia',50000.00,2,3)
SET IDENTITY_INSERT dbo.departamentos OFF

SET IDENTITY_INSERT dbo.config_revisor ON
INSERT INTO dbo.config_revisor ([Id],[rol],[colaborador_id],[suplente_id]) VALUES
(1,'Finanzas',4,5),
(2,'Gerencia',6,7)
SET IDENTITY_INSERT dbo.config_revisor OFF

SET IDENTITY_INSERT dbo.proveedores ON
INSERT INTO dbo.proveedores ([Id],[ruc],[razon_social],[contacto]) VALUES
(1,'9001','TecnoSuministros S.A.S.','ventas@tecnosuministros.com'),
(2,'9002','OfiCenter Ltda.','pedidos@oficenter.com')
SET IDENTITY_INSERT dbo.proveedores OFF

PRINT 'Tablas complementarias y datos de prueba creados correctamente.'
GO
