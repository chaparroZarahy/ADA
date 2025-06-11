-- Roles
INSERT INTO Rol(Nombre) VALUES ('Administrador')
INSERT INTO Rol(Nombre) VALUES ('Cliente')

GO
-- Usuario Admin
INSERT INTO Usuario([Nombre]
      ,[Direccion]
      ,[Telefono]
      ,[Usuario]
      ,[Identificacion]
      ,[Contrasena]
      ,[RolId])
VALUES ('Admin','Bogota', '123456789','admin','123456789','8ns3FgkAYCBalCb0Nj5RDw==',1)

GO