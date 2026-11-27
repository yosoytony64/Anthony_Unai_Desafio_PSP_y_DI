USE Hospital;

-- Tabla de usuarios para el inicio de sesión.
-- La contraseña NO se guarda en claro: se guarda su hash SHA-256 (64 caracteres hexadecimales).
-- id_medico es opcional: relaciona al usuario con un médico (ON DELETE SET NULL).
CREATE TABLE usuarios (
    id_usuario INT AUTO_INCREMENT PRIMARY KEY,
    nombre_usuario VARCHAR(50) NOT NULL UNIQUE,
    password_hash CHAR(64) NOT NULL,
    rol VARCHAR(30) NOT NULL DEFAULT 'Recepcion' CHECK (rol IN ('Administrador', 'Medico', 'Recepcion')),
    id_medico INT NULL,
    FOREIGN KEY (id_medico) REFERENCES medicos(id_medico) ON DELETE SET NULL ON UPDATE CASCADE
);

-- Contraseñas de prueba: admin -> admin123 | lmartinez -> medico123 | recepcion -> recep123
INSERT INTO usuarios (nombre_usuario, password_hash, rol, id_medico) VALUES
('admin',     '240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9', 'Administrador', NULL),
('lmartinez', '673ab82a6530ee3bd9b04ee72a4d66afa7fa059aedc685cf44e35d29d90ebafa', 'Medico',        1),
('recepcion', '5d37ed314cf2b5c8462b52b12cd512e2ac4a180e75598da4f12bfb0dea6d0a67', 'Recepcion',     NULL);
