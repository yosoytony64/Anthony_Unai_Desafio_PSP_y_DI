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
    id_cita INT NULL, -- solo si el registro viena de una cita agendada
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
    valores_referencia TEXT, 
    precio DECIMAL(10, 2) CHECK (precio >= 0),
    id_departamento INT NOT NULL,
    FOREIGN KEY (id_departamento) REFERENCES departamentos(id_departamento) ON DELETE RESTRICT
);

CREATE TABLE resultados_pruebas (
    id_resultado INT AUTO_INCREMENT PRIMARY KEY,
    id_paciente INT NOT NULL,
    id_prueba INT NOT NULL,
    id_medico_solicitante INT NULL, -- Opcional
    fecha_realizacion DATETIME DEFAULT CURRENT_TIMESTAMP,
    resultado_detalles TEXT NOT NULL,
    observaciones TEXT,
    FOREIGN KEY (id_paciente) REFERENCES pacientes(id_paciente) ON DELETE CASCADE,
    FOREIGN KEY (id_prueba) REFERENCES pruebas(id_prueba) ON DELETE RESTRICT,
    FOREIGN KEY (id_medico_solicitante) REFERENCES medicos(id_medico) ON DELETE SET NULL
);

USE Hospital;

ALTER TABLE pruebas ADD COLUMN codigo VARCHAR(10) NOT NULL UNIQUE AFTER id_prueba;

INSERT INTO departamentos (nombre, ubicacion, telefono) VALUES
('Cardiología','Planta 2, Ala Norte','911234567'),
('Laboratorio','Planta 0, Ala Sur','911234568'),
('Medicina General','Planta 1, Ala Este','911234569'),
('Endocrinología','Planta 3, Ala Norte','911234570');

INSERT INTO medicos (nombre, apellidos, especialidad, num_colegiado, telefono, email, id_departamento) VALUES
('Laura','Martínez Gil','Cardiología','MED-10234','612345678','laura.martinez@hospital.es', 1),
('Carlos','Ruiz Soto','Medicina General','MED-20456','623456789','carlos.ruiz@hospital.es',    3),
('Elena','Navarro Pérez','Endocrinología','MED-30567','634567890','elena.navarro@hospital.es',  4),
('Javier','Ortega Luna','Medicina General','MED-40678','645678901','javier.ortega@hospital.es',  3),
('Marta','Herrera Díaz','Análisis Clínicos','MED-50789','656789012','marta.herrera@hospital.es',  2),
('Pablo','Sánchez Vidal','Cardiología','MED-60890','667890123','pablo.sanchez@hospital.es',  1);

INSERT INTO pacientes (nombre, apellidos, dni, fecha_nacimiento, genero, telefono, email, direccion, grupo_sanguineo) VALUES
('Ana','García López','12345678Z','1991-05-14','Femenino','612345679','ana.garcia@mail.com','C/ Mayor 12, Madrid','A+'),
('Luis','Martínez Pérez','23456789D','1973-11-02','Masculino','645678901','luis.martinez@mail.com','C/ Alcalá 45, Madrid','O+'),
('María','Sánchez Ruiz','34567891H','1997-03-21','Femenino','623456789','maria.sanchez@mail.com','Av. de América 8, Madrid','B+'),
('David','Fernández Díaz','45678912S','1980-07-30','Masculino','678901234','david.fernandez@mail.com','C/ Atocha 23, Madrid','AB+'),
('Laura','Torres Gómez','56789123F','1988-12-09','Femenino','690123456','laura.torres@mail.com','C/ Serrano 77, Madrid','O-'),
('Javier','Ramírez Martín','67891234H','1965-02-17','Masculino','601234567','javier.ramirez@mail.com','C/ Goya 31, Madrid','A-'),
('Carmen','López Herrera','78912345N','1996-09-25','Femenino','670987654','carmen.lopez@mail.com','Paseo de la Castellana 100, Madrid','B-'),
('Sergio','Pérez Navarro','89123456C','1984-04-06','Masculino','649876543','sergio.perez@mail.com','C/ Toledo 5, Madrid','O+'),
('Paula','Díaz Moreno','91234567F','1992-08-18','Femenino','688765432','paula.diaz@mail.com','C/ Fuencarral 60, Madrid','A+'),
('Miguel','Ruiz Ortega','10293847J','1977-01-12','Masculino','677654321','miguel.ruiz@mail.com','C/ Bravo Murillo 14, Madrid','AB-');

