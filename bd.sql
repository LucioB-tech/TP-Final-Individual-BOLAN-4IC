USE [TP_Final_Lucio]
GO
/****** Objeto: Table [dbo].[Categorías] Fecha de script: 20/11/2026 20:11:53 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Categorías](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[Nombre] [varchar](50) NOT NULL,
 CONSTRAINT [PK_Categorías] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[Libros] Fecha de script: 20/11/2026 20:11:54 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Libros](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[Titulo] [varchar](100) NOT NULL,
	[Autor] [varchar](100) NOT NULL,
	[AnioEdicion] [int] NOT NULL,
	[Descripcion] [varchar](500) NOT NULL,
	[Imagen] [varchar](200) NOT NULL,
	[IdCategoria] [int] NOT NULL,
	[Activo] [bit] NOT NULL,
 CONSTRAINT [PK_Libros] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[Prestamos] Fecha de script: 20/11/2026 20:11:54 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Prestamos](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[IdLibro] [int] NOT NULL,
	[IdUsuario] [int] NOT NULL,
	[FechaRetiro] [date] NOT NULL,
	[FechaDevolucion] [date] NOT NULL,
	[Codigo] [varchar](6) NOT NULL,
	[Cancelado] [bit] NOT NULL,
	[fechaCancelacion] [datetime] NULL,
 CONSTRAINT [PK_Prestamos] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[Usuarios] Fecha de script: 20/11/2026 20:11:54 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Usuarios](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[NombreUsuario] [varchar](50) NOT NULL,
	[Contrasena] [varchar](50) NOT NULL,
	[Nombre] [varchar](50) NOT NULL,
	[Apellido] [varchar](50) NOT NULL,
 CONSTRAINT [PK_Usuarios] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Libros]  WITH CHECK ADD  CONSTRAINT [FK_Libros_Categorías] FOREIGN KEY([IdCategoria])
REFERENCES [dbo].[Categorías] ([id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Libros] CHECK CONSTRAINT [FK_Libros_Categorías]
GO
ALTER TABLE [dbo].[Prestamos]  WITH CHECK ADD  CONSTRAINT [FK_Prestamos_Libros] FOREIGN KEY([IdLibro])
REFERENCES [dbo].[Libros] ([id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Prestamos] CHECK CONSTRAINT [FK_Prestamos_Libros]
GO
ALTER TABLE [dbo].[Prestamos]  WITH CHECK ADD  CONSTRAINT [FK_Prestamos_Usuarios] FOREIGN KEY([IdUsuario])
REFERENCES [dbo].[Usuarios] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Prestamos] CHECK CONSTRAINT [FK_Prestamos_Usuarios]
GO

ALTER TABLE [dbo].[Usuarios]
ADD CONSTRAINT UQ_Usuarios_NombreUsuario UNIQUE (NombreUsuario)
GO

ALTER TABLE [dbo].[Prestamos]
ADD CONSTRAINT UQ_Prestamos_Codigo UNIQUE (Codigo)
GO

INSERT INTO [dbo].[Categorías] (Nombre) VALUES
('Novela'), ('Ciencia ficción'), ('Terror'), ('Historia'),
('Fantasía'), ('Biografía'), ('Ciencia'), ('Infantil')
GO

INSERT INTO [dbo].[Libros]
(Titulo, Autor, AnioEdicion, Descripcion, Imagen, IdCategoria, Activo)
VALUES
('Cien años de soledad', 'Gabriel García Márquez', 1967, 'La historia de la familia Buendía.', '/libros/libro.svg', 1, 1),
('El túnel', 'Ernesto Sabato', 1948, 'Novela psicológica sobre la obsesión.', '/libros/libro.svg', 1, 1),
('Rayuela', 'Julio Cortázar', 1963, 'Una novela abierta sobre amor y búsqueda.', '/libros/libro.svg', 1, 1),
('Ficciones', 'Jorge Luis Borges', 1944, 'Relatos de mundos imaginarios y filosofía.', '/libros/libro.svg', 1, 1),
('Dune', 'Frank Herbert', 1965, 'Una lucha por el poder en el planeta Arrakis.', '/libros/libro.svg', 2, 1),
('Fundación', 'Isaac Asimov', 1951, 'La caída y reconstrucción de un imperio galáctico.', '/libros/libro.svg', 2, 1),
('1984', 'George Orwell', 1949, 'Una sociedad vigilada por el Gran Hermano.', '/libros/libro.svg', 2, 1),
('El cuento de la criada', 'Margaret Atwood', 1985, 'Distopía sobre control y resistencia.', '/libros/libro.svg', 2, 1),
('It', 'Stephen King', 1986, 'Un grupo enfrenta un antiguo terror en su pueblo.', '/libros/libro.svg', 3, 1),
('Drácula', 'Bram Stoker', 1897, 'El clásico vampiro acecha a sus víctimas.', '/libros/libro.svg', 3, 1),
('Frankenstein', 'Mary Shelley', 1818, 'Un científico crea una criatura con consecuencias trágicas.', '/libros/libro.svg', 3, 1),
('Sapiens', 'Yuval Noah Harari', 2011, 'Historia breve de la humanidad.', '/libros/libro.svg', 4, 1),
('El nombre del viento', 'Patrick Rothfuss', 2007, 'La vida legendaria de un músico y hechicero.', '/libros/libro.svg', 5, 1),
('Harry Potter y la piedra filosofal', 'J. K. Rowling', 1997, 'Un niño descubre que es mago.', '/libros/libro.svg', 5, 1),
('Steve Jobs', 'Walter Isaacson', 2011, 'Biografía del fundador de Apple.', '/libros/libro.svg', 6, 1),
('Una breve historia del tiempo', 'Stephen Hawking', 1988, 'El universo explicado para lectores generales.', '/libros/libro.svg', 7, 1),
('Cosmos', 'Carl Sagan', 1980, 'Un viaje por el espacio y la historia científica.', '/libros/libro.svg', 7, 1),
('El principito', 'Antoine de Saint-Exupéry', 1943, 'Un pequeño príncipe viaja entre planetas.', '/libros/libro.svg', 8, 1),
('Matilda', 'Roald Dahl', 1988, 'Una niña brillante descubre sus extraordinarios poderes.', '/libros/libro.svg', 8, 1),
('Charlie y la fábrica de chocolate', 'Roald Dahl', 1964, 'Niños visitan una fábrica de chocolate mágica.', '/libros/libro.svg', 8, 1)
GO

INSERT INTO [dbo].[Usuarios]
(NombreUsuario, Contrasena, Nombre, Apellido)
VALUES
('lucio', '123456', 'Lucio', 'Bolán'),
('ana', '123456', 'Ana', 'Gómez'),
('martin', '123456', 'Martín', 'Pérez')
GO

INSERT INTO [dbo].[Prestamos]
(IdLibro, IdUsuario, FechaRetiro, FechaDevolucion, Codigo, Cancelado, fechaCancelacion)
VALUES
(1, 1, DATEADD(day, -20, CAST(GETDATE() AS date)), DATEADD(day, -13, CAST(GETDATE() AS date)), 'A1B2C3', 0, NULL),
(2, 1, DATEADD(day, -10, CAST(GETDATE() AS date)), DATEADD(day, -3, CAST(GETDATE() AS date)), 'D4E5F6', 0, NULL),
(3, 1, DATEADD(day, -2, CAST(GETDATE() AS date)), DATEADD(day, 5, CAST(GETDATE() AS date)), 'G7H8I9', 0, NULL),
(4, 1, DATEADD(day, 2, CAST(GETDATE() AS date)), DATEADD(day, 9, CAST(GETDATE() AS date)), 'J1K2L3', 0, NULL),
(5, 2, DATEADD(day, -30, CAST(GETDATE() AS date)), DATEADD(day, -23, CAST(GETDATE() AS date)), 'M4N5P6', 0, NULL),
(6, 2, DATEADD(day, -15, CAST(GETDATE() AS date)), DATEADD(day, -8, CAST(GETDATE() AS date)), 'Q7R8S9', 1, DATEADD(day, -12, GETDATE())),
(7, 2, DATEADD(day, 1, CAST(GETDATE() AS date)), DATEADD(day, 8, CAST(GETDATE() AS date)), 'T1U2V3', 0, NULL),
(8, 3, DATEADD(day, -40, CAST(GETDATE() AS date)), DATEADD(day, -33, CAST(GETDATE() AS date)), 'W4X5Y6', 0, NULL),
(9, 3, DATEADD(day, -5, CAST(GETDATE() AS date)), DATEADD(day, 2, CAST(GETDATE() AS date)), 'Z7A8B9', 0, NULL),
(10, 3, DATEADD(day, 3, CAST(GETDATE() AS date)), DATEADD(day, 10, CAST(GETDATE() AS date)), 'C1D2E3', 0, NULL)
GO
