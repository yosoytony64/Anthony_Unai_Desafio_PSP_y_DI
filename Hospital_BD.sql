create database Hospital;

use Hospital;

CREATE TABLE departamentos (
    id_departamento INT AUTO_INCREMENT PRIMARY KEY,
    nombre VARCHAR(100) NOT NULL UNIQUE,
    ubicacion VARCHAR(100),
    telefono VARCHAR(20) CHECK (telefono REGEXP '^[0-9]{9}$')
);

CREATE TABLE medicos (
    id_medico INT AUTO_INCREMENT PRIMARY KEY,
    nombre VARCHAR(50) NOT NULL,
    apellidos VARCHAR(100) NOT NULL,
    especialidad VARCHAR(100) NOT NULL,
    num_colegiado VARCHAR(50) NOT NULL UNIQUE CHECK (num_colegiado REGEXP '^MED-[0-9]{4,6}$'),
    telefono VARCHAR(20) CHECK (telefono REGEXP '^[0-9]{9}$'),
    email VARCHAR(100) UNIQUE CHECK (email REGEXP '^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$'),
    id_departamento INT NOT NULL,
    FOREIGN KEY (id_departamento) REFERENCES departamentos(id_departamento) ON DELETE RESTRICT ON UPDATE CASCADE
);

CREATE TABLE pacientes (
    id_paciente INT AUTO_INCREMENT PRIMARY KEY,
    nombre VARCHAR(50) NOT NULL,
    apellidos VARCHAR(100) NOT NULL,
    dni VARCHAR(20) NOT NULL UNIQUE CHECK (dni REGEXP '^[0-9XYZ][0-9]{7}[A-Z]$'),
    fecha_nacimiento DATE NOT NULL,
    genero VARCHAR(20),
    telefono VARCHAR(20) CHECK (telefono REGEXP '^[0-9]{9}$'),
    email VARCHAR(100) CHECK (email REGEXP '^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$'),
    direccion TEXT,
    grupo_sanguineo VARCHAR(10) CHECK (grupo_sanguineo REGEXP '^(A|B|AB|O)[+-]$')
);

CREATE TABLE citas (
    id_cita INT AUTO_INCREMENT PRIMARY KEY,
    id_paciente INT NOT NULL,
    id_medico INT NOT NULL,
    fecha_hora DATETIME NOT NULL,
    motivo VARCHAR(255),
    estado VARCHAR(30) DEFAULT 'Programada' CHECK (estado IN ('Programada', 'Completada', 'Cancelada', 'No asistió')),
    FOREIGN KEY (id_paciente) REFERENCES pacientes(id_paciente) ON DELETE CASCADE,
    FOREIGN KEY (id_medico) REFERENCES medicos(id_medico) ON DELETE RESTRICT
);

CREATE TABLE historial_clinico (
    id_historial INT AUTO_INCREMENT PRIMARY KEY,
    id_paciente INT NOT NULL,
    id_medico INT NOT NULL,
    id_cita INT NULL,
    fecha_registro DATETIME DEFAULT CURRENT_TIMESTAMP,
    diagnostico TEXT NOT NULL,
    tratamiento TEXT,
    observaciones TEXT,
    FOREIGN KEY (id_paciente) REFERENCES pacientes(id_paciente) ON DELETE CASCADE,
    FOREIGN KEY (id_medico) REFERENCES medicos(id_medico) ON DELETE RESTRICT,
    FOREIGN KEY (id_cita) REFERENCES citas(id_cita) ON DELETE SET NULL
);

CREATE TABLE pruebas (
    id_prueba INT AUTO_INCREMENT PRIMARY KEY,
    nombre_prueba VARCHAR(100) NOT NULL UNIQUE,
    descripcion TEXT,
    id_departamento INT NOT NULL,
    precio DECIMAL(10, 2) CHECK (precio >= 0),
    FOREIGN KEY (id_departamento) REFERENCES departamentos(id_departamento) ON DELETE RESTRICT
);

CREATE TABLE resultados_pruebas (
    id_resultado INT AUTO_INCREMENT PRIMARY KEY,
    id_prueba INT NOT NULL,
    id_paciente INT NOT NULL,
    id_medico_solicitante INT NOT NULL,
    fecha_realizacion DATETIME DEFAULT CURRENT_TIMESTAMP,
    resultado_detalles TEXT NOT NULL,
    valores_referencia TEXT,
    observaciones TEXT,
    FOREIGN KEY (id_prueba) REFERENCES pruebas(id_prueba) ON DELETE RESTRICT,
    FOREIGN KEY (id_paciente) REFERENCES pacientes(id_paciente) ON DELETE CASCADE,
    FOREIGN KEY (id_medico_solicitante) REFERENCES medicos(id_medico) ON DELETE RESTRICT
);

CREATE TABLE peticiones_procesadas (
    id_peticion INT AUTO_INCREMENT PRIMARY KEY,
    id_paciente INT NOT NULL,
    id_medico_solicitante INT NOT NULL,
    id_prueba INT NOT NULL,
    id_resultado INT NULL,
    fecha_solicitud DATETIME NOT NULL,
    fecha_procesamiento DATETIME NULL,
    estado_procesamiento VARCHAR(30) DEFAULT 'Pendiente' CHECK (estado_procesamiento IN ('Pendiente', 'En Proceso', 'Completada', 'Rechazada')),
    observaciones_procesamiento TEXT,
    FOREIGN KEY (id_paciente) REFERENCES pacientes(id_paciente) ON DELETE CASCADE,
    FOREIGN KEY (id_medico_solicitante) REFERENCES medicos(id_medico) ON DELETE RESTRICT,
    FOREIGN KEY (id_prueba) REFERENCES pruebas(id_prueba) ON DELETE RESTRICT,
    FOREIGN KEY (id_resultado) REFERENCES resultados_pruebas(id_resultado) ON DELETE SET NULL
);