INSERT INTO citas (id_paciente, id_medico, fecha_hora, motivo, estado) VALUES
(1,  1, '2026-01-15 10:30:00', 'Revisión general','Completada'),
(2,  2, '2026-01-16 09:00:00', 'Dolor de cabeza persistente','Completada'),
(3,  3, '2026-01-20 11:15:00', 'Control de tiroides','Cancelada'),
(4,  1, '2026-02-03 12:00:00', 'Palpitaciones','Completada'),
(5,  4, '2026-02-10 16:30:00', 'Chequeo anual','No asistió'),
(6,  5, '2026-02-12 08:45:00', 'Analítica de control','Completada'),
(7,  2, '2026-02-18 10:00:00', 'Revisión general','Programada'),
(8,  6, '2026-03-02 13:30:00', 'Control de hipertensión','Completada'),
(9,  3, '2026-03-05 17:00:00', 'Control de glucosa','Programada'),
(10, 4, '2026-03-09 09:30:00', 'Dolor lumbar','Programada'),
(1,  6, '2026-03-12 11:00:00', 'Seguimiento cardiológico','Programada'),
(2,  1, '2026-03-15 10:15:00', 'Electrocardiograma','Programada'),
(4,  5, '2026-03-18 12:30:00', 'Analítica de colesterol','Cancelada'),
(6,  2, '2026-03-20 15:00:00', 'Revisión de resultados','Programada');


INSERT INTO historial_clinico (id_paciente, id_medico, id_cita, fecha_registro, diagnostico, tratamiento, observaciones) VALUES
(1, 1, 1,'2026-01-15 11:00:00','Hipertensión leve','Dieta baja en sal y ejercicio moderado','Control en 3 meses'),
(2, 2, 2,'2026-01-16 09:30:00','Cefalea tensional','Paracetamol 1 g cada 8 h','Reducir estrés y mejorar descanso'),
(4, 1, 4,'2026-02-03 12:40:00','Extrasístoles benignas','Sin tratamiento farmacológico','Evitar cafeína; repetir ECG'),
(6, 5, 6,'2026-02-12 09:15:00','Hipercolesterolemia leve','Dieta y actividad física','Repetir analítica en 6 meses'),
(8, 6, 8,'2026-03-02 14:10:00','Hipertensión arterial','Enalapril 10 mg diarios','Revisar tensión en casa'),
(3, 3,NULL,'2025-11-20 10:00:00','Hipotiroidismo subclínico','Seguimiento sin medicación','Registro previo sin cita asociada'),
(9, 3,NULL,'2025-12-10 12:00:00','Prediabetes','Plan nutricional','Control de glucosa trimestral'),
(5, 4,NULL,'2025-10-05 16:00:00','Lumbalgia mecánica','Antiinflamatorios y fisioterapia',NULL);

INSERT INTO pruebas (codigo, nombre_prueba, descripcion, valores_referencia, precio, id_departamento) VALUES
('GLU','Glucosa','Glucosa en sangre en ayunas','70-100 mg/dL',8.50,2),
('COL','Colesterol','Colesterol total en sangre','< 200 mg/dL',9.75,2),
('TRI','Triglicéridos','Triglicéridos en sangre','< 150 mg/dL',9.75,2),
('HBA','Hemoglobina glicosilada','Control de glucemia a 3 meses','4.0-5.6 %',15.00,4),
('ECG','Electrocardiograma','Registro de la actividad eléctrica del corazón','Ritmo sinusal 60-100 lpm',30.00,1);

INSERT INTO resultados_pruebas (id_paciente, id_prueba, id_medico_solicitante, fecha_realizacion, resultado_detalles, observaciones) VALUES
(1, 1, 1,'2026-01-15 12:00:00','98 mg/dL - Dentro del intervalo',NULL),
(1, 2, 1,'2026-01-15 12:00:00','185 mg/dL - Dentro del intervalo',NULL),
(1, 3, 1,'2026-01-15 12:00:00','120 mg/dL - Dentro del intervalo',NULL),
(6, 2, 5,'2026-02-12 09:00:00','215 mg/dL - Por encima del intervalo','Repetir en 6 meses'),
(6, 3, 5,'2026-02-12 09:00:00','160 mg/dL - Por encima del intervalo',NULL),
(9, 1, 3,'2025-12-10 11:30:00','108 mg/dL - Por encima del intervalo','Compatible con prediabetes'),
(9, 4, 3,'2025-12-10 11:30:00','5.9 % - Por encima del intervalo',NULL),
(4, 5, 1,'2026-02-03 12:20:00','72 lpm - Dentro del intervalo','Extrasístoles aisladas'),
(8, 1,NULL,'2026-03-02 14:00:00','95 mg/dL - Dentro del intervalo','Sin médico solicitante registrado');