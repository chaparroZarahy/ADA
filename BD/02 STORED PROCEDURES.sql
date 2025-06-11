/****** Object:  StoredProcedure [dbo].[actualizar_producto]    Script Date: 11/06/2025 10:01:30 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[actualizar_producto]
    @productoId INT,
    @cantidad INT,
    @usuarioId INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Validar administrador
    DECLARE @RolId INT;
    SELECT @RolId = RolId FROM Usuario WHERE Id = @usuarioId;

    IF @RolId IS NULL OR (SELECT Nombre FROM Rol WHERE Id = @RolId) != 'Administrador'
    BEGIN
        RAISERROR('No tiene permisos para actualizar productos.', 16, 1);
        RETURN;
    END

    -- Validar producto
    IF NOT EXISTS (SELECT 1 FROM Producto WHERE Id = @productoId)
    BEGIN
        RAISERROR('Producto no encontrado.', 16, 1);
        RETURN;
    END

    -- Actualizar cantidad
    UPDATE Producto
    SET Cantidad = @cantidad
    WHERE Id = @productoId;

	SELECT 'Producto actualizado correctamente' AS Mensaje;

END;
GO
/****** Object:  StoredProcedure [dbo].[confirmar_compra]    Script Date: 11/06/2025 10:01:30 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[confirmar_compra]
    @UsuarioId INT,
    @ProductoId INT,
	@Cantidad INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Disponible INT;
    SELECT @Disponible = Cantidad FROM Producto WHERE Id = @ProductoId;

    IF @Disponible IS NULL
    BEGIN
        SELECT 'Producto no encontrado.';
        RETURN;
    END

    IF @Cantidad > @Disponible
    BEGIN
        SELECT 'Solo hay ' + CAST(@Disponible AS NVARCHAR) + ' unidades. ¿Desea comprarlas?';
        RETURN;
    END

    -- Descontar del inventario
    UPDATE Producto
    SET Cantidad = Cantidad - @Cantidad
    WHERE Id = @ProductoId;

    -- Registrar la venta
    INSERT INTO Venta (UsuarioId, ProductoId, Cantidad)
    VALUES (@UsuarioId, @ProductoId, @Cantidad);

    SELECT 'Compra parcial confirmada exitosamente.';
END
GO
/****** Object:  StoredProcedure [dbo].[login_usuario]    Script Date: 11/06/2025 10:01:30 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[login_usuario]
    @Usuario VARCHAR(50),
    @Contrasena VARCHAR(100)
AS
BEGIN
      SET NOCOUNT ON;

    BEGIN TRY
        IF EXISTS (
            SELECT 1 
            FROM Usuario 
            WHERE Usuario = @Usuario AND Contrasena = @Contrasena
        )
        BEGIN
            SELECT 
                u.Id, 
                u.Nombre,
                u.Usuario,
                CASE 
                    WHEN u.RolId = 1 THEN 'Administrador'
                    WHEN u.RolId = 2 THEN 'Cliente'
                    ELSE 'Rol desconocido'
                END AS Rol
            FROM Usuario u
            WHERE u.Usuario = @Usuario AND u.Contrasena = @Contrasena;

            RETURN 1; -- Login exitoso
        END
        ELSE
        BEGIN
            RETURN 0; -- Credenciales incorrectas
        END
    END TRY
    BEGIN CATCH
        DECLARE @ErrMsg NVARCHAR(4000), @ErrSeverity INT, @ErrState INT;
        SELECT 
            @ErrMsg = ERROR_MESSAGE(),
            @ErrSeverity = ERROR_SEVERITY(),
            @ErrState = ERROR_STATE();

        RAISERROR(@ErrMsg, @ErrSeverity, @ErrState);
    END CATCH
END;
GO
/****** Object:  StoredProcedure [dbo].[obtener_historial_compras]    Script Date: 11/06/2025 10:01:30 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[obtener_historial_compras]
    @UsuarioId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        v.Id,
        p.Nombre AS Producto,
        v.Cantidad,
        v.Fecha
    FROM Venta v
    INNER JOIN Producto p ON v.ProductoId = p.Id
    WHERE v.UsuarioId = @UsuarioId
    ORDER BY v.Fecha DESC;
END
GO
/****** Object:  StoredProcedure [dbo].[obtener_producto]    Script Date: 11/06/2025 10:01:30 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[obtener_producto]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT Id, Nombre, Cantidad, Descripcion
    FROM Producto
    WHERE Cantidad > 0;
END;
GO
/****** Object:  StoredProcedure [dbo].[obtener_usuario]    Script Date: 11/06/2025 10:01:30 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[obtener_usuario]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        u.Id,
        u.Nombre,
        u.Usuario,
        u.Identificacion,
        u.Telefono,
        u.Direccion,
        r.Nombre AS Rol
    FROM Usuario u
    JOIN Rol r ON u.RolId = r.Id
    WHERE r.Nombre != 'Administrador'; 
END;
GO
/****** Object:  StoredProcedure [dbo].[obtener_venta]    Script Date: 11/06/2025 10:01:30 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[obtener_venta]
AS
BEGIN
    SELECT
        T.Id,
        U.Nombre AS Cliente,
        P.Nombre AS Producto,
        T.Cantidad,
        T.Fecha
    FROM Venta T
    INNER JOIN Usuario U ON U.Id = T.UsuarioId
    INNER JOIN Producto P ON P.Id = T.ProductoId
END

GO
/****** Object:  StoredProcedure [dbo].[realizar_compra]    Script Date: 11/06/2025 10:01:30 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[realizar_compra]
     @UsuarioId INT,
    @ProductoId INT,
    @Cantidad INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Disponible INT;

    -- Verificar si el producto existe
    SELECT @Disponible = Cantidad FROM Producto WHERE Id = @ProductoId;

    IF @Disponible IS NULL
    BEGIN
        SELECT 'Producto no encontrado.';
        RETURN;
    END

    -- Validar stock suficiente
    IF @Cantidad > @Disponible
BEGIN
    SELECT 'Solo hay ' + CAST(@Disponible AS NVARCHAR) + ' unidades. ¿Desea comprarlas?';
    RETURN;
END


    -- Descontar del inventario
    UPDATE Producto
    SET Cantidad = Cantidad - @Cantidad
    WHERE Id = @ProductoId;

    -- Registrar la venta
    INSERT INTO Venta (UsuarioId, ProductoId, Cantidad)
    VALUES (@UsuarioId, @ProductoId, @Cantidad);

    -- Confirmación
    SELECT 'Compra realizada con éxito.';
END
GO
/****** Object:  StoredProcedure [dbo].[registrar_producto]    Script Date: 11/06/2025 10:01:30 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE Procedure [dbo].[registrar_producto]
    @Nombre VARCHAR(100),
    @Cantidad INT,
    @Descripcion VARCHAR(250)
AS
BEGIN
   IF EXISTS (SELECT 1 FROM Producto WHERE Nombre = @Nombre)
BEGIN
    SELECT 1 AS Codigo, 'Ya existe un producto con ese nombre' AS Mensaje;
    RETURN;
END

INSERT INTO Producto (Nombre, Cantidad, Descripcion)
VALUES (@Nombre, @Cantidad, @Descripcion);

SELECT 0 AS Codigo, 'Producto creado correctamente' AS Mensaje;

END
GO
/****** Object:  StoredProcedure [dbo].[registrar_usuario]    Script Date: 11/06/2025 10:01:30 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[registrar_usuario]
    @Nombre VARCHAR(100),
    @Direccion VARCHAR(150),
    @Telefono VARCHAR(20),
    @Usuario VARCHAR(50),
    @Identificacion VARCHAR(20),
    @Contrasena VARCHAR(100),
    @RolNombre VARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        -- Validaciones de campos obligatorios
        IF LEN(ISNULL(@Nombre, '')) = 0 OR
           LEN(ISNULL(@Direccion, '')) = 0 OR
           LEN(ISNULL(@Telefono, '')) = 0 OR
           LEN(ISNULL(@Usuario, '')) = 0 OR
           LEN(ISNULL(@Identificacion, '')) = 0 OR
           LEN(ISNULL(@Contrasena, '')) = 0 OR
           LEN(ISNULL(@RolNombre, '')) = 0
        BEGIN
            RAISERROR('Todos los campos son obligatorios.', 16, 1);
            RETURN;
        END

        -- Validar que el teléfono solo tenga números
        IF @Telefono LIKE '%[^0-9]%'
        BEGIN
            RAISERROR('El teléfono solo debe contener números.', 16, 1);
            RETURN;
        END

        -- Validar que la identificación solo tenga números
        IF @Identificacion LIKE '%[^0-9]%'
        BEGIN
            RAISERROR('La identificación solo debe contener números.', 16, 1);
            RETURN;
        END

        -- Validar contraseña: al menos una letra y un número
        IF NOT (@Contrasena LIKE '%[A-Za-z]%' AND @Contrasena LIKE '%[0-9]%')
        BEGIN
            RAISERROR('La contraseña debe contener al menos una letra y un número.', 16, 1);
            RETURN;
        END

        -- Validar existencia del rol
        DECLARE @RolId INT;
        SELECT @RolId = Id FROM Rol WHERE Nombre = @RolNombre;

        IF @RolId IS NULL
        BEGIN
            RAISERROR('El rol especificado no existe.', 16, 1);
            RETURN;
        END

        -- Validar si el nombre de usuario ya existe
        IF EXISTS (SELECT 1 FROM Usuario WHERE Usuario = @Usuario)
        BEGIN
            SELECT 'El usuario ya está registrado.' AS Mensaje;
            RETURN;
        END

        -- Validar si la identificación ya está registrada
        IF EXISTS (SELECT 1 FROM Usuario WHERE Identificacion = @Identificacion)
        BEGIN
            SELECT 'La identificación ya está registrada.' AS Mensaje;
            RETURN;
        END

        -- Insertar nuevo usuario
        INSERT INTO Usuario (Nombre, Direccion, Telefono, Usuario, Identificacion, Contrasena, RolId)
        VALUES (@Nombre, @Direccion, @Telefono, @Usuario, @Identificacion, @Contrasena, @RolId);

        -- Mensaje de éxito
        SELECT 'Usuario registrado exitosamente.' AS Mensaje;
    END TRY
    BEGIN CATCH
        DECLARE @ErrMsg NVARCHAR(4000), @ErrSeverity INT, @ErrState INT;
        SELECT 
            @ErrMsg = ERROR_MESSAGE(),
            @ErrSeverity = ERROR_SEVERITY(),
            @ErrState = ERROR_STATE();

        RAISERROR(@ErrMsg, @ErrSeverity, @ErrState);
    END CATCH
END;
GO
