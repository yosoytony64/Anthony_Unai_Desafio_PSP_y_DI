create database Hospital;

use Hospal;

create table Pacientes(
	Id_pacientes INT auto_increment primary KEY,
	nombre varchar(50) NOT NULL,
    DNI varchar(60) NULL,
    apellidos varchar(50) NOT NULL,
    fecha_nacimiento date NULL,
    tfn integer NULL,
    email varchar(60) NULL
);